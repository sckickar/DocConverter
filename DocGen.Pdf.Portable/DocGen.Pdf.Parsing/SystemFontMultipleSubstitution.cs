using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontMultipleSubstitution : SystemFontSubTable
{
	private ushort coverageOffset;

	private ushort[] sequenceOffsets;

	private SystemFontCoverage coverage;

	private SystemFontSequence[] sequences;

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

	protected SystemFontSequence[] Sequences
	{
		get
		{
			if (sequences == null)
			{
				sequences = new SystemFontSequence[sequenceOffsets.Length];
				for (int i = 0; i < sequences.Length; i++)
				{
					sequences[i] = ReadSequence(base.Reader, sequenceOffsets[i]);
				}
			}
			return sequences;
		}
	}

	public SystemFontMultipleSubstitution(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	private SystemFontSequence ReadSequence(SystemFontOpenTypeFontReader reader, ushort offset)
	{
		reader.BeginReadingBlock();
		reader.Seek(base.Offset + offset, SeekOrigin.Begin);
		SystemFontSequence systemFontSequence = new SystemFontSequence();
		systemFontSequence.Read(reader);
		reader.EndReadingBlock();
		return systemFontSequence;
	}

	public override SystemFontGlyphsSequence Apply(SystemFontGlyphsSequence glyphIDs)
	{
		SystemFontGlyphsSequence systemFontGlyphsSequence = new SystemFontGlyphsSequence(glyphIDs.Count);
		for (int i = 0; i < glyphIDs.Count; i++)
		{
			int coverageIndex = Coverage.GetCoverageIndex(glyphIDs[i].GlyphId);
			if (coverageIndex >= 0)
			{
				systemFontGlyphsSequence.AddRange(sequences[coverageIndex].Subsitutes);
			}
		}
		return systemFontGlyphsSequence;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadUShort();
		coverageOffset = reader.ReadUShort();
		ushort num = reader.ReadUShort();
		sequenceOffsets = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			sequenceOffsets[i] = reader.ReadUShort();
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		Coverage.Write(writer);
		writer.WriteUShort((ushort)Sequences.Length);
		for (int i = 0; i < Sequences.Length; i++)
		{
			Sequences[i].Write(writer);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		coverage = SystemFontCoverage.ImportCoverageTable(base.FontSource, reader);
		ushort num = reader.ReadUShort();
		sequences = new SystemFontSequence[num];
		for (int i = 0; i < num; i++)
		{
			SystemFontSequence systemFontSequence = new SystemFontSequence();
			systemFontSequence.Import(reader);
			sequences[i] = systemFontSequence;
		}
	}
}
