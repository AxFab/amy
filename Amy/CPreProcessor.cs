using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amy;
using Amy.Lexer;

namespace AmySuite
{


  class CprBlock
  {
    public CprBlock (int level, bool get, bool already, Token tok)
    {
      this.token = tok;
      this.level = level;
      this.getBlock = get;
      this.alreadyGet = already;
    }

    public Token token;
    public int level;
    public bool getBlock;
    public bool alreadyGet;
  }

  class CPreProcessor
  {
    Stack<Tokenizer> fileReaderStack = new Stack<Tokenizer>();
    Stack<string> fileNameStack = new Stack<string>();
    Stack<CprBlock> blockStack = new Stack<CprBlock>();

    public CPreProcessor (string path)
    {
      ErrorReport.PreProcessor("# \"" + path + "\"");
      fileReaderStack.Push(new Tokenizer(path, Language.CLanguage()));
      fileNameStack.Push(path);

      Define(null, "__i386__");
      Define(null, "_GNU_SOURCE");

      blockStack.Push(new CprBlock(0, true, true, null));
    }

    private bool IgnoreBlock
    {
      get
      {
        return !blockStack.Peek().getBlock;
      }
    }

    private int BlockLevel
    {
      get
      {
        return blockStack.Peek().level;
      }
    }

    List<string> IncludeDir = new List<string>();
    public void AddIncludeDir (string path)
    {
      IncludeDir.Add(path);
    }

    private string getHeader (string path)
    {
      foreach (string idir in IncludeDir)
        if (File.Exists(idir + '/' + path))
          return idir + '/' + path;
      return null;
    }

    private void Include (Token token, string path)
    {
      string header = null;
      if (path[0] == '<') {
        path = path.Substring(1, path.IndexOf('>') - 1);
        header = getHeader(path);
      } else if (path[0] == '"') {
        path = path.Substring(1, path.IndexOf('"', 1) - 1);
        if (File.Exists(path))
          header = path;
        else
          header = getHeader(path);
      } else {
        throw new Exception();
      }


      if (header != null) {
        // path = "./" + path;
        ErrorReport.PreProcessor("# \"" + header + "\"");
        fileReaderStack.Push(new Tokenizer(header, Language.CLanguage()));
        fileNameStack.Push(path);
      } else {
        ErrorReport.Error("# Inable to find header file " + path);
      }
    }

    public void Define (Token token, string value)
    {
      List<Token> toks = new List<Token>();
      string key;
      string[] args = null;
      int k = value.IndexOfAny(Tokenizer.WhiteChars);
      int k2 = value.IndexOf('(');

      if (k2 >= 0 && k2 < k) {
        key = value.Substring(0, k2);
        value = value.Substring(k2);
        k = value.IndexOf(')');
        args = value.Substring(1, k - 1).Split(new char[] { ',' });
        value = value.Substring(k + 1).Trim();
        toks = Tokenizer.AnalyzeString(Language.CLanguage(), value);

        // ErrorReport.ToImplement("-- Need to define macro '" + key + "'");
      } else if (k < 0) {
        key = value;
      } else {
        key = value.Substring(0, k);
        value = value.Substring(k).Trim();
        toks = Tokenizer.AnalyzeString(Language.CLanguage(), value);
      }

      ErrorReport.PreProcessor2("# Macro define '" + key + "'" + GetTokenPosition(token));
      Macro md = mssesion.FindMacro(key);
      if (md != null) {
        ErrorReport.Error("Redefine macro " + key);
      }

      if (args != null)
        mssesion.Define(key, toks, new List<string>(args));
      else
        mssesion.Define(key, toks);

    }

    private string GetTokenPosition (Token token)
    {
      if (token == null)
        return string.Empty;
      return string.Format(" at {0}:{1}", token.File, token.Start.Row);
    }

    private void Undef (Token token, string value)
    {
      int k = value.IndexOfAny(Tokenizer.WhiteChars);
      int k2 = value.IndexOf('(');
      if (k2 >= 0 && k2 < k)
        k = k2;
      if (k >= 0) {
        value = value.Substring(0, k);
      }

      ErrorReport.PreProcessor2("# Macro undef '" + value + "'" + GetTokenPosition(token));
      mssesion.Undef(value);
      // macros.Remove(value);
    }

    // protected bool IsDefined(string macro)
    // {
    //     return macros.Keys.Contains(macro);
    // }

