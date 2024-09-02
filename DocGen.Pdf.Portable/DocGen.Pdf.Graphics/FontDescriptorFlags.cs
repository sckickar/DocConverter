namespace DocGen.Pdf.Graphics;

internal enum FontDescriptorFlags
{
	FixedPitch = 1,
	Serif = 2,
	Symbolic = 4,
	Script = 8,
	Nonsymbolic = 0x20,
	Italic = 0x40,
	ForceBold = 0x40000
}
