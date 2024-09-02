using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartBorderImpl : CommonObject, IOfficeChartBorder, ICloneParent
{
	private const OfficeKnownColors DEF_COLOR_INEDX = (OfficeKnownColors)77;

	private ChartLineFormatRecord m_lineFormat = (ChartLineFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartLineFormat);

	private WorkbookImpl m_parentBook;

	private ChartSerieDataFormatImpl m_serieFormat;

	private ChartColor m_color;

	private double m_solidTransparency;

	private IInternalFill m_fill;

	private Excel2007BorderJoinType m_joinType;

	private string m_lineWeightString;

	private bool m_lineProperties;

	private OfficeArrowType m_beginArrowType = OfficeArrowType.None;

	private OfficeArrowType m_endArrowType = OfficeArrowType.None;

	private OfficeArrowSize m_beginArrowSize;

	private OfficeArrowSize m_endArrowSize;

	internal string m_beginArrowLg;

	internal string m_endArrowLg;

	internal string m_beginArrowwidth;

	internal string m_endArrowWidth;

	internal LineCap m_lineCap;

	private Excel2007ShapeLineStyle m_lineStyle;

	public Color LineColor
	{
		get
		{
			return m_color.GetRGB(m_parentBook);
		}
		set
		{
			if (m_color.ColorType != ColorType.RGB || value.ToArgb() != m_color.Value || AutoFormat)
			{
				AutoFormat = false;
				m_color.SetRGB(value, m_parentBook);
				m_lineFormat.IsAutoLineColor = false;
				HasLineProperties = true;
				if (m_serieFormat != null)
				{
					m_serieFormat.ClearOnPropertyChange();
				}
			}
		}
	}

	public OfficeArrowType BeginArrowType
	{
		get
		{
			return m_beginArrowType;
		}
		set
		{
			m_beginArrowType = value;
		}
	}

	internal LineCap CapStyle
	{
		get
		{
			return m_lineCap;
		}
		set
		{
			m_lineCap = value;
		}
	}

	public OfficeArrowType EndArrowType
	{
		get
		{
			return m_endArrowType;
		}
		set
		{
			m_endArrowType = value;
		}
	}

	public OfficeArrowSize BeginArrowSize
	{
		get
		{
			return m_beginArrowSize;
		}
		set
		{
			m_beginArrowSize = value;
		}
	}

	public OfficeArrowSize EndArrowSize
	{
		get
		{
			return m_endArrowSize;
		}
		set
		{
			m_endArrowSize = value;
		}
	}

	public OfficeChartLinePattern LinePattern
	{
		get
		{
			return m_lineFormat.LinePattern;
		}
		set
		{
			if (value != LinePattern || AutoFormat)
			{
				m_lineFormat.LinePattern = value;
				AutoFormat = false;
				if (m_serieFormat != null)
				{
					m_serieFormat.ClearOnPropertyChange();
				}
				HasLineProperties = true;
				if (base.Parent is ChartWallOrFloorImpl)
				{
					(base.Parent as ChartWallOrFloorImpl).HasShapeProperties = true;
				}
			}
		}
	}

	public OfficeChartLineWeight LineWeight
	{
		get
		{
			return m_lineFormat.LineWeight;
		}
		set
		{
			if (value == LineWeight && !AutoFormat)
			{
				return;
			}
			m_lineFormat.LineWeight = value;
			AutoFormat = false;
			if (!m_parentBook.IsWorkbookOpening)
			{
				if ((short)value == -1)
				{
					LineWeightString = "3175";
				}
				else
				{
					LineWeightString = (((short)value + 1) * 12700).ToString();
				}
			}
			if (m_serieFormat != null)
			{
				m_serieFormat.ClearOnPropertyChange();
			}
		}
	}

	internal IInternalFill Fill
	{
		get
		{
			return m_fill;
		}
		set
		{
			m_fill = value;
		}
	}

	internal bool HasGradientFill
	{
		get
		{
			if (m_fill != null)
			{
				return m_fill.FillType == OfficeFillType.Gradient;
			}
			return false;
		}
	}

	internal bool HasLineProperties
	{
		get
		{
			return m_lineProperties;
		}
		set
		{
			m_lineProperties = value;
		}
	}

	internal Excel2007BorderJoinType JoinType
	{
		get
		{
			return m_joinType;
		}
		set
		{
			m_joinType = value;
		}
	}

	public bool AutoFormat
	{
		get
		{
			return m_lineFormat.AutoFormat;
		}
		set
		{
			if (AutoFormat != value)
			{
				m_lineFormat.AutoFormat = value;
				if (value)
				{
					m_lineFormat.LineWeight = OfficeChartLineWeight.Hairline;
					m_lineFormat.LinePattern = OfficeChartLinePattern.Solid;
					IsAutoLineColor = true;
				}
				else
				{
					TryAndClearAutoColor();
				}
				if (m_serieFormat != null && !m_serieFormat.ParentChart.TypeChanging)
				{
					m_serieFormat.ClearOnPropertyChange();
				}
			}
		}
	}

	internal bool DrawTickLabels
	{
		get
		{
			return m_lineFormat.DrawTickLabels;
		}
		set
		{
			m_lineFormat.DrawTickLabels = value;
		}
	}

	public bool IsAutoLineColor
	{
		get
		{
			return m_lineFormat.IsAutoLineColor;
		}
		set
		{
			m_lineFormat.IsAutoLineColor = value;
			if (value)
			{
				m_lineFormat.ColorIndex = 77;
			}
			if (m_serieFormat != null)
			{
				m_serieFormat.ClearOnPropertyChange();
			}
		}
	}

	public OfficeKnownColors ColorIndex
	{
		get
		{
			return m_color.GetIndexed(m_parentBook);
		}
		set
		{
			if (m_color.ColorType != ColorType.Indexed || ColorIndex != value || AutoFormat)
			{
				value = ChartFrameFormatImpl.UpdateLineColor(value);
				AutoFormat = false;
				m_color.SetIndexed(value);
				m_lineFormat.IsAutoLineColor = false;
				if (m_serieFormat != null)
				{
					m_serieFormat.ClearOnPropertyChange();
				}
			}
		}
	}

	public ChartColor Color => m_color;

	public double Transparency
	{
		get
		{
			return m_solidTransparency;
		}
		set
		{
			if (value < 0.0 || value > 1.0)
			{
				throw new ArgumentOutOfRangeException("Transparency is out of range");
			}
			if (value != m_solidTransparency && (AutoFormat || IsAutoLineColor))
			{
				AutoFormat = false;
				IsAutoLineColor = false;
			}
			m_solidTransparency = value;
		}
	}

	internal string LineWeightString
	{
		get
		{
			return m_lineWeightString;
		}
		set
		{
			m_lineWeightString = value;
		}
	}

	internal Excel2007ShapeLineStyle LineStyle
	{
		get
		{
			return m_lineStyle;
		}
		set
		{
			m_lineStyle = value;
		}
	}

	public ChartBorderImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_lineFormat = (ChartLineFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartLineFormat);
		Fill = new ShapeFillImpl(application, parent);
		SetParents();
	}

	[CLSCompliant(false)]
	public ChartBorderImpl(IApplication application, object parent, ChartLineFormatRecord line)
		: base(application, parent)
	{
		if (line == null)
		{
			throw new ArgumentNullException("line");
		}
		m_lineFormat = line;
		SetParents();
	}

	public ChartBorderImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: base(application, parent)
	{
		Parse(data, ref iPos);
		SetParents();
	}

	public void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartLineFormat);
		m_lineFormat = (ChartLineFormatRecord)biffRecordRaw;
		iPos++;
	}

	public void Serialize(IList<IBiffStorage> records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_lineFormat != null)
		{
			UpdateColor();
			records.Add((IBiffStorage)m_lineFormat.Clone());
		}
	}

	private void SetParents()
	{
		m_parentBook = (WorkbookImpl)FindParent(typeof(WorkbookImpl));
		m_serieFormat = (ChartSerieDataFormatImpl)FindParent(typeof(ChartSerieDataFormatImpl));
		if (m_parentBook == null)
		{
			throw new ApplicationException("cannot find parent objects.");
		}
		m_color = new ChartColor((OfficeKnownColors)m_lineFormat.ColorIndex);
		m_color.AfterChange += UpdateColor;
	}

	internal void UpdateColor()
	{
		m_lineFormat.ColorIndex = (ushort)m_color.GetIndexed(m_parentBook);
	}

	internal void TryAndClearAutoColor()
	{
		if (m_serieFormat != null && !m_serieFormat.ParentChart.TypeChanging)
		{
			int num = m_serieFormat.UpdateLineColor();
			if (num != -1)
			{
				m_lineFormat.ColorIndex = (ushort)num;
				m_lineFormat.IsAutoLineColor = false;
			}
		}
	}

	public ChartBorderImpl Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ChartBorderImpl chartBorderImpl = new ChartBorderImpl(base.Application, parent);
		chartBorderImpl.m_lineFormat = (ChartLineFormatRecord)CloneUtils.CloneCloneable(m_lineFormat);
		chartBorderImpl.m_lineProperties = m_lineProperties;
		chartBorderImpl.JoinType = JoinType;
		if (m_fill != null)
		{
			chartBorderImpl.m_fill = m_fill;
		}
		chartBorderImpl.SetParent(parent);
		chartBorderImpl.SetParents();
		chartBorderImpl.m_color = m_color.Clone();
		chartBorderImpl.m_solidTransparency = m_solidTransparency;
		chartBorderImpl.m_lineWeightString = m_lineWeightString;
		chartBorderImpl.m_beginArrowType = m_beginArrowType;
		chartBorderImpl.m_endArrowType = m_endArrowType;
		chartBorderImpl.m_lineStyle = m_lineStyle;
		chartBorderImpl.m_lineCap = m_lineCap;
		chartBorderImpl.m_beginArrowSize = m_beginArrowSize;
		chartBorderImpl.m_endArrowSize = m_endArrowSize;
		return chartBorderImpl;
	}

	internal void ClearAutoColor()
	{
		IsAutoLineColor = false;
	}

	object ICloneParent.Clone(object parent)
	{
		return Clone(parent);
	}
}
