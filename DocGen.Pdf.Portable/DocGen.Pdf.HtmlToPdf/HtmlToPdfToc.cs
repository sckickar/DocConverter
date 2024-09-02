using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.HtmlToPdf;

public class HtmlToPdfToc
{
	public enum TabLeaderStyle
	{
		None = 0,
		Dot = 46,
		Dash = 45,
		Solid = 95
	}

	private string m_title;

	private PdfTextAlignment m_titleAlignment;

	private HtmlToPdfTocStyle m_titleStyle = new HtmlToPdfTocStyle();

	private HtmlToPdfTocStyle m_headerStyle = new HtmlToPdfTocStyle();

	private TabLeaderStyle m_tabLeader;

	private int m_tocPageCount;

	private bool m_isBlinkRenderingEngine;

	private char m_dotStyle;

	private char m_tabLeaderChar;

	private int m_maximumHeaderLevel;

	private int m_startingPageNumber;

	private bool isNextPage;

	private bool isFirstPage = true;

	private bool isTabLeaderChar;

	private PdfLayoutResult m_pageLayoutResult;

	private float m_pageLayoutBottom;

	private PdfTemplate m_template;

	private PdfPage m_pageTemplate;

	private List<HtmlToPdfTocStyle> m_headerStyleCollection = new List<HtmlToPdfTocStyle>();

	private const int beginRect = 15;

	private const int beginTitle = 12;

	private const int leftRectPadding = 15;

	private const int rightRectPadding = 15;

	private const int rectLineSpacing = 3;

	private const int maxHeaderLevel = 6;

	private float m_headerHeight;

	private float m_footerHeight;

