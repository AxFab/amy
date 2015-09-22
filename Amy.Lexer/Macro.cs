using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amy.Lexer
{
  class Macro
  {
    public string Name;
    public List<Token> Value;
    public List<string> Arguments;
    public Macro (string name, List<Token> value = null, List<string> args = null)
    {
      Name = name;
      Value = value ?? new List<Token>();
      Arguments = args ?? new List<string>();
    }
  }
}
