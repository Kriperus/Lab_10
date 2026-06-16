using System;

namespace PascalCompiler
{
    public enum TokenType
    {
        // Ключевые слова
        Program, Var, Const, Begin, End, If, Then, Else, While, Do, For, To, Downto, Repeat, Until,
        Function, Procedure, Array, Of, Record, Type, String, Integer, Real, Boolean, Char,
        With,
        // Идентификаторы
        Identifier,
        IntegerNumber, RealNumber,
        // Операторы и разделители
        Plus, Minus, Multiply, Divide, Assign, Equal, NotEqual, Less, LessEqual, Greater, GreaterEqual,
        Semicolon, Colon, Comma, Dot, DotDot, LeftParen, RightParen, LeftBracket, RightBracket, Quote,
        // Специальные
        EOF, Unknown
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
        public int LineNumber { get; set; }
        public int CharPosition { get; set; }

        public Token(TokenType type, string value, int lineNumber, int charPosition)
        {
            Type = type;
            Value = value;
            LineNumber = lineNumber;
            CharPosition = charPosition;
        }

        public override string ToString()
        {
            return $"[{Type}] '{Value}' at line {LineNumber}, pos {CharPosition}";
        }
    }
}
