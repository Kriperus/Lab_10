using System;
using System.Collections.Generic;

namespace PascalCompiler
{
    public class Parser
    {
        private readonly LexicalAnalyzer _lexer;
        private readonly ErrorHandler _errors;
        private readonly SourceReader _reader;
        private int _tokenIndex = 0;
        private List<Token> _tokens;

        public Parser(LexicalAnalyzer lexer, ErrorHandler errors)
        {
            _lexer = lexer;
            _errors = errors;
            _reader = _lexer.Reader;
            _tokens = new List<Token>(_lexer.Tokens);
        }

        private void ReportParserError(int errorCode)
        {
            int lineNum = CurrentToken?.LineNumber ?? _reader.LineNumber;
            string currentLine = _reader.GetLine(lineNum);
            int charPos = CurrentToken?.CharPosition ?? _reader.CharIndex;
            _errors.ReportError(errorCode, currentLine, lineNum, charPos);
        }

        public ProgramNode Parse()
        {
            _tokenIndex = 0;
            return ParseProgram();
        }

        private ProgramNode ParseProgram()
        {
            ProgramNode program = new ProgramNode();

            if (CurrentToken.Type == TokenType.Program)
                Consume();
            else
            {
                ReportParserError(200);
                while (CurrentToken != null && CurrentToken.Type != TokenType.Identifier)
                    Consume();
            }

            if (CurrentToken.Type == TokenType.Identifier)
            {
                program.Name = CurrentToken.Value;
                Consume();
            }
            else
            {
                ReportParserError(201);
                program.Name = "unknown";
            }

            if (CurrentToken.Type == TokenType.LeftParen)
            {
                Consume();
                while (CurrentToken.Type != TokenType.RightParen && CurrentToken.Type != TokenType.EOF)
                    Consume();
                if (CurrentToken.Type == TokenType.RightParen)
                    Consume();
            }

            if (CurrentToken.Type == TokenType.Semicolon)
                Consume();

            while (CurrentToken.Type != TokenType.Begin && CurrentToken.Type != TokenType.Var &&
                   CurrentToken.Type != TokenType.Const && CurrentToken.Type != TokenType.Type &&
                   CurrentToken.Type != TokenType.EOF)
                Consume();

            while (CurrentToken.Type == TokenType.Var || CurrentToken.Type == TokenType.Const ||
                   CurrentToken.Type == TokenType.Type)
            {
                if (CurrentToken.Type == TokenType.Var)
                    program.Declarations.AddRange(ParseVarDeclarations());
                else
                {
                    ReportParserError(224);
                    Consume();
                }
            }

            while (CurrentToken.Type != TokenType.Begin && CurrentToken.Type != TokenType.EOF)
                Consume();

            if (CurrentToken.Type == TokenType.Begin)
                program.Body = ParseCompoundStatement();
            else
                ReportParserError(214);

            if (CurrentToken.Type == TokenType.End)
            {
                Consume();
                if (CurrentToken.Type == TokenType.Dot)
                    Consume();
                else
                    ReportParserError(222);
            }
            else if (CurrentToken.Type == TokenType.EOF)
                return program;
            else
                ReportParserError(223);

            return program;
        }

        private List<VarDeclarationNode> ParseVarDeclarations()
        {
            List<VarDeclarationNode> declarations = new List<VarDeclarationNode>();

            if (CurrentToken.Type == TokenType.Var)
                Consume();
            else
                return declarations;

            while (CurrentToken.Type == TokenType.Identifier)
            {
                VarDeclarationNode decl = ParseVarDeclaration();
                if (decl != null)
                    declarations.Add(decl);
                if (CurrentToken.Type == TokenType.Semicolon)
                    Consume();
                else
                {
                    while (CurrentToken != null && CurrentToken.Type != TokenType.Semicolon &&
                           CurrentToken.Type != TokenType.Identifier && CurrentToken.Type != TokenType.Var)
                        Consume();
                    if (CurrentToken.Type == TokenType.Semicolon)
                        Consume();
                }
            }

            return declarations;
        }

        private VarDeclarationNode ParseVarDeclaration()
        {
            VarDeclarationNode decl = new VarDeclarationNode();

            while (CurrentToken.Type == TokenType.Identifier)
            {
                decl.Identifiers.Add(CurrentToken.Value);
                Consume();
                if (CurrentToken.Type == TokenType.Comma)
                    Consume();
                else
                    break;
            }

            if (CurrentToken.Type == TokenType.Colon)
                Consume();
            else
            {
                ReportParserError(204);
                return null;
            }

            decl.TypeNode = ParseType();
            if (decl.TypeNode == null)
                return null;
            else
                decl.TypeName = decl.TypeNode.GetType().Name;

            return decl;
        }

