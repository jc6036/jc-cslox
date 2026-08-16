using System.Text;

namespace my_jlox
{
    internal static class Lox
    {
        private static Interpreter interpreter = new Interpreter();
        private static bool hadError = false;
        private static bool hadRuntimeError = false;

        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: jlox [script]");
                System.Environment.Exit(64);
            }

            if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }

            System.Environment.Exit(0);
        }

        // Primary instruction executor
        private static void Run(string source)
        {
            Scanner scanner = new(source);
            List<Token> tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens);
            List<Stmt> statements = parser.parse();

            if (hadError || statements == null) return;

            Resolver resolver = new Resolver(interpreter);
            resolver.resolve(statements);

            if (hadError) return;

            interpreter.interpret(statements);
        }

        // Wrapper for Run
        private static void RunFile(string path)
        {
            string source = File.ReadAllText(path, Encoding.Default);

            Run(source);

            if (hadError) System.Environment.Exit(65);
            if (hadRuntimeError) System.Environment.Exit(70);
        }

        // Wrapper for Run
        private static void RunPrompt()
        {
            while (true)
            {
                Console.Write("jc-cslox> ");
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

        public static void runtimeError(RuntimeError error)
        {
            Console.Error.WriteLine($"Runtime Error: {error.Message}\n[line {error.token.line}]");
            hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[Line {line} ] Error{where}: {message}"); // Very basic, possible upgrade, make rust-like error reporting

            hadError = true;
        }
    }
}