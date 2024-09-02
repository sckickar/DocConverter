namespace DocGen.DocIO;

internal class SpecialCharacters
{
	public const char ParagraphEnd = '\r';

	public const char PageBreak = '\f';

	public const char ColumnBreak = '\u000e';

	public const char TableAscii = '\a';

	public const char ImageAscii = '\u0001';

	public const char ShapeAscii = '\b';

	public const char FootnoteAscii = '\u0002';

	public const char FieldBeginMark = '\u0013';

	public const char FieldEndMark = '\u0015';

	public const char FieldSeparator = '\u0014';

	public const char TabAscii = '\t';

	public const char LineBreakAscii = '\v';

	public const char SymbolAscii = '(';

	public const char AnnotationAscii = '\u0005';

	public const char CurrPageNumber = '\0';

	public static readonly string FootnoteAsciiStr = '\u0002'.ToString();

	public static readonly string PageBreakStr = '\f'.ToString();

	internal const char NonBreakingHyphen = '\u001e';

	internal const char SoftHyphen = '\u001f';

	internal const char NonBreakingSpace = '\u00a0';

	internal const char Separator = '\u0003';

	internal const char ContinuationSeparator = '\u0004';

	public static readonly char[] SpecialSymbolArr = new char[13]
	{
		'\r', '\f', '\u000e', '\u0001', '\a', '\u0002', '\t', '\u0013', '\u0014', '\u0015',
		'\v', '\b', '\u0005'
	};
}
