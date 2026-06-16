using System;
using System.Collections.Generic;

namespace PascalCompiler
{
    public class ErrorHandler
    {
        private static readonly Dictionary<int, string> _errorMessages = new Dictionary<int, string>
        {
            { 100, "использование имени не соответствует описанию" },
            { 101, "число выходит за пределы допустимого диапазона" },
            { 102, "неизвестный символ" },
            { 200, "ожидается 'program'" },
            { 201, "ожидается имя программы" },
            { 202, "ожидается ';' после имени программы" },
            { 203, "ожидается 'var'" },
            { 204, "ожидается ':' после идентификаторов" },
            { 205, "ожидается ';' после объявления" },
            { 206, "ожидается тип" },
            { 207, "ожидается 'end' для record" },
            { 208, "ожидается '[' после array" },
            { 209, "ожидается нижняя граница массива" },
            { 210, "ожидается '..' для диапазона" },
            { 211, "ожидается верхняя граница массива" },
            { 212, "ожидается ']' после границ массива" },
            { 213, "ожидается 'of' после границ массива" },
            { 214, "ожидается 'begin'" },
            { 215, "ожидается 'end' для составного оператора" },
            { 216, "неизвестный оператор" },
            { 217, "ожидается имя поля" },
            { 218, "ожидается имя записи" },
            { 219, "ожидается 'do' после with" },
            { 220, "ожидается ')'" },
            { 221, "ожидается выражение" },
            { 222, "ожидается '.' после 'end'" },
            { 223, "ожидается 'end'" },
            { 224, "объявление не поддерживается" },
            { 300, "переменная не объявлена" },
            { 301, "запись не объявлена" },
            { 302, "поле не существует в записи" },
            { 303, "тип не соответствует ожидаемому" },
            { 400, "деление на ноль" },
            { 401, "выход за границы массива" },
            { 0, "неизвестная ошибка" }
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

        public void ReportError(int errorCode, int charPosition)
        {
            _errorCount++;
            int totalOffset = charPosition;
            string arrow = new string(' ', totalOffset) + "^";
            Console.WriteLine($"**{_errorCount:D2}** {arrow} ошибка код {errorCode}");
            if (_errorMessages.ContainsKey(errorCode))
                Console.WriteLine($"******  {_errorMessages[errorCode]}");
            else
                Console.WriteLine($"******  {_errorMessages[0]}");
        }
    }
}
