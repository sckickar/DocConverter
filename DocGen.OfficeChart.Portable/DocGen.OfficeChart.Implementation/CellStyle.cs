using System;

namespace DocGen.OfficeChart.Implementation;

internal class CellStyle : ExtendedFormatWrapper, IStyle, IExtendedFormat, IParentApplication, IOptimizedUpdate
{
	private RangeImpl m_range;

	private bool m_bAskAdjacent = true;

	public override ChartColor LeftBorderColor => GetLeftBorderColor(AskAdjacent);

	public override ChartColor RightBorderColor => GetRightBorderColor(AskAdjacent);

	public override ChartColor TopBorderColor => GetTopBorderColor(AskAdjacent);

	public override ChartColor BottomBorderColor => GetBottomBorderColor(AskAdjacent);

	public override OfficeLineStyle LeftBorderLineStyle
	{
		get
		{
			if (m_range.IsMerged && m_range.MergeArea.Column != m_range.Column)
			{
				return GetLeftLineStyle(askAdjecent: false);
			}
			return GetLeftLineStyle(AskAdjacent);
		}
		set
		{
			if (GetLeftLineStyle(askAdjecent: false) != value)
			{
				base.LeftBorderLineStyle = value;
			}
		}
	}

	public override OfficeLineStyle RightBorderLineStyle
	{
		get
		{
			if (m_range.IsMerged && m_range.MergeArea.LastColumn != m_range.Column)
			{
				return GetRightLineStyle(askAdjecent: false);
			}
			return GetRightLineStyle(AskAdjacent);
		}
		set
		{
			if (GetRightLineStyle(askAdjecent: false) != value)
			{
				base.RightBorderLineStyle = value;
			}
		}
	}

	public override OfficeLineStyle TopBorderLineStyle
	{
		get
		{
			if (m_range.IsMerged && m_range.MergeArea.Row != m_range.Row)
			{
				return GetTopLineStyle(askAdjecent: false);
			}
			return GetTopLineStyle(AskAdjacent);
		}
		set
		{
			if (GetTopLineStyle(askAdjecent: false) != value)
			{
				base.TopBorderLineStyle = value;
			}
		}
	}

	public override OfficeLineStyle BottomBorderLineStyle
	{
		get
		{
			if (m_range.IsMerged && m_range.MergeArea.LastColumn != m_range.Column)
			{
				return GetBottomLineStyle(askAdjecent: false);
			}
			return GetBottomLineStyle(AskAdjacent);
		}
		set
		{
			if (GetBottomLineStyle(askAdjecent: false) != value)
			{
				base.BottomBorderLineStyle = value;
			}
		}
	}

	internal bool AskAdjacent
	{
		get
		{
			return m_bAskAdjacent;
		}
		set
		{
			m_bAskAdjacent = value;
		}
	}

	public CellStyle(RangeImpl range)
		: base(range.Workbook)
	{
		m_range = range;
	}

	public CellStyle(RangeImpl range, int iXFIndex)
		: base(range.Workbook, iXFIndex)
	{
		m_range = range;
	}

	public override void BeginUpdate()
	{
		if (base.BeginCallsCount == 0)
		{
			BeforeRead();
			m_xFormat = m_book.CreateExtFormatWithoutRegister(m_xFormat);
		}
		base.BeginUpdate();
	}

	public override void EndUpdate()
	{
		base.EndUpdate();
		if (base.BeginCallsCount != 0)
		{
			return;
		}
		m_xFormat = m_book.AddExtendedProperties(m_xFormat);
		if (m_xFormat.Index <= m_book.DefaultXFIndex || !m_book.m_xfCellCount.ContainsKey(m_xFormat.Index))
		{
			m_xFormat = m_book.RegisterExtFormat(m_xFormat);
			m_range.ExtendedFormatIndex = (ushort)m_xFormat.Index;
			return;
		}
		m_book.m_xfCellCount.TryGetValue(m_xFormat.Index, out var value);
		if (value == 1)
		{
			m_book.InnerExtFormats[m_xFormat.Index].UpdateFromCurrentExtendedFormat(m_xFormat);
			m_range.ExtendedFormatIndex = (ushort)m_xFormat.Index;
		}
		else
		{
			m_book.m_xfCellCount[m_xFormat.Index] = value - 1;
			m_xFormat = m_book.RegisterExtFormat(m_xFormat);
			m_range.ExtendedFormatIndex = (ushort)m_xFormat.Index;
		}
	}

	protected override void SetParents(object parent)
	{
		m_range = CommonObject.FindParent(parent, typeof(RangeImpl)) as RangeImpl;
		if (m_range == null)
		{
			throw new ArgumentNullException("parent", "Can't find parent range.");
		}
		m_book = m_range.Workbook;
	}

	protected override void BeforeRead()
	{
		if (base.BeginCallsCount == 0)
		{
			base.BeforeRead();
			SetFormatIndex(m_range.ExtendedFormatIndex);
		}
	}

	protected OfficeLineStyle GetLeftLineStyle(bool askAdjecent)
	{
		RangeImpl rangeImpl = null;
		bool flag = true;
		if (m_range.IsMerged && askAdjecent)
		{
			rangeImpl = m_range;
			IRange[] cells = m_range.MergeArea.Cells;
			m_range = cells[0] as RangeImpl;
			if (!rangeImpl.CellsList.Contains(m_range))
			{
				flag = false;
			}
		}
		OfficeLineStyle officeLineStyle = base.LeftBorderLineStyle;
		if (officeLineStyle == OfficeLineStyle.None && askAdjecent && flag)
		{
			IRange leftCell = GetLeftCell();
			if (leftCell != null && leftCell.Columns[0].ColumnWidth != 0.0)
			{
				officeLineStyle = (leftCell.CellStyle as CellStyle).GetRightLineStyle(askAdjecent: false);
			}
		}
		if (rangeImpl != null)
		{
			m_range = rangeImpl;
		}
		return officeLineStyle;
	}

