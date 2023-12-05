using compilers.CodeAnalysis.Symbol;

namespace compilers.CodeAnalysis.Binding
{
    internal sealed class BoundForStatement : BoundStatement
    {


        public BoundForStatement(VariableSymbol variable, BoundExpression lowerBound, BoundExpression upperBound, BoundStatement body,bool reverse)
        {
            Variable = variable;
            LowerBound = lowerBound;
            UpperBound = upperBound;
            Body = body;
            Reverse = reverse;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ForStatement;

        public VariableSymbol Variable { get; }
        public BoundExpression LowerBound { get; }
        public BoundExpression UpperBound { get; }
        public BoundStatement Body { get; }
        public bool Reverse { get; }
    }
}