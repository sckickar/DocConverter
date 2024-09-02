using System;
using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.DLS;

public class Revision
{
	private string m_author = string.Empty;

	private string m_name = string.Empty;

	private DateTime m_date;

	private Range m_range;

	private RevisionType m_revisionType;

	private RevisionCollection m_childRevisions;

	private object m_ownerBase;

	private WordDocument m_doc;

	private byte m_bFlags;

	public string Author
	{
		get
		{
			return m_author;
		}
		internal set
		{
			m_author = value;
		}
	}

	public DateTime Date
	{
		get
		{
			return m_date;
		}
		internal set
		{
			m_date = value;
		}
	}

	internal Range Range
	{
		get
		{
			if (m_range == null)
			{
				m_range = new Range();
			}
			return m_range;
		}
	}

	public RevisionType RevisionType
	{
		get
		{
			return m_revisionType;
		}
		internal set
		{
			m_revisionType = value;
		}
	}

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal RevisionCollection ChildRevisions
	{
		get
		{
			if (m_childRevisions == null)
			{
				m_childRevisions = new RevisionCollection(m_doc);
			}
			return m_childRevisions;
		}
	}

	internal object Owner
	{
		get
		{
			return m_ownerBase;
		}
		set
		{
			m_ownerBase = value;
		}
	}

	internal bool IsAfterParagraphMark
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

	internal bool IsAfterCellMark
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool IsAfterRowMark
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool IsAfterTableMark
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

	public void Accept()
	{
		List<WCharacterFormat> list = new List<WCharacterFormat>();
		if (m_range == null)
		{
			RemoveSelf();
		}
		RevisionType revisionType = RevisionType;
		if (revisionType <= RevisionType.Formatting)
		{
			if (revisionType == RevisionType.Insertions)
			{
				goto IL_01ff;
			}
			if (revisionType == RevisionType.Deletions)
			{
				goto IL_0454;
			}
			if (revisionType == RevisionType.Formatting)
			{
				goto IL_04d5;
			}
		}
		else
		{
			if (revisionType == RevisionType.StyleDefinitionChange)
			{
				goto IL_04d5;
			}
			if (revisionType == RevisionType.MoveFrom)
			{
				goto IL_0454;
			}
			if (revisionType == RevisionType.MoveTo)
			{
				goto IL_01ff;
			}
		}
		goto IL_04e3;
		IL_01ff:
		while (Range.Count > 0)
		{
			if (Range.Items[0] is Entity)
			{
				RevisionCollection revisions = (Range.Items[0] as Entity).Document.Revisions;
				List<Revision> revisionsInternal = (Range.Items[0] as Entity).RevisionsInternal;
				if ((Range.Items[0] as Entity).RevisionsInternal.Count == 0)
				{
					Range.Items.Remove(Range.Items[0] as Entity);
					if (Range.Count == 0 && revisions.InnerList.Contains(this))
					{
						revisions.Remove(this);
					}
				}
				else
				{
					UnlinkRangeItem(Range.Items[0] as Entity, this, isFromAccept: true);
				}
				for (int i = 0; i < revisionsInternal.Count; i++)
				{
					if (revisionsInternal[i].Range.Items.Count == 0 && revisions.InnerList.Contains(revisionsInternal[i]))
					{
						revisions.Remove(revisionsInternal[i]);
					}
				}
			}
			else
			{
				if (Range.Items[0] is WCharacterFormat && (Range.Items[0] as WCharacterFormat).OwnerBase is WParagraph)
				{
					list.Add(Range.Items[0] as WCharacterFormat);
				}
				UnlinkRangeItem(Range.Items[0] as FormatBase, this, isFromAccept: true);
			}
		}
		foreach (WCharacterFormat item in list)
		{
			MakeChangesForBreakCharFormat(item.OwnerBase as WParagraph, acceptChanges: true);
		}
		if (RevisionType == RevisionType.MoveTo)
		{
			ClearDependentRevision(isFromAccept: true);
		}
		goto IL_04e3;
		IL_0454:
		while (Range.Count > 0)
		{
			if (Range.Items[0] is Entity)
			{
				RevisionCollection revisions2 = (Range.Items[0] as Entity).Document.Revisions;
				List<Revision> revisionsInternal2 = (Range.Items[0] as Entity).RevisionsInternal;
				bool flag = false;
				while (revisionsInternal2.Count > 0 && Range.Items.Count > 0 && Range.Items[0] is Entity)
				{
					UnlinkRangeItem(Range.Items[0] as Entity, this, isFromAccept: true);
					flag = true;
				}
				if (!flag && revisionsInternal2.Count == 0 && Range.Items.Count > 0)
				{
					Range.Items.Remove(Range.Items[0] as Entity);
					if (Range.Count == 0 && revisions2.InnerList.Contains(this))
					{
						revisions2.Remove(this);
					}
				}
				for (int j = 0; j < revisionsInternal2.Count; j++)
				{
					if (revisionsInternal2[j].Range.Items.Count == 0 && revisions2.InnerList.Contains(revisionsInternal2[j]))
					{
						revisions2.Remove(revisionsInternal2[j]);
					}
				}
			}
			else
			{
				if (Range.Items[0] is WCharacterFormat && (Range.Items[0] as WCharacterFormat).OwnerBase is WParagraph)
				{
					list.Add(Range.Items[0] as WCharacterFormat);
				}
				UnlinkRangeItem(Range.Items[0] as FormatBase, this, isFromAccept: true);
			}
		}
		foreach (WCharacterFormat item2 in list)
		{
			MakeChangesForBreakCharFormat(item2.OwnerBase as WParagraph, acceptChanges: true);
		}
		if (RevisionType == RevisionType.MoveFrom)
		{
			ClearDependentRevision(isFromAccept: true);
		}
		goto IL_04e3;
		IL_04e3:
		list.Clear();
		return;
		IL_04d5:
		while (Range.Count > 0)
		{
			UnlinkRangeItem(Range.Items[0] as FormatBase, this, isFromAccept: true);
		}
		goto IL_04e3;
	}

