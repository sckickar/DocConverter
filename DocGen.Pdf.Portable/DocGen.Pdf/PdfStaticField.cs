using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

public abstract class PdfStaticField : PdfAutomaticField
{
	private PdfTemplate m_template;

	private List<PdfGraphics> m_graphicsList = new List<PdfGraphics>();

	public PdfStaticField()
	{
	}

	public PdfStaticField(PdfFont font)
		: base(font)
	{
	}

	public PdfStaticField(PdfFont font, PdfBrush brush)
		: base(font, brush)
	{
	}

	public PdfStaticField(PdfFont font, RectangleF bounds)
		: base(font, bounds)
	{
	}

	protected internal override void PerformDraw(PdfGraphics graphics, PointF location, float scalingX, float scalingY)
	{
		base.PerformDraw(graphics, location, scalingX, scalingY);
		string value = GetValue(graphics);
		PointF location2 = new PointF(location.X + base.Location.X, location.Y + base.Location.Y);
		if (m_template == null)
		{
			m_template = new PdfTemplate(ObtainSize());
			m_template.Graphics.DrawString(value, ObtainFont(), base.Pen, ObtainBrush(), new RectangleF(PointF.Empty, ObtainSize()), base.StringFormat);
			graphics.DrawPdfTemplate(m_template, location2, new SizeF(m_template.Width * scalingX, m_template.Height * scalingY));
			m_graphicsList.Add(graphics);
		}
		else if (!m_graphicsList.Contains(graphics))
		{
			graphics.DrawPdfTemplate(m_template, location2, new SizeF(m_template.Width * scalingX, m_template.Height * scalingY));
			m_graphicsList.Add(graphics);
		}
	}
}
