using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.DocIO.DLS.XML;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WTable : TextBodyItem, IWTable, ICompositeEntity, IEntity, ITableWidget, IWidget
{
	private WRowCollection m_rows;

	private RowFormat m_initTableFormat;

	internal float m_tableWidth = float.MinValue;

	private WTableColumnCollection m_tableGrid;

	private XmlTableFormat m_xmlTblFormat;

	internal XmlTableFormat m_trackTblFormat;

	internal WTableColumnCollection m_trackTableGrid;

	private IWTableStyle m_style;

	private string m_title;

	private string m_description;

	private short m_bFlags = 21;

	private byte m_bFlags1 = 144;

	private byte m_bFlags2 = 144;

	internal List<WTable> m_recalculateTables;

	private int maxPrefWidthRowIndex = -1;

	internal List<float> m_maxPrefCellWidthOfColumns;

	private float m_totalmaxPrefCellWidthOfColumns;

	public override EntityType EntityType => EntityType.Table;

	public WRowCollection Rows => m_rows;

	public RowFormat TableFormat
	{
		get
		{
			if (m_initTableFormat == null)
			{
				m_initTableFormat = new RowFormat();
				m_initTableFormat.SetOwner(this);
			}
			if (m_initTableFormat.IsDefault && Rows != null && Rows.Count > 0)
			{
				m_initTableFormat.ImportContainer(FirstRow.RowFormat);
				m_initTableFormat.RemoveRowSprms();
			}
			return m_initTableFormat;
		}
	}

	internal PreferredWidthInfo PreferredTableWidth => TableFormat.PreferredWidth;

	public string StyleName
	{
		get
		{
			if (m_style == null)
			{
				return null;
			}
			return m_style.Name;
		}
	}

	public WTableCell LastCell
	{
		get
		{
			WTableRow lastRow = LastRow;
			if (lastRow != null)
			{
				int count = lastRow.Cells.Count;
				if (count <= 0)
				{
					return null;
				}
				return lastRow.Cells[count - 1];
			}
			return null;
		}
	}

	public WTableRow FirstRow => Rows.FirstItem as WTableRow;

	public WTableRow LastRow => Rows.LastItem as WTableRow;

	public WTableCell this[int row, int column] => Rows[row].Cells[column];

	public float Width
	{
		get
		{
			if (m_tableWidth == float.MinValue)
			{
				m_tableWidth = UpdateWidth();
			}
			return m_tableWidth;
		}
	}

	public EntityCollection ChildEntities => m_rows;

	internal WTableColumnCollection TableGrid
	{
		get
		{
			if (!IsTableGridUpdated)
			{
				m_tableGrid = null;
			}
			else if (m_rows != null && m_rows.Count > 0 && !base.Document.IsOpening && !base.Document.IsCloning && !base.Document.IsMailMerge && (base.Document.ActualFormatType.ToString().Contains("Docx") || base.Document.ActualFormatType.ToString().Contains("Word")) && (!IsTableGridVerified || IsTableGridCorrupted))
			{
				CheckTableGrid();
			}
			if (m_tableGrid == null)
			{
				if (m_rows != null && m_rows.Count > 0)
				{
					UpdateTableGrid(isTableGridMissMatch: false, isgridafter: true);
				}
				else
				{
					m_tableGrid = new WTableColumnCollection(base.Document);
				}
				IsTableGridUpdated = true;
			}
			return m_tableGrid;
		}
	}

	public float IndentFromLeft
	{
		get
		{
			return TableFormat.LeftIndent;
		}
		set
		{
			if (value > 1080f || value < -1080f)
			{
				throw new ArgumentOutOfRangeException("IndentFromLeft", "Table Indent must be between -1080 and 1080");
			}
			foreach (WTableRow row in Rows)
			{
				row.RowFormat.LeftIndent = value;
			}
			TableFormat.LeftIndent = value;
		}
	}

	internal XmlTableFormat DocxTableFormat
	{
		get
		{
			if (m_xmlTblFormat == null)
			{
				m_xmlTblFormat = new XmlTableFormat(this);
			}
			return m_xmlTblFormat;
		}
		set
		{
			m_xmlTblFormat = value;
		}
	}

	internal XmlTableFormat TrackTblFormat
	{
		get
		{
			if (m_trackTblFormat == null)
			{
				m_trackTblFormat = new XmlTableFormat(this);
			}
			return m_trackTblFormat;
		}
	}

	internal WTableColumnCollection TrackTableGrid
	{
		get
		{
			if (m_trackTableGrid == null)
			{
				m_trackTableGrid = new WTableColumnCollection(base.Document);
			}
			return m_trackTableGrid;
		}
	}

	public bool ApplyStyleForHeaderRow
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFFE) | (value ? 1 : 0));
		}
	}

	public bool ApplyStyleForLastRow
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFFD) | (int)((value ? 1u : 0u) << 1));
		}
	}

	public bool ApplyStyleForFirstColumn
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFFB) | (int)((value ? 1u : 0u) << 2));
		}
	}

	public bool ApplyStyleForLastColumn
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFF7) | (int)((value ? 1u : 0u) << 3));
		}
	}

	public bool ApplyStyleForBandedRows
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFEF) | (int)((value ? 1u : 0u) << 4));
		}
	}

	public bool ApplyStyleForBandedColumns
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFDF) | (int)((value ? 1u : 0u) << 5));
		}
	}

	public string Title
	{
		get
		{
			return m_title;
		}
		set
		{
			m_title = value;
		}
	}

	public string Description
	{
		get
		{
			return m_description;
		}
		set
		{
			m_description = value;
		}
	}

	internal bool IsFrame
	{
		get
		{
			if (Rows.Count > 0 && Rows[0].Cells.Count > 0 && Rows[0].Cells[0].Paragraphs.Count > 0)
			{
				return Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.IsFrame;
			}
			return false;
		}
	}

	internal bool IsCompleteFrame
	{
		get
		{
			bool flag = false;
			foreach (WTableRow row in Rows)
			{
				if (row.Cells.Count > 0 && row.Cells[0].Paragraphs.Count > 0)
				{
					flag = row.Cells[0].Paragraphs[0].ParagraphFormat.IsFrame;
				}
				if (!flag)
				{
					break;
				}
			}
			return flag;
		}
	}

	internal bool IsTableGridUpdated
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFFBF) | (int)((value ? 1u : 0u) << 6));
		}
	}

	internal bool IsTableGridVerified
	{
		get
		{
			return (m_bFlags & 0x100) >> 8 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFEFF) | (int)((value ? 1u : 0u) << 8));
		}
	}

	internal bool IsTableGridCorrupted
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFF7F) | (int)((value ? 1u : 0u) << 7));
		}
	}

	internal bool IsTableCellWidthDefined
	{
		get
		{
			return (m_bFlags & 0x200) >> 9 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xFDFF) | (int)((value ? 1u : 0u) << 9));
		}
	}

	internal bool IsUpdateCellWidthByPartitioning
	{
		get
		{
			return (m_bFlags & 0x400) >> 10 != 0;
		}
		set
		{
			bool flag = value && IsCellWidthZero();
			if (flag)
			{
				IsTableGridUpdated = false;
				IsTableGridVerified = false;
			}
			m_bFlags = (short)((m_bFlags & 0xFBFF) | (int)((flag ? 1u : 0u) << 10));
		}
	}

	internal bool UsePreferredCellWidth
	{
		get
		{
			return (m_bFlags & 0x800) >> 11 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xF7FF) | (int)((value ? 1u : 0u) << 11));
		}
	}

	internal bool IsInCell
	{
		get
		{
			if (base.Owner is WTableCell)
			{
				return true;
			}
			if (base.Owner is WTextBody && (base.Owner as WTextBody).Owner is BlockContentControl)
			{
				return IsSDTInTableCell((base.Owner as WTextBody).Owner as BlockContentControl);
			}
			return false;
		}
	}

	internal bool IsAllCellsHavePointWidth
	{
		get
		{
			return (m_bFlags & 0x1000) >> 12 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xEFFF) | (int)((value ? 1u : 0u) << 12));
		}
	}

	internal bool HasOnlyParagraphs
	{
		get
		{
			return (m_bFlags & 0x2000) >> 13 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xDFFF) | (int)((value ? 1u : 0u) << 13));
		}
	}

	internal bool IsRecalulateBasedOnLastCol
	{
		get
		{
			return (m_bFlags & 0x4000) >> 14 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0xBFFF) | (int)((value ? 1u : 0u) << 14));
		}
	}

	internal bool HasOnlyHorizontalText
	{
		get
		{
			return (m_bFlags & 0x8000) >> 15 != 0;
		}
		set
		{
			m_bFlags = (short)((m_bFlags & 0x7FFF) | (int)((value ? 1u : 0u) << 15));
		}
	}

	internal bool HasPercentPreferredCellWidth
	{
		get
		{
			return (m_bFlags1 & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool HasAutoPreferredCellWidth
	{
		get
		{
			return (m_bFlags1 & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool HasNonePreferredCellWidth
	{
		get
		{
			return (m_bFlags1 & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool HasPointPreferredCellWidth
	{
		get
		{
			return (m_bFlags1 & 1) != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsNeedToRecalculate
	{
		get
		{
			return (m_bFlags1 & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal List<WTable> RecalculateTables
	{
		get
		{
			if (m_recalculateTables == null)
			{
				m_recalculateTables = new List<WTable>();
			}
			return m_recalculateTables;
		}
	}

	internal bool HasHorizontalMergeCells
	{
		get
		{
			return (m_bFlags1 & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal bool IsFromHTML
	{
		get
		{
			return (m_bFlags2 & 1) != 0;
		}
		set
		{
			m_bFlags2 = (byte)((m_bFlags2 & 0xFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsAllRowsHaveSameCellCount
	{
		get
		{
			return (m_bFlags2 & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags2 = (byte)((m_bFlags2 & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	int ITableWidget.MaxRowIndex
	{
		get
		{
			int num = 0;
			int result = 0;
			for (int i = 0; i < m_rows.Count; i++)
			{
				if (num < m_rows[i].Cells.Count)
				{
					num = m_rows[i].Cells.Count;
					result = i;
				}
			}
			return result;
		}
	}

	int ITableWidget.RowsCount => m_rows.Count;

	internal void ChangeTrackTableGrid()
	{
		TableGrid.Close();
		m_tableGrid = new WTableColumnCollection(base.Document);
		foreach (WTableColumn item in TrackTableGrid)
		{
			m_tableGrid.AddColumns(item.EndOffset);
		}
		m_trackTableGrid = null;
	}

	public WTable(IWordDocument doc)
		: this(doc, showBorder: false)
	{
		if (base.Document.IsOpening || base.Document.IsCloning || base.Document.IsMailMerge)
		{
			return;
		}
		IStyleCollection styles = base.Document.Styles;
		IStyle style = styles.FindByName("TableGrid");
		if (style != null && style.StyleType != StyleType.TableStyle)
		{
			RenameDupliateStyle(style, styles);
		}
		if (!(style is WTableStyle))
		{
			style = styles.FindByName("Table Grid");
			if (style != null && style.StyleType != StyleType.TableStyle)
			{
				RenameDupliateStyle(style, styles);
			}
		}
		if (!(style is WTableStyle))
		{
			style = CreateTableGridStyle(styles);
		}
		if (style is WTableStyle)
		{
			ApplyStyle(style.Name, isClearCellShading: false);
		}
	}

	private void RenameDupliateStyle(IStyle style, IStyleCollection docStyles)
	{
		int num = 0;
		string name = style.Name + "_" + num;
		while (docStyles.FindByName(name) != null)
		{
			num++;
			name = style.Name + "_" + num;
		}
		style.Name = name;
	}

	private WTableStyle CreateTableGridStyle(IStyleCollection docStyles)
	{
		WTableStyle wTableStyle = new WTableStyle(base.Document);
		wTableStyle.Name = "Table Grid";
		string[] array = new string[4] { "Normal Table", "NormalTable", "Table Normal", "TableNormal" };
		IStyle style = null;
		string[] array2 = array;
		foreach (string name in array2)
		{
			style = docStyles.FindByName(name, StyleType.TableStyle);
			if (style != null)
			{
				break;
			}
		}
		if (style == null)
		{
			array2 = array;
			foreach (string name2 in array2)
			{
				style = docStyles.FindByName(name2);
				if (style != null)
				{
					if (style.StyleType == StyleType.TableStyle)
					{
						break;
					}
					RenameDupliateStyle(style, docStyles);
				}
			}
			if (!(style is WTableStyle))
			{
				style = CreateTableNormalStyle(docStyles);
			}
		}
		if (style is WTableStyle)
		{
			wTableStyle.ApplyBaseStyle(style as WTableStyle);
		}
		wTableStyle.TableProperties.LeftIndent = 0f;
		wTableStyle.TableProperties.Borders.BorderType = BorderStyle.Single;
		wTableStyle.TableProperties.Borders.Color = Color.Black;
		docStyles.Add(wTableStyle);
		base.Document.StyleNameIds.Add("TableGrid", wTableStyle.Name);
		return wTableStyle;
	}

	private WTableStyle CreateTableNormalStyle(IStyleCollection styleCollection)
	{
		WTableStyle wTableStyle = new WTableStyle(base.Document);
		wTableStyle.Name = "Normal Table";
		wTableStyle.IsSemiHidden = true;
		wTableStyle.UnhideWhenUsed = true;
		wTableStyle.IsPrimaryStyle = true;
		wTableStyle.UIPriority = 99;
		wTableStyle.TableProperties.LeftIndent = 0f;
		wTableStyle.TableProperties.Paddings.Top = 0f;
		wTableStyle.TableProperties.Paddings.Left = 5.4f;
		wTableStyle.TableProperties.Paddings.Right = 5.4f;
		wTableStyle.TableProperties.Paddings.Bottom = 0f;
		styleCollection.Add(wTableStyle);
		return wTableStyle;
	}

	public WTable(IWordDocument doc, bool showBorder)
		: base((WordDocument)doc)
	{
		m_rows = new WRowCollection(this);
		if (showBorder)
		{
			TableFormat.Borders.BorderType = BorderStyle.Single;
			TableFormat.Borders.Color = Color.Black;
			TableFormat.Borders.LineWidth = 1f;
		}
	}

	public new WTable Clone()
	{
		return (WTable)CloneImpl();
	}

	public void ResetCells(int rowsNum, int columnsNum)
	{
		if (rowsNum <= 0 || columnsNum <= 0)
		{
			throw new ArgumentException("Table should have atleast 1 row and 1 column");
		}
		if (columnsNum > 63)
		{
			throw new ArgumentException("The number of cells must be between 1 and 63.");
		}
		float cellWidth = GetOwnerWidth() / (float)columnsNum;
		ResetCells(rowsNum, columnsNum, null, cellWidth);
	}

	public void ResetCells(int rowsNum, int columnsNum, RowFormat format, float cellWidth)
	{
		if (rowsNum <= 0 || columnsNum <= 0)
		{
			throw new ArgumentException("Table should have atleast 1 row and 1 column");
		}
		if (columnsNum > 63)
		{
			throw new ArgumentException("Not supported more than 63 cells.");
		}
		if (format != null)
		{
			TableFormat.ClearFormatting();
			TableFormat.ImportContainer(format);
		}
		m_rows.Clear();
		WTableRow wTableRow = AddRow();
		rowsNum--;
		while (columnsNum > 0)
		{
			WTableCell wTableCell = new WTableCell(base.Document);
			wTableCell.Width = cellWidth;
			wTableCell.PreferredWidth.Width = cellWidth;
			wTableCell.PreferredWidth.WidthType = FtsWidth.Point;
			wTableRow.Cells.Add(wTableCell);
			columnsNum--;
		}
		while (rowsNum > 0)
		{
			AddRow();
			rowsNum--;
		}
	}

	public void ApplyStyle(BuiltinTableStyle builtinTableStyle)
	{
		ApplyStyle(builtinTableStyle, isClearCellShading: true);
	}

	internal void ApplyStyle(BuiltinTableStyle builtinTableStyle, bool isClearCellShading)
	{
		IStyle builtInTableStyle = base.Document.GetBuiltInTableStyle(builtinTableStyle);
		ApplyStyle(builtInTableStyle as IWTableStyle, isClearCellShading);
	}

	public WTableRow AddRow()
	{
		return AddRow(isCopyFormat: true, autoPopulateCells: true);
	}

	public WTableRow AddRow(bool isCopyFormat)
	{
		return AddRow(isCopyFormat, autoPopulateCells: true);
	}

	public WTableRow AddRow(bool isCopyFormat, bool autoPopulateCells)
	{
		WTableRow wTableRow = new WTableRow(base.Document);
		if (autoPopulateCells)
		{
			WTableRow lastRow = LastRow;
			if (lastRow != null)
			{
				WTableCell wTableCell = null;
				int i = 0;
				for (int count = lastRow.Cells.Count; i < count; i++)
				{
					wTableCell = lastRow.Cells[i];
					WTableCell wTableCell2 = new WTableCell(base.Document);
					wTableRow.Cells.Add(wTableCell2);
					wTableCell2.Width = wTableCell.Width;
					if (isCopyFormat)
					{
						wTableCell2.CellFormat.ImportContainer(wTableCell.CellFormat);
					}
				}
				wTableRow.Height = lastRow.Height;
			}
		}
		if (isCopyFormat)
		{
			if (LastRow != null)
			{
				wTableRow.RowFormat.ImportContainer(LastRow.RowFormat);
			}
			else
			{
				wTableRow.RowFormat.ImportContainer(TableFormat);
			}
		}
		Rows.Add(wTableRow);
		return wTableRow;
	}

	public override int Replace(Regex pattern, string replace)
	{
		int num = 0;
		foreach (WTableRow row in Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				foreach (TextBodyItem childEntity in cell.ChildEntities)
				{
					num += childEntity.Replace(pattern, replace);
					if (base.Document.ReplaceFirst && num > 0)
					{
						return num;
					}
				}
			}
		}
		return num;
	}

	public override int Replace(string given, string replace, bool caseSensitive, bool wholeWord)
	{
		Regex pattern = FindUtils.StringToRegex(given, caseSensitive, wholeWord);
		return Replace(pattern, replace);
	}

	public override int Replace(Regex pattern, TextSelection textSelection)
	{
		return Replace(pattern, textSelection, saveFormatting: false);
	}

	public override int Replace(Regex pattern, TextSelection textSelection, bool saveFormatting)
	{
		textSelection.CacheRanges();
		int num = 0;
		foreach (WTableRow row in m_rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				num += cell.Replace(pattern, textSelection, saveFormatting);
				if (base.Document.ReplaceFirst && num > 0)
				{
					return num;
				}
			}
		}
		return num;
	}

	public override TextSelection Find(Regex pattern)
	{
		foreach (WTableRow row in m_rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				TextSelection textSelection = cell.Find(pattern);
				if (textSelection != null && textSelection.Count > 0)
				{
					return textSelection;
				}
			}
		}
		return null;
	}

	public void ApplyVerticalMerge(int columnIndex, int startRowIndex, int endRowIndex)
	{
		if (m_rows == null || m_rows.Count == 0)
		{
			throw new Exception("Table rows are not initialized.");
		}
		if (startRowIndex < 0 || startRowIndex >= m_rows.Count)
		{
			throw new ArgumentOutOfRangeException("startRowIndex", "Row with specified row index doesn't exist");
		}
		if (endRowIndex < 0 || endRowIndex >= m_rows.Count)
		{
			throw new ArgumentOutOfRangeException("endRowIndex", "Row with specified row index doesn't exist");
		}
		if (startRowIndex > endRowIndex)
		{
			throw new Exception("Start row index is greater than end row index.");
		}
		if (columnIndex < 0)
		{
			throw new ArgumentOutOfRangeException("columnIndex", "Column with specified column index doesn't exist");
		}
		for (int i = startRowIndex; i <= endRowIndex; i++)
		{
			if (columnIndex >= m_rows[i].Cells.Count)
			{
				throw new ArgumentOutOfRangeException("columnIndex", "Column with specified column index doesn't exist");
			}
		}
		RemoveStartCellEmptyParagraph(columnIndex, startRowIndex, endRowIndex);
		m_rows[startRowIndex].Cells[columnIndex].CellFormat.VerticalMerge = CellMerge.Start;
		for (int j = startRowIndex + 1; j <= endRowIndex; j++)
		{
			RemoveContinueCellEmptyParagraph(columnIndex, j, endRowIndex);
			m_rows[j].Cells[columnIndex].CellFormat.VerticalMerge = CellMerge.Continue;
			m_rows[j].Cells[columnIndex].Items.CloneTo(m_rows[startRowIndex].Cells[columnIndex].Items);
			m_rows[j].Cells[columnIndex].Items.Clear();
		}
	}

	private void RemoveStartCellEmptyParagraph(int columnIndex, int startRowIndex, int endRowIndex)
	{
		bool flag = false;
		WTableCell wTableCell = m_rows[startRowIndex].Cells[columnIndex];
		if (wTableCell.Items.Count > 0 && wTableCell.LastParagraph != null)
		{
			if (wTableCell.LastParagraph.PreviousSibling != null && wTableCell.LastParagraph.PreviousSibling is WTable)
			{
				flag = true;
			}
			if (!HasRenderableItem(wTableCell.LastParagraph) && !flag && !IsFollowingCellsEmpty(columnIndex, startRowIndex, endRowIndex))
			{
				wTableCell.Items.Remove(wTableCell.LastParagraph);
			}
		}
	}

	private void RemoveContinueCellEmptyParagraph(int columnIndex, int rowIndex, int endRowIndex)
	{
		bool flag = false;
		WTableCell wTableCell = m_rows[rowIndex].Cells[columnIndex];
		if (wTableCell.Items.Count > 1 && wTableCell.LastParagraph != null && rowIndex != endRowIndex)
		{
			WTableCell wTableCell2 = m_rows[endRowIndex].Cells[columnIndex];
			if (wTableCell.LastParagraph.PreviousSibling != null && wTableCell.LastParagraph.PreviousSibling is WTable)
			{
				flag = true;
			}
			if (!HasRenderableItem(wTableCell.LastParagraph) && !flag && (rowIndex != endRowIndex - 1 || wTableCell2.Items.Count > 1 || (wTableCell2.Items.Count == 1 && wTableCell2.LastParagraph != null && wTableCell2.LastParagraph.Items.Count > 0 && HasRenderableItem(wTableCell2.LastParagraph))))
			{
				wTableCell.Items.Remove(wTableCell.LastParagraph);
			}
		}
		else if (wTableCell.Items.Count == 1 && wTableCell.LastParagraph != null && !HasRenderableItem(wTableCell.LastParagraph))
		{
			wTableCell.Items.Remove(wTableCell.LastParagraph);
		}
	}

	private bool IsFollowingCellsEmpty(int columnIndex, int startRowIndex, int endRowIndex)
	{
		for (int i = startRowIndex + 1; i <= endRowIndex; i++)
		{
			WTableCell wTableCell = m_rows[i].Cells[columnIndex];
			if (wTableCell.Items.Count > 1 || (wTableCell.Items.Count == 1 && wTableCell.LastParagraph != null && wTableCell.LastParagraph.Items.Count > 0 && HasRenderableItem(wTableCell.LastParagraph)))
			{
				return false;
			}
		}
		return true;
	}

	internal bool HasRenderableItem(IWParagraph paragraph)
	{
		for (int i = 0; i < paragraph.ChildEntities.Count; i++)
		{
			Entity entity = paragraph.ChildEntities[i];
			if (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && !(entity is EditableRangeStart) && !(entity is EditableRangeEnd))
			{
				return true;
			}
		}
		return false;
	}

	public void ApplyHorizontalMerge(int rowIndex, int startCellIndex, int endCellIndex)
	{
		if (m_rows == null || m_rows.Count == 0)
		{
			throw new Exception("Table rows are not initialized.");
		}
		if (rowIndex < 0 || rowIndex >= m_rows.Count)
		{
			throw new ArgumentOutOfRangeException("rowIndex", "Row with specified row index doesn't exist");
		}
		WCellCollection cells = m_rows[rowIndex].Cells;
		if (cells == null || cells.Count == 0)
		{
			throw new Exception("Table row cells are not initialized.");
		}
		if (startCellIndex < 0 || startCellIndex > cells.Count - 1)
		{
			throw new ArgumentOutOfRangeException("startCellIndex", "Cell with specified start cell index doesn't exist");
		}
		if (endCellIndex < 0 || endCellIndex > cells.Count - 1)
		{
			throw new ArgumentOutOfRangeException("endCellIndex", "Cell with specified end cell index doesn't exist");
		}
		if (startCellIndex > endCellIndex)
		{
			throw new Exception("Start cell index is greater than end cell index.");
		}
		UpdateStartcell(cells, startCellIndex, endCellIndex);
		cells[startCellIndex].CellFormat.HorizontalMerge = CellMerge.Start;
		for (int i = startCellIndex + 1; i <= endCellIndex; i++)
		{
			UpdateContinuecell(cells, i, endCellIndex);
			cells[i].CellFormat.HorizontalMerge = CellMerge.Continue;
			cells[i].Items.CloneTo(cells[startCellIndex].Items);
			cells[i].Items.Clear();
		}
	}

	internal void UpdateStartcell(WCellCollection cells, int startCellIndex, int endCellIndex)
	{
		bool isEmpty = false;
		bool isNotConsider = false;
		if (ChecksLastPara(cells[startCellIndex].Items, ref isEmpty, ref isNotConsider) && startCellIndex + 1 <= endCellIndex)
		{
			UpdateBookmark(cells, startCellIndex + 1, startCellIndex);
		}
		else if (startCellIndex + 1 <= endCellIndex)
		{
			UpdateParaItems(cells, startCellIndex + 1, startCellIndex, isEmpty);
		}
	}

	internal void UpdateContinuecell(WCellCollection cells, int cellIndex, int endCellIndex)
	{
		bool flag = false;
		bool isEmpty = false;
		bool isNotConsider = false;
		if (cellIndex != endCellIndex || cells[cellIndex].Items.Count < 2)
		{
			flag = ChecksLastPara(cells[cellIndex].Items, ref isEmpty, ref isNotConsider);
		}
		if (flag && cellIndex + 1 <= endCellIndex)
		{
			UpdateBookmark(cells, cellIndex + 1, cellIndex);
		}
		else if (flag && cellIndex == endCellIndex)
		{
			UpdateBookmark(cells, cellIndex, cellIndex - 1);
		}
		else if (cellIndex + 1 <= endCellIndex)
		{
			UpdateParaItems(cells, cellIndex + 1, cellIndex, isEmpty);
		}
	}

	internal void UpdateBookmark(WCellCollection cells, int currCellIndex, int prevCellIndex)
	{
		if (cells[currCellIndex].Items.FirstItem is WParagraph && cells[prevCellIndex].Items.LastItem is WParagraph)
		{
			EntityCollection childEntities = (cells[currCellIndex].Items.FirstItem as WParagraph).ChildEntities;
			EntityCollection childEntities2 = (cells[prevCellIndex].Items.LastItem as WParagraph).ChildEntities;
			int count = childEntities2.Count;
			for (int i = 0; i < count; i++)
			{
				childEntities.Insert(0, childEntities2[childEntities2.Count - 1]);
			}
			cells[prevCellIndex].Items.LastItem.RemoveSelf();
		}
		else
		{
			if (!(cells[currCellIndex].Items.FirstItem is WTable))
			{
				return;
			}
			WTable wTable = cells[currCellIndex].Items.FirstItem as WTable;
			if (wTable.Rows.Count > 0 && wTable.Rows[0].Cells.Count > 0 && wTable.Rows[0].Cells[0].ChildEntities.Count > 0 && wTable.Rows[0].Cells[0].ChildEntities.FirstItem is WParagraph && cells[prevCellIndex].Items.LastItem is WParagraph)
			{
				EntityCollection childEntities3 = (wTable.Rows[0].Cells[0].ChildEntities.FirstItem as WParagraph).ChildEntities;
				EntityCollection childEntities4 = (cells[prevCellIndex].Items.LastItem as WParagraph).ChildEntities;
				int count2 = childEntities4.Count;
				for (int j = 0; j < count2; j++)
				{
					childEntities3.Insert(0, childEntities4[childEntities4.Count - 1]);
				}
				cells[prevCellIndex].Items.LastItem.RemoveSelf();
			}
		}
	}

	internal void UpdateParaItems(WCellCollection cells, int currCellIndex, int prevCellIndex, bool isPrevCellEmpty)
	{
		if (cells[currCellIndex].Items.Count == 1 && cells[currCellIndex].Items[0] is WParagraph)
		{
			bool isEmpty = false;
			bool isNotConsider = false;
			bool flag = ChecksLastPara(cells[currCellIndex].Items, ref isEmpty, ref isNotConsider);
			if (isEmpty || flag || isNotConsider)
			{
				if (cells[prevCellIndex].Items.LastItem is WParagraph && cells[currCellIndex].Items.FirstItem is WParagraph)
				{
					EntityCollection childEntities = (cells[currCellIndex].Items.FirstItem as WParagraph).ChildEntities;
					EntityCollection childEntities2 = (cells[prevCellIndex].Items.LastItem as WParagraph).ChildEntities;
					int count = childEntities2.Count;
					for (int i = 0; i < count; i++)
					{
						childEntities.Insert(0, childEntities2[childEntities2.Count - 1]);
					}
					cells[prevCellIndex].Items.LastItem.RemoveSelf();
				}
			}
			else if (isPrevCellEmpty)
			{
				cells[prevCellIndex].Items.LastItem.RemoveSelf();
			}
		}
		else if (isPrevCellEmpty)
		{
			cells[prevCellIndex].Items.LastItem.RemoveSelf();
		}
	}

	private bool ChecksLastPara(BodyItemCollection items, ref bool isEmpty, ref bool isNotConsider)
	{
		bool result = false;
		if (items.LastItem is WParagraph && ((items.LastItem.PreviousSibling != null && items.LastItem.PreviousSibling.EntityType != EntityType.Table) || items.LastItem.PreviousSibling == null))
		{
			WParagraph wParagraph = items.LastItem as WParagraph;
			if (wParagraph.ChildEntities.Count > 0)
			{
				foreach (ParagraphItem childEntity in wParagraph.ChildEntities)
				{
					if (childEntity is BookmarkStart)
					{
						if ((childEntity as BookmarkStart).Name != "_GoBack")
						{
							result = true;
							isNotConsider = false;
						}
						else
						{
							isNotConsider = true;
						}
						continue;
					}
					if (childEntity is BookmarkEnd)
					{
						if ((childEntity as BookmarkEnd).Name != "_GoBack")
						{
							result = true;
							isNotConsider = false;
						}
						else
						{
							isNotConsider = true;
						}
						continue;
					}
					result = false;
					isNotConsider = false;
					break;
				}
			}
			else
			{
				isEmpty = true;
			}
		}
		return result;
	}

	public void RemoveAbsPosition()
	{
		foreach (WTableRow row in m_rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				foreach (TextBodyItem item in cell.Items)
				{
					if (item is WParagraph)
					{
						(item as WParagraph).RemoveAbsPosition();
					}
					else if (item is WTable)
					{
						(item as WTable).RemoveAbsPosition();
					}
				}
			}
			if (row.RowFormat != null)
			{
				row.RowFormat.RemovePositioning();
			}
		}
	}

	internal void GetMinimumAndMaximumWordWidth(ref float minimumWordWidth, ref float maximumWordWidth)
	{
		float ownerWidth = GetOwnerWidth();
		bool num = IsTableBasedOnContent(this);
		AutoFitColumns(forceAutoFitToContent: false);
		if (num)
		{
			CheckToRecalculatAgain();
		}
		float num2 = m_tableGrid.GetTotalWidth(0);
		if ((ownerWidth < num2 && !IsSkipToResizeNestedTable(num2)) || TableFormat.PreferredWidth.WidthType == FtsWidth.Percentage)
		{
			float num3 = num2 / ownerWidth;
			num2 *= num3;
		}
		if (num2 > minimumWordWidth)
		{
			minimumWordWidth = num2;
		}
		if (num2 > maximumWordWidth)
		{
			maximumWordWidth = num2;
		}
	}

	private void CheckToRecalculatAgain()
	{
		if (IsInCell)
		{
			WTableCell wTableCell = base.Owner as WTableCell;
			WTable wTable = GetOwnerTable(base.Owner) as WTable;
			bool flag = wTableCell != null && wTableCell.Index == (wTableCell.Owner as WTableRow).Cells.Count - 1;
			wTable.IsRecalulateBasedOnLastCol = flag && IsTableBasedOnContent(wTable);
		}
	}

	private bool IsSkipToResizeNestedTable(float tableWidth)
	{
		WTable wTable = null;
		WTableCell wTableCell = null;
		WSection ownerSection = GetOwnerSection();
		if (IsInCell)
		{
			wTableCell = GetOwnerTableCell();
			wTable = ((wTableCell.OwnerRow != null) ? wTableCell.OwnerRow.OwnerTable : null);
		}
		if ((m_doc.ActualFormatType == FormatType.Docx || m_doc.ActualFormatType == FormatType.Word2013) && wTableCell != null && wTable != null && wTable.TableFormat.IsAutoResized && tableWidth < ownerSection.PageSetup.ClientWidth)
		{
			if (wTable.Rows.Count == 1 && wTable.Rows[0].Cells.Count == 1 && wTable.PreferredTableWidth.WidthType == FtsWidth.Auto && wTableCell.PreferredWidth.WidthType == FtsWidth.Auto && !wTable.IsInCell && !wTable.TableFormat.WrapTextAround)
			{
				return true;
			}
			if (wTable.IsInCell || wTable.IsNeedToRecalculate)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsContainsDifferentKeys(WTable previousTable, WTable currentTable, int Key)
	{
		if (previousTable.TableFormat.Paddings.HasKey(Key) != currentTable.TableFormat.Paddings.HasKey(Key))
		{
			return true;
		}
		if (previousTable.TableFormat.Paddings.HasKey(Key))
		{
			switch (Key)
			{
			case 1:
				return previousTable.TableFormat.Paddings.Left != currentTable.TableFormat.Paddings.Left;
			case 4:
				return previousTable.TableFormat.Paddings.Right != currentTable.TableFormat.Paddings.Right;
			case 2:
				return previousTable.TableFormat.Paddings.Top != currentTable.TableFormat.Paddings.Top;
			case 3:
				return previousTable.TableFormat.Paddings.Bottom != currentTable.TableFormat.Paddings.Bottom;
			}
		}
		return false;
	}

	private bool IsTableHasSamePadding(WTable previousTable, WTable currentTable)
	{
		if (IsContainsDifferentKeys(previousTable, currentTable, 1))
		{
			return false;
		}
		if (IsContainsDifferentKeys(previousTable, currentTable, 4))
		{
			return false;
		}
		if (IsContainsDifferentKeys(previousTable, currentTable, 2))
		{
			return false;
		}
		if (IsContainsDifferentKeys(previousTable, currentTable, 3))
		{
			return false;
		}
		return true;
	}

	private void ChangeSamePaddingTableState(WTableCell tableCell)
	{
		if (tableCell.CellFormat.SamePaddingsAsTable)
		{
			tableCell.CellFormat.SamePaddingsAsTable = false;
		}
	}

	private void SetPaddingValue(WTableRow row)
	{
		for (int i = 0; i < row.Cells.Count; i++)
		{
			WTableCell wTableCell = row.Cells[i];
			if (!wTableCell.CellFormat.Paddings.HasKey(1) && !row.RowFormat.Paddings.HasKey(1))
			{
				ChangeSamePaddingTableState(wTableCell);
				wTableCell.CellFormat.Paddings.Left = wTableCell.GetLeftPadding();
			}
			if (!wTableCell.CellFormat.Paddings.HasKey(4) && !row.RowFormat.Paddings.HasKey(4))
			{
				ChangeSamePaddingTableState(wTableCell);
				wTableCell.CellFormat.Paddings.Right = wTableCell.GetRightPadding();
			}
			if (!wTableCell.CellFormat.Paddings.HasKey(2) && !row.RowFormat.Paddings.HasKey(2))
			{
				ChangeSamePaddingTableState(wTableCell);
				wTableCell.CellFormat.Paddings.Top = wTableCell.GetTopPadding();
			}
			if (!wTableCell.CellFormat.Paddings.HasKey(3) && !row.RowFormat.Paddings.HasKey(3))
			{
				ChangeSamePaddingTableState(wTableCell);
				wTableCell.CellFormat.Paddings.Bottom = wTableCell.GetBottomPadding();
			}
		}
	}

	internal void MergeTables(WTable currentTable)
	{
		WTable wTable = currentTable.PreviousSibling as WTable;
		if (wTable.Title == null)
		{
			wTable.Title = currentTable.Title;
		}
		if (wTable.Description == null)
		{
			wTable.Description = currentTable.Description;
		}
		bool flag = !wTable.LastRow.IsHeader;
		bool flag2 = IsTableHasSamePadding(wTable, currentTable);
		while (currentTable.Rows.Count > 0)
		{
			WTableRow wTableRow = currentTable.Rows[0];
			if (!flag2)
			{
				SetPaddingValue(wTableRow);
			}
			wTable.Rows.Add(wTableRow);
			wTable.LastRow.HasTblPrEx = true;
			SetMergedRowBorders(currentTable, wTable, wTable.LastRow);
			if (flag && wTable.LastRow.IsHeader)
			{
				wTable.LastRow.IsHeader = false;
			}
		}
		if (wTable.HasOnlyParagraphs)
		{
			wTable.HasOnlyParagraphs = currentTable.HasOnlyParagraphs;
		}
		currentTable.OwnerTextBody.ChildEntities.Remove(currentTable);
		wTable.UpdateTableGrid(isTableGridMissMatch: false, isgridafter: true);
		wTable.UpdateGridSpan();
	}

	private void SetMergedRowBorders(WTable currentTable, WTable previousTable, WTableRow tableRow)
	{
		string[] array = new string[8] { "Left", "Right", "Top", "Bottom", "Horizontal", "Vertical", "DiagonalDown", "DiagonalUp" };
		foreach (string border in array)
		{
			Borders borders = currentTable.TableFormat.Borders;
			if (borders == null)
			{
				break;
			}
			Border border2 = GetBorder(border, borders);
			if (border2 == null)
			{
				break;
			}
			BorderStyle borderType = border2.BorderType;
			if (HasBorder(borderType, border2))
			{
				continue;
			}
			Borders borders2 = previousTable.TableFormat.Borders;
			if (borders2 == null)
			{
				break;
			}
			Border border3 = GetBorder(border, borders2);
			if (border3 == null)
			{
				break;
			}
			BorderStyle borderType2 = border3.BorderType;
			if (HasBorder(borderType2, border3))
			{
				if (!(m_doc.Styles.FindByName(currentTable.StyleName) is WTableStyle wTableStyle))
				{
					break;
				}
				Borders borders3 = wTableStyle.TableProperties.Borders;
				if (borders3 == null)
				{
					break;
				}
				Border border4 = GetBorder(border, borders3);
				if (border4 == null)
				{
					break;
				}
				BorderStyle borderType3 = border4.BorderType;
				if (HasBorder(borderType3, border4))
				{
					border4.UpdateSourceFormatting(GetBorder(border, tableRow.RowFormat.Borders));
				}
			}
		}
	}

	private Border GetBorder(string border, Borders borders)
	{
		return border switch
		{
			"Left" => borders.Left, 
			"Right" => borders.Right, 
			"Top" => borders.Top, 
			"Bottom" => borders.Bottom, 
			"Horizontal" => borders.Horizontal, 
			"Vertical" => borders.Vertical, 
			"DiagonalDown" => borders.DiagonalDown, 
			"DiagonalUp" => borders.DiagonalUp, 
			_ => null, 
		};
	}

	private bool HasBorder(BorderStyle borderStyle, Border tableBorder)
	{
		if (borderStyle != 0 || tableBorder.HasNoneStyle)
		{
			if (borderStyle == BorderStyle.Cleared)
			{
				return !tableBorder.HasNoneStyle;
			}
			return true;
		}
		return false;
	}

	internal void AutoFitTable(AutoFitType autoFitType)
	{
		TableFormat.IsAutoResized = autoFitType != AutoFitType.FixedColumnWidth;
		if (autoFitType == AutoFitType.FitToContent)
		{
			ClearPreferredWidths(beforeAutoFit: true);
		}
		AutoFitColumns(autoFitType == AutoFitType.FitToContent);
		UpdatePreferredWidthProperties(TableFormat.IsAutoResized, autoFitType);
	}

	internal bool IsSDTInTableCell(BlockContentControl sdtBlock)
	{
		while (sdtBlock != null)
		{
			if (sdtBlock.Owner is WTableCell)
			{
				return true;
			}
			if (sdtBlock.Owner is WTextBody && (sdtBlock.Owner as WTextBody).Owner is BlockContentControl)
			{
				sdtBlock = (sdtBlock.Owner as WTextBody).Owner as BlockContentControl;
				continue;
			}
			return false;
		}
		return false;
	}

	internal WTableCell GetOwnerTableCell()
	{
		if (base.Owner is WTableCell)
		{
			return base.Owner as WTableCell;
		}
		BlockContentControl blockContentControl = ((base.Owner is WTextBody && (base.Owner as WTextBody).Owner is BlockContentControl) ? ((base.Owner as WTextBody).Owner as BlockContentControl) : null);
		while (blockContentControl != null)
		{
			if (blockContentControl.Owner is WTableCell)
			{
				return blockContentControl.Owner as WTableCell;
			}
			if (blockContentControl.Owner is WTextBody && (blockContentControl.Owner as WTextBody).Owner is BlockContentControl)
			{
				blockContentControl = (blockContentControl.Owner as WTextBody).Owner as BlockContentControl;
				continue;
			}
			return null;
		}
		return null;
	}

	internal string GetTableText()
	{
		string text = string.Empty;
		foreach (WTableRow row in Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				for (int i = 0; i < cell.Items.Count; i++)
				{
					if (cell.Items[i] is WParagraph)
					{
						text += (cell.Items[i] as WParagraph).GetParagraphText(isLastPargraph: false);
					}
					else if (cell.Items[i] is WTable)
					{
						text += (cell.Items[i] as WTable).GetTableText();
					}
					if (base.Document.m_prevClonedEntity != null && base.Document.m_prevClonedEntity.OwnerTextBody == cell)
					{
						i = base.Document.m_prevClonedEntity.GetIndexInOwnerCollection();
						base.Document.m_prevClonedEntity = null;
					}
					if (i == cell.Items.Count - 1 && cell.CellFormat.CurCellIndex < row.Cells.Count - 1)
					{
						text = text.Substring(0, text.Length - 1);
						text += ControlChar.Tab;
					}
				}
			}
		}
		return text;
	}

	public IWTableStyle GetStyle()
	{
		return m_style;
	}

	private void ApplyStyle(IWTableStyle style, bool isClearCellShading)
	{
		if (style == null)
		{
			throw new ArgumentNullException("newStyle");
		}
		m_style = style;
		DocxTableFormat.StyleName = m_style.Name;
		if (isClearCellShading)
		{
			RemoveCellBackGroundColor();
		}
	}

	private void RemoveCellBackGroundColor()
	{
		foreach (WTableRow row in Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				CellFormat cellFormat = cell.CellFormat;
				if (cellFormat != null)
				{
					if (cellFormat.PropertiesHash.ContainsKey(4))
					{
						cellFormat.PropertiesHash.Remove(4);
					}
					if (cellFormat.PropertiesHash.ContainsKey(7))
					{
						cellFormat.PropertiesHash.Remove(7);
					}
				}
			}
		}
	}

	public void ApplyStyle(string styleName)
	{
		if (!(base.Document.Styles.FindByName(styleName, StyleType.TableStyle) is IWTableStyle style))
		{
			throw new ArgumentNullException("newStyle");
		}
		ApplyStyle(style, isClearCellShading: true);
	}

	internal void ApplyStyle(string styleName, bool isClearCellShading)
	{
		if (!(base.Document.Styles.FindByName(styleName, StyleType.TableStyle) is IWTableStyle style))
		{
			throw new ArgumentNullException("newStyle");
		}
		ApplyStyle(style, isClearCellShading);
	}

	internal void ApplyBaseStyleFormats()
	{
		if (m_style == null)
		{
			return;
		}
		TableFormat.ApplyBase((m_style as WTableStyle).TableProperties.GetAsTableFormat());
		bool flag = IsStyleContainConditionalFormattingType(ConditionalFormattingType.FirstRow);
		bool flag2 = Rows.Count > 1 && Rows[1].RowFormat.IsHeaderRow;
		int num = 1;
		for (int i = 0; i < Rows.Count; i++)
		{
			if (!(flag && flag2) || (i != 0 && !Rows[i].RowFormat.IsHeaderRow))
			{
				flag2 = false;
			}
			else if (i != 0)
			{
				num++;
			}
			int num2 = i + 1;
			ConditionalFormattingStyle conditionalFormattingStyle = null;
			long rowStripe = (m_style as WTableStyle).TableProperties.RowStripe;
			bool isOddRow = rowStripe == 0L || ((!(ApplyStyleForHeaderRow && flag)) ? (num2 / rowStripe % 2 == 1) : (i != 0 && ((i - num) / rowStripe + 1) % 2 == 1));
			bool isContainsLastRowFormatting = IsStyleContainConditionalFormattingType(ConditionalFormattingType.LastRow);
			conditionalFormattingStyle = GetUpdatedcnfRowStyleFormatting(m_style as WTableStyle, i, flag2, isContainsLastRowFormatting, isOddRow, rowStripe);
			if (conditionalFormattingStyle != null)
			{
				Rows[i].RowFormat.ApplyBase(conditionalFormattingStyle.RowProperties.GetAsRowFormat());
				if (!Rows[i].RowFormat.HasValue(107))
				{
					Rows[i].IsHeader = conditionalFormattingStyle.RowProperties.IsHeader;
				}
			}
			else
			{
				Rows[i].RowFormat.ApplyBase((m_style as WTableStyle).RowProperties.GetAsRowFormat());
			}
			for (int j = 0; j < Rows[i].Cells.Count; j++)
			{
				int num3 = j + 1;
				long columnStripe = (m_style as WTableStyle).TableProperties.ColumnStripe;
				bool flag3 = IsStyleContainConditionalFormattingType(ConditionalFormattingType.FirstColumn);
				bool isOddColumn = columnStripe == 0L || ((!(ApplyStyleForFirstColumn && flag3)) ? (num3 / columnStripe % 2 == 1) : (j != 0 && ((j - 1) / columnStripe + 1) % 2 == 1));
				WParagraphFormat wParagraphFormat = new WParagraphFormat(base.Document);
				if (m_style.ParagraphFormat.BaseFormat != null)
				{
					wParagraphFormat.ApplyBase(m_style.ParagraphFormat.BaseFormat);
				}
				WCharacterFormat wCharacterFormat = new WCharacterFormat(base.Document);
				CellFormat cellFormat = new CellFormat();
				wParagraphFormat.CopyFormat(m_style.ParagraphFormat);
				wCharacterFormat.CopyFormat((m_style as WTableStyle).CharacterFormat);
				WTableStyle baseStyle = (m_style as WTableStyle).BaseStyle;
				while (baseStyle != null && baseStyle.IsCustom)
				{
					BaseStyleFormatCopy(baseStyle, wCharacterFormat, wParagraphFormat);
					baseStyle = baseStyle.BaseStyle;
				}
				baseStyle = null;
				cellFormat.UpdateCellFormat((m_style as WTableStyle).CellProperties);
				if (conditionalFormattingStyle != null)
				{
					cellFormat.UpdateCellFormat(conditionalFormattingStyle.CellProperties);
					UpdateRowBorders(cellFormat.Borders, conditionalFormattingStyle.CellProperties.Borders, TableFormat.Borders, j, Rows[i].Cells.Count, isLeftRightWidthapplicable: true);
					wParagraphFormat.CopyFormat(conditionalFormattingStyle.ParagraphFormat);
					wCharacterFormat.CopyFormat(conditionalFormattingStyle.CharacterFormat);
				}
				ConditionalFormattingStyle conditionalFormattingStyle2 = null;
				foreach (ConditionalFormattingType item in new List<ConditionalFormattingType>
				{
					ConditionalFormattingType.FirstColumn,
					ConditionalFormattingType.LastColumn,
					ConditionalFormattingType.OddColumnBanding,
					ConditionalFormattingType.EvenColumnBanding,
					ConditionalFormattingType.FirstRowLastCell,
					ConditionalFormattingType.FirstRowFirstCell,
					ConditionalFormattingType.LastRowLastCell,
					ConditionalFormattingType.LastRowFirstCell
				})
				{
					conditionalFormattingStyle2 = GetUpdatedcnfCellStyleFormatting(m_style as WTableStyle, item, i, j, isOddColumn, columnStripe);
					if (conditionalFormattingStyle2 == null)
					{
						continue;
					}
					cellFormat.UpdateCellFormat(conditionalFormattingStyle2.CellProperties);
					UpdateColumnBorders(cellFormat.Borders, conditionalFormattingStyle2.CellProperties.Borders, TableFormat.Borders, i, Rows.Count, isLeftRightWidthapplicable: true);
					wParagraphFormat.CopyFormat(conditionalFormattingStyle2.ParagraphFormat);
					wCharacterFormat.CopyFormat(conditionalFormattingStyle2.CharacterFormat);
					if (conditionalFormattingStyle != null)
					{
						if (conditionalFormattingStyle.ConditionalFormattingType == ConditionalFormattingType.FirstRow)
						{
							wParagraphFormat.CopyFormat(conditionalFormattingStyle.ParagraphFormat);
							wCharacterFormat.CopyFormat(conditionalFormattingStyle.CharacterFormat);
						}
						cellFormat.UpdateCellFormat(conditionalFormattingStyle.CellProperties);
						UpdateRowBorders(cellFormat.Borders, conditionalFormattingStyle.CellProperties.Borders, TableFormat.Borders, j, Rows[i].Cells.Count, conditionalFormattingStyle2.ConditionalFormattingType != ConditionalFormattingType.OddColumnBanding && conditionalFormattingStyle2.ConditionalFormattingType != ConditionalFormattingType.EvenColumnBanding);
					}
					switch (conditionalFormattingStyle2.ConditionalFormattingType)
					{
					case ConditionalFormattingType.FirstColumn:
					case ConditionalFormattingType.OddColumnBanding:
					case ConditionalFormattingType.EvenColumnBanding:
						if (conditionalFormattingStyle == null || conditionalFormattingStyle.ConditionalFormattingType != 0)
						{
							cellFormat.UpdateCellFormat(conditionalFormattingStyle2.CellProperties);
							UpdateColumnBorders(cellFormat.Borders, conditionalFormattingStyle2.CellProperties.Borders, TableFormat.Borders, i, Rows.Count, conditionalFormattingStyle2.ConditionalFormattingType != ConditionalFormattingType.OddColumnBanding && conditionalFormattingStyle2.ConditionalFormattingType != ConditionalFormattingType.EvenColumnBanding);
						}
						break;
					case ConditionalFormattingType.FirstRowLastCell:
					case ConditionalFormattingType.FirstRowFirstCell:
					case ConditionalFormattingType.LastRowLastCell:
					case ConditionalFormattingType.LastRowFirstCell:
						cellFormat.CopyFormat(conditionalFormattingStyle2.CellProperties);
						wParagraphFormat.CopyFormat(conditionalFormattingStyle2.ParagraphFormat);
						wCharacterFormat.CopyFormat(conditionalFormattingStyle2.CharacterFormat);
						break;
					}
				}
				Rows[i].Cells[j].ApplyTableStyleBaseFormats(cellFormat, wParagraphFormat, wCharacterFormat, Rows[i].Cells[j].Items);
			}
		}
	}

	private bool IsStyleContainConditionalFormattingType(ConditionalFormattingType formattingType)
	{
		WTableStyle wTableStyle = m_style as WTableStyle;
		while (wTableStyle != null && (wTableStyle == m_style as WTableStyle || wTableStyle.Name != "Normal Table"))
		{
			foreach (ConditionalFormattingStyle conditionalFormattingStyle in wTableStyle.ConditionalFormattingStyles)
			{
				if (conditionalFormattingStyle.ConditionalFormattingType == formattingType)
				{
					return true;
				}
			}
			wTableStyle = wTableStyle.BaseStyle;
		}
		return false;
	}

	private ConditionalFormattingStyle GetUpdatedcnfRowStyleFormatting(WTableStyle tableStyle, int rowIndex, bool isContinuesHeaderRow, bool isContainsLastRowFormatting, bool isOddRow, long rowStripe)
	{
		ConditionalFormattingStyle conditionalFormattingStyle = GetConditionalFormattingRowStyle(tableStyle, rowIndex, isContinuesHeaderRow, isContainsLastRowFormatting, isOddRow, rowStripe);
		WTableStyle baseStyle = tableStyle.BaseStyle;
		while (baseStyle != null && baseStyle.Name != "Normal Table")
		{
			ConditionalFormattingStyle conditionalFormattingRowStyle = GetConditionalFormattingRowStyle(baseStyle, rowIndex, isContinuesHeaderRow, isContainsLastRowFormatting, isOddRow, rowStripe);
			if (conditionalFormattingStyle == null)
			{
				conditionalFormattingStyle = conditionalFormattingRowStyle;
			}
			else if (conditionalFormattingRowStyle != null)
			{
				UpdatePropertiesKeys(conditionalFormattingStyle.ParagraphFormat.PropertiesHash, conditionalFormattingRowStyle.ParagraphFormat.PropertiesHash);
				UpdatePropertiesKeys(conditionalFormattingStyle.CharacterFormat.PropertiesHash, conditionalFormattingRowStyle.CharacterFormat.PropertiesHash);
				UpdatePropertiesKeys(conditionalFormattingStyle.CellProperties.PropertiesHash, conditionalFormattingRowStyle.CellProperties.PropertiesHash);
				UpdatePropertiesKeys(conditionalFormattingStyle.RowProperties.PropertiesHash, conditionalFormattingRowStyle.RowProperties.PropertiesHash);
			}
			baseStyle = baseStyle.BaseStyle;
		}
		return conditionalFormattingStyle;
	}

	private ConditionalFormattingStyle GetConditionalFormattingRowStyle(WTableStyle style, int rowIndex, bool isContinuesHeaderRow, bool isContainsLastRowFormatting, bool isOddRow, long rowStripe)
	{
		ConditionalFormattingStyle conditionalFormattingStyle = null;
		foreach (ConditionalFormattingStyle conditionalFormattingStyle2 in style.ConditionalFormattingStyles)
		{
			switch (conditionalFormattingStyle2.ConditionalFormattingType)
			{
			case ConditionalFormattingType.FirstRow:
				if ((rowIndex == 0 && ApplyStyleForHeaderRow) || isContinuesHeaderRow)
				{
					conditionalFormattingStyle = conditionalFormattingStyle2;
				}
				break;
			case ConditionalFormattingType.LastRow:
				if (rowIndex != 0 && !isContinuesHeaderRow && conditionalFormattingStyle == null && rowIndex == Rows.Count - 1 && ApplyStyleForLastRow)
				{
					conditionalFormattingStyle = conditionalFormattingStyle2;
				}
				break;
			case ConditionalFormattingType.OddRowBanding:
				if (conditionalFormattingStyle == null && (rowIndex != Rows.Count - 1 || !ApplyStyleForLastRow || !isContainsLastRowFormatting) && isOddRow && rowStripe != 0L && ApplyStyleForBandedRows)
				{
					conditionalFormattingStyle = conditionalFormattingStyle2;
				}
				break;
			case ConditionalFormattingType.EvenRowBanding:
				if (conditionalFormattingStyle == null && rowIndex != 0 && (rowIndex != Rows.Count - 1 || !ApplyStyleForLastRow || !isContainsLastRowFormatting) && !isOddRow && ApplyStyleForBandedRows)
				{
					conditionalFormattingStyle = conditionalFormattingStyle2;
				}
				break;
			}
		}
		return conditionalFormattingStyle;
	}

	private ConditionalFormattingStyle GetUpdatedcnfCellStyleFormatting(WTableStyle tableStyle, ConditionalFormattingType conditionalFormattingType, int rowIndex, int cellIndex, bool isOddColumn, long colStripe)
	{
		ConditionalFormattingStyle conditionalFormattingStyle = GetConditionalFormattingCellStyle(tableStyle, conditionalFormattingType, rowIndex, cellIndex, isOddColumn, colStripe);
		WTableStyle baseStyle = tableStyle.BaseStyle;
		while (baseStyle != null && baseStyle.Name != "Normal Table")
		{
			ConditionalFormattingStyle conditionalFormattingCellStyle = GetConditionalFormattingCellStyle(baseStyle, conditionalFormattingType, rowIndex, cellIndex, isOddColumn, colStripe);
			if (conditionalFormattingStyle == null)
			{
				conditionalFormattingStyle = conditionalFormattingCellStyle;
			}
			else if (conditionalFormattingCellStyle != null)
			{
				UpdatePropertiesKeys(conditionalFormattingStyle.ParagraphFormat.PropertiesHash, conditionalFormattingCellStyle.ParagraphFormat.PropertiesHash);
				UpdatePropertiesKeys(conditionalFormattingStyle.CharacterFormat.PropertiesHash, conditionalFormattingCellStyle.CharacterFormat.PropertiesHash);
				UpdatePropertiesKeys(conditionalFormattingStyle.CellProperties.PropertiesHash, conditionalFormattingCellStyle.CellProperties.PropertiesHash);
				UpdatePropertiesKeys(conditionalFormattingStyle.RowProperties.PropertiesHash, conditionalFormattingCellStyle.RowProperties.PropertiesHash);
			}
			baseStyle = baseStyle.BaseStyle;
		}
		return conditionalFormattingStyle;
	}

	private ConditionalFormattingStyle GetConditionalFormattingCellStyle(WTableStyle tableStyle, ConditionalFormattingType conditionalFormattingType, int rowIndex, int cellIndex, bool isOddColumn, long colStripe)
	{
		ConditionalFormattingStyle result = null;
		foreach (ConditionalFormattingStyle conditionalFormattingStyle in tableStyle.ConditionalFormattingStyles)
		{
			if (conditionalFormattingStyle.ConditionalFormattingType != conditionalFormattingType)
			{
				continue;
			}
			switch (conditionalFormattingType)
			{
			case ConditionalFormattingType.FirstColumn:
				if (cellIndex == 0 && ApplyStyleForFirstColumn)
				{
					result = conditionalFormattingStyle;
				}
				break;
			case ConditionalFormattingType.LastColumn:
				if (cellIndex != 0 && cellIndex == Rows[rowIndex].Cells.Count - 1 && ApplyStyleForLastColumn)
				{
					result = conditionalFormattingStyle;
				}
				break;
			case ConditionalFormattingType.OddColumnBanding:
				if ((cellIndex != ((Rows[rowIndex].Cells.Count > 1) ? (Rows[rowIndex].Cells.Count - 1) : (Rows[rowIndex].Cells[cellIndex].GridSpan - 1)) || !ApplyStyleForLastColumn) && isOddColumn && colStripe != 0L && ApplyStyleForBandedColumns)
				{
					result = conditionalFormattingStyle;
				}
				break;
			case ConditionalFormattingType.EvenColumnBanding:
				if (cellIndex != 0 && (cellIndex != Rows[rowIndex].Cells.Count - 1 || !ApplyStyleForLastColumn) && !isOddColumn && ApplyStyleForBandedColumns)
				{
					result = conditionalFormattingStyle;
				}
				break;
			case ConditionalFormattingType.FirstRowLastCell:
				if (rowIndex == 0 && cellIndex != 0 && cellIndex == Rows[rowIndex].Cells.Count - 1 && ApplyStyleForHeaderRow && ApplyStyleForLastColumn)
				{
					result = conditionalFormattingStyle;
				}
				break;
			case ConditionalFormattingType.FirstRowFirstCell:
				if (rowIndex == 0 && cellIndex == 0 && ApplyStyleForHeaderRow && ApplyStyleForFirstColumn)
				{
					result = conditionalFormattingStyle;
				}
				break;
			case ConditionalFormattingType.LastRowLastCell:
				if (rowIndex != 0 && rowIndex == Rows.Count - 1 && cellIndex != 0 && cellIndex == Rows[rowIndex].Cells.Count - 1 && ApplyStyleForLastRow && ApplyStyleForLastColumn)
				{
					result = conditionalFormattingStyle;
				}
				break;
			case ConditionalFormattingType.LastRowFirstCell:
				if (rowIndex != 0 && rowIndex == Rows.Count - 1 && cellIndex == 0 && ApplyStyleForLastRow && ApplyStyleForFirstColumn)
				{
					result = conditionalFormattingStyle;
				}
				break;
			}
		}
		return result;
	}

	private void UpdatePropertiesKeys(Dictionary<int, object> destination, Dictionary<int, object> source)
	{
		foreach (KeyValuePair<int, object> item in source)
		{
			if (!destination.ContainsKey(item.Key))
			{
				destination.Add(item.Key, item.Value);
			}
		}
	}

	private void BaseStyleFormatCopy(WTableStyle baseStyle, WCharacterFormat characterFormat, WParagraphFormat paragraphFormat)
	{
		foreach (KeyValuePair<int, object> item in baseStyle.CharacterFormat.PropertiesHash)
		{
			if (!characterFormat.PropertiesHash.ContainsKey(item.Key))
			{
				characterFormat.PropertiesHash.Add(item.Key, item.Value);
			}
		}
		foreach (KeyValuePair<int, object> item2 in baseStyle.ParagraphFormat.PropertiesHash)
		{
			if (!paragraphFormat.PropertiesHash.ContainsKey(item2.Key))
			{
				paragraphFormat.PropertiesHash.Add(item2.Key, item2.Value);
			}
		}
	}

	internal CellFormat GetCellFormatFromStyle(int rowIndex, int cellIndex)
	{
		if (m_style != null)
		{
			int num = rowIndex + 1;
			ConditionalFormattingStyle conditionalFormattingStyle = null;
			long rowStripe = (m_style as WTableStyle).TableProperties.RowStripe;
			bool flag = false;
			foreach (ConditionalFormattingStyle conditionalFormattingStyle5 in (m_style as WTableStyle).ConditionalFormattingStyles)
			{
				if (conditionalFormattingStyle5.ConditionalFormattingType == ConditionalFormattingType.FirstRow)
				{
					flag = true;
					break;
				}
			}
			bool flag2 = rowStripe == 0L || ((!(ApplyStyleForHeaderRow && flag)) ? (num / rowStripe % 2 == 1) : (rowIndex != 0 && ((rowIndex - 1) / rowStripe + 1) % 2 == 1));
			foreach (ConditionalFormattingStyle conditionalFormattingStyle6 in (m_style as WTableStyle).ConditionalFormattingStyles)
			{
				switch (conditionalFormattingStyle6.ConditionalFormattingType)
				{
				case ConditionalFormattingType.FirstRow:
					if (rowIndex == 0 && ApplyStyleForHeaderRow)
					{
						conditionalFormattingStyle = conditionalFormattingStyle6;
					}
					break;
				case ConditionalFormattingType.LastRow:
					if (rowIndex != 0 && rowIndex == Rows.Count - 1 && ApplyStyleForLastRow)
					{
						conditionalFormattingStyle = conditionalFormattingStyle6;
					}
					break;
				case ConditionalFormattingType.OddRowBanding:
					if ((rowIndex != Rows.Count - 1 || !ApplyStyleForLastRow) && flag2 && rowStripe != 0L && ApplyStyleForBandedRows)
					{
						conditionalFormattingStyle = conditionalFormattingStyle6;
					}
					break;
				case ConditionalFormattingType.EvenRowBanding:
					if (rowIndex != 0 && (rowIndex != Rows.Count - 1 || !ApplyStyleForLastRow) && !flag2 && ApplyStyleForBandedRows)
					{
						conditionalFormattingStyle = conditionalFormattingStyle6;
					}
					break;
				}
			}
			int num2 = cellIndex + 1;
			long columnStripe = (m_style as WTableStyle).TableProperties.ColumnStripe;
			bool flag3 = false;
			foreach (ConditionalFormattingStyle conditionalFormattingStyle7 in (m_style as WTableStyle).ConditionalFormattingStyles)
			{
				if (conditionalFormattingStyle7.ConditionalFormattingType == ConditionalFormattingType.FirstColumn)
				{
					flag3 = true;
					break;
				}
			}
			bool flag4 = columnStripe == 0L || ((!(ApplyStyleForFirstColumn && flag3)) ? (num2 / columnStripe % 2 == 1) : (cellIndex != 0 && ((cellIndex - 1) / columnStripe + 1) % 2 == 1));
			CellFormat cellFormat = new CellFormat();
			cellFormat.UpdateCellFormat((m_style as WTableStyle).CellProperties);
			if (conditionalFormattingStyle != null)
			{
				cellFormat.UpdateCellFormat(conditionalFormattingStyle.CellProperties);
			}
			ConditionalFormattingStyle conditionalFormattingStyle3 = null;
			{
				foreach (ConditionalFormattingStyle conditionalFormattingStyle8 in (m_style as WTableStyle).ConditionalFormattingStyles)
				{
					switch (conditionalFormattingStyle8.ConditionalFormattingType)
					{
					case ConditionalFormattingType.FirstColumn:
						if (cellIndex == 0 && ApplyStyleForFirstColumn)
						{
							conditionalFormattingStyle3 = conditionalFormattingStyle8;
						}
						break;
					case ConditionalFormattingType.LastColumn:
						if (cellIndex != 0 && cellIndex == Rows[rowIndex].Cells.Count - 1 && ApplyStyleForLastColumn)
						{
							conditionalFormattingStyle3 = conditionalFormattingStyle8;
						}
						break;
					case ConditionalFormattingType.OddColumnBanding:
						if (cellIndex != Rows[rowIndex].Cells.Count - 1 && flag4 && columnStripe != 0L && ApplyStyleForBandedColumns)
						{
							conditionalFormattingStyle3 = conditionalFormattingStyle8;
						}
						break;
					case ConditionalFormattingType.EvenColumnBanding:
						if (cellIndex != 0 && cellIndex != Rows[rowIndex].Cells.Count - 1 && !flag4 && ApplyStyleForBandedColumns)
						{
							conditionalFormattingStyle3 = conditionalFormattingStyle8;
						}
						break;
					case ConditionalFormattingType.FirstRowLastCell:
						if (rowIndex == 0 && cellIndex != 0 && cellIndex == Rows[rowIndex].Cells.Count - 1 && ApplyStyleForHeaderRow && ApplyStyleForLastColumn)
						{
							conditionalFormattingStyle3 = conditionalFormattingStyle8;
						}
						break;
					case ConditionalFormattingType.FirstRowFirstCell:
						if (rowIndex == 0 && cellIndex == 0 && ApplyStyleForHeaderRow && ApplyStyleForFirstColumn)
						{
							conditionalFormattingStyle3 = conditionalFormattingStyle8;
						}
						break;
					case ConditionalFormattingType.LastRowLastCell:
						if (rowIndex != 0 && rowIndex == Rows.Count - 1 && cellIndex != 0 && cellIndex == Rows[rowIndex].Cells.Count - 1 && ApplyStyleForLastRow && ApplyStyleForLastColumn)
						{
							conditionalFormattingStyle3 = conditionalFormattingStyle8;
						}
						break;
					case ConditionalFormattingType.LastRowFirstCell:
						if (rowIndex != 0 && rowIndex == Rows.Count - 1 && cellIndex == 0 && ApplyStyleForLastRow && ApplyStyleForFirstColumn)
						{
							conditionalFormattingStyle3 = conditionalFormattingStyle8;
						}
						break;
					}
					if (conditionalFormattingStyle3 == null)
					{
						continue;
					}
					cellFormat.UpdateCellFormat(conditionalFormattingStyle3.CellProperties);
					if (conditionalFormattingStyle != null)
					{
						cellFormat.UpdateCellFormat(conditionalFormattingStyle.CellProperties);
					}
					switch (conditionalFormattingStyle3.ConditionalFormattingType)
					{
					case ConditionalFormattingType.FirstColumn:
						if (conditionalFormattingStyle == null || conditionalFormattingStyle.ConditionalFormattingType != 0)
						{
							cellFormat.UpdateCellFormat(conditionalFormattingStyle3.CellProperties);
						}
						break;
					case ConditionalFormattingType.FirstRowLastCell:
					case ConditionalFormattingType.FirstRowFirstCell:
					case ConditionalFormattingType.LastRowLastCell:
					case ConditionalFormattingType.LastRowFirstCell:
						cellFormat.CopyFormat(conditionalFormattingStyle3.CellProperties);
						break;
					}
				}
				return cellFormat;
			}
		}
		return null;
	}

	private void UpdateRowBorders(Borders dest, Borders src, Borders tableBorders, int index, int count, bool isLeftRightWidthapplicable)
	{
		if (src.NoBorder)
		{
			return;
		}
		dest.Top.CopyBorderFormatting(src.Top);
		dest.Bottom.CopyBorderFormatting(src.Bottom);
		if (index == 0 && isLeftRightWidthapplicable)
		{
			dest.Left.CopyBorderFormatting(src.Left);
		}
		if (index == count - 1 && isLeftRightWidthapplicable)
		{
			dest.Right.CopyBorderFormatting(src.Right);
		}
		if (src.Vertical.HasKey(2))
		{
			if (index < count - 1)
			{
				dest.Right.CopyBorderFormatting(src.Vertical);
			}
			if (index > 0 && index < count)
			{
				dest.Left.CopyBorderFormatting(src.Vertical);
			}
		}
	}

	private void UpdateColumnBorders(Borders dest, Borders src, Borders tableBorders, int index, int count, bool isLeftRightWidthapplicable)
	{
		if (src.NoBorder)
		{
			return;
		}
		dest.Left.CopyBorderFormatting(src.Left);
		dest.Right.CopyBorderFormatting(src.Right);
		if (index == 0 && isLeftRightWidthapplicable)
		{
			dest.Top.CopyBorderFormatting(src.Top);
		}
		if (index == count - 1 && isLeftRightWidthapplicable)
		{
			dest.Bottom.CopyBorderFormatting(src.Bottom);
		}
		if (!src.Horizontal.HasKey(2))
		{
			return;
		}
		if (index < count - 1)
		{
			dest.Bottom.CopyBorderFormatting(src.Horizontal);
			if (src.Horizontal.BorderType == BorderStyle.Cleared && tableBorders.Horizontal.BorderType != 0)
			{
				dest.Bottom.CopyBorderFormatting(tableBorders.Vertical);
			}
		}
		if (index > 0 && index < count)
		{
			dest.Top.CopyBorderFormatting(src.Horizontal);
			if (src.Horizontal.BorderType == BorderStyle.Cleared && tableBorders.Horizontal.BorderType != 0)
			{
				dest.Top.CopyBorderFormatting(tableBorders.Vertical);
			}
		}
	}

	internal override TextSelectionList FindAll(Regex pattern, bool isDocumentComparison)
	{
		TextSelectionList textSelectionList = null;
		if (isDocumentComparison)
		{
			return textSelectionList;
		}
		foreach (WTableRow row in m_rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				TextSelectionList textSelectionList2 = cell.FindAll(pattern, isDocumentComparison: false, isFromTextbody: false);
				if (textSelectionList2 != null && textSelectionList2.Count > 0)
				{
					if (textSelectionList == null)
					{
						textSelectionList = textSelectionList2;
					}
					else
					{
						textSelectionList.AddRange(textSelectionList2);
					}
				}
			}
		}
		return textSelectionList;
	}

	internal override void AddSelf()
	{
		foreach (WTableRow row in Rows)
		{
			row.AddSelf();
		}
	}

	protected override object CloneImpl()
	{
		WTable wTable = (WTable)base.CloneImpl();
		wTable.m_rows = new WRowCollection(wTable);
		wTable.m_initTableFormat = null;
		if (m_tableGrid != null)
		{
			wTable.m_tableGrid = m_tableGrid.Clone();
		}
		wTable.TableFormat.ImportContainer(TableFormat);
		wTable.TableFormat.SetOwner(wTable);
		if (m_xmlTblFormat != null)
		{
			wTable.m_xmlTblFormat = m_xmlTblFormat.Clone(wTable);
		}
		Rows.CloneTo(wTable.m_rows);
		if (m_style != null)
		{
			wTable.ApplyStyle(m_style.Clone() as IWTableStyle, isClearCellShading: false);
		}
		return wTable;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		int i = 0;
		for (int count = ChildEntities.Count; i < count; i++)
		{
			ChildEntities[i].CloneRelationsTo(doc, nextOwner);
		}
		if (doc.ImportStyles || doc.UpdateAlternateChunk)
		{
			CloneStyleTo(doc);
		}
	}

	private void CloneStyleTo(WordDocument doc)
	{
		if (m_style != null)
		{
			IStyle style = (m_style as Style).ImportStyleTo(doc, isParagraphStyle: false);
			if (style is WTableStyle)
			{
				ApplyStyle(style as WTableStyle, isClearCellShading: false);
			}
		}
	}

	private void CheckTableGrid()
	{
		if (m_tableGrid != null && (PreferredTableWidth.WidthType >= FtsWidth.Percentage || IsTableCellWidthDefined))
		{
			float ownerWidth = GetOwnerWidth();
			float num = (float)Math.Round(GetTableClientWidth(ownerWidth) * 20f);
			float num2 = 0f;
			if (m_tableGrid.Count != 0)
			{
				num2 = m_tableGrid[m_tableGrid.Count - 1].EndOffset;
			}
			if (num != num2)
			{
				UpdateTableGrid(isTableGridMissMatch: false, isgridafter: true);
				IsTableGridVerified = true;
			}
			else if (IsCorruptGridUpdateBasedOnCellPrefWidth())
			{
				UpdateGridAndCellWidthBasedOnCellPrefWidth();
				IsTableGridCorrupted = false;
			}
		}
		else if (IsTableGridCorrupted)
		{
			UpdateTableGrid(isTableGridMissMatch: false, isgridafter: true);
			IsTableGridCorrupted = false;
		}
	}

	private bool IsCellWidthZero()
	{
		foreach (WTableRow row in Rows)
		{
			if (!row.IsCellWidthZero())
			{
				return false;
			}
		}
		return true;
	}

	internal void UpdateCellWidthByPartitioning(float tableWidth, ref bool isTableGridMissMatch)
	{
		IsUpdateCellWidthByPartitioning = false;
		bool flag = false;
		foreach (WTableRow row in Rows)
		{
			flag = (flag ? flag : IsSkipToUpdateByEqualPartition(row));
			row.UpdateCellWidthByPartitioning(tableWidth, flag);
		}
		if (m_doc.ActualFormatType != FormatType.Docx || !IsInCell || PreferredTableWidth.WidthType != 0)
		{
			return;
		}
		foreach (WTableRow row2 in Rows)
		{
			foreach (WTableCell cell in row2.Cells)
			{
				if (cell.PreferredWidth.WidthType != FtsWidth.Percentage)
				{
					isTableGridMissMatch = false;
					break;
				}
			}
			if (row2.Cells.Count > m_tableGrid.Count)
			{
				isTableGridMissMatch = true;
				break;
			}
		}
	}

	private bool IsSkipToUpdateByEqualPartition(WTableRow row)
	{
		foreach (WTableCell cell in row.Cells)
		{
			if (cell.PreferredWidth.WidthType == FtsWidth.Percentage || cell.PreferredWidth.WidthType == FtsWidth.Point)
			{
				return true;
			}
		}
		return false;
	}

	internal void UpdateUnDefinedCellWidth()
	{
		float ownerWidth = GetOwnerWidth();
		float width = Width;
		foreach (WTableRow row in Rows)
		{
			row.UpdateUnDefinedCellWidth((ownerWidth < width) ? ownerWidth : width);
		}
	}

	internal void UpdateTableGrid(bool isTableGridMissMatch, bool isgridafter)
	{
		m_tableGrid = new WTableColumnCollection(base.Document);
		float ownerWidth = GetOwnerWidth();
		float tableClientWidth = GetTableClientWidth(ownerWidth);
		float maxRowWidth = GetMaxRowWidth(ownerWidth);
		_ = TableFormat.IsAutoResized;
		_ = TableFormat.PreferredWidth.WidthType;
		bool isSkiptoCalculateCellWidth = false;
		List<float> maxCellPrefWidth = null;
		float totalMaxCellPrefWidth = 0f;
		if (IsUpdateCellWidthByPartitioning || (!CheckCellWidth() && !base.Document.IsOpening))
		{
			isTableGridMissMatch = false;
			UpdateCellWidthByPartitioning(tableClientWidth, ref isTableGridMissMatch);
		}
		if ((PreferredTableWidth.WidthType >= FtsWidth.Percentage && PreferredTableWidth.Width > 0f) || IsTableCellWidthDefined)
		{
			isSkiptoCalculateCellWidth = IsNeedtoResizeCell(tableClientWidth);
		}
		if (maxPrefWidthRowIndex == -1 && m_doc.IsDOCX() && HasPointPreferredCellWidth && !HasPercentPreferredCellWidth && !HasAutoPreferredCellWidth && !HasNonePreferredCellWidth && !TableFormat.IsAutoResized && !HasMergeCellsInTable(isNeedToConsidervMerge: false) && IsAllRowsHaveSameCellCount && !IsAllRowCellHasSamePrefWidth())
		{
			maxPrefWidthRowIndex = GetMaxPreferredWidthRowIndex(tableClientWidth);
		}
		if (IsCorruptGridUpdateBasedOnCellPrefWidth())
		{
			UpdateGridAndCellWidthBasedOnCellPrefWidth();
			IsTableGridCorrupted = false;
		}
		else if (IsGridUpdateBasedOnMaxPrefWidth(tableClientWidth, ref totalMaxCellPrefWidth, ref maxCellPrefWidth) || IsGridUpdateForAutoFitTblBasedOnMaxPrefWidth(ref totalMaxCellPrefWidth, ref maxCellPrefWidth))
		{
			UpdateGridAndCellWidthBasedOnMaxPrefWidth(tableClientWidth, ref totalMaxCellPrefWidth, ref maxCellPrefWidth);
		}
		else if (IsGridUpdateBasedOnMaxPrefWidthPercent(TableFormat.PreferredWidth.Width, isTableGridMissMatch, ref totalMaxCellPrefWidth, ref maxCellPrefWidth))
		{
			UpdateGridAndCellWidthBasedOnMaxPrefWidthPercent(tableClientWidth, ref totalMaxCellPrefWidth, ref maxCellPrefWidth);
		}
		else
		{
			bool isRowHasDefinedCells = IsTablesAnyOneOfRowsCellWidthsDefined(this);
			foreach (WTableRow row in Rows)
			{
				float num = 0f;
				if ((PreferredTableWidth.WidthType >= FtsWidth.Percentage && PreferredTableWidth.Width > 0f) || IsTableCellWidthDefined)
				{
					UpdateCellWidth(row, ownerWidth, tableClientWidth, maxRowWidth, isSkiptoCalculateCellWidth, isgridafter);
				}
				if (row.RowFormat.BeforeWidth > 0f)
				{
					num += GetGridBeforeAfter(row, ownerWidth, isAfterWidth: false, tableClientWidth, num, maxRowWidth, isTableGridMissMatch);
					UpdateTableGrid(num);
				}
				int num2 = 0;
				for (int i = 0; i < row.Cells.Count; i++)
				{
					WTableCell wTableCell = row.Cells[i];
					num += GetCellWidth(wTableCell, ownerWidth, tableClientWidth, num, maxRowWidth, isTableGridMissMatch, isRowHasDefinedCells, num2);
					num2 += wTableCell.GridSpan;
					UpdateTableGrid(num);
				}
				if (row.RowFormat.AfterWidth > 0f)
				{
					num += GetGridBeforeAfter(row, ownerWidth, isAfterWidth: true, tableClientWidth, num, maxRowWidth, isTableGridMissMatch);
					UpdateTableGrid(num);
				}
			}
		}
		if (isTableGridMissMatch)
		{
			m_tableGrid.ValidateColumnWidths();
		}
	}

	private bool IsGridUpdateBasedOnMaxPrefWidthPercent(float tableWidth, bool isTableGridMissMatch, ref float totalMaxCellPrefWidth, ref List<float> maxCellPrefWidth)
	{
		if (base.Document.IsDOCX())
		{
			bool num;
			if (!isTableGridMissMatch)
			{
				if (TableFormat.IsAutoResized)
				{
					goto IL_0187;
				}
				num = !HasNonePreferredCellWidth;
			}
			else
			{
				num = TableFormat.IsAutoResized;
			}
			if (num && !IsInCell && HasOnlyParagraphs && PreferredTableWidth.WidthType == FtsWidth.Percentage && HasPercentPreferredCellWidth && !HasPointPreferredCellWidth && !HasAutoPreferredCellWidth && !IsTableRowHasBeforeAfterWidth() && !IsFromHTML && IsUpdateMaxPrefCellWidthOfColumns(ref maxCellPrefWidth))
			{
				foreach (float item in maxCellPrefWidth)
				{
					totalMaxCellPrefWidth += item;
				}
				if (isTableGridMissMatch && maxCellPrefWidth.Contains(0f) && totalMaxCellPrefWidth < tableWidth)
				{
					List<float> list = new List<float>();
					for (int i = 0; i < maxCellPrefWidth.Count; i++)
					{
						if (maxCellPrefWidth[i] == 0f)
						{
							list.Add(i);
						}
					}
					float value = (tableWidth - totalMaxCellPrefWidth) / (float)list.Count;
					foreach (float item2 in list)
					{
						int index = (int)item2;
						maxCellPrefWidth[index] = value;
						totalMaxCellPrefWidth += maxCellPrefWidth[index];
					}
				}
				return true;
			}
		}
		goto IL_0187;
		IL_0187:
		if (maxCellPrefWidth != null)
		{
			maxCellPrefWidth = new List<float>();
			maxCellPrefWidth = null;
			totalMaxCellPrefWidth = 0f;
		}
		return false;
	}

	private bool IsGridUpdateForAutoFitTblBasedOnMaxPrefWidth(ref float totalMaxCellPrefWidth, ref List<float> maxCellPrefWidth)
	{
		bool flag = false;
		if (TableFormat.IsAutoResized && base.Document.IsDOCX() && !IsInCell && HasOnlyParagraphs && PreferredTableWidth.Width == 0f && HasPercentPreferredCellWidth && HasNonePreferredCellWidth && !HasAutoPreferredCellWidth && !HasPointPreferredCellWidth && !IsTableRowHasBeforeAfterWidth() && IsUpdateMaxPrefCellWidthOfColumns(ref maxCellPrefWidth))
		{
			flag = true;
			foreach (float item in maxCellPrefWidth)
			{
				if (item == 0f)
				{
					flag = false;
					break;
				}
				totalMaxCellPrefWidth += item;
			}
		}
		if (!flag && maxCellPrefWidth != null)
		{
			maxCellPrefWidth = new List<float>();
			maxCellPrefWidth = null;
			totalMaxCellPrefWidth = 0f;
		}
		return flag;
	}

	private bool IsUpdateMaxPrefCellWidthOfColumns(ref List<float> maxCellPrefWidth)
	{
		float totalCellPreferredWidth = 0f;
		int num = MaxCellCountWithoutSpannedCells(this);
		if (num != 0)
		{
			maxCellPrefWidth = GetMaxPrefCellWidthOfColumns(ref totalCellPreferredWidth, num);
			if (maxCellPrefWidth.Count == num)
			{
				UpdateMaxPrefWidthBasedOnSpannedCells(ref maxCellPrefWidth);
				return true;
			}
		}
		return false;
	}

	private void UpdateGridAndCellWidthBasedOnMaxPrefWidthPercent(float tableWidth, ref float totalMaxCellPrefWidth, ref List<float> maxCellPrefWidth)
	{
		UpdateMaxPrefWidthBasedOnTableWidthPercent(tableWidth, ref totalMaxCellPrefWidth, ref maxCellPrefWidth);
		UpdateCellWidthAndTableGrid(maxCellPrefWidth);
	}

	private List<float> UpdateMaxPrefWidthBasedOnTableWidthPercent(float tableWidth, ref float totalMaxCellPrefWidth, ref List<float> maxCellPrefWidth)
	{
		float num = tableWidth * 20f / (tableWidth * totalMaxCellPrefWidth / 5f);
		for (int i = 0; i < maxCellPrefWidth.Count; i++)
		{
			maxCellPrefWidth[i] = tableWidth * maxCellPrefWidth[i] / 100f * num;
		}
		return maxCellPrefWidth;
	}

	private void UpdateCellWidthAndTableGrid(List<float> maxCellPrefWidth)
	{
		foreach (WTableRow row in Rows)
		{
			float num = 0f;
			for (int i = 0; i < row.Cells.Count; i++)
			{
				WTableCell wTableCell = row.Cells[i];
				float cellWidthFromMaxCellPrefWidth = GetCellWidthFromMaxCellPrefWidth(wTableCell.GridColumnStartIndex, wTableCell.GridSpan, maxCellPrefWidth);
				wTableCell.CellFormat.CellWidth = cellWidthFromMaxCellPrefWidth;
				num += cellWidthFromMaxCellPrefWidth * 20f;
				UpdateTableGrid(num);
			}
		}
	}

	private bool IsGridUpdateBasedOnMaxPrefWidth(float tableWidth, ref float totalMaxCellPrefWidth, ref List<float> maxCellPrefWidth)
	{
		if (!TableFormat.IsAutoResized && !base.Document.IsOpening && !base.Document.IsCloning && base.Document.IsDOCX() && base.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && !IsInCell && HasOnlyParagraphs && PreferredTableWidth.Width > 0f && PreferredTableWidth.WidthType == FtsWidth.Percentage && HasPointPreferredCellWidth && !HasPercentPreferredCellWidth && !HasNonePreferredCellWidth && !HasAutoPreferredCellWidth && !IsTableRowHasBeforeAfterWidth() && IsUpdateMaxPrefCellWidthOfColumns(ref maxCellPrefWidth))
		{
			foreach (float item in maxCellPrefWidth)
			{
				totalMaxCellPrefWidth += item;
			}
			return totalMaxCellPrefWidth > tableWidth;
		}
		if (maxCellPrefWidth != null)
		{
			maxCellPrefWidth = new List<float>();
			maxCellPrefWidth = null;
			totalMaxCellPrefWidth = 0f;
		}
		return false;
	}

	private void UpdateMaxPrefWidthBasedOnTableWidth(float tableWidth, ref float totalMaxCellPrefWidth, ref List<float> maxCellPrefWidth)
	{
		if (TableFormat.LeftIndent > 0f)
		{
			tableWidth -= TableFormat.LeftIndent;
		}
		else if (TableFormat.LeftIndent < 0f)
		{
			tableWidth += TableFormat.LeftIndent;
		}
		float num = tableWidth / totalMaxCellPrefWidth;
		for (int i = 0; i < maxCellPrefWidth.Count; i++)
		{
			maxCellPrefWidth[i] = num * maxCellPrefWidth[i];
		}
	}

	private float GetCellWidthFromMaxCellPrefWidth(int columnIndex, int columnSpan, List<float> maxCellPrefWidth)
	{
		float num = 0f;
		for (int i = 0; i < columnSpan; i++)
		{
			num += maxCellPrefWidth[i + columnIndex];
		}
		return num;
	}

	private void UpdateMaxPrefWidthBasedOnSpannedCells(ref List<float> maxCellPrefWidth)
	{
		List<WTableCell> list = new List<WTableCell>();
		foreach (WTableRow row in Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				if (cell.GridSpan <= 1)
				{
					continue;
				}
				int num = list.FindIndex((WTableCell existingCell) => existingCell.Index == cell.Index && existingCell.GridSpan == cell.GridSpan);
				if (num != -1)
				{
					if (list[num].PreferredWidth.Width < cell.PreferredWidth.Width)
					{
						list[num] = cell;
					}
				}
				else
				{
					list.Add(cell);
				}
			}
		}
		list = (from cell in list
			orderby cell.GridColumnStartIndex + cell.GridSpan - 1, cell.GridSpan
			select cell).ToList();
		foreach (WTableCell item in list)
		{
			int num2 = item.GridColumnStartIndex + item.GridSpan - 1;
			float num3 = 0f;
			for (int i = item.GridColumnStartIndex; i <= num2; i++)
			{
				num3 += maxCellPrefWidth[i];
			}
			if (item.PreferredWidth.Width > num3)
			{
				float num4 = num3 - maxCellPrefWidth[num2];
				float value = item.PreferredWidth.Width - num4;
				maxCellPrefWidth[num2] = value;
			}
		}
	}

	private void UpdateGridAndCellWidthBasedOnMaxPrefWidth(float tableWidth, ref float totalMaxCellPrefWidth, ref List<float> maxCellPrefWidth)
	{
		UpdateMaxPrefWidthBasedOnTableWidth(tableWidth, ref totalMaxCellPrefWidth, ref maxCellPrefWidth);
		UpdateCellWidthAndTableGrid(maxCellPrefWidth);
	}

	private bool IsCorruptGridUpdateBasedOnCellPrefWidth()
	{
		if (!m_doc.IsOpening && m_doc.IsDOCX() && IsTableGridCorrupted && !IsInCell && HasOnlyParagraphs && PreferredTableWidth.WidthType == FtsWidth.Point && HasPointPreferredCellWidth && !HasPercentPreferredCellWidth && !HasNonePreferredCellWidth && !HasAutoPreferredCellWidth && !IsTableRowHasBeforeAfterWidth())
		{
			float totalCellPreferredWidth = 0f;
			GetMaxPrefCellWidthOfColumns(ref totalCellPreferredWidth, MaximumCellCount());
			return totalCellPreferredWidth > PreferredTableWidth.Width;
		}
		return false;
	}

	private bool IsTableRowHasBeforeAfterWidth()
	{
		foreach (WTableRow row in Rows)
		{
			if (row.RowFormat.GridBeforeWidth.Width > 0f || row.RowFormat.GridAfterWidth.Width > 0f)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateGridAndCellWidthBasedOnCellPrefWidth()
	{
		float totalCellPreferredWidth = 0f;
		List<float> maxPrefCellWidthOfColumns = GetMaxPrefCellWidthOfColumns(ref totalCellPreferredWidth, MaximumCellCount());
		foreach (WTableRow row in Rows)
		{
			float num = 0f;
			for (int i = 0; i < row.Cells.Count; i++)
			{
				row.Cells[i].CellFormat.CellWidth = maxPrefCellWidthOfColumns[i];
				num += maxPrefCellWidthOfColumns[i] * 20f;
				UpdateTableGrid(num);
			}
		}
	}

	internal bool CheckCellWidth()
	{
		bool result = true;
		foreach (WTableRow row in Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				if (cell.CellFormat.CellWidth == 0f)
				{
					result = false;
					continue;
				}
				return true;
			}
		}
		return result;
	}

	private bool IsNeedToMergeTwoTables()
	{
		if (m_doc.IsDOCX() && base.NextSibling is WTable && !(base.NextSibling.NextSibling is WTable))
		{
			WTable wTable = base.NextSibling as WTable;
			int num = wTable.MaximumCellCount();
			if (!IsInCell && TableFormat.IsAutoResized && TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && TableFormat.PreferredWidth.Width == 0f && !HasAutoPreferredCellWidth && !HasPercentPreferredCellWidth && HasPointPreferredCellWidth && !HasNonePreferredCellWidth && MaximumCellCount() == TableGrid.Count && !HasMergeCellsInTable(isNeedToConsidervMerge: true) && !wTable.IsInCell && wTable.TableFormat.IsAutoResized && wTable.TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && wTable.TableFormat.PreferredWidth.Width == 0f && !wTable.HasAutoPreferredCellWidth && !wTable.HasPercentPreferredCellWidth && !wTable.HasPointPreferredCellWidth && wTable.HasNonePreferredCellWidth && num == wTable.TableGrid.Count && !wTable.HasMergeCellsInTable(isNeedToConsidervMerge: true) && num == 2)
			{
				if (!wTable.TableFormat.WrapTextAround && !TableFormat.WrapTextAround && !wTable.IsFrame && !IsFrame)
				{
					return wTable.StyleName == StyleName;
				}
				return false;
			}
		}
		return false;
	}

	internal void AutoFitColumns(bool forceAutoFitToContent)
	{
		if (IsNeedToMergeTwoTables())
		{
			MergeTables(base.NextSibling as WTable);
		}
		bool flag = IsTableGridCorrupted && m_doc.IsDOCX();
		if (forceAutoFitToContent && m_doc.IsDOCX() && !IsInCell && !HasOnlyParagraphs)
		{
			HasOnlyParagraphs = true;
			foreach (WTableRow row in Rows)
			{
				foreach (WTableCell cell in row.Cells)
				{
					if ((cell.Tables != null && cell.Tables.Count > 0) || cell.ContentControl != null)
					{
						HasOnlyParagraphs = false;
						break;
					}
				}
			}
		}
		bool needtoCalculateParaWidth = m_doc.IsDOCX() && !IsInCell && TableFormat.IsAutoResized && HasOnlyParagraphs && ((flag && IsTableBasedOnContent(this)) || TableFormat.PreferredWidth.WidthType == FtsWidth.Percentage || TableFormat.PreferredWidth.WidthType == FtsWidth.None || (TableFormat.PreferredWidth.WidthType == FtsWidth.Auto && TableFormat.PreferredWidth.Width == 0f) || forceAutoFitToContent);
		if (TableGrid.Count == 0)
		{
			UpdateTableGrid(isTableGridMissMatch: false, isgridafter: true);
		}
		UpdateGridSpan();
		List<WTableCell> list = new List<WTableCell>();
		bool isAutoResized = TableFormat.IsAutoResized;
		float ownerWidth = GetOwnerWidth();
		float tableClientWidth = GetTableClientWidth(ownerWidth);
		bool isAutoWidth = IsAutoWidth(ownerWidth, tableClientWidth);
		Dictionary<int, float> preferredWidths = new Dictionary<int, float>();
		bool flag2 = true;
		flag2 = !IsInCell && base.Document != null && base.Document.ActualFormatType == FormatType.Docx && !TableFormat.IsAutoResized && PreferredTableWidth.WidthType == FtsWidth.Point && PreferredTableWidth.Width > 0f;
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < Rows.Count; i++)
		{
			num = 0f;
			WTableRow wTableRow = Rows[i];
			ColumnSizeInfo columnSizeInfo = new ColumnSizeInfo();
			short num3 = 0;
			float num4 = 0f;
			float num5 = 0f;
			if (wTableRow.RowFormat.GridBefore > 0)
			{
				num4 = (columnSizeInfo.MinimumWidth = GetCellWidth(wTableRow.RowFormat.GridBeforeWidth.Width, PreferredTableWidth.WidthType, tableClientWidth, null));
				TableGrid.UpdateColumns(num3, num3 = wTableRow.RowFormat.GridBefore, num5 += num4, columnSizeInfo, preferredWidths, isGridAfterColumn: false);
			}
			for (int j = 0; j < wTableRow.Cells.Count; j++)
			{
				WTableCell wTableCell2 = wTableRow.Cells[j];
				flag2 = ((!flag2) ? flag2 : (wTableCell2.CellFormat.VerticalMerge == CellMerge.None && wTableCell2.PreferredWidth.Width > 0f && wTableCell2.PreferredWidth.WidthType == FtsWidth.Point));
				int num6 = 1;
				for (int k = 0; k < list.Count; k++)
				{
					WTableCell wTableCell3 = list[k];
					if (wTableCell3.GridColumnStartIndex >= num3)
					{
						bool flag3 = true;
						num6 = GetRowSpan(wTableCell3);
						if (wTableCell3.GridColumnStartIndex > num3)
						{
							flag3 = false;
						}
						else
						{
							num4 = GetCellWidth(wTableCell3.CellFormat.PreferredWidth.Width, wTableCell3.CellFormat.PreferredWidth.WidthType, tableClientWidth, wTableCell3);
							columnSizeInfo = wTableCell3.GetSizeInfo(isAutoResized, isAutoWidth, needtoCalculateParaWidth);
							short num7 = 1;
							wTableCell3.GridSpan = ((wTableCell3.GridSpan == 0) ? num7 : wTableCell3.GridSpan);
							TableGrid.UpdateColumns(num3, num3 + wTableCell3.GridSpan, num5 += num4, columnSizeInfo, preferredWidths, isGridAfterColumn: false);
						}
						if (!flag3 && j == wTableRow.Cells.Count - 1)
						{
							flag3 = true;
						}
						if (flag3 && i - wTableCell3.OwnerRow.Index == num6 - 1)
						{
							list.RemoveAt(k);
							k--;
						}
					}
				}
				num6 = GetRowSpan(wTableCell2);
				if (num6 > 1)
				{
					if (list.Count == 0 || list[list.Count - 1].GridColumnStartIndex <= num3)
					{
						list.Add(wTableCell2);
					}
					else
					{
						int index = 0;
						for (int num8 = list.Count; num8 > 0; num8--)
						{
							if (list[num8 - 1].GridColumnStartIndex > num3)
							{
								index = num8 - 1;
							}
						}
						list.Insert(index, wTableCell2);
					}
				}
				columnSizeInfo = wTableCell2.GetSizeInfo(isAutoResized, isAutoWidth, needtoCalculateParaWidth);
				num4 = GetCellWidth(wTableCell2.CellFormat.PreferredWidth.Width, wTableCell2.CellFormat.PreferredWidth.WidthType, tableClientWidth, wTableCell2);
				num += num4;
				TableGrid.UpdateColumns(num3, num3 += wTableCell2.GridSpan, num5 += num4, columnSizeInfo, preferredWidths, isGridAfterColumn: false);
			}
			if (wTableRow.RowFormat.GridAfter > 0)
			{
				num4 = (columnSizeInfo.MinimumWidth = GetCellWidth(wTableRow.RowFormat.GridAfterWidth.Width, wTableRow.RowFormat.GridAfterWidth.WidthType, tableClientWidth, null));
				TableGrid.UpdateColumns(num3, num3 += wTableRow.RowFormat.GridAfter, num5 += num4, columnSizeInfo, preferredWidths, isGridAfterColumn: true);
			}
			flag2 = ((!flag2) ? flag2 : (num <= PreferredTableWidth.Width));
			num2 = ((num2 < num) ? num : num2);
		}
		UsePreferredCellWidth = flag2;
		TableGrid.UpdatePreferredWidhToColumns(preferredWidths);
		bool flag4 = CheckNeedToAutoFit();
		if (forceAutoFitToContent || (flag4 && isAutoResized && TableFormat.PreferredWidth.Width == 0f))
		{
			TableGrid.AutoFitColumns(ownerWidth, tableClientWidth, isAutoWidth, forceAutoFitToContent, this);
		}
		else
		{
			bool flag5 = CheckIsNeedToSkipGridValue(ownerWidth, isAutoWidth, num2);
			if (flag5 && IsAutoTableSkipTableGrid())
			{
				ResizeAutoTableColumnWidth();
			}
			if (!UsePreferredCellWidth && !flag5)
			{
				TableGrid.ValidateColumnWidths();
			}
			TableGrid.FitColumns(ownerWidth, tableClientWidth, isAutoWidth, this, flag);
		}
		SetNewWidthToCells(flag);
	}

	internal void DocAutoFitColumns()
	{
		float ownerWidth = GetOwnerWidth();
		float tableClientWidth = GetTableClientWidth(ownerWidth);
		float rowWidth = Rows[0].GetRowWidth();
		if (!TableFormat.IsAutoResized || PreferredTableWidth.WidthType != FtsWidth.Auto || IsInCell || HasMergeCellsInTable(isNeedToConsidervMerge: false) || !HasPointPreferredCellWidth || HasPercentPreferredCellWidth || HasAutoPreferredCellWidth || HasNonePreferredCellWidth || !(rowWidth > tableClientWidth) || !(rowWidth > GetOwnerSection().PageSetup.PageSize.Width) || !IsAllRowsHaveSameWidth(rowWidth) || TableFormat.LeftIndent != 0f)
		{
			return;
		}
		float num = tableClientWidth / rowWidth;
		foreach (WTableRow row in Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				cell.Width *= num;
			}
		}
	}

	private bool IsAllRowsHaveSameWidth(float rowWidth)
	{
		for (int i = 1; i < Rows.Count; i++)
		{
			if (rowWidth != Rows[i].GetRowWidth())
			{
				return false;
			}
		}
		return true;
	}

	private bool CheckNeedToAutoFit()
	{
		int num = 0;
		foreach (WTableColumn item in TableGrid)
		{
			if (item.PreferredWidth == 0f)
			{
				num++;
			}
		}
		return num == TableGrid.Count;
	}

	private bool CheckIsNeedToSkipGridValue(float containerWidth, bool isAutoWidth, float currentRowWidthBasedOnCells)
	{
		if (!IsInCell && (base.Document.ActualFormatType == FormatType.Word2013 || base.Document.ActualFormatType == FormatType.Docx) && isAutoWidth && IsAllCellsHavePointWidth && PreferredTableWidth.Width == 0f && Math.Round(currentRowWidthBasedOnCells, 2) <= Math.Round(containerWidth, 2) && Math.Round(TableFormat.LeftIndent + currentRowWidthBasedOnCells, 2) <= Math.Round(containerWidth, 2) && TableFormat.CellSpacing <= 0f)
		{
			int num = 0;
			bool flag = HasMergeCellsInTable(isNeedToConsidervMerge: true);
			for (int i = 0; i < TableGrid.InnerList.Count; i++)
			{
				WTableColumn wTableColumn = TableGrid.InnerList[i] as WTableColumn;
				if (wTableColumn.PreferredWidth != 0f && wTableColumn.MaximumWordWidth <= wTableColumn.PreferredWidth)
				{
					num++;
				}
			}
			if (num == TableGrid.InnerList.Count && !flag)
			{
				return true;
			}
		}
		else if (IsAutoTableSkipTableGrid())
		{
			return true;
		}
		return false;
	}

	internal bool HasMergeCellsInTable(bool isNeedToConsidervMerge)
	{
		for (int i = 0; i < Rows.Count; i++)
		{
			WTableRow wTableRow = Rows[i];
			for (int j = 0; j < wTableRow.Cells.Count; j++)
			{
				WTableCell wTableCell = wTableRow.Cells[j];
				if (wTableCell.GridSpan > 1 || wTableCell.CellFormat.HorizontalMerge != 0 || (wTableCell.CellFormat.VerticalMerge != CellMerge.None && isNeedToConsidervMerge))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsAutoTableSkipTableGrid()
	{
		if (base.Document.ActualFormatType == FormatType.Word2010 && IsAutoTableExceedsClientWidth() && IsAllCellsHavePointWidth && TableFormat.CellSpacing <= 0f && !HasMergeCellsInTable(isNeedToConsidervMerge: true))
		{
			return MaximumCellCount() == TableGrid.Count;
		}
		return false;
	}

	private void ResizeAutoTableColumnWidth()
	{
		float num = 0f;
		for (int i = 0; i < TableGrid.InnerList.Count; i++)
		{
			WTableColumn wTableColumn = TableGrid.InnerList[i] as WTableColumn;
			num += ((wTableColumn.PreferredWidth > wTableColumn.MaximumWordWidth) ? wTableColumn.PreferredWidth : wTableColumn.MaximumWordWidth);
		}
		if (!(Math.Round(num, 2) > Math.Round(PreferredTableWidth.Width, 2)))
		{
			return;
		}
		for (int j = 0; j < TableGrid.InnerList.Count; j++)
		{
			int num2 = -1;
			WTableColumn wTableColumn2 = TableGrid.InnerList[j] as WTableColumn;
			if (wTableColumn2.PreferredWidth < wTableColumn2.MaximumWordWidth)
			{
				wTableColumn2.PreferredWidth = wTableColumn2.MaximumWordWidth;
				num2 = j;
			}
			float num3 = PreferredTableWidth.Width / num;
			if (num2 != j && wTableColumn2.PreferredWidth > wTableColumn2.MaximumWordWidth)
			{
				wTableColumn2.PreferredWidth = (float)Math.Round(num3, 2) * wTableColumn2.PreferredWidth;
			}
		}
	}

	internal void UpdateGridSpan()
	{
		if (TableGrid.Count <= 0)
		{
			return;
		}
		foreach (WTableRow childEntity in ChildEntities)
		{
			foreach (WTableCell childEntity2 in childEntity.ChildEntities)
			{
				childEntity2.GridSpan = childEntity.RowFormat.GetGridCount(childEntity2.Index);
			}
		}
	}

	internal void UpdateGridSpan(WTable table)
	{
		if (table.m_tableGrid == null || table.m_tableGrid.Count <= 0)
		{
			return;
		}
		foreach (WTableRow childEntity in ChildEntities)
		{
			foreach (WTableCell childEntity2 in childEntity.ChildEntities)
			{
				childEntity2.GridSpan = childEntity.RowFormat.GetGridCount(childEntity2.Index);
			}
		}
	}

	private bool IsAutoWidth(float parentCellWidth, float tableWidth)
	{
		bool result = TableFormat.PreferredWidth.WidthType == FtsWidth.Auto || TableFormat.PreferredWidth.WidthType == FtsWidth.None;
		if (IsInCell && (parentCellWidth < tableWidth || TableFormat.PreferredWidth.WidthType == FtsWidth.Percentage))
		{
			result = false;
		}
		return result;
	}

	internal int GetRowSpan(WTableCell wCell)
	{
		int num = 1;
		if (wCell.OwnerRow.NextSibling != null && wCell.CellFormat.VerticalMerge == CellMerge.Start)
		{
			WTableRow ownerRow = wCell.OwnerRow;
			int num2 = ownerRow.RowFormat.GridBefore;
			if (num2 < 0)
			{
				num2 = 0;
			}
			foreach (WTableCell cell in ownerRow.Cells)
			{
				if (cell == wCell)
				{
					break;
				}
				num2 += cell.GridSpan;
			}
			for (int i = ownerRow.GetRowIndex() + 1; i < Rows.Count; i++)
			{
				WTableRow wTableRow = Rows[i];
				int num3 = wTableRow.RowFormat.GridBefore;
				if (num3 < 0)
				{
					num3 = 0;
				}
				WTableCell wTableCell2 = null;
				foreach (WTableCell cell2 in wTableRow.Cells)
				{
					if (num2 == num3)
					{
						wTableCell2 = cell2;
						break;
					}
					num3 += cell2.GridSpan;
				}
				if (wTableCell2 == null || wTableCell2.CellFormat.VerticalMerge != CellMerge.Continue)
				{
					break;
				}
				num++;
			}
		}
		return num;
	}

	internal float GetCellWidth(float preferredWidth, FtsWidth preferredWidthType, float containerWidth, WTableCell cell)
	{
		float result = preferredWidth;
		switch (preferredWidthType)
		{
		case FtsWidth.Percentage:
			result = preferredWidth * containerWidth / 100f;
			break;
		case FtsWidth.Point:
			result = preferredWidth;
			break;
		default:
			if (cell != null)
			{
				result = GetMinimumPreferredWidth(cell);
			}
			break;
		}
		return result;
	}

	internal float GetMinimumPreferredWidth(WTableCell cell)
	{
		float num = 0f;
		if (cell.CellFormat.PreferredWidth.Width == 0f && cell.CellFormat.CellWidth != 0f)
		{
			return cell.CellFormat.CellWidth;
		}
		return cell.GetMinimumPreferredWidth();
	}

	private void SetNewWidthToCells(bool isTableGridCorrupts)
	{
		WTableColumnCollection tableGrid = TableGrid;
		if (isTableGridCorrupts && !IsInCell && IsTableBasedOnContent(this) && HasOnlyParagraphs)
		{
			SetParagraphWidthToCells();
			UpdateRowBeforeAfter(this);
			tableGrid.UpdateEndOffset();
			m_tableWidth = UpdateWidth();
			return;
		}
		float totalWidth = tableGrid.GetTotalWidth(0);
		float ownerWidth = GetOwnerWidth();
		WSection ownerSection = GetOwnerSection();
		float num = 0f;
		bool flag = false;
		foreach (WTableRow row in Rows)
		{
			float rowPreferredWidth = row.GetRowPreferredWidth(m_tableWidth);
			if (rowPreferredWidth > num)
			{
				num = rowPreferredWidth;
			}
		}
		if (m_doc.IsDOCX() && (Math.Round(totalWidth, 2) < Math.Round(ownerWidth, 2) || (Math.Round(totalWidth, 2) > Math.Round(ownerWidth, 2) && totalWidth < ownerSection.PageSetup.PageSize.Width)) && !IsInCell && TableFormat.IsAutoResized && PreferredTableWidth.WidthType == FtsWidth.Auto && Math.Round(num / 20f) <= Math.Round(totalWidth) && MaximumCellCount() == tableGrid.Count && TableFormat.Positioning.AllowOverlap)
		{
			float totalWidth2 = tableGrid.GetTotalWidth(2);
			float totalWidth3 = tableGrid.GetTotalWidth(3);
			bool flag2 = totalWidth3 <= ownerWidth;
			bool flag3 = !(totalWidth2 <= ownerWidth && flag2) || totalWidth2 > totalWidth3;
			if (isTableGridCorrupts && PreferredTableWidth.Width == 0f && IsAllCellsHaveAutoZeroWidth() && IsRecalulateBasedOnLastCol && IsColumnNotHaveEnoughWidth(ownerWidth, isConsiderPointsValue: true) && (flag2 || flag3))
			{
				flag = true;
				if (flag3)
				{
					for (int i = 0; i < tableGrid.Count; i++)
					{
						tableGrid[i].PreferredWidth = tableGrid[i].MaximumWordWidth;
					}
					float num2 = ownerWidth - totalWidth2;
					if (num2 > 0f)
					{
						float maxNestedTableWidthFromLastColumn = GetMaxNestedTableWidthFromLastColumn();
						WTableColumn wTableColumn = tableGrid[tableGrid.Count - 1];
						if (wTableColumn.PreferredWidth < maxNestedTableWidthFromLastColumn && maxNestedTableWidthFromLastColumn - wTableColumn.PreferredWidth <= num2)
						{
							wTableColumn.PreferredWidth = maxNestedTableWidthFromLastColumn;
						}
					}
				}
				else if (flag2)
				{
					for (int j = 0; j < tableGrid.Count; j++)
					{
						tableGrid[j].PreferredWidth = tableGrid[j].MaxParaWidth;
					}
					float num3 = ownerWidth - totalWidth3;
					if (num3 > 0f)
					{
						float maxNestedTableWidthFromLastColumn2 = GetMaxNestedTableWidthFromLastColumn();
						WTableColumn wTableColumn2 = tableGrid[tableGrid.Count - 1];
						if (wTableColumn2.PreferredWidth < maxNestedTableWidthFromLastColumn2 && maxNestedTableWidthFromLastColumn2 - wTableColumn2.PreferredWidth <= num3)
						{
							wTableColumn2.PreferredWidth = maxNestedTableWidthFromLastColumn2;
						}
					}
				}
			}
			else
			{
				float num4 = ((TableFormat.HorizontalAlignment != RowAlignment.Right) ? IndentFromLeft : 0f);
				float num5 = ((totalWidth < ownerWidth) ? (ownerWidth - (totalWidth + num4)) : (ownerSection.PageSetup.PageSize.Width - totalWidth));
				for (int k = 0; k < tableGrid.Count; k++)
				{
					float num6 = tableGrid[k].MaximumWordWidth - tableGrid[k].PreferredWidth;
					if (tableGrid[k].MaximumWordWidth > 0f && tableGrid[k].HasMaximumWordWidth && tableGrid[k].MaximumWordWidth > tableGrid[k].PreferredWidth && num6 > 0f && num5 - num6 >= 0f)
					{
						tableGrid[k].PreferredWidth = tableGrid[k].MaximumWordWidth;
						num5 -= num6;
					}
				}
			}
		}
		if (m_doc.IsDOCX() && IsTableGridCorrupted && !IsInCell && !TableFormat.IsAutoResized && IsAllCellsHaveAutoZeroWidth() && TableFormat.PreferredWidth.WidthType == FtsWidth.Percentage)
		{
			float preferredWidth = GetTableClientWidth(ownerWidth) / 100f * TableFormat.PreferredWidth.Width / (float)tableGrid.Count;
			for (int l = 0; l < tableGrid.Count; l++)
			{
				tableGrid[l].PreferredWidth = preferredWidth;
			}
		}
		foreach (WTableRow row2 in Rows)
		{
			short gridBefore = row2.RowFormat.GridBefore;
			short gridAfter = row2.RowFormat.GridAfter;
			if (gridBefore > 0)
			{
				row2.RowFormat.BeforeWidth = tableGrid.GetCellWidth(0, gridBefore);
			}
			for (int m = 0; m < row2.Cells.Count; m++)
			{
				WTableCell wTableCell = row2.Cells[m];
				float num7 = tableGrid.GetCellWidth(wTableCell.GridColumnStartIndex, wTableCell.GridSpan);
				if (wTableCell.GridSpan > 0 && wTableCell.IsFitAsPerMaximumWordWidth(num7, tableGrid[wTableCell.GridColumnStartIndex].MaximumWordWidth) && HasSpaceToConsiderMaxWordWidth(wTableCell.GridColumnStartIndex))
				{
					tableGrid[wTableCell.GridColumnStartIndex].PreferredWidth = tableGrid[wTableCell.GridColumnStartIndex].MaximumWordWidth;
					wTableCell.CellFormat.CellWidth = tableGrid[wTableCell.GridColumnStartIndex].MaximumWordWidth;
				}
				else
				{
					wTableCell.CellFormat.CellWidth = num7;
				}
			}
			if (gridAfter > 0)
			{
				row2.RowFormat.AfterWidth = tableGrid.GetCellWidth(row2.Cells.Count, gridAfter);
			}
		}
		tableGrid.UpdateEndOffset();
		if (flag || (isTableGridCorrupts && PreferredTableWidth.Width == 0f && IsAllCellsHaveAutoZeroWidth() && !IsInCell && TableFormat.IsAutoResized && PreferredTableWidth.WidthType == FtsWidth.Auto))
		{
			m_tableWidth = UpdateWidth();
		}
		if (IsInCell && isTableGridCorrupts)
		{
			SetNewWidthToNestedTableCells();
		}
	}

	private float GetMaxNestedTableWidthFromLastColumn()
	{
		float num = 0f;
		foreach (WTableRow row in Rows)
		{
			foreach (WTable table in (row.Cells.LastItem as WTableCell).Tables)
			{
				num = ((table.Width > num) ? table.Width : num);
			}
		}
		return num;
	}

	private void SetNewWidthToNestedTableCells()
	{
		WTable wTable = GetOwnerTable(base.Owner) as WTable;
		WTableColumnCollection tableGrid = TableGrid;
		if (IsTableBasedOnContent(wTable) && !wTable.IsInCell && IsTableBasedOnContent(this) && HasOnlyParagraphs && tableGrid.GetTotalWidth(2) < tableGrid.GetTotalWidth(0))
		{
			for (int i = 0; i < tableGrid.Count; i++)
			{
				tableGrid[i].PreferredWidth = tableGrid[i].MaximumWordWidth;
			}
			SetCellWidthAsColumnPreferredWidth(this, tableGrid);
			UpdateRowBeforeAfter(this);
			tableGrid.UpdateEndOffset();
		}
	}

	internal void SetCellWidthAsColumnPreferredWidth(WTable table, WTableColumnCollection columns)
	{
		for (int i = 0; i < table.Rows.Count; i++)
		{
			WTableRow wTableRow = Rows[i];
			for (int j = 0; j < Rows[i].Cells.Count; j++)
			{
				wTableRow.Cells[j].Width = columns[j].PreferredWidth;
			}
		}
	}

	internal void UpdateRowBeforeAfter(WTable table)
	{
		WTableColumnCollection tableGrid = table.TableGrid;
		foreach (WTableRow row in table.Rows)
		{
			short gridBefore = row.RowFormat.GridBefore;
			short gridAfter = row.RowFormat.GridAfter;
			if (gridBefore > 0)
			{
				row.RowFormat.BeforeWidth = tableGrid.GetCellWidth(0, gridBefore);
			}
			if (gridAfter > 0)
			{
				row.RowFormat.AfterWidth = tableGrid.GetCellWidth(row.Cells.Count, gridAfter);
			}
		}
	}

	internal bool IsColumnNotHaveEnoughWidth(float clientWidth, bool isConsiderPointsValue)
	{
		WTableColumnCollection tableGrid = TableGrid;
		if (tableGrid.GetTotalWidth(2) <= clientWidth)
		{
			foreach (WTableColumn item in tableGrid)
			{
				if (isConsiderPointsValue ? (item.PreferredWidth < item.MaximumWordWidth) : ((int)item.PreferredWidth < (int)item.MaximumWordWidth))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal bool IsColumnsNotHaveEnoughPreferredWidthForWordWidth(WTableColumnCollection columns, float tableWidth)
	{
		if (columns.GetTotalWidth(2) <= tableWidth)
		{
			foreach (WTableColumn column in columns)
			{
				if (column.PreferredWidth < column.MaximumWordWidth - column.MinimumWidth)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsTableBasedOnContent(WTable table)
	{
		WTableColumnCollection tableGrid = table.TableGrid;
		if (m_doc.IsDOCX() && table.TableFormat.IsAutoResized && table.PreferredTableWidth.WidthType == FtsWidth.Auto && table.PreferredTableWidth.Width == 0f && table.MaximumCellCount() == tableGrid.Count)
		{
			return table.IsAllCellsHaveAutoZeroWidth();
		}
		return false;
	}

	private void SetParagraphWidthToCells()
	{
		WTableColumnCollection tableGrid = TableGrid;
		float ownerWidth = GetOwnerWidth();
		float num = 0f;
		foreach (WTableColumn item in tableGrid)
		{
			num += item.MaxParaWidth;
		}
		if (num <= ownerWidth)
		{
			foreach (WTableColumn item2 in tableGrid)
			{
				item2.PreferredWidth = item2.MaxParaWidth;
			}
		}
		else
		{
			float num2 = 0f;
			foreach (WTableColumn item3 in tableGrid)
			{
				item3.PreferredWidth = item3.MaximumWordWidth;
				num2 += item3.PreferredWidth;
			}
			float num3 = num - num2;
			float num4 = ownerWidth - num2;
			foreach (WTableColumn item4 in tableGrid)
			{
				float num5 = (item4.MaxParaWidth - item4.PreferredWidth) / num3 * num4;
				item4.PreferredWidth += num5;
			}
		}
		for (int i = 0; i < Rows.Count; i++)
		{
			WTableRow wTableRow = Rows[i];
			for (int j = 0; j < wTableRow.Cells.Count; j++)
			{
				wTableRow.Cells[j].CellFormat.CellWidth = tableGrid[j].PreferredWidth;
			}
		}
	}

	private bool IsAllCellsHaveAutoZeroWidth()
	{
		foreach (WTableRow row in Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				if (cell.PreferredWidth.Width != 0f || cell.PreferredWidth.WidthType != FtsWidth.Auto)
				{
					return false;
				}
			}
		}
		return true;
	}

	internal int MaximumCellCount()
	{
		int num = 0;
		foreach (WTableRow row in Rows)
		{
			if (row.Cells.Count > num)
			{
				num = row.Cells.Count;
			}
		}
		return num;
	}

	private bool HasSpaceToConsiderMaxWordWidth(short gridColumnStartIndex)
	{
		WTableColumnCollection tableGrid = TableGrid;
		float totalWidth = tableGrid.GetTotalWidth(0);
		WSection ownerSection = GetOwnerSection();
		float num = 0f;
		num = ((!ownerSection.PageSetup.Borders.NoBorder) ? (ownerSection.PageSetup.PageSize.Width - (ownerSection.PageSetup.Borders.Left.Space + ownerSection.PageSetup.Borders.Right.Space) - totalWidth) : (ownerSection.PageSetup.PageSize.Width - totalWidth));
		float num2 = tableGrid[gridColumnStartIndex].MaximumWordWidth - tableGrid[gridColumnStartIndex].PreferredWidth;
		if (num2 > 0f)
		{
			return num - num2 > 0f;
		}
		return false;
	}

	private void UpdatePreferredWidthProperties(bool updateAllowAutoFit, AutoFitType autoFittype)
	{
		if (updateAllowAutoFit)
		{
			TableFormat.IsAutoResized = autoFittype != AutoFitType.FixedColumnWidth;
		}
		switch (autoFittype)
		{
		case AutoFitType.FixedColumnWidth:
			TableFormat.ClearPreferredWidthPropertyValue(12);
			TableFormat.ClearPreferredWidthPropertyValue(11);
			{
				foreach (WTableRow row in Rows)
				{
					foreach (WTableCell cell in row.Cells)
					{
						cell.CellFormat.PreferredWidth.WidthType = FtsWidth.Point;
						cell.CellFormat.PreferredWidth.Width = cell.CellFormat.CellWidth;
					}
				}
				break;
			}
		case AutoFitType.FitToWindow:
		{
			float totalWidth = TableGrid.GetTotalWidth(0);
			TableFormat.PreferredWidth.Width = 100f;
			TableFormat.PreferredWidth.WidthType = FtsWidth.Percentage;
			{
				foreach (WTableRow row2 in Rows)
				{
					foreach (WTableCell cell2 in row2.Cells)
					{
						if (cell2.CellFormat.PreferredWidth.WidthType != FtsWidth.Percentage)
						{
							cell2.CellFormat.PreferredWidth.WidthType = FtsWidth.Percentage;
							cell2.CellFormat.PreferredWidth.Width = cell2.CellFormat.CellWidth / totalWidth * 100f;
						}
					}
				}
				break;
			}
		}
		default:
			ClearPreferredWidths(beforeAutoFit: false);
			break;
		}
	}

	private void ClearPreferredWidths(bool beforeAutoFit)
	{
		TableFormat.ClearPreferredWidthPropertyValue(12);
		TableFormat.ClearPreferredWidthPropertyValue(11);
		foreach (WTableRow row in Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				cell.CellFormat.ClearPreferredWidthPropertyValue(14);
				cell.CellFormat.ClearPreferredWidthPropertyValue(13);
			}
		}
		if (!beforeAutoFit)
		{
			return;
		}
		foreach (WTableColumn item in TableGrid)
		{
			item.PreferredWidth = 0f;
		}
	}

	private bool IsTablesAnyOneOfRowsCellWidthsDefined(WTable table)
	{
		bool flag = false;
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				if (cell.PreferredWidth.Width != 0f || cell.CellFormat.CellWidth != 0f)
				{
					flag = true;
					continue;
				}
				flag = false;
				break;
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private float GetMaxRowWidth(float clientWidth)
	{
		float num = clientWidth;
		foreach (WTableRow row in Rows)
		{
			float widthToResizeCells = row.GetWidthToResizeCells(clientWidth);
			if (widthToResizeCells > num)
			{
				num = widthToResizeCells;
			}
		}
		return num;
	}

	private void UpdateCellWidth(WTableRow row, float clientWidth, float tableWidth, float maxRowWidth, bool isSkiptoCalculateCellWidth, bool isGridafter)
	{
		float num = 1f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		if (row.RowFormat.GridBeforeWidth.Width > 0f)
		{
			num3 = GetGridBeforeAfter(row.RowFormat.GridBeforeWidth, tableWidth);
		}
		if (row.RowFormat.GridAfterWidth.Width > 0f && isGridafter)
		{
			num4 = GetGridBeforeAfter(row.RowFormat.GridAfterWidth, tableWidth);
		}
		if (row.RowFormat.BeforeWidth > 0f)
		{
			num2 += row.RowFormat.BeforeWidth * 20f;
		}
		else if (num3 > 0f)
		{
			num2 += num3;
			row.RowFormat.BeforeWidth = num3 / 20f;
		}
		if (row.RowFormat.AfterWidth > 0f && isGridafter)
		{
			num2 += row.RowFormat.AfterWidth * 20f;
		}
		else if (num4 > 0f && isGridafter)
		{
			num2 += num4;
			row.RowFormat.AfterWidth = num4 / 20f;
		}
		float rowWidth = row.GetRowWidth();
		rowWidth *= 20f;
		num2 += rowWidth;
		float num5 = row.RowFormat.AfterWidth;
		if (num5 > 0f && !isGridafter)
		{
			num5 = 0f;
		}
		if (!TableFormat.IsAutoResized && (base.Document.ActualFormatType != 0 || row.Cells.Count <= 0) && row.IsAllCellsHasPointAndPerecentPrefWidth())
		{
			num2 += row.GetRowPreferredWidth(tableWidth) - rowWidth + num3 - row.RowFormat.BeforeWidth * 20f + num4 - num5 * 20f;
		}
		if ((num2 > 0f && (float)Math.Round(num2) != (float)Math.Round(tableWidth * 20f)) || (IsAutoTableExceedsClientWidth() && (float)Math.Round(maxRowWidth * 20f) != (float)Math.Round(num2)))
		{
			num = tableWidth * 20f / num2;
			if (TableFormat.IsAutoResized)
			{
				if ((!(num2 > clientWidth * 20f) && (((PreferredTableWidth.WidthType != FtsWidth.Percentage || !(PreferredTableWidth.Width > 0f)) && !IsTableCellWidthDefined) || !(num > 1f))) || (float)Math.Round(maxRowWidth * 20f) == (float)Math.Round(num2))
				{
					return;
				}
				if (num2 > clientWidth * 20f)
				{
					num = ((tableWidth > maxRowWidth) ? tableWidth : maxRowWidth) * 20f / num2;
				}
				row.RowFormat.BeforeWidth = row.RowFormat.BeforeWidth * num;
				row.RowFormat.AfterWidth = row.RowFormat.AfterWidth * num;
				{
					foreach (WTableCell cell in row.Cells)
					{
						cell.CellFormat.CellWidth = cell.CellFormat.CellWidth * num;
					}
					return;
				}
			}
			if (row.RowFormat.GridBeforeWidth.WidthType >= FtsWidth.Percentage)
			{
				row.RowFormat.BeforeWidth = num3 / 20f * num;
			}
			if (row.RowFormat.GridAfterWidth.WidthType >= FtsWidth.Percentage)
			{
				row.RowFormat.AfterWidth = num4 / 20f * num;
			}
			if (isSkiptoCalculateCellWidth)
			{
				return;
			}
			if (!base.Document.IsOpening && !base.Document.IsCloning && base.Document.IsDOCX() && !IsInCell && PreferredTableWidth.Width > 0f && PreferredTableWidth.WidthType == FtsWidth.Point && !HasPercentPreferredCellWidth && !HasAutoPreferredCellWidth && HasPointPreferredCellWidth && HasNonePreferredCellWidth)
			{
				int defaultPrefCellWidth = 18;
				float rowPreferredWidthFromPoint = row.GetRowPreferredWidthFromPoint(defaultPrefCellWidth);
				num = tableWidth / rowPreferredWidthFromPoint;
				if (row.RowFormat.GridBeforeWidth.WidthType >= FtsWidth.Percentage)
				{
					row.RowFormat.BeforeWidth = num3 / 20f * num;
				}
				if (row.RowFormat.GridAfterWidth.WidthType >= FtsWidth.Percentage)
				{
					row.RowFormat.AfterWidth = num4 / 20f * num;
				}
				float totalWidthToShrink = 0f;
				float totalWidthToExpand = 0f;
				if (rowPreferredWidthFromPoint >= PreferredTableWidth.Width)
				{
					totalWidthToShrink = rowPreferredWidthFromPoint - PreferredTableWidth.Width;
				}
				else
				{
					totalWidthToExpand = PreferredTableWidth.Width - rowPreferredWidthFromPoint;
				}
				{
					foreach (WTableCell cell2 in row.Cells)
					{
						if (cell2.CellFormat.VerticalMerge == CellMerge.Continue)
						{
							WTableCell wTableCell3 = cell2.GetVerticalMergeStartCell();
							if (wTableCell3 == null)
							{
								wTableCell3 = cell2;
							}
							CalculateCellWidthFixedTable(wTableCell3, rowPreferredWidthFromPoint, defaultPrefCellWidth, totalWidthToShrink, totalWidthToExpand);
						}
						else
						{
							CalculateCellWidthFixedTable(cell2, rowPreferredWidthFromPoint, defaultPrefCellWidth, totalWidthToShrink, totalWidthToExpand);
						}
					}
					return;
				}
			}
			{
				foreach (WTableCell cell3 in row.Cells)
				{
					if (cell3.CellFormat.VerticalMerge == CellMerge.Continue)
					{
						WTableCell wTableCell5 = cell3.GetVerticalMergeStartCell();
						if (wTableCell5 == null)
						{
							wTableCell5 = cell3;
						}
						if (wTableCell5.PreferredWidth.WidthType == FtsWidth.Point)
						{
							List<float> list = RecalculatevMergeRow(wTableCell5, cell3, row, tableWidth);
							if (list != null)
							{
								int num6 = list.Count;
								foreach (WTableCell cell4 in row.Cells)
								{
									num6--;
									cell4.CellFormat.CellWidth = list[num6];
								}
							}
							else
							{
								cell3.CellFormat.CellWidth = wTableCell5.CellFormat.CellWidth;
							}
						}
						else if (wTableCell5.PreferredWidth.WidthType == FtsWidth.Percentage)
						{
							cell3.CellFormat.CellWidth = tableWidth * wTableCell5.PreferredWidth.Width / 100f * num;
						}
					}
					else
					{
						if (cell3.PreferredWidth.Width == 0f)
						{
							continue;
						}
						if (cell3.PreferredWidth.WidthType == FtsWidth.Point && !UsePreferredCellWidth)
						{
							if (maxPrefWidthRowIndex != -1)
							{
								if (maxPrefWidthRowIndex == cell3.OwnerRow.Index)
								{
									cell3.CellFormat.CellWidth = cell3.PreferredWidth.Width * num;
									if (cell3.Index != cell3.OwnerRow.Cells.Count - 1)
									{
										continue;
									}
									for (int i = 0; i < maxPrefWidthRowIndex; i++)
									{
										for (int j = 0; j < Rows[0].Cells.Count; j++)
										{
											Rows[i].Cells[j].CellFormat.CellWidth = Rows[maxPrefWidthRowIndex].Cells[j].CellFormat.CellWidth;
										}
									}
								}
								else if (maxPrefWidthRowIndex < cell3.OwnerRow.Index)
								{
									cell3.CellFormat.CellWidth = Rows[maxPrefWidthRowIndex].Cells[cell3.Index].CellFormat.CellWidth;
								}
								else
								{
									cell3.CellFormat.CellWidth = 0f;
								}
							}
							else
							{
								cell3.CellFormat.CellWidth = cell3.PreferredWidth.Width * num;
							}
						}
						else if (cell3.PreferredWidth.WidthType == FtsWidth.Percentage)
						{
							cell3.CellFormat.CellWidth = tableWidth * cell3.PreferredWidth.Width / 100f * num;
						}
					}
				}
				return;
			}
		}
		if (TableFormat.IsAutoResized || (base.Document.ActualFormatType == FormatType.Doc && row.Cells.Count > 0))
		{
			return;
		}
		if (row.RowFormat.GridBeforeWidth.WidthType >= FtsWidth.Percentage)
		{
			row.RowFormat.BeforeWidth = num3 / 20f;
		}
		if (row.RowFormat.GridAfterWidth.WidthType >= FtsWidth.Percentage)
		{
			row.RowFormat.AfterWidth = num4 / 20f;
		}
		foreach (WTableCell cell5 in row.Cells)
		{
			if (cell5.CellFormat.VerticalMerge == CellMerge.Continue)
			{
				WTableCell wTableCell7 = cell5.GetVerticalMergeStartCell();
				if (wTableCell7 == null)
				{
					wTableCell7 = cell5;
				}
				if (wTableCell7.PreferredWidth.WidthType == FtsWidth.Point)
				{
					cell5.CellFormat.CellWidth = wTableCell7.CellFormat.CellWidth;
				}
				else if (wTableCell7.PreferredWidth.WidthType == FtsWidth.Percentage)
				{
					cell5.CellFormat.CellWidth = tableWidth * wTableCell7.PreferredWidth.Width / 100f;
				}
			}
			else if (cell5.PreferredWidth.Width != 0f)
			{
				if (cell5.PreferredWidth.WidthType == FtsWidth.Point)
				{
					cell5.CellFormat.CellWidth = cell5.PreferredWidth.Width;
				}
				else if (cell5.PreferredWidth.WidthType == FtsWidth.Percentage)
				{
					cell5.CellFormat.CellWidth = tableWidth * cell5.PreferredWidth.Width / 100f;
				}
			}
		}
	}

	private List<float> RecalculatevMergeRow(WTableCell vmergecell, WTableCell cell, WTableRow currRow, float tableWidth)
	{
		List<float> list = null;
		if ((m_doc.IsDOCX() || m_doc.ActualFormatType == FormatType.Rtf) && vmergecell == cell && cell.Index == currRow.Cells.Count - 1 && currRow.PreviousSibling is WTableRow && currRow.GetRowPreferredWidth(tableWidth) <= (currRow.PreviousSibling as WTableRow).GetRowPreferredWidth(tableWidth) && (currRow.Cells.Count - 1 == (currRow.PreviousSibling as WTableRow).Cells.Count || currRow.Cells.Count == (currRow.PreviousSibling as WTableRow).Cells.Count))
		{
			list = new List<float>();
			WTableRow wTableRow = currRow.PreviousSibling as WTableRow;
			int num = currRow.Cells.Count - 1;
			while (num >= 0)
			{
				int index = ((currRow.Cells.Count - 1 == wTableRow.Cells.Count) ? (num - 1) : num);
				if (currRow.Cells.Count - 1 == wTableRow.Cells.Count && currRow.Cells[num].PreferredWidth.Width != wTableRow.Cells[index].PreferredWidth.Width)
				{
					float num2 = 0f;
					float num3 = 0f;
					for (int i = 0; i <= num; i++)
					{
						num2 += currRow.Cells[i].PreferredWidth.Width;
					}
					if (num == 1 && wTableRow.Cells[0].PreferredWidth.Width > num2)
					{
						num3 = wTableRow.Cells[0].CellFormat.CellWidth;
						for (int num4 = num; num4 >= 0; num4--)
						{
							float num5 = currRow.Cells[num4].PreferredWidth.Width / num2;
							list.Add(num3 * num5);
						}
						return list;
					}
					return null;
				}
				if (currRow.Cells[num].PreferredWidth.Width == wTableRow.Cells[index].PreferredWidth.Width)
				{
					list.Add(wTableRow.Cells[index].CellFormat.CellWidth);
					num--;
					continue;
				}
				return null;
			}
		}
		return list;
	}

	private bool IsAllRowCellHasSamePrefWidth()
	{
		for (int i = 1; i < Rows.Count && Rows[0].Cells.Count == Rows[i].Cells.Count; i++)
		{
			for (int j = 0; j < Rows[i].Cells.Count; j++)
			{
				if (Rows[0].Cells[j].PreferredWidth.Width != Rows[i].Cells[j].PreferredWidth.Width)
				{
					return false;
				}
			}
		}
		return true;
	}

	private int GetMaxPreferredWidthRowIndex(float tableWidth)
	{
		int result = 0;
		float num = Rows[0].GetRowPreferredWidth(tableWidth);
		for (int i = 1; i < Rows.Count; i++)
		{
			float rowPreferredWidth = Rows[i].GetRowPreferredWidth(tableWidth);
			if (rowPreferredWidth > num)
			{
				result = i;
				num = rowPreferredWidth;
			}
		}
		return result;
	}

	private void CalculateCellWidthFixedTable(WTableCell cell, float rowWidth, int defaultPrefCellWidth, float totalWidthToShrink, float totalWidthToExpand)
	{
		float num = 0f;
		if (cell.PreferredWidth.WidthType == FtsWidth.Point)
		{
			num = cell.PreferredWidth.Width / rowWidth;
			cell.CellFormat.CellWidth = ((rowWidth >= PreferredTableWidth.Width) ? (cell.PreferredWidth.Width - num * totalWidthToShrink) : (cell.PreferredWidth.Width + num * totalWidthToExpand));
		}
		else if (cell.PreferredWidth.WidthType == FtsWidth.None)
		{
			num = (float)defaultPrefCellWidth / rowWidth;
			cell.CellFormat.CellWidth = ((rowWidth >= PreferredTableWidth.Width) ? ((float)defaultPrefCellWidth - num * totalWidthToShrink) : ((float)defaultPrefCellWidth + num * totalWidthToExpand));
		}
	}

	internal bool IsAutoTableExceedsClientWidth()
	{
		WSection wSection = GetOwnerSection(this) as WSection;
		if (m_doc.IsDOCX() && !IsInCell && TableFormat.IsAutoResized && !TableFormat.WrapTextAround && PreferredTableWidth.WidthType == FtsWidth.Point && wSection != null)
		{
			return PreferredTableWidth.Width > wSection.PageSetup.ClientWidth;
		}
		return false;
	}

	private bool IsNeedtoResizeCell(float tableWidth)
	{
		float num = 0f;
		float num2 = 0f;
		if (base.Document.ActualFormatType == FormatType.Doc && !TableFormat.IsAutoResized)
		{
			foreach (WTableRow row in Rows)
			{
				foreach (WTableCell cell in row.Cells)
				{
					if ((cell.PreferredWidth.WidthType == FtsWidth.Point || cell.PreferredWidth.WidthType == FtsWidth.Percentage) && cell.PreferredWidth.Width > 0f)
					{
						num += ((cell.PreferredWidth.WidthType == FtsWidth.Percentage) ? (cell.PreferredWidth.Width * tableWidth / 100f) : cell.PreferredWidth.Width);
						num2 += cell.CellFormat.CellWidth;
						continue;
					}
					return false;
				}
				if (num > num2)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private float GetGridBeforeAfter(WTableRow row, float clientWidth, bool isAfterWidth, float tableWidth, float currOffset, float maxRowWidth, bool isTableGridMissMatch)
	{
		float num = (isAfterWidth ? row.RowFormat.AfterWidth : row.RowFormat.BeforeWidth) * 20f;
		PreferredWidthInfo preferredWidthInfo = (isAfterWidth ? row.RowFormat.GridAfterWidth : row.RowFormat.GridBeforeWidth);
		float num2 = 0f;
		if ((!TableFormat.IsAutoResized && (PreferredTableWidth.WidthType < FtsWidth.Percentage || PreferredTableWidth.Width == 0f)) || isTableGridMissMatch)
		{
			if (preferredWidthInfo.WidthType == FtsWidth.Point)
			{
				num2 = preferredWidthInfo.Width * 20f;
			}
			else if (preferredWidthInfo.WidthType == FtsWidth.Percentage)
			{
				num2 = tableWidth * preferredWidthInfo.Width / 5f;
			}
		}
		if (num < num2 || isTableGridMissMatch)
		{
			if (isAfterWidth)
			{
				row.RowFormat.AfterWidth = num2 / 20f;
			}
			else
			{
				row.RowFormat.BeforeWidth = num2 / 20f;
			}
			num = num2;
		}
		if (PreferredTableWidth.WidthType >= FtsWidth.Percentage && PreferredTableWidth.Width > 0f && (float)Math.Round(tableWidth * 20f) < (float)Math.Round(currOffset + num))
		{
			if (TableFormat.IsAutoResized)
			{
				if (currOffset + num > clientWidth * 20f && (float)Math.Round(maxRowWidth * 20f) != (float)Math.Round(currOffset + num))
				{
					num = clientWidth * 20f - currOffset;
					if (isAfterWidth)
					{
						row.RowFormat.AfterWidth = num / 20f;
					}
					else
					{
						row.RowFormat.BeforeWidth = num / 20f;
					}
				}
			}
			else
			{
				num = tableWidth * 20f - currOffset;
				if (isAfterWidth)
				{
					row.RowFormat.AfterWidth = num / 20f;
				}
				else
				{
					row.RowFormat.BeforeWidth = num / 20f;
				}
			}
		}
		return num;
	}

	private float GetGridBeforeAfter(PreferredWidthInfo widthInfo, float tableWidth)
	{
		float result = 0f;
		if (widthInfo.WidthType == FtsWidth.Point)
		{
			result = widthInfo.Width * 20f;
		}
		else if (widthInfo.WidthType == FtsWidth.Percentage)
		{
			result = tableWidth * widthInfo.Width / 5f;
		}
		return result;
	}

	private float GetCellWidth(WTableCell cell, float clientWidth, float tableWidth, float currOffset, float maxRowWidth, bool isTableGridMissMatch, bool isRowHasDefinedCells, int cellcolumnIndex)
	{
		float num = cell.Width * 20f;
		float num2 = 0f;
		if ((!TableFormat.IsAutoResized && (PreferredTableWidth.WidthType < FtsWidth.Percentage || PreferredTableWidth.Width == 0f)) || isTableGridMissMatch)
		{
			if (!isTableGridMissMatch && cell.CellFormat.HorizontalMerge == CellMerge.Start && cell.NextSibling is WTableCell && (cell.NextSibling as WTableCell).CellFormat.HorizontalMerge == CellMerge.Continue && cell.CellFormat.PropertiesHash.ContainsKey(13) && cell.CellFormat.PropertiesHash.ContainsKey(14) && cell.CellFormat.PreferredWidth.WidthType == FtsWidth.Point && HasPointPreferredCellWidth && !HasPercentPreferredCellWidth && !HasAutoPreferredCellWidth)
			{
				float totalCellPreferredWidth = 0f;
				List<float> maxPrefCellWidthOfColumns = GetMaxPrefCellWidthOfColumns(ref totalCellPreferredWidth, MaximumCellCount());
				if (totalCellPreferredWidth > 0f)
				{
					cell.GridSpan = 1;
					while (cell.NextSibling is WTableCell && (cell.NextSibling as WTableCell).CellFormat.HorizontalMerge == CellMerge.Continue)
					{
						(cell.NextSibling as WTableCell).RemoveSelf();
						cell.GridSpan++;
					}
					if (cellcolumnIndex + cell.GridSpan <= maxPrefCellWidthOfColumns.Count)
					{
						cell.CellFormat.CellWidth = 0f;
						for (int i = cellcolumnIndex; i < cellcolumnIndex + cell.GridSpan; i++)
						{
							cell.CellFormat.CellWidth += maxPrefCellWidthOfColumns[i];
						}
					}
					cell.CellFormat.HorizontalMerge = CellMerge.None;
					if (cell.CellFormat.CellWidth < cell.PreferredWidth.Width)
					{
						cell.CellFormat.CellWidth = cell.PreferredWidth.Width;
					}
					return cell.CellFormat.CellWidth;
				}
			}
			if (cell.CellFormat.VerticalMerge == CellMerge.Continue)
			{
				WTableCell wTableCell = cell.GetVerticalMergeStartCell();
				if (wTableCell == null)
				{
					wTableCell = cell;
				}
				if (wTableCell.PreferredWidth.WidthType == FtsWidth.Point)
				{
					num2 = wTableCell.PreferredWidth.Width * 20f;
				}
				else if (wTableCell.PreferredWidth.WidthType == FtsWidth.Percentage)
				{
					num2 = tableWidth * wTableCell.PreferredWidth.Width / 5f;
				}
			}
			else if (cell.PreferredWidth.WidthType == FtsWidth.Point)
			{
				num2 = cell.PreferredWidth.Width * 20f;
			}
			else if (cell.PreferredWidth.WidthType == FtsWidth.Percentage)
			{
				float preferredWidth = 0f;
				cell.OwnerRow.GetRowWidth(ref preferredWidth, tableWidth);
				float num3 = ((preferredWidth == 0f) ? 1f : (100f / preferredWidth));
				num2 = tableWidth * (cell.PreferredWidth.Width / 5f * num3);
			}
			else if (cell.PreferredWidth.WidthType == FtsWidth.Auto && !isRowHasDefinedCells)
			{
				num2 = ((tableWidth > 0f) ? tableWidth : clientWidth) / (float)cell.OwnerRow.Cells.Count * 20f;
			}
		}
		bool flag = base.Document != null && base.Document.IsOpening && (base.Document.ActualFormatType.ToString().Contains("Docx") || base.Document.ActualFormatType.ToString().Contains("Word")) && !IsInCell && !isTableGridMissMatch && !TableFormat.IsAutoResized && (PreferredTableWidth.WidthType < FtsWidth.Percentage || PreferredTableWidth.Width == 0f);
		if (num2 != 0f && (num < num2 || isTableGridMissMatch || flag))
		{
			cell.CellFormat.CellWidth = num2 / 20f;
			num = num2;
		}
		if (PreferredTableWidth.WidthType >= FtsWidth.Percentage && PreferredTableWidth.Width > 0f && (float)Math.Round(tableWidth * 20f) < (float)Math.Round(currOffset + num) && !IsHtmlTableExceedsClientWidth())
		{
			if (TableFormat.IsAutoResized)
			{
				if (!m_doc.IsOpening && PreferredTableWidth.WidthType != FtsWidth.Point && PreferredTableWidth.Width < clientWidth && currOffset + num > clientWidth * 20f && (float)Math.Round(maxRowWidth * 20f) != (float)Math.Round(currOffset + num))
				{
					num = clientWidth * 20f - currOffset;
					cell.CellFormat.CellWidth = num / 20f;
				}
			}
			else
			{
				num = tableWidth * 20f - currOffset;
				cell.CellFormat.CellWidth = num / 20f;
			}
		}
		return num;
	}

	internal List<float> GetMaxPrefCellWidthOfColumns(ref float totalCellPreferredWidth, int maxCellCount)
	{
		if (m_maxPrefCellWidthOfColumns == null)
		{
			m_maxPrefCellWidthOfColumns = new List<float>();
			for (int i = 0; i < maxCellCount; i++)
			{
				float num = 0f;
				for (int j = 0; j < Rows.Count; j++)
				{
					WTableRow wTableRow = Rows[j];
					if (i < wTableRow.Cells.Count && wTableRow.Cells.Count == maxCellCount && wTableRow.Cells[i].CellFormat.HorizontalMerge == CellMerge.None && num < wTableRow.Cells[i].PreferredWidth.Width)
					{
						num = wTableRow.Cells[i].PreferredWidth.Width;
					}
				}
				m_maxPrefCellWidthOfColumns.Add(num);
				m_totalmaxPrefCellWidthOfColumns += num;
			}
		}
		totalCellPreferredWidth = m_totalmaxPrefCellWidthOfColumns;
		return m_maxPrefCellWidthOfColumns;
	}

	internal bool IsHtmlTableExceedsClientWidth()
	{
		if (m_doc.IsDOCX() && IsFromHTML && TableFormat.IsAutoResized && !IsInCell && PreferredTableWidth.WidthType == FtsWidth.Point && HasOnlyParagraphs && HasPointPreferredCellWidth && !HasPercentPreferredCellWidth && !HasNonePreferredCellWidth)
		{
			return !HasAutoPreferredCellWidth;
		}
		return false;
	}

	internal float GetOwnerWidth()
	{
		float num = 0f;
		if (IsInCell)
		{
			WTableCell ownerTableCell = GetOwnerTableCell();
			num = ownerTableCell.Width;
			if (ownerTableCell.CellFormat.HorizontalMerge == CellMerge.Start)
			{
				WTableCell wTableCell = ownerTableCell.NextSibling as WTableCell;
				while (wTableCell != null && wTableCell.CellFormat.HorizontalMerge == CellMerge.Continue)
				{
					num += wTableCell.Width;
					wTableCell = wTableCell.NextSibling as WTableCell;
				}
			}
			Paddings paddings = ((ownerTableCell.OwnerRow != null && ownerTableCell.OwnerRow.OwnerTable != null) ? ownerTableCell.OwnerRow.OwnerTable.TableFormat.Paddings : ((ownerTableCell.OwnerRow != null) ? ownerTableCell.OwnerRow.RowFormat.Paddings : ownerTableCell.CellFormat.Paddings));
			float num2 = ((ownerTableCell.OwnerRow != null && ownerTableCell.OwnerRow.OwnerTable != null) ? ownerTableCell.OwnerRow.OwnerTable.TableFormat.CellSpacing : ((ownerTableCell.OwnerRow != null) ? ownerTableCell.OwnerRow.RowFormat.CellSpacing : 0f));
			float num3 = ownerTableCell.CellFormat.Paddings.Left;
			if (ownerTableCell.CellFormat.SamePaddingsAsTable)
			{
				if (paddings.HasKey(1))
				{
					num3 = paddings.Left;
				}
				else if (base.Document.ActualFormatType != 0 || !base.Document.IsOpening)
				{
					num3 = 5.4f;
				}
			}
			float num4 = ownerTableCell.CellFormat.Paddings.Right;
			if (ownerTableCell.CellFormat.SamePaddingsAsTable)
			{
				if (paddings.HasKey(4))
				{
					num4 = paddings.Right;
				}
				else if (base.Document.ActualFormatType != 0 || !base.Document.IsOpening)
				{
					num4 = 5.4f;
				}
			}
			if (num2 > 0f)
			{
				num3 += num2 * 2f + TableFormat.Borders.Left.GetLineWidthValue();
				num4 += num2 * 2f + TableFormat.Borders.Right.GetLineWidthValue();
			}
			num -= num3 + num4;
		}
		else if (base.Owner != null && base.Owner.OwnerBase is WTextBox)
		{
			WTextBox wTextBox = base.Owner.OwnerBase as WTextBox;
			num = wTextBox.TextBoxFormat.Width;
			if (wTextBox.TextBoxFormat.NoLine)
			{
				if (m_doc.IsDOCX() && m_doc.Settings.CompatibilityMode == CompatibilityMode.Word2007 && TableFormat.IsAutoResized && !wTextBox.TextBoxFormat.AutoFit && (!wTextBox.IsShape || !wTextBox.Shape.TextFrame.ShapeAutoFit))
				{
					num -= wTextBox.TextBoxFormat.InternalMargin.Left;
					num += 7.2f;
				}
				else
				{
					num -= wTextBox.TextBoxFormat.InternalMargin.Left + wTextBox.TextBoxFormat.InternalMargin.Right;
				}
			}
			else
			{
				num -= wTextBox.TextBoxFormat.LineWidth + wTextBox.TextBoxFormat.InternalMargin.Left + wTextBox.TextBoxFormat.InternalMargin.Right;
			}
		}
		else if (IsFrame && base.Document.ActualFormatType == FormatType.Docx && Rows[0].Cells[0].Paragraphs[0] != null && Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.FrameWidth > 0f)
		{
			num = Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.FrameWidth;
		}
		else
		{
			WSection ownerSection = GetOwnerSection();
			if (ownerSection != null)
			{
				num = ((ownerSection.Columns.Count <= 1 || GetOwnerHeaderFooter() != null) ? ownerSection.PageSetup.ClientWidth : ownerSection.Columns[0].Width);
				num -= (base.Document.DOP.GutterAtTop ? 0f : ownerSection.PageSetup.Margins.Gutter);
			}
			else if (base.Document.LastSection != null)
			{
				num = base.Document.LastSection.PageSetup.ClientWidth - (base.Document.DOP.GutterAtTop ? 0f : base.Document.LastSection.PageSetup.Margins.Gutter);
			}
		}
		if (num < 0f)
		{
			num = 0f;
		}
		return num;
	}

	internal float GetTableClientWidth(float clientWidth)
	{
		float num = 0f;
		if (PreferredTableWidth.WidthType == FtsWidth.Point && PreferredTableWidth.Width > 0f)
		{
			num = PreferredTableWidth.Width;
		}
		else
		{
			num = clientWidth;
			if (PreferredTableWidth.WidthType == FtsWidth.Percentage && PreferredTableWidth.Width > 0f)
			{
				num = num * PreferredTableWidth.Width / 100f;
				if (IsInCell || (IsFrame && base.Document.ActualFormatType == FormatType.Docx))
				{
					float lineWidthValue = Rows[0].Cells[0].CellFormat.Borders.Left.GetLineWidthValue();
					if (lineWidthValue < TableFormat.Borders.Left.LineWidth)
					{
						lineWidthValue = TableFormat.Borders.Left.GetLineWidthValue();
					}
					float lineWidthValue2 = Rows[0].Cells[Rows[0].Cells.Count - 1].CellFormat.Borders.Right.GetLineWidthValue();
					if (lineWidthValue2 < TableFormat.Borders.Right.LineWidth)
					{
						lineWidthValue2 = TableFormat.Borders.Right.GetLineWidthValue();
					}
					num -= lineWidthValue / 2f + lineWidthValue2 / 2f;
				}
				else if (Rows[0].Cells.Count > 0)
				{
					if (m_doc.IsDOCX() && !TableFormat.IsAutoResized && m_doc.Settings.CompatibilityMode == CompatibilityMode.Word2013)
					{
						float lineWidthValue3 = Rows[0].Cells[0].CellFormat.Borders.Left.GetLineWidthValue();
						if (lineWidthValue3 < TableFormat.Borders.Left.LineWidth)
						{
							lineWidthValue3 = TableFormat.Borders.Left.GetLineWidthValue();
						}
						float lineWidthValue4 = Rows[0].Cells[Rows[0].Cells.Count - 1].CellFormat.Borders.Right.GetLineWidthValue();
						if (lineWidthValue4 < TableFormat.Borders.Right.LineWidth)
						{
							lineWidthValue4 = TableFormat.Borders.Right.GetLineWidthValue();
						}
						num -= lineWidthValue3 / 2f + lineWidthValue4 / 2f;
					}
					else
					{
						float leftPad = 0f;
						float rightPad = 0f;
						bool flag = !TableFormat.Paddings.HasKey(1) && (m_style == null || !(m_style as WTableStyle).TableProperties.Paddings.HasKey(1));
						bool flag2 = !TableFormat.Paddings.HasKey(4) && (m_style == null || !(m_style as WTableStyle).TableProperties.Paddings.HasKey(4));
						CalculatePaddingOfTableWidth(ref leftPad, ref rightPad);
						if (!(base.Document != null && base.Document.IsDOCX() && TableFormat.IsAutoResized && base.Document.Settings.CompatibilityMode == CompatibilityMode.Word2013 && flag && flag2))
						{
							num += leftPad + rightPad;
						}
						if (TableFormat.CellSpacing > 0f)
						{
							num += TableFormat.CellSpacing * 2f + TableFormat.Borders.Left.GetLineWidthValue();
							num += TableFormat.CellSpacing * 2f + TableFormat.Borders.Right.GetLineWidthValue();
						}
					}
				}
				if (num > clientWidth && base.Owner != null && base.Owner.OwnerBase is WTextBox)
				{
					num = clientWidth * PreferredTableWidth.Width / 100f;
				}
			}
		}
		if (num < 0f)
		{
			num = 0f;
		}
		return num;
	}

	internal void CalculatePaddingOfTableWidth(ref float leftPad, ref float rightPad)
	{
		leftPad = Rows[0].Cells[0].CellFormat.Paddings.Left;
		if (Rows[0].Cells[0].CellFormat.SamePaddingsAsTable)
		{
			if (TableFormat.Paddings.HasKey(1))
			{
				leftPad = TableFormat.Paddings.Left;
			}
			else if (m_style != null && (m_style as WTableStyle).TableProperties.Paddings.HasKey(1))
			{
				leftPad = (m_style as WTableStyle).TableProperties.Paddings.Left;
			}
			else if (base.Document.ActualFormatType != 0)
			{
				leftPad = 5.4f;
			}
		}
		rightPad = Rows[0].Cells[Rows[0].Cells.Count - 1].CellFormat.Paddings.Right;
		if (Rows[0].Cells[Rows[0].Cells.Count - 1].CellFormat.SamePaddingsAsTable)
		{
			if (TableFormat.Paddings.HasKey(4))
			{
				rightPad = TableFormat.Paddings.Right;
			}
			else if (m_style != null && (m_style as WTableStyle).TableProperties.Paddings.HasKey(4))
			{
				rightPad = (m_style as WTableStyle).TableProperties.Paddings.Right;
			}
			else if (base.Document.ActualFormatType != 0)
			{
				rightPad = 5.4f;
			}
		}
	}

	private void UpdateTableGrid(float currOffset)
	{
		currOffset = (float)Math.Round(currOffset);
		if (m_tableGrid.IndexOf(currOffset) >= 0 || currOffset == 0f)
		{
			return;
		}
		if (m_tableGrid.Count > 0)
		{
			int i = 0;
			for (int count = m_tableGrid.Count; i < count; i++)
			{
				if (m_tableGrid[i].EndOffset > currOffset)
				{
					m_tableGrid.InsertColumn(i, currOffset);
					break;
				}
				if (count == i + 1)
				{
					m_tableGrid.AddColumns(currOffset);
				}
			}
		}
		else
		{
			m_tableGrid.AddColumns(currOffset);
		}
	}

	internal override TextBodyItem GetNextTextBodyItemValue()
	{
		if (base.NextSibling == null)
		{
			if (IsInCell)
			{
				GetOwnerTableCell().GetNextTextBodyItem();
			}
			else if (base.OwnerTextBody != null)
			{
				GetNextInSection(base.OwnerTextBody.Owner as WSection);
			}
			return null;
		}
		return base.NextSibling as TextBodyItem;
	}

	internal void UpdateFormat(FormatBase format, int propKey)
	{
		if (format is RowFormat)
		{
			foreach (WTableRow row in Rows)
			{
				row.RowFormat.SetPropertyValue(propKey, TableFormat.GetPropertyValue(propKey));
			}
			return;
		}
		if (format is Border || format is Borders)
		{
			foreach (WTableRow row2 in Rows)
			{
				row2.RowFormat.Borders.ImportContainer(TableFormat.Borders);
			}
			return;
		}
		if (!(format is Paddings))
		{
			return;
		}
		foreach (WTableRow row3 in Rows)
		{
			row3.RowFormat.Paddings.ImportContainer(TableFormat.Paddings);
		}
	}

	internal override void Close()
	{
		if (m_rows != null && m_rows.Count > 0)
		{
			int count = m_rows.Count;
			for (int i = 0; i < count; i++)
			{
				m_rows[i].Close();
			}
			m_rows.Close();
			m_rows = null;
		}
		if (m_initTableFormat != null)
		{
			m_initTableFormat.Close();
			m_initTableFormat = null;
		}
		if (m_tableGrid != null)
		{
			m_tableGrid.Close();
			m_tableGrid = null;
		}
		if (m_xmlTblFormat != null)
		{
			m_xmlTblFormat.Close();
			m_xmlTblFormat = null;
		}
		if (m_style != null)
		{
			m_style.Close();
			m_style = null;
		}
		if (m_trackTableGrid != null)
		{
			m_trackTableGrid.Close();
			m_trackTableGrid = null;
		}
		if (m_recalculateTables != null)
		{
			m_recalculateTables.Clear();
			m_recalculateTables = null;
		}
		if (m_maxPrefCellWidthOfColumns != null)
		{
			m_maxPrefCellWidthOfColumns.Clear();
			m_maxPrefCellWidthOfColumns = null;
		}
		base.Close();
	}

	internal float UpdateWidth()
	{
		float num = 0f;
		if (Rows.Count > 0 && TableGrid != null)
		{
			float num2 = 0f;
			WTableCell wTableCell = null;
			int i = 0;
			for (int count = Rows.Count; i < count; i++)
			{
				num2 = 0f;
				int num3 = 0;
				int j = 0;
				for (int count2 = Rows[i].Cells.Count; j < count2; j++)
				{
					wTableCell = Rows[i].Cells[j];
					num2 = ((!(wTableCell.Width > 1584f)) ? (num2 + wTableCell.Width) : ((num3 + wTableCell.GridSpan - 1 >= m_tableGrid.Count) ? (num2 + 1584f) : (num2 + ((num3 == 0) ? m_tableGrid[num3 + wTableCell.GridSpan - 1].EndOffset : (m_tableGrid[num3 + wTableCell.GridSpan - 1].EndOffset - m_tableGrid[num3 - 1].EndOffset)) / 20f)));
					num3 += wTableCell.GridSpan;
				}
				if (base.Document != null && base.Document.GrammarSpellingData == null && Rows[i].RowFormat.HorizontalAlignment == RowAlignment.Left)
				{
					num2 += Math.Abs(Rows[i].RowFormat.LeftIndent);
				}
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	private WSection GetOwnerSection()
	{
		for (IEntity entity = this; entity != null; entity = entity.Owner)
		{
			if (entity.EntityType == EntityType.Section)
			{
				return entity as WSection;
			}
		}
		return null;
	}

	private HeaderFooter GetOwnerHeaderFooter()
	{
		for (IEntity entity = this; entity != null; entity = entity.Owner)
		{
			if (entity.EntityType == EntityType.HeaderFooter)
			{
				return entity as HeaderFooter;
			}
		}
		return null;
	}

	internal WTable GetOwnerTable()
	{
		if (!IsInCell)
		{
			return null;
		}
		return ((base.Owner as WTableCell).Owner as WTableRow).Owner as WTable;
	}

	internal void RemoveUnwantedHorizontalMergeCells(WTable table)
	{
		if (!HasHorizontalMergeCells)
		{
			return;
		}
		int num = table.MaximumCellCount();
		foreach (WTableRow row in table.Rows)
		{
			if (row.Cells.Count != num || row.Cells[row.Cells.Count - 1].CellFormat.HorizontalMerge != CellMerge.Continue || row.Cells[row.Cells.Count - 1].GridSpan != 1)
			{
				continue;
			}
			for (int num2 = row.Cells.Count - 2; num2 >= 0; num2--)
			{
				WTableCell wTableCell = row.Cells[num2];
				if (wTableCell.CellFormat.HorizontalMerge != CellMerge.Continue || wTableCell.GridSpan != 1)
				{
					if (wTableCell.CellFormat.HorizontalMerge == CellMerge.Start && wTableCell.GridSpan > 1 && !wTableCell.CellFormat.PropertiesHash.ContainsKey(13))
					{
						wTableCell.CellFormat.HorizontalMerge = CellMerge.None;
						for (int num3 = row.Cells.Count - 1; num3 > num2; num3--)
						{
							row.Cells.RemoveAt(num3);
						}
					}
					break;
				}
			}
		}
	}

	internal int MaxCellCountWithoutSpannedCells(WTable table)
	{
		bool flag = false;
		int num = 0;
		foreach (WTableRow row in table.Rows)
		{
			if (row.Cells.Count <= num)
			{
				continue;
			}
			foreach (WTableCell cell in row.Cells)
			{
				if (cell.GridSpan > 1)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				num = row.Cells.Count;
			}
			flag = false;
		}
		return num;
	}

	internal override void MakeChanges(bool acceptChanges)
	{
		for (int i = 0; i < m_rows.Count; i++)
		{
			WTableRow wTableRow = m_rows[i];
			foreach (WTableCell cell in wTableRow.Cells)
			{
				cell.MakeChanges(acceptChanges);
				if (acceptChanges)
				{
					cell.m_trackCellFormat = null;
					if (cell.CellFormat.OldPropertiesHash.Count > 0)
					{
						cell.CellFormat.OldPropertiesHash.Clear();
					}
				}
				else if (cell.m_trackCellFormat != null)
				{
					cell.CellFormat.ClearFormatting();
					cell.CellFormat.ImportContainer(cell.TrackCellFormat);
					cell.m_trackCellFormat = null;
				}
			}
			if (acceptChanges)
			{
				wTableRow.m_trackRowFormat = null;
				if (wTableRow.RowFormat.OldPropertiesHash.Count > 0)
				{
					wTableRow.RowFormat.OldPropertiesHash.Clear();
				}
			}
			else if (wTableRow.m_trackRowFormat != null)
			{
				wTableRow.RowFormat.ClearFormatting();
				wTableRow.m_trackRowFormat = null;
			}
			if ((wTableRow.IsDeleteRevision && acceptChanges) || (wTableRow.IsInsertRevision && !acceptChanges))
			{
				m_rows.RemoveAt(i);
				i--;
			}
			if (wTableRow.IsInsertRevision && acceptChanges)
			{
				wTableRow.IsInsertRevision = false;
			}
		}
		if (Rows.Count == 0 && base.OwnerTextBody != null)
		{
			base.OwnerTextBody.ChildEntities.Remove(this);
		}
	}

	internal override void RemoveCFormatChanges()
	{
		foreach (WTableRow row in m_rows)
		{
			row.CharacterFormat.RemoveChanges();
			foreach (WTableCell cell in row.Cells)
			{
				cell.CharacterFormat.RemoveChanges();
			}
		}
	}

	internal override void RemovePFormatChanges()
	{
		foreach (WTableRow row in m_rows)
		{
			row.RowFormat.RemoveChanges();
		}
	}

	internal override void AcceptCChanges()
	{
		foreach (WTableRow row in m_rows)
		{
			row.CharacterFormat.AcceptChanges();
			foreach (WTableCell cell in row.Cells)
			{
				cell.CharacterFormat.AcceptChanges();
			}
		}
	}

	internal override void AcceptPChanges()
	{
		foreach (WTableRow row in m_rows)
		{
			row.RowFormat.AcceptChanges();
		}
	}

	internal override bool CheckChangedPFormat()
	{
		foreach (WTableRow row in m_rows)
		{
			if (row.RowFormat.IsChangedFormat)
			{
				return true;
			}
			foreach (WTableCell cell in row.Cells)
			{
				if (cell.CellFormat.IsChangedFormat)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal override bool CheckDeleteRev()
	{
		foreach (WTableRow row in m_rows)
		{
			if (row.CharacterFormat.IsDeleteRevision)
			{
				return true;
			}
		}
		return false;
	}

	internal override bool CheckInsertRev()
	{
		foreach (WTableRow row in m_rows)
		{
			if (row.CharacterFormat.IsInsertRevision)
			{
				return true;
			}
		}
		return false;
	}

	internal override bool CheckChangedCFormat()
	{
		foreach (WTableRow row in m_rows)
		{
			if (row.CharacterFormat.IsChangedFormat)
			{
				return true;
			}
			foreach (WTableCell cell in row.Cells)
			{
				if (cell.CharacterFormat.IsChangedFormat)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal override bool HasTrackedChanges()
	{
		if (base.IsDeleteRevision || base.IsInsertRevision || base.IsChangedCFormat || base.IsChangedPFormat)
		{
			return true;
		}
		foreach (WTableRow row in m_rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				foreach (TextBodyItem item in cell.Items)
				{
					if (item.HasTrackedChanges())
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	internal bool RemoveChangedTable(bool acceptChanges)
	{
		WTableRow wTableRow = null;
		for (int i = 0; i < m_rows.Count; i++)
		{
			wTableRow = m_rows[i];
			if (wTableRow.IsDeleteRevision)
			{
				if (m_rows.Count <= 1)
				{
					return true;
				}
				if (acceptChanges)
				{
					m_rows.Remove(wTableRow);
					i--;
				}
			}
		}
		return false;
	}

	internal override void SetDeleteRev(bool check)
	{
		RowFormat format = DocxTableFormat.Format;
		string text = (format.HasValue(123) ? format.FormatChangeAuthorName : string.Empty);
		DateTime dateTime = (format.HasValue(124) ? format.FormatChangeDateTime : DateTime.MinValue);
		foreach (WTableRow row in m_rows)
		{
			row.CharacterFormat.IsDeleteRevision = check;
			if (!string.IsNullOrEmpty(text))
			{
				row.RowFormat.FormatChangeAuthorName = text;
			}
			if (dateTime != DateTime.MinValue)
			{
				row.RowFormat.FormatChangeDateTime = dateTime;
			}
		}
	}

	internal override void SetInsertRev(bool check)
	{
		RowFormat format = DocxTableFormat.Format;
		string text = (format.HasValue(123) ? format.FormatChangeAuthorName : string.Empty);
		DateTime dateTime = (format.HasValue(124) ? format.FormatChangeDateTime : DateTime.MinValue);
		foreach (WTableRow row in m_rows)
		{
			row.CharacterFormat.IsInsertRevision = check;
			if (!string.IsNullOrEmpty(text))
			{
				row.RowFormat.FormatChangeAuthorName = text;
			}
			if (dateTime != DateTime.MinValue)
			{
				row.RowFormat.FormatChangeDateTime = dateTime;
			}
		}
	}

	internal override void SetChangedCFormat(bool check)
	{
		foreach (WTableRow row in m_rows)
		{
			row.CharacterFormat.IsChangedFormat = check;
			foreach (WTableCell cell in row.Cells)
			{
				cell.CharacterFormat.IsChangedFormat = check;
			}
		}
	}

	internal override void SetChangedPFormat(bool check)
	{
		foreach (WTableRow row in m_rows)
		{
			row.RowFormat.IsChangedFormat = check;
		}
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("rows", Rows);
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", "Table");
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new TableLayoutInfo(this);
		IEntity nextSibling = base.NextSibling;
		if (nextSibling is WParagraph)
		{
			m_layoutInfo.IsPageBreakItem = (nextSibling as WParagraph).ParagraphFormat.PageBreakBefore;
		}
		else if (nextSibling is WTable && (nextSibling as WTable).Rows.Count > 0 && (nextSibling as WTable).Rows[0].Cells.Count > 0 && (nextSibling as WTable).Rows[0].Cells[0].Paragraphs.Count > 0)
		{
			m_layoutInfo.IsPageBreakItem = (nextSibling as WTable).Rows[0].Cells[0].Paragraphs[0].ParagraphFormat.PageBreakBefore;
		}
		if (Rows.Count < 1 || IsHiddenTable(this))
		{
			m_layoutInfo.IsSkip = true;
		}
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (m_rows == null || m_rows.Count <= 0)
		{
			return;
		}
		int count = m_rows.Count;
		for (int i = 0; i < count; i++)
		{
			m_rows[i].InitLayoutInfo(entity, ref isLastTOCEntry);
			if (isLastTOCEntry)
			{
				break;
			}
		}
	}

	internal bool IsHiddenRow(int rowIndex, WTable table)
	{
		bool result = false;
		if (table.Rows[rowIndex].RowFormat.Hidden || table.Rows[rowIndex].CharacterFormat.Hidden)
		{
			for (int i = 0; i < table.Rows[rowIndex].Cells.Count; i++)
			{
				for (int j = 0; j < table.Rows[rowIndex].Cells[i].ChildEntities.Count; j++)
				{
					if (table.Rows[rowIndex].Cells[i].ChildEntities[j] is WParagraph)
					{
						result = (IsHiddenParagraph(table.Rows[rowIndex].Cells[i].ChildEntities[j] as WParagraph) ? true : false);
					}
					else if (table.Rows[rowIndex].Cells[i].ChildEntities[j] is WTable)
					{
						result = (IsHiddenTable(table.Rows[rowIndex].Cells[i].ChildEntities[j] as WTable) ? true : false);
					}
				}
			}
		}
		return result;
	}

	internal bool IsHiddenParagraph(WParagraph paragraph)
	{
		bool result = false;
		for (int i = 0; i < paragraph.ChildEntities.Count; i++)
		{
			if ((paragraph.ChildEntities[i] is InlineContentControl) ? (paragraph.ChildEntities[i] as InlineContentControl).IsHidden() : (paragraph.ChildEntities[i] as ParagraphItem).ParaItemCharFormat.Hidden)
			{
				result = true;
				continue;
			}
			result = false;
			break;
		}
		if (paragraph.ChildEntities.Count == 0 && paragraph.BreakCharacterFormat.Hidden)
		{
			return true;
		}
		return result;
	}

	internal bool IsHiddenTable(WTable table)
	{
		bool result = false;
		for (int i = 0; i < table.Rows.Count; i++)
		{
			if (IsHiddenRow(i, table))
			{
				result = true;
				continue;
			}
			return false;
		}
		return result;
	}

	IWidgetContainer ITableWidget.GetCellWidget(int row, int column)
	{
		return m_rows[row].Cells[column];
	}

	IWidget ITableWidget.GetRowWidget(int row)
	{
		return m_rows[row];
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}

	internal override void Compare(WordDocument orgDoc)
	{
		CompareTable(orgDoc);
	}

	internal void Compare(WTextBody originalTextBody)
	{
		CompareTable(originalTextBody.Document, originalTextBody);
	}

	internal void CompareTable(WordDocument orgDoc, WTextBody orgTextbody = null)
	{
		List<WTable> tables = orgDoc.Comparison.Tables;
		if (tables.Count > 0)
		{
			WTable maxRowMatchedTable = null;
			List<string> matchedTableAndIndexes = GetMatchedTableAndIndexes(tables, ref maxRowMatchedTable);
			if (maxRowMatchedTable != null)
			{
				base.Document.Comparison.InsertAndDeleteUnmatchedItems(orgDoc, maxRowMatchedTable, this, orgTextbody);
				ApplyMatchedTableRevision(maxRowMatchedTable, matchedTableAndIndexes);
				base.Document.Comparison.MoveCurrPosition(orgDoc, maxRowMatchedTable, this, orgTextbody);
				orgDoc.Comparison.RemoveFromDocCollection(maxRowMatchedTable);
			}
		}
	}

	internal bool CompareTableFormattings(WTable table)
	{
		if (table == null)
		{
			return false;
		}
		if (!TableFormat.Compare(table.TableFormat))
		{
			return false;
		}
		if (StyleName != table.StyleName)
		{
			return false;
		}
		return true;
	}

	private List<string> GetMatchedTableAndIndexes(List<WTable> orgDocTables, ref WTable maxRowMatchedTable)
	{
		maxRowMatchedTable = null;
		int num = 0;
		List<string> list = new List<string>();
		foreach (WTable orgDocTable in orgDocTables)
		{
			List<string> longestMatchedIndexes = GetLongestMatchedIndexes(orgDocTable, this);
			int num2 = 0;
			foreach (string item in longestMatchedIndexes)
			{
				string[] array = item.Split(',');
				num2 += int.Parse(array[0]);
			}
			if (num < num2)
			{
				maxRowMatchedTable = orgDocTable;
				list.Clear();
				list.AddRange(longestMatchedIndexes);
				num = num2;
			}
		}
		return list;
	}

	private List<string> GetLongestMatchedIndexes(WTable orgTable, WTable revTable)
	{
		List<string> allMatchedIndex = GetAllMatchedIndex(orgTable, revTable);
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		int num5 = -1;
		int num6 = 0;
		do
		{
			int num7 = 0;
			num = -1;
			for (int i = 0; i < allMatchedIndex.Count - num6; i++)
			{
				string[] array = allMatchedIndex[i].Split(',');
				int num8 = int.Parse(array[0]);
				int num9 = int.Parse(array[1]);
				int num10 = int.Parse(array[2]);
				if ((num8 > num7 || (num7 + num2 - 1 < num9 && num7 + num3 - 1 < num10)) && (num6 == 0 || (num9 < num4 && num10 < num5)))
				{
					num7 = num8;
					num = i;
					num2 = num9;
					num3 = num10;
				}
			}
			if (num != -1)
			{
				allMatchedIndex.RemoveRange(num + 1, allMatchedIndex.Count - (num + 1 + num6));
				num4 = num2;
				num5 = num3;
				num6++;
			}
		}
		while (num != -1);
		allMatchedIndex.RemoveRange(0, allMatchedIndex.Count - num6);
		return allMatchedIndex;
	}

	private List<string> GetAllMatchedIndex(WTable orgTable, WTable revTable)
	{
		List<string> list = new List<string>();
		int num = 0;
		int num2 = -1;
		int num3 = -1;
		for (int i = 0; i < orgTable.Rows.Count; i++)
		{
			for (int j = 0; j < revTable.Rows.Count; j++)
			{
				WTableRow tableRow = orgTable.Rows[i];
				if (revTable.Rows[j].CompareRowText(tableRow))
				{
					if (num == 0)
					{
						num2 = i;
						num3 = j;
					}
					i++;
					num++;
					if (i >= orgTable.Rows.Count)
					{
						break;
					}
				}
				else if (num > 0)
				{
					list.Add(num + "," + num2 + "," + num3);
					i -= num;
					num = 0;
					num2 = -1;
					num3 = -1;
				}
			}
			if (num > 0)
			{
				list.Add(num + "," + num2 + "," + num3);
				i -= num;
				num = 0;
				num2 = -1;
				num3 = -1;
			}
		}
		return list;
	}

	private void ApplyMatchedTableRevision(WTable orgDocMatchedTable, List<string> matchedTableRowIndexes)
	{
		WordDocument document = orgDocMatchedTable.Document;
		bool isComparing = base.Document.IsComparing;
		bool isComparing2 = document.IsComparing;
		base.Document.IsComparing = true;
		document.IsComparing = true;
		int num = 0;
		int num2 = -1;
		int num3 = -1;
		WTableStyle style = null;
		if (!string.IsNullOrEmpty(orgDocMatchedTable.StyleName) && !string.IsNullOrEmpty(StyleName) && orgDocMatchedTable.StyleName == StyleName)
		{
			style = orgDocMatchedTable.Document.Styles.FindByName(StyleName) as WTableStyle;
		}
		CompareTableFormat(orgDocMatchedTable);
		foreach (string matchedTableRowIndex in matchedTableRowIndexes)
		{
			string[] array = matchedTableRowIndex.Split(',');
			int num4 = int.Parse(array[0]);
			int num5 = int.Parse(array[1]) + num;
			int num6 = int.Parse(array[2]);
			int num7 = 0;
			int num8 = num6;
			for (int i = num5; i < num5 + num4; i++)
			{
				WTableRow wTableRow = orgDocMatchedTable.Rows[i];
				WTableRow wTableRow2 = Rows[num8];
				for (int j = 0; j < wTableRow.Cells.Count; j++)
				{
					WTableCell wTableCell = wTableRow.Cells[j];
					WTableCell wTableCell2 = wTableRow2.Cells[j];
					wTableCell.Document.Comparison.IsComparingMatchedCells = true;
					wTableCell2.Compare(wTableCell);
					wTableCell.Document.Comparison.IsComparingMatchedCells = false;
					CompareCellFormat(wTableCell, wTableCell2, style, orgDocMatchedTable);
				}
				CompareRowFormat(wTableRow, wTableRow2, style);
				num8++;
			}
			for (int k = num2 + 1; k < num5; k++)
			{
				orgDocMatchedTable.Rows[k].AddDelMark();
				num2++;
			}
			for (int num9 = num6 - 1; num9 >= num3 + 1; num9--)
			{
				WTableRow wTableRow3 = Rows[num9];
				orgDocMatchedTable.Rows.Insert(num2 + 1, wTableRow3.Clone());
				orgDocMatchedTable.Rows[num2 + 1].AddInsMark();
				num++;
				num7++;
			}
			num2 += num4 + num7;
			num3 += num4 + num7;
		}
		for (int l = num2 + 1; l < orgDocMatchedTable.Rows.Count; l++)
		{
			orgDocMatchedTable.Rows[l].AddDelMark();
		}
		for (int m = num3 + 1; m < Rows.Count; m++)
		{
			WTableRow wTableRow4 = Rows[m];
			orgDocMatchedTable.Rows.Add(wTableRow4.Clone());
			orgDocMatchedTable.LastRow.AddInsMark();
		}
		base.Document.IsComparing = isComparing;
		document.IsComparing = isComparing2;
	}

	private void CompareTableFormat(WTable orgTable)
	{
		if (base.Document.ComparisonOptions != null && !base.Document.ComparisonOptions.DetectFormatChanges)
		{
			orgTable.TableFormat.CompareProperties(TableFormat);
			if (!string.IsNullOrEmpty(StyleName))
			{
				orgTable.ApplyStyle(StyleName);
				orgTable.DocxTableFormat.StyleName = StyleName;
			}
		}
		else
		{
			if (orgTable.CompareTableFormattings(this))
			{
				return;
			}
			orgTable.TableFormat.CompareProperties(TableFormat);
			WTableStyle wTableStyle = null;
			if (!string.IsNullOrEmpty(orgTable.StyleName) && !string.IsNullOrEmpty(StyleName) && orgTable.StyleName == StyleName)
			{
				wTableStyle = orgTable.Document.Styles.FindByName(StyleName) as WTableStyle;
			}
			if (wTableStyle != null && wTableStyle.TableProperties.OldPropertiesHash.Count > 0)
			{
				foreach (KeyValuePair<int, object> item in wTableStyle.TableProperties.OldPropertiesHash)
				{
					if (!orgTable.TableFormat.OldPropertiesHash.ContainsKey(item.Key))
					{
						orgTable.TableFormat.OldPropertiesHash.Add(item.Key, item.Value);
					}
				}
			}
			else if (wTableStyle == null && !string.IsNullOrEmpty(orgTable.StyleName) && !string.IsNullOrEmpty(StyleName))
			{
				orgTable.TrackTblFormat.StyleName = orgTable.StyleName;
				orgTable.ApplyStyle(StyleName);
			}
			else if (!string.IsNullOrEmpty(orgTable.StyleName) && string.IsNullOrEmpty(StyleName))
			{
				orgTable.TrackTblFormat.StyleName = orgTable.StyleName;
				orgTable.m_style = null;
			}
			else if (string.IsNullOrEmpty(orgTable.StyleName) && !string.IsNullOrEmpty(StyleName))
			{
				orgTable.ApplyStyle(StyleName);
				orgTable.DocxTableFormat.StyleName = StyleName;
			}
			orgTable.TableFormat.FormatChangeAuthorName = base.Document.m_authorName;
			orgTable.TableFormat.FormatChangeDateTime = base.Document.m_dateTime;
			orgTable.Document.UpdateTableRevision(orgTable);
		}
	}

	private void CompareCellFormat(WTableCell orgTableCell, WTableCell revTableCell, WTableStyle style, WTable orgDocMatchedTable)
	{
		if (base.Document.ComparisonOptions != null && !base.Document.ComparisonOptions.DetectFormatChanges)
		{
			orgTableCell.CellFormat.CompareProperties(revTableCell.CellFormat);
		}
		else
		{
			if (orgDocMatchedTable.DocxTableFormat.Format.Revisions.Count <= 0 && orgTableCell.CellFormat.Compare(revTableCell.CellFormat))
			{
				return;
			}
			orgTableCell.CellFormat.CompareProperties(revTableCell.CellFormat);
			if (style != null && style.CellProperties.OldPropertiesHash.Count > 0)
			{
				foreach (KeyValuePair<int, object> item in style.CellProperties.OldPropertiesHash)
				{
					if (!orgTableCell.CellFormat.OldPropertiesHash.ContainsKey(item.Key))
					{
						orgTableCell.CellFormat.OldPropertiesHash.Add(item.Key, item.Value);
					}
				}
			}
			orgTableCell.CellFormat.IsChangedFormat = true;
			orgTableCell.CellFormat.FormatChangeAuthorName = base.Document.m_authorName;
			orgTableCell.CellFormat.FormatChangeDateTime = base.Document.m_dateTime;
			orgTableCell.Document.UpdateCellFormatRevision(orgTableCell);
		}
	}

	private void CompareRowFormat(WTableRow orgTableRow, WTableRow revTableRow, WTableStyle style)
	{
		if (base.Document.ComparisonOptions != null && !base.Document.ComparisonOptions.DetectFormatChanges)
		{
			orgTableRow.RowFormat.CompareProperties(revTableRow.RowFormat);
		}
		else
		{
			if (orgTableRow.RowFormat.Compare(revTableRow.RowFormat))
			{
				return;
			}
			orgTableRow.RowFormat.CompareProperties(revTableRow.RowFormat);
			if (style != null && style.RowProperties.OldPropertiesHash.Count > 0)
			{
				foreach (KeyValuePair<int, object> item in style.RowProperties.OldPropertiesHash)
				{
					if (!orgTableRow.RowFormat.OldPropertiesHash.ContainsKey(item.Key))
					{
						orgTableRow.RowFormat.OldPropertiesHash.Add(item.Key, item.Value);
					}
				}
			}
			orgTableRow.RowFormat.IsChangedFormat = true;
			orgTableRow.RowFormat.FormatChangeAuthorName = base.Document.m_authorName;
			orgTableRow.RowFormat.FormatChangeDateTime = base.Document.m_dateTime;
			orgTableRow.Document.UpdateRowFormatRevision(orgTableRow.RowFormat);
		}
	}

	internal StringBuilder GetAsString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('\a');
		foreach (WTableRow row in Rows)
		{
			stringBuilder.Append(row.ComparisonText);
		}
		stringBuilder.Append('\a');
		return stringBuilder;
	}

	internal override void AddDelMark()
	{
		for (int i = 0; i < Rows.Count; i++)
		{
			Rows[i].AddDelMark();
		}
		base.Document.Comparison.RemoveFromDocCollection(this);
	}

	internal override void AddInsMark()
	{
		foreach (WTableRow row in m_rows)
		{
			row.AddInsMark();
		}
	}

	internal void RemoveDeleteRevision()
	{
		foreach (WTableRow row in m_rows)
		{
			row.CharacterFormat.IsDeleteRevision = false;
			row.RemoveDeleteRevision();
		}
	}

	internal void RemoveInsertRevision()
	{
		foreach (WTableRow row in m_rows)
		{
			row.CharacterFormat.IsInsertRevision = false;
			row.RemoveInsertRevision();
		}
	}
}
