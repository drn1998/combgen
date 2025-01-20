grammar combgen;

script:     parameterAssignment*
            datafieldAssignment+
            combinationalExpression?;

parameterAssignment:  IDENTIFIER '=' NUMBER;

datafieldAssignment:  SECTION* ANNOTATION? VARIABLE '=' datafieldExpression ';';

datafieldExpression:   ORDERED? stringDatafield ((NCR | NPR) NUMBER)?     # stringDatafieldExpression
                     | (NUMBER ('*' | '×'))? intDatafield                 # intDatafieldExpression
                     | floatDatafield                                     # floatDatafieldExpression
                     ;

/*datafieldAssignment:  VARIABLE '=' (NUMBER ('*' | '×'))? datafieldExpression ';';

datafieldExpression:   (ORDERED)? stringDatafield ((NCR | NPR) (NUMBER (',' NUMBER)* | NUMBER '-' NUMBER | ALL))?     # stringDatafieldExpression
                     | intDatafield                                         # intDatafieldExpression
                     ;*/

stringDatafield:       literalStringDatafield
                     | fileStringDatafield
                     | optionalStringDatafield
                     | anonymousStringDatafield
                     ;
                     
literalStringDatafield: '{' (DQ_STRING | stringArray) (',' (DQ_STRING | stringArray))* '}';

stringArray:        '[' DQ_STRING (';' DQ_STRING)+ ']';

fileStringDatafield:    SQ_STRING;

optionalStringDatafield: '~' DQ_STRING;

anonymousStringDatafield: '[' NUMBER ']';
                     
intDatafield:         MKINT '(' integer '-' integer (',' NUMBER)? ')';
floatDatafield:       MKFLOAT '(' float '-' float (',' DECIMAL)? ')';

integer: ('+' | '-')? NUMBER;
float:   ('+' | '-')? DECIMAL;

ORDERED:        '°';

combinationalExpression:    '<'
                                expression
                                ('.' expression)*
                            '>'
                            ;

// Check reasonable precedece of those expressions; implement modulus % and exponentiation ^

expression:                   expression mulOp expression                       #MulExpression
                            | expression addOp expression                       #AddExpression
                            | expression compOp expression                      #CompareExpression
                            | expression eqOp expression                        #EqualityExpression
                            | expression logOp expression                       #LogicalExpression
                            | '!' expression                                    #NegatedExpression
                            | '(' expression ')'                                #ParenthesizedExpression
                            | variableAccess                                    #VariableExpression
                            | functionCall                                      #FunctionCallExpression
                            | dqString                                          #StringExpression
                            | integer                                           #IntExpression
                            | float                                             #FloatExpression
                            | boolean                                           #BooleanExpression
                            ;

boolean:                    'true' | 'false';

functionCall:       IDENTIFIER '(' (expression (',' expression)*)? ')';
dqString:           DQ_STRING;
addOp:     '+' | '-';
mulOp:     '*' | '/';
compOp:    '>=' | '<=' | '<' | '>';
eqOp:      '!=' | '==';
logOp:     'and' | 'or';

variableAccess:     VARIABLE (('[' expression ']' ('[' expression ']')?)? | ARROW '[' expression ']');

ARROW: '->';

// Reserved keywords

NCR:        'nCr';
NPR:        'nPr';

MKINT:      'mkint';
MKFLOAT:    'mknum';
MKTIME:     'mktime';
MKDATE:     'mkdate';
COUNT:      'count';
REM:        'rem';
//ALL:        'all';

DQ_STRING: '"' (DQ_ESC|.)*? '"' ;
SQ_STRING: '\'' (SQ_ESC|.)*? '\'' ;

fragment
DQ_ESC : '\\"' | '\\\\' ;

fragment
SQ_ESC : '\\\'' | '\\\\' ;

VARIABLE:       '$' IDENTIFIER;
IDENTIFIER:     [A-Za-z][A-Za-z_]*;
NUMBER:         '0' | [1-9][0-9]*;

DECIMAL:      NUMBER '.' DECIMAL_PART;

fragment DECIMAL_PART: [0-9]+;

SECTION: '#' ~[\n]*;
ANNOTATION: '>>>' ( ANNOTATION | . )*? '<<<';

LINE_COMMENT: '//' ~[\n]* -> skip;
BLOCK_COMMENT : '/*' ( BLOCK_COMMENT | . )*? '*/'  -> skip ;
WS:         [ \n\r\t\f]+ -> skip;