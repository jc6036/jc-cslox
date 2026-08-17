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
}
