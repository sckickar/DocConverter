using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class LookupTable1 : LookupTable
{
	private Dictionary<int, GPOSValueRecord> m_records = new Dictionary<int, GPOSValueRecord>();

	internal LookupTable1(OtfTable table, int flag, int[] offsets)
		: base(table, flag, offsets)
	{
		ReadSubTables();
	}

	internal override bool ReplaceGlyph(OtfGlyphInfoList glyphList)
	{
		bool result = false;
		if (glyphList.Index >= glyphList.End)
		{
			result = false;
		}
		if (base.OpenTypeFontTable.GDEFTable.IsSkip(glyphList.Glyphs[glyphList.Index].Index, base.Flag))
		{
			glyphList.Index++;
			result = false;
		}
		int index = glyphList.Glyphs[glyphList.Index].Index;
		GPOSValueRecord value = null;
		m_records.TryGetValue(index, out value);
		if (value != null)
		{
			OtfGlyphInfo glyph = new OtfGlyphInfo(glyphList.Glyphs[glyphList.Index]);
			glyphList.Set(glyphList.Index, glyph);
			result = true;
		}
		glyphList.Index++;
		return result;
	}

	internal override void ReadSubTable(int offset)
	{
		base.OpenTypeFontTable.Reader.Seek(offset);
		int num = base.OpenTypeFontTable.Reader.ReadInt16();
		int num2 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int foramt = base.OpenTypeFontTable.Reader.ReadUInt16();
		if (num == 1)
		{
			GPOSValueRecord value = ReadGposValueRecord(base.OpenTypeFontTable, foramt);
			foreach (int item2 in base.OpenTypeFontTable.ReadFormat(offset + num2))
			{
				m_records.Add(item2, value);
			}
		}
		if (num == 2)
		{
			int num3 = base.OpenTypeFontTable.Reader.ReadUInt16();
			IList<GPOSValueRecord> list = new List<GPOSValueRecord>();
			for (int i = 0; i < num3; i++)
			{
				GPOSValueRecord item = ReadGposValueRecord(base.OpenTypeFontTable, foramt);
				list.Add(item);
			}
			IList<int> list2 = base.OpenTypeFontTable.ReadFormat(offset + num2);
			for (int j = 0; j < list2.Count; j++)
			{
				m_records.Add(list2[j], list[j]);
			}
		}
	}
}
