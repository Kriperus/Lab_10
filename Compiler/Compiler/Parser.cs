using System.Collections.Generic;

namespace PascalCompiler
{
    public class Parser
    {
        private readonly LexicalAnalyzer _lexer;
        private readonly ErrorHandler _errors;
        private readonly SemanticAnalyzer _semantic;
        private readonly InputOutput _io;
        private int _tokenIndex = 0;
        private int _currentLineNumber = 0;

        public Parser(LexicalAnalyzer lexer, ErrorHandler errors, SemanticAnalyzer semantic, InputOutput io)
        {
            _lexer = lexer;
            _errors = errors;
            _semantic = semantic;
            _io = io;
        }

        public ProgramNode Parse()
        {
            _tokenIndex = 0;
            return ParseProgram();
        }

        private Token CurrentToken
        {
            get
            {
                var tokens = _lexer.Tokens;
                if (tokens == null || _tokenIndex >= tokens.Count)
                    return null;
                return tokens[_tokenIndex];
            }
        }

        private void Consume()
        {
            if (_tokenIndex < _lexer.Tokens.Count)
                _tokenIndex++;
        }

        private void PrintCurrentLine()
        {
            if (CurrentToken != null && CurrentToken.LineNumber != _currentLineNumber)
            {
                _currentLineNumber = CurrentToken.LineNumber;
                string line = _io.GetLine(_currentLineNumber);
                if (!string.IsNullOrEmpty(line))
                {
                    _io.PrintSourceLine(_currentLineNumber, line);
                }
            }
        }

        private ProgramNode ParseProgram()
        {
            ProgramNode program = new ProgramNode();

            if (CurrentToken == null) return program;

            PrintCurrentLine();

            if (CurrentToken.Type == TokenType.Program) Consume();
            else _errors.ReportError(200, "ожидается 'program'", CurrentToken.LineNumber, CurrentToken.CharPosition);

            PrintCurrentLine();

            if (CurrentToken != null && CurrentToken.Type == TokenType.Identifier)
            {
                program.Name = CurrentToken.Value;
                Consume();
            }
            else if (CurrentToken != null)
            {
                _errors.ReportError(201, "ожидается имя программы", CurrentToken.LineNumber, CurrentToken.CharPosition);
                program.Name = "unknown";
            }

            PrintCurrentLine();

            if (CurrentToken != null && CurrentToken.Type == TokenType.LeftParen)
            {
                Consume();
                while (CurrentToken != null && CurrentToken.Type != TokenType.RightParen)
                    Consume();
                if (CurrentToken != null && CurrentToken.Type == TokenType.RightParen) Consume();
            }

            PrintCurrentLine();

            if (CurrentToken != null && CurrentToken.Type == TokenType.Semicolon)
                Consume();
            else if (CurrentToken != null)
                _errors.ReportError(202, "ожидается ';'", CurrentToken.LineNumber, CurrentToken.CharPosition);

            while (CurrentToken != null && (CurrentToken.Type == TokenType.Const || CurrentToken.Type == TokenType.Type || CurrentToken.Type == TokenType.Var))
            {
                PrintCurrentLine();
                if (CurrentToken.Type == TokenType.Const) ParseConstDeclarations(program);
                else if (CurrentToken.Type == TokenType.Type) ParseTypeDeclarations(program);
                else if (CurrentToken.Type == TokenType.Var) ParseVarDeclarations(program);
            }

            PrintCurrentLine();

            if (CurrentToken != null && CurrentToken.Type == TokenType.Begin)
                program.Body = ParseCompoundStatement();
            else if (CurrentToken != null)
                _errors.ReportError(114, "ожидается 'begin'", CurrentToken.LineNumber, CurrentToken.CharPosition);

            if (CurrentToken != null && CurrentToken.Type == TokenType.End)
            {
                string endLine = _io.GetLine(CurrentToken.LineNumber);
                if (!string.IsNullOrEmpty(endLine))
                {
                    _io.PrintSourceLine(CurrentToken.LineNumber, endLine);
                }

                Consume();

                if (CurrentToken != null && CurrentToken.Type == TokenType.Dot)
                {
                    Consume();
                }

                if (CurrentToken != null && CurrentToken.Type != TokenType.EOF)
                {
                    _errors.ReportError(112, "ожидается 'end'", CurrentToken.LineNumber, CurrentToken.CharPosition);
                }
            }

            return program;
        }

