using System.Collections.Immutable;

namespace compilers.CodeAnalysis
{
    public sealed class BlockStatementSyntax : StatementSyntax
    {
        public BlockStatementSyntax(SyntaxToken startToken, ImmutableArray<StatementSyntax> statements, SyntaxToken endToken)
        {
            StartToken = startToken;
            Statements = statements;
            EndToken = endToken;
        }

        public SyntaxToken StartToken { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken EndToken { get; }

        public override SyntaxKind Kind => SyntaxKind.BlockStatement;
    }
}