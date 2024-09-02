using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Pdf.Graphics.Images.Metafiles;
using DocGen.Pdf.HtmlToPdf;
using DocGen.Pdf.Interactive;

namespace DocGen.Pdf.Graphics;

internal class ShapeLayouter : ElementLayouter
{
	private struct ShapeLayoutResult
	{
		public PdfPage Page;

		public RectangleF Bounds;

		public bool End;
	}

	internal int olderPdfForm;

	[ThreadStatic]
	private static int index;

	[ThreadStatic]
	private static float splitDiff;

	[ThreadStatic]
	private static bool last;

	private const int borderWidth = 0;

	internal bool m_isPdfGrid;

	internal RectangleF shapeBounds = RectangleF.Empty;

	internal float m_bottomCellPadding;

	private double TotalPageSize;

	private TextRegionManager m_textRegions = new TextRegionManager();

	private ImageRegionManager m_imageRegions = new ImageRegionManager();

	private ImageRegionManager m_formRegions = new ImageRegionManager();

	public new PdfShapeElement Element => base.Element as PdfShapeElement;

	private TextRegionManager TextRegions
	{
		get
		{
			return m_textRegions;
		}
		set
		{
			m_textRegions = value;
		}
	}

	private ImageRegionManager ImageRegions
	{
		get
		{
			return m_imageRegions;
		}
		set
		{
			m_imageRegions = value;
		}
	}

	private ImageRegionManager FormRegions
	{
		get
		{
			return m_formRegions;
		}
		set
		{
			m_formRegions = value;
		}
	}

	public ShapeLayouter(PdfShapeElement element)
		: base(element)
	{
	}