	private void ClearDependentRevision(bool isFromAccept)
	{
		if (RevisionType != RevisionType.MoveFrom && RevisionType != RevisionType.MoveTo)
		{
			return;
		}
		Revision dependentName = GetDependentName();
		if (dependentName != null)
		{
			if (isFromAccept)
			{
				dependentName.Accept();
			}
			else
			{
				dependentName.Reject();
			}
		}
	}

	private Revision GetDependentName()
	{
		if (!(Owner is RevisionCollection revisionCollection))
		{
			return null;
		}
		foreach (Revision item in revisionCollection)
		{
			if (item.Name == Name)
			{
				return item;
			}
		}
		return null;
	}

	private void UnlinkRangeItem(Entity entity, Revision revision, bool isFromAccept)
	{
		entity.RevisionsInternal.Remove(revision);
		revision.Range.Items.Remove(entity);
		RemoveItemFromCollectionn(revision, entity, isFromAccept);
		MakeChanges(revision, entity as ParagraphItem, isFromAccept);
		if (revision.Range.Count == 0)
		{
			OnClearComplete(revision, entity, isFromAccept);
		}
	}

	private void RemoveItemFromCollectionn(Revision revision, Entity entity, bool isFromAccept)
	{
		if ((revision.RevisionType != RevisionType.Insertions || isFromAccept) && !(revision.RevisionType == RevisionType.Deletions && isFromAccept))
		{
			return;
		}
		for (int i = 0; i < entity.RevisionsInternal.Count; i++)
		{
			if (entity.RevisionsInternal[i].Range.Count > 0 && entity.RevisionsInternal[i].Range.Items.Contains(entity))
			{
				entity.RevisionsInternal[i].Range.Items.Remove(entity);
			}
			if (entity.RevisionsInternal[i].Range.Count == 0)
			{
				OnClearComplete(entity.RevisionsInternal[i], entity, isFromAccept);
			}
		}
	}

