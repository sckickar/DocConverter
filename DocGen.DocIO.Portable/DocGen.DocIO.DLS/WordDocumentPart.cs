namespace DocGen.DocIO.DLS;

public class WordDocumentPart
{
	private WSectionCollection m_sections;

	public WSectionCollection Sections => m_sections;

	public WordDocumentPart()
	{
		m_sections = new WSectionCollection(null);
	}

	public WordDocumentPart(WordDocument document)
	{
		m_sections = new WSectionCollection(null);
		Load(document);
	}

	public void Load(WordDocument document)
	{
		foreach (WSection section in document.Sections)
		{
			m_sections.Add(section.Clone());
		}
	}

	public WordDocument GetAsWordDocument()
	{
		WordDocument wordDocument = new WordDocument();
		foreach (WSection section in m_sections)
		{
			wordDocument.Sections.Add(section.Clone());
		}
		return wordDocument;
	}

	public void Close()
	{
		if (m_sections != null && m_sections.Count > 0)
		{
			for (int i = 0; i < m_sections.Count; i++)
			{
				m_sections[i].Close();
			}
			m_sections.Clear();
			m_sections = null;
		}
	}

	internal void GetWordDocumentPart(BookmarkStart bkmkStart, BookmarkEnd bkmkEnd)
	{
		WParagraph ownerParagraph = bkmkStart.OwnerParagraph;
		WParagraph ownerParagraph2 = bkmkEnd.OwnerParagraph;
		if ((!ownerParagraph.IsInCell && !ownerParagraph2.IsInCell) || ownerParagraph.OwnerTextBody == ownerParagraph2.OwnerTextBody)
		{
			GetParagraphDocumentPart(ownerParagraph, ownerParagraph2, bkmkStart, bkmkEnd);
		}
		else if (ownerParagraph.IsInCell && ownerParagraph2.IsInCell)
		{
			GetTableDocumentPart(ownerParagraph, ownerParagraph2, bkmkStart, bkmkEnd, null);
		}
		else if (ownerParagraph.IsInCell && !ownerParagraph2.IsInCell)
		{
			GetTableAfterParagraphDocumentPart(ownerParagraph, ownerParagraph2, bkmkStart, bkmkEnd);
		}
		else if (!ownerParagraph.IsInCell && ownerParagraph2.IsInCell)
		{
			GetParagraphAfterTableDocumentPart(ownerParagraph, ownerParagraph2, bkmkStart, bkmkEnd);
		}
	}

	private Entity GetSection(Entity entity)
	{
		while (!(entity is WSection))
		{
			entity = entity.Owner;
		}
		return entity;
	}

