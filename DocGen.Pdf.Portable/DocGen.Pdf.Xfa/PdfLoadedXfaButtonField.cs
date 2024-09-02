using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Xfa;

internal class PdfLoadedXfaButtonField : PdfLoadedXfaStyledField
{
	private PdfHighlightMode m_highlight;

	private string m_rolloverText;

	private string m_downText;

	private string m_content = string.Empty;

	public PdfHighlightMode Highlight
	{
		get
		{
			return m_highlight;
		}
		set
		{
			m_highlight = value;
		}
	}

	public string MouseRolloverText
	{
		get
		{
			return m_rolloverText;
		}
		set
		{
			if (value != null)
			{
				m_rolloverText = value;
			}
		}
	}

	public string MouseDownText
	{
		get
		{
			return m_downText;
		}
		set
		{
			if (value != null)
			{
				m_downText = value;
			}
		}
	}

	public string Content
	{
		get
		{
			return m_content;
		}
		set
		{
			if (value != null)
			{
				m_content = value;
			}
		}
	}

	internal void Read(XmlNode node)
	{
		currentNode = node;
		ReadCommonProperties(node);
		if (node["value"] != null && node["value"]["text"] != null)
		{
			Content = node["value"]["text"].InnerText;
		}
		else if (node["value"] != null && node["value"]["exData"] != null)
		{
			Content = node["value"]["exData"].InnerText;
		}
	}

	internal void DrawField(PdfGraphics graphics, RectangleF bounds)
	{
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.LineAlignment = (PdfVerticalAlignment)((base.Caption != null) ? base.Caption.VerticalAlignment : base.VerticalAlignment);
		pdfStringFormat.Alignment = ConvertToPdfTextAlignment((base.Caption != null) ? base.Caption.HorizontalAlignment : base.HorizontalAlignment);
		PdfBrush foreBrush = PdfBrushes.Black;
		if (!base.ForeColor.IsEmpty)
		{
			foreBrush = new PdfSolidBrush(base.ForeColor);
		}
		PdfBrush backBrush = null;
		PdfPen pdfPen = null;
		PdfBorderStyle style = PdfBorderStyle.Solid;
		int num = 0;
		if (base.CompleteBorder != null && base.CompleteBorder.Visibility != PdfXfaVisibility.Hidden && base.CompleteBorder.Visibility != PdfXfaVisibility.Invisible)
		{
			backBrush = base.CompleteBorder.GetBrush(bounds);
			pdfPen = base.CompleteBorder.GetFlattenPen();
			style = base.CompleteBorder.GetBorderStyle();
			num = (int)base.CompleteBorder.Width;
		}
		if (num == 0 && pdfPen != null)
		{
			num = (int)pdfPen.Width;
		}
		SizeF size = GetSize();
		RectangleF bounds2 = new RectangleF(new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top), new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom)));
		if (base.Caption != null && base.Caption.Text != string.Empty)
		{
			CheckUnicodeFont(base.Caption.Text);
		}
		graphics.Save();
		PaintParams paintParams = new PaintParams(bounds2, backBrush, foreBrush, pdfPen, style, num, null, GetRotationAngle());
		FieldPainter.DrawButton(graphics, paintParams, base.Caption.Text, base.Font, pdfStringFormat);
		graphics.Restore();
	}
}
