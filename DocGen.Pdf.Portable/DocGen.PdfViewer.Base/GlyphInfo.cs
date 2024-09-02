namespace DocGen.PdfViewer.Base;

internal class GlyphInfo
{
	public GlyphOutlinesCollection Oultlines { get; private set; }

	public ushort AdvancedWidth { get; private set; }

	public bool HasWidth { get; private set; }

	public GlyphInfo(GlyphOutlinesCollection outlines, ushort? width)
	{
		Oultlines = outlines;
		if (width.HasValue)
		{
			AdvancedWidth = width.Value;
			HasWidth = true;
		}
	}
}
