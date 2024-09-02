using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.OfficeChart;

namespace DocGen.Chart;

internal class ChartControl : Control, IChartAreaHost, IDisposable
{
	private double m_minPointsDelta = double.NaN;

	private ChartModel m_model;

	private Graphics m_g;

	private Rectangle m_borderBounds = Rectangle.Empty;

	private ChartDockingManager m_titleDockingManager = new ChartDockingManager();

	private ChartTitle m_defaultTitle = new ChartTitle();

	private ChartLegend m_defaultLegend;

	private int columnFixedWidth = 20;

	private Color m_borderColor = SystemColors.ControlText;

	private LineInfo m_border = new LineInfo();

	private BorderStyle m_borderStyle;

	private bool m_isRadar;

	public DocGen.Drawing.Image BackgroundImage { get; set; }

	public ChartArea ChartArea { get; }

	public ChartDockingManager DockingManager { get; }

	public int Resolution { get; set; }

	public Rectangle Bounds => new Rectangle(Point.Empty, base.Size);

	public bool RequireAxes
	{
		get
		{
			return ChartArea.RequireAxes;
		}
		set
		{
			ChartArea.RequireAxes = value;
		}
	}

	public bool RequireInvertedAxes
	{
		get
		{
			return ChartArea.RequireInvertedAxes;
		}
		set
		{
			ChartArea.RequireInvertedAxes = value;
		}
	}

	public Color ForeColor { get; set; }

	public BrushInfo BackInterior
	{
		get
		{
			return ChartArea.BackInterior;
		}
		set
		{
			ChartArea.BackInterior = value;
		}
	}

	public ChartMargins ChartAreaMargins
	{
		get
		{
			return ChartArea.ChartAreaMargins;
		}
		set
		{
			ChartArea.ChartAreaMargins = value;
		}
	}

	public ChartColumnDrawMode ColumnDrawMode { get; set; }

	public Font Font { get; set; }

	public ChartColumnWidthMode ColumnWidthMode { get; set; }

	public int ColumnFixedWidth
	{
		get
		{
			return columnFixedWidth;
		}
		set
		{
			if (columnFixedWidth != value)
			{
				columnFixedWidth = value;
			}
		}
	}

	public ChartCustomPointCollection CustomPoints => ChartArea.CustomPoints;

	public bool Radar
	{
		get
		{
			if (Series.Count > 0)
			{
				if (Series[0].Type != ChartSeriesType.Radar)
				{
					return Series[0].Type == ChartSeriesType.Polar;
				}
				return true;
			}
			return false;
		}
	}

	public bool Bar
	{
		get
		{
			if (Series.Count > 0)
			{
				if (Series[0].Type != ChartSeriesType.Bar && Series[0].Type != ChartSeriesType.StackingBar)
				{
					return Series[0].Type == ChartSeriesType.StackingBar100;
				}
				return true;
			}
			return false;
		}
	}

	internal bool IsScatter
	{
		get
		{
			if (Series.Count > 0)
			{
				return Series[0].Type.ToString().Contains("Scatter");
			}
			return false;
		}
	}

	public bool Polar
	{
		get
		{
			if (Series.Count > 0)
			{
				return Series[0].Type == ChartSeriesType.Polar;
			}
			return false;
		}
	}

	public ChartRadarAxisStyle RadarStyle { get; set; }

	public int RoundingPlaces { get; set; }

	public Size SizeMargins
	{
		get
		{
			Size result = new Size(base.Size.Width, base.Size.Height);
			result.Height -= ChartArea.RenderBounds.Height;
			result.Width -= ChartArea.RenderBounds.Width;
			return result;
		}
	}

	public ChartTextPosition TextPosition
	{
		get
		{
			if (m_defaultTitle != null)
			{
				switch (m_defaultTitle.Position)
				{
				case ChartDock.Left:
					return ChartTextPosition.Left;
				case ChartDock.Right:
					return ChartTextPosition.Right;
				case ChartDock.Top:
					return ChartTextPosition.Top;
				case ChartDock.Bottom:
					return ChartTextPosition.Bottom;
				}
			}
			return ChartTextPosition.Top;
		}
		set
		{
			if (m_defaultTitle != null)
			{
				switch (value)
				{
				case ChartTextPosition.Top:
					m_defaultTitle.Position = ChartDock.Top;
					break;
				case ChartTextPosition.Bottom:
					m_defaultTitle.Position = ChartDock.Bottom;
					break;
				case ChartTextPosition.Left:
					m_defaultTitle.Position = ChartDock.Left;
					break;
				case ChartTextPosition.Right:
					m_defaultTitle.Position = ChartDock.Right;
					break;
				}
			}
		}
	}

