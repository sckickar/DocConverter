using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

public class PdfHTMLTextElement
{
	private PdfFont m_font;

	private PdfBrush m_brush;

	private string m_htmlText;

	private TextAlign m_textAlign;

	internal bool m_nativeRendering = true;

	internal bool m_isPdfGrid;

	internal RectangleF shapeBounds = RectangleF.Empty;

	internal float m_bottomCellpadding;

	private float m_height;

	private int m_htmlElementFont;

	private Color m_Color = Color.Black;

	private float m_htmlFontHeight;

	private float m_htmlFontElementHeight;

	private string m_stringFontName;

	private PdfFont m_currentFont;

	internal List<Htmltext> m_htmllist = new List<Htmltext>();

	private float xPosition;

	private float yPosition;

	private PdfBrush m_Htmlbrush;

	private PdfFontStyle m_style;

	private PdfFontStyle m_style1;

	private PdfFontFamily m_fontFace = PdfFontFamily.TimesRoman;

	private float m_initalXvalue;

	private float m_maxHeight = float.MinValue;

	private bool m_fontAttribute;

	private float m_paginateHeight;

	public PdfFont Font
	{
		get
		{
			return m_font;
		}
		set
		{
			m_font = value;
		}
	}

	public PdfBrush Brush
	{
		get
		{
			return m_brush;
		}
		set
		{
			m_brush = value;
		}
	}

	internal float Height => m_height;

	public string HTMLText
	{
		get
		{
			return m_htmlText;
		}
		set
		{
			m_htmlText = value;
		}
	}

	public TextAlign TextAlign
	{
		get
		{
			return m_textAlign;
		}
		set
		{
			m_textAlign = value;
		}
	}

	internal bool RaiseEndPageLayout => this.EndPageLayout != null;

	internal bool RaiseBeginPageLayout => this.BeginPageLayout != null;

	public event EndPageLayoutEventHandler EndPageLayout;

	public event BeginPageLayoutEventHandler BeginPageLayout;

	public PdfHTMLTextElement()
	{
		m_font = new PdfStandardFont(PdfFontFamily.Helvetica, 3f);
		m_brush = PdfBrushes.Black;
		m_htmlText = "";
		m_textAlign = TextAlign.Left;
	}

	public PdfHTMLTextElement(string htmlText, PdfFont font, PdfBrush brush)
	{
		m_htmlText = htmlText;
		m_font = font;
		m_brush = brush;
	}

