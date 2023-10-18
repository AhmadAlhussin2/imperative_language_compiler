
namespace compilers.CodeAnalysis
{
    public struct TextSpan
    {
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }
        public int Start { get; }
        public int Length { get; }

        public static TextSpan FromBound(int start, int end)
        {
            var length = end - start;
            return new TextSpan(start, length);
        }
    }
}