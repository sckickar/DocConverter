using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartDropBarImpl : CommonObject, IOfficeChartDropBar, IOfficeChartFillBorder, IFillColor
{
	private ChartDropBarRecord m_dropBar;

	private ChartBorderImpl m_lineFormat;

	private ChartInteriorImpl m_interior;

	private WorkbookImpl m_parentBook;

	private ChartFillImpl m_fill;

	private ThreeDFormatImpl m_3D;

	private ShadowImpl m_shadow;

	public bool HasInterior => m_interior != null;

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

	public bool HasLineProperties => m_lineFormat != null;

	public int Gap
	{
		get
		{
			return m_dropBar.Gap;
		}
		set
		{
			m_dropBar.Gap = (ushort)value;
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

	public IOfficeChartBorder LineProperties
	{
		get
		{
			if (m_lineFormat == null)
			{
				m_lineFormat = new ChartBorderImpl(base.Application, this);
			}
			return m_lineFormat;
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
			return m_fill;
		}
	}

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

	public ChartDropBarImpl(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
		m_dropBar = (ChartDropBarRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartDropBar);
	}

	private void SetParents()
	{
		m_parentBook = (WorkbookImpl)FindParent(typeof(WorkbookImpl));
		if (m_parentBook == null)
		{
			throw new ArgumentNullException("Cannot find parent object.");
		}
	}

	[CLSCompliant(false)]
	public void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartDropBar);
		m_dropBar = (ChartDropBarRecord)data[iPos];
		data[iPos + 1].CheckTypeCode(TBIFFRecord.Begin);
		iPos += 2;
		int num = 1;
		while (num > 0)
		{
			biffRecordRaw = data[iPos];
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.Begin:
				num++;
				iPos = BiffRecordRaw.SkipBeginEndBlock(data, iPos);
				break;
			case TBIFFRecord.End:
				num--;
				break;
			case TBIFFRecord.ChartLineFormat:
				m_lineFormat = new ChartBorderImpl(base.Application, this, (ChartLineFormatRecord)biffRecordRaw);
				break;
			case TBIFFRecord.ChartAreaFormat:
				m_interior = new ChartInteriorImpl(base.Application, this, (ChartAreaFormatRecord)biffRecordRaw);
				break;
			}
			iPos++;
		}
		iPos--;
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentException("records");
		}
		if (m_dropBar == null)
		{
			throw new ApplicationException("Exception occured inside of ChartDropBar object.");
		}
		records.Add((BiffRecordRaw)m_dropBar.Clone());
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		if (m_lineFormat != null)
		{
			m_lineFormat.Serialize(records);
		}
		if (m_interior != null && !m_interior.UseAutomaticFormat)
		{
			m_interior.Serialize(records);
		}
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
	}

	public ChartDropBarImpl Clone(object parent)
	{
		ChartDropBarImpl chartDropBarImpl = (ChartDropBarImpl)MemberwiseClone();
		chartDropBarImpl.SetParent(parent);
		chartDropBarImpl.SetParents();
		chartDropBarImpl.m_dropBar = (ChartDropBarRecord)CloneUtils.CloneCloneable(m_dropBar);
		chartDropBarImpl.m_lineFormat = (ChartBorderImpl)CloneUtils.CloneCloneable(m_lineFormat, this);
		chartDropBarImpl.m_interior = (ChartInteriorImpl)CloneUtils.CloneCloneable(m_interior, this);
		if (m_interior != null)
		{
			_ = m_interior.Pattern;
			chartDropBarImpl.Pattern = m_interior.Pattern;
		}
		if (m_fill != null)
		{
			chartDropBarImpl.m_fill = (ChartFillImpl)m_fill.Clone(this);
		}
		if (m_3D != null)
		{
			chartDropBarImpl.m_3D = m_3D.Clone(chartDropBarImpl);
		}
		if (m_shadow != null)
		{
			chartDropBarImpl.m_shadow = m_shadow.Clone(chartDropBarImpl);
		}
		return chartDropBarImpl;
	}
}
