using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace my_jlox
{
    public class LoxInstance
    {
        private LoxClass klass;
        private Dictionary<string, object> fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            this.klass = klass;
        }

        public string toString()
        {
            return $"{klass.name} instance";
        }

        public object get(Token name)
        {
            if(fields.ContainsKey(name.lexeme))
            {
                return fields[name.lexeme];
            }

            LoxFunction? method = klass.findMethod(name.lexeme);
            if (method != null) return method.bind(this);

            throw new RuntimeError(name, $"Undefined property {name.lexeme} .");
        }

        public void set(Token name, object value)
        {
            if (fields.ContainsKey(name.lexeme))
            {
                fields[name.lexeme] = value;
            }
            else
            {
                fields.Add(name.lexeme, value);
            }
        }
    }
}