	private void RemoveItemFromCollectionn(Revision revision, FormatBase formatBase, bool isFromAccept)
	{
		if ((revision.RevisionType != RevisionType.Insertions || isFromAccept) && !(revision.RevisionType == RevisionType.Deletions && isFromAccept))
		{
			return;
		}
		for (int i = 0; i < formatBase.Revisions.Count; i++)
		{
			if (formatBase.Revisions[i].Range.Count > 0 && formatBase.Revisions[i].Range.Items.Contains(formatBase))
			{
				formatBase.Revisions[i].Range.Items.Remove(formatBase);
			}
			if (formatBase.Revisions[i].Range.Count == 0)
			{
				OnClearComplete(formatBase.Revisions[i], formatBase, isFromAccept);
			}
		}
	}

	private void MakeChanges(Revision revision, ParagraphItem item, bool acceptChanges)
	{
		if (IsToRemove(revision, acceptChanges))
		{
			item.RemoveSelf();
			return;
		}
		if (item.IsChangedCFormat && !acceptChanges)
		{
			item.RemoveChanges();
		}
		if (item is Break)
		{
			if ((acceptChanges && (item as Break).CharacterFormat.IsDeleteRevision) || (!acceptChanges && (item as Break).CharacterFormat.IsInsertRevision))
			{
				item.RemoveSelf();
			}
			else
			{
				(item as Break).TextRange.AcceptChanges();
			}
		}
		item.AcceptChanges();
	}

	private void MakeChanges(FormatBase format, bool acceptChanges)
	{
		if (format is RowFormat && format.OwnerBase is WTableRow)
		{
			MakeChanges(format.OwnerBase as WTableRow, acceptChanges);
		}
		else if (format is RowFormat && format.OwnerBase is WTable)
		{
			MakeChanges(format.OwnerBase as WTable, acceptChanges);
		}
		else if (format is CellFormat)
		{
			MakeChanges(format.OwnerBase as WTableCell, acceptChanges);
		}
		else if (format is WCharacterFormat)
		{
			MakeChanges(format as WCharacterFormat, acceptChanges);
		}
		else if (format is WParagraphFormat)
		{
			MakeChanges(format as WParagraphFormat, acceptChanges);
		}
		else if (format is WSectionFormat)
		{
			MakeChanges(format as WSectionFormat, acceptChanges);
		}
	}

	private void MakeChanges(WSectionFormat sectionFormat, bool acceptChanges)
	{
		WSection wSection = sectionFormat.OwnerBase as WSection;
		if (wSection.m_internalData != null && wSection.m_internalData.Length < 300 && wSection.m_internalData.Length != 0)
		{
			SinglePropertyModifierArray singlePropertyModifierArray = new SinglePropertyModifierArray(wSection.m_internalData, 0);
			if (singlePropertyModifierArray.TryGetSprm(12857) != null)
			{
				while (singlePropertyModifierArray.TryGetSprm(12857) != null)
				{
					singlePropertyModifierArray.Modifiers.Remove(singlePropertyModifierArray.Modifiers[(!acceptChanges) ? (singlePropertyModifierArray.Count - 1) : 0]);
				}
				wSection.m_internalData = new byte[wSection.m_internalData.Length];
				singlePropertyModifierArray.Save(wSection.m_internalData, 0);
			}
		}
		Dictionary<int, object> oldFormatBase = new Dictionary<int, object>(wSection.SectionFormat.OldPropertiesHash);
		Dictionary<int, object> oldFormatBase2 = new Dictionary<int, object>(wSection.PageSetup.OldPropertiesHash);
		Dictionary<int, object> oldFormatBase3 = new Dictionary<int, object>(wSection.PageSetup.PageNumbers.OldPropertiesHash);
		Dictionary<int, object> oldFormatBase4 = new Dictionary<int, object>(wSection.PageSetup.Margins.OldPropertiesHash);
		if (sectionFormat.PageSetup != null)
		{
			MakeChangesInPropertiesHash(sectionFormat.PageSetup, acceptChanges, oldFormatBase2);
			if (sectionFormat.PageSetup.Margins != null)
			{
				MakeChangesInPropertiesHash(sectionFormat.PageSetup.Margins, acceptChanges, oldFormatBase4);
			}
			if (sectionFormat.PageSetup.PageNumbers != null)
			{
				MakeChangesInPropertiesHash(sectionFormat.PageSetup.PageNumbers, acceptChanges, oldFormatBase3);
			}
		}
		if (sectionFormat.Columns != null && sectionFormat.Columns.Count > 0)
		{
			if (acceptChanges)
			{
				if (wSection.SectionFormat.SectFormattingColumnCollection != null)
				{
					sectionFormat.SectFormattingColumnCollection.InnerList.Clear();
				}
			}
			else if (sectionFormat.SectFormattingColumnCollection != null)
			{
				wSection.Columns.InnerList.Clear();
				for (int i = 0; i < wSection.SectionFormat.SectFormattingColumnCollection.Count; i++)
				{
					Column column = new Column(wSection.Document);
					wSection.Columns.Add(column);
					foreach (KeyValuePair<int, object> item in wSection.SectionFormat.SectFormattingColumnCollection[i].OldPropertiesHash)
					{
						wSection.Columns[i].PropertiesHash[item.Key] = item.Value;
					}
				}
			}
		}
		MakeChangesInPropertiesHash(sectionFormat, acceptChanges, oldFormatBase);
		if (acceptChanges)
		{
			sectionFormat.PropertiesHash.Remove(4);
			return;
		}
		sectionFormat.PropertiesHash.Remove(5);
		sectionFormat.PropertiesHash.Remove(6);
	}

