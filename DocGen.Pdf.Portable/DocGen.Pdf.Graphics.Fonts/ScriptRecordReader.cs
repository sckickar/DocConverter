using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class ScriptRecordReader
{
	private OtfTable m_table;

	private IList<ScriptRecord> m_records;

	internal IList<ScriptRecord> Records
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

	internal ScriptRecordReader(OtfTable table, int offset)
	{
		m_table = table;
		m_records = new List<ScriptRecord>();
		table.Reader.Seek(offset);
		FeatureTag[] array = table.ReadFeatureTag(offset);
		foreach (FeatureTag featureTag in array)
		{
			ReadScriptRecord(featureTag);
		}
	}

	private void ReadScriptRecord(FeatureTag featureTag)
	{
		m_table.Reader.Seek(featureTag.Offset);
		int num = m_table.Reader.ReadUInt16();
		if (num > 0)
		{
			num += featureTag.Offset;
		}
		FeatureTag[] array = m_table.ReadFeatureTag(featureTag.Offset);
		ScriptRecord item = default(ScriptRecord);
		item.ScriptTag = featureTag.TagName;
		item.LanguageRecord = new LanguageRecord[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			item.LanguageRecord[i] = ReadLanguageRecord(array[i]);
		}
		if (num > 0)
		{
			FeatureTag featureTag2 = default(FeatureTag);
			featureTag2.TagName = "";
			featureTag2.Offset = num;
			item.Language = ReadLanguageRecord(featureTag2);
		}
		m_records.Add(item);
	}

	private LanguageRecord ReadLanguageRecord(FeatureTag featureTag)
	{
		LanguageRecord languageRecord = new LanguageRecord();
		m_table.Reader.Seek(featureTag.Offset + 2);
		m_table.Reader.ReadUInt16();
		int size = m_table.Reader.ReadUInt16();
		languageRecord.Records = m_table.ReadUInt32(size);
		languageRecord.LanguageTag = featureTag.TagName;
		return languageRecord;
	}
}
