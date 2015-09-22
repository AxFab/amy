using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Configuration;

using Amy.Lexer;
using Amy.Syntax;

namespace AmySuite
{
  class ImageJob
  {
    public bool printHeader;
    public bool printDisassemble;

    public void Run (string file)
    {
      using (BinaryReader reader = new BinaryReader(File.OpenRead(file))) {
        AssemblyBox box = AssemblyImage.ReadPEFile(reader);
        Console.WriteLine("{0,-40}file-format pe-i386\n", file);
        if (printHeader) {
          DisplayHeader(box);
        }

        if (printDisassemble) {
          box.TextSection.ReadData(reader);
          Extract(box);
        }
      }
    }

    public void DisplayHeader (AssemblyBox box)
    {
      Console.WriteLine("Architecture {0}\nFlags: {1}\n", box.Machine, box.Flags);
      Console.WriteLine("Start address {0}\n", null);
      Console.WriteLine("Time/Date \t{0}\n", new DateTime());
      Console.WriteLine();
      Console.WriteLine("Sections:");
      Console.WriteLine(" Idx Name       Size      Virtual   Offset    RawSize");
      int idx = 0;
      foreach (AssemblySection section in box.Sections) {
        Console.WriteLine(" {0,3} {1,-10} {2}  {3}  {4}  {5}", idx, section.Alias,
            Hexa.ToString(section.Size).Substring(2), Hexa.ToString(section.Virtual).Substring(2),
            Hexa.ToString(section.FileOffset).Substring(2), Hexa.ToString(section.FileLength).Substring(2));
        Console.WriteLine("                {0}", section.Flags);
      }

      Console.WriteLine();
      Console.WriteLine("Symbols:\n  Not implemented");
      Console.WriteLine();
    }

    public void Extract (AssemblyBox box)
    {
      Intelx86 ix86 = new Intelx86(box.TextSection.Data);

      // TODO Support file type detection !
      Console.WriteLine("Disassembly of section .text:\n");

      long address = box.TextSection.Virtual;
      Console.WriteLine("{0} <...>:", Hexa.ToString(address, 8, false).Substring(2));
      for (; ; ) {
        x86Operator op = ix86.Next();
        if (op == null)
          break;

        string amyMn = "  " + Hexa.ToString(address).Substring(7) + ":\t";
        for (var i = 0; i < Math.Min(op.Length, 7); ++i)
          amyMn += Hexa.ToString(ix86.Buffer[ix86.Offset + i], 2, false, false) + " ";
        for (var i = Math.Min(op.Length, 7); i < 7; ++i)
          amyMn += "   ";
        amyMn += "\t";
        amyMn += op.ATNTWriting;

        Console.WriteLine(amyMn);

        address += op.Length;
        int lg = op.Length, k = 7;
        while (lg > 7) {
          amyMn = "     :\t";
          for (var i = k; i < Math.Min(op.Length, k + 7); ++i)
            amyMn += Hexa.ToString(ix86.Buffer[ix86.Offset + i], 2, false, false) + " ";
          Console.WriteLine(amyMn);
          lg -= 7;
          k += 7;
        }


      }

      Console.WriteLine();
    }
  }

  enum OptionGroup
  {
    Compile_Command,
    Unit_Test,
    General,
    C_Source,
  }

  class Program
  {
    static void ExtractPreProcessorFromFile (string path)
    {
      int line = 0;
      //try
      //{
      CPreProcessor fr = new CPreProcessor(path);
      while (true) {
        Token tk = fr.ReadToken();
        if (tk == null)
          break;
        TokenType type = (TokenType)tk.Type;

        if (tk.Start.Row != line) {
          Console.WriteLine();
          line = tk.Start.Row;
        }

        Console.Write(tk.Litteral + " ");
        // Console.WriteLine("- {0} '{1}'", type, t.Litteral);
      }

      //}
      //catch (Exception e)
      //{
      //    Console.ForegroundColor = ConsoleColor.Red;
      //    Console.Error.WriteLine(e);
      //    Console.ForegroundColor = ConsoleColor.Gray;
      //}
    }

