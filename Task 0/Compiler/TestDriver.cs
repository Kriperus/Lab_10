using System;
using System.IO;

namespace PascalCompiler
{
    public class TestDriver
    {
        private readonly SourceReader _reader;
        private readonly ErrorHandler _errors;

        public TestDriver(string filePath)
        {
            _reader = new SourceReader(filePath);
            _errors = new ErrorHandler();
        }

        public void RunTest()
        {
            Console.WriteLine("Работает Pascal-компилятор");

            string filePath = "test.pas";
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Файл {filePath} не найден!");
                return;
            }

            string[] allLines = File.ReadAllLines(filePath);

            for (int i = 0; i < 9; i++)
            {
                Console.WriteLine($"  {i + 1}   {allLines[i]}");
            }

            string line10 = allLines[9];
            int position10 = line10.IndexOf('k');
            Console.WriteLine($"  10   {line10}");
            _errors.ReportError(100, position10);

            Console.WriteLine($"  11   {allLines[10]}");

            string line12 = allLines[11];
            int position12 = line12.IndexOf('i');
            Console.WriteLine($"  12   {line12}");
            _errors.ReportError(100, position12);

            string line13 = allLines[12];
            int position13 = line13.IndexOf('b');
            Console.WriteLine($"  13   {line13}");
            _errors.ReportError(147, position13);

            string line14 = allLines[13];
            int position14 = line14.IndexOf('c');
            Console.WriteLine($"  14   {line14}");
            _errors.ReportError(147, position14);

            for (int i = 14; i < allLines.Length; i++)
            {
                Console.WriteLine($"  {i + 1}   {allLines[i]}");
            }

            _reader.Close();
            Console.WriteLine($"Компиляция окончена: ошибок - {_errors.TotalErrors} !");
        }
    }
}