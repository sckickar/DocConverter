using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontBezierSegment : SystemFontPathSegment
{
	public Point Point1 { get; set; }

	public Point Point2 { get; set; }

	public Point Point3 { get; set; }

	public override SystemFontPathSegment Clone()
	{
		return new SystemFontBezierSegment
		{
			Point1 = Point1,
			Point2 = Point2,
			Point3 = Point3
		};
	}

	public override void Transform(SystemFontMatrix transformMatrix)
	{
		Point1 = transformMatrix.Transform(Point1);
		Point2 = transformMatrix.Transform(Point2);
		Point3 = transformMatrix.Transform(Point3);
	}
}
