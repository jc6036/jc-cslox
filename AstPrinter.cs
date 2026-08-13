using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace my_jlox
{
    public class AstPrinter : Operate<string>
    {
        public string print(Expr expr)
        {
            return expr.pickForOp<string>(this); // this = expr type. pick THIS expr type op
        }

        public string opBinary(Binary expr) // Binary calls this on selection
        {
            return parenthesize(expr.oprtr.lexeme, expr.left, expr.right);
        }

        public string opUnary(Unary expr) // Same for unary
        {
            return parenthesize(expr.oprtr.lexeme, expr.right);
        }

        public string opGrouping(Grouping expr) // Grouping
        {
            return parenthesize("group", expr.expression);
        }

        public string opLiteral(Literal expr) // Literal, and so on
        {
            return expr.value.ToString() ?? "nil";
        }

        private string parenthesize(string name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr e in exprs)
            {
                builder.Append(" ");
                builder.Append(e.pickForOp(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}