using System.Collections.Immutable;
namespace ImperativeCompiler.CodeAnalysis.Text;

public sealed class SourceText
{
    private readonly string _text;

    private SourceText(string text)
    {
        _text = text;
        Lines = ParseLines(this, text);
    }
    public char this[int index] => _text[index];
    public int Length => _text.Length;
    public int GetLineIndex(int position)
    {
        var lo = 0;
        var hi = Lines.Length - 1;
        while (lo <= hi)
        {
            var mid = lo + (hi - lo) / 2;
            var start = Lines[mid].Start;
            if (start == position)
            {
                return mid;
            }
            if (start > position)
            {
                hi = mid - 1;
            }
            else
            {
                lo = mid + 1;
            }
        }
        return lo - 1;
    }
    public ImmutableArray<TextLine> Lines { get; }
    private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
    {
        var result = ImmutableArray.CreateBuilder<TextLine>();
        var lineStart = 0;
        var position = 0;
        while (position < text.Length)
        {
            var lineBreakWidth = GetLineBreakWidth(text, position);
            if (lineBreakWidth == 0)
            {
                position++;
            }
            else
            {
                AddLine(result, sourceText, position, lineStart, lineBreakWidth);

                position += lineBreakWidth;
                lineStart = position;
            }
        }
        if (position >= lineStart)
        {
            AddLine(result, sourceText, position, lineStart, 0);
        }
        return result.ToImmutable();
    }

    private static void AddLine(ImmutableArray<TextLine>.Builder result, SourceText sourceText, int position, int lineStart, int lineBreakWidth)
    {
        var lineLength = position - lineStart;
        var lineLengthIncludingLineBreaks = lineLength + lineBreakWidth;
        var line = new TextLine(sourceText, lineStart, lineLength, lineLengthIncludingLineBreaks);
        result.Add(line);
    }

    private static int GetLineBreakWidth(string text, int position)
    {
        var c = text[position];
        var l = position + 1 >= text.Length ? '\0' : text[position + 1];
        if (c == '\r' && l == '\n')
            return 2;
        if (c is '\r' or '\n')
            return 1;
        return 0;
    }

    public static SourceText From(string text)
    {
        return new SourceText(text);
    }
    public override string ToString()
    {
        return _text;
    }
    public string ToString(int start, int length) => _text.Substring(start, length);
    public string ToString(TextSpan span)
    {
        return ToString(span.Start, span.Length);

    }
}