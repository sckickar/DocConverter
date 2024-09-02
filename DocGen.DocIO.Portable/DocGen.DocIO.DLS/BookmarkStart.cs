using DocGen.DocIO.DLS.XML;
using DocGen.DocIO.Rendering;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class BookmarkStart : ParagraphItem, ILeafWidget, IWidget
{
	private string m_strName = string.Empty;

	private short m_colFirst = -1;

	private short m_colLast = -1;

	private string m_displacedByCustomXml;

	private byte m_bFlags;

	public override EntityType EntityType => EntityType.BookmarkStart;

	public string Name => m_strName;

	internal bool IsCellGroupBkmk
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

	internal bool IsDetached
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

	internal short ColumnFirst
	{
		get
		{
			return m_colFirst;
		}
		set
		{
			m_colFirst = value;
		}
	}

	internal short ColumnLast
	{
		get
		{
			return m_colLast;
		}
		set
		{
			m_colLast = value;
		}
	}

	internal string DisplacedByCustomXml
	{
		get
		{
			return m_displacedByCustomXml;
		}
		set
		{
			m_displacedByCustomXml = value;
		}
	}

	internal BookmarkStart(WordDocument doc)
		: this(doc, string.Empty)
	{
	}

	public BookmarkStart(IWordDocument doc, string name)
		: base((WordDocument)doc)
	{
		SetName(name);
	}

	internal void SetName(string name)
	{
		m_strName = name.Replace('-', '_');
	}

	internal override void AttachToParagraph(WParagraph owner, int itemPos)
	{
		base.AttachToParagraph(owner, itemPos);
		if (!base.DeepDetached)
		{
			base.Document.Bookmarks.AttachBookmarkStart(this);
			IsDetached = false;
		}
		else
		{
			IsDetached = true;
		}
	}

	internal override void Detach()
	{
		base.Detach();
		if (!base.DeepDetached)
		{
			BookmarkCollection bookmarks = base.Document.Bookmarks;
			Bookmark bookmark = bookmarks.FindByName(Name);
			if (bookmark != null)
			{
				bookmark.SetStart(null);
				bookmarks.Remove(bookmark);
			}
		}
	}

	internal override void AttachToDocument()
	{
		if (IsDetached)
		{
			base.Document.Bookmarks.AttachBookmarkStart(this);
			IsDetached = false;
		}
	}

	protected override object CloneImpl()
	{
		BookmarkStart obj = (BookmarkStart)base.CloneImpl();
		obj.IsDetached = true;
		return obj;
	}

	internal override void Close()
	{
		if (!string.IsNullOrEmpty(m_strName))
		{
			m_strName = string.Empty;
		}
		base.Close();
	}

	internal void GetBkmkContentInDiffCell(WTable bkmkTable, int startTableRowIndex, int endTableRowIndex, int startCellIndex, int endCellIndex, WTextBody textBody)
	{
		WTable wTable = bkmkTable.Clone();
		while (startTableRowIndex > 0)
		{
			wTable.Rows[0].RemoveSelf();
			startTableRowIndex--;
			endTableRowIndex--;
		}
		while (wTable.Rows.Count > endTableRowIndex + 1)
		{
			wTable.Rows[endTableRowIndex + 1].RemoveSelf();
		}
		int num = startCellIndex;
		int maximumCellCount = GetMaximumCellCount(wTable, endCellIndex);
		for (int i = startTableRowIndex; i <= endTableRowIndex; i++)
		{
			startCellIndex = num;
			endCellIndex = ((maximumCellCount < wTable.Rows[i].Cells.Count) ? maximumCellCount : (wTable.Rows[i].Cells.Count - 1));
			WTableCell bkmkStartCell = wTable.Rows[i].Cells[startCellIndex];
			WTableCell bkmkEndCell = wTable.Rows[i].Cells[endCellIndex];
			GetCellRangeForMergedCells(bkmkStartCell, bkmkEndCell, wTable, wTable, i, i, ref startCellIndex, ref endCellIndex);
			while (startCellIndex > 0)
			{
				wTable.Rows[i].Cells[0].RemoveSelf();
				startCellIndex--;
				endCellIndex--;
			}
			while (wTable.Rows[i].Cells.Count > endCellIndex + 1)
			{
				wTable.Rows[i].Cells[endCellIndex + 1].RemoveSelf();
			}
		}
		WTableCell wTableCell = wTable.Rows[startTableRowIndex].Cells[startCellIndex];
		WTableCell wTableCell2 = wTable.Rows[endTableRowIndex].Cells.LastItem as WTableCell;
		if (wTableCell == wTableCell2)
		{
			wTableCell = RemoveBkmkStartEndFromCell(wTableCell);
			{
				foreach (TextBodyItem item in wTableCell.Items)
				{
					textBody.ChildEntities.AddToInnerList(item);
				}
				return;
			}
		}
		for (int j = startTableRowIndex; j <= endTableRowIndex; j++)
		{
			for (int k = startCellIndex; k < wTable.Rows[j].Cells.Count && k <= endCellIndex; k++)
			{
				RemoveBkmkStartEndFromCell(wTable.Rows[j].Cells[k]);
			}
		}
		textBody.Items.AddToInnerList(wTable);
	}

	private int GetMaximumCellCount(WTable Table, int bkmkEndCellIndex)
	{
		if (ColumnFirst == -1 && ColumnLast == -1)
		{
			for (int i = 0; i < Table.Rows.Count; i++)
			{
				WTableRow wTableRow = Table.Rows[i];
				if (bkmkEndCellIndex < wTableRow.Cells.Count - 1)
				{
					bkmkEndCellIndex = wTableRow.Cells.Count - 1;
				}
			}
		}
		return bkmkEndCellIndex;
	}

	private WTableCell RemoveBkmkStartEndFromCell(WTableCell tableCell)
	{
		foreach (WParagraph paragraph in tableCell.Paragraphs)
		{
			for (int num = paragraph.Items.Count - 1; num >= 0; num--)
			{
				if (paragraph.Items[num] is BookmarkStart && (paragraph.Items[num] as BookmarkStart).Name == Name)
				{
					paragraph.Items.RemoveFromInnerList(num);
				}
				else if (paragraph.Items[num] is BookmarkEnd && (paragraph.Items[num] as BookmarkEnd).Name == Name)
				{
					paragraph.Items.RemoveAt(num);
				}
			}
		}
		return tableCell;
	}

	internal void GetBookmarkStartAndEndCell(WTableCell bkmkStartCell, WTableCell bkmkEndCell, WTableCell tempBkmkEndCell, WTable bkmkStartTable, WTable bkmkEndTable, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd, int startTableRowIndex, ref int endTableRowIndex, ref int bkmkStartCellIndex, ref int bkmkEndCellIndex)
	{
		int columnFirst = bkmkStart.ColumnFirst;
		int columnLast = bkmkStart.ColumnLast;
		bool flag = columnFirst >= 0 && columnFirst < bkmkStartTable.Rows[startTableRowIndex].Cells.Count;
		bool flag2 = columnLast >= 0 && columnLast < bkmkEndTable.Rows[endTableRowIndex].Cells.Count;
		bool flag3 = columnFirst > columnLast;
		bool flag4 = columnFirst < 0 || columnLast < 0;
		if (flag && flag2 && !flag3)
		{
			bkmkStartCellIndex = columnFirst;
			bkmkEndCellIndex = columnLast;
		}
		else if (flag && !flag2 && !flag3)
		{
			bkmkStartCellIndex = bkmkStart.ColumnFirst;
			bkmkEndCellIndex = bkmkEndTable.Rows[endTableRowIndex].Cells.Count - 1;
		}
		else if (flag3 || flag4 || !flag || !flag2)
		{
			bkmkStartCellIndex = 0;
			bkmkEndCellIndex = bkmkEndTable.Rows[endTableRowIndex].Cells.Count - 1;
		}
		bkmkStartCell = bkmkStartTable.Rows[startTableRowIndex].Cells[bkmkStartCellIndex];
		bkmkEndCell = bkmkEndTable.Rows[endTableRowIndex].Cells[bkmkEndCellIndex];
		GetCellRangeForMergedCells(bkmkStartCell, bkmkEndCell, bkmkStartTable, bkmkEndTable, startTableRowIndex, endTableRowIndex, ref bkmkStartCellIndex, ref bkmkEndCellIndex);
	}

	private void GetCellRangeForMergedCells(WTableCell bkmkStartCell, WTableCell bkmkEndCell, WTable bkmkStartTable, WTable bkmkEndTable, int startTableRowIndex, int endTableRowIndex, ref int bkmkStartCellIndex, ref int bkmkEndCellIndex)
	{
		if (bkmkStartCell.CellFormat.HorizontalMerge == CellMerge.Continue)
		{
			while (bkmkStartCell.CellFormat.HorizontalMerge == CellMerge.Continue)
			{
				bkmkStartCellIndex--;
				bkmkStartCell = bkmkStartTable.Rows[startTableRowIndex].Cells[bkmkStartCellIndex];
			}
		}
		if (bkmkEndCell.CellFormat.HorizontalMerge != CellMerge.Continue)
		{
			return;
		}
		while (bkmkEndCell.CellFormat.HorizontalMerge == CellMerge.Continue)
		{
			bkmkEndCellIndex++;
			if (bkmkEndCellIndex >= bkmkEndTable.Rows[endTableRowIndex].Cells.Count)
			{
				break;
			}
			bkmkEndCell = bkmkEndTable.Rows[endTableRowIndex].Cells[bkmkEndCellIndex];
		}
		bkmkEndCellIndex--;
		bkmkEndCell = bkmkEndTable.Rows[endTableRowIndex].Cells[bkmkEndCellIndex];
	}

	internal bool IsBookmarkEndAtSameCell(WTableCell bkmkStartCell, WTable bkmkStartTable, ref WTable bkmkEndTable, ref int bkmkEndRowIndex)
	{
		int num = bkmkEndRowIndex;
		WTable wTable = bkmkEndTable;
		WTableCell wTableCell = wTable.Owner as WTableCell;
		while (wTable.IsInCell)
		{
			wTableCell = wTable.Owner as WTableCell;
			if (wTableCell == null || wTableCell.Owner.Owner as WTable == bkmkStartTable)
			{
				break;
			}
			num = wTableCell.OwnerRow.Index;
			wTable = wTableCell.Owner.Owner as WTable;
		}
		int num2;
		if (bkmkEndTable != bkmkStartTable)
		{
			num2 = ((bkmkStartCell == wTableCell) ? 1 : 0);
			if (num2 != 0)
			{
				bkmkEndTable = wTable;
				bkmkEndRowIndex = num;
			}
		}
		else
		{
			num2 = 0;
		}
		return (byte)num2 != 0;
	}

	protected override void WriteXmlAttributes(IXDLSAttributeWriter writer)
	{
		base.WriteXmlAttributes(writer);
		writer.WriteValue("type", ParagraphItemType.BookmarkStart);
		writer.WriteValue("BookmarkName", Name);
		if (IsCellGroupBkmk)
		{
			writer.WriteValue("IsCellGroupBkmk", IsCellGroupBkmk);
		}
	}

	protected override void ReadXmlAttributes(IXDLSAttributeReader reader)
	{
		base.ReadXmlAttributes(reader);
		m_strName = reader.ReadString("BookmarkName");
		base.Document.Bookmarks.AttachBookmarkStart(this);
		if (reader.HasAttribute("IsCellGroupBkmk"))
		{
			IsCellGroupBkmk = reader.ReadBoolean("IsCellGroupBkmk");
		}
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo(ChildrenLayoutDirection.Horizontal);
		m_layoutInfo.IsSkipBottomAlign = true;
		m_layoutInfo.IsClipped = ((IWidget)GetOwnerParagraphValue()).LayoutInfo.IsClipped;
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	SizeF ILeafWidget.Measure(DrawingContext dc)
	{
		SizeF result = default(SizeF);
		WParagraph ownerParagraphValue = GetOwnerParagraphValue();
		if (ownerParagraphValue.IsNeedToMeasureBookMarkSize)
		{
			result.Height = ((IWidget)ownerParagraphValue).LayoutInfo.Size.Height;
		}
		return result;
	}

	void IWidget.InitLayoutInfo()
	{
		m_layoutInfo = null;
	}

	void IWidget.InitLayoutInfo(IWidget widget)
	{
	}
}
