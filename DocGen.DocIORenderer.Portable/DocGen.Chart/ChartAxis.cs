using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using DocGen.Chart.Drawing;
using DocGen.ChartToImageConverter;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

[TypeConverter(typeof(ExpandableObjectConverter))]
internal class ChartAxis : IDisposable
{
	private const int c_rowsCount = 4;

	private const int c_maxRoundingPlaces = 15;

	private static readonly double[] c_intervalDivs = new double[4] { 1.0, 2.0, 5.0, 10.0 };

	private static readonly DateTime c_dateTimeZero = DateTime.FromOADate(0.0);

	private bool m_needUpdateVisibleLables = true;

	private ChartAxisLabel[] m_visibleLables;

	private bool m_isFreezed;

	private bool m_scaleLables;

	private float m_scaleLablesCoef = 1f;

	private float m_scaleLength;

	private ChartSetMode adjustPlotAreaMargins;

	private ChartMargins chartPlotAreaMargins;

	private double m_labelsOffset;

	private double m_labelOffset;

	private bool m_interlacedGrid;

	private BrushInfo m_interlacedGridInterior = new BrushInfo(Color.LightGray);

	private StringAlignment m_labelAligment = StringAlignment.Center;

	private bool m_margin = true;

	private bool m_autoValueType;

	private bool m_customOrigin;

	private string m_dateTimeFormat = "g";

	private int m_desiredIntervals = 10;

	private float m_dimension;

	private bool m_needUpdateDimension = true;

	private float m_ticksAndLabelsDimension;

	private float[] groupingLabelsRowsDimensions = new float[18];

	private bool m_drawGrid = true;

	private bool m_drawMinorGrid;

	private LogLabelsDisplayMode m_showLogBaseLabels;

	private ChartAxisGridDrawingMode m_gridDrawMode;

	private ChartAxisLabelPlacement m_labelPlacement;

	private ChartAxisTickDrawingOperationMode m_tickDrawingOperationMode = ChartAxisTickDrawingOperationMode.NumberOfIntervalsFixed;

	private Font m_font = new Font("Verdana", 8f);

	private Font m_titleFont = new Font("Verdana", 8f);

	private Color m_foreColor = Color.Empty;

	private Color m_titleColor = Color.Empty;

	private BrushInfo m_backInterior;

	private ChartTitle m_chartAxisTitle = new ChartTitle();

	private string m_format = string.Empty;

	private LineInfo m_gridLineType;

	private ChartLabelIntersectAction m_intersectAction;

	private ChartLabelIntersectionActionEffect m_intersectEffect;

	private ChartDateTimeIntervalType m_intervalType;

	private bool m_hidePartialLabels;

	private bool m_inversed;

	private bool m_labelRotate;

	private bool m_rotateFromTicks;

	private int m_labelRotateAngle;

	private IChartAxisLabelModel m_labels;

	private IChartAxisGroupingLabelModel m_groupingLabels;

	private LineInfo m_lineType;

	private LineInfo m_minorGridLineType = new LineInfo();

	private int m_logBase = 10;

	private bool m_needUpdate = true;

	private double m_offset;

	private bool m_opposedPosition;

	private ChartOrientation m_orientation;

	private double m_origin;

	private MinMaxInfo m_range = new MinMaxInfo(0.0, 10.0, 1.0);

	private MinMaxInfo m_visibleRange = new MinMaxInfo(0.0, 10.0, 1.0);

	private ChartAxisRangeType m_rangeType;

	private bool requireInvertedAxes;

	private int m_roundingPlaces = 2;

	private Size m_smallTickSize = new Size(1, 1);

	private int m_smallTicksPerInterval;

	private ChartStripLineCollection m_stripLines;

	private Color m_tickColor = SystemColors.ControlText;

	private Size m_tickSize = new Size(1, 1);

	private ChartTitleDrawMode m_titleDrawMode;

	private string m_toolTip = "";

	private ArrayList cLabels = new ArrayList();

	private Array tempLabels = new Array[0];

	private ChartValueType m_valueType;

	private ChartAxisTickLabelDrawingMode tickLabelDrawingMode = ChartAxisTickLabelDrawingMode.AutomaticMode;

	private ChartAxisRangePaddingType m_rangePaddingType = ChartAxisRangePaddingType.Calculate;

	private bool m_forceZeroToDouble;

	private bool m_forceZero;

	private bool m_preferZero = true;

	private bool m_isMax;

	private bool m_autoSize = true;

	private float m_length;

	private PointF m_location = PointF.Empty;

	private PointF m_axisLineLocation = PointF.Empty;

	private double m_crossing = double.NaN;

	private ChartAxisLocationType m_locationType;

	private ChartDateTimeInterval m_dateTimeInterval;

	private bool smartDateZoom;

	private string smartDateZoomLabelsCulture = "en-US";

	private string smartDateZoomYearLevelLabelFormat = "y";

	private string smartDateZoomMonthLevelLabelFormat = "MMMM d, yyyy";

	private string smartDateZoomWeekLevelLabelFormat = "MMM, ddd d, yyyy";

	private string smartDateZoomDayLevelLabelFormat = "g";

	private string smartDateZoomHourLevelLabelFormat = "t";

	private string smartDateZoomMinuteLevelLabelFormat = "T";

	private string smartDateZoomSecondLevelLabelFormat = "T";

	private string currentSmartDateTimeFormat = "g";

	private ChartAxisEdgeLabelsDrawingMode edgeLabelsDrawingMode;

	private string m_title = "";

	private StringAlignment m_titleAlignment = StringAlignment.Center;

	private float m_titleSpacing = 6f;

	private bool m_drawTickLabelGrid;

	private float m_tickLabelGridPadding = 5f;

	private ChartOrientation m_defaultAxisOrienation;

	private ChartAxisBreakInfo m_breakInfo = new ChartAxisBreakInfo();

	private ChartAxisRange m_breakRanges;

	private bool m_makeBreaks = true;

	private ChartArea m_area;

	private bool m_isVisible = true;

	private bool m_excludeInvisibleSeriesRange;

	private double m_pointOffset;

	private ChartCustomLabelsParameter m_customLabelsParameter;

	private ChartAxisLayout m_layout;

	private Pen[] m_pens;

	private StringFormat m_labelStringFormat = new StringFormat(StringFormat.GenericDefault);

	private bool m_isHorizontalAxisAutoCross;

	private bool m_isVerticalAxisAutoCross;

	private ChartPlacement m_axisLabelPlacement = ChartPlacement.Outside;

	private bool m_isDateTimeCategoryAxis;

	private bool m_changeDateTimeAxisValue;

	private bool m_isMaxSet;

	private bool m_isMinSet;

	private bool m_isMajorSet;

	private double m_displayUnits;

	private AxisLabelConverter m_axisLabelConverter;

	internal bool IsVerticalAxisAutoCross
	{
		get
		{
			return m_isVerticalAxisAutoCross;
		}
		set
		{
			m_isVerticalAxisAutoCross = value;
		}
	}

	internal bool IsHorizontalAxisAutoCross
	{
		get
		{
			return m_isHorizontalAxisAutoCross;
		}
		set
		{
			m_isHorizontalAxisAutoCross = value;
		}
	}

	internal float TickAndLabelsDimension => m_ticksAndLabelsDimension;

	internal float[] GroupingLabelsRowsDimensions => groupingLabelsRowsDimensions;

	internal double CurrentOrigin
	{
		get
		{
			if (!m_customOrigin)
			{
				return (m_valueType == ChartValueType.Logarithmic) ? 1 : 0;
			}
			return m_origin;
		}
	}

	internal Array SortLabels => tempLabels;

	public bool AutoSize
	{
		get
		{
			return m_autoSize;
		}
		set
		{
			m_autoSize = value;
		}
	}

	public SizeF Size
	{
		get
		{
			SizeF empty = SizeF.Empty;
			return (m_orientation != 0) ? new SizeF(0f, RealLength) : new SizeF(RealLength, 0f);
		}
		set
		{
			if (!m_autoSize)
			{
				if (m_orientation == ChartOrientation.Horizontal && RealLength != value.Width)
				{
					RealLength = value.Width;
				}
				else if (m_orientation == ChartOrientation.Vertical && RealLength != value.Height)
				{
					RealLength = value.Height;
				}
			}
		}
	}

	public RectangleF Rect
	{
		get
		{
			RectangleF result = RectangleF.Empty;
			if (m_orientation == ChartOrientation.Horizontal)
			{
				float num = ((AxisLabelPlacement != 0) ? ((float)(OpposedPosition ? 1 : 0)) : ((float)((!OpposedPosition) ? 1 : 0)));
				result = new RectangleF(Location.X, Location.Y - num * Dimension, RealLength, Dimension);
			}
			else
			{
				float num2 = ((AxisLabelPlacement != 0) ? ((float)((!OpposedPosition) ? 1 : 0)) : ((float)(OpposedPosition ? 1 : 0)));
				result = new RectangleF(Location.X - num2 * Dimension, Location.Y - RealLength, Dimension, RealLength);
			}
			return result;
		}
		set
		{
			if (!m_autoSize)
			{
				if (m_orientation == ChartOrientation.Horizontal)
				{
					Location = new PointF(value.Left, value.Top);
					RealLength = value.Width;
				}
				else
				{
					Location = new PointF(value.Left, value.Bottom);
					RealLength = value.Width;
				}
			}
		}
	}

