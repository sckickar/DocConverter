using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.OfficeChart;

namespace DocGen.Chart;

internal class ChartSeriesRenderer
{
	protected internal class ChartStyledPoint : ChartPointWithIndex, ICloneable
	{
		private double m_x;

		private double[] m_yValues;

		private bool m_isVisible;

		private string m_category;

		private ChartStyleInfo m_style;

		private string m_toolTip;

		private Arrow m_beginArrow;

		private Arrow m_endArrow;

		public double X
		{
			get
			{
				return m_x;
			}
			set
			{
				m_x = value;
			}
		}

		public double[] YValues
		{
			get
			{
				return m_yValues;
			}
			set
			{
				m_yValues = value;
			}
		}

		public string Category
		{
			get
			{
				return m_category;
			}
			set
			{
				m_category = value;
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
				m_isVisible = value;
			}
		}

		public ChartStyleInfo Style
		{
			get
			{
				return m_style;
			}
			set
			{
				m_style = value;
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

		public ChartStyledPoint(ChartPoint cp, int index)
			: base(cp, index)
		{
		}

		public ChartStyledPoint(ChartPoint cp, ChartStyleInfo info, int index)
			: base(cp, index)
		{
			Style = info;
			base.Point = cp;
			X = cp.X;
			YValues = cp.YValues;
			Category = cp.Category;
			IsVisible = cp.IsVisible();
			base.Index = index;
		}

		internal void Dispose()
		{
			m_yValues = null;
			if (m_style != null)
			{
				m_style.Dispose();
				m_style = null;
			}
		}

		public ChartStyledPoint Clone()
		{
			return new ChartStyledPoint(base.Point, base.Index)
			{
				m_style = m_style,
				m_toolTip = m_toolTip
			};
		}

		object ICloneable.Clone()
		{
			return Clone();
		}
	}

	protected class ChartStyledPointComparer : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			if (x != null && y != null)
			{
				ChartStyledPoint chartStyledPoint = x as ChartStyledPoint;
				ChartStyledPoint chartStyledPoint2 = y as ChartStyledPoint;
				if (chartStyledPoint.X < chartStyledPoint2.X)
				{
					return -1;
				}
				if (chartStyledPoint.X > chartStyledPoint2.X)
				{
					return 1;
				}
				if (chartStyledPoint.Index < chartStyledPoint2.Index)
				{
					return -1;
				}
				if (chartStyledPoint.Index > chartStyledPoint2.Index)
				{
					return 1;
				}
			}
			return 0;
		}
	}

