namespace DocGen.Pdf.Parsing;

internal abstract class SystemFontPathSegment
{
	public abstract SystemFontPathSegment Clone();

	public abstract void Transform(SystemFontMatrix transformMatrix);
}
