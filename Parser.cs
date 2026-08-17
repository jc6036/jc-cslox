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
            if (match(TokenType.FOR)) return forStatement();
            if (match(TokenType.IF)) return ifStatement();
            if (match(TokenType.PRINT)) return printStatement();
            if (match(TokenType.RETURN)) return returnStatement();
            if (match(TokenType.WHILE)) return whileStatement();
            if (match(TokenType.LEFT_BRACE)) return new Block(block());

            return expressionStatement();
        }

        private List<Stmt> block()
        {
            List<Stmt> statements = new List<Stmt>();

            while(!check(TokenType.RIGHT_BRACE) && !isAtEnd())
            {                
                statements.Add(declaration());
            }

            consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Stmt? declaration() // null checks all over the damn place, hard to work out with the given patterns
        {
            try
            {
                if (match(TokenType.CLASS)) return classDeclaration();
                if (match(TokenType.FUN)) return function("function");
                if (match(TokenType.VAR)) return varDeclaration();

                return statement();
            }
            catch (ParseException)
            {
                synchronize();
                return null;
            }
        }

        private Function function(string kind)
        {
            Token name = consume(TokenType.IDENTIFIER, $"Expect {kind} name.");
            consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");

            List<Token> parameters = new List<Token>();
            if(!check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if(parameters.Count >= 255)
                    {
                        error(peek(), "Can't have more than 255 params.");
                    }

                    parameters.Add(consume(TokenType.IDENTIFIER, "Expect param name."));
                } while (match(TokenType.COMMA));
            }
            consume(TokenType.RIGHT_PAREN, "Expect ')' after params.");

            consume(TokenType.LEFT_BRACE, $"Expect '{"{"}' before {kind} body.");
            List<Stmt> body = block();
            return new Function(name, parameters, body);
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

        private Stmt classDeclaration()
        {
            Token name = consume(TokenType.IDENTIFIER, "Expect class name.");
            consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");

            List<Function> methods = new List<Function>();
            while(!check(TokenType.RIGHT_BRACE) && !isAtEnd())
            {
                methods.Add(function("method"));
            }

            consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");

            return new Class(name, methods);
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

            consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new ExpressionStmt(expr);
        }

        private Stmt ifStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = statement();
            Stmt? elseBranch = null;
            if (match(TokenType.ELSE)) elseBranch = statement();

            return new If(condition, thenBranch, elseBranch);
        }

        private Stmt whileStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after 'while'.");
            Stmt body = statement();

            return new While(condition, body);
        }

        private Stmt forStatement()
        {
            consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt? initializer;
            if(match(TokenType.SEMICOLON))
            {
                initializer = null;
            }
            else if (match(TokenType.VAR))
            {
                initializer = varDeclaration();
            }
            else
            {
                initializer = expressionStatement();
            }

            Expr? condition = null;
            if(!check(TokenType.SEMICOLON))
            {
                condition = expression();
            }
            consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if(!check(TokenType.RIGHT_PAREN))
            {
                increment = expression();
            }
            consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            Stmt body = statement();

            if(increment != null)
            {
                body = new Block(new List<Stmt> {body, new ExpressionStmt(increment)});
            }

            if (condition == null) condition = new Literal(true);
            body = new While(condition, body);

            if(initializer != null)
            {
                body = new Block(new List<Stmt> {initializer, body});
            }

            return body;
        }

        private Stmt returnStatement()
        {
            Token keyword = previous();
            Expr value = null;
            if(!check(TokenType.SEMICOLON))
            {
                value = expression();
            }

            consume(TokenType.SEMICOLON, "Expected ';' after return value.");
            return new Return(keyword, value);
        }

        #region recursive descent expression parsing
        private Expr expression()
        {
            return assignment();
        }

        private Expr assignment()
        {
            Expr expr = or();

            if (match(TokenType.EQUAL))
            {
                Token equals = previous();
                Expr value = assignment();

                if (expr.GetType() == typeof(Variable))
                {
                    Token name = ((Variable)expr).name;
                    return new Assign(name, value);
                }
                else if (expr.GetType() == typeof(Get))
                {
                    Get get = (Get)expr;
                    return new Set(get.obj, get.name, value);
                }

                    error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr or()
        {
            Expr expr = and();

            while(match(TokenType.OR))
            {
                Token oprtr = previous();
                Expr right = and();
                expr = new Logical(oprtr, expr, right);
            }

            return expr;
        }

        private Expr and()
        {
            Expr expr = equality();

            while(match(TokenType.AND))
            {
                Token oprtr = previous();
                Expr right = equality();
                expr = new Logical(oprtr, expr, right);
            }

            return expr;
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

            return call();
        }

        private Expr call()
        {
            Expr expr = primary();

            while (true)
            {
                if (match(TokenType.LEFT_PAREN))
                {
                    expr = finishCall(expr);
                }
                else if (match(TokenType.DOT))
                {
                    Token name = consume(TokenType.IDENTIFIER, "Expect prop name after '.'.");
                    expr = new Get(expr, name);
                }
                else
                    break;
            }

            return expr;
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

            if (match(TokenType.THIS)) return new This(previous());

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

        private Expr finishCall(Expr callee)
        {
            List<Expr> arguments = new List<Expr>();

            if(!check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if(arguments.Count >= 255)
                    {
                        error(peek(), "Can't have more than 255 args."); // Bit arbitrary, based on java arg limit from text
                    }

                    arguments.Add(expression());
                } while (match(TokenType.COMMA));
            }

            Token paren = consume(TokenType.RIGHT_PAREN, "Expect ')' after args.");

            return new Call(callee, paren, arguments);
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
