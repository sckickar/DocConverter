using System;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal class RenderingHelper
{
	public static void DrawPointSymbol(ChartGraph graph, ChartStyleInfo style, PointF point)
	{
		Brush brush = new SolidBrush(style.Symbol.Color);
		Pen gdipPen = style.Symbol.Border.GdipPen;
		DrawPointSymbol(graph, style.Symbol.Shape, brush, gdipPen, style.Symbol.ImageIndex, style.Images, point, style.Symbol.Size);
		brush.Dispose();
	}

	public static void DrawPointSymbol(ChartGraph graph, ChartSymbolShape shape, Brush brush, Pen pen, int imgIndex, ChartImageCollection images, PointF point, SizeF size)
	{
		if (shape == ChartSymbolShape.None)
		{
			return;
		}
		RectangleF rectByCenter = ChartMath.GetRectByCenter(point, size);
		switch (shape)
		{
		case ChartSymbolShape.Arrow:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathArrow(rectByCenter));
			break;
		case ChartSymbolShape.InvertedArrow:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathInvertedArrow(rectByCenter));
			break;
		case ChartSymbolShape.Circle:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathCircle(rectByCenter));
			break;
		case ChartSymbolShape.Plus:
		{
			using Pen pen7 = new Pen(brush);
			graph.DrawRect(brush, pen7, rectByCenter);
			graph.DrawLine(pen, rectByCenter.Left, point.Y, rectByCenter.Right, point.Y);
			graph.DrawLine(pen, point.X, rectByCenter.Top, point.X, rectByCenter.Bottom);
			break;
		}
		case ChartSymbolShape.Cross:
		{
			using Pen pen6 = new Pen(brush);
			graph.DrawRect(brush, pen6, rectByCenter);
			graph.DrawLine(pen, rectByCenter.Left, rectByCenter.Top, rectByCenter.Right, rectByCenter.Bottom);
			graph.DrawLine(pen, rectByCenter.Right, rectByCenter.Top, rectByCenter.Left, rectByCenter.Bottom);
			break;
		}
		case ChartSymbolShape.ExcelStar:
		{
			using Pen pen5 = new Pen(brush);
			graph.DrawRect(brush, pen5, rectByCenter);
			graph.DrawLine(pen, rectByCenter.Left, rectByCenter.Top, rectByCenter.Right, rectByCenter.Bottom);
			graph.DrawLine(pen, rectByCenter.Right, rectByCenter.Top, rectByCenter.Left, rectByCenter.Bottom);
			graph.DrawLine(pen, rectByCenter.Left + rectByCenter.Width / 2f, rectByCenter.Top, rectByCenter.Left + rectByCenter.Width / 2f, rectByCenter.Bottom);
			break;
		}
		case ChartSymbolShape.HorizLine:
			using (new Pen(brush))
			{
				graph.DrawRect(brush, pen, rectByCenter.Left, rectByCenter.Top + rectByCenter.Height / 2f, rectByCenter.Width, rectByCenter.Height / 5f);
				break;
			}
		case ChartSymbolShape.DowJonesLine:
			using (new Pen(brush))
			{
				graph.DrawRect(brush, pen, rectByCenter.Left + rectByCenter.Width / 2f, rectByCenter.Top + rectByCenter.Height / 2f, rectByCenter.Width / 2f, rectByCenter.Height / 5f);
				break;
			}
		case ChartSymbolShape.VertLine:
			using (new Pen(brush))
			{
				graph.DrawLine(pen, point.X, rectByCenter.Top, point.X, rectByCenter.Bottom);
				break;
			}
		case ChartSymbolShape.Diamond:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathDiamond(rectByCenter));
			break;
		case ChartSymbolShape.Hexagon:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathHexagon(rectByCenter));
			break;
		case ChartSymbolShape.InvertedTriangle:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathInvertedTriangle(rectByCenter));
			break;
		case ChartSymbolShape.Pentagon:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathPentagon(rectByCenter));
			break;
		case ChartSymbolShape.Star:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathStar(rectByCenter));
			break;
		case ChartSymbolShape.Square:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathSquare(rectByCenter));
			break;
		case ChartSymbolShape.Triangle:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathTriangle(rectByCenter));
			break;
		case ChartSymbolShape.Image:
			if (images != null && imgIndex >= 0 && imgIndex < images.Count)
			{
				graph.DrawImage(images[imgIndex], rectByCenter.X, rectByCenter.Y, rectByCenter.Width, rectByCenter.Height);
			}
			break;
		}
	}

	public static void DrawMarker(Graphics g, ChartMarker marker, PointF p2, PointF p1)
	{
		Pen pen = (Pen)marker.LineInfo.GdipPen.Clone();
		pen.EndCap = marker.LineCap;
		g.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
	}

	public static void DrawRelatedPointSymbol(Graphics g, ChartRelatedPointSymbolInfo symbol, ChartRelatedPointLineInfo border, ChartImageCollection imgList, PointF pt)
	{
		if (symbol.Shape == ChartSymbolShape.None)
		{
			return;
		}
		pt.X -= symbol.Size.Width / 2;
		pt.Y -= symbol.Size.Height / 2;
		RectangleF bounds = new RectangleF(pt, symbol.Size);
		Brush brush = new SolidBrush(symbol.Color);
		Pen gdipPen = border.GdipPen;
		switch (symbol.Shape)
		{
		case ChartSymbolShape.Arrow:
			DrawArrow(g, bounds, brush, gdipPen);
			break;
		case ChartSymbolShape.InvertedArrow:
			DrawInvertedArrow(g, bounds, brush, gdipPen);
			break;
		case ChartSymbolShape.Circle:
			DrawCircle(g, bounds, brush, gdipPen);
			break;
		case ChartSymbolShape.Plus:
			DrawPlus(g, bounds, gdipPen, brush);
			break;
		case ChartSymbolShape.Cross:
			DrawCross(g, bounds, gdipPen, brush);
			break;
		case ChartSymbolShape.ExcelStar:
			DrawExcelStar(g, bounds, gdipPen, brush);
			break;
		case ChartSymbolShape.Diamond:
			DrawDiamond(g, bounds, brush, gdipPen);
			break;
		case ChartSymbolShape.Hexagon:
			DrawHexagon(g, bounds, brush, gdipPen);
			break;
		case ChartSymbolShape.InvertedTriangle:
			DrawInvertedTriangle(g, bounds, brush, gdipPen);
			break;
		case ChartSymbolShape.Pentagon:
			DrawPentagon(g, bounds, brush, gdipPen);
			break;
		case ChartSymbolShape.Star:
			DrawStar(g, bounds, brush, gdipPen);
			break;
		case ChartSymbolShape.Square:
			DrawSquare(g, bounds, brush, gdipPen);
			break;
		case ChartSymbolShape.Triangle:
			DrawTriangle(g, bounds, brush, gdipPen);
			break;
		case ChartSymbolShape.Image:
		{
			int imageIndex = symbol.ImageIndex;
			if (imageIndex >= 0 && imageIndex < imgList.Count)
			{
				DrawImage(g, bounds, imgList[imageIndex]);
			}
			break;
		}
		}
		gdipPen.Dispose();
		brush.Dispose();
	}

	public static void DrawPointSymbol(Graphics g, ChartStyleInfo style, PointF pt, bool drawMarker)
	{
		ChartSymbolInfo symbol = style.Symbol;
		Brush brush = new SolidBrush(symbol.Color);
		Pen gdipPen = style.Symbol.Border.GdipPen;
		DrawPointSymbol(g, symbol.Shape, symbol.Marker, symbol.Size, symbol.Offset, symbol.ImageIndex, brush, gdipPen, style.Images, pt, drawMarker);
		brush.Dispose();
	}

	public static void DrawPointSymbol(Graphics g, ChartStyleInfo style, PointF pt, bool drawMarker, BrushInfo brushInfo)
	{
		ChartSymbolInfo symbol = style.Symbol;
		Brush brush = new SolidBrush(symbol.Color);
		RectangleF bounds = new RectangleF(pt, symbol.Size);
		brush = ChartGraph.GetBrushItem(brushInfo, bounds);
		Pen gdipPen = style.Symbol.Border.GdipPen;
		DrawPointSymbol(g, symbol.Shape, symbol.Marker, symbol.Size, symbol.Offset, symbol.ImageIndex, brush, gdipPen, style.Images, pt, drawMarker);
		brush.Dispose();
	}

	public static void DrawPointSymbol(Graphics g, ChartSymbolShape symbolShape, ChartMarker symbolMarker, Size symbolSize, Size symbolOffset, int symbolImageIndex, Brush brush, Pen pen, ChartImageCollection images, PointF pt, bool drawMarker)
	{
		SizeF symbolSize2 = symbolSize;
		DrawPointSymbol(g, symbolShape, symbolMarker, symbolSize2, symbolOffset, symbolImageIndex, brush, pen, images, pt, drawMarker);
	}

	internal static void DrawPointSymbol(Graphics g, ChartSymbolShape symbolShape, ChartMarker symbolMarker, SizeF symbolSize, Size symbolOffset, int symbolImageIndex, Brush brush, Pen pen, ChartImageCollection images, PointF pt, bool drawMarker)
	{
		if (symbolShape == ChartSymbolShape.None)
		{
			return;
		}
		symbolSize.Width *= 1.5f;
		symbolSize.Height *= 1.5f;
		if (drawMarker)
		{
			PointF p = new PointF(pt.X - (float)symbolOffset.Width + symbolSize.Width / 2f, pt.Y - (float)symbolOffset.Height + symbolSize.Height / 2f);
			DrawMarker(g, symbolMarker, p, pt);
		}
		pt.X -= symbolSize.Width / 2f;
		pt.Y -= symbolSize.Height / 2f;
		RectangleF bounds = new RectangleF(pt, symbolSize);
		switch (symbolShape)
		{
		case ChartSymbolShape.Arrow:
			DrawArrow(g, bounds, brush, pen);
			break;
		case ChartSymbolShape.InvertedArrow:
			DrawInvertedArrow(g, bounds, brush, pen);
			break;
		case ChartSymbolShape.Circle:
			DrawCircle(g, bounds, brush, pen);
			break;
		case ChartSymbolShape.Plus:
			DrawPlus(g, bounds, pen, brush);
			break;
		case ChartSymbolShape.Cross:
			DrawCross(g, bounds, pen, brush);
			break;
		case ChartSymbolShape.ExcelStar:
			DrawExcelStar(g, bounds, pen, brush);
			break;
		case ChartSymbolShape.HorizLine:
			DrawHorizLine(g, bounds, pen, brush);
			break;
		case ChartSymbolShape.DowJonesLine:
			DrawDowJonesLine(g, bounds, pen, brush);
			break;
		case ChartSymbolShape.VertLine:
			DrawVertLine(g, bounds, brush);
			break;
		case ChartSymbolShape.Diamond:
			DrawDiamond(g, bounds, brush, pen);
			break;
		case ChartSymbolShape.Hexagon:
			DrawHexagon(g, bounds, brush, pen);
			break;
		case ChartSymbolShape.InvertedTriangle:
			DrawInvertedTriangle(g, bounds, brush, pen);
			break;
		case ChartSymbolShape.Pentagon:
			DrawPentagon(g, bounds, brush, pen);
			break;
		case ChartSymbolShape.Star:
			DrawStar(g, bounds, brush, pen);
			break;
		case ChartSymbolShape.Square:
			DrawSquare(g, bounds, brush, pen);
			break;
		case ChartSymbolShape.Triangle:
			DrawTriangle(g, bounds, brush, pen);
			break;
		case ChartSymbolShape.Image:
			if (images != null && symbolImageIndex >= 0 && symbolImageIndex < images.Count)
			{
				DrawImage(g, bounds, images[symbolImageIndex]);
			}
			break;
		}
	}

	public static void DrawPointSymbol(ChartGraph graph, ChartSymbolShape symbolShape, ChartMarker symbolMarker, Size symbolSize, Size symbolOffset, int imgIndex, Brush brush, Pen pen, ChartImageCollection images, PointF point, bool drawMarker)
	{
		if (symbolShape == ChartSymbolShape.None)
		{
			return;
		}
		if (drawMarker)
		{
			PointF pointF = new PointF(point.X - (float)symbolOffset.Width + (float)(symbolSize.Width / 2), point.Y - (float)symbolOffset.Height + (float)(symbolSize.Height / 2));
			using Pen pen2 = symbolMarker.LineInfo.GdipPen.Clone() as Pen;
			pen2.EndCap = symbolMarker.LineCap;
			graph.DrawLine(pen, pointF.X, pointF.Y, point.X, point.Y);
		}
		point.X -= symbolSize.Width / 2;
		point.Y -= symbolSize.Height / 2;
		RectangleF rectangleF = new RectangleF(point, symbolSize);
		switch (symbolShape)
		{
		case ChartSymbolShape.Arrow:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathArrow(rectangleF));
			break;
		case ChartSymbolShape.InvertedArrow:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathInvertedArrow(rectangleF));
			break;
		case ChartSymbolShape.Circle:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathCircle(rectangleF));
			break;
		case ChartSymbolShape.Plus:
		{
			using Pen pen8 = new Pen(brush);
			graph.DrawRect(brush, pen8, rectangleF);
			graph.DrawLine(pen, rectangleF.X, rectangleF.Y + rectangleF.Height / 2f, rectangleF.Right, rectangleF.Y + rectangleF.Height / 2f);
			graph.DrawLine(pen, rectangleF.X + rectangleF.Width / 2f, rectangleF.Y, rectangleF.X + rectangleF.Width / 2f, rectangleF.Bottom);
			break;
		}
		case ChartSymbolShape.ExcelStar:
		{
			using Pen pen7 = new Pen(brush);
			graph.DrawRect(brush, pen7, rectangleF);
			graph.DrawLine(pen, rectangleF.Left, rectangleF.Top, rectangleF.Right, rectangleF.Bottom);
			graph.DrawLine(pen, rectangleF.Right, rectangleF.Top, rectangleF.Left, rectangleF.Bottom);
			graph.DrawLine(pen, rectangleF.Left + rectangleF.Width / 2f, rectangleF.Top, rectangleF.Left + rectangleF.Width / 2f, rectangleF.Bottom);
			break;
		}
		case ChartSymbolShape.Cross:
		{
			using Pen pen6 = new Pen(brush);
			graph.DrawRect(brush, pen6, rectangleF);
			graph.DrawLine(pen, rectangleF.Left, rectangleF.Top, rectangleF.Right, rectangleF.Bottom);
			graph.DrawLine(pen, rectangleF.Right, rectangleF.Top, rectangleF.Left, rectangleF.Bottom);
			break;
		}
		case ChartSymbolShape.HorizLine:
			using (new Pen(brush))
			{
				graph.DrawRect(brush, pen, rectangleF.Left, rectangleF.Top + rectangleF.Height / 2f, rectangleF.Width, rectangleF.Height / 5f);
				break;
			}
		case ChartSymbolShape.DowJonesLine:
			using (new Pen(brush))
			{
				graph.DrawRect(brush, pen, rectangleF.Left + rectangleF.Width / 2f, rectangleF.Top + rectangleF.Height / 2f, rectangleF.Width / 2f, rectangleF.Height / 5f);
				break;
			}
		case ChartSymbolShape.VertLine:
			using (new Pen(brush))
			{
				graph.DrawLine(pen, point.X, rectangleF.Top, point.X, rectangleF.Bottom);
				break;
			}
		case ChartSymbolShape.Diamond:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathDiamond(rectangleF));
			break;
		case ChartSymbolShape.Hexagon:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathHexagon(rectangleF));
			break;
		case ChartSymbolShape.InvertedTriangle:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathInvertedTriangle(rectangleF));
			break;
		case ChartSymbolShape.Pentagon:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathPentagon(rectangleF));
			break;
		case ChartSymbolShape.Star:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathStar(rectangleF));
			break;
		case ChartSymbolShape.Square:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathSquare(rectangleF));
			break;
		case ChartSymbolShape.Triangle:
			graph.DrawPath(brush, pen, ChartSymbolHelper.GetPathTriangle(rectangleF));
			break;
		case ChartSymbolShape.Image:
			if (images != null && imgIndex >= 0 && imgIndex < images.Count)
			{
				graph.DrawImage(images[imgIndex], rectangleF.X, rectangleF.Y, rectangleF.Width, rectangleF.Height);
			}
			break;
		}
	}

	public static void DrawImage(Graphics g, RectangleF bounds, DocGen.Drawing.Image image)
	{
		g.DrawImage(image, bounds);
	}

	public static void DrawCircle(Graphics g, RectangleF bounds, Brush brush, Pen pen)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddEllipse(bounds);
		g.FillPath(brush, graphicsPath);
		g.DrawPath(pen, graphicsPath);
	}

	public static void DrawPlus(Graphics g, RectangleF bounds, Pen pen, Brush brush)
	{
		g.FillRectangle(brush, bounds);
		g.DrawLine(pen, bounds.X, bounds.Y + bounds.Height / 2f, bounds.Right, bounds.Y + bounds.Height / 2f);
		g.DrawLine(pen, bounds.X + bounds.Width / 2f, bounds.Y, bounds.X + bounds.Width / 2f, bounds.Bottom);
	}

	public static void DrawCross(Graphics g, RectangleF bounds, Pen pen, Brush brush)
	{
		g.FillRectangle(brush, bounds);
		g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
		g.DrawLine(pen, bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
	}

	public static void DrawExcelStar(Graphics g, RectangleF bounds, Pen pen, Brush brush)
	{
		g.FillRectangle(brush, bounds);
		g.DrawLine(pen, bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
		g.DrawLine(pen, bounds.Right, bounds.Top, bounds.Left, bounds.Bottom);
		g.DrawLine(pen, bounds.Left + bounds.Width / 2f, bounds.Top, bounds.Left + bounds.Width / 2f, bounds.Bottom);
	}

	public static void DrawHorizLine(Graphics g, RectangleF bounds, Pen pen, Brush brush)
	{
		Rectangle rectangle = new Rectangle((int)bounds.X, (int)(bounds.Y + bounds.Height / 2f), (int)bounds.Width, (int)bounds.Height / 5);
		g.DrawRectangle(pen, rectangle);
		g.FillRectangle(brush, rectangle);
	}

	public static void DrawDowJonesLine(Graphics g, RectangleF bounds, Pen pen, Brush brush)
	{
		Rectangle rectangle = new Rectangle((int)(bounds.X + bounds.Width / 2f), (int)(bounds.Y + bounds.Height / 2f), (int)bounds.Width / 2, (int)bounds.Height / 5);
		g.DrawRectangle(pen, rectangle);
		g.FillRectangle(brush, rectangle);
	}

	public static void DrawVertLine(Graphics g, RectangleF bounds, Brush brush)
	{
		Pen pen = new Pen(brush);
		g.DrawLine(pen, bounds.X + bounds.Width / 2f, bounds.Y, bounds.X + bounds.Width / 2f, bounds.Bottom);
		pen.Dispose();
	}

	public static void DrawDiamond(Graphics g, RectangleF bounds, Brush brush, Pen pen)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		PointF[] linePoints = new PointF[4]
		{
			new PointF(bounds.X + bounds.Width / 2f, bounds.Y),
			new PointF(bounds.Right, bounds.Y + bounds.Height / 2f),
			new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom),
			new PointF(bounds.X, bounds.Y + bounds.Height / 2f)
		};
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseAllFigures();
		g.FillPath(brush, graphicsPath);
		g.DrawPath(pen, graphicsPath);
	}

	public static void DrawHexagon(Graphics g, RectangleF bounds, Brush brush, Pen pen)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		PointF[] linePoints = new PointF[6]
		{
			new PointF(bounds.X + bounds.Width / 4f, bounds.Y),
			new PointF(bounds.X + bounds.Width * 0.75f, bounds.Y),
			new PointF(bounds.Right, bounds.Y + bounds.Height / 2f),
			new PointF(bounds.X + bounds.Width * 0.75f, bounds.Bottom),
			new PointF(bounds.X + bounds.Width / 4f, bounds.Bottom),
			new PointF(bounds.X, bounds.Y + bounds.Height / 2f)
		};
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseAllFigures();
		g.FillPath(brush, graphicsPath);
		g.DrawPath(pen, graphicsPath);
	}

	public static void DrawInvertedTriangle(Graphics g, RectangleF bounds, Brush brush, Pen pen)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		PointF[] linePoints = new PointF[3]
		{
			new PointF(bounds.X, bounds.Y),
			new PointF(bounds.Right, bounds.Y),
			new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom)
		};
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseAllFigures();
		g.FillPath(brush, graphicsPath);
		g.DrawPath(pen, graphicsPath);
	}

	public static void DrawArrow(Graphics g, RectangleF bounds, Brush brush, Pen pen)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		_ = bounds.Height / 3f;
		float num = bounds.Width / 3f;
		PointF[] linePoints = new PointF[4]
		{
			new PointF(bounds.Left, bounds.Top),
			new PointF(bounds.Right, bounds.Top + bounds.Height / 2f),
			new PointF(bounds.Left, bounds.Bottom),
			new PointF(bounds.Right - num * 2f, bounds.Bottom - bounds.Height / 2f)
		};
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseAllFigures();
		g.FillPath(brush, graphicsPath);
		g.DrawPath(pen, graphicsPath);
	}

	public static void DrawInvertedArrow(Graphics g, RectangleF bounds, Brush brush, Pen pen)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float num = bounds.Height / 3f;
		_ = bounds.Width / 3f;
		PointF[] linePoints = new PointF[4]
		{
			new PointF(bounds.Right, bounds.Top),
			new PointF(bounds.Left, bounds.Top + bounds.Width / 2f),
			new PointF(bounds.Right, bounds.Bottom),
			new PointF(bounds.Left + num * 2f, bounds.Bottom - bounds.Width / 2f)
		};
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseAllFigures();
		g.FillPath(brush, graphicsPath);
		g.DrawPath(pen, graphicsPath);
	}

	public static void DrawPentagon(Graphics g, RectangleF bounds, Brush brush, Pen pen)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		PointF[] linePoints = new PointF[5]
		{
			new PointF(bounds.X + bounds.Width / 5f, bounds.Y),
			new PointF(bounds.X + bounds.Width - bounds.Width / 5f, bounds.Y),
			new PointF(bounds.Right, bounds.Y + bounds.Height * 0.6f),
			new PointF(bounds.X + bounds.Width / 2f, bounds.Bottom),
			new PointF(bounds.X, bounds.Y + bounds.Height * 0.6f)
		};
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseAllFigures();
		g.FillPath(brush, graphicsPath);
		g.DrawPath(pen, graphicsPath);
	}

	public static void DrawStar(Graphics g, RectangleF bounds, Brush brush, Pen pen)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		PointF[] linePoints = new PointF[10]
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
		};
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseAllFigures();
		g.FillPath(brush, graphicsPath);
		g.DrawPath(pen, graphicsPath);
	}

	public static void DrawSquare(Graphics g, RectangleF bounds, Brush brush, Pen pen)
	{
		g.FillRectangle(brush, bounds);
		g.DrawRectangle(pen, Rectangle.Round(bounds));
	}

	public static void DrawTriangle(Graphics g, RectangleF bounds, Brush brush, Pen pen)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		PointF[] linePoints = new PointF[3]
		{
			new PointF(bounds.X + bounds.Width / 2f, bounds.Y),
			new PointF(bounds.X, bounds.Bottom),
			new PointF(bounds.Right, bounds.Bottom)
		};
		graphicsPath.AddLines(linePoints);
		graphicsPath.CloseAllFigures();
		g.FillPath(brush, graphicsPath);
		g.DrawPath(pen, graphicsPath);
	}

	public static void DrawText(Graphics g, ChartStyleInfo style, PointF pt)
	{
		Brush brush = new SolidBrush(style.TextColor);
		if (style.Font.Orientation == 0)
		{
			g.DrawString(style.Text, style.Font.GdipFont, brush, pt);
		}
		else
		{
			g.MeasureString(style.Text, style.GdipFont).ToSize();
			GraphicsContainer cont = DrawingHelper.BeginTransform(g);
			g.TranslateTransform(pt.X, pt.Y);
			g.RotateTransform(style.Font.Orientation);
			g.DrawString(style.Text, style.Font.GdipFont, brush, style.TextOffset, (0f - style.GdipFont.GetHeight()) / 2f);
			DrawingHelper.EndTransform(g, cont);
		}
		brush.Dispose();
	}

	public static void DrawText(Graphics g, ChartStyleInfo style, PointF pt, SizeF sz)
	{
		Brush brush = new SolidBrush(style.TextColor);
		StringFormat format = style.Format;
		if (style.Font.Orientation == 0)
		{
			if (format != null)
			{
				g.DrawString(style.Text, style.GdipFont, brush, Rectangle.Round(new RectangleF(pt, sz)), format);
			}
			else
			{
				g.DrawString(style.Text, style.GdipFont, brush, Rectangle.Round(new RectangleF(pt, sz)), DrawingHelper.CenteredFormat);
			}
		}
		else
		{
			GraphicsContainer cont = DrawingHelper.BeginTransform(g);
			g.TranslateTransform(pt.X, pt.Y + sz.Height / 2f);
			g.RotateTransform(style.Font.Orientation);
			if (format != null)
			{
				g.DrawString(style.Text, style.GdipFont, brush, new RectangleF(0f - style.TextOffset, (0f - sz.Height) / 2f, sz.Width, sz.Height), format);
			}
			else
			{
				g.DrawString(style.Text, style.GdipFont, brush, new RectangleF(0f - style.TextOffset, (0f - sz.Height) / 2f, sz.Width, sz.Height), DrawingHelper.CenteredFormat);
			}
			DrawingHelper.EndTransform(g, cont);
		}
		brush.Dispose();
	}

	public static void AddTextPath(GraphicsPath gp, Graphics g, string text, Font font, RectangleF rect, StringFormat strFormat)
	{
		gp.AddString(text, font.GetFontFamily(), (int)font.Style, font.GetHeight(g), rect, strFormat);
	}

	public static void AddTextPath(GraphicsPath gp, Graphics g, string text, Font font, RectangleF rect)
	{
		gp.AddString(text, font.GetFontFamily(), (int)font.Style, GetFontSizeInPixels(font, g), rect, StringFormat.GenericDefault);
	}

	public static float GetFontSizeInPixels(Font font, Graphics g)
	{
		if (font.Unit != GraphicsUnit.Pixel)
		{
			return font.GetHeight(g) * font.GetFontFamily().GetEmHeight(font.Style) / font.GetFontFamily().GetLineSpacing(font.Style);
		}
		return font.Size;
	}

	public static float GetFontSizeInPixels(Font font)
	{
		if (font.Unit == GraphicsUnit.Point)
		{
			float sizeInPoints = new Font(font.GetFontName(), 10f, font.Style, GraphicsUnit.Pixel).SizeInPoints;
			return 10f * font.Size / sizeInPoints;
		}
		return font.Size;
	}

	public static RectangleF GetBounds(int index, int count, RectangleF bounds)
	{
		double num = Math.Sqrt(count);
		num = (int)((num % 1.0 != 0.0) ? (num + 1.0) : num);
		double num2 = (double)count / num;
		num2 = (int)((num2 % 1.0 != 0.0) ? (num2 + 1.0) : num2);
		int num3 = (int)((double)index % num);
		int num4 = (int)((double)index / num);
		SizeF sizeF = new SizeF((float)((double)bounds.Width / num), (float)((double)bounds.Height / num2));
		return new RectangleF(bounds.X + (float)num3 * sizeF.Width, bounds.Y + (float)num4 * sizeF.Height, sizeF.Width, sizeF.Height);
	}

	public static RectangleF GetPieBounds(int index, int count, RectangleF bounds)
	{
		index++;
		float num = bounds.Width / 2f / (float)count * (float)(count - index);
		float num2 = bounds.Height / 2f / (float)count * (float)(count - index);
		return RectangleF.Inflate(bounds, 0f - num, 0f - num2);
	}

	public static RectangleF GetPieBounds(int index, int count, RectangleF bounds, double coeff)
	{
		index++;
		if (count == index)
		{
			return bounds;
		}
		float num = bounds.Width / 2f * (1f - (float)coeff) / (float)count * (float)(count - index);
		float num2 = bounds.Height / 2f * (1f - (float)coeff) / (float)count * (float)(count - index);
		return RectangleF.Inflate(bounds, 0f - num, 0f - num2);
	}

	public static GraphicsPath CreateRoundRect(RectangleF rect, SizeF roundCorner)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float num = Math.Min(2f * roundCorner.Width, rect.Width);
		float num2 = Math.Min(2f * roundCorner.Height, rect.Height);
		if (num == 0f || num2 == 0f)
		{
			graphicsPath.AddLine(rect.Left, rect.Top, rect.Right, rect.Top);
			graphicsPath.AddLine(rect.Right, rect.Bottom, rect.Left, rect.Bottom);
			graphicsPath.CloseFigure();
		}
		else
		{
			graphicsPath.AddArc(rect.Left, rect.Top, num, num2, 180f, 90f);
			graphicsPath.AddArc(rect.Right - num, rect.Top, num, num2, 270f, 90f);
			graphicsPath.AddArc(rect.Right - num, rect.Bottom - num2, num, num2, 0f, 90f);
			graphicsPath.AddArc(rect.Left, rect.Bottom - num2, num, num2, 90f, 90f);
			graphicsPath.CloseFigure();
		}
		return graphicsPath;
	}

	public static GraphicsPath CreateRoundRect(RectangleF rect, float radius)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		float num = Math.Min(2f * radius, rect.Width);
		float num2 = Math.Min(2f * radius, rect.Height);
		if (num == 0f || num2 == 0f)
		{
			graphicsPath.AddRectangle(rect);
		}
		else if (num > 0f && num2 > 0f)
		{
			graphicsPath.AddArc(rect.Left, rect.Top, num, num2, 180f, 90f);
			graphicsPath.AddArc(rect.Right - num, rect.Top, num, num2, 270f, 90f);
			graphicsPath.AddArc(rect.Right - num, rect.Bottom - num2, num, num2, 0f, 90f);
			graphicsPath.AddArc(rect.Left, rect.Bottom - num2, num, num2, 90f, 90f);
			graphicsPath.CloseFigure();
		}
		else
		{
			graphicsPath.CloseAllFigures();
		}
		return graphicsPath;
	}

	public static GraphicsPath CreateRoundRect(RectangleF rect, float tlRadius, float trRadius, float brRadius, float blRadius)
	{
		GraphicsPath graphicsPath = new GraphicsPath();
		if (!rect.IsEmpty)
		{
			float num = tlRadius;
			float num2 = tlRadius;
			float num3 = trRadius;
			float num4 = trRadius;
			float num5 = brRadius;
			float num6 = brRadius;
			float num7 = blRadius;
			float num8 = blRadius;
			if (tlRadius + trRadius > rect.Width)
			{
				num = tlRadius * rect.Width / (tlRadius + trRadius);
				num3 = trRadius * rect.Width / (tlRadius + trRadius);
			}
			if (trRadius + brRadius > rect.Height)
			{
				num4 = trRadius * rect.Height / (trRadius + brRadius);
				num6 = brRadius * rect.Height / (trRadius + brRadius);
			}
			if (brRadius + blRadius > rect.Width)
			{
				num7 = blRadius * rect.Width / (brRadius + blRadius);
				num5 = brRadius * rect.Width / (brRadius + blRadius);
			}
			if (tlRadius + blRadius > rect.Height)
			{
				num2 = tlRadius * rect.Height / (tlRadius + blRadius);
				num8 = tlRadius * rect.Height / (tlRadius + blRadius);
			}
			if (num > 0f && num2 > 0f)
			{
				graphicsPath.AddArc(rect.Left, rect.Top, 2f * num, 2f * num2, 180f, 90f);
			}
			graphicsPath.AddLine(rect.Left + num, rect.Top, rect.Right - num3, rect.Top);
			if (num3 > 0f && num4 > 0f)
			{
				graphicsPath.AddArc(rect.Right - 2f * num3, rect.Top, 2f * num3, 2f * num4, 270f, 90f);
			}
			graphicsPath.AddLine(rect.Right, rect.Top + num4, rect.Right, rect.Bottom - num6);
			if (num5 > 0f && num6 > 0f)
			{
				graphicsPath.AddArc(rect.Right - 2f * num5, rect.Bottom - 2f * num6, 2f * num5, 2f * num6, 0f, 90f);
			}
			graphicsPath.AddLine(rect.Right - num5, rect.Bottom, rect.Left + num7, rect.Bottom);
			if (num7 > 0f && num8 > 0f)
			{
				graphicsPath.AddArc(rect.Left, rect.Bottom - 2f * num8, 2f * num7, 2f * num8, 90f, 90f);
			}
			graphicsPath.AddLine(rect.Left, rect.Bottom - num8, rect.Left, rect.Top + num2);
			graphicsPath.CloseFigure();
		}
		return graphicsPath;
	}

	public static PointF[] GetRendomBeziersPoints(PointF pt1, PointF pt2, float evr, float fault)
	{
		Random random = new Random((int)(evr * fault));
		float num = pt2.X - pt1.X;
		float num2 = pt2.Y - pt1.Y;
		float num3 = (float)Math.Sqrt(num * num + num2 * num2);
		int num4 = (int)Math.Max(4.0, 3.0 * Math.Round(num3 / evr) + 1.0);
		float num5 = num2 / num3;
		float num6 = num / num3;
		float num7 = num / (float)(num4 - 1);
		float num8 = num2 / (float)(num4 - 1);
		PointF[] array = new PointF[num4];
		array[0] = pt1;
		array[num4 - 1] = pt2;
		for (int i = 1; i < num4 - 1; i++)
		{
			float num9 = (float)((double)(2f * fault) * random.NextDouble() - (double)fault);
			array[i] = new PointF(pt1.X + (float)i * num7 + num9 * num5, pt1.Y + (float)i * num8 + num9 * num6);
		}
		return array;
	}

	public static PointF[] GetRendomBeziersPoints(PointF pt1, PointF pt2, int count, float fault)
	{
		Random random = new Random(10);
		int num = 3 * count + 1;
		float num2 = pt2.X - pt1.X;
		float num3 = pt2.Y - pt1.Y;
		float num4 = (float)Math.Sqrt(num2 * num2 + num3 * num3);
		float num5 = num3 / num4;
		float num6 = num2 / num4;
		float num7 = num2 / (float)(num - 1);
		float num8 = num3 / (float)(num - 1);
		PointF[] array = new PointF[num];
		array[0] = pt1;
		array[num - 1] = pt2;
		for (int i = 1; i < num - 1; i++)
		{
			float num9 = (float)((double)(2f * fault) * random.NextDouble() - (double)fault);
			array[i] = new PointF(pt1.X + (float)i * num7 + num9 * num5, pt1.Y + (float)i * num8 + num9 * num6);
		}
		return array;
	}

	public static PointF[] GetWaveBeziersPoints(PointF pt1, PointF pt2, float evr, float fault)
	{
		Random random = new Random((int)(evr * fault));
		float num = pt2.X - pt1.X;
		float num2 = pt2.Y - pt1.Y;
		float num3 = (float)Math.Sqrt(num * num + num2 * num2);
		int num4 = (int)Math.Max(4.0, 3.0 * Math.Round(num3 / evr) + 1.0);
		float num5 = num2 / num3;
		float num6 = num / num3;
		float num7 = num / (float)(num4 - 1);
		float num8 = num2 / (float)(num4 - 1);
		PointF[] array = new PointF[num4];
		array[0] = pt1;
		array[num4 - 1] = pt2;
		for (int i = 1; i < num4 - 1; i++)
		{
			float num9 = (float)((double)(2f * fault) * random.NextDouble() - (double)fault);
			array[i] = new PointF(pt1.X + (float)i * num7 + num9 * num5, pt1.Y + (float)i * num8 + num9 * num6);
		}
		return array;
	}

	public static PointF[] GetWaveBeziersPoints(PointF pt1, PointF pt2, int count, float fault)
	{
		float num = pt2.X - pt1.X;
		float num2 = pt2.Y - pt1.Y;
		float num3 = (float)Math.Sqrt(num * num + num2 * num2);
		float num4 = fault * num2 / num3;
		float num5 = fault * num / num3;
		float num6 = num / (float)count;
		float num7 = num2 / (float)count;
		PointF[] array = new PointF[3 * count + 1];
		for (int i = 0; i < count; i++)
		{
			array[3 * i] = new PointF(pt1.X + num6 * (float)i, pt1.Y + num7 * (float)i);
			array[3 * i + 1] = new PointF(pt1.X + num6 * ((float)i + 0.5f) + num4, pt1.Y + num7 * ((float)i + 0.5f) + num5);
			array[3 * i + 2] = new PointF(pt1.X + num6 * ((float)i + 0.5f) - num4, pt1.Y + num7 * ((float)i + 0.5f) - num5);
		}
		array[^1] = pt2;
		return array;
	}
}
