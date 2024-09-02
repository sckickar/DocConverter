using System;
using System.Collections.Generic;
using System.Globalization;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal abstract class LayoutContext
{
	internal const float DEF_LEFT_MIN_WIDTH_SQUARE = 18f;

	internal const float DEF_LEFT_MIN_WIDTH_2013_TIGHTANDTHROW = 18f;

	internal const float DEF_LEFT_MIN_WIDTH_TIGHTANDTHROW = 9f;

	internal const float DEF_MIN_WIDTH_SQUARE = 18f;

	internal const float DEF_MIN_WIDTH_2013_TIGHTANDTHROW = 17.6f;

	internal const float DEF_MIN_WIDTH_TIGHTANDTHROW = 8f;

	internal const float MAX_WIDTH = 1584f;

	private const float BottomOverlapDifferenceForTightAndThroughWrappingStyle = 2f;

	internal LayoutState m_ltState;

	protected IWidget m_sptWidget;

	protected IWidget m_notFittedWidget;

	protected IWidget m_LineNumberWidget;

	protected IWidget m_widget;

	protected LayoutedWidget m_ltWidget;

	internal bool m_bSkipAreaSpacing;

	protected LayoutArea m_layoutArea;

	protected ILCOperator m_lcOperator;

	private float m_clientLayoutAreaRight;

	protected byte m_bFlags = 16;

	public IWidget SplittedWidget
	{
		get
		{
			return m_sptWidget;
		}
		set
		{
			m_sptWidget = value;
		}
	}

	public LayoutState State => m_ltState;

	public ILayoutInfo LayoutInfo => m_widget.LayoutInfo;

	public LayoutArea LayoutArea => m_layoutArea;

	public DrawingContext DrawingContext => m_lcOperator.DrawingContext;

	public float BoundsPaddingRight
	{
		get
		{
			float result = 0f;
			if (m_widget.LayoutInfo is ILayoutSpacingsInfo)
			{
				result = (m_widget.LayoutInfo as ILayoutSpacingsInfo).Paddings.Right + (m_widget.LayoutInfo as ILayoutSpacingsInfo).Margins.Right;
			}
			return result;
		}
	}

	public float BoundsMarginBottom
	{
		get
		{
			float result = 0f;
			if (m_widget.LayoutInfo is ILayoutSpacingsInfo)
			{
				result = (m_widget.LayoutInfo as ILayoutSpacingsInfo).Margins.Bottom;
			}
			return result;
		}
	}

	public IWidget Widget
	{
		get
		{
			return m_widget;
		}
		set
		{
			m_widget = value;
		}
	}

	public bool IsVerticalNotFitted
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsInnerLayouting
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool IsAreaUpdated
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsTabStopBeyondRightMarginExists
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool IsNeedToWrap
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	protected bool IsForceFitLayout
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal float ClientLayoutAreaRight
	{
		get
		{
			return m_clientLayoutAreaRight;
		}
		set
		{
			m_clientLayoutAreaRight = value;
		}
	}

	public LayoutContext(IWidget widget, ILCOperator lcOperator, bool isForceFitLayout)
	{
		m_widget = widget;
		m_sptWidget = widget;
		m_lcOperator = lcOperator;
		IsForceFitLayout = isForceFitLayout;
	}

	public abstract LayoutedWidget Layout(RectangleF rect);

	public bool IsEnsureSplitted()
	{
		if (State == LayoutState.Splitted)
		{
			return SplittedWidget != null;
		}
		return false;
	}

	protected virtual void DoLayoutAfter()
	{
	}

	internal void LayoutFootnote(WFootnote footnote, LayoutedWidget currLtWidget, bool isFootnoteRefrencedlineLayouted)
	{
		if (!IsNeedToWrap)
		{
			return;
		}
		float height = 0f;
		WParagraph ownerParagraphValue = footnote.GetOwnerParagraphValue();
		bool isClippedLine = false;
		bool isTextInLine = false;
		bool isFirstLineOfParagraph = false;
		bool flag = false;
		if (currLtWidget.ChildWidgets.Count > 0)
		{
			isFirstLineOfParagraph = ownerParagraphValue.IsFirstLine(currLtWidget.ChildWidgets[0]);
			flag = ownerParagraphValue.IsLastLine(currLtWidget.ChildWidgets[currLtWidget.ChildWidgets.Count - 1]);
		}
		IStringWidget lastTextWidget = null;
		LayoutedWidget layoutedWidget = null;
		double maxHeight;
		double maxAscent;
		double maxTextHeight;
		double maxTextAscent;
		double maxTextDescent;
		float maxY;
		double maxAscentOfLoweredPos;
		bool containsInlinePicture;
		bool isAllWordsContainLoweredPos;
		if (isFootnoteRefrencedlineLayouted)
		{
			layoutedWidget = currLtWidget.Owner.ChildWidgets[currLtWidget.Owner.ChildWidgets.Count - 1];
			flag = ownerParagraphValue.IsLastLine(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1]);
			layoutedWidget.CalculateMaxChildWidget(DocumentLayouter.DrawingContext, ownerParagraphValue, isFirstLineOfParagraph: false, flag, out maxHeight, out maxAscent, out maxTextHeight, out maxTextAscent, out maxTextDescent, out maxY, out maxAscentOfLoweredPos, out lastTextWidget, out isClippedLine, out isTextInLine, out containsInlinePicture, out isAllWordsContainLoweredPos);
		}
		else
		{
			currLtWidget.CalculateMaxChildWidget(DocumentLayouter.DrawingContext, ownerParagraphValue, isFirstLineOfParagraph, flag, out maxHeight, out maxAscent, out maxTextHeight, out maxTextAscent, out maxTextDescent, out maxY, out maxAscentOfLoweredPos, out lastTextWidget, out isClippedLine, out isTextInLine, out containsInlinePicture, out isAllWordsContainLoweredPos);
		}
		float num = Math.Abs(ownerParagraphValue.ParagraphFormat.LineSpacing);
		if (maxHeight != 0.0 || maxTextHeight != 0.0)
		{
			switch (ownerParagraphValue.ParagraphFormat.LineSpacingRule)
			{
			case LineSpacingRule.Exactly:
				maxHeight = Math.Abs(num);
				break;
			case LineSpacingRule.AtLeast:
				if (maxHeight < (double)num)
				{
					maxHeight = num;
				}
				break;
			case LineSpacingRule.Multiple:
				if (num != 12f)
				{
					maxHeight += maxTextHeight * (double)(num / 12f) - maxTextHeight;
				}
				break;
			}
		}
		float num2 = 0f;
		ILayoutInfo layoutInfo = ownerParagraphValue.m_layoutInfo;
		if (layoutInfo is ParagraphLayoutInfo)
		{
			num2 = ((!isFootnoteRefrencedlineLayouted) ? (currLtWidget.Bounds.Y + (float)maxHeight + (layoutInfo as ParagraphLayoutInfo).Margins.Bottom + currLtWidget.m_footnoteHeight) : (layoutedWidget.Bounds.Y + (float)maxHeight + (layoutInfo as ParagraphLayoutInfo).Margins.Bottom + currLtWidget.m_footnoteHeight));
		}
		float num3 = m_layoutArea.ClientActiveArea.Bottom - num2;
		bool flag2 = ownerParagraphValue.GetOwnerEntity() is WTableCell wTableCell && (ownerParagraphValue.IsExactlyRowHeight() || IsExactlyRowVerticalMergeStartCell(wTableCell) || (wTableCell.m_layoutInfo != null && wTableCell.m_layoutInfo.IsVerticalText));
		if (flag2 || (ownerParagraphValue.ParagraphFormat.IsInFrame() && !ownerParagraphValue.IsAtleastFrameHeight() && ownerParagraphValue.ParagraphFormat.FrameHeight != 0f))
		{
			num3 = GetFootNoteLayoutingHeight();
		}
		if ((m_lcOperator as Layouter).IsNeedToRestartFootnote)
		{
			(m_lcOperator as Layouter).IsLayoutingFootnote = true;
			LayoutFootnoteTextBody(footnote.Document.Footnotes.Separator, ref height, num3, isFootnoteRefrencedlineLayouted);
			if (flag2)
			{
				num3 -= height;
			}
			(m_lcOperator as Layouter).IsLayoutingFootnote = false;
			(m_lcOperator as Layouter).IsNeedToRestartFootnote = false;
		}
		float num4 = 0f;
		if (footnote.TextBody.LastParagraph != null)
		{
			num4 = DrawingContext.MeasureString(" ", footnote.TextBody.LastParagraph.BreakCharacterFormat.GetFontToRender(FontScriptType.English), null, FontScriptType.English).Height;
		}
		if (num4 > num3)
		{
			if (footnote.Owner is WParagraph && (footnote.Owner as WParagraph).IsInCell)
			{
				if (!(((footnote.Owner as WParagraph).GetOwnerEntity() as WTableCell).OwnerRow.m_layoutInfo as RowLayoutInfo).IsFootnoteSplitted)
				{
					(m_lcOperator as Layouter).FootnoteSplittedWidgets.Add(new SplitWidgetContainer(footnote.TextBody, footnote.TextBody.Items[0], 0));
				}
			}
			else
			{
				(m_lcOperator as Layouter).FootnoteSplittedWidgets.Add(new SplitWidgetContainer(footnote.TextBody, footnote.TextBody.Items[0], 0));
			}
			if ((m_lcOperator as Layouter).IsNeedToRestartFootnoteID)
			{
				DocumentLayouter.m_footnoteIDRestartEachPage = 1;
				(m_lcOperator as Layouter).IsNeedToRestartFootnoteID = false;
			}
			if (ownerParagraphValue.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && !flag)
			{
				m_ltState = LayoutState.NotFitted;
			}
			WParagraph ownerParagraphValue2 = footnote.GetOwnerParagraphValue();
			Entity entity = null;
			if (ownerParagraphValue2 != null)
			{
				entity = GetBaseEntity(ownerParagraphValue2);
			}
			if (entity != null && entity is WSection && (entity as WSection).PageSetup.RestartIndexForFootnotes == FootnoteRestartIndex.RestartForEachPage)
			{
				(footnote.m_layoutInfo as FootnoteLayoutInfo).FootnoteID = (footnote.m_layoutInfo as FootnoteLayoutInfo).GetFootnoteID(footnote, DocumentLayouter.m_footnoteIDRestartEachPage++);
				if (footnote.CustomMarkerIsSymbol || footnote.CustomMarker != string.Empty)
				{
					DocumentLayouter.m_footnoteIDRestartEachPage--;
				}
			}
		}
		else
		{
			(m_lcOperator as Layouter).IsLayoutingFootnote = true;
			LayoutFootnoteTextBody(footnote.TextBody, ref height, num3, isFootnoteRefrencedlineLayouted);
			(m_lcOperator as Layouter).IsLayoutingFootnote = false;
		}
		currLtWidget.m_footnoteHeight += height;
		(footnote.m_layoutInfo as FootnoteLayoutInfo).FootnoteHeight = height;
		if ((m_lcOperator as Layouter).FootnoteWidgets.Count < 2)
		{
			(m_lcOperator as Layouter).FootnoteWidgets.Clear();
			(m_lcOperator as Layouter).FootNoteSectionIndex.Clear();
		}
	}

	internal bool IsNeedToConsiderAdjustValues(ref float adjustingValue, WParagraph paragraph, TextWrappingStyle textWrappingStyle, int index)
	{
		Layouter layouter = m_lcOperator as Layouter;
		WSection wSection = GetBaseEntity(paragraph) as WSection;
		int num;
		if (textWrappingStyle == TextWrappingStyle.TopAndBottom && IsWord2013(paragraph.Document) && layouter.FloatingItems[index].FloatingEntity is WTextBox)
		{
			num = ((wSection != null) ? 1 : 0);
			if (num != 0)
			{
				WTextBox wTextBox = layouter.FloatingItems[index].FloatingEntity as WTextBox;
				if (wTextBox.CharacterFormat.Border != null && wTextBox.IsShape)
				{
					Shape shape = wTextBox.Shape;
					adjustingValue = ((shape.LineFormat != null) ? AdjustingValueToWrap(wSection.PageSetup.Margins.Left, shape.LineFormat.Weight, shape.LineFormat.Line) : 0f);
				}
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	internal float AdjustingValueToWrap(float margin, float borderValue, bool hasBorder)
	{
		string[] array = margin.ToString(CultureInfo.InvariantCulture).Split('.');
		if (hasBorder)
		{
			float num = 0.8f;
			string text = array[0];
			for (int i = 0; i < text.Length; i++)
			{
				int num2 = int.Parse(text[i].ToString());
				for (int j = 0; j < num2; j++)
				{
					if (Math.Round(num, 2) == 0.8)
					{
						num += 1f;
					}
					else if (Math.Round(num, 2) == 1.8)
					{
						num -= 0.5f;
					}
					else if (Math.Round(num, 2) == 1.3)
					{
						num -= 0.5f;
					}
				}
			}
			if (array.Length == 2)
			{
				float num3 = float.Parse("0." + array[1]);
				if ((double)(num + num3) < 2.05)
				{
					num += num3;
				}
				else
				{
					num3 -= 2.05f - num;
					num = 2.05f;
					if ((double)num3 >= 0.25)
					{
						num3 -= 0.25f;
						num = num - 1.25f + num3;
					}
					else
					{
						num = 2.05f - 5f * num3;
					}
				}
			}
			if ((double)borderValue < 2.25)
			{
				return num;
			}
			int num4 = (int)(((double)borderValue - 2.25) / 1.5) + 1;
			return 1.5f * (float)num4 + num;
		}
		float result = 0f;
		if (margin > 10f)
		{
			margin -= 10f;
			if (margin % 15f == 0f)
			{
				result = 0.3f;
			}
			return result;
		}
		return 0f;
	}

	private float GetFootNoteLayoutingHeight()
	{
		Layouter layouter = m_lcOperator as Layouter;
		float num = layouter.ClientLayoutArea.Bottom - m_layoutArea.ClientActiveArea.Bottom;
		if (layouter.FootnoteWidgets.Count > 0)
		{
			num -= layouter.FootnoteWidgets[layouter.FootnoteWidgets.Count - 1].Bounds.Bottom - layouter.FootnoteWidgets[0].Bounds.Y;
		}
		return num;
	}

	private bool IsExactlyRowVerticalMergeStartCell(WTableCell cell)
	{
		if ((m_lcOperator as Layouter).IsLayoutingVerticalMergeStartCell)
		{
			return cell.OwnerRow.HeightType == TableRowHeightType.Exactly;
		}
		return false;
	}

	internal WTextBox IsEntityOwnerIsWTextbox(Entity entity)
	{
		while (entity != null)
		{
			if (entity.EntityType == EntityType.HeaderFooter || entity.EntityType == EntityType.Section || entity.Owner == null)
			{
				return null;
			}
			entity = entity.Owner;
			if (entity is WTextBox)
			{
				return entity as WTextBox;
			}
		}
		return null;
	}

	internal bool IsWord2013(WordDocument document)
	{
		return document.Settings.CompatibilityMode == CompatibilityMode.Word2013;
	}

	internal bool IsNotWord2013Jusitfy(WParagraph paragraph)
	{
		if (paragraph != null && IsWord2013(paragraph.Document) && paragraph.m_layoutInfo is ParagraphLayoutInfo)
		{
			if ((paragraph.m_layoutInfo as ParagraphLayoutInfo).Justification != HAlignment.Justify)
			{
				return (paragraph.m_layoutInfo as ParagraphLayoutInfo).Justification != HAlignment.Distributed;
			}
			return false;
		}
		return true;
	}

	internal float GetTotalTopMarginAndPaddingValues(WTable table)
	{
		float num = 0f;
		float num2 = 0f;
		Entity owner = table.Owner;
		while (owner is WTable || owner is WTableRow || owner is WTableCell)
		{
			if (owner is WTableCell)
			{
				ILayoutInfo layoutInfo = (owner as WTableCell).m_layoutInfo;
				if (layoutInfo is CellLayoutInfo)
				{
					num += (layoutInfo as CellLayoutInfo).TopPadding;
					num2 += (layoutInfo as CellLayoutInfo).Margins.Top;
				}
			}
			owner = owner.Owner;
		}
		return num + num2;
	}

	internal void RemoveBehindWidgets(LayoutedWidget ltWidget)
	{
		if ((m_lcOperator as Layouter).BehindWidgets.Count == 0)
		{
			return;
		}
		switch ((ltWidget.Widget is SplitWidgetContainer) ? ((ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer as Entity).EntityType : (ltWidget.Widget as Entity).EntityType)
		{
		case EntityType.Paragraph:
			if (ltWidget.ChildWidgets.Count > 0 && ltWidget.ChildWidgets[0].Widget is WParagraph)
			{
				RemoveFromLayoutedParagraph(ltWidget);
			}
			else
			{
				RemoveFromLayoutedLine(ltWidget);
			}
			break;
		case EntityType.Table:
			RemoveFromLayoutedTable(ltWidget);
			break;
		case EntityType.TableRow:
			RemoveFromLayoutedRow(ltWidget);
			break;
		}
	}

	private void RemoveFromLayoutedTable(LayoutedWidget ltTable)
	{
		foreach (LayoutedWidget childWidget in ltTable.ChildWidgets)
		{
			RemoveFromLayoutedRow(childWidget);
		}
	}

	private void RemoveFromLayoutedRow(LayoutedWidget ltRow)
	{
		foreach (LayoutedWidget childWidget in ltRow.ChildWidgets)
		{
			foreach (LayoutedWidget childWidget2 in childWidget.ChildWidgets[0].ChildWidgets)
			{
				RemoveFromLayoutedParagraph(childWidget2);
			}
		}
	}

	private void RemoveFromLayoutedParagraph(LayoutedWidget ltWidget)
	{
		foreach (LayoutedWidget childWidget in ltWidget.ChildWidgets)
		{
			RemoveFromLayoutedLine(childWidget);
		}
	}

	private void RemoveFromLayoutedLine(LayoutedWidget lineLtWidget)
	{
		Layouter layouter = m_lcOperator as Layouter;
		if (layouter.BehindWidgets.Count == 0)
		{
			return;
		}
		foreach (LayoutedWidget childWidget in lineLtWidget.ChildWidgets)
		{
			if (childWidget.IsBehindWidget() && layouter.BehindWidgets.Contains(childWidget))
			{
				layouter.BehindWidgets.Remove(childWidget);
			}
		}
	}

	internal void LayoutEndnote(WFootnote endnote, LayoutedWidget currLtWidget)
	{
		if (!IsNeedToWrap)
		{
			return;
		}
		float height = 0f;
		float num = 0f;
		ILayoutInfo layoutInfo = endnote.m_layoutInfo;
		num = currLtWidget.Bounds.Bottom + currLtWidget.m_endnoteHeight;
		if ((m_lcOperator as Layouter).IsNeedToRestartEndnote)
		{
			LayoutEndnoteTextBody(endnote.Document.Endnotes.Separator, ref height, m_layoutArea.ClientActiveArea.Bottom - num);
			(m_lcOperator as Layouter).IsNeedToRestartEndnote = false;
		}
		float num2 = 0f;
		if (endnote.TextBody.LastParagraph != null)
		{
			num2 = DrawingContext.MeasureString(" ", endnote.TextBody.LastParagraph.BreakCharacterFormat.GetFontToRender(FontScriptType.English), null, FontScriptType.English).Height;
		}
		if (num2 > m_layoutArea.ClientActiveArea.Bottom - num)
		{
			(m_lcOperator as Layouter).EndnoteSplittedWidgets.Add(new SplitWidgetContainer(endnote.TextBody, endnote.TextBody.Items[0], 0));
			if ((m_lcOperator as Layouter).IsNeedToRestartEndnote)
			{
				DocumentLayouter.m_footnoteIDRestartEachPage = 1;
				(m_lcOperator as Layouter).IsNeedToRestartEndnote = false;
			}
			WParagraph ownerParagraphValue = endnote.GetOwnerParagraphValue();
			Entity entity = null;
			if (ownerParagraphValue != null)
			{
				entity = GetBaseEntity(ownerParagraphValue);
			}
			if (entity != null && entity is WSection && (entity as WSection).PageSetup.RestartIndexForFootnotes == FootnoteRestartIndex.RestartForEachPage && layoutInfo is FootnoteLayoutInfo)
			{
				(layoutInfo as FootnoteLayoutInfo).FootnoteID = (layoutInfo as FootnoteLayoutInfo).GetFootnoteID(endnote, DocumentLayouter.m_endnoteIDRestartEachPage++);
				if (endnote.CustomMarkerIsSymbol || endnote.CustomMarker != string.Empty)
				{
					DocumentLayouter.m_endnoteIDRestartEachPage--;
				}
			}
		}
		else
		{
			LayoutEndnoteTextBody(endnote.TextBody, ref height, m_layoutArea.ClientActiveArea.Bottom - num);
		}
		currLtWidget.m_endnoteHeight += height;
		if (layoutInfo is FootnoteLayoutInfo)
		{
			(layoutInfo as FootnoteLayoutInfo).Endnoteheight = height;
		}
		if ((m_lcOperator as Layouter).EndnoteWidgets.Count < 2)
		{
			(m_lcOperator as Layouter).EndnoteWidgets.Clear();
			(m_lcOperator as Layouter).EndNoteSectionIndex.Clear();
		}
	}

	internal void LayoutFootnoteTextBody(IWidgetContainer widgetContainer, ref float height, float clientHeight, bool referencedLineIsLayouted)
	{
		bool canSplitbyCharacter = (m_lcOperator as Layouter).m_canSplitbyCharacter;
		bool canSplitByTab = (m_lcOperator as Layouter).m_canSplitByTab;
		bool isFirstItemInLine = (m_lcOperator as Layouter).IsFirstItemInLine;
		List<float> list = new List<float>();
		if ((m_lcOperator as Layouter).m_lineSpaceWidths != null)
		{
			list.AddRange((m_lcOperator as Layouter).m_lineSpaceWidths);
		}
		float effectiveJustifyWidth = (m_lcOperator as Layouter).m_effectiveJustifyWidth;
		LayoutContext layoutContext = Create(widgetContainer, m_lcOperator, isForceFitLayout: false);
		float y = (m_lcOperator as Layouter).ClientLayoutArea.Y;
		if ((m_lcOperator as Layouter).FootnoteWidgets.Count > 0)
		{
			y = (m_lcOperator as Layouter).FootnoteWidgets[(m_lcOperator as Layouter).FootnoteWidgets.Count - 1].Bounds.Bottom;
		}
		if ((m_lcOperator as Layouter).FootnoteWidgets.Count == 1)
		{
			clientHeight -= (m_lcOperator as Layouter).FootnoteWidgets[0].Bounds.Height;
		}
		RectangleF rect = new RectangleF((m_lcOperator as Layouter).ClientLayoutArea.X, y, (m_lcOperator as Layouter).ClientLayoutArea.Width, clientHeight);
		layoutContext.ClientLayoutAreaRight = rect.Width;
		LayoutedWidget layoutedWidget = layoutContext.Layout(rect);
		(m_lcOperator as Layouter).ResetWordLayoutingFlags(canSplitbyCharacter, canSplitByTab, isFirstItemInLine, list, effectiveJustifyWidth);
		if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
		{
			return;
		}
		if (layoutContext.State == LayoutState.Splitted || layoutContext.State == LayoutState.NotFitted)
		{
			if (layoutContext.SplittedWidget is SplitWidgetContainer)
			{
				(m_lcOperator as Layouter).FootnoteSplittedWidgets.Add(layoutContext.SplittedWidget as SplitWidgetContainer);
				(m_lcOperator as Layouter).FootnoteWidgets.Add(layoutedWidget);
				height += layoutedWidget.Bounds.Height;
			}
			else if (layoutContext.SplittedWidget is WTextBody)
			{
				(m_lcOperator as Layouter).FootnoteSplittedWidgets.Add(new SplitWidgetContainer(widgetContainer, (layoutContext.SplittedWidget as WTextBody).Items[0], 0));
			}
			return;
		}
		WTextBody wTextBody = ((layoutedWidget.Widget is WTextBody) ? (layoutedWidget.Widget as WTextBody) : ((layoutedWidget.Widget is SplitWidgetContainer && (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WTextBody) ? ((layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WTextBody) : null));
		if (wTextBody != null && IsFootnoteSplitted(wTextBody))
		{
			(m_lcOperator as Layouter).FootnoteSplittedWidgets.Add(new SplitWidgetContainer(widgetContainer, wTextBody.Items[0], 0));
			return;
		}
		(m_lcOperator as Layouter).FootnoteWidgets.Add(layoutedWidget);
		if (wTextBody != null && wTextBody.Owner is WFootnote)
		{
			(wTextBody.Owner as WFootnote).IsLayouted = true;
		}
		height += layoutedWidget.Bounds.Height;
	}

	private bool IsFootnoteSplitted(WTextBody textBody)
	{
		WParagraph wParagraph = ((textBody.Owner is WFootnote) ? (textBody.Owner as WFootnote).GetOwnerParagraphValue() : null);
		if (wParagraph != null && wParagraph.IsInCell)
		{
			Entity ownerEntity = wParagraph.GetOwnerEntity();
			if (ownerEntity is WTableCell)
			{
				return (((ownerEntity as WTableCell).Owner as WTableRow).m_layoutInfo as RowLayoutInfo).IsFootnoteSplitted;
			}
		}
		return false;
	}

	internal void LayoutEndnoteTextBody(IWidgetContainer widgetContainer, ref float height, float clientHeight)
	{
		bool canSplitbyCharacter = (m_lcOperator as Layouter).m_canSplitbyCharacter;
		bool canSplitByTab = (m_lcOperator as Layouter).m_canSplitByTab;
		bool isFirstItemInLine = (m_lcOperator as Layouter).IsFirstItemInLine;
		List<float> list = new List<float>();
		if ((m_lcOperator as Layouter).m_lineSpaceWidths != null)
		{
			list.AddRange((m_lcOperator as Layouter).m_lineSpaceWidths);
		}
		float effectiveJustifyWidth = (m_lcOperator as Layouter).m_effectiveJustifyWidth;
		LayoutContext layoutContext = Create(widgetContainer, m_lcOperator, isForceFitLayout: false);
		float y = (m_lcOperator as Layouter).ClientLayoutArea.Y;
		if ((m_lcOperator as Layouter).EndnoteWidgets.Count > 0)
		{
			y = (m_lcOperator as Layouter).EndnoteWidgets[(m_lcOperator as Layouter).EndnoteWidgets.Count - 1].Bounds.Bottom;
		}
		if ((m_lcOperator as Layouter).EndnoteWidgets.Count == 1)
		{
			clientHeight -= (m_lcOperator as Layouter).EndnoteWidgets[0].Bounds.Height;
		}
		RectangleF rect = new RectangleF((m_lcOperator as Layouter).ClientLayoutArea.X, y, (m_lcOperator as Layouter).ClientLayoutArea.Width, clientHeight);
		LayoutedWidget layoutedWidget = layoutContext.Layout(rect);
		(m_lcOperator as Layouter).ResetWordLayoutingFlags(canSplitbyCharacter, canSplitByTab, isFirstItemInLine, list, effectiveJustifyWidth);
		if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
		{
			return;
		}
		if ((m_lcOperator as Layouter).EndnoteSplittedWidgets.Count == 0)
		{
			if (layoutContext.State == LayoutState.Splitted || layoutContext.State == LayoutState.NotFitted)
			{
				if (layoutContext.SplittedWidget is SplitWidgetContainer)
				{
					(m_lcOperator as Layouter).EndnoteSplittedWidgets.Add(layoutContext.SplittedWidget as SplitWidgetContainer);
					(m_lcOperator as Layouter).EndnoteWidgets.Add(layoutedWidget);
				}
				else if (layoutContext.SplittedWidget is WTextBody)
				{
					(m_lcOperator as Layouter).EndnoteSplittedWidgets.Add(new SplitWidgetContainer(widgetContainer, (layoutContext.SplittedWidget as WTextBody).Items[0], 0));
				}
			}
			else
			{
				(m_lcOperator as Layouter).EndnoteWidgets.Add(layoutedWidget);
			}
		}
		else if (layoutContext.SplittedWidget is SplitWidgetContainer)
		{
			if (layoutContext.State == LayoutState.Splitted && layoutContext.Widget is WTextBody)
			{
				(m_lcOperator as Layouter).EndnoteSplittedWidgets.Add(new SplitWidgetContainer(widgetContainer, (layoutContext.Widget as WTextBody).Items[0], 0));
			}
			else
			{
				(m_lcOperator as Layouter).EndnoteSplittedWidgets.Add(layoutContext.SplittedWidget as SplitWidgetContainer);
			}
		}
		else if (layoutContext.SplittedWidget is WTextBody)
		{
			(m_lcOperator as Layouter).EndnoteSplittedWidgets.Add(new SplitWidgetContainer(widgetContainer, (layoutContext.SplittedWidget as WTextBody).Items[0], 0));
		}
		height += layoutedWidget.Bounds.Height;
	}

	internal void AddLayoutWidgetInBeforeInsectingPoint(LayoutedWidget interSectWidget, int index)
	{
		LayoutedWidget layoutedWidget = (m_lcOperator as Layouter).MaintainltWidget;
		while (layoutedWidget.ChildWidgets.Count > 0 && !(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget is ParagraphItem) && !(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget is SplitStringWidget) && !(layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1].Widget is WTable))
		{
			layoutedWidget = layoutedWidget.ChildWidgets[layoutedWidget.ChildWidgets.Count - 1];
		}
		float x = interSectWidget.Bounds.X;
		float num = interSectWidget.Bounds.Y;
		float width = m_layoutArea.ClientActiveArea.Width;
		bool flag = interSectWidget.Owner != null && interSectWidget.Owner.Widget is WParagraph;
		ParagraphLayoutInfo paragraphLayoutInfo = null;
		if (interSectWidget.Widget is WParagraph && (interSectWidget.Widget as WParagraph).m_layoutInfo != null)
		{
			paragraphLayoutInfo = interSectWidget.Widget.LayoutInfo as ParagraphLayoutInfo;
		}
		else if (interSectWidget.Widget is SplitWidgetContainer && (interSectWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)
		{
			paragraphLayoutInfo = ((interSectWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).m_layoutInfo as ParagraphLayoutInfo;
		}
		if (interSectWidget.Widget is WTable)
		{
			x = m_layoutArea.ClientActiveArea.X;
		}
		else if (interSectWidget.Widget is SplitWidgetContainer && (interSectWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WTable)
		{
			x = m_layoutArea.ClientActiveArea.X;
		}
		if (paragraphLayoutInfo != null)
		{
			if (flag)
			{
				x = paragraphLayoutInfo.XPosition;
				if (index > 0)
				{
					paragraphLayoutInfo.IsFirstLine = false;
				}
				else if (index == 0)
				{
					interSectWidget.UpdateParaFirstLineHorizontalPositions(paragraphLayoutInfo, interSectWidget.Widget, ref x, ref width);
					width -= ((x - m_layoutArea.ClientActiveArea.X > 0f) ? (x - m_layoutArea.ClientActiveArea.X) : 0f);
				}
			}
			else
			{
				x -= paragraphLayoutInfo.Margins.Left;
			}
			num -= paragraphLayoutInfo.Margins.Top;
		}
		if (flag && index == 0)
		{
			m_layoutArea.UpdateDynamicRelayoutBounds(x, num, isNeedToUpdateWidth: true, width);
		}
		else
		{
			m_layoutArea.UpdateDynamicRelayoutBounds(x, num, isNeedToUpdateWidth: false, 0f);
		}
		for (int i = 0; i < index; i++)
		{
			m_ltWidget.ChildWidgets.Add(interSectWidget.Owner.ChildWidgets[i]);
		}
		if ((!(m_ltWidget.Widget is WSection) && (!(m_ltWidget.Widget is SplitWidgetContainer) || !((m_ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WSection))) || m_ltWidget.ChildWidgets.Count <= 0)
		{
			return;
		}
		UpdateWrappingDifferenceValue(m_ltWidget.ChildWidgets[0]);
		if ((m_lcOperator as Layouter).FootnoteWidgets.Count > 0)
		{
			float num2 = 0f;
			for (int num3 = (m_lcOperator as Layouter).FootnoteWidgets.Count - 1; num3 >= (m_lcOperator as Layouter).FootnoteCount; num3--)
			{
				LayoutedWidget layoutedWidget2 = (m_lcOperator as Layouter).FootnoteWidgets[num3];
				num2 += layoutedWidget2.Bounds.Height;
			}
			RectangleF rectangle = new RectangleF(m_layoutArea.ClientActiveArea.X, m_layoutArea.ClientActiveArea.Y, m_layoutArea.ClientActiveArea.Width, m_layoutArea.ClientActiveArea.Height - num2);
			m_layoutArea.UpdateClientActiveArea(rectangle);
		}
	}

	internal void UpdateWrappingDifferenceValue(LayoutedWidget firstItem)
	{
		if ((m_lcOperator as Layouter).WrappingDifference == float.MinValue && !(m_lcOperator as Layouter).IsLayoutingHeaderFooter && firstItem != null && firstItem.ChildWidgets.Count > 0 && IsBaseFromSection((firstItem.Widget is SplitWidgetContainer) ? ((firstItem.Widget as SplitWidgetContainer).RealWidgetContainer as Entity) : (firstItem.Widget as Entity)))
		{
			float num = 0f;
			if (firstItem.Widget is SplitWidgetContainer && (firstItem.Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)
			{
				num = ((firstItem.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph).ParagraphFormat.BeforeSpacing;
			}
			else if (firstItem.Widget is WParagraph)
			{
				num = (firstItem.Widget as WParagraph).ParagraphFormat.BeforeSpacing;
			}
			if (firstItem.Widget is WTable)
			{
				(m_lcOperator as Layouter).WrappingDifference = firstItem.Bounds.Y - (m_lcOperator as Layouter).ClientLayoutArea.Y;
			}
			else
			{
				(m_lcOperator as Layouter).WrappingDifference = firstItem.ChildWidgets[0].Bounds.Y - num - (m_lcOperator as Layouter).ClientLayoutArea.Y;
			}
		}
	}

	internal void UpdateFootnoteWidgets(LayoutedWidget ltWidget)
	{
		if (ltWidget.Widget is WParagraph)
		{
			UpdateFootnoteWidgets(ltWidget.Widget as WParagraph);
			return;
		}
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			if (!(ltWidget.ChildWidgets[i].Widget is WFootnote))
			{
				continue;
			}
			for (int num = (m_lcOperator as Layouter).FootnoteWidgets.Count - 1; num >= 0; num--)
			{
				if ((((m_lcOperator as Layouter).FootnoteWidgets[num].Widget is WTextBody) ? ((m_lcOperator as Layouter).FootnoteWidgets[num].Widget as WTextBody) : (((m_lcOperator as Layouter).FootnoteWidgets[num].Widget as SplitWidgetContainer).RealWidgetContainer as WTextBody)).Owner as WFootnote == ltWidget.ChildWidgets[i].Widget)
				{
					(m_lcOperator as Layouter).FootnoteWidgets.RemoveAt(num);
					(ltWidget.ChildWidgets[i].Widget as WFootnote).IsLayouted = false;
					ltWidget.ChildWidgets[i].Widget.InitLayoutInfo();
					break;
				}
			}
			for (int num = (m_lcOperator as Layouter).FootnoteSplittedWidgets.Count - 1; num >= 0; num--)
			{
				if (((m_lcOperator as Layouter).FootnoteSplittedWidgets[num].RealWidgetContainer as WTextBody).Owner as WFootnote == ltWidget.ChildWidgets[i].Widget)
				{
					(m_lcOperator as Layouter).FootnoteSplittedWidgets.RemoveAt(num);
					ltWidget.ChildWidgets[i].Widget.InitLayoutInfo();
					break;
				}
			}
		}
		if ((m_lcOperator as Layouter).FootnoteWidgets.Count < 2 && IsNeedToRemoveFootnoteSeparator(ltWidget))
		{
			(m_lcOperator as Layouter).FootnoteWidgets.Clear();
			(m_lcOperator as Layouter).FootNoteSectionIndex.Clear();
		}
	}

	private bool IsNeedToRemoveFootnoteSeparator(LayoutedWidget ltWidget)
	{
		WParagraph wParagraph = ((ltWidget.Widget is WParagraph) ? (ltWidget.Widget as WParagraph) : ((ltWidget.Widget is SplitWidgetContainer) ? ((ltWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null));
		if (wParagraph != null && wParagraph.ParagraphFormat.WidowControl && (m_lcOperator as Layouter).IsTwoLinesLayouted && ltWidget.ChildWidgets.Count > 0)
		{
			while (ltWidget.ChildWidgets.Count > 0 && !(ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1].Widget is ParagraphItem) && !(ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1].Widget is SplitStringWidget))
			{
				ltWidget = ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1];
			}
			return wParagraph.IsLastLine(ltWidget.ChildWidgets[ltWidget.ChildWidgets.Count - 1]);
		}
		return true;
	}

	internal void UpdateFootnoteWidgets(WParagraph paragraph)
	{
		for (int i = 0; i < paragraph.ChildEntities.Count; i++)
		{
			if (!(paragraph.ChildEntities[i] is WFootnote))
			{
				continue;
			}
			for (int num = (m_lcOperator as Layouter).FootnoteWidgets.Count - 1; num >= 0; num--)
			{
				WTextBody wTextBody = (((m_lcOperator as Layouter).FootnoteWidgets[num].Widget is WTextBody) ? ((m_lcOperator as Layouter).FootnoteWidgets[num].Widget as WTextBody) : ((((m_lcOperator as Layouter).FootnoteWidgets[num].Widget as SplitWidgetContainer).RealWidgetContainer is WTextBody) ? (((m_lcOperator as Layouter).FootnoteWidgets[num].Widget as SplitWidgetContainer).RealWidgetContainer as WTextBody) : null));
				if (wTextBody != null && wTextBody.Owner as WFootnote == paragraph.ChildEntities[i])
				{
					(m_lcOperator as Layouter).FootnoteWidgets.RemoveAt(num);
					(paragraph.ChildEntities[i] as WFootnote).IsLayouted = false;
					(m_lcOperator as Layouter).FootNoteSectionIndex.RemoveAt(num);
					(paragraph.ChildEntities[i] as IWidget).InitLayoutInfo();
					break;
				}
			}
			for (int num2 = (m_lcOperator as Layouter).FootnoteSplittedWidgets.Count - 1; num2 >= 0; num2--)
			{
				if (((m_lcOperator as Layouter).FootnoteSplittedWidgets[num2].RealWidgetContainer as WTextBody).Owner as WFootnote == paragraph.ChildEntities[i])
				{
					(m_lcOperator as Layouter).FootnoteSplittedWidgets.RemoveAt(num2);
					(paragraph.ChildEntities[i] as IWidget).InitLayoutInfo();
					break;
				}
			}
		}
		if ((m_lcOperator as Layouter).FootnoteWidgets.Count < 2)
		{
			(m_lcOperator as Layouter).FootnoteWidgets.Clear();
			(m_lcOperator as Layouter).FootNoteSectionIndex.Clear();
		}
	}

	protected void CreateLayoutArea(RectangleF rect)
	{
		WParagraph wParagraph = ((m_widget is WParagraph) ? (m_widget as WParagraph) : ((m_widget is SplitWidgetContainer && (m_widget as SplitWidgetContainer).RealWidgetContainer is WParagraph) ? ((m_widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null));
		if (wParagraph != null && LayoutInfo is ParagraphLayoutInfo && !(LayoutInfo as ParagraphLayoutInfo).IsFirstLine && LayoutInfo is ILayoutSpacingsInfo)
		{
			(LayoutInfo as ParagraphLayoutInfo).Paddings.Top = 0f;
		}
		if (m_widget is WParagraph && (m_lcOperator as Layouter).IsTabWidthUpdatedBasedOnIndent)
		{
			(m_lcOperator as Layouter).IsTabWidthUpdatedBasedOnIndent = false;
		}
		LayoutInfo.IsFirstItemInPage = false;
		if (m_bSkipAreaSpacing)
		{
			m_layoutArea = new LayoutArea(rect);
			UpdateParagraphYPositionBasedonTextWrap();
			return;
		}
		m_layoutArea = new LayoutArea(rect, LayoutInfo as ILayoutSpacingsInfo, m_widget);
		bool flag = ((wParagraph == null || wParagraph.GetOwnerSection() == null || wParagraph.GetOwnerSection().Columns.Count < 2) ? IsForceFitLayout : (LayoutInfo is ParagraphLayoutInfo && Math.Round((LayoutInfo as ParagraphLayoutInfo).YPosition, 2) == Math.Round((m_lcOperator as Layouter).PageTopMargin, 2)));
		if (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter && flag)
		{
			LayoutInfo.IsFirstItemInPage = true;
			float topPad = ((LayoutInfo is ILayoutSpacingsInfo) ? ((LayoutInfo as ILayoutSpacingsInfo).Margins.Top + (LayoutInfo as ILayoutSpacingsInfo).Paddings.Top) : 0f);
			WParagraph wParagraph2 = ((m_widget is WParagraph) ? (m_widget as WParagraph) : ((m_widget is SplitWidgetContainer) ? ((m_widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : null));
			if (wParagraph2 != null && !wParagraph2.IsInCell)
			{
				UpdateParagraphTopMargin(wParagraph2);
				if ((LayoutInfo as ParagraphLayoutInfo).Margins.Top == 0f && ((wParagraph2.PreviousSibling is WParagraph && ((wParagraph2.PreviousSibling as WParagraph).ParagraphFormat.IsInFrame() || (wParagraph2.PreviousSibling as WParagraph).Text != "")) || (wParagraph2.PreviousSibling is WTable && wParagraph2.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)))
				{
					m_layoutArea = new LayoutArea(rect, LayoutInfo as ILayoutSpacingsInfo, m_widget);
				}
				IEntity previousSibling = wParagraph2.PreviousSibling;
				if ((LayoutInfo as ParagraphLayoutInfo).IsFirstItemInPage && wParagraph2.Document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing && previousSibling != null && previousSibling is WParagraph && (previousSibling as WParagraph).m_layoutInfo != null && !(previousSibling as WParagraph).m_layoutInfo.IsSkip && wParagraph2.GetOwnerEntity() != null && wParagraph2.GetOwnerEntity() is WSection && (!IsParagraphFirstItemHasPageOrColumnBreak(wParagraph2) || wParagraph2.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013))
				{
					(LayoutInfo as ParagraphLayoutInfo).Margins.Top = 0f;
					m_layoutArea = new LayoutArea(rect, LayoutInfo as ILayoutSpacingsInfo, m_widget);
				}
				if (((LayoutInfo as ParagraphLayoutInfo).Paddings.Top != 0f || (LayoutInfo as ParagraphLayoutInfo).Margins.Top != 0f) && LayoutInfo is ParagraphLayoutInfo && (LayoutInfo as ParagraphLayoutInfo).IsFirstLine)
				{
					m_layoutArea.UpdateBounds(topPad);
				}
			}
		}
		if (m_widget is WParagraph || (m_widget is SplitWidgetContainer && (m_widget as SplitWidgetContainer).RealWidgetContainer is WParagraph && flag))
		{
			LayoutTextWrapWidgets(m_widget);
		}
	}

	private void LayoutTextWrapWidgets(IWidget widget)
	{
		if ((!(widget is WParagraph) || (widget as WParagraph).IsFloatingItemsLayouted) && !(widget is SplitWidgetContainer))
		{
			return;
		}
		WParagraph wParagraph = null;
		int num = 0;
		if (widget is SplitWidgetContainer)
		{
			Entity owner = (widget as SplitWidgetContainer).WidgetInnerCollection.Owner;
			if (owner is WParagraph)
			{
				wParagraph = owner as WParagraph;
				num = wParagraph.ChildEntities.IndexOf((widget as SplitWidgetContainer).m_currentChild as IEntity);
			}
		}
		else
		{
			wParagraph = widget as WParagraph;
		}
		Layouter layouter = m_lcOperator as Layouter;
		if (num >= 0)
		{
			for (int i = num; i < wParagraph.ChildEntities.Count; i++)
			{
				if (wParagraph.ChildEntities[i] != null && wParagraph.ChildEntities[i].IsFloatingItem(isTextWrapAround: true) && wParagraph.ChildEntities[i] is IWidget && !(wParagraph.ChildEntities[i] as IWidget).LayoutInfo.IsSkip && (wParagraph.ChildEntities[i] as ParagraphItem).GetHorizontalOrigin() != HorizontalOrigin.Character && (wParagraph.ChildEntities[i] as ParagraphItem).GetVerticalOrigin() != VerticalOrigin.Line && (layouter.NotFittedFloatingItems.Count <= 0 || !layouter.NotFittedFloatingItems.Contains(wParagraph.ChildEntities[i])))
				{
					LayoutContext layoutContext = Create(wParagraph.ChildEntities[i] as IWidget, m_lcOperator, IsForceFitLayout);
					RectangleF rect = new RectangleF(m_layoutArea.ClientActiveArea.X, m_layoutArea.ClientActiveArea.Y, m_layoutArea.ClientActiveArea.Width, m_layoutArea.ClientActiveArea.Height);
					LayoutedWidget layoutedWidget = layoutContext.Layout(rect);
					if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
					{
						return;
					}
					if (layoutedWidget != null)
					{
						AddToFloatingItems(layoutedWidget, wParagraph.ChildEntities[i] as ILeafWidget);
						layoutedWidget.InitLayoutInfoForTextWrapElements();
						wParagraph.IsFloatingItemsLayouted = true;
					}
				}
			}
		}
		float xPosition = (wParagraph.m_layoutInfo as ParagraphLayoutInfo).XPosition;
		if (wParagraph.IsFloatingItemsLayouted)
		{
			UpdateParagraphXPositionBasedOnTextWrap(wParagraph, (wParagraph.m_layoutInfo as ParagraphLayoutInfo).XPosition, (wParagraph.m_layoutInfo as ParagraphLayoutInfo).YPosition);
			if (xPosition != (wParagraph.m_layoutInfo as ParagraphLayoutInfo).XPosition)
			{
				wParagraph.IsXpositionUpated = true;
			}
			if (DocumentLayouter.IsUpdatingTOC)
			{
				ResetFloatingEntityProperty(wParagraph);
			}
		}
		if (layouter.NotFittedFloatingItems.Count > 0 && layouter.DynamicParagraph == wParagraph)
		{
			layouter.NotFittedFloatingItems.Clear();
			layouter.DynamicParagraph = null;
		}
	}

	protected void AddToFloatingItems(LayoutedWidget ltWidget, ILeafWidget leafWidget)
	{
		RectangleF rectangleF = ltWidget.Bounds;
		bool IsDoesNotDenotesRectangle = false;
		if (leafWidget is WTextBox && !(leafWidget as WTextBox).TextBoxFormat.IsWrappingBoundsAdded)
		{
			WTextBoxFormat textBoxFormat = (leafWidget as WTextBox).TextBoxFormat;
			FloatingItem floatingItem = new FloatingItem();
			if (textBoxFormat.TextWrappingStyle == TextWrappingStyle.Tight || textBoxFormat.TextWrappingStyle == TextWrappingStyle.Through)
			{
				rectangleF = AdjustboundsBasedOnWrapPolygon(rectangleF, textBoxFormat.WrapPolygon.Vertices, textBoxFormat.Width, textBoxFormat.Height, ref IsDoesNotDenotesRectangle);
			}
			floatingItem.TextWrappingBounds = new RectangleF(rectangleF.X - textBoxFormat.WrapDistanceLeft, rectangleF.Y - textBoxFormat.WrapDistanceTop, rectangleF.Width + textBoxFormat.WrapDistanceRight + textBoxFormat.WrapDistanceLeft, rectangleF.Height + textBoxFormat.WrapDistanceBottom + textBoxFormat.WrapDistanceTop);
			floatingItem.FloatingEntity = leafWidget as Entity;
			floatingItem.IsDoesNotDenotesRectangle = IsDoesNotDenotesRectangle;
			(m_lcOperator as Layouter).FloatingItems.Add(floatingItem);
			textBoxFormat.IsWrappingBoundsAdded = true;
			textBoxFormat.WrapCollectionIndex = (short)((m_lcOperator as Layouter).FloatingItems.Count - 1);
			floatingItem.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
		}
		else if (leafWidget is WPicture && !(leafWidget as WPicture).IsWrappingBoundsAdded)
		{
			WPicture wPicture = leafWidget as WPicture;
			SizeF sizeF = DrawingContext.MeasureImage(wPicture);
			FloatingItem floatingItem2 = new FloatingItem();
			if (wPicture.TextWrappingStyle == TextWrappingStyle.Tight || wPicture.TextWrappingStyle == TextWrappingStyle.Through)
			{
				rectangleF = AdjustboundsBasedOnWrapPolygon(rectangleF, wPicture.WrapPolygon.Vertices, sizeF.Width, sizeF.Height, ref IsDoesNotDenotesRectangle);
			}
			float lineWidth = DrawingContext.GetLineWidth(wPicture);
			if (lineWidth > 0f && (wPicture.TextWrappingStyle == TextWrappingStyle.Square || wPicture.TextWrappingStyle == TextWrappingStyle.TopAndBottom))
			{
				rectangleF.X -= lineWidth;
				rectangleF.Y -= lineWidth;
				rectangleF.Width += 2f * lineWidth;
				rectangleF.Height += 2f * lineWidth;
			}
			floatingItem2.TextWrappingBounds = new RectangleF(rectangleF.X - wPicture.DistanceFromLeft, rectangleF.Y - wPicture.DistanceFromTop, rectangleF.Width + wPicture.DistanceFromRight + wPicture.DistanceFromLeft, rectangleF.Height + wPicture.DistanceFromBottom + wPicture.DistanceFromTop);
			floatingItem2.FloatingEntity = leafWidget as Entity;
			floatingItem2.IsDoesNotDenotesRectangle = IsDoesNotDenotesRectangle;
			(m_lcOperator as Layouter).FloatingItems.Add(floatingItem2);
			(leafWidget as WPicture).IsWrappingBoundsAdded = true;
			(leafWidget as WPicture).WrapCollectionIndex = (short)((m_lcOperator as Layouter).FloatingItems.Count - 1);
			floatingItem2.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
		}
		else if (leafWidget is Shape && !(leafWidget as Shape).WrapFormat.IsWrappingBoundsAdded)
		{
			Shape shape = leafWidget as Shape;
			FloatingItem floatingItem3 = new FloatingItem();
			if (shape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Tight || shape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Through)
			{
				rectangleF = AdjustboundsBasedOnWrapPolygon(rectangleF, shape.WrapFormat.WrapPolygon.Vertices, shape.Width, shape.Height, ref IsDoesNotDenotesRectangle);
			}
			floatingItem3.TextWrappingBounds = new RectangleF(rectangleF.X - shape.WrapFormat.DistanceLeft, rectangleF.Y - shape.WrapFormat.DistanceTop, rectangleF.Width + shape.WrapFormat.DistanceRight + shape.WrapFormat.DistanceLeft, rectangleF.Height + shape.WrapFormat.DistanceBottom + shape.WrapFormat.DistanceTop);
			floatingItem3.FloatingEntity = leafWidget as Entity;
			floatingItem3.IsDoesNotDenotesRectangle = IsDoesNotDenotesRectangle;
			(m_lcOperator as Layouter).FloatingItems.Add(floatingItem3);
			(leafWidget as Shape).WrapFormat.IsWrappingBoundsAdded = true;
			(leafWidget as Shape).WrapFormat.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
			floatingItem3.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
		}
		else if (leafWidget is GroupShape && !(leafWidget as GroupShape).WrapFormat.IsWrappingBoundsAdded)
		{
			GroupShape groupShape = leafWidget as GroupShape;
			FloatingItem floatingItem4 = new FloatingItem();
			if (groupShape.Rotation != 0f)
			{
				rectangleF = DrawingContext.GetBoundingBoxCoordinates(rectangleF, groupShape.Rotation);
			}
			if (groupShape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Tight || groupShape.WrapFormat.TextWrappingStyle == TextWrappingStyle.Through)
			{
				rectangleF = AdjustboundsBasedOnWrapPolygon(rectangleF, groupShape.WrapFormat.WrapPolygon.Vertices, groupShape.Width, groupShape.Height, ref IsDoesNotDenotesRectangle);
			}
			floatingItem4.TextWrappingBounds = new RectangleF(rectangleF.X - groupShape.WrapFormat.DistanceLeft, rectangleF.Y - groupShape.WrapFormat.DistanceTop, rectangleF.Width + groupShape.WrapFormat.DistanceRight + groupShape.WrapFormat.DistanceLeft, rectangleF.Height + groupShape.WrapFormat.DistanceBottom + groupShape.WrapFormat.DistanceTop);
			floatingItem4.FloatingEntity = leafWidget as Entity;
			floatingItem4.IsDoesNotDenotesRectangle = IsDoesNotDenotesRectangle;
			(m_lcOperator as Layouter).FloatingItems.Add(floatingItem4);
			(leafWidget as GroupShape).WrapFormat.IsWrappingBoundsAdded = true;
			(leafWidget as GroupShape).WrapFormat.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
			floatingItem4.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
		}
		else if (leafWidget is WChart && !(leafWidget as WChart).WrapFormat.IsWrappingBoundsAdded)
		{
			WChart wChart = leafWidget as WChart;
			FloatingItem floatingItem5 = new FloatingItem();
			if (wChart.WrapFormat.TextWrappingStyle == TextWrappingStyle.Tight || wChart.WrapFormat.TextWrappingStyle == TextWrappingStyle.Through)
			{
				rectangleF = AdjustboundsBasedOnWrapPolygon(rectangleF, wChart.WrapFormat.WrapPolygon.Vertices, wChart.Width, wChart.Height, ref IsDoesNotDenotesRectangle);
			}
			floatingItem5.TextWrappingBounds = new RectangleF(rectangleF.X - wChart.WrapFormat.DistanceLeft, rectangleF.Y - wChart.WrapFormat.DistanceTop, rectangleF.Width + wChart.WrapFormat.DistanceRight + wChart.WrapFormat.DistanceLeft, rectangleF.Height + wChart.WrapFormat.DistanceBottom + wChart.WrapFormat.DistanceTop);
			floatingItem5.FloatingEntity = leafWidget as Entity;
			floatingItem5.IsDoesNotDenotesRectangle = IsDoesNotDenotesRectangle;
			(m_lcOperator as Layouter).FloatingItems.Add(floatingItem5);
			(leafWidget as WChart).WrapFormat.IsWrappingBoundsAdded = true;
			(leafWidget as WChart).WrapFormat.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
			floatingItem5.WrapCollectionIndex = (m_lcOperator as Layouter).FloatingItems.Count - 1;
		}
	}

	protected bool IsDrawingElement(ILeafWidget leafWidget)
	{
		if (!(leafWidget is WPicture) && !(leafWidget is Shape) && !(leafWidget is WChart) && !(leafWidget is GroupShape))
		{
			return leafWidget is WTextBox;
		}
		return true;
	}

	internal bool IsDoNotSuppressIndent(WParagraph paragraph, float yPosition, float wrappingBoundsBottom, int floatingItemIndex)
	{
		bool result = false;
		if (paragraph.Document.Settings.CompatibilityOptions[CompatibilityOption.WW11IndentRules] && paragraph.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && wrappingBoundsBottom > yPosition)
		{
			WTextBody wTextBody = null;
			bool flag = true;
			if (!(paragraph.Owner is WTextBody) || !((paragraph.Owner as WTextBody).Owner is WSection))
			{
				Entity floatingEntity = (m_lcOperator as Layouter).FloatingItems[floatingItemIndex].FloatingEntity;
				if (floatingEntity is WTable)
				{
					wTextBody = (floatingEntity as WTable).OwnerTextBody;
				}
				else if (floatingEntity is WParagraph)
				{
					wTextBody = (floatingEntity as WParagraph).OwnerTextBody;
				}
				else if (floatingEntity is ParagraphItem)
				{
					wTextBody = (floatingEntity as ParagraphItem).OwnerParagraph.OwnerTextBody;
				}
				WTextBody wTextBody2 = paragraph.OwnerTextBody;
				if (wTextBody2 != null && wTextBody2.Owner is BlockContentControl)
				{
					wTextBody2 = GetSDTOwnerTextBody(wTextBody2.Owner as BlockContentControl);
				}
				if (wTextBody != null && wTextBody.Owner is BlockContentControl)
				{
					wTextBody = GetSDTOwnerTextBody(wTextBody.Owner as BlockContentControl);
				}
				flag = wTextBody == wTextBody2;
			}
			if (flag)
			{
				result = true;
			}
		}
		return result;
	}

	private WTextBody GetSDTOwnerTextBody(BlockContentControl sdtBlockContent)
	{
		if (sdtBlockContent.Owner is BlockContentControl)
		{
			return (sdtBlockContent.Owner as BlockContentControl).OwnerTextBody;
		}
		return null;
	}

	internal void ResetFloatingEntityProperty(WParagraph paragraph)
	{
		if (paragraph.IsFloatingItemsLayouted)
		{
			for (int i = 0; i < paragraph.ChildEntities.Count; i++)
			{
				if (paragraph.ChildEntities[i] is ParagraphItem && paragraph.ChildEntities[i].IsFloatingItem(isTextWrapAround: true))
				{
					if (paragraph.ChildEntities[i] is WTextBox)
					{
						(paragraph.ChildEntities[i] as WTextBox).TextBoxFormat.IsWrappingBoundsAdded = false;
					}
					else if (paragraph.ChildEntities[i] is WPicture)
					{
						(paragraph.ChildEntities[i] as WPicture).IsWrappingBoundsAdded = false;
					}
					else if (paragraph.ChildEntities[i] is Shape)
					{
						(paragraph.ChildEntities[i] as Shape).WrapFormat.IsWrappingBoundsAdded = false;
					}
					else if (paragraph.ChildEntities[i] is WChart)
					{
						(paragraph.ChildEntities[i] as WChart).WrapFormat.IsWrappingBoundsAdded = false;
					}
					else if (paragraph.ChildEntities[i] is GroupShape)
					{
						(paragraph.ChildEntities[i] as GroupShape).WrapFormat.IsWrappingBoundsAdded = false;
					}
				}
			}
		}
		paragraph.IsFloatingItemsLayouted = false;
	}

	internal float GetParagraphTopMargin(WParagraph paragraph)
	{
		IEntity entity = null;
		if (!paragraph.Document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing && Math.Round((paragraph.m_layoutInfo as ParagraphLayoutInfo).YPosition, 2) != Math.Round((m_lcOperator as Layouter).PageTopMargin, 2) && (entity = paragraph.PreviousSibling) != null && entity is WParagraph && ((paragraph.ParagraphFormat.BeforeSpacing > (entity as WParagraph).ParagraphFormat.AfterSpacing && !paragraph.ParagraphFormat.SpaceBeforeAuto) || (paragraph.ParagraphFormat.SpaceBeforeAuto && (entity as WParagraph).ParagraphFormat.AfterSpacing < 14f)) && !paragraph.ParagraphFormat.ContextualSpacing && (!(entity as WParagraph).ParagraphFormat.ContextualSpacing || !(paragraph.StyleName == (entity as WParagraph).StyleName)))
		{
			if (paragraph.ParagraphFormat.SpaceBeforeAuto)
			{
				return 14f;
			}
			return paragraph.ParagraphFormat.BeforeSpacing;
		}
		return 0f;
	}

	internal WTextRange GetTextRange(IWidget widget)
	{
		if (!(widget is WTextRange))
		{
			if (!(widget is SplitStringWidget))
			{
				return null;
			}
			return (widget as SplitStringWidget).RealStringWidget as WTextRange;
		}
		return widget as WTextRange;
	}

	private RectangleF AdjustboundsBasedOnWrapPolygon(RectangleF rect, List<PointF> vertices, float imageWidth, float imageHeight, ref bool IsDoesNotDenotesRectangle)
	{
		if (vertices == null || vertices.Count < 2)
		{
			return rect;
		}
		float minX = 0f;
		float maxX = 0f;
		float minY = 0f;
		float maxY = 0f;
		RectangleF rectangleF = default(RectangleF);
		rectangleF = rect;
		if (IsWrapPolygonDenotesRectangle(vertices, ref minX, ref maxX, ref minY, ref maxY))
		{
			float num = 21600f / imageWidth;
			float num2 = vertices[0].X / num;
			float num3 = vertices[0].Y / (21600f / imageHeight);
			rectangleF.Width = (maxX - minX) / num;
			rectangleF.Height = (maxY - minY) / (21600f / imageHeight);
			rectangleF.X = rect.X + num2;
			rectangleF.Y = rect.Y + num3;
		}
		else
		{
			IsDoesNotDenotesRectangle = true;
		}
		return rectangleF;
	}

	internal PointF LineIntersectionPoint(PointF ps1, PointF pe1, PointF ps2, PointF pe2)
	{
		float x = ps1.X;
		float y = ps1.Y;
		float x2 = pe1.X;
		float y2 = pe1.Y;
		float x3 = ps2.X;
		float y3 = ps2.Y;
		float x4 = pe2.X;
		float y4 = pe2.Y;
		float num = y2 - y;
		float num2 = x - x2;
		float num3 = x2 * y - x * y2;
		float num4 = num * x3 + num2 * y3 + num3;
		float num5 = num * x4 + num2 * y4 + num3;
		if (num4 != 0f && num5 != 0f && sameSign(num4, num5))
		{
			return new PointF(0f, 0f);
		}
		float num6 = y4 - y3;
		float num7 = x3 - x4;
		float num8 = x4 * y3 - x3 * y4;
		float num9 = num6 * x + num7 * y + num8;
		float num10 = num6 * x2 + num7 * y2 + num8;
		if (num9 != 0f && num10 != 0f && sameSign(num9, num10))
		{
			return new PointF(0f, 0f);
		}
		float num11 = num * num7 - num6 * num2;
		if (num11 == 0f)
		{
			return new PointF(0f, 0f);
		}
		float num12 = ((!(num11 < 0f)) ? (num11 / 2f) : ((0f - num11) / 2f));
		float num13 = num2 * num8 - num7 * num3;
		float x5 = ((!(num13 < 0f)) ? ((num13 + num12) / num11) : ((num13 - num12) / num11));
		num13 = num6 * num3 - num * num8;
		float y5 = ((!(num13 < 0f)) ? ((num13 + num12) / num11) : ((num13 - num12) / num11));
		return new PointF(x5, y5);
	}

	internal bool sameSign(float a, float b)
	{
		return a * b >= 0f;
	}

	private bool IsWrapPolygonDenotesRectangle(List<PointF> vertices, ref float minX, ref float maxX, ref float minY, ref float maxY)
	{
		if (vertices.Count == 0)
		{
			return false;
		}
		minX = vertices[0].X;
		maxX = vertices[0].X;
		minY = vertices[0].Y;
		maxY = vertices[0].Y;
		for (int i = 0; i < vertices.Count - 1; i++)
		{
			if (i % 2 == 0)
			{
				if (vertices[i].X != vertices[i + 1].X && vertices[i].Y != vertices[i + 1].Y)
				{
					return false;
				}
				if (minX > vertices[i].X)
				{
					minX = vertices[i].X;
				}
				if (maxX < vertices[i].X)
				{
					maxX = vertices[i].X;
				}
			}
			else
			{
				if (vertices[i].Y != vertices[i + 1].Y && vertices[i].X != vertices[i + 1].X)
				{
					return false;
				}
				if (minY > vertices[i].Y)
				{
					minY = vertices[i].Y;
				}
				if (maxY < vertices[i].Y)
				{
					maxY = vertices[i].Y;
				}
			}
		}
		if (vertices[0].X != vertices[vertices.Count - 1].X || minX == maxX || minY == maxY)
		{
			return false;
		}
		return true;
	}

	internal void UpdateParagraphXPositionBasedOnTextWrap(WParagraph paragraph, float xPosition, float yPosition)
	{
		float num = 0f;
		bool flag = false;
		if (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter && (m_lcOperator as Layouter).WrappingDifference == float.MinValue && Math.Round(yPosition, 2) == Math.Round((m_lcOperator as Layouter).PageTopMargin, 2))
		{
			num = yPosition;
			flag = true;
		}
		if ((m_lcOperator as Layouter).FloatingItems.Count <= 0 || ((m_lcOperator as Layouter).IsLayoutingHeaderFooter && !paragraph.IsInCell && paragraph.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013) || IsInFootnote(paragraph) || (m_lcOperator as Layouter).IsLayoutingFootnote)
		{
			return;
		}
		RectangleF clientLayoutArea = (m_lcOperator as Layouter).ClientLayoutArea;
		clientLayoutArea.X = xPosition;
		clientLayoutArea.Y = yPosition;
		float num2 = 0f;
		if (paragraph.IsInCell)
		{
			CellLayoutInfo cellLayoutInfo = ((IWidget)(paragraph.GetOwnerEntity() as WTableCell)).LayoutInfo as CellLayoutInfo;
			num2 = cellLayoutInfo.Paddings.Left + cellLayoutInfo.Paddings.Right;
		}
		float num3 = 18f - num2;
		SizeF size = ((IWidget)paragraph).LayoutInfo.Size;
		for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
		{
			if (paragraph.IsInCell && (m_lcOperator as Layouter).FloatingItems[i].AllowOverlap && (paragraph.GetOwnerEntity() as WTableCell).OwnerRow.OwnerTable.TableFormat.Positioning.AllowOverlap && (!((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity is WTable) || !((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity as WTable).IsInCell))
			{
				WParagraph ownerParagraph = (m_lcOperator as Layouter).FloatingItems[i].OwnerParagraph;
				if (ownerParagraph == null || !ownerParagraph.IsInCell || paragraph.GetOwnerEntity() != ownerParagraph.GetOwnerEntity())
				{
					continue;
				}
			}
			float x = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds.X;
			RectangleF textWrappingBounds = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds;
			TextWrappingStyle textWrappingStyle = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingStyle;
			WTextBody ownerBody = null;
			if ((!IsInSameTextBody(paragraph, (m_lcOperator as Layouter).FloatingItems[i], ref ownerBody) && paragraph.IsInCell && ownerBody is WTableCell) || (IsInFrame((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity as WParagraph) && IsOwnerCellInFrame(paragraph)))
			{
				continue;
			}
			if (paragraph.ParagraphFormat.Bidi && IsInSameTextBody(paragraph, (m_lcOperator as Layouter).FloatingItems[i], ref ownerBody) && paragraph.IsInCell && ownerBody is WTableCell)
			{
				ModifyXPositionForRTLLayouting(i, ref textWrappingBounds, m_layoutArea.ClientArea);
			}
			else if (paragraph.ParagraphFormat.Bidi)
			{
				ModifyXPositionForRTLLayouting(i, ref textWrappingBounds, (m_lcOperator as Layouter).ClientLayoutArea);
			}
			float num4 = 18f;
			if (textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through)
			{
				num4 = ((paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? 17.6f : 8f);
			}
			num4 -= num2;
			num3 = num4;
			if ((textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && (m_lcOperator as Layouter).FloatingItems[i].IsDoesNotDenotesRectangle)
			{
				RectangleF rectangleF = AdjustTightAndThroughBounds((m_lcOperator as Layouter).FloatingItems[i], clientLayoutArea, size.Height);
				if (rectangleF.X != 0f)
				{
					textWrappingBounds = rectangleF;
					num3 = size.Width;
				}
			}
			if (!paragraph.IsInCell && !paragraph.ParagraphFormat.IsFrame && !IsInTable((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity) && !(clientLayoutArea.X > textWrappingBounds.Right + num4) && !(clientLayoutArea.Right < textWrappingBounds.X - num4) && (m_lcOperator as Layouter).FloatingItems.Count > 0 && clientLayoutArea.Y + size.Height > textWrappingBounds.Y && clientLayoutArea.Y < textWrappingBounds.Bottom && textWrappingStyle != 0 && textWrappingStyle != TextWrappingStyle.TopAndBottom && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind)
			{
				float num5 = (((IWidget)paragraph).LayoutInfo as ParagraphLayoutInfo).Margins.Right;
				num5 = ((num5 < 0f) ? Math.Abs(num5) : 0f);
				if (paragraph.ParagraphFormat.GetAlignmentToRender() != 0 && clientLayoutArea.X < textWrappingBounds.X && clientLayoutArea.X + size.Width > textWrappingBounds.X)
				{
					(paragraph.m_layoutInfo as ParagraphLayoutInfo).XPosition = clientLayoutArea.X;
				}
				else if (clientLayoutArea.X >= textWrappingBounds.X && clientLayoutArea.X < textWrappingBounds.Right)
				{
					clientLayoutArea.Width = clientLayoutArea.Width - (textWrappingBounds.Right - clientLayoutArea.X) - num5;
					if (clientLayoutArea.Width < num3)
					{
						clientLayoutArea.Width = m_layoutArea.ClientActiveArea.Right - textWrappingBounds.Right - num5;
						if (clientLayoutArea.Width < num3)
						{
							(paragraph.m_layoutInfo as ParagraphLayoutInfo).XPosition = clientLayoutArea.X;
							clientLayoutArea.Width = (m_lcOperator as Layouter).ClientLayoutArea.Width;
							clientLayoutArea.Height = textWrappingBounds.Bottom - clientLayoutArea.X;
							clientLayoutArea.Y = textWrappingBounds.Bottom;
						}
						else
						{
							(paragraph.m_layoutInfo as ParagraphLayoutInfo).XPosition = textWrappingBounds.Right;
						}
					}
					else if (paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2007 || clientLayoutArea.Y <= textWrappingBounds.Bottom)
					{
						if (IsNeedToUpdateParagraphYPosition(clientLayoutArea.Y, textWrappingStyle, paragraph, clientLayoutArea.Y + size.Height + paragraph.ParagraphFormat.AfterSpacing, textWrappingBounds.Bottom))
						{
							(paragraph.m_layoutInfo as ParagraphLayoutInfo).XPosition = clientLayoutArea.X;
							clientLayoutArea.Width = (m_lcOperator as Layouter).ClientLayoutArea.Width;
							clientLayoutArea.Y = textWrappingBounds.Bottom;
							clientLayoutArea.Height = (m_lcOperator as Layouter).ClientLayoutArea.Height - (textWrappingBounds.Bottom - (m_lcOperator as Layouter).ClientLayoutArea.Y);
							m_layoutArea.UpdateBoundsBasedOnTextWrap(textWrappingBounds.Bottom);
						}
						else
						{
							ParagraphLayoutInfo paragraphLayoutInfo = paragraph.m_layoutInfo as ParagraphLayoutInfo;
							if ((IsDoNotSuppressIndent(paragraph, clientLayoutArea.Y, textWrappingBounds.Bottom, i) ? 0f : paragraphLayoutInfo.Margins.Left) + paragraphLayoutInfo.FirstLineIndent + clientLayoutArea.X < textWrappingBounds.Right)
							{
								(paragraph.m_layoutInfo as ParagraphLayoutInfo).XPosition = textWrappingBounds.Right;
								clientLayoutArea.X = textWrappingBounds.Right;
								if (paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
								{
									paragraph.IsXpositionUpated = true;
								}
							}
						}
					}
				}
				else if ((textWrappingBounds.X - num3 > clientLayoutArea.X && clientLayoutArea.Right > textWrappingBounds.X) || (clientLayoutArea.X > textWrappingBounds.X && clientLayoutArea.X > textWrappingBounds.Right))
				{
					(paragraph.m_layoutInfo as ParagraphLayoutInfo).XPosition = clientLayoutArea.X;
				}
				else if (clientLayoutArea.X > textWrappingBounds.X - num3 && clientLayoutArea.X < textWrappingBounds.Right)
				{
					if (clientLayoutArea.Width + (clientLayoutArea.X - textWrappingBounds.Right) < num3)
					{
						clientLayoutArea.Y = textWrappingBounds.Bottom;
					}
					else
					{
						(paragraph.m_layoutInfo as ParagraphLayoutInfo).XPosition = textWrappingBounds.Right;
					}
				}
				else if (IsNeedToUpdateParagraphYPosition(clientLayoutArea.Y, textWrappingStyle, paragraph, clientLayoutArea.Y + size.Height + paragraph.ParagraphFormat.AfterSpacing, textWrappingBounds.Bottom))
				{
					(paragraph.m_layoutInfo as ParagraphLayoutInfo).XPosition = clientLayoutArea.X;
					clientLayoutArea.Width = (m_lcOperator as Layouter).ClientLayoutArea.Width;
					clientLayoutArea.Y = textWrappingBounds.Bottom;
					clientLayoutArea.Height = (m_lcOperator as Layouter).ClientLayoutArea.Height - (textWrappingBounds.Bottom - (m_lcOperator as Layouter).ClientLayoutArea.Y);
					m_layoutArea.UpdateBoundsBasedOnTextWrap(textWrappingBounds.Bottom);
				}
			}
			ResetXPositionForRTLLayouting(i, ref textWrappingBounds, x);
		}
		if (m_widget is WParagraph)
		{
			List<FloatingItem> floatingItems = new List<FloatingItem>((m_lcOperator as Layouter).FloatingItems);
			FloatingItem.SortFloatingItems(floatingItems, SortPosition.Y, isNeedToUpdateWrapCollectionIndex: false);
			UpdateXYPositionBasedOnAdjacentFloatingItems(floatingItems, clientLayoutArea, size, m_widget as WParagraph, isFromLeafLayoutContext: false);
		}
		if (flag && num < yPosition)
		{
			(m_lcOperator as Layouter).WrappingDifference = yPosition - (m_lcOperator as Layouter).PageTopMargin;
		}
	}

	internal bool IsOwnerCellInFrame(WParagraph paragraph)
	{
		if (paragraph.GetOwnerTableCell(paragraph.OwnerTextBody) == null)
		{
			return false;
		}
		return paragraph.GetOwnerTableCell(paragraph.OwnerTextBody).OwnerRow.OwnerTable.IsFrame;
	}

	internal void ModifyXPositionForRTLLayouting(int floatingItemIndex, ref RectangleF textWrappingBounds, RectangleF clientLayoutArea)
	{
		float num = clientLayoutArea.Right - textWrappingBounds.Right;
		textWrappingBounds.X = clientLayoutArea.X + num;
		(m_lcOperator as Layouter).FloatingItems[floatingItemIndex].TextWrappingBounds = textWrappingBounds;
	}

	internal void ResetXPositionForRTLLayouting(int floatingItemIndex, ref RectangleF textWrappingBounds, float floatingItemXPosition)
	{
		textWrappingBounds.X = floatingItemXPosition;
		textWrappingBounds.Y = (m_lcOperator as Layouter).FloatingItems[floatingItemIndex].TextWrappingBounds.Y;
		textWrappingBounds.Size = (m_lcOperator as Layouter).FloatingItems[floatingItemIndex].TextWrappingBounds.Size;
		(m_lcOperator as Layouter).FloatingItems[floatingItemIndex].TextWrappingBounds = textWrappingBounds;
	}

	private bool IsNeedToUpdateParagraphYPosition(float yPosition, TextWrappingStyle textWrappingStyle, WParagraph paragraph, float paraMarkEndPosition, float bottomPosition)
	{
		ILayoutSpacingsInfo layoutSpacingsInfo = LayoutInfo as ILayoutSpacingsInfo;
		if ((textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && paragraph.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && Math.Round(paraMarkEndPosition) > Math.Round(bottomPosition) && IsFloatingItemIntersectParaMark(yPosition - ((layoutSpacingsInfo != null) ? (layoutSpacingsInfo.Margins.Top + layoutSpacingsInfo.Paddings.Top) : 0f), paraMarkEndPosition))
		{
			return true;
		}
		return false;
	}

	private bool IsFloatingItemIntersectParaMark(float startValue, float endValue)
	{
		for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
		{
			if (startValue <= (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds.Y && endValue >= (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds.Y)
			{
				return true;
			}
		}
		return false;
	}

	internal RectangleF AdjustTightAndThroughBounds(FloatingItem floatingItem, RectangleF clientArea, float size1)
	{
		if (floatingItem.TextWrappingStyle == TextWrappingStyle.Tight && floatingItem.TextWrappingBounds.X < clientArea.X)
		{
			clientArea.X = floatingItem.TextWrappingBounds.X;
		}
		RectangleF textWrappingBounds = floatingItem.TextWrappingBounds;
		PointF minimumInterSectPoint = new PointF(0f, 0f);
		PointF maximumIntersectPoint = new PointF(0f, 0f);
		float num = textWrappingBounds.X;
		if (floatingItem.FloatingEntity is WPicture)
		{
			WPicture wPicture = floatingItem.FloatingEntity as WPicture;
			textWrappingBounds.X += wPicture.DistanceFromLeft;
			textWrappingBounds.Y += wPicture.DistanceFromTop;
			textWrappingBounds.Width -= wPicture.DistanceFromRight + wPicture.DistanceFromLeft;
			textWrappingBounds.Height -= wPicture.DistanceFromBottom + wPicture.DistanceFromTop;
			SizeF sizeF = DrawingContext.MeasureImage(wPicture);
			List<PointF> vertices = wPicture.WrapPolygon.Vertices;
			FindMaxMinIntersectPoint(vertices, sizeF.Width, sizeF.Height, textWrappingBounds, size1, floatingItem, clientArea, ref minimumInterSectPoint, ref maximumIntersectPoint);
			if (minimumInterSectPoint.X == 0f && maximumIntersectPoint.X == 0f)
			{
				return floatingItem.TextWrappingBounds;
			}
			if (minimumInterSectPoint.X == 0f)
			{
				textWrappingBounds.X -= wPicture.DistanceFromLeft;
				textWrappingBounds.Y -= wPicture.DistanceFromTop;
				textWrappingBounds.Width = maximumIntersectPoint.X - textWrappingBounds.X + wPicture.DistanceFromRight;
				textWrappingBounds.Height += wPicture.DistanceFromBottom + wPicture.DistanceFromTop;
			}
			else
			{
				num = minimumInterSectPoint.X - wPicture.DistanceFromLeft;
				textWrappingBounds.Y -= wPicture.DistanceFromTop;
				textWrappingBounds.Width = maximumIntersectPoint.X - num + wPicture.DistanceFromRight;
				textWrappingBounds.Height += wPicture.DistanceFromBottom + wPicture.DistanceFromTop;
			}
		}
		else if (floatingItem.FloatingEntity is Shape || floatingItem.FloatingEntity is WChart || floatingItem.FloatingEntity is GroupShape)
		{
			ShapeBase shapeBase = ((floatingItem.FloatingEntity is Shape) ? (floatingItem.FloatingEntity as Shape) : ((!(floatingItem.FloatingEntity is GroupShape)) ? ((ShapeBase)(floatingItem.FloatingEntity as WChart)) : ((ShapeBase)(floatingItem.FloatingEntity as GroupShape))));
			textWrappingBounds.X += shapeBase.WrapFormat.DistanceLeft;
			textWrappingBounds.Y += shapeBase.WrapFormat.DistanceTop;
			textWrappingBounds.Width -= shapeBase.WrapFormat.DistanceRight + shapeBase.WrapFormat.DistanceLeft;
			textWrappingBounds.Height -= shapeBase.WrapFormat.DistanceBottom + shapeBase.WrapFormat.DistanceTop;
			List<PointF> vertices2 = shapeBase.WrapFormat.WrapPolygon.Vertices;
			FindMaxMinIntersectPoint(vertices2, shapeBase.Width, shapeBase.Height, textWrappingBounds, size1, floatingItem, clientArea, ref minimumInterSectPoint, ref maximumIntersectPoint);
			if (minimumInterSectPoint.X == 0f && maximumIntersectPoint.X == 0f)
			{
				return floatingItem.TextWrappingBounds;
			}
			if (minimumInterSectPoint.X == 0f)
			{
				textWrappingBounds.X -= shapeBase.WrapFormat.DistanceLeft;
				textWrappingBounds.Y -= shapeBase.WrapFormat.DistanceTop;
				textWrappingBounds.Width = maximumIntersectPoint.X - textWrappingBounds.X + shapeBase.WrapFormat.DistanceRight;
				textWrappingBounds.Height += shapeBase.WrapFormat.DistanceBottom + shapeBase.WrapFormat.DistanceTop;
			}
			else
			{
				num = minimumInterSectPoint.X - shapeBase.WrapFormat.DistanceLeft;
				textWrappingBounds.Y -= shapeBase.WrapFormat.DistanceTop;
				textWrappingBounds.Width = maximumIntersectPoint.X - num + shapeBase.WrapFormat.DistanceRight;
				textWrappingBounds.Height += shapeBase.WrapFormat.DistanceBottom + shapeBase.WrapFormat.DistanceTop;
			}
		}
		else if (floatingItem.FloatingEntity is WTextBox)
		{
			WTextBox wTextBox = floatingItem.FloatingEntity as WTextBox;
			textWrappingBounds.X += wTextBox.TextBoxFormat.WrapDistanceLeft;
			textWrappingBounds.Y += wTextBox.TextBoxFormat.WrapDistanceTop;
			textWrappingBounds.Width -= wTextBox.TextBoxFormat.WrapDistanceRight + wTextBox.TextBoxFormat.WrapDistanceLeft;
			textWrappingBounds.Height -= wTextBox.TextBoxFormat.WrapDistanceBottom + wTextBox.TextBoxFormat.WrapDistanceTop;
			List<PointF> vertices3 = wTextBox.TextBoxFormat.WrapPolygon.Vertices;
			FindMaxMinIntersectPoint(vertices3, wTextBox.TextBoxFormat.Width, wTextBox.TextBoxFormat.Height, textWrappingBounds, size1, floatingItem, clientArea, ref minimumInterSectPoint, ref maximumIntersectPoint);
			if (minimumInterSectPoint.X == 0f && maximumIntersectPoint.X == 0f)
			{
				return floatingItem.TextWrappingBounds;
			}
			if (minimumInterSectPoint.X == 0f)
			{
				textWrappingBounds.X -= wTextBox.TextBoxFormat.WrapDistanceLeft;
				textWrappingBounds.Y -= wTextBox.TextBoxFormat.WrapDistanceTop;
				textWrappingBounds.Width = maximumIntersectPoint.X - textWrappingBounds.X + wTextBox.TextBoxFormat.WrapDistanceRight;
				textWrappingBounds.Height += wTextBox.TextBoxFormat.WrapDistanceBottom + wTextBox.TextBoxFormat.WrapDistanceTop;
			}
			else
			{
				num = minimumInterSectPoint.X - wTextBox.TextBoxFormat.WrapDistanceLeft;
				textWrappingBounds.Y -= wTextBox.TextBoxFormat.WrapDistanceTop;
				textWrappingBounds.Width = maximumIntersectPoint.X - num + wTextBox.TextBoxFormat.WrapDistanceRight;
				textWrappingBounds.Height += wTextBox.TextBoxFormat.WrapDistanceBottom + wTextBox.TextBoxFormat.WrapDistanceTop;
			}
		}
		return new RectangleF((float)Math.Round(num, 1), (float)Math.Round(textWrappingBounds.Y, 1), (float)Math.Round(textWrappingBounds.Width, 1), (float)Math.Round(textWrappingBounds.Height, 1));
	}

	private void FindMaxMinIntersectPoint(List<PointF> vertices, float width, float height, RectangleF rect, float size1, FloatingItem floatingItem, RectangleF clientArea, ref PointF minimumInterSectPoint, ref PointF maximumIntersectPoint)
	{
		List<PointF> list = new List<PointF>();
		for (int i = 0; i < vertices.Count; i++)
		{
			float num = vertices[i].X / (21600f / width);
			float num2 = vertices[i].Y / (21600f / height);
			list.Add(new PointF(rect.X + num, rect.Y + num2));
		}
		for (int j = 0; j < (int)Math.Ceiling(size1); j++)
		{
			List<PointF> list2 = new List<PointF>();
			for (int k = 0; k < list.Count - 1; k++)
			{
				if (floatingItem.TextWrappingStyle == TextWrappingStyle.Tight && clientArea.X < rect.Right)
				{
					if ((!(clientArea.Y + (float)j > list[k].Y) || !(clientArea.Y + (float)j < list[k + 1].Y)) && (!(clientArea.Y + (float)j < list[k].Y) || !(clientArea.Y + (float)j > list[k + 1].Y)))
					{
						continue;
					}
					PointF pointF = default(PointF);
					pointF = ((!(floatingItem.TextWrappingBounds.Right > clientArea.Right)) ? LineIntersectionPoint(new PointF(clientArea.X, clientArea.Y + (float)j), new PointF(clientArea.Right, clientArea.Y + (float)j), list[k], list[k + 1]) : LineIntersectionPoint(new PointF(clientArea.X, clientArea.Y + (float)j), new PointF(floatingItem.TextWrappingBounds.Right, clientArea.Y + (float)j), list[k], list[k + 1]));
					pointF.X = (float)Math.Round(pointF.X, 2);
					pointF.Y = (float)Math.Round(pointF.Y, 2);
					if ((pointF.X != 0f && minimumInterSectPoint.X > pointF.X) || minimumInterSectPoint.X == 0f)
					{
						if (IsLineSlopeIsLeftToRight(list[k], list[k + 1]))
						{
							minimumInterSectPoint = pointF;
							minimumInterSectPoint.X -= (float)Math.Round(1.0, 2);
						}
						else
						{
							minimumInterSectPoint = pointF;
						}
					}
					if (maximumIntersectPoint.X < pointF.X || maximumIntersectPoint.X == 0f)
					{
						if (IsLineSlopeIsLeftToRight(list[k], list[k + 1]))
						{
							maximumIntersectPoint = pointF;
							maximumIntersectPoint.X += 1f;
						}
						else
						{
							maximumIntersectPoint = pointF;
						}
					}
				}
				else
				{
					if ((!(clientArea.Y + (float)j > list[k].Y) || !(clientArea.Y + (float)j < list[k + 1].Y)) && (!(clientArea.Y + (float)j < list[k].Y) || !(clientArea.Y + (float)j > list[k + 1].Y)))
					{
						continue;
					}
					PointF pointF2 = default(PointF);
					pointF2 = ((!(floatingItem.TextWrappingBounds.Right > clientArea.Right)) ? LineIntersectionPoint(new PointF(clientArea.X, clientArea.Y + (float)j), new PointF(clientArea.Right, clientArea.Y + (float)j), list[k], list[k + 1]) : LineIntersectionPoint(new PointF(clientArea.X, clientArea.Y + (float)j), new PointF(floatingItem.TextWrappingBounds.Right, clientArea.Y + (float)j), list[k], list[k + 1]));
					if (pointF2.X != 0f)
					{
						if (IsLineSlopeIsLeftToRight(list[k], list[k + 1]))
						{
							pointF2.Y = float.MinValue;
						}
						list2.Add(pointF2);
					}
				}
			}
			list2.Sort(new SortPointByX());
			if (list2.Count <= 0)
			{
				continue;
			}
			if (list2.Count > 1 && list2.Count % 2 == 0 && minimumInterSectPoint.X == 0f)
			{
				minimumInterSectPoint = list2[0];
				maximumIntersectPoint = list2[1];
				if (minimumInterSectPoint.Y == float.MinValue)
				{
					minimumInterSectPoint.X -= (float)Math.Round(1.0, 2);
				}
				if (maximumIntersectPoint.Y == float.MinValue)
				{
					maximumIntersectPoint.X += 1f;
				}
			}
			else if (list2.Count % 2 == 1 && list2[0].X != 0f && maximumIntersectPoint.X < list2[0].X)
			{
				maximumIntersectPoint = list2[0];
				if (maximumIntersectPoint.Y == float.MinValue)
				{
					maximumIntersectPoint.X += 1f;
				}
			}
			else if (list2.Count != 1)
			{
				PointF pointF3 = list2[0];
				PointF pointF4 = list2[1];
				if (pointF3.X != 0f && minimumInterSectPoint.X > pointF3.X)
				{
					minimumInterSectPoint = pointF3;
					if (minimumInterSectPoint.Y == float.MinValue)
					{
						minimumInterSectPoint.X -= (float)Math.Round(1.0, 2);
					}
				}
				if (pointF3.X != 0f && maximumIntersectPoint.X < pointF4.X)
				{
					maximumIntersectPoint = pointF4;
					if (maximumIntersectPoint.Y == float.MinValue)
					{
						maximumIntersectPoint.X += 1f;
					}
				}
			}
			list2.Clear();
		}
	}

	private bool IsLineSlopeIsLeftToRight(PointF firstPoint, PointF secondPoint)
	{
		if ((firstPoint.Y < secondPoint.Y && firstPoint.X < secondPoint.X) || (firstPoint.Y > secondPoint.Y && firstPoint.X > secondPoint.X))
		{
			return true;
		}
		return false;
	}

	private bool pnpoly(PointF[] poly, PointF pnt)
	{
		int num = poly.Length;
		bool flag = false;
		int num2 = 0;
		int num3 = num - 1;
		while (num2 < num)
		{
			if (poly[num2].Y > pnt.Y != poly[num3].Y > pnt.Y && pnt.X < (poly[num3].X - poly[num2].X) * (pnt.Y - poly[num2].Y) / (poly[num3].Y - poly[num2].Y) + poly[num2].X)
			{
				flag = !flag;
			}
			num3 = num2++;
		}
		return flag;
	}

	private bool IsNeedToUpdateYPosition(Entity floatingEntity)
	{
		if (GetBaseTextBody(floatingEntity) is HeaderFooter)
		{
			return false;
		}
		return true;
	}

	private void UpdateParagraphYPositionBasedonTextWrap()
	{
		if ((m_lcOperator as Layouter).FloatingItems.Count <= 0 || !(m_widget is WParagraph) || IsInFrame(m_widget as WParagraph) || IsInFootnote(m_widget as WParagraph) || GetFloattingItemIndex(m_widget as WParagraph) != -1 || (m_widget as WParagraph).IsFloatingItemsLayouted || (m_lcOperator as Layouter).IsLayoutingFootnote)
		{
			return;
		}
		SizeF size = m_widget.LayoutInfo.Size;
		bool flag = (m_widget as Entity).Document.Settings.CompatibilityMode == CompatibilityMode.Word2013;
		if (!(!(m_lcOperator as Layouter).IsLayoutingHeaderFooter || (m_widget as WParagraph).IsInCell || flag))
		{
			return;
		}
		RectangleF clientActiveArea = m_layoutArea.ClientActiveArea;
		RectangleF clientActiveArea2 = m_layoutArea.ClientActiveArea;
		float y = clientActiveArea2.Y;
		ParagraphLayoutInfo paragraphLayoutInfo = LayoutInfo as ParagraphLayoutInfo;
		float num = 0f;
		bool flag2 = false;
		if (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter && (m_lcOperator as Layouter).WrappingDifference == float.MinValue && Math.Round(clientActiveArea2.Y, 2) == Math.Round((m_lcOperator as Layouter).PageTopMargin, 2))
		{
			num = clientActiveArea2.Y;
			flag2 = true;
		}
		bool flag3 = false;
		FloatingItem.SortXYPostionFloatingItems((m_lcOperator as Layouter).FloatingItems, clientActiveArea2, size);
		Entity previousItem = GetPreviousItem(m_widget as WParagraph);
		for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
		{
			if ((m_widget as WParagraph).IsInCell && (m_lcOperator as Layouter).FloatingItems[i].AllowOverlap && ((m_widget as WParagraph).GetOwnerEntity() as WTableCell).OwnerRow.OwnerTable.TableFormat.Positioning.AllowOverlap && (!((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity is WTable) || !((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity as WTable).IsInCell))
			{
				WParagraph ownerParagraph = (m_lcOperator as Layouter).FloatingItems[i].OwnerParagraph;
				if (ownerParagraph == null || !ownerParagraph.IsInCell || (m_widget as WParagraph).GetOwnerEntity() != ownerParagraph.GetOwnerEntity())
				{
					continue;
				}
			}
			float x = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds.X;
			RectangleF textWrappingBounds = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds;
			TextWrappingStyle textWrappingStyle = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingStyle;
			if ((m_widget as WParagraph).IsInCell && ((m_widget as WParagraph).GetOwnerEntity() as WTableCell).OwnerRow.OwnerTable.TableFormat.WrapTextAround && textWrappingStyle == TextWrappingStyle.TopAndBottom)
			{
				WParagraph ownerParagraph2 = (m_lcOperator as Layouter).FloatingItems[i].OwnerParagraph;
				if (ownerParagraph2 == null || !ownerParagraph2.IsInCell || (m_widget as WParagraph).GetOwnerEntity() != ownerParagraph2.GetOwnerEntity())
				{
					continue;
				}
			}
			bool flag4 = (textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds.Width > (m_lcOperator as Layouter).CurrentSection.PageSetup.PageSize.Width && (m_lcOperator as Layouter).FloatingItems[i].IsDoesNotDenotesRectangle;
			WTextBody ownerBody = null;
			WParagraph wParagraph = m_widget as WParagraph;
			if ((!IsInSameTextBody(wParagraph, (m_lcOperator as Layouter).FloatingItems[i], ref ownerBody) && wParagraph.IsInCell && ownerBody is WTableCell) || (IsInFrame((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity as WParagraph) && IsOwnerCellInFrame(wParagraph)))
			{
				continue;
			}
			if ((m_widget as WParagraph).ParagraphFormat.Bidi && IsInSameTextBody(wParagraph, (m_lcOperator as Layouter).FloatingItems[i], ref ownerBody) && wParagraph.IsInCell && ownerBody is WTableCell)
			{
				ModifyXPositionForRTLLayouting(i, ref textWrappingBounds, m_layoutArea.ClientArea);
			}
			else if ((m_widget as WParagraph).ParagraphFormat.Bidi)
			{
				ModifyXPositionForRTLLayouting(i, ref textWrappingBounds, (m_lcOperator as Layouter).ClientLayoutArea);
			}
			float num2 = 0f;
			if ((m_widget as Entity).Owner is WTableCell)
			{
				CellLayoutInfo cellLayoutInfo = ((m_widget as Entity).Owner as IWidget).LayoutInfo as CellLayoutInfo;
				num2 = cellLayoutInfo.Paddings.Left + cellLayoutInfo.Paddings.Right;
			}
			float num3 = 18f;
			if (textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through)
			{
				num3 = (flag ? 17.6f : 8f);
			}
			num3 -= num2;
			bool allowOverlap = (m_lcOperator as Layouter).FloatingItems[i].AllowOverlap;
			if ((textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && !(GetBaseTextBody((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity) is HeaderFooter) && !(m_lcOperator as Layouter).FloatingItems[i].IsDoesNotDenotesRectangle)
			{
				float floattingItemBottom = GetFloattingItemBottom((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity, textWrappingBounds.Bottom);
				float num4 = 0f;
				if ((m_widget as WParagraph).ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly)
				{
					num4 = (m_widget as WParagraph).ParagraphFormat.LineSpacing;
				}
				else
				{
					num4 = GetMultipleFactorValue(m_widget as WParagraph);
					if ((m_widget as WParagraph).ParagraphFormat.LineSpacingRule == LineSpacingRule.AtLeast && num4 < (m_widget as WParagraph).ParagraphFormat.LineSpacing)
					{
						num4 = (m_widget as WParagraph).ParagraphFormat.LineSpacing;
					}
				}
				float exceededBottomValueForTightAndThrough = GetExceededBottomValueForTightAndThrough(floattingItemBottom, clientActiveArea2.Y, num4, m_widget as WParagraph, isSplittedLine: false);
				if (exceededBottomValueForTightAndThrough > 0f)
				{
					floattingItemBottom += exceededBottomValueForTightAndThrough;
					if (floattingItemBottom < textWrappingBounds.Height)
					{
						floattingItemBottom += num4;
					}
					textWrappingBounds.Height = floattingItemBottom - textWrappingBounds.Y;
				}
			}
			if ((textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through) && (m_lcOperator as Layouter).FloatingItems[i].IsDoesNotDenotesRectangle)
			{
				textWrappingBounds = AdjustTightAndThroughBounds((m_lcOperator as Layouter).FloatingItems[i], clientActiveArea2, size.Height);
			}
			float adjustingValue = 0f;
			bool flag5 = IsNeedToConsiderAdjustValues(ref adjustingValue, wParagraph, textWrappingStyle, i);
			if (!(IsInTextBox(m_widget as WParagraph) is WTextBox && allowOverlap) && !(flag5 ? (clientActiveArea.X > textWrappingBounds.Right + adjustingValue) : (clientActiveArea.X > textWrappingBounds.Right + num3)) && !(clientActiveArea.Right < textWrappingBounds.X - num3))
			{
				if ((clientActiveArea2.Y + size.Height > textWrappingBounds.Y || flag3) && clientActiveArea2.Y < textWrappingBounds.Bottom && textWrappingStyle != 0 && textWrappingStyle != TextWrappingStyle.TopAndBottom && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind)
				{
					float num5 = paragraphLayoutInfo.Margins.Right;
					_ = paragraphLayoutInfo.Margins.Left;
					if (paragraphLayoutInfo.IsFirstLine)
					{
						_ = paragraphLayoutInfo.Margins.Left;
						_ = paragraphLayoutInfo.FirstLineIndent;
					}
					num5 = ((num5 < 0f) ? num5 : 0f);
					if ((m_widget as WParagraph).ParagraphFormat.GetAlignmentToRender() != 0 && clientActiveArea2.X < textWrappingBounds.X && clientActiveArea2.X + size.Width > textWrappingBounds.X)
					{
						if (clientActiveArea2.Right > textWrappingBounds.X)
						{
							clientActiveArea2.Width -= clientActiveArea2.Right - textWrappingBounds.Right;
						}
						if (clientActiveArea2.Width < num3)
						{
							if (flag || !((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity is WTable) || previousItem != (m_lcOperator as Layouter).FloatingItems[i].FloatingEntity)
							{
								if (flag4)
								{
									m_layoutArea.UpdateBoundsBasedOnTextWrap(paragraphLayoutInfo.YPosition + size.Height);
								}
								else
								{
									m_layoutArea.UpdateBoundsBasedOnTextWrap(textWrappingBounds.Bottom);
								}
								clientActiveArea2 = m_layoutArea.ClientActiveArea;
							}
							if ((paragraphLayoutInfo.IsFirstLine && flag) || (IsNeedToUpdateYPosition((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity) && !flag4))
							{
								paragraphLayoutInfo.YPosition = m_layoutArea.ClientActiveArea.Y;
							}
						}
						else
						{
							clientActiveArea2.X = textWrappingBounds.Right;
						}
					}
					else if (clientActiveArea2.X >= textWrappingBounds.X && clientActiveArea2.X < textWrappingBounds.Right)
					{
						clientActiveArea2.Width = clientActiveArea2.Width - (textWrappingBounds.Right - clientActiveArea2.X) - num5;
						if (clientActiveArea2.Width < num3 || flag3)
						{
							clientActiveArea2.Width = m_layoutArea.ClientActiveArea.Right - textWrappingBounds.Right - num5;
							bool flag6 = false;
							if (clientActiveArea2.Width < num3 || flag3)
							{
								if (m_layoutArea.ClientActiveArea.X + num3 < (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds.X)
								{
									RectangleF intersectingItemBounds = FloatingItem.GetIntersectingItemBounds(m_lcOperator as Layouter, (m_lcOperator as Layouter).FloatingItems[i], y);
									if (intersectingItemBounds != RectangleF.Empty && intersectingItemBounds.Bottom <= textWrappingBounds.Bottom)
									{
										m_layoutArea.UpdateBoundsBasedOnTextWrap(intersectingItemBounds.Bottom);
										clientActiveArea2 = m_layoutArea.ClientActiveArea;
										flag6 = true;
										(wParagraph.m_layoutInfo as ParagraphLayoutInfo).XPosition = intersectingItemBounds.X;
									}
								}
								if ((flag || !((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity is WTable) || previousItem != (m_lcOperator as Layouter).FloatingItems[i].FloatingEntity) && !flag6)
								{
									if (flag4)
									{
										m_layoutArea.UpdateBoundsBasedOnTextWrap(paragraphLayoutInfo.YPosition + size.Height);
									}
									else
									{
										m_layoutArea.UpdateBoundsBasedOnTextWrap(textWrappingBounds.Bottom);
									}
									clientActiveArea2 = m_layoutArea.ClientActiveArea;
								}
								if (((paragraphLayoutInfo.IsFirstLine && flag) || IsNeedToUpdateYPosition((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity)) && !flag4)
								{
									paragraphLayoutInfo.YPosition = m_layoutArea.ClientActiveArea.Y;
								}
							}
							else
							{
								clientActiveArea2.X = textWrappingBounds.Right;
							}
						}
						else
						{
							clientActiveArea2.X = textWrappingBounds.Right;
						}
					}
					else if (textWrappingBounds.X > clientActiveArea2.X && clientActiveArea2.Right > textWrappingBounds.X)
					{
						clientActiveArea2.Width = textWrappingBounds.X - clientActiveArea2.X - num5;
						if (clientActiveArea2.Width < num3)
						{
							clientActiveArea2.Width = m_layoutArea.ClientActiveArea.Right - textWrappingBounds.Right - num5;
							if (clientActiveArea2.Width < num3)
							{
								if (flag || !((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity is WTable) || previousItem != (m_lcOperator as Layouter).FloatingItems[i].FloatingEntity)
								{
									if (flag4)
									{
										m_layoutArea.UpdateBoundsBasedOnTextWrap(paragraphLayoutInfo.YPosition + size.Height);
									}
									else
									{
										m_layoutArea.UpdateBoundsBasedOnTextWrap(textWrappingBounds.Bottom);
									}
									clientActiveArea2 = m_layoutArea.ClientActiveArea;
								}
								if ((paragraphLayoutInfo.IsFirstLine && flag) || (IsNeedToUpdateYPosition((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity) && !flag4))
								{
									paragraphLayoutInfo.YPosition = m_layoutArea.ClientActiveArea.Y;
								}
							}
							else
							{
								clientActiveArea2.X = textWrappingBounds.Right;
							}
						}
					}
				}
				else if ((m_lcOperator as Layouter).FloatingItems.Count > 0 && ((clientActiveArea2.Y >= textWrappingBounds.Y && clientActiveArea2.Y < textWrappingBounds.Bottom) || (clientActiveArea2.Y + size.Height >= textWrappingBounds.Y && clientActiveArea2.Y + size.Height < textWrappingBounds.Bottom)) && textWrappingStyle == TextWrappingStyle.TopAndBottom && IsFrameInClientArea((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity as WParagraph, textWrappingBounds))
				{
					float bottomValueForSquareAndTopandBottom = GetBottomValueForSquareAndTopandBottom(m_widget as WParagraph);
					if (flag || !((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity is WTable) || previousItem != (m_lcOperator as Layouter).FloatingItems[i].FloatingEntity)
					{
						if (flag4)
						{
							m_layoutArea.UpdateBoundsBasedOnTextWrap(paragraphLayoutInfo.YPosition + size.Height);
						}
						else if (paragraphLayoutInfo.IsFirstLine && paragraphLayoutInfo.Margins.Top == 0f)
						{
							m_layoutArea.UpdateBoundsBasedOnTextWrap(textWrappingBounds.Bottom + bottomValueForSquareAndTopandBottom);
						}
						else
						{
							m_layoutArea.UpdateBoundsBasedOnTextWrap(textWrappingBounds.Bottom);
						}
						if (!flag && paragraphLayoutInfo.IsFirstLine && !flag4)
						{
							flag3 = true;
						}
					}
					if ((paragraphLayoutInfo.IsFirstLine && flag) || (IsNeedToUpdateYPosition((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity) && !flag4))
					{
						paragraphLayoutInfo.YPosition = m_layoutArea.ClientActiveArea.Y;
					}
				}
			}
			ResetXPositionForRTLLayouting(i, ref textWrappingBounds, x);
		}
		if (flag2 && num < clientActiveArea2.Y)
		{
			(m_lcOperator as Layouter).WrappingDifference = clientActiveArea2.Y - (m_lcOperator as Layouter).PageTopMargin;
		}
	}

	private Entity GetPreviousItem(WParagraph wParagraph)
	{
		Entity entity = wParagraph.PreviousSibling as Entity;
		if (entity is WParagraph)
		{
			wParagraph = entity as WParagraph;
			if (wParagraph.m_layoutInfo.IsSkip)
			{
				entity = wParagraph.PreviousSibling as Entity;
				if (entity is WParagraph)
				{
					entity = GetPreviousItem(entity as WParagraph);
				}
			}
		}
		return entity;
	}

	internal bool IsInSameTextBody(TextBodyItem bodyItem, FloatingItem fItem, ref WTextBody ownerBody)
	{
		if (fItem.FloatingEntity is WParagraph || fItem.FloatingEntity is WTable)
		{
			ownerBody = fItem.FloatingEntity.Owner as WTextBody;
		}
		else if (fItem.OwnerParagraph != null)
		{
			ownerBody = fItem.OwnerParagraph.OwnerTextBody;
		}
		if (ownerBody != null)
		{
			return bodyItem.OwnerTextBody == ownerBody;
		}
		return false;
	}

	internal void UpdateXYPositionBasedOnAdjacentFloatingItems(List<FloatingItem> floatingItems, RectangleF rect, SizeF size, WParagraph paragraph, bool isFromLeafLayoutContext)
	{
		List<FloatingItem> list = new List<FloatingItem>();
		for (int i = 0; i < floatingItems.Count; i++)
		{
			if (IsSquareOrTightAndThrow(floatingItems[i]) && IsYPositionIntersect(floatingItems[i].TextWrappingBounds, rect, size.Height))
			{
				list.Add(floatingItems[i]);
			}
		}
		if (list.Count <= 1)
		{
			return;
		}
		FloatingItem.SortFloatingItems(list, SortPosition.X, isNeedToUpdateWrapCollectionIndex: false);
		bool flag = false;
		string wordVersion = GetWordVersion(paragraph);
		for (int j = 0; j + 1 < list.Count && list[j].TextWrappingBounds.Right <= rect.X; j++)
		{
			if (!(list[j + 1].TextWrappingBounds.X >= rect.X))
			{
				continue;
			}
			float minWidthBetweenFloatingItems = GetMinWidthBetweenFloatingItems(list[j].TextWrappingStyle, list[j + 1].TextWrappingStyle, wordVersion);
			if (list[j + 1].TextWrappingBounds.X - list[j].TextWrappingBounds.Right <= minWidthBetweenFloatingItems)
			{
				if (m_layoutArea.ClientActiveArea.Right - list[j + 1].TextWrappingBounds.Right < GetMinWidth(list[j].TextWrappingStyle, paragraph))
				{
					flag = true;
					break;
				}
				rect.Width -= list[j + 1].TextWrappingBounds.Right - rect.X;
				rect.X = list[j + 1].TextWrappingBounds.Right;
				if (isFromLeafLayoutContext)
				{
					CreateLayoutArea(rect);
				}
				else
				{
					(paragraph.m_layoutInfo as ParagraphLayoutInfo).XPosition = rect.X;
				}
			}
		}
		if (flag)
		{
			UpdateYPosition(list, rect, size, paragraph, isFromLeafLayoutContext);
		}
	}

	private void UpdateYPosition(List<FloatingItem> interSectingItems, RectangleF rect, SizeF size, WParagraph paragraph, bool isFromLeafLayoutContext)
	{
		int i = 0;
		float num = (m_lcOperator as Layouter).ClientLayoutArea.X;
		float num2 = interSectingItems[i].TextWrappingBounds.Bottom;
		float height = interSectingItems[i].TextWrappingBounds.Height;
		for (; i + 1 < interSectingItems.Count; i++)
		{
			FloatingItem floatingItem = interSectingItems[i + 1];
			if (num2 >= floatingItem.TextWrappingBounds.Bottom)
			{
				num2 = GetFloattingItemBottom(floatingItem.FloatingEntity, floatingItem.TextWrappingBounds.Bottom);
				if (num2 != float.MinValue && (floatingItem.TextWrappingStyle == TextWrappingStyle.Tight || floatingItem.TextWrappingStyle == TextWrappingStyle.Through) && !(GetBaseEntity(floatingItem.FloatingEntity) is HeaderFooter) && !floatingItem.IsDoesNotDenotesRectangle)
				{
					RectangleF bottomPositionForTightAndThrough = GetBottomPositionForTightAndThrough(num2, floatingItem.TextWrappingBounds, paragraph, rect.Y, size.Height);
					num = bottomPositionForTightAndThrough.Right;
					num2 = bottomPositionForTightAndThrough.Y;
					height = bottomPositionForTightAndThrough.Height;
				}
				else
				{
					num = floatingItem.TextWrappingBounds.Right;
					height = floatingItem.TextWrappingBounds.Height;
				}
			}
		}
		if (isFromLeafLayoutContext)
		{
			rect.Width -= num - rect.X;
			rect.Height -= height;
			rect.X = num;
			rect.Y = num2;
			CreateLayoutArea(rect);
		}
		else
		{
			m_layoutArea.UpdateBoundsBasedOnTextWrap(num2);
			(paragraph.m_layoutInfo as ParagraphLayoutInfo).XPosition = num;
		}
	}

	private float GetMinWidth(TextWrappingStyle textWrappingStyle, WParagraph ownerParagraph)
	{
		float num = 0f;
		if (ownerParagraph.Owner is WTableCell)
		{
			CellLayoutInfo cellLayoutInfo = (ownerParagraph.Owner as IWidget).LayoutInfo as CellLayoutInfo;
			num = cellLayoutInfo.Paddings.Left + cellLayoutInfo.Paddings.Right;
		}
		float num2 = 18f;
		if (textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through)
		{
			num2 = ((ownerParagraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) ? 17.6f : 8f);
		}
		return num2 - num;
	}

	private string GetWordVersion(WParagraph paragraph)
	{
		if (paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2003)
		{
			return "Word2003";
		}
		if (paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2007)
		{
			return "Word2007";
		}
		if (paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2010)
		{
			return "Word2010";
		}
		return "Word2013";
	}

	private float GetMinWidthBetweenFloatingItems(TextWrappingStyle leftStyle, TextWrappingStyle rightStyle, string wordVersion)
	{
		float result = 0f;
		if (leftStyle == TextWrappingStyle.Square && rightStyle == TextWrappingStyle.Square)
		{
			result = ((!(wordVersion == "Word2003")) ? 18f : 19f);
		}
		else if (leftStyle == TextWrappingStyle.Square && (rightStyle == TextWrappingStyle.Tight || rightStyle == TextWrappingStyle.Through))
		{
			switch (wordVersion)
			{
			case "Word2003":
				result = 10f;
				break;
			case "Word2007":
			case "Word2010":
				result = 9f;
				break;
			case "Word2013":
				result = 18f;
				break;
			}
		}
		else if ((leftStyle == TextWrappingStyle.Tight || leftStyle == TextWrappingStyle.Through) && (rightStyle == TextWrappingStyle.Tight || rightStyle == TextWrappingStyle.Through || rightStyle == TextWrappingStyle.Square))
		{
			switch (wordVersion)
			{
			case "Word2003":
				result = ((rightStyle != TextWrappingStyle.Square) ? 10f : 9f);
				break;
			case "Word2007":
			case "Word2010":
				result = 7f;
				break;
			case "Word2013":
				result = 16f;
				break;
			}
		}
		return result;
	}

	private bool IsSquareOrTightAndThrow(FloatingItem floatingItem)
	{
		TextWrappingStyle textWrappingStyle = floatingItem.TextWrappingStyle;
		if (textWrappingStyle == TextWrappingStyle.Square || textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through)
		{
			return true;
		}
		return false;
	}

	internal bool IsYPositionIntersect(RectangleF floatingItemBounds, RectangleF currentItemBounds, float height)
	{
		if ((Math.Round(currentItemBounds.Y, 2) > Math.Round(floatingItemBounds.Y, 2) && Math.Round(currentItemBounds.Y, 2) < Math.Round(floatingItemBounds.Bottom, 2)) || (Math.Round(currentItemBounds.Y + height, 2) > Math.Round(floatingItemBounds.Y, 2) && Math.Round(currentItemBounds.Y + height, 2) < Math.Round(floatingItemBounds.Bottom, 2)) || (Math.Round(currentItemBounds.Y, 2) < Math.Round(floatingItemBounds.Bottom, 2) && Math.Round(currentItemBounds.Y, 2) > Math.Round(floatingItemBounds.Y, 2)))
		{
			return true;
		}
		return false;
	}

	internal RectangleF GetBottomPositionForTightAndThrough(float floattingItemBottomPosition, RectangleF textWrappingBounds, WParagraph paragraph, float yPostion, float leafWidgetHeight)
	{
		float num = 0f;
		bool isSplittedLine = false;
		if (paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly)
		{
			num = paragraph.ParagraphFormat.LineSpacing;
		}
		else
		{
			if (paragraph.ChildEntities.Count == 0)
			{
				num = ((IWidget)paragraph).LayoutInfo.Size.Height;
			}
			else
			{
				isSplittedLine = true;
				num = leafWidgetHeight;
			}
			if (paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.AtLeast && num < paragraph.ParagraphFormat.LineSpacing)
			{
				num = paragraph.ParagraphFormat.LineSpacing;
			}
		}
		float exceededBottomValueForTightAndThrough = GetExceededBottomValueForTightAndThrough(floattingItemBottomPosition, yPostion, num, paragraph, isSplittedLine);
		if (exceededBottomValueForTightAndThrough > 0f)
		{
			floattingItemBottomPosition += exceededBottomValueForTightAndThrough;
			if (floattingItemBottomPosition < textWrappingBounds.Height)
			{
				floattingItemBottomPosition += num;
			}
			textWrappingBounds.Height = floattingItemBottomPosition - textWrappingBounds.Y;
		}
		return textWrappingBounds;
	}

	private float GetMultipleFactorValue(WParagraph paragraph)
	{
		float result = 0f;
		if (paragraph.ChildEntities.Count > 0)
		{
			int num = 0;
			bool flag = false;
			for (num = 0; num < paragraph.ChildEntities.Count; num++)
			{
				if (paragraph.ChildEntities[num].EntityType != EntityType.BookmarkStart && paragraph.ChildEntities[num].EntityType != EntityType.BookmarkEnd)
				{
					flag = true;
					break;
				}
			}
			if (flag && paragraph.ChildEntities[num] is ILeafWidget)
			{
				result = (paragraph.ChildEntities[num] as ILeafWidget).Measure(DrawingContext).Height;
			}
			else if (paragraph.m_layoutInfo is ParagraphLayoutInfo)
			{
				result = (paragraph.m_layoutInfo as ParagraphLayoutInfo).Size.Height;
			}
		}
		else if (paragraph.m_layoutInfo is ParagraphLayoutInfo)
		{
			result = (paragraph.m_layoutInfo as ParagraphLayoutInfo).Size.Height;
		}
		return result;
	}

	internal float GetFloattingItemBottom(Entity entity, float bottom)
	{
		if (entity is WPicture)
		{
			WPicture wPicture = entity as WPicture;
			bottom -= wPicture.DistanceFromBottom;
		}
		else if (entity is WTextBox)
		{
			WTextBox wTextBox = entity as WTextBox;
			bottom -= wTextBox.TextBoxFormat.WrapDistanceBottom;
		}
		else if (entity is Shape || entity is WChart || entity is GroupShape)
		{
			ShapeBase shapeBase = ((entity is Shape) ? (entity as Shape) : ((!(entity is GroupShape)) ? ((ShapeBase)(entity as WChart)) : ((ShapeBase)(entity as GroupShape))));
			bottom -= shapeBase.WrapFormat.DistanceBottom;
		}
		return bottom;
	}

	private float GetExceededBottomValueForTightAndThrough(float floattingItemBottomPosition, float yPosition, float multipleFactorValue, WParagraph paragraph, bool isSplittedLine)
	{
		float num = 0f;
		bool flag = false;
		if (!isSplittedLine && (!(paragraph.m_layoutInfo is ParagraphLayoutInfo) || !(paragraph.m_layoutInfo as ParagraphLayoutInfo).IsFirstItemInPage))
		{
			WParagraph previousParagraph = GetPreviousParagraph(paragraph);
			if (previousParagraph != null)
			{
				if (previousParagraph.ParagraphFormat.AfterSpacing >= paragraph.ParagraphFormat.BeforeSpacing)
				{
					num = previousParagraph.ParagraphFormat.AfterSpacing - 2f;
				}
				else
				{
					flag = true;
					num = paragraph.ParagraphFormat.BeforeSpacing - 2f;
				}
				yPosition -= previousParagraph.ParagraphFormat.AfterSpacing - 2f;
			}
		}
		else if (paragraph.m_layoutInfo is ParagraphLayoutInfo && (paragraph.m_layoutInfo as ParagraphLayoutInfo).IsFirstItemInPage)
		{
			num = paragraph.ParagraphFormat.BeforeSpacing - 2f;
		}
		float num2 = floattingItemBottomPosition - yPosition;
		num2 %= multipleFactorValue;
		num2 = ((!(num2 < 2f)) ? (multipleFactorValue - num2) : 0f);
		num2 += num;
		if (!flag)
		{
			num2 %= multipleFactorValue;
		}
		return num2;
	}

	private float GetBottomValueForSquareAndTopandBottom(WParagraph paragraph)
	{
		float result = 0f;
		WParagraph previousParagraph = GetPreviousParagraph(paragraph);
		if (previousParagraph != null)
		{
			result = ((!(previousParagraph.ParagraphFormat.AfterSpacing <= paragraph.ParagraphFormat.BeforeSpacing)) ? 0f : (paragraph.ParagraphFormat.BeforeSpacing - previousParagraph.ParagraphFormat.AfterSpacing));
		}
		return result;
	}

	internal WParagraph GetPreviousParagraph(WParagraph paragrph)
	{
		IEntity previousSibling = paragrph.PreviousSibling;
		if (previousSibling is WParagraph)
		{
			return previousSibling as WParagraph;
		}
		if (previousSibling is BlockContentControl)
		{
			return GetPreviousParagraphIsInSDTContent(previousSibling as BlockContentControl);
		}
		if (previousSibling is WTable)
		{
			return GetPreviousParagraphIsInTable(previousSibling as WTable);
		}
		return null;
	}

	private WParagraph GetPreviousParagraphIsInSDTContent(BlockContentControl sdtContent)
	{
		BodyItemCollection items = sdtContent.TextBody.Items;
		IEntity entity = items[items.Count - 1];
		if (entity is WParagraph)
		{
			return entity as WParagraph;
		}
		if (entity is BlockContentControl)
		{
			return GetPreviousParagraphIsInSDTContent(entity as BlockContentControl);
		}
		if (entity is WTable)
		{
			return GetPreviousParagraphIsInTable(entity as WTable);
		}
		return null;
	}

	private WParagraph GetPreviousParagraphIsInTable(WTable table)
	{
		IEntity entity = table.LastCell.WidgetInnerCollection[table.LastCell.WidgetInnerCollection.Count - 1];
		if (entity is WParagraph)
		{
			return entity as WParagraph;
		}
		if (entity is BlockContentControl)
		{
			return GetPreviousParagraphIsInSDTContent(entity as BlockContentControl);
		}
		if (entity is WTable)
		{
			return GetPreviousParagraphIsInTable(entity as WTable);
		}
		return null;
	}

	internal int GetFloattingItemIndex(Entity entity)
	{
		while (entity != null && entity.EntityType != EntityType.TextBox && entity.EntityType != EntityType.TextBox && !(entity is Shape) && !(entity is GroupShape))
		{
			entity = entity.Owner;
		}
		if (entity != null && entity is WTextBox)
		{
			return (entity as WTextBox).TextBoxFormat.WrapCollectionIndex;
		}
		if (entity != null && entity is Shape)
		{
			return (entity as Shape).WrapFormat.WrapCollectionIndex;
		}
		if (entity != null && entity is GroupShape)
		{
			return (entity as GroupShape).WrapFormat.WrapCollectionIndex;
		}
		return -1;
	}

	internal bool IsInFootnote(WParagraph paragraph)
	{
		Entity owner = paragraph.Owner;
		while (!(owner is WFootnote) && owner != null)
		{
			owner = owner.Owner;
		}
		if (owner is WFootnote)
		{
			return true;
		}
		return false;
	}

	internal Entity IsInTextBox(WParagraph paragraph)
	{
		Entity owner = paragraph.Owner;
		while (!(owner is WTextBox) && owner != null)
		{
			owner = owner.Owner;
		}
		if (owner is WTextBox)
		{
			return owner;
		}
		return null;
	}

	internal float GetPageMarginLeft(WParagraph paragraph)
	{
		float left = (m_lcOperator as Layouter).ClientLayoutArea.Left;
		if (paragraph != null && paragraph.IsInCell)
		{
			left = ((paragraph.GetOwnerEntity() as WTableCell).m_layoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Left;
		}
		return left;
	}

	private void UpdateParagraphTopMargin(WParagraph paragraph)
	{
		Borders borders = paragraph.ParagraphFormat.Borders;
		if (!borders.NoBorder && borders.Top.BorderType != 0 && (LayoutInfo as ParagraphLayoutInfo).Paddings.Top == 0f && LayoutInfo is ParagraphLayoutInfo && (LayoutInfo as ParagraphLayoutInfo).IsFirstLine)
		{
			(LayoutInfo as ParagraphLayoutInfo).Paddings.Top = borders.Top.Space;
		}
		if (!paragraph.IsTopMarginValueUpdated && (LayoutInfo as ParagraphLayoutInfo).Margins.Top == 0f)
		{
			if (paragraph.ParagraphFormat.SpaceBeforeAuto && !paragraph.Document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing)
			{
				if ((LayoutInfo as ParagraphLayoutInfo).ListValue != string.Empty || paragraph.IsFirstParagraphOfOwnerTextBody())
				{
					(LayoutInfo as ParagraphLayoutInfo).Margins.Top = 0f;
				}
				else
				{
					(LayoutInfo as ParagraphLayoutInfo).Margins.Top = 14f;
				}
			}
			else
			{
				(LayoutInfo as ParagraphLayoutInfo).Margins.Top = paragraph.ParagraphFormat.BeforeSpacing;
			}
		}
		if (paragraph.IsTopMarginValueUpdated || paragraph.IsFirstParagraphOfOwnerTextBody())
		{
			return;
		}
		if (!(paragraph.OwnerTextBody.Owner is BlockContentControl) && !paragraph.IsInCell && (((IsPageBreak(m_widget) || paragraph.ParagraphFormat.PageBreakBefore || (paragraph.PreviousSibling is WParagraph && (paragraph.PreviousSibling as WParagraph).ParagraphFormat.PageBreakAfter)) && paragraph.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013) || IsSectionBreak(paragraph) || IsTOC(paragraph)))
		{
			if (paragraph.ParagraphFormat.SpaceBeforeAuto && !paragraph.Document.DOP.Dop2000.Copts.DontUseHTMLParagraphAutoSpacing)
			{
				(LayoutInfo as ParagraphLayoutInfo).Margins.Top = 14f;
			}
			else
			{
				(LayoutInfo as ParagraphLayoutInfo).Margins.Top = paragraph.ParagraphFormat.BeforeSpacing;
			}
		}
		else
		{
			(LayoutInfo as ParagraphLayoutInfo).Margins.Top = 0f;
		}
	}

	private bool IsTOC(WParagraph paragraph)
	{
		Hyperlink hyperlink;
		if (paragraph.ChildEntities.FirstItem is TableOfContent || (paragraph.ChildEntities.FirstItem is WField && (paragraph.ChildEntities.FirstItem as WField).FieldType == FieldType.FieldHyperlink && (hyperlink = new Hyperlink(paragraph.ChildEntities.FirstItem as WField)).BookmarkName != null && StartsWithExt(hyperlink.BookmarkName, "_Toc")))
		{
			if (IsPageBreak(m_widget) || paragraph.ParagraphFormat.PageBreakBefore || (paragraph.PreviousSibling is WParagraph && (paragraph.PreviousSibling as WParagraph).ParagraphFormat.PageBreakAfter))
			{
				return true;
			}
			return false;
		}
		return false;
	}

	private bool IsPageBreak(IWidget childWidget)
	{
		if (childWidget is SplitWidgetContainer && (childWidget as SplitWidgetContainer).RealWidgetContainer is WParagraph)
		{
			WParagraph wParagraph = (childWidget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
			int count = wParagraph.ChildEntities.Count;
			if (count > (childWidget as SplitWidgetContainer).Count && wParagraph.ChildEntities[count - 1 - (childWidget as SplitWidgetContainer).Count] is Break)
			{
				Break @break = wParagraph.ChildEntities[count - 1 - (childWidget as SplitWidgetContainer).Count] as Break;
				if (@break.BreakType != 0)
				{
					return @break.BreakType == BreakType.ColumnBreak;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	private bool IsParagraphFirstItemHasPageOrColumnBreak(WParagraph paragraph)
	{
		foreach (ParagraphItem childEntity in paragraph.ChildEntities)
		{
			if (!(childEntity is BookmarkStart) && !(childEntity is BookmarkEnd))
			{
				if (childEntity is Break && ((childEntity as Break).BreakType == BreakType.PageBreak || (childEntity as Break).BreakType == BreakType.ColumnBreak))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	private bool IsSectionBreak(WParagraph para)
	{
		if (para.Owner is WTextBody && !para.IsInCell)
		{
			bool num = (para.Owner as WTextBody).Items[0] == para;
			IWSection iWSection = GetBaseEntity(para) as WSection;
			if (num && iWSection != null)
			{
				if (iWSection.BreakCode != SectionBreakCode.NewPage && iWSection.BreakCode != SectionBreakCode.Oddpage)
				{
					return iWSection.BreakCode == SectionBreakCode.EvenPage;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	protected void CreateLayoutArea(RectangleF rect, Paddings cellPadding)
	{
		m_layoutArea = new LayoutArea(rect, LayoutInfo as ILayoutSpacingsInfo, m_widget);
	}

	protected void CreateLayoutedWidget(PointF location)
	{
		m_ltWidget = new LayoutedWidget(m_widget);
		RectangleF bounds = m_ltWidget.Bounds;
		if ((m_widget is WTableCell || (m_widget is SplitWidgetContainer && (m_widget as SplitWidgetContainer).RealWidgetContainer is WTableCell)) && LayoutInfo is ILayoutSpacingsInfo)
		{
			location = m_layoutArea.ClientArea.Location;
		}
		else if (LayoutInfo is ILayoutSpacingsInfo)
		{
			location.X += (LayoutInfo as ILayoutSpacingsInfo).Margins.Left;
			location.Y += (LayoutInfo as ILayoutSpacingsInfo).Margins.Top;
			if (LayoutInfo is ParagraphLayoutInfo && !(LayoutInfo as ParagraphLayoutInfo).IsFirstLine)
			{
				location.Y -= (LayoutInfo as ILayoutSpacingsInfo).Margins.Top;
			}
		}
		bounds.Location = location;
		m_ltWidget.Bounds = bounds;
	}

	internal void UpdateAreaWidth(float previousTabPosition)
	{
		m_layoutArea.UpdateWidth(previousTabPosition);
	}

	internal bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}

	protected void UpdateForceFitLayoutState(LayoutContext childContext)
	{
		if (IsForceFitLayout && ((this is LCLineContainer && childContext is LCContainer) || childContext is LCLineContainer || childContext is LCTable || Widget is WSection || (Widget is SplitWidgetContainer && (Widget as SplitWidgetContainer).RealWidgetContainer is WSection) || Widget is WordDocument || (Widget is SplitWidgetContainer && (Widget as SplitWidgetContainer).RealWidgetContainer is WordDocument)))
		{
			IsForceFitLayout = false;
		}
	}

	public static LayoutContext Create(IWidget widget, ILCOperator lcOperator, bool isForceFitLayout)
	{
		if (widget is IWidgetContainer widgetContainer)
		{
			if (widgetContainer.LayoutInfo.IsLineContainer)
			{
				return new LCLineContainer(widgetContainer, lcOperator, isForceFitLayout);
			}
			return new LCContainer(widgetContainer, lcOperator, isForceFitLayout);
		}
		ILeafWidget leafWidget = widget as ILeafWidget;
		if (widget is WField && (widget as WField).FieldType == FieldType.FieldSymbol)
		{
			leafWidget = (widget as WField).GetAsSymbolOrTextRange() as ILeafWidget;
		}
		if (leafWidget != null)
		{
			if (leafWidget is WMath)
			{
				return new MathLayoutContext(leafWidget, lcOperator, isForceFitLayout);
			}
			return new LeafLayoutContext(leafWidget, lcOperator, isForceFitLayout);
		}
		ITableWidget tableWidget = null;
		if (widget is ITableWidget)
		{
			tableWidget = widget as ITableWidget;
		}
		if (tableWidget != null)
		{
			return new LCTable(tableWidget, lcOperator, isForceFitLayout);
		}
		if (widget is SplitTableWidget splitWidget)
		{
			return new LCTable(splitWidget, lcOperator, isForceFitLayout);
		}
		throw new ArgumentException("Invalid widget type: " + widget.GetType());
	}

	internal bool IsInFrame(WParagraph paragraph)
	{
		if (paragraph != null && paragraph.ParagraphFormat.IsFrame && !(paragraph.OwnerTextBody.Owner is WTextBox) && !(paragraph.OwnerTextBody.Owner is Shape) && !paragraph.IsInCell)
		{
			return true;
		}
		return false;
	}

	internal bool IsFrameInClientArea(WParagraph paragraph, RectangleF textWrappingBounds)
	{
		if (IsInFrame(paragraph) && (IsWord2013(paragraph.Document) || !(m_lcOperator as Layouter).IsLayoutingHeaderFooter))
		{
			return (m_lcOperator as Layouter).ClientLayoutArea.Right > textWrappingBounds.X - paragraph.ParagraphFormat.FrameHorizontalDistanceFromText;
		}
		return true;
	}

	internal RectangleF GetFrameBounds(WParagraph paragraph, RectangleF bounds)
	{
		WSection wSection = ((!(m_lcOperator is Layouter { IsLayoutingHeaderFooter: not false } layouter)) ? (GetBaseEntity(paragraph) as WSection) : (layouter.CurrentSection as WSection));
		WParagraphFormat paragraphFormat = paragraph.ParagraphFormat;
		float x = bounds.X;
		float y = bounds.Y;
		float width = bounds.Width;
		if (paragraphFormat.FrameWidth != 0f)
		{
			width = paragraphFormat.FrameWidth;
		}
		float height = bounds.Height;
		if (wSection != null && paragraphFormat != null && paragraphFormat.WrapFrameAround == FrameWrapMode.Around && !paragraph.IsInCell && wSection.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && paragraphFormat.FrameHorizontalPos == 2 && paragraphFormat.FrameVerticalPos == 1)
		{
			height = wSection.PageSetup.PageSize.Height;
		}
		Entity baseTextBody = GetBaseTextBody(paragraph);
		if (baseTextBody is HeaderFooter)
		{
			height = (baseTextBody.Owner as WSection).PageSetup.PageSize.Height;
		}
		float num = 0f;
		bool flag = false;
		if (paragraphFormat.FrameHeight != 0f)
		{
			ushort num2 = (ushort)(paragraphFormat.FrameHeight * 20f);
			flag = (num2 & 0x8000) != 0;
			num = (num2 & 0x7FFF) / 20;
		}
		if (!flag && paragraphFormat.FrameHeight != 0f)
		{
			height = num;
		}
		if (wSection != null)
		{
			x = ((paragraph.ParagraphFormat.FrameWidth != 0f || (!paragraph.ParagraphFormat.IsNextParagraphInSameFrame() && !paragraph.ParagraphFormat.IsPreviousParagraphInSameFrame())) ? GetPositionX(paragraphFormat, wSection, bounds, paragraphFormat.FrameWidth) : GetPositionX(paragraphFormat, wSection, bounds, bounds.Width));
			y = GetPositionY(paragraphFormat, wSection, bounds, num, flag);
		}
		bounds = new RectangleF(x, y, width, height);
		if (paragraphFormat.IsPreviousParagraphInSameFrame())
		{
			return (m_lcOperator as Layouter).FrameLayoutArea;
		}
		if (IsFrameLayouted(paragraph, out var j) && paragraphFormat.FrameVerticalPos == 2)
		{
			bounds.Y = (m_lcOperator as Layouter).FloatingItems[j].TextWrappingBounds.Y;
			if (!paragraph.ParagraphFormat.IsNextParagraphInSameFrame() || !paragraph.ParagraphFormat.IsPreviousParagraphInSameFrame())
			{
				bounds.Y += paragraph.ParagraphFormat.FrameVerticalDistanceFromText + ((LayoutInfo is ILayoutSpacingsInfo) ? (LayoutInfo as ILayoutSpacingsInfo).Margins.Top : 0f) + ((((IWidget)paragraph).LayoutInfo as ParagraphLayoutInfo).SkipTopBorder ? 0f : (paragraph.ParagraphFormat.Borders.Top.GetLineWidthValue() + paragraph.ParagraphFormat.Borders.Top.Space));
			}
		}
		(m_lcOperator as Layouter).FrameLayoutArea = bounds;
		if (flag)
		{
			(m_lcOperator as Layouter).FrameHeight = num;
			(m_lcOperator as Layouter).IsSkipBottomForFrame = false;
		}
		return bounds;
	}

	private bool IsFrameLayouted(WParagraph paragraph, out int j)
	{
		j = 0;
		List<FloatingItem> floatingItems = (m_lcOperator as Layouter).FloatingItems;
		for (int i = 0; i < floatingItems.Count; i++)
		{
			if (floatingItems[i].FloatingEntity == paragraph)
			{
				j = i;
				return true;
			}
		}
		return false;
	}

	private Entity GetBaseTextBody(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			if (entity2.Owner == null)
			{
				return entity2;
			}
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WSection) && !(entity2 is HeaderFooter));
		return entity2;
	}

	internal Entity GetBaseEntity(Entity entity)
	{
		if (entity == null)
		{
			return null;
		}
		Entity entity2 = entity;
		while (!(entity2 is WSection) && !(entity2 is WTextBox) && !(entity2 is Shape) && !(entity2 is GroupShape) && entity2.Owner != null)
		{
			entity2 = entity2.Owner;
		}
		return entity2;
	}

	internal bool IsBaseFromSection(Entity entity)
	{
		Entity entity2 = entity;
		while (!(entity2 is WSection) && !(entity2 is WTextBox) && !(entity2 is Shape) && !(entity2 is GroupShape) && !(entity2 is WTableCell) && entity2.Owner != null)
		{
			entity2 = entity2.Owner;
		}
		if (entity2 is WSection)
		{
			return true;
		}
		return false;
	}

	internal bool IsInTable(Entity entity)
	{
		Entity entity2 = entity;
		if (entity2.Owner == null)
		{
			return false;
		}
		entity2 = entity2.Owner;
		while (!(entity2 is WTable) && !(entity2 is WParagraph))
		{
			if (entity2.Owner == null)
			{
				return false;
			}
			entity2 = entity2.Owner;
		}
		if (entity2 is WParagraph)
		{
			return (entity2 as WParagraph).IsInCell;
		}
		return true;
	}

	private float GetPositionX(WParagraphFormat paraFormat, WSection section, RectangleF bounds, float frameWidth)
	{
		float result = 0f;
		if (paraFormat.IsFrameXAlign(paraFormat.FrameX))
		{
			switch ((short)paraFormat.FrameX)
			{
			case -12:
			case 0:
				switch (paraFormat.FrameHorizontalPos)
				{
				case 0:
					result = (m_lcOperator as Layouter).ClientLayoutArea.X;
					break;
				case 1:
					result = Layouter.GetLeftMargin(section);
					break;
				case 2:
					result = 0f;
					break;
				}
				break;
			case -4:
				switch (paraFormat.FrameHorizontalPos)
				{
				case 0:
				{
					RectangleF clientLayoutArea = (m_lcOperator as Layouter).ClientLayoutArea;
					result = ((!(frameWidth < clientLayoutArea.Width)) ? (clientLayoutArea.Left - (frameWidth - clientLayoutArea.Width) / 2f) : ((m_lcOperator as Layouter).ClientLayoutArea.Left + (clientLayoutArea.Width - frameWidth) / 2f));
					break;
				}
				case 1:
					result = Layouter.GetLeftMargin(section) + (section.PageSetup.PageSize.Width - Layouter.GetRightMargin(section) - frameWidth - Layouter.GetLeftMargin(section)) / 2f;
					break;
				case 2:
					result = (section.PageSetup.PageSize.Width - frameWidth) / 2f;
					break;
				}
				break;
			case -16:
			case -8:
				switch (paraFormat.FrameHorizontalPos)
				{
				case 0:
					result = (m_lcOperator as Layouter).ClientLayoutArea.Width + (m_lcOperator as Layouter).ClientLayoutArea.Left - frameWidth;
					break;
				case 1:
					result = section.PageSetup.PageSize.Width - Layouter.GetRightMargin(section) - frameWidth;
					break;
				case 2:
					result = section.PageSetup.PageSize.Width - frameWidth;
					break;
				}
				break;
			default:
				switch (paraFormat.FrameHorizontalPos)
				{
				case 0:
					result = (m_lcOperator as Layouter).ClientLayoutArea.X + paraFormat.FrameX;
					break;
				case 1:
					result = Layouter.GetLeftMargin(section) + paraFormat.FrameX;
					break;
				case 2:
					result = paraFormat.FrameX;
					break;
				}
				break;
			}
		}
		else
		{
			switch (paraFormat.FrameHorizontalPos)
			{
			case 0:
				result = (m_lcOperator as Layouter).ClientLayoutArea.X + paraFormat.FrameX;
				break;
			case 1:
				result = Layouter.GetLeftMargin(section) + paraFormat.FrameX;
				break;
			case 2:
				result = paraFormat.FrameX;
				break;
			}
		}
		return result;
	}

	private float GetPositionY(WParagraphFormat paraFormat, WSection section, RectangleF bounds, float frameHeight, bool IsAtleastHeight)
	{
		float result = 0f;
		float num = section.PageSetup.Margins.Top + (section.Document.DOP.GutterAtTop ? section.PageSetup.Margins.Gutter : 0f);
		float bottom = section.PageSetup.Margins.Bottom;
		float num2 = section.PageSetup.PageSize.Height - num - bottom;
		if ((int)(paraFormat.FrameY * 20f) == -3 && paraFormat.FrameVerticalAnchor == 2)
		{
			return bounds.Y;
		}
		if (paraFormat.IsFrameYAlign(paraFormat.FrameY))
		{
			switch ((short)paraFormat.FrameY)
			{
			case -16:
			case -4:
				switch (paraFormat.FrameVerticalAnchor)
				{
				case 0:
					result = num;
					break;
				case 1:
					result = 0f;
					break;
				}
				break;
			case -8:
				switch (paraFormat.FrameVerticalAnchor)
				{
				case 0:
					result = num + num2 / 2f;
					break;
				case 1:
					result = section.PageSetup.PageSize.Height / 2f;
					break;
				}
				break;
			case -20:
			case -12:
				switch (paraFormat.FrameVerticalAnchor)
				{
				case 0:
					result = num + num2;
					break;
				case 1:
					result = section.PageSetup.PageSize.Height;
					break;
				}
				break;
			default:
				switch (paraFormat.FrameVerticalAnchor)
				{
				case 0:
					result = num + paraFormat.FrameY;
					break;
				case 1:
					result = ((IsAtleastHeight || !(paraFormat.FrameY > section.PageSetup.PageSize.Height - frameHeight) || paraFormat.FrameHeight == 0f) ? paraFormat.FrameY : (section.PageSetup.PageSize.Height - frameHeight));
					break;
				case 2:
					result = bounds.Y + paraFormat.FrameY;
					break;
				}
				break;
			}
		}
		else
		{
			switch (paraFormat.FrameVerticalAnchor)
			{
			case 0:
				result = num + paraFormat.FrameY;
				break;
			case 1:
				result = ((IsAtleastHeight || !(paraFormat.FrameY > section.PageSetup.PageSize.Height - frameHeight) || paraFormat.FrameHeight == 0f) ? paraFormat.FrameY : (section.PageSetup.PageSize.Height - frameHeight));
				break;
			case 2:
				result = bounds.Y + paraFormat.FrameY;
				break;
			}
		}
		return result;
	}
}
