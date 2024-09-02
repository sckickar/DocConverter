using System.Collections.Generic;
using DocGen.DocIO.DLS.XML;

namespace DocGen.DocIO.DLS;

public abstract class Entity : XDLSSerializableBase, IEntity
{
	internal int Index = -1;

	internal List<Revision> m_revisions;

	internal List<Revision> m_clonedRevisions;

	public Entity Owner => (Entity)base.OwnerBase;

	public abstract EntityType EntityType { get; }

	public IEntity NextSibling
	{
		get
		{
			if (!(Owner is ICompositeEntity compositeEntity))
			{
				if (!(Owner is InlineContentControl))
				{
					if (!(Owner is XmlParagraphItem) || ((XmlParagraphItem)Owner).MathParaItemsCollection == null)
					{
						return null;
					}
					return ((XmlParagraphItem)Owner).MathParaItemsCollection.NextSibling(this);
				}
				return (Owner as InlineContentControl).ParagraphItems.NextSibling(this);
			}
			return compositeEntity.ChildEntities.NextSibling(this);
		}
	}

	public IEntity PreviousSibling
	{
		get
		{
			if (!(Owner is ICompositeEntity compositeEntity))
			{
				if (!(Owner is InlineContentControl))
				{
					if (!(Owner is XmlParagraphItem) || ((XmlParagraphItem)Owner).MathParaItemsCollection == null)
					{
						return null;
					}
					return ((XmlParagraphItem)Owner).MathParaItemsCollection.NextSibling(this);
				}
				return (Owner as InlineContentControl).ParagraphItems.PreviousSibling(this);
			}
			return compositeEntity.ChildEntities.PreviousSibling(this);
		}
	}

	public bool IsComposite => this is ICompositeEntity;

	internal bool DeepDetached
	{
		get
		{
			if (EntityType == EntityType.WordDocument)
			{
				return false;
			}
			if (Owner != null)
			{
				return Owner.DeepDetached;
			}
			return true;
		}
	}

	internal List<Revision> RevisionsInternal
	{
		get
		{
			if (m_revisions == null)
			{
				m_revisions = new List<Revision>();
			}
			return m_revisions;
		}
	}

	internal bool IsFloatingItem(bool isTextWrapAround)
	{
		TextWrappingStyle textWrappingStyle = TextWrappingStyle.Inline;
		if (this is WPicture)
		{
			textWrappingStyle = (this as WPicture).TextWrappingStyle;
		}
		else if (this is Shape)
		{
			textWrappingStyle = (this as Shape).WrapFormat.TextWrappingStyle;
		}
		else if (this is WTextBox)
		{
			textWrappingStyle = (this as WTextBox).TextBoxFormat.TextWrappingStyle;
		}
		else if (this is GroupShape)
		{
			textWrappingStyle = (this as GroupShape).WrapFormat.TextWrappingStyle;
		}
		else if (this is WChart)
		{
			textWrappingStyle = (this as WChart).WrapFormat.TextWrappingStyle;
		}
		if (isTextWrapAround)
		{
			if (textWrappingStyle != 0 && textWrappingStyle != TextWrappingStyle.Behind)
			{
				return textWrappingStyle != TextWrappingStyle.InFrontOfText;
			}
			return false;
		}
		return textWrappingStyle != TextWrappingStyle.Inline;
	}