	private void MakeChangesInPropertiesHash(FormatBase formatBase, bool acceptChanges, Dictionary<int, object> oldFormatBase)
	{
		if (acceptChanges)
		{
			formatBase.AcceptChanges();
			if (formatBase.OldPropertiesHash.Count > 0)
			{
				formatBase.OldPropertiesHash.Clear();
			}
			return;
		}
		formatBase.RemoveChanges();
		formatBase.PropertiesHash.Clear();
		formatBase.OldPropertiesHash.Clear();
		foreach (KeyValuePair<int, object> item in oldFormatBase)
		{
			formatBase.PropertiesHash.Add(item.Key, item.Value);
		}
	}

	private void MakeChanges(WCharacterFormat format, bool acceptChanges)
	{
		if (acceptChanges)
		{
			if (RevisionType == RevisionType.Formatting)
			{
				object obj = null;
				object obj2 = null;
				if (format.PropertiesHash.ContainsKey(103))
				{
					obj = format.PropertiesHash[103];
				}
				if (format.PropertiesHash.ContainsKey(104))
				{
					obj2 = format.PropertiesHash[104];
				}
				format.AcceptChanges();
				if (obj != null)
				{
					format.PropertiesHash[103] = obj;
				}
				if (obj2 != null)
				{
					format.PropertiesHash[104] = obj2;
				}
			}
			else
			{
				format.AcceptChanges();
			}
			return;
		}
		format.RemoveChanges();
		if (format.PropertiesHash.ContainsKey(103))
		{
			format.OldPropertiesHash[103] = format.PropertiesHash[103];
		}
		format.PropertiesHash.Clear();
		foreach (KeyValuePair<int, object> item in format.OldPropertiesHash)
		{
			format.PropertiesHash[item.Key] = item.Value;
		}
		format.OldPropertiesHash.Clear();
	}

	private void MakeChangesForBreakCharFormat(TextBodyItem item, bool acceptChanges)
	{
		if (!RemoveChangedItem(item, acceptChanges))
		{
			bool num = CheckMoveToNext(item, acceptChanges);
			if (!acceptChanges)
			{
				RemoveChangedFormat(item);
			}
			if (item.IsInsertRevision || item.IsDeleteRevision || item.IsChangedCFormat)
			{
				item.AcceptCChanges();
			}
			if (item.IsChangedPFormat)
			{
				item.AcceptPChanges();
			}
			if (num && MoveToNextPara(item))
			{
				item.RemoveSelf();
			}
		}
	}

