using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Niut
{
  public class JUnitReport : IReporter
  {
    XElement root;
    Stream Out;
    Stream Err;
    TextWriter BackupOut;
    TextWriter BackupErr;

    public JUnitReport (string name)
    {
      Out = new MemoryStream();
      Err = new MemoryStream();
      root = new XElement("testsuite",
        new XAttribute("name", name),
        new XAttribute("timestamp", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")),
        new XAttribute("hostname", Environment.MachineName)
        );
      BackupErr = Console.Error;
      BackupOut = Console.Out;
      Console.SetError(new StreamWriter(Err));
      Console.SetOut(new StreamWriter(Out));
    }

    public void TestCase (MethodInfo test, Exception exc, long timeMs)
    {
      XElement tcase = new XElement("testcase",
        new XAttribute("name", test.Name),
        new XAttribute("classname", test.DeclaringType.FullName),
        new XAttribute("time", timeMs / 1000.0));
      AssertException asEx = exc as AssertException;
      if (asEx != null)
        tcase.Add(new XElement("error",
          new XAttribute("message", asEx.Message),
          new XAttribute("type", asEx.Type),
          exc.ToString()
          ));
      else if (exc != null)
        tcase.Add(new XElement("failure",
          new XAttribute("message", exc.Message),
          new XAttribute("type", exc.GetType()),
          exc.ToString()
          ));
      root.Add(tcase);
    }

    public void AddProperty (string key, string value)
    {
      root.Add(new XElement("properties",
          new XAttribute("name", "arch"),
          new XAttribute("value", "x86")
          )
        );
    }

    public void EndSuite (int tests, int errors, int failures, long timeMs)
    {
      Console.Out.Flush();
      Console.Error.Flush();
      StreamReader rdOut = new StreamReader(Out);
      rdOut.BaseStream.Seek(0, SeekOrigin.Begin);
      StreamReader rdErr = new StreamReader(Err);
      rdErr.BaseStream.Seek(0, SeekOrigin.Begin);
      Console.SetError(BackupErr);
      Console.SetOut(BackupOut);
      root.Add(
        new XAttribute("tests", tests),
        new XAttribute("failures", failures),
        new XAttribute("errors", errors),
        new XAttribute("time", timeMs / 1000.0),
        new XElement("system-out", new XCData(rdOut.ReadToEnd())),
        new XElement("system-err", new XCData(rdErr.ReadToEnd()))
        );
      Console.WriteLine(root);
    }
  }
}