	protected override PdfLayoutResult LayoutInternal(PdfLayoutParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		PdfPage pdfPage = param.Page;
		RectangleF currentBounds = param.Bounds;
		RectangleF rectangleF = default(RectangleF);
		rectangleF = ((!(Element is PdfBezierCurve)) ? Element.GetBounds() : currentBounds);
		rectangleF.Location = PointF.Empty;
		if (m_isPdfGrid && shapeBounds != RectangleF.Empty)
		{
			rectangleF = shapeBounds;
		}
		PdfLayoutResult pdfLayoutResult = null;
		ShapeLayoutResult pageResult = default(ShapeLayoutResult);
		pageResult.Page = pdfPage;
		while (true)
		{
			bool flag = RaiseBeforePageLayout(pdfPage, ref currentBounds);
			EndPageLayoutEventArgs endPageLayoutEventArgs = null;
			if (!flag)
			{
				pageResult = LayoutOnPage(pdfPage, currentBounds, rectangleF, param);
				endPageLayoutEventArgs = RaiseEndPageLayout(pageResult);
				flag = endPageLayoutEventArgs?.Cancel ?? false;
			}
			PdfFormFieldCollection fields = pageResult.Page.Document.Form.Fields;
			for (int num = fields.Count - 1; num >= olderPdfForm; num--)
			{
				if (fields[num] is PdfRadioButtonListField)
				{
					PdfRadioButtonListField pdfRadioButtonListField = fields[num] as PdfRadioButtonListField;
					int num2 = 0;
					for (int num3 = olderPdfForm - 1; num3 >= 0; num3--)
					{
						if (fields[num3] is PdfRadioButtonListField)
						{
							PdfRadioButtonListField pdfRadioButtonListField2 = fields[num3] as PdfRadioButtonListField;
							if (pdfRadioButtonListField.Name.Contains(pdfRadioButtonListField2.Name))
							{
								num2++;
							}
						}
					}
					int num4 = pdfRadioButtonListField.Items.Count - 1;
					while (num4 >= num2 && pdfPage.GetClientSize().Height >= pdfRadioButtonListField.Items[num4].Bounds.Height + pdfRadioButtonListField.Items[num4].Bounds.Y)
					{
						PdfRadioButtonListItem pdfRadioButtonListItem = pdfRadioButtonListField.Items[num4];
						PdfSolidBrush brush = new PdfSolidBrush(pdfRadioButtonListItem.BackRectColor);
						pdfPage.Graphics.DrawRectangle(brush, pdfRadioButtonListItem.Bounds);
						num4--;
					}
				}
			}
			for (int num5 = fields.Count - 1; num5 >= olderPdfForm; num5--)
			{
				if (fields[num5] is PdfTextBoxField)
				{
					PdfTextBoxField pdfTextBoxField = fields[num5] as PdfTextBoxField;
					PdfSolidBrush brush2 = new PdfSolidBrush(pdfTextBoxField.BackRectColor);
					pdfPage.Graphics.DrawRectangle(brush2, pdfTextBoxField.Bounds);
				}
				else if (fields[num5] is PdfCheckBoxField)
				{
					PdfCheckBoxField pdfCheckBoxField = fields[num5] as PdfCheckBoxField;
					PdfSolidBrush brush3 = new PdfSolidBrush(pdfCheckBoxField.BackRectColor);
					pdfPage.Graphics.DrawRectangle(brush3, pdfCheckBoxField.Bounds);
				}
				else if (fields[num5] is PdfButtonField)
				{
					PdfButtonField pdfButtonField = fields[num5] as PdfButtonField;
					PdfSolidBrush brush4 = new PdfSolidBrush(pdfButtonField.BackRectColor);
					pdfPage.Graphics.DrawRectangle(brush4, pdfButtonField.Bounds);
				}
				else if (fields[num5] is PdfListBoxField)
				{
					PdfListBoxField pdfListBoxField = fields[num5] as PdfListBoxField;
					PdfSolidBrush brush5 = new PdfSolidBrush(pdfListBoxField.BackRectColor);
					pdfPage.Graphics.DrawRectangle(brush5, pdfListBoxField.Bounds);
				}
				else if (fields[num5] is PdfComboBoxField)
				{
					PdfComboBoxField pdfComboBoxField = fields[num5] as PdfComboBoxField;
					PdfSolidBrush brush6 = new PdfSolidBrush(pdfComboBoxField.BackRectColor);
					pdfPage.Graphics.DrawRectangle(brush6, pdfComboBoxField.Bounds);
				}
			}
			olderPdfForm = fields.Items.Count;
			bool flag2 = false;
			if (pageResult.Page.Document.FileStructure.TaggedPdf && !pageResult.End && !flag && flag2)
			{
				return new PdfLayoutResult(pageResult.Page, pageResult.Bounds);
			}
			if (pageResult.End || flag)
			{
				break;
			}
			currentBounds = GetPaginateBounds(param);
			rectangleF = GetNextShapeBounds(rectangleF, pageResult);
			pdfPage = ((endPageLayoutEventArgs == null || endPageLayoutEventArgs.NextPage == null) ? GetNextPage(pdfPage) : endPageLayoutEventArgs.NextPage);
			pdfPage.Graphics.Tag = pageResult.Page.Graphics.Tag;
			if (m_isPdfGrid)
			{
				return GetLayoutResult(pageResult);
			}
		}
		return GetLayoutResult(pageResult);
	}

	protected RectangleF GetPaginateBounds(HtmlToPdfParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		if (!param.Format.UsePaginateBounds)
		{
			return new RectangleF(param.Bounds.X, 0f, param.Bounds.Width, param.Bounds.Height);
		}
		return param.Format.PaginateBounds;
	}

