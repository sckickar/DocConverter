using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class FeatureRecordReader
{
	private IList<FeatureRecord> m_records;

	internal IList<FeatureRecord> Records
	{
		get
		{
			return m_records;
		}
		set
		{
			m_records = value;
		}
	}

	internal FeatureRecordReader(OtfTable table, int offset)
	{
		m_records = new List<FeatureRecord>();
		table.Reader.Seek(offset);
		FeatureTag[] array = table.ReadFeatureTag(offset);
		for (int i = 0; i < array.Length; i++)
		{
			FeatureTag featureTag = array[i];
			table.Reader.Seek(featureTag.Offset + 2);
			int size = table.Reader.ReadUInt16();
			FeatureRecord item = new FeatureRecord
			{
				Tag = featureTag.TagName,
				Indexes = table.ReadUInt32(size)
			};
			m_records.Add(item);
		}
	}
}
