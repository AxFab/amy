using System;
using System.Reflection;

namespace Niut
{
  public interface IReporter
  {
    void TestCase (MethodInfo test, Exception exc, long timeMs);
    void AddProperty (string key, string value);
    void EndSuite (int tests, int errors, int failures, long timeMs);
  }
}
