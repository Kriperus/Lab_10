using System;
using System.Collections.Generic;

namespace PascalCompiler
{
    public class ErrorHandler
    {
        private static readonly Dictionary<int, string> _errorMessages = new Dictionary<int, string>
        {
            { 100, "использование имени не соответствует описанию" },
            { 147, "тип метки не совпадает с типом выбирающего выражения" },
            { 0,   "неизвестная ошибка" }
        };

        private int _errorCount = 0;

        public int TotalErrors => _errorCount;

        public void ReportError(int errorCode, int charPosition)
        {
            _errorCount++;

            int totalOffset = charPosition;
            string arrow = new string(' ', totalOffset) + "^";

            Console.WriteLine($"**{_errorCount:D2}** {arrow} ошибка код {errorCode}");

            if (_errorMessages.ContainsKey(errorCode))
            {
                Console.WriteLine($"******  {_errorMessages[errorCode]}");
            }
            else
            {
                Console.WriteLine($"******  {_errorMessages[0]}");
            }
        }
    }
}