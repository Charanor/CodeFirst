grammar Ecs;

tokens { INDENT, DEDENT }

@lexer::header {
    #nullable enable
    using CodeFirst.Generators.Ecs.Utils;
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

program: NEWLINE* namespace? components systems NEWLINE* world NEWLINE* EOF;

namespace: NAMESPACE NamespaceIdentifier NEWLINE;

components: (NEWLINE | component)*;
component: SINGLETON? COMPONENT Identifier NEWLINE;

systems: (NEWLINE | system)*;
system: ORDERED? ASYNC? SYSTEM Identifier COLON NEWLINE INDENT processParam aspectParam DEDENT;

processParam: PROCESS COLON ProcessPass NEWLINE;

aspectParam: EXTERNAL? ASPECT COLON NEWLINE INDENT tagComponent* aspectComponent+ excludedComponent* DEDENT;
tagComponent: TAG Identifier NEWLINE;
aspectComponent: OPTIONAL? EXTERNAL? READONLY? Identifier NEWLINE;
excludedComponent: EXCLUDE Identifier NEWLINE;

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
TAG: 'tag';
EXCLUDE: 'exclude';
ASYNC: 'async';

// Compounds
ProcessPass: 'Update' | 'FixedUpdate' | 'Draw';
Identifier: [a-zA-Z_][a-zA-Z_0-9]*;
NamespaceIdentifier: Identifier (DOT NamespaceIdentifier)?;

NEWLINE: ('\r'? '\n' | 'r' | '\f') (' ' | '\t')*;

// Skips
COMMENT: ('//' ~('\n'|'\r')* '\r'? '\n') -> skip;