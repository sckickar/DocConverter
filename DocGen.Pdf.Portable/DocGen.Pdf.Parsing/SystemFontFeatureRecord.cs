namespace DocGen.Pdf.Parsing;

internal class SystemFontFeatureRecord
{
	public uint FeatureTag { get; private set; }

	public ushort FeatureOffset { get; private set; }

	public void Read(SystemFontOpenTypeFontReader reader)
	{
		FeatureTag = reader.ReadULong();
		FeatureOffset = reader.ReadUShort();
	}

	public override string ToString()
	{
		return SystemFontTags.GetStringFromTag(FeatureTag);
	}
}
