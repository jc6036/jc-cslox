using System.Xml.Schema;

namespace my_jlox
{
    public class Resolver : Execute<object?>, Operate<object?>
    {
        private Interpreter interpreter;
        private Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;

        public Resolver(Interpreter interpreter)
        {
            this.interpreter = interpreter;
        }

        public object? exBlock(Block stmt)
        {
            beginScope();
            resolve(stmt.statements);
            endScope();
            return null;
        }

        public object? exVar(Var stmt)
        {
            declare(stmt.name);

            if (stmt.initializer != null)
            {
                resolve(stmt.initializer);
            }

            define(stmt.name);
            return null;
        }

        public object? opVariable(Variable expr)
        {
            if(scopes.Count != 0 && scopes.Peek().ContainsKey(expr.name.lexeme))
            {
                if (scopes.Peek()[expr.name.lexeme] == false)
                {
                    Lox.Error(expr.name, "Can't read local variable in its own initializer.");
                }
            }

            resolveLocal(expr, expr.name);
            return null;
        }

        public object? opAssign(Assign expr)
        {
            resolve(expr.value);
            resolveLocal(expr, expr.name);
            return null;
        }

        public object? exFunction(Function stmt)
        {
            declare(stmt.name);
            define(stmt.name);

            resolveFunction(stmt, FunctionType.FUNCTION);

            return null;
        }

        public object? exExpressionStmt(ExpressionStmt stmt)
        {
            resolve(stmt.expression);

            return null;
        }

        public object? exIf(If stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) resolve(stmt.elseBranch);
            return null;
        }

        public object? exPrint(Print stmt)
        {
            resolve(stmt.expression);
            return null;
        }

        public object? exReturn(Return stmt)
        {
            if(currentFunction == FunctionType.NONE)
            {
                Lox.Error(stmt.keyword, "Can't return from top level code.");
            }

            if(stmt.value != null)
            {
                if (currentFunction == FunctionType.INITIALIZER)
                    Lox.Error(stmt.keyword, "Can't return a value from an initializer.");

                resolve(stmt.value);
            }

            return null;
        }

        public object? exWhile(While stmt)
        {
            resolve(stmt.condition);
            resolve(stmt.body);
            return null;
        }

        public object? exClass(Class stmt)
        {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;

            declare(stmt.name);
            define(stmt.name);

            beginScope();
            scopes.Peek().Add("this", true);

            foreach(Function method in stmt.methods)
            {
                FunctionType declaration = FunctionType.METHOD;
                if (method.name.lexeme == "init") declaration = FunctionType.INITIALIZER;
                resolveFunction(method, declaration);
            }

            endScope();

            currentClass = enclosingClass;
            return null;
        }

        public object? opBinary(Binary expr)
        {
            resolve(expr.left);
            resolve(expr.right);
            return null;
        }

        public object? opCall(Call expr)
        {
            resolve(expr.callee);

            foreach(Expr argument in expr.arguments)
            {
                resolve(argument);
            }

            return null;
        }

        public object? opGrouping(Grouping expr)
        {
            resolve(expr.expression);
            return null;
        }

        public object? opLiteral(Literal expr)
        {
            return null;
        }

        public object? opLogical(Logical expr)
        {
            resolve(expr.left);
            resolve(expr.right);

            return null;
        }

        public object? opUnary(Unary expr)
        {
            resolve(expr.right);
            return null;
        }

        public object? opGet(Get expr)
        {
            resolve(expr.obj);
            return null;
        }

        public object? opSet(Set expr)
        {
            resolve(expr.value);
            resolve(expr.obj);
            return null;
        }

        public object? opThis(This expr)
        {
            if (currentClass == ClassType.NONE)
            {
                Lox.Error(expr.keyword, "Can't use 'this' outside of a class.");
                return null;
            }

            resolveLocal(expr, expr.keyword);
            return null;
        }

        public void resolve(List<Stmt> statements)
        {
            foreach(Stmt stmt in statements)
            {
                resolve(stmt);
            }
        }

        private void resolve(Stmt stmt)
        {
            stmt.pickForExecute(this);
        }

        private void resolve(Expr expr)
        {
            expr.pickForOp(this);
        }

        private void resolveLocal(Expr expr, Token name)
        {
            var i = scopes.Count;
            var reverseScopes = scopes.Reverse();                           // Matching some goofiness in the text
            foreach(var scope in reverseScopes)                             // Apparently java allows indexed access to stacks? LOL
            {                                                               // So I had to recreate what the book is doing
                if(scope.ContainsKey(name.lexeme))
                {
                    interpreter.resolve(expr, scopes.Count - i);
                }
                i--;
            }
        }

        private void resolveFunction(Function function, FunctionType type)
        {
            FunctionType enclosingFunction = currentFunction;
            currentFunction = type;

            beginScope();
            foreach(Token param in function.paramlist)
            {
                declare(param);
                define(param);
            }
            resolve(function.body);
            endScope();
            currentFunction = enclosingFunction;
        }

        private void beginScope()
        {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void endScope()
        {
            scopes.Pop();
        }

        private void declare(Token name)
        {
            if (scopes.Count == 0) return;

            Dictionary<string, bool> scope = scopes.Peek();

            if(scope.ContainsKey(name.lexeme))
            {
                Lox.Error(name, "Already a variable with this name in this scope.");
            }

            scope.Add(name.lexeme, false);
        }

        private void define(Token name)
        {
            if (scopes.Count == 0) return;

            if (scopes.Peek().ContainsKey(name.lexeme))
            {
                scopes.Peek()[name.lexeme] = true;
            }
            else
            {
                scopes.Peek().Add(name.lexeme, true);
            }
        }

        private enum FunctionType
        {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
        }

        private enum ClassType
        {
            NONE,
            CLASS
        }
    }
}
