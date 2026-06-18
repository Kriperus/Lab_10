using System.Collections.Generic;

namespace PascalCompiler
{
    public abstract class ASTNode
    {
        public int LineNumber { get; set; }
        public int CharPosition { get; set; }
    }

    public class ProgramNode : ASTNode
    {
        public string Name { get; set; }
        public List<ASTNode> Declarations { get; set; } = new List<ASTNode>();
        public CompoundStatementNode Body { get; set; }
    }

    public class ConstDeclarationNode : ASTNode
    {
        public string Name { get; set; }
        public ExpressionNode Value { get; set; }
    }

    public class VarDeclarationNode : ASTNode
    {
        public List<string> Identifiers { get; set; } = new List<string>();
        public TypeNode TypeNode { get; set; }
    }

    public abstract class TypeNode : ASTNode { }

    public class StandardTypeNode : TypeNode
    {
        public string TypeName { get; set; }
    }

    public class RecordTypeNode : TypeNode
    {
        public List<VarDeclarationNode> Fields { get; set; } = new List<VarDeclarationNode>();
    }

    public class ArrayTypeNode : TypeNode
    {
        public ExpressionNode LowerBound { get; set; }
        public ExpressionNode UpperBound { get; set; }
        public TypeNode ElementType { get; set; }
    }

    public abstract class ExpressionNode : ASTNode { }

    public class VariableNode : ExpressionNode
    {
        public string Name { get; set; }
    }

    public class IndexedVariableNode : ExpressionNode
    {
        public string ArrayName { get; set; }
        public ExpressionNode Index { get; set; }
    }

    public class FieldAccessNode : ExpressionNode
    {
        public string RecordName { get; set; }
        public string FieldName { get; set; }
    }

    public class IntegerConstantNode : ExpressionNode
    {
        public int Value { get; set; }
    }

    public class RealConstantNode : ExpressionNode
    {
        public double Value { get; set; }
    }

    public class CharConstantNode : ExpressionNode
    {
        public char Value { get; set; }
    }

    public class StringConstantNode : ExpressionNode
    {
        public string Value { get; set; }
    }

    public class BinaryOperationNode : ExpressionNode
    {
        public ExpressionNode Left { get; set; }
        public string Operator { get; set; }
        public ExpressionNode Right { get; set; }
    }

    public class AssignmentNode : ASTNode
    {
        public ExpressionNode Target { get; set; }
        public ExpressionNode Value { get; set; }
    }

    public class WithNode : ASTNode
    {
        public string RecordName { get; set; }
        public List<ASTNode> Statements { get; set; } = new List<ASTNode>();
    }

    public class CompoundStatementNode : ASTNode
    {
        public List<ASTNode> Statements { get; set; } = new List<ASTNode>();
    }
}