	protected override PdfLayoutResult LayoutInternal(HtmlToPdfParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		PdfPage pdfPage = param.Page;
		RectangleF currentBounds = param.Bounds;
		RectangleF shapeLayoutBounds = Element.GetBounds();
		shapeLayoutBounds.Location = PointF.Empty;
		TotalPageSize = param.Format.TotalPageSize;
		PdfLayoutResult pdfLayoutResult = null;
		ShapeLayoutResult pageResult = default(ShapeLayoutResult);
		pageResult.Page = pdfPage;
		while (true)
		{
			bool flag = RaiseBeforePageLayout(pdfPage, ref currentBounds);
			EndPageLayoutEventArgs endPageLayoutEventArgs = null;
			if (!flag)
			{
				if (currentBounds.Y != 0f && currentBounds.Height > currentBounds.Y && currentBounds.Height > shapeLayoutBounds.Height)
				{
					currentBounds.Height = shapeLayoutBounds.Height;
				}
				if (currentBounds.Y == currentBounds.Height && !param.SinglePageLayout)
				{
					pdfPage = GetNextPage(pdfPage);
					currentBounds.Y = 0f;
				}
				pageResult = LayoutOnPage(pdfPage, currentBounds, shapeLayoutBounds, param);
				TotalPageSize += pageResult.Bounds.Height;
				endPageLayoutEventArgs = RaiseEndPageLayout(pageResult);
				flag = endPageLayoutEventArgs?.Cancel ?? false;
				if (TotalPageSize > (double)param.Format.TotalPageLayoutSize)
				{
					flag = true;
				}
			}
			if (pageResult.End || flag)
			{
				break;
			}
			currentBounds = GetPaginateBounds(param);
			shapeLayoutBounds = GetNextShapeBounds(shapeLayoutBounds, pageResult);
			if (!param.SinglePageLayout)
			{
				pdfPage = ((endPageLayoutEventArgs == null || endPageLayoutEventArgs.NextPage == null) ? GetNextPage(pdfPage) : endPageLayoutEventArgs.NextPage);
			}
		}
		pdfLayoutResult = GetLayoutResult(pageResult);
		pdfLayoutResult.TotalPageSize = TotalPageSize;
		return pdfLayoutResult;
	}

	protected override PdfLayoutResult LayoutInternal(HtmlToPdfLayoutParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		PdfLayoutParams pdfLayoutParams = new PdfLayoutParams();
		pdfLayoutParams.Bounds = param.Bounds;
		pdfLayoutParams.Format = param.Format;
		pdfLayoutParams.Page = param.Page;
		if (param.VerticalOffsets.Length == 1 || (param.Format as PdfMetafileLayoutFormat).m_enableDirectLayout)
		{
			return LayoutInternal(pdfLayoutParams);
		}
		PdfPage pdfPage = param.Page;
		RectangleF currentBounds = param.Bounds;
		RectangleF shapeLayoutBounds = Element.GetBounds();
		shapeLayoutBounds.Location = PointF.Empty;
		PdfLayoutResult result = null;
		ShapeLayoutResult pageResult = default(ShapeLayoutResult);
		pageResult.Page = pdfPage;
		if (param.Page.Section.Count == 1)
		{
			last = false;
			index = 0;
			splitDiff = 0f;
		}
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		int num3 = param.VerticalOffsets.Length;
		float[] verticalOffsets = param.VerticalOffsets;
		foreach (float num4 in verticalOffsets)
		{
			if (index <= num3 - 1 && param.VerticalOffsets[index] != num4)
			{
				continue;
			}
			flag = false;
			while (!flag)
			{
				if ((double)num4 != 0.0)
				{
					num = Math.Min(pdfPage.Graphics.ClientSize.Height, num4);
					if (num2 + num > num4)
					{
						num = num4 - num2;
						(pdfLayoutParams.Format as PdfMetafileLayoutFormat).IsHTMLPageBreak = true;
					}
				}
				else
				{
					num = Math.Min(pdfPage.Graphics.ClientSize.Height, shapeLayoutBounds.Height);
				}
				currentBounds = new RectangleF(0f, param.Bounds.Y, 0f, num - param.Bounds.Y);
				bool flag2 = RaiseBeforePageLayout(pdfPage, ref currentBounds);
				EndPageLayoutEventArgs endPageLayoutEventArgs = null;
				bool flag3 = false;
				if (index == num3 - 1)
				{
					last = true;
				}
				if (!flag2)
				{
					pageResult = LayoutOnPage(pdfPage, currentBounds, shapeLayoutBounds, pdfLayoutParams);
					endPageLayoutEventArgs = RaiseEndPageLayout(pageResult);
					flag2 = endPageLayoutEventArgs?.Cancel ?? false;
					(pdfLayoutParams.Format as PdfMetafileLayoutFormat).IsHTMLPageBreak = false;
				}
				num2 += ((pageResult.Bounds.Height > 0f) ? pageResult.Bounds.Height : num);
				if ((int)num2 == (int)num4)
				{
					flag3 = true;
					index++;
				}
				if (!pageResult.End && !flag2 && !flag3)
				{
					currentBounds = GetPaginateBounds(pdfLayoutParams);
					shapeLayoutBounds = GetNextShapeBounds(shapeLayoutBounds, pageResult);
					pdfPage = ((endPageLayoutEventArgs == null || endPageLayoutEventArgs.NextPage == null) ? GetNextPage(pdfPage) : endPageLayoutEventArgs.NextPage);
					if ((int)num2 == (int)num4)
					{
						num2 = 0f;
						break;
					}
					continue;
				}
				if (!last)
				{
					currentBounds = GetPaginateBounds(pdfLayoutParams);
					pageResult.Page = ((endPageLayoutEventArgs == null || endPageLayoutEventArgs.NextPage == null) ? GetNextPage(pdfPage) : endPageLayoutEventArgs.NextPage);
					num2 = 0f;
					pageResult.Bounds = RectangleF.Empty;
				}
				result = GetLayoutResult(pageResult);
				pageResult.Bounds = RectangleF.Empty;
				flag = true;
				break;
			}
			break;
		}
		return result;
	}

