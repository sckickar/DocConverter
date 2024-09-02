namespace DocGen.Pdf.Parsing;

internal class SystemFontFeature : SystemFontTableBase
{
	private ushort[] lookupListIndices;

	public SystemFontFeatureInfo FeatureInfo { get; private set; }

	public ushort[] LookupsListIndices => lookupListIndices;

	public SystemFontFeature(SystemFontOpenTypeFontSourceBase fontFile, SystemFontFeatureInfo featureInfo)
		: base(fontFile)
	{
		FeatureInfo = featureInfo;
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		reader.ReadUShort();
		ushort num = reader.ReadUShort();
		lookupListIndices = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			lookupListIndices[i] = reader.ReadUShort();
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteULong(FeatureInfo.Tag);
		writer.WriteUShort((ushort)lookupListIndices.Length);
		for (int i = 0; i < lookupListIndices.Length; i++)
		{
			writer.WriteUShort(lookupListIndices[i]);
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		lookupListIndices = new ushort[num];
		for (int i = 0; i < num; i++)
		{
			lookupListIndices[i] = reader.ReadUShort();
		}
	}

	public override string ToString()
	{
		if (FeatureInfo != null)
		{
			return SystemFontTags.GetStringFromTag(FeatureInfo.Tag);
		}
		return "Not supported";
	}
}
