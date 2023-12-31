namespace compilers.CodeAnalysis
{
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
        UnknowToken,
        EOFToken,
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
        VariableDecleration,
        IfStatement,
        ElseClause,
        WhileStatement,
        ForStatement,
        InKeyword,
        CallExpression,
        TypeClause,
        GlobalStatement,
        FunctionDecleration,
        Parameter,
        TypeStatement,
        RecordDecleration,
        ReturnStatement,
        BadToken,
        ArrayDeclaration,
        TypeSyntax,
        PrimativeType,
        ArrayType,
        Variable
    }
}