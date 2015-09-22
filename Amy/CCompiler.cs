using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amy.Lexer;

namespace AmySuite
{
  [Flags]
  enum CType
  {
    Typedef = 1 << 1,
    Extern = 1 << 2,
    Static = 1 << 3,
    Auto = 1 << 4,
    Register = 1 << 5,
    ThreadLocal = 1 << 6,

    StorageMask = CType.Typedef | CType.Extern | CType.Static | CType.Auto | CType.Register | CType.ThreadLocal,

    Const = 1 << 7,
    Restrict = 1 << 8,
    Volatile = 1 << 9,
    Atomic = 1 << 10,
    Inline = 1 << 11,
    NoReturn = 1 << 12,
    Array = 1 << 13,

    Void = 1 << 16,
    Char = 1 << 17,
    Short = 1 << 18,
    Int = 1 << 19,
    Long = 1 << 20,
    Float = 1 << 21,
    Double = 1 << 22,
    Signed = 1 << 23,
    Unsigned = 1 << 24,
    Bool = 1 << 25,
    Complex = 1 << 26,
    Long2 = 1 << 27,

    PrimitiveMask = 0xffff << 16,

  }

  class AmyDeclarator
  {
    public AmyDeclarator (AmyType type)
    {
      Type = type;
      Alias = string.Empty;
    }
    public String Alias;
    public override string ToString ()
    {
      string str = string.Empty;
      foreach (CType qual in pointers_)
        if ((int)qual != 0)
          str += " *" + qual;
        else
          str += " *";
      return Type.Name + str + " - " + Alias;
    }
    public AmyType Type;
    List<CType> pointers_ = new List<CType>();
    public void PushPointer (CType qualifiers)
    {
      pointers_.Add(qualifiers);
    }
  }
  class AmyType
  {
    public AmyType (CType ctype)
    {
      Storage = ctype & CType.StorageMask;
      Primitive = ctype & CType.PrimitiveMask;
      build_ = ctype;
      Name = "_";
      switch (Primitive) {
        case CType.Void:
          Name = "void";
          break;
        case CType.Char:
        case CType.Char | CType.Unsigned:
          Name = "unsigned 8";
          break;
        case CType.Char | CType.Signed:
          Name = "signed 8";
          break;
        case CType.Short:
        case CType.Short | CType.Signed:
        case CType.Short | CType.Int:
        case CType.Short | CType.Int | CType.Signed:
          Name = "signed 16";
          break;
        case CType.Short | CType.Unsigned:
        case CType.Short | CType.Int | CType.Unsigned:
          Name = "unsigned 16";
          break;
        case CType.Long:
        case CType.Long | CType.Signed:
        case CType.Long | CType.Int:
        case CType.Long | CType.Int | CType.Signed:
        case CType.Int:
        case CType.Int | CType.Signed:
          Name = "signed 32";
          break;
        case CType.Long | CType.Unsigned:
        case CType.Long | CType.Int | CType.Unsigned:
        case CType.Int | CType.Unsigned:
          Name = "unsigned 32";
          break;
        case CType.Long | CType.Long2:
        case CType.Long | CType.Long2 | CType.Signed:
        case CType.Long | CType.Long2 | CType.Int:
        case CType.Long | CType.Long2 | CType.Int | CType.Signed:
          Name = "signed 64";
          break;
        case CType.Long | CType.Long2 | CType.Unsigned:
        case CType.Long | CType.Long2 | CType.Int | CType.Unsigned:
          Name = "unsigned 64";
          break;

        case CType.Float:
          Name = "float";
          break;
        case CType.Double:
          Name = "double";
          break;
        case CType.Double | CType.Long:
          Name = "decimal";
          break;

        default:
          Name = "_";
          break;
      }



    }
    public AmyType (CType ctype, string name)
    {
      Storage = ctype & CType.StorageMask;
      Primitive = ctype & CType.PrimitiveMask;
      build_ = ctype;
      Name = name;
    }
    public AmyType Refered (AmyDeclarator decl)
    {
      AmyType at = new AmyType(this.Storage, decl.Alias);
      // FIXME
      return at;
    }
    public String Name;
    CType build_;
    public CType Storage;
    public CType Primitive;