    private void AddToken (Resolver slvExp, IEnumerable<Token> tks)
    {
      IEnumerator<Token> tkEnum = tks.GetEnumerator();
      tkEnum.Reset();
      for (; ; ) {
        if (!tkEnum.MoveNext())
          return;
        Token tk = tkEnum.Current;


        switch ((TokenType)tk.Type) {
          case TokenType.OperatorNot:
            slvExp.Push(tk, Operator.Not);
            break;

          case TokenType.OperatorAdd:
            slvExp.Push(tk, Operator.Add);
            break;

          case TokenType.OperatorSub:
            slvExp.Push(tk, Operator.Sub);
            break;

          case TokenType.OperatorEqual:
            slvExp.Push(tk, Operator.Equals);
            break;

          case TokenType.OperatorNotEqual:
            slvExp.Push(tk, Operator.NotEquals);
            break;

          case TokenType.OperatorLogicAnd:
            slvExp.Push(tk, Operator.And);
            break;

          case TokenType.OperatorLogicOr:
            slvExp.Push(tk, Operator.Or);
            break;

          case TokenType.HexadecimalInteger: // 54
          case TokenType.OctalInteger: // 57
          case TokenType.DecimalInteger: // 60
            slvExp.Push(tk, int.Parse(tk.Litteral));
            break;

          case TokenType.HexadecimalLong:
          case TokenType.OctalLong:
          case TokenType.DecimalLong:
            string value = tk.Litteral;
            // TODO Handle integer types better
            value = value.Replace('L', ' ');
            value = value.Replace('l', ' ');
            value = value.Replace('U', ' ');
            value = value.Replace('u', ' ');
            slvExp.Push(tk, int.Parse(value));
            break;

          case TokenType.Identifier:
            if (tk.Litteral == "defined") {
              bool isDef;
              Token s;
              if (!tkEnum.MoveNext())
                throw new Exception();
              s = tkEnum.Current;
              if (s.Type == (int)TokenType.OpenParenthesis) {
                if (!tkEnum.MoveNext())
                  throw new Exception();
                s = tkEnum.Current;
                if (s.Type != (int)TokenType.Identifier)
                  throw new Exception();
                isDef = mssesion.Defined(s.Litteral);

                if (!tkEnum.MoveNext())
                  throw new Exception();
                s = tkEnum.Current;
                if (s.Type != (int)TokenType.CloseParenthesis)
                  throw new Exception();
              } else if (s.Type == (int)TokenType.Identifier) {
                isDef = mssesion.Defined(s.Litteral);
              } else
                throw new Exception();

              slvExp.Push(tk, isDef);
            } else {
              Macro md = mssesion.FindMacro(tk.Litteral);
              if (md != null)
                AddToken(slvExp, md.Value);
              else
                slvExp.Push(tk, Primitive.Error, false);
            }
            break;
          case TokenType.OpenParenthesis:
            slvExp.OpenParenthese(tk);
            break;
          case TokenType.CloseParenthesis:
            slvExp.CloseParenthese(tk);
            break;
          case TokenType.OperatorMoreEq:
            slvExp.Push(tk, Operator.MoreEq);
            break;
          case TokenType.OperatorMore:
            slvExp.Push(tk, Operator.More);
            break;
          case TokenType.OperatorLessEq:
            slvExp.Push(tk, Operator.LessEq);
            break;
          case TokenType.OperatorLess:
            slvExp.Push(tk, Operator.Less);
            break;
          default:
            break;
        }
      }
    }

    private bool PreProcCondition (Token token, string value)
    {
      Resolver slvExp = new Resolver();
      List<Token> tks = Tokenizer.AnalyzeString(Language.CPreProcIf(), value);

      AddToken(slvExp, tks);
      // Console.WriteLine("  PREPROC - IF {0}, {1}", (TokenType)tk.Type, tk.Litteral);

      slvExp.Compile();
      ErrorReport.PreProcessor2("# If '" + value + "' " + slvExp.AsBoolean + GetTokenPosition(token));
      return slvExp.AsBoolean;
    }

    private void PrePorcIf (Token token, string value)
    {
      if (IgnoreBlock) {
        blockStack.Push(new CprBlock(BlockLevel + 1, false, true, token));
        return;
      }

      if (PreProcCondition(token, value)) {
        blockStack.Push(new CprBlock(BlockLevel + 1, true, true, token));
      } else {
        blockStack.Push(new CprBlock(BlockLevel + 1, false, false, token));
      }
    }

    private void PrePorcElsif (Token token, string value)
    {
      if (blockStack.Peek().getBlock) {
        blockStack.Peek().getBlock = false;
        ErrorReport.PreProcessor2("# Elsif start to ignore" + GetTokenPosition(token));
      } else if (!blockStack.Peek().alreadyGet) {
        if (PreProcCondition(token, value)) {
          blockStack.Peek().getBlock = true;
          blockStack.Peek().alreadyGet = true;
          ErrorReport.PreProcessor2("# Elsif stop to ignore" + GetTokenPosition(token));
        }
      }
    }

    private void PrePorcEndif (Token token)
    {
      bool ign = IgnoreBlock;
      blockStack.Pop();

      if (ign && !IgnoreBlock) {
        ErrorReport.PreProcessor2("# End ignore " + GetTokenPosition(token));
      }
    }

    private void PrePorcElse (Token token)
    {
      if (blockStack.Peek().getBlock) {
        blockStack.Peek().getBlock = false;
        ErrorReport.PreProcessor2("# Else start to ignore" + GetTokenPosition(token));
      } else if (!blockStack.Peek().alreadyGet) {
        blockStack.Peek().getBlock = true;
        blockStack.Peek().alreadyGet = true;
        ErrorReport.PreProcessor2("# Else stop to ignore" + GetTokenPosition(token));
      }
      // else  ASSERTION NEEDED throw new Exception();
    }

