namespace DocGen.Pdf.Parsing;

internal class SystemFontSubRule : SystemFontTableBase
{
	private ushort[] input;

	private SystemFontSubstLookupRecord[] substitutions;

	public SystemFontSubRule(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public bool IsMatch(SystemFontGlyphsSequence glyphIDs, int startIndex)
	{
		for (int i = 0; i < input.Length; i++)
		{
			if (glyphIDs[startIndex + i + 1].GlyphId != input[i])
			{
				return false;
			}
		}
		return true;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		ushort num2 = reader.ReadUShort();
		input = new ushort[num];
		substitutions = new SystemFontSubstLookupRecord[num2];
		for (int i = 0; i < num; i++)
		{
			input[i] = reader.ReadUShort();
		}
		for (int j = 0; j < num2; j++)
		{
			substitutions[j] = new SystemFontSubstLookupRecord(base.FontSource);
			substitutions[j].Read(reader);
		}
	}
}