	protected virtual RectangleF CheckCorrectCurrentBounds(PdfPage currentPage, RectangleF currentBounds, RectangleF shapeLayoutBounds, PdfLayoutParams param)
	{
		if (currentPage == null)
		{
			throw new ArgumentNullException("currentPage");
		}
		SizeF clientSize = currentPage.Graphics.ClientSize;
		currentBounds.Width = ((currentBounds.Width > 0f) ? currentBounds.Width : (clientSize.Width - currentBounds.X));
		currentBounds.Height = ((currentBounds.Height > 0f) ? currentBounds.Height : (clientSize.Height - currentBounds.Y));
		if (m_isPdfGrid)
		{
			currentBounds.Height -= m_bottomCellPadding;
		}
		return currentBounds;
	}

	private PdfLayoutResult GetLayoutResult(ShapeLayoutResult pageResult)
	{
		return new PdfLayoutResult(pageResult.Page, pageResult.Bounds);
	}

	protected virtual RectangleF CheckCorrectCurrentBounds(PdfPage currentPage, RectangleF currentBounds, RectangleF shapeLayoutBounds, HtmlToPdfParams param)
	{
		if (currentPage == null)
		{
			throw new ArgumentNullException("currentPage");
		}
		SizeF clientSize = currentPage.Graphics.ClientSize;
		currentBounds.Width = ((currentBounds.Width > 0f) ? currentBounds.Width : (clientSize.Width - currentBounds.X));
		currentBounds.Height = ((currentBounds.Height > 0f) ? currentBounds.Height : (clientSize.Height - currentBounds.Y));
		return currentBounds;
	}

