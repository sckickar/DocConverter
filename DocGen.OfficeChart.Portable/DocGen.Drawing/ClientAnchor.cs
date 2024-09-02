using System;
using DocGen.OfficeChart;
using DocGen.OfficeChart.Implementation;
using DocGen.OfficeChart.Implementation.Collections;

namespace DocGen.Drawing;

internal class ClientAnchor
{
	private SizeProperties m_size;

	private WorksheetImpl m_workSheet;

	internal WorksheetImpl Worksheet
	{
		get
		{
			return m_workSheet;
		}
		set
		{
			m_workSheet = value;
		}
	}

	internal PlacementType Placement
	{
		get
		{
			return m_size.GetPlacementType();
		}
		set
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			int num5 = 0;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			int num9 = 0;
			int num10 = 0;
			if (m_size.GetPlacementType() == PlacementType.MoveAndSize)
			{
				num3 = m_size.GetTopRow();
				num4 = m_size.Top;
				num5 = m_size.GetLeftColumn();
				num6 = m_size.Left;
				num7 = m_size.GetBottomRow();
				num8 = m_size.Bottom;
				num9 = m_size.GetRightColumn();
				num10 = m_size.Right;
				if (value == PlacementType.FreeFloating)
				{
					num = CalculateWidth(0, 0, num5, num6);
					num2 = CalculateHeight(0, 0, num3, num4);
					m_size.Top = num2;
					m_size.Left = num;
				}
				num = CalculateWidth(num5, num6, num9, num10);
				num2 = CalculateHeight(num3, num4, num7, num8);
				m_size.Right = num;
				m_size.Bottom = num2;
				m_size.SetPlacementType(value);
			}
			else if (m_size.GetPlacementType() == PlacementType.Move)
			{
				num3 = m_size.GetTopRow();
				num4 = m_size.Top;
				num5 = m_size.GetLeftColumn();
				num6 = m_size.Left;
				switch (value)
				{
				case PlacementType.MoveAndSize:
				{
					num = m_size.Right;
					num2 = m_size.Bottom;
					int[] leftAndLeftColumnOffset = GetLeftAndLeftColumnOffset(num5, num6, num);
					m_size.SetRightColumn(leftAndLeftColumnOffset[0]);
					m_size.Right = leftAndLeftColumnOffset[1];
					leftAndLeftColumnOffset = GetTopAndTopRowOffset(num3, num4, num2);
					m_size.SetBottomRow(leftAndLeftColumnOffset[0]);
					m_size.Bottom = leftAndLeftColumnOffset[1];
					break;
				}
				case PlacementType.FreeFloating:
					num = CalculateWidth(0, 0, num5, num6);
					m_size.Left = num;
					num2 = CalculateHeight(0, 0, num3, num4);
					m_size.Top = num2;
					break;
				}
				m_size.SetPlacementType(value);
			}
			else if (m_size.GetPlacementType() == PlacementType.FreeFloating)
			{
				num = m_size.Left;
				num2 = m_size.Top;
				int[] topAndTopRowOffset = GetTopAndTopRowOffset(0, 0, num2);
				m_size.SetTopRow(topAndTopRowOffset[0]);
				m_size.Top = topAndTopRowOffset[1];
				num3 = topAndTopRowOffset[0];
				num4 = topAndTopRowOffset[1];
				topAndTopRowOffset = GetLeftAndLeftColumnOffset(0, 0, num);
				m_size.SetLeftColumn(topAndTopRowOffset[0]);
				m_size.Left = topAndTopRowOffset[1];
				num5 = topAndTopRowOffset[0];
				num6 = topAndTopRowOffset[1];
				if (value == PlacementType.MoveAndSize)
				{
					num = m_size.Right;
					num2 = m_size.Bottom;
					topAndTopRowOffset = GetTopAndTopRowOffset(num3, num4, num2);
					m_size.SetBottomRow(topAndTopRowOffset[0]);
					m_size.Bottom = topAndTopRowOffset[1];
					topAndTopRowOffset = GetLeftAndLeftColumnOffset(num5, num6, num);
					m_size.SetRightColumn(topAndTopRowOffset[0]);
					m_size.Right = topAndTopRowOffset[1];
				}
				m_size.SetPlacementType(value);
			}
		}
	}

	internal int Height
	{
		get
		{
			if (Placement != PlacementType.Move && Placement != 0)
			{
				int topRow = m_size.GetTopRow();
				int bottomRow = m_size.GetBottomRow();
				int top = m_size.Top;
				int bottom = m_size.Bottom;
				return CalculateHeight(topRow, top, bottomRow, bottom);
			}
			return m_size.Bottom;
		}
		set
		{
			if (Placement != PlacementType.Move && Placement != 0)
			{
				int topRow = m_size.GetTopRow();
				int top = m_size.Top;
				int[] topAndTopRowOffset = GetTopAndTopRowOffset(topRow, top, value);
				m_size.SetBottomRow(topAndTopRowOffset[0]);
				m_size.Bottom = topAndTopRowOffset[1];
			}
			else
			{
				m_size.Bottom = value;
			}
		}
	}

	internal int Width
	{
		get
		{
			if (Placement != PlacementType.Move && Placement != 0)
			{
				int leftColumn = m_size.GetLeftColumn();
				int rightColumn = m_size.GetRightColumn();
				int left = m_size.Left;
				int right = m_size.Right;
				return CalculateWidth(leftColumn, left, rightColumn, right);
			}
			return m_size.Right;
		}
		set
		{
			if (Placement != PlacementType.Move && Placement != 0)
			{
				int leftColumn = m_size.GetLeftColumn();
				int left = m_size.Left;
				int[] leftAndLeftColumnOffset = GetLeftAndLeftColumnOffset(leftColumn, left, value);
				m_size.SetRightColumn(leftAndLeftColumnOffset[0]);
				m_size.Right = leftAndLeftColumnOffset[1];
			}
			else
			{
				m_size.Right = value;
			}
		}
	}

	internal int Top
	{
		get
		{
			return (int)((double)((float)(m_workSheet.GetInnerRowHeightInPixels(TopRow) * TopRowOffset) / SizeProperties.FULL_ROW_OFFSET) + 0.5);
		}
		set
		{
			int innerRowHeightInPixels = m_workSheet.GetInnerRowHeightInPixels(TopRow);
			if (innerRowHeightInPixels < value)
			{
				throw new ArgumentException("The top value must be less than the upper left row height.");
			}
			TopRowOffset = (int)((double)((float)value * SizeProperties.FULL_ROW_OFFSET / (float)innerRowHeightInPixels) + 0.5);
		}
	}

	internal int Left
	{
		get
		{
			return (int)((double)((float)(m_workSheet.GetViewColumnWidthPixel(LeftColumn) * LeftColumnOffset) / SizeProperties.FULL_COLUMN_OFFSET) + 0.5);
		}
		set
		{
			int viewColumnWidthPixel = m_workSheet.GetViewColumnWidthPixel(LeftColumn);
			if (viewColumnWidthPixel < value)
			{
				throw new ArgumentException("The left value must be less than the upper left column width.");
			}
			LeftColumnOffset = (int)((double)((float)value * SizeProperties.FULL_COLUMN_OFFSET / (float)viewColumnWidthPixel) + 0.5);
		}
	}

	internal int LeftColumn
	{
		get
		{
			if (Placement != PlacementType.Move && Placement != PlacementType.MoveAndSize)
			{
				int left = m_size.Left;
				return GetLeftAndLeftColumnOffset(0, 0, left)[0];
			}
			return m_size.GetLeftColumn();
		}
		set
		{
			WorksheetImpl.CheckColumnIndex(value);
			PlacementType placement = Placement;
			Placement = PlacementType.Move;
			m_size.SetLeftColumn(value);
			Placement = placement;
		}
	}

	internal int TopRow
	{
		get
		{
			if (Placement != PlacementType.Move && Placement != PlacementType.MoveAndSize)
			{
				int top = m_size.Top;
				return GetTopAndTopRowOffset(0, 0, top)[0];
			}
			return m_size.GetTopRow();
		}
		set
		{
			WorksheetImpl.CheckRowIndex(value);
			PlacementType placement = Placement;
			Placement = PlacementType.Move;
			m_size.SetTopRow(value);
			Placement = placement;
		}
	}

	internal int TopRowOffset
	{
		get
		{
			if (Placement != PlacementType.Move && Placement != PlacementType.MoveAndSize)
			{
				int top = m_size.Top;
				return GetTopAndTopRowOffset(0, 0, top)[1];
			}
			if ((float)m_size.Top <= SizeProperties.FULL_ROW_OFFSET)
			{
				return m_size.Top;
			}
			return (int)SizeProperties.FULL_ROW_OFFSET;
		}
		set
		{
			if (value >= 0 && (float)value <= SizeProperties.FULL_ROW_OFFSET)
			{
				PlacementType placement = Placement;
				Placement = PlacementType.Move;
				m_size.Top = value;
				Placement = placement;
			}
		}
	}

	internal int LeftColumnOffset
	{
		get
		{
			if (Placement != PlacementType.Move && Placement != PlacementType.MoveAndSize)
			{
				int left = m_size.Left;
				return GetLeftAndLeftColumnOffset(0, 0, left)[1];
			}
			if ((float)m_size.Left <= SizeProperties.FULL_COLUMN_OFFSET)
			{
				return m_size.Left;
			}
			return (int)SizeProperties.FULL_COLUMN_OFFSET;
		}
		set
		{
			if (value >= 0 && (float)value <= SizeProperties.FULL_COLUMN_OFFSET)
			{
				PlacementType placement = Placement;
				Placement = PlacementType.Move;
				m_size.Left = value;
				Placement = placement;
			}
		}
	}

	internal int RightColumnOffset
	{
		get
		{
			if (Placement == PlacementType.MoveAndSize)
			{
				return m_size.Right;
			}
			int leftColumn = 0;
			int left = 0;
			int num = m_size.Right;
			if (Placement == PlacementType.Move)
			{
				leftColumn = m_size.GetLeftColumn();
				left = m_size.Left;
			}
			else
			{
				num += m_size.Left;
			}
			return GetLeftAndLeftColumnOffset(leftColumn, left, num)[1];
		}
		set
		{
			if (value >= 0 || (float)value <= SizeProperties.FULL_COLUMN_OFFSET)
			{
				int rightColumn = RightColumn;
				PlacementType placement = Placement;
				Placement = PlacementType.Move;
				int right = m_size.Right;
				int[] leftAndLeftOffset = GetLeftAndLeftOffset(rightColumn, value, right);
				m_size.SetLeftColumn(leftAndLeftOffset[0]);
				m_size.Left = leftAndLeftOffset[1];
				Placement = placement;
			}
		}
	}

	internal int BottomRowOffset
	{
		get
		{
			if (Placement == PlacementType.MoveAndSize)
			{
				return m_size.Bottom;
			}
			int num = m_size.Bottom;
			int topRow = 0;
			int top = 0;
			if (Placement == PlacementType.Move)
			{
				topRow = m_size.GetTopRow();
				top = m_size.Top;
			}
			else
			{
				num += m_size.Top;
			}
			return GetTopAndTopRowOffset(topRow, top, num)[1];
		}
		set
		{
			if (value >= 0 && (float)value <= SizeProperties.FULL_ROW_OFFSET)
			{
				int bottomRow = BottomRow;
				PlacementType placement = Placement;
				Placement = PlacementType.Move;
				int bottom = m_size.Bottom;
				int[] topAndTopOffset = GetTopAndTopOffset(bottomRow, value, bottom);
				m_size.SetTopRow(topAndTopOffset[0]);
				m_size.Top = topAndTopOffset[1];
				Placement = placement;
			}
		}
	}

	internal int RightColumn
	{
		get
		{
			if (Placement == PlacementType.MoveAndSize)
			{
				return m_size.GetRightColumn();
			}
			int leftColumn = 0;
			int left = 0;
			int num = m_size.Right;
			if (Placement == PlacementType.Move)
			{
				leftColumn = m_size.GetLeftColumn();
				left = m_size.Left;
			}
			else
			{
				num += m_size.Left;
			}
			return GetLeftAndLeftColumnOffset(leftColumn, left, num)[0];
		}
		set
		{
			WorksheetImpl.CheckColumnIndex(value);
			int rightColumnOffset = RightColumnOffset;
			PlacementType placement = Placement;
			Placement = PlacementType.Move;
			int right = m_size.Right;
			int[] leftAndLeftOffset = GetLeftAndLeftOffset(value, rightColumnOffset, right);
			m_size.SetLeftColumn(leftAndLeftOffset[0]);
			m_size.Left = leftAndLeftOffset[1];
			Placement = placement;
		}
	}

	internal int BottomRow
	{
		get
		{
			if (Placement == PlacementType.MoveAndSize)
			{
				return m_size.GetBottomRow();
			}
			int num = m_size.Bottom;
			int topRow = 0;
			int top = 0;
			if (Placement == PlacementType.Move)
			{
				topRow = m_size.GetTopRow();
				top = m_size.Top;
			}
			else
			{
				num += m_size.Top;
			}
			return GetTopAndTopRowOffset(topRow, top, num)[0];
		}
		set
		{
			WorksheetImpl.CheckRowIndex(value);
			int bottomRowOffset = BottomRowOffset;
			PlacementType placement = Placement;
			Placement = PlacementType.Move;
			int bottom = m_size.Bottom;
			int[] topAndTopOffset = GetTopAndTopOffset(value, bottomRowOffset, bottom);
			m_size.SetTopRow(topAndTopOffset[0]);
			m_size.Top = topAndTopOffset[1];
			Placement = placement;
		}
	}

	internal ClientAnchor(WorksheetImpl worksheetImpl)
	{
		m_workSheet = worksheetImpl;
		m_size = new SizeProperties();
	}

	internal int[] GetTopAndTopRowOffset(int topRow, int top, int bottom)
	{
		int[] array = new int[2];
		if (bottom == 0)
		{
			array[0] = topRow;
			array[1] = top;
			return array;
		}
		int num = 0;
		WorksheetImpl workSheet = m_workSheet;
		RecordTable table = workSheet.CellRecords.Table;
		int count = table.Rows.GetCount();
		if (top != 0)
		{
			num = workSheet.GetInnerRowHeightInPixels(topRow);
			int num2 = (int)((double)((float)num - (float)(num * top) / SizeProperties.FULL_ROW_OFFSET) + 0.5);
			if (bottom <= num2)
			{
				array[0] = topRow;
				array[1] = (int)((double)((float)bottom * SizeProperties.FULL_ROW_OFFSET / (float)num + (float)top) + 0.5);
				return array;
			}
			topRow++;
			bottom -= num2;
		}
		int num3 = (int)(workSheet.StandardHeight * (double)workSheet.GetAppImpl().GetdpiY() / 72.0 + 0.5);
		int arrIndex = 0;
		if (count == 0)
		{
			int num4 = (int)Math.Ceiling((double)bottom / (double)num3);
			topRow += num4 - 1;
			bottom -= num3 * num4;
			num = num3;
		}
		else
		{
			table.Rows.GetRowIndex(topRow, out arrIndex);
			if (arrIndex >= count)
			{
				int num5 = (int)Math.Ceiling((double)bottom / (double)num3);
				topRow += num5 - 1;
				bottom -= num3 * num5;
				num = num3;
			}
			else
			{
				_ = table.Rows[arrIndex];
				while (topRow <= 1048575)
				{
					if (arrIndex == topRow)
					{
						num = (int)(workSheet.GetInnerRowHeight(arrIndex) * (double)workSheet.GetAppImpl().GetdpiY() / 72.0 + 0.5);
						bottom -= num;
						if (bottom <= 0)
						{
							break;
						}
						arrIndex++;
						if (arrIndex >= count)
						{
							int num6 = (int)Math.Ceiling((double)bottom / (double)num3);
							topRow += num6;
							bottom -= num3 * num6;
							num = num3;
							break;
						}
						_ = table.Rows[arrIndex];
					}
					else
					{
						num = num3;
						bottom -= num;
						if (bottom <= 0)
						{
							break;
						}
					}
					topRow++;
				}
			}
		}
		if (bottom <= 0 && (bottom != 0 || topRow != 1048575))
		{
			if (bottom == 0)
			{
				array[0] = topRow + 1;
				return array;
			}
			array[0] = topRow;
			array[1] = (int)((double)((float)(bottom + num) * SizeProperties.FULL_ROW_OFFSET / (float)num) + 0.5);
			return array;
		}
		array[0] = 1048575;
		array[1] = (int)SizeProperties.FULL_ROW_OFFSET;
		return array;
	}

	internal int CalculateHeight(int topRow, int top, int bottomRow, int bottom)
	{
		int num = 0;
		int num2 = 0;
		WorksheetImpl workSheet = m_workSheet;
		if ((float)top >= SizeProperties.FULL_ROW_OFFSET)
		{
			top = (int)SizeProperties.FULL_ROW_OFFSET;
		}
		if ((float)bottom >= SizeProperties.FULL_ROW_OFFSET)
		{
			bottom = (int)SizeProperties.FULL_ROW_OFFSET;
		}
		if (bottomRow == topRow)
		{
			num2 = workSheet.GetInnerRowHeightInPixels(topRow);
			return (int)((double)((float)((bottom - top) * num2) / SizeProperties.FULL_ROW_OFFSET) + 0.5);
		}
		if (bottomRow < topRow)
		{
			return 0;
		}
		num2 = workSheet.GetInnerRowHeightInPixels(topRow);
		num += num2 - (int)((double)((float)(top * num2) / SizeProperties.FULL_ROW_OFFSET) + 0.5);
		int num3 = topRow;
		topRow++;
		int i = 0;
		int num4 = 0;
		RecordTable table = workSheet.CellRecords.Table;
		for (int count = table.Rows.GetCount(); i < count; i++)
		{
			if (table.Rows[i] != null && i >= topRow)
			{
				if (i >= bottomRow)
				{
					break;
				}
				num4++;
				num += (int)(workSheet.GetInnerRowHeight(i) * (double)workSheet.GetAppImpl().GetdpiY() / 72.0 + 0.5);
			}
		}
		int num5 = bottomRow - num3 - 1 - num4;
		if (num5 > 0)
		{
			num += num5 * (int)(workSheet.StandardHeight * (double)workSheet.GetAppImpl().GetdpiY() / 72.0 + 0.5);
		}
		num2 = workSheet.GetInnerRowHeightInPixels(bottomRow);
		return num + (int)((double)((float)(bottom * num2) / SizeProperties.FULL_ROW_OFFSET) + 0.5);
	}

	internal int CalculateRowOffset(int minRow, int minOffsetValue, int maxRow, int maxOffsetValue)
	{
		int num = 0;
		int num2 = 0;
		WorksheetImpl workSheet = m_workSheet;
		if (maxRow == minRow)
		{
			num2 = workSheet.GetInnerRowHeightInPixels(minRow);
			return (int)((double)((float)((maxOffsetValue - minOffsetValue) * num2) / SizeProperties.FULL_ROW_OFFSET) + 0.5);
		}
		num2 = workSheet.GetInnerRowHeightInPixels(minRow);
		num += num2 - (int)((double)((float)(minOffsetValue * num2) / SizeProperties.FULL_ROW_OFFSET) + 0.5);
		int num3 = minRow;
		minRow++;
		int i = 0;
		int num4 = 0;
		RecordTable table = workSheet.CellRecords.Table;
		for (int count = table.Rows.GetCount(); i < count; i++)
		{
			if (table.Rows[i] != null && i >= minRow)
			{
				if (i >= maxRow)
				{
					break;
				}
				num4++;
				num += (int)(workSheet.StandardHeight * (double)workSheet.GetAppImpl().GetdpiY() / 1440.0);
			}
		}
		int num5 = maxRow - num3 - 1 - num4;
		if (num5 > 0)
		{
			num += num5 * (int)(workSheet.StandardHeight * (double)workSheet.GetAppImpl().GetdpiY() / 72.0 + 0.5);
		}
		num2 = workSheet.GetInnerRowHeightInPixels(maxRow);
		return num + (int)((double)((float)(maxOffsetValue * num2) / SizeProperties.FULL_ROW_OFFSET) + 0.5);
	}

	internal int CalculateColumnOffset(int minColumn, int minOffsetValue, int maxColumn, int maxOffsetValue)
	{
		int num = 0;
		int num2 = 0;
		WorksheetImpl workSheet = m_workSheet;
		if (maxColumn == minColumn)
		{
			num2 = workSheet.GetColumnWidthInPixels(minColumn + 1);
			return (int)((double)((float)((maxOffsetValue - minOffsetValue) * num2) / SizeProperties.FULL_COLUMN_OFFSET) + 0.5);
		}
		num2 = workSheet.GetColumnWidthInPixels(minColumn + 1);
		num += num2 - (int)((double)((float)(minOffsetValue * num2) / SizeProperties.FULL_COLUMN_OFFSET) + 0.5);
		int num3 = minColumn;
		minColumn++;
		int num4 = 0;
		workSheet.Columnss.GetColumnIndex(minColumn, out var i);
		for (; i < workSheet.Columnss.Count; i++)
		{
			Column columnByIndex = workSheet.Columnss.GetColumnByIndex(i);
			if (columnByIndex.Index >= minColumn)
			{
				if (columnByIndex.Index >= maxColumn)
				{
					break;
				}
				num4++;
				if (!columnByIndex.IsHidden)
				{
					num += WorksheetImpl.CharacterWidth(columnByIndex.Width, workSheet.GetAppImpl());
				}
			}
		}
		num += workSheet.Columnss.GetWidth(num3 + num4 + 1, maxColumn - 1, isDefaultWidth: true, isLayout: true);
		num2 = workSheet.GetColumnWidthInPixels(maxColumn + 1);
		return num + (int)((double)((float)(maxOffsetValue * num2) / SizeProperties.FULL_COLUMN_OFFSET) + 0.5);
	}

	internal int[] GetLeftAndLeftColumnOffset(int leftColumn, int left, int right)
	{
		int[] array = new int[2];
		int num = 0;
		WorksheetImpl workSheet = m_workSheet;
		if (left != 0)
		{
			num = workSheet.GetViewColumnWidthPixel(leftColumn);
			int num2 = (int)((double)((float)num - (float)(num * left) / SizeProperties.FULL_COLUMN_OFFSET) + 0.5);
			if (right <= num2)
			{
				array[0] = leftColumn;
				array[1] = (int)((double)((float)right * SizeProperties.FULL_COLUMN_OFFSET / (float)num + (float)left) + 0.5);
				return array;
			}
			leftColumn++;
			right -= num2;
		}
		int arrIndex = 0;
		workSheet.Columnss.GetColumnIndex(leftColumn, out arrIndex);
		do
		{
			double num3 = 0.0;
			if (arrIndex >= workSheet.Columnss.Count)
			{
				num3 = workSheet.Columnss.GetWidth(leftColumn, isDefaultWidth: false);
			}
			else
			{
				Column columnByIndex = workSheet.Columnss.GetColumnByIndex(arrIndex);
				if (columnByIndex.Index == leftColumn)
				{
					arrIndex++;
					num3 = (columnByIndex.IsHidden ? 0.0 : columnByIndex.defaultWidth);
				}
				else
				{
					num3 = workSheet.Columnss.GetWidth(leftColumn, isDefaultWidth: false);
				}
			}
			num = WorksheetImpl.CharacterWidth(num3, workSheet.GetAppImpl());
			right -= num;
			if (right <= 0)
			{
				if (right <= 0 && (right != 0 || leftColumn != 16383))
				{
					if (right == 0)
					{
						array[0] = leftColumn + 1;
						return array;
					}
					array[0] = leftColumn;
					array[1] = (int)((double)((float)(right + num) * SizeProperties.FULL_COLUMN_OFFSET / (float)num) + 0.5);
					return array;
				}
				array[0] = 16383;
				array[1] = (int)SizeProperties.FULL_COLUMN_OFFSET;
				return array;
			}
			leftColumn++;
		}
		while (leftColumn <= 16383);
		array[0] = 16383;
		array[1] = (int)SizeProperties.FULL_COLUMN_OFFSET;
		return array;
	}

	internal int CalculateWidth(int leftColumn, int left, int rightColumn, int right)
	{
		int num = 0;
		int num2 = 0;
		WorksheetImpl workSheet = m_workSheet;
		if (rightColumn == leftColumn)
		{
			num2 = workSheet.GetViewColumnWidthPixel(leftColumn);
			return (int)((double)((float)((right - left) * num2) / SizeProperties.FULL_COLUMN_OFFSET) + 0.5);
		}
		if (rightColumn < leftColumn)
		{
			return 0;
		}
		num2 = workSheet.GetViewColumnWidthPixel(leftColumn);
		num += num2 - (int)((double)((float)(left * num2) / SizeProperties.FULL_COLUMN_OFFSET) + 0.5);
		int num3 = leftColumn;
		leftColumn++;
		int num4 = 0;
		workSheet.Columnss.GetColumnIndex(leftColumn, out var i);
		for (; i < workSheet.Columnss.Count; i++)
		{
			Column columnByIndex = workSheet.Columnss.GetColumnByIndex(i);
			if (columnByIndex.Index >= leftColumn)
			{
				if (columnByIndex.Index >= rightColumn)
				{
					break;
				}
				num4++;
				if (!columnByIndex.IsHidden)
				{
					num += WorksheetImpl.CharacterWidth(columnByIndex.Width, workSheet.GetAppImpl());
				}
			}
		}
		num += workSheet.Columnss.GetWidth(num3 + num4 + 1, rightColumn - 1, isDefaultWidth: false, isLayout: true);
		num2 = workSheet.GetViewColumnWidthPixel(rightColumn);
		return num + (int)((double)((float)(right * num2) / SizeProperties.FULL_COLUMN_OFFSET) + 0.5);
	}

	internal int[] GetTopAndTopOffset(int bottomRow, int bottomRowOffset, int bottom)
	{
		int[] array = new int[2];
		int num = 0;
		WorksheetImpl workSheet = m_workSheet;
		if (bottomRowOffset != 0)
		{
			num = workSheet.GetInnerRowHeightInPixels(bottomRow);
			int num2 = (int)((double)((float)(num * bottomRowOffset) / SizeProperties.FULL_ROW_OFFSET) + 0.5);
			if (bottom <= num2)
			{
				array[0] = bottomRow;
				array[1] = (int)((double)((float)(num2 - bottom) * SizeProperties.FULL_ROW_OFFSET / (float)num + (float)bottomRowOffset) + 0.5);
				return array;
			}
			bottom -= num2;
		}
		for (bottomRow--; bottomRow >= 0; bottomRow--)
		{
			num = workSheet.GetInnerRowHeightInPixels(bottomRow);
			bottom -= num;
			if (bottom <= 0)
			{
				break;
			}
		}
		if (bottom <= 0 && (bottom != 0 || bottomRow > 0))
		{
			if (bottom == 0)
			{
				array[0] = bottomRow;
				return array;
			}
			array[0] = bottomRow;
			array[1] = (int)((double)((float)(-bottom) * SizeProperties.FULL_ROW_OFFSET / (float)num) + 0.5);
		}
		return array;
	}

	internal int[] GetLeftAndLeftOffset(int rightColumn, int rightColumnOffset, int right)
	{
		int[] array = new int[2];
		if (right == 0)
		{
			array[0] = rightColumn;
			array[1] = rightColumnOffset;
			return array;
		}
		int num = 0;
		WorksheetImpl workSheet = m_workSheet;
		if (rightColumnOffset != 0)
		{
			num = workSheet.GetViewColumnWidthPixel(rightColumn);
			int num2 = (int)((double)((float)(num * rightColumnOffset) / SizeProperties.FULL_COLUMN_OFFSET) + 0.5);
			if (right <= num2)
			{
				array[0] = rightColumn;
				array[1] = (int)((double)((float)(num2 - right) * SizeProperties.FULL_COLUMN_OFFSET / (float)num + (float)rightColumnOffset) + 0.5);
				return array;
			}
			right -= num2;
		}
		for (rightColumn--; rightColumn >= 0; rightColumn--)
		{
			num = workSheet.GetViewColumnWidthPixel(rightColumn);
			right -= num;
			if (right <= 0)
			{
				break;
			}
		}
		if (right <= 0 && (right != 0 || rightColumn > 0))
		{
			array[0] = rightColumn;
			if (right != 0)
			{
				array[0] = rightColumn;
				array[1] = (int)((double)((float)(-right) * SizeProperties.FULL_COLUMN_OFFSET / (float)num) + 0.5);
			}
		}
		return array;
	}

	internal void SetAnchor(int left, int top, int right, int bottom)
	{
		m_size.SetPlacementType(PlacementType.FreeFloating);
		m_size.Top = top;
		m_size.Left = left;
		m_size.Right = right;
		m_size.Bottom = bottom;
	}

	internal void SetAnchor(int topRow, int top, int leftColumn, int left, int height, int width)
	{
		PlacementType placement = Placement;
		int innerRowHeightInPixels = m_workSheet.GetInnerRowHeightInPixels(topRow);
		top = (int)((double)((float)top * SizeProperties.FULL_ROW_OFFSET / (float)innerRowHeightInPixels) + 0.5);
		innerRowHeightInPixels = m_workSheet.GetViewColumnWidthPixel(leftColumn);
		left = (int)((double)((float)left * SizeProperties.FULL_COLUMN_OFFSET / (float)innerRowHeightInPixels) + 0.5);
		m_size.Right = width;
		m_size.Bottom = height;
		m_size.SetLeftColumn(leftColumn);
		m_size.Left = left;
		m_size.SetTopRow(topRow);
		m_size.Top = top;
		if (placement != PlacementType.Move)
		{
			m_size.SetPlacementType(PlacementType.Move);
			Placement = placement;
		}
	}

	internal void SetAnchor(int topRow, int topRowOffset, int leftColumn, int leftColumnOffset, int bottomRow, int bottomRowOffset, int rightColumn, int rightColumnOffset)
	{
		PlacementType placement = Placement;
		int innerRowHeightInPixels = m_workSheet.GetInnerRowHeightInPixels(topRow);
		topRowOffset = ((innerRowHeightInPixels > topRowOffset) ? ((int)((double)((float)topRowOffset * SizeProperties.FULL_ROW_OFFSET / (float)innerRowHeightInPixels) + 0.5)) : ((int)SizeProperties.FULL_ROW_OFFSET));
		innerRowHeightInPixels = m_workSheet.GetViewColumnWidthPixel(leftColumn);
		leftColumnOffset = ((innerRowHeightInPixels > leftColumnOffset) ? ((int)((double)((float)leftColumnOffset * SizeProperties.FULL_COLUMN_OFFSET / (float)innerRowHeightInPixels) + 0.5)) : ((int)SizeProperties.FULL_COLUMN_OFFSET));
		m_size.SetLeftColumn(leftColumn);
		m_size.Left = leftColumnOffset;
		m_size.SetTopRow(topRow);
		m_size.Top = topRowOffset;
		innerRowHeightInPixels = m_workSheet.GetInnerRowHeightInPixels(bottomRow);
		bottomRowOffset = ((innerRowHeightInPixels > bottomRowOffset) ? ((int)((double)((float)bottomRowOffset * SizeProperties.FULL_ROW_OFFSET / (float)innerRowHeightInPixels) + 0.5)) : ((int)SizeProperties.FULL_ROW_OFFSET));
		if (rightColumn < 16383)
		{
			innerRowHeightInPixels = m_workSheet.GetViewColumnWidthPixel(rightColumn);
			rightColumnOffset = ((innerRowHeightInPixels > rightColumnOffset) ? ((int)((double)((float)rightColumnOffset * SizeProperties.FULL_COLUMN_OFFSET / (float)innerRowHeightInPixels) + 0.5)) : ((int)SizeProperties.FULL_COLUMN_OFFSET));
		}
		else
		{
			rightColumn = 16383;
			rightColumnOffset = (int)SizeProperties.FULL_COLUMN_OFFSET;
		}
		m_size.SetRightColumn(rightColumn);
		m_size.Right = rightColumnOffset;
		m_size.SetBottomRow(bottomRow);
		m_size.Bottom = bottomRowOffset;
		if (placement != PlacementType.MoveAndSize)
		{
			m_size.SetPlacementType(PlacementType.MoveAndSize);
			Placement = placement;
		}
	}
}
