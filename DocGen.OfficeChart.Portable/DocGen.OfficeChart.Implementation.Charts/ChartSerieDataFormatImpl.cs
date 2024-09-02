using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Shapes;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartSerieDataFormatImpl : CommonObject, IOfficeChartSerieDataFormat, IOfficeChartFillBorder, IFillColor
{
	private const ushort DEF_NONE_COLOR = 78;

	private const int DEF_MARKER_SIZE_MUL = 20;

	public const int DEF_MARKER_START_COLOR = 24;

	private const string DEF_PIE_START_TYPE = "Pie";

	private const string DEF_DOUGHNUT_START_TYPE = "Doughnut";

	private const string DEF_SURFACE_START_TYPE = "Surface";

	public const string DEF_LINE_START_TYPE = "Line";

	public const string DEF_SCATTER_START_TYPE = "Scatter";

	private const int DEF_MARKER_LINE_SIZE = 60;

	private const int DEF_LINE_SIZE = 5;

	private const int DEF_LINE_COLOR = 8388608;

	private const int DEF_MARKER_INDEX = 32;

	private const OfficeKnownColors DEF_MARKER_COLOR_INDEX = (OfficeKnownColors)77;

	public static readonly OfficeChartType[] DEF_SUPPORT_DATAFORMAT_PROPERTIES = new OfficeChartType[28]
	{
		OfficeChartType.Bar_Clustered_3D,
		OfficeChartType.Bar_Stacked_100_3D,
		OfficeChartType.Bar_Stacked_3D,
		OfficeChartType.Column_3D,
		OfficeChartType.Column_Clustered_3D,
		OfficeChartType.Column_Stacked_100_3D,
		OfficeChartType.Column_Stacked_3D,
		OfficeChartType.Cone_Bar_Clustered,
		OfficeChartType.Cone_Bar_Stacked,
		OfficeChartType.Cone_Bar_Stacked_100,
		OfficeChartType.Cone_Clustered,
		OfficeChartType.Cone_Clustered_3D,
		OfficeChartType.Cone_Stacked,
		OfficeChartType.Cone_Stacked_100,
		OfficeChartType.Cylinder_Bar_Clustered,
		OfficeChartType.Cylinder_Bar_Stacked,
		OfficeChartType.Cylinder_Bar_Stacked_100,
		OfficeChartType.Cylinder_Clustered,
		OfficeChartType.Cylinder_Clustered_3D,
		OfficeChartType.Cylinder_Stacked,
		OfficeChartType.Cylinder_Stacked_100,
		OfficeChartType.Pyramid_Bar_Clustered,
		OfficeChartType.Pyramid_Bar_Stacked,
		OfficeChartType.Pyramid_Bar_Stacked_100,
		OfficeChartType.Pyramid_Clustered,
		OfficeChartType.Pyramid_Clustered_3D,
		OfficeChartType.Pyramid_Stacked,
		OfficeChartType.Pyramid_Stacked_100
	};

	private ChartDataFormatRecord m_dataFormat = (ChartDataFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartDataFormat);

	private Chart3DDataFormatRecord m_3DDataFormat;

	private ChartPieFormatRecord m_pieFormat;

	private ThreeDFormatImpl m_3D;

	private ShadowImpl m_shadow;

	private ChartMarkerFormatRecord m_markerFormat;

	private ChartAttachedLabelRecord m_attachedLabel;

	private UnknownRecord m_startBlock;

	private UnknownRecord m_shapePropsStream;

	private UnknownRecord m_endBlock;

	private ChartAttachedLabelLayoutRecord m_attachedLabelLayout;

	private ChartSerFmtRecord m_seriesFormat;

	private ChartDataPointImpl m_dataPoint;

	private ChartSerieImpl m_serie;

	private ChartFormatImpl m_format;

	private ChartImpl m_chart;

	private ChartBorderImpl m_border;

	private ChartInteriorImpl m_interior;

	private bool m_bFormatted;

	private ChartFillImpl m_fill;

	private double m_markerLineWidth = 0.75;

	private ChartColor m_markerBackColor;

	private ChartColor m_markerForeColor;

	private GradientStops m_markerGradient;

	private double m_markerTransparency = 1.0;

	private Stream m_markerLineStream;

	private Stream m_markerEffectList;

	private bool m_HasMarkerProperties;

	private bool m_bIsParsed;

	private bool m_bIsDataPointColorParsed;

	private bool m_markerChanged;

	private bool m_showConnectorLines = true;

	private TreeMapLabelOption m_treeMapLabelOption = TreeMapLabelOption.Overlapping;

	private BoxAndWhiskerSerieFormat m_boxAndWhsikerFormat;

	private HistogramAxisFormat m_histogramAxisFormat;

	internal bool m_isMarkerDefaultSymbol;

	public bool HasLineProperties
	{
		get
		{
			return m_border != null;
		}
		internal set
		{
			if (m_border == null && value)
			{
				m_border = new ChartBorderImpl(base.Application, this);
			}
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

	public bool HasInterior
	{
		get
		{
			if (!IsInteriorSupported)
			{
				m_interior = null;
			}
			return m_interior != null;
		}
		internal set
		{
			if (m_interior == null && value)
			{
				m_interior = new ChartInteriorImpl(base.Application, this);
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

	public IOfficeChartBorder LineProperties
	{
		get
		{
			if (!m_chart.TypeChanging && !IsBorderSupported)
			{
				throw new NotSupportedException("This property dosn't support in this chart type");
			}
			if (m_chart.ParentWorkbook.IsWorkbookOpening)
			{
				HasLineProperties = true;
			}
			else
			{
				UpdateSerieFormat();
			}
			m_bFormatted = true;
			return m_border;
		}
	}

	public IOfficeChartInterior AreaProperties
	{
		get
		{
			if (!m_chart.TypeChanging && m_chart.IsParsed && !IsInteriorSupported)
			{
				throw new NotSupportedException("This property dosn't support in this chart type");
			}
			UpdateSerieFormat();
			m_bFormatted = true;
			if (!m_chart.TypeChanging && m_chart.IsParsed && m_interior != null && m_interior.UseAutomaticFormat)
			{
				OfficeKnownColors indexed = (OfficeKnownColors)UpdateColor(m_serie, m_dataPoint);
				m_interior.ForegroundColorObject.SetIndexed(indexed);
				m_interior.UseAutomaticFormat = true;
			}
			return m_interior;
		}
	}

	public OfficeBaseFormat BarShapeBase
	{
		get
		{
			return Serie3DDataFormat.DataFormatBase;
		}
		set
		{
			if (value != BarShapeBase)
			{
				bool loading = m_chart.Loading;
				if (!loading && Array.IndexOf(DEF_SUPPORT_DATAFORMAT_PROPERTIES, SerieType) == -1)
				{
					throw new NotSupportedException("This property is not supported in current chart type.");
				}
				Serie3DDataFormat.DataFormatBase = value;
				if (!loading)
				{
					UpdateBarFormat(bIsDataTop: false);
				}
				m_bFormatted = true;
				ClearOnPropertyChange();
			}
		}
	}

	public OfficeTopFormat BarShapeTop
	{
		get
		{
			return Serie3DDataFormat.DataFormatTop;
		}
		set
		{
			if (value != BarShapeTop)
			{
				bool loading = m_chart.Loading;
				if (!loading && Array.IndexOf(DEF_SUPPORT_DATAFORMAT_PROPERTIES, SerieType) == -1)
				{
					throw new NotSupportedException("This property is not supported in current chart type.");
				}
				Serie3DDataFormat.DataFormatTop = value;
				if (!loading)
				{
					UpdateBarFormat(bIsDataTop: true);
				}
				m_bFormatted = true;
				ClearOnPropertyChange();
			}
		}
	}

	public Color MarkerBackgroundColor
	{
		get
		{
			return m_markerBackColor.GetRGB(m_chart.Workbook);
		}
		set
		{
			m_markerBackColor.SetRGB(value);
			if (m_dataPoint != null)
			{
				m_dataPoint.IsDefaultmarkertype = true;
			}
		}
	}

	public Color MarkerForegroundColor
	{
		get
		{
			return m_markerForeColor.GetRGB(m_chart.Workbook);
		}
		set
		{
			if (((WorkbookImpl)m_chart.Workbook).IsCreated || base.Parent is ChartDataPointImpl)
			{
				MarkerFormat.FlagOptions |= 2;
			}
			m_markerForeColor.SetRGB(value);
			if (m_dataPoint != null)
			{
				m_dataPoint.IsDefaultmarkertype = true;
			}
		}
	}

	public OfficeChartMarkerType MarkerStyle
	{
		get
		{
			if (!m_chart.Loading && !m_chart.TypeChanging && !ValidateMarkerProprties())
			{
				throw new NotSupportedException("This property is not supported in this chart type.");
			}
			return MarkerFormat.MarkerType;
		}
		set
		{
			if (MarkerStyle != value)
			{
				MarkerFormat.MarkerType = value;
				IsAutoMarker = false;
				HasMarkerProperties = true;
				if (!m_chart.TypeChanging)
				{
					ClearOnPropertyChange();
				}
				if (m_dataPoint != null)
				{
					m_dataPoint.IsDefaultmarkertype = true;
				}
			}
			if (!ParentChart.Loading)
			{
				m_isMarkerDefaultSymbol = false;
			}
		}
	}

	public OfficeKnownColors MarkerForegroundColorIndex
	{
		get
		{
			if (!m_chart.Loading && !m_chart.TypeChanging && !ValidateMarkerProprties())
			{
				throw new NotSupportedException("This property is not supported in this chart type.");
			}
			return (OfficeKnownColors)MarkerFormat.BorderColorIndex;
		}
		set
		{
			if (MarkerForegroundColorIndex != value)
			{
				MarkerFormat.FlagOptions |= 2;
				m_markerForeColor.SetIndexed(value);
				if (m_dataPoint != null)
				{
					m_dataPoint.IsDefaultmarkertype = true;
				}
			}
		}
	}

	public OfficeKnownColors MarkerBackgroundColorIndex
	{
		get
		{
			if (!m_chart.Loading && !m_chart.TypeChanging && !ValidateMarkerProprties())
			{
				throw new NotSupportedException("This property is not supported in this chart type.");
			}
			return (OfficeKnownColors)MarkerFormat.FillColorIndex;
		}
		set
		{
			if (MarkerBackgroundColorIndex != value)
			{
				m_markerBackColor.SetIndexed(value);
				if (m_dataPoint != null)
				{
					m_dataPoint.IsDefaultmarkertype = true;
				}
			}
		}
	}

	public int MarkerSize
	{
		get
		{
			if (!m_chart.Loading && !m_chart.TypeChanging && !ValidateMarkerProprties())
			{
				throw new NotSupportedException("This property is not supported in this chart type.");
			}
			return MarkerFormat.LineSize / 20;
		}
		set
		{
			if (value != MarkerSize)
			{
				if (value < 2 || value > 72)
				{
					throw new ArgumentOutOfRangeException("MarkerSize");
				}
				MarkerFormat.LineSize = value * 20;
				if (MarkerFormat.IsAutoColor)
				{
					MarkerFormat.MarkerType = OfficeChartMarkerType.Square;
				}
				IsAutoMarker = false;
				ClearOnPropertyChange();
				if (m_dataPoint != null)
				{
					m_dataPoint.IsDefaultmarkertype = true;
				}
			}
		}
	}

	public bool IsAutoMarker
	{
		get
		{
			if (!m_chart.Loading && !m_chart.TypeChanging && !ValidateMarkerProprties())
			{
				throw new NotSupportedException("This property is not supported in this chart type.");
			}
			return MarkerFormat.IsAutoColor;
		}
		set
		{
			if (value != IsAutoMarker)
			{
				MarkerFormat.IsAutoColor = value;
				if (!value)
				{
					int num = UpdateColor(m_serie, m_dataPoint);
					MarkerFormat.FillColorIndex = (ushort)num;
					MarkerFormat.BorderColorIndex = (ushort)num;
				}
				if (!m_chart.TypeChanging)
				{
					ClearOnPropertyChange();
				}
			}
		}
	}

	public bool IsNotShowInt
	{
		get
		{
			return MarkerFormat.IsNotShowInt;
		}
		set
		{
			MarkerFormat.IsNotShowInt = value;
		}
	}

	public bool IsNotShowBrd
	{
		get
		{
			return MarkerFormat.IsNotShowBrd;
		}
		set
		{
			MarkerFormat.IsNotShowBrd = value;
		}
	}

	public int Percent
	{
		get
		{
			return PieFormat.Percent;
		}
		set
		{
			if (!m_chart.TypeChanging)
			{
				string startSerieType = ChartFormatImpl.GetStartSerieType(SerieType);
				if (startSerieType != "Pie" && startSerieType != "Doughnut")
				{
					throw new NotSupportedException("This property is not supported in current chart type.");
				}
			}
			PieFormat.Percent = (ushort)value;
			ClearOnPropertyChange();
		}
	}

	public bool IsSmoothedLine
	{
		get
		{
			return SerieFormat.IsSmoothedLine;
		}
		set
		{
			SerieFormat.IsSmoothedLine = value;
		}
	}

	public bool Is3DBubbles
	{
		get
		{
			return SerieFormat.Is3DBubbles;
		}
		set
		{
			if (Is3DBubbles != value)
			{
				OfficeChartType serieType = SerieType;
				if (serieType != OfficeChartType.Bubble && serieType != OfficeChartType.Bubble_3D)
				{
					throw new NotSupportedException("This property is not supported in this chart type.");
				}
				SerieFormat.Is3DBubbles = value;
				ClearOnPropertyChange();
			}
		}
	}

	public bool IsArShadow
	{
		get
		{
			return SerieFormat.IsArShadow;
		}
		set
		{
			SerieFormat.IsArShadow = value;
		}
	}

	public bool ShowActiveValue
	{
		get
		{
			return AttachedLabel.ShowActiveValue;
		}
		set
		{
			AttachedLabel.ShowActiveValue = value;
		}
	}

	public bool ShowPieInPercents
	{
		get
		{
			return AttachedLabel.ShowPieInPercents;
		}
		set
		{
			AttachedLabel.ShowPieInPercents = value;
		}
	}

	public bool ShowPieCategoryLabel
	{
		get
		{
			return AttachedLabel.ShowPieCategoryLabel;
		}
		set
		{
			AttachedLabel.ShowPieCategoryLabel = value;
		}
	}

	public bool SmoothLine
	{
		get
		{
			return AttachedLabel.SmoothLine;
		}
		set
		{
			AttachedLabel.SmoothLine = value;
		}
	}

	public bool ShowCategoryLabel
	{
		get
		{
			return AttachedLabel.ShowCategoryLabel;
		}
		set
		{
			AttachedLabel.ShowCategoryLabel = value;
		}
	}

	public bool ShowBubble
	{
		get
		{
			return AttachedLabel.ShowBubble;
		}
		set
		{
			AttachedLabel.ShowBubble = value;
		}
	}

	public IOfficeFill Fill
	{
		get
		{
			if (!m_chart.TypeChanging && m_chart.IsParsed)
			{
				if (!IsSupportFill)
				{
					throw new NotSupportedException("This property isn't supported in this chart type");
				}
				UpdateSerieFormat();
			}
			return m_fill;
		}
	}

	public bool IsSupportFill
	{
		get
		{
			OfficeChartType serieType = SerieType;
			string startSerieType = ChartFormatImpl.GetStartSerieType(serieType);
			bool flag = (startSerieType == "Line" && serieType != OfficeChartType.Line_3D) || serieType == OfficeChartType.Radar || serieType == OfficeChartType.Radar_Markers;
			return !(startSerieType == "Surface" || startSerieType == "Scatter" || flag);
		}
	}

	public IOfficeChartFormat CommonSerieOptions
	{
		get
		{
			if (m_serie == null)
			{
				throw new NotSupportedException("Cannot get series options.");
			}
			return m_serie.GetCommonSerieFormat();
		}
	}

	public bool IsMarkerSupported => ValidateMarkerProprties();

	public IOfficeChartInterior Interior => AreaProperties;

	public bool IsInteriorSupported
	{
		get
		{
			if (m_chart.Series.Count == 0)
			{
				return false;
			}
			return GetIsInteriorSupported(SerieType);
		}
	}

	public bool IsBorderSupported => GetIsBorderSupported(SerieType);

	internal bool HasMarkerProperties
	{
		get
		{
			return m_HasMarkerProperties;
		}
		set
		{
			m_HasMarkerProperties = value;
		}
	}

	internal double MarkerLineWidth
	{
		get
		{
			return m_markerLineWidth;
		}
		set
		{
			m_markerLineWidth = value;
		}
	}

	public ChartSerieImpl ParentSerie => m_serie;

	[CLSCompliant(false)]
	public ChartDataFormatRecord DataFormat
	{
		get
		{
			return m_dataFormat;
		}
		set
		{
			m_dataFormat = value;
		}
	}

	[CLSCompliant(false)]
	public ChartPieFormatRecord PieFormat
	{
		get
		{
			if (m_pieFormat == null)
			{
				m_pieFormat = (ChartPieFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPieFormat);
			}
			m_bFormatted = true;
			return m_pieFormat;
		}
	}

	[CLSCompliant(false)]
	public ChartMarkerFormatRecord MarkerFormat
	{
		get
		{
			if (m_markerFormat == null)
			{
				m_markerFormat = (ChartMarkerFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartMarkerFormat);
				m_bFormatted = true;
				m_markerFormat.IsAutoColor = true;
			}
			return m_markerFormat;
		}
	}

	[CLSCompliant(false)]
	public Chart3DDataFormatRecord Serie3DDataFormat
	{
		get
		{
			if (m_3DDataFormat == null)
			{
				m_3DDataFormat = (Chart3DDataFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3DDataFormat);
			}
			return m_3DDataFormat;
		}
	}

	[CLSCompliant(false)]
	public ChartSerFmtRecord SerieFormat
	{
		get
		{
			if (m_seriesFormat == null)
			{
				m_seriesFormat = (ChartSerFmtRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSerFmt);
			}
			m_bFormatted = true;
			return m_seriesFormat;
		}
	}

	[CLSCompliant(false)]
	public ChartAttachedLabelRecord AttachedLabel
	{
		get
		{
			if (m_attachedLabel == null)
			{
				UpdateSerieFormat();
				m_attachedLabel = (ChartAttachedLabelRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAttachedLabel);
			}
			return m_attachedLabel;
		}
	}

	[CLSCompliant(false)]
	public ChartAttachedLabelLayoutRecord AttachedLabelLayout
	{
		get
		{
			if (m_attachedLabelLayout == null)
			{
				m_attachedLabelLayout = (ChartAttachedLabelLayoutRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAttachedLabelLayout);
			}
			return m_attachedLabelLayout;
		}
	}

	public bool ContainsLineProperties => m_border != null;

	[CLSCompliant(false)]
	public ChartMarkerFormatRecord MarkerFormatOrNull => m_markerFormat;

	[CLSCompliant(false)]
	public Chart3DDataFormatRecord Serie3DdDataFormatOrNull => m_3DDataFormat;

	[CLSCompliant(false)]
	public ChartSerFmtRecord SerieFormatOrNull => m_seriesFormat;

	[CLSCompliant(false)]
	public ChartPieFormatRecord PieFormatOrNull => m_pieFormat;

	public int SeriesNumber
	{
		get
		{
			return m_dataFormat.SeriesNumber;
		}
		set
		{
			m_dataFormat.SeriesNumber = (ushort)value;
		}
	}

	public bool IsMarker
	{
		get
		{
			if (MarkerFormatOrNull != null)
			{
				return MarkerFormatOrNull.MarkerType != OfficeChartMarkerType.None;
			}
			return true;
		}
	}

	public bool IsLine
	{
		get
		{
			if (m_border != null && !m_border.AutoFormat)
			{
				return m_border.LinePattern != OfficeChartLinePattern.None;
			}
			return true;
		}
	}

	public bool IsSmoothed
	{
		get
		{
			if (SerieFormatOrNull != null)
			{
				return IsSmoothedLine;
			}
			return false;
		}
	}

	private OfficeChartType SerieType
	{
		get
		{
			if (m_serie != null)
			{
				return m_serie.SerieType;
			}
			return ((ChartSeriesCollection)m_chart.Series).GetTypeByOrder(m_dataFormat.SeriesIndex);
		}
	}

	public bool IsFormatted => m_bFormatted;

	public ChartImpl ParentChart => m_chart;

	public ChartColor MarkerBackColorObject => m_markerBackColor;

	public ChartColor MarkerForeColorObject => m_markerForeColor;

	internal GradientStops MarkerGradient
	{
		get
		{
			return m_markerGradient;
		}
		set
		{
			m_markerGradient = value;
		}
	}

	public double MarkerTransparency
	{
		get
		{
			return m_markerTransparency;
		}
		set
		{
			m_markerTransparency = value;
		}
	}

	public Stream MarkerLineStream
	{
		get
		{
			return m_markerLineStream;
		}
		set
		{
			m_markerLineStream = value;
		}
	}

	internal Stream EffectListStream
	{
		get
		{
			return m_markerEffectList;
		}
		set
		{
			m_markerEffectList = value;
		}
	}

	internal bool IsParsed
	{
		get
		{
			return m_bIsParsed;
		}
		set
		{
			m_bIsParsed = value;
		}
	}

	internal bool IsDataPointColorParsed
	{
		get
		{
			return m_bIsDataPointColorParsed;
		}
		set
		{
			m_bIsDataPointColorParsed = value;
		}
	}

	internal bool IsMarkerChanged
	{
		get
		{
			return m_markerChanged;
		}
		set
		{
			m_markerChanged = value;
		}
	}

	internal bool IsDefault => (!HasInterior || ((Interior.UseAutomaticFormat || (Interior.Pattern == OfficePattern.None && (Fill == null || Fill.FillType == OfficeFillType.Pattern || Fill.FillType == OfficeFillType.SolidColor))) && Interior.Pattern != 0)) && (Shadow == null || Shadow.ShadowInnerPresets == Office2007ChartPresetsInner.NoShadow) && Shadow.ShadowOuterPresets == Office2007ChartPresetsOuter.NoShadow && Shadow.ShadowPerspectivePresets == Office2007ChartPresetsPerspective.NoShadow && !Shadow.HasCustomShadowStyle && !Has3dProperties && !(Shadow is ShadowImpl) && (LineProperties == null || LineProperties.AutoFormat);

	public ChartColor ForeGroundColorObject
	{
		get
		{
			if (AreaProperties == null)
			{
				return null;
			}
			return (AreaProperties as ChartInteriorImpl).ForegroundColorObject;
		}
	}

	public ChartColor BackGroundColorObject
	{
		get
		{
			if (AreaProperties == null)
			{
				return null;
			}
			return (AreaProperties as ChartInteriorImpl).BackgroundColorObject;
		}
	}

	public OfficePattern Pattern
	{
		get
		{
			return AreaProperties.Pattern;
		}
		set
		{
			AreaProperties.Pattern = value;
		}
	}

	public bool IsAutomaticFormat
	{
		get
		{
			return AreaProperties.UseAutomaticFormat;
		}
		set
		{
			AreaProperties.UseAutomaticFormat = value;
		}
	}

	public bool Visible
	{
		get
		{
			return AreaProperties.Pattern != OfficePattern.None;
		}
		set
		{
			if (value)
			{
				if (AreaProperties.Pattern == OfficePattern.None)
				{
					AreaProperties.Pattern = OfficePattern.Solid;
				}
			}
			else
			{
				AreaProperties.Pattern = OfficePattern.None;
			}
		}
	}

	internal HistogramAxisFormat HistogramAxisFormatProperty
	{
		get
		{
			return m_histogramAxisFormat;
		}
		set
		{
			m_histogramAxisFormat = value;
		}
	}

	public bool ShowConnectorLines
	{
		get
		{
			return m_showConnectorLines;
		}
		set
		{
			m_showConnectorLines = value;
		}
	}

	public TreeMapLabelOption TreeMapLabelOption
	{
		get
		{
			return m_treeMapLabelOption;
		}
		set
		{
			m_treeMapLabelOption = value;
		}
	}

	public bool ShowMeanLine
	{
		get
		{
			return m_boxAndWhsikerFormat.ShowMeanLine;
		}
		set
		{
			m_boxAndWhsikerFormat.ShowMeanLine = value;
		}
	}

	public bool ShowMeanMarkers
	{
		get
		{
			return m_boxAndWhsikerFormat.ShowMeanMarkers;
		}
		set
		{
			m_boxAndWhsikerFormat.ShowMeanMarkers = value;
		}
	}

	public bool ShowInnerPoints
	{
		get
		{
			return m_boxAndWhsikerFormat.ShowInnerPoints;
		}
		set
		{
			m_boxAndWhsikerFormat.ShowInnerPoints = value;
		}
	}

	public bool ShowOutlierPoints
	{
		get
		{
			return m_boxAndWhsikerFormat.ShowOutlierPoints;
		}
		set
		{
			m_boxAndWhsikerFormat.ShowOutlierPoints = value;
		}
	}

	public QuartileCalculation QuartileCalculationType
	{
		get
		{
			return m_boxAndWhsikerFormat.QuartileCalculationType;
		}
		set
		{
			m_boxAndWhsikerFormat.QuartileCalculationType = value;
		}
	}

	internal bool IsBinningByCategory
	{
		get
		{
			return m_histogramAxisFormat.IsBinningByCategory;
		}
		set
		{
			m_histogramAxisFormat.IsBinningByCategory = value;
		}
	}

	internal bool HasAutomaticBins
	{
		get
		{
			return m_histogramAxisFormat.HasAutomaticBins;
		}
		set
		{
			m_histogramAxisFormat.HasAutomaticBins = value;
		}
	}

	internal int NumberOfBins
	{
		get
		{
			return m_histogramAxisFormat.NumberOfBins;
		}
		set
		{
			m_histogramAxisFormat.NumberOfBins = value;
		}
	}

	internal double BinWidth
	{
		get
		{
			return m_histogramAxisFormat.BinWidth;
		}
		set
		{
			m_histogramAxisFormat.BinWidth = value;
		}
	}

	internal double OverflowBinValue
	{
		get
		{
			return m_histogramAxisFormat.OverflowBinValue;
		}
		set
		{
			m_histogramAxisFormat.OverflowBinValue = value;
		}
	}

	internal double UnderflowBinValue
	{
		get
		{
			return m_histogramAxisFormat.UnderflowBinValue;
		}
		set
		{
			m_histogramAxisFormat.UnderflowBinValue = value;
		}
	}

	internal bool IsIntervalClosedinLeft
	{
		get
		{
			return m_histogramAxisFormat.IsIntervalClosedinLeft;
		}
		set
		{
			m_histogramAxisFormat.IsIntervalClosedinLeft = value;
		}
	}

	public ChartSerieDataFormatImpl(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
		m_fill = new ChartFillImpl(application, this);
		if (!m_chart.ParentWorkbook.IsWorkbookOpening)
		{
			SetDefault3DDataFormat();
		}
		if (parent is ChartDataPointImpl { IsDefault: not false })
		{
			m_boxAndWhsikerFormat.Options = 14;
		}
		InitializeColors();
	}

	private void InitializeColors()
	{
		m_markerForeColor = new ChartColor(ColorExtension.Empty);
		m_markerForeColor.AfterChange += MarkerForeColorChanged;
		m_markerBackColor = new ChartColor(ColorExtension.Empty);
		m_markerBackColor.AfterChange += MarkerBackColorChanged;
	}

	internal void SetParents()
	{
		m_chart = FindParent(typeof(ChartImpl)) as ChartImpl;
		Type[] arrTypes = new Type[2]
		{
			typeof(ChartSerieImpl),
			typeof(ChartFormatImpl)
		};
		object obj = FindParent(arrTypes);
		if (obj == null || m_chart == null)
		{
			throw new ArgumentNullException("Can't find parent objects.");
		}
		m_serie = obj as ChartSerieImpl;
		m_format = obj as ChartFormatImpl;
		m_dataPoint = FindParent(typeof(ChartDataPointImpl)) as ChartDataPointImpl;
		if (m_format != null && !m_chart.TypeChanging)
		{
			UpdateSerieFormat();
		}
	}

	[CLSCompliant(false)]
	public int Parse(IList<BiffRecordRaw> arrData, int iPos)
	{
		if (arrData == null)
		{
			throw new ArgumentNullException("arrData");
		}
		if (iPos < 0 || iPos > arrData.Count)
		{
			throw new ArgumentOutOfRangeException("iPos", "Value cannot be less than 0 and greater than arrData.Length");
		}
		BiffRecordRaw biffRecordRaw = arrData[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartDataFormat);
		m_dataFormat = (ChartDataFormatRecord)biffRecordRaw;
		iPos++;
		biffRecordRaw = arrData[iPos++];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.Begin);
		biffRecordRaw = arrData[iPos];
		int num = 1;
		while (num > 0)
		{
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.Begin:
				num++;
				break;
			case TBIFFRecord.End:
				num--;
				break;
			case TBIFFRecord.Chart3DDataFormat:
				m_3DDataFormat = (Chart3DDataFormatRecord)biffRecordRaw;
				m_bFormatted = ChackDataRecord(m_3DDataFormat);
				break;
			case TBIFFRecord.ChartLineFormat:
				m_border = new ChartBorderImpl(base.Application, this, (ChartLineFormatRecord)biffRecordRaw);
				m_bFormatted = true;
				break;
			case TBIFFRecord.ChartAreaFormat:
				m_interior = new ChartInteriorImpl(base.Application, this, (ChartAreaFormatRecord)biffRecordRaw);
				m_bFormatted = true;
				break;
			case TBIFFRecord.ChartPieFormat:
				m_pieFormat = (ChartPieFormatRecord)biffRecordRaw;
				m_bFormatted = true;
				break;
			case TBIFFRecord.ChartMarkerFormat:
				m_markerFormat = (ChartMarkerFormatRecord)biffRecordRaw;
				m_bFormatted = true;
				break;
			case TBIFFRecord.ChartGelFrame:
				m_fill = new ChartFillImpl(base.Application, this, (ChartGelFrameRecord)biffRecordRaw);
				break;
			case TBIFFRecord.ChartAttachedLabel:
				m_attachedLabel = (ChartAttachedLabelRecord)biffRecordRaw;
				break;
			case TBIFFRecord.ChartAttachedLabelLayout:
				m_attachedLabelLayout = (ChartAttachedLabelLayoutRecord)biffRecordRaw;
				break;
			case TBIFFRecord.ChartSerFmt:
				m_seriesFormat = (ChartSerFmtRecord)biffRecordRaw;
				m_bFormatted = true;
				break;
			case TBIFFRecord.StartBlock:
				m_startBlock = (UnknownRecord)biffRecordRaw;
				break;
			case TBIFFRecord.ShapePropsStream:
				m_shapePropsStream = (UnknownRecord)biffRecordRaw;
				break;
			case TBIFFRecord.EndBlock:
				m_endBlock = (UnknownRecord)biffRecordRaw;
				break;
			}
			iPos++;
			biffRecordRaw = arrData[iPos];
		}
		return iPos;
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		IOfficeChartDataLabels officeChartDataLabels = ((m_dataPoint != null && m_dataPoint.HasDataLabels) ? m_dataPoint.DataLabels : null);
		bool flag = officeChartDataLabels != null && (officeChartDataLabels.IsSeriesName || officeChartDataLabels.IsCategoryName);
		records.Add(m_dataFormat);
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		if (m_3DDataFormat != null)
		{
			records.Add((BiffRecordRaw)m_3DDataFormat.Clone());
		}
		if (m_border != null)
		{
			m_border.Serialize(records);
		}
		else if (flag && IsBorderSupported)
		{
			BiffRecordRaw record = BiffRecordFactory.GetRecord(TBIFFRecord.ChartLineFormat);
			records.Add(record);
		}
		if (m_interior != null)
		{
			m_interior.Serialize(records);
		}
		else if (flag && IsInteriorSupported)
		{
			BiffRecordRaw record2 = BiffRecordFactory.GetRecord(TBIFFRecord.ChartAreaFormat);
			records.Add(record2);
		}
		if (m_pieFormat != null)
		{
			records.Add((BiffRecordRaw)m_pieFormat.Clone());
		}
		if (m_seriesFormat != null)
		{
			records.Add((BiffRecordRaw)m_seriesFormat.Clone());
		}
		if ((m_serie == null || m_serie.StartType != "Scatter") && IsInteriorSupported)
		{
			m_fill.Serialize(records);
		}
		if (m_markerFormat != null && IsMarkerSupported)
		{
			records.Add((BiffRecordRaw)m_markerFormat.Clone());
		}
		if (m_attachedLabel != null)
		{
			records.Add((BiffRecordRaw)m_attachedLabel.Clone());
		}
		if (m_startBlock != null)
		{
			records.Add((UnknownRecord)m_startBlock.Clone());
		}
		if (m_shapePropsStream != null)
		{
			records.Add((UnknownRecord)m_shapePropsStream.Clone());
		}
		if (m_endBlock != null)
		{
			records.Add((UnknownRecord)m_endBlock.Clone());
		}
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
	}

	public void SetDefaultValues()
	{
		SetFieldsToNull();
		m_dataFormat.SeriesIndex = (ushort)m_serie.Index;
		m_dataFormat.SeriesNumber = (ushort)m_serie.Number;
		m_3DDataFormat = m_serie.Get3DDataFormat();
		ChartImpl innerChart = m_serie.InnerChart;
		if (innerChart.IsChartStock && innerChart.Series[innerChart.Series.Count - 1] == m_serie)
		{
			LineProperties.LinePattern = innerChart.DefaultLinePattern;
			m_border.AutoFormat = false;
			m_pieFormat = (ChartPieFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPieFormat);
			MarkerFormat.MarkerType = OfficeChartMarkerType.DowJones;
			m_markerFormat.LineSize = 60;
		}
		else if (m_serie.ChartGroup > 0)
		{
			LineProperties.LineColor = ColorExtension.FromArgb(8388608);
			m_pieFormat = (ChartPieFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPieFormat);
			MarkerFormat.MarkerType = OfficeChartMarkerType.Diamond;
			m_markerFormat.BorderColorIndex = 32;
			m_markerFormat.FillColorIndex = 32;
			m_markerFormat.LineSize = 100;
			m_markerFormat.IsAutoColor = true;
			m_border.AutoFormat = true;
		}
	}

	private void SetDefault3DDataFormat()
	{
		m_3DDataFormat = (Chart3DDataFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3DDataFormat);
		switch (m_chart.ChartType)
		{
		case OfficeChartType.Cylinder_Clustered:
		case OfficeChartType.Cylinder_Stacked:
		case OfficeChartType.Cylinder_Stacked_100:
		case OfficeChartType.Cylinder_Bar_Clustered:
		case OfficeChartType.Cylinder_Bar_Stacked:
		case OfficeChartType.Cylinder_Bar_Stacked_100:
		case OfficeChartType.Cylinder_Clustered_3D:
			m_3DDataFormat.DataFormatBase = OfficeBaseFormat.Circle;
			m_3DDataFormat.DataFormatTop = OfficeTopFormat.Straight;
			break;
		case OfficeChartType.Cone_Stacked_100:
		case OfficeChartType.Cone_Bar_Stacked_100:
			m_3DDataFormat.DataFormatBase = OfficeBaseFormat.Circle;
			m_3DDataFormat.DataFormatTop = OfficeTopFormat.Trunc;
			break;
		case OfficeChartType.Cone_Clustered:
		case OfficeChartType.Cone_Stacked:
		case OfficeChartType.Cone_Bar_Clustered:
		case OfficeChartType.Cone_Bar_Stacked:
		case OfficeChartType.Cone_Clustered_3D:
			m_3DDataFormat.DataFormatBase = OfficeBaseFormat.Circle;
			m_3DDataFormat.DataFormatTop = OfficeTopFormat.Sharp;
			break;
		case OfficeChartType.Pyramid_Stacked_100:
		case OfficeChartType.Pyramid_Bar_Stacked_100:
			m_3DDataFormat.DataFormatBase = OfficeBaseFormat.Rectangle;
			m_3DDataFormat.DataFormatTop = OfficeTopFormat.Trunc;
			break;
		case OfficeChartType.Pyramid_Clustered:
		case OfficeChartType.Pyramid_Stacked:
		case OfficeChartType.Pyramid_Bar_Clustered:
		case OfficeChartType.Pyramid_Bar_Stacked:
		case OfficeChartType.Pyramid_Clustered_3D:
			m_3DDataFormat.DataFormatBase = OfficeBaseFormat.Rectangle;
			m_3DDataFormat.DataFormatTop = OfficeTopFormat.Sharp;
			break;
		}
	}

	private void SetFieldsToNull()
	{
		m_dataFormat = null;
		m_3DDataFormat = null;
		m_border = null;
		m_interior = null;
		m_pieFormat = null;
		m_markerFormat = null;
	}

	internal void SetDefaultValuesForSerieRecords()
	{
		m_dataFormat.SeriesIndex = (ushort)m_serie.Index;
		m_dataFormat.SeriesNumber = (ushort)m_serie.Number;
		m_3DDataFormat = (Chart3DDataFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3DDataFormat);
		m_3D = null;
		m_pieFormat = null;
		m_seriesFormat = null;
	}

	public ChartSerieDataFormatImpl Clone(object parent)
	{
		ChartSerieDataFormatImpl chartSerieDataFormatImpl = (ChartSerieDataFormatImpl)MemberwiseClone();
		chartSerieDataFormatImpl.SetParent(parent);
		chartSerieDataFormatImpl.SetParents();
		chartSerieDataFormatImpl.m_dataFormat = (ChartDataFormatRecord)CloneUtils.CloneCloneable(m_dataFormat);
		chartSerieDataFormatImpl.m_3DDataFormat = (Chart3DDataFormatRecord)CloneUtils.CloneCloneable(m_3DDataFormat);
		if (m_border != null)
		{
			chartSerieDataFormatImpl.m_border = m_border.Clone(chartSerieDataFormatImpl);
		}
		if (m_interior != null)
		{
			chartSerieDataFormatImpl.m_interior = m_interior.Clone(chartSerieDataFormatImpl);
		}
		chartSerieDataFormatImpl.m_pieFormat = (ChartPieFormatRecord)CloneUtils.CloneCloneable(m_pieFormat);
		chartSerieDataFormatImpl.m_markerFormat = (ChartMarkerFormatRecord)CloneUtils.CloneCloneable(m_markerFormat);
		chartSerieDataFormatImpl.m_attachedLabel = (ChartAttachedLabelRecord)CloneUtils.CloneCloneable(m_attachedLabel);
		chartSerieDataFormatImpl.m_attachedLabelLayout = (ChartAttachedLabelLayoutRecord)CloneUtils.CloneCloneable(m_attachedLabelLayout);
		chartSerieDataFormatImpl.m_seriesFormat = (ChartSerFmtRecord)CloneUtils.CloneCloneable(m_seriesFormat);
		chartSerieDataFormatImpl.m_fill = (ChartFillImpl)m_fill.Clone(chartSerieDataFormatImpl);
		if (!m_chart.TypeChanging && !m_chart.IsParsed && IsInteriorSupported && chartSerieDataFormatImpl.IsSupportFill)
		{
			CopyFillBackForeGroundColorObjects(chartSerieDataFormatImpl);
		}
		chartSerieDataFormatImpl.InitializeColors();
		chartSerieDataFormatImpl.m_markerBackColor.CopyFrom(m_markerBackColor, callEvent: false);
		chartSerieDataFormatImpl.m_markerForeColor.CopyFrom(m_markerForeColor, callEvent: false);
		if (m_3D != null)
		{
			chartSerieDataFormatImpl.m_3D = m_3D.Clone(chartSerieDataFormatImpl);
		}
		if (m_shadow != null)
		{
			chartSerieDataFormatImpl.m_shadow = m_shadow.Clone(chartSerieDataFormatImpl);
		}
		if (m_shapePropsStream != null)
		{
			chartSerieDataFormatImpl.m_shapePropsStream = (UnknownRecord)m_shapePropsStream.Clone();
		}
		if (m_startBlock != null)
		{
			chartSerieDataFormatImpl.m_startBlock = (UnknownRecord)m_startBlock.Clone();
		}
		if (m_endBlock != null)
		{
			chartSerieDataFormatImpl.m_endBlock = (UnknownRecord)m_endBlock.Clone();
		}
		if (m_markerLineStream != null)
		{
			m_markerLineStream.Position = 0L;
			chartSerieDataFormatImpl.m_markerLineStream = CloneUtils.CloneStream(m_markerLineStream);
		}
		if (m_markerEffectList != null)
		{
			m_markerEffectList.Position = 0L;
			chartSerieDataFormatImpl.m_markerEffectList = CloneUtils.CloneStream(m_markerEffectList);
		}
		if (m_markerGradient != null)
		{
			chartSerieDataFormatImpl.m_markerGradient = m_markerGradient;
		}
		return chartSerieDataFormatImpl;
	}

	internal void CopyFillBackForeGroundColorObjects(ChartSerieDataFormatImpl result)
	{
		if (m_fill != null && result.m_fill != null)
		{
			if (result.m_fill.ForeColorObject != m_fill.ForeColorObject)
			{
				result.m_fill.ForeColorObject.CopyFrom(m_fill.ForeColorObject, callEvent: false);
			}
			if (result.m_fill.BackColorObject != m_fill.BackColorObject)
			{
				result.m_fill.BackColorObject.CopyFrom(m_fill.BackColorObject, callEvent: false);
			}
		}
	}

	public void UpdateSerieIndex()
	{
		m_dataFormat.SeriesIndex = (ushort)m_serie.Index;
		m_dataFormat.SeriesNumber = (ushort)m_serie.Number;
	}

	public void UpdateDataFormatInDataPoint()
	{
		if (ParentSerie == null)
		{
			throw new ArgumentException("Parent serie");
		}
		m_dataFormat.SeriesIndex = (ushort)ParentSerie.Index;
		m_dataFormat.SeriesNumber = (ushort)ParentSerie.Number;
	}

	public void ChangeRadarDataFormat(OfficeChartType type)
	{
		if (type == OfficeChartType.Radar)
		{
			MarkerForegroundColorIndex = (OfficeKnownColors)77;
			MarkerBackgroundColorIndex = (OfficeKnownColors)77;
			LineProperties.AutoFormat = true;
			m_border.IsAutoLineColor = true;
			IsAutoMarker = false;
			MarkerStyle = OfficeChartMarkerType.None;
		}
		if (type == OfficeChartType.Radar_Markers && !ParentChart.Loading)
		{
			LineProperties.AutoFormat = false;
		}
	}

	public void ChangeScatterDataFormat(OfficeChartType type)
	{
		if (type == OfficeChartType.Scatter_Line_Markers)
		{
			LineProperties.LinePattern = OfficeChartLinePattern.None;
			m_border.AutoFormat = true;
			return;
		}
		MarkerSize = 5;
		MarkerStyle = OfficeChartMarkerType.None;
		LineProperties.AutoFormat = true;
		if (type == OfficeChartType.Scatter_SmoothedLine || type == OfficeChartType.Scatter_SmoothedLine_Markers)
		{
			IsSmoothedLine = true;
		}
		if (type == OfficeChartType.Scatter_SmoothedLine_Markers || type == OfficeChartType.Scatter_Markers)
		{
			MarkerStyle = OfficeChartMarkerType.Diamond;
			IsAutoMarker = true;
		}
		if (type == OfficeChartType.Scatter_Markers)
		{
			m_border.LinePattern = OfficeChartLinePattern.None;
			m_markerFormat = null;
		}
	}

	public void ChangeLineDataFormat(OfficeChartType type)
	{
		if (type == OfficeChartType.Line || type == OfficeChartType.Line_Stacked || type == OfficeChartType.Line_Stacked_100)
		{
			IsAutoMarker = false;
			MarkerStyle = OfficeChartMarkerType.None;
		}
		if (type == OfficeChartType.Line_Markers || type == OfficeChartType.Line_Markers_Stacked || type == OfficeChartType.Line_Markers_Stacked_100)
		{
			LineProperties.AutoFormat = false;
		}
	}

	internal void UpdateBarFormat(bool bIsDataTop)
	{
		if (m_serie != null)
		{
			return;
		}
		IOfficeChartSeries series = m_chart.Series;
		int i = 0;
		for (int count = series.Count; i < count; i++)
		{
			IOfficeChartSerieDataFormat dataFormat = series[i].DataPoints.DefaultDataPoint.DataFormat;
			if (bIsDataTop)
			{
				dataFormat.BarShapeTop = BarShapeTop;
			}
			else
			{
				dataFormat.BarShapeBase = BarShapeBase;
			}
		}
	}

	public int UpdateLineColor()
	{
		OfficeChartType serieType = SerieType;
		string startSerieType = ChartFormatImpl.GetStartSerieType(serieType);
		if (serieType != OfficeChartType.Radar_Markers && serieType != OfficeChartType.Radar && !(startSerieType == "Line"))
		{
			return -1;
		}
		return UpdateColor(m_serie, m_dataPoint);
	}

	public static int UpdateColor(ChartSerieImpl serie, ChartDataPointImpl dataPoint)
	{
		if (serie == null)
		{
			return 24;
		}
		int num = (serie.SerieFormat.CommonSerieOptions.IsVaryColor ? dataPoint.Index : serie.Index);
		if (num <= 30)
		{
			return num + 24;
		}
		num -= 30;
		return num % 55 + 7;
	}

	public void UpdateSerieFormat()
	{
		if (m_3DDataFormat == null)
		{
			m_3DDataFormat = (Chart3DDataFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.Chart3DDataFormat);
		}
		if (m_pieFormat == null)
		{
			m_pieFormat = (ChartPieFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPieFormat);
		}
		if (m_markerFormat == null && !m_chart.ParentWorkbook.IsWorkbookOpening)
		{
			m_markerFormat = (ChartMarkerFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartMarkerFormat);
		}
		if (m_seriesFormat == null)
		{
			m_seriesFormat = (ChartSerFmtRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartSerFmt);
		}
		bool typeChanging = m_chart.TypeChanging;
		bool isWorkbookOpening = m_chart.ParentWorkbook.IsWorkbookOpening;
		OfficeChartType destinationType = m_chart.DestinationType;
		bool flag = !isWorkbookOpening && ((typeChanging && GetIsBorderSupported(destinationType)) || (!typeChanging && IsBorderSupported));
		if (m_border == null && flag)
		{
			m_border = new ChartBorderImpl(base.Application, this);
		}
		flag = !isWorkbookOpening && ((typeChanging && GetIsInteriorSupported(destinationType)) || (!typeChanging && IsInteriorSupported));
		if (m_interior == null && flag)
		{
			m_interior = new ChartInteriorImpl(base.Application, this);
		}
		m_bFormatted = true;
	}

	private bool ChackDataRecord(Chart3DDataFormatRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (record.DataFormatBase == OfficeBaseFormat.Rectangle)
		{
			return record.DataFormatTop != OfficeTopFormat.Straight;
		}
		return true;
	}

	public void ClearOnPropertyChange()
	{
		if (!m_chart.Loading)
		{
			if (m_format != null)
			{
				((ChartSeriesCollection)m_chart.Series).ClearDataFormats(this);
			}
			else if (m_dataPoint != null && m_dataPoint.Index == 65535)
			{
				m_dataPoint.ClearDataFormats(this);
			}
		}
	}

	private bool ValidateMarkerProprties()
	{
		OfficeChartType serieType = SerieType;
		string startSerieType = ChartFormatImpl.GetStartSerieType(serieType);
		bool num = startSerieType == "Line" || startSerieType == "Radar" || startSerieType == "Scatter";
		if (serieType == OfficeChartType.Radar || startSerieType == "Scatter")
		{
			HasMarkerProperties = true;
		}
		if (num && serieType != OfficeChartType.Line_3D)
		{
			return serieType != OfficeChartType.Radar_Filled;
		}
		return false;
	}

	internal static bool GetIsInteriorSupported(OfficeChartType chartType)
	{
		string startSerieType = ChartFormatImpl.GetStartSerieType(chartType);
		bool flag = (startSerieType == "Line" && chartType != OfficeChartType.Line_3D) || chartType == OfficeChartType.Radar || chartType == OfficeChartType.Radar_Markers;
		return !(startSerieType == "Surface" || startSerieType == "Scatter" || flag);
	}

	private static bool GetIsBorderSupported(OfficeChartType chartType)
	{
		return true;
	}

	internal void MarkerForeColorChanged()
	{
		IsAutoMarker = false;
		OfficeKnownColors indexed = m_markerForeColor.GetIndexed(m_chart.Workbook);
		MarkerFormat.BorderColorIndex = (ushort)indexed;
		MarkerFormat.IsNotShowBrd = indexed == OfficeKnownColors.Black;
		if (m_chart != null && m_chart.ParentWorkbook != null && !m_chart.ParentWorkbook.IsWorkbookOpening)
		{
			m_markerGradient = null;
		}
		if (EffectListStream != null)
		{
			IsMarkerChanged = true;
		}
		if (!m_chart.ParentWorkbook.IsWorkbookOpening)
		{
			m_markerLineStream = null;
		}
		ClearOnPropertyChange();
	}

	internal void MarkerBackColorChanged()
	{
		IsAutoMarker = false;
		OfficeKnownColors indexed = m_markerBackColor.GetIndexed(m_chart.Workbook);
		MarkerFormat.FillColorIndex = (ushort)indexed;
		MarkerFormat.IsNotShowInt = indexed == OfficeKnownColors.Black;
		if (EffectListStream != null)
		{
			IsMarkerChanged = true;
		}
		ClearOnPropertyChange();
	}
}
