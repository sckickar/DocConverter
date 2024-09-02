using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class LookupSubTable2 : LookupTable
{
	private IDictionary<int, int[]> m_records;

	internal LookupSubTable2(OtfTable table, int flag, int[] offsets)
		: base(table, flag, offsets)
	{
		m_records = new Dictionary<int, int[]>();
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
				m_records.TryGetValue(otfGlyphInfo.Index, out int[] value);
				if (value != null && value.Length != 0)
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
		int num = base.OpenTypeFontTable.Reader.ReadInt16();
		if (num == 1)
		{
			int num2 = base.OpenTypeFontTable.Reader.ReadUInt16();
			int num3 = base.OpenTypeFontTable.Reader.ReadUInt16();
			int[] array = base.OpenTypeFontTable.ReadUInt16(num3, offset);
			IList<int> list = base.OpenTypeFontTable.ReadFormat(offset + num2);
			for (int i = 0; i < num3; i++)
			{
				base.OpenTypeFontTable.Reader.Seek(array[i]);
				int size = base.OpenTypeFontTable.Reader.ReadUInt16();
				m_records.Add(list[i], base.OpenTypeFontTable.ReadUInt32(size));
			}
			return;
		}
		throw new Exception("Bad Format: " + num);
	}

	internal override bool IsSubstitute(int index)
	{
		return m_records.ContainsKey(index);
	}
}
