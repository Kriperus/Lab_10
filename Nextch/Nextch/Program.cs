using System;
using System.Collections.Generic;
using System.IO;

namespace PascalCompiler
{
    internal class Program
    {
        private static void Main()
        {
            // Набор тестовых файлов.
            Dictionary<string, string> testFiles =
                new Dictionary<string, string>
                {
                    { "Test1.txt", "abc" },
                    { "Test2.txt", string.Empty },
                    { "Test3.txt", "a\nb" },
                    { "Test5.txt", "program test;" },
                    { "Test6.txt", "Привет\nмир" }
                };

            // Создание тестовых файлов.
            foreach (KeyValuePair<string, string> file in testFiles)
            {
                File.WriteAllText(file.Key, file.Value);
            }

            // Выполнение тестов.
            foreach (KeyValuePair<string, string> file in testFiles)
            {
                Console.WriteLine(
                    $"========== ТЕСТ: {file.Key} ==========");

                InputModule inputModule =
                    new InputModule(file.Key);

                // Проверяем ошибки открытия файла.
                if (inputModule.Errors.Count > 0)
                {
                    foreach (CompilerError error
                             in inputModule.Errors)
                    {
                        Console.WriteLine(
                            $"Ошибка: {error.Code}");
                    }

                    continue;
                }

                // Читаем файл посимвольно.
                while (!inputModule.EndOfFile)
                {
                    inputModule.NextCh();

                    if (!inputModule.EndOfFile)
                    {
                        Console.WriteLine(
                            $"Символ: '{inputModule.Ch}' " +
                            $"Строка: {inputModule.Line} " +
                            $"Столбец: {inputModule.Column}");
                    }
                }

                Console.WriteLine("Конец файла.");

                inputModule.Close();

                Console.WriteLine();
            }

            // Отдельный тест отсутствующего файла.
            Console.WriteLine(
                "========== ТЕСТ НЕСУЩЕСТВУЮЩЕГО ФАЙЛА ==========");

            InputModule missingFile =
                new InputModule("UnknownFile.txt");

            foreach (CompilerError error in missingFile.Errors)
            {
                Console.WriteLine(
                    $"Ошибка: {error.Code} - {error.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Тестирование завершено.");
        }
    }
}