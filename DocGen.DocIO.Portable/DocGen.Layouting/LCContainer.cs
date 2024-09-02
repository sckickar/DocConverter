using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Rendering;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LCContainer : LayoutContext
{
	protected int m_curWidgetIndex;

	protected LayoutedWidget m_currChildLW;

	protected bool m_bAtLastOneChildFitted;

	protected IWidgetContainer WidgetContainer => m_widget as IWidgetContainer;

	protected IWidget CurrentChildWidget
	{
		get
		{
			if (m_curWidgetIndex <= -1 || m_curWidgetIndex >= WidgetContainer.Count)
			{
				return null;
			}
			return WidgetContainer[m_curWidgetIndex];
		}
	}

	public LCContainer(IWidgetContainer widget, ILCOperator lcOperator, bool isForceFitLayout)
		: base(widget, lcOperator, isForceFitLayout)
	{
	}

	public override LayoutedWidget Layout(RectangleF rect)
	{
		CreateLayoutArea(rect);
		CreateLayoutedWidget(rect.Location);
		bool isInnerLayouting = base.IsInnerLayouting;
		LayoutContext layoutContext;
		do
		{
			layoutContext = CreateNextChildContext();
			if (m_currChildLW != null && isInnerLayouting && m_currChildLW.TextTag == "Splitted")
			{
				layoutContext = null;
			}
			if (layoutContext == null)
			{
				if (m_bAtLastOneChildFitted)
				{
					m_ltState = LayoutState.Fitted;
					base.IsTabStopBeyondRightMarginExists = false;
				}
				break;
			}
			if (layoutContext is LCContainer && m_ltWidget.Widget is WParagraph && m_ltWidget.ChildWidgets.Count > 0 && (m_lcOperator as Layouter).UnknownField != null && (m_ltWidget.Widget as WParagraph).IsInCell)
			{
				m_bAtLastOneChildFitted = true;
				m_ltState = LayoutState.Fitted;
				base.IsTabStopBeyondRightMarginExists = false;
				break;
			}
			if (layoutContext.Widget is WParagraph)
			{
				WParagraph wParagraph = layoutContext.Widget as WParagraph;
				if (layoutContext is LCLineContainer && (m_lcOperator as Layouter).HiddenLineBottom > 0f && !((m_lcOperator as Layouter).FieldEntity is WFieldMark))
				{
					(m_lcOperator as Layouter).IsFirstItemInLine = false;
				}
				if (wParagraph.ParagraphFormat != null)
				{
					layoutContext.ClientLayoutAreaRight = base.ClientLayoutAreaRight - wParagraph.ParagraphFormat.RightIndent;
				}
			}
			else
			{
				layoutContext.ClientLayoutAreaRight = base.ClientLayoutAreaRight;
			}
			layoutContext.IsTabStopBeyondRightMarginExists = base.IsTabStopBeyondRightMarginExists;
			layoutContext.IsNeedToWrap = base.IsNeedToWrap;
			layoutContext.LayoutInfo.TextWrap = base.LayoutInfo.TextWrap;
			if (layoutContext != null && (!(layoutContext.Widget is ParagraphItem) || !IsNonRenderableItem((layoutContext.Widget as ParagraphItem).PreviousSibling as IWidget)))
			{
				(m_lcOperator as Layouter).AtLeastOneChildFitted = m_bAtLastOneChildFitted;
			}
			DoLayoutChild(layoutContext);
			if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
			{
				return null;
			}
			base.IsTabStopBeyondRightMarginExists = layoutContext.IsTabStopBeyondRightMarginExists;
			SaveChildContextState(layoutContext);
			if ((m_lcOperator as Layouter).IsNeedToRelayout && this is LCLineContainer)
			{
				AddToCollectionAndRelayout(layoutContext);
			}
			if (layoutContext is LCContainer && !(layoutContext is LCLineContainer) && m_currChildLW.IsNotFitted)
			{
				WParagraph wParagraph2 = ((layoutContext.Widget is WParagraph) ? (layoutContext.Widget as WParagraph) : ((!(layoutContext.Widget is SplitWidgetContainer)) ? null : (((layoutContext.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) ? ((layoutContext.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null)));
				if (m_currChildLW.ChildWidgets.Count > 0)
				{
					m_currChildLW.ChildWidgets.Clear();
					m_currChildLW.IsNotFitted = false;
					RectangleF bounds = new RectangleF(m_currChildLW.Bounds.X, m_currChildLW.Bounds.Y, 0f, m_currChildLW.Bounds.Height);
					m_currChildLW.Bounds = bounds;
				}
				if (wParagraph2 == null || wParagraph2.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 || !DocumentLayouter.IsEndPage || m_ltWidget.IsLastItemInPage)
				{
					SaveChildContextState(layoutContext);
				}
			}
		}
		while ((((base.State != 0 || !(layoutContext is LCTable)) && (base.State != LayoutState.DynamicRelayout || layoutContext is LCTable)) || !(m_ltWidget.Widget is WTableCell) || !(m_lcOperator as Layouter).IsNeedToRelayoutTable) && (base.State == LayoutState.Unknown || (base.State == LayoutState.DynamicRelayout && ((m_ltWidget.Widget is SplitWidgetContainer) ? ((m_ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WTableCell) : (m_ltWidget.Widget is WTableCell)))));
		DoLayoutAfter();
		if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
		{
			return null;
		}
		return m_ltWidget;
	}

	private void AddToCollectionAndRelayout(LayoutContext childContext)
	{
		for (int i = 0; i < m_currChildLW.ChildWidgets.Count; i++)
		{
			ParagraphItem paragraphItem = m_currChildLW.ChildWidgets[i].Widget as ParagraphItem;
			if (IsDrawingElement(paragraphItem as ILeafWidget) && (paragraphItem.GetHorizontalOrigin() == HorizontalOrigin.Character || paragraphItem.GetVerticalOrigin() == VerticalOrigin.Line) && !paragraphItem.IsWrappingBoundsAdded())
			{
				WPageSetup pageSetup = (m_lcOperator as Layouter).CurrentSection.PageSetup;
				RectangleF bounds = m_currChildLW.ChildWidgets[i].Bounds;
				float num = pageSetup.PageSize.Width - pageSetup.Margins.Right;
				if (bounds.Right > num && (paragraphItem.GetVerticalOrigin() == VerticalOrigin.Paragraph || paragraphItem.GetVerticalOrigin() == VerticalOrigin.Line))
				{
					bounds.X = num - bounds.Width;
				}
				m_currChildLW.ChildWidgets[i].Bounds = bounds;
				AddToFloatingItems(m_currChildLW.ChildWidgets[i], paragraphItem as ILeafWidget);
			}
		}
		SplitedUpWidget(childContext.SplittedWidget, isEndNoteSplitWidgets: false);
		m_ltState = LayoutState.DynamicRelayout;
		m_currChildLW.Owner = m_ltWidget;
		(m_lcOperator as Layouter).IsNeedToRelayout = false;
	}

	private bool IsFloatingTextBodyItem(IWidget widget)
	{
		if ((widget is WTable && ((widget as WTable).TableFormat.WrapTextAround || (widget as WTable).IsFrame)) || (widget is WParagraph && (widget as WParagraph).ParagraphFormat.IsFrame && (widget as WParagraph).ParagraphFormat.WrapFrameAround != FrameWrapMode.None))
		{
			return true;
		}
		return false;
	}

	private float GetRightPosition(float rightPosition, ref bool isNeedToUpdateXPosition)
	{
		if ((m_lcOperator as Layouter).FloatingItems.Count != 0)
		{
			for (int num = m_currChildLW.ChildWidgets.Count - 1; num >= 0; num--)
			{
				for (int num2 = m_currChildLW.ChildWidgets[num].ChildWidgets.Count - 1; num2 >= 0; num2--)
				{
					ParagraphItem paragraphItem = m_currChildLW.ChildWidgets[num].ChildWidgets[num2].Widget as ParagraphItem;
					if ((paragraphItem == null || !paragraphItem.IsFloatingItem(isTextWrapAround: false)) && paragraphItem != null)
					{
						isNeedToUpdateXPosition = true;
						return m_currChildLW.ChildWidgets[num].ChildWidgets[num2].Bounds.Right;
					}
				}
			}
		}
		else
		{
			isNeedToUpdateXPosition = true;
		}
		return rightPosition;
	}

	private bool IsSkipParaMarkItem(IWidget widget)
	{
		if (widget is WTextRange && (widget as WTextRange).Owner == null && (widget as WTextRange).Text == " " && WidgetContainer.WidgetInnerCollection.Count > 1 && WidgetContainer.WidgetInnerCollection.Owner is WParagraph)
		{
			WParagraph wParagraph = WidgetContainer.WidgetInnerCollection.Owner as WParagraph;
			if (wParagraph.IsContainFloatingItems() && IsSizeExceedsClientSize(wParagraph))
			{
				widget.LayoutInfo.IsSkip = true;
				return true;
			}
		}
		return false;
	}

	private bool IsSkipParagraphBreak(IWidget widget)
	{
		if (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter && base.IsForceFitLayout && DocumentLayouter.PageNumber != 1 && widget is WParagraph)
		{
			WParagraph wParagraph = widget as WParagraph;
			if (wParagraph.ChildEntities.Count == 1 && wParagraph.ChildEntities[0] is Break && (wParagraph.ChildEntities[0] as Break).BreakType == BreakType.PageBreak && (wParagraph.BreakCharacterFormat.Hidden || wParagraph.BreakCharacterFormat.IsDeleteRevision) && wParagraph.NextSibling != null)
			{
				widget.LayoutInfo.IsSkip = true;
				return true;
			}
		}
		return false;
	}

	private bool CheckKeepWithNextForHiddenPara(IEntity widget)
	{
		IEntity nextSibling = (widget as Entity).NextSibling;
		if (nextSibling is WParagraph && (nextSibling as WParagraph).BreakCharacterFormat.Hidden && !(nextSibling as WParagraph).m_layoutInfo.IsKeepWithNext)
		{
			return false;
		}
		return true;
	}

	private bool IsSizeExceedsClientSize(WParagraph paragraph)
	{
		bool flag = false;
		foreach (ParagraphItem item in paragraph.Items)
		{
			if (item is WFieldMark || item is BookmarkStart || item is BookmarkEnd || !item.IsFloatingItem(isTextWrapAround: true))
			{
				continue;
			}
			SizeF sizeF = (item as ILeafWidget).Measure(base.DrawingContext);
			if (item is WPicture)
			{
				WPicture wPicture = item as WPicture;
				if (wPicture.Rotation != 0f && (wPicture.TextWrappingStyle == TextWrappingStyle.Behind || wPicture.TextWrappingStyle == TextWrappingStyle.InFrontOfText || wPicture.TextWrappingStyle == TextWrappingStyle.Square || wPicture.TextWrappingStyle == TextWrappingStyle.TopAndBottom))
				{
					int num = 0;
					int num2 = 0;
					sizeF = base.DrawingContext.GetBoundingBoxCoordinates(new RectangleF(num, num2, sizeF.Width, sizeF.Height), wPicture.Rotation).Size;
				}
			}
			if (sizeF.Height > (m_lcOperator as Layouter).ClientLayoutArea.Height)
			{
				flag = IsWord2013(paragraph.Document) || sizeF.Width > (m_lcOperator as Layouter).ClientLayoutArea.Width;
			}
			if (flag)
			{
				break;
			}
		}
		return flag;
	}

	private IEntity GetPreviousParagraphDeletedItem(Entity childWidget)
	{
		if (childWidget.Index == 0)
		{
			WParagraph ownerParagraphValue = (childWidget as ParagraphItem).GetOwnerParagraphValue();
			if (ownerParagraphValue == null)
			{
				return null;
			}
			WParagraph previousParagraph = GetPreviousParagraph(ownerParagraphValue);
			if (previousParagraph != null && previousParagraph.BreakCharacterFormat.IsDeleteRevision && previousParagraph.ChildEntities.Count > 0)
			{
				return previousParagraph.LastItem;
			}
			return null;
		}
		if (!(childWidget is ParagraphItem))
		{
			if (!(childWidget is Break))
			{
				return null;
			}
			return (childWidget as Break).PreviousSibling;
		}
		return (childWidget as ParagraphItem).PreviousSibling;
	}

	protected virtual LayoutContext CreateNextChildContext()
	{
		while (true)
		{
			LayoutedWidget layoutedWidget = CheckNullConditionAndReturnltwidget();
			int[] interSectingPoint = (m_lcOperator as Layouter).m_interSectingPoint;
			if (interSectingPoint[2] != int.MinValue && layoutedWidget != null)
			{
				IWidget widget;
				if (!(CurrentChildWidget is SplitWidgetContainer))
				{
					if (!(CurrentChildWidget is SplitTableWidget))
					{
						widget = CurrentChildWidget;
					}
					else
					{
						IWidget tableWidget = (CurrentChildWidget as SplitTableWidget).TableWidget;
						widget = tableWidget;
					}
				}
				else
				{
					IWidget tableWidget = (CurrentChildWidget as SplitWidgetContainer).RealWidgetContainer;
					widget = tableWidget;
				}
				IWidget widget2 = widget;
				if (widget2 != null && widget2 is WSection)
				{
					AddLayoutWidgetInBeforeInsectingPoint(layoutedWidget, interSectingPoint[1]);
					m_curWidgetIndex = interSectingPoint[1];
				}
				else if ((widget2 is WTable || widget2 is WParagraph || widget2 is BlockContentControl) && (widget2 as Entity).Owner is WTextBody && ((widget2 as Entity).Owner as WTextBody).Owner is BlockContentControl && layoutedWidget.ChildWidgets.Count > interSectingPoint[2] && (layoutedWidget = layoutedWidget.ChildWidgets[interSectingPoint[2]]) != null && layoutedWidget.ChildWidgets.Count > interSectingPoint[3])
				{
					AddLayoutWidgetInBeforeInsectingPoint(layoutedWidget.ChildWidgets[interSectingPoint[3]], interSectingPoint[3]);
					m_curWidgetIndex = interSectingPoint[3];
					(m_lcOperator as Layouter).m_interSectingPoint[0] = int.MinValue;
					(m_lcOperator as Layouter).m_interSectingPoint[1] = int.MinValue;
					(m_lcOperator as Layouter).m_interSectingPoint[2] = int.MinValue;
					(m_lcOperator as Layouter).m_interSectingPoint[3] = int.MinValue;
				}
				else if (layoutedWidget.ChildWidgets.Count > interSectingPoint[2] && (widget2 is WTable || widget2 is WParagraph))
				{
					if (IsFloatingTextBodyItem(layoutedWidget.ChildWidgets[interSectingPoint[2]].Widget))
					{
						(m_lcOperator as Layouter).m_interSectingPoint[0] = int.MinValue;
					}
					else
					{
						AddLayoutWidgetInBeforeInsectingPoint(layoutedWidget.ChildWidgets[interSectingPoint[2]], interSectingPoint[2]);
						m_curWidgetIndex = interSectingPoint[2];
					}
					if (CurrentChildWidget == null || CurrentChildWidget is WTable || CurrentChildWidget is SplitTableWidget || CurrentChildWidget.LayoutInfo.IsSkip)
					{
						(m_lcOperator as Layouter).m_interSectingPoint[0] = int.MinValue;
						(m_lcOperator as Layouter).m_interSectingPoint[1] = int.MinValue;
						(m_lcOperator as Layouter).m_interSectingPoint[2] = int.MinValue;
						(m_lcOperator as Layouter).m_interSectingPoint[3] = int.MinValue;
					}
				}
			}
			if (base.State == LayoutState.DynamicRelayout)
			{
				m_ltState = LayoutState.Unknown;
				(m_lcOperator as Layouter).MaintainltWidget = new LayoutedWidget(m_ltWidget);
				FindIntersectPointAndRemovltWidgetForTable();
				layoutedWidget = CheckNullConditionAndReturnltwidget();
				interSectingPoint = (m_lcOperator as Layouter).m_interSectingPoint;
				if (interSectingPoint[0] != int.MinValue && layoutedWidget != null)
				{
					LayoutedWidget interSectWidget = layoutedWidget;
					AddLayoutWidgetInBeforeInsectingPoint(interSectWidget, interSectingPoint[1]);
					RemoveLineLayoutedWidgetFromTable();
					m_curWidgetIndex = interSectingPoint[0];
					if ((m_lcOperator as Layouter).DynamicTable != null)
					{
						(m_lcOperator as Layouter).m_interSectingPoint[0] = int.MinValue;
					}
				}
			}
			IWidget currentChildWidget = CurrentChildWidget;
			if (currentChildWidget is WField && (currentChildWidget as WField).FieldSeparator != null && (currentChildWidget as WField).FieldEnd != null && (currentChildWidget as WField).FieldSeparator.OwnerParagraph != (currentChildWidget as WField).OwnerParagraph && IsInSameTextBody(currentChildWidget as WField))
			{
				(m_lcOperator as Layouter).FieldEntity = currentChildWidget as WField;
			}
			if (currentChildWidget is WAbsoluteTab && DocumentLayouter.IsLayoutingHeaderFooter && (currentChildWidget as WAbsoluteTab).m_layoutInfo is TabsLayoutInfo)
			{
				UpdateAboluteTabPosition(currentChildWidget as WAbsoluteTab);
			}
			if (!(m_lcOperator as Layouter).IsLayoutingTrackChanges)
			{
				if (currentChildWidget is WCommentMark && IsNeedToShowComments((currentChildWidget as ParagraphItem).OwnerParagraph))
				{
					CreateBalloonForComments(currentChildWidget);
				}
				if (currentChildWidget is ParagraphItem && IsNeedToShowDeletedMarkUp((currentChildWidget as ParagraphItem).OwnerParagraph) && ((currentChildWidget as ParagraphItem).IsDeleteRevision || (currentChildWidget is Break && (currentChildWidget as Break).CharacterFormat.IsDeleteRevision)))
				{
					CreateBalloonForDeletedParagraphItem(currentChildWidget);
				}
				else if (currentChildWidget is WParagraph && IsNeedToShowDeletedMarkUp(currentChildWidget as WParagraph) && (currentChildWidget as WParagraph).BreakCharacterFormat.IsDeleteRevision && (currentChildWidget as WParagraph).IsDeletionParagraph())
				{
					CreateBalloonForDeletedParagraphText(currentChildWidget);
				}
				if (currentChildWidget is WTextRange && IsNeedToShowFormattingMarkUp((currentChildWidget as ParagraphItem).OwnerParagraph) && ((currentChildWidget as ParagraphItem).IsChangedCFormat || (currentChildWidget is Break && (currentChildWidget as Break).CharacterFormat.IsChangedFormat)))
				{
					CreateBalloonValueForCFormat(currentChildWidget);
				}
				else if (currentChildWidget is WParagraph && IsNeedToShowFormattingMarkUp(currentChildWidget as WParagraph) && (currentChildWidget as WParagraph).IsChangedPFormat)
				{
					CreateBalloonValueForPFormat(currentChildWidget);
					if (!(currentChildWidget as WParagraph).ListFormat.IsEmptyList)
					{
						CreateBalloonValueForListFormat(currentChildWidget);
					}
				}
				else if (currentChildWidget is WTable && IsNeedToShowFormattingMarkUp(currentChildWidget as WTable) && (currentChildWidget as WTable).TableFormat.IsFormattingChange)
				{
					CreateBalloonValueForTableFormat(currentChildWidget);
				}
			}
			if (DocumentLayouter.IsLayoutingHeaderFooter)
			{
				UpdateExpressionField(currentChildWidget);
			}
			if (currentChildWidget is WField)
			{
				WField wField = currentChildWidget as WField;
				if (wField.FieldType == FieldType.FieldUnknown && wField.FieldEnd != null && wField.FieldEnd.OwnerParagraph.Owner is WTableCell && (!(wField.OwnerParagraph.Owner is WTableCell) || wField.FieldEnd.OwnerParagraph.Owner != wField.OwnerParagraph.Owner))
				{
					(m_lcOperator as Layouter).UnknownField = wField;
				}
			}
			LayoutFootnoteSplittedWidgets(currentChildWidget);
			LayoutEndnoteSplittedWidgets(currentChildWidget);
			if (currentChildWidget == null)
			{
				break;
			}
			if ((currentChildWidget.LayoutInfo != null && currentChildWidget.LayoutInfo.IsSkip) || IsSkipParaMarkItem(currentChildWidget) || IsSkipParagraphBreak(currentChildWidget))
			{
				if (currentChildWidget is WParagraph)
				{
					WParagraph item = currentChildWidget as WParagraph;
					if (!(m_lcOperator as Layouter).HiddenParagraphCollection.Contains(item))
					{
						(m_lcOperator as Layouter).HiddenParagraphCollection.Add(item);
					}
				}
				if (currentChildWidget is TableOfContent)
				{
					(m_lcOperator as Layouter).LayoutingTOC = currentChildWidget as TableOfContent;
				}
				if (currentChildWidget is WFieldMark && (m_lcOperator as Layouter).LayoutingTOC != null && (m_lcOperator as Layouter).LayoutingTOC.TOCField != null && currentChildWidget == (m_lcOperator as Layouter).LayoutingTOC.TOCField.FieldEnd)
				{
					(m_lcOperator as Layouter).LayoutingTOC = null;
				}
				if (currentChildWidget is XmlParagraphItem xmlParagraphItem && !(xmlParagraphItem.Owner is InlineContentControl) && xmlParagraphItem.MathParaItemsCollection != null && xmlParagraphItem.MathParaItemsCollection.Count > 0)
				{
					int num = m_curWidgetIndex + 1;
					if (WidgetContainer is SplitWidgetContainer)
					{
						num = WidgetContainer.WidgetInnerCollection.InnerList.IndexOf(currentChildWidget) + 1;
					}
					foreach (ParagraphItem item2 in xmlParagraphItem.MathParaItemsCollection)
					{
						if (WidgetContainer.WidgetInnerCollection.InnerList.IndexOf(item2) == -1)
						{
							if (xmlParagraphItem.Owner is WParagraph && xmlParagraphItem.OwnerParagraph.HasSDTInlineItem)
							{
								WidgetContainer.WidgetInnerCollection.InnerList.Insert(num, item2);
							}
							else
							{
								WidgetContainer.WidgetInnerCollection.Insert(num, item2);
							}
							num++;
						}
					}
				}
				if (!NextChildWidget())
				{
					m_bAtLastOneChildFitted = true;
					return null;
				}
				continue;
			}
			if (currentChildWidget is Break && (currentChildWidget as Break).BreakType == BreakType.PageBreak)
			{
				int index = (currentChildWidget as Break).Index;
				WParagraph ownerParagraph = (currentChildWidget as Break).OwnerParagraph;
				bool flag = false;
				if (WidgetContainer.WidgetInnerCollection.Count - 1 != index && !IsNeedToSkipMovingBreakItem(ownerParagraph))
				{
					for (int i = index + 1; i < WidgetContainer.WidgetInnerCollection.Count; i++)
					{
						Entity entity = WidgetContainer.WidgetInnerCollection[i];
						if (entity is BookmarkStart || entity is BookmarkEnd || entity is WFieldMark || entity.IsFloatingItem(isTextWrapAround: false))
						{
							flag = true;
							continue;
						}
						flag = false;
						break;
					}
				}
				if (flag)
				{
					WidgetContainer.WidgetInnerCollection.AddToInnerList(currentChildWidget as Break);
					WidgetContainer.WidgetInnerCollection.RemoveFromInnerList(index);
					if (WidgetContainer is SplitWidgetContainer && (WidgetContainer as SplitWidgetContainer).m_currentChild == currentChildWidget)
					{
						(WidgetContainer as SplitWidgetContainer).m_currentChild = WidgetContainer.WidgetInnerCollection[index] as IWidget;
					}
					continue;
				}
			}
			if (currentChildWidget is WTable wTable && !wTable.TableFormat.WrapTextAround && IsInFrame(wTable))
			{
				if (base.IsForceFitLayout)
				{
					currentChildWidget.LayoutInfo.IsFirstItemInPage = true;
				}
				GetFrameBounds(wTable.Rows[0].Cells[0].Paragraphs[0], m_layoutArea.ClientActiveArea);
			}
			if ((m_lcOperator as Layouter).DynamicParagraph != null && currentChildWidget is TextBodyItem)
			{
				IList innerList = (WidgetContainer.WidgetInnerCollection as BodyItemCollection).InnerList;
				int num2 = innerList.IndexOf(currentChildWidget);
				if (num2 > 0)
				{
					TextBodyItem textBodyItem = innerList[num2 - 1] as TextBodyItem;
					if ((textBodyItem is WParagraph && (m_lcOperator as Layouter).DynamicParagraph == textBodyItem as WParagraph) || (textBodyItem is WTable && LastRowHaveDynamicPara((textBodyItem as WTable).LastRow)))
					{
						(m_lcOperator as Layouter).DynamicParagraph = null;
					}
				}
				else if (num2 == 0 && currentChildWidget is WParagraph && (currentChildWidget as WParagraph).Owner is WTableCell)
				{
					WTableCell wTableCell = (currentChildWidget as WParagraph).Owner as WTableCell;
					if (wTableCell.PreviousSibling is WTableCell)
					{
						wTableCell = wTableCell.PreviousSibling as WTableCell;
						if (wTableCell.ChildEntities.Contains((m_lcOperator as Layouter).DynamicParagraph))
						{
							(m_lcOperator as Layouter).DynamicParagraph = null;
						}
					}
					else if (wTableCell.OwnerRow.Index > 0 && LastRowHaveDynamicPara(wTableCell.OwnerRow.PreviousSibling as WTableRow))
					{
						(m_lcOperator as Layouter).DynamicParagraph = null;
					}
				}
			}
			UpdateTextBodyItemPosition(currentChildWidget);
			if (currentChildWidget is WParagraph || (currentChildWidget is SplitWidgetContainer && (currentChildWidget as SplitWidgetContainer).RealWidgetContainer is WParagraph))
			{
				(m_lcOperator as Layouter).IsTwoLinesLayouted = false;
				(m_lcOperator as Layouter).IsFootnoteHeightAdjusted = false;
			}
			if (currentChildWidget is WTextRange || currentChildWidget is WFootnote)
			{
				WParagraph wParagraph = ((currentChildWidget is WFootnote) ? (currentChildWidget as WFootnote).OwnerParagraph : (currentChildWidget as WTextRange).OwnerParagraph);
				if (wParagraph != null)
				{
					currentChildWidget.LayoutInfo.IsClipped = ((IWidget)wParagraph).LayoutInfo.IsClipped;
				}
				else
				{
					currentChildWidget.LayoutInfo.IsClipped = base.LayoutInfo.IsClipped;
				}
			}
			return LayoutContext.Create(currentChildWidget, m_lcOperator, base.IsForceFitLayout);
		}
		if ((m_lcOperator as Layouter).DynamicParagraph != null && WidgetContainer.WidgetInnerCollection is BodyItemCollection)
		{
			IList innerList2 = (WidgetContainer.WidgetInnerCollection as BodyItemCollection).InnerList;
			if (innerList2.Count > 0 && innerList2[innerList2.Count - 1] is TextBodyItem)
			{
				TextBodyItem textBodyItem2 = innerList2[innerList2.Count - 1] as TextBodyItem;
				if ((textBodyItem2 is WParagraph && (m_lcOperator as Layouter).DynamicParagraph == textBodyItem2 as WParagraph) || (textBodyItem2 is WTable && LastRowHaveDynamicPara((textBodyItem2 as WTable).LastRow)))
				{
					(m_lcOperator as Layouter).DynamicParagraph = null;
				}
			}
		}
		m_bAtLastOneChildFitted = true;
		return null;
	}

	private void RemoveLineLayoutedWidgetFromTable()
	{
		int num = 0;
		while (num < m_ltWidget.ChildWidgets.Count)
		{
			LayoutedWidget layoutedWidget = m_ltWidget.ChildWidgets[num];
			if (layoutedWidget.ChildWidgets.Count >= 1 && !(layoutedWidget.Widget is WTable) && !(layoutedWidget.Widget is SplitTableWidget) && !((layoutedWidget.ChildWidgets[0].Widget is SplitWidgetContainer) ? ((layoutedWidget.ChildWidgets[0].Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) : (layoutedWidget.ChildWidgets[0].Widget is WParagraph)))
			{
				m_ltWidget.ChildWidgets.Remove(layoutedWidget);
			}
			else
			{
				num++;
			}
		}
	}

	private bool IsInSameTextBody(WField field)
	{
		WParagraph ownerParagraph = field.OwnerParagraph;
		WParagraph ownerParagraph2 = field.FieldSeparator.OwnerParagraph;
		if (ownerParagraph.OwnerTextBody != ownerParagraph2.OwnerTextBody)
		{
			return false;
		}
		return true;
	}

	private void UpdateAboluteTabPosition(WAbsoluteTab absoluteTab)
	{
		if (absoluteTab.OwnerParagraph != null)
		{
			(absoluteTab.m_layoutInfo as TabsLayoutInfo).m_list[0].Position = absoluteTab.GetAbsolutePosition((m_lcOperator as Layouter).CurrentSection, 0f);
		}
	}

	private bool IsNeedToShowComments(TextBodyItem bodyItem)
	{
		if (bodyItem != null && bodyItem.Document != null)
		{
			return bodyItem.Document.RevisionOptions.CommentDisplayMode == CommentDisplayMode.ShowInBalloons;
		}
		return false;
	}

	private void CreateBalloonForComments(IWidget childWidget)
	{
		CommentMarkType type = (childWidget as WCommentMark).Type;
		WComment comment = (childWidget as WCommentMark).Comment;
		if (comment != null && (type == CommentMarkType.CommentEnd || comment.CommentRangeEnd == null))
		{
			CommentsMarkups commentsMarkups = new CommentsMarkups((childWidget as ParagraphItem).Document, comment);
			commentsMarkups.AppendInCommentsBalloon();
			commentsMarkups.Position = base.LayoutArea.ClientActiveArea.Location;
			commentsMarkups.BallonYPosition = base.LayoutArea.ClientActiveArea.Location.Y;
			(m_lcOperator as Layouter).TrackChangesMarkups.Add(commentsMarkups);
		}
	}

	private bool IsNeedToShowDeletedMarkUp(TextBodyItem bodyItem)
	{
		if (bodyItem != null && bodyItem.Document != null)
		{
			RevisionOptions revisionOptions = bodyItem.Document.RevisionOptions;
			if ((revisionOptions.ShowMarkup & RevisionType.Deletions) == RevisionType.Deletions && (revisionOptions.BalloonOptions & RevisionBalloonsOptions.Deletions) == RevisionBalloonsOptions.Deletions)
			{
				return (revisionOptions.ShowInBalloons & RevisionType.Deletions) == RevisionType.Deletions;
			}
			return false;
		}
		return false;
	}

	private bool IsNeedToShowFormattingMarkUp(TextBodyItem bodyItem)
	{
		if (bodyItem != null && bodyItem.Document != null)
		{
			RevisionOptions revisionOptions = bodyItem.Document.RevisionOptions;
			if ((revisionOptions.ShowMarkup & RevisionType.Formatting) == RevisionType.Formatting && (revisionOptions.BalloonOptions & RevisionBalloonsOptions.Formatting) == RevisionBalloonsOptions.Formatting)
			{
				return (revisionOptions.ShowInBalloons & RevisionType.Formatting) == RevisionType.Formatting;
			}
			return false;
		}
		return false;
	}

	private bool IsNeedToCreateNewBalloonForCFormat(ParagraphItem paragraphItem, string newBalloonValue)
	{
		if (paragraphItem.Owner != null && paragraphItem.Owner is InlineContentControl)
		{
			return true;
		}
		IEntity previousSibling = paragraphItem.PreviousSibling;
		if (previousSibling == null)
		{
			WParagraph ownerParagraphValue = paragraphItem.GetOwnerParagraphValue();
			if (ownerParagraphValue == null)
			{
				return true;
			}
			WParagraph previousParagraph = GetPreviousParagraph(ownerParagraphValue);
			if (previousParagraph != null && previousParagraph.BreakCharacterFormat.IsChangedFormat)
			{
				return !GetPreviousFormattedBalloonValue().Equals(newBalloonValue, StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		if ((previousSibling is WTextRange && (previousSibling as ParagraphItem).IsChangedCFormat) || (previousSibling is Break && (previousSibling as Break).CharacterFormat.IsChangedFormat))
		{
			return !GetPreviousFormattedBalloonValue().Equals(newBalloonValue, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private void CreateBalloonForDeletedParagraphItem(IWidget childWidget)
	{
		if (!(childWidget as ParagraphItem).Document.RevisionOptions.ShowDeletedText)
		{
			childWidget.LayoutInfo.IsSkip = true;
		}
		TrackChangesMarkups trackChangesMarkups = new TrackChangesMarkups((childWidget as ParagraphItem).Document);
		IEntity previousParagraphDeletedItem = GetPreviousParagraphDeletedItem(childWidget as Entity);
		bool flag = false;
		if (previousParagraphDeletedItem is ParagraphItem && ((previousParagraphDeletedItem as ParagraphItem).IsDeleteRevision || (previousParagraphDeletedItem is Break && (previousParagraphDeletedItem as Break).CharacterFormat.IsDeleteRevision)) && (m_lcOperator as Layouter).TrackChangesMarkups.Count > 0 && !((m_lcOperator as Layouter).TrackChangesMarkups[(m_lcOperator as Layouter).TrackChangesMarkups.Count - 1] is CommentsMarkups))
		{
			flag = true;
			if (childWidget is Break)
			{
				trackChangesMarkups.ChangedValue.AddParagraph().AppendBreak(BreakType.LineBreak);
			}
			else if (childWidget is WTextRange)
			{
				trackChangesMarkups = (m_lcOperator as Layouter).TrackChangesMarkups[(m_lcOperator as Layouter).TrackChangesMarkups.Count - 1];
				if (trackChangesMarkups.ChangedValue.ChildEntities.Count == 0)
				{
					trackChangesMarkups.ChangedValue.AddParagraph();
				}
				trackChangesMarkups.AppendInDeletionBalloon(childWidget as WTextRange);
			}
		}
		else
		{
			trackChangesMarkups.TypeOfMarkup = RevisionType.Deletions;
			if (childWidget is WTextRange)
			{
				trackChangesMarkups.ChangedValue.AddParagraph();
				AppendContentsForDeletedTextRange(trackChangesMarkups, childWidget);
				trackChangesMarkups.Position = base.LayoutArea.ClientActiveArea.Location;
				trackChangesMarkups.BallonYPosition = base.LayoutArea.ClientActiveArea.Location.Y;
			}
			else if (childWidget is WPicture)
			{
				trackChangesMarkups.ChangedValue.AddParagraph();
				AppendContentsForDeletedImage(trackChangesMarkups, childWidget);
				trackChangesMarkups.Position = base.LayoutArea.ClientActiveArea.Location;
				trackChangesMarkups.BallonYPosition = base.LayoutArea.ClientActiveArea.Location.Y;
			}
		}
		if (!flag)
		{
			(m_lcOperator as Layouter).TrackChangesMarkups.Add(trackChangesMarkups);
		}
		if (m_ltWidget != null)
		{
			m_ltWidget.IsTrackChanges = true;
		}
	}

	private void AppendContentsForDeletedTextRange(TrackChangesMarkups trackChangesMarkups, IWidget childWidget)
	{
		if (childWidget is ParagraphItem && (childWidget as ParagraphItem).OwnerParagraph != null && (childWidget as ParagraphItem).OwnerParagraph.ParagraphFormat.Bidi)
		{
			if ((childWidget as WTextRange).CharacterRange == CharacterRangeType.RTL)
			{
				trackChangesMarkups.ChangedValue.LastParagraph.AppendText(" :" + trackChangesMarkups.GetBalloonValueForMarkupType()).CharacterFormat.Bold = true;
				trackChangesMarkups.AppendInDeletionBalloon(childWidget as WTextRange);
			}
			else
			{
				trackChangesMarkups.AppendInDeletionBalloon(childWidget as WTextRange);
				trackChangesMarkups.ChangedValue.LastParagraph.AppendText(" :" + trackChangesMarkups.GetBalloonValueForMarkupType()).CharacterFormat.Bold = true;
			}
			trackChangesMarkups.ChangedValue.LastParagraph.ParagraphFormat.Bidi = true;
		}
		else
		{
			trackChangesMarkups.ChangedValue.LastParagraph.AppendText(trackChangesMarkups.GetBalloonValueForMarkupType() + ": ").CharacterFormat.Bold = true;
			trackChangesMarkups.AppendInDeletionBalloon(childWidget as WTextRange);
		}
	}

	private void AppendContentsForDeletedImage(TrackChangesMarkups trackChangesMarkups, IWidget childWidget)
	{
		if (childWidget is ParagraphItem && (childWidget as ParagraphItem).OwnerParagraph != null && (childWidget as ParagraphItem).OwnerParagraph.ParagraphFormat.Bidi)
		{
			trackChangesMarkups.ChangedValue.LastParagraph.AppendPicture((childWidget as WPicture).ImageBytes);
			trackChangesMarkups.ChangedValue.LastParagraph.AppendText(" :" + trackChangesMarkups.GetBalloonValueForMarkupType()).CharacterFormat.Bold = true;
			trackChangesMarkups.ChangedValue.LastParagraph.ParagraphFormat.Bidi = true;
		}
		else
		{
			trackChangesMarkups.ChangedValue.LastParagraph.AppendText(trackChangesMarkups.GetBalloonValueForMarkupType() + ": ").CharacterFormat.Bold = true;
			trackChangesMarkups.ChangedValue.LastParagraph.AppendPicture((childWidget as WPicture).ImageBytes);
		}
	}

	private void CreateBalloonForDeletedParagraphText(IWidget childWidget)
	{
		if (!(childWidget as WParagraph).Document.RevisionOptions.ShowDeletedText)
		{
			childWidget.LayoutInfo.IsSkip = true;
		}
		TrackChangesMarkups trackChangesMarkups = new TrackChangesMarkups((childWidget as WParagraph).Document);
		WParagraph previousParagraph = GetPreviousParagraph(childWidget as WParagraph);
		bool flag = false;
		if (previousParagraph != null && previousParagraph.BreakCharacterFormat.IsDeleteRevision && (m_lcOperator as Layouter).TrackChangesMarkups.Count > 0 && !((m_lcOperator as Layouter).TrackChangesMarkups[(m_lcOperator as Layouter).TrackChangesMarkups.Count - 1] is CommentsMarkups))
		{
			flag = true;
			trackChangesMarkups = (m_lcOperator as Layouter).TrackChangesMarkups[(m_lcOperator as Layouter).TrackChangesMarkups.Count - 1];
			if (trackChangesMarkups.ChangedValue.ChildEntities.Count == 0)
			{
				trackChangesMarkups.ChangedValue.AddParagraph();
			}
			trackChangesMarkups.ChangedValue.LastParagraph.AppendBreak(BreakType.LineBreak);
			foreach (ParagraphItem item in (childWidget as WParagraph).Items)
			{
				if (item is WTextRange)
				{
					trackChangesMarkups.AppendInDeletionBalloon(item as WTextRange);
				}
			}
		}
		else
		{
			trackChangesMarkups.TypeOfMarkup = RevisionType.Deletions;
			trackChangesMarkups.ChangedValue.AddParagraph();
			AppendContentsForDeletedParagraph(trackChangesMarkups, childWidget);
			trackChangesMarkups.Position = base.LayoutArea.ClientActiveArea.Location;
			trackChangesMarkups.BallonYPosition = base.LayoutArea.ClientActiveArea.Location.Y;
		}
		if (!flag)
		{
			(m_lcOperator as Layouter).TrackChangesMarkups.Add(trackChangesMarkups);
		}
		if (m_ltWidget != null)
		{
			m_ltWidget.IsTrackChanges = true;
		}
	}

	private void AppendContentsForDeletedParagraph(TrackChangesMarkups trackChangesMarkups, IWidget childWidget)
	{
		bool flag = false;
		if (!(childWidget is WParagraph))
		{
			return;
		}
		foreach (ParagraphItem item in (childWidget as WParagraph).Items)
		{
			if (item is WTextRange && (item as WTextRange).CharacterRange == CharacterRangeType.RTL)
			{
				flag = true;
				break;
			}
		}
		if ((childWidget as WParagraph).ParagraphFormat.Bidi)
		{
			if (flag)
			{
				int num = 0;
				foreach (ParagraphItem item2 in (childWidget as WParagraph).Items)
				{
					if (item2 is WTextRange && (item2 as WTextRange).CharacterRange == CharacterRangeType.RTL)
					{
						if (num == 0)
						{
							trackChangesMarkups.ChangedValue.LastParagraph.AppendText(" :" + trackChangesMarkups.GetBalloonValueForMarkupType()).CharacterFormat.Bold = true;
						}
						trackChangesMarkups.AppendInDeletionBalloon(item2 as WTextRange);
					}
					else
					{
						trackChangesMarkups.AppendInDeletionBalloon(item2 as WTextRange);
						if (num == 0)
						{
							trackChangesMarkups.ChangedValue.LastParagraph.AppendText(" :" + trackChangesMarkups.GetBalloonValueForMarkupType()).CharacterFormat.Bold = true;
						}
					}
					num++;
				}
			}
			else
			{
				foreach (ParagraphItem item3 in (childWidget as WParagraph).Items)
				{
					if (item3 is WTextRange)
					{
						trackChangesMarkups.AppendInDeletionBalloon(item3 as WTextRange);
					}
				}
				trackChangesMarkups.ChangedValue.LastParagraph.AppendText(" :" + trackChangesMarkups.GetBalloonValueForMarkupType()).CharacterFormat.Bold = true;
			}
			trackChangesMarkups.ChangedValue.LastParagraph.ParagraphFormat.Bidi = true;
			return;
		}
		trackChangesMarkups.ChangedValue.LastParagraph.AppendText(trackChangesMarkups.GetBalloonValueForMarkupType() + ": ").CharacterFormat.Bold = true;
		foreach (ParagraphItem item4 in (childWidget as WParagraph).Items)
		{
			if (item4 is WTextRange)
			{
				trackChangesMarkups.AppendInDeletionBalloon(item4 as WTextRange);
			}
		}
	}

	private void CreateBalloonValueForCFormat(IWidget childWidget)
	{
		TrackChangesMarkups trackChangesMarkups = new TrackChangesMarkups((childWidget as ParagraphItem).Document);
		Dictionary<int, object> dictionary = ((childWidget is WTextRange) ? new Dictionary<int, object>((childWidget as WTextRange).CharacterFormat.PropertiesHash) : new Dictionary<int, object>((childWidget as Break).CharacterFormat.PropertiesHash));
		Dictionary<int, object> standardDic = ((childWidget is WTextRange) ? new Dictionary<int, object>((childWidget as WTextRange).CharacterFormat.OldPropertiesHash) : new Dictionary<int, object>((childWidget as Break).CharacterFormat.OldPropertiesHash));
		RemoveSameValues(dictionary, standardDic);
		if (dictionary.Count <= 0)
		{
			return;
		}
		FontScriptType scriptType = FontScriptType.English;
		if (childWidget is WTextRange)
		{
			scriptType = (childWidget as WTextRange).ScriptType;
		}
		Dictionary<int, string> hierarchyOrder = new Dictionary<int, string>();
		WCharacterFormat characterformat = ((childWidget is WTextRange) ? (childWidget as WTextRange).CharacterFormat : (childWidget as Break).CharacterFormat);
		trackChangesMarkups.DisplayBalloonValueCFormat(scriptType, dictionary, characterformat, ref hierarchyOrder);
		dictionary = ((childWidget is WTextRange) ? new Dictionary<int, object>((childWidget as WTextRange).CharacterFormat.PropertiesHash) : new Dictionary<int, object>((childWidget as Break).CharacterFormat.PropertiesHash));
		standardDic = ((childWidget is WTextRange) ? new Dictionary<int, object>((childWidget as WTextRange).CharacterFormat.OldPropertiesHash) : new Dictionary<int, object>((childWidget as Break).CharacterFormat.OldPropertiesHash));
		RemoveSameValues(standardDic, dictionary);
		if (standardDic.Count > 0)
		{
			trackChangesMarkups.DisplayBalloonValueforRemovedCFormat(standardDic, characterformat, ref hierarchyOrder);
		}
		trackChangesMarkups.ChangedValue.AddParagraph();
		trackChangesMarkups.TypeOfMarkup = RevisionType.Formatting;
		trackChangesMarkups.ChangedValue.LastParagraph.AppendText(trackChangesMarkups.GetBalloonValueForMarkupType() + ": ").CharacterFormat.Bold = true;
		trackChangesMarkups.ChangedValue.LastParagraph.AppendText(trackChangesMarkups.ConvertDictionaryValuesToString(hierarchyOrder));
		trackChangesMarkups.Position = base.LayoutArea.ClientActiveArea.Location;
		trackChangesMarkups.BallonYPosition = base.LayoutArea.ClientActiveArea.Location.Y;
		if (IsNeedToCreateNewBalloonForCFormat(childWidget as ParagraphItem, trackChangesMarkups.ChangedValue.LastParagraph.Text) && !string.IsNullOrEmpty(trackChangesMarkups.ChangedValue.LastParagraph.Text))
		{
			(m_lcOperator as Layouter).TrackChangesMarkups.Add(trackChangesMarkups);
			if (m_ltWidget != null)
			{
				m_ltWidget.IsTrackChanges = true;
			}
		}
	}

	private void RemoveSameValues(Dictionary<int, object> dicToRemove, Dictionary<int, object> standardDic)
	{
		foreach (KeyValuePair<int, object> item in standardDic)
		{
			if (dicToRemove.ContainsKey(item.Key) && dicToRemove[item.Key].Equals(item.Value))
			{
				dicToRemove.Remove(item.Key);
			}
		}
	}

	private void CreateBalloonValueForPFormat(IWidget childWidget)
	{
		TrackChangesMarkups trackChangesMarkups = new TrackChangesMarkups((childWidget as WParagraph).Document);
		Dictionary<int, object> dictionary = new Dictionary<int, object>((childWidget as WParagraph).ParagraphFormat.PropertiesHash);
		Dictionary<int, object> dictionary2 = new Dictionary<int, object>((childWidget as WParagraph).ParagraphFormat.OldPropertiesHash);
		foreach (KeyValuePair<int, object> item in dictionary2)
		{
			if (dictionary.ContainsKey(item.Key) && dictionary[item.Key].Equals(item.Value))
			{
				dictionary.Remove(item.Key);
			}
		}
		if (dictionary.Count <= 0)
		{
			return;
		}
		Dictionary<int, string> hierarchyOrder = new Dictionary<int, string>();
		trackChangesMarkups.DisplayBalloonValueForPFormat(dictionary, (childWidget as WParagraph).ParagraphFormat, ref hierarchyOrder);
		dictionary = ((childWidget is WParagraph) ? new Dictionary<int, object>((childWidget as WParagraph).ParagraphFormat.PropertiesHash) : new Dictionary<int, object>((childWidget as Break).ParaItemCharFormat.PropertiesHash));
		dictionary2 = ((childWidget is WParagraph) ? new Dictionary<int, object>((childWidget as WParagraph).ParagraphFormat.OldPropertiesHash) : new Dictionary<int, object>((childWidget as Break).ParaItemCharFormat.OldPropertiesHash));
		foreach (KeyValuePair<int, object> item2 in dictionary)
		{
			if (dictionary2.ContainsKey(item2.Key) && dictionary2[item2.Key].Equals(item2.Value))
			{
				dictionary2.Remove(item2.Key);
			}
		}
		if (dictionary2.Count > 0)
		{
			trackChangesMarkups.DisplayBalloonValueForRemovedPFormat(dictionary2, (childWidget as WParagraph).ParagraphFormat, ref hierarchyOrder);
		}
		trackChangesMarkups.TypeOfMarkup = RevisionType.Formatting;
		trackChangesMarkups.ChangedValue.AddParagraph().AppendText(trackChangesMarkups.GetBalloonValueForMarkupType() + ":").CharacterFormat.Bold = true;
		trackChangesMarkups.ChangedValue.LastParagraph.AppendText(trackChangesMarkups.ConvertDictionaryValuesToString(hierarchyOrder));
		trackChangesMarkups.Position = base.LayoutArea.ClientActiveArea.Location;
		trackChangesMarkups.BallonYPosition = base.LayoutArea.ClientActiveArea.Location.Y;
		(m_lcOperator as Layouter).TrackChangesMarkups.Add(trackChangesMarkups);
		if (m_ltWidget != null)
		{
			m_ltWidget.IsTrackChanges = true;
		}
	}

	private void CreateBalloonValueForListFormat(IWidget childWidget)
	{
		TrackChangesMarkups trackChangesMarkups = (m_lcOperator as Layouter).TrackChangesMarkups[(m_lcOperator as Layouter).TrackChangesMarkups.Count - 1];
		Dictionary<int, object> dictionary = new Dictionary<int, object>((childWidget as WParagraph).ListFormat.PropertiesHash);
		Dictionary<int, object> standardDic = new Dictionary<int, object>((childWidget as WParagraph).ListFormat.OldPropertiesHash);
		RemoveSameValues(dictionary, standardDic);
		if (trackChangesMarkups.ChangedValue.ChildEntities.Count == 0)
		{
			trackChangesMarkups.ChangedValue.AddParagraph();
		}
		if (dictionary.Count > 0)
		{
			trackChangesMarkups.ChangedValue.LastParagraph.AppendText(", " + trackChangesMarkups.DisplayBalloonValueForListFormat(dictionary, (childWidget as WParagraph).ListFormat));
		}
		trackChangesMarkups.TypeOfMarkup = RevisionType.Formatting;
		trackChangesMarkups.Position = base.LayoutArea.ClientActiveArea.Location;
		trackChangesMarkups.BallonYPosition = base.LayoutArea.ClientActiveArea.Location.Y;
		(m_lcOperator as Layouter).TrackChangesMarkups.Add(trackChangesMarkups);
		if (m_ltWidget != null)
		{
			m_ltWidget.IsTrackChanges = true;
		}
	}

	private void CreateBalloonValueForTableFormat(IWidget childWidget)
	{
		TrackChangesMarkups trackChangesMarkups = new TrackChangesMarkups((childWidget as WTable).Document);
		Dictionary<int, object> dictionary = new Dictionary<int, object>((childWidget as WTable).TableFormat.PropertiesHash);
		Dictionary<int, object> standardDic = new Dictionary<int, object>((childWidget as WTable).TableFormat.OldPropertiesHash);
		RemoveSameValues(dictionary, standardDic);
		if (dictionary.Count > 0)
		{
			trackChangesMarkups.TypeOfMarkup = RevisionType.Formatting;
			trackChangesMarkups.ChangedValue.AddParagraph().AppendText(trackChangesMarkups.GetBalloonValueForMarkupType() + ": ").CharacterFormat.Bold = true;
			trackChangesMarkups.ChangedValue.LastParagraph.AppendText("Table");
			trackChangesMarkups.Position = base.LayoutArea.ClientActiveArea.Location;
			trackChangesMarkups.BallonYPosition = base.LayoutArea.ClientActiveArea.Location.Y;
		}
		(m_lcOperator as Layouter).TrackChangesMarkups.Add(trackChangesMarkups);
		if (m_ltWidget != null)
		{
			m_ltWidget.IsTrackChanges = true;
		}
	}

	private bool LastRowHaveDynamicPara(WTableRow row)
	{
		if (row != null)
		{
			for (int num = row.ChildEntities.Count - 1; num >= 0; num--)
			{
				WTableCell wTableCell = row.ChildEntities[num] as WTableCell;
				if (wTableCell.ChildEntities.Contains((m_lcOperator as Layouter).DynamicParagraph))
				{
					return true;
				}
				if (IsLastItemTable(wTableCell))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsLastItemTable(WTableCell cell)
	{
		for (int num = cell.ChildEntities.Count - 1; num >= 0; num--)
		{
			Entity entity = cell.ChildEntities[num];
			if (entity is WTable)
			{
				return LastRowHaveDynamicPara((entity as WTable).LastRow);
			}
			if (!(entity is WParagraph) || (entity as WParagraph).m_layoutInfo == null || !(entity as WParagraph).m_layoutInfo.IsSkip)
			{
				return false;
			}
		}
		return false;
	}

	private bool IsNeedToSkipMovingBreakItem(WParagraph para)
	{
		if (para != null && (para.Document.Settings.CompatibilityOptions[CompatibilityOption.SplitPgBreakAndParaMark] || para == para.Document.LastParagraph || (para.Document.ActualFormatType == FormatType.Doc && para.Document.WordVersion <= 268)))
		{
			return true;
		}
		return false;
	}

	private void UpdateExpressionField(IWidget childWidget)
	{
		WField wField = childWidget as WField;
		if (wField != null && wField.m_layoutInfo.IsSkip && wField.HasInnerPageField && !wField.IsNestedField)
		{
			for (int i = 0; i < wField.Range.Items.Count; i++)
			{
				Entity entity = wField.Range.Items[i] as Entity;
				if (entity is ParagraphItem && entity is WField { FieldType: FieldType.FieldPage } wField2)
				{
					wField2.UpdateFieldResult(((m_lcOperator as Layouter).PageNumber + 1).ToString());
				}
			}
			wField.Update();
			wField.SkipLayoutingOfFieldCode();
		}
		if (!DocumentLayouter.IsFirstLayouting)
		{
			if (wField != null && wField.IsNumPagesInsideExpressionField && !wField.IsFieldInsideUnknownField && (wField.FieldType == FieldType.FieldCompare || wField.FieldType == FieldType.FieldIf || wField.FieldType == FieldType.FieldFormula))
			{
				wField.Update();
				wField.SkipLayoutingOfFieldCode();
			}
			else if (wField != null && wField.FieldType == FieldType.FieldNumPages && wField.IsNumPageUsedForEvaluation)
			{
				childWidget.LayoutInfo.IsSkip = true;
				for (int j = 0; j < wField.Range.Items.Count; j++)
				{
					(wField.Range.Items[j] as IWidget).LayoutInfo.IsSkip = true;
				}
			}
		}
		else if (wField != null && wField.IsNumPageUsedForEvaluation && wField.FieldType != FieldType.FieldNumPages)
		{
			childWidget.LayoutInfo.IsSkip = true;
			for (int k = 0; k < wField.Range.Items.Count; k++)
			{
				(wField.Range.Items[k] as IWidget).LayoutInfo.IsSkip = true;
			}
		}
	}

	internal LayoutedWidget CheckNullConditionAndReturnltwidget()
	{
		LayoutedWidget maintainltWidget = (m_lcOperator as Layouter).MaintainltWidget;
		int[] interSectingPoint = (m_lcOperator as Layouter).m_interSectingPoint;
		if (interSectingPoint[0] != int.MinValue && interSectingPoint[1] != int.MinValue && maintainltWidget != null && maintainltWidget.ChildWidgets.Count > interSectingPoint[0] && (maintainltWidget = maintainltWidget.ChildWidgets[interSectingPoint[0]]) != null)
		{
			return maintainltWidget.ChildWidgets[interSectingPoint[1]];
		}
		return null;
	}

	private void FindIntersectPointAndRemovltWidgetForTable()
	{
		LayoutedWidget layoutedWidget = m_ltWidget;
		while (!(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget is ParagraphItem) && !(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget is WTableRow))
		{
			layoutedWidget = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
			if (layoutedWidget.ChildWidgets.Count == 0)
			{
				return;
			}
		}
		float y = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Bounds.Y;
		IWidget widget;
		if (!(layoutedWidget.Widget is SplitWidgetContainer))
		{
			if (!(layoutedWidget.Widget is SplitTableWidget))
			{
				widget = layoutedWidget.Widget;
			}
			else
			{
				IWidget tableWidget = (layoutedWidget.Widget as SplitTableWidget).TableWidget;
				widget = tableWidget;
			}
		}
		else
		{
			IWidget tableWidget = (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer;
			widget = tableWidget;
		}
		IWidget widget2 = widget;
		if (widget2 is WTable)
		{
			(m_lcOperator as Layouter).DynamicTable = widget2 as WTable;
		}
		else if (widget2 is WParagraph)
		{
			(m_lcOperator as Layouter).DynamicParagraph = widget2 as WParagraph;
		}
		layoutedWidget = m_ltWidget;
		int num = 0;
		for (int num2 = layoutedWidget.ChildWidgets.Count - 1; num2 >= 0; num2--)
		{
			if (num == 0 && y >= layoutedWidget.ChildWidgets[num2].Bounds.Bottom)
			{
				if (layoutedWidget.ChildWidgets[num2].Widget is WTable && (layoutedWidget.ChildWidgets[num2].Widget as WTable).TableFormat.WrapTextAround)
				{
					num2 = GetIntersectingWidgetItem(layoutedWidget, y, num2);
				}
				m_layoutArea.UpdateDynamicRelayoutBounds(layoutedWidget.ChildWidgets[num2].Bounds.X, layoutedWidget.ChildWidgets[num2].Bounds.Y, isNeedToUpdateWidth: false, 0f);
				m_curWidgetIndex = num2;
				(m_lcOperator as Layouter).m_interSectingPoint[0] = num2;
				m_ltWidget.ChildWidgets.RemoveRange(num2, m_ltWidget.ChildWidgets.Count - num2);
				num++;
				layoutedWidget = (m_lcOperator as Layouter).MaintainltWidget.ChildWidgets[num2];
				num2 = layoutedWidget.ChildWidgets.Count;
				if (layoutedWidget.Widget is WTable)
				{
					(m_lcOperator as Layouter).m_interSectingPoint[0] = int.MinValue;
					(m_lcOperator as Layouter).m_interSectingPoint[1] = int.MinValue;
					break;
				}
			}
			else if (y >= layoutedWidget.ChildWidgets[num2].Bounds.Bottom)
			{
				(m_lcOperator as Layouter).m_interSectingPoint[num] = num2;
				layoutedWidget = layoutedWidget.ChildWidgets[num2];
				num2 = layoutedWidget.ChildWidgets.Count;
				if (layoutedWidget.ChildWidgets.Count > 0 && (layoutedWidget.ChildWidgets[0].Widget is ParagraphItem || layoutedWidget.ChildWidgets[0].Widget is SplitStringWidget))
				{
					break;
				}
				num++;
			}
		}
		if (num == 0)
		{
			m_ltWidget.ChildWidgets.RemoveRange(0, m_ltWidget.ChildWidgets.Count);
			(m_lcOperator as Layouter).m_interSectingPoint[0] = 0;
			(m_lcOperator as Layouter).m_interSectingPoint[1] = 0;
		}
	}

	private int GetIntersectingWidgetItem(LayoutedWidget ltwidget, float yPosition, int i)
	{
		int num = int.MinValue;
		for (int num2 = i - 1; num2 >= 0; num2--)
		{
			if (yPosition < ltwidget.ChildWidgets[num2].Bounds.Bottom)
			{
				num = num2;
			}
		}
		if (num == int.MinValue)
		{
			return i;
		}
		return num;
	}

	private void LayoutFootnoteSplittedWidgets(IWidget childWidget)
	{
		if ((m_lcOperator as Layouter).FootnoteSplittedWidgets.Count <= 0 || !(childWidget is SplitWidgetContainer) || !((childWidget as SplitWidgetContainer).RealWidgetContainer is WSection))
		{
			return;
		}
		float height = 0f;
		SplitWidgetContainer[] array = new SplitWidgetContainer[(m_lcOperator as Layouter).FootnoteSplittedWidgets.Count];
		(m_lcOperator as Layouter).FootnoteSplittedWidgets.CopyTo(array);
		(m_lcOperator as Layouter).FootnoteSplittedWidgets.Clear();
		if ((m_lcOperator as Layouter).IsNeedToRestartFootnote && (array[0].RealWidgetContainer as WTextBody).Owner != null)
		{
			WFootnote wFootnote = (array[0].RealWidgetContainer as WTextBody).Owner as WFootnote;
			LayoutFootnoteTextBody(wFootnote.Document.Footnotes.ContinuationSeparator, ref height, m_layoutArea.ClientActiveArea.Height, referencedLineIsLayouted: false);
			(m_lcOperator as Layouter).IsNeedToRestartFootnote = false;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (i > 0)
			{
				height = 0f;
			}
			LayoutFootnoteTextBody(array[i], ref height, m_layoutArea.ClientActiveArea.Height, referencedLineIsLayouted: false);
			CreateLayoutArea(new RectangleF(m_layoutArea.ClientActiveArea.X, m_layoutArea.ClientActiveArea.Y, m_layoutArea.ClientActiveArea.Width, m_layoutArea.ClientActiveArea.Height - height));
		}
		int count = (m_lcOperator as Layouter).FootNoteSectionIndex.Count;
		while ((m_lcOperator as Layouter).FootnoteWidgets.Count > (m_lcOperator as Layouter).FootNoteSectionIndex.Count)
		{
			(m_lcOperator as Layouter).FootNoteSectionIndex.Add(count + 1);
		}
		UpdateForceFitLayoutState(this);
	}

	private void LayoutEndnoteSplittedWidgets(IWidget childWidget)
	{
		if ((m_lcOperator as Layouter).EndnoteSplittedWidgets.Count <= 0 || !(childWidget is SplitWidgetContainer) || !((childWidget as SplitWidgetContainer).RealWidgetContainer is WSection))
		{
			return;
		}
		float height = 0f;
		SplitWidgetContainer[] array = new SplitWidgetContainer[(m_lcOperator as Layouter).EndnoteSplittedWidgets.Count];
		(m_lcOperator as Layouter).EndnoteSplittedWidgets.CopyTo(array);
		(m_lcOperator as Layouter).EndnoteSplittedWidgets.Clear();
		if ((m_lcOperator as Layouter).IsNeedToRestartEndnote)
		{
			WTextBody wTextBody = array[0].RealWidgetContainer as WTextBody;
			LayoutEndnoteTextBody(wTextBody.Document.Endnotes.ContinuationSeparator, ref height, m_layoutArea.ClientActiveArea.Height);
			(m_lcOperator as Layouter).IsNeedToRestartEndnote = false;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (i > 0)
			{
				height = 0f;
			}
			LayoutEndnoteTextBody(array[i], ref height, m_layoutArea.ClientActiveArea.Height);
			CreateLayoutArea(new RectangleF(m_layoutArea.ClientActiveArea.X, m_layoutArea.ClientActiveArea.Y, m_layoutArea.ClientActiveArea.Width, m_layoutArea.ClientActiveArea.Height - height));
		}
		int count = (m_lcOperator as Layouter).EndNoteSectionIndex.Count;
		while ((m_lcOperator as Layouter).EndnoteWidgets.Count > (m_lcOperator as Layouter).EndNoteSectionIndex.Count)
		{
			(m_lcOperator as Layouter).EndNoteSectionIndex.Add(count + 1);
		}
	}

	private bool IsParagraphSplittedByPageBreak(WParagraph paragraph)
	{
		if (base.SplittedWidget is SplitWidgetContainer splitWidgetContainer && (splitWidgetContainer.m_currentChild is WParagraph || (splitWidgetContainer.m_currentChild is SplitWidgetContainer && (splitWidgetContainer.m_currentChild as SplitWidgetContainer).RealWidgetContainer is WParagraph)))
		{
			int num = -1;
			if (!(paragraph.PreviousSibling is WParagraph wParagraph))
			{
				return false;
			}
			num = ((!(splitWidgetContainer.m_currentChild is WParagraph)) ? (((splitWidgetContainer.m_currentChild as SplitWidgetContainer).m_currentChild is Entity) ? ((splitWidgetContainer.m_currentChild as SplitWidgetContainer).m_currentChild as Entity).Index : (-1)) : wParagraph.ChildEntities.Count);
			if (num > 0 && wParagraph.ChildEntities.Count > num - 1 && wParagraph.ChildEntities[num - 1] is Break && (wParagraph.ChildEntities[num - 1] as Break).BreakType == BreakType.PageBreak)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsPreviousParagraphHaveSectionBreak(WParagraph paragraph)
	{
		WSection wSection = GetBaseEntity(paragraph).PreviousSibling as WSection;
		if (paragraph.PreviousSibling != null || wSection == null)
		{
			return false;
		}
		Entity entity = wSection.ChildEntities[0];
		if (entity is WTextBody)
		{
			WTextBody wTextBody = entity as WTextBody;
			if (wTextBody.ChildEntities[wTextBody.ChildEntities.Count - 1] is WParagraph wParagraph && wParagraph.IsParagraphHasSectionBreak())
			{
				return true;
			}
		}
		return false;
	}

	private bool IsHeaderContentExceedsTopMargin()
	{
		IWSection currentSection = (m_lcOperator as Layouter).CurrentSection;
		if (currentSection != null && (m_lcOperator as Layouter).PageTopMargin > currentSection.PageSetup.Margins.Top)
		{
			return true;
		}
		return false;
	}

	private void UpdateTextBodyItemPosition(IWidget childWidget)
	{
		if (childWidget is WParagraph)
		{
			ParagraphLayoutInfo paragraphLayoutInfo = childWidget.LayoutInfo as ParagraphLayoutInfo;
			WParagraph wParagraph = childWidget as WParagraph;
			paragraphLayoutInfo.IsFirstLine = true;
			float num = 0f;
			if (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter && base.IsForceFitLayout && IsBaseFromSection(wParagraph) && DocumentLayouter.PageNumber != 1 && !IsPreviousParagraphHaveSectionBreak(wParagraph) && (wParagraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 || (!wParagraph.ParagraphFormat.PageBreakBefore && !IsParagraphSplittedByPageBreak(wParagraph) && !IsHeaderContentExceedsTopMargin())) && !(m_lcOperator as Layouter).IsLayoutingFootnote)
			{
				num = paragraphLayoutInfo.Margins.Top;
				paragraphLayoutInfo.Margins.Top = 0f;
			}
			UpdateParagraphMargins(wParagraph);
			if (num > 0f)
			{
				paragraphLayoutInfo.TopMargin = num;
			}
			if (IsInFrame(wParagraph))
			{
				RectangleF frameBounds = GetFrameBounds(wParagraph, m_layoutArea.ClientActiveArea);
				Layouter obj = m_lcOperator as Layouter;
				float paragraphYPosition = (paragraphLayoutInfo.YPosition = frameBounds.Y);
				obj.ParagraphYPosition = paragraphYPosition;
				paragraphLayoutInfo.XPosition = frameBounds.X;
			}
			else
			{
				paragraphLayoutInfo.YPosition = m_layoutArea.ClientActiveArea.Y;
				paragraphLayoutInfo.YPosition -= GetParagraphTopMargin(wParagraph);
				if (paragraphLayoutInfo.PargaraphOriginalYPosition != float.MinValue)
				{
					(m_lcOperator as Layouter).ParagraphYPosition = paragraphLayoutInfo.PargaraphOriginalYPosition;
				}
				else
				{
					(m_lcOperator as Layouter).ParagraphYPosition = paragraphLayoutInfo.YPosition;
				}
				paragraphLayoutInfo.XPosition = m_layoutArea.ClientActiveArea.X;
			}
			float xPosition = paragraphLayoutInfo.XPosition;
			UpdateParagraphXPositionBasedOnTextWrap(wParagraph, xPosition, paragraphLayoutInfo.YPosition);
			if (xPosition != paragraphLayoutInfo.XPosition)
			{
				wParagraph.IsXpositionUpated = true;
			}
		}
		if (childWidget is WTable)
		{
			(childWidget.LayoutInfo as TableLayoutInfo).IsHeaderNotRepeatForAllPages = false;
		}
		if (childWidget is SplitWidgetContainer && (childWidget as SplitWidgetContainer).RealWidgetContainer is WParagraph)
		{
			WParagraph wParagraph2 = (childWidget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
			int count = wParagraph2.ChildEntities.Count;
			if (!wParagraph2.IsInCell && count > (childWidget as SplitWidgetContainer).Count && wParagraph2.m_layoutInfo is ParagraphLayoutInfo)
			{
				(wParagraph2.m_layoutInfo as ParagraphLayoutInfo).YPosition = m_layoutArea.ClientActiveArea.Y;
				(m_lcOperator as Layouter).ParagraphYPosition = (wParagraph2.m_layoutInfo as ParagraphLayoutInfo).YPosition;
				(wParagraph2.m_layoutInfo as ParagraphLayoutInfo).XPosition = m_layoutArea.ClientActiveArea.X;
				UpdateParagraphXPositionBasedOnTextWrap(wParagraph2, (wParagraph2.m_layoutInfo as ParagraphLayoutInfo).XPosition, (childWidget.LayoutInfo as ParagraphLayoutInfo).YPosition);
			}
		}
	}

	internal float GetFootnoteHeight()
	{
		float height = 0f;
		WParagraph wParagraph = ((m_currChildLW.Widget is WParagraph) ? (m_currChildLW.Widget as WParagraph) : ((m_currChildLW.Widget is SplitWidgetContainer && (m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) ? ((m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null));
		if (wParagraph != null)
		{
			if (wParagraph.IsInCell)
			{
				if (!((wParagraph.GetOwnerEntity() as WTableCell).OwnerRow.m_layoutInfo as RowLayoutInfo).IsFootnoteReduced)
				{
					m_currChildLW.GetFootnoteHeight(ref height);
				}
			}
			else
			{
				m_currChildLW.GetFootnoteHeight(ref height);
			}
		}
		else if (m_currChildLW.Widget is WTable)
		{
			m_currChildLW.GetFootnoteHeight(ref height);
		}
		else if (m_currChildLW.Widget is BlockContentControl)
		{
			m_currChildLW.GetFootnoteHeight(ref height);
		}
		return height;
	}

	private bool IsUpdatedParagraph(ParagraphLayoutInfo paraInfo)
	{
		if (DocumentLayouter.IsLayoutingHeaderFooter)
		{
			if (!(paraInfo.TopMargin > 0f) && !(paraInfo.BottomMargin > 0f) && !(paraInfo.TopPadding > 0f))
			{
				return paraInfo.BottomPadding > 0f;
			}
			return true;
		}
		return false;
	}

	private void UpdateParagraphMargins(WParagraph paragraph)
	{
		ParagraphLayoutInfo paragraphLayoutInfo = ((IWidget)paragraph).LayoutInfo as ParagraphLayoutInfo;
		if (DocumentLayouter.IsFirstLayouting && !IsUpdatedParagraph(paragraphLayoutInfo))
		{
			paragraphLayoutInfo.TopMargin = paragraphLayoutInfo.Margins.Top;
			paragraphLayoutInfo.BottomMargin = paragraphLayoutInfo.Margins.Bottom;
			paragraphLayoutInfo.TopPadding = paragraphLayoutInfo.Paddings.Top;
			paragraphLayoutInfo.BottomPadding = paragraphLayoutInfo.Paddings.Bottom;
			if (paragraph.IsParagraphBeforeSpacingNeedToSkip())
			{
				paragraphLayoutInfo.Margins.Top = 0f;
				paragraphLayoutInfo.Margins.Bottom = 0f;
			}
		}
		else
		{
			paragraphLayoutInfo.Margins.Top = paragraphLayoutInfo.TopMargin;
			paragraphLayoutInfo.Margins.Bottom = paragraphLayoutInfo.BottomMargin;
			paragraphLayoutInfo.Paddings.Top = paragraphLayoutInfo.TopPadding;
			paragraphLayoutInfo.Paddings.Bottom = paragraphLayoutInfo.BottomPadding;
			ResetFloatingEntityProperty(paragraph);
		}
	}

	protected virtual void MarkAsNotFitted(LayoutContext childContext, bool isFootnote)
	{
		if (childContext.Widget is WFootnote)
		{
			childContext.Widget.InitLayoutInfo();
		}
		base.IsVerticalNotFitted = childContext.IsVerticalNotFitted;
		IWidget splittedWidget = null;
		bool isKeep = IsNeedToCommitKeepWithNext();
		CommitKeepWithNext(ref splittedWidget, isKeep);
		if (m_ltWidget.ChildWidgets.Count > 0 && m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Widget is WParagraph)
		{
			m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].IsLastItemInPage = true;
			RectangleF bounds = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Bounds;
			Borders borders = (m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Widget as WParagraph).ParagraphFormat.Borders;
			if (!borders.NoBorder && borders.Bottom.BorderType != 0 && (m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Widget.LayoutInfo as ParagraphLayoutInfo).Paddings.Bottom == 0f)
			{
				(m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Widget.LayoutInfo as ParagraphLayoutInfo).Paddings.Bottom = borders.Bottom.Space;
				bounds.Height += borders.Bottom.Space;
				m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Bounds = bounds;
			}
		}
		WSection wSection = ((m_sptWidget is WSection) ? (m_sptWidget as WSection) : ((m_sptWidget is SplitWidgetContainer) ? ((m_sptWidget as SplitWidgetContainer).RealWidgetContainer as WSection) : null));
		if (IsNeedToSetNotFitted(isKeep))
		{
			m_ltState = LayoutState.NotFitted;
		}
		else if (m_bAtLastOneChildFitted || ((m_lcOperator as Layouter).FootnoteWidgets.Count > 1 && wSection != null && (m_lcOperator as Layouter).ClientLayoutArea.Height - ((m_lcOperator as Layouter).FootnoteWidgets[(m_lcOperator as Layouter).FootnoteWidgets.Count - 1].Bounds.Bottom - (m_lcOperator as Layouter).ClientLayoutArea.Y) < CurrentChildWidget.LayoutInfo.Size.Height) || (splittedWidget == null && m_sptWidget is WParagraph && !(m_sptWidget as WParagraph).IsInCell && !IsInFrame(m_sptWidget as WParagraph) && (m_lcOperator as Layouter).DynamicParagraph == null))
		{
			if (splittedWidget != null)
			{
				SplitedUpWidget(splittedWidget, isEndNoteSplitWidgets: false);
			}
			else
			{
				if (isFootnote && CurrentChildWidget is WFootnote && ((m_lcOperator as Layouter).FootnoteWidgets.Count == 0 || m_ltState == LayoutState.NotFitted))
				{
					if (WidgetContainer is SplitWidgetContainer)
					{
						m_sptWidget = WidgetContainer as SplitWidgetContainer;
					}
					else
					{
						m_sptWidget = new SplitWidgetContainer(WidgetContainer, WidgetContainer.WidgetInnerCollection[0] as IWidget, 0);
					}
					m_ltState = LayoutState.NotFitted;
					return;
				}
				SplitedUpWidget(CurrentChildWidget, isEndNoteSplitWidgets: false);
			}
			if (childContext is LCLineContainer)
			{
				WParagraph wParagraph = ((childContext.Widget is SplitWidgetContainer) ? ((childContext.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : (childContext.Widget as WParagraph));
				WParagraph dynamicParagraph = (m_lcOperator as Layouter).DynamicParagraph;
				if (wParagraph != null && (m_lcOperator as Layouter).DynamicParagraph == wParagraph && !wParagraph.ParagraphFormat.IsInFrame())
				{
					(m_lcOperator as Layouter).DynamicParagraph = null;
				}
				else if (wParagraph != null && wParagraph.IsInCell && dynamicParagraph != null && dynamicParagraph.IsInCell && wParagraph.Owner == dynamicParagraph.Owner && !wParagraph.ParagraphFormat.IsInFrame() && dynamicParagraph.Index > wParagraph.Index)
				{
					(m_lcOperator as Layouter).DynamicParagraph = null;
				}
			}
			UpdateSplittedWidgetIndex(childContext);
			if (base.LayoutInfo is ParagraphLayoutInfo paragraphLayoutInfo)
			{
				paragraphLayoutInfo.IsFirstLine = false;
			}
			m_ltState = LayoutState.Splitted;
		}
		else
		{
			m_ltState = LayoutState.NotFitted;
		}
	}

	internal bool IsNeedToSetNotFitted(bool isKeep)
	{
		if (m_ltWidget.Widget is BlockContentControl && (!isKeep || (m_ltWidget.Widget as BlockContentControl).ChildEntities.Count == 0) && !m_ltWidget.Widget.LayoutInfo.IsFirstItemInPage && IsNotFittedItem((m_ltWidget.Widget as BlockContentControl).PreviousSibling as IWidget))
		{
			return true;
		}
		if (m_ltWidget.Widget is SplitWidgetContainer && (m_ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer is BlockContentControl && (!isKeep || ((m_ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer as BlockContentControl).ChildEntities.Count == 0) && !(m_ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer.LayoutInfo.IsFirstItemInPage)
		{
			return true;
		}
		return false;
	}

	private IWidget GetPriviousSibling(IWidget widget)
	{
		if (widget is WParagraph)
		{
			return (widget as WParagraph).PreviousSibling as IWidget;
		}
		if (widget is WTable)
		{
			return (widget as WTable).PreviousSibling as IWidget;
		}
		return null;
	}

	private bool IsNotFittedItem(IWidget widget)
	{
		if (widget != null && !widget.LayoutInfo.IsKeepWithNext)
		{
			return true;
		}
		if (widget != null && !widget.LayoutInfo.IsFirstItemInPage)
		{
			return IsNotFittedItem(GetPriviousSibling(widget));
		}
		return false;
	}

	internal void RemoveAutoHyphenatedString(IWidget SplittedWidget, bool isAutoHyphen)
	{
		string text = null;
		bool flag = false;
		int index = m_ltWidget.ChildWidgets.Count - 1;
		if (m_ltWidget.ChildWidgets[index].Widget is WTable)
		{
			return;
		}
		int index2 = m_ltWidget.ChildWidgets[index].ChildWidgets.Count - 1;
		int num = m_ltWidget.ChildWidgets[index].ChildWidgets[index2].ChildWidgets.Count - 1;
		if (isAutoHyphen)
		{
			SplitStringWidget splitStringWidget = ((num >= 0) ? (m_ltWidget.ChildWidgets[index].ChildWidgets[index2].ChildWidgets[num].Widget as SplitStringWidget) : null);
			bool num2 = SplittedWidget is SplitWidgetContainer && (SplittedWidget as SplitWidgetContainer).m_currentChild is SplitStringWidget && splitStringWidget != null;
			SplitStringWidget splitStringWidget2 = (SplittedWidget as SplitWidgetContainer).m_currentChild as SplitStringWidget;
			if (num2 && splitStringWidget.SplittedText.EndsWith("-") && !splitStringWidget.SplittedText.Trim().Equals("-") && !string.IsNullOrEmpty(splitStringWidget2.SplittedText))
			{
				text = GetPeekWord(splitStringWidget.SplittedText);
				_ = text + splitStringWidget2.SplittedText;
				int startIndex = splitStringWidget2.StartIndex;
				StringBuilder stringBuilder = new StringBuilder((splitStringWidget.RealStringWidget as WTextRange).Text);
				stringBuilder.Remove(splitStringWidget.StartIndex + (splitStringWidget.SplittedText.Length - 1), 1);
				(splitStringWidget.RealStringWidget as WTextRange).Text = stringBuilder.ToString();
				splitStringWidget2.StartIndex = startIndex - (text.Length + 1);
				splitStringWidget2.Length += text.Length;
				string value = splitStringWidget.SplittedText.TrimEnd('-').Trim();
				if (text.Equals(value))
				{
					m_ltWidget.ChildWidgets[index].ChildWidgets[index2].ChildWidgets.RemoveAt(num);
				}
				else
				{
					splitStringWidget.Length -= text.Length + 1;
					flag = true;
				}
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			LayoutedWidget layoutedWidget = m_ltWidget.ChildWidgets[index].ChildWidgets[index2];
			RectangleF bounds = layoutedWidget.Bounds;
			LayoutedWidget layoutedWidget2 = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
			RectangleF bounds2 = layoutedWidget2.Bounds;
			WTextRange wTextRange = ((layoutedWidget2.Widget is SplitStringWidget) ? ((layoutedWidget2.Widget as SplitStringWidget).RealStringWidget as WTextRange) : null);
			SizeF sizeF = base.DrawingContext.MeasureString(text + "-", wTextRange.CharacterFormat.GetFontToRender(wTextRange.ScriptType), null, wTextRange.ScriptType);
			bounds2.Width -= sizeF.Width;
			bounds.Width -= sizeF.Width;
			if (flag)
			{
				m_ltWidget.ChildWidgets[index].ChildWidgets[index2].ChildWidgets[num].Bounds = new RectangleF(bounds2.X, bounds2.Y, bounds2.Width, bounds2.Height);
			}
			m_ltWidget.ChildWidgets[index].ChildWidgets[index2].Bounds = new RectangleF(bounds.X, bounds.Y, bounds.Width, bounds.Height);
			UpdateltBounds(m_ltWidget.ChildWidgets[index]);
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

	private bool IsPageBreakInTable(IWidget LeafWidget)
	{
		if (LeafWidget is Break)
		{
			Break @break = LeafWidget as Break;
			if (@break.OwnerParagraph != null && ((IWParagraph)@break.OwnerParagraph).IsInCell)
			{
				return true;
			}
		}
		return false;
	}

	internal WParagraph GetParagraph()
	{
		WParagraph result = null;
		if (m_currChildLW.Widget is WParagraph)
		{
			result = m_currChildLW.Widget as WParagraph;
		}
		else if (m_currChildLW.Widget is SplitWidgetContainer && (m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)
		{
			result = (m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
		}
		return result;
	}

	private void RemoveXmlMathParaItems(WParagraph paragraph)
	{
		for (int i = 0; i < paragraph.Items.Count; i++)
		{
			if (!(paragraph.Items[i] is XmlParagraphItem))
			{
				continue;
			}
			XmlParagraphItem xmlParagraphItem = paragraph.Items[i] as XmlParagraphItem;
			if (xmlParagraphItem.MathParaItemsCollection == null || xmlParagraphItem.MathParaItemsCollection.Count <= 0)
			{
				continue;
			}
			foreach (ParagraphItem item in xmlParagraphItem.MathParaItemsCollection)
			{
				int num = paragraph.Items.InnerList.IndexOf(item);
				if (num != -1)
				{
					paragraph.Items.RemoveAt(num);
				}
			}
		}
	}

	protected virtual void MarkAsFitted(LayoutContext childContext)
	{
		AddChildLW(childContext);
		if (childContext is LCContainer && !(childContext is LCLineContainer) && m_currChildLW.IsNotFitted)
		{
			return;
		}
		if (childContext is LCLineContainer)
		{
			WParagraph paragraph = GetParagraph();
			if (paragraph != null && !paragraph.HasSDTInlineItem)
			{
				RemoveXmlMathParaItems(paragraph);
			}
			string breakCFormatBalloonValue = "";
			if (paragraph != null && paragraph.BreakCharacterFormat.IsChangedFormat && IsNeedToShowFormattingMarkUp(paragraph) && IsNeedToCreateBalloonForBreakCharacterFormat(paragraph, ref breakCFormatBalloonValue) && !(m_lcOperator as Layouter).IsLayoutingTrackChanges)
			{
				TrackChangesMarkups trackChangesMarkups = new TrackChangesMarkups(paragraph.Document);
				trackChangesMarkups.TypeOfMarkup = RevisionType.Formatting;
				trackChangesMarkups.ChangedValue.AddParagraph().AppendText(trackChangesMarkups.GetBalloonValueForMarkupType() + ": ").CharacterFormat.Bold = true;
				trackChangesMarkups.ChangedValue.LastParagraph.AppendText(breakCFormatBalloonValue);
				trackChangesMarkups.Position = new PointF(m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Bounds.Right, m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Bounds.Y);
				trackChangesMarkups.BallonYPosition = base.LayoutArea.ClientActiveArea.Location.Y;
				(m_lcOperator as Layouter).TrackChangesMarkups.Add(trackChangesMarkups);
				if (m_ltWidget != null)
				{
					m_ltWidget.IsTrackChanges = true;
				}
				ShiftTrackChangesBalloons(m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Bounds.Y, m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Bounds.Bottom, m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Bounds.Bottom);
			}
		}
		IWSection iWSection = GetBaseEntity(childContext.Widget as Entity) as WSection;
		if (childContext.Widget is WFootnote && (childContext.Widget as WFootnote).FootnoteType == FootnoteType.Footnote && iWSection.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && (iWSection as WSection).IsSectionFitInSamePage)
		{
			(iWSection as WSection).IsSectionFitInSamePage = false;
			m_ltWidget.IsNotFitted = true;
			return;
		}
		if (childContext.Widget is WFootnote && (childContext.Widget as WFootnote).FootnoteType == FootnoteType.Footnote && !(childContext.Widget as WFootnote).IsLayouted && IsNeedToLayoutFootnoteTextBody(childContext))
		{
			bool isTwoLinesLayouted = (m_lcOperator as Layouter).IsTwoLinesLayouted;
			LayoutFootnote(childContext.Widget as WFootnote, m_currChildLW.Owner, isFootnoteRefrencedlineLayouted: false);
			(m_lcOperator as Layouter).IsTwoLinesLayouted = isTwoLinesLayouted;
			if ((m_lcOperator as Layouter).FootnoteWidgets.Count == 0 || m_ltState == LayoutState.NotFitted)
			{
				MarkAsNotFitted(childContext, isFootnote: true);
				(m_lcOperator as Layouter).FootnoteSplittedWidgets.Clear();
				return;
			}
			while ((m_lcOperator as Layouter).FootnoteWidgets.Count > (m_lcOperator as Layouter).FootNoteSectionIndex.Count)
			{
				(m_lcOperator as Layouter).FootNoteSectionIndex.Add(iWSection.Document.Sections.IndexOf(iWSection));
			}
		}
		else if (childContext.Widget is WFootnote && (childContext.Widget as WFootnote).FootnoteType == FootnoteType.Endnote && !(m_lcOperator as Layouter).EndnotesInstances.Contains(childContext.Widget as Entity))
		{
			(m_lcOperator as Layouter).EndnotesInstances.Add(childContext.Widget as Entity);
		}
		bool flag = NextChildWidget();
		if (WidgetContainer is SplitWidgetContainer && (WidgetContainer as SplitWidgetContainer).RealWidgetContainer is WTableCell && !flag)
		{
			m_sptWidget = null;
		}
		ParagraphItem paragraphItem = childContext.Widget as ParagraphItem;
		if ((childContext.LayoutInfo.IsLineBreak && CurrentChildWidget != null) || IsTableNextParagraphNeedToSplit() || (((paragraphItem != null && IsHorizantalRule(CurrentChildWidget) && paragraphItem.OwnerParagraph.HasInlineItem(paragraphItem.Index)) || IsHorizantalRule(childContext.Widget)) && CurrentChildWidget != null))
		{
			SplitedUpWidget(CurrentChildWidget, isEndNoteSplitWidgets: false);
			m_ltState = LayoutState.Splitted;
			m_ltWidget.TextTag = "Splitted";
		}
		else if (childContext.LayoutInfo.IsPageBreakItem && !IsPageBreakInTable(childContext.Widget))
		{
			if (CurrentChildWidget != null)
			{
				SplitedUpWidget(CurrentChildWidget, isEndNoteSplitWidgets: false);
			}
			else
			{
				m_sptWidget = new SplitWidgetContainer(WidgetContainer, childContext.Widget, WidgetContainer.Count - 1);
			}
			if (CurrentChildWidget != null && (CurrentChildWidget as Entity).PreviousSibling is WTable)
			{
				m_ltState = LayoutState.Splitted;
			}
			else
			{
				m_ltState = LayoutState.Breaked;
			}
		}
		else if (!m_bAtLastOneChildFitted)
		{
			m_bAtLastOneChildFitted = true;
		}
		WSection wSection = ((m_sptWidget is WSection) ? (m_sptWidget as WSection) : ((m_sptWidget is SplitWidgetContainer) ? ((m_sptWidget as SplitWidgetContainer).RealWidgetContainer as WSection) : null));
		if (!flag && wSection != null && m_ltState != LayoutState.Splitted && (m_lcOperator as Layouter).FootnoteSplittedWidgets.Count > 0)
		{
			m_sptWidget = new SplitWidgetContainer(WidgetContainer);
			m_ltState = LayoutState.Splitted;
		}
		UpdateForceFitLayoutState(childContext);
	}

	private bool IsHorizantalRule(IWidget currentChildWidget)
	{
		if (currentChildWidget is ParagraphItem && ((currentChildWidget is Shape && (currentChildWidget as Shape).IsHorizontalRule) || (currentChildWidget is WPicture && (currentChildWidget as WPicture).IsShape && (currentChildWidget as WPicture).PictureShape.IsHorizontalRule)))
		{
			return true;
		}
		return false;
	}

	private bool IsNeedToCreateBalloonForBreakCharacterFormat(WParagraph paragraph, ref string breakCFormatBalloonValue)
	{
		bool flag = false;
		if (paragraph != null && paragraph.IsEmptyParagraph())
		{
			WParagraph previousParagraph = GetPreviousParagraph(paragraph);
			if (previousParagraph != null)
			{
				flag = previousParagraph.BreakCharacterFormat.IsChangedFormat;
			}
			else
			{
				WSection ownerSection = paragraph.GetOwnerSection();
				WSection wSection = ((ownerSection != null && ownerSection.Index > 0) ? (ownerSection.PreviousSibling as WSection) : null);
				if (wSection != null)
				{
					previousParagraph = ((wSection.Paragraphs.Count > 0) ? wSection.Paragraphs[wSection.Paragraphs.Count - 1] : null);
					string b = ((previousParagraph != null && previousParagraph.BreakCharacterFormat.IsChangedFormat) ? GetCFormatBallloonValue(previousParagraph.BreakCharacterFormat) : null);
					breakCFormatBalloonValue = GetCFormatBallloonValue(paragraph.BreakCharacterFormat);
					if (!string.IsNullOrEmpty(breakCFormatBalloonValue))
					{
						return !string.Equals(breakCFormatBalloonValue, b, StringComparison.OrdinalIgnoreCase);
					}
					return false;
				}
			}
		}
		else
		{
			for (int num = paragraph.ChildEntities.Count - 1; num >= 0; num--)
			{
				Entity entity = paragraph.ChildEntities[num];
				if (entity is WTextRange || entity is Break)
				{
					flag = (entity is WTextRange && (entity as WTextRange).IsChangedCFormat) || (entity is Break && (entity as Break).CharacterFormat.IsChangedFormat);
					break;
				}
				if (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && !(entity is WFieldMark) && !(entity is EditableRangeStart) && !(entity is EditableRangeEnd))
				{
					break;
				}
			}
		}
		breakCFormatBalloonValue = GetCFormatBallloonValue(paragraph.BreakCharacterFormat);
		if (flag)
		{
			string previousFormattedBalloonValue = GetPreviousFormattedBalloonValue();
			if (!string.IsNullOrEmpty(breakCFormatBalloonValue))
			{
				return !string.Equals("Formatted: " + breakCFormatBalloonValue, previousFormattedBalloonValue, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}
		return !string.IsNullOrEmpty(breakCFormatBalloonValue);
	}

	private string GetCFormatBallloonValue(WCharacterFormat characterFormat)
	{
		TrackChangesMarkups trackChangesMarkups = new TrackChangesMarkups(characterFormat.Document);
		Dictionary<int, object> dictionary = new Dictionary<int, object>(characterFormat.PropertiesHash);
		Dictionary<int, object> standardDic = new Dictionary<int, object>(characterFormat.OldPropertiesHash);
		RemoveSameValues(dictionary, standardDic);
		Dictionary<int, string> hierarchyOrder = new Dictionary<int, string>();
		if (dictionary.Count > 0)
		{
			trackChangesMarkups.DisplayBalloonValueCFormat(FontScriptType.English, dictionary, characterFormat, ref hierarchyOrder);
		}
		dictionary = new Dictionary<int, object>(characterFormat.PropertiesHash);
		standardDic = new Dictionary<int, object>(characterFormat.OldPropertiesHash);
		RemoveSameValues(standardDic, dictionary);
		if (standardDic.Count > 0)
		{
			trackChangesMarkups.DisplayBalloonValueforRemovedCFormat(standardDic, characterFormat, ref hierarchyOrder);
		}
		return trackChangesMarkups.ConvertDictionaryValuesToString(hierarchyOrder);
	}

	private string GetPreviousFormattedBalloonValue()
	{
		for (int num = (m_lcOperator as Layouter).TrackChangesMarkups.Count - 1; num >= 0; num--)
		{
			if ((m_lcOperator as Layouter).TrackChangesMarkups[num].TypeOfMarkup == RevisionType.Formatting && (m_lcOperator as Layouter).TrackChangesMarkups[num].ChangedValue.ChildEntities.Count > 0)
			{
				return (m_lcOperator as Layouter).TrackChangesMarkups[num].ChangedValue.LastParagraph.Text;
			}
		}
		return "";
	}

	private bool HasCommentMark(string commentId)
	{
		foreach (LayoutedWidget childWidget in m_currChildLW.ChildWidgets)
		{
			if (childWidget.Widget is WCommentMark && (childWidget.Widget as WCommentMark).CommentId == commentId && ((childWidget.Widget as WCommentMark).Type == CommentMarkType.CommentEnd || ((childWidget.Widget as WCommentMark).Type == CommentMarkType.CommentStart && (childWidget.Widget as WCommentMark).Comment != null && (childWidget.Widget as WCommentMark).Comment.CommentRangeEnd == null)))
			{
				return true;
			}
		}
		return false;
	}

	protected void ShiftTrackChangesBalloons(float lineYPostion, float bottomPositionWithLineSpacing, float bottomPositionWithoutLineSpacing)
	{
		if ((m_lcOperator as Layouter).IsLayoutingTrackChanges)
		{
			return;
		}
		int num = (m_lcOperator as Layouter).TrackChangesMarkups.Count - 1;
		while (num >= 0 && m_currChildLW.ChildWidgets.Count > 0)
		{
			float num2 = (float)Math.Round((m_lcOperator as Layouter).TrackChangesMarkups[num].Position.Y, 2);
			lineYPostion = (float)Math.Round(lineYPostion, 2);
			bottomPositionWithLineSpacing = (float)Math.Round(bottomPositionWithLineSpacing, 2);
			if (num2 < lineYPostion)
			{
				break;
			}
			if (num2 >= lineYPostion && num2 <= bottomPositionWithLineSpacing)
			{
				if ((m_lcOperator as Layouter).TrackChangesMarkups[num] is CommentsMarkups)
				{
					CommentsMarkups commentsMarkups = (m_lcOperator as Layouter).TrackChangesMarkups[num] as CommentsMarkups;
					if (HasCommentMark(commentsMarkups.CommentID))
					{
						WParagraph paragraph = GetParagraph();
						if (paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Multiple)
						{
							if (bottomPositionWithoutLineSpacing - lineYPostion > 0f)
							{
								commentsMarkups.ExtraSpacing = bottomPositionWithLineSpacing - bottomPositionWithoutLineSpacing;
							}
							else
							{
								float height = paragraph.m_layoutInfo.Size.Height;
								commentsMarkups.ExtraSpacing = bottomPositionWithLineSpacing - (height + lineYPostion);
							}
						}
						(m_lcOperator as Layouter).TrackChangesMarkups[num].Position = new PointF((m_lcOperator as Layouter).TrackChangesMarkups[num].Position.X, bottomPositionWithLineSpacing - 0.125f);
					}
				}
				else
				{
					(m_lcOperator as Layouter).TrackChangesMarkups[num].Position = new PointF((m_lcOperator as Layouter).TrackChangesMarkups[num].Position.X, bottomPositionWithLineSpacing - 0.125f);
				}
			}
			num--;
		}
	}

	private bool IsNeedToLayoutFootnoteTextBody(LayoutContext childContext)
	{
		IWSection iWSection = GetBaseEntity(childContext.Widget as Entity) as WSection;
		if ((iWSection.PageSetup.FootnotePosition != FootnotePosition.PrintAtBottomOfPage || !(childContext.Widget as WFootnote).OwnerParagraph.ParagraphFormat.WidowControl || !(m_lcOperator as Layouter).IsTwoLinesLayouted) && (childContext.Widget as WFootnote).OwnerParagraph.ParagraphFormat.WidowControl)
		{
			return iWSection.PageSetup.FootnotePosition != FootnotePosition.PrintAtBottomOfPage;
		}
		return true;
	}

	private void MarkAsWrapText(LayoutContext childContext)
	{
		AddChildLW(childContext);
		int num = m_curWidgetIndex;
		if (WidgetContainer is SplitWidgetContainer)
		{
			num = WidgetContainer.WidgetInnerCollection.InnerList.IndexOf(childContext.Widget);
		}
		if (childContext.Widget is SplitStringWidget)
		{
			if (num == -1)
			{
				num = (childContext.Widget as SplitStringWidget).m_prevWidgetIndex;
			}
			else
			{
				(childContext.SplittedWidget as SplitStringWidget).m_prevWidgetIndex = num;
			}
		}
		if (num < WidgetContainer.WidgetInnerCollection.InnerList.Count)
		{
			EntityCollection widgetInnerCollection = WidgetContainer.WidgetInnerCollection;
			widgetInnerCollection.InnerList.Insert(num + 1, childContext.SplittedWidget);
			widgetInnerCollection.UpdateIndex(num + 2, isAdd: true);
		}
		else
		{
			WidgetContainer.WidgetInnerCollection.InnerList.Add(childContext.SplittedWidget);
		}
		m_bAtLastOneChildFitted = true;
		NextChildWidget();
	}

	private bool IsTableNextParagraphNeedToSplit()
	{
		if (!(CurrentChildWidget is WParagraph { IsInCell: false } wParagraph))
		{
			return false;
		}
		if (wParagraph.PreviousSibling is WTable wTable)
		{
			TableLayoutInfo tableLayoutInfo = wTable.m_layoutInfo as TableLayoutInfo;
			if (m_currChildLW.ChildWidgets != null && m_currChildLW.ChildWidgets.Count > 0)
			{
				if (m_currChildLW.ChildWidgets[0].Widget is WTableRow wTableRow)
				{
					if (tableLayoutInfo != null)
					{
						if (!wParagraph.IsInCell && !wTableRow.IsHeader && tableLayoutInfo.IsHeaderNotRepeatForAllPages && IsWord2013(wTable.Document))
						{
							return m_currChildLW.Bounds.Height != 0f;
						}
						return false;
					}
					return false;
				}
				return false;
			}
		}
		return false;
	}

	protected virtual void MarkAsSplitted(LayoutContext childContext)
	{
		base.IsVerticalNotFitted = childContext.IsVerticalNotFitted;
		if (base.LayoutInfo is ParagraphLayoutInfo paragraphLayoutInfo)
		{
			paragraphLayoutInfo.IsFirstLine = false;
		}
		AddChildLW(childContext);
		if (childContext is LCLineContainer && m_currChildLW.Widget is WParagraph)
		{
			WParagraph obj = m_currChildLW.Widget as WParagraph;
			ParagraphLayoutInfo paragraphLayoutInfo2 = null;
			if (obj.IsParagraphBeforeSpacingNeedToSkip() && m_currChildLW.Widget.LayoutInfo is ParagraphLayoutInfo paragraphLayoutInfo3)
			{
				paragraphLayoutInfo3.Margins.Top = paragraphLayoutInfo3.TopMargin;
				paragraphLayoutInfo3.Margins.Bottom = paragraphLayoutInfo3.BottomMargin;
			}
		}
		if (!(childContext is LCContainer) || childContext is LCLineContainer || !m_currChildLW.IsNotFitted)
		{
			UpdateSplittedWidgetIndex(childContext);
			SplitedUpWidget(childContext.SplittedWidget, isEndNoteSplitWidgets: false);
			m_ltState = LayoutState.Splitted;
			UpdateForceFitLayoutState(childContext);
			if (childContext.Widget is WTable && (m_lcOperator as Layouter).DynamicTable != null && childContext.Widget as WTable == (m_lcOperator as Layouter).DynamicTable.GetOwnerTable())
			{
				(m_lcOperator as Layouter).DynamicTable = null;
			}
		}
	}

	private void UpdateSplittedWidgetIndex(LayoutContext childContext)
	{
		int num = m_curWidgetIndex;
		if (WidgetContainer is SplitWidgetContainer)
		{
			num = WidgetContainer.WidgetInnerCollection.InnerList.IndexOf(childContext.Widget);
		}
		if (childContext.SplittedWidget is SplitStringWidget)
		{
			if (num != -1)
			{
				(childContext.SplittedWidget as SplitStringWidget).m_prevWidgetIndex = num;
			}
			else if (childContext.Widget is SplitStringWidget)
			{
				(childContext.SplittedWidget as SplitStringWidget).m_prevWidgetIndex = (childContext.Widget as SplitStringWidget).m_prevWidgetIndex + 1;
			}
		}
	}

	protected virtual void MarkAsBreaked(LayoutContext childContext)
	{
		AddChildLW(childContext);
		if (childContext is LCContainer && !(childContext is LCLineContainer) && m_currChildLW.IsNotFitted)
		{
			return;
		}
		LayoutedWidget layoutedWidget = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1];
		if (layoutedWidget != null)
		{
			LayoutedWidget layoutedWidget2 = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
			if (layoutedWidget2 != null && layoutedWidget2.Widget.LayoutInfo.IsPageBreakItem)
			{
				for (int i = 0; i < WidgetContainer.Count; i++)
				{
					if (WidgetContainer[i] == layoutedWidget2.Widget)
					{
						m_curWidgetIndex = i;
						break;
					}
				}
			}
		}
		NextChildWidget();
		if (CurrentChildWidget != null && (!(CurrentChildWidget is BookmarkEnd) || !StartsWithExt((CurrentChildWidget as BookmarkEnd).Name.ToLower(), "_toc") || CurrentChildWidget != WidgetContainer[WidgetContainer.Count - 1]))
		{
			SplitedUpWidget(CurrentChildWidget, isEndNoteSplitWidgets: false);
			m_ltState = LayoutState.Splitted;
			UpdateSplittedWidgetIndex(childContext);
		}
		else
		{
			m_ltState = LayoutState.Breaked;
		}
	}

	protected virtual void UpdateClientArea()
	{
		RectangleF bounds = m_currChildLW.Bounds;
		float num = 0f;
		if ((m_currChildLW.Widget is WPicture || m_currChildLW.Widget is Shape || m_currChildLW.Widget is WChart || m_currChildLW.Widget is GroupShape) && (m_currChildLW.Widget as ParagraphItem).GetTextWrappingStyle() != 0)
		{
			return;
		}
		if (m_currChildLW.Widget is WParagraph)
		{
			if (IsInFrame(m_currChildLW.Widget as WParagraph))
			{
				bool flag = (m_currChildLW.Widget as WParagraph).IsAtleastFrameHeight();
				if (flag && m_currChildLW.Widget.LayoutInfo is ILayoutSpacingsInfo && !(m_lcOperator as Layouter).IsSkipBottomForFrame)
				{
					bounds.Height += (m_currChildLW.Widget.LayoutInfo as ILayoutSpacingsInfo).Paddings.Bottom;
				}
				num = GetFootnoteHeight();
				if (num > 0f)
				{
					m_layoutArea.CutFromTop(bounds.Bottom, num);
				}
				UpdateFrameBounds(bounds, flag);
				return;
			}
			if (m_currChildLW.Widget.LayoutInfo is ILayoutSpacingsInfo)
			{
				bounds.Height += (m_currChildLW.Widget.LayoutInfo as ILayoutSpacingsInfo).Paddings.Bottom;
			}
			m_currChildLW.Bounds = bounds;
			if (((m_currChildLW.Widget as WParagraph).BreakCharacterFormat.Hidden || (m_currChildLW.Widget as WParagraph).BreakCharacterFormat.IsDeleteRevision || (m_lcOperator as Layouter).FieldEntity is WField) && m_currChildLW.TextTag == null)
			{
				WParagraph wParagraph = m_currChildLW.Widget as WParagraph;
				if (((m_currChildLW.Widget as WParagraph).HasNonHiddenPara() && IsNextTextBodyItemIsParagraph(wParagraph)) || (!(m_currChildLW.Widget as WParagraph).BreakCharacterFormat.Hidden && !(m_currChildLW.Widget as WParagraph).BreakCharacterFormat.IsDeleteRevision && wParagraph.NextSibling is WTable))
				{
					num = 0f;
					bool isNeedToUpdateXPosition = false;
					float rightPosition = GetRightPosition(bounds.Right, ref isNeedToUpdateXPosition);
					m_currChildLW.GetFootnoteHeight(ref num);
					if (isNeedToUpdateXPosition)
					{
						m_layoutArea.CutFromLeft(rightPosition);
					}
					if ((m_lcOperator as Layouter).FieldEntity is WField)
					{
						m_layoutArea.CutFromTop(bounds.Y, num);
					}
					if (!((m_lcOperator as Layouter).FieldEntity is WFieldMark))
					{
						(m_lcOperator as Layouter).HiddenLineBottom = m_currChildLW.Bounds.Bottom;
					}
					if ((m_lcOperator as Layouter).FieldEntity != null)
					{
						(m_lcOperator as Layouter).FieldEntity = ((m_lcOperator as Layouter).FieldEntity as WField).FieldEnd;
					}
					if (m_currChildLW.ChildWidgets.Count > 0 && m_currChildLW.ChildWidgets[0].ChildWidgets.Count > 0 && m_currChildLW.ChildWidgets[0].Bounds.Width > 0f)
					{
						return;
					}
				}
			}
			else if (((m_currChildLW.Widget as WParagraph).BreakCharacterFormat.Hidden || (m_currChildLW.Widget as WParagraph).BreakCharacterFormat.IsDeleteRevision || (m_lcOperator as Layouter).FieldEntity is WField) && m_currChildLW.TextTag == "Splitted")
			{
				WParagraph wParagraph2 = m_currChildLW.Widget as WParagraph;
				if (((m_currChildLW.Widget as WParagraph).HasNonHiddenPara() && IsNextTextBodyItemIsParagraph(wParagraph2)) || (!(m_currChildLW.Widget as WParagraph).BreakCharacterFormat.Hidden && !(m_currChildLW.Widget as WParagraph).BreakCharacterFormat.IsDeleteRevision && wParagraph2.NextSibling is WTable))
				{
					m_layoutArea.CutFromLeft(m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Bounds.Right, isSkip: true);
					if (!((m_lcOperator as Layouter).FieldEntity is WFieldMark))
					{
						(m_lcOperator as Layouter).HiddenLineBottom = m_currChildLW.Bounds.Bottom;
					}
					if (m_currChildLW.ChildWidgets.Count > 0 && m_currChildLW.ChildWidgets[0].ChildWidgets.Count > 0 && m_currChildLW.ChildWidgets[0].Bounds.Width > 0f)
					{
						num = 0f;
						m_currChildLW.GetFootnoteHeight(ref num);
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
						m_layoutArea.CutFromTop(bounds.Bottom - m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Bounds.Height, num);
						if ((m_lcOperator as Layouter).FieldEntity != null)
						{
							(m_lcOperator as Layouter).FieldEntity = ((m_lcOperator as Layouter).FieldEntity as WField).FieldEnd;
						}
						(m_lcOperator as Layouter).m_canSplitbyCharacter = true;
						(m_lcOperator as Layouter).m_canSplitByTab = false;
						(m_lcOperator as Layouter).IsFirstItemInLine = true;
						return;
					}
				}
			}
		}
		IEntity prevEntity = null;
		if (m_currChildLW.Widget is WParagraph && ((m_currChildLW.Widget as WParagraph).IsPreviousParagraphMarkIsHidden() || (m_currChildLW.Widget as WParagraph).IsPreviousParagraphMarkIsInDeletion(ref prevEntity) || (m_lcOperator as Layouter).FieldEntity is WFieldMark))
		{
			float pageMarginLeft = GetPageMarginLeft(m_currChildLW.Widget as WParagraph);
			if (pageMarginLeft != m_currChildLW.Bounds.X)
			{
				m_layoutArea.UpdateLeftPosition(pageMarginLeft);
			}
			if ((m_lcOperator as Layouter).HiddenLineBottom != 0f && (m_lcOperator as Layouter).HiddenLineBottom > bounds.Bottom && !((m_lcOperator as Layouter).FieldEntity is WFieldMark))
			{
				bounds.Height = (m_lcOperator as Layouter).HiddenLineBottom - bounds.Y;
			}
			(m_lcOperator as Layouter).HiddenLineBottom = 0f;
			(m_lcOperator as Layouter).FieldEntity = null;
		}
		if (m_currChildLW.Widget is BlockContentControl && ((m_currChildLW.Widget as BlockContentControl).IsHiddenParagraphMarkIsInLastItemOfSDTContent() || (m_currChildLW.Widget as BlockContentControl).IsDeletionParagraphMarkIsInLastItemOfSDTContent()))
		{
			m_layoutArea.CutFromLeft(m_currChildLW.Bounds.Right);
		}
		else
		{
			if (m_currChildLW.Widget is WTextBox && (m_currChildLW.Widget as WTextBox).TextBoxFormat.TextWrappingStyle != 0)
			{
				return;
			}
			if (m_currChildLW.Widget is WTable)
			{
				WTable wTable = m_currChildLW.Widget as WTable;
				if (wTable.TableFormat.WrapTextAround)
				{
					return;
				}
				if (IsInFrame(wTable))
				{
					UpdateFrameBounds(bounds, isAtleastHeight: false);
					return;
				}
			}
			if (m_currChildLW.Widget is BlockContentControl)
			{
				BlockContentControl blockContentControl = m_currChildLW.Widget as BlockContentControl;
				if (blockContentControl.ChildEntities != null && blockContentControl.ChildEntities.Count == 1 && blockContentControl.ChildEntities.FirstItem is WTable && (blockContentControl.ChildEntities.FirstItem as WTable).TableFormat.WrapTextAround)
				{
					return;
				}
			}
			if (m_currChildLW.Widget.LayoutInfo is ILayoutSpacingsInfo && (!(m_currChildLW.Widget is WParagraph) || !(m_currChildLW.Widget as WParagraph).IsNeedToSkip))
			{
				bounds = UpdateSpacingInfo(bounds, m_currChildLW);
			}
			float rightEgeExtent = 0f;
			float topEdgeExtent = 0f;
			float bottomEdgeExtent = 0f;
			if (m_currChildLW.Widget is ParagraphItem)
			{
				(m_currChildLW.Widget as ParagraphItem).GetEffectExtentValues(out var _, out rightEgeExtent, out topEdgeExtent, out bottomEdgeExtent);
			}
			switch (base.LayoutInfo.ChildrenLayoutDirection)
			{
			case ChildrenLayoutDirection.Horizontal:
			{
				float num3 = 0f;
				if (m_currChildLW.Widget is WTextRange)
				{
					num3 = (m_currChildLW.Widget as WTextRange).CharacterFormat.Position;
				}
				else if (m_currChildLW.Widget is WPicture || m_currChildLW.Widget is Shape || m_currChildLW.Widget is WChart)
				{
					num3 = ((m_currChildLW.Widget is Shape) ? (m_currChildLW.Widget as Shape).ParaItemCharFormat.Position : ((m_currChildLW.Widget is WChart) ? (m_currChildLW.Widget as WChart).ParaItemCharFormat.Position : ((m_currChildLW.Widget is GroupShape) ? (m_currChildLW.Widget as GroupShape).ParaItemCharFormat.Position : (m_currChildLW.Widget as WPicture).CharacterFormat.Position)));
				}
				if (bounds.Y + num3 != m_layoutArea.ClientActiveArea.Y && bounds.Y > m_layoutArea.ClientActiveArea.Y && topEdgeExtent + bottomEdgeExtent == 0f)
				{
					m_layoutArea.CutFromTop(bounds.Y);
					if (m_ltWidget.ChildWidgets.IndexOf(m_currChildLW) == 0)
					{
						RectangleF bounds2 = m_ltWidget.Bounds;
						bounds2.Y = m_currChildLW.Bounds.Y;
						m_ltWidget.Bounds = bounds2;
					}
				}
				if ((m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems != float.MinValue && (m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems != float.MaxValue && base.IsTabStopBeyondRightMarginExists)
				{
					float num4 = 0f;
					WParagraph wParagraph4 = ((m_currChildLW.Widget is SplitStringWidget) ? ((m_currChildLW.Widget as SplitStringWidget).RealStringWidget as ParagraphItem) : (m_currChildLW.Widget as ParagraphItem))?.GetOwnerParagraphValue();
					if (wParagraph4 != null)
					{
						ParagraphLayoutInfo paragraphLayoutInfo = ((IWidget)wParagraph4).LayoutInfo as ParagraphLayoutInfo;
						num4 = ((!paragraphLayoutInfo.IsFirstLine) ? paragraphLayoutInfo.Margins.Left : (paragraphLayoutInfo.Margins.Left + paragraphLayoutInfo.FirstLineIndent));
					}
					m_layoutArea.UpdateWidth((m_lcOperator as Layouter).PreviousTabWidth + num4);
					base.IsTabStopBeyondRightMarginExists = false;
				}
				m_layoutArea.CutFromLeft(bounds.Right + rightEgeExtent);
				break;
			}
			case ChildrenLayoutDirection.Vertical:
			{
				num = 0f;
				if (!(m_currChildLW.Widget is WParagraph wParagraph3) || !wParagraph3.IsExactlyRowHeight())
				{
					num = GetFootnoteHeight();
				}
				float num2 = 0f;
				if ((m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems != float.MinValue && (m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems != float.MaxValue)
				{
					m_layoutArea.UpdateLeftPosition((m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems);
					(m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems = float.MaxValue;
					break;
				}
				if ((m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems == float.MaxValue)
				{
					RectangleF clientActiveArea2 = m_layoutArea.ClientActiveArea;
					clientActiveArea2.Width += clientActiveArea2.X - (m_lcOperator as Layouter).ClientLayoutArea.X;
					clientActiveArea2.X = (m_lcOperator as Layouter).ClientLayoutArea.X;
					m_layoutArea.UpdateClientActiveArea(clientActiveArea2);
					(m_lcOperator as Layouter).RightPositionOfTabStopInterSectingFloattingItems = float.MinValue;
				}
				m_layoutArea.CutFromTop(bounds.Bottom + num2, num);
				(m_lcOperator as Layouter).m_canSplitbyCharacter = true;
				(m_lcOperator as Layouter).m_canSplitByTab = false;
				(m_lcOperator as Layouter).IsFirstItemInLine = true;
				break;
			}
			}
		}
	}

	private RectangleF UpdateSpacingInfo(RectangleF bounds, LayoutedWidget layoutedWidget)
	{
		ILayoutSpacingsInfo layoutSpacingsInfo = layoutedWidget.Widget.LayoutInfo as ILayoutSpacingsInfo;
		bounds.X -= layoutSpacingsInfo.Margins.Left;
		bounds.Y -= layoutSpacingsInfo.Margins.Top;
		bounds.Width += layoutSpacingsInfo.Margins.Left + layoutSpacingsInfo.Margins.Right;
		bounds.Height += layoutSpacingsInfo.Margins.Top + layoutSpacingsInfo.Margins.Bottom;
		if (layoutSpacingsInfo is ParagraphLayoutInfo && !(layoutSpacingsInfo as ParagraphLayoutInfo).IsFirstLine)
		{
			bounds.Y += layoutSpacingsInfo.Margins.Top;
			bounds.Height -= layoutSpacingsInfo.Margins.Top;
		}
		if (layoutedWidget.Widget is WParagraph || (layoutedWidget.Widget is SplitWidgetContainer && (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph))
		{
			WParagraph wParagraph2 = ((!(layoutedWidget.Widget is WParagraph wParagraph)) ? ((layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : wParagraph);
			LayoutedWidgetList layoutedWidgetList = null;
			if (layoutedWidget.ChildWidgets.Count > 0)
			{
				layoutedWidgetList = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].ChildWidgets;
			}
			float rowHeight = 0f;
			if (wParagraph2 != null && layoutedWidgetList != null && layoutedWidgetList.Count > 0 && (wParagraph2.IsLastLine(layoutedWidgetList[layoutedWidgetList.Count - 1]) || layoutedWidget.TextTag == "Splitted") && wParagraph2.IsInCell && !wParagraph2.IsExactlyRowHeight(wParagraph2.GetOwnerEntity() as WTableCell, ref rowHeight))
			{
				bounds.Height -= layoutSpacingsInfo.Margins.Bottom;
			}
		}
		return bounds;
	}

	private bool IsNextTextBodyItemIsParagraph(WParagraph paragraph)
	{
		if (paragraph != null && (paragraph.NextSibling is WParagraph || (paragraph.OwnerTextBody != null && paragraph.OwnerTextBody.Owner != null && paragraph.OwnerTextBody.Owner is BlockContentControl && (paragraph.OwnerTextBody.Owner as BlockContentControl).NextSibling is WParagraph)))
		{
			return true;
		}
		return false;
	}

	private bool IsInFrame(WTable table)
	{
		bool flag = true;
		if (table.IsInCell)
		{
			flag = false;
		}
		if (table.IsFrame && flag)
		{
			return true;
		}
		return false;
	}

	private void UpdateFrameBounds(RectangleF bounds, bool isAtleastHeight)
	{
		m_currChildLW.Bounds = bounds;
		if (m_currChildLW.Widget.LayoutInfo is ILayoutSpacingsInfo)
		{
			bounds = UpdateSpacingInfo(bounds, m_currChildLW);
		}
		float num = bounds.Bottom;
		if (num < (m_lcOperator as Layouter).FrameLayoutArea.Top)
		{
			num = (m_lcOperator as Layouter).FrameLayoutArea.Top;
		}
		else if (num > (m_lcOperator as Layouter).FrameLayoutArea.Bottom)
		{
			num = (m_lcOperator as Layouter).FrameLayoutArea.Bottom;
		}
		RectangleF frameLayoutArea = (m_lcOperator as Layouter).FrameLayoutArea;
		if (isAtleastHeight)
		{
			(m_lcOperator as Layouter).FrameHeight -= bounds.Height;
		}
		frameLayoutArea.Height = frameLayoutArea.Bottom - num;
		frameLayoutArea.Y = num;
		float num2 = frameLayoutArea.Y - m_currChildLW.Bounds.Bottom;
		bounds = m_currChildLW.Bounds;
		bounds.Height += num2;
		m_currChildLW.Bounds = bounds;
		(m_lcOperator as Layouter).FrameLayoutArea = frameLayoutArea;
	}

	protected virtual void ChangeChildsAlignment()
	{
	}

	protected bool NextChildWidget()
	{
		if (m_curWidgetIndex > -1 && m_curWidgetIndex < WidgetContainer.Count - 1)
		{
			m_curWidgetIndex++;
			return true;
		}
		m_curWidgetIndex = -1;
		return false;
	}

	internal void SplitedUpWidget(IWidget splitWidget, bool isEndNoteSplitWidgets)
	{
		SplitedUpWidget(splitWidget, isEndNoteSplitWidgets, IsInFrame: false);
	}

	internal void SplitedUpWidget(IWidget splitWidget, bool isEndNoteSplitWidgets, bool IsInFrame)
	{
		int firstIndex = m_curWidgetIndex;
		if (isEndNoteSplitWidgets && m_curWidgetIndex < 0)
		{
			firstIndex = 0;
		}
		if (IsInFrame)
		{
			firstIndex = ((splitWidget is WParagraph) ? (splitWidget as WParagraph).Index : (splitWidget as WTable).Index);
		}
		m_sptWidget = new SplitWidgetContainer(WidgetContainer, splitWidget, firstIndex);
	}

	protected void SaveChildContextState(LayoutContext childContext)
	{
		switch (childContext.State)
		{
		case LayoutState.Unknown:
			m_ltState = LayoutState.Unknown;
			break;
		case LayoutState.Fitted:
			MarkAsFitted(childContext);
			break;
		case LayoutState.NotFitted:
			m_ltWidget.IsNotFitted = false;
			MarkAsNotFitted(childContext, isFootnote: false);
			m_ltWidget.TextTag = "Splitted";
			break;
		case LayoutState.Splitted:
			m_ltWidget.TextTag = "Splitted";
			MarkAsSplitted(childContext);
			break;
		case LayoutState.Breaked:
			MarkAsBreaked(childContext);
			break;
		case LayoutState.WrapText:
			MarkAsWrapText(childContext);
			break;
		case LayoutState.DynamicRelayout:
			AddChildLW(childContext);
			break;
		}
	}

	private float GetTableHeight(WTable table)
	{
		float num = 0f;
		foreach (WTableRow row in table.Rows)
		{
			num += row.Height;
		}
		return num;
	}

	protected virtual void DoLayoutChild(LayoutContext childContext)
	{
		if (base.IsTabStopBeyondRightMarginExists && !base.IsAreaUpdated)
		{
			UpdateAreaWidth(0f);
			base.IsAreaUpdated = true;
		}
		RectangleF rectangleF = m_layoutArea.ClientActiveArea;
		bool flag = childContext.Widget is WTable;
		if ((childContext.Widget is WParagraph && IsInFrame(childContext.Widget as WParagraph)) || (flag && !(childContext.Widget as WTable).TableFormat.WrapTextAround && IsInFrame(childContext.Widget as WTable)))
		{
			WParagraph wParagraph = childContext.Widget as WParagraph;
			float num = 0f;
			if (wParagraph == null && flag)
			{
				wParagraph = (childContext.Widget as WTable).Rows[0].Cells[0].Paragraphs[0];
				num = GetTableHeight(childContext.Widget as WTable);
			}
			if (wParagraph != null && !base.IsForceFitLayout && !DocumentLayouter.IsLayoutingHeaderFooter)
			{
				WParagraphFormat paragraphFormat = wParagraph.ParagraphFormat;
				float num2 = ((ushort)(paragraphFormat.FrameHeight * 20f) & 0x7FFF) / 20;
				_ = ((ushort)(paragraphFormat.FrameWidth * 20f) & 0x7FFF) / 20;
				if (paragraphFormat.FrameVerticalAnchor == 2 && ((wParagraph.GetOwnerSection().Columns.Count == 1 && num2 > rectangleF.Height) || (flag && num > rectangleF.Height)))
				{
					childContext.m_ltState = LayoutState.NotFitted;
					m_currChildLW = null;
					return;
				}
			}
			rectangleF = (m_lcOperator as Layouter).FrameLayoutArea;
			childContext.ClientLayoutAreaRight = (m_lcOperator as Layouter).FrameLayoutArea.Width;
		}
		if (childContext is LeafLayoutContext)
		{
			ParagraphItem paragraphItem = ((childContext.Widget is SplitStringWidget) ? ((childContext.Widget as SplitStringWidget).RealStringWidget as ParagraphItem) : (childContext.Widget as ParagraphItem));
			WParagraph ownerpara = paragraphItem?.GetOwnerParagraphValue();
			bool flag2 = HasTextInRange(childContext);
			if ((!(childContext.Widget is Break) || ((childContext.Widget as Break).BreakType != BreakType.LineBreak && (childContext.Widget as Break).BreakType != BreakType.TextWrappingBreak) || !((childContext.Widget as Break).Owner.Owner is WTableCell)) && !(paragraphItem is BookmarkStart) && !(paragraphItem is BookmarkEnd) && (IsSplitLine(rectangleF, ownerpara, flag2) || (m_ltWidget.ChildWidgets.Count > 0 && m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Widget is SplitStringWidget && (m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Widget as SplitStringWidget).IsTrailSpacesWrapped && (!(childContext.Widget is Break) || ((childContext.Widget as Break).BreakType != BreakType.LineBreak && (childContext.Widget as Break).BreakType != BreakType.TextWrappingBreak)) && (!(childContext.Widget is Entity) || !(childContext.Widget as Entity).IsFloatingItem(isTextWrapAround: false)) && flag2)))
			{
				childContext.m_ltState = LayoutState.NotFitted;
				m_currChildLW = null;
			}
			else
			{
				float characterPositionOfFloatingitem = GetCharacterPositionOfFloatingitem(childContext);
				rectangleF.X -= characterPositionOfFloatingitem;
				m_currChildLW = childContext.Layout(rectangleF);
				rectangleF.X += characterPositionOfFloatingitem;
			}
		}
		else
		{
			m_currChildLW = childContext.Layout(rectangleF);
		}
		UpdateWrappingDifferenceValue(m_currChildLW);
	}

	private float GetCharacterPositionOfFloatingitem(LayoutContext childContext)
	{
		float result = 0f;
		IWidget widget = ((m_currChildLW != null) ? m_currChildLW.Widget : null);
		IWidget widget2 = childContext.Widget;
		if (widget != null && widget2 is Entity && (widget2 as Entity).IsFloatingItem(isTextWrapAround: false) && !(widget2 as Entity).IsFallbackItem())
		{
			ParagraphItem obj = ((widget2 is WOleObject) ? (widget2 as WOleObject).OlePicture : (widget2 as ParagraphItem));
			HorizontalOrigin horizontalOrigin = obj.GetHorizontalOrigin();
			ShapeHorizontalAlignment shapeHorizontalAlignment = obj.GetShapeHorizontalAlignment();
			if (horizontalOrigin == HorizontalOrigin.Character && shapeHorizontalAlignment == ShapeHorizontalAlignment.None)
			{
				if (widget is WTextRange)
				{
					WTextRange wTextRange = widget as WTextRange;
					if (wTextRange.Text.Length > 0)
					{
						char c = wTextRange.Text[wTextRange.Text.Length - 1];
						result = base.DrawingContext.MeasureString(c.ToString(), wTextRange.CharacterFormat.GetFontToRender(wTextRange.ScriptType), null, wTextRange.ScriptType).Width;
					}
					else if (wTextRange.m_layoutInfo is TabsLayoutInfo)
					{
						result = wTextRange.m_layoutInfo.Size.Width;
					}
				}
				else if (widget is WSymbol)
				{
					result = m_currChildLW.Bounds.Width;
				}
			}
		}
		return result;
	}

	private bool IsSplitLine(RectangleF clientArea, WParagraph ownerpara, bool hasTextInRange)
	{
		if (clientArea.X > m_layoutArea.ClientArea.Right && ownerpara != null && (hasTextInRange || (m_ltWidget.ChildWidgets.Count > 0 && !(m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Widget is WTextRange))))
		{
			if (ownerpara.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
			{
				if (!ownerpara.IsInCell)
				{
					return true;
				}
				return !(ownerpara.GetOwnerEntity() as WTableCell).OwnerRow.OwnerTable.TableFormat.IsAutoResized;
			}
			return false;
		}
		return false;
	}

	private bool HasTextInRange(LayoutContext childContext)
	{
		if (childContext.Widget is WTextRange && !(childContext.Widget is WField) && (childContext.Widget as WTextRange).Text.TrimEnd(ControlChar.SpaceChar) == string.Empty)
		{
			return childContext.Widget.LayoutInfo is TabsLayoutInfo;
		}
		return true;
	}

	private bool IsNeedToUpdateFloatingEntityBounds(Entity entity)
	{
		if (entity == null)
		{
			return false;
		}
		TextWrappingStyle textWrappingStyle = TextWrappingStyle.Inline;
		switch (entity.EntityType)
		{
		case EntityType.Picture:
			textWrappingStyle = (entity as WPicture).TextWrappingStyle;
			break;
		case EntityType.Shape:
		case EntityType.AutoShape:
			if (entity is Shape)
			{
				textWrappingStyle = (entity as Shape).WrapFormat.TextWrappingStyle;
			}
			break;
		case EntityType.GroupShape:
			if (entity is GroupShape)
			{
				textWrappingStyle = (entity as GroupShape).WrapFormat.TextWrappingStyle;
			}
			break;
		case EntityType.TextBox:
			textWrappingStyle = (entity as WTextBox).TextBoxFormat.TextWrappingStyle;
			break;
		case EntityType.Chart:
			textWrappingStyle = (entity as WChart).WrapFormat.TextWrappingStyle;
			break;
		}
		if (textWrappingStyle != TextWrappingStyle.Tight)
		{
			return textWrappingStyle == TextWrappingStyle.Through;
		}
		return true;
	}

	private FloatingItem GetFloatingItemFromCollection(Entity entity)
	{
		for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
		{
			if ((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity == entity)
			{
				return (m_lcOperator as Layouter).FloatingItems[i];
			}
		}
		return null;
	}

	protected void AddChildLW(LayoutContext childContext)
	{
		WParagraph wParagraph = null;
		if (DocumentLayouter.IsLayoutingHeaderFooter)
		{
			wParagraph = ((m_currChildLW.Widget is WParagraph) ? (m_currChildLW.Widget as WParagraph) : null);
		}
		if ((!m_currChildLW.Widget.LayoutInfo.IsSkip && (wParagraph == null || !wParagraph.IsNeedToSkip)) || m_currChildLW.ChildWidgets.Count != 0)
		{
			if (IsNeedToUpdateFloatingEntityBounds(m_currChildLW.Widget as Entity))
			{
				FloatingItem floatingItemFromCollection = GetFloatingItemFromCollection(m_currChildLW.Widget as Entity);
				if (floatingItemFromCollection != null && !floatingItemFromCollection.IsDoesNotDenotesRectangle)
				{
					SizeF sizeF = (m_currChildLW.Widget as ILeafWidget).Measure(base.DrawingContext);
					m_currChildLW.Bounds = new RectangleF(m_currChildLW.Bounds.X, m_currChildLW.Bounds.Y, sizeF.Width, sizeF.Height);
					m_currChildLW.Bounds = ResetAdjustedboundsBasedOnWrapPolygon(floatingItemFromCollection, m_currChildLW.Bounds);
				}
			}
			m_ltWidget.ChildWidgets.Add(m_currChildLW);
			if (childContext is LeafLayoutContext && (m_currChildLW.Bounds.Width > 0f || (m_currChildLW.Widget is WTextRange && (m_currChildLW.Widget as WTextRange).m_layoutInfo is TabsLayoutInfo)) && (m_lcOperator as Layouter).IsFirstItemInLine)
			{
				if (!(m_currChildLW.Widget is Entity entity))
				{
					(m_lcOperator as Layouter).IsFirstItemInLine = false;
				}
				else if (!entity.IsFloatingItem(isTextWrapAround: false))
				{
					(m_lcOperator as Layouter).IsFirstItemInLine = false;
				}
			}
		}
		if (m_ltWidget.Widget is WSection || (m_ltWidget.Widget is SplitWidgetContainer && (m_ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WSection))
		{
			(m_lcOperator as Layouter).CountForConsecutiveLimit = 0;
		}
		if (m_currChildLW.IsBehindWidget())
		{
			AddBehindWidgets(m_currChildLW);
		}
		if (childContext.State == LayoutState.DynamicRelayout)
		{
			IWidget widget = childContext.SplittedWidget;
			if (childContext is LCLineContainer && widget is SplitWidgetContainer && (widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)
			{
				if ((m_lcOperator as Layouter).NotFittedFloatingItems.Count > 0)
				{
					if (childContext.Widget is SplitWidgetContainer)
					{
						base.SplittedWidget = (childContext.Widget as SplitWidgetContainer).m_currentChild;
					}
					else
					{
						widget = (childContext.Widget as WParagraph).ChildEntities[0] as IWidget;
					}
				}
				else
				{
					widget = (widget as SplitWidgetContainer).m_currentChild;
				}
			}
			SplitedUpWidget(widget, isEndNoteSplitWidgets: false);
			m_ltState = LayoutState.DynamicRelayout;
			m_currChildLW.Owner = m_ltWidget;
			return;
		}
		if (childContext.Widget is ParagraphItem || childContext.Widget is SplitStringWidget)
		{
			ParagraphItem paragraphItem = ((childContext.Widget is ParagraphItem) ? (childContext.Widget as ParagraphItem) : ((childContext.Widget as SplitStringWidget).RealStringWidget as ParagraphItem));
			if ((paragraphItem.IsInsertRevision || paragraphItem.IsChangedCFormat) && m_ltWidget != null)
			{
				m_ltWidget.IsTrackChanges = true;
			}
		}
		if (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter && childContext.Widget is WPicture && (childContext.Widget as WPicture).IsWrappingBoundsAdded && (childContext.Widget as WPicture).WrapCollectionIndex >= 0 && (childContext.Widget as WPicture).WrapCollectionIndex < (m_lcOperator as Layouter).FloatingItems.Count)
		{
			(m_lcOperator as Layouter).FloatingItems[(childContext.Widget as WPicture).WrapCollectionIndex].IsFloatingItemFit = true;
		}
		if (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter && childContext.Widget is Shape && (childContext.Widget as Shape).WrapFormat.IsWrappingBoundsAdded && (childContext.Widget as Shape).WrapFormat.WrapCollectionIndex >= 0 && (childContext.Widget as Shape).WrapFormat.WrapCollectionIndex < (m_lcOperator as Layouter).FloatingItems.Count)
		{
			(m_lcOperator as Layouter).FloatingItems[(childContext.Widget as Shape).WrapFormat.WrapCollectionIndex].IsFloatingItemFit = true;
		}
		if (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter && childContext.Widget is WTextBox && (childContext.Widget as WTextBox).TextBoxFormat.IsWrappingBoundsAdded && (childContext.Widget as WTextBox).TextBoxFormat.WrapCollectionIndex >= 0 && (childContext.Widget as WTextBox).TextBoxFormat.WrapCollectionIndex < (m_lcOperator as Layouter).FloatingItems.Count)
		{
			(m_lcOperator as Layouter).FloatingItems[(childContext.Widget as WTextBox).TextBoxFormat.WrapCollectionIndex].IsFloatingItemFit = true;
		}
		if (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter && childContext.Widget is WChart && (childContext.Widget as WChart).WrapFormat.IsWrappingBoundsAdded && (childContext.Widget as WChart).WrapFormat.WrapCollectionIndex >= 0 && (childContext.Widget as WChart).WrapFormat.WrapCollectionIndex < (m_lcOperator as Layouter).FloatingItems.Count)
		{
			(m_lcOperator as Layouter).FloatingItems[(childContext.Widget as WChart).WrapFormat.WrapCollectionIndex].IsFloatingItemFit = true;
		}
		if (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter && childContext.Widget is GroupShape && (childContext.Widget as GroupShape).WrapFormat.IsWrappingBoundsAdded && (childContext.Widget as GroupShape).WrapFormat.WrapCollectionIndex >= 0 && (childContext.Widget as GroupShape).WrapFormat.WrapCollectionIndex < (m_lcOperator as Layouter).FloatingItems.Count)
		{
			(m_lcOperator as Layouter).FloatingItems[(childContext.Widget as GroupShape).WrapFormat.WrapCollectionIndex].IsFloatingItemFit = true;
		}
		if (m_currChildLW.Widget is ParagraphItem || m_currChildLW.Widget is SplitStringWidget)
		{
			WParagraph wParagraph2 = null;
			wParagraph2 = ((!(m_currChildLW.Widget is ParagraphItem)) ? ((m_currChildLW.Widget as SplitStringWidget).RealStringWidget as ParagraphItem).OwnerParagraph : (m_currChildLW.Widget as ParagraphItem).OwnerParagraph);
			if (wParagraph2 != null && (((IWidget)wParagraph2).LayoutInfo as ParagraphLayoutInfo).XPosition > (m_lcOperator as Layouter).ClientLayoutArea.Left && (m_lcOperator as Layouter).ClientLayoutArea.Left == m_currChildLW.Bounds.X && !(childContext.Widget is BookmarkStart) && !(childContext.Widget is BookmarkEnd))
			{
				wParagraph2.IsXpositionUpated = true;
				(((IWidget)wParagraph2).LayoutInfo as ParagraphLayoutInfo).IsXPositionReUpdate = true;
			}
		}
		if (m_ltWidget.Widget is WParagraph && (m_ltWidget.Widget as WParagraph).Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && m_ltWidget.ChildWidgets.Count > 2 && m_currChildLW.Widget is WTextRange && !(m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 2].Widget is WTextRange))
		{
			for (int i = 0; i < m_ltWidget.ChildWidgets.Count - 1; i++)
			{
				if (!CheckWidgetWrappingType(m_ltWidget.ChildWidgets[i].Widget) || !(m_currChildLW.Bounds.X >= m_ltWidget.ChildWidgets[i].Bounds.Right + ((m_ltWidget.ChildWidgets[i].Widget is WPicture) ? (m_ltWidget.ChildWidgets[i].Widget as WPicture).DistanceFromRight : ((m_ltWidget.ChildWidgets[i].Widget is WTextBox) ? (m_ltWidget.ChildWidgets[i].Widget as WTextBox).TextBoxFormat.WrapDistanceRight : ((m_ltWidget.ChildWidgets[i].Widget is Shape) ? (m_ltWidget.ChildWidgets[i].Widget as Shape).WrapFormat.DistanceRight : ((m_ltWidget.ChildWidgets[i].Widget is GroupShape) ? (m_ltWidget.ChildWidgets[i].Widget as GroupShape).WrapFormat.DistanceRight : 0f))))))
				{
					continue;
				}
				for (int j = 0; j < m_ltWidget.ChildWidgets.Count - 1; j++)
				{
					if (CheckWidgetWrappingTypeAndHorizontalOrigin(m_ltWidget.ChildWidgets[j].Widget))
					{
						float num = m_ltWidget.ChildWidgets[i].Bounds.Right;
						float num2 = 0f;
						ShapeHorizontalAlignment shapeHorizontalAlignment = ShapeHorizontalAlignment.None;
						switch ((m_ltWidget.ChildWidgets[i].Widget as Entity).EntityType)
						{
						case EntityType.Picture:
							num += (m_ltWidget.ChildWidgets[i].Widget as WPicture).DistanceFromRight;
							break;
						case EntityType.Shape:
						case EntityType.AutoShape:
							num += (m_ltWidget.ChildWidgets[i].Widget as Shape).WrapFormat.DistanceRight;
							break;
						case EntityType.TextBox:
							num += (m_ltWidget.ChildWidgets[i].Widget as WTextBox).TextBoxFormat.WrapDistanceRight;
							break;
						case EntityType.Chart:
							num += (m_ltWidget.ChildWidgets[i].Widget as WChart).WrapFormat.DistanceRight;
							break;
						case EntityType.GroupShape:
							num += (m_ltWidget.ChildWidgets[i].Widget as GroupShape).WrapFormat.DistanceRight;
							break;
						}
						switch ((m_ltWidget.ChildWidgets[j].Widget as Entity).EntityType)
						{
						case EntityType.Picture:
							num2 = (m_ltWidget.ChildWidgets[j].Widget as WPicture).HorizontalPosition;
							shapeHorizontalAlignment = (m_ltWidget.ChildWidgets[j].Widget as WPicture).HorizontalAlignment;
							break;
						case EntityType.Shape:
						case EntityType.AutoShape:
							num2 = (m_ltWidget.ChildWidgets[j].Widget as Shape).HorizontalPosition;
							shapeHorizontalAlignment = (m_ltWidget.ChildWidgets[j].Widget as Shape).HorizontalAlignment;
							break;
						case EntityType.TextBox:
							num2 = (m_ltWidget.ChildWidgets[j].Widget as WTextBox).TextBoxFormat.HorizontalPosition;
							shapeHorizontalAlignment = (m_ltWidget.ChildWidgets[j].Widget as WTextBox).TextBoxFormat.HorizontalAlignment;
							break;
						case EntityType.Chart:
							num2 = (m_ltWidget.ChildWidgets[j].Widget as WChart).HorizontalPosition;
							shapeHorizontalAlignment = (m_ltWidget.ChildWidgets[j].Widget as WChart).HorizontalAlignment;
							break;
						case EntityType.GroupShape:
							num2 = (m_ltWidget.ChildWidgets[j].Widget as GroupShape).HorizontalPosition;
							shapeHorizontalAlignment = (m_ltWidget.ChildWidgets[j].Widget as GroupShape).HorizontalAlignment;
							break;
						}
						switch (shapeHorizontalAlignment)
						{
						case ShapeHorizontalAlignment.Center:
							num2 = (m_lcOperator as Layouter).ClientLayoutArea.Left + ((m_lcOperator as Layouter).ClientLayoutArea.Width - m_ltWidget.ChildWidgets[j].Bounds.Width) / 2f;
							break;
						case ShapeHorizontalAlignment.Left:
							num2 = (m_lcOperator as Layouter).ClientLayoutArea.Left;
							break;
						case ShapeHorizontalAlignment.Right:
							num2 = (m_lcOperator as Layouter).ClientLayoutArea.Left + (m_lcOperator as Layouter).ClientLayoutArea.Width - m_ltWidget.ChildWidgets[j].Bounds.Width;
							break;
						}
						m_ltWidget.ChildWidgets[j].Bounds = new RectangleF(num + num2, m_ltWidget.ChildWidgets[j].Bounds.Y, m_ltWidget.ChildWidgets[j].Bounds.Width, m_ltWidget.ChildWidgets[j].Bounds.Height);
					}
				}
				break;
			}
		}
		if (childContext is LCLineContainer)
		{
			UpdateParagraphYPosition(m_ltWidget, childContext);
		}
		m_currChildLW.Owner = m_ltWidget;
		UpdateClientArea();
		if (childContext is LCLineContainer && (m_lcOperator as Layouter).UnknownField != null && (m_lcOperator as Layouter).UnknownField.OwnerParagraph.m_layoutInfo is ParagraphLayoutInfo && m_currChildLW.Widget is WParagraph && (m_currChildLW.Widget as WParagraph).Items.Contains((m_lcOperator as Layouter).UnknownField.FieldEnd))
		{
			(m_lcOperator as Layouter).UnknownField = null;
		}
		if (childContext is LCContainer && !(childContext is LCLineContainer) && m_currChildLW.IsNotFitted && m_ltWidget.ChildWidgets.Contains(m_currChildLW))
		{
			if (childContext.SplittedWidget is SplitWidgetContainer && m_currChildLW.Widget is SplitWidgetContainer && ((childContext.SplittedWidget as SplitWidgetContainer).m_currentChild is SplitStringWidget || (childContext.SplittedWidget as SplitWidgetContainer).m_currentChild is WTextRange) && ((m_currChildLW.Widget as SplitWidgetContainer).m_currentChild is SplitStringWidget || (m_currChildLW.Widget as SplitWidgetContainer).m_currentChild is WTextRange) && (childContext.SplittedWidget as SplitWidgetContainer).m_currentChild != (m_currChildLW.Widget as SplitWidgetContainer).m_currentChild)
			{
				childContext.SplittedWidget = m_currChildLW.Widget;
			}
			m_ltWidget.ChildWidgets.Remove(m_currChildLW);
			childContext.m_ltState = LayoutState.NotFitted;
			return;
		}
		UpdateLWBounds(childContext);
		WordDocument obj = ((m_ltWidget.Widget is WordDocument) ? (m_ltWidget.Widget as WordDocument) : ((m_ltWidget.Widget is SplitWidgetContainer) ? ((m_ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WordDocument) : null));
		IWSection iWSection = ((m_currChildLW.Widget is WSection) ? (m_currChildLW.Widget as WSection) : ((m_currChildLW.Widget is SplitWidgetContainer) ? ((m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer as WSection) : null));
		if (obj != null && iWSection != null && childContext.State != LayoutState.Splitted)
		{
			if ((m_lcOperator as Layouter).EndnotesInstances.Count > 0)
			{
				if (iWSection.Document.EndnotePosition == EndnotePosition.DisplayEndOfDocument && iWSection.Document.Sections.IndexOf(iWSection) == iWSection.Document.Sections.Count - 1)
				{
					if ((m_lcOperator as Layouter).EndnotesInstances.Count > 0)
					{
						for (int k = 0; k < (m_lcOperator as Layouter).EndnotesInstances.Count; k++)
						{
							if (!((m_lcOperator as Layouter).EndnotesInstances[k] as WFootnote).IsLayouted)
							{
								LayoutEndnote((m_lcOperator as Layouter).EndnotesInstances[k] as WFootnote, m_currChildLW.Owner);
							}
							((m_lcOperator as Layouter).EndnotesInstances[k] as WFootnote).IsLayouted = true;
						}
						(m_lcOperator as Layouter).EndnotesInstances.Clear();
					}
					if ((m_lcOperator as Layouter).EndnoteWidgets.Count == 0)
					{
						MarkAsNotFitted(childContext, isFootnote: true);
						(m_lcOperator as Layouter).EndnoteWidgets.Clear();
						(m_lcOperator as Layouter).EndNoteSectionIndex.Clear();
						SplitEndNoteWidgets();
						return;
					}
				}
				else if (iWSection.Document.EndnotePosition == EndnotePosition.DisplayEndOfSection)
				{
					if ((m_lcOperator as Layouter).EndnotesInstances.Count > 0)
					{
						for (int l = 0; l < (m_lcOperator as Layouter).EndnotesInstances.Count; l++)
						{
							if (!((m_lcOperator as Layouter).EndnotesInstances[l] as WFootnote).IsLayouted)
							{
								LayoutEndnote((m_lcOperator as Layouter).EndnotesInstances[l] as WFootnote, m_currChildLW.Owner);
							}
							((m_lcOperator as Layouter).EndnotesInstances[l] as WFootnote).IsLayouted = true;
						}
						(m_lcOperator as Layouter).EndnotesInstances.Clear();
					}
					if ((m_lcOperator as Layouter).EndnoteWidgets.Count == 0)
					{
						MarkAsNotFitted(childContext, isFootnote: true);
						(m_lcOperator as Layouter).EndnoteSplittedWidgets.Clear();
						return;
					}
				}
				while ((m_lcOperator as Layouter).EndnoteWidgets.Count > (m_lcOperator as Layouter).EndNoteSectionIndex.Count)
				{
					(m_lcOperator as Layouter).EndNoteSectionIndex.Add(iWSection.Document.Sections.IndexOf(iWSection));
				}
			}
			SplitEndNoteWidgets();
		}
		if (!(childContext is LCContainer) || childContext is LCLineContainer)
		{
			return;
		}
		WParagraph wParagraph3 = ((m_ltWidget.Widget is WParagraph) ? (m_ltWidget.Widget as WParagraph) : ((m_ltWidget.Widget is SplitWidgetContainer) ? ((m_ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null));
		int num3;
		if (wParagraph3 == null || wParagraph3.GetOwnerSection() == null)
		{
			num3 = 0;
		}
		else
		{
			num3 = ((wParagraph3.GetOwnerSection().PageSetup.FootnotePosition == FootnotePosition.PrintAtBottomOfPage) ? 1 : 0);
			if (num3 != 0 && m_ltWidget.ChildWidgets.Count == 1 && m_currChildLW.ChildWidgets.Count > 0 && wParagraph3 != null && wParagraph3.ParagraphFormat.WidowControl && (wParagraph3.IsLastLine(m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1]) || (m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Widget is Break && (m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].Widget as Break).BreakType == BreakType.PageBreak)))
			{
				LayoutFootnoteOfLayoutedLines(m_ltWidget);
			}
		}
		if (num3 != 0 && m_ltWidget.ChildWidgets.Count == 2 && wParagraph3 != null && wParagraph3.ParagraphFormat.WidowControl && m_currChildLW.ChildWidgets.Count > 0)
		{
			LayoutFootnoteOfLayoutedLines(m_ltWidget);
			(m_lcOperator as Layouter).IsTwoLinesLayouted = true;
		}
		if (m_ltState == LayoutState.NotFitted)
		{
			(m_lcOperator as Layouter).IsTwoLinesLayouted = false;
			(m_lcOperator as Layouter).IsFootnoteHeightAdjusted = false;
			m_currChildLW.IsNotFitted = true;
		}
	}

	private void LayoutFootnoteOfLayoutedLines(LayoutedWidget layoutedWidget)
	{
		IWSection iWSection = null;
		bool flag = false;
		float num = 0f;
		for (int i = 0; i < layoutedWidget.ChildWidgets.Count; i++)
		{
			for (int j = 0; j < layoutedWidget.ChildWidgets[i].ChildWidgets.Count; j++)
			{
				if (!(layoutedWidget.ChildWidgets[i].ChildWidgets[j].Widget is WFootnote { IsLayouted: false, FootnoteType: not FootnoteType.Endnote } wFootnote))
				{
					continue;
				}
				flag = true;
				bool isTwoLinesLayouted = (m_lcOperator as Layouter).IsTwoLinesLayouted;
				LayoutContext layoutContext = LayoutContext.Create(layoutedWidget.ChildWidgets[i].ChildWidgets[j].Widget, m_lcOperator, base.IsForceFitLayout);
				iWSection = GetBaseEntity(layoutContext.Widget as Entity) as WSection;
				LayoutFootnote(wFootnote, layoutedWidget.ChildWidgets[i], isFootnoteRefrencedlineLayouted: true);
				(m_lcOperator as Layouter).IsTwoLinesLayouted = isTwoLinesLayouted;
				if ((m_lcOperator as Layouter).FootnoteWidgets.Count == 0 && layoutedWidget.Widget is WParagraph)
				{
					if (this is LCLineContainer && (this as LCLineContainer).IsFirstItemInPage)
					{
						m_sptWidget = new SplitWidgetContainer(wFootnote.TextBody, wFootnote.TextBody.Items[0], 0);
						m_ltState = LayoutState.Splitted;
						return;
					}
					string footnoteID = (layoutContext.Widget.LayoutInfo as FootnoteLayoutInfo).FootnoteID;
					MarkAsNotFitted(layoutContext, isFootnote: true);
					(m_lcOperator as Layouter).FootnoteSplittedWidgets.Clear();
					layoutContext.Widget.InitLayoutInfo();
					(layoutContext.Widget.LayoutInfo as FootnoteLayoutInfo).FootnoteID = ((layoutContext.Widget.LayoutInfo as FootnoteLayoutInfo).FootnoteID.Equals(footnoteID) ? (layoutContext.Widget.LayoutInfo as FootnoteLayoutInfo).FootnoteID : footnoteID);
					m_sptWidget = new SplitWidgetContainer(layoutedWidget.Widget as WParagraph, (layoutedWidget.Widget as WParagraph).Items[0], 0);
					m_ltState = LayoutState.NotFitted;
					return;
				}
				(m_lcOperator as Layouter).IsFootnoteHeightAdjusted = layoutedWidget.ChildWidgets.Count == 2 && (m_lcOperator as Layouter).FootnoteSplittedWidgets.Count > 0;
				while ((m_lcOperator as Layouter).FootnoteWidgets.Count > (m_lcOperator as Layouter).FootNoteSectionIndex.Count)
				{
					(m_lcOperator as Layouter).FootNoteSectionIndex.Add(iWSection.Document.Sections.IndexOf(iWSection));
				}
				num += (layoutedWidget.ChildWidgets[i].ChildWidgets[j].Widget.LayoutInfo as FootnoteLayoutInfo).FootnoteHeight;
			}
		}
		if (num > 0f && (!(layoutedWidget.Widget is WParagraph) || !(layoutedWidget.Widget as WParagraph).IsExactlyRowHeight()))
		{
			m_layoutArea.CutFromTop(layoutedWidget.Bounds.Bottom, num);
		}
		if (flag && iWSection != null && layoutedWidget.Widget is WParagraph && iWSection.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && IsLastFootnoteNotInSamePage(layoutedWidget))
		{
			UpdateFootnoteWidgets(layoutedWidget);
			(m_lcOperator as Layouter).FootnoteSplittedWidgets.Clear();
			m_sptWidget = new SplitWidgetContainer(layoutedWidget.Widget as WParagraph, (layoutedWidget.Widget as WParagraph).Items[0], 0);
			m_ltState = LayoutState.NotFitted;
		}
	}

	private bool IsLastFootnoteNotInSamePage(LayoutedWidget layoutedWidget)
	{
		for (int num = layoutedWidget.ChildWidgets.Count - 1; num >= 0; num--)
		{
			for (int num2 = layoutedWidget.ChildWidgets[num].ChildWidgets.Count - 1; num2 >= 0; num2--)
			{
				if (layoutedWidget.ChildWidgets[num].ChildWidgets[num2].Widget is WFootnote { FootnoteType: not FootnoteType.Endnote })
				{
					WFootnote wFootnote2 = layoutedWidget.ChildWidgets[num].ChildWidgets[num2].Widget as WFootnote;
					bool flag = false;
					for (int num3 = (m_lcOperator as Layouter).FootnoteWidgets.Count - 1; num3 >= 0; num3--)
					{
						if ((((m_lcOperator as Layouter).FootnoteWidgets[num3].Widget is WTextBody) ? ((m_lcOperator as Layouter).FootnoteWidgets[num3].Widget as WTextBody) : (((m_lcOperator as Layouter).FootnoteWidgets[num3].Widget as SplitWidgetContainer).RealWidgetContainer as WTextBody)).Owner as WFootnote == wFootnote2)
						{
							flag = true;
							break;
						}
					}
					return !flag;
				}
			}
		}
		return false;
	}

	private void UpdateParagraphYPosition(LayoutedWidget widget, LayoutContext ltwidget)
	{
		if ((!(ltwidget.Widget is WParagraph) || !(ltwidget.Widget as WParagraph).ParagraphFormat.IsInFrame() || (ltwidget.Widget as WParagraph).ParagraphFormat.IsNextParagraphInSameFrame() || (ltwidget.Widget as WParagraph).IsInCell) && (!(ltwidget.Widget is SplitWidgetContainer) || !((ltwidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).ParagraphFormat.IsInFrame() || ((ltwidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).ParagraphFormat.IsNextParagraphInSameFrame() || ((ltwidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).IsInCell))
		{
			return;
		}
		WParagraph wParagraph = ((ltwidget.Widget is WParagraph) ? (ltwidget.Widget as WParagraph) : ((ltwidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph));
		WSection ownerSection = wParagraph.GetOwnerSection();
		ushort num = (ushort)(((ltwidget.Widget is WParagraph) ? (ltwidget.Widget as WParagraph).ParagraphFormat.FrameHeight : ((ltwidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).ParagraphFormat.FrameHeight) * 20f);
		float frameheight = (num & 0x7FFF) / 20;
		if ((num & 0x8000u) != 0 || wParagraph.ParagraphFormat.FrameHeight == 0f)
		{
			int index = widget.ChildWidgets.Count - 1;
			float num2 = GetsFrameHeight(ref index, widget, frameheight);
			if (wParagraph.ParagraphFormat.FrameY > ownerSection.PageSetup.PageSize.Height - num2 && index != -1 && wParagraph.ParagraphFormat.FrameVerticalPos == 1)
			{
				float num3 = ownerSection.PageSetup.PageSize.Height - num2;
				num3 = widget.ChildWidgets[index].Bounds.Y - num3;
				shiftYPosition(widget, index, num3);
				UpdateFloatingItemBounds(num3);
			}
		}
	}

	private void shiftYPosition(LayoutedWidget ltwidget, int index, float yposition)
	{
		for (int i = index; i < ltwidget.ChildWidgets.Count; i++)
		{
			ltwidget.ChildWidgets[i].ShiftLocation(0.0, 0f - yposition, isPictureNeedToBeShifted: false, isFromFloatingItemVerticalAlignment: false);
		}
	}

	private float GetsFrameHeight(ref int index, LayoutedWidget LayoutedWidget, float frameheight)
	{
		float num = 0f;
		while (index >= 0)
		{
			LayoutedWidget layoutedWidget = LayoutedWidget.ChildWidgets[index];
			WParagraph wParagraph = ((layoutedWidget.Widget is WParagraph && (layoutedWidget.Widget as WParagraph).ParagraphFormat.IsInFrame()) ? (layoutedWidget.Widget as WParagraph) : ((layoutedWidget.Widget is SplitWidgetContainer && ((layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).ParagraphFormat.IsInFrame()) ? ((layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null));
			if (wParagraph != null)
			{
				num += layoutedWidget.Bounds.Height + (layoutedWidget.Widget.LayoutInfo as ParagraphLayoutInfo).Margins.Top + (layoutedWidget.Widget.LayoutInfo as ParagraphLayoutInfo).Margins.Bottom;
				if (num > frameheight)
				{
					frameheight = num;
				}
				if (wParagraph.PreviousSibling == null || !(wParagraph.PreviousSibling is WParagraph) || !wParagraph.ParagraphFormat.IsPreviousParagraphInSameFrame())
				{
					return frameheight;
				}
				index--;
			}
			else
			{
				index--;
			}
		}
		return frameheight;
	}

	private void UpdateFloatingItemBounds(float yPosition)
	{
		for (int num = (m_lcOperator as Layouter).FloatingItems.Count - 1; num >= 0; num--)
		{
			FloatingItem floatingItem = (m_lcOperator as Layouter).FloatingItems[num];
			if (floatingItem.FloatingEntity is WParagraph)
			{
				floatingItem.TextWrappingBounds = new RectangleF(floatingItem.TextWrappingBounds.X, floatingItem.TextWrappingBounds.Y - yPosition, floatingItem.TextWrappingBounds.Width, floatingItem.TextWrappingBounds.Height);
				if (floatingItem.FloatingEntity is WParagraph && !(floatingItem.FloatingEntity as WParagraph).ParagraphFormat.IsPreviousParagraphInSameFrame())
				{
					break;
				}
			}
		}
	}

	private RectangleF ResetAdjustedboundsBasedOnWrapPolygon(FloatingItem floatingItem, RectangleF currChildLWBounds)
	{
		float num = 21600f / currChildLWBounds.Width;
		float num2 = 21600f / currChildLWBounds.Height;
		float num3 = 0f;
		float num4 = 0f;
		if (floatingItem.FloatingEntity is WTextBox)
		{
			num3 = (floatingItem.FloatingEntity as WTextBox).TextBoxFormat.WrapPolygon.Vertices[0].X / num;
			num4 = (floatingItem.FloatingEntity as WTextBox).TextBoxFormat.WrapPolygon.Vertices[0].Y / num2;
		}
		else if (floatingItem.FloatingEntity is WPicture)
		{
			num3 = (floatingItem.FloatingEntity as WPicture).WrapPolygon.Vertices[0].X / num;
			num4 = (floatingItem.FloatingEntity as WPicture).WrapPolygon.Vertices[0].Y / num2;
		}
		else if (floatingItem.FloatingEntity is Shape)
		{
			num3 = (floatingItem.FloatingEntity as Shape).WrapFormat.WrapPolygon.Vertices[0].X / num;
			num4 = (floatingItem.FloatingEntity as Shape).WrapFormat.WrapPolygon.Vertices[0].Y / num2;
		}
		else if (floatingItem.FloatingEntity is GroupShape)
		{
			num3 = (floatingItem.FloatingEntity as GroupShape).WrapFormat.WrapPolygon.Vertices[0].X / num;
			num4 = (floatingItem.FloatingEntity as GroupShape).WrapFormat.WrapPolygon.Vertices[0].Y / num2;
		}
		else if (floatingItem.FloatingEntity is WChart)
		{
			num3 = (floatingItem.FloatingEntity as WChart).WrapFormat.WrapPolygon.Vertices[0].X / num;
			num4 = (floatingItem.FloatingEntity as WChart).WrapFormat.WrapPolygon.Vertices[0].Y / num2;
		}
		currChildLWBounds.X -= num3;
		currChildLWBounds.Y -= num4;
		return currChildLWBounds;
	}

	private void SplitEndNoteWidgets()
	{
		if ((m_lcOperator as Layouter).EndnoteSplittedWidgets.Count > 0)
		{
			WidgetContainer container = ((m_currChildLW.Widget is SplitWidgetContainer) ? ((m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer as WidgetContainer) : (m_currChildLW.Widget as WidgetContainer));
			m_sptWidget = new SplitWidgetContainer(container);
			SplitedUpWidget(m_sptWidget, isEndNoteSplitWidgets: true);
			m_ltState = LayoutState.Splitted;
		}
	}

	private void AddBehindWidgets(LayoutedWidget ltWidget)
	{
		if ((m_lcOperator as Layouter).IsLayoutingHeaderFooter)
		{
			if ((m_lcOperator as Layouter).IsLayoutingHeader)
			{
				(m_lcOperator as Layouter).NumberOfBehindWidgetsInHeader++;
			}
			else
			{
				(m_lcOperator as Layouter).NumberOfBehindWidgetsInFooter++;
			}
		}
		int num = -1;
		for (int i = 0; i < (m_lcOperator as Layouter).BehindWidgets.Count; i++)
		{
			if ((m_lcOperator as Layouter).BehindWidgets[i].Widget == ltWidget.Widget)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			(m_lcOperator as Layouter).BehindWidgets.Add(ltWidget);
		}
		else
		{
			(m_lcOperator as Layouter).BehindWidgets[num] = ltWidget;
		}
	}

	private bool CheckWidgetWrappingType(IWidget widget)
	{
		if (widget is WPicture || widget is WTextBox || widget is Shape || widget is WChart || widget is GroupShape)
		{
			TextWrappingStyle textWrappingStyle;
			if ((textWrappingStyle = (widget as ParagraphItem).GetTextWrappingStyle()) != TextWrappingStyle.Square && textWrappingStyle != TextWrappingStyle.Tight)
			{
				return textWrappingStyle == TextWrappingStyle.Through;
			}
			return true;
		}
		return false;
	}

	private bool CheckWidgetWrappingTypeAndHorizontalOrigin(IWidget widget)
	{
		ParagraphItem paragraphItem = widget as ParagraphItem;
		if (widget is WPicture || widget is Shape || widget is WTextBox || widget is WChart || widget is GroupShape)
		{
			return CheckWrappingTypeAndHorizontalOrigin(paragraphItem.GetTextWrappingStyle(), paragraphItem.GetHorizontalOrigin());
		}
		return false;
	}

	private bool CheckWrappingTypeAndHorizontalOrigin(TextWrappingStyle textWrappingStyle, HorizontalOrigin horizontalOrigin)
	{
		if (textWrappingStyle == TextWrappingStyle.InFrontOfText || textWrappingStyle == TextWrappingStyle.Behind)
		{
			if (horizontalOrigin != HorizontalOrigin.Column)
			{
				return horizontalOrigin == HorizontalOrigin.Character;
			}
			return true;
		}
		return false;
	}

	private void IsDynamicRelayoutOccurByFrame(LayoutContext childContext)
	{
		int num = m_ltWidget.ChildWidgets.IndexOf(m_currChildLW);
		if (num <= 0)
		{
			return;
		}
		float previousItemBottom = GetPreviousItemBottom(num);
		float frameYPosition = GetFrameYPosition((m_currChildLW.Widget as WParagraph).ParagraphFormat, num);
		if (previousItemBottom > 0f && previousItemBottom > frameYPosition)
		{
			if (m_lcOperator is Layouter layouter && layouter.MaintainltWidget.ChildWidgets.Count > 0)
			{
				layouter.MaintainltWidget.ChildWidgets.RemoveRange(0, layouter.MaintainltWidget.ChildWidgets.Count);
			}
			SplitedUpWidget(GetFirstFrameItem(m_currChildLW.Widget as WParagraph, null) as IWidget, isEndNoteSplitWidgets: false, IsInFrame: true);
			m_ltState = LayoutState.DynamicRelayout;
		}
	}

	private float GetFrameYPosition(WParagraphFormat paraFormat, int index)
	{
		for (int num = index - 1; num >= 0; num--)
		{
			LayoutedWidget layoutedWidget = m_ltWidget.ChildWidgets[num];
			if (!(layoutedWidget.Widget is WParagraph) || !(layoutedWidget.Widget as WParagraph).ParagraphFormat.IsInSameFrame(paraFormat))
			{
				return m_ltWidget.ChildWidgets[num + 1].Bounds.Y;
			}
		}
		return m_ltWidget.ChildWidgets[index].Bounds.Y;
	}

	private float GetPreviousItemBottom(int index)
	{
		for (int num = index - 1; num >= 0; num--)
		{
			LayoutedWidget layoutedWidget = m_ltWidget.ChildWidgets[num];
			if (!(layoutedWidget.Bounds.Y > m_currChildLW.Bounds.Bottom))
			{
				if (layoutedWidget.Widget is WParagraph && !(layoutedWidget.Widget as WParagraph).ParagraphFormat.IsInFrame())
				{
					return GetPreviousLineBottom(layoutedWidget, m_currChildLW.Bounds.Bottom);
				}
				if (layoutedWidget.Widget is SplitWidgetContainer && (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph && !((layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).ParagraphFormat.IsInFrame())
				{
					return GetPreviousLineBottom(layoutedWidget, m_currChildLW.Bounds.Bottom);
				}
				if (layoutedWidget.Widget is WTable && !(layoutedWidget.Widget as WTable).TableFormat.WrapTextAround && !(layoutedWidget.Widget as WTable).IsFrame)
				{
					return layoutedWidget.Bounds.Bottom;
				}
			}
		}
		return 0f;
	}

	private float GetPreviousLineBottom(LayoutedWidget lineContainer, float frameBottom)
	{
		for (int num = lineContainer.ChildWidgets.Count - 1; num >= 0; num--)
		{
			if (!(lineContainer.ChildWidgets[num].Bounds.Y > frameBottom))
			{
				return lineContainer.ChildWidgets[num].Bounds.Bottom;
			}
		}
		return lineContainer.Bounds.Bottom;
	}

	private bool IsLastItemInFrame(WParagraph para)
	{
		if (para.NextSibling == null || ((!(para.NextSibling is WParagraph) || !(para.NextSibling as WParagraph).ParagraphFormat.IsFrame || !(para.NextSibling as WParagraph).ParagraphFormat.IsInSameFrame(para.ParagraphFormat)) && (!(para.NextSibling is WTable) || !IsInFrame(para.NextSibling as WTable) || !IsInSameFrame(para, para.NextSibling as WTable))))
		{
			return true;
		}
		return false;
	}

	private bool IsInSameFrame(WParagraph para, WTable table)
	{
		return para.ParagraphFormat.IsInSameFrame(table.Rows[0].Cells[0].Paragraphs[0].ParagraphFormat);
	}

	private void UpdateLWBounds(LayoutContext childContext)
	{
		float num = 0f;
		RectangleF bounds = m_ltWidget.Bounds;
		bool flag = false;
		double num2 = (m_bSkipAreaSpacing ? 0f : childContext.BoundsPaddingRight);
		double num3 = (m_bSkipAreaSpacing ? 0f : childContext.BoundsMarginBottom);
		bool flag2 = false;
		bool flag3 = false;
		if (childContext is LCLineContainer)
		{
			WParagraph wParagraph = ((m_currChildLW.Widget is WParagraph) ? (m_currChildLW.Widget as WParagraph) : ((m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph));
			num3 += (double)wParagraph.ParagraphFormat.Borders.Bottom.LineWidth;
			WParagraph wParagraph2 = ((wParagraph.NextSibling is WParagraph) ? (wParagraph.NextSibling as WParagraph) : null);
			if (wParagraph2 != null && !wParagraph.IsInCell && wParagraph2.SectionEndMark)
			{
				num3 = 0.0;
			}
			flag = wParagraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly;
			num = m_currChildLW.Bounds.Height;
			LayoutedWidgetList layoutedWidgetList = null;
			if (m_currChildLW.ChildWidgets.Count > 0)
			{
				layoutedWidgetList = m_currChildLW.ChildWidgets[m_currChildLW.ChildWidgets.Count - 1].ChildWidgets;
			}
			float rowHeight = 0f;
			if (wParagraph != null && wParagraph.IsInCell && layoutedWidgetList != null && layoutedWidgetList.Count > 0 && (wParagraph.IsLastLine(layoutedWidgetList[layoutedWidgetList.Count - 1]) || m_currChildLW.TextTag == "Splitted") && !wParagraph.IsExactlyRowHeight(wParagraph.GetOwnerEntity() as WTableCell, ref rowHeight) && m_currChildLW.Widget.LayoutInfo is ILayoutSpacingsInfo)
			{
				num3 -= (double)(m_currChildLW.Widget.LayoutInfo as ILayoutSpacingsInfo).Margins.Bottom;
			}
		}
		float num4 = num;
		if (num > bounds.Height && bounds.Height != 0f)
		{
			num4 -= bounds.Height;
		}
		if (m_ltWidget.ChildWidgets.Count > 0 && m_ltWidget.Widget is WParagraph && !(childContext.Widget is ParagraphItem) && IsInFrame(m_ltWidget.Widget as WParagraph))
		{
			LayoutedWidget layoutedWidget = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1];
			WParagraph wParagraph3 = m_ltWidget.Widget as WParagraph;
			IWidget widget = layoutedWidget.Widget;
			if (layoutedWidget.ChildWidgets.Count > 0)
			{
				widget = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget;
			}
			if (widget is SplitStringWidget && (widget as SplitStringWidget).SplittedText != null && ((widget as SplitStringWidget).SplittedText == string.Empty || (widget as SplitStringWidget).SplittedText[(widget as SplitStringWidget).SplittedText.Length - 1] == ((widget as SplitStringWidget).RealStringWidget as WTextRange).Text[((widget as SplitStringWidget).RealStringWidget as WTextRange).Text.Length - 1]))
			{
				widget = (widget as SplitStringWidget).RealStringWidget;
			}
			if (widget is ParagraphItem)
			{
				int num5 = wParagraph3.ChildEntities.Count;
				if (wParagraph3.ChildEntities.IndexOf(widget as Entity) == num5 - 1)
				{
					UpdateFrameBounds(wParagraph3);
					flag3 = true;
				}
				else
				{
					for (int i = wParagraph3.ChildEntities.IndexOf(widget as Entity) + 1; i < wParagraph3.ChildEntities.Count && (wParagraph3.ChildEntities[i] as IWidget).LayoutInfo.IsSkip; i++)
					{
						num5--;
					}
					if (wParagraph3.ChildEntities.IndexOf(widget as Entity) == num5 - 1)
					{
						UpdateFrameBounds(wParagraph3);
						flag3 = true;
					}
				}
				if (wParagraph3.ChildEntities.IndexOf(widget as Entity) == num5 - 1 && IsLastItemInFrame(wParagraph3))
				{
					flag2 = true;
				}
			}
		}
		float rightEgeExtent = 0f;
		float bottomEdgeExtent = 0f;
		if (m_currChildLW.Widget is ParagraphItem)
		{
			(m_currChildLW.Widget as ParagraphItem).GetEffectExtentValues(out var _, out rightEgeExtent, out var _, out bottomEdgeExtent);
		}
		RectangleF bounds2 = m_currChildLW.Bounds;
		bool flag4 = false;
		if (childContext is LCLineContainer)
		{
			WParagraph paragraph = ((m_currChildLW.Widget is WParagraph) ? (m_currChildLW.Widget as WParagraph) : ((m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph));
			flag4 = IsNeedToSkipRightPad(paragraph);
		}
		if (num2 < 0.0 && flag4)
		{
			num2 = ((bounds2.Right < (m_lcOperator as Layouter).ClientLayoutArea.Right) ? 0f : ((m_lcOperator as Layouter).ClientLayoutArea.Right - bounds2.Right));
		}
		double num6 = Math.Max((double)bounds2.Right + num2 + (double)rightEgeExtent, bounds.Right);
		double num7 = 0.0;
		WTextRange textRange = GetTextRange(childContext.Widget);
		if (IsBottomPositionNeedToBeUpdate(childContext))
		{
			num7 = ((bounds.Bottom > bounds2.Bottom && bounds.X < bounds2.X) ? Math.Max((double)bounds2.Bottom + num3 + (double)bottomEdgeExtent, bounds.Bottom) : ((num > bounds.Height) ? Math.Max((double)bounds2.Bottom + num3 + (double)bottomEdgeExtent, bounds.Bottom + num4) : ((!flag || !(m_currChildLW.Widget is SplitWidgetContainer) || flag2 || !((m_currChildLW.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)) ? Math.Max((double)bounds2.Bottom + num3 + (double)bottomEdgeExtent, bounds.Bottom) : ((double)(bounds2.Y + num)))));
		}
		bool flag5 = (childContext.Widget is WPicture || childContext.Widget is WChart || childContext.Widget is Shape || childContext.Widget is GroupShape) && (childContext.Widget as ParagraphItem).GetTextWrappingStyle() == TextWrappingStyle.Inline;
		WParagraph wParagraph4 = ((textRange != null) ? textRange.OwnerParagraph : (flag5 ? (childContext.Widget as ParagraphItem).GetOwnerParagraphValue() : null));
		if (wParagraph4 == null && textRange != null)
		{
			if (textRange.Owner is InlineContentControl || textRange.Owner is XmlParagraphItem)
			{
				wParagraph4 = textRange.GetOwnerParagraphValue();
			}
			else if (textRange.OwnerParagraph == null)
			{
				wParagraph4 = textRange.CharacterFormat.BaseFormat.OwnerBase as WParagraph;
			}
		}
		if ((textRange != null || flag5) && bounds2.Height != 0f && (num > bounds2.Height || flag) && !childContext.Widget.LayoutInfo.IsLineBreak && wParagraph4 != null && (wParagraph4.ParagraphFormat.LineSpacingRule != LineSpacingRule.Multiple || (num < 12f && Math.Abs(wParagraph4.ParagraphFormat.LineSpacing) < 12f && !flag5)))
		{
			float num8 = (wParagraph4.m_layoutInfo as ParagraphLayoutInfo).Margins.Top;
			if ((!base.LayoutInfo.IsClipped || !wParagraph4.IsInCell || !((wParagraph4.GetOwnerEntity() as WTableCell).OwnerRow.Height < num)) && (!(wParagraph4.OwnerTextBody.Owner is Shape) || !((wParagraph4.OwnerTextBody.Owner as Shape).TextLayoutingBounds.Height < num)))
			{
				if (num < bounds2.Height && flag)
				{
					m_currChildLW.Bounds = new RectangleF(bounds2.X, (float)num7 + num8 - bounds2.Height, bounds2.Width, bounds2.Height);
				}
				else if (!flag5 && textRange != null && textRange.CharacterFormat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.None)
				{
					m_currChildLW.Bounds = new RectangleF(bounds2.X, (float)num7 - bounds2.Height, bounds2.Width, bounds2.Height);
				}
				else if (flag5 && num > bounds2.Height && wParagraph4.ParagraphFormat.LineSpacingRule == LineSpacingRule.AtLeast)
				{
					m_currChildLW.Bounds = new RectangleF(bounds2.X, (float)num7 - bounds2.Height, bounds2.Width, bounds2.Height);
				}
			}
		}
		SizeF size = new SizeF((float)(num6 - (double)bounds.Left), (float)(num7 - (double)bounds.Top));
		if (flag3)
		{
			float num9 = bounds2.X - bounds.X;
			size.Width = bounds2.Width + ((num9 > 0f) ? num9 : 0f);
		}
		if (childContext.Widget is WParagraph || (childContext.Widget is SplitWidgetContainer && (childContext.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph))
		{
			WParagraph wParagraph5 = childContext.Widget as WParagraph;
			if (wParagraph5 == null)
			{
				wParagraph5 = (childContext.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
			}
			WParagraphStyle wParagraphStyle = wParagraph5.GetStyle() as WParagraphStyle;
			if (!wParagraph5.ParagraphFormat.Borders.NoBorder || (wParagraphStyle != null && wParagraphStyle.ParagraphFormat != null && !wParagraphStyle.ParagraphFormat.Borders.NoBorder) || (wParagraphStyle != null && wParagraphStyle.BaseStyle != null && wParagraphStyle.BaseStyle.ParagraphFormat != null && !wParagraphStyle.BaseStyle.ParagraphFormat.Borders.NoBorder) || wParagraph5.ParagraphFormat.FrameWidth != 0f)
			{
				size.Width = m_layoutArea.ClientActiveArea.Width;
			}
			if (childContext is LCLineContainer && wParagraph5 != null && wParagraph5.ParagraphFormat.IsInFrame())
			{
				if (wParagraph5.GetBaseEntity(wParagraph5) is WSection wSection && wSection.Columns.Count == 1 && IsLastItemInFrame(wParagraph5))
				{
					if (wParagraph5 == (m_lcOperator as Layouter).DynamicParagraph)
					{
						(m_lcOperator as Layouter).DynamicParagraph = null;
					}
					else if ((m_lcOperator as Layouter).DynamicParagraph == null && (m_lcOperator as Layouter).DynamicTable == null)
					{
						IsDynamicRelayoutOccurByFrame(childContext);
					}
				}
				if (IsInFrame(wParagraph5) && wParagraph5.ParagraphFormat.IsFrameYAlign(wParagraph5.ParagraphFormat.FrameY))
				{
					UpdateVerticalAlignment((short)wParagraph5.ParagraphFormat.FrameY);
				}
			}
		}
		if (childContext.Widget is WPicture || childContext.Widget is Shape || childContext.Widget is WChart || childContext.Widget is GroupShape)
		{
			WPicture wPicture = childContext.Widget as WPicture;
			Shape shape = childContext.Widget as Shape;
			WChart wChart = childContext.Widget as WChart;
			GroupShape groupShape = childContext.Widget as GroupShape;
			TextWrappingStyle textWrappingStyle = (childContext.Widget as ParagraphItem).GetTextWrappingStyle();
			bool flag6 = shape?.WrapFormat.IsWrappingBoundsAdded ?? wChart?.WrapFormat.IsWrappingBoundsAdded ?? groupShape?.WrapFormat.IsWrappingBoundsAdded ?? wPicture.IsWrappingBoundsAdded;
			wParagraph4 = (childContext.Widget as ParagraphItem).GetOwnerParagraphValue();
			if (textWrappingStyle == TextWrappingStyle.InFrontOfText || textWrappingStyle == TextWrappingStyle.Behind)
			{
				if (wParagraph4 != null && wParagraph4.ChildEntities.Count == 1 && m_ltWidget.Bounds.Height == 0f)
				{
					float num10 = Math.Abs(wParagraph4.ParagraphFormat.LineSpacing);
					SizeF size2 = ((IWidget)wParagraph4).LayoutInfo.Size;
					if (wParagraph4.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly)
					{
						size.Height = num10;
					}
					else if (wParagraph4.ParagraphFormat.LineSpacingRule == LineSpacingRule.AtLeast)
					{
						if (size2.Height > num10)
						{
							size.Height = size2.Height;
						}
						else
						{
							size.Height = num10;
						}
					}
					else
					{
						size.Height = size2.Height * (num10 / 12f);
					}
				}
				else
				{
					size.Height = m_ltWidget.Bounds.Height;
				}
			}
			if (textWrappingStyle != 0 && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind && !flag6 && !(m_lcOperator as Layouter).IsNeedToRelayout)
			{
				FloatingItem floatingItem = new FloatingItem();
				floatingItem.TextWrappingBounds = m_currChildLW.Bounds;
				floatingItem.FloatingEntity = m_currChildLW.Widget as Entity;
				(m_lcOperator as Layouter).FloatingItems.Add(floatingItem);
				floatingItem.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
				floatingItem.IsFloatingItemFit = true;
			}
			if (textWrappingStyle != 0 && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind && num7 == (double)bounds2.Bottom && (m_ltWidget.Bounds.Height == 0f || size.Height != m_ltWidget.Bounds.Height))
			{
				size.Height = ((IWidget)wParagraph4).LayoutInfo.Size.Height;
			}
		}
		if (childContext.Widget is WTextBox)
		{
			WTextBox wTextBox = childContext.Widget as WTextBox;
			if (wTextBox.TextBoxFormat.TextWrappingStyle != 0)
			{
				if (!wTextBox.TextBoxFormat.IsWrappingBoundsAdded)
				{
					FloatingItem floatingItem2 = new FloatingItem();
					floatingItem2.TextWrappingBounds = new RectangleF(m_currChildLW.ChildWidgets[0].Bounds.X, m_currChildLW.ChildWidgets[0].Bounds.Y, m_currChildLW.Bounds.Width, m_currChildLW.Bounds.Height);
					floatingItem2.FloatingEntity = m_currChildLW.Widget as Entity;
					(m_lcOperator as Layouter).FloatingItems.Add(floatingItem2);
					wTextBox.TextBoxFormat.WrapCollectionIndex = (short)((m_lcOperator as Layouter).FloatingItems.Count - 1);
					floatingItem2.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
					floatingItem2.IsFloatingItemFit = true;
				}
				if (m_ltWidget.Widget is WParagraph)
				{
					if (m_ltWidget.Widget is WParagraph wParagraph6 && m_ltWidget.Bounds.Height == 0f && wParagraph6.Text == string.Empty)
					{
						SizeF size3 = ((IWidget)wParagraph6).LayoutInfo.Size;
						float num11 = Math.Abs(wParagraph6.ParagraphFormat.LineSpacing);
						if (wParagraph6.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly)
						{
							size.Height = num11;
						}
						else if (wParagraph6.ParagraphFormat.LineSpacingRule == LineSpacingRule.AtLeast)
						{
							if (size3.Height > num11)
							{
								size.Height = size3.Height;
							}
							else
							{
								size.Height = num11;
							}
						}
						else
						{
							size.Height = size3.Height * (num11 / 12f);
						}
					}
					else
					{
						size.Height = m_ltWidget.Bounds.Height;
					}
				}
				else
				{
					size.Height = m_ltWidget.Bounds.Height;
				}
			}
		}
		if (childContext.Widget is WTable)
		{
			WTable wTable = childContext.Widget as WTable;
			if (wTable != (m_lcOperator as Layouter).DynamicTable && (childContext.Widget as WTable).TableFormat.WrapTextAround)
			{
				RowFormat.TablePositioning positioning = (childContext.Widget as WTable).TableFormat.Positioning;
				FloatingItem floatingItem3 = new FloatingItem();
				if (m_currChildLW.ChildWidgets.Count > 0)
				{
					float num12 = m_currChildLW.Bounds.Width + positioning.DistanceFromLeft + positioning.DistanceFromRight;
					floatingItem3.TextWrappingBounds = new RectangleF(m_currChildLW.Bounds.X - positioning.DistanceFromLeft, m_currChildLW.Bounds.Y - positioning.DistanceFromTop, (float)Math.Round(num12), m_currChildLW.Bounds.Height + positioning.DistanceFromTop + positioning.DistanceFromBottom);
				}
				else
				{
					floatingItem3.TextWrappingBounds = m_currChildLW.Bounds;
				}
				floatingItem3.FloatingEntity = m_currChildLW.Widget as Entity;
				(m_lcOperator as Layouter).FloatingItems.Add(floatingItem3);
				floatingItem3.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
			}
			else if (IsInFrame(wTable))
			{
				UpdateFrameBounds(wTable);
				RectangleF empty = RectangleF.Empty;
				if ((m_lcOperator as Layouter).FrameBounds.X == 0f)
				{
					empty.X = m_currChildLW.Bounds.X;
				}
				else
				{
					empty.X = (m_lcOperator as Layouter).FrameBounds.X;
				}
				if ((m_lcOperator as Layouter).FrameBounds.Y == 0f)
				{
					empty.Y = m_currChildLW.Bounds.Y;
				}
				else
				{
					empty.Y = (m_lcOperator as Layouter).FrameBounds.Y;
				}
				if ((m_lcOperator as Layouter).FrameBounds.Width == 0f)
				{
					empty.Width = ((wTable.Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.FrameWidth > 0f) ? wTable.Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.FrameWidth : m_currChildLW.Bounds.Width);
				}
				else
				{
					empty.Width = (m_lcOperator as Layouter).FrameBounds.Width;
				}
				empty.Height = (m_lcOperator as Layouter).FrameBounds.Height + m_currChildLW.Bounds.Height;
				(m_lcOperator as Layouter).FrameBounds = empty;
			}
			if (IsInFrame(wTable) && (!(wTable.NextSibling is WParagraph) || !(wTable.NextSibling as WParagraph).ParagraphFormat.IsFrame || !(wTable.NextSibling as WParagraph).ParagraphFormat.IsInSameFrame(wTable.Rows[0].Cells[0].Paragraphs[0].ParagraphFormat)) && (!(wTable.NextSibling is WTable) || !(wTable.NextSibling as WTable).IsFrame || !IsInSameFrame(wTable.Rows[0].Cells[0].Paragraphs[0], wTable.NextSibling as WTable)))
			{
				flag2 = true;
			}
			int num13 = m_ltWidget.ChildWidgets.IndexOf(m_currChildLW);
			int num14 = -1;
			if (num13 > 0)
			{
				num14 = GetPreviousItemIndex(num13);
			}
			if (num14 > -1 && Math.Round(m_ltWidget.ChildWidgets[num14].Bounds.Bottom) > Math.Round(m_currChildLW.Bounds.Y - wTable.TableFormat.Positioning.DistanceFromTop) && m_ltWidget.ChildWidgets[num14].Bounds.Height > 0f && !(m_lcOperator as Layouter).IsLayoutingHeaderFooter && (m_lcOperator as Layouter).DynamicTable == null && (!IsInFrame(wTable) || flag2))
			{
				Layouter layouter = m_lcOperator as Layouter;
				if (layouter.MaintainltWidget.ChildWidgets.Count > 0)
				{
					layouter.MaintainltWidget.ChildWidgets.RemoveRange(0, layouter.MaintainltWidget.ChildWidgets.Count);
				}
				if (flag2)
				{
					SplitedUpWidget(GetFirstFrameItem(m_currChildLW.Widget as WTable, null) as IWidget, isEndNoteSplitWidgets: false, IsInFrame: true);
				}
				else
				{
					SplitedUpWidget(childContext.SplittedWidget, isEndNoteSplitWidgets: false);
				}
				m_ltState = LayoutState.DynamicRelayout;
			}
			if (wTable == (m_lcOperator as Layouter).DynamicTable)
			{
				(m_lcOperator as Layouter).DynamicTable = null;
			}
		}
		if (size.Height < 0f)
		{
			size.Height = 0f;
		}
		if (size.Width < 0f)
		{
			size.Width = 0f;
		}
		if (m_currChildLW.Bounds.Height == 0f && m_currChildLW.Widget is WParagraph && m_currChildLW.Widget.LayoutInfo.IsSkip && m_currChildLW.ChildWidgets.Count == 0)
		{
			size = default(SizeF);
		}
		m_ltWidget.Bounds = new RectangleF(bounds.Location, size);
		if (flag3)
		{
			UpdateFrameBounds();
		}
		if (flag2)
		{
			AddFrameBounds();
		}
	}

	private bool IsNeedToSkipRightPad(WParagraph paragraph)
	{
		bool isNeedToUpdateWidth = false;
		paragraph.OwnerTextBody?.IsAutoFit(ref isNeedToUpdateWidth, paragraph.OwnerTextBody.Owner as IWidget);
		return isNeedToUpdateWidth;
	}

	private int GetPreviousItemIndex(int index)
	{
		for (int num = index - 1; num >= 0; num--)
		{
			if ((m_ltWidget.ChildWidgets[num].Widget is WParagraph && !IsFloatingFrame(m_ltWidget.ChildWidgets[num].Widget as WParagraph)) || (m_ltWidget.ChildWidgets[num].Widget is SplitWidgetContainer && (m_ltWidget.ChildWidgets[num].Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph && !IsFloatingFrame((m_ltWidget.ChildWidgets[num].Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph)))
			{
				return num;
			}
			if (m_ltWidget.ChildWidgets[num].Widget is WTable && !(m_ltWidget.ChildWidgets[num].Widget as WTable).TableFormat.WrapTextAround && !(m_ltWidget.ChildWidgets[num].Widget as WTable).IsFrame)
			{
				return num;
			}
		}
		return -1;
	}

	private bool IsFloatingFrame(WParagraph paragraph)
	{
		if (paragraph != null && paragraph.ParagraphFormat.IsFrame && paragraph.ParagraphFormat.WrapFrameAround != FrameWrapMode.None)
		{
			return true;
		}
		return false;
	}

	private bool IsBottomPositionNeedToBeUpdate(LayoutContext childContext)
	{
		if (m_ltWidget.Widget is WSection || m_ltWidget.Widget is HeaderFooter || (m_ltWidget.Widget is BlockContentControl && IsInSection(m_ltWidget.Widget as BlockContentControl) && (!(childContext.Widget is WTable) || !(childContext.Widget as WTable).TableFormat.WrapTextAround)) || (m_ltWidget.Widget is SplitWidgetContainer && (m_ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WSection))
		{
			if ((!(childContext.Widget is WParagraph) || !IsInFrame(childContext.Widget as WParagraph)) && (!(childContext.Widget is SplitWidgetContainer) || !((childContext.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) || !IsInFrame((childContext.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) || !(m_ltWidget.Widget is BlockContentControl)) && (!(childContext.Widget is SplitWidgetContainer) || !((childContext.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) || !IsInFrame((childContext.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph)))
			{
				if (childContext.Widget is WTable)
				{
					if (!(childContext.Widget as WTable).TableFormat.WrapTextAround)
					{
						return !IsInFrame(childContext.Widget as WTable);
					}
					return false;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	protected bool IsNonRenderableItem(IWidget widget)
	{
		WField wField = null;
		if (widget is WField)
		{
			wField = widget as WField;
		}
		if ((wField == null || (wField.FieldType != FieldType.FieldPageRef && wField.FieldType != FieldType.FieldRef)) && !(widget is BookmarkStart) && !(widget is BookmarkEnd) && !(widget is EditableRangeStart))
		{
			return widget is EditableRangeEnd;
		}
		return true;
	}

	private bool IsInSection(BlockContentControl blockContentControl)
	{
		bool result = false;
		if (blockContentControl.OwnerTextBody.Owner is WSection)
		{
			result = true;
		}
		else if (blockContentControl.OwnerTextBody.Owner is BlockContentControl)
		{
			result = IsInSection(blockContentControl.OwnerTextBody.Owner as BlockContentControl);
		}
		return result;
	}

	private void AddFrameBounds()
	{
		WParagraph wParagraph = m_ltWidget.Widget as WParagraph;
		WTable wTable = m_currChildLW.Widget as WTable;
		FloatingItem floatingItem = null;
		if ((wParagraph != null && wParagraph.ParagraphFormat.IsFrame && wParagraph.ParagraphFormat.WrapFrameAround != FrameWrapMode.None) || (wTable != null && wTable.IsFrame))
		{
			List<Entity> list = new List<Entity>();
			floatingItem = new FloatingItem();
			if (wTable != null)
			{
				list.Add(m_currChildLW.Widget as Entity);
				floatingItem.FloatingEntity = GetFirstFrameItem(m_currChildLW.Widget as Entity, list);
			}
			else
			{
				list.Add(m_ltWidget.Widget as Entity);
				floatingItem.FloatingEntity = GetFirstFrameItem(m_ltWidget.Widget as Entity, list);
			}
			floatingItem.FrameEntities = list;
		}
		if (floatingItem != null)
		{
			floatingItem.TextWrappingBounds = (m_lcOperator as Layouter).FrameBounds;
			if (!IsFloatingEntityExist(floatingItem.FloatingEntity))
			{
				(m_lcOperator as Layouter).FloatingItems.Add(floatingItem);
				floatingItem.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
			}
		}
		(m_lcOperator as Layouter).FrameBounds = RectangleF.Empty;
	}

	private void UpdateFrameBounds()
	{
		RectangleF bounds = m_ltWidget.Bounds;
		WParagraph wParagraph = m_ltWidget.Widget as WParagraph;
		bool flag = false;
		ILayoutSpacingsInfo layoutSpacingsInfo = base.LayoutInfo as ILayoutSpacingsInfo;
		if (wParagraph == null || !wParagraph.ParagraphFormat.IsFrame || wParagraph.IsInCell || wParagraph.ParagraphFormat.WrapFrameAround == FrameWrapMode.None)
		{
			return;
		}
		bool flag2 = false;
		if (bounds.Bottom < (m_lcOperator as Layouter).FrameLayoutArea.Y)
		{
			bounds.Height += (m_lcOperator as Layouter).FrameLayoutArea.Y - bounds.Bottom;
		}
		if (wParagraph.ParagraphFormat.FrameWidth != 0f)
		{
			bounds.Width = wParagraph.ParagraphFormat.FrameWidth;
		}
		else if (!wParagraph.ParagraphFormat.IsNextParagraphInSameFrame() && !wParagraph.ParagraphFormat.IsPreviousParagraphInSameFrame())
		{
			bounds.Height += 2f * wParagraph.ParagraphFormat.FrameVerticalDistanceFromText;
			flag = true;
		}
		else
		{
			bounds.Width = (m_lcOperator as Layouter).FrameLayoutArea.Width;
			bounds.Height += 2f * wParagraph.ParagraphFormat.FrameVerticalDistanceFromText + ((layoutSpacingsInfo != null) ? (layoutSpacingsInfo.Margins.Top + layoutSpacingsInfo.Margins.Bottom) : 0f);
			flag2 = true;
		}
		bool flag3 = true;
		if (wParagraph.ParagraphFormat.FrameHeight != 0f)
		{
			flag3 = ((ushort)Math.Round(wParagraph.ParagraphFormat.FrameHeight * 20f) & 0x8000) != 0;
		}
		if (!wParagraph.ParagraphFormat.IsNextParagraphInSameFrame() || !wParagraph.ParagraphFormat.IsPreviousParagraphInSameFrame())
		{
			bounds.Y -= wParagraph.ParagraphFormat.FrameVerticalDistanceFromText + (layoutSpacingsInfo?.Margins.Top ?? 0f) + ((((IWidget)wParagraph).LayoutInfo as ParagraphLayoutInfo).SkipTopBorder ? 0f : (wParagraph.ParagraphFormat.Borders.Top.GetLineWidthValue() + wParagraph.ParagraphFormat.Borders.Top.Space));
		}
		if (!wParagraph.ParagraphFormat.IsNextParagraphInSameFrame() || !wParagraph.ParagraphFormat.IsPreviousParagraphInSameFrame())
		{
			bounds.Height += 2f * wParagraph.ParagraphFormat.FrameVerticalDistanceFromText + ((layoutSpacingsInfo != null) ? (layoutSpacingsInfo.Margins.Top + layoutSpacingsInfo.Margins.Bottom) : 0f) + ((((IWidget)wParagraph).LayoutInfo as ParagraphLayoutInfo).SkipTopBorder ? 0f : (wParagraph.ParagraphFormat.Borders.Bottom.GetLineWidthValue() + wParagraph.ParagraphFormat.Borders.Bottom.Space));
		}
		else if (!flag2 && bounds.Height != 0f && (flag3 || !(wParagraph.ParagraphFormat.FrameHeight < (m_lcOperator as Layouter).FrameBounds.Height + bounds.Height)))
		{
			bounds.Height += ((layoutSpacingsInfo != null) ? (layoutSpacingsInfo.Margins.Top + layoutSpacingsInfo.Margins.Bottom) : 0f);
		}
		if (flag)
		{
			bounds.Width += 2f * wParagraph.ParagraphFormat.FrameHorizontalDistanceFromText;
			bounds.X -= wParagraph.ParagraphFormat.FrameHorizontalDistanceFromText;
		}
		else
		{
			if (wParagraph.ParagraphFormat.FrameWidth != 0f)
			{
				bounds.Width += 2f * wParagraph.ParagraphFormat.FrameHorizontalDistanceFromText;
			}
			else
			{
				bounds.Width += 2f * wParagraph.ParagraphFormat.FrameHorizontalDistanceFromText + ((layoutSpacingsInfo != null) ? (layoutSpacingsInfo.Margins.Left + layoutSpacingsInfo.Margins.Right) : 0f);
			}
			bounds.X -= wParagraph.ParagraphFormat.FrameHorizontalDistanceFromText + (layoutSpacingsInfo?.Margins.Left ?? 0f);
		}
		_ = m_ltWidget.Widget;
		RectangleF empty = RectangleF.Empty;
		if ((m_lcOperator as Layouter).FrameBounds.X == 0f)
		{
			empty.X = bounds.X;
		}
		else
		{
			empty.X = (m_lcOperator as Layouter).FrameBounds.X;
		}
		if ((m_lcOperator as Layouter).FrameBounds.Y == 0f)
		{
			empty.Y = bounds.Y;
		}
		else
		{
			empty.Y = (m_lcOperator as Layouter).FrameBounds.Y;
		}
		if ((m_lcOperator as Layouter).FrameBounds.Width == 0f)
		{
			empty.Width = bounds.Width;
		}
		else
		{
			empty.Width = (m_lcOperator as Layouter).FrameBounds.Width;
		}
		empty.Height = (m_lcOperator as Layouter).FrameBounds.Height + bounds.Height;
		(m_lcOperator as Layouter).FrameBounds = empty;
	}

	private Entity GetFirstFrameItem(Entity entity, List<Entity> frameEntities)
	{
		while (entity.PreviousSibling != null)
		{
			if (entity is WParagraph && entity.PreviousSibling is WParagraph && (entity.PreviousSibling as WParagraph).ParagraphFormat.IsFrame && (entity as WParagraph).ParagraphFormat.IsInSameFrame((entity.PreviousSibling as WParagraph).ParagraphFormat))
			{
				entity = entity.PreviousSibling as WParagraph;
			}
			else if (entity is WTable && entity.PreviousSibling is WParagraph && (entity.PreviousSibling as WParagraph).ParagraphFormat.IsFrame && (entity as WTable).Rows[0].Cells[0].Paragraphs.Count > 0 && (entity as WTable).Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.IsInSameFrame((entity.PreviousSibling as WParagraph).ParagraphFormat))
			{
				entity = entity.PreviousSibling as WParagraph;
			}
			else if (entity is WParagraph && entity.PreviousSibling is WTable && (entity.PreviousSibling as WTable).IsFrame && (entity.PreviousSibling as WTable).Rows[0].Cells[0].Paragraphs.Count > 0 && (entity.PreviousSibling as WTable).Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.IsInSameFrame((entity as WParagraph).ParagraphFormat))
			{
				entity = entity.PreviousSibling as WTable;
			}
			else
			{
				if (!(entity is WTable) || !(entity.PreviousSibling is WTable) || !(entity.PreviousSibling as WTable).IsFrame || (entity.PreviousSibling as WTable).Rows[0].Cells[0].Paragraphs.Count <= 0 || !(entity.PreviousSibling as WTable).Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.IsInSameFrame((entity.PreviousSibling as WTable).Rows[0].Cells[0].Paragraphs[0].ParagraphFormat))
				{
					break;
				}
				entity = entity.PreviousSibling as WTable;
			}
			frameEntities?.Insert(0, entity);
		}
		return entity;
	}

	private bool IsFloatingEntityExist(Entity entity)
	{
		foreach (FloatingItem floatingItem in (m_lcOperator as Layouter).FloatingItems)
		{
			if (floatingItem.FloatingEntity == entity)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateFrameBounds(WParagraph paragraph)
	{
		RectangleF bounds = m_ltWidget.Bounds;
		IEntity nextSibling = paragraph.NextSibling;
		if ((nextSibling == null || (nextSibling is WParagraph && !paragraph.ParagraphFormat.IsNextParagraphInSameFrame()) || (nextSibling is WTable && !(nextSibling as WTable).IsFrame)) && paragraph.ParagraphFormat.FrameHeight != 0f)
		{
			if (paragraph.IsAtleastFrameHeight())
			{
				if (IsNeedToConsiderAtleastFrameHeight(m_ltWidget, m_currChildLW))
				{
					bounds.Height = (m_lcOperator as Layouter).FrameHeight;
					m_currChildLW.Bounds = bounds;
					(m_lcOperator as Layouter).IsSkipBottomForFrame = true;
				}
			}
			else
			{
				bounds.Height = bounds.Height + (m_lcOperator as Layouter).FrameLayoutArea.Bottom - bounds.Bottom;
				m_currChildLW.Bounds = bounds;
			}
		}
		if (paragraph.ParagraphFormat.FrameWidth < m_layoutArea.ClientActiveArea.Width && paragraph.ParagraphFormat.IsFrameXAlign(paragraph.ParagraphFormat.FrameX) && !paragraph.ParagraphFormat.IsNextParagraphInSameFrame() && !paragraph.ParagraphFormat.IsPreviousParagraphInSameFrame())
		{
			UpdateHorizontalAlignment((short)paragraph.ParagraphFormat.FrameX);
		}
	}

	private bool IsNeedToConsiderAtleastFrameHeight(LayoutedWidget paraLayoutedWidget, LayoutedWidget lastLineLayoutedWidget)
	{
		RectangleF bounds = paraLayoutedWidget.Bounds;
		float num = UpdateSpacingInfo(bounds, paraLayoutedWidget).Height - bounds.Height;
		return lastLineLayoutedWidget.Bounds.Bottom - paraLayoutedWidget.Bounds.Y + num + ((paraLayoutedWidget.Widget.LayoutInfo is ILayoutSpacingsInfo) ? (paraLayoutedWidget.Widget.LayoutInfo as ILayoutSpacingsInfo).Paddings.Bottom : 0f) < (m_lcOperator as Layouter).FrameHeight;
	}

	private void UpdateFrameBounds(WTable table)
	{
		RectangleF bounds = m_currChildLW.Bounds;
		IEntity nextSibling = table.NextSibling;
		if ((nextSibling == null || (nextSibling is WParagraph && !(nextSibling as WParagraph).ParagraphFormat.IsFrame) || (nextSibling is WTable && !(nextSibling as WTable).IsFrame)) && table.Rows.Count > 0 && table.Rows[0].Cells.Count > 0 && table.Rows[0].Cells[0].Paragraphs.Count > 0 && table.Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.FrameHeight != 0f)
		{
			ushort num = (ushort)(table.Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.FrameHeight * 20f);
			bool flag = (num & 0x8000) != 0;
			float num2 = (num & 0x7FFF) / 20;
			if (!flag || m_currChildLW.Bounds.Height < num2)
			{
				bounds.Height = bounds.Height + (m_lcOperator as Layouter).FrameLayoutArea.Bottom - bounds.Bottom;
				m_currChildLW.Bounds = bounds;
			}
		}
		WParagraphFormat paragraphFormat = table.Rows[0].Cells[0].Paragraphs[0].ParagraphFormat;
		if (paragraphFormat.IsFrameXAlign(paragraphFormat.FrameX))
		{
			UpdateHorizontalAlignment((short)paragraphFormat.FrameX);
		}
		if (paragraphFormat.IsFrameYAlign(paragraphFormat.FrameY))
		{
			UpdateVerticalAlignment((short)paragraphFormat.FrameY);
		}
	}

	protected virtual void UpdateHorizontalAlignment(short xAlignment)
	{
	}

	private void UpdateVerticalAlignment(short yAlginment)
	{
		RectangleF bounds = m_currChildLW.Bounds;
		switch (yAlginment)
		{
		case -8:
			m_currChildLW.ShiftLocation(0.0, (0f - bounds.Height) / 2f, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
			break;
		case -20:
		case -12:
			m_currChildLW.ShiftLocation(0.0, 0f - bounds.Height, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
			break;
		}
	}

	private void CommitKeepWithNext(ref IWidget splittedWidget, bool isKeep)
	{
		WSection section = GetSection();
		bool isLastTocParagraphRemoved = false;
		if (section != null && section.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
		{
			CommitWithKeepWithNexForWord2013Format(ref splittedWidget, section, ref isLastTocParagraphRemoved);
			if (!isLastTocParagraphRemoved)
			{
				IsEndOfTOC();
			}
			if (splittedWidget != null)
			{
				RemoveAutoHyphenatedString(splittedWidget, section.Document.DOP.AutoHyphen);
			}
			return;
		}
		bool isWidgetsRemoved = false;
		int splittedWidgetIndex = 0;
		if (isKeep)
		{
			RemoveltWidgets(ref isLastTocParagraphRemoved, m_ltWidget, ref splittedWidget, isBlockContentControlChild: false, ref isWidgetsRemoved, ref splittedWidgetIndex);
		}
		if (section != null && isKeep && !isLastTocParagraphRemoved)
		{
			IsEndOfTOC();
		}
		if (section != null && splittedWidget != null)
		{
			RemoveAutoHyphenatedString(splittedWidget, section.Document.DOP.AutoHyphen);
		}
	}

	private WSection GetSection()
	{
		if (m_sptWidget is WSection)
		{
			return m_sptWidget as WSection;
		}
		if (m_sptWidget is BlockContentControl)
		{
			return (m_sptWidget as BlockContentControl).GetOwnerSection(m_sptWidget as Entity) as WSection;
		}
		if (m_sptWidget is SplitWidgetContainer)
		{
			if ((m_sptWidget as SplitWidgetContainer).RealWidgetContainer is WSection)
			{
				return (m_sptWidget as SplitWidgetContainer).RealWidgetContainer as WSection;
			}
			if ((m_sptWidget as SplitWidgetContainer).RealWidgetContainer is BlockContentControl)
			{
				return ((m_sptWidget as SplitWidgetContainer).RealWidgetContainer as BlockContentControl).GetOwnerSection(m_sptWidget as Entity) as WSection;
			}
		}
		return null;
	}

	private bool RemoveltWidgets(ref bool isLastTocParagraphRemoved, LayoutedWidget lwtWidget, ref IWidget splittedWidget, bool isBlockContentControlChild, ref bool isWidgetsRemoved, ref int splittedWidgetIndex)
	{
		while (lwtWidget.ChildWidgets.Count > 0 && !(m_lcOperator as Layouter).IsLayoutingHeaderFooter)
		{
			bool flag = false;
			IWidget widget = lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1].Widget;
			if (widget is WTable && !(widget as WTable).TableFormat.WrapTextAround && !(widget as WTable).IsInCell)
			{
				LayoutedWidget layoutedWidget = lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1];
				int num = layoutedWidget.ChildWidgets.Count;
				while (layoutedWidget.ChildWidgets.Count > 0 && layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget.LayoutInfo.IsKeepWithNext && !base.IsForceFitLayout)
				{
					if (IsLastTOCParagraph(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget as Entity))
					{
						isLastTocParagraphRemoved = true;
					}
					RemoveBehindWidgets(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1]);
					layoutedWidget.ChildWidgets.RemoveAt(layoutedWidget.ChildWidgets.Count - 1);
					isWidgetsRemoved = true;
					flag = true;
					num--;
				}
				if (!flag)
				{
					return true;
				}
				if (layoutedWidget.ChildWidgets.Count > 0)
				{
					splittedWidget = new SplitTableWidget(layoutedWidget.Widget as ITableWidget, num + 1);
					m_ltState = LayoutState.Splitted;
					if (isBlockContentControlChild)
					{
						splittedWidgetIndex--;
					}
					else
					{
						m_curWidgetIndex--;
					}
					return true;
				}
				RemoveBehindWidgets(lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1]);
				lwtWidget.ChildWidgets.RemoveAt(lwtWidget.ChildWidgets.Count - 1);
				if (isBlockContentControlChild)
				{
					splittedWidgetIndex--;
				}
				else
				{
					m_curWidgetIndex--;
				}
			}
			else if (widget is BlockContentControl)
			{
				LayoutedWidget layoutedWidget2 = lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1];
				if (widget.LayoutInfo.IsKeepWithNext)
				{
					RemoveBehindWidgets(layoutedWidget2);
					lwtWidget.ChildWidgets.Remove(layoutedWidget2);
					isWidgetsRemoved = true;
					if (isBlockContentControlChild)
					{
						splittedWidget = widget;
						splittedWidgetIndex--;
					}
					else
					{
						m_curWidgetIndex--;
					}
					continue;
				}
				int num2 = splittedWidgetIndex;
				bool flag2 = isWidgetsRemoved;
				splittedWidgetIndex = layoutedWidget2.ChildWidgets.Count;
				isWidgetsRemoved = false;
				bool flag3 = layoutedWidget2.ChildWidgets.Count <= 0 || RemoveltWidgets(ref isLastTocParagraphRemoved, layoutedWidget2, ref splittedWidget, isBlockContentControlChild: true, ref isWidgetsRemoved, ref splittedWidgetIndex);
				if (splittedWidget != null && splittedWidget is SplitWidgetContainer)
				{
					splittedWidgetIndex--;
				}
				if ((splittedWidget != null) & isWidgetsRemoved)
				{
					splittedWidget = new SplitWidgetContainer(widget as IWidgetContainer, splittedWidget, splittedWidgetIndex);
				}
				splittedWidgetIndex = num2;
				if (!isBlockContentControlChild & isWidgetsRemoved)
				{
					m_curWidgetIndex--;
				}
				if (!isWidgetsRemoved)
				{
					isWidgetsRemoved = flag2;
				}
				if (flag3)
				{
					return flag3;
				}
			}
			else
			{
				if (!(widget.LayoutInfo is ParagraphLayoutInfo) || base.IsForceFitLayout || !widget.LayoutInfo.IsKeepWithNext || !CheckKeepWithNextForHiddenPara(widget as IEntity) || !m_bAtLastOneChildFitted || ((widget as WParagraph).Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && (widget as WParagraph).IsInCell))
				{
					return true;
				}
				UpdateFootnoteWidgets(lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1]);
				if (DocumentLayouter.IsUpdatingTOC && IsLastTOCParagraph(lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1].Widget as Entity))
				{
					isLastTocParagraphRemoved = true;
				}
				if (isBlockContentControlChild)
				{
					splittedWidget = lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1].Widget;
				}
				RemoveBehindWidgets(lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1]);
				lwtWidget.ChildWidgets.RemoveAt(lwtWidget.ChildWidgets.Count - 1);
				isWidgetsRemoved = true;
				if (isBlockContentControlChild)
				{
					splittedWidgetIndex--;
				}
				else
				{
					m_curWidgetIndex--;
				}
			}
		}
		return false;
	}

	private void IsEndOfTOC()
	{
		if (DocumentLayouter.IsUpdatingTOC && (m_lcOperator as Layouter).LastTOCParagraph != null && (m_lcOperator as Layouter).LastTOCParagraph.ParagraphFormat.KeepFollow && IsEndOfTocParagraphLayouted())
		{
			DocumentLayouter.IsEndUpdateTOC = true;
		}
	}

	private bool IsEndOfTocParagraphLayouted()
	{
		foreach (KeyValuePair<Entity, int> tOCEntryPageNumber in (m_lcOperator as Layouter).TOCEntryPageNumbers)
		{
			if (IsLastTOCParagraph(tOCEntryPageNumber.Key))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsLastTOCParagraph(Entity entity)
	{
		if (entity is WParagraph)
		{
			return entity == (m_lcOperator as Layouter).LastTOCParagraph;
		}
		if (entity is WTableRow)
		{
			foreach (WTableCell cell in (entity as WTableRow).Cells)
			{
				foreach (WParagraph paragraph in cell.Paragraphs)
				{
					if (paragraph == (m_lcOperator as Layouter).LastTOCParagraph)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void CommitWithKeepWithNexForWord2013Format(ref IWidget splittedWidget, WSection section, ref bool isLastTocParagraphRemoved)
	{
		int columnIndex = base.DrawingContext.GetColumnIndex(section, m_ltWidget.Bounds);
		bool flag = columnIndex == 0 && IsNeedToRemoveItems(m_ltWidget);
		bool isWidgetsRemoved = false;
		int splittedWidgetIndex = 0;
		if ((columnIndex == 0 && flag) || columnIndex != 0)
		{
			RemoveItemsFromltWidgets(ref splittedWidget, ref isLastTocParagraphRemoved, m_ltWidget, isBlockContentControlChild: false, ref isWidgetsRemoved, ref splittedWidgetIndex);
		}
		else if (!(m_ltWidget.Widget is BlockContentControl))
		{
			DocumentLayouter.IsEndPage = true;
		}
	}

	private bool IsNeedToRemoveItems(LayoutedWidget ltWidget)
	{
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			IWidget widget = ltWidget.ChildWidgets[i].Widget;
			if (widget is WTable && !(widget as WTable).TableFormat.WrapTextAround && !(widget as WTable).IsInCell)
			{
				LayoutedWidget layoutedWidget = ltWidget.ChildWidgets[i];
				for (int j = 0; layoutedWidget.ChildWidgets.Count > j; j++)
				{
					if (!layoutedWidget.ChildWidgets[j].Widget.LayoutInfo.IsKeepWithNext)
					{
						return true;
					}
				}
				continue;
			}
			if (widget is BlockContentControl)
			{
				LayoutedWidget ltWidget2 = ltWidget.ChildWidgets[i];
				if (IsNeedToRemoveItems(ltWidget2))
				{
					return true;
				}
				continue;
			}
			if (widget.LayoutInfo is ParagraphLayoutInfo && widget.LayoutInfo.IsKeepWithNext)
			{
				int num = 3;
				WParagraph wParagraph = ((widget is WParagraph) ? (widget as WParagraph) : ((!(widget is SplitWidgetContainer)) ? null : (((widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) ? ((widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null)));
				if (wParagraph != null && !wParagraph.ParagraphFormat.WidowControl)
				{
					num--;
				}
				if (wParagraph != null && ltWidget.ChildWidgets[i].ChildWidgets.Count > num && !wParagraph.ParagraphFormat.Keep)
				{
					return true;
				}
				continue;
			}
			return true;
		}
		return false;
	}

	private bool RemoveItemsFromltWidgets(ref IWidget splittedWidget, ref bool isLastTocParagraphRemoved, LayoutedWidget lwtWidget, bool isBlockContentControlChild, ref bool isWidgetsRemoved, ref int splittedWidgetIndex)
	{
		while (lwtWidget.ChildWidgets.Count > 0 && !(m_lcOperator as Layouter).IsLayoutingHeaderFooter)
		{
			bool flag = false;
			IWidget widget = lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1].Widget;
			if (widget is WTable && !(widget as WTable).TableFormat.WrapTextAround && !(widget as WTable).IsInCell)
			{
				LayoutedWidget layoutedWidget = lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1];
				int rowCount = layoutedWidget.ChildWidgets.Count;
				int num = ((layoutedWidget.ChildWidgets.Count > 0) ? StartRowIndex(layoutedWidget, ref rowCount) : 0);
				while (layoutedWidget.ChildWidgets.Count > 0 && layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget.LayoutInfo.IsKeepWithNext)
				{
					if (IsLastTOCParagraph(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget as Entity))
					{
						isLastTocParagraphRemoved = true;
					}
					RemoveBehindWidgets(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1]);
					layoutedWidget.ChildWidgets.RemoveAt(layoutedWidget.ChildWidgets.Count - 1);
					isWidgetsRemoved = true;
					flag = true;
					rowCount--;
				}
				if (flag)
				{
					if (layoutedWidget.ChildWidgets.Count > 0)
					{
						splittedWidget = new SplitTableWidget(layoutedWidget.Widget as ITableWidget, num + rowCount + 1);
						m_ltState = LayoutState.Splitted;
						if (isBlockContentControlChild)
						{
							splittedWidgetIndex--;
						}
						else
						{
							m_curWidgetIndex--;
						}
						return true;
					}
					RemoveBehindWidgets(lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1]);
					lwtWidget.ChildWidgets.RemoveAt(lwtWidget.ChildWidgets.Count - 1);
					bool flag2 = false;
					if (isBlockContentControlChild)
					{
						splittedWidget = widget;
						splittedWidgetIndex--;
						continue;
					}
					while (!flag2)
					{
						m_curWidgetIndex--;
						if ((CurrentChildWidget is WTable && (layoutedWidget.Widget as WTable).Index == (CurrentChildWidget as WTable).Index) || (CurrentChildWidget is SplitTableWidget && (layoutedWidget.Widget as WTable).Index == ((CurrentChildWidget as SplitTableWidget).TableWidget as WTable).Index))
						{
							flag2 = true;
						}
					}
					continue;
				}
				return true;
			}
			if (widget is BlockContentControl)
			{
				LayoutedWidget layoutedWidget2 = lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1];
				if (widget.LayoutInfo.IsKeepWithNext)
				{
					RemoveBehindWidgets(layoutedWidget2);
					lwtWidget.ChildWidgets.Remove(layoutedWidget2);
					isWidgetsRemoved = true;
					if (isBlockContentControlChild)
					{
						splittedWidget = widget;
						splittedWidgetIndex--;
					}
					else
					{
						m_curWidgetIndex--;
					}
					continue;
				}
				int num2 = splittedWidgetIndex;
				bool flag3 = isWidgetsRemoved;
				splittedWidgetIndex = layoutedWidget2.ChildWidgets.Count;
				isWidgetsRemoved = false;
				bool flag4 = layoutedWidget2.ChildWidgets.Count <= 0 || RemoveItemsFromltWidgets(ref splittedWidget, ref isLastTocParagraphRemoved, layoutedWidget2, isBlockContentControlChild: true, ref isWidgetsRemoved, ref splittedWidgetIndex);
				if (splittedWidget != null && splittedWidget is SplitWidgetContainer)
				{
					splittedWidgetIndex--;
				}
				if ((splittedWidget != null) & isWidgetsRemoved)
				{
					splittedWidget = new SplitWidgetContainer(widget as IWidgetContainer, splittedWidget, splittedWidgetIndex);
				}
				splittedWidgetIndex = num2;
				if (!isBlockContentControlChild & isWidgetsRemoved)
				{
					m_curWidgetIndex--;
				}
				if (!isWidgetsRemoved)
				{
					isWidgetsRemoved = flag3;
				}
				if (!flag4)
				{
					continue;
				}
				return flag4;
			}
			if (widget.LayoutInfo is ParagraphLayoutInfo && widget.LayoutInfo.IsKeepWithNext && (!(widget is WParagraph) || !IsInFrame(widget as WParagraph) || (widget as WParagraph).ParagraphFormat == null || ((widget as WParagraph).ParagraphFormat.FrameVerticalPos != 1 && (widget as WParagraph).ParagraphFormat.FrameVerticalPos != 0)))
			{
				LayoutedWidget layoutedWidget3 = lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1];
				int num3 = 3;
				WParagraph wParagraph = ((widget is WParagraph) ? (widget as WParagraph) : ((!(widget is SplitWidgetContainer)) ? null : (((widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) ? ((widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null)));
				if (wParagraph != null && !wParagraph.ParagraphFormat.WidowControl)
				{
					num3--;
				}
				if (layoutedWidget3.ChildWidgets.Count <= num3 || (wParagraph != null && wParagraph.ParagraphFormat.Keep))
				{
					UpdateFootnoteWidgets(layoutedWidget3);
					if (DocumentLayouter.IsUpdatingTOC && IsLastTOCParagraph(lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1].Widget as Entity))
					{
						isLastTocParagraphRemoved = true;
					}
					RemoveBehindWidgets(lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1]);
					(m_lcOperator as Layouter).RemovedWidgetsHeight += lwtWidget.ChildWidgets[lwtWidget.ChildWidgets.Count - 1].Bounds.Height;
					if (isBlockContentControlChild)
					{
						splittedWidget = layoutedWidget3.ChildWidgets[layoutedWidget3.ChildWidgets.Count - 1].Widget;
					}
					lwtWidget.ChildWidgets.RemoveAt(lwtWidget.ChildWidgets.Count - 1);
					isWidgetsRemoved = true;
					bool flag5 = false;
					if (isBlockContentControlChild)
					{
						splittedWidgetIndex--;
						continue;
					}
					while (!flag5)
					{
						m_curWidgetIndex--;
						if ((CurrentChildWidget is WParagraph && (layoutedWidget3.Widget as WParagraph).Index == (CurrentChildWidget as WParagraph).Index) || (CurrentChildWidget is SplitWidgetContainer && ((layoutedWidget3.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).Index == ((CurrentChildWidget as SplitWidgetContainer).RealWidgetContainer as WParagraph).Index))
						{
							flag5 = true;
						}
					}
					continue;
				}
				int num4 = 0;
				while (true)
				{
					UpdateFootnoteWidgets(layoutedWidget3.ChildWidgets[layoutedWidget3.ChildWidgets.Count - 1]);
					layoutedWidget3.ChildWidgets[layoutedWidget3.ChildWidgets.Count - 1].InitLayoutInfo(resetTabLayoutInfo: false);
					num4++;
					if (num4 == num3 - 1)
					{
						break;
					}
					RemoveBehindWidgets(layoutedWidget3.ChildWidgets[layoutedWidget3.ChildWidgets.Count - 1]);
					layoutedWidget3.ChildWidgets.RemoveAt(layoutedWidget3.ChildWidgets.Count - 1);
					if (isBlockContentControlChild)
					{
						splittedWidgetIndex--;
					}
					else
					{
						m_curWidgetIndex--;
					}
				}
				splittedWidget = layoutedWidget3.ChildWidgets[layoutedWidget3.ChildWidgets.Count - 1].Widget as SplitWidgetContainer;
				RemoveBehindWidgets(layoutedWidget3.ChildWidgets[layoutedWidget3.ChildWidgets.Count - 1]);
				layoutedWidget3.ChildWidgets.RemoveAt(layoutedWidget3.ChildWidgets.Count - 1);
				isWidgetsRemoved = true;
				if (num3 == 2 && !isBlockContentControlChild)
				{
					m_curWidgetIndex--;
				}
				else if (isBlockContentControlChild)
				{
					splittedWidgetIndex--;
				}
				return true;
			}
			return true;
		}
		return false;
	}

	private int StartRowIndex(LayoutedWidget tableWidget, ref int rowCount)
	{
		for (int i = 0; i < tableWidget.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = tableWidget.ChildWidgets[i];
			if (layoutedWidget.Widget is WTableRow && (layoutedWidget.Widget as WTableRow).IsHeader)
			{
				rowCount--;
				continue;
			}
			return (tableWidget.ChildWidgets[i].Widget as WTableRow).Index;
		}
		return (tableWidget.ChildWidgets[0].Widget as WTableRow).Index;
	}

	private bool IsAllTextBodyItemHavingKeepWithNext()
	{
		for (int i = 0; i < m_ltWidget.ChildWidgets.Count; i++)
		{
			if (!m_ltWidget.ChildWidgets[i].Widget.LayoutInfo.IsKeepWithNext)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsNeedToCommitKeepWithNext()
	{
		bool result = false;
		if (m_ltWidget.ChildWidgets.Count > 0)
		{
			int num = m_ltWidget.ChildWidgets.Count - 1;
			while (num >= 0)
			{
				IWidget widget = m_ltWidget.ChildWidgets[num].Widget;
				if (!(widget is WTable) || !(widget as WTable).TableFormat.WrapTextAround)
				{
					if (!widget.LayoutInfo.IsKeepWithNext)
					{
						if (!(widget is BlockContentControl))
						{
							result = true;
							break;
						}
						bool isKeepWithNext = false;
						if (!SetKeepWithNextForBlockContentControl(widget, ref isKeepWithNext))
						{
							result = true;
							break;
						}
						widget.LayoutInfo.IsKeepWithNext = true;
					}
					if (base.IsForceFitLayout)
					{
						if (!DocumentLayouter.IsFirstLayouting)
						{
							result = widget.LayoutInfo.IsKeepWithNext;
						}
						break;
					}
					num--;
					continue;
				}
				result = true;
				break;
			}
			WSection wSection = ((m_ltWidget.Widget is WSection) ? (m_ltWidget.Widget as WSection) : null);
			if (wSection != null && wSection.BreakCode == SectionBreakCode.NoBreak && wSection.GetIndexInOwnerCollection() > 0 && IsAllTextBodyItemHavingKeepWithNext())
			{
				result = true;
			}
		}
		return result;
	}

	internal bool SetKeepWithNextForBlockContentControl(IWidget widget, ref bool isKeepWithNext)
	{
		BlockContentControl blockContentControl = widget as BlockContentControl;
		for (int num = blockContentControl.ChildEntities.Count - 1; num >= 0; num--)
		{
			Entity entity = blockContentControl.ChildEntities[num];
			if (entity is WParagraph)
			{
				if (!(entity as WParagraph).m_layoutInfo.IsKeepWithNext)
				{
					isKeepWithNext = false;
					return false;
				}
				isKeepWithNext = true;
			}
			else if (entity is BlockContentControl)
			{
				IWidget widget2 = entity as IWidget;
				SetKeepWithNextForBlockContentControl(widget2, ref isKeepWithNext);
				if (!isKeepWithNext)
				{
					return false;
				}
				widget2.LayoutInfo.IsKeepWithNext = true;
			}
			else
			{
				if (!(entity is WTable))
				{
					isKeepWithNext = false;
					return false;
				}
				if (!(entity as WTable).m_layoutInfo.IsKeepWithNext)
				{
					isKeepWithNext = false;
					return false;
				}
				isKeepWithNext = true;
			}
		}
		return isKeepWithNext;
	}
}
