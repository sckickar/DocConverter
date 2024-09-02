using System;
using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal class TextLayouter : ElementLayouter
{
	private struct TextPageLayoutResult
	{
		public PdfPage Page;

		public RectangleF Bounds;

		public bool End;

		public string Remainder;

		public RectangleF LastLineBounds;
	}

	private PdfStringFormat m_format;

	private float m_maxValue;

	public new PdfTextElement Element => base.Element as PdfTextElement;

	public TextLayouter(PdfTextElement element)
		: base(element)
	{
	}

	protected override PdfLayoutResult LayoutInternal(PdfLayoutParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		m_format = ((Element.StringFormat != null) ? ((PdfStringFormat)Element.StringFormat.Clone()) : null);
		PdfPage pdfPage = param.Page;
		RectangleF currentBounds = param.Bounds;
		string text = Element.Value;
		PdfTextLayoutResult pdfTextLayoutResult = null;
		m_maxValue = currentBounds.Height;
		TextPageLayoutResult pageResult = default(TextPageLayoutResult);
		if (pdfPage.Document != null)
		{
			_ = pdfPage.Document.PageCount;
		}
		pageResult.Page = pdfPage;
		pageResult.Remainder = text;
		while (true)
		{
			bool flag = RaiseBeforePageLayout(pdfPage, ref currentBounds);
			EndTextPageLayoutEventArgs endTextPageLayoutEventArgs = null;
			if (!flag)
			{
				pageResult = LayoutOnPage(text, pdfPage, currentBounds, param);
				float height = pageResult.Page.Graphics.ClientSize.Height;
				float y = currentBounds.Y;
				float height2 = Element.Font.Height;
				float num = y + height2;
				endTextPageLayoutEventArgs = RaisePageLayouted(pageResult);
				flag = endTextPageLayoutEventArgs?.Cancel ?? false;
				if (text != string.Empty && pageResult.LastLineBounds == RectangleF.Empty && !pageResult.End && text == pageResult.Remainder && Element.Font.GetLineWidth(text, m_format) > currentBounds.Width && m_format == null && height > num)
				{
					pdfTextLayoutResult = GetLayoutResult(pageResult);
					if (param.Format == null || param.Format.Break != PdfLayoutBreakType.FitElement)
					{
						break;
					}
				}
			}
			if (!pageResult.End && !flag)
			{
				if (Element.ispdfTextElement)
				{
					pdfTextLayoutResult = GetLayoutResult(pageResult);
					break;
				}
				currentBounds = GetPaginateBounds(param);
				if (Element.m_pdfHtmlTextElement && param.Format.UsePaginateBounds && currentBounds.Height > param.Bounds.Height)
				{
					currentBounds.Height = param.Bounds.Height;
				}
				text = pageResult.Remainder;
				PdfPage pdfPage2 = ((endTextPageLayoutEventArgs == null || endTextPageLayoutEventArgs.NextPage == null) ? GetNextPage(pdfPage) : endTextPageLayoutEventArgs.NextPage);
				if (Element != null && Element.m_isPdfGrid)
				{
					currentBounds = ((pdfPage2 == null || param.Format.PaginateBounds.Y == 0f) ? new RectangleF(param.Bounds.X, 0f, param.Bounds.Width, pdfPage.GetClientSize().Height) : new RectangleF(param.Bounds.X, param.Format.PaginateBounds.Y, param.Bounds.Width, pdfPage.GetClientSize().Height));
				}
				pdfPage = pdfPage2;
				continue;
			}
			pdfTextLayoutResult = GetLayoutResult(pageResult);
			break;
		}
		return pdfTextLayoutResult;
	}

	private PdfTextLayoutResult GetLayoutResult(TextPageLayoutResult pageResult)
	{
		return new PdfTextLayoutResult(pageResult.Page, pageResult.Bounds, pageResult.Remainder, pageResult.LastLineBounds);
	}

	private TextPageLayoutResult LayoutOnPage(string text, PdfPage currentPage, RectangleF currentBounds, PdfLayoutParams param)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (currentPage == null)
		{
			throw new ArgumentNullException("currentPage");
		}
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		TextPageLayoutResult result = default(TextPageLayoutResult);
		result.Remainder = text;
		result.Page = currentPage;
		currentBounds = CheckCorrectBounds(currentPage, currentBounds);
		if (currentBounds.Height < 0f)
		{
			currentPage = GetNextPage(currentPage);
			_ = currentPage.Section.PageSettings.Margins;
			result.Page = currentPage;
			currentBounds = new RectangleF(currentBounds.X, 0f, currentBounds.Width, currentBounds.Height);
		}
		PdfStringLayoutResult pdfStringLayoutResult = new PdfStringLayouter().Layout(text, Element.Font, m_format, currentBounds, currentPage.GetClientSize().Height);
		bool flag = pdfStringLayoutResult.Remainder == null || pdfStringLayoutResult.Remainder.Length == 0;
		bool flag2 = (param.Format.Break != PdfLayoutBreakType.FitElement || currentPage != param.Page || flag) && !pdfStringLayoutResult.Empty;
		if (flag2)
		{
			PdfGraphics graphics = currentPage.Graphics;
			if (Element.PdfTag != null)
			{
				graphics.Tag = Element.PdfTag;
			}
			graphics.DrawStringLayoutResult(pdfStringLayoutResult, Element.Font, Element.Pen, Element.ObtainBrush(), currentBounds, m_format);
			LineInfo lineInfo = pdfStringLayoutResult.Lines[pdfStringLayoutResult.LineCount - 1];
			result.LastLineBounds = graphics.GetLineBounds(pdfStringLayoutResult.LineCount - 1, pdfStringLayoutResult, Element.Font, currentBounds, m_format);
			result.Bounds = GetTextPageBounds(currentPage, currentBounds, pdfStringLayoutResult);
			result.Remainder = pdfStringLayoutResult.Remainder;
			CheckCorectStringFormat(lineInfo);
		}
		else
		{
			result.Bounds = GetTextPageBounds(currentPage, currentBounds, pdfStringLayoutResult);
		}
		bool flag3 = (pdfStringLayoutResult.Empty && param.Format.Break != PdfLayoutBreakType.FitElement && param.Format.Layout != PdfLayoutType.Paginate && flag2) || (param.Format.Break == PdfLayoutBreakType.FitElement && currentPage != param.Page);
		result.End = flag || flag3 || param.Format.Layout == PdfLayoutType.OnePage;
		if (!result.End && Element.m_pdfHtmlTextElement)
		{
			if (result.Bounds.Height != 0f && m_maxValue < result.Bounds.Height + Element.Font.Height)
			{
				result.End = true;
			}
			else
			{
				param.Bounds = new RectangleF(currentBounds.X, currentBounds.Y, currentBounds.Width, m_maxValue - result.Bounds.Height);
				m_maxValue = param.Bounds.Height;
			}
		}
		if (!result.End && Element.m_pdfHtmlTextElement && result.Bounds.Height == 0f && currentBounds.Y + Element.Font.Height <= currentPage.GetClientSize().Height)
		{
			currentBounds = new RectangleF(currentBounds.X, currentBounds.Y, currentBounds.Width, currentPage.GetClientSize().Height);
			return LayoutOnPage(text, currentPage, currentBounds, param);
		}
		return result;
	}

	private RectangleF CheckCorrectBounds(PdfPage currentPage, RectangleF currentBounds)
	{
		if (currentPage == null)
		{
			throw new ArgumentNullException("currentPage");
		}
		SizeF clientSize = currentPage.Graphics.ClientSize;
		currentBounds.Height = ((currentBounds.Height > 0f) ? currentBounds.Height : (clientSize.Height - currentBounds.Y));
		return currentBounds;
	}

	private RectangleF GetTextPageBounds(PdfPage currentPage, RectangleF currentBounds, PdfStringLayoutResult stringResult)
	{
		if (currentPage == null)
		{
			throw new ArgumentNullException("currentPage");
		}
		if (stringResult == null)
		{
			throw new ArgumentNullException("stringResult");
		}
		SizeF actualSize = stringResult.ActualSize;
		float x = currentBounds.X;
		float y = currentBounds.Y;
		float width = ((currentBounds.Width > 0f) ? currentBounds.Width : actualSize.Width);
		float height = actualSize.Height;
		RectangleF rectangleF = currentPage.Graphics.CheckCorrectLayoutRectangle(actualSize, currentBounds.X, currentBounds.Y, m_format);
		if (currentBounds.Width <= 0f)
		{
			x = rectangleF.X;
		}
		if (currentBounds.Height <= 0f)
		{
			y = rectangleF.Y;
		}
		float textVerticalAlignShift = currentPage.Graphics.GetTextVerticalAlignShift(actualSize.Height, currentBounds.Height, m_format);
		y += textVerticalAlignShift;
		return new RectangleF(x, y, width, height);
	}

	private EndTextPageLayoutEventArgs RaisePageLayouted(TextPageLayoutResult pageResult)
	{
		EndTextPageLayoutEventArgs endTextPageLayoutEventArgs = null;
		if (Element.RaiseEndPageLayout)
		{
			endTextPageLayoutEventArgs = new EndTextPageLayoutEventArgs(GetLayoutResult(pageResult));
			Element.OnEndPageLayout(endTextPageLayoutEventArgs);
		}
		return endTextPageLayoutEventArgs;
	}

	private bool RaiseBeforePageLayout(PdfPage currentPage, ref RectangleF currentBounds)
	{
		bool result = false;
		if (Element.RaiseBeginPageLayout)
		{
			BeginPageLayoutEventArgs beginPageLayoutEventArgs = new BeginPageLayoutEventArgs(currentBounds, currentPage);
			Element.OnBeginPageLayout(beginPageLayoutEventArgs);
			result = beginPageLayoutEventArgs.Cancel;
			currentBounds = beginPageLayoutEventArgs.Bounds;
		}
		return result;
	}

	private void CheckCorectStringFormat(LineInfo lineInfo)
	{
		if (m_format != null)
		{
			m_format.FirstLineIndent = (((lineInfo.LineType & LineType.NewLineBreak) > LineType.None) ? Element.StringFormat.FirstLineIndent : 0f);
		}
	}
}
