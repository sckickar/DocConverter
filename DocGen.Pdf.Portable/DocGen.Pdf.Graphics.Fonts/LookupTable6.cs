using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class LookupTable6 : LookupTable
{
	private readonly IList<GPOSRecordsCollection> m_records;

	internal LookupTable6(OtfTable table, int flag, int[] offsets)
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
				record2.Records.TryGetValue(glyphList.Glyphs[glyphList.Index].Index, out GPOSRecord value);
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
						int index = glyphInfoIndex.Index;
						bool flag = false;
						glyphInfoIndex.MovePrevious(base.OpenTypeFontTable, base.Flag);
						if (glyphInfoIndex.Index != -1)
						{
							for (int i = glyphInfoIndex.Index; i < index; i++)
							{
								base.OpenTypeFontTable.GDEFTable.GlyphCdefTable.Records.TryGetValue(glyphList.Glyphs[i].Index, out var value2);
								if (value2 == 1)
								{
									flag = true;
									break;
								}
							}
						}
						if (flag)
						{
							glyphInfoIndex.GlyphInfo = null;
							break;
						}
					}
					while (glyphInfoIndex.GlyphInfo != null && !record2.Collection.ContainsKey(glyphInfoIndex.GlyphInfo.Index));
					if (glyphInfoIndex.GlyphInfo == null)
					{
						glyphInfoIndex = null;
						continue;
					}
				}
				if (glyphInfoIndex != null && glyphInfoIndex.GlyphInfo != null)
				{
					record2.Collection.TryGetValue(glyphInfoIndex.GlyphInfo.Index, out GPOSValueRecord[] value3);
					if (value3 != null)
					{
						int index2 = value.Index;
						GPOSValueRecord gPOSValueRecord = value3[index2];
						GPOSValueRecord record = value.Record;
						glyphList.Set(glyphList.Index, new OtfGlyphInfo(glyphList.Glyphs[glyphList.Index], -record.XPlacement + gPOSValueRecord.XPlacement, -record.YPlacement + gPOSValueRecord.YPlacement, 0, 0, glyphInfoIndex.Index - glyphList.Index));
						result = true;
						break;
					}
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
		m_records.Add(gPOSRecordsCollection);
	}
}
