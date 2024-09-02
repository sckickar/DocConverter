using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class PieRenderer : ChartSeriesRenderer
{
	internal enum PieSectorCorner
	{
		TopLeft,
		TopRight,
		BottomLeft,
		BottomRight
	}

	private class PieSector
	{
		private float m_depth;

		private RectangleF m_inSideRect;

		private RectangleF m_outSideRect;

		private RectangleF m_inUpSideRect;

		private RectangleF m_outUpSideRect;

		private float m_startAngle;

		private float m_endAngle;

		private ChartStyledPoint m_point;

		private GraphicsPath m_geometry;

		private static readonly DoubleRange c_1PISector = new DoubleRange(0.0, 180.0);

		private static readonly DoubleRange c_2PISector = new DoubleRange(180.0, 360.0);

		public ChartStyledPoint StyledPoint => m_point;

		public float StartAngle => m_startAngle;

		public float EndAngle => m_endAngle;

		public RectangleF InnerBounds => m_inSideRect;

		public RectangleF OuterBounds => m_outSideRect;

		public PieSector(ChartStyledPoint point, RectangleF outSideRect, RectangleF inSideRect, float startAngle, float endAngle, float depth)
		{
			m_point = point;
			m_depth = depth;
			m_inSideRect = inSideRect;
			m_outSideRect = outSideRect;
			m_inUpSideRect = inSideRect;
			m_outUpSideRect = outSideRect;
			m_startAngle = startAngle;
			m_endAngle = endAngle;
			m_inSideRect.Offset(0f, depth);
			m_outSideRect.Offset(0f, depth);
		}

		public IEnumerable<Pie3DSegment> Render()
		{
			IList<Pie3DSegment> list = null;
			if ((int)m_outSideRect.Width > 0 && (int)m_outSideRect.Height > 0)
			{
				if (m_depth != 0f)
				{
					list = new List<Pie3DSegment>();
					float num = m_startAngle % 360f;
					float num2 = m_endAngle % 360f;
					float num3 = m_endAngle - m_startAngle;
					if (num3 == 360f)
					{
						list.Add(CreateSegment(0f, 180f, left: false, right: false));
						list.Add(CreateSegment(180f, 360f, left: false, right: false));
					}
					else if (num3 <= 180f && ((num < 180f && num2 <= 180f) || (num >= 180f && num2 >= 180f)))
					{
						list.Add(CreateSegment(num, num2, left: true, right: true));
					}
					else if (num > num2)
					{
						if (num < 180f)
						{
							list.Add(CreateSegment(num, 180f, left: true, right: false));
							if (num2 < 180f)
							{
								list.Add(CreateSegment(180f, 360f, left: true, right: false));
							}
						}
						else
						{
							list.Add(CreateSegment(num, 360f, left: true, right: false));
						}
						if (num2 < 180f)
						{
							list.Add(CreateSegment(0f, num2, left: false, right: true));
						}
						else
						{
							list.Add(CreateSegment(0f, 180f, left: false, right: false));
							list.Add(CreateSegment(180f, num2, left: false, right: true));
						}
					}
					else if (num < 180f && num2 > 180f)
					{
						list.Add(CreateSegment(num, 180f, left: true, right: false));
						list.Add(CreateSegment(180f, num2, left: false, right: true));
					}
				}
				m_geometry = new GraphicsPath();
				if (m_endAngle - m_startAngle == 360f)
				{
					m_endAngle -= 0.001f;
				}
				if (m_inSideRect.IsEmpty)
				{
					AddClosedSector(m_geometry, Rectangle.Round(m_outUpSideRect), m_startAngle, m_endAngle - m_startAngle);
				}
				else
				{
					AddSector(m_geometry, m_outUpSideRect, m_startAngle, m_endAngle - m_startAngle);
					AddSector(m_geometry, m_inUpSideRect, m_endAngle, m_startAngle - m_endAngle);
					m_geometry.CloseFigure();
				}
			}
			return list;
		}

		public void Draw(ChartGraph graph, BrushInfo brInfo, Pen pen, ChartPieFillMode type, ColorBlend gradient)
		{
			if (m_geometry == null)
			{
				return;
			}
			graph.DrawPath(brInfo, null, m_geometry);
			if (gradient != null && m_geometry.GetBounds().Height != 0f)
			{
				using PathGradientBrush pathGradientBrush = new PathGradientBrush(m_geometry);
				pathGradientBrush.InterpolationColors = gradient;
				if (type == ChartPieFillMode.AllPie)
				{
					pathGradientBrush.CenterPoint = new PointF(m_outUpSideRect.X + m_outUpSideRect.Width / 2f, m_outUpSideRect.Y + m_outUpSideRect.Height / 2f);
				}
				pathGradientBrush.CenterColor = Color.Transparent;
				pathGradientBrush.SurroundColors = new Color[1] { Color.Transparent };
				graph.DrawPath(pathGradientBrush, null, m_geometry);
			}
			graph.DrawPath(pen, m_geometry);
		}

		private Pie3DSegment CreateSegment(float startAngle, float endAngle, bool left, bool right)
		{
			Pie3DSegment pie3DSegment = new Pie3DSegment();
			pie3DSegment.StartAngle = startAngle;
			pie3DSegment.EndAngle = endAngle;
			pie3DSegment.Sector = this;
			if (left)
			{
				PointF pointByAngle = ChartMath.GetPointByAngle(m_outSideRect, (double)startAngle * (Math.PI / 180.0), isCircle: true);
				PointF pointByAngle2 = ChartMath.GetPointByAngle(m_inSideRect, (double)startAngle * (Math.PI / 180.0), isCircle: true);
				pie3DSegment.LeftSide = new GraphicsPath();
				pie3DSegment.LeftSide.AddPolygon(new PointF[4]
				{
					pointByAngle2,
					new PointF(pointByAngle2.X, pointByAngle2.Y - m_depth),
					new PointF(pointByAngle.X, pointByAngle.Y - m_depth),
					pointByAngle
				});
			}
			if (right)
			{
				PointF pointByAngle3 = ChartMath.GetPointByAngle(m_outSideRect, (double)endAngle * (Math.PI / 180.0), isCircle: true);
				PointF pointByAngle4 = ChartMath.GetPointByAngle(m_inSideRect, (double)endAngle * (Math.PI / 180.0), isCircle: true);
				pie3DSegment.RightSide = new GraphicsPath();
				pie3DSegment.RightSide.AddPolygon(new PointF[4]
				{
					pointByAngle4,
					new PointF(pointByAngle4.X, pointByAngle4.Y - m_depth),
					new PointF(pointByAngle3.X, pointByAngle3.Y - m_depth),
					pointByAngle3
				});
			}
			if (!m_inSideRect.IsEmpty)
			{
				pie3DSegment.InnerSide = new GraphicsPath();
				AddSector(pie3DSegment.InnerSide, m_inSideRect, startAngle, endAngle - startAngle);
				AddSector(pie3DSegment.InnerSide, m_inUpSideRect, endAngle, startAngle - endAngle);
				pie3DSegment.InnerSide.CloseFigure();
			}
			pie3DSegment.OuterSide = new GraphicsPath();
			AddSector(pie3DSegment.OuterSide, m_outSideRect, startAngle, endAngle - startAngle);
			AddSector(pie3DSegment.OuterSide, m_outUpSideRect, endAngle, startAngle - endAngle);
			pie3DSegment.OuterSide.CloseFigure();
			return pie3DSegment;
		}

		private void AddSector(GraphicsPath gp, RectangleF rect, float start, float angle)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			PointF[] points = new PointF[3]
			{
				rect.Location,
				new PointF(rect.Right, rect.Top),
				new PointF(rect.Left, rect.Bottom)
			};
			angle = ((!(angle < 0f)) ? Math.Max(angle, 0.0001f) : (0f - Math.Max(0f - angle, 0.0001f)));
			graphicsPath.AddArc(0f, 0f, rect.Width, rect.Width, start, angle);
			graphicsPath.Transform(new Matrix(new RectangleF(0f, 0f, rect.Width, rect.Width), points));
			gp.AddPath(graphicsPath, connect: true);
		}

		private void AddClosedSector(GraphicsPath gp, RectangleF rect, float start, float angle)
		{
			GraphicsPath graphicsPath = new GraphicsPath();
			PointF[] points = new PointF[3]
			{
				rect.Location,
				new PointF(rect.Right, rect.Top),
				new PointF(rect.Left, rect.Bottom)
			};
			angle = ((!(angle < 0f)) ? Math.Max(angle, 0.0001f) : (0f - Math.Max(0f - angle, 0.0001f)));
			if (angle == 360f)
			{
				graphicsPath.AddArc(0f, 0f, rect.Width, rect.Width, start, angle);
			}
			else
			{
				graphicsPath.AddPie(0f, 0f, rect.Width, rect.Width, start, angle);
			}
			graphicsPath.Transform(new Matrix(new RectangleF(0f, 0f, rect.Width, rect.Width), points));
			gp.AddPath(graphicsPath, connect: true);
		}
	}

	private class PieSectorComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			int result = 0;
			PieSector pieSector = x as PieSector;
			PieSector pieSector2 = y as PieSector;
			if (pieSector != pieSector2 && pieSector != null && pieSector2 != null)
			{
				float num = pieSector.StartAngle % 360f;
				float num2 = pieSector2.StartAngle % 360f;
				result = ((num > 90f && num < 270f) ? ((!(num2 > 90f) || !(num2 < 270f)) ? (-1) : ((!(num > num2)) ? 1 : (-1))) : (((num2 > 90f && num2 < 270f) || (num < 90f && num2 > 270f)) ? 1 : ((!(num > 270f) || !(num2 < 90f)) ? ((!(num < num2)) ? 1 : (-1)) : (-1))));
			}
			return result;
		}
	}

	private class PieLabel
	{
		private ChartStyledPoint m_styledPoint;

		private RectangleF m_rect;

		private PointF m_connectPoint;

		private PointF m_notCorrectPoint;

		private double m_value;

		private float m_angle;

		private PieSectorCorner m_corner;

		private ChartSeries m_series;

		private int m_labelIndex;

		private bool m_cornerLabel;

		public ChartStyledPoint StyledPoint => m_styledPoint;

		public RectangleF Rectangle
		{
			get
			{
				return m_rect;
			}
			set
			{
				m_rect = value;
			}
		}

		public PointF ConnectPoint
		{
			get
			{
				return m_connectPoint;
			}
			set
			{
				m_connectPoint = value;
			}
		}

		public PointF NotCorrectPoint => m_notCorrectPoint;

		public double Value
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value = value;
			}
		}

		public bool CornerLabel
		{
			get
			{
				return m_cornerLabel;
			}
			set
			{
				m_cornerLabel = value;
			}
		}

		public ChartSeries Series
		{
			get
			{
				return m_series;
			}
			set
			{
				m_series = value;
			}
		}

		public int LabelIndex
		{
			get
			{
				return m_labelIndex;
			}
			set
			{
				m_labelIndex = value;
			}
		}

		public float Angle
		{
			get
			{
				return m_angle;
			}
			set
			{
				m_angle = value;
			}
		}

		public PieSectorCorner Corner
		{
			get
			{
				return m_corner;
			}
			set
			{
				m_corner = value;
			}
		}

		public PieLabel(ChartStyledPoint styledPoint, float angle)
		{
			m_styledPoint = styledPoint;
			m_rect = RectangleF.Empty;
			m_angle = angle;
			m_series = null;
			m_labelIndex = 0;
		}

		public PieLabel(ChartStyledPoint styledPoint, float angle, ChartSeries series, int LblIndex)
		{
			m_styledPoint = styledPoint;
			m_rect = RectangleF.Empty;
			m_angle = angle;
			m_series = series;
			m_labelIndex = LblIndex;
		}

		public SizeF Measure(ChartGraph g, float maxWidth)
		{
			if (m_styledPoint.Style.Callout.Enable)
			{
				ChartCalloutInfo callout = m_styledPoint.Style.Callout;
				SizeF empty = SizeF.Empty;
				SizeF sizeF = SizeF.Empty;
				SizeF empty2 = SizeF.Empty;
				string[] array = Regex.Split(callout.Text, "\n");
				float textOffset = callout.TextOffset;
				string[] array2 = array;
				foreach (string text in array2)
				{
					empty2 = g.MeasureString(text, callout.Font.GdipFont, maxWidth);
					if (empty.Width < empty2.Width)
					{
						empty.Width = empty2.Width;
					}
					empty.Height += empty2.Height + textOffset;
					sizeF = new SizeF(empty.Width + textOffset * 2f, empty.Height + textOffset);
				}
				return m_rect.Size = sizeF;
			}
			if (m_styledPoint.Style.DisplayText)
			{
				if (Series.ConfigItems.PieItem.ShowDataBindLabels && Series.XAxis.LabelsImpl != null)
				{
					string text2 = m_series.XAxis.LabelsImpl.GetLabelAt(StyledPoint.Index).Text;
					Font font = m_series.XAxis.LabelsImpl.GetLabelAt(StyledPoint.Index).Font;
					return m_rect.Size = g.MeasureString(text2, font, maxWidth);
				}
				return m_rect.Size = g.MeasureString(m_styledPoint.Style.Text, m_styledPoint.Style.GdipFont, maxWidth);
			}
			return m_rect.Size = SizeF.Empty;
		}

		public void SetConnectPoint(PointF pt)
		{
			m_connectPoint = pt;
			m_notCorrectPoint = pt;
			m_rect.Location = pt;
		}

		public void CorrectTopLeft(float value)
		{
			ChartAccumulationLabelStyle labelStyle = m_series.ConfigItems.PieItem.LabelStyle;
			float num = ((labelStyle == ChartAccumulationLabelStyle.Outside && m_series.Style.TextOrientation == ChartTextOrientation.Up) ? 3f : 0f);
			m_rect.Location = new PointF(m_connectPoint.X - m_rect.Width - num, m_connectPoint.Y - m_rect.Height / 2f - num);
			if (labelStyle != ChartAccumulationLabelStyle.OutsideInArea)
			{
				float num2 = m_rect.Bottom - value;
				if (num2 > 0f)
				{
					m_rect.Location = new PointF(m_rect.X, m_rect.Y - num2);
					m_connectPoint = new PointF(m_connectPoint.X, m_connectPoint.Y - num2);
				}
			}
		}

		public void CorrectTopRight(float value)
		{
			ChartAccumulationLabelStyle labelStyle = m_series.ConfigItems.PieItem.LabelStyle;
			float num = ((labelStyle == ChartAccumulationLabelStyle.Outside && m_series.Style.TextOrientation == ChartTextOrientation.Up) ? 3f : 0f);
			m_rect.Location = new PointF(m_connectPoint.X + num, m_connectPoint.Y - m_rect.Height / 2f - num);
			if (labelStyle != ChartAccumulationLabelStyle.OutsideInArea)
			{
				float num2 = m_rect.Bottom - value;
				if (num2 > 0f)
				{
					m_rect.Location = new PointF(m_rect.X, m_rect.Y - num2);
					m_connectPoint = new PointF(m_connectPoint.X, m_connectPoint.Y - num2);
				}
			}
		}

		public void CorrectBottomLeft(float value)
		{
			ChartAccumulationLabelStyle labelStyle = m_series.ConfigItems.PieItem.LabelStyle;
			float num = ((labelStyle == ChartAccumulationLabelStyle.Outside && m_series.Style.TextOrientation == ChartTextOrientation.Up) ? 3f : 0f);
			m_rect.Location = new PointF(m_connectPoint.X - m_rect.Width - num, m_connectPoint.Y - m_rect.Height / 2f + num);
			if (labelStyle != ChartAccumulationLabelStyle.OutsideInArea)
			{
				float num2 = m_rect.Top - value;
				if (num2 < 0f)
				{
					m_rect.Location = new PointF(m_rect.X, m_rect.Y - num2);
					m_connectPoint = new PointF(m_connectPoint.X, m_connectPoint.Y - num2);
				}
			}
		}

		public void CorrectBottomRight(float value)
		{
			ChartAccumulationLabelStyle labelStyle = m_series.ConfigItems.PieItem.LabelStyle;
			float num = ((labelStyle == ChartAccumulationLabelStyle.Outside && m_series.Style.TextOrientation == ChartTextOrientation.Up) ? 3f : 0f);
			m_rect.Location = new PointF(m_connectPoint.X + num, m_connectPoint.Y - m_rect.Height / 2f + num);
			if (labelStyle != ChartAccumulationLabelStyle.OutsideInArea)
			{
				float num2 = m_rect.Top - value;
				if (num2 < 0f)
				{
					m_rect.Location = new PointF(m_rect.X, m_rect.Y - num2);
					m_connectPoint = new PointF(m_connectPoint.X, m_connectPoint.Y - num2);
				}
			}
		}

		public void AlignRightSide(float value)
		{
			float num = m_rect.Right - value;
			m_rect.Location = new PointF(m_rect.X - num, m_rect.Y);
			m_connectPoint = new PointF(m_connectPoint.X - num, m_connectPoint.Y);
		}

		public void AlignLeftSide(float value)
		{
			m_rect.Location = new PointF(value, m_rect.Y);
			m_connectPoint = new PointF(value, m_connectPoint.Y);
		}
	}

	private class PieLabelComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			int result = 0;
			PieLabel pieLabel = x as PieLabel;
			PieLabel pieLabel2 = y as PieLabel;
			if (pieLabel != null && pieLabel2 != null && pieLabel.Value != pieLabel2.Value)
			{
				result = ((pieLabel.Value > pieLabel2.Value) ? 1 : (-1));
			}
			return result;
		}
	}

	private class Pie3DSegment
	{
		public float StartAngle;

		public float EndAngle;

		public GraphicsPath LeftSide;

		public GraphicsPath RightSide;

		public GraphicsPath OuterSide;

		public GraphicsPath InnerSide;

		public PieSector Sector;

		public void Draw(ChartGraph graph, BrushInfo interior, Pen pen, ChartSeries series)
		{
			if (StartAngle < 90f || StartAngle > 270f)
			{
				DrawLeft(graph, interior, pen, series);
			}
			if (EndAngle > 90f && EndAngle < 270f)
			{
				DrawRight(graph, interior, pen, series);
			}
			if (EndAngle != StartAngle)
			{
				if (StartAngle >= 180f)
				{
					DrawOuter(graph, interior, pen);
					DrawInner(graph, interior, pen);
				}
				else
				{
					DrawInner(graph, interior, pen);
					DrawOuter(graph, interior, pen);
				}
			}
			if (StartAngle > 90f && StartAngle < 270f)
			{
				DrawLeft(graph, interior, pen, series);
			}
			if (EndAngle < 90f || EndAngle > 270f)
			{
				DrawRight(graph, interior, pen, series);
			}
		}

		private void DrawInner(ChartGraph graph, BrushInfo interior, Pen pen)
		{
			if (InnerSide != null)
			{
				graph.DrawPath(interior, null, InnerSide);
				using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Sector.InnerBounds, Color.White, Color.Black, LinearGradientMode.Horizontal);
				linearGradientBrush.InterpolationColors = c_cylinderPhong;
				graph.DrawPath(linearGradientBrush, pen, InnerSide);
			}
		}

		private void DrawOuter(ChartGraph graph, BrushInfo interior, Pen pen)
		{
			if (OuterSide != null)
			{
				graph.DrawPath(interior, null, OuterSide);
				using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Sector.OuterBounds, Color.White, Color.Black, LinearGradientMode.Horizontal);
				linearGradientBrush.InterpolationColors = c_cylinderPhong;
				graph.DrawPath(linearGradientBrush, pen, OuterSide);
			}
		}

		private void DrawLeft(ChartGraph graph, BrushInfo interior, Pen pen, ChartSeries series)
		{
			if (LeftSide == null)
			{
				return;
			}
			graph.DrawPath(interior, pen, LeftSide);
			if (series.ExplodedAll || series.ExplodedIndex >= 0)
			{
				using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Sector.OuterBounds, Color.White, Color.Black, LinearGradientMode.Horizontal))
				{
					linearGradientBrush.InterpolationColors = c_cylinderPhong;
					graph.DrawPath(linearGradientBrush, pen, LeftSide);
				}
			}
		}

		private void DrawRight(ChartGraph graph, BrushInfo interior, Pen pen, ChartSeries series)
		{
			if (RightSide == null)
			{
				return;
			}
			graph.DrawPath(interior, pen, RightSide);
			if (series.ExplodedAll || series.ExplodedIndex >= 0)
			{
				using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Sector.OuterBounds, Color.White, Color.Black, LinearGradientMode.Horizontal))
				{
					linearGradientBrush.InterpolationColors = c_cylinderPhong;
					graph.DrawPath(linearGradientBrush, pen, RightSide);
				}
			}
		}
	}

	private const double D_TO_R = Math.PI / 180.0;

	private const double R_TO_D = 180.0 / Math.PI;

	private const float MIN_RADIUS = 50f;

	private const float MAX_TEXT_WIDTH = 300f;

	private const float MIN_FOV_SECTOR = 8f;

	private const float MARGINS_RATIO = 0.03f;

	private const float SPACE_RATIO = 0.02f;

	private const float TICK_RATIO = 0.05f;

	private const float PERCENT_COEF = 0.01f;

	private const float INSIDE_LABELS_COEF = 0.8f;

	private const float SHRINK_RATIO = 0.86f;

	private const string c_pieRegionDescription = "Pie Chart Region";

	private const int c_drawSides180Order = 1;

	private const int c_drawSides360Order = 0;

	private const int c_drawInnerOrder = 2;

	private const int c_drawOuterOrder = 3;

	private const int c_drawTopOrder = 4;

	private const int c_drawOrdersCount = 5;

	private static ColorBlend c_cylinderPhong;

	private const float c_orderEpsilon = 0.0001f;

	private float m_startAngle;

	private static readonly ColorBlend s_insideGradient;

	private static readonly ColorBlend s_outsideGradient;

	private static readonly ColorBlend s_roundGradient;

	private static readonly ColorBlend s_bevelGradient;

	private RectangleF? m_outRect;

	internal RectangleF? OuterRect
	{
		get
		{
			return m_outRect;
		}
		set
		{
			m_outRect = value;
		}
	}

	public override ChartUsedSpaceType FillSpaceType
	{
		get
		{
			if (base.ChartArea.MultiplePies)
			{
				return ChartUsedSpaceType.OneForOne;
			}
			return ChartUsedSpaceType.All;
		}
	}

	protected override int RequireYValuesCount => 1;

	static PieRenderer()
	{
		s_insideGradient = new ColorBlend
		{
			Positions = new float[3] { 0f, 0.05f, 1f },
			Colors = new Color[3]
			{
				Color.Transparent,
				Color.Transparent,
				Color.FromArgb(1677721600)
			}
		};
		s_outsideGradient = new ColorBlend
		{
			Positions = new float[3] { 0f, 0.05f, 1f },
			Colors = new Color[3]
			{
				Color.FromArgb(838860800),
				Color.Transparent,
				Color.Transparent
			}
		};
		s_roundGradient = new ColorBlend
		{
			Positions = new float[4] { 0f, 0.4f, 0.6f, 1f },
			Colors = new Color[4]
			{
				Color.FromArgb(1207959552),
				Color.Transparent,
				Color.Transparent,
				Color.FromArgb(1207959552)
			}
		};
		s_bevelGradient = new ColorBlend
		{
			Positions = new float[6] { 0f, 0.04f, 0.05f, 0.3f, 0.4f, 1f },
			Colors = new Color[6]
			{
				Color.FromArgb(838860800),
				Color.FromArgb(1677721600),
				Color.FromArgb(838860800),
				Color.FromArgb(1207959552),
				Color.FromArgb(603979776),
				Color.Transparent
			}
		};
	}

	public PieRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		ColorBlend colorBlend = new ColorBlend();
		ChartSeriesRenderer.PhongShadingColors(Color.FromArgb(144, Color.Black), Color.FromArgb(144, Color.Black), Color.FromArgb(100, Color.White), Math.PI / 4.0, 30.0, out var colors, out var positions);
		colorBlend.Positions = positions;
		colorBlend.Colors = colors;
		c_cylinderPhong = colorBlend;
		ChartPieConfigItem pieItem = m_series.ConfigItems.PieItem;
		m_startAngle = (float)ChartMath.ModAngle(pieItem.AngleOffset, 360.0);
		int num = base.Chart.Series.IndexOf(m_series);
		int num2 = m_series.PointFormats[ChartYValueUsage.YValue];
		double allValue = GetAllValue();
		float num3 = 0f;
		foreach (ChartSeries item in base.Chart.Series)
		{
			if (item.Type == ChartSeriesType.Pie && item.ConfigItems.PieItem.PieWithSameRadius)
			{
				ChartStyleInfo offlineStyle = item.GetOfflineStyle();
				SizeF sizeF = args.Graph.MeasureString(item.Text, offlineStyle.GdipFont);
				if (num3 < sizeF.Height)
				{
					num3 = sizeF.Height;
				}
			}
		}
		if (allValue == 0.0)
		{
			return;
		}
		double num4 = 360.0 / allValue;
		bool series3D = base.Chart.Series3D;
		_ = m_series.OptimizePiePointPositions;
		bool flag = pieItem.LabelStyle == ChartAccumulationLabelStyle.OutsideInColumn || pieItem.LabelStyle == ChartAccumulationLabelStyle.Outside || pieItem.LabelStyle == ChartAccumulationLabelStyle.OutsideInArea;
		bool flag2 = m_series.ShowTicks && flag;
		RectangleF rectangleF = args.Bounds;
		SizeF empty = SizeF.Empty;
		empty = pieItem.PieSize;
		if (base.ChartArea.MultiplePies && pieItem.PieRadius <= 0f)
		{
			rectangleF = RenderingHelper.GetPieBounds(num, base.Chart.Series.VisibleList.Count, rectangleF, pieItem.DoughnutCoeficient);
			if (num != 0)
			{
				RenderingHelper.GetPieBounds(num - 1, base.Chart.Series.VisibleList.Count, rectangleF);
			}
		}
		if (pieItem.ShowSeriesTitle)
		{
			SizeF size = args.Graph.MeasureString(args.Series.Text, base.SeriesStyle.GdipFont);
			if (pieItem.PieWithSameRadius && (pieItem.LabelStyle == ChartAccumulationLabelStyle.Outside || pieItem.LabelStyle == ChartAccumulationLabelStyle.OutsideInColumn))
			{
				size.Height = num3;
			}
			RectangleF rect = LayoutHelper.AlignRectangle(rectangleF, size, ContentAlignment.BottomCenter);
			using (SolidBrush brush = new SolidBrush(base.SeriesStyle.TextColor))
			{
				args.Graph.DrawString(args.Series.Text, base.SeriesStyle.GdipFont, brush, rect);
			}
			rectangleF.Height -= size.Height;
		}
		float num5 = Math.Min(rectangleF.Width / 2f, rectangleF.Height / 2f);
		float num6 = num5;
		float num7 = 0f;
		PointF center = ChartMath.GetCenter(rectangleF);
		SizeF empty2 = SizeF.Empty;
		SizeF empty3 = SizeF.Empty;
		ChartStyledPoint[] list = PrepearePoints().Clone() as ChartStyledPoint[];
		ChartStyledPoint[] visiblePoints = GetVisiblePoints(list);
		int num8 = visiblePoints.Length;
		PieLabel[] array = null;
		for (int i = 0; i < num8; i++)
		{
			ChartCalloutInfo callout = visiblePoints[i].Style.Callout;
			if (callout.Enable)
			{
				string displayTextAndFormat = callout.DisplayTextAndFormat;
				object[] array2 = new object[3] { m_series.Name, null, null };
				if (m_series.Points.isCategory)
				{
					array2[1] = visiblePoints[i].Category;
				}
				else if (m_series.XAxis.ValueType == ChartValueType.DateTime)
				{
					array2[1] = m_series.Points[i].DateX;
				}
				else
				{
					array2[1] = visiblePoints[i].X;
				}
				array2[2] = visiblePoints[i].YValues[0];
				callout.Text = string.Format(displayTextAndFormat, array2);
			}
		}
		if (flag)
		{
			array = CreateLabels(visiblePoints);
			SizeF sizeF2 = MeasureLabels(array, args.Graph, new SizeF(rectangleF.Width / 2f, rectangleF.Height / 2f));
			num5 = ((array == null || array.Length == 0 || !array.Any((PieLabel x) => x.StyledPoint.Style.DisplayText)) ? Math.Min(sizeF2.Width, sizeF2.Height) : (num5 * 0.86f));
		}
		if (pieItem.PieWithSameRadius && (pieItem.LabelStyle == ChartAccumulationLabelStyle.Outside || pieItem.LabelStyle == ChartAccumulationLabelStyle.OutsideInColumn))
		{
			num5 = num6;
		}
		if (pieItem.PieRadius > 0f && !pieItem.PieWithSameRadius)
		{
			num5 = pieItem.PieRadius;
		}
		if (args.Is3D)
		{
			num7 = (pieItem.HeightByAreaDepth ? base.ChartArea.Depth : (pieItem.HeightCoeficient * num5));
			num7 += num7 * base.ChartArea.Tilt / 180f;
			empty2 = new SizeF(num5 - 0.03f * num5, num5 - 0.03f * num5 - num7 * base.ChartArea.Tilt / 90f);
			if (base.ChartArea.MultiplePies)
			{
				num7 = (pieItem.HeightByAreaDepth ? (base.ChartArea.Depth * (base.ChartArea.Tilt / 180f)) : pieItem.PieHeight);
				empty2 = new SizeF(num5 - 0.03f * num5, num5 - 0.03f * num5 - num7 * pieItem.PieTilt);
			}
			center = new PointF(center.X, center.Y - num7 / 2f);
		}
		else
		{
			empty2 = new SizeF(num5 - 0.03f * num5, num5 - 0.03f * num5);
		}
		if (flag)
		{
			SizeF empty4 = SizeF.Empty;
			int num9 = 5;
			empty4 = ((pieItem.LabelStyle != ChartAccumulationLabelStyle.Outside || !m_series.Style.Callout.Enable) ? empty2 : new SizeF(empty2.Width + (float)num9, empty2.Height + (float)num9));
			ArrangeLabels(array, empty4, center, pieItem.LabelStyle == ChartAccumulationLabelStyle.OutsideInColumn || pieItem.LabelStyle == ChartAccumulationLabelStyle.OutsideInArea);
			if (pieItem.PieRadius > 0f)
			{
				WrapLabels(args.Graph, array);
			}
		}
		float num10 = 0.98f;
		empty2 = new SizeF(num10 * empty2.Width, num10 * empty2.Height);
		if (flag2)
		{
			empty2.Width -= base.SeriesStyle.TextOffset;
			empty2.Height -= base.SeriesStyle.TextOffset;
		}
		empty3 = empty2;
		if (m_series.ExplodedAll || m_series.ExplodedIndex >= 0)
		{
			float num11 = 1f - 0.01f * m_series.ExplosionOffset;
			empty2 = new SizeF(empty2.Width * num11, empty2.Height * num11);
		}
		List<PieSector> list2 = new List<PieSector>();
		RectangleF rectangleF2 = new RectangleF(center.X - empty2.Width, center.Y - empty2.Height, 2f * empty2.Width, 2f * empty2.Height);
		double num12 = allValue * (double)m_startAngle / 360.0;
		int j = 0;
		for (int num13 = visiblePoints.Length; j < num13; j++)
		{
			if (!visiblePoints[j].IsVisible)
			{
				continue;
			}
			RectangleF rectangleF3 = rectangleF2;
			RectangleF inSideRect = rectangleF2;
			if (base.ChartArea.MultiplePies && base.Chart.Series.VisibleCount > 1)
			{
				if (!m_outRect.HasValue)
				{
					m_outRect = rectangleF3;
				}
				inSideRect = RenderingHelper.GetPieBounds(num - 1, base.Chart.Series.VisibleList.Count, m_outRect.Value, pieItem.DoughnutCoeficient);
			}
			else
			{
				inSideRect.Inflate(0f - (1f - pieItem.DoughnutCoeficient) * (rectangleF3.Width - empty.Width) / 2f, 0f - (1f - pieItem.DoughnutCoeficient) * (rectangleF3.Height - empty.Height) / 2f);
			}
			double maxZero = GetMaxZero(visiblePoints[j].YValues[num2]);
			ChartStyleInfo style = visiblePoints[j].Style;
			if (m_series.ExplodedIndex == visiblePoints[j].Index || m_series.ExplodedAll)
			{
				PointF pointF = new PointF((float)Math.Cos(Math.PI * 2.0 * (num12 + maxZero / 2.0) / allValue), (float)Math.Sin(Math.PI * 2.0 * (num12 + maxZero / 2.0) / allValue));
				rectangleF3.Offset(0.01f * empty3.Width * pointF.X * m_series.ExplosionOffset, 0.01f * empty3.Height * pointF.Y * m_series.ExplosionOffset);
				inSideRect.Offset(0.01f * empty3.Width * pointF.X * m_series.ExplosionOffset, 0.01f * empty3.Height * pointF.Y * m_series.ExplosionOffset);
			}
			list2.Add(new PieSector(visiblePoints[j], rectangleF3, inSideRect, (float)(num4 * num12), (float)(num4 * (num12 + maxZero)), num7));
			if (!series3D && style.DisplayShadow)
			{
				Matrix matrix = new Matrix();
				GraphicsPath graphicsPath = new GraphicsPath();
				graphicsPath.AddPie(Rectangle.Round(rectangleF3), (float)(num4 * num12), (float)(num4 * maxZero));
				matrix.Translate(style.ShadowOffset.Width, style.ShadowOffset.Height);
				graphicsPath.Transform(matrix);
				args.Graph.DrawPath(style.ShadowInterior, null, graphicsPath);
			}
			num12 = (num12 + maxZero) % allValue;
		}
		ColorBlend gradient = ((pieItem.PieType == ChartPieType.Custom) ? pieItem.Gradient : SelectKnow(pieItem.PieType));
		List<Pie3DSegment> list3 = new List<Pie3DSegment>();
		foreach (PieSector item2 in list2)
		{
			IEnumerable<Pie3DSegment> enumerable = item2.Render();
			if (enumerable != null)
			{
				list3.AddRange(enumerable);
			}
		}
		list3.Sort(CompareByStartAngle);
		if (args.Is3D)
		{
			foreach (Pie3DSegment item3 in list3)
			{
				item3.Draw(args.Graph, GetBrush(item3.Sector.StyledPoint.Index), item3.Sector.StyledPoint.Style.GdipPen, m_series);
			}
		}
		foreach (PieSector item4 in list2)
		{
			item4.Draw(args.Graph, GetBrush(item4.StyledPoint.Index), item4.StyledPoint.Style.GdipPen, pieItem.FillMode, gradient);
		}
		if (pieItem.LabelStyle == ChartAccumulationLabelStyle.Disabled)
		{
			return;
		}
		if (flag)
		{
			PieLabel[] array3 = array;
			foreach (PieLabel pieLabel in array3)
			{
				RectangleF rect2 = rectangleF2;
				ChartStyleInfo style2 = pieLabel.StyledPoint.Style;
				ChartCalloutInfo callout2 = style2.Callout;
				if (callout2.Enable)
				{
					DrawCalloutOutsideLabel(args, callout2, pieLabel, rectangleF2);
					continue;
				}
				if (style2.DrawTextShape && style2.TextShape.Type == ChartCustomShape.Square)
				{
					using SolidBrush brush2 = new SolidBrush(style2.TextShape.Color);
					float num14 = style2.TextShape.BorderWidth / 2f;
					args.Graph.DrawRect(brush2, style2.TextShape.BorderGdiPen, pieLabel.Rectangle.X - num14, pieLabel.Rectangle.Y - num14, pieLabel.Rectangle.Width + num14, pieLabel.Rectangle.Height + num14);
				}
				if (!style2.DisplayText)
				{
					continue;
				}
				if (m_series.ShowTicks)
				{
					if (m_series.ExplodedIndex == pieLabel.StyledPoint.Index || m_series.ExplodedAll)
					{
						PointF pointF2 = new PointF((float)Math.Cos(pieLabel.Angle), (float)Math.Sin(pieLabel.Angle));
						rect2.Offset(0.01f * empty3.Width * pointF2.X * m_series.ExplosionOffset, 0.01f * empty3.Height * pointF2.Y * m_series.ExplosionOffset);
					}
					args.Graph.DrawLine(style2.GdipPen, ChartMath.GetPointByAngle(rect2, pieLabel.Angle, isCircle: true), pieLabel.NotCorrectPoint);
					args.Graph.DrawLine(style2.GdipPen, pieLabel.ConnectPoint, pieLabel.NotCorrectPoint);
				}
				using SolidBrush brush3 = new SolidBrush(style2.TextColor);
				if (m_series.ConfigItems.PieItem.ShowDataBindLabels && m_series.XAxis.LabelsImpl != null)
				{
					string text = m_series.XAxis.LabelsImpl.GetLabelAt(pieLabel.LabelIndex).Text;
					Font font = m_series.XAxis.LabelsImpl.GetLabelAt(pieLabel.LabelIndex).Font;
					args.Graph.DrawString(text, font, brush3, pieLabel.Rectangle);
				}
				else if (style2.Text != "")
				{
					args.Graph.DrawString(style2.Text, style2.GdipFont, brush3, pieLabel.Rectangle, StringFormat.GenericDefault);
				}
			}
			return;
		}
		StringFormat stringFormat = new StringFormat();
		stringFormat.Alignment = StringAlignment.Center;
		stringFormat.LineAlignment = StringAlignment.Center;
		num12 = allValue * (double)m_startAngle / 360.0;
		double num15 = Math.PI * 2.0 / allValue;
		double num16 = 0.0;
		double num17 = num12;
		int num18 = -1;
		array = CreateLabels(visiblePoints);
		ChartStyledPoint[] array4 = visiblePoints;
		foreach (ChartStyledPoint chartStyledPoint in array4)
		{
			num18++;
			if (chartStyledPoint.Point.IsEmpty)
			{
				continue;
			}
			ChartStyleInfo style3 = chartStyledPoint.Style;
			double maxZero2 = GetMaxZero(chartStyledPoint.YValues[num2]);
			double num19 = 0.800000011920929;
			num19 = ((m_series.Style.TextOrientation != ChartTextOrientation.Center) ? (num19 * 3.0 / 4.0) : (num19 * 0.9));
			PointF location = new PointF(center.X + (float)(num19 * (double)empty2.Width * Math.Cos(Math.PI * 2.0 * (num12 + maxZero2 / 2.0) / allValue)), center.Y + (float)(num19 * (double)empty2.Height * Math.Sin(Math.PI * 2.0 * (num12 + maxZero2 / 2.0) / allValue)));
			num17 = (num17 + maxZero2 / 2.0) % allValue;
			num16 = (double)(float)(maxZero2 / 2.0) / allValue * 360.0;
			if (style3.Callout.Enable)
			{
				DrawCalloutInsideLabel(args, style3.Callout, list2[num18], rectangleF2, (float)(num17 * num15), num16, array[num18]);
				num12 += maxZero2;
				num17 = (num17 + maxZero2 / 2.0) % allValue;
				continue;
			}
			string text2 = style3.Text;
			if (style3.DrawTextShape && style3.TextShape.Type == ChartCustomShape.Square)
			{
				using SolidBrush brush4 = new SolidBrush(style3.TextShape.Color);
				float num20 = style3.TextShape.BorderWidth / 2f;
				Font gdipFont = style3.GdipFont;
				SizeF sizeF3 = args.Graph.MeasureString(text2, gdipFont);
				args.Graph.DrawRect(brush4, style3.TextShape.BorderGdiPen, location.X - num20, location.Y - num20 - (float)gdipFont.GetAscent(text2), sizeF3.Width + num20, sizeF3.Height + num20);
			}
			if (style3.DisplayText)
			{
				using (Brush brush5 = new SolidBrush(style3.TextColor))
				{
					if (m_series.ConfigItems.PieItem.ShowDataBindLabels && m_series.XAxis.LabelsImpl != null)
					{
						text2 = m_series.XAxis.LabelsImpl.GetLabelAt(chartStyledPoint.Index).Text;
						Font font2 = m_series.XAxis.LabelsImpl.GetLabelAt(chartStyledPoint.Index).Font;
						args.Graph.DrawString(text2, font2, brush5, location, stringFormat);
					}
					else if (!string.IsNullOrEmpty(style3.Text))
					{
						SizeF sizeF4 = args.Graph.MeasureString(style3.Text, style3.GdipFont);
						if ((double)pieItem.DoughnutCoeficient <= 0.5)
						{
							location.X -= sizeF4.Width / 2f;
						}
						else
						{
							location.X -= sizeF4.Width * pieItem.DoughnutCoeficient / 2f;
						}
						args.Graph.DrawString(style3.Text, style3.GdipFont, brush5, location, stringFormat);
					}
				}
				num12 += maxZero2;
			}
			num17 = (num17 + maxZero2 / 2.0) % allValue;
		}
	}

	private void DrawCalloutOutsideLabel(ChartRenderArgs2D args, ChartCalloutInfo callout, PieLabel pieLabel, RectangleF boundsRect)
	{
		SizeF siz = SizeF.Empty;
		Regex.Split(callout.Text, "\n");
		PointF pointByAngle = ChartMath.GetPointByAngle(boundsRect, pieLabel.Angle, isCircle: true);
		float hiddenX = callout.HiddenX;
		float hiddenY = callout.HiddenY;
		_ = pointByAngle.X;
		_ = pointByAngle.Y;
		float offsetX = callout.OffsetX;
		float offsetY = callout.OffsetY;
		m_series.ConfigItems.PieItem.LabelStyle.ToString();
		PointF pt = new PointF(pieLabel.Rectangle.X + hiddenX, pieLabel.Rectangle.Y + hiddenY);
		siz = new SizeF(pieLabel.Rectangle.Width, pieLabel.Rectangle.Height);
		if (callout.IsDragged)
		{
			Rectangle bounds = base.ChartArea.Bounds;
			float num = bounds.X + bounds.Width;
			float num2 = bounds.Y + bounds.Height;
			hiddenX = callout.HiddenX / 100f * num;
			hiddenY = callout.HiddenY / 100f * num2;
			pt.X = hiddenX;
			pt.Y = hiddenY;
		}
		else
		{
			pt.X += offsetX;
			pt.Y += offsetY;
		}
		pt = ChangeCalloutPosition(pt, siz);
		Rectangle caloutBnds = new Rectangle((int)pt.X, (int)pt.Y, (int)siz.Width, (int)siz.Height);
		callout.Position = LabelPosition.Center;
		caloutBnds = CalculateOuterPolygonPosition(callout, caloutBnds, pointByAngle, pieLabel);
		DrawCalloutPolygon(args, callout, caloutBnds, pointByAngle);
		DrawCalloutText(args, callout, caloutBnds, siz, pieLabel.LabelIndex);
	}

	private void DrawCalloutInsideLabel(ChartRenderArgs2D args, ChartCalloutInfo callout, PieSector sector, RectangleF bnds, float angl, double centerAngle, PieLabel label)
	{
		PointF pointByAngle = ChartMath.GetPointByAngle(bnds, angl, isCircle: true);
		PointF pt = pointByAngle;
		_ = pointByAngle.X;
		_ = pointByAngle.Y;
		float textOffset = callout.TextOffset;
		float num = 10f;
		float num2 = 0f;
		float offsetX = callout.OffsetX;
		float offsetY = callout.OffsetY;
		float num3 = 0f;
		int labelIndex = label.LabelIndex;
		SizeF empty = SizeF.Empty;
		SizeF siz = SizeF.Empty;
		SizeF empty2 = SizeF.Empty;
		Rectangle rectangle = default(Rectangle);
		string[] array = Regex.Split(callout.Text, "\n");
		double[] array2 = new double[10] { 45.0, 67.5, 112.5, 135.0, 202.5, 225.0, 247.5, 292.5, 315.0, 337.5 };
		centerAngle += (double)sector.StartAngle;
		string[] array3 = array;
		foreach (string text in array3)
		{
			empty2 = args.Graph.MeasureString(text, callout.Font.GdipFont);
			if (empty.Width < empty2.Width)
			{
				empty.Width = empty2.Width;
			}
			empty.Height += empty2.Height + textOffset;
			siz = new SizeF(empty.Width + textOffset * 2f, empty.Height + textOffset);
		}
		if (!callout.IsDragged)
		{
			switch (label.Corner)
			{
			case PieSectorCorner.BottomRight:
				if (centerAngle <= array2[0] || centerAngle > 360.0)
				{
					pt.X -= num + siz.Width;
					pt.Y -= num + siz.Height / 2f;
				}
				else if (centerAngle <= array2[1])
				{
					pt.X -= siz.Width * 0.75f;
					pt.Y -= num + siz.Height;
				}
				else
				{
					pt.X -= siz.Width / 2f;
					pt.Y -= num + siz.Height;
				}
				break;
			case PieSectorCorner.BottomLeft:
				if (centerAngle <= array2[2])
				{
					pt.X -= siz.Width / 2f;
					pt.Y -= num + siz.Height;
				}
				else if (centerAngle <= array2[3])
				{
					pt.X -= siz.Width * 0.25f;
					pt.Y -= num + siz.Height;
				}
				else
				{
					pt.X += num;
					pt.Y -= num + siz.Height / 2f;
				}
				break;
			case PieSectorCorner.TopLeft:
				if (centerAngle <= array2[4])
				{
					pt.X += num;
					pt.Y -= siz.Height / 2f;
				}
				else if (centerAngle <= array2[5])
				{
					pt.X += num;
				}
				else if (centerAngle <= array2[6])
				{
					pt.X -= siz.Width * 0.25f;
					pt.Y += num;
				}
				else
				{
					pt.X -= siz.Width / 2f;
					pt.Y += num;
				}
				break;
			case PieSectorCorner.TopRight:
				if (centerAngle <= array2[7])
				{
					pt.X -= siz.Width / 2f;
					pt.Y += num;
				}
				else if (centerAngle <= array2[8])
				{
					pt.X -= siz.Width * 0.75f;
					pt.Y += num;
				}
				else if (centerAngle <= array2[9])
				{
					pt.X -= siz.Width + num;
				}
				else
				{
					pt.X -= num + siz.Width;
					pt.Y -= siz.Height / 2f;
				}
				break;
			}
			pt.X += offsetX;
			pt.Y += offsetY;
		}
		else
		{
			Rectangle bounds = base.ChartArea.Bounds;
			float num4 = bounds.X + bounds.Width;
			float num5 = bounds.Y + bounds.Height;
			num2 = callout.HiddenX / 100f * num4;
			num3 = callout.HiddenY / 100f * num5;
			pt.X = num2;
			pt.Y = num3;
		}
		pt = ChangeCalloutPosition(pt, siz);
		rectangle = new Rectangle((int)pt.X, (int)pt.Y, (int)siz.Width, (int)siz.Height);
		callout.Position = LabelPosition.Center;
		CalculatePolygonPosition(callout, rectangle, pointByAngle);
		DrawCalloutPolygon(args, callout, rectangle, pointByAngle);
		DrawCalloutText(args, callout, rectangle, siz, labelIndex);
	}

	private Rectangle CalculateOuterPolygonPosition(ChartCalloutInfo callout, Rectangle caloutBnds, PointF pointPos, PieLabel label)
	{
		int num = 5;
		if ((float)caloutBnds.X > pointPos.X + (float)num)
		{
			callout.Position = LabelPosition.Right;
		}
		if ((float)caloutBnds.Y > pointPos.Y + (float)num)
		{
			callout.Position = LabelPosition.Bottom;
		}
		if ((float)(caloutBnds.X + caloutBnds.Width) < pointPos.X - (float)num)
		{
			callout.Position = LabelPosition.Left;
		}
		if ((float)(caloutBnds.Y + caloutBnds.Height) < pointPos.Y - (float)num)
		{
			callout.Position = LabelPosition.Top;
		}
		if (callout.Position == LabelPosition.Center && !callout.IsDragged)
		{
			switch (label.Corner)
			{
			case PieSectorCorner.BottomLeft:
				caloutBnds.X -= num;
				caloutBnds.Y += num;
				break;
			case PieSectorCorner.TopLeft:
				caloutBnds.X -= num;
				caloutBnds.Y -= num;
				break;
			case PieSectorCorner.BottomRight:
				caloutBnds.X += num;
				caloutBnds.Y += num;
				break;
			case PieSectorCorner.TopRight:
				caloutBnds.X += num;
				caloutBnds.Y -= num;
				break;
			}
			caloutBnds = CalculateOuterPolygonPosition(callout, caloutBnds, pointPos, label);
		}
		return caloutBnds;
	}

	private PointF ChangeCalloutPosition(PointF pt, SizeF siz)
	{
		Rectangle renderBounds = base.ChartArea.RenderBounds;
		if (pt.X < (float)renderBounds.X)
		{
			pt.X = renderBounds.X;
		}
		if (pt.Y < (float)renderBounds.Y)
		{
			pt.Y = renderBounds.Y;
		}
		if (pt.X + siz.Width > (float)(renderBounds.X + renderBounds.Width))
		{
			pt.X = (float)(renderBounds.X + renderBounds.Width) - siz.Width;
		}
		if (pt.Y + siz.Height > (float)(renderBounds.Y + renderBounds.Height))
		{
			pt.Y = (float)(renderBounds.Y + renderBounds.Height) - siz.Height;
		}
		return pt;
	}

	private void DrawCalloutPolygon(ChartRenderArgs2D args, ChartCalloutInfo callout, Rectangle bounds, PointF pointPos)
	{
		using SolidBrush brush = new SolidBrush(callout.Color);
		PointF[] points = CalculateCalloutPath(callout, bounds, pointPos);
		args.Graph.DrawPolygon(callout.Border.GdipPen, points);
		args.Graph.FillPolygon(brush, points);
	}

	private void DrawCalloutText(ChartRenderArgs2D args, ChartCalloutInfo callout, Rectangle bounds, SizeF siz, int index)
	{
		float textOffset = callout.TextOffset;
		string[] array = Regex.Split(callout.Text, "\n");
		PointF location = new PointF(bounds.X, bounds.Y);
		float height = args.Graph.MeasureString(array[0], callout.Font.GdipFont).Height;
		location.X += textOffset;
		location.Y += textOffset;
		float num = bounds.Y;
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (!(text == ""))
			{
				location.Y = num + textOffset;
				using (Brush brush = new SolidBrush(callout.TextColor))
				{
					args.Graph.DrawString(text, callout.Font.GdipFont, brush, Rectangle.Round(new RectangleF(location, siz)));
				}
				num = location.Y + height;
			}
		}
	}

	public override void Render(Graphics3D g)
	{
		ChartPieConfigItem pieItem = m_series.ConfigItems.PieItem;
		int num = base.Chart.Series.IndexOf(m_series);
		m_startAngle = (float)ChartMath.ModAngle(pieItem.AngleOffset, 360.0);
		double allValue = GetAllValue();
		if (allValue == 0.0)
		{
			return;
		}
		double num2 = 360.0 / allValue;
		_ = m_series.OptimizePiePointPositions;
		bool flag = pieItem.LabelStyle == ChartAccumulationLabelStyle.Outside || pieItem.LabelStyle == ChartAccumulationLabelStyle.OutsideInColumn;
		RectangleF bounds = base.Bounds;
		SizeF empty = SizeF.Empty;
		empty = pieItem.PieSize;
		if (base.ChartArea.MultiplePies && pieItem.PieRadius <= 0f)
		{
			bounds = RenderingHelper.GetPieBounds(num, base.Chart.Series.VisibleList.Count, bounds);
			if (num != 0)
			{
				RenderingHelper.GetPieBounds(num - 1, base.Chart.Series.VisibleList.Count, bounds);
			}
		}
		float num3 = 0f;
		foreach (ChartSeries item in base.Chart.Series)
		{
			if (item.Type == ChartSeriesType.Pie && item.ConfigItems.PieItem.PieWithSameRadius)
			{
				ChartStyleInfo offlineStyle = item.GetOfflineStyle();
				SizeF sizeF = g.Graphics.MeasureString(item.Text, offlineStyle.GdipFont);
				if (num3 < sizeF.Height)
				{
					num3 = sizeF.Height;
				}
			}
		}
		if (pieItem.ShowSeriesTitle)
		{
			GraphicsPath gp = new GraphicsPath();
			SizeF size = g.Graphics.MeasureString(m_series.Text, base.SeriesStyle.GdipFont);
			if (pieItem.PieWithSameRadius && (pieItem.LabelStyle == ChartAccumulationLabelStyle.Outside || pieItem.LabelStyle == ChartAccumulationLabelStyle.OutsideInColumn))
			{
				size.Height = num3;
			}
			RectangleF rect = LayoutHelper.AlignRectangle(bounds, size, ContentAlignment.BottomCenter);
			RenderingHelper.AddTextPath(gp, g.Graphics, m_series.Text, base.SeriesStyle.GdipFont, rect);
			g.AddPolygon(Path3D.FromGraphicsPath(gp, 0.0, new SolidBrush(base.SeriesStyle.TextColor)));
			bounds.Height -= size.Height;
		}
		base.Chart.Series.IndexOf(m_series);
		float num4 = (float)(0.9700000006705523 * (double)Math.Min(bounds.Width / 2f, bounds.Height / 2f));
		float num5 = num4;
		float num6 = 0f;
		ChartStyledPoint[] list = PrepearePoints().Clone() as ChartStyledPoint[];
		ChartStyledPoint[] visiblePoints = GetVisiblePoints(list);
		int num7 = visiblePoints.Length;
		PieLabel[] array = null;
		if (flag)
		{
			array = CreateLabels(visiblePoints);
			SizeF sizeF2 = MeasureLabels(array, new ChartGDIGraph(g.Graphics), new SizeF(bounds.Width / 2f, bounds.Height / 2f));
			num4 = Math.Min(sizeF2.Width, sizeF2.Height);
			ArrangeLabels(array, new SizeF(num4, num4), base.Center, pieItem.LabelStyle == ChartAccumulationLabelStyle.OutsideInColumn);
		}
		if (pieItem.PieWithSameRadius && (pieItem.LabelStyle == ChartAccumulationLabelStyle.Outside || pieItem.LabelStyle == ChartAccumulationLabelStyle.OutsideInColumn))
		{
			num4 = num5;
			ArrangeLabels(array, new SizeF(num4, num4), base.Center, inColumn: false);
		}
		if (pieItem.PieRadius > 0f)
		{
			num4 = pieItem.PieRadius;
			if (pieItem.PieWithSameRadius && (pieItem.LabelStyle == ChartAccumulationLabelStyle.Outside || pieItem.LabelStyle == ChartAccumulationLabelStyle.OutsideInColumn))
			{
				ArrangeLabels(array, new SizeF(num4, num4), base.Center, inColumn: false);
			}
		}
		float num8 = (pieItem.HeightByAreaDepth ? base.ChartArea.Depth : (pieItem.HeightCoeficient * num4));
		if (base.ChartArea.MultiplePies)
		{
			num8 = (pieItem.HeightByAreaDepth ? base.ChartArea.Depth : pieItem.PieHeight);
		}
		num4 *= 0.98f;
		if (m_series.ShowTicks)
		{
			num4 -= base.SeriesStyle.TextOffset;
		}
		num6 = num4;
		if (m_series.ExplodedAll || m_series.ExplodedIndex >= 0)
		{
			num4 = (1f - 0.01f * m_series.ExplosionOffset) * num4;
		}
		g.CreateBox(new Vector3D(base.Bounds.Left, base.Bounds.Top, 0.0), new Vector3D(base.Bounds.Right, base.Bounds.Bottom, num8), (Pen)null, (BrushInfo)null);
		double num9 = allValue * (double)m_startAngle / 360.0;
		RectangleF rectangleF = new RectangleF(base.Center.X - num4, base.Center.Y - num4, 2f * num4, 2f * num4);
		ArrayList[] array2 = new ArrayList[4]
		{
			new ArrayList(),
			new ArrayList(),
			new ArrayList(),
			new ArrayList()
		};
		for (int i = 0; i < num7; i++)
		{
			if (visiblePoints[i].Point.IsEmpty)
			{
				continue;
			}
			RectangleF rectangleF2 = rectangleF;
			double maxZero = GetMaxZero(visiblePoints[i].Point.YValues[0]);
			ChartStyleInfo styleAt = GetStyleAt(visiblePoints[i].Index);
			BrushInfo brush = GetBrush(visiblePoints[i].Index);
			new GraphicsPath();
			if (m_series.ExplodedIndex == visiblePoints[i].Index || m_series.ExplodedAll)
			{
				PointF pointF = new PointF((float)Math.Cos(Math.PI * 2.0 * (num9 + maxZero / 2.0) / allValue), (float)Math.Sin(Math.PI * 2.0 * (num9 + maxZero / 2.0) / allValue));
				rectangleF2.Offset(0.01f * num6 * pointF.X * m_series.ExplosionOffset, 0.01f * num6 * pointF.Y * m_series.ExplosionOffset);
			}
			Vector3D center = new Vector3D(rectangleF2.X + rectangleF2.Width / 2f, rectangleF2.Y + rectangleF2.Height / 2f, 0.0);
			Polygon[][] array3 = CreateSector(center, pieItem.DoughnutCoeficient * num4 + Math.Min(empty.Width, empty.Height) / 2f, num4, (float)(num2 * num9), (float)(num2 * maxZero), num8, styleAt.GdipPen, brush);
			for (int j = 0; j < array3.Length; j++)
			{
				if (array3[j] != null)
				{
					for (int k = 0; k < array3[j].Length; k++)
					{
						array2[j].Add(array3[j][k]);
					}
				}
			}
			num9 += maxZero;
		}
		for (int l = 0; l < array2.Length; l++)
		{
			foreach (Polygon item2 in array2[l])
			{
				g.AddPolygon(item2);
			}
		}
		if (pieItem.LabelStyle == ChartAccumulationLabelStyle.Disabled)
		{
			return;
		}
		num9 = (double)m_startAngle * allValue / 360.0;
		GraphicsPath graphicsPath = new GraphicsPath();
		ArrayList arrayList = new ArrayList();
		int num10 = 0;
		for (int m = 0; m < num7; m++)
		{
			if (visiblePoints[m].Point.IsEmpty)
			{
				continue;
			}
			double maxZero2 = GetMaxZero(visiblePoints[m].Point.YValues[0]);
			ChartStyleInfo styleAt2 = GetStyleAt(visiblePoints[m].Index);
			Brush br = new SolidBrush(styleAt2.TextColor);
			new StringFormat();
			if (styleAt2.DisplayText && styleAt2.Text != "" && !styleAt2.IsEmpty)
			{
				if (flag)
				{
					PieLabel pieLabel = array[num10];
					if (pieItem.PieRadius > 0f)
					{
						ChartGDIGraph g2 = new ChartGDIGraph(g.Graphics);
						WrapLabels(g2, array);
					}
					num10++;
					if (m_series.ShowTicks)
					{
						RectangleF rect2 = rectangleF;
						if (m_series.ExplodedIndex == pieLabel.StyledPoint.Index || m_series.ExplodedAll)
						{
							PointF pointF2 = new PointF((float)Math.Cos(pieLabel.Angle), (float)Math.Sin(pieLabel.Angle));
							rect2.Offset(0.01f * num6 * pointF2.X * m_series.ExplosionOffset, 0.01f * num6 * pointF2.Y * m_series.ExplosionOffset);
						}
						graphicsPath.AddLine(ChartMath.GetPointByAngle(rect2, pieLabel.Angle, isCircle: true), pieLabel.NotCorrectPoint);
						graphicsPath.AddLine(pieLabel.NotCorrectPoint, pieLabel.ConnectPoint);
						graphicsPath.AddLine(pieLabel.ConnectPoint, pieLabel.NotCorrectPoint);
						graphicsPath.CloseFigure();
					}
					if (m_series.ConfigItems.PieItem.ShowDataBindLabels && m_series.XAxis.LabelsImpl != null)
					{
						string text = m_series.XAxis.LabelsImpl.GetLabelAt(visiblePoints[m].Index).Text;
						Font font = m_series.XAxis.LabelsImpl.GetLabelAt(visiblePoints[m].Index).Font;
						Pseudo3DText pseudo3DText = new Pseudo3DText(text, font, br, new Vector3D(pieLabel.Rectangle.X + pieLabel.Rectangle.Width / 2f, pieLabel.Rectangle.Y + pieLabel.Rectangle.Height / 2f, 0.0), pieLabel.Rectangle);
						pseudo3DText.Alignment = ContentAlignment.MiddleCenter;
						arrayList.Add(pseudo3DText);
					}
					else if (styleAt2.Text != "")
					{
						Pseudo3DText pseudo3DText2 = new Pseudo3DText(styleAt2.Text, styleAt2.GdipFont, br, new Vector3D(pieLabel.Rectangle.X + pieLabel.Rectangle.Width / 2f, pieLabel.Rectangle.Y + pieLabel.Rectangle.Height / 2f, 0.0), pieLabel.Rectangle);
						pseudo3DText2.Alignment = ContentAlignment.MiddleCenter;
						arrayList.Add(pseudo3DText2);
					}
				}
				else
				{
					PointF pointF3 = new PointF(base.Center.X + (float)((double)(0.8f * num4) * Math.Cos(Math.PI * 2.0 * (num9 + maxZero2 / 2.0) / allValue)), base.Center.Y + (float)((double)(0.8f * num4) * Math.Sin(Math.PI * 2.0 * (num9 + maxZero2 / 2.0) / allValue)));
					if (m_series.ConfigItems.PieItem.ShowDataBindLabels && m_series.XAxis.LabelsImpl != null)
					{
						string text2 = m_series.XAxis.LabelsImpl.GetLabelAt(visiblePoints[m].Index).Text;
						Font font2 = m_series.XAxis.LabelsImpl.GetLabelAt(visiblePoints[m].Index).Font;
						Pseudo3DText pseudo3DText3 = new Pseudo3DText(text2, font2, br, new Vector3D(pointF3.X, pointF3.Y, 0.0));
						pseudo3DText3.Alignment = ContentAlignment.MiddleCenter;
						arrayList.Add(pseudo3DText3);
					}
					else if (styleAt2.Text != null)
					{
						Pseudo3DText pseudo3DText4 = new Pseudo3DText(styleAt2.Text, styleAt2.GdipFont, br, new Vector3D(pointF3.X, pointF3.Y, 0.0));
						pseudo3DText4.Alignment = ContentAlignment.MiddleCenter;
						arrayList.Add(pseudo3DText4);
					}
				}
			}
			num9 += maxZero2;
		}
		if (arrayList.Count > 0)
		{
			Path3DCollect path3DCollect = new Path3DCollect((Polygon[])arrayList.ToArray(typeof(Polygon)));
			if (graphicsPath.PointCount > 0)
			{
				path3DCollect.Add(Path3D.FromGraphicsPath(graphicsPath, 0.0, base.SeriesStyle.GdipPen));
			}
			g.AddPolygon(path3DCollect);
		}
	}

	protected internal override void RenderAdornments(Graphics g)
	{
	}

	protected internal override void RenderAdornments(Graphics3D g)
	{
	}

	private int CompareByStartAngle(Pie3DSegment x, Pie3DSegment y)
	{
		if (x != y)
		{
			float cost = GetCost(x);
			float cost2 = GetCost(y);
			return cost.CompareTo(cost2);
		}
		return 0;
	}

	private float GetCost(Pie3DSegment segment)
	{
		if (segment.StartAngle < 90f && segment.EndAngle > 90f)
		{
			return GetCost(90f);
		}
		if (segment.StartAngle < 270f && segment.EndAngle > 270f)
		{
			return GetCost(270f);
		}
		if (segment.StartAngle == segment.EndAngle)
		{
			return Math.Max(GetCost(segment.StartAngle), GetCost(segment.EndAngle + 0.0001f));
		}
		return Math.Max(GetCost(segment.StartAngle), GetCost(segment.EndAngle));
	}

	private float GetCost(float angle)
	{
		if (angle < 90f)
		{
			return 90f + angle;
		}
		if (angle > 270f)
		{
			return angle - 270f;
		}
		return 270f - angle;
	}

	private int CompareByEndAngle(Pie3DSegment x, Pie3DSegment y)
	{
		int num = 0;
		float endAngle = x.EndAngle;
		float endAngle2 = y.EndAngle;
		if (endAngle > 90f && endAngle < 270f)
		{
			if (endAngle2 > 90f && endAngle2 < 270f)
			{
				return (!(endAngle > endAngle2)) ? 1 : (-1);
			}
			return -1;
		}
		if ((endAngle2 > 90f && endAngle2 < 270f) || (endAngle < 90f && endAngle2 > 270f))
		{
			return 1;
		}
		if (endAngle > 270f && endAngle2 < 90f)
		{
			return -1;
		}
		return (!(endAngle < endAngle2)) ? 1 : (-1);
	}

	public override SizeF GetMinSize(Graphics g)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = float.MaxValue;
		float num4 = float.MaxValue;
		_ = m_series.Points.Count;
		ChartStyledPoint[] array = PrepearePoints().Clone() as ChartStyledPoint[];
		if (m_series.OptimizePiePointPositions)
		{
			int num5 = array.Length / 4;
			for (int i = 0; i < num5; i++)
			{
				int num6 = 2 * i + 1;
				ChartStyledPoint chartStyledPoint = array[num6];
				array[num6] = array[array.Length - num6 - 1];
				array[array.Length - num6 - 1] = chartStyledPoint;
			}
		}
		float num7 = 50f;
		PieLabel[] array2 = CreateLabels(array);
		MeasureLabels(array2, new ChartGDIGraph(g), new SizeF(num7, num7));
		for (int j = 0; j < array2.Length; j++)
		{
			num4 = Math.Min(num4, array2[j].Rectangle.Top);
			num3 = Math.Min(num3, array2[j].Rectangle.Left);
			num = Math.Max(num, array2[j].Rectangle.Right);
			num2 = Math.Max(num2, array2[j].Rectangle.Bottom);
		}
		float width = ((base.Chart != null) ? Math.Max(num - num3, base.ChartArea.MinSize.Width) : (num - num3));
		float height = ((base.Chart != null) ? Math.Max(num2 - num4, base.ChartArea.MinSize.Height) : (num2 - num4));
		return new SizeF(width, height);
	}

	private Polygon[][] CreateSector(Vector3D center, float inSideRadius, float outSideRadius, float start, float fov, float depth, Pen pen, BrushInfo brInfo)
	{
		int num = ((!(fov >= 8f)) ? 1 : ((int)(fov / 8f)));
		float num2 = fov / (float)num;
		Polygon[][] array = new Polygon[4][];
		PointF[] array2 = new PointF[num + 1];
		PointF[] array3 = new PointF[num + 1];
		for (int i = 0; i < num + 1; i++)
		{
			float x = (float)(center.X + (double)outSideRadius * Math.Cos((double)(start + (float)i * num2) * (Math.PI / 180.0)));
			float y = (float)(center.Y + (double)outSideRadius * Math.Sin((double)(start + (float)i * num2) * (Math.PI / 180.0)));
			array2[i] = new PointF(x, y);
			float x2 = (float)(center.X + (double)inSideRadius * Math.Cos((double)(start + (float)i * num2) * (Math.PI / 180.0)));
			float y2 = (float)(center.Y + (double)inSideRadius * Math.Sin((double)(start + (float)i * num2) * (Math.PI / 180.0)));
			array3[i] = new PointF(x2, y2);
		}
		Polygon[] array4 = new Polygon[num];
		for (int j = 0; j < num; j++)
		{
			Vector3D[] points = new Vector3D[4]
			{
				new Vector3D(array2[j].X, array2[j].Y, 0.0),
				new Vector3D(array2[j].X, array2[j].Y, depth),
				new Vector3D(array2[j + 1].X, array2[j + 1].Y, depth),
				new Vector3D(array2[j + 1].X, array2[j + 1].Y, 0.0)
			};
			array4[j] = new Polygon(points, brInfo, null);
		}
		array[1] = array4;
		if (inSideRadius > 0f)
		{
			Polygon[] array5 = new Polygon[num];
			for (int k = 0; k < num; k++)
			{
				Vector3D[] points2 = new Vector3D[4]
				{
					new Vector3D(array3[k].X, array3[k].Y, 0.0),
					new Vector3D(array3[k].X, array3[k].Y, depth),
					new Vector3D(array3[k + 1].X, array3[k + 1].Y, depth),
					new Vector3D(array3[k + 1].X, array3[k + 1].Y, 0.0)
				};
				array5[k] = new Polygon(points2, brInfo, null);
			}
			array[3] = array5;
		}
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		for (int l = 0; l < num + 1; l++)
		{
			arrayList.Add(new Vector3D(array2[l].X, array2[l].Y, 0.0));
			arrayList2.Add(new Vector3D(array2[l].X, array2[l].Y, depth));
		}
		if (inSideRadius > 0f)
		{
			for (int num3 = num; num3 > -1; num3--)
			{
				arrayList.Add(new Vector3D(array3[num3].X, array3[num3].Y, 0.0));
				arrayList2.Add(new Vector3D(array3[num3].X, array3[num3].Y, depth));
			}
		}
		else
		{
			arrayList.Add(center);
			arrayList2.Add(new Vector3D(center.X, center.Y, depth));
		}
		if (base.ChartArea.MultiplePies)
		{
			Pen pen2 = new Pen(Color.Transparent, pen.Width);
			array[0] = new Polygon[2]
			{
				new Polygon((Vector3D[])arrayList.ToArray(typeof(Vector3D)), brInfo, pen),
				new Polygon((Vector3D[])arrayList2.ToArray(typeof(Vector3D)), brInfo, pen2)
			};
		}
		else
		{
			array[0] = new Polygon[2]
			{
				new Polygon((Vector3D[])arrayList.ToArray(typeof(Vector3D)), brInfo, pen),
				new Polygon((Vector3D[])arrayList2.ToArray(typeof(Vector3D)), brInfo, pen)
			};
		}
		if (inSideRadius > 0f)
		{
			Vector3D[] points3 = new Vector3D[4]
			{
				new Vector3D(array2[0].X, array2[0].Y, 0.0),
				new Vector3D(array2[0].X, array2[0].Y, depth),
				new Vector3D(array3[0].X, array3[0].Y, depth),
				new Vector3D(array3[0].X, array3[0].Y, 0.0)
			};
			Vector3D[] points4 = new Vector3D[4]
			{
				new Vector3D(array2[num].X, array2[num].Y, 0.0),
				new Vector3D(array2[num].X, array2[num].Y, depth),
				new Vector3D(array3[num].X, array3[num].Y, depth),
				new Vector3D(array3[num].X, array3[num].Y, 0.0)
			};
			array[2] = new Polygon[2]
			{
				new Polygon(points3, brInfo, pen),
				new Polygon(points4, brInfo, pen)
			};
		}
		else
		{
			Vector3D[] points5 = new Vector3D[4]
			{
				new Vector3D(array2[0].X, array2[0].Y, 0.0),
				new Vector3D(array2[0].X, array2[0].Y, depth),
				new Vector3D(center.X, center.Y, depth),
				new Vector3D(center.X, center.Y, 0.0)
			};
			Vector3D[] points6 = new Vector3D[4]
			{
				new Vector3D(array2[num].X, array2[num].Y, 0.0),
				new Vector3D(array2[num].X, array2[num].Y, depth),
				new Vector3D(center.X, center.Y, depth),
				new Vector3D(center.X, center.Y, 0.0)
			};
			array[2] = new Polygon[2]
			{
				new Polygon(points5, brInfo, pen),
				new Polygon(points6, brInfo, pen)
			};
		}
		return array;
	}

	private PieLabel[] CreateLabels(ChartStyledPoint[] points)
	{
		double allValue = GetAllValue();
		double num = Math.PI * 2.0 / allValue;
		double num2 = (double)m_startAngle * allValue / 360.0;
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i < points.Length; i++)
		{
			if (points[i].Point.IsEmpty)
			{
				continue;
			}
			double maxZero = GetMaxZero(points[i].Point.YValues[0]);
			num2 = (num2 + maxZero / 2.0) % allValue;
			PieLabel pieLabel = new PieLabel(points[i], (float)(num * num2), m_series, points[i].Index);
			if (num2 <= 0.25 * allValue)
			{
				pieLabel.Corner = PieSectorCorner.BottomRight;
				if (num2 >= 0.2 * allValue)
				{
					pieLabel.CornerLabel = true;
				}
			}
			else if (num2 <= 0.5 * allValue)
			{
				pieLabel.Corner = PieSectorCorner.BottomLeft;
				if (num2 <= 0.3 * allValue)
				{
					pieLabel.CornerLabel = true;
				}
			}
			else if (num2 <= 0.75 * allValue)
			{
				pieLabel.Corner = PieSectorCorner.TopLeft;
				if (num2 >= 0.7 * allValue)
				{
					pieLabel.CornerLabel = true;
				}
			}
			else
			{
				pieLabel.Corner = PieSectorCorner.TopRight;
				if (num2 <= 0.8 * allValue)
				{
					pieLabel.CornerLabel = true;
				}
			}
			pieLabel.Value = num2;
			arrayList.Add(pieLabel);
			num2 = (num2 + maxZero / 2.0) % allValue;
		}
		return (PieLabel[])arrayList.ToArray(typeof(PieLabel));
	}

	private SizeF MeasureLabels(PieLabel[] labels, ChartGraph g, SizeF radius)
	{
		float val = 0f;
		float num = 0f;
		float val2 = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		foreach (PieLabel pieLabel in labels)
		{
			pieLabel.Measure(g, radius.Width);
			if (pieLabel.Corner == PieSectorCorner.TopLeft || pieLabel.Corner == PieSectorCorner.BottomLeft)
			{
				val = Math.Max(val, pieLabel.Rectangle.Width);
				if (m_series.Styles[pieLabel.LabelIndex].Callout.Enable)
				{
					val2 = Math.Max(val2, pieLabel.Rectangle.Height);
				}
				if (pieLabel.Corner == PieSectorCorner.BottomLeft && pieLabel.CornerLabel)
				{
					num5 += pieLabel.Rectangle.Height;
				}
				if (pieLabel.Corner == PieSectorCorner.TopLeft && pieLabel.CornerLabel)
				{
					num3 += pieLabel.Rectangle.Height;
				}
			}
			else
			{
				num = Math.Max(num, pieLabel.Rectangle.Width);
				if (m_series.Styles[pieLabel.LabelIndex].Callout.Enable)
				{
					num2 = Math.Max(num2, pieLabel.Rectangle.Height);
				}
				if (pieLabel.Corner == PieSectorCorner.TopRight && pieLabel.CornerLabel)
				{
					num4 += pieLabel.Rectangle.Height;
				}
				if (pieLabel.Corner == PieSectorCorner.BottomRight && pieLabel.CornerLabel)
				{
					num6 += pieLabel.Rectangle.Height;
				}
			}
		}
		num7 = Math.Max(Math.Max(num6, num5), Math.Max(num3, num4));
		if (m_series.SmartLabels && radius.Width > radius.Height)
		{
			return new SizeF(radius.Width - Math.Max(val, num), radius.Height - num7);
		}
		return new SizeF(radius.Width - Math.Max(val, num), radius.Height - Math.Max(val2, num2));
	}

	private void ArrangeLabels(PieLabel[] labels, SizeF radius, PointF center, bool inColumn)
	{
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		ArrayList arrayList3 = new ArrayList();
		ArrayList arrayList4 = new ArrayList();
		PieLabelComparer comparer = new PieLabelComparer();
		PieLabel[] array = labels;
		foreach (PieLabel pieLabel in array)
		{
			float num = (float)Math.Cos(pieLabel.Angle);
			float num2 = (float)Math.Sin(pieLabel.Angle);
			pieLabel.SetConnectPoint(new PointF(center.X + radius.Width * num, center.Y + radius.Height * num2));
			if (pieLabel.Corner == PieSectorCorner.TopLeft)
			{
				arrayList.Add(pieLabel);
			}
			if (pieLabel.Corner == PieSectorCorner.TopRight)
			{
				arrayList2.Add(pieLabel);
			}
			if (pieLabel.Corner == PieSectorCorner.BottomLeft)
			{
				arrayList3.Add(pieLabel);
			}
			if (pieLabel.Corner == PieSectorCorner.BottomRight)
			{
				arrayList4.Add(pieLabel);
			}
		}
		arrayList.Sort(comparer);
		arrayList2.Sort(comparer);
		arrayList3.Sort(comparer);
		arrayList4.Sort(comparer);
		float value = center.Y;
		for (int j = 0; j < arrayList.Count; j++)
		{
			PieLabel obj = arrayList[j] as PieLabel;
			obj.CorrectTopLeft(value);
			value = obj.Rectangle.Top;
		}
		value = center.Y;
		for (int num3 = arrayList3.Count - 1; num3 > -1; num3--)
		{
			PieLabel obj2 = arrayList3[num3] as PieLabel;
			obj2.CorrectBottomLeft(value);
			value = obj2.Rectangle.Bottom;
		}
		value = center.Y;
		for (int num4 = arrayList2.Count - 1; num4 > -1; num4--)
		{
			PieLabel obj3 = arrayList2[num4] as PieLabel;
			obj3.CorrectTopRight(value);
			value = obj3.Rectangle.Top;
		}
		value = center.Y;
		int k = 0;
		for (int count = arrayList4.Count; k < count; k++)
		{
			PieLabel obj4 = arrayList4[k] as PieLabel;
			obj4.CorrectBottomRight(value);
			value = obj4.Rectangle.Bottom;
		}
		if (!inColumn)
		{
			return;
		}
		array = labels;
		foreach (PieLabel pieLabel2 in array)
		{
			if (pieLabel2.Corner == PieSectorCorner.TopLeft || pieLabel2.Corner == PieSectorCorner.BottomLeft)
			{
				pieLabel2.AlignRightSide(center.X - radius.Width);
			}
			else
			{
				pieLabel2.AlignLeftSide(center.X + radius.Width);
			}
		}
	}

	private void WrapLabels(ChartGraph g, PieLabel[] labels)
	{
		RectangleF empty = RectangleF.Empty;
		RectangleF bounds = base.Bounds;
		int i = 0;
		for (int num = labels.Length; i < num; i++)
		{
			PieLabel pieLabel = labels[i];
			empty = RectangleF.Intersect(bounds, pieLabel.Rectangle);
			if (!empty.Equals(RectangleF.Empty))
			{
				SizeF sizeF = pieLabel.Measure(g, empty.Width);
				if (pieLabel.Corner == PieSectorCorner.BottomLeft)
				{
					pieLabel.CorrectTopLeft(pieLabel.ConnectPoint.Y + sizeF.Height / 2f);
				}
				else if (pieLabel.Corner == PieSectorCorner.TopLeft)
				{
					pieLabel.CorrectBottomLeft(pieLabel.ConnectPoint.Y - sizeF.Height / 2f);
				}
				if (pieLabel.Corner == PieSectorCorner.BottomRight)
				{
					pieLabel.CorrectBottomRight(pieLabel.ConnectPoint.Y - sizeF.Height / 2f);
				}
				else if (pieLabel.Corner == PieSectorCorner.TopRight)
				{
					pieLabel.CorrectTopRight(pieLabel.ConnectPoint.Y + sizeF.Height / 2f);
				}
			}
		}
	}

	private double GetAllValue()
	{
		double num = 0.0;
		for (int i = 0; i < m_series.Points.Count; i++)
		{
			ChartPoint chartPoint = m_series.Points[i];
			if (IsVisiblePoint(chartPoint))
			{
				num += GetMaxZero(chartPoint.YValues[0]);
			}
		}
		return num;
	}

	private double GetMaxZero(double value)
	{
		return Math.Abs(value);
	}

	private ColorBlend SelectKnow(ChartPieType type)
	{
		ColorBlend result = null;
		switch (type)
		{
		case ChartPieType.InSide:
			result = s_insideGradient;
			break;
		case ChartPieType.OutSide:
			result = s_outsideGradient;
			break;
		case ChartPieType.Round:
			result = s_roundGradient;
			break;
		case ChartPieType.Bevel:
			result = s_bevelGradient;
			break;
		}
		return result;
	}

	public override void DrawIcon(int index, Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(bounds);
		if (isShadow)
		{
			using (SolidBrush brush = new SolidBrush(shadowColor))
			{
				g.FillPath(brush, graphicsPath);
				return;
			}
		}
		BrushPaint.FillPath(g, graphicsPath, GetBrush(index));
		g.DrawPath(base.SeriesStyle.GdipPen, graphicsPath);
	}
}
