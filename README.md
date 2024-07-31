# Imperative Language Compile

## Formal language grammar:

- Main program:

    ```properties
    { SimpleDeclaration | RoutineDeclaration }
    ```

- Simple Declaration:

    ```properties
    VariableDeclaration | TypeDeclaration
    ```

- TypeDeclaration:

    ```properties
    type Identifier is Type
    ```

- Routine declaration:

    ```properties
    routine Identifier ( Parameters ) [ : Type ] is
        Body
    end
    ```

- Parameters:

    ```properties
    ParameterDeclaration { , ParameterDeclaration }
    ```

- ParameterDeclaration:

    ```properties
    Identifier : Identifier
    ```

- Type:

    ```properties
    PrimitiveType | ArrayType | RecordType | Identifier
    ```

- PrimitiveType:

    ```properties
    integer | real | boolean
    ```

- RecordType:

    ```properties
    record { VariableDeclaration } end
    ```

- ArrayType:

    ```properties
    array [ Expression ] Type
    ```

- Body:

    ```properties
    { SimpleDeclaration | Statement }
    ```

- Statement:

    ```properties
    Assignment | RoutineCall
    | WhileLoop | ForLoop | IfStatement
    ```

- Assignment:

    ```properties
    ModifiablePrimary := Expression
    ```

- RoutineCall:

    ```properties
    Identifier [ ( Expression { , Expression } ) ]
    ```

- WhileLoop

    ```properties
    while Expression loop Body end
    ```

- ForLoop

    ```properties
    for Identifier Range loop Body end
    ```

- Range

    ```properties
    in [ reverse ] Expression .. Expression
    ```

- IfStatement:

    ```properties
    if Expression then Body [ else Body ] end
    ```

- Expression:

    ```properties
    Relation { ( and | or | xor ) Relation }
    ```

- Relation:

    ```properties
    Simple [ ( < | <= | > | >= | = | /= ) Simple ]
    ```

- Simple:

    ```properties
    Factor { ( * | / | % ) Factor }
    ```

- Factor:

    ```properties
    Summand { ( + | - ) Summand }
    ```

- Summand:

    ```properties
    Primary | ( Expression )
    ```

- Primary:

    ```properties
    IntegralLiteral | RealLiteral | true | false
    ```