	public bool RealMode3D
	{
		get
		{
			if (ChartArea != null)
			{
				return ChartArea.RealSeries3D;
			}
			return false;
		}
		set
		{
			if (ChartArea != null)
			{
				ChartArea.RealSeries3D = value;
			}
		}
	}

	public bool InvertedSeriesIsCompatible { get; set; }

	public ChartTitlesList Titles { get; }

	public ChartTitle Title
	{
		get
		{
			return m_defaultTitle;
		}
		set
		{
			m_defaultTitle = value;
		}
	}

	public List<Control> Controls { get; }

	public ChartLegendsList Legends { get; }

	public ChartLegend Legend => m_defaultLegend;

	public ChartDock LegendPosition
	{
		get
		{
			return m_defaultLegend.Position;
		}
		set
		{
			m_defaultLegend.Position = value;
		}
	}

	public bool ShowLegend
	{
		get
		{
			if (m_defaultLegend != null)
			{
				return m_defaultLegend.Visible;
			}
			return false;
		}
		set
		{
			if (m_defaultLegend != null)
			{
				m_defaultLegend.Visible = value;
			}
		}
	}

	public ChartAlignment LegendAlignment
	{
		get
		{
			if (m_defaultLegend != null)
			{
				return m_defaultLegend.Alignment;
			}
			return ChartAlignment.Near;
		}
		set
		{
			if (m_defaultLegend != null)
			{
				m_defaultLegend.Alignment = value;
			}
		}
	}

	public ChartPlacement LegendsPlacement
	{
		get
		{
			return DockingManager.Placement;
		}
		set
		{
			DockingManager.Placement = value;
		}
	}

	public ChartModel Model
	{
		get
		{
			if (m_model == null)
			{
				SetModel(new ChartModel());
			}
			return m_model;
		}
		set
		{
			if (m_model != value)
			{
				SetModel(m_model);
			}
		}
	}

	public bool CompatibleSeries { get; set; }

	public ChartSeriesCollection Series => Model.Series;

	public ChartAxisCollection Axes => ChartArea.Axes;

	public bool Indexed
	{
		get
		{
			return ChartArea.IsIndexed;
		}
		set
		{
			ChartArea.IsIndexed = value;
		}
	}

	public bool AllowGapForEmptyPoints
	{
		get
		{
			return ChartArea.IsAllowGap;
		}
		set
		{
			ChartArea.IsAllowGap = value;
		}
	}

	public ChartAxis PrimaryXAxis => ChartArea.PrimaryXAxis;

	public ChartAxis PrimaryYAxis => ChartArea.PrimaryYAxis;

	public DocGen.Drawing.Image ChartAreaBackImage
	{
		get
		{
			return ChartArea.BackImage;
		}
		set
		{
			ChartArea.BackImage = value;
		}
	}

	public DocGen.Drawing.Image ChartInteriorBackImage
	{
		get
		{
			return ChartArea.InteriorBackImage;
		}
		set
		{
			ChartArea.InteriorBackImage = value;
		}
	}

	public bool ChartAreaShadow { get; set; }

	public BrushInfo ChartInterior
	{
		get
		{
			return ChartArea.GridBackInterior;
		}
		set
		{
			ChartArea.GridBackInterior = value;
		}
	}

	public float Depth
	{
		get
		{
			return ChartArea.Depth;
		}
		set
		{
			ChartArea.Depth = value;
		}
	}

	public int ElementsSpacing { get; set; }

	public ChartColorPalette Palette
	{
		get
		{
			return Model.ColorModel.Palette;
		}
		set
		{
			Model.ColorModel.Palette = value;
		}
	}

	public Color[] CustomPalette
	{
		get
		{
			return Model.ColorModel.CustomColors;
		}
		set
		{
			Model.ColorModel.CustomColors = value;
		}
	}

	public bool AllowGradientPalette
	{
		get
		{
			return Model.ColorModel.AllowGradient;
		}
		set
		{
			Model.ColorModel.AllowGradient = value;
		}
	}

	public float Rotation
	{
		get
		{
			return ChartArea.Rotation;
		}
		set
		{
			ChartArea.Rotation = value;
		}
	}

