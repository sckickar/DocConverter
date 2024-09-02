using System;
using System.Collections.Generic;
using System.ComponentModel;
using SkiaSharp;
using DocGen.Chart.Drawing;
using DocGen.Chart.Renderers;
using DocGen.ChartToImageConverter;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartSeries : IChartSeriesStylesHost
{
	private const string c_seriesToolTipFormat = "{0}";

	private const string c_pointsToolTipFormat = "{4}";

	protected ChartModel m_model;

	protected ChartSeriesIndexedModelAdapter m_indexedModelAdapter;

	protected string m_name;

	protected ChartPointIndexer m_pointsIndexer;

	protected ChartPrepareStyleInfoHandler m_prepareSeriesStyleInfoHandler;

	protected ChartPrepareStyleInfoHandler m_prepareStyleInfoHandler;

	protected ChartSeriesConfigCollection m_seriesConfigCollection;

	protected IChartSeriesModel m_seriesModelAdapter;

	private IChartSeriesCategory m_seriesCategoryModelAdapter;

	protected IChartSeriesModel m_seriesModel;

	private IChartSeriesCategory m_seriesCategoryModel;

	protected IChartSeriesStylesModel m_seriesStylesModel;

	protected ChartStyleInfoIndexer m_stylesIndexer;

	protected string m_text;

	private IDictionary<int, string> tempDataLabelsResult;

	private bool m_enableStyles = true;

	private int m_explodedIndex = -1;

	private object m_tag;

	private bool m_explodedAll;

	private float m_explosionOffset = 20f;

	private ChartSeriesRenderer m_renderer;

	private IChartSeriesSummary m_seriesSummary;

	private ChartSeriesType m_type = ChartSeriesType.Column;

	private bool m_visible = true;

	private bool m_compatible = true;

	private ChartAxis m_xAxis;

	private ChartAxis m_yAxis;

	private int m_zOrder = -1;

	private string m_stackingGroup = "Default Group";

	private double m_heightBox = 1.0;

	private double m_reversalAmount = 1.0;

	private bool m_reversalIsPercent = true;

	private TrendlineCollection m_trendlines;

	private bool m_optimizePiePointPositions = true;

	private string m_seriesToolTipFormat = "{0}";

	private string m_pointsToolTipFormat = "{4}";

	private bool m_rotate;

	private bool m_drawColumnSeparatingLines;

	private bool m_showTicks = true;

	private ScatterConnectType m_scatterConnect;

	private double m_scatterSplineTension = 0.5;

	private bool m_drawSeriesNameInDepth;

	private float m_seriesNameOXAngle = 90f;

	private ChartSeriesLegendItem m_legendItem;

	private bool m_legendItemUseSeriesStyle = true;

	private bool m_needUpdateLegend = true;

	private ChartLegendItemStyle m_baseLegendStyle = new ChartLegendItemStyle();

	private bool m_smartLabels;

	private string m_legendName = "";

	private bool m_sortPoints = true;

	private ChartSeriesSortingType m_sortBy;

	private ChartSeriesSortingOrder m_sortPath;

	private ChartPointFormatsRegistry m_pointFormats;

	private float m_smartLabelsBorderWidth = 1f;

	private Color m_smartLabelsBorderColor = Color.Red;

	public bool m_resetStyles = true;

	private ChartControl m_chartControl;

	private double m_seriesResolution;

	private Arrow m_beginArrow;

	private Arrow m_endArrow;

	private List<Arrow> m_beginArrows;

	private List<Arrow> m_endArrows;

	private LabelConvertor m_labelConvertor;

	private MarkerConverter m_markerConvertor;

	private int m_index = -1;

	private string m_serieType;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(CollectionConverter))]
	public ChartPointIndexer Points
	{
		get
		{
			if (m_pointsIndexer == null)
			{
				m_pointsIndexer = new ChartPointIndexer(SeriesModelAdapter, SeriesCategoryModelAdapter);
			}
			return m_pointsIndexer;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	internal ChartPointFormatsRegistry PointFormats => m_pointFormats;

	[DefaultValue("")]
	[Category("Data")]
	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			if (m_name != value)
			{
				m_name = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public object Tag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	[DefaultValue("")]
	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			if (m_text != value)
			{
				m_text = value;
				UpdateLegendItem();
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartModel ChartModel
	{
		get
		{
			return m_model;
		}
		set
		{
			if (m_model != value)
			{
				if (m_model != null)
				{
					m_model.ColorModel.Changed -= OnColorModelChanged;
					m_model.Series.Remove(this);
				}
				m_model = value;
				if (m_model != null)
				{
					m_model.ColorModel.Changed += OnColorModelChanged;
					m_model.Series.Add(this);
				}
				StylesImpl.ComposedStyles.ResetCache();
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public IChartSeriesIndexedModel SeriesIndexedModelImpl
	{
		get
		{
			return m_indexedModelAdapter.Inner;
		}
		set
		{
			m_indexedModelAdapter = new ChartSeriesIndexedModelAdapter(value);
			SetSeriesModel(m_indexedModelAdapter);
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	[Browsable(false)]
	public IChartSeriesModel SeriesModel
	{
		get
		{
			if (m_seriesModel == null)
			{
				SetSeriesModel(OnCreateSeriesModel());
			}
			return m_seriesModel;
		}
		set
		{
			SetSeriesModel(value);
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	[Browsable(false)]
	public IChartSeriesCategory CategoryModel
	{
		get
		{
			if (m_seriesCategoryModel == null)
			{
				SetCategorySeriesModel(OnCreateGategorySeriesModel());
			}
			return m_seriesCategoryModel;
		}
		set
		{
			if (value is IChartSeriesModel)
			{
				SeriesModel = (IChartSeriesModel)value;
			}
			SetCategorySeriesModel(value);
		}
	}

	internal EmptyPointValue EmptyPointValue { get; set; }

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	internal IChartSeriesModel SeriesModelAdapter
	{
		get
		{
			if (m_seriesModelAdapter == null)
			{
				SetSeriesModelAdapter(new ChartSeriesModelAdapter(this));
			}
			return m_seriesModelAdapter;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	internal IChartSeriesCategory SeriesCategoryModelAdapter
	{
		get
		{
			if (m_seriesCategoryModelAdapter == null)
			{
				SetSeriesGategoryModelAdapter(new ChartSeriesModelAdapter(this));
			}
			return m_seriesCategoryModelAdapter;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public IChartSeriesSummary Summary
	{
		get
		{
			if (m_seriesSummary == null)
			{
				SetSummaryImpl(OnCreateSeriesSummaryImpl());
			}
			return m_seriesSummary;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartAxis XAxis
	{
		get
		{
			return m_xAxis;
		}
		set
		{
			if (m_xAxis != value)
			{
				m_xAxis = value;
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartAxis ActualXAxis
	{
		get
		{
			if (!RequireInvertedAxes)
			{
				return XAxis;
			}
			return YAxis;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartAxis ActualYAxis
	{
		get
		{
			if (!RequireInvertedAxes)
			{
				return YAxis;
			}
			return XAxis;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ChartValueType XType => ActualXAxis.ValueType;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartAxis YAxis
	{
		get
		{
			return m_yAxis;
		}
		set
		{
			if (m_yAxis != value)
			{
				m_yAxis = value;
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ChartValueType YType => ActualYAxis.ValueType;

	public double Resolution
	{
		get
		{
			return m_seriesResolution;
		}
		set
		{
			m_seriesResolution = value;
		}
	}

	[DefaultValue(true)]
	public bool EnableStyles
	{
		get
		{
			return m_enableStyles;
		}
		set
		{
			m_enableStyles = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Browsable(false)]
	public ChartStyleInfo Style
	{
		get
		{
			if (m_seriesStylesModel == null)
			{
				SetStylesImpl(OnCreateSeriesStylesModelImpl());
			}
			return m_seriesStylesModel.Style;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Browsable(false)]
	public ChartStyleInfoIndexer Styles
	{
		get
		{
			if (m_seriesStylesModel == null)
			{
				SetStylesImpl(OnCreateSeriesStylesModelImpl());
			}
			return m_stylesIndexer;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public IChartSeriesStylesModel StylesImpl
	{
		get
		{
			if (m_seriesStylesModel == null)
			{
				SetStylesImpl(OnCreateSeriesStylesModelImpl());
			}
			return m_seriesStylesModel;
		}
		set
		{
			SetStylesImpl(value);
		}
	}

	[Browsable(false)]
	public ChartSeriesBaseType BaseType
	{
		get
		{
			if (m_type == ChartSeriesType.Bar || m_type == ChartSeriesType.Candle || m_type == ChartSeriesType.Column || m_type == ChartSeriesType.ColumnRange || m_type == ChartSeriesType.HiLo || m_type == ChartSeriesType.HiLoOpenClose || m_type == ChartSeriesType.Gantt || m_type == ChartSeriesType.BoxAndWhisker || m_type == ChartSeriesType.StackingColumn || m_type == ChartSeriesType.StackingBar || m_type == ChartSeriesType.StackingArea100 || m_type == ChartSeriesType.StackingBar100 || m_type == ChartSeriesType.StackingColumn100 || m_type == ChartSeriesType.StackingLine100)
			{
				return ChartSeriesBaseType.SideBySide;
			}
			if (m_type == ChartSeriesType.Area || m_type == ChartSeriesType.Line || m_type == ChartSeriesType.SplineArea || m_type == ChartSeriesType.RotatedSpline || m_type == ChartSeriesType.Spline || m_type == ChartSeriesType.StepArea || m_type == ChartSeriesType.Tornado || m_type == ChartSeriesType.StackingArea)
			{
				return ChartSeriesBaseType.Independent;
			}
			if (m_type == ChartSeriesType.Radar || m_type == ChartSeriesType.Polar)
			{
				return ChartSeriesBaseType.Circular;
			}
			if (m_type == ChartSeriesType.Pie || m_type == ChartSeriesType.Funnel || m_type == ChartSeriesType.Pyramid || m_type == ChartSeriesType.HeatMap)
			{
				return ChartSeriesBaseType.Single;
			}
			return ChartSeriesBaseType.Other;
		}
	}

	[Browsable(false)]
	public ChartSeriesBaseStackingType BaseStackingType
	{
		get
		{
			if (m_type == ChartSeriesType.StackingBar || m_type == ChartSeriesType.StackingColumn || m_type == ChartSeriesType.StackingArea)
			{
				return ChartSeriesBaseStackingType.Stacked;
			}
			if (m_type == ChartSeriesType.StackingArea100 || m_type == ChartSeriesType.StackingBar100 || m_type == ChartSeriesType.StackingColumn100 || m_type == ChartSeriesType.StackingLine100)
			{
				return ChartSeriesBaseStackingType.FullStacked;
			}
			return ChartSeriesBaseStackingType.NotStacked;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartSeriesConfigCollection ConfigItems => m_seriesConfigCollection;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartSeriesRenderer Renderer
	{
		get
		{
			return m_renderer;
		}
		set
		{
			m_renderer = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool RequireAxes
	{
		get
		{
			if (BaseType != ChartSeriesBaseType.Circular)
			{
				return BaseType != ChartSeriesBaseType.Single;
			}
			return false;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool RequireInvertedAxes
	{
		get
		{
			if (m_type != ChartSeriesType.Bar && m_type != ChartSeriesType.Gantt && m_type != ChartSeriesType.StackingBar && m_type != ChartSeriesType.StackingBar100 && m_type != ChartSeriesType.RotatedSpline && m_type != ChartSeriesType.Tornado)
			{
				return Rotate;
			}
			return true;
		}
	}

	[Browsable(false)]
	public bool OriginDependent
	{
		get
		{
			if (m_type != ChartSeriesType.Area && m_type != ChartSeriesType.Bar && m_type != ChartSeriesType.Column && m_type != ChartSeriesType.SplineArea && m_type != ChartSeriesType.StackingArea && m_type != ChartSeriesType.StackingBar && m_type != ChartSeriesType.StackingColumn && m_type != ChartSeriesType.StepArea && m_type != ChartSeriesType.StackingArea100 && m_type != ChartSeriesType.StackingBar100 && m_type != ChartSeriesType.Histogram)
			{
				return m_type == ChartSeriesType.StackingColumn100;
			}
			return true;
		}
	}

	[DefaultValue(ChartSeriesType.Column)]
	[Category("Chart")]
	public ChartSeriesType Type
	{
		get
		{
			return m_type;
		}
		set
		{
			if (m_type != value || m_renderer == null)
			{
				m_type = value;
				m_pointFormats.OnSeriesTypeChanged(value);
				switch (m_type)
				{
				case ChartSeriesType.RangeArea:
					m_renderer = new RangeAreaRenderer(this);
					break;
				case ChartSeriesType.Line:
					m_renderer = new LineRenderer(this);
					break;
				case ChartSeriesType.Column:
					m_renderer = new ColumnRenderer(this);
					break;
				case ChartSeriesType.Pie:
					m_renderer = new PieRenderer(this);
					break;
				case ChartSeriesType.Area:
					m_renderer = new AreaRenderer(this);
					break;
				case ChartSeriesType.Bar:
					m_renderer = new BarRenderer(this);
					break;
				case ChartSeriesType.SplineArea:
					m_renderer = new SplineAreaRenderer(this);
					break;
				case ChartSeriesType.Gantt:
					m_renderer = new GanttRenderer(this);
					break;
				case ChartSeriesType.StackingArea:
					m_renderer = new StackingAreaRenderer(this);
					break;
				case ChartSeriesType.HiLo:
					m_renderer = new HiLoRenderer(this);
					break;
				case ChartSeriesType.HiLoOpenClose:
					m_renderer = new HiLoOpenCloseRenderer(this);
					break;
				case ChartSeriesType.Candle:
					m_renderer = new CandleRenderer(this);
					break;
				case ChartSeriesType.Scatter:
					m_renderer = new ScatterRenderer(this);
					break;
				case ChartSeriesType.StackingColumn:
					m_renderer = new StackingColumnRenderer(this);
					break;
				case ChartSeriesType.StackingBar:
					m_renderer = new StackingBarRenderer(this);
					break;
				case ChartSeriesType.Spline:
					m_renderer = new SplineRenderer(this);
					break;
				case ChartSeriesType.Bubble:
					m_renderer = new BubbleRenderer(this);
					break;
				case ChartSeriesType.Custom:
					m_renderer = new ChartSeriesRenderer(this);
					break;
				case ChartSeriesType.StepLine:
					m_renderer = new StepLineRenderer(this);
					break;
				case ChartSeriesType.StepArea:
					m_renderer = new StepAreaRenderer(this);
					break;
				case ChartSeriesType.Radar:
					m_renderer = new RadarRenderer(this);
					break;
				case ChartSeriesType.Kagi:
					m_renderer = new KagiRenderer(this);
					break;
				case ChartSeriesType.Renko:
					m_renderer = new RenkoRenderer(this);
					break;
				case ChartSeriesType.Polar:
					m_renderer = new RadarRenderer(this);
					break;
				case ChartSeriesType.ThreeLineBreak:
					m_renderer = new ThreeLineBreakRenderer(this);
					break;
				case ChartSeriesType.PointAndFigure:
					m_renderer = new PointAndFigureRenderer(this);
					break;
				case ChartSeriesType.RotatedSpline:
					m_renderer = new RotatedSplineRenderer(this);
					break;
				case ChartSeriesType.ColumnRange:
					m_renderer = new ColumnRangeRenderer(this);
					break;
				case ChartSeriesType.BoxAndWhisker:
					m_renderer = new BoxWhiskerRenderer(this);
					break;
				case ChartSeriesType.Histogram:
					m_renderer = new HistogramRenderer(this);
					break;
				case ChartSeriesType.Tornado:
					m_renderer = new TornadoRenderer(this);
					break;
				case ChartSeriesType.StackingArea100:
					m_renderer = new FullStackingAreaRenderer(this);
					break;
				case ChartSeriesType.StackingBar100:
					m_renderer = new FullStackingBarRenderer(this);
					break;
				case ChartSeriesType.StackingColumn100:
					m_renderer = new FullStackedColumnRenderer(this);
					break;
				case ChartSeriesType.StackingLine100:
					m_renderer = new FullStackingLineRenderer(this);
					break;
				case ChartSeriesType.Funnel:
					m_renderer = new FunnelRenderer(this);
					break;
				case ChartSeriesType.Pyramid:
					m_renderer = new PyramidRenderer(this);
					break;
				case ChartSeriesType.HeatMap:
					m_renderer = new ChartHeatMapRenderer(this);
					break;
				}
				OnSeriesChanged(EventArgs.Empty);
				InvalidateStyles();
			}
		}
	}

	public string StackingGroup
	{
		get
		{
			return m_stackingGroup;
		}
		set
		{
			if (m_stackingGroup != value)
			{
				m_stackingGroup = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public int ZOrder
	{
		get
		{
			return m_zOrder;
		}
		set
		{
			if (m_zOrder != value)
			{
				m_zOrder = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public string SeriesToolTipFormat
	{
		get
		{
			return m_seriesToolTipFormat;
		}
		set
		{
			if (m_seriesToolTipFormat != value)
			{
				m_seriesToolTipFormat = value;
				UpdateRenderer(ChartUpdateFlags.Styles);
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public string PointsToolTipFormat
	{
		get
		{
			return m_pointsToolTipFormat;
		}
		set
		{
			if (m_pointsToolTipFormat != value)
			{
				m_pointsToolTipFormat = value;
				UpdateRenderer(ChartUpdateFlags.Styles);
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(true)]
	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			if (m_visible != value)
			{
				m_visible = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[Browsable(false)]
	public Color BackColor
	{
		get
		{
			int index = m_model.Series.IndexOf(this);
			return m_model.ColorModel.GetColor(index);
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool Compatible
	{
		get
		{
			return m_compatible;
		}
		set
		{
			if (m_compatible != value)
			{
				m_compatible = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Browsable(false)]
	public TrendlineCollection Trendlines => m_trendlines;

	[NotifyParentProperty(true)]
	[DefaultValue(ChartSeriesSortingType.X)]
	public ChartSeriesSortingType SortBy
	{
		get
		{
			return m_sortBy;
		}
		set
		{
			if (m_sortBy != value)
			{
				m_sortBy = value;
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[NotifyParentProperty(true)]
	[DefaultValue(ChartSeriesSortingOrder.Ascending)]
	public ChartSeriesSortingOrder SortOrder
	{
		get
		{
			return m_sortPath;
		}
		set
		{
			if (m_sortPath != value)
			{
				m_sortPath = value;
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(true)]
	public bool SortPoints
	{
		get
		{
			return m_sortPoints;
		}
		set
		{
			m_sortPoints = value;
			OnSeriesChanged(EventArgs.Empty);
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartSeriesLegendItem LegendItem
	{
		get
		{
			if (m_legendItem == null)
			{
				m_legendItem = new ChartSeriesLegendItem(this);
			}
			if (m_needUpdateLegend)
			{
				UpdateLegendItem();
				m_needUpdateLegend = false;
			}
			return m_legendItem;
		}
	}

	[DefaultValue("")]
	public string LegendName
	{
		get
		{
			return m_legendName;
		}
		set
		{
			m_legendName = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool LegendItemUseSeriesStyle
	{
		get
		{
			return m_legendItemUseSeriesStyle;
		}
		set
		{
			if (m_legendItemUseSeriesStyle != value)
			{
				m_legendItemUseSeriesStyle = value;
				if (!m_legendItemUseSeriesStyle)
				{
					LegendItem.ItemStyle.Clear();
				}
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(-1)]
	public int ExplodedIndex
	{
		get
		{
			return m_explodedIndex;
		}
		set
		{
			if (m_explodedIndex != value)
			{
				m_explodedIndex = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(false)]
	public bool ExplodedAll
	{
		get
		{
			return m_explodedAll;
		}
		set
		{
			if (m_explodedAll != value)
			{
				m_explodedAll = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(20f)]
	public float ExplosionOffset
	{
		get
		{
			return m_explosionOffset;
		}
		set
		{
			value = ChartMath.MinMax(value, 0f, 100f);
			if (m_explosionOffset != value)
			{
				m_explosionOffset = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(1.0)]
	public double ReversalAmount
	{
		get
		{
			return m_reversalAmount;
		}
		set
		{
			if (m_reversalAmount != value)
			{
				m_reversalAmount = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(true)]
	public bool ReversalIsPercent
	{
		get
		{
			return m_reversalIsPercent;
		}
		set
		{
			if (m_reversalIsPercent != value)
			{
				m_reversalIsPercent = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(1.0)]
	public double HeightBox
	{
		get
		{
			return m_heightBox;
		}
		set
		{
			m_heightBox = value;
		}
	}

	[DefaultValue(true)]
	public bool OptimizePiePointPositions
	{
		get
		{
			return m_optimizePiePointPositions;
		}
		set
		{
			if (m_optimizePiePointPositions != value)
			{
				m_optimizePiePointPositions = value;
				OnAppearanceChanged(EventArgs.Empty);
			}
		}
	}

	[Description("Gets or sets a value indicating whether reset all the styles while modifying the ChartPoint properties.")]
	[DefaultValue(true)]
	public bool ResetStyles
	{
		get
		{
			return m_resetStyles;
		}
		set
		{
			if (value != m_resetStyles)
			{
				m_resetStyles = value;
			}
		}
	}

	[DefaultValue(false)]
	[Category("Apearance")]
	public bool Rotate
	{
		get
		{
			return m_rotate;
		}
		set
		{
			if (m_rotate != value)
			{
				m_rotate = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(false)]
	public bool DrawColumnSeparatingLines
	{
		get
		{
			return m_drawColumnSeparatingLines;
		}
		set
		{
			if (m_drawColumnSeparatingLines != value)
			{
				m_drawColumnSeparatingLines = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(true)]
	[Category("Apearance")]
	public bool ShowTicks
	{
		get
		{
			return m_showTicks;
		}
		set
		{
			if (m_showTicks != value)
			{
				m_showTicks = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(ScatterConnectType.None)]
	public ScatterConnectType ScatterConnectType
	{
		get
		{
			return m_scatterConnect;
		}
		set
		{
			if (m_scatterConnect != value)
			{
				m_scatterConnect = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(0.5)]
	public double ScatterSplineTension
	{
		get
		{
			return m_scatterSplineTension;
		}
		set
		{
			if (m_scatterSplineTension != value)
			{
				m_scatterSplineTension = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(false)]
	[Category("Apearance")]
	public bool DrawSeriesNameInDepth
	{
		get
		{
			return m_drawSeriesNameInDepth;
		}
		set
		{
			if (m_drawSeriesNameInDepth != value)
			{
				m_drawSeriesNameInDepth = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(90f)]
	public float SeriesNameOXAngle
	{
		get
		{
			return m_seriesNameOXAngle;
		}
		set
		{
			if (m_seriesNameOXAngle != value)
			{
				m_seriesNameOXAngle = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(false)]
	[Category("Apearance")]
	public bool SmartLabels
	{
		get
		{
			return m_smartLabels;
		}
		set
		{
			if (m_smartLabels != value)
			{
				m_smartLabels = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[Description("Specifies the BorderWidth of the Smartlabels")]
	[DefaultValue(1f)]
	public float SmartLabelsBorderWidth
	{
		get
		{
			return m_smartLabelsBorderWidth;
		}
		set
		{
			if (m_smartLabelsBorderWidth != value)
			{
				m_smartLabelsBorderWidth = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	[Description("Specifies the BorderColor of the Smartlabels")]
	[DefaultValue(typeof(Color), "Red")]
	[Category("Appearance")]
	public Color SmartLabelsBorderColor
	{
		get
		{
			return m_smartLabelsBorderColor;
		}
		set
		{
			if (m_smartLabelsBorderColor != value)
			{
				m_smartLabelsBorderColor = value;
				OnAppearanceChanged(EventArgs.Empty);
				OnSeriesChanged(EventArgs.Empty);
			}
		}
	}

	internal ChartControl ParentChart => m_chartControl;

	[DefaultValue(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ConfigItems.ErrorBars.Enabled property")]
	public bool DrawErrorBars
	{
		get
		{
			return m_seriesConfigCollection.ErrorBars.Enabled;
		}
		set
		{
			m_seriesConfigCollection.ErrorBars.Enabled = value;
		}
	}

	[DefaultValue(ChartSymbolShape.Diamond)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Use ConfigItems.ErrorBars.SymbolShape property")]
	public ChartSymbolShape ErrorBarsSymbolShape
	{
		get
		{
			return m_seriesConfigCollection.ErrorBars.SymbolShape;
		}
		set
		{
			m_seriesConfigCollection.ErrorBars.SymbolShape = value;
		}
	}

	[DefaultValue(10)]
	[Obsolete("Use ConfigItems.HistogramItem.NumberOfIntervals")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public int NumberOfHistogramIntervals
	{
		get
		{
			return m_seriesConfigCollection.HistogramItem.NumberOfIntervals;
		}
		set
		{
			m_seriesConfigCollection.HistogramItem.NumberOfIntervals = value;
		}
	}

	[DefaultValue(true)]
	[Obsolete("Use ConfigItems.HistogramItem.ShowDataPoints")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShowHistogramDataPoints
	{
		get
		{
			return m_seriesConfigCollection.HistogramItem.ShowDataPoints;
		}
		set
		{
			m_seriesConfigCollection.HistogramItem.ShowDataPoints = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[Obsolete("Use SeriesModel property. This property just duplicate it.")]
	public IChartSeriesModel SeriesModelImpl
	{
		get
		{
			return SeriesModel;
		}
		set
		{
			SetSeriesModel(value);
		}
	}

	[DefaultValue(false)]
	[Obsolete("Use ConfigItems.HistogramItem.ShowNormalDistribution")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool DrawHistogramNormalDistribution
	{
		get
		{
			return m_seriesConfigCollection.HistogramItem.ShowNormalDistribution;
		}
		set
		{
			m_seriesConfigCollection.HistogramItem.ShowNormalDistribution = value;
		}
	}

	[Obsolete("Use ConfigItems.GanttItem.DrawMode")]
	[DefaultValue(ChartGanttDrawMode.CustomPointWidthMode)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ChartGanttDrawMode GanttDrawMode
	{
		get
		{
			return m_seriesConfigCollection.GanttItem.DrawMode;
		}
		set
		{
			m_seriesConfigCollection.GanttItem.DrawMode = value;
		}
	}

	[DefaultValue(ChartOpenCloseDrawMode.Both)]
	[Obsolete("Use ConfigItems.HiLoOpenCloseItem.DrawMode")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ChartOpenCloseDrawMode OpenCloseDrawMode
	{
		get
		{
			return ConfigItems.HiLoOpenCloseItem.DrawMode;
		}
		set
		{
			ConfigItems.HiLoOpenCloseItem.DrawMode = value;
		}
	}

	[Obsolete("Use ConfigItems.PieItem.DoughnutCoeficient")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public float InSideRadius
	{
		get
		{
			return ConfigItems.PieItem.DoughnutCoeficient;
		}
		set
		{
			ConfigItems.PieItem.DoughnutCoeficient = value;
		}
	}

	[Obsolete("Use ConfigItems.FinancialItem.PriceUpColor")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Color PriceUpColor
	{
		get
		{
			return ConfigItems.FinancialItem.PriceUpColor;
		}
		set
		{
			ConfigItems.FinancialItem.PriceUpColor = value;
		}
	}

	[Obsolete("Use CConfigItems.FinancialItem.PriceDownColor")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public Color PriceDownColor
	{
		get
		{
			return ConfigItems.FinancialItem.PriceDownColor;
		}
		set
		{
			ConfigItems.FinancialItem.PriceDownColor = value;
		}
	}

	internal LabelConvertor LabelConverterObject
	{
		get
		{
			return m_labelConvertor;
		}
		set
		{
			m_labelConvertor = value;
		}
	}

	internal MarkerConverter MarkerConverterObject
	{
		get
		{
			return m_markerConvertor;
		}
		set
		{
			m_markerConvertor = value;
		}
	}

	internal int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	internal string SerieType
	{
		get
		{
			return m_serieType;
		}
		set
		{
			m_serieType = value;
		}
	}

	internal IDictionary<int, string> TempDataLabelsResult
	{
		get
		{
			if (tempDataLabelsResult == null)
			{
				tempDataLabelsResult = new Dictionary<int, string>();
			}
			return tempDataLabelsResult;
		}
	}

	internal Arrow BeginArrow
	{
		get
		{
			return m_beginArrow;
		}
		set
		{
			m_beginArrow = value;
		}
	}

	internal Arrow EndArrow
	{
		get
		{
			return m_endArrow;
		}
		set
		{
			m_endArrow = value;
		}
	}

	internal List<Arrow> BeginArrows
	{
		get
		{
			return m_beginArrows;
		}
		set
		{
			if (m_beginArrows == null)
			{
				m_beginArrows = new List<Arrow>();
			}
			m_beginArrows = value;
		}
	}

	internal List<Arrow> EndArrows
	{
		get
		{
			return m_endArrows;
		}
		set
		{
			if (m_endArrows == null)
			{
				m_endArrows = new List<Arrow>();
			}
			m_endArrows = value;
		}
	}

	public event EventHandler AppearanceChanged;

	internal event ListChangedEventHandler DataChanged;

	public event ChartPrepareStyleInfoHandler PrepareSeriesStyle
	{
		add
		{
			m_prepareSeriesStyleInfoHandler = (ChartPrepareStyleInfoHandler)Delegate.Combine(m_prepareSeriesStyleInfoHandler, value);
			RaiseSeriesStylesImplChanged();
		}
		remove
		{
			m_prepareSeriesStyleInfoHandler = (ChartPrepareStyleInfoHandler)Delegate.Remove(m_prepareSeriesStyleInfoHandler, value);
			RaiseSeriesStylesImplChanged();
		}
	}

	public event ChartPrepareStyleInfoHandler PrepareStyle
	{
		add
		{
			m_prepareStyleInfoHandler = (ChartPrepareStyleInfoHandler)Delegate.Combine(m_prepareStyleInfoHandler, value);
			RaiseSeriesStylesImplChanged();
		}
		remove
		{
			m_prepareStyleInfoHandler = (ChartPrepareStyleInfoHandler)Delegate.Remove(m_prepareStyleInfoHandler, value);
			RaiseSeriesStylesImplChanged();
		}
	}

	internal event EventHandler SeriesChanged;

	private bool ShouldSerializePoints()
	{
		if (m_seriesModel is ChartSeriesModel)
		{
			return m_seriesModel.Count > 0;
		}
		return false;
	}

	private bool ShouldSerializeSeriesModel()
	{
		return !(m_seriesModel is ChartSeriesModel);
	}

	public void ResetSeriesModel()
	{
		SeriesModel = OnCreateSeriesModel();
	}

	public ChartSeries()
		: this("")
	{
	}

	public ChartSeries(ChartControl chart)
		: this("")
	{
		m_chartControl = chart;
	}

	public ChartSeries(string name, ChartSeriesType type)
	{
		m_name = name;
		m_seriesConfigCollection = new ChartSeriesConfigCollection();
		m_seriesConfigCollection.Changed += OnConfigItemsChanged;
		m_pointFormats = new ChartPointFormatsRegistry(this);
		m_trendlines = new TrendlineCollection();
		m_trendlines.Changed += OnTrendlineItemsChanged;
		Type = type;
		Text = name;
	}

	public void DisposeChartSeries()
	{
		if (m_pointFormats != null)
		{
			m_pointFormats.Dispose();
		}
		if (m_renderer != null)
		{
			m_renderer.Dispose();
			m_renderer = null;
		}
		m_chartControl = null;
	}

	public void Dispose()
	{
		if (m_name != null)
		{
			m_name = null;
		}
		if (m_seriesConfigCollection != null)
		{
			m_seriesConfigCollection.Changed -= OnConfigItemsChanged;
			m_seriesConfigCollection = null;
		}
		if (m_trendlines != null)
		{
			m_trendlines.Changed -= OnTrendlineItemsChanged;
			m_trendlines = null;
		}
		if (m_pointFormats != null)
		{
			m_pointFormats = null;
		}
		if (m_trendlines != null)
		{
			m_trendlines = null;
		}
		if (tempDataLabelsResult != null)
		{
			tempDataLabelsResult.Clear();
			tempDataLabelsResult = null;
		}
	}

	public ChartSeries(string name)
		: this(name, ChartSeriesType.Column)
	{
	}

	public void UpdateSeriesModel(object dataSource)
	{
		if (SeriesModel is IUpdateModel)
		{
			(SeriesModel as IUpdateModel).UpdateDataSource(dataSource);
			OnSeriesModelImplChanged();
		}
	}

	public void UpdateCategoryModel(object dataSource)
	{
		if (CategoryModel is IUpdateModel)
		{
			(CategoryModel as IUpdateModel).UpdateDataSource(dataSource);
			OnSeriesCategoryModelImplChanged();
		}
	}

	public ChartStyleInfo GetOfflineStyle()
	{
		ChartStyleInfo offlineStyle = StylesImpl.ComposedStyles.GetOfflineStyle();
		if (m_model != null)
		{
			int seriesIndex = m_model.Series.IndexOf(this);
			OnPrepareSeriesStyleInfo(offlineStyle, seriesIndex);
		}
		return offlineStyle;
	}

	public ChartStyleInfo GetOfflineStyle(int index)
	{
		ChartStyleInfo offlineStyle = StylesImpl.ComposedStyles.GetOfflineStyle(index);
		if (Points[index].YValues.Length != 0)
		{
			OnPrepareStyleInfo(offlineStyle, index);
		}
		return offlineStyle;
	}

	internal SKPathEffect GetDashStyle(Pen pen, float width)
	{
		if (pen.DashStyle != 0)
		{
			pen.SKPaint.Style = SKPaintStyle.Stroke;
			switch (pen.DashStyle)
			{
			case DashStyle.Dot:
				pen.SKPaint.PathEffect = SKPathEffect.CreateDash(new float[2]
				{
					width * 2f,
					(pen.DashCap == DashCap.Round) ? (width * 2f) : width
				}, 0f);
				break;
			case DashStyle.CircleDot:
				pen.SKPaint.PathEffect = SKPathEffect.CreateDash(new float[2]
				{
					0f,
					width * 2f
				}, 0f);
				break;
			case DashStyle.Custom:
				pen.SKPaint.PathEffect = SKPathEffect.CreateDash(new float[2]
				{
					0f,
					width * 2f
				}, 0f);
				break;
			case DashStyle.Dash:
				pen.SKPaint.PathEffect = SKPathEffect.CreateDash(new float[2]
				{
					width * 2f,
					width * 2f
				}, 0f);
				break;
			case DashStyle.LongDash:
				pen.SKPaint.PathEffect = SKPathEffect.CreateDash(new float[2]
				{
					width * 6f,
					width * 4f
				}, 0f);
				break;
			case DashStyle.DashDot:
				pen.SKPaint.PathEffect = SKPathEffect.CreateDash(new float[4]
				{
					0f,
					width * 2f,
					width * 2f,
					width * 2f
				}, 0f);
				break;
			case DashStyle.LongDashDot:
				pen.SKPaint.PathEffect = SKPathEffect.CreateDash(new float[4]
				{
					0f,
					width * 4f,
					width * 6f,
					width * 4f
				}, 0f);
				break;
			case DashStyle.LongDashDotDot:
			{
				SKPathEffect pathEffect = SKPathEffect.CreateDash(new float[6]
				{
					width * 6f,
					width * 4f,
					0f,
					width * 4f,
					0f,
					width * 4f
				}, 0f);
				pen.SKPaint.PathEffect = pathEffect;
				break;
			}
			}
		}
		return pen.SKPaint.PathEffect;
	}

	internal SKStrokeCap GetCapStyle(Pen pen)
	{
		switch (pen.DashCap)
		{
		case DashCap.Flat:
			pen.SKPaint.StrokeCap = SKStrokeCap.Butt;
			break;
		case DashCap.Round:
			pen.SKPaint.StrokeCap = SKStrokeCap.Round;
			break;
		default:
			pen.SKPaint.StrokeCap = SKStrokeCap.Square;
			break;
		}
		return pen.SKPaint.StrokeCap;
	}

	public ChartBaseStylesMap GetStylesMap()
	{
		if (m_model != null)
		{
			return m_model.GetStylesMap();
		}
		return null;
	}

	internal void ResetLegend()
	{
		UpdateLegendItem();
	}

	public override string ToString()
	{
		return $"{base.ToString()} - [ {Name} ]";
	}

	public void AddAxis(ChartAxis axis, bool horizontal)
	{
		if (horizontal)
		{
			m_xAxis = axis;
		}
		else
		{
			m_yAxis = axis;
		}
	}

	internal void RemoveChartModel()
	{
		if (m_model != null)
		{
			m_model.ColorModel.Changed -= OnColorModelChanged;
		}
		m_model = null;
		StylesImpl.ComposedStyles.ResetCache();
	}

	private IChartSeriesSummary OnCreateSeriesSummaryImpl()
	{
		return new ChartSeriesSummary();
	}

	protected internal IEditableChartSeriesModel GetEditableData()
	{
		return SeriesModel as IEditableChartSeriesModel;
	}

	protected internal IChartEditableCategory GetCategoryEditableData()
	{
		return CategoryModel as IChartEditableCategory;
	}

	protected internal bool IsEditableData()
	{
		return SeriesModel is IEditableChartSeriesModel;
	}

	protected internal virtual void OnAppearanceChanged(EventArgs e)
	{
		m_needUpdateLegend = true;
		UpdateRenderer(ChartUpdateFlags.All);
		if (this.AppearanceChanged != null)
		{
			this.AppearanceChanged(this, e);
		}
	}

	protected virtual void OnChartStyleChanged(object sender, ChartStyleChangedEventArgs args)
	{
		InvalidateStyles();
	}

	protected virtual IChartSeriesModel OnCreateSeriesModel()
	{
		return new ChartSeriesModel();
	}

	protected virtual IChartSeriesCategory OnCreateGategorySeriesModel()
	{
		if (SeriesModel is ChartSeriesModel)
		{
			return (ChartSeriesModel)SeriesModel;
		}
		return new ChartSeriesModel();
	}

	protected virtual IChartSeriesStylesModel OnCreateSeriesStylesModelImpl()
	{
		ChartSeriesStylesModel chartSeriesStylesModel = new ChartSeriesStylesModel(this);
		((IChartSeriesStylesModel)chartSeriesStylesModel).Changed += OnChartStyleChanged;
		return chartSeriesStylesModel;
	}

	protected virtual void OnPrepareSeriesStyleInfo(ChartStyleInfo styleInfo, int seriesIndex)
	{
		if (seriesIndex > -1 && m_prepareSeriesStyleInfoHandler != null)
		{
			m_prepareSeriesStyleInfoHandler(this, new ChartPrepareStyleInfoEventArgs(styleInfo, seriesIndex));
		}
		if (styleInfo.Interior == null)
		{
			styleInfo.Interior = new BrushInfo(m_model.ColorModel.GetColor(seriesIndex));
		}
	}

	protected virtual void OnPrepareStyleInfo(ChartStyleInfo styleInfo, int index)
	{
		if (m_prepareStyleInfoHandler != null)
		{
			m_prepareStyleInfoHandler(this, new ChartPrepareStyleInfoEventArgs(styleInfo, index));
		}
		if (BaseType != 0)
		{
			int num = m_model.Series.IndexOf(this);
			if (num != -1 && styleInfo.Interior == null)
			{
				styleInfo.Interior = new BrushInfo(m_model.ColorModel.GetColor(num));
			}
		}
		else if (styleInfo.Interior == null)
		{
			styleInfo.Interior = new BrushInfo(m_model.ColorModel.GetColor(index));
		}
		if (styleInfo.Text == string.Empty)
		{
			if (styleInfo.TextFormat != string.Empty)
			{
				styleInfo.Text = string.Format(styleInfo.TextFormat, Points[index].YValues[0]);
			}
			else
			{
				styleInfo.Text = Points[index].YValues[0].ToString();
			}
		}
		if (styleInfo.ToolTip == string.Empty)
		{
			if (styleInfo.ToolTipFormat != string.Empty)
			{
				styleInfo.ToolTip = string.Format(styleInfo.ToolTipFormat, Points[index].YValues[0]);
			}
			else
			{
				styleInfo.ToolTip = Points[index].YValues[0].ToString();
			}
		}
	}

	protected virtual void OnSeriesChanged(EventArgs e)
	{
		m_needUpdateLegend = true;
		UpdateRenderer(ChartUpdateFlags.Data);
		if (this.SeriesChanged != null)
		{
			this.SeriesChanged(this, e);
		}
	}

	protected void OnSeriesModelImplChanged()
	{
		Summary.ModelImpl = m_seriesModelAdapter;
		OnSeriesModelChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
		RaiseModelImplChanged();
	}

	protected void OnSeriesCategoryModelImplChanged()
	{
		Summary.CategoryModel = m_seriesCategoryModelAdapter;
		OnSeriesModelChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
		RaiseModelImplChanged();
	}

	protected void OnSeriesModelImplChanging()
	{
	}

	protected virtual void OnSeriesStylesImplChanged()
	{
		m_stylesIndexer = new ChartStyleInfoIndexer(m_seriesStylesModel);
		WireStylesEvents();
		RaiseSeriesStylesImplChanged();
	}

	protected virtual void OnSeriesStylesImplChanging()
	{
		UnwireStylesEvents();
	}

	protected virtual void OnSeriesSummaryImplChanged()
	{
	}

	protected virtual void OnSeriesSummaryImplChanging()
	{
	}

	private void OnColorModelChanged(object sender, EventArgs e)
	{
		StylesImpl.ComposedStyles.ResetCache();
		UpdateRenderer(ChartUpdateFlags.Styles);
		UpdateLegendItem();
	}

	protected void RaiseModelImplChanged()
	{
		if (this.DataChanged != null)
		{
			this.DataChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
	}

	protected void RaiseSeriesStylesImplChanged()
	{
		InvalidateStyles();
	}

	private void SetSummaryImpl(IChartSeriesSummary summaryImpl)
	{
		if (m_seriesSummary != summaryImpl)
		{
			OnSeriesSummaryImplChanging();
			m_seriesSummary = summaryImpl;
			m_seriesSummary.ModelImpl = m_seriesModelAdapter;
			OnSeriesSummaryImplChanged();
		}
	}

	protected void SetSeriesModel(IChartSeriesModel seriesModel)
	{
		if (m_seriesModel != seriesModel)
		{
			OnSeriesModelImplChanging();
			if (m_seriesModel != null)
			{
				m_seriesModel.Changed -= OnSeriesModelChanged;
			}
			m_seriesModel = seriesModel;
			if (m_seriesModel != null)
			{
				m_seriesModel.Changed += OnSeriesModelChanged;
			}
			OnSeriesModelImplChanged();
		}
	}

	protected void SetCategorySeriesModel(IChartSeriesCategory seriesCategoryModel)
	{
		if (m_seriesCategoryModel != seriesCategoryModel)
		{
			OnSeriesModelImplChanging();
			if (m_seriesCategoryModel != null)
			{
				m_seriesCategoryModel.Changed -= OnSeriesModelChanged;
			}
			m_seriesCategoryModel = seriesCategoryModel;
			if (m_seriesCategoryModel != null)
			{
				m_seriesCategoryModel.Changed += OnSeriesModelChanged;
			}
			OnSeriesCategoryModelImplChanged();
		}
	}

	private void SetSeriesModelAdapter(IChartSeriesModel seriesModelAdapter)
	{
		if (m_seriesModelAdapter != seriesModelAdapter)
		{
			OnSeriesModelImplChanging();
			if (m_seriesModelAdapter != null)
			{
				m_seriesModelAdapter.Changed -= OnSeriesModelChanged;
			}
			m_seriesModelAdapter = seriesModelAdapter;
			m_seriesModelAdapter = seriesModelAdapter;
			if (m_pointsIndexer != null)
			{
				m_pointsIndexer.SeriesModel = seriesModelAdapter;
			}
			if (m_seriesModelAdapter != null)
			{
				m_seriesModelAdapter.Changed += OnSeriesModelChanged;
			}
			OnSeriesModelImplChanged();
		}
	}

	private void SetSeriesGategoryModelAdapter(IChartSeriesCategory seriesModelAdapter)
	{
		if (m_seriesCategoryModelAdapter != seriesModelAdapter)
		{
			OnSeriesModelImplChanging();
			if (m_seriesCategoryModelAdapter != null)
			{
				m_seriesCategoryModelAdapter.Changed -= OnSeriesModelChanged;
			}
			m_seriesCategoryModelAdapter = seriesModelAdapter;
			m_seriesCategoryModelAdapter = seriesModelAdapter;
			if (m_pointsIndexer != null)
			{
				m_pointsIndexer.SeriesCategoryModel = seriesModelAdapter;
			}
			if (m_seriesCategoryModelAdapter != null)
			{
				m_seriesCategoryModelAdapter.Changed += OnSeriesModelChanged;
			}
			OnSeriesModelImplChanged();
		}
	}

	protected void SetStylesImpl(IChartSeriesStylesModel seriesStylesModel)
	{
		if (m_seriesStylesModel != seriesStylesModel)
		{
			OnSeriesStylesImplChanging();
			if (m_seriesStylesModel != null && m_seriesStylesModel.Style != null)
			{
				m_seriesStylesModel.Style.Changed -= Style_Changed;
			}
			m_seriesStylesModel = seriesStylesModel;
			if (m_seriesStylesModel != null && m_seriesStylesModel.Style != null)
			{
				m_seriesStylesModel.Style.Changed += Style_Changed;
			}
			OnSeriesStylesImplChanged();
		}
	}

	protected void UnwireStylesEvents()
	{
		if (m_seriesStylesModel != null)
		{
			m_seriesStylesModel.Changed -= OnChartStyleChanged;
		}
	}

	protected void WireStylesEvents()
	{
		m_seriesStylesModel.Changed += OnChartStyleChanged;
	}

	private void OnConfigItemsChanged(object sender, EventArgs args)
	{
		UpdateRenderer(ChartUpdateFlags.Config);
		OnAppearanceChanged(EventArgs.Empty);
	}

	private void OnTrendlineItemsChanged(object sender, EventArgs args)
	{
		OnSeriesChanged(args);
	}

	private void Style_Changed(object sender, StyleChangedEventArgs e)
	{
		InvalidateStyles();
		OnAppearanceChanged(EventArgs.Empty);
	}

	private void OnSeriesModelChanged(object sender, ListChangedEventArgs args)
	{
		m_renderer.Update(ChartUpdateFlags.Data);
		m_renderer.DataUpdate(args);
		if (this.DataChanged != null)
		{
			this.DataChanged(this, args);
		}
	}

	private void UpdateLegendItem()
	{
		if (m_legendItem == null)
		{
			return;
		}
		m_legendItem.Refresh(m_legendItemUseSeriesStyle);
		if (BaseType == ChartSeriesBaseType.Single && m_type != ChartSeriesType.HeatMap)
		{
			m_legendItem.Children.Clear();
			for (int i = 0; i < Points.Count; i++)
			{
				ChartSeriesLegendItem chartSeriesLegendItem = new ChartSeriesLegendItem(this, i);
				chartSeriesLegendItem.Refresh(m_legendItemUseSeriesStyle);
				m_legendItem.Children.Add(chartSeriesLegendItem);
			}
		}
		else
		{
			m_legendItem.Children.Clear();
		}
	}

	internal void UpdateRenderer(ChartUpdateFlags flags)
	{
		if (m_renderer != null)
		{
			m_renderer.Update(flags);
		}
	}

	private void InvalidateStyles()
	{
		StylesImpl.ComposedStyles.ResetCache();
		UpdateRenderer(ChartUpdateFlags.Styles);
		UpdateLegendItem();
	}
}
