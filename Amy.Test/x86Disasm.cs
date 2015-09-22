using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmySuite
{
    enum Reg32
    {
        eax, ecx, edx, ebx,
    }

    class x86Disasm
    {
        public static IROperand OperandRegister (int reg) 
        {
            return new IROperand()
            {
                RegNo = reg,
                PType = IRPrimitives._U32,
                Literal = ((Reg32)reg).ToString()
            };
        }

        public static IROpcode Opcode(string op, IROperand dest, IROperand srcr, IROperand srcl)
        {
            return new IROpcode()
            {
                opcode = op,
                srcLeft = srcl,
                srcRight = srcr,
                output = dest,
            };
        }

        #region
        
        public static object Read_4x(byte[] arr, int idx)
        {
            IROperand l = OperandRegister (arr[idx] & 0x7);
            return Opcode ((arr[idx] < 0x48) ? "inc" : "dec", l, null, l);
        }
        
        public static object Read_5x(byte[] arr, int idx)
        {
            IROperand l = OperandRegister(arr[idx] & 0x7);
            return Opcode((arr[idx] < 0x58) ? "push" : "pop", l, null, l);
        }

        
        public static object Read_6x(byte[] arr, int idx)
        {
            IROperand d, s, t;
            switch (arr[idx])
            {
                case 0x60:
                    return Opcode("pusha", null, null, null);

                case 0x61:
                    return Opcode("popa", null, null, null);

                case 0x62:
                    d = ExtractMa (idx + 1);
                    s = ExtractGv (idx + 2);
                    if (d == null) return null;
                    return Opcode("bound", d, null, s);

                case 0x63:
                    d = ExtractEw (idx + 1);
                    s = ExtractGw (idx + 2);
                    return Opcode("arpl", d, null, s);


                case 0x68:
                    return Opcode("push", null, null, ExtractLz());

                case 0x69:
                    t = ExtractGv(idx + 1);
                    d = ExtractEv (idx + 1);
                    s = ExtractLz ();
                    return Opcode("imul", d, null, s, t);

                case 0x6a:
                    return Opcode("push", null, null, ExtractLb());
                    
                case 0x6b:
                    t = ExtractGv(idx + 1);
                    d = ExtractEv (idx + 1);
                    s = ExtractLb ();
                    return Opcode("imul", d, null, s, t);
                    
                case 0x6c:
                    d = ExtractLb ();
                    s = ExtractLb ();
                    return Opcode("ins", d, null, s);

            }
        }
        
        public static object Read_Bx(byte[] arr, int idx)
        {
            IROperand d = OperandRegister(arr[idx] - 0xB8);
            IROperand l = ExtractLz();
            return Opcode("mov", d, null, l);
        }

        #endregion

        public static object Read(byte[] arr, int idx)
        {
            int lg = 1;
            byte b = arr[idx];
            switch ((b >> 4) & 0xF)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    return Read_00_to_40(arr, idx);
                case 4:
                    return Read_4x(arr, idx);
                case 5:
                    return Read_5x(arr, idx);
                case 6:
                    return Read_6x(arr, idx);
                case 7:
                    return Read_7x(arr, idx);
                case 8:
                    return Read_8x(arr, idx);
                case 9:
                    return Read_9x(arr, idx);
                case 10:
                    return Read_Ax(arr, idx);
                case 11:
                    return Read_Bx(arr, idx);
                case 12:
                    return Read_Cx(arr, idx);
                case 13:
                    return Read_Dx(arr, idx);
                case 14:
                    return Read_Ex(arr, idx);
                case 15:
                    return Read_Fx(arr, idx);
            }
            return null;
        }
    }


}
#if t
    /// <summary>
    /// Class used to convert intelx86 machine code segment into opcode
    /// </summary>
    public class x86Disasm
    {
        private int X { get { return Offset + Length; } }
        private byte[] Buffer { get; set; }
        public int Offset { get; private set; }
        public int Length { get; private set; }

        public x86Disasm(byte[] buffer)
        {
            this.Buffer = buffer;
            this.Offset = 0;
            this.Length = 0;
        }

        public Operator Next()
        {
            this.Offset += Length;
            this.Length = 0;
            if (X >= Buffer.Length)
                return null;
            x86Operator _Operator = Read();
            if (_Operator == null)
            {
                this.Length = 0;
                return null;
            }
            _Operator.Length = this.Length;
            if (X <= Buffer.Length)
                return _Operator;
            return null;
        }

        public void Echo(long virtualAddress)
        {
            for (; ; )
            {
                x86Operator instr = Next() as x86Operator;
                String amyDism = " " + Hexa.ToString(virtualAddress, 8, false, false) + ":\t";
                if (instr == null)
                {
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

        private x86Operand extractSIB(byte b, int mod)
        {
            int scale = b >> 6;
            int bas = b & 0x7;
            int index = (b >> 3) & 0x7;

            if (index == 4 && bas == 4 && scale == 0)
            {
                if (mod == 0)
                    return x86Operand.NewMemory((int)IntelRegister.Esp);
                else if (mod == 1)
                {
                    long value = (long)Buffer[X];
                    Length++;
                    return x86Operand.NewOffset((int)IntelRegister.Esp, value);
                }
                else if (mod == 2)
                {
                    long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                        | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
                    Length += 4;
                    return x86Operand.NewOffset((int)IntelRegister.Esp, value);
                }
                else
                    throw new NotImplementedException();
            }
            else if (bas == 5)
            {
                if (mod == 0)
                {
                    long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                        | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
                    Length += 4;
                    return x86Operand.NewMemoryOffset((int)IntelRegister.Eax + index, 1 << scale, value);
                }
                else if (mod == 1)
                {
                    long value = (long)Buffer[X];
                    Length++;
                    return x86Operand.NewMemoryFull((int)IntelRegister.Ebp, (int)IntelRegister.Eax + index, 1 << scale, value);
                }
                else if (mod == 2)
                {
                    long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                        | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
                    Length += 4;
                    return x86Operand.NewMemoryFull((int)IntelRegister.Ebp, (int)IntelRegister.Eax + index, 1 << scale, value);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                if (mod == 0)
                    return x86Operand.NewMemoryScale((int)IntelRegister.Eax + bas, (int)IntelRegister.Eax + index, 1 << scale);
                else if (mod == 1)
                {
                    long value = (long)Buffer[X];
                    Length++;
                    return x86Operand.NewMemoryFull((int)IntelRegister.Eax + bas, (int)IntelRegister.Eax + index, 1 << scale, value);
                }
                else if (mod == 2)
                {
                    long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                        | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
                    Length += 4;
                    return x86Operand.NewMemoryFull((int)IntelRegister.Eax + bas, (int)IntelRegister.Eax + index, 1 << scale, value);
                }
                else
                    throw new NotImplementedException();
            }
        }

        private x86Operand extractEb(byte b)
        {
            int mod = b >> 6;
            int rm = b & 0x7;
            long value;
            switch (mod)
            {
                case 0:
                    if (rm == 4)
                    {
                        b = Buffer[X];
                        Length++;
                        return extractSIB(b, mod);
                    }
                    else if (rm == 5)
                    {
                        value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                            | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
                        Length += 4;
                        return x86Operand.NewAddress(value);
                    }
                    else
                    {
                        return x86Operand.NewMemory((int)IntelRegister.Eax + rm);
                    }
                case 1:
                    if (rm == 4)
                    {
                        b = Buffer[X];
                        Length++;
                        return extractSIB(b, mod);
                    }
                    value = (sbyte)Buffer[X];
                    Length++;
                    return x86Operand.NewOffset((int)IntelRegister.Eax + rm, value);
                case 2:
                    if (rm == 4)
                    {
                        b = Buffer[X];
                        Length++;
                        return extractSIB(b, mod);
                    }
                    value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                        | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
                    Length += 4;
                    return x86Operand.NewOffset((int)IntelRegister.Eax + rm, value);
                case 3:
                    return x86Operand.NewRegister((int)IntelRegister.Al + rm);
            }
            return null;
        }

        private x86Operand extractEv(byte b)
        {
            int mod = b >> 6;
            int rm = b & 0x7;
            long value;
            switch (mod)
            {
                case 0:
                    if (rm == 4)
                    {
                        b = Buffer[X];
                        Length++;
                        return extractSIB(b, mod);
                    }
                    else if (rm == 5)
                    {
                        value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                            | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
                        Length += 4;
                        return x86Operand.NewAddress(value);
                    }
                    else
                    {
                        return x86Operand.NewMemory((int)IntelRegister.Eax + rm);
                    }
                case 1:
                    if (rm == 4)
                    {
                        b = Buffer[X];
                        Length++;
                        return extractSIB(b, mod);
                    }
                    value = (sbyte)Buffer[X];
                    Length++;
                    return x86Operand.NewOffset((int)IntelRegister.Eax + rm, value);
                case 2:
                    if (rm == 4)
                    {
                        b = Buffer[X];
                        Length++;
                        return extractSIB(b, mod);
                    }
                    value = (int)(Buffer[X] | (Buffer[X + 1] << 8) | (Buffer[X + 2] << 16) | (Buffer[X + 3] << 24));
                    Length += 4;
                    return x86Operand.NewOffset((int)IntelRegister.Eax + rm, value);
                case 3:
                    return x86Operand.NewRegister((int)IntelRegister.Eax + rm);
            }
            return null;
        }

        private x86Operand extractEw(byte b)
        {
            int mod = b >> 6;
            int rm = b & 0x7;
            if (mod < 3)
                return extractEv(b);
            return x86Operand.NewRegister((int)IntelRegister.Ax + rm);
        }

        private x86Operand extractGb(byte b)
        {
            //int mod = b >> 6;
            //int rm = b & 0x7;
            int reg = (b >> 3) & 0x7;
            return x86Operand.NewRegister((int)IntelRegister.Al + reg);
        }

        private x86Operand extractGv(byte b)
        {
            //int mod = b >> 6;
            //int rm = b & 0x7;
            int reg = (b >> 3) & 0x7;
            return x86Operand.NewRegister((int)IntelRegister.Eax + reg);
        }

        private x86Operand extractGw(byte b)
        {
            //int mod = b >> 6;
            //int rm = b & 0x7;
            int reg = (b >> 3) & 0x7;
            return x86Operand.NewRegister((int)IntelRegister.Ax + reg);
        }

        private x86Operand extractSw(byte b)
        {
            //int mod = b >> 6;
            //int rm = b & 0x7;
            int reg = (b >> 3) & 0x7;
            return x86Operand.NewRegister((int)IntelRegister.Es + reg);
        }

        private x86Operand extractMa(byte b)
        {
            int mod = b >> 6;
            if (mod < 3)
                return extractEv(b);
            return null;
        }

        private x86Operand extractLz()
        {
            long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
            Length += 4;
            return x86Operand.NewValue(value);
        }

        private x86Operand extractLb()
        {
            long value = (long)Buffer[X];
            Length++;
            return x86Operand.NewValue(value);
        }

        private x86Operand extractLw()
        {
            long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8);
            Length += 2;
            return x86Operand.NewValue(value);
        }

        private x86Operand extractOz()
        {
            long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
            Length += 4;
            return x86Operand.NewAddress(value);
        }

        private x86Operand extractOb()
        {
            long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8)
                | ((long)Buffer[X + 2] << 16) | ((long)Buffer[X + 3] << 24);
            Length += 4;
            return x86Operand.NewAddress(value);
        }

        private x86Operand extractOw()
        {
            long value = (long)Buffer[X] | ((long)Buffer[X + 1] << 8);
            Length += 2;
            return x86Operand.NewAddress(value);
        }

        #endregion


        private x86Operator Read_00_to_40(byte b)
        {
            x86Operator _Operator = null;
            long value;
            if ((b & 0x7) < 6)
            {
                _Operator = new x86Operator(IntelOpcode.Add + (b >> 3));
                switch (b & 0x7)
                {
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
                        _Operator.Destination = x86Operand.NewRegister((int)IntelRegister.Al);
                        _Operator.Source = x86Operand.NewValue(value);
                        break;
                    case 5:
                        value = Buffer[X];
                        value |= (uint)Buffer[X + 1] << 8;
                        value |= (uint)Buffer[X + 2] << 16;
                        value |= (uint)Buffer[X + 3] << 24;
                        Length += 4;
                        _Operator.Destination = x86Operand.NewRegister((int)IntelRegister.Eax);
                        _Operator.Source = x86Operand.NewValue(value);
                        break;
                }

            }
            else if (b < 0x20)
            {
                _Operator = new x86Operator((b & 1) == 0 ? IntelOpcode.Push : IntelOpcode.Pop);
                _Operator.Source = x86Operand.NewRegister((int)IntelRegister.Es + (b >> 3));
            }
            else if (b < 0x40)
            {
                if (b == 0x26)
                {
                    _Operator = Read();
                    if (_Operator.OperatorCode < (int)IntelOpcode.Add)
                    {
                        _Operator = new x86Operator(IntelOpcode.Segment);
                        Length = 1;
                    }
                    _Operator.Segment = IntelRegister.Es;
                }
            }
            else
                throw new ArgumentException();
            return _Operator;
        }

        /// <summary>
        /// Diassemble all x86 instructions from 0x40 to 0x4F
        /// </summary>
        /// <param name="b">First byte of opcode</param>
        /// <returns>The next x86 Operator</returns>
        private x86Operator Read_4x(byte b)
        {
            x86Operator _Operator = null;
            _Operator = new x86Operator(b < 0x48 ? IntelOpcode.Inc : IntelOpcode.Dec);
            _Operator.Source = x86Operand.NewRegister((int)IntelRegister.Eax + (b & 0x7));
            return _Operator;
        }

        /// <summary>
        /// Diassemble all x86 instructions from 0x50 to 0x5F
        /// </summary>
        /// <param name="b">First byte of opcode</param>
        /// <returns>The next x86 Operator</returns>
        private x86Operator Read_5x(byte b)
        {
            x86Operator _Operator = null;
            _Operator = new x86Operator(b < 0x58 ? IntelOpcode.Push : IntelOpcode.Pop);
            _Operator.Source = x86Operand.NewRegister((int)IntelRegister.Eax + (b & 0x7));
            return _Operator;
        }

        private x86Operator Read_6x(byte b)
        {
            x86Operator _Operator = null;

            if (b == 0x60)
            {
                _Operator = new x86Operator(IntelOpcode.Pusha);
            }
            else if (b == 0x61)
            {
                _Operator = new x86Operator(IntelOpcode.Popa);
            }
            else if (b == 0x62)
            {
                _Operator = new x86Operator(IntelOpcode.Bound);
                b = Buffer[X];
                Length++;
                _Operator.Destination = extractMa(b);
                _Operator.Source = extractGv(b);
                if (_Operator.Destination == null)
                {
                    Length--;
                    _Operator = new x86Operator(IntelOpcode.Bad);
                }
            }
            else if (b == 0x63)
            {
                _Operator = new x86Operator(IntelOpcode.Arpl);
                b = Buffer[X];
                Length++;
                _Operator.Destination = extractEw(b);
                _Operator.Source = extractGw(b);
            }
            else if (b < 0x68)
            {
            }
            else if (b == 0x68)
            {
                _Operator = new x86Operator(IntelOpcode.Push);
                _Operator.Source = extractLz();
            }
            else if (b == 0x69)
            {
                _Operator = new x86Operator(IntelOpcode.IMul);
                b = Buffer[X];
                Length++;
                _Operator.ThirdOpad = extractGv(b);
                _Operator.Destination = extractEv(b);
                _Operator.Source = extractLz();

            }
            else if (b == 0x6a)
            {
                _Operator = new x86Operator(IntelOpcode.Push);
                _Operator.Source = extractLb();
            }
            else if (b == 0x6b)
            {
                _Operator = new x86Operator(IntelOpcode.IMul);
                b = Buffer[X];
                Length++;
                _Operator.ThirdOpad = extractGv(b);
                _Operator.Destination = extractEv(b);
                _Operator.Source = extractLb();

            }
            else if (b == 0x6c)
            {
                _Operator = new x86Operator(IntelOpcode.Ins);
                _Operator.Destination = extractLb();
                _Operator.Source = extractLb();
            }
            else if (b < 0x70)
            {

            }
            else
            {
                throw new ArgumentException();
            }

            return _Operator;
        }

        /// <summary>
        /// Diassemble all x86 instructions from 0x70 to 0x7F
        /// </summary>
        /// <param name="b">First byte of opcode</param>
        /// <returns>The next x86 Operator</returns>
        private x86Operator Read_7x(byte b)
        {
            x86Operator _Operator = null;
            long value;
            value = (byte)Buffer[X];
            if (value >= 0xfe)
            {
                value -= 256;
            }
            if (value >= 0x80)
            {
                value |= 0xffffff00;
            }
            Length++;
            _Operator = new x86Operator(IntelOpcode.Jo + (b & 0xF));
            _Operator.Source = x86Operand.NewDisplacement(value + 2);
            return _Operator;
        }

        private x86Operator Read_8x(byte b)
        {
            x86Operator _Operator = null;
            long value;

            if (b == 0x80)
            {
                b = Buffer[X];
                Length++;
                int reg = (b >> 3) & 0x7;
                _Operator = new x86Operator(IntelOpcode.Add + reg, OperatorSize.Byte);
                _Operator.Destination = extractEb(b);
                value = (byte)Buffer[X];
                Length++;
                _Operator.Source = x86Operand.NewValue(value);
            }
            else if (b == 0x81)
            {
                b = Buffer[X];
                Length++;
                int reg = (b >> 3) & 0x7;
                _Operator = new x86Operator(IntelOpcode.Add + reg, OperatorSize.DWord);
                _Operator.Destination = extractEv(b);
                _Operator.Source = extractLz();
            }
            else if (b == 0x82)
            {
                b = Buffer[X];
                Length++;
                int reg = (b >> 3) & 0x7;
                _Operator = new x86Operator(IntelOpcode.Add + reg, OperatorSize.Byte);
                _Operator.Destination = extractEb(b);
                _Operator.Source = extractLb();
            }
            else if (b == 0x83)
            {
                b = Buffer[X];
                Length++;
                int reg = (b >> 3) & 0x7;
                _Operator = new x86Operator(IntelOpcode.Add + reg, OperatorSize.DWord);
                _Operator.Destination = extractEv(b);
                _Operator.Source = extractLb();
            }
            else if (b < 0x88)
            {
                _Operator = new x86Operator(b < 0x86 ? IntelOpcode.Test : IntelOpcode.Xchg);
                if ((b & 1) == 0)
                {
                    b = Buffer[X];
                    Length++;
                    _Operator.Source = extractGb(b);
                    _Operator.Destination = extractEb(b);
                }
                else
                {
                    b = Buffer[X];
                    Length++;
                    _Operator.Source = extractGv(b);
                    _Operator.Destination = extractEv(b);
                }
            }
            else if (b == 0x88)
            {
                _Operator = new x86Operator(IntelOpcode.Mov);
                b = Buffer[X];
                Length++;
                _Operator.Source = extractGb(b);
                _Operator.Destination = extractEb(b);
            }
            else if (b == 0x89)
            {
                _Operator = new x86Operator(IntelOpcode.Mov);
                b = Buffer[X];
                Length++;
                _Operator.Source = extractGv(b);
                _Operator.Destination = extractEv(b);
            }
            else if (b == 0x8A)
            {
                _Operator = new x86Operator(IntelOpcode.Mov);
                b = Buffer[X];
                Length++;
                _Operator.Source = extractEb(b);
                _Operator.Destination = extractGb(b);
            }
            else if (b == 0x8B)
            {
                _Operator = new x86Operator(IntelOpcode.Mov);
                b = Buffer[X];
                Length++;
                _Operator.Source = extractEv(b);
                _Operator.Destination = extractGv(b);
            }
            else if (b == 0x8C)
            {
                _Operator = new x86Operator(IntelOpcode.Mov);
                b = Buffer[X];
                Length++;
                _Operator.Source = extractSw(b);
                _Operator.Destination = extractEv(b);
            }
            else if (b == 0x8D)
            {
                _Operator = new x86Operator(IntelOpcode.Lea);
                b = Buffer[X];
                Length++;
                _Operator.Source = extractMa(b);
                _Operator.Destination = extractGv(b);
            }
            else if (b == 0x8E)
            {
                _Operator = new x86Operator(IntelOpcode.Mov);
                b = Buffer[X];
                Length++;
                _Operator.Source = extractSw(b);
                _Operator.Destination = extractEw(b);
            }
            else if (b == 0x8F)
            {
                b = Buffer[X];
                Length++;
                int reg = (b >> 3) & 0x7;
                if (reg == 0)
                {
                    _Operator = new x86Operator(IntelOpcode.Pop, OperatorSize.DWord);
                    _Operator.Source = extractEv(b);
                }
                else
                {
                    _Operator = new x86Operator(IntelOpcode.Bad);
                }
            }
            else
                throw new ArgumentException();
            return _Operator;
        }

        private x86Operator Read_9x(byte b)
        {
            x86Operator _Operator = null;

            if (b == 0x90)
            {
                _Operator = new x86Operator(IntelOpcode.Nop);
            }
            else if (b < 0x98)
            {
                _Operator = new x86Operator(IntelOpcode.Xchg);
                _Operator.Source = x86Operand.NewRegister((int)IntelRegister.Eax);
                _Operator.Destination = x86Operand.NewRegister((int)IntelRegister.Eax + b - 0x90);
            }
            else if (b < 0x9A)
            {
                _Operator = new x86Operator(IntelOpcode.Cwtl + b - 0x98);
            }
            else if (b == 0x9A)
            {
                _Operator = new x86Operator(IntelOpcode.LCall);
                _Operator.Destination = extractLw();
                _Operator.Source = extractLz();
            }
            else if (b == 0x9B)
            {
                _Operator = new x86Operator(IntelOpcode.FWait);
            }
            else if (b < 0xA0)
            {
                _Operator = new x86Operator(IntelOpcode.Pushf + b - 0x9C);
            }
            else
            {
                throw new ArgumentException();
            }

            return _Operator;
        }

        private x86Operator Read_Ax(byte b)
        {
            x86Operator _Operator = null;

            if (b == 0xA0)
            {
                _Operator = new x86Operator(IntelOpcode.Mov);
                _Operator.Destination = x86Operand.NewRegister((int)IntelRegister.Al);
                _Operator.Source = extractOb();
            }
            else if (b == 0xA1)
            {
                _Operator = new x86Operator(IntelOpcode.Mov);
                _Operator.Destination = x86Operand.NewRegister((int)IntelRegister.Eax);
                _Operator.Source = extractOz();
            }
            else if (b == 0xA2)
            {
                _Operator = new x86Operator(IntelOpcode.Mov);
                _Operator.Destination = extractOb();
                _Operator.Source = x86Operand.NewRegister((int)IntelRegister.Al);
            }
            else if (b == 0xA3)
            {
                _Operator = new x86Operator(IntelOpcode.Mov);
                _Operator.Destination = extractOz();
                _Operator.Source = x86Operand.NewRegister((int)IntelRegister.Eax);
            }
            else if (b == 0xA4)
            {
                _Operator = new x86Operator(IntelOpcode.Mov, OperatorSize.Byte);
                _Operator.Destination = x86Operand.NewRegister((int)IntelRegister.Eax);
                _Operator.Source = extractOz();
            }
            else if (b < 0xb0)
            {
            }
            else
            {
                throw new ArgumentException();
            }

            return _Operator;
        }

        private x86Operator Read_Bx(byte b)
        {
            x86Operator _Operator = null;
            _Operator = new x86Operator(IntelOpcode.Mov);
            _Operator.Destination = x86Operand.NewRegister((int)IntelRegister.Eax + b - 0xB8);
            _Operator.Source = extractLz();
            return _Operator;
        }

        private x86Operator Read_Cx(byte b)
        {
            x86Operator _Operator = null;

            if (b == 0xC0)
            {
                b = Buffer[X];
                int v = ((b >> 3) & 0x7);
                if (v == 6)
                {
                    _Operator = new x86Operator(IntelOpcode.Bad);
                }
                else
                {
                    switch (v)
                    {
                        case 0:
                            _Operator = new x86Operator(IntelOpcode.Rol);
                            break;
                        case 1:
                            _Operator = new x86Operator(IntelOpcode.Ror);
                            break;
                        case 2:
                            _Operator = new x86Operator(IntelOpcode.Rcl);
                            break;
                        case 3:
                            _Operator = new x86Operator(IntelOpcode.Rcr);
                            break;
                        case 4:
                            _Operator = new x86Operator(IntelOpcode.Shl);
                            break;
                        case 5:
                            _Operator = new x86Operator(IntelOpcode.Shr);
                            break;
                        case 7:
                            _Operator = new x86Operator(IntelOpcode.Sar);
                            break;
                    }
                    Length++;
                    _Operator.Destination = extractEb(b);
                    _Operator.Source = extractLb();
                }
            }
            else if (b == 0xC1)
            {
                b = Buffer[X];
                int v = ((b >> 3) & 0x7);
                if (v == 6)
                {
                    _Operator = new x86Operator(IntelOpcode.Bad);
                }
                else
                {
                    switch (v)
                    {
                        case 0:
                            _Operator = new x86Operator(IntelOpcode.Rol);
                            break;
                        case 1:
                            _Operator = new x86Operator(IntelOpcode.Ror);
                            break;
                        case 2:
                            _Operator = new x86Operator(IntelOpcode.Rcl);
                            break;
                        case 3:
                            _Operator = new x86Operator(IntelOpcode.Rcr);
                            break;
                        case 4:
                            _Operator = new x86Operator(IntelOpcode.Shl);
                            break;
                        case 5:
                            _Operator = new x86Operator(IntelOpcode.Shr);
                            break;
                        case 7:
                            _Operator = new x86Operator(IntelOpcode.Sar);
                            break;
                    }
                    Length++;
                    _Operator.Destination = extractEv(b);
                    _Operator.Source = extractLb();
                }
            }
            else if (b == 0xC3)
            {
                _Operator = new x86Operator(IntelOpcode.Ret);
            }
            else if (b == 0xC6)
            {
                b = Buffer[X];
                int v = ((b >> 3) & 0x7);
                if (v != 0)
                {
                    _Operator = new x86Operator(IntelOpcode.Bad);
                }
                else
                {
                    _Operator = new x86Operator(IntelOpcode.Mov);
                    Length++;
                    _Operator.Destination = extractEb(b);
                    _Operator.Source = extractLb();
                }
            }
            else if (b == 0xC7)
            {
                b = Buffer[X];
                int v = ((b >> 3) & 0x7);
                if (v != 0)
                {
                    _Operator = new x86Operator(IntelOpcode.Bad);
                }
                else
                {
                    _Operator = new x86Operator(IntelOpcode.Mov);
                    Length++;
                    _Operator.Destination = extractEv(b);
                    _Operator.Source = extractLz();
                }
            }
            else if (b == 0xC9)
            {
                _Operator = new x86Operator(IntelOpcode.Leave);
            }
            else if (b == 0xCC)
            {
                _Operator = new x86Operator(IntelOpcode.Int3);
            }
            else if (b == 0xCD)
            {
                _Operator = new x86Operator(IntelOpcode.Int);
                _Operator.Source = extractLb();
            }
            else if (b < 0xD0)
            {
            }
            else
            {
                throw new ArgumentException();
            }

            return _Operator;
        }

        private x86Operator Read_Dx(byte b)
        {
            x86Operator _Operator = null;
            if (b == 0xD0)
            {
                b = Buffer[X];
                int v = ((b >> 3) & 0x7);
                if (v == 6)
                {
                    _Operator = new x86Operator(IntelOpcode.Bad);
                }
                else
                {
                    switch (v)
                    {
                        case 0:
                            _Operator = new x86Operator(IntelOpcode.Rol);
                            break;
                        case 1:
                            _Operator = new x86Operator(IntelOpcode.Ror);
                            break;
                        case 2:
                            _Operator = new x86Operator(IntelOpcode.Rcl);
                            break;
                        case 3:
                            _Operator = new x86Operator(IntelOpcode.Rcr);
                            break;
                        case 4:
                            _Operator = new x86Operator(IntelOpcode.Shl);
                            break;
                        case 5:
                            _Operator = new x86Operator(IntelOpcode.Shr);
                            break;
                        case 7:
                            _Operator = new x86Operator(IntelOpcode.Sar);
                            break;
                    }
                    Length++;
                    _Operator.Destination = extractEb(b);
                    _Operator.Source = x86Operand.NewValue(1);
                }
            }
            else if (b == 0xD1)
            {
                b = Buffer[X];
                int v = ((b >> 3) & 0x7);
                if (v == 6)
                {
                    _Operator = new x86Operator(IntelOpcode.Bad);
                }
                else
                {
                    switch (v)
                    {
                        case 0:
                            _Operator = new x86Operator(IntelOpcode.Rol);
                            break;
                        case 1:
                            _Operator = new x86Operator(IntelOpcode.Ror);
                            break;
                        case 2:
                            _Operator = new x86Operator(IntelOpcode.Rcl);
                            break;
                        case 3:
                            _Operator = new x86Operator(IntelOpcode.Rcr);
                            break;
                        case 4:
                            _Operator = new x86Operator(IntelOpcode.Shl);
                            break;
                        case 5:
                            _Operator = new x86Operator(IntelOpcode.Shr);
                            break;
                        case 7:
                            _Operator = new x86Operator(IntelOpcode.Sar);
                            break;
                    }
                    Length++;
                    _Operator.Destination = extractEv(b);
                    _Operator.Source = x86Operand.NewValue(1);
                }
            }
            else
            {
            }
            return _Operator;
        }

        private x86Operator Read_Ex(byte b)
        {
            x86Operator _Operator = null;
            if (b == 0xE8)
            {
                _Operator = new x86Operator(IntelOpcode.Call);
                _Operator.Source = extractLz();
            }
            else if (b == 0xE9)
            {
                _Operator = new x86Operator(IntelOpcode.Jmp);
                _Operator.Source = extractLz();
            }
            else if (b == 0xEB)
            {
                _Operator = new x86Operator(IntelOpcode.Jmp);
                _Operator.Source = extractLb();
            }
            else
            {
            }
            return _Operator;
        }

        private x86Operator Read_Fx(byte b)
        {
            x86Operator _Operator = null;
            if (b == 0xF0)
            {
                _Operator = Read();
                _Operator.Prefix = "lock";
            }
            else if (b == 0xF1)
            {
                _Operator = new x86Operator(IntelOpcode.Icebp);
            }
            else if (b == 0xF2)
            {
                _Operator = Read();
                _Operator.Prefix = "repnz";
            }
            else if (b == 0xF3)
            {
                _Operator = Read();
                _Operator.Prefix = "repz";
            }
            else if (b == 0xF4)
            {
                _Operator = new x86Operator(IntelOpcode.Hlt);
            }
            else if (b == 0xF5)
            {
                _Operator = new x86Operator(IntelOpcode.Cmc);
            }
            else if (b == 0xF6)
            {
                b = Buffer[X];
                if (b == 0x8)
                {
                    _Operator = new x86Operator(IntelOpcode.Bad);
                    return _Operator;
                }
                _Operator = new x86Operator(IntelOpcode.Test, OperatorSize.Byte);
                Length++;
                _Operator.Destination = extractEb(b);
                _Operator.Source = extractLb();
            }
            else if (b == 0xF7)
            {
                b = Buffer[X];
                if (b == 0x8)
                {
                    _Operator = new x86Operator(IntelOpcode.Bad);
                    return _Operator;
                }
                _Operator = new x86Operator(IntelOpcode.Test, OperatorSize.DWord);
                Length++;
                _Operator.Destination = extractEv(b);
                _Operator.Source = extractLz();

            }
            else if (b < 0xFE)
                _Operator = new x86Operator(IntelOpcode.Clc + b - 0xF8);
            else if (b == 0xFE)
            {
                b = Buffer[X];
                if (((b >> 3) & 0x7) > 1)
                {
                    _Operator = new x86Operator(IntelOpcode.Bad);
                    return _Operator;
                }
                _Operator = new x86Operator(IntelOpcode.Inc + ((b >> 3) & 0x1), OperatorSize.Byte);
                Length++;
                _Operator.Source = extractEb(b);
            }
            else if (b == 0xFF)
            {
                b = Buffer[X];
                int v = ((b >> 3) & 0x7);
                if (v == 7)
                {
                    _Operator = new x86Operator(IntelOpcode.Bad);
                }
                else
                {
                    Length++;
                    switch (v)
                    {
                        case 0:
                            _Operator = new x86Operator(IntelOpcode.Inc, OperatorSize.DWord);
                            _Operator.Source = extractEv(b);
                            break;
                        case 1:
                            _Operator = new x86Operator(IntelOpcode.Dec, OperatorSize.DWord);
                            _Operator.Source = extractEv(b);
                            break;
                        case 2:
                            _Operator = new x86Operator(IntelOpcode.Call);
                            _Operator.Source = extractEv(b);
                            break;
                        case 3:
                            _Operator = new x86Operator(IntelOpcode.Call);
                            _Operator.Source = extractEv(b);
                            break;
                        case 4:
                            _Operator = new x86Operator(IntelOpcode.Jmp);
                            _Operator.Source = extractEv(b);
                            break;
                        case 5:
                            _Operator = new x86Operator(IntelOpcode.Jmp);
                            _Operator.Source = extractMa(b);
                            break;
                        case 6:
                            _Operator = new x86Operator(IntelOpcode.Push, OperatorSize.DWord);
                            _Operator.Source = extractEv(b);
                            break;
                    }
                }
            }
            else
            {
            }
            return _Operator;
        }

        private x86Operator Read()
        {
            byte b = Buffer[X];
            Length++;
            switch ((b >> 4) & 0xF)
            {
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

#endif