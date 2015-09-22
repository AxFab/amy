using System;
using System.Collections.Generic;

namespace Amy.Syntax
{
    public enum StorageQualifier
    {
        None, Const, Volatile, Pointer, ConstPointer, VolatilePointer, Array,
    }

    [Flags]
    public enum PrimitiveType
    {
        Struct = 1,
        Integer = 2,
        Float = 4,
        Void = 8,
        Mask = 15,
        Signed = 16,
        Unsigned = 32,
        Enum = 64,
        Union = 128,
    }

    public class ASTType
    {
        readonly PrimitiveType Primitive;
        readonly int Length;

        readonly StorageQualifier Sq;
        readonly ASTType PointTo;

        List<ASTOperand> Fields = new List<ASTOperand>();

        static Dictionary<string, ASTType> TypesPool = new Dictionary<string, ASTType>();

        public static ASTType NewType(PrimitiveType type, int length)
        {
            ASTType atype = new ASTType(type, length);
            ASTType stype = null;
            if (!TypesPool.TryGetValue(atype.ToString(), out stype))
            {
                stype = atype;
                TypesPool.Add(atype.ToString(), atype);
            }

            return stype;
        }

        public static ASTType NewType(ASTType type, StorageQualifier sq)
        {
            ASTType atype = new ASTType(type, sq);
            ASTType stype = null;
            if (!TypesPool.TryGetValue(atype.ToString(), out stype))
            {
                stype = atype;
                TypesPool.Add(atype.ToString(), atype);
            }

            return stype;
        }

        public static ASTType NewType(PrimitiveType type, params ASTOperand[] fields)
        {
            if ((type & PrimitiveType.Struct | PrimitiveType.Union) == 0)
                throw new Exception("Only union or struct can have fields");
            ASTType atype = new ASTType(type, fields);
            ASTType stype = null;
            if (!TypesPool.TryGetValue(atype.ToString(), out stype))
            {
                stype = atype;
                TypesPool.Add(atype.ToString(), atype);
            }

            return stype;
        }

        private ASTType(PrimitiveType type, int length)
        {
            Length = length != 0 ? length : 4;
            Primitive = type;
            if ((type & PrimitiveType.Union) != 0)
                Primitive |= PrimitiveType.Struct;
            else if ((type & PrimitiveType.Enum) != 0 && (type & PrimitiveType.Mask) == 0)
                Primitive |= PrimitiveType.Integer;
        }

        private ASTType(PrimitiveType type, params ASTOperand[] fields)
        {
            Length = 0;
            foreach (ASTOperand op in fields)
            {
                Fields.Add(op);
                if (Length + op.Type.Length % 4 != 0)
                    Length += (4 - (Length % 4)) % 4;
                op.Position = Length;
                Length += op.Type.Length;
            }
            Primitive = type;
            if ((type & PrimitiveType.Union) != 0)
                Primitive |= PrimitiveType.Struct;
        }

        private ASTType(ASTType type, StorageQualifier sq)
        {
            PointTo = type;
            Sq = sq;
            Length = 4; // TODO Depend of architecture
            Primitive = PrimitiveType.Void;
        }

        public ASTType PushField(string name, ASTType type)
        {
            Fields.Add(new ASTOperand(name, type, ASTOperandType.Structure));
            return this;
        }

        public override string ToString()
        {
            string str = string.Empty;

            if ((Primitive & PrimitiveType.Enum) != 0)
                str += "e";
            if ((Primitive & PrimitiveType.Union) != 0)
                str += "u";
            else if ((Primitive & PrimitiveType.Struct) != 0)
                str += "s";
            switch (Primitive & PrimitiveType.Mask)
            {
                case PrimitiveType.Float:
                    str += "f";
                    str += (Length * 8);
                    break;

                case PrimitiveType.Integer:
                    if ((Primitive & PrimitiveType.Unsigned) != 0)
                        str += "u";
                    else
                        str += "i";
                    str += (Length * 8);
                    break;

                case PrimitiveType.Struct:
                    str += "{";
                    foreach (ASTOperand op in Fields)
                    {
                        if (str[str.Length - 1] != '{')
                            str += ",";
                        str += op.Type.ToString();
                    }
                    str += "}";
                    break;

                case PrimitiveType.Void:
                    if (PointTo == null)
                        return "v";
                    switch (Sq)
                    {
                        case StorageQualifier.Array:
                            str = PointTo.ToString() + "a";
                            break;

                        case StorageQualifier.Const:
                            str = "C" + PointTo.ToString();
                            break;

                        case StorageQualifier.Volatile:
                            str = "V" + PointTo.ToString();
                            break;

                        case StorageQualifier.Pointer:
                            str = PointTo.ToString() + "p";
                            break;

                        case StorageQualifier.ConstPointer:
                            str = PointTo.ToString() + "c";
                            break;

                        case StorageQualifier.VolatilePointer:
                            str = PointTo.ToString() + "v";
                            break;
                    }
                    break;
            }

            return str;
        }
    }

}
