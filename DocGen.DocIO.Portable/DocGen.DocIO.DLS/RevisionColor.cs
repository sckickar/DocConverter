using System;

namespace DocGen.DocIO.DLS;

[Flags]
public enum RevisionColor
{
	ByAuthor = 0,
	Black = 1,
	Blue = 2,
	BrightGreen = 3,
	DarkBlue = 4,
	DarkRed = 5,
	DarkYellow = 6,
	Gray25 = 7,
	Gray50 = 8,
	Green = 9,
	Pink = 0xA,
	Red = 0xB,
	Teal = 0xC,
	Turquoise = 0xD,
	Violet = 0xE,
	White = 0xF,
	Yellow = 0x10,
	Auto = 0x12,
	ClassicRed = 0x13,
	ClassicBlue = 0x14
}