	internal bool IsFallbackItem()
	{
		if (this is Shape)
		{
			return (this as Shape).Is2007Shape;
		}
		if (this is WTextBox)
		{
			return !(this as WTextBox).IsShape;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).Is2007Shape;
		}
		if (this is WPicture)
		{
			return (this as WPicture).IsShape;
		}
		return false;
	}

	internal bool IsBuiltInCharacterStyle(BuiltinStyle builtInStyle)
	{
		if (builtInStyle != BuiltinStyle.FootnoteReference && builtInStyle != BuiltinStyle.CommentReference && builtInStyle != BuiltinStyle.LineNumber && builtInStyle != BuiltinStyle.PageNumber && builtInStyle != BuiltinStyle.EndnoteReference && builtInStyle != BuiltinStyle.EndnoteText && builtInStyle != BuiltinStyle.Hyperlink && builtInStyle != BuiltinStyle.FollowedHyperlink && builtInStyle != BuiltinStyle.Strong && builtInStyle != BuiltinStyle.Emphasis && builtInStyle != BuiltinStyle.HtmlAcronym && builtInStyle != BuiltinStyle.HtmlCite && builtInStyle != BuiltinStyle.HtmlCode && builtInStyle != BuiltinStyle.HtmlDefinition && builtInStyle != BuiltinStyle.HtmlKeyboard && builtInStyle != BuiltinStyle.HtmlSample && builtInStyle != BuiltinStyle.HtmlTypewriter)
		{
			return builtInStyle == BuiltinStyle.HtmlVariable;
		}
		return true;
	}

	protected Entity(WordDocument doc, Entity owner)
		: base(doc, owner)
	{
	}

	public Entity Clone()
	{
		return (Entity)CloneImpl();
	}

	internal static bool IsVerticalTextDirection(TextDirection textDirection)
	{
		if (textDirection != TextDirection.VerticalTopToBottom && textDirection != TextDirection.VerticalFarEast)
		{
			return textDirection == TextDirection.VerticalBottomToTop;
		}
		return true;
	}

	internal virtual void AddSelf()
	{
	}

	internal virtual void AttachToDocument()
	{
		if (!(this is ICompositeEntity { ChildEntities: var childEntities }))
		{
			return;
		}
		int i = 0;
		for (int num = childEntities.Count; i < num; i++)
		{
			childEntities[i].AttachToDocument();
			if (childEntities.Count < num)
			{
				i--;
				num--;
			}
		}
	}

	internal virtual void RemoveSelf()
	{
		if (Owner is ICompositeEntity compositeEntity)
		{
			compositeEntity.ChildEntities.Remove(this);
		}
		else if (Owner is InlineContentControl)
		{
			(Owner as InlineContentControl).ParagraphItems.Remove(this);
		}
		else if (Owner is Break)
		{
			(Owner as Break).OwnerParagraph.ChildEntities.Remove(Owner);
		}
	}

	internal int GetIndexInOwnerCollection()
	{
		if (Owner is ICompositeEntity compositeEntity)
		{
			return compositeEntity.ChildEntities.IndexOf(this);
		}
		if (Owner is InlineContentControl)
		{
			return (Owner as InlineContentControl).ParagraphItems.IndexOf(this);
		}
		return -1;
	}

	internal bool IsParentOf(Entity entity)
	{
		bool result = false;
		for (OwnerHolder ownerBase = entity.OwnerBase; ownerBase != null; ownerBase = ownerBase.OwnerBase)
		{
			if (ownerBase == this)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	internal virtual void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
	}

	internal Entity GetOwnerTextBody(Entity entity)
	{
		Entity entity2 = entity;
		while (!(entity2 is WSection) && !(entity2 is HeaderFooter) && !(entity2 is WComment) && !(entity2 is WFootnote))
		{
			if (entity2.Owner == null)
			{
				return entity2;
			}
			entity2 = entity2.Owner;
		}
		return entity2;
	}

	internal Entity GetOwnerShape(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			if (entity2.Owner == null)
			{
				return null;
			}
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WTextBox) && !(entity2 is Shape));
		return entity2;
	}

	internal Entity GetBaseEntity(Entity entity)
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
		while (!(entity2 is WSection) && !(entity2 is HeaderFooter));
		return entity2;
	}

	internal string GetHierarchicalIndex(string hierarchicalIndex)
	{
		if (Owner is ICompositeEntity)
		{
			if (this is HeaderFooter)
			{
				return null;
			}
			hierarchicalIndex = GetIndexInOwnerCollection() + ";" + hierarchicalIndex;
			if (!(Owner is WordDocument))
			{
				return Owner.GetHierarchicalIndex(hierarchicalIndex);
			}
		}
		return hierarchicalIndex;
	}

	internal int GetZOrder()
	{
		switch (EntityType)
		{
		case EntityType.TextBox:
			return (this as WTextBox).TextBoxFormat.OrderIndex;
		case EntityType.Picture:
			return (this as WPicture).OrderIndex;
		case EntityType.Shape:
		case EntityType.AutoShape:
			return (this as Shape).ZOrderPosition;
		case EntityType.GroupShape:
			return (this as GroupShape).ZOrderPosition;
		case EntityType.Chart:
			return (this as WChart).ZOrderPosition;
		case EntityType.XmlParaItem:
			return (this as XmlParagraphItem).ZOrderIndex;
		case EntityType.OleObject:
			if ((this as WOleObject).OlePicture != null)
			{
				return (this as WOleObject).OlePicture.OrderIndex;
			}
			break;
		}
		return -1;
	}

	internal bool IsNeedToSortByItsPosition(Entity secondfloatingItem)
	{
		if (Owner == null || secondfloatingItem.Owner == null)
		{
			return false;
		}
		if (Owner == secondfloatingItem.Owner)
		{
			if (GetIndexInOwnerCollection() < secondfloatingItem.GetIndexInOwnerCollection())
			{
				return true;
			}
			return false;
		}
		if (Owner.Owner == null || secondfloatingItem.Owner.Owner == null)
		{
			return false;
		}
		if (Owner.Owner == secondfloatingItem.Owner.Owner)
		{
			if (Owner.GetIndexInOwnerCollection() < secondfloatingItem.Owner.GetIndexInOwnerCollection())
			{
				return true;
			}
			return false;
		}
		WParagraph wParagraph = Owner as WParagraph;
		WParagraph wParagraph2 = secondfloatingItem.Owner as WParagraph;
		WSection wSection = GetOwnerSection(wParagraph) as WSection;
		WSection wSection2 = GetOwnerSection(wParagraph2) as WSection;
		if (wSection == null || wSection2 == null)
		{
			return false;
		}
		if (wSection != wSection2)
		{
			if (wSection.GetIndexInOwnerCollection() < wSection2.GetIndexInOwnerCollection())
			{
				return true;
			}
			return false;
		}
		if ((!wParagraph.IsInCell && wParagraph2.IsInCell) || (wParagraph.IsInCell && !wParagraph2.IsInCell))
		{
			return IsNeedToSortByItsPosition(wParagraph, wParagraph2);
		}
		if (wParagraph.IsInCell && wParagraph2.IsInCell)
		{
			return IsNeedToSortByItsPosition(wParagraph.GetOwnerEntity() as WTableCell, wParagraph2.GetOwnerEntity() as WTableCell);
		}
		return false;
	}

	private bool IsNeedToSortByItsPosition(WParagraph firstFloatingItem, WParagraph secondfloatingItem)
	{
		WTable wTable = (firstFloatingItem.IsInCell ? (GetOwnerTable(firstFloatingItem) as WTable) : (GetOwnerTable(secondfloatingItem) as WTable));
		if (wTable == null)
		{
			return false;
		}
		if ((firstFloatingItem.IsInCell ? wTable.GetIndexInOwnerCollection() : firstFloatingItem.GetIndexInOwnerCollection()) < (secondfloatingItem.IsInCell ? wTable.GetIndexInOwnerCollection() : secondfloatingItem.GetIndexInOwnerCollection()))
		{
			return true;
		}
		return false;
	}

	private bool IsNeedToSortByItsPosition(WTableCell firstItemOwnerCell, WTableCell secondItemOwnerCell)
	{
		if (firstItemOwnerCell.OwnerRow == secondItemOwnerCell.OwnerRow)
		{
			if (firstItemOwnerCell.GetIndexInOwnerCollection() < secondItemOwnerCell.GetIndexInOwnerCollection())
			{
				return true;
			}
			return false;
		}
		WTable wTable = GetOwnerTable(firstItemOwnerCell.OwnerRow) as WTable;
		WTable wTable2 = GetOwnerTable(secondItemOwnerCell.OwnerRow) as WTable;
		if (wTable == null || wTable2 == null)
		{
			return false;
		}
		if (wTable != wTable2)
		{
			if (wTable.GetIndexInOwnerCollection() < wTable2.GetIndexInOwnerCollection())
			{
				return true;
			}
			return false;
		}
		if (firstItemOwnerCell.OwnerRow.OwnerTable == secondItemOwnerCell.OwnerRow.OwnerTable)
		{
			if (firstItemOwnerCell.OwnerRow.GetIndexInOwnerCollection() < secondItemOwnerCell.OwnerRow.GetIndexInOwnerCollection())
			{
				return true;
			}
			return false;
		}
		if (firstItemOwnerCell.OwnerRow.OwnerTable.IsInCell && !secondItemOwnerCell.OwnerRow.OwnerTable.IsInCell)
		{
			return IsNeedToSortByItsPosition(firstItemOwnerCell.OwnerRow.OwnerTable.GetOwnerTableCell(), secondItemOwnerCell);
		}
		if (!firstItemOwnerCell.OwnerRow.OwnerTable.IsInCell && secondItemOwnerCell.OwnerRow.OwnerTable.IsInCell)
		{
			return IsNeedToSortByItsPosition(firstItemOwnerCell, secondItemOwnerCell.OwnerRow.OwnerTable.GetOwnerTableCell());
		}
		if (firstItemOwnerCell.OwnerRow.OwnerTable.IsInCell && secondItemOwnerCell.OwnerRow.OwnerTable.IsInCell)
		{
			return IsNeedToSortByItsPosition(firstItemOwnerCell.OwnerRow.OwnerTable.GetOwnerTableCell(), secondItemOwnerCell.OwnerRow.OwnerTable.GetOwnerTableCell());
		}
		return false;
	}

	internal Entity GetOwnerSection(Entity entity)
	{
		while (entity != null && !(entity is WSection))
		{
			entity = entity.Owner;
		}
		if (!(entity is WSection))
		{
			return null;
		}
		return entity;
	}

	internal Entity GetOwnerTable(Entity entity)
	{
		while (entity != null && !(entity is WTable))
		{
			entity = entity.Owner;
			if (entity is WTable && (entity as WTable).IsInCell)
			{
				entity = entity.Owner;
			}
		}
		if (!(entity is WTable))
		{
			return null;
		}
		return entity;
	}

	internal Entity GetOwnerCellEntity()
	{
		Entity entity = this;
		while (!(entity is WTableCell) && entity.Owner != null)
		{
			entity = entity.Owner;
		}
		return entity;
	}

	internal virtual void Compare(WordDocument document)
	{
	}

	internal void RemoveEntityRevision(bool isNeedToRemoveFormatRev)
	{
		for (int i = 0; i < RevisionsInternal.Count; i++)
		{
			Revision revision = RevisionsInternal[i];
			if (revision.Range.Count > 0 && revision.Range.InnerList.Contains(this))
			{
				revision.Range.InnerList.Remove(this);
				RevisionsInternal.Remove(revision);
				i--;
			}
			if (revision.Range.Count == 0)
			{
				revision.RemoveSelf();
			}
		}
		if (this is InlineContentControl)
		{
			foreach (Entity paragraphItem2 in (this as InlineContentControl).ParagraphItems)
			{
				paragraphItem2.RemoveEntityRevision(isNeedToRemoveFormatRev);
			}
			return;
		}
		if (!(this is WParagraph))
		{
			return;
		}
		WParagraph wParagraph = this as WParagraph;
		if (isNeedToRemoveFormatRev)
		{
			RemoveFormatRevision(wParagraph.ParagraphFormat);
		}
		RemoveFormatRevision(wParagraph.BreakCharacterFormat);
		if ((this as WParagraph).Items == null)
		{
			return;
		}
		for (int j = 0; j < (this as WParagraph).Items.Count; j++)
		{
			ParagraphItem paragraphItem = (this as WParagraph).Items[j];
			if (!(paragraphItem is BookmarkStart) && !(paragraphItem is BookmarkEnd) && !(paragraphItem is EditableRangeStart) && !(paragraphItem is EditableRangeEnd))
			{
				if (paragraphItem.GetCharFormat() != null)
				{
					RemoveFormatRevision(paragraphItem.GetCharFormat());
				}
				paragraphItem.RemoveEntityRevision(isNeedToRemoveFormatRev);
			}
		}
	}

	internal void RemoveFormatRevision(FormatBase formatBase)
	{
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

	internal override void Close()
	{
		base.Close();
		if (m_revisions != null)
		{
			m_revisions.Clear();
			m_revisions = null;
		}
	}
}