	private void MakeChanges(WParagraphFormat format, bool acceptChanges)
	{
		WListFormat wListFormat = null;
		if (format.OwnerBase is WParagraph)
		{
			wListFormat = (format.OwnerBase as WParagraph).ListFormat;
		}
		else if (format.OwnerBase is WParagraphStyle)
		{
			wListFormat = (format.OwnerBase as WParagraphStyle).ListFormat;
		}
		if (acceptChanges && wListFormat != null)
		{
			if (wListFormat.OldPropertiesHash != null && wListFormat.OldPropertiesHash.Count > 0)
			{
				wListFormat.OldPropertiesHash.Clear();
			}
			if (format.m_unParsedSprms != null && format.m_unParsedSprms.Count > 0)
			{
				if (format.m_unParsedSprms[50757] != null)
				{
					format.m_unParsedSprms.RemoveValue(50757);
				}
				if (format.m_unParsedSprms[9283] != null)
				{
					format.m_unParsedSprms.RemoveValue(9283);
				}
			}
		}
		if (acceptChanges)
		{
			format.AcceptChanges();
			return;
		}
		format.RemoveChanges();
		format.PropertiesHash.Clear();
		foreach (KeyValuePair<int, object> item in format.OldPropertiesHash)
		{
			format.PropertiesHash[item.Key] = item.Value;
		}
		if (format.OldPropertiesHash.ContainsKey(47))
		{
			string name = m_doc.StyleNameIds[(string)format.OldPropertiesHash[47]];
			if (m_doc.Styles.FindByName(name, StyleType.ParagraphStyle) is IWParagraphStyle style && format.OwnerBase is WParagraph)
			{
				(format.OwnerBase as WParagraph).ApplyStyle(style, isDomChanges: false);
			}
		}
		format.OldPropertiesHash.Clear();
		if (wListFormat == null)
		{
			return;
		}
		wListFormat.PropertiesHash.Clear();
		foreach (KeyValuePair<int, object> item2 in wListFormat.OldPropertiesHash)
		{
			wListFormat.PropertiesHash[item2.Key] = item2.Value;
		}
		wListFormat.OldPropertiesHash.Clear();
	}

	private void MakeChanges(WTable table, bool acceptChanges)
	{
		if (acceptChanges)
		{
			table.m_trackTableGrid = null;
			if (table.DocxTableFormat.Format.OldPropertiesHash.Count > 0)
			{
				table.DocxTableFormat.Format.OldPropertiesHash.Clear();
			}
		}
		if (!acceptChanges && table.m_trackTableGrid != null)
		{
			table.TableGrid.InnerList.Clear();
			foreach (WTableColumn item in table.TrackTableGrid)
			{
				table.TableGrid.AddColumns(item.EndOffset);
			}
			table.m_trackTableGrid = null;
		}
		if (acceptChanges)
		{
			table.DocxTableFormat.Format.AcceptChanges();
			return;
		}
		table.DocxTableFormat.Format.RemoveChanges();
		table.DocxTableFormat.Format.PropertiesHash.Clear();
		foreach (KeyValuePair<int, object> item2 in table.DocxTableFormat.Format.OldPropertiesHash)
		{
			table.DocxTableFormat.Format.PropertiesHash[item2.Key] = item2.Value;
		}
		table.DocxTableFormat.Format.OldPropertiesHash.Clear();
	}

	private void MakeChanges(WTableRow row, bool acceptChanges)
	{
		if (acceptChanges)
		{
			row.RowFormat.AcceptChanges();
		}
		else
		{
			row.RowFormat.RemoveChanges();
			row.RowFormat.PropertiesHash.Clear();
			foreach (KeyValuePair<int, object> item in row.RowFormat.OldPropertiesHash)
			{
				row.RowFormat.PropertiesHash[item.Key] = item.Value;
			}
			row.RowFormat.OldPropertiesHash.Clear();
		}
		if (acceptChanges)
		{
			row.m_trackRowFormat = null;
			if (row.RowFormat.OldPropertiesHash.Count > 0)
			{
				row.RowFormat.OldPropertiesHash.Clear();
			}
		}
		else if (row.m_trackRowFormat != null)
		{
			row.RowFormat.ClearFormatting();
			row.m_trackRowFormat = null;
		}
		if ((row.IsDeleteRevision && acceptChanges) || (row.IsInsertRevision && !acceptChanges))
		{
			if (row.OwnerTable != null && row.OwnerTable.ChildEntities.Count == 1)
			{
				row.OwnerTable.RemoveSelf();
			}
			else
			{
				row.RemoveSelf();
			}
		}
		else if (row.IsInsertRevision && acceptChanges)
		{
			row.IsInsertRevision = false;
		}
		else if (row.IsDeleteRevision && !acceptChanges)
		{
			row.IsDeleteRevision = false;
		}
	}

