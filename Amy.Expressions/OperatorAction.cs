using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amy
{
  delegate void OperatorImplem (Operand result, Operand left, Operand right);

  class OperatorAction
  {
    //static HashSet<OperatorAction> alls_ = new HashSet<OperatorAction>();

    //public OperatorAction (Operator op, Primitive left, Primitive right)
    //{
    //  if (this.Operator < Operator.__Binary)
    //    this.Left = Primitive.Undefined;
    //  this.Operator = op;
    //  this.Left = left;
    //  this.Right = right;

    //  alls_.Add(this);
    //}

    //public static OperatorAction GetAction (Operator op, Primitive left, Primitive right)
    //{
    //  if (op < Operator.__Binary)
    //    left = Primitive.Undefined;

    //  foreach (OperatorAction action in alls_) {
    //    if (op == action.Operator && left == action.Left && right == action.Right)
    //      return action;
    //  }

    //  return null;
    //}

    //public Operator Operator { get; private set; }
    //public Primitive Left { get; private set; }
    //public Primitive Right { get; private set; }
    //public OperatorImplem Action { get; set; }

    //public override bool Equals (object obj)
    //{
    //  OperatorAction action = obj as OperatorAction;
    //  if (action == null)
    //    return false;
    //  return this.Operator == action.Operator && this.Left == action.Left && this.Right == action.Right;
    //}

    //public override int GetHashCode ()
    //{
    //  return (int)this.Operator ^ (int)this.Left ^ (int)this.Right;
    //}

    //public override string ToString ()
    //{
    //  if (this.Operator < Operator.__Binary)
    //    return this.Operator + " " + this.Right;
    //  return this.Left + " " + this.Operator + " " + this.Right;
    //}

    public static OperatorImplem Get (Operator op, Primitive left, Primitive right)
    {
      if (op < Operator.__Binary)
        left = Primitive.Undefined;

      if (op == Operator.And)
        return OperatorAction.andLogic;
      if (op == Operator.Or)
        return OperatorAction.orLogic;
      
      if (left == Primitive.Error || right == Primitive.Error)
        return OperatorAction.isError;

      IOperations ops;
      if (Operand.TypeIsSigned(left) && Operand.TypeIsSigned(right))
        ops = OperationsSigned.I;
      else if (Operand.TypeIsUnsigned(left) && Operand.TypeIsUnsigned(right))
        ops = OperationsUnsigned.I;
      else if (Operand.TypeIsNumber(left) || Operand.TypeIsNumber(right))
        ops = OperationsDecimal.I;
      else if (left == Primitive.Boolean || right == Primitive.Boolean)
        ops = OperationsBoolean.I;
      else
        throw new Exception("Undef operator");

      switch (op) {
        case Operator.Add:
          return ops.Add;
        case Operator.Sub:
          return ops.Sub;
        case Operator.Mul:
          return ops.Mul;
        case Operator.Div:
          return ops.Div;
        case Operator.Equals:
          return ops.Equals;
        case Operator.NotEquals:
          return ops.NotEquals;
        case Operator.Less:
          return ops.Less;
        case Operator.LessEq:
          return ops.LessEq;
        case Operator.More:
          return ops.More;
        case Operator.MoreEq:
          return ops.MoreEq;
        case Operator.Not:
          return ops.Not;
        default:
          throw new Exception("Undef operator");
      }

      throw new Exception("Undef operator");
    }

    private static void isError (Operand result, Operand left, Operand right) 
    {
      result.Set(Primitive.Error, false, left.IsConst && right.IsConst);
    }

    private static void andLogic (Operand result, Operand left, Operand right) 
    {
      if (left.PType == Primitive.Error)
        result.Set(Primitive.Error, false, left.IsConst);
      else if (!left.AsBoolean)
        result.Set(Primitive.Boolean, false, left.IsConst);
      else if (right.PType == Primitive.Error)
        result.Set(Primitive.Error, false, left.IsConst && right.IsConst);
      else
        result.Set(Primitive.Boolean, right.AsBoolean, left.IsConst && right.IsConst);
    }

    private static void orLogic (Operand result, Operand left, Operand right) 
    {
      if (left.PType == Primitive.Error)
        result.Set(Primitive.Error, false, left.IsConst);
      else if (left.AsBoolean)
        result.Set(Primitive.Boolean, true, left.IsConst);
      else if (right.PType == Primitive.Error)
        result.Set(Primitive.Error, false, left.IsConst && right.IsConst);
      else
        result.Set(Primitive.Boolean, right.AsBoolean, left.IsConst && right.IsConst);
    }

  }

}
