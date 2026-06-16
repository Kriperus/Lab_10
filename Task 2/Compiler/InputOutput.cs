using System;
using System.Collections.Generic;
using System.IO;

namespace PascalCompiler
{
    public class SourceReader
    {
        private readonly StreamReader _reader;
        private readonly List<string> _allLines;
        private string _currentLine;
        private int _lineNumber;
        private int _charIndex;
        private bool _endOfFile;

        public char CurrentChar { get; private set; }
        public int LineNumber => _lineNumber;
        public int CharIndex => _charIndex;
        public string CurrentLineText => _currentLine ?? string.Empty;
        public bool IsEndOfFile => _endOfFile;

        public SourceReader(string filePath)
        {
            _allLines = new List<string>(File.ReadAllLines(filePath));
            _reader = new StreamReader(filePath);
            _lineNumber = 0;
            _charIndex = -1;
            _endOfFile = false;
            _currentLine = null;

            ReadNextLine();
        }

        public string GetLine(int lineNumber)
        {
            if (lineNumber - 1 < _allLines.Count && lineNumber - 1 >= 0)
                return _allLines[lineNumber - 1];
            return "";
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