	protected OfficeLineStyle GetRightLineStyle(bool askAdjecent)
	{
		RangeImpl rangeImpl = null;
		bool flag = true;
		if (m_range.IsMerged && askAdjecent)
		{
			rangeImpl = m_range;
			m_range = m_range.MergeArea.Cells[^1] as RangeImpl;
			if (!rangeImpl.CellsList.Contains(m_range))
			{
				flag = false;
			}
		}
		OfficeLineStyle officeLineStyle = base.RightBorderLineStyle;
		if (officeLineStyle == OfficeLineStyle.None && askAdjecent && flag)
		{
			IRange rightCell = GetRightCell();
			if (rightCell != null && rightCell.Columns[0].ColumnWidth != 0.0)
			{
				officeLineStyle = (rightCell.CellStyle as CellStyle).GetLeftLineStyle(askAdjecent: false);
			}
		}
		if (rangeImpl != null)
		{
			m_range = rangeImpl;
		}
		return officeLineStyle;
	}

	protected OfficeLineStyle GetTopLineStyle(bool askAdjecent)
	{
		RangeImpl rangeImpl = null;
		bool flag = true;
		if (m_range.IsMerged && askAdjecent)
		{
			rangeImpl = m_range;
			IRange[] cells = m_range.MergeArea.Cells;
			m_range = cells[0] as RangeImpl;
			if (!rangeImpl.CellsList.Contains(m_range))
			{
				flag = false;
			}
		}
		OfficeLineStyle officeLineStyle = base.TopBorderLineStyle;
		if (officeLineStyle == OfficeLineStyle.None && askAdjecent && flag)
		{
			IRange topCell = GetTopCell();
			if (topCell != null && topCell.Rows[0].RowHeight != 0.0)
			{
				officeLineStyle = (topCell.CellStyle as CellStyle).GetBottomLineStyle(askAdjecent: false);
			}
		}
		if (rangeImpl != null)
		{
			m_range = rangeImpl;
		}
		return officeLineStyle;
	}

	protected OfficeLineStyle GetBottomLineStyle(bool askAdjecent)
	{
		RangeImpl rangeImpl = null;
		bool flag = true;
		if (m_range.IsMerged && askAdjecent)
		{
			rangeImpl = m_range;
			m_range = m_range.MergeArea.Cells[^1] as RangeImpl;
			if (!rangeImpl.CellsList.Contains(m_range))
			{
				flag = false;
			}
		}
		OfficeLineStyle officeLineStyle = base.BottomBorderLineStyle;
		if (officeLineStyle == OfficeLineStyle.None && askAdjecent && flag)
		{
			IRange bottomCell = GetBottomCell();
			if (bottomCell != null && bottomCell.Rows[0].RowHeight != 0.0)
			{
				officeLineStyle = (bottomCell.CellStyle as CellStyle).GetTopLineStyle(askAdjecent: false);
			}
		}
		if (rangeImpl != null)
		{
			m_range = rangeImpl;
		}
		return officeLineStyle;
	}

	protected ChartColor GetLeftBorderColor(bool askAdjacent)
	{
		ChartColor result = base.LeftBorderColor;
		if (base.LeftBorderLineStyle == OfficeLineStyle.None && askAdjacent)
		{
			IRange leftCell = GetLeftCell();
			if (leftCell != null)
			{
				result = (leftCell.CellStyle as CellStyle).GetRightBorderColor(askAdjacent: false);
			}
		}
		return result;
	}

	protected ChartColor GetRightBorderColor(bool askAdjacent)
	{
		ChartColor result = base.RightBorderColor;
		if (base.RightBorderLineStyle == OfficeLineStyle.None && askAdjacent)
		{
			IRange rightCell = GetRightCell();
			if (rightCell != null)
			{
				result = (rightCell.CellStyle as CellStyle).GetLeftBorderColor(askAdjacent: false);
			}
		}
		return result;
	}

	protected ChartColor GetTopBorderColor(bool askAdjecent)
	{
		ChartColor result = base.TopBorderColor;
		if (base.TopBorderLineStyle == OfficeLineStyle.None && askAdjecent)
		{
			IRange topCell = GetTopCell();
			if (topCell != null)
			{
				result = (topCell.CellStyle as CellStyle).GetBottomBorderColor(askAdjecent: false);
			}
		}
		return result;
	}

	protected ChartColor GetBottomBorderColor(bool askAdjecent)
	{
		ChartColor result = base.BottomBorderColor;
		if (base.BottomBorderLineStyle == OfficeLineStyle.None && askAdjecent)
		{
			IRange bottomCell = GetBottomCell();
			if (bottomCell != null)
			{
				result = (bottomCell.CellStyle as CellStyle).GetTopBorderColor(askAdjecent: false);
			}
		}
		return result;
	}

	private IRange GetLeftCell()
	{
		return GetCell(0, -1);
	}

	private IRange GetRightCell()
	{
		return GetCell(0, 1);
	}

	private IRange GetTopCell()
	{
		return GetCell(-1, 0);
	}

	private IRange GetBottomCell()
	{
		return GetCell(1, 0);
	}

	private IRange GetCell(int rowDelta, int colDelta)
	{
		int num = m_range.Row + rowDelta;
		int num2 = m_range.Column + colDelta;
		IRange result = null;
		if (num > 0 && num <= m_book.MaxRowCount && num2 > 0 && num2 <= m_book.MaxColumnCount)
		{
			result = m_range[num, num2];
		}
		return result;
	}
}
