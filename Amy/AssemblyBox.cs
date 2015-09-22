using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace AmySuite
{

  enum Architecture
  {
    Intelx86,
    Intelx64,
    ARM,
  }

  [Flags]
  enum AssemblyFlag
  {

  }

  class AssemblyBox
  {
    public List<AssemblySection> sections_ = new List<AssemblySection>();
    public BinaryReader Reader { get; private set; }
    public Architecture Machine { get; private set; }
    public AssemblyFlag Flags { get; private set; }
    public long PageSize { get; protected set; }
    public AssemblySection TextSection { get; private set; }
    public AssemblySection DataSection { get; private set; }
    public ReadOnlyCollection<AssemblySection> Sections
    {
      get
      {
        return sections_.AsReadOnly();
      }
    }

    public AssemblyBox (BinaryReader reader)
    {
      this.Reader = reader;
    }

    public void Add (AssemblySection section)
    {
      sections_.Add(section);
      switch (section.Name) {
        case SectionType.Text:
          this.TextSection = section;
          break;

        case SectionType.Data:
          this.DataSection = section;
          break;
      }
    }
  }


  enum SectionType
  {
    Unknown,
    Text,
    Data,
    Bss,
    Import,
    Symbols,
    Types,
  }

  [Flags]
  enum SectionFlags
  {
  }

  class AssemblySection
  {
    public SectionType Name { get; private set; }
    public string Alias { get; private set; }
    public long Size { get; private set; }
    public long Virtual { get; private set; }
    public SectionFlags Flags { get; private set; }
    public byte[] Data { get; set; }

    public int FileOffset { get; private set; }
    public int FileLength { get; private set; }

    public AssemblySection (string name, long address, long size, SectionFlags flags)
    {
      this.Alias = name;
      this.Size = size;
      this.Virtual = address;
      this.Flags = flags;

      switch (name) {
        case ".text":
          this.Name = SectionType.Text;
          break;

        case ".data":
          this.Name = SectionType.Data;
          break;

        case ".bss":
          this.Name = SectionType.Bss;
          break;
      }
    }

    public void SetFileInfo (int offset, int length)
    {
      this.FileOffset = offset;
      this.FileLength = length;
    }

    public void ReadData (BinaryReader reader)
    {
      reader.BaseStream.Seek(this.FileOffset, SeekOrigin.Begin);
      this.Data = reader.ReadBytes(this.FileLength);
    }

  }

}