    public override string ToString ()
    {
      if (Primitive == 0)
        return Storage + " " + Name;
      return Storage + " " + Primitive;
    }
  }

  class CCompiler
  {
    #region Settings

    public bool DoCompile; // c
    public bool DoAssemble; // S
    public bool DoPreProcess; // E
    public bool DoParse; // P
    public bool DoDependancies; // P

    public string Output;
    public bool Statistics;

    public readonly List<string> Macros = new List<string>();
    public readonly List<string> Warning = new List<string>();
    public readonly List<string> IncludeDirs = new List<string>();
    public readonly List<string> LibrariesDirs = new List<string>();

    public void Define (string macro)
    {
      Macros.Add(macro);
      // Console.WriteLine("DEFINE MACRO :: " + macro);
    }
    public void Undefine (string macro)
    {
      Console.WriteLine("UNDEFINE MACRO :: " + macro);
    }

    #endregion Settings



    CPreProcessor input;


    public void Compile (string path)
    {
      input = new CPreProcessor(path);
      foreach (string idir in IncludeDirs)
        input.AddIncludeDir(idir);

      foreach (string macro in Macros)
        input.Define(null, macro.Replace('=', ' '));

      if (this.DoCompile)
        Read();
      else if (this.DoPreProcess) {
        Token token;
        for (; ; ) {
          token = input.ReadToken();
          if (token == null)
            break;
          Console.Write(token.Litteral + " ");
        }

      } else
        throw new Exception();
    }

    static int k = 0;
    static string s = "KrepqAwv94yoilBZ1N3";
    public static string Anonyme ()
    {
      ++k;
      return "_" + s[(k) % s.Length] + s[(k + 9) % s.Length] + s[(k + 17) % s.Length] + s[(k + 1) % s.Length];
    }

    public void Read ()
    {
      _Source();
    }

    private void _Source ()
    {
      Token token;
      for (; ; ) {
        token = input.ReadToken();
        input.UnToken(token);
        AmyType type = _TypeDeclaration();
        if (type == null)
          Console.WriteLine();
        AmyDeclarator decl = _Declarator(type);
        if (decl == null)
          Console.WriteLine();
        // FIXME We may have declaration for function like this:  
        //   int main(argc, arv) int argc, char**argv {}
        token = input.ReadToken();
        bool another = true;
        while (another) {
          another = false;
          switch ((TokenType)token.Type) {
            case TokenType.SemiColon:
              // What to do with this decl
              Console.WriteLine("Compile :: " + decl);
              break;

            case TokenType.OpenBraclet:
              // We enter on a function !! Check the type, push scope read statement
              break;

            case TokenType.OperatorAssign:
              // _Initializer   Carrefull to storage-class
              token = input.ReadToken();
              if (token.Type == (int)TokenType.Comma) {
                _Declarator(type);
                another = true;
              } else if (token.Type != (int)TokenType.SemiColon) {
                // ERROR
              }
              break;
          }
        }
      }
    }

    private void _Pointer (AmyDeclarator declarator)
    {
      CType qualifier = 0;
      bool ok = false;
      while (!ok) {
        Token token = input.ReadToken();
        switch ((TokenType)token.Type) {
          case TokenType.KeywordConst:
            qualifier |= CType.Const;
            break;
          case TokenType.KeywordRestrict:
            qualifier |= CType.Restrict;
            break;
          case TokenType.KeywordVolatile:
            qualifier |= CType.Volatile;
            break;
          //case TokenType.KeywordAtomic:
          //  qualifier |= CType.Atomic;
          //break;
          default:
            input.UnToken(token);
            ok = true;
            break;
        }
      }
      declarator.PushPointer(qualifier);
    }

