namespace DocGen.Pdf;

internal class PageInformationFlags : JBIG2BaseFlags
{
	internal const string DEFAULT_PIXEL_VALUE = "DEFAULT_PIXEL_VALUE";

	internal const string DEFAULT_COMBINATION_OPERATOR = "DEFAULT_COMBINATION_OPERATOR";

	internal override void setFlags(int flagAsInt)
	{
		flagsAsInt = flagAsInt;
		flags.Add("DEFAULT_PIXEL_VALUE", (flagAsInt >> 2) & 1);
		flags.Add("DEFAULT_COMBINATION_OPERATOR", (flagAsInt >> 3) & 3);
	}
}
