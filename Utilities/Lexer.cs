using System;
using System.ComponentModel;
using System.Numerics;
using X_Programming_Language.Constants;
using X_Programming_Language.Errors;

namespace X_Programming_Language.Utilities
{
    public class Lexer
    {
        public string Text { get; private set; }
        public Position Position { get; private set; }
        public char? CurrentChar { get; private set; }

        public Lexer(string fileName, string text)
        {
            Text = text;
            Position = new Position(-1, 0, -1, fileName, text);
            CurrentChar = null;
            Advance();
        }

        public void Advance()
        {
            Position.Advance(CurrentChar);
            CurrentChar = Position.Index < Text.Length ? Text[Position.Index] : null;
        }

        public Tuple<List<Token>, Error?> Tokenize()
        {
            List<Token> tokens = new();

            while (CurrentChar != null)
            {
                if (" \t".Contains((char)CurrentChar)) Advance();
                else if (";\n".Contains((char)CurrentChar))
                {
                    tokens.Add(new Token(TokenIdentifier.TT_NEWLINE, (char)CurrentChar, Position));
                    Advance();
                }
                else if (SyntaxIdentifier.DIGITS.Contains((char)CurrentChar))
                {
                    var callback = TokenizeNumber();
                    if (callback.Item2 != null) return new(new(), callback.Item2);
                    tokens.Add(callback.Item1!);
                }
                else if (SyntaxIdentifier.LETTERS.Contains((char)CurrentChar)) tokens.Add(TokenizeIdentifier());
                else if (SyntaxIdentifier.OPERATORS.Contains((char)CurrentChar))
                {
                    if (CurrentChar == '"')
                    {
                        var callback = TokenizeString();
                        if (callback.Item2 != null) return new(new(), callback.Item2);
                        tokens.Add(callback.Item1!);
                        continue;
                    }
                    else if (CurrentChar == '!')
                    {
                        tokens.Add(TokenizeNotEquals());
                        continue;
                    }
                    else if (CurrentChar == '=')
                    {
                        tokens.Add(TokenizeEqualsOrArrow());
                        continue;
                    }
                    else if (CurrentChar == '<')
                    {
                        tokens.Add(TokenizeLesserThan());
                        continue;
                    }
                    else if (CurrentChar == '>')
                    {
                        tokens.Add(TokenizeGreaterThan());
                        continue;
                    }
                    else if (CurrentChar == '/')
                    {
                        var callback = TokenizeDivide();
                        if (callback.Item2 != null) return new(new(), callback.Item2);
                        if (callback.Item1 != null) tokens.Add(callback.Item1);
                        continue;
                    }
                    tokens.Add(new Token(TokenIdentifier.TT_OP, CurrentChar, Position));
                    Advance();
                }
                else
                {
                    Position positionStart = Position.Copy();
                    char currentChar = (char)this.CurrentChar;
                    Advance();
                    return new(new(), new IllegalCharError(positionStart, Position, $"Illegal character '{currentChar}'"));
                }

            }
            tokens.Add(new Token(TokenIdentifier.TT_EOF, null, Position));
            return new(tokens, null);
        }

        public Tuple<Token?, Error?> TokenizeNumber()
        {
            string numString = "";
            int dotCount = 0;
            var positionStart = Position.Copy();

            while (CurrentChar != null && (SyntaxIdentifier.DIGITS + ".").Contains((char) CurrentChar))
            {
                if (CurrentChar == '.')
                {
                    if (dotCount == 1) break;
                    dotCount += 1;
                    numString += '.';
                }
                else numString += CurrentChar;
                Advance();
            }

            

            if (dotCount == 0)
            {
                if (BigInteger.Parse(numString) > BigInteger.Parse(int.MaxValue.ToString()))
                {
                    return new(null, new OverflowError(positionStart, Position, $"Integer literal '{numString}' cannot be held by int type"));
                }
                return new(new Token(TokenIdentifier.TT_INT, int.Parse(numString), positionStart, Position), null);
            }
                
            return new(new Token(TokenIdentifier.TT_FLOAT, float.Parse(numString), positionStart, Position), null);
        }

