using System;

namespace HarfBuzzSharp;

[Flags]
public enum BufferFlags
{
	Default = 0,
	BeginningOfText = 1,
	EndOfText = 2,
	PreserveDefaultIgnorables = 4,
	RemoveDefaultIgnorables = 8,
	DoNotInsertDottedCircle = 0x10
}
