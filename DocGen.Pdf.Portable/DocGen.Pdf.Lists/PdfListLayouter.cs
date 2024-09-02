using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

internal class PdfListLayouter : ElementLayouter
{
	private PdfGraphics m_graphics;

	private bool m_finish;

	private PdfList m_curList;

	private Stack<ListInfo> m_info = new Stack<ListInfo>();

	private int m_index;

	private float m_indent;

	private float m_resultHeight;

	private RectangleF m_bounds;

	private PdfPage currentPage;

	private SizeF size;

	private bool usePaginateBounds = true;

	private PdfBrush currentBrush;

	private PdfPen currentPen;

	private PdfFont currentFont;

	private PdfStringFormat currentFormat;

	private float markerMaxWidth;

	public new PdfList Element => base.Element as PdfList;

	public PdfListLayouter(PdfList element)
		: base(element)
	{
	}

	public void Layout(PdfGraphics graphics, float x, float y)
	{
		PointF location = new PointF(x, y);
		RectangleF boundaries = new RectangleF(location, SizeF.Empty);
		Layout(graphics, boundaries);
	}

	public void Layout(PdfGraphics graphics, PointF point)
	{
		RectangleF boundaries = new RectangleF(point, SizeF.Empty);
		Layout(graphics, boundaries);
	}

	public void Layout(PdfGraphics graphics, RectangleF boundaries)
	{
		if (graphics == null)
		{
			throw new ArgumentNullException("graphics");
		}
		m_graphics = graphics;
		PdfLayoutParams pdfLayoutParams = new PdfLayoutParams();
		pdfLayoutParams.Bounds = boundaries;
		pdfLayoutParams.Format = new PdfLayoutFormat();
		pdfLayoutParams.Format.Layout = PdfLayoutType.OnePage;
		LayoutInternal(pdfLayoutParams);
	}

	protected override PdfLayoutResult LayoutInternal(PdfLayoutParams param)
	{
		currentPage = param.Page;
		m_bounds = param.Bounds;
		if (param.Bounds.Size == SizeF.Empty && currentPage != null)
		{
			m_bounds.Size = currentPage.GetClientSize();
			m_bounds.Width -= m_bounds.X;
			m_bounds.Height -= m_bounds.Y;
		}
		if (currentPage != null)
		{
			m_graphics = currentPage.Graphics;
		}
		PageLayoutResult pageLayoutResult = new PageLayoutResult();
		pageLayoutResult.Broken = false;
		pageLayoutResult.Y = m_bounds.Y;
		m_curList = Element;
		m_indent = Element.Indent;
		SetCurrentParameters(Element);
		if (Element.Brush == null)
		{
			currentBrush = PdfBrushes.Black;
		}
		if (Element.Font == null)
		{
			currentFont = PdfDocument.DefaultFont;
		}
		if (m_curList is PdfOrderedList)
		{
			markerMaxWidth = GetMarkerMaxWidth(m_curList as PdfOrderedList, m_info);
		}
		bool flag = param.Format.Layout == PdfLayoutType.OnePage;
		while (!m_finish)
		{
			bool flag2 = BeforePageLayout(m_bounds, currentPage, m_curList);
			pageLayoutResult.Y = m_bounds.Y;
			ListEndPageLayoutEventArgs listEndPageLayoutEventArgs = null;
			if (!flag2)
			{
				if ((param.Page != null && param.Page.Document != null && param.Page.Document.AutoTag) || m_curList.PdfTag != null)
				{
					PdfCatalog.StructTreeRoot.IsNewList = true;
					if (m_curList.PdfTag == null)
					{
						m_curList.PdfTag = new PdfStructureElement(PdfTagType.List);
					}
				}
				pageLayoutResult = LayoutOnPage(pageLayoutResult);
				listEndPageLayoutEventArgs = AfterPageLayouted(m_bounds, currentPage, m_curList);
				flag2 = listEndPageLayoutEventArgs?.Cancel ?? false;
			}
			if (flag || flag2)
			{
				break;
			}
			if (currentPage != null && !m_finish)
			{
				if (listEndPageLayoutEventArgs != null && listEndPageLayoutEventArgs.NextPage != null)
				{
					currentPage = listEndPageLayoutEventArgs.NextPage;
				}
				else
				{
					currentPage = GetNextPage(currentPage);
				}
				m_graphics = currentPage.Graphics;
				if (param.Bounds.Size == SizeF.Empty)
				{
					m_bounds.Size = currentPage.GetClientSize();
					m_bounds.Width -= m_bounds.X;
					m_bounds.Height -= m_bounds.Y;
				}
				if (param.Format != null && param.Format.UsePaginateBounds && usePaginateBounds)
				{
					m_bounds = param.Format.PaginateBounds;
				}
			}
		}
		m_info.Clear();
		PdfLayoutResult result = new PdfLayoutResult(bounds: new RectangleF(m_bounds.X, pageLayoutResult.Y, m_bounds.Width, m_resultHeight), page: currentPage);
		if (currentFormat != null)
		{
			currentFormat.m_isList = false;
		}
		return result;
	}

