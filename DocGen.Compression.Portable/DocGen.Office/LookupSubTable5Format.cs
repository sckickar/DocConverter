using System.Collections.Generic;

namespace DocGen.Office;

internal class LookupSubTable5Format : BaseTable
{
	private IDictionary<int, IList<SubsetTable>> m_records;

	private ICollection<int> m_glyphIds;

	private IList<IList<SubsetTable>> m_subsetTables;

	private SubsetTable m_subSetTable;

	private CDEFTable m_cdefTable;

	private LookupSubTableFormat m_format;

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

	internal IList<IList<SubsetTable>> SubsetTables
	{
		get
		{
			return m_subsetTables;
		}
		set
		{
			m_subsetTables = value;
		}
	}

	internal LookupSubTable5Format(OtfTable table, int flag, IDictionary<int, IList<SubsetTable>> records, LookupSubTableFormat format)
		: base(table, flag)
	{
		m_records = records;
		m_format = format;
	}

	internal LookupSubTable5Format(OtfTable table, int flag, ICollection<int> glyphIds, CDEFTable ctable, LookupSubTableFormat format)
		: base(table, flag)
	{
		m_glyphIds = glyphIds;
		m_cdefTable = ctable;
		m_format = format;
	}

	internal LookupSubTable5Format(OtfTable table, int flag, SubsetTableFormat subsetTable, LookupSubTableFormat format)
		: base(table, flag)
	{
		m_subSetTable = subsetTable;
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
			if (m_glyphIds.Contains(index) && !base.OTFontTable.GDEFTable.IsSkip(index, base.LookupID))
			{
				int value = m_cdefTable.GetValue(index);
				return m_subsetTables[value];
			}
			return new List<SubsetTable>();
		case LookupSubTableFormat.Format3:
			if (((SubsetTableFormat)m_subSetTable).Coverages[0].Contains(index) && !base.OTFontTable.GDEFTable.IsSkip(index, base.LookupID))
			{
				return new List<SubsetTable> { m_subSetTable };
			}
			return new List<SubsetTable>();
		default:
			return new List<SubsetTable>();
		}
	}
}
