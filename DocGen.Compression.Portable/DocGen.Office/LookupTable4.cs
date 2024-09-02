using System.Collections.Generic;

namespace DocGen.Office;

internal class LookupTable4 : LookupTable
{
	private readonly IList<GPOSRecordsCollection> m_recordCollection;

	internal LookupTable4(OtfTable table, int flag, int[] offset)
		: base(table, flag, offset)
	{
		m_recordCollection = new List<GPOSRecordsCollection>();
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
			foreach (GPOSRecordsCollection item in m_recordCollection)
			{
				item.Records.TryGetValue(glyphList.Glyphs[glyphList.Index].Index, out var value);
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
					while (glyphInfoIndex.GlyphInfo != null && item.Records.ContainsKey(glyphInfoIndex.GlyphInfo.Index));
					if (glyphInfoIndex.GlyphInfo == null)
					{
						break;
					}
				}
				item.Collection.TryGetValue(glyphInfoIndex.GlyphInfo.Index, out var value2);
				if (value2 != null)
				{
					int index = value.Index;
					GPOSValueRecord gPOSValueRecord = value2[index];
					GPOSValueRecord record = value.Record;
					glyphList.Set(glyphList.Index, new OtfGlyphInfo(glyphList.Glyphs[glyphList.Index], -record.XPlacement + gPOSValueRecord.XPlacement, -record.YPlacement + gPOSValueRecord.YPlacement));
					result = true;
					break;
				}
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
		IList<GPOSValueRecord[]> list4 = ReadBaseArray(base.OpenTypeFontTable, classCount, location2);
		for (int j = 0; j < list2.Count; j++)
		{
			gPOSRecordsCollection.Collection.Add(list2[j], list4[j]);
		}
		m_recordCollection.Add(gPOSRecordsCollection);
	}
}
