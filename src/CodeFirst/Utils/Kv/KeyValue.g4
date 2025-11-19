grammar KeyValue;

tokens { INDENT, DEDENT }

@lexer::header {
#nullable enable
using CodeFirst.Utils.Kv;
// Console is not supported in generators
#pragma warning disable RS1035
}

@parser::header {
// Console is not supported in generators
#pragma warning disable RS1035
}

@lexer::members {
private DenterHelper denter;
  
public override IToken NextToken()
{
    if (denter == null)
    {
        denter = DenterHelper.Builder()
            .Nl(NEWLINE)
            .Indent(KeyValueParser.INDENT)
            .Dedent(KeyValueParser.DEDENT)
            .PullToken(base.NextToken);
    }

    return denter.NextToken();
}
}

program: definitions (NEWLINE | kv)* EOF;

definitions: (NEWLINE | definition)*;
definition: DEFINITION_START Identifier definitionValue NEWLINE;
definitionValue: Identifier | String;
kv: Identifier COLON value NEWLINE;

value: primitiveValue | children | valueList | PASS;
primitiveValue: Number | Boolean | String;
valueList: LIST_START valueListChain? LIST_END;
valueListChain: primitiveValue (LIST_SEPARATOR valueListChain)?;
children: INDENT (NEWLINE | kv)* DEDENT;

// Tokens
COLON: ':';
DOT: '.';
QUOTE: '"';
LIST_START: '[';
LIST_END: ']';
LIST_SEPARATOR: ',';
DEFINITION_START: '#';

// Keywords
PASS: 'pass';

// Compounds
Identifier: [a-zA-Z_][a-zA-Z_0-9]*;
String: '"' ~["]* '"';
Number: DIGIT+ (DOT DIGIT+)?;
Boolean: 'true' | 'false';

NEWLINE: ('\r'? '\n' | 'r' | '\f') (' ' | '\t')*;

// Skips
COMMENT: ('//' ~('\n'|'\r')* '\r'? '\n') -> skip;

// Fragments
fragment DIGIT: [0-9];