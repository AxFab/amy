using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; // Debug
// using Debug = Niut.Debug;

namespace Amy
{
  /// <summary>List all possible primitives</summary>
  /// <remarks>The order of those enum is realy important to check against range.</remarks>
  public enum Primitive
  {
    Error = -1,
    Undefined = 0,
    Null,
    Boolean,
    I8, I16, I32, I64,
    U8, U16, U32, U64,
    Float, Double, Decimal, 
    String, Pointer, Reference, 
    __Count,
   
    SByte = I8,
    Byte = U8,
    Short = I16,
    UShort = U16,
    Int = I32,
    UInt = U32,
    Long = I64,
    ULong = U64,
  }
  
  public enum Operator
  {
    Parenthese = -1,
    Operand = 0,

    // Unitary operator
    Not, BitwiseNot,
    IncPfx, DecPfx,

    // Binary operator
    __Binary,
    Add, Sub,
    Equals, NotEquals,
    And, Or,
    Mul, Div, Mod,
    NullCoalessence,
    Dot,
    Call,
    BitwiseShiftLeft, BitwiseShiftRight,
    Less, More, LessEq, MoreEq, 
    BitwiseAnd, BitwiseXor, BitwiseOr,
    Throw, Comma,

    Assign,

    // Ternary operator
    ConditionThen,
    ConditionIf,

    __Count,
  }

  class Variant
  {
    private long value_i;
    private decimal value_f;
    private string value_s;

    public Primitive PType { get; protected set; }
    public bool IsConst { get; protected set; }

    public string Value
    {
      get
      {
        if (IsSigned)
          return AsSigned.ToString();
        if (IsUnsigned)
          return AsUnsigned.ToString();
        if (IsNumber)
          return AsDecimal.ToString();
        return AsBoolean.ToString();
      }
    }

    public string Litteral
    {
      get
      {
        string value = "!?";
        if (this.IsConst && this.PType == Primitive.U64)
          value = ((ulong)this.value_i).ToString();
        else if (this.IsConst && this.PType == Primitive.String)
          value = this.value_s;
        else if (this.IsConst && this.PType == Primitive.Pointer)
          value = "@" + this.value_i.ToString("X8");
        else if (this.IsConst && this.PType == Primitive.Reference)
          value = "#" + this.value_i.ToString("X8");
        else if (this.IsConst && this.PType == Primitive.Boolean)
          value = this.value_i != 0 ? "True" : "False";
        else if (this.IsConst && this.IsNumber)
          value = this.value_f.ToString();
        else if (this.IsConst && this.IsInteger)
          value = this.value_i.ToString();
        return value;
      }
    }

    public bool IsNumber
    {
      get
      {
        return TypeIsNumber(PType);
      }
    }

    public bool IsInteger
    {
      get
      {
        return TypeIsInteger(PType);
      }
    }

    public bool IsSigned
    {
      get
      {
        return TypeIsSigned(PType);
      }
    }

    public bool IsUnsigned
    {
      get
      {
        return TypeIsUnsigned(PType);
      }
    }

    public bool AsBoolean
    {
      get
      {
        if (this.PType != Primitive.Boolean && !this.IsInteger && this.PType != Primitive.Error)
          throw new Exception("Incorrect type");
        return this.value_i != 0;
      }
    }

    public ulong AsUnsigned
    {
      get
      {
        if (!this.IsUnsigned)
          throw new Exception("Incorrect type");
        return (ulong)this.value_i;
      }
    }

    public long AsSigned
    {
      get
      {
        if (this.IsNumber)
          return (long)this.value_f;
        if (!this.IsSigned)
          throw new Exception("Incorrect type");
        return (long)this.value_i;
      }
    }

    public decimal AsDecimal
    {
      get
      {
        if (this.IsSigned)
          return (decimal)this.value_i;
        if (this.IsUnsigned)
          return (decimal)((ulong)this.value_i);
        if (!this.IsNumber)
          throw new Exception("Incorrect type");
        return this.value_f;
      }
    }

    public void Set (Primitive type, long value, bool isconst = true)
    {
      this.PType = type;
      this.IsConst = isconst;
      this.value_i = value;
      Debug.Assert(this.IsInteger);
    }

