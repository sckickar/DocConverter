using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart.Renderers;

internal class ChartHeatMapRenderer : ChartSeriesRenderer
{
	private class HeatRectangle
	{
		public RectangleF Rectangle;

		public double AreaCoeficient;

		public double ColorCoeficient;

		public ChartStyledPoint StyledPoint;
	}

	protected override string RegionDescription => "HeatMap Chart Renderer";

	protected override int RequireYValuesCount => 2;

	public override ChartUsedSpaceType FillSpaceType => ChartUsedSpaceType.All;

	public ChartHeatMapRenderer(ChartSeries series)
		: base(series)
	{
	}

	public override void Render(ChartRenderArgs2D args)
	{
		RectangleF bounds = args.Bounds;
		ChartHeatMapConfigItem heatMapItem = args.Series.ConfigItems.HeatMapItem;
		if (heatMapItem.DisplayColorSwatch)
		{
			float num = DrawColorSwatch(args);
			bounds.Y += num;
			bounds.Height -= num;
		}
		double num2 = double.MaxValue;
		double num3 = double.MinValue;
		ChartStyledPoint[] list = PrepearePoints();
		ChartStyledPoint[] visiblePoints = GetVisiblePoints(list);
		HeatRectangle[] array = new HeatRectangle[visiblePoints.Length];
		for (int i = 0; i < visiblePoints.Length; i++)
		{
			array[i] = new HeatRectangle();
			array[i].StyledPoint = visiblePoints[i];
			num2 = Math.Min(num2, visiblePoints[i].YValues[1]);
			num3 = Math.Max(num3, visiblePoints[i].YValues[1]);
		}
		for (int j = 0; j < visiblePoints.Length; j++)
		{
			array[j].ColorCoeficient = (visiblePoints[j].YValues[1] - num2) / (num3 - num2);
		}
		ComputeAreaCoeficient(array, 0, array.Length);
		Array.Sort(array, CompareHeatRectangles);
		switch (heatMapItem.HeatMapStyle)
		{
		case ChartHeatMapLayoutStyle.Rectangular:
			RectangleLayout(array, bounds);
			break;
		case ChartHeatMapLayoutStyle.Vertical:
			VerticalLayout(array, bounds);
			break;
		case ChartHeatMapLayoutStyle.Horizontal:
			HorizontalLayout(array, bounds);
			break;
		}
		base.Render(args);
	}

	public override void Render(ChartRenderArgs3D args)
	{
		ChartRenderArgs2D chartRenderArgs2D = new ChartRenderArgs2D(args.Chart, args.Series);
		chartRenderArgs2D.Graph = new ChartGDIGraph(args.Graph.Graphics);
		chartRenderArgs2D.Bounds = args.Bounds;
		Render(chartRenderArgs2D);
	}

