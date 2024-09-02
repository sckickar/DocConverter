using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontOperator
{
	internal static Point CalculatePoint(SystemFontBuildChar interpreter, int dx, int dy)
	{
		interpreter.CurrentPoint = new Point(interpreter.CurrentPoint.X + (double)dx, interpreter.CurrentPoint.Y + (double)dy);
		return new Point(interpreter.CurrentPoint.X, interpreter.CurrentPoint.Y);
	}

	internal static void HLineTo(SystemFontBuildChar interpreter, int dx)
	{
		LineTo(interpreter, dx, 0);
	}

	internal static void VLineTo(SystemFontBuildChar interpreter, int dy)
	{
		LineTo(interpreter, 0, dy);
	}

	internal static void LineTo(SystemFontBuildChar interpreter, int dx, int dy)
	{
		SystemFontLineSegment systemFontLineSegment = new SystemFontLineSegment();
		systemFontLineSegment.Point = CalculatePoint(interpreter, dx, dy);
		interpreter.CurrentPathFigure.Segments.Add(systemFontLineSegment);
	}

	internal static void CurveTo(SystemFontBuildChar interpreter, int dxa, int dya, int dxb, int dyb, int dxc, int dyc)
	{
		SystemFontBezierSegment systemFontBezierSegment = new SystemFontBezierSegment();
		systemFontBezierSegment.Point1 = CalculatePoint(interpreter, dxa, dya);
		systemFontBezierSegment.Point2 = CalculatePoint(interpreter, dxb, dyb);
		systemFontBezierSegment.Point3 = CalculatePoint(interpreter, dxc, dyc);
		interpreter.CurrentPathFigure.Segments.Add(systemFontBezierSegment);
	}

	internal static void MoveTo(SystemFontBuildChar interpreter, int dx, int dy)
	{
		interpreter.CurrentPathFigure = new SystemFontPathFigure();
		interpreter.CurrentPathFigure.IsClosed = true;
		interpreter.CurrentPathFigure.IsFilled = true;
		interpreter.CurrentPathFigure.StartPoint = CalculatePoint(interpreter, dx, dy);
		interpreter.GlyphOutlines.Add(interpreter.CurrentPathFigure);
	}

	internal static void ReadWidth(SystemFontBuildChar interpreter, int operands)
	{
		if (!interpreter.Width.HasValue)
		{
			if (interpreter.Operands.Count == operands + 1)
			{
				interpreter.Width = interpreter.Operands.GetFirstAsInt();
			}
			else
			{
				interpreter.Width = 0;
			}
		}
	}

	public abstract void Execute(SystemFontBuildChar buildChar);
}