    private void _DeclaratorDeco (AmyDeclarator declarator)
    {
      for (; ; ) {
        Token token = input.ReadToken();
        if (token.Type == (int)TokenType.OpenParenthesis) {
          for (; ; ) {
            // FIXME no storage
            AmyType type = _TypeDeclaration();
            if (type == null) {
              token = input.ReadToken();
              if (token.Type == (int)TokenType.Operator3Dot) {
                Console.WriteLine("Add unbounded param to " + declarator);
                token = input.ReadToken();
                if (token.Type != (int)TokenType.CloseParenthesis)
                  ErrorReport.Error("Unexpected");
                return;
              } else if (token.Type == (int)TokenType.CloseParenthesis)
                return;
              else {
                ErrorReport.Error("Unexpected");
              }
            }
            AmyDeclarator param = _Declarator(type);
            // param.Alias = token.Litteral;
            Console.WriteLine("Add param " + param + " to " + declarator);

            token = input.ReadToken();
            if (token.Type == (int)TokenType.CloseParenthesis)
              return;
            if (token.Type != (int)TokenType.Comma) {
              ErrorReport.Error("Unexpected");
              return;
            }
          }
        } else if (token.Type == (int)TokenType.OpenBraclet) {
          token = input.ReadToken();
          if (token.Type == (int)TokenType.CloseBraclet) {
            declarator.PushPointer(CType.Array);
          } else {
            while (token.Type != (int)TokenType.CloseBraclet)
              token = input.ReadToken();
            declarator.PushPointer(CType.Array);
            // FIXME read Array expression
          }
        } else {
          input.UnToken(token);
          return;
        }
      }
    }

    private AmyDeclarator _Declarator (AmyType type)
    {
      AmyDeclarator declarator = new AmyDeclarator(type);
      for (; ; ) {
        Token token = input.ReadToken();
        switch ((TokenType)token.Type) {
          case TokenType.Star:
            _Pointer(declarator);
            break;

          case TokenType.Identifier:
            declarator.Alias = token.Litteral;
            // Console.WriteLine("Declare " + declarator);
            _DeclaratorDeco(declarator);
            return declarator;

          case TokenType.OpenParenthesis:
            // FIXME What if there is no IDENTIFIER !?
            // FIXME avoid recursivity
            AmyDeclarator refer = _Declarator(type);
            token = input.ReadToken();
            if (token.Type != (int)TokenType.CloseParenthesis)
              ErrorReport.Error("Unexpected token");

            _DeclaratorDeco(refer);

            if (false) {
              // If type is unchanged
              // refer.PointerCount == 0 && refer,Paramters == 0
              return refer;
            } else {
              declarator.Alias = refer.Alias;
              refer.Alias = Anonyme();
              // CreateTypeWith refer (FunctionPointer!!)
              declarator.Type = declarator.Type.Refered(refer);
              return declarator;
            }

          case TokenType.OpenBraclet:
            input.UnToken(token);
            _DeclaratorDeco(declarator);
            return declarator;

          default:
            input.UnToken(token);
            return declarator;
        }
      }
    }

    private AmyType _Struct (CType storage)
    {
      string name = null;
      Token token = input.ReadToken();
      if (token.Type == (int)TokenType.Identifier) {
        name = "struct " + token.Litteral;
        token = input.ReadToken();
      }

      if (name == null)
        name = Anonyme();
      AmyType structure = new AmyType(storage, name);
      if (token.Type != (int)TokenType.OpenBraclet) {
        input.UnToken(token);
        return structure;
      }

      // Push structure scope
      //Console.WriteLine("_ PUSH Struct scope" + structure);
      for (; ; ) {
        token = input.ReadToken();
        input.UnToken(token);
        AmyType f = _TypeDeclaration(); // No storage/function spec allowed, STATIC_ASSERT is allowed^^
        if (f == null) {
          token = input.ReadToken();
          if (token.Type != (int)TokenType.CloseBraclet) {
            ErrorReport.Error("Unexepcted token");
          }

          break;
        }
        AmyDeclarator field = _Declarator(f);
        token = input.ReadToken();
        //if (token.Type == (int)TokenType.DoubleColon)
        // : <cst> (,)

        Console.WriteLine("Add param " + field + " to " + structure);

        if (token.Type == (int)TokenType.SemiColon)
          continue;

        ErrorReport.Error("Unexpected token");
        return structure;
      }
      //Console.WriteLine("_ POP Struct scope" + structure);
      // Pop structure scopde

      return structure;
    }