	private void GetParagraphAfterTableDocumentPart(WParagraph startParagraph, WParagraph endParagraph, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd)
	{
		WTextBody ownerTextBody = startParagraph.OwnerTextBody;
		WTableCell wTableCell = endParagraph.GetOwnerEntity() as WTableCell;
		WTable wTable = (WTable)wTableCell.Owner.Owner;
		WTextBody ownerTextBody2 = wTable.OwnerTextBody;
		int indexInOwnerCollection = wTable.GetIndexInOwnerCollection();
		int num = GetSection(ownerTextBody).GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection2 = GetSection(ownerTextBody2).GetIndexInOwnerCollection();
		bool flag = false;
		if (num - 1 == indexInOwnerCollection2)
		{
			flag = true;
		}
		int num2 = wTableCell.Owner.GetIndexInOwnerCollection();
		int indexInOwnerCollection3 = wTableCell.GetIndexInOwnerCollection();
		if (indexInOwnerCollection3 == 0 && num2 != 0 && wTableCell.Paragraphs[0] == bkmkEnd.OwnerParagraph && IsBkmkEndInFirstItem(wTableCell.Paragraphs[0], bkmkEnd, bkmkEnd.GetIndexInOwnerCollection() - 1))
		{
			num2--;
		}
		if (indexInOwnerCollection >= 0 && num2 == 0 && indexInOwnerCollection3 == 0 && wTableCell.Paragraphs[0] == bkmkEnd.OwnerParagraph)
		{
			if (wTableCell.ChildEntities[0] is WParagraph && indexInOwnerCollection == 0)
			{
				WParagraph wParagraph = wTableCell.ChildEntities[0] as WParagraph;
				if (IsBkmkEndInFirstItem(wParagraph, bkmkEnd, bkmkEnd.GetIndexInOwnerCollection() - 1))
				{
					WParagraph wParagraph2 = new WParagraph(ownerTextBody2.Document);
					wParagraph2.Items.Add(wParagraph.Items[bkmkEnd.GetIndexInOwnerCollection()]);
					ownerTextBody2.ChildEntities.Insert(0, wParagraph2);
					GetParagraphDocumentPart(startParagraph, wParagraph2, bkmkStart, bkmkEnd);
					return;
				}
			}
			else if (indexInOwnerCollection > 0)
			{
				(ownerTextBody2.Items[indexInOwnerCollection - 1] as WParagraph).Items.Add(bkmkEnd);
				GetParagraphDocumentPart(startParagraph, ownerTextBody2.Items[indexInOwnerCollection - 1] as WParagraph, bkmkStart, bkmkEnd);
				return;
			}
		}
		int indexInOwnerCollection4 = startParagraph.GetIndexInOwnerCollection();
		int bkmkStartNextItemIndex = bkmkStart.GetIndexInOwnerCollection() + 1;
		AddFirstSectionToDocumentPart(indexInOwnerCollection4, indexInOwnerCollection, ownerTextBody, bkmkStartNextItemIndex, indexInOwnerCollection - 1, flag, (ownerTextBody.Owner as WSection).CloneWithoutBodyItems());
		if (!flag)
		{
			AddInBetweenSections(num, indexInOwnerCollection2, ownerTextBody.Document);
			AddLastSectionToDocumentPart(indexInOwnerCollection, -1, wTable.OwnerTextBody, IsFirstBkmkEnd: true, flag);
			bkmkStart.GetBkmkContentInDiffCell(wTable, 0, num2, 0, wTable.LastCell.GetIndexInOwnerCollection(), m_sections[m_sections.Count - 1].Body);
		}
		else
		{
			bkmkStart.GetBkmkContentInDiffCell(wTable, 0, num2, 0, wTable.LastCell.GetIndexInOwnerCollection(), m_sections[0].Body);
		}
	}

