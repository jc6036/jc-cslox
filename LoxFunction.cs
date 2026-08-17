using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace my_jlox
{
    public class LoxFunction : LoxCallable
    {
        private Function declaration;
        private Environment closure;
        private bool isInitializer;

        public LoxFunction(Function declaration, Environment closure, bool isInitializer)
        {
            this.isInitializer = isInitializer;
            this.declaration = declaration;
            this.closure = closure;
        }

        public int arity()
        {
            return declaration.paramlist.Count;
        }

        public object? call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(closure);
            for (int i = 0; i < declaration.paramlist.Count; i++)
            {
                environment.define(declaration.paramlist[i], arguments[i]);
            }

            try
            {
                interpreter.executeBlock(declaration.body, environment);
            }
            catch (ReturnException e) // Using exceptions to pass return values and unwind the stack is insane
            {
                if (isInitializer) return closure.getAt(0, "this");

                return e.value;
            }

            if (isInitializer) return closure.getAt(0, "this");

            return null;
        }

        public string toString()
        {
            return $"<fn {declaration.name.lexeme} >";
        }

        public LoxFunction bind(LoxInstance instance)
        {
            Environment environment = new Environment(closure);
            Token name = new Token(TokenType.IDENTIFIER, "this", null, 0);
            environment.define(name, instance);
            return new LoxFunction(declaration, environment, isInitializer);
        }
    }
}