    private AmyType _Enum (CType storage)
    {
      string name = null;
      Token token = input.ReadToken();
      if (token.Type == (int)TokenType.Identifier) {
        name = "enum " + token.Litteral;
        token = input.ReadToken();
      }

      if (name == null)
        name = Anonyme();
      AmyType enumeration = new AmyType(storage, name);
      if (token.Type != (int)TokenType.OpenBraclet) {
        input.UnToken(token);
        return enumeration;
      }

      for (; ; ) {
        token = input.ReadToken();
        if (token.Type == (int)TokenType.Comma)
          continue;

        if (token.Type == (int)TokenType.Identifier) {
          token = input.ReadToken();
          if (token.Type == (int)TokenType.OperatorAssign) {
          }
          // New Value
        }

        if (token.Type == (int)TokenType.CloseBraclet)
          break;
      }
      return enumeration;
    }


    private AmyType _TypeDeclaration ()
    {
      CType type = 0;

      for (; ; ) {
        Token token = input.ReadToken();
        switch ((TokenType)token.Type) {
          case TokenType.KeywordTypeDef:
            if ((type & CType.StorageMask) != 0)
              ErrorReport.Error("Only a single storage class is allowed");
            type |= CType.Typedef;
            break;

          case TokenType.KeywordExtern:
            if ((type & CType.StorageMask) != 0)
              ErrorReport.Error("Only a single storage class is allowed");
            type |= CType.Extern;
            break;

          case TokenType.KeywordStatic:
            if ((type & CType.StorageMask) != 0)
              ErrorReport.Error("Only a single storage class is allowed");
            type |= CType.Static;
            break;

          case TokenType.KeywordAuto:
            if ((type & CType.StorageMask) != 0)
              ErrorReport.Error("Only a single storage class is allowed");
            type |= CType.Auto;
            break;

          case TokenType.KeywordRegister:
            if ((type & CType.StorageMask) != 0)
              ErrorReport.Error("Only a single storage class is allowed");
            type |= CType.Register;
            break;

          case TokenType.KeywordConst:
            type |= CType.Const;
            break;

          case TokenType.KeywordRestrict:
            type |= CType.Restrict;
            break;

          case TokenType.KeywordVolatile:
            type |= CType.Volatile;
            break;

          case TokenType.TypeStruct:
          case TokenType.TypeUnion:
            if ((type & CType.PrimitiveMask) != 0)
              ErrorReport.Error("Incompatible type specificator");
            return _Struct(type);

          case TokenType.TypeEnum:
            if ((type & CType.PrimitiveMask) != 0)
              ErrorReport.Error("Incompatible type specificator");
            return _Enum(type);

          case TokenType.TypeVoid:
            type |= CType.Void;
            break;
          case TokenType.TypeSigned:
            type |= CType.Signed;
            break;

          case TokenType.TypeUnsigned:
            type |= CType.Unsigned;
            break;

          case TokenType.TypeChar:
            type |= CType.Char;
            break;

          case TokenType.TypeShort:
            type |= CType.Short;
            break;

          case TokenType.TypeLong:
            if ((type & CType.Long) != 0)
              type |= CType.Long2;
            else
              type |= CType.Long;
            break;

          case TokenType.TypeInt:
            type |= CType.Int;
            break;

          case TokenType.TypeFloat:
            type |= CType.Float;
            break;

          case TokenType.TypeDouble:
            type |= CType.Double;
            break;

          case TokenType.Identifier:

          default:
            if ((type & CType.PrimitiveMask) != 0) {
              input.UnToken(token);
              return new AmyType(type);
            } else if (token.Type == (int)TokenType.Identifier) {
              return new AmyType(type, token.Litteral);
            }
            input.UnToken(token);
            return null;
          // ErrorReport.Error("Unexpect token");
        }
      }
    }
  }





}
