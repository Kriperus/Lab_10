using System;
using System.Collections.Generic;
using System.IO;

namespace PascalCompiler
{
    public class InputOutput
    {
        private readonly List<string> _allLines;

        public InputOutput(string filePath)
        {
            _allLines = new List<string>(File.ReadAllLines(filePath));
        }

        public string GetLine(int lineNumber)
        {
            if (lineNumber - 1 < _allLines.Count && lineNumber - 1 >= 0)
                return _allLines[lineNumber - 1];
            return "";
        }

        public void PrintHeader()
        {
            Console.WriteLine("Работает Pascal-компилятор");
            Console.WriteLine();
        }

        public void PrintSourceLine(int lineNumber, string lineContent)
        {
            Console.WriteLine($"{lineNumber,3} {lineContent}");
        }

        public void PrintError(int errorIndex, int errorCode, string message, int lineNumber, int charPosition)
        {
            string arrow = new string(' ', Math.Max(0, charPosition) - 2) + "^";
            Console.WriteLine($"******{arrow}  ошибка код {errorCode}");
            Console.WriteLine($"******  {message}");
            Console.WriteLine();
        }

        public void PrintSummary(int errorCount)
        {
            Console.WriteLine($"Компиляция окончена: ошибок - {errorCount} !");
        }
    }

    public class SourceReader
    {
        private readonly StreamReader _reader;
        private string _currentLine;
        private int _lineNumber;
        private int _charIndex;
        private bool _endOfFile;

        public char CurrentChar { get; private set; }
        public int LineNumber => _lineNumber;
        public int CharIndex => _charIndex;
        public bool IsEndOfFile => _endOfFile;

        public SourceReader(string filePath)
        {
            _reader = new StreamReader(filePath);
            _lineNumber = 0;
            _charIndex = -1;
            _endOfFile = false;
            _currentLine = null;
            ReadNextLine();
        }

        public void NextChar()
        {
            if (_endOfFile) return;
            _charIndex++;

            if (_currentLine != null && _charIndex >= _currentLine.Length)
            {
                ReadNextLine();
                return;
            }

            if (_currentLine != null && _charIndex < _currentLine.Length)
                CurrentChar = _currentLine[_charIndex];
            else
                CurrentChar = '\0';
        }

        public (int line, int index, char current) SaveState()
        {
            return (_lineNumber, _charIndex, CurrentChar);
        }

        public void RestoreState((int line, int index, char current) state)
        {
            if (_lineNumber == state.line)
            {
                _charIndex = state.index;
                CurrentChar = state.current;
            }
        }

        private void ReadNextLine()
        {
            if (_reader.EndOfStream)
            {
                _endOfFile = true;
                _currentLine = null;
                CurrentChar = '\0';
                return;
            }

            _currentLine = _reader.ReadLine();
            _lineNumber++;
            _charIndex = 0;

            if (_currentLine != null && _currentLine.Length > 0)
                CurrentChar = _currentLine[0];
            else
                CurrentChar = '\0';
        }

        public void Close()
        {
            _reader?.Close();
        }
    }
}
