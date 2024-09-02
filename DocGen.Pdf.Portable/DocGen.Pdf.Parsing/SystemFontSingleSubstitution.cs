namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontSingleSubstitution : SystemFontSubTable
{
	private ushort coverageOffset;

	private SystemFontCoverage coverage;

	protected SystemFontCoverage Coverage
	{
		get
		{
			if (coverage == null)
			{
				coverage = ReadCoverage(base.Reader, coverageOffset);
			}
			return coverage;
		}
	}

	internal static SystemFontSingleSubstitution CreateSingleSubstitutionTable(SystemFontOpenTypeFontSourceBase fontFile, ushort format)
	{
		return format switch
		{
			1 => new SystemFontSingleSubstitutionFormat1(fontFile), 
			2 => new SystemFontSingleSubstitutionFormat2(fontFile), 
			_ => null, 
		};
	}

	public SystemFontSingleSubstitution(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		coverageOffset = reader.ReadUShort();
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		Coverage.Write(writer);
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		coverage = SystemFontCoverage.ImportCoverageTable(base.FontSource, reader);
	}
}
