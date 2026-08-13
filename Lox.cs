using System.Text;

namespace my_jlox
{
    internal static class Lox
    {
        private static bool hadError = false;

        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
                Environment.Exit(64);
            }

            if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }

            Environment.Exit(0);
        }

        // Primary instruction executor
        private static void Run(string source)
        {
            Scanner scanner = new(source);
            List<Token> tokens = scanner.ScanTokens();

            // For now, just print the tokens.
            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }
        }

        // Wrapper for Run
        private static void RunFile(string path)
        {
            string source = File.ReadAllText(path, Encoding.Default);

            Run(source);

            if (hadError) Environment.Exit(65);
        }

        // Wrapper for Run
        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("> ");
                string? line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                {
                    break;
                }

                Run(line);

                hadError = false;
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if (token.type == TokenType.EOF)
            {
                Report(token.line, " at end", message);
            }
            else
            {
                Report(token.line, $" at '{token.lexeme}'", message);
            }
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[Line {line} ] Error{where}: {message}"); // Very basic, possible upgrade, make rust-like error reporting

            hadError = true;
        }
    }
}