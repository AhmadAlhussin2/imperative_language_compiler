using compilers.IO;

namespace compilers.CodeAnalysis.Symbol
{
    internal static class SymbolPrinter
    {
        public static void WriteTo(Symbol symbol, TextWriter writer)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Function:
                    WriteFunction((FunctionSymbol)symbol, writer);
                    break;
                case SymbolKind.GlobalVariable:
                    WriteGlobalVariable((FunctionSymbol)symbol, writer);
                    break;
                case SymbolKind.LocalVariable:
                    WriteLocalVariable((FunctionSymbol)symbol, writer);
                    break;
                case SymbolKind.Parameter:
                    WriteParameter((FunctionSymbol)symbol, writer);
                    break;
                case SymbolKind.Type:
                    WriteType((FunctionSymbol)symbol, writer);
                    break;
                default:
                    throw new Exception($"Unexpected symbol {symbol.Kind}");
            }
        }

        private static void WriteFunction(FunctionSymbol symbol, TextWriter writer)
        {
            writer.WriteKeyword("function ");
            writer.WriteIdentifier(symbol.Name);
            writer.WritePunctuation("(");
            for (int i = 0; i < symbol.Parameters.Length; i++)
            {
                if (i > 0)
                    writer.WritePunctuation(", ");
                symbol.Parameters[i].WriteTo(writer);
            }
            writer.WritePunctuation(")");
            writer.WriteLine();
        }

        private static void WriteGlobalVariable(FunctionSymbol symbol, TextWriter writer)
        {
            writer.WriteKeyword("var ");
            writer.WriteIdentifier(symbol.Name);
            writer.WriteKeyword(": ");
            symbol.Type.WriteTo(writer);
        }

        private static void WriteLocalVariable(FunctionSymbol symbol, TextWriter writer)
        {
            writer.WriteKeyword("var ");
            writer.WriteIdentifier(symbol.Name);
            writer.WriteKeyword(": ");
            symbol.Type.WriteTo(writer);
        }

        private static void WriteParameter(FunctionSymbol symbol, TextWriter writer)
        {
            writer.WriteIdentifier(symbol.Name);
            writer.WriteKeyword(": ");
            symbol.Type.WriteTo(writer);
        }

        private static void WriteType(FunctionSymbol symbol, TextWriter writer)
        {
            writer.WriteIdentifier(symbol.Name);
        }
    }
}