	private float GetMarkerMaxWidth(PdfOrderedList list, Stack<ListInfo> info)
	{
		float num = -1f;
		for (int i = 0; i < list.Items.Count; i++)
		{
			PdfStringLayoutResult pdfStringLayoutResult = CreateOrderedMarkerResult(list, list.Items[i], i + list.Marker.StartNumber, info, findMaxWidth: true);
			if (num < pdfStringLayoutResult.ActualSize.Width)
			{
				num = pdfStringLayoutResult.ActualSize.Width;
			}
		}
		return num;
	}

	private void SetCurrentParameters(PdfList list)
	{
		if (list.Brush != null)
		{
			currentBrush = list.Brush;
		}
		if (list.Pen != null)
		{
			currentPen = list.Pen;
		}
		if (list.Font != null)
		{
			currentFont = list.Font;
		}
		if (list.StringFormat != null)
		{
			currentFormat = list.StringFormat;
			currentFormat.m_isList = true;
		}
	}

	private void SetCurrentParameters(PdfListItem item)
	{
		if (item.Brush != null)
		{
			currentBrush = item.Brush;
		}
		if (item.Pen != null)
		{
			currentPen = item.Pen;
		}
		if (item.Font != null)
		{
			currentFont = item.Font;
		}
		if (item.StringFormat != null)
		{
			currentFormat = item.StringFormat;
		}
	}

