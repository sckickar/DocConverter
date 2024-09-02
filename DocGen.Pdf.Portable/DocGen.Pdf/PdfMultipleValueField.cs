using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public abstract class PdfMultipleValueField : PdfDynamicField
{
	private Dictionary<PdfGraphics, PdfTemplateValuePair> m_list = new Dictionary<PdfGraphics, PdfTemplateValuePair>();

	private PdfTag m_tag;

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

	public PdfMultipleValueField()
	{
	}

	public PdfMultipleValueField(PdfFont font)
		: base(font)
	{
	}

	public PdfMultipleValueField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfMultipleValueField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	protected internal override void PerformDraw(PdfGraphics graphics, PointF location, float scalingX, float scalingY)
	{
		base.PerformDraw(graphics, location, scalingX, scalingY);
		string value = GetValue(graphics);
		if (m_list.ContainsKey(graphics))
		{
			PdfTemplateValuePair pdfTemplateValuePair = m_list[graphics];
			if (pdfTemplateValuePair.Value != value)
			{
				SizeF size = ObtainSize();
				pdfTemplateValuePair.Template.Reset(size);
				pdfTemplateValuePair.Template.Graphics.DrawString(value, ObtainFont(), base.Pen, ObtainBrush(), new RectangleF(PointF.Empty, size), base.StringFormat);
			}
			return;
		}
		PdfTemplate pdfTemplate = new PdfTemplate(ObtainSize());
		if (PdfTag != null)
		{
			pdfTemplate.Graphics.Tag = PdfTag;
		}
		m_list[graphics] = new PdfTemplateValuePair(pdfTemplate, value);
		pdfTemplate.Graphics.ColorSpace = graphics.ColorSpace;
		pdfTemplate.Graphics.DrawString(value, ObtainFont(), base.Pen, ObtainBrush(), new RectangleF(PointF.Empty, ObtainSize()), base.StringFormat);
		PointF location2 = new PointF(location.X + base.Location.X, location.Y + base.Location.Y);
		if (graphics.IsTaggedPdf && pdfTemplate.Graphics.Tag == null)
		{
			pdfTemplate.Graphics.Tag = new PdfArtifact();
		}
		graphics.DrawPdfTemplate(pdfTemplate, location2, new SizeF(pdfTemplate.Width * scalingX, pdfTemplate.Height * scalingY));
	}
}
