namespace DocGen.Pdf.Parsing;

internal class SystemFontSubstLookupRecord : SystemFontTableBase
{
	private ushort lookupIndex;

	private SystemFontLookup lookup;

	public ushort SequenceIndex { get; private set; }

	public SystemFontLookup Lookup
	{
		get
		{
			if (lookup == null)
			{
				lookup = base.FontSource.GetLookup(lookupIndex);
			}
			return lookup;
		}
	}

	public SystemFontSubstLookupRecord(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		SequenceIndex = reader.ReadUShort();
		lookupIndex = reader.ReadUShort();
	}
}
