using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace my_jlox
{
    // Will advance through the flat token list much in the same way the lexer advances through characters
    // adding each discovered expression to our expression tree by way of cascading precedence grammar filter functions
    public class Parser
    {
        private List<Token> tokens;
        private int current = 0;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<Stmt> parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while(!isAtEnd())
            {
                statements.Add(declaration());
            }

            return statements;
        }

        private Stmt statement()
        {
            if (match(TokenType.PRINT)) return printStatement();

            return expressionStatement();
        }

        private Stmt declaration()
        {
            try
            {
                if (match(TokenType.VAR)) return varDeclaration();

                return statement();
            }
            catch (ParseException e)
            {
                synchronize();
                return null;
            }
        }

        private Stmt varDeclaration()
        {
            Token name = consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr? initializer = null;
            if(match(TokenType.EQUAL))
            {
                initializer = expression();
            }

            consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Var(name, initializer);
        }

        private Stmt printStatement()
        {
            Expr value = expression();
            consume(TokenType.SEMICOLON, "Expect ';' after value.");
            return new Print(value);
        }

        private Stmt expressionStatement()
        {
            Expr expr = expression();

            consume(TokenType.SEMICOLON, "Expet ';' after expression.");
            return new ExpressionStmt(expr);
        }

        #region recursive descent expression parsing
        private Expr expression()
        {
            return equality();
        }

        private Expr equality()
        {
            Expr expr = comparation();

            while (match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token oprtr = previous();
                Expr right = comparation();
                expr = new Binary(expr, oprtr, right);
            }

            return expr;
        }

        private Expr comparation()
        {
            Expr expr = term();

            while (match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token oprtr = previous();
                Expr right = term();
                expr = new Binary(expr, oprtr, right);
            }

            return expr;
        }

        private Expr term()
        {
            Expr expr = factor();

            while(match(TokenType.MINUS, TokenType.PLUS))
            {
                Token oprtr = previous();
                Expr right = factor();
                expr = new Binary(expr, oprtr, right);
            }

            return expr;
        }

        private Expr factor()
        {
            Expr expr = unary();

            while(match(TokenType.SLASH, TokenType.STAR))
            {
                Token oprtr = previous();
                Expr right = unary();
                expr = new Binary(expr, oprtr, right);
            }

            return expr;
        }

        private Expr unary()
        {
            if(match(TokenType.BANG, TokenType.MINUS))
            {
                Token oprtr = previous();
                Expr right = unary();
                return new Unary(oprtr, right);
            }

            return primary();
        }

        private Expr primary()
        {
            if (match(TokenType.FALSE)) return new Literal(false);
            if (match(TokenType.TRUE)) return new Literal(true);
            if (match(TokenType.NIL)) return new Literal(null);
            
            if(match(TokenType.NUMBER, TokenType.STRING))
            {
                return new Literal(previous().literal);
            }

            if(match(TokenType.IDENTIFIER))
            {
                return new Variable(previous());
            }

            if(match(TokenType.LEFT_PAREN))
            {
                Expr expr = expression();
                consume(TokenType.RIGHT_PAREN, "Expected ')' after expression.");
                return new Grouping(expr);
            }

            throw error(peek(), "Expected expression.");
        }
        #endregion

        // Errors
        private Token consume(TokenType type, string message)
        {
            if (check(type)) return advance();

            throw error(peek(), message);
        }

        private ParseException error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParseException();
        }

        private void synchronize()
        {
            advance();

            while(!isAtEnd())
            {
                if (previous().type == TokenType.SEMICOLON) return;

                switch(peek().type)
                {
                    case TokenType.CLASS: return;
                    case TokenType.FUN: return;
                    case TokenType.VAR: return;
                    case TokenType.FOR: return;
                    case TokenType.IF: return;
                    case TokenType.WHILE: return;
                    case TokenType.PRINT: return;
                    case TokenType.RETURN: return;
                    default: break;
                }

                advance();
            }
        }

        // Helpers
        private bool match(params TokenType[] types)
        {
            foreach(TokenType type in types)
            {
                if(check(type))
                {
                    advance();
                    return true;
                }
            }

            return false;
        }

        private bool check(TokenType type)
        {
            if (isAtEnd()) return false;
            return peek().type == type;
        }

        private Token advance()
        {
            if (!isAtEnd()) current++;
            return previous();
        }

        private bool isAtEnd()
        {
            return peek().type == TokenType.EOF;
        }

        private Token peek()
        {
            return tokens[current];
        }

        private Token previous()
        {
            return tokens[current - 1];
        }
    }

    public class ParseException : Exception
    {

    }
}
