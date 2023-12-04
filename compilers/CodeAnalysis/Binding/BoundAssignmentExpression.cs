using compilers.CodeAnalysis.Symbol;

namespace compilers.CodeAnalysis.Binding
{
    internal sealed class BoundAssignmentExpression : BoundExpression
    {

        public BoundAssignmentExpression(BoundVariableExpression variable, BoundExpression boundExpression)
        {
            Variable = variable;
            Expression = boundExpression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
        public override TypeSymbol Type => Expression.Type;
        public BoundVariableExpression Variable { get; }

        public BoundExpression Expression { get; }
    }
}