        private void ParseConstDeclarations(ProgramNode program)
        {
            if (CurrentToken != null && CurrentToken.Type == TokenType.Const) Consume();

            while (CurrentToken != null && CurrentToken.Type == TokenType.Identifier)
            {
                PrintCurrentLine();
                string name = CurrentToken.Value;
                int line = CurrentToken.LineNumber;
                int pos = CurrentToken.CharPosition;
                Consume();

                if (CurrentToken != null && CurrentToken.Type == TokenType.Equal) Consume();
                else _errors.ReportError(103, "ожидается '='", line, pos);

                ExpressionNode value = ParseExpression();
                _semantic.DeclareConstant(name, value, line, pos);
                program.Declarations.Add(new ConstDeclarationNode { Name = name, Value = value });

                if (CurrentToken != null && CurrentToken.Type == TokenType.Semicolon) Consume();
                else break;
            }
        }

        private void ParseTypeDeclarations(ProgramNode program)
        {
            if (CurrentToken != null && CurrentToken.Type == TokenType.Type) Consume();

            while (CurrentToken != null && CurrentToken.Type == TokenType.Identifier)
            {
                PrintCurrentLine();
                string typeName = CurrentToken.Value;
                int line = CurrentToken.LineNumber;
                int pos = CurrentToken.CharPosition;
                Consume();

                if (CurrentToken != null && CurrentToken.Type == TokenType.Equal) Consume();
                else _errors.ReportError(103, "ожидается '='", line, pos);

                TypeNode type = ParseType();
                _semantic.DeclareType(typeName, type, line, pos);
                program.Declarations.Add(new VarDeclarationNode { Identifiers = new List<string> { typeName }, TypeNode = type });

                if (CurrentToken != null && CurrentToken.Type == TokenType.Semicolon) Consume();
                else break;
            }
        }

        private void ParseVarDeclarations(ProgramNode program)
        {
            if (CurrentToken != null && CurrentToken.Type == TokenType.Var) Consume();
            else _errors.ReportError(203, "ожидается 'var'", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);

            while (CurrentToken != null && CurrentToken.Type == TokenType.Identifier)
            {
                PrintCurrentLine();
                List<string> ids = new List<string>();

                while (CurrentToken != null && CurrentToken.Type == TokenType.Identifier)
                {
                    ids.Add(CurrentToken.Value);
                    Consume();
                    if (CurrentToken != null && CurrentToken.Type == TokenType.Comma) Consume();
                    else break;
                }

                if (CurrentToken != null && CurrentToken.Type == TokenType.Colon) Consume();
                else _errors.ReportError(104, "ожидается ':'", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);

                TypeNode type = ParseType();

                if (type != null)
                {
                    foreach (var id in ids)
                    {
                        _semantic.DeclareVariable(id, type, CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);
                        program.Declarations.Add(new VarDeclarationNode { Identifiers = new List<string> { id }, TypeNode = type });
                    }
                }

                if (CurrentToken != null && CurrentToken.Type == TokenType.Semicolon) Consume();
                else _errors.ReportError(105, "ожидается ';'", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);
            }
        }

        private TypeNode ParseType()
        {
            if (CurrentToken != null && (CurrentToken.Type == TokenType.Integer || CurrentToken.Type == TokenType.Real ||
                CurrentToken.Type == TokenType.Boolean || CurrentToken.Type == TokenType.Char || CurrentToken.Type == TokenType.String))
            {
                string typeName = CurrentToken.Value;
                Consume();
                return new StandardTypeNode { TypeName = typeName };
            }
            else if (CurrentToken != null && CurrentToken.Type == TokenType.Record)
                return ParseRecordType();
            else if (CurrentToken != null && CurrentToken.Type == TokenType.Array)
                return ParseArrayType();
            else if (CurrentToken != null && CurrentToken.Type == TokenType.Identifier)
            {
                string typeName = CurrentToken.Value;
                Consume();
                TypeNode declaredType = _semantic.GetType(typeName);
                if (declaredType != null)
                    return declaredType;
                else
                    return new StandardTypeNode { TypeName = typeName };
            }
            else
            {
                _errors.ReportError(204, "ожидается тип", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);
                return null;
            }
        }

