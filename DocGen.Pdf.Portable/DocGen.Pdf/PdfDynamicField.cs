using System;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public abstract class PdfDynamicField : PdfAutomaticField
{
	public PdfDynamicField()
	{
	}

	public PdfDynamicField(PdfFont font)
		: base(font)
	{
	}

	public PdfDynamicField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfDynamicField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	internal static PdfPage GetPageFromGraphics(PdfGraphics graphics)
	{
		return (graphics.Page as PdfPage) ?? throw new NotSupportedException("The field was placed on not PdfPage class instance.");
	}

	internal static PdfLoadedPage GetLoadedPageFromGraphics(PdfGraphics graphics)
	{
		return (graphics.Page as PdfLoadedPage) ?? throw new NotSupportedException("The field was placed on not PdfPage class instance.");
	}
}
