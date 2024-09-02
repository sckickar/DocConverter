using System;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class RadarRenderer : ChartSeriesRenderer
{
	protected override int RequireYValuesCount => 1;

	protected override string RegionDescription => "Radar Chart Renderer";

	public RadarRenderer(ChartSeries series)
		: base(series)
	{
	}

	private BrushInfo GradientBrush(BrushInfo brushInfo)
	{
		ChartColumnConfigItem columnItem = m_series.ConfigItems.ColumnItem;
		if (base.Chart.Model.ColorModel.AllowGradient && columnItem.ShadingMode == ChartColumnShadingMode.PhongCylinder)
		{
			brushInfo = GetPhongInterior(brushInfo, columnItem.LightColor, columnItem.LightAngle, columnItem.PhongAlpha);
		}
		return brushInfo;
	}

	public override void Render(ChartRenderArgs2D args)
	{
		_ = args.SeriesIndex;
		_ = args.Series.PointFormats[ChartYValueUsage.YValue];
		ChartRadarConfigItem radarItem = m_series.ConfigItems.RadarItem;
		ChartStyledPoint[] array = PrepearePoints();
		PointF[] array2 = new PointF[array.Length];
		ChartStyleInfo seriesStyle = base.SeriesStyle;
		BrushInfo brush = GetBrush();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IsVisible)
			{
				array2[i] = GetPointFromValue(array[i].Point);
			}
			else
			{
				array2[i] = base.ChartArea.Center;
			}
		}
		if (array2.Length == 0)
		{
			return;
		}
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddLines(array2);
		graphicsPath.CloseAllFigures();
		switch (radarItem.Type)
		{
		case ChartRadarDrawType.Area:
			if (seriesStyle.DisplayShadow)
			{
				GraphicsPath graphicsPath3 = (GraphicsPath)graphicsPath.Clone();
				Matrix matrix = new Matrix();
				matrix.Translate(seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height);
				graphicsPath3.Transform(matrix);
				args.Graph.DrawPath(seriesStyle.ShadowInterior, null, graphicsPath3);
			}
			args.Graph.DrawPath(brush, seriesStyle.GdipPen, graphicsPath);
			break;
		case ChartRadarDrawType.Line:
			if (seriesStyle.DisplayShadow)
			{
				GraphicsPath graphicsPath4 = (GraphicsPath)graphicsPath.Clone();
				Matrix matrix2 = new Matrix();
				matrix2.Translate(seriesStyle.ShadowOffset.Width, seriesStyle.ShadowOffset.Height);
				graphicsPath4.Transform(matrix2);
				args.Graph.DrawPath(seriesStyle.ShadowInterior, null, graphicsPath4);
			}
			args.Graph.DrawPath(seriesStyle.GdipPen, graphicsPath);
			break;
		case ChartRadarDrawType.Symbol:
			foreach (ChartStyledPoint chartStyledPoint in array)
			{
				if (chartStyledPoint.Style.Symbol.Shape == ChartSymbolShape.None)
				{
					GraphicsPath graphicsPath2 = new GraphicsPath();
					PointF symbolPoint = GetSymbolPoint(chartStyledPoint);
					Size size = chartStyledPoint.Style.Symbol.Size;
					graphicsPath2.AddEllipse(symbolPoint.X - (float)(size.Width / 2), symbolPoint.Y - (float)(size.Height / 2), size.Width, size.Height);
					args.Graph.DrawPath(GetBrush(chartStyledPoint.Index), chartStyledPoint.Style.Border.GdipPen, graphicsPath2);
					AddSymbolRegion(chartStyledPoint);
				}
			}
			break;
		}
	}

	public override void Render(Graphics3D g)
	{
		float num = base.ChartArea.Depth / (float)base.PlaceSize;
		float num2 = (float)base.Place * num;
		ChartRadarConfigItem radarItem = m_series.ConfigItems.RadarItem;
		base.Chart.Series.IndexOf(m_series);
		ChartStyleInfo offlineStyle = m_series.GetOfflineStyle();
		Vector3D[] array = new Vector3D[m_series.Points.Count];
		for (int i = 0; i < m_series.Points.Count; i++)
		{
			PointF pointFromIndex = GetPointFromIndex(i);
			array[i] = new Vector3D(pointFromIndex.X, pointFromIndex.Y, num2);
		}
		Polygon polygon = null;
		switch (radarItem.Type)
		{
		case ChartRadarDrawType.Area:
			polygon = new Polygon(array, GetBrush(), offlineStyle.GdipPen);
			break;
		case ChartRadarDrawType.Line:
			polygon = new Polygon(array, offlineStyle.GdipPen);
			break;
		case ChartRadarDrawType.Symbol:
		{
			for (int j = 0; j < m_series.Points.Count; j++)
			{
				ChartStyleInfo styleAt = GetStyleAt(j);
				if (styleAt.Symbol.Shape == ChartSymbolShape.None)
				{
					GraphicsPath graphicsPath = new GraphicsPath();
					Vector3D vector3D = array[j];
					Size size = styleAt.Symbol.Size;
					graphicsPath.AddEllipse((float)(vector3D.X - (double)(size.Width / 2)), (float)(vector3D.Y - (double)(size.Height / 2)), size.Width, size.Height);
					Path3D polygon2 = Path3D.FromGraphicsPath(graphicsPath, vector3D.Z, GetBrush(), styleAt.GdipPen);
					g.AddPolygon(polygon2);
				}
			}
			break;
		}
		}
		g.AddPolygon(polygon);
	}

	protected override BrushInfo GetBrush()
	{
		return GradientBrush(base.GetBrush());
	}

	protected override BrushInfo GetBrush(int index)
	{
		return GradientBrush(base.GetBrush(index));
	}

	protected override BrushInfo GetBrush(ChartStyleInfo style)
	{
		return GradientBrush(base.GetBrush(style));
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
		BrushInfo brushInfo = base.SeriesStyle.Interior;
		ChartColumnConfigItem columnItem = m_series.ConfigItems.ColumnItem;
		if (base.Chart.Model.ColorModel.AllowGradient && columnItem.ShadingMode == ChartColumnShadingMode.PhongCylinder)
		{
			brushInfo = GetPhongInterior(brushInfo, columnItem.LightColor, columnItem.LightAngle, columnItem.PhongAlpha);
		}
		if (m_series.ConfigItems.RadarItem.Type == ChartRadarDrawType.Area)
		{
			BrushPaint.FillRectangle(g, bounds, brushInfo);
			g.DrawRectangle(base.SeriesStyle.GdipPen, bounds);
		}
		else
		{
			g.DrawLine(base.SeriesStyle.GdipPen, bounds.Left + bounds.Width / 8, (float)((double)bounds.Y + Math.Ceiling((double)bounds.Height / 2.0)), bounds.Right, (float)((double)bounds.Y + Math.Ceiling((double)bounds.Height / 2.0)));
		}
	}
}