	private PageLayoutResult LayoutOnPage(PageLayoutResult pageResult)
	{
		float height = 0f;
		float num = 0f;
		float y = m_bounds.Y;
		float x = m_bounds.X;
		size = m_bounds.Size;
		size.Width -= m_indent;
		while (true)
		{
			if (m_index < m_curList.Items.Count)
			{
				PdfListItem pdfListItem = m_curList.Items[m_index];
				if (currentPage != null && !pageResult.Broken)
				{
					BeforeItemLayout(pdfListItem, currentPage);
				}
				if ((currentPage != null && currentPage.Document != null && currentPage.Document.AutoTag) || pdfListItem.PdfTag != null)
				{
					PdfCatalog.StructTreeRoot.IsNewListItem = true;
					if (pdfListItem.PdfTag == null)
					{
						pdfListItem.PdfTag = new PdfStructureElement(PdfTagType.ListItem);
					}
				}
				DrawItem(ref pageResult, x, m_curList, m_index, m_indent, m_info, pdfListItem, ref height, ref y);
				num += height;
				if (pageResult.Broken)
				{
					return pageResult;
				}
				if (currentPage != null)
				{
					AfterItemLayouted(pdfListItem, currentPage);
				}
				if (PdfCatalog.StructTreeRoot != null && m_index == m_curList.Items.Count - 1 && PdfCatalog.StructTreeRoot.m_isSubList)
				{
					PdfCatalog.StructTreeRoot.m_isSubList = false;
				}
				pageResult.MarkerWrote = false;
				if (pdfListItem.SubList != null && pdfListItem.SubList.Items.Count > 0)
				{
					if (m_curList is PdfOrderedList)
					{
						PdfOrderedList pdfOrderedList = m_curList as PdfOrderedList;
						pdfOrderedList.Marker.CurrentIndex = m_index;
						ListInfo listInfo = new ListInfo(m_curList, m_index, pdfOrderedList.Marker.GetNumber());
						listInfo.Brush = currentBrush;
						listInfo.Font = currentFont;
						listInfo.Format = currentFormat;
						listInfo.Pen = currentPen;
						listInfo.MarkerWidth = markerMaxWidth;
						m_info.Push(listInfo);
					}
					else
					{
						ListInfo listInfo2 = new ListInfo(m_curList, m_index);
						listInfo2.Brush = currentBrush;
						listInfo2.Font = currentFont;
						listInfo2.Format = currentFormat;
						listInfo2.Pen = currentPen;
						m_info.Push(listInfo2);
					}
					m_curList = pdfListItem.SubList;
					if ((currentPage != null && currentPage.Document != null && currentPage.Document.AutoTag) || pdfListItem.SubList.PdfTag != null)
					{
						PdfCatalog.StructTreeRoot.IsNewList = true;
						PdfCatalog.StructTreeRoot.m_isSubList = true;
						if (pdfListItem.SubList.PdfTag == null)
						{
							pdfListItem.SubList.PdfTag = new PdfStructureElement(PdfTagType.List);
						}
					}
					if (m_curList is PdfOrderedList)
					{
						markerMaxWidth = GetMarkerMaxWidth(m_curList as PdfOrderedList, m_info);
					}
					m_index = -1;
					m_indent += m_curList.Indent;
					size.Width -= m_curList.Indent;
					SetCurrentParameters(pdfListItem);
					SetCurrentParameters(m_curList);
				}
				m_index++;
			}
			else
			{
				if (m_info.Count == 0)
				{
					break;
				}
				ListInfo listInfo3 = m_info.Pop();
				m_index = listInfo3.Index + 1;
				m_indent -= m_curList.Indent;
				size.Width += m_curList.Indent;
				markerMaxWidth = listInfo3.MarkerWidth;
				currentBrush = listInfo3.Brush;
				currentPen = listInfo3.Pen;
				currentFont = listInfo3.Font;
				currentFormat = listInfo3.Format;
				m_curList = listInfo3.List;
			}
		}
		m_resultHeight = num;
		m_finish = true;
		return pageResult;
	}

