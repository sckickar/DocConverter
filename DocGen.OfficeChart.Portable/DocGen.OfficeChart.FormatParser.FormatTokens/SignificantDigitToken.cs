namespace DocGen.OfficeChart.FormatParser.FormatTokens;

internal class SignificantDigitToken : DigitToken
{
	private const char DEF_FORMAT_CHAR = '0';

	public override TokenType TokenType => TokenType.SignificantDigit;

	public override char FormatChar => '0';
}
