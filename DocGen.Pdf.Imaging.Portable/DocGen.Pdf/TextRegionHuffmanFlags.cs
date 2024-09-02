namespace DocGen.Pdf;

internal class TextRegionHuffmanFlags : JBIG2BaseFlags
{
	public const string SB_HUFF_FS = "SB_HUFF_FS";

	public const string SB_HUFF_DS = "SB_HUFF_DS";

	public const string SB_HUFF_DT = "SB_HUFF_DT";

	public const string SB_HUFF_RDW = "SB_HUFF_RDW";

	public const string SB_HUFF_RDH = "SB_HUFF_RDH";

	public const string SB_HUFF_RDX = "SB_HUFF_RDX";

	public const string SB_HUFF_RDY = "SB_HUFF_RDY";

	public const string SB_HUFF_RSIZE = "SB_HUFF_RSIZE";

	internal override void setFlags(int flagsAsInt)
	{
		base.flagsAsInt = flagsAsInt;
		flags.Add("SB_HUFF_FS", flagsAsInt & 3);
		flags.Add("SB_HUFF_DS", (flagsAsInt >> 2) & 3);
		flags.Add("SB_HUFF_DT", (flagsAsInt >> 4) & 3);
		flags.Add("SB_HUFF_RDW", (flagsAsInt >> 6) & 3);
		flags.Add("SB_HUFF_RDH", (flagsAsInt >> 8) & 3);
		flags.Add("SB_HUFF_RDX", (flagsAsInt >> 10) & 3);
		flags.Add("SB_HUFF_RDY", (flagsAsInt >> 12) & 3);
		flags.Add("SB_HUFF_RSIZE", (flagsAsInt >> 14) & 1);
	}
}
