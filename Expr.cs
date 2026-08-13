using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace my_jlox
{
    public abstract class Expr
    {
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
    }

    public class Grouping : Expr
    {
        public Expr expression;

        public Grouping(Expr expression)
        {
            this.expression = expression;
        }
    }

    public class Literal : Expr
    {
        public object value;

        public Literal(object value)
        {
            this.value = value;
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
    }
}
