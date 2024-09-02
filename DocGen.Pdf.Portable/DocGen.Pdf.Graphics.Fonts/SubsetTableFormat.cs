using System.Collections.Generic;

namespace DocGen.Pdf.Graphics.Fonts;

internal class SubsetTableFormat : SubsetTable
{
	private int[] m_glyphs;

	private int[] m_btGlyphs;

	private int[] m_lookupGlyphs;

	private CDEFTable m_cdefTable;

	private LookupSubTableRecord[] m_records;

	private IList<ICollection<int>> m_coverages;

	private IList<ICollection<int>> m_btCoverages;

	private IList<ICollection<int>> m_lookupCoverages;

	private LookupSubTable6Format subTable;

	private bool m_isLookup;

	private bool m_isBackTrack;

	private bool m_isConverage;

	private bool m_cdefMatch;

	private bool m_isFormat2;

	private bool m_isFormat3;

	internal IList<ICollection<int>> Coverages
	{
		get
		{
			return m_coverages;
		}
		set
		{
			m_coverages = value;
		}
	}

	internal override int Length
	{
		get
		{
			if (m_isConverage || m_isFormat3)
			{
				return m_coverages.Count;
			}
			return m_glyphs.Length + 1;
		}
	}

	internal override int LookupLength
	{
		get
		{
			if (m_isFormat3)
			{
				return m_lookupCoverages.Count;
			}
			return m_lookupGlyphs.Length;
		}
	}

	internal override int BTCLength
	{
		get
		{
			if (m_isFormat3)
			{
				return m_btCoverages.Count;
			}
			return m_btGlyphs.Length;
		}
	}

	internal override LookupSubTableRecord[] LookupRecord => m_records;

	internal SubsetTableFormat(int[] glyphs, LookupSubTableRecord[] records)
	{
		m_glyphs = glyphs;
		m_records = records;
	}

	internal SubsetTableFormat(int[] btGlyphs, int[] glyphs, int[] lookupGlyphs, LookupSubTableRecord[] records)
	{
		m_btGlyphs = btGlyphs;
		m_glyphs = glyphs;
		m_lookupGlyphs = lookupGlyphs;
		m_records = records;
		m_isBackTrack = true;
		m_isLookup = true;
	}

	internal SubsetTableFormat(LookupSubTable5Format subtable5, int[] glyphs, LookupSubTableRecord[] records)
	{
		m_glyphs = glyphs;
		m_records = records;
		m_cdefTable = subtable5.CDEFTable;
		m_cdefMatch = true;
	}

	internal SubsetTableFormat(IList<ICollection<int>> coverages, LookupSubTableRecord[] records)
	{
		m_coverages = coverages;
		m_records = records;
		m_isConverage = true;
	}

	internal SubsetTableFormat(LookupSubTable6Format subTable, int[] backtrackClassIds, int[] inputClassIds, int[] lookAheadClassIds, LookupSubTableRecord[] substLookupRecords)
	{
		this.subTable = subTable;
		m_btGlyphs = backtrackClassIds;
		m_glyphs = inputClassIds;
		m_lookupGlyphs = lookAheadClassIds;
		m_records = substLookupRecords;
		m_isFormat2 = true;
	}

	internal SubsetTableFormat(IList<ICollection<int>> bkCoverages, IList<ICollection<int>> coverages, IList<ICollection<int>> lookupCoverages, LookupSubTableRecord[] records)
	{
		m_btCoverages = bkCoverages;
		m_coverages = coverages;
		m_lookupCoverages = lookupCoverages;
		m_records = records;
		m_isFormat3 = true;
	}

	internal override bool Match(int id, int index)
	{
		if (m_cdefMatch)
		{
			return m_cdefTable.GetValue(id) == m_glyphs[index - 1];
		}
		if (m_isConverage || m_isFormat3)
		{
			return m_coverages[index].Contains(id);
		}
		if (m_isFormat2)
		{
			return subTable.CDEFTable.GetValue(id) == m_glyphs[index - 1];
		}
		return id == m_glyphs[index - 1];
	}

	internal override bool IsLookup(int glyphId, int index)
	{
		if (m_isLookup)
		{
			return glyphId == m_lookupGlyphs[index];
		}
		if (m_isFormat2)
		{
			return subTable.LookupCDEFTable.GetValue(glyphId) == m_lookupGlyphs[index];
		}
		if (m_isFormat3)
		{
			return m_lookupCoverages[index].Contains(glyphId);
		}
		return false;
	}

	internal override bool IsBackTrack(int glyphId, int index)
	{
		if (m_isBackTrack)
		{
			return glyphId == m_btGlyphs[index];
		}
		if (m_isFormat2)
		{
			return subTable.BtCDEFTable.GetValue(glyphId) == m_btGlyphs[index];
		}
		if (m_isFormat3)
		{
			return m_btCoverages[index].Contains(glyphId);
		}
		return false;
	}
}
