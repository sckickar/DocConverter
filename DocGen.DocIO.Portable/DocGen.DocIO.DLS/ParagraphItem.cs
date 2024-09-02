using System;
using System.Collections.Generic;
using DocGen.Drawing;
using DocGen.Office;

namespace DocGen.DocIO.DLS;

public abstract class ParagraphItem : WidgetBase, IParagraphItem, IEntity, IOfficeRun
{
	private int m_startIndex;

	protected WCharacterFormat m_charFormat;

	private byte m_bFlags = 4;

	private IOfficeMathRunElement m_ownerMathRunElement;

	internal int m_wcStartPos = -1;

	internal bool SkipDocxItem
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

	internal bool IsMappedItem
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

	internal bool IsCloned
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

	public WParagraph OwnerParagraph
	{
		get
		{
			if (OwnerMathRunElement != null)
			{
				return GetBaseWMath(OwnerMathRunElement)?.OwnerParagraph;
			}
			if (base.Owner is InlineContentControl || base.Owner is Break)
			{
				return GetOwnerParagraphValue();
			}
			return base.Owner as WParagraph;
		}
	}

	public bool IsInsertRevision
	{
		get
		{
			if (m_charFormat != null)
			{
				return m_charFormat.IsInsertRevision;
			}
			return false;
		}
	}

	internal string AuthorName
	{
		get
		{
			if (m_charFormat != null)
			{
				return m_charFormat.AuthorName;
			}
			return string.Empty;
		}
	}

	internal DateTime RevDateTime
	{
		get
		{
			if (m_charFormat != null)
			{
				return m_charFormat.RevDateTime;
			}
			return DateTime.MinValue;
		}
	}

	public bool IsDeleteRevision
	{
		get
		{
			if (m_charFormat != null)
			{
				return m_charFormat.IsDeleteRevision;
			}
			return false;
		}
	}

	internal bool IsChangedCFormat
	{
		get
		{
			if (m_charFormat != null)
			{
				return m_charFormat.IsChangedFormat;
			}
			return false;
		}
		set
		{
			if (m_charFormat != null)
			{
				m_charFormat.IsChangedFormat = value;
			}
		}
	}

	internal int StartPos
	{
		get
		{
			return m_startIndex;
		}
		set
		{
			if (m_startIndex != value)
			{
				IsDetachedTextChanged = true;
			}
			m_startIndex = value;
		}
	}

	internal bool IsDetachedTextChanged
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

	internal virtual int EndPos => StartPos;

	internal bool ItemDetached => base.OwnerBase == null;

	internal WCharacterFormat ParaItemCharFormat
	{
		get
		{
			if (m_charFormat == null)
			{
				if (base.Owner is InlineContentControl || base.Owner is XmlParagraphItem)
				{
					m_charFormat = GetOwnerParagraphValue().BreakCharacterFormat;
				}
				else
				{
					m_charFormat = OwnerParagraph.BreakCharacterFormat;
				}
			}
			return m_charFormat;
		}
	}

	public IOfficeMathRunElement OwnerMathRunElement
	{
		get
		{
			return m_ownerMathRunElement;
		}
		set
		{
			m_ownerMathRunElement = value;
			if (OwnerParagraph != null && OwnerParagraph.BreakCharacterFormat != null && OwnerParagraph.BreakCharacterFormat.BaseFormat != null && ParaItemCharFormat != null)
			{
				ParaItemCharFormat.ApplyBase(OwnerParagraph.BreakCharacterFormat.BaseFormat);
				ParaItemCharFormat.FontName = "Cambria Math";
				if (this is WOleObject)
				{
					(this as WOleObject).OlePicture.OwnerMathRunElement = value;
				}
			}
		}
	}

	internal bool IsMoveRevisionFirstItem
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

	internal bool IsMoveRevisionLastItem
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0x7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal virtual int WCStartPos
	{
		get
		{
			if (m_wcStartPos != -1 && base.Document.IsComparing)
			{
				return m_wcStartPos;
			}
			return StartPos;
		}
		set
		{
			m_wcStartPos = value;
		}
	}

	internal virtual int WCEndPos => WCStartPos;

	internal WMath GetBaseWMath(IOfficeMathRunElement OwnerMathRunElement)
	{
		IOfficeMathEntity officeMathEntity = OwnerMathRunElement;
		while (!(officeMathEntity is IOfficeMathParagraph) && officeMathEntity != null)
		{
			officeMathEntity = officeMathEntity.OwnerMathEntity;
		}
		if (!(officeMathEntity is IOfficeMathParagraph))
		{
			return null;
		}
		return (officeMathEntity as IOfficeMathParagraph).Owner as WMath;
	}

