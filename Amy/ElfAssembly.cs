using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AmySuite
{
  public enum ElfFileType
  {
    None = 0,
    RelocatableFile = 1,
    ExecuatbleFile = 2,
    SharedObject = 3,
    CoreFile = 4,
    ProcessorSpecific = 0xff00,
    ProcessorSpecific2 = 0xffff,
  }

  public enum ElfMachine
  {
    NoMachine = 0,
    ATnT_We_32100 = 1,
    SPARC = 2,
    Intel_80386 = 3,
    Motorola_68000 = 4,
    Motorola_88000 = 5,
    Intel_80860 = 7,
    MIPS_RS3000 = 8,
  }

  public enum ElfPType : uint
  {
    Null = 0,
    Load = 1,
    Dynamic = 2,
    Interp = 3,
    Note = 4,
    Shlib = 5,
    Phdr = 6,
    LoProc = 0x70000000,
    HiProc = 0x7fffffff,
  }

  [Flags]
  public enum ElfPFlags : uint
  {
    ExecRight = 1,
    WriteRight = 2,
    ReadRight = 4,
  }

  [Flags]
  public enum ElfSFlags : uint
  {
    Write = 1,
    Alloc = 2,
    Exec = 4,
    Proc = 0xf0000000,
  }

  public enum ElfSType : uint
  {
    Null = 0,
    ProgBits = 1,
    SymTab = 2,
    StrTab = 3,
    Rela = 4,
    Hash = 5,
    Dynamic = 6,
    Note = 7,
    NoBits = 8,
    Rel = 9,
    Shlib = 10,
    DynSym = 11,
    LoProc = 0x70000000,
    HiProc = 0x7fffffff,
    LoUser = 0x80000000,
    HiUser = 0xffffffff,
  }



  class ElfPHeader
  {
    private ElfPType type;
    private uint fileAddr;
    private uint virtAddr;
    private uint physAddr;
    private uint fileSize;
    private uint memSize;
    private ElfPFlags flags;
    private uint align;

    private byte[] data;

    public string Name
    {
      get
      {
        return Encoding.ASCII.GetString(data);
      }
    }

    public ElfPHeader (BinaryReader reader)
    {
      this.type = (ElfPType)reader.ReadUInt32();
      this.fileAddr = reader.ReadUInt32();
      this.virtAddr = reader.ReadUInt32();
      this.physAddr = reader.ReadUInt32();
      this.fileSize = reader.ReadUInt32();
      this.memSize = reader.ReadUInt32();
      this.flags = (ElfPFlags)reader.ReadUInt32();
      this.align = reader.ReadUInt32();

      if (this.fileAddr != 0) {
        if (this.fileSize > this.memSize)
          throw new Exception();
        reader.BaseStream.Seek(this.fileAddr, SeekOrigin.Begin);
        this.data = new byte[this.memSize];
        reader.BaseStream.Read(data, 0, (int)this.fileSize);
      }
    }

  }

  class ElfSection
  {
    public uint name;
    public ElfSType type;
    public ElfSFlags flags;
    public uint addr;
    public uint offset;
    public uint size;
    public uint link;
    public uint info;
    public uint addralign;
    public uint entsize;

    public byte[] data;

    public string Title;

    public string Name
    {
      get
      {
        return Encoding.ASCII.GetString(data);
      }
    }

    public ElfSection (BinaryReader reader)
    {
      this.name = reader.ReadUInt32();
      this.type = (ElfSType)reader.ReadUInt32();
      this.flags = (ElfSFlags)reader.ReadUInt32();
      this.addr = reader.ReadUInt32();
      this.offset = reader.ReadUInt32();
      this.size = reader.ReadUInt32();
      this.link = reader.ReadUInt32();
      this.info = reader.ReadUInt32();
      this.addralign = reader.ReadUInt32();
      this.entsize = reader.ReadUInt32();

      if (this.offset != 0) {
        reader.BaseStream.Seek(this.offset, SeekOrigin.Begin);
        this.data = new byte[this.size];
        reader.BaseStream.Read(data, 0, (int)this.size);
      }
    }

    public override string ToString ()
    {
      return string.Format("{0} <{1}-{2}>[{3}]({4})", Title, offset, size, type, flags);
    }
  }


  class ElfAssembly
  {
    private ElfFileType type;
    private ElfMachine machine;
    private uint version;
    private uint entry;
    private uint phOff;
    private uint shOff;
    private uint flags;
    private int ehSize;
    private int phSize;
    private int phCount;
    private int shSize;
    private int shCount;
    private int shstRndx;

    List<ElfPHeader> headers = new List<ElfPHeader>();
    List<ElfSection> sections = new List<ElfSection>();

    public ElfSection symbolNames;
    public ElfSection sectionNames;
    public ElfSection textSection;

    public ElfAssembly (string url)
    {
      using (BinaryReader rd = new BinaryReader(File.OpenRead(url))) {
        if (rd.ReadInt32() != 0x464c457f)
          throw new Exception("Not ELF Format");
        rd.BaseStream.Seek(16, SeekOrigin.Begin);

        this.type = (ElfFileType)rd.ReadUInt16(); // 1Reloc, 2Exec,3Shared, 4Core, 0xff00|0xffff Process
        this.machine = (ElfMachine)rd.ReadUInt16(); // 0-8  (3)
        this.version = rd.ReadUInt32();
        this.entry = rd.ReadUInt32();
        this.phOff = rd.ReadUInt32();
        this.shOff = rd.ReadUInt32();
        this.flags = rd.ReadUInt32();
        this.ehSize = rd.ReadInt16();
        this.phSize = rd.ReadInt16();
        this.phCount = rd.ReadInt16();
        this.shSize = rd.ReadInt16();
        this.shCount = rd.ReadInt16();
        this.shstRndx = rd.ReadInt16();



        rd.BaseStream.Seek(this.phOff, SeekOrigin.Begin);
        for (int i = 0; i < phCount; ++i) {
          rd.BaseStream.Seek(this.phOff + i * 32, SeekOrigin.Begin);
          ElfPHeader eph = new ElfPHeader(rd);
          headers.Add(eph);
        }

        rd.BaseStream.Seek(this.shOff, SeekOrigin.Begin);
        for (int i = 0; i < shCount; ++i) {
          rd.BaseStream.Seek(this.shOff + i * this.shSize, SeekOrigin.Begin);
          ElfSection esh = new ElfSection(rd);
          sections.Add(esh);

          if (esh.type == ElfSType.StrTab && (esh.flags & ElfSFlags.Alloc) == 0)
            sectionNames = esh;
        }

        foreach (ElfSection sec in sections) {
          int k = (int)sec.name;
          while (sectionNames.data[k] != '\0')
            ++k;
          k -= (int)sec.name;
          sec.Title = Encoding.ASCII.GetString(sectionNames.data, (int)sec.name, k);

          switch (sec.Title) {
            case ".text":
              textSection = sec;
              break;
          }

          Console.WriteLine(sec.Title);
        }
      }
    }
  }

}
