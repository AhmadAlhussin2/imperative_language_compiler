namespace compilers.CodeAnalysis.Binding
{
    internal sealed class BoundGoToStatement : BoundStatement
    {
        public BoundGoToStatement(BoundLabel label)
        {
            Label = label;
        }

        public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;

        public BoundLabel Label { get; }
    }
}