    static void ExtractTokenFromFile (string path)
    {
      try {
        Tokenizer fr = new Tokenizer(path, Language.CLanguage());
        while (true) {
          Token t = fr.ReadToken();
          if (t == null)
            break;
          TokenType type = (TokenType)t.Type;
          Console.WriteLine("- {0} '{1}'", type, t.Litteral);
        }

      } catch (Exception e) {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(e);
        Console.ForegroundColor = ConsoleColor.Gray;
      }
    }

    static void AST ()
    {
      Dictionary<string, ASTType> types = new Dictionary<string, ASTType>();

      types.Add("char", ASTType.NewType(PrimitiveType.Integer, 1));
      types.Add("signed char", ASTType.NewType(PrimitiveType.Integer | PrimitiveType.Signed, 1));
      types.Add("unsigned char", ASTType.NewType(PrimitiveType.Integer | PrimitiveType.Unsigned, 1));
      types.Add("short", ASTType.NewType(PrimitiveType.Integer, 2));
      types.Add("signed short", ASTType.NewType(PrimitiveType.Integer | PrimitiveType.Signed, 2));
      types.Add("unsigned short", ASTType.NewType(PrimitiveType.Integer | PrimitiveType.Unsigned, 2));
      types.Add("int", ASTType.NewType(PrimitiveType.Integer, 4));
      types.Add("signed int", ASTType.NewType(PrimitiveType.Integer | PrimitiveType.Signed, 4));
      types.Add("unsigned int", ASTType.NewType(PrimitiveType.Integer | PrimitiveType.Unsigned, 4));
      types.Add("long", ASTType.NewType(PrimitiveType.Integer, 4));
      types.Add("signed long", ASTType.NewType(PrimitiveType.Integer | PrimitiveType.Signed, 4));
      types.Add("unsigned long", ASTType.NewType(PrimitiveType.Integer | PrimitiveType.Unsigned, 4));
      types.Add("long long", ASTType.NewType(PrimitiveType.Integer, 8));
      types.Add("signed long long", ASTType.NewType(PrimitiveType.Integer | PrimitiveType.Signed, 8));
      types.Add("unsigned long long", ASTType.NewType(PrimitiveType.Integer | PrimitiveType.Unsigned, 8));
      types.Add("float", ASTType.NewType(PrimitiveType.Float, 4));
      types.Add("double", ASTType.NewType(PrimitiveType.Float, 8));
      types.Add("long double", ASTType.NewType(PrimitiveType.Float, 16));
      types.Add("const char*", ASTType.NewType(ASTType.NewType(ASTType.NewType(PrimitiveType.Integer, 1), StorageQualifier.Const), StorageQualifier.Pointer));

      ASTType ty;
      ty = ASTType.NewType(PrimitiveType.Struct,
          new ASTOperand("quot", ASTType.NewType(PrimitiveType.Integer, 4), ASTOperandType.Structure),
          new ASTOperand("rem", ASTType.NewType(PrimitiveType.Integer, 4), ASTOperandType.Structure));
      types.Add("struct div", ty);
      foreach (KeyValuePair<string, ASTType> type in types) {
        Console.WriteLine("{0} --> {1}", type.Key, type.Value);
      }

      ASTType integer = ASTType.NewType(PrimitiveType.Integer, 4);
      ASTType boolean = ASTType.NewType(PrimitiveType.Integer, 1);

      ASTStatement stmt;
      stmt = new ASTStatement();
      // 3 + 6 * (X - 2) + 3
      // -> 6 * (X - 2) + 6  (Associativity)
      // -> 6 * (X - 1)      (Factorisation)
      stmt.PushOperand(null, new ASTOperand("Y", integer, ASTOperandType.Data));
      stmt.PushOperator(null, ASTOperator.Assign);
      stmt.PushInteger(null, integer, 3);
      stmt.PushOperator(null, ASTOperator.Add);
      stmt.PushInteger(null, integer, 6);
      stmt.PushOperator(null, ASTOperator.Mul);
      stmt.OpenParenthesis(null);
      stmt.PushOperand(null, new ASTOperand("x", integer, ASTOperandType.Data));
      stmt.PushOperator(null, ASTOperator.Sub);
      stmt.PushInteger(null, integer, 2);
      stmt.CloseParenthesis(null);
      stmt.PushOperator(null, ASTOperator.Add);
      stmt.PushInteger(null, integer, 3);
      stmt.Compile();
      stmt.Convert();


      // stmt = new ASTStatement();
      // ( x - 1 ) * ( 2 * x + 3 ) - 4 * ( 2 * x + 3 )
      // ( 2 * x + 3 ) * ( ( x - 1) - ( 4 ) )          (Factorisation)
      // ( 2 * x + 3 ) * ( x - 5 )                     (Simplification)

      // ( x - 1 ) * ( 2 * x + 3 ) - 4 * ( 2 * x + 3 )
      // ( x - 1 ) * ( 2 * x + 3 ) - ( 8 * x + 12 )


      // Factorisation
      // ( x - 1 ) * ( 2 * x + 3 ) - 4 * ( 2 * x + 3 )     ->     ( 2 * x + 3 ) * ( x - 5 )
      // 5 * x * ( x - 2 ) + ( 3 * x + 1 ) * ( 2 - x )     ->     ( x - 2 ) * ( 2 * x - 1 )
      // ( 1 - 7 * x ) * ( 3 * x + 5 ) - ( 9 * x + 15 ) * ( x - 4 )   ->    ( 3 * x + 5 ) * ( 13 - 10 * x )


    }


