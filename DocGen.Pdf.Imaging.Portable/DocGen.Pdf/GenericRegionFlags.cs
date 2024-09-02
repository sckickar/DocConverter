namespace DocGen.Pdf;

internal class GenericRegionFlags : JBIG2BaseFlags
{
	public const string MMR = "MMR";

	public const string GB_TEMPLATE = "GB_TEMPLATE";

	public const string TPGDON = "TPGDON";

	internal override void setFlags(int flagsAsInt)
	{
		base.flagsAsInt = flagsAsInt;
		flags.Add("MMR", flagsAsInt & 1);
		flags.Add("GB_TEMPLATE", (flagsAsInt >> 1) & 3);
		flags.Add("TPGDON", (flagsAsInt >> 3) & 1);
	}
}
