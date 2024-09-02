using System.Collections.Generic;

namespace DocGen.Office;

internal class LookupSubTable4 : LookupTable
{
	private IDictionary<int, IList<int[]>> m_records;

	internal LookupSubTable4(OtfTable table, int flag, int[] offsets)
		: base(table, flag, offsets)
	{
		m_records = new Dictionary<int, IList<int[]>>();
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
			bool flag = false;
			if (m_records.ContainsKey(otfGlyphInfo.Index) && !base.OpenTypeFontTable.GDEFTable.IsSkip(otfGlyphInfo.Index, base.Flag))
			{
				GlyphInfoIndex glyphInfoIndex = new GlyphInfoIndex();
				glyphInfoIndex.GlyphInfoList = glyphList;
				m_records.TryGetValue(otfGlyphInfo.Index, out var value);
				foreach (int[] item in value)
				{
					flag = true;
					glyphInfoIndex.Index = glyphList.Index;
					for (int i = 1; i < item.Length; i++)
					{
						glyphInfoIndex.MoveNext(base.OpenTypeFontTable, base.Flag);
						if (glyphInfoIndex.GlyphInfo == null || glyphInfoIndex.GlyphInfo.Index != item[i])
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						glyphList.CombineAlternateGlyphs(base.OpenTypeFontTable, base.Flag, item.Length - 1, item[0]);
						break;
					}
				}
			}
			if (flag)
			{
				result = true;
			}
			glyphList.Index++;
		}
		return result;
	}

	internal override void ReadSubTable(int offset)
	{
		base.OpenTypeFontTable.Reader.Seek(offset);
		base.OpenTypeFontTable.Reader.ReadInt16();
		int offset2 = base.OpenTypeFontTable.Reader.ReadUInt16() + offset;
		int num = base.OpenTypeFontTable.Reader.ReadUInt16();
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = base.OpenTypeFontTable.Reader.ReadUInt16() + offset;
		}
		IList<int> list = base.OpenTypeFontTable.ReadFormat(offset2);
		for (int j = 0; j < num; j++)
		{
			base.OpenTypeFontTable.Reader.Seek(array[j]);
			int num2 = base.OpenTypeFontTable.Reader.ReadUInt16();
			int[] array2 = new int[num2];
			for (int k = 0; k < num2; k++)
			{
				array2[k] = base.OpenTypeFontTable.Reader.ReadUInt16() + array[j];
			}
			IList<int[]> list2 = new List<int[]>(num2);
			for (int l = 0; l < num2; l++)
			{
				base.OpenTypeFontTable.Reader.Seek(array2[l]);
				int num3 = base.OpenTypeFontTable.Reader.ReadUInt16();
				int num4 = base.OpenTypeFontTable.Reader.ReadUInt16();
				int[] array3 = new int[num4];
				array3[0] = num3;
				for (int m = 1; m < num4; m++)
				{
					array3[m] = base.OpenTypeFontTable.Reader.ReadUInt16();
				}
				list2.Add(array3);
			}
			if (m_records.ContainsKey(list[j]))
			{
				m_records[list[j]] = list2;
			}
			else
			{
				m_records.Add(list[j], list2);
			}
		}
	}
}
