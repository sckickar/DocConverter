namespace DocGen.PdfViewer.Base;

internal class Type1FontReader : PostScriptParser
{
	public Type1FontReader(byte[] data)
		: base(data)
	{
	}

	public static byte[] StripData(byte[] data)
	{
		if (Chars.IsValidHexCharacter(data[0]))
		{
			return new StdFontReader().ReadData(data);
		}
		return data;
	}
}
