namespace DocGen.Pdf;

internal class RefinementRegionFlags : JBIG2BaseFlags
{
	public const string GR_TEMPLATE = "GR_TEMPLATE";

	public const string TPGDON = "TPGDON";

	internal override void setFlags(int flagsAsInt)
	{
		base.flagsAsInt = flagsAsInt;
		flags.Add("GR_TEMPLATE", flagsAsInt & 1);
		flags.Add("TPGDON", (flagsAsInt >> 1) & 1);
	}
}
