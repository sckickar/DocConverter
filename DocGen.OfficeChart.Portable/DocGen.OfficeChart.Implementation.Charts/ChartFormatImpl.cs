using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Exceptions;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartFormatImpl : CommonObject, IOfficeChartFormat, ICloneParent
{
	public const int DEF_BAR_STACKED = -65436;

	private const int DEF_SERIES_NUMBER = 65533;

	private ChartChartFormatRecord m_chartChartFormat;

	private BiffRecordRaw m_serieFormat;

	private Chart3DRecord m_chart3D;

	private ChartFormatLinkRecord m_formatLink;

	private ChartDataLabelsRecord m_dataLabels;

	private ChartChartLineRecord m_chartChartLine;

	private ChartSerieDataFormatImpl m_dataFormat;

	private ChartDropBarImpl m_firstDropBar;

	private ChartDropBarImpl m_secondDropBar;

	private ChartSeriesListRecord m_seriesList;

	private ChartImpl m_chart;

	private ChartParentAxisImpl m_parentAxis;

	private ChartBorderImpl m_serieLine;

	private ChartBorderImpl m_highlowLine;

	private ChartBorderImpl m_dropLine;

	private bool m_isChartExType;

	internal bool IsVeryColor
	{
		get
		{
			return IsVaryColor;
		}
		set
		{
			IsVaryColor = value;
		}
	}

	public bool IsVaryColor
	{
		get
		{
			return ChartChartFormatRecord.IsVaryColor;
		}
		set
		{
			ChartChartFormatRecord.IsVaryColor = value;
		}
	}

	public IOfficeChartSerieDataFormat SerieDataFormat => DataFormat;

	public int Overlap
	{
		get
		{
			if (m_chart.IsChart3D)
			{
				throw new NotSupportedException("This property is not supported in 3d chart types");
			}
			return BarRecord.Overlap;
		}
		set
		{
			if (!m_chart.ParentWorkbook.IsWorkbookOpening && m_chart.IsChart3D)
			{
				throw new NotSupportedException("This property is not supported in 3d chart types");
			}
			if ((!m_chart.ParentWorkbook.IsWorkbookOpening && value < -100) || value > 100)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			BarRecord.Overlap = value;
		}
	}

	public int GapWidth
	{
		get
		{
			if (m_serieFormat.TypeCode == TBIFFRecord.ChartLine)
			{
				return LineRecord.Gapwidth;
			}
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartBar)
			{
				return BoppopRecord.Gap;
			}
			return BarRecord.CategoriesSpace;
		}
		set
		{
			if (m_serieFormat.TypeCode == TBIFFRecord.ChartBar)
			{
				if (value < 0 || value > 500)
				{
					throw new ArgumentOutOfRangeException("GapWidth");
				}
				BarRecord.CategoriesSpace = (ushort)value;
			}
			else if (m_serieFormat.TypeCode == TBIFFRecord.ChartLine)
			{
				LineRecord.Gapwidth = (ushort)value;
			}
			else
			{
				if (value < 0 || value > 500)
				{
					throw new ArgumentOutOfRangeException("GapWidth");
				}
				BoppopRecord.Gap = (ushort)value;
			}
		}
	}

	public bool IsHorizontalBar
	{
		get
		{
			return BarRecord.IsHorizontalBar;
		}
		set
		{
			BarRecord.IsHorizontalBar = value;
		}
	}

	public bool StackValuesBar
	{
		get
		{
			return BarRecord.StackValues;
		}
		set
		{
			BarRecord.StackValues = value;
		}
	}

	public bool ShowAsPercentsBar
	{
		get
		{
			return BarRecord.ShowAsPercents;
		}
		set
		{
			BarRecord.ShowAsPercents = value;
		}
	}

	public bool HasShadowBar
	{
		get
		{
			return BarRecord.HasShadow;
		}
		set
		{
			BarRecord.HasShadow = value;
		}
	}

	public bool StackValuesLine
	{
		get
		{
			return LineRecord.StackValues;
		}
		set
		{
			LineRecord.StackValues = value;
		}
	}

	public bool ShowAsPercentsLine
	{
		get
		{
			return LineRecord.ShowAsPercents;
		}
		set
		{
			LineRecord.ShowAsPercents = value;
		}
	}

	public bool HasShadowLine
	{
		get
		{
			return LineRecord.HasShadow;
		}
		set
		{
			LineRecord.HasShadow = value;
		}
	}

	public int FirstSliceAngle
	{
		get
		{
			return PieRecord.StartAngle;
		}
		set
		{
			if (value < 0 || value > 360)
			{
				throw new ArgumentOutOfRangeException("StartAngle");
			}
			PieRecord.StartAngle = (ushort)value;
		}
	}

	public int DoughnutHoleSize
	{
		get
		{
			return PieRecord.DonutHoleSize;
		}
		set
		{
			if (value < 0 || value > 90)
			{
				throw new ArgumentOutOfRangeException("DonutHoleSize");
			}
			if (!m_chart.TypeChanging)
			{
				OfficeChartType chartType = m_chart.ChartType;
				if (chartType != OfficeChartType.Doughnut && chartType != OfficeChartType.Doughnut_Exploded)
				{
					throw new NotSupportedException("This property is supported only in doughnut chart types");
				}
			}
			PieRecord.DonutHoleSize = (ushort)value;
		}
	}

	public bool HasShadowPie
	{
		get
		{
			return PieRecord.HasShadow;
		}
		set
		{
			PieRecord.HasShadow = value;
		}
	}

	public bool ShowLeaderLines
	{
		get
		{
			if (PieRecord != null)
			{
				return PieRecord.ShowLeaderLines;
			}
			if (BoppopRecord != null)
			{
				return BoppopRecord.ShowLeaderLines;
			}
			return true;
		}
		set
		{
			if (PieRecord != null)
			{
				PieRecord.ShowLeaderLines = value;
			}
			else if (BoppopRecord != null)
			{
				BoppopRecord.ShowLeaderLines = value;
			}
		}
	}

	public int BubbleScale
	{
		get
		{
			return ScatterRecord.BubleSizeRation;
		}
		set
		{
			if (value < 0 || value > 300)
			{
				throw new ArgumentOutOfRangeException("BubleSizeScale");
			}
			if (!m_chart.TypeChanging)
			{
				OfficeChartType chartType = m_chart.ChartType;
				if (chartType != OfficeChartType.Bubble && chartType != OfficeChartType.Bubble_3D)
				{
					throw new NotSupportedException("This property supported only in bubble chart types.");
				}
			}
			ScatterRecord.BubleSizeRation = (ushort)value;
		}
	}

	public ChartBubbleSize SizeRepresents
	{
		get
		{
			return ScatterRecord.BubleSize;
		}
		set
		{
			if (!m_chart.TypeChanging)
			{
				OfficeChartType chartType = m_chart.ChartType;
				if (chartType != OfficeChartType.Bubble && chartType != OfficeChartType.Bubble_3D)
				{
					throw new NotSupportedException("This property is supported only in bubble chart types.");
				}
			}
			ScatterRecord.BubleSize = value;
		}
	}

	public bool IsBubbles
	{
		get
		{
			return ScatterRecord.IsBubbles;
		}
		set
		{
			ScatterRecord.IsBubbles = value;
		}
	}

	public bool ShowNegativeBubbles
	{
		get
		{
			return ScatterRecord.IsShowNegBubbles;
		}
		set
		{
			OfficeChartType chartType = m_chart.ChartType;
			if (chartType != OfficeChartType.Bubble && chartType != OfficeChartType.Bubble_3D)
			{
				throw new NotSupportedException("This property is supported only in bubble chart types.");
			}
			ScatterRecord.IsShowNegBubbles = value;
		}
	}

	public bool HasShadowScatter
	{
		get
		{
			return ScatterRecord.HasShadow;
		}
		set
		{
			ScatterRecord.HasShadow = value;
		}
	}

	public bool IsStacked
	{
		get
		{
			return AreaRecord.IsStacked;
		}
		set
		{
			AreaRecord.IsStacked = value;
		}
	}

	public bool IsCategoryBrokenDown
	{
		get
		{
			return AreaRecord.IsCategoryBrokenDown;
		}
		set
		{
			AreaRecord.IsCategoryBrokenDown = value;
		}
	}

	public bool IsAreaShadowed
	{
		get
		{
			return AreaRecord.IsAreaShadowed;
		}
		set
		{
			AreaRecord.IsAreaShadowed = value;
		}
	}

	public bool IsFillSurface
	{
		get
		{
			return SurfaceRecord.IsFillSurface;
		}
		set
		{
			SurfaceRecord.IsFillSurface = value;
		}
	}

	public bool Is3DPhongShade
	{
		get
		{
			return SurfaceRecord.Is3DPhongShade;
		}
		set
		{
			SurfaceRecord.Is3DPhongShade = value;
		}
	}

	public bool HasShadowRadar
	{
		get
		{
			return RadarRecord.HasShadow;
		}
		set
		{
			RadarRecord.HasShadow = value;
		}
	}

	public bool HasRadarAxisLabels
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartRadarArea)
			{
				return RadarAreaRecord.IsRadarAxisLabel;
			}
			return RadarRecord.IsRadarAxisLabel;
		}
		set
		{
			if (m_serieFormat.TypeCode == TBIFFRecord.ChartRadarArea)
			{
				RadarAreaRecord.IsRadarAxisLabel = value;
			}
			else
			{
				RadarRecord.IsRadarAxisLabel = value;
			}
		}
	}

	public OfficePieType PieChartType
	{
		get
		{
			return BoppopRecord.PieChartType;
		}
		set
		{
			BoppopRecord.PieChartType = value;
		}
	}

	public bool UseDefaultSplitValue
	{
		get
		{
			return BoppopRecord.UseDefaultSplitValue;
		}
		set
		{
			BoppopRecord.UseDefaultSplitValue = value;
		}
	}

	public OfficeSplitType SplitType
	{
		get
		{
			return BoppopRecord.ChartSplitType;
		}
		set
		{
			BoppopRecord.ChartSplitType = value;
		}
	}

	public int SplitValue
	{
		get
		{
			return BoppopRecord.SplitPosition;
		}
		set
		{
			if (SplitType == OfficeSplitType.Percent)
			{
				BoppopRecord.SplitPercent = (ushort)value;
			}
			else
			{
				BoppopRecord.SplitPosition = (ushort)value;
			}
			UseDefaultSplitValue = false;
		}
	}

	public int SplitPercent
	{
		get
		{
			return BoppopRecord.SplitPercent;
		}
		set
		{
			BoppopRecord.SplitPercent = (ushort)value;
		}
	}

	public int PieSecondSize
	{
		get
		{
			return BoppopRecord.Pie2Size;
		}
		set
		{
			if (value < 5 || value > 200)
			{
				throw new ArgumentOutOfRangeException("PieSecondSize");
			}
			BoppopRecord.Pie2Size = (ushort)value;
		}
	}

	public int Gap
	{
		get
		{
			return BoppopRecord.Gap;
		}
		set
		{
			BoppopRecord.Gap = (ushort)value;
		}
	}

	public int NumSplitValue
	{
		get
		{
			return BoppopRecord.NumSplitValue;
		}
		set
		{
			BoppopRecord.NumSplitValue = value;
		}
	}

	public bool HasShadowBoppop
	{
		get
		{
			return BoppopRecord.HasShadow;
		}
		set
		{
			BoppopRecord.HasShadow = value;
		}
	}

	public bool IsSeriesName
	{
		get
		{
			return DataLabelsRecord.IsSeriesName;
		}
		set
		{
			DataLabelsRecord.IsSeriesName = value;
		}
	}

	public bool IsCategoryName
	{
		get
		{
			return DataLabelsRecord.IsCategoryName;
		}
		set
		{
			DataLabelsRecord.IsCategoryName = value;
		}
	}

	public bool IsValue
	{
		get
		{
			return DataLabelsRecord.IsValue;
		}
		set
		{
			DataLabelsRecord.IsValue = value;
		}
	}

	public bool IsPercentage
	{
		get
		{
			return DataLabelsRecord.IsPercentage;
		}
		set
		{
			DataLabelsRecord.IsPercentage = value;
		}
	}

	public bool IsBubbleSize
	{
		get
		{
			return DataLabelsRecord.IsBubbleSize;
		}
		set
		{
			DataLabelsRecord.IsBubbleSize = value;
		}
	}

	public int DelimiterLength => DataLabelsRecord.DelimiterLength;

	public string Delimiter
	{
		get
		{
			return DataLabelsRecord.Delimiter;
		}
		set
		{
			DataLabelsRecord.Delimiter = value;
		}
	}

	public ExcelDropLineStyle LineStyle
	{
		get
		{
			return DropLineStyle;
		}
		set
		{
			DropLineStyle = value;
		}
	}

	public ExcelDropLineStyle DropLineStyle
	{
		get
		{
			if (m_chartChartLine == null)
			{
				m_chartChartLine = (ChartChartLineRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartChartLine);
			}
			return m_chartChartLine.LineStyle;
		}
		set
		{
			SetDropLineStyle(value);
			switch (value)
			{
			case ExcelDropLineStyle.Drop:
				HasDropLines = true;
				HasHighLowLines = false;
				HasSeriesLines = false;
				break;
			case ExcelDropLineStyle.HiLow:
				HasDropLines = false;
				HasHighLowLines = true;
				HasSeriesLines = false;
				break;
			case ExcelDropLineStyle.Series:
				HasDropLines = false;
				HasHighLowLines = false;
				HasSeriesLines = true;
				break;
			}
		}
	}

	public IOfficeChartDropBar FirstDropBar
	{
		get
		{
			if (m_firstDropBar == null)
			{
				m_firstDropBar = new ChartDropBarImpl(base.Application, this);
			}
			return m_firstDropBar;
		}
	}

	public IOfficeChartDropBar SecondDropBar
	{
		get
		{
			if (m_secondDropBar == null)
			{
				m_secondDropBar = new ChartDropBarImpl(base.Application, this);
			}
			return m_secondDropBar;
		}
	}

	public IOfficeChartBorder PieSeriesLine
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartBoppop && m_chart.ChartType != OfficeChartType.Column_Stacked && m_chart.ChartType != OfficeChartType.Column_Stacked_100 && m_chart.ChartType != OfficeChartType.Bar_Stacked && m_chart.ChartType != OfficeChartType.Bar_Stacked_100)
			{
				throw new ArgumentNullException("This property is not supported in this chart type");
			}
			if (m_serieLine == null)
			{
				m_serieLine = new ChartBorderImpl(base.Application, this);
			}
			return m_serieLine;
		}
		internal set
		{
			if (HasSeriesLines)
			{
				m_serieLine = (ChartBorderImpl)value;
			}
		}
	}

	public int Rotation
	{
		get
		{
			if (m_chart3D != null)
			{
				return Chart3DRecord.RotationAngle;
			}
			return 20;
		}
		set
		{
			if (value < 0 || value > 360)
			{
				throw new ArgumentOutOfRangeException("Rotation");
			}
			Chart3DRecord.RotationAngle = (ushort)value;
		}
	}

	public bool IsDefaultRotation => Chart3DRecord.IsDefaultRotation;

	public int Elevation
	{
		get
		{
			if (m_chart3D != null)
			{
				return Chart3DRecord.ElevationAngle;
			}
			return 15;
		}
		set
		{
			if (value < -90 || value > 90)
			{
				throw new ArgumentOutOfRangeException("Elevation");
			}
			Chart3DRecord.ElevationAngle = (short)value;
		}
	}

	public bool IsDefaultElevation => Chart3DRecord.IsDefaultElevation;

	public int Perspective
	{
		get
		{
			if (m_chart3D != null)
			{
				return Chart3DRecord.DistanceFromEye;
			}
			return 30;
		}
		set
		{
			if (value < 0 || value > 100)
			{
				throw new ArgumentOutOfRangeException("Elevation");
			}
			Chart3DRecord.DistanceFromEye = (ushort)value;
			Chart3DRecord.IsPerspective = true;
		}
	}

	public int HeightPercent
	{
		get
		{
			if (m_chart3D != null)
			{
				return Chart3DRecord.Height;
			}
			return 100;
		}
		set
		{
			if (value < 5 || value > 500)
			{
				throw new ArgumentOutOfRangeException("Elevation");
			}
			Chart3DRecord.Height = (ushort)value;
			Chart3DRecord.IsAutoScaled = false;
		}
	}

	public int DepthPercent
	{
		get
		{
			if (m_chart3D != null)
			{
				return Chart3DRecord.Depth;
			}
			return 100;
		}
		set
		{
			if (value < 20 || value > 2000)
			{
				throw new ArgumentOutOfRangeException("DepthPercent");
			}
			Chart3DRecord.Depth = (ushort)value;
		}
	}

	public int GapDepth
	{
		get
		{
			if (m_chart3D != null)
			{
				return Chart3DRecord.SeriesSpace;
			}
			return 150;
		}
		set
		{
			if (value < 0 || value > 500)
			{
				throw new ArgumentOutOfRangeException("GapDepth");
			}
			Chart3DRecord.SeriesSpace = (ushort)value;
		}
	}

	public bool RightAngleAxes
	{
		get
		{
			if (m_chart3D != null)
			{
				return !Chart3DRecord.IsPerspective;
			}
			return true;
		}
		set
		{
			Chart3DRecord.IsPerspective = !value;
		}
	}

	public bool IsClustered
	{
		get
		{
			return Chart3DRecord.IsClustered;
		}
		set
		{
			Chart3DRecord.IsClustered = value;
		}
	}

	internal bool IsChartExType
	{
		get
		{
			return m_isChartExType;
		}
		set
		{
			m_isChartExType = value;
		}
	}

	public bool AutoScaling
	{
		get
		{
			if (m_chart3D != null)
			{
				return Chart3DRecord.IsAutoScaled;
			}
			return true;
		}
		set
		{
			Chart3DRecord.IsAutoScaled = value;
		}
	}

	public bool WallsAndGridlines2D
	{
		get
		{
			if (m_chart3D != null)
			{
				return Chart3DRecord.Is2DWalls;
			}
			return false;
		}
		set
		{
			Chart3DRecord.Is2DWalls = value;
		}
	}

	private ChartBarRecord BarRecord
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartBar)
			{
				throw new NotSupportedException("This property is not suported in current chart type.");
			}
			return m_serieFormat as ChartBarRecord;
		}
	}

	private ChartLineRecord LineRecord
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartLine)
			{
				throw new NotSupportedException("This property is not suported in current chart type.");
			}
			return m_serieFormat as ChartLineRecord;
		}
	}

	private ChartPieRecord PieRecord
	{
		get
		{
			if (m_serieFormat.TypeCode == TBIFFRecord.ChartBoppop)
			{
				return null;
			}
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartPie && !(m_chart.Workbook as WorkbookImpl).IsWorkbookOpening)
			{
				throw new NotSupportedException("This property is not suported in current chart type.");
			}
			return m_serieFormat as ChartPieRecord;
		}
	}

	private ChartScatterRecord ScatterRecord
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartScatter)
			{
				throw new NotSupportedException("This property is not suported in current chart type.");
			}
			return m_serieFormat as ChartScatterRecord;
		}
	}

	private ChartAreaRecord AreaRecord
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartArea)
			{
				throw new NotSupportedException("This property is not suported in current chart type.");
			}
			return m_serieFormat as ChartAreaRecord;
		}
	}

	private ChartSurfaceRecord SurfaceRecord
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartSurface)
			{
				throw new NotSupportedException("This property is not suported in current chart type.");
			}
			return m_serieFormat as ChartSurfaceRecord;
		}
	}

	private ChartRadarRecord RadarRecord
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartRadar)
			{
				throw new NotSupportedException("This property is not suported in current chart type.");
			}
			return m_serieFormat as ChartRadarRecord;
		}
	}

	private ChartRadarAreaRecord RadarAreaRecord
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartRadarArea)
			{
				throw new NotSupportedException("This property is not suported in current chart type.");
			}
			return m_serieFormat as ChartRadarAreaRecord;
		}
	}

	private ChartBoppopRecord BoppopRecord
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartBoppop)
			{
				throw new NotSupportedException("This property is not suported in current chart type.");
			}
			return m_serieFormat as ChartBoppopRecord;
		}
	}

	private ChartDataLabelsRecord DataLabelsRecord
	{
		get
		{
			if (m_dataLabels == null)
			{
				m_dataLabels = (ChartDataLabelsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartDataLabels);
			}
			return m_dataLabels;
		}
	}

	private ChartSerieDataFormatImpl DataFormat
	{
		get
		{
			if (m_dataFormat == null)
			{
				m_dataFormat = new ChartSerieDataFormatImpl(base.Application, this);
			}
			return m_dataFormat;
		}
	}

	private ChartChartFormatRecord ChartChartFormatRecord
	{
		get
		{
			if (m_chartChartFormat == null)
			{
				m_chartChartFormat = (ChartChartFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartChartFormat);
			}
			return m_chartChartFormat;
		}
	}

	private Chart3DRecord Chart3DRecord
	{
		get
		{
			if (m_chart3D == null)
			{
				m_chart3D = (Chart3DRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3D);
			}
			return m_chart3D;
		}
	}

	public bool IsPrimaryAxis => m_parentAxis.IsPrimary;

	public bool IsChartChartLine => m_chartChartLine != null;

	public bool IsChartLineFormat
	{
		get
		{
			if (m_highlowLine == null && m_dropLine == null)
			{
				return m_serieLine != null;
			}
			return true;
		}
	}

	public bool IsDropBar => m_firstDropBar != null;

	[CLSCompliant(false)]
	public BiffRecordRaw SerieFormat
	{
		get
		{
			if (m_serieFormat == null)
			{
				throw new ArgumentNullException("m_serieFormat");
			}
			return m_serieFormat;
		}
	}

	public int DrawingZOrder
	{
		get
		{
			return ChartChartFormatRecord.DrawingZOrder;
		}
		set
		{
			if (ChartChartFormatRecord.DrawingZOrder != value)
			{
				ChartChartFormatRecord.DrawingZOrder = (ushort)value;
			}
		}
	}

	public TBIFFRecord FormatRecordType => m_serieFormat.TypeCode;

	public bool Is3D => m_chart3D != null;

	public ChartSerieDataFormatImpl DataFormatOrNull => m_dataFormat;

	public bool IsMarker
	{
		get
		{
			if (m_dataFormat != null)
			{
				return m_dataFormat.IsMarker;
			}
			return true;
		}
	}

	public bool IsLine
	{
		get
		{
			if (m_dataFormat != null)
			{
				return m_dataFormat.IsLine;
			}
			return true;
		}
	}

	public bool IsSmoothed
	{
		get
		{
			if (m_dataFormat != null)
			{
				return m_dataFormat.IsSmoothed;
			}
			return false;
		}
	}

	internal IOfficeChartBorder HighLowLineProperties
	{
		get
		{
			return HighLowLines;
		}
		set
		{
			HighLowLines = value;
		}
	}

	public IOfficeChartBorder HighLowLines
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartLine || Is3D)
			{
				throw new ArgumentNullException("This property is not supported in this chart type");
			}
			if (m_highlowLine == null)
			{
				m_highlowLine = new ChartBorderImpl(base.Application, this);
			}
			return m_highlowLine;
		}
		internal set
		{
			if (HasHighLowLines)
			{
				m_highlowLine = (ChartBorderImpl)value;
			}
		}
	}

	public IOfficeChartBorder DropLines
	{
		get
		{
			if (m_serieFormat.TypeCode != TBIFFRecord.ChartLine && m_serieFormat.TypeCode != TBIFFRecord.ChartArea)
			{
				throw new ArgumentNullException("This property is not supported in this chart type");
			}
			if (m_dropLine == null)
			{
				m_dropLine = new ChartBorderImpl(base.Application, this);
			}
			return m_dropLine;
		}
		internal set
		{
			if (HasDropLines)
			{
				m_dropLine = (ChartBorderImpl)value;
			}
		}
	}

	public bool HasDropLines
	{
		get
		{
			if (m_chartChartLine != null)
			{
				return m_chartChartLine.HasDropLine;
			}
			return false;
		}
		set
		{
			if (m_chartChartLine != null)
			{
				m_chartChartLine.HasDropLine = value;
			}
			if (value)
			{
				SetDropLineStyle(ExcelDropLineStyle.Drop);
			}
		}
	}

	public bool HasHighLowLines
	{
		get
		{
			if (m_chartChartLine != null)
			{
				return m_chartChartLine.HasHighLowLine;
			}
			return false;
		}
		set
		{
			if (m_chartChartLine != null)
			{
				m_chartChartLine.HasHighLowLine = value;
			}
			if (value)
			{
				SetDropLineStyle(ExcelDropLineStyle.HiLow);
			}
		}
	}

	public bool HasSeriesLines
	{
		get
		{
			if (m_chartChartLine != null)
			{
				return m_chartChartLine.HasSeriesLine;
			}
			return false;
		}
		set
		{
			if (m_chartChartLine != null)
			{
				m_chartChartLine.HasSeriesLine = value;
			}
			if (value)
			{
				SetDropLineStyle(ExcelDropLineStyle.Series);
			}
		}
	}

	public ChartFormatImpl(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
		m_chartChartFormat = (ChartChartFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartChartFormat);
		m_serieFormat = BiffRecordFactory.GetRecord(TBIFFRecord.ChartBar);
	}

	public void SetParents()
	{
		m_parentAxis = (ChartParentAxisImpl)FindParent(typeof(ChartParentAxisImpl));
		if (m_parentAxis == null)
		{
			throw new ArgumentNullException("Cannot find parent axis object.");
		}
		m_chart = m_parentAxis.m_parentChart;
	}

	[CLSCompliant(false)]
	public void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartChartFormat);
		m_chartChartFormat = (ChartChartFormatRecord)data[iPos];
		iPos++;
		biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.Begin);
		iPos++;
		int num = 1;
		int num2 = 0;
		while (num != 0)
		{
			biffRecordRaw = data[iPos];
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.Begin:
				iPos = BiffRecordRaw.SkipBeginEndBlock(data, iPos) - 1;
				break;
			case TBIFFRecord.End:
				num--;
				break;
			case TBIFFRecord.ChartBar:
			case TBIFFRecord.ChartLine:
			case TBIFFRecord.ChartPie:
			case TBIFFRecord.ChartArea:
			case TBIFFRecord.ChartScatter:
			case TBIFFRecord.ChartRadar:
			case TBIFFRecord.ChartSurface:
			case TBIFFRecord.ChartRadarArea:
			case TBIFFRecord.ChartBoppop:
				m_serieFormat = biffRecordRaw;
				break;
			case TBIFFRecord.Chart3D:
				m_chart3D = (Chart3DRecord)biffRecordRaw;
				break;
			case TBIFFRecord.ChartFormatLink:
				m_formatLink = (ChartFormatLinkRecord)biffRecordRaw;
				break;
			case TBIFFRecord.ChartLegend:
				m_chart.ParseLegend(data, ref iPos);
				iPos--;
				break;
			case TBIFFRecord.ChartDataFormat:
				m_dataFormat = new ChartSerieDataFormatImpl(base.Application, this);
				iPos = m_dataFormat.Parse(data, iPos) - 1;
				break;
			case TBIFFRecord.ChartDataLabels:
				m_dataLabels = (ChartDataLabelsRecord)biffRecordRaw;
				break;
			case TBIFFRecord.ChartChartLine:
				if (DropLineStyle == ExcelDropLineStyle.Series)
				{
					m_serieLine = new ChartBorderImpl(base.Application, this, (ChartLineFormatRecord)biffRecordRaw);
				}
				else if (DropLineStyle == ExcelDropLineStyle.HiLow)
				{
					m_highlowLine = new ChartBorderImpl(base.Application, this, (ChartLineFormatRecord)biffRecordRaw);
				}
				else if (DropLineStyle == ExcelDropLineStyle.Drop)
				{
					m_dropLine = new ChartBorderImpl(base.Application, this, (ChartLineFormatRecord)biffRecordRaw);
				}
				break;
			case TBIFFRecord.ChartLineFormat:
				m_serieLine = new ChartBorderImpl(base.Application, this, (ChartLineFormatRecord)biffRecordRaw);
				break;
			case TBIFFRecord.ChartDropBar:
			{
				if (num2 > 1)
				{
					throw new ParseException("Find more then two ChartBarRecords.");
				}
				ChartDropBarImpl chartDropBarImpl = new ChartDropBarImpl(base.Application, this);
				chartDropBarImpl.Parse(data, ref iPos);
				if (num2 == 0)
				{
					m_firstDropBar = chartDropBarImpl;
				}
				else
				{
					m_secondDropBar = chartDropBarImpl;
				}
				num2++;
				break;
			}
			case TBIFFRecord.ChartSeriesList:
				m_seriesList = (ChartSeriesListRecord)biffRecordRaw;
				break;
			}
			iPos++;
		}
		if (m_chartChartLine != null && m_chartChartLine.LineStyle == ExcelDropLineStyle.HiLow && m_dataFormat != null && m_dataFormat.HasLineProperties && !m_dataFormat.LineProperties.AutoFormat && m_dataFormat.LineProperties.LinePattern == OfficeChartLinePattern.None)
		{
			m_chart.IsStock = true;
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		records.Add((BiffRecordRaw)m_chartChartFormat.Clone());
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		records.Add((BiffRecordRaw)m_serieFormat.Clone());
		if (m_formatLink != null)
		{
			records.Add((BiffRecordRaw)m_formatLink.Clone());
		}
		if (m_seriesList != null)
		{
			records.Add((BiffRecordRaw)m_seriesList.Clone());
		}
		if (m_chart3D != null)
		{
			records.Add((Chart3DRecord)m_chart3D.Clone());
		}
		if (DrawingZOrder == 0)
		{
			m_chart.SerializeLegend(records);
		}
		if (m_firstDropBar != null)
		{
			m_firstDropBar.Serialize(records);
		}
		if (m_secondDropBar != null)
		{
			m_secondDropBar.Serialize(records);
		}
		if (m_chartChartLine != null)
		{
			records.Add((BiffRecordRaw)m_chartChartLine.Clone());
		}
		if (m_serieLine != null)
		{
			m_serieLine.Serialize(records);
		}
		if (m_highlowLine != null)
		{
			m_highlowLine.Serialize(records);
		}
		if (m_dropLine != null)
		{
			m_dropLine.Serialize(records);
		}
		if (m_dataFormat != null)
		{
			m_dataFormat.Serialize(records);
		}
		if (m_dataLabels != null)
		{
			records.Add((ChartDataLabelsRecord)m_dataLabels.Clone());
		}
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
	}

	public static string GetStartSerieType(OfficeChartType type)
	{
		if (type == OfficeChartType.PieOfPie)
		{
			return "Pie";
		}
		string text = type.ToString();
		int num = text.IndexOf('_');
		if (num == -1)
		{
			return text;
		}
		return text.Substring(0, num);
	}

	public void ChangeChartType(OfficeChartType type, bool isSeriesCreation)
	{
		ChangeSerieType(type, isSeriesCreation);
	}

	private void SetDropLineStyle(ExcelDropLineStyle value)
	{
		if (m_chartChartLine == null)
		{
			m_chartChartLine = (ChartChartLineRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartChartLine);
		}
		m_chartChartLine.LineStyle = value;
	}

	private void SetNullForAllRecords()
	{
		m_chart3D = null;
		m_chartChartLine = null;
		m_serieLine = null;
		m_highlowLine = null;
		m_dropLine = null;
		m_dataFormat = null;
		m_dataLabels = null;
		m_firstDropBar = null;
		m_secondDropBar = null;
		m_seriesList = null;
	}

	private void ChangeChartStockLine()
	{
		OfficeChartType destinationType = m_chart.DestinationType;
		m_chart.DestinationType = OfficeChartType.Line;
		m_serieFormat = BiffRecordFactory.GetRecord(TBIFFRecord.ChartLine);
		m_serieLine = new ChartBorderImpl(base.Application, this);
		m_highlowLine = new ChartBorderImpl(base.Application, this);
		m_dropLine = new ChartBorderImpl(base.Application, this);
		m_serieLine.LineWeight = OfficeChartLineWeight.Hairline;
		m_highlowLine.LineWeight = OfficeChartLineWeight.Hairline;
		m_dropLine.LineWeight = OfficeChartLineWeight.Hairline;
		m_serieLine.ColorIndex = (OfficeKnownColors)79;
		m_highlowLine.ColorIndex = (OfficeKnownColors)79;
		m_dropLine.ColorIndex = (OfficeKnownColors)79;
		DropLineStyle = ExcelDropLineStyle.HiLow;
		HasHighLowLines = true;
		IOfficeChartBorder lineProperties = SerieDataFormat.LineProperties;
		lineProperties.LineWeight = OfficeChartLineWeight.Hairline;
		lineProperties.LinePattern = OfficeChartLinePattern.None;
		lineProperties.ColorIndex = (OfficeKnownColors)79;
		m_dataFormat.SeriesNumber = 65533;
		m_dataFormat.MarkerStyle = OfficeChartMarkerType.None;
		m_dataFormat.MarkerForegroundColorIndex = (OfficeKnownColors)77;
		m_dataFormat.MarkerBackgroundColorIndex = (OfficeKnownColors)77;
		m_dataFormat.IsAutoMarker = false;
		m_chart.PrimaryCategoryAxis.IsBetween = true;
		_ = m_chart.PrimaryValueAxis.MajorGridLines;
		m_chart.DestinationType = destinationType;
	}

	public void ChangeChartStockHigh_Low_CloseType()
	{
		OfficeChartType destinationType = m_chart.DestinationType;
		m_chart.DestinationType = OfficeChartType.Line;
		ChangeChartStockLine();
		((ChartDataPointImpl)m_chart.Series[2].DataPoints.DefaultDataPoint).ChangeChartStockHigh_Low_CloseType();
		m_chart.DestinationType = destinationType;
	}

	public void ChangeChartStockOpen_High_Low_CloseType()
	{
		ChangeChartStockLine();
		FirstDropBar.Gap = 150;
		IOfficeChartBorder lineProperties = m_firstDropBar.LineProperties;
		lineProperties.LinePattern = OfficeChartLinePattern.Solid;
		lineProperties.LineWeight = OfficeChartLineWeight.Hairline;
		IOfficeChartInterior interior = m_firstDropBar.Interior;
		interior.Pattern = OfficePattern.Solid;
		lineProperties.ColorIndex = (OfficeKnownColors)79;
		lineProperties.AutoFormat = true;
		interior.ForegroundColorIndex = OfficeKnownColors.WhiteCustom;
		interior.BackgroundColorIndex = OfficeKnownColors.Custom0;
		m_secondDropBar = m_firstDropBar.Clone(this);
		IOfficeChartInterior interior2 = m_secondDropBar.Interior;
		interior2.ForegroundColorIndex = OfficeKnownColors.Custom0;
		interior2.BackgroundColorIndex = OfficeKnownColors.WhiteCustom;
	}

	public void ChangeChartStockVolume_High_Low_CloseTypeFirst()
	{
		m_serieFormat = BiffRecordFactory.GetRecord(TBIFFRecord.ChartBar);
		IsVaryColor = false;
	}

	public void ChangeChartStockVolume_High_Low_CloseTypeSecond()
	{
		ChangeChartStockLine();
		ushort[] series = new ushort[4] { 1, 2, 3, 4 };
		m_seriesList = (ChartSeriesListRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSeriesList);
		m_seriesList.Series = series;
		m_chart.SecondaryParentAxis.UpdateSecondaryAxis(bCreateAxis: true);
		for (int i = 1; i < 4; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)m_chart.Series[i];
			chartSerieImpl.ChartGroup = 1;
			if (i == 3)
			{
				((ChartDataPointImpl)chartSerieImpl.DataPoints.DefaultDataPoint).ChangeChartStockVolume_High_Low_CloseType();
			}
		}
	}

	public void ChangeChartStockVolume_Open_High_Low_CloseType()
	{
		ChangeChartStockOpen_High_Low_CloseType();
		FirstDropBar.Gap = 100;
		SecondDropBar.Gap = 100;
		IOfficeChartInterior interior = m_firstDropBar.Interior;
		interior.ForegroundColorIndex = OfficeKnownColors.WhiteCustom;
		interior.BackgroundColorIndex = OfficeKnownColors.Custom0;
		OfficeChartType destinationType = m_chart.DestinationType;
		m_chart.DestinationType = OfficeChartType.Line;
		((ChartSerieDataFormatImpl)SerieDataFormat).SeriesNumber = 65533;
		m_chart.DestinationType = destinationType;
		SecondDropBar.Interior.BackgroundColor = Color.FromArgb(0, 255, 255, 255);
		m_seriesList = (ChartSeriesListRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSeriesList);
		ushort[] series = new ushort[5] { 1, 2, 3, 4, 5 };
		m_seriesList.Series = series;
		for (int i = 1; i < 5; i++)
		{
			((ChartSerieImpl)m_chart.Series[i]).ChartGroup = 1;
		}
	}

	public void ChangeSerieType(OfficeChartType type, bool isSeriesCreation)
	{
		if (m_isChartExType)
		{
			m_isChartExType = false;
		}
		switch (type)
		{
		case OfficeChartType.Doughnut:
		case OfficeChartType.Doughnut_Exploded:
			ChangeSerieDoughnut(type);
			break;
		case OfficeChartType.Surface_3D:
		case OfficeChartType.Surface_NoColor_3D:
		case OfficeChartType.Surface_Contour:
		case OfficeChartType.Surface_NoColor_Contour:
			ChangeSerieSurface(type, isSeriesCreation);
			break;
		case OfficeChartType.Column_Clustered:
		case OfficeChartType.Column_Clustered_3D:
		case OfficeChartType.Column_3D:
		case OfficeChartType.Bar_Clustered:
		case OfficeChartType.Bar_Clustered_3D:
			ChangeSerieBarClustered(type);
			break;
		case OfficeChartType.Radar:
		case OfficeChartType.Radar_Markers:
		case OfficeChartType.Radar_Filled:
			ChangeSerieRadar(type);
			break;
		case OfficeChartType.Column_Stacked:
		case OfficeChartType.Column_Stacked_100:
		case OfficeChartType.Column_Stacked_3D:
		case OfficeChartType.Column_Stacked_100_3D:
		case OfficeChartType.Bar_Stacked:
		case OfficeChartType.Bar_Stacked_100:
		case OfficeChartType.Bar_Stacked_3D:
		case OfficeChartType.Bar_Stacked_100_3D:
			ChangeSerieBarStacked(type);
			break;
		case OfficeChartType.Line:
		case OfficeChartType.Line_Stacked:
		case OfficeChartType.Line_Stacked_100:
		case OfficeChartType.Line_Markers:
		case OfficeChartType.Line_Markers_Stacked:
		case OfficeChartType.Line_Markers_Stacked_100:
		case OfficeChartType.Line_3D:
			ChangeSerieLine(type);
			break;
		case OfficeChartType.Pie:
		case OfficeChartType.Pie_3D:
		case OfficeChartType.PieOfPie:
		case OfficeChartType.Pie_Exploded:
		case OfficeChartType.Pie_Exploded_3D:
		case OfficeChartType.Pie_Bar:
			ChangeSeriePie(type);
			break;
		case OfficeChartType.Area:
		case OfficeChartType.Area_Stacked:
		case OfficeChartType.Area_Stacked_100:
		case OfficeChartType.Area_3D:
		case OfficeChartType.Area_Stacked_3D:
		case OfficeChartType.Area_Stacked_100_3D:
			ChangeSerieArea(type);
			break;
		case OfficeChartType.Scatter_Markers:
		case OfficeChartType.Scatter_SmoothedLine_Markers:
		case OfficeChartType.Scatter_SmoothedLine:
		case OfficeChartType.Scatter_Line_Markers:
		case OfficeChartType.Scatter_Line:
			ChangeSerieScatter(type);
			break;
		case OfficeChartType.Bubble:
		case OfficeChartType.Bubble_3D:
			ChangeSerieBuble(type, isSeriesCreation);
			break;
		case OfficeChartType.Cylinder_Clustered:
		case OfficeChartType.Cylinder_Stacked:
		case OfficeChartType.Cylinder_Stacked_100:
		case OfficeChartType.Cylinder_Bar_Clustered:
		case OfficeChartType.Cylinder_Bar_Stacked:
		case OfficeChartType.Cylinder_Bar_Stacked_100:
		case OfficeChartType.Cylinder_Clustered_3D:
		case OfficeChartType.Cone_Clustered:
		case OfficeChartType.Cone_Stacked:
		case OfficeChartType.Cone_Stacked_100:
		case OfficeChartType.Cone_Bar_Clustered:
		case OfficeChartType.Cone_Bar_Stacked:
		case OfficeChartType.Cone_Bar_Stacked_100:
		case OfficeChartType.Cone_Clustered_3D:
		case OfficeChartType.Pyramid_Clustered:
		case OfficeChartType.Pyramid_Stacked:
		case OfficeChartType.Pyramid_Stacked_100:
		case OfficeChartType.Pyramid_Bar_Clustered:
		case OfficeChartType.Pyramid_Bar_Stacked:
		case OfficeChartType.Pyramid_Bar_Stacked_100:
		case OfficeChartType.Pyramid_Clustered_3D:
			ChangeSerieConeCylinderPyramyd(type);
			break;
		case OfficeChartType.Pareto:
		case OfficeChartType.Funnel:
		case OfficeChartType.Histogram:
		case OfficeChartType.WaterFall:
		case OfficeChartType.TreeMap:
		case OfficeChartType.SunBurst:
		case OfficeChartType.BoxAndWhisker:
			SetNullForAllRecords();
			m_serieFormat = (ChartBarRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartBar);
			m_isChartExType = true;
			break;
		default:
			throw new NotSupportedException("Cannot change serie type.");
		}
		if (!m_isChartExType)
		{
			string startSerieType = GetStartSerieType(type);
			if (!m_chart.ParentWorkbook.IsWorkbookOpening)
			{
				m_chart.PrimaryCategoryAxis.IsBetween = !(startSerieType == "Area") && !(startSerieType == "Surface");
			}
		}
	}

	private void ChangeSerieDoughnut(OfficeChartType type)
	{
		SetNullForAllRecords();
		m_serieFormat = (ChartPieRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPie);
		m_chartChartFormat.IsVaryColor = true;
		DoughnutHoleSize = 50;
		if (type == OfficeChartType.Doughnut_Exploded)
		{
			SerieDataFormat.Percent = 25;
		}
	}

	private void ChangeSerieBuble(OfficeChartType type, bool isSeriesCreation)
	{
		SetNullForAllRecords();
		m_serieFormat = (ChartScatterRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartScatter);
		SizeRepresents = ChartBubbleSize.Area;
		BubbleScale = 100;
		IsBubbles = true;
		if (type == OfficeChartType.Bubble_3D)
		{
			SerieDataFormat.Is3DBubbles = true;
		}
		if (!isSeriesCreation)
		{
			UpdateBubbleSeries(m_chart.Series);
		}
	}

	private void UpdateBubbleSeries(IOfficeChartSeries series)
	{
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		int count = m_chart.Series.Count;
		int num;
		for (num = 0; num < series.Count - 1; num += 2)
		{
			ChartSerieImpl obj = (ChartSerieImpl)series[num];
			IOfficeChartSerie officeChartSerie = series[num + 1];
			obj.BubblesIRange = (officeChartSerie.Values as ChartDataRange).Range;
			obj.Index = num;
			obj.Number = num;
			series.RemoveAt(num + 1);
			num--;
		}
		if (count % 2 != 0)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)series[0];
			IRange range = (chartSerieImpl.Values as ChartDataRange).Range;
			int num2 = ((chartSerieImpl.Values != null) ? Math.Max(range.LastRow - range.Row + 1, range.LastColumn - range.Column + 1) : chartSerieImpl.EnteredDirectlyValues.Length);
			object[] array = new object[num2];
			for (int i = 0; i < num2; i++)
			{
				IRange range2 = chartSerieImpl.BubblesIRange.Cells[i];
				array[i] = range2.Value2;
			}
			int num3 = series.Count - 1;
			ChartSerieImpl obj2 = (ChartSerieImpl)series[series.Count - 1];
			obj2.EnteredDirectlyBubbles = array;
			obj2.Index = num3;
			obj2.Number = num3;
		}
	}

	private void ChangeSerieSurface(OfficeChartType type, bool isSeriesCreation)
	{
		if (m_chart.Series.Count < 2 && !isSeriesCreation)
		{
			throw new ArgumentException("Cannot change type. Chart cannot contain less then 2 series.");
		}
		SetNullForAllRecords();
		m_serieFormat = (ChartSurfaceRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSurface);
		RightAngleAxes = false;
		if (type == OfficeChartType.Surface_3D || type == OfficeChartType.Surface_Contour)
		{
			IsFillSurface = true;
		}
		if (type == OfficeChartType.Surface_NoColor_Contour || type == OfficeChartType.Surface_Contour)
		{
			Rotation = 0;
			Elevation = 90;
			Perspective = 0;
			IsVaryColor = false;
		}
	}

	private void ChangeSerieRadar(OfficeChartType type)
	{
		SetNullForAllRecords();
		if (type == OfficeChartType.Radar_Filled)
		{
			ChartRadarAreaRecord chartRadarAreaRecord = (ChartRadarAreaRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartRadarArea);
			chartRadarAreaRecord.IsRadarAxisLabel = true;
			m_serieFormat = chartRadarAreaRecord;
			IsCategoryName = true;
			return;
		}
		ChartRadarRecord chartRadarRecord = (ChartRadarRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartRadar);
		chartRadarRecord.IsRadarAxisLabel = true;
		m_serieFormat = chartRadarRecord;
		if (type == OfficeChartType.Radar)
		{
			HasRadarAxisLabels = true;
			((ChartSerieDataFormatImpl)SerieDataFormat).ChangeRadarDataFormat(type);
		}
	}

	private void ChangeSerieBarClustered(OfficeChartType type)
	{
		SetNullForAllRecords();
		m_serieFormat = (ChartBarRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartBar);
		if (type == OfficeChartType.Column_Clustered_3D || type == OfficeChartType.Bar_Clustered_3D)
		{
			IsClustered = true;
		}
		if (type == OfficeChartType.Column_3D)
		{
			RightAngleAxes = false;
		}
		else if (type.ToString().IndexOf("Bar_") >= 0)
		{
			IsHorizontalBar = true;
		}
	}

	private void ChangeSerieBarStacked(OfficeChartType type)
	{
		ChangeSerieBarClustered(type);
		StackValuesBar = true;
		BarRecord.Overlap = -65436;
		if (m_chart != null)
		{
			m_chart.OverLap = -65436;
		}
		switch (type)
		{
		case OfficeChartType.Column_Stacked_100:
		case OfficeChartType.Bar_Stacked_100:
			ShowAsPercentsBar = true;
			break;
		case OfficeChartType.Column_Stacked_3D:
		case OfficeChartType.Bar_Stacked_3D:
			m_chart3D = (Chart3DRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3D);
			break;
		case OfficeChartType.Column_Stacked_100_3D:
		case OfficeChartType.Bar_Stacked_100_3D:
			m_chart3D = (Chart3DRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3D);
			ShowAsPercentsBar = true;
			break;
		}
	}

	private void ChangeSerieLine(OfficeChartType type)
	{
		if (!m_chart.ParentWorkbook.IsWorkbookOpening)
		{
			SetNullForAllRecords();
		}
		m_serieFormat = (ChartLineRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartLine);
		switch (type)
		{
		case OfficeChartType.Line_Markers:
			return;
		case OfficeChartType.Line_3D:
			m_chart3D = (Chart3DRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3D);
			RightAngleAxes = false;
			return;
		case OfficeChartType.Line_Stacked:
		case OfficeChartType.Line_Markers_Stacked:
			StackValuesLine = true;
			break;
		}
		if (type == OfficeChartType.Line_Markers_Stacked_100 || type == OfficeChartType.Line_Stacked_100)
		{
			StackValuesLine = true;
			ShowAsPercentsLine = true;
		}
		if (type == OfficeChartType.Line || type == OfficeChartType.Line_Stacked || type == OfficeChartType.Line_Stacked_100)
		{
			((ChartSerieDataFormatImpl)SerieDataFormat).ChangeLineDataFormat(type);
		}
	}

	private void ChangeSeriePie(OfficeChartType type)
	{
		SetNullForAllRecords();
		IsVaryColor = true;
		m_serieFormat = (ChartPieRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPie);
		if (type == OfficeChartType.Pie_3D || type == OfficeChartType.Pie_Exploded_3D)
		{
			m_chart3D = (Chart3DRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3D);
		}
		if (type == OfficeChartType.Pie_Exploded || type == OfficeChartType.Pie_Exploded_3D)
		{
			SerieDataFormat.Percent = 25;
		}
		if (type == OfficeChartType.Pie_Bar || type == OfficeChartType.PieOfPie)
		{
			m_serieFormat = (ChartBoppopRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartBoppop);
			UseDefaultSplitValue = true;
			PieChartType = OfficePieType.Bar;
			PieSecondSize = 75;
			Gap = 100;
			HasSeriesLines = true;
			m_serieLine = new ChartBorderImpl(base.Application, this);
			if (type == OfficeChartType.PieOfPie)
			{
				PieChartType = OfficePieType.Pie;
			}
		}
	}

	private void ChangeSerieArea(OfficeChartType type)
	{
		SetNullForAllRecords();
		m_serieFormat = (ChartAreaRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartArea);
		if (type == OfficeChartType.Area_3D || type == OfficeChartType.Area_Stacked_3D || type == OfficeChartType.Area_Stacked_100_3D)
		{
			m_chart3D = (Chart3DRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3D);
		}
		if (type == OfficeChartType.Area_Stacked || type == OfficeChartType.Area_Stacked_3D)
		{
			IsStacked = true;
		}
		if (type == OfficeChartType.Area_Stacked_100 || type == OfficeChartType.Area_Stacked_100_3D)
		{
			IsStacked = true;
			IsCategoryBrokenDown = true;
		}
		if (type == OfficeChartType.Area_3D)
		{
			RightAngleAxes = false;
		}
	}

	private void ChangeSerieScatter(OfficeChartType type)
	{
		SetNullForAllRecords();
		m_serieFormat = (ChartScatterRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartScatter);
		SizeRepresents = ChartBubbleSize.Area;
		BubbleScale = 100;
		if (type != OfficeChartType.Scatter_Line_Markers)
		{
			((ChartSerieDataFormatImpl)SerieDataFormat).ChangeScatterDataFormat(type);
		}
	}

	private void ChangeSerieConeCylinderPyramyd(OfficeChartType type)
	{
		switch (type)
		{
		case OfficeChartType.Cylinder_Bar_Clustered:
		case OfficeChartType.Cone_Bar_Clustered:
		case OfficeChartType.Pyramid_Bar_Clustered:
			ChangeSerieBarClustered(OfficeChartType.Bar_Clustered_3D);
			break;
		case OfficeChartType.Cylinder_Clustered:
		case OfficeChartType.Cone_Clustered:
		case OfficeChartType.Pyramid_Clustered:
			ChangeSerieBarClustered(OfficeChartType.Column_Clustered_3D);
			break;
		case OfficeChartType.Cylinder_Clustered_3D:
		case OfficeChartType.Cone_Clustered_3D:
		case OfficeChartType.Pyramid_Clustered_3D:
			ChangeSerieBarClustered(OfficeChartType.Column_3D);
			break;
		case OfficeChartType.Cylinder_Bar_Stacked:
		case OfficeChartType.Cone_Bar_Stacked:
		case OfficeChartType.Pyramid_Bar_Stacked:
			ChangeSerieBarStacked(OfficeChartType.Bar_Stacked_3D);
			break;
		case OfficeChartType.Cylinder_Stacked:
		case OfficeChartType.Cone_Stacked:
		case OfficeChartType.Pyramid_Stacked:
			ChangeSerieBarStacked(OfficeChartType.Column_Stacked_3D);
			break;
		case OfficeChartType.Cylinder_Bar_Stacked_100:
		case OfficeChartType.Cone_Bar_Stacked_100:
		case OfficeChartType.Pyramid_Bar_Stacked_100:
			ChangeSerieBarStacked(OfficeChartType.Bar_Stacked_100_3D);
			break;
		case OfficeChartType.Cylinder_Stacked_100:
		case OfficeChartType.Cone_Stacked_100:
		case OfficeChartType.Pyramid_Stacked_100:
			ChangeSerieBarStacked(OfficeChartType.Column_Stacked_100_3D);
			break;
		}
		OfficeBaseFormat officeBaseFormat;
		OfficeTopFormat officeTopFormat;
		switch (type)
		{
		case OfficeChartType.Cone_Clustered:
		case OfficeChartType.Cone_Stacked:
		case OfficeChartType.Cone_Stacked_100:
		case OfficeChartType.Cone_Bar_Clustered:
		case OfficeChartType.Cone_Bar_Stacked:
		case OfficeChartType.Cone_Bar_Stacked_100:
		case OfficeChartType.Cone_Clustered_3D:
			officeBaseFormat = OfficeBaseFormat.Circle;
			officeTopFormat = OfficeTopFormat.Sharp;
			break;
		case OfficeChartType.Pyramid_Clustered:
		case OfficeChartType.Pyramid_Stacked:
		case OfficeChartType.Pyramid_Stacked_100:
		case OfficeChartType.Pyramid_Bar_Clustered:
		case OfficeChartType.Pyramid_Bar_Stacked:
		case OfficeChartType.Pyramid_Bar_Stacked_100:
		case OfficeChartType.Pyramid_Clustered_3D:
			officeBaseFormat = OfficeBaseFormat.Rectangle;
			officeTopFormat = OfficeTopFormat.Sharp;
			break;
		case OfficeChartType.Cylinder_Clustered:
		case OfficeChartType.Cylinder_Stacked:
		case OfficeChartType.Cylinder_Stacked_100:
		case OfficeChartType.Cylinder_Bar_Clustered:
		case OfficeChartType.Cylinder_Bar_Stacked:
		case OfficeChartType.Cylinder_Bar_Stacked_100:
		case OfficeChartType.Cylinder_Clustered_3D:
			officeBaseFormat = OfficeBaseFormat.Circle;
			officeTopFormat = OfficeTopFormat.Straight;
			break;
		default:
			throw new ArgumentException("type");
		}
		((ChartSeriesCollection)m_chart.Series).UpdateDataPointForCylConePurChartType(officeBaseFormat, officeTopFormat);
		SerieDataFormat.BarShapeBase = officeBaseFormat;
		SerieDataFormat.BarShapeTop = officeTopFormat;
	}

	public object Clone(object parent)
	{
		ChartFormatImpl chartFormatImpl = (ChartFormatImpl)MemberwiseClone();
		chartFormatImpl.SetParent(parent);
		chartFormatImpl.SetParents();
		chartFormatImpl.m_chartChartFormat = (ChartChartFormatRecord)CloneUtils.CloneCloneable(m_chartChartFormat);
		chartFormatImpl.m_serieFormat = (BiffRecordRaw)CloneUtils.CloneCloneable(m_serieFormat);
		chartFormatImpl.m_chart3D = (Chart3DRecord)CloneUtils.CloneCloneable(m_chart3D);
		chartFormatImpl.m_formatLink = (ChartFormatLinkRecord)CloneUtils.CloneCloneable(m_formatLink);
		chartFormatImpl.m_dataLabels = (ChartDataLabelsRecord)CloneUtils.CloneCloneable(m_dataLabels);
		chartFormatImpl.m_chartChartLine = (ChartChartLineRecord)CloneUtils.CloneCloneable(m_chartChartLine);
		if (m_serieLine != null)
		{
			chartFormatImpl.m_serieLine = m_serieLine.Clone(chartFormatImpl);
		}
		if (m_highlowLine != null)
		{
			chartFormatImpl.m_highlowLine = m_highlowLine.Clone(chartFormatImpl);
		}
		if (m_dropLine != null)
		{
			chartFormatImpl.m_dropLine = m_dropLine.Clone(chartFormatImpl);
		}
		chartFormatImpl.m_seriesList = (ChartSeriesListRecord)CloneUtils.CloneCloneable(m_seriesList);
		if (m_firstDropBar != null)
		{
			chartFormatImpl.m_firstDropBar = m_firstDropBar.Clone(chartFormatImpl);
		}
		if (m_secondDropBar != null)
		{
			chartFormatImpl.m_secondDropBar = m_secondDropBar.Clone(chartFormatImpl);
		}
		if (m_dataFormat != null)
		{
			chartFormatImpl.m_dataFormat = m_dataFormat.Clone(chartFormatImpl);
		}
		return chartFormatImpl;
	}

	public static bool operator ==(ChartFormatImpl format1, ChartFormatImpl format2)
	{
		if (object.Equals(format1, null) && object.Equals(format2, null))
		{
			return true;
		}
		if (object.Equals(format1, null) || object.Equals(format2, null))
		{
			return false;
		}
		if (format1.m_serieFormat.TypeCode != format2.m_serieFormat.TypeCode)
		{
			return false;
		}
		int storeSize = format1.m_serieFormat.GetStoreSize(OfficeVersion.Excel97to2003);
		int storeSize2 = format1.m_serieFormat.GetStoreSize(OfficeVersion.Excel97to2003);
		if (storeSize != storeSize2)
		{
			return false;
		}
		ByteArrayDataProvider byteArrayDataProvider = new ByteArrayDataProvider(new byte[storeSize]);
		format1.m_serieFormat.InfillInternalData(byteArrayDataProvider, 0, OfficeVersion.Excel97to2003);
		ByteArrayDataProvider byteArrayDataProvider2 = new ByteArrayDataProvider(new byte[storeSize2]);
		format2.m_serieFormat.InfillInternalData(byteArrayDataProvider2, 0, OfficeVersion.Excel97to2003);
		if (BiffRecordRaw.CompareArrays(byteArrayDataProvider.InternalBuffer, byteArrayDataProvider2.InternalBuffer) && format1.m_chartChartFormat.EqualsWithoutOrder(format2.m_chartChartFormat) && format1.m_chart3D == format2.m_chart3D && format1.m_seriesList == format2.m_seriesList && format1.m_chartChartLine == format2.m_chartChartLine)
		{
			return format1.m_dataLabels == format2.m_dataLabels;
		}
		return false;
	}

	public static bool operator !=(ChartFormatImpl format1, ChartFormatImpl format2)
	{
		return !(format1 == format2);
	}

	internal void InitializeStockFormat()
	{
		if (m_highlowLine == null)
		{
			m_highlowLine = new ChartBorderImpl(base.Application, this);
			m_highlowLine.LineWeight = OfficeChartLineWeight.Hairline;
			m_highlowLine.ColorIndex = (OfficeKnownColors)79;
			m_highlowLine.AutoFormat = true;
		}
		IOfficeChartBorder lineProperties = SerieDataFormat.LineProperties;
		lineProperties.LineWeight = OfficeChartLineWeight.Hairline;
		lineProperties.LinePattern = OfficeChartLinePattern.None;
		lineProperties.ColorIndex = (OfficeKnownColors)79;
		if (IsDropBar && !m_firstDropBar.HasLineProperties)
		{
			lineProperties = m_firstDropBar.LineProperties;
			lineProperties.LinePattern = OfficeChartLinePattern.Solid;
			lineProperties.LineWeight = OfficeChartLineWeight.Hairline;
			IOfficeChartInterior interior = m_firstDropBar.Interior;
			interior.Pattern = OfficePattern.Solid;
			lineProperties.ColorIndex = (OfficeKnownColors)79;
			lineProperties.AutoFormat = true;
			interior.ForegroundColorIndex = OfficeKnownColors.WhiteCustom;
			interior.BackgroundColorIndex = OfficeKnownColors.Custom0;
			interior.UseAutomaticFormat = true;
			IOfficeChartInterior interior2 = m_secondDropBar.Interior;
			interior2.ForegroundColorIndex = OfficeKnownColors.Custom0;
			interior2.BackgroundColorIndex = OfficeKnownColors.WhiteCustom;
			interior2.UseAutomaticFormat = true;
		}
	}

	internal void CloneDeletedFormat(object parent, ref ChartFormatImpl format, bool cloneDataFormat)
	{
		if (!cloneDataFormat && format == null)
		{
			format = (ChartFormatImpl)MemberwiseClone();
			format.SetParent(parent);
			format.SetParents();
			format.m_chartChartFormat = (ChartChartFormatRecord)CloneUtils.CloneCloneable(m_chartChartFormat);
			format.m_serieFormat = (BiffRecordRaw)CloneUtils.CloneCloneable(m_serieFormat);
			format.m_chart3D = (Chart3DRecord)CloneUtils.CloneCloneable(m_chart3D);
			format.m_formatLink = (ChartFormatLinkRecord)CloneUtils.CloneCloneable(m_formatLink);
			format.m_dataLabels = (ChartDataLabelsRecord)CloneUtils.CloneCloneable(m_dataLabels);
			format.m_chartChartLine = (ChartChartLineRecord)CloneUtils.CloneCloneable(m_chartChartLine);
			if (m_serieLine != null)
			{
				format.m_serieLine = m_serieLine.Clone(format);
			}
			if (m_highlowLine != null)
			{
				format.m_highlowLine = m_highlowLine.Clone(format);
			}
			if (m_dropLine != null)
			{
				format.m_dropLine = m_dropLine.Clone(format);
			}
			format.m_seriesList = (ChartSeriesListRecord)CloneUtils.CloneCloneable(m_seriesList);
			if (m_firstDropBar != null)
			{
				format.m_firstDropBar = m_firstDropBar.Clone(format);
			}
			if (m_secondDropBar != null)
			{
				format.m_secondDropBar = m_secondDropBar.Clone(format);
			}
			format.m_dataFormat = null;
		}
		else if (m_dataFormat != null)
		{
			format.m_dataFormat = m_dataFormat.Clone(format);
		}
	}

	internal OfficeChartType CheckAndApplyChartType()
	{
		OfficeChartType result = OfficeChartType.Column_Clustered;
		string text = "";
		switch (FormatRecordType)
		{
		case TBIFFRecord.ChartBar:
		{
			if (DataFormatOrNull != null && DataFormatOrNull.Serie3DdDataFormatOrNull != null)
			{
				if (DataFormatOrNull.BarShapeBase == OfficeBaseFormat.Circle)
				{
					text = ((DataFormatOrNull.BarShapeTop == OfficeTopFormat.Straight) ? "Cylinder" : "Cone");
					if (IsHorizontalBar)
					{
						text += "_Bar";
					}
				}
				else if (DataFormatOrNull.BarShapeTop == OfficeTopFormat.Straight)
				{
					text = (IsHorizontalBar ? "Bar" : "Column");
				}
				else
				{
					text = "Pyramid";
					if (IsHorizontalBar)
					{
						text += "_Bar";
					}
				}
			}
			else
			{
				text += (IsHorizontalBar ? "Bar" : "Column");
			}
			bool flag = text.IndexOf("Cone") != -1 || text.IndexOf("Cylinder") != -1 || text.IndexOf("Pyramid") != -1;
			if (text == "Column" && Is3D && !RightAngleAxes && !IsClustered && !StackValuesBar)
			{
				text += "_3D";
			}
			else
			{
				text = ((!StackValuesBar) ? (text + "_Clustered") : (text + "_Stacked"));
				if (ShowAsPercentsBar)
				{
					text += "_100";
				}
				if (!flag && Is3D)
				{
					text += "_3D";
				}
			}
			if (flag && !IsClustered && !StackValuesBar)
			{
				text += "_3D";
			}
			break;
		}
		case TBIFFRecord.ChartLine:
			if (Is3D)
			{
				text = "Line_3D";
				break;
			}
			text += "Line";
			if (IsMarker)
			{
				text += "_Markers";
			}
			if (StackValuesLine)
			{
				text += "_Stacked";
			}
			if (ShowAsPercentsLine)
			{
				text += "_100";
			}
			break;
		case TBIFFRecord.ChartRadar:
			text = "Radar";
			if (IsMarker)
			{
				text += "_Markers";
			}
			break;
		case TBIFFRecord.ChartRadarArea:
			text = "Radar_Filled";
			break;
		case TBIFFRecord.ChartArea:
			if (Is3D && !IsStacked)
			{
				text = "Area_3D";
				break;
			}
			text += "Area";
			if (IsStacked)
			{
				text += "_Stacked";
			}
			if (IsCategoryBrokenDown)
			{
				text += "_100";
			}
			if (Is3D)
			{
				text += "_3D";
			}
			break;
		case TBIFFRecord.ChartScatter:
			if (IsBubbles)
			{
				text = ((DataFormatOrNull == null || !DataFormatOrNull.Is3DBubbles) ? "Bubble" : "Bubble_3D");
				break;
			}
			text = "Scatter";
			text = ((!IsSmoothed) ? (text + "_Line") : (text + "_SmoothedLine"));
			if (IsMarker)
			{
				text += "_Markers";
			}
			break;
		case TBIFFRecord.ChartBoppop:
			switch (PieChartType)
			{
			case OfficePieType.Normal:
				text = "Pie";
				break;
			case OfficePieType.Bar:
				text = "Pie_Bar";
				break;
			case OfficePieType.Pie:
				text = "PieOfPie";
				break;
			}
			break;
		case TBIFFRecord.ChartPie:
			text = ((DoughnutHoleSize != 0) ? "Doughnut" : "Pie");
			if (DataFormatOrNull != null && DataFormatOrNull.PieFormat != null && DataFormatOrNull.PieFormat.Percent > 0)
			{
				text += "_Exploded";
			}
			if (Is3D)
			{
				text += "_3D";
			}
			break;
		case TBIFFRecord.ChartSurface:
			text = "Surface";
			if (!IsFillSurface)
			{
				text += "_NoColor";
			}
			text = ((Rotation != 0 || Elevation != 90 || Perspective != 0) ? (text + "_3D") : (text + "_Contour"));
			break;
		}
		if (text != "")
		{
			result = (OfficeChartType)Enum.Parse(typeof(OfficeChartType), text, ignoreCase: true);
		}
		return result;
	}
}
