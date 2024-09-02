namespace DocGen.Pdf.Parsing;

internal class SystemFontSupplement
{
	public void Read(SystemFontCFFFontReader reader)
	{
		reader.ReadCard8();
		reader.ReadSID();
	}
}
