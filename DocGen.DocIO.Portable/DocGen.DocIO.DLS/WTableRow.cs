using System;
using System.Collections.Generic;
using System.Text;
using DocGen.DocIO.DLS.XML;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class WTableRow : WidgetBase, ICompositeEntity, IEntity, IWidget
{
	private WCellCollection m_cells;

	private RowFormat m_tableFormat;

	private WCharacterFormat m_charFormat;

	private TableRowHeightType m_heightType;

	private byte[] m_internalData;

	internal RowFormat m_trackRowFormat;

	private byte m_bFlags;

	private RowContentControl m_RowContentControl;

	private string m_ComparisonText;

	public EntityCollection ChildEntities => m_cells;

	public override EntityType EntityType => EntityType.TableRow;

	public WCellCollection Cells
	{
		get
		{
			return m_cells;
		}
		set
		{
			m_cells = value;
		}
	}

	public TableRowHeightType HeightType
	{
		get
		{
			return m_heightType;
		}
		set
		{
			m_heightType = value;
		}
	}

	public RowFormat RowFormat => m_tableFormat;

	public float Height
	{
		get
		{
			return m_tableFormat.Height;
		}
		set
		{
			m_tableFormat.Height = value;
		}
	}

	public bool IsHeader
	{
		get
		{
			return m_tableFormat.IsHeaderRow;
		}
		set
		{
			m_tableFormat.IsHeaderRow = value;
		}
	}

	internal WTable OwnerTable => base.Owner as WTable;

	internal byte[] DataArray
	{
		get
		{
			return m_internalData;
		}
		set
		{
			m_internalData = value;
		}
	}

	internal WCharacterFormat CharacterFormat => m_charFormat;

	internal RowFormat TrackRowFormat
	{
		get
		{
			if (m_trackRowFormat == null)
			{
				m_trackRowFormat = new RowFormat();
			}
			return m_trackRowFormat;
		}
	}

	internal bool IsDeleteRevision
	{
		get
		{
			if ((m_bFlags & 1) == 0)
			{
				return CharacterFormat.IsDeleteRevision;
			}
			return true;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
			CharacterFormat.IsDeleteRevision = value;
		}
	}

	internal bool IsInsertRevision
	{
		get
		{
			if ((m_bFlags & 2) >> 1 == 0)
			{
				return CharacterFormat.IsInsertRevision;
			}
			return true;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFDu) | ((value ? 1u : 0u) << 1));
			CharacterFormat.IsInsertRevision = value;
		}
	}

	internal bool HasTblPrEx
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

	internal RowContentControl ContentControl
	{
		get
		{
			return m_RowContentControl;
		}
		set
		{
			m_RowContentControl = value;
		}
	}

	internal string ComparisonText
	{
		get
		{
			if (m_ComparisonText == null)
			{
				m_ComparisonText = GetWordComparisonText();
			}
			return m_ComparisonText;
		}
	}

	internal bool IsKeepWithNext
	{
		get
		{
			WParagraph paragraph = null;
			GetFirstParagraphOfRow(ref paragraph);
			return paragraph?.ParagraphFormat.KeepFollow ?? false;
		}
	}

	public WTableRow(IWordDocument document)
		: base((WordDocument)document, null)
	{
		m_cells = new WCellCollection(this);
		m_charFormat = new WCharacterFormat(base.Document);
		m_charFormat.SetOwner(this);
		m_tableFormat = new RowFormat();
		m_tableFormat.SetOwner(this);
	}

	public new WTableRow Clone()
	{
		return (WTableRow)CloneImpl();
	}

	public WTableCell AddCell()
	{
		return AddCell(isCopyFormat: true);
	}

	public WTableCell AddCell(bool isCopyFormat)
	{
		if (Cells.Count > 62)
		{
			throw new ArgumentException("This exceeds the maximum number of cells.");
		}
		WTableCell wTableCell = new WTableCell(base.Document);
		WTableRow wTableRow = base.PreviousSibling as WTableRow;
		WTableCell wTableCell2 = null;
		if (wTableRow != null && wTableRow.Cells.Count > Cells.Count)
		{
			wTableCell2 = wTableRow.Cells[Cells.Count];
		}
		if (isCopyFormat)
		{
			if (wTableCell2 != null)
			{
				wTableCell.CellFormat.ImportContainer(wTableCell2.CellFormat);
				wTableCell.Width = wTableCell2.Width;
			}
			else
			{
				wTableCell.CellFormat.ImportContainer(m_tableFormat);
			}
		}
		Cells.Add(wTableCell);
		return wTableCell;
	}

	public int GetRowIndex()
	{
		return GetIndexInOwnerCollection();
	}

	internal bool IsCellWidthZero()
	{
		foreach (WTableCell cell in Cells)
		{
			if (!cell.IsCellWidthZero())
			{
				return false;
			}
		}
		if ((RowFormat.GridBeforeWidth.WidthType < FtsWidth.Percentage || RowFormat.GridBeforeWidth.Width == 0f) && RowFormat.BeforeWidth == 0f && (RowFormat.GridAfterWidth.WidthType < FtsWidth.Percentage || RowFormat.GridAfterWidth.Width == 0f))
		{
			return RowFormat.AfterWidth == 0f;
		}
		return false;
	}

	internal void UpdateCellWidthByPartitioning(float tableWidth, bool isSkipToEqualPartition)
	{
		if (isSkipToEqualPartition)
		{
			float num = tableWidth;
			List<int> list = new List<int>();
			foreach (WTableCell cell in Cells)
			{
				if (cell.PreferredWidth.WidthType == FtsWidth.Percentage && cell.PreferredWidth.Width > 0f && cell.PreferredWidth.Width <= 100f)
				{
					cell.CellFormat.CellWidth = cell.PreferredWidth.Width * tableWidth / 100f;
					num -= cell.CellFormat.CellWidth;
				}
				else if (cell.PreferredWidth.WidthType == FtsWidth.Point && cell.PreferredWidth.Width > 0f)
				{
					cell.CellFormat.CellWidth = cell.PreferredWidth.Width;
					num -= cell.CellFormat.CellWidth;
				}
				else
				{
					list.Add(cell.Index);
				}
				foreach (WTable table in cell.Tables)
				{
					table.IsUpdateCellWidthByPartitioning = true;
					if (table.IsUpdateCellWidthByPartitioning)
					{
						table.UpdateTableGrid(isTableGridMissMatch: false, isgridafter: true);
					}
				}
			}
			num /= (float)list.Count;
			{
				foreach (int item in list)
				{
					Cells[item].CellFormat.CellWidth = ((num > 0f) ? num : 0f);
				}
				return;
			}
		}
		float num2 = (float)Math.Round(tableWidth * 20f / (float)Cells.Count);
		foreach (WTableCell cell2 in Cells)
		{
			cell2.CellFormat.CellWidth = num2 / 20f;
			foreach (WTable table2 in cell2.Tables)
			{
				table2.IsUpdateCellWidthByPartitioning = true;
				if (table2.IsUpdateCellWidthByPartitioning)
				{
					table2.UpdateTableGrid(isTableGridMissMatch: false, isgridafter: true);
				}
			}
		}
	}

	internal void UpdateUnDefinedCellWidth(float tableWidth)
	{
		List<WTableCell> list = new List<WTableCell>();
		float num = 0f;
		foreach (WTableCell cell in Cells)
		{
			if (cell.IsCellWidthZero())
			{
				list.Add(cell);
			}
			else
			{
				num = cell.CellFormat.CellWidth;
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		float num2 = tableWidth - num;
		if (num2 > 0f)
		{
			num2 /= (float)list.Count;
			{
				foreach (WTableCell item in list)
				{
					item.CellFormat.CellWidth = num2;
					foreach (WTable table in item.Tables)
					{
						table.IsUpdateCellWidthByPartitioning = true;
						if (table.IsUpdateCellWidthByPartitioning)
						{
							table.UpdateTableGrid(isTableGridMissMatch: false, isgridafter: true);
						}
					}
				}
				return;
			}
		}
		tableWidth -= (float)list.Count * 3f;
		foreach (WTableCell cell2 in Cells)
		{
			if (!cell2.IsCellWidthZero())
			{
				cell2.Width *= tableWidth / num;
			}
			else
			{
				cell2.CellFormat.CellWidth = 3f;
			}
		}
	}

	internal override void AddSelf()
	{
		foreach (WTableCell cell in Cells)
		{
			cell.AddSelf();
		}
	}

	protected override object CloneImpl()
	{
		WTableRow wTableRow = (WTableRow)base.CloneImpl();
		wTableRow.m_charFormat = new WCharacterFormat(base.Document);
		wTableRow.m_charFormat.ImportContainer(CharacterFormat);
		wTableRow.m_charFormat.SetOwner(wTableRow);
		wTableRow.m_tableFormat = new RowFormat(base.Document);
		wTableRow.m_tableFormat.ImportContainer(RowFormat);
		wTableRow.m_tableFormat.SetOwner(wTableRow);
		wTableRow.m_tableFormat.Paddings.ImportPaddings(m_tableFormat.Paddings);
		if (DataArray != null)
		{
			wTableRow.m_internalData = new byte[DataArray.Length];
			DataArray.CopyTo(wTableRow.m_internalData, 0);
		}
		wTableRow.m_cells = new WCellCollection(wTableRow);
		Cells.CloneTo(wTableRow.m_cells);
		return wTableRow;
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		base.CloneRelationsTo(doc, nextOwner);
		int i = 0;
		for (int count = ChildEntities.Count; i < count; i++)
		{
			Entity entity = ChildEntities[i];
			entity.CloneRelationsTo(doc, nextOwner);
			if (entity is WTableCell)
			{
				WTableCell wTableCell = entity as WTableCell;
				if (wTableCell.ChildEntities.Count > 0 && wTableCell.ChildEntities[wTableCell.ChildEntities.Count - 1] is WParagraph)
				{
					wTableCell.CharacterFormat.ImportContainer((wTableCell.ChildEntities[wTableCell.ChildEntities.Count - 1] as WParagraph).BreakCharacterFormat);
				}
			}
		}
	}

	private void CheckFormatOwner()
	{
		if (RowFormat.OwnerBase != this)
		{
			RowFormat.SetOwner(this);
		}
	}

	internal void OnInsertCell(int index, CellFormat cellFormat)
	{
		if (OwnerTable != null)
		{
			OwnerTable.IsTableGridUpdated = false;
			OwnerTable.IsTableGridVerified = false;
		}
	}

	internal void OnRemoveCell(int index)
	{
		if (OwnerTable != null)
		{
			OwnerTable.IsTableGridUpdated = false;
			OwnerTable.IsTableGridVerified = false;
		}
	}

	internal float GetWidthToResizeCells(float clientWidth)
	{
		float num = 0f;
		bool flag = false;
		for (int i = 0; i < Cells.Count; i++)
		{
			num += Cells[i].Width;
			foreach (WTable table in Cells[i].Tables)
			{
				if (table.PreferredTableWidth.WidthType == FtsWidth.Point && table.PreferredTableWidth.Width > 0f)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			num += ((RowFormat.BeforeWidth > 0f) ? RowFormat.BeforeWidth : GetGridBeforeAfter(RowFormat.GridBeforeWidth, clientWidth));
			return num + ((RowFormat.AfterWidth > 0f) ? RowFormat.AfterWidth : GetGridBeforeAfter(RowFormat.GridAfterWidth, clientWidth));
		}
		if (m_doc.IsDOCX() && (OwnerTable.IsAutoTableExceedsClientWidth() || (OwnerTable.TableFormat.IsAutoResized && OwnerTable.PreferredTableWidth.WidthType == FtsWidth.Percentage && OwnerTable.PreferredTableWidth.Width > 100f && !OwnerTable.IsInCell) || OwnerTable.IsHtmlTableExceedsClientWidth()))
		{
			return num;
		}
		return clientWidth;
	}

	private float GetGridBeforeAfter(PreferredWidthInfo widthInfo, float clientWidth)
	{
		float result = 0f;
		if (widthInfo.WidthType == FtsWidth.Point)
		{
			result = widthInfo.Width;
		}
		else if (widthInfo.WidthType == FtsWidth.Percentage)
		{
			result = clientWidth * widthInfo.Width / 100f;
		}
		return result;
	}

	internal float GetRowWidth()
	{
		float num = 0f;
		foreach (WTableCell cell in Cells)
		{
			num += cell.Width;
		}
		return num;
	}

	internal void GetRowWidth(ref float preferredWidth, float tableWidth)
	{
		foreach (WTableCell cell in Cells)
		{
			preferredWidth += cell.PreferredWidth.Width;
		}
		float num = 0f;
		float num2 = 0f;
		if (RowFormat.GridBeforeWidth.Width > 0f)
		{
			num = GetGridBeforeAfter(RowFormat.GridBeforeWidth, tableWidth);
		}
		if (RowFormat.GridAfterWidth.Width > 0f)
		{
			num2 = GetGridBeforeAfter(RowFormat.GridAfterWidth, tableWidth);
		}
		preferredWidth += num2 + num;
	}

	internal float GetRowPreferredWidth(float tableWidth)
	{
		float num = 0f;
		foreach (WTableCell cell in Cells)
		{
			int rowIndex = GetRowIndex();
			if (cell.CellFormat.VerticalMerge != CellMerge.Continue || (cell.CellFormat.VerticalMerge == CellMerge.Continue && rowIndex == 0))
			{
				if (cell.PreferredWidth.WidthType == FtsWidth.Point)
				{
					num += cell.PreferredWidth.Width * 20f;
				}
				else if (cell.PreferredWidth.WidthType == FtsWidth.Percentage)
				{
					num += tableWidth * cell.PreferredWidth.Width / 5f;
				}
			}
			else
			{
				if (rowIndex <= 0)
				{
					continue;
				}
				WTableCell verticalMergeStartCell = cell.GetVerticalMergeStartCell();
				if (verticalMergeStartCell != null)
				{
					if (verticalMergeStartCell.PreferredWidth.WidthType == FtsWidth.Point)
					{
						num += verticalMergeStartCell.PreferredWidth.Width * 20f;
					}
					else if (verticalMergeStartCell.PreferredWidth.WidthType == FtsWidth.Percentage)
					{
						num += tableWidth * verticalMergeStartCell.PreferredWidth.Width / 5f;
					}
				}
			}
		}
		return num;
	}

	internal bool IsAllCellsHasPointAndPerecentPrefWidth()
	{
		foreach (WTableCell cell in Cells)
		{
			if (cell.PreferredWidth.WidthType != FtsWidth.Percentage && cell.PreferredWidth.WidthType != FtsWidth.Point)
			{
				return false;
			}
		}
		return true;
	}

	internal float GetRowPreferredWidthFromPoint(int defaultPrefCellWidth)
	{
		float num = 0f;
		foreach (WTableCell cell in Cells)
		{
			int rowIndex = GetRowIndex();
			if (cell.CellFormat.VerticalMerge != CellMerge.Continue || (cell.CellFormat.VerticalMerge == CellMerge.Continue && rowIndex == 0))
			{
				if (cell.PreferredWidth.WidthType == FtsWidth.Point)
				{
					num += cell.PreferredWidth.Width;
				}
				else if (cell.PreferredWidth.WidthType == FtsWidth.None)
				{
					num += (float)defaultPrefCellWidth;
				}
			}
			else
			{
				if (rowIndex <= 0)
				{
					continue;
				}
				WTableCell verticalMergeStartCell = cell.GetVerticalMergeStartCell();
				if (verticalMergeStartCell != null)
				{
					if (verticalMergeStartCell.PreferredWidth.WidthType == FtsWidth.Point)
					{
						num += verticalMergeStartCell.PreferredWidth.Width;
					}
					else if (cell.PreferredWidth.WidthType == FtsWidth.None)
					{
						num += (float)defaultPrefCellWidth;
					}
				}
			}
		}
		return num;
	}

	internal void UpdateRowRevision(WordDocument document)
	{
		if (RowFormat.HasKey(122))
		{
			document.UpdateRowFormatRevision(RowFormat);
		}
		if (IsInsertRevision)
		{
			document.TableRowRevision(RevisionType.Insertions, this, null);
		}
		if (IsDeleteRevision)
		{
			document.TableRowRevision(RevisionType.Deletions, this, null);
		}
	}

	internal override void Close()
	{
		if (m_cells != null && m_cells.Count > 0)
		{
			int count = m_cells.Count;
			for (int i = 0; i < count; i++)
			{
				m_cells[i].Close();
			}
			m_cells.Close();
			m_cells = null;
		}
		if (m_tableFormat != null)
		{
			m_tableFormat.Close();
			m_tableFormat = null;
		}
		if (m_charFormat != null)
		{
			m_charFormat.Close();
			m_charFormat = null;
		}
		if (m_trackRowFormat != null)
		{
			m_trackRowFormat.Close();
			m_trackRowFormat = null;
		}
		if (m_RowContentControl != null)
		{
			m_RowContentControl.Close();
			m_RowContentControl = null;
		}
		if (m_internalData != null)
		{
			m_internalData = null;
		}
		base.Close();
	}

	internal bool CompareRowText(WTableRow tableRow)
	{
		if (Cells.Count == tableRow.Cells.Count && ComparisonText.Length == tableRow.ComparisonText.Length)
		{
			return ComparisonText.Equals(tableRow.ComparisonText);
		}
		return false;
	}

	internal StringBuilder GetAsString()
	{
		char value = '\u0005';
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(value);
		foreach (WTableCell cell in Cells)
		{
			stringBuilder.Append(cell.ComparisonText);
		}
		stringBuilder.Append(value);
		return stringBuilder;
	}

	internal string GetWordComparisonText()
	{
		byte[] bytes = Encoding.UTF8.GetBytes(GetAsString().ToString());
		return base.Document.Comparison.ConvertBytesAsHash(bytes);
	}

	internal List<WTableRow> GetMatchedRows(List<WTable> tables, bool withTableAscii, int startRowIndex, int startTableIndex)
	{
		string text = ComparisonText;
		List<WTableRow> list = new List<WTableRow>();
		for (int i = 0; i < tables.Count; i++)
		{
			WTable wTable = tables[i];
			if (wTable.Index < startTableIndex)
			{
				continue;
			}
			foreach (WTableRow row in wTable.Rows)
			{
				if (wTable.Index == startTableIndex && row.Index < startRowIndex)
				{
					continue;
				}
				string comparisonText = row.ComparisonText;
				if (withTableAscii)
				{
					if (string.Equals(comparisonText, text) && !string.IsNullOrEmpty(comparisonText.Replace("\a", "")))
					{
						list.Add(row);
					}
					continue;
				}
				comparisonText = comparisonText.Replace('\a', ' ');
				text = text.Replace('\a', ' ');
				if (string.Equals(comparisonText, text))
				{
					list.Add(row);
				}
			}
		}
		return list;
	}

	internal void AddDelMark()
	{
		IsDeleteRevision = true;
		CharacterFormat.AuthorName = base.Document.m_authorName;
		CharacterFormat.RevDateTime = base.Document.m_dateTime;
		UpdateRowRevision(base.Document);
		foreach (WTableCell cell in Cells)
		{
			foreach (TextBodyItem item in cell.Items)
			{
				item.AddDelMark();
			}
		}
	}

	internal void AddInsMark()
	{
		IsInsertRevision = true;
		CharacterFormat.AuthorName = base.Document.m_authorName;
		CharacterFormat.RevDateTime = base.Document.m_dateTime;
		UpdateRowRevision(base.Document);
		foreach (WTableCell cell in Cells)
		{
			foreach (TextBodyItem item in cell.Items)
			{
				item.AddInsMark();
			}
		}
	}

	internal void RemoveDeleteRevision()
	{
		IsDeleteRevision = false;
		CharacterFormat.IsDeleteRevision = false;
		foreach (WTableCell cell in Cells)
		{
			foreach (TextBodyItem item in cell.Items)
			{
				if (item is WParagraph)
				{
					(item as WParagraph).RemoveDelMark();
				}
				else if (item is WTable)
				{
					(item as WTable).RemoveDeleteRevision();
				}
			}
		}
	}

	internal void RemoveInsertRevision()
	{
		IsInsertRevision = false;
		CharacterFormat.IsInsertRevision = false;
		foreach (WTableCell cell in Cells)
		{
			foreach (TextBodyItem item in cell.Items)
			{
				if (item is WParagraph)
				{
					(item as WParagraph).RemoveInsMark();
				}
				else if (item is WTable)
				{
					(item as WTable).RemoveInsertRevision();
				}
			}
		}
	}

	protected override void InitXDLSHolder()
	{
		base.XDLSHolder.AddElement("cells", Cells);
		base.XDLSHolder.AddElement("character-format", CharacterFormat);
		base.XDLSHolder.AddElement("table-format", RowFormat);
	}

	protected override void WriteXmlContent(IXDLSContentWriter writer)
	{
		if (DataArray != null)
		{
			writer.WriteChildBinaryElement("internal-data", DataArray);
		}
	}

	protected override bool ReadXmlContent(IXDLSContentReader reader)
	{
		bool result = base.ReadXmlContent(reader);
		if (reader.TagName == "internal-data")
		{
			DataArray = reader.ReadChildBinaryElement();
			result = true;
		}
		return result;
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		if (IsHeader)
		{
			writer.WriteValue("IsHeader", IsHeader);
		}
		if (!m_tableFormat.HasSprms())
		{
			if (Height > 0f)
			{
				writer.WriteValue("RowHeight", Height);
			}
			writer.WriteValue("HeightType", HeightType);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		if (reader.HasAttribute("RowHeight"))
		{
			Height = reader.ReadFloat("RowHeight");
		}
		if (reader.HasAttribute("IsHeader"))
		{
			IsHeader = reader.ReadBoolean("IsHeader");
		}
		if (reader.HasAttribute("HeightType"))
		{
			HeightType = (TableRowHeightType)(object)reader.ReadEnum("HeightType", typeof(TableRowHeightType));
		}
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new RowLayoutInfo(HeightType == TableRowHeightType.Exactly && ((Height >= 0f) ? Height : (-1f * Height)) > 0f, Height);
		m_layoutInfo.IsVerticalText = true;
		m_layoutInfo.IsSkip = false;
		for (int i = 0; i < Cells.Count; i++)
		{
			if (Cells[i].CellFormat.TextDirection != TextDirection.VerticalBottomToTop && Cells[i].CellFormat.TextDirection != TextDirection.VerticalTopToBottom)
			{
				m_layoutInfo.IsVerticalText = false;
				break;
			}
		}
		if (!OwnerTable.IsInCell && !OwnerTable.TableFormat.WrapTextAround)
		{
			m_layoutInfo.IsKeepWithNext = IsKeepWithNext;
		}
		bool flag = false;
		for (int j = 0; j < Cells.Count; j++)
		{
			flag = Cells[j].CellFormat.HideMark && Cells[j].Paragraphs.Count == 1 && Cells[j].Tables.Count == 0 && !Cells[j].IsContainBlockContentControl() && (Cells[j].Paragraphs[0].Items.Count <= 0 || IsNeedToSkipParaMark(Cells[j].Paragraphs[0], Cells[j].Paragraphs[0].Items.Count));
			if (!flag)
			{
				break;
			}
		}
		if (flag)
		{
			m_layoutInfo.IsHiddenRow = true;
		}
		if (Cells.Count < 1)
		{
			m_layoutInfo.IsSkip = true;
		}
		if (IsDeleteRevision)
		{
			m_layoutInfo.IsSkip = true;
		}
	}

	private bool IsNeedToSkipParaMark(WParagraph paragraph, int paraItemCount)
	{
		for (int i = 0; i < paraItemCount; i++)
		{
			if (paragraph.ChildEntities[i] is ParagraphItem paragraphItem && !(paragraphItem is BookmarkStart) && !(paragraphItem is BookmarkEnd) && (!(paragraphItem is WTextRange) || !((paragraphItem as WTextRange).Text == "")))
			{
				return false;
			}
		}
		return true;
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (m_cells == null || m_cells.Count <= 0)
		{
			return;
		}
		int count = m_cells.Count;
		for (int i = 0; i < count; i++)
		{
			m_cells[i].InitLayoutInfo(entity, ref isLastTOCEntry);
			if (isLastTOCEntry)
			{
				break;
			}
		}
	}

	private void GetFirstParagraphOfRow(ref WParagraph paragraph)
	{
		if (ChildEntities.Count > 0 && Cells[0].ChildEntities.Count > 0)
		{
			if (Cells[0].ChildEntities[0] is WTable && (Cells[0].ChildEntities[0] as WTable).Rows.Count > 0)
			{
				(Cells[0].ChildEntities[0] as WTable).Rows[0].GetFirstParagraphOfRow(ref paragraph);
			}
			if (Cells[0].ChildEntities[0] is WParagraph)
			{
				paragraph = Cells[0].ChildEntities[0] as WParagraph;
			}
		}
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}
}
