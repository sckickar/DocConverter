namespace DocGen.Pdf.Parsing;

internal class SystemFontType1GlyphData
{
	public SystemFontGlyphOutlinesCollection Oultlines { get; private set; }

	public ushort AdvancedWidth { get; private set; }

	public bool HasWidth { get; private set; }

	public SystemFontType1GlyphData(SystemFontGlyphOutlinesCollection outlines, ushort? width)
	{
		Oultlines = outlines;
		if (width.HasValue)
		{
			AdvancedWidth = width.Value;
			HasWidth = true;
		}
	}
}
