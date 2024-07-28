
using System.CodeDom.Compiler;
using compilers.CodeAnalysis.IO;
using compilers.CodeAnalysis.Symbols;
using compilers.CodeAnalysis.Syntax;

namespace compilers.CodeAnalysis.Binding
{
    internal static class BoundNodePrinter
    {
        public static void WriteTo(this BoundNode node, TextWriter writer)
        {
            if (writer is IndentedTextWriter iw)
                WriteTo(node, iw);
            else
                WriteTo(node, new IndentedTextWriter(writer));
        }
        public static void WriteTo(this BoundNode node, IndentedTextWriter writer)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.LiteralExpression:
                    WriteLiteralExpression((BoundLiteralExpression)node, writer);
                    break;
                case BoundNodeKind.UnaryExpression:
                    WriteUnaryExpression((BoundUnaryExpression)node, writer);
                    break;
                case BoundNodeKind.BinaryExpression:
                    WriteBinaryExpression((BoundBinaryExpression)node, writer);
                    break;
                case BoundNodeKind.VariableExpression:
                    WriteVariableExpression((BoundVariableExpression)node, writer);
                    break;
                case BoundNodeKind.AssignmentExpression:
                    WriteAssignmentExpression((BoundAssignmentExpression)node, writer);
                    break;
                case BoundNodeKind.BlockStatement:
                    WriteBlockStatement((BoundBlockStatement)node, writer);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    WriteExpressionStatement((BoundExpressionStatement)node, writer);
                    break;
                case BoundNodeKind.VariableDeclaration:
                    WriteVariableDeclaration((BoundVariableDeclaration)node, writer);
                    break;
                case BoundNodeKind.IfStatement:
                    WriteIfStatement((BoundIfStatement)node, writer);
                    break;
                case BoundNodeKind.WhileStatement:
                    WriteWhileStatement((BoundWhileStatement)node, writer);
                    break;
                case BoundNodeKind.ForStatement:
                    WriteForStatement((BoundForStatement)node, writer);
                    break;
                case BoundNodeKind.GoToStatement:
                    WriteGotoStatement((BoundGoToStatement)node, writer);
                    break;
                case BoundNodeKind.LabelStatement:
                    WriteLabelStatement((BoundLabelStatement)node, writer);
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    WriteConditionalGotoStatement((BoundConditionalGotoStatement)node, writer);
                    break;
                case BoundNodeKind.ErrorExpression:
                    WriteErrorExpression((BoundErrorExpression)node, writer);
                    break;
                case BoundNodeKind.CallExpression:
                    WriteCallExpression((BoundCallExpression)node, writer);
                    break;
                case BoundNodeKind.ConversionExpression:
                    WriteConversionExpression((BoundConversionExpression)node, writer);
                    break;
                case BoundNodeKind.ReturnStatement:
                    WriteReturnStatement((BoundReturnStatement)node, writer);
                    break;
                default:
                    writer.WriteLine("routine func");
                    break;
            }
        }
        private static void WriterNestedStatement(this IndentedTextWriter writer, BoundStatement node)
        {
            var needIndent = node is not BoundBlockStatement;
            if (needIndent)
                writer.Indent++;
            node.WriteTo(writer);
            if (needIndent)
                writer.Indent--;
        }
        private static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence, BoundExpression expression)
        {
            if (expression is BoundUnaryExpression unary)
                writer.WriteNestedExpression(parentPrecedence, OperatorsPriority.GetUnaryOperatorPriority(unary.Op.SyntaxKind), unary);
            else if (expression is BoundBinaryExpression binary)
                writer.WriteNestedExpression(parentPrecedence, OperatorsPriority.GetBinaryOperatorPriority(binary.Op.SyntaxKind), binary);
            else
                expression.WriteTo(writer);
        }
        private static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence, int currentPrecedence, BoundExpression expression)
        {
            var needParenthesis = parentPrecedence >= currentPrecedence;
            if (needParenthesis)
                writer.WritePunctuation("(");
            expression.WriteTo(writer);
            if (needParenthesis)
                writer.WritePunctuation(")");
        }

        private static void WriteLiteralExpression(BoundLiteralExpression node, IndentedTextWriter writer)
        {
            if (node.Type == TypeSymbol.Bool)
            {
                writer.WriteKeyword(node.Value.ToString()!);
            }
            else if (node.Type == TypeSymbol.Int || node.Type == TypeSymbol.Real)
            {
                writer.WriteNumber(node.Value.ToString()!);
            }
            else
            {
                throw new Exception($"Unexpected type {node.Type}");
            }
        }

        private static void WriteUnaryExpression(BoundUnaryExpression node, IndentedTextWriter writer)
        {
            writer.WritePunctuation(OperatorsPriority.GetText(node.Op.SyntaxKind)!);
            var precedence = OperatorsPriority.GetUnaryOperatorPriority(node.Op.SyntaxKind);
            writer.WriteNestedExpression(precedence, node.Operand);
        }

        private static void WriteBinaryExpression(BoundBinaryExpression node, IndentedTextWriter writer)
        {
            var precedence = OperatorsPriority.GetBinaryOperatorPriority(node.Op.SyntaxKind);
            writer.WriteNestedExpression(precedence, node.Left);
            writer.WritePunctuation(OperatorsPriority.GetText(node.Op.SyntaxKind)!);
            writer.WriteNestedExpression(precedence, node.Right);
        }

        private static void WriteVariableExpression(BoundVariableExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Variable.Name);
            if (node.Indicies != null)
            {
                foreach (var expr in node.Indicies)
                {
                    writer.WritePunctuation(" [ ");
                    expr.WriteTo(writer);
                    writer.WritePunctuation(" ] ");
                }
            }
        }

        private static void WriteAssignmentExpression(BoundAssignmentExpression node, IndentedTextWriter writer)
        {

            if (node.ExactVar != null)
            {
                WriteVariableExpression(node.ExactVar, writer);
            }
            else writer.Write(node.Variable.Name);
            writer.WritePunctuation(" := ");
            node.Expression.WriteTo(writer);
        }

        private static void WriteBlockStatement(BoundBlockStatement node, IndentedTextWriter writer)
        {
            writer.WriteLine();
            writer.Indent++;
            foreach (var s in node.Statements)
            {
                s.WriteTo(writer);
            }
            writer.Indent--;
            writer.WriteLine();
        }

        private static void WriteExpressionStatement(BoundExpressionStatement node, IndentedTextWriter writer)
        {
            node.Expression.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteVariableDeclaration(BoundVariableDeclaration node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("var ");
            writer.WriteIdentifier(node.Variable.Name);
            if (node.Type != null)
            {
                writer.WritePunctuation(" : ");
                writer.WriteIdentifier(node.Type.ToString());
            }
            writer.WriteKeyword(" is ");
            node.Initializer.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteIfStatement(BoundIfStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("if ");
            node.Condition.WriteTo(writer);
            writer.WriteKeyword(" then ");
            writer.WriteLine();
            writer.WriterNestedStatement(node.ThenStatement);
            if (node.ElseStatement != null)
            {
                writer.WriteKeyword("else ");
                writer.WriteLine();
                writer.WriterNestedStatement(node.ElseStatement);
            }
            writer.WriteKeyword("end");
            writer.WriteLine();
        }

        private static void WriteWhileStatement(BoundWhileStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("while ");
            node.Condition.WriteTo(writer);
            writer.WriteKeyword(" loop");
            writer.WriteLine();
            writer.WriterNestedStatement(node.Body);
            writer.WriteKeyword(" end");
            writer.WriteLine();
        }

        private static void WriteForStatement(BoundForStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("for ");
            writer.WriteIdentifier(node.Variable.Name);
            writer.WritePunctuation(" = ");
            node.LowerBound.WriteTo(writer);
            writer.WriteKeyword(" to ");
            node.UpperBound.WriteTo(writer);
            writer.WriteKeyword(" loop");
            writer.WriteLine();
            writer.WriterNestedStatement(node.Body);
            writer.WriteKeyword(" end");
            writer.WriteLine();
        }

        private static void WriteGotoStatement(BoundGoToStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("goto ");
            writer.WriteIdentifier(node.Label.Name);
            writer.WriteLine();
        }

        private static void WriteLabelStatement(BoundLabelStatement node, IndentedTextWriter writer)
        {
            var unindent = writer.Indent > 0;
            if (unindent)
                writer.Indent--;
            writer.WritePunctuation(node.Label.Name);
            writer.WritePunctuation(":");
            writer.WriteLine();
            if (unindent)
                writer.Indent++;
        }

        private static void WriteConditionalGotoStatement(BoundConditionalGotoStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("goto ");
            writer.WriteIdentifier(node.Label.Name);
            writer.WriteKeyword(node.JumpIfTrue ? " if " : " unless ");
            node.Condition.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteErrorExpression(BoundErrorExpression _, IndentedTextWriter writer)
        {
            writer.WriteKeyword("?");
        }

        private static void WriteCallExpression(BoundCallExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Function.Name);
            writer.WritePunctuation("(");
            var isFirst = true;
            foreach (var arg in node.Arguments)
            {
                if (isFirst)
                    isFirst = false;
                else
                    writer.WritePunctuation(", ");
                arg.WriteTo(writer);
            }
            writer.WritePunctuation(")");
        }

        private static void WriteConversionExpression(BoundConversionExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Type.Name);
            writer.WritePunctuation("(");
            node.Expression.WriteTo(writer);
            writer.WritePunctuation(")");
        }

        private static void WriteReturnStatement(BoundReturnStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("return ");
            node.Expression.WriteTo(writer);
            writer.WriteLine();
        }
    }
}