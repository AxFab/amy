using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmySuite
{
  public static class Hexa
  {

    /// <summary>
    /// Transform a long integer into a hexa string.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="digits">The minimum digits number to print.</param>
    /// <param name="negative">Is negative value have to be printed with minus sign.</param>
    /// <param name="suffix">Is the number should be preceed by '0x' characters.</param>
    /// <returns>A string representation of the number into hexadecimal base.</returns>
    public static String ToString (long value, int digits = 8, bool negative = false, bool suffix = true)
    {
      String s = String.Empty;
      // Print at least one character
      if (digits > 0) {
        digits--;
      }

      // If negative print minus
      if (negative && value < 0) {
        s += "-";
        value = -value;
      }

      // Print suffix
      if (suffix) {
        s += "0x";
      }

      // Check if digits count is enough
      while (value >> (digits * 4) > 0xF) {
        digits++;
      }

      // Print digits
      for (int i = digits; i >= 0; --i) {
        int digit = (int)(value >> (i * 4)) & 0xF;
        s += (char)(digit < 10 ? digit + '0' : digit + 'a' - 10);
      }

      return s;
    }
  }

  public enum OperatorSize
  {
    /// <summary>The size is defined by opcode or architecture</summary>
    Idem,
    /// <summary>The size is a byte (8 bits)</summary>
    Byte,
    /// <summary>The size is a word (16 bits)</summary>
    Word,
    /// <summary>The size is a double word (32 bits)</summary>
    DWord,
    /// <summary>The size is a quad word (64 bits)</summary>
    QWord,
    /// <summary>The size is a T-word (128 bits)</summary>
    TWord,
    /// <summary>The size is a O-word (256 bits)</summary>
    OWord,
    /// <summary>The size is a Y-word (512 bits)</summary>
    YWord
  }

  public enum OperandType
  {
    Unknown,
    Memory,
    Register,
    Value,
    Address,
    Offset,
    MemoryScale,
    MemoryOffset,
    MemoryFull,
    Displacement
  }

  enum Intelx86Register
  {
    None,
    Al,
    Cl,
    Dl,
    Bl,
    Ah,
    Ch,
    Dh,
    Bh,
    Ax,
    Cx,
    Dx,
    Bx,
    Sp,
    Bp,
    Si,
    Di,
    Eax,
    Ecx,
    Edx,
    Ebx,
    Esp,
    Ebp,
    Esi,
    Edi,
    Rax,
    Rcx,
    Rdx,
    Rbx,
    Rsp,
    Rbp,
    Rsi,
    Rdi,
    Mm0,
    Mm1,
    Mm2,
    Mm3,
    Mm4,
    Mm5,
    Mm6,
    Mm7,
    Xmm0,
    Xmm1,
    Xmm2,
    Xmm3,
    Xmm4,
    Xmm5,
    Xmm6,
    Xmm7,
    Es,
    Cs,
    Ss,
    Ds,
    Fs,
    Gs,
  }

  enum Intelx86Opcode
  {
    Unknown,
    Bad,
    Segment,

    Nop,
    FWait,

    Pusha,
    Popa,
    Cwtl,
    Cltd,
    Pushf,
    Popf,
    Sahf,
    Lahf,
    Daa,
    Aaa,
    Das,
    Aas,

    Leave,
    Ret,
    Retf,
    LRet,
    Int3,
    Icebp,
    Cmc,
    Hlt,

    Clc,
    Stc,
    Cli,
    Sti,
    Cld,
    Std,

    Push,
    Pop,
    Inc,
    Dec,
    Call,
    Int,
    Jmp,
    Ret_,
    Retf_,
    LRet_,
    Rol_,
    Ror_,
    Rcl_,
    Rcr_,
    Shl_,
    Shr_,
    Sar_,

    Jo,
    Jno,
    Jb,
    Jae,
    Je,
    Jne,
    Jbe,
    Ja,
    Js,
    Jns,
    Jp,
    Jnp,
    Jl,
    Jge,
    Jle,
    Jg,

    Add,
    Or,
    Adc,
    Sbb,
    And,
    Sub,
    Xor,
    Cmp,
    Mov,
    Lea,
    Test,
    Xchg,
    Bound,
    Arpl,
    Scas,
    Movs,
    Cmps,
    Stos,
    Lods,

    Rol,
    Ror,
    Rcl,
    Rcr,
    Shl,
    Shr,
    Sar,

    Ins,
    Outs,

    LCall,
    Enter,

    Outbound,
    IMul,
  }

  class x86Operand
  {
    public static string FunctionName = "_atexit";

    public OperandType Type { get; protected set; }
    public int Register { get; set; }
    public long Value { get; protected set; }
    public int SdRegister { get; protected set; }
    public long SdValue { get; protected set; }

    private x86Operand (OperandType type, int register, long value)
    {
      this.Type = type;
      this.Register = register;
      this.Value = value;
      this.SdRegister = 0;
      this.SdValue = 0;
    }

    private x86Operand (OperandType type, int register, long value, int sdregister, long sdvalue)
    {
      this.Type = type;
      this.Register = register;
      this.Value = value;
      this.SdRegister = sdregister;
      this.SdValue = sdvalue;
    }

    public static string RegString (int register)
    {
      return ((Intelx86Register)register).ToString().ToLower();
    }



    public static x86Operand NewMemory (int register)
    {
      return new x86Operand(OperandType.Memory, register, 0);
    }

    public static x86Operand NewRegister (int register)
    {
      return new x86Operand(OperandType.Register, register, 0);
    }

    public static x86Operand NewValue (long value)
    {
      return new x86Operand(OperandType.Value, 0, value);
    }

    public static x86Operand NewAddress (long value)
    {
      return new x86Operand(OperandType.Address, 0, value);
    }

    public static x86Operand NewOffset (int register, long value)
    {
      return new x86Operand(OperandType.Offset, register, value);
    }

    public static x86Operand NewMemoryScale (int reg, int sreg, int scale)
    {
      return new x86Operand(OperandType.MemoryScale, reg, 0, sreg, scale);
    }

    public static x86Operand NewMemoryOffset (int sreg, int scale, long disp)
    {
      return new x86Operand(OperandType.MemoryOffset, 0, disp, sreg, scale);
    }

    public static x86Operand NewMemoryFull (int reg, int sreg, int scale, long disp)
    {
      return new x86Operand(OperandType.MemoryFull, reg, disp, sreg, scale);
    }

    public static x86Operand NewDisplacement (long value)
    {
      return new x86Operand(OperandType.Displacement, 0, value);
    }

    public String IntelWriting
    {
      get
      {
        switch (Type) {
          case OperandType.Unknown:
          default:
            return "??";

          case OperandType.Memory:
            return "[" + x86Operand.RegString(Register) + "]";
          case OperandType.Register:
            return x86Operand.RegString(Register);
          case OperandType.Value:
            return Hexa.ToString(Value, 0, false);
          case OperandType.Address:
            return "[" + Hexa.ToString(Value, 0, false) + "]";
          case OperandType.Offset:
            return "[" + x86Operand.RegString(Register) + " + " + Hexa.ToString(Value, 0, false) + "]";

          case OperandType.MemoryScale:
          case OperandType.MemoryOffset:
          case OperandType.MemoryFull:
            return "[ _ + _ * _ + 0x_]";

          case OperandType.Displacement:
            return Hexa.ToString(Value, 0, true, false);
        }
      }
    }

    public String ATNTWriting
    {
      get
      {
        switch (Type) {
          case OperandType.Unknown:
          default:
            return "??";

          case OperandType.Memory:
            return "(%" + x86Operand.RegString(Register) + ")";
          case OperandType.Register:
            return "%" + x86Operand.RegString(Register);
          case OperandType.Value:
            return "$" + Hexa.ToString(Value, 0, true);
          case OperandType.Address:
            return Hexa.ToString(Value, 0, false);
          case OperandType.Offset:
            return Hexa.ToString(Value, 0, true) + "(%" + x86Operand.RegString(Register) + ")";

          case OperandType.MemoryScale:
            if (SdRegister == (int)Intelx86Register.Esp)
              return "(%" + x86Operand.RegString(Register) + ",%eiz," + SdValue + ")";
            else
              return "(%" + x86Operand.RegString(Register) + ",%" + x86Operand.RegString(SdRegister) + "," + SdValue + ")";

          case OperandType.MemoryOffset:
            if (SdRegister == (int)Intelx86Register.Esp)
              return Hexa.ToString(Value, 0, true) + "(,%eiz," + SdValue + ")";
            else
              return Hexa.ToString(Value, 0, true) + "(,%" + x86Operand.RegString(SdRegister) + "," + SdValue + ")";

          case OperandType.MemoryFull:
            if (SdRegister == (int)Intelx86Register.Esp)
              return Hexa.ToString(Value, 0, true) + "(%" + x86Operand.RegString(Register) + ",%eiz," + SdValue + ")";
            else
              return Hexa.ToString(Value, 0, true) + "(%" + x86Operand.RegString(Register) + ",%" + x86Operand.RegString(SdRegister) + "," + SdValue + ")";

          case OperandType.Displacement:
            if (Value != 0) {
              return Hexa.ToString(Value, 0, false, false) + " <" + FunctionName + "+" + Hexa.ToString(Value, 0, false) + ">";
            } else {
              return Hexa.ToString(Value, 0, false, false) + " <" + FunctionName + ">";
            }

        }
      }
    }
  }

  class x86Operator
  {
    public string OperatorName { get; protected set; }
    public Intelx86Opcode OperatorCode { get; protected set; }
    public int OperandCount { get { return this.Operands.Length; } }
    public x86Operand[] Operands { get; protected set; }
    public OperatorSize DataSize { get; set; }

    public string Prefix;

    public x86Operand Source
    {
      get
      {
        if (this.OperandCount > 0)
          return this.Operands[0];
        return null;
      }
      set
      {
        if (this.OperandCount <= 0)
          throw new IndexOutOfRangeException();
        this.Operands[0] = value;
      }
    }

    public x86Operand Destination
    {
      get
      {
        if (this.OperandCount > 1)
          return this.Operands[1];
        return null;
      }
      set
      {
        if (this.OperandCount <= 1)
          throw new IndexOutOfRangeException();
        this.Operands[1] = value;
      }
    }

    public x86Operand ThirdOpad
    {
      get
      {
        if (this.OperandCount > 2)
          return this.Operands[2];
        return null;
      }
      set
      {
        if (this.OperandCount <= 2)
          throw new IndexOutOfRangeException();
        this.Operands[2] = value;
      }
    }



    public x86Operator (Intelx86Opcode opcode)
      : this(opcode, OperatorSize.Idem, Intelx86Register.None)
    {
    }

    public x86Operator (Intelx86Opcode opcode, OperatorSize size)
      : this(opcode, size, Intelx86Register.None)
    {
    }

    public x86Operator (Intelx86Opcode opcode, OperatorSize size, Intelx86Register segment)
    {
      int count = (opcode < Intelx86Opcode.Push ? 0 : (opcode < Intelx86Opcode.Add ? 1 : (opcode < Intelx86Opcode.Outbound ? 2 : 3)));
      this.OperatorCode = opcode;
      this.OperatorName = opcode.ToString();
      this.Operands = new x86Operand[count];
      this.DataSize = size;
      this.Segment = segment;
    }

    public Intelx86Register Segment { get; set; }
    public Intelx86Register SegmentSrc { get; set; }
    public int Length { get; set; }


    /// <summary>
    /// Get the assembly code for this instruction into the Intel code style
    /// </summary>
    public String IntelWriting
    {
      get
      {
        switch ((Intelx86Opcode)this.OperatorCode) {
          case Intelx86Opcode.Unknown:
            return "...";
          case Intelx86Opcode.Bad:
            return "(bad)  ";
          case Intelx86Opcode.Segment:
            return Segment.ToString().ToLower();
          default:
            if (this.OperatorCode < Intelx86Opcode.Pusha)
              return this.OperatorName.ToLower();
            string instr = this.OperatorName.ToLower();
            if (this.DataSize == OperatorSize.Byte)
              instr += " byte";
            else if (this.DataSize == OperatorSize.Word)
              instr += " word";
            else if (this.DataSize == OperatorSize.DWord)
              instr += " dword";
            for (int i = instr.Length; i < 7; ++i) {
              instr += ' ';
            }
            if (this.OperatorCode < Intelx86Opcode.Push)
              return instr;
            if (this.OperatorCode < Intelx86Opcode.Add)
              return instr + (Source == null ? "??" : (Source as x86Operand).IntelWriting);

            if (Segment == Intelx86Register.None)
              return instr + (Destination == null ? "??" : (Destination as x86Operand).IntelWriting) + ", " + (Source == null ? "??" : (Source as x86Operand).IntelWriting);
            return instr + Segment.ToString().ToLower() + ":" + (Destination == null ? "??" : (Destination as x86Operand).IntelWriting) + ", " + (Source == null ? "??" : (Source as x86Operand).IntelWriting);
        }
      }
    }

    /// <summary>
    /// Get the assembly code for this instruction into the AT&T code style
    /// </summary>
    public String ATNTWriting
    {
      get
      {
        switch ((Intelx86Opcode)this.OperatorCode) {
          case Intelx86Opcode.Unknown:
            return "...";
          case Intelx86Opcode.Bad:
            return "(bad)  ";
          case Intelx86Opcode.Segment:
            return Segment.ToString().ToLower();
          default:
            if (this.OperatorCode < Intelx86Opcode.Pusha) {
              return this.OperatorName.ToLower();
            }

            string instr = string.Empty;
            if (!string.IsNullOrEmpty(this.Prefix))
              instr += Prefix + " ";
            instr += this.OperatorName.ToLower();
            instr = instr.Replace("_", "");
            if (this.DataSize == OperatorSize.Byte)
              instr += 'b';
            else if (this.DataSize == OperatorSize.Word)
              instr += 'w';
            else if (this.DataSize == OperatorSize.DWord)
              instr += 'l';
            instr += ' ';
            for (int i = instr.Length; i < 7; ++i) {
              instr += ' ';
            }

            if (this.OperatorCode < Intelx86Opcode.Push)
              return instr;
            if (this.OperatorCode < Intelx86Opcode.Add)
              return instr + (Source == null ? "??" : (Source as x86Operand).ATNTWriting);
            if (this.OperatorCode < Intelx86Opcode.Outbound) {
              if (Segment == Intelx86Register.None && SegmentSrc == Intelx86Register.None)
                return instr + (Source == null ? "??" : (Source as x86Operand).ATNTWriting) + "," + (Destination == null ? "??" : (Destination as x86Operand).ATNTWriting);

              if (Segment == Intelx86Register.None)
                return instr + "%" + SegmentSrc.ToString().ToLower() + ":" + (Source as x86Operand).ATNTWriting + "," + (Destination as x86Operand).ATNTWriting;

              if (SegmentSrc == Intelx86Register.None)
                return instr + (Source as x86Operand).ATNTWriting + "," + "%" + Segment.ToString().ToLower() + ":" + (Destination as x86Operand).ATNTWriting;

              return instr + "%" + SegmentSrc.ToString().ToLower() + ":" + (Source as x86Operand).ATNTWriting + "," + "%" + Segment.ToString().ToLower() + ":" + (Destination as x86Operand).ATNTWriting;

              //if (Destination.Type == OperandType.Memory)
              //    return instr + (Source as x86Operand).ATNTWriting + "," + "%" + Segment.ToString().ToLower() + ":" + (Destination as x86Operand).ATNTWriting;

              //if (Source.Type == OperandType.Memory)
              //    return instr + "%" + Segment.ToString().ToLower() + ":" + (Source as x86Operand).ATNTWriting + "," + (Destination == null ? "??" : (Destination as x86Operand).ATNTWriting);


              // return instr + (Source == null ? "??" : (Source as x86Operand).ATNTWriting) + ",%" + Segment.ToString().ToLower() + ":" + (Destination == null ? "??" : (Destination as x86Operand).ATNTWriting);
            }
            return instr + (Source == null ? "??" : (Source as x86Operand).ATNTWriting) + "," + (Destination == null ? "??" : (Destination as x86Operand).ATNTWriting) + "," + (ThirdOpad == null ? "??" : (ThirdOpad as x86Operand).ATNTWriting);
        }
      }
    }


  }

  class Intelx86
  {
    private int X { get { return Offset + Length; } }
    public byte[] Buffer { get; set; }
    public int Offset { get; private set; }
    public int Length { get; private set; }

    public Intelx86 (byte[] buffer)
    {
      this.Buffer = buffer;
      this.Offset = 0;
      this.Length = 0;
    }

    public x86Operator Next ()
    {
      this.Offset += Length;
      this.Length = 0;
      if (X >= Buffer.Length)
        return null;
      x86Operator _Operator = Read();
      // if (_Operator.OperatorCode == Intelx86Opcode.Bad)
      // {
      //    Length = 1;
      // }
      if (_Operator == null) {
        this.Length = 0;
        return null;
      }
      _Operator.Length = this.Length;
      if (X <= Buffer.Length)
        return _Operator;
      return null;
    }

    public void Echo (long virtualAddress)
    {
      for (; ; ) {
        x86Operator instr = Next() as x86Operator;
        String amyDism = " " + Hexa.ToString(virtualAddress, 8, false, false) + ":\t";
        if (instr == null) {
          for (int i = 0; i < Math.Min(Buffer.Length - X, 7); ++i)
            amyDism += Hexa.ToString(Buffer[i + X], 2, false, false) + " ";
          Console.WriteLine(amyDism);
          return;
        }
        for (int i = 0; i < Math.Min(instr.Length, 7); ++i)
          amyDism += Hexa.ToString(Buffer[i + X - instr.Length], 2, false, false) + " ";
        for (int i = instr.Length; i < 7; ++i)
          amyDism += "   ";
        amyDism += "\t";

        amyDism += instr.ATNTWriting;
        virtualAddress += instr.Length;
        Console.WriteLine(amyDism);
      }
    }

    #region Extract Args Byte

    private x86Operand extractSIB (byte b, int mod)
    {
      int scale = b >> 6;
      int bas = b & 0x7;
      int index = (b >> 3) & 0x7;

      if (index == 4 && bas == 4 && scale == 0) {
        if (mod == 0)
          return x86Operand.NewMemory((int)Intelx86Register.Esp);
        else if (mod == 1) {
          long value = (long)Buffer[X];
          Length++;
          return x86Operand.NewOffset((int)Intelx86Register.Esp, value);
        } else if (mod == 2) {
          long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
              | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
          Length += 4;
          return x86Operand.NewOffset((int)Intelx86Register.Esp, value);
        } else
          throw new NotImplementedException();
      } else if (bas == 5) {
        if (mod == 0) {
          long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
              | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
          Length += 4;
          return x86Operand.NewMemoryOffset((int)Intelx86Register.Eax + index, 1 << scale, value);
        } else if (mod == 1) {
          long value = (long)Buffer[X];
          Length++;
          return x86Operand.NewMemoryFull((int)Intelx86Register.Ebp, (int)Intelx86Register.Eax + index, 1 << scale, value);
        } else if (mod == 2) {
          long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
              | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
          Length += 4;
          return x86Operand.NewMemoryFull((int)Intelx86Register.Ebp, (int)Intelx86Register.Eax + index, 1 << scale, value);
        } else {
          throw new NotImplementedException();
        }
      } else {
        if (mod == 0)
          return x86Operand.NewMemoryScale((int)Intelx86Register.Eax + bas, (int)Intelx86Register.Eax + index, 1 << scale);
        else if (mod == 1) {
          long value = (long)Buffer[X];
          Length++;
          return x86Operand.NewMemoryFull((int)Intelx86Register.Eax + bas, (int)Intelx86Register.Eax + index, 1 << scale, value);
        } else if (mod == 2) {
          long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
              | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
          Length += 4;
          return x86Operand.NewMemoryFull((int)Intelx86Register.Eax + bas, (int)Intelx86Register.Eax + index, 1 << scale, value);
        } else
          throw new NotImplementedException();
      }
    }

    private x86Operand extractEb (byte b)
    {
      int mod = b >> 6;
      int rm = b & 0x7;
      long value;
      switch (mod) {
        case 0:
          if (rm == 4) {
            b = Buffer[X];
            Length++;
            return extractSIB(b, mod);
          } else if (rm == 5) {
            value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
            Length += 4;
            return x86Operand.NewAddress(value);
          } else {
            return x86Operand.NewMemory((int)Intelx86Register.Eax + rm);
          }
        case 1:
          if (rm == 4) {
            b = Buffer[X];
            Length++;
            return extractSIB(b, mod);
          }
          value = (sbyte)Buffer[X];
          Length++;
          return x86Operand.NewOffset((int)Intelx86Register.Eax + rm, value);
        case 2:
          if (rm == 4) {
            b = Buffer[X];
            Length++;
            return extractSIB(b, mod);
          }
          value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
              | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
          Length += 4;
          return x86Operand.NewOffset((int)Intelx86Register.Eax + rm, value);
        case 3:
          return x86Operand.NewRegister((int)Intelx86Register.Al + rm);
      }
      return null;
    }

    private x86Operand extractEv (byte b)
    {
      int mod = b >> 6;
      int rm = b & 0x7;
      long value;
      switch (mod) {
        case 0:
          if (rm == 4) {
            b = Buffer[X];
            Length++;
            return extractSIB(b, mod);
          } else if (rm == 5) {
            value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
            Length += 4;
            return x86Operand.NewAddress(value);
          } else {
            return x86Operand.NewMemory((int)Intelx86Register.Eax + rm);
          }
        case 1:
          if (rm == 4) {
            b = Buffer[X];
            Length++;
            return extractSIB(b, mod);
          }
          value = (sbyte)Buffer[X];
          Length++;
          return x86Operand.NewOffset((int)Intelx86Register.Eax + rm, value);
        case 2:
          if (rm == 4) {
            b = Buffer[X];
            Length++;
            return extractSIB(b, mod);
          }
          value = (int)(Buffer[X] | (Buffer[X + 1] << 8) | (Buffer[X + 2] << 16) | (Buffer[X + 3] << 24));
          Length += 4;
          return x86Operand.NewOffset((int)Intelx86Register.Eax + rm, value);
        case 3:
          return x86Operand.NewRegister((int)Intelx86Register.Eax + rm);
      }
      return null;
    }

    private x86Operand extractEw (byte b)
    {
      int mod = b >> 6;
      int rm = b & 0x7;
      if (mod < 3)
        return extractEv(b);
      return x86Operand.NewRegister((int)Intelx86Register.Ax + rm);
    }

    private x86Operand extractGb (byte b)
    {
      //int mod = b >> 6;
      //int rm = b & 0x7;
      int reg = (b >> 3) & 0x7;
      return x86Operand.NewRegister((int)Intelx86Register.Al + reg);
    }

    private x86Operand extractGv (byte b)
    {
      //int mod = b >> 6;
      //int rm = b & 0x7;
      int reg = (b >> 3) & 0x7;
      return x86Operand.NewRegister((int)Intelx86Register.Eax + reg);
    }

    private x86Operand extractGw (byte b)
    {
      //int mod = b >> 6;
      //int rm = b & 0x7;
      int reg = (b >> 3) & 0x7;
      return x86Operand.NewRegister((int)Intelx86Register.Ax + reg);
    }

    private x86Operand extractSw (byte b)
    {
      //int mod = b >> 6;
      //int rm = b & 0x7;
      int reg = (b >> 3) & 0x7;
      return x86Operand.NewRegister((int)Intelx86Register.Es + reg);
    }

    private x86Operand extractMa (byte b)
    {
      int mod = b >> 6;
      if (mod < 3)
        return extractEv(b);
      return null;
    }

    private x86Operand extractLz ()
    {
      long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
          | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
      Length += 4;
      return x86Operand.NewValue(value);
    }

    private x86Operand extractLb ()
    {
      long value = (long)Buffer[X];
      Length++;
      return x86Operand.NewValue(value);
    }

    private x86Operand extractLw ()
    {
      long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8);
      Length += 2;
      return x86Operand.NewValue(value);
    }

    private x86Operand extractOz ()
    {
      long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
          | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
      Length += 4;
      return x86Operand.NewAddress(value);
    }

    private x86Operand extractOb ()
    {
      long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
          | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
      Length += 4;
      return x86Operand.NewAddress(value);
    }

    private x86Operand extractOw ()
    {
      long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8);
      Length += 2;
      return x86Operand.NewAddress(value);
    }

    #endregion

    private x86Operator Read_00_to_40 (byte b)
    {
      x86Operator _Operator = null;
      long value;
      if ((b & 0x7) < 6) {
        _Operator = new x86Operator(Intelx86Opcode.Add + (b >> 3));
        switch (b & 0x7) {
          case 0:
            b = Buffer[X];
            Length++;
            _Operator.Destination = extractEb(b);
            _Operator.Source = extractGb(b);
            break;
          case 1:
            b = Buffer[X];
            Length++;
            _Operator.Destination = extractEv(b);
            _Operator.Source = extractGv(b);
            break;
          case 2:
            b = Buffer[X];
            Length++;
            _Operator.Destination = extractGb(b);
            _Operator.Source = extractEb(b);
            break;
          case 3:
            b = Buffer[X];
            Length++;
            _Operator.Destination = extractGv(b);
            _Operator.Source = extractEv(b);
            break;
          case 4:
            value = (byte)Buffer[X];
            Length++;
            _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Al);
            _Operator.Source = x86Operand.NewValue(value);
            break;
          case 5:
            value = Buffer[X];
            value |= (uint)Buffer[X + 1] << 8;
            value |= (uint)Buffer[X + 2] << 16;
            value |= (uint)Buffer[X + 3] << 24;
            Length += 4;
            _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Eax);
            _Operator.Source = x86Operand.NewValue(value);
            break;
        }

      } else if (b < 0x20) {
        if (b == 0x0F) {
          return Read_0F();
        }

        _Operator = new x86Operator((b & 1) == 0 ? Intelx86Opcode.Push : Intelx86Opcode.Pop);
        _Operator.Source = x86Operand.NewRegister((int)Intelx86Register.Es + (b >> 3));
      } else if (b < 0x40) {
        if (b == 0x26) {
          _Operator = Read_Segment(Intelx86Register.Es);
        } else if (b == 0x27) {
          _Operator = new x86Operator(Intelx86Opcode.Daa);
        } else if (b == 0x2E) {
          _Operator = Read_Segment(Intelx86Register.Cs);
        } else if (b == 0x2F) {
          _Operator = new x86Operator(Intelx86Opcode.Das);
        } else if (b == 0x36) {
          _Operator = Read_Segment(Intelx86Register.Ss);
        } else if (b == 0x37) {
          _Operator = new x86Operator(Intelx86Opcode.Aaa);
        } else if (b == 0x3E) {
          _Operator = Read_Segment(Intelx86Register.Ds);
        } else if (b == 0x3F) {
          _Operator = new x86Operator(Intelx86Opcode.Aas);
        }
      } else
        throw new ArgumentException();
      return _Operator;
    }

    private x86Operator Read_0F ()
    {
      byte b = Buffer[X];
      Length++;
      switch ((b >> 4) & 0xF) {
        case 8:
          return Read_0F_8x(b);
      }
      return null;
    }

    private x86Operator Read_0F_8x (byte b)
    {
      x86Operator _Operator = null;
      long value;
      value = Buffer[X];
      value |= (uint)Buffer[X + 1] << 8;
      value |= (uint)Buffer[X + 2] << 16;
      value |= (uint)Buffer[X + 3] << 24;
      Length += 4;
      _Operator = new x86Operator(Intelx86Opcode.Jo + (b & 0xF));
      _Operator.Source = x86Operand.NewDisplacement(value + 2);
      return _Operator;
    }

    private x86Operator Read_4x (byte b)
    {
      x86Operator _Operator = null;
      _Operator = new x86Operator(b < 0x48 ? Intelx86Opcode.Inc : Intelx86Opcode.Dec);
      _Operator.Source = x86Operand.NewRegister((int)Intelx86Register.Eax + (b & 0x7));
      return _Operator;
    }

    private x86Operator Read_5x (byte b)
    {
      x86Operator _Operator = null;
      _Operator = new x86Operator(b < 0x58 ? Intelx86Opcode.Push : Intelx86Opcode.Pop);
      _Operator.Source = x86Operand.NewRegister((int)Intelx86Register.Eax + (b & 0x7));
      return _Operator;
    }

    private x86Operator Read_6x (byte b)
    {
      x86Operator _Operator = null;

      if (b == 0x60) {
        _Operator = new x86Operator(Intelx86Opcode.Pusha);
      } else if (b == 0x61) {
        _Operator = new x86Operator(Intelx86Opcode.Popa);
      } else if (b == 0x62) {
        _Operator = new x86Operator(Intelx86Opcode.Bound);
        b = Buffer[X];
        Length++;
        _Operator.Destination = extractMa(b);
        _Operator.Source = extractGv(b);
        if (_Operator.Destination == null) {
          Length--;
          _Operator = new x86Operator(Intelx86Opcode.Bad);
        }
      } else if (b == 0x63) {
        _Operator = new x86Operator(Intelx86Opcode.Arpl);
        b = Buffer[X];
        Length++;
        _Operator.Destination = extractEw(b);
        _Operator.Source = extractGw(b);
      } else if (b == 0x64) {
        _Operator = Read_Segment(Intelx86Register.Fs);
      } else if (b == 0x65) {
        _Operator = Read_Segment(Intelx86Register.Gs);

      } else if (b == 0x66) {
        _Operator = Read();

        if (_Operator.Destination.Register >= (int)Intelx86Register.Eax &&
            _Operator.Destination.Register <= (int)Intelx86Register.Edi) {
          _Operator.Destination.Register -= 8;
        }

        if (_Operator.Source.Register >= (int)Intelx86Register.Eax &&
            _Operator.Source.Register <= (int)Intelx86Register.Edi) {
          _Operator.Source.Register -= 8;
        }
      } else if (b == 0x68) {
        _Operator = new x86Operator(Intelx86Opcode.Push);
        _Operator.Source = extractLz();
      } else if (b == 0x69) {
        _Operator = new x86Operator(Intelx86Opcode.IMul);
        b = Buffer[X];
        Length++;
        _Operator.ThirdOpad = extractGv(b);
        _Operator.Destination = extractEv(b);
        _Operator.Source = extractLz();

      } else if (b == 0x6a) {
        _Operator = new x86Operator(Intelx86Opcode.Push);
        _Operator.Source = extractLb();
      } else if (b == 0x6b) {
        _Operator = new x86Operator(Intelx86Opcode.IMul);
        b = Buffer[X];
        Length++;
        _Operator.ThirdOpad = extractGv(b);
        _Operator.Destination = extractEv(b);
        _Operator.Source = extractLb();

      } else if (b == 0x6c) {
        _Operator = new x86Operator(Intelx86Opcode.Ins, OperatorSize.Byte);
        _Operator.Segment = Intelx86Register.Es;
        _Operator.Destination = x86Operand.NewMemory((int)Intelx86Register.Edi);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Dx);
      } else if (b == 0x6d) {
        _Operator = new x86Operator(Intelx86Opcode.Ins, OperatorSize.DWord);
        _Operator.Segment = Intelx86Register.Es;
        _Operator.Destination = x86Operand.NewMemory((int)Intelx86Register.Edi);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Dx);
      } else if (b == 0x6e) {
        _Operator = new x86Operator(Intelx86Opcode.Outs, OperatorSize.Byte);
        _Operator.SegmentSrc = Intelx86Register.Ds;
        _Operator.Destination = x86Operand.NewMemory((int)Intelx86Register.Dx);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Esi);
      } else if (b == 0x6f) {
        _Operator = new x86Operator(Intelx86Opcode.Outs, OperatorSize.DWord);
        _Operator.SegmentSrc = Intelx86Register.Ds;
        _Operator.Destination = x86Operand.NewMemory((int)Intelx86Register.Dx);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Esi);
      } else {
        throw new ArgumentException();
      }

      return _Operator;
    }

    private x86Operator Read_7x (byte b)
    {
      x86Operator _Operator = null;
      long value;
      value = (byte)Buffer[X];
      if (value >= 0xfe) {
        value -= 256;
      }
      if (value >= 0x80) {
        value |= 0xffffff00;
      }
      Length++;
      _Operator = new x86Operator(Intelx86Opcode.Jo + (b & 0xF));
      _Operator.Source = x86Operand.NewDisplacement(value + 2);
      return _Operator;
    }

    private x86Operator Read_8x (byte b)
    {
      x86Operator _Operator = null;
      long value;

      if (b == 0x80) {
        b = Buffer[X];
        Length++;
        int reg = (b >> 3) & 0x7;
        _Operator = new x86Operator(Intelx86Opcode.Add + reg, OperatorSize.Byte);
        _Operator.Destination = extractEb(b);
        value = (byte)Buffer[X];
        Length++;
        _Operator.Source = x86Operand.NewValue(value);
      } else if (b == 0x81) {
        b = Buffer[X];
        Length++;
        int reg = (b >> 3) & 0x7;
        _Operator = new x86Operator(Intelx86Opcode.Add + reg, OperatorSize.DWord);
        _Operator.Destination = extractEv(b);
        _Operator.Source = extractLz();
      } else if (b == 0x82) {
        b = Buffer[X];
        Length++;
        int reg = (b >> 3) & 0x7;
        _Operator = new x86Operator(Intelx86Opcode.Add + reg, OperatorSize.Byte);
        _Operator.Destination = extractEb(b);
        _Operator.Source = extractLb();
      } else if (b == 0x83) {
        b = Buffer[X];
        Length++;
        int reg = (b >> 3) & 0x7;
        _Operator = new x86Operator(Intelx86Opcode.Add + reg, OperatorSize.DWord);
        _Operator.Destination = extractEv(b);
        _Operator.Source = extractLb();
      } else if (b < 0x88) {
        _Operator = new x86Operator(b < 0x86 ? Intelx86Opcode.Test : Intelx86Opcode.Xchg);
        if ((b & 1) == 0) {
          b = Buffer[X];
          Length++;
          _Operator.Source = extractGb(b);
          _Operator.Destination = extractEb(b);
        } else {
          b = Buffer[X];
          Length++;
          _Operator.Source = extractGv(b);
          _Operator.Destination = extractEv(b);
        }
      } else if (b == 0x88) {
        _Operator = new x86Operator(Intelx86Opcode.Mov);
        b = Buffer[X];
        Length++;
        _Operator.Source = extractGb(b);
        _Operator.Destination = extractEb(b);
      } else if (b == 0x89) {
        _Operator = new x86Operator(Intelx86Opcode.Mov);
        b = Buffer[X];
        Length++;
        _Operator.Source = extractGv(b);
        _Operator.Destination = extractEv(b);
      } else if (b == 0x8A) {
        _Operator = new x86Operator(Intelx86Opcode.Mov);
        b = Buffer[X];
        Length++;
        _Operator.Source = extractEb(b);
        _Operator.Destination = extractGb(b);
      } else if (b == 0x8B) {
        _Operator = new x86Operator(Intelx86Opcode.Mov);
        b = Buffer[X];
        Length++;
        _Operator.Source = extractEv(b);
        _Operator.Destination = extractGv(b);
      } else if (b == 0x8C) {
        _Operator = new x86Operator(Intelx86Opcode.Mov);
        b = Buffer[X];
        Length++;
        _Operator.Source = extractSw(b);
        _Operator.Destination = extractEv(b);
      } else if (b == 0x8D) {
        _Operator = new x86Operator(Intelx86Opcode.Lea);
        b = Buffer[X];
        Length++;
        _Operator.Source = extractMa(b);
        _Operator.Destination = extractGv(b);
      } else if (b == 0x8E) {
        _Operator = new x86Operator(Intelx86Opcode.Mov);
        b = Buffer[X];
        Length++;
        _Operator.Destination = extractSw(b);
        _Operator.Source = extractEw(b);
      } else if (b == 0x8F) {
        b = Buffer[X];
        Length++;
        int reg = (b >> 3) & 0x7;
        if (reg == 0) {
          _Operator = new x86Operator(Intelx86Opcode.Pop, OperatorSize.DWord);
          _Operator.Source = extractEv(b);
        } else {
          _Operator = new x86Operator(Intelx86Opcode.Bad);
        }
      } else
        throw new ArgumentException();
      return _Operator;
    }

    private x86Operator Read_9x (byte b)
    {
      x86Operator _Operator = null;

      if (b == 0x90 && Length == 1) {
        _Operator = new x86Operator(Intelx86Opcode.Nop);
      } else if (b < 0x98) {
        _Operator = new x86Operator(Intelx86Opcode.Xchg);
        _Operator.Source = x86Operand.NewRegister((int)Intelx86Register.Eax);
        _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Eax + b - 0x90);
      } else if (b < 0x9A) {
        _Operator = new x86Operator(Intelx86Opcode.Cwtl + b - 0x98);
      } else if (b == 0x9A) {
        _Operator = new x86Operator(Intelx86Opcode.LCall);
        _Operator.Destination = extractLw();
        _Operator.Source = extractLz();
      } else if (b == 0x9B) {
        _Operator = new x86Operator(Intelx86Opcode.FWait);
      } else if (b < 0xA0) {
        _Operator = new x86Operator(Intelx86Opcode.Pushf + b - 0x9C);
      } else {
        throw new ArgumentException();
      }

      return _Operator;
    }

    private x86Operator Read_Ax (byte b)
    {
      x86Operator _Operator = null;

      if (b == 0xA0) {
        _Operator = new x86Operator(Intelx86Opcode.Mov);
        _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Al);
        _Operator.Source = extractOb();
      } else if (b == 0xA1) {
        _Operator = new x86Operator(Intelx86Opcode.Mov);
        _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Eax);
        _Operator.Source = extractOz();
      } else if (b == 0xA2) {
        _Operator = new x86Operator(Intelx86Opcode.Mov);
        _Operator.Destination = extractOb();
        _Operator.Source = x86Operand.NewRegister((int)Intelx86Register.Al);
      } else if (b == 0xA3) {
        _Operator = new x86Operator(Intelx86Opcode.Mov);
        _Operator.Destination = extractOz();
        _Operator.Source = x86Operand.NewRegister((int)Intelx86Register.Eax);
      } else if (b == 0xA4) {
        _Operator = new x86Operator(Intelx86Opcode.Movs, OperatorSize.Byte);
        _Operator.Segment = Intelx86Register.Es;
        _Operator.SegmentSrc = Intelx86Register.Ds;
        _Operator.Destination = x86Operand.NewMemory((int)Intelx86Register.Edi);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Esi);
      } else if (b == 0xA5) {
        _Operator = new x86Operator(Intelx86Opcode.Movs, OperatorSize.DWord);
        _Operator.Segment = Intelx86Register.Es;
        _Operator.SegmentSrc = Intelx86Register.Ds;
        _Operator.Destination = x86Operand.NewMemory((int)Intelx86Register.Edi);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Esi);
      } else if (b == 0xA6) {
        _Operator = new x86Operator(Intelx86Opcode.Cmps, OperatorSize.Byte);
        _Operator.Segment = Intelx86Register.Ds;
        _Operator.SegmentSrc = Intelx86Register.Es;
        _Operator.Destination = x86Operand.NewMemory((int)Intelx86Register.Esi);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Edi);
      } else if (b == 0xA7) {
        _Operator = new x86Operator(Intelx86Opcode.Cmps, OperatorSize.DWord);
        _Operator.Segment = Intelx86Register.Ds;
        _Operator.SegmentSrc = Intelx86Register.Es;
        _Operator.Destination = x86Operand.NewMemory((int)Intelx86Register.Esi);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Edi);
      } else if (b == 0xA8) {
        long value = Buffer[X];
        Length++;
        _Operator = new x86Operator(Intelx86Opcode.Test);
        _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Al);
        _Operator.Source = x86Operand.NewValue(value);
      } else if (b == 0xA9) {
        long value = Buffer[X];
        value |= (uint)Buffer[X + 1] << 8;
        value |= (uint)Buffer[X + 2] << 16;
        value |= (uint)Buffer[X + 3] << 24;
        Length += 4;
        _Operator = new x86Operator(Intelx86Opcode.Test);
        _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Eax);
        _Operator.Source = x86Operand.NewValue(value);
      } else if (b == 0xAA) {
        _Operator = new x86Operator(Intelx86Opcode.Stos);
        _Operator.Segment = Intelx86Register.Es;
        _Operator.Source = x86Operand.NewRegister((int)Intelx86Register.Al);
        _Operator.Destination = x86Operand.NewMemory((int)Intelx86Register.Edi);
      } else if (b == 0xAB) {
        _Operator = new x86Operator(Intelx86Opcode.Stos);
        _Operator.Segment = Intelx86Register.Es;
        _Operator.Source = x86Operand.NewRegister((int)Intelx86Register.Eax);
        _Operator.Destination = x86Operand.NewMemory((int)Intelx86Register.Edi);
      } else if (b == 0xAC) {
        _Operator = new x86Operator(Intelx86Opcode.Lods);
        _Operator.SegmentSrc = Intelx86Register.Ds;
        _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Al);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Esi);
      } else if (b == 0xAD) {
        _Operator = new x86Operator(Intelx86Opcode.Lods);
        _Operator.SegmentSrc = Intelx86Register.Ds;
        _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Eax);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Esi);
      } else if (b == 0xAE) {
        _Operator = new x86Operator(Intelx86Opcode.Scas);
        _Operator.SegmentSrc = Intelx86Register.Es;
        _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Al);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Edi);
      } else if (b == 0xAF) {
        _Operator = new x86Operator(Intelx86Opcode.Scas);
        _Operator.SegmentSrc = Intelx86Register.Es;
        _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Eax);
        _Operator.Source = x86Operand.NewMemory((int)Intelx86Register.Edi);
      } else {
        throw new ArgumentException();
      }

      return _Operator;
    }

    private x86Operator Read_Bx (byte b)
    {
      x86Operator _Operator = null;
      _Operator = new x86Operator(Intelx86Opcode.Mov);
      if (b < 0xB8) {
        _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Al + (b & 7));
        _Operator.Source = extractLb();
      } else {
        _Operator.Destination = x86Operand.NewRegister((int)Intelx86Register.Eax + (b & 7));
        _Operator.Source = extractLz();
      }
      return _Operator;
    }

    private x86Operator Read_Cx (byte b)
    {
      x86Operator _Operator = null;

      if (b == 0xC0) {
        b = Buffer[X];
        int v = ((b >> 3) & 0x7);
        if (v == 6) {
          _Operator = new x86Operator(Intelx86Opcode.Bad);
        } else {
          switch (v) {
            case 0:
              _Operator = new x86Operator(Intelx86Opcode.Rol, OperatorSize.Byte);
              break;
            case 1:
              _Operator = new x86Operator(Intelx86Opcode.Ror, OperatorSize.Byte);
              break;
            case 2:
              _Operator = new x86Operator(Intelx86Opcode.Rcl, OperatorSize.Byte);
              break;
            case 3:
              _Operator = new x86Operator(Intelx86Opcode.Rcr, OperatorSize.Byte);
              break;
            case 4:
              _Operator = new x86Operator(Intelx86Opcode.Shl, OperatorSize.Byte);
              break;
            case 5:
              _Operator = new x86Operator(Intelx86Opcode.Shr, OperatorSize.Byte);
              break;
            case 7:
              _Operator = new x86Operator(Intelx86Opcode.Sar, OperatorSize.Byte);
              break;
          }
          Length++;
          _Operator.Destination = extractEb(b);
          _Operator.Source = extractLb();
        }
      } else if (b == 0xC1) {
        b = Buffer[X];
        int v = ((b >> 3) & 0x7);
        if (v == 6) {
          _Operator = new x86Operator(Intelx86Opcode.Bad);
        } else {
          switch (v) {
            case 0:
              _Operator = new x86Operator(Intelx86Opcode.Rol, OperatorSize.DWord);
              break;
            case 1:
              _Operator = new x86Operator(Intelx86Opcode.Ror, OperatorSize.DWord);
              break;
            case 2:
              _Operator = new x86Operator(Intelx86Opcode.Rcl, OperatorSize.DWord);
              break;
            case 3:
              _Operator = new x86Operator(Intelx86Opcode.Rcr, OperatorSize.DWord);
              break;
            case 4:
              _Operator = new x86Operator(Intelx86Opcode.Shl, OperatorSize.DWord);
              break;
            case 5:
              _Operator = new x86Operator(Intelx86Opcode.Shr, OperatorSize.DWord);
              break;
            case 7:
              _Operator = new x86Operator(Intelx86Opcode.Sar, OperatorSize.DWord);
              break;
          }
          Length++;
          _Operator.Destination = extractEv(b);
          _Operator.Source = extractLb();
        }
      } else if (b == 0xC2) {
        long value = Buffer[X] | (Buffer[X + 1] << 8);
        Length += 2;
        _Operator = new x86Operator(Intelx86Opcode.Ret_);
        _Operator.Source = x86Operand.NewValue(value);
      } else if (b == 0xC3) {
        _Operator = new x86Operator(Intelx86Opcode.Ret);
      } else if (b == 0xC4) {
        _Operator = new x86Operator(Intelx86Opcode.Ret);
      } else if (b == 0xC6) {
        b = Buffer[X];
        int v = ((b >> 3) & 0x7);
        if (v != 0) {
          _Operator = new x86Operator(Intelx86Opcode.Bad);
        } else {
          _Operator = new x86Operator(Intelx86Opcode.Mov, OperatorSize.Byte);
          Length++;
          _Operator.Destination = extractEb(b);
          _Operator.Source = extractLb();
        }
      } else if (b == 0xC7) {
        b = Buffer[X];
        int v = ((b >> 3) & 0x7);
        if (v != 0) {
          _Operator = new x86Operator(Intelx86Opcode.Bad);
        } else {
          _Operator = new x86Operator(Intelx86Opcode.Mov, OperatorSize.DWord);
          Length++;
          _Operator.Destination = extractEv(b);
          _Operator.Source = extractLz();
        }
      } else if (b == 0xC8) {
        long valueB = Buffer[X];
        Length++;
        long valueW = Buffer[X] | (Buffer[X + 1] << 8);
        Length += 2;
        _Operator = new x86Operator(Intelx86Opcode.Enter);
        _Operator.Source = x86Operand.NewValue(valueB);
        _Operator.Destination = x86Operand.NewValue(valueW);
      } else if (b == 0xC9) {
        _Operator = new x86Operator(Intelx86Opcode.Leave);
      } else if (b == 0xCA) {
        long value = Buffer[X] | (Buffer[X + 1] << 8);
        Length += 2;
        _Operator = new x86Operator(Intelx86Opcode.LRet_);
        _Operator.Source = x86Operand.NewValue(value);
      } else if (b == 0xCB) {
        _Operator = new x86Operator(Intelx86Opcode.LRet);
      } else if (b == 0xCC) {
        _Operator = new x86Operator(Intelx86Opcode.Int3);
      } else if (b == 0xCD) {
        _Operator = new x86Operator(Intelx86Opcode.Int);
        _Operator.Source = extractLb();
      } else if (b < 0xD0) {
      } else {
        throw new ArgumentException();
      }

      return _Operator;
    }

    private x86Operator Read_Dx (byte b)
    {
      x86Operator _Operator = null;
      if (b == 0xD0) {
        b = Buffer[X];
        int v = ((b >> 3) & 0x7);
        if (v == 6) {
          _Operator = new x86Operator(Intelx86Opcode.Bad);
        } else {
          switch (v) {
            case 0:
              _Operator = new x86Operator(Intelx86Opcode.Rol_, OperatorSize.Byte);
              break;
            case 1:
              _Operator = new x86Operator(Intelx86Opcode.Ror_, OperatorSize.Byte);
              break;
            case 2:
              _Operator = new x86Operator(Intelx86Opcode.Rcl_, OperatorSize.Byte);
              break;
            case 3:
              _Operator = new x86Operator(Intelx86Opcode.Rcr_, OperatorSize.Byte);
              break;
            case 4:
              _Operator = new x86Operator(Intelx86Opcode.Shl_, OperatorSize.Byte);
              break;
            case 5:
              _Operator = new x86Operator(Intelx86Opcode.Shr_, OperatorSize.Byte);
              break;
            case 7:
              _Operator = new x86Operator(Intelx86Opcode.Sar_, OperatorSize.Byte);
              break;
          }
          Length++;
          _Operator.Source = extractEb(b);
        }
      } else if (b == 0xD1) {
        b = Buffer[X];
        int v = ((b >> 3) & 0x7);
        if (v == 6) {
          _Operator = new x86Operator(Intelx86Opcode.Bad);
        } else {
          switch (v) {
            case 0:
              _Operator = new x86Operator(Intelx86Opcode.Rol_, OperatorSize.DWord);
              break;
            case 1:
              _Operator = new x86Operator(Intelx86Opcode.Ror_, OperatorSize.DWord);
              break;
            case 2:
              _Operator = new x86Operator(Intelx86Opcode.Rcl_, OperatorSize.DWord);
              break;
            case 3:
              _Operator = new x86Operator(Intelx86Opcode.Rcr_, OperatorSize.DWord);
              break;
            case 4:
              _Operator = new x86Operator(Intelx86Opcode.Shl_, OperatorSize.DWord);
              break;
            case 5:
              _Operator = new x86Operator(Intelx86Opcode.Shr_, OperatorSize.DWord);
              break;
            case 7:
              _Operator = new x86Operator(Intelx86Opcode.Sar_, OperatorSize.DWord);
              break;
          }
          Length++;
          _Operator.Source = extractEv(b);
        }
      } else {
      }
      return _Operator;
    }

    private x86Operator Read_Ex (byte b)
    {
      x86Operator _Operator = null;
      if (b == 0xE8) {
        _Operator = new x86Operator(Intelx86Opcode.Call);
        _Operator.Source = extractLz();
      } else if (b == 0xE9) {
        _Operator = new x86Operator(Intelx86Opcode.Jmp);
        _Operator.Source = extractLz();
      } else if (b == 0xEB) {
        _Operator = new x86Operator(Intelx86Opcode.Jmp);
        _Operator.Source = extractLb();
      } else {
      }
      return _Operator;
    }

    private x86Operator Read_Fx (byte b)
    {
      x86Operator _Operator = null;
      if (b == 0xF0) {
        _Operator = Read();
        _Operator.Prefix = "lock";
      } else if (b == 0xF1) {
        _Operator = new x86Operator(Intelx86Opcode.Icebp);
      } else if (b == 0xF2) {
        _Operator = Read();
        _Operator.Prefix = "repnz";
      } else if (b == 0xF3) {
        _Operator = Read();
        _Operator.Prefix = "repz";
      } else if (b == 0xF4) {
        _Operator = new x86Operator(Intelx86Opcode.Hlt);
      } else if (b == 0xF5) {
        _Operator = new x86Operator(Intelx86Opcode.Cmc);
      } else if (b == 0xF6) {
        b = Buffer[X];
        if (b == 0x8) {
          _Operator = new x86Operator(Intelx86Opcode.Bad);
          return _Operator;
        }
        _Operator = new x86Operator(Intelx86Opcode.Test, OperatorSize.Byte);
        Length++;
        _Operator.Destination = extractEb(b);
        _Operator.Source = extractLb();
      } else if (b == 0xF7) {
        b = Buffer[X];
        if (b == 0x8) {
          _Operator = new x86Operator(Intelx86Opcode.Bad);
          return _Operator;
        }
        _Operator = new x86Operator(Intelx86Opcode.Test, OperatorSize.DWord);
        Length++;
        _Operator.Destination = extractEv(b);
        _Operator.Source = extractLz();

      } else if (b < 0xFE)
        _Operator = new x86Operator(Intelx86Opcode.Clc + b - 0xF8);
      else if (b == 0xFE) {
        b = Buffer[X];
        if (((b >> 3) & 0x7) > 1) {
          _Operator = new x86Operator(Intelx86Opcode.Bad);
          return _Operator;
        }
        _Operator = new x86Operator(Intelx86Opcode.Inc + ((b >> 3) & 0x1), OperatorSize.Byte);
        Length++;
        _Operator.Source = extractEb(b);
      } else if (b == 0xFF) {
        b = Buffer[X];
        int v = ((b >> 3) & 0x7);
        if (v == 7) {
          _Operator = new x86Operator(Intelx86Opcode.Bad);
        } else {
          Length++;
          switch (v) {
            case 0:
              _Operator = new x86Operator(Intelx86Opcode.Inc, OperatorSize.DWord);
              _Operator.Source = extractEv(b);
              break;
            case 1:
              _Operator = new x86Operator(Intelx86Opcode.Dec, OperatorSize.DWord);
              _Operator.Source = extractEv(b);
              break;
            case 2:
              _Operator = new x86Operator(Intelx86Opcode.Call);
              _Operator.Source = extractEv(b);
              break;
            case 3:
              _Operator = new x86Operator(Intelx86Opcode.Call);
              _Operator.Source = extractEv(b);
              break;
            case 4:
              _Operator = new x86Operator(Intelx86Opcode.Jmp);
              _Operator.Source = extractEv(b);
              break;
            case 5:
              _Operator = new x86Operator(Intelx86Opcode.Jmp);
              _Operator.Source = extractMa(b);
              break;
            case 6:
              _Operator = new x86Operator(Intelx86Opcode.Push, OperatorSize.DWord);
              _Operator.Source = extractEv(b);
              break;
          }
        }
      } else {
      }
      return _Operator;
    }

    private x86Operator Read_Segment (Intelx86Register segment)
    {
      x86Operator _Operator;
      _Operator = Read();

      if (_Operator.OperandCount >= 1 &&
          (_Operator.Source.Type == OperandType.Address ||
          _Operator.Source.Type == OperandType.Memory ||
          _Operator.Source.Type == OperandType.MemoryFull ||
          _Operator.Source.Type == OperandType.MemoryOffset ||
          _Operator.Source.Type == OperandType.MemoryScale))
        _Operator.SegmentSrc = segment;

      else if (_Operator.OperandCount >= 2 &&
          (_Operator.Destination.Type == OperandType.Address ||
          _Operator.Destination.Type == OperandType.Memory ||
          _Operator.Destination.Type == OperandType.MemoryFull ||
          _Operator.Destination.Type == OperandType.MemoryOffset ||
          _Operator.Destination.Type == OperandType.MemoryScale))
        _Operator.Segment = segment;

      else {
        _Operator = new x86Operator(Intelx86Opcode.Segment);
        _Operator.Segment = segment;
        Length = 1;
      }

      //byte c = Buffer[X];
      //if ((c & 7) >= 4)
      //{
      //    _Operator = new x86Operator(Intelx86Opcode.Segment);
      //    _Operator.Segment = segment;
      //}
      //else
      //{
      //    _Operator = Read();
      //    if ((c & 0x2) == 0)
      //        _Operator.Segment = segment;
      //    else
      //        _Operator.SegmentSrc = segment;
      //}
      return _Operator;
    }

    private x86Operator Read ()
    {
      byte b = Buffer[X];
      Length++;
      switch ((b >> 4) & 0xF) {
        case 0:
        case 1:
        case 2:
        case 3:
          return Read_00_to_40(b);
        case 4:
          return Read_4x(b);
        case 5:
          return Read_5x(b);
        case 6:
          return Read_6x(b);
        case 7:
          return Read_7x(b);
        case 8:
          return Read_8x(b);
        case 9:
          return Read_9x(b);
        case 10:
          return Read_Ax(b);
        case 11:
          return Read_Bx(b);
        case 12:
          return Read_Cx(b);
        case 13:
          return Read_Dx(b);
        case 14:
          return Read_Ex(b);
        case 15:
          return Read_Fx(b);
      }
      return null;
    }
  }
}
