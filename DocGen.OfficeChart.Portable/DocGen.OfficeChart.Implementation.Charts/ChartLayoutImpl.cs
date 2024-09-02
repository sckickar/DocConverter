using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartLayoutImpl : CommonObject, IOfficeChartLayout, IParentApplication
{
	protected ChartImpl m_book;

	protected object m_Parent;

	protected IOfficeChart m_chart;

	protected IShape m_chartShape;

	protected IOfficeChartManualLayout m_manualLayout;

	public WorkbookImpl Workbook => m_book.InnerWorkbook;

	public new object Parent => m_Parent;

	public IOfficeChartManualLayout ManualLayout
	{
		get
		{
			if (m_manualLayout == null)
			{
				m_manualLayout = new ChartManualLayoutImpl(Workbook.Application, this);
			}
			return m_manualLayout;
		}
		set
		{
			m_manualLayout = value;
		}
	}

	public LayoutTargets LayoutTarget
	{
		get
		{
			return ManualLayout.LayoutTarget;
		}
		set
		{
			ManualLayout.LayoutTarget = value;
		}
	}

	public LayoutModes LeftMode
	{
		get
		{
			return ManualLayout.LeftMode;
		}
		set
		{
			if (Parent is ChartLegendImpl || Parent is ChartPlotAreaImpl || Parent is ChartDataLabelsImpl)
			{
				ManualLayout.LeftMode = value;
			}
			else if (Parent is ChartTextAreaImpl && Parent is ChartTextAreaImpl && !((Parent as ChartTextAreaImpl).Parent is ChartLegendImpl))
			{
				ManualLayout.LeftMode = value;
			}
		}
	}

	public LayoutModes TopMode
	{
		get
		{
			return ManualLayout.TopMode;
		}
		set
		{
			if (Parent is ChartLegendImpl || Parent is ChartPlotAreaImpl || Parent is ChartDataLabelsImpl)
			{
				ManualLayout.TopMode = value;
			}
			else if (Parent is ChartTextAreaImpl && Parent is ChartTextAreaImpl && !((Parent as ChartTextAreaImpl).Parent is ChartLegendImpl))
			{
				ManualLayout.TopMode = value;
			}
		}
	}

	public double Left
	{
		get
		{
			double num = 0.0;
			if (m_chart != null)
			{
				return ManualLayout.Left * ((m_chartShape != null) ? ((double)m_chartShape.Width) : m_chart.Width);
			}
			return ManualLayout.Left;
		}
		set
		{
			if (Parent is ChartLegendImpl || Parent is ChartPlotAreaImpl || Parent is ChartDataLabelsImpl)
			{
				if (m_chart != null)
				{
					ManualLayout.Left = value / ((m_chartShape != null) ? ((double)m_chartShape.Width) : m_chart.Width);
				}
				else
				{
					ManualLayout.Left = value;
				}
			}
			else if (Parent is ChartTextAreaImpl && Parent is ChartTextAreaImpl && !((Parent as ChartTextAreaImpl).Parent is ChartLegendImpl))
			{
				if (m_chart != null)
				{
					ManualLayout.Left = value / ((m_chartShape != null) ? ((double)m_chartShape.Width) : m_chart.Width);
				}
				else
				{
					ManualLayout.Left = value;
				}
			}
		}
	}

	public double Top
	{
		get
		{
			double num = 0.0;
			if (m_chart != null)
			{
				return ManualLayout.Top * ((m_chartShape != null) ? ((double)m_chartShape.Height) : m_chart.Height);
			}
			return ManualLayout.Top;
		}
		set
		{
			if (Parent is ChartLegendImpl || Parent is ChartPlotAreaImpl || Parent is ChartDataLabelsImpl)
			{
				if (m_chart != null)
				{
					ManualLayout.Top = value / ((m_chartShape != null) ? ((double)m_chartShape.Height) : m_chart.Height);
				}
				else
				{
					ManualLayout.Top = value;
				}
			}
			else if (Parent is ChartTextAreaImpl && Parent is ChartTextAreaImpl && !((Parent as ChartTextAreaImpl).Parent is ChartLegendImpl))
			{
				if (m_chart != null)
				{
					ManualLayout.Top = value / ((m_chartShape != null) ? ((double)m_chartShape.Height) : m_chart.Height);
				}
				else
				{
					ManualLayout.Top = value;
				}
			}
		}
	}

	public LayoutModes WidthMode
	{
		get
		{
			return ManualLayout.WidthMode;
		}
		set
		{
			if (Parent is ChartLegendImpl || Parent is ChartPlotAreaImpl || Parent is ChartDataLabelsImpl)
			{
				ManualLayout.WidthMode = value;
			}
			else if (Parent is ChartTextAreaImpl && Parent is ChartTextAreaImpl && !((Parent as ChartTextAreaImpl).Parent is ChartLegendImpl))
			{
				ManualLayout.WidthMode = value;
			}
		}
	}

	public LayoutModes HeightMode
	{
		get
		{
			return ManualLayout.HeightMode;
		}
		set
		{
			if (Parent is ChartLegendImpl || Parent is ChartPlotAreaImpl || Parent is ChartDataLabelsImpl)
			{
				ManualLayout.HeightMode = value;
			}
			else if (Parent is ChartTextAreaImpl && Parent is ChartTextAreaImpl && !((Parent as ChartTextAreaImpl).Parent is ChartLegendImpl))
			{
				ManualLayout.HeightMode = value;
			}
		}
	}

	public double Width
	{
		get
		{
			double num = 0.0;
			if (m_chart != null)
			{
				return ManualLayout.Width * ((m_chartShape != null) ? ((double)m_chartShape.Width) : m_chart.Width);
			}
			return ManualLayout.Width;
		}
		set
		{
			if (Parent is ChartLegendImpl || Parent is ChartPlotAreaImpl || Parent is ChartDataLabelsImpl)
			{
				if (m_chart != null)
				{
					ManualLayout.Width = value / ((m_chartShape != null) ? ((double)m_chartShape.Width) : m_chart.Width);
				}
				else
				{
					ManualLayout.Width = value;
				}
			}
			else if (Parent is ChartTextAreaImpl && Parent is ChartTextAreaImpl && !((Parent as ChartTextAreaImpl).Parent is ChartLegendImpl))
			{
				if (m_chart != null)
				{
					ManualLayout.Width = value / ((m_chartShape != null) ? ((double)m_chartShape.Width) : m_chart.Width);
				}
				else
				{
					ManualLayout.Width = value;
				}
			}
		}
	}

	public double Height
	{
		get
		{
			double num = 0.0;
			if (m_chart != null)
			{
				return ManualLayout.Height * ((m_chartShape != null) ? ((double)m_chartShape.Height) : m_chart.Height);
			}
			return ManualLayout.Height;
		}
		set
		{
			if (Parent is ChartLegendImpl || Parent is ChartPlotAreaImpl || Parent is ChartDataLabelsImpl)
			{
				if (m_chart != null)
				{
					ManualLayout.Height = value / ((m_chartShape != null) ? ((double)m_chartShape.Height) : m_chart.Height);
				}
				else
				{
					ManualLayout.Height = value;
				}
			}
			else if (Parent is ChartTextAreaImpl && Parent is ChartTextAreaImpl && !((Parent as ChartTextAreaImpl).Parent is ChartLegendImpl))
			{
				if (m_chart != null)
				{
					ManualLayout.Height = value / ((m_chartShape != null) ? ((double)m_chartShape.Height) : m_chart.Height);
				}
				else
				{
					ManualLayout.Height = value;
				}
			}
		}
	}

	internal bool IsManualLayout => m_manualLayout != null;

	public ChartLayoutImpl(IApplication application, object parent, object chartObject)
		: this(application, parent, bAutoSize: false, bIsInteriorGrey: false, bSetDefaults: true)
	{
		if (chartObject is IOfficeChart)
		{
			m_chart = (IOfficeChart)chartObject;
			if ((m_chart as ChartImpl).Parent != null && (m_chart as ChartImpl).Parent is IShape)
			{
				m_chartShape = (IShape)(m_chart as ChartImpl).Parent;
			}
		}
		else if (chartObject is IOfficeChartDataPoint && ((IOfficeChartDataPoint)chartObject) is ChartDataPointImpl)
		{
			m_chart = (IOfficeChart)FindParent(typeof(ChartImpl));
			if ((m_chart as ChartImpl).Parent != null && (m_chart as ChartImpl).Parent is IShape)
			{
				m_chartShape = (IShape)(m_chart as ChartImpl).Parent;
			}
		}
	}

	public ChartLayoutImpl(IApplication application, object parent, bool bSetDefaults)
		: this(application, parent, bAutoSize: false, bIsInteriorGrey: false, bSetDefaults)
	{
	}

	public ChartLayoutImpl(IApplication application, object parent, bool bAutoSize, bool bIsInteriorGrey, bool bSetDefaults)
		: base(application, parent)
	{
		SetParents(parent);
		if (!Workbook.IsWorkbookOpening && bSetDefaults)
		{
			SetDefaultValues();
		}
	}

	public ChartLayoutImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: base(application, parent)
	{
		SetParents(parent);
		Parse(data, ref iPos);
	}

	private void SetParents(object parent)
	{
		m_book = FindParent(typeof(ChartImpl)) as ChartImpl;
		m_Parent = parent;
		if (m_book == null)
		{
			throw new ArgumentNullException("Can't find parent chart");
		}
	}

	[CLSCompliant(false)]
	public void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (iPos < 0 || iPos > data.Count)
		{
			throw new ArgumentOutOfRangeException("iPos", "Value cannot be less than 0 and greater than data.Count");
		}
		BiffRecordRaw record = data[iPos];
		record = UnwrapRecord(record);
		record.CheckTypeCode(TBIFFRecord.ChartFrame);
	}

	[CLSCompliant(false)]
	protected virtual bool CheckBegin(BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		return record.TypeCode == TBIFFRecord.Begin;
	}

	[CLSCompliant(false)]
	protected virtual void ParseRecord(BiffRecordRaw record, ref int iBeginCounter)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		switch (record.TypeCode)
		{
		case TBIFFRecord.Begin:
			iBeginCounter++;
			break;
		case TBIFFRecord.End:
			iBeginCounter--;
			break;
		}
	}

	[CLSCompliant(false)]
	public void Serialize(IList<IBiffStorage> records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
	}

	[CLSCompliant(false)]
	protected virtual void SerializeRecord(IList<IBiffStorage> list, BiffRecordRaw record)
	{
		if (list == null)
		{
			throw new ArgumentNullException("list");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		list.Add((BiffRecordRaw)record.Clone());
	}

	[CLSCompliant(false)]
	protected virtual BiffRecordRaw UnwrapRecord(BiffRecordRaw record)
	{
		return record;
	}

	public void SetDefaultValues()
	{
	}
}