    public void Set (Primitive type, bool value, bool isconst = true)
    {
      if (type == Primitive.Error)
        value = false;
      this.PType = type;
      this.IsConst = isconst;
      this.value_i = value ? 1 : 0;
      Debug.Assert(type == Primitive.Boolean || type == Primitive.Error);
    }

    public void Set (Primitive type, decimal value, bool isconst = true)
    {
      this.PType = type;
      this.IsConst = isconst;
      this.value_f = value;
      System.Diagnostics.Debug.Assert(this.IsNumber);
    }

    public static bool TypeIsNumber (Primitive ptype)
    {
      return ptype >= Primitive.Float && ptype <= Primitive.Decimal;
    }

    public static bool TypeIsInteger (Primitive ptype)
    {
      return ptype >= Primitive.I8 && ptype <= Primitive.U64;
    }

    public static bool TypeIsSigned (Primitive ptype)
    {
      return ptype >= Primitive.I8 && ptype <= Primitive.I64;
    }

    public static bool TypeIsUnsigned (Primitive ptype)
    {
      return ptype >= Primitive.U8 && ptype <= Primitive.U64;
    }

    public override string ToString ()
    {
      string value = "!?";
      if (this.IsConst && this.PType == Primitive.U64)
        value = ((ulong)this.value_i).ToString();
      else if (this.IsConst && this.PType == Primitive.String)
        value = this.value_s;
      else if (this.IsConst && this.PType == Primitive.Pointer)
        value = "@" + this.value_i.ToString("X8");
      else if (this.IsConst && this.PType == Primitive.Reference)
        value = "#" + this.value_i.ToString("X8");
      else if (this.IsConst && this.PType == Primitive.Boolean)
        value = this.value_i != 0 ? "True" : "False";
      else if (this.IsConst && this.IsNumber)
        value = this.value_f.ToString();
      else if (this.IsConst && this.IsInteger)
        value = this.value_i.ToString();
      return value;
    }
  }

  class Operand : Variant
  {
    static readonly int[] priorities_ = new int[(int)Operator.__Count];
    static readonly string[] litterals_ = new string[(int)Operator.__Count];

    static Operand ()
    {
      priorities_[(int)Operator.Not] = 3;
      priorities_[(int)Operator.BitwiseNot] = 3;
      priorities_[(int)Operator.IncPfx] = 2;
      priorities_[(int)Operator.DecPfx] = 2;
      priorities_[(int)Operator.Mul] = 5;
      priorities_[(int)Operator.Div] = 5;
      priorities_[(int)Operator.Mod] = 5;
      priorities_[(int)Operator.Add] = 6;
      priorities_[(int)Operator.Sub] = 6;
      priorities_[(int)Operator.BitwiseShiftLeft] = 7;
      priorities_[(int)Operator.BitwiseShiftRight] = 7;
      priorities_[(int)Operator.Less] = 8;
      priorities_[(int)Operator.More] = 8;
      priorities_[(int)Operator.LessEq] = 8;
      priorities_[(int)Operator.MoreEq] = 8;
      priorities_[(int)Operator.Equals] = 9;
      priorities_[(int)Operator.NotEquals] = 9;
      priorities_[(int)Operator.BitwiseAnd] = 10;
      priorities_[(int)Operator.BitwiseXor] = 11;
      priorities_[(int)Operator.BitwiseOr] = 12;
      priorities_[(int)Operator.And] = 13;
      priorities_[(int)Operator.Or] = 14;

      litterals_[(int)Operator.Not] = "!";
      litterals_[(int)Operator.BitwiseNot] = "~";
      litterals_[(int)Operator.IncPfx] = "++";
      litterals_[(int)Operator.DecPfx] = "++";
      litterals_[(int)Operator.Mul] = "*";
      litterals_[(int)Operator.Div] = "/";
      litterals_[(int)Operator.Mod] = "%";
      litterals_[(int)Operator.Add] = "+";
      litterals_[(int)Operator.Sub] = "-";
      litterals_[(int)Operator.BitwiseShiftLeft] = "<<";
      litterals_[(int)Operator.BitwiseShiftRight] = ">>";
      litterals_[(int)Operator.Less] = "<";
      litterals_[(int)Operator.More] = ">";
      litterals_[(int)Operator.LessEq] = "<=";
      litterals_[(int)Operator.MoreEq] = ">=";
      litterals_[(int)Operator.Equals] = "==";
      litterals_[(int)Operator.NotEquals] = "!=";
      litterals_[(int)Operator.BitwiseAnd] = "&";
      litterals_[(int)Operator.BitwiseXor] = "^";
      litterals_[(int)Operator.BitwiseOr] = "|";
      litterals_[(int)Operator.And] = "&&";
      litterals_[(int)Operator.Or] = "||";

    }