        private TypeNode ParseType()
        {
            if (CurrentToken.Type == TokenType.Integer || CurrentToken.Type == TokenType.Real ||
                CurrentToken.Type == TokenType.Boolean || CurrentToken.Type == TokenType.Char ||
                CurrentToken.Type == TokenType.String)
            {
                string typeName = CurrentToken.Value;
                Consume();
                return new StandardTypeNode { TypeName = typeName };
            }
            else if (CurrentToken.Type == TokenType.Record)
                return ParseRecordType();
            else if (CurrentToken.Type == TokenType.Array)
                return ParseArrayType();
            else
            {
                ReportParserError(206);
                return null;
            }
        }

        private RecordTypeNode ParseRecordType()
        {
            RecordTypeNode record = new RecordTypeNode();
            Consume();

            while (CurrentToken.Type == TokenType.Identifier)
            {
                VarDeclarationNode field = ParseVarDeclaration();
                if (field != null)
                    record.Fields.Add(field);
                if (CurrentToken.Type == TokenType.Semicolon)
                    Consume();
                else if (CurrentToken.Type == TokenType.End)
                    break;
                else
                {
                    while (CurrentToken != null && CurrentToken.Type != TokenType.Semicolon &&
                           CurrentToken.Type != TokenType.End && CurrentToken.Type != TokenType.EOF)
                        Consume();
                    if (CurrentToken.Type == TokenType.Semicolon)
                        Consume();
                }
            }

            if (CurrentToken.Type == TokenType.End)
                Consume();
            else
                ReportParserError(207);

            return record;
        }

        private ArrayTypeNode ParseArrayType()
        {
            ArrayTypeNode array = new ArrayTypeNode();
            Consume();

            if (CurrentToken.Type == TokenType.LeftBracket)
                Consume();
            else
                ReportParserError(208);

            if (CurrentToken.Type == TokenType.IntegerNumber)
            {
                array.LowerBound = int.Parse(CurrentToken.Value);
                Consume();
            }
            else
                ReportParserError(209);

            if (CurrentToken.Type == TokenType.DotDot)
                Consume();
            else
                ReportParserError(210);

            if (CurrentToken.Type == TokenType.IntegerNumber)
            {
                array.UpperBound = int.Parse(CurrentToken.Value);
                Consume();
            }
            else
                ReportParserError(211);

            if (CurrentToken.Type == TokenType.RightBracket)
                Consume();
            else
                ReportParserError(212);

            if (CurrentToken.Type == TokenType.Of)
                Consume();
            else
                ReportParserError(213);

            array.ElementType = ParseType();
            return array;
        }

        private CompoundStatementNode ParseCompoundStatement()
        {
            CompoundStatementNode compound = new CompoundStatementNode();

            if (CurrentToken.Type == TokenType.Begin)
                Consume();
            else
            {
                ReportParserError(214);
                return null;
            }

            while (CurrentToken.Type != TokenType.End && CurrentToken.Type != TokenType.EOF)
            {
                ASTNode stmt = ParseStatement();
                if (stmt != null)
                    compound.Statements.Add(stmt);
            }

            if (CurrentToken.Type == TokenType.End)
                Consume();

            return compound;
        }

        private ASTNode ParseStatement()
        {
            if (CurrentToken.Type == TokenType.EOF)
                return null;

            if (CurrentToken.Type == TokenType.Identifier)
            {
                string name = CurrentToken.Value;
                Consume();

                if (CurrentToken.Type == TokenType.Assign)
                    return ParseAssignment(name);
                else if (CurrentToken.Type == TokenType.LeftBracket)
                {
                    ExpressionNode expr = ParseIndexedVariable(name);
                    if (CurrentToken.Type == TokenType.Assign)
                    {
                        Consume();
                        ExpressionNode value = ParseExpression();
                        return new AssignmentNode { Target = expr, Value = value };
                    }
                    else
                    {
                        ReportParserError(216);
                        return null;
                    }
                }
                else if (CurrentToken.Type == TokenType.Dot)
                {
                    ExpressionNode expr = ParseFieldAccess(name);
                    if (CurrentToken.Type == TokenType.Assign)
                    {
                        Consume();
                        ExpressionNode value = ParseExpression();
                        return new AssignmentNode { Target = expr, Value = value };
                    }
                    else
                    {
                        ReportParserError(216);
                        return null;
                    }
                }
                else
                {
                    ReportParserError(216);
                    return null;
                }
            }
            else if (CurrentToken.Type == TokenType.Begin)
                return ParseCompoundStatement();
            else if (CurrentToken.Type == TokenType.With)
                return ParseWithStatement();
            else if (CurrentToken.Type == TokenType.Semicolon)
            {
                Consume();
                return null;
            }
            else
            {
                ReportParserError(216);
                Consume();
                return null;
            }
        }

