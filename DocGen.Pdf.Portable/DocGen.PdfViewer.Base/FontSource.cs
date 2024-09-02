namespace DocGen.PdfViewer.Base;

internal abstract class FontSource
{
	public abstract string FontFamily { get; }

	public abstract bool IsBold { get; }

	public abstract bool IsItalic { get; }

	public abstract short Ascender { get; }

	public abstract short Descender { get; }

	public FontSource()
	{
	}

	public virtual void GetAdvancedWidth(Glyph glyph)
	{
	}

	public virtual void GetGlyphOutlines(Glyph glyph, double fontSize)
	{
	}

	public virtual void GetGlyphName(Glyph glyph)
	{
	}
}
