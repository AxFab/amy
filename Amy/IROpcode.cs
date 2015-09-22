using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmySuite
{
  enum IRType
  {
    Undefined,
    Variable,
    Register,
    RAddress,
    Address,
    Number,
    AddrScale,
  }

  enum IRPrimitives
  {
    _S8,
    _U8,
    _S16,
    _U16,
    _S32,
    _U32,
    _S64,
    _U64,
    _F32,
    _F64,
    _LP,
  }

  class IROperand
  {
    public IRType Type;
    public IRPrimitives PType;
    public int RegNo;
    public int Mul;
    public int RegDp;
    public long Value;
    public int Version;
    public string Literal;

    public override string ToString ()
    {
      return Literal;
    }

    public string IntelForm
    {
      get
      {
        switch (Type) {
          case IRType.Register:
            return ((Intelx86Register)RegNo).ToString().ToLower();
          case IRType.RAddress:
            return "[" + ((Intelx86Register)RegNo).ToString().ToLower() + "]";
          case IRType.Number:
            return Value.ToString();
          case IRType.Address:
            return "[" + Value + "]";

          default:
            return "??";
        }
      }
    }

    public string ATnTForm
    {
      get
      {
        switch (Type) {
          case IRType.Register:
            return "%" + ((Intelx86Register)RegNo).ToString().ToLower();
          case IRType.RAddress:
            return "(%" + ((Intelx86Register)RegNo).ToString().ToLower() + ")";
          case IRType.Number:
            return "$" + Value;
          case IRType.Address:
            return Value.ToString();

          default:
            return "??";
        }
      }
    }


  }

  class IROpcode
  {
    public IROperand output;
    public IROperand srcRight;
    public IROperand srcLeft;
    public string opcode;
    public IROpcode next;
    public IROpcode branch;

    public override string ToString ()
    {
      return output + " <-- " + srcRight + "  -" + opcode + "-  " + srcLeft;
      // return output + " <-- " + opcode + "-  " + srcLeft;
      // return  "   ? " + srcRight + "  -" + opcode + "-  " + srcLeft;
    }

    public string ATnTForm
    {
      get
      {
        string instr = string.Empty;

        instr += this.opcode.ToLower();
        if (this.output.PType == IRPrimitives._U8 || this.output.PType == IRPrimitives._S8)
          instr += 'b';
        else if (this.output.PType == IRPrimitives._U16 || this.output.PType == IRPrimitives._S16)
          instr += 'w';
        else if (this.output.PType == IRPrimitives._U32 || this.output.PType == IRPrimitives._S32)
          instr += 'l';
        instr += ' ';
        for (int i = instr.Length; i < 7; ++i) {
          instr += ' ';
        }

        if (srcLeft != null)
          instr += srcLeft.ATnTForm + "," + srcRight.ATnTForm;
        else if (srcRight != null)
          instr += srcRight.ATnTForm;
        return instr;
      }
    }
  }
}
