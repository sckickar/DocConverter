namespace DocGen.Pdf.Parsing;

internal class SystemFontLigature : SystemFontTableBase
{
	private ushort[] componentGlyphIds;

	public ushort LigatureGlyphId { get; private set; }

	public int Length => componentGlyphIds.Length;

	public SystemFontLigature(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public bool IsMatch(SystemFontGlyphsSequence glyphIDs, int startIndex)
	{
		for (int i = 0; i < componentGlyphIds.Length; i++)
		{
			if (i + startIndex >= glyphIDs.Count || componentGlyphIds[i] != glyphIDs[i + startIndex].GlyphId)
			{
				return false;
			}
		}
		return true;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		LigatureGlyphId = reader.ReadUShort();
		ushort num = (ushort)(reader.ReadUShort() - 1);
		componentGlyphIds = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			componentGlyphIds[i] = reader.ReadUShort();
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort(LigatureGlyphId);
		writer.WriteUShort((ushort)(componentGlyphIds.Length + 1));
		for (int i = 0; i < componentGlyphIds.Length; i++)
		{
			writer.WriteUShort(componentGlyphIds[i]);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		Read(reader);
	}
}