	internal RectangleF GetPdfLayoutBounds(PdfPage currentPage, RectangleF currentBounds, RectangleF shapeLayoutBounds, HtmlToPdfParams param)
	{
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		TextRegions = param.Format.TextRegionManager;
		ImageRegions = param.Format.ImageRegionManager;
		FormRegions = param.Format.FormRegionManager;
		RectangleF result = CheckCorrectCurrentBounds(currentPage, currentBounds, shapeLayoutBounds, param);
		HtmlToPdfFormat format = param.Format;
		bool flag = format?.SplitTextLines ?? false;
		bool flag2 = format?.SplitImages ?? false;
		bool flag3 = false;
		if (!base.IsImagePath)
		{
			if (TextRegions != null && !flag && !flag3)
			{
				float value = shapeLayoutBounds.Y + result.Height;
				PdfUnitConvertor pdfUnitConvertor = new PdfUnitConvertor(96f);
				value = pdfUnitConvertor.ConvertToPixels(value, PdfGraphicsUnit.Point);
				value = TextRegions.GetTopCoordinate(value);
				if (!(result.Height <= currentPage.GetClientSize().Height))
				{
					value -= 2f;
					value = TextRegions.GetTopCoordinate(value);
				}
				value = pdfUnitConvertor.ConvertFromPixels(value, PdfGraphicsUnit.Point);
				float num = 0f;
				if (value > shapeLayoutBounds.Y)
				{
					num = value - shapeLayoutBounds.Y;
				}
				result.Height = ((currentPage != null && currentPage.GetClientSize().Height < num) ? currentPage.GetClientSize().Height : num);
				if (result.Y != 0f)
				{
					float num2 = result.Y + num;
					if (num2 > currentPage.GetClientSize().Height)
					{
						float height = currentPage.GetClientSize().Height;
						float num3 = num2 - height;
						result.Height = num - num3;
						value = shapeLayoutBounds.Y + result.Height;
						value = pdfUnitConvertor.ConvertToPixels(value, PdfGraphicsUnit.Point);
						value = TextRegions.GetTopCoordinate(value);
						value = pdfUnitConvertor.ConvertFromPixels(value, PdfGraphicsUnit.Point);
						if (value > shapeLayoutBounds.Y)
						{
							num = value - shapeLayoutBounds.Y;
						}
						result.Height = ((currentPage != null && currentPage.GetClientSize().Height < num) ? currentPage.GetClientSize().Height : num);
					}
				}
			}
			if (ImageRegions != null && !flag2 && !flag3)
			{
				float height2 = result.Height;
				float value2 = shapeLayoutBounds.Y + result.Height;
				PdfUnitConvertor pdfUnitConvertor2 = new PdfUnitConvertor(96f);
				value2 = pdfUnitConvertor2.ConvertToPixels(value2, PdfGraphicsUnit.Point);
				value2 = ImageRegions.GetTopCoordinate(value2);
				value2 = pdfUnitConvertor2.ConvertFromPixels(value2, PdfGraphicsUnit.Point);
				if (Math.Round(value2) != Math.Round(shapeLayoutBounds.Y + result.Height))
				{
					value2 = (float)Math.Floor(value2);
				}
				float num4 = 0f;
				if (value2 > shapeLayoutBounds.Y)
				{
					num4 = (result.Height = value2 - shapeLayoutBounds.Y);
				}
				if (num4 == 0f || TextRegions.Count == 0)
				{
					result.Height = height2;
				}
				else
				{
					PdfPage page = param.Page;
					if (shapeLayoutBounds.Height > page.Size.Height)
					{
						result.Height = num4;
					}
					if (TextRegions != null && !flag)
					{
						value2 = shapeLayoutBounds.Y + result.Height;
						value2 = pdfUnitConvertor2.ConvertToPixels(value2, PdfGraphicsUnit.Point);
						value2 = TextRegions.GetTopCoordinate(value2);
						value2 = pdfUnitConvertor2.ConvertFromPixels(value2, PdfGraphicsUnit.Point);
						if (value2 > shapeLayoutBounds.Y && value2 < result.Height)
						{
							num4 = value2 - shapeLayoutBounds.Y;
						}
						result.Height = ((currentPage != null && currentPage.GetClientSize().Height < num4) ? currentPage.GetClientSize().Height : num4);
						if (result.Height == 0f && currentPage.GetClientSize().Height > num4)
						{
							result.Height = height2;
						}
					}
					else
					{
						result.Height = num4;
					}
				}
			}
			if (FormRegions != null)
			{
				float value3 = shapeLayoutBounds.Y + result.Height;
				PdfUnitConvertor pdfUnitConvertor3 = new PdfUnitConvertor(96f);
				value3 = pdfUnitConvertor3.ConvertToPixels(value3, PdfGraphicsUnit.Point);
				value3 = FormRegions.GetTopCoordinate(value3);
				value3 = pdfUnitConvertor3.ConvertFromPixels(value3, PdfGraphicsUnit.Point);
				float num6 = 0f;
				if (value3 > shapeLayoutBounds.Y)
				{
					num6 = value3 - shapeLayoutBounds.Y;
				}
				result.Height = ((currentPage != null && currentPage.GetClientSize().Height < num6) ? currentPage.GetClientSize().Height : num6);
				if (result.Y != 0f)
				{
					float num7 = result.Y + num6;
					if (num7 > currentPage.GetClientSize().Height)
					{
						float height3 = currentPage.GetClientSize().Height;
						float num8 = num7 - height3;
						result.Height = num6 - num8;
						value3 = shapeLayoutBounds.Y + result.Height;
						value3 = pdfUnitConvertor3.ConvertToPixels(value3, PdfGraphicsUnit.Point);
						value3 = FormRegions.GetTopCoordinate(value3);
						value3 = pdfUnitConvertor3.ConvertFromPixels(value3, PdfGraphicsUnit.Point);
						if (value3 > shapeLayoutBounds.Y)
						{
							num6 = value3 - shapeLayoutBounds.Y;
						}
						result.Height = ((currentPage != null && currentPage.GetClientSize().Height < num6) ? currentPage.GetClientSize().Height : num6);
					}
				}
			}
		}
		List<HtmlHyperLink> list = new List<HtmlHyperLink>();
		float num9 = 0f;
		foreach (HtmlHyperLink item in format.HtmlHyperlinksCollection)
		{
			num9 = result.Height + result.Y;
			if (num9 > item.Bounds.Y)
			{
				if (string.IsNullOrEmpty(item.Hash))
				{
					PdfUriAnnotation pdfUriAnnotation = new PdfUriAnnotation(item.Bounds, item.Href);
					pdfUriAnnotation.Border.Width = 0f;
					currentPage.Annotations.Add(pdfUriAnnotation);
				}
				else
				{
					PdfDocumentLinkAnnotation pdfDocumentLinkAnnotation = new PdfDocumentLinkAnnotation(item.Bounds);
					pdfDocumentLinkAnnotation.Border.Width = 0f;
					pdfDocumentLinkAnnotation.ApplyText(item.Hash);
					currentPage.Annotations.Add(pdfDocumentLinkAnnotation);
				}
				list.Add(item);
			}
		}
		foreach (HtmlInternalLink item2 in format.HtmlInternalLinksCollection)
		{
			num9 = result.Height + result.Y;
			float y = ((!(num9 > item2.Bounds.Y)) ? (item2.Bounds.Y - result.Height) : item2.Bounds.Y);
			if (item2.DestinationPage is PdfLoadedPage)
			{
				PdfDocumentLinkAnnotation pdfDocumentLinkAnnotation2 = new PdfDocumentLinkAnnotation(new RectangleF(item2.Bounds.X, y, item2.Bounds.Width, item2.Bounds.Height));
				pdfDocumentLinkAnnotation2.Border.Width = 0f;
				pdfDocumentLinkAnnotation2.ApplyText(item2.Href);
				currentPage.Annotations.Add(pdfDocumentLinkAnnotation2);
			}
			else
			{
				PdfDestination pdfDestination = new PdfDestination(item2.DestinationPage);
				pdfDestination.Location = item2.Destination;
				PdfDocumentLinkAnnotation pdfDocumentLinkAnnotation3 = new PdfDocumentLinkAnnotation(new RectangleF(item2.Bounds.X, y, item2.Bounds.Width, item2.Bounds.Height), pdfDestination);
				pdfDocumentLinkAnnotation3.Border.Width = 0f;
				pdfDestination.isModified = false;
				currentPage.Annotations.Add(pdfDocumentLinkAnnotation3);
			}
		}
		RepositionLinks(list, num9, format);
		return result;
	}