	private void MakeChanges(WTableCell cell, bool acceptChanges)
	{
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
		if (acceptChanges)
		{
			cell.CellFormat.AcceptChanges();
			return;
		}
		cell.CellFormat.RemoveChanges();
		if (cell.CellFormat.PropertiesHash.ContainsKey(14) && cell.CellFormat.OldPropertiesHash.ContainsKey(14) && cell.CellFormat.PropertiesHash.ContainsKey(12) && cell.CellFormat.PropertiesHash[14].Equals(cell.CellFormat.OldPropertiesHash[14]))
		{
			cell.CellFormat.OldPropertiesHash[12] = cell.CellFormat.PropertiesHash[12];
		}
		else if (cell.OwnerRow.OwnerTable != null)
		{
			cell.OwnerRow.OwnerTable.IsTableGridUpdated = false;
		}
		cell.CellFormat.PropertiesHash.Clear();
		foreach (KeyValuePair<int, object> item in cell.CellFormat.OldPropertiesHash)
		{
			cell.CellFormat.PropertiesHash[item.Key] = item.Value;
		}
		cell.CellFormat.OldPropertiesHash.Clear();
	}

	private bool RemoveChangedItem(TextBodyItem item, bool acceptChanges)
	{
		if (IsToRemove(this, acceptChanges) && item is WParagraph)
		{
			WParagraph wParagraph = item as WParagraph;
			if (wParagraph.Owner != null && wParagraph.IsEndOfSection)
			{
				MoveToNextSection(wParagraph.GetOwnerSection());
			}
			if (wParagraph.ChildEntities.Count == 0)
			{
				wParagraph.RemoveSelf();
				return true;
			}
		}
		return false;
	}

	private bool IsToRemove(Revision revision, bool acceptChanges)
	{
		if ((revision.RevisionType != RevisionType.Insertions || acceptChanges) && !(revision.RevisionType == RevisionType.Deletions && acceptChanges) && !(revision.RevisionType == RevisionType.MoveFrom && acceptChanges))
		{
			if (revision.RevisionType == RevisionType.MoveTo)
			{
				return !acceptChanges;
			}
			return false;
		}
		return true;
	}

	private void MoveToNextSection(WSection currSection)
	{
		if (!(currSection.NextSibling is WSection wSection))
		{
			return;
		}
		int num = 0;
		WParagraph wParagraph = new WParagraph(m_doc);
		wSection.Body.ChildEntities.Insert(0, wParagraph);
		while (currSection.Body.Count > 0)
		{
			if (wParagraph.ChildEntities.Count > 0)
			{
				if (currSection.Body.ChildEntities[0] is WParagraph)
				{
					WParagraph wParagraph2 = currSection.Body.ChildEntities[0] as WParagraph;
					while (wParagraph2.Items.Count > 0)
					{
						wParagraph.ChildEntities.Add(wParagraph2.Items[0]);
					}
					wParagraph2.RemoveSelf();
				}
				num = wSection.Body.ChildEntities.IndexOf(wParagraph) + 1;
				wParagraph = new WParagraph(m_doc);
				wSection.Body.ChildEntities.Insert(num, wParagraph);
			}
			else
			{
				wSection.Body.ChildEntities.Insert(num, currSection.Body.ChildEntities[0]);
				num++;
			}
		}
		if (wParagraph.ChildEntities.Count == 0)
		{
			wParagraph.RemoveSelf();
		}
		currSection.RemoveSelf();
	}

	private bool CheckMoveToNext(TextBodyItem item, bool acceptChanges)
	{
		bool result = false;
		if (item is WParagraph && item.NextSibling is WParagraph && ((item.IsInsertRevision && !acceptChanges) || (item.IsDeleteRevision && acceptChanges)))
		{
			result = true;
		}
		return result;
	}

