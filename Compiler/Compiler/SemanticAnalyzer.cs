using System.Collections.Generic;

namespace PascalCompiler
{
    public class SemanticAnalyzer
    {
        private readonly ErrorHandler _errors;

        private readonly Dictionary<string, TypeNode> _variables = new Dictionary<string, TypeNode>();
        private readonly Dictionary<string, ExpressionNode> _constants = new Dictionary<string, ExpressionNode>();
        private readonly Dictionary<string, TypeNode> _types = new Dictionary<string, TypeNode>();

        private string _currentWithRecordName = "";

        public SemanticAnalyzer(ErrorHandler errors)
        {
            _errors = errors;
        }

        public void DeclareType(string name, TypeNode type, int line, int pos)
        {
            if (_types.ContainsKey(name))
            {
                _errors.ReportError(100, $"Тип '{name}' уже объявлен", line, pos);
            }
            else
            {
                _types[name] = type;
            }
        }

        public TypeNode GetType(string name)
        {
            _types.TryGetValue(name, out TypeNode type);
            return type;
        }

        public void DeclareConstant(string name, ExpressionNode value, int line, int pos)
        {
            if (_constants.ContainsKey(name) || _variables.ContainsKey(name))
            {
                _errors.ReportError(100, $"Имя '{name}' уже используется", line, pos);
            }
            else
            {
                _constants[name] = value;
            }
        }

        public void DeclareVariable(string name, TypeNode type, int line, int pos)
        {
            if (_variables.ContainsKey(name) || _constants.ContainsKey(name))
            {
                _errors.ReportError(100, $"Переменная '{name}' уже объявлена", line, pos);
            }
            else
            {
                _variables[name] = type;
            }
        }

        public TypeNode GetVariableType(string name, int line, int pos)
        {
            if (_variables.TryGetValue(name, out TypeNode type))
                return type;

            _errors.ReportError(148, $"Переменная '{name}' не объявлена", line, pos);
            return null;
        }

        public void CheckAssignment(ExpressionNode target, ExpressionNode value, int line, int pos)
        {
            if (target is VariableNode varNode && _constants.ContainsKey(varNode.Name))
            {
                _errors.ReportError(100, $"Нельзя изменять константу '{varNode.Name}'", line, pos);
                return;
            }
            if (target is IndexedVariableNode idxNode && _constants.ContainsKey(idxNode.ArrayName))
            {
                _errors.ReportError(100, $"Нельзя изменять константу '{idxNode.ArrayName}'", line, pos);
                return;
            }
            if (target is FieldAccessNode fieldNode && _constants.ContainsKey(fieldNode.RecordName))
            {
                _errors.ReportError(100, $"Нельзя изменять константу '{fieldNode.RecordName}'", line, pos);
                return;
            }

            TypeNode targetType = GetTargetType(target, line, pos);
            if (targetType == null) return;

            TypeNode valueType = GetExpressionType(value, line, pos);
            if (valueType == null) return;

            if (!AreTypesCompatible(targetType, valueType))
            {
                _errors.ReportError(147, "Несоответствие типов", line, pos);
            }
        }

        private TypeNode GetTargetType(ExpressionNode target, int line, int pos)
        {
            if (target is VariableNode varNode)
            {
                return GetVariableType(varNode.Name, line, pos);
            }
            else if (target is IndexedVariableNode idxNode)
            {
                TypeNode arrayType = GetVariableType(idxNode.ArrayName, line, pos);
                if (arrayType == null) return null;

                if (arrayType is ArrayTypeNode arrayNode)
                {
                    return arrayNode.ElementType;
                }
                else
                {
                    _errors.ReportError(147, "Индексация возможна только для массивов", line, pos);
                    return null;
                }
            }
            else if (target is FieldAccessNode fieldNode)
            {
                TypeNode recordType = GetVariableType(fieldNode.RecordName, line, pos);
                if (recordType == null) return null;

                if (recordType is RecordTypeNode recordNode)
                {
                    foreach (var field in recordNode.Fields)
                    {
                        if (field.Identifiers.Contains(fieldNode.FieldName))
                        {
                            return field.TypeNode;
                        }
                    }
                    _errors.ReportError(149, $"Поле '{fieldNode.FieldName}' не существует в записи '{fieldNode.RecordName}'", line, pos);
                    return null;
                }
                else
                {
                    _errors.ReportError(147, "Обращение к полю возможно только для записей", line, pos);
                    return null;
                }
            }
            return null;
        }

