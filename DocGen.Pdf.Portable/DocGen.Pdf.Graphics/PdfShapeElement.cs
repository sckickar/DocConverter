using System;
using DocGen.Drawing;
using DocGen.Pdf.HtmlToPdf;

namespace DocGen.Pdf.Graphics;

public abstract class PdfShapeElement : PdfLayoutElement
{
	public RectangleF GetBounds()
	{
		return GetBoundsInternal();
	}

	protected abstract RectangleF GetBoundsInternal();

	protected override PdfLayoutResult Layout(PdfLayoutParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		return new ShapeLayouter(this).Layout(param);
	}

	internal override PdfLayoutResult Layout(HtmlToPdfParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		return new ShapeLayouter(this).Layout(param);
	}
}
