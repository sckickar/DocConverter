using System;

namespace DocGen.CompoundFile.DocIO;

[Flags]
public enum PropertyType
{
	Bool = 0xB,
	Int = 0x16,
	Int32 = 3,
	Int16 = 2,
	UInt32 = 0x13,
	String = 0x1F,
	AsciiString = 0x1E,
	DateTime = 0x40,
	Blob = 0x41,
	Vector = 0x1000,
	Object = 0xC,
	Double = 5,
	Empty = 0,
	Null = 1,
	ClipboardData = 0x47,
	AsciiStringArray = 0x101E,
	StringArray = 0x101F,
	ObjectArray = 0x100C
}
