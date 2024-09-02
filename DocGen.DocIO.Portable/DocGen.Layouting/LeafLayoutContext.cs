using System;
using System.Collections.Generic;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LeafLayoutContext : LayoutContext
{
	private bool m_isXPositionUpdated;

	private bool m_isYPositionUpdated;

	private bool m_isWrapText;

	private bool isHyphenated;

	public ILeafWidget LeafWidget => m_widget as ILeafWidget;

	public LeafLayoutContext(ILeafWidget strWidget, ILCOperator lcOperator, bool isForceFitLayout)
		: base(strWidget, lcOperator, isForceFitLayout)
	{
	}

	internal bool IsDecimaltabIsInCell(WParagraph paragraph, ILeafWidget leafWidget)
	{
		if (paragraph != null && paragraph.IsInCell && paragraph.ParagraphFormat.Tabs.Count == 1 && paragraph.ParagraphFormat.Tabs[0].Justification == DocGen.DocIO.DLS.TabJustification.Decimal && leafWidget is WTextRange && (leafWidget as WTextRange).Text.Contains(".") && IsDecimalTabPoint(paragraph, leafWidget))
		{
			return true;
		}
		return false;
	}

	internal bool IsDecimalTabPoint(WParagraph paragraph, ILeafWidget leafWidget)
	{
		int num = -1;
		for (int i = 0; i < paragraph.Items.Count; i++)
		{
			if (paragraph.Items[i] is WTextRange && (paragraph.Items[i] as WTextRange).Text.Contains(".") && num == -1)
			{
				num = (paragraph.Items[i] as WTextRange).Index;
				if (num == (leafWidget as WTextRange).Index)
				{
					return true;
				}
			}
			else if (paragraph.Items[i] is Break || (paragraph.Items[i] is WTextRange && (paragraph.Items[i] as WTextRange).m_layoutInfo is TabsLayoutInfo))
			{
				num = -1;
			}
		}
		return false;
	}

	internal RectangleF NeedToUpdateClientArea(RectangleF rect, WParagraph paragraph, ILeafWidget leafWidget, Layouter layouter, ParagraphLayoutInfo paragraphLayoutInfo)
	{
		if (IsDecimaltabIsInCell(paragraph, leafWidget))
		{
			string text = paragraph.Text;
			int num = text.IndexOf('.');
			string text2 = text.Substring(0, num);
			string text3 = text.Substring(num + 1);
			text3 = text3.Split(' ')[0];
			float num2 = rect.Width - paragraph.ParagraphFormat.Tabs[0].Position;
			float width = base.DrawingContext.MeasureString(text2, (LeafWidget as WTextRange).CharacterFormat.GetFontToRender((LeafWidget as WTextRange).ScriptType), null, (LeafWidget as WTextRange).ScriptType).Width;
			float width2 = base.DrawingContext.MeasureString("." + text3, (LeafWidget as WTextRange).CharacterFormat.GetFontToRender((LeafWidget as WTextRange).ScriptType), null, (LeafWidget as WTextRange).ScriptType).Width;
			float pageMarginLeft = GetPageMarginLeft();
			float position = paragraph.ParagraphFormat.Tabs[0].Position;
			position -= ((paragraphLayoutInfo != null && paragraphLayoutInfo.XPosition != pageMarginLeft) ? (paragraphLayoutInfo.XPosition - pageMarginLeft) : 0f);
			WTableCell wTableCell = paragraph.Owner as WTableCell;
			float num3 = 0f;
			if (wTableCell != null)
			{
				num3 = wTableCell.GetRightPadding();
			}
			if (width <= position - num3 && (!layouter.IsFirstItemInLine || !(rect.Right - (pageMarginLeft + paragraph.ParagraphFormat.Tabs[0].Position) - num3 < width2)))
			{
				rect = new RectangleF(rect.X, rect.Y, num2 + width, rect.Height);
			}
		}
		else if ((m_lcOperator as Layouter).m_effectiveJustifyWidth > 0f)
		{
			rect = new RectangleF(rect.X, rect.Y, rect.Width + (m_lcOperator as Layouter).m_effectiveJustifyWidth, rect.Height);
		}
		return rect;
	}

	internal void UpdateShiftDistance(ref RectangleF bounds, ParagraphItem paraItem)
	{
		if (!(GetBaseEntity(paraItem.Owner) is WSection))
		{
			return;
		}
		float num = 0f;
		float num2 = bounds.X;
		float num3 = bounds.Y;
		ShapeHorizontalAlignment shapeHorizontalAlignment = paraItem.GetShapeHorizontalAlignment();
		ShapeVerticalAlignment shapeVerticalAlignment = paraItem.GetShapeVerticalAlignment();
		float num4 = GetAngle(paraItem);
		if (num4 > 360f)
		{
			num4 %= 360f;
		}
		else if (num4 < 0f)
		{
			num4 = 360f + num4;
		}
		if ((num4 >= 45f && num4 < 135f) || (num4 >= 225f && num4 < 315f))
		{
			num = (bounds.Height - bounds.Width) / 2f;
			switch (shapeHorizontalAlignment)
			{
			case ShapeHorizontalAlignment.Left:
				num2 += num;
				break;
			case ShapeHorizontalAlignment.Right:
				num2 -= num;
				break;
			}
			switch (shapeVerticalAlignment)
			{
			case ShapeVerticalAlignment.Top:
				num3 -= num;
				break;
			case ShapeVerticalAlignment.Bottom:
				num3 += num;
				break;
			}
			bounds = new RectangleF(num2, num3, bounds.Width, bounds.Height);
		}
	}

	public override LayoutedWidget Layout(RectangleF rect)
	{
		isHyphenated = false;
		WParagraph ownerParagraph = GetOwnerParagraph();
		Layouter layouter = m_lcOperator as Layouter;
		ILeafWidget leafWidget = LeafWidget;
		ParagraphLayoutInfo paragraphLayoutInfo = null;
		float width = rect.Width;
		rect = NeedToUpdateClientArea(rect, ownerParagraph, leafWidget, layouter, paragraphLayoutInfo);
		if (ownerParagraph != null)
		{
			paragraphLayoutInfo = ((IWidget)ownerParagraph).LayoutInfo as ParagraphLayoutInfo;
		}
		CreateLayoutArea(rect);
		float width2 = rect.Width;
		SizeF size = leafWidget.Measure(base.DrawingContext);
		if (leafWidget is WField && (leafWidget as WField).FieldType == FieldType.FieldExpression && size.Width > m_layoutArea.ClientArea.Width)
		{
			size = UpdateEQFieldWidth(base.DrawingContext, (leafWidget as WField).GetCharFormat());
		}
		bool flag = layouter.m_canSplitbyCharacter;
		if (!layouter.IsLayoutingHeaderFooter && ownerParagraph != null && paragraphLayoutInfo.IsFirstItemInPage && layouter.m_firstItemInPageYPosition == 0f)
		{
			layouter.m_firstItemInPageYPosition = rect.Y;
		}
		float indentX = 0f;
		float indentY = 0f;
		if (IsFloatingItemLayouted(ownerParagraph))
		{
			return m_ltWidget;
		}
		if (m_ltWidget == null && LeafWidget is WTextBox && IsWord2013((LeafWidget as WTextBox).Document) && (LeafWidget as Entity).IsFloatingItem(isTextWrapAround: true) && (LeafWidget as WTextBox).TextBoxFormat.AutoFit && ((LeafWidget as WTextBox).TextBoxFormat.VerticalOrigin == VerticalOrigin.Paragraph || (LeafWidget as WTextBox).TextBoxFormat.VerticalOrigin == VerticalOrigin.Line))
		{
			m_ltWidget = new LayoutedWidget(LeafWidget);
			m_ltWidget.Bounds = new RectangleF(rect.X, rect.Y, size.Width, size.Height);
			LayoutTextBoxBody();
			size.Height = m_ltWidget.Bounds.Size.Height;
			m_ltWidget = null;
		}
		if (leafWidget is WPicture && (leafWidget as WPicture).TextWrappingStyle == TextWrappingStyle.Inline)
		{
			if ((leafWidget as WPicture).HasBorder)
			{
				UpdatePictureBorderSize(leafWidget as WPicture, ref size);
			}
			if ((leafWidget as WPicture).Rotation != 0f && !leafWidget.LayoutInfo.IsVerticalText)
			{
				GetPictureWrappingBounds(ref indentX, ref indentY, ref size, (leafWidget as WPicture).Rotation);
			}
		}
		if (leafWidget is WTextRange && (leafWidget as WTextRange).Owner == null)
		{
			size.Width = 0f;
		}
		TabsLayoutInfo tabsLayoutInfo = leafWidget.LayoutInfo as TabsLayoutInfo;
		WTextRange currTextRange = GetCurrTextRange();
		if (tabsLayoutInfo != null)
		{
			UpdateTabWidth(ref rect, ref size, currTextRange);
			if ((DocumentLayouter.IsUpdatingTOC || layouter.UpdatingPageFields) && currTextRange != null)
			{
				currTextRange.Text = ControlChar.Tab;
			}
			float pageMarginLeft = GetPageMarginLeft();
			if (!tabsLayoutInfo.IsTabWidthUpdatedBasedOnIndent && tabsLayoutInfo.m_currTab.Position > 0f && tabsLayoutInfo.m_currTab.Position + ((paragraphLayoutInfo != null && paragraphLayoutInfo.XPosition != pageMarginLeft) ? (paragraphLayoutInfo.XPosition - pageMarginLeft) : pageMarginLeft) > GetPageMarginRight() - paragraphLayoutInfo.Margins.Right && !base.IsTabStopBeyondRightMarginExists && (tabsLayoutInfo.m_currTab.Justification != TabJustification.Decimal || !IsLeafWidgetIsInCell(currTextRange)) && (tabsLayoutInfo.m_currTab.Justification != TabJustification.Decimal || !IsLeafWidgetIsInTextBox(currTextRange)) && ownerParagraph != null && ownerParagraph.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
			{
				UpdateAreaMaxWidth();
				rect.Width = m_layoutArea.ClientArea.Width;
			}
			if (layouter.m_lineSpaceWidths != null)
			{
				layouter.m_lineSpaceWidths.Clear();
			}
		}
		RectangleF rectangleF = rect;
		bool flag2 = false;
		if (LeafWidget is Shape && (LeafWidget as Shape).IsHorizontalRule && size.Width > m_layoutArea.ClientActiveArea.Width)
		{
			size.Width = m_layoutArea.ClientActiveArea.Width;
		}
		if ((LeafWidget is WPicture || LeafWidget is Shape || LeafWidget is WTextBox || leafWidget is GroupShape || leafWidget is WChart) && (leafWidget as Entity).IsFloatingItem(isTextWrapAround: false))
		{
			float shiftDistance = 0f;
			flag2 = true;
			GetFloattingItemPosition(ref indentX, ref indentY, ref size, shiftDistance);
			if (leafWidget is WPicture && ((leafWidget as WPicture).TextWrappingStyle == TextWrappingStyle.Behind || (leafWidget as WPicture).TextWrappingStyle == TextWrappingStyle.InFrontOfText) && base.DrawingContext.GetLineWidth(leafWidget as WPicture) > 0f)
			{
				float lineWidth = base.DrawingContext.GetLineWidth(leafWidget as WPicture);
				indentX -= lineWidth;
				indentY -= lineWidth;
				size.Width += 2f * lineWidth;
				size.Height += 2f * lineWidth;
			}
			rect = new RectangleF(indentX, indentY, rect.Width - (indentX - rect.X), rect.Height - (indentY - rect.Y));
			CreateLayoutArea(rect);
		}
		if (leafWidget is WField && (leafWidget as WField).FieldType == FieldType.FieldPage)
		{
			size = GetPageFieldSize(leafWidget as WField);
		}
		if ((!(leafWidget is WPicture) && !(leafWidget is WTextBox) && !(leafWidget is Shape) && !(leafWidget is GroupShape) && !(leafWidget is WChart) && !(leafWidget is WOleObject)) || !(Math.Round(size.Height, 2) >= Math.Round(m_layoutArea.ClientActiveArea.Height, 2)) || !base.IsForceFitLayout || (LeafWidget as ParagraphItem).GetTextWrappingStyle() != 0)
		{
			AdjustClientAreaBasedOnTextWrap(leafWidget, ref size, ref rect);
		}
		flag2 = flag2 || indentX != 0f || indentY != 0f;
		if (IsNeedToResetClientArea(ownerParagraph, leafWidget, flag2))
		{
			indentX = rect.X;
			indentY = rect.Y;
			rect = rectangleF;
			CreateLayoutArea(rect);
		}
		if (currTextRange != null && (!(leafWidget is WField) || (leafWidget as WField).FieldType != FieldType.FieldExpression))
		{
			flag = (!layouter.m_canSplitByTab || !(currTextRange.m_layoutInfo is TabsLayoutInfo)) && layouter.m_canSplitbyCharacter;
			m_ltWidget = WordLayout(rect, size, currTextRange, ownerParagraph);
			if (m_ltWidget != null)
			{
				DoWord2013JustificationWordFit(ownerParagraph, width, layouter);
				if (m_ltState == LayoutState.Splitted && m_isWrapText)
				{
					m_ltState = LayoutState.WrapText;
				}
				return m_ltWidget;
			}
			if (currTextRange.Text == '\u0003'.ToString())
			{
				if (rect.Width < 144f)
				{
					size.Width = rect.Width;
				}
				else
				{
					size.Width = 144f;
				}
			}
			else if (currTextRange.Text == '\u0004'.ToString())
			{
				size.Width = rect.Width;
			}
		}
		float num = rect.X + size.Width;
		bool flag3 = true;
		if (!IsLeafWidgetNeedToBeSplitted(size, width2, rect, indentY) || (tabsLayoutInfo != null && ((Math.Round(num, 2) <= Math.Round((double)tabsLayoutInfo.m_currTab.Position + tabsLayoutInfo.PageMarginLeft, 2) && IsWord2013(ownerParagraph.Document) && IsTabNeedToFit(tabsLayoutInfo, tabsLayoutInfo.m_currTab.Position, ownerParagraph, rect) && size.Height <= m_layoutArea.ClientArea.Height) || (currTextRange != null && currTextRange.Document.ActualFormatType == FormatType.Doc && tabsLayoutInfo.m_list.Count > 0 && tabsLayoutInfo.m_list[tabsLayoutInfo.m_list.Count - 1].Position > ClientAreaRight(ownerParagraph, rect.Right) - GetPageMarginLeft()))))
		{
			FitWidget(size, leafWidget, isLastWordFit: false, indentX, indentY, flag2);
			if (!(base.LayoutInfo is ParagraphLayoutInfo { IsPageBreak: not false }))
			{
				m_ltState = LayoutState.Fitted;
			}
			else
			{
				m_ltState = LayoutState.Breaked;
			}
			if (base.LayoutInfo.IsPageBreakItem)
			{
				m_ltState = LayoutState.Fitted;
			}
		}
		else
		{
			base.IsTabStopBeyondRightMarginExists = false;
			ISplitLeafWidget splitLeafWidget = LeafWidget as ISplitLeafWidget;
			bool flag4 = currTextRange != null && LeafWidget.LayoutInfo.IsClipped;
			if (splitLeafWidget != null && (IsNeedToFitBasedOnLineSpacing(ownerParagraph, size) || (currTextRange != null && ownerParagraph != null && ownerParagraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly && Math.Abs(ownerParagraph.ParagraphFormat.LineSpacing) <= m_layoutArea.ClientActiveArea.Height) || (flag4 && (m_layoutArea.ClientArea.Height >= 0f || (IsInFrame(ownerParagraph) && m_layoutArea.ClientArea.Height >= 0f))) || base.IsForceFitLayout))
			{
				layouter.m_canSplitbyCharacter = (!flag || !IsNeedToSkipSplitTextByCharacter(currTextRange, ownerParagraph)) && flag;
				SplitUpWidget(splitLeafWidget, width2);
			}
			else
			{
				m_ltState = LayoutState.NotFitted;
				bool flag6 = (base.IsVerticalNotFitted = size.Height > m_layoutArea.ClientArea.Height);
				if (flag6 && layouter.IsRowFitInSamePage && (!(m_layoutArea.ClientArea.Height <= 0f) || !IsWord2013(ownerParagraph.Document)))
				{
					FitWidget(size, leafWidget, isLastWordFit: false, 0f, 0f, isFloatingItem: false);
					base.IsVerticalNotFitted = false;
					m_ltState = LayoutState.Fitted;
				}
				else
				{
					flag3 = false;
				}
			}
		}
		if (flag3)
		{
			DoWord2013JustificationWordFit(ownerParagraph, width, layouter);
		}
		if (currTextRange != null && m_ltState == LayoutState.Splitted && m_isWrapText)
		{
			float width3 = size.Width;
			if (base.SplittedWidget is SplitStringWidget splitStringWidget)
			{
				width3 = base.DrawingContext.MeasureTextRange(currTextRange, splitStringWidget.SplittedText.Split(' ')[0]).Width;
				if (base.DrawingContext.IsUnicodeText(splitStringWidget.SplittedText))
				{
					width3 = base.DrawingContext.MeasureTextRange(currTextRange, splitStringWidget.SplittedText[0].ToString()).Width;
				}
			}
			else if (base.SplittedWidget is WTextRange && base.SplittedWidget.LayoutInfo is TabsLayoutInfo)
			{
				width3 = size.Width;
			}
			float right = layouter.ClientLayoutArea.Right;
			if (ownerParagraph != null && ownerParagraph.IsInCell)
			{
				right = ((ownerParagraph.GetOwnerEntity() as WTableCell).m_layoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Right;
			}
			if (m_ltWidget.Bounds.Right + width3 + paragraphLayoutInfo.Margins.Right < right)
			{
				m_ltState = LayoutState.WrapText;
			}
		}
		DoLayoutAfter();
		if (m_ltWidget != null && m_ltWidget.Widget is ParagraphItem && (m_ltWidget.Widget as Entity).IsFloatingItem(isTextWrapAround: false) && GetAngle(m_ltWidget.Widget as ParagraphItem) != 0f)
		{
			ParagraphItem paraItem = m_ltWidget.Widget as ParagraphItem;
			m_ltWidget.Bounds = ShiftXYBasedOnRotation(paraItem, m_ltWidget.Bounds);
		}
		if (m_ltWidget != null && (m_ltWidget.Widget is Shape || m_ltWidget.Widget is WTextBox || m_ltWidget.Widget is GroupShape))
		{
			if (m_ltWidget.Widget is WTextBox)
			{
				LayoutTextBoxBody();
			}
			else if (m_ltWidget.Widget is Shape)
			{
				LayoutShapeTextBody();
			}
			else
			{
				LayoutGroupShapeTextBody();
			}
		}
		HandleFloatingItemHaveCharacterOrigin(ownerParagraph);
		return m_ltWidget;
	}

	private bool IsTabNeedToFit(TabsLayoutInfo tabsInfo, float currentTabPosition, WParagraph paragraph, RectangleF rect)
	{
		bool result = true;
		if (tabsInfo.m_list.Count > 0)
		{
			int num = 0;
			for (int i = 0; i < tabsInfo.m_list.Count; i++)
			{
				if (tabsInfo.m_list[i].Position == currentTabPosition)
				{
					num = i;
				}
			}
			if (tabsInfo.m_list.Count > 1)
			{
				for (int num2 = num - 1; num2 >= 0; num2--)
				{
					if (tabsInfo.m_list[num2].Position > 0f && currentTabPosition > ClientAreaRight(paragraph, rect.Right) - GetPageMarginLeft())
					{
						result = false;
					}
				}
			}
		}
		return result;
	}

	private RectangleF ShiftXYBasedOnRotation(ParagraphItem paraItem, RectangleF bounds)
	{
		WParagraph ownerParagraphValue = paraItem.GetOwnerParagraphValue();
		if (ownerParagraphValue != null && !ownerParagraphValue.IsInCell)
		{
			if (paraItem is WPicture)
			{
				WPicture wPicture = paraItem as WPicture;
				RectangleF boundingBoxCoordinates = base.DrawingContext.GetBoundingBoxCoordinates(new RectangleF(0f, 0f, wPicture.Width, wPicture.Height), wPicture.Rotation);
				bounds = new RectangleF(bounds.X - boundingBoxCoordinates.X, bounds.Y - boundingBoxCoordinates.Y, wPicture.Width, wPicture.Height);
			}
			UpdateShiftDistance(ref bounds, paraItem);
			if (paraItem is WPicture)
			{
				bounds = base.DrawingContext.GetBoundingBoxCoordinates(bounds, (paraItem as WPicture).Rotation);
			}
			return bounds;
		}
		return bounds;
	}

	private void HandleFloatingItemHaveCharacterOrigin(WParagraph paragraph)
	{
		ParagraphLayoutInfo paragraphLayoutInfo = null;
		if (paragraph != null)
		{
			paragraphLayoutInfo = ((IWidget)paragraph).LayoutInfo as ParagraphLayoutInfo;
		}
		if (m_ltWidget == null || !IsDrawingElement(LeafWidget) || !(LeafWidget as ParagraphItem).IsFloatingItem(isTextWrapAround: true) || ((LeafWidget as ParagraphItem).GetHorizontalOrigin() != HorizontalOrigin.Character && (LeafWidget as ParagraphItem).GetVerticalOrigin() != VerticalOrigin.Line))
		{
			return;
		}
		if (paragraphLayoutInfo != null && (paragraphLayoutInfo.Justification == HAlignment.Left || !((float)Math.Round(paragraphLayoutInfo.YPosition, 2) > m_ltWidget.Bounds.Y)))
		{
			paragraph.IsFloatingItemsLayouted = true;
			AddToFloatingItems(m_ltWidget, LeafWidget);
			int num = (m_lcOperator as Layouter).FloatingItems.Count - 1;
			if (num >= 0)
			{
				IsDynamicRelayoutingOccur(paragraph, num);
			}
		}
		else
		{
			(m_lcOperator as Layouter).IsNeedToRelayout = true;
		}
	}

	internal RectangleF FindWrappedPosition(SizeF widgetSize, RectangleF clientArea)
	{
		try
		{
			CreateLayoutArea(clientArea);
			AdjustClientAreaBasedOnTextWrap(LeafWidget, ref widgetSize, ref clientArea);
		}
		catch (Exception)
		{
		}
		return clientArea;
	}

	private bool IsLineSpacingFitsWidget(WParagraph paragraph, float height)
	{
		if (paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly)
		{
			return Math.Abs(paragraph.ParagraphFormat.LineSpacing) <= m_layoutArea.ClientActiveArea.Height;
		}
		if (paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Multiple)
		{
			return paragraph.ParagraphFormat.LineSpacing / 12f * height <= m_layoutArea.ClientActiveArea.Height;
		}
		return false;
	}

	private void UpdatePictureBorderSize(WPicture picture, ref SizeF size)
	{
		if (picture.IsShape)
		{
			float lineWidth = base.DrawingContext.GetLineWidth(picture.PictureShape.PictureDescriptor.BorderLeft);
			float lineWidth2 = base.DrawingContext.GetLineWidth(picture.PictureShape.PictureDescriptor.BorderRight);
			float lineWidth3 = base.DrawingContext.GetLineWidth(picture.PictureShape.PictureDescriptor.BorderTop);
			float lineWidth4 = base.DrawingContext.GetLineWidth(picture.PictureShape.PictureDescriptor.BorderBottom);
			size.Width += lineWidth + lineWidth2;
			size.Height += lineWidth3 + lineWidth4;
		}
		else
		{
			float lineWidth5 = base.DrawingContext.GetLineWidth(picture);
			size.Width += 2f * lineWidth5;
			size.Height += 2f * lineWidth5;
		}
	}

	private bool IsFloatingItemLayouted(WParagraph paragraph)
	{
		if ((LeafWidget is WPicture || LeafWidget is Shape || LeafWidget is WChart || LeafWidget is GroupShape || LeafWidget is WTextBox) && (LeafWidget as ParagraphItem).IsWrappingBoundsAdded())
		{
			int i = 0;
			if (IsFloatingItemExistInCollection(ref i))
			{
				CreateLayoutedWidget(i);
				IsDynamicRelayoutingOccur(paragraph, i);
				return true;
			}
		}
		return false;
	}

	private bool IsNeedToResetClientArea(WParagraph paragraph, ILeafWidget leafWidget, bool isFloating)
	{
		bool flag = (LeafWidget is WPicture || LeafWidget is Shape || LeafWidget is WTextBox || leafWidget is GroupShape || leafWidget is WChart) && (leafWidget as Entity).IsFloatingItem(isTextWrapAround: true);
		ParagraphLayoutInfo paragraphLayoutInfo = ((paragraph != null) ? (((IWidget)paragraph).LayoutInfo as ParagraphLayoutInfo) : null);
		if (!(paragraph != null && paragraphLayoutInfo != null && paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && paragraph.IsInCell && flag))
		{
			return isFloating;
		}
		return paragraphLayoutInfo.IsFirstItemInPage;
	}

	private void IsDynamicRelayoutingOccur(WParagraph paragraph, int i)
	{
		WSection wSection = null;
		if (paragraph != null)
		{
			wSection = GetBaseEntity(paragraph) as WSection;
		}
		ParagraphLayoutInfo paragraphLayoutInfo = null;
		if (paragraph != null)
		{
			paragraphLayoutInfo = ((IWidget)paragraph).LayoutInfo as ParagraphLayoutInfo;
		}
		Layouter layouter = m_lcOperator as Layouter;
		VerticalOrigin verticalOrigin = VerticalOrigin.Margin;
		if (layouter.FloatingItems.Count > i && layouter.FloatingItems[i].FloatingEntity is ParagraphItem)
		{
			verticalOrigin = (layouter.FloatingItems[i].FloatingEntity as ParagraphItem).GetVerticalOrigin();
		}
		bool flag = false;
		if (IsNeedToForceDynamicRelayout(paragraph, paragraphLayoutInfo, layouter.FloatingItems[i].TextWrappingStyle, layouter.FloatingItems[i].FloatingEntity))
		{
			flag = true;
		}
		if (paragraph != null && (!layouter.IsLayoutingHeaderFooter || IsFloatingItemLayoutInCell(paragraph, layouter.FloatingItems[i].TextWrappingStyle, layouter.FloatingItems[i].FloatingEntity)) && (((Math.Round(paragraphLayoutInfo.YPosition, 2) > Math.Round(m_ltWidget.Bounds.Y, 2) || (verticalOrigin == VerticalOrigin.Line && m_layoutArea.ClientActiveArea.Y > m_ltWidget.Bounds.Y) || (paragraphLayoutInfo.XPosition > m_ltWidget.Bounds.X && wSection != null && wSection.Columns.Count > 1 && !wSection.PageSetup.Bidi && layouter.CurrentColumnIndex != 0) || (wSection != null && wSection.Columns.Count > 1 && wSection.PageSetup.Bidi && layouter.CurrentColumnIndex != 0 && paragraphLayoutInfo.XPosition + wSection.Columns[layouter.CurrentColumnIndex].Width + wSection.Columns[layouter.CurrentColumnIndex - 1].Space < m_ltWidget.Bounds.Right)) && !paragraphLayoutInfo.IsFirstItemInPage) || flag) && layouter.DynamicParagraph == null && !layouter.FloatingItems[i].IsFloatingItemFit && layouter.m_firstItemInPageYPosition < m_ltWidget.Bounds.Bottom)
		{
			if (layouter.MaintainltWidget.ChildWidgets.Count > 0)
			{
				layouter.MaintainltWidget.ChildWidgets.RemoveRange(0, layouter.MaintainltWidget.ChildWidgets.Count);
			}
			m_ltState = LayoutState.DynamicRelayout;
			if (m_ltWidget.Bounds.Height > layouter.CurrentSection.PageSetup.PageSize.Height || IsOwnerParaNotFittedInSamePage(layouter.FloatingItems[i].TextWrappingStyle, layouter.FloatingItems[i].FloatingEntity, layouter, m_ltWidget, paragraphLayoutInfo))
			{
				m_ltState = LayoutState.NotFitted;
				m_ltWidget = null;
			}
			FloatingItem floatingItem = layouter.FloatingItems[i];
			if (floatingItem.FloatingEntity is ParagraphItem && m_ltState == LayoutState.DynamicRelayout)
			{
				ParagraphItem paragraphItem = floatingItem.FloatingEntity as ParagraphItem;
				if ((floatingItem.FloatingEntity as ParagraphItem).GetVerticalOrigin() == VerticalOrigin.Paragraph && paragraphItem.IsWrappingBoundsAdded() && paragraphItem.GetVerticalPosition() < 0f)
				{
					paragraphLayoutInfo.PargaraphOriginalYPosition = paragraphLayoutInfo.YPosition;
				}
			}
			if (flag)
			{
				(m_lcOperator as Layouter).IsNeedToRelayoutTable = true;
			}
		}
		else
		{
			m_ltState = LayoutState.Fitted;
		}
	}

	private bool IsFloatingItemExistInCollection(ref int i)
	{
		Layouter layouter = m_lcOperator as Layouter;
		for (i = 0; i < layouter.FloatingItems.Count; i++)
		{
			if (layouter.FloatingItems[i].FloatingEntity == LeafWidget)
			{
				return true;
			}
		}
		return false;
	}

	private void CreateLayoutedWidget(int i)
	{
		m_ltWidget = new LayoutedWidget(LeafWidget);
		Layouter layouter = m_lcOperator as Layouter;
		if (LeafWidget is WPicture)
		{
			WPicture wPicture = LeafWidget as WPicture;
			SetBoundsForLayoutedWidget(layouter.FloatingItems[i].TextWrappingBounds, wPicture.DistanceFromLeft, wPicture.DistanceFromTop, wPicture.DistanceFromRight, wPicture.DistanceFromBottom);
		}
		else if (LeafWidget is Shape)
		{
			Shape shape = LeafWidget as Shape;
			SetBoundsForLayoutedWidget(layouter.FloatingItems[i].TextWrappingBounds, shape.WrapFormat.DistanceLeft, shape.WrapFormat.DistanceTop, shape.WrapFormat.DistanceRight, shape.WrapFormat.DistanceBottom);
			if (shape.Rotation != 0f)
			{
				RectangleF boundingBoxCoordinates = base.DrawingContext.GetBoundingBoxCoordinates(new RectangleF(0f, 0f, shape.Width, shape.Height), shape.Rotation);
				SizeF sizeF = LeafWidget.Measure(base.DrawingContext);
				m_ltWidget.Bounds = new RectangleF(m_ltWidget.Bounds.X - boundingBoxCoordinates.X, m_ltWidget.Bounds.Y - boundingBoxCoordinates.Y, sizeF.Width, sizeF.Height);
			}
			LayoutShapeTextBody();
			if (shape.Rotation != 0f)
			{
				m_ltWidget.Bounds = base.DrawingContext.GetBoundingBoxCoordinates(m_ltWidget.Bounds, shape.Rotation);
			}
		}
		else if (LeafWidget is WTextBox)
		{
			WTextBoxFormat textBoxFormat = (LeafWidget as WTextBox).TextBoxFormat;
			SetBoundsForLayoutedWidget(layouter.FloatingItems[i].TextWrappingBounds, textBoxFormat.WrapDistanceLeft, textBoxFormat.WrapDistanceTop, textBoxFormat.WrapDistanceRight, textBoxFormat.WrapDistanceBottom);
			if (textBoxFormat.Rotation != 0f)
			{
				RectangleF boundingBoxCoordinates2 = base.DrawingContext.GetBoundingBoxCoordinates(new RectangleF(0f, 0f, textBoxFormat.Width, textBoxFormat.Height), textBoxFormat.Rotation);
				SizeF sizeF2 = LeafWidget.Measure(base.DrawingContext);
				m_ltWidget.Bounds = new RectangleF(m_ltWidget.Bounds.X - boundingBoxCoordinates2.X, m_ltWidget.Bounds.Y - boundingBoxCoordinates2.Y, sizeF2.Width, sizeF2.Height);
			}
			LayoutTextBoxBody();
			if (textBoxFormat.Rotation != 0f)
			{
				m_ltWidget.Bounds = base.DrawingContext.GetBoundingBoxCoordinates(m_ltWidget.Bounds, textBoxFormat.Rotation);
			}
		}
		else if (LeafWidget is GroupShape)
		{
			GroupShape groupShape = LeafWidget as GroupShape;
			SetBoundsForLayoutedWidget(layouter.FloatingItems[i].TextWrappingBounds, groupShape.WrapFormat.DistanceLeft, groupShape.WrapFormat.DistanceTop, groupShape.WrapFormat.DistanceRight, groupShape.WrapFormat.DistanceBottom);
			if (groupShape.Rotation != 0f)
			{
				RectangleF boundingBoxCoordinates3 = base.DrawingContext.GetBoundingBoxCoordinates(new RectangleF(0f, 0f, groupShape.Width, groupShape.Height), groupShape.Rotation);
				m_ltWidget.Bounds = new RectangleF(m_ltWidget.Bounds.X - boundingBoxCoordinates3.X, m_ltWidget.Bounds.Y - boundingBoxCoordinates3.Y, groupShape.Width, groupShape.Height);
			}
			LayoutGroupShapeTextBody();
		}
		else if (LeafWidget is WChart)
		{
			WChart wChart = LeafWidget as WChart;
			SetBoundsForLayoutedWidget(layouter.FloatingItems[i].TextWrappingBounds, wChart.WrapFormat.DistanceLeft, wChart.WrapFormat.DistanceTop, wChart.WrapFormat.DistanceRight, wChart.WrapFormat.DistanceBottom);
		}
	}

	private void SetBoundsForLayoutedWidget(RectangleF textWrappingBounds, float distanceFromLeft, float distanceFromTop, float distanceFromRight, float distanceFromBottom)
	{
		m_ltWidget.Bounds = new RectangleF(textWrappingBounds.X + distanceFromLeft, textWrappingBounds.Y + distanceFromTop, textWrappingBounds.Width - (distanceFromRight + distanceFromLeft), textWrappingBounds.Height - (distanceFromBottom + distanceFromTop));
	}

	private void LayoutGroupShapeTextBody()
	{
		GroupShape groupShape = m_ltWidget.Widget as GroupShape;
		if (groupShape.Is2007Shape && !groupShape.IsFloatingItem(isTextWrapAround: true) && !groupShape.HasChildGroupShape())
		{
			LayoutCustomChildShape(groupShape, m_ltWidget);
			return;
		}
		if (groupShape.GetTextWrappingStyle() == TextWrappingStyle.Inline)
		{
			if (groupShape.Rotation != 0f)
			{
				RectangleF boundingBoxCoordinates = base.DrawingContext.GetBoundingBoxCoordinates(new RectangleF(0f, 0f, m_ltWidget.Bounds.Width, m_ltWidget.Bounds.Height), groupShape.Rotation);
				m_ltWidget.Bounds = new RectangleF(m_ltWidget.Bounds.X - boundingBoxCoordinates.X, m_ltWidget.Bounds.Y - boundingBoxCoordinates.Y, m_ltWidget.Bounds.Width, m_ltWidget.Bounds.Height);
			}
			else
			{
				m_ltWidget.Bounds = new RectangleF(m_ltWidget.Bounds.X + groupShape.LeftEdgeExtent, m_ltWidget.Bounds.Y + groupShape.TopEdgeExtent, m_ltWidget.Bounds.Width, m_ltWidget.Bounds.Height);
			}
		}
		List<RectangleF> list = new List<RectangleF>();
		List<float> list2 = new List<float>();
		List<bool?> list3 = new List<bool?>();
		List<bool?> list4 = new List<bool?>();
		list.Add(m_ltWidget.Bounds);
		list2.Add(groupShape.Rotation);
		list3.Add(groupShape.flipH);
		list4.Add(groupShape.flipV);
		LayoutChildGroupTextBody(groupShape.Rotation, groupShape, m_ltWidget, 1f, 1f, list, list2, list3, list4);
		list.Clear();
		list = null;
		list2.Clear();
		list2 = null;
		list3.Clear();
		list3 = null;
		list4.Clear();
		list4 = null;
		if (DocumentLayouter.IsUpdatingTOC)
		{
			_ = DocumentLayouter.IsEndUpdateTOC;
		}
	}

	private void LayoutCustomChildShape(Entity entity, LayoutedWidget layoutedWidget)
	{
		RectangleF bounds = layoutedWidget.Bounds;
		GroupShape groupShape = null;
		ChildGroupShape childGroupShape = null;
		if (entity is GroupShape)
		{
			groupShape = entity as GroupShape;
		}
		else
		{
			childGroupShape = entity as ChildGroupShape;
		}
		ChildShapeCollection childShapeCollection = ((groupShape != null) ? groupShape.ChildShapes : childGroupShape.ChildShapes);
		float x = groupShape?.CoordinateXOrigin ?? childGroupShape.CoordinateXOrigin;
		float y = groupShape?.CoordinateYOrigin ?? childGroupShape.CoordinateYOrigin;
		string text = ((groupShape != null) ? groupShape.CoordinateSize : childGroupShape.CoordinateSize);
		PointF pointF = new PointF(x, y);
		SizeF sizeF = new SizeF(1000f, 1000f);
		if (text != null)
		{
			string[] array = text.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 2)
			{
				if (float.TryParse(array[0].Trim(), out var result))
				{
					float num = float.Parse(array[0].Trim());
					if (num > 0f)
					{
						sizeF.Width = num;
					}
				}
				if (float.TryParse(array[1].Trim(), out result))
				{
					float num2 = float.Parse(array[1].Trim());
					if (num2 > 0f)
					{
						sizeF.Height = num2;
					}
				}
			}
		}
		SizeF sizeF2 = default(SizeF);
		sizeF2.Width = bounds.Width / sizeF.Width;
		sizeF2.Height = bounds.Height / sizeF.Height;
		PointF pointF2 = default(PointF);
		pointF2.X = bounds.X - sizeF2.Width * pointF.X;
		pointF2.Y = bounds.Y - sizeF2.Height * pointF.Y;
		List<RectangleF> list = new List<RectangleF>();
		List<float> list2 = new List<float>();
		List<bool?> list3 = new List<bool?>();
		List<bool?> list4 = new List<bool?>();
		list.Add(m_ltWidget.Bounds);
		list2.Add(groupShape.Rotation);
		list3.Add(groupShape.flipH);
		list4.Add(groupShape.flipV);
		foreach (ChildShape item in childShapeCollection)
		{
			if (item != null)
			{
				ChildShape childShape2 = item;
				float leftMargin = childShape2.LeftMargin;
				float topMargin = childShape2.TopMargin;
				float width = childShape2.Width;
				float height = childShape2.Height;
				leftMargin *= sizeF2.Width;
				topMargin *= sizeF2.Height;
				float extensionWidth = sizeF2.Width;
				float extensionHeight = sizeF2.Height;
				float rotation = childShape2.Rotation;
				CalculateRotationOfChildShape(rotation, width, height, ref leftMargin, ref topMargin, ref extensionWidth, ref extensionHeight);
				leftMargin += pointF2.X;
				topMargin += pointF2.Y;
				LayoutedWidget layoutedWidget2 = new LayoutedWidget(childShape2);
				layoutedWidget2.Bounds = new RectangleF(leftMargin, topMargin, width * extensionWidth, height * extensionHeight);
				if (item.VMLPathPoints != null && item.VMLPathPoints.Count > 0)
				{
					childShape2.UpdateVMLPathPoints(layoutedWidget2.Bounds, childShape2.Path, new PointF(childShape2.CoordinateXOrigin, childShape2.CoordinateYOrigin), childShape2.CoordinateSize, childShape2.VMLPathPoints, childShape2.m_isVMLPathUpdated);
					childShape2.m_isVMLPathUpdated = true;
				}
				CalculateSumOfRotationAngle(ref rotation, layoutedWidget2, list, list2, list3, list4);
				SetChildShapeFlips(rotation, childShape2);
				if (!childShape2.IsPicture)
				{
					LayoutChildShapeTextBody(layoutedWidget2);
				}
				layoutedWidget.ChildWidgets.Add(layoutedWidget2);
				layoutedWidget2.Owner = layoutedWidget;
			}
		}
		list.Clear();
		list = null;
		list2.Clear();
		list2 = null;
		list3.Clear();
		list3 = null;
		list4.Clear();
		list4 = null;
	}

	private RectangleF GetChildShapePositionToDraw(RectangleF groupShapeBounds, float groupShapeRotation, RectangleF childShapeBounds)
	{
		double num = groupShapeBounds.X + groupShapeBounds.Width / 2f;
		double num2 = groupShapeBounds.Y + groupShapeBounds.Height / 2f;
		if (groupShapeRotation > 360f)
		{
			groupShapeRotation %= 360f;
		}
		double num3 = (double)groupShapeRotation * Math.PI / 180.0;
		double num4 = Math.Sin(num3);
		double num5 = Math.Cos(num3);
		double num6 = childShapeBounds.X + childShapeBounds.Width / 2f;
		double num7 = childShapeBounds.Y + childShapeBounds.Height / 2f;
		double num8 = num + ((double)childShapeBounds.X - num) * num5 - ((double)childShapeBounds.Y - num2) * num4;
		double num9 = num2 + ((double)childShapeBounds.X - num) * num4 + ((double)childShapeBounds.Y - num2) * num5;
		double num10 = num + (num6 - num) * num5 - (num7 - num2) * num4;
		double num11 = num2 + (num6 - num) * num4 + (num7 - num2) * num5;
		double num12 = (double)(360f - groupShapeRotation) * Math.PI / 180.0;
		num4 = Math.Sin(num12);
		num5 = Math.Cos(num12);
		double num13 = num10 + (num8 - num10) * num5 - (num9 - num11) * num4;
		double num14 = num11 + (num8 - num10) * num4 + (num9 - num11) * num5;
		return new RectangleF((float)num13, (float)num14, childShapeBounds.Width, childShapeBounds.Height);
	}

	private void LayoutChildShapeTextBody(LayoutedWidget layoutedtWidget)
	{
		ChildShape childShape = layoutedtWidget.Widget as ChildShape;
		RectangleF layoutRect = ((childShape.AutoShapeType == AutoShapeType.Rectangle) ? layoutedtWidget.Bounds : childShape.GetBoundsToLayoutShapeTextBody(childShape.AutoShapeType, childShape.ShapeGuide, layoutedtWidget.Bounds));
		if ((childShape.AutoShapeType == AutoShapeType.Rectangle || childShape.IsTextBoxShape) && (childShape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.Horizontal || childShape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.HorizontalFarEast) && layoutedtWidget.Bounds.Height < m_layoutArea.ClientActiveArea.Height)
		{
			layoutRect.Height = m_layoutArea.ClientActiveArea.Height;
		}
		UpdateChildShapeBoundsToLayoutTextBody(ref layoutRect, childShape.TextFrame.InternalMargin, layoutedtWidget);
		bool isNeedToUpdateWidth = false;
		bool flag = childShape.TextBody.IsAutoFit(ref isNeedToUpdateWidth, layoutedtWidget.Widget);
		if ((childShape.AutoShapeType == AutoShapeType.Rectangle || childShape.IsTextBoxShape) && (flag || isNeedToUpdateWidth))
		{
			layoutRect = UpdateAutoFitLayoutingBounds(layoutRect, !((IWidget)childShape).LayoutInfo.IsVerticalText, flag, isNeedToUpdateWidth);
		}
		if (((IWidget)childShape).LayoutInfo.IsVerticalText)
		{
			layoutRect = new RectangleF(layoutRect.X, layoutRect.Y, layoutRect.Height, layoutRect.Width);
		}
		childShape.TextLayoutingBounds = layoutRect;
		float paragraphYPosition = (m_lcOperator as Layouter).ParagraphYPosition;
		LayoutedWidget layoutedWidget = LayoutContext.Create(childShape.TextBody, m_lcOperator, base.IsForceFitLayout).Layout(layoutRect);
		(m_lcOperator as Layouter).ParagraphYPosition = paragraphYPosition;
		if (!DocumentLayouter.IsUpdatingTOC || !DocumentLayouter.IsEndUpdateTOC)
		{
			if ((childShape.AutoShapeType == AutoShapeType.Rectangle || childShape.IsTextBoxShape) && (flag || isNeedToUpdateWidth))
			{
				UpdateAutoFitRenderingBounds(layoutedtWidget, layoutedWidget, !((IWidget)childShape).LayoutInfo.IsVerticalText, isNeedToUpdateWidth, flag, childShape.TextFrame.InternalMargin);
			}
			if ((childShape.AutoShapeType == AutoShapeType.Rectangle || childShape.IsTextBoxShape) && (childShape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.Horizontal || childShape.TextFrame.TextDirection == DocGen.DocIO.DLS.TextDirection.HorizontalFarEast) && layoutedtWidget.Bounds.Height < m_layoutArea.ClientActiveArea.Height)
			{
				layoutRect.Height = layoutedtWidget.Bounds.Height - (childShape.TextFrame.InternalMargin.Top + childShape.TextFrame.InternalMargin.Bottom);
			}
			UpdateLayoutedWidgetBasedOnVerticalAlignment(layoutRect, layoutedWidget, childShape.TextFrame.TextVerticalAlignment);
			layoutedtWidget.ChildWidgets.Add(layoutedWidget);
			layoutedWidget.Owner = layoutedtWidget;
		}
	}

	private void LayoutChildGroupTextBody(float groupShapeRotation, Entity entity, LayoutedWidget layoutedWidget, float extensionWidth, float extensionHeight, List<RectangleF> groupShapeBounds, List<float> groupShapeRotations, List<bool?> groupShapeHorzFlips, List<bool?> groupShapeVertFlips)
	{
		float num = 0f;
		float num2 = 0f;
		RectangleF bounds = layoutedWidget.Bounds;
		ChildShapeCollection obj = ((entity is GroupShape) ? (entity as GroupShape).ChildShapes : (entity as ChildGroupShape).ChildShapes);
		float num3 = ((entity is GroupShape) ? (entity as GroupShape).CoordinateXOrigin : (entity as ChildGroupShape).CoordinateXOrigin);
		float num4 = ((entity is GroupShape) ? (entity as GroupShape).CoordinateYOrigin : (entity as ChildGroupShape).CoordinateYOrigin);
		float num5 = ((entity is GroupShape) ? (entity as GroupShape).X : (entity as ChildGroupShape).X);
		float num6 = ((entity is GroupShape) ? (entity as GroupShape).Y : (entity as ChildGroupShape).Y);
		foreach (ChildShape item in obj)
		{
			layoutedWidget.GetGroupShapeExtent(ref extensionWidth, ref extensionHeight, entity, bounds);
			if (item is ChildGroupShape)
			{
				ChildGroupShape childGroupShape = item as ChildGroupShape;
				if (childGroupShape.Is2007Shape)
				{
					num = childGroupShape.LeftMargin - num3;
					num2 = childGroupShape.TopMargin - num4;
				}
				else
				{
					num = childGroupShape.OffsetXValue - num5;
					num2 = childGroupShape.OffsetYValue - num6;
				}
				float rotation = childGroupShape.Rotation;
				groupShapeRotations.Add(rotation);
				groupShapeHorzFlips.Add(childGroupShape.flipH);
				groupShapeVertFlips.Add(childGroupShape.flipV);
				float left = num * extensionWidth + bounds.X;
				float top = num2 * extensionHeight + bounds.Y;
				float width = childGroupShape.Width;
				float height = childGroupShape.Height;
				CalculateRotationOfChildShape(rotation, width, height, ref left, ref top, ref extensionWidth, ref extensionHeight);
				LayoutedWidget layoutedWidget2 = new LayoutedWidget(childGroupShape);
				layoutedWidget2.Bounds = new RectangleF(left, top, width * extensionWidth, height * extensionHeight);
				groupShapeBounds.Add(layoutedWidget2.Bounds);
				LayoutChildGroupTextBody(rotation, childGroupShape, layoutedWidget2, extensionWidth, extensionHeight, groupShapeBounds, groupShapeRotations, groupShapeHorzFlips, groupShapeVertFlips);
				layoutedWidget.ChildWidgets.Add(layoutedWidget2);
				layoutedWidget2.Owner = layoutedWidget;
			}
			else if (item != null)
			{
				ChildShape childShape2 = item;
				if (childShape2.Is2007Shape)
				{
					num = childShape2.LeftMargin - num3;
					num2 = childShape2.TopMargin - num4;
				}
				else
				{
					num = childShape2.X - num5;
					num2 = childShape2.Y - num6;
				}
				float rotation2 = childShape2.Rotation;
				float left2 = num * extensionWidth + bounds.X;
				float top2 = num2 * extensionHeight + bounds.Y;
				float width2 = childShape2.Width;
				float height2 = childShape2.Height;
				CalculateRotationOfChildShape(rotation2, width2, height2, ref left2, ref top2, ref extensionWidth, ref extensionHeight);
				LayoutedWidget layoutedWidget3 = new LayoutedWidget(childShape2);
				layoutedWidget3.Bounds = new RectangleF(left2, top2, width2 * extensionWidth, height2 * extensionHeight);
				CalculateSumOfRotationAngle(ref rotation2, layoutedWidget3, groupShapeBounds, groupShapeRotations, groupShapeHorzFlips, groupShapeVertFlips);
				SetChildShapeFlips(rotation2, childShape2);
				if (!childShape2.IsPicture)
				{
					LayoutChildShapeTextBody(layoutedWidget3);
				}
				layoutedWidget.ChildWidgets.Add(layoutedWidget3);
				layoutedWidget3.Owner = layoutedWidget;
			}
		}
		if (groupShapeRotations.Count > 0)
		{
			groupShapeRotations.RemoveAt(groupShapeRotations.Count - 1);
		}
		if (groupShapeBounds.Count > 0)
		{
			groupShapeBounds.RemoveAt(groupShapeBounds.Count - 1);
		}
		if (groupShapeHorzFlips.Count > 0)
		{
			groupShapeHorzFlips.RemoveAt(groupShapeHorzFlips.Count - 1);
		}
		if (groupShapeVertFlips.Count > 0)
		{
			groupShapeVertFlips.RemoveAt(groupShapeVertFlips.Count - 1);
		}
	}

	private void CalculateRotationOfChildShape(float rotation, float width, float height, ref float left, ref float top, ref float extensionWidth, ref float extensionHeight)
	{
		if ((rotation >= 45f && rotation < 135f) || (rotation >= 225f && rotation < 315f))
		{
			float num = extensionWidth;
			extensionWidth = extensionHeight;
			extensionHeight = num;
			left -= width * (extensionWidth - extensionHeight) / 2f;
			top -= height * (extensionHeight - extensionWidth) / 2f;
		}
	}

	private void CalculateSumOfRotationAngle(ref float rotation, LayoutedWidget ltWidget, List<RectangleF> groupShapeBounds, List<float> groupShapeRotations, List<bool?> groupShapeHorzFlips, List<bool?> groupShapeVertFlips)
	{
		float num = 0f;
		float num2 = 0f;
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i <= groupShapeRotations.Count; i++)
		{
			num2 = ((i != groupShapeRotations.Count) ? groupShapeRotations[i] : rotation);
			if (i > 0)
			{
				if (groupShapeHorzFlips[i - 1].HasValue)
				{
					flag ^= groupShapeHorzFlips[i - 1].Value;
				}
				if (groupShapeVertFlips[i - 1].HasValue)
				{
					flag2 ^= groupShapeVertFlips[i - 1].Value;
				}
			}
			num = ((!(flag ^ flag2)) ? (num + num2) : (num - num2));
		}
		num %= 360f;
		rotation = num;
		for (int num3 = groupShapeBounds.Count - 1; num3 >= 0; num3--)
		{
			flag = false;
			flag2 = false;
			num2 = groupShapeRotations[num3];
			RectangleF groupShapeBounds2 = groupShapeBounds[num3];
			if (groupShapeHorzFlips[num3].HasValue)
			{
				flag = groupShapeHorzFlips[num3].Value;
			}
			if (groupShapeVertFlips[num3].HasValue)
			{
				flag2 = groupShapeVertFlips[num3].Value;
			}
			if (flag || flag2)
			{
				PointF[] points = new PointF[4]
				{
					new PointF(ltWidget.Bounds.X, ltWidget.Bounds.Y),
					new PointF(ltWidget.Bounds.X + ltWidget.Bounds.Width, ltWidget.Bounds.Y),
					new PointF(ltWidget.Bounds.Right, ltWidget.Bounds.Bottom),
					new PointF(ltWidget.Bounds.X, ltWidget.Bounds.Y + ltWidget.Bounds.Height)
				};
				Matrix matrix = new Matrix();
				PointF pointF = new PointF(groupShapeBounds2.X + groupShapeBounds2.Width / 2f, groupShapeBounds2.Y + groupShapeBounds2.Height / 2f);
				Matrix target = new Matrix(1f, 0f, 0f, -1f, 0f, 0f);
				Matrix target2 = new Matrix(-1f, 0f, 0f, 1f, 0f, 0f);
				if (flag2)
				{
					base.DrawingContext.MatrixMultiply(matrix, target, MatrixOrder.Append);
					base.DrawingContext.MatrixTranslate(matrix, 0f, pointF.Y * 2f, MatrixOrder.Append);
				}
				if (flag)
				{
					base.DrawingContext.MatrixMultiply(matrix, target2, MatrixOrder.Append);
					base.DrawingContext.MatrixTranslate(matrix, pointF.X * 2f, 0f, MatrixOrder.Append);
				}
				matrix.TransformPoints(points);
				ltWidget.Bounds = CreateRect(points);
			}
			if (num2 != 0f)
			{
				ltWidget.Bounds = GetChildShapePositionToDraw(groupShapeBounds2, num2, ltWidget.Bounds);
			}
		}
	}

	private void SetChildShapeFlips(float rotation, ChildShape childShape)
	{
		if (IsGroupFlipV(childShape.Owner) || IsGroupFlipH(childShape.Owner))
		{
			int flipHCount = GetFlipHCount(childShape.Owner, childShape.FlipHorizantal ? 1 : 0);
			int flipVCount = GetFlipVCount(childShape.Owner, childShape.FlipVertical ? 1 : 0);
			bool flipHorizantalToRender = flipHCount % 2 != 0;
			bool flipVerticalToRender = flipVCount % 2 != 0;
			childShape.FlipVerticalToRender = flipVerticalToRender;
			childShape.FlipHorizantalToRender = flipHorizantalToRender;
			childShape.RotationToRender = rotation;
		}
		else
		{
			childShape.FlipVerticalToRender = childShape.FlipVertical;
			childShape.FlipHorizantalToRender = childShape.FlipHorizantal;
			childShape.RotationToRender = rotation;
		}
	}

	internal int GetFlipHCount(Entity entity, int count)
	{
		while (entity != null && (entity is GroupShape || entity is ChildGroupShape))
		{
			if ((entity is GroupShape && (entity as GroupShape).FlipHorizontal) || (entity is ChildGroupShape && (entity as ChildGroupShape).FlipHorizantal))
			{
				count++;
			}
			entity = ((entity is GroupShape) ? (entity as GroupShape).Owner : (entity as ChildGroupShape).Owner);
		}
		return count;
	}

	internal int GetFlipVCount(Entity entity, int count)
	{
		while (entity != null && (entity is GroupShape || entity is ChildGroupShape))
		{
			if ((entity is GroupShape && (entity as GroupShape).FlipVertical) || (entity is ChildGroupShape && (entity as ChildGroupShape).FlipVertical))
			{
				count++;
			}
			entity = ((entity is GroupShape) ? (entity as GroupShape).Owner : (entity as ChildGroupShape).Owner);
		}
		return count;
	}

	internal bool IsGroupFlipH(Entity entity)
	{
		while (entity != null && (entity is GroupShape || entity is ChildGroupShape))
		{
			if ((entity is GroupShape && (entity as GroupShape).FlipHorizontal) || (entity is ChildGroupShape && (entity as ChildGroupShape).FlipHorizantal))
			{
				return true;
			}
			entity = ((entity is GroupShape) ? (entity as GroupShape).Owner : (entity as ChildGroupShape).Owner);
		}
		return false;
	}

	internal bool IsGroupFlipV(Entity entity)
	{
		while (entity != null && (entity is GroupShape || entity is ChildGroupShape))
		{
			if ((entity is GroupShape && (entity as GroupShape).FlipVertical) || (entity is ChildGroupShape && (entity as ChildGroupShape).FlipVertical))
			{
				return true;
			}
			entity = ((entity is GroupShape) ? (entity as GroupShape).Owner : (entity as ChildGroupShape).Owner);
		}
		return false;
	}

	private static RectangleF CreateRect(PointF[] points)
	{
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		int num5 = points.Length;
		for (int i = 0; i < num5; i++)
		{
			float x = points[i].X;
			float y = points[i].Y;
			if (x < num)
			{
				num = x;
			}
			if (x > num3)
			{
				num3 = x;
			}
			if (y < num2)
			{
				num2 = y;
			}
			if (y > num4)
			{
				num4 = y;
			}
		}
		return new RectangleF(num, num2, num3 - num, num4 - num2);
	}

	private void LayoutShapeTextBody()
	{
		Shape shape = m_ltWidget.Widget as Shape;
		if (shape.Is2007Shape && shape.VMLPathPoints != null && shape.VMLPathPoints.Count > 0 && !shape.IsFloatingItem(isTextWrapAround: true))
		{
			shape.UpdateVMLPathPoints(m_ltWidget.Bounds, shape.Path, new PointF(shape.CoordinateXOrigin, shape.CoordinateYOrigin), shape.CoordinateSize, shape.VMLPathPoints, shape.m_isVMLPathUpdated);
			shape.m_isVMLPathUpdated = true;
		}
		else
		{
			shape.VMLPathPoints = null;
		}
		if (shape.GetTextWrappingStyle() == TextWrappingStyle.Inline && shape.Rotation == 0f)
		{
			m_ltWidget.Bounds = new RectangleF(m_ltWidget.Bounds.X + shape.LeftEdgeExtent, m_ltWidget.Bounds.Y + shape.TopEdgeExtent, m_ltWidget.Bounds.Width, m_ltWidget.Bounds.Height);
		}
		RectangleF layoutRect = shape.GetBoundsToLayoutShapeTextBody(shape.AutoShapeType, shape.ShapeGuide, m_ltWidget.Bounds);
		UpdateShapeBoundsToLayoutTextBody(ref layoutRect, shape.TextFrame.InternalMargin);
		bool isNeedToUpdateWidth = false;
		bool flag = shape.TextBody.IsAutoFit(ref isNeedToUpdateWidth, m_ltWidget.Widget);
		if (shape.AutoShapeType == AutoShapeType.Rectangle && (flag || isNeedToUpdateWidth))
		{
			layoutRect = UpdateAutoFitLayoutingBounds(layoutRect, !((IWidget)shape).LayoutInfo.IsVerticalText, flag, isNeedToUpdateWidth);
		}
		if (((IWidget)shape).LayoutInfo.IsVerticalText)
		{
			layoutRect = new RectangleF(layoutRect.X, layoutRect.Y, layoutRect.Height, layoutRect.Width);
		}
		shape.TextLayoutingBounds = layoutRect;
		RectangleF rectangleF = m_ltWidget.Bounds;
		if (shape.Rotation != 0f && !shape.IsWrappingBoundsAdded())
		{
			rectangleF = base.DrawingContext.GetBoundingBoxCoordinates(m_ltWidget.Bounds, shape.Rotation);
		}
		RectangleF rectangleF2 = layoutRect;
		if (shape.Rotation != 0f && shape.TextFrame.Upright)
		{
			layoutRect = base.DrawingContext.GetBoundingBoxCoordinates(layoutRect, shape.Rotation);
		}
		if (shape.Rotation != 0f && shape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			RectangleF rectangleF3 = rectangleF;
			rectangleF.X = m_ltWidget.Bounds.X;
			rectangleF.Y = m_ltWidget.Bounds.Y;
			if (shape.TextFrame.Upright)
			{
				float num = layoutRect.X - rectangleF3.X;
				float num2 = layoutRect.Y - rectangleF3.Y;
				layoutRect.X = m_ltWidget.Bounds.X + num;
				layoutRect.Y = m_ltWidget.Bounds.Y + num2;
				shape.TextLayoutingBounds = new RectangleF(layoutRect.X, layoutRect.Y, shape.TextLayoutingBounds.Height, shape.TextLayoutingBounds.Width);
			}
			else
			{
				layoutRect.X = rectangleF2.X;
				layoutRect.Y = rectangleF2.Y;
			}
		}
		float height = layoutRect.Height;
		if (!((IWidget)shape).LayoutInfo.IsVerticalText && (shape.Rotation == 0f || !shape.TextFrame.Upright) && m_ltWidget.Bounds.Height < m_layoutArea.ClientActiveArea.Height)
		{
			layoutRect.Height = m_layoutArea.ClientActiveArea.Height;
		}
		bool canSplitbyCharacter = (m_lcOperator as Layouter).m_canSplitbyCharacter;
		bool canSplitByTab = (m_lcOperator as Layouter).m_canSplitByTab;
		bool isFirstItemInLine = (m_lcOperator as Layouter).IsFirstItemInLine;
		List<float> list = new List<float>();
		if ((m_lcOperator as Layouter).m_lineSpaceWidths != null)
		{
			list.AddRange((m_lcOperator as Layouter).m_lineSpaceWidths);
		}
		float effectiveJustifyWidth = (m_lcOperator as Layouter).m_effectiveJustifyWidth;
		LayoutContext layoutContext = LayoutContext.Create(shape.TextBody, m_lcOperator, base.IsForceFitLayout);
		float paragraphYPosition = (m_lcOperator as Layouter).ParagraphYPosition;
		LayoutedWidget layoutedWidget = layoutContext.Layout(layoutRect);
		(m_lcOperator as Layouter).ParagraphYPosition = paragraphYPosition;
		if (!DocumentLayouter.IsUpdatingTOC || !DocumentLayouter.IsEndUpdateTOC)
		{
			if (shape.AutoShapeType == AutoShapeType.Rectangle && (flag || isNeedToUpdateWidth))
			{
				UpdateAutoFitRenderingBounds(m_ltWidget, layoutedWidget, !((IWidget)shape).LayoutInfo.IsVerticalText, isNeedToUpdateWidth, flag, shape.TextFrame.InternalMargin);
			}
			if (!((IWidget)shape).LayoutInfo.IsVerticalText && (shape.Rotation == 0f || !shape.TextFrame.Upright) && m_ltWidget.Bounds.Height < m_layoutArea.ClientActiveArea.Height)
			{
				layoutRect.Height = height;
			}
			UpdateLayoutedWidgetBasedOnVerticalAlignment(layoutRect, layoutedWidget, shape.TextFrame.TextVerticalAlignment);
			m_ltWidget.ChildWidgets.Add(layoutedWidget);
			layoutedWidget.Owner = m_ltWidget;
			if (shape.Rotation != 0f && !shape.IsWrappingBoundsAdded())
			{
				m_ltWidget.Bounds = rectangleF;
			}
			(m_lcOperator as Layouter).ResetWordLayoutingFlags(canSplitbyCharacter, canSplitByTab, isFirstItemInLine, list, effectiveJustifyWidth);
		}
	}

	private void LayoutTextBoxBody()
	{
		WTextBox wTextBox = m_ltWidget.Widget as WTextBox;
		if (wTextBox.TextBoxFormat.VMLPathPoints != null && wTextBox.TextBoxFormat.VMLPathPoints.Count > 0 && !wTextBox.IsFloatingItem(isTextWrapAround: true))
		{
			wTextBox.UpdateVMLPathPoints(m_ltWidget.Bounds, wTextBox.TextBoxFormat.Path, new PointF(wTextBox.TextBoxFormat.CoordinateXOrigin, wTextBox.TextBoxFormat.CoordinateYOrigin), wTextBox.TextBoxFormat.CoordinateSize, wTextBox.TextBoxFormat.VMLPathPoints, wTextBox.TextBoxFormat.m_isVMLPathUpdated);
			wTextBox.TextBoxFormat.m_isVMLPathUpdated = true;
		}
		else
		{
			wTextBox.TextBoxFormat.VMLPathPoints = null;
		}
		WTextBoxFormat textBoxFormat = wTextBox.TextBoxFormat;
		if (wTextBox.GetTextWrappingStyle() == TextWrappingStyle.Inline && textBoxFormat.Rotation == 0f && wTextBox.Shape != null)
		{
			m_ltWidget.Bounds = new RectangleF(m_ltWidget.Bounds.X + wTextBox.Shape.LeftEdgeExtent, m_ltWidget.Bounds.Y + wTextBox.Shape.TopEdgeExtent, m_ltWidget.Bounds.Width, m_ltWidget.Bounds.Height);
		}
		RectangleF rectangleF = m_ltWidget.Bounds;
		if (textBoxFormat.Rotation != 0f && !wTextBox.IsWrappingBoundsAdded())
		{
			rectangleF = base.DrawingContext.GetBoundingBoxCoordinates(m_ltWidget.Bounds, textBoxFormat.Rotation);
		}
		if (textBoxFormat.Rotation != 0f && textBoxFormat.VerticalAlignment == ShapeVerticalAlignment.Top && (textBoxFormat.VerticalOrigin == VerticalOrigin.Margin || textBoxFormat.VerticalOrigin == VerticalOrigin.BottomMargin) && textBoxFormat.VerticalPosition == 0f)
		{
			rectangleF.Y = m_ltWidget.Bounds.Y;
		}
		if (textBoxFormat.Rotation != 0f && textBoxFormat.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			rectangleF.X = m_ltWidget.Bounds.X;
			rectangleF.Y = m_ltWidget.Bounds.Y;
		}
		RectangleF bounds = m_ltWidget.Bounds;
		if (textBoxFormat.Rotation != 0f && wTextBox.Shape != null && wTextBox.Shape.TextFrame.Upright)
		{
			bounds = rectangleF;
		}
		if ((textBoxFormat.TextDirection == DocGen.DocIO.DLS.TextDirection.Horizontal || textBoxFormat.TextDirection == DocGen.DocIO.DLS.TextDirection.HorizontalFarEast) && m_ltWidget.Bounds.Height < m_layoutArea.ClientActiveArea.Height)
		{
			bounds.Height = m_layoutArea.ClientActiveArea.Height;
		}
		bool flag = !((IWidget)wTextBox).LayoutInfo.IsVerticalText;
		bool isNeedToUpdateWidth = false;
		bool flag2 = wTextBox.TextBoxBody.IsAutoFit(ref isNeedToUpdateWidth, m_ltWidget.Widget);
		if (flag2 || isNeedToUpdateWidth)
		{
			bounds = UpdateAutoFitLayoutingBounds(bounds, flag, flag2, isNeedToUpdateWidth);
		}
		if (wTextBox.IsNoNeedToConsiderLineWidth())
		{
			bounds.X += textBoxFormat.InternalMargin.Left;
			bounds.Y += textBoxFormat.InternalMargin.Top;
			bounds.Width -= textBoxFormat.InternalMargin.Left + textBoxFormat.InternalMargin.Right;
			bounds.Height -= textBoxFormat.InternalMargin.Top + textBoxFormat.InternalMargin.Bottom;
		}
		else
		{
			wTextBox.CalculateBoundsBasedOnLineWidth(ref bounds, textBoxFormat);
		}
		if (bounds.Width <= 0f && textBoxFormat.InternalMargin.Right > 0f)
		{
			bounds.Width = textBoxFormat.InternalMargin.Right;
		}
		if (!flag)
		{
			bounds = new RectangleF(bounds.X, bounds.Y, bounds.Height, bounds.Width);
		}
		else if (textBoxFormat.LineStyle == TextBoxLineStyle.ThinThick)
		{
			bounds.Y += textBoxFormat.LineWidth;
		}
		wTextBox.TextLayoutingBounds = bounds;
		bool canSplitbyCharacter = (m_lcOperator as Layouter).m_canSplitbyCharacter;
		bool canSplitByTab = (m_lcOperator as Layouter).m_canSplitByTab;
		bool isFirstItemInLine = (m_lcOperator as Layouter).IsFirstItemInLine;
		List<float> list = new List<float>();
		if ((m_lcOperator as Layouter).m_lineSpaceWidths != null)
		{
			list.AddRange((m_lcOperator as Layouter).m_lineSpaceWidths);
		}
		float effectiveJustifyWidth = (m_lcOperator as Layouter).m_effectiveJustifyWidth;
		LayoutContext layoutContext = LayoutContext.Create(wTextBox.TextBoxBody, m_lcOperator, base.IsForceFitLayout);
		layoutContext.ClientLayoutAreaRight = bounds.Width;
		float paragraphYPosition = (m_lcOperator as Layouter).ParagraphYPosition;
		LayoutedWidget layoutedWidget = layoutContext.Layout(bounds);
		(m_lcOperator as Layouter).ParagraphYPosition = paragraphYPosition;
		if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
		{
			return;
		}
		if (flag2 || isNeedToUpdateWidth)
		{
			UpdateAutoFitRenderingBounds(m_ltWidget, layoutedWidget, flag, isNeedToUpdateWidth, flag2, textBoxFormat.InternalMargin);
		}
		if (textBoxFormat.Rotation != 0f && flag2 && textBoxFormat.TextWrappingStyle == TextWrappingStyle.Square)
		{
			UpdateAutoFitTextBoxBounds(wTextBox, layoutedWidget, textBoxFormat);
		}
		if (flag2 && layoutedWidget.Bounds.Height != rectangleF.Height && !base.IsForceFitLayout && textBoxFormat.TextWrappingStyle == TextWrappingStyle.Inline && IsLeafWidgetNeedToBeSplitted(layoutedWidget.Bounds.Size, m_layoutArea.ClientActiveArea.Width, bounds, 0f))
		{
			m_ltState = LayoutState.NotFitted;
			return;
		}
		if (flag)
		{
			if ((textBoxFormat.TextDirection == DocGen.DocIO.DLS.TextDirection.Horizontal || textBoxFormat.TextDirection == DocGen.DocIO.DLS.TextDirection.HorizontalFarEast) && m_ltWidget.Bounds.Height < m_layoutArea.ClientActiveArea.Height)
			{
				bounds.Height = m_ltWidget.Bounds.Height - (textBoxFormat.InternalMargin.Top + textBoxFormat.InternalMargin.Bottom);
			}
			UpdateLayoutedWidgetBasedOnVerticalAlignment(bounds, layoutedWidget, textBoxFormat.TextVerticalAlignment);
		}
		m_ltWidget.ChildWidgets.Add(layoutedWidget);
		layoutedWidget.Owner = m_ltWidget;
		if (textBoxFormat.Rotation != 0f && !wTextBox.IsWrappingBoundsAdded())
		{
			m_ltWidget.Bounds = rectangleF;
		}
		(m_lcOperator as Layouter).ResetWordLayoutingFlags(canSplitbyCharacter, canSplitByTab, isFirstItemInLine, list, effectiveJustifyWidth);
	}

	private void UpdateAutoFitTextBoxBounds(WTextBox textBox, LayoutedWidget textBodyLtWidget, WTextBoxFormat textBoxFormat)
	{
		float num = 0f;
		float x = m_ltWidget.Bounds.X;
		float y = m_ltWidget.Bounds.Y;
		float num2 = x;
		float num3 = y;
		if (!textBox.IsWrappingBoundsAdded())
		{
			return;
		}
		float num4 = ((textBox.Shape != null) ? textBox.Shape.Rotation : textBoxFormat.Rotation);
		if (num4 > 360f)
		{
			num4 %= 360f;
		}
		else if (num4 < 0f)
		{
			num4 = 360f + num4;
		}
		if ((m_lcOperator as Layouter).FloatingItems.Count > 0)
		{
			for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
			{
				if ((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity == textBox)
				{
					num = ((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity as WTextBox).TextLayoutingBounds.Height + textBoxFormat.InternalMargin.Top + textBoxFormat.InternalMargin.Bottom;
				}
			}
		}
		if ((!(num4 >= 45f) || !(num4 < 135f)) && (!(num4 >= 225f) || !(num4 < 315f)))
		{
			return;
		}
		if (textBox.GetShapeHorizontalAlignment() == ShapeHorizontalAlignment.Right)
		{
			if (textBoxFormat.Height > m_ltWidget.Bounds.Height)
			{
				float num5 = textBoxFormat.Height / 2f - num / 2f;
				num2 += num5;
				num3 += num5;
			}
			else
			{
				float num6 = num / 2f - textBoxFormat.Height / 2f;
				num2 -= num6;
				num3 -= num6;
			}
		}
		else if (textBoxFormat.Height > m_ltWidget.Bounds.Height)
		{
			float num7 = textBoxFormat.Height / 2f - num / 2f;
			num2 -= num7;
			num3 += num7;
		}
		else
		{
			float num8 = num / 2f - textBoxFormat.Height / 2f;
			num2 += num8;
			num3 -= num8;
		}
		m_ltWidget.Bounds = new RectangleF(num2, num3, m_ltWidget.Bounds.Width, num);
		float num9 = 0f;
		float num10 = 0f;
		if (Math.Round(num2, 2) != Math.Round(x, 2))
		{
			num10 = num2 - x;
		}
		if (Math.Round(num3, 2) != Math.Round(y, 2))
		{
			num9 = num3 - y;
		}
		if (num10 != 0f || num9 != 0f)
		{
			textBodyLtWidget.ShiftLocation(num10, num9, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false, isNeedToShiftOwnerWidget: true);
		}
	}

	private void UpdateAutoFitRenderingBounds(LayoutedWidget textbodyOwnerWidget, LayoutedWidget textbodyWidget, bool isHorizontalText, bool isNeedToUpdateWidth, bool isAutoFit, InternalMargin margin)
	{
		float height = textbodyOwnerWidget.Bounds.Height;
		RectangleF textLayoutingBounds = default(RectangleF);
		if (textbodyOwnerWidget.Widget is WTextBox)
		{
			textLayoutingBounds = (textbodyOwnerWidget.Widget as WTextBox).TextLayoutingBounds;
		}
		else if (textbodyOwnerWidget.Widget is Shape)
		{
			textLayoutingBounds = (textbodyOwnerWidget.Widget as Shape).TextLayoutingBounds;
		}
		else if (textbodyOwnerWidget.Widget is ChildShape)
		{
			textLayoutingBounds = (textbodyOwnerWidget.Widget as ChildShape).TextLayoutingBounds;
		}
		if (isHorizontalText)
		{
			UpdateHorizontalTextRenderingBounds(textbodyOwnerWidget, isAutoFit, isNeedToUpdateWidth, textbodyWidget, margin, textLayoutingBounds);
		}
		else
		{
			UpdateVerticalTextRenderingBounds(textbodyOwnerWidget, isAutoFit, isNeedToUpdateWidth, textbodyWidget, margin, textLayoutingBounds);
		}
		float shiftDistance = textbodyOwnerWidget.Bounds.Height - height;
		UpdateXYPositionBasedOnModifiedSize(textbodyOwnerWidget.Bounds.Width, textbodyOwnerWidget.Bounds.Height, textbodyWidget, shiftDistance);
		if (textbodyOwnerWidget.Widget is WTextBox)
		{
			(textbodyOwnerWidget.Widget as WTextBox).TextLayoutingBounds = textbodyWidget.Bounds;
		}
		else if (textbodyOwnerWidget.Widget is Shape)
		{
			(textbodyOwnerWidget.Widget as Shape).TextLayoutingBounds = textbodyWidget.Bounds;
		}
		else if (textbodyOwnerWidget.Widget is ChildShape)
		{
			(textbodyOwnerWidget.Widget as ChildShape).TextLayoutingBounds = textbodyWidget.Bounds;
		}
	}

	private void UpdateHorizontalTextRenderingBounds(LayoutedWidget textbodyOwnerWidget, bool isAutoFit, bool isNeedToUpdateWidth, LayoutedWidget ltWidget, InternalMargin margin, RectangleF textLayoutingBounds)
	{
		float num = 0f;
		if (textbodyOwnerWidget.Widget is WTextBox && !(textbodyOwnerWidget.Widget as WTextBox).IsNoNeedToConsiderLineWidth())
		{
			num = (textbodyOwnerWidget.Widget as WTextBox).TextBoxFormat.LineWidth;
		}
		else if (textbodyOwnerWidget.Widget is Shape && !(textbodyOwnerWidget.Widget as Shape).IsNoNeedToConsiderLineWidth())
		{
			num = (textbodyOwnerWidget.Widget as Shape).LineFormat.Weight;
		}
		float num2 = 0f;
		if (isAutoFit)
		{
			num2 = ltWidget.Bounds.Height + margin.Top + margin.Bottom + num;
			textbodyOwnerWidget.Bounds = new RectangleF(textbodyOwnerWidget.Bounds.X, textbodyOwnerWidget.Bounds.Y, textbodyOwnerWidget.Bounds.Width, num2);
		}
		if (isNeedToUpdateWidth)
		{
			num2 = ltWidget.Bounds.Width + margin.Left + margin.Right;
			if (IsAnyOfParagraphHasMultipleLines(ltWidget))
			{
				num2 = m_layoutArea.ClientActiveArea.Width;
			}
			else
			{
				float maximumWidth = 0f;
				UpdateBoundsBasedOnParagraphAlignments(ltWidget, ref num2, textLayoutingBounds.Width, ref maximumWidth, isRecursiveCall: false);
				num2 += margin.Left + margin.Right;
			}
			textbodyOwnerWidget.Bounds = new RectangleF(textbodyOwnerWidget.Bounds.X, textbodyOwnerWidget.Bounds.Y, num2, textbodyOwnerWidget.Bounds.Height);
		}
	}

	private void UpdateVerticalTextRenderingBounds(LayoutedWidget textbodyOwnerWidget, bool isAutoFit, bool isNeedToUpdateWidth, LayoutedWidget ltWidget, InternalMargin margin, RectangleF textLayoutingBounds)
	{
		float num = 0f;
		if (isNeedToUpdateWidth)
		{
			num = ltWidget.Bounds.Width + margin.Top + margin.Bottom;
			if (IsAnyOfParagraphHasMultipleLines(ltWidget))
			{
				num = (m_lcOperator as Layouter).ClientLayoutArea.Height + margin.Top + margin.Bottom;
			}
			else
			{
				float maximumWidth = 0f;
				UpdateBoundsBasedOnParagraphAlignments(ltWidget, ref num, textLayoutingBounds.Width, ref maximumWidth, isRecursiveCall: false);
				num += margin.Top + margin.Bottom;
			}
			textbodyOwnerWidget.Bounds = new RectangleF(textbodyOwnerWidget.Bounds.X, textbodyOwnerWidget.Bounds.Y, textbodyOwnerWidget.Bounds.Width, num);
		}
		if (isAutoFit)
		{
			float num2 = margin.Left + margin.Right;
			num = ltWidget.Bounds.Height + num2;
			if (num > 1584f)
			{
				num = 1584f;
			}
			textbodyOwnerWidget.Bounds = new RectangleF(textbodyOwnerWidget.Bounds.X, textbodyOwnerWidget.Bounds.Y, num, textbodyOwnerWidget.Bounds.Height);
		}
	}

	private void UpdateXYPositionBasedOnModifiedSize(float modifiedWidth, float modifiedHeight, LayoutedWidget textBodyLtWidget, float shiftDistance)
	{
		float indentX = m_ltWidget.Bounds.X;
		float indentY = m_ltWidget.Bounds.Y;
		float num = indentX;
		float num2 = indentY;
		SizeF size = new SizeF(modifiedWidth, modifiedHeight);
		WParagraph ownerParagraphValue = (m_ltWidget.Widget as ParagraphItem).GetOwnerParagraphValue();
		if (ownerParagraphValue != null && !ownerParagraphValue.IsInCell)
		{
			GetFloattingItemPosition(ref indentX, ref indentY, ref size, shiftDistance);
		}
		if (m_ltWidget != null && m_ltWidget.Widget is ParagraphItem && (m_ltWidget.Widget as Entity).IsFloatingItem(isTextWrapAround: false) && GetAngle(m_ltWidget.Widget as ParagraphItem) != 0f)
		{
			ParagraphItem paragraphItem = m_ltWidget.Widget as ParagraphItem;
			RectangleF rectangleF = ShiftXYBasedOnRotation(paragraphItem, new RectangleF(indentX, indentY, m_ltWidget.Bounds.Width, m_ltWidget.Bounds.Height - shiftDistance));
			indentX = rectangleF.X;
			indentY = ((paragraphItem.GetShapeVerticalAlignment() != ShapeVerticalAlignment.Bottom || rectangleF.Y == indentY) ? rectangleF.Y : num2);
		}
		m_ltWidget.Bounds = new RectangleF(indentX, indentY, m_ltWidget.Bounds.Width, m_ltWidget.Bounds.Height);
		float num3 = 0f;
		float num4 = 0f;
		if (Math.Round(indentX, 2) != Math.Round(num, 2))
		{
			num4 = indentX - num;
		}
		if (Math.Round(indentY, 2) != Math.Round(num2, 2))
		{
			num3 = indentY - num2;
		}
		if (num4 != 0f || num3 != 0f)
		{
			textBodyLtWidget.ShiftLocation(num4, num3, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false, isNeedToShiftOwnerWidget: true);
		}
	}

	private RectangleF UpdateAutoFitLayoutingBounds(RectangleF bounds, bool isHorizontalText, bool isAutoFit, bool isNeedToUpdateWidth)
	{
		if (isHorizontalText)
		{
			if (isNeedToUpdateWidth)
			{
				bounds.Width = m_layoutArea.ClientActiveArea.Width;
			}
			if (isAutoFit)
			{
				if ((m_lcOperator as Layouter).IsLayoutingHeaderFooter && !(m_lcOperator as Layouter).IsLayoutingHeader)
				{
					bounds.Height = (m_lcOperator as Layouter).ClientLayoutArea.Bottom - bounds.Y;
				}
				else
				{
					bounds.Height = (m_lcOperator as Layouter).CurrentSection.PageSetup.PageSize.Height - bounds.Y;
				}
			}
		}
		else
		{
			if (isNeedToUpdateWidth)
			{
				bounds.Height = (m_lcOperator as Layouter).ClientLayoutArea.Height;
			}
			if (isAutoFit)
			{
				bounds.Width = 1584f;
			}
		}
		return bounds;
	}

	private bool IsAnyOfParagraphHasMultipleLines(LayoutedWidget textBodyItems)
	{
		for (int i = 0; i < textBodyItems.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = textBodyItems.ChildWidgets[i];
			if (layoutedWidget.Widget is BlockContentControl && IsAnyOfParagraphHasMultipleLines(layoutedWidget))
			{
				return true;
			}
			if (layoutedWidget.Widget is WParagraph && textBodyItems.ChildWidgets[i].ChildWidgets.Count > 1)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateBoundsBasedOnParagraphAlignments(LayoutedWidget ltWidget, ref float updatedWidth, float layoutedClientWidth, ref float maximumWidth, bool isRecursiveCall)
	{
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = ltWidget.ChildWidgets[i];
			if (layoutedWidget.Widget is WParagraph)
			{
				float maximumWidth2 = GetMaximumWidth(layoutedWidget);
				if (maximumWidth < maximumWidth2)
				{
					maximumWidth = maximumWidth2;
				}
			}
			else if (layoutedWidget.Widget is BlockContentControl)
			{
				UpdateBoundsBasedOnParagraphAlignments(layoutedWidget, ref updatedWidth, layoutedClientWidth, ref maximumWidth, isRecursiveCall: true);
			}
		}
		if (!isRecursiveCall && maximumWidth != layoutedClientWidth)
		{
			ShiftXPositionBasedOnAlignment(ltWidget, maximumWidth - layoutedClientWidth);
			updatedWidth = maximumWidth;
		}
	}

	private void ShiftXPositionBasedOnAlignment(LayoutedWidget ltWidget, float widthReduced)
	{
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = ltWidget.ChildWidgets[i];
			if (layoutedWidget.Widget is WParagraph)
			{
				if (layoutedWidget.Widget.LayoutInfo is ParagraphLayoutInfo paragraphLayoutInfo)
				{
					if (paragraphLayoutInfo.Justification == HAlignment.Center)
					{
						layoutedWidget.ShiftLocation(widthReduced / 2f, 0.0, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false, isNeedToShiftOwnerWidget: false);
					}
					else if (paragraphLayoutInfo.Justification == HAlignment.Right)
					{
						layoutedWidget.ShiftLocation(widthReduced, 0.0, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false, isNeedToShiftOwnerWidget: false);
					}
				}
			}
			else if (layoutedWidget.Widget is BlockContentControl)
			{
				ShiftXPositionBasedOnAlignment(layoutedWidget, widthReduced);
			}
		}
	}

	private float GetMaximumWidth(LayoutedWidget lineContainer)
	{
		float num = 0f;
		float num2 = 0f;
		ParagraphLayoutInfo paragraphLayoutInfo = lineContainer.Widget.LayoutInfo as ParagraphLayoutInfo;
		WParagraph wParagraph = lineContainer.Widget as WParagraph;
		if (wParagraph.ListFormat != null && !wParagraph.ListFormat.IsEmptyList && wParagraph.ListFormat.ListType != ListType.NoList && paragraphLayoutInfo.FirstLineIndent > 0f)
		{
			num2 = Math.Abs(base.DrawingContext.GetListValue(wParagraph, paragraphLayoutInfo, wParagraph.ListFormat));
		}
		num2 += ((paragraphLayoutInfo.FirstLineIndent > 0f) ? paragraphLayoutInfo.FirstLineIndent : 0f);
		num2 += paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.Margins.Right;
		if (lineContainer.ChildWidgets.Count > 0)
		{
			LayoutedWidget layoutedWidget = lineContainer.ChildWidgets[0];
			if (paragraphLayoutInfo.Justification == HAlignment.Center || paragraphLayoutInfo.Justification == HAlignment.Right)
			{
				RectangleF innerItemsRenderingBounds = base.DrawingContext.GetInnerItemsRenderingBounds(layoutedWidget);
				if (num < innerItemsRenderingBounds.Width)
				{
					num = innerItemsRenderingBounds.Width;
				}
			}
			else if (num < layoutedWidget.Bounds.Width)
			{
				num = layoutedWidget.Bounds.Width;
			}
		}
		return num + num2;
	}

	private void UpdateLayoutedWidgetBasedOnVerticalAlignment(RectangleF bounds, LayoutedWidget ltWidget, DocGen.DocIO.DLS.VerticalAlignment textVerticalAlignment)
	{
		float num = 0f;
		switch (textVerticalAlignment)
		{
		case DocGen.DocIO.DLS.VerticalAlignment.Middle:
			num = (bounds.Height - ltWidget.Bounds.Height) / 2f;
			break;
		case DocGen.DocIO.DLS.VerticalAlignment.Bottom:
			num = bounds.Height - ltWidget.Bounds.Height;
			break;
		}
		if (num > 0f)
		{
			ltWidget.ShiftLocation(0.0, num, isPictureNeedToBeShifted: false, isFromFloatingItemVerticalAlignment: false);
		}
	}

	private void UpdateShapeBoundsToLayoutTextBody(ref RectangleF layoutRect, InternalMargin internalMargin)
	{
		Shape shape = m_ltWidget.Widget as Shape;
		layoutRect.Height -= layoutRect.Y;
		layoutRect.Y += m_ltWidget.Bounds.Y;
		layoutRect.Width -= layoutRect.X;
		layoutRect.X += m_ltWidget.Bounds.X;
		if (shape.IsNoNeedToConsiderLineWidth())
		{
			layoutRect.X += internalMargin.Left;
			layoutRect.Y += internalMargin.Top;
			layoutRect.Width -= internalMargin.Left + internalMargin.Right;
			layoutRect.Height -= internalMargin.Top + internalMargin.Bottom;
		}
		else
		{
			layoutRect.X += internalMargin.Left + shape.LineFormat.Weight / 2f;
			layoutRect.Y += internalMargin.Top + shape.LineFormat.Weight / 2f;
			layoutRect.Width -= internalMargin.Left + internalMargin.Right + shape.LineFormat.Weight;
			layoutRect.Height -= internalMargin.Top + internalMargin.Bottom + shape.LineFormat.Weight;
		}
	}

	private void UpdateChildShapeBoundsToLayoutTextBody(ref RectangleF layoutRect, InternalMargin internalMargin, LayoutedWidget ltWidget)
	{
		ChildShape childShape = ltWidget.Widget as ChildShape;
		if (childShape.AutoShapeType != AutoShapeType.Rectangle)
		{
			layoutRect.Height -= layoutRect.Y;
			layoutRect.Y += ltWidget.Bounds.Y;
			layoutRect.Width -= layoutRect.X;
			layoutRect.X += ltWidget.Bounds.X;
			layoutRect.X += internalMargin.Left + childShape.LineFormat.Weight / 2f;
			layoutRect.Y += internalMargin.Top + childShape.LineFormat.Weight / 2f;
			layoutRect.Width -= internalMargin.Left + internalMargin.Right + childShape.LineFormat.Weight;
			layoutRect.Height -= internalMargin.Top + internalMargin.Bottom + childShape.LineFormat.Weight;
			return;
		}
		if (childShape.ElementType == EntityType.TextBox && !childShape.LineFormat.Line)
		{
			layoutRect.X += internalMargin.Left;
			layoutRect.Y += internalMargin.Top;
			layoutRect.Width -= internalMargin.Left + internalMargin.Right;
			layoutRect.Height -= internalMargin.Top + internalMargin.Bottom;
		}
		else
		{
			layoutRect.X += internalMargin.Left + childShape.LineFormat.Weight / 2f;
			layoutRect.Y += internalMargin.Top + childShape.LineFormat.Weight / 2f;
			layoutRect.Width -= internalMargin.Left + internalMargin.Right + childShape.LineFormat.Weight;
			layoutRect.Height -= internalMargin.Top + internalMargin.Bottom + childShape.LineFormat.Weight;
		}
		if (layoutRect.Width <= 0f && internalMargin.Right > 0f)
		{
			layoutRect.Width = internalMargin.Right;
		}
	}

	private WTextRange GetCurrTextRange()
	{
		if (!(LeafWidget is WTextRange))
		{
			if (!(LeafWidget is SplitStringWidget) || !((LeafWidget as SplitStringWidget).RealStringWidget is WTextRange))
			{
				return null;
			}
			return (LeafWidget as SplitStringWidget).RealStringWidget as WTextRange;
		}
		return LeafWidget as WTextRange;
	}

	private void UpdateTabWidth(ref RectangleF rect, ref SizeF size, WTextRange textRange)
	{
		Layouter layouter = m_lcOperator as Layouter;
		float xPosition = rect.X;
		float pageMarginLeft = GetPageMarginLeft();
		float pageMarginRight = GetPageMarginRight();
		float num = layouter.PreviousTab.Position + pageMarginLeft;
		float previousTabWidth = layouter.PreviousTabWidth;
		float num2 = rect.X - (num - previousTabWidth);
		TabJustification justification = layouter.PreviousTab.Justification;
		if (justification == TabJustification.Centered && num2 / 2f < previousTabWidth)
		{
			xPosition = num + num2 / 2f;
			if (rect.Right < xPosition)
			{
				xPosition = rect.Right;
			}
			rect.Width -= xPosition - rect.X;
			rect.X = xPosition;
			CreateLayoutArea(rect);
		}
		else if (justification == TabJustification.Right && rect.X < num)
		{
			xPosition = num;
			rect.Width -= xPosition - rect.X;
			rect.X = xPosition;
			CreateLayoutArea(rect);
		}
		else if (justification == TabJustification.Decimal)
		{
			UpdateLeafWidgetPosition(ref rect, ref xPosition, num2);
		}
		TabsLayoutInfo tabsLayoutInfo = LeafWidget.LayoutInfo as TabsLayoutInfo;
		WParagraph ownerParagraph = GetOwnerParagraph();
		float num3 = ((ownerParagraph.ParagraphFormat.FirstLineIndent < 0f) ? ownerParagraph.ParagraphFormat.LeftIndent : 0f);
		tabsLayoutInfo.PageMarginLeft = pageMarginLeft;
		tabsLayoutInfo.PageMarginRight = pageMarginRight;
		ParagraphLayoutInfo paragraphLayoutInfo = ((IWidget)ownerParagraph).LayoutInfo as ParagraphLayoutInfo;
		float num4 = ((!paragraphLayoutInfo.IsFirstLine) ? paragraphLayoutInfo.Margins.Left : (paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.FirstLineIndent));
		float num5 = (float)tabsLayoutInfo.GetNextTabPosition(xPosition - pageMarginLeft);
		if ((ownerParagraph.ParagraphFormat.FirstLineIndent < 0f || (paragraphLayoutInfo.IsFirstLine && num4 < 0f && num2 < 0f && tabsLayoutInfo.m_currTab.Position + pageMarginLeft < num5 && !layouter.IsLayoutingHeaderFooter)) && Math.Round(xPosition - pageMarginLeft, 2) < Math.Round(num3, 2) && (tabsLayoutInfo.m_currTab.Position == 0f || tabsLayoutInfo.m_currTab.Position > num3) && !layouter.IsTabWidthUpdatedBasedOnIndent && ownerParagraph.Document.UseHangingIndentAsTabPosition && (tabsLayoutInfo.m_list.Count <= 0 || !(ownerParagraph.ParagraphFormat.FirstLineIndent < 0f) || tabsLayoutInfo.m_currTab.Position != 0f || !(num3 > 0f) || !(xPosition < pageMarginLeft)))
		{
			if (Math.Round(xPosition + num5, 2) >= Math.Round(pageMarginLeft + num3, 2))
			{
				tabsLayoutInfo.IsTabWidthUpdatedBasedOnIndent = true;
			}
			layouter.IsTabWidthUpdatedBasedOnIndent = true;
			num5 = num3 - (xPosition - pageMarginLeft);
			if (ownerParagraph.ParagraphFormat.FirstLineIndent < 0f && paragraphLayoutInfo.IsFirstLine && num4 < 0f && num2 < 0f && IsInTextBox(ownerParagraph) != null)
			{
				num5 += paragraphLayoutInfo.Margins.Left;
			}
			if (paragraphLayoutInfo.IsFirstLine && Math.Round(xPosition + num5 - pageMarginLeft, 2) > Math.Round(base.ClientLayoutAreaRight, 2) && ownerParagraph.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && !IsLeafWidgetIsInCell(LeafWidget as WTextRange))
			{
				UpdateAreaMaxWidth();
			}
		}
		else if (num4 > base.ClientLayoutAreaRight && Math.Round(xPosition - pageMarginLeft, 2) <= Math.Round(num4, 2) && tabsLayoutInfo.m_currTab.Position != xPosition - pageMarginLeft + num5 && !IsLeafWidgetIsInCell(LeafWidget as WTextRange))
		{
			num5 = num4 - (xPosition - pageMarginLeft);
		}
		if (IsLeafWidgetIsInCell(LeafWidget as WTextRange) && num5 > m_layoutArea.ClientActiveArea.Width && xPosition - num4 == pageMarginLeft && tabsLayoutInfo.m_currTab.Position == 0f && tabsLayoutInfo.m_currTab.Justification == TabJustification.Left)
		{
			tabsLayoutInfo.IsTabWidthUpdatedBasedOnIndent = false;
			num5 = m_layoutArea.ClientActiveArea.Width;
		}
		size.Width = (float)Math.Round(num5, 2);
		if (!tabsLayoutInfo.IsTabWidthUpdatedBasedOnIndent && (tabsLayoutInfo.CurrTabJustification == TabJustification.Centered || tabsLayoutInfo.CurrTabJustification == TabJustification.Right || (tabsLayoutInfo.CurrTabJustification == TabJustification.Decimal && LeafWidget is ParagraphItem && (!IsLeafWidgetIsInCell(LeafWidget as ParagraphItem) || ownerParagraph.ParagraphFormat.Tabs.Count != 1))))
		{
			size.Width = 0f;
		}
		(m_lcOperator as Layouter).PreviousTabWidth = num5;
		tabsLayoutInfo.TabWidth = num5;
		LeafWidget.LayoutInfo.Size = new SizeF(size.Width, LeafWidget.LayoutInfo.Size.Height);
	}

	private void UpdateAreaMaxWidth()
	{
		UpdateAreaWidth(0f);
		base.IsAreaUpdated = true;
		base.IsTabStopBeyondRightMarginExists = true;
	}

	private float ClientAreaRight(WParagraph paragraph, float rectRight)
	{
		if (IsBaseFromSection(paragraph))
		{
			return (m_lcOperator as Layouter).ClientLayoutArea.Right;
		}
		return rectRight;
	}

	private float GetPageMarginLeft()
	{
		float left = (m_lcOperator as Layouter).ClientLayoutArea.Left;
		ParagraphItem paraItem = ((LeafWidget is ParagraphItem) ? (LeafWidget as ParagraphItem) : ((LeafWidget is SplitStringWidget) ? ((LeafWidget as SplitStringWidget).RealStringWidget as ParagraphItem) : null));
		WParagraph ownerParagraph = GetOwnerParagraph();
		if (IsLeafWidgetIsInCell(paraItem))
		{
			left = (((IWidget)(ownerParagraph.GetOwnerEntity() as WTableCell)).LayoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Left;
		}
		else if (ownerParagraph != null && !ownerParagraph.IsInCell && ownerParagraph.ParagraphFormat.IsFrame)
		{
			left = (m_lcOperator as Layouter).FrameLayoutArea.Left;
		}
		else if (ownerParagraph != null && ownerParagraph.OwnerBase != null)
		{
			if (ownerParagraph.OwnerBase.OwnerBase is WTextBox)
			{
				left = (ownerParagraph.OwnerBase.OwnerBase as WTextBox).TextLayoutingBounds.Left;
			}
			else if (ownerParagraph.OwnerBase.OwnerBase is Shape)
			{
				left = (ownerParagraph.OwnerBase.OwnerBase as Shape).TextLayoutingBounds.Left;
			}
		}
		return left;
	}

	private float GetPageMarginRight()
	{
		float result = (m_lcOperator as Layouter).ClientLayoutArea.Right;
		WParagraph ownerParagraph = GetOwnerParagraph();
		if (ownerParagraph == null)
		{
			return result;
		}
		if (ownerParagraph.IsInCell)
		{
			CellLayoutInfo cellLayoutInfo = ((IWidget)(ownerParagraph.GetOwnerEntity() as WTableCell)).LayoutInfo as CellLayoutInfo;
			result = cellLayoutInfo.CellContentLayoutingBounds.Right + cellLayoutInfo.Margins.Right;
		}
		else if (!ownerParagraph.IsInCell && ownerParagraph.ParagraphFormat.IsFrame)
		{
			result = (m_lcOperator as Layouter).FrameLayoutArea.Right;
		}
		else if (ownerParagraph.OwnerBase != null && ownerParagraph.OwnerBase.OwnerBase is WTextBox)
		{
			WTextBox wTextBox = ownerParagraph.OwnerBase.OwnerBase as WTextBox;
			result = wTextBox.TextLayoutingBounds.Right + wTextBox.TextBoxFormat.InternalMargin.Right;
		}
		return result;
	}

	private void UpdateLeafWidgetPosition(ref RectangleF rect, ref float xPosition, float width)
	{
		WParagraph ownerParagraph = GetOwnerParagraph();
		if (ownerParagraph == null)
		{
			return;
		}
		int num = ownerParagraph.ChildEntities.InnerList.IndexOf(LeafWidget as WTextRange);
		int decimalTabStart = 0;
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			if (ownerParagraph.ChildEntities[num2] is WTextRange && (ownerParagraph.ChildEntities[num2] as ILeafWidget).LayoutInfo is TabsLayoutInfo)
			{
				decimalTabStart = num2;
				break;
			}
		}
		float leftWidth = base.DrawingContext.GetLeftWidth(ownerParagraph, decimalTabStart, num);
		Layouter layouter = m_lcOperator as Layouter;
		if (leftWidth < layouter.PreviousTabWidth)
		{
			width -= leftWidth;
			xPosition = layouter.PreviousTab.Position + GetPageMarginLeft() + width;
			rect.Width -= xPosition - rect.X;
			rect.X = xPosition;
			CreateLayoutArea(rect);
		}
	}

	internal bool IsLeafWidgetNeedToBeSplitted(SizeF size, float clientActiveAreaWidth, RectangleF rect, float floatingItemIndentY)
	{
		bool flag = false;
		bool flag2 = IsClipped(size);
		if (size.Width < clientActiveAreaWidth && IsTextContainsLineBreakCharacters())
		{
			return true;
		}
		if (((!(LeafWidget is WPicture) && !(LeafWidget is WOleObject) && !(LeafWidget is Shape) && !(LeafWidget is GroupShape) && !(LeafWidget is WTextBox) && !(LeafWidget is WChart)) ? TryFit(size) : (IsPictureFit(size, clientActiveAreaWidth, rect, floatingItemIndentY) && (m_layoutArea.Width != 0.0 || IsParagraphItemNeedToFit(LeafWidget as ParagraphItem)))) || flag2 || ((LeafWidget is WTextRange || LeafWidget is SplitStringWidget) && IsTextRangeFitInClientActiveArea(size)))
		{
			return false;
		}
		return true;
	}

	private bool TryFit(SizeF s)
	{
		WParagraph ownerParagraph = GetOwnerParagraph();
		if (s.Width <= m_layoutArea.ClientActiveArea.Width)
		{
			if (!IsNeedToFitBasedOnLineSpacing(ownerParagraph, s) && !base.IsForceFitLayout && (ownerParagraph == null || (!IsLineSpacingFitsWidget(ownerParagraph, s.Height) && !ownerParagraph.IsZeroAutoLineSpace() && !IsNeedToFitSectionBreakParaHavingOnlyFloatingItems(ownerParagraph, LeafWidget))) && !IsNeedToFitItemOfLastParagraph())
			{
				if (LeafWidget is Break && ((LeafWidget as Break).BreakType == BreakType.LineBreak || (LeafWidget as Break).BreakType == BreakType.TextWrappingBreak))
				{
					return IsNeedToFitLineBreak(s);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool IsNeedToFitBasedOnLineSpacing(WParagraph paragraph, SizeF size)
	{
		float num = ((paragraph != null && (paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.AtLeast || paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly)) ? paragraph.ParagraphFormat.LineSpacing : 0f);
		if (paragraph != null && paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly)
		{
			if (LeafWidget is Break && (LeafWidget as Break).BreakType == BreakType.PageBreak)
			{
				num = (((IsWord2013(paragraph.Document) && (LeafWidget as Break).Index == 0 && paragraph.ChildEntities.Count > 1) || !IsWord2013(paragraph.Document)) ? 0f : paragraph.ParagraphFormat.LineSpacing);
			}
			return Math.Abs(num) <= m_layoutArea.ClientActiveArea.Height;
		}
		return Math.Max(size.Height, num) <= m_layoutArea.ClientActiveArea.Height;
	}

	private bool IsNeedToFitSectionBreakParaHavingOnlyFloatingItems(WParagraph ownerPara, ILeafWidget leafWidget)
	{
		WParagraph lastParagraph = ownerPara.Document.LastParagraph;
		if (leafWidget is WTextRange && (leafWidget as WTextRange).OwnerParagraph == null && ownerPara.ChildEntities.Count > 0 && ownerPara.IsContainFloatingItems() && ownerPara.IsParagraphHasSectionBreak() && lastParagraph != ownerPara)
		{
			foreach (Entity childEntity in ownerPara.ChildEntities)
			{
				if (!(childEntity is BookmarkStart) && !(childEntity is BookmarkEnd) && !(childEntity is WFieldMark) && (childEntity as ParagraphItem).GetTextWrappingStyle() != TextWrappingStyle.InFrontOfText && (childEntity as ParagraphItem).GetTextWrappingStyle() != TextWrappingStyle.Behind)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private bool IsNeedToFitItemOfLastParagraph()
	{
		WParagraph ownerParagraph = GetOwnerParagraph();
		if (ownerParagraph != null)
		{
			return (((IWidget)ownerParagraph).LayoutInfo as ParagraphLayoutInfo).IsNotFitted;
		}
		return false;
	}

	private bool IsNeedToFitLastParagraph(float columnWidth, SizeF size)
	{
		WParagraph ownerParagraph = GetOwnerParagraph();
		if (ownerParagraph != null && !ownerParagraph.ParagraphFormat.WidowControl)
		{
			return false;
		}
		if (ownerParagraph != null && ownerParagraph.ChildEntities.Count > 0 && (ownerParagraph.ChildEntities[0] as IWidget).LayoutInfo.IsFirstItemInPage)
		{
			return false;
		}
		if (ownerParagraph != null && (LeafWidget is WOleObject || IsPictureFromOleObject()) && !IsWord2013(ownerParagraph.Document) && size.Width > columnWidth)
		{
			return false;
		}
		return true;
	}

	private bool IsPictureFromOleObject()
	{
		if (LeafWidget is WPicture)
		{
			IEntity previousSibling = (LeafWidget as WPicture).PreviousSibling;
			if (previousSibling != null && previousSibling is WFieldMark && (previousSibling as WFieldMark).Type == FieldMarkType.FieldSeparator && (previousSibling as WFieldMark).ParentField != null && (previousSibling as WFieldMark).ParentField.FieldType == FieldType.FieldEmbed)
			{
				return (previousSibling as WFieldMark).ParentField.OwnerBase is WOleObject;
			}
			return false;
		}
		return false;
	}

	private bool IsNeedToFitLineBreak(SizeF size)
	{
		bool result = false;
		WParagraph ownerParagraph = GetOwnerParagraph();
		ParagraphLayoutInfo paragraphLayoutInfo = ((IWidget)ownerParagraph).LayoutInfo as ParagraphLayoutInfo;
		if (Math.Round(paragraphLayoutInfo.XPosition + paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.Paddings.Left, 2) != Math.Round(m_layoutArea.ClientActiveArea.X, 2) || (size.Height > m_layoutArea.ClientActiveArea.Height && ownerParagraph.ParagraphFormat.IsInFrame() && ((IWidget)ownerParagraph).LayoutInfo.IsClipped))
		{
			result = true;
		}
		return result;
	}

	private bool IsTextContainsLineBreakCharacters()
	{
		string text = ((!(LeafWidget is WField)) ? ((LeafWidget is WTextRange) ? (LeafWidget as WTextRange).Text : null) : (((LeafWidget as WField).FieldType == FieldType.FieldNumPages) ? (LeafWidget as WField).FieldResult : string.Empty));
		SplitStringWidget splitStringWidget = LeafWidget as SplitStringWidget;
		if (text == null || (!text.Contains(ControlChar.LineFeed) && !text.Contains(ControlChar.CarriegeReturn)))
		{
			if (splitStringWidget != null && splitStringWidget.SplittedText != null)
			{
				if (!splitStringWidget.SplittedText.Contains(ControlChar.LineFeed))
				{
					return splitStringWidget.SplittedText.Contains(ControlChar.CarriegeReturn);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	private bool IsClipped(SizeF size)
	{
		bool result = false;
		Entity entity = ((LeafWidget is WPicture) ? (LeafWidget as WPicture) : ((LeafWidget is WChart) ? (LeafWidget as WChart) : ((LeafWidget is WOleObject) ? (LeafWidget as WOleObject).OlePicture : (LeafWidget as Entity))));
		WParagraph ownerParagraph = GetOwnerParagraph();
		if (base.LayoutInfo.IsClipped && !(LeafWidget is WTextRange) && !(LeafWidget is SplitStringWidget) && m_layoutArea.Height != 0.0 && m_layoutArea.Width != 0.0)
		{
			bool isInCell = false;
			bool isInTextBox = false;
			if (entity is ParagraphItem paraItem)
			{
				if (ownerParagraph != null)
				{
					isInCell = ownerParagraph.IsInCell;
					isInTextBox = GetBaseEntity(ownerParagraph) is WTextBox;
				}
				if (ownerParagraph != null && IsFitLeafWidgetInContainerHeight(paraItem, isInCell, isInTextBox, null))
				{
					if ((size.Height > m_layoutArea.ClientActiveArea.Height && ((IWidget)ownerParagraph).LayoutInfo.IsClipped && !base.LayoutInfo.IsVerticalText) || (base.LayoutInfo.IsVerticalText && size.Height > m_layoutArea.ClientActiveArea.Width))
					{
						result = true;
					}
					bool flag = !ownerParagraph.IsInCell || base.DrawingContext.GetCellWidth(paraItem) < size.Width;
					if ((size.Height <= m_layoutArea.ClientActiveArea.Height && flag && (m_lcOperator as Layouter).IsFirstItemInLine && !base.LayoutInfo.IsVerticalText) || (base.LayoutInfo.IsVerticalText && size.Height <= m_layoutArea.ClientActiveArea.Width && size.Width > m_layoutArea.ClientActiveArea.Height))
					{
						result = true;
					}
				}
			}
			else if (!(entity is WPicture) && !(entity is Shape) && !(entity is WChart) && !(entity is GroupShape))
			{
				result = true;
			}
		}
		WTextRange textRange = GetTextRange(LeafWidget);
		if (base.LayoutInfo.IsClipped && textRange != null && m_layoutArea.Height != 0.0 && m_layoutArea.Width != 0.0 && ownerParagraph != null && ownerParagraph.IsInCell && base.LayoutInfo.IsVerticalText && size.Height >= m_layoutArea.ClientActiveArea.Height)
		{
			result = true;
		}
		float num = m_layoutArea.ClientActiveArea.Width;
		if (((textRange != null && textRange.Text.Length == 1) || LeafWidget is WCheckBox || LeafWidget is WDropDownFormField || LeafWidget is WFootnote || (LeafWidget is SplitStringWidget && (LeafWidget as SplitStringWidget).SplittedText != null && (LeafWidget as SplitStringWidget).SplittedText.Length == 1)) && ownerParagraph != null)
		{
			if (ownerParagraph.IsInCell)
			{
				num = ((!(LeafWidget is WFootnote)) ? base.DrawingContext.GetCellWidth(textRange) : base.DrawingContext.GetCellWidth(LeafWidget as WFootnote));
				if (size.Width > num)
				{
					result = true;
				}
			}
			else if (ownerParagraph.OwnerBase != null && ownerParagraph.OwnerBase.OwnerBase is WTextBox)
			{
				num = (ownerParagraph.OwnerBase.OwnerBase as WTextBox).TextLayoutingBounds.Width;
				if (size.Width > num)
				{
					result = true;
				}
			}
			if (IsParagraphItemNeedToFit(textRange))
			{
				result = true;
			}
		}
		Entity ownerEntity = ownerParagraph.GetOwnerEntity();
		if (LeafWidget is WSymbol && ownerParagraph != null && (ownerParagraph.IsInCell || ownerParagraph.IsNeedToFitSymbol(ownerParagraph)))
		{
			num = ((ownerEntity is WTextBox) ? (ownerEntity as WTextBox).TextLayoutingBounds.Width : ((ownerEntity is Shape) ? (ownerEntity as Shape).Width : ((!(ownerEntity is ChildShape)) ? base.DrawingContext.GetCellWidth(LeafWidget as WSymbol) : (ownerEntity as ChildShape).Width)));
			if (size.Width > num)
			{
				result = true;
			}
		}
		if ((LeafWidget is BookmarkStart || LeafWidget is BookmarkEnd) && ownerParagraph != null && ownerParagraph.IsInCell && ownerParagraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly && Math.Abs(ownerParagraph.ParagraphFormat.LineSpacing) > m_layoutArea.ClientActiveArea.Height)
		{
			result = true;
		}
		if (size.Width < num && size.Height >= m_layoutArea.ClientActiveArea.Height && base.LayoutInfo.IsClipped)
		{
			result = true;
		}
		return result;
	}

	private bool IsParagraphItemNeedToFit(ParagraphItem paraItem)
	{
		float num = 0f;
		float num2 = 0f;
		WParagraph ownerParagraph = GetOwnerParagraph();
		if (paraItem != null && ownerParagraph != null)
		{
			WTableCell wTableCell = ownerParagraph.OwnerTextBody as WTableCell;
			num2 = ((!ownerParagraph.IsInCell || wTableCell == null) ? (m_lcOperator as Layouter).ClientLayoutArea.Width : wTableCell.Width);
			ParagraphLayoutInfo paragraphLayoutInfo = ((IWidget)ownerParagraph).LayoutInfo as ParagraphLayoutInfo;
			num = ((!paragraphLayoutInfo.IsFirstLine) ? (paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.Margins.Right) : (paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.Margins.Right + paragraphLayoutInfo.FirstLineIndent));
			if ((m_lcOperator as Layouter).IsFirstItemInLine && (paraItem is WPicture || paraItem is WChart || paraItem is Shape || paraItem is GroupShape || paraItem is WTextBox) && wTableCell != null)
			{
				return true;
			}
			if ((paraItem is WPicture || paraItem is WChart || paraItem is Shape || paraItem is GroupShape || paraItem is WTextBox) && (paraItem.GetTextWrappingStyle() == TextWrappingStyle.InFrontOfText || paraItem.GetTextWrappingStyle() == TextWrappingStyle.Behind) && ownerParagraph.IsInCell)
			{
				return true;
			}
		}
		if (num >= num2)
		{
			return true;
		}
		return false;
	}

	private float GetPararaphLeftIndent()
	{
		WParagraph ownerParagraph = GetOwnerParagraph();
		if (ownerParagraph != null)
		{
			ParagraphLayoutInfo paragraphLayoutInfo = ((IWidget)ownerParagraph).LayoutInfo as ParagraphLayoutInfo;
			if (paragraphLayoutInfo.IsFirstLine)
			{
				return paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.FirstLineIndent;
			}
			return paragraphLayoutInfo.Margins.Left;
		}
		return 0f;
	}

	private bool IsPictureFit(SizeF size, float clientActiveAreaWidth, RectangleF rect, float floatingItemIndentY)
	{
		bool flag = false;
		float emptyTextRangeHeight = GetEmptyTextRangeHeight();
		ParagraphItem paragraphItem = ((LeafWidget is WOleObject) ? (LeafWidget as WOleObject).OlePicture : (LeafWidget as ParagraphItem));
		if (paragraphItem == null)
		{
			return true;
		}
		TextWrappingStyle textWrappingStyle = paragraphItem.GetTextWrappingStyle();
		VerticalOrigin verticalOrigin = paragraphItem.GetVerticalOrigin();
		HorizontalOrigin horizontalOrigin = paragraphItem.GetHorizontalOrigin();
		bool flag2 = textWrappingStyle == TextWrappingStyle.Inline;
		float height = (m_lcOperator as Layouter).ClientLayoutArea.Height;
		WParagraph ownerParagraphValue = paragraphItem.GetOwnerParagraphValue();
		Entity entity = GetBaseEntity(paragraphItem, ownerParagraphValue);
		ParagraphLayoutInfo paragraphLayoutInfo = null;
		if (ownerParagraphValue != null)
		{
			paragraphLayoutInfo = ((IWidget)ownerParagraphValue).LayoutInfo as ParagraphLayoutInfo;
		}
		if (flag2)
		{
			float angle = GetAngle(paragraphItem);
			if (angle != 0f && !(paragraphItem is WPicture))
			{
				rect = base.DrawingContext.GetBoundingBoxCoordinates(new RectangleF(rect.X, rect.Y, size.Width, size.Height), angle);
				size = new SizeF(rect.Width, rect.Height);
			}
			(m_lcOperator as Layouter).m_canSplitbyCharacter = false;
			float num = 0f;
			float num2 = 0f;
			if (ownerParagraphValue != null)
			{
				num2 = GetLineSpacing(ownerParagraphValue);
			}
			float picHeight = ((ownerParagraphValue == null) ? size.Height : ((ownerParagraphValue.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly) ? num2 : ((ownerParagraphValue.ParagraphFormat.LineSpacingRule == LineSpacingRule.AtLeast && num2 > size.Height) ? num2 : size.Height)));
			if (entity is WSection)
			{
				num = (m_lcOperator as Layouter).ClientLayoutArea.Width;
				num -= ((paragraphLayoutInfo == null) ? 0f : (paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.Margins.Right + (paragraphLayoutInfo.IsFirstLine ? (paragraphLayoutInfo.FirstLineIndent + paragraphLayoutInfo.ListTab) : 0f)));
				if (IsFitPictureInSection(num, picHeight, height, size, clientActiveAreaWidth, ownerParagraphValue, paragraphItem))
				{
					flag = true;
				}
			}
			else if (entity is WTextBox)
			{
				if (IsFitPictureInTextBox(size, picHeight, entity))
				{
					flag = true;
				}
			}
			else if (entity is WTable)
			{
				float cellWidth = base.DrawingContext.GetCellWidth(paragraphItem);
				if (IsFitPictureInTable(cellWidth, picHeight, size, entity as WTable))
				{
					flag = true;
				}
			}
			else if (ownerParagraphValue != null && entity is HeaderFooter && entity.Owner is WSection)
			{
				double num3 = 0.0;
				if (ownerParagraphValue.ParagraphFormat.IsFrame)
				{
					height = m_layoutArea.ClientActiveArea.Height;
				}
				else if ((m_lcOperator as Layouter).IsLayoutingHeader)
				{
					num3 = Math.Round((entity.Owner as WSection).PageSetup.HeaderDistance, 2);
					height = m_layoutArea.ClientActiveArea.Height - (float)num3;
				}
				else
				{
					num3 = Math.Round((entity.Owner as WSection).PageSetup.PageSize.Height) - Math.Round((entity.Owner as WSection).PageSetup.FooterDistance, 2);
					height = m_layoutArea.ClientActiveArea.Height - (entity.Owner as WSection).PageSetup.FooterDistance;
				}
				num = (m_lcOperator as Layouter).ClientLayoutArea.Width;
				num -= ((paragraphLayoutInfo == null) ? 0f : (paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.Margins.Right + (paragraphLayoutInfo.IsFirstLine ? (paragraphLayoutInfo.FirstLineIndent + paragraphLayoutInfo.ListTab) : 0f)));
				if (IsFitPictureInHeaderFooter(picHeight, height, size, num, clientActiveAreaWidth, entity, num3))
				{
					flag = true;
				}
			}
		}
		else if ((entity is WTable || verticalOrigin == VerticalOrigin.Paragraph) && (textWrappingStyle == TextWrappingStyle.Square || textWrappingStyle == TextWrappingStyle.Through || textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.TopAndBottom))
		{
			if (entity is HeaderFooter)
			{
				entity = entity.Owner;
			}
			flag = IsFloatingItemFit(entity, size, ownerParagraphValue, paragraphItem, height, rect, floatingItemIndentY, paragraphLayoutInfo);
			if (flag && !IsNextSiblingFitted(verticalOrigin, horizontalOrigin, paragraphItem))
			{
				flag = false;
			}
			if (!flag && paragraphItem is WTextBox && !IsWord2013(paragraphItem.Document) && (paragraphItem as WTextBox).TextBoxFormat.AutoFit)
			{
				flag = true;
			}
		}
		else if ((emptyTextRangeHeight <= m_layoutArea.ClientActiveArea.Height || paragraphItem is WTextBox) && (!(LeafWidget as Entity).IsFloatingItem(isTextWrapAround: true) || verticalOrigin != VerticalOrigin.Line || !IsShapePlacedOnParagraphBottom(ownerParagraphValue, size, paragraphLayoutInfo)))
		{
			flag = true;
			if (!IsNextSiblingFitted(verticalOrigin, horizontalOrigin, paragraphItem))
			{
				flag = false;
			}
		}
		else if ((size.Height <= m_layoutArea.ClientActiveArea.Height && size.Width <= m_layoutArea.ClientActiveArea.Width) || base.IsForceFitLayout)
		{
			flag = true;
		}
		bool flag3 = true;
		if (ownerParagraphValue != null && ownerParagraphValue.ParagraphFormat.FrameHeight != 0f)
		{
			flag3 = ((ushort)Math.Round(ownerParagraphValue.ParagraphFormat.FrameHeight * 20f) & 0x8000) != 0;
		}
		if (ownerParagraphValue != null && !flag && ownerParagraphValue.ParagraphFormat.IsInFrame() && (!flag3 || size.Height <= m_layoutArea.ClientActiveArea.Height))
		{
			flag = true;
			ParagraphLayoutInfo paragraphLayoutInfo2 = null;
			paragraphLayoutInfo2 = ((IWidget)ownerParagraphValue).LayoutInfo as ParagraphLayoutInfo;
			if (flag2 && paragraphLayoutInfo2 != null)
			{
				paragraphLayoutInfo2.SkipBottomBorder = true;
			}
		}
		return flag;
	}

	private bool IsNextSiblingFitted(VerticalOrigin vOrgin, HorizontalOrigin hOrigin, ParagraphItem paraItem)
	{
		if ((vOrgin == VerticalOrigin.Line || hOrigin == HorizontalOrigin.Character) && paraItem.IsFloatingItem(isTextWrapAround: false) && paraItem.NextSibling != null)
		{
			IWidget validInlineNextSibling = GetValidInlineNextSibling(paraItem);
			if (validInlineNextSibling != null)
			{
				SizeF sizeF = default(SizeF);
				if (validInlineNextSibling is WTextRange)
				{
					WTextRange wTextRange = validInlineNextSibling as WTextRange;
					if (wTextRange.m_layoutInfo is TabsLayoutInfo && wTextRange.Text == "")
					{
						TabsLayoutInfo tabsLayoutInfo = wTextRange.m_layoutInfo as TabsLayoutInfo;
						sizeF = new SizeF((float)tabsLayoutInfo.DefaultTabWidth, tabsLayoutInfo.Size.Height);
					}
					else
					{
						sizeF = base.DrawingContext.MeasureTextRange(wTextRange, wTextRange.Text.Split(' ')[0]);
					}
				}
				else
				{
					sizeF = (validInlineNextSibling as ILeafWidget).Measure(base.DrawingContext);
				}
				if ((sizeF.Width > m_layoutArea.ClientActiveArea.Width && (!(sizeF.Width > (m_lcOperator as Layouter).ClientLayoutArea.Width) || !(m_lcOperator as Layouter).IsFirstItemInLine)) || sizeF.Height > m_layoutArea.ClientActiveArea.Height)
				{
					if (IsNeedToWrapFloatingItem(LeafWidget))
					{
						(m_lcOperator as Layouter).WrapFloatingItems.RemoveAt((m_lcOperator as Layouter).WrapFloatingItems.Count - 1);
					}
					return false;
				}
			}
		}
		return true;
	}

	private float GetLineSpacing(WParagraph ownerParagraph)
	{
		switch (ownerParagraph.ParagraphFormat.LineSpacingRule)
		{
		case LineSpacingRule.AtLeast:
		case LineSpacingRule.Exactly:
			return ownerParagraph.ParagraphFormat.LineSpacing;
		case LineSpacingRule.Multiple:
			return ownerParagraph.ParagraphFormat.LineSpacing / 12f;
		default:
			return 0f;
		}
	}

	private bool IsFitPictureInSection(float columnWidth, float picHeight, float pageHeight, SizeF size, float clientActiveAreaWidth, Entity ent, ParagraphItem paraItem)
	{
		paraItem.GetEffectExtentValues(out var leftEdgeExtent, out var rightEgeExtent, out var topEdgeExtent, out var bottomEdgeExtent);
		float num = leftEdgeExtent + rightEgeExtent;
		float num2 = topEdgeExtent + bottomEdgeExtent;
		if (picHeight + num2 <= m_layoutArea.ClientActiveArea.Height || (IsNeedToFitItemOfLastParagraph() && IsNeedToFitLastParagraph(columnWidth, size)) || (picHeight + num2 >= m_layoutArea.ClientActiveArea.Height && ((m_lcOperator as Layouter).IsLayoutingHeaderFooter || base.IsForceFitLayout)))
		{
			if (!(Math.Round(size.Width + num, 2) <= Math.Round(m_layoutArea.ClientActiveArea.Width, 2)))
			{
				if (!(size.Width + num > columnWidth) || !(m_lcOperator as Layouter).IsFirstItemInLine)
				{
					return Math.Round(clientActiveAreaWidth, 2) != Math.Round(m_layoutArea.ClientActiveArea.Width, 2);
				}
				return true;
			}
			return true;
		}
		return false;
	}

	private bool IsFitPictureInTextBox(SizeF size, float picHeight, Entity ent)
	{
		WTextBox wTextBox = ent as WTextBox;
		Layouter layouter = m_lcOperator as Layouter;
		if ((!(picHeight <= m_layoutArea.ClientActiveArea.Height) || (size.Width > m_layoutArea.ClientActiveArea.Width && Math.Round(m_layoutArea.ClientActiveArea.Width, 2) != Math.Round(wTextBox.TextBoxFormat.InternalMargin.Top, 2)) || base.LayoutInfo == null || base.LayoutInfo.IsVerticalText) && (base.LayoutInfo == null || !base.LayoutInfo.IsVerticalText || !(size.Height <= m_layoutArea.ClientActiveArea.Width)))
		{
			if (size.Height > layouter.ClientLayoutArea.Height && base.LayoutInfo != null && !base.LayoutInfo.IsClipped)
			{
				return base.IsForceFitLayout;
			}
			return false;
		}
		return true;
	}

	private bool IsFitPictureInTable(float cellWidth, float picHeight, SizeF size, WTable table)
	{
		Layouter layouter = m_lcOperator as Layouter;
		float headerRowHeight = (((IWidget)table).LayoutInfo as TableLayoutInfo).HeaderRowHeight;
		if (picHeight > layouter.ClientLayoutArea.Height && base.IsForceFitLayout)
		{
			layouter.IsRowFitInSamePage = true;
			RectangleF area = new RectangleF(m_layoutArea.ClientActiveArea.X, m_layoutArea.ClientActiveArea.Y, m_layoutArea.ClientActiveArea.Width, size.Height);
			m_layoutArea = new LayoutArea(area);
		}
		if ((!(Math.Round(picHeight, 4) <= Math.Round(m_layoutArea.ClientActiveArea.Height, 4) + (double)((IsWord2013(table.Document) && base.IsForceFitLayout) ? headerRowHeight : 0f)) || (size.Width > m_layoutArea.ClientActiveArea.Width && Math.Round(m_layoutArea.ClientActiveArea.Width, 2) != Math.Round(cellWidth, 2)) || base.LayoutInfo == null || base.LayoutInfo.IsVerticalText) && (base.LayoutInfo == null || !base.LayoutInfo.IsVerticalText || !(size.Height <= m_layoutArea.ClientActiveArea.Width)))
		{
			if (layouter.IsFirstItemInLine)
			{
				return base.IsForceFitLayout;
			}
			return false;
		}
		return true;
	}

	private bool IsFitPictureInHeaderFooter(float picHeight, float pageHeight, SizeF size, float columnWidth, float clientActiveAreaWidth, Entity ent, double headerFooterPosition)
	{
		if (picHeight <= pageHeight || (picHeight > pageHeight && base.IsForceFitLayout))
		{
			if (!(size.Width <= m_layoutArea.ClientActiveArea.Width))
			{
				if (!(size.Width > columnWidth) || !(m_lcOperator as Layouter).IsFirstItemInLine)
				{
					return Math.Round(clientActiveAreaWidth, 2) != Math.Round(m_layoutArea.ClientActiveArea.Width, 2);
				}
				return true;
			}
			return true;
		}
		return false;
	}

	private bool IsFloatingItemFit(Entity ent, SizeF size, WParagraph ownerParagraph, ParagraphItem paraItem, float pageHeight, RectangleF rect, float floatingItemIndentY, ParagraphLayoutInfo paragraphLayoutInfo)
	{
		Layouter layouter = m_lcOperator as Layouter;
		float angle = GetAngle(paraItem);
		if (angle != 0f)
		{
			rect = base.DrawingContext.GetBoundingBoxCoordinates(new RectangleF(rect.X, rect.Y, size.Width, size.Height), angle);
			size = new SizeF(rect.Width, rect.Height);
		}
		if (ent is WSection)
		{
			if (size.Height + (IsShapePlacedOnParagraphBottom(ownerParagraph, size, paragraphLayoutInfo) ? paragraphLayoutInfo.Size.Height : 0f) <= m_layoutArea.ClientActiveArea.Height || (paraItem.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && floatingItemIndentY + size.Height <= m_layoutArea.ClientActiveArea.Bottom) || (size.Height > m_layoutArea.ClientActiveArea.Height && (layouter.IsLayoutingHeaderFooter || base.IsForceFitLayout)))
			{
				return true;
			}
		}
		else if (ent is WTable && ((size.Height <= m_layoutArea.ClientActiveArea.Height && ((paraItem is WPicture && !base.LayoutInfo.IsVerticalText) || !(paraItem is WPicture))) || (((paraItem is WPicture && base.LayoutInfo.IsVerticalText) || !(paraItem is WPicture)) && size.Height <= m_layoutArea.ClientActiveArea.Width) || (size.Height > layouter.ClientLayoutArea.Height && !base.LayoutInfo.IsClipped && base.IsForceFitLayout)))
		{
			return true;
		}
		return false;
	}

	private bool IsShapePlacedOnParagraphBottom(WParagraph ownerParagraph, SizeF size, ParagraphLayoutInfo paragraphLayoutInfo)
	{
		ParagraphItem paragraphItem = LeafWidget as ParagraphItem;
		float verticalPosition = paragraphItem.GetVerticalPosition();
		float floatingItemSpacing = GetFloatingItemSpacing(ownerParagraph);
		Layouter layouter = m_lcOperator as Layouter;
		float num = (float)Math.Round((m_lcOperator as Layouter).ParagraphYPosition, 2) + floatingItemSpacing + verticalPosition;
		if (ownerParagraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && !layouter.IsLayoutingHeaderFooter && GetAngle(paragraphItem) == 0f && !IsInFrame(ownerParagraph) && num + size.Height > layouter.ClientLayoutArea.Bottom && !(paragraphItem.GetVerticalPosition() <= 0f))
		{
			return (m_lcOperator as Layouter).ParagraphYPosition + paragraphLayoutInfo.Size.Height > layouter.ClientLayoutArea.Bottom - size.Height;
		}
		return false;
	}

	private float GetAngle(ParagraphItem paraItem)
	{
		if (paraItem is WPicture)
		{
			return (paraItem as WPicture).Rotation;
		}
		if (paraItem is Shape)
		{
			return (paraItem as Shape).Rotation;
		}
		if (paraItem is GroupShape)
		{
			return (paraItem as GroupShape).Rotation;
		}
		if (paraItem is WTextBox)
		{
			return (paraItem as WTextBox).TextBoxFormat.Rotation;
		}
		return 0f;
	}

	private float GetEmptyTextRangeHeight()
	{
		WParagraph ownerParagraph = GetOwnerParagraph();
		if (ownerParagraph != null)
		{
			float num = Math.Abs(ownerParagraph.ParagraphFormat.LineSpacing);
			float num2 = ownerParagraph.GetHeight(ownerParagraph, LeafWidget as ParagraphItem);
			if (ownerParagraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly || (ownerParagraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.AtLeast && num > num2))
			{
				return num;
			}
			return num2;
		}
		return 0f;
	}

	private Entity GetBaseEntity(ParagraphItem entity, WParagraph ownerParagraph)
	{
		bool isInCell = ownerParagraph?.IsInCell ?? false;
		Entity entity2 = entity;
		while (!(entity2 is WSection) && ((!(entity2 is WTable) && !(entity2 is WTextBox)) || !IsFitLeafWidgetInContainerHeight(entity, isInCell, entity2 is WTextBox, entity2)) && !(entity2 is HeaderFooter) && entity2.Owner != null)
		{
			entity2 = entity2.Owner;
		}
		return entity2;
	}

	private bool IsTextRangeFitInClientActiveArea(SizeF size)
	{
		bool result = false;
		WParagraph ownerParagraph = GetOwnerParagraph();
		Entity baseEntity = GetBaseEntity(ownerParagraph);
		CellLayoutInfo cellLayoutInfo = (ownerParagraph.IsInCell ? ((ownerParagraph.GetOwnerEntity() as IWidget).LayoutInfo as CellLayoutInfo) : null);
		if ((((base.LayoutInfo.IsClipped && ownerParagraph.IsInCell && cellLayoutInfo != null && cellLayoutInfo.IsRowMergeStart && cellLayoutInfo.IsRowMergeEnd) || (base.LayoutInfo.IsClipped && ownerParagraph.IsInCell && cellLayoutInfo != null && !cellLayoutInfo.IsRowMergeStart) || (base.LayoutInfo.IsClipped && !ownerParagraph.IsInCell)) && (double)size.Width <= m_layoutArea.Width) || IsTextRangeNeedToFit())
		{
			result = true;
		}
		float height = (m_lcOperator as Layouter).ClientLayoutArea.Height;
		if (!ownerParagraph.IsInCell && !(baseEntity is WTextBox) && !(baseEntity is Shape) && !(baseEntity is GroupShape) && size.Height > height && m_layoutArea.ClientActiveArea.Height > 0f)
		{
			result = true;
		}
		return result;
	}

	private bool IsTextRangeNeedToFit()
	{
		WTextRange textRange = GetTextRange(LeafWidget);
		TabsLayoutInfo tabsLayoutInfo = textRange.m_layoutInfo as TabsLayoutInfo;
		Layouter layouter = m_lcOperator as Layouter;
		float position = layouter.PreviousTab.Position;
		bool num = IsLeafWidgetIsInCell(textRange);
		float num2 = 0f;
		if (!num)
		{
			if (tabsLayoutInfo != null)
			{
				if (!(position + layouter.ClientLayoutArea.Left >= m_layoutArea.ClientActiveArea.Right) && (textRange == null || tabsLayoutInfo == null || !(tabsLayoutInfo.m_currTab.Position + layouter.ClientLayoutArea.Left >= m_layoutArea.ClientActiveArea.Right) || IsWord2013(textRange.Document)))
				{
					return layouter.ClientLayoutArea.Left >= m_layoutArea.ClientActiveArea.Right;
				}
				return true;
			}
			return false;
		}
		if (textRange != null && ((position > (num2 = base.DrawingContext.GetCellWidth(textRange)) && !IsWord2013(textRange.Document)) || (tabsLayoutInfo != null && (tabsLayoutInfo.m_currTab.Position > num2 || (tabsLayoutInfo.m_currTab.Position == 0f && tabsLayoutInfo.m_currTab.Justification == TabJustification.Left && textRange.m_layoutInfo.Size.Width > num2))) || num2 == 0f))
		{
			return !base.IsTabStopBeyondRightMarginExists;
		}
		return false;
	}

	private bool IsWordFittedByJustification(float availableLineWidth, float nextWordWidth)
	{
		if (nextWordWidth != 0f && availableLineWidth >= nextWordWidth / 2f)
		{
			Layouter layouter = m_lcOperator as Layouter;
			float num = 0f;
			float num2 = 0f;
			foreach (float lineSpaceWidth in layouter.LineSpaceWidths)
			{
				num += lineSpaceWidth;
			}
			float num3 = num / (float)layouter.LineSpaceWidths.Count;
			num2 = ((!(((availableLineWidth + num) / (float)layouter.LineSpaceWidths.Count - num3) / num3 * 100f <= 33f)) ? (num / 4f) : (num / 8f));
			if (availableLineWidth + num2 >= nextWordWidth)
			{
				layouter.m_effectiveJustifyWidth = num2;
				return true;
			}
		}
		return false;
	}

	private void FitWordAndUpdateState()
	{
		ILeafWidget leafWidget = LeafWidget;
		string nextText = GetNextText();
		string text = nextText.TrimStart();
		char c = (text.Contains(" ") ? ' ' : '-');
		WParagraph ownerParagraph = GetOwnerParagraph();
		int num = ((text.Contains(ControlChar.Space) && ownerParagraph != null && ownerParagraph.IsTextContainsNonBreakingSpaceCharacter(text) && ownerParagraph.IsNonBreakingCharacterCombinedWithSpace(text, text.IndexOf(ControlChar.SpaceChar))) ? ownerParagraph.GetsTheIndexOfSpaceToSplit(text, text.IndexOf(ControlChar.SpaceChar)) : text.IndexOf(ControlChar.SpaceChar));
		if (text.Contains(c.ToString()))
		{
			int num2 = ((c == ControlChar.SpaceChar) ? num : text.IndexOf(c)) + (nextText.Length - text.Length) + 1;
			int num3 = nextText.Length - num2;
			if (m_ltWidget != null && m_ltWidget.Widget is SplitStringWidget)
			{
				num2 += (m_ltWidget.Widget as SplitStringWidget).SplittedText.Length;
			}
			int num4 = 0;
			if (leafWidget is SplitStringWidget)
			{
				num4 = (leafWidget as SplitStringWidget).StartIndex;
			}
			WTextRange currTextRange = GetCurrTextRange();
			if (isHyphenated && m_ltWidget.Widget is SplitStringWidget)
			{
				string text2 = currTextRange.Text;
				int num5 = num4 + (m_ltWidget.Widget as SplitStringWidget).SplittedText.Length - 1;
				if (text2[num5] == '-')
				{
					currTextRange.Text = text2.Remove(num5, 1);
					num2--;
				}
			}
			if (nextText.Length == text.Length && m_sptWidget is SplitStringWidget && m_ltWidget != null && m_ltWidget.Widget is SplitStringWidget)
			{
				int num6 = (m_ltWidget.Widget as SplitStringWidget).StartIndex + (m_ltWidget.Widget as SplitStringWidget).Length - 1;
				int startIndex = (m_sptWidget as SplitStringWidget).StartIndex;
				if (startIndex - num6 > 1)
				{
					num3 += startIndex - num6 - 1;
				}
			}
			SplitStringWidget splitStringWidget = new SplitStringWidget(currTextRange, num4, num2);
			SplitStringWidget splitStringWidget2 = new SplitStringWidget(currTextRange, num4 + num2, num3);
			ForceFitWidget(splitStringWidget, splitStringWidget.Measure(base.DrawingContext));
			if (splitStringWidget2.SplittedText.Trim(' ') == "" && currTextRange.OwnerParagraph != null && currTextRange.Index == currTextRange.OwnerParagraph.ChildEntities.Count - 1)
			{
				m_sptWidget = null;
				m_ltState = LayoutState.Fitted;
			}
			else
			{
				m_sptWidget = splitStringWidget2;
				m_ltState = LayoutState.Splitted;
			}
		}
		else
		{
			ForceFitWidget(leafWidget, leafWidget.Measure(base.DrawingContext));
			m_sptWidget = null;
			m_ltState = LayoutState.Fitted;
		}
	}

	private void ForceFitWidget(IWidget widget, SizeF size)
	{
		m_ltWidget = new LayoutedWidget(widget);
		m_ltWidget.Bounds = new RectangleF(m_layoutArea.ClientArea.X, m_layoutArea.ClientArea.Y, size.Width, size.Height);
	}

	private void DoWord2013JustificationWordFit(WParagraph paragraph, float clientWidth, Layouter layouter)
	{
		if (layouter.IsWord2013WordFitLayout)
		{
			if (layouter.m_effectiveJustifyWidth > 0f && m_ltWidget != null && m_ltWidget.Bounds.Width > clientWidth)
			{
				layouter.m_effectiveJustifyWidth -= m_ltWidget.Bounds.Width - clientWidth;
			}
		}
		else
		{
			if (IsNotWord2013Jusitfy(paragraph) || GetCurrTextRange() == null)
			{
				return;
			}
			switch (m_ltState)
			{
			case LayoutState.Fitted:
				UpdateSpaceWidth(LeafWidget);
				break;
			case LayoutState.NotFitted:
			case LayoutState.Splitted:
			{
				float num = m_layoutArea.ClientActiveArea.Width;
				if (m_ltWidget != null)
				{
					UpdateSpaceWidth(m_ltWidget.Widget);
					if (m_ltWidget.Widget is SplitStringWidget && (m_ltWidget.Widget as SplitStringWidget).RealStringWidget is WTextRange && isHyphenated)
					{
						DocGen.Drawing.Font fontToRender = ((m_ltWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange).CharacterFormat.GetFontToRender(((m_ltWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange).ScriptType);
						num -= base.DrawingContext.MeasureString((m_ltWidget.Widget as SplitStringWidget).SplittedText.Trim('-'), fontToRender, null, ((m_ltWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange).CharacterFormat, isMeasureFromTabList: false, ((m_ltWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange).ScriptType).Width;
					}
					else
					{
						num -= m_ltWidget.Bounds.Width;
					}
				}
				float leadingSpaceWidth = 0f;
				float nextWordWidth = GetNextWordWidth(ref leadingSpaceWidth);
				num -= leadingSpaceWidth;
				if (IsWordFittedByJustification(num, nextWordWidth))
				{
					layouter.IsWord2013WordFitLayout = true;
					float num2 = ((m_ltWidget == null) ? 0f : m_ltWidget.Bounds.Width);
					FitWordAndUpdateState();
					num -= m_ltWidget.Bounds.Width - num2;
					if (num < 0f)
					{
						(m_lcOperator as Layouter).m_effectiveJustifyWidth += num;
					}
				}
				break;
			}
			}
		}
	}

	private string GetNextText()
	{
		string text = null;
		if (m_sptWidget is SplitStringWidget)
		{
			return (m_sptWidget as SplitStringWidget).SplittedText;
		}
		if (LeafWidget is SplitStringWidget)
		{
			return (LeafWidget as SplitStringWidget).SplittedText;
		}
		return GetCurrTextRange().Text;
	}

	private bool IsNextWordFound(string text, DocGen.Drawing.Font font, WCharacterFormat charFormat, ref float nextWordWidth, FontScriptType scriptType)
	{
		WParagraph ownerParagraph = GetOwnerParagraph();
		bool result = true;
		int startIndex = ((text.Contains(ControlChar.Space) && ownerParagraph != null && ownerParagraph.IsTextContainsNonBreakingSpaceCharacter(text) && ownerParagraph.IsNonBreakingCharacterCombinedWithSpace(text, text.IndexOf(ControlChar.SpaceChar))) ? ownerParagraph.GetsTheIndexOfSpaceToSplit(text, text.IndexOf(ControlChar.SpaceChar)) : text.IndexOf(ControlChar.SpaceChar));
		if (text.Contains(" "))
		{
			text = text.Remove(startIndex);
		}
		else if (text.Contains("-"))
		{
			text = text.Remove(text.IndexOf('-'));
		}
		else
		{
			result = false;
		}
		nextWordWidth += base.DrawingContext.MeasureString(text, font, null, charFormat, isMeasureFromTabList: false, scriptType).Width;
		return result;
	}

	private float GetNextWordWidth(ref float leadingSpaceWidth)
	{
		float nextWordWidth = 0f;
		WTextRange currTextRange = GetCurrTextRange();
		DocGen.Drawing.Font fontToRender = currTextRange.CharacterFormat.GetFontToRender(currTextRange.ScriptType);
		bool flag = true;
		string text = GetNextText();
		if (!string.IsNullOrEmpty(text))
		{
			if (text.StartsWith(" "))
			{
				leadingSpaceWidth = AddLeadingSpaces(ref text, fontToRender, currTextRange.ScriptType);
			}
			if (IsNextWordFound(text, fontToRender, currTextRange.CharacterFormat, ref nextWordWidth, currTextRange.ScriptType))
			{
				return nextWordWidth;
			}
			flag = false;
		}
		else if (currTextRange is WCheckBox || currTextRange is WDropDownFormField || currTextRange is WTextFormField)
		{
			return LeafWidget.Measure(base.DrawingContext).Width;
		}
		IEntity nextSibling = currTextRange.NextSibling;
		while (nextSibling != null)
		{
			if (nextSibling is BookmarkStart || nextSibling is BookmarkEnd || (nextSibling as IWidget).LayoutInfo.IsSkip)
			{
				nextSibling = nextSibling.NextSibling;
				continue;
			}
			if (!(nextSibling is WTextRange))
			{
				break;
			}
			currTextRange = nextSibling as WTextRange;
			fontToRender = currTextRange.CharacterFormat.GetFontToRender(currTextRange.ScriptType);
			if (flag)
			{
				text = currTextRange.Text;
				leadingSpaceWidth = AddLeadingSpaces(ref text, fontToRender, currTextRange.ScriptType);
				flag = false;
			}
			else
			{
				text = currTextRange.Text;
			}
			if (IsNextWordFound(text, fontToRender, currTextRange.CharacterFormat, ref nextWordWidth, currTextRange.ScriptType))
			{
				break;
			}
			nextSibling = nextSibling.NextSibling;
		}
		return nextWordWidth;
	}

	private float AddLeadingSpaces(ref string text, DocGen.Drawing.Font font, FontScriptType scriptType)
	{
		float width = base.DrawingContext.MeasureString(" ", font, null, scriptType).Width;
		float num = 0f;
		while (text.Length > 0 && text[0] == ' ')
		{
			text = text.Remove(0, 1);
			(m_lcOperator as Layouter).LineSpaceWidths.Add(width);
			num += width;
		}
		return num;
	}

	private void UpdateSpaceWidth(IWidget leafWidget)
	{
		string text = null;
		DocGen.Drawing.Font font = null;
		if (leafWidget is WTextRange)
		{
			WTextRange wTextRange = leafWidget as WTextRange;
			text = wTextRange.Text;
			font = wTextRange.CharacterFormat.GetFontToRender(wTextRange.ScriptType);
		}
		else if (leafWidget is SplitStringWidget)
		{
			WTextRange wTextRange2 = (leafWidget as SplitStringWidget).RealStringWidget as WTextRange;
			text = (leafWidget as SplitStringWidget).SplittedText;
			font = wTextRange2.CharacterFormat.GetFontToRender(wTextRange2.ScriptType);
		}
		Layouter layouter = m_lcOperator as Layouter;
		if (text == null || !text.Contains(" "))
		{
			return;
		}
		float width = base.DrawingContext.MeasureString(" ", font, null, FontScriptType.English).Width;
		string text2 = text;
		for (int i = 0; i < text2.Length; i++)
		{
			if (text2[i] == ' ')
			{
				layouter.LineSpaceWidths.Add(width);
			}
		}
	}

	internal LayoutedWidget WordLayout(RectangleF rect, SizeF size, WTextRange textRange, WParagraph paragraph)
	{
		Layouter layouter = m_lcOperator as Layouter;
		float num = (base.IsTabStopBeyondRightMarginExists ? 1584f : textRange.GetClientWidth(base.DrawingContext, layouter.ClientLayoutArea.Width));
		string text = GetText();
		if (layouter.m_canSplitbyCharacter && (text.Contains(" ") || text.Contains('-'.ToString()) || text.Contains('\u001f'.ToString()) || layouter.m_canSplitByTab || IsNeedToSkipSplitTextByCharacter(textRange, paragraph)))
		{
			layouter.m_canSplitbyCharacter = false;
		}
		IWidget nextSibling = GetNextSibling(textRange);
		if (layouter.m_canSplitbyCharacter && !layouter.m_canSplitByTab && textRange.m_layoutInfo is TabsLayoutInfo)
		{
			layouter.m_canSplitByTab = true;
			if (!layouter.IsFirstItemInLine)
			{
				layouter.m_canSplitbyCharacter = false;
			}
			else
			{
				layouter.m_canSplitByTab = false;
			}
		}
		if (size.Width > num)
		{
			if (!layouter.m_canSplitbyCharacter && !base.DrawingContext.IsUnicodeText(GetText()) && !text.Contains(" ") && !text.Contains('-'.ToString()) && !text.Contains('\u001f'.ToString()))
			{
				ISplitLeafWidget splitLeafWidget = LeafWidget as ISplitLeafWidget;
				SplitByWord(splitLeafWidget, size, textRange, num, isWrapTextBasedOnAbsTable: false);
				DoLayoutAfter();
				return m_ltWidget;
			}
			return null;
		}
		if (textRange != null && textRange.CharacterRange == CharacterRangeType.RTL && paragraph.ParagraphFormat.Bidi && !textRange.CharacterFormat.Bidi && nextSibling is WTextRange && (nextSibling as WTextRange).Text != string.Empty && (nextSibling as WTextRange).Text[0] == ControlChar.SpaceChar && !layouter.IsFirstItemInLine && TryFit(size))
		{
			WTextRange wTextRange = nextSibling as WTextRange;
			SplitStringWidget splitStringWidget = ((LeafWidget is SplitStringWidget) ? (LeafWidget as SplitStringWidget) : null);
			string text2 = ((splitStringWidget != null) ? splitStringWidget.SplittedText : textRange.Text);
			string text3 = new string(text2.ToCharArray());
			bool flag = false;
			IWidget widget = wTextRange;
			float nextsiblingWidth = 0f;
			if (wTextRange.Text.Trim() == string.Empty)
			{
				nextsiblingWidth = (nextSibling as ILeafWidget).Measure(base.DrawingContext).Width;
				while (widget is WTextRange)
				{
					WTextRange wTextRange2 = widget as WTextRange;
					if (wTextRange2.Text.Trim() == string.Empty)
					{
						nextsiblingWidth += (nextSibling as ILeafWidget).Measure(base.DrawingContext).Width;
						widget = GetNextSibling(widget as WTextRange);
						continue;
					}
					if (wTextRange2.Text[0] == ControlChar.SpaceChar)
					{
						UpdateSpaceWidth(wTextRange2, ref nextsiblingWidth);
					}
					break;
				}
			}
			else
			{
				UpdateSpaceWidth(wTextRange, ref nextsiblingWidth);
			}
			WTextRange wTextRange3 = widget as WTextRange;
			int num2 = 0;
			while (wTextRange3 != null && num2 < wTextRange3.TextLength)
			{
				if (TextSplitter.IsRTLChar(wTextRange3.Text[num2]))
				{
					flag = true;
				}
				else if (wTextRange3.Text[num2] != ControlChar.SpaceChar)
				{
					break;
				}
				num2++;
			}
			if (size.Width + nextsiblingWidth > m_layoutArea.ClientActiveArea.Width && flag)
			{
				bool flag2 = true;
				for (int num3 = text3.Length - 1; num3 >= 0; num3--)
				{
					if (text3[num3] != ControlChar.SpaceChar)
					{
						flag2 = false;
						text3 = text3.Remove(num3);
					}
					else
					{
						if (!flag2)
						{
							break;
						}
						text3 = text3.Remove(num3);
					}
				}
				string text4 = text2.Substring(text3.Length);
				ISplitLeafWidget[] array = new ISplitLeafWidget[2]
				{
					new SplitStringWidget(LeafWidget as IStringWidget, -1, -1),
					new SplitStringWidget(LeafWidget as IStringWidget, 0, textRange.TextLength)
				};
				if (text3 != string.Empty && text4 != string.Empty)
				{
					array[0] = new SplitStringWidget(LeafWidget as IStringWidget, 0, text3.Length);
					array[1] = new SplitStringWidget(LeafWidget as IStringWidget, text3.Length, text4.Length);
				}
				size = array[0].Measure(base.DrawingContext);
				FitWidget(size, array[0], isLastWordFit: false, 0f, 0f, isFloatingItem: false);
				m_sptWidget = array[1];
				m_ltState = LayoutState.Splitted;
				return m_ltWidget;
			}
		}
		TabsLayoutInfo tabsLayoutInfo = null;
		if (nextSibling != null)
		{
			tabsLayoutInfo = nextSibling.LayoutInfo as TabsLayoutInfo;
		}
		if (size.Width > 0f || (size.Width == 0f && textRange.m_layoutInfo is TabsLayoutInfo && (textRange.m_layoutInfo as TabsLayoutInfo).CurrTabJustification == TabJustification.Right && tabsLayoutInfo == null))
		{
			if (IsTextRangeNeedToFit())
			{
				return null;
			}
			if (base.DrawingContext.IsUnicodeText(GetText()))
			{
				SplitUnicodeTextByWord(textRange, rect, size, num);
				DoLayoutAfter();
				return m_ltWidget;
			}
			if (IsTextNeedToBeSplitted(size, rect, textRange) && IsTextNeedToBeSplittedByWord(size, rect, textRange, num))
			{
				ISplitLeafWidget splitLeafWidget2 = LeafWidget as ISplitLeafWidget;
				SplitByWord(splitLeafWidget2, size, textRange, num, isWrapTextBasedOnAbsTable: false);
				DoLayoutAfter();
				return m_ltWidget;
			}
		}
		else if (tabsLayoutInfo != null && size.Width < rect.Width)
		{
			tabsLayoutInfo.GetNextTabPosition(m_layoutArea.ClientArea.X);
			TabsLayoutInfo tabsLayoutInfo2 = ((textRange.m_layoutInfo is TabsLayoutInfo) ? (textRange.m_layoutInfo as TabsLayoutInfo) : null);
			if (tabsLayoutInfo.m_currTab.Position > base.ClientLayoutAreaRight)
			{
				if (IsTextRangeNeedToFit())
				{
					return null;
				}
				if (IsTextNeedToBeSplitted(size, rect, textRange) && IsTextNeedToBeSplittedByWord(size, new RectangleF(rect.X, rect.Y, 1584f - rect.Width, rect.Height), nextSibling, 1584f))
				{
					ISplitLeafWidget splitLeafWidget3 = LeafWidget as ISplitLeafWidget;
					SplitByWord(splitLeafWidget3, size, textRange, m_layoutArea.ClientArea.Width, isWrapTextBasedOnAbsTable: false);
					DoLayoutAfter();
					if (m_ltWidget.Widget is SplitStringWidget && !(m_ltWidget.Widget as SplitStringWidget).SplittedText.Equals(""))
					{
						return m_ltWidget;
					}
				}
				else if (layouter.m_canSplitByTab && textRange != null && tabsLayoutInfo2 != null && tabsLayoutInfo2.m_currTab.Position + layouter.ClientLayoutArea.Left > m_layoutArea.ClientActiveArea.Right && IsWord2013(textRange.Document))
				{
					ISplitLeafWidget splitLeafWidget4 = LeafWidget as ISplitLeafWidget;
					SplitByWord(splitLeafWidget4, size, textRange, num, isWrapTextBasedOnAbsTable: false);
					DoLayoutAfter();
					return m_ltWidget;
				}
			}
		}
		return m_ltWidget = null;
	}

	private bool IsNeedToSkipSplitTextByCharacter(WTextRange textRange, WParagraph paragraph)
	{
		WTextRange wTextRange = ((textRange != null && paragraph != null) ? (paragraph.GetPreviousInlineItems(textRange) as WTextRange) : null);
		if (!(m_lcOperator as Layouter).IsFirstItemInLine && paragraph != null && (paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 || paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2010) && wTextRange != null && (wTextRange.ScriptType == FontScriptType.Chinese || wTextRange.ScriptType == FontScriptType.Japanese) && wTextRange.ScriptType != textRange.ScriptType)
		{
			return true;
		}
		return false;
	}

	private void UpdateSpaceWidth(WTextRange nextText, ref float nextsiblingWidth)
	{
		string[] array = nextText.Text.Split(' ');
		for (int i = 0; i < array.Length && array[i] == string.Empty; i++)
		{
			nextsiblingWidth += base.DrawingContext.MeasureTextRange(nextText, " ").Width;
		}
	}

	private void GetDefaultBorderSpacingValue(ref float border, ref bool isBorderValueZero, bool isWord2013, HorizontalPosition tableHorizontalPosition)
	{
		if (border == 0f)
		{
			if (!isWord2013 && tableHorizontalPosition == HorizontalPosition.Center)
			{
				border = 1.5f;
			}
			else
			{
				border = 0.75f;
			}
			isBorderValueZero = true;
		}
	}

	private void SplitUnicodeTextByWord(WTextRange textRange, RectangleF rect, SizeF size, float clientWidth)
	{
		if (!(GetNextSibling(textRange) is WTextRange wTextRange) || wTextRange.Text.Length <= 0 || !base.DrawingContext.IsBeginCharacter(wTextRange.Text[0]))
		{
			return;
		}
		float width = base.DrawingContext.MeasureTextRange(wTextRange, wTextRange.Text[0].ToString()).Width;
		string text = GetText();
		int index = text.Length;
		if (IsBeginCJKCharacter(text, ref index) && index > 0 && size.Width + width > rect.Width && IsUnicodeTextNeedToBeSplittedByWord(size, rect, textRange, clientWidth))
		{
			ISplitLeafWidget[] array = new ISplitLeafWidget[2]
			{
				new SplitStringWidget(textRange, 0, index - 1),
				new SplitStringWidget(textRange, index - 1, -1)
			};
			m_ltState = LayoutState.NotFitted;
			size = array[0].Measure(base.DrawingContext);
			if (!TryFit(size))
			{
				size.Width = m_layoutArea.ClientArea.Width;
			}
			FitWidget(size, array[0], isLastWordFit: false, 0f, 0f, isFloatingItem: false);
			m_sptWidget = array[1];
			m_ltState = LayoutState.Splitted;
		}
	}

	private bool IsUnicodeTextNeedToBeSplittedByWord(SizeF size, RectangleF rect, WTextRange textRange, float clientWidth)
	{
		bool result = false;
		if (GetNextSibling(textRange) is WTextRange wTextRange && size.Width < rect.Width)
		{
			string nextSiblingText = wTextRange.Text;
			float unicodeNextTextRangeWidth = GetUnicodeNextTextRangeWidth(wTextRange, ref nextSiblingText, size, rect);
			if (size.Width + unicodeNextTextRangeWidth > rect.Width && clientWidth >= rect.Width && unicodeNextTextRangeWidth < clientWidth)
			{
				result = true;
			}
		}
		return result;
	}

	private string GetText()
	{
		if (LeafWidget is WField)
		{
			WField wField = LeafWidget as WField;
			if (wField.FieldType == FieldType.FieldExpression || wField.FieldType == FieldType.FieldPageRef || wField.FieldType == FieldType.FieldRef)
			{
				return string.Empty;
			}
			if (LeafWidget.Measure(base.DrawingContext) == Size.Empty)
			{
				return string.Empty;
			}
		}
		if (!(LeafWidget is WTextRange))
		{
			return (LeafWidget as SplitStringWidget).SplittedText;
		}
		return (LeafWidget as WTextRange).Text;
	}

	private bool IsTextNeedToBeSplittedByWord(SizeF size, RectangleF rect, IWidget iwidget, float clientWidth)
	{
		WTextRange widget = ((iwidget is WTextRange) ? (iwidget as WTextRange) : null);
		bool result = false;
		IWidget nextSibling = GetNextSibling(widget);
		WTextRange wTextRange = nextSibling as WTextRange;
		if (((wTextRange != null && !StartsWithExt(wTextRange.Text, " ") && !GetText().EndsWith(" ") && !(((IWidget)wTextRange).LayoutInfo is TabsLayoutInfo)) || (nextSibling != null && nextSibling.LayoutInfo is FootnoteLayoutInfo) || nextSibling is WSymbol) && !(size.Width > rect.Width))
		{
			string text = null;
			text = ((nextSibling is WFootnote) ? (nextSibling.LayoutInfo as FootnoteLayoutInfo).FootnoteID : ((!(nextSibling is WSymbol)) ? wTextRange.Text : char.ConvertFromUtf32((nextSibling as WSymbol).CharacterCode)));
			float nextTextRangeWidth = GetNextTextRangeWidth(nextSibling, ref text, size, rect);
			if (!base.DrawingContext.IsUnicodeText(text) && !(m_lcOperator as Layouter).m_canSplitbyCharacter && size.Width + nextTextRangeWidth > rect.Width && Math.Round(clientWidth, 2) >= Math.Round(rect.Width, 2))
			{
				result = true;
			}
		}
		return result;
	}

	private float GetWidthToFitText(WTextRange textRange, float nextTextRangeWidth)
	{
		string text = "";
		string[] array = GetText().Split(' ');
		if (array.Length == 1)
		{
			if (textRange.Text != array[0])
			{
				return base.DrawingContext.MeasureTextRange(textRange, array[0]).Width + nextTextRangeWidth;
			}
			return ((IWidget)textRange).LayoutInfo.Size.Width + nextTextRangeWidth;
		}
		for (int i = 0; i < array.Length - 1; i++)
		{
			text = text + array[i] + " ";
		}
		string[] array2 = array[^1].Split('-');
		for (int j = 0; j < array2.Length - 1; j++)
		{
			text = text + array2[j] + '-';
		}
		if (textRange.Text != text)
		{
			return base.DrawingContext.MeasureTextRange(textRange, text).Width;
		}
		return ((IWidget)textRange).LayoutInfo.Size.Width;
	}

	private float GetNextTextRangeWidth(IWidget nextSiblingTextRange, ref string nextSiblingText, SizeF size, RectangleF rect)
	{
		if (nextSiblingTextRange is WFootnote)
		{
			_ = (nextSiblingTextRange.LayoutInfo as LayoutFootnoteInfoImpl).TextRange;
		}
		SizeF sizeNext = default(SizeF);
		bool flag = IsNextSibligSizeNeedToBeMeasure(ref sizeNext, nextSiblingTextRange, rect, size) && !StringParser.IsUnicodeChineseText(nextSiblingText);
		while (flag && (nextSiblingTextRange = GetNextSibling(nextSiblingTextRange)) != null && (nextSiblingTextRange is WTextRange || nextSiblingTextRange is WSymbol || nextSiblingTextRange is WFootnote) && (size + sizeNext).Width < rect.Width && IsNextSibligSizeNeedToBeMeasure(ref sizeNext, nextSiblingTextRange, rect, size))
		{
			if (nextSiblingTextRange is WSymbol)
			{
				nextSiblingText += char.ConvertFromUtf32((nextSiblingTextRange as WSymbol).CharacterCode);
			}
			else if (nextSiblingTextRange is WFootnote)
			{
				nextSiblingText += (nextSiblingTextRange.LayoutInfo as LayoutFootnoteInfoImpl).FootnoteID;
			}
			else
			{
				nextSiblingText += (nextSiblingTextRange as WTextRange).Text;
			}
		}
		return sizeNext.Width;
	}

	private float GetUnicodeNextTextRangeWidth(WTextRange nextSiblingTextRange, ref string nextSiblingText, SizeF size, RectangleF rect)
	{
		SizeF sizeNext = default(SizeF);
		bool flag = IsUnicodeNextSibligSizeNeedToBeMeasure(ref sizeNext, nextSiblingTextRange, rect, size);
		while (flag && IsLeafWidgetNextSiblingIsTextRange(nextSiblingTextRange) && (size + sizeNext).Width < rect.Width)
		{
			nextSiblingTextRange = GetNextSibling(nextSiblingTextRange) as WTextRange;
			if (!IsUnicodeNextSibligSizeNeedToBeMeasure(ref sizeNext, nextSiblingTextRange, rect, size))
			{
				break;
			}
			nextSiblingText += nextSiblingTextRange.Text;
		}
		return sizeNext.Width;
	}

	private bool IsNextSibligSizeNeedToBeMeasure(ref SizeF sizeNext, IWidget nextSiblingwidget, RectangleF rect, SizeF size)
	{
		string text = null;
		WParagraph ownerParagraph = GetOwnerParagraph();
		WTextRange wTextRange = nextSiblingwidget as WTextRange;
		if (nextSiblingwidget is WFootnote)
		{
			wTextRange = (nextSiblingwidget.LayoutInfo as LayoutFootnoteInfoImpl).TextRange;
			text = (nextSiblingwidget.LayoutInfo as LayoutFootnoteInfoImpl).FootnoteID;
		}
		else if (nextSiblingwidget is WSymbol)
		{
			sizeNext += (nextSiblingwidget as ILeafWidget).Measure(base.DrawingContext);
		}
		else
		{
			text = wTextRange.Text;
		}
		if (wTextRange != null)
		{
			int num = text.IndexOf(ControlChar.SpaceChar);
			if (text.Contains(" ") || (text.Contains('-'.ToString()) && (ownerParagraph == null || IsWord2013(ownerParagraph.Document) || ownerParagraph.Document.CharacterSpacingControl == CharacterSpacingControl.DoNotCompress)) || (text.Contains('\u001f'.ToString()) && size.Width + sizeNext.Width + base.DrawingContext.MeasureString("-", nextSiblingwidget.LayoutInfo.Font.GetFont(wTextRange.Document, FontScriptType.English), new StringFormat(base.DrawingContext.StringFormt), FontScriptType.English).Width < rect.Width) || ((IWidget)wTextRange).LayoutInfo is TabsLayoutInfo)
			{
				float width = ((IWidget)wTextRange).LayoutInfo.Size.Width;
				if (text.Contains(ControlChar.Space) && ownerParagraph != null && ownerParagraph.IsTextContainsNonBreakingSpaceCharacter(text) && ownerParagraph.IsNonBreakingCharacterCombinedWithSpace(text, num))
				{
					num = ownerParagraph.GetsTheIndexOfSpaceToSplit(text, num);
					width = base.DrawingContext.MeasureTextRange(wTextRange, text.Substring(0, num)).Width;
				}
				else if (text != text.Split(' ')[0])
				{
					width = base.DrawingContext.MeasureTextRange(wTextRange, text.Split(' ')[0]).Width;
				}
				if (size.Width + sizeNext.Width + width > rect.Width && text.Contains('-'.ToString()) && text != text.Split('-')[0] + '-')
				{
					width = base.DrawingContext.MeasureTextRange(wTextRange, text.Split('-')[0] + '-').Width;
				}
				sizeNext.Width += width;
				return false;
			}
			sizeNext += nextSiblingwidget.LayoutInfo.Size;
		}
		return true;
	}

	private bool IsUnicodeNextSibligSizeNeedToBeMeasure(ref SizeF sizeNext, WTextRange nextSiblingTextRange, RectangleF rect, SizeF size)
	{
		int index = 0;
		if (IsBeginCJKCharacter(nextSiblingTextRange.Text, ref index))
		{
			float width = ((IWidget)nextSiblingTextRange).LayoutInfo.Size.Width;
			string text = string.Empty;
			for (int i = index; i < nextSiblingTextRange.Text.Length && base.DrawingContext.IsBeginCharacter(nextSiblingTextRange.Text[i]); i++)
			{
				text += nextSiblingTextRange.Text[i];
			}
			width = base.DrawingContext.MeasureTextRange(nextSiblingTextRange, text).Width;
			sizeNext.Width += width;
			if (text == nextSiblingTextRange.Text)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private bool IsBeginCJKCharacter(string text, ref int index)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (base.DrawingContext.IsBeginCharacter(text[i]))
			{
				index = i;
				return true;
			}
		}
		return false;
	}

	private IWidget GetValidInlineNextSibling(ParagraphItem paragraphItem)
	{
		IEntity entity = paragraphItem.NextSibling;
		if (entity is WOleObject)
		{
			entity = (entity as WOleObject).OlePicture;
		}
		while (entity != null && ((entity as IWidget).LayoutInfo.IsSkip || (entity as Entity).IsFloatingItem(isTextWrapAround: false) || entity is Break || entity is InlineContentControl || entity is BookmarkStart || entity is BookmarkEnd))
		{
			entity = entity.NextSibling;
		}
		return entity as IWidget;
	}

	private IWidget GetNextSibling(IWidget widget)
	{
		WParagraph ownerParagraph = GetOwnerParagraph();
		IWidget nextSibling = ownerParagraph.GetNextSibling(widget);
		while (nextSibling != null && (!(nextSibling is WTextRange) || (nextSibling is WField && (nextSibling as WField).FieldType == FieldType.FieldHyperlink) || nextSibling.LayoutInfo.IsSkip))
		{
			if (!(nextSibling is WFootnote) && (nextSibling is BookmarkStart || nextSibling is BookmarkEnd || nextSibling is WFieldMark || nextSibling is SplitStringWidget || nextSibling.LayoutInfo.IsSkip || (nextSibling is WField && (nextSibling as WField).FieldType == FieldType.FieldHyperlink) || ((nextSibling is WPicture || nextSibling is Shape || nextSibling is WTextBox || nextSibling is GroupShape || nextSibling is WChart) && (nextSibling as Entity).IsFloatingItem(isTextWrapAround: false))))
			{
				nextSibling = ownerParagraph.GetNextSibling(nextSibling);
				continue;
			}
			return nextSibling;
		}
		return nextSibling;
	}

	internal bool IsLeafWidgetIsInCell(ParagraphItem paraItem)
	{
		bool result = false;
		WParagraph wParagraph = null;
		if (paraItem != null)
		{
			wParagraph = paraItem.GetOwnerParagraphValue();
		}
		if (wParagraph != null && wParagraph.IsInCell)
		{
			result = true;
		}
		return result;
	}

	internal bool IsLeafWidgetIsInTextBox(ParagraphItem paraItem)
	{
		bool result = false;
		WParagraph wParagraph = null;
		if (paraItem != null)
		{
			wParagraph = paraItem.GetOwnerParagraphValue();
		}
		if (wParagraph != null && wParagraph.OwnerBase != null && wParagraph.OwnerBase.OwnerBase is WTextBox)
		{
			result = true;
		}
		return result;
	}

	internal bool IsLeafWidgetNextSiblingIsTextRange(WTextRange textRange)
	{
		if (GetNextSibling(textRange) is WTextRange)
		{
			return true;
		}
		return false;
	}

	internal void SplitByWord(ISplitLeafWidget splitLeafWidget, SizeF size, WTextRange textRange, float clientWidth, bool isWrapTextBasedOnAbsTable)
	{
		if (splitLeafWidget != null && size.Height <= m_layoutArea.ClientArea.Height)
		{
			ISplitLeafWidget[] array = null;
			if (base.LayoutInfo is TabsLayoutInfo)
			{
				m_ltWidget = new LayoutedWidget(splitLeafWidget);
				if (((TabsLayoutInfo)base.LayoutInfo).CurrTabLeader == TabLeader.NoLeader)
				{
					m_ltWidget.Bounds = new RectangleF(m_layoutArea.ClientArea.X, m_layoutArea.ClientArea.Y, size.Width, size.Height);
				}
				m_sptWidget = splitLeafWidget;
				m_ltState = LayoutState.Splitted;
				return;
			}
			SplitStringWidget splitStringWidget = LeafWidget as SplitStringWidget;
			WTextRange wTextRange = LeafWidget as WTextRange;
			string text = "";
			string[] array2 = GetText().Split(' ');
			int num = -1;
			int num2 = -1;
			ISplitLeafWidget[] array3 = new ISplitLeafWidget[2];
			if (array2.Length == 1 && size.Width > clientWidth && (m_lcOperator as Layouter).m_canSplitbyCharacter)
			{
				if (wTextRange != null)
				{
					num = 0;
					num2 = wTextRange.Text.IndexOf(' ');
				}
				else
				{
					num = splitStringWidget.StartIndex;
					num2 = splitStringWidget.SplittedText.IndexOf(' ');
				}
				array3[0] = new SplitStringWidget(textRange, num, num2);
				array3[1] = new SplitStringWidget(textRange, -1, -1);
			}
			else if (array2[^1].EndsWith('-'.ToString()) || array2[^1].EndsWith('\u001f'.ToString(), StringComparison.Ordinal))
			{
				if (wTextRange != null)
				{
					num = 0;
					if (wTextRange.Text == null)
					{
						num = (num2 = int.MinValue);
					}
					else
					{
						num2 = wTextRange.Text.Length;
					}
				}
				else
				{
					num = splitStringWidget.StartIndex;
					if (splitStringWidget.SplittedText == null)
					{
						num = (num2 = int.MinValue);
					}
					else
					{
						num2 = splitStringWidget.SplittedText.Length;
					}
				}
				array3[0] = new SplitStringWidget(textRange, num, num2);
				array3[1] = new SplitStringWidget(textRange, -1, -1);
			}
			else
			{
				for (int i = 0; i < array2.Length - 1; i++)
				{
					text = text + array2[i] + " ";
				}
				string text2 = (array2[^1].Contains('\u001f'.ToString()) ? '\u001f'.ToString() : '-'.ToString());
				string[] array4 = array2[^1].Split(text2[0]);
				for (int j = 0; j < array4.Length - 1; j++)
				{
					text = text + array4[j] + text2;
				}
				if (isWrapTextBasedOnAbsTable)
				{
					if (wTextRange != null)
					{
						num = 0;
						if (wTextRange.Text == null)
						{
							num = (num2 = int.MinValue);
						}
						else
						{
							num2 = wTextRange.Text.Length;
						}
					}
					else
					{
						num = splitStringWidget.StartIndex;
						if (splitStringWidget.SplittedText == null)
						{
							num = (num2 = int.MinValue);
						}
						else
						{
							num2 = splitStringWidget.SplittedText.Length;
						}
					}
					array3[0] = new SplitStringWidget(textRange, -1, -1);
					array3[1] = new SplitStringWidget(textRange, num, num2);
				}
				else
				{
					if (wTextRange != null)
					{
						num = 0;
						num2 = text.Length;
					}
					else
					{
						num = splitStringWidget.StartIndex;
						num2 = text.Length;
					}
					array3[0] = new SplitStringWidget(textRange, num, num2);
					if (wTextRange != null)
					{
						num = text.Length;
						num2 = array4[^1].Length;
					}
					else
					{
						num = splitStringWidget.StartIndex + text.Length;
						num2 = array4[^1].Length;
					}
					array3[1] = new SplitStringWidget(textRange, num, num2);
				}
			}
			array = array3;
			m_ltState = LayoutState.NotFitted;
			if (array != null)
			{
				size = array[0].Measure(base.DrawingContext);
				if (!TryFit(size))
				{
					size.Width = m_layoutArea.ClientArea.Width;
				}
				FitWidget(size, array[0], isLastWordFit: false, 0f, 0f, isFloatingItem: false);
				m_sptWidget = array[1];
				m_ltState = LayoutState.Splitted;
			}
		}
		else
		{
			m_ltState = LayoutState.NotFitted;
			base.IsVerticalNotFitted = size.Height > m_layoutArea.ClientArea.Height;
		}
	}

	internal bool IsTextNeedToBeSplitted(SizeF size, RectangleF rect, WTextRange textRange)
	{
		WTextRange prevSiblingTextRange = base.DrawingContext.GetPreviousSibling(textRange) as WTextRange;
		IWidget nextSibling = GetNextSibling(textRange);
		WParagraph ownerParagraph = GetOwnerParagraph();
		WTextRange wTextRange = nextSibling as WTextRange;
		if (nextSibling is WFootnote)
		{
			wTextRange = (nextSibling.LayoutInfo as LayoutFootnoteInfoImpl).TextRange;
		}
		bool result = IsTextNeedToBeSplitted(prevSiblingTextRange, wTextRange);
		if (wTextRange != null && wTextRange.Text.Contains(" "))
		{
			int num = wTextRange.Text.IndexOf(" ");
			if (num != -1)
			{
				num = ((ownerParagraph != null && ownerParagraph.IsTextContainsNonBreakingSpaceCharacter(wTextRange.Text) && ownerParagraph.IsNonBreakingCharacterCombinedWithSpace(wTextRange.Text, num)) ? ownerParagraph.GetsTheIndexOfSpaceToSplit(wTextRange.Text, num) : num);
				SizeF sizeF = base.DrawingContext.MeasureTextRange(wTextRange, wTextRange.Text.Substring(0, num + 1));
				result = ((!(size.Width > rect.Width) && (size + sizeF).Width > rect.Width) ? true : false);
			}
		}
		return result;
	}

	internal bool IsTextNeedToBeSplitted(WTextRange prevSiblingTextRange, WTextRange nextSiblingTextRange)
	{
		bool result = true;
		string text = GetText();
		if (text.EndsWith(" ") || text.EndsWith(",") || (nextSiblingTextRange != null && StartsWithExt(nextSiblingTextRange.Text, " ")) || (prevSiblingTextRange != null && prevSiblingTextRange.Text.EndsWith("") && StartsWithExt(text, "") && text.EndsWith("") && (nextSiblingTextRange == null || !StartsWithExt(nextSiblingTextRange.Text, "") || !nextSiblingTextRange.Text.EndsWith(""))))
		{
			result = false;
		}
		return result;
	}

	private WParagraph GetOwnerParagraph()
	{
		ParagraphItem paragraphItem = ((LeafWidget is SplitStringWidget) ? ((LeafWidget as SplitStringWidget).RealStringWidget as WTextRange) : ((LeafWidget is ParagraphItem) ? (LeafWidget as ParagraphItem) : null));
		if (paragraphItem is WTextRange && (paragraphItem as WTextRange).Owner == null)
		{
			return (paragraphItem as WTextRange).CharacterFormat.BaseFormat.OwnerBase as WParagraph;
		}
		return paragraphItem?.GetOwnerParagraphValue();
	}

	private bool CheckWrappingStyleForWrapping(TextWrappingStyle textWrappingStyle)
	{
		if (textWrappingStyle != TextWrappingStyle.InFrontOfText)
		{
			return textWrappingStyle == TextWrappingStyle.Behind;
		}
		return true;
	}

	private bool IsNeedToWrapLeafWidget(WParagraph ownerPara, Layouter layouter)
	{
		if (layouter.FloatingItems.Count > 0 && base.IsNeedToWrap && ownerPara != null && (!layouter.IsLayoutingHeaderFooter || ownerPara.IsInCell || ownerPara.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) && ((!(LeafWidget is WPicture) && !(LeafWidget is WChart) && !(LeafWidget is Shape) && !(LeafWidget is WTextBox) && !(LeafWidget is GroupShape)) || !CheckWrappingStyleForWrapping((LeafWidget as ParagraphItem).GetTextWrappingStyle())) && !IsLeafWidgetOwnerIsTextBox())
		{
			if (!IsInFrame(ownerPara))
			{
				return !IsInFootnote(ownerPara);
			}
			return false;
		}
		return false;
	}

	private bool IsNeedToWrapParaMarkToRightSide(WParagraph ownerPara, RectangleF textWrappingBounds, float bottom, Layouter layouter, RectangleF rect, TextWrappingType textWrappingType, TextWrappingStyle textWrappingStyle, float minimumWidthRequired)
	{
		if (ownerPara != null && !ownerPara.IsInCell && textWrappingBounds.Right < layouter.ClientLayoutArea.Right - minimumWidthRequired && LeafWidget is Break && ((LeafWidget as Break).BreakType == BreakType.LineBreak || (LeafWidget as Break).BreakType == BreakType.TextWrappingBreak) && textWrappingType == TextWrappingType.Both && textWrappingBounds.X > rect.X && ((textWrappingBounds.Y <= rect.Y && textWrappingBounds.Bottom >= rect.Y) || (textWrappingBounds.Y <= bottom && textWrappingBounds.Y >= rect.Y) || (textWrappingBounds.Bottom >= rect.Y && textWrappingBounds.Bottom <= bottom)))
		{
			if (textWrappingStyle != TextWrappingStyle.Square && textWrappingStyle != TextWrappingStyle.Tight)
			{
				return textWrappingStyle == TextWrappingStyle.Through;
			}
			return true;
		}
		return false;
	}

	private bool IsNeedToWrapForSquareTightAndThrough(Layouter layouter, int wrapOwnerIndex, int wrapItemIndex, TextWrappingStyle textWrappingStyle, RectangleF textWrappingBounds, bool allowOverlap, int wrapCollectionIndex, Entity floatingEntity, bool isTextRangeInTextBox, RectangleF rect, SizeF size, bool isWithInTextBox)
	{
		if (layouter.FloatingItems.Count > 0 && wrapOwnerIndex != wrapCollectionIndex && wrapItemIndex != wrapCollectionIndex && textWrappingStyle != 0 && textWrappingStyle != TextWrappingStyle.TopAndBottom && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind && ((Math.Round(rect.Y + size.Height, 2) > Math.Round(textWrappingBounds.Y, 2) && IsParaSpacingExceed(Math.Round(size.Height, 2), Math.Round(textWrappingBounds.Y, 2), rect)) || IsTextFitBelow(textWrappingBounds, rect.Y + size.Height, floatingEntity)) && Math.Round(rect.Y, 2) < Math.Round(textWrappingBounds.Bottom, 2))
		{
			if (!isWithInTextBox)
			{
				if (allowOverlap)
				{
					if (!isTextRangeInTextBox)
					{
						if ((LeafWidget is WPicture || LeafWidget is Shape || LeafWidget is WChart || LeafWidget is WTextBox || LeafWidget is GroupShape) && (LeafWidget as ParagraphItem).GetTextWrappingStyle() != 0)
						{
							return !(LeafWidget as ParagraphItem).GetAllowOverlap();
						}
						return true;
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private bool IsParaSpacingExceed(double textHeight, double floatingItemYpos, RectangleF rect)
	{
		WParagraphFormat wParagraphFormat = GetOwnerParagraph()?.ParagraphFormat;
		if (wParagraphFormat != null && wParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly && Math.Round(wParagraphFormat.LineSpacing, 2) > 0.0 && Math.Round(wParagraphFormat.LineSpacing, 2) < textHeight)
		{
			if (Math.Round(rect.Y + wParagraphFormat.LineSpacing, 2) > Math.Round(floatingItemYpos, 2))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	private bool IsNeedToWrapForTopAndBottom(WParagraph currWidgetOwnerPara, Layouter layouter, int wrapOwnerIndex, int wrapItemIndex, TextWrappingStyle textWrappingStyle, RectangleF textWrappingBounds, bool allowOverlap, int wrapCollectionIndex, Entity floatingEntity, bool isTextRangeInTextBox, RectangleF rect, SizeF size)
	{
		if (currWidgetOwnerPara.IsInCell && textWrappingStyle == TextWrappingStyle.TopAndBottom && !(floatingEntity is WTable))
		{
			WParagraph wParagraph = ((floatingEntity != null && floatingEntity.Owner is WParagraph) ? (floatingEntity.Owner as WParagraph) : (floatingEntity as WParagraph));
			if (wParagraph != null && wParagraph.IsInCell)
			{
				bool layOutInCell = (floatingEntity as ParagraphItem).GetLayOutInCell();
				WTableCell wTableCell = wParagraph.GetOwnerEntity() as WTableCell;
				WTableCell wTableCell2 = currWidgetOwnerPara.GetOwnerEntity() as WTableCell;
				if ((wTableCell != null && wTableCell2 != null && wTableCell != wTableCell2) || (!layOutInCell && currWidgetOwnerPara.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013))
				{
					return false;
				}
			}
		}
		float num = m_layoutArea.ClientActiveArea.Right - textWrappingBounds.Right;
		WParagraph ownerParagraph = GetOwnerParagraph();
		Entity entity = null;
		if (ownerParagraph != null)
		{
			entity = ownerParagraph.GetOwnerShape(ownerParagraph);
		}
		if (layouter.FloatingItems.Count > 0 && wrapOwnerIndex != wrapCollectionIndex && wrapItemIndex != wrapCollectionIndex && IsFrameInClientArea(floatingEntity as WParagraph, textWrappingBounds) && textWrappingStyle == TextWrappingStyle.TopAndBottom && ((rect.Y >= textWrappingBounds.Y && rect.Y < textWrappingBounds.Bottom) || (((rect.Y + size.Height > textWrappingBounds.Y && IsParaSpacingExceed(Math.Round(size.Height, 2), Math.Round(textWrappingBounds.Y, 2), rect)) || IsTextFitBelow(textWrappingBounds, rect.Y + size.Height, floatingEntity)) && rect.Y + size.Height < textWrappingBounds.Bottom) || (rect.Y < textWrappingBounds.Y && rect.Y + size.Height > textWrappingBounds.Bottom && textWrappingBounds.Height > 0f)) && (!allowOverlap || (!isTextRangeInTextBox && ((!(LeafWidget is WPicture) && !(LeafWidget is Shape) && !(LeafWidget is WChart) && !(LeafWidget is WTextBox) && !(LeafWidget is GroupShape)) || (LeafWidget as ParagraphItem).GetTextWrappingStyle() == TextWrappingStyle.Inline || !(LeafWidget as ParagraphItem).GetAllowOverlap()))))
		{
			if (!(LeafWidget is Entity) || !(LeafWidget as Entity).IsFloatingItem(isTextWrapAround: false) || !(size.Width < num))
			{
				if ((LeafWidget is ParagraphItem || LeafWidget is SplitStringWidget) && entity != null)
				{
					return !entity.IsFloatingItem(isTextWrapAround: false);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private bool IsNeedToForceDynamicRelayout(WParagraph currWidgetOwnerPara, ParagraphLayoutInfo paragraphLayoutInfo, TextWrappingStyle textWrappingStyle, Entity floatingEntity)
	{
		if (currWidgetOwnerPara.IsInCell && textWrappingStyle == TextWrappingStyle.TopAndBottom && !(floatingEntity is WTable))
		{
			WParagraph wParagraph = ((floatingEntity != null && floatingEntity.Owner is WParagraph) ? (floatingEntity.Owner as WParagraph) : (floatingEntity as WParagraph));
			if (wParagraph != null && wParagraph.IsInCell && !(floatingEntity as ParagraphItem).GetLayOutInCell() && currWidgetOwnerPara.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && paragraphLayoutInfo.YPosition + paragraphLayoutInfo.Size.Height >= m_ltWidget.Bounds.Y && paragraphLayoutInfo.YPosition + paragraphLayoutInfo.Size.Height < m_ltWidget.Bounds.Bottom)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsFloatingItemLayoutInCell(WParagraph currWidgetOwnerPara, TextWrappingStyle textWrappingStyle, Entity floatingEntity)
	{
		if (currWidgetOwnerPara.IsInCell && (textWrappingStyle != TextWrappingStyle.Behind || textWrappingStyle != TextWrappingStyle.InFrontOfText) && !(floatingEntity is WTable))
		{
			WParagraph wParagraph = ((floatingEntity != null && floatingEntity.Owner is WParagraph) ? (floatingEntity.Owner as WParagraph) : (floatingEntity as WParagraph));
			if (wParagraph != null && wParagraph.IsInCell && ((floatingEntity as ParagraphItem).GetLayOutInCell() || currWidgetOwnerPara.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsOwnerParaNotFittedInSamePage(TextWrappingStyle textWrappingStyle, Entity floatingEntity, Layouter layouter, LayoutedWidget floatingWidget, ParagraphLayoutInfo paragraphLayoutInfo)
	{
		if ((textWrappingStyle == TextWrappingStyle.Square || textWrappingStyle == TextWrappingStyle.TopAndBottom) && !(floatingEntity is WTable))
		{
			bool num = textWrappingStyle == TextWrappingStyle.Square && floatingWidget.Bounds.X - 18f < m_layoutArea.ClientActiveArea.X && floatingWidget.Bounds.Right > m_layoutArea.ClientActiveArea.Right;
			bool flag = layouter.m_firstItemInPageYPosition > floatingWidget.Bounds.Y && paragraphLayoutInfo.YPosition + floatingWidget.Bounds.Bottom > m_layoutArea.ClientActiveArea.Bottom;
			return num && flag;
		}
		return false;
	}

	private bool IsNeedDoIntermediateWrapping(float remainingClientWidth, TextWrappingStyle textWrappingStyle, Layouter layouter, TextWrappingType textWrappingType, RectangleF rect, SizeF size, ParagraphLayoutInfo paragraphLayoutInfo, bool isDoesNotDenotesRectangle, RectangleF textWrappingBounds, ILeafWidget leafWidget, float minwidth, float minimumWidthRequired)
	{
		if ((!(remainingClientWidth > minimumWidthRequired) && (!((textWrappingStyle == TextWrappingStyle.Through || textWrappingStyle == TextWrappingStyle.Tight) && isDoesNotDenotesRectangle) || !(remainingClientWidth > minwidth))) || (((!(Math.Round(rect.Width, 2) <= Math.Round(minwidth, 2)) && (!(rect.Width < size.Width) || !(leafWidget.LayoutInfo is TabsLayoutInfo))) || textWrappingType == TextWrappingType.Left || textWrappingType == TextWrappingType.Largest) && textWrappingType != TextWrappingType.Right && (!(rect.Width < remainingClientWidth) || textWrappingType != TextWrappingType.Largest)))
		{
			if ((!((textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && isDoesNotDenotesRectangle) || !(textWrappingBounds.X < paragraphLayoutInfo.XPosition)) && (Math.Round(textWrappingBounds.X - paragraphLayoutInfo.XPosition + GetPararaphLeftIndent(), 2) < (double)minimumWidthRequired || (leafWidget is WTextRange && IsFloatingItemOnLeft(rect, minwidth, textWrappingBounds))))
			{
				if (textWrappingType == TextWrappingType.Left)
				{
					return remainingClientWidth < minimumWidthRequired;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	internal bool IsFloatingItemOnLeft(RectangleF rect, float minwidth, RectangleF textWrappingBounds)
	{
		Layouter layouter = m_lcOperator as Layouter;
		List<FloatingItem> list = new List<FloatingItem>(layouter.FloatingItems);
		FloatingItem.SortFloatingItems(list, SortPosition.Bottom, isNeedToUpdateWrapCollectionIndex: false);
		List<FloatingItem> list2 = new List<FloatingItem>();
		for (int i = 0; i < list.Count; i++)
		{
			RectangleF textWrappingBounds2 = list[i].TextWrappingBounds;
			textWrappingBounds2.Width += layouter.ClientLayoutArea.Width - textWrappingBounds2.Right;
			if (rect.IntersectsWith(textWrappingBounds2) && rect.X >= list[i].TextWrappingBounds.Right)
			{
				list2.Add(list[i]);
			}
		}
		if (list2.Count > 0 && rect.X + minwidth > textWrappingBounds.X)
		{
			return true;
		}
		return false;
	}

	private bool IsLineBreakIntersectOnFloatingItem(ILeafWidget leafWidget, TextWrappingStyle textWrappingStyle, RectangleF textWrappingBounds, RectangleF rect, SizeF size, WParagraph ownerPara)
	{
		float num = rect.Y + size.Height;
		float num2 = rect.X - ownerPara.ParagraphFormat.FirstLineIndent;
		if (leafWidget is Break && ((leafWidget as Break).BreakType == BreakType.LineBreak || (leafWidget as Break).BreakType == BreakType.TextWrappingBreak) && (textWrappingStyle == TextWrappingStyle.Through || textWrappingStyle == TextWrappingStyle.Square || textWrappingStyle == TextWrappingStyle.Tight) && ownerPara.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && (m_lcOperator as Layouter).IsFirstItemInLine && ownerPara.ParagraphFormat.FirstLineIndent > 0f)
		{
			if (num2 < textWrappingBounds.X && textWrappingBounds.X <= rect.X && textWrappingBounds.Y <= num)
			{
				return num <= textWrappingBounds.Bottom;
			}
			return false;
		}
		return false;
	}

	private RectangleF RemoveDistancesFromTextValueOfLeftAndRight(Entity entity, RectangleF bounds)
	{
		if (entity is WPicture)
		{
			return new RectangleF(bounds.X + (entity as WPicture).DistanceFromLeft, bounds.Y, bounds.Width - ((entity as WPicture).DistanceFromRight + (entity as WPicture).DistanceFromLeft), bounds.Height);
		}
		if (entity is Shape)
		{
			return new RectangleF(bounds.X + (entity as Shape).WrapFormat.DistanceLeft, bounds.Y, bounds.Width - ((entity as Shape).WrapFormat.DistanceRight + (entity as Shape).WrapFormat.DistanceLeft), bounds.Height);
		}
		if (entity is WTextBox)
		{
			return new RectangleF(bounds.X + (entity as WTextBox).TextBoxFormat.WrapDistanceLeft, bounds.Y, bounds.Width - ((entity as WTextBox).TextBoxFormat.WrapDistanceRight + (entity as WTextBox).TextBoxFormat.WrapDistanceLeft), bounds.Height);
		}
		if (entity is GroupShape)
		{
			return new RectangleF(bounds.X + (entity as GroupShape).WrapFormat.DistanceLeft, bounds.Y, bounds.Width - ((entity as GroupShape).WrapFormat.DistanceRight + (entity as GroupShape).WrapFormat.DistanceLeft), bounds.Height);
		}
		if (entity is WChart)
		{
			return new RectangleF(bounds.X + (entity as WChart).WrapFormat.DistanceLeft, bounds.Y, bounds.Width - ((entity as WChart).WrapFormat.DistanceRight + (entity as WChart).WrapFormat.DistanceLeft), bounds.Height);
		}
		return bounds;
	}

	internal void AdjustClientAreaBasedOnTextWrap(ILeafWidget leafWidget, ref SizeF size, ref RectangleF rect)
	{
		bool flag = (leafWidget is WPicture || leafWidget is Shape || leafWidget is WTextBox || leafWidget is GroupShape || leafWidget is WChart) && (leafWidget as Entity).IsFloatingItem(isTextWrapAround: true);
		VerticalOrigin verticalOrigin = VerticalOrigin.Line;
		ShapeVerticalAlignment shapeVerticalAlignment = ShapeVerticalAlignment.None;
		if (flag)
		{
			verticalOrigin = (leafWidget as ParagraphItem).GetVerticalOrigin();
			shapeVerticalAlignment = (leafWidget as ParagraphItem).GetShapeVerticalAlignment();
		}
		WParagraph ownerParagraph = GetOwnerParagraph();
		Layouter layouter = m_lcOperator as Layouter;
		float y = rect.Y;
		float num = 0f;
		RectangleF rectangleF = RectangleF.Empty;
		ParagraphLayoutInfo paragraphLayoutInfo = null;
		if (ownerParagraph != null)
		{
			paragraphLayoutInfo = ((IWidget)ownerParagraph).LayoutInfo as ParagraphLayoutInfo;
		}
		if (IsNeedToWrapLeafWidget(ownerParagraph, layouter) && !layouter.IsLayoutingFootnote && !SkipBookmark(size) && !SkipSectionBreak(ownerParagraph, leafWidget))
		{
			RectangleF clientLayoutArea = layouter.ClientLayoutArea;
			int wrapItemIndex = -1;
			if (leafWidget is WPicture || leafWidget is WChart || leafWidget is WTextBox || leafWidget is Shape || leafWidget is GroupShape)
			{
				wrapItemIndex = (leafWidget as ParagraphItem).GetWrapCollectionIndex();
			}
			bool isTextRangeInTextBox = false;
			bool isWithInTextBox = false;
			if (ownerParagraph != null && (leafWidget is ParagraphItem || leafWidget is SplitStringWidget))
			{
				WTextBox wTextBox = IsEntityOwnerIsWTextbox(ownerParagraph);
				if (wTextBox != null)
				{
					isWithInTextBox = true;
					if (wTextBox.TextBoxFormat.TextWrappingStyle != 0)
					{
						isTextRangeInTextBox = wTextBox.TextBoxFormat.AllowOverlap;
					}
				}
				else if (ownerParagraph.OwnerTextBody.Owner is Shape)
				{
					isWithInTextBox = true;
					Shape shape = ownerParagraph.OwnerTextBody.Owner as Shape;
					if (shape.WrapFormat.TextWrappingStyle != 0)
					{
						isTextRangeInTextBox = shape.WrapFormat.AllowOverlap;
					}
				}
				else if (ownerParagraph.OwnerTextBody.Owner is ChildShape)
				{
					isWithInTextBox = true;
					GroupShape ownerGroupShape = (ownerParagraph.OwnerTextBody.Owner as ChildShape).GetOwnerGroupShape();
					if (ownerGroupShape.WrapFormat.TextWrappingStyle != 0)
					{
						isTextRangeInTextBox = ownerGroupShape.WrapFormat.AllowOverlap;
					}
				}
			}
			float x = rect.X;
			FloatingItem.SortFloatingItems(layouter.FloatingItems, SortPosition.Y, isNeedToUpdateWrapCollectionIndex: true);
			if (IsWord2013(ownerParagraph.Document))
			{
				FloatingItem.SortIntersectedYPostionFloatingItems(layouter.FloatingItems, SortPosition.X);
			}
			else
			{
				FloatingItem.SortSameYPostionFloatingItems(layouter.FloatingItems, SortPosition.X);
			}
			int floattingItemIndex = GetFloattingItemIndex(ownerParagraph);
			for (int i = 0; i < layouter.FloatingItems.Count; i++)
			{
				bool allowOverlap = layouter.FloatingItems[i].AllowOverlap;
				if (ownerParagraph.IsInCell && allowOverlap && (ownerParagraph.GetOwnerEntity() as WTableCell).OwnerRow.OwnerTable.TableFormat.Positioning.AllowOverlap && (!((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity is WTable) || !((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity as WTable).IsInCell))
				{
					WParagraph ownerParagraph2 = (m_lcOperator as Layouter).FloatingItems[i].OwnerParagraph;
					if (ownerParagraph2 == null || !ownerParagraph2.IsInCell || ownerParagraph.GetOwnerEntity() != ownerParagraph2.GetOwnerEntity())
					{
						continue;
					}
				}
				float x2 = layouter.FloatingItems[i].TextWrappingBounds.X;
				RectangleF textWrappingBounds = layouter.FloatingItems[i].TextWrappingBounds;
				TextWrappingStyle textWrappingStyle = layouter.FloatingItems[i].TextWrappingStyle;
				TextWrappingType textWrappingType = layouter.FloatingItems[i].TextWrappingType;
				if (ownerParagraph.IsInCell && (ownerParagraph.GetOwnerEntity() as WTableCell).OwnerRow.OwnerTable.TableFormat.WrapTextAround && textWrappingStyle == TextWrappingStyle.TopAndBottom)
				{
					WParagraph ownerParagraph3 = (m_lcOperator as Layouter).FloatingItems[i].OwnerParagraph;
					if (ownerParagraph3 == null || !ownerParagraph3.IsInCell || ownerParagraph.GetOwnerEntity() != ownerParagraph3.GetOwnerEntity())
					{
						continue;
					}
				}
				if (IsLineBreakIntersectOnFloatingItem(leafWidget, textWrappingStyle, textWrappingBounds, rect, size, ownerParagraph))
				{
					continue;
				}
				WTextBody ownerBody = null;
				if ((!IsInSameTextBody(ownerParagraph, (m_lcOperator as Layouter).FloatingItems[i], ref ownerBody) && ownerParagraph.IsInCell && ownerBody is WTableCell) || (IsInFrame((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity as WParagraph) && IsOwnerCellInFrame(ownerParagraph)))
				{
					continue;
				}
				if (flag && !((LeafWidget as ParagraphItem).GetAllowOverlap() && allowOverlap))
				{
					textWrappingBounds = RemoveDistancesFromTextValueOfLeftAndRight(layouter.FloatingItems[i].FloatingEntity, textWrappingBounds);
				}
				if (ownerParagraph.ParagraphFormat.Bidi && IsInSameTextBody(ownerParagraph, (m_lcOperator as Layouter).FloatingItems[i], ref ownerBody) && ownerParagraph.IsInCell && ownerBody is WTableCell)
				{
					ModifyXPositionForRTLLayouting(i, ref textWrappingBounds, m_layoutArea.ClientArea);
				}
				else if (ownerParagraph.ParagraphFormat.Bidi)
				{
					ModifyXPositionForRTLLayouting(i, ref textWrappingBounds, (m_lcOperator as Layouter).ClientLayoutArea);
				}
				float num2 = 0f;
				if (ownerParagraph.IsInCell)
				{
					CellLayoutInfo cellLayoutInfo = (ownerParagraph.GetOwnerEntity() as IWidget).LayoutInfo as CellLayoutInfo;
					num2 = cellLayoutInfo.Paddings.Left + cellLayoutInfo.Paddings.Right;
				}
				float num3 = 18f;
				if (textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through)
				{
					num3 = ((ownerParagraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? 17.6f : 8f);
				}
				num3 -= num2;
				if ((textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && layouter.FloatingItems[i].IsDoesNotDenotesRectangle)
				{
					float x3 = rect.X;
					if (IsDoNotSuppressIndent(ownerParagraph, rect.Y, textWrappingBounds.Bottom, i) && !(leafWidget.LayoutInfo is TabsLayoutInfo))
					{
						rect.X -= paragraphLayoutInfo.Margins.Left;
					}
					textWrappingBounds = AdjustTightAndThroughBounds(layouter.FloatingItems[i], rect, size.Height);
					if (textWrappingBounds == layouter.FloatingItems[i].TextWrappingBounds && (!(rect.X < textWrappingBounds.X) || !(rect.Y + size.Height > textWrappingBounds.Top) || !(textWrappingBounds.Bottom > rect.Y + size.Height)))
					{
						continue;
					}
					rect.X = x3;
				}
				bool flag2 = (textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && layouter.FloatingItems[i].TextWrappingBounds.Width > (m_lcOperator as Layouter).CurrentSection.PageSetup.PageSize.Width && layouter.FloatingItems[i].IsDoesNotDenotesRectangle;
				float bottom = rect.Y + ((IWidget)ownerParagraph).LayoutInfo.Size.Height;
				if (((Math.Round(textWrappingBounds.Y, 2) <= (double)rect.Y && textWrappingBounds.Bottom > rect.Y) || (rect.Y + size.Height >= textWrappingBounds.Y && rect.Y + size.Height <= textWrappingBounds.Bottom)) && (textWrappingStyle == TextWrappingStyle.Square || textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && leafWidget is Break && (leafWidget as Break).CharacterFormat.BreakClear == BreakClearType.All && !flag2 && num < textWrappingBounds.Bottom)
				{
					num = textWrappingBounds.Bottom;
				}
				if (CanWrapBreak(ownerParagraph, (m_lcOperator as Layouter).FloatingItems[i].OwnerParagraph) && IsNeedToWrapParaMarkToRightSide(ownerParagraph, textWrappingBounds, bottom, layouter, rect, textWrappingType, textWrappingStyle, num3) && !flag2)
				{
					if (num != 0f)
					{
						rect.Y = num;
						m_layoutArea.UpdateBoundsBasedOnTextWrap(num);
					}
					rect.X += textWrappingBounds.Width;
					LeafWidget.LayoutInfo.IsLineBreak = false;
					size.Height = 0f;
					size.Width = textWrappingBounds.Width;
					return;
				}
				WSection wSection = GetBaseEntity(ownerParagraph) as WSection;
				bool flag3 = IsWord2013(ownerParagraph.Document);
				if (ownerParagraph.OwnerTextBody is HeaderFooter && textWrappingStyle == TextWrappingStyle.Square && layouter.FloatingItems[i].FloatingEntity is WTable && (layouter.FloatingItems[i].FloatingEntity as WTable).OwnerTextBody == ownerParagraph.OwnerTextBody && (layouter.FloatingItems[i].FloatingEntity as WTable).TableFormat.WrapTextAround && (layouter.FloatingItems[i].FloatingEntity as WTable).TableFormat.Positioning.VertRelationTo == VerticalRelation.Page && (m_lcOperator as Layouter).IsLayoutingHeaderFooter && !(m_lcOperator as Layouter).IsLayoutingHeader)
				{
					WTextRange currTextRange = GetCurrTextRange();
					WTable wTable = layouter.FloatingItems[i].FloatingEntity as WTable;
					float border = GetMaximumRightCellBorderWidth(wTable);
					HorizontalPosition horizPositionAbs = wTable.TableFormat.Positioning.HorizPositionAbs;
					bool isBorderValueZero = false;
					GetDefaultBorderSpacingValue(ref border, ref isBorderValueZero, flag3, horizPositionAbs);
					float minimumWidthRequiredForTable = GetMinimumWidthRequiredForTable(currTextRange, wTable, border, flag3, horizPositionAbs, isBorderValueZero);
					if ((rect.X + size.Width > textWrappingBounds.X || minimumWidthRequiredForTable > textWrappingBounds.X - rect.X) && (textWrappingBounds.Right + size.Width > rect.Right || minimumWidthRequiredForTable > rect.Right - textWrappingBounds.Right) && rect.Y + size.Height > wSection.PageSetup.PageSize.Height - wSection.PageSetup.FooterDistance)
					{
						rect.Y = textWrappingBounds.Y - size.Height;
						rect.Height -= size.Height;
						CreateLayoutArea(rect);
						(m_lcOperator as Layouter).IsNeedtoAdjustFooter = true;
					}
				}
				float adjustingValue = 0f;
				bool num4;
				if (!IsNeedToConsiderAdjustValues(ref adjustingValue, ownerParagraph, textWrappingStyle, i))
				{
					if (clientLayoutArea.X > textWrappingBounds.Right + num3)
					{
						goto IL_264a;
					}
					num4 = clientLayoutArea.Right < textWrappingBounds.X - num3;
				}
				else
				{
					num4 = clientLayoutArea.X > textWrappingBounds.Right + adjustingValue;
				}
				float num5;
				float num7;
				float num8;
				bool flag4;
				IEntity floatingEntity;
				bool flag5;
				float num10;
				bool num11;
				if (!num4 && !flag2)
				{
					if (IsNeedToWrapForSquareTightAndThrough(layouter, floattingItemIndex, wrapItemIndex, textWrappingStyle, textWrappingBounds, allowOverlap, layouter.FloatingItems[i].WrapCollectionIndex, layouter.FloatingItems[i].FloatingEntity, isTextRangeInTextBox, rect, size, isWithInTextBox))
					{
						num5 = 0f;
						float num6 = 0f;
						num7 = 0f;
						num8 = (paragraphLayoutInfo.IsFirstLine ? (paragraphLayoutInfo.FirstLineIndent + paragraphLayoutInfo.ListTab) : 0f);
						num8 = ((num8 > 0f) ? num8 : 0f);
						flag4 = false;
						WTextRange currTextRange2 = GetCurrTextRange();
						if (ownerParagraph != null)
						{
							if (rect.X >= textWrappingBounds.X && textWrappingType != TextWrappingType.Left)
							{
								num5 = paragraphLayoutInfo.Margins.Right;
							}
							if (rect.X < textWrappingBounds.X && textWrappingType != TextWrappingType.Right)
							{
								num6 = paragraphLayoutInfo.Margins.Left;
							}
							WListFormat listFormatValue;
							WListLevel listLevel;
							if (Math.Round(rect.X, 2) == Math.Round(clientLayoutArea.X + paragraphLayoutInfo.Margins.Left, 2) && (listFormatValue = ownerParagraph.GetListFormatValue()) != null && listFormatValue.CurrentListStyle != null && (listLevel = ownerParagraph.GetListLevel(listFormatValue)) != null && listLevel.ParagraphFormat.LeftIndent != 0f)
							{
								num7 = paragraphLayoutInfo.Margins.Left;
								flag4 = true;
							}
						}
						if ((textWrappingStyle == TextWrappingStyle.Square || textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && leafWidget is Shape && (leafWidget as Shape).WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline && (leafWidget as Shape).IsHorizontalRule)
						{
							if (textWrappingBounds.X - rect.X > 18f)
							{
								size.Width = textWrappingBounds.X - rect.X;
							}
							else
							{
								size.Width = size.Width - (textWrappingBounds.Right - rect.X) - num5;
							}
						}
						float border2 = 0f;
						bool isBorderValueZero2 = false;
						WTable wTable2 = null;
						float num9 = 0f;
						HorizontalPosition tableHorizontalPosition = HorizontalPosition.Left;
						floatingEntity = layouter.FloatingItems[i].FloatingEntity;
						if (layouter.FloatingItems[i].FloatingEntity is WTable)
						{
							wTable2 = floatingEntity as WTable;
							tableHorizontalPosition = wTable2.TableFormat.Positioning.HorizPositionAbs;
							border2 = GetMaximumRightCellBorderWidth(wTable2);
							GetDefaultBorderSpacingValue(ref border2, ref isBorderValueZero2, flag3, tableHorizontalPosition);
							num9 = wTable2.TableFormat.Borders.Left.LineWidth / 2f;
						}
						if (((textWrappingStyle != TextWrappingStyle.Tight && textWrappingStyle != TextWrappingStyle.Through) || !layouter.FloatingItems[i].IsDoesNotDenotesRectangle || !(textWrappingBounds == layouter.FloatingItems[i].TextWrappingBounds)) && rect.X + num9 >= textWrappingBounds.X && rect.X < textWrappingBounds.Right && !(textWrappingType == TextWrappingType.Left && flag3))
						{
							rect.Width = rect.Width - (textWrappingBounds.Right - rect.X) - num5 - border2;
							m_isWrapText = true;
							flag5 = true;
							if (floatingEntity is WParagraph && ownerParagraph != null)
							{
								if (flag3)
								{
									num3 = 22f;
									if (rect.Width < size.Width)
									{
										flag5 = false;
									}
								}
								else
								{
									num3 = 75f;
								}
								if (num3 > rect.Width)
								{
									flag5 = false;
								}
							}
							if (wTable2 != null)
							{
								num3 = GetMinimumWidthRequiredForTable(currTextRange2, wTable2, border2, flag3, tableHorizontalPosition, isBorderValueZero2);
							}
							if (!flag5 || Math.Round(rect.Width) < (double)num3 || (rect.Width < size.Width && leafWidget.LayoutInfo is TabsLayoutInfo) || textWrappingBounds.X < paragraphLayoutInfo.XPosition + GetPararaphLeftIndent())
							{
								rect.Width = m_layoutArea.ClientActiveArea.Right - textWrappingBounds.Right - border2 - (flag4 ? num7 : 0f);
								if (((num5 < 0f) ? (rect.Width + num5) : rect.Width) < num3 && num5 < 0f)
								{
									rect.Width += num5;
								}
								num10 = 0f;
								num10 = ((currTextRange2 == null) ? size.Width : GetMinWidth(currTextRange2, size, rect));
								if (Math.Round(rect.Width) < (double)num3)
								{
									goto IL_0f8b;
								}
								if (leafWidget is ISplitLeafWidget)
								{
									if (rect.Width < num10 && (m_lcOperator as Layouter).m_canSplitbyCharacter && currTextRange2 != null)
									{
										if (wTable2 == null && !flag3)
										{
											num11 = textWrappingStyle != TextWrappingStyle.Square;
											goto IL_0f86;
										}
										goto IL_0f8b;
									}
								}
								else if (rect.Width < size.Width)
								{
									if (wTable2 != null && !flag3)
									{
										num11 = textWrappingStyle != TextWrappingStyle.Square;
										goto IL_0f86;
									}
									goto IL_0f8b;
								}
								goto IL_1365;
							}
							if (layouter.RightPositionOfTabStopInterSectingFloattingItems == float.MinValue)
							{
								rect.X = textWrappingBounds.Right + (flag4 ? num7 : 0f) + num8;
								rect.Width = m_layoutArea.ClientActiveArea.Right - textWrappingBounds.Right - (flag4 ? num7 : 0f) - num8;
								float num12 = 0f;
								num12 = ((currTextRange2 == null) ? size.Width : GetMinWidth(currTextRange2, size, rect));
								if (textWrappingStyle == TextWrappingStyle.Through && (m_lcOperator as Layouter).FloatingItems[i].IsDoesNotDenotesRectangle)
								{
									UpdateXposition(textWrappingBounds, i, ref rect, size, num12);
								}
								if (IsFirstTextRangeInParagraph(leafWidget))
								{
									num12 += paragraphLayoutInfo.ListTabWidth;
								}
								if ((textWrappingStyle == TextWrappingStyle.Square && rect.Width < 0f && size.Width > 0f) || rect.Width < num12)
								{
									float topMarginValueForFloatingTable = GetTopMarginValueForFloatingTable(ownerParagraph, layouter.FloatingItems[i].FloatingEntity, rect.Y);
									m_isYPositionUpdated = true;
									rect.Width = m_layoutArea.ClientArea.Width;
									rect.Height -= textWrappingBounds.Bottom + topMarginValueForFloatingTable - rect.Y;
									rect.Y = textWrappingBounds.Bottom + topMarginValueForFloatingTable;
									rect.X = x;
								}
								else
								{
									m_isXPositionUpdated = true;
								}
								if (!(leafWidget is Break))
								{
									AdjustClientAreaBasedOnExceededTab(leafWidget, size, ref rect, ownerParagraph);
								}
								if (ownerParagraph != null && ownerParagraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
								{
									layouter.m_canSplitbyCharacter = false;
								}
								m_isWrapText = true;
								CreateLayoutArea(rect);
							}
						}
						else if (textWrappingBounds.X >= rect.X && rect.Right > textWrappingBounds.X)
						{
							float num13 = 18f;
							if (textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through)
							{
								num13 = ((ownerParagraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? 18f : 9f);
							}
							rect.Width = textWrappingBounds.X - rect.X - num5;
							float num14 = m_layoutArea.ClientActiveArea.Right - textWrappingBounds.Right;
							num14 = ((num14 > 0f) ? num14 : 0f);
							if (num14 == 0f && rectangleF != RectangleF.Empty && rectangleF.Bottom < textWrappingBounds.Y)
							{
								num14 += rectangleF.Width;
							}
							rectangleF = textWrappingBounds;
							m_isWrapText = true;
							float num15 = 0f;
							num15 = ((currTextRange2 == null) ? size.Width : GetMinWidth(currTextRange2, size, rect));
							if (layouter.FloatingItems[i].FloatingEntity is WParagraph && ownerParagraph != null)
							{
								num3 = ((!flag3) ? 75f : 22f);
							}
							if (wTable2 != null)
							{
								num3 = GetMinimumWidthRequiredForTable(currTextRange2, wTable2, border2, flag3, tableHorizontalPosition, isBorderValueZero2);
							}
							if ((num14 < num3 && textWrappingStyle != TextWrappingStyle.Tight && textWrappingStyle != TextWrappingStyle.Through) || num14 < num15)
							{
								m_isWrapText = false;
							}
							if (IsNeedDoIntermediateWrapping(num14, textWrappingStyle, layouter, textWrappingType, rect, size, paragraphLayoutInfo, layouter.FloatingItems[i].IsDoesNotDenotesRectangle, textWrappingBounds, leafWidget, num15, num3))
							{
								rect.Width = num14;
								m_isWrapText = true;
								if (rect.X + num15 > textWrappingBounds.X || textWrappingType == TextWrappingType.Right || textWrappingType == TextWrappingType.Largest || clientLayoutArea.X > textWrappingBounds.X - num13)
								{
									rect.X = textWrappingBounds.Right;
									WListFormat wListFormat = null;
									WListLevel wListLevel = null;
									if (paragraphLayoutInfo.IsFirstLine && (wListFormat = ownerParagraph.GetListFormatValue()) != null && wListFormat.CurrentListStyle != null && (wListLevel = ownerParagraph.GetListLevel(wListFormat)) != null && wListLevel.ParagraphFormat.LeftIndent != 0f)
									{
										float x4 = 0f;
										float width = rect.Width;
										UpdateParaFirstLineHorizontalPositions(paragraphLayoutInfo, ownerParagraph, ref x4, ref width);
										rect.X += x4 + paragraphLayoutInfo.Margins.Left;
										rect.Width -= x4 + paragraphLayoutInfo.Margins.Left;
									}
									m_isXPositionUpdated = true;
									if (textWrappingStyle == TextWrappingStyle.Through && layouter.FloatingItems[i].IsDoesNotDenotesRectangle)
									{
										UpdateXposition(textWrappingBounds, i, ref rect, size, num15);
									}
									if ((textWrappingStyle == TextWrappingStyle.Through && layouter.FloatingItems[i].IsDoesNotDenotesRectangle) || rect.Width > num15 || textWrappingType == TextWrappingType.Right || textWrappingType == TextWrappingType.Largest)
									{
										CreateLayoutArea(rect);
									}
								}
								if ((rect.Width < num3 && (!(num15 < num14) || (TextWrappingStyle.Tight != textWrappingStyle && textWrappingStyle != TextWrappingStyle.Through))) || (rect.Width < num15 && Math.Round(rect.Right, 2) == Math.Round(layouter.ClientLayoutArea.Right, 2) && textWrappingType == TextWrappingType.Both))
								{
									List<FloatingItem> list = new List<FloatingItem>(layouter.FloatingItems);
									List<FloatingItem> list2 = new List<FloatingItem>(layouter.FloatingItems);
									FloatingItem.SortFloatingItems(list, SortPosition.X, isNeedToUpdateWrapCollectionIndex: false);
									FloatingItem.SortFloatingItems(list2, SortPosition.Bottom, isNeedToUpdateWrapCollectionIndex: false);
									RectangleF textWrappingBounds2 = textWrappingBounds;
									float num16 = float.MinValue;
									if (list2.Count > 1)
									{
										for (int j = 0; j < list2.Count; j++)
										{
											if ((!ownerParagraph.IsInCell && IsInTable(list2[j].FloatingEntity)) || rect.Y > list2[j].TextWrappingBounds.Bottom || rect.Bottom < list2[j].TextWrappingBounds.Y)
											{
												continue;
											}
											int num17 = list.IndexOf(list2[j]);
											if (num17 == 0)
											{
												if (!(list[1].FloatingEntity.Owner is HeaderFooter) && list[1].TextWrappingBounds.Bottom - list[num17].TextWrappingBounds.Bottom > num3 && (list[num17].TextWrappingBounds.Right + num15 > list[num17 + 1].TextWrappingBounds.X || list[num17 + 1].TextWrappingBounds.X - list[num17].TextWrappingBounds.X > num3))
												{
													textWrappingBounds2 = list[num17].TextWrappingBounds;
													if ((list[num17].TextWrappingStyle == TextWrappingStyle.Tight || list[num17].TextWrappingStyle == TextWrappingStyle.Through) && !list[num17].IsDoesNotDenotesRectangle)
													{
														num16 = GetFloattingItemBottom(list[num17].FloatingEntity, list[num17].TextWrappingBounds.Bottom);
													}
													break;
												}
											}
											else if (num17 == list2.Count - 1)
											{
												if (list[num17 - 1].TextWrappingBounds.Right + num15 < list[num17].TextWrappingBounds.X && list[num17 - 1].TextWrappingBounds.Bottom - list[num17].TextWrappingBounds.Bottom > num3 && list[num17 - 1].TextWrappingBounds.Right - list[num17].TextWrappingBounds.Right > num3)
												{
													textWrappingBounds2 = list[num17].TextWrappingBounds;
													if ((list[num17].TextWrappingStyle == TextWrappingStyle.Tight || list[num17].TextWrappingStyle == TextWrappingStyle.Through) && !list[num17].IsDoesNotDenotesRectangle)
													{
														num16 = GetFloattingItemBottom(list[num17].FloatingEntity, list[num17].TextWrappingBounds.Bottom);
													}
													break;
												}
											}
											else if (list[num17 + 1].TextWrappingBounds.X - list[num17].TextWrappingBounds.X > num3 && list[num17 + 1].TextWrappingBounds.Bottom - list[num17].TextWrappingBounds.Bottom > num3 && list[num17].TextWrappingBounds.Right + num15 > list[num17 + 1].TextWrappingBounds.X && list[num17 - 1].TextWrappingBounds.Right + num15 < list[num17 + 1].TextWrappingBounds.X)
											{
												textWrappingBounds2 = list[num17].TextWrappingBounds;
												if ((list[num17].TextWrappingStyle == TextWrappingStyle.Tight || list[num17].TextWrappingStyle == TextWrappingStyle.Through) && !list[num17].IsDoesNotDenotesRectangle)
												{
													num16 = GetFloattingItemBottom(list[num17].FloatingEntity, list[num17].TextWrappingBounds.Bottom);
												}
												break;
											}
										}
									}
									if (num16 != float.MinValue && (textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && !(GetBaseEntity(layouter.FloatingItems[i].FloatingEntity) is HeaderFooter) && !layouter.FloatingItems[i].IsDoesNotDenotesRectangle)
									{
										textWrappingBounds2 = GetBottomPositionForTightAndThrough(num16, textWrappingBounds2, ownerParagraph, rect.Y, size.Height);
									}
									if (Math.Round(rect.X, 2) == Math.Round(GetPageMarginLeft() + GetPararaphLeftIndent(), 2))
									{
										float num18 = 0f;
										num18 = GetTopMarginValueForFloatingTable(ownerParagraph, layouter.FloatingItems[i].FloatingEntity, rect.Y);
										rect.Y = textWrappingBounds2.Bottom + num18;
										m_isYPositionUpdated = true;
										rect.Width = m_layoutArea.ClientArea.Width;
										rect.Height -= textWrappingBounds2.Height + num18;
										CreateLayoutArea(rect);
										m_isWrapText = false;
									}
									else if ((Math.Round(rect.Right, 2) >= Math.Round(layouter.ClientLayoutArea.Right, 2) && textWrappingType == TextWrappingType.Both) || (ownerParagraph != null && ownerParagraph.IsInCell && Math.Round(rect.Right, 2) >= Math.Round(((ownerParagraph.GetOwnerEntity() as WidgetBase).m_layoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Right, 2) && layouter.IsFirstItemInLine && IsInSameTextBody(ownerParagraph, layouter.FloatingItems[i], ref ownerBody)))
									{
										float width2 = (ownerParagraph.IsInCell ? ((ownerParagraph.GetOwnerEntity() as WidgetBase).m_layoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Width : layouter.ClientLayoutArea.Width);
										float num19 = (ownerParagraph.IsInCell ? ((ownerParagraph.GetOwnerEntity() as WidgetBase).m_layoutInfo as CellLayoutInfo).CellContentLayoutingBounds.X : layouter.ClientLayoutArea.X);
										float num20 = 0f;
										num20 = GetTopMarginValueForFloatingTable(ownerParagraph, layouter.FloatingItems[i].FloatingEntity, rect.Y);
										rect.Y = textWrappingBounds2.Bottom + num20;
										rect.Width = width2;
										rect.Height -= textWrappingBounds2.Height + num20;
										rect.X = num19 + num6;
										CreateLayoutArea(rect);
										m_isXPositionUpdated = true;
										m_isYPositionUpdated = true;
										m_isWrapText = false;
									}
									else
									{
										rect.Width = 0f;
										CreateLayoutArea(rect);
									}
								}
								CreateLayoutArea(rect);
							}
							else
							{
								if (layouter.IsFirstItemInLine && ownerParagraph != null && textWrappingStyle == TextWrappingStyle.Square && IsWord2013(ownerParagraph.Document) && Math.Round(rect.Width, 2) <= Math.Round(num15, 2) && (IsInSameTextBody(ownerParagraph, layouter.FloatingItems[i], ref ownerBody) || ownerBody is HeaderFooter))
								{
									rect.X = clientLayoutArea.X + num6 + num8;
									rect.Height -= textWrappingBounds.Bottom - rect.Y;
									rect.Y = textWrappingBounds.Bottom;
									rect.Width = clientLayoutArea.Width;
								}
								else if (Math.Round(rect.Width, 2) <= Math.Round(num15, 2) && Math.Round(rect.X - num6, 2) != Math.Round((m_lcOperator as Layouter).ClientLayoutArea.X, 2))
								{
									rect.Width = 0f;
								}
								CreateLayoutArea(rect);
							}
						}
						else if (rect.X > textWrappingBounds.X && Math.Round(rect.X, 2) > Math.Round(textWrappingBounds.Right, 2) && textWrappingType != TextWrappingType.Left)
						{
							TabsLayoutInfo tabsLayoutInfo = null;
							if (leafWidget != null)
							{
								tabsLayoutInfo = leafWidget.LayoutInfo as TabsLayoutInfo;
							}
							rect.X = ((IsDoNotSuppressIndent(ownerParagraph, rect.Y, textWrappingBounds.Bottom, i) && layouter.IsFirstItemInLine && (!(leafWidget is Entity) || !(leafWidget as Entity).IsFloatingItem(isTextWrapAround: false))) ? (textWrappingBounds.Right + (paragraphLayoutInfo.IsFirstLine ? paragraphLayoutInfo.FirstLineIndent : 0f)) : rect.X);
							float num21 = 0f;
							num21 = ((currTextRange2 == null) ? size.Width : GetMinWidth(currTextRange2, size, rect));
							if (tabsLayoutInfo != null && num21 + size.Width > rect.Width)
							{
								rect.X = m_layoutArea.ClientArea.X;
								m_isYPositionUpdated = true;
								CreateLayoutArea(rect);
							}
							else
							{
								rect.Width = m_layoutArea.ClientArea.Width;
								CreateLayoutArea(rect);
							}
						}
						else if (rect.X > textWrappingBounds.X && rect.X < textWrappingBounds.Right && textWrappingType != TextWrappingType.Left)
						{
							rect.Width -= textWrappingBounds.Right - rect.X;
							rect.X = textWrappingBounds.Right;
							if (textWrappingStyle == TextWrappingStyle.Through && layouter.FloatingItems[i].IsDoesNotDenotesRectangle)
							{
								float num22 = 0f;
								UpdateXposition(minwidth: (currTextRange2 == null) ? size.Width : GetMinWidth(currTextRange2, size, rect), textWrappingBounds: textWrappingBounds, i: i, rect: ref rect, size: size);
							}
							CreateLayoutArea(rect);
							m_isXPositionUpdated = true;
							m_isWrapText = true;
						}
						goto IL_24ad;
					}
					if (IsNeedToWrapForTopAndBottom(ownerParagraph, layouter, floattingItemIndex, wrapItemIndex, textWrappingStyle, textWrappingBounds, allowOverlap, layouter.FloatingItems[i].WrapCollectionIndex, layouter.FloatingItems[i].FloatingEntity, isTextRangeInTextBox, rect, size))
					{
						if ((textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && !(GetBaseEntity(layouter.FloatingItems[i].FloatingEntity) is HeaderFooter) && !layouter.FloatingItems[i].IsDoesNotDenotesRectangle)
						{
							float floattingItemBottom = GetFloattingItemBottom(layouter.FloatingItems[i].FloatingEntity, textWrappingBounds.Bottom);
							textWrappingBounds = GetBottomPositionForTightAndThrough(floattingItemBottom, textWrappingBounds, ownerParagraph, rect.Y, size.Height);
						}
						float num23 = 0f;
						num23 = GetTopMarginValueForFloatingTable(ownerParagraph, layouter.FloatingItems[i].FloatingEntity, rect.Y);
						float y2 = rect.Y;
						rect.Y = textWrappingBounds.Bottom + num23;
						m_isYPositionUpdated = true;
						rect.Height -= textWrappingBounds.Bottom - y2 + num23;
						if (rect.Y != y && leafWidget is WTextRange && !(layouter.FloatingItems[i].FloatingEntity is WTable) && paragraphLayoutInfo.IsFirstLine)
						{
							rect.Y += paragraphLayoutInfo.Margins.Top;
							y = rect.Y;
						}
						CreateLayoutArea(rect);
					}
				}
				goto IL_264a;
				IL_0f86:
				if (num11)
				{
					goto IL_0f8b;
				}
				goto IL_1365;
				IL_264a:
				ResetXPositionForRTLLayouting(i, ref textWrappingBounds, x2);
				continue;
				IL_0f8b:
				if (flag5 && textWrappingBounds.X - (paragraphLayoutInfo.XPosition + GetPararaphLeftIndent()) > num3 && (wSection == null || wSection.Columns.Count == 1 || layouter.ClientLayoutArea.Right - textWrappingBounds.Right > num3))
				{
					rect.Width = 0f;
					if (textWrappingStyle == TextWrappingStyle.Square && size.Width > 0f && clientLayoutArea.X + num3 < textWrappingBounds.X)
					{
						float topMarginValueForFloatingTable2 = GetTopMarginValueForFloatingTable(ownerParagraph, layouter.FloatingItems[i].FloatingEntity, rect.Y);
						RectangleF intersectingItemBounds = FloatingItem.GetIntersectingItemBounds(m_lcOperator as Layouter, layouter.FloatingItems[i], y);
						if (intersectingItemBounds != RectangleF.Empty && intersectingItemBounds.Bottom <= textWrappingBounds.Bottom && clientLayoutArea.X + num3 > intersectingItemBounds.X)
						{
							rect.X = x;
							rect.Width = textWrappingBounds.X - rect.X - num5;
							rect.Y = intersectingItemBounds.Bottom + topMarginValueForFloatingTable2;
							rect.Height = clientLayoutArea.Bottom - intersectingItemBounds.Bottom;
							m_isYPositionUpdated = true;
						}
					}
				}
				else if ((textWrappingStyle != TextWrappingStyle.Tight && textWrappingStyle != TextWrappingStyle.Through) || !layouter.FloatingItems[i].IsDoesNotDenotesRectangle)
				{
					if ((textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && !(GetBaseEntity(layouter.FloatingItems[i].FloatingEntity) is HeaderFooter) && !layouter.FloatingItems[i].IsDoesNotDenotesRectangle)
					{
						float floattingItemBottom2 = GetFloattingItemBottom(layouter.FloatingItems[i].FloatingEntity, textWrappingBounds.Bottom);
						textWrappingBounds = GetBottomPositionForTightAndThrough(floattingItemBottom2, textWrappingBounds, ownerParagraph, rect.Y, size.Height);
					}
					float num24 = 0f;
					num24 = GetTopMarginValueForFloatingTable(ownerParagraph, layouter.FloatingItems[i].FloatingEntity, rect.Y);
					bool flag6 = false;
					if (flag && (verticalOrigin == VerticalOrigin.Margin || verticalOrigin == VerticalOrigin.Page) && (shapeVerticalAlignment == ShapeVerticalAlignment.Bottom || shapeVerticalAlignment == ShapeVerticalAlignment.Outside))
					{
						if (LayoutBasedOnFloatingItemTop(ownerParagraph, size, layouter.FloatingItems[i]))
						{
							float y3 = rect.Y;
							rect.Y = layouter.FloatingItems[i].TextWrappingBounds.Top - size.Height;
							rect.Height += y3 - rect.Y;
							flag6 = true;
						}
						else if (!flag3)
						{
							flag6 = true;
						}
					}
					if (layouter.ClientLayoutArea.X + size.Width < layouter.FloatingItems[i].TextWrappingBounds.X && !flag6)
					{
						RectangleF intersectingItemBounds2 = FloatingItem.GetIntersectingItemBounds(m_lcOperator as Layouter, layouter.FloatingItems[i], y);
						if (intersectingItemBounds2 != RectangleF.Empty && intersectingItemBounds2.Bottom <= textWrappingBounds.Bottom)
						{
							rect.X = clientLayoutArea.X;
							rect.Width = clientLayoutArea.Width;
							rect.Y = intersectingItemBounds2.Bottom + num24;
							rect.Height = clientLayoutArea.Bottom - intersectingItemBounds2.Bottom;
							m_isYPositionUpdated = true;
							flag6 = true;
						}
					}
					if (!flag6)
					{
						m_isYPositionUpdated = true;
						rect.Width = m_layoutArea.ClientArea.Width;
						rect.Height -= textWrappingBounds.Bottom + num24 - rect.Y;
						rect.Y = textWrappingBounds.Bottom + num24;
					}
				}
				CreateLayoutArea(rect);
				m_isWrapText = false;
				goto IL_24ad;
				IL_24ad:
				if (textWrappingType != 0)
				{
					m_isWrapText = false;
				}
				goto IL_264a;
				IL_1365:
				if (layouter.RightPositionOfTabStopInterSectingFloattingItems == float.MinValue)
				{
					float x5 = rect.X;
					TabsLayoutInfo tabsLayoutInfo2 = null;
					rect.X = textWrappingBounds.Right + (flag4 ? num7 : 0f) + num8;
					rect.Width -= num8;
					if (textWrappingStyle == TextWrappingStyle.Through && layouter.FloatingItems[i].IsDoesNotDenotesRectangle)
					{
						UpdateXposition(textWrappingBounds, i, ref rect, size, num10);
					}
					if (textWrappingStyle == TextWrappingStyle.Square && rect.Width < 0f && size.Width > 0f)
					{
						float topMarginValueForFloatingTable3 = GetTopMarginValueForFloatingTable(ownerParagraph, layouter.FloatingItems[i].FloatingEntity, rect.Y);
						m_isYPositionUpdated = true;
						rect.Width = m_layoutArea.ClientArea.Width;
						rect.Height -= textWrappingBounds.Bottom + topMarginValueForFloatingTable3 - rect.Y;
						rect.Y = textWrappingBounds.Bottom + topMarginValueForFloatingTable3;
						rect.X = x5;
					}
					else
					{
						m_isXPositionUpdated = true;
					}
					CreateLayoutArea(rect);
					if (!(leafWidget is Break))
					{
						AdjustClientAreaBasedOnExceededTab(leafWidget, size, ref rect, ownerParagraph);
					}
					if (leafWidget != null)
					{
						tabsLayoutInfo2 = leafWidget.LayoutInfo as TabsLayoutInfo;
					}
					if (tabsLayoutInfo2 == null)
					{
						m_isWrapText = true;
						CreateLayoutArea(rect);
						if (floatingEntity is WTable)
						{
							m_isWrapText = false;
						}
					}
				}
				goto IL_24ad;
			}
			if (ownerParagraph != null)
			{
				UpdateXYPositionBasedOnAdjacentFloatingItems(layouter.FloatingItems, rect, size, ownerParagraph, isFromLeafLayoutContext: true);
			}
			if (IsWord2013(ownerParagraph.Document))
			{
				FloatingItem.SortFloatingItems(layouter.FloatingItems, SortPosition.Y, isNeedToUpdateWrapCollectionIndex: true);
				FloatingItem.SortSameYPostionFloatingItems(layouter.FloatingItems, SortPosition.X);
			}
		}
		else if (IsNeedToWrapFloatingItem(LeafWidget) && !(LeafWidget as ParagraphItem).IsWrappingBoundsAdded())
		{
			RectangleF clientLayoutArea2 = layouter.ClientLayoutArea;
			FloatingItem.SortFloatingItems(layouter.WrapFloatingItems, SortPosition.Y, isNeedToUpdateWrapCollectionIndex: false);
			FloatingItem.SortSameYPostionFloatingItems(layouter.WrapFloatingItems, SortPosition.X);
			FloatingItem floatingItem = new FloatingItem();
			floatingItem.FloatingEntity = LeafWidget as Entity;
			for (int k = 0; k < layouter.WrapFloatingItems.Count; k++)
			{
				RectangleF textWrappingBounds3 = layouter.WrapFloatingItems[k].TextWrappingBounds;
				if (!(clientLayoutArea2.X > textWrappingBounds3.Right) && !(clientLayoutArea2.Right < textWrappingBounds3.X) && rect.X >= textWrappingBounds3.X && rect.X < textWrappingBounds3.Right && rect.Y >= textWrappingBounds3.Y && rect.Y < textWrappingBounds3.Bottom)
				{
					if (rect.Width < size.Width)
					{
						rect.Y = textWrappingBounds3.Bottom;
					}
					else
					{
						rect.X = textWrappingBounds3.Right;
					}
					CreateLayoutArea(rect);
				}
			}
			floatingItem.TextWrappingBounds = new RectangleF(rect.X, rect.Y, size.Width, size.Height);
			if (LeafWidget is Shape)
			{
				(LeafWidget as Shape).WrapFormat.IsWrappingBoundsAdded = true;
			}
			else if (LeafWidget is WPicture)
			{
				(LeafWidget as WPicture).IsWrappingBoundsAdded = true;
			}
			else if (LeafWidget is WChart)
			{
				(LeafWidget as WChart).WrapFormat.IsWrappingBoundsAdded = true;
			}
			else if (LeafWidget is WTextBox)
			{
				(LeafWidget as WTextBox).TextBoxFormat.IsWrappingBoundsAdded = true;
			}
			else if (LeafWidget is GroupShape)
			{
				(LeafWidget as GroupShape).WrapFormat.IsWrappingBoundsAdded = true;
			}
			(m_lcOperator as Layouter).WrapFloatingItems.Add(floatingItem);
		}
		if (num != 0f)
		{
			if (num > rect.Y)
			{
				size.Height = 0f;
			}
			rect.Y = num;
			m_layoutArea.UpdateBoundsBasedOnTextWrap(num);
		}
	}

	private bool LayoutBasedOnFloatingItemTop(WParagraph ownerPara, SizeF size, FloatingItem floatingItem)
	{
		Layouter layouter = m_lcOperator as Layouter;
		if (ownerPara != null && IsWord2013(ownerPara.Document))
		{
			return layouter.CurrentSection.PageSetup.PageSize.Height - floatingItem.TextWrappingBounds.Bottom < size.Height;
		}
		if (LeafWidget is Entity && !(LeafWidget as Entity).IsFallbackItem())
		{
			return IsLayoutOnTopBasedOnBottomEdgeExtent();
		}
		return IsLayoutOnTopBasedOnLine();
	}

	private bool IsLayoutOnTopBasedOnLine()
	{
		if (LeafWidget is Shape)
		{
			return !(LeafWidget as Shape).LineFormat.Line;
		}
		if (LeafWidget is GroupShape)
		{
			return !(LeafWidget as GroupShape).LineFormat.Line;
		}
		if (LeafWidget is WPicture)
		{
			if ((LeafWidget as WPicture).PictureShape.ShapeContainer.ShapeOptions != null)
			{
				return !(LeafWidget as WPicture).PictureShape.ShapeContainer.ShapeOptions.LineProperties.Line;
			}
			return true;
		}
		return false;
	}

	private bool IsLayoutOnTopBasedOnBottomEdgeExtent()
	{
		if (LeafWidget is Shape)
		{
			return (double)(LeafWidget as Shape).BottomEdgeExtent < 0.5;
		}
		if (LeafWidget is WTextBox)
		{
			if ((LeafWidget as WTextBox).Shape != null)
			{
				return (double)(LeafWidget as WTextBox).Shape.BottomEdgeExtent < 0.5;
			}
			return false;
		}
		if (LeafWidget is GroupShape)
		{
			return (double)(LeafWidget as GroupShape).BottomEdgeExtent < 0.5;
		}
		return false;
	}

	private bool CanWrapBreak(WParagraph owner, WParagraph fItemOwner)
	{
		Entity ownerEntity = owner.GetOwnerEntity();
		if (fItemOwner != null && (ownerEntity is WTextBox || ownerEntity is Shape))
		{
			return fItemOwner.GetOwnerEntity() == ownerEntity;
		}
		return true;
	}

	private float GetMinimumWidthRequiredForTable(WTextRange currTextRange, WTable table, float border, bool isWord2013, HorizontalPosition tableHorizontalPosition, bool isBorderValueZero)
	{
		float num = 0f;
		if (isWord2013)
		{
			if (tableHorizontalPosition == HorizontalPosition.Center)
			{
				if (isBorderValueZero)
				{
					return 18.5f + (float)Math.Round(0.375, 1);
				}
				return 18.5f + (float)Math.Round(border / 2f, 1);
			}
			if (isBorderValueZero)
			{
				return 19.25f;
			}
			return 18.5f + border;
		}
		if (tableHorizontalPosition == HorizontalPosition.Center)
		{
			if (isBorderValueZero)
			{
				return 19.25f;
			}
			return 18.5f + (float)Math.Round(border / 2f, 1);
		}
		if (border == 0.25f)
		{
			return 18.5f;
		}
		return 19.3f;
	}

	private float GetMaximumRightCellBorderWidth(WTable table)
	{
		float num = 0f;
		foreach (WTableRow row in table.Rows)
		{
			float lineWidth = row.Cells[row.Cells.Count - 1].CellFormat.Borders.Right.LineWidth;
			if (num < lineWidth)
			{
				num = lineWidth;
			}
		}
		return num;
	}

	private bool IsNeedToWrapFloatingItem(ILeafWidget leafWidget)
	{
		if ((LeafWidget is WPicture || LeafWidget is WChart || LeafWidget is Shape || LeafWidget is WTextBox || LeafWidget is GroupShape) && CheckWrappingStyleForWrapping((LeafWidget as ParagraphItem).GetTextWrappingStyle()))
		{
			return !(LeafWidget as ParagraphItem).GetAllowOverlap();
		}
		return false;
	}

	private bool IsFirstTextRangeInParagraph(ILeafWidget leafWidget)
	{
		if (leafWidget is WTextRange && (leafWidget as WTextRange).Owner is WParagraph wParagraph)
		{
			foreach (Entity childEntity in wParagraph.ChildEntities)
			{
				if (childEntity is WTextRange && childEntity as WTextRange == leafWidget)
				{
					return true;
				}
				if (!(childEntity is BookmarkStart) && !(childEntity is BookmarkEnd))
				{
					return false;
				}
			}
		}
		return false;
	}

	private bool SkipSectionBreak(WParagraph ownerPara, ILeafWidget leafWidget)
	{
		WParagraph lastParagraph = ownerPara.Document.LastParagraph;
		if (leafWidget is WTextRange && (leafWidget as WTextRange).OwnerParagraph == null && ownerPara.ChildEntities.Count > 0 && ownerPara.IsContainFloatingItems() && ownerPara.IsParagraphHasSectionBreak() && lastParagraph != ownerPara)
		{
			return true;
		}
		return false;
	}

	internal void UpdateParaFirstLineHorizontalPositions(ParagraphLayoutInfo paragraphInfo, IWidget widget, ref float x, ref float width)
	{
		float num = paragraphInfo.FirstLineIndent;
		float listTab = paragraphInfo.ListTab;
		if (!(widget is SplitWidgetContainer))
		{
			if (paragraphInfo.LevelNumber != -1)
			{
				num += listTab;
			}
			x += num;
			width -= num;
		}
		WParagraph wParagraph = ((widget is SplitWidgetContainer) ? ((widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null);
		if (wParagraph != null)
		{
			int count = wParagraph.ChildEntities.Count;
			int count2 = (widget as SplitWidgetContainer).Count;
			if (!wParagraph.IsInCell && count > count2 && wParagraph.ChildEntities[count - 1 - count2] is Break && wParagraph.ChildEntities[count - 1 - count2] is Break @break && (@break.BreakType == BreakType.PageBreak || @break.BreakType == BreakType.ColumnBreak))
			{
				x += num + listTab;
				width -= num + listTab;
			}
		}
	}

	private bool SkipBookmark(SizeF size)
	{
		if (LeafWidget is BookmarkStart || LeafWidget is BookmarkEnd)
		{
			if (size.Width <= 0f)
			{
				return size.Height <= 0f;
			}
			return false;
		}
		return false;
	}

	private bool IsTextFitBelow(RectangleF wrappingBounds, float textHeight, Entity entity)
	{
		if (entity is WTextBox || entity is Shape || entity is GroupShape)
		{
			float num = ((entity is WTextBox) ? (entity as WTextBox).TextBoxFormat.LineWidth : ((entity is Shape) ? (entity as Shape).LineFormat.Weight : (entity as GroupShape).LineFormat.Weight));
			int num2 = 2;
			double num3 = Math.Floor(num);
			do
			{
				if ((num3 - (double)num2) % 3.0 == 0.0 || num3 <= 0.0)
				{
					double value = ((num3 > 2.0) ? ((double)textHeight + (num3 - (double)num2) / 3.0 * 1.5) : ((double)textHeight));
					double value2 = ((num3 > (double)num2) ? ((double)textHeight + (num3 - (double)num2) / 3.0 * 1.5 + 1.4700000286102295) : ((double)(textHeight + 1.47f)));
					if (num3 <= (double)num2 && (double)wrappingBounds.Y > Math.Round(value, 2) && (double)wrappingBounds.Y < Math.Round(value2, 2) && num > (float)num2)
					{
						return true;
					}
					if (num3 != 0.0 && (double)wrappingBounds.Y > Math.Round(value, 2) && (double)wrappingBounds.Y < Math.Round(value2, 2) && (double)num > num3)
					{
						return true;
					}
					return false;
				}
				num3 -= 1.0;
			}
			while (num3 > 0.0);
		}
		return false;
	}

	private float GetTopMarginValueForFloatingTable(WParagraph paragraph, Entity floatingEntity, float yPosition)
	{
		ParagraphLayoutInfo paragraphLayoutInfo = null;
		if (paragraph != null)
		{
			paragraphLayoutInfo = ((IWidget)paragraph).LayoutInfo as ParagraphLayoutInfo;
		}
		if (paragraphLayoutInfo != null && paragraphLayoutInfo.IsFirstLine && yPosition > paragraphLayoutInfo.YPosition && floatingEntity is WTable && (!(floatingEntity as WTable).IsFrame || paragraph.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013))
		{
			return yPosition - paragraphLayoutInfo.YPosition;
		}
		return 0f;
	}

	private void UpdateXposition(RectangleF textWrappingBounds, int i, ref RectangleF rect, SizeF size, float minwidth)
	{
		bool flag = true;
		Layouter layouter = m_lcOperator as Layouter;
		while (flag)
		{
			textWrappingBounds = AdjustTightAndThroughBounds(layouter.FloatingItems[i], rect, size.Height);
			if (textWrappingBounds.X != 0f && textWrappingBounds.X != layouter.FloatingItems[i].TextWrappingBounds.X)
			{
				rect.Width = textWrappingBounds.X - rect.X;
				if (rect.Width > minwidth)
				{
					flag = false;
					continue;
				}
				rect.X = textWrappingBounds.Right;
				rect.Width = m_layoutArea.ClientActiveArea.Right - textWrappingBounds.Right;
			}
			else
			{
				flag = false;
			}
		}
	}

	private void AdjustClientAreaBasedOnExceededTab(ILeafWidget leafWidget, SizeF size, ref RectangleF rect, WParagraph paragraph)
	{
		Entity entity = null;
		Layouter layouter = m_lcOperator as Layouter;
		float num = (((IWidget)paragraph).LayoutInfo as ParagraphLayoutInfo).Margins.Left;
		if (leafWidget.LayoutInfo is TabsLayoutInfo)
		{
			return;
		}
		entity = ((!(leafWidget is SplitStringWidget)) ? ((leafWidget as Entity).PreviousSibling as Entity) : (((leafWidget as SplitStringWidget).RealStringWidget as Entity).PreviousSibling as Entity));
		if (layouter.RightPositionOfTabStopInterSectingFloattingItems == float.MinValue && paragraph.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
		{
			while (entity != null && ((entity as IWidget).LayoutInfo.IsSkip || entity is BookmarkStart || entity is BookmarkEnd))
			{
				entity = entity.PreviousSibling as Entity;
			}
			TabsLayoutInfo tabsLayoutInfo = null;
			if (entity != null)
			{
				tabsLayoutInfo = (entity as ILeafWidget).LayoutInfo as TabsLayoutInfo;
			}
			if (tabsLayoutInfo != null)
			{
				layouter.RightPositionOfTabStopInterSectingFloattingItems = rect.X - num;
				rect.X = layouter.PreviousTabWidth + GetPararaphLeftIndent() + Layouter.GetLeftMargin(layouter.CurrentSection as WSection);
				rect.Width = (float)Math.Round(1584f - layouter.PreviousTabWidth - GetPararaphLeftIndent());
				layouter.MaxRightPositionOfTabStopInterSectingFloattingItems = rect.Right;
				base.IsTabStopBeyondRightMarginExists = true;
			}
		}
	}

	private float GetMinWidth(WTextRange currTextRange, SizeF size, RectangleF rect)
	{
		string text = GetText();
		string[] array = text.Split(' ');
		if (text != string.Empty && text.Trim() == string.Empty && currTextRange != null && currTextRange.OwnerParagraph != null && currTextRange.PreviousSibling == null && currTextRange.NextSibling != null && currTextRange.OwnerParagraph.ChildEntities.IndexOf(currTextRange) != -1 && currTextRange.OwnerParagraph.Text.Trim() != string.Empty)
		{
			array = new string[2] { text, "" };
		}
		string text2 = array[0];
		if (text2 == string.Empty)
		{
			text2 = " ";
		}
		float num = base.DrawingContext.MeasureTextRange(currTextRange, text2).Width;
		if (base.DrawingContext.IsUnicodeText(text))
		{
			num = base.DrawingContext.MeasureTextRange(currTextRange, text[0].ToString()).Width;
		}
		WTextRange wTextRange = GetNextSibling(currTextRange) as WTextRange;
		if (array.Length == 1 && wTextRange != null && !StringParser.IsUnicodeChineseText(text))
		{
			string nextSiblingText = wTextRange.Text;
			num += GetNextTextRangeWidth(wTextRange, ref nextSiblingText, size, rect);
		}
		else if (currTextRange != null && currTextRange.OwnerParagraph == null && wTextRange == null)
		{
			WParagraph wParagraph = currTextRange.CharacterFormat.BaseFormat.OwnerBase as WParagraph;
			ParagraphLayoutInfo paragraphLayoutInfo = null;
			num += ((wParagraph != null && ((IWidget)wParagraph).LayoutInfo is ParagraphLayoutInfo { Size: var size2 }) ? size2.Width : 0f);
		}
		return num;
	}

	private bool IsLeafWidgetOwnerIsTextBox()
	{
		Entity entity = GetOwnerParagraph();
		while (entity != null)
		{
			if (entity.EntityType == EntityType.HeaderFooter || entity.EntityType == EntityType.Section || entity.Owner == null)
			{
				return false;
			}
			entity = entity.Owner;
			WTextBox wTextBox = null;
			if (entity is WTextBox && ((wTextBox = entity as WTextBox).TextBoxFormat.TextWrappingStyle == TextWrappingStyle.InFrontOfText || wTextBox.TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Behind))
			{
				return true;
			}
		}
		return false;
	}

	private SizeF GetPageFieldSize(WField field)
	{
		IWSection currentSection = (m_lcOperator as Layouter).CurrentSection;
		string text = "";
		bool pageFieldHasFormatText = false;
		string fieldCode = field.FieldCode;
		if (fieldCode.Contains("\\"))
		{
			text = field.UpdateTextFormat(((m_lcOperator as Layouter).PageNumber + 1).ToString(), fieldCode.Substring(fieldCode.IndexOf("\\")), ref pageFieldHasFormatText);
		}
		if (!pageFieldHasFormatText)
		{
			text = currentSection.PageSetup.GetNumberFormatValue((byte)currentSection.PageSetup.PageNumberStyle, (m_lcOperator as Layouter).PageNumber + 1);
		}
		WCharacterFormat characterFormatValue = field.GetCharacterFormatValue();
		return base.DrawingContext.MeasureString(text, characterFormatValue.GetFontToRender(field.ScriptType), null, characterFormatValue, isMeasureFromTabList: false, field.ScriptType);
	}

	protected override void DoLayoutAfter()
	{
		FieldLayoutInfo obj = base.LayoutInfo as FieldLayoutInfo;
		Layouter layouter = m_lcOperator as Layouter;
		bool num = obj != null;
		if (base.Widget is ParagraphItem && layouter.tocParaItems.Contains(base.Widget as ParagraphItem) && !layouter.IsLayoutingHeaderRow && m_ltWidget != null && layouter.LayoutingTOC == null)
		{
			m_lcOperator.SendLeafLayoutAfter(m_ltWidget, isFromTOCLinkStyle: true);
		}
		if (num && m_ltWidget != null && !layouter.IsLayoutingHeaderRow && layouter.LayoutingTOC == null)
		{
			m_lcOperator.SendLeafLayoutAfter(m_ltWidget, isFromTOCLinkStyle: false);
		}
		if (m_ltWidget != null && m_ltWidget.Widget is BookmarkStart && !StartsWithExt((m_ltWidget.Widget as BookmarkStart).Name, "_"))
		{
			m_lcOperator.SendLeafLayoutAfter(m_ltWidget, isFromTOCLinkStyle: false);
		}
	}

	private SizeF UpdateEQFieldWidth(DrawingContext dc, WCharacterFormat charFormat)
	{
		for (int i = 0; i < DocumentLayouter.EquationFields.Count; i++)
		{
			if (DocumentLayouter.EquationFields[i].EQFieldEntity == LeafWidget)
			{
				LayoutedEQFields layoutedEQFields = new LayoutedEQFields();
				dc.GenerateErrorFieldCode(layoutedEQFields, 0f, 0f, charFormat);
				DocumentLayouter.EquationFields[i].LayouttedEQField = layoutedEQFields;
				LeafWidget.LayoutInfo.Size = layoutedEQFields.Bounds.Size;
			}
		}
		return LeafWidget.LayoutInfo.Size;
	}

	private void FitWidget(SizeF size, IWidget widget, bool isLastWordFit, float indentX, float indentY, bool isFloatingItem)
	{
		TabsLayoutInfo tabsLayoutInfo = widget.LayoutInfo as TabsLayoutInfo;
		ILayoutSpacingsInfo layoutSpacingsInfo = base.LayoutInfo as ILayoutSpacingsInfo;
		Layouter layouter = m_lcOperator as Layouter;
		if (tabsLayoutInfo != null)
		{
			if (tabsLayoutInfo.IsTabWidthUpdatedBasedOnIndent)
			{
				layouter.PreviousTab = new TabsLayoutInfo.LayoutTab((widget as ParagraphItem).OwnerParagraph.ParagraphFormat.LeftIndent, TabJustification.Left, TabLeader.NoLeader);
			}
			else
			{
				(m_lcOperator as Layouter).PreviousTab = tabsLayoutInfo.m_currTab;
			}
		}
		double num = size.Width + ((layoutSpacingsInfo != null) ? (layoutSpacingsInfo.Paddings.Left + layoutSpacingsInfo.Paddings.Right) : 0f);
		double num2 = size.Height + ((layoutSpacingsInfo != null) ? (layoutSpacingsInfo.Paddings.Top + layoutSpacingsInfo.Paddings.Bottom) : 0f);
		ParagraphItem paragraphItem = ((LeafWidget is WOleObject) ? (LeafWidget as WOleObject).OlePicture : (LeafWidget as ParagraphItem));
		WPicture wPicture = paragraphItem as WPicture;
		if (!isLastWordFit)
		{
			num = UpdateLeafWidgetWidth(num, widget);
		}
		ParagraphItem paragraphItem2 = widget as ParagraphItem;
		if (widget is SplitStringWidget)
		{
			paragraphItem2 = (widget as SplitStringWidget).RealStringWidget as ParagraphItem;
		}
		WParagraph wParagraph = paragraphItem2.OwnerParagraph;
		if (paragraphItem2.Owner is InlineContentControl || paragraphItem2.Owner is XmlParagraphItem)
		{
			wParagraph = paragraphItem2.GetOwnerParagraphValue();
		}
		bool isInCell = false;
		bool isInTextBox = false;
		if (wParagraph != null)
		{
			isInCell = wParagraph.IsInCell;
			isInTextBox = GetBaseEntity(wParagraph) is WTextBox;
		}
		if (((wPicture == null && !(paragraphItem is Shape) && !(paragraphItem is WTextBox) && !(paragraphItem is GroupShape) && !(paragraphItem is WChart)) || IsFitLeafWidgetInContainerHeight(paragraphItem, isInCell, isInTextBox, null)) && num2 > (double)m_layoutArea.ClientArea.Height && (wParagraph == null || !wParagraph.IsZeroAutoLineSpace()) && !base.IsForceFitLayout)
		{
			num2 = m_layoutArea.ClientArea.Height;
		}
		if (base.LayoutInfo.IsVerticalText && wPicture != null && wPicture.TextWrappingStyle == TextWrappingStyle.Inline && num2 > (double)m_layoutArea.ClientArea.Width)
		{
			CellLayoutInfo cellLayoutInfo = ((IWidget)wParagraph.OwnerTextBody).LayoutInfo as CellLayoutInfo;
			num2 = m_layoutArea.ClientArea.Width + (((IWidget)wParagraph).LayoutInfo as ParagraphLayoutInfo).Margins.Right - 2f * (cellLayoutInfo.Margins.Top + cellLayoutInfo.Margins.Bottom) - (cellLayoutInfo.Margins.Left + cellLayoutInfo.Margins.Right);
		}
		m_ltWidget = new LayoutedWidget(widget);
		ILayoutSpacingsInfo layoutSpacingsInfo2 = base.LayoutInfo as ILayoutSpacingsInfo;
		if (!isFloatingItem)
		{
			m_ltWidget.Bounds = new RectangleF(m_layoutArea.ClientArea.X - (layoutSpacingsInfo2?.Paddings.Left ?? 0f) + indentX, m_layoutArea.ClientArea.Y - (layoutSpacingsInfo2?.Paddings.Top ?? 0f) + indentY, (float)num, (float)num2);
		}
		else
		{
			m_ltWidget.Bounds = new RectangleF(indentX - (layoutSpacingsInfo2?.Paddings.Left ?? 0f), indentY - (layoutSpacingsInfo2?.Paddings.Top ?? 0f), (float)num, (float)num2);
		}
		if (!(m_ltWidget.Widget is WTextBox))
		{
			m_ltWidget.PrevTabJustification = layouter.PreviousTab.Justification;
		}
		if (isLastWordFit)
		{
			m_ltWidget.TextTag = "IsLastWordFit";
		}
	}

	private bool GetFloattingItemPosition(ref float indentX, ref float indentY, ref SizeF size, float shiftDistance)
	{
		float num = indentY;
		Layouter layouter = m_lcOperator as Layouter;
		bool result = false;
		ParagraphItem paragraphItem = ((LeafWidget is WOleObject) ? (LeafWidget as WOleObject).OlePicture : (LeafWidget as ParagraphItem));
		Shape shape = paragraphItem as Shape;
		WPicture wPicture = paragraphItem as WPicture;
		WTextBox wTextBox = paragraphItem as WTextBox;
		WChart wChart = paragraphItem as WChart;
		GroupShape groupShape = paragraphItem as GroupShape;
		float num2 = shape?.RightEdgeExtent ?? groupShape?.RightEdgeExtent ?? ((wTextBox != null && wTextBox.Shape != null) ? wTextBox.Shape.RightEdgeExtent : 0f);
		float num3 = shape?.LeftEdgeExtent ?? groupShape?.LeftEdgeExtent ?? ((wTextBox != null && wTextBox.Shape != null) ? wTextBox.Shape.LeftEdgeExtent : 0f);
		float num4 = shape?.TopEdgeExtent ?? groupShape?.TopEdgeExtent ?? ((wTextBox != null && wTextBox.Shape != null) ? wTextBox.Shape.TopEdgeExtent : 0f);
		float num5 = shape?.BottomEdgeExtent ?? groupShape?.BottomEdgeExtent ?? ((wTextBox != null && wTextBox.Shape != null) ? wTextBox.Shape.BottomEdgeExtent : 0f);
		if (paragraphItem is WPicture || paragraphItem is Shape || paragraphItem is WTextBox || paragraphItem is WChart || paragraphItem is GroupShape)
		{
			result = true;
			float num6 = 0f;
			float num7 = 0f;
			float num8 = 0f;
			float num9 = 0f;
			float num10 = 0f;
			float num11 = 0f;
			IWSection currentSection = layouter.CurrentSection;
			float num12 = 0f;
			float num13 = 0f;
			float num14 = 0f;
			float num15 = 0f;
			WParagraph ownerParagraphValue = paragraphItem.GetOwnerParagraphValue();
			ParagraphLayoutInfo paragraphLayoutInfo = null;
			TextWrappingStyle textWrappingStyle = shape?.WrapFormat.TextWrappingStyle ?? wPicture?.TextWrappingStyle ?? wChart?.WrapFormat.TextWrappingStyle ?? wTextBox?.TextBoxFormat.TextWrappingStyle ?? groupShape.WrapFormat.TextWrappingStyle;
			if (ownerParagraphValue != null)
			{
				paragraphLayoutInfo = ((IWidget)ownerParagraphValue).LayoutInfo as ParagraphLayoutInfo;
			}
			if (paragraphItem.Owner != null)
			{
				num6 = Layouter.GetLeftMargin(currentSection as WSection);
				num11 = Layouter.GetRightMargin(currentSection as WSection);
				if (!layouter.IsLayoutingHeaderFooter)
				{
					num7 = ((layouter.CurrentSection.HeadersFooters.Header.Count == 0 || !(currentSection.PageSetup.Margins.Top > 0f)) ? Math.Abs(currentSection.PageSetup.Margins.Top) : currentSection.PageSetup.HeaderDistance);
				}
				else
				{
					num7 = ((!layouter.IsLayoutingHeader || !(currentSection.PageSetup.Margins.Top <= 0f)) ? ((currentSection.PageSetup.Margins.Top > 0f) ? currentSection.PageSetup.Margins.Top : 36f) : ((Math.Abs(currentSection.PageSetup.Margins.Top) > 0f) ? Math.Abs(currentSection.PageSetup.Margins.Top) : (currentSection.PageSetup.HeaderDistance + (paragraphLayoutInfo?.Size.Height ?? 0f))));
					if (!layouter.IsLayoutingHeader && textWrappingStyle != 0 && paragraphItem.GetVerticalOrigin() == VerticalOrigin.Margin && !ownerParagraphValue.IsInCell && num7 < layouter.GetCurrentPageHeaderHeight())
					{
						num7 = layouter.GetCurrentPageHeaderHeight();
					}
				}
				if (currentSection.PageSetup.Margins.Gutter > 0f && currentSection.Document.DOP.GutterAtTop)
				{
					num7 += currentSection.PageSetup.Margins.Gutter;
				}
				if (!layouter.IsLayoutingHeaderFooter && num7 < layouter.ClientLayoutArea.Y && Math.Round(layouter.m_firstItemInPageYPosition, 2) >= Math.Round(layouter.ClientLayoutArea.Y, 2))
				{
					num7 = layouter.ClientLayoutArea.Y;
				}
				num8 = ((currentSection.PageSetup.Margins.Bottom > 0f) ? currentSection.PageSetup.Margins.Bottom : 36f);
				num13 = currentSection.PageSetup.PageSize.Height;
				num12 = currentSection.PageSetup.PageSize.Width;
				num14 = currentSection.PageSetup.ClientWidth;
				num10 = currentSection.PageSetup.FooterDistance;
				num9 = currentSection.PageSetup.HeaderDistance;
				num15 = currentSection.PageSetup.PageSize.Height - num7 - num8;
			}
			bool flag = false;
			CellLayoutInfo cellLayoutInfo = null;
			float num16 = 0f;
			if (textWrappingStyle != 0)
			{
				VerticalOrigin verticalOrigin = paragraphItem.GetVerticalOrigin();
				HorizontalOrigin horizontalOrigin = paragraphItem.GetHorizontalOrigin();
				ShapeHorizontalAlignment shapeHorizontalAlignment = paragraphItem.GetShapeHorizontalAlignment();
				ShapeVerticalAlignment shapeVerticalAlignment = paragraphItem.GetShapeVerticalAlignment();
				float height = size.Height;
				if (shape != null && shape.IsHorizontalRule && size.Width > m_layoutArea.ClientActiveArea.Width)
				{
					size.Width = m_layoutArea.ClientActiveArea.Width;
				}
				float width = size.Width;
				float verticalPosition = paragraphItem.GetVerticalPosition();
				float horizontalPosition = paragraphItem.GetHorizontalPosition();
				bool flag2 = shape?.LayoutInCell ?? wPicture?.LayoutInCell ?? wChart?.LayoutInCell ?? groupShape?.LayoutInCell ?? (wTextBox == null || ((wTextBox.Shape != null) ? wTextBox.Shape.LayoutInCell : wTextBox.TextBoxFormat.AllowInCell));
				if (ownerParagraphValue.IsInCell && (flag2 || ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013))
				{
					flag = true;
					cellLayoutInfo = GetCellLayoutInfo(ownerParagraphValue);
					num16 = base.DrawingContext.GetCellWidth(paragraphItem);
					indentY = (float)GetVerticalPosition(paragraphItem, verticalPosition, verticalOrigin, textWrappingStyle, cellLayoutInfo);
					indentX = (float)GetHorizontalPosition(size, paragraphItem, shapeHorizontalAlignment, horizontalOrigin, horizontalPosition, textWrappingStyle, num16, num2, num3, num6);
				}
				else
				{
					if (m_isYPositionUpdated)
					{
						indentY = m_layoutArea.ClientArea.Y;
					}
					else
					{
						switch (verticalOrigin)
						{
						case VerticalOrigin.Page:
						case VerticalOrigin.TopMargin:
							indentY = verticalPosition;
							switch (shapeVerticalAlignment)
							{
							case ShapeVerticalAlignment.Top:
								indentY = verticalPosition + num4;
								break;
							case ShapeVerticalAlignment.Center:
								if (verticalOrigin == VerticalOrigin.TopMargin)
								{
									indentY = (num7 - height) / 2f;
								}
								else
								{
									indentY = (num13 - height) / 2f;
								}
								break;
							case ShapeVerticalAlignment.Bottom:
							case ShapeVerticalAlignment.Outside:
								if (verticalOrigin == VerticalOrigin.Page && shapeVerticalAlignment == ShapeVerticalAlignment.Bottom)
								{
									indentY = ((shiftDistance == 0f) ? (num13 - height - num5) : (num - shiftDistance - num5));
								}
								else if (verticalOrigin == VerticalOrigin.TopMargin)
								{
									indentY = num7 - height;
								}
								else if (layouter.CurrPageIndex % 2 != 0)
								{
									indentY = num13 - height - num10 / 2f;
								}
								else
								{
									indentY = num9 / 2f;
								}
								break;
							case ShapeVerticalAlignment.Inside:
								if (verticalOrigin == VerticalOrigin.Page)
								{
									if (layouter.CurrPageIndex % 2 == 0)
									{
										indentY = num13 - height - num10 / 2f;
									}
									else
									{
										indentY = num9 / 2f;
									}
								}
								break;
							case ShapeVerticalAlignment.None:
							{
								if (!(paragraphItem is WTextBox) && !(paragraphItem is Shape))
								{
									break;
								}
								Shape shape2 = paragraphItem as Shape;
								float num27 = shape2?.TextFrame.VerticalRelativePercent ?? (paragraphItem as WTextBox).TextBoxFormat.VerticalRelativePercent;
								if (Math.Abs(num27) <= 1000f)
								{
									if (verticalOrigin == VerticalOrigin.Page)
									{
										indentY = num13 * (num27 / 100f);
									}
									else
									{
										indentY = num7 * (num27 / 100f);
									}
								}
								else
								{
									indentY = shape2?.VerticalPosition ?? (paragraphItem as WTextBox).TextBoxFormat.VerticalPosition;
								}
								break;
							}
							}
							break;
						case VerticalOrigin.Line:
							indentY = verticalPosition;
							switch (shapeVerticalAlignment)
							{
							case ShapeVerticalAlignment.Top:
							case ShapeVerticalAlignment.Inside:
								indentY = m_layoutArea.ClientActiveArea.Y;
								break;
							case ShapeVerticalAlignment.Center:
							{
								if (ownerParagraphValue.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
								{
									indentY = m_layoutArea.ClientActiveArea.Y - height / 2f;
									break;
								}
								float num19 = ((IWidget)ownerParagraphValue).LayoutInfo.Size.Height;
								if (ownerParagraphValue.ParagraphFormat.LineSpacing > 12f)
								{
									num19 *= ownerParagraphValue.ParagraphFormat.LineSpacing / 12f;
								}
								int num20 = IsParagraphContainingLineBreakInFirst(ownerParagraphValue);
								if (num20 == int.MinValue)
								{
									float num21 = num19 + ownerParagraphValue.ParagraphFormat.BeforeSpacing + ownerParagraphValue.ParagraphFormat.AfterSpacing;
									num21 /= 2f;
									indentY = m_layoutArea.ClientActiveArea.Y - ownerParagraphValue.ParagraphFormat.BeforeSpacing + num21 - height / 2f;
								}
								else
								{
									float num22 = num19 + ownerParagraphValue.ParagraphFormat.AfterSpacing;
									num22 /= 2f;
									num22 = ((num20 != 0) ? (num19 / 2f) : (num22 + (ownerParagraphValue.ChildEntities[0] as ILeafWidget).Measure(base.DrawingContext).Height));
									indentY = (float)Math.Round(m_layoutArea.ClientActiveArea.Y + num22 - height / 2f);
								}
								break;
							}
							case ShapeVerticalAlignment.Bottom:
							case ShapeVerticalAlignment.Outside:
							{
								if (ownerParagraphValue.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
								{
									indentY = m_layoutArea.ClientActiveArea.Y - height;
									break;
								}
								float num19 = ((IWidget)ownerParagraphValue).LayoutInfo.Size.Height;
								if (ownerParagraphValue.ParagraphFormat.LineSpacing > 12f)
								{
									num19 *= ownerParagraphValue.ParagraphFormat.LineSpacing / 12f;
								}
								indentY = m_layoutArea.ClientActiveArea.Y + (num19 + ownerParagraphValue.ParagraphFormat.AfterSpacing - height);
								if (indentY < layouter.ClientLayoutArea.Y)
								{
									indentY = layouter.ClientLayoutArea.Y;
								}
								break;
							}
							case ShapeVerticalAlignment.None:
								indentY = m_layoutArea.ClientActiveArea.Y + verticalPosition;
								break;
							}
							break;
						case VerticalOrigin.BottomMargin:
							indentY = verticalPosition;
							switch (shapeVerticalAlignment)
							{
							case ShapeVerticalAlignment.Top:
							case ShapeVerticalAlignment.Inside:
								indentY = num13 - num8;
								break;
							case ShapeVerticalAlignment.Center:
								indentY = num13 - num8 + (num8 - height) / 2f;
								break;
							case ShapeVerticalAlignment.Bottom:
							case ShapeVerticalAlignment.Outside:
								if (layouter.CurrPageIndex % 2 != 0)
								{
									indentY = num13 - height;
								}
								else
								{
									indentY = num13 - num8;
								}
								break;
							case ShapeVerticalAlignment.None:
								indentY = num13 - num8 + verticalPosition;
								if (paragraphItem is WTextBox && Math.Abs((paragraphItem as WTextBox).TextBoxFormat.VerticalRelativePercent) <= 1000f)
								{
									float num23 = num13 - num8;
									float verticalRelativePercent2 = (paragraphItem as WTextBox).TextBoxFormat.VerticalRelativePercent;
									float num24 = num8 * (verticalRelativePercent2 / 100f);
									indentY = num23 + num24;
								}
								break;
							}
							break;
						case VerticalOrigin.InsideMargin:
						case VerticalOrigin.OutsideMargin:
							indentY = verticalPosition;
							switch (shapeVerticalAlignment)
							{
							case ShapeVerticalAlignment.Inside:
								if (verticalOrigin == VerticalOrigin.InsideMargin)
								{
									if (layouter.CurrPageIndex % 2 == 0)
									{
										indentY = num13 - height;
									}
									else
									{
										indentY = 0f;
									}
								}
								else
								{
									indentY = ((layouter.CurrPageIndex % 2 == 0) ? ((ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? (num7 - height) : (num13 - height)) : ((ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? (num13 - num8) : 0f));
								}
								break;
							case ShapeVerticalAlignment.Top:
								if (verticalOrigin == VerticalOrigin.InsideMargin)
								{
									if (layouter.CurrPageIndex % 2 == 0)
									{
										indentY = num13 - num8;
									}
									else
									{
										indentY = 0f;
									}
								}
								else
								{
									indentY = ((layouter.CurrPageIndex % 2 == 0) ? ((ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? 0f : (num13 - num8)) : ((ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? (num13 - num8) : 0f));
								}
								break;
							case ShapeVerticalAlignment.Center:
								if (verticalOrigin == VerticalOrigin.OutsideMargin)
								{
									indentY = ((layouter.CurrPageIndex % 2 == 0) ? ((ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? (indentY = (num7 - height) / 2f) : (indentY = num13 - num8 + (num8 - height) / 2f)) : ((ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? (num13 - num8 + (num8 - height) / 2f) : ((num7 - height) / 2f)));
								}
								else if (layouter.CurrPageIndex % 2 == 0)
								{
									indentY = num13 - num8 + (num8 - height) / 2f;
								}
								else
								{
									indentY = (num7 - height) / 2f;
								}
								break;
							case ShapeVerticalAlignment.Outside:
								if (verticalOrigin == VerticalOrigin.InsideMargin)
								{
									if (layouter.CurrPageIndex % 2 == 0)
									{
										indentY = num13 - num8;
									}
									else
									{
										indentY = num7 - height;
									}
								}
								else
								{
									indentY = ((layouter.CurrPageIndex % 2 == 0) ? ((ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? 0f : (num13 - num8)) : ((ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? (num13 - height) : (num7 - height)));
								}
								break;
							case ShapeVerticalAlignment.Bottom:
								if (verticalOrigin == VerticalOrigin.OutsideMargin)
								{
									indentY = ((layouter.CurrPageIndex % 2 == 0) ? ((ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? (num7 - height) : (num13 - height)) : ((ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? (num13 - height) : (num7 - height)));
								}
								else if (layouter.CurrPageIndex % 2 == 0)
								{
									indentY = num13 - height;
								}
								else
								{
									indentY = num7 - height;
								}
								break;
							case ShapeVerticalAlignment.None:
								if (paragraphItem is WTextBox && Math.Abs((paragraphItem as WTextBox).TextBoxFormat.VerticalRelativePercent) <= 1000f)
								{
									float verticalRelativePercent = (paragraphItem as WTextBox).TextBoxFormat.VerticalRelativePercent;
									if ((layouter.CurrPageIndex % 2 == 0) ? (verticalOrigin == VerticalOrigin.InsideMargin) : (verticalOrigin == VerticalOrigin.OutsideMargin))
									{
										float num17 = num13 - num8;
										float num18 = num8 * (verticalRelativePercent / 100f);
										indentY = num17 + num18;
									}
									else
									{
										indentY = num7 * (verticalRelativePercent / 100f);
									}
								}
								break;
							}
							break;
						case VerticalOrigin.Paragraph:
						{
							float num26 = 0f;
							if (shape != null || wPicture != null)
							{
								num26 = GetFloatingItemSpacing(ownerParagraphValue);
							}
							indentY = (float)Math.Round((m_lcOperator as Layouter).ParagraphYPosition, 2) + num26 + verticalPosition;
							break;
						}
						case VerticalOrigin.Margin:
							if (layouter.IsLayoutingHeader && num9 > num7)
							{
								indentY = num9 + (paragraphLayoutInfo?.Size.Height ?? 0f) + verticalPosition;
							}
							else
							{
								indentY = num7 + verticalPosition;
							}
							switch (shapeVerticalAlignment)
							{
							case ShapeVerticalAlignment.Top:
								indentY = num7 + num4;
								break;
							case ShapeVerticalAlignment.Center:
								indentY = num7 + (num15 - height) / 2f;
								break;
							case ShapeVerticalAlignment.Bottom:
							case ShapeVerticalAlignment.Outside:
								if (layouter.CurrPageIndex % 2 != 0 || (ownerParagraphValue != null && ownerParagraphValue.Document.MultiplePage != MultiplePage.MirrorMargins))
								{
									indentY = ((shiftDistance == 0f) ? (layouter.ClientLayoutArea.Bottom - height - num5) : (num - shiftDistance - num5));
								}
								else
								{
									indentY = num7;
								}
								break;
							case ShapeVerticalAlignment.Inside:
								if (layouter.CurrPageIndex % 2 == 0 || (ownerParagraphValue != null && ownerParagraphValue.Document.MultiplePage != MultiplePage.MirrorMargins))
								{
									indentY = num7 + num15 - height;
								}
								else
								{
									indentY = num7;
								}
								break;
							case ShapeVerticalAlignment.None:
								if (paragraphItem is WTextBox && Math.Abs((paragraphItem as WTextBox).TextBoxFormat.VerticalRelativePercent) <= 1000f)
								{
									float verticalRelativePercent3 = (paragraphItem as WTextBox).TextBoxFormat.VerticalRelativePercent;
									float num25 = num15 * (verticalRelativePercent3 / 100f);
									indentY = num7 + num25;
								}
								break;
							}
							break;
						default:
							indentY = m_layoutArea.ClientArea.Y - ((base.LayoutInfo is ILayoutSpacingsInfo) ? (base.LayoutInfo as ILayoutSpacingsInfo).Paddings.Top : 0f) + verticalPosition;
							break;
						}
					}
					if (m_isXPositionUpdated && horizontalOrigin != HorizontalOrigin.Column && shapeHorizontalAlignment != 0)
					{
						indentX = m_layoutArea.ClientArea.X;
					}
					else if (ownerParagraphValue != null && ownerParagraphValue.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind && verticalOrigin == VerticalOrigin.Paragraph && width >= num12)
					{
						indentX = 0f;
					}
					else
					{
						switch (horizontalOrigin)
						{
						case HorizontalOrigin.Page:
							indentX = horizontalPosition;
							switch (shapeHorizontalAlignment)
							{
							case ShapeHorizontalAlignment.Center:
								if (flag)
								{
									indentX = (base.DrawingContext.GetCellWidth(paragraphItem) - width) / 2f;
								}
								else
								{
									indentX = (num12 - width) / 2f;
								}
								break;
							case ShapeHorizontalAlignment.Left:
								indentX = 0f + num3;
								break;
							case ShapeHorizontalAlignment.Right:
							case ShapeHorizontalAlignment.Outside:
								if (flag)
								{
									indentX = base.DrawingContext.GetCellWidth(paragraphItem) - width;
								}
								else
								{
									indentX = num12 - width;
								}
								indentX -= num2;
								break;
							case ShapeHorizontalAlignment.None:
								if (flag)
								{
									indentX = (((IWidget)ownerParagraphValue.OwnerTextBody).LayoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Left + horizontalPosition;
								}
								else if (paragraphItem is WTextBox || paragraphItem is Shape)
								{
									Shape shape3 = paragraphItem as Shape;
									float num31 = shape3?.TextFrame.HorizontalRelativePercent ?? (paragraphItem as WTextBox).TextBoxFormat.HorizontalRelativePercent;
									if (Math.Abs(num31) <= 1000f)
									{
										indentX = num12 * (num31 / 100f);
									}
									else
									{
										indentX = shape3?.HorizontalPosition ?? (paragraphItem as WTextBox).TextBoxFormat.HorizontalPosition;
									}
								}
								else
								{
									indentX = horizontalPosition;
								}
								break;
							}
							if (indentX < 0f && flag)
							{
								indentX = (((IWidget)ownerParagraphValue.OwnerTextBody).LayoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Left;
							}
							break;
						case HorizontalOrigin.Column:
							if (!ownerParagraphValue.IsXpositionUpated || layouter.ClientLayoutArea.Left < paragraphLayoutInfo.XPosition)
							{
								float num29 = 0f;
								if (ownerParagraphValue.IsInCell)
								{
									CellLayoutInfo cellLayoutInfo2 = (ownerParagraphValue.GetOwnerEntity() as IWidget).LayoutInfo as CellLayoutInfo;
									num29 = cellLayoutInfo2.Paddings.Left + cellLayoutInfo2.Paddings.Right;
								}
								float num30 = 18f;
								if (textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through)
								{
									num30 = ((ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? 17.6f : 8f);
								}
								num30 -= num29;
								if ((ownerParagraphValue.IsXpositionUpated ? (ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) : (!ownerParagraphValue.ParagraphFormat.IsFrame)) || paragraphLayoutInfo.XPosition > num12 - num30 - num11 || paragraphLayoutInfo.IsXPositionReUpdate)
								{
									indentX = layouter.ClientLayoutArea.Left + horizontalPosition;
								}
								else
								{
									indentX = paragraphLayoutInfo.XPosition + horizontalPosition;
								}
							}
							else if (ownerParagraphValue.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && (textWrappingStyle == TextWrappingStyle.InFrontOfText || textWrappingStyle == TextWrappingStyle.Behind))
							{
								indentX = paragraphLayoutInfo.XPosition + horizontalPosition;
							}
							else
							{
								indentX = layouter.ClientLayoutArea.Left + horizontalPosition;
							}
							if (ownerParagraphValue != null && ownerParagraphValue.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind && Math.Round(indentX + width) > Math.Round(num12) && width < num12)
							{
								indentX = num12 - width;
							}
							switch (shapeHorizontalAlignment)
							{
							case ShapeHorizontalAlignment.Center:
								indentX = layouter.ClientLayoutArea.Left + (layouter.ClientLayoutArea.Width - width) / 2f;
								break;
							case ShapeHorizontalAlignment.Left:
								indentX = layouter.ClientLayoutArea.Left + num3;
								break;
							case ShapeHorizontalAlignment.Right:
								indentX = layouter.ClientLayoutArea.Left + layouter.ClientLayoutArea.Width - width - num2;
								break;
							}
							break;
						case HorizontalOrigin.Margin:
							if (currentSection != null)
							{
								indentX = num6 + horizontalPosition;
								switch (shapeHorizontalAlignment)
								{
								case ShapeHorizontalAlignment.Center:
									indentX = num6 + (num14 - width) / 2f;
									break;
								case ShapeHorizontalAlignment.Left:
									indentX = num6 + num3;
									break;
								case ShapeHorizontalAlignment.Outside:
									if (layouter.CurrPageIndex % 2 != 0 || (ownerParagraphValue != null && ownerParagraphValue.Document.MultiplePage != MultiplePage.MirrorMargins))
									{
										indentX = num6 + num14 - width;
									}
									break;
								case ShapeHorizontalAlignment.Right:
									indentX = num6 + num14 - width - num2;
									break;
								case ShapeHorizontalAlignment.Inside:
									if (layouter.CurrPageIndex % 2 == 0 || (ownerParagraphValue != null && ownerParagraphValue.Document.MultiplePage != MultiplePage.MirrorMargins))
									{
										indentX = num6 + num14 - width;
									}
									break;
								case ShapeHorizontalAlignment.None:
									if (paragraphItem is WTextBox && Math.Abs((paragraphItem as WTextBox).TextBoxFormat.HorizontalRelativePercent) <= 1000f)
									{
										float horizontalRelativePercent = (paragraphItem as WTextBox).TextBoxFormat.HorizontalRelativePercent;
										float num28 = num14 * (horizontalRelativePercent / 100f);
										indentX = num6 + num28;
									}
									break;
								}
							}
							else
							{
								indentX = m_layoutArea.ClientArea.X + horizontalPosition;
							}
							break;
						case HorizontalOrigin.Character:
							switch (shapeHorizontalAlignment)
							{
							case ShapeHorizontalAlignment.Center:
							case ShapeHorizontalAlignment.Right:
								indentX = GetLeftMarginHorizPosition(num6, shapeHorizontalAlignment, horizontalPosition, width, textWrappingStyle, num2, num3, paragraphItem);
								break;
							case ShapeHorizontalAlignment.Left:
								indentX = m_layoutArea.ClientArea.X + horizontalPosition + num3;
								break;
							default:
								indentX = m_layoutArea.ClientArea.X + horizontalPosition;
								break;
							}
							break;
						case HorizontalOrigin.LeftMargin:
							indentX = GetLeftMarginHorizPosition(num6, shapeHorizontalAlignment, horizontalPosition, width, textWrappingStyle, num2, num3, paragraphItem);
							break;
						case HorizontalOrigin.RightMargin:
							indentX = GetRightMarginHorizPosition(num12, num11, shapeHorizontalAlignment, horizontalPosition, width, textWrappingStyle, num2, num3, paragraphItem);
							break;
						case HorizontalOrigin.InsideMargin:
							if (layouter.CurrPageIndex % 2 == 0)
							{
								indentX = GetRightMarginHorizPosition(num12, num11, shapeHorizontalAlignment, horizontalPosition, width, textWrappingStyle, num2, num3, paragraphItem);
							}
							else
							{
								indentX = GetLeftMarginHorizPosition(num6, shapeHorizontalAlignment, horizontalPosition, width, textWrappingStyle, num2, num3, paragraphItem);
							}
							break;
						case HorizontalOrigin.OutsideMargin:
							if (layouter.CurrPageIndex % 2 == 0)
							{
								indentX = GetLeftMarginHorizPosition(num6, shapeHorizontalAlignment, horizontalPosition, width, textWrappingStyle, num2, num3, paragraphItem);
							}
							else
							{
								indentX = GetRightMarginHorizPosition(num12, num11, shapeHorizontalAlignment, horizontalPosition, width, textWrappingStyle, num2, num3, paragraphItem);
							}
							break;
						default:
							indentX = m_layoutArea.ClientArea.X + horizontalPosition;
							break;
						}
						if (ownerParagraphValue != null && ownerParagraphValue.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind && verticalOrigin == VerticalOrigin.Paragraph && num12 < indentX + width)
						{
							indentX = num12 - width;
						}
					}
				}
			}
			else
			{
				result = false;
			}
			if (wPicture != null && wPicture.Rotation != 0f && (wPicture.TextWrappingStyle == TextWrappingStyle.Behind || wPicture.TextWrappingStyle == TextWrappingStyle.InFrontOfText || wPicture.TextWrappingStyle == TextWrappingStyle.Square || wPicture.TextWrappingStyle == TextWrappingStyle.TopAndBottom))
			{
				GetPictureWrappingBounds(ref indentX, ref indentY, ref size, wPicture.Rotation);
			}
			VerticalOrigin verticalOrigin2 = paragraphItem.GetVerticalOrigin();
			if (textWrappingStyle != 0 && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind)
			{
				if (flag)
				{
					float top = cellLayoutInfo.CellContentLayoutingBounds.Top;
					ILayoutSpacingsInfo layoutSpacingsInfo = cellLayoutInfo;
					float num32 = cellLayoutInfo.CellContentLayoutingBounds.Left - layoutSpacingsInfo.Margins.Left;
					num16 -= layoutSpacingsInfo.Paddings.Left - layoutSpacingsInfo.Paddings.Right;
					if (indentY < top)
					{
						indentY = top;
					}
					if (indentX < num32 || num16 < size.Width)
					{
						indentX = num32;
					}
				}
				else if (ownerParagraphValue != null && (verticalOrigin2 == VerticalOrigin.Paragraph || verticalOrigin2 == VerticalOrigin.Line) && !IsInFrame(ownerParagraphValue))
				{
					float height2 = size.Height;
					float width2 = size.Width;
					if (GetAngle(paragraphItem) == 0f)
					{
						if (ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
						{
							if (!layouter.IsLayoutingHeaderFooter)
							{
								if (indentY + height2 > layouter.ClientLayoutArea.Bottom && !(paragraphItem.GetVerticalPosition() <= 0f))
								{
									float num33 = (m_lcOperator as Layouter).ParagraphYPosition + paragraphLayoutInfo.Size.Height;
									float num34 = layouter.ClientLayoutArea.Bottom - height2;
									indentY = ((num33 > num34) ? num33 : num34);
								}
								if (indentY < layouter.PageTopMargin)
								{
									indentY = layouter.PageTopMargin;
								}
								if (indentX + width2 > num12)
								{
									indentX = num12 - width2;
								}
								if (indentX < 0f)
								{
									indentX = 0f;
								}
							}
						}
						else
						{
							if (!IsWord2013(currentSection.Document) && (!layouter.IsLayoutingHeaderFooter || layouter.IsLayoutingHeader) && indentY + height2 > currentSection.PageSetup.PageSize.Height)
							{
								indentY = currentSection.PageSetup.PageSize.Height - height2;
							}
							if (indentX < 0f)
							{
								indentX = 0f;
							}
							if (indentY < 0f)
							{
								indentY = 0f;
							}
						}
					}
				}
			}
		}
		return result;
	}

	private CellLayoutInfo GetCellLayoutInfo(Entity entity)
	{
		while (!(entity is WTableCell))
		{
			entity = entity.Owner;
		}
		return (entity as IWidget).LayoutInfo as CellLayoutInfo;
	}

	private float GetFloatingItemSpacing(WParagraph paragraph)
	{
		IEntity entity = null;
		if (!paragraph.Document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing && !base.IsForceFitLayout && (entity = paragraph.PreviousSibling) != null && entity is WParagraph && ((paragraph.ParagraphFormat.BeforeSpacing > (entity as WParagraph).ParagraphFormat.AfterSpacing && !paragraph.ParagraphFormat.SpaceBeforeAuto) || (paragraph.ParagraphFormat.SpaceBeforeAuto && (entity as WParagraph).ParagraphFormat.AfterSpacing < 14f)) && !paragraph.ParagraphFormat.ContextualSpacing && (!(entity as WParagraph).ParagraphFormat.ContextualSpacing || !(paragraph.StyleName == (entity as WParagraph).StyleName)))
		{
			if ((entity as WParagraph).ParagraphFormat.SpaceAfterAuto)
			{
				return 14f;
			}
			return (entity as WParagraph).ParagraphFormat.AfterSpacing;
		}
		return 0f;
	}

	private void GetPictureWrappingBounds(ref float indentX, ref float indentY, ref SizeF size, float angle)
	{
		RectangleF boundingBoxCoordinates = base.DrawingContext.GetBoundingBoxCoordinates(new RectangleF(indentX, indentY, size.Width, size.Height), angle);
		indentX = boundingBoxCoordinates.X;
		indentY = boundingBoxCoordinates.Y;
		size = boundingBoxCoordinates.Size;
	}

	private float GetRightMarginHorizPosition(float pageWidth, float rightMargin, ShapeHorizontalAlignment horzAlignment, float horzPosition, float shapeWidth, TextWrappingStyle textWrapStyle, float rightEdgeExtent, float leftEdgeExtent, ParagraphItem paraItem)
	{
		float num = pageWidth - rightMargin;
		float num2 = num + horzPosition;
		switch (horzAlignment)
		{
		case ShapeHorizontalAlignment.Center:
			num2 = num + (rightMargin - shapeWidth) / 2f;
			break;
		case ShapeHorizontalAlignment.Left:
			num2 = num + leftEdgeExtent;
			break;
		case ShapeHorizontalAlignment.Right:
			num2 = pageWidth - shapeWidth - rightEdgeExtent;
			break;
		case ShapeHorizontalAlignment.None:
			if (paraItem is WTextBox && Math.Abs((paraItem as WTextBox).TextBoxFormat.HorizontalRelativePercent) <= 1000f)
			{
				float horizontalRelativePercent = (paraItem as WTextBox).TextBoxFormat.HorizontalRelativePercent;
				float num3 = rightMargin * (horizontalRelativePercent / 100f);
				num2 = num + num3;
			}
			break;
		}
		if ((num2 < 0f || num2 + shapeWidth > pageWidth) && textWrapStyle != TextWrappingStyle.InFrontOfText && textWrapStyle != TextWrappingStyle.Behind)
		{
			num2 = pageWidth - shapeWidth;
		}
		return num2;
	}

	private int IsParagraphContainingLineBreakInFirst(WParagraph paragraph)
	{
		for (int i = 0; i < paragraph.ChildEntities.Count; i++)
		{
			if (!(paragraph.ChildEntities[i] is BookmarkStart) && !(paragraph.ChildEntities[i] is BookmarkEnd) && paragraph.ChildEntities[i] is Break && ((paragraph.ChildEntities[i] as Break).BreakType == BreakType.LineBreak || (paragraph.ChildEntities[i] as Break).BreakType == BreakType.TextWrappingBreak))
			{
				return i;
			}
		}
		return int.MinValue;
	}

	private float GetLeftMarginHorizPosition(float leftMargin, ShapeHorizontalAlignment horzAlignment, float horzPosition, float shapeWidth, TextWrappingStyle textWrapStyle, float rightEdgeExtent, float leftEdgeExtent, ParagraphItem paraItem)
	{
		float num = horzPosition;
		switch (horzAlignment)
		{
		case ShapeHorizontalAlignment.Center:
			num = (leftMargin - shapeWidth) / 2f;
			break;
		case ShapeHorizontalAlignment.Left:
			num = 0f + leftEdgeExtent;
			break;
		case ShapeHorizontalAlignment.Right:
			num = leftMargin - shapeWidth - rightEdgeExtent;
			break;
		case ShapeHorizontalAlignment.None:
			if (paraItem is WTextBox && Math.Abs((paraItem as WTextBox).TextBoxFormat.HorizontalRelativePercent) <= 1000f)
			{
				float horizontalRelativePercent = (paraItem as WTextBox).TextBoxFormat.HorizontalRelativePercent;
				num = leftMargin * (horizontalRelativePercent / 100f);
			}
			break;
		}
		if (num < 0f && textWrapStyle != TextWrappingStyle.InFrontOfText && textWrapStyle != TextWrappingStyle.Behind)
		{
			num = 0f;
		}
		return num;
	}

	private bool IsFitLeafWidgetInContainerHeight(ParagraphItem paraItem, bool isInCell, bool isInTextBox, Entity ent)
	{
		if (!isInCell && !isInTextBox)
		{
			return false;
		}
		bool flag = paraItem.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013;
		if (paraItem is WPicture || paraItem is WChart || paraItem is Shape || paraItem is GroupShape)
		{
			return IsFitLeafWidgetInContainer(paraItem, flag);
		}
		if (paraItem is WTextBox && isInCell && ent != null && ent is WTable)
		{
			if (!flag && !(paraItem as WTextBox).TextBoxFormat.AllowInCell)
			{
				return (paraItem as WTextBox).TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Inline;
			}
			return true;
		}
		return false;
	}

	private bool IsFitLeafWidgetInContainer(ParagraphItem paraItem, bool isWord2013)
	{
		if (!isWord2013 && !paraItem.GetLayOutInCell())
		{
			return paraItem.GetTextWrappingStyle() == TextWrappingStyle.Inline;
		}
		return true;
	}

	private double GetVerticalPosition(ParagraphItem paraItem, float vertPosition, VerticalOrigin vertOrigin, TextWrappingStyle textWrapStyle, CellLayoutInfo cellLayoutInfo)
	{
		WParagraph ownerParagraphValue = paraItem.GetOwnerParagraphValue();
		ParagraphLayoutInfo paragraphLayoutInfo = ((IWidget)ownerParagraphValue).LayoutInfo as ParagraphLayoutInfo;
		Shape shape = paraItem as Shape;
		WPicture wPicture = paraItem as WPicture;
		double num = 0.0;
		double num2 = cellLayoutInfo.CellContentLayoutingBounds.Top;
		switch (vertOrigin)
		{
		case VerticalOrigin.Margin:
		case VerticalOrigin.Page:
		case VerticalOrigin.TopMargin:
		case VerticalOrigin.BottomMargin:
		case VerticalOrigin.InsideMargin:
		case VerticalOrigin.OutsideMargin:
			return num2 + (double)vertPosition;
		case VerticalOrigin.Paragraph:
		case VerticalOrigin.Line:
		{
			float num3 = 0f;
			if (shape != null || wPicture != null)
			{
				num3 = GetFloatingItemSpacing(ownerParagraphValue);
			}
			return paragraphLayoutInfo.YPosition + vertPosition + num3;
		}
		default:
			return m_layoutArea.ClientActiveArea.Y + vertPosition;
		}
	}

	private double GetHorizontalPosition(SizeF size, ParagraphItem paraItem, ShapeHorizontalAlignment horzAlignment, HorizontalOrigin horzOrigin, float horzPosition, TextWrappingStyle textWrapStyle, float cellWid, float rightEdgeExtent, float leftEdgeExtent, float leftMargin)
	{
		double result = 0.0;
		WParagraph ownerParagraphValue = paraItem.GetOwnerParagraphValue();
		ILayoutSpacingsInfo cellLayoutInfo;
		ILayoutSpacingsInfo layoutSpacingsInfo = (cellLayoutInfo = GetCellLayoutInfo(ownerParagraphValue));
		double num = cellWid - cellLayoutInfo.Paddings.Left - cellLayoutInfo.Paddings.Right;
		double num2 = ((CellLayoutInfo)layoutSpacingsInfo).CellContentLayoutingBounds.Width;
		double num3 = ((CellLayoutInfo)layoutSpacingsInfo).CellContentLayoutingBounds.Left;
		double num4 = num3 - (double)cellLayoutInfo.Margins.Left;
		switch (horzOrigin)
		{
		case HorizontalOrigin.Page:
			result = horzPosition;
			switch (horzAlignment)
			{
			case ShapeHorizontalAlignment.Center:
				result = num4 + (num - (double)size.Width) / 2.0;
				break;
			case ShapeHorizontalAlignment.Left:
				result = num4 + (double)leftEdgeExtent;
				break;
			case ShapeHorizontalAlignment.Right:
				result = num4 + (num - (double)size.Width) - (double)rightEdgeExtent;
				break;
			case ShapeHorizontalAlignment.None:
				result = num4 + (double)horzPosition;
				break;
			}
			break;
		case HorizontalOrigin.Margin:
		case HorizontalOrigin.Column:
			switch (horzAlignment)
			{
			case ShapeHorizontalAlignment.Center:
				result = num3 + (num2 - (double)size.Width) / 2.0;
				break;
			case ShapeHorizontalAlignment.Left:
				result = num3 + (double)leftEdgeExtent;
				break;
			case ShapeHorizontalAlignment.Right:
				result = num3 + (num2 - (double)size.Width) - (double)rightEdgeExtent;
				break;
			case ShapeHorizontalAlignment.None:
				result = num3 + (double)horzPosition;
				break;
			}
			break;
		case HorizontalOrigin.LeftMargin:
			switch (horzAlignment)
			{
			case ShapeHorizontalAlignment.Center:
				result = (num4 - (double)size.Width) / 2.0;
				break;
			case ShapeHorizontalAlignment.Left:
				result = num4 + (double)leftEdgeExtent;
				break;
			case ShapeHorizontalAlignment.Right:
				result = num4 - (double)size.Width - (double)rightEdgeExtent;
				break;
			case ShapeHorizontalAlignment.None:
				result = num4 + (double)horzPosition;
				break;
			}
			break;
		case HorizontalOrigin.Character:
			switch (horzAlignment)
			{
			case ShapeHorizontalAlignment.Center:
			case ShapeHorizontalAlignment.Right:
				result = GetLeftMarginHorizPosition(leftMargin, horzAlignment, horzPosition, size.Width, textWrapStyle, rightEdgeExtent, leftEdgeExtent, paraItem);
				break;
			case ShapeHorizontalAlignment.Left:
				result = m_layoutArea.ClientArea.X + horzPosition + leftEdgeExtent;
				break;
			default:
				result = m_layoutArea.ClientArea.X + horzPosition;
				break;
			}
			break;
		default:
			result = num3 + (double)horzPosition;
			break;
		}
		return result;
	}

	private double UpdateLeafWidgetWidth(double width, IWidget widget)
	{
		ParagraphItem paragraphItem = widget as ParagraphItem;
		if (widget is SplitStringWidget)
		{
			paragraphItem = (widget as SplitStringWidget).RealStringWidget as ParagraphItem;
		}
		WParagraph wParagraph = paragraphItem.OwnerParagraph;
		if (paragraphItem.Owner is InlineContentControl || paragraphItem.Owner is XmlParagraphItem)
		{
			wParagraph = paragraphItem.GetOwnerParagraphValue();
		}
		WPicture wPicture = ((LeafWidget is WOleObject) ? (LeafWidget as WOleObject).OlePicture : (LeafWidget as WPicture));
		Shape shape = LeafWidget as Shape;
		WTextBox wTextBox = LeafWidget as WTextBox;
		WChart wChart = LeafWidget as WChart;
		GroupShape groupShape = LeafWidget as GroupShape;
		Layouter layouter = m_lcOperator as Layouter;
		if (wPicture == null && shape == null && wTextBox == null && wChart == null && groupShape == null && width > (double)m_layoutArea.ClientArea.Width && (!(LeafWidget is WSymbol) || wParagraph == null || !wParagraph.IsNeedToFitSymbol(wParagraph)) && (wParagraph.IsInCell || LeafWidget is WFootnote || !IsTextRangeNeedToFit()) && ((layouter.PreviousTab.Justification != TabJustification.Right && layouter.PreviousTab.Justification != TabJustification.Centered) || (IsLeafWidgetIsInCell(paragraphItem) && layouter.PreviousTab.Justification == TabJustification.Right)) && !IsParagraphItemNeedToFit(LeafWidget as ParagraphItem))
		{
			if (wParagraph.IsInCell)
			{
				return m_layoutArea.ClientActiveArea.Width + ((wParagraph.GetOwnerEntity() as IWidget).LayoutInfo as CellLayoutInfo).Margins.Right;
			}
			if (wParagraph.OwnerTextBody.OwnerBase != null && wParagraph.OwnerTextBody.OwnerBase is WTextBox && (wParagraph.OwnerTextBody.OwnerBase as WTextBox).TextBoxFormat.Width > 0f)
			{
				return m_layoutArea.ClientActiveArea.Width + (wParagraph.OwnerTextBody.OwnerBase as WTextBox).TextBoxFormat.InternalMargin.Right;
			}
			return m_layoutArea.ClientArea.Width;
		}
		if ((wPicture != null || wChart != null || wTextBox != null || shape != null || groupShape != null) && IsNeedToAddCellBounds((LeafWidget is WOleObject) ? wPicture.TextWrappingStyle : (LeafWidget as ParagraphItem).GetTextWrappingStyle()) && wParagraph.IsInCell)
		{
			WTableCell wTableCell = wParagraph.GetOwnerEntity() as WTableCell;
			CellLayoutInfo cellLayoutInfo = ((IWidget)wTableCell).LayoutInfo as CellLayoutInfo;
			if (width > (double)(m_layoutArea.ClientActiveArea.Width + cellLayoutInfo.Margins.Right) && !base.LayoutInfo.IsVerticalText)
			{
				if (!(width > (double)wTableCell.Width))
				{
					return width;
				}
				return wTableCell.Width;
			}
			if (width > (double)(m_layoutArea.ClientActiveArea.Width + cellLayoutInfo.Margins.Bottom) && base.LayoutInfo.IsVerticalText)
			{
				return m_layoutArea.ClientArea.Width + cellLayoutInfo.Margins.Bottom;
			}
		}
		return width;
	}

	private bool IsNeedToAddCellBounds(TextWrappingStyle textWrappingStyle)
	{
		if (textWrappingStyle != TextWrappingStyle.InFrontOfText)
		{
			return textWrappingStyle != TextWrappingStyle.Behind;
		}
		return false;
	}

	private void SplitUpWidget(ISplitLeafWidget splitLeafWidget, float clientActiveAreaWidth)
	{
		ISplitLeafWidget[] array = null;
		bool isLastWordFit = false;
		if (base.LayoutInfo is TabsLayoutInfo)
		{
			float pageMarginLeft = GetPageMarginLeft();
			float num = (float)(base.LayoutInfo as TabsLayoutInfo).GetNextTabPosition(m_layoutArea.ClientActiveArea.X - pageMarginLeft);
			if ((m_lcOperator as Layouter).IsFirstItemInLine && Math.Round(m_layoutArea.ClientActiveArea.X + num - pageMarginLeft, 2) >= Math.Round(base.ClientLayoutAreaRight, 2))
			{
				SizeF size = new SizeF(clientActiveAreaWidth, 0f);
				m_ltState = LayoutState.Fitted;
				FitWidget(size, splitLeafWidget, isLastWordFit: false, 0f, 0f, isFloatingItem: false);
				return;
			}
			if (m_layoutArea.ClientArea.Size.Width != 0f)
			{
				array = new ISplitLeafWidget[2] { splitLeafWidget, splitLeafWidget };
			}
		}
		else
		{
			Layouter layouter = m_lcOperator as Layouter;
			float width = layouter.ClientLayoutArea.Width;
			WParagraph ownerParagraph = GetOwnerParagraph();
			if (ownerParagraph != null)
			{
				if (IsInFrame(ownerParagraph))
				{
					width = layouter.FrameLayoutArea.Width;
				}
				WTableCell wTableCell = null;
				if (ownerParagraph.IsInCell)
				{
					wTableCell = ownerParagraph.GetOwnerEntity() as WTableCell;
					width = wTableCell.Width;
				}
				if (wTableCell != null && ((IWidget)wTableCell).LayoutInfo.IsVerticalText)
				{
					width = (((IWidget)wTableCell).LayoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Width;
				}
			}
			bool isTabStopInterSectingfloattingItem = false;
			if (layouter.RightPositionOfTabStopInterSectingFloattingItems != float.MinValue)
			{
				isTabStopInterSectingfloattingItem = true;
			}
			int countForConsecutivelimit = layouter.CountForConsecutiveLimit;
			array = splitLeafWidget.SplitBySize(base.DrawingContext, m_layoutArea.ClientArea.Size, width, clientActiveAreaWidth, ref isLastWordFit, isTabStopInterSectingfloattingItem, layouter.m_canSplitbyCharacter, layouter.IsFirstItemInLine, ref countForConsecutivelimit, layouter, ref isHyphenated);
			layouter.CountForConsecutiveLimit = countForConsecutivelimit;
		}
		m_ltState = LayoutState.NotFitted;
		if (array != null)
		{
			SizeF size = array[0].Measure(base.DrawingContext);
			if (array[0].LayoutInfo is TabsLayoutInfo)
			{
				size.Width = 0f;
			}
			if (size.Width > m_layoutArea.ClientArea.Width && !isLastWordFit && !IsParagraphItemNeedToFit(LeafWidget as ParagraphItem))
			{
				size.Width = m_layoutArea.ClientArea.Width;
			}
			FitWidget(size, array[0], isLastWordFit, 0f, 0f, isFloatingItem: false);
			if (array[1] != null)
			{
				m_sptWidget = array[1];
				m_ltState = LayoutState.Splitted;
			}
			else
			{
				m_ltState = LayoutState.Fitted;
			}
		}
	}
}
