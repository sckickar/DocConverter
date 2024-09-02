using System.Collections.Generic;

namespace DocGen.Office;

internal class LookupSubTable6Format : LookupSubTable
{
	private IDictionary<int, IList<SubsetTable>> m_records;

	private LookupSubTableFormat m_format;

	private ICollection<int> m_glyphs;

	private IList<IList<SubsetTable>> m_subSetTables;

	private CDEFTable m_btCdefTable;

	private CDEFTable m_cdefTable;

	private CDEFTable m_lookupCdefTable;

	private SubsetTable m_subsetTable;

	internal IList<IList<SubsetTable>> SubSetTables
	{
		get
		{
			return m_subSetTables;
		}
		set
		{
			m_subSetTables = value;
		}
	}

	internal CDEFTable CDEFTable
	{
		get
		{
			return m_cdefTable;
		}
		set
		{
			m_cdefTable = value;
		}
	}

	internal CDEFTable BtCDEFTable
	{
		get
		{
			return m_btCdefTable;
		}
		set
		{
			m_btCdefTable = value;
		}
	}

	internal CDEFTable LookupCDEFTable
	{
		get
		{
			return m_lookupCdefTable;
		}
		set
		{
			m_lookupCdefTable = value;
		}
	}

	internal LookupSubTable6Format(OtfTable table, int flag, IDictionary<int, IList<SubsetTable>> substMap, LookupSubTableFormat format)
		: base(table, flag)
	{
		m_records = substMap;
		m_format = format;
	}

	internal LookupSubTable6Format(OtfTable table, int flag, ICollection<int> glyphs, CDEFTable btcdef, CDEFTable cdef, CDEFTable lookupcdef, LookupSubTableFormat format)
		: base(table, flag)
	{
		m_glyphs = glyphs;
		m_btCdefTable = btcdef;
		m_cdefTable = cdef;
		m_lookupCdefTable = lookupcdef;
		m_format = format;
	}

	internal LookupSubTable6Format(OtfTable table, int flag, SubsetTableFormat subsetFormat, LookupSubTableFormat format)
		: base(table, flag)
	{
		m_subsetTable = subsetFormat;
		m_format = format;
	}

	internal override IList<SubsetTable> GetSubsetTables(int index)
	{
		switch (m_format)
		{
		case LookupSubTableFormat.Format1:
			if (m_records.ContainsKey(index) && !base.OTFontTable.GDEFTable.IsSkip(index, base.LookupID))
			{
				m_records.TryGetValue(index, out var value2);
				return value2;
			}
			return new List<SubsetTable>();
		case LookupSubTableFormat.Format2:
			if (m_glyphs.Contains(index) && !base.OTFontTable.GDEFTable.IsSkip(index, base.LookupID))
			{
				int value = m_cdefTable.GetValue(index);
				return m_subSetTables[value];
			}
			return new List<SubsetTable>();
		case LookupSubTableFormat.Format3:
			if (((SubsetTableFormat)m_subsetTable).Coverages[0].Contains(index) && !base.OTFontTable.GDEFTable.IsSkip(index, base.LookupID))
			{
				return new List<SubsetTable> { m_subsetTable };
			}
			return new List<SubsetTable>();
		default:
			return new List<SubsetTable>();
		}
	}
}
