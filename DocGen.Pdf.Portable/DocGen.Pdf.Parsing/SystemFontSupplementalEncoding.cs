namespace DocGen.Pdf.Parsing;

internal class SystemFontSupplementalEncoding
{
	private SystemFontSupplement[] supplements;

	public void Read(SystemFontCFFFontReader reader)
	{
		byte b = reader.ReadCard8();
		supplements = new SystemFontSupplement[b];
		for (int i = 0; i < b; i++)
		{
			SystemFontSupplement systemFontSupplement = new SystemFontSupplement();
			systemFontSupplement.Read(reader);
			supplements[i] = systemFontSupplement;
		}
	}
}
