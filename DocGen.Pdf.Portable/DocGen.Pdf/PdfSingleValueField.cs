using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Parsing;

namespace DocGen.Pdf;

public abstract class PdfSingleValueField : PdfDynamicField
{
	private Dictionary<PdfDocumentBase, PdfTemplateValuePair> m_list = new Dictionary<PdfDocumentBase, PdfTemplateValuePair>();

	private List<PdfGraphics> m_painterGraphics = new List<PdfGraphics>();

	public PdfSingleValueField()
	{
	}

	public PdfSingleValueField(PdfFont font)
		: base(font)
	{
	}

	public PdfSingleValueField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfSingleValueField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	protected internal override void PerformDraw(PdfGraphics graphics, PointF location, float scalingX, float scalingY)
	{
		if (graphics.Page is PdfPage)
		{
			base.PerformDraw(graphics, location, scalingX, scalingY);
			PdfPage pageFromGraphics = PdfDynamicField.GetPageFromGraphics(graphics);
			if (pageFromGraphics.Section.m_document is PdfLoadedDocument)
			{
				PdfLoadedDocument key = pageFromGraphics.Section.m_document as PdfLoadedDocument;
				base.PerformDraw(graphics, location, scalingX, scalingY);
				PdfDynamicField.GetPageFromGraphics(graphics);
				PdfLoadedDocument key2 = pageFromGraphics.Section.m_document as PdfLoadedDocument;
				string value = GetValue(graphics);
				if (m_list.ContainsKey(key2))
				{
					PdfTemplateValuePair pdfTemplateValuePair = m_list[key];
					if (pdfTemplateValuePair.Value != value)
					{
						SizeF size = ObtainSize();
						pdfTemplateValuePair.Template.Reset(size);
						pdfTemplateValuePair.Template.Graphics.DrawString(value, ObtainFont(), base.Pen, ObtainBrush(), new RectangleF(PointF.Empty, size), base.StringFormat);
					}
					if (!m_painterGraphics.Contains(graphics))
					{
						PointF location2 = new PointF(location.X + base.Location.X, location.Y + base.Location.Y);
						graphics.DrawPdfTemplate(pdfTemplateValuePair.Template, location2, new SizeF(pdfTemplateValuePair.Template.Width * scalingX, pdfTemplateValuePair.Template.Height * scalingY));
						m_painterGraphics.Add(graphics);
					}
				}
				else
				{
					PdfTemplate pdfTemplate = new PdfTemplate(ObtainSize());
					m_list[key] = new PdfTemplateValuePair(pdfTemplate, value);
					pdfTemplate.Graphics.DrawString(value, ObtainFont(), base.Pen, ObtainBrush(), new RectangleF(PointF.Empty, ObtainSize()), base.StringFormat);
					PointF location3 = new PointF(location.X + base.Location.X, location.Y + base.Location.Y);
					graphics.DrawPdfTemplate(pdfTemplate, location3, new SizeF(pdfTemplate.Width * scalingX, pdfTemplate.Height * scalingY));
					m_painterGraphics.Add(graphics);
				}
				return;
			}
			PdfDocument document = pageFromGraphics.Document;
			string value2 = GetValue(graphics);
			if (m_list.ContainsKey(document))
			{
				PdfTemplateValuePair pdfTemplateValuePair2 = m_list[document];
				if (pdfTemplateValuePair2.Value != value2)
				{
					SizeF size2 = ObtainSize();
					pdfTemplateValuePair2.Template.Reset(size2);
					pdfTemplateValuePair2.Template.Graphics.DrawString(value2, ObtainFont(), base.Pen, ObtainBrush(), new RectangleF(PointF.Empty, size2), base.StringFormat);
				}
				if (!m_painterGraphics.Contains(graphics))
				{
					PointF location4 = new PointF(location.X + base.Location.X, location.Y + base.Location.Y);
					graphics.DrawPdfTemplate(pdfTemplateValuePair2.Template, location4, new SizeF(pdfTemplateValuePair2.Template.Width * scalingX, pdfTemplateValuePair2.Template.Height * scalingY));
					m_painterGraphics.Add(graphics);
				}
			}
			else
			{
				PdfTemplate pdfTemplate2 = new PdfTemplate(ObtainSize());
				m_list[document] = new PdfTemplateValuePair(pdfTemplate2, value2);
				pdfTemplate2.Graphics.DrawString(value2, ObtainFont(), base.Pen, ObtainBrush(), new RectangleF(PointF.Empty, ObtainSize()), base.StringFormat);
				PointF location5 = new PointF(location.X + base.Location.X, location.Y + base.Location.Y);
				graphics.DrawPdfTemplate(pdfTemplate2, location5, new SizeF(pdfTemplate2.Width * scalingX, pdfTemplate2.Height * scalingY));
				m_painterGraphics.Add(graphics);
			}
		}
		else
		{
			if (!(graphics.Page is PdfLoadedPage))
			{
				return;
			}
			base.PerformDraw(graphics, location, scalingX, scalingY);
			PdfLoadedDocument key3 = PdfDynamicField.GetLoadedPageFromGraphics(graphics).Document as PdfLoadedDocument;
			string value3 = GetValue(graphics);
			if (m_list.ContainsKey(key3))
			{
				PdfTemplateValuePair pdfTemplateValuePair3 = m_list[key3];
				if (pdfTemplateValuePair3.Value != value3)
				{
					SizeF size3 = ObtainSize();
					pdfTemplateValuePair3.Template.Reset(size3);
					pdfTemplateValuePair3.Template.Graphics.DrawString(value3, ObtainFont(), base.Pen, ObtainBrush(), new RectangleF(PointF.Empty, size3), base.StringFormat);
				}
				if (!m_painterGraphics.Contains(graphics))
				{
					PointF location6 = new PointF(location.X + base.Location.X, location.Y + base.Location.Y);
					graphics.DrawPdfTemplate(pdfTemplateValuePair3.Template, location6, new SizeF(pdfTemplateValuePair3.Template.Width * scalingX, pdfTemplateValuePair3.Template.Height * scalingY));
					m_painterGraphics.Add(graphics);
				}
			}
			else
			{
				PdfTemplate pdfTemplate3 = new PdfTemplate(ObtainSize());
				m_list[key3] = new PdfTemplateValuePair(pdfTemplate3, value3);
				pdfTemplate3.Graphics.DrawString(value3, ObtainFont(), base.Pen, ObtainBrush(), new RectangleF(PointF.Empty, ObtainSize()), base.StringFormat);
				PointF location7 = new PointF(location.X + base.Location.X, location.Y + base.Location.Y);
				graphics.DrawPdfTemplate(pdfTemplate3, location7, new SizeF(pdfTemplate3.Width * scalingX, pdfTemplate3.Height * scalingY));
				m_painterGraphics.Add(graphics);
			}
		}
	}
}
