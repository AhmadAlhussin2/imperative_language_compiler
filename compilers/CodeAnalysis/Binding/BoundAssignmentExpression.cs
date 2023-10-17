namespace compilers.CodeAnalysis.Binding
{
    internal sealed class BoundAssignmentExpression : BoundExpression
    {

        public BoundAssignmentExpression(VariableSymbol variable, BoundExpression boundExpression)
        {
            Variable = variable;
            Expression = boundExpression;
        }

        public override Type Type => Expression.Type;
        public VariableSymbol Variable { get; }

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
        public BoundExpression Expression { get; }
    }
}