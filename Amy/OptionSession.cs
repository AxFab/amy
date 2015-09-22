using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmySuite
{
  class OptionSession<S, T>
    where S : new()
    where T : struct, IConvertible
  {
    List<Option<T>> allOptions_ = new List<Option<T>>();

    public void NewOption (string name, T group, char sh, string lg, string arg, string brief)
    {
      Option<T> opt = new Option<T>(name, group, sh, lg, arg, brief);
      allOptions_.Add(opt);
    }

    public int MargeWidth = 20;

    public int TextWidth;

    public void Usage ()
    {
      TextWidth = Console.BufferWidth - MargeWidth;

      Console.WriteLine("Usage:");
      Console.WriteLine("    amy [options] files...");

      foreach (T val in Enum.GetValues(typeof(T))) {
        Console.WriteLine();
        Console.WriteLine(Enum.GetName(typeof(T), val).Replace('_', ' ') + " options:");
        foreach (Option<T> opt in allOptions_) {
          if (opt.group.Equals(val)) {
            string strm = string.Empty;
            if (opt.option == null && opt.args == null)
              strm = string.Format("-{0}", opt.shortcut);
            else if (opt.option == null)
              strm = string.Format("-{0} {1}", opt.shortcut, opt.args);
            else if (opt.shortcut != '\0')
              strm = string.Format("-{0}, --{1} {2}", opt.shortcut, opt.option, opt.args);
            else if (opt.args == null)
              strm = string.Format("--{0}", opt.option);
            else
              strm = string.Format("--{0} {1}", opt.option, opt.args);



            string str = string.Empty;
            string brief = opt.brief;
            bool first = true;
            while (brief.Length > 0) {
              if (brief.Length > TextWidth) {
                int width = TextWidth;
                str = brief.Substring(0, width);
                width = str.LastIndexOf(' ');
                str = brief.Substring(0, width);
                brief = brief.Substring(width);
              } else {
                str = brief;
                brief = string.Empty;
              }

              str = str.Trim();
              if (first) {
                Console.WriteLine("  {0,-" + (MargeWidth - 4) + "}  {1}", strm, str);
                first = false;
              } else
                Console.WriteLine("{0}{1}", string.Empty.PadLeft(MargeWidth, ' '), str);
            }
          }
        }
      }
      Console.WriteLine();
    }

    public Option<T> LookForShortOption (char sh)
    {
      foreach (Option<T> opt in allOptions_) {
        if (opt.shortcut == sh) {
          return opt;
        }
      }
      return null;
    }

    public Option<T> LookForLongOption (string lg)
    {
      foreach (Option<T> opt in allOptions_) {
        if (lg == opt.option || lg.StartsWith(opt.option + "=")) {
          return opt;
        }
      }
      return null;
    }

    public List<string> Parse (string[] args, S config)
    {
      int err = 0;
      List<string> parameters = new List<string>();
      for (int i = 0; i < args.Length; ++i) {
        if (args[i] == "--")
          break;

        if (args[i].StartsWith("--")) {
          string value = null;
          Option<T> opt = LookForLongOption(args[i].Substring(2));
          if (opt == null) {
            Console.Error.WriteLine("Option " + args[i] + " is not recognized.");
            err++;
            continue;
          }

          if (opt.args != null) {
            int v = args[i].IndexOf('=');
            if (v > 0) {
              value = args[i].Substring(v + 1);
            } else {
              ++i;
              if (i < args.Length) {
                value = args[i];
                if (value.StartsWith("-")) {
                  Console.Error.WriteLine("Option " + args[i] + " need an argument.");
                  err++;
                  --i;
                  break;
                }
              } else {
                Console.Error.WriteLine("Option " + args[i] + " need an argument.");
                err++;
                break;
              }
            }

            System.Reflection.MemberInfo m = typeof(S).GetMember(opt.Name)[0];
            if (m.MemberType == System.Reflection.MemberTypes.Field) {
              System.Reflection.FieldInfo f = (System.Reflection.FieldInfo)m;
              if (f.FieldType == typeof(List<string>))
                (f.GetValue(config) as List<string>).Add(value);
              else
                f.SetValue(config, value);
            } else if (m.MemberType == System.Reflection.MemberTypes.Method) {
              typeof(S).InvokeMember(opt.Name, System.Reflection.BindingFlags.InvokeMethod, null, config, new object[] { value });
            }

            // typeof(S).GetField(opt.Name).SetValue(config, value);
            // Console.WriteLine("OPTION " + opt.option + "(" + opt.shortcut + ") ; VALUE " + value);
          } else {
            typeof(S).GetField(opt.Name).SetValue(config, true);
            // Console.WriteLine("OPTION " + opt.option + "("+opt.shortcut+")");
          }
        } else if (args[i].StartsWith("-")) {
          for (int j = 1; j < args[i].Length; ++j) {
            string value = null;
            Option<T> opt = LookForShortOption(args[i][j]);
            if (opt == null) {
              Console.Error.WriteLine("Option -" + args[i][j] + " is not recognized.");
              err++;
            } else if (opt.args != null) {
              if (j == args[i].Length - 1) {
                ++i;
                if (i < args.Length) {
                  value = args[i];
                  if (value.StartsWith("-")) {
                    Console.Error.WriteLine("Option -" + args[i][j] + " need an argument.");
                    err++;
                    --i;
                    break;
                  }
                } else {
                  Console.Error.WriteLine("Option -" + args[i][j] + " need an argument.");
                  err++;
                  break;
                }
              } else {
                value = args[i].Substring(j + 1);
              }
            }

            if (value != null) {

              System.Reflection.MemberInfo m = typeof(S).GetMember(opt.Name)[0];
              if (m.MemberType == System.Reflection.MemberTypes.Field) {
                System.Reflection.FieldInfo f = (System.Reflection.FieldInfo)m;
                if (f.FieldType == typeof(List<string>))
                  (f.GetValue(config) as List<string>).Add(value);
                else
                  f.SetValue(config, value);
              } else if (m.MemberType == System.Reflection.MemberTypes.Method) {
                typeof(S).InvokeMember(opt.Name, System.Reflection.BindingFlags.InvokeMethod, null, config, new object[] { value });
              }

              // Console.WriteLine("OPTION " + opt.option + "(" + opt.shortcut + ") ; VALUE " + value);
              break;
            } else {
              typeof(S).GetField(opt.Name).SetValue(config, true);
              // Console.WriteLine("OPTION " + opt.option + "(" + opt.shortcut + ")");
            }
          }
        } else {
          parameters.Add(args[i]);
        }
      }

      if (err > 0) {
        Usage();
        Environment.Exit(-1);
      }

      return parameters;
    }
  }

}
