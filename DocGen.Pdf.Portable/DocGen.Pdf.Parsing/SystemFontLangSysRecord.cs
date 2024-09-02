namespace DocGen.Pdf.Parsing;

internal class SystemFontLangSysRecord
{
	public uint LangSysTag { get; private set; }

	public uint LangSys { get; private set; }

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		LangSysTag = reader.ReadULong();
		LangSys = reader.ReadULong();
	}
}
