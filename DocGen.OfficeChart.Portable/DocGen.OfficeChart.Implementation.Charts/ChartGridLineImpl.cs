using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartGridLineImpl : CommonObject, IOfficeChartGridLine, IOfficeChartFillBorder
{
	private const OfficeKnownColors DEF_COLOR_INEDX = (OfficeKnownColors)77;

	private ChartAxisLineFormatRecord m_axisLine;

	private ChartAxisImpl m_parentAxis;

	private ShadowImpl m_shadow;

	protected WorkbookImpl m_parentBook;

	private ChartBorderImpl m_border;

	private ThreeDFormatImpl m_3D;

	public IOfficeChartBorder Border
	{
		get
		{
			if (m_border == null)
			{
				m_border = new ChartBorderImpl(base.Application, this);
			}
			m_border.HasLineProperties = true;
			return m_border;
		}
	}

	public IOfficeChartBorder LineProperties => Border;

	public bool HasLineProperties => m_border != null;

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

	public bool HasInterior => false;

	public IOfficeChartInterior Interior
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public IOfficeFill Fill
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public ExcelAxisLineIdentifier AxisLineType
	{
		get
		{
			return m_axisLine.LineIdentifier;
		}
		set
		{
			m_axisLine.LineIdentifier = value;
		}
	}

	protected ChartAxisImpl ParentAxis => m_parentAxis;

	public ChartGridLineImpl(IApplication application, object parent, ExcelAxisLineIdentifier axisType)
		: base(application, parent)
	{
		if (axisType != ExcelAxisLineIdentifier.MajorGridLine && axisType != ExcelAxisLineIdentifier.MinorGridLine)
		{
			throw new ArgumentException("axisType");
		}
		m_axisLine = (ChartAxisLineFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxisLineFormat);
		m_border = new ChartBorderImpl(application, this);
		m_border.ColorIndex = (OfficeKnownColors)77;
		m_border.LineWeight = OfficeChartLineWeight.Hairline;
		AxisLineType = axisType;
		m_border.AutoFormat = true;
		SetParents();
	}

	public ChartGridLineImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: base(application, parent)
	{
		Parse(data, ref iPos);
		SetParents();
	}

	private void SetParents()
	{
		m_parentAxis = (ChartAxisImpl)CommonObject.FindParent(base.Parent, typeof(ChartAxisImpl), bSubTypes: true);
		m_parentBook = (WorkbookImpl)FindParent(typeof(WorkbookImpl));
		if (m_parentBook == null)
		{
			throw new ApplicationException("Can't find parent objects");
		}
	}

	[CLSCompliant(false)]
	public virtual void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartAxisLineFormat);
		m_axisLine = (ChartAxisLineFormatRecord)biffRecordRaw;
		iPos++;
		biffRecordRaw = data[iPos];
		if (biffRecordRaw.TypeCode == TBIFFRecord.ChartLineFormat)
		{
			m_border = new ChartBorderImpl(base.Application, this, data, ref iPos);
		}
	}

	[CLSCompliant(false)]
	public virtual void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_axisLine != null)
		{
			records.Add((BiffRecordRaw)m_axisLine.Clone());
			m_border.Serialize(records);
		}
	}

	public virtual void Delete()
	{
		if (m_axisLine.LineIdentifier == ExcelAxisLineIdentifier.MajorGridLine)
		{
			m_parentAxis.HasMajorGridLines = false;
		}
		else
		{
			m_parentAxis.HasMinorGridLines = false;
		}
	}

	public virtual object Clone(object parent)
	{
		ChartGridLineImpl chartGridLineImpl = (ChartGridLineImpl)MemberwiseClone();
		chartGridLineImpl.SetParent(parent);
		chartGridLineImpl.SetParents();
		chartGridLineImpl.m_axisLine = (ChartAxisLineFormatRecord)CloneUtils.CloneCloneable(m_axisLine);
		if (m_border != null)
		{
			chartGridLineImpl.m_border = m_border.Clone(chartGridLineImpl);
		}
		if (m_shadow != null)
		{
			chartGridLineImpl.m_shadow = m_shadow.Clone(chartGridLineImpl);
		}
		if (m_3D != null)
		{
			chartGridLineImpl.m_3D = m_3D.Clone(chartGridLineImpl);
		}
		return chartGridLineImpl;
	}
}
