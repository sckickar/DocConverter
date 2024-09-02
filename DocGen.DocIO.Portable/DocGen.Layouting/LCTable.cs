using System;
using System.Collections.Generic;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.DocIO.DLS.Rendering;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.Layouting;

internal class LCTable : LayoutContext
{
	private const float DEF_MIN_WIDTH = 16f;

	private bool m_bHeaderRepeat;

	private bool isRowMoved;

	private int m_currHeaderRowIndex = -1;

	private int m_currRowIndex = -1;

	private int m_currColIndex = -1;

	private LayoutedWidget m_currRowLW;

	private LayoutedWidget m_currCellLW;

	protected bool m_bAtLastOneCellFitted;

	private SplitWidgetContainer[] m_splitedCells;

	private LayoutState m_blastRowState;

	private SplitTableWidget m_spitTableWidget;

	private WTable m_table;

	private bool m_isTableSplitted;

	private LayoutArea m_rowLayoutArea;

	private List<LayoutedWidget> m_verticallyMergeStartLW = new List<LayoutedWidget>();

	private float m_headerRowHeight;

	private float m_verticallyMergedCellFootnoteHeight;

	private RectangleF TableClientActiveArea;

	private float LayoutedHeaderRowHeight;

	private bool IsFirstItemInPage
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	public ILayoutInfo TableLayoutInfo => TableWidget.LayoutInfo;

	public ITableWidget TableWidget => m_widget as ITableWidget;

	public int CurrRowIndex
	{
		get
		{
			if (m_bHeaderRepeat)
			{
				return m_currHeaderRowIndex;
			}
			return m_currRowIndex;
		}
	}

	internal float LeftPad
	{
		get
		{
			float num = (((IWidget)m_table.Rows[0].Cells[0]).LayoutInfo as CellLayoutInfo).Paddings.Left + (((IWidget)m_table.Rows[0].Cells[0]).LayoutInfo as CellLayoutInfo).Margins.Left;
			if (m_table.TableFormat.CellSpacing > 0f)
			{
				num += m_table.TableFormat.Borders.Left.LineWidth;
			}
			return num;
		}
	}

	public LCTable(SplitTableWidget splitWidget, ILCOperator lcOperator, bool isForceFitLayout)
		: base(splitWidget.TableWidget, lcOperator, isForceFitLayout)
	{
		m_bHeaderRepeat = splitWidget.TableWidget is WTable && !(splitWidget.TableWidget as WTable).IsInCell;
		m_currRowIndex = splitWidget.StartRowNumber - (m_bHeaderRepeat ? 1 : 2);
		m_spitTableWidget = splitWidget;
		m_isTableSplitted = true;
	}

	public LCTable(ITableWidget table, ILCOperator lcOperator, bool isForceFitLayout)
		: base(table, lcOperator, isForceFitLayout)
	{
	}

