using System.Diagnostics;

namespace Niut 
{
  public class Debug
  {
    public static void Assert (bool condition)
    {
      Niut.Assert.IsTrue(condition);
    }
  }

  public class Assert
  {
    private static void error (string msg)
    {
      var stack = new StackTrace(1);
      var frame = stack.GetFrame(0);
      string type = frame.GetMethod().Name;
      throw new AssertException(msg, type);
    }

    public static void IsEquals (int expected, int actual)
    {
      if (expected != actual)
        error("Expected '" + expected + "' got '" + actual + "'");
    }

    public static void IsEquals (long expected, long actual)
    {
      if (expected != actual)
        error("Expected '" + expected + "' got '" + actual + "'");
    }

    public static void IsEquals (bool expected, bool actual)
    {
      if (expected != actual)
        error("Expected '" + expected + "' got '" + actual + "'");
    }

    public static void IsTrue (bool actual)
    {
      if (!actual)
        error("Expected to be True got '" + actual + "'");
    }

    public static void IsFalse (bool actual)
    {
      if (actual)
        error("Expected to be False got '" + actual + "'");
    }

    public static void Throw (System.Action action, System.Type exType = null) 
    {
      if (exType == null)
        exType = typeof(System.Exception);
      try {
        action();
        error("Expected to throw '"+ exType + "' got nothing.");
      } catch (System.Exception ex) {
        if (!exType.IsInstanceOfType(ex))
          error("Expected to throw '" + exType + "' got '" + ex.GetType() + "'.");
      }
    }
  }
}
