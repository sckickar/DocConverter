using System;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.OfficeChart;

namespace DocGen.Chart.Renderers;

internal class SplineRenderer : ChartSeriesRenderer
{
	protected override int RequireYValuesCount => 1;

	protected override string RegionDescription => "Spline Chart Region";

	protected override bool ShouldSort => m_series.SortPoints;

	public SplineRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		ChartStyledPoint[] array = PrepearePoints();
		ChartStyledPoint[] visiblePoints = GetVisiblePoints(array);
		RectangleF clipBounds = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		if (array.Length <= 1)
		{
			return;
		}
		IndexRange indexRange = CalculateVisibleRange();
		_ = args.Series.ConfigItems.LineSegment;
		NaturalSpline(visiblePoints, out var ys);
		int num = m_series.PointFormats[ChartYValueUsage.YValue];
		int num2 = (args.ActualXAxis.Inversed ? indexRange.To : indexRange.From);
		int num3 = (args.ActualXAxis.Inversed ? (indexRange.From - 1) : (indexRange.To + 1));
		int num4 = ((!args.ActualXAxis.Inversed) ? 1 : (-1));
		ChartStyledPoint chartStyledPoint = null;
		ChartStyledPoint chartStyledPoint2 = null;
		for (int i = num2; i != num3; i += num4)
		{
			chartStyledPoint2 = array[i];
			if (chartStyledPoint2.IsVisible)
			{
				if (chartStyledPoint != null)
				{
					ChartPoint controlPoint = null;
					ChartPoint controlPoint2 = null;
					int num5 = Array.IndexOf(visiblePoints, chartStyledPoint2);
					GetBezierControlPoints(chartStyledPoint, chartStyledPoint2, ys[num5 - num4], ys[num5], out controlPoint, out controlPoint2, num);
					DoubleRange xRange = new DoubleRange(chartStyledPoint.X, chartStyledPoint2.X);
					DoubleRange yRange = new DoubleRange(chartStyledPoint.YValues[num], chartStyledPoint2.YValues[num]);
					if (args.IsVisible(xRange, yRange))
					{
						PointF pe = args.GetPoint(chartStyledPoint.X, chartStyledPoint.YValues[num]);
						PointF pe2 = args.GetPoint(chartStyledPoint2.X, chartStyledPoint2.YValues[num]);
						PointF pe3 = args.GetPoint(controlPoint.X, controlPoint.YValues[0]);
						PointF pe4 = args.GetPoint(controlPoint2.X, controlPoint2.YValues[0]);
						BrushInfo brush = GetBrush(chartStyledPoint.Index);
						Pen gdipPen = chartStyledPoint.Style.GdipPen;
						if (args.Is3D)
						{
							Region region = new Region(RectangleF.Empty);
							args.Graph.DrawLine(gdipPen, pe.X, pe.Y, pe.X + args.DepthOffset.Width, pe.Y + args.DepthOffset.Height);
							if (ComputeExtremums(chartStyledPoint, chartStyledPoint2, controlPoint, controlPoint2, out var intelator, out var intelator2, num))
							{
								if (!double.IsNaN(intelator))
								{
									ChartMath.SplitBezierCurve(pe, pe3, pe4, pe2, (float)intelator, out var pb, out var pb2, out var pb3, out var pb4, out pe, out pe3, out pe4, out pe2);
									region.Union(DrawBezier(args, gdipPen, brush, pb, pb2, pb3, pb4));
									region.Intersect(clipBounds);
									using Pen pen = new Pen(brush.BackColor);
									args.Graph.DrawLine(pen, pb4.X, pb4.Y, pb4.X + args.DepthOffset.Width, pb4.Y + args.DepthOffset.Height);
								}
								if (!double.IsNaN(intelator2))
								{
									ChartMath.SplitBezierCurve(pe, pe3, pe4, pe2, (float)intelator2, out var pb5, out var pb6, out var pb7, out var pb8, out pe, out pe3, out pe4, out pe2);
									region.Union(DrawBezier(args, gdipPen, brush, pb5, pb6, pb7, pb8));
									region.Intersect(clipBounds);
									using Pen pen2 = new Pen(brush.BackColor);
									args.Graph.DrawLine(pen2, pb8.X, pb8.Y, pb8.X + args.DepthOffset.Width, pb8.Y + args.DepthOffset.Height);
								}
							}
							region.Union(DrawBezier(args, gdipPen, brush, pe, pe3, pe4, pe2));
							region.Intersect(clipBounds);
							args.Graph.DrawLine(gdipPen, pe2.X, pe2.Y, pe2.X + args.DepthOffset.Width, pe2.Y + args.DepthOffset.Height);
						}
						else
						{
							using Pen pen3 = gdipPen.Clone() as Pen;
							GraphicsPath graphicsPath = new GraphicsPath();
							graphicsPath.AddBezier(pe, pe3, pe4, pe2);
							DrawArrows((args.Graph as ChartGDIGraph).Graphics, pe, pe3, pe4, pe2, i - 1);
							if (chartStyledPoint.Style.DisplayShadow)
							{
								Size shadowOffset = chartStyledPoint.Style.ShadowOffset;
								pen3.Color = chartStyledPoint.Style.ShadowInterior.BackColor;
								args.Graph.PushTransform();
								args.Graph.Transform = new Matrix(1f, 0f, 0f, 1f, shadowOffset.Width, shadowOffset.Height);
								args.Graph.DrawPath(pen3, graphicsPath);
								args.Graph.PopTransform();
							}
							pen3.Color = GetBrush(chartStyledPoint.Index).BackColor;
							if (!(Math.Abs(pe.X - pe2.X) > 1f) && !(Math.Abs(pe.Y - pe2.Y) > 1f))
							{
								pen3.Width = 1f;
							}
							args.Graph.DrawPath(pen3, graphicsPath);
						}
					}
				}
				chartStyledPoint = chartStyledPoint2;
			}
			else if (base.Chart.AllowGapForEmptyPoints)
			{
				chartStyledPoint = null;
			}
		}
	}

	protected override void RenderAdornment(Graphics g, ChartStyledPoint point)
	{
		RenderErrorBar(g, point);
		base.RenderAdornment(g, point);
	}

	private void DrawArrows(Graphics g, PointF pf0, PointF p0, PointF p1, PointF pf1, int i)
	{
		Math.Atan2(pf1.Y - pf0.Y, pf1.X - pf0.X);
		float num = (float)Math.Atan2(pf0.Y - p0.Y, pf0.X - p0.X);
		Color color = base.StyledPoints[i + 1].Style.Border.Color;
		if (base.StyledPoints[i + 1].BeginArrow != null)
		{
			Arrow beginArrow = base.StyledPoints[i + 1].BeginArrow;
			switch (beginArrow.Type)
			{
			case OfficeArrowType.OvalArrow:
				num *= 180f / (float)Math.PI;
				DrawFilledOval(g, pf0, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num);
				break;
			case OfficeArrowType.DiamondArrow:
				DrawDiamond(g, pf0, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num);
				break;
			case OfficeArrowType.OpenArrow:
			{
				GraphicsState state = g.Save();
				DrawOpenArrow(g, pf1, pf0, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num);
				g.Restore(state);
				break;
			}
			case OfficeArrowType.StealthArrow:
			{
				GraphicsState state = g.Save();
				DrawStealthArrow(g, pf1, pf0, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num);
				g.Restore(state);
				break;
			}
			case OfficeArrowType.Arrow:
			{
				GraphicsState state = g.Save();
				DrawSimpleArrow(g, pf1, pf0, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num);
				g.Restore(state);
				break;
			}
			}
		}
		num = (float)Math.Atan2(pf1.Y - p1.Y, pf1.X - p1.X);
		if (base.StyledPoints[i + 1].EndArrow != null)
		{
			Arrow endArrow = base.StyledPoints[i + 1].EndArrow;
			switch (endArrow.Type)
			{
			case OfficeArrowType.OvalArrow:
				num *= 180f / (float)Math.PI;
				DrawFilledOval(g, pf1, endArrow.ArrowWidth, endArrow.ArrowLength, color, num + 90f);
				break;
			case OfficeArrowType.OpenArrow:
			{
				GraphicsState state2 = g.Save();
				DrawOpenArrow(g, pf0, pf1, endArrow.ArrowWidth, endArrow.ArrowLength, color, num);
				g.Restore(state2);
				break;
			}
			case OfficeArrowType.DiamondArrow:
				DrawDiamond(g, pf1, endArrow.ArrowWidth, endArrow.ArrowLength, color, num);
				break;
			case OfficeArrowType.StealthArrow:
			{
				GraphicsState state2 = g.Save();
				DrawStealthArrow(g, pf0, pf1, endArrow.ArrowWidth, endArrow.ArrowLength, color, num);
				g.Restore(state2);
				break;
			}
			case OfficeArrowType.Arrow:
			{
				GraphicsState state2 = g.Save();
				DrawSimpleArrow(g, pf0, pf1, endArrow.ArrowWidth, endArrow.ArrowLength, color, num);
				g.Restore(state2);
				break;
			}
			}
		}
	}

	public override void Render(ChartRenderArgs3D args)
	{
		ChartStyledPoint[] array = PrepearePoints();
		ChartStyledPoint[] visiblePoints = GetVisiblePoints(array);
		if (array.Length <= 1)
		{
			return;
		}
		IndexRange indexRange = CalculateVisibleRange();
		_ = args.Series.ConfigItems.LineSegment;
		NaturalSpline(visiblePoints, out var ys);
		int num = m_series.PointFormats[ChartYValueUsage.YValue];
		int num2 = (args.ActualXAxis.Inversed ? indexRange.To : indexRange.From);
		int num3 = (args.ActualXAxis.Inversed ? (indexRange.From - 1) : (indexRange.To + 1));
		int num4 = ((!args.ActualXAxis.Inversed) ? 1 : (-1));
		ChartStyledPoint chartStyledPoint = null;
		ChartStyledPoint chartStyledPoint2 = null;
		for (int i = num2; i != num3; i += num4)
		{
			chartStyledPoint2 = array[i];
			if (chartStyledPoint2.IsVisible)
			{
				if (chartStyledPoint != null)
				{
					ChartPoint controlPoint = null;
					ChartPoint controlPoint2 = null;
					int num5 = Array.IndexOf(visiblePoints, chartStyledPoint2);
					GetBezierControlPoints(chartStyledPoint, chartStyledPoint2, ys[num5 - num4], ys[num5], out controlPoint, out controlPoint2, num);
					DoubleRange xRange = new DoubleRange(chartStyledPoint.X, chartStyledPoint2.X);
					DoubleRange yRange = new DoubleRange(chartStyledPoint.YValues[num], chartStyledPoint2.YValues[num]);
					if (args.IsVisible(xRange, yRange))
					{
						PointF point = args.GetPoint(chartStyledPoint.X, chartStyledPoint.YValues[num]);
						PointF point2 = args.GetPoint(chartStyledPoint2.X, chartStyledPoint2.YValues[num]);
						PointF point3 = args.GetPoint(controlPoint.X, controlPoint.YValues[0]);
						PointF point4 = args.GetPoint(controlPoint2.X, controlPoint2.YValues[0]);
						BrushInfo brush = GetBrush(chartStyledPoint.Index);
						_ = chartStyledPoint.Style.GdipPen;
						PointF[] array2 = ChartMath.InterpolateBezier(point, point3, point4, point2, SPLINE_DIGITIZATION);
						Vector3D vector3D = new Vector3D(point.X, point.Y, args.Z);
						Vector3D vector3D2 = new Vector3D(point.X, point.Y, args.Z + args.Depth);
						Pen pen = new Pen(brush.BackColor);
						for (int j = 0; j < array2.Length; j++)
						{
							Vector3D vector3D3 = new Vector3D(array2[j].X, array2[j].Y, args.Z);
							Vector3D vector3D4 = new Vector3D(array2[j].X, array2[j].Y, args.Z + args.Depth);
							Polygon polygon = new Polygon(new Vector3D[4] { vector3D, vector3D2, vector3D3, vector3D4 }, brush, pen);
							args.Graph.AddPolygon(polygon);
							vector3D = vector3D4;
							vector3D2 = vector3D3;
						}
					}
				}
				chartStyledPoint = chartStyledPoint2;
			}
			else if (base.Chart.AllowGapForEmptyPoints)
			{
				chartStyledPoint = null;
			}
		}
	}

	private GraphicsPath DrawBezier(ChartRenderArgs2D args, Pen pen, BrushInfo interior, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		GraphicsPath graphicsPath2 = new GraphicsPath();
		GraphicsPath graphicsPath3 = new GraphicsPath();
		graphicsPath.AddBezier(pt1, pt2, pt3, pt4);
		graphicsPath3.AddBezier(pt1, pt2, pt3, pt4);
		pt1 = ChartMath.AddPoint(pt1, args.DepthOffset);
		pt2 = ChartMath.AddPoint(pt2, args.DepthOffset);
		pt3 = ChartMath.AddPoint(pt3, args.DepthOffset);
		pt4 = ChartMath.AddPoint(pt4, args.DepthOffset);
		graphicsPath2.AddBezier(pt4, pt3, pt2, pt1);
		graphicsPath3.AddBezier(pt4, pt3, pt2, pt1);
		graphicsPath3.CloseFigure();
		args.Graph.DrawPath(interior, pen, graphicsPath3);
		return graphicsPath3;
	}

	private bool ComputeExtremums(ChartStyledPoint point1, ChartStyledPoint point2, ChartPoint controlPoint1, ChartPoint controlPoint2, out double intelator1, out double intelator2, int yIndex)
	{
		double x = point1.X;
		double x2 = controlPoint1.X;
		double x3 = controlPoint2.X;
		double x4 = point2.X;
		double num = point1.YValues[yIndex];
		double num2 = controlPoint1.YValues[0];
		double num3 = controlPoint2.YValues[0];
		double num4 = point2.YValues[yIndex];
		double num5 = 3.0 * (x2 - x);
		double num6 = 3.0 * (num2 - num);
		double num7 = 3.0 * (x3 - x2) - num5;
		double num8 = 3.0 * (num3 - num2) - num6;
		double num9 = num4 - num - num8 - num6;
		intelator1 = double.NaN;
		intelator2 = double.NaN;
		if (ChartMath.SolveQuadraticEquation(3.0 * num9, 2.0 * num8, num6, out var root, out var root2))
		{
			bool flag = root > 0.0 && root < 1.0;
			bool flag2 = root2 > 0.0 && root2 < 1.0;
			if (flag && flag2)
			{
				intelator1 = Math.Min(root, root2);
				intelator2 = Math.Max(root, root2);
			}
			else if (flag)
			{
				intelator1 = root;
			}
			else if (flag2)
			{
				intelator1 = root2;
			}
			return true;
		}
		return false;
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddLine(bounds.Left + bounds.Width / 8, (float)((double)bounds.Y + Math.Ceiling((double)bounds.Height / 2.0)), bounds.Right, (float)((double)bounds.Y + Math.Ceiling((double)bounds.Height / 2.0)));
		using Pen pen = base.SeriesStyle.GdipPen.Clone() as Pen;
		pen.Color = (isShadow ? shadowColor : base.SeriesStyle.Interior.BackColor);
		g.DrawPath(pen, graphicsPath);
	}
}