        public Token TokenizeIdentifier()
        {
            var idString = "";
            var positionStart = Position.Copy();

            while (CurrentChar != null && SyntaxIdentifier.PERMITTED_IDENTIFIER_OR_COND_OP.Contains((char)CurrentChar))
            {
                idString += CurrentChar;
                Advance();
            }

            var tokType = SyntaxIdentifier.KEYWORDS.Contains(idString) ? TokenIdentifier.TT_KEYWORD : SyntaxIdentifier.BOOLEAN_VALUES.Contains(idString) ? TokenIdentifier.TT_BOOL : TokenIdentifier.TT_ID;
            return new Token(tokType, idString, positionStart, Position);
        }

        public Tuple<Token?, Error?> TokenizeDivide()
        {
            var positionStart = Position.Copy();
            Advance();

            if (CurrentChar == '/')
            {
                SkipComment();
                return new(null, null);
            }
            else if (CurrentChar == '*')
            {
                Advance();

                while (CurrentChar != '*' && CurrentChar != null) Advance();
                if (CurrentChar == null)
                    return new(null, new InvalidSyntaxError(positionStart, Position, "Unexpected EOF while scanning multi-line comment, expected ending token '*/'"));
                Advance();
                if (CurrentChar == null)
                    return new(null, new InvalidSyntaxError(positionStart, Position, "Unexpected EOF while scanning multi-line comment, expected ending token '*/'"));
                Advance();
                return new(null, null);
            }

            return new(new Token(TokenIdentifier.TT_OP, '/', positionStart, Position), null);
        }

        public Token TokenizeNotEquals()
        {
            var positionStart = Position.Copy();
            Advance();

            if (CurrentChar == '=')
            {
                Advance();
                return new Token(TokenIdentifier.TT_COND, "!=", positionStart, Position);
            }

            return new Token(TokenIdentifier.TT_OP, '!', positionStart, Position);
        }

        public Token TokenizeEqualsOrArrow()
        {
            var positionStart = Position.Copy();
            Advance();

            if (CurrentChar == '=')
            {
                Advance();
                return new Token(TokenIdentifier.TT_COND, "==", positionStart, Position);
            }
            else if (CurrentChar == '>')
            {
                Advance();
                return new Token(TokenIdentifier.TT_SYNTAX_OP, "=>", positionStart, Position);
            }

            return new Token(TokenIdentifier.TT_OP, '=', positionStart, Position);
        }

        public Token TokenizeGreaterThan()
        {
            var positionStart = Position.Copy();
            Advance();

            if (CurrentChar == '=')
            {
                Advance();
                return new Token(TokenIdentifier.TT_COND, ">=", positionStart, Position);
            }

            return new Token(TokenIdentifier.TT_OP, '>', positionStart, Position);
        }

        public Token TokenizeLesserThan()
        {
            var positionStart = Position.Copy();
            Advance();

            if (CurrentChar == '=')
            {
                Advance();
                return new Token(TokenIdentifier.TT_COND, "<=", positionStart, Position);
            }

            return new Token(TokenIdentifier.TT_OP, '<', positionStart, Position);
        }

        public Tuple<Token?, Error?> TokenizeString()
        {
            var str = "";
            var positionStart = Position.Copy();
            var escapeCharacter = false;
            Advance();

            var escapeCharacters = new Dictionary<char, char>
            {
                { 'n', '\n' },
                { 't', '\t' },
                { 'r', '\r' },
                { 'v', '\v' },

            };
            var lastChar = CurrentChar;
            while (CurrentChar != null && (CurrentChar != '"' || escapeCharacter))
            {
                
                if (escapeCharacter) str += (escapeCharacters.ContainsKey((char)CurrentChar) ? escapeCharacters[(char)CurrentChar] : CurrentChar);
                else
                {
                    if (CurrentChar == '\\') escapeCharacter = true;
                    else str += CurrentChar;
                }
                Advance();
                escapeCharacter = false;
                lastChar = CurrentChar;
            }
            if (lastChar != '"') return new(null, new InvalidSyntaxError(Position!, new Position(Position.Index, Position.Line, Position.Column + 1, Position.FileName, Position.FileText), "Unexpected EOF while scanning string literal"));

            Advance();
            return new(new Token(TokenIdentifier.TT_STRING, str, positionStart, Position), null);
        }

        public void SkipComment()
        {
            Advance();

            while (CurrentChar != '\n' && CurrentChar != null) Advance();
            Advance();
        }
    }
}

