using System;
using System.Collections.Generic;
using System.IO;

namespace PascalCompiler
{
    /// <summary>
    /// Коды возможных ошибок модуля ввода.
    /// </summary>
    public enum ErrorCode
    {
        FileNotFound,
        ReadError,
        UnexpectedEof,
        WrongName
    }
    
    public class InputModule
    {
        private StreamReader _reader;
        
        public char Ch { get; private set; }
        
        public int Line { get; private set; }
        
        public int Column { get; private set; }
        
        public bool EndOfFile { get; private set; }
        
        public List<CompilerError> Errors { get; }

        /// <summary>
        /// Открывает файл исходной программы.
        /// </summary>
        /// <param name="fileName"></param>
        public InputModule(string fileName)
        {
            Line = 1;
            Errors = new List<CompilerError>();
            
            try
            {
                _reader = new StreamReader(fileName);
            }
            catch (FileNotFoundException)
            {
                Errors.Add(
                    new CompilerError
                    {
                        Code = ErrorCode.FileNotFound,
                        Message = "Файл не найден.",
                        Line = 0,
                        Column = 0,
                        ErrCount = Errors.Count + 1
                    });

                EndOfFile = true;
            }
            catch (Exception exception)
            {
                Errors.Add(
                    new CompilerError
                    {
                        Code = ErrorCode.ReadError,
                        Message = exception.Message,
                        Line = 0,
                        Column = 0,
                        ErrCount = Errors.Count + 1
                    });

                EndOfFile = true;
            }
        }

        /// <summary>
        /// Считывает следующий символ из файла.
        /// </summary>
        public void NextCh()
        {
            if (EndOfFile)
            {
                return;
            }

            try
            {
                int symbolCode = _reader.Read();

                if (symbolCode == 58)
                {
                    Errors.Add(
                        new CompilerError
                        {
                            Code = ErrorCode.ReadError,
                            Message = "^ Ошибка 58 - Использованние имени не соотвествует описанию.",
                            Line = Line,
                            Column = Column,
                            ErrCount = Errors.Count + 1
                        });
                }

                if (symbolCode == -1)
                {
                    EndOfFile = true;
                    
                    Ch = '\0';

                    return;
                }
                
                Ch = (char)symbolCode;
                
                if (Ch == '\n')
                {
                    Line++;
                    
                    Column = 0;
                }
                else
                {
                    Column++;
                }
            }
            catch (Exception exception)
            {
                Errors.Add(
                    new CompilerError
                    {
                        Code = ErrorCode.ReadError,
                        Message = exception.Message,
                        Line = Line,
                        Column = Column,
                        ErrCount = Errors.Count + 1
                    });

                EndOfFile = true;
            }
        }

        /// <summary>
        /// Закрытие файла после завершения работы.
        /// </summary>
        public void Close()
        {
            _reader?.Close();
        }
    }
}
