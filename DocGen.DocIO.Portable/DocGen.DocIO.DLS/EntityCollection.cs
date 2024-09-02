using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public abstract class EntityCollection : CollectionImpl, IEnumerable, IEntityCollectionBase, ICollectionBase
{
	public enum ChangeItemsType
	{
		Add,
		Remove,
		Clear
	}

	public delegate void ChangeItems(ChangeItemsType type, Entity entity);

	internal class ChangeItemsHandlerList : IEnumerable
	{
		private List<ChangeItems> m_list = new List<ChangeItems>();

		public void Add(ChangeItems handler)
		{
			if (m_list.Contains(handler))
			{
				throw new ArgumentException("handler already exists");
			}
			m_list.Add(handler);
		}

		public void Remove(ChangeItems handler)
		{
			if (!m_list.Contains(handler))
			{
				throw new ArgumentException("handler not exists");
			}
			m_list.Remove(handler);
		}

		public IEnumerator GetEnumerator()
		{
			return m_list.GetEnumerator();
		}

		public void Send(ChangeItemsType type, Entity entity)
		{
			foreach (ChangeItems item in m_list)
			{
				item.DynamicInvoke(type, entity);
			}
		}
	}

	internal ChangeItemsHandlerList ChangeItemsHandlers = new ChangeItemsHandlerList();

	private byte m_bFlags;

	public Entity this[int index] => base.InnerList[index] as Entity;

	public Entity FirstItem
	{
		get
		{
			if (base.Count <= 0)
			{
				return null;
			}
			return this[0];
		}
	}

	public Entity LastItem
	{
		get
		{
			if (base.Count <= 0)
			{
				return null;
			}
			return this[base.Count - 1];
		}
	}

	internal bool Joined => base.OwnerBase != null;

	internal bool IsNewEntityHasField
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

	internal Entity Owner => (Entity)base.OwnerBase;

	protected abstract Type[] TypesOfElement { get; }

	internal EntityCollection(WordDocument doc)
		: this(doc, null)
	{
	}

	internal EntityCollection(WordDocument doc, Entity owner)
		: base(doc, owner)
	{
	}

	public int Add(IEntity entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		if ((entity is BookmarkStart || entity is BookmarkEnd) && Owner is InlineContentControl && !IsContentControlAllowBookmark(entity))
		{
			throw new Exception($"Cannot add an object of type {entity.EntityType} into the {(Owner as InlineContentControl).ContentControlProperties.Type} Content Control");
		}
		int count = base.Count;
		if (!IsContentControlAllowParagraph() && entity is IWParagraph)
		{
			throw new InvalidOperationException("Paragraph can't be added for CheckBox and Picture content control.");
		}
		if (m_doc != null && !m_doc.IsOpening && Owner is WParagraph && base.InnerList.Count > 0 && base.InnerList[count - 1] is BookmarkEnd)
		{
			count = GetIndexOfLastBookMarkEnd();
			Insert(count, entity);
		}
		else
		{
			OnInsert(count, (Entity)entity);
			count = base.Count;
			count = OnInsertField(count, (Entity)entity);
			AddToInnerList((Entity)entity);
			OnInsertComplete(count, (Entity)entity);
			UpdateParagraphTextForInlineControl(count, (Entity)entity);
			if (base.Document != null && !base.Document.IsFieldRangeAdding && !base.Document.IsOpening && !base.Document.IsMailMerge && !base.Document.IsCloning && IsNewEntityHasField)
			{
				UpdateFieldSeparatorAndEnd((Entity)entity);
				IsNewEntityHasField = false;
			}
			OnInsertFieldComplete(base.Count - 1, (Entity)entity);
			if (entity is WSection && base.Document != null && !base.Document.IsOpening && !base.Document.IsMailMerge && !base.Document.IsCloning && !base.Document.IsClosing)
			{
				UpdateTrackRevision(entity);
			}
			if (base.Document != null && base.Document.IsComparing && base.Document.UpdateRevisionOnComparing)
			{
				UpdateTrackRevision(entity);
			}
			ClearExistingMaxPrefCellWidthOfColumns(entity);
		}
		return count;
	}

	private void ClearExistingMaxPrefCellWidthOfColumns(IEntity entity)
	{
		if (entity is WTableCell)
		{
			WTableCell wTableCell = entity as WTableCell;
			if (wTableCell.OwnerRow != null && wTableCell.OwnerRow.OwnerTable != null && wTableCell.OwnerRow.OwnerTable.m_maxPrefCellWidthOfColumns != null)
			{
				wTableCell.OwnerRow.OwnerTable.m_maxPrefCellWidthOfColumns = null;
			}
		}
		else if (entity is WTableRow)
		{
			WTableRow wTableRow = entity as WTableRow;
			if (wTableRow.OwnerTable != null && wTableRow.OwnerTable.m_maxPrefCellWidthOfColumns != null)
			{
				wTableRow.OwnerTable.m_maxPrefCellWidthOfColumns = null;
			}
		}
	}

	private bool IsContentControlAllowBookmark(IEntity entity)
	{
		if (Owner is InlineContentControl inlineContentControl)
		{
			switch (inlineContentControl.ContentControlProperties.Type)
			{
			case ContentControlType.RichText:
			case ContentControlType.Text:
			case ContentControlType.BuildingBlockGallery:
			case ContentControlType.RepeatingSection:
				return true;
			case ContentControlType.Picture:
			case ContentControlType.ComboBox:
			case ContentControlType.DropDownList:
			case ContentControlType.Date:
			case ContentControlType.Group:
			case ContentControlType.CheckBox:
				return m_doc.IsOpening;
			default:
				return false;
			}
		}
		return false;
	}

	private bool IsContentControlAllowParagraph()
	{
		bool flag = false;
		if (Owner is IInlineContentControl)
		{
			ContentControlType type = (Owner as IInlineContentControl).ContentControlProperties.Type;
			flag = type == ContentControlType.CheckBox || type == ContentControlType.Picture;
		}
		else if (Owner is IBlockContentControl)
		{
			ContentControlType type2 = (Owner as IBlockContentControl).ContentControlProperties.Type;
			flag = type2 == ContentControlType.CheckBox || type2 == ContentControlType.Picture;
		}
		else if (Owner is ICellContentControl)
		{
			ContentControlType type3 = (Owner as ICellContentControl).ContentControlProperties.Type;
			flag = type3 == ContentControlType.CheckBox || type3 == ContentControlType.Picture;
		}
		return !flag;
	}

	private int GetIndexOfLastBookMarkEnd()
	{
		int num = base.InnerList.Count;
		while (num > 0 && base.InnerList[num - 1] is BookmarkEnd)
		{
			if (base.InnerList[num - 1] is BookmarkEnd bookmarkEnd)
			{
				if (!bookmarkEnd.IsAfterParagraphMark && !bookmarkEnd.IsAfterCellMark && !bookmarkEnd.IsAfterTableMark && !bookmarkEnd.IsAfterRowMark)
				{
					break;
				}
			}
			else if (0 == 0)
			{
				break;
			}
			num--;
		}
		return num;
	}

	public void Clear()
	{
		if (base.InnerList.Count > 0 && this[0].Owner is WSection)
		{
			throw new ArgumentException("Cannot Clear objects from WSection.");
		}
		OnClear();
		base.InnerList.Clear();
	}

	public bool Contains(IEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		int index = (entity as Entity).Index;
		if (base.InnerList.Count > index && index >= 0)
		{
			return base.InnerList[index] == entity;
		}
		return false;
	}

	public int IndexOf(IEntity entity)
	{
		if (Contains(entity))
		{
			return (entity as Entity).Index;
		}
		return -1;
	}

	public void Insert(int index, IEntity entity)
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		if ((entity is BookmarkStart || entity is BookmarkEnd) && Owner is InlineContentControl && !IsContentControlAllowBookmark(entity))
		{
			throw new Exception($"Cannot insert an object of type {entity.EntityType} into the {(Owner as InlineContentControl).ContentControlProperties.Type} Content Control");
		}
		OnInsert(index, (Entity)entity);
		index = OnInsertField(index, (Entity)entity);
		InsertToInnerList(index, entity);
		OnInsertComplete(index, (Entity)entity);
		UpdateParagraphTextForInlineControl(index, (Entity)entity);
		if (base.Document != null && !base.Document.IsFieldRangeAdding && !base.Document.IsOpening && !base.Document.IsMailMerge && !base.Document.IsCloning && IsNewEntityHasField)
		{
			UpdateFieldSeparatorAndEnd((Entity)entity);
			IsNewEntityHasField = false;
		}
		OnInsertFieldComplete(index, (Entity)entity);
		if (entity is WSection && base.Document != null && !base.Document.IsOpening && !base.Document.IsMailMerge && !base.Document.IsCloning && !base.Document.IsClosing && !base.Document.IsComparing)
		{
			UpdateTrackRevision(entity);
		}
		if (base.Document != null && base.Document.IsComparing && base.Document.UpdateRevisionOnComparing)
		{
			UpdateTrackRevision(entity);
		}
		ClearExistingMaxPrefCellWidthOfColumns(entity);
	}

	internal void UpdatePositionForGroupShape(Entity entity)
	{
		if (base.Document != null && !base.Document.IsOpening && entity is GroupShape)
		{
			(entity as GroupShape).UpdatePositionForGroupShapeAndChildShape();
		}
	}

	internal void UpdateParagraphTextForInlineControl(int index, Entity entity)
	{
		if (index <= base.Count - 1 && entity is InlineContentControl)
		{
			WParagraph ownerParagraphValue = (entity as InlineContentControl).GetOwnerParagraphValue();
			InlineContentControl inlineContentControl = entity as InlineContentControl;
			UpdateTextFromInlineControl(ownerParagraphValue, inlineContentControl);
		}
	}

	internal void InsertToInnerList(int index, IEntity entity)
	{
		base.InnerList.Insert(index, entity);
		(entity as Entity).Index = index;
		UpdateIndex(index + 1, isAdd: true);
	}

	internal void RemoveFromInnerList(int index)
	{
		base.InnerList.RemoveAt(index);
		UpdateIndex(index, isAdd: false);
	}

	internal void AddToInnerList(Entity entity)
	{
		base.InnerList.Add(entity);
		entity.Index = base.InnerList.Count - 1;
	}

	internal void UpdateIndexForDuplicateEntity(int startIndex, bool isAdd)
	{
		int count = base.InnerList.Count;
		for (int i = startIndex; i < count; i++)
		{
			if (base.InnerList[i] is Entity && (i <= 0 || base.InnerList[i] != base.InnerList[i - 1] || (base.InnerList[i - 1] as Entity).Index != i - 1))
			{
				if (isAdd)
				{
					(base.InnerList[i] as Entity).Index++;
				}
				else
				{
					(base.InnerList[i] as Entity).Index--;
				}
			}
		}
	}

	internal void UpdateIndex(int startIndex, bool isAdd)
	{
		int count = base.InnerList.Count;
		for (int i = startIndex; i < count; i++)
		{
			if (base.InnerList[i] is Entity)
			{
				if (isAdd)
				{
					(base.InnerList[i] as Entity).Index++;
				}
				else
				{
					(base.InnerList[i] as Entity).Index--;
				}
			}
		}
	}

	public void Remove(IEntity entity)
	{
		if (entity.Owner is WSection)
		{
			throw new ArgumentException("Cannot remove an object of type " + entity.EntityType.ToString() + " from the " + entity.Owner.EntityType);
		}
		UpdateDocumentCollection(entity);
		OnRemove((entity as Entity).Index);
		RemoveFromInnerList((entity as Entity).Index);
	}

	public void RemoveAt(int index)
	{
		if ((base.InnerList[index] as IEntity).Owner is WSection)
		{
			throw new ArgumentException("Cannot remove an object of type " + (base.InnerList[index] as IEntity).EntityType.ToString() + " from the " + (base.InnerList[index] as IEntity).Owner.EntityType);
		}
		UpdateDocumentCollection(base.InnerList[index] as IEntity);
		OnRemove(index);
		RemoveFromInnerList(index);
	}

	private void UpdateDocumentCollection(IEntity entity)
	{
		if (base.Document == null)
		{
			return;
		}
		if (entity is ParagraphItem && (entity as ParagraphItem).GetCharFormat() != null && IsRenderableItem(entity))
		{
			UpdateRevisionCollection((entity as ParagraphItem).GetCharFormat());
		}
		UpdateRevisionCollection(entity as Entity);
		if (entity is WParagraph)
		{
			WParagraph wParagraph = entity as WParagraph;
			UpdateRevisionCollection(wParagraph.ParagraphFormat);
			UpdateRevisionCollection(wParagraph.BreakCharacterFormat);
			if ((entity as WParagraph).Items == null)
			{
				return;
			}
			for (int i = 0; i < (entity as WParagraph).Items.Count; i++)
			{
				ParagraphItem paragraphItem = (entity as WParagraph).Items[i];
				if (paragraphItem.GetCharFormat() != null)
				{
					UpdateRevisionCollection(paragraphItem.GetCharFormat());
				}
				UpdateRevisionCollection(paragraphItem);
				if (paragraphItem is BookmarkStart)
				{
					Bookmark bookmark = base.Document.Bookmarks.FindByName((paragraphItem as BookmarkStart).Name);
					if (bookmark != null && !(paragraphItem as BookmarkStart).IsDetached)
					{
						if (bookmark.BookmarkStart != null)
						{
							bookmark.BookmarkStart.IsDetached = true;
						}
						if (bookmark.BookmarkEnd != null)
						{
							bookmark.BookmarkEnd.IsDetached = true;
						}
						base.Document.Bookmarks.InnerList.Remove(bookmark);
					}
				}
				else if (paragraphItem is WField)
				{
					base.Document.Fields.Remove(paragraphItem as WField);
				}
				else if (paragraphItem is WTextBox)
				{
					foreach (TextBodyItem item in (paragraphItem as WTextBox).TextBoxBody.Items)
					{
						UpdateDocumentCollection(item);
					}
					base.Document.TextBoxes.Remove(paragraphItem as WTextBox);
					base.Document.FloatingItems.Remove(paragraphItem as WTextBox);
				}
				else if (paragraphItem is Shape)
				{
					foreach (TextBodyItem item2 in (paragraphItem as Shape).TextBody.Items)
					{
						UpdateDocumentCollection(item2);
					}
					base.Document.AutoShapeCollection.Remove(paragraphItem as Shape);
					base.Document.FloatingItems.Remove(paragraphItem as Shape);
				}
				else if (paragraphItem is GroupShape)
				{
					UpdateGroupShapeCollection(paragraphItem as GroupShape);
				}
				else if (paragraphItem is WPicture)
				{
					(paragraphItem as WPicture).RemoveImageInCollection();
				}
				else if (paragraphItem is WComment)
				{
					(paragraphItem as WComment).IsDetached = true;
					base.Document.Comments.InnerList.Remove(paragraphItem as WComment);
				}
			}
		}
		if (entity is Shape)
		{
			foreach (TextBodyItem item3 in (entity as Shape).TextBody.Items)
			{
				UpdateDocumentCollection(item3);
			}
			base.Document.AutoShapeCollection.Remove(entity as Shape);
			base.Document.FloatingItems.Remove(entity as Shape);
		}
		if (entity is GroupShape)
		{
			UpdateGroupShapeCollection(entity as GroupShape);
			return;
		}
		if (entity is WTextBox)
		{
			if ((entity as WTextBox).TextBoxBody != null)
			{
				foreach (TextBodyItem item4 in (entity as WTextBox).TextBoxBody.Items)
				{
					UpdateDocumentCollection(item4);
				}
			}
			base.Document.TextBoxes.Remove(entity as WTextBox);
			base.Document.FloatingItems.Remove(entity as WTextBox);
			return;
		}
		if (entity is WTableCell)
		{
			UpdateRevisionCollection((entity as WTableCell).CellFormat);
			{
				foreach (TextBodyItem item5 in ((WTableCell)entity).Items)
				{
					UpdateDocumentCollection(item5);
				}
				return;
			}
		}
		if (entity is WTableRow)
		{
			UpdateRevisionCollection((entity as WTableRow).RowFormat);
			{
				foreach (WTableCell cell in ((WTableRow)entity).Cells)
				{
					UpdateRevisionCollection(cell.CellFormat);
					foreach (TextBodyItem item6 in cell.Items)
					{
						UpdateDocumentCollection(item6);
					}
				}
				return;
			}
		}
		if (entity is WTable)
		{
			UpdateRevisionCollection((entity as WTable).DocxTableFormat.Format);
			{
				foreach (WTableRow row in (entity as WTable).Rows)
				{
					UpdateRevisionCollection(row.RowFormat);
					foreach (WTableCell cell2 in row.Cells)
					{
						UpdateRevisionCollection(cell2.CellFormat);
						foreach (TextBodyItem item7 in cell2.Items)
						{
							UpdateDocumentCollection(item7);
						}
					}
				}
				return;
			}
		}
		if (entity is BookmarkStart)
		{
			Bookmark bookmark2 = base.Document.Bookmarks.FindByName((entity as BookmarkStart).Name);
			if (bookmark2 != null && !(entity as BookmarkStart).IsDetached)
			{
				if (bookmark2.BookmarkStart != null)
				{
					bookmark2.BookmarkStart.IsDetached = true;
				}
				if (bookmark2.BookmarkEnd != null)
				{
					bookmark2.BookmarkEnd.IsDetached = true;
				}
				base.Document.Bookmarks.InnerList.Remove(bookmark2);
			}
		}
		else
		{
			if (!(entity is WSection))
			{
				return;
			}
			UpdateRevisionCollection((entity as WSection).SectionFormat);
			foreach (WTextBody childEntity in ((WSection)entity).ChildEntities)
			{
				foreach (TextBodyItem item8 in childEntity.Items)
				{
					UpdateDocumentCollection(item8);
				}
			}
		}
	}

	private bool IsRenderableItem(IEntity entity)
	{
		if (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && !(entity is EditableRangeStart) && !(entity is EditableRangeEnd))
		{
			return !(entity is WCommentMark);
		}
		return false;
	}

	private void UpdateGroupShapeCollection(GroupShape groupShape)
	{
		foreach (ParagraphItem childShape2 in groupShape.ChildShapes)
		{
			if (childShape2 is ChildGroupShape)
			{
				foreach (ChildShape childShape3 in (childShape2 as ChildGroupShape).ChildShapes)
				{
					if (!(childShape3 is ChildGroupShape))
					{
						continue;
					}
					foreach (TextBodyItem item in (childShape3 as ChildGroupShape).TextBody.Items)
					{
						UpdateDocumentCollection(item);
					}
				}
			}
			else if (childShape2 is ChildShape)
			{
				foreach (TextBodyItem item2 in (childShape2 as ChildShape).TextBody.Items)
				{
					UpdateDocumentCollection(item2);
				}
			}
			base.Document.FloatingItems.Remove(groupShape);
		}
	}

	private void UpdateRevisionCollection(Entity entity)
	{
		if (base.Document == null || (!base.Document.TrackChanges && !base.Document.IsComparing) || base.Document.IsOpening || base.Document.IsMailMerge || base.Document.IsCloning || base.Document.IsClosing)
		{
			return;
		}
		for (int i = 0; i < entity.RevisionsInternal.Count; i++)
		{
			Revision revision = entity.RevisionsInternal[i];
			if (revision.Range.Count > 0 && revision.Range.InnerList.Contains(entity))
			{
				revision.Range.InnerList.Remove(entity);
				entity.RevisionsInternal.Remove(revision);
				i--;
			}
			if (revision.Range.Count == 0)
			{
				revision.RemoveSelf();
			}
		}
	}

	private void UpdateRevisionCollection(FormatBase formatBase)
	{
		if (base.Document == null || (!base.Document.TrackChanges && !base.Document.IsComparing) || base.Document.IsOpening || base.Document.IsMailMerge || base.Document.IsCloning || base.Document.IsClosing)
		{
			return;
		}
		for (int i = 0; i < formatBase.Revisions.Count; i++)
		{
			Revision revision = formatBase.Revisions[i];
			if (revision.Range.Count > 0 && revision.Range.InnerList.Contains(formatBase))
			{
				revision.Range.InnerList.Remove(formatBase);
				formatBase.Revisions.Remove(revision);
				i--;
			}
			if (revision.Range.Count == 0)
			{
				revision.RemoveSelf();
			}
		}
	}

	internal void UpdateTrackRevision(IEntity entity)
	{
		if (entity is ParagraphItem)
		{
			UpdateTrackRevision(entity as ParagraphItem);
			return;
		}
		if (entity is TextBodyItem)
		{
			UpdateTrackRevision(entity as TextBodyItem);
			return;
		}
		if (entity is WSection)
		{
			UpdateTrackRevision(entity as WSection);
			return;
		}
		if (entity is WordDocument)
		{
			foreach (WSection childEntity in ((WordDocument)entity).ChildEntities)
			{
				UpdateTrackRevision(childEntity);
			}
			return;
		}
		if (entity is WTableCell)
		{
			if (((WTableCell)entity).CellFormat.HasKey(15))
			{
				base.Document.UpdateCellFormatRevision((WTableCell)entity);
			}
			UpdateTrackRevision((WTableCell)entity);
		}
		else
		{
			if (!(entity is WTableRow))
			{
				return;
			}
			(entity as WTableRow).UpdateRowRevision(base.Document);
			foreach (WTableCell cell in ((WTableRow)entity).Cells)
			{
				if (cell.CellFormat.HasKey(15))
				{
					base.Document.UpdateCellFormatRevision(cell);
				}
				UpdateTrackRevision(cell);
			}
			base.Document.UpdateTableFormatRevision(entity as WTableRow);
		}
	}

	private void UpdateTrackRevision(WSection section)
	{
		base.Document.SectionFormatChange(section);
		foreach (WTextBody childEntity in section.ChildEntities)
		{
			foreach (TextBodyItem item in childEntity.Items)
			{
				UpdateTrackRevision(item);
			}
		}
	}

	private void UpdateTrackRevision(TextBodyItem bodyItemEntity)
	{
		switch (bodyItemEntity.EntityType)
		{
		case EntityType.Paragraph:
		{
			WParagraph paragraph = bodyItemEntity as WParagraph;
			UpdateParagraphRevision(paragraph);
			break;
		}
		case EntityType.Table:
			UpdateTrackRevision(bodyItemEntity as WTable);
			break;
		case EntityType.BlockContentControl:
		{
			BlockContentControl blockContentControl = bodyItemEntity as BlockContentControl;
			UpdateTrackRevision(blockContentControl.TextBody);
			break;
		}
		}
	}

	private void UpdateTrackRevision(ParagraphItem paraItem)
	{
		UpdateParaItemRevision(paraItem);
		if (paraItem is InlineContentControl)
		{
			InlineContentControl inlineContentControl = paraItem as InlineContentControl;
			UpdateTrackRevision(inlineContentControl.ParagraphItems);
		}
		else if (paraItem is WTextBox)
		{
			WTextBox wTextBox = paraItem as WTextBox;
			UpdateTrackRevision(wTextBox.TextBoxBody);
		}
		else if (paraItem is Shape)
		{
			Shape shape = paraItem as Shape;
			UpdateTrackRevision(shape.TextBody);
		}
	}

	private void UpdateTrackRevision(ParagraphItemCollection paraItems)
	{
		for (int i = 0; i < paraItems.Count; i++)
		{
			UpdateTrackRevision(paraItems[i]);
		}
	}

	internal void UpdateTrackRevision(WTextBody textBody)
	{
		for (int i = 0; i < textBody.ChildEntities.Count; i++)
		{
			UpdateTrackRevision(textBody.ChildEntities[i] as TextBodyItem);
		}
	}

	private void UpdateTrackRevision(WTable table)
	{
		if (table.DocxTableFormat.Format.HasKey(15))
		{
			base.Document.UpdateTableRevision(table);
		}
		foreach (WTableRow row in table.Rows)
		{
			row.UpdateRowRevision(base.Document);
			foreach (WTableCell cell in row.Cells)
			{
				if (cell.CellFormat.HasKey(15))
				{
					base.Document.UpdateCellFormatRevision(cell);
				}
				UpdateTrackRevision(cell);
			}
			base.Document.UpdateTableFormatRevision(row);
		}
	}

	private void UpdateParaItemRevision(ParagraphItem paraItem)
	{
		WCharacterFormat wCharacterFormat = ((!(paraItem is Break)) ? paraItem.GetCharFormat() : (paraItem as Break).CharacterFormat);
		if (wCharacterFormat == null)
		{
			return;
		}
		if (wCharacterFormat.HasBoolKey(105))
		{
			base.Document.CharacterFormatChange(wCharacterFormat, paraItem, null);
		}
		if (paraItem.m_clonedRevisions != null)
		{
			CheckTrackChange(paraItem);
			return;
		}
		if (wCharacterFormat.IsInsertRevision)
		{
			base.Document.ParagraphItemRevision(paraItem, RevisionType.Insertions, wCharacterFormat.AuthorName, wCharacterFormat.RevDateTime, null, isNestedRevision: true, null, null, null);
		}
		if (wCharacterFormat.IsDeleteRevision)
		{
			base.Document.ParagraphItemRevision(paraItem, RevisionType.Deletions, wCharacterFormat.AuthorName, wCharacterFormat.RevDateTime, null, isNestedRevision: true, null, null, null);
		}
	}

	private void CheckTrackChange(ParagraphItem item)
	{
		if (base.Document == null)
		{
			return;
		}
		item.SetOwnerDoc(base.Document);
		List<Revision> clonedRevisions = item.m_clonedRevisions;
		if (clonedRevisions == null)
		{
			return;
		}
		if (clonedRevisions.Count == 0)
		{
			if (base.Document.cloneMoveRevision != null)
			{
				base.Document.cloneMoveRevision.Range.Items.Add(item);
			}
			return;
		}
		for (int i = 0; i < clonedRevisions.Count; i++)
		{
			Revision revision = clonedRevisions[i];
			RevisionType revisionType = revision.RevisionType;
			string author = revision.Author;
			DateTime date = revision.Date;
			string name = revision.Name;
			if (item.GetCharFormat() != null && item.GetCharFormat().HasBoolKey(105))
			{
				base.Document.CharFormatChangeRevision(item.GetCharFormat(), item);
			}
			if (base.Document.cloneMoveRevision != null && (revisionType == RevisionType.Insertions || revisionType == RevisionType.Deletions))
			{
				base.Document.cloneMoveRevision.Range.Items.Add(item);
			}
			switch (revisionType)
			{
			case RevisionType.Deletions:
				base.Document.ParagraphItemRevision(item, RevisionType.Deletions, author, date, name, isNestedRevision: true, base.Document.cloneMoveRevision, null, null);
				break;
			case RevisionType.Insertions:
				base.Document.ParagraphItemRevision(item, RevisionType.Insertions, author, date, name, isNestedRevision: true, base.Document.cloneMoveRevision, null, null);
				break;
			case RevisionType.MoveFrom:
			case RevisionType.MoveTo:
				if (item.IsMoveRevisionFirstItem)
				{
					base.Document.cloneMoveRevision = base.Document.CreateNewRevision(revisionType, author, date, name);
					base.Document.cloneMoveRevision.IsAfterParagraphMark = revision.IsAfterParagraphMark;
					base.Document.cloneMoveRevision.IsAfterTableMark = revision.IsAfterTableMark;
					base.Document.cloneMoveRevision.IsAfterRowMark = revision.IsAfterRowMark;
					base.Document.cloneMoveRevision.IsAfterCellMark = revision.IsAfterCellMark;
					item.IsMoveRevisionFirstItem = false;
				}
				base.Document.ParagraphItemRevision(item, revisionType, author, date, null, isNestedRevision: true, base.Document.cloneMoveRevision, null, null);
				if (item.IsMoveRevisionLastItem)
				{
					base.Document.cloneMoveRevision = null;
					item.IsMoveRevisionLastItem = false;
				}
				break;
			}
		}
		item.m_clonedRevisions.Clear();
		item.m_clonedRevisions = null;
	}

	private void UpdateParagraphRevision(WParagraph paragraph)
	{
		base.Document.ParagraphFormatChange(paragraph.ParagraphFormat);
		base.Document.CharacterFormatChange(paragraph.BreakCharacterFormat, null, null);
		UpdateTrackRevision(paragraph.Items);
		base.Document.UpdateLastItemRevision(paragraph, paragraph.Items);
	}

	internal Entity NextSibling(Entity entity)
	{
		int index = entity.Index;
		if (index < 0 || index > base.Count - 2)
		{
			return null;
		}
		return this[index + 1];
	}

	internal Entity PreviousSibling(Entity entity)
	{
		int index = entity.Index;
		if (index < 1 || index > base.Count - 1)
		{
			return null;
		}
		return this[index - 1];
	}

	internal int GetNextOrPrevIndex(int index, EntityType type, bool next)
	{
		do
		{
			index += (next ? 1 : (-1));
			if (index > base.InnerList.Count - 1 || index < 0)
			{
				return -1;
			}
		}
		while ((base.InnerList[index] as Entity).EntityType != type);
		return index;
	}

	internal void InternalClearBy(EntityType type)
	{
		for (int i = 0; i < base.Count; i++)
		{
			Entity entity = this[i];
			if (entity.EntityType == type)
			{
				entity.SetOwner(null);
				RemoveFromInnerList(i);
				i--;
			}
		}
	}

	internal void CloneTo(EntityCollection destColl)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			destColl.Add(this[i].Clone());
		}
	}

	protected virtual void OnClear()
	{
		for (int i = 0; i < base.Count; i++)
		{
			Entity entity = this[i];
			UpdateDocumentCollection(entity);
			entity.SetOwner(null);
			entity.Index = -1;
		}
		ChangeItemsHandlers.Send(ChangeItemsType.Clear, null);
	}

	protected virtual void OnInsert(int index, Entity entity)
	{
		if (!IsCorrectElementType(entity))
		{
			throw new ArgumentException($"Cannot insert an object of type {entity.EntityType} into the {Owner.EntityType}");
		}
		if (Joined)
		{
			bool flag = entity.Owner == null;
			_ = Owner.DeepDetached;
			WordDocument document = entity.Document;
			if (base.Document != document)
			{
				if (!flag && !entity.DeepDetached)
				{
					throw new InvalidOperationException("You can not add no clonned entity from other document");
				}
				entity.CloneRelationsTo(base.Document, Owner);
				if (!string.IsNullOrEmpty(base.Document.Settings.DuplicateListStyleNames) && !base.Document.Settings.MaintainImportedListCache && base.Document.IsCloning)
				{
					base.Document.Settings.DuplicateListStyleNames = string.Empty;
				}
			}
			else if (base.Document.IsMailMerge && entity is WParagraph && (base.Document.ImportOptions & ImportOptions.ListRestartNumbering) != 0)
			{
				(entity as WParagraph).ListFormat.CloneListRelationsTo(base.Document, null);
			}
			if (!flag)
			{
				if (!base.Document.Settings.DisableMovingEntireField)
				{
					UpdateFieldRange(entity);
				}
				base.Document.IsSkipFieldDetach = true;
				entity.RemoveSelf();
				base.Document.IsSkipFieldDetach = false;
			}
			else if (!base.Document.IsOpening && !base.Document.IsCloning && base.Document == document)
			{
				entity.AddSelf();
			}
			if (entity.EntityType == EntityType.TextBox && (entity as WTextBox).Shape != null)
			{
				(entity as WTextBox).Shape.SetOwner(Owner);
			}
			entity.SetOwner(Owner);
		}
		ChangeItemsHandlers.Send(ChangeItemsType.Add, entity);
	}

	private void UpdateFieldRange(Entity entity)
	{
		if (entity is WParagraph)
		{
			for (int i = 0; i < (entity as WParagraph).Items.Count; i++)
			{
				ParagraphItem paragraphItem = (entity as WParagraph).Items[i];
				if (paragraphItem is WField && (paragraphItem as WField).FieldEnd != null && (paragraphItem as WField).FieldEnd.OwnerParagraph != entity)
				{
					WField obj = paragraphItem as WField;
					obj.Range.Items.Clear();
					obj.IsFieldRangeUpdated = false;
					obj.UpdateFieldRange();
					IsNewEntityHasField = true;
					break;
				}
			}
		}
		else if (entity is WField && (entity as WField).FieldEnd != null && (entity as WField).FieldEnd.OwnerParagraph != Owner)
		{
			WField obj2 = entity as WField;
			obj2.Range.Items.Clear();
			obj2.IsFieldRangeUpdated = false;
			obj2.UpdateFieldRange();
			IsNewEntityHasField = true;
		}
	}

	private void UpdateFieldSeparatorAndEnd(Entity entity)
	{
		if (entity is WParagraph)
		{
			for (int i = 0; i < (entity as WParagraph).Items.Count; i++)
			{
				ParagraphItem paragraphItem = (entity as WParagraph).Items[i];
				if (paragraphItem is WField && (paragraphItem as WField).FieldEnd != null && (paragraphItem as WField).FieldEnd.OwnerParagraph != entity)
				{
					InsertFieldRange(paragraphItem as WField, (entity as WParagraph).GetIndexInOwnerCollection() + 1, i + 1, isSkipParaItems: true);
				}
			}
		}
		else if (entity is WField && (entity as WField).FieldEnd != null && (entity as WField).FieldEnd.OwnerParagraph != Owner)
		{
			if (Owner is WParagraph)
			{
				InsertFieldRange(entity as WField, (Owner as WParagraph).GetIndexInOwnerCollection() + 1, (entity as WField).GetIndexInOwnerCollection() + 1, isSkipParaItems: false);
			}
			else if (Owner is InlineContentControl && (Owner as InlineContentControl).GetOwnerParagraphValue() != null)
			{
				InsertFieldRange(entity as WField, (Owner as InlineContentControl).GetOwnerParagraphValue().GetIndexInOwnerCollection() + 1, (entity as WField).GetIndexInOwnerCollection() + 1, isSkipParaItems: false);
			}
		}
	}

	private void InsertFieldRange(WField field, int bodyItemIndex, int paraItemIndex, bool isSkipParaItems)
	{
		base.Document.IsFieldRangeAdding = true;
		WParagraph ownerParagraph = field.OwnerParagraph;
		for (int i = 0; i < field.Range.Items.Count; i++)
		{
			Entity entity = field.Range.Items[i] as Entity;
			if (entity is ParagraphItem && Owner is WParagraph && !isSkipParaItems)
			{
				(Owner as WParagraph).ChildEntities.Insert(paraItemIndex, entity);
				paraItemIndex++;
			}
			else
			{
				if (!(entity is TextBodyItem))
				{
					continue;
				}
				if (i == field.Range.Items.Count - 1)
				{
					if (Owner is WParagraph && paraItemIndex < (Owner as WParagraph).ChildEntities.Count)
					{
						WParagraph wParagraph = new WParagraph(base.Document);
						WParagraph wParagraph2 = entity as WParagraph;
						int indexInOwnerCollection = field.FieldEnd.GetIndexInOwnerCollection();
						for (int j = 0; j <= indexInOwnerCollection; j++)
						{
							wParagraph.ChildEntities.Add(wParagraph2.ChildEntities[0]);
						}
						ownerParagraph.OwnerTextBody.ChildEntities.Insert(bodyItemIndex, wParagraph);
						int count = (Owner as WParagraph).ChildEntities.Count;
						for (int k = paraItemIndex; k < count; k++)
						{
							wParagraph.ChildEntities.Add((Owner as WParagraph).ChildEntities[paraItemIndex]);
						}
					}
					else if (ownerParagraph.OwnerTextBody.ChildEntities.Count > 0 && ownerParagraph.OwnerTextBody.ChildEntities.Count > bodyItemIndex && ownerParagraph.OwnerTextBody.ChildEntities[bodyItemIndex] is WParagraph)
					{
						WParagraph wParagraph3 = ownerParagraph.OwnerTextBody.ChildEntities[bodyItemIndex] as WParagraph;
						WParagraph wParagraph4 = entity as WParagraph;
						int indexInOwnerCollection2 = field.FieldEnd.GetIndexInOwnerCollection();
						for (int l = 0; l <= indexInOwnerCollection2; l++)
						{
							wParagraph3.ChildEntities.Insert(l, wParagraph4.ChildEntities[0]);
						}
					}
					else
					{
						WParagraph wParagraph5 = new WParagraph(base.Document);
						WParagraph wParagraph6 = entity as WParagraph;
						int indexInOwnerCollection3 = field.FieldEnd.GetIndexInOwnerCollection();
						for (int m = 0; m <= indexInOwnerCollection3; m++)
						{
							wParagraph5.ChildEntities.Add(wParagraph6.ChildEntities[0]);
						}
						ownerParagraph.OwnerTextBody.ChildEntities.Insert(bodyItemIndex, wParagraph5);
					}
				}
				else
				{
					if (ownerParagraph.OwnerTextBody.ChildEntities.Contains(entity) && ownerParagraph.OwnerTextBody.ChildEntities.IndexOf(entity) < bodyItemIndex)
					{
						bodyItemIndex--;
					}
					ownerParagraph.OwnerTextBody.ChildEntities.Insert(bodyItemIndex, entity);
					bodyItemIndex++;
				}
			}
		}
		base.Document.IsFieldRangeAdding = false;
	}

	protected virtual void OnInsertComplete(int index, Entity entity)
	{
		if (Joined && !Owner.DeepDetached && (!(entity.Owner is WParagraph) || (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && !(entity is EditableRangeStart) && !(entity is EditableRangeEnd))))
		{
			entity.AttachToDocument();
		}
	}

	protected virtual void OnRemove(int index)
	{
		Entity entity = this[index];
		entity.SetOwner(null);
		ChangeItemsHandlers.Send(ChangeItemsType.Remove, entity);
	}

	private bool IsCorrectElementType(Entity entity)
	{
		bool flag = false;
		Type[] typesOfElement = TypesOfElement;
		foreach (Type type in typesOfElement)
		{
			flag = IsInstanceOfType(type, entity);
			if (flag)
			{
				break;
			}
		}
		return flag;
	}

	private bool IsInstanceOfType(Type type, Entity ent)
	{
		if (type == ent.GetType())
		{
			return true;
		}
		switch (ent.EntityType)
		{
		case EntityType.WordDocument:
		case EntityType.Section:
			if (type.Name == "WidgetContainer" || type.Name == "WidgetBase" || type.Name == "Entity")
			{
				return true;
			}
			break;
		case EntityType.TextBody:
		case EntityType.HeaderFooter:
		case EntityType.TableCell:
			if (type.Name == "WTextBody" || type.Name == "WidgetContainer" || type.Name == "WidgetBase" || type.Name == "Entity")
			{
				return true;
			}
			break;
		case EntityType.Paragraph:
		case EntityType.AlternateChunk:
		case EntityType.BlockContentControl:
		case EntityType.Table:
			if (type.Name == "TextBodyItem" || type.Name == "WidgetBase" || type.Name == "Entity")
			{
				return true;
			}
			break;
		case EntityType.TableRow:
			if (type.Name == "WidgetBase" || type.Name == "Entity")
			{
				return true;
			}
			break;
		case EntityType.InlineContentControl:
		case EntityType.TextRange:
		case EntityType.Picture:
		case EntityType.FieldMark:
		case EntityType.BookmarkStart:
		case EntityType.BookmarkEnd:
		case EntityType.Shape:
		case EntityType.Comment:
		case EntityType.Footnote:
		case EntityType.TextBox:
		case EntityType.Break:
		case EntityType.Symbol:
		case EntityType.TOC:
		case EntityType.XmlParaItem:
		case EntityType.Undefined:
		case EntityType.Chart:
		case EntityType.CommentMark:
		case EntityType.OleObject:
		case EntityType.AbsoluteTab:
		case EntityType.AutoShape:
		case EntityType.EditableRangeStart:
		case EntityType.EditableRangeEnd:
		case EntityType.GroupShape:
		case EntityType.ChildShape:
		case EntityType.ChildGroupShape:
		case EntityType.Math:
			if (type.Name == "ParagraphItem" || type.Name == "WidgetBase" || type.Name == "Entity")
			{
				return true;
			}
			break;
		case EntityType.Field:
		case EntityType.MergeField:
		case EntityType.SeqField:
		case EntityType.EmbededField:
		case EntityType.ControlField:
			if (type.Name == "WField" || type.Name == "WTextRange" || type.Name == "ParagraphItem" || type.Name == "WidgetBase" || type.Name == "Entity")
			{
				return true;
			}
			break;
		case EntityType.TextFormField:
		case EntityType.DropDownFormField:
		case EntityType.CheckBox:
			if (type.Name == "WFormField" || type.Name == "WField" || type.Name == "WTextRange" || type.Name == "ParagraphItem" || type.Name == "WidgetBase" || type.Name == "Entity")
			{
				return true;
			}
			break;
		default:
			return false;
		}
		return false;
	}

	private int OnInsertField(int index, Entity entity)
	{
		if (m_doc != null && !m_doc.IsOpening)
		{
			if (entity is WFormField && !m_doc.IsHTMLImport && !m_doc.IsSkipFieldDetach)
			{
				index = OnInsertFormField(index, entity);
				if ((entity as WField).FieldEnd != null)
				{
					base.Document.ClonedFields.Push(entity as WField);
				}
			}
			else if (entity is WField && (entity as WField).FieldEnd != null)
			{
				base.Document.ClonedFields.Push(entity as WField);
			}
			else if (entity is WOleObject && (entity as WOleObject).Field != null && (entity as WOleObject).Field.FieldEnd != null)
			{
				base.Document.ClonedFields.Push((entity as WOleObject).Field);
			}
		}
		return index;
	}

	private void OnInsertFieldComplete(int index, Entity entity)
	{
		if (m_doc == null || m_doc.IsOpening)
		{
			return;
		}
		if (entity is WFormField && (entity as WFormField).FieldEnd == null && !m_doc.IsHTMLImport && !m_doc.IsSkipFieldDetach)
		{
			OnInsertFormFieldComplete(index, entity);
		}
		else if (IsMergeFieldNeedToBeUpdated(entity))
		{
			OnMergeFieldComplete(index, entity);
		}
		else if (entity is WField && !string.IsNullOrEmpty((entity as WField).m_detachedFieldCode) && !m_doc.IsInternalManipulation())
		{
			WTextRange wTextRange = new WTextRange(m_doc);
			wTextRange.Text = (entity as WField).m_detachedFieldCode;
			(entity as WField).m_detachedFieldCode = string.Empty;
			wTextRange.ApplyCharacterFormat((entity as WField).CharacterFormat);
			Insert(++index, wTextRange);
		}
		else if (entity is WFieldMark && base.Document.ClonedFields.Count > 0)
		{
			WField wField = base.Document.ClonedFields.Peek();
			if ((entity as WFieldMark).Type == FieldMarkType.FieldSeparator)
			{
				wField.FieldSeparator = entity as WFieldMark;
				return;
			}
			wField = base.Document.ClonedFields.Pop();
			wField.FieldEnd = entity as WFieldMark;
		}
	}

	private bool IsMergeFieldNeedToBeUpdated(Entity entity)
	{
		if (entity is WMergeField && ((entity as WField).FieldEnd == null || !((entity as WField).FieldEnd.Owner is WParagraph)) && !m_doc.IsMailMerge && !m_doc.IsCloning && !m_doc.IsHTMLImport)
		{
			return !m_doc.IsSkipFieldDetach;
		}
		return false;
	}

	private void OnMergeFieldComplete(int fieldIndex, Entity entity)
	{
		WMergeField wMergeField = entity as WMergeField;
		WFieldMark wFieldMark = new WFieldMark(m_doc, FieldMarkType.FieldSeparator);
		WFieldMark wFieldMark2 = new WFieldMark(m_doc, FieldMarkType.FieldEnd);
		WTextRange wTextRange = new WTextRange(m_doc);
		if (string.IsNullOrEmpty(wMergeField.m_detachedFieldCode))
		{
			wTextRange.Text = wMergeField.FindFieldCode();
		}
		else
		{
			wTextRange.Text = wMergeField.m_detachedFieldCode;
			wMergeField.m_detachedFieldCode = string.Empty;
		}
		(Owner as WParagraph).Items.Insert(++fieldIndex, wTextRange);
		if (wMergeField.FieldSeparator == null || !(wMergeField.FieldSeparator.Owner is WParagraph))
		{
			wMergeField.FieldSeparator = wFieldMark;
			(Owner as WParagraph).Items.Insert(fieldIndex + 1, wFieldMark);
		}
		fieldIndex = wMergeField.FieldSeparator.GetIndexInOwnerCollection();
		wMergeField.FieldEnd = wFieldMark2;
		(Owner as WParagraph).Items.Insert(fieldIndex + 1, wFieldMark2);
		wMergeField.UpdateMergeFieldResult();
	}

	private int OnInsertFormField(int index, Entity entity)
	{
		switch ((entity as WFormField).FormFieldType)
		{
		case FormFieldType.CheckBox:
		{
			WCheckBox wCheckBox = entity as WCheckBox;
			if (wCheckBox.Name == null || wCheckBox.Name == string.Empty)
			{
				string text3 = "Check_" + Guid.NewGuid().ToString().Replace("-", "_");
				wCheckBox.Name = text3.Substring(0, 20);
			}
			break;
		}
		case FormFieldType.DropDown:
		{
			WDropDownFormField wDropDownFormField = entity as WDropDownFormField;
			if (wDropDownFormField.Name == null || wDropDownFormField.Name == string.Empty)
			{
				string text2 = "Drop_" + Guid.NewGuid().ToString().Replace("-", "_");
				wDropDownFormField.Name = text2.Substring(0, 20);
			}
			break;
		}
		case FormFieldType.TextInput:
		{
			WTextFormField wTextFormField = entity as WTextFormField;
			if (wTextFormField.Name == null || wTextFormField.Name == string.Empty)
			{
				string text = "Text_" + Guid.NewGuid().ToString().Replace("-", "_");
				wTextFormField.Name = text.Substring(0, 20);
			}
			if (wTextFormField.DefaultText == null || wTextFormField.DefaultText == string.Empty)
			{
				wTextFormField.DefaultText = "\u2002\u2002\u2002\u2002\u2002";
			}
			break;
		}
		}
		ParagraphItemCollection paragraphItemCollection = ((Owner is WParagraph) ? (Owner as WParagraph).Items : ((Owner is InlineContentControl) ? (Owner as InlineContentControl).ParagraphItems : null));
		if (paragraphItemCollection == null || (paragraphItemCollection.Count > 0 && ((paragraphItemCollection.LastItem is BookmarkStart && (paragraphItemCollection.LastItem as BookmarkStart).Name == (entity as WFormField).Name) || (index < paragraphItemCollection.Count && index > 0 && paragraphItemCollection[index - 1] is BookmarkStart && (paragraphItemCollection[index - 1] as BookmarkStart).Name == (entity as WFormField).Name))))
		{
			return index;
		}
		CheckFormFieldName((entity as WFormField).Name);
		index = ((index < base.InnerList.Count) ? index : base.InnerList.Count);
		if (!m_doc.IsMailMerge && !base.Document.IsComparing)
		{
			paragraphItemCollection.Insert(index, new BookmarkStart(base.Document, (entity as WFormField).Name));
			index++;
		}
		return index;
	}

	internal void CheckFormFieldName(string formFieldName)
	{
		Bookmark bookmark = base.Document.Bookmarks[formFieldName];
		if (bookmark == null)
		{
			return;
		}
		base.Document.Bookmarks.Remove(bookmark);
		foreach (WSection section in base.Document.Sections)
		{
			if (section.Body.FormFields != null)
			{
				WFormField wFormField = section.Body.FormFields[formFieldName];
				if (wFormField != null)
				{
					wFormField.Name = string.Empty;
				}
			}
		}
	}

	internal void OnInsertFormFieldComplete(int index, Entity entity)
	{
		WFieldMark wFieldMark = new WFieldMark(m_doc, FieldMarkType.FieldSeparator);
		WFieldMark wFieldMark2 = new WFieldMark(m_doc, FieldMarkType.FieldEnd);
		ParagraphItemCollection paragraphItemCollection = ((Owner is WParagraph) ? (Owner as WParagraph).Items : ((Owner is InlineContentControl) ? (Owner as InlineContentControl).ParagraphItems : null));
		if (paragraphItemCollection == null)
		{
			return;
		}
		WTextRange wTextRange = new WTextRange(m_doc);
		wTextRange.Text = (entity as WFormField).FieldCode;
		wTextRange.ApplyCharacterFormat((entity as WFormField).CharacterFormat);
		paragraphItemCollection.Insert(++index, wTextRange);
		if (entity is WTextFormField)
		{
			(entity as WTextFormField).FieldSeparator = wFieldMark;
			paragraphItemCollection.Insert(++index, wFieldMark);
			if ((entity as WTextFormField).TextRange.Owner == null)
			{
				paragraphItemCollection.Insert(++index, (entity as WTextFormField).TextRange);
			}
			(entity as WTextFormField).FieldEnd = wFieldMark2;
			paragraphItemCollection.Insert(++index, wFieldMark2);
			paragraphItemCollection.Insert(++index, new BookmarkEnd(base.Document, (entity as WFormField).Name));
		}
		else
		{
			(entity as WFormField).FieldEnd = wFieldMark2;
			paragraphItemCollection.Insert(++index, wFieldMark2);
			paragraphItemCollection.Insert(++index, new BookmarkEnd(base.Document, (entity as WFormField).Name));
		}
	}

	private void UpdateTextFromInlineControl(WParagraph ownerPara, InlineContentControl inlineContentControl)
	{
		foreach (ParagraphItem paragraphItem in inlineContentControl.ParagraphItems)
		{
			if (paragraphItem is WTextRange)
			{
				(paragraphItem as WTextRange).InsertTextInParagraphText(ownerPara);
			}
			else if (paragraphItem is InlineContentControl)
			{
				UpdateTextFromInlineControl(paragraphItem.GetOwnerParagraphValue(), paragraphItem as InlineContentControl);
			}
		}
	}
}