    static string StandardLibraryHeaderDirectory;

    static void Image (string[] args)
    {
      OptionSession<ImageJob, OptionGroup> session = new OptionSession<ImageJob, OptionGroup>();
      session.NewOption("printHeader", OptionGroup.General, 'x', null, null, "...");
      session.NewOption("printDisassemble", OptionGroup.General, 'D', null, null, "...");

      ImageJob ij = new ImageJob();
      List<string> files = session.Parse(args, ij);

      foreach (string file in files) {
        ij.Run(file);
        Console.WriteLine();
      }
    }

    static void Cc (string[] args)
    {
      OptionSession<CCompiler, OptionGroup> session = new OptionSession<CCompiler, OptionGroup>();
      session.NewOption("DoCompile", OptionGroup.Compile_Command, 'c', null, null, "Compile and assemble the source files, but do not link.");
      session.NewOption("DoAssemble", OptionGroup.Compile_Command, 'S', null, null, "Compile but doesn't assemble. The output is formed of mnemonics.");
      session.NewOption("DoPreProcess", OptionGroup.Compile_Command, 'E', null, null, "Stop after the pre-processing stage. Output give all pre-processing decision.");
      session.NewOption("DoParse", OptionGroup.Compile_Command, 'P', null, null, "Parse a C source file and print all definitions.");
      session.NewOption("DoDependancies", OptionGroup.Compile_Command, 'M', null, null, "Print all dependence headers as make target.");

      // session.NewOption("Disx86", OptionGroup.Unit_Test, '\0', "Disx86", null, "Lanch unitt-test for Intelx86 disassembly.");

      session.NewOption("Output", OptionGroup.General, 'o', null, "file", "Place output in file file. By default give text output on stdout and binary on file named a.out.");
      session.NewOption("Statistics", OptionGroup.General, 'Q', null, null, "Print some statistics about each modules/pass.");
      session.NewOption("Warning", OptionGroup.General, 'W', null, "warn", "Enable/disable some compilation warning.");

      session.NewOption("IncludeDirs", OptionGroup.C_Source, 'I', null, "dir", "Add a new header directory.");
      session.NewOption("LibrariesDirs", OptionGroup.C_Source, 'L', null, "dir", "Add a new libraries directory.");
      session.NewOption("Define", OptionGroup.C_Source, 'D', null, "macro", "Define a macro (equal to MACRO=value or MACRO).");
      session.NewOption("Undefine", OptionGroup.C_Source, 'U', null, "macro", "Undefine a macro (equal to MACRO).");

      CCompiler cc = new CCompiler();

      // DEFINE CONFIG_LDDIR="\"lib64\"" & TCC_TARGET_X86_64
      // cc.Define("CONFIG_LDDIR=\"lib64\"");
      // cc.Define("TCC_TARGET_X86_64");
      cc.Define("__STDC_VERSION__=201410");

      // ADD INCLUDE DIR
      cc.IncludeDirs.Add(StandardLibraryHeaderDirectory);

      // Read Command line
      List<string> files = session.Parse(args, cc);

      foreach (string file in files) {
        cc.Compile(file);
        Console.WriteLine();
      }
    }

