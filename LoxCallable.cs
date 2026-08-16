using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace my_jlox
{
    public interface LoxCallable
    {
        public int arity();
        public object? call(Interpreter interpreter, List<object> arguments);
    }

    public class Clock : LoxCallable
    {
        public int arity() { return 0; }

        public object? call(Interpreter interpreter, List<object> arguments)
        {
            return (double)DateTime.Now.Millisecond / 1000.0;
        }

        public string toString() { return "<native fn>"; }
    }

    public class LoxFunction : LoxCallable
    {
        private Function declaration;
        private Environment closure;
        
        public LoxFunction(Function declaration, Environment closure)
        {
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
            for(int i = 0; i < declaration.paramlist.Count; i++)
            {
                environment.define(declaration.paramlist[i], arguments[i]);
            }

            try
            {
                interpreter.executeBlock(declaration.body, environment);
            }
            catch (ReturnException e) // Using exceptions to pass return values and unwind the stack is insane
            {
                return e.value;
            }

            return null;
        }

        public string toString()
        {
            return $"<fn {declaration.name.lexeme} >";
        }
    }
}