	private void DrawItem(ref PageLayoutResult pageResult, float x, PdfList curList, int index, float indent, Stack<ListInfo> info, PdfListItem item, ref float height, ref float y)
	{
		PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
		PdfStringLayoutResult pdfStringLayoutResult = null;
		PdfStringLayoutResult pdfStringLayoutResult2 = null;
		bool flag = false;
		float textIndent = curList.TextIndent;
		float num = height + y;
		float num2 = indent + x;
		float num3 = 0f;
		float num4 = 0f;
		SizeF sizeF = size;
		string text = item.Text;
		string text2 = null;
		PdfBrush brush = currentBrush;
		if (item.Brush != null)
		{
			brush = item.Brush;
		}
		PdfPen pen = currentPen;
		if (item.Pen != null)
		{
			pen = item.Pen;
		}
		PdfFont font = currentFont;
		if (item.Font != null)
		{
			font = item.Font;
		}
		PdfStringFormat pdfStringFormat = currentFormat;
		if (item.StringFormat != null)
		{
			pdfStringFormat = item.StringFormat;
		}
		if ((size.Width <= 0f || size.Width < font.Size) && currentPage != null)
		{
			throw new Exception("There is not enough space to layout list.");
		}
		size.Height -= height;
		PdfMarker pdfMarker = null;
		if (curList is PdfUnorderedList)
		{
			pdfMarker = (curList as PdfUnorderedList).Marker;
			if ((curList as PdfUnorderedList).PdfTag != null)
			{
				(item.PdfTag as PdfStructureElement).Parent = (curList as PdfUnorderedList).PdfTag as PdfStructureElement;
			}
		}
		else
		{
			pdfMarker = (curList as PdfOrderedList).Marker;
			if ((curList as PdfOrderedList).PdfTag != null)
			{
				(item.PdfTag as PdfStructureElement).Parent = (curList as PdfOrderedList).PdfTag as PdfStructureElement;
			}
		}
		if (pageResult.Broken)
		{
			text = pageResult.ItemText;
			text2 = pageResult.MarkerText;
		}
		bool flag2 = true;
		if (text2 != null && pdfMarker is PdfUnorderedMarker && (pdfMarker as PdfUnorderedMarker).Style == PdfUnorderedMarkerStyle.CustomString)
		{
			pdfStringLayoutResult = pdfStringLayouter.Layout(text2, GetMarkerFont(pdfMarker, item), GetMarkerFormat(pdfMarker, item), size);
			num2 += pdfStringLayoutResult.ActualSize.Width;
			pageResult.MarkerWidth = pdfStringLayoutResult.ActualSize.Width;
			num4 = pdfStringLayoutResult.ActualSize.Height;
			flag2 = true;
		}
		else
		{
			pdfStringLayoutResult = CreateMarkerResult(index, curList, info, item);
			if (pdfStringLayoutResult != null)
			{
				if (curList is PdfOrderedList)
				{
					num2 += markerMaxWidth;
					pageResult.MarkerWidth = markerMaxWidth;
				}
				else
				{
					num2 += pdfStringLayoutResult.ActualSize.Width;
					pageResult.MarkerWidth = pdfStringLayoutResult.ActualSize.Width;
				}
				num4 = pdfStringLayoutResult.ActualSize.Height;
				if (currentPage != null)
				{
					flag2 = num4 < size.Height;
				}
				if (pdfStringLayoutResult.Empty)
				{
					flag2 = false;
				}
			}
			else
			{
				num2 += (pdfMarker as PdfUnorderedMarker).Size.Width;
				pageResult.MarkerWidth = (pdfMarker as PdfUnorderedMarker).Size.Width;
				num4 = (pdfMarker as PdfUnorderedMarker).Size.Height;
				if (currentPage != null)
				{
					flag2 = num4 < size.Height;
				}
			}
		}
		if (text2 == null || text2 == string.Empty)
		{
			flag2 = true;
		}
		if (text != null && flag2)
		{
			sizeF = size;
			sizeF.Width -= pageResult.MarkerWidth;
			if (item.TextIndent == 0f)
			{
				sizeF.Width -= textIndent;
			}
			else
			{
				sizeF.Width -= item.TextIndent;
			}
			if ((sizeF.Width <= 0f || sizeF.Width < font.Size) && currentPage != null)
			{
				throw new Exception("There is not enough space to layout the item text. Marker is too long or there is no enough space to draw it.");
			}
			float num5 = num2;
			if (!pdfMarker.RightToLeft)
			{
				num5 = ((item.TextIndent != 0f) ? (num5 + item.TextIndent) : (num5 + textIndent));
			}
			else
			{
				num5 -= pageResult.MarkerWidth;
				if (pdfStringFormat != null && (pdfStringFormat.Alignment == PdfTextAlignment.Right || pdfStringFormat.Alignment == PdfTextAlignment.Center))
				{
					num5 -= indent;
				}
			}
			if (currentPage == null && pdfStringFormat != null)
			{
				pdfStringFormat = (PdfStringFormat)pdfStringFormat.Clone();
				pdfStringFormat.Alignment = PdfTextAlignment.Left;
			}
			pdfStringLayoutResult2 = pdfStringLayouter.Layout(text, font, pdfStringFormat, sizeF);
			RectangleF layoutRectangle = new RectangleF(num5, num, sizeF.Width, sizeF.Height);
			if ((currentPage != null && currentPage.Document != null && currentPage.Document.AutoTag) || item.PdfTag != null)
			{
				m_graphics.Tag = new PdfStructureElement(PdfTagType.ListBody);
				(m_graphics.Tag as PdfStructureElement).Parent = item.PdfTag as PdfStructureElement;
			}
			m_graphics.DrawStringLayoutResult(pdfStringLayoutResult2, font, pen, brush, layoutRectangle, pdfStringFormat);
			y = num;
			num3 = pdfStringLayoutResult2.ActualSize.Height;
		}
		height = ((num3 < num4) ? num4 : num3);
		if ((pdfStringLayoutResult2 != null && !IsNullOrEmpty(pdfStringLayoutResult2.Remainder)) || (pdfStringLayoutResult != null && !IsNullOrEmpty(pdfStringLayoutResult.Remainder)) || !flag2)
		{
			y = 0f;
			height = 0f;
			if (pdfStringLayoutResult2 != null)
			{
				pageResult.ItemText = pdfStringLayoutResult2.Remainder;
				if (pdfStringLayoutResult2.Remainder == item.Text)
				{
					flag2 = false;
				}
			}
			else if (!flag2)
			{
				pageResult.ItemText = item.Text;
			}
			else
			{
				pageResult.ItemText = null;
			}
			if (pdfStringLayoutResult != null)
			{
				pageResult.MarkerText = pdfStringLayoutResult.Remainder;
			}
			else
			{
				pageResult.MarkerText = null;
			}
			pageResult.Broken = true;
			pageResult.Y = 0f;
			m_bounds.Y = 0f;
		}
		else
		{
			pageResult.Broken = false;
		}
		if (pdfStringLayoutResult2 != null)
		{
			pageResult.MarkerX = num2;
			if (pdfStringFormat != null)
			{
				switch (pdfStringFormat.Alignment)
				{
				case PdfTextAlignment.Right:
					pageResult.MarkerX = num2 + sizeF.Width - pdfStringLayoutResult2.ActualSize.Width;
					break;
				case PdfTextAlignment.Center:
					pageResult.MarkerX = num2 + sizeF.Width / 2f - pdfStringLayoutResult2.ActualSize.Width / 2f;
					break;
				}
			}
			if (pdfMarker.RightToLeft)
			{
				pageResult.MarkerX += pdfStringLayoutResult2.ActualSize.Width;
				if (item.TextIndent == 0f)
				{
					pageResult.MarkerX += textIndent;
				}
				else
				{
					pageResult.MarkerX += item.TextIndent;
				}
				if (pdfStringFormat != null && (pdfStringFormat.Alignment == PdfTextAlignment.Right || pdfStringFormat.Alignment == PdfTextAlignment.Center))
				{
					pageResult.MarkerX -= indent;
				}
			}
		}
		if (pdfMarker is PdfUnorderedMarker && (pdfMarker as PdfUnorderedMarker).Style == PdfUnorderedMarkerStyle.CustomString)
		{
			if (pdfStringLayoutResult != null)
			{
				flag = DrawMarker(curList, item, pdfStringLayoutResult, num, pageResult.MarkerX);
				pageResult.MarkerWrote = true;
				pageResult.MarkerWidth = pdfStringLayoutResult.ActualSize.Width;
			}
		}
		else if (flag2 && !pageResult.MarkerWrote)
		{
			flag = DrawMarker(curList, item, pdfStringLayoutResult, num, pageResult.MarkerX);
			pageResult.MarkerWrote = flag;
			if (curList is PdfOrderedList)
			{
				pageResult.MarkerWidth = pdfStringLayoutResult.ActualSize.Width;
			}
			else
			{
				pageResult.MarkerWidth = (pdfMarker as PdfUnorderedMarker).Size.Width;
			}
		}
	}

