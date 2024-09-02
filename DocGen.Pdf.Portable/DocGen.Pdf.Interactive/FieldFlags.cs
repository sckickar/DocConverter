using System;

namespace DocGen.Pdf.Interactive;

[Flags]
internal enum FieldFlags
{
	Default = 0,
	ReadOnly = 1,
	Required = 2,
	NoExport = 4,
	Multiline = 0x1000,
	Password = 0x2000,
	FileSelect = 0x100000,
	DoNotSpellCheck = 0x400000,
	DoNotScroll = 0x800000,
	Comb = 0x1000000,
	RichText = 0x2000000,
	NoToggleToOff = 0x4000,
	Radio = 0x8000,
	PushButton = 0x10000,
	RadiosInUnison = 0x2000000,
	Combo = 0x20000,
	Edit = 0x40000,
	Sort = 0x80000,
	MultiSelect = 0x200000,
	CommitOnSelChange = 0x4000000
}