	private void GetTableAfterParagraphDocumentPart(WParagraph startParagraph, WParagraph endParagraph, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd)
	{
		WTableCell wTableCell = startParagraph.GetOwnerEntity() as WTableCell;
		WTableCell wTableCell2 = null;
		WTable wTable = (WTable)wTableCell.Owner.Owner;
		int indexInOwnerCollection = wTableCell.Owner.GetIndexInOwnerCollection();
		int num = -1;
		if (bkmkStart.ColumnFirst != -1 && bkmkStart.ColumnLast != -1)
		{
			num = wTable.LastRow.GetIndexInOwnerCollection();
			if (bkmkStart.ColumnFirst < wTable.Rows[indexInOwnerCollection].Cells.Count && bkmkStart.ColumnLast < wTable.Rows[num].Cells.Count)
			{
				wTableCell = wTable.Rows[indexInOwnerCollection].Cells[bkmkStart.ColumnFirst];
				wTableCell2 = wTable.Rows[num].Cells[bkmkStart.ColumnLast];
			}
			wTableCell.GetIndexInOwnerCollection();
			wTableCell2.GetIndexInOwnerCollection();
			GetTableDocumentPart(startParagraph, endParagraph, bkmkStart, bkmkEnd, wTableCell2);
			return;
		}
		int indexInOwnerCollection2 = wTable.GetIndexInOwnerCollection();
		int num2 = endParagraph.GetIndexInOwnerCollection();
		int bkmkEndPreviosItemIndex = bkmkEnd.GetIndexInOwnerCollection() - 1;
		WTextBody wTextBody = (WTextBody)wTable.Owner;
		WTextBody wTextBody2 = (WTextBody)endParagraph.Owner;
		bool flag = false;
		int num3 = GetSection(wTextBody).GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection3 = GetSection(wTextBody2).GetIndexInOwnerCollection();
		if (num3 - 1 == indexInOwnerCollection3)
		{
			flag = true;
		}
		bool flag2 = false;
		flag2 = IsBkmkEndInFirstItem(endParagraph, bkmkEnd, bkmkEndPreviosItemIndex);
		if (!(wTextBody == wTextBody2 && wTextBody.ChildEntities[indexInOwnerCollection2 + 1] == endParagraph && flag2))
		{
			WSection wSection = (GetSection(wTable) as WSection).CloneWithoutBodyItems();
			bkmkStart.GetBkmkContentInDiffCell(wTable, indexInOwnerCollection, wTable.LastRow.GetIndexInOwnerCollection(), 0, wTable.LastCell.GetIndexInOwnerCollection(), wSection.Body);
			AddFirstSectionToDocumentPart(indexInOwnerCollection2 + 1, num2, wTable.OwnerTextBody, -1, -1, flag, wSection);
			if (!flag2 || !flag)
			{
				AddInBetweenSections(num3, indexInOwnerCollection3, wTextBody.Document);
				if (flag2)
				{
					num2--;
				}
				AddLastSectionToDocumentPart(num2, bkmkEndPreviosItemIndex, endParagraph.OwnerTextBody, flag2, flag);
			}
		}
		else
		{
			GetTableDocumentPart(startParagraph, endParagraph, bkmkStart, bkmkEnd, wTableCell2);
		}
	}

	private void GetTableDocumentPart(WParagraph startParagraph, WParagraph endParagraph, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd, WTableCell bkmkEndCell)
	{
		WTableCell wTableCell = startParagraph.GetOwnerEntity() as WTableCell;
		if (endParagraph.IsInCell)
		{
			bkmkEndCell = endParagraph.GetOwnerEntity() as WTableCell;
		}
		else if (endParagraph.PreviousSibling is WTable)
		{
			bkmkEndCell = (endParagraph.PreviousSibling as WTable).LastCell;
		}
		if (wTableCell == null || bkmkEndCell == null)
		{
			return;
		}
		WTableCell tempBkmkEndCell = bkmkEndCell;
		WTable wTable = (WTable)wTableCell.Owner.Owner;
		WTable wTable2 = (WTable)bkmkEndCell.Owner.Owner;
		WSection wSection = (wTable.OwnerTextBody.Owner as WSection).CloneWithoutBodyItems();
		int indexInOwnerCollection = wTableCell.Owner.GetIndexInOwnerCollection();
		int endTableRowIndex = bkmkEndCell.Owner.GetIndexInOwnerCollection();
		int bkmkEndCellIndex = bkmkEndCell.GetIndexInOwnerCollection();
		int bkmkStartCellIndex = wTableCell.GetIndexInOwnerCollection();
		bool flag = false;
		int num = GetSection(wTable).GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection2 = GetSection(wTable2).GetIndexInOwnerCollection();
		if (num - 1 == indexInOwnerCollection2)
		{
			flag = true;
		}
		bkmkStart.GetBookmarkStartAndEndCell(wTableCell, bkmkEndCell, tempBkmkEndCell, wTable, wTable2, bkmkStart, bkmkEnd, indexInOwnerCollection, ref endTableRowIndex, ref bkmkStartCellIndex, ref bkmkEndCellIndex);
		if (wTable == wTable2)
		{
			bkmkStart.GetBkmkContentInDiffCell(wTable, indexInOwnerCollection, endTableRowIndex, bkmkStartCellIndex, bkmkEndCellIndex, wSection.Body);
			m_sections.Add(wSection);
			return;
		}
		bkmkStart.GetBkmkContentInDiffCell(wTable, indexInOwnerCollection, wTable.Rows.Count - 1, 0, wTable.LastCell.GetIndexInOwnerCollection(), wSection.Body);
		int indexInOwnerCollection3 = wTable.GetIndexInOwnerCollection();
		int indexInOwnerCollection4 = wTable2.GetIndexInOwnerCollection();
		if (flag)
		{
			AddSectionBodyItems(indexInOwnerCollection3 + 1, indexInOwnerCollection4, wTable.OwnerTextBody, wSection);
			bkmkStart.GetBkmkContentInDiffCell(wTable2, 0, endTableRowIndex, 0, wTable2.LastCell.GetIndexInOwnerCollection(), wSection.Body);
			m_sections.Add(wSection);
			return;
		}
		AddSectionBodyItems(indexInOwnerCollection3 + 1, wTable.OwnerTextBody.ChildEntities.Count, wTable.OwnerTextBody, wSection);
		m_sections.Add(wSection);
		AddInBetweenSections(num, indexInOwnerCollection2, wTable.Document);
		WSection wSection2 = (GetSection(wTable2) as WSection).CloneWithoutBodyItems();
		AddSectionBodyItems(0, indexInOwnerCollection4, wTable2.OwnerTextBody, wSection2);
		bkmkStart.GetBkmkContentInDiffCell(wTable2, 0, endTableRowIndex, 0, wTable2.LastCell.GetIndexInOwnerCollection(), wSection2.Body);
		m_sections.Add(wSection2);
	}

