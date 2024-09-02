using System;
using DocGen.Drawing;
using DocGen.Pdf.HtmlToPdf;

namespace DocGen.Pdf.Graphics;

public abstract class PdfLayoutElement : PdfGraphicsElement
{
	private bool m_bEmbedFonts;

	private PdfTag m_tag;

	internal bool RaiseEndPageLayout => this.EndPageLayout != null;

	internal bool RaiseBeginPageLayout => this.BeginPageLayout != null;

	internal bool EmbedFontResource => m_bEmbedFonts;

	public PdfTag PdfTag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	public event EndPageLayoutEventHandler EndPageLayout;

	public event BeginPageLayoutEventHandler BeginPageLayout;

	public PdfLayoutResult Draw(PdfPage page, PointF location)
	{
		return Draw(page, location.X, location.Y);
	}

	public PdfLayoutResult Draw(PdfPage page, float x, float y)
	{
		return Draw(page, x, y, null);
	}

	public PdfLayoutResult Draw(PdfPage page, RectangleF layoutRectangle)
	{
		return Draw(page, layoutRectangle, null);
	}

	internal PdfLayoutResult Draw(PdfPage page, RectangleF layoutRectangle, bool embedFonts)
	{
		m_bEmbedFonts = embedFonts;
		return Draw(page, layoutRectangle, null);
	}

	public PdfLayoutResult Draw(PdfPage page, PointF location, PdfLayoutFormat format)
	{
		return Draw(page, location.X, location.Y, format);
	}

	public PdfLayoutResult Draw(PdfPage page, float x, float y, PdfLayoutFormat format)
	{
		RectangleF layoutRectangle = new RectangleF(x, y, 0f, 0f);
		return Draw(page, layoutRectangle, format);
	}

	public PdfLayoutResult Draw(PdfPage page, RectangleF layoutRectangle, PdfLayoutFormat format)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		PdfLayoutParams pdfLayoutParams = new PdfLayoutParams();
		pdfLayoutParams.Page = page;
		pdfLayoutParams.Bounds = layoutRectangle;
		pdfLayoutParams.Format = ((format != null) ? format : new PdfLayoutFormat());
		return Layout(pdfLayoutParams);
	}

	internal virtual PdfLayoutResult Layout(HtmlToPdfParams param)
	{
		return null;
	}

	internal PdfLayoutResult Draw(PdfPage page, HtmlToPdfFormat format, RectangleF layoutRectangle)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page cannot be null");
		}
		HtmlToPdfParams htmlToPdfParams = new HtmlToPdfParams();
		htmlToPdfParams.Page = page;
		htmlToPdfParams.Bounds = layoutRectangle;
		htmlToPdfParams.Format = ((format != null) ? format : new HtmlToPdfFormat());
		if (htmlToPdfParams.Format.Layout == PdfLayoutType.OnePage)
		{
			htmlToPdfParams.SinglePageLayout = true;
		}
		return Layout(htmlToPdfParams);
	}

	internal PdfLayoutResult Draw(PdfPage page, RectangleF bounds, float[] pageOffsets, PdfLayoutFormat format)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		HtmlToPdfLayoutParams htmlToPdfLayoutParams = new HtmlToPdfLayoutParams();
		htmlToPdfLayoutParams.VerticalOffsets = pageOffsets;
		htmlToPdfLayoutParams.Page = page;
		htmlToPdfLayoutParams.Bounds = bounds;
		htmlToPdfLayoutParams.Format = ((format != null) ? format : new PdfLayoutFormat());
		return Layout(htmlToPdfLayoutParams);
	}

	protected abstract PdfLayoutResult Layout(PdfLayoutParams param);

	protected virtual PdfLayoutResult Layout(HtmlToPdfLayoutParams param)
	{
		return null;
	}

	internal void OnEndPageLayout(EndPageLayoutEventArgs e)
	{
		if (this.EndPageLayout != null)
		{
			this.EndPageLayout(this, e);
		}
	}

	internal void OnBeginPageLayout(BeginPageLayoutEventArgs e)
	{
		if (this.BeginPageLayout != null)
		{
			this.BeginPageLayout(this, e);
		}
	}
}
