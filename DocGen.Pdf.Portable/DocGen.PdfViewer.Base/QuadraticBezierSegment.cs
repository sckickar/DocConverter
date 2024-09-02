namespace DocGen.PdfViewer.Base;

internal class QuadraticBezierSegment : PathSegment
{
	public Point Point1 { get; set; }

	public Point Point2 { get; set; }

	public override PathSegment Clone()
	{
		return new QuadraticBezierSegment
		{
			Point1 = Point1,
			Point2 = Point2
		};
	}

	public override void Transform(Matrix transformMatrix)
	{
		Point1 = transformMatrix.Transform(Point1);
		Point2 = transformMatrix.Transform(Point2);
	}
}