	private bool MoveToNextPara(TextBodyItem item)
	{
		if (item is WParagraph wParagraph)
		{
			_ = wParagraph.Items.Count;
			if (!(item.NextSibling is WParagraph wParagraph2))
			{
				return false;
			}
			if (wParagraph2.ChildEntities.Count == 0)
			{
				while (wParagraph.Items.Count > 0)
				{
					wParagraph2.ChildEntities.Add(wParagraph.Items[0]);
				}
			}
			else
			{
				Entity entity = wParagraph2.ChildEntities[0];
				while (wParagraph.Items.Count > 0)
				{
					int index = wParagraph2.ChildEntities.IndexOf(entity);
					wParagraph2.Items.Insert(index, wParagraph.ChildEntities[0]);
				}
			}
		}
		return true;
	}

	private void RemoveChangedFormat(TextBodyItem item)
	{
		if (item.IsChangedCFormat)
		{
			item.RemoveCFormatChanges();
		}
		if (item.IsChangedPFormat)
		{
			item.RemovePFormatChanges();
		}
	}

	private void UnlinkRangeItem(FormatBase formatBase, Revision revision, bool isFromAccept)
	{
		formatBase.Revisions.Remove(revision);
		revision.Range.Items.Remove(formatBase);
		RemoveItemFromCollectionn(revision, formatBase, isFromAccept);
		if (!(formatBase is WCharacterFormat) || !(formatBase.OwnerBase is WParagraph) || revision.RevisionType == RevisionType.Formatting)
		{
			MakeChanges(formatBase, isFromAccept);
		}
		if (revision.Range.Count == 0)
		{
			OnClearComplete(revision, formatBase, isFromAccept);
		}
	}

	private void OnClearComplete(Revision revision, object item, bool isFromAccept)
	{
		if (revision.ChildRevisions.Count > 0)
		{
			if (revision.RevisionType == RevisionType.MoveFrom || revision.RevisionType == RevisionType.MoveTo)
			{
				for (int i = 0; i < revision.ChildRevisions.Count; i++)
				{
					while (revision.ChildRevisions[i].Range.Count > 0)
					{
						if (revision.ChildRevisions[i].Range.Items[0] is Entity)
						{
							UnlinkRangeItem(revision.ChildRevisions[i].Range.Items[0] as Entity, this, isFromAccept);
						}
						else
						{
							UnlinkRangeItem(revision.ChildRevisions[i].Range.Items[0] as FormatBase, this, isFromAccept);
						}
					}
				}
			}
			else
			{
				for (int j = 0; j < revision.ChildRevisions.Count; j++)
				{
					if (item is FormatBase)
					{
						(item as FormatBase).Document.Revisions.Add(revision.ChildRevisions[j]);
					}
					else
					{
						(item as Entity).Document.Revisions.Add(revision.ChildRevisions[j]);
					}
				}
			}
		}
		RevisionCollection revisionCollection = null;
		revisionCollection = ((!(item is FormatBase)) ? (item as Entity).Document.Revisions : (item as FormatBase).Document.Revisions);
		if (revisionCollection.InnerList.Contains(revision))
		{
			revisionCollection.Remove(revision);
		}
	}

