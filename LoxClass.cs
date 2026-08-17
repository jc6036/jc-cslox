using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace my_jlox
{
    public class LoxClass : LoxCallable
    {
        public string name;
        private Dictionary<string, LoxFunction> methods;

        public LoxClass(string name, Dictionary<string, LoxFunction> methods)
        { 
            this.name = name;
            this.methods = methods;
        }

        public string toString()
        {
            return name;
        }

        public int arity()
        {
            LoxFunction? initializer = findMethod("init");
            if (initializer == null) return 0;
            return initializer.arity();
        }

        // Our instantiation treats the ClassName() like a factory that produces instances of itself, so no need for new
        // I like this decision from the book, seems like a decently elegant way to handle instantiation
        public object call(Interpreter interpreter, List<object> arguments)
        {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction? initializer = findMethod("init");
            if (initializer != null)
            {
                initializer.bind(instance).call(interpreter, arguments);
            }

            return instance;
        }

        public LoxFunction? findMethod(string name)
        {
            if(methods.ContainsKey(name))
            {
                return methods[name];
            }

            return null;
        }
    }
}
