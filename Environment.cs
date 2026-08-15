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

        public void define(Token name, object? value)
        {
            if(values.ContainsKey(name.lexeme))
            {
                throw new RuntimeError(name, $"Attempted declaration of existing variable '{name.lexeme}'.");
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