	public void ApplyStyle(string styleName)
	{
		IWCharacterStyle iWCharacterStyle = base.Document.Styles.FindByName(styleName, StyleType.CharacterStyle) as WCharacterStyle;
		if (iWCharacterStyle == null && styleName == "Default Paragraph Font")
		{
			iWCharacterStyle = (IWCharacterStyle)Style.CreateBuiltinCharacterStyle(BuiltinStyle.DefaultParagraphFont, base.Document);
		}
		if (iWCharacterStyle == null)
		{
			if (base.Document.Styles.FindByName(styleName) is Style { StyleType: StyleType.ParagraphStyle } style && !string.IsNullOrEmpty(style.LinkedStyleName))
			{
				iWCharacterStyle = base.Document.Styles.FindByName(style.LinkedStyleName, StyleType.CharacterStyle) as WCharacterStyle;
			}
			if (iWCharacterStyle == null)
			{
				throw new ArgumentException("Specified Character style not found");
			}
		}
		ApplyCharacterStyle(iWCharacterStyle);
	}

	internal void ApplyCharacterStyle(IWCharacterStyle style)
	{
		if (style == null)
		{
			throw new ArgumentNullException("newStyle");
		}
		m_charFormat.CharStyleName = style.Name;
		if (this is Break)
		{
			(this as Break).CharacterFormat.CharStyleName = style.Name;
		}
		else if (this is InlineContentControl)
		{
			(this as InlineContentControl).ApplyBaseFormatForCharacterStyle(style);
		}
	}

	internal WParagraph GetOwnerParagraphValue()
	{
		if (OwnerMathRunElement != null)
		{
			return GetBaseWMath(OwnerMathRunElement)?.OwnerParagraph;
		}
		Entity owner = base.Owner;
		while (!(owner is WParagraph) && owner != null)
		{
			owner = owner.Owner;
		}
		return owner as WParagraph;
	}

	internal void SetInsertRev(bool value, string authorName, DateTime dt)
	{
		if (this is Break)
		{
			(this as Break).CharacterFormat.IsInsertRevision = value;
			if (!string.IsNullOrEmpty(authorName))
			{
				(this as Break).CharacterFormat.AuthorName = authorName;
			}
			if (dt.Year > 1900)
			{
				(this as Break).CharacterFormat.RevDateTime = dt;
			}
		}
		else
		{
			ParaItemCharFormat.IsInsertRevision = value;
			if (!string.IsNullOrEmpty(authorName))
			{
				ParaItemCharFormat.AuthorName = authorName;
			}
			if (dt.Year > 1900)
			{
				ParaItemCharFormat.RevDateTime = dt;
			}
		}
	}

	internal void SetDeleteRev(bool value, string authorName, DateTime dt)
	{
		if (this is Break)
		{
			(this as Break).CharacterFormat.IsDeleteRevision = value;
			if (!string.IsNullOrEmpty(authorName))
			{
				(this as Break).CharacterFormat.AuthorName = authorName;
			}
			if (dt.Year > 1900)
			{
				(this as Break).CharacterFormat.RevDateTime = dt;
			}
		}
		else
		{
			ParaItemCharFormat.IsDeleteRevision = value;
			if (!string.IsNullOrEmpty(authorName))
			{
				ParaItemCharFormat.AuthorName = authorName;
			}
			if (dt.Year > 1900)
			{
				ParaItemCharFormat.RevDateTime = dt;
			}
		}
	}

	protected ParagraphItem(WordDocument doc)
		: base(doc, null)
	{
	}

	internal virtual void AttachToParagraph(WParagraph owner, int itemPos)
	{
		if (owner == null && !(base.Owner is InlineContentControl))
		{
			throw new ArgumentNullException("owner");
		}
		if (owner != OwnerParagraph && !(base.Owner is InlineContentControl))
		{
			throw new InvalidOperationException();
		}
		if (ItemDetached)
		{
			throw new InvalidOperationException();
		}
		if (this is InlineContentControl && OwnerParagraph.BreakCharacterFormat.BaseFormat != null)
		{
			foreach (ParagraphItem paragraphItem in (this as InlineContentControl).ParagraphItems)
			{
				paragraphItem.ParaItemCharFormat.ApplyBase(OwnerParagraph.BreakCharacterFormat.BaseFormat);
			}
		}
		if (base.Owner is WParagraph && OwnerParagraph.BreakCharacterFormat.BaseFormat != null)
		{
			ParaItemCharFormat.ApplyBase(OwnerParagraph.BreakCharacterFormat.BaseFormat);
		}
		else if (base.Owner is InlineContentControl && OwnerParagraph.BreakCharacterFormat.BaseFormat != null)
		{
			ParaItemCharFormat.ApplyBase(OwnerParagraph.BreakCharacterFormat.BaseFormat);
		}
		if (this is WMath)
		{
			(this as WMath).ApplyBaseFormat();
		}
		if (this is InlineContentControl)
		{
			base.Document.UpdateStartPos(this as InlineContentControl, itemPos - StartPos);
		}
		else
		{
			StartPos = itemPos;
		}
	}

