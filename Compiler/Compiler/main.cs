using System;

namespace PascalCompiler
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                const string filePath = "test_errors.pas";

                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"Файл {filePath} не найден!");
                    return;
                }

                TestDriver test = new TestDriver(filePath);
                test.RunTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
