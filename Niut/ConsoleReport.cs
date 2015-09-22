using System;
using System.Reflection;

namespace Niut
{
  public class ConsoleReport : IReporter
  {
    public ConsoleReport (string name)
    {
      Console.WriteLine("Start test suite: " + name + " at " + DateTime.Now.ToString());
    }

    public void TestCase (MethodInfo test, Exception exc, long timeMs)
    {
      string info = "Success";
      string msg = string.Empty;

      AssertException asEx = exc as AssertException;
      if (asEx != null) {
        msg = asEx.Message;
        info = "Failure";
      } else if (exc != null) {
        msg = exc.Message;
        info = " Error ";
      }

      Console.WriteLine(" [{0}] {1}.{2} {4}({3}ms)",
        info, test.DeclaringType.FullName, test.Name, timeMs, msg);
    }

    public void AddProperty (string key, string value)
    {
      Console.WriteLine("Set: {0} <- {1}", key, value);
    }

    public void EndSuite (int tests, int errors, int failures, long timeMs)
    {
      int successes = tests - errors - failures;
      double completed = successes * 100.0 / tests;
      if (successes == tests) {
        Console.WriteLine("Full success, {0} tests.", successes);
      } else {
        Console.WriteLine("Finish {0:0.00}% succeed, {2} error, {3} failure on {1} tests.",
          completed, tests, errors, failures);
      }
    }
  }


}
