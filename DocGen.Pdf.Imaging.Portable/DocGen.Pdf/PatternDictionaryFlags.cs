namespace DocGen.Pdf;

internal class PatternDictionaryFlags : JBIG2BaseFlags
{
	public const string HD_MMR = "HD_MMR";

	public const string HD_TEMPLATE = "HD_TEMPLATE";

	internal override void setFlags(int flagsAsInt)
	{
		base.flagsAsInt = flagsAsInt;
		flags.Add("HD_MMR", flagsAsInt & 1);
		flags.Add("HD_TEMPLATE", (flagsAsInt >> 1) & 3);
	}
}