	public string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			m_title = value;
		}
	}

	public PdfTextAlignment TitleAlignment
	{
		get
		{
			return m_titleAlignment;
		}
		set
		{
			m_titleAlignment = value;
		}
	}

	public HtmlToPdfTocStyle TitleStyle
	{
		get
		{
			return m_titleStyle;
		}
		set
		{
			m_titleStyle = value;
		}
	}

	public TabLeaderStyle TabLeader
	{
		get
		{
			return m_tabLeader;
		}
		set
		{
			m_tabLeader = value;
		}
	}

	public char TabLeaderChar
	{
		get
		{
			return m_tabLeaderChar;
		}
		set
		{
			m_tabLeaderChar = value;
		}
	}

	public int MaximumHeaderLevel
	{
		get
		{
			return m_maximumHeaderLevel;
		}
		set
		{
			m_maximumHeaderLevel = value;
		}
	}

	public int StartingPageNumber
	{
		get
		{
			return m_startingPageNumber;
		}
		set
		{
			m_startingPageNumber = value;
		}
	}

	internal HtmlToPdfTocStyle HeaderStyle
	{
		get
		{
			return m_headerStyle;
		}
		set
		{
			m_headerStyle = value;
		}
	}

	internal int TocPageCount
	{
		get
		{
			return m_tocPageCount;
		}
		set
		{
			m_tocPageCount = value;
		}
	}

	internal bool IsBlinkRenderingEngine
	{
		get
		{
			return m_isBlinkRenderingEngine;
		}
		set
		{
			m_isBlinkRenderingEngine = value;
		}
	}

	internal float HeaderHeight
	{
		get
		{
			return m_headerHeight;
		}
		set
		{
			m_headerHeight = value;
		}
	}

	internal float FooterHeight
	{
		get
		{
			return m_footerHeight;
		}
		set
		{
			m_footerHeight = value;
		}
	}

	public HtmlToPdfToc()
	{
		TabLeader = TabLeaderStyle.Dot;
		MaximumHeaderLevel = 6;
		IsBlinkRenderingEngine = false;
		if (PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A1B && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A1A)
		{
			Title = "Table of Contents";
			TitleStyle.Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 16f, PdfFontStyle.Regular);
			TitleStyle.ForeColor = PdfBrushes.DarkBlue;
			TitleAlignment = PdfTextAlignment.Left;
			HtmlToPdfTocStyle item = new HtmlToPdfTocStyle
			{
				Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 11.5f, PdfFontStyle.Regular),
				ForeColor = PdfBrushes.Black,
				Padding = new PdfPaddings(0f, 0f, 2f, 2f)
			};
			m_headerStyleCollection.Add(item);
			HtmlToPdfTocStyle item2 = new HtmlToPdfTocStyle
			{
				Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 9.5f, PdfFontStyle.Regular),
				ForeColor = PdfBrushes.Black,
				Padding = new PdfPaddings(0f, 0f, 2f, 2f)
			};
			m_headerStyleCollection.Add(item2);
			HtmlToPdfTocStyle item3 = new HtmlToPdfTocStyle
			{
				Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 8f, PdfFontStyle.Regular),
				ForeColor = PdfBrushes.Black,
				Padding = new PdfPaddings(0f, 0f, 2f, 2f)
			};
			m_headerStyleCollection.Add(item3);
			HtmlToPdfTocStyle item4 = new HtmlToPdfTocStyle
			{
				Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 8f, PdfFontStyle.Italic),
				ForeColor = PdfBrushes.Black,
				Padding = new PdfPaddings(0f, 0f, 2f, 2f)
			};
			m_headerStyleCollection.Add(item4);
			HtmlToPdfTocStyle item5 = new HtmlToPdfTocStyle
			{
				Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 8f, PdfFontStyle.Regular),
				ForeColor = PdfBrushes.Black,
				Padding = new PdfPaddings(0f, 0f, 2f, 2f)
			};
			m_headerStyleCollection.Add(item5);
			HtmlToPdfTocStyle item6 = new HtmlToPdfTocStyle
			{
				Font = new PdfStandardFont(PdfFontFamily.TimesRoman, 8f, PdfFontStyle.Regular),
				ForeColor = PdfBrushes.Black,
				Padding = new PdfPaddings(0f, 0f, 2f, 2f)
			};
			m_headerStyleCollection.Add(item6);
		}
	}

	internal int GetRectangleHeightAndTocPageCount(PdfPageBase page, List<HtmlInternalLink> internalLinkCollection)
	{
		int num = 0;
		SizeF sizeF = TitleStyle.Font.MeasureString(Title);
		float num2 = 0f;
		float num3 = 12f + sizeF.Height + 15f + HeaderHeight;
		float num4 = page.Graphics.ClientSize.Width - 15f;
		int num5 = 0;
		int num6 = 0;
		bool flag = true;
		int index = 0;
		HtmlInternalLink htmlInternalLink = internalLinkCollection[index];
		for (index = 0; index < internalLinkCollection.Count; index++)
		{
			htmlInternalLink = internalLinkCollection[index];
			if (htmlInternalLink.HeaderTagLevel == null)
			{
				continue;
			}
			int num7 = int.Parse(htmlInternalLink.HeaderTagLevel.Split('H')[1]);
			if (num7 > MaximumHeaderLevel)
			{
				continue;
			}
			if (flag)
			{
				num6 = num7;
				flag = false;
			}
			if (num7 <= num6)
			{
				num2 = 15f;
			}
			else if (num7 > num5)
			{
				num2 += 15f;
			}
			else if (num7 < num5)
			{
				index--;
				int num8 = 0;
				int num9 = 0;
				while (true)
				{
					num9++;
					htmlInternalLink = internalLinkCollection[index - num9];
					int num10 = int.Parse(htmlInternalLink.HeaderTagLevel.Split('H')[1]);
					if (num10 <= MaximumHeaderLevel)
					{
						if (num7 <= num6)
						{
							num2 = 15f;
							break;
						}
						if (num7 > num10)
						{
							htmlInternalLink = internalLinkCollection[index - (num9 - num8 - 1)];
							num2 = htmlInternalLink.TocXcoordinate;
							break;
						}
						if (num7 == num10)
						{
							htmlInternalLink = internalLinkCollection[index - num9];
							num2 = htmlInternalLink.TocXcoordinate;
							break;
						}
					}
					else
					{
						num8 += num8;
					}
				}
				index++;
			}
			htmlInternalLink = internalLinkCollection[index];
			htmlInternalLink.TocXcoordinate = num2;
			num5 = num7;
			float num11 = (htmlInternalLink.TocRectHeight = GetRectangleHeight(htmlInternalLink, num4 - num2, num7));
			num3 += num11;
			if (num3 > page.Graphics.ClientSize.Height - FooterHeight)
			{
				num++;
				num3 = 0f;
				num3 = num11 + 3f + HeaderHeight;
			}
			else
			{
				num3 += 3f;
			}
		}
		return num + 1;
	}

	private float GetRectangleHeight(HtmlInternalLink htmlToc, float rectWidth, int currentHeaderLevel)
	{
		float num = 0f;
		HtmlToPdfTocStyle headerStyle = m_headerStyleCollection[currentHeaderLevel - 1];
		HeaderStyle = headerStyle;
		SizeF sizeF = HeaderStyle.Font.MeasureString(htmlToc.HeaderContent);
		if (sizeF.Width < rectWidth - (HeaderStyle.Padding.Left + HeaderStyle.Padding.Right))
		{
			return sizeF.Height + HeaderStyle.Padding.Top + HeaderStyle.Padding.Bottom;
		}
		int linesFilled = 0;
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.Alignment = PdfTextAlignment.Left;
		HeaderStyle.Font.MeasureString(htmlToc.HeaderContent, rectWidth - (HeaderStyle.Padding.Left + HeaderStyle.Padding.Right), pdfStringFormat, out var _, out linesFilled);
		return sizeF.Height * (float)linesFilled + HeaderStyle.Padding.Top + HeaderStyle.Padding.Bottom;
	}

	internal void DrawTable(PdfDocument lDoc, PdfPage page, List<HtmlInternalLink> internalLinkCollection)
	{
		int num = 0;
		m_template = lDoc.Pages[num].CreateTemplate();
		PdfGraphics graphics = lDoc.Pages[num].Graphics;
		SizeF sizeF = TitleStyle.Font.MeasureString(Title);
		float num2 = 0f;
		num2 = ((!(HeaderHeight > 0f)) ? HeaderHeight : (HeaderHeight + lDoc.PageSettings.Margins.Top));
		RectangleF rectangle = new RectangleF(12f, 12f + num2, page.Graphics.ClientSize.Width - 27f, sizeF.Height - FooterHeight);
		graphics.DrawRectangle(TitleStyle.BackgroundColor, rectangle);
		graphics.DrawString(Title, TitleStyle.Font, TitleStyle.ForeColor, new RectangleF(12f, 12f + num2, page.Graphics.ClientSize.Width, page.Graphics.ClientSize.Height), new PdfStringFormat(TitleAlignment));
		float num3 = 12f + sizeF.Height + 15f + num2;
		int index = 0;
		HtmlInternalLink htmlInternalLink = internalLinkCollection[index];
		for (index = 0; index < internalLinkCollection.Count; index++)
		{
			htmlInternalLink = internalLinkCollection[index];
			if (htmlInternalLink.HeaderTagLevel == null)
			{
				continue;
			}
			int num4 = int.Parse(htmlInternalLink.HeaderTagLevel.Split('H')[1]);
			if (num4 > MaximumHeaderLevel)
			{
				continue;
			}
			float tocRectHeight = htmlInternalLink.TocRectHeight;
			if (num3 + tocRectHeight > page.Graphics.ClientSize.Height - FooterHeight)
			{
				num++;
				isNextPage = true;
				m_template = lDoc.Pages[num].CreateTemplate();
				graphics = lDoc.Pages[num].Graphics;
				num3 = 0f;
				num3 = HeaderHeight;
			}
			float tocXcoordinate = htmlInternalLink.TocXcoordinate;
			float width = page.Graphics.ClientSize.Width - (tocXcoordinate + 15f);
			RectangleF rectangleF = new RectangleF(tocXcoordinate, num3, width, tocRectHeight);
			HtmlToPdfTocStyle headerStyle = m_headerStyleCollection[num4 - 1];
			HeaderStyle = headerStyle;
			graphics.DrawRectangle(HeaderStyle.BackgroundColor, rectangleF);
			DrawHeaderContent(rectangleF, graphics, htmlInternalLink);
			if (IsBlinkRenderingEngine)
			{
				rectangleF.Y -= HeaderHeight;
				PdfDocumentLinkAnnotation pdfDocumentLinkAnnotation = new PdfDocumentLinkAnnotation(rectangleF);
				htmlInternalLink.DestinationPage = lDoc.Pages[htmlInternalLink.DestinationPageNumber + (TocPageCount - 1)];
				PdfDestination pdfDestination = new PdfDestination(htmlInternalLink.DestinationPage);
				float num5 = htmlInternalLink.TocRectHeight / htmlInternalLink.Bounds.Height;
				if (Math.Floor(num5) > 1.0)
				{
					float num6 = (float)((double)htmlInternalLink.TocRectHeight - (double)htmlInternalLink.TocRectHeight / Math.Floor(num5));
					htmlInternalLink.Destination = new PointF(htmlInternalLink.Destination.X, htmlInternalLink.Destination.Y - HeaderHeight - num6);
				}
				else
				{
					htmlInternalLink.Destination = new PointF(htmlInternalLink.Destination.X, htmlInternalLink.Destination.Y - HeaderHeight);
				}
				pdfDestination.Location = htmlInternalLink.Destination;
				pdfDestination.isModified = false;
				pdfDocumentLinkAnnotation.Destination = pdfDestination;
				pdfDocumentLinkAnnotation.Border.Width = 0f;
				lDoc.Pages[num].Annotations.Add(pdfDocumentLinkAnnotation);
			}
			else
			{
				AddDocumentLinkAnnotation(rectangleF, lDoc, page, htmlInternalLink);
			}
			num3 += tocRectHeight + 3f;
		}
	}

	private void DrawHeaderContent(RectangleF rectBounds, PdfGraphics graphics, HtmlInternalLink htmlToc)
	{
		PdfStringFormat pdfStringFormat = new PdfStringFormat();
		pdfStringFormat.Alignment = PdfTextAlignment.Left;
		graphics.DrawString(htmlToc.HeaderContent, HeaderStyle.Font, HeaderStyle.ForeColor, new RectangleF(rectBounds.X + HeaderStyle.Padding.Left, rectBounds.Y + HeaderStyle.Padding.Top, rectBounds.Width - (HeaderStyle.Padding.Right + HeaderStyle.Padding.Left), rectBounds.Height), pdfStringFormat);
		int num = htmlToc.DestinationPageNumber + TocPageCount + StartingPageNumber;
		SizeF sizeF = HeaderStyle.Font.MeasureString(num.ToString());
		SizeF contentSize = HeaderStyle.Font.MeasureString(htmlToc.HeaderContent);
		if (TabLeader != 0)
		{
			DrawTabLeader(rectBounds, graphics, contentSize, sizeF.Width, htmlToc.HeaderContent);
		}
		pdfStringFormat.Alignment = PdfTextAlignment.Right;
		if (contentSize.Width < rectBounds.Width - (sizeF.Width + HeaderStyle.Padding.Left))
		{
			graphics.DrawString(num.ToString(), HeaderStyle.Font, HeaderStyle.ForeColor, new RectangleF(rectBounds.X, rectBounds.Y + HeaderStyle.Padding.Top, rectBounds.Width - HeaderStyle.Padding.Right, rectBounds.Height), pdfStringFormat);
		}
		else
		{
			graphics.DrawString(num.ToString(), HeaderStyle.Font, HeaderStyle.ForeColor, new RectangleF(rectBounds.X, rectBounds.Y + rectBounds.Height - (contentSize.Height + HeaderStyle.Padding.Bottom), rectBounds.Width - HeaderStyle.Padding.Right, rectBounds.Height), pdfStringFormat);
		}
	}

	private void DrawTabLeader(RectangleF rectBounds, PdfGraphics graphics, SizeF contentSize, float pageNumberWidth, string HeaderContent)
	{
		if (TabLeaderChar == '\0' && !isTabLeaderChar)
		{
			isTabLeaderChar = true;
			switch (TabLeader)
			{
			case TabLeaderStyle.Dot:
				TabLeaderChar = '.';
				break;
			case TabLeaderStyle.Dash:
				TabLeaderChar = '-';
				break;
			case TabLeaderStyle.Solid:
				TabLeaderChar = '_';
				break;
			}
		}
		bool flag = true;
		PdfFont pdfFont = new PdfStandardFont(PdfFontFamily.TimesRoman, 9.5f, PdfFontStyle.Regular);
		SizeF sizeF = pdfFont.MeasureString(TabLeaderChar.ToString());
		PdfStringFormat format = new PdfStringFormat();
		float num = HeaderStyle.Padding.Left;
		SizeF sizeF2 = HeaderStyle.Font.MeasureString(" ");
		float num2;
		if (contentSize.Width < rectBounds.Width - (pageNumberWidth + HeaderStyle.Padding.Right))
		{
			num2 = contentSize.Width + HeaderStyle.Padding.Left + sizeF.Width;
		}
		else
		{
			string[] array = HeaderContent.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				SizeF sizeF3 = HeaderStyle.Font.MeasureString(array[i]);
				num += sizeF3.Width;
				num = ((!(num > rectBounds.Width - HeaderStyle.Padding.Right)) ? (num + sizeF2.Width) : (HeaderStyle.Padding.Left + sizeF3.Width + sizeF2.Width));
			}
			num2 = num;
			flag = false;
		}
		string text = "";
		for (; num2 < rectBounds.Width - (pageNumberWidth + HeaderStyle.Padding.Right); num2 += sizeF.Width)
		{
			text += TabLeaderChar;
		}
		float num3 = HeaderStyle.Font.Size - pdfFont.Size;
		if (flag)
		{
			graphics.DrawString(text, pdfFont, HeaderStyle.ForeColor, new RectangleF(rectBounds.X + contentSize.Width + HeaderStyle.Padding.Left, rectBounds.Y + HeaderStyle.Padding.Top + num3, rectBounds.Width - HeaderStyle.Padding.Right, rectBounds.Height), format);
		}
		else
		{
			graphics.DrawString(text, pdfFont, HeaderStyle.ForeColor, new RectangleF(rectBounds.X + num - sizeF2.Width, rectBounds.Y + num3 + rectBounds.Height - (contentSize.Height + HeaderStyle.Padding.Bottom), rectBounds.Width, rectBounds.Height), format);
		}
	}

	private void AddDocumentLinkAnnotation(RectangleF rectBounds, PdfDocument lDoc, PdfPage page, HtmlInternalLink htmltoc)
	{
		HtmlToPdfFormat htmlToPdfFormat = new HtmlToPdfFormat();
		HtmlInternalLink htmlInternalLink = new HtmlInternalLink();
		htmlInternalLink.Bounds = rectBounds;
		htmlInternalLink.Destination = new PointF(htmltoc.Destination.X, htmltoc.Destination.Y);
		htmlInternalLink.DestinationPage = lDoc.Pages[htmltoc.DestinationPageNumber + (TocPageCount - 1)];
		htmlToPdfFormat.HtmlInternalLinksCollection.Add(htmlInternalLink);
		if (isFirstPage)
		{
			m_pageTemplate = page;
			isFirstPage = false;
		}
		if (isNextPage)
		{
			m_pageTemplate = m_pageLayoutResult.Page;
			m_pageLayoutBottom = m_pageLayoutResult.Bounds.Bottom;
			isNextPage = false;
		}
		m_pageLayoutResult = m_template.Draw(m_pageTemplate, htmlToPdfFormat, new RectangleF(0f, m_pageLayoutBottom, page.Graphics.ClientSize.Width, page.Graphics.ClientSize.Height));
	}

	public void SetItemStyle(int headingStyle, HtmlToPdfTocStyle tocStyle)
	{
		if (headingStyle <= 6 && headingStyle > 0)
		{
			HtmlToPdfTocStyle htmlToPdfTocStyle = m_headerStyleCollection[headingStyle - 1];
			if (tocStyle.BackgroundColor != null)
			{
				htmlToPdfTocStyle.BackgroundColor = tocStyle.BackgroundColor;
			}
			if (tocStyle.Font != null)
			{
				htmlToPdfTocStyle.Font = tocStyle.Font;
			}
			if (tocStyle.ForeColor != null)
			{
				htmlToPdfTocStyle.ForeColor = tocStyle.ForeColor;
			}
			if (tocStyle.Padding != null)
			{
				htmlToPdfTocStyle.Padding = tocStyle.Padding;
			}
		}
	}
}
