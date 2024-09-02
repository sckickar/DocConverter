namespace DocGen.Pdf.Native;

internal enum StringInfoCtype1 : ushort
{
	C1_UPPER = 1,
	C1_LOWER = 2,
	C1_DIGIT = 4,
	C1_SPACE = 8,
	C1_PUNCT = 0x10,
	C1_CNTRL = 0x20,
	C1_BLANK = 0x40,
	C1_XDIGIT = 0x80,
	C1_ALPHA = 0x100
}
