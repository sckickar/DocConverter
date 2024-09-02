using DocGen.Drawing;

namespace DocGen.Pdf.Parsing;

internal class SystemFontOpenTypeGlyphScaler
{
	internal const double Dpi = 72.0;

	private const double Ppi = 72.0;

	private readonly SystemFontOpenTypeFontSourceBase fontFile;

	public SystemFontOpenTypeGlyphScaler(SystemFontOpenTypeFontSourceBase fontFile)
	{
		this.fontFile = fontFile;
	}

	public void ScaleGlyphMetrics(SystemFontGlyph glyph, double fontSize)
	{
		glyph.AdvancedWidth = FUnitsToPixels(fontFile.HMtx.GetAdvancedWidth(glyph.GlyphId), fontSize);
	}

	public double FUnitsToPixels(int units, double fontSize)
	{
		return FUnitsToPixels((double)units, fontSize);
	}

	public double FUnitsToPixels(double units, double fontSize)
	{
		return units * 72.0 * fontSize / (72.0 * (double)(int)fontFile.Head.UnitsPerEm);
	}

	public PointF FUnitsOutlinePointToPixels(Point unitPoint, double fontSize)
	{
		double num = FUnitsToPixels(unitPoint.X, fontSize);
		double num2 = FUnitsToPixels(unitPoint.Y, fontSize);
		return new PointF((float)num, (float)(0.0 - num2));
	}

	public Point FUnitsPointToPixels(Point unitPoint, double fontSize)
	{
		double num = FUnitsToPixels(unitPoint.X, fontSize);
		double num2 = FUnitsToPixels(unitPoint.Y, fontSize);
		return new Point((int)num, (int)num2);
	}

	public void GetScaleGlyphOutlines(SystemFontGlyph glyph, double fontSize)
	{
		switch (fontFile.Outlines)
		{
		case SystemFontOutlines.TrueType:
			CreateTrueTypeOutlines(glyph, fontSize);
			break;
		case SystemFontOutlines.OpenType:
			CreateOpenTypeOutlines(glyph, fontSize);
			break;
		}
	}

	private static Point GetMidPoint(Point a, Point b)
	{
		return new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);
	}

	private static SystemFontLineSegment CreateLineSegment(PointF point)
	{
		return new SystemFontLineSegment
		{
			Point = point
		};
	}

	private static SystemFontQuadraticBezierSegment CreateBezierSegment(PointF control, PointF end)
	{
		return new SystemFontQuadraticBezierSegment
		{
			Point1 = control,
			Point2 = end
		};
	}

	private SystemFontPathFigure CreatePathFigureFromContour(SystemFontOutlinePoint[] points, double fontSize)
	{
		SystemFontPathFigure systemFontPathFigure = new SystemFontPathFigure();
		systemFontPathFigure.StartPoint = FUnitsOutlinePointToPixels(points[0].Point, fontSize);
		for (int i = 1; i < points.Length; i++)
		{
			if (points[i].IsOnCurve)
			{
				systemFontPathFigure.Segments.Add(CreateLineSegment(FUnitsOutlinePointToPixels(points[i].Point, fontSize)));
			}
			else if (points[(i + 1) % points.Length].IsOnCurve)
			{
				systemFontPathFigure.Segments.Add(CreateBezierSegment(FUnitsOutlinePointToPixels(points[i].Point, fontSize), FUnitsOutlinePointToPixels(points[(i + 1) % points.Length].Point, fontSize)));
				i++;
			}
			else
			{
				systemFontPathFigure.Segments.Add(CreateBezierSegment(FUnitsOutlinePointToPixels(points[i].Point, fontSize), FUnitsOutlinePointToPixels(GetMidPoint(points[i].Point, points[(i + 1) % points.Length].Point), fontSize)));
			}
		}
		systemFontPathFigure.IsClosed = true;
		systemFontPathFigure.IsFilled = true;
		return systemFontPathFigure;
	}

	private void CreateOpenTypeOutlines(SystemFontGlyph glyph, double fontSize)
	{
		fontFile.CFF.GetGlyphOutlines(glyph, fontSize);
	}

	private void CreateTrueTypeOutlines(SystemFontGlyph glyph, double fontSize)
	{
		glyph.Outlines = new SystemFontGlyphOutlinesCollection();
		foreach (SystemFontOutlinePoint[] contour in fontFile.GetGlyphData(glyph.GlyphId).Contours)
		{
			glyph.Outlines.Add(CreatePathFigureFromContour(contour, fontSize));
		}
	}
}
