using System.Xml;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Xfa;

internal class PdfLoadedXfaTextElement : PdfLoadedXfaStyledField
{
	private string m_text = string.Empty;

	internal string alterText = string.Empty;

	internal bool isExData;

	public string Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	internal PdfLoadedXfaTextElement()
	{
	}

	internal void Read(XmlNode node)
	{
		currentNode = node;
		ReadCommonProperties(node);
		if (node["value"] != null && node["value"]["text"] != null)
		{
			Text = node["value"]["text"].InnerText;
		}
		else if (node["value"] != null && node["value"]["exData"] != null)
		{
			Text = node["value"]["exData"].OuterXml;
			alterText = node["value"]["exData"].InnerText;
			isExData = true;
		}
	}

	internal void DrawTextElement(PdfGraphics graphics, RectangleF bounds)
	{
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.LineAlignment = (PdfVerticalAlignment)base.VerticalAlignment;
		pdfStringFormat.Alignment = ConvertToPdfTextAlignment(base.HorizontalAlignment);
		RectangleF tempBounds = default(RectangleF);
		PdfBrush brush = PdfBrushes.Black;
		if (!base.ForeColor.IsEmpty)
		{
			brush = new PdfSolidBrush(base.ForeColor);
		}
		SizeF size = GetSize();
		tempBounds = new RectangleF(new PointF(bounds.Location.X + base.Margins.Left, bounds.Location.Y + base.Margins.Top), new SizeF(size.Width - (base.Margins.Right + base.Margins.Left), size.Height - (base.Margins.Top + base.Margins.Bottom)));
		if (base.CompleteBorder != null && base.CompleteBorder.Visibility != PdfXfaVisibility.Hidden && base.CompleteBorder.Visibility != PdfXfaVisibility.Invisible)
		{
			RectangleF tempBounds2 = new RectangleF(bounds.Location, size);
			graphics.Save();
			graphics.TranslateTransform(tempBounds2.X, tempBounds2.Y);
			graphics.RotateTransform(-GetRotationAngle());
			tempBounds2 = GetRenderingRect(tempBounds2);
			base.CompleteBorder.DrawBorder(graphics, tempBounds2);
			graphics.Restore();
		}
		graphics.Save();
		if (base.Font == null)
		{
			base.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 10f, PdfFontStyle.Regular);
		}
		if (tempBounds.Height < base.Font.Height)
		{
			if (tempBounds.Height + (base.Margins.Top + base.Margins.Bottom) >= base.Font.Height)
			{
				tempBounds.Height = base.Font.Height;
			}
			else
			{
				tempBounds.Height = base.Font.Height;
			}
		}
		graphics.TranslateTransform(tempBounds.X, tempBounds.Y);
		graphics.RotateTransform(-GetRotationAngle());
		RectangleF renderingRect = GetRenderingRect(tempBounds);
		graphics.DrawString(isExData ? alterText : Text, base.Font, brush, renderingRect, pdfStringFormat);
		graphics.Restore();
	}

	internal new SizeF GetSize()
	{
		if (base.Width <= 0f)
		{
			if (currentNode.Attributes["maxW"] != null)
			{
				base.Width = ConvertToPoint(currentNode.Attributes["maxW"].Value);
			}
			if (currentNode.Attributes["minW"] != null)
			{
				base.Width = ConvertToPoint(currentNode.Attributes["minW"].Value);
			}
		}
		if (base.Height <= 0f)
		{
			if (currentNode.Attributes["maxH"] != null)
			{
				base.Height = ConvertToPoint(currentNode.Attributes["maxH"].Value);
			}
			if (currentNode.Attributes["minH"] != null)
			{
				base.Height = ConvertToPoint(currentNode.Attributes["minH"].Value);
				if (base.Font != null)
				{
					if (isExData)
					{
						float height = base.Font.MeasureString(alterText, base.Width).Height;
						if (height > base.Height)
						{
							base.Height = height;
						}
					}
					else if (Text != string.Empty)
					{
						float height2 = base.Font.MeasureString(Text, base.Width).Height;
						if (height2 > base.Height)
						{
							base.Height = height2;
						}
					}
					else if (base.Font.Height > base.Height)
					{
						base.Height = base.Font.Height + 0.5f;
					}
				}
				if (parent is PdfLoadedXfaForm { FlowDirection: not PdfLoadedXfaFlowDirection.Row })
				{
					base.Height += base.Margins.Top + base.Margins.Bottom;
				}
			}
			else if (currentNode.Attributes["h"] == null && base.Font != null)
			{
				if (isExData)
				{
					base.Height = base.Font.MeasureString(alterText, base.Width).Height;
				}
				else if (Text != string.Empty)
				{
					base.Height = base.Font.MeasureString(Text, base.Width).Height;
				}
				else if (base.Font.Height > base.Height)
				{
					base.Height = base.Font.Height + 0.5f;
				}
			}
		}
		if (base.Rotate == PdfXfaRotateAngle.RotateAngle270 || base.Rotate == PdfXfaRotateAngle.RotateAngle90)
		{
			return new SizeF(base.Height, base.Width);
		}
		return new SizeF(base.Width, base.Height);
	}
}
