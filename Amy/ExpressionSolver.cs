using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmyDeprecated
{
  class Token {
    int no_;
    public Token (int no) { no_ = no; }
    public override string ToString () { return no_.ToString(); }
  }

  enum OperatorType
  {
    Operand = 0,
    Not,
    Add,
    Sub,
    Parenthese,
    Equals,
    NotEquals,
    And,
    Or,
    Mul,
    Div,
    __Count,
  }

  enum OperandType
  {
    Undefined = -1,
    Unknow = 0,
    Boolean,
    Byte,
    SmallInt,
    Integer,
    Long,
    Float,
    Double,
    Decimal,
    String,
    Pointer,
    Reference,
    VarBoolean,
    VarByte,
    VarSmallInt,
    VarInt,
    VarLong,
    VarFloat,
    VarDouble,
    VarDecimal,
    VarString,
    VarPointer,
    VarReference,
    __Count,
  }

  enum ExpressionStatus
  {
    Start = 0,
    Operand,
    BinaryOperator,
    UnaryOperatorLeftRight,
    UnaryOperatorRightLeft,
    Error,
  };

  class ExpressionFunctions
  {
    public static void Dot (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      if (right.Type != OperandType.Reference && right.Type != OperandType.VarReference) {
        res.Type = OperandType.Undefined;
        res.Value = 0;
        return;
      }

      res.Type = OperandType.Reference; // TODO Might be pointer
      res.Value = right.Value + 0; // TODO Find member offset!
    }

    public static void Call (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      // TODO Right must be operand or tuplet!
      // Left must be a function: pointer or reference
      // Res get return value
      // If the function is marked as const, and we checked, we can execute it (still sandboxed).
      res.Type = OperandType.Undefined;
      res.Value = 0;
    }

    public static void Inc (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      res.Type = right.Type;
      res.Value = (right.Type != OperandType.Undefined) ? right.Value + 1 : 0;
    }

    public static void Dec (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      res.Type = right.Type;
      res.Value = (right.Type != OperandType.Undefined) ? right.Value - 1 : 0;
    }


    public static void Not (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      if (right.Type == OperandType.Undefined) {
        res.Type = OperandType.Undefined;
        res.Value = 0;
        return;
      }

      res.Type = OperandType.Boolean;
      res.Value = right.Value == 0 ? 1 : 0;
    }

    public static void BitwiseNot (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      if (right.Type == OperandType.Undefined) {
        res.Type = OperandType.Undefined;
        res.Value = 0;
        return;
      }

      res.Type = right.Type;
      res.Value = ~right.Value;
    }


      ////AddOperator(OperatorType.BitwiseNot, 3, 1, ExpressionFunctions.Not);
      //AddOperator(OperatorType.Mul, 5, 2, ExpressionFunctions.Mul); // Gch->Drt
      //AddOperator(OperatorType.Div, 5, 2, ExpressionFunctions.Div);
      ////AddOperator(OperatorType.Mod, 5, 2, ExpressionFunctions.Mod);
      //AddOperator(OperatorType.Add, 6, 2, ExpressionFunctions.Add);
      //AddOperator(OperatorType.Sub, 6, 2, ExpressionFunctions.Sub);
      ////BitwiseShiftLeft, BitwiseShiftRight -> 7
      //// Less, More, LessEq, MoreEq -> 8
      //AddOperator(OperatorType.Equals, 9, 2, ExpressionFunctions.Equals);
      //AddOperator(OperatorType.NotEquals, 9, 2, ExpressionFunctions.NotEquals);
      ////AddOperator(OperatorType.BitwiseAnd, 10, 2, ExpressionFunctions.And);
      ////AddOperator(OperatorType.BitwiseXor, 11, 2, ExpressionFunctions.And);
      ////AddOperator(OperatorType.BitwiseOr, 12, 2, ExpressionFunctions.And);
      //AddOperator(OperatorType.And, 13, 2, ExpressionFunctions.And);
      //AddOperator(OperatorType.Or, 14, 2, ExpressionFunctions.Or);
      //// 15 CondTernaire, Assign(s)
      //// 16 throw
      //// 17 ','
      //// 99 '()'


    public static void Add (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      if (left.Type == OperandType.Undefined || right.Type == OperandType.Undefined) {
        res.Type = OperandType.Undefined;
        res.Value = 0;
        return;
      }

      if (left.Type == right.Type)
        res.Type = left.Type;
      res.Value = left.Value + right.Value;
    }

    public static void Sub (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      if (left.Type == OperandType.Undefined || right.Type == OperandType.Undefined) {
        res.Type = OperandType.Undefined;
        res.Value = 0;
        return;
      }

      if (left.Type == right.Type)
        res.Type = left.Type;
      res.Value = left.Value - right.Value;
    }

    public static void And(ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      res.Type = OperandType.Boolean;
      res.Value = 0;
      if (left.Type == OperandType.Undefined || (left.Value != 0 && right.Type == OperandType.Undefined))
        res.Type = OperandType.Undefined;
      else if (left.Value != 0 && right.Value != 0)
        res.Value = 1;
    }

    public static void Or (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      res.Type = OperandType.Boolean;
      res.Value = 0;
      if (left.Type == OperandType.Undefined || (left.Value == 0 && right.Type == OperandType.Undefined))
        res.Type = OperandType.Undefined;
      else if (left.Value != 0 || right.Value != 0)
        res.Value = 1;
    }

    public static void Equals (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      res.Type = OperandType.Boolean;
      res.Value = 0;
      if (left.Type == OperandType.Undefined || right.Type == OperandType.Undefined)
        res.Type = OperandType.Undefined;
      else if (left.Value == right.Value) // TODO Type handling
        res.Value = 1;
    }

    public static void NotEquals (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      res.Type = OperandType.Boolean;
      res.Value = 0;
      if (left.Type == OperandType.Undefined || right.Type == OperandType.Undefined)
        res.Type = OperandType.Undefined;
      else if (left.Value != right.Value) // TODO Type handling
        res.Value = 1;
    }

    public static void Mul (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      if (left.Type == OperandType.Undefined || right.Type == OperandType.Undefined) {
        res.Type = OperandType.Undefined;
        res.Value = 0;
        return;
      }

      if (left.Type == right.Type)
        res.Type = left.Type;
      res.Value = left.Value * right.Value;
    }

    public static void Div (ExpressionNode left, ExpressionNode right, ref ExpressionNode res)
    {
      if (left.Type == OperandType.Undefined || right.Type == OperandType.Undefined) {
        res.Type = OperandType.Undefined;
        res.Value = 0;
        return;
      }

      if (left.Type == right.Type)
        res.Type = left.Type;
      if (right.Value == 0) {
        res.Value = 0;
        res.Type = OperandType.Undefined;
      } else {
        res.Value = left.Value / right.Value;
      }
    }
  }

  /// <summary>Represent a single word on an expression.</summary>
  struct ExpressionNode
  {
    public Token Token; //< <summary>The token representing the word.</summary>
    public OperandType Type; ///< <summary>The type of the value of the node.</summary>
    public OperatorType Opcode; ///< <summary>Define the type of the node operator.</summary>
    public long Value; ///< <summary>Hold the computed value of the </summary>
    public int Priority;

    public override string ToString ()
    {
      return this.Opcode.ToString() + "{" + this.Type.ToString() + "}" + "(" + this.Value + ")";
    }
  }

  delegate void ComputeMethod (ExpressionNode left, ExpressionNode right, ref ExpressionNode res);
  
  struct ExpressionOperator
  {
    public int Priority;
    public int OperandCount;
    public ComputeMethod Compute;
  }

  /// <summary>Engine that allow to resolve prefixed expression.</summary>
  class ExpressionSolver
  {
    static ExpressionOperator[] operators_;

    static void AddOperator (OperatorType op, int prio, int count, ComputeMethod func)
    {
      operators_[(int)op] = new ExpressionOperator()
      {
        Priority = prio,
        OperandCount = count,
        Compute = func
      };
    }

    static ExpressionSolver ()
    {
      operators_ = new ExpressionOperator[(int)OperatorType.__Count];
      //AddOperator(OperatorType.Dot, 1, 2, ExpressionFunctions.Not); // Assoc Gch->Drt
      //AddOperator(OperatorType.IncSfx, 2, 1, ExpressionFunctions.Not); 
      //AddOperator(OperatorType.DecSfx, 2, 1, ExpressionFunctions.Not);
      //AddOperator(OperatorType.Call, 2, 2, ExpressionFunctions.Not);
      //AddOperator(OperatorType.IncPfx, 2, 1, ExpressionFunctions.Not); // Drt -> Gch
      //AddOperator(OperatorType.DecPfx, 2, 1, ExpressionFunctions.Not);
      AddOperator(OperatorType.Not, 3, 1, ExpressionFunctions.Not);
      //AddOperator(OperatorType.BitwiseNot, 3, 1, ExpressionFunctions.Not);
      AddOperator(OperatorType.Mul, 5, 2, ExpressionFunctions.Mul); // Gch->Drt
      AddOperator(OperatorType.Div, 5, 2, ExpressionFunctions.Div);
      //AddOperator(OperatorType.Mod, 5, 2, ExpressionFunctions.Mod);
      AddOperator(OperatorType.Add, 6, 2, ExpressionFunctions.Add);
      AddOperator(OperatorType.Sub, 6, 2, ExpressionFunctions.Sub);
      //BitwiseShiftLeft, BitwiseShiftRight -> 7
      // Less, More, LessEq, MoreEq -> 8
      AddOperator(OperatorType.Equals, 9, 2, ExpressionFunctions.Equals);
      AddOperator(OperatorType.NotEquals, 9, 2, ExpressionFunctions.NotEquals);
      //AddOperator(OperatorType.BitwiseAnd, 10, 2, ExpressionFunctions.And);
      //AddOperator(OperatorType.BitwiseXor, 11, 2, ExpressionFunctions.And);
      //AddOperator(OperatorType.BitwiseOr, 12, 2, ExpressionFunctions.And);
      AddOperator(OperatorType.And, 13, 2, ExpressionFunctions.And);
      AddOperator(OperatorType.Or, 14, 2, ExpressionFunctions.Or);
      // 15 CondTernaire, Assign(s)
      // 16 throw
      // 17 ','
      // 99 '()'

    }


    private Stack<ExpressionNode> postFixStack_ = new Stack<ExpressionNode>();
    private Stack<ExpressionNode> inFixStack_ = new Stack<ExpressionNode>();

    private void addPostFixOperand(ExpressionNode node)
    {
      postFixStack_.Push(node);
    }

    private void addPostFixOperator(ExpressionNode node)
    {
      ExpressionOperator op = operators_[(int)node.Opcode];
      ExpressionNode opRg;
      ExpressionNode opLf;
      if (op.Priority == 0)
        throw new Exception("Internal error, the definition for this operator can't be found: " + node.Opcode);

      if (op.OperandCount == 1) {
        if (postFixStack_.Count < 1)
          throw new Exception("Missing operand for the operator: " + node.Opcode);
        opRg = postFixStack_.Pop();
        opLf = new ExpressionNode();
      } else {
        if (postFixStack_.Count < 2)
          throw new Exception("Missing operands for the operator: " + node.Opcode);
        opRg = postFixStack_.Pop();
        opLf = postFixStack_.Pop();
      }

      op.Compute(opLf, opRg, ref node);
      postFixStack_.Push(node);
      // TODO Write SSA
    }

    private bool error (string msg)
    {
      this.status_ = ExpressionStatus.Error;
      if (last_ != null)
        this.errMsg_ = msg + " after " + last_.ToString() + ".";
      else
        this.errMsg_ = msg + " to begin.";

      return false;
    }

    public bool OpenParenthesis(Token token)
    {
      if (this.status_ != ExpressionStatus.Start && this.status_ != ExpressionStatus.BinaryOperator)
        return error("Unexpected operand");
      ExpressionNode node = new ExpressionNode();
      node.Token = token;
      node.Opcode = OperatorType.Parenthese;
      node.Priority = 99;
      inFixStack_.Push(node);
      setLast(token, ExpressionStatus.Start);
      return true;
    }

    public bool CloseParenthesis(Token token)
    {
      if (this.status_ != ExpressionStatus.Operand)
        return error("unexpected parenthese");
      ExpressionNode node = inFixStack_.Pop();
      while (node.Opcode != OperatorType.Parenthese)
      {
        addPostFixOperator(node);
        if (inFixStack_.Count == 0)
          return error("closing parenthese without openning");
        node = inFixStack_.Pop();
      }
      setLast(token, ExpressionStatus.Operand);
      return true;
    }

    private string errMsg_;
    private ExpressionStatus status_;
    private Token last_;

    private void setLast (Token token, ExpressionStatus status)
    {
      this.status_ = status;
      this.last_ = token;
    }

    public bool AddOperand(Token token, OperandType type, long value)
    {
      if (this.status_ != ExpressionStatus.Start && this.status_ != ExpressionStatus.BinaryOperator)
        return error("Unexpected operand");
      ExpressionNode node = new ExpressionNode();
      node.Token = token;
      node.Type = type;
      node.Value = value;
      addPostFixOperand(node);
      setLast(token, ExpressionStatus.Operand);
      return true;
    }

    public bool AddOperator(Token token, OperatorType opcode)
    {
      ExpressionOperator op = operators_[(int)opcode];
      if (op.OperandCount == 2 && this.status_ != ExpressionStatus.Operand)
        return error("Unexpected operator");

      ExpressionNode node = new ExpressionNode();
      node.Token = token;
      node.Opcode = opcode;
      node.Priority = op.Priority;

      while (inFixStack_.Count > 0 && inFixStack_.Peek().Priority <= node.Priority)
      {
          ExpressionNode nd = inFixStack_.Pop();
          addPostFixOperator(nd);
      }

      inFixStack_.Push(node);
      if (op.OperandCount == 2)
        setLast(token, ExpressionStatus.BinaryOperator);
      else
        setLast(token, ExpressionStatus.UnaryOperatorLeftRight);
      return true;
    }

    public bool Compile()
    {
      if (this.status_ != ExpressionStatus.Operand)
        return error("Expression incompleted, expected operand");
      while (inFixStack_.Count > 0) 
      {
        var pop = inFixStack_.Pop ();
        addPostFixOperator (pop);
      }

      if (postFixStack_.Count != 1)
        return error("Unexpected error, unable to resolve the expression");
      return true;
    }

    public bool IsTrue()
    {
      return status_ != ExpressionStatus.Error && postFixStack_.Peek().Value != 0;
    }

    public long Value
    {
      get
      {
        return postFixStack_.Peek().Value;
      }
    }
  }
}
