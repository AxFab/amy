using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amy
{
  class OperationsSigned : IOperations
  {
    public static OperationsSigned I = new OperationsSigned();

    public void Add (Operand result, Operand left, Operand right)
    {
      Primitive type = (Primitive)Math.Max((int)left.PType, (int)right.PType);
      result.Set(type, left.AsSigned + right.AsSigned, left.IsConst && right.IsConst);
    }

    public void Sub (Operand result, Operand left, Operand right)
    {
      Primitive type = (Primitive)Math.Max((int)left.PType, (int)right.PType);
      result.Set(type, left.AsSigned - right.AsSigned, left.IsConst && right.IsConst);
    }

    public void Mul (Operand result, Operand left, Operand right)
    {
      Primitive type = (Primitive)Math.Max((int)left.PType, (int)right.PType);
      result.Set(type, left.AsSigned * right.AsSigned, left.IsConst && right.IsConst);
    }

    public void Div (Operand result, Operand left, Operand right)
    {
      Primitive type = (Primitive)Math.Max((int)left.PType, (int)right.PType);
      if (!right.AsBoolean)
        result.Set(Primitive.Error, false, right.IsConst);
      else
        result.Set(type, left.AsSigned / right.AsSigned, left.IsConst && right.IsConst);
    }

    public void Equals (Operand result, Operand left, Operand right)
    {
      result.Set(Primitive.Boolean, left.AsSigned == right.AsSigned, left.IsConst && right.IsConst);
    }

    public void NotEquals (Operand result, Operand left, Operand right)
    {
      result.Set(Primitive.Boolean, left.AsSigned != right.AsSigned, left.IsConst && right.IsConst);
    }

    public void Less (Operand result, Operand left, Operand right)
    {
      result.Set(Primitive.Boolean, left.AsSigned < right.AsSigned, left.IsConst && right.IsConst);
    }

    public void LessEq (Operand result, Operand left, Operand right)
    {
      result.Set(Primitive.Boolean, left.AsSigned <= right.AsSigned, left.IsConst && right.IsConst);
    }

    public void More (Operand result, Operand left, Operand right)
    {
      result.Set(Primitive.Boolean, left.AsSigned > right.AsSigned, left.IsConst && right.IsConst);
    }

    public void MoreEq (Operand result, Operand left, Operand right)
    {
      result.Set(Primitive.Boolean, left.AsSigned >= right.AsSigned, left.IsConst && right.IsConst);
    }
    public void Not (Operand result, Operand left, Operand right)
    {
      result.Set(Primitive.Boolean, !right.AsBoolean, right.IsConst);
    }
  }
}
