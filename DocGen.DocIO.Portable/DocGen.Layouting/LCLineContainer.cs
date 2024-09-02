using System;
using System.Collections.Generic;
using System.Text;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Rendering;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LCLineContainer : LCContainer
{
	internal bool IsFirstItemInPage
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	public LCLineContainer(IWidgetContainer container, ILCOperator lcOperator, bool isForceFitLayout)
		: base(container, lcOperator, isForceFitLayout)
	{
		m_bSkipAreaSpacing = true;
		(lcOperator as Layouter).m_canSplitbyCharacter = true;
		(lcOperator as Layouter).m_canSplitByTab = false;
		(lcOperator as Layouter).IsFirstItemInLine = true;
	}

	protected override void DoLayoutChild(LayoutContext childContext)
	{
		base.IsTabStopBeyondRightMarginExists = false;
		childContext.IsTabStopBeyondRightMarginExists = false;
		RectangleF clientActiveArea = m_layoutArea.ClientActiveArea;
		if (m_ltWidget.ChildWidgets.Count == 0)
		{
			float x = clientActiveArea.X;
			float width = clientActiveArea.Width;
			m_ltWidget.UpdateParaFirstLineHorizontalPositions(base.LayoutInfo as ParagraphLayoutInfo, childContext.Widget, ref x, ref width);
			clientActiveArea.X = x;
			clientActiveArea.Width = width;
		}
		if (base.IsForceFitLayout)
		{
			IsFirstItemInPage = true;
		}
		LayoutedWidget layoutedWidget = CheckNullConditionAndReturnltwidget();
		if ((base.CurrentChildWidget is ParagraphItem || base.CurrentChildWidget is SplitStringWidget) && layoutedWidget != null && !(layoutedWidget.Widget is SplitStringWidget))
		{
			int[] interSectingPoint = (m_lcOperator as Layouter).m_interSectingPoint;
			if (interSectingPoint[2] != int.MinValue && interSectingPoint[3] != int.MinValue && layoutedWidget.ChildWidgets.Count > interSectingPoint[2] && (layoutedWidget = layoutedWidget.ChildWidgets[interSectingPoint[2]]) != null && layoutedWidget.ChildWidgets.Count > interSectingPoint[3] && (m_lcOperator as Layouter).NotFittedFloatingItems.Count == 0)
			{
				LayoutedWidget layoutedWidget2 = layoutedWidget.ChildWidgets[interSectingPoint[3]];
				childContext.Widget = layoutedWidget2.Widget;
				AddLayoutWidgetInBeforeInsectingPoint(layoutedWidget2, interSectingPoint[3]);
			}
			else if (layoutedWidget != null && interSectingPoint[3] == int.MinValue)
			{
				LayoutedWidget layoutedWidget3 = layoutedWidget;
				childContext.Widget = layoutedWidget3.Widget;
				AddLayoutWidgetInBeforeInsectingPoint(layoutedWidget3, interSectingPoint[1]);
			}
			clientActiveArea = m_layoutArea.ClientActiveArea;
			(m_lcOperator as Layouter).m_interSectingPoint[0] = int.MinValue;
			(m_lcOperator as Layouter).m_interSectingPoint[1] = int.MinValue;
			(m_lcOperator as Layouter).m_interSectingPoint[2] = int.MinValue;
			(m_lcOperator as Layouter).m_interSectingPoint[3] = int.MinValue;
		}
		m_currChildLW = childContext.Layout(clientActiveArea);
		if (m_currChildLW != null && m_currChildLW.Widget is WParagraph && !(childContext is LCLineContainer))
		{
			WParagraph wParagraph = m_currChildLW.Widget as WParagraph;
			if ((m_lcOperator as Layouter).FloatingItems.Count > 0 && IsNeedToUpdateIntersectingBounds(wParagraph))
			{
				WParagraphFormat paragraphFormat = (m_currChildLW.Widget as WParagraph).ParagraphFormat;
				if (paragraphFormat.TextureStyle != 0 || !paragraphFormat.BackColor.IsEmpty)
				{
					UpdateItersectingFloatingItemBounds();
				}
			}
			UpdatePositionByLineTextWrap(wParagraph);
		}
		(m_lcOperator as Layouter).PreviousTab = new TabsLayoutInfo.LayoutTab();
		(m_lcOperator as Layouter).PreviousTabWidth = 0f;
		if ((m_lcOperator as Layouter).m_lineSpaceWidths != null)
		{
			(m_lcOperator as Layouter).m_lineSpaceWidths.Clear();
			(m_lcOperator as Layouter).m_lineSpaceWidths = null;
		}
		(m_lcOperator as Layouter).m_effectiveJustifyWidth = 0f;
		(m_lcOperator as Layouter).IsWord2013WordFitLayout = false;
	}

	private void UpdatePositionByLineTextWrap(WParagraph paragraph)
	{
		if (SkipUpdatingPosition(paragraph))
		{
			return;
		}
		bool flag = false;
		LayoutedWidget layoutedWidget = null;
		foreach (LayoutedWidget childWidget in m_currChildLW.ChildWidgets)
		{
			if (childWidget.Widget is ParagraphItem && (childWidget.Widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false) && (childWidget.Widget as ParagraphItem).GetHorizontalOrigin() == HorizontalOrigin.Column)
			{
				if (!flag)
				{
					TextWrappingStyle textWrappingStyle = (childWidget.Widget as ParagraphItem).GetTextWrappingStyle();
					flag = textWrappingStyle == TextWrappingStyle.Behind || textWrappingStyle == TextWrappingStyle.InFrontOfText;
				}
			}
			else if (layoutedWidget == null && childWidget.Widget is ILeafWidget && !(childWidget.Widget is BookmarkStart) && !(childWidget.Widget is BookmarkEnd))
			{
				layoutedWidget = childWidget;
			}
		}
		if (layoutedWidget == null || m_currChildLW.Bounds.X != layoutedWidget.Bounds.X || !(m_currChildLW.Bounds.Height > layoutedWidget.Bounds.Height))
		{
			return;
		}
		RectangleF bounds = m_currChildLW.Bounds;
		bounds.Width = m_layoutArea.ClientArea.Width;
		bounds.Height = m_layoutArea.ClientArea.Height;
		SizeF widgetSize = new SizeF(layoutedWidget.Bounds.Width, m_currChildLW.Bounds.Height);
		RectangleF rectangleF = new LeafLayoutContext(layoutedWidget.Widget as ILeafWidget, m_lcOperator, base.IsForceFitLayout).FindWrappedPosition(widgetSize, bounds);
		if (!(bounds.X < rectangleF.X) || !(rectangleF.Width >= widgetSize.Width))
		{
			return;
		}
		float num = rectangleF.X - bounds.X;
		foreach (LayoutedWidget childWidget2 in m_currChildLW.ChildWidgets)
		{
			if (childWidget2.Widget is ParagraphItem && (childWidget2.Widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false) && (childWidget2.Widget as ParagraphItem).GetHorizontalOrigin() == HorizontalOrigin.Column)
			{
				TextWrappingStyle textWrappingStyle2 = (childWidget2.Widget as ParagraphItem).GetTextWrappingStyle();
				if (textWrappingStyle2 == TextWrappingStyle.Behind || textWrappingStyle2 == TextWrappingStyle.InFrontOfText)
				{
					childWidget2.Bounds = new RectangleF(childWidget2.Bounds.X + num, childWidget2.Bounds.Y, childWidget2.Bounds.Width, childWidget2.Bounds.Height);
				}
			}
		}
	}

	private bool SkipUpdatingPosition(WParagraph paragraph)
	{
		if (paragraph.IsXpositionUpated || paragraph.ParagraphFormat.IsFrame || IsWord2013(paragraph.Document) || !IsFirstItemInPage || (m_lcOperator as Layouter).IsLayoutingHeaderFooter || m_ltWidget.Bounds.X != m_currChildLW.Bounds.X)
		{
			return true;
		}
		if (IsBaseFromSection(paragraph))
		{
			if (paragraph.GetOwnerSection().Columns.Count > 1)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	private bool IsNeedToUpdateIntersectingBounds(WParagraph currentParagraph)
	{
		IEntity prevEntity = null;
		if ((!(m_lcOperator as Layouter).IsLayoutingHeaderFooter || currentParagraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) && !currentParagraph.IsPreviousParagraphMarkIsHidden() && !currentParagraph.IsPreviousParagraphMarkIsInDeletion(ref prevEntity) && !IsInFrame(currentParagraph) && !IsInFootnote(currentParagraph) && IsBaseFromSection(currentParagraph))
		{
			return true;
		}
		return false;
	}

	private void UpdateItersectingFloatingItemBounds()
	{
		RectangleF bounds = m_currChildLW.Bounds;
		List<FloatingItem> list = new List<FloatingItem>((m_lcOperator as Layouter).FloatingItems);
		FloatingItem.SortFloatingItems(list, SortPosition.X, isNeedToUpdateWrapCollectionIndex: false);
		for (int i = 0; i < list.Count; i++)
		{
			FloatingItem floatingItem = list[i];
			RectangleF rectangleF = RectangleF.Empty;
			if (floatingItem.TextWrappingStyle == TextWrappingStyle.Square)
			{
				rectangleF = floatingItem.TextWrappingBounds;
			}
			else if (floatingItem.TextWrappingStyle == TextWrappingStyle.Tight || floatingItem.TextWrappingStyle == TextWrappingStyle.Through)
			{
				rectangleF = ((!floatingItem.IsDoesNotDenotesRectangle) ? floatingItem.TextWrappingBounds : AdjustTightAndThroughBounds(floatingItem, bounds, bounds.Height));
			}
			if (rectangleF != RectangleF.Empty && IsYPositionIntersect(rectangleF, bounds, bounds.Height))
			{
				m_currChildLW.IntersectingBounds.Add(rectangleF);
			}
		}
	}

	protected override LayoutContext CreateNextChildContext()
	{
		if (base.WidgetContainer == null)
		{
			return null;
		}
		return new LCContainer(base.WidgetContainer, m_lcOperator, base.IsForceFitLayout);
	}

	protected override void MarkAsNotFitted(LayoutContext childContext, bool isFootnote)
	{
		base.IsVerticalNotFitted = childContext.IsVerticalNotFitted;
		if (m_bAtLastOneChildFitted)
		{
			if (!IsKeepLineTogether(childContext))
			{
				WParagraph paragraph = GetParagraph();
				WSection wSection = paragraph?.GetOwnerSection();
				ParagraphLayoutInfo paragraphLayoutInfo = base.LayoutInfo as ParagraphLayoutInfo;
				if (!IsLastParagraphNeedToBeLayout(childContext) && !m_ltWidget.IsNotFitted)
				{
					if (m_notFittedWidget != null)
					{
						if (wSection != null && base.SplittedWidget != null)
						{
							RemoveAutoHyphenatedString(m_notFittedWidget, wSection.Document.DOP.AutoHyphen);
						}
						m_sptWidget = m_notFittedWidget;
						m_notFittedWidget = null;
						RemoveTrackChangesBalloon(m_currChildLW.Bounds.Bottom);
						if ((m_lcOperator as Layouter).TrackChangesMarkups.Count > 0)
						{
							RemoveCommentMarkUps(m_currChildLW.Bounds.Bottom);
						}
					}
					else
					{
						if (paragraph != null && !paragraph.IsInCell)
						{
							LayoutContext layoutContext = LayoutContext.Create(childContext.SplittedWidget, m_lcOperator, base.IsForceFitLayout);
							RectangleF rect = new RectangleF((m_lcOperator as Layouter).ClientLayoutArea.X, (m_lcOperator as Layouter).ClientLayoutArea.Y, (m_lcOperator as Layouter).ClientLayoutArea.Width, (m_lcOperator as Layouter).ClientLayoutArea.Height);
							layoutContext.IsNeedToWrap = false;
							layoutContext.IsInnerLayouting = true;
							LayoutedWidget layoutedWidget = layoutContext.Layout(rect);
							if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
							{
								return;
							}
							UpdateFootnoteWidgets(layoutedWidget);
							layoutedWidget.InitLayoutInfo(resetTabLayoutInfo: false);
							if (IsNeedToResetSplitWidget(childContext, paragraph, layoutedWidget.TextTag == "Splitted") || IsNeedToSplitPreviousItem(layoutedWidget, childContext))
							{
								MoveLayoutedLineToNextPage(paragraph, childContext);
							}
							else if (wSection != null && wSection.PageSetup.FootnotePosition == FootnotePosition.PrintAtBottomOfPage && (m_lcOperator as Layouter).IsFootnoteHeightAdjusted && m_ltWidget.ChildWidgets.Count > 2)
							{
								LayoutedWidget layoutedWidget2 = m_ltWidget.ChildWidgets[2];
								if (layoutedWidget2.Widget is SplitWidgetContainer)
								{
									m_sptWidget = layoutedWidget2.Widget as SplitWidgetContainer;
									for (int num = m_ltWidget.ChildWidgets.Count - 1; num > 1; num--)
									{
										UpdateFootnoteWidgets(m_ltWidget.ChildWidgets[num]);
										layoutedWidget2.InitLayoutInfo(resetTabLayoutInfo: false);
										RemoveTrackChangesBalloon(m_ltWidget.ChildWidgets[num].Bounds.Y);
										m_ltWidget.ChildWidgets.Remove(m_ltWidget.ChildWidgets[num]);
									}
									IsEndPage(paragraph, paragraph.PreviousSibling == null);
								}
								else
								{
									m_sptWidget = childContext.SplittedWidget;
								}
							}
							else
							{
								if (wSection != null && wSection.Document.DOP.AutoHyphen)
								{
									RemoveAutoHyphenatedString(childContext.SplittedWidget, wSection.Document.DOP.AutoHyphen);
								}
								m_sptWidget = childContext.SplittedWidget;
							}
						}
						else
						{
							m_sptWidget = childContext.SplittedWidget;
						}
						if (paragraph != null && paragraph.IsInCell && !((paragraph.GetOwnerEntity() as WTableCell).OwnerRow.m_layoutInfo as RowLayoutInfo).IsExactlyRowHeight && paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
						{
							LayoutContext layoutContext2 = LayoutContext.Create(childContext.SplittedWidget, m_lcOperator, base.IsForceFitLayout);
							RectangleF rect2 = new RectangleF(m_layoutArea.ClientActiveArea.X, (m_lcOperator as Layouter).ClientLayoutArea.Y, m_layoutArea.ClientActiveArea.Width, (m_lcOperator as Layouter).ClientLayoutArea.Height);
							layoutContext2.IsNeedToWrap = false;
							layoutContext2.IsInnerLayouting = true;
							LayoutedWidget layoutedWidget3 = layoutContext2.Layout(rect2);
							if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
							{
								return;
							}
							UpdateFootnoteWidgets(layoutedWidget3);
							layoutedWidget3.InitLayoutInfo(resetTabLayoutInfo: false);
							if (IsNeedToResetSplitWidget(childContext, paragraph, layoutedWidget3.TextTag == "Splitted"))
							{
								if (paragraph.GetIndexInOwnerCollection() == 0)
								{
									DocumentLayouter.IsEndPage = m_ltWidget.ChildWidgets.Count + 1 <= 3;
								}
								else if (m_ltWidget.ChildWidgets.Count == 2 && paragraphLayoutInfo != null)
								{
									DocumentLayouter.IsEndPage = true;
									paragraphLayoutInfo.IsNotFitted = true;
									if (wSection != null && base.SplittedWidget != null)
									{
										RemoveAutoHyphenatedString(childContext.SplittedWidget, wSection.Document.DOP.AutoHyphen);
									}
									m_notFittedWidget = childContext.SplittedWidget;
									m_ltWidget.IsNotFitted = true;
									MarkAsSplitted(childContext);
									return;
								}
								MoveLayoutedLineToNextPage(paragraph, childContext);
							}
						}
					}
					m_ltWidget.IsLastItemInPage = true;
					m_ltState = LayoutState.Splitted;
				}
				else if (m_ltWidget.ChildWidgets.Count == 2 && paragraphLayoutInfo != null && !IsLineContainOnlyNonRenderableItem(m_ltWidget.ChildWidgets[1]))
				{
					paragraphLayoutInfo.IsNotFitted = true;
					if (wSection != null && base.SplittedWidget != null)
					{
						RemoveAutoHyphenatedString(childContext.SplittedWidget, wSection.Document.DOP.AutoHyphen);
					}
					m_notFittedWidget = childContext.SplittedWidget;
					m_ltWidget.IsNotFitted = true;
					MarkAsSplitted(childContext);
				}
				else if ((paragraph != null && !paragraph.ParagraphFormat.WidowControl && !IsParagraphContainsBookMarksOnly() && !IsNeedToNotFitTheItem()) || (IsFirstItemInPage && m_ltWidget.ChildWidgets.Count >= 1))
				{
					if (wSection != null && base.SplittedWidget != null)
					{
						RemoveAutoHyphenatedString(childContext.SplittedWidget, wSection.Document.DOP.AutoHyphen);
					}
					m_sptWidget = childContext.SplittedWidget;
					m_ltWidget.IsLastItemInPage = true;
					m_ltState = LayoutState.Splitted;
				}
				else
				{
					m_notFittedWidget = null;
					m_ltState = LayoutState.NotFitted;
					if (paragraph != null && paragraph.PreviousSibling != null && paragraph.PreviousSibling is WParagraph)
					{
						WParagraph wParagraph = paragraph.PreviousSibling as WParagraph;
						IsEndPage(wParagraph, wParagraph.ParagraphFormat.KeepFollow);
					}
					else
					{
						IsEndPage(paragraph, paragraph.PreviousSibling == null);
					}
					UpdateFootnoteWidgets();
					ResetFloatingEntityProperty(GetParagraph());
					RemoveTrackChangesBalloon(m_ltWidget.Bounds.Y);
				}
			}
			else
			{
				UpdateFootnoteWidgets();
				ResetFloatingEntityProperty(GetParagraph());
				m_ltState = LayoutState.NotFitted;
			}
		}
		else
		{
			UpdateFootnoteWidgets();
			m_ltState = LayoutState.NotFitted;
		}
		if (m_ltState == LayoutState.Splitted)
		{
			IsFloatingItemFitted(m_lcOperator as Layouter, childContext);
		}
	}

	private void MoveLayoutedLineToNextPage(WParagraph paragraph, LayoutContext childContext)
	{
		WSection wSection = paragraph?.GetOwnerSection();
		LayoutedWidget layoutedWidget = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1];
		if (layoutedWidget.Widget is SplitWidgetContainer)
		{
			m_sptWidget = layoutedWidget.Widget as SplitWidgetContainer;
			UpdateFootnoteWidgets(layoutedWidget);
			layoutedWidget.InitLayoutInfo(resetTabLayoutInfo: false);
			m_ltWidget.ChildWidgets.Remove(layoutedWidget);
			IsEndPage(paragraph, paragraph.PreviousSibling == null);
			if (wSection != null && base.SplittedWidget != null)
			{
				RemoveAutoHyphenatedString(m_sptWidget, wSection.Document.DOP.AutoHyphen);
			}
			if ((m_lcOperator as Layouter).TrackChangesMarkups.Count > 0)
			{
				RemoveCommentMarkUps(m_currChildLW.Bounds.Bottom);
			}
		}
		else
		{
			if (wSection != null && base.SplittedWidget != null)
			{
				RemoveAutoHyphenatedString(childContext.SplittedWidget, wSection.Document.DOP.AutoHyphen);
			}
			m_sptWidget = childContext.SplittedWidget;
		}
	}

	private void RemoveCommentMarkUps(float yPos)
	{
		yPos = (float)Math.Round(yPos, 2);
		for (int num = (m_lcOperator as Layouter).TrackChangesMarkups.Count - 1; num >= 0; num--)
		{
			float num2 = (float)Math.Round((m_lcOperator as Layouter).TrackChangesMarkups[num].Position.Y + 0.125f, 2);
			if ((m_lcOperator as Layouter).TrackChangesMarkups[num] is CommentsMarkups)
			{
				if (!(num2 >= yPos))
				{
					break;
				}
				(m_lcOperator as Layouter).TrackChangesMarkups.RemoveAt(num);
			}
		}
	}

	private bool IsNeedToSplitPreviousItem(LayoutedWidget widget, LayoutContext childContext)
	{
		bool result = false;
		LayoutedWidget layoutedWidget = ((widget.ChildWidgets.Count > 0) ? widget.ChildWidgets[widget.ChildWidgets.Count - 1] : widget);
		LayoutedWidget layoutedWidget2 = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1];
		if (widget.TextTag == "Splitted" && layoutedWidget.ChildWidgets.Count > 0 && layoutedWidget2.ChildWidgets.Count > 0 && layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget is Break && ((layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget as Break).BreakType == BreakType.LineBreak || (layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget as Break).BreakType == BreakType.TextWrappingBreak) && (!(layoutedWidget2.ChildWidgets[layoutedWidget2.ChildWidgets.Count - 1].Widget is Break) || ((layoutedWidget2.ChildWidgets[layoutedWidget2.ChildWidgets.Count - 1].Widget as Break).BreakType != BreakType.LineBreak && (layoutedWidget2.ChildWidgets[layoutedWidget2.ChildWidgets.Count - 1].Widget as Break).BreakType != BreakType.TextWrappingBreak)) && m_layoutArea.ClientActiveArea.Height < layoutedWidget.Bounds.Height && m_layoutArea.ClientActiveArea.Width > layoutedWidget2.Bounds.Width + layoutedWidget.Bounds.Width)
		{
			result = true;
		}
		return result;
	}

	private void IsFloatingItemFitted(Layouter layouter, LayoutContext childContext)
	{
		if (layouter.FloatingItems.Count <= 0 || m_ltWidget.ChildWidgets.Count <= 0)
		{
			return;
		}
		WParagraph paragraph = GetParagraph();
		if (paragraph == null || !paragraph.IsFloatingItemsLayouted)
		{
			return;
		}
		int num = 0;
		while (num < layouter.FloatingItems.Count)
		{
			FloatingItem floatingItem = layouter.FloatingItems[num];
			if (!floatingItem.IsFloatingItemFit && floatingItem.FloatingEntity is ParagraphItem)
			{
				layouter.NotFittedFloatingItems.Add(floatingItem.FloatingEntity);
				(floatingItem.FloatingEntity as ParagraphItem).SetIsWrappingBoundsAdded(boolean: false);
				layouter.FloatingItems.Remove(floatingItem);
			}
			else
			{
				num++;
			}
		}
		if (layouter.NotFittedFloatingItems.Count > 0)
		{
			IWidget splittedWidget = childContext.SplittedWidget;
			SplitedUpWidget(splittedWidget, isEndNoteSplitWidgets: false);
			m_ltState = LayoutState.DynamicRelayout;
			m_currChildLW.Owner = m_ltWidget;
			paragraph.IsFloatingItemsLayouted = false;
		}
	}

	private void RemoveTrackChangesBalloon(float yPos)
	{
		if ((m_lcOperator as Layouter).IsLayoutingTrackChanges)
		{
			yPos = (float)Math.Round(yPos, 2);
			int num = (m_lcOperator as Layouter).TrackChangesMarkups.Count - 1;
			while (num >= 0 && (float)Math.Round((m_lcOperator as Layouter).TrackChangesMarkups[num].Position.Y, 2) >= yPos)
			{
				(m_lcOperator as Layouter).TrackChangesMarkups.RemoveAt(num);
				num--;
			}
		}
	}

	private new void RemoveAutoHyphenatedString(IWidget SplittedWidget, bool isAutoHyphen)
	{
		string text = null;
		bool flag = false;
		int num;
		SplitStringWidget splitStringWidget;
		int num2;
		object obj;
		if (isAutoHyphen)
		{
			num = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].ChildWidgets.Count - 1;
			splitStringWidget = ((num >= 0) ? (m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].ChildWidgets[num].Widget as SplitStringWidget) : null);
			if (SplittedWidget is SplitWidgetContainer && (SplittedWidget as SplitWidgetContainer).m_currentChild is SplitStringWidget)
			{
				num2 = ((splitStringWidget != null) ? 1 : 0);
				if (num2 != 0)
				{
					obj = (SplittedWidget as SplitWidgetContainer).m_currentChild as SplitStringWidget;
					goto IL_00af;
				}
			}
			else
			{
				num2 = 0;
			}
			obj = null;
			goto IL_00af;
		}
		goto IL_01e1;
		IL_00af:
		SplitStringWidget splitStringWidget2 = (SplitStringWidget)obj;
		if (num2 != 0 && splitStringWidget.SplittedText.EndsWith("-") && !splitStringWidget.SplittedText.Trim().Equals("-") && !string.IsNullOrEmpty(splitStringWidget2.SplittedText))
		{
			text = GetPeekWord(splitStringWidget.SplittedText);
			int startIndex = splitStringWidget2.StartIndex;
			StringBuilder stringBuilder = new StringBuilder((splitStringWidget.RealStringWidget as WTextRange).Text);
			stringBuilder.Remove(splitStringWidget.StartIndex + (splitStringWidget.SplittedText.Length - 1), 1);
			(splitStringWidget.RealStringWidget as WTextRange).Text = stringBuilder.ToString();
			splitStringWidget2.StartIndex = startIndex - (text.Length + 1);
			splitStringWidget2.Length += text.Length;
			string value = splitStringWidget.SplittedText.TrimEnd('-').Trim();
			if (text.Equals(value))
			{
				m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].ChildWidgets.RemoveAt(num);
			}
			else
			{
				splitStringWidget.Length -= text.Length + 1;
				flag = true;
			}
		}
		goto IL_01e1;
		IL_01e1:
		if (!string.IsNullOrEmpty(text))
		{
			LayoutedWidget layoutedWidget = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1];
			RectangleF bounds = layoutedWidget.Bounds;
			LayoutedWidget layoutedWidget2 = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
			RectangleF bounds2 = layoutedWidget2.Bounds;
			WTextRange wTextRange = ((layoutedWidget2.Widget is SplitStringWidget) ? ((layoutedWidget2.Widget as SplitStringWidget).RealStringWidget as WTextRange) : null);
			SizeF sizeF = base.DrawingContext.MeasureString(text + "-", wTextRange.CharacterFormat.GetFontToRender(wTextRange.ScriptType), null, wTextRange.ScriptType);
			bounds2.Width -= sizeF.Width;
			bounds.Width -= sizeF.Width;
			if (flag)
			{
				m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].ChildWidgets[m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].ChildWidgets.Count - 1].Bounds = new RectangleF(bounds2.X, bounds2.Y, bounds2.Width, bounds2.Height);
			}
			m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Bounds = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
			UpdateltBounds(m_ltWidget);
		}
	}

	private void UpdateltBounds(LayoutedWidget widget)
	{
		widget.Bounds = new RectangleF(widget.Bounds.X, widget.Bounds.Y, 0f - widget.Bounds.X + GetMaximumRight(widget), 0f - widget.Bounds.Y + GetMaximumBottom(widget));
	}

	private float GetMaximumRight(LayoutedWidget widget)
	{
		float right = widget.Bounds.Right;
		for (int i = 0; i < widget.ChildWidgets.Count; i++)
		{
			if (i == 0)
			{
				right = widget.ChildWidgets[i].Bounds.Right;
			}
			else if (widget.ChildWidgets[i].Bounds.Right > right)
			{
				right = widget.ChildWidgets[i].Bounds.Right;
			}
		}
		return right;
	}

	private float GetMaximumBottom(LayoutedWidget widget)
	{
		float bottom = widget.Bounds.Bottom;
		for (int i = 0; i < widget.ChildWidgets.Count; i++)
		{
			if (i == 0)
			{
				bottom = widget.ChildWidgets[i].Bounds.Bottom;
			}
			else if (widget.ChildWidgets[i].Bounds.Bottom >= bottom)
			{
				bottom = widget.ChildWidgets[i].Bounds.Bottom;
			}
		}
		return bottom;
	}

	private string GetPeekWord(string hyphenatedLine)
	{
		int num = hyphenatedLine.Length - 1;
		int num2 = num;
		int num3 = hyphenatedLine.Length - 2;
		while (num3 < hyphenatedLine.Length - 1 && num3 >= 0)
		{
			char c = hyphenatedLine[num3];
			if (c == '\r' || c == '\n' || c == ' ' || c == '\t' || c == '-')
			{
				break;
			}
			num2--;
			num3--;
		}
		if (num2 < num && num2 >= 0)
		{
			return hyphenatedLine.Substring(num2, num - num2);
		}
		return hyphenatedLine;
	}

	private bool IsLineContainOnlyNonRenderableItem(LayoutedWidget lineWidget)
	{
		for (int i = 0; i < lineWidget.ChildWidgets.Count; i++)
		{
			if (!IsNonRenderableItem(lineWidget.ChildWidgets[i].Widget))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsLineContainsOnlyMathItems(LayoutedWidget lineWidget)
	{
		for (int i = 0; i < lineWidget.ChildWidgets.Count; i++)
		{
			IWidget widget = lineWidget.ChildWidgets[i].Widget;
			if (!(widget is WMath) && !(widget is BookmarkStart) && !(widget is BookmarkEnd) && !(widget is EditableRangeStart) && !(widget is EditableRangeEnd))
			{
				return false;
			}
		}
		return true;
	}

	private void IsEndPage(WParagraph paragraph, bool keepFollow)
	{
		if (paragraph != null && GetBaseEntity(paragraph) is WSection wSection && !paragraph.IsInCell)
		{
			int columnIndex = base.DrawingContext.GetColumnIndex(wSection, m_ltWidget.Bounds);
			if (keepFollow && paragraph.ParagraphFormat.Keep && columnIndex == wSection.Columns.Count - 1)
			{
				DocumentLayouter.IsEndPage = true;
			}
		}
	}

	private bool IsNeedToResetSplitWidget(LayoutContext childContext, WParagraph paragraph, bool isSplittedLine)
	{
		if (!isSplittedLine && paragraph.ParagraphFormat.WidowControl && !(GetBaseEntity(paragraph) is WTextBox))
		{
			if (childContext.SplittedWidget is SplitWidgetContainer)
			{
				if (!((childContext.SplittedWidget as SplitWidgetContainer).m_currentChild is WChart) && !((childContext.SplittedWidget as SplitWidgetContainer).m_currentChild is Shape) && !((childContext.SplittedWidget as SplitWidgetContainer).m_currentChild is GroupShape))
				{
					return !((childContext.SplittedWidget as SplitWidgetContainer).m_currentChild is WOleObject);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private void UpdateFootnoteWidgets()
	{
		if (!(m_lcOperator as Layouter).IsLayoutingFootnote)
		{
			WParagraph paragraph = GetParagraph();
			if (paragraph != null)
			{
				UpdateFootnoteWidgets(paragraph);
			}
		}
	}

	private bool IsKeepLineTogether(LayoutContext childContext)
	{
		ParagraphLayoutInfo paragraphLayoutInfo = base.LayoutInfo as ParagraphLayoutInfo;
		bool result = false;
		if (paragraphLayoutInfo != null)
		{
			result = paragraphLayoutInfo.IsKeepTogether;
			WParagraph paragraph = GetParagraph();
			if ((paragraph != null && IsInFrame(paragraph)) || IsFirstItemInPage)
			{
				result = false;
			}
		}
		return result;
	}

	private bool IsLastParagraphNeedToBeLayout(LayoutContext childContext)
	{
		int num;
		if (base.LayoutInfo is ParagraphLayoutInfo)
		{
			WParagraph paragraph = GetParagraph();
			if (paragraph == null || !paragraph.IsInCell)
			{
				if (!paragraph.IsInCell && (paragraph.OwnerBase == null || !(paragraph.OwnerBase.OwnerBase is WTextBox)) && !IsInFrame(paragraph) && childContext.Widget is SplitWidgetContainer)
				{
					num = (((!base.IsForceFitLayout) ? (m_ltWidget.ChildWidgets.Count < 3) : (m_ltWidget.ChildWidgets.Count == 1)) ? 1 : 0);
					goto IL_00b2;
				}
			}
			else if (IsWord2013(paragraph.Document))
			{
				num = (IsNeedToLayout(paragraph.GetOwnerTableCell(paragraph.OwnerTextBody)) ? 1 : 0);
				goto IL_00b2;
			}
		}
		goto IL_00b6;
		IL_00b6:
		return false;
		IL_00b2:
		if (num != 0)
		{
			return true;
		}
		goto IL_00b6;
	}

	private bool IsNeedToLayout(WTableCell ownerTableCell)
	{
		if (base.IsForceFitLayout)
		{
			return false;
		}
		return m_ltWidget.ChildWidgets.Count < 2;
	}

	private bool IsParagraphContainsBookMarksOnly()
	{
		for (int i = 0; i < m_ltWidget.ChildWidgets.Count; i++)
		{
			for (int j = 0; j < m_ltWidget.ChildWidgets[i].ChildWidgets.Count; j++)
			{
				if (!(m_ltWidget.ChildWidgets[i].ChildWidgets[j].Widget is BookmarkStart) && !(m_ltWidget.ChildWidgets[i].ChildWidgets[j].Widget is BookmarkEnd))
				{
					return false;
				}
			}
		}
		return true;
	}

	private bool IsNeedToNotFitTheItem()
	{
		if (base.CurrentChildWidget is ParagraphItem && m_ltWidget.ChildWidgets.Count == 1 && m_ltWidget.ChildWidgets[0].ChildWidgets.Count > 0 && !(base.CurrentChildWidget as ParagraphItem).IsFloatingItem(isTextWrapAround: false) && m_ltWidget.ChildWidgets[0].ChildWidgets[m_ltWidget.ChildWidgets[0].ChildWidgets.Count - 1].Widget is ParagraphItem && (m_ltWidget.ChildWidgets[0].ChildWidgets[m_ltWidget.ChildWidgets[0].ChildWidgets.Count - 1].Widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false))
		{
			return true;
		}
		return false;
	}

	protected override void MarkAsFitted(LayoutContext childContext)
	{
		ParagraphLayoutInfo paragraphLayoutInfo = childContext.LayoutInfo as ParagraphLayoutInfo;
		if (paragraphLayoutInfo != null && m_ltWidget.IsNotFitted)
		{
			(base.LayoutInfo as ParagraphLayoutInfo).IsNotFitted = false;
			if (m_ltWidget.Widget is SplitWidgetContainer)
			{
				m_ltWidget.IsNotFitted = false;
			}
			MarkAsNotFitted(childContext, isFootnote: false);
		}
		else
		{
			AddChildLW(childContext);
			if (childContext is LCContainer && !(childContext is LCLineContainer) && m_currChildLW.IsNotFitted)
			{
				return;
			}
			IEntity prevEntity = null;
			WParagraph paragraph = GetParagraph();
			bool flag = paragraphLayoutInfo?.IsPageBreak ?? false;
			if (flag && paragraph != null && paragraph.IsParagraphHasSectionBreak() && (paragraph.BreakCharacterFormat.Hidden || paragraph.BreakCharacterFormat.IsDeleteRevision) && (!paragraph.IsEmptyParagraph() || paragraph.PreviousSibling == null || paragraph.IsPreviousParagraphMarkIsHidden() || paragraph.IsPreviousParagraphMarkIsInDeletion(ref prevEntity)))
			{
				flag = false;
			}
			m_ltState = LayoutState.Fitted;
			if (flag)
			{
				m_layoutArea.CutFromTop();
				m_ltState = LayoutState.Breaked;
			}
		}
		UpdateForceFitLayoutState(childContext);
	}

	protected override void MarkAsSplitted(LayoutContext childContext)
	{
		ParagraphLayoutInfo paragraphLayoutInfo = base.LayoutInfo as ParagraphLayoutInfo;
		if (paragraphLayoutInfo != null && m_ltWidget.IsNotFitted && m_ltWidget.ChildWidgets.Count > 2)
		{
			paragraphLayoutInfo.IsNotFitted = false;
			m_ltWidget.IsNotFitted = false;
			MarkAsNotFitted(childContext, isFootnote: false);
		}
		else
		{
			AddChildLW(childContext);
			if (childContext is LCContainer && !(childContext is LCLineContainer) && m_currChildLW.IsNotFitted)
			{
				return;
			}
			m_widget = childContext.SplittedWidget;
			m_bAtLastOneChildFitted = true;
			if (paragraphLayoutInfo != null)
			{
				paragraphLayoutInfo.IsFirstLine = false;
			}
		}
		UpdateForceFitLayoutState(childContext);
	}

	protected override void UpdateClientArea()
	{
		ChangeChildsAlignment();
		WParagraph paragraph = GetParagraph();
		if ((m_lcOperator as Layouter).FootnoteWidgets.Count > 0 && !base.IsForceFitLayout && m_currChildLW.Bounds.Width > 0f && m_currChildLW.Bounds.Height > 0f && m_currChildLW.ChildWidgets.Count > 0 && Math.Truncate(m_currChildLW.Bounds.Bottom) > Math.Truncate(m_layoutArea.ClientActiveArea.Bottom) && paragraph != null && paragraph.OwnerTextBody.OwnerBase is WSection && !paragraph.ParagraphFormat.IsInFrame() && paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
		{
			m_currChildLW.IsNotFitted = true;
		}
		if (m_currChildLW.IsNotFitted)
		{
			return;
		}
		RectangleF bounds = m_currChildLW.Bounds;
		float footnoteHeight = GetFootnoteHeight();
		double num = bounds.Bottom;
		IEntity prevEntity = null;
		if (m_currChildLW.Widget is WParagraph && ((m_currChildLW.Widget as WParagraph).IsPreviousParagraphMarkIsHidden() || (m_currChildLW.Widget as WParagraph).IsPreviousParagraphMarkIsInDeletion(ref prevEntity) || (m_lcOperator as Layouter).FieldEntity is WFieldMark) && m_currChildLW.TextTag == "Splitted")
		{
			float pageMarginLeft = GetPageMarginLeft(m_currChildLW.Widget as WParagraph);
			float num2 = 0f;
			if (prevEntity != null && prevEntity is WParagraph && (prevEntity as WParagraph).ListFormat.ListType != ListType.NoList && (prevEntity as WParagraph).m_layoutInfo != null)
			{
				num2 = ((prevEntity as WParagraph).m_layoutInfo as ParagraphLayoutInfo).Margins.Left;
			}
			if (pageMarginLeft != m_currChildLW.Bounds.X)
			{
				m_layoutArea.UpdateLeftPosition(pageMarginLeft + num2);
			}
			if ((m_lcOperator as Layouter).HiddenLineBottom != 0f && (double)(m_lcOperator as Layouter).HiddenLineBottom > num && !((m_lcOperator as Layouter).FieldEntity is WFieldMark))
			{
				num = (m_lcOperator as Layouter).HiddenLineBottom;
			}
			(m_lcOperator as Layouter).HiddenLineBottom = 0f;
		}
		if ((m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems != float.MinValue && (m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems != float.MaxValue)
		{
			m_layoutArea.UpdateLeftPosition((m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems);
			(m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems = float.MaxValue;
			return;
		}
		if ((m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems == float.MaxValue)
		{
			RectangleF clientActiveArea = m_layoutArea.ClientActiveArea;
			clientActiveArea.Width += clientActiveArea.X - (m_lcOperator as Layouter).ClientLayoutArea.X;
			clientActiveArea.X = (m_lcOperator as Layouter).ClientLayoutArea.X;
			m_layoutArea.UpdateClientActiveArea(clientActiveArea);
			(m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems = float.MinValue;
		}
		m_layoutArea.CutFromTop(num, footnoteHeight);
		(m_lcOperator as Layouter).m_canSplitbyCharacter = true;
		(m_lcOperator as Layouter).m_canSplitByTab = false;
		(m_lcOperator as Layouter).IsFirstItemInLine = true;
	}

	private bool IsRTLChar(char character)
	{
		if ((character < '\u0590' || character > '\u05ff') && (character < '\u0600' || character > 'ۿ') && (character < 'ݐ' || character > 'ݿ') && (character < 'ࢠ' || character > '\u08ff') && (character < 'ﭐ' || character > '﷿') && (character < 'ﹰ' || character > '\ufeff') && (character < '\ua980' || character > '꧟') && (character < '܀' || character > 'ݏ') && (character < 'ހ' || character > '\u07bf') && (character < 'ࡀ' || character > '\u085f') && (character < '߀' || character > '߿') && (character < 'ࠀ' || character > '\u083f'))
		{
			if (character >= 'ⴰ')
			{
				return character <= '\u2d7f';
			}
			return false;
		}
		return true;
	}

	private bool IsRTLText(string text)
	{
		if (text != null)
		{
			foreach (char character in text)
			{
				if (IsRTLChar(character))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsLineContainsRTL()
	{
		foreach (LayoutedWidget childWidget in m_currChildLW.ChildWidgets)
		{
			if (childWidget.Widget is WTextRange || childWidget.Widget is SplitStringWidget)
			{
				string text = ((childWidget.Widget is SplitStringWidget) ? (childWidget.Widget as SplitStringWidget).SplittedText : (childWidget.Widget as WTextRange).Text);
				if (IsRTLText(text))
				{
					return true;
				}
			}
		}
		return false;
	}

	protected override void ChangeChildsAlignment()
	{
		WParagraph paragraph = GetParagraph();
		WSection ownerSection = paragraph.GetOwnerSection();
		if (((m_currChildLW.Widget is WParagraph && m_currChildLW.TextTag != "Splitted" && m_currChildLW.ChildWidgets.Count > 0) || (m_currChildLW.Widget is SplitWidgetContainer && (m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph && m_currChildLW.TextTag != "Splitted" && m_currChildLW.ChildWidgets.Count > 0)) && !base.IsInnerLayouting && (!(paragraph.GetOwnerEntity() is WSection) || !(m_ltWidget.Widget is SplitWidgetContainer)))
		{
			paragraph.RemoveSplitStringWidget();
		}
		float bottom = m_currChildLW.Bounds.Bottom;
		m_currChildLW.AlignBottom(base.DrawingContext, m_layoutArea.ClientActiveArea.Height, m_layoutArea.ClientActiveArea.Bottom, (m_lcOperator as Layouter).IsRowFitInSamePage, (m_lcOperator as Layouter).IsLayoutingHeaderRow, (m_lcOperator as Layouter).IsLayoutingHeaderFooter, base.IsForceFitLayout);
		ShiftTrackChangesBalloons(m_currChildLW.Bounds.Y, m_currChildLW.Bounds.Bottom, bottom);
		if (m_currChildLW.IsNotFitted || IsDisplayMath())
		{
			return;
		}
		base.DrawingContext.UpdateTabPosition(m_currChildLW, m_layoutArea.ClientActiveArea);
		bool flag = false;
		if (m_currChildLW.ChildWidgets.Count > 0)
		{
			WTextRange wTextRange = null;
			LayoutedWidget layoutedWidget = null;
			for (int num = m_currChildLW.ChildWidgets.Count - 1; num >= 0; num--)
			{
				layoutedWidget = m_currChildLW.ChildWidgets[num];
				if (layoutedWidget.Widget is WTextRange || layoutedWidget.Widget is SplitStringWidget)
				{
					wTextRange = ((layoutedWidget.Widget is SplitStringWidget) ? ((layoutedWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange) : (layoutedWidget.Widget as WTextRange));
					if (wTextRange.Text == '\u001f'.ToString())
					{
						if (wTextRange.OwnerParagraph != null && wTextRange.OwnerParagraph.Text.Replace('\u001f'.ToString(), string.Empty) != string.Empty)
						{
							flag = true;
						}
						break;
					}
					if (wTextRange.Text != string.Empty)
					{
						break;
					}
				}
			}
			if (flag && wTextRange != null && layoutedWidget != null)
			{
				StringFormat stringFormat = new StringFormat(base.DrawingContext.StringFormt);
				if (wTextRange.CharacterFormat.Bidi)
				{
					stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
				}
				else
				{
					stringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
				}
				layoutedWidget.Bounds = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, base.DrawingContext.MeasureString("-", layoutedWidget.Widget.LayoutInfo.Font.GetFont(wTextRange.Document, wTextRange.ScriptType), stringFormat, wTextRange.ScriptType).Width, layoutedWidget.Bounds.Height);
				int num2 = m_currChildLW.ChildWidgets.IndexOf(layoutedWidget);
				if (num2 != m_currChildLW.ChildWidgets.Count - 1)
				{
					for (int i = num2 + 1; i < m_currChildLW.ChildWidgets.Count; i++)
					{
						m_currChildLW.ChildWidgets[i].ShiftLocation(layoutedWidget.Bounds.Width, 0.0, isPictureNeedToBeShifted: false, isFromFloatingItemVerticalAlignment: false);
					}
				}
			}
		}
		bool flag2 = false;
		flag2 = ((m_currChildLW.ChildWidgets.Count > 0 && m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Widget is Break) ? (m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Widget as Break).IsCarriageReturn() : flag2);
		ParagraphLayoutInfo paragraphLayoutInfo = m_currChildLW.Widget.LayoutInfo as ParagraphLayoutInfo;
		HAlignment hAlignment = paragraphLayoutInfo?.Justification ?? HAlignment.Left;
		if ((m_lcOperator as Layouter).UnknownField != null && (m_lcOperator as Layouter).UnknownField.OwnerParagraph.m_layoutInfo is ParagraphLayoutInfo)
		{
			hAlignment = ((m_lcOperator as Layouter).UnknownField.OwnerParagraph.m_layoutInfo as ParagraphLayoutInfo).Justification;
		}
		if (!IsDisplayMath() && IsLineContainsOnlyMathItems(m_currChildLW))
		{
			hAlignment = HAlignment.Center;
		}
		double subWidth = 0.0;
		double subWidthBeforeSpaceTrim = 0.0;
		float num3 = paragraphLayoutInfo.Margins.Right;
		WParagraph paragraph2 = GetParagraph();
		if (paragraph2 != null && !(m_lcOperator as Layouter).IsLayoutingHeaderFooter && paragraph2.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
		{
			bool flag3 = false;
			if (m_currChildLW.IntersectingBounds.Count <= 0)
			{
				UpdateItersectingFloatingItemBounds();
				flag3 = m_currChildLW.IntersectingBounds.Count > 0;
			}
			bool flag4 = false;
			List<RectangleF> intersectingBounds = m_currChildLW.IntersectingBounds;
			for (int j = 0; j < intersectingBounds.Count; j++)
			{
				RectangleF rectangleF = intersectingBounds[j];
				if (m_currChildLW.Bounds.Right < rectangleF.X)
				{
					flag4 = true;
					break;
				}
			}
			if (flag4)
			{
				num3 = 0f;
			}
			if (flag3 && !paragraph2.Document.IsNeedToAddLineNumbers())
			{
				m_currChildLW.IntersectingBounds.Clear();
			}
		}
		float num4 = 0f;
		float xPosition = 0f;
		LayoutedWidget layoutedWidget2 = new LayoutedWidget(m_currChildLW);
		LayoutedWidget layoutedWidget3 = new LayoutedWidget(m_currChildLW);
		layoutedWidget3.ChildWidgets.Clear();
		RectangleF interSectingFloattingItem = new RectangleF(0f, 0f, 0f, 0f);
		bool flag5 = paragraph != null && IsInFrame(paragraph);
		float num5 = 0f;
		while (layoutedWidget2.ChildWidgets.Count > 0)
		{
			SplitLineBasedOnInterSectingFlotingEntity(layoutedWidget2, ref interSectingFloattingItem, layoutedWidget3);
			if (m_currChildLW.ChildWidgets.Count <= 0)
			{
				subWidth = ((interSectingFloattingItem.Bottom == 0f) ? ((double)(m_layoutArea.ClientActiveArea.Right - m_currChildLW.Bounds.Right - num3)) : ((double)(interSectingFloattingItem.X - m_currChildLW.Bounds.Right - num3)));
			}
			else
			{
				for (int k = 0; k < m_currChildLW.ChildWidgets.Count; k++)
				{
					EntityType entityType = EntityType.WordDocument;
					if (m_currChildLW.ChildWidgets[k].Widget is SplitStringWidget)
					{
						entityType = EntityType.TextRange;
					}
					else if (m_currChildLW.ChildWidgets[k].Widget is Entity)
					{
						entityType = (m_currChildLW.ChildWidgets[k].Widget as Entity).EntityType;
					}
					if (m_currChildLW.ChildWidgets[k].Widget is WTextRange)
					{
						entityType = EntityType.TextRange;
					}
					switch (entityType)
					{
					case EntityType.Symbol:
						num4 = m_currChildLW.ChildWidgets[k].Bounds.Right;
						if (flag5)
						{
							num5 += m_currChildLW.ChildWidgets[k].Bounds.Width;
						}
						break;
					case EntityType.TextBox:
						if ((m_currChildLW.ChildWidgets[k].Widget as WTextBox).TextBoxFormat.TextWrappingStyle == TextWrappingStyle.Inline)
						{
							num4 = m_currChildLW.ChildWidgets[k].Bounds.Right;
							if (flag5)
							{
								num5 += m_currChildLW.ChildWidgets[k].Bounds.Width;
							}
						}
						break;
					case EntityType.Picture:
					case EntityType.OleObject:
					{
						WPicture wPicture = ((m_currChildLW.ChildWidgets[k].Widget is WOleObject) ? (m_currChildLW.ChildWidgets[k].Widget as WOleObject).OlePicture : (m_currChildLW.ChildWidgets[k].Widget as WPicture));
						if (wPicture != null && wPicture.TextWrappingStyle == TextWrappingStyle.Inline)
						{
							num4 = m_currChildLW.ChildWidgets[k].Bounds.Right;
							if (flag5)
							{
								num5 += m_currChildLW.ChildWidgets[k].Bounds.Width;
							}
						}
						break;
					}
					case EntityType.AutoShape:
						if (m_currChildLW.ChildWidgets[k].Widget is Shape shape && shape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline)
						{
							num4 = m_currChildLW.ChildWidgets[k].Bounds.Right;
							if (flag5)
							{
								num5 += m_currChildLW.ChildWidgets[k].Bounds.Width;
							}
						}
						break;
					case EntityType.GroupShape:
						if (m_currChildLW.ChildWidgets[k].Widget is GroupShape groupShape && groupShape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline)
						{
							num4 = m_currChildLW.ChildWidgets[k].Bounds.Right;
						}
						break;
					case EntityType.Chart:
						if (m_currChildLW.ChildWidgets[k].Widget is WChart wChart && wChart.WrapFormat.TextWrappingStyle == TextWrappingStyle.Inline)
						{
							num4 = m_currChildLW.ChildWidgets[k].Bounds.Right;
							if (flag5)
							{
								num5 += m_currChildLW.ChildWidgets[k].Bounds.Width;
							}
						}
						break;
					case EntityType.TextRange:
						if (m_currChildLW.ChildWidgets[k].Bounds.Width < 0f)
						{
							num4 = m_currChildLW.ChildWidgets[k].Bounds.Left;
							xPosition = m_currChildLW.ChildWidgets[k].Bounds.X;
						}
						else
						{
							num4 = m_currChildLW.ChildWidgets[k].Bounds.Right;
							xPosition = m_currChildLW.ChildWidgets[k].Bounds.X;
						}
						if (flag5)
						{
							num5 += m_currChildLW.ChildWidgets[k].Bounds.Width;
						}
						break;
					case EntityType.Math:
						num4 = m_currChildLW.ChildWidgets[k].Bounds.Right;
						break;
					}
					if (num4 == 0f && paragraphLayoutInfo.ListValue != string.Empty)
					{
						num4 = m_currChildLW.ChildWidgets[k].Bounds.Right;
					}
					if (num4 == 0f && !paragraph.IsInCell && m_currChildLW.ChildWidgets[k].Widget is Entity && (m_currChildLW.ChildWidgets[k].Widget as Entity).IsFloatingItem(isTextWrapAround: false))
					{
						ParagraphItem obj = ((m_currChildLW.ChildWidgets[k].Widget is WOleObject) ? (m_currChildLW.ChildWidgets[k].Widget as WOleObject).OlePicture : (m_currChildLW.ChildWidgets[k].Widget as ParagraphItem));
						HorizontalOrigin horizontalOrigin = obj.GetHorizontalOrigin();
						ShapeHorizontalAlignment shapeHorizontalAlignment = obj.GetShapeHorizontalAlignment();
						if (horizontalOrigin == HorizontalOrigin.Character && shapeHorizontalAlignment == ShapeHorizontalAlignment.None)
						{
							num4 = ownerSection.PageSetup.Margins.Left;
						}
					}
				}
				subWidth = ((interSectingFloattingItem.Bottom == 0f) ? ((double)(m_layoutArea.ClientActiveArea.Right - num4 - num3)) : ((double)(interSectingFloattingItem.X - num4 - num3)));
			}
			if (interSectingFloattingItem.Bottom == 0f && hAlignment != HAlignment.Right)
			{
				UpdateSubWidthBasedOnTextWrap(paragraph, ref subWidth, xPosition, num3);
			}
			if (paragraph != null && IsInFrame(paragraph))
			{
				if (paragraph.ParagraphFormat.FrameWidth == 0f)
				{
					subWidth = ((!paragraph.ParagraphFormat.IsNextParagraphInSameFrame() && !paragraph.ParagraphFormat.IsPreviousParagraphInSameFrame()) ? 0.0 : ((double)(m_layoutArea.ClientActiveArea.Right - m_currChildLW.Bounds.Right - num3)));
				}
				else
				{
					subWidth = m_layoutArea.ClientActiveArea.Right - num4 - num3;
					for (int l = 0; l < layoutedWidget3.ChildWidgets.Count; l++)
					{
						subWidth -= (double)layoutedWidget3.ChildWidgets[l].Bounds.Width;
					}
				}
			}
			if (subWidth < 0.0)
			{
				if ((m_lcOperator as Layouter).MaxRightPositionOfTabStopInterSectingFloattingItems != float.MinValue)
				{
					subWidth = (m_lcOperator as Layouter).MaxRightPositionOfTabStopInterSectingFloattingItems - num4;
					(m_lcOperator as Layouter).MaxRightPositionOfTabStopInterSectingFloattingItems = float.MinValue;
				}
				else if (m_currChildLW.ChildWidgets.Count > 0 && m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].TextTag == "IsLastWordFit")
				{
					m_currChildLW.ChildWidgets[0].TextTag = null;
				}
				else if (IsNotWord2013Jusitfy(paragraph))
				{
					subWidth = 0.0;
				}
			}
			if (((m_currChildLW.Widget is WParagraph && m_currChildLW.TextTag != "Splitted" && m_currChildLW.ChildWidgets.Count > 0) || (m_currChildLW.Widget is SplitWidgetContainer && (m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph && m_currChildLW.TextTag != "Splitted" && m_currChildLW.ChildWidgets.Count > 0)) && m_currChildLW.ChildWidgets.Count > 0 && hAlignment == HAlignment.Justify && (interSectingFloattingItem.Bottom == 0f || layoutedWidget2.ChildWidgets.Count == 0))
			{
				flag2 = ((!(subWidth < 0.0) || !IsWord2013(paragraph.Document)) ? true : false);
				if (!flag2)
				{
					m_currChildLW.IsLastLine = true;
				}
			}
			float num6 = 0f;
			WParagraphFormat currentTabFormat = base.DrawingContext.GetCurrentTabFormat(paragraph);
			if (m_currChildLW.ChildWidgets.Count > 0 && paragraph.IsLastLine(m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1]) && paragraph.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && paragraph.Document.DOP.Dop2000.Copts.ForgetLastTabAlign && currentTabFormat != null && currentTabFormat.Tabs != null && currentTabFormat.Tabs.Count > 0 && subWidth > 0.0)
			{
				num6 = GetLastTabWidth(currentTabFormat, currentTabFormat.Tabs.Count);
			}
			switch (hAlignment)
			{
			case HAlignment.Center:
				if (interSectingFloattingItem.Bottom != 0f && num4 == 0f && layoutedWidget2.ChildWidgets.Count > 0)
				{
					subWidth -= (double)layoutedWidget2.Bounds.Width;
				}
				subWidth = subWidth / 2.0 + (double)(num6 / 2f);
				m_currChildLW.AlignCenter(base.DrawingContext, subWidth, isAlignCenter: true);
				break;
			case HAlignment.Right:
				if (interSectingFloattingItem.Bottom != 0f && num4 == 0f)
				{
					subWidth = interSectingFloattingItem.Width - m_currChildLW.Bounds.Width - num3;
					if (layoutedWidget2.ChildWidgets.Count > 0)
					{
						subWidth -= (double)layoutedWidget2.Bounds.Width;
					}
				}
				subWidth += (double)num6;
				subWidthBeforeSpaceTrim = subWidth;
				subWidth = m_currChildLW.AlignRight(base.DrawingContext, subWidth, isAlignCenter: false);
				break;
			case HAlignment.Justify:
			case HAlignment.Distributed:
				if (!flag2)
				{
					if (layoutedWidget3.ChildWidgets.Count > 0 || interSectingFloattingItem.Bottom != 0f)
					{
						m_currChildLW.AlignJustify(base.DrawingContext, subWidth, layoutedWidget3.ChildWidgets.Count > 0, isParaBidi: false);
					}
					for (int m = 0; m < m_currChildLW.ChildWidgets.Count; m++)
					{
						m_currChildLW.ChildWidgets[m].HorizontalAlign = hAlignment;
					}
				}
				break;
			}
			if (interSectingFloattingItem.Bottom != 0f)
			{
				layoutedWidget3.ChildWidgets.AddRange(m_currChildLW.ChildWidgets);
				num4 = 0f;
				continue;
			}
			if (layoutedWidget3.ChildWidgets.Count > 0)
			{
				layoutedWidget3.ChildWidgets.AddRange(m_currChildLW.ChildWidgets);
			}
			break;
		}
		bool flag6 = false;
		bool flag7 = false;
		LayoutedWidgetList layoutedWidgetList = null;
		if (!paragraph.Document.Settings.CompatibilityOptions[CompatibilityOption.ExpShRtn] && (m_currChildLW.Widget.LayoutInfo as ParagraphLayoutInfo).Justification == HAlignment.Justify && m_currChildLW.ChildWidgets.Count > 0 && m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Widget is Break && ((m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Widget as Break).BreakType == BreakType.LineBreak || (m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Widget as Break).BreakType == BreakType.TextWrappingBreak))
		{
			for (int n = 0; n < m_currChildLW.ChildWidgets.Count; n++)
			{
				m_currChildLW.ChildWidgets[n].HorizontalAlign = HAlignment.Left;
			}
			hAlignment = HAlignment.Left;
		}
		if (layoutedWidget3.ChildWidgets.Count > 0)
		{
			flag6 = ShiftWidgetsForRTLLayouting(layoutedWidget3);
		}
		else
		{
			flag7 = IsLineContainsRTL();
			layoutedWidgetList = m_currChildLW.ChildWidgets;
			flag6 = ShiftWidgetsForRTLLayouting(subWidth, subWidthBeforeSpaceTrim, hAlignment, layoutedWidget3.ChildWidgets.Count > 0, flag2, flag7);
			if (!flag2 && (hAlignment == HAlignment.Justify || hAlignment == HAlignment.Distributed))
			{
				m_currChildLW.AlignJustify(base.DrawingContext, subWidth, layoutedWidget3.ChildWidgets.Count > 0, flag6);
			}
		}
		if (layoutedWidget3.ChildWidgets.Count > 0)
		{
			m_currChildLW.ChildWidgets.RemoveRange(0, m_currChildLW.ChildWidgets.Count);
			for (int num7 = 0; num7 < layoutedWidget3.ChildWidgets.Count; num7++)
			{
				m_currChildLW.ChildWidgets.Add(layoutedWidget3.ChildWidgets[num7]);
				layoutedWidget3.ChildWidgets[num7].Owner = m_currChildLW;
			}
			m_ltWidget.ChildWidgets.RemoveAt(m_ltWidget.ChildWidgets.Count - 1);
			m_ltWidget.ChildWidgets.Add(m_currChildLW);
			m_currChildLW.Owner = m_ltWidget;
		}
		else if (layoutedWidgetList != null)
		{
			m_currChildLW.ChildWidgets = layoutedWidgetList;
		}
		if (paragraph != null && paragraph.Document.RevisionOptions.CommentDisplayMode == CommentDisplayMode.ShowInBalloons && !(m_lcOperator as Layouter).IsLayoutingTrackChanges && (hAlignment == HAlignment.Center || hAlignment == HAlignment.Right || hAlignment == HAlignment.Justify || hAlignment == HAlignment.Distributed || flag7))
		{
			UpdateXPositionOfCommentBalloon();
		}
		if (paragraph != null && hAlignment != 0 && hAlignment != HAlignment.Justify && !(m_lcOperator as Layouter).IsLayoutingTrackChanges)
		{
			TrackChangesAlignment((float)subWidth);
		}
	}

	private void TrackChangesAlignment(float subWidth)
	{
		if ((m_lcOperator as Layouter).TrackChangesMarkups.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < (m_lcOperator as Layouter).TrackChangesMarkups.Count; i++)
		{
			if ((m_lcOperator as Layouter).TrackChangesMarkups[i].IsAligned || Math.Round(m_currChildLW.Bounds.Y, 2) != Math.Round((m_lcOperator as Layouter).TrackChangesMarkups[i].BallonYPosition, 2))
			{
				continue;
			}
			if (m_currChildLW.Widget is WParagraph && (m_currChildLW.Widget as WParagraph).PreviousSibling is WParagraph && ((m_currChildLW.Widget as WParagraph).PreviousSibling as WParagraph).IsDeleteRevision)
			{
				if (!((m_currChildLW.Widget as WParagraph).PreviousSibling as WParagraph).ParagraphFormat.Bidi)
				{
					(m_lcOperator as Layouter).TrackChangesMarkups[i].Position = new PointF(m_currChildLW.Bounds.X, (m_lcOperator as Layouter).TrackChangesMarkups[i].Position.Y);
				}
				else
				{
					(m_lcOperator as Layouter).TrackChangesMarkups[i].Position = new PointF(m_currChildLW.Bounds.Right, (m_lcOperator as Layouter).TrackChangesMarkups[i].Position.Y);
				}
			}
			else
			{
				(m_lcOperator as Layouter).TrackChangesMarkups[i].Position = new PointF((m_lcOperator as Layouter).TrackChangesMarkups[i].Position.X + subWidth, (m_lcOperator as Layouter).TrackChangesMarkups[i].Position.Y);
			}
			(m_lcOperator as Layouter).TrackChangesMarkups[i].IsAligned = true;
		}
	}

	private void UpdateXPositionOfCommentBalloon()
	{
		foreach (LayoutedWidget childWidget in m_currChildLW.ChildWidgets)
		{
			if (!(childWidget.Widget is WCommentMark))
			{
				continue;
			}
			WCommentMark wCommentMark = childWidget.Widget as WCommentMark;
			if (wCommentMark.Type != CommentMarkType.CommentEnd && (wCommentMark.Type != 0 || wCommentMark.Comment == null || wCommentMark.Comment.CommentRangeEnd != null))
			{
				continue;
			}
			foreach (TrackChangesMarkups trackChangesMarkup in (m_lcOperator as Layouter).TrackChangesMarkups)
			{
				if (trackChangesMarkup is CommentsMarkups && (trackChangesMarkup as CommentsMarkups).CommentID == wCommentMark.CommentId)
				{
					(trackChangesMarkup as CommentsMarkups).Position = new PointF(childWidget.Bounds.X, trackChangesMarkup.Position.Y);
					trackChangesMarkup.IsAligned = true;
				}
			}
		}
	}

	private bool IsDisplayMath()
	{
		foreach (LayoutedWidget childWidget in m_currChildLW.ChildWidgets)
		{
			if ((!(childWidget.Widget is WMath) || (childWidget.Widget as WMath).IsInline) && !(childWidget.Widget is BookmarkStart) && !(childWidget.Widget is BookmarkEnd))
			{
				return false;
			}
		}
		return m_currChildLW.ChildWidgets.Count > 0;
	}

	private bool HasTextRangeBidi(LayoutedWidgetList layoutedWidgets)
	{
		foreach (LayoutedWidget layoutedWidget in layoutedWidgets)
		{
			if ((layoutedWidget.Widget is WTextRange || layoutedWidget.Widget is SplitStringWidget) && ((layoutedWidget.Widget is SplitStringWidget) ? ((layoutedWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange) : (layoutedWidget.Widget as WTextRange)).CharacterFormat.Bidi)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateCharacterRange(int i, int rtlStartIndex, List<bool> splittedWidgetBidiValues, ref List<CharacterRangeType> characterRangeTypes)
	{
		int num = i;
		if (!splittedWidgetBidiValues[i])
		{
			if (characterRangeTypes[i] == CharacterRangeType.LTR)
			{
				num--;
			}
			for (int num2 = num; num2 >= rtlStartIndex; num2--)
			{
				if (characterRangeTypes[num2] != CharacterRangeType.WordSplit)
				{
					num = num2;
					break;
				}
			}
		}
		for (int j = rtlStartIndex; j <= num; j++)
		{
			if (characterRangeTypes[j] != CharacterRangeType.WordSplit)
			{
				continue;
			}
			characterRangeTypes[j] = (CharacterRangeType)3;
			int num3 = j - 1;
			int num4 = j + 1;
			if (num3 >= 0 && num4 < characterRangeTypes.Count && characterRangeTypes[num3] == CharacterRangeType.RTL && (characterRangeTypes[num4] == CharacterRangeType.RTL || characterRangeTypes[num4] == CharacterRangeType.Number) && m_currChildLW.ChildWidgets[j].Widget is WTextRange)
			{
				IWTextRange iWTextRange = (WTextRange)m_currChildLW.ChildWidgets[j].Widget;
				if (iWTextRange.CharacterFormat.FontNameFarEast == "Times New Roman")
				{
					char[] array = iWTextRange.Text.ToCharArray();
					Array.Reverse(array);
					iWTextRange.Text = new string(array);
				}
			}
		}
	}

	private LayoutedWidget GetNextValidWidget(int startIndex, LayoutedWidgetList layoutedWidgets)
	{
		if (startIndex < layoutedWidgets.Count)
		{
			_ = layoutedWidgets[startIndex];
			return layoutedWidgets[startIndex];
		}
		return null;
	}

	private bool ShiftWidgetsForRTLLayouting(double subWidth, double subWidthBeforeSpaceTrim, HAlignment alignment, bool hasIntersectingFloatingItem, bool isLastLine, bool isContainsRTL)
	{
		bool isAutoFrame = false;
		bool flag = false;
		bool flag2 = HasTextRangeBidi(m_currChildLW.ChildWidgets);
		if (m_currChildLW.Widget is WParagraph || (m_currChildLW.Widget is SplitWidgetContainer && (m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph))
		{
			if (m_currChildLW.Widget is WParagraph)
			{
				WParagraph wParagraph = m_currChildLW.Widget as WParagraph;
				isAutoFrame = wParagraph.ParagraphFormat.IsFrame && wParagraph.ParagraphFormat.FrameWidth == 0f;
				flag = wParagraph.ParagraphFormat.Bidi;
			}
			else
			{
				flag = ((m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).ParagraphFormat.Bidi;
			}
		}
		if (isContainsRTL || flag || flag2)
		{
			List<CharacterRangeType> characterRangeTypes = new List<CharacterRangeType>();
			List<bool> list = new List<bool>();
			for (int i = 0; i < m_currChildLW.ChildWidgets.Count; i++)
			{
				LayoutedWidget layoutedWidget = m_currChildLW.ChildWidgets[i];
				if ((layoutedWidget.Widget is WTextRange || layoutedWidget.Widget is SplitStringWidget) && layoutedWidget.Bounds.Height > 0f)
				{
					WTextRange wTextRange = ((layoutedWidget.Widget is SplitStringWidget) ? ((layoutedWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange) : (layoutedWidget.Widget as WTextRange));
					list.Add(wTextRange.CharacterFormat.Bidi);
					if (layoutedWidget.Widget.LayoutInfo is TabsLayoutInfo)
					{
						characterRangeTypes.Add(CharacterRangeType.Tab);
					}
					else
					{
						characterRangeTypes.Add(wTextRange.CharacterRange);
					}
				}
				else if (layoutedWidget.Widget is WCommentMark)
				{
					WCommentMark wCommentMark = layoutedWidget.Widget as WCommentMark;
					if (wCommentMark.Type == CommentMarkType.CommentStart && i < m_currChildLW.ChildWidgets.Count - 1)
					{
						LayoutedWidget nextValidWidget = GetNextValidWidget(i + 1, m_currChildLW.ChildWidgets);
						if (nextValidWidget != null && (nextValidWidget.Widget is WTextRange || nextValidWidget.Widget is SplitStringWidget) && nextValidWidget.Bounds.Height > 0f)
						{
							WTextRange wTextRange2 = ((nextValidWidget.Widget is SplitStringWidget) ? ((nextValidWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange) : (nextValidWidget.Widget as WTextRange));
							list.Add(wTextRange2.CharacterFormat.Bidi);
							if (nextValidWidget.Widget.LayoutInfo is TabsLayoutInfo)
							{
								characterRangeTypes.Add(CharacterRangeType.Tab);
							}
							else
							{
								characterRangeTypes.Add(wTextRange2.CharacterRange);
							}
						}
						else
						{
							list.Add(item: false);
							characterRangeTypes.Add(CharacterRangeType.LTR);
						}
					}
					else if (wCommentMark.Type == CommentMarkType.CommentEnd && i > 0)
					{
						list.Add(list[list.Count - 1]);
						characterRangeTypes.Add(characterRangeTypes[characterRangeTypes.Count - 1]);
					}
					else
					{
						list.Add(item: false);
						characterRangeTypes.Add(CharacterRangeType.LTR);
					}
				}
				else
				{
					list.Add(item: false);
					characterRangeTypes.Add(CharacterRangeType.LTR);
				}
			}
			int num = -1;
			bool? flag3 = null;
			for (int j = 0; j < characterRangeTypes.Count; j++)
			{
				if (j + 1 < list.Count && list[j] != list[j + 1])
				{
					if (num != -1)
					{
						UpdateCharacterRange(j, num, list, ref characterRangeTypes);
						num = -1;
					}
					flag3 = null;
					continue;
				}
				if (j > 0 && j != characterRangeTypes.Count - 1 && characterRangeTypes[j] == CharacterRangeType.WordSplit && list[j] && characterRangeTypes[j - 1] == CharacterRangeType.Number && list[j - 1] && characterRangeTypes[j + 1] == CharacterRangeType.Number && list[j + 1] && IsNumberNonReversingCharacter(m_currChildLW.ChildWidgets[j]))
				{
					characterRangeTypes[j] = CharacterRangeType.Number;
				}
				else if (characterRangeTypes[j] == CharacterRangeType.RTL || characterRangeTypes[j] == CharacterRangeType.LTR || (characterRangeTypes[j] == CharacterRangeType.Number && num != -1) || ((!flag3.HasValue || !flag3.Value) && list[j]))
				{
					if (num == -1 && characterRangeTypes[j] != 0)
					{
						num = j;
					}
					else
					{
						if (num == -1)
						{
							if (characterRangeTypes[j] == CharacterRangeType.LTR)
							{
								flag3 = true;
							}
							else if (characterRangeTypes[j] == CharacterRangeType.RTL)
							{
								flag3 = false;
							}
							continue;
						}
						if (characterRangeTypes[j] == CharacterRangeType.LTR)
						{
							UpdateCharacterRange(j, num, list, ref characterRangeTypes);
							num = ((characterRangeTypes[j] == CharacterRangeType.RTL || (characterRangeTypes[j] == CharacterRangeType.Number && num != -1)) ? j : (-1));
						}
					}
				}
				if (characterRangeTypes[j] == CharacterRangeType.LTR)
				{
					flag3 = true;
				}
				else if (characterRangeTypes[j] == CharacterRangeType.RTL)
				{
					flag3 = false;
				}
			}
			if (num != -1 && num < characterRangeTypes.Count - 1)
			{
				UpdateCharacterRange(characterRangeTypes.Count - 1, num, list, ref characterRangeTypes);
				num = -1;
			}
			if (characterRangeTypes.Count != m_currChildLW.ChildWidgets.Count)
			{
				throw new Exception("Splitted Widget count mismatch while reordering layouted child widgets of a line");
			}
			LayoutedWidgetList layoutedWidgetList = ReorderWidgets(characterRangeTypes, list, flag);
			list.Clear();
			characterRangeTypes.Clear();
			if (m_currChildLW.ChildWidgets.Count > 0)
			{
				double trimmedSpaceDiff = 0.0;
				if (subWidthBeforeSpaceTrim != 0.0 && subWidthBeforeSpaceTrim != subWidth && m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1] != layoutedWidgetList[layoutedWidgetList.Count - 1])
				{
					trimmedSpaceDiff = subWidth - subWidthBeforeSpaceTrim;
					subWidth = subWidthBeforeSpaceTrim;
				}
				UpdateBounds(layoutedWidgetList, flag, alignment, subWidth, trimmedSpaceDiff, isAutoFrame);
				if (!flag)
				{
					m_currChildLW.ChildWidgets = layoutedWidgetList;
				}
				else
				{
					layoutedWidgetList.Reverse();
					m_currChildLW.ChildWidgets = layoutedWidgetList;
				}
			}
		}
		return flag;
	}

	private bool ShiftWidgetsForRTLLayouting(LayoutedWidget resultedWidget)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		if (m_currChildLW.Widget is WParagraph || (m_currChildLW.Widget is SplitWidgetContainer && (m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph))
		{
			flag3 = ((m_currChildLW.Widget is WParagraph) ? (m_currChildLW.Widget as WParagraph).ParagraphFormat.Bidi : ((m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).ParagraphFormat.Bidi);
		}
		WTextRange wTextRange = null;
		int num = -1;
		int lastRtlTextIndex = -1;
		int num2 = -1;
		for (int i = 0; i < m_currChildLW.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = m_currChildLW.ChildWidgets[i];
			if (!(layoutedWidget.Widget is WTextRange) && !(layoutedWidget.Widget is SplitStringWidget))
			{
				continue;
			}
			wTextRange = ((layoutedWidget.Widget is SplitStringWidget) ? ((layoutedWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange) : (layoutedWidget.Widget as WTextRange));
			if (IsRTLText(wTextRange.Text) || wTextRange.CharacterFormat.Bidi || wTextRange.CharacterFormat.BiDirectionalOverride == BiDirectionalOverride.RTL)
			{
				if (num == -1)
				{
					num = i;
				}
				lastRtlTextIndex = i;
				flag = true;
			}
			else
			{
				if (num2 == -1)
				{
					num2 = i;
				}
				flag2 = true;
			}
		}
		if (!flag3)
		{
			ShiftRTLText(num, flag, flag2);
		}
		if (flag3)
		{
			LayoutedWidget layoutedWidget2 = null;
			if (resultedWidget.ChildWidgets.Count > 0)
			{
				float x = m_layoutArea.ClientActiveArea.X;
				float right = m_layoutArea.ClientActiveArea.Right;
				for (int j = 0; j < resultedWidget.ChildWidgets.Count; j++)
				{
					layoutedWidget2 = resultedWidget.ChildWidgets[j];
					if (layoutedWidget2.Widget is SplitStringWidget || (layoutedWidget2.Widget is ParagraphItem && !(layoutedWidget2.Widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false)))
					{
						float num3 = layoutedWidget2.Bounds.Right - x;
						layoutedWidget2.Bounds = new RectangleF(new PointF(right - num3, layoutedWidget2.Bounds.Y), layoutedWidget2.Bounds.Size);
					}
				}
			}
			else if (m_currChildLW.ChildWidgets.Count > 1)
			{
				if (flag && !flag2)
				{
					float right2 = m_currChildLW.Bounds.Right;
					float x2 = m_currChildLW.Bounds.X;
					ParagraphLayoutInfo paragraphLayoutInfo = m_currChildLW.Widget.LayoutInfo as ParagraphLayoutInfo;
					float listWidthToShiftLine = GetListWidthToShiftLine();
					right2 -= ((listWidthToShiftLine == 0f) ? paragraphLayoutInfo.Margins.Left : listWidthToShiftLine);
					for (int k = 0; k < m_currChildLW.ChildWidgets.Count; k++)
					{
						layoutedWidget2 = m_currChildLW.ChildWidgets[k];
						if (layoutedWidget2.Widget is WTextRange || layoutedWidget2.Widget is SplitStringWidget || !(layoutedWidget2.Widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false))
						{
							float x3 = right2 - layoutedWidget2.Bounds.Right + x2;
							layoutedWidget2.Bounds = new RectangleF(new PointF(x3, layoutedWidget2.Bounds.Y), layoutedWidget2.Bounds.Size);
						}
					}
				}
				else if (!flag && flag2)
				{
					ShiftLineForListWidth();
				}
				else if (flag)
				{
					ShiftRTLAndNormalText(lastRtlTextIndex, num2, flag2);
				}
			}
			else if (m_currChildLW.ChildWidgets.Count == 1 && ((flag && !flag2) || (flag2 && !flag)))
			{
				ShiftLineForListWidth();
			}
		}
		return flag3;
	}

	private float GetStartPosition(bool paraBidi, HAlignment alignment, double subWidth, double trimmedSpaceDiff, bool isAutoFrame)
	{
		float x = m_layoutArea.ClientActiveArea.X;
		float right = m_layoutArea.ClientActiveArea.Right;
		if (isAutoFrame)
		{
			x = m_currChildLW.ChildWidgets[0].Bounds.X;
			right = m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Bounds.Right;
		}
		float firstNonFloatingItemX = GetFirstNonFloatingItemX();
		firstNonFloatingItemX -= (float)trimmedSpaceDiff;
		float num = firstNonFloatingItemX - x;
		if (alignment == HAlignment.Right)
		{
			num -= (float)subWidth;
		}
		if (alignment == HAlignment.Left)
		{
			num += (float)subWidth;
		}
		if (paraBidi)
		{
			return right - num;
		}
		return firstNonFloatingItemX;
	}

	private void UpdateBounds(LayoutedWidgetList reorderedWidgets, bool paraBidi, HAlignment alignment, double subWidth, double trimmedSpaceDiff, bool isAutoFrame)
	{
		float num = GetStartPosition(paraBidi, alignment, subWidth, trimmedSpaceDiff, isAutoFrame);
		if (paraBidi)
		{
			for (int num2 = reorderedWidgets.Count - 1; num2 >= 0; num2--)
			{
				LayoutedWidget layoutedWidget = reorderedWidgets[num2];
				if (!(layoutedWidget.Widget is ParagraphItem) || !(layoutedWidget.Widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false))
				{
					if (layoutedWidget.Widget.LayoutInfo is TabsLayoutInfo)
					{
						TabsLayoutInfo tabsLayoutInfo = layoutedWidget.Widget.LayoutInfo as TabsLayoutInfo;
						num = ((layoutedWidget.Bounds.Width != 0f || tabsLayoutInfo.TabWidth == layoutedWidget.Bounds.Width) ? (num - layoutedWidget.Bounds.Width) : (num - tabsLayoutInfo.TabWidth));
					}
					else
					{
						num -= layoutedWidget.Bounds.Width;
					}
					UpdateBounds(layoutedWidget, num);
				}
			}
			return;
		}
		for (int i = 0; i < reorderedWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget2 = reorderedWidgets[i];
			if (!(layoutedWidget2.Widget is ParagraphItem) || !(layoutedWidget2.Widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false))
			{
				UpdateBounds(layoutedWidget2, num);
				if (layoutedWidget2.Widget.LayoutInfo is TabsLayoutInfo)
				{
					TabsLayoutInfo tabsLayoutInfo2 = layoutedWidget2.Widget.LayoutInfo as TabsLayoutInfo;
					num = ((layoutedWidget2.Bounds.Width != 0f || tabsLayoutInfo2.TabWidth == layoutedWidget2.Bounds.Width) ? (num + layoutedWidget2.Bounds.Width) : (num + tabsLayoutInfo2.TabWidth));
				}
				else
				{
					num += layoutedWidget2.Bounds.Width;
				}
			}
		}
	}

	private void UpdateBounds(LayoutedWidget childltWidget, float lineX)
	{
		if (childltWidget.Widget is WMath)
		{
			float num = lineX - childltWidget.Bounds.X;
			childltWidget.ShiftLocation(num, 0.0, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false, isNeedToShiftOwnerWidget: true);
		}
		else
		{
			childltWidget.Bounds = new RectangleF(new PointF(lineX, childltWidget.Bounds.Y), childltWidget.Bounds.Size);
		}
	}

	private float GetFirstNonFloatingItemX()
	{
		for (int i = 0; i < m_currChildLW.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = m_currChildLW.ChildWidgets[i];
			if (!(layoutedWidget.Widget is ParagraphItem) || !(layoutedWidget.Widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false))
			{
				return layoutedWidget.Bounds.X;
			}
		}
		return -1f;
	}

	private LayoutedWidgetList ReorderWidgets(List<CharacterRangeType> characterRangeTypes, List<bool> splittedWidgetBidiValues, bool paraBidi)
	{
		int num = 0;
		int num2 = -1;
		int num3 = 0;
		int num4 = 0;
		LayoutedWidgetList layoutedWidgetList = new LayoutedWidgetList();
		CharacterRangeType characterRangeType = CharacterRangeType.LTR;
		bool flag = false;
		for (int i = 0; i < m_currChildLW.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = m_currChildLW.ChildWidgets[i];
			layoutedWidget.CharacterRange = characterRangeTypes[i];
			bool flag2 = (layoutedWidget.CharacterRange & CharacterRangeType.RTL) == CharacterRangeType.RTL || layoutedWidget.CharacterRange == CharacterRangeType.Number;
			bool flag3 = splittedWidgetBidiValues[i];
			if (characterRangeTypes[i] == CharacterRangeType.Tab)
			{
				if (paraBidi)
				{
					num = 0;
					num2 = -1;
					num3 = 0;
					characterRangeType = CharacterRangeType.LTR;
					flag = false;
					layoutedWidgetList.Insert(num, layoutedWidget);
					continue;
				}
				if (flag3)
				{
					flag3 = false;
				}
			}
			if (i > 0 && flag != flag3)
			{
				if (paraBidi)
				{
					num = 0;
					num2 = -1;
					num3 = 0;
				}
				else
				{
					num2 = layoutedWidgetList.Count - 1;
				}
				num4 = 0;
			}
			if (!flag3 && !flag2)
			{
				if (paraBidi)
				{
					if (num3 > 0 && flag == flag3)
					{
						num += num3;
					}
					layoutedWidgetList.Insert(num, layoutedWidget);
					num++;
				}
				else
				{
					layoutedWidgetList.Add(layoutedWidget);
					num = i + 1;
				}
				num3 = 0;
				num2 = (paraBidi ? (num - 1) : (layoutedWidgetList.Count - 1));
			}
			else if (flag2 || (flag3 && layoutedWidget.CharacterRange == CharacterRangeType.WordSplit && (characterRangeType == CharacterRangeType.RTL || IsInsertWordSplitToLeft(characterRangeTypes, splittedWidgetBidiValues, i))))
			{
				num3++;
				num = num2 + 1;
				int num5 = num;
				if (layoutedWidget.CharacterRange == CharacterRangeType.Number)
				{
					if (characterRangeType == CharacterRangeType.Number)
					{
						num += num4;
					}
					num4++;
				}
				layoutedWidgetList.Insert(num, layoutedWidget);
				num = num5;
			}
			else
			{
				layoutedWidgetList.Insert(num, layoutedWidget);
				num++;
				num3 = 0;
			}
			if (layoutedWidget.CharacterRange != CharacterRangeType.Number)
			{
				num4 = 0;
			}
			if (layoutedWidget.CharacterRange != CharacterRangeType.WordSplit)
			{
				characterRangeType = layoutedWidget.CharacterRange;
			}
			flag = flag3;
		}
		return layoutedWidgetList;
	}

	private bool IsNumberNonReversingCharacter(LayoutedWidget childltWidget)
	{
		if ((childltWidget.Widget is WTextRange || childltWidget.Widget is SplitStringWidget) && childltWidget.Bounds.Height > 0f)
		{
			WTextRange wTextRange = ((childltWidget.Widget is SplitStringWidget) ? ((childltWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange) : (childltWidget.Widget as WTextRange));
			if (wTextRange.Text.Length == 1)
			{
				if (!wTextRange.CharacterFormat.HasValueWithParent(75))
				{
					return TextSplitter.IsNumberNonReversingCharacter(wTextRange.Text, wTextRange.CharacterFormat.Bidi);
				}
				char c = wTextRange.Text[0];
				if ((c == '/' && !IsNumberReverseLangForSlash(wTextRange.CharacterFormat.LocaleIdBidi)) || ((c == '#' || c == '$' || c == '%' || c == '+' || c == '-') && !IsNumberReverseLangForOthers(wTextRange.CharacterFormat.LocaleIdBidi)) || c == ',' || c == '.' || c == ':' || c == '،')
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsNumberReverseLangForSlash(short id)
	{
		if (!IsNumberReverseLangForOthers(id) && id != 6145 && id != 1164 && id != 1125 && id != 1120 && id != 1123 && id != 1065 && id != 2137 && id != 1114 && id != 1119 && id != 1152)
		{
			return id == 1056;
		}
		return true;
	}

	private bool IsNumberReverseLangForOthers(short id)
	{
		if (id != 14337 && id != 15361 && id != 5121 && id != 3073 && id != 2049 && id != 11265 && id != 13313 && id != 12289 && id != 4097 && id != 8193 && id != 16385 && id != 1025 && id != 10241 && id != 7169)
		{
			return id == 9217;
		}
		return true;
	}

	private bool IsInsertWordSplitToLeft(List<CharacterRangeType> characterRangeTypes, List<bool> splittedWidgetBidiValues, int widgetIndex)
	{
		for (int i = widgetIndex + 1; i < characterRangeTypes.Count; i++)
		{
			if ((characterRangeTypes[i] & CharacterRangeType.RTL) == CharacterRangeType.RTL)
			{
				return true;
			}
			if (characterRangeTypes[i] == CharacterRangeType.LTR)
			{
				if (splittedWidgetBidiValues[i])
				{
					return false;
				}
				return true;
			}
		}
		return true;
	}

	private float GetListWidthToShiftLine()
	{
		float result = 0f;
		if (m_currChildLW.Widget is WParagraph || (m_currChildLW.Widget is SplitWidgetContainer && (m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph))
		{
			ParagraphLayoutInfo paragraphLayoutInfo = ((m_currChildLW.Widget is WParagraph) ? ((m_currChildLW.Widget as WParagraph).m_layoutInfo as ParagraphLayoutInfo) : (((m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).m_layoutInfo as ParagraphLayoutInfo));
			if (paragraphLayoutInfo.ListValue != string.Empty)
			{
				result = paragraphLayoutInfo.Margins.Left;
			}
		}
		return result;
	}

	private void ShiftLineForListWidth()
	{
		LayoutedWidget layoutedWidget = null;
		ParagraphLayoutInfo paragraphLayoutInfo = m_currChildLW.Widget.LayoutInfo as ParagraphLayoutInfo;
		float listWidthToShiftLine = GetListWidthToShiftLine();
		float num = ((listWidthToShiftLine == 0f) ? paragraphLayoutInfo.Margins.Left : listWidthToShiftLine);
		for (int i = 0; i < m_currChildLW.ChildWidgets.Count; i++)
		{
			layoutedWidget = m_currChildLW.ChildWidgets[i];
			if (layoutedWidget.Widget is WTextRange || layoutedWidget.Widget is SplitStringWidget || !(layoutedWidget.Widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false))
			{
				float x = layoutedWidget.Bounds.X - num;
				layoutedWidget.Bounds = new RectangleF(new PointF(x, layoutedWidget.Bounds.Y), layoutedWidget.Bounds.Size);
			}
		}
	}

	private void ShiftRTLAndNormalText(int lastRtlTextIndex, int engTextIndex, bool isNormalText)
	{
		LayoutedWidget layoutedWidget = null;
		float num = m_currChildLW.Bounds.Width;
		float x = m_currChildLW.Bounds.X;
		float listWidthToShiftLine = GetListWidthToShiftLine();
		float num2 = 0f;
		if (lastRtlTextIndex == -1)
		{
			lastRtlTextIndex = m_currChildLW.ChildWidgets.Count - 1;
		}
		ParagraphLayoutInfo paragraphLayoutInfo = m_currChildLW.Widget.LayoutInfo as ParagraphLayoutInfo;
		for (int i = 0; i <= lastRtlTextIndex; i++)
		{
			layoutedWidget = m_currChildLW.ChildWidgets[i];
			if (layoutedWidget.Widget.LayoutInfo is TabsLayoutInfo)
			{
				float num3 = ((i == 0) ? (num - layoutedWidget.Bounds.Width) : (m_currChildLW.ChildWidgets[i - 1].Bounds.X - layoutedWidget.Bounds.Width));
				layoutedWidget.Bounds = new RectangleF(new PointF(num3, layoutedWidget.Bounds.Y), layoutedWidget.Bounds.Size);
				num = ((listWidthToShiftLine == 0f) ? (num3 - x + paragraphLayoutInfo.Margins.Left) : (num3 - x + listWidthToShiftLine));
			}
			else if (layoutedWidget.Widget is WTextRange || layoutedWidget.Widget is SplitStringWidget || !(layoutedWidget.Widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false))
			{
				float num4 = num - layoutedWidget.Bounds.Width;
				float num5 = ((listWidthToShiftLine == 0f) ? paragraphLayoutInfo.Margins.Left : listWidthToShiftLine);
				if (layoutedWidget.Bounds.Width == 0f)
				{
					layoutedWidget.Bounds = new RectangleF(new PointF(num4 + x, layoutedWidget.Bounds.Y), layoutedWidget.Bounds.Size);
				}
				else
				{
					layoutedWidget.Bounds = new RectangleF(new PointF(num4 + x - num5, layoutedWidget.Bounds.Y), layoutedWidget.Bounds.Size);
				}
				num = num4;
			}
			num2 += layoutedWidget.Bounds.Width;
		}
		num2 += ((listWidthToShiftLine == 0f) ? paragraphLayoutInfo.Margins.Left : listWidthToShiftLine);
		for (int j = lastRtlTextIndex + 1; j < m_currChildLW.ChildWidgets.Count; j++)
		{
			layoutedWidget = m_currChildLW.ChildWidgets[j];
			if (layoutedWidget.Widget.LayoutInfo is TabsLayoutInfo)
			{
				float num6 = m_currChildLW.ChildWidgets[j - 1].Bounds.X - layoutedWidget.Bounds.Width;
				layoutedWidget.Bounds = new RectangleF(new PointF(num6, layoutedWidget.Bounds.Y), layoutedWidget.Bounds.Size);
				num = num6;
				num2 += layoutedWidget.Bounds.Width;
			}
			else if (layoutedWidget.Widget is WTextRange || layoutedWidget.Widget is SplitStringWidget || !(layoutedWidget.Widget as ParagraphItem).IsFloatingItem(isTextWrapAround: false))
			{
				layoutedWidget.Bounds = new RectangleF(new PointF(layoutedWidget.Bounds.X - num2, layoutedWidget.Bounds.Y), layoutedWidget.Bounds.Size);
			}
		}
		if (isNormalText)
		{
			ShiftNormalText(engTextIndex);
		}
	}

	private void ShiftNormalText(int engTextIndex)
	{
		LayoutedWidget layoutedWidget = null;
		int num = engTextIndex;
		int i = num;
		int num2 = -1;
		WTextRange wTextRange = null;
		for (; i < m_currChildLW.ChildWidgets.Count; i++)
		{
			layoutedWidget = m_currChildLW.ChildWidgets[i];
			if (!(layoutedWidget.Widget is WTextRange) && !(layoutedWidget.Widget is SplitStringWidget))
			{
				continue;
			}
			wTextRange = ((layoutedWidget.Widget is SplitStringWidget) ? ((layoutedWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange) : (layoutedWidget.Widget as WTextRange));
			if (IsRTLText(wTextRange.Text) || wTextRange.CharacterFormat.Bidi || wTextRange.CharacterFormat.BiDirectionalOverride == BiDirectionalOverride.RTL)
			{
				if (num != -1 && (num2 != -1 || (!(m_currChildLW.ChildWidgets[num].Widget is WTextRange) && !(m_currChildLW.ChildWidgets[num].Widget is SplitStringWidget)) || !((m_currChildLW.ChildWidgets[num].Widget as WTextRange).Text == " ")))
				{
					if (num2 == -1)
					{
						num2 = num;
					}
					float right = m_currChildLW.ChildWidgets[i].Bounds.Right;
					for (int j = num; j <= num2; j++)
					{
						m_currChildLW.ChildWidgets[j].Bounds = new RectangleF(new PointF(right, m_currChildLW.ChildWidgets[j].Bounds.Y), m_currChildLW.ChildWidgets[j].Bounds.Size);
						right = m_currChildLW.ChildWidgets[j].Bounds.Right;
					}
					i = num2 + 1;
					num = -1;
				}
			}
			else
			{
				if (num == -1)
				{
					num = i;
				}
				num2 = i;
			}
		}
	}

	private void ShiftRTLText(int rtlTextIndex, bool isBidi, bool isNormalText)
	{
		LayoutedWidget layoutedWidget = null;
		int num = rtlTextIndex;
		int i = num;
		int num2 = -1;
		WTextRange wTextRange = null;
		if (!isBidi)
		{
			return;
		}
		for (; i < m_currChildLW.ChildWidgets.Count; i++)
		{
			layoutedWidget = m_currChildLW.ChildWidgets[i];
			if ((layoutedWidget.Widget is WTextRange || layoutedWidget.Widget is SplitStringWidget) && !(layoutedWidget.Widget.LayoutInfo is TabsLayoutInfo))
			{
				wTextRange = ((layoutedWidget.Widget is SplitStringWidget) ? ((layoutedWidget.Widget as SplitStringWidget).RealStringWidget as WTextRange) : (layoutedWidget.Widget as WTextRange));
				if (IsRTLText(wTextRange.Text) || wTextRange.CharacterFormat.Bidi || wTextRange.CharacterFormat.BiDirectionalOverride == BiDirectionalOverride.RTL)
				{
					if (num == -1)
					{
						num = i;
					}
					num2 = i;
					if (i == m_currChildLW.ChildWidgets.Count - 1)
					{
						ShiftWidgets(num, num2);
						num = -1;
						num2 = -1;
						break;
					}
					continue;
				}
				if (num == -1)
				{
					continue;
				}
				if (num2 == -1)
				{
					bool num3;
					if (!(m_currChildLW.ChildWidgets[num].Widget is WTextRange))
					{
						if (!(m_currChildLW.ChildWidgets[num].Widget is SplitStringWidget))
						{
							goto IL_0195;
						}
						num3 = ((m_currChildLW.ChildWidgets[num].Widget as SplitStringWidget).RealStringWidget as WTextRange).Text == " ";
					}
					else
					{
						num3 = (m_currChildLW.ChildWidgets[num].Widget as WTextRange).Text == " ";
					}
					if (num3)
					{
						continue;
					}
				}
				goto IL_0195;
			}
			if (layoutedWidget.Widget.LayoutInfo is TabsLayoutInfo && num != -1 && num2 != -1)
			{
				ShiftWidgets(num, num2);
				num = i + 1;
			}
			continue;
			IL_0195:
			if (num2 == -1)
			{
				num2 = num;
			}
			ShiftWidgets(num, num2);
			i = num2 + 1;
			num = -1;
		}
		if (num != -1 && num2 != -1)
		{
			ShiftWidgets(num, num2);
		}
	}

	private void ShiftWidgets(int startIndex, int endIndex)
	{
		float num = m_currChildLW.ChildWidgets[endIndex].Bounds.Right;
		for (int i = startIndex; i <= endIndex; i++)
		{
			m_currChildLW.ChildWidgets[i].Bounds = new RectangleF(new PointF(num - m_currChildLW.ChildWidgets[i].Bounds.Width, m_currChildLW.ChildWidgets[i].Bounds.Y), m_currChildLW.ChildWidgets[i].Bounds.Size);
			num -= m_currChildLW.ChildWidgets[i].Bounds.Width;
		}
	}

	private float GetLastTabWidth(WParagraphFormat paraFormat, int tabsCount)
	{
		float result = 0f;
		if (paraFormat.Tabs[tabsCount - 1].Justification != 0)
		{
			for (int num = m_currChildLW.ChildWidgets.Count - 1; num >= 0; num--)
			{
				if (m_currChildLW.ChildWidgets[num].Widget.LayoutInfo is TabsLayoutInfo)
				{
					result = m_currChildLW.ChildWidgets[num].Bounds.Width;
					break;
				}
			}
		}
		return result;
	}

	private void SplitLineBasedOnInterSectingFlotingEntity(LayoutedWidget m_backupWidget, ref RectangleF interSectingFloattingItem, LayoutedWidget m_resulttedWidgt)
	{
		RectangleF bounds = m_backupWidget.Bounds;
		int num = GetFirstInlineItemIndex(m_backupWidget);
		if (num == int.MinValue)
		{
			return;
		}
		bounds.X = m_backupWidget.ChildWidgets[num].Bounds.X;
		bounds.Width = m_backupWidget.ChildWidgets[m_backupWidget.ChildWidgets.Count - 1].Bounds.Right - m_backupWidget.ChildWidgets[num].Bounds.X;
		bounds.Height = m_backupWidget.ChildWidgets[num].Bounds.Height;
		Entity entity = null;
		while (++num < m_backupWidget.ChildWidgets.Count && (!(m_backupWidget.ChildWidgets[num].Widget is Entity entity2) || !entity2.IsFloatingItem(isTextWrapAround: true)))
		{
			if (bounds.Height < m_backupWidget.ChildWidgets[num].Bounds.Height)
			{
				bounds.Height = m_backupWidget.ChildWidgets[num].Bounds.Height;
			}
		}
		entity = m_currChildLW.Widget as Entity;
		if (entity == null && m_currChildLW.Widget is SplitWidgetContainer)
		{
			entity = (m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer as Entity;
		}
		if ((m_lcOperator as Layouter).MaxRightPositionOfTabStopInterSectingFloattingItems == float.MinValue && (!(entity is WParagraph) || (!(entity as WParagraph).IsInCell && !(entity as WParagraph).ParagraphFormat.IsFrame)) && GetBaseEntity(entity) is WSection && m_backupWidget.ChildWidgets.Count > 1 && (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter || entity.Owner is WTableCell || entity.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013))
		{
			interSectingFloattingItem = InterSectingFloattingItem(bounds);
		}
		else
		{
			interSectingFloattingItem = new RectangleF(0f, 0f, 0f, 0f);
		}
		if (interSectingFloattingItem.Bottom != 0f)
		{
			m_currChildLW.ChildWidgets.RemoveRange(0, m_currChildLW.ChildWidgets.Count);
			for (int i = 0; i < m_backupWidget.ChildWidgets.Count; i++)
			{
				if ((!(m_backupWidget.ChildWidgets[i].Widget is Entity) || !(m_backupWidget.ChildWidgets[i].Widget as Entity).IsFloatingItem(isTextWrapAround: true)) && m_backupWidget.ChildWidgets[i].Bounds.X > interSectingFloattingItem.X)
				{
					bounds = m_currChildLW.Bounds;
					bounds.X = m_backupWidget.Bounds.X;
					bounds.Width = m_backupWidget.ChildWidgets[i - 1].Bounds.Right - bounds.X;
					m_currChildLW.Bounds = bounds;
					m_backupWidget.ChildWidgets.RemoveRange(0, i);
					bounds = m_currChildLW.Bounds;
					bounds.X = m_backupWidget.ChildWidgets[0].Bounds.X;
					bounds.Width = m_backupWidget.Bounds.Right - bounds.X;
					m_backupWidget.Bounds = bounds;
					break;
				}
				m_currChildLW.ChildWidgets.Add(m_backupWidget.ChildWidgets[i]);
				if (i == m_backupWidget.ChildWidgets.Count - 1 && m_backupWidget.ChildWidgets.Count > 1)
				{
					bounds = m_currChildLW.Bounds;
					bounds.X = m_backupWidget.Bounds.X;
					bounds.Width = m_backupWidget.ChildWidgets[i - 1].Bounds.Right - bounds.X;
					m_currChildLW.Bounds = bounds;
					m_backupWidget.ChildWidgets.RemoveRange(0, m_backupWidget.ChildWidgets.Count);
				}
				else if (i == m_backupWidget.ChildWidgets.Count - 1 && m_backupWidget.ChildWidgets.Count == 1)
				{
					m_backupWidget.ChildWidgets.RemoveRange(0, m_backupWidget.ChildWidgets.Count);
				}
			}
		}
		else if (m_resulttedWidgt.ChildWidgets.Count > 0)
		{
			m_currChildLW.ChildWidgets.RemoveRange(0, m_currChildLW.ChildWidgets.Count);
			m_currChildLW = m_backupWidget;
		}
	}

	private int GetFirstInlineItemIndex(LayoutedWidget ltWidget)
	{
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			if (!(ltWidget.ChildWidgets[i].Widget is Entity) || !(ltWidget.ChildWidgets[i].Widget as Entity).IsFloatingItem(isTextWrapAround: true))
			{
				return i;
			}
		}
		return int.MinValue;
	}

	private RectangleF InterSectingFloattingItem(RectangleF rect)
	{
		if (rect.Width < 0f)
		{
			return new RectangleF(0f, 0f, 0f, 0f);
		}
		for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
		{
			RectangleF rectangleF = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds;
			TextWrappingStyle textWrappingStyle = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingStyle;
			if ((textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && (m_lcOperator as Layouter).FloatingItems[i].IsDoesNotDenotesRectangle)
			{
				rectangleF = AdjustTightAndThroughBounds((m_lcOperator as Layouter).FloatingItems[i], rect, rect.Height);
				if (rectangleF == (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds)
				{
					continue;
				}
			}
			else if (textWrappingStyle == TextWrappingStyle.Behind || textWrappingStyle == TextWrappingStyle.InFrontOfText || rectangleF.Right > (m_lcOperator as Layouter).ClientLayoutArea.Right)
			{
				continue;
			}
			if (rect.X <= rectangleF.X && rect.IntersectsWith(rectangleF))
			{
				return rectangleF;
			}
		}
		return new RectangleF(0f, 0f, 0f, 0f);
	}

	private void UpdateSubWidthBasedOnTextWrap(WParagraph paragraph, ref double subWidth, float xPosition, float rightMargin)
	{
		if (paragraph != null && paragraph.IsInCell)
		{
			Entity baseEntity = GetBaseEntity(paragraph);
			if (baseEntity is WTextBox || baseEntity is Shape || baseEntity is GroupShape)
			{
				return;
			}
		}
		if (paragraph == null || (m_lcOperator as Layouter).FloatingItems.Count <= 0 || m_currChildLW.ChildWidgets.Count <= 0 || ((m_lcOperator as Layouter).IsLayoutingHeaderFooter && !paragraph.IsInCell && paragraph.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013) || IsInFootnote(paragraph) || IsInTextBox(paragraph) != null)
		{
			return;
		}
		float num = 0f;
		Entity baseEntity2 = GetBaseEntity(paragraph);
		for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
		{
			RectangleF textWrappingBounds = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds;
			TextWrappingStyle textWrappingStyle = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingStyle;
			if (baseEntity2 != (m_lcOperator as Layouter).FloatingItems[i].FloatingEntity && xPosition < textWrappingBounds.X && m_layoutArea.ClientActiveArea.Right > textWrappingBounds.X && textWrappingBounds.X > m_currChildLW.Bounds.X && m_currChildLW.Bounds.Bottom > textWrappingBounds.Y && Math.Round(m_currChildLW.Bounds.Y, 2) < Math.Round(textWrappingBounds.Bottom, 2) && textWrappingStyle != 0 && textWrappingStyle != TextWrappingStyle.TopAndBottom && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind && (!paragraph.IsInCell || paragraph.GetOwnerCellEntity() == (m_lcOperator as Layouter).FloatingItems[i].FloatingEntity.GetOwnerCellEntity()))
			{
				float num2 = m_layoutArea.ClientActiveArea.Right - textWrappingBounds.X - rightMargin;
				if (Math.Round(num, 2) < Math.Round(num2, 2))
				{
					subWidth -= num2 - num;
					num = num2;
				}
			}
		}
	}

	protected override void UpdateHorizontalAlignment(short xAlignment)
	{
		RectangleF bounds = m_currChildLW.Bounds;
		switch (xAlignment)
		{
		case -4:
			m_currChildLW.ShiftLocation((0f - bounds.Width) / 2f, 0.0, isPictureNeedToBeShifted: false, isFromFloatingItemVerticalAlignment: false);
			break;
		case -16:
		case -8:
			m_currChildLW.ShiftLocation(0f - bounds.Width, 0.0, isPictureNeedToBeShifted: false, isFromFloatingItemVerticalAlignment: false);
			break;
		}
	}

	internal bool IsTOC(WParagraph para)
	{
		if (para != null && para.ChildEntities.FirstItem != null && (para.ChildEntities.FirstItem is TableOfContent || (para.ChildEntities.FirstItem is WField && (para.ChildEntities.FirstItem as WField).FieldType == FieldType.FieldHyperlink && new Hyperlink(para.ChildEntities.FirstItem as WField).BookmarkName != null && new Hyperlink(para.ChildEntities.FirstItem as WField).BookmarkName.StartsWith("_Toc"))))
		{
			return true;
		}
		return false;
	}

	protected override void DoLayoutAfter()
	{
		WParagraph wParagraph = null;
		if (m_currChildLW.Widget is WParagraph)
		{
			wParagraph = m_currChildLW.Widget as WParagraph;
		}
		else if (m_currChildLW.Widget is SplitWidgetContainer)
		{
			SplitWidgetContainer splitWidgetContainer = m_currChildLW.Widget as SplitWidgetContainer;
			if (splitWidgetContainer.RealWidgetContainer is WParagraph)
			{
				wParagraph = splitWidgetContainer.RealWidgetContainer as WParagraph;
				if (IsTOCNeedNotToBeUpdated(wParagraph, splitWidgetContainer))
				{
					return;
				}
			}
		}
		if (DocumentLayouter.IsUpdatingTOC && !(m_lcOperator as Layouter).IsLayoutingHeaderRow && (m_lcOperator as Layouter).LayoutingTOC == null && !SkipUpdatingPageNumber(wParagraph))
		{
			UpdateTOCPageNumber(wParagraph);
		}
		if (DocumentLayouter.IsUpdatingTOC)
		{
			_ = DocumentLayouter.IsEndUpdateTOC;
		}
	}

	private bool SkipUpdatingPageNumber(WParagraph paragraph)
	{
		if (m_ltState == LayoutState.NotFitted && m_ltWidget.ChildWidgets.Count == 1 && IsLineContainOnlyNonRenderableItem(m_ltWidget.ChildWidgets[0]))
		{
			return true;
		}
		LayoutedWidget layoutedWidget = m_ltWidget;
		while (layoutedWidget.ChildWidgets.Count > 0)
		{
			layoutedWidget = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
		}
		if (layoutedWidget.Widget is Break && (layoutedWidget.Widget as Break).BreakType == BreakType.PageBreak)
		{
			int index = (layoutedWidget.Widget as Break).Index;
			if (IsParagraphContainsInvalidItemsOnly(paragraph, index))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsParagraphContainsInvalidItemsOnly(WParagraph para, int pageBreakIndex)
	{
		for (int i = 0; i < pageBreakIndex && i < para.ChildEntities.Count; i++)
		{
			ParagraphItem paragraphItem = para.ChildEntities[i] as ParagraphItem;
			if (!(paragraphItem is BookmarkStart) && !(paragraphItem is BookmarkEnd) && !(paragraphItem is WFieldMark) && (!(paragraphItem is WTextRange) || paragraphItem is WField || !IsNullOrWhiteSpace((paragraphItem as WTextRange).Text)))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsNullOrWhiteSpace(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return true;
		}
		for (int i = 0; i < text.Length; i++)
		{
			if (!char.IsWhiteSpace(text[i]))
			{
				return false;
			}
		}
		return true;
	}

	private bool IsTOCNeedNotToBeUpdated(WParagraph paragraph, SplitWidgetContainer swc)
	{
		IWidget currentChild = swc.m_currentChild;
		if (currentChild is SplitWidgetContainer)
		{
			currentChild = (currentChild as SplitWidgetContainer).m_currentChild;
		}
		IEntity entity = null;
		entity = ((!(currentChild is SplitStringWidget)) ? (currentChild as IEntity) : ((currentChild as SplitStringWidget).RealStringWidget as IEntity));
		int num = ((entity is Entity) ? (entity as Entity).Index : (-1));
		if (num == -1)
		{
			return false;
		}
		if (paragraph.HasSDTInlineItem)
		{
			num = ((IWidgetContainer)paragraph).WidgetInnerCollection.IndexOf(entity);
		}
		int validParaItemIndex = GetValidParaItemIndex(paragraph, num);
		if ((m_lcOperator as Layouter).TOCEntryPageNumbers.Count > 0 && (m_lcOperator as Layouter).TOCEntryPageNumbers.ContainsKey(paragraph) && validParaItemIndex != int.MaxValue && validParaItemIndex < num)
		{
			return true;
		}
		return false;
	}

	private int GetValidParaItemIndex(WParagraph paragraph, int paraItemIndex)
	{
		for (int i = 0; i < paraItemIndex && i < paragraph.ChildEntities.Count; i++)
		{
			ParagraphItem paragraphItem = null;
			paragraphItem = ((!paragraph.HasSDTInlineItem) ? (paragraph.ChildEntities[i] as ParagraphItem) : (((IWidgetContainer)paragraph).WidgetInnerCollection[i] as ParagraphItem));
			if (paragraphItem != null && (!(paragraphItem is WTextRange) || paragraphItem is WField || !((paragraphItem as WTextRange).Text.Trim() == "")) && !(paragraphItem is BookmarkStart) && !(paragraphItem is BookmarkEnd) && !paragraphItem.IsFloatingItem(isTextWrapAround: false) && !((IWidget)paragraphItem).LayoutInfo.IsSkip && !(paragraphItem is Break))
			{
				return paragraphItem.Index;
			}
		}
		return int.MaxValue;
	}

	private void UpdateTOCPageNumber(WParagraph para)
	{
		if (para.Document.TOC.Values.Count > 0 && (!para.IsEmptyParagraph() || para.ListFormat.ListType == ListType.Numbered) && (CheckHeadingStyle(para) || CheckTableOfFiguresLabel(para)))
		{
			m_lcOperator.SendLeafLayoutAfter(m_ltWidget, isFromTOCLinkStyle: false);
			if (DocumentLayouter.IsUpdatingTOC)
			{
				_ = DocumentLayouter.IsEndUpdateTOC;
			}
		}
	}

	private bool CheckHeadingStyle(WParagraph para)
	{
		string value = para.ParagraphFormat.OutlineLevel.ToString().ToLower().Replace("level", "");
		string styleName = para.StyleName;
		styleName = ((styleName == null) ? "normal" : styleName.ToLower().Replace(" ", ""));
		foreach (TableOfContent value2 in para.Document.TOC.Values)
		{
			foreach (KeyValuePair<int, List<string>> tOCLevel in value2.TOCLevels)
			{
				foreach (string item in tOCLevel.Value)
				{
					string text = item.ToLower().Replace(" ", "");
					if (styleName.StartsWith(text) || text.Contains(value))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool CheckTableOfFiguresLabel(WParagraph para)
	{
		foreach (ParagraphItem childEntity in para.ChildEntities)
		{
			if (!(childEntity is WSeqField))
			{
				continue;
			}
			WSeqField wSeqField = (WSeqField)childEntity;
			foreach (TableOfContent value2 in para.Document.TOC.Values)
			{
				if (value2.TableOfFiguresLabel != null)
				{
					string value = value2.TableOfFiguresLabel.Replace(' ', '_').ToLower();
					string seqCaptionName = wSeqField.GetSeqCaptionName();
					if (seqCaptionName != null && seqCaptionName.ToLower().Equals(value))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