	internal virtual void Detach()
	{
		if (ItemDetached && !base.Document.IsClosing)
		{
			throw new InvalidOperationException();
		}
	}

	internal void AcceptChanges()
	{
		if (m_charFormat != null)
		{
			m_charFormat.AcceptChanges();
		}
	}

	internal void RemoveChanges()
	{
		if (m_charFormat != null)
		{
			m_charFormat.RemoveChanges();
		}
	}

	internal bool HasTrackedChanges()
	{
		if (IsInsertRevision || IsDeleteRevision || IsChangedCFormat)
		{
			return true;
		}
		return false;
	}

	internal WCharacterFormat GetCharFormat()
	{
		return m_charFormat;
	}

	internal void ApplyTableStyleFormatting(WParagraph source, WParagraph clonedParagraph)
	{
		if (source.BreakCharacterFormat.TableStyleCharacterFormat != null)
		{
			clonedParagraph.BreakCharacterFormat.TableStyleCharacterFormat = source.BreakCharacterFormat.TableStyleCharacterFormat;
		}
		if (source.ParagraphFormat.TableStyleParagraphFormat != null)
		{
			clonedParagraph.ParagraphFormat.TableStyleParagraphFormat = source.ParagraphFormat.TableStyleParagraphFormat;
		}
	}

	internal void SetParagraphItemCharacterFormat(WCharacterFormat charFormat)
	{
		if (m_charFormat != null)
		{
			m_charFormat.SetOwner(null, null);
		}
		m_charFormat = new WCharacterFormat(base.Document, this);
		m_charFormat.ImportContainer(charFormat);
		m_charFormat.CopyProperties(charFormat);
		if (charFormat.BaseFormat != null && !base.Document.IsOpening && !base.Document.IsCloning && !base.Document.IsMailMerge)
		{
			m_charFormat.ApplyBase(charFormat.BaseFormat);
		}
		if (m_charFormat != null)
		{
			m_charFormat.SetOwner(this);
		}
	}

	internal bool IsNotFieldShape()
	{
		if (base.PreviousSibling is WPicture || base.PreviousSibling is Shape)
		{
			Entity entity = base.PreviousSibling as Entity;
			if (entity.PreviousSibling is WFieldMark)
			{
				WFieldMark wFieldMark = entity.PreviousSibling as WFieldMark;
				if (wFieldMark.Type == FieldMarkType.FieldSeparator && wFieldMark.ParentField != null && wFieldMark.ParentField.FieldType == FieldType.FieldShape)
				{
					return false;
				}
			}
		}
		return true;
	}

	internal void UpdateParaItemRevision(ParagraphItem paraItem)
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

	internal override void Close()
	{
		if (m_charFormat != null)
		{
			m_charFormat.Close();
			m_charFormat = null;
		}
		base.Close();
	}

	public IOfficeRun CloneRun()
	{
		return (IOfficeRun)CloneImpl();
	}

	public void Dispose()
	{
		Close();
	}

	protected override object CloneImpl()
	{
		ParagraphItem paragraphItem = (ParagraphItem)base.CloneImpl();
		if (m_charFormat != null)
		{
			paragraphItem.m_charFormat = new WCharacterFormat(base.Document, paragraphItem);
			paragraphItem.m_charFormat.ImportContainer(m_charFormat);
			paragraphItem.m_charFormat.CopyProperties(m_charFormat);
			if (m_charFormat.m_revisions != null)
			{
				if (base.Document.IsCloning || base.Document.IsComparing)
				{
					paragraphItem.m_charFormat.m_clonedRevisions = new List<Revision>();
					foreach (Revision revision in m_charFormat.m_revisions)
					{
						paragraphItem.m_charFormat.m_clonedRevisions.Add(revision.Clone());
					}
				}
				else if (paragraphItem.m_charFormat.PropertiesHash.Count > 0)
				{
					foreach (int revisionKey in m_charFormat.RevisionKeys)
					{
						paragraphItem.m_charFormat.PropertiesHash.Remove(revisionKey);
					}
				}
			}
		}
		paragraphItem.m_ownerMathRunElement = m_ownerMathRunElement;
		if (m_revisions != null)
		{
			if ((base.Document.IsCloning || base.Document.IsComparing) && paragraphItem.m_revisions.Count > 0)
			{
				paragraphItem.m_clonedRevisions = new List<Revision>();
				foreach (Revision revision2 in paragraphItem.m_revisions)
				{
					if (revision2.RevisionType == RevisionType.MoveFrom || revision2.RevisionType == RevisionType.MoveTo)
					{
						SetFirstAndLastItem(revision2, paragraphItem);
					}
					paragraphItem.m_clonedRevisions.Add(revision2.Clone());
				}
			}
			paragraphItem.m_revisions = new List<Revision>();
		}
		return paragraphItem;
	}

