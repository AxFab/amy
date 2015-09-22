using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy
{
  /// <summary>Engine that allow to resolve prefixed expression.</summary>
  public class Resolver
  {
    /// <summary>Status represent the type of last operand or operator 
    /// pushed on the expression.</summary>
    private enum ExpressionStatus
    {
      /// <summary>Indicates that the expression or an sub-expression just started. You can consider the previous token is '('.</summary>
      Start = 0,
      /// <summary>The token placed at the left is an operand, not an operator.</summary>
      Operand,
      /// <summary>The last token pushed is a binary operator that expect an operand at right and left.</summary>
      BinaryOperator,
      /// <summary>The last token pushed is a unitary operator with left to right associativity.</summary>
      UnaryOperatorLeftRight,
      /// <summary>The last token pushed is a unitary operator with right to left associativity.</summary>
      UnaryOperatorRightLeft,
      /// <summary>Indicates that the expression is not valid. </summary>
      Error,
    };

    /// <summary>the post-fix stack allow to store operand(as value) as they come.</summary>
    private Stack<Operand> postFixStack_;
    
    /// <summary>The in-fix stack store operators as they come.</summary>
    /// Note that if a lower priority need to be pushed, every operators 
    /// will be poped, processed and pushed back on the post-fix stack.
    private Stack<Operand> inFixStack_;

    /// <summary>Store an error message in case of invalid expression</summary>
    private string errMsg_;

    /// <summary>Keep track of the expression state to push operand in the correct order.</summary>
    private ExpressionStatus status_;

    /// <summary>Keep track of the last pushed token.</summary>
    private object last_;

    /// <summary>Index used internaly for SSA variable naming.</summary>
    private int ssaNameIdx;

    /// <summary>Static array used internaly for SSA variable naming.</summary>
    private const string ssaNameArr = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>Initializes a new instance of the <see cref="Resolver" /> class.</summary>
    public Resolver ()
    {
      this.Reset();
    }

    /// <summary>Push an operand on the post-fix stack.</summary>
    /// <param name="node">An operand object without operator.</param>
    private void addPostFixOperand(Operand node)
    {
      System.Diagnostics.Debug.Assert(node.Operator == Operator.Operand);
      postFixStack_.Push(node);
    }

    /// <summary>Resolve an operator and push it pack on the post-fix stack.</summary>
    /// <param name="node">An operand object with an operator.</param>
    private void addPostFixOperator(Operand node)
    {
      Operand opRg;
      Operand opLf;
      System.Diagnostics.Debug.Assert(node.Operator != Operator.Operand);
      System.Diagnostics.Debug.Assert(node.Operator != Operator.Parenthese);

      if (node.Priority == 0)
        throw new Exception("Internal error, the definition for this operator can't be found: " + node.Operator);

      if (node.OperandCount == 1) {
        if (postFixStack_.Count < 1)
          throw new Exception("Missing operand for the operator: " + node.Operator);
        opRg = postFixStack_.Pop();
        opLf = new Operand(null, Primitive.Error, false);

      } else if (node.OperandCount == 2) {
        if (postFixStack_.Count < 2)
          throw new Exception("Missing operands for the operator: " + node.Operator);
        opRg = postFixStack_.Pop();
        opLf = postFixStack_.Pop();

      } else {
        // TODO Parenthesis can be there if the expression is incorrect
        throw new Exception("Invalid operator: " + node.Operator);
      }

      node.Resolve(opLf, opRg);
      postFixStack_.Push(node);
    }

    /// <summary>Set the expression in error and keep track of an error message.</summary>
    /// <param name="msg">An error message.</param>
    /// <returns>Always return false to be return by the calling function.</returns>
    private bool error (string msg)
    {
      if (this.status_ == ExpressionStatus.Error)
        return false;
      this.status_ = ExpressionStatus.Error;
      if (last_ != null)
        this.errMsg_ = msg + " after " + last_.ToString() + ".";
      else
        this.errMsg_ = msg + " to begin.";

      return false;
    }

    /// <summary>Set the last token and check for expression consistency using the saved status.</summary>
    /// <param name="token">An generic object used to get info about the token.</param>
    /// <param name="op">the operator value to pushed.</param>
    private bool setLast (object token, Operator op)
    {
      if (op == Operator.Operand || op == Operator.Parenthese) {
        if (this.status_ != ExpressionStatus.Start &&
          this.status_ != ExpressionStatus.BinaryOperator &&
          this.status_ != ExpressionStatus.UnaryOperatorLeftRight)
          return error("Unexpected operand");
        this.status_ = op == Operator.Operand ? ExpressionStatus.Operand : ExpressionStatus.Start;
      } else if (op < Operator.__Binary) {
        if (this.status_ == ExpressionStatus.Operand)
          return error("Unexpected prefix operator");
        this.status_ = ExpressionStatus.UnaryOperatorLeftRight;
      } else {
        if (this.status_ != ExpressionStatus.Operand)
          return error("Unexpected operator");
        this.status_ = ExpressionStatus.BinaryOperator;
      }

      this.last_ = token;
      return true;
    }


    public void Reset ()
    {
      postFixStack_ = new Stack<Operand>();
      inFixStack_ = new Stack<Operand>();
      errMsg_ = null;
      status_ = ExpressionStatus.Start;
      last_ = null;
    }

    public bool OpenParenthese(object token)
    {
      if (!setLast(token, Operator.Parenthese))
        return false;

      Operand node = new Operand(token, Operator.Parenthese);
      inFixStack_.Push(node);
      return true;
    }

    public bool CloseParenthese(object token)
    {
      if (this.status_ != ExpressionStatus.Operand)
        return error("unexpected parenthese");

      this.last_ = token;
      this.status_ = ExpressionStatus.Operand;
      Operand node = inFixStack_.Pop();
      while (node.Operator != Operator.Parenthese) {
        addPostFixOperator(node);
        if (inFixStack_.Count == 0)
          return error("closing parenthese without openning");
        node = inFixStack_.Pop();
      }
      return true;
    }

    #region [   Push every types   ]

    public bool Push (object token, Primitive type, bool value)
    {
      if (!setLast(token, Operator.Operand))
        return false;

      Operand node = new Operand(token, type, value);
      addPostFixOperand(node);
      return true;
    }

    public bool Push (object token, Primitive type, long value)
    {
      if (!setLast(token, Operator.Operand))
        return false;

      Operand node = new Operand(token, type, value);
      addPostFixOperand(node);
      return true;
    }

    public bool Push (object token, Primitive type, decimal value)
    {
      if (!setLast(token, Operator.Operand))
        return false;

      Operand node = new Operand(token, type, value);
      addPostFixOperand(node);
      return true;
    }

    public bool Push (object token, bool value)
    {
      return this.Push(token, Primitive.Boolean, value);
    }

    public bool Push (object token, sbyte value)
    {
      return this.Push(token, Primitive.SByte, (long)value);
    }

    public bool Push (object token, byte value)
    {
      return this.Push(token, Primitive.Byte, (long)value);
    }

    public bool Push (object token, short value)
    {
      return this.Push(token, Primitive.Short, (long)value);
    }

    public bool Push (object token, ushort value)
    {
      return this.Push(token, Primitive.UShort, (long)value);
    }

    public bool Push (object token, int value)
    {
      return this.Push(token, Primitive.Int, (long)value);
    }

    public bool Push (object token, uint value)
    {
      return this.Push(token, Primitive.UInt, (long)value);
    }

    public bool Push (object token, long value)
    {
      return this.Push(token, Primitive.Long, (long)value);
    }

    public bool Push (object token, float value)
    {
      return this.Push(token, Primitive.Float, (decimal)value);
    }

    public bool Push (object token, double value)
    {
      return this.Push(token, Primitive.Double, (decimal)value);
    }

    public bool Push (object token, decimal value)
    {
      return this.Push(token, Primitive.Decimal, (decimal)value);
    }

    #endregion [   Push every types   ]

    public bool Push (object token, Operator opcode)
    {
      if (!setLast(token, opcode))
        return false;

      Operand node = new Operand(token, opcode);
      while (inFixStack_.Count > 0 && inFixStack_.Peek().Priority <= node.Priority) {
        Operand nd = inFixStack_.Pop();
        addPostFixOperator(nd);
      }

      inFixStack_.Push(node);
      return true;
    }

    public bool Compile()
    {
      if (this.status_ == ExpressionStatus.Error)
        return false;

      foreach (Operand op in inFixStack_) {
        if (op.Operator == Operator.Parenthese)
          return error("Missing clossing parenthese");

      }

      if (this.status_ != ExpressionStatus.Operand)
        return error("Expression incompleted, expected operand");
      while (inFixStack_.Count > 0) {
        var pop = inFixStack_.Pop ();
        addPostFixOperator (pop);
      }

      if (postFixStack_.Count != 1)
        return error("Unexpected error, unable to resolve the expression");
      return true;
    }

    public SSAFlow SSA (bool removeConst = true)
    {
      ssaNameIdx = 0;
      Operand op = postFixStack_.Peek();
      SSAFlow flow = SSA(op, null, removeConst);
      return flow;
    }

    private SSAFlow SSA (Operand op, SSAFlow flow, bool removeConst)
    {
      if (flow == null)
        flow = new SSAFlow();
      Operand origin = op;
      while (op.Left != null && (!removeConst || !op.Left.IsConst))
        op = op.Left;
      for (; ; ) {
        if (op.Operator == Operator.Operand || (op.IsConst && removeConst))
          op.Code = ssaNameArr[ssaNameIdx++].ToString();
        flow.Push(new SSA(op, removeConst));
        //Console.WriteLine(op.SSA()); // TODO
        op = op.Parent;
        if (op == null || op == origin.Parent)
          break;
        if (op != null && op.Right != null && op.Right.Operator != Operator.Operand) 
          SSA(op.Right, flow, removeConst);
      }

      return flow;
    }

    public bool AsBoolean
    {
      get
      {
        if (postFixStack_.Count != 1 || inFixStack_.Count != 0)
          throw new Exception("Incomplete expression");
        return status_ != ExpressionStatus.Error && postFixStack_.Peek().AsBoolean;
      }
    }

    public long AsSigned
    {
      get
      {
        if (status_ == ExpressionStatus.Error)
          throw new Exception("Invalid expression");
        if (postFixStack_.Count != 1 || inFixStack_.Count != 0)
          throw new Exception("Incomplete expression");
        return postFixStack_.Peek().AsSigned;
      }
    }

    public ulong AsUnsigned
    {
      get
      {
        if (status_ == ExpressionStatus.Error)
          throw new Exception("Invalid expression");
        if (postFixStack_.Count != 1 || inFixStack_.Count != 0)
          throw new Exception("Incomplete expression");
        return postFixStack_.Peek().AsUnsigned;
      }
    }

    public decimal AsDecimal
    {
      get
      {
        if (status_ == ExpressionStatus.Error)
          throw new Exception("Invalid expression");
        if (postFixStack_.Count != 1 || inFixStack_.Count != 0)
          throw new Exception("Incomplete expression");
        return postFixStack_.Peek().AsDecimal;
      }
    }

    public Primitive Type
    {
      get
      {
        return postFixStack_.Peek().PType;
      }
    }
  }
}
