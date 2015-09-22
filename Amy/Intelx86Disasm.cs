using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmySuite
{
  class Intelx86Disasm
  {
    private byte[] data;
    private int idx;
    private int length;

    public Intelx86Disasm (byte[] data)
    {
      this.data = data;
      this.idx = 0;
      this.length = 0;

      this.Parse();
    }

    public static char HexDigit (int b)
    {
      return (char)(b < 10 ? b + '0' : b + 'a' - 10);
    }

    public static string Hex (byte b)
    {
      return "0x" + HexDigit((b >> 4) & 0xf) + HexDigit(b & 0xf);
    }

    public static string Hex (uint b)
    {
      string s = "0x";
      uint c;
      for (var i = 0; i < 8; ++i) {
        c = b;
        c = c >> 28;
        s += HexDigit((int)(c & 0xf));
        b = b << 4;
      }
      return s;
    }

    public static IROperand OperandRegister (int reg)
    {
      return new IROperand()
      {
        Type = IRType.Register,
        RegNo = reg,
        PType = IRPrimitives._U32,
        Literal = ((Intelx86Register)reg).ToString()
      };
    }

    public static IROperand OperandValue (long value, IRPrimitives size)
    {
      return new IROperand()
      {
        Type = IRType.Number,
        PType = size,
        Literal = value.ToString(),
        Value = value
      };
    }

    public static IROperand OperandAddress (byte[] data, int idx)
    {
      int pos = 0;
      long result = 0;
      for (int i = 0; i < 4; ++i) {
        byte by = data[idx + i];
        result |= (long)(long)((int)by << pos);
        pos += 8;
      }

      return new IROperand()
      {
        Type = IRType.Address,
        PType = IRPrimitives._U32,
        Literal = "[" + Hex((uint)result) + "]",
        Value = result
      };
    }

    public static IROpcode Opcode (string op, IROperand dest, IROperand srcl, IROperand srcr)
    {
      return new IROpcode()
      {
        opcode = op,
        srcLeft = srcl,
        srcRight = srcr,
        output = dest,
      };
    }

    #region Extraction

    #region Extraction E

    public IROperand extractEv (byte b)
    {
      int mod = b >> 6;
      int rm = b & 0x7;
      long value;
      switch (mod) {
        case 0:
          if (rm == 4) {
            return null;
          } else if (rm == 5) {
            length += 4;
            return OperandAddress(data, idx + length - 4);
            // value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)  | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
            // Length += 4;
            // return x86Operand.NewAddress(value);
          } else {
            return null;
            // return x86Operand.NewMemory((int)IntelRegister.Eax + rm);
          }
        case 1:
          break;
        case 2:
          break;
        case 3:
          return OperandRegister(rm);
      }
      return null;
    }

    #endregion

    #region Extraction L

    private IROperand extractLb ()
    {
      length++;
      return OperandValue((long)data[idx + length - 1], IRPrimitives._U8);
    }

    #endregion

    #endregion

    public object Read_00_to_40 ()
    {
      return null;
    }

    public object Read_4x ()
    {
      IROperand s = OperandRegister((int)Intelx86Register.Eax + (data[idx] & 0x7));
      length++;
      return Opcode((data[idx] < 0x48) ? "inc" : "dec", s, null, s);
    }

    public object Read_5x ()
    {
      IROperand s = OperandRegister((int)Intelx86Register.Eax + (data[idx] & 0x7));
      length++;
      return Opcode((data[idx] < 0x58) ? "push" : "pop", s, null, s);
    }

    public object Read_6x ()
    {
      return null;
    }
    public object Read_7x ()
    {
      return null;
    }
    public object Read_8x ()
    {
      string[] op = { "add", "a1", "a2", "a3", "a4", "a5", "a6", "cmp" };
      int reg;
      IROperand d, s;
      byte b = data[idx + length];
      byte c = data[idx + length + 1];
      switch (b) {
        case 0x80:
          break;
        case 0x81:
          break;
        case 0x82:
          break;
        case 0x83:
          length += 2;
          reg = (c >> 3) & 0x7;
          d = extractEv(c);
          s = extractLb();
          return Opcode(op[reg], d, s, d);
      }
      return null;
    }
    public object Read_9x ()
    {
      return null;
    }
    public object Read_Ax ()
    {
      return null;
    }
    public object Read_Bx ()
    {
      return null;
    }
    public object Read_Cx ()
    {
      return null;
    }
    public object Read_Dx ()
    {
      return null;
    }
    public object Read_Ex ()
    {
      return null;
    }
    public object Read_Fx ()
    {
      return null;
    }


    public void Parse ()
    {
      uint physBase = 0x8062ca0;
      object r = null;
      for (; ; ) {
        r = Read();
        if (r == null)
          return;
        Console.Write(Hex((uint)idx + physBase).Substring(2) + ":       ");
        for (var i = 0; i < length; ++i) {
          Console.Write(Hex(data[idx + i]).Substring(2) + " ");
        }
        for (var i = length; i < 8; ++i) {
          Console.Write("   ");
        }

        Console.WriteLine(((IROpcode)r).ATnTForm);
        idx += length;
        length = 0;
      }
    }

    public object Read ()
    {
      byte b = data[idx];
      //string bc = Hex(b);

      switch ((b >> 4) & 0xf) {
        case 0:
        case 1:
        case 2:
        case 3:
          return Read_00_to_40();
        case 4:
          return Read_4x();
        case 5:
          return Read_5x();
        case 6:
          return Read_6x();
        case 7:
          return Read_7x();
        case 8:
          return Read_8x();
        case 9:
          return Read_9x();
        case 10:
          return Read_Ax();
        case 11:
          return Read_Bx();
        case 12:
          return Read_Cx();
        case 13:
          return Read_Dx();
        case 14:
          return Read_Ex();
        case 15:
          return Read_Fx();
        default:
          return null;
      }
    }

  }
}