        private TypeNode GetExpressionType(ExpressionNode expr, int line, int pos)
        {
            if (expr is IntegerConstantNode) return new StandardTypeNode { TypeName = "integer" };
            if (expr is RealConstantNode) return new StandardTypeNode { TypeName = "real" };
            if (expr is CharConstantNode) return new StandardTypeNode { TypeName = "char" };
            if (expr is StringConstantNode) return new StandardTypeNode { TypeName = "string" };

            if (expr is VariableNode varNode)
            {
                return GetVariableType(varNode.Name, line, pos);
            }
            if (expr is IndexedVariableNode idxNode)
            {
                TypeNode arrayType = GetVariableType(idxNode.ArrayName, line, pos);
                if (arrayType is ArrayTypeNode arrayNode)
                {
                    return arrayNode.ElementType;
                }
                return null;
            }
            if (expr is FieldAccessNode fieldNode)
            {
                TypeNode recordType = GetVariableType(fieldNode.RecordName, line, pos);
                if (recordType is RecordTypeNode recordNode)
                {
                    foreach (var field in recordNode.Fields)
                    {
                        if (field.Identifiers.Contains(fieldNode.FieldName))
                        {
                            return field.TypeNode;
                        }
                    }
                }
                return null;
            }

            if (expr is BinaryOperationNode binOp)
            {
                TypeNode leftType = GetExpressionType(binOp.Left, line, pos);
                TypeNode rightType = GetExpressionType(binOp.Right, line, pos);

                if (leftType != null && rightType != null && AreTypesCompatible(leftType, rightType))
                    return leftType;

                return null;
            }

            return null;
        }

        private bool AreTypesCompatible(TypeNode left, TypeNode right)
        {
            if (left == null || right == null) return false;

            string l = (left as StandardTypeNode)?.TypeName?.ToLower();
            string r = (right as StandardTypeNode)?.TypeName?.ToLower();

            if (l == "integer" && r == "integer") return true;
            if (l == "real" && (r == "integer" || r == "real")) return true;
            if (l == "char" && r == "char") return true;
            if (l == "string" && r == "string") return true;

            return false;
        }

        public void EnterWithContext(string recordName, int line, int pos)
        {
            if (!_variables.ContainsKey(recordName))
            {
                _errors.ReportError(148, $"Запись '{recordName}' не объявлена", line, pos);
                return;
            }
            _currentWithRecordName = recordName;
        }

        public void ExitWithContext()
        {
            _currentWithRecordName = "";
        }

        public void CheckVariableOrFieldInWith(string name, int line, int pos, bool isField = false)
        {
            if (_constants.ContainsKey(name))
            {
                return;
            }

            if (isField)
            {
                return;
            }


            if (!string.IsNullOrEmpty(_currentWithRecordName))
            {
                TypeNode recordType = GetVariableType(_currentWithRecordName, line, pos);
                if (recordType == null) return;

                RecordTypeNode recordNode = recordType as RecordTypeNode;
                if (recordNode == null)
                {
                    _errors.ReportError(148, $"Переменная '{_currentWithRecordName}' не является записью", line, pos);
                    return;
                }

                bool found = false;
                foreach (var field in recordNode.Fields)
                {
                    if (field.Identifiers.Contains(name))
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    return;
                }
                else
                {
                    if (_variables.ContainsKey(name))
                    {
                        return;
                    }
                    _errors.ReportError(148, $"Переменная или поле '{name}' не объявлены", line, pos);
                    return;
                }
            }

            if (!_variables.ContainsKey(name))
            {
                _errors.ReportError(148, $"Переменная '{name}' не объявлена", line, pos);
            }
        }
    }
}
