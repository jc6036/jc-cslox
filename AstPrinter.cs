using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace my_jlox
{
    // Implementation of "Visitor" pattern
    // Operations on expressions all inherit from the Operate interface
    // This enforces each operation to have an implementation for each expression type
    // The operation class is called via an operation method, here it is Print
    // It then calls the expression abstract method to pick a type for the operation
    // Whichever sub type the call is routed to, has its own call back to the Operate implementer where it
    // calls its own flavor of the operation
    // So, operation -> visit/pick the correct expr type through type system -> Type -> Call back to given Operate implementer type to Do Thing For My Expr Type
    //
    // New Operation behaviors will be implemented via new Operate implementers which implement a behavior for each expr sub type
    // with minimal need to edit the expr subtypes themselves
    // New expr types are harder to add, as the base Operate interface and all implementers will need updated with new behavior
    // once the expr subtype is added, but that's the tradeoff
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
            if (expr.value != null)
                return $"{expr.value}";
            else
                return "nil";
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