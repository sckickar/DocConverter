using DocGen.Drawing;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class SystemFontGlyph
{
	private double descent;

	internal ushort GlyphId { get; set; }

	public CharCode CharId { get; set; }

	public SystemFontGlyphOutlinesCollection Outlines { get; set; }

	public double AdvancedWidth { get; set; }

	public DocGen.PdfViewer.Base.Point HorizontalKerning { get; set; }

	public DocGen.PdfViewer.Base.Point VerticalKerning { get; set; }

	public string Name { get; set; }

	public int FontId { get; set; }

	public string ToUnicode { get; set; }

	public string FontFamily { get; set; }

	public FontStyle FontStyle { get; set; }

	public bool IsBold { get; set; }

	public bool IsItalic { get; set; }

	public double FontSize { get; set; }

	public double Rise { get; set; }

	public double CharSpacing { get; set; }

	public double WordSpacing { get; set; }

	public double HorizontalScaling { get; set; }

	public double Width { get; set; }

	public double Ascent { get; set; }

	public double Descent
	{
		get
		{
			return descent;
		}
		set
		{
			if (value > 0.0)
			{
				descent = 0.0 - value;
			}
			else
			{
				descent = value;
			}
		}
	}

	public bool IsFilled { get; set; }

	public bool IsStroked { get; set; }

	public SystemFontMatrix TransformMatrix { get; set; }

	public DocGen.PdfViewer.Base.Size Size { get; set; }

	public Rect BoundingRect { get; set; }

	public int ZIndex { get; set; }

	public SystemFontPathGeometry Clip { get; set; }

	public bool HasChildren => false;

	public double StrokeThickness { get; set; }

	public SystemFontGlyph()
	{
		Ascent = 1000.0;
		Descent = 0.0;
		TransformMatrix = SystemFontMatrix.Identity;
	}

	public override string ToString()
	{
		return ToUnicode;
	}
}
