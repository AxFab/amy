using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Amy.Lexer;

namespace Amy.Syntax
{
    public class ASTNode
    {
        static readonly int[] Priorities = {
              0, 0, 0,
              12, 12, 13, 13, 13, 8, 6, 7, 11, 11, 11,
              10, 10, 10, 10, 9, 9,
              3, 4, 5, 0,
              15, 15, 0 };

        public ASTNode(Token token, ASTOperand operand)
        {
            Token = token;
            Priority = 0;
            Operand = operand;
            Type = operand.Type;
            Operator = ASTOperator.None;
        }

        public ASTNode(Token token, ASTType type, long value)
        {
            Token = token;
            Type = type;
            Operator = ASTOperator.None;
            Operand = null;
            IValue = value;
            Computed = true;
        }

        public ASTNode(Token token, ASTOperator op)
        {
            Token = token;
            Operator = op;
            Priority = Priorities[(int)op];
        }

        public void ComputeSingle()
        {
            this.Type = this.Right.Type;
            if (!this.Right.Computed)
                return;
            switch (this.Operator)
            {
                case ASTOperator.Not:
                    // FIXME check Right is a boolean
                    this.BValue = !this.Right.BValue;
                    break;

                case ASTOperator.Neg:
                    return;

                default:
                    throw new Exception();
            }
            this.Computed = true;
        }

        public void ComputeDouble()
        {
            if (!this.Right.Computed || !this.Left.Computed)
            {
                if (this.Operator != ASTOperator.Add) // FIXME Commutable/Associativity
                    return;

                if (this.Left.Computed)
                {
                    var temp = this.Left;
                    this.Left = this.Right;
                    this.Right = temp;
                }

                if (this.Right.Computed && this.Operator == this.Left.Operator && this.Left.Right.Computed)
                {
                    var temp = this.Left;
                    this.Left = this.Right;
                    this.Right = temp;

                    temp = this.Left;
                    this.Left = Right.Left;
                    this.Right.Left = temp;

                    this.Right.ComputeDouble();
                    /*
                     *     +      +      +
                     *    + 4    4 +    x +_
                     *   x 3      x 3    4 3
                     *
                     *     +         *
                     *    * 12      +_6
                     *   + 6       + 2
                     *  x 4       x 4
                     *
                     */
                }

                return;
            }

            switch (this.Operator)
            {
                // FIXME check Types
                case ASTOperator.Add:
                    this.IValue = this.Left.IValue + this.Right.IValue;
                    break;

                case ASTOperator.Sub:
                    this.IValue = this.Left.IValue - this.Right.IValue;
                    break;

                case ASTOperator.Mul:
                    this.IValue = this.Left.IValue * this.Right.IValue;
                    break;

                case ASTOperator.Div:
                    if (this.Right.IValue != 0)
                        this.IValue = this.Left.IValue / this.Right.IValue;
                    else
                    {
                        // WRITE THIS WILL ALWAYS TRIGGER AN ERROR
                        return;
                    }
                    break;

                case ASTOperator.Rem:
                    if (this.Right.IValue != 0)
                        this.IValue = this.Left.IValue % this.Right.IValue;
                    else
                    {
                        // WRITE THIS WILL ALWAYS TRIGGER AN ERROR
                        return;
                    }
                    break;

                case ASTOperator.And:
                case ASTOperator.Or:
                case ASTOperator.Xor:
                case ASTOperator.Shl:
                case ASTOperator.Lshr:
                case ASTOperator.Ashr:
                    return;

                case ASTOperator.Less:
                    this.BValue = this.Left.IValue < this.Right.IValue;
                    break;

                case ASTOperator.LessEq:
                    this.BValue = this.Left.IValue <= this.Right.IValue;
                    break;

                case ASTOperator.More:
                    this.BValue = this.Left.IValue > this.Right.IValue;
                    break;

                case ASTOperator.MoreEq:
                    this.BValue = this.Left.IValue >= this.Right.IValue;
                    break;

                case ASTOperator.Equals:
                    this.BValue = this.Left.IValue == this.Right.IValue;
                    break;

                case ASTOperator.NotEquals:
                    this.BValue = this.Left.IValue != this.Right.IValue;
                    break;

                default:
                    throw new Exception();
            }
            this.Computed = true;
        }

        public Token Token;
        public ASTOperand Operand;
        public ASTOperator Operator;
        public ASTType Type;
        public int Priority;
        public long IValue;
        public bool BValue;
        public bool Computed;
        public ASTNode Left;
        public ASTNode Right;

        public string ToSSA()
        {
            if (Computed)
            {
                return IValue.ToString();
            }
            else if (Operator == ASTOperator.None)
            {
                if (Operand != null)
                    return Operand.ToString();
                return "!?";
            }

            return "!!?";
        }
    }

}
