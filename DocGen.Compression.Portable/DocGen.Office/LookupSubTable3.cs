using System.Collections.Generic;

namespace DocGen.Office;

internal class LookupSubTable3 : LookupTable
{
	private IDictionary<int, int[]> m_records;

	internal LookupSubTable3(OtfTable table, int flag, int[] offsets)
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
				m_records.TryGetValue(otfGlyphInfo.Index, out var value);
				if (value != null)
				{
					glyphList.CombineAlternateGlyphs(base.OpenTypeFontTable, value[0]);
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
		base.OpenTypeFontTable.Reader.ReadInt16();
		int num = base.OpenTypeFontTable.Reader.ReadUInt16();
		int num2 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int[][] array = new int[num2][];
		int[] array2 = base.OpenTypeFontTable.ReadUInt16(num2, offset);
		for (int i = 0; i < num2; i++)
		{
			base.OpenTypeFontTable.Reader.Seek(array2[i]);
			int size = base.OpenTypeFontTable.Reader.ReadUInt16();
			array[i] = base.OpenTypeFontTable.ReadUInt32(size);
		}
		IList<int> list = base.OpenTypeFontTable.ReadFormat(offset + num);
		for (int j = 0; j < num2; j++)
		{
			m_records.Add(list[j], array[j]);
		}
	}

	internal override bool IsSubstitute(int index)
	{
		return m_records.ContainsKey(index);
	}
}
