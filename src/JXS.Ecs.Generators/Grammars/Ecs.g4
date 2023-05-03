grammar Ecs;

tokens { INDENT, DEDENT }

@lexer::header {
    #nullable enable
    using Ecs.Generators.Utils;
}

@lexer::members {
    private DenterHelper denter;
      
    public override IToken NextToken()
    {
        if (denter == null)
        {
            denter = DenterHelper.Builder()
                .Nl(NEWLINE)
                .Indent(EcsParser.INDENT)
                .Dedent(EcsParser.DEDENT)
                .PullToken(base.NextToken);
        }
    
        return denter.NextToken();
    }
}

program: namespace? components systems world EOF;

namespace: NAMESPACE NamespaceIdentifier NEWLINE;

components: (NEWLINE | component)*;
component: SINGLETON? COMPONENT Identifier NEWLINE;

systems: (NEWLINE | system)*;
system: ORDERED? SYSTEM Identifier COLON NEWLINE INDENT processParam aspectParam DEDENT;

processParam: PROCESS COLON ProcessPass NEWLINE;

aspectParam: EXTERNAL? ASPECT COLON NEWLINE INDENT aspectComponent+ DEDENT;
aspectComponent: transientComponent | nonTransientComponent;
transientComponent: TRANSIENT Identifier NEWLINE;
nonTransientComponent: OPTIONAL? EXTERNAL? READONLY? Identifier NEWLINE;

world: WORLD Identifier? COLON NEWLINE INDENT worldBody DEDENT;
worldBody:
    PreWildcard=worldSystemDeclarations WILDCARD NEWLINE PostWildcard=worldSystemDeclarations |
    PreWildcard=worldSystemDeclarations WILDCARD NEWLINE |
    WILDCARD NEWLINE PostWildcard=worldSystemDeclarations |
    WILDCARD NEWLINE?
;
worldSystemDeclarations: worldSystemDeclaration+;
worldSystemDeclaration: (worldSystem | worldSystemList) NEWLINE;
worldSystem: Identifier;
worldSystemList: LIST_START worldSystemChain LIST_END;
worldSystemChain: worldSystem (LIST_SEPARATOR worldSystemChain)?;

// Tokens
COLON: ':';
DOT: '.';
WILDCARD: '*';
LIST_START: '[';
LIST_END: ']';
LIST_SEPARATOR: ',';

// Keywords
NAMESPACE: 'namespace';
COMPONENT: 'component';
SYSTEM: 'system';
WORLD: 'world';

PROCESS: 'process';
ASPECT: 'aspect';

ORDERED: 'ordered';
OPTIONAL: 'optional';
EXTERNAL: 'external';
READONLY: 'readonly';
SINGLETON: 'singleton';
TRANSIENT: 'transient';

// Compounds
ProcessPass: 'Update' | 'FixedUpdate' | 'Draw';
Identifier: [a-zA-Z_][a-zA-Z_0-9]*;
NamespaceIdentifier: Identifier (DOT NamespaceIdentifier)?;

NEWLINE: ('\r'? '\n' | 'r' | '\f') (' ' | '\t')*;

// Skips
COMMENT: ('//' ~('\n'|'\r')* '\r'? '\n') -> skip;