	internal void RepositionLinks(List<HtmlHyperLink> list, float height, HtmlToPdfFormat format)
	{
		foreach (HtmlHyperLink item in list)
		{
			format.HtmlHyperlinksCollection.Remove(item);
		}
		list.Clear();
		list = format.HtmlHyperlinksCollection;
		format.HtmlHyperlinksCollection.Clear();
		foreach (HtmlHyperLink item2 in list)
		{
			float y = item2.Bounds.Y - height;
			item2.Bounds = new RectangleF(item2.Bounds.X, y, item2.Bounds.Width, item2.Bounds.Height);
			format.HtmlHyperlinksCollection.Add(item2);
		}
	}

	private ShapeLayoutResult LayoutOnPage(PdfPage currentPage, RectangleF currentBounds, RectangleF shapeLayoutBounds, HtmlToPdfParams param)
	{
		if (currentPage == null)
		{
			throw new ArgumentNullException("currentPage");
		}
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		ShapeLayoutResult result = default(ShapeLayoutResult);
		currentBounds = GetPdfLayoutBounds(currentPage, currentBounds, shapeLayoutBounds, param);
		bool flag = FitsToBounds(currentBounds, shapeLayoutBounds);
		bool num = param.Format.Break != PdfLayoutBreakType.FitElement || flag || currentPage != param.Page;
		bool flag2 = false;
		if (num)
		{
			RectangleF drawBounds = GetDrawBounds(currentBounds, shapeLayoutBounds);
			if (Math.Round(shapeLayoutBounds.Height) + 2.0 == Math.Round(currentPage.GetClientSize().Height))
			{
				shapeLayoutBounds.Height = currentPage.GetClientSize().Height;
			}
			DrawShape(currentPage.Graphics, currentBounds, drawBounds);
			result.Bounds = GetPageResultBounds(currentBounds, shapeLayoutBounds);
			flag2 = (int)currentBounds.Height >= (int)shapeLayoutBounds.Height;
		}
		result.End = flag2 || param.Format.Layout == PdfLayoutType.OnePage;
		result.Page = currentPage;
		return result;
	}

