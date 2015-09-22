using System;
using Amy.Lexer;

namespace Amy.Test 
{
  
  class TestLexer
  {
    public void Write () {
      Language lang = Language.Load("basics.xml");
      List<Token> tokens;
      tokens = new List<> (Tokenizer.AnalyzeString (lang, "9+326*sin(4)")); 
    }
  }

}