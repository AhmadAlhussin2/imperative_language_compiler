using System.Collections;
using System.Collections.Generic;
using compilers.CodeAnalysis.Symbol;

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

        private void Report(TextSpan span, string message)
        {
            var diagnostic = new Diagnostic(span, message);
            _diagnostics.Add(diagnostic);
        }

        public void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type)
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

        public void ReportUndefinedUnaryOperator(TextSpan span, string operatorText, TypeSymbol operandType)
        {
            var message = $"Unary Operator '{operatorText}' is not definted for type {operandType}";
            Report(span, message);
        }

        public void ReportUndefinedName(TextSpan span, string name)
        {
            var message = $"Var '{name}' is not definted";
            Report(span, message);
        }

        internal void ReportUndefinedBinaryOperator(TextSpan span, string operatorText, TypeSymbol type1, TypeSymbol? type2)
        {
            var message = $"Binary operator '{operatorText}' is not defined for types {type1} and {type2}";
            Report(span, message);
        }

        internal void ReportCannotConvert(TextSpan span, TypeSymbol type1, TypeSymbol type2)
        {
            var message = $"Cannot convert type '{type1}' to '{type2}'.";
            Report(span, message);
        }

        internal void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            var message = $"Variable {name} already declared.";
            Report(span, message);
        }

        public void ReportUndefinedFunction(TextSpan span, string name)
        {
            var message = $"Function {name} doesn't exist.";
            Report(span, message);
            
        }

        internal void ReportWrongArgumentCount(TextSpan span,string name, int length, int count)
        {
            var message = $"Function '{name}' requires {length} arguments but was given {count}.";
            Report(span, message);
        }

        internal void ReportWrongArgumentType(TextSpan span, string name, string parameterName, TypeSymbol parameterType, TypeSymbol argumentType)
        {
            var message = $"Function '{name}': Prameter '{parameterName}' requires a value of type '{parameterType}' but was given a value of type'{argumentType}'.";
            Report(span, message);
        }
    }
}