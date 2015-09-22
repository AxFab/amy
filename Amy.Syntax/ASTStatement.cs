using System;
using System.Collections.Generic;

using Amy.Lexer;

namespace Amy.Syntax
{

    public class ASTStatement
    {
        Stack<ASTNode> postFixStack = new Stack<ASTNode>();
        Stack<ASTNode> inFixStack = new Stack<ASTNode>();

        private void addPostFixOperand(ASTNode node)
        {
            postFixStack.Push(node);
        }

        private void addPostFixOperator(ASTNode node)
        {
            if (node.Operator > ASTOperator.Single && node.Operator < ASTOperator.End)
            {
                if (postFixStack.Count < 1)
                {
                    throw new Exception();
                }

                ASTNode oprd1 = postFixStack.Pop();

                node.Right = oprd1;
                node.ComputeSingle();
            }
            else if (node.Operator > ASTOperator.Binary && node.Operator < ASTOperator.Single)
            {
                if (postFixStack.Count < 2)
                {
                    throw new Exception();
                }

                ASTNode oprd2 = postFixStack.Pop();
                ASTNode oprd1 = postFixStack.Pop();


                // Assign have Associativity R to L
                node.Left = oprd1;
                node.Right = oprd2;
                node.ComputeDouble();
            }
            else
            {
                throw new Exception();
            }

            postFixStack.Push(node);

            // Write SSA
        }

        public ASTStatement OpenParenthesis(Token token)
        {
            ASTNode node = new ASTNode(token, ASTOperator.Parenthesis);
            inFixStack.Push(node);
            return this;
        }

        public ASTStatement CloseParenthesis(Token token)
        {
            ASTNode node = inFixStack.Pop();
            while (node.Operator != ASTOperator.Parenthesis)
            {
                addPostFixOperator(node);
                node = inFixStack.Pop();
                if (node == null)
                {
                    throw new Exception("Too much parenthesis");
                }
            }
            return this;
        }

        public ASTStatement PushOperand(Token token, ASTOperand operand)
        {
            ASTNode node = new ASTNode(token, operand);
            addPostFixOperand(node);
            return this;
        }

        public ASTStatement PushInteger(Token token, ASTType type, long value)
        {
            ASTNode node = new ASTNode(token, type, value);
            addPostFixOperand(node);
            return this;
        }

        public ASTStatement PushOperator(Token token, ASTOperator opcode)
        {
            ASTNode node = new ASTNode(token, opcode);
            while (inFixStack.Count > 0 && inFixStack.Peek().Priority > node.Priority)
            {
                ASTNode nd = inFixStack.Pop();
                addPostFixOperator(nd);
            }

            inFixStack.Push(node);
            return this;
        }

        public void Compile()
        {
            while (inFixStack.Count > 0)
            {
                ASTNode pop = inFixStack.Pop();
                addPostFixOperator(pop);
            }
        }

        public void Convert(ASTNode node)
        {
            if (node.Left != null && node.Left.Operand == null && node.Left.Computed == false)
                Convert(node.Left);
            if (node.Right != null && node.Right.Operand == null && node.Right.Computed == false)
                Convert(node.Right);

            if (node.Operator > ASTOperator.Single && node.Operator < ASTOperator.End)
            {

            }
            else if (node.Operator > ASTOperator.Binary && node.Operator < ASTOperator.Single)
            {
                if (node.Operator == ASTOperator.Assign)
                {
                    Console.WriteLine("{1} {0} {2}", node.Operator, node.Left.ToSSA(), node.Right.ToSSA());
                }
                else
                    Console.WriteLine("rA <- {1} {0} {2}", node.Operator, node.Left.ToSSA(), node.Right.ToSSA());


            }
            else
            {
                throw new Exception();
            }

        }

        public void Convert()
        {
            ASTNode nd = postFixStack.Peek();
            Convert(nd);

        }
    }

}