	public void Draw(PdfGraphics graphics, RectangleF layoutRectangle)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		if (layoutRectangle.Height < 0f)
		{
			throw new ArgumentNullException("height");
		}
		m_htmllist.Clear();
		ParseHtml(HTMLText);
		PdfLayoutResult pdfLayoutResult = null;
		PdfFont pdfFont = null;
		RectangleF rectangleF = layoutRectangle;
		m_initalXvalue = layoutRectangle.X;
		m_maxHeight = layoutRectangle.Height;
		float width = layoutRectangle.Width;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		PdfLayoutFormat pdfLayoutFormat = new PdfLayoutFormat();
		pdfLayoutFormat.Layout = PdfLayoutType.Paginate;
		pdfLayoutFormat.Break = PdfLayoutBreakType.FitPage;
		if (graphics.Page == null || !(graphics.Page is PdfPage))
		{
			return;
		}
		PdfPage pdfPage = graphics.Page as PdfPage;
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		if (TextAlign != TextAlign.Left)
		{
			if (TextAlign == TextAlign.Right)
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Right;
			}
			else if (TextAlign == TextAlign.Justify)
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Justify;
			}
			else if (TextAlign == TextAlign.Center)
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Center;
			}
		}
		pdfStringFormat.MeasureTrailingSpaces = true;
		for (int i = 0; i < m_htmllist.Count; i++)
		{
			Htmltext htmltext = m_htmllist[i];
			if (!htmltext.mbaseBrushColor)
			{
				m_Color = htmltext.mbrushcolor;
				m_Htmlbrush = new PdfSolidBrush(m_Color);
			}
			else
			{
				m_Htmlbrush = Brush;
			}
			if (i == 0)
			{
				xPosition = rectangleF.X;
				yPosition = rectangleF.Y;
				if (rectangleF.Height == float.MinValue)
				{
					m_maxHeight = pdfPage.GetClientSize().Height;
				}
			}
			else
			{
				xPosition = pdfLayoutResult.Bounds.Right;
				yPosition = (pdfLayoutResult as PdfTextLayoutResult).LastLineBounds.Y;
				if (!(layoutRectangle.Width > pdfLayoutResult.Bounds.Right + htmltext.mfont.MeasureString(" ", pdfStringFormat).Width - m_initalXvalue) && pdfLayoutResult.Bounds.Y == (pdfLayoutResult as PdfTextLayoutResult).LastLineBounds.Y)
				{
					xPosition = m_initalXvalue;
					yPosition += htmltext.mfont.Height;
					num = 0f;
				}
				else if (pdfLayoutResult.Bounds.Y != (pdfLayoutResult as PdfTextLayoutResult).LastLineBounds.Y)
				{
					xPosition = (pdfLayoutResult as PdfTextLayoutResult).LastLineBounds.Right;
					num = num3;
				}
				else if (layoutRectangle.Width > pdfLayoutResult.Bounds.Right + htmltext.mfont.MeasureString(" ", pdfStringFormat).Width - m_initalXvalue)
				{
					num = pdfLayoutResult.Bounds.Width + (pdfLayoutResult.Bounds.X - m_initalXvalue);
				}
				if (htmltext.mfont.Height != pdfFont.Height)
				{
					yPosition += pdfFont.Height - htmltext.mfont.Height;
				}
				num2 = pdfLayoutResult.Bounds.Bottom;
			}
			PdfTextElement pdfTextElement = new PdfTextElement(htmltext.minnerText, htmltext.mfont, m_Htmlbrush);
			pdfTextElement.StringFormat = pdfStringFormat;
			if (layoutRectangle.Height != float.MinValue)
			{
				pdfTextElement.m_pdfHtmlTextElement = true;
			}
			pdfTextElement.BeginPageLayout += img_BeginPageLayout;
			pdfTextElement.EndPageLayout += img_EndPageLayout;
			pdfFont = htmltext.mfont;
			RectangleF rectangleF2 = new RectangleF(xPosition, yPosition, width - num, m_maxHeight - num2);
			pdfPage.Graphics.Save();
			PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
			PdfStringLayoutResult pdfStringLayoutResult = pdfStringLayouter.Layout(htmltext.minnerText, htmltext.mfont, pdfStringFormat, rectangleF2, pdfPage.GetClientSize().Height);
			if (pdfTextElement.m_pdfHtmlTextElement && yPosition + htmltext.mfont.Height > pdfPage.GetClientSize().Height)
			{
				break;
			}
			if (pdfStringLayoutResult.LineCount > 1)
			{
				if (pdfPage.GetClientSize().Height < pdfStringLayoutResult.ActualSize.Height + pdfStringLayoutResult.LineHeight && pdfLayoutFormat.Break == PdfLayoutBreakType.FitElement && pdfLayoutFormat.Layout == PdfLayoutType.OnePage)
				{
					break;
				}
				if (!string.IsNullOrEmpty(pdfStringLayoutResult.Remainder) && pdfPage.GetClientSize().Height < pdfStringLayoutResult.ActualSize.Height + pdfStringLayoutResult.LineHeight)
				{
					rectangleF2 = new RectangleF(rectangleF2.X, rectangleF2.Y, width, m_maxHeight);
					pdfLayoutResult = pdfTextElement.Draw(pdfPage, rectangleF2, pdfLayoutFormat);
					continue;
				}
				if (!string.IsNullOrEmpty(pdfStringLayoutResult.Remainder) && m_maxHeight < pdfStringLayoutResult.ActualSize.Height + pdfStringLayoutResult.LineHeight)
				{
					rectangleF2 = new RectangleF(rectangleF2.X, rectangleF2.Y, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
					pdfLayoutResult = pdfTextElement.Draw(pdfPage, rectangleF2, pdfLayoutFormat);
					continue;
				}
				pdfTextElement.Text = pdfStringLayoutResult.Lines[0].Text;
				string text = htmltext.minnerText.Remove(0, pdfTextElement.Text.Length);
				string text2 = pdfStringLayoutResult.Lines[1].Text;
				pdfStringLayoutResult = pdfStringLayouter.Layout(pdfTextElement.Text, htmltext.mfont, pdfStringFormat, rectangleF2, pdfPage.GetClientSize().Height);
				rectangleF2 = new RectangleF(rectangleF2.X, rectangleF2.Y, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
				pdfLayoutResult = ((pdfLayoutResult == null || pdfLayoutResult.Page == pdfPage) ? pdfTextElement.Draw(pdfPage, rectangleF2, pdfLayoutFormat) : pdfTextElement.Draw(pdfLayoutResult.Page, rectangleF2, pdfLayoutFormat));
				string text3 = text.Remove(0, text.Length - text2.Length);
				rectangleF2 = ((!(text2 != text3) || !(text3 == "\n")) ? new RectangleF(m_initalXvalue, pdfLayoutResult.Bounds.Bottom, rectangleF.Width, m_maxHeight) : new RectangleF(m_initalXvalue, pdfLayoutResult.Bounds.Y, rectangleF.Width, m_maxHeight));
				pdfStringLayoutResult = pdfStringLayouter.Layout(text, htmltext.mfont, pdfStringFormat, rectangleF2, pdfPage.GetClientSize().Height);
				pdfTextElement.Text = text;
				rectangleF2 = new RectangleF(rectangleF2.X, rectangleF2.Y, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
				pdfLayoutResult = ((pdfLayoutResult == null || pdfLayoutResult.Page == pdfPage) ? pdfTextElement.Draw(pdfPage, rectangleF2, pdfLayoutFormat) : pdfTextElement.Draw(pdfLayoutResult.Page, rectangleF2, pdfLayoutFormat));
				num3 = pdfStringLayoutResult.Lines[pdfStringLayoutResult.LineCount - 1].Width;
			}
			else
			{
				if (pdfStringLayoutResult.ActualSize.Width > 0f && pdfStringLayoutResult.ActualSize.Height > 0f)
				{
					rectangleF2 = new RectangleF(rectangleF2.X, rectangleF2.Y, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
				}
				else if (pdfStringLayoutResult.ActualSize.Width == 0f && pdfStringLayoutResult.ActualSize.Height > 0f && htmltext.minnerText == "\n")
				{
					rectangleF2 = new RectangleF(m_initalXvalue, pdfLayoutResult.Bounds.Bottom, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
				}
				else if (pdfStringLayoutResult.ActualSize.Width == 0f && pdfStringLayoutResult.ActualSize.Height > 0f && htmltext.minnerText != "\n")
				{
					rectangleF2 = new RectangleF(m_initalXvalue, pdfLayoutResult.Bounds.Bottom, width, m_maxHeight - num2);
				}
				else if (pdfStringLayoutResult.ActualSize.Width == 0f && pdfStringLayoutResult.ActualSize.Height == 0f)
				{
					rectangleF2 = new RectangleF(m_initalXvalue, pdfLayoutResult.Bounds.Bottom, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
				}
				if (pdfLayoutResult != null && pdfLayoutResult.Page != pdfPage)
				{
					pdfLayoutResult = ((pdfStringFormat.Alignment != 0) ? pdfTextElement.Draw(layoutRectangle: new RectangleF(rectangleF2.X, rectangleF2.Y, layoutRectangle.Width, rectangleF2.Height), page: pdfLayoutResult.Page, format: pdfLayoutFormat) : pdfTextElement.Draw(pdfLayoutResult.Page, rectangleF2, pdfLayoutFormat));
					continue;
				}
				if (pdfStringFormat.Alignment == PdfTextAlignment.Left)
				{
					pdfLayoutResult = pdfTextElement.Draw(pdfPage, rectangleF2, pdfLayoutFormat);
					continue;
				}
				RectangleF layoutRectangle3 = new RectangleF(rectangleF2.X, rectangleF2.Y, layoutRectangle.Width, rectangleF2.Height);
				pdfLayoutResult = pdfTextElement.Draw(pdfPage, layoutRectangle3, pdfLayoutFormat);
			}
		}
		pdfPage.Graphics.Restore();
	}

	public void Draw(PdfGraphics graphics, PointF location, float width, float height)
	{
		RectangleF layoutRectangle = new RectangleF(location, new SizeF(width, height));
		Draw(graphics, layoutRectangle);
	}

	public PdfLayoutResult Draw(PdfPage page, PointF location, float width, float height, PdfLayoutFormat format)
	{
		return Draw(page, new RectangleF(location.X, location.Y, width, height), format);
	}

	public PdfLayoutResult Draw(PdfPage page, PointF location, float width, PdfLayoutFormat format)
	{
		return Draw(page, new RectangleF(location.X, location.Y, width, float.MinValue), format);
	}

	public PdfLayoutResult Draw(PdfPage page, RectangleF layoutRectangle, PdfLayoutFormat format)
	{
		m_htmllist.Clear();
		ParseHtml(HTMLText);
		PdfLayoutResult pdfLayoutResult = null;
		PdfFont pdfFont = null;
		RectangleF rectangleF = layoutRectangle;
		m_initalXvalue = layoutRectangle.X;
		m_maxHeight = layoutRectangle.Height;
		float width = layoutRectangle.Width;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		if (TextAlign != TextAlign.Left)
		{
			if (TextAlign == TextAlign.Right)
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Right;
			}
			else if (TextAlign == TextAlign.Justify)
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Justify;
			}
			else if (TextAlign == TextAlign.Center)
			{
				pdfStringFormat.Alignment = PdfTextAlignment.Center;
			}
		}
		pdfStringFormat.MeasureTrailingSpaces = true;
		for (int i = 0; i < m_htmllist.Count; i++)
		{
			Htmltext htmltext = m_htmllist[i];
			if (!htmltext.mbaseBrushColor)
			{
				m_Color = htmltext.mbrushcolor;
				m_Htmlbrush = new PdfSolidBrush(m_Color);
			}
			else
			{
				m_Htmlbrush = Brush;
			}
			if (i == 0)
			{
				xPosition = rectangleF.X;
				yPosition = rectangleF.Y;
				if (rectangleF.Height == float.MinValue)
				{
					m_maxHeight = page.GetClientSize().Height;
				}
			}
			else
			{
				xPosition = pdfLayoutResult.Bounds.Right;
				yPosition = (pdfLayoutResult as PdfTextLayoutResult).LastLineBounds.Y;
				if (!(layoutRectangle.Width > pdfLayoutResult.Bounds.Right + htmltext.mfont.MeasureString(" ", pdfStringFormat).Width - m_initalXvalue) && pdfLayoutResult.Bounds.Y == (pdfLayoutResult as PdfTextLayoutResult).LastLineBounds.Y)
				{
					xPosition = m_initalXvalue;
					yPosition += htmltext.mfont.Height;
					num = 0f;
				}
				else if (pdfLayoutResult.Bounds.Y != (pdfLayoutResult as PdfTextLayoutResult).LastLineBounds.Y)
				{
					xPosition = (pdfLayoutResult as PdfTextLayoutResult).LastLineBounds.Right;
					num = num3;
				}
				else if (layoutRectangle.Width > pdfLayoutResult.Bounds.Right + htmltext.mfont.MeasureString(" ", pdfStringFormat).Width - m_initalXvalue)
				{
					num = pdfLayoutResult.Bounds.Width + (pdfLayoutResult.Bounds.X - m_initalXvalue);
				}
				if (htmltext.mfont.Height != pdfFont.Height)
				{
					yPosition += pdfFont.Height - htmltext.mfont.Height;
				}
				num2 = pdfLayoutResult.Bounds.Bottom;
			}
			PdfTextElement pdfTextElement = new PdfTextElement(htmltext.minnerText, htmltext.mfont, m_Htmlbrush);
			pdfTextElement.m_isPdfGrid = m_isPdfGrid;
			pdfTextElement.StringFormat = pdfStringFormat;
			if (layoutRectangle.Height != float.MinValue)
			{
				pdfTextElement.m_pdfHtmlTextElement = true;
			}
			pdfTextElement.BeginPageLayout += img_BeginPageLayout;
			pdfTextElement.EndPageLayout += img_EndPageLayout;
			pdfFont = htmltext.mfont;
			RectangleF rectangleF2 = new RectangleF(xPosition, yPosition, width - num, m_maxHeight - num2);
			page.Graphics.Save();
			PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
			PdfStringLayoutResult pdfStringLayoutResult = pdfStringLayouter.Layout(htmltext.minnerText, htmltext.mfont, pdfStringFormat, rectangleF2, page.GetClientSize().Height);
			if (pdfLayoutResult != null)
			{
				m_paginateHeight = rectangleF2.Y + pdfStringLayoutResult.ActualSize.Height;
				num5 = m_paginateHeight - rectangleF2.Y;
				num6 = pdfStringLayoutResult.ActualSize.Width + rectangleF2.Height;
				num4 = rectangleF2.Height - pdfStringLayoutResult.ActualSize.Height;
				num7 = pdfStringLayoutResult.ActualSize.Width + htmltext.mfont.Size;
			}
			if (pdfTextElement.m_pdfHtmlTextElement && yPosition + htmltext.mfont.Height > page.GetClientSize().Height && format.Layout != 0)
			{
				break;
			}
			if (pdfStringLayoutResult.LineCount > 1)
			{
				if (page.GetClientSize().Height < pdfStringLayoutResult.ActualSize.Height + pdfStringLayoutResult.LineHeight && format.Break == PdfLayoutBreakType.FitElement && format.Layout == PdfLayoutType.OnePage)
				{
					break;
				}
				if (!string.IsNullOrEmpty(pdfStringLayoutResult.Remainder) && page.GetClientSize().Height < pdfStringLayoutResult.ActualSize.Height + pdfStringLayoutResult.LineHeight)
				{
					rectangleF2 = new RectangleF(rectangleF2.X, rectangleF2.Y, width, m_maxHeight);
					pdfLayoutResult = pdfTextElement.Draw(page, rectangleF2, format);
				}
				else if (!string.IsNullOrEmpty(pdfStringLayoutResult.Remainder) && m_maxHeight < pdfStringLayoutResult.ActualSize.Height + pdfStringLayoutResult.LineHeight)
				{
					rectangleF2 = new RectangleF(rectangleF2.X, rectangleF2.Y, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
					pdfLayoutResult = pdfTextElement.Draw(page, rectangleF2, format);
				}
				else
				{
					pdfTextElement.Text = pdfStringLayoutResult.Lines[0].Text;
					string text = htmltext.minnerText.Remove(0, pdfTextElement.Text.Length);
					string text2 = pdfStringLayoutResult.Lines[1].Text;
					pdfStringLayoutResult = pdfStringLayouter.Layout(pdfTextElement.Text, htmltext.mfont, pdfStringFormat, rectangleF2, page.GetClientSize().Height);
					rectangleF2 = new RectangleF(rectangleF2.X, rectangleF2.Y, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
					pdfLayoutResult = ((pdfLayoutResult == null || pdfLayoutResult.Page == page) ? pdfTextElement.Draw(page, rectangleF2, format) : pdfTextElement.Draw(pdfLayoutResult.Page, rectangleF2, format));
					string text3 = text.Remove(0, text.Length - text2.Length);
					rectangleF2 = ((!(text2 != text3) || !(text3 == "\n")) ? new RectangleF(m_initalXvalue, pdfLayoutResult.Bounds.Bottom, rectangleF.Width, m_maxHeight) : new RectangleF(m_initalXvalue, pdfLayoutResult.Bounds.Y, rectangleF.Width, m_maxHeight));
					pdfStringLayoutResult = pdfStringLayouter.Layout(text, htmltext.mfont, pdfStringFormat, rectangleF2, page.GetClientSize().Height);
					pdfTextElement.Text = text;
					num7 = pdfStringLayoutResult.ActualSize.Width + pdfStringLayoutResult.ActualSize.Height + pdfLayoutResult.Bounds.Height;
					if (num7 < page.GetClientSize().Width)
					{
						rectangleF2 = new RectangleF(rectangleF2.X, rectangleF2.Y, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
					}
					pdfLayoutResult = ((pdfLayoutResult == null || pdfLayoutResult.Page == page) ? pdfTextElement.Draw(page, rectangleF2, format) : pdfTextElement.Draw(pdfLayoutResult.Page, rectangleF2, format));
					num3 = pdfStringLayoutResult.Lines[pdfStringLayoutResult.LineCount - 1].Width;
					PdfTextLayoutResult pdfTextLayoutResult = pdfLayoutResult as PdfTextLayoutResult;
					while (pdfTextLayoutResult != null && pdfTextLayoutResult.Remainder != null && pdfTextLayoutResult.LastLineBounds != RectangleF.Empty)
					{
						rectangleF2 = new RectangleF(pdfTextLayoutResult.Bounds.X, pdfTextLayoutResult.Bounds.Bottom, pdfTextLayoutResult.Bounds.Width, pdfTextLayoutResult.Bounds.Height);
						pdfTextElement.Text = pdfTextLayoutResult.Remainder;
						pdfLayoutResult = pdfTextElement.Draw(pdfTextLayoutResult.Page, rectangleF2, format);
						pdfTextLayoutResult = pdfLayoutResult as PdfTextLayoutResult;
					}
				}
			}
			else
			{
				if ((pdfStringLayoutResult.ActualSize.Width > 0f && pdfStringLayoutResult.ActualSize.Height > 0f && pdfLayoutResult != null && m_paginateHeight == pdfLayoutResult.Bounds.Bottom && num4 < num5 && num6 > width) || (m_paginateHeight > width && rectangleF2.Width < num7))
				{
					rectangleF2 = new RectangleF(m_initalXvalue, pdfLayoutResult.Bounds.Bottom, rectangleF.Width, m_maxHeight);
				}
				else if (pdfStringLayoutResult.ActualSize.Width > 0f && pdfStringLayoutResult.ActualSize.Height > 0f)
				{
					rectangleF2 = new RectangleF(rectangleF2.X, rectangleF2.Y, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
				}
				else if (pdfLayoutResult != null && pdfStringLayoutResult.ActualSize.Width == 0f && pdfStringLayoutResult.ActualSize.Height > 0f && htmltext.minnerText == "\n")
				{
					rectangleF2 = new RectangleF(m_initalXvalue, pdfLayoutResult.Bounds.Bottom, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
				}
				else if (pdfLayoutResult != null && pdfStringLayoutResult.ActualSize.Width == 0f && pdfStringLayoutResult.ActualSize.Height > 0f && htmltext.minnerText != "\n")
				{
					rectangleF2 = new RectangleF(m_initalXvalue, pdfLayoutResult.Bounds.Bottom, width, m_maxHeight - num2);
				}
				else if (pdfLayoutResult != null && pdfStringLayoutResult.ActualSize.Width == 0f && pdfStringLayoutResult.ActualSize.Height == 0f)
				{
					rectangleF2 = new RectangleF(m_initalXvalue, pdfLayoutResult.Bounds.Bottom, pdfStringLayoutResult.ActualSize.Width, pdfStringLayoutResult.ActualSize.Height);
				}
				if (pdfLayoutResult != null && pdfLayoutResult.Page != page)
				{
					pdfLayoutResult = ((pdfStringFormat.Alignment != 0) ? pdfTextElement.Draw(layoutRectangle: new RectangleF(rectangleF2.X, rectangleF2.Y, layoutRectangle.Width, rectangleF2.Height), page: pdfLayoutResult.Page, format: format) : pdfTextElement.Draw(pdfLayoutResult.Page, rectangleF2, format));
				}
				else if (pdfStringFormat.Alignment == PdfTextAlignment.Left)
				{
					pdfLayoutResult = pdfTextElement.Draw(page, rectangleF2, format);
				}
				else
				{
					RectangleF layoutRectangle3 = new RectangleF(rectangleF2.X, rectangleF2.Y, layoutRectangle.Width, rectangleF2.Height);
					pdfLayoutResult = pdfTextElement.Draw(page, layoutRectangle3, format);
				}
			}
			page.Graphics.Restore();
		}
		return pdfLayoutResult;
	}

	internal void ParseHtml(string text)
	{
		if (!text.StartsWith("<?xml version="))
		{
			text = "<?xml version=\"1.0\" encoding=\"utf-8\"?> <html>" + text + "</html>";
		}
		XDocument xDocument = XDocument.Parse(text, LoadOptions.SetLineInfo);
		if (Font == null)
		{
			m_currentFont = new PdfStandardFont(PdfFontFamily.TimesRoman, 14f);
		}
		else
		{
			m_currentFont = Font;
		}
		if (xDocument.Root != null)
		{
			ParseElements(xDocument.Root);
		}
	}

	private void ParseElements(XElement parent)
	{
		Htmltext htmltext = new Htmltext();
		foreach (XNode item in parent.Nodes())
		{
			if (!m_fontAttribute)
			{
				m_style = (m_style1 = PdfFontStyle.Regular);
			}
			if (item is XText)
			{
				XText xText = item as XText;
				m_Color = Color.Black;
				bool mbaseBrushColor = Brush != null;
				htmltext = new Htmltext();
				htmltext.mtag = item;
				htmltext.minnerText = xText.Value;
				htmltext.mfont = m_currentFont;
				htmltext.mbrushcolor = m_Color;
				htmltext.mbaseBrushColor = mbaseBrushColor;
				m_htmllist.Add(htmltext);
			}
			if (!(item is XElement))
			{
				continue;
			}
			XElement xElement = item as XElement;
			m_htmlFontHeight = m_currentFont.Size;
			if (Font == null)
			{
				m_stringFontName = m_currentFont.Name;
			}
			else
			{
				m_stringFontName = null;
			}
			if (!m_fontAttribute)
			{
				m_Color = Color.Black;
			}
			if (xElement.Name == "p")
			{
				ParseElements(xElement);
				continue;
			}
			if (xElement.Name == "font")
			{
				foreach (XAttribute item2 in xElement.Attributes())
				{
					if (item2.Name == "color")
					{
						if (item2.Value[0] != '#')
						{
							Color color = Color.FromName(item2.Value);
							m_htmlElementFont = GetColorRgb(color);
							m_Color = color;
							continue;
						}
						try
						{
							item2.Value = item2.Value;
							Color color2 = Color.FromArgb((byte)int.Parse(item2.Value.Substring(1, 2), NumberStyles.HexNumber), (byte)int.Parse(item2.Value.Substring(3, 2), NumberStyles.HexNumber), (byte)int.Parse(item2.Value.Substring(5, 2), NumberStyles.HexNumber));
							m_htmlElementFont = GetColorRgb(color2);
							m_Color = color2;
						}
						catch
						{
						}
					}
					else if (item2.Name == "size")
					{
						m_htmlFontElementHeight = float.Parse(item2.Value);
						m_htmlFontHeight = m_htmlFontElementHeight;
					}
					else if (item2.Name == "face")
					{
						m_stringFontName = item2.Value;
					}
				}
			}
			Pickstyle(xElement.Name.ToString());
			Pickstyle(m_stringFontName);
			if (!m_fontAttribute)
			{
				m_style1 = m_style;
			}
			else
			{
				m_style1 |= m_style;
			}
			if (xElement.Name == "br")
			{
				htmltext = new Htmltext();
				htmltext.mtag = item;
				htmltext.minnerText = "\n";
				htmltext.mfont = new PdfStandardFont(m_fontFace, m_htmlFontHeight, m_style1);
				htmltext.mbrushcolor = m_Color;
				if (m_stringFontName == null && !(m_currentFont is PdfStandardFont) && m_currentFont is PdfTrueTypeFont)
				{
					htmltext.mfont = new PdfTrueTypeFont(m_currentFont as PdfTrueTypeFont, m_htmlFontHeight);
					htmltext.mfont.Style = m_style1;
				}
			}
			else
			{
				htmltext = new Htmltext();
				htmltext.mtag = item;
				htmltext.minnerText = xElement.Value;
				htmltext.mfont = new PdfStandardFont(m_fontFace, m_htmlFontHeight, m_style1);
				htmltext.mbrushcolor = m_Color;
				if (m_stringFontName == null && !(m_currentFont is PdfStandardFont) && m_currentFont is PdfTrueTypeFont)
				{
					htmltext.mfont = new PdfTrueTypeFont(m_currentFont as PdfTrueTypeFont, m_htmlFontHeight);
					htmltext.mfont.Style = m_style1;
				}
			}
			bool flag = false;
			using (IEnumerator<XElement> enumerator3 = xElement.Descendants().GetEnumerator())
			{
				if (enumerator3.MoveNext())
				{
					_ = enumerator3.Current;
					flag = true;
					m_fontAttribute = true;
					ParseElements(xElement);
					m_fontAttribute = false;
				}
			}
			if (!flag)
			{
				m_htmllist.Add(htmltext);
			}
		}
	}

	private void Pickstyle(string chos)
	{
		if (chos == null)
		{
			return;
		}
		switch (chos.Length)
		{
		case 1:
			switch (chos[0])
			{
			case 'I':
			case 'i':
				m_style = PdfFontStyle.Italic;
				break;
			case 'B':
			case 'b':
				m_style = PdfFontStyle.Bold;
				break;
			case 'U':
			case 'u':
				m_style = PdfFontStyle.Underline;
				break;
			}
			break;
		case 9:
			if (chos == "Helvetica")
			{
				m_fontFace = PdfFontFamily.Helvetica;
			}
			break;
		case 7:
			if (chos == "Courier")
			{
				m_fontFace = PdfFontFamily.Courier;
			}
			break;
		case 10:
			if (chos == "TimesRoman")
			{
				m_fontFace = PdfFontFamily.TimesRoman;
			}
			break;
		case 6:
			if (chos == "Symbol")
			{
				m_fontFace = PdfFontFamily.Symbol;
			}
			break;
		}
	}

	private int GetColorRgb(Color color)
	{
		int r = color.R;
		int g = color.G;
		int b = color.B;
		return GetCOLORREF(r, g, b);
	}

	private int GetCOLORREF(int r, int g, int b)
	{
		int num = g << 8;
		int num2 = b << 16;
		return r | num | num2;
	}

	private void img_EndPageLayout(object sender, EndPageLayoutEventArgs e)
	{
		if (this.EndPageLayout != null)
		{
			this.EndPageLayout(this, e);
		}
	}

	private void img_BeginPageLayout(object sender, BeginPageLayoutEventArgs e)
	{
		if (this.BeginPageLayout != null)
		{
			this.BeginPageLayout(this, e);
		}
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
