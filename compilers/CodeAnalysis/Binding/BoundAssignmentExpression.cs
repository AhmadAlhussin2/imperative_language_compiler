using compilers.CodeAnalysis.Symbol;

namespace compilers.CodeAnalysis.Binding
{
    internal sealed class BoundAssignmentExpression : BoundExpression
    {

        public BoundAssignmentExpression(VariableSymbol variable, BoundExpression boundExpression, BoundVariableExpression? exactVar = null)
        {
            Variable = variable;
            ExactVar = exactVar;
            Expression = boundExpression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
        public override TypeSymbol Type => Expression.Type;
        public VariableSymbol Variable { get; }
        public BoundVariableExpression? ExactVar { get; }
        public BoundExpression Expression { get; }
    }
}