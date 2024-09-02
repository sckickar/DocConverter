using System;
using System.Collections.Generic;
using DocGen.DocIO;
using DocGen.DocIO.DLS;
using DocGen.Drawing;

namespace DocGen.Layouting;

internal class CellLayoutInfo : LayoutInfo, ILayoutSpacingsInfo
{
	internal class CellBorder
	{
		private BorderStyle m_borderType;

		private Color m_borderColor;

		private float m_renderingLineWidth;

		private float m_borderLineWidth;

		private CellBorder m_adjCellLeftBorder;

		private CellBorder m_adjCellRightBorder;

		internal BorderStyle BorderType => m_borderType;

		internal Color BorderColor => m_borderColor;

		internal float RenderingLineWidth => m_renderingLineWidth;

		internal float BorderLineWidth => m_borderLineWidth;

		internal CellBorder AdjCellLeftBorder
		{
			get
			{
				return m_adjCellLeftBorder;
			}
			set
			{
				m_adjCellLeftBorder = value;
			}
		}

		internal CellBorder AdjCellRightBorder
		{
			get
			{
				return m_adjCellRightBorder;
			}
			set
			{
				m_adjCellRightBorder = value;
			}
		}

		public CellBorder(BorderStyle borderStyle, Color borderColor, float renderingLineWidth, float borderLineWidth)
		{
			m_borderType = borderStyle;
			m_borderColor = borderColor;
			m_renderingLineWidth = renderingLineWidth;
			m_borderLineWidth = borderLineWidth;
		}

		internal void InitLayoutInfo()
		{
			m_adjCellLeftBorder = null;
			m_adjCellRightBorder = null;
		}
	}

	private WTableCell m_cell;

	private VerticalAlignment m_verticalAlignment;

	private byte m_bFlags;

	private Spacings m_paddings;

	private Spacings m_margins;

	private RectangleF m_cellContentLayoutingBounds;

	private byte m_bFlags1;

	private float m_topPadding;

	private float m_updatedTopPadding;

	private float m_bottomPadding;

	private Dictionary<CellBorder, float> m_updatedTopBorders;

	private Dictionary<CellBorder, float> m_updatedSplittedTopBorders;

	private CellBorder m_topBorder;

	private CellBorder m_bottomBorder;

	private CellBorder m_leftBorder;

	private CellBorder m_rightBorder;

	private CellBorder m_prevCellTopBorder;

	private CellBorder m_nextCellTopBorder;

	private CellBorder m_prevCellBottomBorder;

	private CellBorder m_nextCellBottomBorder;

	internal bool IsColumnMergeStart
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

	internal bool IsColumnMergeContinue
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

	internal bool IsRowMergeStart
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

	internal bool IsRowMergeContinue
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

	internal bool IsRowMergeEnd
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

	internal VerticalAlignment VerticalAlignment
	{
		get
		{
			return m_verticalAlignment;
		}
		set
		{
			m_verticalAlignment = value;
		}
	}

	internal RectangleF CellContentLayoutingBounds
	{
		get
		{
			_ = m_cellContentLayoutingBounds;
			return m_cellContentLayoutingBounds;
		}
		set
		{
			m_cellContentLayoutingBounds = value;
		}
	}

