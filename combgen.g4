grammar combgen;

script:     parameterAssignment*
            datafieldAssignment+
            combinationalExpression;

parameterAssignment:  IDENTIFIER '=' NUMBER;

datafieldAssignment:  VARIABLE '=' datafieldExpression ';';

datafieldExpression:   stringDatafield ((NCR | NPR) NUMBER)?     # stringDatafieldExpression
                     | (NUMBER ('*' | '×'))? intDatafield        # intDatafieldExpression
                     ;

stringDatafield:       literalStringDatafield
                     | fileStringDatafield
                     | optionalStringDatafield
                     ;
                     
literalStringDatafield: '{' (DQ_STRING | stringArray) (',' (DQ_STRING | stringArray))* '}';

stringArray:        '[' DQ_STRING (';' DQ_STRING)+ ']';

fileStringDatafield:    SQ_STRING;

optionalStringDatafield: '~' DQ_STRING;
                     
intDatafield:         MKINT '(' integer '-' integer (',' NUMBER)? ')';

integer: ('+' | '-')? NUMBER;
float:   ('+' | '-')? DECIMAL;

combinationalExpression:    '<'
                                expression
                                ('.' expression)*
                            '>'
                            ;

expression:                   expression ('*' | '/') expression   #MulExpression
                            | expression ('+' | '-') expression   #AddExpression
                            | expression ('<' | '>') expression   #CompareExpression
                            | expression ('=' | '!=') expression  #EqualityExpression
                            | '!' expression                      #NegatedExpression
                            | '(' expression ')'                  #ParenthesizedExpression
                            | variableAccess                      #VariableExpression
                            | functionCall                        #FunctionCallExpression
                            | dqString                            #StringExpression
                            | integer                             #IntExpression
                            | boolean                             #BooleanExpression
                            ;

boolean:                    'true' | 'false';

functionCall:       IDENTIFIER '(' expression (',' expression)* ')';
dqString:           DQ_STRING;

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

DQ_STRING:  '"' ~["\n\r\f]* '"';
SQ_STRING:  '\'' ~['\n\r\f]* '\'';

VARIABLE:       '$' IDENTIFIER;
IDENTIFIER:     [A-Za-z][A-Za-z_]*;
NUMBER:         '0' | [1-9][0-9]*;

fragment DECIMAL_PART: [0-9]+;

DECIMAL:      NUMBER '.' DECIMAL_PART;

LINE_COMMENT: '//' ~[\n]* -> skip;
WS:         [ \n\r\f]+ -> skip;