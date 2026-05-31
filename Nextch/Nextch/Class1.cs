using System;
using System.Collections.Generic;
using System.IO;

namespace PascalCompiler
{
    // Коды возможных ошибок модуля ввода.
    public enum ErrorCode
    {
        FileNotFound,
        ReadError,
        UnexpectedEOF
    }

    // Описание одной ошибки компилятора.
    public class CompilerError
    {
        // Код ошибки.
        public ErrorCode Code { get; set; }

        // Текст ошибки.
        public string Message { get; set; }

        // Номер строки, где возникла ошибка.
        public int Line { get; set; }

        // Номер столбца, где возникла ошибка.
        public int Column { get; set; }
    }

    // Модуль ввода исходного текста программы.
    // Обеспечивает посимвольное чтение файла.
    public class InputModule
    {
        // Поток чтения файла.
        private StreamReader _reader;

        // Текущий считанный символ.
        public char Ch { get; private set; }

        // Текущий номер строки.
        // Начинается с 1.
        public int Line { get; private set; } = 1;

        // Текущий номер столбца.
        public int Column { get; private set; }

        // Флаг конца файла.
        public bool EndOfFile { get; private set; }

        // Список найденных ошибок.
        public List<CompilerError> Errors { get; } =
            new List<CompilerError>();

        // Открывает файл исходной программы.
        // name="fileName">Имя файла.
        public InputModule(string fileName)
        {
            try
            {
                // Пытаемся открыть файл для чтения.
                _reader = new StreamReader(fileName);
            }
            catch (FileNotFoundException)
            {
                // Файл не найден.
                Errors.Add(
                    new CompilerError
                    {
                        Code = ErrorCode.FileNotFound,
                        Message = "Файл не найден.",
                        Line = 0,
                        Column = 0
                    });

                EndOfFile = true;
            }
            catch (Exception exception)
            {
                // Любая другая ошибка открытия файла.
                Errors.Add(
                    new CompilerError
                    {
                        Code = ErrorCode.ReadError,
                        Message = exception.Message,
                        Line = 0,
                        Column = 0
                    });

                EndOfFile = true;
            }
        }

        // Считывает следующий символ из файла.
        public void NextCh()
        {
            // Если конец файла уже достигнут,
            // дальнейшее чтение невозможно.
            if (EndOfFile)
            {
                return;
            }

            try
            {
                // Считываем следующий символ.
                // Метод Read() возвращает:
                // - код символа,
                // - либо -1 при достижении конца файла.
                int symbolCode = _reader.Read();

                // Проверяем конец файла.
                if (symbolCode == -1)
                {
                    EndOfFile = true;

                    // Символ конца файла.
                    Ch = '\0';

                    return;
                }

                // Преобразуем код в символ.
                Ch = (char)symbolCode;

                // Если встретился перевод строки,
                // увеличиваем номер строки.
                if (Ch == '\n')
                {
                    Line++;

                    // Переходим в начало новой строки.
                    Column = 0;
                }
                else
                {
                    // Для обычного символа
                    // увеличиваем номер столбца.
                    Column++;
                }
            }
            catch (Exception exception)
            {
                // Ошибка чтения файла.
                Errors.Add(
                    new CompilerError
                    {
                        Code = ErrorCode.ReadError,
                        Message = exception.Message,
                        Line = Line,
                        Column = Column
                    });

                EndOfFile = true;
            }
        }

        // Закрытие файла после завершения работы.
        public void Close()
        {
            _reader?.Close();
        }
    }
}