	internal bool SkipTopBorder
	{
		get
		{
			return (m_bFlags1 & 1) != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFFFEu) | (value ? 1u : 0u));
		}
	}

	internal bool SkipBottomBorder
	{
		get
		{
			return (m_bFlags1 & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFFFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool SkipLeftBorder
	{
		get
		{
			return (m_bFlags1 & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFFFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool SkipRightBorder
	{
		get
		{
			return (m_bFlags1 & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFFF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal float TopPadding
	{
		get
		{
			return m_topPadding;
		}
		set
		{
			m_topPadding = value;
		}
	}

	internal float UpdatedTopPadding
	{
		get
		{
			return m_updatedTopPadding;
		}
		set
		{
			m_updatedTopPadding = value;
		}
	}

	internal float BottomPadding
	{
		get
		{
			return m_bottomPadding;
		}
		set
		{
			m_bottomPadding = value;
		}
	}

	internal CellBorder TopBorder
	{
		get
		{
			return m_topBorder;
		}
		set
		{
			m_topBorder = value;
		}
	}

	internal CellBorder BottomBorder
	{
		get
		{
			return m_bottomBorder;
		}
		set
		{
			m_bottomBorder = value;
		}
	}

	internal CellBorder LeftBorder
	{
		get
		{
			return m_leftBorder;
		}
		set
		{
			m_leftBorder = value;
		}
	}

	internal CellBorder RightBorder
	{
		get
		{
			return m_rightBorder;
		}
		set
		{
			m_rightBorder = value;
		}
	}

	internal Dictionary<CellBorder, float> UpdatedTopBorders
	{
		get
		{
			if (m_updatedTopBorders == null)
			{
				m_updatedTopBorders = new Dictionary<CellBorder, float>();
			}
			return m_updatedTopBorders;
		}
	}

	internal Dictionary<CellBorder, float> UpdatedSplittedTopBorders
	{
		get
		{
			return m_updatedSplittedTopBorders;
		}
		set
		{
			m_updatedSplittedTopBorders = value;
		}
	}

	internal CellBorder PrevCellTopBorder => m_prevCellTopBorder;

	internal CellBorder NextCellTopBorder
	{
		get
		{
			return m_nextCellTopBorder;
		}
		set
		{
			m_nextCellTopBorder = value;
		}
	}

	internal CellBorder PrevCellBottomBorder
	{
		get
		{
			return m_prevCellBottomBorder;
		}
		set
		{
			m_prevCellBottomBorder = value;
		}
	}

	internal CellBorder NextCellBottomBorder
	{
		get
		{
			return m_nextCellBottomBorder;
		}
		set
		{
			m_nextCellBottomBorder = value;
		}
	}

	internal bool IsCellHasEndNote
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

	internal bool IsCellHasFootNote
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

	public Spacings Paddings
	{
		get
		{
			if (m_paddings == null)
			{
				m_paddings = new Spacings();
			}
			return m_paddings;
		}
	}

	public Spacings Margins
	{
		get
		{
			if (m_margins == null)
			{
				m_margins = new Spacings();
			}
			return m_margins;
		}
	}

	public CellLayoutInfo(WTableCell cell)
		: base(ChildrenLayoutDirection.Vertical)
	{
		m_cell = cell;
		InitMerges();
		InitSpacings();
		CellFormat cellFormat = m_cell.CellFormat;
		if (cellFormat.TextDirection == TextDirection.VerticalBottomToTop || cellFormat.TextDirection == TextDirection.VerticalTopToBottom)
		{
			base.IsVerticalText = true;
		}
		if (cellFormat.HasKey(2))
		{
			VerticalAlignment = cellFormat.VerticalAlignment;
		}
		else if (m_cell.OwnerRow.OwnerTable.GetStyle() is WTableStyle wTableStyle && wTableStyle.CellProperties.HasKey(2) && wTableStyle.CellProperties.VerticalAlignment != 0)
		{
			VerticalAlignment = wTableStyle.CellProperties.VerticalAlignment;
		}
		else
		{
			VerticalAlignment = cellFormat.VerticalAlignment;
		}
		base.TextWrap = cellFormat.TextWrap;
	}

	internal void InitMerges()
	{
		CellFormat cellFormat = m_cell.CellFormat;
		int num = m_cell.OwnerRow.Cells.IndexOf(m_cell);
		IsColumnMergeStart = cellFormat.HorizontalMerge == CellMerge.Start && num < m_cell.OwnerRow.Cells.Count - 1 && m_cell.OwnerRow.Cells[num + 1].CellFormat.HorizontalMerge == CellMerge.Continue;
		if (cellFormat.HorizontalMerge == CellMerge.Continue && num > 0 && ((((IWidget)m_cell.OwnerRow.Cells[num - 1]).LayoutInfo as CellLayoutInfo).IsColumnMergeStart || (((IWidget)m_cell.OwnerRow.Cells[num - 1]).LayoutInfo as CellLayoutInfo).IsColumnMergeContinue))
		{
			IsColumnMergeContinue = true;
		}
		int rowIndex = m_cell.OwnerRow.GetRowIndex();
		int num2 = m_cell.OwnerRow.OwnerTable.Rows.Count - 1;
		float cellStartPosition = m_cell.CellStartPosition;
		float cellWidth = m_cell.GetCellWidth();
		if (rowIndex < num2 && cellFormat.VerticalMerge == CellMerge.Start)
		{
			IsRowMergeStart = IsCellVerticallyMerged(cellStartPosition, cellWidth, rowIndex + 1, checkMergeEnd: false, checkMergeStart: true);
		}
		if (rowIndex > 0 && cellFormat.VerticalMerge == CellMerge.Continue && cellFormat.VerticalMerge != CellMerge.Start)
		{
			IsRowMergeContinue = IsCellVerticallyMerged(cellStartPosition, cellWidth, rowIndex - 1, checkMergeEnd: false, checkMergeStart: false);
		}
		if (!IsRowMergeContinue && !IsRowMergeStart && rowIndex < num2 && cellFormat.VerticalMerge == CellMerge.Continue)
		{
			IsRowMergeStart = IsCellVerticallyMerged(cellStartPosition, cellWidth, rowIndex + 1, checkMergeEnd: false, checkMergeStart: true);
		}
		if (IsRowMergeContinue)
		{
			if (rowIndex < num2)
			{
				IsRowMergeEnd = !IsCellVerticallyMerged(cellStartPosition, cellWidth, rowIndex + 1, checkMergeEnd: true, checkMergeStart: false);
			}
			else
			{
				IsRowMergeEnd = true;
			}
		}
	}

	private bool IsCellVerticallyMerged(float cellStartPos, float cellWidth, int adjRowIndex, bool checkMergeEnd, bool checkMergeStart)
	{
		float num = 0f;
		bool result = false;
		for (int i = 0; i < m_cell.OwnerRow.OwnerTable.Rows[adjRowIndex].Cells.Count; i++)
		{
			if (Math.Round(cellStartPos, 2) == Math.Round(num, 2))
			{
				WTableCell wTableCell = m_cell.OwnerRow.OwnerTable.Rows[adjRowIndex].Cells[i];
				if (checkMergeEnd)
				{
					if (wTableCell.CellFormat.VerticalMerge == CellMerge.Start || wTableCell.CellFormat.VerticalMerge == CellMerge.None)
					{
						result = false;
						break;
					}
					float cellWidth2 = wTableCell.GetCellWidth();
					result = Math.Round(cellWidth, 2) == Math.Round(cellWidth2, 2);
					break;
				}
				if (checkMergeStart && wTableCell.CellFormat.VerticalMerge == CellMerge.Continue)
				{
					float cellWidth3 = wTableCell.GetCellWidth();
					result = Math.Round(cellWidth, 2) == Math.Round(cellWidth3, 2);
					break;
				}
				if (!checkMergeStart && (wTableCell.CellFormat.VerticalMerge == CellMerge.Start || wTableCell.CellFormat.VerticalMerge == CellMerge.Continue))
				{
					float cellWidth4 = wTableCell.GetCellWidth();
					result = Math.Round(cellWidth, 2) == Math.Round(cellWidth4, 2);
					break;
				}
			}
			else if (cellStartPos < num)
			{
				result = false;
				break;
			}
			num += m_cell.OwnerRow.OwnerTable.Rows[adjRowIndex].Cells[i].Width;
		}
		return result;
	}

	internal void InitSpacings()
	{
		if (IsColumnMergeContinue)
		{
			SkipBottomBorder = true;
			SkipLeftBorder = true;
			SkipRightBorder = true;
			SkipTopBorder = true;
			return;
		}
		float num = m_cell.CellFormat.Paddings.Left;
		float num2 = m_cell.CellFormat.Paddings.Right;
		float num3 = m_cell.CellFormat.Paddings.Top;
		float num4 = m_cell.CellFormat.Paddings.Bottom;
		CellFormat cellFormatFromStyle = m_cell.OwnerRow.OwnerTable.GetCellFormatFromStyle(m_cell.OwnerRow.Index, m_cell.Index);
		bool flag = true;
		if (cellFormatFromStyle != null && (cellFormatFromStyle.Paddings.HasKey(1) || cellFormatFromStyle.Paddings.HasKey(4) || cellFormatFromStyle.Paddings.HasKey(2) || cellFormatFromStyle.Paddings.HasKey(3)))
		{
			flag = false;
		}
		if (m_cell.CellFormat.SamePaddingsAsTable && flag)
		{
			WTable ownerTable = m_cell.OwnerRow.OwnerTable;
			WTableStyle wTableStyle = null;
			if (ownerTable.StyleName != null && ownerTable.StyleName != string.Empty && ownerTable.Document.StyleNameIds.ContainsValue(ownerTable.StyleName))
			{
				wTableStyle = ownerTable.Document.Styles.FindByName(ownerTable.StyleName) as WTableStyle;
			}
			num = (m_cell.OwnerRow.RowFormat.Paddings.HasKey(1) ? m_cell.OwnerRow.RowFormat.Paddings.Left : (ownerTable.TableFormat.Paddings.HasKey(1) ? ownerTable.TableFormat.Paddings.Left : ((wTableStyle != null && IsTableHavePadding(wTableStyle.TableProperties.Paddings)) ? wTableStyle.TableProperties.Paddings.Left : ((m_cell.Document.ActualFormatType != 0) ? 5.4f : 0f))));
			num2 = (m_cell.OwnerRow.RowFormat.Paddings.HasKey(4) ? m_cell.OwnerRow.RowFormat.Paddings.Right : (ownerTable.TableFormat.Paddings.HasKey(4) ? ownerTable.TableFormat.Paddings.Right : ((m_cell.Document.ActualFormatType == FormatType.Doc) ? 0f : ((wTableStyle == null || !wTableStyle.TableProperties.Paddings.HasKey(4)) ? 5.4f : wTableStyle.TableProperties.Paddings.Right))));
			num3 = (m_cell.OwnerRow.RowFormat.Paddings.HasKey(2) ? m_cell.OwnerRow.RowFormat.Paddings.Top : (ownerTable.TableFormat.Paddings.HasKey(2) ? ownerTable.TableFormat.Paddings.Top : ((wTableStyle == null || !wTableStyle.TableProperties.Paddings.HasKey(2)) ? 0f : wTableStyle.TableProperties.Paddings.Top)));
			num4 = (m_cell.OwnerRow.RowFormat.Paddings.HasKey(3) ? m_cell.OwnerRow.RowFormat.Paddings.Bottom : (ownerTable.TableFormat.Paddings.HasKey(3) ? ownerTable.TableFormat.Paddings.Bottom : ((wTableStyle == null || !wTableStyle.TableProperties.Paddings.HasKey(3)) ? 0f : wTableStyle.TableProperties.Paddings.Bottom)));
		}
		else if (cellFormatFromStyle == null)
		{
			num = m_cell.GetLeftPadding();
			num2 = m_cell.GetRightPadding();
			num3 = m_cell.GetTopPadding();
			num4 = m_cell.GetBottomPadding();
		}
		else
		{
			if (!cellFormatFromStyle.Paddings.HasKey(1) || num == 0f || num == -0.05f)
			{
				num = m_cell.GetLeftPadding();
			}
			if (!cellFormatFromStyle.Paddings.HasKey(4) || num2 == 0f || num2 == -0.05f)
			{
				num2 = m_cell.GetRightPadding();
			}
			if (!cellFormatFromStyle.Paddings.HasKey(2) || num3 == 0f || num3 == -0.05f)
			{
				num3 = m_cell.GetTopPadding();
			}
			if (!cellFormatFromStyle.Paddings.HasKey(3) || num4 == 0f || num3 == -0.05f)
			{
				num4 = m_cell.GetBottomPadding();
			}
		}
		int cellIndex = m_cell.GetCellIndex();
		int rowIndex = m_cell.OwnerRow.GetRowIndex();
		int cellLast = m_cell.OwnerRow.Cells.Count - 1;
		int rowLast = m_cell.OwnerRow.OwnerTable.Rows.Count - 1;
		GetTopHalfWidth(cellIndex, rowIndex, m_cell, rowIndex - 1);
		Paddings.Left = GetLeftHalfWidth(cellIndex);
		Paddings.Right = GetRightHalfWidth(cellIndex, cellLast);
		GetBottomHalfWidth(cellIndex, cellLast, rowIndex, rowLast);
		if (m_cell.OwnerRow.RowFormat.CellSpacing > 0f || m_cell.OwnerRow.OwnerTable.TableFormat.CellSpacing > 0f)
		{
			Paddings.Top = TopPadding;
			Margins.Left = num;
			Margins.Right = num2;
		}
		else
		{
			Margins.Left = ((num == 0f) ? 0f : ((num - Paddings.Left < 0f) ? 0f : (num - Paddings.Left)));
			Margins.Right = ((num2 == 0f) ? 0f : ((num2 - (Paddings.Right + ((Paddings.Left - num > 0f) ? (Paddings.Left - num) : 0f)) > 0f) ? (num2 - (Paddings.Right + ((Paddings.Left - num > 0f) ? (Paddings.Left - num) : 0f))) : 0f));
		}
		Margins.Top = num3;
		Margins.Bottom = num4;
	}

	private bool IsTableHavePadding(Paddings padding)
	{
		if (padding.HasKey(1))
		{
			return true;
		}
		while (padding.BaseFormat is Paddings)
		{
			padding = padding.BaseFormat as Paddings;
			if (padding.HasKey(1))
			{
				return true;
			}
		}
		return false;
	}

	private float GetLeftHalfWidth(int cellIndex)
	{
		Borders borders = m_cell.CellFormat.Borders;
		float result = 0f;
		Border border = borders.Left;
		if (!border.IsBorderDefined || (border.IsBorderDefined && border.BorderType == BorderStyle.None && border.LineWidth == 0f && border.Color.IsEmpty))
		{
			border = ((cellIndex != 0) ? m_cell.OwnerRow.RowFormat.Borders.Vertical : m_cell.OwnerRow.RowFormat.Borders.Left);
		}
		if (!border.IsBorderDefined)
		{
			border = ((cellIndex == 0 && !(m_cell.OwnerRow.RowFormat.CellSpacing > 0f) && !(m_cell.OwnerRow.OwnerTable.TableFormat.CellSpacing > 0f)) ? m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Left : m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Vertical);
		}
		if (m_cell.OwnerRow.OwnerTable.TableFormat.CellSpacing > 0f || m_cell.OwnerRow.RowFormat.CellSpacing > 0f)
		{
			m_leftBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
			if (m_leftBorder.BorderType == BorderStyle.Cleared || m_leftBorder.BorderType == BorderStyle.None)
			{
				SkipLeftBorder = true;
			}
			result = ((m_leftBorder.BorderType != 0 && m_leftBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f);
		}
		else if (cellIndex > 0)
		{
			WTableCell previousCell = m_cell.GetPreviousCell();
			CellLayoutInfo cellLayoutInfo = ((IWidget)previousCell).LayoutInfo as CellLayoutInfo;
			Border border2 = previousCell.CellFormat.Borders.Right;
			if (!border2.IsBorderDefined || (border2.IsBorderDefined && border2.BorderType == BorderStyle.None && border2.LineWidth == 0f && border2.Color.IsEmpty))
			{
				border2 = m_cell.OwnerRow.RowFormat.Borders.Vertical;
			}
			if (!border2.IsBorderDefined)
			{
				border2 = m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Vertical;
			}
			if ((border.IsBorderDefined || border2.IsBorderDefined) && cellLayoutInfo != null)
			{
				if ((border.BorderType == BorderStyle.None || border.BorderType == BorderStyle.Cleared) && (border2.BorderType == BorderStyle.Cleared || border2.BorderType == BorderStyle.None))
				{
					SkipLeftBorder = true;
				}
				else if ((border.BorderType == BorderStyle.None || border.BorderType == BorderStyle.Cleared) && previousCell.m_layoutInfo != null && !(previousCell.m_layoutInfo as CellLayoutInfo).SkipRightBorder)
				{
					m_leftBorder = new CellBorder(border2.BorderType, border2.Color, border2.GetLineWidthValue(), border2.LineWidth);
					result = ((m_leftBorder.BorderType != 0 && m_leftBorder.BorderType != BorderStyle.Cleared) ? border2.GetLineWidthValue() : 0f) / 2f;
					SkipLeftBorder = true;
				}
				else if ((border2.BorderType == BorderStyle.Cleared || border2.BorderType == BorderStyle.None) && previousCell.m_layoutInfo != null && (previousCell.m_layoutInfo as CellLayoutInfo).SkipRightBorder)
				{
					m_leftBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
					result = ((m_leftBorder.BorderType != 0 && m_leftBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f) / 2f;
				}
				else if (Border.IsSkipBorder(border, border2, isFirstRead: false) && !cellLayoutInfo.SkipRightBorder)
				{
					m_leftBorder = new CellBorder(border2.BorderType, border2.Color, border2.GetLineWidthValue(), border2.LineWidth);
					result = ((m_leftBorder.BorderType != 0 && m_leftBorder.BorderType != BorderStyle.Cleared) ? border2.GetLineWidthValue() : 0f) / 2f;
					SkipLeftBorder = true;
				}
				else
				{
					m_leftBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
					result = ((m_leftBorder.BorderType != 0 && m_leftBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f) / 2f;
				}
				m_prevCellTopBorder = ((cellLayoutInfo.UpdatedTopBorders.Count > 0) ? new List<CellBorder>(cellLayoutInfo.UpdatedTopBorders.Keys)[(((IWidget)previousCell).LayoutInfo as CellLayoutInfo).UpdatedTopBorders.Count - 1] : (((IWidget)previousCell).LayoutInfo as CellLayoutInfo).TopBorder);
			}
			if (cellLayoutInfo != null)
			{
				cellLayoutInfo.NextCellTopBorder = ((UpdatedTopBorders.Count > 0) ? new List<CellBorder>(UpdatedTopBorders.Keys)[0] : TopBorder);
			}
		}
		else
		{
			m_leftBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
			if (m_leftBorder.BorderType == BorderStyle.Cleared || m_leftBorder.BorderType == BorderStyle.None)
			{
				SkipLeftBorder = true;
			}
			result = ((m_leftBorder.BorderType != 0 && m_leftBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f) / 2f;
		}
		return result;
	}

	private float GetRightHalfWidth(int cellIndex, int cellLast)
	{
		Borders borders = m_cell.CellFormat.Borders;
		float result = 0f;
		Border border = borders.Right;
		if (IsColumnMergeStart)
		{
			cellIndex = m_cell.GetHorizontalMergeEndCellIndex();
			border = m_cell.OwnerRow.Cells[cellIndex].CellFormat.Borders.Right;
		}
		if (!border.IsBorderDefined || (border.IsBorderDefined && border.BorderType == BorderStyle.None && border.LineWidth == 0f && border.Color.IsEmpty))
		{
			border = ((cellIndex != cellLast) ? m_cell.OwnerRow.RowFormat.Borders.Vertical : m_cell.OwnerRow.RowFormat.Borders.Right);
		}
		if (!border.IsBorderDefined)
		{
			if (cellIndex != cellLast || m_cell.OwnerRow.RowFormat.CellSpacing > 0f || m_cell.OwnerRow.OwnerTable.TableFormat.CellSpacing > 0f)
			{
				border = m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Vertical;
			}
			else if (cellIndex == cellLast)
			{
				border = m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Right;
			}
		}
		if (m_cell.OwnerRow.OwnerTable.TableFormat.CellSpacing > 0f || m_cell.OwnerRow.RowFormat.CellSpacing > 0f)
		{
			m_rightBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
			if (m_rightBorder.BorderType == BorderStyle.Cleared || m_rightBorder.BorderType == BorderStyle.None)
			{
				SkipRightBorder = true;
			}
			result = ((m_rightBorder.BorderType != 0 && m_rightBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f);
		}
		else if (cellIndex < cellLast)
		{
			Border border2 = m_cell.OwnerRow.Cells[cellIndex + 1].CellFormat.Borders.Left;
			if (!border2.IsBorderDefined || (border2.IsBorderDefined && border2.BorderType == BorderStyle.None && border2.LineWidth == 0f && border2.Color.IsEmpty))
			{
				border2 = m_cell.OwnerRow.RowFormat.Borders.Vertical;
			}
			if (!border2.IsBorderDefined)
			{
				border2 = m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Vertical;
			}
			if (border.IsBorderDefined || border2.IsBorderDefined)
			{
				if ((border.BorderType == BorderStyle.None || border.BorderType == BorderStyle.Cleared) && (border2.BorderType == BorderStyle.Cleared || border2.BorderType == BorderStyle.None))
				{
					SkipRightBorder = true;
				}
				else if (border.BorderType == BorderStyle.None || border.BorderType == BorderStyle.Cleared)
				{
					m_rightBorder = new CellBorder(border2.BorderType, border2.Color, border2.GetLineWidthValue(), border2.LineWidth);
					result = ((m_rightBorder.BorderType != 0 && m_rightBorder.BorderType != BorderStyle.Cleared) ? border2.GetLineWidthValue() : 0f) / 2f;
					SkipRightBorder = true;
				}
				else if (border2.BorderType == BorderStyle.Cleared || border2.BorderType == BorderStyle.None)
				{
					m_rightBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
					result = ((m_rightBorder.BorderType != 0 && m_rightBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f) / 2f;
				}
				else if (Border.IsSkipBorder(border, border2, isFirstRead: true))
				{
					m_rightBorder = new CellBorder(border2.BorderType, border2.Color, border2.GetLineWidthValue(), border2.LineWidth);
					result = ((m_rightBorder.BorderType != 0 && m_rightBorder.BorderType != BorderStyle.Cleared) ? border2.GetLineWidthValue() : 0f) / 2f;
					SkipRightBorder = true;
				}
				else
				{
					m_rightBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
					result = ((m_rightBorder.BorderType != 0 && m_rightBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f) / 2f;
				}
			}
		}
		else
		{
			m_rightBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
			if (m_rightBorder.BorderType == BorderStyle.Cleared || m_rightBorder.BorderType == BorderStyle.None)
			{
				SkipRightBorder = true;
			}
			result = ((m_rightBorder.BorderType != 0 && m_rightBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f) / 2f;
		}
		return result;
	}

	private void GetBottomHalfWidth(int cellIndex, int cellLast, int rowIndex, int rowLast)
	{
		Border border = m_cell.CellFormat.Borders.Bottom;
		if (!border.IsBorderDefined || (border.IsBorderDefined && border.BorderType == BorderStyle.None && border.LineWidth == 0f && border.Color.IsEmpty))
		{
			border = m_cell.OwnerRow.RowFormat.Borders.Bottom;
		}
		if (!border.IsBorderDefined)
		{
			border = m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Bottom;
		}
		if (!border.IsBorderDefined && (m_cell.OwnerRow.RowFormat.CellSpacing > 0f || m_cell.OwnerRow.OwnerTable.TableFormat.CellSpacing > 0f))
		{
			border = m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Horizontal;
		}
		if (border.IsBorderDefined)
		{
			m_bottomBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
			if (m_bottomBorder.BorderType == BorderStyle.Cleared || m_bottomBorder.BorderType == BorderStyle.None)
			{
				SkipBottomBorder = true;
			}
			m_bottomPadding = ((m_bottomBorder.BorderType != 0 && m_bottomBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f);
		}
		if (cellIndex > 0)
		{
			m_prevCellBottomBorder = (((IWidget)m_cell.OwnerRow.Cells[cellIndex - 1]).LayoutInfo as CellLayoutInfo).BottomBorder;
		}
		if (cellIndex > 0 && cellIndex <= cellLast)
		{
			(((IWidget)m_cell.OwnerRow.Cells[cellIndex - 1]).LayoutInfo as CellLayoutInfo).NextCellBottomBorder = m_bottomBorder;
		}
	}

	internal float GetTopHalfWidth(int cellIndex, int rowIndex, WTableCell m_cell, int previousRowIndex)
	{
		Border border = m_cell.CellFormat.Borders.Top;
		if (!border.IsBorderDefined || (border.IsBorderDefined && border.BorderType == BorderStyle.None && border.LineWidth == 0f && border.Color.IsEmpty))
		{
			border = ((rowIndex != 0) ? m_cell.OwnerRow.RowFormat.Borders.Horizontal : m_cell.OwnerRow.RowFormat.Borders.Top);
		}
		if (!border.IsBorderDefined)
		{
			border = ((rowIndex == 0 && !(m_cell.OwnerRow.OwnerTable.TableFormat.CellSpacing > 0f) && !(m_cell.OwnerRow.RowFormat.CellSpacing > 0f)) ? m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Top : m_cell.OwnerRow.OwnerTable.TableFormat.Borders.Horizontal);
		}
		if (m_cell.OwnerRow.OwnerTable.TableFormat.CellSpacing > 0f || m_cell.OwnerRow.RowFormat.CellSpacing > 0f)
		{
			m_topBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
			if (m_topBorder.BorderType == BorderStyle.Cleared || m_topBorder.BorderType == BorderStyle.None)
			{
				SkipTopBorder = true;
			}
			m_topPadding = ((m_topBorder.BorderType != 0 && m_topBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f);
		}
		else
		{
			CellBorder cellBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
			m_topPadding = ((cellBorder.BorderType != 0 && cellBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f);
			if (rowIndex > 0)
			{
				float cellStartPosition = m_cell.CellStartPosition;
				float cellEndPosition = m_cell.CellEndPosition;
				List<WTableCell> adjacentRowCell = GetAdjacentRowCell(cellStartPosition, cellEndPosition, previousRowIndex);
				for (int i = 0; i < adjacentRowCell.Count; i++)
				{
					Border border2 = adjacentRowCell[i].CellFormat.Borders.Bottom;
					if (!border2.IsBorderDefined || (border2.IsBorderDefined && border2.BorderType == BorderStyle.None && border2.LineWidth == 0f && border2.Color.IsEmpty))
					{
						border2 = adjacentRowCell[i].OwnerRow.RowFormat.Borders.Horizontal;
					}
					if (!border2.IsBorderDefined)
					{
						border2 = adjacentRowCell[i].OwnerRow.OwnerTable.TableFormat.Borders.Horizontal;
					}
					CellBorder cellBorder2 = null;
					if (!border.IsBorderDefined && !border2.IsBorderDefined)
					{
						continue;
					}
					if (border.BorderType == BorderStyle.None || border.BorderType == BorderStyle.Cleared)
					{
						float num = border2.GetLineWidthValue();
						if ((m_cell.Owner as WTableRow).Height < num && m_cell.GridSpan > 1 && (m_cell.Owner as WTableRow).HeightType == TableRowHeightType.Exactly)
						{
							num = (m_cell.Owner as WTableRow).Height;
						}
						cellBorder2 = new CellBorder(border2.BorderType, border2.Color, num, border2.LineWidth);
						float num2 = ((cellBorder2.BorderType != 0 && cellBorder2.BorderType != BorderStyle.Cleared) ? border2.GetLineWidthValue() : 0f);
						if (m_updatedTopPadding < num2)
						{
							m_updatedTopPadding = num2;
						}
					}
					else if (border2.BorderType == BorderStyle.Cleared || border2.BorderType == BorderStyle.None)
					{
						cellBorder2 = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
						float num3 = ((cellBorder2.BorderType != 0 && cellBorder2.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f);
						if (m_updatedTopPadding < num3)
						{
							m_updatedTopPadding = num3;
						}
					}
					else if (Border.IsSkipBorder(border, border2, isFirstRead: true))
					{
						cellBorder2 = new CellBorder(border2.BorderType, border2.Color, border2.GetLineWidthValue(), border2.LineWidth);
						float num4 = ((cellBorder2.BorderType != 0 && cellBorder2.BorderType != BorderStyle.Cleared) ? border2.GetLineWidthValue() : 0f);
						if (m_updatedTopPadding < num4)
						{
							m_updatedTopPadding = num4;
						}
					}
					else
					{
						cellBorder2 = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
						float num5 = ((cellBorder2.BorderType != 0 && cellBorder2.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f);
						if (m_updatedTopPadding < num5)
						{
							m_updatedTopPadding = num5;
						}
					}
					if (cellBorder2 == null)
					{
						continue;
					}
					float cellStartPosition2 = adjacentRowCell[i].CellStartPosition;
					float cellEndPosition2 = adjacentRowCell[i].CellEndPosition;
					float num6 = 0f;
					num6 = ((Math.Round(cellEndPosition2, 2) == Math.Round(cellEndPosition, 2) && Math.Round(cellStartPosition2, 2) == Math.Round(cellStartPosition, 2)) ? (cellEndPosition - cellStartPosition) : ((Math.Round(cellStartPosition2, 2) >= Math.Round(cellStartPosition, 2) && Math.Round(cellEndPosition2, 2) >= Math.Round(cellEndPosition, 2)) ? (cellEndPosition - cellStartPosition2) : ((Math.Round(cellStartPosition2, 2) >= Math.Round(cellStartPosition, 2) && Math.Round(cellEndPosition2, 2) <= Math.Round(cellEndPosition, 2)) ? (cellEndPosition2 - cellStartPosition2) : ((Math.Round(cellStartPosition2, 2) <= Math.Round(cellStartPosition, 2) && Math.Round(cellEndPosition2, 2) <= Math.Round(cellEndPosition, 2)) ? (cellEndPosition2 - cellStartPosition) : ((!(Math.Round(cellStartPosition2, 2) <= Math.Round(cellStartPosition, 2)) || !(Math.Round(cellEndPosition2, 2) >= Math.Round(cellEndPosition, 2))) ? (cellEndPosition - cellStartPosition) : (cellEndPosition - cellStartPosition))))));
					if (num6 < 0f)
					{
						num6 = 0f;
					}
					if (Math.Round(cellStartPosition2, 2) == Math.Round(cellStartPosition, 2) || Math.Round(cellStartPosition2, 2) > Math.Round(cellStartPosition, 2))
					{
						cellBorder2.AdjCellLeftBorder = (((IWidget)adjacentRowCell[i]).LayoutInfo as CellLayoutInfo).LeftBorder;
					}
					if (Math.Round(cellEndPosition2, 2) == Math.Round(cellEndPosition, 2) || Math.Round(cellEndPosition2, 2) < Math.Round(cellEndPosition, 2))
					{
						cellBorder2.AdjCellRightBorder = (((IWidget)adjacentRowCell[i]).LayoutInfo as CellLayoutInfo).RightBorder;
					}
					if (num6 > 0f)
					{
						if (UpdatedSplittedTopBorders == null)
						{
							UpdatedTopBorders.Add(cellBorder2, num6);
						}
						else
						{
							UpdatedSplittedTopBorders.Add(cellBorder2, num6);
						}
					}
					float cellEndPosition3 = m_cell.OwnerRow.Cells[m_cell.OwnerRow.Cells.Count - 1].CellEndPosition;
					WTableCell wTableCell = adjacentRowCell[i].OwnerRow.Cells[adjacentRowCell[i].OwnerRow.Cells.Count - 1];
					float cellEndPosition4 = wTableCell.CellEndPosition;
					if (i != adjacentRowCell.Count - 1 || (Math.Round(cellEndPosition, 2) == Math.Round(cellEndPosition2, 2) && (Math.Round(cellEndPosition, 2) != Math.Round(cellEndPosition3, 2) || Math.Round(cellEndPosition3, 2) == Math.Round(cellEndPosition4, 2))))
					{
						continue;
					}
					if (cellIndex == m_cell.OwnerRow.Cells.Count - 1 && Math.Round(cellEndPosition, 2) < Math.Round(cellEndPosition4, 2))
					{
						if (adjacentRowCell[i] == wTableCell && border2.BorderType != BorderStyle.Cleared && border2.BorderType != 0)
						{
							cellBorder2 = new CellBorder(border2.BorderType, border2.Color, border2.GetLineWidthValue(), border2.LineWidth);
							num6 = cellEndPosition2 - cellEndPosition;
							if (num6 < 0f)
							{
								num6 = 0f;
							}
							if (UpdatedSplittedTopBorders == null)
							{
								UpdatedTopBorders.Add(cellBorder2, num6);
							}
							else
							{
								UpdatedSplittedTopBorders.Add(cellBorder2, num6);
							}
							continue;
						}
						List<WTableCell> adjacentRowCell2 = GetAdjacentRowCell(cellEndPosition, cellEndPosition4, rowIndex - 1);
						for (int j = 0; j < adjacentRowCell2.Count; j++)
						{
							border2 = adjacentRowCell2[j].CellFormat.Borders.Bottom;
							if (!border2.IsBorderDefined || (border2.IsBorderDefined && border2.BorderType == BorderStyle.None && border2.LineWidth == 0f && border2.Color.IsEmpty))
							{
								border2 = adjacentRowCell2[j].OwnerRow.RowFormat.Borders.Horizontal;
							}
							if (!border2.IsBorderDefined)
							{
								border2 = adjacentRowCell2[j].OwnerRow.OwnerTable.TableFormat.Borders.Horizontal;
							}
							if (border2.BorderType != 0)
							{
								float cellStartPosition3 = adjacentRowCell2[j].CellStartPosition;
								float cellEndPosition5 = adjacentRowCell2[j].CellEndPosition;
								cellBorder2 = new CellBorder(border2.BorderType, border2.Color, border2.GetLineWidthValue(), border2.LineWidth);
								num6 = ((j != 0) ? (cellEndPosition5 - cellStartPosition3) : (cellEndPosition5 - cellEndPosition));
								if (num6 < 0f)
								{
									num6 = 0f;
								}
								if (UpdatedSplittedTopBorders == null)
								{
									UpdatedTopBorders.Add(cellBorder2, num6);
								}
								else
								{
									UpdatedSplittedTopBorders.Add(cellBorder2, num6);
								}
							}
						}
					}
					else if (Math.Round(cellEndPosition, 2) > Math.Round(cellEndPosition2, 2) && adjacentRowCell[i].GetCellIndex() == adjacentRowCell[i].OwnerRow.Cells.Count - 1 && border.BorderType != BorderStyle.Cleared && border.BorderType != 0)
					{
						cellBorder2 = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
						num6 = cellEndPosition - cellEndPosition2;
						if (num6 < 0f)
						{
							num6 = 0f;
						}
						if (UpdatedSplittedTopBorders == null)
						{
							UpdatedTopBorders.Add(cellBorder2, num6);
						}
						else
						{
							UpdatedSplittedTopBorders.Add(cellBorder2, num6);
						}
					}
				}
				if (adjacentRowCell.Count == 0)
				{
					CellBorder cellBorder3 = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
					float num7 = ((cellBorder3.BorderType != 0 && cellBorder3.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f);
					if (cellBorder3.BorderType == BorderStyle.Cleared || cellBorder3.BorderType == BorderStyle.None)
					{
						SkipTopBorder = true;
					}
					if (m_updatedTopPadding < num7)
					{
						m_updatedTopPadding = num7;
					}
					if (UpdatedSplittedTopBorders == null)
					{
						UpdatedTopBorders.Add(cellBorder3, cellEndPosition - cellStartPosition);
					}
					else
					{
						UpdatedSplittedTopBorders.Add(cellBorder3, cellEndPosition - cellStartPosition);
					}
				}
				if (UpdatedTopBorders.Count > 0)
				{
					SkipTopBorder = true;
					foreach (CellBorder key in UpdatedTopBorders.Keys)
					{
						if (key.BorderType != 0 && key.BorderType != BorderStyle.Cleared)
						{
							SkipTopBorder = false;
							break;
						}
					}
				}
				if (UpdatedSplittedTopBorders != null && UpdatedSplittedTopBorders.Count > 0)
				{
					SkipTopBorder = true;
					foreach (CellBorder key2 in UpdatedSplittedTopBorders.Keys)
					{
						if (key2.BorderType != 0 && key2.BorderType != BorderStyle.Cleared)
						{
							SkipTopBorder = false;
							break;
						}
					}
				}
			}
			else
			{
				m_topBorder = new CellBorder(border.BorderType, border.Color, border.GetLineWidthValue(), border.LineWidth);
				m_topPadding = ((m_topBorder.BorderType != 0 && m_topBorder.BorderType != BorderStyle.Cleared) ? border.GetLineWidthValue() : 0f);
				if (m_topBorder.BorderType == BorderStyle.Cleared || m_topBorder.BorderType == BorderStyle.None)
				{
					SkipTopBorder = true;
				}
			}
		}
		return 0f;
	}

	internal List<WTableCell> GetAdjacentRowCell(float cellStartPos, float cellEndPos, int rowIndex)
	{
		List<WTableCell> list = new List<WTableCell>();
		WTableRow wTableRow = m_cell.OwnerRow.OwnerTable.Rows[rowIndex];
		for (int i = 0; i < wTableRow.Cells.Count; i++)
		{
			float cellStartPosition = wTableRow.Cells[i].CellStartPosition;
			float cellEndPosition = wTableRow.Cells[i].CellEndPosition;
			if ((Math.Round(cellEndPosition, 2) > Math.Round(cellStartPos, 2) && Math.Round(cellEndPosition, 2) <= Math.Round(cellEndPos, 2)) || (Math.Round(cellStartPosition, 2) >= Math.Round(cellStartPos, 2) && Math.Round(cellStartPosition, 2) < Math.Round(cellEndPos, 2)) || (Math.Round(cellStartPosition, 2) <= Math.Round(cellStartPos, 2) && Math.Round(cellEndPosition, 2) >= Math.Round(cellEndPos, 2)))
			{
				WTableCell wTableCell = wTableRow.Cells[i];
				if (wTableCell.m_layoutInfo != null && (wTableCell.m_layoutInfo as CellLayoutInfo).IsColumnMergeContinue)
				{
					for (int num = i; num >= 0; num--)
					{
						if (wTableRow.Cells[num].m_layoutInfo != null && (wTableRow.Cells[num].m_layoutInfo as CellLayoutInfo).IsColumnMergeStart)
						{
							wTableCell = wTableRow.Cells[num];
						}
					}
				}
				if (!list.Contains(wTableCell))
				{
					list.Add(wTableCell);
				}
			}
			if (Math.Round(cellEndPosition, 2) >= Math.Round(cellEndPos, 2))
			{
				break;
			}
		}
		return list;
	}

	internal void InitLayoutInfo()
	{
		m_paddings = null;
		m_margins = null;
		if (m_topBorder != null)
		{
			m_topBorder.InitLayoutInfo();
			m_topBorder = null;
		}
		if (m_leftBorder != null)
		{
			m_leftBorder.InitLayoutInfo();
			m_leftBorder = null;
		}
		if (m_rightBorder != null)
		{
			m_rightBorder.InitLayoutInfo();
			m_rightBorder = null;
		}
		if (m_bottomBorder != null)
		{
			m_bottomBorder.InitLayoutInfo();
			m_bottomBorder = null;
		}
		if (m_prevCellTopBorder != null)
		{
			m_prevCellTopBorder.InitLayoutInfo();
			m_prevCellTopBorder = null;
		}
		if (m_prevCellBottomBorder != null)
		{
			m_prevCellBottomBorder.InitLayoutInfo();
			m_prevCellBottomBorder = null;
		}
		if (m_nextCellTopBorder != null)
		{
			m_nextCellTopBorder.InitLayoutInfo();
			m_nextCellTopBorder = null;
		}
		if (m_nextCellBottomBorder != null)
		{
			m_nextCellBottomBorder.InitLayoutInfo();
			m_nextCellBottomBorder = null;
		}
		m_cell = null;
		if (m_updatedTopBorders != null)
		{
			m_updatedTopBorders.Clear();
			m_updatedTopBorders = null;
		}
		if (m_updatedSplittedTopBorders != null)
		{
			m_updatedSplittedTopBorders.Clear();
			m_updatedTopBorders = null;
		}
	}
}
