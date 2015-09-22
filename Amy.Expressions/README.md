

This package provider one important class, the Expression Resolver.
An instance of this can be used to resolve mathematical and logical 
expressions. The developer push operand and operator into the resolver. The 
token have to be pushed in prefix order (reading order). The resolver will 
sort them as a tree. And then will be able to give the corresponding SSA 
flow chart (postfix order).

```cs
class Resolver
{
  bool OpenParenthese(object token);
  bool CloseParenthese(object token);
  bool Push(object token, Primitive type, long value);
  bool Push(object token, bool value);
  bool Push(object token, int value);
  bool Push(object token, long value);
  bool Push(object token, short value);
  bool Push(object token, sbyte value);
  bool Push(object token, uint value);
  bool Push(object token, ulong value);
  bool Push(object token, ushort value);
  bool Push(object token, byte value);
  bool Push(object token, float value);
  bool Push(object token, double value);
  bool Push(object token, decimal value);
  bool Push(object token, string value);
  bool Push(object token, Opcode opcode);
  bool Compile();
  bool Optimize();
  SSAFlow SSA(bool removeConst = true);
  bool IsTrue;
  Primitive Type;
  bool Value_b;
  sbyte Value_i8;
  byte Value_u8;
  short Value_i16;
  float Value_f;
  double Value_d;
  decimal Value_m;
  long Value_p;
}
```

Optimization can be done both by the Resolver or by the SSAFlow.


