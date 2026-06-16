using System;
using System.Collections.Generic;

namespace PascalCompiler
{
    public class ErrorHandler
    {
        private static readonly Dictionary<int, string> _errorMessages = new Dictionary<int, string>
        {
            { 200, "число выходит за пределы допустимого диапазона" },
            { 0,   "неизвестная ошибка" }
        };

        private int _errorCount = 0;

        public int TotalErrors => _errorCount;

        public void ReportError(int errorCode, string sourceLine, int lineNumber, int charPosition)
        {
            _errorCount++;

            Console.WriteLine($"**{_errorCount:D2}** Ошибка код {errorCode}");
            Console.WriteLine(sourceLine);

            string arrow = new string(' ', charPosition) + "^";
            Console.WriteLine(arrow);

            if (_errorMessages.ContainsKey(errorCode))
                Console.WriteLine($"    {_errorMessages[errorCode]}");
            else
                Console.WriteLine($"    {_errorMessages[0]}");

            Console.WriteLine();
        }
    }
}
