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

program: namespace? components systems EOF;

namespace: NAMESPACE NamespaceIdentifier NEWLINE;

components: (NEWLINE | component)*;
component: SINGLETON? COMPONENT Identifier NEWLINE;

systems: (NEWLINE | system)*;
system: ORDERED? SYSTEM Identifier COLON NEWLINE INDENT processParam aspectParam DEDENT;

processParam: PROCESS COLON ProcessPass NEWLINE;

aspectParam: ASPECT COLON NEWLINE INDENT aspectComponent+ DEDENT;
aspectComponent: transientComponent | nonTransientComponent;
transientComponent: TRANSIENT Identifier NEWLINE;
nonTransientComponent: OPTIONAL? READONLY? Identifier NEWLINE;

// Tokens
COLON: ':';
DOT: '.';

// Keywords
NAMESPACE: 'namespace';
COMPONENT: 'component';
SYSTEM: 'system';

PROCESS: 'process';
ASPECT: 'aspect';

ORDERED: 'ordered';
OPTIONAL: 'optional';
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