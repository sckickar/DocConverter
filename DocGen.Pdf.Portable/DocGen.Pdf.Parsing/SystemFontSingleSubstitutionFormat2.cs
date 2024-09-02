namespace DocGen.Pdf.Parsing;

internal class SystemFontSingleSubstitutionFormat2 : SystemFontSingleSubstitution
{
	private ushort[] substitutes;

	public SystemFontSingleSubstitutionFormat2(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public override SystemFontGlyphsSequence Apply(SystemFontGlyphsSequence glyphIDs)
	{
		SystemFontGlyphsSequence systemFontGlyphsSequence = new SystemFontGlyphsSequence(glyphIDs.Count);
		for (int i = 0; i < glyphIDs.Count; i++)
		{
			int coverageIndex = base.Coverage.GetCoverageIndex(glyphIDs[i].GlyphId);
			if (coverageIndex < 0)
			{
				systemFontGlyphsSequence.Add(glyphIDs[i]);
			}
			else
			{
				systemFontGlyphsSequence.Add(substitutes[coverageIndex]);
			}
		}
		return systemFontGlyphsSequence;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		base.Read(reader);
		ushort num = reader.ReadUShort();
		substitutes = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			substitutes[i] = reader.ReadUShort();
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(2);
		base.Write(writer);
		writer.WriteUShort((ushort)substitutes.Length);
		for (int i = 0; i < substitutes.Length; i++)
		{
			writer.WriteUShort(substitutes[i]);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		base.Import(reader);
		ushort num = reader.ReadUShort();
		substitutes = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			substitutes[i] = reader.ReadUShort();
		}
	}
}