    static void UTx86 (string[] args)
    {
      string start = "   0:\t";
      string objdump = @"C:\MinGW\bin\objdump";
      string file = @"C:\Users\Aesgar\Downloads\exit.o";
      // string objdump = @"D:\Programs\MinGW\bin\objdump";
      // string file = @"D:/exit.o";


      byte[] instr = new byte[15];
      instr[0] = 1;
      long Tested = 0;
      long Failure = 0;

      for (; ; ) {
        if (instr[0] == 0x0f)
          instr[0]++;
        if (instr[0] == 0x66)
          instr[0] += 2;
        if (instr[0] == 0x82)
          instr[0]++;
        if (instr[0] == 0xc4)
          instr[0] += 2;
        if (instr[0] == 0xce)
          instr[0] += 2;
        if (instr[0] == 0xd2)
          instr[0] += 22;
        if (instr[0] == 0xe8)
          instr[0] += 8;

        // Disassemble using MingW
        try {
          using (BinaryWriter wr = new BinaryWriter(File.Open(file, FileMode.Open, FileAccess.ReadWrite))) {
            wr.BaseStream.Seek(0x104, SeekOrigin.Begin);
            wr.Write(instr);
          }
        } catch (Exception e) {
        }

        // Start Process
        string strCmdText;
        strCmdText = "-d " + file;
        ProcessStartInfo startInfo = new ProcessStartInfo(objdump, strCmdText);
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        Process p = Process.Start(startInfo);

        // Disassemble using Amy
        Intelx86 dn = new Intelx86(instr);
        x86Operator op = dn.Next();
        string amyMn = start;
        for (var i = 0; i < Math.Min(op.Length, 7); ++i)
          amyMn += Hexa.ToString(instr[i], 2, false).Substring(2) + " ";
        for (var i = Math.Min(op.Length, 7); i < 7; ++i)
          amyMn += "   ";
        amyMn += "\t";
        amyMn += op.ATNTWriting;

        int lg = Math.Min(op.Length - 1, 1);
        if (instr[lg] == 0xff)
          while (lg >= 0 && instr[lg] == 0xff)
            instr[lg--] = 0;
        if (lg < 0)
          return;
        instr[lg]++;


        // Read
        // p.WaitForExit();
        StreamReader rd = p.StandardOutput;
        string mingwMn = string.Empty;
        while (!mingwMn.StartsWith(start))
          mingwMn = rd.ReadLine();


        //Console.WriteLine(st);
        Tested++;
        if (mingwMn != amyMn) {
          Failure++;
          Console.WriteLine(mingwMn);
        }
      }

    }