	private new Entity GetBaseEntity(Entity entity)
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
		while (!(entity2 is WSection) && !(entity2 is HeaderFooter) && !(entity2 is WTableCell) && !(entity2 is WTextBox) && !(entity2 is Shape) && !(entity2 is GroupShape));
		return entity2;
	}

	private Entity GetParentBaseEntity(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WSection) && !(entity2 is HeaderFooter));
		return entity2;
	}

	public override LayoutedWidget Layout(RectangleF rect)
	{
		m_table = m_widget as WTable;
		if ((m_lcOperator as Layouter).IsNeedToRelayoutTable)
		{
			(m_lcOperator as Layouter).IsNeedToRelayoutTable = false;
		}
		if (GetLayoutedFloatingTable())
		{
			m_ltState = LayoutState.Fitted;
			return m_ltWidget;
		}
		IsFirstItemInPage = base.IsForceFitLayout;
		bool isSplittedTable = (TableLayoutInfo as ITableLayoutInfo).IsSplittedTable;
		MarginsF marginsF = InitializePageMargins();
		if (m_table.TableFormat.WrapTextAround && !IsInTextBoxOrShape(m_table))
		{
			TableClientActiveArea = rect;
			Entity baseEntity = GetBaseEntity(m_table);
			float height = (m_lcOperator as Layouter).ClientLayoutArea.Height;
			if (baseEntity is HeaderFooter)
			{
				height = (rect.Height = (baseEntity.Owner as WSection).PageSetup.PageSize.Height);
			}
			float x = rect.X;
			float y = rect.Y;
			if (m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && !m_table.IsInCell && !(TableLayoutInfo as ITableLayoutInfo).IsSplittedTable && m_table.TableFormat.WrapTextAround)
			{
				if (m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Page && m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.None)
				{
					rect.Height = m_table.TableFormat.Positioning.VertPosition - (m_lcOperator as Layouter).ClientLayoutArea.Y + (m_lcOperator as Layouter).ClientLayoutArea.Height;
				}
				else if (m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Margin && m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.None)
				{
					rect.Height = m_table.TableFormat.Positioning.VertPosition + (m_lcOperator as Layouter).ClientLayoutArea.Height;
				}
			}
			if (!(TableLayoutInfo as ITableLayoutInfo).IsSplittedTable)
			{
				if (m_table.IsInCell)
				{
					if (rect.Height <= Math.Abs(m_table.Rows[0].Height) && m_table.GetOwnerTableCell().OwnerRow.HeightType == TableRowHeightType.Exactly)
					{
						rect.Y = y;
					}
					else if (m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.None)
					{
						if (m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph)
						{
							rect.Y += m_table.TableFormat.Positioning.VertPosition;
						}
						else
						{
							rect.Y = (m_table.GetOwnerTableCell().m_layoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Top + m_table.TableFormat.Positioning.VertPosition;
						}
					}
					if (rect.Y < (m_table.GetOwnerTableCell().m_layoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Top || (m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Margin && m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Top))
					{
						rect.Y = (m_table.GetOwnerTableCell().m_layoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Top;
					}
				}
				else if (m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph)
				{
					if (m_table.TableFormat.Positioning.VertPosition < 0f && (TableLayoutInfo as ITableLayoutInfo).Height > rect.Height && IsTableMoveToNextPage(m_table) && !IsFirstItemInPage)
					{
						rect.Y += rect.Height;
					}
					else if (m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.None)
					{
						rect.Y += m_table.TableFormat.Positioning.VertPosition - ((m_table.TableFormat.Bidi || m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 || (m_lcOperator as Layouter).WrappingDifference == float.MinValue || (m_lcOperator as Layouter).WrappingDifference < 0f) ? 0f : (m_lcOperator as Layouter).WrappingDifference);
					}
				}
				else if (m_table.TableFormat.Positioning.VertPositionAbs != 0)
				{
					if ((m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Top || m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Inside) && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Margin && marginsF != null)
					{
						rect.Y = marginsF.Top + (m_table.Document.DOP.GutterAtTop ? marginsF.Gutter : 0f);
					}
				}
				else if (m_table.TableFormat.Positioning.VertPosition != 0f)
				{
					if (!m_table.IsInCell && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Margin && marginsF != null)
					{
						rect.Y = marginsF.Top + m_table.TableFormat.Positioning.VertPosition + (m_table.Document.DOP.GutterAtTop ? marginsF.Gutter : 0f);
					}
					else
					{
						rect.Y = m_table.TableFormat.Positioning.VertPosition;
					}
				}
				if (rect.Y < 0f && !(m_lcOperator as Layouter).IsLayoutingHeaderFooter)
				{
					rect.Y = y;
				}
				if (m_table.TableFormat.WrapTextAround && m_table.Document.DOP.Dop2000.Copts.DontBreakWrappedTables && IsFirstItemInPage && !m_table.IsInCell && baseEntity is WSection)
				{
					rect.Height = (baseEntity as WSection).PageSetup.PageSize.Height;
				}
				else if (m_table.OwnerTextBody != null && !(m_lcOperator as Layouter).IsLayoutingHeaderFooter && m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Bottom && (m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Margin || m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Page) && !m_table.Document.DOP.Dop2000.Copts.DontBreakWrappedTables)
				{
					rect.Height = TableClientActiveArea.Height;
				}
				else
				{
					rect.Height += y - rect.Y;
					if (m_table.TableFormat.Positioning.VertPositionAbs != 0 && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Page && !m_table.IsInCell)
					{
						rect.Height = height;
					}
				}
			}
			bool flag = false;
			if (m_table.TableFormat.Positioning.HorizRelationTo == HorizontalRelation.Column && m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left)
			{
				rect.X += m_table.TableFormat.Positioning.HorizPosition;
				flag = true;
			}
			if (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left && (!(baseEntity is WSection) || (baseEntity as WSection).Columns.Count <= 1))
			{
				if (m_table.IsInCell)
				{
					CellLayoutInfo cellLayoutInfo = ((IWidget)m_table.GetOwnerTableCell()).LayoutInfo as CellLayoutInfo;
					if (rect.Right + cellLayoutInfo.Margins.Right < x + m_table.Width + m_table.TableFormat.Positioning.HorizPosition)
					{
						float num = rect.Right + cellLayoutInfo.Margins.Right - m_table.Width;
						if (x - cellLayoutInfo.Margins.Left > num)
						{
							num = x - cellLayoutInfo.Margins.Left;
						}
						rect.X = num;
					}
					else if (!flag)
					{
						rect.X += m_table.TableFormat.Positioning.HorizPosition;
					}
				}
				else if ((m_table.TableFormat.Positioning.HorizRelationTo == HorizontalRelation.Margin || m_table.TableFormat.Positioning.HorizRelationTo == HorizontalRelation.Column) && marginsF != null)
				{
					rect.X = Layouter.GetLeftMargin(m_table.GetOwnerSection(m_table) as WSection) + m_table.TableFormat.Positioning.HorizPosition;
				}
				else
				{
					rect.X = m_table.TableFormat.Positioning.HorizPosition;
				}
			}
			if (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Inside && m_table.TableFormat.Positioning.HorizRelationTo == HorizontalRelation.Page)
			{
				rect.X = 0f;
			}
		}
		if (m_table.OwnerTextBody.Owner is WTextBox && (m_table.OwnerTextBody.Owner as WTextBox).TextBoxFormat.AutoFit)
		{
			float height3 = (TableLayoutInfo as ITableLayoutInfo).Height;
			if (height3 > rect.Height)
			{
				rect.Height = height3;
			}
		}
		CreateTableClientArea(ref rect);
		if (m_table.IsInCell)
		{
			float num2 = (m_table.GetOwnerTableCell().m_layoutInfo as CellLayoutInfo).CellContentLayoutingBounds.Left - (m_table.GetOwnerTableCell().m_layoutInfo as CellLayoutInfo).Margins.Left;
			if (rect.X < num2)
			{
				rect.X = num2;
			}
			CreateLayoutArea(rect);
		}
		DocGen.Drawing.Font font = ((m_table.Rows[0].Cells[0].LastParagraph != null) ? m_table.Rows[0].Cells[0].LastParagraph.BreakCharacterFormat.GetFontToRender(FontScriptType.English) : m_table.Rows[0].Cells[0].CharacterFormat.GetFontToRender(FontScriptType.English));
		SizeF sizeF = base.DrawingContext.MeasureString(" ", font, null, FontScriptType.English);
		sizeF.Width = m_table.Width;
		float num3 = ((m_table.Rows.Count > 0) ? m_table.Rows[0].Height : 0f);
		if (num3 > sizeF.Height)
		{
			sizeF.Height = num3;
		}
		SizeF sizeF2 = sizeF;
		float height4 = sizeF.Height;
		sizeF2.Height = (TableLayoutInfo as ITableLayoutInfo).Height;
		if (m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && (!IsFirstItemInPage || !m_table.TableFormat.WrapTextAround) && (m_lcOperator as Layouter).FloatingItems.Count > 0 && sizeF2.Height < m_layoutArea.ClientActiveArea.Height)
		{
			float num4 = (float)m_table.Rows.Count * sizeF.Height;
			if (sizeF2.Height <= sizeF.Height)
			{
				sizeF.Height = ((num4 <= m_layoutArea.ClientActiveArea.Height) ? num4 : m_layoutArea.ClientActiveArea.Height);
			}
			else
			{
				sizeF.Height = ((sizeF2.Height <= m_layoutArea.ClientActiveArea.Height) ? sizeF2.Height : m_layoutArea.ClientActiveArea.Height);
			}
		}
		RectangleF tableClientArea = rect;
		AdjustClientAreaBasedOnTextWrap(sizeF, ref rect, height4);
		bool isWrappedTable = m_table.TableFormat.WrapTextAround && rect.Y != tableClientArea.Y && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Page;
		bool isFitRowByUsingVerticalDistance = false;
		CreateLayoutedWidget(rect.Location);
		int currRowIndex = m_currRowIndex;
		do
		{
			IWidget widget = CreateRowLayoutedWidget();
			if (widget == null || widget.LayoutInfo.IsSkip)
			{
				if (m_bAtLastOneCellFitted)
				{
					m_ltState = LayoutState.Fitted;
				}
				if (widget == null || !m_table.Rows[CurrRowIndex].IsDeleteRevision)
				{
					break;
				}
				if (!m_table.Document.RevisionOptions.ShowDeletedText)
				{
					m_ltState = LayoutState.Unknown;
					continue;
				}
			}
			LayoutRow(widget, ref isFitRowByUsingVerticalDistance);
			if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
			{
				return null;
			}
			if (IsAdjacentCellHasFootnote())
			{
				FootnoteRowLayouting();
			}
			CommitRow();
			if ((widget as WTableRow).IsDeleteRevision && (widget as WTableRow).Index != (base.Widget as WTable).LastRow.Index)
			{
				m_ltState = LayoutState.Unknown;
			}
			((m_currRowLW.Widget as WTableRow).m_layoutInfo as RowLayoutInfo).IsFootnoteReduced = false;
			((m_currRowLW.Widget as WTableRow).m_layoutInfo as RowLayoutInfo).IsFootnoteSplitted = false;
			if (isFitRowByUsingVerticalDistance && m_ltWidget.ChildWidgets.Count == 1)
			{
				RectangleF clientActiveArea = m_layoutArea.ClientActiveArea;
				clientActiveArea.Height = 0f;
				m_layoutArea = new LayoutArea(clientActiveArea);
			}
		}
		while ((m_ltState == LayoutState.Unknown && !(m_lcOperator as Layouter).IsNeedToRelayoutTable) || IsTableRelayout(ref tableClientArea, currRowIndex));
		UpdateTableLWBounds(isWrappedTable, isSplittedTable);
		return m_ltWidget;
	}

	private void ClearVerticalMergeStartLW()
	{
		foreach (LayoutedWidget childWidget in m_currRowLW.ChildWidgets)
		{
			if (m_verticallyMergeStartLW == null || m_verticallyMergeStartLW.Count == 0)
			{
				break;
			}
			if (m_verticallyMergeStartLW.Contains(childWidget))
			{
				m_verticallyMergeStartLW.Remove(childWidget);
			}
		}
	}

	private bool IsLayoutedFloatingTableInTextBodyItems()
	{
		int num = (m_lcOperator as Layouter).MaintainltWidget.ChildWidgets.Count;
		while (num > 0)
		{
			LayoutedWidget layoutedWidget = (m_lcOperator as Layouter).MaintainltWidget.ChildWidgets[--num];
			for (int num2 = layoutedWidget.ChildWidgets.Count - 1; num2 >= 0; num2--)
			{
				LayoutedWidget layoutedWidget2 = layoutedWidget.ChildWidgets[num2];
				for (int num3 = layoutedWidget2.ChildWidgets.Count - 1; num3 >= 0; num3--)
				{
					LayoutedWidget layoutedWidget3 = layoutedWidget2.ChildWidgets[num3];
					if (m_table == layoutedWidget3.Widget)
					{
						m_ltWidget = layoutedWidget3;
						return true;
					}
					if ((layoutedWidget3.Widget is BlockContentControl || (layoutedWidget3.Widget is SplitWidgetContainer && (layoutedWidget3.Widget as SplitWidgetContainer).RealWidgetContainer is BlockContentControl)) && IsLayoutedFloatingTableInBlockContentControl(layoutedWidget3))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool IsLayoutedFloatingTableInBlockContentControl(LayoutedWidget ltWidget)
	{
		for (int num = ltWidget.ChildWidgets.Count - 1; num >= 0; num--)
		{
			LayoutedWidget layoutedWidget = ltWidget.ChildWidgets[num];
			if (m_table == layoutedWidget.Widget)
			{
				m_ltWidget = layoutedWidget;
				return true;
			}
			if (layoutedWidget.Widget is BlockContentControl || (layoutedWidget.Widget is SplitWidgetContainer && (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer is BlockContentControl))
			{
				IsLayoutedFloatingTableInBlockContentControl(layoutedWidget);
			}
			if (m_ltWidget != null)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsTableRelayout(ref RectangleF tableClientArea, int startingRowIndex)
	{
		if (m_ltState != LayoutState.Fitted && m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph && IsWord2013(m_table.Document) && tableClientArea != RectangleF.Empty && tableClientArea.Y < m_ltWidget.Bounds.Y)
		{
			for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
			{
				Entity floatingEntity = (m_lcOperator as Layouter).FloatingItems[i].FloatingEntity;
				if ((floatingEntity is WParagraph && (floatingEntity as WParagraph).ParagraphFormat.IsFrame && (floatingEntity as WParagraph).IsInCell && (floatingEntity as WParagraph).Owner.Owner.Owner as WTable == m_table) || (floatingEntity is ParagraphItem && (floatingEntity as ParagraphItem).OwnerParagraph.IsInCell && (floatingEntity as ParagraphItem).OwnerParagraph.Owner.Owner.Owner as WTable == m_table) || (floatingEntity is WTable && (floatingEntity as WTable).IsInCell && (floatingEntity as WTable).Owner.Owner.Owner as WTable == m_table))
				{
					(m_lcOperator as Layouter).FloatingItems.Remove((m_lcOperator as Layouter).FloatingItems[i]);
				}
			}
			while (m_ltWidget.ChildWidgets.Count > 0)
			{
				m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].InitLayoutInfoAll();
				m_ltWidget.ChildWidgets.RemoveAt(m_ltWidget.ChildWidgets.Count - 1);
			}
			CreateLayoutArea(tableClientArea);
			CreateLayoutedWidget(tableClientArea.Location);
			m_currRowIndex = startingRowIndex;
			m_currColIndex = -1;
			tableClientArea = RectangleF.Empty;
			m_blastRowState = (m_ltState = LayoutState.Unknown);
			return true;
		}
		return false;
	}

	private bool IsNeedToMoveRow(FloatingItem item)
	{
		if (!item.LayoutInCell && m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && item.TextWrappingStyle == TextWrappingStyle.TopAndBottom)
		{
			return true;
		}
		return false;
	}

	private bool IsInTextBoxOrShape(WTable table)
	{
		Entity baseEntity = GetBaseEntity(table);
		if (baseEntity is WTextBox || baseEntity is Shape)
		{
			return true;
		}
		return false;
	}

	private bool IsLayoutedFloatingTableInWTablacell()
	{
		for (int num = (m_lcOperator as Layouter).MaintainltWidget.ChildWidgets.Count - 1; num >= 0; num--)
		{
			if (m_table == (m_lcOperator as Layouter).MaintainltWidget.ChildWidgets[num].Widget)
			{
				m_ltWidget = (m_lcOperator as Layouter).MaintainltWidget.ChildWidgets[num];
				return true;
			}
		}
		return false;
	}

	private bool IsContainsTable(FloatingItem floatingItem)
	{
		if (floatingItem.FrameEntities != null)
		{
			for (int i = 0; i < floatingItem.FrameEntities.Count; i++)
			{
				if (floatingItem.FrameEntities[i] == m_table)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool GetLayoutedFloatingTable()
	{
		for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
		{
			if (((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity == m_table || (m_table.IsFrame && IsContainsTable((m_lcOperator as Layouter).FloatingItems[i]))) && (m_lcOperator as Layouter).MaintainltWidget.ChildWidgets.Count > 0 && (m_table.GetOwnerSection(m_table) as WSection).Columns.Count <= 1)
			{
				IWidget widget = (m_lcOperator as Layouter).MaintainltWidget.Widget;
				if (widget is WTableCell || (widget is SplitWidgetContainer && (widget as SplitWidgetContainer).RealWidgetContainer is WTableCell))
				{
					return IsLayoutedFloatingTableInWTablacell();
				}
				if (!m_table.IsInCell)
				{
					return IsLayoutedFloatingTableInTextBodyItems();
				}
			}
		}
		return false;
	}

	private void FootnoteRowLayouting()
	{
		float height = 0f;
		LayoutedWidget layoutedWidget = new LayoutedWidget(m_currRowLW as IWidget);
		if (((m_currRowLW.Widget as WTableRow).m_layoutInfo as RowLayoutInfo).IsFootnoteSplitted)
		{
			layoutedWidget.GetFootnoteHeightForTableRow(ref height, m_currRowLW);
			for (int i = 0; i < (m_lcOperator as Layouter).FootnoteSplittedWidgets.Count; i++)
			{
				height -= (m_lcOperator as Layouter).FootnoteSplittedWidgets[i].m_currentChild.LayoutInfo.Size.Height;
			}
		}
		else
		{
			layoutedWidget.GetFootnoteHeightForTableRow(ref height, m_currRowLW);
		}
		height += m_verticallyMergedCellFootnoteHeight;
		m_currColIndex = -1;
		ClearVerticalMergeStartLW();
		m_currRowLW.ChildWidgets.Clear();
		(m_lcOperator as Layouter).FootnoteSplittedWidgets.Clear();
		((m_currRowLW.Widget as WTableRow).m_layoutInfo as RowLayoutInfo).IsFootnoteSplitted = false;
		RemoveFootnoteFromLayouter();
		RectangleF clientActiveArea = m_layoutArea.ClientActiveArea;
		clientActiveArea.Height -= height;
		m_layoutArea.UpdateClientActiveArea(clientActiveArea);
		SizeF size = new SizeF(0f, 0f);
		m_currRowLW.Bounds = new RectangleF(m_currRowLW.Bounds.Location, size);
		((m_currRowLW.Widget as WTableRow).m_layoutInfo as RowLayoutInfo).IsFootnoteReduced = true;
		LayoutRow(m_currRowLW.Widget as WTableRow);
		m_verticallyMergedCellFootnoteHeight = 0f;
	}

	private bool IsAdjacentCellHasFootnote()
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < (m_currRowLW.Widget as WTableRow).Cells.Count; i++)
		{
			if (flag)
			{
				flag2 = true;
			}
			for (int j = 0; j < (m_currRowLW.Widget as WTableRow).Cells[i].Items.Count; j++)
			{
				CellLayoutInfo cellLayoutInfo = ((IWidget)(m_currRowLW.Widget as WTableRow).Cells[i]).LayoutInfo as CellLayoutInfo;
				if (cellLayoutInfo.IsSkip || cellLayoutInfo.IsRowMergeContinue || !((m_currRowLW.Widget as WTableRow).Cells[i].Items[j] is WParagraph) || !((m_currRowLW.Widget as WTableRow).Cells[i].Items[j] is WParagraph wParagraph))
				{
					continue;
				}
				for (int k = 0; k < wParagraph.Items.Count; k++)
				{
					if (wParagraph.Items[k] is WFootnote)
					{
						if (!flag)
						{
							flag = true;
						}
						else if (flag2)
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private void RemoveFootnoteFromLayouter()
	{
		for (int i = 0; i < (m_currRowLW.Widget as WTableRow).Cells.Count; i++)
		{
			for (int j = 0; j < (m_currRowLW.Widget as WTableRow).Cells[i].Items.Count; j++)
			{
				if (!((m_currRowLW.Widget as WTableRow).Cells[i].Items[j] is WParagraph) || !((m_currRowLW.Widget as WTableRow).Cells[i].Items[j] is WParagraph wParagraph))
				{
					continue;
				}
				for (int k = 0; k < wParagraph.Items.Count; k++)
				{
					if (!(wParagraph.Items[k] is WFootnote))
					{
						continue;
					}
					for (int num = (m_lcOperator as Layouter).FootnoteWidgets.Count - 1; num >= 0; num--)
					{
						LayoutedWidget layoutedWidget = (m_lcOperator as Layouter).FootnoteWidgets[num];
						if (layoutedWidget.Widget is WTextBody && (layoutedWidget.Widget as WTextBody).Owner == wParagraph.Items[k] && wParagraph.Items[k] is WFootnote)
						{
							(m_lcOperator as Layouter).FootnoteWidgets.RemoveAt(num);
							(wParagraph.Items[k] as WFootnote).IsLayouted = false;
						}
					}
				}
			}
		}
	}

	private bool IsWrappedTable()
	{
		if (m_table.TableFormat.WrapTextAround && m_table.OwnerTextBody.OwnerBase is WSection && !(m_lcOperator as Layouter).IsLayoutingHeaderFooter && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph && m_table.TableFormat.Positioning.VertPosition < 0f && m_ltState == LayoutState.Splitted && m_ltWidget.Bounds.X < (m_table.OwnerTextBody.OwnerBase as WSection).PageSetup.PageSize.Width && m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && !IsFirstItemInPage)
		{
			return true;
		}
		return false;
	}

	private void UpdateTableLWBounds(bool isWrappedTable, bool isSplittedtable)
	{
		if (m_ltWidget.ChildWidgets.Count == 0)
		{
			(TableLayoutInfo as TableLayoutInfo).IsSplittedTable = false;
		}
		if (!isSplittedtable && m_table.TableFormat.Positioning.VertPositionAbs != 0 && (m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Page || (m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Margin && m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Bottom)))
		{
			m_ltWidget.ShiftLocation(0.0, 0f - m_ltWidget.Bounds.Y, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
		}
		UpdateTableKeepWithNext();
		TableLayoutInfo tableLayoutInfo = TableLayoutInfo as TableLayoutInfo;
		if (m_table.TableFormat.WrapTextAround && m_table.OwnerTextBody.OwnerBase is WSection && !(m_lcOperator as Layouter).IsLayoutingHeaderFooter && m_ltWidget.Bounds.Height > TableClientActiveArea.Height && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Page && (m_table.TableFormat.Positioning.HorizRelationTo != HorizontalRelation.Page || !(m_ltWidget.Bounds.X < (m_table.OwnerTextBody.OwnerBase as WSection).PageSetup.PageSize.Width)) && ((m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && !IsFirstItemInPage) || (m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.None && m_ltWidget.Bounds.X >= TableClientActiveArea.X && m_ltWidget.Bounds.Right > TableClientActiveArea.Right)))
		{
			isWrappedTable = true;
		}
		else if (!isWrappedTable)
		{
			isWrappedTable = IsWrappedTable();
		}
		bool flag = (m_lcOperator as Layouter).CurrentSection.Columns.Count > 1;
		if (tableLayoutInfo != null && !tableLayoutInfo.IsSplittedTable && m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.VertRelationTo != VerticalRelation.Paragraph)
		{
			UpdateAbsoluteTablePosition();
		}
		float num = m_ltWidget.Bounds.Y;
		if (m_table.TableFormat.WrapTextAround && (((m_table.Document.DOP.Dop2000.Copts.DontBreakWrappedTables || flag) && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph) || isWrappedTable))
		{
			num -= m_table.TableFormat.Positioning.VertPosition;
		}
		if (!DocumentLayouter.IsFirstLayouting)
		{
			m_ltWidget.Widget.LayoutInfo.IsFirstItemInPage = false;
		}
		float pageTopMargin = (m_lcOperator as Layouter).PageTopMargin;
		if (IsFirstItemInPage)
		{
			m_ltWidget.Widget.LayoutInfo.IsFirstItemInPage = true;
		}
		if (flag && Math.Round(num, 2) == Math.Round(pageTopMargin, 2))
		{
			m_ltWidget.Widget.LayoutInfo.IsFirstItemInPage = true;
		}
		WTableRow wTableRow = null;
		if (m_ltWidget.ChildWidgets.Count > 0)
		{
			wTableRow = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Widget as WTableRow;
			RowLayoutInfo rowLayoutInfo = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Widget.LayoutInfo as RowLayoutInfo;
			bool flag2 = IsNeedToSkipSplittingTable();
			if ((m_table.TableFormat.WrapTextAround && (((m_table.Document.DOP.Dop2000.Copts.DontBreakWrappedTables || flag) && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph) || isWrappedTable)) || flag2)
			{
				if (m_ltWidget.Widget.LayoutInfo.IsFirstItemInPage)
				{
					Entity baseEntity = GetBaseEntity(m_table);
					float num2 = ((baseEntity is WSection) ? (baseEntity as WSection).PageSetup.PageSize.Height : 0f);
					float bottom = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Bounds.Bottom;
					float num3 = ((bottom > num2) ? (num2 - bottom) : 0f);
					if (Math.Abs(num3) > pageTopMargin)
					{
						num3 = 0f - pageTopMargin;
					}
					m_ltState = LayoutState.Fitted;
					if (num3 != 0f)
					{
						m_ltWidget.ShiftLocation(0.0, num3, m_ltWidget.Bounds.Width, m_ltWidget.Bounds.Height, isHeader: false);
					}
				}
				else
				{
					if (!((wTableRow != null && wTableRow.Index != m_table.Rows.Count - 1) || (m_ltState != LayoutState.Fitted && m_table.TableFormat.WrapTextAround && (isWrappedTable || flag)) || (m_ltState == LayoutState.Fitted && m_table.TableFormat.WrapTextAround && isWrappedTable && TableClientActiveArea.Height < m_ltWidget.Bounds.Height) || flag2))
					{
						return;
					}
					if (isWrappedTable || flag)
					{
						for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
						{
							if (GetOwnerTable((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity) == m_table)
							{
								(m_lcOperator as Layouter).FloatingItems.RemoveAt(i);
							}
						}
					}
					m_ltState = LayoutState.NotFitted;
					tableLayoutInfo.IsSplittedTable = false;
				}
				return;
			}
			if (wTableRow != null && wTableRow.GetRowIndex() == m_table.Rows.Count - 1 && DocumentLayouter.IsFirstLayouting && !rowLayoutInfo.IsRowSplitted)
			{
				tableLayoutInfo.IsSplittedTable = false;
			}
			if (m_table.TableFormat.WrapTextAround && !IsInTextBoxOrShape(m_table) && !m_table.IsFrame && m_ltState == LayoutState.Splitted && !IsFirstItemInPage && m_ltWidget.Bounds.Y < TableClientActiveArea.Y && m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.None)
			{
				m_ltState = LayoutState.NotFitted;
				tableLayoutInfo.IsSplittedTable = false;
				return;
			}
		}
		if (m_ltWidget.ChildWidgets.Count > 0)
		{
			tableLayoutInfo.IsHeaderRowHeightUpdated = true;
		}
		if (LayoutedHeaderRowHeight > 0f)
		{
			tableLayoutInfo.HeaderRowHeight = LayoutedHeaderRowHeight;
			if (wTableRow != null && m_ltState == LayoutState.Fitted && wTableRow.Index == m_table.Rows.Count - 1)
			{
				tableLayoutInfo.IsHeaderNotRepeatForAllPages = true;
			}
		}
		if (!m_table.TableFormat.WrapTextAround && m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && m_ltWidget.ChildWidgets.Count > 0 && (m_ltWidget.ChildWidgets[0].Widget as WTableRow).GetRowIndex() == 0 && m_ltWidget.ChildWidgets.Count < m_table.Rows.Count && m_table.Rows[m_ltWidget.ChildWidgets.Count - 1].IsHeader)
		{
			bool flag3 = true;
			for (int j = 0; j < m_ltWidget.ChildWidgets.Count - 1; j++)
			{
				if (!m_table.Rows[j].IsHeader)
				{
					flag3 = false;
				}
			}
			if (flag3)
			{
				if (!IsFirstItemInPage)
				{
					tableLayoutInfo.IsHeaderNotRepeatForAllPages = false;
					(m_table.Rows[CurrRowIndex].m_layoutInfo as RowLayoutInfo).IsRowBreakByPageBreakBefore = false;
					(m_ltWidget.Widget.LayoutInfo as TableLayoutInfo).HeaderRowHeight = 0f;
					m_ltState = LayoutState.NotFitted;
					base.IsVerticalNotFitted = true;
				}
				else
				{
					(m_table.Rows[CurrRowIndex].m_layoutInfo as RowLayoutInfo).IsRowBreakByPageBreakBefore = true;
				}
				tableLayoutInfo.IsHeaderRowHeightUpdated = false;
			}
		}
		if (!(m_table.Owner is WTextBody) || (m_lcOperator as Layouter).IsLayoutingHeaderFooter || !m_table.TableFormat.WrapTextAround || m_table.TableFormat.Positioning.VertPositionAbs != VerticalPosition.Bottom || m_spitTableWidget != null || m_table.Document.DOP.Dop2000.Copts.DontBreakWrappedTables)
		{
			return;
		}
		float num4 = 0f;
		float num5 = ((m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Margin) ? (m_lcOperator as Layouter).CurrentSection.PageSetup.Margins.Bottom : 0f);
		if (tableLayoutInfo.IsSplittedTable && (m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Page || m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Margin))
		{
			num4 = (m_lcOperator as Layouter).CurrentSection.PageSetup.PageSize.Height - m_ltWidget.Bounds.Height - num5;
			m_ltWidget.ShiftLocation(0.0, num4, isPictureNeedToBeShifted: false, isFromFloatingItemVerticalAlignment: false);
		}
		if (m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Page && m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && m_ltWidget.ChildWidgets.Count > 0)
		{
			if (!base.IsForceFitLayout && m_ltWidget.Bounds.Y + m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Bounds.Height > TableClientActiveArea.Bottom)
			{
				m_ltState = LayoutState.NotFitted;
			}
			else
			{
				RelayoutingTable();
			}
		}
	}

	internal Entity GetOwnerTable(Entity entity)
	{
		Entity entity2 = entity;
		if (entity2.Owner == null)
		{
			return null;
		}
		entity2 = entity2.Owner;
		while (!(entity2 is WTable))
		{
			if (entity2.Owner == null)
			{
				return null;
			}
			entity2 = entity2.Owner;
		}
		return entity2 as WTable;
	}

	private bool IsNeedToSkipSplittingTable()
	{
		if (!m_table.TableFormat.WrapTextAround && !m_table.IsInCell && m_table.IsCompleteFrame)
		{
			return m_ltState != LayoutState.Fitted;
		}
		return false;
	}

	private void RelayoutingTable()
	{
		int num = 0;
		for (int i = 0; i < m_ltWidget.ChildWidgets.Count; i++)
		{
			if (m_ltWidget.ChildWidgets[i].Bounds.Bottom > TableClientActiveArea.Bottom)
			{
				num++;
				if (i == 0)
				{
					num = m_ltWidget.ChildWidgets.Count - num;
					break;
				}
			}
		}
		m_ltWidget.ChildWidgets.RemoveRange(m_ltWidget.ChildWidgets.Count - num, num);
		m_sptWidget = new SplitTableWidget(TableWidget, m_ltWidget.ChildWidgets.Count + 1);
		m_ltState = LayoutState.Splitted;
		(TableLayoutInfo as ITableLayoutInfo).IsSplittedTable = true;
	}

	private void UpdateTableKeepWithNext()
	{
		if (m_table.TableFormat.WrapTextAround || m_table.IsInCell || m_ltWidget.ChildWidgets.Count <= 0)
		{
			return;
		}
		m_ltWidget.Widget.LayoutInfo.IsKeepWithNext = true;
		for (int i = 0; i < m_ltWidget.ChildWidgets.Count; i++)
		{
			if (!m_ltWidget.ChildWidgets[i].Widget.LayoutInfo.IsKeepWithNext)
			{
				m_ltWidget.Widget.LayoutInfo.IsKeepWithNext = false;
				break;
			}
		}
	}

	private void UpdateAbsoluteTablePosition()
	{
		float num = 0f;
		float num2 = 0f;
		MarginsF marginsF = InitializePageMargins();
		float num3 = 0f;
		if (marginsF != null)
		{
			Entity baseEntity = GetBaseEntity(m_table);
			if (baseEntity is HeaderFooter)
			{
				num3 = (baseEntity.Owner as WSection).PageSetup.PageSize.Height;
			}
			else
			{
				while (!(baseEntity is WSection))
				{
					baseEntity = GetBaseEntity(baseEntity);
				}
				float num4 = (m_table.Document.DOP.GutterAtTop ? (baseEntity as WSection).PageSetup.Margins.Gutter : 0f);
				num3 = (m_lcOperator as Layouter).ClientLayoutArea.Height + marginsF.Top + num4 + marginsF.Bottom;
			}
		}
		if (m_ltWidget.Bounds.Height < (m_lcOperator as Layouter).ClientLayoutArea.Height && m_table.TableFormat.Positioning.VertRelationTo != VerticalRelation.Page)
		{
			if (m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Bottom && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Margin)
			{
				num2 = (m_lcOperator as Layouter).ClientLayoutArea.Bottom - m_ltWidget.Bounds.Height;
				num2 -= m_ltWidget.Bounds.Y;
			}
			else if (m_table.TableFormat.Positioning.VertPositionAbs != VerticalPosition.Center)
			{
				num2 = (((m_lcOperator as Layouter).CurrPageIndex % 2 != 0) ? ((m_lcOperator as Layouter).ClientLayoutArea.Height - m_ltWidget.Bounds.Height) : ((m_lcOperator as Layouter).ClientLayoutArea.Y - m_ltWidget.Bounds.Y));
			}
			else
			{
				num2 = (m_lcOperator as Layouter).ClientLayoutArea.Height / 2f - m_ltWidget.Bounds.Height / 2f + (m_lcOperator as Layouter).ClientLayoutArea.Y;
				num2 -= m_ltWidget.Bounds.Y;
				num2 *= 2f;
			}
		}
		else if (m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Outside && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Page)
		{
			num2 = (((m_lcOperator as Layouter).CurrPageIndex % 2 != 0) ? (num3 - m_ltWidget.Bounds.Height - (m_lcOperator as Layouter).CurrentSection.PageSetup.FooterDistance / 2f) : ((m_lcOperator as Layouter).CurrentSection.PageSetup.HeaderDistance / 2f - m_ltWidget.Bounds.Y));
		}
		else if (m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Inside && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Page)
		{
			num2 = (((m_lcOperator as Layouter).CurrPageIndex % 2 == 0) ? (num3 - m_ltWidget.Bounds.Height - (m_lcOperator as Layouter).CurrentSection.PageSetup.FooterDistance / 2f) : ((m_lcOperator as Layouter).CurrentSection.PageSetup.HeaderDistance / 2f - m_ltWidget.Bounds.Y));
		}
		else if (m_ltWidget.Bounds.Height < num3)
		{
			num2 = num3 - m_ltWidget.Bounds.Height;
		}
		if (m_table.OwnerTextBody != null && m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Bottom && (m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Page || m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Margin))
		{
			float num5 = ((m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Margin) ? (m_lcOperator as Layouter).CurrentSection.PageSetup.Margins.Bottom : 0f);
			if (((m_table.Document.ActualFormatType == FormatType.Doc && m_table.Document.WordVersion <= 257) || m_table.Document.DOP.Dop2000.Copts.DontBreakWrappedTables) && m_ltWidget.Bounds.Height > TableClientActiveArea.Height)
			{
				m_ltState = LayoutState.NotFitted;
				return;
			}
			num2 = (m_lcOperator as Layouter).CurrentSection.PageSetup.PageSize.Height - m_ltWidget.Bounds.Height - num5;
		}
		if (!m_table.IsInCell && num2 != 0f)
		{
			if (m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Bottom || m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Outside || m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Inside)
			{
				m_ltWidget.ShiftLocation(num, num2, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
			}
			else if (m_table.TableFormat.Positioning.VertPositionAbs == VerticalPosition.Center)
			{
				m_ltWidget.ShiftLocation(num, num2 / 2f, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false);
			}
		}
	}

	private MarginsF InitializePageMargins()
	{
		IWSection iWSection = null;
		MarginsF result = null;
		IEntity owner = m_table.Owner;
		if (owner != null)
		{
			while (owner.EntityType != EntityType.Section && owner.Owner != null)
			{
				owner = owner.Owner;
			}
			if (owner.EntityType == EntityType.Section)
			{
				iWSection = owner as WSection;
				if (iWSection != null)
				{
					result = iWSection.PageSetup.Margins;
				}
			}
		}
		return result;
	}

	private void AdjustClientAreaBasedOnTextWrap(SizeF size, ref RectangleF rect, float rowHeight)
	{
		float firstRowWidth = GetFirstRowWidth();
		Entity baseEntity = GetBaseEntity(m_table);
		if (((m_lcOperator as Layouter).IsLayoutingHeaderFooter && m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013) || baseEntity is WTextBox || baseEntity is Shape || baseEntity is GroupShape)
		{
			return;
		}
		RectangleF clientLayoutArea = (m_lcOperator as Layouter).ClientLayoutArea;
		int floattingItemIndex = GetFloattingItemIndex(baseEntity);
		FloatingItem.SortFloatingItems((m_lcOperator as Layouter).FloatingItems, SortPosition.Y, isNeedToUpdateWrapCollectionIndex: true);
		for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
		{
			RectangleF textWrappingBounds = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds;
			TextWrappingStyle textWrappingStyle = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingStyle;
			bool allowOverlap = (m_lcOperator as Layouter).FloatingItems[i].AllowOverlap;
			if (IsAdjustTightAndThroughBounds(textWrappingStyle, i))
			{
				textWrappingBounds = AdjustTightAndThroughBounds((m_lcOperator as Layouter).FloatingItems[i], rect, size.Height);
			}
			WTextBody ownerBody = null;
			if ((!IsInSameTextBody(m_table, (m_lcOperator as Layouter).FloatingItems[i], ref ownerBody) && m_table.IsInCell) || (IsInFrame((m_lcOperator as Layouter).FloatingItems[i].FloatingEntity as WParagraph) && m_table.IsFrame))
			{
				continue;
			}
			textWrappingBounds = AdjustTextWrappingBounds((m_lcOperator as Layouter).FloatingItems[i], clientLayoutArea, size, floattingItemIndex, i, rect, textWrappingBounds, textWrappingStyle, allowOverlap);
			float remainingHeightOfFloatingItem = 0f;
			if (clientLayoutArea.X > textWrappingBounds.Right + 16f || clientLayoutArea.Right < textWrappingBounds.X - 16f)
			{
				continue;
			}
			if (IsFloatingItemIntersect(floattingItemIndex, i, rect, textWrappingBounds, textWrappingStyle, allowOverlap, size))
			{
				if (rect.X >= textWrappingBounds.X && rect.X < textWrappingBounds.Right)
				{
					rect.Width -= textWrappingBounds.Right - rect.X;
					if (rect.Width < 16f || (rect.Width < firstRowWidth && firstRowWidth > 0f))
					{
						rect.Width = m_layoutArea.ClientActiveArea.Right - textWrappingBounds.Right;
						if (rect.Width < 16f || (rect.Width < firstRowWidth && firstRowWidth > 0f && clientLayoutArea.Right <= firstRowWidth + textWrappingBounds.Right))
						{
							if (IsNeedToUpdateTableYPosition(textWrappingBounds, rect, ref remainingHeightOfFloatingItem, rowHeight))
							{
								rect.Y = textWrappingBounds.Bottom;
								rect.Height -= remainingHeightOfFloatingItem;
							}
							rect.Width = m_layoutArea.ClientArea.Width;
							CreateLayoutArea(rect);
							continue;
						}
						rect.X = textWrappingBounds.Right;
						if (textWrappingStyle == TextWrappingStyle.Through && (m_lcOperator as Layouter).FloatingItems[i].IsDoesNotDenotesRectangle)
						{
							textWrappingBounds = AdjustTightAndThroughBounds((m_lcOperator as Layouter).FloatingItems[i], rect, size.Height);
							if (textWrappingBounds.X != 0f)
							{
								rect.Width = textWrappingBounds.X - rect.X;
							}
						}
						CreateLayoutArea(rect);
						continue;
					}
					rect.X = textWrappingBounds.Right;
					if (textWrappingStyle == TextWrappingStyle.Through && (m_lcOperator as Layouter).FloatingItems[i].IsDoesNotDenotesRectangle)
					{
						textWrappingBounds = AdjustTightAndThroughBounds((m_lcOperator as Layouter).FloatingItems[i], rect, size.Height);
						if (textWrappingBounds.X != 0f)
						{
							rect.Width = textWrappingBounds.X - rect.X;
						}
					}
					CreateLayoutArea(rect);
				}
				else if (rect.Right - textWrappingBounds.Right > 0f && rect.Right - textWrappingBounds.Right < rect.Width && (rect.Y >= textWrappingBounds.Y || rect.Y + size.Height >= textWrappingBounds.Y))
				{
					if (rect.X < textWrappingBounds.X && rect.Right > textWrappingBounds.X)
					{
						if (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left)
						{
							rect.X += m_table.TableFormat.Positioning.DistanceFromLeft;
						}
						else if (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Right)
						{
							rect.X -= m_table.TableFormat.Positioning.DistanceFromRight;
						}
					}
					if (IsNeedToUpdateTableYPosition(textWrappingBounds, rect, ref remainingHeightOfFloatingItem, rowHeight))
					{
						rect.Y = textWrappingBounds.Bottom;
						rect.Height -= remainingHeightOfFloatingItem;
					}
					CreateLayoutArea(rect);
				}
				else if (textWrappingBounds.X > rect.X && rect.Right > textWrappingBounds.X)
				{
					rect.Width = textWrappingBounds.X - rect.X;
					if (rect.Width < 16f || (rect.Width < firstRowWidth && firstRowWidth > 0f))
					{
						rect.Width = m_layoutArea.ClientActiveArea.Right - textWrappingBounds.Right;
						if (!(rect.Width < 16f) && (!(rect.Width < firstRowWidth) || !(firstRowWidth > 0f)))
						{
							continue;
						}
						if (m_layoutArea.ClientArea.Right < (m_lcOperator as Layouter).ClientLayoutArea.Right && textWrappingBounds.Right < (m_lcOperator as Layouter).ClientLayoutArea.Right && (m_lcOperator as Layouter).ClientLayoutArea.Right - textWrappingBounds.Right > 16f && Math.Round(m_layoutArea.ClientActiveArea.Width, 2) > Math.Round(firstRowWidth, 2))
						{
							rect.Width = (m_lcOperator as Layouter).ClientLayoutArea.Right - textWrappingBounds.Right;
							rect.X = textWrappingBounds.Right;
							if (textWrappingStyle == TextWrappingStyle.Through && (m_lcOperator as Layouter).FloatingItems[i].IsDoesNotDenotesRectangle)
							{
								textWrappingBounds = AdjustTightAndThroughBounds((m_lcOperator as Layouter).FloatingItems[i], rect, size.Height);
								if (textWrappingBounds.X != 0f)
								{
									rect.Width = textWrappingBounds.X - rect.X;
								}
							}
						}
						else if (IsNeedToUpdateTableYPosition(textWrappingBounds, rect, ref remainingHeightOfFloatingItem, rowHeight))
						{
							rect.Y = textWrappingBounds.Bottom;
							rect.Height -= remainingHeightOfFloatingItem;
						}
						CreateLayoutArea(rect);
					}
					else
					{
						CreateLayoutArea(rect);
					}
				}
				else if (rect.X > textWrappingBounds.X && rect.X > textWrappingBounds.Right)
				{
					rect.Width = m_layoutArea.ClientArea.Width;
					CreateLayoutArea(rect);
				}
				else
				{
					if (!(rect.X > textWrappingBounds.X) || !(rect.X < textWrappingBounds.Right))
					{
						continue;
					}
					rect.Width -= textWrappingBounds.Right - rect.X;
					rect.X = textWrappingBounds.Right;
					if (textWrappingStyle == TextWrappingStyle.Through && (m_lcOperator as Layouter).FloatingItems[i].IsDoesNotDenotesRectangle)
					{
						textWrappingBounds = AdjustTightAndThroughBounds((m_lcOperator as Layouter).FloatingItems[i], rect, size.Height);
						if (textWrappingBounds.X != 0f)
						{
							rect.Width = textWrappingBounds.X - rect.X;
						}
					}
					CreateLayoutArea(rect);
				}
			}
			else if (IsFloatingItemIntersectForTopAndBottom(floattingItemIndex, i, rect, textWrappingBounds, textWrappingStyle, allowOverlap, size) && ((m_lcOperator as Layouter).IsLayoutingTableHeight || !m_table.TableFormat.WrapTextAround || !(GetHeightofTable() + rect.Y > textWrappingBounds.Bottom) || (!((m_table.GetOwnerSection(m_table) as WSection).PageSetup.PageSize.Width - textWrappingBounds.Right > firstRowWidth) && !((m_table.GetOwnerSection(m_table) as WSection).PageSetup.PageSize.Width - ((m_table.GetOwnerSection(m_table) as WSection).PageSetup.PageSize.Width - textWrappingBounds.X) > firstRowWidth))) && IsNeedToUpdateTableYPosition(textWrappingBounds, rect, ref remainingHeightOfFloatingItem, rowHeight))
			{
				rect.Y = textWrappingBounds.Bottom;
				rect.Height -= textWrappingBounds.Height;
				CreateLayoutArea(rect);
			}
		}
	}

	private float GetHeightofTable()
	{
		WTable table = m_table;
		(m_lcOperator as Layouter).IsLayoutingTableHeight = true;
		LayoutContext layoutContext = LayoutContext.Create(table, m_lcOperator, base.IsForceFitLayout);
		RectangleF rect = new RectangleF(m_layoutArea.ClientActiveArea.X, m_layoutArea.ClientActiveArea.Y, m_layoutArea.ClientActiveArea.Width, m_layoutArea.ClientActiveArea.Height);
		LayoutedWidget layoutedWidget = layoutContext.Layout(rect);
		(m_lcOperator as Layouter).IsLayoutingTableHeight = false;
		return layoutedWidget.Bounds.Height;
	}

	internal RectangleF AdjustTextWrappingBounds(FloatingItem floatingItem, RectangleF clientLayoutArea, SizeF size, int wrapItemIndex, int i, RectangleF rect, RectangleF textWrappingBounds, TextWrappingStyle textWrappingStyle, bool allowOverlap)
	{
		float distanceLeft = 0f;
		float distanceRight = 0f;
		float distanceTop = 0f;
		float distanceBottom = 0f;
		bool flag = ((floatingItem.FloatingEntity.EntityType != EntityType.Table) ? (floatingItem.TextWrappingStyle != 0 && floatingItem.TextWrappingStyle != TextWrappingStyle.InFrontOfText && floatingItem.TextWrappingStyle != TextWrappingStyle.Behind) : ((floatingItem.FloatingEntity as WTable).TableFormat != null && (floatingItem.FloatingEntity as WTable).TableFormat.Positioning.AllowOverlap && m_table.TableFormat.Positioning.AllowOverlap));
		if (m_table.TableFormat != null && m_table.TableFormat.WrapTextAround && flag && !IsAdjustTightAndThroughBounds(textWrappingStyle, i) && !(clientLayoutArea.X > rect.Right + 16f) && !(clientLayoutArea.Right < rect.X - 16f) && (IsFloatingItemIntersect(wrapItemIndex, i, rect, textWrappingBounds, textWrappingStyle, allowOverlap, size) || IsFloatingItemIntersectForTopAndBottom(wrapItemIndex, i, rect, textWrappingBounds, textWrappingStyle, allowOverlap, size)))
		{
			switch (floatingItem.FloatingEntity.EntityType)
			{
			case EntityType.Chart:
			case EntityType.AutoShape:
			case EntityType.GroupShape:
			{
				ShapeBase shapeBase = ((floatingItem.FloatingEntity is Shape) ? (floatingItem.FloatingEntity as Shape) : ((!(floatingItem.FloatingEntity is GroupShape)) ? ((ShapeBase)(floatingItem.FloatingEntity as WChart)) : ((ShapeBase)(floatingItem.FloatingEntity as GroupShape))));
				distanceLeft = shapeBase.WrapFormat.DistanceLeft;
				distanceTop = shapeBase.WrapFormat.DistanceTop;
				distanceRight = shapeBase.WrapFormat.DistanceRight;
				distanceBottom = shapeBase.WrapFormat.DistanceBottom;
				break;
			}
			case EntityType.TextBox:
			{
				WTextBox obj3 = floatingItem.FloatingEntity as WTextBox;
				distanceLeft = obj3.TextBoxFormat.WrapDistanceLeft;
				distanceTop = obj3.TextBoxFormat.WrapDistanceTop;
				distanceRight = obj3.TextBoxFormat.WrapDistanceRight;
				distanceBottom = obj3.TextBoxFormat.WrapDistanceBottom;
				break;
			}
			case EntityType.Picture:
			{
				WPicture obj2 = floatingItem.FloatingEntity as WPicture;
				distanceLeft = obj2.DistanceFromLeft;
				distanceTop = obj2.DistanceFromTop;
				distanceRight = obj2.DistanceFromRight;
				distanceBottom = obj2.DistanceFromBottom;
				break;
			}
			case EntityType.Table:
			{
				WTable obj = floatingItem.FloatingEntity as WTable;
				distanceLeft = obj.TableFormat.Positioning.DistanceFromLeft;
				distanceTop = obj.TableFormat.Positioning.DistanceFromTop;
				distanceRight = obj.TableFormat.Positioning.DistanceFromRight;
				distanceBottom = obj.TableFormat.Positioning.DistanceFromBottom;
				break;
			}
			}
		}
		return AdjustWrappingBounds(textWrappingBounds, distanceLeft, distanceRight, distanceTop, distanceBottom);
	}

	private RectangleF AdjustWrappingBounds(RectangleF textWrappingBounds, float DistanceLeft, float DistanceRight, float DistanceTop, float DistanceBottom)
	{
		textWrappingBounds.X += DistanceLeft;
		textWrappingBounds.Y += DistanceTop;
		textWrappingBounds.Width -= DistanceRight + DistanceLeft;
		textWrappingBounds.Height -= DistanceBottom + DistanceTop;
		return textWrappingBounds;
	}

	private bool IsFloatingItemIntersect(int wrapItemIndex, int i, RectangleF rect, RectangleF textWrappingBounds, TextWrappingStyle textWrappingStyle, bool allowOverlap, SizeF size)
	{
		if ((m_lcOperator as Layouter).FloatingItems.Count > 0 && wrapItemIndex != (m_lcOperator as Layouter).FloatingItems[i].WrapCollectionIndex && ((Math.Round(rect.Y + size.Height, 2) >= Math.Round(textWrappingBounds.Y, 2) && Math.Round(rect.Y, 2) < Math.Round(textWrappingBounds.Bottom, 2)) || (Math.Round(rect.Y + size.Height, 2) <= Math.Round(textWrappingBounds.Bottom, 2) && Math.Round(rect.Y + size.Height, 2) >= Math.Round(textWrappingBounds.Y, 2))) && textWrappingStyle != 0 && textWrappingStyle != TextWrappingStyle.TopAndBottom && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind)
		{
			if (allowOverlap)
			{
				if (m_table.TableFormat != null && m_table.TableFormat.WrapTextAround)
				{
					return !m_table.TableFormat.Positioning.AllowOverlap;
				}
				return true;
			}
			return true;
		}
		return false;
	}

	private bool IsNeedToUpdateTableYPosition(RectangleF textWrappingBounds, RectangleF rect, ref float remainingHeightOfFloatingItem, float rowHeight)
	{
		remainingHeightOfFloatingItem = ((textWrappingBounds.Bottom > rect.Y) ? (textWrappingBounds.Bottom - rect.Y) : 0f);
		if (m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 || DocumentLayouter.IsLayoutingHeaderFooter || m_table.TableFormat == null || m_table.TableFormat.WrapTextAround)
		{
			return true;
		}
		return rect.Height - remainingHeightOfFloatingItem > 0f;
	}

	private bool IsFloatingItemIntersectForTopAndBottom(int wrapItemIndex, int i, RectangleF rect, RectangleF textWrappingBounds, TextWrappingStyle textWrappingStyle, bool allowOverlap, SizeF size)
	{
		if ((m_lcOperator as Layouter).FloatingItems.Count > 0 && wrapItemIndex != (m_lcOperator as Layouter).FloatingItems[i].WrapCollectionIndex && ((rect.Y >= textWrappingBounds.Y && rect.Y < textWrappingBounds.Bottom) || (rect.Y + GetMaxCellHeight(size.Height) >= textWrappingBounds.Y && rect.Y + size.Height < textWrappingBounds.Bottom)) && textWrappingStyle == TextWrappingStyle.TopAndBottom)
		{
			if (allowOverlap)
			{
				if (m_table.TableFormat != null && m_table.TableFormat.WrapTextAround)
				{
					return !m_table.TableFormat.Positioning.AllowOverlap;
				}
				return true;
			}
			return true;
		}
		return false;
	}

	private bool IsAdjustTightAndThroughBounds(TextWrappingStyle textWrappingStyle, int i)
	{
		if (textWrappingStyle == TextWrappingStyle.Tight || textWrappingStyle == TextWrappingStyle.Through)
		{
			return (m_lcOperator as Layouter).FloatingItems[i].IsDoesNotDenotesRectangle;
		}
		return false;
	}

	private bool IsTableMoveToNextPage(TextBodyItem widget)
	{
		if (widget.PreviousSibling is WTable && !(widget.PreviousSibling as WTable).TableFormat.WrapTextAround)
		{
			return true;
		}
		if (widget.PreviousSibling is BlockContentControl && (widget.PreviousSibling as BlockContentControl).LastChildEntity is WTable && !((widget.PreviousSibling as BlockContentControl).LastChildEntity as WTable).TableFormat.WrapTextAround)
		{
			return true;
		}
		if (widget.PreviousSibling == null && widget.Owner.Owner is BlockContentControl)
		{
			return IsTableMoveToNextPage(widget.Owner.Owner as TextBodyItem);
		}
		if (widget.PreviousSibling is WParagraph)
		{
			return true;
		}
		return false;
	}

	private float GetFirstRowWidth()
	{
		float num = 0f;
		int num2 = ((CurrRowIndex != -1) ? CurrRowIndex : 0);
		if (m_table.Rows.Count > num2)
		{
			for (int i = 0; i < m_table.Rows[num2].Cells.Count; i++)
			{
				num += GetCellWidth(num2, i);
			}
		}
		return num;
	}

	private bool IsRowNotFittedBasedOnFloatingItem()
	{
		RectangleF clientActiveArea = m_layoutArea.ClientActiveArea;
		RectangleF clientLayoutArea = (m_lcOperator as Layouter).ClientLayoutArea;
		SizeF size = new SizeF(m_currRowLW.Bounds.Width, m_currRowLW.Bounds.Height);
		Entity baseEntity = GetBaseEntity(m_table);
		Entity parentBaseEntity = GetParentBaseEntity(m_table);
		int floattingItemIndex = GetFloattingItemIndex(baseEntity);
		FloatingItem.SortFloatingItems((m_lcOperator as Layouter).FloatingItems, SortPosition.Y, isNeedToUpdateWrapCollectionIndex: true);
		for (int i = 0; i < (m_lcOperator as Layouter).FloatingItems.Count; i++)
		{
			WParagraph ownerParagraph = (m_lcOperator as Layouter).FloatingItems[i].OwnerParagraph;
			Entity entity = null;
			Entity entity2 = null;
			if (ownerParagraph != null)
			{
				entity = GetBaseEntity(ownerParagraph);
				entity2 = GetParentBaseEntity(ownerParagraph);
			}
			WTextBody ownerBody = null;
			RectangleF textWrappingBounds = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingBounds;
			TextWrappingStyle textWrappingStyle = (m_lcOperator as Layouter).FloatingItems[i].TextWrappingStyle;
			bool allowOverlap = (m_lcOperator as Layouter).FloatingItems[i].AllowOverlap;
			textWrappingBounds = AdjustTextWrappingBounds((m_lcOperator as Layouter).FloatingItems[i], clientLayoutArea, size, floattingItemIndex, i, clientActiveArea, textWrappingBounds, textWrappingStyle, allowOverlap);
			if (((IsInSameTextBody(m_table, (m_lcOperator as Layouter).FloatingItems[i], ref ownerBody) && parentBaseEntity is WSection && entity2 is WSection) || (entity is HeaderFooter && parentBaseEntity is WSection)) && m_currRowLW.Bounds.X < textWrappingBounds.X + textWrappingBounds.Width && textWrappingBounds.X < m_currRowLW.Bounds.X + m_currRowLW.Bounds.Width && (!(m_currRowLW.Bounds.Y >= textWrappingBounds.Y) || !(m_currRowLW.Bounds.Y <= textWrappingBounds.Bottom)) && IsFloatingItemIntersect(floattingItemIndex, i, m_currRowLW.Bounds, textWrappingBounds, textWrappingStyle, allowOverlap, size))
			{
				return true;
			}
		}
		return false;
	}

	private float GetMaxCellHeight(float cellMinHeight)
	{
		float num = 0f;
		for (int i = 0; i < m_table.Rows[0].Cells.Count; i++)
		{
			num = Math.Max(num, GetCellHeight(0, i, cellMinHeight));
		}
		return num;
	}

	private IWidget CreateRowLayoutedWidget()
	{
		if (CurrRowIndex + 1 < TableWidget.RowsCount)
		{
			m_currColIndex = -1;
			NextRowIndex();
			while (CurrRowIndex < TableWidget.RowsCount && (TableWidget as WTable).IsHiddenRow(CurrRowIndex, TableWidget as WTable))
			{
				m_currRowIndex++;
				if (CurrRowIndex == TableWidget.RowsCount)
				{
					m_currRowIndex--;
					return null;
				}
			}
			IWidget rowWidget = TableWidget.GetRowWidget(CurrRowIndex);
			m_currRowLW = new LayoutedWidget(rowWidget);
			m_rowLayoutArea = new LayoutArea(m_layoutArea.ClientActiveArea, rowWidget.LayoutInfo as ILayoutSpacingsInfo, rowWidget);
			m_currRowLW.Bounds = new RectangleF(m_rowLayoutArea.ClientActiveArea.Location, default(SizeF));
			if ((rowWidget.LayoutInfo as RowLayoutInfo).IsRowBreakByPageBreakBefore)
			{
				(rowWidget.LayoutInfo as RowLayoutInfo).IsRowBreakByPageBreakBefore = false;
			}
			if (rowWidget.LayoutInfo.IsSkip)
			{
				m_bAtLastOneCellFitted = true;
			}
			return rowWidget;
		}
		return null;
	}

	private void InitCellSpacing(WTableRow row)
	{
		for (int i = 0; i < row.Cells.Count; i++)
		{
			WTableCell wTableCell = row.Cells[i];
			if (wTableCell.m_layoutInfo != null)
			{
				(wTableCell.m_layoutInfo as CellLayoutInfo).InitSpacings();
			}
		}
	}

	private void LayoutRow(IWidget rowWidget)
	{
		bool isFitRowByUsingVerticalDistance = false;
		LayoutRow(rowWidget, ref isFitRowByUsingVerticalDistance);
	}

	private void LayoutRow(IWidget rowWidget, ref bool isFitRowByUsingVerticalDistance)
	{
		RowLayoutInfo rowLayoutInfo = rowWidget.LayoutInfo as RowLayoutInfo;
		float maxTopPading = 0f;
		float maxBottomPadding = 0f;
		float maxTopMargin = 0f;
		float maxBottomMargin = 0f;
		GetCellsMaxTopAndBottomPadding(rowWidget as WTableRow, out maxTopPading, out maxBottomPadding, out maxTopMargin, out maxBottomMargin);
		if (!((m_currRowLW.Widget as WTableRow).RowFormat.CellSpacing > 0f) && !(m_table.TableFormat.CellSpacing > 0f))
		{
			if (!(m_lcOperator as Layouter).IsLayoutingHeaderRow && !rowLayoutInfo.IsCellPaddingUpdated)
			{
				UpdateCellsMaxTopAndBottomPadding(rowWidget as WTableRow, maxTopPading, maxBottomPadding);
			}
			m_rowLayoutArea = CreateRowLayoutArea(rowWidget as WTableRow, maxBottomPadding);
		}
		else
		{
			float bottomPad = UpdateCellsBottomPaddingAndMargin(rowWidget as WTableRow, maxBottomPadding, maxBottomMargin);
			m_rowLayoutArea = CreateRowLayoutArea(rowWidget as WTableRow, bottomPad);
		}
		Entity baseEntity = GetBaseEntity(m_table);
		bool flag = true;
		if (!(baseEntity is WTextBox) && !(baseEntity is Shape) && !m_table.TableFormat.WrapTextAround && !IsInFrame() && m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
		{
			foreach (FloatingItem floatingItem in ((Layouter)m_lcOperator).FloatingItems)
			{
				if (((!(floatingItem.FloatingEntity is WTable wTable) || wTable == m_table || m_table.IsInCell) && !IsNeedToMoveRow(floatingItem)) || !floatingItem.TextWrappingBounds.IntersectsWith(m_currRowLW.Bounds) || !((m_lcOperator as Layouter).ClientLayoutArea.Bottom >= floatingItem.TextWrappingBounds.Y))
				{
					continue;
				}
				m_currRowLW.Bounds = new RectangleF(m_currRowLW.Bounds.X, floatingItem.TextWrappingBounds.Bottom, m_currRowLW.Bounds.Width, m_currRowLW.Bounds.Height);
				flag = false;
				if (IsNeedToMoveRow(floatingItem))
				{
					RectangleF clientActiveArea = m_layoutArea.ClientActiveArea;
					clientActiveArea.Y = floatingItem.TextWrappingBounds.Bottom;
					m_layoutArea = (m_rowLayoutArea = new LayoutArea(clientActiveArea));
					if (m_currRowIndex != 0)
					{
						(((IWidget)(m_table.ChildEntities[m_currRowIndex - 1] as WTableRow)).LayoutInfo as RowLayoutInfo).IsRowSplittedByFloatingItem = true;
					}
				}
				break;
			}
		}
		if (rowLayoutInfo.IsExactlyRowHeight || (rowWidget as WTableRow).HeightType == TableRowHeightType.AtLeast || rowLayoutInfo.IsVerticalText)
		{
			if ((double)m_currRowLW.Bounds.Bottom > Math.Round(m_layoutArea.ClientActiveArea.Bottom, 1) && !IsInExactlyRow(m_table) && !(baseEntity is WTextBox) && !(baseEntity is Shape) && !(baseEntity is GroupShape) && (m_table.TableFormat.WrapTextAround || !IsInFrame()))
			{
				bool flag2 = false;
				if (m_table.IsInCell && m_table.GetOwnerTableCell().ChildEntities[0] is WTable && m_currRowIndex == 0)
				{
					flag2 = true;
				}
				bool flag3 = false;
				if (m_table.TableFormat.WrapTextAround && (m_currRowLW.Widget as WTableRow).Index == 0 && !m_table.IsInCell && m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
				{
					flag3 = base.IsForceFitLayout;
				}
				if (!flag3 && !base.IsForceFitLayout && (!flag2 || m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013))
				{
					if (!IsFitRowByUsingVerticalDistance())
					{
						m_ltState = LayoutState.NotFitted;
						if (flag)
						{
							CommitKeepWithNext();
						}
						return;
					}
					m_currRowLW.Bounds = new RectangleF(m_currRowLW.Bounds.Location, new SizeF(m_currRowLW.Bounds.Width, m_currRowLW.Bounds.Height - rowLayoutInfo.Margins.Bottom - rowLayoutInfo.Paddings.Bottom));
					isFitRowByUsingVerticalDistance = true;
				}
				else if ((rowWidget as WTableRow).Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
				{
					m_currRowLW.Bounds = new RectangleF(m_currRowLW.Bounds.Location, new SizeF(m_currRowLW.Bounds.Width, m_layoutArea.ClientArea.Height - m_headerRowHeight));
				}
				else if ((rowWidget as WTableRow).Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
				{
					if ((rowWidget as WTableRow).HeightType == TableRowHeightType.Exactly)
					{
						m_currRowLW.Bounds = new RectangleF(m_currRowLW.Bounds.Location, new SizeF(m_currRowLW.Bounds.Width, m_currRowLW.Bounds.Height));
					}
					else
					{
						m_currRowLW.Bounds = new RectangleF(m_currRowLW.Bounds.Location, new SizeF(m_currRowLW.Bounds.Width, m_layoutArea.ClientArea.Height));
					}
				}
			}
			else
			{
				m_currRowLW.Bounds = new RectangleF(m_currRowLW.Bounds.Location, new SizeF(m_currRowLW.Bounds.Width, m_currRowLW.Bounds.Height - rowLayoutInfo.Margins.Bottom - rowLayoutInfo.Paddings.Bottom));
			}
		}
		if (!rowLayoutInfo.IsRowBreakByPageBreakBefore && m_ltState != LayoutState.NotFitted && !base.IsForceFitLayout && !(rowWidget as WTableRow).OwnerTable.IsInCell && IsRowSplitByPageBreakBefore(rowWidget as WTableRow))
		{
			m_ltState = LayoutState.NotFitted;
			rowLayoutInfo.IsRowBreakByPageBreakBefore = true;
			return;
		}
		m_splitedCells = new SplitWidgetContainer[m_table.Rows[CurrRowIndex].Cells.Count];
		if (m_table != null && m_table.TableFormat.Bidi)
		{
			m_currColIndex = m_table.Rows[CurrRowIndex].Cells.Count;
		}
		else
		{
			m_currColIndex = -1;
		}
		do
		{
			LayoutContext layoutContext = CreateNextCellContext();
			LayoutArea layoutArea = null;
			if (layoutContext == null)
			{
				break;
			}
			CellLayoutInfo cellLayoutInfo = layoutContext.Widget.LayoutInfo as CellLayoutInfo;
			layoutArea = GetCellClientArea(cellLayoutInfo, CurrRowIndex, m_currColIndex, maxTopPading, maxTopMargin);
			layoutContext.ClientLayoutAreaRight = (float)layoutArea.Width;
			if (!rowLayoutInfo.IsRowHasVerticalTextCell && cellLayoutInfo.IsVerticalText)
			{
				rowLayoutInfo.IsRowHasVerticalTextCell = !rowLayoutInfo.IsVerticalText && cellLayoutInfo.IsVerticalText;
			}
			m_currCellLW = LayoutCell(layoutContext, layoutArea.ClientArea, cellLayoutInfo.IsRowMergeStart || cellLayoutInfo.IsRowMergeContinue || (!rowLayoutInfo.IsVerticalText && cellLayoutInfo.IsVerticalText));
			if (DocumentLayouter.IsEndUpdateTOC && DocumentLayouter.IsUpdatingTOC)
			{
				return;
			}
			if (m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
			{
				rowLayoutInfo.IsFootnoteSplitted = CheckFootnoteInRowIsSplitted(layoutContext);
			}
			if (isFitRowByUsingVerticalDistance && layoutContext.State == LayoutState.Splitted)
			{
				m_ltState = LayoutState.NotFitted;
				if (flag)
				{
					CommitKeepWithNext();
				}
				(m_lcOperator as Layouter).RemovedWidgetsHeight += m_layoutArea.ClientActiveArea.Height;
				return;
			}
			SaveChildContextState(layoutContext);
		}
		while (base.State == LayoutState.Unknown && !(m_lcOperator as Layouter).IsNeedToRelayoutTable);
		if (!base.IsForceFitLayout && (!(rowWidget as WTableRow).RowFormat.IsBreakAcrossPages || m_blastRowState != LayoutState.Splitted) && IsRowNotFittedBasedOnFloatingItem())
		{
			m_ltState = LayoutState.NotFitted;
		}
	}

	private bool IsInExactlyRow(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			if (entity2.Owner == null)
			{
				return false;
			}
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WTableCell) || !(((IWidget)(entity2 as WTableCell).OwnerRow).LayoutInfo as RowLayoutInfo).IsExactlyRowHeight);
		return true;
	}

	private bool IsFitRowByUsingVerticalDistance()
	{
		float num = (m_lcOperator as Layouter).ClientLayoutArea.Bottom - (m_currRowLW.Bounds.Y - m_table.TableFormat.Positioning.VertPosition);
		if (m_table.TableFormat.WrapTextAround && !IsWord2013(m_table.Document) && m_currRowIndex == 0 && m_table.TableFormat.Positioning.VertRelationTo == VerticalRelation.Paragraph && num > m_currRowLW.Bounds.Height && m_table.PreviousSibling is IWidget && !(m_table.PreviousSibling as IWidget).LayoutInfo.IsKeepWithNext)
		{
			RowLayoutInfo rowLayoutInfo = m_currRowLW.Widget.LayoutInfo as RowLayoutInfo;
			RectangleF clientActiveArea = m_layoutArea.ClientActiveArea;
			if (rowLayoutInfo != null && rowLayoutInfo.IsExactlyRowHeight)
			{
				clientActiveArea.Height = m_currRowLW.Bounds.Height;
			}
			else
			{
				clientActiveArea.Height = num;
			}
			m_layoutArea = (m_rowLayoutArea = new LayoutArea(clientActiveArea));
			return true;
		}
		return false;
	}

	private bool IsRowSplitByPageBreakBefore(WTableRow tableRow)
	{
		WTableCell wTableCell = tableRow.Cells[0];
		WParagraph wParagraph = null;
		WTableRow wTableRow = tableRow.PreviousSibling as WTableRow;
		if (!(m_lcOperator as Layouter).IsLayoutingHeaderFooter && wTableCell.ChildEntities.Count != 0 && wTableCell.ChildEntities[0] is WParagraph && wTableRow != null && (!wTableRow.IsKeepWithNext || (wTableRow.IsKeepWithNext && m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)) && (!tableRow.IsHeader || !wTableRow.IsHeader || m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013))
		{
			wParagraph = wTableCell.ChildEntities[0] as WParagraph;
		}
		else if (wTableCell.ChildEntities.Count != 0 && wTableCell.ChildEntities[0] is WTable)
		{
			return IsRowSplitByPageBreakBefore((wTableCell.ChildEntities[0] as WTable).FirstRow);
		}
		if (wParagraph != null && wParagraph.ParagraphFormat.PageBreakBefore)
		{
			return true;
		}
		return false;
	}

	private bool CheckFootnoteInRowIsSplitted(LayoutContext childContext)
	{
		bool result = false;
		if (childContext.SplittedWidget is SplitWidgetContainer && (childContext.SplittedWidget as SplitWidgetContainer).m_currentChild is SplitWidgetContainer)
		{
			SplitWidgetContainer splitWidgetContainer = (childContext.SplittedWidget as SplitWidgetContainer).m_currentChild as SplitWidgetContainer;
			WParagraph wParagraph = null;
			IEntity entity = null;
			if (splitWidgetContainer.m_currentChild is SplitStringWidget)
			{
				WTextRange wTextRange = (splitWidgetContainer.m_currentChild as SplitStringWidget).RealStringWidget as WTextRange;
				wParagraph = wTextRange.GetOwnerParagraphValue();
				if (wTextRange.Owner == null)
				{
					wParagraph = wTextRange.CharacterFormat.BaseFormat.OwnerBase as WParagraph;
				}
				entity = wTextRange;
			}
			else if (splitWidgetContainer.m_currentChild is IEntity)
			{
				entity = splitWidgetContainer.m_currentChild as IEntity;
				if (entity is ParagraphItem)
				{
					wParagraph = (entity as ParagraphItem).GetOwnerParagraphValue();
				}
				if (entity is WTextRange && entity.Owner == null)
				{
					wParagraph = (entity as WTextRange).CharacterFormat.BaseFormat.OwnerBase as WParagraph;
				}
			}
			int num = 0;
			if (wParagraph != null && entity != null)
			{
				num = ((IWidgetContainer)wParagraph).WidgetInnerCollection.InnerList.IndexOf(entity);
			}
			for (int i = num; i < splitWidgetContainer.WidgetInnerCollection.Count; i++)
			{
				if (splitWidgetContainer.WidgetInnerCollection[i] is WFootnote)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private LayoutArea CreateRowLayoutArea(WTableRow row, float bottomPad)
	{
		RowLayoutInfo rowLayoutInfo = m_currRowLW.Widget.LayoutInfo as RowLayoutInfo;
		isRowMoved = !rowLayoutInfo.IsRowSplitted;
		RectangleF clientActiveArea = m_layoutArea.ClientActiveArea;
		Spacings margins = (m_currRowLW.Widget.LayoutInfo as RowLayoutInfo).Margins;
		Spacings paddings = (m_currRowLW.Widget.LayoutInfo as RowLayoutInfo).Paddings;
		if ((row.RowFormat.BeforeWidth != 0f || row.RowFormat.GridBeforeWidth.Width != 0f) && (!row.Document.DOP.Dop2000.Copts.AlignTablesRowByRow || row.OwnerTable.TableFormat.HorizontalAlignment == RowAlignment.Left))
		{
			clientActiveArea.X += ((row.RowFormat.BeforeWidth != 0f) ? row.RowFormat.BeforeWidth : row.RowFormat.GridBeforeWidth.Width);
		}
		int rowIndex = row.GetRowIndex();
		float num = ((row.RowFormat.CellSpacing > 0f) ? row.RowFormat.CellSpacing : ((row.OwnerTable.TableFormat.CellSpacing > 0f) ? row.OwnerTable.TableFormat.CellSpacing : 0f));
		if (!m_table.TableFormat.Bidi && !(m_lcOperator as Layouter).IsLayoutingHeaderRow && row.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
		{
			paddings.Left = ((num > 0f) ? ((((IWidget)m_table.Rows[0].Cells[0]).LayoutInfo as CellLayoutInfo).Paddings.Left / 2f) : (((IWidget)m_table.Rows[0].Cells[0]).LayoutInfo as CellLayoutInfo).Paddings.Left);
		}
		else if (m_table.TableFormat.Bidi && !(m_lcOperator as Layouter).IsLayoutingHeaderRow && row.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
		{
			paddings.Left = ((num > 0f) ? ((((IWidget)m_table.Rows[0].Cells[m_table.Rows[0].Cells.Count - 1]).LayoutInfo as CellLayoutInfo).Paddings.Left / 2f) : (((IWidget)m_table.Rows[0].Cells[m_table.Rows[0].Cells.Count - 1]).LayoutInfo as CellLayoutInfo).Paddings.Left);
		}
		if (num > 0f && !(m_lcOperator as Layouter).IsLayoutingHeaderRow)
		{
			if (rowIndex == 0 && rowIndex == row.OwnerTable.Rows.Count - 1)
			{
				margins.Top = num * 2f;
				margins.Bottom = num * 2f;
				if (row.OwnerTable.TableFormat.Borders.Top.IsBorderDefined && row.OwnerTable.TableFormat.Borders.Top.BorderType != 0 && row.OwnerTable.TableFormat.Borders.Top.BorderType != BorderStyle.Cleared)
				{
					paddings.Top = row.OwnerTable.TableFormat.Borders.Top.GetLineWidthValue();
				}
				if (row.OwnerTable.TableFormat.Borders.Bottom.IsBorderDefined && row.OwnerTable.TableFormat.Borders.Bottom.BorderType != 0 && row.OwnerTable.TableFormat.Borders.Bottom.BorderType != BorderStyle.Cleared)
				{
					paddings.Bottom = row.OwnerTable.TableFormat.Borders.Bottom.GetLineWidthValue() + bottomPad;
				}
			}
			else if (rowIndex == 0)
			{
				margins.Top = num * 2f;
				if (row.OwnerTable.TableFormat.Borders.Top.IsBorderDefined && row.OwnerTable.TableFormat.Borders.Top.BorderType != 0 && row.OwnerTable.TableFormat.Borders.Top.BorderType != BorderStyle.Cleared)
				{
					paddings.Top = row.OwnerTable.TableFormat.Borders.Top.GetLineWidthValue();
				}
				margins.Bottom = num;
				paddings.Bottom = bottomPad;
			}
			else if (rowIndex == row.OwnerTable.Rows.Count - 1)
			{
				margins.Top = num;
				margins.Bottom = num * 2f;
				if (row.OwnerTable.TableFormat.Borders.Bottom.IsBorderDefined && row.OwnerTable.TableFormat.Borders.Bottom.BorderType != 0 && row.OwnerTable.TableFormat.Borders.Bottom.BorderType != BorderStyle.Cleared)
				{
					paddings.Bottom = row.OwnerTable.TableFormat.Borders.Bottom.GetLineWidthValue() + bottomPad;
				}
			}
			else
			{
				margins.Top = num;
				margins.Bottom = num;
				paddings.Bottom = bottomPad;
			}
			if (!m_table.TableFormat.Bidi && row.OwnerTable.TableFormat.Borders.Right.IsBorderDefined && row.OwnerTable.TableFormat.Borders.Right.BorderType != 0 && row.OwnerTable.TableFormat.Borders.Right.BorderType != BorderStyle.Cleared)
			{
				paddings.Right = row.OwnerTable.TableFormat.Borders.Right.GetLineWidthValue();
			}
			else if (m_table.TableFormat.Bidi && row.OwnerTable.TableFormat.Borders.Left.IsBorderDefined && row.OwnerTable.TableFormat.Borders.Left.BorderType != 0 && row.OwnerTable.TableFormat.Borders.Left.BorderType != BorderStyle.Cleared)
			{
				paddings.Right = row.OwnerTable.TableFormat.Borders.Left.GetLineWidthValue();
			}
			margins.Right = num * 2f;
		}
		else if (!(m_lcOperator as Layouter).IsLayoutingHeaderRow)
		{
			paddings.Bottom = bottomPad;
		}
		rowLayoutInfo.IsRowSplitted = false;
		rowLayoutInfo.IsRowHasVerticalMergeStartCell = false;
		float height = clientActiveArea.Height;
		LayoutArea layoutArea = new LayoutArea();
		if (rowLayoutInfo.IsVerticalText)
		{
			Spacings margins2 = (((IWidget)row.Cells[0]).LayoutInfo as CellLayoutInfo).Margins;
			Spacings paddings2 = (((IWidget)row.Cells[0]).LayoutInfo as CellLayoutInfo).Paddings;
			if (rowLayoutInfo.IsExactlyRowHeight)
			{
				height = (float)rowLayoutInfo.RowHeight + margins2.Bottom;
				layoutArea = new LayoutArea(new RectangleF(clientActiveArea.X + paddings.Left, clientActiveArea.Y + margins.Top + paddings.Top, clientActiveArea.Width - paddings.Right, height - (margins.Top + paddings.Top)));
				m_currRowLW.Bounds = new RectangleF(layoutArea.ClientArea.X, layoutArea.ClientArea.Y, layoutArea.ClientArea.Width, layoutArea.ClientArea.Height + margins.Bottom + paddings.Bottom);
			}
			else if (row.HeightType == TableRowHeightType.AtLeast)
			{
				height = (float)(rowLayoutInfo.RowHeight + (double)margins2.Top + (double)margins2.Bottom + (double)paddings2.Top);
				layoutArea = new LayoutArea(new RectangleF(clientActiveArea.X + paddings.Left, clientActiveArea.Y + margins.Top + paddings.Top, clientActiveArea.Width - paddings.Right, height));
				m_currRowLW.Bounds = layoutArea.ClientArea;
			}
			else
			{
				height = (row.Cells[0].LastParagraph as IWidget).LayoutInfo.Size.Height + margins2.Top + margins2.Bottom + paddings2.Top + margins.Bottom + paddings.Bottom;
				layoutArea = new LayoutArea(new RectangleF(clientActiveArea.X + paddings.Left, clientActiveArea.Y + margins.Top + paddings.Top, clientActiveArea.Width - paddings.Right, height));
				m_currRowLW.Bounds = layoutArea.ClientArea;
			}
		}
		else
		{
			if (rowLayoutInfo.IsExactlyRowHeight)
			{
				height = (float)rowLayoutInfo.RowHeight + (((IWidget)row.Cells[0]).LayoutInfo as CellLayoutInfo).Margins.Bottom;
				layoutArea = new LayoutArea(new RectangleF(clientActiveArea.X + paddings.Left, clientActiveArea.Y + margins.Top + paddings.Top, clientActiveArea.Width - paddings.Right, height - (margins.Top + paddings.Top)));
				m_currRowLW.Bounds = new RectangleF(layoutArea.ClientArea.X, layoutArea.ClientArea.Y, layoutArea.ClientArea.Width, layoutArea.ClientArea.Height + margins.Bottom + paddings.Bottom);
			}
			else
			{
				layoutArea = new LayoutArea(new RectangleF(clientActiveArea.X + paddings.Left, clientActiveArea.Y + margins.Top + paddings.Top, clientActiveArea.Width - paddings.Right, height - (margins.Top + margins.Bottom + paddings.Top + paddings.Bottom)));
			}
			if (row.HeightType == TableRowHeightType.AtLeast)
			{
				Spacings margins3 = (((IWidget)row.Cells[0]).LayoutInfo as CellLayoutInfo).Margins;
				Spacings paddings3 = (((IWidget)row.Cells[0]).LayoutInfo as CellLayoutInfo).Paddings;
				float height2 = (float)(rowLayoutInfo.RowHeight + (double)margins3.Top + (double)margins3.Bottom + (double)paddings3.Top + (double)margins.Bottom + (double)paddings.Bottom);
				m_currRowLW.Bounds = new RectangleF(layoutArea.ClientArea.X, layoutArea.ClientArea.Y, layoutArea.ClientArea.Width, height2);
			}
		}
		return layoutArea;
	}

	private void GetCellsMaxTopAndBottomPadding(WTableRow row, out float maxTopPading, out float maxBottomPadding, out float maxTopMargin, out float maxBottomMargin)
	{
		maxBottomPadding = 0f;
		maxTopPading = 0f;
		maxTopMargin = 0f;
		maxBottomMargin = 0f;
		RowLayoutInfo rowLayoutInfo = ((IWidget)row).LayoutInfo as RowLayoutInfo;
		for (int i = 0; i < row.Cells.Count; i++)
		{
			CellLayoutInfo cellLayoutInfo = ((IWidget)row.Cells[i]).LayoutInfo as CellLayoutInfo;
			if (!DocumentLayouter.IsFirstLayouting)
			{
				cellLayoutInfo.IsRowMergeStart = false;
				cellLayoutInfo.IsRowMergeContinue = false;
				cellLayoutInfo.IsRowMergeEnd = false;
				cellLayoutInfo.InitMerges();
			}
			if (!(m_lcOperator as Layouter).IsLayoutingHeaderRow && !row.IsHeader && (m_table.m_layoutInfo as TableLayoutInfo).IsSplittedTable && !(row.RowFormat.CellSpacing > 0f) && !(m_table.TableFormat.CellSpacing > 0f) && base.IsForceFitLayout && m_ltWidget.ChildWidgets.Count >= 1 && cellLayoutInfo.UpdatedSplittedTopBorders == null)
			{
				cellLayoutInfo.UpdatedSplittedTopBorders = new Dictionary<CellLayoutInfo.CellBorder, float>();
				cellLayoutInfo.GetTopHalfWidth(i, row.GetRowIndex(), row.Cells[i], m_ltWidget.ChildWidgets.Count - 1);
			}
			if (m_ltWidget.ChildWidgets.Count == 0 && !(row.RowFormat.CellSpacing > 0f) && !(m_table.TableFormat.CellSpacing > 0f) && m_currRowIndex != 0)
			{
				WTableCell wTableCell = row.Cells[i];
				if (cellLayoutInfo.IsRowMergeContinue)
				{
					WTableCell verticalMergeStartCell = wTableCell.GetVerticalMergeStartCell();
					if (verticalMergeStartCell != null)
					{
						wTableCell = verticalMergeStartCell;
						cellLayoutInfo = ((IWidget)verticalMergeStartCell).LayoutInfo as CellLayoutInfo;
					}
				}
				Border top = wTableCell.CellFormat.Borders.Top;
				if (!top.IsBorderDefined || (top.IsBorderDefined && top.BorderType == BorderStyle.None && top.LineWidth == 0f && top.Color.IsEmpty))
				{
					top = wTableCell.OwnerRow.RowFormat.Borders.Top;
				}
				if (!top.IsBorderDefined)
				{
					top = wTableCell.OwnerRow.OwnerTable.TableFormat.Borders.Top;
				}
				if (top.IsBorderDefined)
				{
					cellLayoutInfo.TopBorder = new CellLayoutInfo.CellBorder(top.BorderType, top.Color, top.GetLineWidthValue(), top.LineWidth);
					cellLayoutInfo.TopPadding = ((cellLayoutInfo.TopBorder.BorderType != 0 && cellLayoutInfo.TopBorder.BorderType != BorderStyle.Cleared) ? top.GetLineWidthValue() : 0f);
					cellLayoutInfo.SkipTopBorder = false;
					if (!(wTableCell.OwnerRow.m_layoutInfo as RowLayoutInfo).IsRowSplitted && !cellLayoutInfo.IsRowMergeStart && (cellLayoutInfo.TopBorder.BorderType == BorderStyle.Cleared || cellLayoutInfo.TopBorder.BorderType == BorderStyle.None))
					{
						cellLayoutInfo.SkipTopBorder = true;
					}
				}
			}
			float num = ((m_currRowIndex == 0 || row.RowFormat.CellSpacing > 0f || m_table.TableFormat.CellSpacing > 0f) ? cellLayoutInfo.TopPadding : (rowLayoutInfo.IsRowSplitted ? cellLayoutInfo.TopPadding : cellLayoutInfo.UpdatedTopPadding));
			if (maxTopPading < num)
			{
				maxTopPading = num;
			}
			if (maxBottomPadding < cellLayoutInfo.BottomPadding)
			{
				maxBottomPadding = cellLayoutInfo.BottomPadding;
			}
			if (maxTopMargin < cellLayoutInfo.Margins.Top)
			{
				maxTopMargin = cellLayoutInfo.Margins.Top;
			}
			if (maxBottomMargin < cellLayoutInfo.Margins.Bottom)
			{
				maxBottomMargin = cellLayoutInfo.Margins.Bottom;
			}
		}
	}

	private void UpdateCellsMaxTopAndBottomPadding(WTableRow row, float maxTopPading, float maxBottomPading)
	{
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < row.Cells.Count; i++)
		{
			CellLayoutInfo cellLayoutInfo = ((IWidget)row.Cells[i]).LayoutInfo as CellLayoutInfo;
			float num3 = ((cellLayoutInfo.Margins.Top + cellLayoutInfo.TopPadding < maxTopPading) ? 0f : (cellLayoutInfo.Margins.Top + cellLayoutInfo.TopPadding - maxTopPading));
			if (num < num3)
			{
				num = num3;
			}
			cellLayoutInfo.Paddings.Top = maxTopPading;
			num3 = ((cellLayoutInfo.Margins.Bottom + cellLayoutInfo.BottomPadding < maxBottomPading) ? 0f : (cellLayoutInfo.Margins.Bottom + cellLayoutInfo.BottomPadding - maxBottomPading));
			if (num2 < num3)
			{
				num2 = num3;
			}
		}
		for (int j = 0; j < row.Cells.Count; j++)
		{
			CellLayoutInfo obj = ((IWidget)row.Cells[j]).LayoutInfo as CellLayoutInfo;
			obj.Margins.Top = num;
			obj.Margins.Bottom = num2;
		}
		(((IWidget)row).LayoutInfo as RowLayoutInfo).IsCellPaddingUpdated = true;
	}

	private float UpdateCellsBottomPaddingAndMargin(WTableRow row, float maxBottomPading, float maxBottomMargin)
	{
		float num = 0f;
		for (int i = 0; i < row.Cells.Count; i++)
		{
			CellLayoutInfo cellLayoutInfo = ((IWidget)row.Cells[i]).LayoutInfo as CellLayoutInfo;
			float num2 = ((cellLayoutInfo.BottomPadding - (maxBottomMargin - cellLayoutInfo.Margins.Bottom) < 0f) ? 0f : (cellLayoutInfo.BottomPadding - (maxBottomMargin - cellLayoutInfo.Margins.Bottom)));
			cellLayoutInfo.Margins.Bottom = maxBottomMargin;
			if (num < num2)
			{
				num = num2;
			}
		}
		return num;
	}

	private void CommitRow()
	{
		if (!m_table.IsInCell)
		{
			(m_lcOperator as Layouter).IsRowFitInSamePage = false;
		}
		RowLayoutInfo rowLayoutInfo = m_currRowLW.Widget.LayoutInfo as RowLayoutInfo;
		LayoutedWidget rowLW = m_currRowLW;
		WTableRow wTableRow = m_currRowLW.Widget as WTableRow;
		if (m_ltState == LayoutState.NotFitted && m_ltWidget.ChildWidgets.Count > 0)
		{
			rowLayoutInfo.IsRowSplitted = false;
			LayoutedWidget layoutedWidget = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1];
			if (layoutedWidget.Widget.LayoutInfo is RowLayoutInfo)
			{
				RowLayoutInfo rowLayoutInfo2 = layoutedWidget.Widget.LayoutInfo as RowLayoutInfo;
				if (!rowLayoutInfo2.IsRowHasVerticalMergeEndCell || rowLayoutInfo2.IsRowHasVerticalMergeStartCell || IsPreviousRowHasVerticalMergeContinueCell(layoutedWidget.Widget as WTableRow))
				{
					rowLW = layoutedWidget;
				}
			}
			if (m_currRowIndex - 1 >= 0 && IsFirstItemInPage)
			{
				if (IsWord2013(m_table.Document))
				{
					if (!rowLayoutInfo.IsRowBreakByPageBreakBefore && (m_widget.LayoutInfo as TableLayoutInfo).HeaderRowHeight + m_table.Rows[m_currRowIndex].Height >= m_layoutArea.ClientArea.Height)
					{
						(m_widget.LayoutInfo as TableLayoutInfo).IsHeaderNotRepeatForAllPages = true;
					}
					else
					{
						(m_widget.LayoutInfo as TableLayoutInfo).IsHeaderNotRepeatForAllPages = false;
						rowLayoutInfo.IsRowBreakByPageBreakBefore = false;
					}
				}
				else if ((m_widget.LayoutInfo as TableLayoutInfo).HeaderRowHeight + m_table.Rows[m_currRowIndex].Height >= m_layoutArea.ClientArea.Height)
				{
					(m_widget.LayoutInfo as TableLayoutInfo).IsHeaderNotRepeatForAllPages = true;
				}
			}
			UpdateSplittedCells();
		}
		UpdateVerticalMergedCell(rowLW, m_ltState == LayoutState.NotFitted);
		if (rowLayoutInfo.IsRowHasVerticalTextCell && !rowLayoutInfo.IsVerticalText && m_currRowLW.ChildWidgets.Count > 0 && m_ltState != LayoutState.NotFitted)
		{
			UpdateVerticalTextCellLW();
		}
		float num = ((CurrRowIndex == m_table.Rows.Count - 1 || (m_currRowLW.Widget as WTableRow).RowFormat.CellSpacing > 0f || m_table.TableFormat.CellSpacing > 0f || m_blastRowState == LayoutState.Splitted) ? ((m_currRowLW.Widget.LayoutInfo as RowLayoutInfo).Paddings.Bottom + (m_currRowLW.Widget.LayoutInfo as RowLayoutInfo).Margins.Bottom) : 0f);
		m_currRowLW.Bounds = new RectangleF(m_currRowLW.Bounds.X, m_currRowLW.Bounds.Y, m_currRowLW.Bounds.Width, m_currRowLW.Bounds.Height + num);
		bool flag = true;
		if (m_ltState == LayoutState.Unknown && m_bAtLastOneCellFitted)
		{
			if (!IsWord2013(m_table.Document) && m_currRowLW.Bounds.Bottom > (m_lcOperator as Layouter).ClientLayoutArea.Bottom && IsRowMoveToNextPageBasedOnNestedFloatingTableBottom(m_currRowLW))
			{
				m_sptWidget = new SplitTableWidget(TableWidget, CurrRowIndex + 1);
				m_ltState = LayoutState.Splitted;
				return;
			}
			IWidget childParaWidget = GetChildParaWidget(m_currRowLW);
			bool flag2 = false;
			if (m_table.TableFormat.WrapTextAround && wTableRow.Index == 0 && !m_table.IsInCell && IsWord2013(m_table.Document))
			{
				flag2 = base.IsForceFitLayout;
			}
			if (((childParaWidget != null && childParaWidget is WParagraph && (childParaWidget as WParagraph).ParagraphFormat.PageBreakBefore) || (Math.Round(m_currRowLW.Bounds.Height, 2) > (double)m_layoutArea.ClientActiveArea.Height && m_blastRowState != LayoutState.Splitted)) && !base.IsForceFitLayout && !flag2 && wTableRow.OwnerTable.ChildEntities.Count == 1 && Math.Round(m_currRowLW.Bounds.Bottom, 2) > Math.Round((m_lcOperator as Layouter).ClientLayoutArea.Bottom, 2) && !wTableRow.OwnerTable.IsFrame)
			{
				m_sptWidget = new SplitTableWidget(TableWidget, CurrRowIndex + 1);
				m_ltState = LayoutState.Splitted;
				return;
			}
			for (int i = 0; i < m_currRowLW.ChildWidgets.Count; i++)
			{
				if (!rowLayoutInfo.IsExactlyRowHeight)
				{
					UpdateCellHeight(i);
					continue;
				}
				LayoutedWidget layoutedWidget2 = m_currRowLW.ChildWidgets[i];
				if (layoutedWidget2.Widget.LayoutInfo.IsVerticalText)
				{
					UpdateVerticalTextCellAlignment(layoutedWidget2);
				}
				else
				{
					UpdateVerticalCellAlignment(layoutedWidget2);
				}
				if (!layoutedWidget2.Widget.LayoutInfo.IsVerticalText)
				{
					RectangleF bounds = layoutedWidget2.ChildWidgets[0].Bounds;
					bounds.Height = layoutedWidget2.Bounds.Bottom - bounds.Top - (layoutedWidget2.Widget.LayoutInfo as CellLayoutInfo).Margins.Bottom;
					layoutedWidget2.ChildWidgets[0].Bounds = bounds;
				}
			}
			if (m_blastRowState != LayoutState.Splitted && m_table.IsInCell && !base.IsForceFitLayout)
			{
				for (int j = 0; j < m_currRowLW.ChildWidgets.Count; j++)
				{
					LayoutedWidget layoutedWidget3 = m_currRowLW.ChildWidgets[j].ChildWidgets[0];
					if (layoutedWidget3.ChildWidgets.Count > 0 && layoutedWidget3.ChildWidgets[0].Widget is WTable && layoutedWidget3.ChildWidgets[0].ChildWidgets.Count > 0)
					{
						RowLayoutInfo rowLayoutInfo3 = layoutedWidget3.ChildWidgets[0].ChildWidgets[0].Widget.LayoutInfo as RowLayoutInfo;
						if (layoutedWidget3.ChildWidgets.Count == 1 && rowLayoutInfo3.RowHeight > m_rowLayoutArea.Height)
						{
							m_sptWidget = new SplitTableWidget(TableWidget, CurrRowIndex + 1);
							m_ltState = LayoutState.Splitted;
							return;
						}
					}
				}
			}
			bool isForceFitLayout = base.IsForceFitLayout;
			if (m_blastRowState == LayoutState.Splitted)
			{
				if (IsRowNeedToBeSplitted(isForceFitLayout))
				{
					if (!IsWord2013(m_table.Document) && m_table.IsInCell && (!m_table.GetOwnerTableCell().OwnerRow.RowFormat.IsBreakAcrossPages || !wTableRow.RowFormat.IsBreakAcrossPages) && isForceFitLayout)
					{
						for (int k = CurrRowIndex + 1; k < m_table.Rows.Count; k++)
						{
							((IWidget)m_table.Rows[k]).LayoutInfo.IsSkip = true;
						}
					}
					if (!((!wTableRow.RowFormat.IsBreakAcrossPages || wTableRow.IsHeader) && !IsWord2013(m_table.Document) && isForceFitLayout))
					{
						m_sptWidget = new SplitTableWidget(TableWidget, CurrRowIndex + 1, m_splitedCells);
						m_ltState = LayoutState.Splitted;
					}
				}
				else
				{
					flag = false;
					if (m_ltWidget.ChildWidgets.Count > 0)
					{
						if ((rowLayoutInfo.IsRowHasVerticalMergeStartCell && rowLayoutInfo.IsRowSplitted) || rowLayoutInfo.IsRowHasVerticalMergeEndCell || (rowLayoutInfo.IsRowSplitted && rowLayoutInfo.IsRowHasVerticalMergeContinueCell))
						{
							LayoutedWidget layoutedWidget4 = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1];
							if (layoutedWidget4.Widget.LayoutInfo is RowLayoutInfo)
							{
								RowLayoutInfo rowLayoutInfo4 = layoutedWidget4.Widget.LayoutInfo as RowLayoutInfo;
								if (!rowLayoutInfo4.IsRowHasVerticalMergeEndCell || rowLayoutInfo4.IsRowHasVerticalMergeStartCell)
								{
									rowLW = layoutedWidget4;
								}
							}
							m_splitedCells = new SplitWidgetContainer[m_table.Rows[CurrRowIndex].Cells.Count];
							for (int l = 0; l < m_table.Rows[CurrRowIndex].Cells.Count; l++)
							{
								m_splitedCells[l] = ((m_table.Rows[CurrRowIndex].Cells[l].ChildEntities.Count > 0) ? new SplitWidgetContainer(m_table.Rows[CurrRowIndex].Cells[l], m_table.Rows[CurrRowIndex].Cells[l].ChildEntities[0] as IWidget, 0) : new SplitWidgetContainer(m_table.Rows[CurrRowIndex].Cells[l]));
							}
							UpdateVerticalMergedCell(rowLW, isNextRowNotFitted: true);
							if (IsNotEmptySplittedCell() && !rowLayoutInfo.IsRowHasVerticalMergeStartCell)
							{
								m_sptWidget = new SplitTableWidget(TableWidget, CurrRowIndex + 1, m_splitedCells);
								m_ltState = LayoutState.Splitted;
							}
							else
							{
								m_sptWidget = new SplitTableWidget(TableWidget, CurrRowIndex + 1);
								m_ltState = LayoutState.Splitted;
							}
						}
						else
						{
							m_sptWidget = new SplitTableWidget(TableWidget, CurrRowIndex + 1);
							m_ltState = LayoutState.Splitted;
						}
					}
					else
					{
						m_ltState = LayoutState.NotFitted;
						if (DocumentLayouter.IsUpdatingTOC && !wTableRow.RowFormat.IsBreakAcrossPages && !IsWord2013(m_table.Document))
						{
							(m_lcOperator as Layouter).IsRowFitInSamePage = true;
						}
					}
				}
			}
			if (flag)
			{
				m_ltWidget.ChildWidgets.Add(m_currRowLW);
				m_currRowLW.Owner = m_ltWidget;
				UpdateLWBounds();
				UpdateClientArea();
				if (!(m_lcOperator as Layouter).IsLayoutingHeaderRow)
				{
					UpdateForceFitLayoutState(this);
				}
			}
			if (flag && !(m_lcOperator as Layouter).IsLayoutingHeaderRow && wTableRow.IsHeader && !(base.LayoutInfo as TableLayoutInfo).IsHeaderRowHeightUpdated && (wTableRow.Index == 0 || (wTableRow.PreviousSibling as WTableRow).IsHeader))
			{
				(base.LayoutInfo as TableLayoutInfo).HeaderRowHeight += m_currRowLW.Bounds.Height;
			}
		}
		else if (m_ltWidget.ChildWidgets.Count > 0)
		{
			if (IsNotEmptySplittedCell())
			{
				m_sptWidget = new SplitTableWidget(TableWidget, CurrRowIndex + 1, m_splitedCells);
				m_ltState = LayoutState.Splitted;
			}
			else
			{
				m_sptWidget = new SplitTableWidget(TableWidget, CurrRowIndex + 1);
				m_ltState = LayoutState.Splitted;
			}
			(TableLayoutInfo as ITableLayoutInfo).IsSplittedTable = true;
		}
	}

	private bool IsRowMoveToNextPageBasedOnNestedFloatingTableBottom(LayoutedWidget currRowWidget)
	{
		if (currRowWidget.ChildWidgets != null)
		{
			for (int i = 0; i < currRowWidget.ChildWidgets.Count; i++)
			{
				LayoutedWidget layoutedWidget = currRowWidget.ChildWidgets[i];
				if (layoutedWidget.ChildWidgets.Count <= 0)
				{
					continue;
				}
				LayoutedWidget layoutedWidget2 = layoutedWidget.ChildWidgets[0];
				for (int j = 0; j < layoutedWidget2.ChildWidgets.Count; j++)
				{
					if (layoutedWidget2.ChildWidgets[j].Widget is WTable && (layoutedWidget2.ChildWidgets[j].Widget as WTable).TableFormat.Positioning.DistanceFromBottom > 0f)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool IsNeedToLayoutHeaderRow()
	{
		if (m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
		{
			WTableRow wTableRow = m_currRowLW.Widget as WTableRow;
			if (wTableRow.NextSibling is WTableRow && !(wTableRow.NextSibling as WTableRow).IsHeader)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateVerticalMergedCell(LayoutedWidget rowLW, bool isNextRowNotFitted)
	{
		RowLayoutInfo rowLayoutInfo = rowLW.Widget.LayoutInfo as RowLayoutInfo;
		bool flag = false;
		WTableRow wTableRow = ((rowLW.Widget is WTableRow) ? (rowLW.Widget as WTableRow) : null);
		bool flag2 = false;
		bool flag3 = false;
		List<LayoutedWidget> list = new List<LayoutedWidget>();
		if (wTableRow != null && wTableRow.IsHeader && (rowLayoutInfo.IsRowHasVerticalMergeContinueCell || rowLayoutInfo.IsRowHasVerticalMergeStartCell))
		{
			flag = IsNeedToLayoutHeaderRow();
		}
		if ((rowLayoutInfo.IsRowHasVerticalMergeStartCell && (rowLayoutInfo.IsRowSplitted || isNextRowNotFitted || flag)) || rowLayoutInfo.IsRowHasVerticalMergeEndCell || ((rowLayoutInfo.IsRowSplitted || isNextRowNotFitted || flag) && rowLayoutInfo.IsRowHasVerticalMergeContinueCell))
		{
			List<LayoutedWidget> list2 = new List<LayoutedWidget>();
			int count = (m_lcOperator as Layouter).FootnoteWidgets.Count;
			int num = 0;
			while (num < rowLW.ChildWidgets.Count)
			{
				LayoutedWidget layoutedWidget = rowLW.ChildWidgets[num];
				while (true)
				{
					LayoutedWidget layoutedWidget2 = ((((layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeContinue && (layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeEnd) || (rowLayoutInfo.IsRowHasVerticalMergeEndCell && (layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeStart && (layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeEnd) || ((layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeContinue && (rowLayoutInfo.IsRowSplitted || isNextRowNotFitted || flag))) ? GetVerticalMergeStartLW(layoutedWidget) : (((layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeStart && (rowLayoutInfo.IsRowSplitted || isNextRowNotFitted || flag)) ? layoutedWidget : null));
					if (layoutedWidget2 != null)
					{
						if ((layoutedWidget2.Widget.LayoutInfo as CellLayoutInfo).IsVerticalText)
						{
							if (!list2.Contains(layoutedWidget))
							{
								if (num != rowLW.ChildWidgets.Count - 1)
								{
									list2.Add(layoutedWidget);
									goto IL_09ff;
								}
							}
							else
							{
								list2.Remove(layoutedWidget);
							}
						}
						float height = ((rowLayoutInfo.IsExactlyRowHeight || (flag && (rowLW.Widget as WTableRow).HeightType != TableRowHeightType.AtLeast) || isNextRowNotFitted || (layoutedWidget2.Widget.LayoutInfo as CellLayoutInfo).IsVerticalText || (rowLayoutInfo.IsRowSplitted && (rowLayoutInfo.IsRowHasVerticalMergeContinueCell || rowLayoutInfo.IsRowHasVerticalMergeStartCell))) ? (rowLW.Bounds.Bottom - layoutedWidget2.Bounds.Top) : (m_layoutArea.ClientActiveArea.Bottom - layoutedWidget2.Bounds.Top));
						LayoutArea layoutArea = new LayoutArea(new RectangleF(layoutedWidget2.Bounds.Left, layoutedWidget2.Bounds.Top, layoutedWidget2.Bounds.Width, height));
						LayoutContext layoutContext = LayoutContext.Create(layoutedWidget2.Widget, m_lcOperator, base.IsForceFitLayout || !IsWord2013(wTableRow.Document));
						if (layoutedWidget2 != layoutedWidget)
						{
							(layoutedWidget2.Widget.LayoutInfo as CellLayoutInfo).Margins.Bottom = (layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).Margins.Bottom;
						}
						layoutContext.ClientLayoutAreaRight = (float)layoutArea.Width;
						(m_currRowLW.Widget.LayoutInfo as RowLayoutInfo).IsHiddenRow = false;
						LayoutedWidget layoutedWidget3 = LayoutCell(layoutContext, layoutArea.ClientArea, isSkip: false);
						layoutedWidget3.Bounds = new RectangleF(layoutedWidget3.Bounds.X, layoutedWidget3.Bounds.Y, layoutedWidget3.Bounds.Width, (rowLayoutInfo.IsExactlyRowHeight || (flag && (rowLW.Widget as WTableRow).HeightType != TableRowHeightType.AtLeast) || isNextRowNotFitted || (layoutedWidget2.Widget.LayoutInfo as CellLayoutInfo).IsVerticalText || (rowLayoutInfo.IsRowSplitted && rowLayoutInfo.IsRowHasVerticalMergeContinueCell)) ? layoutArea.ClientArea.Height : (((layoutedWidget3.Bounds.Bottom < rowLW.Bounds.Bottom) ? rowLW.Bounds.Bottom : layoutedWidget3.Bounds.Bottom) - layoutArea.ClientArea.Top));
						if (layoutedWidget3.Bounds.Bottom >= m_layoutArea.ClientActiveArea.Bottom && rowLayoutInfo.IsRowHasVerticalMergeEndCell && !IsWord2013(wTableRow.Document) && !rowLayoutInfo.IsKeepWithNext && !flag2 && !IsRowHasExactlyHeight(rowLW.Widget as WTableRow))
						{
							UpdateSplittedCells();
							m_ltState = LayoutState.NotFitted;
							layoutContext.m_ltState = LayoutState.Splitted;
							flag2 = true;
						}
						if (layoutContext.State == LayoutState.NotFitted)
						{
							m_currColIndex = ((layoutedWidget3.Widget is WTableCell) ? (layoutedWidget3.Widget as WTableCell) : ((layoutedWidget3.Widget is SplitWidgetContainer && (layoutedWidget3.Widget as SplitWidgetContainer).RealWidgetContainer is WTableCell) ? ((layoutedWidget3.Widget as SplitWidgetContainer).RealWidgetContainer as WTableCell) : null))?.GetIndexInOwnerCollection() ?? (-1);
							WTableCell wTableCell = ((layoutContext.Widget is WTableCell) ? (layoutContext.Widget as WTableCell) : ((layoutContext.Widget is SplitWidgetContainer && (layoutContext.Widget as SplitWidgetContainer).RealWidgetContainer is WTableCell) ? ((layoutContext.Widget as SplitWidgetContainer).RealWidgetContainer as WTableCell) : null));
							IWidget widget = ((layoutContext.Widget is SplitTableWidget) ? (layoutContext.Widget as SplitWidgetContainer) : ((wTableCell != null) ? new SplitWidgetContainer(layoutContext.Widget as IWidgetContainer, wTableCell.ChildEntities[0] as IWidget, 0) : null));
							if (isNextRowNotFitted || flag3)
							{
								rowLW = m_currRowLW;
								rowLayoutInfo = m_currRowLW.Widget.LayoutInfo as RowLayoutInfo;
								if (widget != null)
								{
									layoutContext.SplittedWidget = widget;
									UpdateSplittedVerticalMergeCell(layoutContext, layoutedWidget2, rowLayoutInfo, layoutedWidget, m_currRowLW, num, isNextRowNotFitted: true);
								}
							}
							else
							{
								int count2 = m_ltWidget.ChildWidgets.Count;
								m_currCellLW = layoutedWidget;
								MarkAsNotFitted(layoutContext);
								UpdateSplittedCells();
								if (widget == null || m_ltWidget.ChildWidgets.Count != count2)
								{
									break;
								}
								layoutContext.SplittedWidget = widget;
								UpdateSplittedVerticalMergeCell(layoutContext, layoutedWidget2, rowLayoutInfo, layoutedWidget, m_currRowLW, num, isNextRowNotFitted);
							}
							flag3 = true;
							goto IL_09ff;
						}
						if ((layoutedWidget2.Widget.LayoutInfo as CellLayoutInfo).IsVerticalText)
						{
							UpdateVerticalTextCellAlignment(layoutedWidget3);
						}
						else
						{
							UpdateVerticalCellAlignment(layoutedWidget3);
						}
						if (!(layoutedWidget2.Widget.LayoutInfo as CellLayoutInfo).IsVerticalText)
						{
							RectangleF bounds = layoutedWidget3.ChildWidgets[0].Bounds;
							bounds.Height = layoutedWidget3.Bounds.Bottom - bounds.Top - (layoutedWidget2.Widget.LayoutInfo as CellLayoutInfo).Margins.Bottom;
							layoutedWidget3.ChildWidgets[0].Bounds = bounds;
						}
						m_currCellLW = layoutedWidget3;
						m_currCellLW.Owner = layoutedWidget2.Owner;
						if (rowLayoutInfo.IsRowHasVerticalMergeEndCell && !(layoutedWidget2.Widget.LayoutInfo as CellLayoutInfo).IsVerticalText)
						{
							UpdateRowLWBounds();
						}
						UpdateSplittedVerticalMergeCell(layoutContext, layoutedWidget2, rowLayoutInfo, layoutedWidget, rowLW, num, isNextRowNotFitted);
						if ((layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeContinue || (rowLayoutInfo.IsRowHasVerticalMergeEndCell && (layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeStart && (layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeEnd))
						{
							for (int i = 0; i < layoutedWidget2.Owner.ChildWidgets.Count; i++)
							{
								if (layoutedWidget2.Owner.ChildWidgets[i].Widget == layoutedWidget2.Widget)
								{
									list.Add(m_currCellLW);
									layoutedWidget2.Owner.ChildWidgets[i] = m_currCellLW;
									break;
								}
							}
							if (!rowLayoutInfo.IsExactlyRowHeight && layoutContext.m_ltState == LayoutState.Splitted && !isNextRowNotFitted)
							{
								m_blastRowState = LayoutState.Splitted;
								rowLayoutInfo.IsRowSplitted = true;
								(TableLayoutInfo as ITableLayoutInfo).IsSplittedTable = true;
							}
							m_verticallyMergeStartLW.Remove(layoutedWidget2);
						}
						else if ((layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeStart)
						{
							if (!(layoutedWidget2.Widget.LayoutInfo as CellLayoutInfo).IsVerticalText)
							{
								rowLW.ChildWidgets[num] = m_currCellLW;
							}
							else
							{
								rowLW.ChildWidgets[rowLW.ChildWidgets.IndexOf(layoutedWidget)] = m_currCellLW;
							}
							if (m_verticallyMergeStartLW.Contains(layoutedWidget))
							{
								m_verticallyMergeStartLW.Remove(layoutedWidget);
							}
						}
					}
					if (num == rowLW.ChildWidgets.Count - 1 && list2.Count > 0)
					{
						layoutedWidget = list2[0];
						continue;
					}
					if (layoutedWidget2 == null && wTableRow.NextSibling is WTableRow && m_verticallyMergeStartLW.Count > 0 && m_verticallyMergeStartLW[0].Widget is SplitWidgetContainer)
					{
						WTableRow wTableRow2 = wTableRow.NextSibling as WTableRow;
						if (num < wTableRow2.Cells.Count && ((IWidget)wTableRow2.Cells[num]).LayoutInfo is CellLayoutInfo && !(wTableRow2.Cells[num].m_layoutInfo as CellLayoutInfo).IsRowMergeContinue && m_verticallyMergeStartLW.Contains(layoutedWidget))
						{
							m_verticallyMergeStartLW.Remove(layoutedWidget);
						}
					}
					goto IL_09ff;
					IL_09ff:
					num++;
					goto IL_0a05;
				}
				break;
				IL_0a05:;
			}
			if (list.Count > 0)
			{
				foreach (LayoutedWidget item in list)
				{
					if (item.Bounds.Bottom < rowLW.Bounds.Bottom)
					{
						item.Bounds = new RectangleF(item.Bounds.X, item.Bounds.Y, item.Bounds.Width, rowLW.Bounds.Bottom - item.Bounds.Top);
						if (!(item.Widget.LayoutInfo as CellLayoutInfo).IsVerticalText)
						{
							UpdateVerticalCellAlignment(item);
							RectangleF bounds2 = item.ChildWidgets[0].Bounds;
							bounds2.Height = item.Bounds.Bottom - bounds2.Top - (item.Widget.LayoutInfo as CellLayoutInfo).Margins.Bottom;
							item.ChildWidgets[0].Bounds = bounds2;
						}
					}
				}
			}
			if (count > 0 && (m_lcOperator as Layouter).FootnoteWidgets.Count > count)
			{
				UpdateUnorderedFootNotesBounds((m_lcOperator as Layouter).FootnoteWidgets.Count - count);
			}
		}
		else if (isNextRowNotFitted)
		{
			m_splitedCells = new SplitWidgetContainer[m_table.Rows[CurrRowIndex].Cells.Count];
		}
		if (m_ltState != LayoutState.NotFitted && flag3)
		{
			m_ltState = LayoutState.NotFitted;
		}
	}

	private bool IsRowHasExactlyHeight(Entity entity)
	{
		do
		{
			if (entity is WTableRow && (entity as WTableRow).HeightType == TableRowHeightType.Exactly)
			{
				return true;
			}
			entity = entity.Owner;
		}
		while (!(entity is WSection));
		return false;
	}

	private void UpdateUnorderedFootNotesBounds(int unOrderCount)
	{
		LayoutedWidgetList footnoteWidgets = (m_lcOperator as Layouter).FootnoteWidgets;
		if (footnoteWidgets.Count <= 2 || footnoteWidgets.Count < unOrderCount)
		{
			return;
		}
		while (unOrderCount > 0)
		{
			LayoutedWidget layoutedWidget = footnoteWidgets[footnoteWidgets.Count - unOrderCount];
			int num = -1;
			if (IsFootNote(layoutedWidget.Widget))
			{
				int footNoteID = GetFootNoteID(layoutedWidget.Widget);
				for (int num2 = footnoteWidgets.Count - unOrderCount - 1; num2 > 0; num2--)
				{
					LayoutedWidget layoutedWidget2 = footnoteWidgets[num2];
					if (IsFootNote(layoutedWidget2.Widget) && GetFootNoteID(layoutedWidget2.Widget) > footNoteID)
					{
						float num3 = layoutedWidget2.Bounds.Y - layoutedWidget.Bounds.Y;
						layoutedWidget.ShiftLocation(0.0, num3, isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false, isNeedToShiftOwnerWidget: true);
						layoutedWidget2.ShiftLocation(0.0, Math.Abs(num3), isPictureNeedToBeShifted: true, isFromFloatingItemVerticalAlignment: false, isNeedToShiftOwnerWidget: true);
						num = num2;
					}
				}
				if (num != -1)
				{
					footnoteWidgets.RemoveAt(footnoteWidgets.Count - unOrderCount);
					footnoteWidgets.Insert(num, layoutedWidget);
				}
			}
			unOrderCount--;
		}
	}

	private int GetFootNoteID(IWidget widget)
	{
		return Convert.ToInt32((((widget as WTextBody).Owner as IWidget).LayoutInfo as FootnoteLayoutInfo).FootnoteID);
	}

	private bool IsFootNote(IWidget widget)
	{
		if (widget is WTextBody)
		{
			return (widget as WTextBody).Owner is WFootnote;
		}
		return false;
	}

	private void UpdateSplittedVerticalMergeCell(LayoutContext lc, LayoutedWidget mergeStartLW, RowLayoutInfo rowInfo, LayoutedWidget childLW, LayoutedWidget rowLW, int currentIndex, bool isNextRowNotFitted)
	{
		if (!(mergeStartLW.Widget.LayoutInfo as CellLayoutInfo).IsVerticalText)
		{
			int num = currentIndex;
			if (isNextRowNotFitted)
			{
				float cellStartPosition = ((mergeStartLW.Widget is SplitWidgetContainer) ? ((mergeStartLW.Widget as SplitWidgetContainer).RealWidgetContainer as WTableCell) : (mergeStartLW.Widget as WTableCell)).CellStartPosition;
				float num2 = 0f;
				for (int i = 0; i < m_table.Rows[CurrRowIndex].Cells.Count; i++)
				{
					if (Math.Round(num2, 2) == Math.Round(cellStartPosition, 2))
					{
						num = i;
						break;
					}
					num2 += m_table.Rows[CurrRowIndex].Cells[i].Width;
				}
			}
			if ((lc.State != LayoutState.NotFitted || !IsHeaderRow(rowLW.Widget as WTableRow)) && (rowInfo.IsRowSplitted || isNextRowNotFitted || (!rowInfo.IsExactlyRowHeight && lc.m_ltState == LayoutState.Splitted)))
			{
				(mergeStartLW.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeEnd = m_splitedCells[num] != null && m_splitedCells[num].RealWidgetContainer != null && (m_splitedCells[num].RealWidgetContainer.LayoutInfo as CellLayoutInfo).IsRowMergeEnd;
			}
			m_splitedCells[num] = ((lc.SplittedWidget is SplitWidgetContainer) ? (lc.SplittedWidget as SplitWidgetContainer) : new SplitWidgetContainer(lc.Widget as IWidgetContainer));
			return;
		}
		int num3 = rowLW.ChildWidgets.IndexOf(childLW);
		if (isNextRowNotFitted)
		{
			float cellStartPosition2 = ((mergeStartLW.Widget is SplitWidgetContainer) ? ((mergeStartLW.Widget as SplitWidgetContainer).RealWidgetContainer as WTableCell) : (mergeStartLW.Widget as WTableCell)).CellStartPosition;
			float num4 = 0f;
			for (int j = 0; j < m_table.Rows[CurrRowIndex].Cells.Count; j++)
			{
				if (Math.Round(num4, 2) == Math.Round(cellStartPosition2, 2))
				{
					num3 = j;
					break;
				}
				num4 += m_table.Rows[CurrRowIndex].Cells[j].Width;
			}
		}
		if (rowInfo.IsRowSplitted || isNextRowNotFitted || (!rowInfo.IsExactlyRowHeight && lc.m_ltState == LayoutState.Splitted))
		{
			(mergeStartLW.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeEnd = m_splitedCells[num3] != null && m_splitedCells[num3].RealWidgetContainer != null && (m_splitedCells[num3].RealWidgetContainer.LayoutInfo as CellLayoutInfo).IsRowMergeEnd;
		}
		m_splitedCells[num3] = ((lc.SplittedWidget is SplitWidgetContainer) ? (lc.SplittedWidget as SplitWidgetContainer) : new SplitWidgetContainer(lc.Widget as IWidgetContainer));
	}

	private bool IsHeaderRow(WTableRow row)
	{
		bool isHeader = row.IsHeader;
		if (isHeader && IsWord2013(row.Document))
		{
			while (row.PreviousSibling != null)
			{
				row = row.PreviousSibling as WTableRow;
				if (!row.IsHeader)
				{
					return false;
				}
			}
		}
		return isHeader;
	}

	private LayoutedWidget GetVerticalMergeStartLW(LayoutedWidget verticalMergeEndLW)
	{
		WTableCell wTableCell = ((verticalMergeEndLW.Widget is WTableCell) ? (verticalMergeEndLW.Widget as WTableCell) : ((verticalMergeEndLW.Widget is SplitWidgetContainer && (verticalMergeEndLW.Widget as SplitWidgetContainer).RealWidgetContainer is WTableCell) ? ((verticalMergeEndLW.Widget as SplitWidgetContainer).RealWidgetContainer as WTableCell) : null));
		for (int i = 0; i < m_verticallyMergeStartLW.Count; i++)
		{
			LayoutedWidget layoutedWidget = m_verticallyMergeStartLW[i];
			WTableCell wTableCell2 = ((layoutedWidget.Widget is WTableCell) ? (layoutedWidget.Widget as WTableCell) : ((layoutedWidget.Widget is SplitWidgetContainer && (layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer is WTableCell) ? ((layoutedWidget.Widget as SplitWidgetContainer).RealWidgetContainer as WTableCell) : null));
			if (Math.Round(layoutedWidget.Bounds.Left) == Math.Round(verticalMergeEndLW.Bounds.Left) && wTableCell.OwnerRow.Index >= wTableCell2.OwnerRow.Index)
			{
				return layoutedWidget;
			}
		}
		return null;
	}

	private void UpdateVerticalTextCellLW()
	{
		for (int i = 0; i < m_currRowLW.ChildWidgets.Count; i++)
		{
			LayoutedWidget layoutedWidget = m_currRowLW.ChildWidgets[i];
			CellLayoutInfo cellLayoutInfo = layoutedWidget.Widget.LayoutInfo as CellLayoutInfo;
			if (cellLayoutInfo.IsVerticalText && !cellLayoutInfo.IsRowMergeStart && !cellLayoutInfo.IsRowMergeContinue)
			{
				float height = m_currRowLW.Bounds.Bottom - layoutedWidget.Bounds.Top;
				LayoutArea layoutArea = new LayoutArea(new RectangleF(layoutedWidget.Bounds.Left, layoutedWidget.Bounds.Top, layoutedWidget.Bounds.Width, height));
				LayoutContext layoutContext = LayoutContext.Create(layoutedWidget.Widget, m_lcOperator, base.IsForceFitLayout);
				layoutContext.ClientLayoutAreaRight = (float)layoutArea.Width;
				LayoutedWidget layoutedWidget2 = LayoutCell(layoutContext, layoutArea.ClientArea, isSkip: false);
				layoutedWidget2.Owner = m_currRowLW.ChildWidgets[i].Owner;
				m_currRowLW.ChildWidgets[i] = layoutedWidget2;
			}
		}
	}

	private void UpdateSplittedCells()
	{
		m_splitedCells = new SplitWidgetContainer[m_table.Rows[CurrRowIndex].Cells.Count];
		for (int i = 0; i < m_table.Rows[CurrRowIndex].Cells.Count; i++)
		{
			m_splitedCells[i] = ((m_table.Rows[CurrRowIndex].Cells[i].ChildEntities.Count > 0) ? new SplitWidgetContainer(m_table.Rows[CurrRowIndex].Cells[i], m_table.Rows[CurrRowIndex].Cells[i].ChildEntities[0] as IWidget, 0) : new SplitWidgetContainer(m_table.Rows[CurrRowIndex].Cells[i]));
		}
	}

	private IWidget GetChildParaWidget(LayoutedWidget layoutedWidget)
	{
		if (layoutedWidget.ChildWidgets != null && layoutedWidget.ChildWidgets.Count > 0)
		{
			if (layoutedWidget.ChildWidgets[0].Widget is WTableCell || layoutedWidget.ChildWidgets[0].Widget is WTableRow || layoutedWidget.ChildWidgets[0].Widget is WTable)
			{
				return GetChildParaWidget(layoutedWidget.ChildWidgets[0]);
			}
			if (layoutedWidget.ChildWidgets[0].Widget is WParagraph)
			{
				return layoutedWidget.ChildWidgets[0].Widget;
			}
		}
		return null;
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

	private bool IsNotEmptySplittedCell()
	{
		for (int i = 0; i < m_splitedCells.Length; i++)
		{
			if (m_splitedCells[i] != null && m_splitedCells[i].m_currentChild != null)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsRowNeedToBeSplitted(bool isFirstItemInPage)
	{
		WTableRow wTableRow = TableWidget.GetRowWidget(CurrRowIndex) as WTableRow;
		if ((wTableRow.RowFormat.IsBreakAcrossPages || (m_table.TableFormat.WrapTextAround && wTableRow.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013) || isFirstItemInPage) && (!m_currRowLW.Widget.LayoutInfo.IsKeepWithNext || isFirstItemInPage) && (!IsHeaderRow(wTableRow) || isFirstItemInPage))
		{
			if (IsHeaderRow(wTableRow))
			{
				(TableLayoutInfo as TableLayoutInfo).IsHeaderNotRepeatForAllPages = true;
			}
			return true;
		}
		if ((m_currRowLW.Widget.LayoutInfo.IsKeepWithNext || (m_currRowIndex >= 1 && TableWidget.GetRowWidget(CurrRowIndex - 1).LayoutInfo.IsKeepWithNext)) && (CommitKeepWithNext() || (IsFirstItemInPage && IsAllRowHavingKeepWithNext())) && m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
		{
			return true;
		}
		return false;
	}

	private bool IsAllRowHavingKeepWithNext()
	{
		bool result = false;
		for (int i = 0; i < m_ltWidget.ChildWidgets.Count; i++)
		{
			if (m_ltWidget.ChildWidgets[i].Widget.LayoutInfo.IsKeepWithNext)
			{
				result = true;
				continue;
			}
			return false;
		}
		return result;
	}

	private bool IsCellhMergeTillEnd(WTableRow currRow, int maxColIndex)
	{
		if (m_currColIndex > -1 && m_currColIndex <= maxColIndex && currRow.Cells[m_currColIndex].CellFormat.HorizontalMerge == CellMerge.Start)
		{
			for (int i = m_currColIndex + 1; i <= maxColIndex; i++)
			{
				if (currRow.Cells[i].CellFormat.HorizontalMerge != CellMerge.Continue)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private double GetRightPadValue(WTableRow currRow, int maxColIndex)
	{
		return ((m_currColIndex == maxColIndex || IsCellhMergeTillEnd(currRow, maxColIndex)) && ((m_currRowLW.Widget as WTableRow).RowFormat.CellSpacing > 0f || m_table.TableFormat.CellSpacing > 0f)) ? (m_currRowLW.Widget.LayoutInfo as RowLayoutInfo).Margins.Right : 0f;
	}

	private void UpdateRowLWBounds()
	{
		RectangleF bounds = m_currRowLW.Bounds;
		RectangleF bounds2 = m_currCellLW.Bounds;
		WTableRow wTableRow = m_table.Rows[m_currRowIndex];
		double rightPadValue = GetRightPadValue(wTableRow, wTableRow.Cells.Count - 1);
		double num = Math.Max((double)bounds2.Right + rightPadValue, bounds.Right);
		double num2 = Math.Max(bounds2.Bottom, bounds.Bottom);
		SizeF size = new SizeF((float)(num - (double)bounds.Left), (float)(num2 - (double)bounds.Top));
		m_currRowLW.Bounds = new RectangleF(bounds.Location, size);
	}

	private void NextRowIndex()
	{
		if (m_bHeaderRepeat)
		{
			WTable wTable = TableWidget as WTable;
			WTableRow wTableRow = wTable.Rows[0];
			if (wTableRow.IsHeader && (m_currRowIndex <= 0 || !wTable.Rows[m_currRowIndex].IsHeader || !IsNeedToLayoutHeaderRow(wTable, m_currRowIndex)) && !(base.LayoutInfo as TableLayoutInfo).IsHeaderNotRepeatForAllPages && !SkipLayoutingHeaderRow())
			{
				LayoutHeaderRow(wTableRow);
			}
			else
			{
				m_bHeaderRepeat = false;
			}
		}
		else
		{
			m_currRowIndex++;
		}
	}

	private bool IsNeedToLayoutHeaderRow(WTable table, int currRowIndex)
	{
		if (IsWord2013(table.Document))
		{
			while (currRowIndex > 0)
			{
				if (table.Rows[currRowIndex].IsHeader)
				{
					currRowIndex--;
					continue;
				}
				return false;
			}
		}
		return true;
	}

	private bool SkipLayoutingHeaderRow()
	{
		float pictureHeight = 0f;
		if (IsWord2013(m_table.Document) && IsCurrentPageFirstItemPicture(ref pictureHeight) && TableLayoutInfo is TableLayoutInfo)
		{
			TableLayoutInfo tableLayoutInfo = TableLayoutInfo as TableLayoutInfo;
			if (tableLayoutInfo.HeaderRowHeight + pictureHeight > (m_lcOperator as Layouter).ClientLayoutArea.Height)
			{
				LayoutedHeaderRowHeight = tableLayoutInfo.HeaderRowHeight;
				tableLayoutInfo.HeaderRowHeight = 0f;
				return true;
			}
		}
		return false;
	}

	private bool IsCurrentPageFirstItemPicture(ref float pictureHeight)
	{
		if (m_spitTableWidget.SplittedCells != null)
		{
			for (int num = m_spitTableWidget.SplittedCells.Length - 1; num >= 0; num--)
			{
				if (m_spitTableWidget.SplittedCells[num] != null && m_spitTableWidget.SplittedCells[num].Count > 0)
				{
					SplitWidgetContainer splitWidgetContainer = m_spitTableWidget.SplittedCells[num];
					if (splitWidgetContainer.RealWidgetContainer is WTableCell && (splitWidgetContainer.RealWidgetContainer as WTableCell).OwnerRow.HeightType == TableRowHeightType.Exactly)
					{
						return false;
					}
					float num2 = (splitWidgetContainer.LayoutInfo as CellLayoutInfo).TopPadding + (splitWidgetContainer.LayoutInfo as CellLayoutInfo).BottomPadding;
					if (splitWidgetContainer.m_currentChild is WParagraph)
					{
						WPicture wPicture = FindPictureInParagraph(splitWidgetContainer.m_currentChild as WParagraph);
						if (wPicture != null)
						{
							pictureHeight = wPicture.Height + num2;
							return true;
						}
					}
					return IsSplittedParagrapghHasPicture(splitWidgetContainer, ref pictureHeight, num2);
				}
			}
		}
		else if (m_currRowIndex >= 0 && m_currRowIndex < TableWidget.RowsCount && m_table.Rows[m_currRowIndex].HeightType != TableRowHeightType.Exactly)
		{
			return IsAnyOfCellFirstItemPicture(ref pictureHeight);
		}
		return false;
	}

	private bool IsSplittedParagrapghHasPicture(SplitWidgetContainer widgetContainer, ref float pictureHeight, float cellPadding)
	{
		while (widgetContainer.m_currentChild is SplitWidgetContainer)
		{
			widgetContainer = widgetContainer.m_currentChild as SplitWidgetContainer;
			if (widgetContainer == null)
			{
				return false;
			}
		}
		if ((widgetContainer.m_currentChild is WPicture || (widgetContainer.m_currentChild is WOleObject && (widgetContainer.m_currentChild as WOleObject).OlePicture != null)) && widgetContainer.m_currentChild is Entity && !(widgetContainer.m_currentChild as Entity).IsFloatingItem(isTextWrapAround: false))
		{
			if (widgetContainer.m_currentChild is WOleObject)
			{
				pictureHeight = (widgetContainer.m_currentChild as WOleObject).OlePicture.Height + cellPadding;
			}
			else
			{
				pictureHeight = (widgetContainer.m_currentChild as WPicture).Height + cellPadding;
			}
			return true;
		}
		return false;
	}

	private bool IsAnyOfCellFirstItemPicture(ref float pictureHeight)
	{
		List<WPicture> list = FindPicturesFromTableCell();
		if (list.Count == 0)
		{
			return false;
		}
		foreach (WPicture item in list)
		{
			if (pictureHeight < item.Height)
			{
				pictureHeight = item.Height;
			}
		}
		return true;
	}

	private List<WPicture> FindPicturesFromTableCell()
	{
		List<WPicture> list = new List<WPicture>();
		foreach (WTableCell cell in m_table.Rows[m_currRowIndex].Cells)
		{
			if (cell.ChildEntities.Count > 0 && cell.ChildEntities[0] is WParagraph)
			{
				WPicture wPicture = FindPictureInParagraph(cell.ChildEntities[0] as WParagraph);
				if (wPicture != null)
				{
					list.Add(wPicture);
				}
			}
		}
		return list;
	}

	private WPicture FindPictureInParagraph(WParagraph paragraph)
	{
		foreach (Entity childEntity in paragraph.ChildEntities)
		{
			if (!(childEntity is BookmarkStart) && !(childEntity is BookmarkEnd))
			{
				if (childEntity is WPicture && !childEntity.IsFloatingItem(isTextWrapAround: false))
				{
					return childEntity as WPicture;
				}
				if (childEntity is WOleObject && (childEntity as WOleObject).OlePicture != null && !childEntity.IsFloatingItem(isTextWrapAround: false))
				{
					return (childEntity as WOleObject).OlePicture;
				}
			}
		}
		return null;
	}

	private void LayoutHeaderRow(WTableRow row)
	{
		if (!row.IsHeader)
		{
			return;
		}
		(m_lcOperator as Layouter).IsLayoutingHeaderRow = true;
		bool flag = false;
		if ((base.LayoutInfo as TableLayoutInfo).HeaderRowHeight <= (m_lcOperator as Layouter).ClientLayoutArea.Height)
		{
			while (row.IsHeader || flag)
			{
				m_currHeaderRowIndex++;
				if (m_currRowIndex != m_currHeaderRowIndex)
				{
					UpdateHeaderRowWidget();
					CommitRow();
					m_currColIndex = -1;
					flag = ((row.m_layoutInfo as RowLayoutInfo).IsRowHasVerticalMergeStartCell || ((row.m_layoutInfo as RowLayoutInfo).IsRowHasVerticalMergeContinueCell && !(row.m_layoutInfo as RowLayoutInfo).IsRowHasVerticalMergeEndCell)) && row.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013;
					if ((row.NextSibling != null && row.NextSibling is WTableRow && (row.NextSibling as WTableRow).IsHeader) || flag)
					{
						row = row.NextSibling as WTableRow;
						continue;
					}
					m_bHeaderRepeat = false;
					flag = false;
					break;
				}
				m_bHeaderRepeat = false;
				break;
			}
			if (m_ltWidget.ChildWidgets.Count > 0)
			{
				m_headerRowHeight = (float)Math.Round(m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Bounds.Bottom - m_ltWidget.ChildWidgets[0].Bounds.Top, 2);
			}
		}
		(m_lcOperator as Layouter).IsLayoutingHeaderRow = false;
	}

	private void UpdateHeaderRowWidget()
	{
		m_currRowLW = new LayoutedWidget(TableWidget.GetRowWidget(CurrRowIndex));
		m_currRowLW.Bounds = new RectangleF(m_layoutArea.ClientActiveArea.Location, default(SizeF));
		for (int i = 0; i < (m_currRowLW.Widget as WTableRow).Cells.Count; i++)
		{
			for (int j = 0; j < (m_currRowLW.Widget as WTableRow).Cells[i].Items.Count; j++)
			{
				if (!((m_currRowLW.Widget as WTableRow).Cells[i].Items[j] is WParagraph) || !((m_currRowLW.Widget as WTableRow).Cells[i].Items[j] is WParagraph wParagraph))
				{
					continue;
				}
				for (int k = 0; k < wParagraph.Items.Count; k++)
				{
					if (wParagraph.Items[k] is WFootnote)
					{
						((IWidget)(wParagraph.Items[k] as WFootnote)).LayoutInfo.IsSkip = true;
					}
				}
			}
		}
		LayoutRow(m_currRowLW.Widget as WTableRow);
	}

	private bool IsInFrame()
	{
		bool flag = true;
		if (m_table.IsInCell)
		{
			flag = false;
		}
		if (m_table.IsFrame && flag)
		{
			return true;
		}
		return false;
	}

	private void UpdateCellHeight(int column)
	{
		LayoutedWidget layoutedWidget = m_currRowLW.ChildWidgets[column];
		RectangleF bounds = layoutedWidget.Bounds;
		RectangleF bounds2 = m_currRowLW.Bounds;
		float num = ((m_currRowIndex == m_table.Rows.Count - 1 || (m_currRowLW.Widget as WTableRow).RowFormat.CellSpacing > 0f || m_table.TableFormat.CellSpacing > 0f || m_blastRowState == LayoutState.Splitted) ? ((m_currRowLW.Widget.LayoutInfo as RowLayoutInfo).Paddings.Bottom + (m_currRowLW.Widget.LayoutInfo as RowLayoutInfo).Margins.Bottom) : 0f);
		bounds.Height = bounds2.Bottom - bounds.Top - num;
		layoutedWidget.Bounds = bounds;
		if (!layoutedWidget.Widget.LayoutInfo.IsVerticalText)
		{
			UpdateVerticalCellAlignment(layoutedWidget);
		}
		else
		{
			UpdateVerticalTextCellAlignment(layoutedWidget);
		}
		if (!layoutedWidget.Widget.LayoutInfo.IsVerticalText)
		{
			bounds = layoutedWidget.ChildWidgets[0].Bounds;
			bounds.Height = layoutedWidget.Bounds.Bottom - bounds.Top - (layoutedWidget.Widget.LayoutInfo as CellLayoutInfo).Margins.Bottom;
			layoutedWidget.ChildWidgets[0].Bounds = bounds;
		}
	}

	private void UpdateVerticalCellAlignment(LayoutedWidget cellLW)
	{
		RectangleF bounds = cellLW.Bounds;
		LayoutedWidget layoutedWidget = cellLW.ChildWidgets[0];
		float displacementValue = GetDisplacementValue(layoutedWidget, bounds.Bottom, isRemainingSpace: false, 0f);
		if (!(displacementValue <= 0f))
		{
			bool flag = false;
			float num = FindMaximumBottomOfFloattingItem(layoutedWidget);
			if (displacementValue > 0f && num < layoutedWidget.Bounds.Bottom && (!m_table.Document.DOP.Dop2000.Copts.DontVertAlignCellWithSp || !IsCellHavingShapes(layoutedWidget)))
			{
				flag = true;
				layoutedWidget.ShiftLocation(0.0, displacementValue, isPictureNeedToBeShifted: false, isFromFloatingItemVerticalAlignment: true, isNeedToShiftOwnerWidget: false);
			}
			else if (!m_table.Document.DOP.Dop2000.Copts.DontVertAlignCellWithSp && displacementValue > 0f && cellLW.Widget is WTableCell && bounds.Height > layoutedWidget.Bounds.Height && num > layoutedWidget.Bounds.Bottom && bounds.Bottom > num)
			{
				flag = true;
				float remainingSpace = cellLW.Bounds.Bottom - num;
				displacementValue = GetDisplacementValue(layoutedWidget, bounds.Height, isRemainingSpace: true, remainingSpace);
				layoutedWidget.ShiftLocation(0.0, displacementValue, isPictureNeedToBeShifted: false, isFromFloatingItemVerticalAlignment: true, isNeedToShiftOwnerWidget: false);
			}
			if (flag && (m_lcOperator as Layouter).TrackChangesMarkups.Count > 0 && m_table.Document.RevisionOptions.CommentDisplayMode == CommentDisplayMode.ShowInBalloons)
			{
				layoutedWidget.ShiftLocationOfCommentsMarkups(0f, displacementValue, (m_lcOperator as Layouter).TrackChangesMarkups);
			}
		}
	}

	private void UpdateVerticalTextCellAlignment(LayoutedWidget cellLW)
	{
		LayoutedWidget layoutedWidget = cellLW.ChildWidgets[0];
		CellLayoutInfo cellLayoutInfo = cellLW.Widget.LayoutInfo as CellLayoutInfo;
		if (cellLayoutInfo.VerticalAlignment != 0 && !(layoutedWidget.Widget is SplitWidgetContainer))
		{
			float width = cellLW.Bounds.Width;
			width = (((cellLW.Widget as WTableCell).Document.Settings.CompatibilityMode != CompatibilityMode.Word2013 && cellLayoutInfo.VerticalAlignment == DocGen.DocIO.DLS.VerticalAlignment.Middle) ? (width - (cellLayoutInfo.Paddings.Left + cellLayoutInfo.Paddings.Right)) : (width - (cellLayoutInfo.Margins.Left + cellLayoutInfo.Margins.Right + cellLayoutInfo.Paddings.Left + cellLayoutInfo.Paddings.Right)));
			float num = 0f;
			switch (cellLayoutInfo.VerticalAlignment)
			{
			case DocGen.DocIO.DLS.VerticalAlignment.Middle:
				num = (width - layoutedWidget.Bounds.Height) / 2f;
				break;
			case DocGen.DocIO.DLS.VerticalAlignment.Bottom:
				num = width - layoutedWidget.Bounds.Height;
				break;
			}
			if (!(num <= 0f))
			{
				layoutedWidget.ShiftLocation(0.0, num, isPictureNeedToBeShifted: false, isFromFloatingItemVerticalAlignment: true);
			}
		}
	}

	private float GetDisplacementValue(LayoutedWidget child, float cellBottom, bool isRemainingSpace, float remainingSpace)
	{
		CellLayoutInfo cellLayoutInfo = child.Widget.LayoutInfo as CellLayoutInfo;
		if (child.Widget.LayoutInfo.IsVerticalText || !(child.Widget is SplitWidgetContainer) || (isRowMoved && !cellLayoutInfo.IsRowMergeStart && !cellLayoutInfo.IsRowMergeContinue && !cellLayoutInfo.IsRowMergeEnd))
		{
			switch ((child.Widget.LayoutInfo as CellLayoutInfo).VerticalAlignment)
			{
			case DocGen.DocIO.DLS.VerticalAlignment.Middle:
				if (!isRemainingSpace)
				{
					return (cellBottom - (child.Widget.LayoutInfo as CellLayoutInfo).Margins.Bottom - child.Bounds.Bottom) / 2f;
				}
				return remainingSpace / 2f;
			case DocGen.DocIO.DLS.VerticalAlignment.Bottom:
				if (!isRemainingSpace)
				{
					return cellBottom - (child.Widget.LayoutInfo as CellLayoutInfo).Margins.Bottom - child.Bounds.Bottom;
				}
				return remainingSpace;
			}
		}
		return 0f;
	}

	private bool IsCellHavingShapes(LayoutedWidget child)
	{
		for (int i = 0; i < child.ChildWidgets.Count; i++)
		{
			for (int j = 0; j < child.ChildWidgets[i].ChildWidgets.Count; j++)
			{
				if (!(child.ChildWidgets[i].ChildWidgets[j].Widget is WParagraph) && (!(child.ChildWidgets[i].ChildWidgets[j].Widget is SplitWidgetContainer) || !((child.ChildWidgets[i].ChildWidgets[j].Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)))
				{
					continue;
				}
				for (int k = 0; k < child.ChildWidgets[i].ChildWidgets[j].ChildWidgets.Count; k++)
				{
					WPicture wPicture = child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Widget as WPicture;
					Shape shape = child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Widget as Shape;
					GroupShape groupShape = child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Widget as GroupShape;
					WTextBox wTextBox = child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Widget as WTextBox;
					if (child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Widget is ShapeObject { AllowInCell: not false } || shape != null || groupShape != null || wTextBox != null || (wPicture != null && wPicture.TextWrappingStyle != 0))
					{
						return true;
					}
					_ = wPicture?.TextWrappingStyle;
				}
			}
		}
		return false;
	}

	private float FindMaximumBottomOfFloattingItem(LayoutedWidget child)
	{
		float num = float.MinValue;
		WTableCell wTableCell = ((child.Widget is WTableCell) ? (child.Widget as WTableCell) : ((child.Widget as SplitWidgetContainer).RealWidgetContainer as WTableCell));
		int num2 = 0;
		int num3 = 0;
		if (wTableCell != null)
		{
			num3 = wTableCell.OwnerRow.GetIndexInOwnerCollection();
			num2 = wTableCell.GetIndexInOwnerCollection();
		}
		for (int i = 0; i < child.ChildWidgets.Count; i++)
		{
			for (int j = 0; j < child.ChildWidgets[i].ChildWidgets.Count; j++)
			{
				if (!(child.ChildWidgets[i].ChildWidgets[j].Widget is WParagraph) && (!(child.ChildWidgets[i].ChildWidgets[j].Widget is SplitWidgetContainer) || !((child.ChildWidgets[i].ChildWidgets[j].Widget as SplitWidgetContainer).RealWidgetContainer is WParagraph)))
				{
					continue;
				}
				for (int k = 0; k < child.ChildWidgets[i].ChildWidgets[j].ChildWidgets.Count; k++)
				{
					WPicture wPicture = child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Widget as WPicture;
					Shape shape = child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Widget as Shape;
					WTextBox wTextBox = child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Widget as WTextBox;
					WChart wChart = child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Widget as WChart;
					GroupShape groupShape = child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Widget as GroupShape;
					DocGen.Drawing.Font font = ((m_table.Rows[num3].Cells[num2].LastParagraph != null) ? m_table.Rows[num3].Cells[num2].LastParagraph.BreakCharacterFormat.GetFontToRender(FontScriptType.English) : m_table.Rows[num3].Cells[num2].CharacterFormat.GetFontToRender(FontScriptType.English));
					SizeF sizeF = base.DrawingContext.MeasureString(" ", font, null, FontScriptType.English);
					RectangleF cellBounds = new RectangleF(child.Bounds.X, child.Bounds.Y, GetCellWidth(num3, num2), GetCellHeight(num3, num2, sizeF.Height));
					if (((wPicture != null && wPicture.LayoutInCell && IsLayoutedWidgetNeedToBeShifted(wPicture.TextWrappingStyle, cellBounds, child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Bounds, wPicture.DistanceFromRight, wPicture.DistanceFromLeft, wPicture.DistanceFromTop, wPicture.DistanceFromBottom, wPicture.Document.Settings.CompatibilityMode, wPicture.LayoutInCell)) || (shape != null && shape.LayoutInCell && IsLayoutedWidgetNeedToBeShifted(shape.WrapFormat.TextWrappingStyle, cellBounds, child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Bounds, shape.WrapFormat.DistanceRight, shape.WrapFormat.DistanceLeft, shape.WrapFormat.DistanceTop, shape.WrapFormat.DistanceBottom, shape.Document.Settings.CompatibilityMode, shape.LayoutInCell)) || (groupShape != null && groupShape.LayoutInCell && IsLayoutedWidgetNeedToBeShifted(groupShape.WrapFormat.TextWrappingStyle, cellBounds, child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Bounds, groupShape.WrapFormat.DistanceRight, groupShape.WrapFormat.DistanceLeft, groupShape.WrapFormat.DistanceTop, groupShape.WrapFormat.DistanceBottom, groupShape.Document.Settings.CompatibilityMode, groupShape.LayoutInCell)) || (wChart != null && wChart.LayoutInCell && IsLayoutedWidgetNeedToBeShifted(wChart.WrapFormat.TextWrappingStyle, cellBounds, child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Bounds, wChart.WrapFormat.DistanceRight, wChart.WrapFormat.DistanceLeft, wChart.WrapFormat.DistanceTop, wChart.WrapFormat.DistanceBottom, wChart.Document.Settings.CompatibilityMode, wChart.LayoutInCell)) || (wTextBox != null && wTextBox.OwnerParagraph != null && wTextBox.OwnerParagraph.IsInCell && IsLayoutedWidgetNeedToBeShifted(wTextBox.TextBoxFormat.TextWrappingStyle, cellBounds, child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Bounds, wTextBox.TextBoxFormat.WrapDistanceRight, wTextBox.TextBoxFormat.WrapDistanceLeft, wTextBox.TextBoxFormat.WrapDistanceTop, wTextBox.TextBoxFormat.WrapDistanceBottom, wTextBox.Document.Settings.CompatibilityMode, wTextBox.TextBoxFormat.AllowInCell))) && num < child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Bounds.Bottom)
					{
						num = child.ChildWidgets[i].ChildWidgets[j].ChildWidgets[k].Bounds.Bottom;
					}
				}
			}
		}
		return num;
	}

	private bool IsLayoutedWidgetNeedToBeShifted(TextWrappingStyle textWrappingStyle, RectangleF cellBounds, RectangleF floatingItemBounds, float distanceFromRight, float distanceFromLeft, float distanceFromTop, float distanceFromBottom, CompatibilityMode mode, bool isLayoutInCell)
	{
		floatingItemBounds = new RectangleF(floatingItemBounds.X - distanceFromLeft, floatingItemBounds.Y + distanceFromTop, floatingItemBounds.Width + (distanceFromLeft + distanceFromRight), floatingItemBounds.Height + (distanceFromTop + distanceFromBottom));
		if (textWrappingStyle != 0 && ((mode != CompatibilityMode.Word2013 && isLayoutInCell) || mode == CompatibilityMode.Word2013) && floatingItemBounds.IntersectsWith(cellBounds))
		{
			return true;
		}
		return false;
	}

	private void CreateTableClientArea(ref RectangleF rect)
	{
		_ = TableLayoutInfo;
		Paddings cellPadding = new Paddings();
		CorrectTableClientArea(ref rect);
		CreateLayoutArea(rect, cellPadding);
	}

	private LayoutedWidget LayoutCell(LayoutContext childContext, RectangleF cellArea, bool isSkip)
	{
		CellLayoutInfo cellLayoutInfo = childContext.LayoutInfo as CellLayoutInfo;
		int count = (m_lcOperator as Layouter).FloatingItems.Count;
		RowLayoutInfo rowLayoutInfo = m_currRowLW.Widget.LayoutInfo as RowLayoutInfo;
		LayoutedWidget layoutedWidget = new LayoutedWidget(childContext.Widget, cellArea.Location);
		if (!isSkip && !rowLayoutInfo.IsHiddenRow)
		{
			layoutedWidget = childContext.Layout(cellArea);
		}
		if (DocumentLayouter.IsEndPage)
		{
			DocumentLayouter.IsEndPage = false;
			if (layoutedWidget.ChildWidgets.Count == 1 && layoutedWidget.ChildWidgets[0].TextTag == "Splitted" && !base.IsForceFitLayout && layoutedWidget.ChildWidgets[0].ChildWidgets.Count < 3)
			{
				m_ltState = LayoutState.NotFitted;
			}
		}
		if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
		{
			return null;
		}
		for (int num = (m_lcOperator as Layouter).FloatingItems.Count - 1; num >= count; num--)
		{
			TextWrappingStyle textWrappingStyle = (m_lcOperator as Layouter).FloatingItems[num].TextWrappingStyle;
			RectangleF textWrappingBounds = (m_lcOperator as Layouter).FloatingItems[num].TextWrappingBounds;
			if (textWrappingBounds.Width > cellArea.Width && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind)
			{
				textWrappingBounds.Width = cellArea.Width;
				(m_lcOperator as Layouter).FloatingItems[num].TextWrappingBounds = textWrappingBounds;
			}
			if (CompareOwnerOfTableCell(m_currRowLW.Widget as WTableRow, (m_lcOperator as Layouter).FloatingItems[num]) && textWrappingBounds.Bottom > layoutedWidget.Bounds.Bottom && layoutedWidget.Bounds.Right > textWrappingBounds.X && textWrappingStyle != TextWrappingStyle.InFrontOfText && textWrappingStyle != TextWrappingStyle.Behind && (m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 || (m_lcOperator as Layouter).FloatingItems[num].LayoutInCell || ((m_lcOperator as Layouter).FloatingItems[num].FloatingEntity is WTable && ((m_lcOperator as Layouter).FloatingItems[num].FloatingEntity as WTable).TableFormat.Positioning.DistanceFromBottom > 0f)))
			{
				float num2 = textWrappingBounds.Bottom - layoutedWidget.Bounds.Bottom;
				layoutedWidget.Bounds = new RectangleF(layoutedWidget.Bounds.X, layoutedWidget.Bounds.Y, layoutedWidget.Bounds.Width, layoutedWidget.Bounds.Height + num2);
			}
		}
		layoutedWidget.Bounds = new RectangleF(cellLayoutInfo.CellContentLayoutingBounds.X, cellLayoutInfo.CellContentLayoutingBounds.Y, cellLayoutInfo.CellContentLayoutingBounds.Width, layoutedWidget.Bounds.Height);
		LayoutedWidget layoutedWidget2 = new LayoutedWidget(childContext.Widget, cellArea.Location);
		layoutedWidget2.ChildWidgets.Add(layoutedWidget);
		layoutedWidget.Owner = layoutedWidget2;
		float num3 = cellLayoutInfo.Margins.Bottom;
		if (isSkip)
		{
			if (cellLayoutInfo.IsRowMergeContinue && rowLayoutInfo.IsExactlyRowHeight)
			{
				layoutedWidget2.Bounds = new RectangleF(cellArea.X, cellArea.Y, cellArea.Width, cellArea.Height);
			}
			else
			{
				layoutedWidget2.Bounds = new RectangleF(cellArea.X, cellArea.Y, cellArea.Width, 0f);
			}
			if (cellLayoutInfo.IsRowMergeStart || cellLayoutInfo.IsRowMergeContinue)
			{
				if (!rowLayoutInfo.IsRowHasVerticalMergeContinueCell)
				{
					rowLayoutInfo.IsRowHasVerticalMergeContinueCell = cellLayoutInfo.IsRowMergeContinue;
				}
				if (!rowLayoutInfo.IsRowHasVerticalMergeEndCell)
				{
					rowLayoutInfo.IsRowHasVerticalMergeEndCell = cellLayoutInfo.IsRowMergeEnd;
				}
				if (!rowLayoutInfo.IsRowHasVerticalMergeStartCell)
				{
					rowLayoutInfo.IsRowHasVerticalMergeStartCell = cellLayoutInfo.IsRowMergeStart;
				}
				if (cellLayoutInfo.IsRowMergeStart)
				{
					m_verticallyMergeStartLW.Add(layoutedWidget2);
				}
			}
			if (!rowLayoutInfo.IsExactlyRowHeight)
			{
				m_splitedCells[m_currColIndex] = new SplitWidgetContainer(childContext.Widget as IWidgetContainer);
			}
			if (cellLayoutInfo.IsRowMergeStart && cellLayoutInfo.IsCellHasEndNote && childContext.Widget is WTextBody)
			{
				AddVerticallyMergedCellFootNote(childContext.Widget as WTextBody);
			}
			if ((cellLayoutInfo.IsRowMergeStart || cellLayoutInfo.IsVerticalText) && cellLayoutInfo.IsCellHasFootNote && !rowLayoutInfo.IsFootnoteReduced)
			{
				LayoutContext layoutContext = LayoutContext.Create(childContext.Widget, m_lcOperator, base.IsForceFitLayout);
				(m_lcOperator as Layouter).IsLayoutingVerticalMergeStartCell = true;
				LayoutedWidget layoutedWidget3 = layoutContext.Layout(cellArea);
				(m_lcOperator as Layouter).IsLayoutingVerticalMergeStartCell = false;
				if (DocumentLayouter.IsUpdatingTOC && DocumentLayouter.IsEndUpdateTOC)
				{
					return null;
				}
				if (m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013)
				{
					rowLayoutInfo.IsFootnoteSplitted = CheckFootnoteInRowIsSplitted(layoutContext);
				}
				layoutedWidget3.GetFootnoteHeight(ref m_verticallyMergedCellFootnoteHeight);
			}
		}
		else
		{
			layoutedWidget2.Bounds = new RectangleF(cellArea.X, cellArea.Y, cellArea.Width, (rowLayoutInfo.IsExactlyRowHeight || cellLayoutInfo.IsVerticalText) ? cellArea.Height : (layoutedWidget.Bounds.Bottom - cellArea.Top + num3));
		}
		return layoutedWidget2;
	}

	private bool CompareOwnerOfTableCell(WTableRow CurrentRow, FloatingItem Item)
	{
		Entity baseEntity = GetBaseEntity(Item.FloatingEntity);
		WTableRow wTableRow = ((baseEntity is WTableCell) ? (baseEntity as WTableCell).GetOwnerRow(baseEntity as WTableCell) : null);
		if (baseEntity != null && baseEntity is WTableCell)
		{
			return wTableRow == CurrentRow;
		}
		return false;
	}

	private void AddVerticallyMergedCellFootNote(WTextBody textBody)
	{
		for (int i = 0; i < textBody.Items.Count; i++)
		{
			if (textBody.Items[i] is WParagraph)
			{
				AddVerticallyMergedCellFootNote(textBody.Items[i] as WParagraph);
			}
			else
			{
				if (!(textBody.Items[i] is WTable))
				{
					continue;
				}
				WTable wTable = textBody.Items[i] as WTable;
				for (int j = 0; j < wTable.Rows.Count; j++)
				{
					WTableRow wTableRow = wTable.Rows[j];
					for (int k = 0; k < wTableRow.Cells.Count; k++)
					{
						AddVerticallyMergedCellFootNote(wTableRow.Cells[k]);
					}
				}
			}
		}
	}

	private void AddVerticallyMergedCellFootNote(WParagraph paragraph)
	{
		for (int i = 0; i < paragraph.Items.Count; i++)
		{
			if (paragraph.Items[i] is WFootnote && (paragraph.Items[i] as WFootnote).FootnoteType == FootnoteType.Endnote && !(m_lcOperator as Layouter).EndnotesInstances.Contains(paragraph.Items[i]))
			{
				(m_lcOperator as Layouter).EndnotesInstances.Add(paragraph.Items[i]);
			}
		}
	}

	private LayoutContext CreateNextCellContext()
	{
		while ((m_table != null && m_table.TableFormat.Bidi && m_currColIndex - 1 >= 0) || (!m_table.TableFormat.Bidi && m_currColIndex + 1 < m_table.Rows[CurrRowIndex].Cells.Count))
		{
			if (m_table.TableFormat.Bidi)
			{
				m_currColIndex--;
			}
			else
			{
				m_currColIndex++;
			}
			IWidgetContainer widgetContainer = null;
			if (m_spitTableWidget != null && m_spitTableWidget.SplittedCells != null && CurrRowIndex == m_spitTableWidget.StartRowNumber - 1 && m_currColIndex < m_spitTableWidget.SplittedCells.Length)
			{
				widgetContainer = m_spitTableWidget.SplittedCells[m_currColIndex];
				if (widgetContainer == null)
				{
					widgetContainer = new SplitWidgetContainer(TableWidget.GetCellWidget(CurrRowIndex, m_currColIndex));
				}
			}
			if (widgetContainer == null)
			{
				widgetContainer = TableWidget.GetCellWidget(CurrRowIndex, m_currColIndex);
			}
			if (widgetContainer == null || widgetContainer.LayoutInfo == null || !widgetContainer.LayoutInfo.IsSkip)
			{
				return LayoutContext.Create(widgetContainer, m_lcOperator, base.IsForceFitLayout);
			}
		}
		return null;
	}

	private void SaveChildContextState(LayoutContext childContext)
	{
		switch (childContext.State)
		{
		case LayoutState.Unknown:
			m_currRowLW.ChildWidgets.Add(m_currCellLW);
			m_currCellLW.Owner = m_currRowLW;
			m_bAtLastOneCellFitted = true;
			break;
		case LayoutState.NotFitted:
			MarkAsNotFitted(childContext);
			break;
		case LayoutState.Splitted:
			MarkAsSplitted(childContext);
			break;
		case LayoutState.Fitted:
			MarkAsFitted(childContext);
			break;
		case LayoutState.Breaked:
			MarkAsBreaked(childContext);
			break;
		case LayoutState.WrapText:
			break;
		}
	}

	private void MarkAsSplitted(LayoutContext childContext)
	{
		if (m_ltState == LayoutState.NotFitted)
		{
			CommitKeepWithNext();
		}
		RowLayoutInfo rowLayoutInfo = m_currRowLW.Widget.LayoutInfo as RowLayoutInfo;
		if (rowLayoutInfo.IsExactlyRowHeight || childContext.LayoutInfo.IsVerticalText)
		{
			MarkAsFitted(childContext);
			m_ltState = LayoutState.Unknown;
			return;
		}
		rowLayoutInfo.IsRowSplitted = true;
		MarkAsFitted(childContext);
		if (m_blastRowState == LayoutState.Unknown && !(m_currCellLW.Widget.LayoutInfo as CellLayoutInfo).IsRowMergeStart)
		{
			m_blastRowState = LayoutState.Splitted;
			(TableLayoutInfo as ITableLayoutInfo).IsSplittedTable = true;
		}
	}

	protected virtual void MarkAsBreaked(LayoutContext childContext)
	{
		RowLayoutInfo rowLayoutInfo = m_currRowLW.Widget.LayoutInfo as RowLayoutInfo;
		if (rowLayoutInfo.IsExactlyRowHeight)
		{
			m_splitedCells[m_currColIndex] = ((childContext.SplittedWidget is SplitWidgetContainer) ? (childContext.SplittedWidget as SplitWidgetContainer) : new SplitWidgetContainer(childContext.Widget as IWidgetContainer));
			rowLayoutInfo.IsRowSplitted = true;
		}
		m_currRowLW.ChildWidgets.Add(m_currCellLW);
		m_currCellLW.Owner = m_currRowLW;
		m_bAtLastOneCellFitted = true;
		m_blastRowState = LayoutState.Breaked;
	}

	private void MarkAsNotFitted(LayoutContext childContext)
	{
		RowLayoutInfo rowLayoutInfo = m_currRowLW.Widget.LayoutInfo as RowLayoutInfo;
		if (!rowLayoutInfo.IsExactlyRowHeight)
		{
			CommitKeepWithNext();
		}
		if (childContext.IsVerticalNotFitted)
		{
			if (rowLayoutInfo.IsExactlyRowHeight)
			{
				MarkAsFitted(childContext);
			}
			else
			{
				m_ltState = LayoutState.NotFitted;
			}
		}
		else if (CurrRowIndex < TableWidget.RowsCount - 1)
		{
			MarkAsSplitted(childContext);
			m_ltState = LayoutState.NotFitted;
		}
		else
		{
			m_ltState = LayoutState.NotFitted;
		}
	}

	private bool CommitKeepWithNext()
	{
		bool isAllItemsInPageHavingKeepWihtNext = false;
		bool flag = IsNeedToCommitKeepWithNext(ref isAllItemsInPageHavingKeepWihtNext);
		bool flag2 = !IsFirstItemInPage && IsWord2013(m_table.Document) && IsHeaderRow(m_table.LastRow);
		while (m_ltWidget.ChildWidgets.Count > 0 && !(m_lcOperator as Layouter).IsLayoutingHeaderFooter && (flag || (isAllItemsInPageHavingKeepWihtNext && IsWord2013(m_table.Document)) || flag2) && ((m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1].Widget.LayoutInfo.IsKeepWithNext && !base.IsForceFitLayout) || flag2))
		{
			LayoutedWidget layoutedWidget = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1];
			RowLayoutInfo rowLayoutInfo = layoutedWidget.Widget.LayoutInfo as RowLayoutInfo;
			if (rowLayoutInfo.IsRowHasVerticalMergeStartCell && !flag2)
			{
				for (int i = 0; i < layoutedWidget.ChildWidgets.Count; i++)
				{
					CellLayoutInfo cellLayoutInfo = layoutedWidget.ChildWidgets[i].Widget.LayoutInfo as CellLayoutInfo;
					if (cellLayoutInfo.IsRowMergeStart && cellLayoutInfo.IsRowMergeEnd)
					{
						cellLayoutInfo.IsRowMergeEnd = false;
					}
				}
			}
			rowLayoutInfo.IsRowHasVerticalMergeStartCell = false;
			rowLayoutInfo.IsRowHasVerticalMergeEndCell = false;
			rowLayoutInfo.IsRowHasVerticalMergeContinueCell = false;
			RemoveBehindWidgets(m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1]);
			m_ltWidget.ChildWidgets.RemoveAt(m_ltWidget.ChildWidgets.Count - 1);
			if (m_splitedCells != null && m_splitedCells.Length != 0)
			{
				Array.Clear(m_splitedCells, 0, m_splitedCells.Length);
			}
			m_currRowIndex--;
		}
		return isAllItemsInPageHavingKeepWihtNext;
	}

	private bool IsNeedToCommitKeepWithNext(ref bool isAllItemsInPageHavingKeepWihtNext)
	{
		bool flag = false;
		bool flag2 = IsContainsKeepLines();
		for (int i = 0; i < m_ltWidget.ChildWidgets.Count; i++)
		{
			IWidget widget = m_ltWidget.ChildWidgets[i].Widget;
			WTableRow wTableRow = widget as WTableRow;
			if (widget.LayoutInfo.IsKeepWithNext)
			{
				continue;
			}
			bool num;
			if (!IsWord2013(wTableRow.Document))
			{
				if (m_isTableSplitted)
				{
					num = !wTableRow.IsHeader;
					goto IL_0071;
				}
			}
			else
			{
				if (wTableRow.IsHeader)
				{
					continue;
				}
				if (!m_isTableSplitted)
				{
					num = flag2;
					goto IL_0071;
				}
			}
			goto IL_0073;
			IL_0071:
			if (!num)
			{
				continue;
			}
			goto IL_0073;
			IL_0073:
			flag = true;
			break;
		}
		if (!flag && m_ltWidget.ChildWidgets.Count > 0 && !IsFirstItemInPage && m_ltWidget.ChildWidgets[0].Widget as WTableRow == m_table.Rows[0] && !m_table.TableFormat.WrapTextAround && !m_table.IsInCell)
		{
			bool flag3 = false;
			Entity entity = m_table.PreviousSibling as Entity;
			while (entity != null && (!(entity is WTable) || !(entity as WTable).TableFormat.WrapTextAround))
			{
				if (!(entity as TextBodyItem).m_layoutInfo.IsKeepWithNext)
				{
					flag3 = false;
					flag = true;
					break;
				}
				flag3 = true;
				if ((entity as TextBodyItem).m_layoutInfo.IsFirstItemInPage)
				{
					flag3 = false;
					isAllItemsInPageHavingKeepWihtNext = true;
					break;
				}
				entity = entity.PreviousSibling as Entity;
			}
			Entity baseEntity = GetBaseEntity(base.Widget as Entity);
			WSection wSection = ((baseEntity != null) ? (baseEntity as WSection) : null);
			if (wSection != null && wSection.BreakCode == SectionBreakCode.NoBreak && ((wSection.Index > 0 && flag3) || m_table.Index == 0))
			{
				flag = true;
			}
		}
		return flag;
	}

	private bool IsContainsKeepLines()
	{
		for (int i = 0; i < m_ltWidget.ChildWidgets.Count; i++)
		{
			WTableRow wTableRow = m_ltWidget.ChildWidgets[i].Widget as WTableRow;
			if (wTableRow.Cells.Count > 0 && wTableRow.Cells[0].ChildEntities != null && wTableRow.Cells[0].ChildEntities.FirstItem is WParagraph && (wTableRow.Cells[0].ChildEntities.FirstItem as WParagraph).ParagraphFormat.Keep)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsPreviousRowHasVerticalMergeContinueCell(WTableRow PreviousRow)
	{
		for (int i = 0; i < PreviousRow.ChildEntities.Count; i++)
		{
			CellLayoutInfo cellLayoutInfo = ((WTableCell)PreviousRow.ChildEntities.InnerList[i]).m_layoutInfo as CellLayoutInfo;
			if (cellLayoutInfo.IsRowMergeContinue && !cellLayoutInfo.IsRowMergeEnd)
			{
				return true;
			}
		}
		return false;
	}

	private void MarkAsFitted(LayoutContext childContext)
	{
		if (!(m_currRowLW.Widget.LayoutInfo as RowLayoutInfo).IsExactlyRowHeight)
		{
			m_splitedCells[m_currColIndex] = ((childContext.SplittedWidget is SplitWidgetContainer) ? (childContext.SplittedWidget as SplitWidgetContainer) : new SplitWidgetContainer(childContext.Widget as IWidgetContainer));
		}
		m_currRowLW.ChildWidgets.Add(m_currCellLW);
		m_currCellLW.Owner = m_currRowLW;
		UpdateRowLWBounds();
		m_bAtLastOneCellFitted = true;
	}

	private void UpdateClientArea()
	{
		float height = 0f;
		if (((m_currRowLW.Widget as WTableRow).m_layoutInfo as RowLayoutInfo).IsFootnoteReduced)
		{
			m_layoutArea.CutFromTop(m_currRowLW.Bounds.Bottom, height, IsInsideClippableItem());
			((m_currRowLW.Widget as WTableRow).m_layoutInfo as RowLayoutInfo).IsFootnoteReduced = false;
		}
		else
		{
			GetFootnoteHeight(ref height);
			m_layoutArea.CutFromTop(m_currRowLW.Bounds.Bottom, height, IsInsideClippableItem());
		}
	}

	private bool IsInsideClippableItem()
	{
		if (m_table.IsFrame)
		{
			WParagraph wParagraph = m_table.Rows[0].Cells[0].Paragraphs[0];
			if (wParagraph != null && wParagraph.ParagraphFormat.IsFrame && wParagraph.ParagraphFormat.FrameHeight != 0f)
			{
				return ((ushort)(wParagraph.ParagraphFormat.FrameHeight * 20f) & 0x8000) == 0;
			}
		}
		Entity entity = ((m_table.OwnerTextBody != null) ? m_table.OwnerTextBody.Owner : null);
		if (entity != null)
		{
			if (entity is WTextBox)
			{
				WTextBox wTextBox = entity as WTextBox;
				if (!wTextBox.TextBoxFormat.AutoFit)
				{
					if (wTextBox.IsShape)
					{
						return !wTextBox.Shape.TextFrame.ShapeAutoFit;
					}
					return true;
				}
				return false;
			}
			if (entity is Shape)
			{
				return !(entity as Shape).TextFrame.ShapeAutoFit;
			}
			if (entity is ChildShape)
			{
				return !(entity as ChildShape).TextFrame.ShapeAutoFit;
			}
		}
		return false;
	}

	private void GetFootnoteHeight(ref float height)
	{
		LayoutedWidget layoutedWidget = m_ltWidget.ChildWidgets[m_ltWidget.ChildWidgets.Count - 1];
		for (int i = 0; i < layoutedWidget.ChildWidgets.Count; i++)
		{
			LayoutedWidget childParagraphWidgets = GetChildParagraphWidgets(layoutedWidget.ChildWidgets[i]);
			if (childParagraphWidgets == null)
			{
				continue;
			}
			for (int j = 0; j < childParagraphWidgets.ChildWidgets.Count; j++)
			{
				for (int k = 0; k < childParagraphWidgets.ChildWidgets[j].ChildWidgets.Count; k++)
				{
					if (childParagraphWidgets.ChildWidgets[j].ChildWidgets[k].Widget is WFootnote)
					{
						height += (childParagraphWidgets.ChildWidgets[j].ChildWidgets[k].Widget.LayoutInfo as FootnoteLayoutInfo).FootnoteHeight;
					}
					else if (childParagraphWidgets.ChildWidgets[j].ChildWidgets[k].Widget is IWidgetContainer)
					{
						childParagraphWidgets.ChildWidgets[j].ChildWidgets[k].GetFootnoteHeight(ref height);
					}
				}
			}
		}
	}

	private float GetRowWidth(WTableRow ownerRow)
	{
		float num = 0f;
		for (int i = 0; i < ownerRow.Cells.Count; i++)
		{
			num += GetCellWidth(ownerRow.Index, i);
		}
		return num;
	}

	private LayoutArea GetCellClientArea(CellLayoutInfo cellInfo, int rowIndex, int columnIndex, float maxCellsTopPadding, float maxCellsTopMargin)
	{
		RectangleF clientActiveArea = m_rowLayoutArea.ClientActiveArea;
		float num = 0f;
		float width = m_table.Width;
		float num2 = ((m_table.Rows[rowIndex].RowFormat.CellSpacing > 0f) ? m_table.Rows[rowIndex].RowFormat.CellSpacing : ((m_table.TableFormat.CellSpacing > 0f) ? m_table.TableFormat.CellSpacing : 0f));
		double num3 = GetCellWidth(rowIndex, columnIndex);
		if (m_currRowLW.ChildWidgets.Count > 0)
		{
			num = m_currRowLW.ChildWidgets[m_currRowLW.ChildWidgets.Count - 1].Bounds.Right + num2;
		}
		double num4 = ((num != 0f) ? ((double)num) : ((double)clientActiveArea.X));
		double num5 = clientActiveArea.Y;
		double num6 = ((!(clientActiveArea.Height > m_layoutArea.ClientArea.Height) || (m_currRowLW.Widget as WTableRow).Document.Settings.CompatibilityMode != CompatibilityMode.Word2010 || (m_currRowLW.Widget as WTableRow).HeightType != TableRowHeightType.Exactly) ? ((double)clientActiveArea.Height) : ((double)m_currRowLW.Bounds.Height));
		if (cellInfo.IsColumnMergeStart)
		{
			num3 = GetCellMergedWidth(rowIndex, columnIndex);
		}
		if (num2 > 0f)
		{
			num5 += (double)(maxCellsTopPadding + maxCellsTopMargin - (cellInfo.Margins.Top + cellInfo.Paddings.Top));
			num6 -= (double)(maxCellsTopPadding + maxCellsTopMargin - (cellInfo.Margins.Top + cellInfo.Paddings.Top));
			if ((!m_table.TableFormat.Bidi && columnIndex == 0) || (m_table.TableFormat.Bidi && columnIndex == m_table.Rows[rowIndex].Cells.Count - 1))
			{
				num4 += (double)(num2 * 2f);
				num3 -= (double)(num2 * 3f);
			}
			else if ((!m_table.TableFormat.Bidi && columnIndex == m_table.Rows[rowIndex].Cells.Count - 1) || (m_table.TableFormat.Bidi && columnIndex == 0))
			{
				num4 += (double)num2;
				num3 -= (double)(num2 * 3f);
			}
			else
			{
				num4 += (double)num2;
				num3 -= (double)(num2 * 2f);
			}
		}
		if (m_table != null && m_table.TableFormat.Bidi && columnIndex == m_table.Rows[rowIndex].Cells.Count - 1)
		{
			float rowWidth = GetRowWidth(m_table.Rows[rowIndex]);
			float maxRowLeftIndent = GetMaxRowLeftIndent();
			width -= maxRowLeftIndent;
			if (Math.Round(rowWidth, 2) < Math.Round(width, 2))
			{
				float num7 = width - rowWidth;
				num4 += (double)num7;
			}
		}
		return new LayoutArea(new RectangleF((float)num4, (float)num5, (float)num3, (float)num6));
	}

	private float GetMaxRowLeftIndent()
	{
		float num = 0f;
		foreach (WTableRow row in m_table.Rows)
		{
			if (row.RowFormat.HorizontalAlignment == RowAlignment.Left && num < Math.Abs(row.RowFormat.LeftIndent))
			{
				num = Math.Abs(row.RowFormat.LeftIndent);
			}
		}
		return num;
	}

	private float GetCellWidth(int rowIndex, int colIndex)
	{
		float num = m_table.Rows[rowIndex].Cells[colIndex].Width;
		if (num == 0f)
		{
			CellLayoutInfo cellLayoutInfo = ((IWidget)m_table.Rows[rowIndex].Cells[colIndex]).LayoutInfo as CellLayoutInfo;
			if (cellLayoutInfo.IsColumnMergeContinue)
			{
				WTableCell wTableCell = m_table.Rows[rowIndex].Cells[colIndex];
				float leftPadding = wTableCell.GetLeftPadding();
				float rightPadding = wTableCell.GetRightPadding();
				num = leftPadding + rightPadding;
			}
			else
			{
				num = cellLayoutInfo.Paddings.Left + cellLayoutInfo.Paddings.Right + cellLayoutInfo.Margins.Left + cellLayoutInfo.Margins.Right;
			}
		}
		return num;
	}

	private float GetCellHeight(int rowIndex, int colIndex, float cellMinHeight)
	{
		float num = m_table.Rows[rowIndex].Height;
		if (num <= 0f)
		{
			num = cellMinHeight;
		}
		return num + ((((IWidget)m_table.Rows[rowIndex].Cells[colIndex]).LayoutInfo as CellLayoutInfo).Paddings.Top + (((IWidget)m_table.Rows[rowIndex].Cells[colIndex]).LayoutInfo as CellLayoutInfo).Paddings.Bottom + (((IWidget)m_table.Rows[rowIndex].Cells[colIndex]).LayoutInfo as CellLayoutInfo).Margins.Top + (((IWidget)m_table.Rows[rowIndex].Cells[colIndex]).LayoutInfo as CellLayoutInfo).Margins.Bottom);
	}

	private float GetCellMergedWidth(int rowIndex, int colIndex)
	{
		float num = GetCellWidth(rowIndex, colIndex);
		WTableCell wTableCell = null;
		int num2 = colIndex + 1;
		if (num2 < m_table.Rows[rowIndex].Cells.Count)
		{
			wTableCell = m_table.Rows[rowIndex].Cells[num2];
		}
		while (wTableCell != null && wTableCell.CellFormat.HorizontalMerge == CellMerge.Continue)
		{
			num += GetCellWidth(rowIndex, num2);
			num2++;
			wTableCell = ((num2 >= m_table.Rows[rowIndex].Cells.Count) ? null : m_table.Rows[rowIndex].Cells[num2]);
		}
		return num;
	}

	private void UpdateLWBounds()
	{
		RectangleF bounds = m_ltWidget.Bounds;
		bounds.Width = m_currRowLW.Bounds.Width + ((TableWidget.LayoutInfo as TableLayoutInfo).Paddings.Left + (TableWidget.LayoutInfo as TableLayoutInfo).Paddings.Right);
		bounds.Height = m_currRowLW.Bounds.Bottom - bounds.Top + (TableWidget.LayoutInfo as TableLayoutInfo).Paddings.Bottom;
		m_ltWidget.Bounds = bounds;
	}

	private void CorrectTableClientArea(ref RectangleF rect)
	{
		ITableLayoutInfo tableLayoutInfo = TableLayoutInfo as ITableLayoutInfo;
		bool flag = false;
		float num = 0f;
		int num2 = 0;
		float num3 = 0f;
		if (m_table.Document != null && m_table.Document.GrammarSpellingData == null && m_table.Rows[0].RowFormat.HorizontalAlignment == RowAlignment.Left)
		{
			num3 = m_table.IndentFromLeft;
		}
		int i = 0;
		for (int num4 = tableLayoutInfo.IsDefaultCells.Length; i < num4; i++)
		{
			if (tableLayoutInfo.IsDefaultCells[i])
			{
				flag = flag || true;
				num += tableLayoutInfo.CellsWidth[i];
				num2++;
			}
		}
		if (!flag && tableLayoutInfo.Width > rect.Width)
		{
			tableLayoutInfo.Width = rect.Width - (float)tableLayoutInfo.CellSpacings - (float)tableLayoutInfo.CellPaddings;
		}
		else if (num2 == m_table.Rows[CurrRowIndex + 1].Cells.Count)
		{
			num3 = 0f;
			tableLayoutInfo.Width = 0f;
			for (int j = 0; j < num2; j++)
			{
				tableLayoutInfo.Width += tableLayoutInfo.CellsWidth[j];
			}
		}
		RowFormat tableFormat = m_table.TableFormat;
		RowAlignment horizontalAlignment = GetHorizontalAlignment();
		MarginsF marginsF = InitializePageMargins();
		Entity baseEntity = GetBaseEntity(m_table);
		if (m_table.IndentFromLeft != float.MinValue && horizontalAlignment == RowAlignment.Left)
		{
			if (m_table.TableFormat.WrapTextAround)
			{
				if (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left)
				{
					if (m_table.IsInCell && rect.Width > tableLayoutInfo.Width)
					{
						rect.X = ((m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013) ? (rect.X + m_table.IndentFromLeft + (((IWidget)m_table.Rows[0].Cells[0]).LayoutInfo as CellLayoutInfo).Paddings.Left) : (rect.X + (((IWidget)m_table.Rows[0].Cells[0]).LayoutInfo as CellLayoutInfo).Paddings.Left));
					}
					else
					{
						rect.X = ((m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013) ? (rect.X + m_table.IndentFromLeft - LeftPad) : rect.X);
					}
				}
				else
				{
					rect.X = ((m_table.Document.Settings.CompatibilityMode != CompatibilityMode.Word2013) ? (rect.X + m_table.IndentFromLeft) : rect.X);
				}
			}
			else if (!tableFormat.Bidi)
			{
				if (m_table.IsInCell || m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013)
				{
					rect.X += m_table.IndentFromLeft;
				}
				else
				{
					rect.X += m_table.IndentFromLeft - LeftPad;
				}
			}
			else if (tableFormat.Bidi)
			{
				float num5 = ((!IsWord2013(m_table.Document)) ? GetRightPad(m_table.Rows[0].Cells[0]) : 0f);
				if (IsNeedToUpdateRTLTableXPosition(tableLayoutInfo, num3, rect))
				{
					rect.X += rect.Width - (tableLayoutInfo.Width - Math.Abs(num3) - num5);
					rect.X -= (m_table.TableFormat.WrapTextAround ? 0f : m_table.IndentFromLeft);
					if (m_table.TableFormat.WrapTextAround)
					{
						rect.X += GetMinimumRightPad() - num5;
					}
				}
			}
		}
		if (!(baseEntity is WSection) || (baseEntity as WSection).Columns.Count <= 1)
		{
			if (m_table.TableFormat.WrapTextAround && (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Right || m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Outside || (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Inside && (m_lcOperator as Layouter).CurrPageIndex % 2 == 0)) && m_table.TableFormat.Positioning.HorizRelationTo == HorizontalRelation.Page && !m_table.IsInCell && marginsF != null)
			{
				WSection section = baseEntity.GetOwnerSection(m_table) as WSection;
				if (tableLayoutInfo.Width + tableFormat.Paddings.Right < Layouter.GetRightMargin(section))
				{
					rect.X = (m_lcOperator as Layouter).ClientLayoutArea.Width + Layouter.GetLeftMargin(section);
				}
				else if (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Outside && (m_lcOperator as Layouter).CurrPageIndex % 2 == 0)
				{
					rect.X = 0f;
				}
				else
				{
					rect.X = (m_lcOperator as Layouter).ClientLayoutArea.Width + Layouter.GetLeftMargin(section) + Layouter.GetRightMargin(section) - tableLayoutInfo.Width - tableFormat.Paddings.Right;
				}
			}
			else if (m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left && m_table.TableFormat.Positioning.HorizPosition == 0f && m_table.TableFormat.Positioning.HorizRelationTo == HorizontalRelation.Page && !m_table.IsInCell && marginsF != null)
			{
				float leftMargin = Layouter.GetLeftMargin(baseEntity as WSection);
				if (tableLayoutInfo.Width + tableFormat.Paddings.Left + m_table.IndentFromLeft < leftMargin)
				{
					rect.X = leftMargin - tableLayoutInfo.Width - tableFormat.Paddings.Left - m_table.IndentFromLeft;
				}
				else
				{
					rect.X = m_table.IndentFromLeft;
				}
			}
			else if (horizontalAlignment == RowAlignment.Right || (m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Right) || (m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Outside && (m_lcOperator as Layouter).CurrPageIndex % 2 != 0))
			{
				if (!m_table.TableFormat.Bidi)
				{
					float num6 = (IsWord2013(m_table.Document) ? 0f : GetRightPad(m_table.Rows[0].Cells[m_table.Rows[0].Cells.Count - 1]));
					rect.X += rect.Width - (tableLayoutInfo.Width - num6);
					if (!IsWord2013(m_table.Document) && m_table.TableFormat.WrapTextAround)
					{
						rect.X += GetMinimumRightPad() - num6;
					}
				}
				else if (m_table.TableFormat.Bidi)
				{
					rect.X = rect.X;
				}
				if (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left)
				{
					rect.X -= m_table.TableFormat.Positioning.HorizPosition;
				}
			}
			else if (horizontalAlignment == RowAlignment.Center || (m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Center))
			{
				rect.X += (rect.Width - tableLayoutInfo.Width) / 2f;
				if (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left)
				{
					rect.X -= m_table.TableFormat.Positioning.HorizPosition;
				}
			}
		}
		else if (baseEntity is WSection && (baseEntity as WSection).Columns.Count > 1)
		{
			if (m_table.TableFormat.Positioning.HorizRelationTo == HorizontalRelation.Margin)
			{
				if (m_table.TableFormat.WrapTextAround && (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Right || (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Outside && (m_lcOperator as Layouter).CurrPageIndex % 2 == 0)) && !m_table.IsInCell && marginsF != null)
				{
					float num7 = 0f;
					if (!IsWord2013(m_table.Document) && m_table.Rows.Count > 0 && m_table.Rows[0].Cells.Count > 0)
					{
						num7 = GetRightPad(m_table.Rows[0].Cells[m_table.Rows[0].Cells.Count - 1]);
					}
					rect.X = (baseEntity as WSection).PageSetup.PageSize.Width - (Layouter.GetRightMargin(baseEntity as WSection) + (tableLayoutInfo.Width - num7));
				}
				else if (m_table.TableFormat.WrapTextAround && (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left || m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Inside) && m_table.TableFormat.Positioning.HorizPosition == 0f && !m_table.IsInCell && marginsF != null)
				{
					rect.X = Layouter.GetLeftMargin(baseEntity as WSection) + m_table.IndentFromLeft - LeftPad;
				}
				else if (m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left && m_table.TableFormat.Positioning.HorizPosition != 0f && m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && !m_table.IsInCell && marginsF != null)
				{
					rect.X = Layouter.GetLeftMargin(baseEntity as WSection) + m_table.TableFormat.Positioning.HorizPosition;
				}
				else if (m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Center)
				{
					rect.X = (baseEntity as WSection).PageSetup.PageSize.Width / 2f - m_table.Width / 2f;
				}
			}
			else if (m_table.TableFormat.Positioning.HorizRelationTo == HorizontalRelation.Column)
			{
				if (m_table.TableFormat.WrapTextAround && (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Right || m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Outside) && !(m_table.OwnerTextBody is WTableCell) && marginsF != null)
				{
					rect.X += (m_lcOperator as Layouter).ClientLayoutArea.Width - tableLayoutInfo.Width;
				}
				else if (m_table.TableFormat.WrapTextAround && (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left || m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Inside) && m_table.TableFormat.Positioning.HorizPosition == 0f && !m_table.IsInCell && marginsF != null)
				{
					rect.X -= (m_lcOperator as Layouter).ClientLayoutArea.Width - tableLayoutInfo.Width;
				}
				else if (m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Center)
				{
					rect.X += ((m_lcOperator as Layouter).ClientLayoutArea.Width - tableLayoutInfo.Width) / 2f;
				}
			}
			else if (m_table.TableFormat.Positioning.HorizRelationTo == HorizontalRelation.Page)
			{
				if (m_table.TableFormat.WrapTextAround && (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Right || m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Outside) && !m_table.IsInCell && marginsF != null)
				{
					rect.X = (baseEntity as WSection).PageSetup.PageSize.Width - tableLayoutInfo.Width;
				}
				else if (m_table.TableFormat.WrapTextAround && (m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left || m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Inside) && m_table.TableFormat.Positioning.HorizPosition == 0f && !m_table.IsInCell && marginsF != null)
				{
					rect.X = m_table.IndentFromLeft;
				}
				else if (m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Left && m_table.TableFormat.Positioning.HorizPosition > 0f && m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && !m_table.IsInCell && marginsF != null)
				{
					rect.X = m_table.TableFormat.Positioning.HorizPosition;
				}
				else if (m_table.TableFormat.WrapTextAround && m_table.TableFormat.Positioning.HorizPositionAbs == HorizontalPosition.Center)
				{
					rect.X = (baseEntity as WSection).PageSetup.PageSize.Width / 2f - m_table.Width / 2f;
				}
			}
		}
		if (m_table.IsInCell && m_table.GetOwnerTableCell().m_layoutInfo.IsVerticalText)
		{
			rect.Height = rect.Width;
		}
		rect.Width = tableLayoutInfo.Width;
		float num8 = (tableLayoutInfo.Width - num) / (float)(tableLayoutInfo.CellsWidth.Length - num2);
		int k = 0;
		for (int num9 = tableLayoutInfo.IsDefaultCells.Length; k < num9; k++)
		{
			if (!tableLayoutInfo.IsDefaultCells[k])
			{
				tableLayoutInfo.CellsWidth[k] = num8;
			}
		}
	}

	private bool IsNeedToUpdateRTLTableXPosition(ITableLayoutInfo info, float leftindent, RectangleF rect)
	{
		Entity baseEntity = GetBaseEntity(m_table);
		if (m_table.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && baseEntity is WSection && (baseEntity as WSection).PageSetup.Bidi && !m_table.TableFormat.WrapTextAround)
		{
			return !(info.Width - Math.Abs(leftindent) + m_table.IndentFromLeft > rect.Width);
		}
		return true;
	}

	private RowAlignment GetHorizontalAlignment()
	{
		RowAlignment horizontalAlignment = m_table.TableFormat.HorizontalAlignment;
		if (m_table.Rows[0].RowFormat.PropertiesHash.ContainsKey(105))
		{
			horizontalAlignment = m_table.Rows[0].RowFormat.HorizontalAlignment;
		}
		else if (m_table.TableFormat.PropertiesHash.ContainsKey(105))
		{
			horizontalAlignment = m_table.TableFormat.HorizontalAlignment;
		}
		return horizontalAlignment;
	}

	private float GetMinimumRightPad()
	{
		int count = m_table.Rows[0].Cells.Count;
		float num = GetRightPad(m_table.Rows[0].Cells[count - 1]);
		for (int i = 1; i < m_table.Rows.Count; i++)
		{
			count = m_table.Rows[i].Cells.Count;
			float rightPad = GetRightPad(m_table.Rows[i].Cells[count - 1]);
			if (num > rightPad)
			{
				num = rightPad;
			}
		}
		return num;
	}

	private float GetRightPad(WTableCell tableCell)
	{
		float num = tableCell.CellFormat.Paddings.Right;
		if (tableCell.CellFormat.SamePaddingsAsTable || num == -0.05f)
		{
			num = (tableCell.OwnerRow.RowFormat.Paddings.HasKey(4) ? tableCell.OwnerRow.RowFormat.Paddings.Right : (tableCell.OwnerRow.OwnerTable.TableFormat.Paddings.HasKey(4) ? tableCell.OwnerRow.OwnerTable.TableFormat.Paddings.Right : ((tableCell.Document.ActualFormatType != 0) ? 5.4f : 0f)));
		}
		return num;
	}
}
