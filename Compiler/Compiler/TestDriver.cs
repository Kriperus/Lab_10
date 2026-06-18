using System;

namespace PascalCompiler
{
    public class TestDriver
    {
        private readonly InputOutput _io;
        private readonly SourceReader _reader;
        private readonly ErrorHandler _errors;
        private readonly LexicalAnalyzer _lexer;
        private readonly Parser _parser;
        private readonly SemanticAnalyzer _semantic;

        public TestDriver(string filePath)
        {
            _io = new InputOutput(filePath);
            _io.PrintHeader();

            _reader = new SourceReader(filePath);
            _errors = new ErrorHandler(_io);
            _lexer = new LexicalAnalyzer(_reader, _errors);
            _semantic = new SemanticAnalyzer(_errors);

            _parser = new Parser(_lexer, _errors, _semantic, _io);
        }

        public void RunTest()
        {
            _lexer.Analyze();
            _lexer.SaveTokenCodesToFile("token_codes.txt");

            ProgramNode program = _parser.Parse();
            _errors.PrintSummary();
        }
    }
}
