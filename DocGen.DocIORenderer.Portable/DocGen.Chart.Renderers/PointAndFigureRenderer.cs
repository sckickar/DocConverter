using System;
using System.Collections;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class PointAndFigureRenderer : ChartSeriesRenderer
{
	private class PNFColumn
	{
		public double X;

		public double Width;

		public double Low;

		public double High;

		public bool IsPoint;

		public int indexX;
	}

	protected override string RegionDescription => "PointAndFigure Chart Region";

	protected override int RequireYValuesCount => 2;

	public PointAndFigureRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		PNFColumn[] array = ComputeRectangles(m_series);
		_ = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		if (array == null)
		{
			return;
		}
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		Pen pen = seriesStyle.GdipPen.Clone() as Pen;
		Pen pen2 = seriesStyle.GdipPen.Clone() as Pen;
		Pen pen3 = seriesStyle.GdipPen.Clone() as Pen;
		pen.Color = m_series.ConfigItems.FinancialItem.PriceUpColor;
		pen2.Color = m_series.ConfigItems.FinancialItem.PriceDownColor;
		pen3.Color = seriesStyle.ShadowInterior.BackColor;
		int i = 0;
		int num = array.Length;
		int num2 = 1;
		if (args.ActualXAxis.Inversed)
		{
			i = num - num2;
			num = -1;
			num2 = -1;
		}
		for (; i != num; i += num2)
		{
			PNFColumn pNFColumn = array[i];
			PointF point = args.GetPoint(pNFColumn.X, pNFColumn.Low);
			PointF point2 = args.GetPoint(pNFColumn.X + pNFColumn.Width, pNFColumn.High);
			RectangleF rectF = ChartMath.CorrectRect(point.X, point.Y, point2.X, point2.Y);
			if (args.Is3D)
			{
				GraphicsPath path = CreateBox(rectF, is3D: true);
				args.Graph.DrawPath(GetBrush(), seriesStyle.GdipPen, path);
			}
			int num3 = (int)((pNFColumn.High - pNFColumn.Low) / m_series.HeightBox + 0.5);
			for (int j = 0; j < num3; j++)
			{
				PointF point3 = args.GetPoint(pNFColumn.X, pNFColumn.Low + (double)j * m_series.HeightBox);
				PointF point4 = args.GetPoint(pNFColumn.X + pNFColumn.Width, pNFColumn.Low + (double)(j + 1) * m_series.HeightBox);
				RectangleF rectangleF = ChartMath.CorrectRect(point3.X, point3.Y, point4.X, point4.Y);
				if (seriesStyle.DisplayShadow && !base.Chart.Series3D)
				{
					RectangleF rect = rectangleF;
					rect.Offset(seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height);
					if (!pNFColumn.IsPoint)
					{
						DrawFigureX(args.Graph, pen3, rect);
					}
					else
					{
						DrawPointO(args.Graph, pen3, rect);
					}
				}
				if (!pNFColumn.IsPoint)
				{
					DrawFigureX(args.Graph, pen, rectangleF);
				}
				else
				{
					DrawPointO(args.Graph, pen2, rectangleF);
				}
			}
		}
		pen.Dispose();
		pen2.Dispose();
		pen3.Dispose();
	}

	public override void Render(ChartRenderArgs3D args)
	{
		PNFColumn[] array = ComputeRectangles(m_series);
		if (array == null)
		{
			return;
		}
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		Pen pen = seriesStyle.GdipPen.Clone() as Pen;
		Pen pen2 = seriesStyle.GdipPen.Clone() as Pen;
		pen.Color = m_series.ConfigItems.FinancialItem.PriceUpColor;
		pen2.Color = m_series.ConfigItems.FinancialItem.PriceDownColor;
		args.Graph.AddPolygon(CreateBoundsPolygon((float)args.Z));
		foreach (PNFColumn pNFColumn in array)
		{
			PointF point = args.GetPoint(pNFColumn.X, pNFColumn.Low);
			PointF point2 = args.GetPoint(pNFColumn.X + pNFColumn.Width, pNFColumn.High);
			RectangleF rectangleF = ChartMath.CorrectRect(point.X, point.Y, point2.X, point2.Y);
			args.Graph.CreateBoxV(new Vector3D(rectangleF.Left, rectangleF.Top, args.Z), new Vector3D(rectangleF.Right, rectangleF.Bottom, args.Z + args.Depth), seriesStyle.GdipPen, seriesStyle.Interior);
			int num = (int)((pNFColumn.High - pNFColumn.Low) / m_series.HeightBox + 0.5);
			float num2 = rectangleF.Top;
			for (int j = 0; j < num; j++)
			{
				if (pNFColumn.IsPoint)
				{
					args.Graph.CreateEllipse(new Vector3D(rectangleF.Left, num2, args.Z), new SizeF(rectangleF.Width, rectangleF.Height / (float)num), 25, pen2, null);
				}
				else
				{
					args.Graph.CreateRectangle(new Vector3D(rectangleF.Left, num2, args.Z), new SizeF(rectangleF.Width, rectangleF.Height / (float)num), pen, null, IsPNF: true);
				}
				num2 += rectangleF.Height / (float)num;
			}
		}
	}

	private void DrawPointO(ChartGraph g, Pen pen, RectangleF rect)
	{
		PointF pointF = new PointF(rect.X + pen.Width, rect.Y + pen.Width);
		SizeF sizeF = new SizeF(rect.Width - 2f * pen.Width, rect.Height - 2f * pen.Width);
		g.DrawEllipse((Brush)null, pen, pointF.X, pointF.Y, sizeF.Width, sizeF.Height);
	}

	private void DrawFigureX(ChartGraph g, Pen pen, RectangleF rect)
	{
		PointF pointF = new PointF(rect.X + pen.Width, rect.Y + pen.Width);
		PointF pointF2 = new PointF(rect.X + pen.Width, rect.Y + rect.Height - pen.Width);
		PointF pointF3 = new PointF(rect.X + rect.Width - pen.Width, rect.Y + pen.Width / 2f);
		PointF pointF4 = new PointF(rect.X + rect.Width - pen.Width, rect.Y + rect.Height - pen.Width);
		g.DrawLine(pen, pointF.X, pointF.Y, pointF4.X, pointF4.Y);
		g.DrawLine(pen, pointF2.X, pointF2.Y, pointF3.X, pointF3.Y);
	}

	private PNFColumn[] ComputeRectangles(ChartSeries series)
	{
		ArrayList arrayList = new ArrayList();
		int count = series.Points.Count;
		for (int i = 0; i < count; i++)
		{
			if (m_series.Points[i].YValues.Length < 2)
			{
				return null;
			}
			if (!IsVisiblePoint(m_series.Points[i]))
			{
				continue;
			}
			_ = m_series.Points[i].X;
			double num = Math.Max(m_series.Points[i].YValues[0], m_series.Points[i].YValues[1]);
			double num2 = Math.Min(m_series.Points[i].YValues[0], m_series.Points[i].YValues[1]);
			num = Math.Floor(num / m_series.HeightBox) * m_series.HeightBox + m_series.HeightBox;
			num2 = Math.Floor(num2 / m_series.HeightBox) * m_series.HeightBox;
			if (arrayList.Count == 0)
			{
				PNFColumn pNFColumn = new PNFColumn();
				pNFColumn.X = m_series.Points[i].X;
				pNFColumn.High = num;
				pNFColumn.Low = num2;
				pNFColumn.Width = 1.0;
				pNFColumn.IsPoint = true;
				arrayList.Add(pNFColumn);
				continue;
			}
			PNFColumn pNFColumn2 = arrayList[arrayList.Count - 1] as PNFColumn;
			pNFColumn2.Width = m_series.Points[i].X - pNFColumn2.X;
			if (pNFColumn2.IsPoint)
			{
				if (num2 < pNFColumn2.Low)
				{
					pNFColumn2.Low = num2;
				}
				if (num > pNFColumn2.Low + series.ReversalAmount)
				{
					PNFColumn pNFColumn3 = new PNFColumn();
					pNFColumn3.indexX = i;
					pNFColumn3.X = m_series.Points[i].X;
					pNFColumn3.High = num;
					pNFColumn3.Low = pNFColumn2.Low + series.HeightBox;
					pNFColumn3.IsPoint = false;
					pNFColumn3.Width = 1.0;
					arrayList.Add(pNFColumn3);
				}
			}
			else
			{
				if (num > pNFColumn2.High)
				{
					pNFColumn2.High = num;
				}
				if (num2 < pNFColumn2.High - series.ReversalAmount)
				{
					PNFColumn pNFColumn4 = new PNFColumn();
					pNFColumn4.indexX = i;
					pNFColumn4.X = m_series.Points[i].X;
					pNFColumn4.Low = num2;
					pNFColumn4.High = pNFColumn2.High - series.HeightBox;
					pNFColumn4.IsPoint = true;
					pNFColumn4.Width = 1.0;
					arrayList.Add(pNFColumn4);
				}
			}
		}
		return arrayList.ToArray(typeof(PNFColumn)) as PNFColumn[];
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		base.DrawIcon(g, bounds, isShadow, shadowColor);
		if (!isShadow)
		{
			_ = bounds.Width / 4;
			_ = bounds.Height / 4;
			Pen pen = new Pen(m_series.ConfigItems.FinancialItem.PriceUpColor);
			Pen pen2 = new Pen(m_series.ConfigItems.FinancialItem.PriceDownColor);
			g.DrawEllipse(pen2, bounds);
			g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
			g.DrawLine(pen, bounds.Left, bounds.Bottom, bounds.Right, bounds.Top);
			pen.Dispose();
			pen2.Dispose();
		}
	}
}