        private AssignmentNode ParseAssignment(string variableName)
        {
            AssignmentNode assign = new AssignmentNode();
            Consume();

            ExpressionNode expr = ParseExpression();
            assign.Target = new VariableNode { Name = variableName };
            assign.Value = expr;

            if (CurrentToken.Type == TokenType.Semicolon)
                Consume();

            return assign;
        }

        private WithNode ParseWithStatement()
        {
            WithNode with = new WithNode();
            Consume();

            if (CurrentToken.Type == TokenType.Identifier)
            {
                with.RecordName = CurrentToken.Value;
                Consume();
            }
            else
                ReportParserError(218);

            if (CurrentToken.Type == TokenType.Do)
                Consume();
            else
                ReportParserError(219);

            ASTNode stmt = ParseStatement();
            if (stmt != null)
                with.Statements.Add(stmt);

            return with;
        }

        private ExpressionNode ParseExpression()
        {
            return ParseSimpleExpression();
        }

        private ExpressionNode ParseSimpleExpression()
        {
            ExpressionNode left = ParseTerm();

            while (CurrentToken.Type == TokenType.Plus || CurrentToken.Type == TokenType.Minus)
            {
                string op = CurrentToken.Value;
                Consume();
                ExpressionNode right = ParseTerm();
                left = new BinaryOperationNode { Left = left, Operator = op, Right = right };
            }

            return left;
        }

        private ExpressionNode ParseTerm()
        {
            ExpressionNode left = ParseFactor();

            while (CurrentToken.Type == TokenType.Multiply || CurrentToken.Type == TokenType.Divide)
            {
                string op = CurrentToken.Value;
                Consume();
                ExpressionNode right = ParseFactor();
                left = new BinaryOperationNode { Left = left, Operator = op, Right = right };
            }

            return left;
        }

        private ExpressionNode ParseFactor()
        {
            if (CurrentToken.Type == TokenType.IntegerNumber)
            {
                string intPart = CurrentToken.Value;
                Consume();

                if (CurrentToken.Type == TokenType.Dot && _tokenIndex + 1 < _tokens.Count &&
                    _tokens[_tokenIndex + 1].Type == TokenType.IntegerNumber)
                {
                    Consume();
                    string fracPart = CurrentToken.Value;
                    Consume();
                    string fullNumber = intPart + "." + fracPart;
                    double value = double.Parse(fullNumber, System.Globalization.CultureInfo.InvariantCulture);
                    return new RealConstantNode { Value = value };
                }
                else
                    return new IntegerConstantNode { Value = int.Parse(intPart) };
            }
            else if (CurrentToken.Type == TokenType.RealNumber)
            {
                double value = double.Parse(CurrentToken.Value, System.Globalization.CultureInfo.InvariantCulture);
                Consume();
                return new RealConstantNode { Value = value };
            }
            else if (CurrentToken.Type == TokenType.Identifier)
            {
                string name = CurrentToken.Value;
                Consume();

                if (CurrentToken.Type == TokenType.LeftBracket)
                    return ParseIndexedVariable(name);
                else if (CurrentToken.Type == TokenType.Dot)
                    return ParseFieldAccess(name);
                else
                    return new VariableNode { Name = name };
            }
            else if (CurrentToken.Type == TokenType.LeftParen)
            {
                Consume();
                ExpressionNode expr = ParseExpression();
                if (CurrentToken.Type == TokenType.RightParen)
                    Consume();
                else
                    ReportParserError(220);
                return expr;
            }
            else
            {
                ReportParserError(221);
                Consume();
                return null;
            }
        }

        private IndexedVariableNode ParseIndexedVariable(string arrayName)
        {
            IndexedVariableNode indexed = new IndexedVariableNode();
            indexed.ArrayName = arrayName;

            Consume();
            indexed.Index = ParseExpression();

            if (CurrentToken.Type == TokenType.RightBracket)
                Consume();
            else
                ReportParserError(212);

            return indexed;
        }

        private FieldAccessNode ParseFieldAccess(string recordName)
        {
            FieldAccessNode field = new FieldAccessNode();
            field.RecordName = recordName;
            Consume();

            if (CurrentToken.Type == TokenType.Identifier)
            {
                field.FieldName = CurrentToken.Value;
                Consume();
            }
            else
                ReportParserError(217);

            return field;
        }

        private Token CurrentToken => _tokenIndex < _tokens.Count ? _tokens[_tokenIndex] : null;

        private void Consume()
        {
            if (_tokenIndex < _tokens.Count)
                _tokenIndex++;
        }
    }
}
