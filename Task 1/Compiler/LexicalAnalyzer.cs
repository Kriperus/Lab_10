using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PascalCompiler
{
    public class LexicalAnalyzer
    {
        private readonly SourceReader _reader;
        private readonly ErrorHandler _errors;
        private readonly List<Token> _tokens;

        private const int MinInteger = -32768;
        private const int MaxInteger = 32767;

        public IReadOnlyList<Token> Tokens => _tokens;

        public LexicalAnalyzer(SourceReader reader, ErrorHandler errors)
        {
            _reader = reader;
            _errors = errors;
            _tokens = new List<Token>();
        }

        public void Analyze()
        {
            _tokens.Clear();

            while (!_reader.IsEndOfFile)
            {
                Token token = GetNextToken();
                if (token != null && token.Type != TokenType.Unknown)
                {
                    _tokens.Add(token);
                }
            }

            _tokens.Add(new Token(TokenType.EOF, "EOF", _reader.LineNumber, _reader.CharIndex));
        }

        public void SaveTokenCodesToFile(string outputPath)
        {
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                foreach (Token token in _tokens)
                {
                    int tokenCode = (int)token.Type;
                    writer.WriteLine($"{tokenCode} {token.Value}");
                }
            }
        }

        private Token GetNextToken()
        {
            // Пропускаем пробелы и управляющие символы
            while (!_reader.IsEndOfFile && (char.IsWhiteSpace(_reader.CurrentChar) || _reader.CurrentChar == '\r' || _reader.CurrentChar == '\n'))
            {
                _reader.NextChar();
            }

            if (_reader.IsEndOfFile) return null;

            int lineNum = _reader.LineNumber;
            int charPos = _reader.CharIndex;
            char ch = _reader.CurrentChar;

            if (char.IsDigit(ch))
            {
                return ReadNumber(lineNum, charPos);
            }

            if (char.IsLetter(ch) || ch == '_')
            {
                return ReadIdentifierOrKeyword(lineNum, charPos);
            }

            // Обработка операторов и разделителей
            switch (ch)
            {
                case '+': _reader.NextChar(); return new Token(TokenType.Plus, "+", lineNum, charPos);
                case '-': _reader.NextChar(); return new Token(TokenType.Minus, "-", lineNum, charPos);
                case '*': _reader.NextChar(); return new Token(TokenType.Multiply, "*", lineNum, charPos);
                case '/': _reader.NextChar(); return new Token(TokenType.Divide, "/", lineNum, charPos);
                case '=': _reader.NextChar(); return new Token(TokenType.Equal, "=", lineNum, charPos);
                case ';': _reader.NextChar(); return new Token(TokenType.Semicolon, ";", lineNum, charPos);
                case ':':
                    _reader.NextChar();
                    if (_reader.CurrentChar == '=')
                    {
                        _reader.NextChar();
                        return new Token(TokenType.Assign, ":=", lineNum, charPos);
                    }
                    return new Token(TokenType.Colon, ":", lineNum, charPos);
                case ',':
                    _reader.NextChar();
                    return new Token(TokenType.Comma, ",", lineNum, charPos);
                case '.':
                    _reader.NextChar();
                    if (_reader.CurrentChar == '.')
                    {
                        _reader.NextChar();
                        return new Token(TokenType.DotDot, "..", lineNum, charPos);
                    }
                    return new Token(TokenType.Dot, ".", lineNum, charPos);
                case '(':
                    _reader.NextChar();
                    return new Token(TokenType.LeftParen, "(", lineNum, charPos);
                case ')':
                    _reader.NextChar();
                    return new Token(TokenType.RightParen, ")", lineNum, charPos);
                case '[':
                    _reader.NextChar();
                    return new Token(TokenType.LeftBracket, "[", lineNum, charPos);
                case ']':
                    _reader.NextChar();
                    return new Token(TokenType.RightBracket, "]", lineNum, charPos);
                case '\'':
                    return ReadStringOrChar(lineNum, charPos);
                case '<':
                    _reader.NextChar();
                    if (_reader.CurrentChar == '>')
                    {
                        _reader.NextChar();
                        return new Token(TokenType.NotEqual, "<>", lineNum, charPos);
                    }
                    if (_reader.CurrentChar == '=')
                    {
                        _reader.NextChar();
                        return new Token(TokenType.LessEqual, "<=", lineNum, charPos);
                    }
                    return new Token(TokenType.Less, "<", lineNum, charPos);
                case '>':
                    _reader.NextChar();
                    if (_reader.CurrentChar == '=')
                    {
                        _reader.NextChar();
                        return new Token(TokenType.GreaterEqual, ">=", lineNum, charPos);
                    }
                    return new Token(TokenType.Greater, ">", lineNum, charPos);
                default:
                    _reader.NextChar();
                    return new Token(TokenType.Unknown, ch.ToString(), lineNum, charPos);
            }
        }

        private Token ReadNumber(int lineNum, int charPos)
        {
            StringBuilder sb = new StringBuilder();
            bool isReal = false;
            int startPos = charPos;

            while (char.IsDigit(_reader.CurrentChar) || _reader.CurrentChar == '.')
            {
                if (_reader.CurrentChar == '.')
                {
                    _reader.NextChar();
                    if (_reader.CurrentChar == '.')
                    {
                        _reader.NextChar();
                        break;
                    }
                    isReal = true;
                    sb.Append('.');
                    continue;
                }
                sb.Append(_reader.CurrentChar);
                _reader.NextChar();
            }

            string value = sb.ToString();

            if (isReal)
            {
                return new Token(TokenType.RealNumber, value, lineNum, startPos);
            }
            else
            {
                if (int.TryParse(value, out int intValue))
                {
                    if (intValue < MinInteger || intValue > MaxInteger)
                    {
                        _errors.ReportError(200, _reader.CurrentLineText, lineNum, startPos);
                    }
                    return new Token(TokenType.IntegerNumber, value, lineNum, startPos);
                }
                else
                {
                    _errors.ReportError(0, _reader.CurrentLineText, lineNum, startPos);
                    return new Token(TokenType.Unknown, value, lineNum, startPos);
                }
            }
        }

        private Token ReadIdentifierOrKeyword(int lineNum, int charPos)
        {
            StringBuilder sb = new StringBuilder();
            int startPos = charPos;

            while (char.IsLetterOrDigit(_reader.CurrentChar) || _reader.CurrentChar == '_')
            {
                sb.Append(_reader.CurrentChar);
                _reader.NextChar();
            }

            string value = sb.ToString();

            if (SymbolTable.IsKeyword(value))
            {
                return new Token(SymbolTable.GetKeywordType(value), value, lineNum, startPos);
            }
            else
            {
                return new Token(TokenType.Identifier, value, lineNum, startPos);
            }
        }

        private Token ReadStringOrChar(int lineNum, int charPos)
        {
            _reader.NextChar();
            StringBuilder sb = new StringBuilder();
            bool closed = false;
            int startPos = charPos;

            while (!_reader.IsEndOfFile)
            {
                if (_reader.CurrentChar == '\'')
                {
                    _reader.NextChar();
                    if (_reader.CurrentChar == '\'')
                    {
                        sb.Append('\'');
                        _reader.NextChar();
                    }
                    else
                    {
                        closed = true;
                        break;
                    }
                }
                else
                {
                    sb.Append(_reader.CurrentChar);
                    _reader.NextChar();
                }
            }

            if (!closed)
            {
                _errors.ReportError(0, _reader.CurrentLineText, lineNum, startPos);
            }

            return new Token(TokenType.Quote, sb.ToString(), lineNum, startPos);
        }
    }
}