	private static bool IsNullOrEmpty(string text)
	{
		if (text != null)
		{
			return text == string.Empty;
		}
		return true;
	}

	private void AfterItemLayouted(PdfListItem item, PdfPage page)
	{
		EndItemLayoutEventArgs args = new EndItemLayoutEventArgs(item, page);
		Element.OnEndItemLayout(args);
	}

	private void BeforeItemLayout(PdfListItem item, PdfPage page)
	{
		BeginItemLayoutEventArgs args = new BeginItemLayoutEventArgs(item, page);
		Element.OnBeginItemLayout(args);
	}

	private ListEndPageLayoutEventArgs AfterPageLayouted(RectangleF currentBounds, PdfPage currentPage, PdfList list)
	{
		ListEndPageLayoutEventArgs listEndPageLayoutEventArgs = null;
		if (Element.RaiseEndPageLayout && currentPage != null)
		{
			listEndPageLayoutEventArgs = new ListEndPageLayoutEventArgs(new PdfLayoutResult(currentPage, currentBounds), list);
			Element.OnEndPageLayout(listEndPageLayoutEventArgs);
		}
		return listEndPageLayoutEventArgs;
	}

	private bool BeforePageLayout(RectangleF currentBounds, PdfPage currentPage, PdfList list)
	{
		bool result = false;
		if (Element.RaiseBeginPageLayout && currentPage != null)
		{
			ListBeginPageLayoutEventArgs listBeginPageLayoutEventArgs = new ListBeginPageLayoutEventArgs(currentBounds, currentPage, list);
			Element.OnBeginPageLayout(listBeginPageLayoutEventArgs);
			result = listBeginPageLayoutEventArgs.Cancel;
			m_bounds = listBeginPageLayoutEventArgs.Bounds;
			usePaginateBounds = false;
		}
		return result;
	}