	public float RealLength
	{
		get
		{
			return m_length;
		}
		set
		{
			if (m_length != value)
			{
				m_length = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public float Dimension
	{
		get
		{
			return m_dimension;
		}
		set
		{
			if (m_dimension != value)
			{
				m_dimension = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public PointF Location
	{
		get
		{
			return m_location;
		}
		set
		{
			if (m_location != value)
			{
				m_location = value;
				if (double.IsNaN(Crossing))
				{
					OnDimensionsChanged(EventArgs.Empty);
				}
			}
		}
	}

	internal PointF AxisLineLocation
	{
		get
		{
			return m_axisLineLocation;
		}
		set
		{
			m_axisLineLocation = value;
		}
	}

	public double Crossing
	{
		get
		{
			return m_crossing;
		}
		set
		{
			m_crossing = value;
		}
	}

	public ChartAxisLocationType LocationType
	{
		get
		{
			return m_locationType;
		}
		set
		{
			if (m_locationType != value)
			{
				m_locationType = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public StringAlignment LabelAlignment
	{
		get
		{
			return m_labelAligment;
		}
		set
		{
			if (m_labelAligment != value)
			{
				m_labelAligment = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public bool RotateFromTicks
	{
		get
		{
			return m_rotateFromTicks;
		}
		set
		{
			if (m_rotateFromTicks != value)
			{
				m_rotateFromTicks = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public bool Margin
	{
		get
		{
			return m_margin;
		}
		set
		{
			if (m_margin != value)
			{
				m_margin = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public bool DrawTickLabelGrid
	{
		get
		{
			return m_drawTickLabelGrid;
		}
		set
		{
			if (m_drawTickLabelGrid != value)
			{
				m_drawTickLabelGrid = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public float TickLabelGridPadding
	{
		get
		{
			return m_tickLabelGridPadding;
		}
		set
		{
			if (m_tickLabelGridPadding != value)
			{
				m_tickLabelGridPadding = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public ChartAxisRange BreakRanges => m_breakRanges;

	public bool MakeBreaks
	{
		get
		{
			return m_makeBreaks;
		}
		set
		{
			if (m_makeBreaks != value)
			{
				m_makeBreaks = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public ChartAxisBreakInfo BreakInfo
	{
		get
		{
			return m_breakInfo;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (m_breakInfo != value)
			{
				m_breakInfo.Changed -= OnNeedRedraw;
				m_breakInfo = value;
				m_breakInfo.Changed += OnNeedRedraw;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public bool SmartDateZoom
	{
		get
		{
			return smartDateZoom;
		}
		set
		{
			if (smartDateZoom != value)
			{
				smartDateZoom = value;
			}
		}
	}

	public string SmartDateZoomLabelsCulture
	{
		get
		{
			return smartDateZoomLabelsCulture;
		}
		set
		{
			if (smartDateZoomLabelsCulture != value)
			{
				smartDateZoomLabelsCulture = value;
			}
		}
	}

	public string SmartDateZoomYearLevelLabelFormat
	{
		get
		{
			return smartDateZoomYearLevelLabelFormat;
		}
		set
		{
			if (smartDateZoomYearLevelLabelFormat != value)
			{
				smartDateZoomYearLevelLabelFormat = value;
			}
		}
	}

	public string SmartDateZoomMonthLevelLabelFormat
	{
		get
		{
			return smartDateZoomMonthLevelLabelFormat;
		}
		set
		{
			if (smartDateZoomMonthLevelLabelFormat != value)
			{
				smartDateZoomMonthLevelLabelFormat = value;
			}
		}
	}

	public string SmartDateZoomDayLevelLabelFormat
	{
		get
		{
			return smartDateZoomDayLevelLabelFormat;
		}
		set
		{
			if (smartDateZoomDayLevelLabelFormat != value)
			{
				smartDateZoomDayLevelLabelFormat = value;
			}
		}
	}

	public string SmartDateZoomWeekLevelLabelFormat
	{
		get
		{
			return smartDateZoomWeekLevelLabelFormat;
		}
		set
		{
			if (smartDateZoomWeekLevelLabelFormat != value)
			{
				smartDateZoomWeekLevelLabelFormat = value;
			}
		}
	}

	public string SmartDateZoomHourLevelLabelFormat
	{
		get
		{
			return smartDateZoomHourLevelLabelFormat;
		}
		set
		{
			if (smartDateZoomHourLevelLabelFormat != value)
			{
				smartDateZoomHourLevelLabelFormat = value;
			}
		}
	}

	public string SmartDateZoomMinuteLevelLabelFormat
	{
		get
		{
			return smartDateZoomMinuteLevelLabelFormat;
		}
		set
		{
			if (smartDateZoomMinuteLevelLabelFormat != value)
			{
				smartDateZoomMinuteLevelLabelFormat = value;
			}
		}
	}

	public string SmartDateZoomSecondLevelLabelFormat
	{
		get
		{
			return smartDateZoomSecondLevelLabelFormat;
		}
		set
		{
			if (smartDateZoomSecondLevelLabelFormat != value)
			{
				smartDateZoomSecondLevelLabelFormat = value;
			}
		}
	}

	public string CurrentSmartDateTimeFormat
	{
		get
		{
			SetSmartZoomFactor(1.0);
			return currentSmartDateTimeFormat;
		}
	}

	public bool InterlacedGrid
	{
		get
		{
			return m_interlacedGrid;
		}
		set
		{
			if (m_interlacedGrid != value)
			{
				m_interlacedGrid = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public BrushInfo InterlacedGridInterior
	{
		get
		{
			return m_interlacedGridInterior;
		}
		set
		{
			if (m_interlacedGridInterior != value)
			{
				m_interlacedGridInterior = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public ChartAxisEdgeLabelsDrawingMode EdgeLabelsDrawingMode
	{
		get
		{
			return edgeLabelsDrawingMode;
		}
		set
		{
			if (edgeLabelsDrawingMode != value)
			{
				edgeLabelsDrawingMode = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public bool CustomOrigin
	{
		get
		{
			return m_customOrigin;
		}
		set
		{
			if (m_customOrigin != value)
			{
				m_customOrigin = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public double Origin
	{
		get
		{
			return m_origin;
		}
		set
		{
			if (m_origin != value)
			{
				m_origin = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public DateTime OriginDate
	{
		get
		{
			return DateTime.FromOADate(m_origin);
		}
		set
		{
			Origin = value.ToOADate();
		}
	}

	internal bool IsDateTimeCategoryAxis
	{
		get
		{
			return m_isDateTimeCategoryAxis;
		}
		set
		{
			m_isDateTimeCategoryAxis = value;
		}
	}

	internal bool ChangeDateTimeAxisValue
	{
		get
		{
			return m_changeDateTimeAxisValue;
		}
		set
		{
			m_changeDateTimeAxisValue = value;
		}
	}

	public string DateTimeFormat
	{
		get
		{
			return m_dateTimeFormat;
		}
		set
		{
			if (m_dateTimeFormat != value)
			{
				m_dateTimeFormat = value;
				m_needUpdateVisibleLables = true;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public TimeSpan DateTimeOffset
	{
		get
		{
			return DateTime.FromOADate(0.0 - m_offset) - DateTime.FromOADate(0.0);
		}
		set
		{
			Offset = DateTime.Now.Add(value).ToOADate() - DateTime.Now.ToOADate();
		}
	}

	public ChartDateTimeRange DateTimeRange
	{
		get
		{
			return new ChartDateTimeRange(DateTime.FromOADate(m_range.Min), DateTime.FromOADate(m_range.Max), m_range.Interval, IntervalType);
		}
		set
		{
			m_dateTimeInterval = value.DefaultInterval;
			m_intervalType = m_dateTimeInterval.Type;
			double num = value.Start.ToOADate();
			double num2 = value.End.ToOADate();
			if (value.DefaultInterval.Type == ChartDateTimeIntervalType.Auto)
			{
				Range = new MinMaxInfo(num, num2, (num2 - num) / (double)Range.NumberOfIntervals);
				return;
			}
			float num3 = ChartDateTimeInterval.GetIntervalCount(value.DefaultInterval.Iterator());
			Range = new MinMaxInfo(num, num2, (num2 - num) / (double)num3);
		}
	}

	public ChartDateTimeInterval DateTimeInterval => m_dateTimeInterval;

	public int DesiredIntervals
	{
		get
		{
			return m_desiredIntervals;
		}
		set
		{
			if (m_desiredIntervals != value)
			{
				m_desiredIntervals = value;
				OnIntervalsChanged(EventArgs.Empty);
			}
		}
	}

	public ChartAxisRangePaddingType RangePaddingType
	{
		get
		{
			return m_rangePaddingType;
		}
		set
		{
			if (m_rangePaddingType != value)
			{
				m_rangePaddingType = value;
				OnIntervalsChanged(EventArgs.Empty);
			}
		}
	}

	public bool ForceZero
	{
		get
		{
			return m_forceZero;
		}
		set
		{
			m_forceZero = value;
			OnIntervalsChanged(EventArgs.Empty);
		}
	}

	public bool ForceZeroToDouble
	{
		get
		{
			return m_forceZeroToDouble;
		}
		set
		{
			m_forceZeroToDouble = value;
			OnIntervalsChanged(EventArgs.Empty);
		}
	}

	public bool PreferZero
	{
		get
		{
			return m_preferZero;
		}
		set
		{
			m_preferZero = value;
			OnIntervalsChanged(EventArgs.Empty);
		}
	}

	internal bool IsMax
	{
		get
		{
			return m_isMax;
		}
		set
		{
			m_isMax = value;
		}
	}

	public bool DrawGrid
	{
		get
		{
			return m_drawGrid;
		}
		set
		{
			if (m_drawGrid != value)
			{
				m_drawGrid = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public LogLabelsDisplayMode LogLabelsDisplayMode
	{
		get
		{
			return m_showLogBaseLabels;
		}
		set
		{
			m_showLogBaseLabels = value;
		}
	}

	public bool DrawMinorGrid
	{
		get
		{
			return m_drawMinorGrid;
		}
		set
		{
			if (m_drawMinorGrid != value)
			{
				m_drawMinorGrid = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public ChartAxisGridDrawingMode GridDrawMode
	{
		get
		{
			return m_gridDrawMode;
		}
		set
		{
			if (m_gridDrawMode != value)
			{
				m_gridDrawMode = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public ChartAxisLabelPlacement LabelPlacement
	{
		get
		{
			return m_labelPlacement;
		}
		set
		{
			if (m_labelPlacement != value)
			{
				m_labelPlacement = ((m_valueType == ChartValueType.Category || m_valueType == ChartValueType.DateTime) ? value : ChartAxisLabelPlacement.OnTicks);
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public LineInfo GridLineType
	{
		get
		{
			return m_gridLineType;
		}
		internal set
		{
			m_gridLineType = value;
		}
	}

	public LineInfo MinorGridLineType
	{
		get
		{
			return m_minorGridLineType;
		}
		internal set
		{
			m_minorGridLineType = value;
		}
	}

	public ChartAxisTickDrawingOperationMode TickDrawingOperationMode
	{
		get
		{
			return m_tickDrawingOperationMode;
		}
		set
		{
			if (m_tickDrawingOperationMode != value)
			{
				m_tickDrawingOperationMode = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public Font Font
	{
		get
		{
			if (m_font != null)
			{
				return m_font;
			}
			return m_area.Chart.Font;
		}
		set
		{
			if (m_font != value)
			{
				m_font = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public Font TitleFont
	{
		get
		{
			if (m_titleFont != null)
			{
				return m_titleFont;
			}
			return m_area.Chart.Font;
		}
		set
		{
			if (m_titleFont != value)
			{
				m_titleFont = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public Color ForeColor
	{
		get
		{
			if (m_foreColor.IsEmpty)
			{
				if (m_area == null)
				{
					return Color.Black;
				}
				return m_area.Chart.ForeColor;
			}
			return m_foreColor;
		}
		set
		{
			if (m_foreColor != value)
			{
				m_foreColor = value;
				m_needUpdateVisibleLables = true;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	internal BrushInfo BackInterior
	{
		get
		{
			return m_backInterior;
		}
		set
		{
			if (m_backInterior != value)
			{
				m_backInterior = value;
			}
		}
	}

	internal ChartTitle ChartAxisTitle
	{
		get
		{
			return m_chartAxisTitle;
		}
		set
		{
			m_chartAxisTitle = value;
		}
	}

	public Color TitleColor
	{
		get
		{
			if (m_titleColor.IsEmpty)
			{
				if (m_area == null)
				{
					return Color.Black;
				}
				return m_area.Chart.ForeColor;
			}
			return m_titleColor;
		}
		set
		{
			if (m_titleColor != value)
			{
				m_titleColor = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public ChartTitleDrawMode TitleDrawMode
	{
		get
		{
			return m_titleDrawMode;
		}
		set
		{
			if (m_titleDrawMode != value)
			{
				m_titleDrawMode = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public Pen[] Pens
	{
		get
		{
			return m_pens;
		}
		set
		{
			if (m_pens != value)
			{
				m_pens = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public string Format
	{
		get
		{
			return m_format;
		}
		set
		{
			if (m_format != value)
			{
				m_format = value;
				m_needUpdateVisibleLables = true;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public ChartDateTimeIntervalType IntervalType
	{
		get
		{
			return m_intervalType;
		}
		set
		{
			if (m_intervalType != value)
			{
				m_intervalType = value;
				OnIntervalsChanged(EventArgs.Empty);
			}
		}
	}

	public bool Inversed
	{
		get
		{
			return m_inversed;
		}
		set
		{
			if (m_inversed != value)
			{
				m_inversed = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public ChartLabelIntersectAction LabelIntersectAction
	{
		get
		{
			return m_intersectAction;
		}
		set
		{
			if (m_intersectAction != value)
			{
				m_intersectAction = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public bool HidePartialLabels
	{
		get
		{
			return m_hidePartialLabels;
		}
		set
		{
			if (m_hidePartialLabels != value)
			{
				m_hidePartialLabels = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public bool LabelRotate
	{
		get
		{
			return m_labelRotate;
		}
		set
		{
			if (m_labelRotate != value)
			{
				m_labelRotate = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public int LabelRotateAngle
	{
		get
		{
			return m_labelRotateAngle;
		}
		set
		{
			if (m_labelRotateAngle != value)
			{
				m_labelRotateAngle = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public ChartAxisLabelCollection Labels => m_labels as ChartAxisLabelCollection;

	public IChartAxisLabelModel LabelsImpl
	{
		get
		{
			return m_labels;
		}
		set
		{
			if (m_labels != value)
			{
				if (Labels != null)
				{
					Labels.Changed -= OnNeedResize;
				}
				m_labels = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public ChartAxisGroupingLabelCollection GroupingLabels => m_groupingLabels as ChartAxisGroupingLabelCollection;

	public IChartAxisGroupingLabelModel GroupingLabelsImpl
	{
		get
		{
			return m_groupingLabels;
		}
		set
		{
			if (GroupingLabels != null)
			{
				GroupingLabels.Changed -= OnNeedResize;
			}
			m_groupingLabels = value;
			OnDimensionsChanged(EventArgs.Empty);
		}
	}

	public LineInfo LineType
	{
		get
		{
			return m_lineType;
		}
		internal set
		{
			m_lineType = value;
		}
	}

	public int LogBase
	{
		get
		{
			return m_logBase;
		}
		set
		{
			if (m_logBase != value)
			{
				m_logBase = value;
				OnIntervalsChanged(EventArgs.Empty);
			}
		}
	}

	public double Offset
	{
		get
		{
			return m_offset;
		}
		set
		{
			if (m_offset != value)
			{
				m_offset = value;
				InvalidateRanges();
			}
		}
	}

	public double PointOffset
	{
		get
		{
			return m_pointOffset;
		}
		set
		{
			if (m_pointOffset != value)
			{
				m_pointOffset = value;
				InvalidateRanges();
			}
		}
	}

	public bool OpposedPosition
	{
		get
		{
			return m_opposedPosition;
		}
		set
		{
			if (m_opposedPosition != value)
			{
				m_opposedPosition = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public ChartPlacement AxisLabelPlacement
	{
		get
		{
			return m_axisLabelPlacement;
		}
		set
		{
			if (m_axisLabelPlacement != value)
			{
				m_axisLabelPlacement = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public ChartOrientation Orientation
	{
		get
		{
			return m_orientation;
		}
		set
		{
			if (m_orientation != value)
			{
				m_orientation = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public MinMaxInfo Range
	{
		get
		{
			return m_range;
		}
		set
		{
			RangeType = ChartAxisRangeType.Set;
			SetRange(value);
		}
	}

	internal bool IsMaxSet
	{
		get
		{
			return m_isMaxSet;
		}
		set
		{
			m_isMaxSet = value;
		}
	}

	internal bool IsMinSet
	{
		get
		{
			return m_isMinSet;
		}
		set
		{
			m_isMinSet = value;
		}
	}

	internal bool IsMajorSet
	{
		get
		{
			return m_isMajorSet;
		}
		set
		{
			m_isMajorSet = value;
		}
	}

	public ChartAxisRangeType RangeType
	{
		get
		{
			return m_rangeType;
		}
		set
		{
			if (m_rangeType != value)
			{
				m_rangeType = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public int RoundingPlaces
	{
		get
		{
			return m_roundingPlaces;
		}
		set
		{
			if (m_roundingPlaces != value)
			{
				m_roundingPlaces = value;
				m_needUpdateVisibleLables = true;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public Size SmallTickSize
	{
		get
		{
			return m_smallTickSize;
		}
		set
		{
			if (m_smallTickSize != value)
			{
				m_smallTickSize = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public int SmallTicksPerInterval
	{
		get
		{
			return m_smallTicksPerInterval;
		}
		set
		{
			if (m_smallTicksPerInterval != value)
			{
				m_smallTicksPerInterval = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public ChartStripLineCollection StripLines => m_stripLines;

	public Color TickColor
	{
		get
		{
			return m_tickColor;
		}
		set
		{
			if (m_tickColor != value)
			{
				m_tickColor = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public Size TickSize
	{
		get
		{
			return m_tickSize;
		}
		set
		{
			if (m_tickSize != value)
			{
				m_tickSize = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			if (m_title != value)
			{
				m_title = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public float TitleSpacing
	{
		get
		{
			return m_titleSpacing;
		}
		set
		{
			if (m_titleSpacing != value)
			{
				m_titleSpacing = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public StringAlignment TitleAlignment
	{
		get
		{
			return m_titleAlignment;
		}
		set
		{
			if (m_titleAlignment != value)
			{
				m_titleAlignment = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	public string ToolTip
	{
		get
		{
			return m_toolTip;
		}
		set
		{
			if (m_toolTip != value)
			{
				m_toolTip = value;
			}
		}
	}

	public ChartValueType ValueType
	{
		get
		{
			return m_valueType;
		}
		set
		{
			if (m_valueType != value)
			{
				m_valueType = value;
				OnIntervalsChanged(EventArgs.Empty);
			}
		}
	}

	public ChartAxisTickLabelDrawingMode TickLabelsDrawingMode
	{
		get
		{
			return tickLabelDrawingMode;
		}
		set
		{
			if (tickLabelDrawingMode != value)
			{
				tickLabelDrawingMode = value;
				OnIntervalsChanged(EventArgs.Empty);
			}
		}
	}

	public virtual MinMaxInfo VisibleRange
	{
		get
		{
			if (m_needUpdate)
			{
				RecalculateRanges();
				m_needUpdate = false;
			}
			return m_visibleRange;
		}
	}

	public ChartCustomLabelsParameter CustomLabelsParameter
	{
		get
		{
			return m_customLabelsParameter;
		}
		set
		{
			m_customLabelsParameter = value;
		}
	}

	protected ChartOrientation DefaultOrientation => m_defaultAxisOrienation;

	internal ChartAxisLayout Layout
	{
		get
		{
			return m_layout;
		}
		set
		{
			if (m_layout == null)
			{
				m_layout = value;
				return;
			}
			if (value == null)
			{
				m_layout = value;
				return;
			}
			throw new ArgumentException("Axis is added to layout already.");
		}
	}

	internal bool Primary
	{
		get
		{
			if (m_area != null)
			{
				if (m_orientation == ChartOrientation.Horizontal)
				{
					return this == m_area.PrimaryXAxis;
				}
				return this == m_area.PrimaryYAxis;
			}
			return false;
		}
	}

	internal ChartAxisLabel[] VisibleLabels
	{
		get
		{
			if (m_needUpdateVisibleLables)
			{
				RecalculateVisibleLabels(m_area);
				m_needUpdateVisibleLables = false;
			}
			return m_visibleLables;
		}
	}

	internal bool IsIndexed
	{
		get
		{
			if (m_area == null)
			{
				return false;
			}
			if (m_area.Chart == null)
			{
				return false;
			}
			if (m_area.AxesType == ChartAreaAxesType.Rectangular)
			{
				if (m_area.RequireInvertedAxes)
				{
					if (m_orientation == ChartOrientation.Horizontal)
					{
						return false;
					}
				}
				else if (m_orientation == ChartOrientation.Vertical)
				{
					return false;
				}
				return m_area.IsIndexed;
			}
			return false;
		}
	}

	private bool BreaksEnabled
	{
		get
		{
			if (!m_makeBreaks)
			{
				return false;
			}
			if (m_breakRanges.BreaksMode == ChartBreaksMode.Auto && m_valueType != 0)
			{
				return false;
			}
			return !m_breakRanges.IsEmpty;
		}
	}

	public bool IsVisible
	{
		get
		{
			return m_isVisible;
		}
		set
		{
			if (m_isVisible != value)
			{
				m_isVisible = value;
				OnDimensionsChanged(EventArgs.Empty);
			}
		}
	}

	public bool ExcludeInvisibleSeriesRange
	{
		get
		{
			return m_excludeInvisibleSeriesRange;
		}
		set
		{
			if (m_excludeInvisibleSeriesRange != value)
			{
				m_excludeInvisibleSeriesRange = value;
			}
		}
	}

	public double LabelsOffset
	{
		get
		{
			return m_labelsOffset;
		}
		set
		{
			if (m_labelsOffset != value)
			{
				m_labelsOffset = value;
				OnIntervalsChanged(EventArgs.Empty);
			}
		}
	}

	public StringFormat LabelStringFormat
	{
		get
		{
			return m_labelStringFormat;
		}
		set
		{
			if (m_labelStringFormat != value)
			{
				m_labelStringFormat = value;
			}
		}
	}

	internal double DisplayUnits
	{
		get
		{
			return m_displayUnits;
		}
		set
		{
			m_displayUnits = value;
		}
	}

	internal AxisLabelConverter AxisLabelConverter
	{
		get
		{
			return m_axisLabelConverter;
		}
		set
		{
			m_axisLabelConverter = value;
		}
	}

	[Browsable(false)]
	public event EventHandler AppearanceChanged;

	[Browsable(false)]
	public event EventHandler DimensionsChanged;

	[Browsable(false)]
	public event EventHandler IntervalsChanged;

	[Browsable(false)]
	public event EventHandler VisibleRangeChanged;

	[Browsable(false)]
	public event ChartFormatAxisLabelEventHandler FormatLabel;

	protected void ResetForeColor()
	{
		ForeColor = Color.Empty;
	}

	protected void ResetTitleColor()
	{
		TitleColor = Color.Empty;
	}

	protected void ResetTickColor()
	{
		TickColor = SystemColors.ControlText;
	}

	protected bool ShouldSerializeDateTimeOffset()
	{
		if (DateTimeOffset == TimeSpan.Parse("00:00:00"))
		{
			return false;
		}
		return true;
	}

	protected bool ShouldSerializeForeColor()
	{
		return !m_foreColor.IsEmpty;
	}

	protected bool ShouldSerializeTitleColor()
	{
		return !m_titleColor.IsEmpty;
	}

	protected bool ShouldSerializeOriginDate()
	{
		return m_origin != 0.0;
	}

	protected bool ShouldSerializeSmallTickSize()
	{
		if (SmallTickSize.Height == 1)
		{
			return SmallTickSize.Width != 1;
		}
		return true;
	}

	protected bool ShouldSerializeTickColor()
	{
		return TickColor != SystemColors.ControlText;
	}

	protected bool ShouldSerializeTickSize()
	{
		if (TickSize.Height == 1 && TickSize.Width == 1)
		{
			return false;
		}
		return true;
	}

	protected bool ShouldSerializeSize()
	{
		return false;
	}

	protected bool ShouldSerializeRange()
	{
		return m_rangeType == ChartAxisRangeType.Set;
	}

	protected bool ShouldSerializeOrientation()
	{
		return Orientation != DefaultOrientation;
	}

	protected void ResetOrientation()
	{
		Orientation = m_defaultAxisOrienation;
	}

	protected bool ShouldSerializeInterlacedGridInterior()
	{
		return InterlacedGridInterior != new BrushInfo(Color.LightGray);
	}

	protected void ResetInterlacedGridInterior()
	{
		InterlacedGridInterior = new BrushInfo(Color.LightGray);
	}

	public ChartAxis()
		: this(ChartOrientation.Horizontal)
	{
	}

	public ChartAxis(ChartOrientation orientation)
	{
		m_orientation = orientation;
		m_defaultAxisOrienation = orientation;
		if (ValueType == ChartValueType.DateTime)
		{
			m_dateTimeInterval = DateTimeRange.DefaultInterval;
		}
		ChartAxisLabelCollection chartAxisLabelCollection = new ChartAxisLabelCollection();
		chartAxisLabelCollection.Changed += OnNeedResize;
		m_labels = chartAxisLabelCollection;
		m_breakInfo.Changed += OnNeedRedraw;
		m_breakRanges = new ChartAxisRange(this);
		m_breakRanges.Changed += OnBreakRangesChanged;
		ChartAxisGroupingLabelCollection chartAxisGroupingLabelCollection = new ChartAxisGroupingLabelCollection();
		chartAxisGroupingLabelCollection.Changed += OnNeedResize;
		m_groupingLabels = chartAxisGroupingLabelCollection;
		m_stripLines = new ChartStripLineCollection();
		m_stripLines.Changed += OnNeedRedraw;
		m_lineType = new LineInfo();
		m_lineType.SettingsChanged += OnNeedRedraw;
		m_gridLineType = new LineInfo();
		m_gridLineType.SettingsChanged += OnNeedRedraw;
		m_minorGridLineType.SettingsChanged += OnNeedRedraw;
		m_range.SettingsChanged += OnNeedResize;
	}

	public string GetCategoryAtIndex(int index)
	{
		if (index > -1 && index < SortLabels.Length)
		{
			return SortLabels.GetValue(index).ToString();
		}
		return index.ToString();
	}

	internal void SetOwner(ChartArea area)
	{
		m_area = area;
	}

	internal void Unsubscribe(object target)
	{
		this.AppearanceChanged = Unsubscribe(this.AppearanceChanged, target) as EventHandler;
		this.DimensionsChanged = Unsubscribe(this.DimensionsChanged, target) as EventHandler;
		this.IntervalsChanged = Unsubscribe(this.IntervalsChanged, target) as EventHandler;
		this.VisibleRangeChanged = Unsubscribe(this.VisibleRangeChanged, target) as EventHandler;
		this.FormatLabel = Unsubscribe(this.FormatLabel, target) as ChartFormatAxisLabelEventHandler;
	}

	internal Delegate Unsubscribe(Delegate del, object target)
	{
		if ((object)del == null)
		{
			return null;
		}
		if (target == null)
		{
			return del;
		}
		Delegate[] invocationList = del.GetInvocationList();
		for (int i = 0; i < invocationList.Length; i++)
		{
			if (target == invocationList[i].Target)
			{
				invocationList[i] = null;
			}
		}
		return Delegate.Combine(invocationList);
	}

	public float GetDimension(Graphics g, ChartArea chartarea)
	{
		return GetDimension(g, chartarea, chartarea.RenderBounds);
	}

	internal float GetDimension(Graphics g, ChartArea chartarea, bool drawaxis)
	{
		return GetDimension(g, chartarea, chartarea.RenderBounds, drawaxis);
	}

	public bool CheckFormatLabel()
	{
		return this.FormatLabel != null;
	}

	public float GetDimension(Graphics g, ChartArea chartarea, RectangleF renderBounds)
	{
		return GetDimension(g, chartarea, renderBounds, drawaxis: false);
	}

	internal float GetDimension(Graphics g, ChartArea chartarea, RectangleF renderBounds, bool drawaxis)
	{
		float num = ((m_orientation == ChartOrientation.Vertical) ? (3 * m_tickSize.Width) : m_tickSize.Height);
		if (m_needUpdateVisibleLables)
		{
			RecalculateVisibleLabels(chartarea);
			m_needUpdateVisibleLables = false;
		}
		num = (m_ticksAndLabelsDimension = num + DoTickLabelsLayout(g, Rect, m_titleSpacing, chartarea));
		num += DoGroupingLabelsLayout(g, num);
		string text = string.Empty;
		List<string> list = new List<string>();
		float rectwidth = ((m_orientation == ChartOrientation.Vertical) ? renderBounds.Height : renderBounds.Width);
		string[] array = Title.Split('\n');
		foreach (string text2 in array)
		{
			string[] array2 = SplitTextToFitRectangle(text2, rectwidth, g, updateAxisTitle: true, chartarea);
			_ = array2.Length;
			for (int j = 0; j < array2.Length; j++)
			{
				list.Add(array2[j]);
			}
		}
		string[] array3 = list.ToArray();
		for (int k = 0; k < array3.Length; k++)
		{
			text = ((k >= array3.Length - 1) ? (text + array3[k]) : (text + array3[k] + "\n"));
		}
		if (drawaxis)
		{
			Title = text;
		}
		for (int l = 0; l < array3.Length; l++)
		{
			float width = ((m_orientation == ChartOrientation.Vertical) ? renderBounds.Height : renderBounds.Width);
			if (string.IsNullOrEmpty(array3[l]) || AxisLabelPlacement != ChartPlacement.Outside)
			{
				continue;
			}
			num += m_titleSpacing;
			if (m_titleDrawMode == ChartTitleDrawMode.Wrap)
			{
				num += g.MeasureString(array3[l], TitleFont).Height;
				continue;
			}
			if (m_titleDrawMode == ChartTitleDrawMode.Ellipsis)
			{
				array3[l] = DrawingHelper.EllipsesText(g, array3[l], TitleFont, width);
			}
			num += g.MeasureString(array3[l], TitleFont).Height;
		}
		if (!m_isVisible)
		{
			return m_dimension = 0f;
		}
		return m_dimension = num;
	}

	private string[] SplitTextToFitRectangle(string text, float rectwidth, Graphics g, bool updateAxisTitle, ChartArea area)
	{
		string[] array = text.Split(' ');
		string text2 = "";
		List<string> list = new List<string>();
		string[] array2 = array;
		foreach (string text3 in array2)
		{
			string text4 = text2 + text3 + " ";
			if (g.MeasureString(text4, TitleFont).Width > rectwidth)
			{
				if (updateAxisTitle)
				{
					float legendLength = 0f;
					float num = UpdatedAxisTitleLength(area, out legendLength);
					if (num == 0f)
					{
						num = 10f;
					}
					if (Orientation == ChartOrientation.Vertical)
					{
						if (!Primary)
						{
							Location = new PointF(Location.X, Location.Y + 20f);
						}
						else
						{
							Location = new PointF(Location.X, Location.Y + 20f + legendLength / 2f);
						}
					}
					float rectwidth2 = rectwidth + num;
					if (Orientation == ChartOrientation.Horizontal)
					{
						if (Location.X + rectwidth + num > (float)m_area.Chart.Bounds.Width)
						{
							rectwidth2 = rectwidth;
						}
						return SplitTextToFitRectangle(text, rectwidth2, g, updateAxisTitle: false, area);
					}
					if (rectwidth + num > (float)m_area.Chart.Bounds.Height && !OpposedPosition)
					{
						rectwidth2 = rectwidth;
					}
					return SplitTextToFitRectangle(text, rectwidth2, g, updateAxisTitle: false, area);
				}
				if (text2 != "")
				{
					list.Add(text2.Trim());
				}
				text2 = text3 + " ";
			}
			else
			{
				text2 = text4;
			}
		}
		list.Add(text2.Trim());
		return list.ToArray();
	}

	internal float UpdatedAxisTitleLength(ChartArea area, out float legendLength)
	{
		legendLength = 0f;
		foreach (ChartAxis axis in area.Axes)
		{
			if (axis.m_orientation != Orientation && ((m_orientation == ChartOrientation.Vertical && axis.Primary && Primary) || axis.OpposedPosition != OpposedPosition) && axis.Title != "")
			{
				ChartControl chartControl = area.Chart as ChartControl;
				if (m_orientation == ChartOrientation.Vertical && axis.Primary && Primary && chartControl.ShowLegend && chartControl.LegendPosition == ChartDock.Bottom)
				{
					legendLength = chartControl.Legend.Height;
				}
				float dimension = axis.Dimension;
				if (Orientation == ChartOrientation.Vertical && chartControl.ShowLegend && (chartControl.LegendPosition == ChartDock.Left || chartControl.LegendPosition == ChartDock.Right))
				{
					return dimension + 40f;
				}
				if (chartControl.Title.Text != "" && chartControl.Title.Position == ChartDock.Top && m_opposedPosition && Orientation == ChartOrientation.Horizontal)
				{
					return dimension + 40f;
				}
				return dimension;
			}
		}
		return 0f;
	}

	internal float GetTitleDimention(Graphics g, ChartArea chartarea, RectangleF renderBounds)
	{
		float num = 0f;
		string text = Title;
		float num2 = ((m_orientation == ChartOrientation.Vertical) ? renderBounds.Height : renderBounds.Width);
		if (!string.IsNullOrEmpty(text) && AxisLabelPlacement == ChartPlacement.Inside)
		{
			num += m_titleSpacing;
			if (m_titleDrawMode == ChartTitleDrawMode.Wrap)
			{
				num += g.MeasureString(text, TitleFont, (int)num2).Height;
			}
			else
			{
				if (m_titleDrawMode == ChartTitleDrawMode.Ellipsis)
				{
					text = DrawingHelper.EllipsesText(g, text, TitleFont, num2);
				}
				num += g.MeasureString(text, TitleFont).Height;
			}
		}
		return num;
	}

	public void DrawAxis(Graphics g, ChartArea chartarea)
	{
		DrawAxis(g, chartarea, RectangleF.Empty);
	}

	public void DrawAxis(Graphics g, ChartArea chartarea, RectangleF labelBounds)
	{
		if (chartarea.RequireInvertedAxes)
		{
			CrossInvertedAxis(chartarea);
		}
		else
		{
			CrossAxis(chartarea);
		}
		bool flag = false;
		for (int i = 0; i < chartarea.Chart.Series.Count; i++)
		{
			string text = chartarea.Chart.Series[i].Type.ToString();
			if (text.Contains("Column") || text.Contains("Area") || text.Contains("Histogram") || text.Contains("WaterFall") || text.Contains("Pareto") || text.Contains("Bar") || text.Contains("Pie") || text.Contains("Radar"))
			{
				flag = true;
			}
		}
		adjustPlotAreaMargins = chartarea.AdjustPlotAreaMargins;
		chartPlotAreaMargins = chartarea.ChartPlotAreaMargins;
		PointF location = Location;
		if (m_needUpdateDimension)
		{
			GetDimension(g, chartarea, drawaxis: true);
			if (!chartarea.ReDrawAxes)
			{
				m_needUpdateDimension = false;
			}
		}
		DrawAxisText(g, Rect);
		Location = location;
		float num = ((AxisLabelPlacement == ChartPlacement.Inside) ? ((GroupingLabels.Count <= 0) ? ((float)(OpposedPosition ? 3 : (-3))) : ((float)(OpposedPosition ? 1 : (-1)))) : ((GroupingLabels.Count <= 0) ? ((float)(OpposedPosition ? (-3) : 3)) : ((float)((!OpposedPosition) ? 1 : (-1)))));
		Pen pen;
		Pen pen2;
		if (Orientation == ChartOrientation.Horizontal)
		{
			pen = new Pen(m_tickColor, m_tickSize.Width);
			pen2 = new Pen(m_tickColor, SmallTickSize.Width);
			if ((chartarea.RequireInvertedAxes && AxisLineLocation != PointF.Empty && IsVerticalAxisAutoCross) || ((!chartarea.RequireInvertedAxes && AxisLineLocation != PointF.Empty && ValueType == ChartValueType.Double) ? m_isVerticalAxisAutoCross : IsHorizontalAxisAutoCross))
			{
				g.DrawLine(LineType.Pen, AxisLineLocation.X, AxisLineLocation.Y, AxisLineLocation.X + RealLength, AxisLineLocation.Y);
			}
			else
			{
				g.DrawLine(LineType.Pen, Location.X, Location.Y, Location.X + RealLength, Location.Y);
			}
			float num2 = num;
			if (m_labelPlacement == ChartAxisLabelPlacement.OnTicks)
			{
				for (int j = 0; j < m_visibleLables.Length; j++)
				{
					num = ((m_visibleLables[j].AxisLabelPlacement == AxisLabelPlacement) ? num2 : (0f - num));
					float coordinateFromValue = GetCoordinateFromValue(m_visibleLables[j].DoubleValue);
					g.DrawLine(pen, coordinateFromValue, Location.Y, coordinateFromValue, Location.Y + num * (float)TickSize.Height);
				}
			}
			else
			{
				for (double num3 = VisibleRange.Min; num3 <= VisibleRange.Max; num3 += VisibleRange.Interval)
				{
					float coordinateFromValue2 = GetCoordinateFromValue(num3);
					g.DrawLine(pen, coordinateFromValue2, Location.Y, coordinateFromValue2, Location.Y + num * (float)TickSize.Height);
				}
			}
			if (m_smallTicksPerInterval > 0)
			{
				foreach (float smallTick in GetSmallTicks())
				{
					g.DrawLine(pen2, smallTick, Location.Y, smallTick, Location.Y + num * (float)SmallTickSize.Height);
				}
			}
		}
		else
		{
			pen = new Pen(TickColor, TickSize.Height);
			pen2 = new Pen(TickColor, SmallTickSize.Height);
			if (((chartarea.RequireInvertedAxes && AxisLineLocation != PointF.Empty && ValueType == ChartValueType.Double) ? IsVerticalAxisAutoCross : IsHorizontalAxisAutoCross) || (!chartarea.RequireInvertedAxes && AxisLineLocation != PointF.Empty && IsVerticalAxisAutoCross))
			{
				g.DrawLine(LineType.Pen, AxisLineLocation.X, AxisLineLocation.Y, AxisLineLocation.X, AxisLineLocation.Y - RealLength);
			}
			else
			{
				g.DrawLine(LineType.Pen, Location.X, Location.Y, Location.X, Location.Y - RealLength);
			}
			float num4 = num;
			if (m_labelPlacement == ChartAxisLabelPlacement.OnTicks)
			{
				for (int k = 0; k < m_visibleLables.Length; k++)
				{
					num = ((m_visibleLables[k].AxisLabelPlacement == AxisLabelPlacement) ? num4 : (0f - num));
					float coordinateFromValue3 = GetCoordinateFromValue(m_visibleLables[k].DoubleValue);
					g.DrawLine(pen, Location.X, coordinateFromValue3, Location.X - num * (float)TickSize.Width, coordinateFromValue3);
				}
			}
			else
			{
				for (double num5 = VisibleRange.Min; num5 <= VisibleRange.Max; num5 += VisibleRange.Interval)
				{
					float coordinateFromValue4 = GetCoordinateFromValue(num5);
					g.DrawLine(pen, Location.X, coordinateFromValue4, Location.X - num * (float)TickSize.Width, coordinateFromValue4);
				}
			}
			if (m_smallTicksPerInterval > 0)
			{
				foreach (float smallTick2 in GetSmallTicks())
				{
					g.DrawLine(pen2, Location.X, smallTick2, Location.X - num * (float)SmallTickSize.Width, smallTick2);
				}
			}
		}
		if (flag)
		{
			DrawTickLabels(g, chartarea, labelBounds);
		}
		if (m_drawTickLabelGrid)
		{
			DrawTickLabelGrids(g, pen, Rect);
		}
		pen.Dispose();
		pen2.Dispose();
		if (GroupingLabelsImpl.Count > 0)
		{
			bool flag2 = m_orientation == ChartOrientation.Horizontal;
			bool flag3 = (m_opposedPosition ? (m_axisLabelPlacement == ChartPlacement.Outside) : (m_axisLabelPlacement == ChartPlacement.Inside));
			float x = (flag2 ? m_location.X : (flag3 ? m_location.X : (m_location.X - (float)chartarea.Width)));
			float y = ((!flag2) ? (m_location.Y - m_length) : (flag3 ? (m_location.Y - (float)chartarea.Height) : m_location.Y));
			RectangleF clip = new RectangleF(x, y, flag2 ? (m_length + 1f) : ((float)chartarea.Width), (!flag2) ? (m_length + 1f) : ((float)chartarea.Height));
			g.SetClip(clip);
			for (int l = 0; l < GroupingLabelsImpl.Count; l++)
			{
				ChartAxisGroupingLabel groupingLabelAt = GroupingLabelsImpl.GetGroupingLabelAt(l);
				float num6 = num * groupingLabelsRowsDimensions[groupingLabelAt.Row];
				num6 = ((m_orientation != 0) ? (m_location.X - num6) : (m_location.Y + num6));
				groupingLabelAt.Draw(g, num6, this);
			}
			g.ResetClip();
		}
	}

	internal void CrossAxis(ChartArea chartarea)
	{
		double num = 0.0;
		if (!double.IsNaN(Crossing))
		{
			if (Orientation == ChartOrientation.Vertical)
			{
				Crossing = ((Crossing == double.MaxValue) ? chartarea.PrimaryXAxis.Range.Max : ((Crossing == double.MinValue) ? chartarea.PrimaryXAxis.Range.Min : Crossing));
				if (chartarea.PrimaryXAxis.ValueType == ChartValueType.Double && chartarea.PrimaryXAxis.Range.Min < 0.0)
				{
					Location = new PointF(chartarea.RenderBounds.Left + (int)chartarea.PrimaryXAxis.GetVisibleValue(Crossing), m_location.Y);
				}
			}
			else
			{
				Crossing = ((Crossing == double.MaxValue) ? chartarea.PrimaryYAxis.Range.Max : ((Crossing == double.MinValue) ? chartarea.PrimaryYAxis.Range.Min : Crossing));
				float y = Location.Y;
				Location = new PointF(m_location.X, chartarea.RenderBounds.Bottom - (int)chartarea.PrimaryYAxis.GetVisibleValue(Crossing));
				if (chartarea.PrimaryYAxis.ValueType == ChartValueType.Logarithmic && VisibleLabels.Length != 0 && Location.Y > VisibleLabels[0].Bounds.Y)
				{
					Location = new PointF(m_location.X, y);
				}
			}
		}
		if (((Orientation == ChartOrientation.Horizontal && ValueType == ChartValueType.Double) ? IsVerticalAxisAutoCross : IsHorizontalAxisAutoCross) && chartarea.PrimaryYAxis.Range.Min <= 0.0)
		{
			num = ((num == double.MaxValue) ? chartarea.PrimaryYAxis.Range.Max : ((num == double.MinValue) ? chartarea.PrimaryYAxis.Range.Min : num));
			float y2 = Location.Y;
			m_axisLineLocation = new PointF(m_location.X, chartarea.RenderBounds.Bottom - (int)chartarea.PrimaryYAxis.GetVisibleValue(num));
			if (chartarea.PrimaryYAxis.ValueType == ChartValueType.Logarithmic && VisibleLabels.Length != 0 && Location.Y > VisibleLabels[0].Bounds.Y)
			{
				Location = new PointF(m_location.X, y2);
			}
		}
		if (Orientation == ChartOrientation.Vertical && IsVerticalAxisAutoCross && chartarea.PrimaryXAxis.ValueType == ChartValueType.Double && chartarea.PrimaryXAxis.Range.Min < 0.0 && chartarea.PrimaryXAxis.Range.Min < 0.0)
		{
			num = ((num == double.MaxValue) ? chartarea.PrimaryXAxis.Range.Max : ((num == double.MinValue) ? chartarea.PrimaryXAxis.Range.Min : num));
			m_axisLineLocation = new PointF(chartarea.RenderBounds.Left + (int)chartarea.PrimaryXAxis.GetVisibleValue(num), m_location.Y);
		}
	}

	internal void CrossInvertedAxis(ChartArea chartarea)
	{
		double num = 0.0;
		if (!double.IsNaN(Crossing))
		{
			if (Orientation == ChartOrientation.Vertical)
			{
				Crossing = ((Crossing == double.MaxValue) ? chartarea.PrimaryXAxis.Range.Max : ((Crossing == double.MinValue) ? chartarea.PrimaryXAxis.Range.Min : Crossing));
				Location = new PointF(chartarea.RenderBounds.Left + (int)chartarea.PrimaryXAxis.GetVisibleValue(Crossing), m_location.Y);
			}
			else
			{
				Crossing = ((Crossing == double.MaxValue) ? chartarea.PrimaryYAxis.Range.Max : ((Crossing == double.MinValue) ? chartarea.PrimaryYAxis.Range.Min : Crossing));
				if (chartarea.PrimaryYAxis.ValueType == ChartValueType.Double && chartarea.PrimaryYAxis.Range.Min < 0.0)
				{
					Location = new PointF(m_location.X, chartarea.RenderBounds.Bottom - (int)chartarea.PrimaryYAxis.GetVisibleValue(Crossing));
				}
			}
		}
		if (Orientation == ChartOrientation.Horizontal && IsVerticalAxisAutoCross && chartarea.PrimaryYAxis.ValueType == ChartValueType.Double && chartarea.PrimaryYAxis.Range.Min < 0.0)
		{
			num = ((num == double.MaxValue) ? chartarea.PrimaryYAxis.Range.Max : ((num == double.MinValue) ? chartarea.PrimaryYAxis.Range.Min : num));
			AxisLineLocation = new PointF(m_location.X, chartarea.RenderBounds.Bottom - (int)chartarea.PrimaryYAxis.GetVisibleValue(num));
		}
		else if ((Orientation == ChartOrientation.Vertical && ValueType == ChartValueType.Double) ? IsVerticalAxisAutoCross : IsHorizontalAxisAutoCross)
		{
			num = ((num == double.MaxValue) ? chartarea.PrimaryXAxis.Range.Max : ((num == double.MinValue) ? chartarea.PrimaryXAxis.Range.Min : num));
			AxisLineLocation = new PointF(chartarea.RenderBounds.Left + (int)chartarea.PrimaryXAxis.GetVisibleValue(num), m_location.Y);
		}
	}

	public void DrawAxis(Graphics3D g, ChartArea chartarea, float z)
	{
		if (VisibleRange.Delta <= 0.0)
		{
			return;
		}
		if (chartarea.RequireInvertedAxes)
		{
			CrossInvertedAxis(chartarea);
		}
		else
		{
			CrossAxis(chartarea);
		}
		ArrayList arrayList = new ArrayList();
		float num = ((AxisLabelPlacement != 0) ? ((float)((!OpposedPosition) ? 1 : (-1))) : ((float)(OpposedPosition ? 1 : (-1))));
		GetDimension(g.Graphics, chartarea);
		adjustPlotAreaMargins = chartarea.AdjustPlotAreaMargins;
		chartPlotAreaMargins = chartarea.ChartPlotAreaMargins;
		GraphicsPath graphicsPath = new GraphicsPath();
		Pen pen;
		Pen pen2;
		if (Orientation == ChartOrientation.Horizontal)
		{
			pen = new Pen(m_tickColor, m_tickSize.Width);
			pen2 = new Pen(m_tickColor, SmallTickSize.Width);
			_ = Rect.X;
			if (m_opposedPosition)
			{
				_ = m_location.Y;
				_ = m_tickSize.Height;
			}
			else
			{
				_ = m_location.Y;
			}
			graphicsPath.AddLines(new PointF[4]
			{
				new PointF(m_location.X + m_length, m_location.Y),
				new PointF(m_location.X, m_location.Y),
				new PointF(m_location.X, m_location.Y + 1f),
				new PointF(m_location.X, m_location.Y)
			});
			graphicsPath.CloseFigure();
			if (m_labelPlacement == ChartAxisLabelPlacement.OnTicks)
			{
				for (int i = 0; i < m_visibleLables.Length; i++)
				{
					float coordinateFromValue = GetCoordinateFromValue(m_visibleLables[i].DoubleValue);
					graphicsPath.AddLine(coordinateFromValue, Location.Y, coordinateFromValue, Location.Y + num * (float)TickSize.Height);
					graphicsPath.CloseFigure();
				}
			}
			else
			{
				for (double num2 = VisibleRange.Min; num2 <= VisibleRange.Max; num2 += VisibleRange.Interval)
				{
					float coordinateFromValue2 = GetCoordinateFromValue(num2);
					graphicsPath.AddLine(coordinateFromValue2, Location.Y, coordinateFromValue2, Location.Y + num * (float)TickSize.Height);
					graphicsPath.CloseFigure();
				}
			}
			if (m_smallTicksPerInterval > 0)
			{
				foreach (float smallTick in GetSmallTicks())
				{
					graphicsPath.AddLine(smallTick, Location.Y, smallTick, Location.Y + num * (float)SmallTickSize.Height);
					graphicsPath.CloseFigure();
				}
			}
		}
		else
		{
			pen = new Pen(TickColor, TickSize.Height);
			pen2 = new Pen(TickColor, SmallTickSize.Height);
			graphicsPath.AddLines(new PointF[4]
			{
				new PointF(Location.X, Location.Y - RealLength),
				new PointF(Location.X, Location.Y),
				new PointF(Location.X + 1f, Location.Y),
				new PointF(Location.X, Location.Y)
			});
			graphicsPath.CloseFigure();
			if (m_labelPlacement == ChartAxisLabelPlacement.OnTicks)
			{
				for (int j = 0; j < m_visibleLables.Length; j++)
				{
					float coordinateFromValue3 = GetCoordinateFromValue(m_visibleLables[j].DoubleValue);
					graphicsPath.AddLine(Location.X, coordinateFromValue3, Location.X - num * (float)TickSize.Width, coordinateFromValue3);
					graphicsPath.CloseFigure();
				}
			}
			else
			{
				for (double num3 = VisibleRange.Min; num3 <= VisibleRange.Max; num3 += VisibleRange.Interval)
				{
					float coordinateFromValue4 = GetCoordinateFromValue(num3);
					graphicsPath.AddLine(Location.X, coordinateFromValue4, Location.X - num * (float)TickSize.Width, coordinateFromValue4);
					graphicsPath.CloseFigure();
				}
			}
			if (m_smallTicksPerInterval > 0)
			{
				foreach (float smallTick2 in GetSmallTicks())
				{
					graphicsPath.AddLine(Location.X, smallTick2, Location.X + num * (float)SmallTickSize.Width, smallTick2);
					graphicsPath.CloseFigure();
				}
			}
		}
		Path3D path3D = DrawAxisText(g, Rect, z);
		if (path3D != null)
		{
			arrayList.Add(path3D);
		}
		for (int k = 0; k < GroupingLabelsImpl.Count; k++)
		{
			ChartAxisGroupingLabel groupingLabelAt = GroupingLabelsImpl.GetGroupingLabelAt(k);
			float num4 = num * groupingLabelsRowsDimensions[groupingLabelAt.Row];
			num4 = ((m_orientation != 0) ? (m_location.X - num4) : (m_location.Y + num4));
			groupingLabelAt.Draw(g, num4, this);
		}
		DrawTickLabels(g, arrayList, z, chartarea);
		arrayList.Add(Path3D.FromGraphicsPath(graphicsPath, z, m_lineType.Pen));
		Polygon[] paths = (Path3D[])arrayList.ToArray(typeof(Path3D));
		Path3DCollect polygon = new Path3DCollect(paths);
		g.AddPolygon(polygon);
		RectangleF rect = Rect;
		g.CreateRectangle(new Vector3D(rect.X, rect.Y, 0.0), rect.Size, null, null);
		pen.Dispose();
		pen2.Dispose();
	}

	internal RectangleF DockToRectangle(RectangleF rect)
	{
		if (m_orientation == ChartOrientation.Horizontal)
		{
			if (m_opposedPosition)
			{
				m_location = new PointF(rect.Left, rect.Top);
				rect.Y -= m_dimension;
				rect.Height += m_dimension;
			}
			else
			{
				m_location = new PointF(rect.Left, rect.Bottom);
				rect.Height += m_dimension;
			}
			m_length = rect.Width;
		}
		else
		{
			if (m_opposedPosition)
			{
				m_location = new PointF(rect.Right, rect.Bottom);
				rect.Width += m_dimension;
			}
			else
			{
				m_location = new PointF(rect.Left, rect.Bottom);
				rect.X -= m_dimension;
				rect.Width += m_dimension;
			}
			m_length = rect.Height;
		}
		m_scaleLablesCoef = (m_scaleLables ? (RealLength / m_scaleLength) : 1f);
		return rect;
	}

	public void SetRange(MinMaxInfo value)
	{
		if (value.interval <= 0.0)
		{
			value.interval = Range.interval;
		}
		if (!m_range.Equals(value))
		{
			m_range.SettingsChanged -= OnRangeChanged;
			m_range = value;
			m_range.SettingsChanged += OnRangeChanged;
			InvalidateRanges();
		}
	}

	public void SetNiceRange(DoubleRange baseRange, ChartAxisRangePaddingType pattingType, ChartArea chartArea)
	{
		SetNiceRange(baseRange, pattingType, chartArea, null);
	}

	internal void SetNiceRange(DoubleRange baseRange, ChartAxisRangePaddingType pattingType, ChartArea chartArea, ChartAxis axis)
	{
		if (IsIndexed)
		{
			if (pattingType == ChartAxisRangePaddingType.None)
			{
				SetRange(new MinMaxInfo(0.0, m_area.Chart.IndexValues.Count - 1, 1.0));
			}
			else
			{
				SetRange(new MinMaxInfo(-1.0, m_area.Chart.IndexValues.Count, 1.0));
			}
			return;
		}
		if (m_valueType == ChartValueType.Category)
		{
			double num = ((m_labelPlacement == ChartAxisLabelPlacement.BetweenTicks) ? (-0.5) : 0.0);
			SetRange(new MinMaxInfo(baseRange.Start + num, baseRange.End - num, Range.Interval));
			return;
		}
		if (m_valueType == ChartValueType.DateTime)
		{
			DateTime dateTime = DateTime.FromOADate(baseRange.Start);
			DateTime dateTime2 = DateTime.FromOADate(baseRange.End);
			if (dateTime == dateTime2)
			{
				dateTime = dateTime.AddDays(-0.5);
				dateTime2 = dateTime2.AddDays(0.5);
			}
			ChartDateTimeRange chartDateTimeRange = new ChartDateTimeNiceRangeMaker
			{
				DesiredIntervals = m_desiredIntervals,
				RangePaddingType = pattingType,
				DesiredIntervalType = IntervalType,
				PreferZero = PreferZero,
				ForceZero = ForceZero,
				IsMax = IsMax
			}.MakeNiceRange(dateTime, dateTime2);
			double num2;
			double num3;
			if (!ChangeDateTimeAxisValue)
			{
				num2 = DateTimeToDouble(chartDateTimeRange.Start);
				num3 = DateTimeToDouble(chartDateTimeRange.End);
			}
			else
			{
				num2 = DateTimeToDouble(dateTime);
				num3 = DateTimeToDouble(dateTime2);
			}
			int intervalCount = ChartDateTimeInterval.GetIntervalCount(chartDateTimeRange.DefaultInterval.Iterator());
			SetRange(new MinMaxInfo(num2, num3, Math.Ceiling((num3 - num2) / (double)intervalCount)));
			return;
		}
		if (baseRange.Delta == 0.0)
		{
			baseRange = DoubleRange.Inflate(baseRange, 0.5);
		}
		if (baseRange.Delta == 0.0)
		{
			baseRange = DoubleRange.Inflate(baseRange, baseRange.Start / 2.0);
		}
		if (m_valueType == ChartValueType.Logarithmic)
		{
			double num4 = Math.Log((baseRange.Start <= 0.0) ? double.Epsilon : baseRange.Start, m_logBase);
			double num5 = Math.Log((baseRange.End <= 0.0) ? double.Epsilon : baseRange.End, m_logBase);
			if (double.IsNaN(num4) || double.IsInfinity(num4))
			{
				num4 = 1.401298464324817E-45;
			}
			if (double.IsNaN(num5) || double.IsInfinity(num5))
			{
				num5 = 1.0;
			}
			baseRange = new DoubleRange(num4, num5);
		}
		if (m_forceZero)
		{
			baseRange += 0.0;
		}
		if (baseRange.Delta <= 120.0)
		{
			m_desiredIntervals = 6;
		}
		double num6 = baseRange.Delta / (double)m_desiredIntervals;
		num6 = ((m_valueType != ChartValueType.Logarithmic || !(Math.Abs(num6) < 1.0)) ? CalculateNiceInternal(num6) : ((double)Math.Sign(num6)));
		if (axis != null && axis.IsMajorSet)
		{
			num6 = axis.Range.Interval;
		}
		baseRange = CalculatePadding(baseRange, num6, pattingType);
		baseRange = TweakToZero(baseRange, num6);
		if (axis != null && axis.IsMaxSet)
		{
			baseRange.End = axis.Range.max;
			if (m_valueType == ChartValueType.Logarithmic)
			{
				double num7 = Math.Log((baseRange.End <= 0.0) ? double.Epsilon : baseRange.End, m_logBase);
				if (double.IsNaN(num7) || double.IsInfinity(num7))
				{
					num7 = 1.0;
				}
				baseRange.End = Math.Ceiling(num7);
			}
		}
		if (axis != null && axis.IsMinSet)
		{
			baseRange.Start = axis.Range.min;
			if (m_valueType == ChartValueType.Logarithmic)
			{
				double d = Math.Log((baseRange.Start <= 0.0) ? double.Epsilon : baseRange.Start, m_logBase);
				if (double.IsNaN(d) || double.IsInfinity(d))
				{
					d = 1.401298464324817E-45;
				}
				baseRange.Start = Math.Floor(d);
			}
		}
		bool num8 = m_valueType == ChartValueType.Double && Orientation == ChartOrientation.Horizontal && !chartArea.RequireInvertedAxes;
		double num9 = 0.0;
		if (num8)
		{
			num9 = ((baseRange.Start < 5.0 / 6.0 * baseRange.End) ? 0.0 : (baseRange.Start - (baseRange.End - baseRange.Start) / 2.0));
		}
		if (m_valueType == ChartValueType.Double && baseRange.Start > 0.0 && num9 == 0.0)
		{
			if (!IsMinSet)
			{
				baseRange = new DoubleRange(0.0, baseRange.End);
			}
			num6 = baseRange.Delta / (double)m_desiredIntervals;
			num6 = ((m_valueType != ChartValueType.Logarithmic || !(Math.Abs(num6) < 1.0)) ? CalculateNiceInternal(num6) : ((double)Math.Sign(num6)));
			if (axis != null && axis.IsMajorSet)
			{
				num6 = axis.Range.Interval;
			}
			baseRange = CalculatePadding(baseRange, num6, pattingType);
			baseRange = TweakToZero(baseRange, num6);
			if (axis != null && axis.IsMaxSet)
			{
				baseRange.End = axis.Range.max;
			}
			if (axis != null && axis.IsMinSet)
			{
				baseRange.Start = axis.Range.min;
			}
		}
		if (axis != null && axis.IsMajorSet)
		{
			num6 = axis.Range.Interval;
		}
		SetRange(new MinMaxInfo(baseRange.Start, baseRange.End, num6));
	}

	public void Update()
	{
		m_needUpdate = true;
	}

	public void Freeze()
	{
		m_isFreezed = true;
	}

	public void Melt()
	{
		m_isFreezed = false;
	}

	public float GetVisibleValue(double value)
	{
		return (float)((double)m_length * ValueToCoefficient(value));
	}

	public double GetRealValue(PointF p)
	{
		return GetRealValue((m_orientation == ChartOrientation.Horizontal) ? (p.X - m_location.X) : (m_location.Y - p.Y));
	}

	public double GetRealValue(float value)
	{
		return CoefficientToValue(value / m_length);
	}

	public float GetCoordinateFromValue(double value)
	{
		if (m_orientation == ChartOrientation.Horizontal)
		{
			return (float)((double)m_location.X + (double)m_length * ValueToCoefficient(value));
		}
		return (float)((double)m_location.Y - (double)m_length * ValueToCoefficient(value));
	}

	public void AddBreak(double from, double to)
	{
		m_breakRanges.Union(new DoubleRange(from, to));
	}

	public void ClearBreaks()
	{
		m_breakRanges.Clear();
	}

	public void CalculateAxisLocation(RectangleF rect)
	{
		m_needUpdateDimension = true;
		if (m_orientation == ChartOrientation.Horizontal)
		{
			if (LocationType != ChartAxisLocationType.Set)
			{
				float x = rect.Left;
				if (LocationType == ChartAxisLocationType.AntiLabelCut)
				{
					x = Location.X;
				}
				Location = (m_opposedPosition ? new PointF(x, rect.Bottom) : new PointF(x, rect.Top));
			}
			if (m_autoSize && rect.Right - Location.X > 0f)
			{
				m_length = rect.Right - Location.X;
			}
		}
		else
		{
			if (LocationType != ChartAxisLocationType.Set)
			{
				float y = rect.Bottom;
				if (LocationType == ChartAxisLocationType.AntiLabelCut)
				{
					y = Location.Y;
				}
				Location = (m_opposedPosition ? new PointF(rect.Left, y) : new PointF(rect.Right, y));
			}
			if (m_autoSize && Location.Y - rect.Top > 0f)
			{
				m_length = Location.Y - rect.Top;
			}
		}
		m_scaleLablesCoef = (m_scaleLables ? (RealLength / m_scaleLength) : 1f);
	}

	public RectangleF CalculateAxis(RectangleF rect, SizeF spacing, float dimension)
	{
		RectangleF result = rect;
		spacing = new SizeF(spacing.Width + dimension, spacing.Height + dimension);
		m_needUpdateDimension = true;
		if (m_orientation == ChartOrientation.Horizontal)
		{
			if (m_locationType != ChartAxisLocationType.Set)
			{
				float x = rect.Left;
				if (LocationType == ChartAxisLocationType.AntiLabelCut)
				{
					x = Location.X;
				}
				if (m_opposedPosition)
				{
					Location = new PointF(x, rect.Top);
					result = new RectangleF(rect.Left, rect.Top - spacing.Height, rect.Width, rect.Height + spacing.Height);
				}
				else
				{
					Location = new PointF(x, rect.Bottom);
					result = new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height + spacing.Height);
				}
			}
			if (m_autoSize && rect.Right - Location.X > 0f)
			{
				m_length = rect.Right - Location.X;
			}
		}
		else
		{
			if (m_locationType != ChartAxisLocationType.Set)
			{
				float y = rect.Bottom;
				if (LocationType == ChartAxisLocationType.AntiLabelCut)
				{
					y = Location.Y;
				}
				if (m_opposedPosition)
				{
					Location = new PointF(rect.Right, y);
					result = new RectangleF(rect.Left, rect.Top, rect.Width + spacing.Width, rect.Height);
				}
				else
				{
					Location = new PointF(rect.Left, y);
					result = new RectangleF(rect.Left - spacing.Width, rect.Top, rect.Width + spacing.Width, rect.Height);
				}
			}
			if (m_autoSize && Location.Y - rect.Top > 0f)
			{
				m_length = Location.Y - rect.Top;
			}
		}
		m_scaleLablesCoef = (m_scaleLables ? (RealLength / m_scaleLength) : 1f);
		return result;
	}

	internal void DrawInterlacedGrid(ChartGraph graph, RectangleF rect)
	{
		if (!m_interlacedGrid)
		{
			return;
		}
		DoubleRange doubleRange = new DoubleRange(VisibleRange.min, VisibleRange.max);
		if (m_gridDrawMode == ChartAxisGridDrawingMode.Default)
		{
			foreach (RectangleF item in (IEnumerable)GetInterlacedGridRects(rect))
			{
				graph.DrawRect(m_interlacedGridInterior, null, item);
			}
			return;
		}
		bool flag = true;
		for (int i = 0; i < GroupingLabelsImpl.Count; i++)
		{
			ChartAxisGroupingLabel groupingLabelAt = GroupingLabelsImpl.GetGroupingLabelAt(i);
			if (groupingLabelAt.Row != 0 || !doubleRange.IsIntersects(groupingLabelAt.Range))
			{
				continue;
			}
			if (flag)
			{
				float coordinateFromValue = GetCoordinateFromValue(groupingLabelAt.Range.Start);
				float coordinateFromValue2 = GetCoordinateFromValue(groupingLabelAt.Range.End);
				if (m_orientation == ChartOrientation.Horizontal)
				{
					coordinateFromValue = ChartMath.MinMax(coordinateFromValue, rect.Left, rect.Right);
					coordinateFromValue2 = ChartMath.MinMax(coordinateFromValue2, rect.Left, rect.Right);
					graph.DrawRect(m_interlacedGridInterior, null, Math.Min(coordinateFromValue, coordinateFromValue2), rect.Top, Math.Abs(coordinateFromValue2 - coordinateFromValue), rect.Height);
				}
				else
				{
					coordinateFromValue = ChartMath.MinMax(coordinateFromValue, rect.Top, rect.Bottom);
					coordinateFromValue2 = ChartMath.MinMax(coordinateFromValue2, rect.Top, rect.Bottom);
					graph.DrawRect(m_interlacedGridInterior, null, rect.Left, Math.Min(coordinateFromValue, coordinateFromValue2), rect.Width, Math.Abs(coordinateFromValue2 - coordinateFromValue));
				}
				flag = false;
			}
			else
			{
				flag = true;
			}
		}
	}

	internal void DrawGridLines(ChartGraph graph, RectangleF rect)
	{
		MinMaxInfo visibleRange = VisibleRange;
		_ = m_gridLineType.Pen;
		SmoothingMode smoothingMode = graph.SmoothingMode;
		graph.SmoothingMode = SmoothingMode.None;
		if (m_drawMinorGrid && m_smallTicksPerInterval > 0 && GridDrawMode == ChartAxisGridDrawingMode.Default && m_drawMinorGrid && m_smallTicksPerInterval > 0)
		{
			foreach (float item in (IEnumerable)GetSmallTicks())
			{
				if (m_orientation == ChartOrientation.Vertical)
				{
					graph.DrawLine(m_minorGridLineType.Pen, rect.Left, item, rect.Right, item);
				}
				else
				{
					graph.DrawLine(m_minorGridLineType.Pen, item, rect.Top, item, rect.Bottom);
				}
			}
		}
		if (m_drawGrid)
		{
			if (GridDrawMode == ChartAxisGridDrawingMode.Default)
			{
				if (m_labelPlacement == ChartAxisLabelPlacement.OnTicks)
				{
					for (int i = 0; i < m_visibleLables.Length; i++)
					{
						ChartAxisLabel chartAxisLabel = m_visibleLables[i];
						if (double.IsInfinity(chartAxisLabel.DoubleValue))
						{
							throw new ArgumentException("Invalid Value type.");
						}
						float num2 = ((ValueType == ChartValueType.Category) ? GetCoordinateFromValue(i) : GetCoordinateFromValue(chartAxisLabel.DoubleValue));
						if (m_orientation == ChartOrientation.Vertical)
						{
							graph.DrawLine(m_gridLineType.Pen, rect.Left, num2, rect.Right, num2);
						}
						else
						{
							graph.DrawLine(m_gridLineType.Pen, num2, rect.Top, num2, rect.Bottom);
						}
					}
				}
				else
				{
					for (double num3 = VisibleRange.Min; num3 <= VisibleRange.Max; num3 += VisibleRange.Interval)
					{
						float coordinateFromValue = GetCoordinateFromValue(num3);
						if (m_orientation == ChartOrientation.Vertical)
						{
							graph.DrawLine(m_gridLineType.Pen, rect.Left, coordinateFromValue, rect.Right, coordinateFromValue);
						}
						else
						{
							graph.DrawLine(m_gridLineType.Pen, coordinateFromValue, rect.Top, coordinateFromValue, rect.Bottom);
						}
					}
				}
				if (m_customOrigin && visibleRange.Contains(m_origin))
				{
					float coordinateFromValue2 = GetCoordinateFromValue(m_origin);
					if (m_orientation == ChartOrientation.Vertical)
					{
						graph.DrawLine(m_gridLineType.Pen, rect.Left, coordinateFromValue2, rect.Right, coordinateFromValue2);
					}
					else
					{
						graph.DrawLine(m_gridLineType.Pen, coordinateFromValue2, rect.Top, coordinateFromValue2, rect.Bottom);
					}
				}
			}
			else
			{
				for (int j = 0; j < GroupingLabelsImpl.Count; j++)
				{
					ChartAxisGroupingLabel groupingLabelAt = GroupingLabelsImpl.GetGroupingLabelAt(j);
					if (groupingLabelAt.Row == 0 && visibleRange.Contains(groupingLabelAt.Range.Start) && visibleRange.Contains(groupingLabelAt.Range.End))
					{
						float coordinateFromValue3 = GetCoordinateFromValue(groupingLabelAt.Range.Start);
						float coordinateFromValue4 = GetCoordinateFromValue(groupingLabelAt.Range.End);
						if (Orientation == ChartOrientation.Horizontal)
						{
							graph.DrawLine(GridLineType.Pen, coordinateFromValue3, rect.Top, coordinateFromValue3, rect.Bottom);
							graph.DrawLine(GridLineType.Pen, coordinateFromValue4, rect.Top, coordinateFromValue4, rect.Bottom);
						}
						else
						{
							graph.DrawLine(GridLineType.Pen, rect.Left, coordinateFromValue3, rect.Right, coordinateFromValue3);
							graph.DrawLine(GridLineType.Pen, rect.Left, coordinateFromValue4, rect.Right, coordinateFromValue4);
						}
					}
				}
			}
		}
		graph.SmoothingMode = smoothingMode;
	}

	internal Polygon DrawInterlacedGrid(Graphics3D g, RectangleF rect, float z)
	{
		if (m_interlacedGrid)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			DoubleRange doubleRange = new DoubleRange(VisibleRange.min, VisibleRange.max);
			if (m_gridDrawMode == ChartAxisGridDrawingMode.Default)
			{
				foreach (RectangleF item in (IEnumerable)GetInterlacedGridRects(rect))
				{
					graphicsPath.AddRectangle(item);
				}
			}
			else
			{
				bool flag = true;
				for (int i = 0; i < GroupingLabelsImpl.Count; i++)
				{
					ChartAxisGroupingLabel groupingLabelAt = GroupingLabelsImpl.GetGroupingLabelAt(i);
					if (groupingLabelAt.Row != 0 || !doubleRange.IsIntersects(groupingLabelAt.Range))
					{
						continue;
					}
					if (flag)
					{
						float coordinateFromValue = GetCoordinateFromValue(groupingLabelAt.Range.Start);
						float coordinateFromValue2 = GetCoordinateFromValue(groupingLabelAt.Range.End);
						if (m_orientation == ChartOrientation.Horizontal)
						{
							coordinateFromValue = ChartMath.MinMax(coordinateFromValue, rect.Left, rect.Right);
							coordinateFromValue2 = ChartMath.MinMax(coordinateFromValue2, rect.Left, rect.Right);
							graphicsPath.AddRectangle(new RectangleF(Math.Min(coordinateFromValue, coordinateFromValue2), rect.Top, Math.Abs(coordinateFromValue2 - coordinateFromValue), rect.Height));
						}
						else
						{
							coordinateFromValue = ChartMath.MinMax(coordinateFromValue, rect.Top, rect.Bottom);
							coordinateFromValue2 = ChartMath.MinMax(coordinateFromValue2, rect.Top, rect.Bottom);
							graphicsPath.AddRectangle(new RectangleF(rect.Left, Math.Min(coordinateFromValue, coordinateFromValue2), rect.Width, Math.Abs(coordinateFromValue2 - coordinateFromValue)));
						}
						graphicsPath.CloseFigure();
						flag = false;
					}
					else
					{
						flag = true;
					}
				}
			}
			return Path3D.FromGraphicsPath(graphicsPath, z, m_interlacedGridInterior, null);
		}
		return null;
	}

	internal Polygon DrawGridLines(Graphics3D g, RectangleF rect, float z)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		GraphicsPath graphicsPath2 = new GraphicsPath();
		MinMaxInfo visibleRange = VisibleRange;
		if (m_drawMinorGrid && m_smallTicksPerInterval > 0 && GridDrawMode == ChartAxisGridDrawingMode.Default && m_drawMinorGrid && m_smallTicksPerInterval > 0)
		{
			foreach (float item in (IEnumerable)GetSmallTicks())
			{
				if (m_orientation == ChartOrientation.Vertical)
				{
					graphicsPath2.AddLine(rect.Left, item, rect.Right, item);
					graphicsPath2.CloseFigure();
				}
				else
				{
					graphicsPath2.AddLine(item, rect.Top, item, rect.Bottom);
					graphicsPath2.CloseFigure();
				}
			}
		}
		if (m_gridDrawMode == ChartAxisGridDrawingMode.Default)
		{
			if (m_labelPlacement == ChartAxisLabelPlacement.OnTicks)
			{
				for (int i = 0; i < m_visibleLables.Length; i++)
				{
					ChartAxisLabel chartAxisLabel = m_visibleLables[i];
					if (!chartAxisLabel.IsAuto)
					{
						_ = chartAxisLabel.DoubleValue;
					}
					else
					{
						_ = chartAxisLabel.DoubleValue;
						_ = m_pointOffset;
					}
					float num2 = ((ValueType == ChartValueType.Category) ? GetCoordinateFromValue(i) : GetCoordinateFromValue(chartAxisLabel.DoubleValue));
					if (m_orientation == ChartOrientation.Vertical)
					{
						graphicsPath.AddLine(rect.Left, num2, rect.Right, num2);
						graphicsPath.CloseFigure();
					}
					else
					{
						graphicsPath.AddLine(num2, rect.Top, num2, rect.Bottom);
						graphicsPath.CloseFigure();
					}
				}
			}
			else
			{
				for (double num3 = VisibleRange.Min; num3 <= VisibleRange.Max; num3 += VisibleRange.Interval)
				{
					float coordinateFromValue = GetCoordinateFromValue(num3);
					if (m_orientation == ChartOrientation.Vertical)
					{
						graphicsPath.AddLine(rect.Left, coordinateFromValue, rect.Right, coordinateFromValue);
						graphicsPath.CloseFigure();
					}
					else
					{
						graphicsPath.AddLine(coordinateFromValue, rect.Top, coordinateFromValue, rect.Bottom);
						graphicsPath.CloseFigure();
					}
				}
			}
			if (m_customOrigin && visibleRange.Contains(m_origin))
			{
				float coordinateFromValue2 = GetCoordinateFromValue(m_origin);
				if (m_orientation == ChartOrientation.Vertical)
				{
					graphicsPath.AddLine(rect.Left, coordinateFromValue2, rect.Right, coordinateFromValue2);
					graphicsPath.CloseFigure();
				}
				else
				{
					graphicsPath.AddLine(coordinateFromValue2, rect.Top, coordinateFromValue2, rect.Bottom);
					graphicsPath.CloseFigure();
				}
			}
		}
		else
		{
			for (int j = 0; j < GroupingLabelsImpl.Count; j++)
			{
				ChartAxisGroupingLabel groupingLabelAt = GroupingLabelsImpl.GetGroupingLabelAt(j);
				if (groupingLabelAt.Row == 0 && visibleRange.Contains(groupingLabelAt.Range.Start) && visibleRange.Contains(groupingLabelAt.Range.End))
				{
					float coordinateFromValue3 = GetCoordinateFromValue(groupingLabelAt.Range.Start);
					float coordinateFromValue4 = GetCoordinateFromValue(groupingLabelAt.Range.End);
					if (Orientation == ChartOrientation.Horizontal)
					{
						graphicsPath.AddLine(coordinateFromValue3, rect.Top, coordinateFromValue3, rect.Bottom);
						graphicsPath.CloseFigure();
						graphicsPath.AddLine(coordinateFromValue4, rect.Top, coordinateFromValue4, rect.Bottom);
						graphicsPath.CloseFigure();
					}
					else
					{
						graphicsPath.AddLine(rect.Left, coordinateFromValue3, rect.Right, coordinateFromValue3);
						graphicsPath.CloseFigure();
						graphicsPath.AddLine(rect.Left, coordinateFromValue4, rect.Right, coordinateFromValue4);
						graphicsPath.CloseFigure();
					}
				}
			}
		}
		Path3DCollect path3DCollect = null;
		if (graphicsPath.PointCount > 0)
		{
			path3DCollect = new Path3DCollect(Path3D.FromGraphicsPath(graphicsPath, z, m_gridLineType.Pen));
		}
		if (graphicsPath2.PointCount > 0)
		{
			if (path3DCollect != null)
			{
				path3DCollect.Add(Path3D.FromGraphicsPath(graphicsPath2, z, m_minorGridLineType.Pen));
			}
			else
			{
				path3DCollect = new Path3DCollect(Path3D.FromGraphicsPath(graphicsPath2, z, m_minorGridLineType.Pen));
			}
		}
		if (path3DCollect != null)
		{
			return path3DCollect;
		}
		return Path3D.FromGraphicsPath(graphicsPath, z, m_gridLineType.Pen);
	}

	internal void DrawBreaks(Graphics g, RectangleF rect)
	{
		if (!BreaksEnabled)
		{
			return;
		}
		double num = 0.0;
		bool flag = m_orientation == ChartOrientation.Vertical;
		if (m_breakRanges.BreaksMode == ChartBreaksMode.Auto)
		{
			int i = 0;
			for (int num2 = m_breakRanges.Segments.Count - 1; i < num2; i++)
			{
				ChartAxisSegment chartAxisSegment = m_breakRanges.Segments[i] as ChartAxisSegment;
				num += chartAxisSegment.Length;
				float num3 = (float)(num * (double)m_length);
				PointF from = (flag ? new PointF(rect.Left, Location.Y - num3) : new PointF(Location.X + num3, rect.Top));
				PointF to = (flag ? new PointF(rect.Right, Location.Y - num3) : new PointF(Location.X + num3, rect.Bottom));
				m_breakInfo.DrawBreakLine(g, from, to);
			}
		}
		else
		{
			if (m_breakRanges.BreaksMode != ChartBreaksMode.Manual)
			{
				return;
			}
			foreach (DoubleRange @break in m_breakRanges.Breaks)
			{
				if (VisibleRange.Contains(@break.End))
				{
					float coordinateFromValue = GetCoordinateFromValue(@break.End);
					PointF from2 = (flag ? new PointF(rect.Left, coordinateFromValue) : new PointF(coordinateFromValue, rect.Top));
					PointF to2 = (flag ? new PointF(rect.Right, coordinateFromValue) : new PointF(coordinateFromValue, rect.Bottom));
					m_breakInfo.DrawBreakLine(g, from2, to2);
				}
			}
		}
	}

	internal void DrawBreaks(Graphics3D g, RectangleF rect)
	{
		if (!BreaksEnabled)
		{
			return;
		}
		double num = 0.0;
		bool flag = m_orientation == ChartOrientation.Vertical;
		if (m_breakRanges.BreaksMode == ChartBreaksMode.Auto)
		{
			int i = 0;
			for (int num2 = m_breakRanges.Segments.Count - 1; i < num2; i++)
			{
				ChartAxisSegment chartAxisSegment = m_breakRanges.Segments[i] as ChartAxisSegment;
				num += chartAxisSegment.Length;
				float num3 = (float)(num * (double)m_length);
				PointF from = (flag ? new PointF(rect.Left, rect.Bottom - num3) : new PointF(rect.Left + num3, rect.Top));
				PointF to = (flag ? new PointF(rect.Right, rect.Bottom - num3) : new PointF(rect.Left + num3, rect.Bottom));
				m_breakInfo.DrawBreakLine(g, from, to);
			}
		}
		else
		{
			if (m_breakRanges.BreaksMode != ChartBreaksMode.Manual)
			{
				return;
			}
			foreach (DoubleRange @break in m_breakRanges.Breaks)
			{
				if (VisibleRange.Contains(@break.End))
				{
					float coordinateFromValue = GetCoordinateFromValue(@break.End);
					PointF from2 = (flag ? new PointF(rect.Left, coordinateFromValue) : new PointF(coordinateFromValue, rect.Top));
					PointF to2 = (flag ? new PointF(rect.Right, coordinateFromValue) : new PointF(coordinateFromValue, rect.Bottom));
					m_breakInfo.DrawBreakLine(g, from2, to2);
				}
			}
		}
	}

	public double ValueToCoefficient(double value)
	{
		MinMaxInfo visibleRange = VisibleRange;
		DoubleRange doubleRange = new DoubleRange(visibleRange.min, visibleRange.max);
		if (doubleRange.Delta == 0.0)
		{
			value = 0.0;
		}
		else
		{
			if (m_valueType == ChartValueType.Logarithmic)
			{
				value = ((!(value <= 0.0)) ? Math.Log(value, m_logBase) : Math.Log(double.Epsilon, m_logBase));
			}
			value = ((!BreaksEnabled) ? doubleRange.Extrapolate(value) : m_breakRanges.ValueToCoefficient(value));
		}
		if (!m_inversed)
		{
			return value;
		}
		return 1.0 - value;
	}

	public double CoefficientToValue(double value)
	{
		MinMaxInfo visibleRange = VisibleRange;
		if (visibleRange.Delta == 0.0)
		{
			value = 0.0;
		}
		else
		{
			DoubleRange doubleRange = new DoubleRange(visibleRange.min, visibleRange.max);
			value = (m_inversed ? (1.0 - value) : value);
			value = ((!BreaksEnabled) ? doubleRange.Interpolate(value) : m_breakRanges.CoefficientToValue(value));
			if (m_valueType == ChartValueType.Logarithmic)
			{
				value = Math.Pow(m_logBase, value);
			}
		}
		return value;
	}

	internal void CalculateNiceRange(ref DoubleRange range, ref double interval, ChartAxisRangePaddingType paddingType)
	{
		interval = CalculateNiceInternal(interval);
		range = CalculatePadding(range, interval, paddingType);
	}

	private double CalculateNiceInternal(double desiredInterval)
	{
		double num = Math.Pow(10.0, Math.Floor(Math.Log10(desiredInterval)));
		double num2 = desiredInterval / num;
		double[] array = c_intervalDivs;
		foreach (double num3 in array)
		{
			if ((m_area.Chart as ChartControl).IsRadar ? (num2 < num3) : ((m_desiredIntervals == 6) ? (Math.Round(num2) <= num3) : (Math.Ceiling(num2) <= num3)))
			{
				return num3 * num;
			}
		}
		return desiredInterval;
	}

	private double CalculateNiceInternalEx(double desiredInterval)
	{
		double num = Math.Pow(10.0, Math.Floor(Math.Log10(desiredInterval)));
		double num2 = desiredInterval / num;
		double num3 = double.MaxValue;
		double[] array = c_intervalDivs;
		foreach (double num4 in array)
		{
			double num5 = Math.Abs(num4 - num2);
			if (num5 < num3)
			{
				num3 = num5;
				desiredInterval = num4 * num;
			}
		}
		return desiredInterval;
	}

	private DoubleRange CalculatePadding(DoubleRange baseRange, double interval, ChartAxisRangePaddingType paddingType)
	{
		bool flag = m_forceZero || m_preferZero;
		bool num = paddingType == ChartAxisRangePaddingType.Calculate;
		double num2 = (num ? ChartMath.Round(baseRange.Start, interval, up: false) : baseRange.Start);
		double num3 = (num ? ChartMath.Round(baseRange.End, interval, up: true) : baseRange.End);
		if (num)
		{
			if (m_margin)
			{
				if (baseRange.Start - num2 < interval / 2.0 && (!flag || num2 != 0.0))
				{
					num2 -= interval;
				}
				if (num3 - baseRange.End < interval / 2.0 && !(m_area.Chart as ChartControl).IsRadar && (!flag || num3 != 0.0))
				{
					num3 += interval;
				}
			}
			if (ForceZero && num2 != 0.0)
			{
				num2 = 0.0;
			}
			if (ForceZeroToDouble && num3 < 0.0)
			{
				num3 = 0.0;
			}
			if (ForceZeroToDouble && num3 > 0.0)
			{
				num2 = 0.0;
			}
		}
		return new DoubleRange(num2, num3);
	}

	private DoubleRange TweakToZero(DoubleRange range, double interval)
	{
		if (m_forceZero)
		{
			range = DoubleRange.Union(range, 0.0);
		}
		else if (m_preferZero)
		{
			DoubleRange doubleRange = new DoubleRange(0.0, interval / 2.0);
			DoubleRange doubleRange2 = new DoubleRange(0.0, (0.0 - interval) / 2.0);
			if (doubleRange.Inside(range.Start) || doubleRange2.Inside(range.End))
			{
				range = DoubleRange.Union(range, 0.0);
			}
		}
		return range;
	}

	protected virtual void OnAppearanceChanged(EventArgs e)
	{
		if (!m_isFreezed && this.AppearanceChanged != null)
		{
			this.AppearanceChanged(this, e);
		}
	}

	protected virtual void OnDimensionsChanged(EventArgs e)
	{
		m_needUpdateVisibleLables = true;
		if (m_area != null)
		{
			m_area.UpdateClientBounds = true;
		}
		if (!m_isFreezed && this.DimensionsChanged != null)
		{
			this.DimensionsChanged(this, e);
		}
	}

	protected virtual void OnIntervalsChanged(EventArgs e)
	{
		m_needUpdateVisibleLables = true;
		if (!m_isFreezed && this.IntervalsChanged != null)
		{
			this.IntervalsChanged(this, e);
		}
	}

	protected virtual void OnVisibleRangeChanged(EventArgs e)
	{
		m_needUpdateVisibleLables = true;
		if (!m_isFreezed && this.VisibleRangeChanged != null)
		{
			this.VisibleRangeChanged(this, e);
		}
	}

	internal float GetOffsetLabel(float sz)
	{
		float result = 0f;
		if (m_labelAligment == StringAlignment.Far)
		{
			result = sz / 2f;
		}
		else if (m_labelAligment == StringAlignment.Near)
		{
			result = (0f - sz) / 2f;
		}
		return result;
	}

	internal void SetDefaultOrientation(ChartOrientation value)
	{
		m_defaultAxisOrienation = value;
	}

	private float DoTickLabelsLayout(Graphics g, RectangleF labelsBounds, float spacing, ChartArea chartarea)
	{
		ChartAxisLabel[] visibleLabels = VisibleLabels;
		int num = ((AxisLabelPlacement != 0) ? ((!m_opposedPosition) ? 1 : (-1)) : (m_opposedPosition ? 1 : (-1)));
		float num2 = 0f;
		if (m_orientation == ChartOrientation.Horizontal)
		{
			float x = labelsBounds.X;
			float num3 = ((!string.IsNullOrEmpty(Title)) ? ((float)(2 * TickSize.Height) - m_titleSpacing / 2f) : ((float)(2 * TickSize.Height)));
			float y = ((AxisLabelPlacement != 0) ? (m_opposedPosition ? (labelsBounds.Bottom - spacing - num3) : (labelsBounds.Top + spacing + num3)) : (m_opposedPosition ? (labelsBounds.Top + spacing + num3) : (labelsBounds.Bottom - spacing - num3)));
			float width = labelsBounds.Width;
			ContentAlignment aligment = ContentAlignment.MiddleCenter;
			if (AxisLabelPlacement == ChartPlacement.Inside)
			{
				switch (m_labelAligment)
				{
				case StringAlignment.Center:
					aligment = (m_opposedPosition ? ContentAlignment.BottomCenter : ContentAlignment.TopCenter);
					break;
				case StringAlignment.Far:
					aligment = (m_opposedPosition ? ContentAlignment.BottomRight : ContentAlignment.TopRight);
					break;
				case StringAlignment.Near:
					aligment = ((!m_opposedPosition) ? ContentAlignment.TopLeft : ContentAlignment.BottomLeft);
					break;
				}
			}
			else
			{
				switch (m_labelAligment)
				{
				case StringAlignment.Center:
					aligment = (m_opposedPosition ? ContentAlignment.TopCenter : ContentAlignment.BottomCenter);
					break;
				case StringAlignment.Far:
					aligment = (m_opposedPosition ? ContentAlignment.TopRight : ContentAlignment.BottomRight);
					break;
				case StringAlignment.Near:
					aligment = (m_opposedPosition ? ContentAlignment.TopLeft : ContentAlignment.BottomLeft);
					break;
				}
			}
			switch (m_intersectAction)
			{
			case ChartLabelIntersectAction.None:
			{
				float num20 = 0f;
				float num21 = 0f;
				int num22 = 0;
				foreach (ChartAxisLabel chartAxisLabel7 in visibleLabels)
				{
					RectangleF empty = RectangleF.Empty;
					float x5 = x + (float)((double)width * ValueToCoefficient(chartAxisLabel7.DoubleValue));
					PointF marginsPoint3 = GetMarginsPoint(g, new PointF(x5, y), chartAxisLabel7.Measure(g, this));
					empty = ((!m_labelRotate) ? chartAxisLabel7.Arrange(marginsPoint3, aligment) : chartAxisLabel7.Arrange(marginsPoint3, aligment, m_labelRotateAngle, m_rotateFromTicks));
					if (num21 < chartAxisLabel7.Font.Size)
					{
						num21 = chartAxisLabel7.Font.Size;
					}
					if (num22 < chartAxisLabel7.Text.Length)
					{
						num22 = chartAxisLabel7.Text.Length;
					}
					num20 = Math.Max(num20, empty.Height);
				}
				chartarea.LargeValueAxisLabelFontSize = num21;
				chartarea.LargeValueAxisLabelLength = num22;
				chartarea.IsAxisVisible = IsVisible;
				num2 += num20;
				break;
			}
			case ChartLabelIntersectAction.Wrap:
			{
				float num12 = 0f;
				PointF[] array3 = new PointF[visibleLabels.Length];
				for (int l = 0; l < visibleLabels.Length; l++)
				{
					ChartAxisLabel chartAxisLabel4 = visibleLabels[l];
					float x3 = x + (float)((double)width * ValueToCoefficient(chartAxisLabel4.DoubleValue));
					array3[l] = new PointF(x3, y);
					chartAxisLabel4.Measure(g, this);
				}
				int m = 0;
				for (int num13 = visibleLabels.Length; m < num13; m++)
				{
					float num14 = float.MaxValue;
					ChartAxisLabel chartAxisLabel5 = visibleLabels[m];
					if (m > 0)
					{
						num14 = Math.Min(num14, array3[m].X - array3[m - 1].X);
					}
					if (m < num13 - 1)
					{
						num14 = Math.Min(num14, array3[m + 1].X - array3[m].X);
					}
					chartAxisLabel5.Measure(g, num14, this);
					num12 = Math.Max(num12, chartAxisLabel5.Arrange(array3[m], aligment).Height);
				}
				num2 += num12;
				break;
			}
			case ChartLabelIntersectAction.MultipleRows:
			{
				int[] array4 = new int[visibleLabels.Length];
				float[] array5 = new float[4]
				{
					float.MinValue,
					float.MinValue,
					float.MinValue,
					float.MinValue
				};
				float[] array6 = new float[5];
				for (int n = 0; n < visibleLabels.Length; n++)
				{
					int num15 = 0;
					float num16 = float.MaxValue;
					ChartAxisLabel chartAxisLabel6 = visibleLabels[n];
					float x4 = x + (float)((double)width * ValueToCoefficient(chartAxisLabel6.DoubleValue));
					PointF marginsPoint2 = GetMarginsPoint(g, new PointF(x4, y), chartAxisLabel6.Measure(g, this));
					RectangleF rectangleF2 = chartAxisLabel6.Arrange(marginsPoint2, aligment);
					for (int num17 = 0; num17 < 4; num17++)
					{
						if (array5[num17] < rectangleF2.Left)
						{
							num15 = num17;
							break;
						}
						if (array5[num17] - rectangleF2.Left < num16)
						{
							num16 = array5[num15] - rectangleF2.Left;
							num15 = num17;
						}
					}
					array5[num15] = rectangleF2.Right;
					array6[num15 + 1] = Math.Max(array6[num15 + 1], rectangleF2.Height);
					array4[n] = num15;
				}
				for (int num18 = 1; num18 < array6.Length; num18++)
				{
					array6[num18] += array6[num18 - 1];
				}
				for (int num19 = 0; num19 < visibleLabels.Length; num19++)
				{
					ChartAxisLabel obj = visibleLabels[num19];
					PointF location = obj.Bounds.Location;
					location.Y += (float)num * array6[array4[num19]];
					obj.Arrange(location);
				}
				num2 += array6[4];
				break;
			}
			case ChartLabelIntersectAction.Rotate:
			{
				float num4 = 0f;
				float num5 = float.MaxValue;
				float val = 0f;
				float num6 = 0f;
				PointF[] array = new PointF[visibleLabels.Length];
				ChartAxisLabel[] array2 = new ChartAxisLabel[visibleLabels.Length];
				int num7 = 0;
				if (m_inversed)
				{
					for (int num8 = visibleLabels.Length - 1; num8 >= 0; num8--)
					{
						array2[num7] = visibleLabels[num8];
						num7++;
					}
				}
				else
				{
					array2 = visibleLabels;
				}
				for (int i = 0; i < visibleLabels.Length; i++)
				{
					ChartAxisLabel chartAxisLabel = array2[i];
					float x2 = x + (float)((double)width * ValueToCoefficient(chartAxisLabel.DoubleValue));
					PointF marginsPoint = GetMarginsPoint(g, new PointF(x2, y), chartAxisLabel.Measure(g, this));
					RectangleF rectangleF = chartAxisLabel.Arrange(marginsPoint, aligment);
					array[i] = new PointF(x2, y);
					if (i != 0 && ((rectangleF.Left > 0f) ? (rectangleF.Left - num6 < 0f) : (0f - rectangleF.Left - num6 < 0f)))
					{
						num5 = Math.Min(num5, rectangleF.Right - num6);
					}
					num6 = rectangleF.Right;
					num4 = Math.Max(num4, rectangleF.Height);
					val = Math.Max(val, rectangleF.Width);
				}
				if (num5 != float.MaxValue)
				{
					float num9 = (float)(180.0 / Math.PI * Math.Atan(num4 / num5));
					num9 = (float)num * ((num9 < 0f) ? 90f : num9);
					if (num9 != 90f && num9 != -90f)
					{
						num9 = 0f - num9;
					}
					if (num9 != 0f)
					{
						foreach (ChartAxisLabel chartAxisLabel2 in visibleLabels)
						{
							PointF connectPoint = new PointF(x + GetVisibleValue(chartAxisLabel2.DoubleValue), y);
							num4 = Math.Max(num4, chartAxisLabel2.Arrange(connectPoint, aligment, num9, m_rotateFromTicks).Height);
						}
						if (num4 > (float)(chartarea.Height / 2))
						{
							float width2 = (num4 = chartarea.Height / 2);
							float num10 = (float)(180.0 / Math.PI * Math.Atan(num4 / num5));
							num10 = (float)num * ((num9 < 0f) ? 90f : num9);
							if (num10 != 90f && num10 != -90f)
							{
								num10 = 0f - num10;
							}
							if (num10 != 0f)
							{
								LabelAlignment = StringAlignment.Near;
								int k = 0;
								for (int num11 = visibleLabels.Length; k < num11; k++)
								{
									ChartAxisLabel chartAxisLabel3 = visibleLabels[k];
									chartAxisLabel3.Measure(g, width2, this);
									num4 = Math.Max(num4, chartAxisLabel3.Arrange(array[k], aligment, num10, m_rotateFromTicks).Height);
								}
							}
						}
					}
				}
				num2 += num4;
				break;
			}
			}
		}
		else
		{
			float num24 = ((!string.IsNullOrEmpty(Title)) ? ((float)(3 * TickSize.Width) - m_titleSpacing / 2f) : ((float)(3 * TickSize.Width)));
			float x6 = ((AxisLabelPlacement != 0) ? (m_opposedPosition ? (labelsBounds.Left + spacing + num24) : (labelsBounds.Right - spacing - num24)) : (m_opposedPosition ? (labelsBounds.Right - spacing - num24) : (labelsBounds.Left + spacing + num24)));
			float bottom = labelsBounds.Bottom;
			float height = labelsBounds.Height;
			ContentAlignment aligment2 = ContentAlignment.MiddleCenter;
			if (AxisLabelPlacement == ChartPlacement.Inside)
			{
				switch (m_labelAligment)
				{
				case StringAlignment.Center:
					aligment2 = (m_opposedPosition ? ContentAlignment.MiddleLeft : ContentAlignment.MiddleRight);
					break;
				case StringAlignment.Far:
					aligment2 = (m_opposedPosition ? ContentAlignment.BottomLeft : ContentAlignment.BottomRight);
					break;
				case StringAlignment.Near:
					aligment2 = (m_opposedPosition ? ContentAlignment.TopLeft : ContentAlignment.TopRight);
					break;
				}
			}
			else
			{
				switch (m_labelAligment)
				{
				case StringAlignment.Center:
					aligment2 = (m_opposedPosition ? ContentAlignment.MiddleRight : ContentAlignment.MiddleLeft);
					break;
				case StringAlignment.Far:
					aligment2 = (m_opposedPosition ? ContentAlignment.BottomRight : ContentAlignment.BottomLeft);
					break;
				case StringAlignment.Near:
					aligment2 = ((!m_opposedPosition) ? ContentAlignment.TopLeft : ContentAlignment.TopRight);
					break;
				}
			}
			float num25 = 0f;
			foreach (ChartAxisLabel chartAxisLabel8 in visibleLabels)
			{
				RectangleF empty2 = RectangleF.Empty;
				float y2 = bottom - (float)((double)height * ValueToCoefficient(chartAxisLabel8.DoubleValue));
				PointF marginsPoint4 = GetMarginsPoint(g, new PointF(x6, y2), chartAxisLabel8.Measure(g, this));
				num25 = Math.Max(num25, ((!m_labelRotate) ? chartAxisLabel8.Arrange(marginsPoint4, aligment2) : chartAxisLabel8.Arrange(marginsPoint4, aligment2, m_labelRotateAngle, m_rotateFromTicks)).Width);
			}
			if (num25 > (float)(chartarea.Width / 2))
			{
				float width3 = chartarea.Width / 2;
				foreach (ChartAxisLabel chartAxisLabel9 in visibleLabels)
				{
					_ = RectangleF.Empty;
					float y3 = bottom - (float)((double)height * ValueToCoefficient(chartAxisLabel9.DoubleValue));
					PointF marginsPoint5 = GetMarginsPoint(g, new PointF(x6, y3), chartAxisLabel9.Measure(g, width3, this));
					if (m_labelRotate)
					{
						chartAxisLabel9.Arrange(marginsPoint5, aligment2, m_labelRotateAngle, m_rotateFromTicks);
					}
					else
					{
						chartAxisLabel9.Arrange(marginsPoint5, aligment2);
					}
					chartAxisLabel9.Measure(g, width3, this);
				}
				num25 = chartarea.Width / 2;
			}
			num2 += num25;
		}
		if (m_drawTickLabelGrid)
		{
			num2 += m_tickLabelGridPadding;
		}
		return num2;
	}

	private float DoGroupingLabelsLayout(Graphics g, float spacing)
	{
		float num = spacing;
		if (m_orientation == ChartOrientation.Horizontal)
		{
			for (int i = 0; i < GroupingLabelsImpl.Count; i++)
			{
				GroupingLabelsImpl.GetGroupingLabelAt(i).GridDimension = 0f;
			}
			for (int j = 0; j < groupingLabelsRowsDimensions.Length; j++)
			{
				float num2 = 0f;
				for (int k = 0; k < GroupingLabelsImpl.Count; k++)
				{
					if (j == GroupingLabelsImpl.GetGroupingLabelAt(k).Row)
					{
						float height = GroupingLabelsImpl.GetGroupingLabelAt(k).GetSize(g, this).Height;
						if (num2 < height)
						{
							num2 = height;
						}
					}
				}
				for (int l = 0; l < GroupingLabelsImpl.Count; l++)
				{
					if (j == GroupingLabelsImpl.GetGroupingLabelAt(l).Row)
					{
						GroupingLabelsImpl.GetGroupingLabelAt(l).GridDimension = num2;
					}
				}
				num += num2;
				groupingLabelsRowsDimensions[j] = num;
			}
		}
		else
		{
			for (int m = 0; m < GroupingLabelsImpl.Count; m++)
			{
				GroupingLabelsImpl.GetGroupingLabelAt(m).GridDimension = 0f;
			}
			for (int n = 0; n < groupingLabelsRowsDimensions.Length; n++)
			{
				float num3 = 0f;
				for (int num4 = 0; num4 < GroupingLabelsImpl.Count; num4++)
				{
					if (n == GroupingLabelsImpl.GetGroupingLabelAt(num4).Row)
					{
						float width = GroupingLabelsImpl.GetGroupingLabelAt(num4).GetSize(g, this).Width;
						if (num3 < width)
						{
							num3 = width;
						}
					}
				}
				for (int num5 = 0; num5 < GroupingLabelsImpl.Count; num5++)
				{
					if (n == GroupingLabelsImpl.GetGroupingLabelAt(num5).Row)
					{
						GroupingLabelsImpl.GetGroupingLabelAt(num5).GridDimension = num3;
					}
				}
				num += num3;
				groupingLabelsRowsDimensions[n] = num;
			}
		}
		return num - spacing;
	}

	internal void DrawTickLabels(Graphics g, ChartArea chartArea, RectangleF labelBounds)
	{
		RectangleF rect = Rect;
		for (int i = 0; i < m_visibleLables.Length; i++)
		{
			ChartAxisLabel chartAxisLabel = m_visibleLables[i];
			if (m_hidePartialLabels)
			{
				if (m_orientation == ChartOrientation.Horizontal)
				{
					if (chartAxisLabel.Bounds.Left < rect.Left || chartAxisLabel.Bounds.Right > rect.Right)
					{
						chartAxisLabel = null;
					}
				}
				else if (chartAxisLabel.Bounds.Top < rect.Top || chartAxisLabel.Bounds.Bottom > rect.Bottom)
				{
					chartAxisLabel = null;
				}
			}
			chartAxisLabel?.Draw(g, this, chartArea, labelBounds);
		}
	}

	private void DrawTickLabels(Graphics3D g, IList paths, float z, ChartArea chartArea)
	{
		RectangleF rect = Rect;
		for (int i = 0; i < m_visibleLables.Length; i++)
		{
			ChartAxisLabel chartAxisLabel = m_visibleLables[i];
			if (m_hidePartialLabels)
			{
				if (m_orientation == ChartOrientation.Horizontal)
				{
					if (chartAxisLabel.Bounds.Left < rect.Left || chartAxisLabel.Bounds.Right > rect.Right)
					{
						chartAxisLabel = null;
					}
				}
				else if (chartAxisLabel.Bounds.Top < rect.Top || chartAxisLabel.Bounds.Bottom > rect.Bottom)
				{
					chartAxisLabel = null;
				}
			}
			if (chartAxisLabel != null)
			{
				Path3D path3D = chartAxisLabel.Draw3D(g, this, z, chartArea);
				if (path3D != null)
				{
					paths.Add(path3D);
				}
			}
		}
	}

	private void DrawTickLabelGrids(Graphics g, Pen pen, RectangleF rect)
	{
		RectangleF clip = rect;
		clip.Inflate(1f, 1f);
		if (m_orientation == ChartOrientation.Horizontal)
		{
			float ticksAndLabelsDimension = m_ticksAndLabelsDimension;
			float y = (m_opposedPosition ? (rect.Bottom - ticksAndLabelsDimension) : rect.Top);
			int i = 0;
			for (int num = m_visibleLables.Length; i < num; i++)
			{
				double num2 = ((i > 0) ? m_visibleLables[i - 1].DoubleValue : double.NaN);
				double num3 = ((i < num - 1) ? m_visibleLables[i + 1].DoubleValue : double.NaN);
				double doubleValue = m_visibleLables[i].DoubleValue;
				if (double.IsNaN(num2) && double.IsNaN(num3))
				{
					num2 = VisibleRange.Min;
					num3 = VisibleRange.Max;
				}
				else if (double.IsNaN(num2))
				{
					num2 = 2.0 * doubleValue - num3;
				}
				else if (double.IsNaN(num3))
				{
					num3 = 2.0 * doubleValue - num2;
				}
				if (VisibleRange.Intersects(new MinMaxInfo(num2, num3, 1.0)))
				{
					float coordinateFromValue = GetCoordinateFromValue((num2 + doubleValue) / 2.0);
					float coordinateFromValue2 = GetCoordinateFromValue((num3 + doubleValue) / 2.0);
					RectangleF rect2 = new RectangleF(Math.Min(coordinateFromValue, coordinateFromValue2), y, Math.Abs(coordinateFromValue - coordinateFromValue2), ticksAndLabelsDimension);
					GraphicsContainer cont = DrawingHelper.BeginTransform(g);
					g.SetClip(clip);
					DrawingHelper.DrawRectangleF(g, pen, rect2);
					DrawingHelper.EndTransform(g, cont);
				}
			}
			return;
		}
		float ticksAndLabelsDimension2 = m_ticksAndLabelsDimension;
		float x = (m_opposedPosition ? rect.Left : (rect.Right - ticksAndLabelsDimension2));
		int j = 0;
		for (int num4 = m_visibleLables.Length; j < num4; j++)
		{
			double num5 = ((j > 0) ? m_visibleLables[j - 1].DoubleValue : double.NaN);
			double num6 = ((j < num4 - 1) ? m_visibleLables[j + 1].DoubleValue : double.NaN);
			double doubleValue2 = m_visibleLables[j].DoubleValue;
			if (double.IsNaN(num5) && double.IsNaN(num6))
			{
				num5 = VisibleRange.Min;
				num6 = VisibleRange.Max;
			}
			else if (double.IsNaN(num5))
			{
				num5 = 2.0 * doubleValue2 - num6;
			}
			else if (double.IsNaN(num6))
			{
				num6 = 2.0 * doubleValue2 - num5;
			}
			if (VisibleRange.Intersects(new MinMaxInfo(num5, num6, 1.0)))
			{
				float coordinateFromValue3 = GetCoordinateFromValue((num5 + doubleValue2) / 2.0);
				float coordinateFromValue4 = GetCoordinateFromValue((num6 + doubleValue2) / 2.0);
				RectangleF rect3 = new RectangleF(x, Math.Min(coordinateFromValue3, coordinateFromValue4), ticksAndLabelsDimension2, Math.Abs(coordinateFromValue3 - coordinateFromValue4));
				GraphicsContainer cont2 = DrawingHelper.BeginTransform(g);
				g.SetClip(clip);
				DrawingHelper.DrawRectangleF(g, pen, rect3);
				DrawingHelper.EndTransform(g, cont2);
			}
		}
	}

	private void DrawAxisText(Graphics g, RectangleF rect)
	{
		string title = Title;
		RectangleF rectangleF = rect;
		string[] array = title.Split('\n');
		if (m_orientation == ChartOrientation.Horizontal && !m_opposedPosition)
		{
			for (int i = 1; i < array.Length; i++)
			{
				SizeF sizeF = g.MeasureString(array[1], TitleFont, (int)m_length);
				rect = new RectangleF(rect.X, rect.Y - sizeF.Height, rect.Width, rect.Height);
			}
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (m_orientation == ChartOrientation.Horizontal)
			{
				rectangleF = MeasureAxisText(g, rectangleF, ref array[j]);
				switch (m_titleAlignment)
				{
				case StringAlignment.Center:
					rectangleF.X += (rect.Width - rectangleF.Width) / 2f;
					break;
				case StringAlignment.Far:
					rectangleF.X += rect.Width - rectangleF.Width;
					break;
				}
				if (AxisLabelPlacement == ChartPlacement.Inside)
				{
					if (m_opposedPosition)
					{
						rectangleF.Y = rect.Top - rectangleF.Height;
					}
					else
					{
						rectangleF.Y = rect.Bottom;
					}
				}
				else if (!m_opposedPosition)
				{
					rectangleF.Y = rect.Bottom - rectangleF.Height;
				}
				else if (j > 0)
				{
					rectangleF.Y = rect.Top;
				}
				BrushPaint.FillRectangle(g, new RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width + 1f, rectangleF.Height + 1f), ChartAxisTitle.BackInterior);
				if (ChartAxisTitle.Border != null)
				{
					g.DrawRectangle(ChartAxisTitle.Border.Pen, rectangleF.X, rectangleF.Y, rectangleF.Width + 1f, rectangleF.Height + 1f);
				}
				using SolidBrush brush = new SolidBrush(TitleColor);
				g.DrawString(array[j], TitleFont, brush, rectangleF, DrawingHelper.NoClipFormat);
			}
			else
			{
				rectangleF.Y = 0f;
				GraphicsContainer cont = DrawingHelper.BeginTransform(g);
				if (AxisLabelPlacement == ChartPlacement.Inside)
				{
					if (m_opposedPosition)
					{
						g.TranslateTransform(rect.Right + rectangleF.Height, rect.Top);
						g.RotateTransform(90f);
					}
					else
					{
						g.TranslateTransform(rect.Left - rectangleF.Height, rect.Bottom);
						g.RotateTransform(-90f);
					}
				}
				else if (m_opposedPosition)
				{
					g.TranslateTransform(rect.Right, rect.Top);
					g.RotateTransform(90f);
				}
				else
				{
					g.TranslateTransform(rect.Left, rect.Bottom);
					g.RotateTransform(-90f);
				}
				rectangleF = MeasureAxisText(g, rectangleF, ref array[j]);
				switch (m_titleAlignment)
				{
				case StringAlignment.Near:
					rectangleF.X = 0f;
					break;
				case StringAlignment.Center:
					rectangleF.X = (rect.Height - rectangleF.Width) / 2f;
					break;
				case StringAlignment.Far:
					rectangleF.X = rect.Height - rectangleF.Width;
					break;
				}
				BrushPaint.FillRectangle(g, new RectangleF(rectangleF.X, rectangleF.Y, rectangleF.Width + 1f, rectangleF.Height + 1f), ChartAxisTitle.BackInterior);
				if (ChartAxisTitle.Border != null)
				{
					g.DrawRectangle(ChartAxisTitle.Border.Pen, rectangleF.X, rectangleF.Y, rectangleF.Width + 1f, rectangleF.Height + 1f);
				}
				using (SolidBrush brush2 = new SolidBrush(TitleColor))
				{
					g.TextRenderingHint = TextRenderingHint.AntiAlias;
					g.DrawString(array[j], TitleFont, brush2, rectangleF, DrawingHelper.NoClipFormat);
				}
				DrawingHelper.EndTransform(g, cont);
			}
			if (array.Length > 1)
			{
				rectangleF = new RectangleF(rect.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
				if (Orientation == ChartOrientation.Horizontal)
				{
					rect = new RectangleF(rect.X, rect.Y + rectangleF.Height + 1f, rect.Width, rect.Height);
				}
				else if (Orientation == ChartOrientation.Vertical)
				{
					rect = ((!m_opposedPosition) ? new RectangleF(rect.X + rectangleF.Height + 1f, rect.Y, rect.Width, rect.Height) : new RectangleF(rect.X - rectangleF.Height, rect.Y, rect.Width, rect.Height));
				}
			}
		}
	}

	private RectangleF MeasureAxisText(Graphics g, RectangleF textRect, ref string title)
	{
		if (m_titleDrawMode == ChartTitleDrawMode.Wrap)
		{
			textRect.Size = g.MeasureString(title, TitleFont, (int)m_length);
		}
		else
		{
			if (m_titleDrawMode == ChartTitleDrawMode.Ellipsis)
			{
				title = DrawingHelper.EllipsesText(g, title, TitleFont, m_length);
			}
			textRect.Size = g.MeasureString(title, TitleFont);
		}
		return textRect;
	}

	private Path3D DrawAxisText(Graphics3D g, RectangleF rect, float z)
	{
		Font titleFont = TitleFont;
		GraphicsPath graphicsPath = new GraphicsPath();
		if (m_title != "")
		{
			string text = Title;
			RectangleF rect2 = rect;
			if (m_titleDrawMode == ChartTitleDrawMode.Wrap)
			{
				rect2.Size = g.Graphics.MeasureString(text, TitleFont, (int)m_length);
			}
			else
			{
				if (m_titleDrawMode == ChartTitleDrawMode.Ellipsis)
				{
					text = DrawingHelper.EllipsesText(g.Graphics, text, TitleFont, m_length);
				}
				rect2.Size = g.Graphics.MeasureString(text, TitleFont);
			}
			if (Orientation == ChartOrientation.Horizontal)
			{
				switch (m_titleAlignment)
				{
				case StringAlignment.Center:
					rect2.X += (rect.Width - rect2.Width) / 2f;
					break;
				case StringAlignment.Far:
					rect2.X += rect.Width - rect2.Width;
					break;
				}
				if (AxisLabelPlacement == ChartPlacement.Inside)
				{
					if (m_opposedPosition)
					{
						rect2.Y = rect.Top - rect2.Height;
					}
					else
					{
						rect2.Y = rect.Bottom;
					}
				}
				else if (!m_opposedPosition)
				{
					rect2.Y = rect.Bottom - rect2.Height;
				}
				RenderingHelper.AddTextPath(graphicsPath, g.Graphics, text, titleFont, rect2);
			}
			else
			{
				rect2.Y = 0f;
				switch (m_titleAlignment)
				{
				case StringAlignment.Near:
					rect2.X = 0f;
					break;
				case StringAlignment.Center:
					rect2.X = (rect.Height - rect2.Width) / 2f;
					break;
				case StringAlignment.Far:
					rect2.X = rect.Height - rect2.Width;
					break;
				}
				Matrix matrix = new Matrix();
				if (AxisLabelPlacement == ChartPlacement.Inside)
				{
					if (m_opposedPosition)
					{
						matrix.Translate(rect.Right + rect2.Height, rect.Top);
						matrix.Rotate(90);
					}
					else
					{
						matrix.Translate(rect.Left - rect2.Height, rect.Bottom);
						matrix.Rotate(-90);
					}
				}
				else if (m_opposedPosition)
				{
					matrix.Translate(rect.Right, rect.Top);
					matrix.Rotate(90);
				}
				else
				{
					matrix.Translate(rect.Left, rect.Bottom);
					matrix.Rotate(-90);
				}
				RenderingHelper.AddTextPath(graphicsPath, g.Graphics, text, titleFont, rect2);
				graphicsPath.Transform(matrix);
			}
		}
		if (graphicsPath.PointCount <= 0)
		{
			return null;
		}
		return Path3D.FromGraphicsPath(graphicsPath, z, new SolidBrush(TitleColor));
	}

	private void OnNeedRedraw(object sender, EventArgs args)
	{
		OnAppearanceChanged(args);
	}

	private void OnNeedResize(object sender, EventArgs args)
	{
		m_needUpdate = true;
		OnDimensionsChanged(args);
	}

	private void OnRangeChanged(object sender, EventArgs args)
	{
		if (m_range.Interval == 0.0)
		{
			throw new ArgumentException("Interval can't zero");
		}
		m_rangeType = ChartAxisRangeType.Set;
		OnNeedResize(sender, args);
	}

	private void OnBreakRangesChanged(object sender, EventArgs args)
	{
		OnIntervalsChanged(args);
	}

	protected void RaiseFormatLabel(object sender, ChartFormatAxisLabelEventArgs args)
	{
		if (this.FormatLabel != null)
		{
			this.FormatLabel(sender, args);
		}
	}

	private PointF GetMarginsPoint(Graphics g, PointF pt, SizeF sz)
	{
		float num = pt.X;
		float num2 = pt.Y;
		if (edgeLabelsDrawingMode == ChartAxisEdgeLabelsDrawingMode.Shift)
		{
			if (Orientation == ChartOrientation.Horizontal)
			{
				float num3 = GetOffsetLabel(sz.Width) - sz.Width / 2f;
				if (adjustPlotAreaMargins == ChartSetMode.AutoSet)
				{
					num = Math.Max(num, Location.X - num3);
					num = Math.Min(num, Location.X + RealLength - sz.Width - num3);
				}
				else if (adjustPlotAreaMargins == ChartSetMode.UserSet)
				{
					num = Math.Max(num, Location.X + (float)chartPlotAreaMargins.Left - num3);
					num = Math.Min(num, Location.X + RealLength - (float)chartPlotAreaMargins.Right - sz.Width - num3);
				}
			}
			else
			{
				float num4 = GetOffsetLabel(sz.Height) - sz.Height / 2f;
				if (adjustPlotAreaMargins == ChartSetMode.AutoSet)
				{
					num2 = Math.Max(num2, Location.Y - RealLength - num4);
					num2 = Math.Min(num2, Location.Y - sz.Height - num4);
				}
				else if (adjustPlotAreaMargins == ChartSetMode.UserSet)
				{
					num2 = Math.Max(num2, Location.Y - RealLength + (float)chartPlotAreaMargins.Top - num4);
					num2 = Math.Min(num2, Location.Y - (float)chartPlotAreaMargins.Bottom - sz.Height - num4);
				}
			}
		}
		else if (edgeLabelsDrawingMode == ChartAxisEdgeLabelsDrawingMode.ClippingProtection)
		{
			RectangleF rectangleF = m_area.ClientRectangle;
			if (Orientation == ChartOrientation.Horizontal)
			{
				float num5 = GetOffsetLabel(sz.Width) - sz.Width / 2f;
				num = Math.Max(num, rectangleF.Left - num5);
				num = Math.Min(num, rectangleF.Right - sz.Width - num5);
			}
			else
			{
				float num6 = GetOffsetLabel(sz.Height) - sz.Height / 2f;
				num2 = Math.Max(num2, rectangleF.Top - num6);
				num2 = Math.Min(num2, rectangleF.Bottom - sz.Height - num6);
			}
		}
		return new PointF(num, num2);
	}

	private double ConvertToValueType(double value)
	{
		if (m_valueType == ChartValueType.Logarithmic)
		{
			return Math.Pow(m_logBase, value);
		}
		return value;
	}

	private double SetSmartZoomFactor(double zoomFact)
	{
		MinMaxInfo range = Range;
		DateTime dateTime = new DateTime(2005, 8, 17, 0, 0, 0);
		double delta = range.Delta;
		double num = 0.5;
		_ = 1.5;
		double num2 = (dateTime.AddYears(1).ToOADate() - dateTime.ToOADate()) / delta;
		double num3 = (dateTime.AddMonths(1).ToOADate() - dateTime.ToOADate()) / delta;
		double num4 = (dateTime.AddDays(7.0).ToOADate() - dateTime.ToOADate()) / delta;
		double num5 = (dateTime.AddDays(1.0).ToOADate() - dateTime.ToOADate()) / delta;
		double num6 = (dateTime.AddHours(1.0).ToOADate() - dateTime.ToOADate()) / delta;
		double num7 = (dateTime.AddHours(1.0).ToOADate() - dateTime.ToOADate()) / delta;
		double num8 = (dateTime.AddSeconds(1.0).ToOADate() - dateTime.ToOADate()) / delta;
		if (zoomFact < 2.0 * num7)
		{
			currentSmartDateTimeFormat = SmartDateZoomMinuteLevelLabelFormat;
			return num7;
		}
		if (zoomFact < 2.0 * num6)
		{
			currentSmartDateTimeFormat = SmartDateZoomHourLevelLabelFormat;
			return num6;
		}
		if (zoomFact < 2.0 * num5)
		{
			currentSmartDateTimeFormat = SmartDateZoomDayLevelLabelFormat;
			return num5;
		}
		if (zoomFact < 2.0 * num4)
		{
			currentSmartDateTimeFormat = SmartDateZoomWeekLevelLabelFormat;
			return num4;
		}
		if (zoomFact < 2.0 * num3)
		{
			currentSmartDateTimeFormat = SmartDateZoomMonthLevelLabelFormat;
			return num3;
		}
		if (zoomFact < 2.0 * num2)
		{
			currentSmartDateTimeFormat = SmartDateZoomYearLevelLabelFormat;
			return num2;
		}
		currentSmartDateTimeFormat = DateTimeFormat;
		return zoomFact;
	}

	internal ChartAxisLabel GenerateLabel(double value, ChartArea area, int index)
	{
		ChartAxisLabel chartAxisLabel = ChartAxisLabel.InitializeStaticVariables();
		chartAxisLabel.IsAuto = true;
		if (ValueType == ChartValueType.Category)
		{
			chartAxisLabel.ValueType = ChartValueType.Custom;
		}
		else
		{
			chartAxisLabel.ValueType = ValueType;
		}
		chartAxisLabel.DoubleValue = value;
		if (m_scaleLables && m_scaleLablesCoef >= 0f)
		{
			Font font = Font;
			float size = 0.5f * font.Size * (1f + m_scaleLablesCoef);
			chartAxisLabel.Font = new Font(font.GetFontName(), size, font.Style, font.Unit, font.GdiCharSet, font.GdiVerticalFont);
		}
		else
		{
			chartAxisLabel.Font = Font;
		}
		if (area.Chart.Series.Count != 0 && IsIndexed)
		{
			value = area.Chart.IndexValues.GetValue(value);
		}
		switch (m_valueType)
		{
		case ChartValueType.Logarithmic:
			chartAxisLabel.CustomText = ((AxisLabelConverter != null) ? AxisLabelConverter.GetLabelText(value, "") : value.ToString(m_format));
			break;
		case ChartValueType.Double:
		{
			if (m_roundingPlaces > 15 && !string.IsNullOrEmpty(value.ToString(m_format)) && value.ToString(m_format).Contains("E-"))
			{
				string text = Math.Abs(value).ToString();
				if (text != null && text.Length < 8)
				{
					chartAxisLabel.CustomText = text;
				}
				else if (text != null && text.Length > 8)
				{
					chartAxisLabel.CustomText = "0";
				}
				break;
			}
			string format = "{0:0.##E+00}";
			if (m_roundingPlaces > -1 && AxisLabelConverter != null && AxisLabelConverter.NumberFormat == "General")
			{
				double num2 = Math.Round(value, Math.Min(m_roundingPlaces, 15));
				if (AxisLabelConverter != null)
				{
					chartAxisLabel.CustomText = AxisLabelConverter.GetLabelText(num2, "");
					if (area.DisplayUnits == 100.0)
					{
						double num3 = num2 / 100.0;
						int num4 = 0;
						while (num3 >= 1.0)
						{
							num3 /= 10.0;
							num4++;
						}
						if (num4 >= 8)
						{
							chartAxisLabel.CustomText = string.Format(CultureInfo.InvariantCulture, format, Convert.ToDouble(num2));
						}
					}
					else if (area.DisplayUnits == 1000.0)
					{
						double num5 = num2 / 1000.0;
						int num6 = 0;
						while (num5 >= 1.0)
						{
							num5 /= 10.0;
							num6++;
						}
						if (num6 >= 7)
						{
							chartAxisLabel.CustomText = string.Format(CultureInfo.InvariantCulture, format, Convert.ToDouble(num2));
						}
					}
					else if (area.DisplayUnits == 10000.0)
					{
						double num7 = num2 / 10000.0;
						int num8 = 0;
						while (num7 >= 1.0)
						{
							num7 /= 10.0;
							num8++;
						}
						if (num8 >= 6)
						{
							chartAxisLabel.CustomText = string.Format(CultureInfo.InvariantCulture, format, Convert.ToDouble(num2));
						}
					}
					else if (area.DisplayUnits == 100000.0)
					{
						double num9 = num2 / 100000.0;
						int num10 = 0;
						while (num9 >= 1.0)
						{
							num9 /= 10.0;
							num10++;
						}
						if (num10 >= 5)
						{
							chartAxisLabel.CustomText = string.Format(CultureInfo.InvariantCulture, format, Convert.ToDouble(num2));
						}
					}
					else if (area.DisplayUnits == 1000000.0)
					{
						double num11 = num2 / 1000000.0;
						int num12 = 0;
						while (num11 >= 1.0)
						{
							num11 /= 10.0;
							num12++;
						}
						if (num12 >= 4)
						{
							chartAxisLabel.CustomText = string.Format(CultureInfo.InvariantCulture, format, Convert.ToDouble(num2));
						}
					}
					else if (area.DisplayUnits == 10000000.0)
					{
						double num13 = num2 / 10000000.0;
						int num14 = 0;
						while (num13 >= 1.0)
						{
							num13 /= 10.0;
							num14++;
						}
						if (num14 >= 3)
						{
							chartAxisLabel.CustomText = string.Format(CultureInfo.InvariantCulture, format, Convert.ToDouble(num2));
						}
					}
					else if (area.DisplayUnits == 100000000.0)
					{
						double num15 = num2 / 100000000.0;
						int num16 = 0;
						while (num15 >= 1.0)
						{
							num15 /= 10.0;
							num16++;
						}
						if (num16 >= 2)
						{
							chartAxisLabel.CustomText = string.Format(CultureInfo.InvariantCulture, format, Convert.ToDouble(num2));
						}
					}
					else if (area.DisplayUnits == 1000000000.0)
					{
						double num17 = num2 / 1000000000.0;
						int num18 = 0;
						while (num17 >= 1.0)
						{
							num17 /= 10.0;
							num18++;
						}
						if (num18 >= 1)
						{
							chartAxisLabel.CustomText = string.Format(CultureInfo.InvariantCulture, format, Convert.ToDouble(num2));
						}
					}
					else if (area.DisplayUnits == 1000000000000.0)
					{
						double num19 = num2 / 10000000000.0;
						int num20 = 0;
						while (num19 >= 1.0)
						{
							num19 /= 10.0;
							num20++;
						}
						if (num20 > 0 || num19 >= 0.1)
						{
							chartAxisLabel.CustomText = string.Format(CultureInfo.InvariantCulture, format, Convert.ToDouble(num2));
						}
					}
					else if (area.DisplayUnits == 1.0 || area.DisplayUnits == 0.0)
					{
						double num21 = num2;
						if (num21 < 0.0)
						{
							num21 = 0.0 - num2;
						}
						int num22 = 0;
						while (num21 >= 1.0)
						{
							num21 /= 10.0;
							num22++;
						}
						if (num22 >= 10)
						{
							chartAxisLabel.CustomText = string.Format(CultureInfo.InvariantCulture, format, Convert.ToDouble(num2));
						}
					}
				}
				else
				{
					chartAxisLabel.CustomText = num2.ToString(m_format);
				}
			}
			else if (AxisLabelConverter != null)
			{
				chartAxisLabel.CustomText = AxisLabelConverter.GetLabelText(value, "");
			}
			else
			{
				chartAxisLabel.CustomText = value.ToString(m_format);
			}
			break;
		}
		case ChartValueType.Category:
			if (AxisLabelConverter != null)
			{
				chartAxisLabel.CustomText = AxisLabelConverter.GetLabelText(value, cLabels[index].ToString());
			}
			else
			{
				chartAxisLabel.CustomText = cLabels[index].ToString();
			}
			break;
		case ChartValueType.DateTime:
		{
			if (smartDateZoom)
			{
				if (AxisLabelConverter != null)
				{
					chartAxisLabel.CustomText = AxisLabelConverter.GetLabelText(value, "");
					break;
				}
				DateTimeFormatInfo dateTimeFormat = new CultureInfo(smartDateZoomLabelsCulture, useUserOverride: false).DateTimeFormat;
				chartAxisLabel.CustomText = DateTime.FromOADate(value).ToString(currentSmartDateTimeFormat, dateTimeFormat);
				break;
			}
			DateTime dateTime = DoubleToDateTime(value);
			if (dateTime.Day != 1 && (IntervalType == ChartDateTimeIntervalType.Months || IntervalType == ChartDateTimeIntervalType.Years))
			{
				dateTime = new DateTime(dateTime.Year, dateTime.Month, 1);
				chartAxisLabel.DoubleValue = DateTimeToDouble(dateTime);
			}
			if (AxisLabelConverter != null)
			{
				chartAxisLabel.CustomText = AxisLabelConverter.GetLabelText(value, "");
			}
			else if (m_dateTimeFormat == "")
			{
				chartAxisLabel.CustomText = dateTime.ToShortDateString();
			}
			else
			{
				chartAxisLabel.CustomText = dateTime.ToString(m_dateTimeFormat);
			}
			break;
		}
		case ChartValueType.Custom:
		{
			if (m_customLabelsParameter == ChartCustomLabelsParameter.Index)
			{
				if (index > -1 && index < LabelsImpl.Count)
				{
					chartAxisLabel.CustomText = LabelsImpl.GetLabelAt(index).Text;
					chartAxisLabel.ToolTip = LabelsImpl.GetLabelAt(index).ToolTip;
				}
				break;
			}
			int num = (int)value;
			if (num > -1 && num < LabelsImpl.Count)
			{
				chartAxisLabel.CustomText = LabelsImpl.GetLabelAt(num).Text;
				chartAxisLabel.ToolTip = LabelsImpl.GetLabelAt(num).ToolTip;
			}
			break;
		}
		}
		chartAxisLabel.m_axisLabelPlacement = AxisLabelPlacement;
		return chartAxisLabel;
	}

	private IEnumerable<float> GetSmallTicks()
	{
		List<float> list = new List<float>();
		if (BreaksEnabled && m_breakRanges.BreaksMode == ChartBreaksMode.Auto)
		{
			foreach (ChartAxisSegment segment in m_breakRanges.Segments)
			{
				double start = segment.Range.Start;
				double end = segment.Range.End;
				double num = segment.Interval / (double)(m_smallTicksPerInterval + 1);
				if (m_valueType == ChartValueType.Logarithmic)
				{
					for (double num2 = start; num2 < end; num2 += 1.0)
					{
						int num3 = 1;
						double num4 = Math.Pow(m_logBase, VisibleRange.interval) / (double)(m_smallTicksPerInterval + 2);
						double num5 = Math.Pow(m_logBase, num2);
						double num6 = Math.Pow(m_logBase, end);
						for (double num7 = num4 + 1.0; num7 < 10.0; num7 += num4)
						{
							if (num3 > m_smallTicksPerInterval)
							{
								break;
							}
							double num8 = num5 * num7;
							if (num8 > num6)
							{
								break;
							}
							list.Add(GetCoordinateFromValue(num8));
							num3++;
						}
					}
				}
				else
				{
					for (double num9 = start; num9 <= end; num9 += num)
					{
						list.Add(GetCoordinateFromValue(num9));
					}
				}
			}
		}
		else
		{
			double min = VisibleRange.min;
			double max = VisibleRange.max;
			double num10 = ((m_valueType != ChartValueType.DateTime || m_visibleLables.Length <= 1) ? (VisibleRange.interval / (double)(m_smallTicksPerInterval + 1)) : (Math.Abs(m_visibleLables[0].DoubleValue - m_visibleLables[1].DoubleValue) / (double)(m_smallTicksPerInterval + 1)));
			if (m_valueType == ChartValueType.Logarithmic)
			{
				double num11 = Math.Pow(m_logBase, min);
				double num12 = Math.Pow(m_logBase, max);
				for (double num13 = ((m_showLogBaseLabels == LogLabelsDisplayMode.IntegerLogValues) ? Math.Log(GetFirstLogBaseLabel(num11, returnLog: false) / (double)m_logBase, m_logBase) : min); num13 < max; num13 += 1.0)
				{
					int num14 = 1;
					double num15 = Math.Ceiling(Math.Pow(m_logBase, VisibleRange.interval)) / (double)(m_smallTicksPerInterval + 2);
					double num16 = Math.Pow(m_logBase, num13);
					for (double num17 = num15 + 1.0; num17 < 10.0; num17 += num15)
					{
						if (num14 > m_smallTicksPerInterval)
						{
							break;
						}
						double num18 = num16 * num17;
						if (num18 >= num11 && num18 <= num12)
						{
							list.Add(GetCoordinateFromValue(num18));
						}
						num14++;
					}
				}
			}
			else
			{
				for (double num19 = min; num19 <= max; num19 += num10)
				{
					list.Add(GetCoordinateFromValue(num19));
				}
			}
		}
		return list;
	}

	private IEnumerable<RectangleF> GetInterlacedGridRects(RectangleF bounds)
	{
		List<RectangleF> list = new List<RectangleF>();
		if (BreaksEnabled && m_breakRanges.BreaksMode == ChartBreaksMode.Auto)
		{
			foreach (ChartAxisSegment segment in m_breakRanges.Segments)
			{
				double start = segment.Range.Start;
				double end = segment.Range.End;
				double num = 2.0 * segment.Interval;
				double interval = segment.Interval;
				for (double num2 = start; num2 < end; num2 += num)
				{
					RectangleF item = default(RectangleF);
					double num3 = num2;
					double num4 = num2 + interval;
					if (m_valueType == ChartValueType.Logarithmic)
					{
						num3 = Math.Pow(m_logBase, num3);
						num4 = Math.Pow(m_logBase, num4);
					}
					float coordinateFromValue = GetCoordinateFromValue(num3);
					float coordinateFromValue2 = GetCoordinateFromValue(num4);
					if (m_orientation == ChartOrientation.Horizontal)
					{
						coordinateFromValue = ChartMath.MinMax(coordinateFromValue, bounds.Left, bounds.Right);
						coordinateFromValue2 = ChartMath.MinMax(coordinateFromValue2, bounds.Left, bounds.Right);
						item.Y = bounds.Y;
						item.Height = bounds.Height;
						item.X = Math.Min(coordinateFromValue, coordinateFromValue2);
						item.Width = Math.Abs(coordinateFromValue2 - coordinateFromValue);
					}
					else
					{
						coordinateFromValue = ChartMath.MinMax(coordinateFromValue, bounds.Top, bounds.Bottom);
						coordinateFromValue2 = ChartMath.MinMax(coordinateFromValue2, bounds.Top, bounds.Bottom);
						item.X = bounds.X;
						item.Width = bounds.Width;
						item.Y = Math.Min(coordinateFromValue, coordinateFromValue2);
						item.Height = Math.Abs(coordinateFromValue2 - coordinateFromValue);
					}
					list.Add(item);
				}
			}
		}
		else
		{
			double min = VisibleRange.min;
			double max = VisibleRange.max;
			double num5 = 2.0 * VisibleRange.interval;
			double interval2 = VisibleRange.interval;
			for (double num6 = min; num6 < max; num6 += num5)
			{
				RectangleF item2 = default(RectangleF);
				double num7 = num6;
				double num8 = num6 + interval2;
				if (m_valueType == ChartValueType.Logarithmic)
				{
					num7 = Math.Pow(m_logBase, num7);
					num8 = Math.Pow(m_logBase, num8);
				}
				float coordinateFromValue3 = GetCoordinateFromValue(num7);
				float coordinateFromValue4 = GetCoordinateFromValue(num8);
				if (m_orientation == ChartOrientation.Horizontal)
				{
					coordinateFromValue3 = ChartMath.MinMax(coordinateFromValue3, bounds.Left, bounds.Right);
					coordinateFromValue4 = ChartMath.MinMax(coordinateFromValue4, bounds.Left, bounds.Right);
					item2.Y = bounds.Y;
					item2.Height = bounds.Height;
					item2.X = Math.Min(coordinateFromValue3, coordinateFromValue4);
					item2.Width = Math.Abs(coordinateFromValue4 - coordinateFromValue3);
				}
				else
				{
					coordinateFromValue3 = ChartMath.MinMax(coordinateFromValue3, bounds.Top, bounds.Bottom);
					coordinateFromValue4 = ChartMath.MinMax(coordinateFromValue4, bounds.Top, bounds.Bottom);
					item2.X = bounds.X;
					item2.Width = bounds.Width;
					item2.Y = Math.Min(coordinateFromValue3, coordinateFromValue4);
					item2.Height = Math.Abs(coordinateFromValue4 - coordinateFromValue3);
				}
				list.Add(item2);
			}
		}
		return list;
	}

	private MinMaxInfo CalculateVisibleRange(double zoomFactor, double zoomPosition)
	{
		MinMaxInfo minMaxInfo = m_range.Clone();
		return new MinMaxInfo(minMaxInfo.min + m_offset + m_pointOffset, minMaxInfo.max + m_offset + m_pointOffset, minMaxInfo.interval);
	}

	public double RecalculateVisibleRange(double zoomFactor, double zoomPosition)
	{
		MinMaxInfo minMaxInfo = m_range.Clone();
		bool isIndexed = IsIndexed;
		double num = CalculateNiceInternalEx(m_range.Interval * zoomFactor);
		double num2 = m_range.Min + m_range.Delta * zoomPosition;
		double max = num2 + m_range.Delta * zoomFactor;
		if (ValueType != ChartValueType.Logarithmic)
		{
			int num3 = (int)Math.Ceiling(m_range.Delta * zoomFactor / num);
			if (ValueType != ChartValueType.DateTime && RangeType != ChartAxisRangeType.Set && zoomPosition != 0.0)
			{
				num2 = Math.Max(ChartMath.Round(num2, num), m_range.min);
			}
			max = num2 + (double)num3 * num;
		}
		if (isIndexed)
		{
			num = Math.Max(0.0, Math.Ceiling(num));
		}
		minMaxInfo = m_visibleRange;
		m_visibleRange = new MinMaxInfo(num2, max, num);
		this.VisibleRangeChanged(this, EventArgs.Empty);
		m_visibleRange = minMaxInfo;
		return m_visibleRange.interval;
	}

	private void RecalculateRanges()
	{
		_ = IsIndexed;
		m_labelOffset = 0.0;
		if (smartDateZoom)
		{
			SetSmartZoomFactor(1.0);
		}
		m_visibleRange = new MinMaxInfo(m_range.Min - m_offset - m_pointOffset, m_range.Max - m_offset - m_pointOffset, m_range.interval);
	}

	private void InvalidateRanges()
	{
		m_needUpdate = true;
		OnVisibleRangeChanged(EventArgs.Empty);
	}

	internal ArrayList SortYValues(ChartArea area, ChartSeries ser, ArrayList labels)
	{
		ArrayList arrayList = new ArrayList();
		int num = 0;
		int count = labels.Count;
		bool flag = false;
		foreach (ChartSeries item in area.Chart.Series)
		{
			if (item.ActualXAxis == this && item.BaseStackingType == ChartSeriesBaseStackingType.NotStacked)
			{
				num += item.Points.Count;
			}
			else if (item.BaseStackingType != ChartSeriesBaseStackingType.NotStacked)
			{
				flag = true;
				break;
			}
		}
		double[] array;
		string[] array2;
		if (flag)
		{
			array = new double[count];
			array2 = new string[count];
			foreach (ChartSeries item2 in area.Chart.Series)
			{
				if (item2.ActualXAxis != this)
				{
					continue;
				}
				for (int i = 0; i < item2.Points.Count; i++)
				{
					int num2 = labels.IndexOf(item2.Points[i].Category);
					if (num2 != -1)
					{
						array[num2] += item2.Points[i].YValues[0];
						array2[num2] = item2.Points[i].Category;
					}
				}
			}
		}
		else
		{
			int num3 = 0;
			array = new double[num];
			array2 = new string[num];
			foreach (ChartSeries item3 in area.Chart.Series)
			{
				if (item3.ActualXAxis == this)
				{
					for (int j = 0; j < item3.Points.Count; j++)
					{
						array[num3] = item3.Points[j].YValues[0];
						array2[num3] = item3.Points[j].Category;
						num3++;
					}
				}
			}
		}
		Array.Sort(array, array2, (ser.SortOrder == ChartSeriesSortingOrder.Descending) ? new DescendingComparer() : null);
		string[] array3 = array2;
		foreach (string text in array3)
		{
			if (!arrayList.Contains(text) && text != null)
			{
				arrayList.Add(text);
			}
		}
		return arrayList;
	}

	internal double GetFirstLogBaseLabel(double d, bool returnLog)
	{
		double num = 1.0;
		if (d == 0.0)
		{
			return 0.0;
		}
		if (d % (double)m_logBase == 0.0)
		{
			if (!returnLog)
			{
				return d;
			}
			return Math.Log(d, m_logBase);
		}
		if ((double)m_logBase < d)
		{
			while (num < d)
			{
				num *= (double)m_logBase;
			}
		}
		else
		{
			while (num >= d)
			{
				num /= (double)m_logBase;
			}
		}
		double num2 = (((double)m_logBase < d) ? num : ((double)m_logBase * num));
		if (!returnLog)
		{
			return num2;
		}
		return Math.Log(num2, m_logBase);
	}

	internal void RecalculateVisibleLabels(ChartArea area)
	{
		List<ChartAxisLabel> list = new List<ChartAxisLabel>();
		MinMaxInfo visibleRange = VisibleRange;
		MinMaxInfo minMaxInfo = Range;
		if (m_tickDrawingOperationMode == ChartAxisTickDrawingOperationMode.NumberOfIntervalsFixed)
		{
			minMaxInfo = visibleRange;
		}
		if ((tickLabelDrawingMode & ChartAxisTickLabelDrawingMode.AutomaticMode) == ChartAxisTickLabelDrawingMode.AutomaticMode)
		{
			if (BreaksEnabled && m_breakRanges.BreaksMode == ChartBreaksMode.Auto)
			{
				int num = 0;
				int i = 0;
				for (int count = m_breakRanges.Segments.Count; i < count; i++)
				{
					ChartAxisSegment chartAxisSegment = m_breakRanges.Segments[i] as ChartAxisSegment;
					for (double num2 = chartAxisSegment.Range.Start; (i == count - 1 && num2 == chartAxisSegment.Range.End) || num2 < chartAxisSegment.Range.End; num2 += chartAxisSegment.Interval)
					{
						num++;
						list.Add(GenerateLabel(ConvertToValueType(num2), area, num));
					}
				}
			}
			else
			{
				double num3 = minMaxInfo.Min + m_labelOffset;
				double interval = minMaxInfo.Interval;
				IEnumerator enumerator = null;
				bool flag = true;
				int num4 = 0;
				if (m_valueType == ChartValueType.DateTime && m_dateTimeInterval != null && m_dateTimeInterval.Type != 0)
				{
					DateTime rangeStart = DateTime.FromOADate(minMaxInfo.Min);
					DateTime rangeEnd = DateTime.FromOADate(minMaxInfo.Max);
					enumerator = m_dateTimeInterval.Iterator(rangeStart, rangeEnd).GetEnumerator();
					enumerator.Reset();
					enumerator.MoveNext();
				}
				if (m_valueType == ChartValueType.Category)
				{
					cLabels.Clear();
					cLabels = GetCategoryLabels(area);
					foreach (ChartSeries item in area.Chart.Series)
					{
						if (item.Trendlines.Count > 0 && item.ActualXAxis.RangeType == ChartAxisRangeType.Auto)
						{
							cLabels = GetCategoryTrendlineLabels(area);
						}
					}
					if (RangeType != ChartAxisRangeType.Set && !IsDateTimeCategoryAxis)
					{
						SetNiceRange(new DoubleRange(0.0, cLabels.Count - 1), m_rangePaddingType, area);
					}
					foreach (ChartSeries item2 in area.Chart.Series)
					{
						if (item2.ActualXAxis != this || !item2.SortPoints || item2.Trendlines.Count != 0 || item2.Type == ChartSeriesType.HiLo || item2.Type == ChartSeriesType.Candle)
						{
							continue;
						}
						if (item2.SortBy == ChartSeriesSortingType.X)
						{
							if (item2.SortOrder == ChartSeriesSortingOrder.Ascending)
							{
								cLabels.Sort();
							}
							else if (item2.SortOrder == ChartSeriesSortingOrder.Descending)
							{
								cLabels.Sort(new DescendingComparer());
							}
						}
						else
						{
							cLabels = SortYValues(area, item2, cLabels);
						}
						break;
					}
					int num5 = ((m_visibleRange.Interval < 1.0) ? 1 : ((int)m_visibleRange.Interval));
					for (int j = 0; j < cLabels.Count; j += num5)
					{
						list.Add(GenerateLabel(j, area, j));
						num4++;
					}
					flag = false;
				}
				else if (m_valueType == ChartValueType.Logarithmic && m_showLogBaseLabels == LogLabelsDisplayMode.IntegerLogValues)
				{
					int num6 = 0;
					double firstLogBaseLabel = GetFirstLogBaseLabel(Math.Pow(m_logBase, m_visibleRange.min), returnLog: true);
					double num7 = Math.Ceiling(m_visibleRange.interval);
					for (double num8 = firstLogBaseLabel; num8 <= m_visibleRange.max; num8 += num7)
					{
						list.Add(GenerateLabel(ConvertToValueType(num8), area, num6++));
					}
					flag = false;
				}
				TryAndSetRoundingPlaces(visibleRange);
				while (flag)
				{
					double num9 = num3 + m_labelsOffset + m_pointOffset;
					if (BreaksEnabled)
					{
						if (m_breakRanges.IsVisible(num9))
						{
							list.Add(GenerateLabel(ConvertToValueType(num9), area, num4));
						}
					}
					else if (Inside(visibleRange, num9 - m_pointOffset))
					{
						list.Add(GenerateLabel(ConvertToValueType(num9), area, num4));
					}
					num4++;
					if (enumerator != null)
					{
						if (flag = enumerator.MoveNext())
						{
							num3 = ((DateTime)enumerator.Current).ToOADate();
						}
					}
					else
					{
						num3 += interval;
						flag = ((ValueType == ChartValueType.Double) ? (num3 - visibleRange.max < 1E-15) : (num3 - visibleRange.max < 1E-06));
					}
				}
			}
		}
		if ((tickLabelDrawingMode & ChartAxisTickLabelDrawingMode.UserMode) == ChartAxisTickLabelDrawingMode.UserMode)
		{
			bool flag2 = m_valueType == ChartValueType.Logarithmic;
			for (int k = 0; k < LabelsImpl.Count; k++)
			{
				ChartAxisLabel labelAt = LabelsImpl.GetLabelAt(k);
				double num10 = (flag2 ? Math.Log(labelAt.DoubleValue, m_logBase) : labelAt.DoubleValue);
				if (visibleRange.Contains(num10) && (!BreaksEnabled || !m_breakRanges.IsVisible(num10)))
				{
					list.Add(labelAt);
				}
			}
		}
		m_visibleLables = list.ToArray();
		tempLabels = cLabels.ToArray();
		if (m_valueType != ChartValueType.Category && m_valueType != ChartValueType.DateTime)
		{
			Array.Sort(m_visibleLables, new ChartAxisLabelByDoubleValueComparer(m_inversed));
		}
	}

	private void TryAndSetRoundingPlaces(MinMaxInfo vRange)
	{
		if (!(Math.Abs(vRange.min) < 0.01) || !(Math.Abs(vRange.max) < 0.01))
		{
			return;
		}
		string text = Math.Abs(vRange.max).ToString();
		int num = text.IndexOf("E-");
		if (num != -1 && num + 1 < text.Length)
		{
			if (int.TryParse(text.Substring(num + 1), out var result))
			{
				RoundingPlaces = Math.Abs(result);
			}
			return;
		}
		num = text.IndexOf(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
		if (num > 0 && num < text.Length)
		{
			RoundingPlaces = text.Length - (num + 1);
		}
	}

	private ArrayList GetCategoryLabels(ChartArea area)
	{
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		foreach (ChartSeries item in area.Chart.Series)
		{
			if (!item.Visible || item.ActualXAxis != this)
			{
				continue;
			}
			IChartSeriesCategory categoryModel = item.CategoryModel;
			for (int i = 0; i < item.Points.Count; i++)
			{
				string category = categoryModel.GetCategory(i);
				if (arrayList2.Count <= 0 || !arrayList2.Contains(category))
				{
					arrayList.Add(category);
				}
			}
			arrayList2 = arrayList;
		}
		return arrayList;
	}

	private ArrayList GetCategoryTrendlineLabels(ChartArea area)
	{
		ArrayList arrayList = new ArrayList();
		foreach (ChartSeries item in area.Chart.Series)
		{
			if (!item.Visible || item.ActualXAxis != this || item.Trendlines.Count <= 0)
			{
				continue;
			}
			IChartSeriesCategory categoryModel = item.CategoryModel;
			int num = (((double)item.Points.Count > VisibleRange.Max) ? item.Points.Count : ((int)VisibleRange.Max));
			for (int i = 0; i < num; i++)
			{
				string category = categoryModel.GetCategory(i);
				if (i >= item.Points.Count)
				{
					arrayList.Add(" ");
				}
				else if (!arrayList.Contains(category))
				{
					arrayList.Add(category);
				}
			}
		}
		return arrayList;
	}

	private bool Inside(MinMaxInfo mmi, double value)
	{
		if (value - mmi.min < -1E-06)
		{
			return false;
		}
		if (value - mmi.max > 1E-06)
		{
			return false;
		}
		return true;
	}

	private double DateTimeToDouble(DateTime dateTime)
	{
		if (dateTime < c_dateTimeZero)
		{
			return dateTime.Date.ToOADate() - dateTime.ToOADate() % 1.0;
		}
		return dateTime.ToOADate();
	}

	private DateTime DoubleToDateTime(double value)
	{
		if (value < 0.0)
		{
			return DateTime.FromOADate(Math.Floor(value) - 1.0 - value % 1.0);
		}
		return DateTime.FromOADate(value);
	}

	public void DisposeChartAxis()
	{
		if (m_stripLines != null)
		{
			foreach (ChartStripLine stripLine in m_stripLines)
			{
				stripLine.Dispose();
			}
		}
		if (m_font != null)
		{
			m_font.Dispose();
			m_font = null;
		}
		if (m_titleFont != null)
		{
			m_titleFont.Dispose();
			m_titleFont = null;
		}
		if (m_visibleLables != null)
		{
			ChartAxisLabel[] visibleLables = m_visibleLables;
			for (int i = 0; i < visibleLables.Length; i++)
			{
				visibleLables[i].Dispose();
			}
		}
		for (int j = 0; j < GroupingLabelsImpl.Count; j++)
		{
			GroupingLabelsImpl.GetGroupingLabelAt(j).Dispose();
		}
	}

	public void Dispose()
	{
		if (m_area != null)
		{
			m_area.Axes.Remove(this);
			m_area = null;
		}
		if (m_stripLines != null)
		{
			m_stripLines.Changed -= OnNeedRedraw;
			m_stripLines.Clear();
			m_stripLines = null;
		}
		m_visibleLables = null;
		m_labelStringFormat.Dispose();
		m_lineType.SettingsChanged -= OnNeedRedraw;
		m_gridLineType.SettingsChanged -= OnNeedRedraw;
		if (m_range != null)
		{
			m_range.SettingsChanged -= OnNeedResize;
			m_range = null;
		}
		if (GroupingLabels != null)
		{
			GroupingLabels.Changed -= OnNeedResize;
		}
		if (Labels != null)
		{
			Labels.Changed -= OnNeedResize;
		}
	}
}
