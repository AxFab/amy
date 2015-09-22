using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amy
{
  interface IOperations
  {
    void Add (Operand result, Operand left, Operand right);
    void Sub (Operand result, Operand left, Operand right);
    void Mul (Operand result, Operand left, Operand right);
    void Div (Operand result, Operand left, Operand right);
    void Equals (Operand result, Operand left, Operand right);
    void NotEquals (Operand result, Operand left, Operand right);
    void Less (Operand result, Operand left, Operand right);
    void LessEq (Operand result, Operand left, Operand right);
    void More (Operand result, Operand left, Operand right);
    void MoreEq (Operand result, Operand left, Operand right);
    void Not (Operand result, Operand left, Operand right);
  }
}