	private void SetFirstAndLastItem(Revision revision, ParagraphItem item)
	{
		ParagraphItem paragraphItem = null;
		for (int i = 0; i < revision.Range.Count; i++)
		{
			if (revision.Range.Items[i] is Entity)
			{
				paragraphItem = revision.Range.Items[i] as ParagraphItem;
				break;
			}
		}
		if (paragraphItem == this)
		{
			item.IsMoveRevisionFirstItem = true;
		}
		paragraphItem = null;
		for (int num = revision.Range.Count - 1; num >= 0; num--)
		{
			if (revision.Range.Items[num] is Entity)
			{
				paragraphItem = revision.Range.Items[num] as ParagraphItem;
				break;
			}
		}
		if (paragraphItem == this)
		{
			item.IsMoveRevisionLastItem = true;
		}
	}

	internal override void CloneRelationsTo(WordDocument doc, OwnerHolder nextOwner)
	{
		if (doc != base.Document && (doc.ImportOptions & ImportOptions.UseDestinationStyles) != 0 && doc.Sections.Count == 0 && base.Document != null && base.Document.DefCharFormat != null)
		{
			if (doc.DefCharFormat == null)
			{
				doc.DefCharFormat = new WCharacterFormat(doc);
			}
			doc.DefCharFormat.ImportContainer(base.Document.DefCharFormat);
		}
		base.CloneRelationsTo(doc, nextOwner);
		if (m_charFormat != null)
		{
			m_charFormat.CloneRelationsTo(doc, nextOwner);
		}
		if (doc == base.Document || (doc.ImportOptions & ImportOptions.UseDestinationStyles) == 0)
		{
			return;
		}
		IWParagraphStyle iWParagraphStyle = null;
		if (OwnerParagraph != null)
		{
			iWParagraphStyle = OwnerParagraph.GetStyle();
		}
		if (iWParagraphStyle != null)
		{
			ParaItemCharFormat.ApplyBase(iWParagraphStyle.CharacterFormat);
			if (this is Break)
			{
				(this as Break).CharacterFormat.ApplyBase(iWParagraphStyle.CharacterFormat);
			}
			else if (this is InlineContentControl)
			{
				(this as InlineContentControl).ApplyBaseFormat();
			}
			else if (this is WMath)
			{
				(this as WMath).ApplyBaseFormat();
			}
		}
	}

