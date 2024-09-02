namespace DocGen.Pdf;

internal class TextRegionFlags : JBIG2BaseFlags
{
	public const string SB_HUFF = "SB_HUFF";

	public const string SB_REFINE = "SB_REFINE";

	public const string LOG_SB_STRIPES = "LOG_SB_STRIPES";

	public const string REF_CORNER = "REF_CORNER";

	public const string TRANSPOSED = "TRANSPOSED";

	public const string SB_COMB_OP = "SB_COMB_OP";

	public const string SB_DEF_PIXEL = "SB_DEF_PIXEL";

	public const string SB_DS_OFFSET = "SB_DS_OFFSET";

	public const string SB_R_TEMPLATE = "SB_R_TEMPLATE";

	internal override void setFlags(int flagsAsInt)
	{
		base.flagsAsInt = flagsAsInt;
		flags.Add("SB_HUFF", flagsAsInt & 1);
		flags.Add("SB_REFINE", (flagsAsInt >> 1) & 1);
		flags.Add("LOG_SB_STRIPES", (flagsAsInt >> 2) & 3);
		flags.Add("REF_CORNER", (flagsAsInt >> 4) & 3);
		flags.Add("TRANSPOSED", (flagsAsInt >> 6) & 1);
		flags.Add("SB_COMB_OP", (flagsAsInt >> 7) & 3);
		flags.Add("SB_DEF_PIXEL", (flagsAsInt >> 9) & 1);
		int num = (flagsAsInt >> 10) & 0x1F;
		if (((uint)num & 0x10u) != 0)
		{
			num |= -16;
		}
		flags.Add("SB_DS_OFFSET", num);
		flags.Add("SB_R_TEMPLATE", (flagsAsInt >> 15) & 1);
	}
}
