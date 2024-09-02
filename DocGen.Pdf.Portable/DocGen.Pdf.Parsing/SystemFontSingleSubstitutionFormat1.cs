namespace DocGen.Pdf.Parsing;

internal class SystemFontSingleSubstitutionFormat1 : SystemFontSingleSubstitution
{
	private ushort deltaGlyphId;

	public SystemFontSingleSubstitutionFormat1(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public override SystemFontGlyphsSequence Apply(SystemFontGlyphsSequence glyphIDs)
	{
		SystemFontGlyphsSequence systemFontGlyphsSequence = new SystemFontGlyphsSequence(glyphIDs.Count);
		for (int i = 0; i < glyphIDs.Count; i++)
		{
			ushort glyphId = glyphIDs[i].GlyphId;
			if (base.Coverage.GetCoverageIndex(glyphId) < 0)
			{
				systemFontGlyphsSequence.Add(glyphIDs[i]);
				continue;
			}
			ushort glyphId2 = (ushort)(glyphId + deltaGlyphId);
			systemFontGlyphsSequence.Add(glyphId2);
		}
		return systemFontGlyphsSequence;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		base.Read(reader);
		deltaGlyphId = reader.ReadUShort();
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(1);
		base.Write(writer);
		writer.WriteUShort(deltaGlyphId);
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		base.Import(reader);
		deltaGlyphId = reader.ReadUShort();
	}
}
