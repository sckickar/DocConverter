using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class ChartArea : IDisposable, IChartArea
{
	private const float c_maxScale = 1.5f;

	private const float c_minScale = 0.5f;

	private const float c_max2DRotate = 90f;

	private const float c_max3DRotate = 360f;

	internal const double CircularChartOffset = Math.PI / 2.0;

	private bool m_updateClientBounds;

	private ChartStyleInfo m_seriesStyle = new ChartStyleInfo();

	private bool m_boundsByAxis = true;

	private readonly IChartAreaHost m_chart;

	private ChartAxisCollection m_axes;

	private SizeF m_axisSpacing = new SizeF(2f, 2f);

	private ChartAxesInfoBar m_chartAxesLabelInfoBar;

	private ChartThickness m_axesThickness = new ChartThickness(0f);

	private Color m_defaultColor = Color.Red;

	private Rectangle bounds;

	private bool m_needRedraw = true;

	private bool m_hidePartialLabels;

	private ChartAxis xAxis;

	private ChartAxis yAxis;

	private bool m_requireAxes = true;

	private bool m_changeAppearance = true;

	private bool m_requireInvertedAxes;

	private ChartSetMode m_adjustPlotAreaMargins = ChartSetMode.AutoSet;

	private bool m_series3D;

	private bool m_leaveTree;

	private bool m_autoScale;

	private float m_depth = 50f;

	private float m_rotation = 30f;

	private float m_tilt = 30f;

	private float m_turn;

	private ChartMargins areaMargins = new ChartMargins(10, 10, 10, 10);

	private ChartMargins m_chartPlotAreaMargins = new ChartMargins(10, 10, 10, 10);

	private string m_chartAreaTooltip = string.Empty;

	private ChartCustomPointCollection m_customPoints;

	private BorderStyle m_borderStyle;

	private BrushInfo m_backInterior;

	private BrushInfo m_gridInterior;

	private BrushInfo m_gridHorzInterior;

	private BrushInfo m_gridVertInterior;

	private DocGen.Drawing.Image m_backImage;

	private DocGen.Drawing.Image m_interiorBackImage;

	private bool m_realSeries3D;

	private float m_scale3DCoeficient = 1f;

	private Vector3D m_rotateCenter = Vector3D.Empty;

	private Matrix3D m_viewMatrix3D = Matrix3D.Identity;

	private Transform3D m_transform3D = new Transform3D();

	private Graphics3DState m_3dSettings = new Graphics3DState();

	private Color m_borderColor = SystemColors.ControlText;

	private int m_borderWidth = 1;

	private BspNode rootNode;

	private bool m_divideArea;

	private bool m_multiplePies;

	private bool reDrawAxes;

	private double m_fullStackMax = 100.0;

	private ChartAxesLayoutMode m_xAxesLayoutMode = ChartAxesLayoutMode.Stacking;

	private ChartAxesLayoutMode m_yAxesLayoutMode = ChartAxesLayoutMode.Stacking;

	private SizeF m_minSize = new SizeF(200f, 100f);

	private Rectangle m_bounds = Rectangle.Empty;

	private Rectangle m_clientRectangle = Rectangle.Empty;

	private RectangleF m_renderbounds = RectangleF.Empty;

	private Rectangle m_globalRenderbounds = Rectangle.Empty;

	private readonly ChartWatermark m_watermark;

	private ChartSeriesParameters m_seriesParameters;

	private bool m_isIndexed;

	private bool m_isAllowGap = true;

	private ChartAxisLayoutCollection m_xLayouts;

	private ChartAxisLayoutCollection m_yLayouts;

	private int m_largeValueAxisLabelLength;

	private float m_largeValueAxisLabelFontSize;

	private bool m_isAxisVisible;

	private double m_displayUnits;

	private Graphics m_graphics;

	internal bool IsAxisVisible
	{
		get
		{
			return m_isAxisVisible;
		}
		set
		{
			m_isAxisVisible = value;
		}
	}

	internal int LargeValueAxisLabelLength
	{
		get
		{
			return m_largeValueAxisLabelLength;
		}
		set
		{
			m_largeValueAxisLabelLength = value;
		}
	}

	internal float LargeValueAxisLabelFontSize
	{
		get
		{
			return m_largeValueAxisLabelFontSize;
		}
		set
		{
			m_largeValueAxisLabelFontSize = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool UpdateClientBounds
	{
		get
		{
			return m_updateClientBounds;
		}
		set
		{
			m_updateClientBounds = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool IsIndexed
	{
		get
		{
			return m_isIndexed;
		}
		set
		{
			if (m_isIndexed == value)
			{
				return;
			}
			m_isIndexed = value;
			if (value)
			{
				m_chart.Series.RaiseResetEvent();
			}
			foreach (ChartSeries item in m_chart.Series)
			{
				item.UpdateRenderer(ChartUpdateFlags.Indexed);
			}
			if (!value)
			{
				m_chart.Series.RaiseResetEvent();
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool IsAllowGap
	{
		get
		{
			return m_isAllowGap;
		}
		set
		{
			if (m_isAllowGap == value)
			{
				return;
			}
			m_isAllowGap = value;
			m_chart.Series.RaiseResetEvent();
			foreach (ChartSeries item in m_chart.Series)
			{
				item.UpdateRenderer(ChartUpdateFlags.Indexed);
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public IChartAreaHost Chart => m_chart;

	[DefaultValue(1)]
	[Description("Specifies the border width of the ChartArea.")]
	public int BorderWidth
	{
		get
		{
			return m_borderWidth;
		}
		set
		{
			if (m_borderWidth != value)
			{
				m_borderWidth = value;
			}
		}
	}

	[DefaultValue(typeof(Color), "ControlText")]
	[Description("Specifies the bordercolor of the ChartArea.")]
	public Color BorderColor
	{
		get
		{
			return m_borderColor;
		}
		set
		{
			if (m_borderColor != value)
			{
				m_borderColor = value;
			}
		}
	}

	[DefaultValue(BorderStyle.None)]
	[Description("Specifies the style of the border that is to be rendered around the ChartArea.")]
	public BorderStyle BorderStyle
	{
		get
		{
			return m_borderStyle;
		}
		set
		{
			if (m_borderStyle != value)
			{
				m_borderStyle = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public int Width
	{
		get
		{
			return m_clientRectangle.Width;
		}
		set
		{
			if (m_bounds.Width != value)
			{
				m_bounds.Width = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public int Height
	{
		get
		{
			return m_bounds.Height;
		}
		set
		{
			if (m_bounds.Height != value)
			{
				m_bounds.Height = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public int Top => m_bounds.Top;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public int Right => m_bounds.Right;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public int Left => m_bounds.Left;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public int Bottom => m_bounds.Bottom;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public Rectangle Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			if (m_bounds != value)
			{
				m_bounds = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public Rectangle ClientRectangle
	{
		get
		{
			return m_clientRectangle;
		}
		set
		{
			if (m_clientRectangle != value)
			{
				m_clientRectangle = value;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Size Size
	{
		get
		{
			return m_clientRectangle.Size;
		}
		set
		{
			if (m_bounds.Size != value)
			{
				m_bounds.Size = value;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Point Location
	{
		get
		{
			return m_clientRectangle.Location;
		}
		set
		{
			if (m_bounds.Location != value)
			{
				m_bounds.Location = value;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public float Radius => GetRadarRadius(xAxis.Font.GetHeight());

	[DefaultValue(false)]
	[Description("Indicates whether area should be divided for each simple chart (Pie, Funnel...)")]
	public bool DivideArea
	{
		get
		{
			return m_divideArea;
		}
		set
		{
			if (m_divideArea != value)
			{
				m_divideArea = value;
			}
		}
	}

	[DefaultValue(false)]
	[Description("If set to true, multiple pie chart series will be rendered in the same chart area.")]
	public bool MultiplePies
	{
		get
		{
			return m_multiplePies;
		}
		set
		{
			if (m_multiplePies != value)
			{
				m_multiplePies = value;
			}
		}
	}

	[DefaultValue(false)]
	[Description("If set to true, Chart axes labels will be rendered each time chart updates")]
	public bool ReDrawAxes
	{
		get
		{
			return reDrawAxes;
		}
		set
		{
			if (reDrawAxes != value)
			{
				reDrawAxes = value;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public PointF Center => ChartMath.GetCenter(RenderBounds);

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Rectangle RenderGlobalBounds
	{
		get
		{
			Rectangle clientRectangle = ClientRectangle;
			if (m_boundsByAxis && AxesType == ChartAreaAxesType.Rectangular)
			{
				float num = float.MaxValue;
				float num2 = float.MaxValue;
				float num3 = float.MinValue;
				float num4 = float.MinValue;
				foreach (ChartAxis axis in Axes)
				{
					if (axis.Orientation == ChartOrientation.Horizontal)
					{
						num = Math.Min(axis.Rect.Left, num);
						num3 = Math.Max(axis.Rect.Right, num3);
					}
					else
					{
						num2 = Math.Min(axis.Rect.Top, num2);
						num4 = Math.Max(axis.Rect.Bottom, num4);
					}
				}
				return Rectangle.FromLTRB((int)num, (int)(num2 - OffsetY), (int)(num3 + OffsetX), (int)num4);
			}
			return GetGlobalBoundsByRect(clientRectangle);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Rectangle RenderBounds
	{
		get
		{
			Rectangle rectangle = RenderGlobalBounds;
			if (DrawingMode == DrawingMode.Pseudo3D)
			{
				rectangle = GetBoundsByRect(rectangle);
			}
			return rectangle;
		}
	}

	[Description("Indicates the margins that will be deduced from the ChartArea`s representation rectangle.")]
	[Category("Appearance")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ChartMargins ChartAreaMargins
	{
		get
		{
			if (areaMargins == null)
			{
				areaMargins = new ChartMargins(10, 10, 10, 10);
				areaMargins.Changed += OnChangingRedraw;
			}
			return areaMargins;
		}
		set
		{
			if (areaMargins != value)
			{
				areaMargins.Changed -= OnChangingRedraw;
				areaMargins = value;
				areaMargins.Changed += OnChangingRedraw;
				OnChangingRedraw(this, EventArgs.Empty);
			}
		}
	}

	[Browsable(false)]
	public float OffsetX
	{
		get
		{
			if (DrawingMode == DrawingMode.Pseudo3D && m_requireAxes)
			{
				return (float)((double)m_depth * Math.Sin((double)m_rotation * (Math.PI / 180.0)));
			}
			return 0f;
		}
	}

	[Browsable(false)]
	public float OffsetY
	{
		get
		{
			if (DrawingMode == DrawingMode.Pseudo3D && m_requireAxes)
			{
				return (float)((double)m_depth * Math.Sin((double)m_tilt * (Math.PI / 180.0)));
			}
			return 0f;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Series3D
	{
		get
		{
			return m_series3D;
		}
		set
		{
			if (m_series3D != value)
			{
				m_series3D = value;
				m_leaveTree = m_leaveTree && m_realSeries3D && m_series3D;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool RealSeries3D
	{
		get
		{
			return m_realSeries3D;
		}
		set
		{
			if (m_realSeries3D != value)
			{
				m_realSeries3D = value;
				if (!m_realSeries3D)
				{
					Rotation = Rotation;
					Tilt = Tilt;
					Turn = Turn;
				}
				m_leaveTree = m_leaveTree && m_realSeries3D && m_series3D;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public float Depth
	{
		get
		{
			return m_depth;
		}
		set
		{
			if (m_depth != value)
			{
				m_depth = ((value == 0f) ? 1f : value);
				if (m_series3D)
				{
					CalculateAxesSizes();
				}
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public float Rotation
	{
		get
		{
			return m_rotation;
		}
		set
		{
			value = RoundRotation(value);
			if (m_rotation != value)
			{
				m_rotation = value;
				if (m_series3D)
				{
					CalculateAxesSizes();
				}
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public float Tilt
	{
		get
		{
			return m_tilt;
		}
		set
		{
			if (Chart.Series.Count > 0 && (Chart.Series[0].Type != ChartSeriesType.Pie || MultiplePies))
			{
				value = RoundRotation(value);
			}
			if (m_tilt != value)
			{
				m_tilt = value;
				if (m_series3D)
				{
					CalculateAxesSizes();
				}
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public float Turn
	{
		get
		{
			return m_turn;
		}
		set
		{
			value = RoundRotation(value);
			if (m_turn != value)
			{
				m_turn = value;
				if (m_series3D)
				{
					CalculateAxesSizes();
				}
			}
		}
	}

	[DefaultValue(false)]
	[Description("Indicates whether area should scale automatically in 3D mode")]
	public bool AutoScale
	{
		get
		{
			return m_autoScale;
		}
		set
		{
			if (m_autoScale != value)
			{
				m_autoScale = value;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public float Scale3DCoeficient
	{
		get
		{
			return m_scale3DCoeficient;
		}
		set
		{
			value = ChartMath.MinMax(value, 0.5f, 1.5f);
			if (m_scale3DCoeficient != value)
			{
				m_scale3DCoeficient = value;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Transform3D Transform3D => m_transform3D;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Graphics3DState Settings3D => m_3dSettings;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
	[Description("Specifies the background brush of the chart area.")]
	public BrushInfo BackInterior
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

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public BrushInfo GridBackInterior
	{
		get
		{
			return m_gridInterior;
		}
		set
		{
			if (m_gridInterior != value)
			{
				m_gridInterior = value;
			}
		}
	}

	internal BrushInfo GridHorizontalInterior
	{
		get
		{
			return m_gridHorzInterior;
		}
		set
		{
			if (m_gridHorzInterior != value)
			{
				m_gridHorzInterior = value;
			}
		}
	}

	internal BrushInfo GridVerticalInterior
	{
		get
		{
			return m_gridVertInterior;
		}
		set
		{
			if (m_gridVertInterior != value)
			{
				m_gridVertInterior = value;
			}
		}
	}

	[DefaultValue(null)]
	[Description("Specifies the image that is to be used as the background for this ChartArea.")]
	public DocGen.Drawing.Image BackImage
	{
		get
		{
			return m_backImage;
		}
		set
		{
			if (m_backImage != value)
			{
				m_backImage = value;
			}
		}
	}

	[DefaultValue(null)]
	[Description("Specifies the image that is to be used as the background for this ChartArea interior.")]
	public DocGen.Drawing.Image InteriorBackImage
	{
		get
		{
			return m_interiorBackImage;
		}
		set
		{
			if (m_interiorBackImage != value)
			{
				m_interiorBackImage = value;
			}
		}
	}

	[DefaultValue(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool RequireAxes
	{
		get
		{
			return m_requireAxes;
		}
		set
		{
			m_requireAxes = value;
		}
	}

	[DefaultValue(true)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool LegacyAppearance
	{
		get
		{
			return m_changeAppearance;
		}
		set
		{
			m_changeAppearance = value;
			DoAppearanceChange();
		}
	}

	[DefaultValue(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public bool RequireInvertedAxes
	{
		get
		{
			return m_requireInvertedAxes;
		}
		set
		{
			m_requireInvertedAxes = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ChartAxisCollection Axes
	{
		get
		{
			if (m_axes == null)
			{
				m_axes = new ChartAxisCollection();
				m_axes.Changed += OnAxesChanged;
				xAxis = new ChartAxis(ChartOrientation.Horizontal);
				yAxis = new ChartAxis(ChartOrientation.Vertical);
				m_axes.AddRange(new ChartAxis[2] { xAxis, yAxis });
			}
			return m_axes;
		}
	}

	[Description("The spacing between different axes on the same side of the ChartArea.")]
	[Category("Axes")]
	public SizeF AxisSpacing
	{
		get
		{
			return m_axisSpacing;
		}
		set
		{
			if (m_axisSpacing != value)
			{
				m_axisSpacing = value;
			}
		}
	}

	[Description("The primary X axis.")]
	[Category("Axes")]
	public ChartAxis PrimaryXAxis
	{
		get
		{
			for (int i = 0; i < Axes.Count; i++)
			{
				if (m_axes[i].Orientation == ChartOrientation.Horizontal)
				{
					return xAxis = Axes[i];
				}
			}
			return null;
		}
	}

	[Description("The primary Y axis.")]
	[Category("Axes")]
	public ChartAxis PrimaryYAxis
	{
		get
		{
			for (int i = 0; i < Axes.Count; i++)
			{
				if (Axes[i].Orientation == ChartOrientation.Vertical)
				{
					return yAxis = Axes[i];
				}
			}
			return null;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public SizeF MinSize
	{
		get
		{
			return m_minSize;
		}
		set
		{
			m_minSize = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ChartMargins ChartPlotAreaMargins
	{
		get
		{
			if (m_chartPlotAreaMargins == null)
			{
				m_chartPlotAreaMargins = new ChartMargins();
				m_chartPlotAreaMargins.Changed += OnChangingRedraw;
			}
			return m_chartPlotAreaMargins;
		}
		set
		{
			if (m_chartPlotAreaMargins != value)
			{
				if (m_chartPlotAreaMargins != null)
				{
					m_chartPlotAreaMargins.Changed -= OnChangingRedraw;
				}
				m_chartPlotAreaMargins = value;
				if (m_chartPlotAreaMargins != null)
				{
					m_chartPlotAreaMargins.Changed += OnChangingRedraw;
				}
				OnChangingRedraw(this, EventArgs.Empty);
			}
		}
	}

	[DefaultValue(ChartSetMode.AutoSet)]
	[Description("Gets or sets the mode of drawing the edge labels. Default is AutoSet.")]
	public ChartSetMode AdjustPlotAreaMargins
	{
		get
		{
			return m_adjustPlotAreaMargins;
		}
		set
		{
			if (m_adjustPlotAreaMargins != value)
			{
				m_adjustPlotAreaMargins = value;
			}
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public ChartAxesInfoBar AxesInfoBar => m_chartAxesLabelInfoBar;

	[DefaultValue(100.0)]
	[Description("The maximal value of full stracking charts")]
	public double FullStackMax
	{
		get
		{
			return m_fullStackMax;
		}
		set
		{
			if (m_fullStackMax != value)
			{
				m_fullStackMax = value;
			}
		}
	}

	[DefaultValue(ChartAxesLayoutMode.Stacking)]
	[Description("Specifies the way in which multiple X-axes will be rendered. Default is Stacking.")]
	public ChartAxesLayoutMode XAxesLayoutMode
	{
		get
		{
			return m_xAxesLayoutMode;
		}
		set
		{
			if (m_xAxesLayoutMode != value)
			{
				m_xAxesLayoutMode = value;
			}
		}
	}

	[DefaultValue(ChartAxesLayoutMode.Stacking)]
	[Description("Specifies the way in which multiple Y-axes will be rendered. Default is Stacking.")]
	public ChartAxesLayoutMode YAxesLayoutMode
	{
		get
		{
			return m_yAxesLayoutMode;
		}
		set
		{
			if (m_yAxesLayoutMode != value)
			{
				m_yAxesLayoutMode = value;
			}
		}
	}

	[Description("Gets the X axes layouts")]
	public ChartAxisLayoutCollection XLayouts => m_xLayouts;

	[Description("Gets the Y axes layouts")]
	public ChartAxisLayoutCollection YLayouts => m_yLayouts;

	[Obsolete("This property isn't used anymore.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool NeedRedraw
	{
		get
		{
			return m_needRedraw;
		}
		set
		{
			m_needRedraw = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("Use ChartAxis.HidePartialLabels instead.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool HidePartialLabels
	{
		get
		{
			return m_hidePartialLabels;
		}
		set
		{
			m_hidePartialLabels = value;
			foreach (ChartAxis axis in Axes)
			{
				axis.HidePartialLabels = value;
			}
		}
	}

	[Obsolete("This property isn't used anymore.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Vector3D RotateCenter
	{
		get
		{
			return m_rotateCenter;
		}
		set
		{
			if (m_rotateCenter != value)
			{
				m_rotateCenter = value;
			}
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("Indicates whether area should be divided for each simple chart (Pie, Funnel...)")]
	[Obsolete("Use DivideArea property.")]
	public bool VisibleAllPies
	{
		get
		{
			return DivideArea;
		}
		set
		{
			DivideArea = value;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete("Use XAxesLayoutMode and YAxesLayoutMode instead")]
	public bool AxesSideBySide
	{
		get
		{
			if (m_xAxesLayoutMode != 0)
			{
				return m_yAxesLayoutMode == ChartAxesLayoutMode.SideBySide;
			}
			return true;
		}
		set
		{
			if (value)
			{
				XAxesLayoutMode = ChartAxesLayoutMode.SideBySide;
				YAxesLayoutMode = ChartAxesLayoutMode.SideBySide;
			}
			else
			{
				XAxesLayoutMode = ChartAxesLayoutMode.Stacking;
				YAxesLayoutMode = ChartAxesLayoutMode.Stacking;
			}
		}
	}

	[DefaultValue(TextRenderingHint.SystemDefault)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Description("Gets or sets the quality of text rendering. Default is AntiAlias.")]
	[Obsolete("This property isn't used anymore. Use Chart.TextRenderingHint property.")]
	public TextRenderingHint TextRenderingHint
	{
		get
		{
			return m_chart.TextRenderingHint;
		}
		set
		{
			m_chart.TextRenderingHint = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool BoundsByAxes
	{
		get
		{
			return m_boundsByAxis;
		}
		set
		{
			m_boundsByAxis = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("Represents the watermark information.")]
	public ChartWatermark Watermark => m_watermark;

	[DefaultValue("")]
	[Description("ToolTip text associated with this ChartArea")]
	public string ChartAreaToolTip
	{
		get
		{
			return m_chartAreaTooltip;
		}
		set
		{
			if (m_chartAreaTooltip != value)
			{
				m_chartAreaTooltip = value;
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ChartCustomPointCollection CustomPoints
	{
		get
		{
			if (m_customPoints == null)
			{
				m_customPoints = new ChartCustomPointCollection();
				m_customPoints.Changed += OnCustomPointsListChanged;
			}
			return m_customPoints;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ChartSeriesParameters SeriesParameters => m_seriesParameters;

	internal DrawingMode DrawingMode
	{
		get
		{
			if (!m_series3D)
			{
				return DrawingMode.Simple2D;
			}
			if (m_realSeries3D)
			{
				return DrawingMode.Real3D;
			}
			return DrawingMode.Pseudo3D;
		}
		set
		{
			switch (value)
			{
			case DrawingMode.Simple2D:
				m_series3D = false;
				m_realSeries3D = false;
				break;
			case DrawingMode.Pseudo3D:
				m_realSeries3D = false;
				m_series3D = true;
				break;
			case DrawingMode.Real3D:
				m_series3D = true;
				m_realSeries3D = true;
				break;
			}
		}
	}

	internal ChartAreaAxesType AxesType
	{
		get
		{
			if (m_requireAxes)
			{
				return ChartAreaAxesType.Rectangular;
			}
			if (m_chart.Polar || m_chart.Radar)
			{
				return ChartAreaAxesType.Circular;
			}
			return ChartAreaAxesType.None;
		}
	}

	private Matrix BackMatrix => new Matrix(RenderBounds, new PointF[3]
	{
		new PointF((float)RenderBounds.Left + OffsetX, (float)RenderBounds.Top - OffsetY),
		new PointF((float)RenderBounds.Right + OffsetX, (float)RenderBounds.Top - OffsetY),
		new PointF((float)RenderBounds.Left + OffsetX, (float)RenderBounds.Bottom - OffsetY)
	});

	private Matrix BottomMatrix
	{
		get
		{
			RectangleF rect = default(RectangleF);
			rect = (double.IsNaN(PrimaryXAxis.Crossing) ? new RectangleF(RenderBounds.Left, (float)RenderBounds.Bottom - OffsetY, RenderBounds.Width, OffsetY) : new RectangleF(RenderBounds.Left, (float)bounds.Y - OffsetY, RenderBounds.Width, OffsetY));
			return new Matrix(rect, new PointF[3]
			{
				new PointF(rect.Left + OffsetX, rect.Top),
				new PointF(rect.Right + OffsetX, rect.Top),
				new PointF(rect.Left, rect.Bottom)
			});
		}
	}

	private Matrix TopMatrix
	{
		get
		{
			RectangleF rect = default(RectangleF);
			rect = (double.IsNaN(PrimaryXAxis.Crossing) ? new RectangleF(RenderBounds.Left, (float)RenderBounds.Top - OffsetY, RenderBounds.Width, OffsetY) : new RectangleF(RenderBounds.Left, (float)bounds.Y - OffsetY, RenderBounds.Width, OffsetY));
			return new Matrix(rect, new PointF[3]
			{
				new PointF(rect.Right + OffsetX, rect.Top),
				new PointF(rect.Left + OffsetX, rect.Top),
				new PointF(rect.Right, rect.Bottom)
			});
		}
	}

	private Matrix LeftMatrix
	{
		get
		{
			RectangleF rect = default(RectangleF);
			rect = (double.IsNaN(PrimaryYAxis.Crossing) ? new RectangleF(RenderBounds.Left, RenderBounds.Top, OffsetX, RenderBounds.Height) : new RectangleF(bounds.Left, RenderBounds.Top, OffsetX, RenderBounds.Height));
			return new Matrix(rect, new PointF[3]
			{
				new PointF(rect.Left, rect.Top),
				new PointF(rect.Right, rect.Top - OffsetY),
				new PointF(rect.Left, rect.Bottom)
			});
		}
	}

	private Matrix RightMatrix
	{
		get
		{
			RectangleF rect = default(RectangleF);
			rect = (double.IsNaN(PrimaryYAxis.Crossing) ? new RectangleF(RenderBounds.Right, RenderBounds.Top, OffsetX, RenderBounds.Height) : new RectangleF(bounds.Left, RenderBounds.Top, OffsetX, RenderBounds.Height));
			return new Matrix(rect, new PointF[3]
			{
				new PointF(rect.Left, rect.Top),
				new PointF(rect.Right, rect.Top - OffsetY),
				new PointF(rect.Left, rect.Bottom)
			});
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

	public ChartArea(IChartAreaHost chart)
	{
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		m_chart = chart;
		m_3dSettings.Changed += OnChangingRedraw;
		m_chartAxesLabelInfoBar = new ChartAxesInfoBar();
		m_chartAxesLabelInfoBar.Changed += OnChangingRedraw;
		m_watermark = new ChartWatermark(this);
		m_seriesParameters = new ChartSeriesParameters(this);
		m_xLayouts = new ChartAxisLayoutCollection(this);
		m_yLayouts = new ChartAxisLayoutCollection(this);
	}

	public void Draw(PaintEventArgs e)
	{
		Draw(e, ChartPaintFlags.All);
	}

	public void Draw(PaintEventArgs e, ChartPaintFlags flags)
	{
		if (ClientRectangle.Width <= 0 || ClientRectangle.Height <= 0 || RenderBounds.Width <= 0 || RenderBounds.Height <= 0)
		{
			return;
		}
		if (RequireAxes)
		{
			m_xLayouts.Validate(ChartOrientation.Horizontal);
			m_yLayouts.Validate(ChartOrientation.Vertical);
		}
		m_needRedraw = false;
		Graphics graphics = e.Graphics;
		graphics.TextRenderingHint = m_chart.TextRenderingHint;
		graphics.SmoothingMode = m_chart.SmoothingMode;
		if (IsPaintFlag(flags, ChartPaintFlags.Background))
		{
			BrushPaint.FillRectangle(graphics, RenderGlobalBounds, m_gridInterior);
			if (m_backImage != null)
			{
				graphics.DrawImage(m_backImage, m_clientRectangle);
			}
		}
		Rectangle clientRectangle = m_clientRectangle;
		clientRectangle.Width++;
		clientRectangle.Height++;
		graphics.SetClip(clientRectangle);
		if (DrawingMode == DrawingMode.Real3D)
		{
			Draw3D(e, flags);
		}
		else
		{
			Draw2D(e, flags);
		}
		graphics.ResetClip();
		if (IsPaintFlag(flags, ChartPaintFlags.Border))
		{
			DrawingHelper.DrawBorder(rect: new Rectangle(m_bounds.X, m_bounds.Y, m_bounds.Width - 1, m_bounds.Height - 1), g: graphics, color: m_borderColor, width: m_borderWidth, borderStyle: m_borderStyle);
		}
	}

	public RectangleF GetSeriesBounds(ChartSeries series)
	{
		if (m_divideArea && AxesType == ChartAreaAxesType.None)
		{
			int index = m_chart.Model.Series.VisibleList.IndexOf(series);
			int count = m_chart.Model.Series.VisibleList.Count;
			return RenderingHelper.GetBounds(index, count, RenderBounds);
		}
		return RenderBounds;
	}

	public ChartAxis GetXAxis(ChartSeries series)
	{
		if (series.XAxis != null)
		{
			return series.XAxis;
		}
		return PrimaryXAxis;
	}

	public ChartAxis GetYAxis(ChartSeries series)
	{
		if (series.YAxis != null)
		{
			return series.YAxis;
		}
		return PrimaryYAxis;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public void CalculateSizes(Rectangle rect, Graphics g)
	{
		m_bounds = rect;
		m_clientRectangle = rect;
		m_renderbounds = RenderBounds;
		m_graphics = g;
		if (m_borderStyle != 0 || m_borderWidth > 0)
		{
			m_clientRectangle.Inflate(-m_borderWidth, -m_borderWidth);
			m_clientRectangle.Width--;
			m_clientRectangle.Height--;
		}
		if (m_updateClientBounds)
		{
			CalculateLabelSizes(g, m_clientRectangle);
			CalculateAxesSizes();
		}
		CalculateLabelSizes(g, RenderBounds);
		CalculateAxesSizes();
		m_renderbounds = RenderBounds;
	}

	public ChartPoint GetValueByPoint(Point pt)
	{
		ChartPoint result = null;
		if (RequireAxes)
		{
			result = ((!RequireInvertedAxes) ? GetValueByPointNormal(xAxis, yAxis, CorrectionTo(pt)) : GetValueByPointInversed(xAxis, yAxis, CorrectionTo(pt)));
		}
		else if (m_chart.Radar)
		{
			result = GetValueByPointPolar(xAxis, yAxis, CorrectionTo(pt));
		}
		return result;
	}

	public Point GetPointByValue(ChartPoint cpt)
	{
		return CorrectionFrom(GetPointByValueInternal(xAxis, yAxis, cpt));
	}

	public Point GetPointByValue(ChartSeries series, ChartPoint cpt)
	{
		return CorrectionFrom(GetPointByValueInternalMulAxes(series.ActualXAxis, series.ActualYAxis, cpt));
	}

	public void DisposeSeriesStyle()
	{
		m_seriesStyle.Dispose();
	}

	public void Dispose()
	{
		if (m_axes != null)
		{
			m_axes.Changed -= OnAxesChanged;
			for (int num = m_axes.Count - 1; num >= 0; num--)
			{
				m_axes[num].Unsubscribe(this);
				m_axes[num].Dispose();
				m_axes.RemoveAt(num);
			}
			m_axes = null;
		}
		if (m_seriesStyle != null)
		{
			m_seriesStyle = null;
		}
		ChartPlotAreaMargins.Changed -= OnChangingRedraw;
		ChartAreaMargins.Changed -= OnChangingRedraw;
		m_3dSettings.Changed -= OnChangingRedraw;
		m_chartAxesLabelInfoBar.Changed -= OnChangingRedraw;
	}

	private void Draw2D(PaintEventArgs e, ChartPaintFlags flags)
	{
		Graphics graphics = e.Graphics;
		RectangleF clipBounds = graphics.ClipBounds;
		PixelOffsetMode pixelOffsetMode = graphics.PixelOffsetMode;
		ChartAreaAxesType axesType = AxesType;
		ChartGDIGraph chartGDIGraph = new ChartGDIGraph(e.Graphics);
		ChartStripLineZorder zOrder = ChartStripLineZorder.Behind;
		switch (axesType)
		{
		case ChartAreaAxesType.Rectangular:
			DrawGrid(chartGDIGraph);
			if (m_chartAxesLabelInfoBar.Visible)
			{
				m_chartAxesLabelInfoBar.Draw(graphics, PrimaryXAxis, PrimaryYAxis);
			}
			graphics.SetClip(Rectangle.Inflate(RenderGlobalBounds, 1, 1), CombineMode.Intersect);
			DrawStripLines(chartGDIGraph, zOrder);
			break;
		case ChartAreaAxesType.Circular:
			DrawRadarAxes(graphics, PrimaryXAxis, PrimaryYAxis);
			break;
		}
		if (m_watermark.ZOrder == ChartWaterMarkOrder.Behind)
		{
			if (m_series3D)
			{
				chartGDIGraph.PushTransform();
				chartGDIGraph.Transform = BackMatrix;
				m_watermark.Draw(chartGDIGraph, RenderBounds);
				chartGDIGraph.PopTransform();
			}
			else
			{
				m_watermark.Draw(chartGDIGraph, RenderBounds);
			}
		}
		bool flag = false;
		for (int i = 0; i < Chart.Series.Count; i++)
		{
			string text = Chart.Series[i].Type.ToString();
			if (text.Contains("Column") || text.Contains("Area") || text.Contains("Histogram") || text.Contains("WaterFall") || text.Contains("Pareto") || text.Contains("Bar") || text.Contains("Pie") || text.Contains("Radar"))
			{
				flag = true;
			}
		}
		if (!flag && axesType == ChartAreaAxesType.Rectangular && (flags & ChartPaintFlags.Axes) == ChartPaintFlags.Axes)
		{
			DrawCustomPoints(chartGDIGraph);
			graphics.PixelOffsetMode = pixelOffsetMode;
			graphics.SetClip(clipBounds);
			DrawAxes(graphics);
		}
		m_chart.Series.DrawSeries(graphics, m_chart);
		if (axesType == ChartAreaAxesType.Circular)
		{
			yAxis.DrawAxis(graphics, this);
		}
		if (!flag)
		{
			graphics.PixelOffsetMode = pixelOffsetMode;
			graphics.SetClip(clipBounds);
			foreach (ChartAxis axis in Axes)
			{
				if (axis.IsVisible)
				{
					axis.DrawTickLabels(graphics, this, RectangleF.Empty);
				}
			}
		}
		if (flag && axesType == ChartAreaAxesType.Rectangular && (flags & ChartPaintFlags.Axes) == ChartPaintFlags.Axes)
		{
			DrawCustomPoints(chartGDIGraph);
			graphics.PixelOffsetMode = pixelOffsetMode;
			graphics.SetClip(clipBounds);
			DrawAxes(graphics);
		}
		if (axesType == ChartAreaAxesType.Rectangular)
		{
			zOrder = ChartStripLineZorder.Over;
			DrawStripLines(chartGDIGraph, zOrder);
		}
		if (m_watermark.ZOrder == ChartWaterMarkOrder.Over)
		{
			m_watermark.Draw(chartGDIGraph, RenderBounds);
		}
	}

	private void DrawAxes(Graphics g)
	{
		foreach (ChartAxis axis in Axes)
		{
			if (axis.IsVisible)
			{
				axis.DrawAxis(g, this);
			}
			axis.DrawBreaks(g, m_renderbounds);
		}
	}

	private void DrawRadarAxes(Graphics g, ChartAxis xAxis, ChartAxis yAxis)
	{
		PointF center = Center;
		bool flag = PrimaryXAxis.TickLabelsDrawingMode != ChartAxisTickLabelDrawingMode.None;
		int numberOfIntervals = yAxis.Range.NumberOfIntervals;
		float radarRadius = GetRadarRadius(flag ? PrimaryXAxis.Font.GetHeight() : 0f);
		bool flag2 = m_chart.RadarStyle == ChartRadarAxisStyle.Polygon && !m_chart.Polar;
		if (radarRadius <= 0f)
		{
			return;
		}
		int num = ((xAxis.IsIndexed || xAxis.ValueType == ChartValueType.Category) ? m_chart.IndexValues.Count : xAxis.Range.NumberOfIntervals);
		GraphicsPath radarPath = GetRadarPath(center, radarRadius, flag2, num);
		BrushPaint.FillPath(g, radarPath, m_gridInterior);
		if (InteriorBackImage != null)
		{
			g.SetClip(radarPath);
			RectangleF rectangle = radarPath.GetBounds();
			g.DrawImage(InteriorBackImage, rectangle);
			g.ResetClip();
		}
		bool flag3 = m_chart.RadarStyle == ChartRadarAxisStyle.Circle && !m_chart.Polar;
		bool polar = m_chart.Polar;
		if (polar)
		{
			num = ((xAxis.ValueType == ChartValueType.Category) ? m_chart.IndexValues.Count : 12);
		}
		for (int i = 0; i <= num; i++)
		{
			double num2 = (double)i * (Math.PI * 2.0) / (double)num;
			double num3 = Math.PI / 2.0 + num2;
			float num4 = center.X + (float)Math.Cos(num3) * radarRadius;
			float num5 = center.Y - (float)Math.Sin(num3) * radarRadius;
			double num6 = ValueToPolarCoefficient(num - i);
			Pen pen = yAxis.LineType.Pen;
			if (xAxis.Pens != null && xAxis.Pens.Length != 0)
			{
				pen = GetRadarPen(xAxis, i);
				if (i == 0 || i == num)
				{
					pen = yAxis.LineType.Pen;
				}
			}
			g.DrawLine(pen, center, new PointF(num4, num5));
			if (!flag)
			{
				continue;
			}
			ChartAxisLabel chartAxisLabel = null;
			if (flag2 || flag3)
			{
				if (xAxis.IsIndexed || xAxis.ValueType == ChartValueType.Category)
				{
					if (xAxis.ValueType == ChartValueType.Category && i == num)
					{
						break;
					}
					chartAxisLabel = PrimaryXAxis.GenerateLabel(i, this, i);
				}
				else
				{
					double min = xAxis.Range.Min;
					double max = xAxis.Range.Max;
					double interval = PrimaryXAxis.Range.Interval;
					double num7 = min + interval * (double)i - xAxis.Offset;
					num7 = ((!xAxis.Inversed) ? (min + interval * (double)i - xAxis.Offset) : (max - interval * (double)i - xAxis.Offset));
					chartAxisLabel = PrimaryXAxis.GenerateLabel(num7, this, i);
				}
			}
			else
			{
				if (xAxis.ValueType == ChartValueType.Category && i == m_chart.IndexValues.Count)
				{
					break;
				}
				chartAxisLabel = PrimaryXAxis.GenerateLabel(num2, this, i);
				if ((polar && xAxis.RangeType == ChartAxisRangeType.Auto) || xAxis.IsIndexed)
				{
					if (xAxis.Inversed)
					{
						double value = Math.PI * 2.0 - Math.PI / 6.0 * (double)i;
						chartAxisLabel = PrimaryXAxis.GenerateLabel(value, this, i);
					}
					else
					{
						chartAxisLabel = PrimaryXAxis.GenerateLabel(num2, this, i);
					}
				}
				else if (polar)
				{
					double min2 = xAxis.Range.Min;
					double max2 = xAxis.Range.Max;
					_ = PrimaryXAxis.Range.Interval;
					double num8 = ((max2 > min2) ? (max2 - min2) : (min2 - max2));
					double num9 = num8 / 12.0 * (double)i;
					num9 = ((!xAxis.Inversed) ? (num8 / 12.0 * (double)i) : (max2 - num8 / 12.0 * (double)i));
					chartAxisLabel = PrimaryXAxis.GenerateLabel(num9, this, i);
					num6 = ValueToPolarCoefficient(i);
				}
			}
			float width = (float)(m_chart as ChartControl).Width / 4.16667f;
			float height = (float)(m_chart as ChartControl).Height / 6.16667f;
			SizeF sizeF = chartAxisLabel.Measure(g, new SizeF(width, height), PrimaryXAxis);
			SizeF sizeF2 = chartAxisLabel.Measure(g, width, PrimaryXAxis);
			float num10 = num4;
			float num11 = num5;
			if (num6 == 0.25)
			{
				num10 -= sizeF2.Width;
				num11 -= sizeF2.Height / 2f;
			}
			else if (num6 == 0.5)
			{
				num10 -= sizeF2.Width / 2f;
			}
			else if (num6 == 0.75)
			{
				num11 -= sizeF2.Height / 2f;
			}
			else if (num6 == 1.0 || num6 == 0.0)
			{
				num10 -= sizeF2.Width / 2f;
				num11 -= sizeF2.Height;
			}
			else if (0.0 < num6 && num6 < 0.25)
			{
				num10 -= sizeF2.Width;
				num11 -= sizeF2.Height;
			}
			else if (0.25 < num6 && num6 < 0.5)
			{
				num10 -= sizeF2.Width;
			}
			else if (0.75 < num6 && num6 < 1.0)
			{
				num11 -= sizeF2.Height;
			}
			if ((num6 > 0.125 && num6 < 0.25) || (num6 > 0.75 && num6 < 0.875))
			{
				num11 += 5f;
			}
			else if ((num6 > 0.625 && num6 < 0.75) || (num6 > 0.25 && num6 < 0.375))
			{
				num11 -= 5f;
			}
			if (num6 == 1.0 || num6 == 0.0)
			{
				num11 -= 10f;
			}
			else if (0.5 < num6 && num6 < 1.0)
			{
				num10 += 5f;
			}
			else if (0.0 < num6 && num6 < 0.5)
			{
				num10 -= 5f;
			}
			using Brush brush = new SolidBrush(PrimaryXAxis.ForeColor);
			g.DrawString(chartAxisLabel.Text, chartAxisLabel.Font, brush, new RectangleF(num10, num11, sizeF.Width + 1f, sizeF.Height));
		}
		for (int j = 1; j < numberOfIntervals + 1; j++)
		{
			float radius = (float)j * (radarRadius / (float)numberOfIntervals);
			Pen pen2 = yAxis.GridLineType.Pen;
			if (yAxis.Pens != null && yAxis.Pens.Length != 0)
			{
				pen2 = GetRadarPen(yAxis, j - 1);
				if (j == numberOfIntervals)
				{
					pen2 = xAxis.LineType.Pen;
				}
			}
			g.DrawPath(pen2, GetRadarPath(center, radius, flag2, num));
		}
		yAxis.DrawAxis(g, this);
	}

	public virtual double ValueToPolarCoefficient(double value)
	{
		double num = double.NaN;
		double num2 = xAxis.Range.Min - 1.0;
		double num3 = xAxis.Range.Delta - 1.0;
		num = (value - num2) / num3;
		num *= 1.0 - 1.0 / (num3 + 1.0);
		return ValueBasedOnAngle(num);
	}

	internal double ValueBasedOnAngle(double result)
	{
		result = 1.0 - result;
		result = ((result < 0.0) ? (result + 1.0) : result);
		result = ((result > 1.0) ? (result - 1.0) : result);
		return result;
	}

	private void DrawCustomPoints(ChartGraph graph)
	{
		foreach (ChartCustomPoint customPoint in CustomPoints)
		{
			customPoint.Draw(this, graph, GetCustomPointLocation(customPoint));
		}
	}

	internal Pen GetRadarPen(ChartAxis axis, int index)
	{
		Pen pen = null;
		pen = axis.Pens[index % axis.Pens.Length];
		if (pen == null)
		{
			pen = axis.LineType.Pen;
		}
		return pen;
	}

	private void DrawGrid(ChartGraph graph)
	{
		bounds = RenderBounds;
		Rectangle rectangle = bounds;
		RectangleF opposedBounds = RectangleF.Empty;
		if (m_series3D)
		{
			graph.PushTransform();
			graph.MultiplyTransform(BackMatrix);
			DrawSimpleGrid(graph, bounds, RectangleF.Empty, null, null, null);
			graph.PopTransform();
			RectangleF rectangleF;
			if (RequireInvertedAxes)
			{
				if (!double.IsNaN(PrimaryXAxis.Crossing))
				{
					PrimaryXAxis.Crossing = ((PrimaryXAxis.Crossing == double.MaxValue) ? PrimaryYAxis.Range.Max : ((PrimaryXAxis.Crossing == double.MinValue) ? PrimaryYAxis.Range.Min : PrimaryXAxis.Crossing));
					ChartPoint cpt = new ChartPoint(PrimaryXAxis.Crossing, PrimaryXAxis.Range.Min);
					Point pointByValue = GetPointByValue(cpt);
					bounds = new Rectangle(rectangle.X, pointByValue.Y, bounds.Width, bounds.Height);
					rectangleF = new RectangleF(bounds.Left, (float)bounds.Y - OffsetY, bounds.Width, OffsetY);
				}
				else
				{
					rectangleF = new RectangleF(bounds.Left, (float)bounds.Bottom - OffsetY, bounds.Width, OffsetY);
					opposedBounds = new RectangleF(bounds.Left, (float)bounds.Top - OffsetY, bounds.Width, OffsetY);
				}
			}
			else if (!double.IsNaN(PrimaryXAxis.Crossing))
			{
				PrimaryXAxis.Crossing = ((PrimaryXAxis.Crossing == double.MaxValue) ? PrimaryYAxis.Range.Max : ((PrimaryXAxis.Crossing == double.MinValue) ? PrimaryYAxis.Range.Min : PrimaryXAxis.Crossing));
				ChartPoint cpt2 = new ChartPoint(PrimaryXAxis.Range.Min, PrimaryXAxis.Crossing);
				Point pointByValue2 = GetPointByValue(cpt2);
				bounds = new Rectangle(rectangle.X, pointByValue2.Y, bounds.Width, bounds.Height);
				rectangleF = new RectangleF(bounds.Left, (float)bounds.Y - OffsetY, bounds.Width, OffsetY);
			}
			else
			{
				rectangleF = new RectangleF(bounds.Left, (float)bounds.Bottom - OffsetY, bounds.Width, OffsetY);
				opposedBounds = new RectangleF(bounds.Left, (float)bounds.Top - OffsetY, bounds.Width, OffsetY);
			}
			if (OffsetY > 0.5f)
			{
				graph.PushTransform();
				DrawSimpleGrid(graph, rectangleF, opposedBounds, BottomMatrix, TopMatrix, ChartOrientation.Horizontal);
				graph.PopTransform();
			}
			if (RequireInvertedAxes)
			{
				if (!double.IsNaN(PrimaryYAxis.Crossing))
				{
					PrimaryYAxis.Crossing = ((PrimaryYAxis.Crossing == double.MaxValue) ? PrimaryXAxis.Range.Max : ((PrimaryYAxis.Crossing == double.MinValue) ? PrimaryXAxis.Range.Min : PrimaryYAxis.Crossing));
					ChartPoint cpt3 = new ChartPoint(PrimaryYAxis.Range.Min, PrimaryYAxis.Crossing);
					bounds = new Rectangle(GetPointByValue(cpt3).X, rectangle.Y, bounds.Width, bounds.Height);
				}
			}
			else if (!double.IsNaN(PrimaryYAxis.Crossing))
			{
				PrimaryYAxis.Crossing = ((PrimaryYAxis.Crossing == double.MaxValue) ? PrimaryXAxis.Range.Max : ((PrimaryYAxis.Crossing == double.MinValue) ? PrimaryXAxis.Range.Min : PrimaryYAxis.Crossing));
				ChartPoint cpt4 = new ChartPoint(PrimaryYAxis.Crossing, PrimaryYAxis.Range.Min);
				bounds = new Rectangle(GetPointByValue(cpt4).X, rectangle.Y, bounds.Width, bounds.Height);
			}
			RectangleF rectangleF2 = new RectangleF(bounds.Left, rectangle.Top, OffsetX, bounds.Height);
			RectangleF opposedBounds2 = new RectangleF(bounds.Right, rectangle.Top, OffsetX, bounds.Height);
			if (OffsetX > 0.5f)
			{
				graph.PushTransform();
				DrawSimpleGrid(graph, rectangleF2, opposedBounds2, LeftMatrix, RightMatrix, ChartOrientation.Vertical);
				graph.PopTransform();
			}
		}
		else
		{
			DrawSimpleGrid(graph, bounds, RectangleF.Empty, null, null, null);
		}
	}

	private void DrawStripLines(ChartGraph graph, ChartStripLineZorder zOrder)
	{
		foreach (ChartAxis axis in Axes)
		{
			foreach (ChartStripLine stripLine in axis.StripLines)
			{
				if (stripLine.Enabled && stripLine.ZOrder == zOrder)
				{
					stripLine.Draw(graph, GetStripLineRects(axis, stripLine));
				}
			}
		}
	}

	private void DrawSimpleGrid(ChartGraph graph, RectangleF bounds, RectangleF opposedBounds, Matrix transform, Matrix opposedTransform, ChartOrientation? orientation)
	{
		BrushInfo brushInfo = m_gridInterior;
		if (orientation == ChartOrientation.Horizontal && GridHorizontalInterior != null)
		{
			brushInfo = GridHorizontalInterior;
		}
		else if (orientation == ChartOrientation.Vertical && GridVerticalInterior != null)
		{
			brushInfo = GridVerticalInterior;
		}
		if (transform != null)
		{
			graph.MultiplyTransform(transform);
		}
		graph.DrawRect(brushInfo, null, bounds);
		if (InteriorBackImage != null)
		{
			graph.DrawImage(InteriorBackImage, bounds);
		}
		foreach (ChartAxis axis in Axes)
		{
			if (!orientation.HasValue)
			{
				axis.DrawInterlacedGrid(graph, bounds);
			}
			else if (orientation.Value == axis.Orientation)
			{
				graph.PopTransform();
				graph.PushTransform();
				if (!axis.OpposedPosition || opposedBounds == RectangleF.Empty)
				{
					graph.MultiplyTransform(transform);
					axis.DrawInterlacedGrid(graph, bounds);
				}
				else
				{
					graph.MultiplyTransform(opposedTransform);
					axis.DrawInterlacedGrid(graph, bounds);
				}
				graph.PopTransform();
				graph.PushTransform();
			}
		}
		foreach (ChartAxis axis2 in Axes)
		{
			if (!orientation.HasValue)
			{
				axis2.DrawGridLines(graph, bounds);
			}
			else if (orientation.Value == axis2.Orientation)
			{
				graph.PopTransform();
				graph.PushTransform();
				if (!axis2.OpposedPosition || opposedBounds == RectangleF.Empty)
				{
					graph.MultiplyTransform(transform);
					axis2.DrawGridLines(graph, bounds);
				}
				else
				{
					graph.MultiplyTransform(opposedTransform);
					axis2.DrawGridLines(graph, opposedBounds);
				}
				graph.PopTransform();
				graph.PushTransform();
			}
		}
		graph.DrawRect(new Pen(m_borderColor, m_borderWidth), bounds);
	}

	private GraphicsPath GetRadarPath(PointF center, float radius, bool isRadar, int count)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		if (isRadar)
		{
			PointF[] array = new PointF[count];
			for (int i = 0; i < count; i++)
			{
				double num = (double)((float)i * 2f) * Math.PI / (double)count + Math.PI / 2.0;
				array[i] = new PointF(center.X + (float)((double)radius * Math.Cos(num)), center.Y - (float)((double)radius * Math.Sin(num)));
			}
			graphicsPath.AddPolygon(array);
		}
		else
		{
			graphicsPath.AddArc(center.X - radius, center.Y - radius, 2f * radius, 2f * radius, 0f, 360f);
		}
		return graphicsPath;
	}

	private RectangleF[] GetStripLineRects(ChartAxis axis, ChartStripLine stripLine)
	{
		MinMaxInfo visibleRange = axis.VisibleRange;
		Rectangle renderBounds = RenderBounds;
		double num = (stripLine.StartAtAxisPosition ? (axis.Range.Min + stripLine.Offset) : stripLine.Start);
		double num2 = (stripLine.StartAtAxisPosition ? axis.Range.Max : stripLine.End);
		ArrayList arrayList = new ArrayList();
		for (double num3 = num; stripLine.Period == 0.0 || (stripLine.Period > 0.0 && num3 < num2) || (stripLine.Period < 0.0 && num3 > num2); num3 += stripLine.Period)
		{
			double num4 = num3;
			double num5 = num3 + stripLine.Width;
			if (axis.IsIndexed)
			{
				num4 = m_chart.IndexValues.GetIndex(num4);
				num5 = m_chart.IndexValues.GetIndex(num5);
			}
			if (visibleRange.Contains(num4) || visibleRange.Contains(num5) || (visibleRange.Contains(num4) && stripLine.FixedWidth != 0.0))
			{
				float coordinateFromValue = axis.GetCoordinateFromValue(num4);
				float num6 = axis.GetCoordinateFromValue(num5);
				if (stripLine.FixedWidth != 0.0)
				{
					num6 = coordinateFromValue + (float)stripLine.FixedWidth;
				}
				if (axis.Orientation == ChartOrientation.Horizontal)
				{
					arrayList.Add(ChartMath.CorrectRect(coordinateFromValue, renderBounds.Top, num6, renderBounds.Bottom));
				}
				else
				{
					arrayList.Add(ChartMath.CorrectRect(renderBounds.Left, coordinateFromValue, renderBounds.Right, num6));
				}
			}
			if (stripLine.Period == 0.0)
			{
				break;
			}
		}
		return arrayList.ToArray(typeof(RectangleF)) as RectangleF[];
	}

	private PointF GetCustomPointLocation(ChartCustomPoint customPoint)
	{
		PointF result = PointF.Empty;
		switch (customPoint.CustomType)
		{
		case ChartCustomPointType.Percent:
			result = new PointF((float)((double)Width * customPoint.XValue / 100.0), (float)Height - (float)((double)Height * customPoint.YValue / 100.0));
			break;
		case ChartCustomPointType.Pixel:
			result = new PointF((float)customPoint.XValue, (float)customPoint.YValue);
			break;
		case ChartCustomPointType.ChartCoordinates:
			if (customPoint.SeriesIndex >= 0)
			{
				int seriesIndex2 = customPoint.SeriesIndex;
				ChartSeries series = m_chart.Series[seriesIndex2];
				ChartPoint cpt = new ChartPoint(customPoint.XValue, customPoint.YValue);
				result = GetPointByValue(series, cpt);
			}
			else
			{
				result = GetPointByValueInternal(xAxis, yAxis, new ChartPoint(customPoint.XValue, customPoint.YValue));
			}
			break;
		case ChartCustomPointType.PointFollow:
		{
			int seriesIndex = customPoint.SeriesIndex;
			int pointIndex = customPoint.PointIndex;
			if (seriesIndex >= 0 && seriesIndex < m_chart.Series.Count && pointIndex >= 0 && pointIndex < m_chart.Series[seriesIndex].Points.Count)
			{
				ChartSeries chartSeries = m_chart.Series[seriesIndex];
				result = ((chartSeries.BaseStackingType == ChartSeriesBaseStackingType.NotStacked) ? ((PointF)GetPointByValueInternal(chartSeries.XAxis, chartSeries.YAxis, chartSeries.Points[pointIndex])) : chartSeries.Renderer.GetStackedSymbolPoint(pointIndex));
			}
			break;
		}
		}
		return result;
	}

	private void Draw3D(PaintEventArgs e, ChartPaintFlags flags)
	{
		ChartAreaAxesType axesType = AxesType;
		Graphics3D graphics3D = new Graphics3D(e.Graphics);
		graphics3D.LoadState(m_3dSettings);
		graphics3D.Transform = UpdateTransform3D();
		Path3DCollect polygon = CreateWorkPlane(0f - m_depth);
		if (!m_leaveTree || rootNode == null)
		{
			ChartSeriesCollection series = m_chart.Series;
			ChartStripLineZorder zOrder = ChartStripLineZorder.Behind;
			switch (axesType)
			{
			case ChartAreaAxesType.Rectangular:
			{
				DrawAxes(graphics3D);
				DrawGrid(graphics3D);
				series.DrawSeriesNamesInDepth(graphics3D, m_chart);
				if (m_chartAxesLabelInfoBar.Visible)
				{
					m_chartAxesLabelInfoBar.Draw(graphics3D, PrimaryXAxis, PrimaryYAxis);
				}
				Rectangle globalBoundsByRect = GetGlobalBoundsByRect(m_clientRectangle);
				graphics3D.AddPolygon(new Polygon(new Vector3D[4]
				{
					new Vector3D(globalBoundsByRect.Left, globalBoundsByRect.Top, 0.0),
					new Vector3D(globalBoundsByRect.Left, globalBoundsByRect.Top, m_depth),
					new Vector3D(globalBoundsByRect.Left, globalBoundsByRect.Bottom, m_depth),
					new Vector3D(globalBoundsByRect.Left, globalBoundsByRect.Bottom, 0.0)
				}, clipPolygon: true));
				graphics3D.AddPolygon(new Polygon(new Vector3D[4]
				{
					new Vector3D(globalBoundsByRect.Left, globalBoundsByRect.Top, 0.0),
					new Vector3D(globalBoundsByRect.Right, globalBoundsByRect.Top, 0.0),
					new Vector3D(globalBoundsByRect.Right, globalBoundsByRect.Top, m_depth),
					new Vector3D(globalBoundsByRect.Left, globalBoundsByRect.Top, m_depth)
				}, clipPolygon: true));
				graphics3D.AddPolygon(new Polygon(new Vector3D[4]
				{
					new Vector3D(globalBoundsByRect.Left, globalBoundsByRect.Bottom, 0.0),
					new Vector3D(globalBoundsByRect.Left, globalBoundsByRect.Bottom, m_depth),
					new Vector3D(globalBoundsByRect.Right, globalBoundsByRect.Bottom, m_depth),
					new Vector3D(globalBoundsByRect.Right, globalBoundsByRect.Bottom, 0.0)
				}, clipPolygon: true));
				graphics3D.AddPolygon(new Polygon(new Vector3D[4]
				{
					new Vector3D(globalBoundsByRect.Right, globalBoundsByRect.Top, 0.0),
					new Vector3D(globalBoundsByRect.Right, globalBoundsByRect.Bottom, 0.0),
					new Vector3D(globalBoundsByRect.Right, globalBoundsByRect.Bottom, m_depth),
					new Vector3D(globalBoundsByRect.Right, globalBoundsByRect.Top, m_depth)
				}, clipPolygon: true));
				DrawStripLines(graphics3D, zOrder);
				break;
			}
			case ChartAreaAxesType.Circular:
				DrawRadarAxes(graphics3D, PrimaryXAxis, PrimaryYAxis);
				break;
			}
			series.DrawSeries(graphics3D, m_chart);
			if (axesType == ChartAreaAxesType.Circular)
			{
				yAxis.DrawAxis(graphics3D, this, -1f);
			}
			DrawCustomPoints(graphics3D);
			if (axesType == ChartAreaAxesType.Rectangular)
			{
				zOrder = ChartStripLineZorder.Over;
				DrawStripLines(graphics3D, zOrder);
			}
			if (m_watermark.ZOrder == ChartWaterMarkOrder.Over)
			{
				graphics3D.AddPolygon(m_watermark.Draw(graphics3D, RenderBounds, 0f));
			}
			else if (m_chart.Model.FirstSeries != null)
			{
				float totalDepth = m_chart.Model.FirstSeries.Renderer.GetTotalDepth();
				graphics3D.AddPolygon(m_watermark.Draw(graphics3D, RenderBounds, totalDepth + 1f));
			}
			else
			{
				graphics3D.AddPolygon(m_watermark.Draw(graphics3D, RenderBounds, m_depth));
			}
			graphics3D.AddPolygon(polygon);
			graphics3D.PrepareView();
			rootNode = null;
		}
		else
		{
			graphics3D.RootNode = rootNode;
		}
		graphics3D.View3D();
	}

	private void DrawGrid(Graphics3D g)
	{
		Rectangle renderGlobalBounds = RenderGlobalBounds;
		Path3DCollect path3DCollect = new Path3DCollect(Polygon.CreateRectangle(renderGlobalBounds, m_depth, m_gridInterior, null));
		foreach (ChartAxis axis in Axes)
		{
			path3DCollect.Add(axis.DrawInterlacedGrid(g, renderGlobalBounds, m_depth));
		}
		foreach (ChartAxis axis2 in Axes)
		{
			path3DCollect.Add(axis2.DrawGridLines(g, renderGlobalBounds, m_depth));
		}
		g.AddPolygon(path3DCollect);
		if (!(m_depth > 0f))
		{
			return;
		}
		Rectangle rectangle = new Rectangle(renderGlobalBounds.Left, -(int)m_depth, renderGlobalBounds.Width, (int)m_depth);
		Rectangle rectangle2 = new Rectangle(-(int)m_depth, renderGlobalBounds.Top, (int)m_depth, renderGlobalBounds.Height);
		foreach (ChartAxis axis3 in Axes)
		{
			if (axis3.Orientation == ChartOrientation.Horizontal)
			{
				Path3DCollect path3DCollect2 = new Path3DCollect(Polygon.CreateRectangle(rectangle, axis3.Location.Y, m_gridHorzInterior ?? m_gridInterior, null));
				path3DCollect2.Add(axis3.DrawInterlacedGrid(g, rectangle, axis3.Location.Y));
				path3DCollect2.Add(axis3.DrawGridLines(g, rectangle, axis3.Location.Y));
				if (path3DCollect2 != null)
				{
					path3DCollect2.Transform(Matrix3D.Tilt(1.5707963705062866));
					g.AddPolygon(path3DCollect2);
				}
			}
			else
			{
				Path3DCollect path3DCollect3 = new Path3DCollect(Polygon.CreateRectangle(rectangle2, axis3.Location.X, m_gridVertInterior ?? m_gridInterior, null));
				path3DCollect3.Add(axis3.DrawInterlacedGrid(g, rectangle2, axis3.Location.X));
				path3DCollect3.Add(axis3.DrawGridLines(g, rectangle2, axis3.Location.X));
				if (path3DCollect3 != null)
				{
					path3DCollect3.Transform(Matrix3D.Turn(-1.5707963705062866));
					g.AddPolygon(path3DCollect3);
				}
			}
		}
	}

	private void DrawAxes(Graphics3D g)
	{
		foreach (ChartAxis axis in Axes)
		{
			if (axis.IsVisible)
			{
				axis.DrawAxis(g, this, 0f);
			}
			axis.DrawBreaks(g, RenderBounds);
		}
	}

	private void DrawRadarAxes(Graphics3D g, ChartAxis xAxis, ChartAxis yAxis)
	{
		PointF center = Center;
		int numberOfIntervals = PrimaryYAxis.Range.NumberOfIntervals;
		int num = PrimaryXAxis.Range.NumberOfIntervals;
		if (PrimaryXAxis.IsIndexed)
		{
			num = m_chart.IndexValues.Count;
		}
		bool flag = m_chart.RadarStyle == ChartRadarAxisStyle.Polygon && !m_chart.Polar;
		float radarRadius = GetRadarRadius(xAxis.Font.GetHeight());
		if (radarRadius <= 0f)
		{
			return;
		}
		Path3DCollect path3DCollect = CreateWorkPlane(m_depth);
		path3DCollect.Add(Path3D.FromGraphicsPath(GetRadarPath(center, radarRadius, flag, num), m_depth, m_gridInterior));
		GraphicsPath graphicsPath = new GraphicsPath();
		for (int i = 1; i < numberOfIntervals + 1; i++)
		{
			if (yAxis.Pens != null)
			{
				GraphicsPath graphicsPath2 = new GraphicsPath();
				graphicsPath2.AddPath(GetRadarPath(center, (float)i * (radarRadius / (float)numberOfIntervals), flag, num), connect: false);
				Pen pen = yAxis.LineType.Pen;
				if (yAxis.Pens != null && yAxis.Pens.Length != 0)
				{
					pen = GetRadarPen(yAxis, i - 1);
					if (i == 0 || i == num)
					{
						pen = xAxis.LineType.Pen;
					}
				}
				path3DCollect.Add(Path3D.FromGraphicsPath(graphicsPath2, m_depth, pen));
			}
			else
			{
				graphicsPath.AddPath(GetRadarPath(center, (float)i * (radarRadius / (float)numberOfIntervals), flag, num), connect: false);
			}
		}
		if (yAxis.Pens == null)
		{
			path3DCollect.Add(Path3D.FromGraphicsPath(graphicsPath, m_depth, yAxis.LineType.Pen));
		}
		graphicsPath = new GraphicsPath();
		double num2 = Math.PI / 2.0;
		bool flag2 = m_chart.RadarStyle == ChartRadarAxisStyle.Circle && !m_chart.Polar;
		bool polar = m_chart.Polar;
		if (polar)
		{
			num = 12;
		}
		for (int j = 0; j <= num; j++)
		{
			double num3 = (double)((float)j * 2f) * Math.PI / (double)num + num2;
			ChartAxisLabel chartAxisLabel = null;
			if (flag || flag2)
			{
				if (xAxis.IsIndexed)
				{
					chartAxisLabel = PrimaryXAxis.GenerateLabel(j, this, j);
				}
				else
				{
					double min = xAxis.Range.Min;
					double max = xAxis.Range.Max;
					double interval = PrimaryXAxis.Range.Interval;
					double num4 = min + interval * (double)j - xAxis.Offset;
					num4 = ((!xAxis.Inversed) ? (min + interval * (double)j - xAxis.Offset) : (max - interval * (double)j - xAxis.Offset));
					chartAxisLabel = PrimaryXAxis.GenerateLabel(num4, this, j);
				}
			}
			else
			{
				chartAxisLabel = PrimaryXAxis.GenerateLabel(num3 - Math.PI / 2.0, this, j);
				if ((polar && xAxis.RangeType == ChartAxisRangeType.Auto) || xAxis.IsIndexed)
				{
					if (xAxis.Inversed)
					{
						double value = Math.PI * 2.0 - Math.PI / 6.0 * (double)j;
						chartAxisLabel = PrimaryXAxis.GenerateLabel(value, this, j);
					}
					else
					{
						chartAxisLabel = PrimaryXAxis.GenerateLabel(num3 - Math.PI / 2.0, this, j);
					}
				}
				else if (polar)
				{
					double min2 = xAxis.Range.Min;
					double max2 = xAxis.Range.Max;
					double num5 = ((max2 > min2) ? (max2 - min2) : (min2 - max2));
					double num6 = num5 / 12.0 * (double)j;
					num6 = ((!xAxis.Inversed) ? (num5 / 12.0 * (double)j) : (max2 - num5 / 12.0 * (double)j));
					chartAxisLabel = PrimaryXAxis.GenerateLabel(num6, this, j);
				}
			}
			SizeF sizeF = chartAxisLabel.Measure(g.Graphics, PrimaryXAxis);
			float x = center.X + (float)Math.Cos(num3) * radarRadius;
			float y = center.Y - (float)Math.Sin(num3) * radarRadius;
			float num7 = center.X + (float)Math.Cos(num3) * (radarRadius + 5f + Math.Abs(sizeF.Height / 2f * (float)Math.Sin(num3)) + Math.Abs(sizeF.Width / 2f * (float)Math.Cos(num3))) - sizeF.Width / 2f;
			float num8 = center.Y - (float)Math.Sin(num3) * (radarRadius + 5f + Math.Abs(sizeF.Height / 2f * (float)Math.Sin(num3)) + Math.Abs(sizeF.Width / 2f * (float)Math.Cos(num3)));
			num7 += sizeF.Width / 2f;
			if (j == 0)
			{
				num7 -= (float)((double)sizeF.Width * Math.Sin(num3)) / 2f;
				num8 -= (float)((double)sizeF.Height * Math.Cos(num3)) / 2f;
			}
			if (j == num)
			{
				num7 += (float)((double)sizeF.Width * Math.Sin(num3)) / 2f;
				num8 += (float)((double)sizeF.Height * Math.Cos(num3)) / 2f;
			}
			graphicsPath.AddLine(center, new PointF(x, y));
			GraphicsPath graphicsPath3 = new GraphicsPath();
			Brush br = new SolidBrush(xAxis.ForeColor);
			Font font = chartAxisLabel.Font;
			graphicsPath3.AddString(chartAxisLabel.Text, font.GetFontFamily(), (int)font.Style, RenderingHelper.GetFontSizeInPixels(font), new PointF(num7, num8), StringFormat.GenericDefault);
			path3DCollect.Add(Path3D.FromGraphicsPath(graphicsPath3, m_depth, br));
			if (xAxis.Pens == null)
			{
				continue;
			}
			GraphicsPath graphicsPath4 = new GraphicsPath();
			graphicsPath4.AddLine(center, center);
			Pen pen2 = xAxis.LineType.Pen;
			if (xAxis.Pens.Length != 0)
			{
				pen2 = GetRadarPen(xAxis, j);
				if (j == 0 || j == num)
				{
					pen2 = yAxis.LineType.Pen;
				}
			}
			graphicsPath4.AddLine(center, new PointF(x, y));
			path3DCollect.Add(Path3D.FromGraphicsPath(graphicsPath4, m_depth, pen2));
			g.AddPolygon(path3DCollect);
		}
		if (xAxis.Pens == null)
		{
			path3DCollect.Add(Path3D.FromGraphicsPath(graphicsPath, m_depth, xAxis.LineType.Pen));
			g.AddPolygon(path3DCollect);
		}
		yAxis.DrawAxis(g, this, -1f);
	}

	private Transform3D UpdateTransform3D()
	{
		m_transform3D.SetCenter(new Vector3D(Left + Width / 2, Top + Height / 2, m_depth / 2f));
		m_transform3D.View = Matrix3D.Transform(0.0, 0.0, m_depth);
		m_transform3D.View *= Matrix3D.Turn(-Math.PI / 180.0 * (double)Rotation);
		m_transform3D.View *= Matrix3D.Tilt(-Math.PI / 180.0 * (double)Tilt);
		m_transform3D.View *= Matrix3D.Twist(-Math.PI / 180.0 * (double)Turn);
		if (m_3dSettings.Perspective)
		{
			if (m_3dSettings.AutoPerspective)
			{
				m_transform3D.SetPerspective(Width);
			}
			else
			{
				m_transform3D.SetPerspective(m_3dSettings.ZDistant);
			}
		}
		if (m_autoScale)
		{
			Matrix3D result = m_transform3D.Result;
			Vector3D vector3D = result * new Vector3D(Left, Top, 0.0);
			Vector3D vector3D2 = result * new Vector3D(Left, Bottom, 0.0);
			Vector3D vector3D3 = result * new Vector3D(Right, Top, 0.0);
			Vector3D vector3D4 = result * new Vector3D(Right, Bottom, 0.0);
			Vector3D vector3D5 = result * new Vector3D(Left, Top, m_depth);
			Vector3D vector3D6 = result * new Vector3D(Left, Bottom, m_depth);
			Vector3D vector3D7 = result * new Vector3D(Right, Top, m_depth);
			Vector3D vector3D8 = result * new Vector3D(Right, Bottom, m_depth);
			double[] values = new double[8] { vector3D.X, vector3D2.X, vector3D3.X, vector3D4.X, vector3D5.X, vector3D6.X, vector3D7.X, vector3D8.X };
			double[] values2 = new double[8] { vector3D.Y, vector3D2.Y, vector3D3.Y, vector3D4.Y, vector3D5.Y, vector3D6.Y, vector3D7.Y, vector3D8.Y };
			RectangleF rectangleF = RectangleF.FromLTRB((float)ChartMath.Min(values), (float)ChartMath.Min(values2), (float)ChartMath.Max(values), (float)ChartMath.Max(values2));
			double value = ((rectangleF.Right > (float)Right) ? (2f * ((float)Right - (rectangleF.Left + rectangleF.Width / 2f)) / rectangleF.Width) : 1f);
			double value2 = ((rectangleF.Left < (float)Left) ? (2f * (rectangleF.Left + rectangleF.Width / 2f - (float)Left) / rectangleF.Width) : 1f);
			double value3 = ((rectangleF.Bottom > (float)Bottom) ? (2f * ((float)Bottom - (rectangleF.Top + rectangleF.Height / 2f)) / rectangleF.Height) : 1f);
			double value4 = ((rectangleF.Top < (float)Top) ? (2f * (rectangleF.Top + rectangleF.Height / 2f - (float)Top) / rectangleF.Height) : 1f);
			double num = ChartMath.Min(new double[4]
			{
				Math.Abs(value),
				Math.Abs(value2),
				Math.Abs(value3),
				Math.Abs(value4)
			});
			m_transform3D.View = Matrix3D.Scale(num, num, num) * m_transform3D.View;
		}
		return m_transform3D;
	}

	private Path3DCollect CreateWorkPlane(float z)
	{
		Polygon polygon = new Polygon(new Vector3D[4]
		{
			new Vector3D(RenderBounds.Left, RenderBounds.Top, z),
			new Vector3D(RenderBounds.Right, RenderBounds.Top, z),
			new Vector3D(RenderBounds.Right, RenderBounds.Bottom, z),
			new Vector3D(RenderBounds.Left, RenderBounds.Bottom, z)
		});
		return new Path3DCollect(new Polygon[1] { polygon });
	}

	private void DrawCustomPoints(Graphics3D g)
	{
		if (CustomPoints.Count <= 0)
		{
			return;
		}
		foreach (ChartCustomPoint customPoint in CustomPoints)
		{
			PointF customPointLocation = GetCustomPointLocation(customPoint);
			customPoint.Draw(this, g, new Vector3D(customPointLocation.X, customPointLocation.Y, 0.0));
		}
	}

	private void DrawStripLines(Graphics3D g, ChartStripLineZorder zOrder)
	{
		foreach (ChartAxis axis in Axes)
		{
			foreach (ChartStripLine stripLine in axis.StripLines)
			{
				if (stripLine.Enabled && stripLine.ZOrder == zOrder)
				{
					Polygon polygon = stripLine.Draw(g, GetStripLineRects(axis, stripLine), m_depth);
					if (polygon != null)
					{
						g.AddPolygon(polygon);
					}
				}
			}
		}
	}

	private bool ShouldSerializeBackInterior()
	{
		return m_backInterior != null;
	}

	private bool ShouldSerializeAxisSpacing()
	{
		return m_axisSpacing != new SizeF(2f, 2f);
	}

	private Point GetPointByValueInternal(ChartAxis xAxis, ChartAxis yAxis, ChartPoint cpt)
	{
		Point result = Point.Empty;
		if (m_requireAxes)
		{
			result = ((!m_requireInvertedAxes) ? GetPointByValueNormal(xAxis, yAxis, cpt) : GetPointByValueInversed(xAxis, yAxis, cpt));
		}
		else if (m_chart.Radar)
		{
			result = GetPointByValuePolar(xAxis, yAxis, cpt);
		}
		return result;
	}

	private Point GetPointByValueInternalMulAxes(ChartAxis xAxis, ChartAxis yAxis, ChartPoint cpt)
	{
		Point result = Point.Empty;
		if (m_requireAxes)
		{
			result = GetPointByValueNormalMulAxes(xAxis, yAxis, cpt);
		}
		else if (m_chart.Radar)
		{
			result = GetPointByValuePolar(xAxis, yAxis, cpt);
		}
		return result;
	}

	private Point GetPointByValueNormal(ChartAxis xAxis, ChartAxis yAxis, ChartPoint cpt)
	{
		return new Point((int)xAxis.GetVisibleValue(GetX(xAxis, cpt)) + RenderBounds.Left, RenderBounds.Bottom - (int)yAxis.GetVisibleValue(cpt.YValues[0]));
	}

	private double GetX(ChartAxis xAxis, ChartPoint cpt)
	{
		if (xAxis.ValueType != ChartValueType.Category)
		{
			return cpt.X;
		}
		return GetPointIndex(xAxis.SortLabels, cpt.Category);
	}

	private int GetPointIndex(Array a, string value)
	{
		for (int i = 0; i < a.Length; i++)
		{
			if (a.GetValue(i).ToString().Equals(value))
			{
				return i;
			}
		}
		return -1;
	}

	private Point GetPointByValueNormalMulAxes(ChartAxis xAxis, ChartAxis yAxis, ChartPoint cpt)
	{
		return new Point((int)xAxis.GetCoordinateFromValue(cpt.X), (int)yAxis.GetCoordinateFromValue(cpt.YValues[0]));
	}

	private Point GetPointByValueInversed(ChartAxis xAxis, ChartAxis yAxis, ChartPoint cpt)
	{
		return new Point((int)xAxis.GetVisibleValue(cpt.YValues[0]) + RenderBounds.Left, RenderBounds.Bottom - (int)yAxis.GetVisibleValue(GetX(yAxis, cpt)));
	}

	private ChartPoint GetValueByPointInversed(ChartAxis xAxis, ChartAxis yAxis, Point pt)
	{
		return new ChartPoint(yAxis.GetRealValue(pt), xAxis.GetRealValue(pt));
	}

	private ChartPoint GetValueByPointNormal(ChartAxis xAxis, ChartAxis yAxis, Point pt)
	{
		return new ChartPoint(xAxis.GetRealValue(pt), yAxis.GetRealValue(pt));
	}

	private ChartPoint GetValueByPointPolar(ChartAxis xAxis, ChartAxis yAxis, Point pt)
	{
		_ = xAxis.VisibleRange;
		MinMaxInfo visibleRange = yAxis.VisibleRange;
		PointF center = Center;
		pt = new Point((int)((float)pt.X - center.X), (int)((float)pt.Y - center.Y));
		double num = 4.71238898038469 - Math.Atan2(pt.Y, pt.X);
		double num2 = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
		num2 = visibleRange.Delta * num2 / (double)Radius;
		if (num > Math.PI * 2.0)
		{
			num -= Math.PI * 2.0;
		}
		return new ChartPoint(num, num2);
	}

	private Point GetPointByValuePolar(ChartAxis xAxis, ChartAxis yAxis, ChartPoint cpt)
	{
		MinMaxInfo visibleRange = xAxis.VisibleRange;
		MinMaxInfo visibleRange2 = yAxis.VisibleRange;
		PointF center = Center;
		if (m_chart.Radar && m_chart.Polar)
		{
			double num = (double)Radius * cpt.YValues[0] / visibleRange2.Delta;
			double num2 = 4.71238898038469 - GetX(xAxis, cpt);
			return new Point((int)((double)center.X + num * Math.Cos(num2)), (int)((double)center.Y + num * Math.Sin(num2)));
		}
		float visibleValue = yAxis.GetVisibleValue(cpt.YValues[0]);
		if (xAxis.Inversed)
		{
			double num3 = Math.PI * 2.0 * (visibleRange.max - cpt.X) / visibleRange.Delta + Math.PI / 2.0;
			double num4 = (float)((double)center.X + (double)visibleValue * Math.Cos(num3));
			double num5 = center.Y - (float)((double)visibleValue * Math.Sin(num3));
			return new Point((int)num4, (int)num5);
		}
		double num6 = Math.PI * 2.0 * (cpt.X - visibleRange.min) / visibleRange.Delta + Math.PI / 2.0;
		double num7 = (float)((double)center.X + (double)visibleValue * Math.Cos(num6));
		double num8 = center.Y - (float)((double)visibleValue * Math.Sin(num6));
		return new Point((int)num7, (int)num8);
	}

	private void UpdatePlotareaWidth(ref RectangleF rec)
	{
		if (!IsAxisVisible)
		{
			return;
		}
		if (m_largeValueAxisLabelFontSize >= 9f && m_largeValueAxisLabelFontSize < 10f)
		{
			switch (LargeValueAxisLabelLength)
			{
			case 1:
				rec.Width -= 5f;
				break;
			case 2:
				rec.Width -= 8f;
				break;
			case 3:
				rec.Width -= 11f;
				break;
			case 4:
				rec.Width -= 14f;
				break;
			case 5:
				rec.Width -= 17f;
				break;
			case 6:
				rec.Width -= 20f;
				break;
			}
		}
		else if (m_largeValueAxisLabelFontSize >= 10f && m_largeValueAxisLabelFontSize < 11f)
		{
			switch (LargeValueAxisLabelLength)
			{
			case 1:
				rec.Width -= 6f;
				break;
			case 2:
				rec.Width -= 9f;
				break;
			case 3:
				rec.Width -= 12f;
				break;
			case 4:
				rec.Width -= 15f;
				break;
			case 5:
				rec.Width -= 18f;
				break;
			case 6:
				rec.Width -= 21f;
				break;
			}
		}
		else if (m_largeValueAxisLabelFontSize >= 11f && m_largeValueAxisLabelFontSize < 12f)
		{
			switch (LargeValueAxisLabelLength)
			{
			case 1:
				rec.Width -= 7f;
				break;
			case 2:
				rec.Width -= 10f;
				break;
			case 3:
				rec.Width -= 13f;
				break;
			case 4:
				rec.Width -= 16f;
				break;
			case 5:
				rec.Width -= 19f;
				break;
			case 6:
				rec.Width -= 22f;
				break;
			}
		}
		else if (m_largeValueAxisLabelFontSize >= 12f && m_largeValueAxisLabelFontSize < 13f)
		{
			switch (LargeValueAxisLabelLength)
			{
			case 1:
				rec.Width -= 8f;
				break;
			case 2:
				rec.Width -= 11f;
				break;
			case 3:
				rec.Width -= 14f;
				break;
			case 4:
				rec.Width -= 17f;
				break;
			case 5:
				rec.Width -= 20f;
				break;
			case 6:
				rec.Width -= 23f;
				break;
			}
		}
		else if (m_largeValueAxisLabelFontSize >= 13f && m_largeValueAxisLabelFontSize < 14f)
		{
			switch (LargeValueAxisLabelLength)
			{
			case 1:
				rec.Width -= 9f;
				break;
			case 2:
				rec.Width -= 12f;
				break;
			case 3:
				rec.Width -= 15f;
				break;
			case 4:
				rec.Width -= 18f;
				break;
			case 5:
				rec.Width -= 21f;
				break;
			case 6:
				rec.Width -= 24f;
				break;
			}
		}
	}

	private void UpdatePlotareaWidthForScatter(ref RectangleF rec)
	{
		if (m_graphics == null || !(m_chart as ChartControl).IsScatter)
		{
			return;
		}
		float num = 0f;
		foreach (ChartAxis axis in Axes)
		{
			if (axis.Orientation != 0)
			{
				continue;
			}
			ChartAxisLabel[] visibleLabels = axis.VisibleLabels;
			if (visibleLabels != null && visibleLabels.Length != 0)
			{
				ChartAxisLabel chartAxisLabel = visibleLabels[^1];
				float num2 = chartAxisLabel.Measure(m_graphics, axis).Width / 2f;
				if (((axis.ValueType == ChartValueType.Category) ? axis.GetCoordinateFromValue(visibleLabels.Length - 1) : (axis.GetCoordinateFromValue(chartAxisLabel.DoubleValue) + num2)) >= (float)Size.Width)
				{
					num = Math.Max(num2, num);
				}
			}
		}
		rec.Width -= num;
	}

	private void UpdatePlotAreaTopPosition(ref RectangleF rec)
	{
		if (m_graphics == null)
		{
			return;
		}
		float num = 0f;
		foreach (ChartAxis axis in Axes)
		{
			if (axis.Orientation == ChartOrientation.Vertical)
			{
				ChartAxisLabel[] visibleLabels = axis.VisibleLabels;
				if (visibleLabels != null && visibleLabels.Length != 0 && visibleLabels[^1].Measure(m_graphics, axis).Height >= 42.5625f)
				{
					num = 2f;
				}
			}
		}
		rec.Y += num;
	}

	private void CalculateAxesSizes()
	{
		if (m_chart.Radar)
		{
			PointF center = Center;
			bool flag = PrimaryXAxis.TickLabelsDrawingMode != ChartAxisTickLabelDrawingMode.None;
			float radarRadius = GetRadarRadius(flag ? PrimaryXAxis.Font.GetHeight() : 0f);
			if (PrimaryYAxis != null)
			{
				PrimaryYAxis.CalculateAxis(new RectangleF(center.X, center.Y - radarRadius, 0f, radarRadius), SizeF.Empty, 0f);
			}
			return;
		}
		RectangleF rec = GetGlobalBoundsByRect(m_clientRectangle);
		UpdatePlotareaWidthForScatter(ref rec);
		UpdatePlotAreaTopPosition(ref rec);
		if (m_chart.Bar)
		{
			UpdatePlotareaWidth(ref rec);
		}
		if (!m_realSeries3D)
		{
			rec = GetBoundsByRect(Rectangle.Round(rec));
		}
		List<ChartAxis> list = new List<ChartAxis>();
		List<ChartAxis> list2 = new List<ChartAxis>();
		foreach (ChartAxis axis in Axes)
		{
			if (axis.Orientation == ChartOrientation.Horizontal)
			{
				list.Add(axis);
			}
			else
			{
				list2.Add(axis);
			}
		}
		if (m_xLayouts.Count > 0)
		{
			m_xLayouts.Arrange(rec, ChartOrientation.Horizontal);
		}
		else
		{
			int num = 0;
			float num2 = 0f;
			RectangleF rect = rec;
			foreach (ChartAxis item in list)
			{
				if (!item.AutoSize)
				{
					num2 += item.RealLength;
					num++;
				}
			}
			float num3 = (rec.Width - num2) / (float)(list.Count - num);
			float num4 = rect.Left;
			for (int i = 0; i < list.Count; i++)
			{
				ChartAxis chartAxis2 = list[i];
				if (m_xAxesLayoutMode == ChartAxesLayoutMode.SideBySide)
				{
					rect = new RectangleF(num4, rec.Y, chartAxis2.AutoSize ? num3 : chartAxis2.RealLength, rec.Height);
					num4 += rect.Width;
				}
				rect = chartAxis2.CalculateAxis(Rectangle.Round(rect), m_axisSpacing, chartAxis2.Dimension);
			}
		}
		if (m_yLayouts.Count > 0)
		{
			m_yLayouts.Arrange(rec, ChartOrientation.Vertical);
			return;
		}
		RectangleF rect2 = rec;
		int num5 = 0;
		float num6 = 0f;
		foreach (ChartAxis item2 in list2)
		{
			if (!item2.AutoSize)
			{
				num6 += item2.RealLength;
				num5++;
			}
		}
		float num7 = (rec.Height - num6) / (float)(list2.Count - num5);
		float num8 = rect2.Bottom;
		for (int j = 0; j < list2.Count; j++)
		{
			ChartAxis chartAxis3 = list2[j];
			if (m_yAxesLayoutMode == ChartAxesLayoutMode.SideBySide)
			{
				float num9 = (chartAxis3.AutoSize ? num7 : chartAxis3.RealLength);
				rect2 = new RectangleF(rec.X, num8 - num9, rec.Width, num9);
				num8 -= rect2.Height;
			}
			rect2 = chartAxis3.CalculateAxis(Rectangle.Round(rect2), m_axisSpacing, chartAxis3.Dimension);
		}
	}

	private void CalculateLabelSizes(Graphics g, RectangleF bounds)
	{
		SizeF empty = SizeF.Empty;
		SizeF empty2 = SizeF.Empty;
		SizeF empty3 = SizeF.Empty;
		RectangleF renderBounds = bounds;
		List<ChartAxis> list = new List<ChartAxis>();
		List<ChartAxis> list2 = new List<ChartAxis>();
		foreach (ChartAxis axis in Axes)
		{
			if (axis.Orientation == ChartOrientation.Horizontal)
			{
				list.Add(axis);
			}
			else
			{
				list2.Add(axis);
			}
		}
		renderBounds.Width -= OffsetX;
		renderBounds.Height -= OffsetY;
		if (m_yLayouts.Count > 0)
		{
			m_yLayouts.Measure(g, renderBounds, ChartOrientation.Vertical, out var left, out var right, out var scrolls);
			empty.Width = left;
			empty2.Width = right;
			empty3.Width = scrolls;
		}
		else
		{
			foreach (ChartAxis axis2 in Axes)
			{
				DisplayUnits = axis2.DisplayUnits;
				if (axis2.Orientation != ChartOrientation.Vertical)
				{
					continue;
				}
				float dimension = axis2.GetDimension(g, this, renderBounds);
				if (!axis2.IsVisible || axis2.LocationType == ChartAxisLocationType.Set)
				{
					continue;
				}
				renderBounds.Width += dimension;
				switch (m_yAxesLayoutMode)
				{
				case ChartAxesLayoutMode.Stacking:
					if (axis2.AxisLabelPlacement == ChartPlacement.Inside)
					{
						float titleDimention2 = axis2.GetTitleDimention(g, this, renderBounds);
						if (axis2.OpposedPosition)
						{
							empty2.Width += m_axisSpacing.Width + titleDimention2;
						}
						else
						{
							empty.Width += m_axisSpacing.Width + titleDimention2;
						}
					}
					else if (axis2.OpposedPosition)
					{
						empty2.Width += dimension + m_axisSpacing.Width;
					}
					else
					{
						empty.Width += dimension + m_axisSpacing.Width;
					}
					break;
				case ChartAxesLayoutMode.SideBySide:
					if (axis2.AxisLabelPlacement == ChartPlacement.Inside)
					{
						float titleDimention = axis2.GetTitleDimention(g, this, renderBounds);
						if (axis2.OpposedPosition)
						{
							empty2.Width = Math.Max(titleDimention + m_axisSpacing.Width, empty2.Width);
						}
						else
						{
							empty.Width = Math.Max(titleDimention + m_axisSpacing.Width, empty.Width);
						}
					}
					else if (axis2.OpposedPosition)
					{
						empty2.Width = Math.Max(dimension + m_axisSpacing.Width, empty2.Width);
					}
					else
					{
						empty.Width = Math.Max(dimension + m_axisSpacing.Width, empty.Width);
					}
					break;
				}
			}
		}
		m_axesThickness.Left = empty.Width;
		m_axesThickness.Right = empty2.Width + empty3.Width;
		renderBounds.X += m_axesThickness.Left;
		renderBounds.Width -= m_axesThickness.Left + m_axesThickness.Right;
		if (m_xLayouts.Count > 0)
		{
			m_xLayouts.Measure(g, renderBounds, ChartOrientation.Horizontal, out var left2, out var right2, out var scrolls2);
			empty.Height = left2;
			empty2.Height = right2;
			empty3.Height = scrolls2;
		}
		else
		{
			foreach (ChartAxis axis3 in Axes)
			{
				if (axis3.Orientation != 0)
				{
					continue;
				}
				float dimension2 = axis3.GetDimension(g, this, renderBounds);
				if (!axis3.IsVisible || axis3.LocationType == ChartAxisLocationType.Set)
				{
					continue;
				}
				renderBounds.Height += dimension2;
				switch (m_xAxesLayoutMode)
				{
				case ChartAxesLayoutMode.Stacking:
					if (axis3.AxisLabelPlacement == ChartPlacement.Inside)
					{
						float titleDimention4 = axis3.GetTitleDimention(g, this, renderBounds);
						if (axis3.OpposedPosition)
						{
							empty2.Height += m_axisSpacing.Height + titleDimention4;
						}
						else
						{
							empty.Height += m_axisSpacing.Height + titleDimention4;
						}
					}
					else if (axis3.OpposedPosition)
					{
						empty2.Height += dimension2 + m_axisSpacing.Height;
					}
					else
					{
						empty.Height += dimension2 + m_axisSpacing.Height;
					}
					break;
				case ChartAxesLayoutMode.SideBySide:
					if (axis3.AxisLabelPlacement == ChartPlacement.Inside)
					{
						float titleDimention3 = axis3.GetTitleDimention(g, this, renderBounds);
						if (axis3.OpposedPosition)
						{
							empty2.Height = Math.Max(titleDimention3 + m_axisSpacing.Height, empty2.Height);
						}
						else
						{
							empty.Height = Math.Max(titleDimention3 + m_axisSpacing.Height, empty.Height);
						}
					}
					else if (axis3.OpposedPosition)
					{
						empty2.Height = Math.Max(dimension2 + m_axisSpacing.Height, empty2.Height);
					}
					else
					{
						empty.Height = Math.Max(dimension2 + m_axisSpacing.Height, empty.Height);
					}
					break;
				}
			}
		}
		m_axesThickness.Top = empty2.Height;
		m_axesThickness.Bottom = empty.Height + empty3.Height;
	}

	private void OnAxesChanged(ChartBaseList list, ChartListChangeArgs args)
	{
		if (args.NewItems != null)
		{
			object[] newItems = args.NewItems;
			for (int i = 0; i < newItems.Length; i++)
			{
				ChartAxis obj = (ChartAxis)newItems[i];
				obj.SetOwner(this);
				obj.DimensionsChanged += OnChangingRedraw;
				obj.AppearanceChanged += ApearanceChanged;
				obj.AppearanceChanged += OnChangingRedraw;
				obj.VisibleRangeChanged += OnChangingRedraw;
				obj.IntervalsChanged += ApearanceChanged;
			}
		}
		if (args.OldItems != null)
		{
			object[] newItems = args.OldItems;
			for (int i = 0; i < newItems.Length; i++)
			{
				ChartAxis obj2 = (ChartAxis)newItems[i];
				obj2.SetOwner(null);
				obj2.DimensionsChanged -= OnChangingRedraw;
				obj2.AppearanceChanged -= ApearanceChanged;
				obj2.AppearanceChanged -= OnChangingRedraw;
				obj2.VisibleRangeChanged -= OnChangingRedraw;
				obj2.IntervalsChanged -= ApearanceChanged;
			}
		}
	}

	private void OnCustomPointsListChanged(ChartBaseList list, ChartListChangeArgs args)
	{
		if (args.NewItems != null)
		{
			object[] newItems = args.NewItems;
			for (int i = 0; i < newItems.Length; i++)
			{
				((ChartCustomPoint)newItems[i]).SettingsChanged += OnChangingRedraw;
			}
		}
		if (args.OldItems != null)
		{
			object[] newItems = args.OldItems;
			for (int i = 0; i < newItems.Length; i++)
			{
				((ChartCustomPoint)newItems[i]).SettingsChanged -= OnChangingRedraw;
			}
		}
	}

	private void ApearanceChanged(object sender, EventArgs e)
	{
		m_chart.SeriesChanged(this, new ChartSeriesCollectionChangedEventArgs(ChartSeriesCollectionChangeType.Changed));
	}

	private Rectangle GetBoundsByRect(Rectangle rect)
	{
		int num = (int)OffsetX;
		if (Rotation > 0f)
		{
			rect.Width -= num;
		}
		else
		{
			rect.Width -= num;
			rect.X += num;
		}
		int num2 = (int)OffsetY;
		if (Tilt > 0f)
		{
			rect.Height -= num2;
			rect.Y += num2;
		}
		else
		{
			rect.Height -= num2;
		}
		return rect;
	}

	private Rectangle GetGlobalBoundsByRect(Rectangle rect)
	{
		if (AxesType == ChartAreaAxesType.Rectangular)
		{
			rect = m_axesThickness.Deflate(rect);
		}
		rect.X += ChartAreaMargins.Left;
		rect.Width -= ChartAreaMargins.Left + ChartAreaMargins.Right;
		rect.Y += ChartAreaMargins.Top;
		rect.Height -= ChartAreaMargins.Top + ChartAreaMargins.Bottom;
		return rect;
	}

	public Point CorrectionFrom(Point pt)
	{
		if (RealSeries3D && Series3D)
		{
			pt = Point.Round(m_transform3D.ToScreen(new Vector3D(pt.X, pt.Y, 0.0)));
		}
		return pt;
	}

	public PointF CorrectionFrom(PointF pt)
	{
		if (RealSeries3D && Series3D)
		{
			pt = m_transform3D.ToScreen(new Vector3D(pt.X, pt.Y, 0.0));
		}
		return pt;
	}

	public void DoAppearanceChange()
	{
		if (LegacyAppearance)
		{
			PrimaryXAxis.GridLineType.ForeColor = ((PrimaryXAxis.GridLineType.ForeColor == Color.Black) ? Color.LightGray : PrimaryXAxis.GridLineType.ForeColor);
			PrimaryYAxis.GridLineType.ForeColor = ((PrimaryYAxis.GridLineType.ForeColor == Color.Black) ? Color.LightGray : PrimaryYAxis.GridLineType.ForeColor);
			PrimaryXAxis.LineType.ForeColor = ((PrimaryXAxis.LineType.ForeColor == Color.Black) ? Color.LightGray : PrimaryXAxis.LineType.ForeColor);
			PrimaryYAxis.LineType.ForeColor = ((PrimaryYAxis.LineType.ForeColor == Color.Black) ? Color.LightGray : PrimaryYAxis.LineType.ForeColor);
			PrimaryXAxis.TickColor = ((PrimaryXAxis.TickColor == SystemColors.ControlText) ? Color.LightGray : PrimaryXAxis.TickColor);
			PrimaryYAxis.TickColor = ((PrimaryYAxis.TickColor == SystemColors.ControlText) ? Color.LightGray : PrimaryYAxis.TickColor);
			PrimaryXAxis.Font = ((PrimaryXAxis.Font.Name == "Verdana" && PrimaryXAxis.Font.Size == 8f) ? new Font("Segoe UI", 9f, FontStyle.Regular) : PrimaryXAxis.Font);
			PrimaryXAxis.TitleFont = ((PrimaryXAxis.TitleFont.Name == "Verdana" && PrimaryXAxis.TitleFont.Size == 8f) ? new Font("Segoe UI", 12f, FontStyle.Regular) : PrimaryXAxis.TitleFont);
			PrimaryYAxis.TitleFont = ((PrimaryYAxis.TitleFont.Name == "Verdana" && PrimaryYAxis.TitleFont.Size == 8f) ? new Font("Segoe UI", 12f, FontStyle.Regular) : PrimaryYAxis.TitleFont);
			PrimaryYAxis.Font = ((PrimaryYAxis.Font.Name == "Verdana" && PrimaryYAxis.Font.Size == 8f) ? new Font("Segoe UI", 9f, FontStyle.Regular) : PrimaryYAxis.Font);
			PrimaryXAxis.ForeColor = ((PrimaryXAxis.ForeColor == Color.Black) ? Color.Black : PrimaryXAxis.ForeColor);
			PrimaryYAxis.ForeColor = ((PrimaryYAxis.ForeColor == Color.Black) ? Color.Black : PrimaryYAxis.ForeColor);
			Chart.ChartInterior = ((Chart.ChartInterior == new BrushInfo(Color.White)) ? new BrushInfo(Color.White) : Chart.ChartInterior);
			Chart.BackInterior = ((Chart.BackInterior == new BrushInfo(Color.White)) ? new BrushInfo(Color.White) : Chart.BackInterior);
		}
	}

	public Point CorrectionTo(Point pt)
	{
		if (RealSeries3D && Series3D)
		{
			Plane3D plane3D = new Plane3D(new Vector3D(0.0, 0.0, 1.0), 0.0);
			plane3D.Transform(m_transform3D.View);
			Vector3D vector3D = m_transform3D.ToPlane(pt, plane3D);
			pt = new Point((int)vector3D.X, (int)vector3D.Y);
		}
		return pt;
	}

	public PointF CorrectionTo(PointF pt)
	{
		if (RealSeries3D && Series3D)
		{
			Plane3D plane3D = new Plane3D(new Vector3D(0.0, 0.0, 1.0), 0.0);
			plane3D.Transform(m_transform3D.View);
			Vector3D vector3D = m_transform3D.ToPlane(pt, plane3D);
			pt = new Point((int)vector3D.X, (int)vector3D.Y);
		}
		return pt;
	}

	private bool IsPaintFlag(ChartPaintFlags target, ChartPaintFlags flag)
	{
		return (target & flag) == flag;
	}

	private float RoundRotation(float value)
	{
		if (DrawingMode != DrawingMode.Real3D)
		{
			if (value > 90f)
			{
				value = 89.9f;
			}
			else if (value < 0.1f)
			{
				value = 0.1f;
			}
		}
		else
		{
			value %= 360f;
		}
		return value;
	}

	private float GetRadarRadius(float fontHeight)
	{
		RectangleF rectangleF = RenderBounds;
		return 0.5f * Math.Min(rectangleF.Width, rectangleF.Height) - fontHeight * 2f;
	}

	private void OnChangingRedraw(object sender, EventArgs e)
	{
	}

	[Obsolete("This method isn't used anymore.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public RectangleF GetFrontBoundByAxes()
	{
		if (!m_chart.Radar)
		{
			return GetFrontBoundByAxes(byAllAxes: false);
		}
		return GetGlobalBoundsByRect(m_clientRectangle);
	}

	[Obsolete("This method isn't used anymore.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public RectangleF GetFrontBoundByAxes(bool byAllAxes)
	{
		RectangleF result = RectangleF.Empty;
		if (byAllAxes)
		{
			float num = ClientRectangle.Right;
			float num2 = ClientRectangle.Bottom;
			float num3 = ClientRectangle.Left;
			float num4 = ClientRectangle.Top;
			for (int i = 0; i < m_axes.Count; i++)
			{
				num = Math.Min(m_axes[i].Rect.Left, num);
				num2 = Math.Min(m_axes[i].Rect.Top, num2);
				num3 = Math.Min(m_axes[i].Rect.Right, num3);
				num4 = Math.Min(m_axes[i].Rect.Bottom, num4);
			}
			result = RectangleF.FromLTRB(num, num2, num3, num4);
		}
		else
		{
			result = new RectangleF(PrimaryXAxis.Rect.X, PrimaryYAxis.Rect.Y, PrimaryXAxis.Rect.Width, PrimaryYAxis.Rect.Height);
		}
		return result;
	}

	[Obsolete("This method is incorrect.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static RectangleF GetAxesRect(ChartAxis ax1, ChartAxis ax2)
	{
		RectangleF rectangleF = RectangleF.Empty;
		RectangleF rectangleF2 = RectangleF.Empty;
		RectangleF empty = RectangleF.Empty;
		if (ax1.Orientation == ChartOrientation.Horizontal)
		{
			rectangleF = ax1.Rect;
		}
		if (ax2.Orientation == ChartOrientation.Horizontal)
		{
			rectangleF = ax2.Rect;
		}
		if (ax1.Orientation == ChartOrientation.Vertical)
		{
			rectangleF2 = ax1.Rect;
		}
		if (ax2.Orientation == ChartOrientation.Vertical)
		{
			rectangleF2 = ax2.Rect;
		}
		if (rectangleF == RectangleF.Empty || rectangleF2 == RectangleF.Empty)
		{
			return empty;
		}
		return new RectangleF(rectangleF.X, rectangleF2.Y, rectangleF.Width, rectangleF2.Height);
	}

	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void CalculateXZoomFactorAndZoomPosition(PointF upPoint, PointF downPoint, double zoomFact, double zoomPos, double zoomPrec, out double newZoomFact, out double newZoomPos)
	{
		if (!RealSeries3D || !m_chart.Series3D)
		{
			Rectangle renderBounds = RenderBounds;
			upPoint.X = Math.Max(renderBounds.Left, upPoint.X);
			downPoint.X = Math.Max(renderBounds.Left, downPoint.X);
			upPoint.Y = Math.Max(renderBounds.Top, upPoint.Y);
			downPoint.Y = Math.Max(renderBounds.Top, downPoint.Y);
			upPoint.X = Math.Min(renderBounds.Right, upPoint.X);
			downPoint.X = Math.Min(renderBounds.Right, downPoint.X);
			upPoint.Y = Math.Min(renderBounds.Bottom, upPoint.Y);
			downPoint.Y = Math.Min(renderBounds.Bottom, downPoint.Y);
			double num = Math.Floor(zoomPrec * (double)renderBounds.Width / Math.Abs((double)upPoint.X - (double)downPoint.X));
			newZoomFact = zoomFact / (num / zoomPrec);
			newZoomPos = zoomPos + (double)Math.Min(downPoint.X - (float)renderBounds.Left, upPoint.X - (float)renderBounds.Left) / ((double)renderBounds.Width / zoomFact);
		}
		else
		{
			PointF pointF = CorrectionTo(upPoint);
			PointF pointF2 = CorrectionTo(downPoint);
			Rectangle renderBounds2 = RenderBounds;
			pointF.X = Math.Max(renderBounds2.Left, pointF.X);
			pointF2.X = Math.Max(renderBounds2.Left, pointF2.X);
			pointF.Y = Math.Max(renderBounds2.Top, pointF.Y);
			pointF2.Y = Math.Max(renderBounds2.Top, pointF2.Y);
			pointF.X = Math.Min(renderBounds2.Right, pointF.X);
			pointF2.X = Math.Min(renderBounds2.Right, pointF2.X);
			pointF.Y = Math.Min(renderBounds2.Bottom, pointF.Y);
			pointF2.Y = Math.Min(renderBounds2.Bottom, pointF2.Y);
			double num2 = Math.Floor(zoomPrec * (double)renderBounds2.Width / Math.Abs((double)pointF.X - (double)pointF2.X));
			newZoomFact = zoomFact / (num2 / zoomPrec);
			newZoomPos = zoomPos + (double)Math.Min(pointF2.X - (float)renderBounds2.Left, pointF.X - (float)renderBounds2.Left) / ((double)renderBounds2.Width / zoomFact);
		}
	}

	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void CalculateYZoomFactorAndZoomPosition(PointF upPoint, PointF downPoint, double zoomFact, double zoomPos, double zoomPrec, out double newZoomFact, out double newZoomPos)
	{
		if (!RealSeries3D || !m_chart.Series3D)
		{
			Rectangle renderBounds = RenderBounds;
			upPoint.X = Math.Max(renderBounds.Left, upPoint.X);
			downPoint.X = Math.Max(renderBounds.Left, downPoint.X);
			upPoint.Y = Math.Max(renderBounds.Top, upPoint.Y);
			downPoint.Y = Math.Max(renderBounds.Top, downPoint.Y);
			upPoint.X = Math.Min(renderBounds.Right, upPoint.X);
			downPoint.X = Math.Min(renderBounds.Right, downPoint.X);
			upPoint.Y = Math.Min(renderBounds.Bottom, upPoint.Y);
			downPoint.Y = Math.Min(renderBounds.Bottom, downPoint.Y);
			double num = Math.Floor(zoomPrec * (double)renderBounds.Height / Math.Abs((double)upPoint.Y - (double)downPoint.Y));
			newZoomFact = zoomFact / (num / zoomPrec);
			newZoomPos = zoomPos + (double)Math.Min(upPoint.Y - (float)renderBounds.Top, downPoint.Y - (float)renderBounds.Top) / ((double)renderBounds.Height / zoomFact);
		}
		else
		{
			PointF pointF = CorrectionTo(upPoint);
			PointF pointF2 = CorrectionTo(downPoint);
			Rectangle renderBounds2 = RenderBounds;
			pointF.X = Math.Max(renderBounds2.Left, pointF.X);
			pointF2.X = Math.Max(renderBounds2.Left, pointF2.X);
			pointF.Y = Math.Max(renderBounds2.Top, pointF.Y);
			pointF2.Y = Math.Max(renderBounds2.Top, pointF2.Y);
			pointF.X = Math.Min(renderBounds2.Right, pointF.X);
			pointF2.X = Math.Min(renderBounds2.Right, pointF2.X);
			pointF.Y = Math.Min(renderBounds2.Bottom, pointF.Y);
			pointF2.Y = Math.Min(renderBounds2.Bottom, pointF2.Y);
			double num2 = Math.Floor(zoomPrec * (double)renderBounds2.Height / Math.Abs((double)pointF.Y - (double)pointF2.Y));
			newZoomFact = zoomFact / (num2 / zoomPrec);
			newZoomPos = zoomPos + (double)Math.Min(pointF.Y - (float)renderBounds2.Top, pointF2.Y - (float)renderBounds2.Top) / ((double)renderBounds2.Height / zoomFact);
		}
	}
}
