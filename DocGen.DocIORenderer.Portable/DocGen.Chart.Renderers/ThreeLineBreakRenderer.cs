using System;
using System.Collections;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class ThreeLineBreakRenderer : ChartSeriesRenderer
{
	protected struct TLBRectangle
	{
		private ChartPoint firstPoint;

		private ChartPoint secondPoint;

		private bool negativeValue;

		private ChartSeriesRenderer r;

		public double MinY => Math.Min(firstPoint.YValues[0], secondPoint.YValues[0]);

		public double MaxY => Math.Max(firstPoint.YValues[0], secondPoint.YValues[0]);

		public bool NegativeValue => negativeValue;

		public ChartPoint FirstPoint
		{
			get
			{
				return firstPoint;
			}
			set
			{
				firstPoint = value;
			}
		}

		public ChartPoint SecondPoint
		{
			get
			{
				return secondPoint;
			}
			set
			{
				secondPoint = value;
			}
		}

		public static TLBRectangle Empty => new TLBRectangle(ChartPoint.Empty, ChartPoint.Empty, negVal: false, null);

		public TLBRectangle(ChartPoint fPoint, ChartPoint sPoint, bool negVal, ChartSeriesRenderer renderer)
		{
			firstPoint = fPoint;
			secondPoint = sPoint;
			negativeValue = negVal;
			r = renderer;
		}
	}

	private const int breakLineCount = 3;

	protected override int RequireYValuesCount => 1;

	public ThreeLineBreakRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		TLBRectangle[] array = CalcTreeLineBreak();
		_ = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		BrushInfo brush = GetBrush();
		BrushInfo upPriceInterior = GetUpPriceInterior(brush);
		BrushInfo downPriceInterior = GetDownPriceInterior(brush);
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
			TLBRectangle tlbr = array[i];
			RectangleF rectangle = GetRectangle(tlbr);
			if (args.Is3D)
			{
				rectangle.Offset(args.Offset.Width, args.Offset.Height);
				GraphicsPath path = CreateBox(rectangle, is3D: true);
				if (tlbr.NegativeValue)
				{
					args.Graph.DrawPath(downPriceInterior, seriesStyle.GdipPen, path);
				}
				else
				{
					args.Graph.DrawPath(upPriceInterior, seriesStyle.GdipPen, path);
				}
				continue;
			}
			if (seriesStyle.DisplayShadow)
			{
				RectangleF rect = rectangle;
				rect.Offset(seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height);
				args.Graph.DrawRect(upPriceInterior, seriesStyle.GdipPen, rect);
			}
			if (tlbr.NegativeValue)
			{
				args.Graph.DrawRect(downPriceInterior, seriesStyle.GdipPen, rectangle);
			}
			else
			{
				args.Graph.DrawRect(upPriceInterior, seriesStyle.GdipPen, rectangle);
			}
		}
	}

	public override void Render(Graphics3D g)
	{
		base.Chart.Series.IndexOf(m_series);
		float placeDepth = GetPlaceDepth();
		float seriesDepth = GetSeriesDepth();
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		TLBRectangle[] array = CalcTreeLineBreak();
		int num = array.Length;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		for (int i = 0; i < num; i++)
		{
			RectangleF rectangle = GetRectangle(array[i]);
			if (num2 < rectangle.Top)
			{
				num2 = rectangle.Top;
			}
			if (num3 > rectangle.Bottom)
			{
				num3 = rectangle.Bottom;
			}
		}
		Polygon polygon = new Polygon(new Vector3D[4]
		{
			new Vector3D(GetRectangle(array[0]).Left, num3, placeDepth),
			new Vector3D(GetRectangle(array[num - 1]).Left, num3, placeDepth),
			new Vector3D(GetRectangle(array[num - 1]).Left, num2, placeDepth),
			new Vector3D(GetRectangle(array[0]).Left, num2, placeDepth)
		}, (BrushInfo)null, (Pen)null);
		g.AddPolygon(polygon);
		int j = 0;
		int num4 = array.Length;
		int num5 = 1;
		_ = base.ChartArea.PrimaryYAxis.Inversed;
		if (base.ChartArea.PrimaryXAxis.Inversed)
		{
			j = num4 - num5;
			num4 = -1;
			num5 = -1;
		}
		for (; j != num4; j += num5)
		{
			TLBRectangle tlbr = array[j];
			RectangleF rectangle2 = GetRectangle(tlbr);
			if (tlbr.NegativeValue)
			{
				g.CreateBoxV(new Vector3D(rectangle2.Left, rectangle2.Top, placeDepth), new Vector3D(rectangle2.Right, rectangle2.Bottom, placeDepth + seriesDepth), seriesStyle.GdipPen, new BrushInfo(m_series.ConfigItems.FinancialItem.PriceDownColor));
			}
			else
			{
				g.CreateBoxV(new Vector3D(rectangle2.Left, rectangle2.Top, placeDepth), new Vector3D(rectangle2.Right, rectangle2.Bottom, placeDepth + seriesDepth), seriesStyle.GdipPen, new BrushInfo(m_series.ConfigItems.FinancialItem.PriceUpColor));
			}
		}
	}

	protected TLBRectangle[] CalcTreeLineBreak()
	{
		int count = m_series.Points.Count;
		_ = new float[count];
		ArrayList arrayList = new ArrayList();
		for (int i = 1; i < count; i++)
		{
			if (arrayList.Count == 0)
			{
				bool negVal = !(m_series.Points[i - 1].YValues[0] < m_series.Points[i].YValues[0]);
				arrayList.Add(new TLBRectangle(m_series.Points[i - 1], m_series.Points[i], negVal, this));
				continue;
			}
			TLBRectangle tLBRectangle = (TLBRectangle)arrayList[arrayList.Count - 1];
			TLBRectangle tLBRectangle2 = TLBRectangle.Empty;
			bool flag = false;
			if (arrayList.Count >= 3)
			{
				tLBRectangle2 = (TLBRectangle)arrayList[arrayList.Count - 3];
				for (int j = 1; j < 3; j++)
				{
					TLBRectangle tLBRectangle3 = (TLBRectangle)arrayList[arrayList.Count - j];
					TLBRectangle tLBRectangle4 = (TLBRectangle)arrayList[arrayList.Count - (j + 1)];
					if (tLBRectangle3.NegativeValue == tLBRectangle4.NegativeValue)
					{
						flag = true;
						continue;
					}
					flag = false;
					break;
				}
			}
			if ((flag && m_series.Points[i].YValues[0] < Math.Min(tLBRectangle2.MinY, tLBRectangle.MinY)) || (!flag && m_series.Points[i].YValues[0] < tLBRectangle.MinY))
			{
				arrayList.Add(new TLBRectangle(new ChartPoint(tLBRectangle.SecondPoint.X, tLBRectangle.MinY), m_series.Points[i], negVal: true, this));
			}
			else if ((flag && m_series.Points[i].YValues[0] > Math.Max(tLBRectangle2.MaxY, tLBRectangle.MaxY)) || (!flag && m_series.Points[i].YValues[0] > tLBRectangle.MaxY))
			{
				arrayList.Add(new TLBRectangle(new ChartPoint(tLBRectangle.SecondPoint.X, tLBRectangle.MaxY), m_series.Points[i], negVal: false, this));
			}
			else
			{
				arrayList[arrayList.Count - 1] = new TLBRectangle(tLBRectangle.FirstPoint, new ChartPoint(m_series.Points[i].X, tLBRectangle.SecondPoint.YValues[0]), ((TLBRectangle)arrayList[arrayList.Count - 1]).NegativeValue, this);
			}
		}
		return (TLBRectangle[])arrayList.ToArray(typeof(TLBRectangle));
	}

	private RectangleF GetRectangle(TLBRectangle tlbr)
	{
		PointF pointF = new PointF(GetXFromValue(tlbr.FirstPoint, 0), GetYFromValue(tlbr.FirstPoint, 0));
		PointF pointF2 = new PointF(GetXFromValue(tlbr.SecondPoint, 0), GetYFromValue(tlbr.SecondPoint, 0));
		return new RectangleF(Math.Min(pointF.X, pointF2.X), Math.Min(pointF.Y, pointF2.Y), Math.Abs(pointF2.X - pointF.X), Math.Abs(pointF2.Y - pointF.Y));
	}

	public override void DrawIcon(Graphics g, Rectangle bounds, bool isShadow, Color shadowColor)
	{
		base.DrawIcon(g, bounds, isShadow, shadowColor);
		if (!isShadow)
		{
			int num = bounds.Width / 4;
			int num2 = bounds.Height / 4;
			SolidBrush solidBrush = new SolidBrush(m_series.ConfigItems.FinancialItem.PriceUpColor);
			SolidBrush solidBrush2 = new SolidBrush(m_series.ConfigItems.FinancialItem.PriceDownColor);
			g.FillRectangle(solidBrush2, bounds.Left, bounds.Top + num2, num, num2);
			g.FillRectangle(solidBrush2, bounds.Left + num, bounds.Top + 2 * num2, num, num2);
			g.FillRectangle(solidBrush, bounds.Left + 2 * num, bounds.Top + num2, num, num2);
			g.FillRectangle(solidBrush, bounds.Left + 3 * num, bounds.Top, num, num2);
			solidBrush.Dispose();
			solidBrush2.Dispose();
		}
	}
}
