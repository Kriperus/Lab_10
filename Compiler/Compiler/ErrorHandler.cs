using System.Collections.Generic;

namespace PascalCompiler
{
    public class ErrorHandler
    {
        private readonly InputOutput _io;
        private int _errorCount = 0;

        private static readonly Dictionary<int, string> _errorMessages = new Dictionary<int, string>
        {
            { 100, "использование имени не соответствует описанию" },
            { 101, "число выходит за пределы допустимого диапазона" },
            { 102, "неизвестный символ" },
            { 103, "ожидается идентификатор" },
            { 104, "ожидается ':'" },
            { 105, "ожидается ';'" },
            { 106, "ожидается ':='" },
            { 107, "ожидается '..'" },
            { 108, "ожидается 'of'" },
            { 109, "ожидается '['" },
            { 110, "ожидается ']'" },
            { 111, "ожидается '.'" },
            { 112, "ожидается 'end'" },
            { 113, "ожидается 'do'" },
            { 114, "ожидается 'begin'" },
            { 147, "несоответствие типов" },
            { 148, "переменная не объявлена" },
            { 149, "поле записи не существует" },
            { 200, "ожидается 'program'" },
            { 201, "ожидается имя программы" },
            { 202, "ожидается ';'" },
            { 203, "ожидается 'var'" },
            { 204, "ожидается тип" }
        };

        public ErrorHandler(InputOutput io)
        {
            _io = io;
        }

        public int TotalErrors => _errorCount;

        public void ReportError(int errorCode, string additionalInfo, int lineNumber, int charPosition)
        {
            _errorCount++;

            string message = "";
            if (_errorMessages.ContainsKey(errorCode))
                message = _errorMessages[errorCode];
            else
                message = "неизвестная ошибка";

            if (!string.IsNullOrEmpty(additionalInfo))
                message += $" ({additionalInfo})";

            _io.PrintError(_errorCount, errorCode, message, lineNumber, charPosition);
        }

        public void PrintSummary()
        {
            _io.PrintSummary(_errorCount);
        }
    }
}
