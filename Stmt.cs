using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace my_jlox
{
    public interface Execute<T>
    {
        public T exExpressionStmt(ExpressionStmt expression);
        public T exPrint(Print print);
        public T exVar(Var var);
        public T exBlock(Block block);
        public T exIf(If ifStmt);
        public T exWhile(While whileStmt);
    }

    public abstract class Stmt
    {
        public abstract T pickForExecute<T>(Execute<T> exPicker);
    }

    public class ExpressionStmt : Stmt
    {
        public Expr expression;

        public ExpressionStmt(Expr expression)
        {
            this.expression = expression;
        }

        public override T pickForExecute<T>(Execute<T> exPicker)
        {
            return exPicker.exExpressionStmt(this);
        }
    }

    public class Print : Stmt
    {
        public Expr expression;
        public Print(Expr expression)
        {
            this.expression = expression;
        }

        public override T pickForExecute<T>(Execute<T> exPicker)
        {
            return exPicker.exPrint(this);
        }
    }

    public class Var : Stmt
    {
        public Token name;
        public Expr? initializer;

        public Var(Token name, Expr? initializer)
        {
            this.name = name;
            this.initializer = initializer;
        }

        public override T pickForExecute<T>(Execute<T> exPicker)
        {
            return exPicker.exVar(this);
        }
    }

    public class Block : Stmt
    {
        public List<Stmt> statements;

        public Block(List<Stmt> statements)
        {
            this.statements = statements;
        }

        public override T pickForExecute<T>(Execute<T> exPicker)
        {
            return exPicker.exBlock(this);
        }
    }

    public class If : Stmt
    {
        public Expr condition;
        public Stmt thenBranch;
        public Stmt? elseBranch;

        public If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
        {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
        }

        public override T pickForExecute<T>(Execute<T> exPicker)
        {
            return exPicker.exIf(this);
        }
    }

    public class While : Stmt
    {
        public Expr condition;
        public Stmt body;

        public While(Expr condition, Stmt body)
        {
            this.condition = condition;
            this.body = body;
        }

        public override T pickForExecute<T>(Execute<T> exPicker)
        {
            return exPicker.exWhile(this);
        }
    }
}
