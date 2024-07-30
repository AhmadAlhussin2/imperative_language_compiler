using ImperativeCompiler.CodeAnalysis.Syntax;

namespace ImperativeCompiler.Tests;

internal sealed class AssertingEnumerator : IDisposable
{
    private readonly IEnumerator<SyntaxNode> _enumerator;
    private bool _hasErrors;

    public AssertingEnumerator(SyntaxNode node)
    {
        _enumerator = Flatten(node).GetEnumerator();
        _hasErrors = false;
    }

    private static IEnumerable<SyntaxNode> Flatten(SyntaxNode node)
    {
        var stack = new Stack<SyntaxNode>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            node = stack.Pop();

            yield return node;
            foreach (var child in node.GetChildren().Reverse())
            {
                stack.Push(child);
            }
        }
    }

    public void AssertToken(SyntaxKind kind, string text)
    {
        try
        {
            Assert.True(_enumerator.MoveNext());
            var token = Assert.IsType<SyntaxToken>(_enumerator.Current);

            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }
        catch (Exception)
        {
            _hasErrors = true;
            throw;
        }
    }

    public void AssertNode(SyntaxKind kind)
    {
        try
        {
            Assert.True(_enumerator.MoveNext());
            // Assert.IsNotType<SyntaxToken>(_enumerator.Current);
            Assert.Equal(kind, _enumerator.Current.Kind);
        }
        catch (Exception)
        {
            _hasErrors = true;
            throw;
        }
    }

    public void Dispose()
    {
        if (!_hasErrors)
        {
            Assert.False(_enumerator.MoveNext());
        }
        _enumerator.Dispose();
    }
}