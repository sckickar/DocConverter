using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartLegendEntryImpl : CommonObject, IOfficeChartLegendEntry
{
	private ChartLegendxnRecord m_legendXN;

	private ChartTextAreaImpl m_text;

	private ChartLegendEntriesColl m_legendEnties;

	private int m_index;

	public bool IsDeleted
	{
		get
		{
			return m_legendXN.IsDeleted;
		}
		set
		{
			if (IsDeleted == value)
			{
				return;
			}
			if (value)
			{
				if (!m_legendEnties.CanDelete(m_index) && !m_text.ParentWorkbook.IsWorkbookOpening)
				{
					throw new ApplicationException("cannot delete last legend entry in chart");
				}
				IsFormatted = !value;
			}
			m_legendXN.IsDeleted = value;
		}
	}

	public bool IsFormatted
	{
		get
		{
			return m_legendXN.IsFormatted;
		}
		set
		{
			if (value != IsFormatted)
			{
				if (value)
				{
					m_text = new ChartTextAreaImpl(base.Application, this);
					m_legendXN.IsDeleted = false;
				}
				m_legendXN.IsFormatted = value;
			}
		}
	}

	public IOfficeChartTextArea TextArea
	{
		get
		{
			m_legendXN.IsDeleted = false;
			m_legendXN.IsFormatted = true;
			return m_text;
		}
	}

	public int LegendEntityIndex
	{
		get
		{
			return m_legendXN.LegendEntityIndex;
		}
		set
		{
			m_legendXN.LegendEntityIndex = (ushort)value;
		}
	}

	public int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	public ChartLegendEntryImpl(IApplication application, object parent, int iIndex)
		: base(application, parent)
	{
		m_legendXN = (ChartLegendxnRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartLegendxn);
		m_text = new ChartTextAreaImpl(application, this);
		m_index = iIndex;
		SetParents();
	}

	public ChartLegendEntryImpl(IApplication application, object parent, int iIndex, IList<BiffRecordRaw> data, ref int iPos)
		: base(application, parent)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		m_index = iIndex;
		SetParents();
		Parse(data, ref iPos);
	}

	public void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		data[iPos].CheckTypeCode(TBIFFRecord.ChartLegendxn);
		m_legendXN = (ChartLegendxnRecord)data[iPos];
		iPos++;
		if (data[iPos].TypeCode != TBIFFRecord.Begin)
		{
			return;
		}
		iPos++;
		int num = 1;
		while (num != 0)
		{
			switch (data[iPos].TypeCode)
			{
			case TBIFFRecord.Begin:
				iPos = BiffRecordRaw.SkipBeginEndBlock(data, iPos);
				break;
			case TBIFFRecord.End:
				num--;
				break;
			case TBIFFRecord.ChartText:
				m_text = new ChartTextAreaImpl(base.Application, this);
				iPos = m_text.Parse(data, iPos) - 1;
				break;
			}
			iPos++;
		}
	}

	public void SetParents()
	{
		m_legendEnties = (ChartLegendEntriesColl)FindParent(typeof(ChartLegendEntriesColl));
		if (m_legendEnties == null)
		{
			throw new ArgumentNullException("cannot find parent object");
		}
	}

	[CLSCompliant(false)]
	public void Serialize(IList<IBiffStorage> records)
	{
		if (records == null)
		{
			throw new ArgumentException("records");
		}
		if (IsFormatted || IsDeleted)
		{
			records.Add((BiffRecordRaw)m_legendXN.Clone());
			if (m_text != null)
			{
				records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
				m_text.Serialize(records, bIsLegendEntry: true);
				records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
			}
		}
	}

	public void Clear()
	{
		IsFormatted = false;
	}

	public void Delete()
	{
		IsDeleted = true;
	}

	public ChartLegendEntryImpl Clone(object parent, Dictionary<int, int> dicIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		ChartLegendEntryImpl chartLegendEntryImpl = (ChartLegendEntryImpl)MemberwiseClone();
		chartLegendEntryImpl.SetParent(parent);
		chartLegendEntryImpl.SetParents();
		if (m_text != null)
		{
			chartLegendEntryImpl.m_text = (ChartTextAreaImpl)m_text.Clone(chartLegendEntryImpl, dicIndexes, dicNewSheetNames);
		}
		chartLegendEntryImpl.m_legendXN = (ChartLegendxnRecord)CloneUtils.CloneCloneable(m_legendXN);
		return chartLegendEntryImpl;
	}
}
