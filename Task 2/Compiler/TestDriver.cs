using System;
using System.IO;

namespace PascalCompiler
{
    public class TestDriver
    {
        private SourceReader _reader;
        private ErrorHandler _errors;
        private LexicalAnalyzer _lexer;
        private Parser _parser;
        private SemanticAnalyzer _semantic;

        public TestDriver(string filePath)
        {
            _reader = new SourceReader(filePath);
            _errors = new ErrorHandler();
            _lexer = new LexicalAnalyzer(_reader, _errors);
        }

        public void RunTest()
        {
            Console.WriteLine("Работает Pascal-компилятор");
            Console.WriteLine("==========================================");
            Console.WriteLine("1. ЛЕКСИЧЕСКИЙ АНАЛИЗ");
            Console.WriteLine("   (преобразование исходного кода в токены)");
            Console.WriteLine("------------------------------------------");

            _lexer.Analyze();
            Console.WriteLine("   Первые 10 токенов:");
            int count = 0;
            foreach (Token token in _lexer.Tokens)
            {
                Console.WriteLine($"   {token}");
                if (++count >= 10) break;
            }
            Console.WriteLine($"   ... (всего {_lexer.Tokens.Count} токенов)");

            string outputFile = "token_codes.txt";
            _lexer.SaveTokenCodesToFile(outputFile);
            Console.WriteLine($"   Коды лексем сохранены в файл: {outputFile}");
            Console.WriteLine();

            Console.WriteLine("2. СИНТАКСИЧЕСКИЙ АНАЛИЗ");
            Console.WriteLine("   (построение AST с нейтрализацией ошибок)");
            Console.WriteLine("------------------------------------------");

            _reader.Close();
            _reader = new SourceReader("test.pas");
            _lexer = new LexicalAnalyzer(_reader, _errors);
            _lexer.Analyze();

            _parser = new Parser(_lexer, _errors);
            ProgramNode program = _parser.Parse();

            if (program != null)
            {
                Console.WriteLine("   ✅ Синтаксический анализ успешно завершён");
                Console.WriteLine($"   Имя программы: {program.Name}");
                Console.WriteLine($"   Объявлений: {program.Declarations.Count}");
                Console.WriteLine($"   Составных операторов в теле: {(program.Body?.Statements.Count ?? 0)}");
            }
            Console.WriteLine();

            Console.WriteLine("3. СЕМАНТИЧЕСКИЙ АНАЛИЗ");
            Console.WriteLine("   (проверка объявлений и типов)");
            Console.WriteLine("------------------------------------------");

            _semantic = new SemanticAnalyzer(_errors);
            _semantic.Analyze(program);

            Console.WriteLine("   ✅ Семантический анализ завершён");
            Console.WriteLine();

            Console.WriteLine("4. ИТОГИ");
            Console.WriteLine("------------------------------------------");
            Console.WriteLine($"   Всего токенов: {_lexer.Tokens.Count}");
            Console.WriteLine($"   Всего ошибок: {_errors.TotalErrors}");
            Console.WriteLine("==========================================");
        }
    }
}
