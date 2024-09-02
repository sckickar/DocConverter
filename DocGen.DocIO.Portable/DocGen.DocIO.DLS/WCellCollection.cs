using System;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public class WCellCollection : EntityCollection
{
	private static readonly Type[] DEF_ELEMENT_TYPES = new Type[1] { typeof(WTableCell) };

	public new WTableCell this[int index] => base.InnerList[index] as WTableCell;

	protected override Type[] TypesOfElement => DEF_ELEMENT_TYPES;

	public WCellCollection(WTableRow owner)
		: base(owner.Document, owner)
	{
	}

	public int Add(WTableCell cell)
	{
		if (base.Count > 62)
		{
			throw new ArgumentException("This exceeds the maximum number of cells.");
		}
		int num = Add((IEntity)cell);
		OnInsertCell(num, cell.CellFormat);
		return num;
	}

	protected override void OnClear()
	{
		while (base.Count >= 1)
		{
			Remove(this[base.Count - 1]);
		}
	}

	public void Insert(int index, WTableCell cell)
	{
		Insert(index, (IEntity)cell);
		OnInsertCell(index, cell.CellFormat);
	}

	public int IndexOf(WTableCell cell)
	{
		return IndexOf((IEntity)cell);
	}

	public void Remove(WTableCell cell)
	{
		int cellIndex = cell.GetCellIndex();
		RemoveCellBookmark(cellIndex);
		Remove((IEntity)cell);
		OnRemoveCell(cellIndex);
	}

	public new void RemoveAt(int index)
	{
		RemoveCellBookmark(index);
		base.RemoveAt(index);
		OnRemoveCell(index);
	}

	private void RemoveCellBookmark(int index)
	{
		WTableCell cell = this[index];
		MoveBookmarkStart(cell);
		MoveBookmarkEnd(cell);
	}

	private void MoveBookmarkStart(WTableCell cell)
	{
		if (cell == null)
		{
			return;
		}
		Stack<BookmarkStart> stack = new Stack<BookmarkStart>();
		WordDocument document = cell.Document;
		if (cell.Index != 0 || cell.ChildEntities.Count <= 0)
		{
			return;
		}
		TextBodyItem textBodyItem = cell.ChildEntities[0] as TextBodyItem;
		if (!(textBodyItem is WParagraph))
		{
			return;
		}
		BookmarkStart bookmarkStart = null;
		foreach (ParagraphItem childEntity in (textBodyItem as WParagraph).ChildEntities)
		{
			if (!(childEntity is BookmarkStart) || !((childEntity as BookmarkStart).Name != "_GoBack"))
			{
				continue;
			}
			bookmarkStart = childEntity as BookmarkStart;
			if (bookmarkStart.ColumnFirst != -1 && bookmarkStart.ColumnLast != -1)
			{
				BookmarkEnd bookmarkEnd = document.Bookmarks.FindByName(bookmarkStart.Name).BookmarkEnd;
				WTable obj = (WTable)bookmarkStart.OwnerParagraph.OwnerTextBody.Owner.Owner;
				WTable wTable = (WTable)bookmarkEnd.OwnerParagraph.OwnerTextBody.Owner.Owner;
				if (obj == wTable)
				{
					stack.Push(bookmarkStart);
				}
			}
		}
		if (stack.Count <= 0)
		{
			return;
		}
		WTableCell wTableCell = ((cell.NextSibling != null) ? (cell.NextSibling as WTableCell) : ((cell.OwnerRow.NextSibling != null) ? (cell.OwnerRow.NextSibling as WTableRow).Cells[0] : null));
		if (wTableCell == null || wTableCell.ChildEntities.Count <= 0)
		{
			return;
		}
		TextBodyItem textBodyItem2 = wTableCell.ChildEntities[0] as TextBodyItem;
		if (textBodyItem2 is WParagraph)
		{
			(textBodyItem2 as WParagraph).ChildEntities.Insert(0, stack.Pop());
		}
		else if (textBodyItem2 is WTable)
		{
			WParagraph wParagraph = null;
			if ((textBodyItem2 as WTable).Rows[0].Cells[0].ChildEntities.Count > 0)
			{
				wParagraph = (textBodyItem2 as WTable).Rows[0].Cells[0].ChildEntities[0] as WParagraph;
			}
			wParagraph?.ChildEntities.Insert(0, stack.Pop());
		}
	}

	private void MoveBookmarkEnd(WTableCell cell)
	{
		if (cell == null)
		{
			return;
		}
		Queue<BookmarkEnd> queue = new Queue<BookmarkEnd>();
		WordDocument document = cell.Document;
		if (cell.Index != cell.OwnerRow.Cells.Count - 1 || cell.ChildEntities.Count <= 0)
		{
			return;
		}
		int index = cell.ChildEntities.Count - 1;
		TextBodyItem textBodyItem = cell.ChildEntities[index] as TextBodyItem;
		if (!(textBodyItem is WParagraph))
		{
			return;
		}
		BookmarkEnd bookmarkEnd = null;
		foreach (ParagraphItem childEntity in (textBodyItem as WParagraph).ChildEntities)
		{
			if (childEntity is BookmarkEnd && (childEntity as BookmarkEnd).Name != "_GoBack")
			{
				bookmarkEnd = childEntity as BookmarkEnd;
				BookmarkStart bookmarkStart = document.Bookmarks.FindByName(bookmarkEnd.Name).BookmarkStart;
				WTable wTable = (WTable)bookmarkStart.OwnerParagraph.OwnerTextBody.Owner.Owner;
				WTable wTable2 = (WTable)bookmarkEnd.OwnerParagraph.OwnerTextBody.Owner.Owner;
				if (bookmarkStart.ColumnFirst != -1 && bookmarkStart.ColumnLast != -1 && wTable == wTable2)
				{
					queue.Enqueue(bookmarkEnd);
				}
			}
		}
		if (queue.Count <= 0)
		{
			return;
		}
		WTableCell wTableCell = ((cell.PreviousSibling != null) ? (cell.PreviousSibling as WTableCell) : ((cell.OwnerRow.PreviousSibling != null) ? (cell.OwnerRow.PreviousSibling as WTableRow).Cells[(cell.OwnerRow.PreviousSibling as WTableRow).Cells.Count - 1] : null));
		if (wTableCell == null || wTableCell.ChildEntities.Count <= 0)
		{
			return;
		}
		int index2 = wTableCell.ChildEntities.Count - 1;
		TextBodyItem textBodyItem2 = wTableCell.ChildEntities[index2] as TextBodyItem;
		if (textBodyItem2 is WParagraph)
		{
			(textBodyItem2 as WParagraph).ChildEntities.Add(queue.Dequeue());
		}
		else if (textBodyItem2 is WTable)
		{
			WParagraph wParagraph = null;
			if ((textBodyItem2 as WTable).Rows[0].Cells[0].ChildEntities.Count > 0)
			{
				wParagraph = (textBodyItem2 as WTable).Rows[0].Cells[0].LastParagraph as WParagraph;
			}
			wParagraph?.ChildEntities.Add(queue.Dequeue());
		}
	}

	internal new void CloneTo(EntityCollection destColl)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			destColl.Add(this[i].CloneCell());
		}
	}

	private void OnInsertCell(int index, CellFormat cellFormat)
	{
		if (base.Owner != null && base.Owner is WTableRow && !base.Owner.Document.IsOpening)
		{
			(base.Owner as WTableRow).OnInsertCell(index, cellFormat);
		}
	}

	private void OnRemoveCell(int index)
	{
		if (base.Owner != null && base.Owner is WTableRow && !base.Owner.Document.IsOpening)
		{
			(base.Owner as WTableRow).OnRemoveCell(index);
		}
	}
}
