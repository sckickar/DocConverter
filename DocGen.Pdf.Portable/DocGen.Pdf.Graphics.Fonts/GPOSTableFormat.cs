using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class GPOSTableFormat : LookupTable
{
	private Dictionary<int, Dictionary<int, GPOSTableFormatRecord>> m_records = new Dictionary<int, Dictionary<int, GPOSTableFormatRecord>>();

	private CDEFTable m_cdefTable1;

	private CDEFTable m_cdefTable2;

	private IList<int> m_coverageTable;

	private IDictionary<int, GPOSTableFormatRecord[]> m_gposTableFormatRecord = new Dictionary<int, GPOSTableFormatRecord[]>();

	private GPOSTableType m_format;

	internal GPOSTableFormat(OtfTable table, int flag, int offset, GPOSTableType format)
		: base(table, flag, null)
	{
		m_format = format;
		Initialize(offset);
	}

	internal override bool ReplaceGlyph(OtfGlyphInfoList glyphList)
	{
		if (m_format == GPOSTableType.Format1)
		{
			return ReplaceFormat1Glyphs(glyphList);
		}
		return ReplaceFormat2Glyphs(glyphList);
	}

	private bool ReplaceFormat1Glyphs(OtfGlyphInfoList glyphList)
	{
		bool result = false;
		if (glyphList.Index >= glyphList.End || glyphList.Index < glyphList.Start)
		{
			result = false;
		}
		else
		{
			OtfGlyphInfo otfGlyphInfo = glyphList.Glyphs[glyphList.Index];
			m_records.TryGetValue(otfGlyphInfo.Index, out Dictionary<int, GPOSTableFormatRecord> value);
			if (value != null)
			{
				GlyphInfoIndex glyphInfoIndex = new GlyphInfoIndex();
				glyphInfoIndex.GlyphInfoList = glyphList;
				glyphInfoIndex.Index = glyphList.Index;
				glyphInfoIndex.MoveNext(base.OpenTypeFontTable, base.Flag);
				if (glyphInfoIndex.GlyphInfo != null)
				{
					value.TryGetValue(glyphInfoIndex.GlyphInfo.Index, out var value2);
					if (value2 != null)
					{
						OtfGlyphInfo glyphInfo = glyphInfoIndex.GlyphInfo;
						glyphList.Set(glyphList.Index, new OtfGlyphInfo(otfGlyphInfo, 0, 0, value2.Record1.XAdvance, value2.Record1.YAdvance, 0));
						glyphList.Set(glyphInfoIndex.Index, new OtfGlyphInfo(glyphInfo, 0, 0, value2.Record2.XAdvance, value2.Record2.YAdvance, 0));
						glyphList.Index = glyphInfoIndex.Index;
						result = true;
					}
				}
			}
		}
		return result;
	}

	private bool ReplaceFormat2Glyphs(OtfGlyphInfoList line)
	{
		if (line.Index >= line.End || line.Index < line.Start)
		{
			return false;
		}
		OtfGlyphInfo otfGlyphInfo = line.Glyphs[line.Index];
		if (!m_coverageTable.Contains(otfGlyphInfo.Index))
		{
			return false;
		}
		int value = m_cdefTable1.GetValue(otfGlyphInfo.Index);
		m_gposTableFormatRecord.TryGetValue(value, out GPOSTableFormatRecord[] value2);
		if (value2 == null)
		{
			return false;
		}
		GlyphInfoIndex glyphInfoIndex = new GlyphInfoIndex();
		glyphInfoIndex.GlyphInfoList = line;
		glyphInfoIndex.Index = line.Index;
		glyphInfoIndex.MoveNext(base.OpenTypeFontTable, base.Flag);
		if (glyphInfoIndex.GlyphInfo == null)
		{
			return false;
		}
		OtfGlyphInfo glyphInfo = glyphInfoIndex.GlyphInfo;
		int value3 = m_cdefTable2.GetValue(glyphInfo.Index);
		if (value3 >= value2.Length)
		{
			return false;
		}
		GPOSTableFormatRecord gPOSTableFormatRecord = value2[value3];
		line.Set(line.Index, new OtfGlyphInfo(otfGlyphInfo, 0, 0, gPOSTableFormatRecord.Record1.XAdvance, gPOSTableFormatRecord.Record1.YAdvance, 0));
		line.Set(glyphInfoIndex.Index, new OtfGlyphInfo(glyphInfo, 0, 0, gPOSTableFormatRecord.Record2.XAdvance, gPOSTableFormatRecord.Record2.YAdvance, 0));
		line.Index = glyphInfoIndex.Index;
		return true;
	}

	internal override void ReadSubTable(int offset)
	{
	}

	private void Initialize(int offset)
	{
		switch (m_format)
		{
		case GPOSTableType.Format1:
			ReadFormat1(offset);
			break;
		case GPOSTableType.Format2:
			ReadFormat2(offset);
			break;
		}
	}

	private void ReadFormat1(int offset)
	{
		int offset2 = base.OpenTypeFontTable.Reader.ReadUInt16() + offset;
		int foramt = base.OpenTypeFontTable.Reader.ReadUInt16();
		int foramt2 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int num = base.OpenTypeFontTable.Reader.ReadUInt16();
		int[] array = base.OpenTypeFontTable.ReadUInt16(num, offset);
		IList<int> list = base.OpenTypeFontTable.ReadFormat(offset2);
		for (int i = 0; i < num; i++)
		{
			base.OpenTypeFontTable.Reader.Seek(array[i]);
			Dictionary<int, GPOSTableFormatRecord> dictionary = new Dictionary<int, GPOSTableFormatRecord>();
			m_records.Add(list[i], dictionary);
			int num2 = base.OpenTypeFontTable.Reader.ReadUInt16();
			for (int j = 0; j < num2; j++)
			{
				int key = base.OpenTypeFontTable.Reader.ReadUInt16();
				GPOSTableFormatRecord gPOSTableFormatRecord = new GPOSTableFormatRecord();
				gPOSTableFormatRecord.Record1 = ReadGposValueRecord(base.OpenTypeFontTable, foramt);
				gPOSTableFormatRecord.Record2 = ReadGposValueRecord(base.OpenTypeFontTable, foramt2);
				dictionary[key] = gPOSTableFormatRecord;
			}
		}
	}

	private void ReadFormat2(int offset)
	{
		int offset2 = base.OpenTypeFontTable.Reader.ReadUInt16() + offset;
		int foramt = base.OpenTypeFontTable.Reader.ReadUInt16();
		int foramt2 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int offset3 = base.OpenTypeFontTable.Reader.ReadUInt16() + offset;
		int offset4 = base.OpenTypeFontTable.Reader.ReadUInt16() + offset;
		int num = base.OpenTypeFontTable.Reader.ReadUInt16();
		int num2 = base.OpenTypeFontTable.Reader.ReadUInt16();
		for (int i = 0; i < num; i++)
		{
			GPOSTableFormatRecord[] array = new GPOSTableFormatRecord[num2];
			m_gposTableFormatRecord.Add(i, array);
			for (int j = 0; j < num2; j++)
			{
				GPOSTableFormatRecord gPOSTableFormatRecord = new GPOSTableFormatRecord();
				gPOSTableFormatRecord.Record1 = ReadGposValueRecord(base.OpenTypeFontTable, foramt);
				gPOSTableFormatRecord.Record2 = ReadGposValueRecord(base.OpenTypeFontTable, foramt2);
				array[j] = gPOSTableFormatRecord;
			}
		}
		m_coverageTable = new List<int>(base.OpenTypeFontTable.ReadFormat(offset2));
		m_cdefTable1 = GetTable(base.OpenTypeFontTable.Reader, offset3);
		m_cdefTable2 = GetTable(base.OpenTypeFontTable.Reader, offset4);
	}
}
