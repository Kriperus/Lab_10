using System;
using System.IO;

namespace PascalCompiler
{
    public class TestDriver
    {
        private SourceReader _reader;
        private ErrorHandler _errors;
        private LexicalAnalyzer _analyzer;

        public TestDriver(string filePath)
        {
            _reader = new SourceReader(filePath);
            _errors = new ErrorHandler();
            _analyzer = new LexicalAnalyzer(_reader, _errors);
        }

        public void RunTest()
        {
            Console.WriteLine("Работает Pascal-компилятор");

            Console.WriteLine("\n=== ЛЕКСИЧЕСКИЙ АНАЛИЗ ===");

            _analyzer.Analyze();

            int tokenCount = 0;
            foreach (Token token in _analyzer.Tokens)
            {
                Console.WriteLine(token);
                tokenCount++;
                if (tokenCount >= 10) break;
            }
            Console.WriteLine("...\n");

            string outputFile = "token_codes.txt";
            _analyzer.SaveTokenCodesToFile(outputFile);
            Console.WriteLine($"Коды лексем сохранены в файл: {outputFile}");

            Console.WriteLine($"Всего токенов: {_analyzer.Tokens.Count}");
            Console.WriteLine($"Всего ошибок: {_errors.TotalErrors}");
        }
    }
}
