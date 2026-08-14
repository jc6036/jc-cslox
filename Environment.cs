namespace my_jlox
{
    public class Environment
    {
        private Dictionary<string, object?> values = new Dictionary<string, object?>();

        public object? get(Token name)
        {
            if(values.ContainsKey(name.lexeme))
            {
                return values[name.lexeme];
            }

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public void define(string name, object? value)
        {
            values.Add(name, value);
        }
    }
}
