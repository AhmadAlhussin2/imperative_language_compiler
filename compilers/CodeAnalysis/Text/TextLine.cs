namespace compilers.CodeAnalysis.Text
{
    public sealed class TextLine
    {
        public TextLine(SourceText text, int start, int length, int lenghtWithLineBreaks)
        {
            LenghtWithLineBreaks = lenghtWithLineBreaks;
            Length = length;
            Text = text;
            Start = start;

        }
        public SourceText Text { get; }
        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;
        public int LenghtWithLineBreaks { get; }
        public TextSpan Span => new TextSpan(Start, Length);
        public TextSpan SpanIncludingLineBreaks => new TextSpan(Start, LenghtWithLineBreaks);
        public override string ToString() => Text.ToString(Span);
    }
}