	public bool Series3D
	{
		get
		{
			if (ChartArea != null)
			{
				return ChartArea.Series3D;
			}
			return false;
		}
		set
		{
			ChartArea.Series3D = value;
		}
	}

	public bool Style3D { get; set; }

	public BrushInfo ShadowColor { get; set; }

	public int ShadowWidth { get; set; }

	public SmoothingMode SmoothingMode { get; set; }

	public float Spacing
	{
		get
		{
			return (float)Math.Round(100f * ChartArea.SeriesParameters.PointSpacing, 4);
		}
		set
		{
			ChartArea.SeriesParameters.PointSpacing = value / 100f;
		}
	}

	public float SpacingBetweenSeries
	{
		get
		{
			return (float)Math.Round(100f * ChartArea.SeriesParameters.SeriesDepthSpacing, 4);
		}
		set
		{
			ChartArea.SeriesParameters.SeriesDepthSpacing = value / 100f;
		}
	}

	public float SpacingBetweenPoints
	{
		get
		{
			return (float)Math.Round(100f * ChartArea.SeriesParameters.SeriesSpacing, 4);
		}
		set
		{
			ChartArea.SeriesParameters.SeriesSpacing = value / 100f;
		}
	}

	public StringAlignment TextAlignment
	{
		get
		{
			if (m_defaultTitle != null)
			{
				switch (m_defaultTitle.Alignment)
				{
				case ChartAlignment.Near:
					return StringAlignment.Near;
				case ChartAlignment.Center:
					return StringAlignment.Center;
				case ChartAlignment.Far:
					return StringAlignment.Far;
				}
			}
			return StringAlignment.Center;
		}
		set
		{
			if (m_defaultTitle != null)
			{
				switch (value)
				{
				case StringAlignment.Near:
					m_defaultTitle.Alignment = ChartAlignment.Near;
					break;
				case StringAlignment.Center:
					m_defaultTitle.Alignment = ChartAlignment.Center;
					break;
				case StringAlignment.Far:
					m_defaultTitle.Alignment = ChartAlignment.Far;
					break;
				}
			}
		}
	}

	public float Tilt
	{
		get
		{
			return ChartArea.Tilt;
		}
		set
		{
			ChartArea.Tilt = value;
		}
	}

	public bool DropSeriesPoints { get; set; }