	internal TextWrappingStyle GetTextWrappingStyle()
	{
		if (this is Shape)
		{
			return (this as Shape).WrapFormat.TextWrappingStyle;
		}
		if (this is WChart)
		{
			return (this as WChart).WrapFormat.TextWrappingStyle;
		}
		if (this is WTextBox)
		{
			return (this as WTextBox).TextBoxFormat.TextWrappingStyle;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).WrapFormat.TextWrappingStyle;
		}
		return (this as WPicture).TextWrappingStyle;
	}

	internal bool IsWrappingBoundsAdded()
	{
		if (this is Shape)
		{
			return (this as Shape).WrapFormat.IsWrappingBoundsAdded;
		}
		if (this is WChart)
		{
			return (this as WChart).WrapFormat.IsWrappingBoundsAdded;
		}
		if (this is WTextBox)
		{
			return (this as WTextBox).TextBoxFormat.IsWrappingBoundsAdded;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).WrapFormat.IsWrappingBoundsAdded;
		}
		return (this as WPicture).IsWrappingBoundsAdded;
	}

	internal void SetIsWrappingBoundsAdded(bool boolean)
	{
		if (this is Shape)
		{
			(this as Shape).WrapFormat.IsWrappingBoundsAdded = boolean;
		}
		else if (this is WChart)
		{
			(this as WChart).WrapFormat.IsWrappingBoundsAdded = boolean;
		}
		else if (this is WTextBox)
		{
			(this as WTextBox).TextBoxFormat.IsWrappingBoundsAdded = boolean;
		}
		else if (this is GroupShape)
		{
			(this as GroupShape).WrapFormat.IsWrappingBoundsAdded = boolean;
		}
		else
		{
			(this as WPicture).IsWrappingBoundsAdded = boolean;
		}
	}

	internal bool GetLayOutInCell()
	{
		if (this is Shape)
		{
			return (this as Shape).LayoutInCell;
		}
		if (this is WTextBox)
		{
			return (this as WTextBox).TextBoxFormat.AllowInCell;
		}
		if (this is WChart)
		{
			return (this as WChart).LayoutInCell;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).LayoutInCell;
		}
		return (this as WPicture).LayoutInCell;
	}

	internal VerticalOrigin GetVerticalOrigin()
	{
		if (this is Shape)
		{
			return (this as Shape).VerticalOrigin;
		}
		if (this is WChart)
		{
			return (this as WChart).VerticalOrigin;
		}
		if (this is WTextBox)
		{
			return (this as WTextBox).TextBoxFormat.VerticalOrigin;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).VerticalOrigin;
		}
		return (this as WPicture).VerticalOrigin;
	}

	internal ShapeVerticalAlignment GetShapeVerticalAlignment()
	{
		if (this is Shape)
		{
			return (this as Shape).VerticalAlignment;
		}
		if (this is WChart)
		{
			return (this as WChart).VerticalAlignment;
		}
		if (this is WTextBox)
		{
			return (this as WTextBox).TextBoxFormat.VerticalAlignment;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).VerticalAlignment;
		}
		return (this as WPicture).VerticalAlignment;
	}

	internal ShapeHorizontalAlignment GetShapeHorizontalAlignment()
	{
		if (this is Shape)
		{
			return (this as Shape).HorizontalAlignment;
		}
		if (this is WChart)
		{
			return (this as WChart).HorizontalAlignment;
		}
		if (this is WTextBox)
		{
			return (this as WTextBox).TextBoxFormat.HorizontalAlignment;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).HorizontalAlignment;
		}
		return (this as WPicture).HorizontalAlignment;
	}

	internal HorizontalOrigin GetHorizontalOrigin()
	{
		if (this is Shape)
		{
			return (this as Shape).HorizontalOrigin;
		}
		if (this is WChart)
		{
			return (this as WChart).HorizontalOrigin;
		}
		if (this is WTextBox)
		{
			return (this as WTextBox).TextBoxFormat.HorizontalOrigin;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).HorizontalOrigin;
		}
		return (this as WPicture).HorizontalOrigin;
	}

	internal float GetHorizontalPosition()
	{
		if (this is Shape)
		{
			return (this as Shape).HorizontalPosition;
		}
		if (this is WChart)
		{
			return (this as WChart).HorizontalPosition;
		}
		if (this is WTextBox)
		{
			return (this as WTextBox).TextBoxFormat.HorizontalPosition;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).HorizontalPosition;
		}
		return (this as WPicture).HorizontalPosition;
	}

	internal float GetVerticalPosition()
	{
		if (this is Shape)
		{
			return (this as Shape).VerticalPosition;
		}
		if (this is WChart)
		{
			return (this as WChart).VerticalPosition;
		}
		if (this is WTextBox)
		{
			return (this as WTextBox).TextBoxFormat.VerticalPosition;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).VerticalPosition;
		}
		return (this as WPicture).VerticalPosition;
	}

	internal bool GetAllowOverlap()
	{
		if (this is Shape)
		{
			return (this as Shape).WrapFormat.AllowOverlap;
		}
		if (this is WChart)
		{
			return (this as WChart).WrapFormat.AllowOverlap;
		}
		if (this is WTextBox)
		{
			return (this as WTextBox).TextBoxFormat.AllowOverlap;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).WrapFormat.AllowOverlap;
		}
		return (this as WPicture).AllowOverlap;
	}

	internal void GetEffectExtentValues(out float leftEdgeExtent, out float rightEgeExtent, out float topEdgeExtent, out float bottomEdgeExtent)
	{
		leftEdgeExtent = 0f;
		rightEgeExtent = 0f;
		topEdgeExtent = 0f;
		bottomEdgeExtent = 0f;
		if (this is WTextBox && (this as WTextBox).GetTextWrappingStyle() == TextWrappingStyle.Inline && (this as WTextBox).Shape != null && (this as WTextBox).TextBoxFormat.Rotation == 0f)
		{
			Shape shape = (this as WTextBox).Shape;
			leftEdgeExtent = shape.LeftEdgeExtent;
			rightEgeExtent = shape.RightEdgeExtent;
			topEdgeExtent = shape.TopEdgeExtent;
			bottomEdgeExtent = shape.BottomEdgeExtent;
		}
		else if (this is Shape && (this as Shape).GetTextWrappingStyle() == TextWrappingStyle.Inline && (this as Shape).Rotation == 0f)
		{
			Shape shape2 = this as Shape;
			leftEdgeExtent = shape2.LeftEdgeExtent;
			rightEgeExtent = shape2.RightEdgeExtent;
			topEdgeExtent = shape2.TopEdgeExtent;
			bottomEdgeExtent = shape2.BottomEdgeExtent;
		}
		else if (this is GroupShape && (this as GroupShape).GetTextWrappingStyle() == TextWrappingStyle.Inline && (this as GroupShape).Rotation == 0f)
		{
			GroupShape groupShape = this as GroupShape;
			leftEdgeExtent = groupShape.LeftEdgeExtent;
			rightEgeExtent = groupShape.RightEdgeExtent;
			topEdgeExtent = groupShape.TopEdgeExtent;
			bottomEdgeExtent = groupShape.BottomEdgeExtent;
		}
	}

	private float GetLeftMargin(WSection section)
	{
		return section.PageSetup.Margins.Left + (section.Document.DOP.GutterAtTop ? 0f : section.PageSetup.Margins.Gutter);
	}

	private float GetRightMargin(WSection section)
	{
		return section.PageSetup.Margins.Right;
	}

	private new Entity GetBaseEntity(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			if (entity2 == null || entity2.Owner == null)
			{
				return entity2;
			}
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WSection));
		return entity2;
	}

	internal float GetWidthRelativeToPercent(bool isDocToPdf)
	{
		Entity baseEntity = GetBaseEntity(this);
		if (baseEntity is WSection)
		{
			WPageSetup pageSetup = (baseEntity as WSection).PageSetup;
			WTextBox wTextBox = this as WTextBox;
			WidthOrigin widthOrigin = WidthOrigin.Page;
			float num = 0f;
			float num2 = 0f;
			if (wTextBox == null)
			{
				if (this is Shape shape)
				{
					widthOrigin = shape.TextFrame.WidthOrigin;
					num = shape.TextFrame.WidthRelativePercent;
					if (isDocToPdf && shape.LineFormat.Line)
					{
						num2 = shape.LineFormat.Weight;
					}
				}
			}
			else
			{
				widthOrigin = wTextBox.TextBoxFormat.WidthOrigin;
				num = wTextBox.TextBoxFormat.WidthRelativePercent;
				if (isDocToPdf && !wTextBox.TextBoxFormat.NoLine)
				{
					num2 = ((wTextBox.TextBoxFormat.LineWidth >= 2f) ? wTextBox.TextBoxFormat.LineWidth : 2f);
				}
			}
			float num3;
			switch (widthOrigin)
			{
			case WidthOrigin.Page:
				num3 = pageSetup.PageSize.Width * (num / 100f);
				break;
			case WidthOrigin.LeftMargin:
			case WidthOrigin.InsideMargin:
				num3 = (isDocToPdf ? GetLeftMargin(baseEntity as WSection) : (pageSetup.Margins.Left + (baseEntity.Document.DOP.GutterAtTop ? 0f : pageSetup.Margins.Gutter))) * (num / 100f);
				break;
			case WidthOrigin.RightMargin:
			case WidthOrigin.OutsideMargin:
				num3 = (isDocToPdf ? GetRightMargin(baseEntity as WSection) : pageSetup.Margins.Right) * (num / 100f);
				break;
			default:
				num3 = pageSetup.ClientWidth * (num / 100f);
				break;
			}
			if (num2 != 0f)
			{
				num3 -= num2;
			}
			return num3;
		}
		if (!(this is Shape))
		{
			return (this as WTextBox).TextBoxFormat.Width;
		}
		return (this as Shape).Width;
	}

	internal float GetHeightRelativeToPercent(bool isDocToPdf)
	{
		Entity baseEntity = GetBaseEntity(this);
		if (baseEntity is WSection)
		{
			WPageSetup pageSetup = (baseEntity as WSection).PageSetup;
			WTextBox wTextBox = this as WTextBox;
			HeightOrigin heightOrigin = HeightOrigin.Page;
			float num = 0f;
			float num2 = 0f;
			if (wTextBox == null)
			{
				if (this is Shape shape)
				{
					heightOrigin = shape.TextFrame.HeightOrigin;
					num = shape.TextFrame.HeightRelativePercent;
					if (isDocToPdf && shape.LineFormat.Line)
					{
						num2 = shape.LineFormat.Weight;
					}
				}
			}
			else
			{
				heightOrigin = wTextBox.TextBoxFormat.HeightOrigin;
				num = wTextBox.TextBoxFormat.HeightRelativePercent;
				if (isDocToPdf && !wTextBox.TextBoxFormat.NoLine)
				{
					num2 = ((wTextBox.TextBoxFormat.LineWidth >= 2f) ? wTextBox.TextBoxFormat.LineWidth : 2f);
				}
			}
			float num3;
			switch (heightOrigin)
			{
			case HeightOrigin.Page:
				num3 = pageSetup.PageSize.Height * (num / 100f);
				break;
			case HeightOrigin.TopMargin:
			case HeightOrigin.InsideMargin:
				num3 = (pageSetup.Margins.Top + (baseEntity.Document.DOP.GutterAtTop ? pageSetup.Margins.Gutter : 0f)) * (num / 100f);
				break;
			case HeightOrigin.BottomMargin:
			case HeightOrigin.OutsideMargin:
				num3 = pageSetup.Margins.Bottom * (num / 100f);
				break;
			default:
				num3 = (pageSetup.PageSize.Height - pageSetup.Margins.Top - (baseEntity.Document.DOP.GutterAtTop ? pageSetup.Margins.Gutter : 0f) - pageSetup.Margins.Bottom) * (num / 100f);
				break;
			}
			if (num2 != 0f)
			{
				num3 -= num2;
			}
			return num3;
		}
		if (!(this is Shape))
		{
			return (this as WTextBox).TextBoxFormat.Height;
		}
		return (this as Shape).Height;
	}

	internal int GetWrapCollectionIndex()
	{
		if (this is Shape)
		{
			return (this as Shape).WrapFormat.WrapCollectionIndex;
		}
		if (this is WChart)
		{
			return (this as WChart).WrapFormat.WrapCollectionIndex;
		}
		if (this is WTextBox)
		{
			return (this as WTextBox).TextBoxFormat.WrapCollectionIndex;
		}
		if (this is GroupShape)
		{
			return (this as GroupShape).WrapFormat.WrapCollectionIndex;
		}
		return (this as WPicture).WrapCollectionIndex;
	}

	internal List<Path2D> Parse2007CustomShapePoints(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return null;
		}
		List<char> list = new List<char>();
		list.Add('m');
		list.Add('l');
		list.Add('x');
		list.Add('e');
		list.Add('t');
		list.Add('r');
		List<char> list2 = new List<char>();
		list2.Add(',');
		list2.Add(' ');
		List<char> list3 = new List<char>();
		list3.Add('-');
		list3.Add('+');
		if (IsValidVMLPath(path, list, list2, list3))
		{
			List<Path2D> list4 = ParseVMLPath(path, list, list2, list3);
			if (list4.Count > 0)
			{
				return list4;
			}
		}
		return null;
	}

	private List<Path2D> ParseVMLPath(string path, List<char> VMLCommands, List<char> VMLSeparators, List<char> VMLOperators)
	{
		List<Path2D> list = new List<Path2D>();
		int index = 0;
		while (index < path.Length)
		{
			SkipWhiteSpaces(path, ref index);
			if (index >= path.Length || (list.Count == 0 && !VMLCommands.Contains(path[index])))
			{
				return list;
			}
			string pathCommandType = path[index].ToString();
			index++;
			int nextIndexOfPathCommand = GetNextIndexOfPathCommand(path, index, VMLCommands);
			string text = path.Substring(index, nextIndexOfPathCommand - index);
			List<PointF> list2 = new List<PointF>();
			int index2 = 0;
			while (index2 < text.Length)
			{
				SkipWhiteSpaces(text, ref index2);
				float pathPoint = GetPathPoint(text, ref index2, VMLOperators);
				index2++;
				SkipWhiteSpaces(text, ref index2);
				float pathPoint2 = GetPathPoint(text, ref index2, VMLOperators);
				SkipWhiteSpaces(text, ref index2);
				PointF item = new PointF(pathPoint, pathPoint2);
				list2.Add(item);
				if (index2 < text.Length && VMLSeparators.Contains(text[index2]))
				{
					index2++;
				}
			}
			index += text.Length;
			list.Add(new Path2D(pathCommandType, list2));
		}
		return list;
	}

	private void SkipWhiteSpaces(string path, ref int index)
	{
		while (index < path.Length && char.IsWhiteSpace(path[index]))
		{
			index++;
		}
	}

	private float GetPathPoint(string path, ref int index, List<char> VMLOperators)
	{
		float result = 0f;
		string text = null;
		while (index < path.Length && (char.IsNumber(path[index]) || VMLOperators.Contains(path[index])))
		{
			text += path[index];
			index++;
		}
		if (text != null && float.TryParse(text, out result))
		{
			result = float.Parse(text);
		}
		return result;
	}

	private int GetNextIndexOfPathCommand(string path, int startIndex, List<char> VMLCommands)
	{
		for (int i = startIndex; i < path.Length; i++)
		{
			if (VMLCommands.Contains(path[i]))
			{
				return i;
			}
		}
		return path.Length;
	}

	private bool IsValidVMLPath(string path, List<char> VMLCommands, List<char> VMLSeparators, List<char> VMLOperators)
	{
		foreach (char c in path)
		{
			if (!char.IsNumber(c) && !VMLCommands.Contains(c) && !VMLSeparators.Contains(c) && !VMLOperators.Contains(c))
			{
				return false;
			}
		}
		return true;
	}

	internal void UpdateVMLPathPoints(RectangleF bounds, string path, PointF coordinateOrgin, string coordinateSize, List<Path2D> vmlPoints, bool isUpdated)
	{
		if (isUpdated)
		{
			vmlPoints = Parse2007CustomShapePoints(path);
		}
		SizeF sizeF = new SizeF(1000f, 1000f);
		if (coordinateSize != null)
		{
			string[] array = coordinateSize.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 2)
			{
				if (float.TryParse(array[0].Trim(), out var result))
				{
					float num = float.Parse(array[0].Trim());
					if (num > 0f)
					{
						sizeF.Width = num;
					}
				}
				if (float.TryParse(array[1].Trim(), out result))
				{
					float num2 = float.Parse(array[1].Trim());
					if (num2 > 0f)
					{
						sizeF.Height = num2;
					}
				}
			}
		}
		SizeF sizeF2 = default(SizeF);
		sizeF2.Width = bounds.Width / sizeF.Width;
		sizeF2.Height = bounds.Height / sizeF.Height;
		PointF pointF = default(PointF);
		pointF.X = bounds.X - sizeF2.Width * coordinateOrgin.X;
		pointF.Y = bounds.Y - sizeF2.Height * coordinateOrgin.Y;
		PointF pointF2 = default(PointF);
		foreach (Path2D vmlPoint in vmlPoints)
		{
			switch (vmlPoint.PathCommandType)
			{
			case "l":
				if (vmlPoint.PathPoints.Count > 0)
				{
					pointF2 = vmlPoint.PathPoints[vmlPoint.PathPoints.Count - 1];
				}
				break;
			case "r":
			case "t":
				if (vmlPoint.PathPoints.Count > 0)
				{
					for (int i = 0; i < vmlPoint.PathPoints.Count; i++)
					{
						float x = vmlPoint.PathPoints[i].X + pointF2.X;
						float y = vmlPoint.PathPoints[i].Y + pointF2.Y;
						vmlPoint.PathPoints[i] = new PointF(x, y);
						pointF2 = vmlPoint.PathPoints[i];
					}
				}
				break;
			case "m":
				if (vmlPoint.PathPoints.Count > 0)
				{
					pointF2 = vmlPoint.PathPoints[vmlPoint.PathPoints.Count - 1];
				}
				break;
			}
		}
		foreach (Path2D vmlPoint2 in vmlPoints)
		{
			List<PointF> pathPoints = vmlPoint2.PathPoints;
			for (int j = 0; j < pathPoints.Count; j++)
			{
				PointF pointF3 = pathPoints[j];
				PointF value = default(PointF);
				value.X = pointF3.X * sizeF2.Width + pointF.X;
				value.Y = pointF3.Y * sizeF2.Height + pointF.Y;
				pathPoints[j] = value;
			}
		}
	}

	internal void ReUpdateVMLPathPoints(float xOffset, float yOffset, List<Path2D> vmlPoints)
	{
		foreach (Path2D vmlPoint in vmlPoints)
		{
			List<PointF> pathPoints = vmlPoint.PathPoints;
			for (int i = 0; i < pathPoints.Count; i++)
			{
				PointF pointF = pathPoints[i];
				PointF value = default(PointF);
				value.X = pointF.X + xOffset;
				value.Y = pointF.Y + yOffset;
				pathPoints[i] = value;
			}
		}
	}
}
