using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amy.Lexer
{
  enum TokenType
  {
    Unknown,
    Identifier,

    DocComments,
    Comments,
    PreProcessor,
    StringLitteral,
    CharacterLitteral,

    OpenParenthesis,
    CloseParenthesis,
    OpenBraclet,
    CloseBraclet,

    Operator,

    OperatorMul,
    OperatorAdd,
    OperatorSub,
    OperatorTile,
    OperatorNot,
    OperatorDiv,
    OperatorMod,
    OperatorSll,
    OperatorSlr,
    OperatorLess,
    OperatorMore,
    OperatorLessEq,
    OperatorMoreEq,
    OperatorEqual,
    OperatorNotEqual,
    OperatorLogicAnd,
    OperatorLogicOr,


    Dot,
    Arrow,
    Star,
    Plus,
    Minus,
    Tilde,
    Slash,

    Colon,
    SemiColon,
    Operator3Dot,
    OperatorAssign,

    Comma,

    KeywordAlignOf,
    KeywordAuto,
    KeywordBreak,
    KeywordCase,
    KeywordConst,
    KeywordContinue,
    KeywordDefault,
    KeywordDo,
    KeywordElse,
    KeywordExtern,
    KeywordFor,
    KeywordGoto,
    KeywordIf,
    KeywordInline,
    KeywordRegister,
    KeywordRestrict,
    KeywordReturn,
    KeywordSizeOf,
    KeywordStatic,
    KeywordSwitch,
    KeywordTypeDef,
    KeywordVolatile,
    KeywordWhile,

    KeywordDefined,

    TypeStruct,
    TypeUnion,
    TypeEnum,
    TypeVoid,
    TypeSigned,
    TypeUnsigned,
    TypeChar,
    TypeShort,
    TypeLong,
    TypeInt,
    TypeFloat,
    TypeDouble,
    TypeBool,
    TypeComplex,

    HexadecimalInteger,
    HexadecimalLong,
    HexadecimalLongLong,
    OctalInteger,
    OctalLong,
    OctalLongLong,
    DecimalInteger,
    DecimalLong,
    DecimalLongLong,

  }

  class TokenDelimiter
  {
    public string Start { get; set; }
    public string End { get; set; }
    public string Escape { get; set; }
    public int Type { get; set; }
    public bool LeaveIt { get; set; }
    public int GetLength (string data)
    {
      int k = Start.Length;
      for (; ; ) {
        int lg = data.IndexOf(End, k);
        if (lg < 0)
          return data.Length;
        if (!LeaveIt)
          lg += End.Length;
        string value = data.Substring(0, lg);
        if (Escape == null || !value.EndsWith(Escape + End))
          return lg;
        k = lg;
        if (LeaveIt)
          k += End.Length;
      }
    }
  }

  class TokenKeyword
  {
    public string Litteral { get; set; }
    public int Type { get; set; }
  }

  class TokenExpr
  {
    public Regex Expression { get; set; }
    public int Type { get; set; }
  }

  class Language
  {
    public bool isOrdered { get; private set; }
    public List<TokenDelimiter> Delimiters { get; private set; }
    public List<TokenKeyword> Operators { get; private set; }
    public List<TokenKeyword> Keywords { get; private set; }
    public List<TokenExpr> Groups { get; private set; }

    private Language ()
    {
      isOrdered = false;
      Delimiters = new List<TokenDelimiter>();
      Operators = new List<TokenKeyword>();
      Keywords = new List<TokenKeyword>();
      Groups = new List<TokenExpr>();
    }

    public static Language CPreProcIf ()
    {
      Language lang = new Language();

      lang.Operators.Add(new TokenKeyword() { Litteral = "(", Type = (int)TokenType.OpenParenthesis });
      lang.Operators.Add(new TokenKeyword() { Litteral = ")", Type = (int)TokenType.CloseParenthesis });
      lang.Operators.Add(new TokenKeyword() { Litteral = "*", Type = (int)TokenType.OperatorMul });
      lang.Operators.Add(new TokenKeyword() { Litteral = "+", Type = (int)TokenType.OperatorAdd });
      lang.Operators.Add(new TokenKeyword() { Litteral = "-", Type = (int)TokenType.OperatorSub });
      lang.Operators.Add(new TokenKeyword() { Litteral = "~", Type = (int)TokenType.OperatorTile });
      lang.Operators.Add(new TokenKeyword() { Litteral = "!", Type = (int)TokenType.OperatorNot });
      lang.Operators.Add(new TokenKeyword() { Litteral = "/", Type = (int)TokenType.OperatorDiv });
      lang.Operators.Add(new TokenKeyword() { Litteral = "%", Type = (int)TokenType.OperatorMod });
      lang.Operators.Add(new TokenKeyword() { Litteral = "<<", Type = (int)TokenType.OperatorSll });
      lang.Operators.Add(new TokenKeyword() { Litteral = ">>", Type = (int)TokenType.OperatorSlr });
      lang.Operators.Add(new TokenKeyword() { Litteral = "<", Type = (int)TokenType.OperatorLess });
      lang.Operators.Add(new TokenKeyword() { Litteral = ">", Type = (int)TokenType.OperatorMore });
      lang.Operators.Add(new TokenKeyword() { Litteral = "<=", Type = (int)TokenType.OperatorLessEq });
      lang.Operators.Add(new TokenKeyword() { Litteral = ">=", Type = (int)TokenType.OperatorMoreEq });
      lang.Operators.Add(new TokenKeyword() { Litteral = "==", Type = (int)TokenType.OperatorEqual });
      lang.Operators.Add(new TokenKeyword() { Litteral = "!=", Type = (int)TokenType.OperatorNotEqual });
      lang.Operators.Add(new TokenKeyword() { Litteral = "&&", Type = (int)TokenType.OperatorLogicAnd });
      lang.Operators.Add(new TokenKeyword() { Litteral = "||", Type = (int)TokenType.OperatorLogicOr });

      // lang.Keywords.Add(new TokenKeyword() { Litteral = "defined", Type = (int)TokenType.KeywordDefined });
      // lang.Keywords.Add(new TokenKeyword() { Litteral = "alignof", Type = (int)TokenType.KeywordAlignOf });
      // lang.Keywords.Add(new TokenKeyword() { Litteral = "sizeof", Type = (int)TokenType.KeywordSizeOf });

      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0x[0-9A-Fa-f]+[Uu]?"), Type = (int)TokenType.HexadecimalInteger });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0x[0-9A-Fa-f]+[Uu]?[Ll]?"), Type = (int)TokenType.HexadecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0x[0-9A-Fa-f]+[Ll]?[Uu]?"), Type = (int)TokenType.HexadecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0x[0-9A-Fa-f]*[Uu]?[Ll]{2}"), Type = (int)TokenType.HexadecimalLongLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0x[0-9A-Fa-f]*[Ll]{2}[Uu]?"), Type = (int)TokenType.HexadecimalLongLong });

      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0[0-7]*[Uu]?[Ll]{2}"), Type = (int)TokenType.OctalInteger });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0[0-7]*[Ll]{2}[Uu]?"), Type = (int)TokenType.OctalInteger });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0[0-7]*[Uu]?[Ll]"), Type = (int)TokenType.OctalInteger });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0[0-7]*[Ll][Uu]?"), Type = (int)TokenType.OctalInteger });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0[0-7]*[Uu]?"), Type = (int)TokenType.OctalInteger });

      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[0-9]+[Uu]?[Ll]{2}"), Type = (int)TokenType.DecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[0-9]+[Ll]{2}[Uu]?"), Type = (int)TokenType.DecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[0-9]+[Uu]?[Ll]"), Type = (int)TokenType.DecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[0-9]+[Ll][Uu]?"), Type = (int)TokenType.DecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[0-9]+[Uu]?"), Type = (int)TokenType.DecimalInteger });

      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[_a-zA-Z][_0-9a-zA-Z]*"), Type = (int)TokenType.Identifier });

      return lang;

    }

    public static Language CLanguage ()
    {
      Language lang = new Language();
      lang.Delimiters.Add(new TokenDelimiter()
      {
        Start = "/**",
        End = "*/",
        Escape = null,
        Type = (int)TokenType.DocComments,
      });
      lang.Delimiters.Add(new TokenDelimiter()
      {
        Start = "///",
        End = "\n",
        Escape = null,
        Type = (int)TokenType.DocComments,
      });
      lang.Delimiters.Add(new TokenDelimiter()
      {
        Start = "/*",
        End = "*/",
        Escape = null,
        Type = (int)TokenType.Comments,
      });
      lang.Delimiters.Add(new TokenDelimiter()
      {
        Start = "//",
        End = "\n",
        Escape = null,
        Type = (int)TokenType.Comments,
      });
      lang.Delimiters.Add(new TokenDelimiter()
      {
        Start = "#",
        End = "/*",
        LeaveIt = true,
        Type = (int)TokenType.PreProcessor,
      });
      lang.Delimiters.Add(new TokenDelimiter()
      {
        Start = "#",
        End = "//",
        LeaveIt = true,
        Type = (int)TokenType.PreProcessor,
      });
      lang.Delimiters.Add(new TokenDelimiter()
      {
        Start = "#",
        End = "\n",
        Escape = "\\",
        Type = (int)TokenType.PreProcessor,
      });
      lang.Delimiters.Add(new TokenDelimiter()
      {
        Start = "\"",
        End = "\"",
        Escape = "\\",
        Type = (int)TokenType.StringLitteral,
      });
      lang.Delimiters.Add(new TokenDelimiter()
      {
        Start = "'",
        End = "'",
        Escape = "\\",
        Type = (int)TokenType.CharacterLitteral,
      });

      lang.Operators.Add(new TokenKeyword() { Litteral = "(", Type = (int)TokenType.OpenParenthesis });
      lang.Operators.Add(new TokenKeyword() { Litteral = ")", Type = (int)TokenType.CloseParenthesis });
      lang.Operators.Add(new TokenKeyword() { Litteral = "{", Type = (int)TokenType.OpenBraclet });
      lang.Operators.Add(new TokenKeyword() { Litteral = "}", Type = (int)TokenType.CloseBraclet });
      lang.Operators.Add(new TokenKeyword() { Litteral = "[", Type = (int)TokenType.OpenBraclet });
      lang.Operators.Add(new TokenKeyword() { Litteral = "]", Type = (int)TokenType.CloseBraclet });
      lang.Operators.Add(new TokenKeyword() { Litteral = ".", Type = (int)TokenType.Dot });
      lang.Operators.Add(new TokenKeyword() { Litteral = "->", Type = (int)TokenType.Arrow });
      lang.Operators.Add(new TokenKeyword() { Litteral = "++", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "--", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "&", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "*", Type = (int)TokenType.Star });
      lang.Operators.Add(new TokenKeyword() { Litteral = "+", Type = (int)TokenType.Plus });
      lang.Operators.Add(new TokenKeyword() { Litteral = "-", Type = (int)TokenType.Minus });
      lang.Operators.Add(new TokenKeyword() { Litteral = "~", Type = (int)TokenType.Tilde });
      lang.Operators.Add(new TokenKeyword() { Litteral = "!", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "/", Type = (int)TokenType.Slash });
      lang.Operators.Add(new TokenKeyword() { Litteral = "%", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "<<", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = ">>", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "<", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = ">", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "<=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = ">=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "==", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "!=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "^", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "|", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "&&", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "||", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "?", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = ":", Type = (int)TokenType.Colon });
      lang.Operators.Add(new TokenKeyword() { Litteral = ";", Type = (int)TokenType.SemiColon });
      lang.Operators.Add(new TokenKeyword() { Litteral = "...", Type = (int)TokenType.Operator3Dot });
      lang.Operators.Add(new TokenKeyword() { Litteral = "=", Type = (int)TokenType.OperatorAssign });
      lang.Operators.Add(new TokenKeyword() { Litteral = "*=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "/=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "%=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "+=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "-=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "<<=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = ">>=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "&=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "^=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "|=", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = ",", Type = (int)TokenType.Comma });
      lang.Operators.Add(new TokenKeyword() { Litteral = "#", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "##", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "<:", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = ":>", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "<%", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "%>", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "%:", Type = (int)TokenType.Operator });
      lang.Operators.Add(new TokenKeyword() { Litteral = "%:%:", Type = (int)TokenType.Operator });

      // Storage class specifiers
      lang.Keywords.Add(new TokenKeyword() { Litteral = "typedef", Type = (int)TokenType.KeywordTypeDef });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "extern", Type = (int)TokenType.KeywordExtern });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "static", Type = (int)TokenType.KeywordStatic });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "auto", Type = (int)TokenType.KeywordAuto });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "register", Type = (int)TokenType.KeywordRegister });

      // Type qualifiers
      lang.Keywords.Add(new TokenKeyword() { Litteral = "const", Type = (int)TokenType.KeywordConst });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "restrict", Type = (int)TokenType.KeywordRestrict });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "volatile", Type = (int)TokenType.KeywordVolatile });

      lang.Keywords.Add(new TokenKeyword() { Litteral = "alignof", Type = (int)TokenType.KeywordAlignOf });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "break", Type = (int)TokenType.KeywordBreak });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "case", Type = (int)TokenType.KeywordCase });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "continue", Type = (int)TokenType.KeywordContinue });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "default", Type = (int)TokenType.KeywordDefault });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "do", Type = (int)TokenType.KeywordDo });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "else", Type = (int)TokenType.KeywordElse });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "for", Type = (int)TokenType.KeywordFor });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "goto", Type = (int)TokenType.KeywordGoto });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "if", Type = (int)TokenType.KeywordIf });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "inline", Type = (int)TokenType.KeywordInline });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "return", Type = (int)TokenType.KeywordReturn });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "sizeof", Type = (int)TokenType.KeywordSizeOf });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "switch", Type = (int)TokenType.KeywordSwitch });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "while", Type = (int)TokenType.KeywordWhile });

      lang.Keywords.Add(new TokenKeyword() { Litteral = "struct", Type = (int)TokenType.TypeStruct });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "union", Type = (int)TokenType.TypeUnion });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "enum", Type = (int)TokenType.TypeEnum });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "void", Type = (int)TokenType.TypeVoid });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "signed", Type = (int)TokenType.TypeSigned });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "unsigned", Type = (int)TokenType.TypeUnsigned });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "char", Type = (int)TokenType.TypeChar });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "short", Type = (int)TokenType.TypeShort });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "long", Type = (int)TokenType.TypeLong });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "int", Type = (int)TokenType.TypeInt });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "float", Type = (int)TokenType.TypeFloat });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "double", Type = (int)TokenType.TypeDouble });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "_Bool", Type = (int)TokenType.TypeBool });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "bool", Type = (int)TokenType.TypeBool });
      lang.Keywords.Add(new TokenKeyword() { Litteral = "_Complex", Type = (int)TokenType.TypeComplex });

      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0x[0-9A-Fa-f]+[Uu]?"), Type = (int)TokenType.HexadecimalInteger });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0x[0-9A-Fa-f]+[Uu]?[Ll]?"), Type = (int)TokenType.HexadecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0x[0-9A-Fa-f]+[Ll]?[Uu]?"), Type = (int)TokenType.HexadecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0x[0-9A-Fa-f]*[Uu]?[Ll]{2}"), Type = (int)TokenType.HexadecimalLongLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0x[0-9A-Fa-f]*[Ll]{2}[Uu]?"), Type = (int)TokenType.HexadecimalLongLong });

      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0[0-7]*[Uu]?[Ll]{2}"), Type = (int)TokenType.OctalInteger });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0[0-7]*[Ll]{2}[Uu]?"), Type = (int)TokenType.OctalInteger });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0[0-7]*[Uu]?[Ll]"), Type = (int)TokenType.OctalInteger });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0[0-7]*[Ll][Uu]?"), Type = (int)TokenType.OctalInteger });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("0[0-7]*[Uu]?"), Type = (int)TokenType.OctalInteger });

      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[0-9]+[Uu]?[Ll]{2}"), Type = (int)TokenType.DecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[0-9]+[Ll]{2}[Uu]?"), Type = (int)TokenType.DecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[0-9]+[Uu]?[Ll]"), Type = (int)TokenType.DecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[0-9]+[Ll][Uu]?"), Type = (int)TokenType.DecimalLong });
      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[0-9]+[Uu]?"), Type = (int)TokenType.DecimalInteger });

      lang.Groups.Add(new TokenExpr() { Expression = new Regex("[_a-zA-Z][_0-9a-zA-Z]*"), Type = (int)TokenType.Identifier });

      return lang;
    }
  }

  class Tokenizer
  {
    public static char[] WhiteChars = new char[] { ' ', '\r', '\n', '\t' };
    private TextReader reader;
    private int peekChar;
    private int lastChar;
    private string textData;
    private Position position;
    private Language lang;


    public static List<Token> AnalyzeString (Language language, string value)
    {
      List<Token> toks = new List<Token>();
      Tokenizer tkz = new Tokenizer(language);
      tkz.SetText(value);
      for (; ; ) {
        Token tk = tkz.ReadToken();
        if (tk == null)
          break;
        if (tk.Type == (int)TokenType.Comments ||
            tk.Type == (int)TokenType.DocComments)
          continue;
        toks.Add(tk);
      }
      return toks;
    }

    public Tokenizer (Language language)
    {
      lang = language;
      SetText(string.Empty);
    }

    public Tokenizer (string path, Language language)
    {
      using (StreamReader reader = new StreamReader(File.OpenRead(path))) {
        Init(reader);
        lang = language;
      }
    }

    public Tokenizer (TextReader reader, Language language)
    {
      Init(reader);
      lang = language;
    }

    private void Init (TextReader reader)
    {
      this.textData = reader.ReadToEnd();
      this.position.Row = 1;
      this.position.Column = 1;
      ConsumeCharacters(0);
    }

    public void SetText (string text)
    {
      textData = text;
      this.position.Row = 1;
      this.position.Column = 1;
      ConsumeCharacters(0);
    }


    /*
    protected int ReadChar()
    {
        int ch;
        if (lastChar == '\n')
        {
            cursorRow++;
            column = 1;
        }
        else if (lastChar > 0)
        {
            column++;
        }

        if (peekChar >= 0)
        {
            ch = peekChar;
            peekChar = 0;
            lastChar = peekChar;
            return peekChar;
        }

        ch = reader.Read();
        peekChar = -1;
        if (ch <= 0)
        {
            ch = 0;
        }

        if (ch == '\r')
        {
            ch = reader.Read();
        }

        lastChar = ch;
        return ch;
    }

    protected int PeekChar()
    {
        if (peekChar < 0)
        {
            peekChar = ReadChar();
        }

        return peekChar;
    }
    */


    public Token ReadToken ()
    {
      Token token = null;
      if (string.IsNullOrEmpty(textData))
        return token;

      // Read delimiters
      if (token == null) {
        TokenDelimiter tkDelSelected = null;
        foreach (TokenDelimiter tkdel in lang.Delimiters) {
          if (textData.StartsWith(tkdel.Start)) {
            if (lang.isOrdered) {
              tkDelSelected = tkdel;
              break;
            } else {
              if (tkDelSelected == null) {
                tkDelSelected = tkdel;

              } else if (tkDelSelected.Start.Length <= tkdel.Start.Length) {

                int idxEndP = tkDelSelected.GetLength(textData);
                int idxEndN = tkdel.GetLength(textData);

                if (idxEndP >= 0 && idxEndN >= 0 && idxEndN < idxEndP) {
                  tkDelSelected = tkdel;
                }
                /*
            else if (idxEndP >= 0 && idxEndN >= 0)
            {
                // tkDelSelected = tkdel;

            }
            else if (idxEndN > idxEndP)
            {
                tkDelSelected = tkdel;
            }*/
              }
            }
          }
        }

        if (tkDelSelected != null) {
          int idxEnd = tkDelSelected.GetLength(textData);
          string value = textData.Substring(0, idxEnd);
          token = new Token()
          {
            Start = position,
            Type = tkDelSelected.Type,
            Litteral = value
          };
        }
      }

      // Read operators
      if (token == null) {
        TokenKeyword tkOprSelected = null;
        foreach (TokenKeyword tkopr in lang.Operators) {
          if (textData.StartsWith(tkopr.Litteral)) {
            if (lang.isOrdered) {
              tkOprSelected = tkopr;
              break;
            } else {
              if (tkOprSelected == null ||
                  tkOprSelected.Litteral.Length < tkopr.Litteral.Length) {
                tkOprSelected = tkopr;
              }
            }
          }
        }

        if (tkOprSelected != null) {
          token = new Token()
          {
            Start = position,
            Type = tkOprSelected.Type,
            Litteral = tkOprSelected.Litteral
          };
        }
      }

      // Read field
      if (token == null) {
        TokenExpr tkExSelected = null;
        foreach (TokenExpr tkEx in lang.Groups) {
          Match m = tkEx.Expression.Match(textData);
          if (m.Index == 0 && m.Length != 0) {
            tkExSelected = tkEx;
            token = new Token()
            {
              Start = position,
              Type = tkEx.Type,
              Litteral = textData.Substring(0, m.Length)
            };
            break;
          }
        }
      }

      if (token == null) {
        string stray = textData.Substring(0, textData.IndexOfAny(Tokenizer.WhiteChars));
        throw new Exception("Lexical error, found stray '" + stray + "' at " + ":" + position.Row);
        // GCC throw new Exception("error: stray '?' in program");
      }

      token.End = ConsumeCharacters(token.Litteral.Length);

      // Keyword
      if (token.Type == 1) {
        foreach (TokenKeyword tkw in lang.Keywords)
          if (tkw.Litteral == token.Litteral) {
            token.Type = tkw.Type;
            break;
          }

      }
      return token;
    }

    public Position ConsumeCharacters (int length)
    {
      int i;
      Position end;

      for (i = 0; i < length; ++i) {
        if (textData[i] == '\r' && textData[i + 1] == '\n')
          i++;
        if (textData[i] == '\n') {
          this.position.Row++;
          this.position.Column = 1;
        } else
          this.position.Column++;
      }

      end = this.position;

      while (i < textData.Length && char.IsWhiteSpace(textData[i])) {
        if (textData[i] == '\r' && textData[i + 1] == '\n')
          i++;
        if (textData[i] == '\n') {
          this.position.Row++;
          this.position.Column = 1;
        } else
          this.position.Column++;
        i++;
      }

      textData = textData.Substring(i);
      return end;
    }
  }

}
