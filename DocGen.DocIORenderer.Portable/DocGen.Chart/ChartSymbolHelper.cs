using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal static class ChartSymbolHelper
{
	public static Brush GetBrushItem(BrushInfo info, RectangleF bounds)
	{
		return ChartGraph.GetBrushItem(info, bounds);
	}

	public static void DrawPointSymbol(Graphics g, ChartStyleInfo info, Point location)
	{
		RenderingHelper.DrawPointSymbol(new ChartGDIGraph(g), info, location);
	}

	public static void FillAndDrawSymbol(Graphics g, ChartSymbolShape symbol, Rectangle bounds, Pen pen, Brush brush)
	{
		GraphicsPath pathSymbol = GetPathSymbol(symbol, bounds);
		if (pathSymbol != null)
		{
			g.FillPath(brush, pathSymbol);
			g.DrawPath(pen, pathSymbol);
		}
	}

	public static void DrawSymbol(Graphics g, ChartSymbolShape symbol, Rectangle bounds, Pen pen)
	{
		GraphicsPath pathSymbol = GetPathSymbol(symbol, bounds);
		if (pathSymbol != null)
		{
			g.DrawPath(pen, pathSymbol);
		}
	}

	public static void FillSymbol(Graphics g, ChartSymbolShape symbol, Rectangle bounds, Brush brush)
	{
		GraphicsPath pathSymbol = GetPathSymbol(symbol, bounds);
		if (pathSymbol != null)
		{
			g.FillPath(brush, pathSymbol);
		}
	}

	public static GraphicsPath GetPathSymbol(ChartSymbolShape symbol, Rectangle bounds)
	{
		GraphicsPath result = null;
		switch (symbol)
		{
		case ChartSymbolShape.Arrow:
			result = GetPathArrow(bounds);
			break;
		case ChartSymbolShape.InvertedArrow:
			result = GetPathInvertedArrow(bounds);
			break;
		case ChartSymbolShape.Circle:
			result = GetPathCircle(bounds);
			break;
		case ChartSymbolShape.Diamond:
			result = GetPathDiamond(bounds);
			break;
		case ChartSymbolShape.Hexagon:
			result = GetPathHexagon(bounds);
			break;
		case ChartSymbolShape.InvertedTriangle:
			result = GetPathInvertedTriangle(bounds);
			break;
		case ChartSymbolShape.Pentagon:
			result = GetPathPentagon(bounds);
			break;
		case ChartSymbolShape.Square:
			result = GetPathSquare(bounds);
			break;
		case ChartSymbolShape.Triangle:
			result = GetPathTriangle(bounds);
			break;
		case ChartSymbolShape.Plus:
			result = GetPathPlus(bounds);
			break;
		case ChartSymbolShape.Cross:
			result = GetPathCross(bounds);
			break;
		case ChartSymbolShape.ExcelStar:
			result = GetPathExcelStar(bounds);
			break;
		case ChartSymbolShape.Star:
			result = GetPathStar(bounds);
			break;
		case ChartSymbolShape.HorizLine:
			result = GetPathHorizLine(bounds);
			break;
		case ChartSymbolShape.DowJonesLine:
			result = GetPathDowJonesLine(bounds);
			break;
		case ChartSymbolShape.VertLine:
			result = GetPathVertLine(bounds);
			break;
		}
		return result;
	}

	public static GraphicsPath GetPathCircle(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddEllipse(bounds);
		return graphicsPath;
	}

	public static GraphicsPath GetPathDiamond(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddPolygon(new PointF[4]
		{
			new PointF(bounds.X + bounds.Width / 2f, bounds.Y),
			new PointF(bounds.Right, bounds.Y + bounds.Height / 2f),
			new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom),
			new PointF(bounds.X, bounds.Y + bounds.Height / 2f)
		});
		return graphicsPath;
	}

	public static GraphicsPath GetPathSquare(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(bounds);
		return graphicsPath;
	}

	public static GraphicsPath GetPathTriangle(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddPolygon(new PointF[3]
		{
			new PointF(bounds.X + bounds.Width / 2f, bounds.Y),
			new PointF(bounds.X, bounds.Bottom),
			new PointF(bounds.Right, bounds.Bottom)
		});
		return graphicsPath;
	}

	public static GraphicsPath GetPathInvertedTriangle(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddPolygon(new PointF[3]
		{
			new PointF(bounds.X, bounds.Y),
			new PointF(bounds.Right, bounds.Y),
			new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom)
		});
		return graphicsPath;
	}

	public static GraphicsPath GetPathArrow(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		_ = bounds.Height / 3f;
		float num = bounds.Width / 3f;
		graphicsPath.AddPolygon(new PointF[4]
		{
			new PointF(bounds.Left, bounds.Top),
			new PointF(bounds.Right, bounds.Top + bounds.Height / 2f),
			new PointF(bounds.Left, bounds.Bottom),
			new PointF(bounds.Right - num * 2f, bounds.Bottom - bounds.Height / 2f)
		});
		return graphicsPath;
	}

	public static GraphicsPath GetPathInvertedArrow(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float num = bounds.Height / 3f;
		_ = bounds.Width / 3f;
		graphicsPath.AddPolygon(new PointF[4]
		{
			new PointF(bounds.Right, bounds.Top),
			new PointF(bounds.Left, bounds.Top + bounds.Width / 2f),
			new PointF(bounds.Right, bounds.Bottom),
			new PointF(bounds.Left + num * 2f, bounds.Bottom - bounds.Width / 2f)
		});
		return graphicsPath;
	}

	public static GraphicsPath GetPathHexagon(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddPolygon(new PointF[6]
		{
			new PointF(bounds.X + bounds.Width / 4f, bounds.Y),
			new PointF(bounds.X + bounds.Width * 0.75f, bounds.Y),
			new PointF(bounds.Right, bounds.Y + bounds.Height / 2f),
			new PointF(bounds.X + bounds.Width * 0.75f, bounds.Bottom),
			new PointF(bounds.X + bounds.Width / 4f, bounds.Bottom),
			new PointF(bounds.X, bounds.Y + bounds.Height / 2f)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static GraphicsPath GetPathPentagon(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddPolygon(new PointF[5]
		{
			new PointF(bounds.X + bounds.Width / 5f, bounds.Y),
			new PointF(bounds.X + bounds.Width - bounds.Width / 5f, bounds.Y),
			new PointF(bounds.Right, bounds.Y + bounds.Height * 0.6f),
			new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom),
			new PointF(bounds.X, bounds.Y + bounds.Height * 0.6f)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static GraphicsPath GetPathPlus(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float num = bounds.Width / 2f;
		float num2 = bounds.Height / 2f;
		graphicsPath.AddRectangle(bounds);
		graphicsPath.AddLines(new PointF[8]
		{
			new PointF(bounds.X, bounds.Y + num2),
			new PointF(bounds.X + num, bounds.Y + num2),
			new PointF(bounds.X + num, bounds.Y),
			new PointF(bounds.X + num, bounds.Y + num2),
			new PointF(bounds.Right, bounds.Y + num2),
			new PointF(bounds.X + num, bounds.Y + num2),
			new PointF(bounds.X + num, bounds.Bottom),
			new PointF(bounds.X + num, bounds.Y + num2)
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static GraphicsPath GetPathCross(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(bounds);
		graphicsPath.AddLine(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
		graphicsPath.AddLine(bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static GraphicsPath GetPathExcelStar(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(bounds);
		graphicsPath.AddLine(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
		graphicsPath.AddLine(bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
		graphicsPath.AddLine(bounds.Left + bounds.Width / 2f, bounds.Top, bounds.Left + bounds.Width / 2f, bounds.Bottom);
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static GraphicsPath GetPathStar(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddLines(new PointF[10]
		{
			new PointF(bounds.X + bounds.Width / 2f, bounds.Top),
			new PointF(bounds.X + bounds.Width * 11f / 18f, bounds.Y + bounds.Height * (1f / 3f)),
			new PointF(bounds.Right, bounds.Y + bounds.Height * (1f / 3f)),
			new PointF(bounds.X + bounds.Width * 2f / 3f, bounds.Y + bounds.Height * 0.6f),
			new PointF(bounds.X + bounds.Width * 5f / 6f, bounds.Bottom),
			new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height * (11f / 15f)),
			new PointF(bounds.X + bounds.Width / 6f, bounds.Bottom),
			new PointF(bounds.X + bounds.Width * 1f / 3f, bounds.Y + bounds.Height * 0.6f),
			new PointF(bounds.X, bounds.Y + bounds.Height * (1f / 3f)),
			new PointF(bounds.X + bounds.Width * 7f / 18f, bounds.Y + bounds.Height * (1f / 3f))
		});
		graphicsPath.CloseFigure();
		return graphicsPath;
	}

	public static GraphicsPath GetPathHorizLine(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(new RectangleF(bounds.X, bounds.Y + bounds.Height / 2f, bounds.Width, bounds.Height / 5f));
		return graphicsPath;
	}

	public static GraphicsPath GetPathDowJonesLine(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddRectangle(new RectangleF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f, bounds.Width / 2f, bounds.Height / 5f));
		return graphicsPath;
	}

	public static GraphicsPath GetPathVertLine(RectangleF bounds)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddLine(new PointF(bounds.X + bounds.Width / 2f, bounds.Top), new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom));
		return graphicsPath;
	}
}
