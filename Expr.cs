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
    public interface Visitor
    {
        public void visitBinary(Binary binary);
        public void visitGrouping(Grouping grouping);
        public void visitUnary(Unary unary);
        public void visitLiteral(Literal litearl);
    }

    public abstract class Expr
    {
        public abstract void accept(Visitor visitor); // Main operation execution point
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

        public override void accept(Visitor visitor)
        {
            visitor.visitBinary(this);
        }
    }

    public class Grouping : Expr
    {
        public Expr expression;

        public Grouping(Expr expression)
        {
            this.expression = expression;
        }

        public override void accept(Visitor visitor)
        {
            visitor.visitGrouping(this);
        }
    }

    public class Literal : Expr
    {
        public object value;

        public Literal(object value)
        {
            this.value = value;
        }

        public override void accept(Visitor visitor)
        {
            visitor.visitLiteral(this);
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

        public override void accept(Visitor visitor)
        {
            visitor.visitUnary(this);
        }
    }
}
