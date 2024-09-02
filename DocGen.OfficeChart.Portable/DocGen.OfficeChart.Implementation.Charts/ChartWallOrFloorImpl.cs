using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartWallOrFloorImpl : ChartGridLineImpl, IOfficeChartWallOrFloor, IOfficeChartGridLine, IOfficeChartFillBorder, IFillColor
{
	public const int DEF_CATEGORY_LINE_COLOR = 8421504;

	public const OfficeKnownColors DEF_CATEGORY_COLOR_INDEX = OfficeKnownColors.Grey_50_percent;

	private const int DEF_VALUE_LINE_COLOR = 0;

	public const int DEF_CATEGORY_FOREGROUND_COLOR = 12632256;

	public const OfficeKnownColors DEF_CATEGORY_BACKGROUND_COLOR_INDEX = (OfficeKnownColors)79;

	public const OfficeKnownColors DEF_VALUE_BACKGROUND_COLOR_INDEX = (OfficeKnownColors)77;

	private const OfficeKnownColors DEF_VALUE_FOREGROUND_COLOR_INDEX = (OfficeKnownColors)78;

	private bool m_bWalls;

	private ChartInteriorImpl m_interior;

	private ChartImpl m_parentChart;

	private ShadowImpl m_shadow;

	private ThreeDFormatImpl m_3D;

	private ChartFillImpl m_fill;

	private bool m_shapeProperties;

	private uint m_thickness;

	private OfficeChartPictureType m_PictureUnit = OfficeChartPictureType.stretch;

	public new IOfficeChartInterior Interior
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

	public new IShadow Shadow
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

	public new bool HasShadowProperties
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

	internal bool HasShapeProperties
	{
		get
		{
			return m_shapeProperties;
		}
		set
		{
			m_shapeProperties = value;
		}
	}

	public uint Thickness
	{
		get
		{
			return m_thickness;
		}
		set
		{
			m_thickness = value;
		}
	}

	public OfficeChartPictureType PictureUnit
	{
		get
		{
			return m_PictureUnit;
		}
		set
		{
			if (value == OfficeChartPictureType.stack)
			{
				m_PictureUnit = value;
			}
			else
			{
				m_PictureUnit = OfficeChartPictureType.stretch;
			}
		}
	}

	public new IThreeDFormat ThreeD
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

	public new bool Has3dProperties
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

	public new IOfficeFill Fill
	{
		get
		{
			IsAutomaticFormat = false;
			return m_fill;
		}
	}

	public new bool HasInterior => m_interior != null;

	private bool IsWall => m_bWalls;

	public ChartColor ForeGroundColorObject => m_interior.ForegroundColorObject;

	public ChartColor BackGroundColorObject => m_interior.BackgroundColorObject;

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

	public ChartWallOrFloorImpl(IApplication application, object parent, bool bWalls)
		: base(application, parent, ExcelAxisLineIdentifier.MajorGridLine)
	{
		base.AxisLineType = ExcelAxisLineIdentifier.WallsOrFloor;
		m_interior = new ChartInteriorImpl(application, this);
		bool bIsInteriorGray = m_parentBook.Version == OfficeVersion.Excel97to2003;
		m_interior.InitForFrameFormat(bIsAutoSize: false, bIs3DChart: true, bIsInteriorGray, !bWalls);
		m_bWalls = bWalls;
		m_parentChart = (ChartImpl)FindParent(typeof(ChartImpl));
		m_fill = new ChartFillImpl(application, this);
		if (m_parentChart == null)
		{
			throw new ApplicationException("Can't find parent objects");
		}
		SetToDefault();
	}

	public ChartWallOrFloorImpl(IApplication application, object parent, bool bWalls, IList<BiffRecordRaw> data, ref int iPos)
		: base(application, parent, data, ref iPos)
	{
		base.AxisLineType = ExcelAxisLineIdentifier.WallsOrFloor;
		m_bWalls = bWalls;
		m_parentChart = (ChartImpl)FindParent(typeof(ChartImpl));
		if (m_fill == null)
		{
			m_fill = new ChartFillImpl(application, this);
		}
		if (m_parentChart == null)
		{
			throw new ApplicationException("Can't find parent objects");
		}
	}

	[CLSCompliant(false)]
	public override void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		m_interior = null;
		base.Parse(data, ref iPos);
		if (base.AxisLineType != ExcelAxisLineIdentifier.WallsOrFloor)
		{
			throw new ParseException("Bad axis line type");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		if (biffRecordRaw.TypeCode == TBIFFRecord.ChartAreaFormat)
		{
			m_interior = new ChartInteriorImpl(base.Application, this, data, ref iPos);
		}
		biffRecordRaw = data[iPos];
		if (biffRecordRaw.TypeCode == TBIFFRecord.ChartGelFrame)
		{
			m_fill = new ChartFillImpl(base.Application, this, (ChartGelFrameRecord)biffRecordRaw);
			iPos++;
		}
		int num = 1;
		while (num > 0)
		{
			biffRecordRaw = data[iPos];
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.Begin:
				num++;
				break;
			case TBIFFRecord.End:
				num--;
				break;
			}
			iPos++;
		}
		iPos--;
	}

	[CLSCompliant(false)]
	public override void Serialize(OffsetArrayList records)
	{
		base.Serialize(records);
		if (m_interior != null)
		{
			m_interior.Serialize(records);
		}
		m_fill.Serialize(records);
	}

	public override void Delete()
	{
		if (m_bWalls)
		{
			m_parentChart.Walls = new ChartWallOrFloorImpl(base.Application, m_parentChart, bWalls: true);
			m_parentChart.SideWall = new ChartWallOrFloorImpl(base.Application, m_parentChart, bWalls: true);
		}
		else
		{
			m_parentChart.Floor = new ChartWallOrFloorImpl(base.Application, m_parentChart, bWalls: false);
		}
	}

	public void SetToDefault()
	{
		if (m_bWalls)
		{
			SetToDefaultCategoryLine();
			SetToDefaultCategoryArea();
		}
		else
		{
			SetToDefaultValueLine();
			SetToDefaultValueArea();
		}
	}

	private void SetToDefaultCategoryLine()
	{
		base.Border.LineWeight = OfficeChartLineWeight.Narrow;
		base.Border.ColorIndex = OfficeKnownColors.Grey_50_percent;
	}

	private void SetToDefaultValueLine()
	{
		if (m_parentBook.Version == OfficeVersion.Excel97to2003)
		{
			base.Border.ColorIndex = (OfficeKnownColors)77;
		}
		else
		{
			base.Border.ColorIndex = OfficeKnownColors.Grey_25_percent;
		}
	}

	private void SetToDefaultCategoryArea()
	{
		if (m_parentBook.Version == OfficeVersion.Excel97to2003)
		{
			m_interior.Pattern = OfficePattern.Solid;
			m_interior.ForegroundColorObject.SetIndexed(OfficeKnownColors.Grey_25_percent);
			m_interior.BackgroundColorObject.SetIndexed((OfficeKnownColors)79);
		}
		else
		{
			m_interior.Pattern = OfficePattern.None;
		}
	}

	private void SetToDefaultValueArea()
	{
		Interior.UseAutomaticFormat = true;
	}

	public override object Clone(object parent)
	{
		ChartWallOrFloorImpl chartWallOrFloorImpl = (ChartWallOrFloorImpl)base.Clone(parent);
		if (m_interior != null)
		{
			chartWallOrFloorImpl.m_interior = m_interior.Clone(chartWallOrFloorImpl);
		}
		if (m_3D != null)
		{
			chartWallOrFloorImpl.m_3D = m_3D.Clone(chartWallOrFloorImpl);
		}
		if (m_shadow != null)
		{
			chartWallOrFloorImpl.m_shadow = m_shadow.Clone(chartWallOrFloorImpl);
		}
		if (m_fill != null)
		{
			chartWallOrFloorImpl.m_fill = (ChartFillImpl)m_fill.Clone(chartWallOrFloorImpl);
		}
		chartWallOrFloorImpl.m_PictureUnit = m_PictureUnit;
		return chartWallOrFloorImpl;
	}
}
