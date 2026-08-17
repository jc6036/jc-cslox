using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace my_jlox
{
    // We tie each operation our expressions must be processed through, to a new class, that implements each visitor
    // That way we can add new expression types by adding a new visitor and expr class
    // And we can add new operations on expressions by adding a new operation class and implementing visitor
    public interface Operate<T>
    {
        public T? opBinary(Binary binary);
        public T opGrouping(Grouping grouping);
        public T? opUnary(Unary unary);
        public T opLiteral(Literal litearl);
        public T? opVariable(Variable var);
        public T? opAssign(Assign assign);
        public T opLogical(Logical logical);
        public T opCall(Call function);
        public T opGet(Get get);
        public T opSet(Set set);
        public T opThis(This thisExpr);
    }

    public abstract class Expr
    {
        public abstract T? pickForOp<T>(Operate<T> opPicker); // Main operation execution point
                                                              // oop trick automatically routes via extension type and visitor type to the correct operation code
    }

    public class Binary : Expr
    {
        public Expr left;
        public Token oprtr;
        public Expr right;

        public Binary(Expr left, Token oprtr, Expr right)
        {
            this.left = left;
            this.oprtr = oprtr;
            this.right = right;
        }

        public override T pickForOp<T>(Operate<T> opPicker)
        {
            return opPicker.opBinary(this);
        }
    }

    public class Grouping : Expr
    {
        public Expr expression;

        public Grouping(Expr expression)
        {
            this.expression = expression;
        }

        public override T pickForOp<T>(Operate<T> opPicker)
        {
            return opPicker.opGrouping(this);
        }
    }

    public class Literal : Expr
    {
        public object? value;

        public Literal(object? value)
        {
            this.value = value;
        }

        public override T pickForOp<T>(Operate<T> opPicker)
        {
            return opPicker.opLiteral(this);
        }
    }

    public class Unary : Expr
    {
        public Token oprtr;
        public Expr right;

        public Unary(Token oprtr, Expr right)
        {
            this.oprtr = oprtr;
            this.right = right;
        }

        public override T pickForOp<T>(Operate<T> opPicker)
        {
            return opPicker.opUnary(this);
        }
    }

    public class Variable : Expr
    {
        public Token name;

        public Variable(Token name)
        {
            this.name = name;
        }

        public override T pickForOp<T>(Operate<T> opPicker)
        {
            return opPicker.opVariable(this);
        }
    }

    public class Assign : Expr
    {
        public Token name;
        public Expr value;

        public Assign(Token name, Expr value)
        {
            this.name = name;
            this.value = value;
        }

        public override T pickForOp<T>(Operate<T> opPicker)
        {
            return opPicker.opAssign(this);
        }
    }

    public class Logical : Expr
    {
        public Expr left;
        public Token oprtr;
        public Expr right;

        public Logical(Token oprtr, Expr left, Expr right)
        {
            this.oprtr = oprtr;
            this.left = left;
            this.right = right;
        }

        public override T pickForOp<T>(Operate<T> opPicker)
        {
            return opPicker.opLogical(this);
        }
    }

    public class Call : Expr
    {
        public Expr callee;
        public Token paren;
        public List<Expr> arguments;

        public Call(Expr callee, Token paren, List<Expr> arguments)
        {
            this.callee = callee;
            this.paren = paren;
            this.arguments = arguments;
        }

        public override T pickForOp<T>(Operate<T> opPicker)
        {
            return opPicker.opCall(this);
        }
    }

    public class Get : Expr
    {
        public Expr obj;
        public Token name;

        public Get(Expr obj, Token name)
        {
            this.obj = obj;
            this.name = name;
        }

        public override T pickForOp<T>(Operate<T> opPicker)
        {
            return opPicker.opGet(this);
        }
    }

    public class Set : Expr
    {
        public Expr obj;
        public Token name;
        public Expr value;

        public Set(Expr obj, Token name, Expr value)
        {
            this.obj = obj;
            this.name = name;
            this.value = value;
        }

        public override T pickForOp<T>(Operate<T> opPicker)
        {
            return opPicker.opSet(this);
        }
    }

    public class This : Expr
    {
        public Token keyword;

        public This(Token keyword)
        {
            this.keyword = keyword;
        }

        public override T pickForOp<T>(Operate<T> opPicker)
        {
            return opPicker.opThis(this);
        }
    }
}
