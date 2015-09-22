using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amy
{
  public class SSAFlow
  {
    SSA top;
    SSA current;

    public void Push(SSA ssa)
    {
      ssa.Previous = current;
      if (top == null)
        top = ssa;
      else
        current.Next1 = ssa;
      current = ssa;

      // Console.WriteLine(this.ToIntelASM(ssa));
    }

    public string ToIntelASM (SSA ssa)
    {
      string reg1 = "e" + ssa.Name + "x";
      string op1 = ssa.Operand1.Value;
      if ((ssa.Operand1 as Operand).Version > 0)
        op1 = "e" + (ssa.Operand1 as Operand).Code + "x";

      if (ssa.Operator == Amy.Operator.Assign)
        return "MOV " + reg1 + ", " + op1;
      if (ssa.Operator == Amy.Operator.Add)
        return "ADD " + reg1 + ", " + op1;
      if (ssa.Operator == Amy.Operator.Sub)
        return "SUB " + reg1 + ", " + op1;
      if (ssa.Operator == Amy.Operator.Mul)
        return "MUL " + reg1 + ", " + op1;
      if (ssa.Operator == Amy.Operator.Div)
        return "DIV " + reg1 + ", " + op1;

      return "!?";
    }

  }

  public class SSA
  {
    public string Name { get; private set; }
    public int Version { get; private set; }
    public SSA Previous { get; internal set; }
    public SSA Next1 { get; internal set; }
    public SSA Next2 { get; internal set; }
    public SSA PrevReg { get; internal set; }
    public Operator Operator { get; private set; }
    internal Variant Operand1 { get; private set; }
    internal Variant Operand2 { get; private set; }

    internal SSA (Operand operand, bool ignoreConst)
    {
      if ((ignoreConst && operand.IsConst) ||
        (operand.Operator == Operator.Operand && (operand.Parent == null ||
        (operand.Parent != null && operand == operand.Parent.Left)))) {
        this.Operator = Operator.Assign;
        this.Name = operand.Code;
        this.Version = 0;
        this.Operand1 = operand;
        operand.Version = 0;

      } else if (operand.OperandCount == 1) {
        this.Operator = operand.Operator;
        this.Name = operand.Right.Code;
        this.Version = operand.Right.Version + 1;

      } else if (operand.OperandCount == 2) {
        this.Operator = operand.Operator;
        this.Name = operand.Left.Code;
        this.Version = operand.Left.Version + 1;
        this.Operand1 = operand.Right;

      }

      operand.Code = this.Name;
      operand.Version = this.Version;
    }



    public override string ToString ()
    {
      if (this.Operator == Amy.Operator.Assign)
        return Name + Version + " <- " + this.Operand1.Litteral;
      if (this.Operator < Operator.__Binary)
        return Name + Version + " <- " + this.Operator + " " + this.Operand1;
      return Name + Version + " <- " + this.Name + (Version - 1) + " " + this.Operator + " " + this.Operand1;
    }

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
}
