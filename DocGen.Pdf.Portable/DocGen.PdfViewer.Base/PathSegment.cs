namespace DocGen.PdfViewer.Base;

internal abstract class PathSegment
{
	public abstract PathSegment Clone();

	public abstract void Transform(Matrix transformMatrix);
}
