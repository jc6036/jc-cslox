namespace my_jlox
{
    internal class Scanner
    {
        private string source;
        private List<Token> tokens = new List<Token>();
#pragma warning disable CS8618
        private static Dictionary<string, TokenType> keywords;
#pragma warning restore CS8618

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string source)
        {
            this.source = source;
            keywords = new Dictionary<string, TokenType>();
            keywords = GenKeywordCollection();
        }

        public List<Token> ScanTokens()
        {
            while (!isAtEnd())
            {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private void ScanToken()
        {
            char c = Advance();

            switch(c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;

                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL: TokenType.GREATER); break;

                case '/':
                    if(Match('/'))
                        while (Peek() != '\n' && !isAtEnd()) Advance();
                    else
                        AddToken(TokenType.SLASH);
                    break;

                case ' ': break;
                case '\r': break;
                case '\t': break;

                case '\n':
                    line++;
                    break;

                case '"': String(); break;

                default:
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Lox.Error(line, "Unexpected Character");
                    }
                    break;
            }
        }

        private Dictionary<string, TokenType> GenKeywordCollection()
        {
            var set = new Dictionary<string, TokenType>();
            set.Add("and", TokenType.AND);
            set.Add("class", TokenType.CLASS);
            set.Add("else", TokenType.ELSE);
            set.Add("false", TokenType.FALSE);
            set.Add("for", TokenType.FOR);
            set.Add("fun", TokenType.FUN);
            set.Add("if", TokenType.IF);
            set.Add("nil", TokenType.NIL);
            set.Add("or", TokenType.OR);
            set.Add("print", TokenType.PRINT);
            set.Add("return", TokenType.RETURN);
            set.Add("super", TokenType.SUPER);
            set.Add("this", TokenType.THIS);
            set.Add("true", TokenType.TRUE);
            set.Add("var", TokenType.VAR);
            set.Add("while", TokenType.WHILE);
            return set;
        }
        
        // Consume-forward - look forward with mutation
        private bool Match(char expected)
        {
            if (isAtEnd()) return false;
            if (source[current] != expected) return false;

            current++;
            return true;
        }

        // Look forward with no mutations
        private char Peek()
        {
            if (isAtEnd()) return '\0';
            return source[current];
        }

        // Look forward + 1 with no mutations
        private char PeekNext()
        {
            if(current + 1 >= source.Length) return '\0';

            return source[current + 1];
        }

        private void String()
        { 
            while(Peek() != '"' && !isAtEnd())
            {
                if (Peek() == '\n') line++;
                Advance();
            }

            if(isAtEnd())
            {
                Lox.Error(line, "Unterminated string.");
                return;
            }

            Advance();
            string value = source.Substring(start + 1, (current - 1) - (start + 1));
            AddToken(TokenType.STRING, value);
        }

        private void Number()
        {
            while (IsDigit(Peek())) Advance();

            if(Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();

                while (IsDigit(Peek())) Advance();
            }
            
            float.TryParse(source.Substring(start, current - start), out var ret);
            AddToken(TokenType.NUMBER, ret);
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = source.Substring(start, current - start);

            TokenType? type;
            try
            {
                type = keywords[text];
            }
            catch (Exception)
            {
                type = null;
            }

            AddToken(type ?? TokenType.IDENTIFIER);
        }
        
        private char Advance()
        {
            return source[current++];
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object? literal)
        {
            string text = source.Substring(start, current - start);
            tokens.Add(new Token(type, text, literal, line));
        }

        private bool isAtEnd()
        {
            return current >= source.Length;
        }

        private bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || c == '_';
        }

        private bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }
    }
}
