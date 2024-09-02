using System;
using System.IO;

namespace DocGen.Pdf.Parsing;

internal static class SystemFontDocumentsHelper
{
	public const char SpaceSymbol = ' ';

	public const char TabSymbol = '\t';

	public const char NewLine = '\n';

	public const char ZeroWidthSymbol = '\u200d';

	public const char LineHeightMeasureSymbol = 'X';

	public static Uri GetResourceUri(string resource)
	{
		return new Uri("/DocGen.Pdf.Portable;component/" + resource, UriKind.Relative);
	}

	public static Stream GetResourceStream(string resource)
	{
		return new MemoryStream();
	}

	public static bool IsLineBreak(char ch)
	{
		return ch == '\n';
	}

	public static bool IsTab(char ch)
	{
		return ch == '\t';
	}

	public static bool IsWhiteSpace(char ch)
	{
		return ch == ' ';
	}
}