	private float DrawColorSwatch(ChartRenderArgs2D args)
	{
		float height = base.SeriesStyle.GdipFont.GetHeight();
		ChartHeatMapConfigItem heatMapItem = args.Series.ConfigItems.HeatMapItem;
		float labelMargins = heatMapItem.LabelMargins;
		Font gdipFont = base.SeriesStyle.GdipFont;
		Brush brush = new SolidBrush(base.SeriesStyle.TextColor);
		SizeF size = args.Graph.MeasureString(heatMapItem.StartText, gdipFont);
		SizeF size2 = args.Graph.MeasureString(heatMapItem.EndText, gdipFont);
		SizeF size3 = (heatMapItem.DisplayTitle ? args.Graph.MeasureString(args.Series.Text, gdipFont) : SizeF.Empty);
		if (!size.IsEmpty)
		{
			size.Width += labelMargins;
		}
		if (!size2.IsEmpty)
		{
			size2.Width += labelMargins;
		}
		height = Math.Max(height, size.Height);
		height = Math.Max(height, size2.Height);
		height = Math.Max(height, size3.Height);
		RectangleF value = Rectangle.Ceiling(args.Bounds);
		value.Height = height;
		if (!size3.IsEmpty)
		{
			RectangleF rect = Rectangle.Ceiling(new RectangleF(value.Location, size3));
			args.Graph.DrawString(args.Series.Text, gdipFont, brush, rect, DrawingHelper.CenteredFormat);
			args.Graph.DrawRect(Pens.Black, rect);
			value.X += size3.Width;
			value.Width -= size3.Width;
		}
		if (!size.IsEmpty)
		{
			RectangleF rectangleF = Rectangle.Ceiling(new RectangleF(value.Location, size));
			args.Graph.DrawString(heatMapItem.StartText, gdipFont, brush, rectangleF, DrawingHelper.CenteredFormat);
			args.Graph.DrawRect(Pens.Black, Rectangle.Ceiling(rectangleF));
			value.X += rectangleF.Width;
			value.Width -= rectangleF.Width;
		}
		if (!size2.IsEmpty)
		{
			RectangleF rectangleF2 = Rectangle.Ceiling(new RectangleF(value.Location, size2));
			rectangleF2.X = value.Right - rectangleF2.Width;
			args.Graph.DrawString(heatMapItem.EndText, gdipFont, brush, rectangleF2, DrawingHelper.CenteredFormat);
			args.Graph.DrawRect(Pens.Black, Rectangle.Ceiling(rectangleF2));
			value.Width -= rectangleF2.Width;
		}
		brush.Dispose();
		if (!value.IsEmpty)
		{
			using LinearGradientBrush linearGradientBrush = new LinearGradientBrush(Rectangle.Ceiling(value), heatMapItem.LowestValueColor, heatMapItem.HighestValueColor, LinearGradientMode.Horizontal);
			ColorBlend colorBlend = new ColorBlend();
			colorBlend.Positions = new float[3] { 0f, 0.5f, 1f };
			colorBlend.Colors = new Color[3] { heatMapItem.LowestValueColor, heatMapItem.MiddleValueColor, heatMapItem.HighestValueColor };
			linearGradientBrush.InterpolationColors = colorBlend;
			args.Graph.DrawRect(linearGradientBrush, Pens.Black, Rectangle.Ceiling(value));
		}
		return height;
	}

	private void VerticalLayout(IList<HeatRectangle> rects, RectangleF bounds)
	{
		float num = bounds.Y;
		foreach (HeatRectangle rect in rects)
		{
			float num2 = (float)(rect.AreaCoeficient * (double)bounds.Height);
			rect.Rectangle = new RectangleF(bounds.X, num, bounds.Width, num2);
			num += num2;
		}
	}

	private void HorizontalLayout(IList<HeatRectangle> rects, RectangleF bounds)
	{
		float num = bounds.X;
		foreach (HeatRectangle rect in rects)
		{
			float num2 = (float)(rect.AreaCoeficient * (double)bounds.Width);
			rect.Rectangle = new RectangleF(num, bounds.Y, num2, bounds.Height);
			num += num2;
		}
	}

	private void RectangleLayout(IList<HeatRectangle> rects, RectangleF bounds)
	{
		RectangleF rectangleF = bounds;
		List<HeatRectangle> list = new List<HeatRectangle>();
		double num = 0.0;
		bool flag = rectangleF.Height < rectangleF.Width;
		double amount = GetAmount(rects, 0, rectangleF, flag);
		double num2 = bounds.Width * bounds.Height;
		int i = 0;
		for (int num3 = rects.Count - 1; i <= num3; i++)
		{
			list.Add(rects[i]);
			num += rects[i].AreaCoeficient;
			float num4 = (flag ? rectangleF.Height : rectangleF.Width);
			if (num2 * num / amount > (double)num4 || i == num3)
			{
				RectangleF bounds2 = rectangleF;
				ComputeAreaCoeficient(list, 0, list.Count);
				ComputeAreaCoeficient(rects, i + 1, rects.Count - i - 1);
				float num5 = (float)(num2 * num / (double)num4);
				if (flag)
				{
					bounds2.Width = num5;
					rectangleF.X += num5;
					rectangleF.Width -= num5;
					VerticalLayout(list, bounds2);
				}
				else
				{
					bounds2.Height = num5;
					rectangleF.Y += num5;
					rectangleF.Height -= num5;
					HorizontalLayout(list, bounds2);
				}
				if (i != num3)
				{
					amount = GetAmount(rects, i, rectangleF, flag);
					num2 = rectangleF.Width * rectangleF.Height;
					flag = rectangleF.Height < rectangleF.Width;
					num = 0.0;
					list.Clear();
				}
			}
		}
	}

