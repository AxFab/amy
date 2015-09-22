using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Niut;

namespace Amy
{
  class Token {
    int no_;
    public Token (int no) { no_ = no; }
    public override string ToString () { return no_.ToString(); }
  }

  [TestFixture]
  class TestExpression
  {
    int no_ = 0;
    Resolver solver;

    private void reset ()
    {
      solver = new Resolver();
      no_ = 0;
    }

    private void check (long value, Primitive type = Primitive.Int)
    {
      Assert.IsTrue(solver.Compile());
      SSAFlow flow = solver.SSA(false);
      Assert.IsEquals(value, solver.AsSigned);
      Assert.IsEquals((int)type, (int)solver.Type);
    }

    private void check_b (bool value)
    {
      Assert.IsTrue(solver.Compile());
      // solver.SSA();
      Assert.IsEquals(value, solver.AsBoolean);
      Assert.IsEquals((int)Primitive.Boolean, (int)solver.Type);
    }

    private void operand (long value, bool success = true)
    {
      Assert.IsEquals(success, solver.Push(new Token(++no_), Primitive.Int, value));
    }

    private void operation (Operator ope, bool success = true)
    {
      Assert.IsEquals(success, solver.Push(new Token(++no_), ope));
    }

    /// <summary>Basic tests with priorities using + - * / and ()</summary>
    [Test]
    public void TestMath ()
    {
      reset(); // 54 + 83 - 4 + 8 -> 141
      operand(54);
      operation(Operator.Add);
      operand(83);
      operation(Operator.Sub);
      operand(4);
      operation(Operator.Add);
      operand(8);
      check(141);

      reset(); // 135 * 6 + 54 * 2 -> 918
      operand(135);
      operation(Operator.Mul);
      operand(6);
      operation(Operator.Add);
      operand(54);
      operation(Operator.Mul);
      operand(2);
      check(918);

      reset(); // 22 * (3 + 5) - 7 * 2 -> 162
      operand(22);
      operation(Operator.Mul);
      Assert.IsTrue(solver.OpenParenthese(new Token(++no_)));
      operand(3);
      operation(Operator.Add);
      operand(5);
      Assert.IsTrue(solver.CloseParenthese(new Token(++no_)));
      operation(Operator.Sub);
      operand(7);
      operation(Operator.Mul);
      operand(2);
      check(162);

      reset(); // 35 / 8 + 3 * (1 - 9) * 0 -> 4
      operand(35);
      operation(Operator.Div);
      operand(8);
      operation(Operator.Add);
      operand(3);
      operation(Operator.Mul);
      Assert.IsTrue(solver.OpenParenthese(new Token(++no_)));
      operand(1);
      operation(Operator.Sub);
      operand(9);
      Assert.IsTrue(solver.CloseParenthese(new Token(++no_)));
      operation(Operator.Mul);
      operand(0);
      check(4);


      reset(); // 35 / (3 * 6 - 2 * 9) -> NaN
      operand(35);
      operation(Operator.Div);
      Assert.IsTrue(solver.OpenParenthese(new Token(++no_)));
      operand(3);
      operation(Operator.Mul);
      operand(6);
      operation(Operator.Sub);
      operand(2);
      operation(Operator.Mul);
      operand(9);
      Assert.IsTrue(solver.CloseParenthese(new Token(++no_)));
      Assert.IsTrue(solver.Compile());
      Assert.IsFalse(solver.AsBoolean);
      Assert.Throw(() => {
        check(0, Primitive.Error);
      });
    }

    [Test]
    public void TestLogic ()
    {
      // Test And logic table
      reset();
      operand(0);
      operation(Operator.And);
      operand(0);
      check_b(false);

      reset();
      operand(0);
      operation(Operator.And);
      operand(1);
      check_b(false);

      reset();
      operand(1);
      operation(Operator.And);
      operand(0);
      check_b(false);

      reset();
      operand(1);
      operation(Operator.And);
      operand(1);
      check_b(true);

      // Test Or logic table
      reset();
      operand(0);
      operation(Operator.Or);
      operand(0);
      check_b(false);

      reset();
      operand(0);
      operation(Operator.Or);
      operand(1);
      check_b(true);

      reset();
      operand(1);
      operation(Operator.Or);
      operand(0);
      check_b(true);

      reset();
      operand(1);
      operation(Operator.Or);
      operand(1);
      check_b(true);
    }

    [Test]
    public void TestWtf ()
    {
      reset(); // 35 8  .. 35 * .. 35 * 4
      solver = new Resolver();
      operand(35);
      operand(8, false);
      operation(Operator.Mul, false);
      Assert.IsFalse(solver.Compile());
      operand(4, false);
      Assert.IsFalse(solver.Compile());

      reset(); // * 8
      solver = new Resolver();
      operation(Operator.Mul, false);
      operand(8, false);
      Assert.IsFalse(solver.Compile());
    }

    static void Main (string[] args)
    {
      TestExpression ex = new TestExpression();
      ex.TestMath();
      ex.TestLogic();
      ex.TestWtf();

      Runner.RunTests(typeof(TestExpression));
    }
  }
}
