using System.IO;

namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontSubTable : SystemFontTableBase
{
	private static SystemFontSubTable CreateSubTable(SystemFontOpenTypeFontSourceBase fontSource, ushort type, SystemFontOpenTypeFontReader reader)
	{
		long position = reader.Position;
		switch (type)
		{
		case 1:
		{
			ushort format = reader.ReadUShort();
			SystemFontSingleSubstitution systemFontSingleSubstitution = SystemFontSingleSubstitution.CreateSingleSubstitutionTable(fontSource, format);
			systemFontSingleSubstitution.Offset = position;
			return systemFontSingleSubstitution;
		}
		case 2:
			return new SystemFontMultipleSubstitution(fontSource)
			{
				Offset = position
			};
		case 4:
			return new SystemFontLigatureSubstitution(fontSource)
			{
				Offset = position
			};
		default:
			return null;
		}
	}

	internal static SystemFontSubTable ReadSubTable(SystemFontOpenTypeFontSourceBase fontSource, SystemFontOpenTypeFontReader reader, ushort type)
	{
		SystemFontSubTable systemFontSubTable = CreateSubTable(fontSource, type, reader);
		systemFontSubTable?.Read(reader);
		return systemFontSubTable;
	}

	internal static SystemFontSubTable ImportSubTable(SystemFontOpenTypeFontSourceBase fontSource, SystemFontOpenTypeFontReader reader, ushort type)
	{
		SystemFontSubTable systemFontSubTable = CreateSubTable(fontSource, type, reader);
		systemFontSubTable?.Import(reader);
		return systemFontSubTable;
	}

	public SystemFontSubTable(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	protected SystemFontCoverage ReadCoverage(SystemFontOpenTypeFontReader reader, ushort offset)
	{
		reader.BeginReadingBlock();
		reader.Seek(base.Offset + offset, SeekOrigin.Begin);
		SystemFontCoverage result = SystemFontCoverage.ReadCoverageTable(base.FontSource, reader);
		reader.EndReadingBlock();
		return result;
	}

	public abstract SystemFontGlyphsSequence Apply(SystemFontGlyphsSequence glyphIndices);
}
