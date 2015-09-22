using System;

namespace Niut
{
  public class AssertException : Exception
  {
    public string Type { get; private set; }
    public AssertException (string msg, string type)
      : base(msg)
    {
      Type = type;
    }
  }
}
