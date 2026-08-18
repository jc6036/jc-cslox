namespace my_jlox
{
    public class Environment
    {
        public Environment? enclosing;
        private Dictionary<string, object?> values = new Dictionary<string, object?>();

        public Environment()
        {
            this.enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            this.enclosing = enclosing;
        }

        public object? get(Token name)
        {
            if(values.ContainsKey(name.lexeme))
            {
                return values[name.lexeme];
            }

            if(enclosing != null)
            {
                return enclosing.get(name);
            }

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public object? getAt(int distance, string name)
        {
            if (ancestor(distance).values.ContainsKey(name))
                return ancestor(distance).values[name];
            else
                return null;
        }

        public void assignAt(int distance, Token name, object value)
        {
            if(ancestor(distance).values.ContainsKey(name.lexeme))
            {
                ancestor(distance).values[name.lexeme] = value;
            }
            else
            {
                ancestor(distance).values.Add(name.lexeme, value);
            }
        }

        private Environment ancestor(int distance)
        {
            Environment environment = this;
            for(int i = 0; i <= distance; i++)
            {
                environment = environment.enclosing;
            }

            return environment; // Won't be null, getAt only ever called after checking we have multiple envs. Null ref is fine, will let us debug for now
        }

        public void define(Token name, object? value)
        {
            if(values.ContainsKey(name.lexeme))
            {
                throw new RuntimeError(name, $"Attempted declaration of existing object '{name.lexeme}'.");
            }

            values.Add(name.lexeme, value);
        }

        public void assign(Token name, object? value)
        {
            if(values.ContainsKey(name.lexeme))
            {
                values[name.lexeme] = value;
                return;
            }

            if(enclosing != null)
            {
                enclosing.assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable: '{name.lexeme}'.");
        }
    }
}
