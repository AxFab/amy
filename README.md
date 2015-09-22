# Amy Suite

_Amy is for AsseMblY._

Amy is a small project I'm working from time to time to __experiment__ about 
languages, expression and machine code.

This project regroup several items but their is not yet a real program. This 
is more like a subset of code that I can experiment with.


# Expressions

  The package `Amy.Expressions` include an utility class named the 
  `Resolver`. Its use is to transform mathematical or logical expression 
  into machine code. I don't endup on machine code yet but I get SSA 
  instruction which is dummy to convert.

  I only tested with constant expression yet and I still try to figure out 
  how to design this part of the solution.

# Macro

  I developed a C - preporcessor parser that works fine except for the 
  operators `#` and `##` which are a bit more tricky.

# Lexer

  The Lexer (or Tokenizer) can be configure with various languages. Then the
  method `ReadToken()` return an `Token` object that is already identified 
  as an operator, number, litteral string or identifier. The parser only 
  have to open a source file throught the lexer and read token as a stream.

  Note that the `CPreProcessor` works as a filter over this stream of tokens.
  The pre processor interpret pre processor command and decide which token to
  transmet to the C compilor. 

# Disassembler

  The disassembler is able to open a binary object, find the .text section
  and transform the mechine code into a serie of `IROpcode` objects. 
  the goal is to make this object as generic as possible to handle several 
  different assembly. Once the object created, the assembly language can be 
  dummed using a simple ToString() like method (named `ATnTForm` & `IntelForm`).


