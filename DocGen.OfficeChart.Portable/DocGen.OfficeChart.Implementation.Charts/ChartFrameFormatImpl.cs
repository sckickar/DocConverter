using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartFrameFormatImpl : CommonObject, IOfficeChartFrameFormat, IOfficeChartFillBorder, IFillColor
{
	private ChartFrameRecord m_chartFrame = (ChartFrameRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartFrame);

	private ChartBorderImpl m_border;

	private ThreeDFormatImpl m_3D;

	private ChartInteriorImpl m_interior;

	private ShadowImpl m_shadow;

	private ChartFillImpl m_fill;

	protected ChartImpl m_chart;

	private IOfficeChartLayout m_layout;

	protected ChartPlotAreaLayoutRecord m_plotAreaLayout;

	[CLSCompliant(false)]
	public ChartFrameRecord FrameRecord => m_chartFrame;

	public WorkbookImpl Workbook => m_chart.InnerWorkbook;

	public IOfficeChartLayout Layout
	{
		get
		{
			return m_layout;
		}
		set
		{
			m_layout = value;
		}
	}

	public bool HasInterior => m_interior != null;

	public bool HasLineProperties
	{
		get
		{
			return m_border != null;
		}
		internal set
		{
			if (value)
			{
				_ = Border;
			}
			else
			{
				m_border = null;
			}
		}
	}

	public IOfficeChartBorder Border
	{
		get
		{
			if (m_border == null)
			{
				m_border = new ChartBorderImpl(base.Application, this);
			}
			return m_border;
		}
	}

	public IOfficeChartInterior Interior
	{
		get
		{
			if (m_interior == null)
			{
				m_interior = new ChartInteriorImpl(base.Application, this);
			}
			return m_interior;
		}
	}

	public IThreeDFormat ThreeD
	{
		get
		{
			if (m_3D == null)
			{
				m_3D = new ThreeDFormatImpl(base.Application, this);
			}
			return m_3D;
		}
	}

	public IOfficeFill Fill
	{
		get
		{
			if (m_fill == null)
			{
				m_fill = new ChartFillImpl(base.Application, this);
			}
			IsAutomaticFormat = false;
			return m_fill;
		}
	}

	public bool HasShadowProperties
	{
		get
		{
			return m_shadow != null;
		}
		internal set
		{
			if (value)
			{
				_ = Shadow;
			}
			else
			{
				m_shadow = null;
			}
		}
	}

	public bool Has3dProperties
	{
		get
		{
			return m_3D != null;
		}
		internal set
		{
			if (value)
			{
				_ = ThreeD;
			}
			else
			{
				m_3D = null;
			}
		}
	}

	public IShadow Shadow
	{
		get
		{
			if (m_shadow == null)
			{
				m_shadow = new ShadowImpl(base.Application, this);
			}
			return m_shadow;
		}
	}

	internal OfficeRectangleStyle RectangleStyle
	{
		get
		{
			return m_chartFrame.Rectangle;
		}
		set
		{
			m_chartFrame.Rectangle = value;
		}
	}

	public bool IsAutoSize
	{
		get
		{
			return m_chartFrame.AutoSize;
		}
		set
		{
			m_chartFrame.AutoSize = value;
		}
	}

	public bool IsAutoPosition
	{
		get
		{
			return m_chartFrame.AutoPosition;
		}
		set
		{
			m_chartFrame.AutoPosition = value;
		}
	}

	public bool IsBorderCornersRound
	{
		get
		{
			return Interior.SwapColorsOnNegative;
		}
		set
		{
			Interior.SwapColorsOnNegative = value;
		}
	}

	public IOfficeChartBorder LineProperties => Border;

	public ChartColor ForeGroundColorObject => (Interior as ChartInteriorImpl).ForegroundColorObject;

	public ChartColor BackGroundColorObject => (Interior as ChartInteriorImpl).BackgroundColorObject;

	public OfficePattern Pattern
	{
		get
		{
			return Interior.Pattern;
		}
		set
		{
			Interior.Pattern = value;
		}
	}

	public bool IsAutomaticFormat
	{
		get
		{
			return Interior.UseAutomaticFormat;
		}
		set
		{
			Interior.UseAutomaticFormat = value;
		}
	}

	public bool Visible
	{
		get
		{
			return Interior.Pattern != OfficePattern.None;
		}
		set
		{
			if (value)
			{
				if (Interior.Pattern == OfficePattern.None)
				{
					Interior.Pattern = OfficePattern.Solid;
				}
			}
			else
			{
				Interior.Pattern = OfficePattern.None;
			}
		}
	}

	public ChartFrameFormatImpl(IApplication application, object parent)
		: this(application, parent, bAutoSize: false, bIsInteriorGrey: false, bSetDefaults: true)
	{
	}

	public ChartFrameFormatImpl(IApplication application, object parent, bool bSetDefaults)
		: this(application, parent, bAutoSize: false, bIsInteriorGrey: false, bSetDefaults)
	{
	}

	public ChartFrameFormatImpl(IApplication application, object parent, bool bAutoSize, bool bIsInteriorGrey, bool bSetDefaults)
		: base(application, parent)
	{
		SetParents();
		if (!Workbook.IsWorkbookOpening && bSetDefaults)
		{
			SetDefaultValues(bAutoSize, bIsInteriorGrey);
		}
	}

	public ChartFrameFormatImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: base(application, parent)
	{
		SetParents();
		Parse(data, ref iPos);
	}

	private void SetParents()
	{
		m_chart = FindParent(typeof(ChartImpl)) as ChartImpl;
		if (m_chart == null)
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
		m_chartFrame = (ChartFrameRecord)record;
		iPos++;
		record = data[iPos];
		int num = 0;
		if (CheckBegin(record))
		{
			num++;
			do
			{
				iPos++;
				record = data[iPos];
				ParseRecord(record, ref num);
			}
			while (num != 0);
			iPos++;
		}
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
		case TBIFFRecord.ChartAreaFormat:
			m_interior = new ChartInteriorImpl(base.Application, this, (ChartAreaFormatRecord)record);
			break;
		case TBIFFRecord.ChartLineFormat:
			m_border = new ChartBorderImpl(base.Application, this, (ChartLineFormatRecord)record);
			break;
		case TBIFFRecord.ChartGelFrame:
			m_fill = new ChartFillImpl(base.Application, this, (ChartGelFrameRecord)record);
			break;
		case TBIFFRecord.PlotAreaLayout:
			if (m_plotAreaLayout == null)
			{
				m_plotAreaLayout = (ChartPlotAreaLayoutRecord)BiffRecordFactory.GetRecord(TBIFFRecord.PlotAreaLayout);
			}
			m_plotAreaLayout = (ChartPlotAreaLayoutRecord)record;
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
		SerializeRecord(records, m_chartFrame);
		SerializeRecord(records, BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		if (m_border != null)
		{
			m_border.Serialize(records);
		}
		if (m_interior != null)
		{
			m_interior.Serialize(records);
		}
		if (m_fill != null)
		{
			m_fill.Serialize(records);
		}
		if (m_plotAreaLayout != null)
		{
			SerializeRecord(records, m_plotAreaLayout);
		}
		SerializeRecord(records, BiffRecordFactory.GetRecord(TBIFFRecord.End));
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

	public void SetDefaultValues(bool bAutoSize, bool bIsInteriorGray)
	{
		m_chartFrame = (ChartFrameRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartFrame);
		m_chartFrame.AutoSize = bAutoSize;
		m_border = new ChartBorderImpl(base.Application, this);
		m_border.ColorIndex = OfficeKnownColors.Grey_50_percent;
		m_border.AutoFormat = !m_chart.IsChart3D;
		m_interior = new ChartInteriorImpl(base.Application, this);
		m_interior.InitForFrameFormat(bAutoSize, m_chart.IsChart3D, bIsInteriorGray);
	}

	public static OfficeKnownColors UpdateLineColor(OfficeKnownColors color)
	{
		if (color < OfficeKnownColors.Custom0)
		{
			return color + 8;
		}
		return color;
	}

	public void Clear()
	{
		SetDefaultValues(bAutoSize: false, bIsInteriorGray: false);
	}

	public ChartFrameFormatImpl Clone(object parent)
	{
		ChartFrameFormatImpl chartFrameFormatImpl = (ChartFrameFormatImpl)MemberwiseClone();
		chartFrameFormatImpl.SetParent(parent);
		chartFrameFormatImpl.SetParents();
		chartFrameFormatImpl.m_bIsDisposed = m_bIsDisposed;
		if (m_chartFrame != null)
		{
			chartFrameFormatImpl.m_chartFrame = (ChartFrameRecord)m_chartFrame.Clone();
		}
		if (m_border != null)
		{
			chartFrameFormatImpl.m_border = m_border.Clone(chartFrameFormatImpl);
		}
		if (m_interior != null)
		{
			_ = m_interior.Pattern;
			chartFrameFormatImpl.Pattern = m_interior.Pattern;
			chartFrameFormatImpl.m_interior = m_interior.Clone(chartFrameFormatImpl);
		}
		if (m_fill != null)
		{
			chartFrameFormatImpl.m_fill = (ChartFillImpl)m_fill.Clone(chartFrameFormatImpl);
		}
		if (!m_chart.TypeChanging && !m_chart.IsParsed && m_fill != null)
		{
			if (chartFrameFormatImpl.m_fill.ForeColorObject != m_fill.ForeColorObject)
			{
				chartFrameFormatImpl.m_fill.ForeColorObject.CopyFrom(m_fill.ForeColorObject, callEvent: false);
			}
			if (chartFrameFormatImpl.m_fill.BackColorObject != m_fill.BackColorObject)
			{
				chartFrameFormatImpl.m_fill.BackColorObject.CopyFrom(m_fill.BackColorObject, callEvent: false);
			}
		}
		if (m_3D != null)
		{
			chartFrameFormatImpl.m_3D = m_3D.Clone(chartFrameFormatImpl);
		}
		if (m_shadow != null)
		{
			chartFrameFormatImpl.m_shadow = m_shadow.Clone(chartFrameFormatImpl);
		}
		if (m_layout != null)
		{
			chartFrameFormatImpl.m_layout = m_layout;
		}
		if (m_plotAreaLayout != null)
		{
			chartFrameFormatImpl.m_plotAreaLayout = (ChartPlotAreaLayoutRecord)m_plotAreaLayout.Clone();
		}
		return chartFrameFormatImpl;
	}
}
