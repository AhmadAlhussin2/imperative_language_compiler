using System.Collections.Immutable;

namespace compilers.CodeAnalysis
{
    public sealed class BlockStatementSyntax : StatementSyntax
    {
        public BlockStatementSyntax(SyntaxTree syntaxTree, ImmutableArray<StatementSyntax> statements)
        : base(syntaxTree)
        {
            //StartToken = startToken;
            Statements = statements;
        }

        //public SyntaxToken StartToken { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }

        public override SyntaxKind Kind => SyntaxKind.BlockStatement;
    }
}