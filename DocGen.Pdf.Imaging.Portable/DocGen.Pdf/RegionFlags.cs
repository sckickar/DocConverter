namespace DocGen.Pdf;

internal class RegionFlags : JBIG2BaseFlags
{
	public const string EXTERNAL_COMBINATION_OPERATOR = "EXTERNAL_COMBINATION_OPERATOR";

	internal override void setFlags(int flagsAsInt)
	{
		base.flagsAsInt = flagsAsInt;
		flags.Add("EXTERNAL_COMBINATION_OPERATOR", flagsAsInt & 7);
	}
}
