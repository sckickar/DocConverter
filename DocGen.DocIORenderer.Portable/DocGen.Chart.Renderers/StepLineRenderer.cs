using System.Collections;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class StepLineRenderer : LineRenderer
{
	protected override string RegionDescription => "Step Line Region";

	protected override int RequireYValuesCount => 1;

	public StepLineRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		_ = base.Chart.Series3D;
		bool flag = (IsInvertedAxes ? base.YAxis.Inversed : base.XAxis.Inversed);
		_ = base.Chart.DropSeriesPoints;
		bool inverted = m_series.ConfigItems.StepItem.Inverted;
		int num = m_series.PointFormats[ChartYValueUsage.YValue];
		_ = ((ChartGDIGraph)args.Graph).Graphics.ClipBounds;
		_ = base.SeriesStyle;
		GetThisOffset();
		SizeF seriesOffset = GetSeriesOffset();
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange vrange = CalculateVisibleRange();
		IndexRange[] array2 = CalculateUnEmptyRanges(vrange);
		for (int i = 0; i < array2.Length; i++)
		{
			IndexRange indexRange = array2[i];
			ChartStyledPoint chartStyledPoint = null;
			ChartStyledPoint chartStyledPoint2 = null;
			PointF pointF = PointF.Empty;
			PointF empty = PointF.Empty;
			int num2 = (flag ? indexRange.To : indexRange.From);
			int num3 = (flag ? (indexRange.From - 1) : (indexRange.To + 1));
			int num4 = ((!flag) ? 1 : (-1));
			for (int j = num2; j != num3; j += num4)
			{
				chartStyledPoint2 = array[j];
				if (!chartStyledPoint2.IsVisible)
				{
					continue;
				}
				empty = args.GetPoint(chartStyledPoint2.X, chartStyledPoint2.YValues[num]);
				if (chartStyledPoint != null)
				{
					PointF pointF2 = (inverted ? args.GetPoint(chartStyledPoint.X, chartStyledPoint2.YValues[num]) : args.GetPoint(chartStyledPoint2.X, chartStyledPoint.YValues[num]));
					if (args.Is3D)
					{
						GraphicsPath graphicsPath = new GraphicsPath();
						GraphicsPath graphicsPath2 = new GraphicsPath();
						graphicsPath.AddLine(pointF, ChartMath.AddPoint(pointF, seriesOffset));
						graphicsPath.AddLine(ChartMath.AddPoint(pointF2, seriesOffset), pointF2);
						graphicsPath.CloseFigure();
						graphicsPath2.AddLine(pointF2, ChartMath.AddPoint(pointF2, seriesOffset));
						graphicsPath2.AddLine(ChartMath.AddPoint(empty, seriesOffset), empty);
						graphicsPath2.CloseFigure();
						args.Graph.DrawPath(GetBrush(chartStyledPoint.Index), chartStyledPoint.Style.GdipPen, graphicsPath);
						args.Graph.DrawPath(GetBrush(chartStyledPoint.Index), chartStyledPoint.Style.GdipPen, graphicsPath2);
					}
					else
					{
						using Pen pen = chartStyledPoint.Style.GdipPen.Clone() as Pen;
						GraphicsPath graphicsPath3 = new GraphicsPath();
						graphicsPath3.AddLine(pointF, pointF2);
						graphicsPath3.AddLine(pointF2, empty);
						if (chartStyledPoint.Style.DisplayShadow)
						{
							Size shadowOffset = chartStyledPoint.Style.ShadowOffset;
							pen.Color = chartStyledPoint.Style.ShadowInterior.BackColor;
							args.Graph.PushTransform();
							args.Graph.Transform = new Matrix(1f, 0f, 0f, 1f, shadowOffset.Width, shadowOffset.Height);
							args.Graph.DrawPath(pen, graphicsPath3);
							args.Graph.PopTransform();
						}
						pen.Color = GetBrush(chartStyledPoint.Index).BackColor;
						args.Graph.DrawPath(pen, graphicsPath3);
					}
				}
				pointF = empty;
				chartStyledPoint = chartStyledPoint2;
			}
		}
	}

	public override void Render(ChartRenderArgs3D args)
	{
		_ = base.Chart.DropSeriesPoints;
		_ = IsInvertedAxes;
		base.Chart.Series.IndexOf(m_series);
		int num = m_series.PointFormats[ChartYValueUsage.YValue];
		bool inverted = m_series.ConfigItems.StepItem.Inverted;
		float placeDepth = GetPlaceDepth();
		float seriesDepth = GetSeriesDepth();
		float num2 = placeDepth + seriesDepth;
		_ = base.SeriesStyle;
		ChartStyledPoint[] array = PrepearePoints();
		IndexRange vrange = CalculateVisibleRange();
		IndexRange[] array2 = CalculateUnEmptyRanges(vrange);
		int i = 0;
		for (int num3 = array2.Length; i < num3; i++)
		{
			IndexRange indexRange = array2[i];
			ChartStyledPoint chartStyledPoint = null;
			ChartStyledPoint chartStyledPoint2 = null;
			PointF pointF = PointF.Empty;
			PointF empty = PointF.Empty;
			args.Graph.AddPolygon(CreateBoundsPolygon(placeDepth));
			ArrayList arrayList = new ArrayList(array.Length);
			ArrayList arrayList2 = new ArrayList(array.Length);
			int j = indexRange.From;
			for (int num4 = indexRange.To + 1; j != num4; j++)
			{
				chartStyledPoint2 = array[j];
				if (!chartStyledPoint2.Point.IsEmpty)
				{
					empty = args.GetPoint(chartStyledPoint2.X, chartStyledPoint2.YValues[num]);
					if (chartStyledPoint != null)
					{
						PointF pointF2 = (inverted ? args.GetPoint(chartStyledPoint.X, chartStyledPoint2.YValues[num]) : args.GetPoint(chartStyledPoint2.X, chartStyledPoint.YValues[num]));
						Vector3D[] points = new Vector3D[4]
						{
							new Vector3D(pointF2.X, pointF2.Y, placeDepth),
							new Vector3D(pointF.X, pointF.Y, placeDepth),
							new Vector3D(pointF.X, pointF.Y, num2),
							new Vector3D(pointF2.X, pointF2.Y, num2)
						};
						Vector3D[] points2 = new Vector3D[4]
						{
							new Vector3D(empty.X, empty.Y, placeDepth),
							new Vector3D(pointF2.X, pointF2.Y, placeDepth),
							new Vector3D(pointF2.X, pointF2.Y, num2),
							new Vector3D(empty.X, empty.Y, num2)
						};
						Polygon value = new Polygon(points, GetBrush(chartStyledPoint.Index), chartStyledPoint.Style.GdipPen);
						Polygon value2 = new Polygon(points2, GetBrush(chartStyledPoint.Index), chartStyledPoint.Style.GdipPen);
						arrayList.Add(value);
						arrayList2.Add(value2);
					}
					pointF = empty;
					chartStyledPoint = chartStyledPoint2;
				}
			}
			if (inverted)
			{
				for (int k = 0; k < arrayList.Count; k++)
				{
					args.Graph.AddPolygon(arrayList[k] as Polygon);
				}
				for (int l = 0; l < arrayList2.Count; l++)
				{
					args.Graph.AddPolygon(arrayList2[l] as Polygon);
				}
			}
			else
			{
				for (int m = 0; m < arrayList2.Count; m++)
				{
					args.Graph.AddPolygon(arrayList2[m] as Polygon);
				}
				for (int n = 0; n < arrayList.Count; n++)
				{
					args.Graph.AddPolygon(arrayList[n] as Polygon);
				}
			}
		}
	}
}