	private PdfStringLayoutResult CreateMarkerResult(int index, PdfList curList, Stack<ListInfo> info, PdfListItem item)
	{
		PdfStringLayoutResult pdfStringLayoutResult = null;
		if (curList is PdfOrderedList)
		{
			return CreateOrderedMarkerResult(curList, item, index, info, findMaxWidth: false);
		}
		SizeF markerSize = SizeF.Empty;
		return CreateUnorderedMarkerResult(curList, item, ref markerSize);
	}

	private PdfStringLayoutResult CreateUnorderedMarkerResult(PdfList curList, PdfListItem item, ref SizeF markerSize)
	{
		PdfUnorderedMarker marker = (curList as PdfUnorderedList).Marker;
		PdfStringLayoutResult pdfStringLayoutResult = null;
		PdfFont markerFont = GetMarkerFont(marker, item);
		PdfStringFormat markerFormat = GetMarkerFormat(marker, item);
		PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
		switch (marker.Style)
		{
		case PdfUnorderedMarkerStyle.CustomImage:
			markerSize = new SizeF(markerFont.Size, markerFont.Size);
			marker.Size = markerSize;
			break;
		case PdfUnorderedMarkerStyle.CustomTemplate:
			markerSize = new SizeF(markerFont.Size, markerFont.Size);
			marker.Size = markerSize;
			break;
		case PdfUnorderedMarkerStyle.CustomString:
			pdfStringLayoutResult = pdfStringLayouter.Layout(marker.Text, markerFont, markerFormat, size);
			break;
		default:
		{
			PdfStandardFont font = new PdfStandardFont(PdfFontFamily.ZapfDingbats, markerFont.Size);
			pdfStringLayoutResult = pdfStringLayouter.Layout(marker.GetStyledText(), font, null, size);
			marker.Size = pdfStringLayoutResult.ActualSize;
			if (marker.Pen != null)
			{
				pdfStringLayoutResult.m_actualSize = new SizeF(pdfStringLayoutResult.ActualSize.Width + 2f * marker.Pen.Width, pdfStringLayoutResult.ActualSize.Height + 2f * marker.Pen.Width);
			}
			break;
		}
		}
		return pdfStringLayoutResult;
	}

