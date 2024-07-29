namespace ImperativeCompiler.CodeAnalysis.Syntax;

public enum SyntaxKind
{
    NumberToken,
    RealNumberToken,
    WhiteSpace,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    OpenParenthesisToken,
    CloseParenthesisToken,
    UnknownToken,
    EofToken,
    LessThanOrEqualToken,
    LessThanToken,
    GreaterThanOrEqualToken,
    GreaterThanToken,
    NotEqualToken,
    NegationToken,
    ModuloToken,
    AssignmentToken,
    ColonToken,
    EqualToken,
    IdentifierToken,
    AndKeyword,
    OrKeyword,
    XorKeyword,
    IfKeyword,
    IsKeyword,
    IntegerKeyword,
    RealKeyword,
    VarKeyword,
    TypeKeyword,
    BooleanKeyword,
    TrueKeyword,
    FalseKeyword,
    RecordKeyword,
    EndKeyword,
    ArrayKeyword,
    WhileKeyword,
    LoopKeyword,
    ForKeyword,
    ReverseKeyword,
    DotToken,
    CommaToken,
    RangeToken,
    ThenKeyword,
    ElseKeyword,
    RoutineKeyword,
    ReturnKeyword,
    NotKeyword,
    OpenSquareBracketToken,
    CloseSquareBracketToken,


    //Expressions
    LiteralExpression,
    BinaryExpression,
    NameExpression,
    ParenthesizedExpression,
    UnaryExpression,
    AssignmentExpression,
    CompilationUnit,
    BlockStatement,
    ExpressionStatement,
    VariableDeclaration,
    IfStatement,
    ElseClause,
    WhileStatement,
    ForStatement,
    InKeyword,
    CallExpression,
    GlobalStatement,
    Parameter,
    TypeStatement,
    RecordDeclaration,
    ReturnStatement,
    PrimitiveType,
    ArrayType,
    Variable,
    FunctionDecleration
}