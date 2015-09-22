using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amy.Lexer
{
  public class Token
  {
    public string File { get; set; }
    public Position Start { get; set; }
    public Position End { get; set; }
    public string Litteral { get; set; }
    public int Type { get; set; }
  }
}
