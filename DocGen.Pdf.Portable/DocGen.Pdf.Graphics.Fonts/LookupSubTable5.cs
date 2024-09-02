using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class LookupSubTable5 : LookupTable
{
	protected internal IList<BaseTable> m_records;

	protected internal LookupSubTable5(OtfTable table, int flag, int[] offsets)
		: base(table, flag, offsets)
	{
		m_records = new List<BaseTable>();
		ReadSubTables();
	}

	internal override bool ReplaceGlyph(OtfGlyphInfoList glyphList)
	{
		bool flag = false;
		int start = glyphList.Start;
		int end = glyphList.End;
		int index = glyphList.Index;
		int i;
		foreach (BaseTable record in m_records)
		{
			SubsetTable table = record.GetTable(glyphList);
			if (table == null)
			{
				continue;
			}
			int end2 = glyphList.End;
			LookupSubTableRecord[] lookupRecord = table.LookupRecord;
			GlyphInfoIndex glyphInfoIndex = new GlyphInfoIndex
			{
				GlyphInfoList = glyphList
			};
			LookupSubTableRecord[] array = lookupRecord;
			for (i = 0; i < array.Length; i++)
			{
				LookupSubTableRecord lookupSubTableRecord = array[i];
				glyphInfoIndex.Index = index;
				for (int j = 0; j < lookupSubTableRecord.Index; j++)
				{
					glyphInfoIndex.MoveNext(base.OpenTypeFontTable, base.Flag);
				}
				glyphList.Index = glyphInfoIndex.Index;
				LookupTable lookupTable = null;
				if (lookupSubTableRecord.LookupIndex >= 0 || lookupSubTableRecord.LookupIndex < base.OpenTypeFontTable.LookupList.Count)
				{
					lookupTable = base.OpenTypeFontTable.LookupList[lookupSubTableRecord.LookupIndex];
				}
				flag = lookupTable.ReplaceGlyph(glyphList) || flag;
			}
			glyphList.Index = glyphList.End;
			glyphList.Start = start;
			int num = end2 - glyphList.End;
			glyphList.End = end - num;
			return flag;
		}
		i = glyphList.Index + 1;
		glyphList.Index = i;
		return flag;
	}

	internal override void ReadSubTable(int offset)
	{
		base.OpenTypeFontTable.Reader.Seek(offset);
		int num = base.OpenTypeFontTable.Reader.ReadInt16();
		switch (num)
		{
		case 1:
			ReadSubTableFormat1(offset);
			break;
		case 2:
			ReadSubTableFormat2(offset);
			break;
		case 3:
			ReadSubTableFormat3(offset);
			break;
		default:
			throw new ArgumentException("Bad Format: " + num);
		}
	}

	internal virtual void ReadSubTableFormat1(int offset)
	{
		IDictionary<int, IList<SubsetTable>> dictionary = new Dictionary<int, IList<SubsetTable>>();
		int num = base.OpenTypeFontTable.Reader.ReadUInt16();
		int num2 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int[] array = base.OpenTypeFontTable.ReadUInt16(num2, offset);
		IList<int> list = base.OpenTypeFontTable.ReadFormat(offset + num);
		for (int i = 0; i < num2; i++)
		{
			base.OpenTypeFontTable.Reader.Seek(array[i]);
			int num3 = base.OpenTypeFontTable.Reader.ReadUInt16();
			int[] array2 = base.OpenTypeFontTable.ReadUInt16(num3, array[i]);
			IList<SubsetTable> list2 = new List<SubsetTable>(num3);
			for (int j = 0; j < num3; j++)
			{
				base.OpenTypeFontTable.Reader.Seek(array2[j]);
				int num4 = base.OpenTypeFontTable.Reader.ReadUInt16();
				int substCount = base.OpenTypeFontTable.Reader.ReadUInt16();
				int[] glyphs = base.OpenTypeFontTable.ReadUInt32(num4 - 1);
				LookupSubTableRecord[] records = base.OpenTypeFontTable.ReadSubstLookupRecords(substCount);
				list2.Add(new SubsetTableFormat(glyphs, records));
			}
			dictionary.Add(list[i], list2);
		}
		m_records.Add(new LookupSubTable5Format(base.OpenTypeFontTable, base.Flag, dictionary, LookupSubTableFormat.Format1));
	}

	internal virtual void ReadSubTableFormat2(int offset)
	{
		int num = base.OpenTypeFontTable.Reader.ReadUInt16();
		int num2 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int num3 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int[] array = base.OpenTypeFontTable.ReadUInt16(num3, offset);
		ICollection<int> glyphIds = new List<int>(base.OpenTypeFontTable.ReadFormat(offset + num));
		CDEFTable table = GetTable(base.OpenTypeFontTable.Reader, offset + num2);
		LookupSubTable5Format lookupSubTable5Format = new LookupSubTable5Format(base.OpenTypeFontTable, base.Flag, glyphIds, table, LookupSubTableFormat.Format2);
		IList<IList<SubsetTable>> list = new List<IList<SubsetTable>>(num3);
		for (int i = 0; i < num3; i++)
		{
			IList<SubsetTable> list2 = null;
			if (array[i] != 0)
			{
				base.OpenTypeFontTable.Reader.Seek(array[i]);
				int num4 = base.OpenTypeFontTable.Reader.ReadUInt16();
				int[] array2 = base.OpenTypeFontTable.ReadUInt16(num4, array[i]);
				list2 = new List<SubsetTable>(num4);
				for (int j = 0; j < num4; j++)
				{
					base.OpenTypeFontTable.Reader.Seek(array2[j]);
					int num5 = base.OpenTypeFontTable.Reader.ReadUInt16();
					int substCount = base.OpenTypeFontTable.Reader.ReadUInt16();
					int[] glyphs = base.OpenTypeFontTable.ReadUInt32(num5 - 1);
					LookupSubTableRecord[] records = base.OpenTypeFontTable.ReadSubstLookupRecords(substCount);
					SubsetTable item = new SubsetTableFormat(lookupSubTable5Format, glyphs, records);
					list2.Add(item);
				}
			}
			list.Add(list2);
		}
		lookupSubTable5Format.SubsetTables = list;
		m_records.Add(lookupSubTable5Format);
	}

	internal virtual void ReadSubTableFormat3(int offset)
	{
		int num = base.OpenTypeFontTable.Reader.ReadUInt16();
		int substCount = base.OpenTypeFontTable.Reader.ReadUInt16();
		int[] offsets = base.OpenTypeFontTable.ReadUInt16(num, offset);
		LookupSubTableRecord[] records = base.OpenTypeFontTable.ReadSubstLookupRecords(substCount);
		IList<ICollection<int>> list = new List<ICollection<int>>(num);
		base.OpenTypeFontTable.ReadFormatGlyphIds(offsets, list);
		SubsetTableFormat subsetTable = new SubsetTableFormat(list, records);
		m_records.Add(new LookupSubTable5Format(base.OpenTypeFontTable, base.Flag, subsetTable, LookupSubTableFormat.Format3));
	}
}
