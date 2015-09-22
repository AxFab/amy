using System;

namespace Amy.Syntax
{
    public enum ASTOperator
    {
        None, Parenthesis,
        Binary,
        Add, Sub, Mul, Div, Rem, And, Or, Xor, Shl, Lshr, Ashr,
        Less, LessEq, More, MoreEq, Equals, NotEquals,
        Assign, LogOr, LogAnd,
        Single,
        Not, Neg,
        End,
    }
}
