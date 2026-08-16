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
        
        public LoxFunction(Function declaration)
        {
            this.declaration = declaration;
        }

        public int arity()
        {
            return declaration.paramlist.Count;
        }

        public object? call(Interpreter interpreter, List<object> arguments)
        {
            Environment environment = new Environment(interpreter.globals);
            for(int i = 0; i < declaration.paramlist.Count; i++)
            {
                environment.define(declaration.paramlist[i], arguments[i]);
            }

            interpreter.executeBlock(declaration.body, environment);
            return null;
        }

        public string toString()
        {
            return $"<fn {declaration.name.lexeme} >";
        }
    }
}
