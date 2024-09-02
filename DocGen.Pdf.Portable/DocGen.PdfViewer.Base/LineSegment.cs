namespace DocGen.PdfViewer.Base;

internal class LineSegment : PathSegment
{
	public Point Point { get; set; }

	public override PathSegment Clone()
	{
		return new LineSegment
		{
			Point = Point
		};
	}

	public override void Transform(Matrix transformMatrix)
	{
		Point = transformMatrix.Transform(Point);
	}
}
