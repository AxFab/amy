using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmySuite
{
  class Option<T>
  {
    public string Name;
    public T group;
    public char shortcut;
    public string option;
    public string args;
    public string brief;

    public Option (string name, T group, char sh, string lg, string arg, string brief)
    {
      this.Name = name;
      this.group = group;
      this.shortcut = sh;
      this.option = lg;
      this.args = arg;
      this.brief = brief;
    }
  }
}
