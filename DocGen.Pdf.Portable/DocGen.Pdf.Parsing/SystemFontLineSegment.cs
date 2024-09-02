using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontLineSegment : SystemFontPathSegment
{
	public Point Point { get; set; }

	public override SystemFontPathSegment Clone()
	{
		return new SystemFontLineSegment
		{
			Point = Point
		};
	}

	public override void Transform(SystemFontMatrix transformMatrix)
	{
		Point = transformMatrix.Transform(Point);
	}
}