    static void Configuration (string root)
    {
      // StandardLibraryHeaderDirectory = root + ConfigurationManager.AppSettings["StandardCLibrary"].ToString();

      // Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      // AppSettingsSection section = (AppSettingsSection)config.GetSection("appSettings");
      // section.Settings.Remove("K");
      // section.Settings.Add("K", "V");
      // config.Save();
    }

    static void Main (string[] args)
    {
      string dropbox = @"C:\Dropbox\";
      if (!Directory.Exists(dropbox))
        dropbox = @"C:\Dropbox\";

      Configuration(dropbox);


      // Environment.CurrentDirectory = dropbox;
      StandardLibraryHeaderDirectory = @"G:\libc\usr\include";
      Environment.CurrentDirectory = @"G:\libc\usr\";

      // string commandline = "Img exit.o -xD -- UTx86";
      string commandline = "cc -E include/bits/types.h";
      /*  adler32.c
          compress.c
          crc32.c
          deflate.c
          gzclose.c
          gzlib.c
          gzread.c
          gzwrite.c
          inflate.c
          infback.c
          inftrees.c
          inffast.c
          trees.c
          uncompr.c
          zutil.c */

      args = commandline.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      string command = args[0];

      List<string> la = new List<string>(args);
      la.RemoveAt(0);
      args = la.ToArray();

      switch (command) {
        case "cc":
          Cc(args);
          break;

        case "UTx86":
          UTx86(args);
          break;

        case "Img":
          Image(args);
          break;
      }


      // ExtractPreProcessorFromFile(path);

      //gcc -o tcc.o -c tcc.c -DCONFIG_LDDIR="\"lib64\"" -DTCC_TARGET_X86_64 -I.  -Wall -g -O2 -fno-strict-aliasing
      //gcc -o libtcc.o -c libtcc.c -DCONFIG_LDDIR="\"lib64\"" -DTCC_TARGET_X86_64 -I.  -Wall -g -O2 -fno-strict-aliasing
      //gcc -o tccpp.o -c tccpp.c -DCONFIG_LDDIR="\"lib64\"" -DTCC_TARGET_X86_64 -I.  -Wall -g -O2 -fno-strict-aliasing
      //gcc -o tccgen.o -c tccgen.c -DCONFIG_LDDIR="\"lib64\"" -DTCC_TARGET_X86_64 -I.  -Wall -g -O2 -fno-strict-aliasing
      //gcc -o tccelf.o -c tccelf.c -DCONFIG_LDDIR="\"lib64\"" -DTCC_TARGET_X86_64 -I.  -Wall -g -O2 -fno-strict-aliasing
      //gcc -o tccasm.o -c tccasm.c -DCONFIG_LDDIR="\"lib64\"" -DTCC_TARGET_X86_64 -I.  -Wall -g -O2 -fno-strict-aliasing
      //gcc -o tccrun.o -c tccrun.c -DCONFIG_LDDIR="\"lib64\"" -DTCC_TARGET_X86_64 -I.  -Wall -g -O2 -fno-strict-aliasing
      //gcc -o x86_64-gen.o -c x86_64-gen.c -DCONFIG_LDDIR="\"lib64\"" -DTCC_TARGET_X86_64 -I.  -Wall -g -O2 -fno-strict-aliasing
      //gcc -o i386-asm.o -c i386-asm.c -DCONFIG_LDDIR="\"lib64\"" -DTCC_TARGET_X86_64 -I.  -Wall -g -O2 -fno-strict-aliasing
      //ar rcs libtcc.a libtcc.o tccpp.o tccgen.o tccelf.o tccasm.o tccrun.o x86_64-gen.o i386-asm.o
      //gcc -o tcc tcc.o libtcc.a -lm -ldl -I.  -Wall -g -O2 -fno-strict-aliasing

      // gcc -o tcc.o -c tcc.c -DCONFIG_LDDIR="\"lib64\"" -DTCC_TARGET_X86_64 -I.

    }
  }
}
