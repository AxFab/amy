using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amy.Syntax
{

    public enum ASTOperandType
    {
        Structure,
        RData,
        Data,
    }

    public class ASTOperand
    {
        string Name;
        public ASTType Type;
        ASTOperandType Flags;
        public int Position;

        public ASTOperand(string name, ASTType type, ASTOperandType flags)
        {
            Name = name;
            Type = type;
            Flags = flags;
        }

        public override string ToString()
        {
            return Name + " :" + Type;
        }
    }

}
