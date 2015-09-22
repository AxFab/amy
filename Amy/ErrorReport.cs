using System;

using Amy.Lexer;

namespace AmySuite
{
  class Errors
  {
    /** Use of pre-processor #error */
    public static string CP0001 = "User Defined Error: {0}";

    /** Use of pre-processor #warning */
    public static string CP0002 = "User Defined Warning: {0}";

    /** When trying to define an existing macro */
    public static string CP0003 = "Macro {0} is already defined";

  }

  class ErrorReport
  {
    public static void RegError (Token token, string message)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine("\nAt " + token.File + ":" + token.Start.Row + " - " + message, token.Litteral);
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void PreProcessor (string message)
    {
      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.WriteLine("\n" + message);
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void PreProcessor2 (string message)
    {
      Console.ForegroundColor = ConsoleColor.DarkGray;
      Console.WriteLine("\n" + message);
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void ToImplement (string message)
    {
      Console.ForegroundColor = ConsoleColor.DarkRed;
      Console.WriteLine("\n" + message);
      Console.ForegroundColor = ConsoleColor.Gray;
    }

    public static void Error (string message)
    {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine("\n" + message);
      Console.ForegroundColor = ConsoleColor.Gray;
    }
  }
}
