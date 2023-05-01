grammar Ecs;

program: namespace? components systems EOF;

namespace: NAMESPACE NAMESPACE_IDENTIFIER SEMI;

components: component*;
component: COMPONENT IDENTIFIER SEMI;

systems: system*;
system: SYSTEM IDENTIFIER BLOCK_START processParam aspectParam BLOCK_END;

processParam: PROCESS COLON PROCESS_PASS SEMI;

aspectParam: ASPECT BLOCK_START aspectComponent+ BLOCK_END;
aspectComponent: OPTIONAL? READONLY? IDENTIFIER SEMI;

// Tokens
SEMI: ';';
COLON: ':';
DOT: '.';
BLOCK_START: '{';
BLOCK_END: '}';

// Keywords
NAMESPACE: 'namespace';
COMPONENT: 'component';
SYSTEM: 'system';

PROCESS: 'process';
ASPECT: 'aspect';

OPTIONAL: 'optional';
READONLY: 'readonly';

PROCESS_PASS: 'Update' | 'FixedUpdate' | 'Draw';

// Compounds
IDENTIFIER: [a-zA-Z_][a-zA-Z_0-9]*;
NAMESPACE_IDENTIFIER: IDENTIFIER (DOT NAMESPACE_IDENTIFIER)?;

// Skips
WHITESPACE: [ \t\r\n\f]+ -> skip;
COMMENT: ('//' ~('\n'|'\r')* '\r'? '\n') -> skip ;