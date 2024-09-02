using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class Glyph
{
	internal ushort GlyphId { get; set; }

	public GlyphOutlinesCollection Outlines { get; set; }

	public double AdvancedWidth { get; set; }

	public Point HorizontalKerning { get; set; }

	public Point VerticalKerning { get; set; }

	public string Name { get; set; }
}
