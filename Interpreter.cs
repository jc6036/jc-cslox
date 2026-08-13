using System.Reflection.Metadata;

namespace my_jlox
{
    public class Interpreter : Operate<object>
    {
        public object interpret(Expr expr)
        {
            return expr.pickForOp(this);
        }

        public object opLiteral(Literal expr)
        {
            return expr.value ?? "nil";
        }

        public object opGrouping(Grouping expr)
        {
            return evaluate(expr.expression);
        }

        public object? opUnary(Unary expr)
        {
            object right = evaluate(expr.right);

            switch(expr.oprtr.type)
            {
                case TokenType.MINUS:
                    return -(float)right;
                case TokenType.BANG:
                    return !isTruthy(right);
            }

            return null;
        }

        public object? opBinary(Binary expr)
        {
            object left = evaluate(expr.left);
            object right = evaluate(expr.right);

            switch(expr.oprtr.type)
            {
                case TokenType.GREATER:
                    return (float)left > (float)right;
                case TokenType.GREATER_EQUAL:
                    return (float)left >= (float)right;
                case TokenType.LESS:
                    return (float)left < (float)right;
                case TokenType.LESS_EQUAL:
                    return (float)left <= (float)right;

                case TokenType.BANG_EQUAL:
                    return !isEqual(left, right); // Note we can check equality on any type
                case TokenType.EQUAL_EQUAL:
                    return isEqual(left, right);

                case TokenType.MINUS:
                    return (float)left - (float)right;
                case TokenType.SLASH:
                    return (float)left / (float)right;
                case TokenType.STAR:
                    return (float)left * (float)right;
                case TokenType.PLUS:
                    if(left.GetType() == typeof(float) && right.GetType() == typeof(float))
                        return (float)left + (float)right;

                    if (left.GetType() == typeof(string) && right.GetType() == typeof(string))
                        return (string)left + (string)right;

                    break;
            }

            return null;
        }

        private object evaluate(Expr expr)
        {
            return expr.pickForOp(this);
        }

        private bool isTruthy(object? val)
        {
            // 0 doesn't come back false which is crazy to me, but the book designs Lox this way...may revisit later
            if (val == null) return false;
            if (val.GetType() == typeof(bool)) return (bool)val;
            return true;
        }

        private bool isEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null) return false;

            return a.Equals(b); // Primarily really leaning on C#'s equality checker here
        }
    }
}