    // Dictionary<string, List<Token>> macros = new Dictionary<string, List<Token>>();
    // Queue<Token> tokensList = new Queue<Token>();

    Token unget = null;
    public void UnToken (Token token)
    {
      unget = token;
    }

    MacroSession mssesion = new MacroSession();

    /*
    bool MacroActif_ = false;
    public void ActiveMacro(List<Token> tokens)
    {
        if (!MacroActif_)
        {
            MacroActif_ = true;
            foreach (Token stk in tokens)
            {
                // Change token positions
                tokensList.Enqueue(stk);
            }
        }
        else
        {
        }
    }

    public Token ReadMacroToken ()
    {
        Token tk = null;
        if (tokensList.Count > 0)
        {
            tk = tokensList.Dequeue();

            if (tk.Type == 1 && IsDefined(tk.Litteral))
            {
                List<Token> tokens;
                if (macros.TryGetValue(tk.Litteral, out tokens) && tokens != null)
                {
                    this.ActiveMacro(tokens);
                    return ReadMacroToken();
                }
            }
        }
        else
        {
            MacroActif_ = false;
        }

        return tk;
    }
     */


    public Token ReadToken ()
    {
      Token tk = null;
      if (unget != null) {
        tk = unget;
        unget = null;
        return tk;
      }

      for (; ; ) {
        if (mssesion.Actif) {
          tk = mssesion.ReadToken();
          if (tk != null)
            return tk;
        }

        if (fileReaderStack.Count == 0)
          return null;
        tk = fileReaderStack.Peek().ReadToken();

        if (tk == null) {
          ErrorReport.PreProcessor("# EOF");
          fileReaderStack.Pop();
          fileNameStack.Pop();

          if (blockStack.Count != 1)  // FILE START
                    {
            // Console.Error.WriteLine("[Warn] Preprocessor condition is not closed at the end of file");
          }

          if (fileReaderStack.Count == 0)
            return null;

          ErrorReport.PreProcessor("# return to file: " + fileNameStack.Peek());

          continue;
        }

        tk.File = fileNameStack.Peek();
        if (tk.Type == (int)TokenType.PreProcessor) {
          string value = tk.Litteral.Substring(1).Trim();
          value = value.Replace("\\\n", "");

          if (value.StartsWith("ifdef")) {
            PrePorcIf(tk, "defined (" + value.Substring(5).Trim() + ")");
          } else if (value.StartsWith("ifndef")) {
            PrePorcIf(tk, "!defined (" + value.Substring(6).Trim() + ")");
          } else if (value.StartsWith("if")) {
            PrePorcIf(tk, value.Substring(2).Trim());
          } else if (value.StartsWith("endif")) {
            PrePorcEndif(tk);
          } else if (value.StartsWith("elif")) {
            PrePorcElsif(tk, value.Substring(4).Trim());
          } else if (value.StartsWith("else")) {
            PrePorcElse(tk);
          } else if (!IgnoreBlock) {

            if (value.StartsWith("include")) {
              Include(tk, value.Substring(7).Trim());
            } else if (value.StartsWith("define")) {
              Define(tk, value.Substring(6).Trim());
            } else if (value.StartsWith("undef")) {
              Undef(tk, value.Substring(5).Trim());
            } else if (value.StartsWith("error")) {
              ErrorReport.Error("#error" + GetTokenPosition(tk) + " : " + value.Substring(5).Trim());
            } else if (value.StartsWith("warning")) {
              ErrorReport.Error("#warning" + GetTokenPosition(tk) + " : " + value.Substring(5).Trim());
            } else {
              ErrorReport.ToImplement("[Err] Preprocessor unknowed");
            }
          }
        } else if (tk.Type == (int)TokenType.Comments) {
        } else if (tk.Type == (int)TokenType.DocComments) {
          // TODO Push on DocComments motor
        } else if (!IgnoreBlock) {
          if (tk.Type == 1 && mssesion.Defined(tk.Litteral)) {
            Macro md = mssesion.FindMacro(tk.Litteral);
            int args = mssesion.SetUpMacro(md, tk);
            if (args > 0) {
              List<List<Token>> tkArgs = new List<List<Token>>();
              List<Token> tkMacro = new List<Token>();
              Token tm = fileReaderStack.Peek().ReadToken();
              if (tm.Litteral != "(")
                ErrorReport.Error("Expect '('");
              for (; ; ) {
                tm = fileReaderStack.Peek().ReadToken();
                if (tm.Litteral == ",") {
                  tkArgs.Add(tkMacro);
                  tkMacro = new List<Token>();
                } else if (tm.Litteral == ")") {
                  tkArgs.Add(tkMacro);
                  break;
                } else {
                  tkMacro.Add(tm);
                }
              }

              mssesion.SetArguments(tkArgs);
            } else
              mssesion.SetArguments(null);

            continue;
          }

          return tk;
        }
      }
    }
  }

}
