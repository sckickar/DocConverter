using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class LookupSubTable6 : LookupSubTable5
{
	internal LookupSubTable6(OtfTable table, int flag, int[] offsets)
		: base(table, flag, offsets)
	{
	}

	internal override void ReadSubTableFormat1(int offset)
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
				int size = base.OpenTypeFontTable.Reader.ReadUInt16();
				int[] btGlyphs = base.OpenTypeFontTable.ReadUInt32(size);
				int num4 = base.OpenTypeFontTable.Reader.ReadUInt16();
				int[] glyphs = base.OpenTypeFontTable.ReadUInt32(num4 - 1);
				int size2 = base.OpenTypeFontTable.Reader.ReadUInt16();
				int[] lookupGlyphs = base.OpenTypeFontTable.ReadUInt32(size2);
				int substCount = base.OpenTypeFontTable.Reader.ReadUInt16();
				LookupSubTableRecord[] records = base.OpenTypeFontTable.ReadSubstLookupRecords(substCount);
				list2.Add(new SubsetTableFormat(btGlyphs, glyphs, lookupGlyphs, records));
			}
			dictionary.Add(list[i], list2);
		}
		m_records.Add(new LookupSubTable6Format(base.OpenTypeFontTable, base.Flag, dictionary, LookupSubTableFormat.Format1));
	}

	internal override void ReadSubTableFormat2(int offset)
	{
		int num = base.OpenTypeFontTable.Reader.ReadUInt16();
		int num2 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int num3 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int num4 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int num5 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int[] array = base.OpenTypeFontTable.ReadUInt16(num5, offset);
		ICollection<int> glyphs = new List<int>(base.OpenTypeFontTable.ReadFormat(offset + num));
		CDEFTable table = GetTable(base.OpenTypeFontTable.Reader, offset + num2);
		CDEFTable table2 = GetTable(base.OpenTypeFontTable.Reader, offset + num3);
		CDEFTable table3 = GetTable(base.OpenTypeFontTable.Reader, offset + num4);
		LookupSubTable6Format lookupSubTable6Format = new LookupSubTable6Format(base.OpenTypeFontTable, base.Flag, glyphs, table, table2, table3, LookupSubTableFormat.Format2);
		IList<IList<SubsetTable>> list = new List<IList<SubsetTable>>(num5);
		for (int i = 0; i < num5; i++)
		{
			IList<SubsetTable> list2 = null;
			if (array[i] != 0)
			{
				base.OpenTypeFontTable.Reader.Seek(array[i]);
				int num6 = base.OpenTypeFontTable.Reader.ReadUInt16();
				int[] array2 = base.OpenTypeFontTable.ReadUInt16(num6, array[i]);
				list2 = new List<SubsetTable>(num6);
				for (int j = 0; j < num6; j++)
				{
					base.OpenTypeFontTable.Reader.Seek(array2[j]);
					int size = base.OpenTypeFontTable.Reader.ReadUInt16();
					int[] backtrackClassIds = base.OpenTypeFontTable.ReadUInt32(size);
					int num7 = base.OpenTypeFontTable.Reader.ReadUInt16();
					int[] inputClassIds = base.OpenTypeFontTable.ReadUInt32(num7 - 1);
					int size2 = base.OpenTypeFontTable.Reader.ReadUInt16();
					int[] lookAheadClassIds = base.OpenTypeFontTable.ReadUInt32(size2);
					int substCount = base.OpenTypeFontTable.Reader.ReadUInt16();
					LookupSubTableRecord[] substLookupRecords = base.OpenTypeFontTable.ReadSubstLookupRecords(substCount);
					SubsetTableFormat item = new SubsetTableFormat(lookupSubTable6Format, backtrackClassIds, inputClassIds, lookAheadClassIds, substLookupRecords);
					list2.Add(item);
				}
			}
			list.Add(list2);
		}
		lookupSubTable6Format.SubSetTables = list;
		m_records.Add(lookupSubTable6Format);
	}

	internal override void ReadSubTableFormat3(int offset)
	{
		int num = base.OpenTypeFontTable.Reader.ReadUInt16();
		int[] offsets = base.OpenTypeFontTable.ReadUInt16(num, offset);
		int num2 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int[] offsets2 = base.OpenTypeFontTable.ReadUInt16(num2, offset);
		int num3 = base.OpenTypeFontTable.Reader.ReadUInt16();
		int[] offsets3 = base.OpenTypeFontTable.ReadUInt16(num3, offset);
		int substCount = base.OpenTypeFontTable.Reader.ReadUInt16();
		LookupSubTableRecord[] records = base.OpenTypeFontTable.ReadSubstLookupRecords(substCount);
		IList<ICollection<int>> list = new List<ICollection<int>>(num);
		base.OpenTypeFontTable.ReadFormatGlyphIds(offsets, list);
		IList<ICollection<int>> list2 = new List<ICollection<int>>(num2);
		base.OpenTypeFontTable.ReadFormatGlyphIds(offsets2, list2);
		IList<ICollection<int>> list3 = new List<ICollection<int>>(num3);
		base.OpenTypeFontTable.ReadFormatGlyphIds(offsets3, list3);
		SubsetTableFormat subsetFormat = new SubsetTableFormat(list, list2, list3, records);
		m_records.Add(new LookupSubTable6Format(base.OpenTypeFontTable, base.Flag, subsetFormat, LookupSubTableFormat.Format3));
	}
}
