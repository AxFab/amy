using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AmySuite
{
  enum PEArchitecture
  {
    Unknown = 0,
    Matsushita_AM33 = 0x1d3,
    Intel_386 = 0x14c,
    AMD64 = 0x8664,

  }

  [Flags]
  enum PEFlags
  {
    RELOCS_STRIPPED = 0x0001,
    EXECUTABLE_IMAGE = 0x0002,
    LINE_NUMS_STRIPPED = 0x0004,
    LOCAL_SYMS_STRIPPED = 0x0008,
    AGGRESSIVE_WS_TRIM = 0x0010,
    LARGE_ADDRESS_AWARE = 0x0020,
    BYTES_REVERSED_LO = 0x0080,
    _32BIT_MACHINE = 0x0100,
    DEBUG_STRIPPED = 0x0200,
    REMOVABLE_RUN_FROM_SWAP = 0x0400,
    NET_RUN_FROM_SWAP = 0x0800,
    SYSTEM = 0x1000,
    DLL = 0x2000,
    UP_SYSTEM_ONLY = 0x4000,
    BYTES_REVERSED_HI = 0x8000,
  }

  [Flags]
  enum PESectionFlags : uint
  {
    IMAGE_SCN_TYPE_NO_PAD = 0x08,
    Code = 0x20,
    InitData = 0x40,
    UninitData = 0x80,
    IMAGE_SCN_LNK_OTHER = 0x100,
    IMAGE_SCN_LNK_INFO = 0x200,
    IMAGE_SCN_LNK_REMOVE = 0x800,
    IMAGE_SCN_LNK_COMDAT = 0x1000,
    IMAGE_SCN_GPREL = 0x8000,
    IMAGE_SCN_MEM_PURGEABLE = 0x00020000,
    IMAGE_SCN_MEM_16BIT = 0x00020000,
    IMAGE_SCN_MEM_LOCKED = 0x00040000,
    IMAGE_SCN_MEM_PRELOAD = 0x00080000,
    Align1 = 0x00100000,
    Align2 = 0x00200000,
    Align4 = 0x00300000,
    IMAGE_SCN_ALIGN_8BYTES = 0x00400000,
    IMAGE_SCN_ALIGN_16BYTES = 0x00500000,
    IMAGE_SCN_ALIGN_32BYTES = 0x00600000,
    IMAGE_SCN_ALIGN_64BYTES = 0x00700000,
    Align128 = 0x00800000,
    Align256 = 0x00900000,
    Align512 = 0x00A00000,
    Align1k = 0x00B00000,
    Align2k = 0x00C00000,
    Align4k = 0x00D00000,
    Align8k = 0x00E00000,
    IMAGE_SCN_LNK_NRELOC_OVFL = 0x01000000,
    IMAGE_SCN_MEM_DISCARDABLE = 0x02000000,
    IMAGE_SCN_MEM_NOT_CACHED = 0x04000000,
    IMAGE_SCN_MEM_NOT_PAGED = 0x08000000,
    MemShared = 0x10000000,
    MemExec = 0x20000000,
    MemRead = 0x40000000,
    MemWrite = 0x80000000,
  }

  class AssemblyImage
  {
    public static AssemblySection ReadPESection (BinaryReader reader)
    {
      byte[] name = reader.ReadBytes(8);
      uint VirtualSize = reader.ReadUInt32();
      uint VirtualAddress = reader.ReadUInt32();
      uint SizeOfRawData = reader.ReadUInt32();
      uint PointerToRawData = reader.ReadUInt32();
      uint PointerToRelocations = reader.ReadUInt32();
      uint PointerToLinenumbers = reader.ReadUInt32();
      uint NumberOfRelocations = reader.ReadUInt16();
      uint NumberOfLinenumbers = reader.ReadUInt16();
      PESectionFlags Characteristics = (PESectionFlags)reader.ReadUInt32();
      int k = 0;
      while (k < 8 && name[k] != '\0')
        ++k;
      string Name = Encoding.ASCII.GetString(name, 0, k);

      AssemblySection section = new AssemblySection(Name, VirtualAddress, VirtualSize, 0);
      section.SetFileInfo((int)PointerToRawData, (int)SizeOfRawData);
      return section;
    }

    public static AssemblyBox ReadPEFile (BinaryReader reader)
    {
      PEArchitecture machine = (PEArchitecture)reader.ReadUInt16();
      uint NumberOfSections = reader.ReadUInt16();
      uint TimeDateStamp = reader.ReadUInt32();
      uint PointerToSymbolTable = reader.ReadUInt32();
      uint NumberOfSymbols = reader.ReadUInt32();
      uint SizeOfOptionalHeader = reader.ReadUInt16();
      PEFlags Characteristics = (PEFlags)reader.ReadUInt16();

      AssemblyBox box = new AssemblyBox(reader);
      for (int i = 0; i < NumberOfSections; ++i) {
        box.Add(ReadPESection(reader));
      }

      return box;
    }

  }
}
