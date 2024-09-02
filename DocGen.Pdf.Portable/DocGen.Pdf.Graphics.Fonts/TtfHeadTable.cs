namespace DocGen.Pdf.Graphics.Fonts;

internal struct TtfHeadTable
{
	public long Modified;

	public long Created;

	public uint MagicNumber;

	public uint CheckSumAdjustment;

	public float FontRevision;

	public float Version;

	public short XMin;

	public short YMin;

	public ushort UnitsPerEm;

	public short YMax;

	public short XMax;

	public ushort MacStyle;

	public ushort Flags;

	public ushort LowestRecPPEM;

	public short FontDirectionHint;

	public short IndexToLocFormat;

	public short GlyphDataFormat;
}
