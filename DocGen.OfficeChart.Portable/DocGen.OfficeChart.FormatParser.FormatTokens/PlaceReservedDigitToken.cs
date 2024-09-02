namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class PlaceReservedDigitToken : DigitToken
{
	private const char DEF_FORMAT_CHAR = '?';

	private const string DEF_EMPTY_DIGIT = " ";

	public override TokenType TokenType => TokenType.PlaceReservedDigit;

	public override char FormatChar => '?';

	protected internal override string GetDigitString(double value, int iDigit, bool bShowHiddenSymbols)
	{
		if (iDigit != 0 || bShowHiddenSymbols || !(value < 1.0))
		{
			return base.GetDigitString(value, iDigit, bShowHiddenSymbols);
		}
		return " ";
	}
}
