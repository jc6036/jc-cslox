using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace my_jlox
{
    public class Interpreter : Operate<object>, Execute<object?>
    {
        private Environment environment = new Environment();

        public void interpret(List<Stmt> statements)
        {
            try
            {
                foreach(Stmt statement in statements)
                {
                    execute(statement);
                }
            }
            catch (RuntimeError ex)
            {
                Lox.runtimeError(ex);
            }
        }

        private object evaluate(Expr expr)
        {
            return expr.pickForOp(this);
        }

        private void execute(Stmt stmt)
        {
            stmt.pickForExecute(this);
        }

        #region exMethods
        public object? exExpressionStmt(ExpressionStmt stmt)
        {
            evaluate(stmt.expression);
            return null;
        }

        public object? exIf(If stmt)
        {
            if(isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                execute(stmt.elseBranch);
            }

            return null;
        }

        public object? exPrint(Print stmt)
        {
            object? val = evaluate(stmt.expression);
            Console.WriteLine(stringify(val));
            return null;
        }

        public object? exVar(Var stmt)
        {
            object? value = null;
            if(stmt.initializer != null)
            {
                value = evaluate(stmt.initializer);
            }

            environment.define(stmt.name.lexeme, value);
            return null;
        }

        public object? exBlock(Block stmt)
        {
            executeBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public object? exWhile(While stmt)
        {
            while(isTruthy(evaluate(stmt.condition)))
            {
                execute(stmt.body);
            }

            return null;
        }
        #endregion

        #region opMethods
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
                    checkNumberOperand(expr.oprtr, right);
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
                    checkNumberOperands(expr.oprtr, left, right);
                    return (float)left > (float)right;
                case TokenType.GREATER_EQUAL:
                    checkNumberOperands(expr.oprtr, left, right);
                    return (float)left >= (float)right;
                case TokenType.LESS:
                    checkNumberOperands(expr.oprtr, left, right);
                    return (float)left < (float)right;
                case TokenType.LESS_EQUAL:
                    checkNumberOperands(expr.oprtr, left, right);
                    return (float)left <= (float)right;

                case TokenType.BANG_EQUAL:
                    return !isEqual(left, right); // Note we can check equality on any type
                case TokenType.EQUAL_EQUAL:
                    return isEqual(left, right);

                case TokenType.MINUS:
                    checkNumberOperands(expr.oprtr, left, right);
                    return (float)left - (float)right;
                case TokenType.SLASH:
                    checkNumberOperands(expr.oprtr, left, right);
                    return (float)left / (float)right;
                case TokenType.STAR:
                    checkNumberOperands(expr.oprtr, left, right);
                    return (float)left * (float)right;
                case TokenType.PLUS:
                    if(left.GetType() == typeof(float) && right.GetType() == typeof(float))
                        return (float)left + (float)right;

                    if (left.GetType() == typeof(string) && right.GetType() == typeof(string))
                        return (string)left + (string)right;

                    throw new RuntimeError(expr.oprtr, "Operands must be two numbers or two strings.");
            }

            return null;
        }

        public object? opVariable(Variable expr)
        {
            return environment.get(expr.name);
        }

        public object? opAssign(Assign expr)
        {
            object value = evaluate(expr.value);
            environment.assign(expr.name, value);
            return value;
        }

        public object opLogical(Logical expr)
        {
            object left = evaluate(expr.left);

            if(expr.oprtr.type == TokenType.OR)
            {
                if (isTruthy(left))
                    return left;
            }
            else
            {
                if (!isTruthy(left)) 
                    return left;
            }

            return evaluate(expr.right);
        }
        #endregion

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

        private void checkNumberOperand(Token oprtr, object operand)
        {
            if (operand.GetType() == typeof(float)) return;
            // otherwise
            throw new RuntimeError(oprtr, "Operand must be a number.");
        }

        private void checkNumberOperands(Token oprtr, object left, object right)
        {
            if (left.GetType() == typeof(float) && right.GetType() == typeof(float)) return;
            // otherwise
            throw new RuntimeError(oprtr, "Operands must be numbers.");
        }

        private string stringify(object val)
        {
            if (val == null) return "nil";

            if(val.GetType() == typeof(float))
            {
                string text = $"{val}";

                if (text.EndsWith(".0")) // I'm not sure why this is cut off, but again I'm going with the book
                {
                    text = text.Substring(0, text.Length - 2);
                }

                return text;
            }

            return $"{val}";
        }

        private void executeBlock(List<Stmt> statements, Environment environment)
        {
            Environment previous = this.environment;
            try
            {
                this.environment = environment;

                foreach (Stmt statement in statements)
                {
                    execute(statement);
                }
            }
            finally
            {
                this.environment = previous;
            }
        }
    }
}
