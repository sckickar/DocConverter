namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontSystemFontName : SystemFontTrueTypeTableBase
{
	internal override uint Tag => SystemFontTags.NAME_TABLE;

	public abstract string FontFamily { get; }

	internal static SystemFontSystemFontName ReadNameTable(SystemFontOpenTypeFontSourceBase fontSource, SystemFontOpenTypeFontReader reader)
	{
		SystemFontSystemFontName systemFontSystemFontName;
		switch (reader.ReadUShort())
		{
		case 0:
			systemFontSystemFontName = new SystemFontNameFormat0(fontSource);
			break;
		case 1:
			systemFontSystemFontName = new SystemFontNameFormat1(fontSource);
			break;
		default:
			return null;
		}
		systemFontSystemFontName.Read(reader);
		return systemFontSystemFontName;
	}

	public SystemFontSystemFontName(SystemFontOpenTypeFontSourceBase fontSource)
		: base(fontSource)
	{
	}

	internal abstract string ReadName(ushort languageID, ushort nameID);
}
