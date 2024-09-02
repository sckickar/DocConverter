using System;
using System.Collections.Generic;

namespace DocGen.Office;

internal class LookupSubTable1 : LookupTable
{
	private Dictionary<int, int> m_records;

	internal LookupSubTable1(OtfTable table, int flag, int[] offsets)
		: base(table, flag, offsets)
	{
		m_records = new Dictionary<int, int>();
		ReadSubTables();
	}

	internal override bool ReplaceGlyph(OtfGlyphInfoList glyphList)
	{
		bool result = false;
		if (glyphList.Index >= glyphList.End)
		{
			result = false;
		}
		else
		{
			OtfGlyphInfo otfGlyphInfo = glyphList.Glyphs[glyphList.Index];
			if (!base.OpenTypeFontTable.GDEFTable.IsSkip(otfGlyphInfo.Index, base.Flag))
			{
				m_records.TryGetValue(otfGlyphInfo.Index, out var value);
				if (value != 0)
				{
					glyphList.CombineAlternateGlyphs(base.OpenTypeFontTable, value);
					result = true;
				}
			}
			glyphList.Index++;
		}
		return result;
	}

	internal override void ReadSubTable(int offset)
	{
		base.OpenTypeFontTable.Reader.Seek(offset);
		switch (base.OpenTypeFontTable.Reader.ReadInt16())
		{
		case 1:
		{
			int num3 = base.OpenTypeFontTable.Reader.ReadUInt16();
			int num4 = base.OpenTypeFontTable.Reader.ReadInt16();
			{
				foreach (int item in base.OpenTypeFontTable.ReadFormat(offset + num3))
				{
					int value = item + num4;
					m_records.Add(item, value);
				}
				break;
			}
		}
		case 2:
		{
			int num = base.OpenTypeFontTable.Reader.ReadUInt16();
			int num2 = base.OpenTypeFontTable.Reader.ReadUInt16();
			int[] array = new int[num2];
			for (int i = 0; i < num2; i++)
			{
				array[i] = base.OpenTypeFontTable.Reader.ReadUInt16();
			}
			IList<int> list = base.OpenTypeFontTable.ReadFormat(offset + num);
			for (int j = 0; j < num2; j++)
			{
				m_records.Add(list[j], array[j]);
			}
			break;
		}
		default:
			throw new Exception("Bad format");
		}
	}

	internal override bool IsSubstitute(int index)
	{
		return m_records.ContainsKey(index);
	}
}
