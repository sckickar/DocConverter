namespace DocGen.Pdf;

internal class HalftoneRegionFlags : JBIG2BaseFlags
{
	internal const string H_MMR = "H_MMR";

	internal const string H_TEMPLATE = "H_TEMPLATE";

	internal const string H_ENABLE_SKIP = "H_ENABLE_SKIP";

	internal const string H_COMB_OP = "H_COMB_OP";

	internal const string H_DEF_PIXEL = "H_DEF_PIXEL";

	internal override void setFlags(int flagsAsInt)
	{
		base.flagsAsInt = flagsAsInt;
		flags.Add("H_MMR", flagsAsInt & 1);
		flags.Add("H_TEMPLATE", (flagsAsInt >> 1) & 3);
		flags.Add("H_ENABLE_SKIP", (flagsAsInt >> 3) & 1);
		flags.Add("H_COMB_OP", (flagsAsInt >> 4) & 7);
		flags.Add("H_DEF_PIXEL", (flagsAsInt >> 7) & 1);
	}
}
