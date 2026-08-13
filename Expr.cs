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
    public interface Visitor<T>
    {
        public T visitBinary(Binary binary);
        public T visitGrouping(Grouping grouping);
        public T visitUnary(Unary unary);
        public T visitLiteral(Literal litearl);
    }

    public abstract class Expr
    {
        public abstract T accept<T>(Visitor<T> visitor); // Main operation execution point
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

        public override T accept<T>(Visitor<T> visitor)
        {
            return visitor.visitBinary(this);
        }
    }

    public class Grouping : Expr
    {
        public Expr expression;

        public Grouping(Expr expression)
        {
            this.expression = expression;
        }

        public override T accept<T>(Visitor<T> visitor)
        {
            return visitor.visitGrouping(this);
        }
    }

    public class Literal : Expr
    {
        public object value;

        public Literal(object value)
        {
            this.value = value;
        }

        public override T accept<T>(Visitor<T> visitor)
        {
            return visitor.visitLiteral(this);
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

        public override T accept<T>(Visitor<T> visitor)
        {
            return visitor.visitUnary(this);
        }
    }
}