        private RecordTypeNode ParseRecordType()
        {
            RecordTypeNode record = new RecordTypeNode();
            if (CurrentToken != null && CurrentToken.Type == TokenType.Record) Consume();

            while (CurrentToken != null && CurrentToken.Type == TokenType.Identifier)
            {
                PrintCurrentLine();
                List<string> ids = new List<string>();
                while (CurrentToken != null && CurrentToken.Type == TokenType.Identifier)
                {
                    ids.Add(CurrentToken.Value);
                    Consume();
                    if (CurrentToken != null && CurrentToken.Type == TokenType.Comma) Consume();
                    else break;
                }

                if (CurrentToken != null && CurrentToken.Type == TokenType.Colon) Consume();
                else _errors.ReportError(104, "ожидается ':'", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);

                TypeNode type = ParseType();
                record.Fields.Add(new VarDeclarationNode { Identifiers = ids, TypeNode = type });

                if (CurrentToken != null && CurrentToken.Type == TokenType.Semicolon) Consume();
                else break;
            }

            if (CurrentToken != null && CurrentToken.Type == TokenType.End)
            {
                string endLine = _io.GetLine(CurrentToken.LineNumber);
                if (!string.IsNullOrEmpty(endLine))
                {
                    _io.PrintSourceLine(CurrentToken.LineNumber, endLine);
                }
                Consume();
            }
            else if (CurrentToken != null)
            {
                _errors.ReportError(112, "ожидается 'end' для записи", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);
            }

            return record;
        }

        private ArrayTypeNode ParseArrayType()
        {
            ArrayTypeNode array = new ArrayTypeNode();
            if (CurrentToken != null && CurrentToken.Type == TokenType.Array) Consume();

            if (CurrentToken != null && CurrentToken.Type == TokenType.LeftBracket) Consume();
            else _errors.ReportError(109, "ожидается '['", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);

            array.LowerBound = ParseExpression();

            if (CurrentToken != null && CurrentToken.Type == TokenType.DotDot) Consume();
            else _errors.ReportError(107, "ожидается '..'", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);

            array.UpperBound = ParseExpression();

            if (CurrentToken != null && CurrentToken.Type == TokenType.RightBracket) Consume();
            else _errors.ReportError(110, "ожидается ']'", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);

            if (CurrentToken != null && CurrentToken.Type == TokenType.Of) Consume();
            else _errors.ReportError(108, "ожидается 'of'", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);

            array.ElementType = ParseType();
            return array;
        }

        private CompoundStatementNode ParseCompoundStatement()
        {
            CompoundStatementNode compound = new CompoundStatementNode();
            if (CurrentToken != null && CurrentToken.Type == TokenType.Begin) Consume();
            else _errors.ReportError(114, "ожидается 'begin'", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);

            while (CurrentToken != null && CurrentToken.Type != TokenType.End && CurrentToken.Type != TokenType.EOF)
            {
                PrintCurrentLine();
                ASTNode stmt = ParseStatement();
                if (stmt != null) compound.Statements.Add(stmt);
                if (CurrentToken != null && CurrentToken.Type == TokenType.Semicolon) Consume();
            }

            if (CurrentToken != null && CurrentToken.Type == TokenType.End)
            {
                string endLine = _io.GetLine(CurrentToken.LineNumber);
                if (!string.IsNullOrEmpty(endLine))
                {
                    _io.PrintSourceLine(CurrentToken.LineNumber, endLine);
                }
                Consume();
            }
            else if (CurrentToken != null)
            {
                _errors.ReportError(112, "ожидается 'end'", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);
            }

            return compound;
        }

        private ASTNode ParseStatement()
        {
            if (CurrentToken == null || CurrentToken.Type == TokenType.EOF) return null;

            PrintCurrentLine();

            if (CurrentToken.Type == TokenType.Identifier)
            {
                string name = CurrentToken.Value;
                int line = CurrentToken.LineNumber;
                int pos = CurrentToken.CharPosition;
                Consume();

                if (CurrentToken != null && CurrentToken.Type == TokenType.Assign)
                {
                    Consume();
                    ExpressionNode value = ParseExpression();
                    _semantic.CheckVariableOrFieldInWith(name, line, pos);
                    return new AssignmentNode { Target = new VariableNode { Name = name }, Value = value };
                }

                if (CurrentToken != null && CurrentToken.Type == TokenType.LeftBracket)
                {
                    Consume();
                    ExpressionNode index = ParseExpression();
                    if (CurrentToken != null && CurrentToken.Type == TokenType.RightBracket) Consume();
                    else _errors.ReportError(110, "ожидается ']'", line, pos);

                    if (CurrentToken != null && CurrentToken.Type == TokenType.Assign)
                    {
                        Consume();
                        ExpressionNode value = ParseExpression();
                        _semantic.CheckVariableOrFieldInWith(name, line, pos);
                        return new AssignmentNode { Target = new IndexedVariableNode { ArrayName = name, Index = index }, Value = value };
                    }
                }

                if (CurrentToken != null && CurrentToken.Type == TokenType.Dot)
                {
                    Consume();
                    if (CurrentToken != null && CurrentToken.Type == TokenType.Identifier)
                    {
                        string fieldName = CurrentToken.Value;
                        int fLine = CurrentToken.LineNumber;
                        int fPos = CurrentToken.CharPosition;
                        Consume();

                        if (CurrentToken != null && CurrentToken.Type == TokenType.Assign)
                        {
                            Consume();
                            ExpressionNode value = ParseExpression();
                            _semantic.CheckVariableOrFieldInWith(fieldName, fLine, fPos, isField: true);
                            return new AssignmentNode { Target = new FieldAccessNode { RecordName = name, FieldName = fieldName }, Value = value };
                        }
                    }
                    else _errors.ReportError(103, "ожидается имя поля", line, pos);
                }
            }

            if (CurrentToken != null && CurrentToken.Type == TokenType.Begin)
                return ParseCompoundStatement();

            if (CurrentToken != null && CurrentToken.Type == TokenType.With)
                return ParseWithStatement();

            if (CurrentToken != null)
                _errors.ReportError(216, "неизвестный оператор", CurrentToken.LineNumber, CurrentToken.CharPosition);

            Consume();
            return null;
        }

