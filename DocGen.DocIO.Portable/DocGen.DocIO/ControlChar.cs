namespace DocGen.DocIO;

public class ControlChar
{
	public static readonly string CarriegeReturn = '\r'.ToString();

	public static readonly string CrLf = "\r\n";

	public static readonly string DefaultTextInput = '\u2002'.ToString();

	public static readonly char DefaultTextInputChar = '\u2002';

	public static readonly string LineBreak = "\v";

	public static readonly char LineBreakChar = '\v';

	public static readonly string LineFeed = '\n'.ToString();

	public static readonly char LineFeedChar = '\n';

	public static readonly string NonBreakingSpace = '\u00a0'.ToString();

	public static readonly char NonBreakingSpaceChar = '\u00a0';

	public static readonly string Tab = "\t";

	public static readonly char TabChar = '\t';

	public static readonly string Hyphen = '\u001f'.ToString();

	public static readonly char HyphenChar = '\u001f';

	public static readonly string Space = ' '.ToString();

	public static readonly char SpaceChar = ' ';

	public static readonly char DoubleQuote = '"';

	public static readonly char LeftDoubleQuote = '“';

	public static readonly char RightDoubleQuote = '”';

	public static readonly char DoubleLowQuote = '„';

	internal static readonly string DoubleQuoteString = '"'.ToString();

	internal static readonly string LeftDoubleQuoteString = '“'.ToString();

	internal static readonly string RightDoubleQuoteString = '”'.ToString();

	internal static readonly string DoubleLowQuoteString = '„'.ToString();

	public static readonly string NonBreakingHyphen = '\u001e'.ToString();

	public static readonly char NonBreakingHyphenChar = '\u001e';

	internal static readonly string Cell = '\a'.ToString();

	internal const char CellChar = '\a';

	internal static readonly string ColumnBreak = '\u000e'.ToString();

	internal const char ColumnBreakChar = '\u000e';

	internal const char FieldEndChar = '\u0015';

	internal const char FieldSeparatorChar = '\u0014';

	internal const char FieldStartChar = '\u0013';

	internal static readonly string PageBreak = '\f'.ToString();

	internal const char PageBreakChar = '\f';

	internal static readonly string ParagraphBreak = '\r'.ToString();

	internal const char ParagraphBreakChar = '\r';

	internal static readonly string SectionBreak = '\f'.ToString();

	internal const char SectionBreakChar = '\f';
}