	private void DrawRectangle(ChartRenderArgs2D args, HeatRectangle rect)
	{
		ChartStyleInfo style = rect.StyledPoint.Style;
		ChartHeatMapConfigItem heatMapItem = args.Series.ConfigItems.HeatMapItem;
		using (SolidBrush brush = new SolidBrush(LeprColor(heatMapItem, rect.ColorCoeficient)))
		{
			args.Graph.DrawRect(brush, style.GdipPen, Rectangle.Round(rect.Rectangle));
		}
		if (!style.DisplayText)
		{
			return;
		}
		bool flag = true;
		Font font = style.GdipFont;
		StringFormat stringFormat = new StringFormat(StringFormatFlags.NoWrap);
		string text = GetText(style.Text, heatMapItem);
		float maxWidth = 2.1474836E+09f;
		stringFormat.LineAlignment = StringAlignment.Center;
		stringFormat.Alignment = StringAlignment.Center;
		if (heatMapItem.EnableLabelRotation && rect.Rectangle.Width < rect.Rectangle.Height)
		{
			stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft | StringFormatFlags.DirectionVertical;
		}
		if (heatMapItem.AllowLabelsAutoFit)
		{
			SizeF sizeF = args.Graph.MeasureString(text, font, maxWidth, stringFormat);
			float val = rect.Rectangle.Width / sizeF.Width;
			float val2 = rect.Rectangle.Height / sizeF.Height;
			float num = Math.Max(heatMapItem.MinimumFontSize, (float)Math.Truncate(Math.Min(val, val2) * font.Size));
			if (num < font.Size)
			{
				font = new Font(font.GetFontName(), num, font.Style);
			}
		}
		if (!heatMapItem.ShowLargeLabels)
		{
			SizeF sizeF2 = args.Graph.MeasureString(text, font, maxWidth, stringFormat);
			flag = (double)sizeF2.Width < Math.Ceiling(rect.Rectangle.Width) && (double)sizeF2.Height < Math.Ceiling(rect.Rectangle.Height);
		}
		if (flag)
		{
			using (SolidBrush brush2 = new SolidBrush(style.TextColor))
			{
				args.Graph.DrawString(text, font, brush2, rect.Rectangle, stringFormat);
			}
		}
	}

	private string GetText(string text, ChartHeatMapConfigItem configItem)
	{
		if (configItem.EnableLabelsTruncation && configItem.MaximumCharacters > 0 && configItem.MaximumCharacters < text.Length)
		{
			return text.Substring(0, configItem.MaximumCharacters) + "...";
		}
		return text;
	}

	private double GetAmount(IList<HeatRectangle> rects, int index, RectangleF bounds, bool vertival)
	{
		double num = rects[index].AreaCoeficient * (double)bounds.Width * (double)bounds.Height;
		return num / (1.2 * Math.Sqrt(num));
	}

	private void ComputeAreaCoeficient(IList<HeatRectangle> rects, int start, int length)
	{
		double num = 0.0;
		int i = start;
		for (int num2 = start + length; i < num2; i++)
		{
			num += rects[i].StyledPoint.YValues[0];
		}
		int j = start;
		for (int num3 = start + length; j < num3; j++)
		{
			rects[j].AreaCoeficient = rects[j].StyledPoint.YValues[0] / num;
		}
	}

	private Color LeprColor(ChartHeatMapConfigItem item, double value)
	{
		if (value < 0.5)
		{
			return DrawingHelper.LeprColor(item.LowestValueColor, item.MiddleValueColor, 2.0 * value);
		}
		if (value == 1.0)
		{
			return DrawingHelper.LeprColor(item.MiddleValueColor, item.HighestValueColor, value);
		}
		return DrawingHelper.LeprColor(item.HighestValueColor, item.MiddleValueColor, Math.Round(1.0 - value, 15) * 2.0);
	}

	private static int CompareHeatRectangles(HeatRectangle x, HeatRectangle y)
	{
		return y.AreaCoeficient.CompareTo(x.AreaCoeficient);
	}

	protected internal override void RenderAdornments(Graphics g)
	{
	}

	protected internal override void RenderAdornments(Graphics3D g)
	{
	}
}
