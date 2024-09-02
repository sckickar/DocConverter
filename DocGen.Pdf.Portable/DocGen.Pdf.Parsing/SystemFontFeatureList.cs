using System.IO;

namespace DocGen.Pdf.Parsing;

internal class SystemFontFeatureList : SystemFontTableBase
{
	private SystemFontFeatureRecord[] featureRecords;

	private SystemFontFeature[] features;

	public SystemFontFeatureList(SystemFontOpenTypeFontSourceBase fontFile)
		: base(fontFile)
	{
	}

	private SystemFontFeature ReadFeature(SystemFontOpenTypeFontReader reader, SystemFontFeatureRecord record)
	{
		SystemFontFeatureInfo systemFontFeatureInfo = SystemFontFeatureInfo.CreateFeatureInfo(record.FeatureTag);
		if (systemFontFeatureInfo == null)
		{
			return null;
		}
		reader.BeginReadingBlock();
		reader.Seek(base.Offset + record.FeatureOffset, SeekOrigin.Begin);
		SystemFontFeature systemFontFeature = new SystemFontFeature(base.FontSource, systemFontFeatureInfo);
		systemFontFeature.Read(reader);
		reader.EndReadingBlock();
		return systemFontFeature;
	}

	public SystemFontFeature GetFeature(int index)
	{
		if (features[index] == null)
		{
			features[index] = ReadFeature(base.Reader, featureRecords[index]);
		}
		return features[index];
	}

	public override void Read(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		featureRecords = new SystemFontFeatureRecord[num];
		features = new SystemFontFeature[num];
		for (int i = 0; i < num; i++)
		{
			featureRecords[i] = new SystemFontFeatureRecord();
			featureRecords[i].Read(reader);
		}
	}

	internal override void Write(SystemFontFontWriter writer)
	{
		writer.WriteUShort((ushort)featureRecords.Length);
		for (int i = 0; i < featureRecords.Length; i++)
		{
			SystemFontFeature feature = GetFeature(i);
			if (feature == null)
			{
				writer.WriteULong(SystemFontTags.NULL_TAG);
			}
			else
			{
				feature.Write(writer);
			}
		}
	}

	internal override void Import(SystemFontOpenTypeFontReader reader)
	{
		ushort num = reader.ReadUShort();
		features = new SystemFontFeature[num];
		for (int i = 0; i < num; i++)
		{
			uint num2 = reader.ReadULong();
			if (num2 != SystemFontTags.NULL_TAG)
			{
				SystemFontFeature systemFontFeature = new SystemFontFeature(base.FontSource, SystemFontFeatureInfo.CreateFeatureInfo(num2));
				systemFontFeature.Import(reader);
				features[i] = systemFontFeature;
			}
		}
	}
}
