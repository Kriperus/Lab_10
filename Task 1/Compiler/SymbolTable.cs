using System.Collections.Generic;

namespace PascalCompiler
{
    public static class SymbolTable
    {
        // Таблица ключевых слов Pascal
        private static readonly Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
        {
            { "program", TokenType.Program },
            { "var", TokenType.Var },
            { "const", TokenType.Const },
            { "begin", TokenType.Begin },
            { "end", TokenType.End },
            { "if", TokenType.If },
            { "then", TokenType.Then },
            { "else", TokenType.Else },
            { "while", TokenType.While },
            { "do", TokenType.Do },
            { "for", TokenType.For },
            { "to", TokenType.To },
            { "downto", TokenType.Downto },
            { "repeat", TokenType.Repeat },
            { "until", TokenType.Until },
            { "function", TokenType.Function },
            { "procedure", TokenType.Procedure },
            { "array", TokenType.Array },
            { "of", TokenType.Of },
            { "record", TokenType.Record },
            { "type", TokenType.Type },
            { "string", TokenType.String },
            { "integer", TokenType.Integer },
            { "real", TokenType.Real },
            { "boolean", TokenType.Boolean },
            { "char", TokenType.Char }
        };

        public static bool IsKeyword(string word)
        {
            return _keywords.ContainsKey(word.ToLower());
        }

        public static TokenType GetKeywordType(string word)
        {
            return _keywords.TryGetValue(word.ToLower(), out TokenType type) ? type : TokenType.Identifier;
        }
    }
}
