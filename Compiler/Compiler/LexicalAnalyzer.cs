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
        public SourceReader Reader => _reader;

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
                if (token != null)
                    _tokens.Add(token);
            }
            _tokens.Add(new Token(TokenType.EOF, "EOF", _reader.LineNumber, _reader.CharIndex));
        }

        public void SaveTokenCodesToFile(string outputPath)
        {
            using (StreamWriter writer = new StreamWriter(outputPath))
            {
                foreach (Token token in _tokens)
                    writer.WriteLine($"{(int)token.Type} {token.Value}");
            }
        }

        private Token GetNextToken()
        {
            while (!_reader.IsEndOfFile)
            {
                char c = _reader.CurrentChar;
                if (char.IsWhiteSpace(c) || c == '\r' || c == '\n' || c == '\0')
                {
                    _reader.NextChar();
                    continue;
                }
                break;
            }

            if (_reader.IsEndOfFile) return null;

            int lineNum = _reader.LineNumber;
            int charPos = _reader.CharIndex;
            char ch = _reader.CurrentChar;

            if (ch == '{')
            {
                SkipBraceComment();
                return GetNextToken();
            }

            if (ch == '/')
            {
                _reader.NextChar();
                if (_reader.CurrentChar == '/')
                {
                    while (!_reader.IsEndOfFile && _reader.CurrentChar != '\n')
                        _reader.NextChar();
                    return GetNextToken();
                }
                return new Token(TokenType.Divide, "/", lineNum, charPos);
            }

            if (ch == '.')
            {
                _reader.NextChar();
                if (_reader.CurrentChar == '.')
                {
                    _reader.NextChar();
                    return new Token(TokenType.DotDot, "..", lineNum, charPos);
                }

                if (char.IsDigit(_reader.CurrentChar))
                {
                    StringBuilder sb = new StringBuilder("0.");
                    while (char.IsDigit(_reader.CurrentChar))
                    {
                        sb.Append(_reader.CurrentChar);
                        _reader.NextChar();
                    }
                    string value = sb.ToString();
                    if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double realValue))
                    {
                        return new Token(TokenType.RealNumber, value, lineNum, charPos);
                    }
                }
                return new Token(TokenType.Dot, ".", lineNum, charPos);
            }

            if (char.IsDigit(ch))
            {
                return ReadNumber(lineNum, charPos);
            }

            if (char.IsLetter(ch) || ch == '_')
                return ReadIdentifierOrKeyword(lineNum, charPos);

            switch (ch)
            {
                case '+': _reader.NextChar(); return new Token(TokenType.Plus, "+", lineNum, charPos);
                case '-': _reader.NextChar(); return new Token(TokenType.Minus, "-", lineNum, charPos);
                case '*': _reader.NextChar(); return new Token(TokenType.Multiply, "*", lineNum, charPos);
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
                case ',': _reader.NextChar(); return new Token(TokenType.Comma, ",", lineNum, charPos);
                case '(': _reader.NextChar(); return new Token(TokenType.LeftParen, "(", lineNum, charPos);
                case ')': _reader.NextChar(); return new Token(TokenType.RightParen, ")", lineNum, charPos);
                case '[': _reader.NextChar(); return new Token(TokenType.LeftBracket, "[", lineNum, charPos);
                case ']': _reader.NextChar(); return new Token(TokenType.RightBracket, "]", lineNum, charPos);
                case '\'': return ReadStringOrChar(lineNum, charPos);
                case '<':
                    _reader.NextChar();
                    if (_reader.CurrentChar == '>') { _reader.NextChar(); return new Token(TokenType.NotEqual, "<>", lineNum, charPos); }
                    if (_reader.CurrentChar == '=') { _reader.NextChar(); return new Token(TokenType.LessEqual, "<=", lineNum, charPos); }
                    return new Token(TokenType.Less, "<", lineNum, charPos);
                case '>':
                    _reader.NextChar();
                    if (_reader.CurrentChar == '=') { _reader.NextChar(); return new Token(TokenType.GreaterEqual, ">=", lineNum, charPos); }
                    return new Token(TokenType.Greater, ">", lineNum, charPos);
                default:
                    _errors.ReportError(102, "неизвестный символ", lineNum, charPos);
                    _reader.NextChar();
                    return new Token(TokenType.Unknown, ch.ToString(), lineNum, charPos);
            }
        }

        private void SkipBraceComment()
        {
            _reader.NextChar();
            while (!_reader.IsEndOfFile && _reader.CurrentChar != '}') _reader.NextChar();
            if (_reader.CurrentChar == '}') _reader.NextChar();
        }

        private Token ReadNumber(int lineNum, int charPos)
        {
            StringBuilder sb = new StringBuilder();
            int startPos = charPos;

            while (char.IsDigit(_reader.CurrentChar))
            {
                sb.Append(_reader.CurrentChar);
                _reader.NextChar();
            }

            var state = _reader.SaveState();

            if (_reader.CurrentChar == '.')
            {
                _reader.NextChar();

                if (char.IsDigit(_reader.CurrentChar))
                {
                    sb.Append('.');
                    while (char.IsDigit(_reader.CurrentChar))
                    {
                        sb.Append(_reader.CurrentChar);
                        _reader.NextChar();
                    }
                    string value = sb.ToString();
                    if (double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double realValue))
                    {
                        return new Token(TokenType.RealNumber, value, lineNum, startPos);
                    }
                    else
                    {
                        _errors.ReportError(0, "неверный формат вещественного числа", lineNum, startPos);
                        return new Token(TokenType.Unknown, value, lineNum, startPos);
                    }
                }
                else
                {
                    _reader.RestoreState(state);
                }
            }

            string intValueStr = sb.ToString();
            if (int.TryParse(intValueStr, out int intValue))
            {
                if (intValue < MinInteger || intValue > MaxInteger)
                    _errors.ReportError(101, "число выходит за пределы допустимого диапазона", lineNum, startPos);
                return new Token(TokenType.IntegerNumber, intValueStr, lineNum, startPos);
            }
            else
            {
                _errors.ReportError(0, "неверный формат целого числа", lineNum, startPos);
                return new Token(TokenType.Unknown, intValueStr, lineNum, startPos);
            }
        }

        private Token ReadIdentifierOrKeyword(int lineNum, int charPos)
        {
            StringBuilder sb = new StringBuilder();
            int startPos = charPos;
            while (char.IsLetterOrDigit(_reader.CurrentChar) || _reader.CurrentChar == '_')
            { sb.Append(_reader.CurrentChar); _reader.NextChar(); }
            string value = sb.ToString();
            string lower = value.ToLower();

            if (lower == "integer") return new Token(TokenType.Integer, value, lineNum, startPos);
            if (lower == "real") return new Token(TokenType.Real, value, lineNum, startPos);
            if (lower == "boolean") return new Token(TokenType.Boolean, value, lineNum, startPos);
            if (lower == "char") return new Token(TokenType.Char, value, lineNum, startPos);
            if (lower == "string") return new Token(TokenType.String, value, lineNum, startPos);
            if (lower == "type") return new Token(TokenType.Type, value, lineNum, startPos);
            if (lower == "record") return new Token(TokenType.Record, value, lineNum, startPos);
            if (lower == "array") return new Token(TokenType.Array, value, lineNum, startPos);
            if (lower == "of") return new Token(TokenType.Of, value, lineNum, startPos);

            if (SymbolTable.IsKeyword(value)) return new Token(SymbolTable.GetKeywordType(value), value, lineNum, startPos);
            return new Token(TokenType.Identifier, value, lineNum, startPos);
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
                    if (_reader.CurrentChar == '\'') { sb.Append('\''); _reader.NextChar(); }
                    else { closed = true; break; }
                }
                else { sb.Append(_reader.CurrentChar); _reader.NextChar(); }
            }
            if (!closed) _errors.ReportError(0, "незакрытая строка", lineNum, startPos);
            return new Token(TokenType.Quote, sb.ToString(), lineNum, startPos);
        }
    }

    public static class ReaderExtensions
    {
        public static char PeekChar(this SourceReader reader)
        {
            return reader.CurrentChar;
        }
    }
}
