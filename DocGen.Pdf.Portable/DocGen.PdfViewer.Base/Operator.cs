namespace DocGen.PdfViewer.Base;

internal abstract class Operator
{
	internal static Point CalculatePoint(CharacterBuilder interpreter, int dx, int dy)
	{
		interpreter.CurrentPoint = new Point(interpreter.CurrentPoint.X + (double)dx, interpreter.CurrentPoint.Y + (double)dy);
		return new Point(interpreter.CurrentPoint.X, interpreter.CurrentPoint.Y);
	}

	internal static void HLineTo(CharacterBuilder interpreter, int dx)
	{
		LineTo(interpreter, dx, 0);
	}

	internal static void VLineTo(CharacterBuilder interpreter, int dy)
	{
		LineTo(interpreter, 0, dy);
	}

	internal static void LineTo(CharacterBuilder interpreter, int dx, int dy)
	{
		LineSegment lineSegment = new LineSegment();
		lineSegment.Point = CalculatePoint(interpreter, dx, dy);
		interpreter.CurrentPathFigure.Segments.Add(lineSegment);
	}

	internal static void CurveTo(CharacterBuilder interpreter, int dxa, int dya, int dxb, int dyb, int dxc, int dyc)
	{
		BezierSegment bezierSegment = new BezierSegment();
		bezierSegment.Point1 = CalculatePoint(interpreter, dxa, dya);
		bezierSegment.Point2 = CalculatePoint(interpreter, dxb, dyb);
		bezierSegment.Point3 = CalculatePoint(interpreter, dxc, dyc);
		interpreter.CurrentPathFigure.Segments.Add(bezierSegment);
	}

	internal static void MoveTo(CharacterBuilder interpreter, int dx, int dy)
	{
		interpreter.CurrentPathFigure = new PathFigure();
		interpreter.CurrentPathFigure.IsClosed = true;
		interpreter.CurrentPathFigure.IsFilled = true;
		interpreter.CurrentPathFigure.StartPoint = CalculatePoint(interpreter, dx, dy);
		interpreter.GlyphOutlines.Add(interpreter.CurrentPathFigure);
	}

	internal static void ReadWidth(CharacterBuilder interpreter, int operands)
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

	public abstract void Execute(CharacterBuilder buildChar);
}