	private void AddSectionBodyItems(int startItemIndex, int endItemIndex, WTextBody textBody, WSection section)
	{
		for (int i = startItemIndex + 1; i < endItemIndex; i++)
		{
			TextBodyItem entity = (TextBodyItem)textBody.ChildEntities[i].CloneInt();
			section.Body.Items.AddToInnerList(entity);
		}
	}

	private void GetParagraphDocumentPart(WParagraph paraStart, WParagraph paraEnd, BookmarkStart bkmkStart, BookmarkEnd bkmkEnd)
	{
		WTextBody ownerTextBody = paraStart.OwnerTextBody;
		WTextBody ownerTextBody2 = paraEnd.OwnerTextBody;
		int indexInOwnerCollection = paraStart.GetIndexInOwnerCollection();
		int bkmkStartNextItemIndex = bkmkStart.GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection2 = paraEnd.GetIndexInOwnerCollection();
		int bkmkEndPreviosItemIndex = bkmkEnd.GetIndexInOwnerCollection() - 1;
		bool flag = false;
		int num = GetSection(ownerTextBody).GetIndexInOwnerCollection() + 1;
		int indexInOwnerCollection3 = GetSection(ownerTextBody2).GetIndexInOwnerCollection();
		if (num - 1 == indexInOwnerCollection3)
		{
			flag = true;
		}
		if (flag)
		{
			if (ownerTextBody.GetOwnerSection(ownerTextBody) is WSection wSection)
			{
				WSection wSection2 = wSection.CloneWithoutBodyItems();
				TextBodySelection textBodySelection = new TextBodySelection(bkmkStart, bkmkEnd);
				textBodySelection.ParagraphItemStartIndex++;
				textBodySelection.ParagraphItemEndIndex--;
				TextBodyPart textBodyPart = new TextBodyPart(textBodySelection);
				while (textBodyPart.BodyItems.Count != 0)
				{
					wSection2.Body.ChildEntities.Add(textBodyPart.BodyItems[0]);
				}
				m_sections.Add(wSection2);
			}
		}
		else
		{
			AddFirstSectionToDocumentPart(indexInOwnerCollection, indexInOwnerCollection2, ownerTextBody, bkmkStartNextItemIndex, bkmkEndPreviosItemIndex, flag, (GetSection(ownerTextBody) as WSection).CloneWithoutBodyItems());
			if (num - 1 != indexInOwnerCollection3)
			{
				AddInBetweenSections(num, indexInOwnerCollection3, ownerTextBody.Owner.Owner as WordDocument);
			}
			bool flag2 = IsBkmkEndInFirstItem(paraEnd, bkmkEnd, bkmkEndPreviosItemIndex);
			if (!flag2 || !flag)
			{
				AddLastSectionToDocumentPart(indexInOwnerCollection2, bkmkEndPreviosItemIndex, ownerTextBody2, flag2, flag);
			}
		}
	}