    public Operand (object token, Primitive ptype, long value)
    {
      this.Token = token;
      this.Operator = Operator.Operand;
      this.Set(ptype, value);
    }

    public Operand (object token, Primitive ptype, bool value)
    {
      this.Token = token;
      this.Operator = Operator.Operand;
      this.Set(ptype, value);
    }

    public Operand (object token, Primitive ptype, decimal value)
    {
      this.Token = token;
      this.Operator = Operator.Operand;
      this.Set(ptype, value);
    }

    public Operand (object token, Operator op)
    {
      this.Token = token;
      this.PType = Primitive.Undefined;
      this.Operator = op;
      System.Diagnostics.Debug.Assert(op != Operator.Operand);
    }

    public object Token { get; private set; }
    public Operator Operator { get; private set; }
    public Operand Left { get; private set; }
    public Operand Right { get; private set; }
    public Operand Parent { get; private set; }
    public string Code { get; set; }
    public int Version { get; set; }
    public string Name { get { return Code + Version; } }

    public int OperandCount
    {
      get
      {
        if (Operator <= Operator.Operand)
          return 0;
        if (Operator < Operator.__Binary)
          return 1;
        return 2;
      }
    }

    public int Priority
    {
      get
      {
        if (Operator == Operator.Parenthese)
          return 99;
        if (Operator == Operator.Operand)
          return 0;
        return priorities_[(int)Operator];
      }
    }

    public new string Litteral
    {
      get
      {
        if (Operator == Operator.Parenthese)
          return "(";
        if (Operator == Operator.Operand) 
          return "("+this.PType+")" + this.Value;
        return litterals_[(int)Operator];
      }
    }

    public SSA SSA ()
    {
      return null;
    }

    public bool Resolve (Operand left, Operand right)
    {
      this.Left = left;
      this.Right = right;
      left.Parent = this;
      right.Parent = this;

      OperatorImplem action = OperatorAction.Get(this.Operator, left.PType, right.PType);
      if (action == null) {
        this.PType = Primitive.Error;
        this.IsConst = true;
        // Push an error or what !?
      } else
        action(this, left, right);

      return false;
    }

    public override string ToString ()
    {
      return this.Operator.ToString() + "{" + this.PType.ToString() + "}" + "(" + this.Value + ")";
    }


  }

  ///// <summary>Represent a single word on an expression.</summary>
  //public class Operand
  //{
  //  }
  //  public string Name { get { return Code + Version; } }
  //  public string Code = "?";
  //  public int Version = 0;

  //  public string SSA(bool ignoreConst)
  //  {
  //    if (ignoreConst && this.IsConst)
  //      return Name + " <- (" + this.Type + ")" + this.Value;
  //    if (this.Operation == Opcode.Operand && (Parent == null || (Parent != null && this == Parent.Left)))
  //      return Name + " <- (" + this.Type + ")" + this.Value;
  //    if (this.OperandCount == 2) {
  //      Code = Left.Code;
  //      Version = Left.Version + 1;
  //      return Name + " <- " + Left.Name + Op + (Right.Operation != Opcode.Operand ? Right.Name : "(" + this.Type + ")" + Right.Value);
  //    }
  //    if (this.OperandCount == 1) {
  //      if (Right.Operation != Opcode.Operand) {
  //        Code = Right.Code;
  //        Version = Right.Version + 1;
  //        return Name + " <- " + Op + Right.Name;
  //      }
  //      return Name + " <- (" + Right.Type + ")" + Right.Value;
  //    }
  //    return string.Empty;
  //  }
  

}