        private WithNode ParseWithStatement()
        {
            WithNode with = new WithNode();
            if (CurrentToken != null && CurrentToken.Type == TokenType.With) Consume();

            string recordName = "";
            if (CurrentToken != null && CurrentToken.Type == TokenType.Identifier)
            {
                recordName = CurrentToken.Value;
                _semantic.EnterWithContext(recordName, CurrentToken.LineNumber, CurrentToken.CharPosition);
                Consume();
            }
            else _errors.ReportError(103, "ожидается имя записи", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);

            if (CurrentToken != null && CurrentToken.Type == TokenType.Do) Consume();
            else _errors.ReportError(113, "ожидается 'do'", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);

            ASTNode body = ParseStatement();
            if (body != null) with.Statements.Add(body);

            _semantic.ExitWithContext();
            with.RecordName = recordName;
            return with;
        }

        private ExpressionNode ParseExpression() => ParseSimpleExpression();

        private ExpressionNode ParseSimpleExpression()
        {
            ExpressionNode left = ParseTerm();
            while (CurrentToken != null && (CurrentToken.Type == TokenType.Plus || CurrentToken.Type == TokenType.Minus))
            {
                PrintCurrentLine();
                string op = CurrentToken.Value;
                Consume();
                left = new BinaryOperationNode { Left = left, Operator = op, Right = ParseTerm() };
            }
            return left;
        }

        private ExpressionNode ParseTerm()
        {
            ExpressionNode left = ParseFactor();
            while (CurrentToken != null && (CurrentToken.Type == TokenType.Multiply || CurrentToken.Type == TokenType.Divide))
            {
                PrintCurrentLine();
                string op = CurrentToken.Value;
                Consume();
                left = new BinaryOperationNode { Left = left, Operator = op, Right = ParseFactor() };
            }
            return left;
        }

        private ExpressionNode ParseFactor()
        {
            if (CurrentToken == null) return null;

            PrintCurrentLine();

            if (CurrentToken.Type == TokenType.IntegerNumber)
            {
                int val = int.Parse(CurrentToken.Value);
                Consume();
                return new IntegerConstantNode { Value = val };
            }
            if (CurrentToken.Type == TokenType.RealNumber)
            {
                double val = double.Parse(CurrentToken.Value, System.Globalization.CultureInfo.InvariantCulture);
                Consume();
                return new RealConstantNode { Value = val };
            }
            if (CurrentToken.Type == TokenType.Quote)
            {
                string val = CurrentToken.Value;
                Consume();
                return val.Length == 1 ? new CharConstantNode { Value = val[0] } : new StringConstantNode { Value = val };
            }
            if (CurrentToken.Type == TokenType.Identifier)
            {
                string name = CurrentToken.Value;
                int line = CurrentToken.LineNumber;
                int pos = CurrentToken.CharPosition;
                Consume();
                return new VariableNode { Name = name, LineNumber = line, CharPosition = pos };
            }
            if (CurrentToken.Type == TokenType.LeftParen)
            {
                Consume();
                ExpressionNode expr = ParseExpression();
                if (CurrentToken != null && CurrentToken.Type == TokenType.RightParen) Consume();
                else _errors.ReportError(110, "ожидается ')'", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);
                return expr;
            }
            _errors.ReportError(103, "ожидается выражение", CurrentToken?.LineNumber ?? 1, CurrentToken?.CharPosition ?? 0);
            Consume();
            return null;
        }
    }
}
