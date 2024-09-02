namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontCoverage : SystemFontTableBase
{
	private static SystemFontCoverage CreateCoverageTable(SystemFontOpenTypeFontSourceBase fontSource, SystemFontOpenTypeFontReader reader)
	{
		long position = reader.Position;
		SystemFontCoverage systemFontCoverage;
		switch (reader.ReadUShort())
		{
		case 1:
			systemFontCoverage = new SystemFontCoverageFormat1(fontSource);
			break;
		case 2:
			systemFontCoverage = new SystemFontCoverageFormat2(fontSource);
			break;
		default:
			return null;
		}
		systemFontCoverage.Offset = position;
		return systemFontCoverage;
	}

	internal static SystemFontCoverage ReadCoverageTable(SystemFontOpenTypeFontSourceBase fontSource, SystemFontOpenTypeFontReader reader)
	{
		SystemFontCoverage systemFontCoverage = CreateCoverageTable(fontSource, reader);
		systemFontCoverage?.Read(reader);
		return systemFontCoverage;
	}

	internal static SystemFontCoverage ImportCoverageTable(SystemFontOpenTypeFontSourceBase fontSource, SystemFontOpenTypeFontReader reader)
	{
		SystemFontCoverage systemFontCoverage = CreateCoverageTable(fontSource, reader);
		systemFontCoverage?.Import(reader);
		return systemFontCoverage;
	}

	public SystemFontCoverage(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		Read(reader);
	}

	public abstract int GetCoverageIndex(ushort glyphIndex);
}
