namespace DocGen.Pdf.Graphics.Fonts;

internal struct TtfOS2Table
{
	public ushort Version;

	public short XAvgCharWidth;

	public ushort UsWeightClass;

	public ushort UsWidthClass;

	public short FsType;

	public short YSubscriptXSize;

	public short YSubscriptYSize;

	public short YSubscriptXOffset;

	public short YSubscriptYOffset;

	public short ySuperscriptXSize;

	public short YSuperscriptYSize;

	public short YSuperscriptXOffset;

	public short YSuperscriptYOffset;

	public short YStrikeoutSize;

	public short YStrikeoutPosition;

	public short SFamilyClass;

	public byte[] Panose;

	public uint UlUnicodeRange1;

	public uint UlUnicodeRange2;

	public uint UlUnicodeRange3;

	public uint UlUnicodeRange4;

	public byte[] AchVendID;

	public ushort FsSelection;

	public ushort UsFirstCharIndex;

	public ushort UsLastCharIndex;

	public short STypoAscender;

	public short STypoDescender;

	public short STypoLineGap;

	public ushort UsWinAscent;

	public ushort UsWinDescent;

	public uint UlCodePageRange1;

	public uint UlCodePageRange2;

	public short SxHeight;

	public short SCapHeight;

	public ushort UsDefaultChar;

	public ushort UsBreakChar;

	public ushort UsMaxContext;
}
