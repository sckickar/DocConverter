using System;
using System.Collections;
using System.Collections.Generic;
using SkiaSharp;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;
using DocGen.OfficeChart;

namespace DocGen.Chart.Renderers;

internal class ScatterRenderer : ChartSeriesRenderer
{
	protected override int RequireYValuesCount => 1;

	public ScatterRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		_ = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		if (m_series.ScatterConnectType != 0)
		{
			IndexRange[] array = ((!base.Chart.AllowGapForEmptyPoints) ? ((IndexRange[])new ArrayList { CalculateVisibleRange() }.ToArray(typeof(IndexRange))) : ScatterCalculateUnEmptyRanges(new IndexRange(0, m_series.Points.Count - 1)));
			Graphics graphics = (args.Graph as ChartGDIGraph).Graphics;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].To - array[i].From > 0)
				{
					Render(graphics, array[i].From, array[i].To - array[i].From + 1);
				}
			}
		}
		CalculateVisibleRange();
		PrepearePoints();
	}

	private new ChartStyledPoint[] GetVisiblePoints(ChartStyledPoint[] list)
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

	protected IndexRange[] ScatterCalculateUnEmptyRanges(IndexRange vrange)
	{
		ArrayList arrayList = new ArrayList();
		int num = 0;
		int i = vrange.From;
		for (int num2 = vrange.To + 1; i < num2; i++)
		{
			if (IsVisiblePoint(m_series.Points[i]))
			{
				if (num == -1)
				{
					num = i;
				}
				else if (m_series.Points.Count == i + 1 && IsVisiblePoint(m_series.Points[i]))
				{
					arrayList.Add(new IndexRange(num, i));
				}
			}
			else if (num >= 0)
			{
				arrayList.Add(new IndexRange(num, i - 1));
				num = -1;
			}
		}
		if (arrayList.Count == 0)
		{
			arrayList.Add(vrange);
		}
		return (IndexRange[])arrayList.ToArray(typeof(IndexRange));
	}

	private void Render(Graphics g, int from, int count)
	{
		if (m_series.ScatterConnectType == ScatterConnectType.None)
		{
			return;
		}
		base.Chart.Series.IndexOf(m_series);
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		Pen pen = seriesStyle.GdipPen.Clone() as Pen;
		pen.Color = seriesStyle.Interior.BackColor;
		Pen gdipPen = seriesStyle.GdipPen;
		BrushInfo brush = GetBrush();
		_ = base.Chart.DropSeriesPoints;
		double tension = m_series.ScatterSplineTension;
		if (base.Chart.AllowGapForEmptyPoints)
		{
			base.StyledPoints = PrepearePoints();
		}
		else
		{
			ChartStyledPoint[] visiblePoints = GetVisiblePoints(base.StyledPoints);
			base.StyledPoints = visiblePoints;
			count = base.StyledPoints.Length;
		}
		if (m_series.ScatterConnectType == ScatterConnectType.Line)
		{
			tension = 0.0;
		}
		if ((m_series.ScatterConnectType == ScatterConnectType.Line && !m_series.Style.IsScatterBorderColor) || base.Chart.Series3D || m_series.ScatterConnectType == ScatterConnectType.Spline)
		{
			ChartPointWithIndex[] array = new ChartPointWithIndex[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = new ChartPointWithIndex(base.StyledPoints[from + i].Point, from + i);
			}
			canonicalSpline(array, tension, base.Chart.Series3D, out var bpoints, out var bextrpoints);
			GraphicsPath graphicsPath = new GraphicsPath();
			ArrayList arrayList = new ArrayList(m_series.Points.Count);
			ArrayList arrayList2 = new ArrayList(m_series.Points.Count);
			int j = 0;
			for (int num = bpoints.Length; j < num; j += 4)
			{
				ChartPoint point = bpoints[j].Point;
				ChartPoint point2 = bpoints[j + 1].Point;
				ChartPoint point3 = bpoints[j + 2].Point;
				ChartPoint point4 = bpoints[j + 3].Point;
				PointF pointF = new PointF(GetXFromValue(point, 0), GetYFromValue(point, 0));
				PointF pointF2 = new PointF(GetXFromValue(point2, 0), GetYFromValue(point2, 0));
				PointF pointF3 = new PointF(GetXFromValue(point3, 0), GetYFromValue(point3, 0));
				PointF pointF4 = new PointF(GetXFromValue(point4, 0), GetYFromValue(point4, 0));
				arrayList.Add(pointF);
				arrayList.Add(pointF2);
				arrayList.Add(pointF3);
				arrayList.Add(pointF4);
				graphicsPath.AddBezier(pointF, pointF2, pointF3, pointF4);
			}
			int k = 0;
			for (int num2 = bextrpoints.Length; k < num2; k += 4)
			{
				ChartPoint point5 = bextrpoints[k].Point;
				ChartPoint point6 = bextrpoints[k + 1].Point;
				ChartPoint point7 = bextrpoints[k + 2].Point;
				ChartPoint point8 = bextrpoints[k + 3].Point;
				PointF pointF5 = new PointF(GetXFromValue(point5, 0), GetYFromValue(point5, 0));
				PointF pointF6 = new PointF(GetXFromValue(point6, 0), GetYFromValue(point6, 0));
				PointF pointF7 = new PointF(GetXFromValue(point7, 0), GetYFromValue(point7, 0));
				PointF pointF8 = new PointF(GetXFromValue(point8, 0), GetYFromValue(point8, 0));
				arrayList2.Add(pointF5);
				arrayList2.Add(pointF6);
				arrayList2.Add(pointF7);
				arrayList2.Add(pointF8);
			}
			PointF[] stepPoints = (PointF[])arrayList.ToArray(typeof(PointF));
			PointF[] stepPoints2 = (PointF[])arrayList2.ToArray(typeof(PointF));
			if (seriesStyle.DisplayShadow && !base.Chart.Series3D)
			{
				pen.Color = seriesStyle.ShadowInterior.ForeColor;
				GraphicsPath graphicsPath2 = (GraphicsPath)graphicsPath.Clone();
				graphicsPath2.Transform(new Matrix(1f, 0f, 0f, 1f, seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height));
				g.DrawPath(pen, graphicsPath2);
				pen.Color = seriesStyle.Interior.BackColor;
			}
			if (base.Chart.Series3D)
			{
				CalculateStepPointsForSeries3D(ref stepPoints);
				CalculateStepPointsForSeries3D(ref stepPoints2);
				Draw3DBeziers(g, stepPoints, stepPoints2, GetSeriesOffset(), brush, gdipPen);
			}
			else
			{
				pen.SKPaint.PathEffect = m_series.GetDashStyle(pen, seriesStyle.GdipPen.Width);
				SKStrokeCap capStyle = m_series.GetCapStyle(pen);
				if (capStyle != 0)
				{
					pen.SKPaint.StrokeCap = capStyle;
				}
				g.DrawPath(pen, graphicsPath);
			}
			for (int l = 0; l < count - 1; l++)
			{
				ChartPoint point9 = array[l].Point;
				ChartPoint point10 = array[l + 1].Point;
				PointF pointF9 = new PointF(GetXFromValue(point9, 0), GetYFromValue(point9, 0));
				PointF pointF10 = new PointF(GetXFromValue(point10, 0), GetYFromValue(point10, 0));
				Math.Atan2(GetYFromValue(point10, 0) - GetYFromValue(point9, 0), GetXFromValue(point10, 0) - GetXFromValue(point9, 0));
				float num3 = (float)Math.Atan2(GetYFromValue(bextrpoints[l * 4].Point, 0) - GetYFromValue(bextrpoints[l * 4 + 1].Point, 0), GetXFromValue(bextrpoints[l * 4].Point, 0) - GetXFromValue(bextrpoints[l * 4 + 1].Point, 0));
				Color color = base.StyledPoints[l + 1].Style.Border.Color;
				if (base.StyledPoints[l + 1].BeginArrow != null)
				{
					Arrow beginArrow = base.StyledPoints[l + 1].BeginArrow;
					switch (beginArrow.Type)
					{
					case OfficeArrowType.OvalArrow:
						DrawFilledOval(g, pointF9, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num3 + 90f);
						break;
					case OfficeArrowType.DiamondArrow:
						DrawDiamond(g, pointF9, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num3);
						break;
					case OfficeArrowType.OpenArrow:
					{
						GraphicsState state = g.Save();
						DrawOpenArrow(g, pointF10, pointF9, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num3);
						g.Restore(state);
						break;
					}
					case OfficeArrowType.StealthArrow:
					{
						GraphicsState state = g.Save();
						DrawStealthArrow(g, pointF10, pointF9, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num3);
						g.Restore(state);
						break;
					}
					case OfficeArrowType.Arrow:
					{
						GraphicsState state = g.Save();
						DrawSimpleArrow(g, pointF10, pointF9, beginArrow.ArrowWidth, beginArrow.ArrowLength, color, num3);
						g.Restore(state);
						break;
					}
					}
				}
				num3 = (float)Math.Atan2(GetYFromValue(bextrpoints[l * 4 + 3].Point, 0) - GetYFromValue(bextrpoints[l * 4 + 2].Point, 0), GetXFromValue(bextrpoints[l * 4 + 3].Point, 0) - GetXFromValue(bextrpoints[l * 4 + 2].Point, 0));
				if (base.StyledPoints[l + 1].EndArrow != null)
				{
					Arrow endArrow = base.StyledPoints[l + 1].EndArrow;
					switch (endArrow.Type)
					{
					case OfficeArrowType.OvalArrow:
						num3 *= 180f / (float)Math.PI;
						DrawFilledOval(g, pointF10, endArrow.ArrowWidth, endArrow.ArrowLength, color, num3 + 90f);
						break;
					case OfficeArrowType.OpenArrow:
					{
						GraphicsState state2 = g.Save();
						DrawOpenArrow(g, pointF9, pointF10, endArrow.ArrowWidth, endArrow.ArrowLength, color, num3);
						g.Restore(state2);
						break;
					}
					case OfficeArrowType.DiamondArrow:
						DrawDiamond(g, pointF10, endArrow.ArrowWidth, endArrow.ArrowLength, color, num3);
						break;
					case OfficeArrowType.StealthArrow:
					{
						GraphicsState state2 = g.Save();
						DrawStealthArrow(g, pointF9, pointF10, endArrow.ArrowWidth, endArrow.ArrowLength, color, num3);
						g.Restore(state2);
						break;
					}
					case OfficeArrowType.Arrow:
					{
						GraphicsState state2 = g.Save();
						DrawSimpleArrow(g, pointF9, pointF10, endArrow.ArrowWidth, endArrow.ArrowLength, color, num3);
						g.Restore(state2);
						break;
					}
					}
				}
			}
		}
		else
		{
			if (m_series.ScatterConnectType != ScatterConnectType.Line)
			{
				return;
			}
			for (int m = 0; m < count - 1; m++)
			{
				ChartPointWithIndex[] array2 = new ChartPointWithIndex[2];
				pen.Color = base.StyledPoints[from + m + 1].Style.Border.Color;
				pen.Width = base.StyledPoints[from + m + 1].Style.Border.Width;
				pen.DashStyle = base.StyledPoints[from + m + 1].Style.Border.DashStyle;
				pen.DashCap = base.StyledPoints[from + m + 1].Style.Border.DashCap;
				int num4 = 0;
				for (int n = m; n <= m + 1; n++)
				{
					array2[num4] = new ChartPointWithIndex(base.StyledPoints[from + n].Point, from + n);
					num4++;
				}
				canonicalSpline(array2, tension, base.Chart.Series3D, out var bpoints2, out var bextrpoints2);
				GraphicsPath graphicsPath3 = new GraphicsPath();
				ArrayList arrayList3 = new ArrayList(2);
				ArrayList arrayList4 = new ArrayList(2);
				int num5 = 0;
				for (int num6 = bpoints2.Length; num5 < num6; num5 += 4)
				{
					ChartPoint point11 = bpoints2[num5].Point;
					ChartPoint point12 = bpoints2[num5 + 1].Point;
					ChartPoint point13 = bpoints2[num5 + 2].Point;
					ChartPoint point14 = bpoints2[num5 + 3].Point;
					PointF pointF11 = new PointF(GetXFromValue(point11, 0), GetYFromValue(point11, 0));
					PointF pointF12 = new PointF(GetXFromValue(point12, 0), GetYFromValue(point12, 0));
					PointF pointF13 = new PointF(GetXFromValue(point13, 0), GetYFromValue(point13, 0));
					PointF pointF14 = new PointF(GetXFromValue(point14, 0), GetYFromValue(point14, 0));
					arrayList3.Add(pointF11);
					arrayList3.Add(pointF12);
					arrayList3.Add(pointF13);
					arrayList3.Add(pointF14);
					float num7 = 2f;
					float num8 = (float)Math.Sqrt(Math.Pow(pointF13.X - pointF12.X, 2.0) + Math.Pow(pointF13.Y - pointF12.Y, 2.0));
					float num9 = (pointF13.X - pointF12.X) / num8;
					float num10 = (pointF13.Y - pointF12.Y) / num8;
					float num11 = num8 - num7;
					PointF pointF15 = new PointF(pointF12.X + num9 * num11, pointF12.Y + num10 * num11);
					PointF pointF16 = new PointF(pointF12.X + num9 * (num8 - num11), pointF12.Y + num10 * (num8 - num11));
					if (base.StyledPoints[m + 1].BeginArrow != null)
					{
						OfficeArrowType type = base.StyledPoints[m + 1].BeginArrow.Type;
						if (type == OfficeArrowType.Arrow || type == OfficeArrowType.OpenArrow || type == OfficeArrowType.StealthArrow)
						{
							pointF11 = pointF16;
							pointF12 = pointF16;
						}
					}
					if (base.StyledPoints[m + 1].EndArrow != null)
					{
						OfficeArrowType type2 = base.StyledPoints[m + 1].EndArrow.Type;
						if (type2 == OfficeArrowType.Arrow || type2 == OfficeArrowType.OpenArrow || type2 == OfficeArrowType.StealthArrow)
						{
							pointF13 = pointF15;
							pointF14 = pointF15;
						}
					}
					graphicsPath3.AddBezier(pointF11, pointF12, pointF13, pointF14);
				}
				int num12 = 0;
				for (int num13 = bextrpoints2.Length; num12 < num13; num12 += 4)
				{
					ChartPoint point15 = bextrpoints2[num12].Point;
					ChartPoint point16 = bextrpoints2[num12 + 1].Point;
					ChartPoint point17 = bextrpoints2[num12 + 2].Point;
					ChartPoint point18 = bextrpoints2[num12 + 3].Point;
					PointF pointF17 = new PointF(GetXFromValue(point15, 0), GetYFromValue(point15, 0));
					PointF pointF18 = new PointF(GetXFromValue(point16, 0), GetYFromValue(point16, 0));
					PointF pointF19 = new PointF(GetXFromValue(point17, 0), GetYFromValue(point17, 0));
					PointF pointF20 = new PointF(GetXFromValue(point18, 0), GetYFromValue(point18, 0));
					arrayList4.Add(pointF17);
					arrayList4.Add(pointF18);
					arrayList4.Add(pointF19);
					arrayList4.Add(pointF20);
				}
				_ = (PointF[])arrayList3.ToArray(typeof(PointF));
				_ = (PointF[])arrayList4.ToArray(typeof(PointF));
				if (seriesStyle.DisplayShadow && !base.Chart.Series3D)
				{
					pen.Color = seriesStyle.ShadowInterior.ForeColor;
					GraphicsPath graphicsPath4 = (GraphicsPath)graphicsPath3.Clone();
					graphicsPath4.Transform(new Matrix(1f, 0f, 0f, 1f, seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height));
					g.DrawPath(pen, graphicsPath4);
					pen.Color = seriesStyle.Interior.BackColor;
				}
				pen.SKPaint.PathEffect = m_series.GetDashStyle(pen, base.StyledPoints[from + m + 1].Style.GdipPen.Width);
				SKStrokeCap capStyle = m_series.GetCapStyle(pen);
				if (capStyle != 0)
				{
					pen.SKPaint.StrokeCap = capStyle;
				}
				g.DrawPath(pen, graphicsPath3);
				DrawArrows(g);
			}
		}
	}

	public override void Render(Graphics3D g)
	{
		IndexRange[] unEmptyRanges = base.UnEmptyRanges;
		for (int i = 0; i < unEmptyRanges.Length; i++)
		{
			if (unEmptyRanges[i].To - unEmptyRanges[i].From > 0)
			{
				Render(g, unEmptyRanges[i].From, unEmptyRanges[i].To - unEmptyRanges[i].From + 1);
			}
		}
		IndexRange indexRange = CalculateVisibleRange();
		ChartStyledPoint[] array = PrepearePoints();
		for (int j = indexRange.From; j <= indexRange.To; j++)
		{
			ChartStyledPoint chartStyledPoint = array[j];
			ChartStyleInfo style = chartStyledPoint.Style;
			if (chartStyledPoint.IsVisible && chartStyledPoint.Style.Symbol.Shape == ChartSymbolShape.None)
			{
				GraphicsPath graphicsPath = new GraphicsPath();
				Vector3D symbolVector = GetSymbolVector(chartStyledPoint);
				Size size = style.Symbol.Size;
				graphicsPath.AddEllipse((float)(symbolVector.X - (double)(size.Width / 2)), (float)(symbolVector.Y - (double)(size.Height / 2)), size.Width, size.Height);
				Path3D polygon = Path3D.FromGraphicsPath(graphicsPath, symbolVector.Z, GetBrush(chartStyledPoint.Index), style.Symbol.Border.GdipPen);
				g.AddPolygon(polygon);
			}
		}
	}

	protected override void RenderAdornment(Graphics g, ChartStyledPoint point)
	{
		RenderErrorBar(g, point);
		base.RenderAdornment(g, point);
	}

	private void Render(Graphics3D g, int from, int count)
	{
		if (m_series.ScatterConnectType == ScatterConnectType.None)
		{
			return;
		}
		base.Chart.Series.IndexOf(m_series);
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		(seriesStyle.GdipPen.Clone() as Pen).Color = seriesStyle.Interior.BackColor;
		_ = seriesStyle.GdipPen;
		GetBrush();
		int sPLINE_DIGITIZATION = SPLINE_DIGITIZATION;
		float placeDepth = GetPlaceDepth();
		float seriesDepth = GetSeriesDepth();
		float num = placeDepth + seriesDepth;
		_ = base.Chart.DropSeriesPoints;
		double tension = m_series.ScatterSplineTension;
		base.StyledPoints = PrepearePoints();
		if (m_series.ScatterConnectType == ScatterConnectType.Line)
		{
			tension = 0.0;
		}
		ChartPointWithIndex[] array = new ChartPointWithIndex[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = new ChartPointWithIndex(base.StyledPoints[from + i].Point, from + i);
		}
		canonicalSpline(array, tension, base.Chart.Series3D, out var _, out var bextrpoints);
		for (int j = 0; j < bextrpoints.Length - 3; j += 4)
		{
			ChartPoint point = bextrpoints[j].Point;
			ChartPoint point2 = bextrpoints[j + 1].Point;
			ChartPoint point3 = bextrpoints[j + 2].Point;
			ChartPoint point4 = bextrpoints[j + 3].Point;
			PointF p = new PointF(GetXFromValue(point, 0), GetYFromValue(point, 0));
			PointF[] array2 = ChartMath.InterpolateBezier(p2: new PointF(GetXFromValue(point2, 0), GetYFromValue(point2, 0)), p3: new PointF(GetXFromValue(point3, 0), GetYFromValue(point3, 0)), p4: new PointF(GetXFromValue(point4, 0), GetYFromValue(point4, 0)), p1: p, count: sPLINE_DIGITIZATION);
			Vector3D vector3D = new Vector3D(p.X, p.Y, placeDepth);
			Vector3D vector3D2 = new Vector3D(p.X, p.Y, num);
			Pen pen = new Pen(base.SeriesStyle.Interior.BackColor);
			for (int k = 0; k < array2.Length; k++)
			{
				Vector3D vector3D3 = new Vector3D(array2[k].X, array2[k].Y, num);
				Vector3D vector3D4 = new Vector3D(array2[k].X, array2[k].Y, placeDepth);
				Polygon polygon = new Polygon(new Vector3D[4] { vector3D, vector3D2, vector3D3, vector3D4 }, GetBrush(), pen);
				g.AddPolygon(polygon);
				vector3D = vector3D4;
				vector3D2 = vector3D3;
			}
		}
	}

	protected override BrushInfo GetBrush()
	{
		BrushInfo brushInfo = base.GetBrush();
		ChartColumnConfigItem columnItem = m_series.ConfigItems.ColumnItem;
		if (base.Chart.Model.ColorModel.AllowGradient && columnItem.ShadingMode == ChartColumnShadingMode.PhongCylinder)
		{
			brushInfo = GetPhongInterior(brushInfo, columnItem.LightColor, columnItem.LightAngle, columnItem.PhongAlpha);
		}
		return brushInfo;
	}

	protected override BrushInfo GetBrush(int index)
	{
		BrushInfo brushInfo = base.GetBrush(index);
		ChartColumnConfigItem columnItem = m_series.ConfigItems.ColumnItem;
		if (base.Chart.Model.ColorModel.AllowGradient && columnItem.ShadingMode == ChartColumnShadingMode.PhongCylinder)
		{
			brushInfo = GetPhongInterior(brushInfo, columnItem.LightColor, columnItem.LightAngle, columnItem.PhongAlpha);
		}
		return brushInfo;
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		if (isShadow)
		{
			using (SolidBrush brush = new SolidBrush(shadowColor))
			{
				g.FillRectangle(brush, bounds);
				return;
			}
		}
		BrushInfo brushInfo = new BrushInfo(m_series.Style.Symbol.Color);
		ChartColumnConfigItem columnItem = m_series.ConfigItems.ColumnItem;
		GraphicsPath graphicsPath = new GraphicsPath();
		Pen pen = null;
		Pen pen2 = null;
		bool flag = false;
		if (base.Chart.Model.ColorModel.AllowGradient && columnItem.ShadingMode == ChartColumnShadingMode.PhongCylinder)
		{
			brushInfo = GetPhongInterior(brushInfo, columnItem.LightColor, columnItem.LightAngle, columnItem.PhongAlpha);
		}
		if (m_series.ScatterConnectType == ScatterConnectType.Line || m_series.ScatterConnectType == ScatterConnectType.Spline)
		{
			graphicsPath = new GraphicsPath();
			int num = bounds.Top + bounds.Height / 2;
			graphicsPath.AddLine(bounds.Left - 2, num, bounds.Right + 2, num);
			graphicsPath.CloseFigure();
			using (pen2 = base.SeriesStyle.GdipPen.Clone() as Pen)
			{
				pen2.Color = (isShadow ? shadowColor : base.SeriesStyle.Interior.BackColor);
				pen2.Width = m_serStyle.Border.Width;
				g.DrawPath(pen2, graphicsPath);
				graphicsPath.Reset();
				flag = true;
			}
			if (!flag)
			{
				using (pen = new Pen(brushInfo.BackColor))
				{
					pen.Width = m_serStyle.Border.Width;
					g.DrawPath(pen, graphicsPath);
				}
			}
			graphicsPath.Reset();
		}
		if (m_series.Style.HasSymbol && m_series.Style.Symbol.Shape != 0)
		{
			brushInfo = new BrushInfo(m_series.Style.Symbol.Color);
			pen = new Pen(m_series.Style.Symbol.Border.Color);
			graphicsPath.AddPath(ChartSymbolHelper.GetPathSymbol(base.SeriesStyle.Symbol.Shape, bounds), connect: false);
			graphicsPath.CloseFigure();
			BrushPaint.FillPath(g, graphicsPath, brushInfo);
			g.DrawPath(pen, graphicsPath);
			graphicsPath.Reset();
		}
	}
}
