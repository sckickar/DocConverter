using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontLigatureSubstitution : SystemFontSubTable
{
	private ushort coverageOffset;

	private ushort[] ligatureSetOffsets;

	private SystemFontLigatureSet[] ligatureSets;

	private SystemFontCoverage coverage;

	public SystemFontCoverage Coverage
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

	public SystemFontLigatureSet[] LigatureSets
	{
		get
		{
			if (ligatureSets == null)
			{
				ligatureSets = new SystemFontLigatureSet[ligatureSetOffsets.Length];
				for (int i = 0; i < ligatureSets.Length; i++)
				{
					ligatureSets[i] = ReadLigatureSet(base.Reader, ligatureSetOffsets[i]);
				}
			}
			return ligatureSets;
		}
	}

	public SystemFontLigatureSubstitution(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	private SystemFontLigatureSet ReadLigatureSet(SystemFontOpenTypeFontReader reader, ushort offset)
	{
		reader.BeginReadingBlock();
		long offset2 = base.Offset + offset;
		reader.Seek(offset2, SeekOrigin.Begin);
		SystemFontLigatureSet systemFontLigatureSet = new SystemFontLigatureSet(base.FontSource);
		systemFontLigatureSet.Read(reader);
		systemFontLigatureSet.Offset = offset2;
		reader.EndReadingBlock();
		return systemFontLigatureSet;
	}

	public override SystemFontGlyphsSequence Apply(SystemFontGlyphsSequence glyphIDs)
	{
		SystemFontGlyphsSequence systemFontGlyphsSequence = new SystemFontGlyphsSequence(glyphIDs.Count);
		for (int i = 0; i < glyphIDs.Count; i++)
		{
			int coverageIndex = Coverage.GetCoverageIndex(glyphIDs[i].GlyphId);
			if (coverageIndex < 0)
			{
				systemFontGlyphsSequence.Add(glyphIDs[i]);
				continue;
			}
			SystemFontLigature systemFontLigature = LigatureSets[coverageIndex].FindLigature(glyphIDs, i);
			if (systemFontLigature != null)
			{
				systemFontGlyphsSequence.Add(systemFontLigature.LigatureGlyphId);
				i += systemFontLigature.Length;
			}
			else
			{
				systemFontGlyphsSequence.Add(glyphIDs[i]);
			}
		}
		return systemFontGlyphsSequence;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadUShort();
		coverageOffset = reader.ReadUShort();
		ushort num = reader.ReadUShort();
		ligatureSetOffsets = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			ligatureSetOffsets[i] = reader.ReadUShort();
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		Coverage.Write(writer);
		writer.WriteUShort((ushort)ligatureSetOffsets.Length);
		SystemFontLigatureSet[] array = LigatureSets;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Write(writer);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		coverage = SystemFontCoverage.ImportCoverageTable(base.FontSource, reader);
		ushort num = reader.ReadUShort();
		ligatureSets = new SystemFontLigatureSet[num];
		for (int i = 0; i < num; i++)
		{
			SystemFontLigatureSet systemFontLigatureSet = new SystemFontLigatureSet(base.FontSource);
			systemFontLigatureSet.Import(reader);
			ligatureSets[i] = systemFontLigatureSet;
		}
	}
}
