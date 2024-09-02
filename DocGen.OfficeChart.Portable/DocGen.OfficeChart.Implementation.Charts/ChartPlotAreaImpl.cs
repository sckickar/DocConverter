using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartPlotAreaImpl : ChartFrameFormatImpl, IOfficeChartFrameFormat, IOfficeChartFillBorder
{
	private ChartPlotAreaRecord m_plotArea;

	private IOfficeChartLayout m_layout;

	public new IOfficeChartLayout Layout
	{
		get
		{
			if (m_layout == null)
			{
				m_layout = new ChartLayoutImpl(base.Application, this, base.Parent);
			}
			return m_layout;
		}
		set
		{
			m_layout = value;
		}
	}

	public ChartPlotAreaLayoutRecord PlotAreaLayout
	{
		get
		{
			if (m_plotAreaLayout == null)
			{
				m_plotAreaLayout = (ChartPlotAreaLayoutRecord)BiffRecordFactory.GetRecord(TBIFFRecord.PlotAreaLayout);
			}
			return m_plotAreaLayout;
		}
	}

	public ChartPlotAreaImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_plotArea = (ChartPlotAreaRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPlotArea);
		base.Border.LinePattern = OfficeChartLinePattern.None;
	}

	public ChartPlotAreaImpl(IApplication application, object parent, OfficeChartType type)
		: this(application, parent)
	{
		if (Array.IndexOf(ChartImpl.DEF_WALLS_OR_FLOOR_TYPES, type) == -1 && Array.IndexOf(ChartImpl.DEF_DONT_NEED_PLOT, type) == -1 && base.Workbook.Version == OfficeVersion.Excel97to2003)
		{
			base.Interior.ForegroundColorIndex = OfficeKnownColors.Grey_25_percent;
		}
		else
		{
			base.Interior.ForegroundColorIndex = OfficeKnownColors.WhiteCustom;
		}
	}

	public ChartPlotAreaImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: base(application, parent, bSetDefaults: false)
	{
		Parse(data, ref iPos);
	}

	public new void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartPlotArea);
		m_plotArea = (ChartPlotAreaRecord)biffRecordRaw;
		iPos++;
		biffRecordRaw = data[iPos];
		if (biffRecordRaw.TypeCode == TBIFFRecord.ChartFrame)
		{
			base.Parse(data, ref iPos);
		}
		iPos--;
	}

	[CLSCompliant(false)]
	public new void Serialize(IList<IBiffStorage> records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_plotArea != null)
		{
			records.Add((BiffRecordRaw)m_plotArea.Clone());
		}
		base.Serialize(records);
		if (m_plotAreaLayout != null)
		{
			SerializeRecord(records, m_plotAreaLayout);
		}
	}
}