	private PdfStringLayoutResult CreateOrderedMarkerResult(PdfList list, PdfListItem item, int index, Stack<ListInfo> info, bool findMaxWidth)
	{
		PdfOrderedList pdfOrderedList = list as PdfOrderedList;
		PdfOrderedMarker marker = pdfOrderedList.Marker;
		marker.CurrentIndex = index;
		string text = string.Empty;
		if (pdfOrderedList.Marker.Style != 0)
		{
			text = pdfOrderedList.Marker.GetNumber() + pdfOrderedList.Marker.Suffix;
		}
		if (pdfOrderedList.MarkerHierarchy)
		{
			object[] array = info.ToArray();
			object[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				ListInfo listInfo = (ListInfo)array2[i];
				if (!(listInfo.List is PdfOrderedList pdfOrderedList2) || pdfOrderedList2.Marker.Style == PdfNumberStyle.None)
				{
					break;
				}
				marker = pdfOrderedList2.Marker;
				text = listInfo.Number + marker.Delimiter + text;
				if (!pdfOrderedList2.MarkerHierarchy)
				{
					break;
				}
			}
		}
		PdfStringLayouter pdfStringLayouter = new PdfStringLayouter();
		pdfOrderedList = list as PdfOrderedList;
		marker = pdfOrderedList.Marker;
		PdfFont markerFont = GetMarkerFont(marker, item);
		PdfStringFormat pdfStringFormat = GetMarkerFormat(marker, item);
		SizeF sizeF = new SizeF(size.Width, size.Height);
		if (!findMaxWidth)
		{
			sizeF.Width = markerMaxWidth;
			pdfStringFormat = SetMarkerStringFormat(marker, pdfStringFormat);
		}
		return pdfStringLayouter.Layout(text, markerFont, pdfStringFormat, sizeF);
	}

	private PdfStringFormat SetMarkerStringFormat(PdfOrderedMarker marker, PdfStringFormat markerFormat)
	{
		markerFormat = ((markerFormat != null) ? ((PdfStringFormat)markerFormat.Clone()) : new PdfStringFormat());
		if (marker.StringFormat == null)
		{
			markerFormat.Alignment = PdfTextAlignment.Right;
			if (marker.RightToLeft)
			{
				markerFormat.Alignment = PdfTextAlignment.Left;
			}
		}
		if (currentPage == null && markerFormat != null)
		{
			markerFormat = (PdfStringFormat)markerFormat.Clone();
			markerFormat.Alignment = PdfTextAlignment.Left;
		}
		return markerFormat;
	}

	private bool DrawMarker(PdfList curList, PdfListItem item, PdfStringLayoutResult markerResult, float posY, float posX)
	{
		if (curList is PdfOrderedList)
		{
			if (curList.Font != null && markerResult != null && curList.Font.Size > markerResult.m_actualSize.Height)
			{
				posY += curList.Font.Size / 2f - markerResult.m_actualSize.Height / 2f;
				markerResult.m_actualSize.Height += posY;
			}
			DrawOrderedMarker(curList, markerResult, item, posX, posY);
		}
		else
		{
			if (curList.Font != null && markerResult != null && curList.Font.Size > markerResult.m_actualSize.Height)
			{
				posY += curList.Font.Size / 2f - markerResult.m_actualSize.Height / 2f;
				markerResult.m_actualSize.Height += posY;
			}
			DrawUnorderedMarker(curList, markerResult, item, posX, posY);
		}
		return true;
	}

