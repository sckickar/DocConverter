using System;
using System.Collections.Generic;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LayoutedWidget
{
	private RectangleF m_bounds = RectangleF.Empty;

	private IWidget m_widget;

	private LayoutedWidgetList m_ltWidgets = new LayoutedWidgetList();

	private string m_textTag;

	private float m_wordSpace;

	private HAlignment m_horizontalAlign;

	private float m_subWidth;

	private int m_spaces;

	private TabJustification m_prevTabJustification;

	private LayoutedWidget m_owner;

	private byte m_bFlags;

	internal float m_footnoteHeight;

	internal float m_endnoteHeight;

	internal List<RectangleF> m_intersectingBounds;

	private bool m_isTrackChanges;

	private CharacterRangeType m_charRangeType;

	internal TabJustification PrevTabJustification
	{
		get
		{
			return m_prevTabJustification;
		}
		set
		{
			m_prevTabJustification = value;
		}
	}

	internal bool IsTrackChanges
	{
		get
		{
			return m_isTrackChanges;
		}
		set
		{
			m_isTrackChanges = value;
		}
	}

	public int Spaces
	{
		get
		{
			return m_spaces;
		}
		set
		{
			m_spaces = value;
		}
	}

	public bool IsLastLine
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

	public float SubWidth
	{
		get
		{
			return m_subWidth;
		}
		set
		{
			m_subWidth = value;
		}
	}

	public HAlignment HorizontalAlign
	{
		get
		{
			return m_horizontalAlign;
		}
		set
		{
			m_horizontalAlign = value;
		}
	}

	public float WordSpace
	{
		get
		{
			return m_wordSpace;
		}
		set
		{
			if (IsNotWord2013())
			{
				m_wordSpace = Math.Abs(Convert.ToSingle(value));
			}
			else
			{
				m_wordSpace = value;
			}
		}
	}

	public string TextTag
	{
		get
		{
			return m_textTag;
		}
		set
		{
			m_textTag = value;
		}
	}

	public RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	internal IWidget Widget => m_widget;

	public LayoutedWidgetList ChildWidgets
	{
		get
		{
			return m_ltWidgets;
		}
		set
		{
			m_ltWidgets = value;
		}
	}

	internal CharacterRangeType CharacterRange
	{
		get
		{
			return m_charRangeType;
		}
		set
		{
			m_charRangeType = value;
		}
	}

	public LayoutedWidget Owner
	{
		get
		{
			return m_owner;
		}
		set
		{
			m_owner = value;
		}
	}

	internal bool IsLastItemInPage
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

	internal bool IsNotFitted
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

	internal bool IsContainsSpaceCharAtEnd
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

	internal List<RectangleF> IntersectingBounds
	{
		get
		{
			if (m_intersectingBounds == null)
			{
				m_intersectingBounds = new List<RectangleF>();
			}
			return m_intersectingBounds;
		}
	}

	public LayoutedWidget(IWidget widget)
	{
		m_widget = widget;
	}

	public LayoutedWidget()
	{
	}

	public LayoutedWidget(IWidget widget, PointF location)
	{
		m_widget = widget;
		m_bounds = new RectangleF(location, default(SizeF));
	}

	public LayoutedWidget(LayoutedWidget ltWidget)
	{
		Bounds = ltWidget.Bounds;
		for (int i = 0; i < ltWidget.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = ltWidget.ChildWidgets[i];
			if (layoutedWidget is LayoutedMathWidget)
			{
				ChildWidgets.Add(new LayoutedMathWidget(layoutedWidget));
			}
			else
			{
				ChildWidgets.Add(new LayoutedWidget(layoutedWidget));
			}
		}
		for (int j = 0; j < ltWidget.IntersectingBounds.Count; j++)
		{
			IntersectingBounds.Add(ltWidget.IntersectingBounds[j]);
		}
		HorizontalAlign = ltWidget.HorizontalAlign;
		IsLastItemInPage = ltWidget.IsLastItemInPage;
		IsLastLine = ltWidget.IsLastLine;
		IsNotFitted = ltWidget.IsNotFitted;
		Owner = ltWidget.Owner;
		PrevTabJustification = ltWidget.PrevTabJustification;
		Spaces = ltWidget.Spaces;
		SubWidth = ltWidget.SubWidth;
		TextTag = ltWidget.TextTag;
		m_widget = ltWidget.Widget;
		WordSpace = ltWidget.WordSpace;
		m_isTrackChanges = ltWidget.m_isTrackChanges;
		m_charRangeType = ltWidget.m_charRangeType;
	}

	internal void GetFootnoteHeight(ref float height)
	{
		foreach (LayoutedWidget childWidget in ChildWidgets)
		{
			if (childWidget.Widget is WTableRow)
			{
				GetFootnoteHeightForTableRow(ref height, childWidget);
			}
			else if (childWidget.Widget is WFootnote)
			{
				height += ((FootnoteLayoutInfo)childWidget.Widget.LayoutInfo).FootnoteHeight;
			}
			else if (childWidget.Widget is IWidgetContainer)
			{
				childWidget.GetFootnoteHeight(ref height);
			}
		}
	}

	internal void GetFootnoteHeightForTableRow(ref float height, LayoutedWidget row)
	{
		foreach (LayoutedWidget childWidget in row.ChildWidgets)
		{
			LayoutedWidget childParagraphWidgets = GetChildParagraphWidgets(childWidget);
			if (childParagraphWidgets == null)
			{
				continue;
			}
			foreach (LayoutedWidget childWidget2 in childParagraphWidgets.ChildWidgets)
			{
				foreach (LayoutedWidget childWidget3 in childWidget2.ChildWidgets)
				{
					if (childWidget3.Widget is WFootnote)
					{
						height += ((FootnoteLayoutInfo)childWidget3.Widget.LayoutInfo).FootnoteHeight;
					}
					else if (childWidget3.Widget is IWidgetContainer)
					{
						childWidget3.GetFootnoteHeight(ref height);
					}
				}
			}
		}
	}

	private LayoutedWidget GetChildParagraphWidgets(LayoutedWidget layoutedWidget)
	{
		if (layoutedWidget.ChildWidgets != null && layoutedWidget.ChildWidgets.Count > 0)
		{
			if (!IsTableWidget(layoutedWidget.ChildWidgets[0].Widget))
			{
				return GetChildParagraphWidgets(layoutedWidget.ChildWidgets[0]);
			}
			if (layoutedWidget.ChildWidgets[0].Widget is WParagraph || (layoutedWidget.ChildWidgets[0].Widget is SplitWidgetContainer && (layoutedWidget.ChildWidgets[0].Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph))
			{
				return layoutedWidget;
			}
		}
		return null;
	}

	private bool IsTableWidget(IWidget widget)
	{
		if (widget is WTableCell || widget is WTableRow || widget is WTable || (widget is SplitWidgetContainer && (widget as SplitWidgetContainer).RealWidgetContainer is WTableCell) || (widget is SplitWidgetContainer && (widget as SplitWidgetContainer).RealWidgetContainer is WTableRow) || (widget is SplitWidgetContainer && (widget as SplitWidgetContainer).RealWidgetContainer is WTable))
		{
			return false;
		}
		return true;
	}

	internal RectangleF GetFrameClipBounds(RectangleF bounds, WParagraphFormat paragraphFormat, ParagraphLayoutInfo paraLayoutInfo, float maxTextHeight)
	{
		float frameHorizontalDistanceFromText = paragraphFormat.FrameHorizontalDistanceFromText;
		float num = paraLayoutInfo.XPosition;
		float num2 = ((paragraphFormat.FrameWidth != 0f) ? (paragraphFormat.FrameWidth - (paragraphFormat.LeftIndent + paragraphFormat.RightIndent)) : bounds.Width);
		if (paragraphFormat.FirstLineIndent < 0f && paraLayoutInfo.Margins.Left > 0f)
		{
			num += paragraphFormat.FirstLineIndent;
			num2 -= paragraphFormat.FirstLineIndent;
		}
		if (paraLayoutInfo.LevelNumber != -1 && paraLayoutInfo.Margins.Left > 0f)
		{
			num -= paraLayoutInfo.ListTab;
			num2 += paraLayoutInfo.ListTab;
		}
		float left = paraLayoutInfo.Paddings.Left;
		float right = paraLayoutInfo.Paddings.Right;
		float top = paraLayoutInfo.Paddings.Top;
		float bottom = paraLayoutInfo.Paddings.Bottom;
		float num3 = 0f;
		if (paragraphFormat.LineSpacingRule == LineSpacingRule.Multiple && paragraphFormat.LineSpacing < 12f && paragraphFormat.BeforeSpacing > 0f)
		{
			float lineSpacing = paragraphFormat.LineSpacing;
			if (maxTextHeight * (lineSpacing / 12f) < 12f && lineSpacing < maxTextHeight)
			{
				lineSpacing = Math.Abs(maxTextHeight * (lineSpacing / 12f) - maxTextHeight);
				num3 = ((paragraphFormat.BeforeSpacing < lineSpacing) ? paragraphFormat.BeforeSpacing : lineSpacing);
			}
		}
		Borders borders = paragraphFormat.Borders;
		float num4 = ((paraLayoutInfo.Margins.Left < 0f) ? 0f : paraLayoutInfo.Margins.Left);
		return new RectangleF(num + left + num4 - frameHorizontalDistanceFromText - 1.5f - borders.Left.LineWidth / 2f, bounds.Y - top - num3 - borders.Top.LineWidth / 2f, num2 + frameHorizontalDistanceFromText - (left + right) + borders.Left.LineWidth / 2f + borders.Right.LineWidth / 2f + 3f, bounds.Height + (top + bottom) + borders.Top.LineWidth / 2f + borders.Bottom.LineWidth / 2f);
	}

	internal bool IsBehindWidget()
	{
		if (Widget is WPicture || Widget is WTextBox || Widget is WChart || Widget is Shape || Widget is GroupShape)
		{
			return ((ParagraphItem)Widget).GetTextWrappingStyle() == TextWrappingStyle.Behind;
		}
		return false;
	}

	public void InitLayoutInfo(bool resetTabLayoutInfo)
	{
		if (m_widget == null)
		{
			return;
		}
		m_widget.InitLayoutInfo();
		int i = 0;
		for (int count = m_ltWidgets.Count; i < count; i++)
		{
			LayoutedWidget layoutedWidget = m_ltWidgets[i];
			if (layoutedWidget is LayoutedMathWidget)
			{
				layoutedWidget.Widget.InitLayoutInfo();
				(layoutedWidget as LayoutedMathWidget).Dispose();
			}
			else if (layoutedWidget != null && (resetTabLayoutInfo || !(layoutedWidget.Widget is WTextRange) || !(((WTextRange)layoutedWidget.Widget).m_layoutInfo is TabsLayoutInfo)))
			{
				layoutedWidget.InitLayoutInfo(resetTabLayoutInfo);
			}
		}
	}

	public void InitLayoutInfoForTextWrapElements()
	{
		if (m_widget is WPicture)
		{
			(m_widget as WPicture).m_layoutInfo = null;
		}
		else if (m_widget is WTextBox)
		{
			(m_widget as WTextBox).m_layoutInfo = null;
		}
		else if (m_widget is Shape)
		{
			(m_widget as Shape).m_layoutInfo = null;
		}
		else if (m_widget is GroupShape)
		{
			(m_widget as GroupShape).m_layoutInfo = null;
		}
		else if (m_widget is WChart)
		{
			(m_widget as WChart).m_layoutInfo = null;
		}
	}

	public void InitLayoutInfoAll()
	{
		if (m_widget == null)
		{
			return;
		}
		if (m_widget is SplitStringWidget)
		{
			(m_widget as SplitStringWidget).RealStringWidget.InitLayoutInfo();
		}
		else if (m_widget is SplitWidgetContainer)
		{
			(m_widget as SplitWidgetContainer).RealWidgetContainer.InitLayoutInfo();
		}
		else if (m_widget is SplitTableWidget)
		{
			(m_widget as SplitTableWidget).TableWidget.InitLayoutInfo();
		}
		else
		{
			m_widget.InitLayoutInfo();
		}
		if (m_widget != null)
		{
			int i = 0;
			for (int count = m_ltWidgets.Count; i < count; i++)
			{
				LayoutedWidget layoutedWidget = m_ltWidgets[i];
				WParagraph wParagraph = ((layoutedWidget.Widget is SplitWidgetContainer) ? ((layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph) : (layoutedWidget.Widget as WParagraph));
				if (wParagraph != null)
				{
					layoutedWidget.InitLayoutInfo(resetTabLayoutInfo: true);
					for (int j = 0; j < wParagraph.ChildEntities.Count; j++)
					{
						if (wParagraph.ChildEntities[j] is IWidget widget)
						{
							widget.InitLayoutInfo();
							if (widget is InlineContentControl)
							{
								InlineContentControl inlineContentControl = widget as InlineContentControl;
								for (int k = 0; k < inlineContentControl.ParagraphItems.Count; k++)
								{
									((IWidget)inlineContentControl.ParagraphItems[k]).InitLayoutInfo();
								}
							}
						}
						else if (wParagraph.ChildEntities.InnerList[j] is SplitStringWidget)
						{
							wParagraph.ChildEntities.InnerList.RemoveAt(j);
							j--;
						}
					}
				}
				else
				{
					layoutedWidget.InitLayoutInfoAll();
				}
			}
		}
		m_widget = null;
		m_owner = null;
		TextTag = null;
		m_ltWidgets.Clear();
		m_ltWidgets = null;
	}

	public void ShiftLocation(double xOffset, double yOffset, bool isPictureNeedToBeShifted, bool isFromFloatingItemVerticalAlignment)
	{
		ShiftLocation(xOffset, yOffset, isPictureNeedToBeShifted, isFromFloatingItemVerticalAlignment, isNeedToShiftOwnerWidget: true);
	}

	public void ShiftLocation(double xOffset, double yOffset, bool isPictureNeedToBeShifted, bool isFromFloatingItemVerticalAlignment, bool isNeedToShiftOwnerWidget)
	{
		if (isNeedToShiftOwnerWidget)
		{
			m_bounds = new RectangleF(new PointF((float)((double)m_bounds.X + xOffset), (float)((double)m_bounds.Y + yOffset)), m_bounds.Size);
			if (m_widget is WTextBox || m_widget is Shape || m_widget is ChildShape)
			{
				RectangleF rectangleF = ((m_widget is WTextBox) ? (m_widget as WTextBox).TextLayoutingBounds : ((m_widget is Shape) ? (m_widget as Shape).TextLayoutingBounds : (m_widget as ChildShape).TextLayoutingBounds));
				rectangleF = new RectangleF((float)((double)rectangleF.X + xOffset), (float)((double)rectangleF.Y + yOffset), rectangleF.Width, rectangleF.Height);
				if (m_widget is WTextBox)
				{
					WTextBox wTextBox = m_widget as WTextBox;
					wTextBox.TextLayoutingBounds = rectangleF;
					if (wTextBox.TextBoxFormat.VMLPathPoints != null && wTextBox.TextBoxFormat.VMLPathPoints.Count > 0)
					{
						wTextBox.ReUpdateVMLPathPoints((float)xOffset, (float)yOffset, wTextBox.TextBoxFormat.VMLPathPoints);
					}
				}
				else if (m_widget is Shape)
				{
					Shape shape = m_widget as Shape;
					shape.TextLayoutingBounds = rectangleF;
					if (shape.VMLPathPoints != null && shape.VMLPathPoints.Count > 0)
					{
						shape.ReUpdateVMLPathPoints((float)xOffset, (float)yOffset, shape.VMLPathPoints);
					}
				}
				else
				{
					ChildShape childShape = m_widget as ChildShape;
					childShape.TextLayoutingBounds = rectangleF;
					if (childShape.VMLPathPoints != null && childShape.VMLPathPoints.Count > 0)
					{
						childShape.ReUpdateVMLPathPoints((float)xOffset, (float)yOffset, childShape.VMLPathPoints);
					}
				}
			}
		}
		if (m_widget is WMath)
		{
			(this as LayoutedMathWidget).ShiftXYPosition((float)xOffset, (float)yOffset, isNeedToShiftOwnerWidget);
			return;
		}
		for (int i = 0; i < ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = ChildWidgets[i];
			if (layoutedWidget == null)
			{
				continue;
			}
			if (layoutedWidget.Widget is WTable || layoutedWidget.Widget is WTextBox)
			{
				if (layoutedWidget.Widget is WTable)
				{
					WTable wTable = layoutedWidget.Widget as WTable;
					if (wTable.TableFormat.WrapTextAround)
					{
						if (!IsShiftAbsTableBasedOnPageBottom(layoutedWidget, xOffset) && wTable.TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph && isPictureNeedToBeShifted)
						{
							layoutedWidget.ShiftLocation(0.0, yOffset, isPictureNeedToBeShifted, isFromFloatingItemVerticalAlignment);
						}
						continue;
					}
				}
				else
				{
					WTextBox wTextBox2 = layoutedWidget.Widget as WTextBox;
					double num = 0.0;
					if (wTextBox2.IsFloatingItem(isTextWrapAround: false))
					{
						if ((layoutedWidget.Widget as ParagraphItem).GetHorizontalOrigin() == HorizontalOrigin.Character)
						{
							num = xOffset;
						}
						if (!IsShiftAbsTableBasedOnPageBottom(layoutedWidget, xOffset) && ((wTextBox2.OwnerParagraph != null && wTextBox2.OwnerParagraph.IsInCell) || wTextBox2.TextBoxFormat.VerticalOrigin == VerticalOrigin.Line || wTextBox2.TextBoxFormat.VerticalOrigin == VerticalOrigin.Paragraph) && isPictureNeedToBeShifted)
						{
							layoutedWidget.ShiftLocation(num, yOffset, isPictureNeedToBeShifted, isFromFloatingItemVerticalAlignment);
						}
						else if (isFromFloatingItemVerticalAlignment && IsFloatingItemNeedToBeAlign(layoutedWidget.Widget))
						{
							layoutedWidget.ShiftLocation(0.0, yOffset, isPictureNeedToBeShifted, isFromFloatingItemVerticalAlignment);
						}
						else if (num > 0.0)
						{
							layoutedWidget.ShiftLocation(num, 0.0, isPictureNeedToBeShifted, isFromFloatingItemVerticalAlignment);
						}
						continue;
					}
				}
			}
			if (layoutedWidget.Widget is WPicture || layoutedWidget.Widget is Shape || layoutedWidget.Widget is WChart || layoutedWidget.Widget is GroupShape)
			{
				TextWrappingStyle textWrappingStyle = (layoutedWidget.Widget as ParagraphItem).GetTextWrappingStyle();
				VerticalOrigin verticalOrigin = (layoutedWidget.Widget as ParagraphItem).GetVerticalOrigin();
				WParagraph ownerParagraphValue = (layoutedWidget.Widget as ParagraphItem).GetOwnerParagraphValue();
				double num2 = 0.0;
				if (textWrappingStyle != 0)
				{
					if ((layoutedWidget.Widget as ParagraphItem).GetHorizontalOrigin() == HorizontalOrigin.Character)
					{
						num2 = xOffset;
					}
					if (((ownerParagraphValue != null && ownerParagraphValue.IsInCell) || verticalOrigin == VerticalOrigin.Paragraph || verticalOrigin == VerticalOrigin.Line) && isPictureNeedToBeShifted)
					{
						layoutedWidget.ShiftLocation(num2, yOffset, isPictureNeedToBeShifted, isFromFloatingItemVerticalAlignment);
					}
					else if (isFromFloatingItemVerticalAlignment && IsFloatingItemNeedToBeAlign(layoutedWidget.Widget))
					{
						layoutedWidget.ShiftLocation(0.0, yOffset, isPictureNeedToBeShifted, isFromFloatingItemVerticalAlignment);
					}
					else if (num2 > 0.0)
					{
						layoutedWidget.ShiftLocation(num2, 0.0, isPictureNeedToBeShifted, isFromFloatingItemVerticalAlignment);
					}
					continue;
				}
			}
			ParagraphLayoutInfo paragraphLayoutInfo = layoutedWidget.Widget.LayoutInfo as ParagraphLayoutInfo;
			if (layoutedWidget.Widget is WParagraph && layoutedWidget.ChildWidgets.Count > 0 && layoutedWidget.ChildWidgets[0].Widget == layoutedWidget.Widget && paragraphLayoutInfo != null && paragraphLayoutInfo.ListValue != string.Empty && paragraphLayoutInfo.ListYPositions.Count >= 1)
			{
				paragraphLayoutInfo.ListYPositions[paragraphLayoutInfo.ListYPositions.Count - 1] += (float)yOffset;
			}
			layoutedWidget.ShiftLocation(xOffset, yOffset, isPictureNeedToBeShifted, isFromFloatingItemVerticalAlignment);
		}
	}

	private bool IsFloatingItemNeedToBeAlign(IWidget widget)
	{
		bool layoutInCell = false;
		if (widget is WPicture)
		{
			layoutInCell = (widget as WPicture).LayoutInCell;
		}
		else if (widget is Shape)
		{
			layoutInCell = (widget as Shape).LayoutInCell;
		}
		else if (widget is WTextBox)
		{
			layoutInCell = (widget as WTextBox).TextBoxFormat.AllowInCell;
		}
		else if (widget is WChart)
		{
			layoutInCell = (widget as WChart).LayoutInCell;
		}
		else if (widget is GroupShape)
		{
			layoutInCell = (widget as GroupShape).LayoutInCell;
		}
		return CompatibilityCheck(((Entity)widget).Document.Settings.CompatibilityMode, ((Entity)widget).Document.DOP.Dop2000.Copts.DontVertAlignCellWithSp, layoutInCell);
	}

	private bool CompatibilityCheck(CompatibilityMode compatibilityMode, bool dontVertAlignCellWithSp, bool layoutInCell)
	{
		if (compatibilityMode != CompatibilityMode.Word2013)
		{
			if (compatibilityMode != 0 || dontVertAlignCellWithSp)
			{
				return compatibilityMode != CompatibilityMode.Word2003 && layoutInCell;
			}
			return true;
		}
		return true;
	}

	public void ShiftLocation(double xOffset, double yOffset, float footerHeight, float pageHeight, bool isHeader, DocumentLayouter layouter = null)
	{
		ShiftLocation(xOffset, yOffset, footerHeight, (float)yOffset, pageHeight, isHeader, layouter);
	}

	public void ShiftLocation(double xOffset, double yOffset, float footerHeight, float originalDistance, float pageHeight, bool isHeader, DocumentLayouter layouter)
	{
		if (!isHeader)
		{
			m_bounds = new RectangleF(new PointF((float)((double)m_bounds.X + xOffset), (float)((double)m_bounds.Y + yOffset)), m_bounds.Size);
		}
		foreach (LayoutedWidget childWidget in ChildWidgets)
		{
			if (childWidget == null)
			{
				continue;
			}
			if ((childWidget.Widget is WTable || childWidget.Widget is WTextBox) && !isHeader)
			{
				if (childWidget.Widget is WTable)
				{
					WTable wTable = childWidget.Widget as WTable;
					if (wTable.TableFormat.WrapTextAround && !wTable.IsInCell)
					{
						if (!IsShiftAbsTableBasedOnPageBottom(childWidget, xOffset, yOffset, footerHeight) && wTable.TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph)
						{
							childWidget.ShiftLocation(0.0, yOffset, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
						}
						continue;
					}
				}
				else
				{
					WTextBox wTextBox = childWidget.Widget as WTextBox;
					if (wTextBox.IsFloatingItem(isTextWrapAround: false))
					{
						if (!IsShiftAbsTableBasedOnPageBottom(childWidget, xOffset) && (wTextBox.TextBoxFormat.VerticalOrigin == VerticalOrigin.Line || wTextBox.TextBoxFormat.VerticalOrigin == VerticalOrigin.Paragraph))
						{
							childWidget.ShiftLocation(0.0, yOffset, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
							if (layouter != null)
							{
								UpdateTextWrappingBounds(childWidget, layouter);
							}
						}
						else if (wTextBox.TextBoxFormat.VerticalOrigin == VerticalOrigin.BottomMargin)
						{
							switch (wTextBox.TextBoxFormat.VerticalAlignment)
							{
							case ShapeVerticalAlignment.Top:
							case ShapeVerticalAlignment.Inside:
								childWidget.Bounds = new RectangleF(childWidget.Bounds.X, pageHeight - footerHeight, childWidget.Bounds.Width, childWidget.Bounds.Height);
								break;
							case ShapeVerticalAlignment.Bottom:
							case ShapeVerticalAlignment.Outside:
								childWidget.Bounds = new RectangleF(childWidget.Bounds.X, pageHeight - childWidget.Bounds.Height, childWidget.Bounds.Width, childWidget.Bounds.Height);
								break;
							case ShapeVerticalAlignment.Center:
								childWidget.Bounds = new RectangleF(childWidget.Bounds.X, pageHeight - footerHeight + (footerHeight - childWidget.Bounds.Height) / 2f, childWidget.Bounds.Width, childWidget.Bounds.Height);
								break;
							}
						}
						continue;
					}
				}
			}
			if (childWidget.Widget is WPicture || childWidget.Widget is Shape || childWidget.Widget is WChart || childWidget.Widget is GroupShape)
			{
				CompatibilityMode compatibilityMode = ((ParagraphItem)childWidget.Widget).Document.Settings.CompatibilityMode;
				TextWrappingStyle textWrappingStyle = ((ParagraphItem)childWidget.Widget).GetTextWrappingStyle();
				float verticalPosition = ((ParagraphItem)childWidget.Widget).GetVerticalPosition();
				VerticalOrigin verticalOrigin = ((ParagraphItem)childWidget.Widget).GetVerticalOrigin();
				ShapeVerticalAlignment shapeVerticalAlignment = ((ParagraphItem)childWidget.Widget).GetShapeVerticalAlignment();
				if (textWrappingStyle != 0)
				{
					bool flag = ((childWidget.Widget is Entity && (childWidget.Widget as Entity).GetOwnerTable(childWidget.Widget as Entity) != null) ? true : false);
					if (!isHeader && (verticalOrigin == VerticalOrigin.Paragraph || verticalOrigin == VerticalOrigin.Line))
					{
						childWidget.ShiftLocation(0.0, yOffset, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
						if (layouter != null)
						{
							UpdateTextWrappingBounds(childWidget, layouter);
						}
					}
					else if (!isHeader && verticalOrigin == VerticalOrigin.BottomMargin)
					{
						switch (shapeVerticalAlignment)
						{
						case ShapeVerticalAlignment.Top:
						case ShapeVerticalAlignment.Inside:
							childWidget.Bounds = new RectangleF(childWidget.Bounds.X, pageHeight - footerHeight, childWidget.Bounds.Width, childWidget.Bounds.Height);
							break;
						case ShapeVerticalAlignment.Bottom:
						case ShapeVerticalAlignment.Outside:
							childWidget.Bounds = new RectangleF(childWidget.Bounds.X, pageHeight - childWidget.Bounds.Height, childWidget.Bounds.Width, childWidget.Bounds.Height);
							break;
						case ShapeVerticalAlignment.Center:
							childWidget.Bounds = new RectangleF(childWidget.Bounds.X, pageHeight - footerHeight + (footerHeight - childWidget.Bounds.Height) / 2f, childWidget.Bounds.Width, childWidget.Bounds.Height);
							break;
						case ShapeVerticalAlignment.None:
							childWidget.Bounds = new RectangleF(childWidget.Bounds.X, pageHeight - footerHeight + verticalPosition, childWidget.Bounds.Width, childWidget.Bounds.Height);
							break;
						}
					}
					else if (isHeader && !flag && shapeVerticalAlignment == ShapeVerticalAlignment.None && (textWrappingStyle == TextWrappingStyle.InFrontOfText || textWrappingStyle == TextWrappingStyle.Behind) && verticalOrigin == VerticalOrigin.Margin)
					{
						childWidget.Bounds = new RectangleF(childWidget.Bounds.X, footerHeight + verticalPosition, childWidget.Bounds.Width, childWidget.Bounds.Height);
					}
					if (!isHeader && (textWrappingStyle == TextWrappingStyle.Square || textWrappingStyle == TextWrappingStyle.Through || textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.TopAndBottom) && verticalOrigin == VerticalOrigin.Paragraph && compatibilityMode != CompatibilityMode.Word2013)
					{
						float num = pageHeight;
						if (compatibilityMode == CompatibilityMode.Word2003)
						{
							num += originalDistance;
						}
						if (num < childWidget.Bounds.Bottom)
						{
							childWidget.Bounds = new RectangleF(childWidget.Bounds.X, childWidget.Bounds.Y - (childWidget.Bounds.Bottom - num), childWidget.Bounds.Width, childWidget.Bounds.Height);
						}
					}
					continue;
				}
			}
			ParagraphLayoutInfo paragraphLayoutInfo = childWidget.Widget.LayoutInfo as ParagraphLayoutInfo;
			if (!isHeader && childWidget.Widget is WParagraph)
			{
				WParagraph wParagraph = childWidget.Widget as WParagraph;
				WParagraphFormat paragraphFormat = wParagraph.ParagraphFormat;
				if (paragraphFormat.IsFrame && !(wParagraph.OwnerTextBody.Owner is WTextBox) && !(wParagraph.OwnerTextBody.Owner is Shape) && !wParagraph.IsInCell && paragraphFormat.FrameVerticalAnchor != 2)
				{
					continue;
				}
				if (childWidget.ChildWidgets.Count > 0 && childWidget.ChildWidgets[0].Widget == childWidget.Widget && paragraphLayoutInfo != null && paragraphLayoutInfo.ListValue != string.Empty && paragraphLayoutInfo.ListYPositions.Count >= 1)
				{
					paragraphLayoutInfo.ListYPositions[paragraphLayoutInfo.ListYPositions.Count - 1] += (float)yOffset;
				}
			}
			childWidget.ShiftLocation(xOffset, yOffset, footerHeight, originalDistance, pageHeight, isHeader, layouter);
		}
	}

	private void UpdateTextWrappingBounds(LayoutedWidget ltWidget, DocumentLayouter layouter)
	{
		for (int i = 0; i < layouter.FloatingItems.Count; i++)
		{
			FloatingItem floatingItem = layouter.FloatingItems[i];
			if (floatingItem.FloatingEntity == ltWidget.Widget)
			{
				float distanceFromLeft = 0f;
				float distanceFromRight = 0f;
				float distanceFromTop = 0f;
				float distanceFromBottom = 0f;
				GetDistanceValues(ltWidget.Widget, ref distanceFromLeft, ref distanceFromRight, ref distanceFromTop, ref distanceFromBottom);
				floatingItem.TextWrappingBounds = new RectangleF(ltWidget.Bounds.X - distanceFromLeft, ltWidget.Bounds.Y - distanceFromTop, ltWidget.Bounds.Width + distanceFromRight + distanceFromLeft, ltWidget.Bounds.Height + distanceFromBottom + distanceFromTop);
				break;
			}
		}
	}

	private void GetDistanceValues(IWidget leafWidget, ref float distanceFromLeft, ref float distanceFromRight, ref float distanceFromTop, ref float distanceFromBottom)
	{
		if (leafWidget is WTextBox)
		{
			WTextBoxFormat textBoxFormat = (leafWidget as WTextBox).TextBoxFormat;
			distanceFromLeft = textBoxFormat.WrapDistanceLeft;
			distanceFromRight = textBoxFormat.WrapDistanceRight;
			distanceFromTop = textBoxFormat.WrapDistanceTop;
			distanceFromBottom = textBoxFormat.WrapDistanceBottom;
		}
		else if (leafWidget is WPicture)
		{
			WPicture wPicture = leafWidget as WPicture;
			distanceFromLeft = wPicture.DistanceFromLeft;
			distanceFromRight = wPicture.DistanceFromRight;
			distanceFromTop = wPicture.DistanceFromTop;
			distanceFromBottom = wPicture.DistanceFromBottom;
		}
		else if (leafWidget is Shape)
		{
			Shape shape = leafWidget as Shape;
			distanceFromLeft = shape.WrapFormat.DistanceLeft;
			distanceFromRight = shape.WrapFormat.DistanceRight;
			distanceFromTop = shape.WrapFormat.DistanceTop;
			distanceFromBottom = shape.WrapFormat.DistanceBottom;
		}
		else if (leafWidget is GroupShape)
		{
			GroupShape groupShape = leafWidget as GroupShape;
			distanceFromLeft = groupShape.WrapFormat.DistanceLeft;
			distanceFromRight = groupShape.WrapFormat.DistanceRight;
			distanceFromTop = groupShape.WrapFormat.DistanceTop;
			distanceFromBottom = groupShape.WrapFormat.DistanceBottom;
		}
		else if (leafWidget is WChart)
		{
			WChart wChart = leafWidget as WChart;
			distanceFromLeft = wChart.WrapFormat.DistanceLeft;
			distanceFromRight = wChart.WrapFormat.DistanceRight;
			distanceFromTop = wChart.WrapFormat.DistanceTop;
			distanceFromBottom = wChart.WrapFormat.DistanceBottom;
		}
	}

	private bool IsShiftAbsTableBasedOnPageBottom(LayoutedWidget ltWidget, double xOffset)
	{
		bool flag = false;
		Entity baseEntity = GetBaseEntity(ltWidget.Widget as Entity);
		if (baseEntity.Owner != null && baseEntity is HeaderFooter && ((baseEntity as HeaderFooter).Type == HeaderFooterType.OddFooter || (baseEntity as HeaderFooter).Type == HeaderFooterType.FirstPageFooter || (baseEntity as HeaderFooter).Type == HeaderFooterType.EvenFooter))
		{
			float height = (baseEntity.Owner as WSection).PageSetup.PageSize.Height;
			if (height <= ltWidget.Bounds.Bottom && baseEntity.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && ((ltWidget.Widget is WTable && (ltWidget.Widget as WTable).TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph) || (ltWidget.Widget is WTextBox && ((ltWidget.Widget as WTextBox).TextBoxFormat.VerticalOrigin == VerticalOrigin.Line || (ltWidget.Widget as WTextBox).TextBoxFormat.VerticalOrigin == VerticalOrigin.Paragraph))))
			{
				flag = true;
				float num = ltWidget.Bounds.Y - height;
				num += ltWidget.Bounds.Height;
				ltWidget.ShiftLocation(xOffset, 0f - num, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
			}
			if (baseEntity.Document.Settings.CompatibilityMode != CompatibilityMode.Word2003 && flag)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsShiftAbsTableBasedOnPageBottom(LayoutedWidget ltWidget, double xOffset, double yOffset, float footerHeight)
	{
		bool flag = false;
		Entity baseEntity = GetBaseEntity(ltWidget.Widget as Entity);
		if (baseEntity.Owner != null && baseEntity is HeaderFooter && ((baseEntity as HeaderFooter).Type == HeaderFooterType.OddFooter || (baseEntity as HeaderFooter).Type == HeaderFooterType.FirstPageFooter || (baseEntity as HeaderFooter).Type == HeaderFooterType.EvenFooter))
		{
			float height = (baseEntity.Owner as WSection).PageSetup.PageSize.Height;
			if (height <= ltWidget.Bounds.Bottom && baseEntity.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && ((baseEntity.Document.Settings.CompatibilityMode != CompatibilityMode.Word2010 && baseEntity.Document.Settings.CompatibilityMode != CompatibilityMode.Word2007) || !((double)ltWidget.Bounds.Y + yOffset + (double)ltWidget.Bounds.Height < (double)height)) && ((ltWidget.Widget is WTable && (ltWidget.Widget as WTable).TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph) || (ltWidget.Widget is WTextBox && ((ltWidget.Widget as WTextBox).TextBoxFormat.VerticalOrigin == VerticalOrigin.Line || (ltWidget.Widget as WTextBox).TextBoxFormat.VerticalOrigin == VerticalOrigin.Paragraph))))
			{
				flag = true;
				float num = ltWidget.Bounds.Y - height;
				num += ltWidget.Bounds.Height;
				ltWidget.ShiftLocation(xOffset, 0f - num, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
			}
			if (baseEntity.Document.Settings.CompatibilityMode != CompatibilityMode.Word2003 && flag)
			{
				return true;
			}
		}
		return false;
	}

	internal void ShiftLocationOfCommentsMarkups(float xOffset, float yOffset, List<TrackChangesMarkups> trackChangesMarkups)
	{
		foreach (LayoutedWidget childWidget in ChildWidgets)
		{
			if (childWidget == null)
			{
				continue;
			}
			if (childWidget.Widget is WCommentMark && ((childWidget.Widget as WCommentMark).Type == CommentMarkType.CommentEnd || ((childWidget.Widget as WCommentMark).Type == CommentMarkType.CommentStart && (childWidget.Widget as WCommentMark).Comment != null && (childWidget.Widget as WCommentMark).Comment.CommentRangeEnd == null)))
			{
				for (int num = trackChangesMarkups.Count - 1; num >= 0; num--)
				{
					if (trackChangesMarkups[num] is CommentsMarkups commentsMarkups && commentsMarkups.CommentID == (childWidget.Widget as WCommentMark).CommentId)
					{
						commentsMarkups.Position = new PointF(commentsMarkups.Position.X + xOffset, commentsMarkups.Position.Y + yOffset);
						break;
					}
				}
			}
			childWidget.ShiftLocationOfCommentsMarkups(xOffset, yOffset, trackChangesMarkups);
		}
	}

	private float GetExtentWidth(Entity entity)
	{
		float num = 0f;
		ChildShapeCollection obj = ((entity is GroupShape) ? (entity as GroupShape).ChildShapes : (entity as ChildGroupShape).ChildShapes);
		float num2 = ((entity is GroupShape) ? (entity as GroupShape).CoordinateXOrigin : (entity as ChildGroupShape).CoordinateXOrigin);
		foreach (ChildShape item in obj)
		{
			if (item != null)
			{
				ChildShape childShape2 = item;
				float num3 = childShape2.LeftMargin - num2 + childShape2.Width;
				if (num < num3)
				{
					num = num3;
				}
			}
		}
		return num;
	}

	private float GetExtentHeight(Entity entity)
	{
		float num = 0f;
		ChildShapeCollection obj = ((entity is GroupShape) ? (entity as GroupShape).ChildShapes : (entity as ChildGroupShape).ChildShapes);
		float num2 = ((entity is GroupShape) ? (entity as GroupShape).CoordinateYOrigin : (entity as ChildGroupShape).CoordinateYOrigin);
		foreach (ChildShape item in obj)
		{
			if (item != null)
			{
				ChildShape childShape2 = item;
				float num3 = childShape2.TopMargin - num2 + childShape2.Height;
				if (num < num3)
				{
					num = num3;
				}
			}
		}
		return num;
	}

	internal void GetGroupShapeExtent(ref float extensionWidth, ref float extensionHeight, Entity entity, RectangleF groupShapeBounds)
	{
		bool num = ((entity is GroupShape) ? (entity as GroupShape).Is2007Shape : (entity as ChildGroupShape).Is2007Shape);
		float num2 = ((entity is GroupShape) ? (entity as GroupShape).Width : groupShapeBounds.Width);
		float num3 = ((entity is GroupShape) ? (entity as GroupShape).Height : groupShapeBounds.Height);
		float num4 = (num ? GetExtentWidth(entity) : ((entity is GroupShape) ? (entity as GroupShape).ExtentXValue : (entity as ChildGroupShape).ExtentXValue));
		float num5 = (num ? GetExtentHeight(entity) : ((entity is GroupShape) ? (entity as GroupShape).ExtentYValue : (entity as ChildGroupShape).ExtentYValue));
		extensionWidth = (float)Math.Round(((num2 > 0f) ? num2 : 1f) / num4, 4);
		extensionHeight = (float)Math.Round(((num3 > 0f) ? num3 : 1f) / num5, 4);
		if (extensionWidth == 0f || float.IsNaN(extensionWidth) || float.IsInfinity(extensionWidth))
		{
			extensionWidth = 1f;
		}
		if (extensionHeight == 0f || float.IsNaN(extensionHeight) || float.IsInfinity(extensionHeight))
		{
			extensionHeight = 1f;
		}
	}

	private Entity GetBaseEntity(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			if (entity2.Owner == null || entity2 is WTextBox)
			{
				return entity2;
			}
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WSection) && !(entity2 is HeaderFooter));
		return entity2;
	}

	public void UpdateLtWidgetBounds(float width, float height, float totalWidth, float totalHeight)
	{
		if (m_bounds.Width > width && totalHeight == 0f)
		{
			m_bounds = new RectangleF(m_bounds.X, m_bounds.Y, width, m_bounds.Height);
		}
		else if (m_bounds.Height > totalHeight && totalWidth == 0f)
		{
			m_bounds = new RectangleF(m_bounds.X, m_bounds.Y, m_bounds.Width, height);
		}
		for (int i = 0; i < ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = ChildWidgets[i];
			float num = 0f;
			int num2 = i - 1;
			while (num2 >= 0 && i != 0)
			{
				if ((ChildWidgets[num2] == null || IsLtWidgetBoundsNeedToUpdate(ChildWidgets[num2])) && ChildWidgets[num2] != null && ChildWidgets[num2].Widget is ParagraphItem)
				{
					num += ChildWidgets[num2].Bounds.Width;
				}
				num2--;
			}
			if (layoutedWidget == null || !IsLtWidgetBoundsNeedToUpdate(layoutedWidget))
			{
				continue;
			}
			Entity entity = layoutedWidget.Widget as Entity;
			while (!(entity is WTableCell) && entity != null && !(entity is WTable))
			{
				entity = entity.Owner;
			}
			if (entity is WTableCell && !(layoutedWidget.Widget is WTableCell))
			{
				if (totalHeight == 0f)
				{
					width = totalWidth - (((CellLayoutInfo)((IWidget)(entity as WTableCell)).LayoutInfo).Margins.Left + ((CellLayoutInfo)((IWidget)(entity as WTableCell)).LayoutInfo).Paddings.Left) - num;
				}
				else
				{
					height = totalHeight - (((CellLayoutInfo)((IWidget)(entity as WTableCell)).LayoutInfo).Margins.Top + ((CellLayoutInfo)((IWidget)(entity as WTableCell)).LayoutInfo).Paddings.Top);
				}
			}
			if (width < 0f)
			{
				width = 0f;
			}
			layoutedWidget.UpdateLtWidgetBounds(width, height, totalWidth, totalHeight);
		}
	}

	internal RectangleF GetOwnerTextBodyBounds()
	{
		LayoutedWidget layoutedWidget = this;
		while (!(layoutedWidget.Widget is WTextBody))
		{
			layoutedWidget = layoutedWidget.Owner;
		}
		return layoutedWidget.Bounds;
	}

	private bool IsLtWidgetBoundsNeedToUpdate(LayoutedWidget ltWidget)
	{
		if ((ltWidget.Widget is WPicture || ltWidget.Widget is WChart || ltWidget.Widget is Shape || ltWidget.Widget is WTextBox || ltWidget.Widget is GroupShape) && (((ParagraphItem)ltWidget.Widget).GetTextWrappingStyle() == TextWrappingStyle.InFrontOfText || ((ParagraphItem)ltWidget.Widget).GetTextWrappingStyle() == TextWrappingStyle.Behind))
		{
			return false;
		}
		return true;
	}

	public void AlignBottom(DrawingContext dc, float remClientAreaHeight, float clientAreaBottom, bool isRowFitInSamePage, bool isLayoutingHeaderRow, bool isLayoutingHeaderFooter, bool isForceFitLayout)
	{
		WParagraph paragraph = GetParagraph();
		bool flag = false;
		bool flag2 = false;
		if (ChildWidgets.Count == 0 && (paragraph.m_layoutInfo.IsSkip || paragraph.IsNeedToSkip || (paragraph.SectionEndMark && paragraph.PreviousSibling != null)))
		{
			return;
		}
		if (ChildWidgets.Count != 0 && paragraph.IsEmptyParagraph() && paragraph.SectionEndMark && paragraph.PreviousSibling != null)
		{
			m_bounds = new RectangleF(m_bounds.X, m_bounds.Y, m_bounds.Width, 0f);
			return;
		}
		if (ChildWidgets.Count != 0)
		{
			flag = paragraph.IsFirstLine(ChildWidgets[0]);
			flag2 = paragraph.IsLastLine(ChildWidgets[ChildWidgets.Count - 1]);
		}
		double maxHeight;
		double maxAscent;
		double maxTextHeight;
		double maxTextAscent;
		double maxTextDescent;
		float maxY;
		double maxAscentOfLoweredPos;
		IStringWidget lastTextWidget;
		bool isClippedLine;
		bool isTextInLine;
		bool containsInlinePicture;
		bool isAllWordsContainLoweredPos;
		bool flag3 = CalculateMaxChildWidget(dc, paragraph, flag, flag2, out maxHeight, out maxAscent, out maxTextHeight, out maxTextAscent, out maxTextDescent, out maxY, out maxAscentOfLoweredPos, out lastTextWidget, out isClippedLine, out isTextInLine, out containsInlinePicture, out isAllWordsContainLoweredPos);
		bool flag4 = false;
		if (paragraph.Document.Settings.CompatibilityOptions[CompatibilityOption.SuppressTopSpacing] && isForceFitLayout && paragraph.Owner.Owner is WSection && (paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.AtLeast || paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly) && (Widget.LayoutInfo as ParagraphLayoutInfo).ListValue == string.Empty && !isLayoutingHeaderFooter)
		{
			flag4 = true;
		}
		if (maxAscent > maxHeight)
		{
			maxAscent = maxHeight;
		}
		if (maxTextAscent > maxAscent)
		{
			maxTextAscent = maxAscent;
		}
		if (maxY != float.MaxValue)
		{
			m_bounds.Y = maxY;
		}
		float num = (float)((maxHeight > maxTextHeight && isTextInLine && containsInlinePicture) ? maxTextDescent : 0.0);
		double num2 = 0.0;
		double num3 = 0.0;
		float lineSpacing = paragraph.ParagraphFormat.LineSpacing;
		if (maxHeight != 0.0 || maxTextHeight != 0.0)
		{
			switch (paragraph.ParagraphFormat.LineSpacingRule)
			{
			case LineSpacingRule.Exactly:
			{
				float num4 = Math.Abs(lineSpacing) - (float)maxTextHeight;
				num2 = num4 * 80f / 100f;
				num3 = num4 * 20f / 100f;
				maxHeight = Math.Abs(lineSpacing);
				break;
			}
			case LineSpacingRule.AtLeast:
				if (maxHeight < (double)lineSpacing && (!isClippedLine || Math.Round(maxHeight, 2) != Math.Round(remClientAreaHeight, 2)))
				{
					num2 = (double)lineSpacing - (maxHeight + (double)num);
					maxHeight = lineSpacing;
				}
				break;
			case LineSpacingRule.Multiple:
				if (lineSpacing != 12f)
				{
					if (lineSpacing < 12f)
					{
						num2 = maxTextHeight * (double)(lineSpacing / 12f) - maxTextHeight;
						maxHeight -= Math.Abs(num2);
					}
					else
					{
						num3 = maxTextHeight * (double)(lineSpacing / 12f) - maxTextHeight;
					}
				}
				break;
			}
		}
		float extraLineAscent = float.MinValue;
		bool flag5 = paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly && containsInlinePicture;
		if (!flag4)
		{
			if ((flag || (ChildWidgets.Count == 0 && IsNeedToUpdateListYPos())) && Widget.LayoutInfo is ParagraphLayoutInfo && (Widget.LayoutInfo as ParagraphLayoutInfo).ListValue != string.Empty)
			{
				if (!isLayoutingHeaderRow)
				{
					((ParagraphLayoutInfo)Widget.LayoutInfo).ListYPositions.Clear();
				}
				ShiftListYPosition(paragraph, dc, maxAscent, num2, maxHeight, ref extraLineAscent, ref maxAscentOfLoweredPos, ref isAllWordsContainLoweredPos);
			}
			if (!flag3)
			{
				if (flag5)
				{
					foreach (LayoutedWidget childWidget in ChildWidgets)
					{
						if (childWidget != null && !childWidget.Widget.LayoutInfo.IsSkipBottomAlign && (!(childWidget.Widget is WField) || (childWidget.Widget as WField).FieldType != FieldType.FieldHyperlink) && IsInlineFloatingItem(childWidget.Widget))
						{
							double yOffset = lineSpacing * 80f / 100f - childWidget.Bounds.Height;
							childWidget.ShiftLocation(0.0, yOffset, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
						}
					}
					maxAscent = maxTextAscent;
					num = 0f;
				}
				ShiftLineWidgetYPosition(dc, maxAscent, num2, maxHeight, ref extraLineAscent, flag5, ref maxAscentOfLoweredPos, ref isAllWordsContainLoweredPos);
			}
		}
		float rowHeight = 0f;
		if (extraLineAscent != float.MinValue)
		{
			rowHeight = extraLineAscent;
			ShiftLocation(0.0, extraLineAscent, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
		}
		if (paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Exactly || paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.AtLeast)
		{
			num3 = 0.0;
		}
		if (flag4)
		{
			m_bounds = new RectangleF(m_bounds.X, m_bounds.Y, m_bounds.Width, m_bounds.Height);
		}
		else
		{
			m_bounds = new RectangleF(m_bounds.X, m_bounds.Y - rowHeight, m_bounds.Width, (float)maxHeight + (float)num3 + num);
		}
		if (!isForceFitLayout && !paragraph.ParagraphFormat.IsInFrame() && paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && Math.Round(m_bounds.Bottom, 2) > Math.Round(clientAreaBottom, 2) && paragraph.OwnerTextBody.OwnerBase is WSection && paragraph.ChildEntities.Count == 1 && paragraph.ChildEntities[0] is Shape && (paragraph.ChildEntities[0] as Shape).IsHorizontalRule && paragraph.ParagraphFormat.LineSpacingRule == LineSpacingRule.Multiple && paragraph.ParagraphFormat.LineSpacing != 12f)
		{
			m_bounds.Height -= (float)num3;
		}
		if (DocumentLayouter.IsEndPage)
		{
			return;
		}
		if (paragraph.IsInCell && !isRowFitInSamePage && !paragraph.IsExactlyRowHeight(paragraph.GetOwnerEntity() as WTableCell, ref rowHeight))
		{
			if (flag2)
			{
				m_bounds.Height += (paragraph.m_layoutInfo as ParagraphLayoutInfo).Margins.Bottom;
			}
			if (!isForceFitLayout && remClientAreaHeight > 0f && Math.Round(m_bounds.Bottom, 2) > Math.Round(clientAreaBottom, 2) && !(paragraph.GetOwnerEntity() as WTableCell).m_layoutInfo.IsVerticalText)
			{
				IsNotFitted = true;
			}
		}
		else if (!isForceFitLayout && remClientAreaHeight > 0f && Math.Round(m_bounds.Bottom, 2) > Math.Round(clientAreaBottom, 2) && paragraph.OwnerTextBody.OwnerBase is WSection && !paragraph.ParagraphFormat.IsInFrame() && IsNeedtoMoveNextPage(paragraph))
		{
			IsNotFitted = true;
		}
	}

	private bool IsNeedToUpdateListYPos()
	{
		if (Widget is WParagraph wParagraph && wParagraph.ChildEntities.Count >= 1)
		{
			return true;
		}
		return false;
	}

	private bool IsLineContainsPicture()
	{
		bool result = false;
		foreach (LayoutedWidget childWidget in ChildWidgets)
		{
			if (childWidget.Widget is WPicture)
			{
				result = true;
			}
		}
		return result;
	}

	private bool IsNeedtoMoveNextPage(WParagraph paragraph)
	{
		if (paragraph.ChildEntities.Count >= 1)
		{
			for (int i = 0; i < paragraph.ChildEntities.Count; i++)
			{
				IWidget widget = ((paragraph.Items[i] != null) ? paragraph.Items[i] : null);
				if ((widget == null || !widget.LayoutInfo.IsSkip || widget is WChart) && (!(widget is Shape) || !(widget as Shape).IsHorizontalRule))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private void ShiftLineWidgetYPosition(DrawingContext dc, double maxAscent, double topLineSpace, double maxHeight, ref float extraLineAscent, bool isInlineDrawingObject, ref double maxAscentOfLoweredPos, ref bool isAllWordsContainLoweredPos)
	{
		foreach (LayoutedWidget childWidget in ChildWidgets)
		{
			if (childWidget == null || childWidget.Widget.LayoutInfo.IsSkipBottomAlign || (childWidget.Widget is WField && ((WField)childWidget.Widget).FieldType == FieldType.FieldHyperlink) || childWidget.Widget is BookmarkEnd)
			{
				continue;
			}
			IWidget widget = childWidget.Widget;
			float topEdgeExtent = 0f;
			float bottomEdgeExtent = 0f;
			if (widget is ParagraphItem)
			{
				(widget as ParagraphItem).GetEffectExtentValues(out var _, out var _, out topEdgeExtent, out bottomEdgeExtent);
			}
			if (!isInlineDrawingObject || !IsInlineFloatingItem(widget))
			{
				if (childWidget.Widget.LayoutInfo is FootnoteLayoutInfo footnoteLayoutInfo)
				{
					widget = footnoteLayoutInfo.TextRange;
				}
				IStringWidget stringWidget = widget as IStringWidget;
				if (stringWidget == null && childWidget.Widget is SplitStringWidget splitStringWidget)
				{
					stringWidget = splitStringWidget.RealStringWidget;
				}
				float exceededLineAscent = float.MinValue;
				double textAscent = 0.0;
				WSymbol wSymbol = childWidget.Widget as WSymbol;
				if (childWidget is LayoutedMathWidget)
				{
					textAscent = (((childWidget as LayoutedMathWidget).ChildWidgets.Count != 0) ? ((double)GetTextAscentValueForMath(childWidget as LayoutedMathWidget, dc)) : 0.0);
				}
				else if (stringWidget != null || wSymbol != null)
				{
					WField wField = childWidget.Widget as WField;
					textAscent = ((wSymbol != null) ? ((double)dc.GetAscent(wSymbol.GetFont(dc), FontScriptType.English)) : ((wField == null || wField.FieldType != FieldType.FieldExpression) ? stringWidget.GetTextAscent(dc, ref exceededLineAscent) : ((double)dc.GetAscentValueForEQField(wField))));
				}
				else if (childWidget.Widget is WCommentMark)
				{
					textAscent = maxAscent;
				}
				else if (!(childWidget.Widget is BookmarkEnd) && !(childWidget.Widget is BookmarkStart) && !(childWidget.Widget is WFieldMark) && (!(childWidget.Widget is Break) || ((Break)childWidget.Widget).BreakType == BreakType.LineBreak || ((Break)childWidget.Widget).BreakType == BreakType.TextWrappingBreak))
				{
					textAscent = childWidget.Bounds.Height + topEdgeExtent + bottomEdgeExtent;
				}
				WCharacterFormat charFormat = ((stringWidget is WTextRange) ? (stringWidget as WTextRange).CharacterFormat : wSymbol?.CharacterFormat);
				ShiftYPosition(dc, textAscent, maxAscent, topLineSpace, maxHeight, exceededLineAscent, ref extraLineAscent, charFormat, childWidget, SizeF.Empty, ref maxAscentOfLoweredPos, ref isAllWordsContainLoweredPos);
			}
		}
	}

	private float GetTextAscentValueForMath(LayoutedMathWidget layoutedMathWidget, DrawingContext dc)
	{
		float num = 0f;
		for (int i = 0; i < layoutedMathWidget.ChildWidgets.Count; i++)
		{
			float verticalCenterPoint = layoutedMathWidget.ChildWidgets[i].GetVerticalCenterPoint();
			if (num < verticalCenterPoint)
			{
				num = verticalCenterPoint;
			}
		}
		return num + dc.GetDescent(layoutedMathWidget.GetFont(), FontScriptType.English);
	}

	private void ShiftListYPosition(WParagraph paragraph, DrawingContext dc, double maxAscent, double topLineSpace, double maxHeight, ref float extraLineAscent, ref double maxAscentOfLoweredPos, ref bool isAllWordsContainLoweredPos)
	{
		if (Widget.LayoutInfo is ParagraphLayoutInfo paragraphLayoutInfo)
		{
			float exceededLineAscent = float.MinValue;
			float num = 0f;
			WListFormat listFormatValue = paragraph.GetListFormatValue();
			SizeF size;
			if (paragraphLayoutInfo.CurrentListType == ListType.Bulleted && paragraph.GetListLevel(listFormatValue).PicBullet != null)
			{
				size = dc.MeasurePictureBulletSize(paragraph.GetListLevel(listFormatValue).PicBullet, (Widget.LayoutInfo as ParagraphLayoutInfo).ListFont.GetFont(paragraph.Document, FontScriptType.English));
				num = size.Height;
			}
			else
			{
				num = dc.GetAscent(paragraphLayoutInfo.ListFont.GetFont(paragraphLayoutInfo.CharacterFormat.Document, FontScriptType.English), FontScriptType.English);
				size = dc.MeasureString(paragraphLayoutInfo.ListValue, paragraphLayoutInfo.ListFont.GetFont(paragraphLayoutInfo.CharacterFormat.Document, FontScriptType.English), null, paragraphLayoutInfo.CharacterFormat, isMeasureFromTabList: true, FontScriptType.English);
			}
			ShiftYPosition(dc, num, maxAscent, topLineSpace, maxHeight, exceededLineAscent, ref extraLineAscent, paragraphLayoutInfo.CharacterFormat, null, size, ref maxAscentOfLoweredPos, ref isAllWordsContainLoweredPos);
		}
	}

	private void ShiftYPosition(DrawingContext dc, double textAscent, double maxAscent, double topLineSpace, double maxHeight, float exceededLineAscent, ref float extraLineAscent, WCharacterFormat charFormat, LayoutedWidget ltWidget, SizeF size, ref double maxAscentOfLoweredPos, ref bool isAllWordsContainLoweredPos)
	{
		double num = 0.0;
		if (isAllWordsContainLoweredPos && charFormat != null && (double)Math.Abs(charFormat.Position) > maxAscentOfLoweredPos)
		{
			num = ((maxAscentOfLoweredPos == 0.0 && (double)Math.Abs(charFormat.Position) - textAscent > 0.0) ? ((double)Math.Abs(charFormat.Position) - textAscent) : (Math.Abs(maxAscentOfLoweredPos - (double)Math.Abs(charFormat.Position)) + topLineSpace));
		}
		else
		{
			num = ((maxAscent > textAscent + (double)((charFormat != null && charFormat.Position > 0f) ? charFormat.Position : 0f)) ? (maxAscent - (textAscent + (double)((charFormat != null && charFormat.Position > 0f) ? charFormat.Position : 0f))) : 0.0) + topLineSpace;
			num += (double)((charFormat != null && charFormat.Position < 0f) ? Math.Abs(charFormat.Position) : 0f);
		}
		float num2 = ltWidget?.Bounds.Height ?? size.Height;
		if (charFormat != null && charFormat.SubSuperScript != 0)
		{
			float num3 = num2 / 1.5f;
			if (charFormat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.SubScript)
			{
				num += (double)(num2 - num3);
			}
			if (ltWidget != null)
			{
				ltWidget.Bounds = new RectangleF(ltWidget.Bounds.X, ltWidget.Bounds.Y, ltWidget.Bounds.Width, num3);
			}
		}
		if ((Math.Round(maxHeight, 1) != Math.Round(num2, 1) || (textAscent != 0.0 && textAscent < maxAscent && (IsMaxHeightInLine(dc, num2) || (Math.Round(maxHeight, 1) == Math.Round(num2, 1) && (HasRaisedPosition() || IsContainsMathItemInLine())))) || (charFormat != null && charFormat.SubSuperScript == DocGen.DocIO.DLS.SubSuperScript.SubScript)) && num != 0.0)
		{
			if (ltWidget != null)
			{
				ltWidget.ShiftLocation(0.0, num, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
			}
			else
			{
				((ParagraphLayoutInfo)Widget.LayoutInfo).ListYPositions.Add(m_bounds.Y + (float)num);
			}
		}
		else if (ltWidget != null)
		{
			ltWidget.ShiftLocation(0.0, topLineSpace, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
		}
		else
		{
			((ParagraphLayoutInfo)Widget.LayoutInfo).ListYPositions.Add(m_bounds.Y + (float)topLineSpace);
		}
		if (exceededLineAscent != float.MinValue && extraLineAscent < exceededLineAscent)
		{
			extraLineAscent = exceededLineAscent;
		}
	}

	private bool HasRaisedPosition()
	{
		for (int i = 0; i < ChildWidgets.Count; i++)
		{
			if (ChildWidgets[i].Widget is WTextRange)
			{
				WTextRange wTextRange = ChildWidgets[i].Widget as WTextRange;
				if (wTextRange.CharacterFormat != null && wTextRange.CharacterFormat.Position > 0f)
				{
					return true;
				}
			}
			else if (ChildWidgets[i].Widget is WSymbol)
			{
				WSymbol wSymbol = ChildWidgets[i].Widget as WSymbol;
				if (wSymbol.CharacterFormat != null && wSymbol.CharacterFormat.Position > 0f)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsMaxHeightInLine(DrawingContext dc, float height)
	{
		int num = 0;
		WParagraph paragraph = GetParagraph();
		ParagraphLayoutInfo paragraphLayoutInfo = Widget.LayoutInfo as ParagraphLayoutInfo;
		if (paragraph != null && ChildWidgets.Count >= 1 && paragraph.IsFirstLine(ChildWidgets[0]) && paragraphLayoutInfo != null && paragraphLayoutInfo.ListValue != string.Empty && dc.MeasureString(paragraphLayoutInfo.ListValue, paragraphLayoutInfo.ListFont.GetFont(paragraph.Document, FontScriptType.English), null, paragraphLayoutInfo.CharacterFormat, isMeasureFromTabList: true, FontScriptType.English).Height >= height)
		{
			num++;
		}
		foreach (LayoutedWidget childWidget in ChildWidgets)
		{
			if (childWidget.Bounds.Height >= height)
			{
				num++;
			}
			if (num > 1)
			{
				break;
			}
		}
		return num == 1;
	}

	private bool IsContainsMathItemInLine()
	{
		foreach (LayoutedWidget childWidget in ChildWidgets)
		{
			if (childWidget is LayoutedMathWidget)
			{
				return true;
			}
		}
		return false;
	}

	internal WParagraph GetParagraph()
	{
		WParagraph result = null;
		if (Widget is WParagraph)
		{
			result = Widget as WParagraph;
		}
		else if (Widget is SplitWidgetContainer && (Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)
		{
			result = (Widget as SplitWidgetContainer).RealWidgetContainer as WParagraph;
		}
		return result;
	}

	internal bool CalculateMaxChildWidget(DrawingContext dc, WParagraph paragraph, bool isFirstLineOfParagraph, bool isLastLineOfParagraph, out double maxHeight, out double maxAscent, out double maxTextHeight, out double maxTextAscent, out double maxTextDescent, out float maxY, out double maxAscentOfLoweredPos, out IStringWidget lastTextWidget, out bool isClippedLine, out bool isTextInLine, out bool containsInlinePicture, out bool isAllWordsContainLoweredPos)
	{
		bool flag = true;
		maxHeight = 0.0;
		maxAscent = 0.0;
		maxTextHeight = 0.0;
		maxTextAscent = 0.0;
		maxTextDescent = 0.0;
		maxAscentOfLoweredPos = 0.0;
		maxY = float.MaxValue;
		isClippedLine = false;
		isTextInLine = false;
		containsInlinePicture = false;
		isAllWordsContainLoweredPos = true;
		bool flag2 = false;
		double num = 0.0;
		double num2 = 0.0;
		if (isFirstLineOfParagraph && Widget.LayoutInfo is ParagraphLayoutInfo && (Widget.LayoutInfo as ParagraphLayoutInfo).ListValue != string.Empty)
		{
			bool flag3 = false;
			using (List<LayoutedWidget>.Enumerator enumerator = ChildWidgets.GetEnumerator())
			{
				while (enumerator.MoveNext() && !(flag3 = IsIncludeWidgetInLineHeight(enumerator.Current.Widget)))
				{
				}
			}
			if (flag3)
			{
				WListFormat listFormatValue = paragraph.GetListFormatValue();
				if ((Widget.LayoutInfo as ParagraphLayoutInfo).CurrentListType == ListType.Bulleted && paragraph.GetListLevel(listFormatValue).PicBullet != null)
				{
					maxHeight = dc.MeasurePictureBulletSize(paragraph.GetListLevel(listFormatValue).PicBullet, (Widget.LayoutInfo as ParagraphLayoutInfo).ListFont.GetFont(paragraph.Document, FontScriptType.English)).Height;
					maxAscent = maxHeight;
				}
				else
				{
					maxAscent = dc.GetAscent((Widget.LayoutInfo as ParagraphLayoutInfo).ListFont.GetFont(paragraph.Document, FontScriptType.English), FontScriptType.English);
					maxHeight = dc.MeasureString((Widget.LayoutInfo as ParagraphLayoutInfo).ListValue, (Widget.LayoutInfo as ParagraphLayoutInfo).ListFont.GetFont(paragraph.Document, FontScriptType.English), null, null, isMeasureFromTabList: true, FontScriptType.English).Height;
					flag2 = true;
					num = maxAscent;
					num2 = maxHeight;
				}
				maxTextHeight = maxHeight;
				maxTextAscent = maxAscent;
				maxTextDescent = maxHeight - maxAscent;
				WCharacterFormat characterFormat = (Widget.LayoutInfo as ParagraphLayoutInfo).CharacterFormat;
				if (characterFormat.Position < 0f && (double)characterFormat.Position + maxAscent > 0.0)
				{
					maxAscentOfLoweredPos = (double)characterFormat.Position + maxAscent;
				}
				else
				{
					isAllWordsContainLoweredPos = false;
				}
			}
		}
		lastTextWidget = null;
		double num3 = 0.0;
		double num4 = 0.0;
		double num5 = 0.0;
		for (int i = 0; i < ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = ChildWidgets[i];
			IWidget widget = layoutedWidget.Widget;
			if (layoutedWidget == null || layoutedWidget.Widget.LayoutInfo.IsSkipBottomAlign || (widget is WField && ((widget as WField).FieldType == FieldType.FieldHyperlink || (widget as WField).FieldType == FieldType.FieldIncludePicture)))
			{
				continue;
			}
			if (layoutedWidget.Widget.LayoutInfo is FootnoteLayoutInfo)
			{
				widget = (layoutedWidget.Widget.LayoutInfo as FootnoteLayoutInfo).TextRange;
			}
			bool num6 = IsIncludeTextWidgetInLineHeight(widget);
			IStringWidget stringWidget = widget as IStringWidget;
			if (stringWidget == null && layoutedWidget.Widget is SplitStringWidget splitStringWidget)
			{
				stringWidget = splitStringWidget.RealStringWidget;
			}
			if ((!num6 && !(layoutedWidget.Widget.LayoutInfo is FootnoteLayoutInfo)) || (layoutedWidget.Widget.LayoutInfo is TabsLayoutInfo && !IsParaHasOnlyTabs()))
			{
				lastTextWidget = stringWidget;
				num3 = layoutedWidget.Bounds.Height;
				continue;
			}
			float topEdgeExtent = 0f;
			float bottomEdgeExtent = 0f;
			if (widget is ParagraphItem)
			{
				(widget as ParagraphItem).GetEffectExtentValues(out var _, out var _, out topEdgeExtent, out bottomEdgeExtent);
			}
			if (num4 < (double)layoutedWidget.Bounds.Height && ((layoutedWidget.Widget is ParagraphItem && (layoutedWidget.Widget as ParagraphItem).GetCharFormat() != null) || (layoutedWidget.Widget is SplitStringWidget && (layoutedWidget.Widget as SplitStringWidget).RealStringWidget is ParagraphItem && ((layoutedWidget.Widget as SplitStringWidget).RealStringWidget as ParagraphItem).GetCharFormat() != null)))
			{
				num4 = layoutedWidget.Bounds.Height;
				if (layoutedWidget.Widget is ParagraphItem)
				{
					num5 = dc.GetDescent((layoutedWidget.Widget as ParagraphItem).GetCharFormat().Font, FontScriptType.English);
				}
				else if (layoutedWidget.Widget is SplitStringWidget)
				{
					num5 = dc.GetDescent(((layoutedWidget.Widget as SplitStringWidget).RealStringWidget as ParagraphItem).GetCharFormat().Font, FontScriptType.English);
				}
			}
			if (((ChildWidgets.Count == 1 && maxHeight == 0.0) || maxHeight <= (double)(layoutedWidget.Bounds.Height + topEdgeExtent + bottomEdgeExtent)) && (!(layoutedWidget.Widget is Break) || ((layoutedWidget.Widget as Break).BreakType != BreakType.LineBreak && (layoutedWidget.Widget as Break).BreakType != BreakType.TextWrappingBreak)))
			{
				maxHeight = layoutedWidget.Bounds.Height + topEdgeExtent + bottomEdgeExtent;
				flag2 = false;
			}
			if (Math.Round(layoutedWidget.Bounds.Y - topEdgeExtent, 2) > Math.Round(m_bounds.Y, 2))
			{
				maxY = layoutedWidget.Bounds.Y;
			}
			if (!isClippedLine && widget.LayoutInfo.IsClipped)
			{
				isClippedLine = true;
			}
			if ((stringWidget != null && (!(layoutedWidget.Widget is WField) || (layoutedWidget.Widget as WField).FieldType != FieldType.FieldExpression)) || layoutedWidget.Widget is WSymbol)
			{
				float exceededLineAscent = float.MinValue;
				double num7 = ((!(layoutedWidget.Widget is WSymbol)) ? stringWidget.GetTextAscent(dc, ref exceededLineAscent) : ((double)dc.GetAscent((layoutedWidget.Widget as WSymbol).GetFont(dc), FontScriptType.English)));
				WCharacterFormat wCharacterFormat = ((stringWidget != null) ? (stringWidget as WTextRange).CharacterFormat : (layoutedWidget.Widget as WSymbol).CharacterFormat);
				if (layoutedWidget.Widget is WCheckBox)
				{
					float height = dc.MeasureString(" ", layoutedWidget.Widget.LayoutInfo.Font.GetFont(paragraph.Document, FontScriptType.English), dc.StringFormt, FontScriptType.English).Height;
					if (maxHeight < (double)height)
					{
						maxHeight = height;
					}
				}
				if ((layoutedWidget.Bounds.Height != 0f) & isAllWordsContainLoweredPos)
				{
					isAllWordsContainLoweredPos = (((wCharacterFormat.Position < 0f) & isAllWordsContainLoweredPos) ? true : false);
				}
				double num8 = num7 + (double)wCharacterFormat.Position;
				if (layoutedWidget.Bounds.Height != 0f && wCharacterFormat.Position < 0f && num8 > 0.0 && num8 > maxAscentOfLoweredPos)
				{
					maxAscentOfLoweredPos = num8;
				}
				if (maxAscent < num7 + (double)wCharacterFormat.Position && layoutedWidget.Bounds.Height != 0f)
				{
					maxAscent = num7 + (double)wCharacterFormat.Position;
				}
				if (wCharacterFormat.Position != 0f)
				{
					float num9 = 0.04f;
					if (maxHeight < maxAscent)
					{
						maxHeight = maxAscent;
					}
					else if (maxHeight < maxAscent + ((double)layoutedWidget.Bounds.Height - (num7 + (double)wCharacterFormat.Position)))
					{
						maxHeight = maxAscent + ((double)(layoutedWidget.Bounds.Height + wCharacterFormat.Font.Size * num9) - (num7 + (double)wCharacterFormat.Position));
					}
				}
				flag = false;
				if ((!(widget is WField) || (widget as WField).FieldType == FieldType.FieldExpression) && !(widget is WCheckBox) && !(widget is WDropDownFormField))
				{
					isTextInLine = true;
				}
				if (maxTextHeight < (double)layoutedWidget.Bounds.Height)
				{
					maxTextHeight = layoutedWidget.Bounds.Height;
				}
				if (maxTextAscent < num7)
				{
					maxTextAscent = num7;
				}
				if (maxTextDescent < (double)layoutedWidget.Bounds.Height - num7)
				{
					maxTextDescent = (double)layoutedWidget.Bounds.Height - num7;
				}
			}
			else
			{
				if (layoutedWidget.Widget is BookmarkEnd || layoutedWidget.Widget is BookmarkStart || layoutedWidget.Widget is InlineShapeObject || layoutedWidget.Widget is WCommentMark || layoutedWidget.Widget is WFieldMark || (layoutedWidget.Widget is Break && (layoutedWidget.Widget as Break).BreakType != BreakType.LineBreak && (layoutedWidget.Widget as Break).BreakType != BreakType.TextWrappingBreak && ((layoutedWidget.Widget as Break).BreakType != BreakType.ColumnBreak || paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)))
				{
					continue;
				}
				if (layoutedWidget.Widget is Break)
				{
					if (maxTextHeight == 0.0 && maxHeight == 0.0)
					{
						maxTextHeight = layoutedWidget.Bounds.Height;
						maxHeight = layoutedWidget.Bounds.Height;
					}
				}
				else
				{
					if (maxAscent < maxHeight)
					{
						if (layoutedWidget.Widget is WField && (layoutedWidget.Widget as WField).FieldType == FieldType.FieldExpression)
						{
							maxAscent = dc.GetAscentValueForEQField(layoutedWidget.Widget as WField);
						}
						else if (layoutedWidget is LayoutedMathWidget)
						{
							double num10 = (((layoutedWidget as LayoutedMathWidget).ChildWidgets.Count != 0) ? ((double)GetTextAscentValueForMath(layoutedWidget as LayoutedMathWidget, dc)) : 0.0);
							if (maxAscent < num10)
							{
								maxAscent = num10;
							}
						}
						else
						{
							maxAscent = maxHeight;
						}
					}
					if (IsInlineFloatingItem(layoutedWidget.Widget) || (layoutedWidget.Widget is WField && (layoutedWidget.Widget as WField).FieldType == FieldType.FieldExpression))
					{
						if (!(layoutedWidget.Widget is WField))
						{
							containsInlinePicture = true;
						}
						if (layoutedWidget.Widget is Shape && (layoutedWidget.Widget as Shape).IsHorizontalRule)
						{
							ParagraphItem paragraphItem = layoutedWidget.Widget as ParagraphItem;
							double num11 = dc.MeasureString(" ", paragraphItem.ParaItemCharFormat.Font, null, null, isMeasureFromTabList: true, FontScriptType.English).Height;
							double num12 = dc.GetAscent(paragraphItem.ParaItemCharFormat.Font, FontScriptType.English);
							if (maxHeight < num11)
							{
								maxHeight = num11;
							}
							if (maxAscent < num12)
							{
								maxAscent = num12;
							}
							maxTextHeight = maxHeight;
						}
						else if (paragraph.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
						{
							ParagraphItem paragraphItem2 = layoutedWidget.Widget as ParagraphItem;
							WCharacterFormat charFormat = paragraphItem2.GetCharFormat();
							if (charFormat != null)
							{
								double num11 = dc.MeasureString(" ", paragraphItem2.ParaItemCharFormat.GetFontToRender(FontScriptType.English), null, null, isMeasureFromTabList: true, FontScriptType.English).Height;
								double num12 = dc.GetAscent(paragraphItem2.ParaItemCharFormat.GetFontToRender(FontScriptType.English), FontScriptType.English);
								if (maxAscent < num12 + (double)charFormat.Position && layoutedWidget.Bounds.Height != 0f)
								{
									maxAscent = num12 + (double)charFormat.Position;
								}
								if (charFormat.Position != 0f)
								{
									if (maxHeight == (double)layoutedWidget.Bounds.Height && maxHeight == maxAscent && charFormat.Position < 0f && maxHeight > (double)Math.Abs(charFormat.Position))
									{
										maxAscent = maxHeight + (double)charFormat.Position;
										num11 = maxHeight;
										num12 = maxAscent;
									}
									else if (maxHeight < maxAscent)
									{
										maxHeight = maxAscent;
									}
									else if (maxHeight < maxAscent + ((double)layoutedWidget.Bounds.Height - (num12 + (double)charFormat.Position)))
									{
										maxHeight = maxAscent + ((double)layoutedWidget.Bounds.Height - (num12 + (double)charFormat.Position));
									}
								}
								if (ChildWidgets.Count == 1 && IsInlineFloatingItem(layoutedWidget.Widget) && !charFormat.PropertiesHash.ContainsKey(3) && (double)layoutedWidget.Bounds.Height < num11)
								{
									maxHeight = num11;
								}
								if (maxTextHeight < num11)
								{
									maxTextHeight = num11;
								}
								if (maxTextAscent < num12)
								{
									maxTextAscent = num12;
								}
								if (maxAscent < num12)
								{
									maxAscent = num12;
								}
							}
						}
						else if (maxTextHeight == 0.0)
						{
							bool flag4 = false;
							for (int j = i + 1; j < ChildWidgets.Count; j++)
							{
								_ = ChildWidgets[j];
								_ = ChildWidgets[j];
							}
							if (!flag4)
							{
								double num11 = paragraph.m_layoutInfo.Size.Height;
								double num12 = dc.GetAscent(paragraph.BreakCharacterFormat.GetFontToRender(FontScriptType.English), FontScriptType.English);
								if (maxHeight < num11)
								{
									maxHeight = num11;
								}
								if (maxTextHeight < num11)
								{
									maxTextHeight = num11;
								}
								if (maxTextAscent < num12)
								{
									maxTextAscent = num12;
								}
								if (maxAscent < num12)
								{
									maxAscent = num12;
								}
							}
						}
					}
				}
				flag = false;
			}
		}
		if (flag2 && maxHeight == num2)
		{
			maxHeight = num + num5;
		}
		if (flag && !IsSkipFieldCodeParagraphHeight())
		{
			double num13;
			if (!isLastLineOfParagraph && lastTextWidget != null)
			{
				float exceededLineAscent2 = float.MinValue;
				num13 = lastTextWidget.GetTextAscent(dc, ref exceededLineAscent2);
			}
			else
			{
				num3 = paragraph.m_layoutInfo.Size.Height;
				num13 = dc.GetAscent(paragraph.BreakCharacterFormat.GetFontToRender(FontScriptType.English), FontScriptType.English);
				if (ChildWidgets.Count == 1 && paragraph.ChildEntities.Count > 0 && paragraph.ChildEntities[0] is Break && (paragraph.ChildEntities[0] as Break).BreakType == BreakType.ColumnBreak && ((paragraph.ChildEntities[0] as Break).CharacterFormat.HasKey(3) || (paragraph.ChildEntities[0] as Break).CharacterFormat.HasKey(2)))
				{
					num3 = dc.MeasureString(" ", (paragraph.ChildEntities[0] as Break).CharacterFormat.Font, null, FontScriptType.English).Height;
					num13 = dc.GetAscent((paragraph.ChildEntities[0] as Break).CharacterFormat.GetFontToRender(FontScriptType.English), FontScriptType.English);
				}
				lastTextWidget = null;
			}
			if (maxAscent < num13)
			{
				maxAscent = num13;
			}
			if (maxHeight < num3)
			{
				maxHeight = num3;
			}
			if (maxTextHeight < num3)
			{
				maxTextHeight = num3;
			}
			if (maxTextAscent < num13)
			{
				maxTextAscent = num13;
			}
			if (maxTextDescent < num3 - num13)
			{
				maxTextDescent = num3 - num13;
			}
		}
		return flag;
	}

	private bool IsParaHasOnlyTabs()
	{
		for (int i = 0; i < ChildWidgets.Count; i++)
		{
			IWidget widget = ChildWidgets[i].Widget;
			if (!widget.LayoutInfo.IsSkipBottomAlign && !(widget.LayoutInfo is TabsLayoutInfo))
			{
				return false;
			}
		}
		return true;
	}

	private WField GetField()
	{
		int i = 0;
		if (Widget is SplitWidgetContainer && (Widget as SplitWidgetContainer).m_currentChild is Entity entity)
		{
			i = entity.Index;
		}
		for (WParagraph paragraph = GetParagraph(); i < paragraph.Items.Count; i++)
		{
			if (paragraph.Items[i] is WField)
			{
				return paragraph.Items[i] as WField;
			}
			if (!(paragraph.Items[i] is WFieldMark))
			{
				break;
			}
		}
		return null;
	}

	private bool IsSkipFieldCodeParagraphHeight()
	{
		WField field = GetField();
		WParagraph paragraph = GetParagraph();
		if (ChildWidgets.Count > 0)
		{
			return false;
		}
		if (field == null || (field.FieldSeparator == null && field.FieldEnd != null && paragraph.Items.Contains(field.FieldEnd)))
		{
			return false;
		}
		if (field.FieldSeparator == null)
		{
			return true;
		}
		for (int i = field.Index + 1; i < paragraph.Items.Count; i++)
		{
			if (paragraph.Items[i] == field.FieldSeparator)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsInlineFloatingItem(IWidget item)
	{
		if (item is WPicture || item is Shape || item is WTextBox || item is WChart || item is GroupShape)
		{
			return true;
		}
		return false;
	}

	public void AlignCenter(DrawingContext dc, double subWidth, bool isAlignCenter)
	{
		AlignCenterorRight(dc, subWidth, isAlignCenter);
	}

	public double AlignRight(DrawingContext dc, double subWidth, bool isAlignCenter)
	{
		return AlignCenterorRight(dc, subWidth, isAlignCenter);
	}

	internal double AlignCenterorRight(DrawingContext dc, double subWidth, bool isAlignCenter)
	{
		if (ChildWidgets.Count > 0)
		{
			int num = ChildWidgets.Count - 1;
			LayoutedWidget layoutedWidget = ChildWidgets[num];
			while (num >= 0)
			{
				if (ChildWidgets[num].Bounds.Width == 0f)
				{
					num--;
					continue;
				}
				layoutedWidget = ChildWidgets[num];
				break;
			}
			SplitStringWidget splitStringWidget = layoutedWidget.Widget as SplitStringWidget;
			string text = ((splitStringWidget != null) ? splitStringWidget.SplittedText : string.Empty);
			if (!string.IsNullOrEmpty(text) && text.EndsWith(ControlChar.Space))
			{
				int length = text.Length;
				text = text.TrimEnd(ControlChar.SpaceChar);
				int num2 = length - text.Length;
				if (splitStringWidget != null && splitStringWidget.Length > 1)
				{
					splitStringWidget.Length -= num2;
				}
				else
				{
					splitStringWidget.Length = text.Length;
				}
				layoutedWidget.IsContainsSpaceCharAtEnd = true;
				if (text == string.Empty)
				{
					subWidth = ((!isAlignCenter) ? (subWidth + (double)layoutedWidget.Bounds.Width) : (subWidth + (double)(layoutedWidget.Bounds.Width / 2f)));
					layoutedWidget.Bounds = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, 0f, layoutedWidget.Bounds.Height);
				}
				else
				{
					SizeF sizeF = splitStringWidget.Measure(dc);
					subWidth = ((!isAlignCenter) ? (subWidth + (double)(layoutedWidget.Bounds.Width - sizeF.Width)) : (subWidth + (double)((layoutedWidget.Bounds.Width - sizeF.Width) / 2f)));
					layoutedWidget.Bounds = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, sizeF.Width, layoutedWidget.Bounds.Height);
				}
			}
			else
			{
				subWidth += (double)ValidLayoutedItemCenterorRightPosition(dc, isAlignCenter, subWidth);
			}
		}
		ShiftLocation(subWidth, 0.0, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
		return subWidth;
	}

	private float ValidLayoutedItemCenterorRightPosition(DrawingContext dc, bool isAlignCenter, double subWidth)
	{
		int count = ChildWidgets.Count;
		float num = 0f;
		for (int num2 = count - 1; num2 >= 0; num2--)
		{
			LayoutedWidget layoutedWidget = ChildWidgets[num2];
			string text = ((!(layoutedWidget.Widget is WField)) ? ((layoutedWidget.Widget is WTextRange) ? (layoutedWidget.Widget as WTextRange).Text : null) : (((layoutedWidget.Widget as WField).FieldType == FieldType.FieldNumPages) ? (layoutedWidget.Widget as WField).FieldResult : (((layoutedWidget.Widget as WField).FieldType == FieldType.FieldPage) ? layoutedWidget.TextTag : null)));
			if (layoutedWidget.Widget is BookmarkStart || layoutedWidget.Widget is BookmarkEnd || (text != null && text.TrimEnd(ControlChar.SpaceChar) == "" && !(layoutedWidget.Widget.LayoutInfo is TabsLayoutInfo) && !(layoutedWidget.Widget is WCheckBox) && !(layoutedWidget.Widget is WDropDownFormField)))
			{
				num += layoutedWidget.Bounds.Width;
			}
			else
			{
				if (text != null && text.EndsWith(ControlChar.Space) && text.TrimEnd(ControlChar.SpaceChar) != "")
				{
					string text2 = (layoutedWidget.Widget as WTextRange).Text.TrimEnd(ControlChar.SpaceChar);
					SizeF sizeF = dc.MeasureTextRange(layoutedWidget.Widget as WTextRange, text2);
					float num3 = layoutedWidget.Bounds.Width - sizeF.Width;
					if (num3 > 0f)
					{
						num += num3;
					}
					break;
				}
				if (layoutedWidget.Widget is WMath && !(layoutedWidget.Widget as WMath).IsInline)
				{
					num = (float)(0.0 - subWidth);
					isAlignCenter = false;
				}
				else if (!(layoutedWidget.Widget is Break))
				{
					break;
				}
			}
		}
		if (isAlignCenter)
		{
			num /= 2f;
		}
		return num;
	}

	public void AlignJustify(DrawingContext dc, double subWidth, bool isFromInterSectingFloattingItem, bool isParaBidi)
	{
		m_bounds.Width += (float)subWidth;
		int[] array = new int[ChildWidgets.Count];
		int num = 0;
		string[] array2 = new string[ChildWidgets.Count];
		int tabIndex = GetTabIndex();
		for (int i = tabIndex; i < array.Length; i++)
		{
			LayoutedWidget layoutedWidget = ChildWidgets[i];
			IStringWidget stringWidget = layoutedWidget.Widget as IStringWidget;
			string text = null;
			if (stringWidget == null)
			{
				if (!(layoutedWidget.Widget is SplitStringWidget { SplittedText: not null } splitStringWidget))
				{
					array[i] = 0;
				}
				else
				{
					if (array.Length == 1 || (i == 0 && IsNeedToTrimTextRange(i + 1, ChildWidgets)))
					{
						string splittedText = splitStringWidget.SplittedText;
						int length = splittedText.Length;
						splittedText = splittedText.TrimStart(ControlChar.SpaceChar);
						int num2 = length - splittedText.Length;
						splitStringWidget.StartIndex += num2;
						if (splitStringWidget.Length > 1)
						{
							splitStringWidget.Length -= num2;
						}
						length = splittedText.Length;
						splittedText = splittedText.TrimEnd(ControlChar.SpaceChar);
						if (splitStringWidget.Length > 1)
						{
							splitStringWidget.Length -= length - splittedText.Length;
						}
						else
						{
							splitStringWidget.Length = splittedText.Length;
						}
						if (length - splittedText.Length > 0)
						{
							layoutedWidget.IsContainsSpaceCharAtEnd = true;
							if (splitStringWidget.SplittedText == string.Empty)
							{
								subWidth += (double)layoutedWidget.Bounds.Width;
								layoutedWidget.Bounds = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, 0f, layoutedWidget.Bounds.Height);
							}
							else
							{
								SizeF sizeF = splitStringWidget.Measure(dc);
								subWidth += (double)(layoutedWidget.Bounds.Width - sizeF.Width);
								layoutedWidget.Bounds = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, sizeF.Width, layoutedWidget.Bounds.Height);
							}
						}
					}
					else if (i == 0)
					{
						string splittedText2 = splitStringWidget.SplittedText;
						int length2 = splittedText2.Length;
						splittedText2 = splittedText2.TrimStart(ControlChar.SpaceChar);
						int num3 = length2 - splittedText2.Length;
						splitStringWidget.StartIndex += num3;
						splitStringWidget.Length -= num3;
					}
					else if (i == array.Length - 1 || IsNeedToTrimTextRange(i + 1, ChildWidgets))
					{
						string splittedText3 = splitStringWidget.SplittedText;
						int length3 = splittedText3.Length;
						splittedText3 = splittedText3.TrimEnd(ControlChar.SpaceChar);
						splitStringWidget.Length -= length3 - splittedText3.Length;
						if (length3 - splittedText3.Length > 0)
						{
							layoutedWidget.IsContainsSpaceCharAtEnd = true;
							if (splitStringWidget.SplittedText == string.Empty)
							{
								subWidth += (double)layoutedWidget.Bounds.Width;
								layoutedWidget.Bounds = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, 0f, layoutedWidget.Bounds.Height);
							}
							else
							{
								SizeF sizeF2 = splitStringWidget.Measure(dc);
								subWidth += (double)(layoutedWidget.Bounds.Width - sizeF2.Width);
								layoutedWidget.Bounds = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, sizeF2.Width, layoutedWidget.Bounds.Height);
							}
						}
					}
					text = splitStringWidget.GetText();
				}
			}
			else
			{
				text = ((!(layoutedWidget.Widget is WField)) ? (stringWidget as WTextRange).Text : (((layoutedWidget.Widget as WField).FieldType == FieldType.FieldNumPages) ? (layoutedWidget.Widget as WField).FieldResult : (((layoutedWidget.Widget as WField).FieldType == FieldType.FieldPage) ? layoutedWidget.TextTag : string.Empty)));
				WTextRange wTextRange = null;
				if (layoutedWidget.Widget is WTextRange && !(layoutedWidget.Widget is WField))
				{
					wTextRange = layoutedWidget.Widget as WTextRange;
				}
				if (wTextRange != null)
				{
					text = wTextRange.Text;
					if (i == 0)
					{
						text = text.TrimStart(ControlChar.SpaceChar);
					}
					if (i == array.Length - 1 || IsNeedToTrimTextRange(i + 1, ChildWidgets))
					{
						string text2 = text.TrimEnd(ControlChar.SpaceChar);
						if (text.Length - text2.Length > 0)
						{
							layoutedWidget.IsContainsSpaceCharAtEnd = true;
							text = text2;
							if (text2 == string.Empty)
							{
								subWidth += (double)layoutedWidget.Bounds.Width;
								layoutedWidget.Bounds = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, 0f, layoutedWidget.Bounds.Height);
							}
							else
							{
								WTextRange wTextRange2 = wTextRange.Clone() as WTextRange;
								wTextRange2.Text = wTextRange.Text.TrimEnd(ControlChar.SpaceChar);
								SizeF textRangeSize = wTextRange.GetTextRangeSize(wTextRange2);
								subWidth += (double)(layoutedWidget.Bounds.Width - textRangeSize.Width);
								layoutedWidget.Bounds = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, textRangeSize.Width, layoutedWidget.Bounds.Height);
							}
						}
					}
				}
			}
			if (text != null)
			{
				int num4 = (array[i] = text.Split(new char[1] { ' ' }).Length - 1);
				ChildWidgets[i].Spaces = num4;
				num += num4;
				array2[i] = text;
			}
		}
		float spaceDelta = GetSpaceDelta(num, subWidth, array, 0);
		double num5 = 0.0;
		int num6 = num;
		for (int j = tabIndex; j < array.Length; j++)
		{
			LayoutedWidget layoutedWidget2 = ChildWidgets[j];
			if (isFromInterSectingFloattingItem && layoutedWidget2.Widget is Entity && (layoutedWidget2.Widget as Entity).IsFloatingItem(isTextWrapAround: false))
			{
				continue;
			}
			IStringWidget obj = layoutedWidget2.Widget as IStringWidget;
			SplitStringWidget splitStringWidget2 = null;
			if (!isParaBidi)
			{
				layoutedWidget2.ShiftLocation(num5, 0.0, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
			}
			if (obj == null)
			{
				splitStringWidget2 = layoutedWidget2.Widget as SplitStringWidget;
			}
			if (obj != null || splitStringWidget2 != null)
			{
				RectangleF bounds = layoutedWidget2.Bounds;
				double num7 = spaceDelta * (float)array[j];
				if (j != array.Length - 1 && ChildWidgets[j + 1].Widget != null && ChildWidgets[j + 1].Widget.LayoutInfo is TabsLayoutInfo)
				{
					num7 = 0.0;
					num6 -= array[j];
					spaceDelta = GetSpaceDelta(num6, subWidth - num5, array, j + 1);
				}
				bounds.Width += (float)num7;
				layoutedWidget2.Bounds = bounds;
				num5 += num7;
				if (isParaBidi)
				{
					layoutedWidget2.ShiftLocation(0.0 - num5, 0.0, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
				}
			}
		}
	}

	private bool IsNeedToTrimTextRange(int index, LayoutedWidgetList childWidgets)
	{
		for (int num = childWidgets.Count - 1; num >= index; num--)
		{
			if (childWidgets[num].Bounds.Width != 0f)
			{
				if (childWidgets[num].Widget is IStringWidget && !(childWidgets[num].Widget is WField) && !(childWidgets[num].Widget.LayoutInfo is TabsLayoutInfo))
				{
					if (((childWidgets[num].Widget as IStringWidget) as WTextRange).Text.Trim(ControlChar.SpaceChar) != string.Empty)
					{
						return false;
					}
				}
				else
				{
					if (!(childWidgets[num].Widget is SplitStringWidget) || childWidgets[num].Widget.LayoutInfo is TabsLayoutInfo)
					{
						return false;
					}
					if ((childWidgets[num].Widget as SplitStringWidget).SplittedText.Trim(ControlChar.SpaceChar) != string.Empty)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private int GetTabIndex()
	{
		int num = 0;
		bool flag = false;
		for (int i = 0; i < ChildWidgets.Count; i++)
		{
			if (ChildWidgets[i].Widget.LayoutInfo is TabsLayoutInfo)
			{
				num = i + 1;
				flag = true;
			}
			else if (num >= i && flag)
			{
				WTextRange textRange = GetTextRange(ChildWidgets[i].Widget);
				if ((textRange != null && textRange.Text.Trim(' ') == string.Empty) || textRange == null)
				{
					num = i + 1;
				}
				else
				{
					flag = false;
				}
			}
		}
		return num;
	}

	private WTextRange GetTextRange(IWidget widget)
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

	private bool IsNotWord2013()
	{
		Entity entity = m_widget as Entity;
		if (m_widget is SplitStringWidget)
		{
			entity = (m_widget as SplitStringWidget).RealStringWidget as Entity;
		}
		else if (m_widget is SplitWidgetContainer)
		{
			entity = (m_widget as SplitWidgetContainer).RealWidgetContainer as Entity;
		}
		if (entity != null)
		{
			return entity.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013;
		}
		return false;
	}

	private float GetSpaceDelta(int countAllSpaces, double subWidth, int[] widgetSpaces, int index)
	{
		float num = ((countAllSpaces != 0) ? (Convert.ToSingle(subWidth) / (float)countAllSpaces) : 0f);
		SubWidth = Convert.ToSingle(subWidth);
		bool flag = IsNotWord2013();
		for (int i = index; i < ChildWidgets.Count; i++)
		{
			if (num < 1f && flag)
			{
				ChildWidgets[i].WordSpace = Convert.ToSingle(num) * -1f;
			}
			else
			{
				ChildWidgets[i].WordSpace = Convert.ToSingle(num);
			}
			ChildWidgets[i].SubWidth = (float)widgetSpaces[i] * num;
		}
		return num;
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

	internal static bool IsIncludeWidgetInLineHeight(IWidget widget)
	{
		if (widget is BookmarkStart || widget is BookmarkEnd || widget is WFieldMark || widget is Break || widget is WComment || widget.LayoutInfo is TabsLayoutInfo)
		{
			return false;
		}
		if (widget.LayoutInfo is FootnoteLayoutInfo)
		{
			widget = (widget.LayoutInfo as FootnoteLayoutInfo).TextRange;
		}
		return IsIncludeTextWidgetInLineHeight(widget);
	}

	internal static bool IsIncludeTextWidgetInLineHeight(IWidget widget)
	{
		string text = "";
		WTextRange wTextRange = null;
		if (widget is WTextRange && !(widget is WField))
		{
			wTextRange = widget as WTextRange;
			text = ((wTextRange is WDropDownFormField) ? (wTextRange as WDropDownFormField).DropDownValue : wTextRange.Text);
		}
		else if (widget is SplitStringWidget)
		{
			text = (widget as SplitStringWidget).SplittedText;
			wTextRange = (widget as SplitStringWidget).RealStringWidget as WTextRange;
		}
		if (wTextRange != null && wTextRange.Owner != null && text.Trim(ControlChar.SpaceChar) == string.Empty)
		{
			return false;
		}
		return true;
	}
}
