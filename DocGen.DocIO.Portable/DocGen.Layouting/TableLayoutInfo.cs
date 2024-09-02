using System;
using DocGen.DocIO;
using DocGen.DocIO.DLS;

namespace DocGen.Layouting;

internal class TableLayoutInfo : LayoutInfo, ITableLayoutInfo, ILayoutSpacingsInfo
{
	private WTable m_table;

	private float[] m_cellsWidth;

	private int m_headersRowCount;

	private bool[] m_isDefaultCells;

	private float m_width;

	private float m_height = float.MinValue;

	private float m_headerRowHeight;

	private byte m_bFlags;

	private Spacings m_paddings;

	private Spacings m_margins;

	public float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	public float Height
	{
		get
		{
			if (m_height == float.MinValue)
			{
				m_height = GetTableHeight();
			}
			return m_height;
		}
	}

	public float[] CellsWidth
	{
		get
		{
			return m_cellsWidth;
		}
		set
		{
			m_cellsWidth = value;
		}
	}

	public int HeadersRowCount => m_headersRowCount;

	public bool[] IsDefaultCells => m_isDefaultCells;

	public bool IsSplittedTable
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

	internal float HeaderRowHeight
	{
		get
		{
			return m_headerRowHeight;
		}
		set
		{
			m_headerRowHeight = value;
		}
	}

	internal bool IsHeaderRowHeightUpdated
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

	internal bool IsHeaderNotRepeatForAllPages
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

	public double CellSpacings
	{
		get
		{
			if (m_table.Owner.Owner is WTableCell)
			{
				double val = (m_table.Owner.Owner as WTableCell).OwnerRow.RowFormat.CellSpacing * 2f;
				return Math.Max(0.0, val);
			}
			return 0.0;
		}
	}

	public double CellPaddings
	{
		get
		{
			if (m_table.Owner.Owner is WTableCell)
			{
				return (m_table.Owner.Owner as WTableCell).CellFormat.Paddings.Left + (m_table.Owner.Owner as WTableCell).CellFormat.Paddings.Right;
			}
			return 0.0;
		}
	}

	public TableLayoutInfo(WTable table)
		: base(ChildrenLayoutDirection.Horizontal)
	{
		m_table = table;
		int num = 0;
		int index = 0;
		float num2 = 0f;
		for (int i = 0; i < m_table.Rows.Count; i++)
		{
			float num3 = 0f;
			for (int j = 0; j < m_table.Rows[i].Cells.Count; j++)
			{
				num3 += m_table.Rows[i].Cells[j].Width;
			}
			if (i == 0 || num2 < num3)
			{
				num2 = num3;
				num = m_table.Rows[i].Cells.Count;
				index = i;
			}
		}
		if (!IsSplittedTable)
		{
			AddParagraphToEmptyCell(m_table);
			m_table.ApplyBaseStyleFormats();
		}
		if (!m_table.IsInCell && m_table.Document.ActualFormatType != 0 && m_table.Document.ActualFormatType != FormatType.Dot)
		{
			m_table.AutoFitColumns(forceAutoFitToContent: false);
		}
		else if ((m_table.Document.ActualFormatType != 0 || !m_table.TableFormat.IsAutoResized || m_table.PreferredTableWidth.Width != 0f || !CheckNeedToAutoFit(m_table)) && m_table.Document.ActualFormatType == FormatType.Doc)
		{
			m_table.DocAutoFitColumns();
		}
		m_width = m_table.Width;
		m_cellsWidth = new float[num];
		m_isDefaultCells = new bool[num];
		int k = 0;
		for (int num4 = num; k < num4; k++)
		{
			WTableCell wTableCell = m_table.Rows[index].Cells[k];
			m_cellsWidth[k] = wTableCell.Width;
			m_isDefaultCells[k] = wTableCell.IsFixedWidth;
		}
		m_headersRowCount = GetHeadersRowCount();
	}

	private bool CheckNeedToAutoFit(WTable table)
	{
		bool result = false;
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				if (cell.PreferredWidth.Width == 0f)
				{
					result = true;
					continue;
				}
				return false;
			}
		}
		return result;
	}

	private void AddParagraphToEmptyCell(WTable table)
	{
		for (int i = 0; i < table.Rows.Count; i++)
		{
			WTableRow wTableRow = table.Rows[i];
			for (int j = 0; j < wTableRow.Cells.Count; j++)
			{
				WTableCell wTableCell = wTableRow.Cells[j];
				bool flag = true;
				if (wTableCell.Paragraphs.Count == 0)
				{
					flag = false;
					for (int k = 0; k < wTableCell.ChildEntities.Count; k++)
					{
						if (flag)
						{
							break;
						}
						if (wTableCell.ChildEntities[k] is BlockContentControl)
						{
							flag = (wTableCell.ChildEntities[k] as BlockContentControl).ContainsParagraph();
						}
					}
				}
				if (!flag)
				{
					wTableCell.AddParagraph();
				}
			}
		}
	}

	private int GetHeadersRowCount()
	{
		int num = 0;
		for (int i = 0; i < m_table.Rows.Count && m_table.Rows[i].IsHeader; i++)
		{
			num++;
		}
		return num;
	}

	private float GetTableHeight()
	{
		float num = 0f;
		foreach (WTableRow row in m_table.Rows)
		{
			for (int i = 0; i < row.Cells.Count; i++)
			{
				_ = ((IWidget)row.Cells[i]).LayoutInfo;
			}
			num += GetRowHeight(row);
		}
		return num;
	}

	private float GetRowHeight(WTableRow row)
	{
		float result = row.Height;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (row.Cells.Count > 0)
		{
			Spacings margins = (((IWidget)row.Cells[0]).LayoutInfo as CellLayoutInfo).Margins;
			Spacings paddings = (((IWidget)row.Cells[0]).LayoutInfo as CellLayoutInfo).Paddings;
			num = margins.Top;
			num2 = margins.Bottom;
			num3 = paddings.Top;
		}
		if (row.HeightType == TableRowHeightType.Exactly)
		{
			result = row.Height + num2;
		}
		else if (row.HeightType == TableRowHeightType.AtLeast)
		{
			result = row.Height + num + num2 + num3;
		}
		return result;
	}
}