	private PdfStringLayoutResult DrawUnorderedMarker(PdfList curList, PdfStringLayoutResult markerResult, PdfListItem item, float posX, float posY)
	{
		PdfUnorderedMarker marker = (curList as PdfUnorderedList).Marker;
		PdfFont markerFont = GetMarkerFont(marker, item);
		PdfPen markerPen = GetMarkerPen(marker, item);
		PdfBrush markerBrush = GetMarkerBrush(marker, item);
		PdfStringFormat markerFormat = GetMarkerFormat(marker, item);
		if ((currentPage != null && currentPage.Document != null && currentPage.Document.AutoTag) || item.PdfTag != null)
		{
			m_graphics.Tag = new PdfStructureElement(PdfTagType.Label);
			(m_graphics.Tag as PdfStructureElement).Parent = item.PdfTag as PdfStructureElement;
		}
		if (markerResult != null)
		{
			PointF pointF = new PointF(posX - markerResult.ActualSize.Width, posY);
			marker.Size = markerResult.ActualSize;
			if (marker.Style == PdfUnorderedMarkerStyle.CustomString)
			{
				RectangleF layoutRectangle = new RectangleF(pointF, markerResult.ActualSize);
				m_graphics.DrawStringLayoutResult(markerResult, markerFont, markerPen, markerBrush, layoutRectangle, markerFormat);
			}
			else
			{
				marker.UnicodeFont = new PdfStandardFont(PdfFontFamily.ZapfDingbats, markerFont.Size);
				marker.Draw(m_graphics, pointF, markerBrush, markerPen);
			}
		}
		else
		{
			marker.Size = new SizeF(markerFont.Size, markerFont.Size);
			marker.Draw(point: new PointF(posX - markerFont.Size, posY), graphics: m_graphics, brush: markerBrush, pen: markerPen);
		}
		return null;
	}

	private PdfStringLayoutResult DrawOrderedMarker(PdfList curList, PdfStringLayoutResult markerResult, PdfListItem item, float posX, float posY)
	{
		PdfOrderedMarker marker = (curList as PdfOrderedList).Marker;
		PdfFont markerFont = GetMarkerFont(marker, item);
		PdfStringFormat markerFormat = GetMarkerFormat(marker, item);
		PdfPen markerPen = GetMarkerPen(marker, item);
		PdfBrush markerBrush = GetMarkerBrush(marker, item);
		PointF location = new PointF(posX - markerMaxWidth, posY);
		RectangleF layoutRectangle = new RectangleF(location, markerResult.ActualSize);
		layoutRectangle.Width = markerMaxWidth;
		markerFormat = SetMarkerStringFormat(marker, markerFormat);
		if ((currentPage != null && currentPage.Document != null && currentPage.Document.AutoTag) || item.PdfTag != null)
		{
			m_graphics.Tag = new PdfStructureElement(PdfTagType.Label);
			(m_graphics.Tag as PdfStructureElement).Parent = item.PdfTag as PdfStructureElement;
		}
		m_graphics.DrawStringLayoutResult(markerResult, markerFont, markerPen, markerBrush, layoutRectangle, markerFormat);
		return markerResult;
	}

	private PdfFont GetMarkerFont(PdfMarker marker, PdfListItem item)
	{
		PdfFont font = marker.Font;
		if (marker.Font == null)
		{
			font = item.Font;
			if (item.Font == null)
			{
				font = currentFont;
			}
		}
		marker.Font = font;
		return font;
	}

	private PdfStringFormat GetMarkerFormat(PdfMarker marker, PdfListItem item)
	{
		PdfStringFormat stringFormat = marker.StringFormat;
		if (marker.StringFormat == null)
		{
			stringFormat = item.StringFormat;
			if (item.StringFormat == null)
			{
				stringFormat = currentFormat;
			}
		}
		return stringFormat;
	}

	private PdfPen GetMarkerPen(PdfMarker marker, PdfListItem item)
	{
		PdfPen pen = marker.Pen;
		if (marker.Pen == null)
		{
			pen = item.Pen;
			if (item.Pen == null)
			{
				pen = currentPen;
			}
		}
		return pen;
	}

	private PdfBrush GetMarkerBrush(PdfMarker marker, PdfListItem item)
	{
		PdfBrush brush = marker.Brush;
		if (marker.Brush == null)
		{
			brush = item.Brush;
			if (item.Brush == null)
			{
				brush = currentBrush;
			}
		}
		return brush;
	}
}
