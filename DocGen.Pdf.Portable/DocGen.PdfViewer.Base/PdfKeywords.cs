namespace DocGen.PdfViewer.Base;

internal static class PdfKeywords
{
	internal const string True = "true";

	internal const string False = "false";

	internal const string IndirectReference = "R";

	internal const string Null = "null";

	internal const string StartXRef = "startxref";

	internal const string XRef = "xref";

	internal const string Trailer = "trailer";

	internal const string StreamStart = "stream";

	internal const string StreamEnd = "endstream";

	internal const string IndirectObjectStart = "obj";

	internal const string IndirectObjectEnd = "endobj";

	internal const string PdfHeader = "%PDF-";

	internal const string BinaryMarker = "%âãÏÓ";

	internal const string EndOfFile = "%%EOF";

	internal const string DictionaryStart = "<<";

	internal const string DictionaryEnd = ">>";

	internal const string EndOfInlineImage = "EI";

	internal const string StandardEncoding = "StandardEncoding";

	internal const string ISOLatin1Encoding = "ISOLatin1Encoding";

	public static bool IsKeyword(string str)
	{
		if (str != null)
		{
			if (!(str == "StandardEncoding"))
			{
				return str == "ISOLatin1Encoding";
			}
			return true;
		}
		return false;
	}

	internal static object GetValue(string keyword)
	{
		if (keyword != null && keyword == "StandardEncoding")
		{
			object[] standardEncoding = PresettedEncodings.StandardEncoding;
			return new PostScriptArray(standardEncoding);
		}
		return null;
	}
}