	public void Reject()
	{
		List<WCharacterFormat> list = new List<WCharacterFormat>();
		if (m_range == null)
		{
			RemoveSelf();
		}
		RevisionType revisionType = RevisionType;
		if (revisionType <= RevisionType.Formatting)
		{
			if (revisionType == RevisionType.Insertions)
			{
				goto IL_0449;
			}
			if (revisionType == RevisionType.Deletions)
			{
				goto IL_0205;
			}
			if (revisionType == RevisionType.Formatting)
			{
				goto IL_051f;
			}
		}
		else
		{
			if (revisionType == RevisionType.StyleDefinitionChange)
			{
				goto IL_051f;
			}
			if (revisionType == RevisionType.MoveFrom)
			{
				goto IL_0205;
			}
			if (revisionType == RevisionType.MoveTo)
			{
				goto IL_0449;
			}
		}
		goto IL_052d;
		IL_0449:
		while (Range.Count > 0)
		{
			if (Range.Items[0] is Entity)
			{
				RevisionCollection revisions = (Range.Items[0] as Entity).Document.Revisions;
				List<Revision> revisionsInternal = (Range.Items[0] as Entity).RevisionsInternal;
				bool flag = false;
				while (revisionsInternal.Count > 0 && Range.Items.Count > 0)
				{
					UnlinkRangeItem(Range.Items[0] as Entity, revisionsInternal[0], isFromAccept: false);
					flag = true;
				}
				if (!flag && revisionsInternal.Count == 0 && Range.Items.Count > 0)
				{
					Range.Items.Remove(Range.Items[0] as Entity);
					if (Range.Count == 0 && revisions.InnerList.Contains(this))
					{
						revisions.Remove(this);
					}
				}
				for (int i = 0; i < revisionsInternal.Count; i++)
				{
					if (revisionsInternal[i].Range.Items.Count == 0 && revisions.InnerList.Contains(revisionsInternal[i]))
					{
						revisions.Remove(revisionsInternal[i]);
					}
				}
			}
			else
			{
				if (Range.Items[0] is WCharacterFormat && (Range.Items[0] as WCharacterFormat).OwnerBase is WParagraph)
				{
					list.Add(Range.Items[0] as WCharacterFormat);
				}
				UnlinkRangeItem(Range.Items[0] as FormatBase, this, isFromAccept: false);
			}
		}
		foreach (WCharacterFormat item in list)
		{
			MakeChangesForBreakCharFormat(item.OwnerBase as WParagraph, acceptChanges: false);
		}
		if (m_doc.ClonedFields.Count > 0)
		{
			WField wField = m_doc.ClonedFields.Peek();
			if (wField.OwnerParagraph.Index == wField.FieldEnd.OwnerParagraph.Index)
			{
				m_doc.ClonedFields.Pop();
			}
		}
		if (RevisionType == RevisionType.MoveTo)
		{
			ClearDependentRevision(isFromAccept: false);
		}
		goto IL_052d;
		IL_052d:
		list.Clear();
		return;
		IL_0205:
		while (Range.Count > 0)
		{
			if (Range.Items[0] is Entity)
			{
				RevisionCollection revisions2 = (Range.Items[0] as Entity).Document.Revisions;
				List<Revision> revisionsInternal2 = (Range.Items[0] as Entity).RevisionsInternal;
				if ((Range.Items[0] as Entity).RevisionsInternal.Count == 0)
				{
					Range.Items.Remove(Range.Items[0] as Entity);
					if (Range.Count == 0 && revisions2.InnerList.Contains(this))
					{
						revisions2.Remove(this);
					}
				}
				else
				{
					UnlinkRangeItem(Range.Items[0] as Entity, revisionsInternal2[0], isFromAccept: false);
				}
				for (int j = 0; j < revisionsInternal2.Count; j++)
				{
					if (revisionsInternal2[j].Range.Items.Count == 0 && revisions2.InnerList.Contains(revisionsInternal2[j]))
					{
						revisions2.Remove(revisionsInternal2[j]);
					}
				}
			}
			else
			{
				if (Range.Items[0] is WCharacterFormat && (Range.Items[0] as WCharacterFormat).OwnerBase is WParagraph)
				{
					list.Add(Range.Items[0] as WCharacterFormat);
				}
				UnlinkRangeItem(Range.Items[0] as FormatBase, this, isFromAccept: false);
			}
		}
		foreach (WCharacterFormat item2 in list)
		{
			MakeChangesForBreakCharFormat(item2.OwnerBase as WParagraph, acceptChanges: false);
		}
		if (RevisionType == RevisionType.MoveFrom)
		{
			ClearDependentRevision(isFromAccept: false);
		}
		goto IL_052d;
		IL_051f:
		while (Range.Count > 0)
		{
			UnlinkRangeItem(Range.Items[0] as FormatBase, this, isFromAccept: false);
		}
		goto IL_052d;
	}

	internal void RemoveSelf()
	{
		(Owner as RevisionCollection).Remove(this);
	}

	internal Revision Clone()
	{
		Revision revision = (Revision)MemberwiseClone();
		if (m_childRevisions != null)
		{
			revision.m_childRevisions = new RevisionCollection(m_doc);
		}
		if (m_range != null)
		{
			revision.m_range = new Range();
		}
		return revision;
	}

	internal Revision(WordDocument doc)
	{
		m_doc = doc;
	}

	internal void Close()
	{
		if (m_range != null)
		{
			m_range.Close();
			m_range = null;
		}
		if (m_childRevisions != null)
		{
			m_childRevisions.Close();
			m_childRevisions = null;
		}
	}
}