	private void AddLastSectionToDocumentPart(int endParagraphIndex, int bkmkEndPreviosItemIndex, WTextBody endTextBody, bool IsFirstBkmkEnd, bool isInSingleSection)
	{
		if (!isInSingleSection)
		{
			AddFirstSectionToDocumentPart(0, endParagraphIndex, endTextBody, 0, -1, isInSingleSection: true, (GetSection(endTextBody) as WSection).CloneWithoutBodyItems());
		}
		if (IsFirstBkmkEnd)
		{
			return;
		}
		WSection wSection = (isInSingleSection ? m_sections[0] : m_sections[m_sections.Count - 1]);
		WParagraph wParagraph = (endTextBody.Items[endParagraphIndex] as WParagraph).Clone() as WParagraph;
		while (bkmkEndPreviosItemIndex + 1 < wParagraph.Items.Count)
		{
			if (endTextBody.Document.IsComparing)
			{
				wParagraph.Items.RemoveAt(bkmkEndPreviosItemIndex + 1);
			}
			else
			{
				wParagraph.Items.RemoveFromInnerList(bkmkEndPreviosItemIndex + 1);
			}
		}
		wSection.Body.ChildEntities.Add(wParagraph);
	}

	private void AddInBetweenSections(int startSectionIndex, int endSectionIndex, WordDocument document)
	{
		for (int i = startSectionIndex; i < endSectionIndex; i++)
		{
			m_sections.Add(document.Sections[i].Clone());
		}
	}

	private void AddFirstSectionToDocumentPart(int startParagraphIndex, int endParagraphIndex, WTextBody startTextBody, int bkmkStartNextItemIndex, int bkmkEndPreviosItemIndex, bool isInSingleSection, WSection section)
	{
		int num = (isInSingleSection ? endParagraphIndex : startTextBody.ChildEntities.Count);
		for (int i = startParagraphIndex; i < num; i++)
		{
			TextBodyItem textBodyItem = startTextBody.ChildEntities[i] as TextBodyItem;
			if (startParagraphIndex == i && bkmkStartNextItemIndex > 0)
			{
				WParagraph wParagraph = (textBodyItem as WParagraph).Clone() as WParagraph;
				while (bkmkStartNextItemIndex > 0)
				{
					wParagraph.Items.RemoveFromInnerList(0);
					bkmkStartNextItemIndex--;
				}
				if (wParagraph.ChildEntities.Count > 0 || startTextBody.Document.IsComparing)
				{
					section.Body.ChildEntities.Add(wParagraph);
				}
			}
			else
			{
				section.Body.ChildEntities.Add(textBodyItem.Clone());
			}
		}
		m_sections.Add(section);
	}

	private bool IsBkmkEndInFirstItem(WParagraph paragraph, ParagraphItem bkmkEnd, int bkmkEndPreviosItemIndex)
	{
		for (int i = 0; i <= bkmkEndPreviosItemIndex; i++)
		{
			if (!(paragraph.Items[i] is BookmarkStart) && !(paragraph.Items[i] is BookmarkEnd))
			{
				return false;
			}
		}
		return true;
	}
}
