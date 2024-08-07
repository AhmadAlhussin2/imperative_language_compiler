namespace ImperativeCompiler.CodeAnalysis.Binding;

internal enum BoundNodeKind
{
    LiteralExpression,
    UnaryExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression,
    BlockStatement,
    ExpressionStatement,
    VariableDeclaration,
    IfStatement,
    WhileStatement,
    ForStatement,
    GoToStatement,
    LabelStatement,
    ConditionalGotoStatement,
    ErrorExpression,
    CallExpression,
    ConversionExpression,
    ReturnStatement,
    BoundType
}