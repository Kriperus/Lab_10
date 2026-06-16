using System;
using System.Collections.Generic;

namespace PascalCompiler
{
    public class SemanticAnalyzer
    {
        private readonly ErrorHandler _errors;
        private readonly Dictionary<string, TypeNode> _symbolTable = new Dictionary<string, TypeNode>();

        public SemanticAnalyzer(ErrorHandler errors)
        {
            _errors = errors;
        }

        public void Analyze(ProgramNode program)
        {
            foreach (var decl in program.Declarations)
            {
                if (decl is VarDeclarationNode varDecl)
                {
                    foreach (string name in varDecl.Identifiers)
                        _symbolTable[name] = varDecl.TypeNode;
                }
            }

            if (program.Body != null)
                CheckStatements(program.Body.Statements);
        }

        private void CheckStatements(List<ASTNode> statements)
        {
            foreach (var stmt in statements)
            {
                if (stmt is AssignmentNode assign)
                    CheckAssignment(assign);
                else if (stmt is WithNode with)
                    CheckWithStatement(with);
                else if (stmt is CompoundStatementNode compound)
                    CheckStatements(compound.Statements);
            }
        }

        private void CheckAssignment(AssignmentNode assign)
        {
            if (assign.Target is VariableNode varNode && !_symbolTable.ContainsKey(varNode.Name))
                _errors.ReportError(300, $"Переменная '{varNode.Name}' не объявлена", assign.Target.LineNumber, assign.Target.CharPosition);
        }

        private void CheckWithStatement(WithNode with)
        {
            if (!_symbolTable.ContainsKey(with.RecordName))
                _errors.ReportError(301, $"Запись '{with.RecordName}' не объявлена", with.LineNumber, with.CharPosition);
        }
    }
}
