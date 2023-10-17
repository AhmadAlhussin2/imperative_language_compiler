using System.Collections;
using System.Collections.Generic;    

namespace compilers.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        
        private readonly List<Diagnostic> _diagnostics = new();
        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
        }

        private void Report (TextSpan span, string message)
        {
            var diagnostic = new Diagnostic(span, message);
            _diagnostics.Add(diagnostic);
        }

        public void ReportInvalidNumber(TextSpan span, string text, Type type)
        {
            var message = $"The number {text} is not a valid {type}.";
            Report(span, message);
        }

        public void ReportBadCharacter(int position, char character)
        {
            var message = $"Bad character input: '{character}'.";
            var span = new TextSpan(position, 1);
            Report(span, message);
        }

        public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
        {
            var message = $"Unexprected token <{actualKind}>, expected <{expectedKind}>.";
            Report(span, message);
        }

        public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, Type operandType)
        {
            var message = $"Unary Operator '{operatorText}' is not definted for type {operandType}";
            Report(span, message);
        }

        internal void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, Type type1, Type type2)
        {
            var message = $"Binary operator '{operatorText}' is not defined for types {type1} and {type2}";
            Report(span, message); 
        }
    }
}