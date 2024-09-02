namespace DocGen.Pdf.Parsing;

internal class SystemFontIndexToLocation : SystemFontTrueTypeTableBase
{
	private uint[] offsets;

	internal override uint Tag => SystemFontTags.LOCA_TABLE;

	public SystemFontIndexToLocation(SystemFontOpenTypeFontSource fontFile)
		: base(fontFile)
	{
	}

	public long GetOffset(ushort index)
	{
		if (offsets == null || index >= offsets.Length || (index < offsets.Length - 1 && offsets[index + 1] == offsets[index]))
		{
			return -1L;
		}
		return offsets[index];
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		offsets = new uint[base.FontSource.GlyphCount + 1];
		for (int i = 0; i < offsets.Length; i++)
		{
			if (base.FontSource.Head.IndexToLocFormat == 0)
			{
				offsets[i] = (uint)(2 * reader.ReadUShort());
			}
			else
			{
				offsets[i] = reader.ReadULong();
			}
		}
	}
}
