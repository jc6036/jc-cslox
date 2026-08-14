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
}
