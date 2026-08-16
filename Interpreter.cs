using System.Data;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace my_jlox
{
    public class Interpreter : Operate<object>, Execute<object?>
    {
        public Environment globals;
        public Environment environment;
        private Dictionary<Expr, int> locals = new Dictionary<Expr, int>();

        public Interpreter()
        {
            this.globals = new Environment();
            this.environment = globals;

            // Book wants me to use an anon class here, but C# doesn't have that that I'm aware of
            // So I defined a global func call by hand
            globals.define(new Token(TokenType.IDENTIFIER, "clock", null, 0), new Clock());
        }

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

        public void resolve(Expr expr, int depth)
        {
            locals.Add(expr, depth);
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

            environment.define(stmt.name, value);
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

        public object? exFunction(Function stmt)
        {
            LoxFunction function = new LoxFunction(stmt, environment);

            environment.define(stmt.name, function);

            return null;
        }

        public object? exReturn(Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = evaluate(stmt.value);

            throw new ReturnException(value);
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
            return lookupVariable(expr.name, expr);
        }

        public object? opAssign(Assign expr)
        {
            object value = evaluate(expr.value);

            int? distance = null;
            if(locals.ContainsKey(expr))
            {
                distance = locals[expr];
            }

            if(distance != null)
            {
                environment.assignAt(distance!.Value, expr.name, value);
            }
            else
            {
                globals.assign(expr.name, value);
            }

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

        public object opCall(Call expr)
        {
            object callee = evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach(Expr argument in expr.arguments)
            {
                arguments.Add(evaluate(argument));
            }

            if(callee.GetType().GetInterface("LoxCallable") != typeof(LoxCallable)) // Was tricky to adapt this to C#, but good reminder java instanceof != C# typeof
            {
                throw new RuntimeError(expr.paren, "Can only call functions and classes.");
            }

            LoxCallable function = (LoxCallable)callee;

            if(arguments.Count != function.arity())
            {
                throw new RuntimeError(expr.paren, $"Expected {function.arity()} arguments but got {arguments.Count}.");
            }

            return function.call(this, arguments);
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

        private object? lookupVariable(Token name, Expr expr)
        {
            int? distance;
            if (locals.ContainsKey(expr))
                distance = locals[expr];
            else
                distance = null;

            if (distance != null)
                return environment.getAt(distance!.Value, name.lexeme);
            else
                return globals.get(name);
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

        public void executeBlock(List<Stmt> statements, Environment environment)
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
