using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontQuadraticBezierSegment : SystemFontPathSegment
{
	public Point Point1 { get; set; }

	public Point Point2 { get; set; }

	public override SystemFontPathSegment Clone()
	{
		return new SystemFontQuadraticBezierSegment
		{
			Point1 = Point1,
			Point2 = Point2
		};
	}

	public override void Transform(SystemFontMatrix transformMatrix)
	{
		Point1 = transformMatrix.Transform(Point1);
		Point2 = transformMatrix.Transform(Point2);
	}
}
