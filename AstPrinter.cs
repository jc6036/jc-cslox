using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace my_jlox
{
    public class AstPrinter : Visitor<string>
    {
        public string print(Expr expr)
        {
            return expr.accept<string>(this);
        }

        public string visitBinary(Binary expr)
        {
            return parenthesize(expr.oprtr.lexeme, expr.left, expr.right);
        }

        public string visitUnary(Unary expr)
        {
            return parenthesize(expr.oprtr.lexeme, expr.right);
        }

        public string visitGrouping(Grouping expr)
        {
            return parenthesize("group", expr.expression);
        }

        public string visitLiteral(Literal expr)
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
                builder.Append(e.accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}
