using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Niut
{
  public class Runner
  {
    private IReporter report;
    private int Failures { get; set; }
    private int Errors { get; set; }
    private int Tests { get; set; }

    private void parseOption(string option) 
    {
    }

    private void testEnd(Exception exc) 
    {
      Tests++;
      AssertException asEx = exc as AssertException;
      if (asEx != null)
        Failures++;
      else if (exc != null)
        Errors++;
    }

    private void run(MethodInfo test, object fixture, 
                     MethodInfo setup, MethodInfo teardown) 
    {
      Stopwatch chrono = new Stopwatch();
      chrono.Start();
      Exception ex = CallWithTimeout(() => {
        var args = new object[0];
        if (setup != null)
          setup.Invoke(fixture, args);
        test.Invoke(fixture, args);
        if (teardown != null)
          teardown.Invoke(fixture, args);
      }, 5000);
      chrono.Stop();
      testEnd(ex);
      report.TestCase(test, ex, chrono.ElapsedMilliseconds);
    }

    private void run(Type type) 
    {
      var fixture = Activator.CreateInstance(type);
      MethodInfo setup = type.GetMethod("setup");
      MethodInfo teardown = type.GetMethod("teardown");
      foreach (MethodInfo test in type.GetMethods()) {
        var obj = test.GetCustomAttributes(typeof(TestAttribute), true);
        if (obj.Length != 0)
          run(test, fixture, setup, teardown);
      }
    }

    private void run (Assembly library)
    {
      foreach (Type type in library.GetTypes()) {
        var obj = type.GetCustomAttributes(typeof(TestFixtureAttribute), true);
        if (obj.Length != 0) {
          this.run(type);
        }
      }
    }

    Stopwatch suiteChrono = new Stopwatch();

    public void Start (IReporter report)
    {
      Tests = 0;
      Errors = 0;
      Failures = 0;
      suiteChrono.Start();
      this.report = report ?? new JUnitReport("Unamed");
    }

    public void Stop ()
    {
      suiteChrono.Stop();
      report.EndSuite(Tests, Errors, Failures, suiteChrono.ElapsedMilliseconds);
    }

    public static void RunTests (Type type)
    {
      Runner runner = new Runner();
      runner.Start(new ConsoleReport(type.Name));
      runner.run(type);
      runner.Stop();
    }

    public static void RunTests (Assembly library)
    {
      Runner runner = new Runner();
      runner.Start(new ConsoleReport(library.FullName));
      runner.run(library);
      runner.Stop();
    }

    static Exception CallWithTimeout (Action action, int timeoutMilliseconds)
    {
      Thread threadToKill = null;
      Exception error = null;
      Action wrappedAction = () => {
        threadToKill = Thread.CurrentThread;
        try {
          action();
        } catch (Exception ex) {
          error = ex;
        }
      };

      IAsyncResult result = wrappedAction.BeginInvoke(null, null);
      if (result.AsyncWaitHandle.WaitOne(timeoutMilliseconds)) {
        wrappedAction.EndInvoke(result);
      } else {
        threadToKill.Abort();
        error = new TimeoutException();
      }

      return error;
    }

    static void Main(string[] args) {
      Runner runner = new Runner();
      runner.Start(new JUnitReport("Unknown"));
      foreach (string arg in args) {
        if (arg.StartsWith("-")) {
          runner.parseOption(arg);
        } else {
          Assembly library = Assembly.LoadFile(arg);
          runner.run(library);
        }
      }
      runner.Stop();
    }
  }

}
