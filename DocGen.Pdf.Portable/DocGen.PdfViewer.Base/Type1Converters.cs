namespace DocGen.PdfViewer.Base;

internal static class Type1Converters
{
	public static EncodingConverter EncodingConverter { get; private set; }

	public static PostScriptObjectConverter PostScriptObjectConverter { get; private set; }

	static Type1Converters()
	{
		EncodingConverter = new EncodingConverter();
		PostScriptObjectConverter = new PostScriptObjectConverter();
	}
}