	protected class ChartStyledPointComparerY : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			ChartStyledPoint chartStyledPoint = x as ChartStyledPoint;
			ChartStyledPoint chartStyledPoint2 = y as ChartStyledPoint;
			if (chartStyledPoint.YValues.Length != 0 && chartStyledPoint2.YValues.Length != 0)
			{
				if (chartStyledPoint.YValues[0] < chartStyledPoint2.YValues[0])
				{
					return -1;
				}
				if (chartStyledPoint.YValues[0] > chartStyledPoint2.YValues[0])
				{
					return 1;
				}
			}
			if (chartStyledPoint.Index < chartStyledPoint2.Index)
			{
				return -1;
			}
			if (chartStyledPoint.Index > chartStyledPoint2.Index)
			{
				return 1;
			}
			return 0;
		}
	}

	protected class ChartStyledPointComparerXDescending : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			ChartStyledPoint chartStyledPoint = x as ChartStyledPoint;
			ChartStyledPoint chartStyledPoint2 = y as ChartStyledPoint;
			if (chartStyledPoint.X < chartStyledPoint2.X)
			{
				return 1;
			}
			if (chartStyledPoint.X > chartStyledPoint2.X)
			{
				return -1;
			}
			if (chartStyledPoint.Index < chartStyledPoint2.Index)
			{
				return 1;
			}
			if (chartStyledPoint.Index > chartStyledPoint2.Index)
			{
				return -1;
			}
			return 0;
		}
	}

	protected class CategoryValueComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			if (x != null && y != null)
			{
				ChartStyledPoint obj = x as ChartStyledPoint;
				ChartStyledPoint chartStyledPoint = y as ChartStyledPoint;
				return string.Compare(obj.Category, chartStyledPoint.Category);
			}
			return 0;
		}
	}

	protected class CategoryValueComparerDescending : IComparer
	{
		public int Compare(object x, object y)
		{
			if (x != null && y != null)
			{
				ChartStyledPoint chartStyledPoint = x as ChartStyledPoint;
				return string.Compare((y as ChartStyledPoint).Category, chartStyledPoint.Category);
			}
			return 0;
		}
	}

	protected class ChartStyledPointComparerYDescending : IComparer
	{
		int IComparer.Compare(object x, object y)
		{
			ChartStyledPoint chartStyledPoint = x as ChartStyledPoint;
			ChartStyledPoint chartStyledPoint2 = y as ChartStyledPoint;
			if (chartStyledPoint.YValues[0] < chartStyledPoint2.YValues[0])
			{
				return 1;
			}
			if (chartStyledPoint.YValues[0] > chartStyledPoint2.YValues[0])
			{
				return -1;
			}
			if (chartStyledPoint.Index < chartStyledPoint2.Index)
			{
				return 1;
			}
			if (chartStyledPoint.Index > chartStyledPoint2.Index)
			{
				return -1;
			}
			return 0;
		}
	}

	protected int POLYGON_SECTORS = 16;

	protected int SPLINE_DIGITIZATION = 10;

	protected ChartSeries m_series;

	private RectangleF m_bounds;

	private IChartAreaHost m_chart;

	private ChartAxis m_xAxis;

	private ChartAxis m_yAxis;

	protected ChartSegment[] m_segments;

	private ArrayList m_styles = new ArrayList();

	private bool m_shouldUpdate = true;

	protected ChartStyleInfo m_serStyle;

	private int m_place;

	private int m_placeSize;

	private bool m_styleUpdating;

	private ChartLabelLayoutManager m_labelLayoutManager;

	private IndexRange[] m_unEmptyRanges;

	private ChartStyledPoint[] m_sPoints;

	private bool m_isSorted;

	private List<ChartStyledPoint> m_pointsCache;

	private List<int> m_pointIndex = new List<int>();

	private ChartStyledPoint[] m_styledPoint;

	private bool isStandardErrorCalculated;

	private bool isStandardDeviationCalculated;

	internal ChartStyledPoint[] m_points;

	protected ChartStyledPoint[] StyledPoints
	{
		get
		{
			return m_styledPoint;
		}
		set
		{
			m_styledPoint = value;
		}
	}

	public int Place
	{
		get
		{
			return m_place;
		}
		set
		{
			m_place = value;
		}
	}

	public int PlaceSize
	{
		get
		{
			return m_placeSize;
		}
		set
		{
			m_placeSize = value;
		}
	}

	public virtual ChartUsedSpaceType FillSpaceType => ChartUsedSpaceType.OneForOne;

	protected PointF Center => new PointF(Bounds.Left + Bounds.Width / 2f, Bounds.Top + Bounds.Height / 2f);

	internal ChartStyleInfo SeriesStyle
	{
		get
		{
			if (m_serStyle == null)
			{
				m_serStyle = m_series.GetOfflineStyle();
			}
			return m_serStyle;
		}
	}

	internal ChartSegment[] Segments => m_segments;

	protected IndexRange[] UnEmptyRanges
	{
		get
		{
			if (m_unEmptyRanges == null)
			{
				m_unEmptyRanges = CalculateUnEmptyRanges(new IndexRange(0, m_series.Points.Count - 1));
			}
			return m_unEmptyRanges;
		}
	}

	protected bool EnableStyles => m_series.EnableStyles;

	protected RectangleF Bounds => m_bounds;

	protected IChartAreaHost Chart => m_chart;

	protected IChartArea ChartArea => m_chart.GetChartArea();

	protected virtual float CustomOriginX
	{
		get
		{
			float num = 0f;
			ChartAxis xAxis = XAxis;
			if (xAxis.CustomOrigin)
			{
				return GetXFromCoordinate(xAxis.Origin);
			}
			return GetXFromCoordinate(0.0);
		}
	}

	protected virtual float CustomOriginY
	{
		get
		{
			float num = 0f;
			ChartAxis yAxis = YAxis;
			if (yAxis.CustomOrigin)
			{
				return GetYFromCoordinate(yAxis.Origin);
			}
			return GetYFromCoordinate(0.0);
		}
	}

	protected virtual bool IgnoreSeriesInversion => false;

	protected virtual PointF OriginLocation => new PointF(GetXFromCoordinate(0.0), GetYFromCoordinate(0.0));

	protected ChartAxis XAxis
	{
		get
		{
			if (m_xAxis == null)
			{
				m_xAxis = m_series.XAxis;
			}
			return m_xAxis;
		}
	}

	protected ChartAxis YAxis
	{
		get
		{
			if (m_yAxis == null)
			{
				m_yAxis = m_series.YAxis;
			}
			return m_yAxis;
		}
	}

	protected virtual string RegionDescription => "Series Chart Region";

	protected virtual int RequireYValuesCount => 1;

	protected virtual bool ShouldSort => true;

	protected virtual bool IsInvertedAxes
	{
		get
		{
			if (!m_series.RequireInvertedAxes)
			{
				if (IgnoreSeriesInversion)
				{
					return m_chart.RequireInvertedAxes;
				}
				return false;
			}
			return true;
		}
	}

	protected virtual bool IsRadial
	{
		get
		{
			if (m_series.Type != ChartSeriesType.Radar)
			{
				return m_series.Type == ChartSeriesType.Polar;
			}
			return true;
		}
	}

	public virtual SizeF IntervalSpace
	{
		get
		{
			ChartAxis xAxis = XAxis;
			ChartAxis yAxis = YAxis;
			double num = xAxis.VisibleRange.Interval * (double)m_bounds.Width / xAxis.VisibleRange.Delta;
			double num2 = yAxis.VisibleRange.Interval * (double)m_bounds.Height / yAxis.VisibleRange.Delta;
			return new SizeF((float)num, (float)num2);
		}
	}

	public SizeF DividedIntervalSpace
	{
		get
		{
			SizeF intervalSpace = IntervalSpace;
			intervalSpace.Width /= (float)XAxis.VisibleRange.Interval;
			intervalSpace.Height /= (float)YAxis.VisibleRange.Interval;
			return intervalSpace;
		}
	}

	internal double GetMinPointsDelta()
	{
		double num = double.MaxValue;
		foreach (ChartSeries item in m_chart.Series)
		{
			if (!item.Visible)
			{
				continue;
			}
			double[] array = new double[item.Points.Count];
			for (int i = 0; i < item.Points.Count; i++)
			{
				array[i] = item.Points[i].X;
			}
			Array.Sort(array);
			for (int j = 1; j < array.Length; j++)
			{
				double num2 = array[j] - array[j - 1];
				if (num2 != 0.0)
				{
					num = Math.Min(num, num2);
				}
			}
		}
		if (num != double.MaxValue)
		{
			return num;
		}
		return 1.0;
	}

	public ChartSeriesRenderer(ChartSeries series)
	{
		if (series == null)
		{
			throw new ArgumentNullException("series");
		}
		m_series = series;
	}

	internal void Dispose()
	{
		m_segments = null;
		m_styles = null;
		m_series = null;
		m_chart = null;
		if (m_xAxis != null)
		{
			m_xAxis.Dispose();
			m_xAxis = null;
		}
		if (m_yAxis != null)
		{
			m_yAxis.Dispose();
		}
		if (m_styledPoint != null)
		{
			ChartStyledPoint[] styledPoint = m_styledPoint;
			for (int i = 0; i < styledPoint.Length; i++)
			{
				styledPoint[i].Dispose();
			}
			m_styledPoint = null;
		}
		m_pointIndex = null;
		if (m_pointsCache != null)
		{
			foreach (ChartStyledPoint item in m_pointsCache)
			{
				item.Dispose();
			}
			m_pointsCache = null;
		}
		m_points = null;
		m_unEmptyRanges = null;
		m_labelLayoutManager = null;
	}

	public virtual void Render(ChartRenderArgs2D args)
	{
	}

	public virtual void Render(ChartRenderArgs3D args)
	{
	}

	public virtual void Render(Graphics g)
	{
		ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(m_chart, m_series);
		chartRenderArgs2D.Graph = new ChartGDIGraph(g);
		chartRenderArgs2D.Bounds = m_bounds;
		if (chartRenderArgs2D.Is3D)
		{
			chartRenderArgs2D.Offset = GetThisOffset();
			chartRenderArgs2D.DepthOffset = GetSeriesOffset();
		}
		if (chartRenderArgs2D.Chart.Style3D && m_series.Style.Border.Color == SystemColors.ControlText && m_series.Type != ChartSeriesType.BoxAndWhisker && m_series.Type != ChartSeriesType.Candle)
		{
			m_series.Style.Border.Color = Color.Transparent;
		}
		Render(chartRenderArgs2D);
		if (m_series.BaseType == ChartSeriesBaseType.Circular || m_series.BaseType == ChartSeriesBaseType.Single)
		{
			return;
		}
		foreach (Trendline trendline in m_series.Trendlines)
		{
			if (trendline.Visible && !chartRenderArgs2D.Is3D)
			{
				trendline.TrendlineDraw(chartRenderArgs2D, m_series);
			}
		}
	}

	public virtual void RenderSeriesNameInDepth(Graphics g)
	{
		if (m_series.DrawSeriesNameInDepth && m_chart.Series3D)
		{
			SizeF seriesOffset = GetSeriesOffset();
			SizeF thisOffset = GetThisOffset();
			ChartPoint cp = new ChartPoint(m_series.ActualXAxis.VisibleRange.Min, m_series.ActualYAxis.VisibleRange.Min);
			PointF pointF = new PointF(GetXFromValue(cp, 0) + thisOffset.Width + seriesOffset.Width / 2f, GetYFromValue(cp, 0) + thisOffset.Height + seriesOffset.Height / 2f);
			_ = SeriesStyle;
			Size size = g.MeasureString(m_series.Name, SeriesStyle.Font.GdipFont).ToSize();
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddString(m_series.Name, SeriesStyle.Font.GdipFont.GetFontFamily(), (int)SeriesStyle.Font.GdipFont.Style, RenderingHelper.GetFontSizeInPixels(SeriesStyle.Font.GdipFont), new PointF(pointF.X, pointF.Y - (float)(size.Height / 2)), Graphics3D.DefaultStrinfFormat);
			if (graphicsPath.PointCount > 0)
			{
				g.FillPath(new SolidBrush(SeriesStyle.TextColor), graphicsPath);
			}
		}
	}

	public virtual void Render(Graphics3D g)
	{
		ChartRenderArgs3D chartRenderArgs3D = new ChartRenderArgs3D(m_chart, m_series);
		chartRenderArgs3D.Graph = g;
		chartRenderArgs3D.Z = GetPlaceDepth();
		chartRenderArgs3D.Depth = GetSeriesDepth();
		chartRenderArgs3D.Bounds = m_bounds;
		Render(chartRenderArgs3D);
	}

	public virtual void RenderSeriesNameInDepth(Graphics3D g)
	{
		if (m_series.DrawSeriesNameInDepth && m_chart.Series3D)
		{
			float placeDepth = GetPlaceDepth();
			float seriesDepth = GetSeriesDepth();
			float num = placeDepth + seriesDepth / 2f;
			ChartPoint cp = new ChartPoint(m_series.ActualXAxis.VisibleRange.Min, m_series.ActualYAxis.VisibleRange.Min);
			ChartPoint cp2 = new ChartPoint(m_series.ActualXAxis.VisibleRange.Max, m_series.ActualYAxis.VisibleRange.Max);
			PointF pointF = new PointF(GetXFromValue(cp, 0), GetYFromValue(cp, 0));
			PointF pointF2 = new PointF(GetXFromValue(cp2, 0), GetYFromValue(cp2, 0));
			_ = SeriesStyle;
			Size size = g.Graphics.MeasureString(m_series.Name, SeriesStyle.Font.GdipFont).ToSize();
			GraphicsPath graphicsPath = new GraphicsPath();
			graphicsPath.AddString(m_series.Name, SeriesStyle.Font.GdipFont.GetFontFamily(), (int)SeriesStyle.Font.GdipFont.Style, RenderingHelper.GetFontSizeInPixels(SeriesStyle.Font.GdipFont), new PointF(0f, -size.Height / 2), Graphics3D.DefaultStrinfFormat);
			if (graphicsPath.PointCount > 0)
			{
				g.AddPolygon(new Polygon(new Vector3D[4]
				{
					new Vector3D(pointF.X, pointF.Y, placeDepth),
					new Vector3D(pointF.X, pointF.Y, placeDepth + seriesDepth),
					new Vector3D(pointF2.X, pointF2.Y, placeDepth + seriesDepth),
					new Vector3D(pointF2.X, pointF2.Y, placeDepth)
				}));
				Path3D path3D = Path3D.FromGraphicsPath(graphicsPath, 0.0, new SolidBrush(SeriesStyle.TextColor));
				path3D.Transform(Matrix3D.RotateAlongOX(m_series.SeriesNameOXAngle * (float)Math.PI / 180f));
				path3D.Transform(Matrix3D.Transform(pointF.X, pointF.Y, num));
				g.AddPolygon(path3D);
			}
		}
	}

	public virtual void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		if (isShadow)
		{
			using (SolidBrush brush = new SolidBrush(shadowColor))
			{
				g.FillRectangle(brush, bounds);
				return;
			}
		}
		BrushPaint.FillRectangle(g, bounds, SeriesStyle.Interior);
		g.DrawRectangle(SeriesStyle.GdipPen, bounds);
	}

	public virtual void DrawIcon(int index, Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		if (isShadow)
		{
			using (SolidBrush brush = new SolidBrush(shadowColor))
			{
				g.FillRectangle(brush, bounds);
				return;
			}
		}
		ChartStyleInfo styleAt = GetStyleAt(index);
		BrushPaint.FillRectangle(g, bounds, styleAt.Interior);
		g.DrawRectangle(styleAt.GdipPen, bounds);
	}

	public virtual bool CanRender()
	{
		StyledPoints = PrepearePoints();
		ChartStyledPoint[] styledPoints = StyledPoints;
		foreach (ChartStyledPoint chartStyledPoint in styledPoints)
		{
			if (chartStyledPoint != null && chartStyledPoint.IsVisible)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void StackSorting()
	{
		m_sPoints = m_pointsCache.ToArray();
		if (m_series.ActualXAxis.ValueType != ChartValueType.Category)
		{
			_ = new Array[m_points.Length];
			bool flag = true;
			{
				foreach (ChartSeries item in m_chart.Series)
				{
					if (!flag)
					{
						continue;
					}
					int count = item.Points.Count;
					double[] array = new double[count];
					double[] array2 = new double[count];
					for (int i = 0; i < count; i++)
					{
						array2[i] = 0.0;
						double x = item.Points[i].X;
						foreach (ChartSeries item2 in m_chart.Series)
						{
							if (!(m_series.StackingGroup == item2.StackingGroup))
							{
								continue;
							}
							for (int j = 0; j < item2.Points.Count; j++)
							{
								if (item2.Points[j].IsVisible() && !item2.Points[j].IsEmpty && x == item2.Points[j].X)
								{
									array2[i] += item2.Points[j].YValues[0];
									break;
								}
							}
						}
						array[i] = item.Points[i].X;
					}
					Array.Sort(array2, array);
					if (m_series.SortOrder == ChartSeriesSortingOrder.Descending)
					{
						Array.Reverse(array2);
						Array.Reverse(array);
					}
					for (int k = 0; k < count; k++)
					{
						for (int l = 0; l < count; l++)
						{
							if (array[k] == m_sPoints[l].X)
							{
								m_points[k] = m_sPoints[l];
								break;
							}
						}
					}
					flag = false;
				}
				return;
			}
		}
		ArrayList arrayList = new ArrayList();
		arrayList.AddRange(m_series.ActualXAxis.SortLabels);
		int m = 0;
		int num = 0;
		for (; m < m_sPoints.Length; m++)
		{
			int num2 = arrayList.IndexOf(m_sPoints[m].Category);
			if (num2 != -1)
			{
				m_points[num] = m_sPoints[m];
				m_points[num].X = num2;
				m_points[num].Point = new ChartPoint(m_points[num].X, m_points[num].YValues);
				num++;
			}
		}
	}

	public virtual SizeF GetMinSize(Graphics g)
	{
		if (Chart == null)
		{
			return new SizeF(200f, 100f);
		}
		return ChartArea.MinSize;
	}

	public void SetChart(IChartAreaHost chart)
	{
		m_chart = chart;
		m_xAxis = chart.GetChartArea().GetXAxis(m_series);
		m_yAxis = chart.GetChartArea().GetYAxis(m_series);
	}

	public virtual PointF GetCharacterPoint(int index)
	{
		PointF empty = PointF.Empty;
		if (ChartArea.Series3D && ChartArea.RealSeries3D)
		{
			Vector3D symbolVector = GetSymbolVector(GetStyledPoint(index));
			return ChartArea.Transform3D.ToScreen(symbolVector);
		}
		return GetSymbolPoint(GetStyledPoint(index));
	}

	internal virtual void Update(ChartUpdateFlags flags)
	{
		if (m_styleUpdating)
		{
			return;
		}
		if ((flags & ChartUpdateFlags.Data) != 0)
		{
			m_points = null;
			m_unEmptyRanges = null;
		}
		if ((flags & ChartUpdateFlags.Styles) != 0)
		{
			m_shouldUpdate = true;
			m_serStyle = null;
		}
		if ((flags & ChartUpdateFlags.Indexed) != 0 && m_points != null)
		{
			ChartStyledPoint[] points = m_points;
			foreach (ChartStyledPoint chartStyledPoint in points)
			{
				chartStyledPoint.X = GetIndexValueFromX(chartStyledPoint.Point.X);
			}
		}
	}

	internal virtual void DataUpdate(ListChangedEventArgs args)
	{
		m_points = null;
		m_unEmptyRanges = null;
		switch (args.ListChangedType)
		{
		case ListChangedType.ItemAdded:
			InsertPoint(args.NewIndex);
			break;
		case ListChangedType.ItemChanged:
			UpdatePoint(args.NewIndex);
			break;
		case ListChangedType.ItemDeleted:
			RemovePoint(args.NewIndex);
			break;
		case ListChangedType.Reset:
			ResetCache();
			break;
		case ListChangedType.ItemMoved:
			break;
		}
	}

	internal void DrawArrows(Graphics g)
	{
		for (int i = 0; i < StyledPoints.Length - 1; i++)
		{
			ChartPoint point = StyledPoints[i].Point;
			ChartPoint point2 = StyledPoints[i + 1].Point;
			PointF pointF = new PointF(GetXFromValue(point, 0), GetYFromValue(point, 0));
			PointF pointF2 = new PointF(GetXFromValue(point2, 0), GetYFromValue(point2, 0));
			float num = (float)Math.Atan2(pointF2.Y - pointF.Y, pointF2.X - pointF.X) * (180f / (float)Math.PI);
			Color color = StyledPoints[i + 1].Style.Border.Color;
			if (StyledPoints[i + 1].BeginArrow != null && StyledPoints[i + 1].BeginArrow.Type != OfficeArrowType.None)
			{
				Arrow beginArrow = StyledPoints[i + 1].BeginArrow;
				switch (beginArrow.Type)
				{
				case OfficeArrowType.OvalArrow:
					DrawFilledOval(g, pointF, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num + 90f);
					break;
				case OfficeArrowType.OpenArrow:
				{
					GraphicsState state = g.Save();
					DrawOpenArrow(g, pointF2, pointF, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, 0f);
					g.Restore(state);
					break;
				}
				case OfficeArrowType.Arrow:
				{
					GraphicsState state = g.Save();
					DrawSimpleArrow(g, pointF2, pointF, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, 0f);
					g.Restore(state);
					break;
				}
				case OfficeArrowType.StealthArrow:
				{
					GraphicsState state = g.Save();
					DrawStealthArrow(g, pointF2, pointF, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, 0f);
					g.Restore(state);
					break;
				}
				case OfficeArrowType.DiamondArrow:
					num = (float)Math.Atan2(pointF2.Y - pointF.Y, pointF2.X - pointF.X);
					DrawDiamond(g, pointF, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num);
					break;
				}
			}
			num = (float)Math.Atan2(pointF2.Y - pointF.Y, pointF2.X - pointF.X) * (180f / (float)Math.PI);
			if (StyledPoints[i + 1].EndArrow != null && StyledPoints[i + 1].EndArrow.Type != OfficeArrowType.None)
			{
				Arrow endArrow = StyledPoints[i + 1].EndArrow;
				switch (endArrow.Type)
				{
				case OfficeArrowType.OvalArrow:
					DrawFilledOval(g, pointF2, endArrow.ArrowWidth, endArrow.ArrowLength, color, num + 90f);
					break;
				case OfficeArrowType.OpenArrow:
				{
					GraphicsState state2 = g.Save();
					DrawOpenArrow(g, pointF, pointF2, endArrow.ArrowWidth, endArrow.ArrowLength, color, 0f);
					g.Restore(state2);
					break;
				}
				case OfficeArrowType.Arrow:
				{
					GraphicsState state2 = g.Save();
					DrawSimpleArrow(g, pointF, pointF2, endArrow.ArrowWidth, endArrow.ArrowLength, color, 0f);
					g.Restore(state2);
					break;
				}
				case OfficeArrowType.StealthArrow:
				{
					GraphicsState state2 = g.Save();
					DrawStealthArrow(g, pointF, pointF2, endArrow.ArrowWidth, endArrow.ArrowLength, color, 0f);
					g.Restore(state2);
					break;
				}
				case OfficeArrowType.DiamondArrow:
					num = (float)Math.Atan2(pointF2.Y - pointF.Y, pointF2.X - pointF.X);
					DrawDiamond(g, pointF2, endArrow.ArrowWidth, endArrow.ArrowLength, color, num);
					break;
				}
			}
		}
	}

	internal void DrawOpenArrow(Graphics g, PointF startPoint, PointF endPoint, float arrowWidth, float arrowLength, Color arrowColor, float angle2)
	{
		using GraphicsPath graphicsPath = new GraphicsPath();
		using Pen pen = new Pen(arrowColor, 1.5f)
		{
			EndCap = DocGen.Drawing.LineCap.Round
		};
		float num = (float)Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2.0) + Math.Pow(endPoint.Y - startPoint.Y, 2.0));
		float num2 = (float)Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
		PointF pointF = new PointF(startPoint.X + num * (float)Math.Cos(num2), startPoint.Y + num * (float)Math.Sin(num2));
		PointF point = new PointF(pointF.X - arrowLength, pointF.Y - arrowWidth / 2f);
		PointF point2 = new PointF(pointF.X - arrowLength, pointF.Y + arrowWidth / 2f);
		if (angle2 != 0f)
		{
			num2 = angle2;
		}
		point = RotatePoint(point, pointF, num2);
		point2 = RotatePoint(point2, pointF, num2);
		graphicsPath.AddLine(point, pointF);
		graphicsPath.AddLine(point2, pointF);
		g.DrawPath(pen, graphicsPath);
	}

	internal void DrawSimpleArrow(Graphics g, PointF startPoint, PointF endPoint, float arrowWidth, float arrowLength, Color arrowColor, float angle2)
	{
		using GraphicsPath graphicsPath = new GraphicsPath();
		using Pen pen = new Pen(arrowColor, 0.5f);
		using SolidBrush brush = new SolidBrush(arrowColor);
		float num = (float)Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2.0) + Math.Pow(endPoint.Y - startPoint.Y, 2.0));
		float num2 = (float)Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
		PointF pointF = new PointF(startPoint.X + num * (float)Math.Cos(num2), startPoint.Y + num * (float)Math.Sin(num2));
		PointF point = new PointF(pointF.X - arrowLength, pointF.Y - arrowWidth / 2f);
		PointF point2 = new PointF(pointF.X - arrowLength, pointF.Y + arrowWidth / 2f);
		if (angle2 != 0f)
		{
			num2 = angle2;
		}
		point = RotatePoint(point, pointF, num2);
		point2 = RotatePoint(point2, pointF, num2);
		graphicsPath.AddPolygon(new PointF[3] { pointF, point, point2 });
		g.DrawPath(pen, graphicsPath);
		g.FillPath(brush, graphicsPath);
	}

	internal void DrawFilledOval(Graphics g, PointF center, float width, float height, Color fillColor, float angle)
	{
		RectangleF rectangleF = new RectangleF(center.X - width / 2f, center.Y - height / 2f, width, height);
		using SolidBrush brush = new SolidBrush(fillColor);
		GraphicsState state = g.Save();
		g.TranslateTransform(center.X, center.Y);
		g.RotateTransform(angle);
		g.TranslateTransform(0f - center.X, 0f - center.Y);
		g.FillEllipse(brush, rectangleF);
		g.Restore(state);
	}

	internal void DrawStealthArrow(Graphics g, PointF startPoint, PointF endPoint, float arrowWidth, float arrowLength, Color arrowColor, float angle2)
	{
		using GraphicsPath graphicsPath = new GraphicsPath();
		using SolidBrush brush = new SolidBrush(arrowColor);
		float num = (float)Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2.0) + Math.Pow(endPoint.Y - startPoint.Y, 2.0)) + 1f;
		float num2 = (float)Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X);
		PointF pointF = new PointF(startPoint.X + num * (float)Math.Cos(num2), startPoint.Y + num * (float)Math.Sin(num2));
		PointF point = new PointF(pointF.X - arrowLength, pointF.Y - arrowWidth / 2f);
		PointF point2 = new PointF(pointF.X - arrowLength, pointF.Y + arrowWidth / 2f);
		if (angle2 != 0f)
		{
			num2 = angle2;
		}
		point = RotatePoint(point, pointF, num2);
		point2 = RotatePoint(point2, pointF, num2);
		PointF pointF2 = new PointF((pointF.X + point.X + point2.X) / 3f, (pointF.Y + point.Y + point2.Y) / 3f);
		graphicsPath.AddPolygon(new PointF[4] { pointF, point, pointF2, point2 });
		g.FillPath(brush, graphicsPath);
	}

	internal void DrawDiamond(Graphics g, PointF center, float width, float height, Color fillColor, float angle)
	{
		using SolidBrush pen = new SolidBrush(fillColor);
		PointF point = new PointF(center.X, center.Y - width / 2f);
		PointF point2 = new PointF(center.X, center.Y + width / 2f);
		PointF point3 = new PointF(center.X - height / 2f, center.Y);
		PointF point4 = new PointF(center.X + height / 2f, center.Y);
		point = RotatePoint(point, center, angle);
		point2 = RotatePoint(point2, center, angle);
		point3 = RotatePoint(point3, center, angle);
		point4 = RotatePoint(point4, center, angle);
		PointF[] points = new PointF[4] { point, point4, point2, point3 };
		g.FillPolygon(pen, points);
	}

	internal PointF RotatePoint(PointF point, PointF center, float angle)
	{
		float x = center.X + (point.X - center.X) * (float)Math.Cos(angle) - (point.Y - center.Y) * (float)Math.Sin(angle);
		float y = center.Y + (point.X - center.X) * (float)Math.Sin(angle) + (point.Y - center.Y) * (float)Math.Cos(angle);
		return new PointF(x, y);
	}

	public virtual void DrawChartPoint(Graphics g, ChartPoint point, ChartStyleInfo info, int pointIndex)
	{
	}

	protected PointF VisiblePoint(IList points, int index, out int n, bool first)
	{
		int num = (first ? 1 : (-1));
		int num2 = (first ? points.Count : 0);
		for (int i = index + num; first ? (i < num2) : (i > num2); i += num)
		{
			if (IsVisiblePoint((ChartPoint)points[i]))
			{
				n = (first ? i : (i + 1));
				return GetPointFromIndex(i);
			}
		}
		n = index;
		return PointF.Empty;
	}

	protected int VisiblePointIndex(IList points, int index, bool first)
	{
		int num = (first ? 1 : (-1));
		int num2 = (first ? points.Count : 0);
		bool flag = points[index].GetType().Name == "ChartStyledPoint";
		for (int i = index + num; first ? (i < num2) : (i > num2); i += num)
		{
			if (flag)
			{
				if (((ChartStyledPoint)points[i]).IsVisible)
				{
					if (!first)
					{
						return i + 1;
					}
					return i;
				}
			}
			else if (IsVisiblePoint(((ChartPointWithIndex)points[i]).Point))
			{
				if (!first)
				{
					return i + 1;
				}
				return i;
			}
		}
		return 0;
	}

	protected ChartPoint VisibleChartPoint(IList points, int index, out int n, bool first)
	{
		int num = (first ? 1 : (-1));
		int num2 = (first ? points.Count : 0);
		for (int i = index + num; first ? (i < num2) : (i > num2); i += num)
		{
			ChartPoint chartPoint = (ChartPoint)points[i];
			if (IsVisiblePoint(chartPoint))
			{
				n = (first ? i : (i + 1));
				return chartPoint;
			}
		}
		n = index;
		return new ChartPoint();
	}

	protected ChartStyledPoint VisibleChartPoint(ChartStyledPoint[] points, int index, out int n, bool first)
	{
		int num = (first ? 1 : (-1));
		int num2 = (first ? points.Length : 0);
		for (int i = index + num; first ? (i < num2) : (i > num2); i += num)
		{
			ChartStyledPoint chartStyledPoint = points[i];
			if (chartStyledPoint.IsVisible)
			{
				n = (first ? i : (i + 1));
				return chartStyledPoint;
			}
		}
		n = index;
		return points[index];
	}

	protected PointF[] RemoveDuplicates(PointF[] points)
	{
		List<PointF> list = new List<PointF>();
		foreach (PointF item in points)
		{
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	protected ChartPointWithIndex[] GetVisiblePoints(ChartPointWithIndex[] list)
	{
		List<ChartPointWithIndex> list2 = new List<ChartPointWithIndex>();
		int num = list.Length;
		for (int i = 0; i < num; i++)
		{
			if (IsVisiblePoint(list[i].Point))
			{
				list2.Add(list[i]);
			}
		}
		return list2.ToArray();
	}

	protected ChartPoint[] GetVisiblePoints(ChartPointIndexer list)
	{
		List<ChartPoint> list2 = new List<ChartPoint>();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (IsVisiblePoint(list[i]))
			{
				list2.Add(list[i]);
			}
		}
		return list2.ToArray();
	}

	protected ChartStyledPoint[] GetVisiblePoints(ChartStyledPoint[] list)
	{
		List<ChartStyledPoint> list2 = new List<ChartStyledPoint>();
		int num = list.Length;
		for (int i = 0; i < num; i++)
		{
			if (list[i].IsVisible)
			{
				list2.Add(list[i]);
			}
		}
		return list2.ToArray();
	}

	[Obsolete("This method isn't used anymore. Use GetCharacterPoint method.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual PointF GetPointByValueForSeries(ChartPoint chpt)
	{
		PointF pointFromValue = GetPointFromValue(chpt);
		if (ChartArea.Series3D && ChartArea.RealSeries3D)
		{
			double z = GetPlaceDepth();
			pointFromValue = ChartArea.Transform3D.ToScreen(new Vector3D(pointFromValue.X, pointFromValue.Y, z));
		}
		else
		{
			SizeF thisOffset = GetThisOffset();
			pointFromValue = new PointF(pointFromValue.X + thisOffset.Width, pointFromValue.Y + thisOffset.Height);
		}
		return pointFromValue;
	}

	protected PointF[] GetOffsetPoints(PointF[] points, SizeF offset)
	{
		PointF[] array = new PointF[points.Length];
		for (int i = 0; i < points.Length; i++)
		{
			array[i] = new PointF(points[i].X + offset.Width, points[i].Y + offset.Height);
		}
		return array;
	}

	protected virtual SizeF GetSeriesOffset()
	{
		if (Chart.Series3D)
		{
			float num = (1f - 0.01f * m_chart.SpacingBetweenSeries) / (float)m_placeSize;
			return new SizeF(num * ChartArea.OffsetX, (0f - num) * ChartArea.OffsetY);
		}
		return SizeF.Empty;
	}

	protected virtual float GetSeriesDepth()
	{
		if (Chart.Series3D)
		{
			return (100f - 2f * m_chart.SpacingBetweenSeries) / 100f * ChartArea.Depth / (float)PlaceSize;
		}
		return 0f;
	}

	protected virtual SizeF GetThisOffset()
	{
		if (Chart.Series3D)
		{
			float num = ((float)m_place + 0.005f * m_chart.SpacingBetweenSeries) / (float)m_placeSize;
			return new SizeF(num * ChartArea.OffsetX, (0f - num) * ChartArea.OffsetY);
		}
		return SizeF.Empty;
	}

	protected virtual float GetPlaceDepth()
	{
		float result = 0f;
		if (Chart.Series3D)
		{
			result = ((float)m_place + m_chart.SpacingBetweenSeries / 200f) * ChartArea.Depth / (float)m_placeSize;
		}
		return result;
	}

	protected virtual void CalculateStepPointsForSeries3D(ref PointF[] stepPoints)
	{
		if (Chart.Series3D)
		{
			CalculateStepPointsForSeries3D(ref stepPoints, GetThisOffset());
		}
	}

	protected virtual void CalculateStepPointsForSeries3D(ref PointF[] stepPoints, SizeF offset)
	{
		for (int i = 0; i < stepPoints.Length; i++)
		{
			stepPoints[i].X += offset.Width;
			stepPoints[i].Y += offset.Height;
		}
	}

	protected virtual BrushInfo GetUpPriceInterior(BrushInfo original)
	{
		BrushInfo result = null;
		switch (m_series.ConfigItems.FinancialItem.ColorsMode)
		{
		case ChartFinancialColorMode.Fixed:
			result = new BrushInfo(m_series.ConfigItems.FinancialItem.PriceUpColor);
			break;
		case ChartFinancialColorMode.Mixed:
			result = DrawingHelper.AddColor(original, m_series.ConfigItems.FinancialItem.PriceUpColor);
			break;
		case ChartFinancialColorMode.DarkLight:
			result = DrawingHelper.AddColor(original, m_series.ConfigItems.FinancialItem.DarkLightPower);
			break;
		}
		return result;
	}

	protected virtual BrushInfo GetDownPriceInterior(BrushInfo original)
	{
		BrushInfo result = null;
		switch (m_series.ConfigItems.FinancialItem.ColorsMode)
		{
		case ChartFinancialColorMode.Fixed:
			result = new BrushInfo(m_series.ConfigItems.FinancialItem.PriceDownColor);
			break;
		case ChartFinancialColorMode.Mixed:
			result = DrawingHelper.AddColor(original, m_series.ConfigItems.FinancialItem.PriceDownColor);
			break;
		case ChartFinancialColorMode.DarkLight:
			result = DrawingHelper.AddColor(original, -m_series.ConfigItems.FinancialItem.DarkLightPower);
			break;
		}
		return result;
	}

	protected virtual Region Draw3DSpline(Graphics g, ChartPointWithIndex[] points, double[] y2, SizeF offset, BrushInfo brush, Pen pen)
	{
		SizeF thisOffset = GetThisOffset();
		Region region = new Region(new Rectangle(0, 0, 0, 0));
		Pen pen2 = new Pen(brush.BackColor, pen.Width);
		if (points.Length != y2.Length)
		{
			return region;
		}
		AddExtremumPoints(points, y2, out var pointsNew, out var y2New);
		GraphicsPath graphicsPath = new GraphicsPath();
		int num;
		int i = (num = 0);
		int num2 = 1;
		int num3 = pointsNew.Length - 1;
		if (XAxis.Inversed)
		{
			i = pointsNew.Length - 1;
			num = points.Length - 1;
			num2 = -1;
			num3 = 0;
		}
		for (; i != num3; i += num2)
		{
			BezierPointsFromSpline(pointsNew[i], pointsNew[i + num2], y2New[i], y2New[i + num2], out var p, out var p2, out var p3, out var p4);
			PointF pointF = new PointF(GetXFromValue(p.Point, 0) + thisOffset.Width, GetYFromValue(p.Point, 0) + thisOffset.Height);
			PointF pointF2 = new PointF(GetXFromValue(p2.Point, 0) + thisOffset.Width, GetYFromValue(p2.Point, 0) + thisOffset.Height);
			PointF pointF3 = new PointF(GetXFromValue(p3.Point, 0) + thisOffset.Width, GetYFromValue(p3.Point, 0) + thisOffset.Height);
			PointF pointF4 = new PointF(GetXFromValue(p4.Point, 0) + thisOffset.Width, GetYFromValue(p4.Point, 0) + thisOffset.Height);
			graphicsPath.AddBezier(pointF, pointF2, pointF3, pointF4);
			graphicsPath.AddLine(pointF4.X, pointF4.Y, pointF4.X + offset.Width, pointF4.Y + offset.Height);
			graphicsPath.AddBezier(pointF4.X + offset.Width, pointF4.Y + offset.Height, pointF3.X + offset.Width, pointF3.Y + offset.Height, pointF2.X + offset.Width, pointF2.Y + offset.Height, pointF.X + offset.Width, pointF.Y + offset.Height);
			graphicsPath.CloseAllFigures();
			ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(Chart, m_series);
			chartRenderArgs2D.Graph = new ChartGDIGraph(g);
			if (!Chart.Style3D)
			{
				BrushPaint.FillPath(g, graphicsPath, brush);
			}
			else
			{
				Draw(chartRenderArgs2D.Graph, graphicsPath, brush, pen);
			}
			region.Union(graphicsPath);
			graphicsPath.Reset();
			try
			{
				g.DrawBezier(pen, pointF, pointF2, pointF3, pointF4);
			}
			catch (Exception)
			{
				pointF = new PointF((float)Math.Round(pointF.X), (float)Math.Round(pointF.Y));
				pointF2 = new PointF((float)Math.Round(pointF2.X), (float)Math.Round(pointF2.Y));
				pointF3 = new PointF((float)Math.Round(pointF3.X), (float)Math.Round(pointF3.Y));
				pointF4 = new PointF((float)Math.Round(pointF4.X), (float)Math.Round(pointF4.Y));
				g.DrawBezier(pen, pointF, pointF2, pointF3, pointF4);
			}
			g.DrawBezier(pen, pointF4.X + offset.Width, pointF4.Y + offset.Height, pointF3.X + offset.Width, pointF3.Y + offset.Height, pointF2.X + offset.Width, pointF2.Y + offset.Height, pointF.X + offset.Width, pointF.Y + offset.Height);
			if (pointsNew[i].Point.X == points[num].Point.X)
			{
				pointF = new PointF(GetXFromValue(points[num].Point, 0) + thisOffset.Width, GetYFromValue(points[num].Point, 0) + thisOffset.Height);
				pointF4 = new PointF(GetXFromValue(points[num + num2].Point, 0) + thisOffset.Width, GetYFromValue(points[num + num2].Point, 0) + thisOffset.Height);
				g.DrawLine(pen, pointF.X, pointF.Y, pointF.X + offset.Width, pointF.Y + offset.Height);
				g.DrawLine(pen, pointF4.X, pointF4.Y, pointF4.X + offset.Width, pointF4.Y + offset.Height);
				num += num2;
			}
			else
			{
				pen2.Color = GetBrush(num).BackColor;
				if (!Chart.Style3D)
				{
					g.DrawLine(pen2, pointF.X, pointF.Y, pointF.X + offset.Width, pointF.Y + offset.Height);
				}
			}
		}
		return region;
	}

	protected void AddExtremumPoints(ChartPointWithIndex[] points, double[] y2, out ChartPointWithIndex[] pointsNew, out double[] y2New)
	{
		int num = points.Length;
		ArrayList arrayList = new ArrayList(num * 2);
		ArrayList arrayList2 = new ArrayList(num * 2);
		double num2 = 0.0;
		for (int i = 0; i <= num - 2; i++)
		{
			arrayList.Add(points[i]);
			arrayList2.Add(y2[i]);
			double num3 = points[i + 1].Point.X - points[i].Point.X;
			num3 *= num3;
			double num4 = 0.5 * num3 * (y2[i + 1] - y2[i]);
			double num5 = num3 * y2[i];
			double num6 = points[i + 1].Point.YValues[0] - points[i].Point.YValues[0] - num3 * (1.0 / 3.0 * y2[i] + 1.0 / 6.0 * y2[i + 1]);
			if (ChartMath.SolveQuadraticEquation(num4, num5, num6, out var root, out var root2))
			{
				if (root > 0.0 && root < 1.0)
				{
					num2 = points[i].Point.YValues[0] + root * (num6 + root * (0.5 * num5 + root * 1.0 / 3.0 * num4));
					arrayList.Add(new ChartPointWithIndex(new ChartPoint(points[i].Point.X + root * (points[i + 1].Point.X - points[i].Point.X), num2), points[i].Index));
					arrayList2.Add(y2[i] + root * (y2[i + 1] - y2[i]));
				}
				if (root2 > 0.0 && root2 < 1.0)
				{
					num2 = points[i].Point.YValues[0] + root2 * (num6 + root2 * (0.5 * num5 + root2 * 1.0 / 3.0 * num4));
					arrayList.Add(new ChartPointWithIndex(new ChartPoint(points[i].Point.X + root2 * (points[i + 1].Point.X - points[i].Point.X), num2), points[i + 1].Index));
					arrayList2.Add(y2[i] + root2 * (y2[i + 1] - y2[i]));
				}
			}
		}
		arrayList.Add(points[num - 1]);
		arrayList2.Add(y2[num - 1]);
		int count = arrayList2.Count;
		y2New = new double[count];
		for (int j = 0; j < count; j++)
		{
			y2New[j] = (double)arrayList2[j];
		}
		count = arrayList.Count;
		pointsNew = new ChartPointWithIndex[count];
		for (int k = 0; k < count; k++)
		{
			pointsNew[k] = (ChartPointWithIndex)arrayList[k];
		}
	}

	protected void NaturalSpline(ChartPointWithIndex[] points, out double[] ys2)
	{
		int num = points.Length;
		int num2 = m_series.PointFormats[ChartYValueUsage.YValue];
		ys2 = new double[num];
		double num3 = 6.0;
		double[] array = new double[num - 1];
		ys2[0] = (array[0] = 0.0);
		ys2[num - 1] = 0.0;
		for (int i = 1; i < num - 1; i++)
		{
			double num4 = points[i].Point.X - points[i - 1].Point.X;
			double num5 = points[i + 1].Point.X - points[i - 1].Point.X;
			double num6 = points[i + 1].Point.X - points[i].Point.X;
			double num7 = points[i + 1].Point.YValues[num2] - points[i].Point.YValues[num2];
			double num8 = points[i].Point.YValues[num2] - points[i - 1].Point.YValues[num2];
			if (points[i].Point.X == points[i - 1].Point.X)
			{
				ys2[i] = 0.0;
				array[i] = 0.0;
			}
			else
			{
				double num9 = 1.0 / (num4 * ys2[i - 1] + 2.0 * num5);
				ys2[i] = (0.0 - num9) * num6;
				array[i] = num9 * (num3 * (num7 / num6 - num8 / num4) - num4 * array[i - 1]);
			}
		}
		for (int num10 = num - 2; num10 >= 0; num10--)
		{
			ys2[num10] = ys2[num10] * ys2[num10 + 1] + array[num10];
		}
	}

	protected void NaturalSpline(ChartStyledPoint[] points, out double[] ys2)
	{
		int num = points.Length;
		int num2 = m_series.PointFormats[ChartYValueUsage.YValue];
		ys2 = new double[num];
		double num3 = 6.0;
		double[] array = new double[num - 1];
		ys2[0] = (array[0] = 0.0);
		ys2[num - 1] = 0.0;
		for (int i = 1; i < num - 1; i++)
		{
			double num4 = points[i].X - points[i - 1].X;
			double num5 = points[i + 1].X - points[i - 1].X;
			double num6 = points[i + 1].X - points[i].X;
			double num7 = points[i + 1].YValues[num2] - points[i].YValues[num2];
			double num8 = points[i].YValues[num2] - points[i - 1].YValues[num2];
			if (num4 == 0.0 || num5 == 0.0 || num6 == 0.0)
			{
				ys2[i] = 0.0;
				array[i] = 0.0;
			}
			else
			{
				double num9 = 1.0 / (num4 * ys2[i - 1] + 2.0 * num5);
				ys2[i] = (0.0 - num9) * num6;
				array[i] = num9 * (num3 * (num7 / num6 - num8 / num4) - num4 * array[i - 1]);
			}
		}
		for (int num10 = num - 2; num10 >= 0; num10--)
		{
			ys2[num10] = ys2[num10] * ys2[num10 + 1] + array[num10];
		}
	}

	protected void BezierPointsFromSpline(PointF point1, PointF point2, float y2_1, float y2_2, out PointF p0, out PointF p1, out PointF p2, out PointF p3)
	{
		float num = point2.X - point1.X;
		num *= num;
		float num2 = 0.33333f;
		p0 = new PointF(point1.X, point1.Y);
		p1 = new PointF((2f * point1.X + point2.X) * num2, num2 * (2f * point1.Y + point2.Y - num2 * num * (y2_1 + 0.5f * y2_2)));
		p2 = new PointF((point1.X + 2f * point2.X) * num2, num2 * (point1.Y + 2f * point2.Y - num2 * num * (0.5f * y2_1 + y2_2)));
		p3 = new PointF(point2.X, point2.Y);
	}

	protected void GetBezierControlPoints(ChartStyledPoint point1, ChartStyledPoint point2, double ys1, double ys2, out ChartPoint controlPoint1, out ChartPoint controlPoint2, int yIndex)
	{
		double num = point2.X - point1.X;
		num *= num;
		double num2 = 2.0 * point1.X + point2.X;
		double num3 = point1.X + 2.0 * point2.X;
		double num4 = 2.0 * point1.YValues[yIndex] + point2.YValues[yIndex];
		double num5 = point1.YValues[yIndex] + 2.0 * point2.YValues[yIndex];
		double y = 1.0 / 3.0 * (num4 - 1.0 / 3.0 * num * (ys1 + 0.5 * ys2));
		double y2 = 1.0 / 3.0 * (num5 - 1.0 / 3.0 * num * (0.5 * ys1 + ys2));
		controlPoint1 = new ChartPoint(num2 * (1.0 / 3.0), y);
		controlPoint2 = new ChartPoint(num3 * (1.0 / 3.0), y2);
	}

	protected void BezierPointsFromSpline(ChartPointWithIndex point1, ChartPointWithIndex point2, double y2_1, double y2_2, out ChartPointWithIndex p0, out ChartPointWithIndex p1, out ChartPointWithIndex p2, out ChartPointWithIndex p3)
	{
		double num = point2.Point.X - point1.Point.X;
		num *= num;
		double num2 = 1.0 / 3.0;
		double num3 = 2.0 * point1.Point.X + point2.Point.X;
		double num4 = point1.Point.X + 2.0 * point2.Point.X;
		double num5 = 2.0 * point1.Point.YValues[0] + point2.Point.YValues[0];
		double num6 = point1.Point.YValues[0] + 2.0 * point2.Point.YValues[0];
		double x = num3 * num2;
		double x2 = num4 * num2;
		double y = num2 * (num5 - num2 * num * (y2_1 + 0.5 * y2_2));
		double y2 = num2 * (num6 - num2 * num * (0.5 * y2_1 + y2_2));
		p0 = point1;
		p1 = new ChartPointWithIndex(new ChartPoint(x, y), point1.Index);
		p2 = new ChartPointWithIndex(new ChartPoint(x2, y2), point2.Index);
		p3 = point2;
	}

	protected void canonicalSpline(ChartPointWithIndex[] points, double tension, bool addextremumpoints, out ChartPointWithIndex[] bpoints, out ChartPointWithIndex[] bextrpoints)
	{
		int num = points.Length;
		ArrayList arrayList = new ArrayList(4 * (num - 1));
		for (int i = 0; i < num - 1; i++)
		{
			ChartPointWithIndex chartPointWithIndex = ((i != 0) ? points[i - 1] : points[i]);
			ChartPointWithIndex chartPointWithIndex2 = points[i];
			ChartPointWithIndex chartPointWithIndex3 = points[i + 1];
			ChartPointWithIndex chartPointWithIndex4 = ((i != num - 2) ? points[i + 2] : points[i + 1]);
			double x = chartPointWithIndex.Point.X;
			double x2 = chartPointWithIndex2.Point.X;
			double x3 = chartPointWithIndex3.Point.X;
			double x4 = chartPointWithIndex4.Point.X;
			double num2 = chartPointWithIndex.Point.YValues[0];
			double num3 = chartPointWithIndex2.Point.YValues[0];
			double num4 = chartPointWithIndex3.Point.YValues[0];
			double num5 = chartPointWithIndex4.Point.YValues[0];
			double x5 = (3.0 * x2 + tension * (x3 - x)) / 3.0;
			double y = (3.0 * num3 + tension * (num4 - num2)) / 3.0;
			double x6 = (3.0 * x3 - tension * (x4 - x2)) / 3.0;
			double y2 = (3.0 * num4 - tension * (num5 - num3)) / 3.0;
			arrayList.Add(chartPointWithIndex2);
			ChartPointWithIndex value = new ChartPointWithIndex(new ChartPoint(x5, y), chartPointWithIndex2.Index);
			arrayList.Add(value);
			ChartPointWithIndex value2 = new ChartPointWithIndex(new ChartPoint(x6, y2), chartPointWithIndex3.Index);
			arrayList.Add(value2);
			arrayList.Add(chartPointWithIndex3);
		}
		bpoints = (ChartPointWithIndex[])arrayList.ToArray(typeof(ChartPointWithIndex));
		bextrpoints = (ChartPointWithIndex[])arrayList.ToArray(typeof(ChartPointWithIndex));
		if (!addextremumpoints)
		{
			return;
		}
		ArrayList arrayList2 = new ArrayList(4 * (num - 1));
		for (int j = 0; j < arrayList.Count - 3; j += 4)
		{
			ChartPointWithIndex chartPointWithIndex5 = (ChartPointWithIndex)arrayList[j];
			ChartPointWithIndex chartPointWithIndex6 = (ChartPointWithIndex)arrayList[j + 1];
			ChartPointWithIndex chartPointWithIndex7 = (ChartPointWithIndex)arrayList[j + 2];
			ChartPointWithIndex chartPointWithIndex8 = (ChartPointWithIndex)arrayList[j + 3];
			double x7 = chartPointWithIndex5.Point.X;
			double x8 = chartPointWithIndex6.Point.X;
			double x9 = chartPointWithIndex7.Point.X;
			double x10 = chartPointWithIndex8.Point.X;
			double num6 = 0.0 - x7 + 3.0 * x8 - 3.0 * x9 + x10;
			double num7 = 3.0 * x7 - 6.0 * x8 + 3.0 * x9;
			double c = -3.0 * x7 + 3.0 * x8;
			if (ChartMath.SolveQuadraticEquation(3.0 * num6, 2.0 * num7, c, out var root, out var root2))
			{
				if (root > 0.0 && root < 1.0)
				{
					SplitBezierCurve(chartPointWithIndex5, chartPointWithIndex6, chartPointWithIndex7, chartPointWithIndex8, root, out var pb, out var pb2, out var pb3, out var pb4, out var pe, out var pe2, out var pe3, out var pe4);
					arrayList2.Add(pb);
					arrayList2.Add(pb2);
					arrayList2.Add(pb3);
					arrayList2.Add(pb4);
					chartPointWithIndex5 = pe;
					chartPointWithIndex6 = pe2;
					chartPointWithIndex7 = pe3;
					chartPointWithIndex8 = pe4;
					root2 = (root2 - root) / (1.0 - root);
				}
				if (root2 > 0.0 && root2 < 1.0)
				{
					SplitBezierCurve(chartPointWithIndex5, chartPointWithIndex6, chartPointWithIndex7, chartPointWithIndex8, root2, out var pb5, out var pb6, out var pb7, out var pb8, out var pe5, out var pe6, out var pe7, out var pe8);
					arrayList2.Add(pb5);
					arrayList2.Add(pb6);
					arrayList2.Add(pb7);
					arrayList2.Add(pb8);
					chartPointWithIndex5 = pe5;
					chartPointWithIndex6 = pe6;
					chartPointWithIndex7 = pe7;
					chartPointWithIndex8 = pe8;
				}
			}
			arrayList2.Add(chartPointWithIndex5);
			arrayList2.Add(chartPointWithIndex6);
			arrayList2.Add(chartPointWithIndex7);
			arrayList2.Add(chartPointWithIndex8);
		}
		ArrayList arrayList3 = new ArrayList(4 * (num - 1));
		for (int k = 0; k < arrayList2.Count - 3; k += 4)
		{
			ChartPointWithIndex chartPointWithIndex9 = (ChartPointWithIndex)arrayList2[k];
			ChartPointWithIndex chartPointWithIndex10 = (ChartPointWithIndex)arrayList2[k + 1];
			ChartPointWithIndex chartPointWithIndex11 = (ChartPointWithIndex)arrayList2[k + 2];
			ChartPointWithIndex chartPointWithIndex12 = (ChartPointWithIndex)arrayList2[k + 3];
			double num8 = chartPointWithIndex9.Point.YValues[0];
			double num9 = chartPointWithIndex10.Point.YValues[0];
			double num10 = chartPointWithIndex11.Point.YValues[0];
			double num11 = chartPointWithIndex12.Point.YValues[0];
			double num12 = 0.0 - num8 + 3.0 * num9 - 3.0 * num10 + num11;
			double num13 = 3.0 * num8 - 6.0 * num9 + 3.0 * num10;
			double c2 = -3.0 * num8 + 3.0 * num9;
			if (ChartMath.SolveQuadraticEquation(3.0 * num12, 2.0 * num13, c2, out var root3, out var root4))
			{
				if (root3 > 0.0 && root3 < 1.0)
				{
					SplitBezierCurve(chartPointWithIndex9, chartPointWithIndex10, chartPointWithIndex11, chartPointWithIndex12, root3, out var pb9, out var pb10, out var pb11, out var pb12, out var pe9, out var pe10, out var pe11, out var pe12);
					arrayList3.Add(pb9);
					arrayList3.Add(pb10);
					arrayList3.Add(pb11);
					arrayList3.Add(pb12);
					chartPointWithIndex9 = pe9;
					chartPointWithIndex10 = pe10;
					chartPointWithIndex11 = pe11;
					chartPointWithIndex12 = pe12;
					root4 = (root4 - root3) / (1.0 - root3);
				}
				if (root4 > 0.0 && root4 < 1.0)
				{
					SplitBezierCurve(chartPointWithIndex9, chartPointWithIndex10, chartPointWithIndex11, chartPointWithIndex12, root4, out var pb13, out var pb14, out var pb15, out var pb16, out var pe13, out var pe14, out var pe15, out var pe16);
					arrayList3.Add(pb13);
					arrayList3.Add(pb14);
					arrayList3.Add(pb15);
					arrayList3.Add(pb16);
					chartPointWithIndex9 = pe13;
					chartPointWithIndex10 = pe14;
					chartPointWithIndex11 = pe15;
					chartPointWithIndex12 = pe16;
				}
			}
			arrayList3.Add(chartPointWithIndex9);
			arrayList3.Add(chartPointWithIndex10);
			arrayList3.Add(chartPointWithIndex11);
			arrayList3.Add(chartPointWithIndex12);
		}
		bextrpoints = (ChartPointWithIndex[])arrayList3.ToArray(typeof(ChartPointWithIndex));
	}

	protected void SplitBezierCurve(ChartPointWithIndex p0, ChartPointWithIndex p1, ChartPointWithIndex p2, ChartPointWithIndex p3, double t0, out ChartPointWithIndex pb0, out ChartPointWithIndex pb1, out ChartPointWithIndex pb2, out ChartPointWithIndex pb3, out ChartPointWithIndex pe0, out ChartPointWithIndex pe1, out ChartPointWithIndex pe2, out ChartPointWithIndex pe3)
	{
		int num = 4;
		double[,] array = new double[num, num];
		double[,] array2 = new double[num, num];
		array[0, 0] = p0.Point.X;
		array[1, 0] = p1.Point.X;
		array[2, 0] = p2.Point.X;
		array[3, 0] = p3.Point.X;
		array2[0, 0] = p0.Point.YValues[0];
		array2[1, 0] = p1.Point.YValues[0];
		array2[2, 0] = p2.Point.YValues[0];
		array2[3, 0] = p3.Point.YValues[0];
		for (int i = 1; i < num; i++)
		{
			for (int j = 0; j < num - i; j++)
			{
				array[j, i] = array[j, i - 1] * (1.0 - t0) + array[j + 1, i - 1] * t0;
				array2[j, i] = array2[j, i - 1] * (1.0 - t0) + array2[j + 1, i - 1] * t0;
			}
		}
		pb0 = new ChartPointWithIndex(new ChartPoint(array[0, 0], array2[0, 0]), p0.Index);
		pb1 = new ChartPointWithIndex(new ChartPoint(array[0, 1], array2[0, 1]), p0.Index);
		pb2 = new ChartPointWithIndex(new ChartPoint(array[0, 2], array2[0, 2]), p0.Index);
		pb3 = new ChartPointWithIndex(new ChartPoint(array[0, 3], array2[0, 3]), p0.Index);
		pe0 = new ChartPointWithIndex(new ChartPoint(array[0, 3], array2[0, 3]), p3.Index);
		pe1 = new ChartPointWithIndex(new ChartPoint(array[1, 2], array2[1, 2]), p3.Index);
		pe2 = new ChartPointWithIndex(new ChartPoint(array[2, 1], array2[2, 1]), p3.Index);
		pe3 = new ChartPointWithIndex(new ChartPoint(array[3, 0], array2[3, 0]), p3.Index);
	}

	protected virtual ArrayList Draw3DBeziers(Graphics g, PointF[] drawpoints, PointF[] fillpoints, SizeF offset, BrushInfo brush, Pen pen)
	{
		ArrayList arrayList = new ArrayList();
		GraphicsPath graphicsPath = new GraphicsPath();
		BrushInfo brush2 = brush.Clone();
		for (int i = 0; i < fillpoints.Length - 3; i += 4)
		{
			PointF point = fillpoints[i];
			PointF point2 = fillpoints[i + 1];
			PointF point3 = fillpoints[i + 2];
			PointF point4 = fillpoints[i + 3];
			graphicsPath.AddBezier(point, point2, point3, point4);
			graphicsPath.AddBezier(new PointF(point4.X + offset.Width, point4.Y + offset.Height), new PointF(point3.X + offset.Width, point3.Y + offset.Height), new PointF(point2.X + offset.Width, point2.Y + offset.Height), new PointF(point.X + offset.Width, point.Y + offset.Height));
			arrayList.Add(new Region(graphicsPath));
			BrushPaint.FillPath(g, graphicsPath, brush2);
			graphicsPath.Reset();
		}
		for (int j = 0; j < drawpoints.Length - 3; j += 4)
		{
			PointF point5 = drawpoints[j];
			PointF point6 = drawpoints[j + 1];
			PointF point7 = drawpoints[j + 2];
			PointF point8 = drawpoints[j + 3];
			graphicsPath.AddBezier(point5, point6, point7, point8);
			graphicsPath.AddBezier(new PointF(point8.X + offset.Width, point8.Y + offset.Height), new PointF(point7.X + offset.Width, point7.Y + offset.Height), new PointF(point6.X + offset.Width, point6.Y + offset.Height), new PointF(point5.X + offset.Width, point5.Y + offset.Height));
			g.DrawPath(pen, graphicsPath);
			graphicsPath.Reset();
		}
		return arrayList;
	}

	protected GraphicsPath CreateVerticalCylinder3D(RectangleF rect, SizeF offset)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float width = rect.Width;
		float width2 = offset.Width;
		float height = offset.Height;
		PointF[] array = new PointF[13]
		{
			new PointF(rect.Left + 0.5f * width, rect.Top),
			new PointF(rect.Left + 0.75f * width, rect.Top),
			new PointF(rect.Right + 0.25f * width2, rect.Top + 0.25f * height),
			new PointF(rect.Right + 0.5f * width2, rect.Top + 0.5f * height),
			new PointF(rect.Right + 0.75f * width2, rect.Top + 0.75f * height),
			new PointF(rect.Left + 0.75f * width + width2, rect.Top + height),
			new PointF(rect.Left + 0.5f * width + width2, rect.Top + height),
			new PointF(rect.Left + 0.25f * width + width2, rect.Top + height),
			new PointF(rect.Left + 0.75f * width2, rect.Top + 0.75f * height),
			new PointF(rect.Left + 0.5f * width2, rect.Top + 0.5f * height),
			new PointF(rect.Left + 0.25f * width2, rect.Top + 0.25f * height),
			new PointF(rect.Left + 0.25f * width, rect.Top),
			new PointF(rect.Left + 0.5f * width, rect.Top)
		};
		PointF[] array2 = new PointF[13]
		{
			new PointF(rect.Left + 0.5f * width, rect.Bottom),
			new PointF(rect.Left + 0.75f * width, rect.Bottom),
			new PointF(rect.Right + 0.25f * width2, rect.Bottom + 0.25f * height),
			new PointF(rect.Right + 0.5f * width2, rect.Bottom + 0.5f * height),
			new PointF(rect.Right + 0.75f * width2, rect.Bottom + 0.75f * height),
			new PointF(rect.Left + 0.75f * width + width2, rect.Bottom + height),
			new PointF(rect.Left + 0.5f * width + width2, rect.Bottom + height),
			new PointF(rect.Left + 0.25f * width + width2, rect.Bottom + height),
			new PointF(rect.Left + 0.75f * width2, rect.Bottom + 0.75f * height),
			new PointF(rect.Left + 0.5f * width2, rect.Bottom + 0.5f * height),
			new PointF(rect.Left + 0.25f * width2, rect.Bottom + 0.25f * height),
			new PointF(rect.Left + 0.25f * width, rect.Bottom),
			new PointF(rect.Left + 0.5f * width, rect.Bottom)
		};
		PointF[] array3 = new PointF[10]
		{
			array[9],
			array[10],
			array[11],
			array[0],
			array[1],
			array[2],
			array[3],
			array[4],
			array[5],
			array[6]
		};
		PointF[] array4 = new PointF[10]
		{
			array2[6],
			array2[5],
			array2[4],
			array2[3],
			array2[2],
			array2[1],
			array2[0],
			array2[11],
			array2[10],
			array2[9]
		};
		GetLeftBezierPoint(array3[3], ref array3[2], ref array3[1], ref array3[0]);
		GetRightBezierPoint(array3[6], ref array3[7], ref array3[8], ref array3[9]);
		GetLeftBezierPoint(array4[6], ref array4[7], ref array4[8], ref array4[9]);
		GetRightBezierPoint(array4[3], ref array4[2], ref array4[1], ref array4[0]);
		graphicsPath.StartFigure();
		graphicsPath.AddBeziers(array3);
		graphicsPath.AddBeziers(array4);
		graphicsPath.CloseFigure();
		graphicsPath.AddBeziers(array);
		return graphicsPath;
	}

	protected GraphicsPath CreateHorizintalCylinder3DTop(RectangleF rect, SizeF offset)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float height = rect.Height;
		float width = offset.Width;
		float height2 = offset.Height;
		PointF[] points = new PointF[13]
		{
			new PointF(rect.Right, rect.Top + 0.5f * height),
			new PointF(rect.Right, rect.Top + 0.75f * height),
			new PointF(rect.Right + 0.25f * width, rect.Bottom + 0.25f * height2),
			new PointF(rect.Right + 0.5f * width, rect.Bottom + 0.5f * height2),
			new PointF(rect.Right + 0.75f * width, rect.Bottom + 0.75f * height2),
			new PointF(rect.Right + width, rect.Top + 0.75f * height + height2),
			new PointF(rect.Right + width, rect.Top + 0.5f * height + height2),
			new PointF(rect.Right + width, rect.Top + 0.25f * height + height2),
			new PointF(rect.Right + 0.75f * width, rect.Top + 0.75f * height2),
			new PointF(rect.Right + 0.5f * width, rect.Top + 0.5f * height2),
			new PointF(rect.Right + 0.25f * width, rect.Top + 0.25f * height2),
			new PointF(rect.Right, rect.Top + 0.25f * height),
			new PointF(rect.Right, rect.Top + 0.5f * height)
		};
		graphicsPath.AddBeziers(points);
		return graphicsPath;
	}

	protected GraphicsPath CreateVerticalCylinder3DTop(RectangleF rect, SizeF offset)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float width = rect.Width;
		float width2 = offset.Width;
		float height = offset.Height;
		PointF[] points = new PointF[13]
		{
			new PointF(rect.Left + 0.5f * width, rect.Top),
			new PointF(rect.Left + 0.75f * width, rect.Top),
			new PointF(rect.Right + 0.25f * width2, rect.Top + 0.25f * height),
			new PointF(rect.Right + 0.5f * width2, rect.Top + 0.5f * height),
			new PointF(rect.Right + 0.75f * width2, rect.Top + 0.75f * height),
			new PointF(rect.Left + 0.75f * width + width2, rect.Top + height),
			new PointF(rect.Left + 0.5f * width + width2, rect.Top + height),
			new PointF(rect.Left + 0.25f * width + width2, rect.Top + height),
			new PointF(rect.Left + 0.75f * width2, rect.Top + 0.75f * height),
			new PointF(rect.Left + 0.5f * width2, rect.Top + 0.5f * height),
			new PointF(rect.Left + 0.25f * width2, rect.Top + 0.25f * height),
			new PointF(rect.Left + 0.25f * width, rect.Top),
			new PointF(rect.Left + 0.5f * width, rect.Top)
		};
		graphicsPath.AddBeziers(points);
		return graphicsPath;
	}

	protected GraphicsPath CreateHorizintalCylinder3D(RectangleF rect, SizeF offset)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float height = rect.Height;
		float width = offset.Width;
		float height2 = offset.Height;
		PointF[] array = new PointF[13]
		{
			new PointF(rect.Right, rect.Top + 0.5f * height),
			new PointF(rect.Right, rect.Top + 0.75f * height),
			new PointF(rect.Right + 0.25f * width, rect.Bottom + 0.25f * height2),
			new PointF(rect.Right + 0.5f * width, rect.Bottom + 0.5f * height2),
			new PointF(rect.Right + 0.75f * width, rect.Bottom + 0.75f * height2),
			new PointF(rect.Right + width, rect.Top + 0.75f * height + height2),
			new PointF(rect.Right + width, rect.Top + 0.5f * height + height2),
			new PointF(rect.Right + width, rect.Top + 0.25f * height + height2),
			new PointF(rect.Right + 0.75f * width, rect.Top + 0.75f * height2),
			new PointF(rect.Right + 0.5f * width, rect.Top + 0.5f * height2),
			new PointF(rect.Right + 0.25f * width, rect.Top + 0.25f * height2),
			new PointF(rect.Right, rect.Top + 0.25f * height),
			new PointF(rect.Right, rect.Top + 0.5f * height)
		};
		PointF[] array2 = new PointF[13]
		{
			new PointF(rect.Left, rect.Top + 0.5f * height),
			new PointF(rect.Left, rect.Top + 0.75f * height),
			new PointF(rect.Left + 0.25f * width, rect.Bottom + 0.25f * height2),
			new PointF(rect.Left + 0.5f * width, rect.Bottom + 0.5f * height2),
			new PointF(rect.Left + 0.75f * width, rect.Bottom + 0.75f * height2),
			new PointF(rect.Left + width, rect.Top + 0.75f * height + height2),
			new PointF(rect.Left + width, rect.Top + 0.5f * height + height2),
			new PointF(rect.Left + width, rect.Top + 0.25f * height + height2),
			new PointF(rect.Left + 0.75f * width, rect.Top + 0.75f * height2),
			new PointF(rect.Left + 0.5f * width, rect.Top + 0.5f * height2),
			new PointF(rect.Left + 0.25f * width, rect.Top + 0.25f * height2),
			new PointF(rect.Left, rect.Top + 0.25f * height),
			new PointF(rect.Left, rect.Top + 0.5f * height)
		};
		PointF[] array3 = new PointF[10]
		{
			array[6],
			array[7],
			array[8],
			array[9],
			array[10],
			array[11],
			array[0],
			array[1],
			array[2],
			array[3]
		};
		PointF[] array4 = new PointF[10]
		{
			array2[3],
			array2[2],
			array2[1],
			array2[0],
			array2[11],
			array2[10],
			array2[9],
			array2[8],
			array2[7],
			array2[6]
		};
		GetTopBezierPoint(array3[3], ref array3[2], ref array3[1], ref array3[0]);
		GetBottomBezierPoint(array3[6], ref array3[7], ref array3[8], ref array3[9]);
		GetTopBezierPoint(array4[6], ref array4[7], ref array4[8], ref array4[9]);
		GetBottomBezierPoint(array4[3], ref array4[2], ref array4[1], ref array4[0]);
		graphicsPath.StartFigure();
		graphicsPath.AddBeziers(array3);
		graphicsPath.AddBeziers(array4);
		graphicsPath.CloseFigure();
		graphicsPath.AddBeziers(array);
		return graphicsPath;
	}

	private void GetLeftBezierPoint(PointF p1, ref PointF p2, ref PointF p3, ref PointF p4)
	{
		float num = 3f * (p2.X - p1.X);
		float num2 = 3f * (p2.Y - p1.Y);
		float num3 = 3f * (p3.X - p2.X) - num;
		float num4 = 3f * (p3.Y - p2.Y) - num2;
		float num5 = p4.X - p1.X - num3 - num;
		_ = p4.Y;
		_ = p1.Y;
		ChartMath.SolveQuadraticEquation(3f * num5, 2f * num3, num, out var root, out var root2);
		PointF pe;
		PointF pe2;
		PointF pe3;
		PointF pe4;
		if (root > 0f && root < 1f)
		{
			ChartMath.SplitBezierCurve(p1, p2, p3, p4, root, out p1, out p2, out p3, out p4, out pe, out pe2, out pe3, out pe4);
		}
		if (root2 > 0f && root2 < 1f)
		{
			ChartMath.SplitBezierCurve(p1, p2, p3, p4, root2, out p1, out p2, out p3, out p4, out pe, out pe2, out pe3, out pe4);
		}
	}

	private void GetRightBezierPoint(PointF p1, ref PointF p2, ref PointF p3, ref PointF p4)
	{
		float num = 3f * (p2.X - p1.X);
		float num2 = 3f * (p2.Y - p1.Y);
		float num3 = 3f * (p3.X - p2.X) - num;
		float num4 = 3f * (p3.Y - p2.Y) - num2;
		float num5 = p4.X - p1.X - num3 - num;
		_ = p4.Y;
		_ = p1.Y;
		ChartMath.SolveQuadraticEquation(3f * num5, 2f * num3, num, out var root, out var root2);
		PointF pe;
		PointF pe2;
		PointF pe3;
		PointF pe4;
		if (root > 0f && root < 1f)
		{
			ChartMath.SplitBezierCurve(p1, p2, p3, p4, root, out p1, out p2, out p3, out p4, out pe, out pe2, out pe3, out pe4);
		}
		if (root2 > 0f && root2 < 1f)
		{
			ChartMath.SplitBezierCurve(p1, p2, p3, p4, root2, out p1, out p2, out p3, out p4, out pe, out pe2, out pe3, out pe4);
		}
	}

	private void GetTopBezierPoint(PointF p1, ref PointF p2, ref PointF p3, ref PointF p4)
	{
		float num = 3f * (p2.X - p1.X);
		float num2 = 3f * (p2.Y - p1.Y);
		float num3 = 3f * (p3.X - p2.X) - num;
		float num4 = 3f * (p3.Y - p2.Y) - num2;
		_ = p4.X;
		_ = p1.X;
		float num5 = p4.Y - p1.Y - num4 - num2;
		ChartMath.SolveQuadraticEquation(3f * num5, 2f * num4, num2, out var root, out var root2);
		PointF pe;
		PointF pe2;
		PointF pe3;
		PointF pe4;
		if (root > 0f && root < 1f)
		{
			ChartMath.SplitBezierCurve(p1, p2, p3, p4, root, out p1, out p2, out p3, out p4, out pe, out pe2, out pe3, out pe4);
		}
		if (root2 > 0f && root2 < 1f)
		{
			ChartMath.SplitBezierCurve(p1, p2, p3, p4, root2, out p1, out p2, out p3, out p4, out pe, out pe2, out pe3, out pe4);
		}
	}

	private void GetBottomBezierPoint(PointF p1, ref PointF p2, ref PointF p3, ref PointF p4)
	{
		float num = 3f * (p2.X - p1.X);
		float num2 = 3f * (p2.Y - p1.Y);
		float num3 = 3f * (p3.X - p2.X) - num;
		float num4 = 3f * (p3.Y - p2.Y) - num2;
		_ = p4.X;
		_ = p1.X;
		float num5 = p4.Y - p1.Y - num4 - num2;
		ChartMath.SolveQuadraticEquation(3f * num5, 2f * num4, num2, out var root, out var root2);
		PointF pe;
		PointF pe2;
		PointF pe3;
		PointF pe4;
		if (root > 0f && root < 1f)
		{
			ChartMath.SplitBezierCurve(p1, p2, p3, p4, root, out p1, out p2, out p3, out p4, out pe, out pe2, out pe3, out pe4);
		}
		if (root2 > 0f && root2 < 1f)
		{
			ChartMath.SplitBezierCurve(p1, p2, p3, p4, root2, out p1, out p2, out p3, out p4, out pe, out pe2, out pe3, out pe4);
		}
	}

	protected virtual Region Draw3DLines(Graphics g, PointF[] points, SizeF offset, BrushInfo brush, Pen pen, Color[] colors)
	{
		Region region = new Region();
		GraphicsPath graphicsPath = new GraphicsPath();
		BrushInfo brushInfo = brush.Clone();
		int num;
		int i = (num = 0);
		int num2 = 1;
		int num3 = points.Length - 1;
		if (XAxis.Inversed)
		{
			i = (num = points.Length - 2);
			num2 = -1;
			num3 = -1;
		}
		for (; i != num3; i += num2)
		{
			graphicsPath.AddPolygon(new PointF[4]
			{
				points[i],
				points[i + 1],
				new PointF(points[i + 1].X + offset.Width, points[i + 1].Y + offset.Height),
				new PointF(points[i].X + offset.Width, points[i].Y + offset.Height)
			});
			if (i == num)
			{
				region = new Region(graphicsPath);
			}
			else
			{
				region.Union(graphicsPath);
			}
			brushInfo = new BrushInfo(colors[i]);
			BrushPaint.FillPath(g, graphicsPath, brushInfo);
			g.DrawPath(pen, graphicsPath);
			graphicsPath.Reset();
		}
		return region;
	}

	protected virtual Region Draw3DLines(Graphics g, PointF[] points, SizeF offset, BrushInfo brush, Pen pen)
	{
		Region region = new Region();
		GraphicsPath graphicsPath = new GraphicsPath();
		int num;
		int i = (num = 0);
		int num2 = 1;
		int num3 = points.Length - 1;
		if (XAxis.Inversed)
		{
			i = (num = points.Length - 2);
			num2 = -1;
			num3 = -1;
		}
		for (; i != num3; i += num2)
		{
			graphicsPath.AddPolygon(new PointF[4]
			{
				points[i],
				points[i + 1],
				new PointF(points[i + 1].X + offset.Width, points[i + 1].Y + offset.Height),
				new PointF(points[i].X + offset.Width, points[i].Y + offset.Height)
			});
			if (i == num)
			{
				region = new Region(graphicsPath);
			}
			else
			{
				region.Union(graphicsPath);
			}
			if (m_series.Type == ChartSeriesType.Line)
			{
				brush = GetBrush(i);
			}
			BrushPaint.FillPath(g, graphicsPath, brush);
			g.DrawPath(pen, graphicsPath);
			graphicsPath.Reset();
		}
		return region;
	}

	public void Draw(ChartGraph cg, GraphicsPath gp, BrushInfo br, Pen p)
	{
		if (cg != null)
		{
			ColorBlend colorBlend = new ColorBlend();
			PhongShadingColors(Color.FromArgb(200, br.BackColor), Color.FromArgb(144, br.BackColor), Color.FromArgb(100, Color.Black), Math.PI / 4.0, 30.0, out var colors, out var positions);
			if (cg.isRight)
			{
				PhongShadingColors(Color.FromArgb(200, br.BackColor), Color.FromArgb(144, br.BackColor), Color.FromArgb(100, Color.Black), Math.PI, 30.0, out colors, out positions);
			}
			else
			{
				PhongShadingColors(Color.FromArgb(200, br.BackColor), Color.FromArgb(144, br.BackColor), Color.FromArgb(100, Color.Black), Math.PI / 4.0, 30.0, out colors, out positions);
			}
			colorBlend.Positions = positions;
			colorBlend.Colors = colors;
			ColorBlend interpolationColors = colorBlend;
			cg.DrawPath(br, p, gp);
			using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(new Rectangle(0, 0, 1, 1), Color.Black, Color.White, LinearGradientMode.Vertical);
			linearGradientBrush.InterpolationColors = interpolationColors;
			cg.DrawPath(linearGradientBrush, p, gp);
		}
	}

	protected virtual RectangleF GetRectangle(ChartPoint firstPoint, ChartPoint secondPoint)
	{
		PointF pointF = new PointF(GetXFromValue(firstPoint, 0), GetYFromValue(firstPoint, 0));
		PointF pointF2 = new PointF(GetXFromValue(secondPoint, 0), GetYFromValue(secondPoint, 0));
		return new RectangleF(Math.Min(pointF.X, pointF2.X), Math.Min(pointF.Y, pointF2.Y), Math.Abs(pointF2.X - pointF.X), Math.Abs(pointF2.Y - pointF.Y));
	}

	protected virtual Region Draw3DRectangle(Graphics g, RectangleF rc, SizeF offset, BrushInfo brush, Pen pen)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddPolygon(new PointF[4]
		{
			new PointF(rc.Right, rc.Top),
			new PointF(rc.Right + offset.Width, rc.Top + offset.Height),
			new PointF(rc.Right + offset.Width, rc.Bottom + offset.Height),
			new PointF(rc.Right, rc.Bottom)
		});
		Region region = new Region(graphicsPath);
		BrushPaint.FillPath(g, graphicsPath, brush);
		if (EnableStyles)
		{
			g.DrawPath(pen, graphicsPath);
		}
		graphicsPath.Reset();
		graphicsPath.AddPolygon(new PointF[4]
		{
			new PointF(rc.X, rc.Top),
			new PointF(rc.X + offset.Width, rc.Top + offset.Height),
			new PointF(rc.X + offset.Width + rc.Width, rc.Top + offset.Height),
			new PointF(rc.Right, rc.Top)
		});
		region.Union(graphicsPath);
		BrushPaint.FillPath(g, graphicsPath, brush);
		if (EnableStyles)
		{
			g.DrawPath(pen, graphicsPath);
		}
		graphicsPath.Reset();
		graphicsPath.AddPolygon(new PointF[4]
		{
			new PointF(rc.Left, rc.Top),
			new PointF(rc.Right, rc.Top),
			new PointF(rc.Right, rc.Bottom),
			new PointF(rc.Left, rc.Bottom)
		});
		region.Union(graphicsPath);
		BrushPaint.FillPath(g, graphicsPath, brush);
		if (EnableStyles)
		{
			g.DrawPath(pen, graphicsPath);
		}
		return region;
	}

	protected virtual GraphicsPath CreateBox(RectangleF rectF, bool is3D)
	{
		Rectangle rectangle = new Rectangle((int)rectF.X, (int)rectF.Y, (int)rectF.Width, (int)rectF.Height);
		GraphicsPath graphicsPath = new GraphicsPath();
		SizeF seriesOffset = GetSeriesOffset();
		Size size = new Size((int)seriesOffset.Width, (int)seriesOffset.Height);
		graphicsPath.AddRectangle(rectangle);
		if (is3D)
		{
			graphicsPath.AddPolygon(new PointF[4]
			{
				new PointF(rectangle.Left, rectangle.Top),
				new PointF(rectangle.Left + size.Width, rectangle.Top + size.Height),
				new PointF(rectangle.Right + size.Width, rectangle.Top + size.Height),
				new PointF(rectangle.Right, rectangle.Top)
			});
			graphicsPath.AddPolygon(new PointF[4]
			{
				new PointF(rectangle.Right, rectangle.Top),
				new PointF(rectangle.Right + size.Width, rectangle.Top + size.Height),
				new PointF(rectangle.Right + size.Width, rectangle.Bottom + size.Height),
				new PointF(rectangle.Right, rectangle.Bottom)
			});
		}
		return graphicsPath;
	}

	protected virtual GraphicsPath CreateBoxRight(RectangleF rectF, bool is3D)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		Rectangle rectangle = new Rectangle((int)rectF.X, (int)rectF.Y, (int)rectF.Width, (int)rectF.Height);
		SizeF seriesOffset = GetSeriesOffset();
		Size size = new Size((int)seriesOffset.Width, (int)seriesOffset.Height);
		if (is3D)
		{
			graphicsPath.AddPolygon(new PointF[4]
			{
				new PointF(rectangle.Left, rectangle.Top),
				new PointF(rectangle.Left + size.Width + 1, rectangle.Top + size.Height),
				new PointF(rectangle.Right + size.Width, rectangle.Top + size.Height),
				new PointF(rectangle.Right, rectangle.Top)
			});
			graphicsPath.AddPolygon(new PointF[4]
			{
				new PointF(rectangle.Right, rectangle.Top),
				new PointF(rectangle.Right + size.Width, rectangle.Top + size.Height),
				new PointF(rectangle.Right + size.Width, rectangle.Bottom + size.Height),
				new PointF(rectangle.Right, rectangle.Bottom)
			});
		}
		return graphicsPath;
	}

	protected virtual GraphicsPath CreateBoxTop(RectangleF rectF, bool is3D)
	{
		Rectangle rectangle = new Rectangle((int)rectF.X, (int)rectF.Y, (int)rectF.Width, (int)rectF.Height);
		GraphicsPath graphicsPath = new GraphicsPath();
		SizeF seriesOffset = GetSeriesOffset();
		Size size = new Size((int)seriesOffset.Width, (int)seriesOffset.Height);
		if (is3D)
		{
			graphicsPath.AddPolygon(new PointF[4]
			{
				new PointF(rectangle.Left, rectangle.Top),
				new PointF(rectangle.Left + size.Width + 1, rectangle.Top + size.Height),
				new PointF(rectangle.Right + size.Width, rectangle.Top + size.Height),
				new PointF(rectangle.Right, rectangle.Top)
			});
		}
		return graphicsPath;
	}

	private void DrawPointSymbol(Graphics3D g, ChartStyledPoint styledPoint)
	{
		ChartStyleInfo style = styledPoint.Style;
		ChartSymbolInfo symbol = style.Symbol;
		Vector3D symbolVector = GetSymbolVector(styledPoint);
		Rectangle rectangle = new Rectangle((int)(symbolVector.X - (double)(symbol.Size.Width / 2)), (int)(symbolVector.Y - (double)(symbol.Size.Height / 2)), symbol.Size.Width, symbol.Size.Height);
		if (symbol.Shape != ChartSymbolShape.Image)
		{
			Path3D polygon = Path3D.FromGraphicsPath(ChartSymbolHelper.GetPathSymbol(symbol.Shape, rectangle), symbolVector.Z, new SolidBrush(symbol.Color), style.Symbol.Border.GdipPen);
			g.AddPolygon(polygon);
		}
		else if (symbol.ImageIndex >= 0)
		{
			Image3D polygon2 = Image3D.FromImage(style.Images[symbol.ImageIndex], rectangle, (float)symbolVector.Z);
			g.AddPolygon(polygon2);
		}
	}

	internal virtual void DrawPointSymbol(Graphics3D g, ChartStyleInfo style, Vector3D pt, bool drawMarker)
	{
		ChartSymbolInfo symbol = style.Symbol;
		Rectangle rectangle = new Rectangle((int)(pt.X - (double)(symbol.Size.Width / 2)), (int)(pt.Y - (double)(symbol.Size.Height / 2)), symbol.Size.Width, symbol.Size.Height);
		if (symbol.Shape != ChartSymbolShape.Image)
		{
			Path3D polygon = Path3D.FromGraphicsPath(ChartSymbolHelper.GetPathSymbol(symbol.Shape, rectangle), pt.Z, new SolidBrush(symbol.Color), style.Symbol.Border.GdipPen);
			g.AddPolygon(polygon);
		}
		else if (symbol.ImageIndex >= 0)
		{
			Image3D polygon2 = Image3D.FromImage(style.Images[symbol.ImageIndex], rectangle, (float)pt.Z);
			g.AddPolygon(polygon2);
		}
	}

	protected virtual void AddSymbolRegion(ChartStyledPoint styledPoint)
	{
		PointF symbolPoint = GetSymbolPoint(styledPoint);
		ChartStyleInfo style = styledPoint.Style;
		new RectangleF(symbolPoint.X - (float)(style.Symbol.Size.Width / 2), symbolPoint.Y - (float)(style.Symbol.Size.Height / 2), style.Symbol.Size.Width, style.Symbol.Size.Height);
	}

	protected virtual Region GetRegionFromCircle(PointF center, float radius)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddEllipse(center.X - radius, center.Y - radius, 2f * radius, 2f * radius);
		return new Region(graphicsPath);
	}

	protected virtual Path3D GetPath3DFromCircle(Vector3D pt, float radius)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddEllipse((float)pt.X - radius, (float)pt.Y - radius, 2f * radius, 2f * radius);
		return Path3D.FromGraphicsPath(graphicsPath, (double)(float)pt.Z, (BrushInfo)null, (Pen)null);
	}

	protected virtual BrushInfo GetBrush(int index)
	{
		return GetStyleAt(index).Interior;
	}

	protected virtual BrushInfo GetBrush(int index, Color color)
	{
		GetStyleAt(index);
		return new BrushInfo(color);
	}

	protected virtual BrushInfo GetBrush()
	{
		ChartStyleInfo seriesStyle = SeriesStyle;
		return GetBrush(seriesStyle);
	}

	protected virtual BrushInfo GetBrush(ChartStyleInfo style)
	{
		return style.Interior;
	}

	protected BrushInfo GetPhongInterior(BrushInfo brushInfo, Color lightColor, double lightAlpha, double phongAlpha)
	{
		if (brushInfo.Style == BrushStyle.Solid)
		{
			int gradientStyle = (IsInvertedAxes ? 4 : 3);
			PhongShadingColors(brushInfo.BackColor, brushInfo.BackColor, lightColor, lightAlpha, phongAlpha, out var colors, out var _);
			brushInfo = new BrushInfo((GradientStyle)gradientStyle, colors);
		}
		return brushInfo;
	}

	protected virtual float GetLowerAnchorPointValue(int index)
	{
		if (m_series.BaseStackingType != ChartSeriesBaseStackingType.NotStacked)
		{
			return GetStackInfo(index);
		}
		if (m_series.RequireInvertedAxes)
		{
			return CustomOriginX;
		}
		return CustomOriginY;
	}

	protected PointF GetPointFromIndex(int i, int j)
	{
		return new PointF(GetXFromIndex(i, j), GetYFromIndex(i, j));
	}

	public PointF GetPointFromIndex(int i)
	{
		return GetPointFromIndex(i, 0);
	}

	protected virtual PointF GetPointFromValue(ChartStyledPoint cpt)
	{
		return GetPointFromValue(cpt.X, cpt.YValues[0]);
	}

	protected virtual PointF GetPointFromValue(double x, double y)
	{
		if (IsRadial)
		{
			PointF center = ChartMath.GetCenter(Bounds);
			double num = Math.PI / 2.0;
			if (m_series.Type == ChartSeriesType.Radar || m_series.ActualXAxis.ValueType == ChartValueType.Category)
			{
				num = ((!m_series.ActualXAxis.Inversed) ? (num + Math.PI * 2.0 * (x - m_series.ActualXAxis.Range.Min) / m_series.ActualXAxis.Range.Delta) : (num + Math.PI * 2.0 * (m_series.ActualXAxis.Range.Max - x) / m_series.ActualXAxis.Range.Delta));
			}
			else if (m_series.Type == ChartSeriesType.Polar)
			{
				num = ((m_series.ActualXAxis.RangeType == ChartAxisRangeType.Set) ? ((!m_series.ActualXAxis.Inversed) ? (num + Math.PI * 2.0 * (x - m_series.ActualXAxis.Range.Min) / m_series.ActualXAxis.Range.Delta) : (num + Math.PI * 2.0 * (m_series.ActualXAxis.Range.Max - x) / m_series.ActualXAxis.Range.Delta)) : ((!m_series.ActualXAxis.Inversed) ? (num + x) : (num + (Math.PI * 2.0 - x))));
			}
			float visibleValue = m_series.ActualYAxis.GetVisibleValue(y);
			return new PointF(center.X + (float)((double)visibleValue * Math.Cos(num)), center.Y - (float)((double)visibleValue * Math.Sin(num)));
		}
		if (IsInvertedAxes)
		{
			return new PointF(XAxis.GetCoordinateFromValue(y), YAxis.GetCoordinateFromValue(x));
		}
		return new PointF(XAxis.GetCoordinateFromValue(x), YAxis.GetCoordinateFromValue(y));
	}

	protected PointF GetPointFromValue(ChartPoint cpt)
	{
		return GetPointFromValue(cpt, 0);
	}

	protected PointF GetPointFromValue(ChartPoint cpt, int j)
	{
		return new PointF(GetXFromValue(cpt, j), GetYFromValue(cpt, j));
	}

	protected virtual DoubleRange GetSideBySideRange()
	{
		double num = 1.0 - 0.01 * (double)m_chart.Spacing;
		double minPointsDelta = GetMinPointsDelta();
		int num2 = -1;
		int num3 = 0;
		if (m_series.BaseType == ChartSeriesBaseType.SideBySide)
		{
			Hashtable hashtable = new Hashtable();
			foreach (ChartSeries item in m_chart.Series)
			{
				if (!item.Visible || m_series.BaseType != ChartSeriesBaseType.SideBySide)
				{
					continue;
				}
				if (item.BaseStackingType != ChartSeriesBaseStackingType.NotStacked)
				{
					if (!hashtable.ContainsKey(item.Type))
					{
						num3++;
						hashtable.Add(item.Type, num3);
					}
					if (m_series == item)
					{
						num2 = (int)hashtable[item.Type];
					}
				}
				else
				{
					num3++;
					if (m_series == item)
					{
						num2 = num3;
					}
				}
			}
		}
		if (num3 == 0)
		{
			num3 = 1;
			num2 = 0;
		}
		minPointsDelta = ((minPointsDelta == double.MaxValue) ? 1.0 : minPointsDelta);
		double num4 = minPointsDelta * num / (double)num3;
		double num5 = num4 * (double)(num2 - 1) - minPointsDelta * num / 2.0;
		double end = num5 + num4;
		return new DoubleRange(num5, end);
	}

	protected DoubleRange GetSideBySideInfo()
	{
		return m_series.ChartModel.GetSideBySideInfo(ChartArea, m_series);
	}

	protected virtual float GetStackInfo(int i)
	{
		double stackInfoValue = GetStackInfoValue(i);
		if (!IsInvertedAxes)
		{
			return GetYFromCoordinate(stackInfoValue);
		}
		return GetXFromCoordinate(stackInfoValue);
	}

	protected virtual double GetStackInfoValue(int i)
	{
		return m_series.ChartModel.GetStackInfo(ChartArea, m_series, i, isWithMe: false);
	}

	protected virtual double GetStackInfoValue(int i, bool isWithMe)
	{
		return m_series.ChartModel.GetStackInfo(ChartArea, m_series, i, isWithMe);
	}

	protected PointF GetSymbolPoint(ChartStyledPoint point)
	{
		PointF pointF = GetSymbolCoordinates(point);
		if (m_series.BaseType != ChartSeriesBaseType.Circular)
		{
			pointF = PointF.Add(pointF, GetThisOffset());
		}
		return pointF;
	}

	internal PointF GetStackedSymbolPoint(int pointIndex)
	{
		ChartStyledPoint point = CreateStyledPoint(pointIndex);
		return GetSymbolPoint(point);
	}

	protected Vector3D GetSymbolVector(ChartStyledPoint point)
	{
		PointF symbolCoordinates = GetSymbolCoordinates(point);
		return new Vector3D(symbolCoordinates.X, symbolCoordinates.Y, GetPlaceDepth());
	}

	public virtual PointF GetSymbolCoordinates(int pointIndex)
	{
		return GetSymbolCoordinates((StyledPoints == null) ? CreateStyledPoint(pointIndex) : StyledPoints[pointIndex]);
	}

	protected PointF GetHiLoSymbolCoordinates(int pointIndex)
	{
		ChartStyledPoint chartStyledPoint = ((StyledPoints == null) ? CreateStyledPoint(pointIndex) : StyledPoints[pointIndex]);
		double x = chartStyledPoint.X + GetSideBySideInfo().Median;
		double y = ((chartStyledPoint.YValues.Length != 0) ? chartStyledPoint.YValues[1] : chartStyledPoint.YValues[0]);
		PointF pointFromValue = GetPointFromValue(x, y);
		Size offset = chartStyledPoint.Style.Symbol.Offset;
		pointFromValue.X += offset.Width;
		pointFromValue.Y += offset.Height;
		return pointFromValue;
	}

	protected virtual PointF GetSymbolCoordinates(ChartStyledPoint styledPoint)
	{
		double num = styledPoint.X;
		double num2 = styledPoint.YValues[0];
		if (m_series.BaseType == ChartSeriesBaseType.SideBySide)
		{
			num += GetSideBySideInfo().Median;
		}
		if (m_series.BaseStackingType == ChartSeriesBaseStackingType.Stacked)
		{
			num2 += GetStackInfoValue(styledPoint.Index);
		}
		else if (m_series.BaseStackingType == ChartSeriesBaseStackingType.FullStacked)
		{
			num2 = GetStackInfoValue(styledPoint.Index, isWithMe: true);
		}
		PointF result = ((m_series.Type != ChartSeriesType.Tornado || styledPoint.YValues.Length <= 1) ? GetPointFromValue(num, num2) : GetPointFromValue(num, styledPoint.YValues[1]));
		Size offset = styledPoint.Style.Symbol.Offset;
		result.X += offset.Width;
		result.Y += offset.Height;
		RectangleF rectangleF = Chart.Bounds;
		result.X = ((Math.Abs(result.X) > rectangleF.Width) ? rectangleF.Width : result.X);
		result.Y = ((Math.Abs(result.Y) > rectangleF.Width) ? rectangleF.Width : result.Y);
		return result;
	}

	protected virtual float GetXFromIndex(int i, int j)
	{
		return GetXFromValue(m_series.Points[i], j);
	}

	protected virtual float GetXFromValue(ChartPoint cp, int j)
	{
		if (m_series.BaseType == ChartSeriesBaseType.Circular)
		{
			PointF center = ChartArea.Center;
			float visibleValue = m_series.ActualYAxis.GetVisibleValue(cp.YValues[j]);
			if (!cp.IsEmpty)
			{
				double angleValue = GetAngleValue(cp, m_series);
				return (float)((double)center.X + (double)visibleValue * Math.Cos(angleValue));
			}
			_ = center.X;
		}
		return GetXFromCoordinate(GetXAxisValue(cp, j));
	}

	protected virtual float GetXFromCoordinate(double value)
	{
		return XAxis.GetCoordinateFromValue(value);
	}

	protected virtual float GetYFromIndex(int i, int j)
	{
		float num = 0f;
		if (m_series.Type == ChartSeriesType.Radar || m_series.Type == ChartSeriesType.Polar)
		{
			float visibleValue = m_series.ActualYAxis.GetVisibleValue(m_series.Points[i].YValues[j]);
			PointF center = ChartArea.Center;
			if (m_series.Points[i].IsEmpty)
			{
				return center.Y;
			}
			double angleValue = GetAngleValue(i, m_series.Points[i], m_series);
			return (float)((double)center.Y - (double)visibleValue * Math.Sin(angleValue));
		}
		if (m_series.RequireInvertedAxes || (IgnoreSeriesInversion && m_chart.RequireInvertedAxes))
		{
			num = GetYFromCoordinate(m_series.Points[i].X);
			if (m_chart.Indexed)
			{
				num = GetYFromCoordinate(GetIndexValueFromX(m_series.Points[i].X));
			}
			if (m_series.XType == ChartValueType.Custom)
			{
				num = GetYFromCoordinate(i);
			}
		}
		else
		{
			num = GetYFromCoordinate(m_series.Points[i].YValues[j]);
		}
		return num;
	}

	protected virtual float GetYFromValue(ChartPoint cp, int j)
	{
		if (IsRadial)
		{
			PointF center = ChartArea.Center;
			if (cp.IsEmpty)
			{
				return center.Y;
			}
			double angleValue = GetAngleValue(cp, m_series);
			float visibleValue = m_series.ActualYAxis.GetVisibleValue(cp.YValues[j]);
			return center.Y - (float)((double)visibleValue * Math.Sin(angleValue));
		}
		return GetYFromCoordinate(GetYAxisValue(cp, j));
	}

	protected virtual float GetYFromCoordinate(double y)
	{
		return YAxis.GetCoordinateFromValue(y);
	}

	protected virtual double GetIndexValueFromX(double x)
	{
		if (m_chart != null && m_chart.Indexed)
		{
			return m_chart.IndexValues.GetIndex(x);
		}
		return x;
	}

	protected virtual double GetAngleValue(ChartPoint cp, ChartSeries series)
	{
		double num = 0.0;
		if (series.Type == ChartSeriesType.Radar || series.ActualXAxis.ValueType == ChartValueType.Category)
		{
			num = ((!series.ActualXAxis.Inversed) ? (Math.PI * 2.0 * (cp.X - series.ActualXAxis.Range.Min) / series.ActualXAxis.Range.Delta) : (Math.PI * 2.0 * (series.ActualXAxis.Range.Max - cp.X) / series.ActualXAxis.Range.Delta));
		}
		else if (series.Type == ChartSeriesType.Polar)
		{
			num = ((series.ActualXAxis.RangeType == ChartAxisRangeType.Set) ? ((!series.ActualXAxis.Inversed) ? (Math.PI * 2.0 * (cp.X - series.ActualXAxis.Range.Min) / series.ActualXAxis.Range.Delta) : (Math.PI * 2.0 * (series.ActualXAxis.Range.Max - cp.X) / series.ActualXAxis.Range.Delta)) : ((!series.ActualXAxis.Inversed) ? cp.X : (Math.PI * 2.0 - cp.X)));
		}
		return num + Math.PI / 2.0;
	}

	protected virtual double GetAngleValue(int index, ChartPoint cp, ChartSeries series)
	{
		double num = Math.PI / 2.0;
		if (m_chart.Indexed)
		{
			if (m_series.Type == ChartSeriesType.Radar)
			{
				num += (double)index * (Math.PI * 2.0) / (double)m_chart.IndexValues.Count;
			}
			else if (m_series.Type == ChartSeriesType.Polar)
			{
				num = ((m_series.ActualXAxis.RangeType != ChartAxisRangeType.Set) ? (num + (double)index) : (num + (double)index * (Math.PI * 2.0) / (double)m_chart.IndexValues.Count));
			}
		}
		else
		{
			num = GetAngleValue(cp, series);
		}
		return num;
	}

	private double GetXAxisValue(ChartPoint cpt, int j)
	{
		if (!IsInvertedAxes)
		{
			return GetIndexValueFromX(cpt.X);
		}
		return cpt.YValues[j];
	}

	private double GetYAxisValue(ChartPoint cpt, int j)
	{
		if (!IsInvertedAxes)
		{
			return cpt.YValues[j];
		}
		return GetIndexValueFromX(cpt.X);
	}

	public virtual DoubleRange GetXDataMeasure()
	{
		if (((m_xAxis.ExcludeInvisibleSeriesRange && m_series.Visible) || !m_xAxis.ExcludeInvisibleSeriesRange) && m_series.Points.Count > 0)
		{
			double num = double.MinValue;
			double num2 = double.MaxValue;
			if (m_xAxis.Range.min > 0.0 && m_xAxis.DrawGrid)
			{
				num2 = m_xAxis.Range.min;
				if (m_xAxis.Range.max > 0.0)
				{
					num = m_xAxis.Range.max;
				}
			}
			else
			{
				for (int i = 0; i < m_series.Points.Count; i++)
				{
					if (!m_series.Points[i].IsEmpty)
					{
						double x = m_series.Points[i].X;
						if (x > num)
						{
							num = x;
						}
						if (x < num2)
						{
							num2 = x;
						}
					}
				}
			}
			if (num == double.MinValue && num2 == double.MaxValue)
			{
				return DoubleRange.Empty;
			}
			return new DoubleRange(num2, num);
		}
		return DoubleRange.Empty;
	}

	public virtual DoubleRange GetYDataMeasure()
	{
		if (((m_yAxis.ExcludeInvisibleSeriesRange && m_series.Visible) || !m_yAxis.ExcludeInvisibleSeriesRange) && m_series.Points.Count > 0)
		{
			double num = double.MinValue;
			double num2 = double.MaxValue;
			for (int i = 0; i < m_series.Points.Count; i++)
			{
				double[] yValues = m_series.Points[i].YValues;
				int j = 0;
				for (int num3 = Math.Min(yValues.Length, RequireYValuesCount); j < num3; j++)
				{
					if (yValues[j] > num)
					{
						num = yValues[j];
					}
					if (yValues[j] < num2)
					{
						num2 = yValues[j];
					}
				}
			}
			if (num == double.MinValue && num2 == double.MaxValue)
			{
				return DoubleRange.Empty;
			}
			if (m_yAxis.IsMaxSet)
			{
				num = YAxis.Range.Max;
			}
			if (m_yAxis.IsMinSet)
			{
				num2 = YAxis.Range.Min;
			}
			DoubleRange doubleRange = new DoubleRange(num2, num);
			if (m_series.OriginDependent && !YAxis.IsMinSet)
			{
				return doubleRange + m_series.ActualYAxis.CurrentOrigin;
			}
			return doubleRange;
		}
		return DoubleRange.Empty;
	}

	protected virtual void DrawText(Graphics g, ChartStyledPoint styledPoint, PointF p)
	{
		ChartStyleInfo style = styledPoint.Style;
		int index = styledPoint.Index;
		PointF pointF = new PointF(0f, 0f);
		PointF pointF2 = new PointF(p.X, p.Y);
		SizeF sizeF = g.MeasureString(style.Text, style.GdipFont);
		SizeF sizeF2 = new SizeF(sizeF.Width / 2f + style.TextOffset, sizeF.Height / 2f + style.TextOffset);
		if (m_series.Type == ChartSeriesType.Column && styledPoint.YValues[0] < 0.0)
		{
			style.TextOrientation = ChartTextOrientation.Down;
		}
		float num = 1.25f;
		switch (style.TextOrientation)
		{
		case ChartTextOrientation.Up:
			pointF2.Y -= sizeF2.Height * num;
			break;
		case ChartTextOrientation.Down:
			pointF2.Y += sizeF2.Height * num;
			break;
		case ChartTextOrientation.Left:
			pointF2.X -= sizeF2.Width * num;
			break;
		case ChartTextOrientation.Right:
			pointF2.X += sizeF2.Width * num;
			break;
		case ChartTextOrientation.UpLeft:
			pointF2.X -= sizeF2.Width;
			pointF2.Y -= sizeF2.Height;
			break;
		case ChartTextOrientation.DownLeft:
			pointF2.X -= sizeF2.Width;
			pointF2.Y += sizeF2.Height;
			break;
		case ChartTextOrientation.UpRight:
			pointF2.X += sizeF2.Width;
			pointF2.Y -= sizeF2.Height;
			break;
		case ChartTextOrientation.DownRight:
			pointF2.X += sizeF2.Width;
			pointF2.Y += sizeF2.Height;
			break;
		case ChartTextOrientation.Smart:
			if (m_chart.RequireInvertedAxes)
			{
				if (pointF2.X < CustomOriginX)
				{
					pointF2.X += 0f - sizeF2.Width;
				}
				else
				{
					pointF2.X += sizeF2.Width;
				}
			}
			else if (pointF2.Y < CustomOriginY)
			{
				pointF2.Y += 0f - sizeF2.Height;
			}
			else
			{
				pointF2.Y += sizeF2.Height;
			}
			break;
		case ChartTextOrientation.RegionUp:
			if (m_series.RequireInvertedAxes)
			{
				pointF2.X -= sizeF.Width;
			}
			else
			{
				pointF2.Y += sizeF.Height;
			}
			break;
		case ChartTextOrientation.RegionDown:
		{
			float lowerAnchorPointValue3 = GetLowerAnchorPointValue(index);
			if (m_series.RequireInvertedAxes)
			{
				pointF2.X = lowerAnchorPointValue3;
			}
			else
			{
				pointF2.Y = lowerAnchorPointValue3 - sizeF.Height;
			}
			break;
		}
		case ChartTextOrientation.RegionLeft:
		{
			float lowerAnchorPointValue2 = GetLowerAnchorPointValue(index);
			if (m_series.RequireInvertedAxes)
			{
				pointF2.X = lowerAnchorPointValue2 * 1.25f;
			}
			else
			{
				pointF2.Y = lowerAnchorPointValue2 - sizeF.Height;
			}
			break;
		}
		case ChartTextOrientation.RegionCenter:
		{
			float lowerAnchorPointValue = GetLowerAnchorPointValue(index);
			if (m_series.RequireInvertedAxes)
			{
				pointF2.X = (lowerAnchorPointValue + pointF2.X) / 2f;
			}
			else
			{
				pointF2.Y = (lowerAnchorPointValue + pointF2.Y) / 2f;
			}
			break;
		}
		case ChartTextOrientation.SymbolCenter:
			pointF2 = GetSymbolPoint(styledPoint);
			break;
		}
		Rectangle renderBounds = ChartArea.RenderBounds;
		double num2 = Math.PI / 180.0 * (double)style.Font.Orientation;
		Math.Sin(num2);
		Math.Cos(num2);
		if (m_series.SmartLabels)
		{
			sizeF = g.MeasureString(style.Text, style.Font.GdipFont).ToSize();
			sizeF.Width += 1f;
			pointF2.X += (0f - sizeF.Width) / 2f + pointF.X;
			pointF2.Y += (0f - sizeF.Height) / 2f + pointF.Y;
			RectangleF r = new RectangleF(pointF2, sizeF);
			RectangleF rectangleF = ChartMath.LeftCenterRotatedRectangleBounds(r, num2);
			ChartLabel chartLabel = new ChartLabel(rectangleF.Location, p, rectangleF.Size, SizeF.Empty);
			RectangleF rectangleF2 = m_labelLayoutManager.AddLabel(chartLabel);
			RectangleF rectangleF3 = rectangleF;
			PointF location = new PointF(r.X + rectangleF2.X - rectangleF.X, r.Y + rectangleF2.Y - rectangleF.Y);
			pointF2 = new RectangleF(location, r.Size).Location;
			if (num2 != 0.0)
			{
				pointF2.X += (0f - sizeF.Width) / 2f;
			}
			chartLabel.DrawPointingLine(g, style, m_series);
		}
		else
		{
			if (num2 != 0.0)
			{
				float num3 = (float)renderBounds.Top - pointF2.Y;
				if (num3 > 0f && Math.Abs(num3) < sizeF.Width)
				{
					pointF2.X = renderBounds.Top;
					num3 = 0f;
				}
				float num4 = (float)renderBounds.Bottom - pointF2.Y;
				if (num4 < 0f && Math.Abs(num4) < sizeF.Width)
				{
					pointF2.X = renderBounds.Bottom;
					num4 = 0f;
					num3 = -renderBounds.Height;
				}
				float num5 = (float)renderBounds.Left - pointF2.X;
				if (num5 > 0f && Math.Abs(num5) < sizeF.Width)
				{
					pointF2.X = renderBounds.Left;
					num5 = 0f;
				}
				float num6 = (float)renderBounds.Right - pointF2.X;
				if (num6 < 0f && Math.Abs(num6) < sizeF.Width)
				{
					pointF2.X = renderBounds.Right;
					num6 = 0f;
					num5 = -renderBounds.Width;
				}
				float num7 = num3 / (float)Math.Sin(num2);
				if (num7 <= 0f)
				{
					num7 = float.MaxValue;
				}
				float num8 = num4 / (float)Math.Sin(num2);
				if (num8 <= 0f)
				{
					num8 = float.MaxValue;
				}
				float num9 = num5 / (float)Math.Cos(num2);
				if (num9 <= 0f)
				{
					num9 = float.MaxValue;
				}
				float num10 = num6 / (float)Math.Cos(num2);
				if (num10 <= 0f)
				{
					num10 = float.MaxValue;
				}
				float val = Math.Min(num7, num8);
				val = Math.Min(num9, val);
				val = Math.Min(num10, val);
				sizeF = ((style.Format == null) ? g.MeasureString(style.Text, style.GdipFont, (int)Math.Round(val - style.GdipFont.GetHeight() / 2f)) : g.MeasureString(style.Text, style.GdipFont));
				sizeF.Width += 1f;
			}
			pointF2.Y += (0f - sizeF.Height) / 2f + pointF.Y;
			if (num2 == 0.0)
			{
				pointF2.X += (0f - sizeF.Width) / 2f + pointF.X;
				float num11 = (float)renderBounds.Top - pointF2.Y;
				float num12 = (float)renderBounds.Bottom - pointF2.Y;
				float num13 = (float)renderBounds.Left - pointF2.X;
				float num14 = (float)renderBounds.Right - (pointF2.X + sizeF.Width);
				if (num11 > 0f && Math.Abs(num11) < sizeF.Height)
				{
					pointF2.Y = renderBounds.Top;
				}
				if (num12 < 0f && Math.Abs(num12) < sizeF.Height)
				{
					pointF2.Y = (float)renderBounds.Bottom - sizeF.Height;
				}
				if (num13 > 0f && Math.Abs(num13) < sizeF.Width)
				{
					pointF2.X = renderBounds.Left;
					sizeF = ((style.Format == null) ? g.MeasureString(style.Text, style.GdipFont, (int)Math.Round(sizeF.Width - num13)) : g.MeasureString(style.Text, style.GdipFont));
				}
				if (num14 < 0f && Math.Abs(num14) < sizeF.Width)
				{
					sizeF = ((style.Format == null) ? g.MeasureString(style.Text, style.GdipFont, (int)Math.Round(sizeF.Width + num14)) : g.MeasureString(style.Text, style.GdipFont));
				}
				sizeF.Width += 1f;
			}
		}
		if (pointF2.IsEmpty)
		{
			return;
		}
		if (style.DrawTextShape)
		{
			Rectangle rectangle = new Rectangle((int)pointF2.X, (int)pointF2.Y, (int)sizeF.Width, (int)sizeF.Height);
			switch (style.TextShape.Type)
			{
			case ChartCustomShape.Circle:
			{
				using (SolidBrush brush2 = new SolidBrush(style.TextShape.Color))
				{
					rectangle = new Rectangle((int)pointF2.X - 2, (int)pointF2.Y - 2, (int)sizeF.Width + 5, (int)sizeF.Height + 5);
					g.DrawEllipse(style.TextShape.BorderGdiPen, rectangle);
					g.FillEllipse(brush2, rectangle);
				}
				break;
			}
			case ChartCustomShape.Pentagon:
			{
				rectangle = new Rectangle((int)pointF2.X - 5, (int)pointF2.Y - 5, (int)sizeF.Width + 10, (int)sizeF.Height + 10);
				using (SolidBrush pen2 = new SolidBrush(style.TextShape.Color))
				{
					PointF pointF9 = new PointF((float)rectangle.X + (float)rectangle.Width / 5f, rectangle.Y);
					PointF pointF10 = new PointF((float)(rectangle.X + rectangle.Width) - (float)rectangle.Width / 5f, rectangle.Y);
					PointF pointF11 = new PointF(rectangle.Right, (float)rectangle.Y + (float)rectangle.Height * 0.6f);
					PointF pointF12 = new PointF((float)rectangle.X + (float)rectangle.Width / 2f, rectangle.Bottom);
					PointF pointF13 = new PointF(rectangle.X, (float)rectangle.Y + (float)rectangle.Height * 0.6f);
					PointF[] points2 = new PointF[5] { pointF9, pointF10, pointF11, pointF12, pointF13 };
					g.DrawPolygon(style.TextShape.BorderGdiPen, points2);
					g.FillPolygon(pen2, points2);
				}
				break;
			}
			case ChartCustomShape.Hexagon:
			{
				rectangle = new Rectangle((int)pointF2.X - 5, (int)pointF2.Y - 5, (int)sizeF.Width + 10, (int)sizeF.Height + 10);
				using (SolidBrush pen = new SolidBrush(style.TextShape.Color))
				{
					PointF pointF3 = new PointF(rectangle.X + rectangle.Width / 4, rectangle.Y);
					PointF pointF4 = new PointF((float)rectangle.X + (float)rectangle.Width * 0.75f, rectangle.Y);
					PointF pointF5 = new PointF(rectangle.Right, rectangle.Y + rectangle.Height / 2);
					PointF pointF6 = new PointF((float)rectangle.X + (float)rectangle.Width * 0.75f, rectangle.Bottom);
					PointF pointF7 = new PointF(rectangle.X + rectangle.Width / 4, rectangle.Bottom);
					PointF pointF8 = new PointF(rectangle.X, rectangle.Y + rectangle.Height / 2);
					PointF[] points = new PointF[6] { pointF3, pointF4, pointF5, pointF6, pointF7, pointF8 };
					g.DrawPolygon(style.TextShape.BorderGdiPen, points);
					g.FillPolygon(pen, points);
				}
				break;
			}
			default:
			{
				using (SolidBrush brush = new SolidBrush(style.TextShape.Color))
				{
					g.DrawRectangle(style.TextShape.BorderGdiPen, rectangle);
					g.FillRectangle(brush, rectangle);
				}
				break;
			}
			}
		}
		RenderingHelper.DrawText(g, style, pointF2, sizeF);
	}

	protected virtual void DrawCalloutShapeText(Graphics g, ChartStyledPoint styledPoint, PointF pointPos)
	{
		ChartCalloutInfo callout = styledPoint.Style.Callout;
		_ = styledPoint.Index;
		PointF calloutPos = new PointF(pointPos.X, pointPos.Y);
		_ = pointPos.X;
		_ = pointPos.Y;
		float num = 0f;
		float textOffset = callout.TextOffset;
		float offsetX = callout.OffsetX;
		float offsetY = callout.OffsetY;
		SizeF empty = SizeF.Empty;
		SizeF sizeF = SizeF.Empty;
		SizeF sizeF2 = SizeF.Empty;
		Rectangle rectangle = default(Rectangle);
		string[] array = Regex.Split(callout.Text, "\n");
		LabelPosition position = callout.Position;
		string[] array2 = array;
		foreach (string text in array2)
		{
			sizeF2 = g.MeasureString(text, callout.Font.GdipFont);
			if (empty.Width < sizeF2.Width)
			{
				empty.Width = sizeF2.Width;
			}
			empty.Height += sizeF2.Height + textOffset * 1f;
			sizeF = new SizeF(empty.Width + textOffset * 2f, empty.Height + textOffset);
		}
		if (callout.IsDragged)
		{
			callout.Position = LabelPosition.Center;
			Rectangle bounds = ChartArea.Bounds;
			float num2 = bounds.X + bounds.Width;
			float num3 = bounds.Y + bounds.Height;
			float x = callout.HiddenX / 100f * num2;
			float y = callout.HiddenY / 100f * num3;
			calloutPos.X = x;
			calloutPos.Y = y;
			rectangle = new Rectangle((int)calloutPos.X, (int)calloutPos.Y, (int)sizeF.Width, (int)sizeF.Height);
			CalculatePolygonPosition(callout, rectangle, pointPos);
		}
		calloutPos = ChangeCalloutPosition(callout, calloutPos, sizeF, offsetX, offsetY);
		rectangle = new Rectangle((int)calloutPos.X, (int)calloutPos.Y, (int)sizeF.Width, (int)sizeF.Height);
		using (SolidBrush pen = new SolidBrush(callout.Color))
		{
			PointF[] points = CalculateCalloutPath(callout, rectangle, pointPos);
			g.DrawPolygon(callout.Border.GdipPen, points);
			g.FillPolygon(pen, points);
		}
		calloutPos.X += textOffset;
		calloutPos.Y += textOffset;
		num = rectangle.Y;
		array2 = array;
		foreach (string text2 in array2)
		{
			if (!(text2 == ""))
			{
				calloutPos.Y = num + textOffset;
				using (Brush brush = new SolidBrush(callout.TextColor))
				{
					g.DrawString(text2, callout.Font.GdipFont, brush, Rectangle.Round(new RectangleF(calloutPos, sizeF)));
				}
				num = calloutPos.Y + sizeF2.Height;
			}
		}
		callout.Position = position;
	}

	internal PointF[] CalculateCalloutPath(ChartCalloutInfo callout, Rectangle bounds, PointF pointPos)
	{
		PointF[] result = null;
		switch (callout.Position)
		{
		case LabelPosition.Top:
		{
			PointF pointF26 = new PointF(bounds.X, bounds.Y);
			PointF pointF27 = new PointF(bounds.X + bounds.Width, bounds.Y);
			PointF pointF28 = new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height);
			PointF pointF29 = new PointF((float)bounds.X + (float)bounds.Width * 0.35f, bounds.Y + bounds.Height);
			PointF pointF30 = new PointF(pointPos.X, pointPos.Y);
			PointF pointF31 = new PointF((float)bounds.X + (float)bounds.Width * 0.2f, bounds.Y + bounds.Height);
			PointF pointF32 = new PointF(bounds.X, bounds.Y + bounds.Height);
			result = new PointF[8] { pointF26, pointF27, pointF28, pointF29, pointF30, pointF31, pointF32, pointF26 };
			break;
		}
		case LabelPosition.Bottom:
		{
			PointF pointF19 = new PointF(bounds.X, bounds.Y);
			PointF pointF20 = new PointF((float)bounds.X + (float)bounds.Width * 0.2f, bounds.Y);
			PointF pointF21 = new PointF(pointPos.X, pointPos.Y);
			PointF pointF22 = new PointF((float)bounds.X + (float)bounds.Width * 0.35f, bounds.Y);
			PointF pointF23 = new PointF(bounds.X + bounds.Width, bounds.Y);
			PointF pointF24 = new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height);
			PointF pointF25 = new PointF(bounds.X, bounds.Y + bounds.Height);
			result = new PointF[8] { pointF19, pointF20, pointF21, pointF22, pointF23, pointF24, pointF25, pointF19 };
			break;
		}
		case LabelPosition.Left:
		{
			PointF pointF12 = new PointF(bounds.X, bounds.Y);
			PointF pointF13 = new PointF(bounds.X + bounds.Width, bounds.Y);
			PointF pointF14 = new PointF(bounds.X + bounds.Width, (float)bounds.Y + (float)bounds.Height * 0.2f);
			PointF pointF15 = new PointF(pointPos.X, pointPos.Y);
			PointF pointF16 = new PointF(bounds.X + bounds.Width, (float)bounds.Y + (float)bounds.Height * 0.35f);
			PointF pointF17 = new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height);
			PointF pointF18 = new PointF(bounds.X, bounds.Y + bounds.Height);
			result = new PointF[8] { pointF12, pointF13, pointF14, pointF15, pointF16, pointF17, pointF18, pointF12 };
			break;
		}
		case LabelPosition.Right:
		{
			PointF pointF5 = new PointF(bounds.X, bounds.Y);
			PointF pointF6 = new PointF(bounds.X + bounds.Width, bounds.Y);
			PointF pointF7 = new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height);
			PointF pointF8 = new PointF(bounds.X, bounds.Y + bounds.Height);
			PointF pointF9 = new PointF(bounds.X, (float)bounds.Y + (float)bounds.Height * 0.35f);
			PointF pointF10 = new PointF(pointPos.X, pointPos.Y);
			PointF pointF11 = new PointF(bounds.X, (float)bounds.Y + (float)bounds.Height * 0.2f);
			result = new PointF[8] { pointF5, pointF6, pointF7, pointF8, pointF9, pointF10, pointF11, pointF5 };
			break;
		}
		case LabelPosition.Center:
		{
			PointF pointF = new PointF(bounds.X, bounds.Y);
			PointF pointF2 = new PointF(bounds.X + bounds.Width, bounds.Y);
			PointF pointF3 = new PointF(bounds.X + bounds.Width, bounds.Y + bounds.Height);
			PointF pointF4 = new PointF(bounds.X, bounds.Y + bounds.Height);
			result = new PointF[5] { pointF, pointF2, pointF3, pointF4, pointF };
			break;
		}
		}
		return result;
	}

	internal PointF ChangeCalloutPosition(ChartCalloutInfo callout, PointF calloutPos, SizeF siz, float offsetX, float offsetY)
	{
		Rectangle renderBounds = ChartArea.RenderBounds;
		bool flag = false;
		if (!callout.IsDragged)
		{
			switch (callout.Position)
			{
			case LabelPosition.Top:
				calloutPos.X -= siz.Width / 2f + offsetX;
				calloutPos.Y -= siz.Height + 10f + Math.Abs(offsetY);
				break;
			case LabelPosition.Bottom:
				calloutPos.X -= siz.Width / 2f + offsetX;
				calloutPos.Y += 10f + Math.Abs(offsetY);
				break;
			case LabelPosition.Left:
				calloutPos.X -= siz.Width + 10f + Math.Abs(offsetX);
				calloutPos.Y -= siz.Height / 2f + offsetY;
				break;
			case LabelPosition.Right:
				calloutPos.X += 10f + Math.Abs(offsetX);
				calloutPos.Y -= siz.Height / 2f + offsetY;
				break;
			case LabelPosition.Center:
				calloutPos.X -= siz.Width / 2f + offsetX;
				calloutPos.Y -= siz.Height / 2f + offsetY;
				break;
			}
		}
		if (calloutPos.X < (float)renderBounds.X)
		{
			calloutPos.X = renderBounds.X;
			flag = true;
		}
		if (calloutPos.Y < (float)renderBounds.Y)
		{
			calloutPos.Y = renderBounds.Y;
			flag = true;
		}
		if (calloutPos.X + siz.Width > (float)(renderBounds.X + renderBounds.Width))
		{
			calloutPos.X = (float)(renderBounds.X + renderBounds.Width) - siz.Width;
			flag = true;
		}
		if (calloutPos.Y + siz.Height > (float)(renderBounds.Y + renderBounds.Height))
		{
			calloutPos.Y = (float)(renderBounds.Y + renderBounds.Height) - siz.Height;
			flag = true;
		}
		if (!callout.IsDragged && flag)
		{
			callout.Position = LabelPosition.Center;
		}
		return calloutPos;
	}

	internal void CalculatePolygonPosition(ChartCalloutInfo callout, Rectangle bnds, PointF pointPos)
	{
		int num = 5;
		if ((float)bnds.X > pointPos.X + (float)num)
		{
			callout.Position = LabelPosition.Right;
		}
		if ((float)bnds.Y > pointPos.Y + (float)num)
		{
			callout.Position = LabelPosition.Bottom;
		}
		if ((float)(bnds.X + bnds.Width) < pointPos.X - (float)num)
		{
			callout.Position = LabelPosition.Left;
		}
		if ((float)(bnds.Y + bnds.Height) < pointPos.Y - (float)num)
		{
			callout.Position = LabelPosition.Top;
		}
	}

	protected virtual void DrawText(Graphics3D g, ChartStyledPoint styledPoint, Vector3D p, Size sz)
	{
		ChartStyleInfo style = styledPoint.Style;
		int index = styledPoint.Index;
		PointF pointF = new PointF(0f, 0f);
		PointF pointF2 = new PointF((float)p.X, (float)p.Y);
		PointF symbolPoint = new PointF((float)p.X, (float)p.Y);
		SizeF sizeF = new SizeF((float)(sz.Width / 2) + style.TextOffset, (float)(sz.Height / 2) + style.TextOffset);
		switch (style.TextOrientation)
		{
		case ChartTextOrientation.Up:
			pointF2.Y -= sizeF.Height;
			break;
		case ChartTextOrientation.Down:
			pointF2.Y += sizeF.Height;
			break;
		case ChartTextOrientation.Left:
			pointF2.X -= sizeF.Width;
			break;
		case ChartTextOrientation.Right:
			pointF2.X += sizeF.Width;
			break;
		case ChartTextOrientation.UpLeft:
			pointF2.X -= sizeF.Width;
			pointF2.Y -= sizeF.Height;
			break;
		case ChartTextOrientation.DownLeft:
			pointF2.X -= sizeF.Width;
			pointF2.Y += sizeF.Height;
			break;
		case ChartTextOrientation.UpRight:
			pointF2.X += sizeF.Width;
			pointF2.Y -= sizeF.Height;
			break;
		case ChartTextOrientation.DownRight:
			pointF2.X += sizeF.Width;
			pointF2.Y += sizeF.Height;
			break;
		case ChartTextOrientation.Smart:
			if (m_chart.RequireInvertedAxes)
			{
				if (pointF2.X < CustomOriginX)
				{
					pointF2.X += 0f - sizeF.Width;
				}
				else
				{
					pointF2.X += sizeF.Width;
				}
			}
			else if (pointF2.Y < CustomOriginY)
			{
				pointF2.Y += 0f - sizeF.Height;
			}
			else
			{
				pointF2.Y += sizeF.Height;
			}
			break;
		case ChartTextOrientation.RegionUp:
			if (m_series.RequireInvertedAxes)
			{
				pointF2.X -= sz.Width;
			}
			else
			{
				pointF2.Y += sz.Height;
			}
			break;
		case ChartTextOrientation.RegionDown:
		{
			float lowerAnchorPointValue2 = GetLowerAnchorPointValue(index);
			if (m_series.RequireInvertedAxes)
			{
				pointF2.X = lowerAnchorPointValue2;
			}
			else
			{
				pointF2.Y = lowerAnchorPointValue2 - (float)sz.Height;
			}
			break;
		}
		case ChartTextOrientation.RegionCenter:
		{
			float lowerAnchorPointValue = GetLowerAnchorPointValue(index);
			if (m_series.RequireInvertedAxes)
			{
				pointF2.X = lowerAnchorPointValue + (pointF2.X - lowerAnchorPointValue - (float)(sz.Width / 2)) / 2f;
			}
			else
			{
				pointF2.Y = lowerAnchorPointValue - (lowerAnchorPointValue - pointF2.Y + (float)(sz.Height / 2)) / 2f;
			}
			break;
		}
		case ChartTextOrientation.SymbolCenter:
		{
			PointF symbolPoint2 = GetSymbolPoint(styledPoint);
			pointF2.X = symbolPoint2.X - (float)(sz.Width / 2);
			pointF2.Y = symbolPoint2.Y - (float)(sz.Height / 2);
			break;
		}
		}
		Rectangle renderBounds = ChartArea.RenderBounds;
		float num = (float)Math.PI * (float)style.Font.Orientation / 180f;
		if (num != 0f)
		{
			float num2 = (float)renderBounds.Top - pointF2.Y;
			if (num2 > 0f && Math.Abs(num2) < (float)sz.Width)
			{
				pointF2.X = renderBounds.Top;
				num2 = 0f;
			}
			float num3 = (float)renderBounds.Bottom - pointF2.Y;
			if (num3 < 0f && Math.Abs(num3) < (float)sz.Width)
			{
				pointF2.X = renderBounds.Bottom;
				num3 = 0f;
				num2 = -renderBounds.Height;
			}
			float num4 = (float)renderBounds.Left - pointF2.X;
			if (num4 > 0f && Math.Abs(num4) < (float)sz.Width)
			{
				pointF2.X = renderBounds.Left;
				num4 = 0f;
			}
			float num5 = (float)renderBounds.Right - pointF2.X;
			if (num5 < 0f && Math.Abs(num5) < (float)sz.Width)
			{
				pointF2.X = renderBounds.Right;
				num5 = 0f;
				num4 = -renderBounds.Width;
			}
			float num6 = num2 / (float)Math.Sin(num);
			if (num6 < 0f)
			{
				num6 = float.MaxValue;
			}
			float num7 = num3 / (float)Math.Sin(num);
			if (num7 < 0f)
			{
				num7 = float.MaxValue;
			}
			float num8 = num4 / (float)Math.Cos(num);
			if (num8 < 0f)
			{
				num8 = float.MaxValue;
			}
			float num9 = num5 / (float)Math.Cos(num);
			if (num9 < 0f)
			{
				num9 = float.MaxValue;
			}
			float val = Math.Min(num6, num7);
			val = Math.Min(num8, val);
			val = Math.Min(num9, val);
			sz = g.Graphics.MeasureString(style.Text, style.GdipFont, (int)val - (int)style.GdipFont.GetHeight() / 2).ToSize();
			sz.Width++;
		}
		pointF2.X += (float)(-sz.Width / 2) + pointF.X;
		pointF2.Y += (float)(-sz.Height / 2) + pointF.Y;
		if (num == 0f)
		{
			float num10 = (float)renderBounds.Top - pointF2.Y;
			float num11 = (float)renderBounds.Bottom - pointF2.Y;
			float num12 = (float)renderBounds.Left - pointF2.X;
			float num13 = (float)renderBounds.Right - (pointF2.X + (float)sz.Width);
			if (num10 > 0f && Math.Abs(num10) < (float)sz.Height)
			{
				pointF2.Y = renderBounds.Top;
			}
			if (num11 < 0f && Math.Abs(num11) < (float)sz.Height)
			{
				pointF2.Y = renderBounds.Bottom - sz.Height;
			}
			if (num12 > 0f && Math.Abs(num12) < (float)sz.Width)
			{
				pointF2.X = renderBounds.Left;
				sz = g.Graphics.MeasureString(style.Text, style.GdipFont, (int)((float)sz.Width - num12)).ToSize();
			}
			if (num13 < 0f && Math.Abs(num13) < (float)sz.Width)
			{
				sz = g.Graphics.MeasureString(style.Text, style.GdipFont, (int)((float)sz.Width + num13)).ToSize();
			}
			sz.Width++;
		}
		Brush brush = new SolidBrush(style.TextColor);
		GraphicsPath graphicsPath = new GraphicsPath();
		Font gdipFont = style.Font.GdipFont;
		if (m_series.SmartLabels)
		{
			pointF2 = m_labelLayoutManager.AddLabel(new ChartLabel(pointF2, symbolPoint, sz, SizeF.Empty)).Location;
		}
		if (!pointF2.IsEmpty)
		{
			if (style.Font.Orientation == 0)
			{
				graphicsPath.AddString(layoutRect: new RectangleF(pointF2, sz), s: style.Text, family: gdipFont.GetFontFamily(), style: (int)gdipFont.Style, emSize: RenderingHelper.GetFontSizeInPixels(gdipFont), format: StringFormat.GenericDefault);
			}
			else
			{
				Matrix matrix = new Matrix();
				matrix.Translate(pointF2.X + (float)(sz.Width / 2), pointF2.Y + (float)sz.Height / 2f);
				matrix.Rotate(style.Font.Orientation, MatrixOrder.Prepend);
				graphicsPath.AddString(layoutRect: new RectangleF(new PointF(style.TextOffset, (float)(-sz.Height) / 2f), sz), s: style.Text, family: gdipFont.GetFontFamily(), style: (int)gdipFont.Style, emSize: RenderingHelper.GetFontSizeInPixels(gdipFont), format: StringFormat.GenericDefault);
				graphicsPath.Transform(matrix);
			}
			g.AddPolygon(Path3D.FromGraphicsPath(graphicsPath, p.Z - 1.0, (SolidBrush)brush.Clone()));
		}
		gdipFont.Dispose();
		brush.Dispose();
		brush = null;
		gdipFont = null;
	}

	protected internal virtual void RenderAdornments(Graphics g)
	{
		if (m_series.BaseType == ChartSeriesBaseType.Single)
		{
			return;
		}
		IndexRange indexRange = CalculateVisibleRange();
		ChartStyledPoint[] array = StyledPoints ?? PrepearePoints();
		if (m_series.SmartLabels)
		{
			m_labelLayoutManager = new ChartLabelLayoutManager(m_bounds);
			SizeF minimalSize = new SizeF(float.MaxValue, float.MaxValue);
			for (int i = indexRange.From; i <= indexRange.To; i++)
			{
				if (array[i].IsVisible)
				{
					ChartStyleInfo style = array[i].Style;
					SizeF sizeF = g.MeasureString(style.Text, style.GdipFont).ToSize();
					if (minimalSize.Width > sizeF.Width)
					{
						minimalSize.Width = sizeF.Width;
					}
					if (minimalSize.Height > sizeF.Height)
					{
						minimalSize.Height = sizeF.Height;
					}
				}
			}
			m_labelLayoutManager.MinimalSize = minimalSize;
			for (int j = indexRange.From; j <= indexRange.To; j++)
			{
				if (array[j].IsVisible)
				{
					m_labelLayoutManager.AddPoint(GetPointFromValue(array[j]));
				}
			}
		}
		for (int k = indexRange.From; k <= indexRange.To; k++)
		{
			if (array[k].IsVisible)
			{
				RenderAdornment(g, array[k]);
			}
		}
	}

	protected internal virtual void RenderAdornments(Graphics3D g)
	{
		m_labelLayoutManager = new ChartLabelLayoutManager(m_bounds);
		if (m_series.BaseType == ChartSeriesBaseType.Single)
		{
			return;
		}
		IndexRange indexRange = CalculateVisibleRange();
		ChartStyledPoint[] array = PrepearePoints();
		for (int i = indexRange.From; i <= indexRange.To; i++)
		{
			ChartStyledPoint chartStyledPoint = array[i];
			ChartStyleInfo style = chartStyledPoint.Style;
			if (chartStyledPoint.IsVisible)
			{
				if (style.Symbol.Shape != 0)
				{
					DrawPointSymbol(g, chartStyledPoint);
				}
				if (style.DisplayText && style.Text != string.Empty && !style.Callout.Enable)
				{
					Vector3D symbolVector = GetSymbolVector(chartStyledPoint);
					Size sz = g.Graphics.MeasureString(style.Text, style.GdipFont).ToSize();
					DrawText(g, chartStyledPoint, symbolVector, sz);
				}
			}
		}
	}

	protected virtual void RenderAdornment(Graphics g, ChartStyledPoint point)
	{
		ChartStyleInfo style = point.Style;
		PointF pointF = PointF.Empty;
		if (!style.Callout.Enable)
		{
			if (style.Symbol.Shape != 0)
			{
				pointF = ((m_series.Renderer == this) ? GetSymbolPoint(point) : m_series.Renderer.GetSymbolPoint(point));
				RenderingHelper.DrawPointSymbol(g, point.Style, pointF, drawMarker: false);
			}
			if (style.DisplayText && style.Text != string.Empty)
			{
				if (pointF.IsEmpty)
				{
					pointF = ((m_series.Renderer == this) ? GetSymbolPoint(point) : m_series.Renderer.GetSymbolPoint(point));
				}
				DrawText(g, point, pointF);
			}
		}
		else
		{
			if (!style.Callout.Enable)
			{
				return;
			}
			if (pointF.IsEmpty)
			{
				pointF = GetSymbolPoint(point);
			}
			if (!pointF.IsEmpty)
			{
				int index = point.Index;
				string displayTextAndFormat = style.Callout.DisplayTextAndFormat;
				object[] array = new object[2 + m_series.Points[index].YValues.Length];
				array[0] = m_series.Name;
				if (m_series.Points.isCategory)
				{
					array[1] = m_series.Points[index].Category;
				}
				else if (m_series.XAxis.ValueType == ChartValueType.DateTime)
				{
					array[1] = m_series.Points[index].DateX;
				}
				else
				{
					array[1] = m_series.Points[index].X;
				}
				array[2] = m_series.Points[index].YValues[0];
				for (int i = 3; i < array.Length; i++)
				{
					array[i] = m_series.Points[point.Index].YValues[i - 2];
				}
				style.Callout.Text = string.Format(displayTextAndFormat, array);
				DrawCalloutShapeText(g, point, pointF);
			}
		}
	}

	internal void RenderErrorBar(Graphics g, ChartStyledPoint point)
	{
		ChartErrorBarsConfigItem errorBars = m_series.ConfigItems.ErrorBars;
		if (!errorBars.Enabled)
		{
			return;
		}
		PointF symbolPoint = GetSymbolPoint(point);
		ChartSymbolShape symbolShape = errorBars.SymbolShape;
		Size symbolSize = new SizeF(4.5f, errorBars.Width).ToSize();
		PointF empty = PointF.Empty;
		PointF empty2 = PointF.Empty;
		double num = 0.0;
		double num2 = 0.0;
		double x = point.X;
		double num3 = point.YValues[0];
		bool flag = false;
		bool flag2 = false;
		if (errorBars.ValueType == OfficeErrorBarType.Fixed)
		{
			num = errorBars.FixedValue;
			num2 = errorBars.FixedValue;
			flag = true;
			flag2 = true;
		}
		else if (errorBars.ValueType == OfficeErrorBarType.Custom)
		{
			if (errorBars.PlusValues.Count >= point.Index + 1 && errorBars.PlusValues[point.Index] != double.NaN)
			{
				num = errorBars.PlusValues[point.Index];
				flag = true;
			}
			if (errorBars.MinusValues.Count >= point.Index + 1 && errorBars.MinusValues[point.Index] != double.NaN)
			{
				num2 = errorBars.MinusValues[point.Index];
				flag2 = true;
			}
		}
		else if (errorBars.ValueType == OfficeErrorBarType.Percentage)
		{
			num = num3 * (errorBars.FixedValue / 100.0);
			num2 = num;
			flag = true;
			flag2 = true;
		}
		else if (errorBars.ValueType == OfficeErrorBarType.StandardError)
		{
			if (!isStandardErrorCalculated)
			{
				errorBars.FixedValue = GetStandardError();
				isStandardErrorCalculated = true;
			}
			num = errorBars.FixedValue;
			num2 = num;
			flag = true;
			flag2 = true;
		}
		else
		{
			if (!isStandardDeviationCalculated)
			{
				ChartStyledPoint[] styledPoints = GetStyledPoints();
				errorBars.Mean = GetMeanValue(styledPoints);
				double standardDeviation = GetStandardDeviation(styledPoints, errorBars.Mean);
				errorBars.FixedValue = standardDeviation * errorBars.FixedValue;
				isStandardDeviationCalculated = true;
			}
			if (errorBars.Orientation == ChartOrientation.Horizontal)
			{
				x = errorBars.Mean;
			}
			else
			{
				num3 = errorBars.Mean;
			}
			num = errorBars.FixedValue;
			num2 = num;
			flag = true;
			flag2 = true;
		}
		string text = m_series.Type.ToString();
		if (text.Contains("Stack"))
		{
			num3 = GetStackInfoValue(point.Index, isWithMe: true);
			if (text.Contains("100"))
			{
				double num4 = 0.0;
				foreach (object visible in m_series.ChartModel.Series.VisibleList)
				{
					ChartSeries chartSeries = visible as ChartSeries;
					num4 += chartSeries.Points[point.Index].YValues[0];
				}
				num = num / num4 * 100.0;
				num2 = num2 / num4 * 100.0;
			}
		}
		if (errorBars.Orientation == ChartOrientation.Horizontal)
		{
			if (errorBars.Type == OfficeErrorBarInclude.Both)
			{
				empty = GetPointFromValue(x, num3 + num);
				empty2 = GetPointFromValue(x, num3 - num2);
			}
			else if (errorBars.Type == OfficeErrorBarInclude.Plus)
			{
				empty = GetPointFromValue(x, num3 + num);
				empty2 = GetPointFromValue(x, num3);
			}
			else
			{
				empty = GetPointFromValue(x, num3);
				empty2 = GetPointFromValue(x, num3 - num2);
			}
			empty = new PointF(empty.X, symbolPoint.Y);
			empty2 = new PointF(empty2.X, symbolPoint.Y);
			symbolSize = new Size(symbolSize.Height, symbolSize.Width);
		}
		else
		{
			if (errorBars.Type == OfficeErrorBarInclude.Both)
			{
				empty = GetPointFromValue(x, num3 + num);
				empty2 = GetPointFromValue(x, num3 - num2);
			}
			else if (errorBars.Type == OfficeErrorBarInclude.Plus)
			{
				empty = GetPointFromValue(x, num3 + num);
				empty2 = GetPointFromValue(x, num3);
			}
			else
			{
				empty = GetPointFromValue(x, num3);
				empty2 = GetPointFromValue(x, num3 - num2);
			}
			empty = new PointF(symbolPoint.X, empty.Y);
			empty2 = new PointF(symbolPoint.X, empty2.Y);
		}
		Pen gdipPen = point.Style.GdipPen;
		gdipPen.Color = errorBars.Color;
		gdipPen.Width = errorBars.Width;
		gdipPen.DashStyle = errorBars.DashStyle;
		gdipPen.SKPaint.PathEffect = m_series.GetDashStyle(gdipPen, gdipPen.Width);
		if ((errorBars.Type == OfficeErrorBarInclude.Plus && flag) || (errorBars.Type == OfficeErrorBarInclude.Minus && flag2) || (errorBars.Type == OfficeErrorBarInclude.Both && (flag || flag2)))
		{
			g.DrawLine(gdipPen, empty.X, empty.Y, empty2.X, empty2.Y);
		}
		using Brush brush = new SolidBrush(errorBars.Color);
		if (errorBars.Type == OfficeErrorBarInclude.Both)
		{
			if (flag)
			{
				RenderingHelper.DrawPointSymbol(g, symbolShape, null, symbolSize, Size.Empty, 0, brush, gdipPen, point.Style.Images, empty, drawMarker: false);
			}
			if (flag2)
			{
				RenderingHelper.DrawPointSymbol(g, symbolShape, null, symbolSize, Size.Empty, 0, brush, gdipPen, point.Style.Images, empty2, drawMarker: false);
			}
		}
		else if (errorBars.Type == OfficeErrorBarInclude.Plus)
		{
			if (flag)
			{
				RenderingHelper.DrawPointSymbol(g, symbolShape, null, symbolSize, Size.Empty, 0, brush, gdipPen, point.Style.Images, empty, drawMarker: false);
			}
		}
		else if (flag2)
		{
			RenderingHelper.DrawPointSymbol(g, symbolShape, null, symbolSize, Size.Empty, 0, brush, gdipPen, point.Style.Images, empty2, drawMarker: false);
		}
	}

	internal void SetBoundsAndRange(IChartAreaHost chart, RectangleF bounds, ChartAxis xAxis, ChartAxis yAxis)
	{
		m_chart = chart;
		m_bounds = bounds;
		m_xAxis = xAxis;
		m_yAxis = yAxis;
	}

	internal ChartStyledPoint[] GetStyledPoints()
	{
		return StyledPoints ?? PrepearePoints();
	}

	internal double GetStandardError()
	{
		ChartStyledPoint[] styledPoints = GetStyledPoints();
		return GetStandardDeviation(styledPoints, GetMeanValue(styledPoints)) / Math.Sqrt(styledPoints.Length);
	}

	internal double GetMeanValue(ChartStyledPoint[] styledPoints)
	{
		double num = 0.0;
		foreach (ChartStyledPoint chartStyledPoint in styledPoints)
		{
			num += chartStyledPoint.YValues[0];
		}
		return num / (double)styledPoints.Length;
	}

	internal double GetStandardDeviation(ChartStyledPoint[] styledPoints, double mean)
	{
		double num = 0.0;
		foreach (ChartStyledPoint chartStyledPoint in styledPoints)
		{
			num += Math.Pow(chartStyledPoint.YValues[0] - mean, 2.0);
		}
		return Math.Sqrt(num / (double)(styledPoints.Length - 1));
	}

	protected Polygon CreateBoundsPolygon(float z)
	{
		return new Polygon(new Vector3D[4]
		{
			new Vector3D(m_bounds.Left, m_bounds.Top, z),
			new Vector3D(m_bounds.Right, m_bounds.Top, z),
			new Vector3D(m_bounds.Right, m_bounds.Bottom, z),
			new Vector3D(m_bounds.Left, m_bounds.Bottom, z)
		});
	}

	protected virtual ChartStyleInfo GetStyleAt(int index)
	{
		ChartStyleInfo chartStyleInfo = null;
		if (m_series.EnableStyles)
		{
			if (m_shouldUpdate)
			{
				FillStyles();
			}
			if (index < m_styles.Count)
			{
				return m_styles[index] as ChartStyleInfo;
			}
			if (index < m_series.Points.Count)
			{
				return m_series.GetOfflineStyle(index);
			}
			return SeriesStyle;
		}
		return SeriesStyle;
	}

	internal void FillStyles()
	{
		m_styleUpdating = true;
		m_styles.Clear();
		int i = 0;
		for (int count = m_series.Points.Count; i < count; i++)
		{
			ChartStyleInfo offlineStyle = m_series.GetOfflineStyle(i);
			m_styles.Add(offlineStyle);
			IList<ChartStyledPoint> cache = GetCache();
			if (cache.Count > i && cache[i] != null)
			{
				m_pointsCache[i].Style = offlineStyle;
			}
			m_shouldUpdate = false;
		}
		m_styleUpdating = false;
	}

	protected bool IsVisiblePoint(ChartPoint cpt)
	{
		if (!cpt.IsEmpty && cpt.YValues != null && cpt.YValues.Length >= RequireYValuesCount && !double.IsNaN(cpt.YValues[0]))
		{
			return !double.IsNaN(cpt.X);
		}
		return false;
	}

	protected ChartStyledPoint[] PrepearePoints()
	{
		bool flag = false;
		if (m_points == null || m_series.SortPoints)
		{
			ArrayList arrayList = new ArrayList();
			ChartAxis actualXAxis = m_series.ActualXAxis;
			if (actualXAxis.ValueType == ChartValueType.Category && actualXAxis.SortLabels.Length < 1)
			{
				actualXAxis.RecalculateVisibleLabels((ChartArea)m_chart.GetChartArea());
			}
			arrayList.AddRange(actualXAxis.SortLabels);
			flag = m_points == null || flag;
			if (m_points == null)
			{
				m_isSorted = false;
				ResetCache();
				GetCache();
				for (int i = 0; i < m_pointsCache.Count; i++)
				{
					ChartStyledPoint chartStyledPoint = m_pointsCache[i];
					if (chartStyledPoint == null)
					{
						chartStyledPoint = CreateStyledPoint(i);
						m_pointsCache[i] = chartStyledPoint;
					}
				}
			}
			m_points = m_pointsCache.ToArray();
			bool sortPoints = m_series.SortPoints;
			if (ShouldSort && sortPoints)
			{
				m_isSorted = true;
				if (m_series.SortBy == ChartSeriesSortingType.X)
				{
					if (m_series.ActualXAxis.ValueType != ChartValueType.Category)
					{
						if (m_series.SortOrder == ChartSeriesSortingOrder.Ascending)
						{
							Array.Sort(m_points, new ChartStyledPointComparer());
						}
						else
						{
							Array.Sort(m_points, new ChartStyledPointComparerXDescending());
						}
						if (m_series.ActualXAxis.ValueType == ChartValueType.DateTime && m_series.ActualXAxis.RangeType == ChartAxisRangeType.Auto && (m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Months || m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Years) && m_series.SortOrder == ChartSeriesSortingOrder.Ascending)
						{
							for (int j = 0; j < m_points.Length; j++)
							{
								double num = m_series.ActualXAxis.Range.Min;
								double num2 = ((m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Months) ? GetDateTimeInterval(num, isMonth: true) : GetDateTimeInterval(num, isMonth: false));
								while (num <= m_series.ActualXAxis.Range.Max)
								{
									double num3 = num + num2;
									if (num < m_points[j].X && num3 > m_points[j].X)
									{
										m_points[j].X = num + 0.5;
										break;
									}
									num = num3;
									num2 = ((m_series.ActualXAxis.IntervalType == ChartDateTimeIntervalType.Months) ? GetDateTimeInterval(num, isMonth: true) : GetDateTimeInterval(num, isMonth: false));
								}
							}
						}
					}
					else
					{
						IComparer comparer2;
						if (m_series.SortOrder != 0)
						{
							IComparer comparer = new CategoryValueComparerDescending();
							comparer2 = comparer;
						}
						else
						{
							IComparer comparer = new CategoryValueComparer();
							comparer2 = comparer;
						}
						IComparer comparer3 = comparer2;
						Array.Sort(m_points, comparer3);
						for (int k = 0; k < m_points.Length && m_points[k] != null; k++)
						{
							m_points[k].X = ((arrayList.Count > 0) ? arrayList.IndexOf(m_points[k].Category) : k);
							m_points[k].Point = new ChartPoint(m_points[k].X, m_points[k].YValues);
						}
					}
				}
				else if (m_series.Type == ChartSeriesType.StackingColumn || m_series.Type == ChartSeriesType.StackingArea || m_series.Type == ChartSeriesType.StackingBar || m_series.Type == ChartSeriesType.StackingColumn100)
				{
					StackSorting();
				}
				else
				{
					ChartStyledPoint[] points = m_points;
					IComparer comparer4;
					if (m_series.SortOrder != 0)
					{
						IComparer comparer = new ChartStyledPointComparerYDescending();
						comparer4 = comparer;
					}
					else
					{
						IComparer comparer = new ChartStyledPointComparerY();
						comparer4 = comparer;
					}
					Array.Sort(points, comparer4);
					if (m_series.ActualXAxis.ValueType == ChartValueType.Category)
					{
						for (int l = 0; l < m_points.Length; l++)
						{
							m_points[l].X = ((arrayList.Count > 0) ? arrayList.IndexOf(m_points[l].Category) : l);
							m_points[l].Point = new ChartPoint(m_points[l].X, m_points[l].YValues);
						}
					}
				}
			}
		}
		if (m_series.EnableStyles && m_shouldUpdate && !flag)
		{
			ChartStyledPoint[] points2 = m_points;
			foreach (ChartStyledPoint chartStyledPoint2 in points2)
			{
				if (chartStyledPoint2 != null)
				{
					chartStyledPoint2.Style = GetStyleAt(chartStyledPoint2.Index);
				}
			}
		}
		return m_points;
	}

	internal double GetDateTimeInterval(double value, bool isMonth)
	{
		DateTime dateTime = DateTime.FromOADate(value);
		int year = dateTime.Year;
		if (isMonth)
		{
			switch (dateTime.Month)
			{
			case 1:
				return 31.0;
			case 2:
				if ((year % 4 == 0 && year % 100 != 0) || year % 400 == 0)
				{
					return 29.0;
				}
				return 28.0;
			case 3:
				return 31.0;
			case 4:
				return 30.0;
			case 5:
				return 31.0;
			case 6:
				return 30.0;
			case 7:
				return 31.0;
			case 8:
				return 31.0;
			case 9:
				return 30.0;
			case 10:
				return 31.0;
			case 11:
				return 30.0;
			case 12:
				return 31.0;
			default:
				return isMonth ? 31 : 365;
			}
		}
		if ((year % 4 == 0 && year % 100 != 0) || year % 400 == 0)
		{
			return 366.0;
		}
		return 365.0;
	}

	protected ChartStyledPoint CreateStyledPoint(int index)
	{
		ChartPoint chartPoint = m_series.Points[index];
		ChartStyledPoint chartStyledPoint = new ChartStyledPoint(chartPoint, index);
		if (m_series.ActualXAxis.ValueType == ChartValueType.Category)
		{
			chartStyledPoint.Category = chartPoint.Category;
		}
		chartStyledPoint.X = GetIndexValueFromX(chartPoint.X);
		chartStyledPoint.YValues = ((chartPoint.YValues.Length == 0) ? new double[4] : chartPoint.YValues);
		chartStyledPoint.Style = GetStyleAt(index);
		chartStyledPoint.IsVisible = IsVisiblePoint(chartPoint);
		if (m_series.BeginArrows != null && m_series.BeginArrows.Count - 1 >= index)
		{
			chartStyledPoint.BeginArrow = m_series.BeginArrows[index];
		}
		if (m_series.EndArrows != null && m_series.EndArrows.Count - 1 >= index)
		{
			chartStyledPoint.EndArrow = m_series.EndArrows[index];
		}
		return chartStyledPoint;
	}

	protected IndexRange CalculateVisibleRange()
	{
		ChartStyledPoint[] styledPoints = StyledPoints;
		int num = 0;
		int num2 = styledPoints.Length - 1;
		int num3 = 0;
		if (m_isSorted)
		{
			MinMaxInfo visibleRange = m_series.ActualXAxis.VisibleRange;
			double num4 = visibleRange.Min;
			double num5 = visibleRange.Max;
			if (m_series.ActualXAxis.ValueType == ChartValueType.Logarithmic)
			{
				num4 = Math.Pow(m_series.ActualXAxis.LogBase, num4);
				num5 = Math.Pow(m_series.ActualXAxis.LogBase, num5);
			}
			int num6 = num;
			int num7 = num2;
			int num8 = num7 + num6 >> 1;
			while (true)
			{
				num3++;
				if (num7 - num6 <= 1)
				{
					num = num6;
					break;
				}
				if (styledPoints[num6].X >= num4)
				{
					num = num6;
					break;
				}
				if (styledPoints[num7].X == num4)
				{
					num = num7;
					break;
				}
				if (styledPoints[num8].X == num4)
				{
					num = num8;
					break;
				}
				if (styledPoints[num8].X < num4)
				{
					num6 = num8 + 1;
				}
				else
				{
					num7 = num8 - 1;
				}
				num8 = num7 + num6 >> 1;
			}
			int num9 = num;
			int num10 = num2;
			int num11 = num10 + num9 >> 1;
			while (true)
			{
				num3++;
				if (num10 - num9 <= 1)
				{
					num2 = num10;
					break;
				}
				if (styledPoints[num9].X == num5)
				{
					num2 = num9;
					break;
				}
				if (styledPoints[num10].X <= num5)
				{
					num2 = num10;
					break;
				}
				if (styledPoints[num11].X == num5)
				{
					num2 = num11;
					break;
				}
				if (styledPoints[num11].X < num5)
				{
					num9 = num11 + 1;
				}
				else
				{
					num10 = num11 - 1;
				}
				num11 = num10 + num9 >> 1;
			}
		}
		return new IndexRange(Math.Max(0, num - 1), Math.Min(styledPoints.Length - 1, num2 + 1));
	}

	protected IndexRange[] CalculateUnEmptyRanges(IndexRange vrange)
	{
		if (!Chart.AllowGapForEmptyPoints)
		{
			return new IndexRange[1] { vrange };
		}
		ArrayList arrayList = new ArrayList();
		int num = 0;
		int i = vrange.From;
		for (int num2 = vrange.To + 1; i < num2; i++)
		{
			if (num == -1)
			{
				num = i;
			}
			if (!IsVisiblePoint(m_series.Points[i]))
			{
				arrayList.Add(new IndexRange(num, i - 1));
				vrange.From = i + 1;
				num = -1;
			}
		}
		arrayList.Add(vrange);
		return (IndexRange[])arrayList.ToArray(typeof(IndexRange));
	}

	internal static void PhongShadingColors(Color ambientColor, Color diffusiveColor, Color lightColor, double alpha, double phong_alpha, out Color[] colors, out float[] positions)
	{
		double num = 0.5;
		int num2 = 10;
		Color color = ambientColor;
		Color color2 = diffusiveColor;
		Color color3 = lightColor;
		double num3 = 0.44999998807907104;
		double num4 = 0.550000011920929;
		double num5 = 0.8999999761581421;
		double num6 = 1.0 / (double)(num2 - 1);
		positions = new float[num2];
		colors = new Color[num2];
		for (int i = 0; i < num2; i++)
		{
			double num7 = (double)i * num6;
			double num8 = Math.Asin((num7 - num) / num);
			Math.Sin(num8);
			double num9 = 0.0;
			double num10 = 0.0;
			double num11 = 0.0;
			num9 = num3 * (double)(int)color.R;
			num10 = num3 * (double)(int)color.G;
			num11 = num3 * (double)(int)color.B;
			double num12 = Math.Max(Math.Cos(alpha + num8), 0.0);
			num9 += num4 * (double)(int)color2.R * num12;
			num10 += num4 * (double)(int)color2.G * num12;
			num11 += num4 * (double)(int)color2.B * num12;
			num12 = Math.Pow(Math.Abs(Math.Cos(alpha + 2.0 * num8)), phong_alpha);
			if (Math.Abs(alpha + 2.0 * num8) > Math.PI / 2.0)
			{
				num12 = 0.0;
			}
			num9 += num5 * (double)(int)color3.R * num12;
			num10 += num5 * (double)(int)color3.G * num12;
			num11 += num5 * (double)(int)color3.B * num12;
			num9 = Math.Min(255.0, num9);
			num10 = Math.Min(255.0, num10);
			num11 = Math.Min(255.0, num11);
			positions[i] = (float)num7;
			colors[i] = Color.FromArgb(ambientColor.A, (int)num9, (int)num10, (int)num11);
		}
		positions[num2 - 1] = 1f;
	}

	internal virtual float GetTotalDepth()
	{
		return m_chart.GetChartArea().Depth;
	}

	private IList<ChartStyledPoint> GetCache()
	{
		if (m_pointsCache == null)
		{
			ResetCache();
		}
		return m_pointsCache;
	}

	private void InsertPoint(int index)
	{
		if (m_pointsCache == null)
		{
			GetCache();
		}
		else
		{
			m_pointsCache.Insert(index, null);
		}
	}

	private void RemovePoint(int index)
	{
		if (m_pointsCache == null)
		{
			GetCache();
			return;
		}
		m_pointsCache.RemoveAt(index);
		foreach (ChartStyledPoint item in m_pointsCache)
		{
			if (item != null && item.Index > index)
			{
				item.Index--;
				item.Point = m_series.Points[item.Index];
			}
		}
	}

	private void UpdatePoint(int index)
	{
		if (m_pointsCache != null)
		{
			m_pointsCache[index] = null;
		}
	}

	private void ResetCache()
	{
		if (m_pointsCache == null)
		{
			m_pointsCache = new List<ChartStyledPoint>(m_series.Points.Count);
		}
		m_pointsCache.Clear();
		for (int i = 0; i < m_series.Points.Count; i++)
		{
			m_pointsCache.Add(null);
		}
	}

	private ChartStyledPoint GetStyledPoint(int pointIndex)
	{
		ChartStyledPoint[] array = PrepearePoints();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Index == pointIndex)
			{
				return array[i];
			}
		}
		return null;
	}

	public List<int> CalCachePoints()
	{
		if (m_chart != null)
		{
			m_pointsCache.Clear();
			m_pointIndex.Clear();
			PointF pointF = PointF.Empty;
			PointF empty = PointF.Empty;
			ChartPoint chartPoint = null;
			bool dropSeriesPoints = m_chart.DropSeriesPoints;
			ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(m_chart, m_series);
			int num = chartRenderArgs2D.Series.PointFormats[ChartYValueUsage.YValue];
			for (int i = 0; i < m_series.Points.Count; i++)
			{
				chartPoint = m_series.Points[i];
				empty = chartRenderArgs2D.GetPoint(chartPoint.X, chartPoint.YValues[num]);
				if ((!dropSeriesPoints && pointF.IsEmpty) || (double)Math.Abs(pointF.X - empty.X) > m_series.Resolution || (double)Math.Abs(empty.Y - pointF.Y) > m_series.Resolution)
				{
					pointF = empty;
					m_pointsCache.Add(null);
					m_pointIndex.Add(i);
				}
			}
		}
		return m_pointIndex;
	}
}
