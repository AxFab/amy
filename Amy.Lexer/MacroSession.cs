using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amy.Lexer
{
  class MacroSession
  {
    Dictionary<string, Macro> allMacros_ = new Dictionary<string, Macro>();
    // Tokenizer lexer;

    public bool Actif { get { return storeToken_.Count != 0; } }

    public Macro FindMacro (string name)
    {
      Macro mp;
      allMacros_.TryGetValue(name, out mp);
      return mp;
    }

    List<List<Token>> currentArgs_;
    Macro currentMacro_;

    public int SetUpMacro (Macro macro, Token token)
    {
      currentMacro_ = macro;
      // currentArgs_ = null;
      // currentIdx_ = 0;
      return macro.Arguments.Count;
    }

    public void Compile (Queue<Token> store, List<Token> value, Macro md, HashSet<string> parents)
    {
      foreach (Token tk in value) {
        if (md != null && md.Arguments.Count > 0) {
          int aIdx = md.Arguments.IndexOf(tk.Litteral);
          if (aIdx >= 0) {
            // FIXME check args numbers
            Compile(store, currentArgs_[aIdx], null, parents);
            continue;
          }
        }

        Macro sub = FindMacro(tk.Litteral);
        bool cnt = !parents.Contains(tk.Litteral); // DEBUG
        if (sub != null && !parents.Contains(tk.Litteral)) {
          if (sub.Arguments.Count > 0) {
            // Read ARGS ON
          }

          HashSet<string> newHs = new HashSet<string>(parents);
          newHs.Add(sub.Name);
          Compile(store, sub.Value, md, newHs);
          continue;
        }

        store.Enqueue(tk);
      }

    }

    Queue<Token> storeToken_ = new Queue<Token>();
    public void SetArguments (List<List<Token>> args)
    {
      currentArgs_ = args;
      storeToken_.Clear();
      if (currentMacro_.Value.Count > 0)
        Compile(storeToken_, currentMacro_.Value, currentMacro_, new HashSet<string>(new string[] { currentMacro_.Name }));
    }

    public void Define (string name, List<Token> value = null, List<string> args = null)
    {
      Macro md;
      if (allMacros_.TryGetValue(name, out md)) {
        // AlreadyExist
        allMacros_.Remove(name);
      }
      md = new Macro(name, value, args);
      allMacros_.Add(name, md);
    }

    public void Undef (string name)
    {
      allMacros_.Remove(name);
    }

    public bool Defined (string name)
    {
      return allMacros_.ContainsKey(name);
    }


    public Token ReadToken ()
    {
      Macro md;
      if (storeToken_.Count == 0)
        return null;
      return storeToken_.Dequeue();
    }
  }

}