	public string Text
	{
		get
		{
			if (m_defaultTitle != null)
			{
				return m_defaultTitle.Text;
			}
			return string.Empty;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				if (Titles.Contains(m_defaultTitle))
				{
					Titles.Remove(m_defaultTitle);
				}
			}
			else if (!Titles.Contains(m_defaultTitle))
			{
				Titles.Add(m_defaultTitle);
			}
			m_defaultTitle.Text = value;
		}
	}

	public TextRenderingHint TextRenderingHint { get; set; }

	public ChartIndexedValues IndexValues => Model.IndexedValues;

	private double MinPointsDelta
	{
		get
		{
			if (double.IsNaN(m_minPointsDelta))
			{
				m_minPointsDelta = double.MaxValue;
				if (Indexed)
				{
					m_minPointsDelta = 1.0;
				}
				else
				{
					foreach (ChartSeries item in Series)
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
							double num = array[j] - array[j - 1];
							if (num != 0.0)
							{
								m_minPointsDelta = Math.Min(m_minPointsDelta, num);
							}
						}
					}
				}
				if (m_minPointsDelta == double.MaxValue)
				{
					m_minPointsDelta = 1.0;
				}
			}
			return m_minPointsDelta;
		}
	}

	internal LineInfo Border
	{
		get
		{
			return m_border;
		}
		set
		{
			if (m_border != value)
			{
				m_border = value;
			}
		}
	}

	internal bool IsRadar
	{
		get
		{
			return m_isRadar;
		}
		set
		{
			m_isRadar = value;
		}
	}

	IChartArea IChartAreaHost.GetChartArea()
	{
		return ChartArea;
	}

	private void SetModel(ChartModel model)
	{
		UnwireSeriesCollection();
		if (m_model != null)
		{
			m_model.SetChart(null);
		}
		m_model = model;
		if (m_model != null)
		{
			m_model.SetChart(this);
		}
		WireSeriesCollection();
	}

	private void WireSeriesCollection()
	{
		if (m_model != null)
		{
			m_model.Series.Changed += SeriesChanged;
		}
	}

	private void UnwireSeriesCollection()
	{
		if (m_model != null)
		{
			m_model.Series.Changed -= SeriesChanged;
		}
	}

	private void Render(Graphics g, Size sz)
	{
		base.Size = sz;
		PrepareAxesSeriesAndChart();
		RecalculateSizes(g);
		g.Clear(Color.Transparent);
		_Paint(g, base.ClientRectangle);
		foreach (Control control in Controls)
		{
			control.Render(g);
		}
	}

	private void _Paint(Graphics g, Rectangle r)
	{
		g.TextRenderingHint = TextRenderingHint;
		g.SmoothingMode = SmoothingMode;
		BrushInfo backInterior = BackInterior;
		BrushPaint.FillRectangle(g, new Rectangle(base.Location.X, base.Location.Y, base.Size.Width + 1, base.Size.Height + 1), backInterior);
		if (ChartAreaShadow && !Series3D)
		{
			Rectangle r2 = new Rectangle(ChartArea.Location.X + ShadowWidth, ChartArea.Location.Y + ShadowWidth, ChartArea.Width, ChartArea.Height);
			BrushPaint.FillRectangle(g, r2, ShadowColor);
		}
		PaintEventArgs e = new PaintEventArgs(g, r);
		ChartArea.Draw(e);
		float num = Border.Pen.Width / 2f;
		if (num > 0f)
		{
			g.DrawRectangle(Border.Pen, num, num, (float)base.Width - Border.Pen.Width, (float)base.Height - Border.Pen.Width);
		}
	}

	internal void PrepareAxesSeriesAndChart()
	{
		CheckSeriesCompatibility();
		Model.UpdateArea(ChartArea);
	}

	internal Graphics GetGraphics()
	{
		return m_g;
	}

	internal virtual void RecalculateSizes(Graphics g)
	{
		Rectangle rect = new Rectangle(Point.Empty, base.Size);
		m_titleDockingManager.Spacing = ElementsSpacing;
		rect = (m_borderBounds = m_titleDockingManager.DoLayout(rect, g));
		rect.Inflate(ElementsSpacing, ElementsSpacing);
		DockingManager.Spacing = ElementsSpacing;
		if (DockingManager.Placement == ChartPlacement.Outside)
		{
			rect = DockingManager.DoLayout(rect, g);
		}
		else
		{
			rect.Inflate(-ElementsSpacing, -ElementsSpacing);
		}
		ChartArea.CalculateSizes(rect, g);
		if (DockingManager.Placement == ChartPlacement.Inside)
		{
			DockingManager.DoLayout(ChartArea.RenderBounds, g);
		}
	}

	private void OnTitlesChanged(ChartBaseList list, ChartListChangeArgs args)
	{
		if (args.OldItems != null)
		{
			object[] oldItems = args.OldItems;
			for (int i = 0; i < oldItems.Length; i++)
			{
				ChartTitle chartTitle = (ChartTitle)oldItems[i];
				m_titleDockingManager.Remove(chartTitle);
				Controls.Remove(chartTitle);
			}
		}
		if (args.NewItems != null)
		{
			object[] oldItems = args.NewItems;
			for (int i = 0; i < oldItems.Length; i++)
			{
				ChartTitle chartTitle2 = (ChartTitle)oldItems[i];
				Controls.Add(chartTitle2);
				m_titleDockingManager.Add(chartTitle2);
			}
		}
	}

	private void OnLegendsChanged(ChartBaseList list, ChartListChangeArgs args)
	{
		if (args.NewItems != null)
		{
			object[] newItems = args.NewItems;
			for (int i = 0; i < newItems.Length; i++)
			{
				ChartLegend chartLegend = (ChartLegend)newItems[i];
				Controls.Add(chartLegend);
				DockingManager.Add(chartLegend);
			}
		}
		if (args.OldItems != null)
		{
			object[] newItems = args.OldItems;
			for (int i = 0; i < newItems.Length; i++)
			{
				ChartLegend chartLegend2 = (ChartLegend)newItems[i];
				DockingManager.Remove(chartLegend2);
				Controls.Remove(chartLegend2);
			}
		}
	}

	private void SetAutoformat()
	{
		Palette = ChartColorPalette.Default;
		Font = new Font("Microsoft Sans Serif", 8.25f);
	}

	public void Draw(DocGen.Drawing.Image img)
	{
		Graphics graphics = Graphics.FromImage((Bitmap)img);
		Size size = img.Size;
		Draw(graphics, size);
		graphics.Dispose();
	}

	public void Draw(Graphics g, Size sz)
	{
		Render(g, sz);
	}

	public void Draw(DocGen.Drawing.Image img, Size sz)
	{
		Graphics graphics = Graphics.FromImage((Bitmap)img);
		Draw(graphics, sz);
		graphics.Dispose();
	}

	public void SaveImage(Stream stream)
	{
		SaveImage(stream, new ChartRenderingOptions
		{
			ScalingMode = ScalingMode.Best
		});
	}

	internal void SaveImage(Stream stream, ChartRenderingOptions imageOptions)
	{
		int num = ((imageOptions.ScalingMode != ScalingMode.Best) ? 1 : 3);
		Bitmap bitmap = new Bitmap(base.Width * num + 1, base.Height * num + 1);
		bitmap.SetResolution(Resolution, Resolution);
		Graphics graphics = Graphics.FromImage(bitmap);
		Matrix matrix = new Matrix();
		matrix.Scale(num, num);
		graphics.Transform = matrix;
		Render(graphics, base.Size);
		graphics.Dispose();
		bitmap.Save(stream, (imageOptions.ImageFormat == ExportImageFormat.Png || ChartArea.BackInterior == null || ChartArea.BackInterior.BackColor.A != byte.MaxValue) ? ImageFormat.Png : ImageFormat.Jpeg);
		bitmap.Dispose();
	}

	private Graphics CreateGraphics(int width, int height, ChartRenderingOptions imageOptions)
	{
		int num = 1;
		if (imageOptions.ScalingMode == ScalingMode.Best)
		{
			num = 3;
		}
		Bitmap bitmap = new Bitmap(width * num + 1, height * num + 1);
		bitmap.SetResolution(Resolution, Resolution);
		Graphics graphics = Graphics.FromImage(bitmap);
		if (imageOptions.ScalingMode == ScalingMode.Best)
		{
			graphics.SkSurface.Canvas.Scale(3f);
		}
		return graphics;
	}

	public void SeriesChanged(object sender, ChartSeriesCollectionChangedEventArgs e)
	{
		m_minPointsDelta = double.NaN;
		CheckAndPrepareSeries();
	}

	private void CheckSeriesCompatibility()
	{
		m_model.CheckSeriesCompatibility(ChartArea, InvertedSeriesIsCompatible);
	}

	private void CheckAndPrepareSeries()
	{
		int i = 0;
		for (int count = Series.Count; i < count; i++)
		{
			ChartSeries chartSeries = Series[i];
			chartSeries.Renderer.SetChart(this);
			chartSeries.ChartModel = Model;
			if (chartSeries.XAxis == null)
			{
				if (i < count - 1)
				{
					chartSeries.AddAxis(PrimaryXAxis, horizontal: true);
				}
				else
				{
					chartSeries.XAxis = PrimaryXAxis;
				}
			}
			if (chartSeries.YAxis == null)
			{
				if (i < count - 1)
				{
					chartSeries.AddAxis(PrimaryYAxis, horizontal: false);
				}
				else
				{
					chartSeries.YAxis = PrimaryYAxis;
				}
			}
		}
	}

	public void Dispose()
	{
		Dispose(dispose: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool dispose)
	{
		if (dispose)
		{
			Font.Dispose();
		}
	}

	public ChartControl()
	{
		ForeColor = SystemColors.ControlText;
		ChartArea = new ChartArea(this);
		SetModel(new ChartModel());
		Controls = new List<Control>();
		BackInterior = new BrushInfo(Color.White);
		Resolution = 96;
		DockingManager = new ChartDockingManager(this);
		DockingManager.DockAlignment = true;
		DockingManager.Placement = ChartPlacement.Outside;
		m_titleDockingManager = new ChartDockingManager(this);
		m_defaultTitle = new ChartTitle();
		m_defaultTitle.Name = "Chart Control";
		m_defaultTitle.Text = "";
		m_titleDockingManager.Add(m_defaultTitle);
		Titles = new ChartTitlesList();
		Titles.Changed += OnTitlesChanged;
		Titles.Add(m_defaultTitle);
		m_defaultLegend = new ChartLegend(this);
		m_defaultLegend.Name = "";
		Legends = new ChartLegendsList();
		Legends.Changed += OnLegendsChanged;
		Legends.Add(m_defaultLegend);
		base.Size = new Size(400, 300);
	}
}
