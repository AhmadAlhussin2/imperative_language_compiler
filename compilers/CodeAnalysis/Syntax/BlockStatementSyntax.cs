using System.Collections.Immutable;

namespace compilers.CodeAnalysis
{
    public sealed class BlockStatementSyntax : StatementSyntax
    {
        public BlockStatementSyntax(SyntaxTree syntaxTree, ImmutableArray<StatementSyntax> statements)
        : base(syntaxTree)
        {
            Statements = statements;
        }

        public ImmutableArray<StatementSyntax> Statements { get; }

        public override SyntaxKind Kind => SyntaxKind.BlockStatement;
    }
}