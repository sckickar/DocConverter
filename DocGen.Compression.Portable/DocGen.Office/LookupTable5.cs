using System.Collections.Generic;

namespace DocGen.Office;

internal class LookupTable5 : LookupTable
{
	private readonly IList<GPOSRecordsCollection> m_records;

	internal LookupTable5(OtfTable table, int flag, int[] offsets)
		: base(table, flag, offsets)
	{
		m_records = new List<GPOSRecordsCollection>();
		ReadSubTables();
	}

	internal override bool ReplaceGlyph(OtfGlyphInfoList glyphList)
	{
		bool result = false;
		if (glyphList.Index >= glyphList.End)
		{
			result = false;
		}
		else if (base.OpenTypeFontTable.GDEFTable.IsSkip(glyphList.Glyphs[glyphList.Index].Index, base.Flag))
		{
			glyphList.Index++;
			result = false;
		}
		else
		{
			GlyphInfoIndex glyphInfoIndex = null;
			foreach (GPOSRecordsCollection record2 in m_records)
			{
				record2.Records.TryGetValue(glyphList.Glyphs[glyphList.Index].Index, out var value);
				if (value == null)
				{
					continue;
				}
				if (glyphInfoIndex == null)
				{
					glyphInfoIndex = new GlyphInfoIndex();
					glyphInfoIndex.Index = glyphList.Index;
					glyphInfoIndex.GlyphInfoList = glyphList;
					do
					{
						glyphInfoIndex.MovePrevious(base.OpenTypeFontTable, base.Flag);
					}
					while (glyphInfoIndex.GlyphInfo != null && record2.Records.ContainsKey(glyphInfoIndex.GlyphInfo.Index));
					if (glyphInfoIndex.GlyphInfo == null)
					{
						break;
					}
				}
				record2.Ligatures.TryGetValue(glyphInfoIndex.GlyphInfo.Index, out var value2);
				if (value2 == null)
				{
					continue;
				}
				int index = value.Index;
				for (int i = 0; i < value2.Count; i++)
				{
					if (value2[i][index] != null)
					{
						GPOSValueRecord gPOSValueRecord = value2[i][index];
						GPOSValueRecord record = value.Record;
						glyphList.Glyphs.Insert(glyphList.Index, new OtfGlyphInfo(glyphList.Glyphs[glyphList.Index], record.XPlacement - gPOSValueRecord.XPlacement, record.YPlacement - gPOSValueRecord.YPlacement));
						glyphList.Text.Insert(glyphList.Index, null);
						result = true;
						break;
					}
				}
				break;
			}
			glyphList.Index++;
		}
		return result;
	}

	internal override void ReadSubTable(int offset)
	{
		base.OpenTypeFontTable.Reader.Seek(offset);
		base.OpenTypeFontTable.Reader.ReadUInt16();
		int offset2 = base.OpenTypeFontTable.Reader.ReadUInt16() + offset;
		int offset3 = base.OpenTypeFontTable.Reader.ReadUInt16() + offset;
		int classCount = base.OpenTypeFontTable.Reader.ReadUInt16();
		int location = base.OpenTypeFontTable.Reader.ReadUInt16() + offset;
		int location2 = base.OpenTypeFontTable.Reader.ReadUInt16() + offset;
		IList<int> list = base.OpenTypeFontTable.ReadFormat(offset2);
		IList<int> list2 = base.OpenTypeFontTable.ReadFormat(offset3);
		IList<GPOSRecord> list3 = ReadMark(base.OpenTypeFontTable, location);
		GPOSRecordsCollection gPOSRecordsCollection = new GPOSRecordsCollection();
		for (int i = 0; i < list.Count; i++)
		{
			gPOSRecordsCollection.Records.Add(list[i], list3[i]);
		}
		IList<IList<GPOSValueRecord[]>> list4 = ReadLigatureArray(base.OpenTypeFontTable, classCount, location2);
		for (int j = 0; j < list2.Count; j++)
		{
			gPOSRecordsCollection.Ligatures.Add(list2[j], list4[j]);
		}
		m_records.Add(gPOSRecordsCollection);
	}
}