	private ShapeLayoutResult LayoutOnPage(PdfPage currentPage, RectangleF currentBounds, RectangleF shapeLayoutBounds, PdfLayoutParams param)
	{
		if (currentPage == null)
		{
			throw new ArgumentNullException("currentPage");
		}
		if (param == null)
		{
			throw new ArgumentNullException("param");
		}
		ShapeLayoutResult result = default(ShapeLayoutResult);
		currentBounds = CheckCorrectCurrentBounds(currentPage, currentBounds, shapeLayoutBounds, param);
		bool flag = FitsToBounds(currentBounds, shapeLayoutBounds);
		bool num = param.Format.Break != PdfLayoutBreakType.FitElement || flag || currentPage != param.Page;
		bool flag2 = false;
		if (num)
		{
			RectangleF drawBounds = GetDrawBounds(currentBounds, shapeLayoutBounds);
			DrawShape(currentPage.Graphics, currentBounds, drawBounds);
			result.Bounds = GetPageResultBounds(currentBounds, shapeLayoutBounds);
			flag2 = (int)currentBounds.Height >= (int)shapeLayoutBounds.Height;
		}
		result.End = flag2 || param.Format.Layout == PdfLayoutType.OnePage;
		result.Page = currentPage;
		return result;
	}

	private RectangleF GetNextShapeBounds(RectangleF shapeLayoutBounds, ShapeLayoutResult pageResult)
	{
		RectangleF bounds = pageResult.Bounds;
		shapeLayoutBounds.Y += bounds.Height;
		shapeLayoutBounds.Height -= bounds.Height;
		return shapeLayoutBounds;
	}

	private bool FitsToBounds(RectangleF currentBounds, RectangleF shapeLayoutBounds)
	{
		return shapeLayoutBounds.Height <= currentBounds.Height;
	}

	private RectangleF GetDrawBounds(RectangleF currentBounds, RectangleF shapeLayoutBounds)
	{
		RectangleF result = currentBounds;
		result.Y -= shapeLayoutBounds.Y;
		result.Height += shapeLayoutBounds.Y;
		return result;
	}

	private RectangleF GetPageResultBounds(RectangleF currentBounds, RectangleF shapeLayoutBounds)
	{
		RectangleF result = currentBounds;
		result.Height = Math.Min(result.Height, shapeLayoutBounds.Height);
		return result;
	}

	private void DrawShape(PdfGraphics g, RectangleF currentBounds, RectangleF drawRectangle)
	{
		if (g == null)
		{
			throw new ArgumentNullException("g");
		}
		PdfGraphicsState state = g.Save();
		try
		{
			g.SetClip(currentBounds);
			Element.Draw(g, drawRectangle.Location);
		}
		finally
		{
			g.Restore(state);
		}
	}

	private EndPageLayoutEventArgs RaiseEndPageLayout(ShapeLayoutResult pageResult)
	{
		EndPageLayoutEventArgs endPageLayoutEventArgs = null;
		if (Element.RaiseEndPageLayout)
		{
			endPageLayoutEventArgs = new EndPageLayoutEventArgs(GetLayoutResult(pageResult));
			Element.OnEndPageLayout(endPageLayoutEventArgs);
		}
		return endPageLayoutEventArgs;
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

	protected virtual float ToCorrectBounds(RectangleF currentBounds, RectangleF shapeLayoutBounds, PdfPage currentPage)
	{
		return currentBounds.Height;
	}
}
