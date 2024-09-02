using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DocGen.DocIO.ReaderWriter;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.DocIO.ReaderWriter.DataStreamParser.OLEObject;
using DocGen.Drawing;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

internal sealed class DocWriterAdapter
{
	private const string LINK_STRING = "OLE_LINK";

	private int m_listID = 1720085641;

	private int m_secNumber;

	private int m_tableNestingLevel;

	private string m_prevStyleName = string.Empty;

	private WParagraph m_lastParagarph;

	private WordWriter m_mainWriter;

	private IWordWriterBase m_currWriter;

	private WordDocument m_document;

	private IWSection m_currSection;

	private WTextBoxCollection m_txbxItems;

	private WTextBoxCollection m_hfTxbxItems;

	private List<WComment> m_commentCollection;

	private List<WFootnote> m_footnoteCollection;

	private List<WFootnote> m_endnoteCollection;

	private Dictionary<string, int> m_charStylesHash = new Dictionary<string, int>();

	private Dictionary<string, ListData> m_listData = new Dictionary<string, ListData>();

	private List<string> m_bookmarksAfterCell = new List<string>();

	private Stack<WField> m_fieldStack = new Stack<WField>();

	private Dictionary<string, DictionaryEntry> m_commOffsets;

	private List<WPicture> m_listPicture;

	private List<WOleObject> m_oleObjects;

	private List<OLEObject> m_OLEObjects;

	private IWordWriterBase CurrentWriter
	{
		get
		{
			return m_currWriter;
		}
		set
		{
			m_currWriter = value;
		}
	}

	private WField CurrentField
	{
		get
		{
			if (m_fieldStack.Count <= 0)
			{
				return null;
			}
			return m_fieldStack.Peek();
		}
	}

	private List<WPicture> ListPicture
	{
		get
		{
			if (m_listPicture == null)
			{
				m_listPicture = new List<WPicture>();
			}
			return m_listPicture;
		}
	}

	private WParagraph LastParagraph => m_lastParagarph;

	private List<WComment> CommentCollection
	{
		get
		{
			if (m_commentCollection == null)
			{
				m_commentCollection = new List<WComment>();
			}
			return m_commentCollection;
		}
	}

	private List<WFootnote> FootnoteCollection
	{
		get
		{
			if (m_footnoteCollection == null)
			{
				m_footnoteCollection = new List<WFootnote>();
			}
			return m_footnoteCollection;
		}
	}

	private List<WFootnote> EndnoteCollection
	{
		get
		{
			if (m_endnoteCollection == null)
			{
				m_endnoteCollection = new List<WFootnote>();
			}
			return m_endnoteCollection;
		}
	}

	private WTextBoxCollection HFTextBoxCollection
	{
		get
		{
			if (m_hfTxbxItems == null)
			{
				m_hfTxbxItems = new WTextBoxCollection(m_document);
			}
			return m_hfTxbxItems;
		}
	}

	private WTextBoxCollection TextBoxCollection
	{
		get
		{
			if (m_txbxItems == null)
			{
				m_txbxItems = new WTextBoxCollection(m_document);
			}
			return m_txbxItems;
		}
	}

	private Dictionary<string, DictionaryEntry> CommentOffsets
	{
		get
		{
			if (m_commOffsets == null)
			{
				m_commOffsets = new Dictionary<string, DictionaryEntry>();
			}
			return m_commOffsets;
		}
	}

	internal DocWriterAdapter()
	{
		AdapterListIDHolder.Instance.ListStyleIDtoName.Clear();
	}

	public void Write(WordWriter writer, WordDocument document)
	{
		m_document = document;
		Init(writer);
		m_mainWriter.WriteDocumentHeader();
		WriteBody();
		m_mainWriter.WriteDocumentEnd(m_document.Password, m_document.BuiltinDocumentProperties.Author, m_document.FIBVersion, m_document.OleObjectCollection);
		writer.Close();
		Close();
	}

	private void Init(WordWriter writer)
	{
		ResetLists();
		m_secNumber = 0;
		CurrentWriter = writer;
		m_mainWriter = writer;
		writer.StyleSheet.FontSubstitutionTable = m_document.FontSubstitutionTable;
		writer.m_docInfo.TablesData.FFNStringTable = m_document.FFNStringTable;
		m_lastParagarph = m_document.LastParagraph;
		m_mainWriter.CHPXStickProperties = false;
		m_mainWriter.PAPXStickProperties = false;
		m_mainWriter.SectionProperties.StickProperties = false;
		WriteStyleSheet(writer);
		AddListPictures();
		SectionPropertiesConverter.Import(writer.SectionProperties, m_document.Sections[0]);
	}

	private void WriteBody()
	{
		m_secNumber = 0;
		WriteBackground();
		WriteDocumentEscher();
		WriteMainBody();
		WriteFootnotesBody();
		WriteHFBody();
		WriteAnnotationsBody();
		WriteEndnotesBody();
		WriteTextBoxes();
		WriteDocumentProperties();
	}

	private void WriteMainBody()
	{
		WSection wSection = null;
		int i = 0;
		for (int count = m_document.Sections.Count; i < count; i++)
		{
			wSection = m_document.Sections[i];
			WriteSectionEnd(wSection);
			if (wSection.Body.Items.Count > 0)
			{
				WriteParagraphs(wSection.Body.Items, isTableBody: false);
				continue;
			}
			m_mainWriter.PAPX.PropertyModifiers.Clear();
			m_mainWriter.CurrentStyleIndex = 0;
		}
	}

	private void WriteHFBody()
	{
		bool flag = false;
		bool flag2 = true;
		if (m_document.Watermark != null && m_document.Watermark.Type != 0)
		{
			WriteWatermarkParagraphs();
		}
		WSection wSection = null;
		int i = 0;
		for (int count = m_document.Sections.Count; i < count; i++)
		{
			wSection = m_document.Sections[i];
			if (!wSection.HeadersFooters.IsEmpty)
			{
				flag2 = false;
				break;
			}
		}
		if (FootnoteCollection.Count <= 0 && EndnoteCollection.Count <= 0 && flag2)
		{
			return;
		}
		WordHeaderFooterWriter wordHeaderFooterWriter = m_mainWriter.GetSubdocumentWriter(WordSubdocument.HeaderFooter) as WordHeaderFooterWriter;
		wordHeaderFooterWriter.CHPXStickProperties = false;
		CurrentWriter = wordHeaderFooterWriter;
		wordHeaderFooterWriter.PAPXStickProperties = false;
		WriteSeparatorStories();
		int j = 0;
		for (int count2 = m_document.Sections.Count; j < count2; j++)
		{
			wSection = m_document.Sections[j];
			if (flag)
			{
				wordHeaderFooterWriter.WriteSectionEnd();
			}
			for (int k = 0; k < 6; k++)
			{
				if (wSection.HeadersFooters[k].WriteWatermark)
				{
					InsertWatermark(wSection.HeadersFooters[k], (HeaderType)k);
				}
				WriteHeaderFooter(wordHeaderFooterWriter, wSection.HeadersFooters[k].ChildEntities as BodyItemCollection, (HeaderType)k);
			}
			flag = true;
		}
		wordHeaderFooterWriter.WriteDocumentEnd();
	}

	private void WriteSeparatorStories()
	{
		WriteSeparatorStory(m_document.Footnotes.Separator);
		WriteSeparatorStory(m_document.Footnotes.ContinuationSeparator);
		WriteSeparatorStory(m_document.Footnotes.ContinuationNotice);
		WriteSeparatorStory(m_document.Endnotes.Separator);
		WriteSeparatorStory(m_document.Endnotes.ContinuationSeparator);
		WriteSeparatorStory(m_document.Endnotes.ContinuationNotice);
	}

	private void WriteSeparatorStory(WTextBody body)
	{
		WriteParagraphs((BodyItemCollection)body.ChildEntities, isTableBody: false);
		if (body.ChildEntities.Count >= 1 && !(body.ChildEntities[body.ChildEntities.Count - 1] is WTable))
		{
			(CurrentWriter as WordHeaderFooterWriter).WriteMarker(WordChunkType.ParagraphEnd);
		}
		(CurrentWriter as WordHeaderFooterWriter).ClosePrevSeparator();
	}

	private void InsertWatermark(WTextBody textBody, HeaderType headerType)
	{
		if (headerType == HeaderType.EvenHeader || headerType == HeaderType.OddHeader || headerType == HeaderType.FirstPageHeader)
		{
			WParagraph wParagraph = GetFirstPara(textBody);
			if (wParagraph == null)
			{
				WSection obj = textBody.OwnerBase as WSection;
				wParagraph = new WParagraph(obj.Document);
				obj.HeadersFooters[(int)headerType].Items.Insert(0, wParagraph);
			}
			wParagraph.Items.Insert(0, (textBody as HeaderFooter).Watermark);
		}
	}

	private WParagraph GetFirstPara(WTextBody textBody)
	{
		if (textBody != null && textBody.Items[0] is WParagraph)
		{
			return textBody.Items[0] as WParagraph;
		}
		if (textBody != null && textBody.Items[0] is WTable)
		{
			return GetFirstTblPara(textBody.Items[0] as WTable);
		}
		if (textBody != null && textBody.Items[0] is BlockContentControl)
		{
			return GetFirstPara((textBody.Items[0] as BlockContentControl).TextBody);
		}
		return null;
	}

	private WParagraph GetFirstTblPara(WTable table)
	{
		foreach (WTableRow row in table.Rows)
		{
			foreach (WTableCell cell in row.Cells)
			{
				foreach (TextBodyItem item in cell.Items)
				{
					if (item is WParagraph)
					{
						return item as WParagraph;
					}
				}
			}
		}
		if (table.Rows.Count > 0 && table.Rows[0].Cells.Count > 0)
		{
			return table.Rows[0].Cells[0].AddParagraph() as WParagraph;
		}
		return null;
	}

	private void WriteTextBoxBody(WTextBoxCollection txbxCollection, WordSubdocument txBxType)
	{
		if (txbxCollection != null && txbxCollection.Count > 0)
		{
			IWordSubdocumentWriter subdocumentWriter = m_mainWriter.GetSubdocumentWriter(txBxType);
			subdocumentWriter.CHPXStickProperties = false;
			subdocumentWriter.PAPXStickProperties = false;
			CurrentWriter = subdocumentWriter;
			int count = txbxCollection.Count;
			for (int i = 0; i < count; i++)
			{
				WriteTextBoxText(subdocumentWriter, txbxCollection[i] as WTextBox);
			}
			subdocumentWriter.WriteDocumentEnd();
		}
	}

	private void WriteTextBoxText(IWordSubdocumentWriter txbxWriter, WTextBox textBox)
	{
		WriteParagraphs((BodyItemCollection)textBox.TextBoxBody.ChildEntities, isTableBody: false);
		if (txbxWriter is WordHFTextBoxWriter)
		{
			((WordHFTextBoxWriter)txbxWriter).WriteHFTextBoxEnd(textBox.TextBoxSpid);
		}
		else
		{
			((WordTextBoxWriter)txbxWriter).WriteTextBoxEnd(textBox.TextBoxSpid);
		}
	}

	private void WriteFootnotesBody()
	{
		if (m_footnoteCollection != null && m_footnoteCollection.Count > 0)
		{
			int count = m_footnoteCollection.Count;
			IWordSubdocumentWriter subdocumentWriter = m_mainWriter.GetSubdocumentWriter(WordSubdocument.Footnote);
			subdocumentWriter.CHPXStickProperties = false;
			subdocumentWriter.PAPXStickProperties = false;
			CurrentWriter = subdocumentWriter;
			for (int i = 0; i < count; i++)
			{
				WFootnote wFootnote = m_footnoteCollection[i];
				WriteSubDocumentText(subdocumentWriter, wFootnote.TextBody);
			}
			subdocumentWriter.WriteDocumentEnd();
		}
	}

	private void WriteAnnotationsBody()
	{
		if (m_commentCollection != null && m_commentCollection.Count > 0)
		{
			int count = m_commentCollection.Count;
			IWordSubdocumentWriter subdocumentWriter = m_mainWriter.GetSubdocumentWriter(WordSubdocument.Annotation);
			subdocumentWriter.CHPXStickProperties = false;
			subdocumentWriter.PAPXStickProperties = false;
			CurrentWriter = subdocumentWriter;
			for (int i = 0; i < count; i++)
			{
				WComment wComment = m_commentCollection[i];
				WriteSubDocumentText(subdocumentWriter, wComment.TextBody);
			}
			subdocumentWriter.WriteDocumentEnd();
		}
	}

	private void WriteEndnotesBody()
	{
		if (m_endnoteCollection != null && m_endnoteCollection.Count > 0)
		{
			int count = m_endnoteCollection.Count;
			IWordSubdocumentWriter subdocumentWriter = m_mainWriter.GetSubdocumentWriter(WordSubdocument.Endnote);
			subdocumentWriter.CHPXStickProperties = false;
			subdocumentWriter.PAPXStickProperties = false;
			CurrentWriter = subdocumentWriter;
			for (int i = 0; i < count; i++)
			{
				WFootnote wFootnote = m_endnoteCollection[i];
				WriteSubDocumentText(subdocumentWriter, wFootnote.TextBody);
			}
			subdocumentWriter.WriteDocumentEnd();
		}
	}

	private void WriteTextBoxes()
	{
		WriteTextBoxBody(m_txbxItems, WordSubdocument.TextBox);
		WriteTextBoxBody(m_hfTxbxItems, WordSubdocument.HeaderTextBox);
	}

	private void WriteHeaderFooter(WordHeaderFooterWriter hfWriter, BodyItemCollection collection, HeaderType hType)
	{
		hfWriter.HeaderType = hType;
		WriteParagraphs(collection, isTableBody: false);
		if (collection.Count >= 1 && !(collection[collection.Count - 1] is WTable))
		{
			hfWriter.WriteMarker(WordChunkType.ParagraphEnd);
		}
	}

	private void WriteSubDocumentText(IWordSubdocumentWriter writer, WTextBody body)
	{
		writer.WriteItemStart();
		WriteParagraphs((BodyItemCollection)body.ChildEntities, isTableBody: false);
		writer.WriteItemEnd();
	}

	private void WriteParagraphs(BodyItemCollection paragraphs, bool isTableBody)
	{
		IEntity entity = null;
		IWordWriterBase currentWriter = CurrentWriter;
		for (int i = 0; i < paragraphs.Count; i++)
		{
			if (i != 0)
			{
				if (isTableBody)
				{
					SetTableNestingLevel(currentWriter.PAPX.PropertyModifiers, m_tableNestingLevel);
				}
				if (entity is IWParagraph)
				{
					WParagraph wParagraph = entity as WParagraph;
					if (i == paragraphs.Count - 1 && m_currSection.NextSibling == null && paragraphs[i] is WParagraph && ((WParagraph)paragraphs[i]).RemoveEmpty)
					{
						break;
					}
					if (!wParagraph.RemoveEmpty || !(wParagraph.Text == string.Empty))
					{
						currentWriter.WriteMarker(WordChunkType.ParagraphEnd);
					}
				}
			}
			if (isTableBody)
			{
				SetCellMark(currentWriter.PAPX.PropertyModifiers, value: true);
				SetTableNestingLevel(currentWriter.PAPX.PropertyModifiers, m_tableNestingLevel);
			}
			entity = paragraphs[i];
			if (entity is IWParagraph)
			{
				WParagraph wParagraph2 = entity as WParagraph;
				if (!wParagraph2.RemoveEmpty || !(wParagraph2.Text == string.Empty))
				{
					if (wParagraph2.ParagraphFormat.PageBreakAfter && !IsPageBreakNeedToBeSkipped(wParagraph2))
					{
						wParagraph2.InsertBreak(BreakType.PageBreak);
					}
					if (wParagraph2.ParagraphFormat.ColumnBreakAfter && !IsPageBreakNeedToBeSkipped(wParagraph2))
					{
						wParagraph2.InsertBreak(BreakType.ColumnBreak);
					}
					WriteParagraph(entity as IWParagraph);
				}
			}
			else if (entity is IWTable)
			{
				WriteTable(entity as IWTable);
			}
			else if (entity is BlockContentControl)
			{
				bool flag = WriteSDTBlock(entity as BlockContentControl, isTableBody);
				if (entity.NextSibling != null && flag)
				{
					currentWriter.WriteMarker(WordChunkType.ParagraphEnd);
				}
			}
		}
	}

	private bool WriteSDTBlock(BlockContentControl sdtBlock, bool isTableBody)
	{
		BodyItemCollection items = sdtBlock.TextBody.Items;
		bool flag = items.LastItem is WParagraph;
		IEntity entity = null;
		IWordWriterBase currentWriter = CurrentWriter;
		for (int i = 0; i < items.Count; i++)
		{
			if (i != 0)
			{
				if (isTableBody)
				{
					SetTableNestingLevel(currentWriter.PAPX.PropertyModifiers, m_tableNestingLevel);
				}
				if (entity is IWParagraph)
				{
					WParagraph wParagraph = entity as WParagraph;
					if (!wParagraph.RemoveEmpty || !(wParagraph.Text == string.Empty))
					{
						currentWriter.WriteMarker(WordChunkType.ParagraphEnd);
					}
				}
			}
			if (isTableBody)
			{
				SetCellMark(currentWriter.PAPX.PropertyModifiers, value: true);
				SetTableNestingLevel(currentWriter.PAPX.PropertyModifiers, m_tableNestingLevel);
			}
			entity = items[i];
			if (entity is IWParagraph)
			{
				WParagraph wParagraph2 = entity as WParagraph;
				if (!wParagraph2.RemoveEmpty || !(wParagraph2.Text == string.Empty))
				{
					if (wParagraph2.ParagraphFormat.PageBreakAfter && !IsPageBreakNeedToBeSkipped(wParagraph2))
					{
						wParagraph2.InsertBreak(BreakType.PageBreak);
					}
					if (wParagraph2.ParagraphFormat.ColumnBreakAfter && !IsPageBreakNeedToBeSkipped(wParagraph2))
					{
						wParagraph2.InsertBreak(BreakType.ColumnBreak);
					}
					WriteParagraph(entity as IWParagraph);
				}
			}
			else if (entity is IWTable)
			{
				WriteTable(entity as IWTable);
			}
			else if (entity is BlockContentControl)
			{
				flag = WriteSDTBlock(entity as BlockContentControl, isTableBody);
				if (entity.NextSibling != null && flag)
				{
					currentWriter.WriteMarker(WordChunkType.ParagraphEnd);
					flag = false;
				}
			}
		}
		return flag;
	}

	private bool CheckCurItemInTable(bool isTableBody, BodyItemCollection paragraphs, int itemIndex)
	{
		bool result = false;
		if (isTableBody)
		{
			IEntity entity = paragraphs[itemIndex];
			if (entity is WParagraph && (entity as WParagraph).Items.Count == 1)
			{
				WParagraph wParagraph = entity as WParagraph;
				if (wParagraph.Items[0] is BookmarkEnd && (wParagraph.Items[0] as BookmarkEnd).IsCellGroupBkmk)
				{
					m_bookmarksAfterCell.Add((wParagraph.Items[0] as BookmarkEnd).Name);
					result = true;
				}
			}
		}
		return result;
	}

	private void WriteParagraph(IWParagraph paragraph)
	{
		(paragraph as WParagraph).SplitTextRange();
		bool flag = paragraph == LastParagraph;
		WriteParagraphProperties(paragraph);
		bool flag2 = false;
		for (int i = 0; i < paragraph.Items.Count; i++)
		{
			ParagraphItem paragraphItem = paragraph[i];
			bool flag3 = false;
			flag3 = (flag2 ? (paragraphItem.GetCharFormat().BiDirectionalOverride != BiDirectionalOverride.None) : SerializeDirectionalOverride(paragraphItem));
			if (!flag2 && flag3)
			{
				flag2 = true;
			}
			if (flag2 && !flag3)
			{
				WriteTextChunks('\u202c'.ToString(), safeText: true);
				flag2 = false;
			}
			WriteParaItem(paragraphItem, paragraph);
		}
		if (flag2)
		{
			WriteTextChunks('\u202c'.ToString(), safeText: true);
		}
		if (flag && ListPicture.Count > 0)
		{
			WriteBookmarkStart(new BookmarkStart(m_document, "_PictureBullets"));
			WriteListPictures();
			WriteBookmarkEnd(new BookmarkEnd(m_document, "_PictureBullets"));
		}
		if (!paragraph.IsInCell || !(paragraph as WParagraph).HasNoRenderableItem() || (paragraph.Owner as WTableCell).Count != 1 || (((paragraph.Owner as WTableCell).CharacterFormat.IsInsertRevision || (paragraph.Owner as WTableCell).CharacterFormat.IsDeleteRevision || !paragraph.BreakCharacterFormat.IsDeleteRevision) && !paragraph.BreakCharacterFormat.IsInsertRevision))
		{
			return;
		}
		if (paragraph.BreakCharacterFormat.IsDeleteRevision)
		{
			(paragraph.Owner as WTableCell).CharacterFormat.IsDeleteRevision = true;
		}
		if (paragraph.BreakCharacterFormat.IsInsertRevision)
		{
			(paragraph.Owner as WTableCell).CharacterFormat.IsInsertRevision = true;
		}
		(paragraph.Owner as WTableCell).CharacterFormat.AuthorName = paragraph.BreakCharacterFormat.AuthorName;
		(paragraph.Owner as WTableCell).CharacterFormat.RevDateTime = paragraph.BreakCharacterFormat.RevDateTime;
		foreach (Revision revision in paragraph.BreakCharacterFormat.Revisions)
		{
			(paragraph.Owner as WTableCell).CharacterFormat.Revisions.Add(revision);
		}
	}

	private bool SerializeDirectionalOverride(ParagraphItem item)
	{
		BiDirectionalOverride biDirectionalOverride = item.GetCharFormat().BiDirectionalOverride;
		if (biDirectionalOverride != 0)
		{
			switch (biDirectionalOverride)
			{
			case BiDirectionalOverride.RTL:
				WriteTextChunks('\u202e'.ToString(), safeText: true);
				break;
			case BiDirectionalOverride.LTR:
				WriteTextChunks('\u202d'.ToString(), safeText: true);
				break;
			}
			return true;
		}
		return false;
	}

	private void WriteListPictures()
	{
		int i = 0;
		for (int count = ListPicture.Count; i < count; i++)
		{
			WPicture wPicture = ListPicture[i];
			wPicture.CharacterFormat.Hidden = true;
			WriteImage(wPicture);
		}
	}

	private void WriteParaItem(ParagraphItem item, IWParagraph paragraph)
	{
		WTextRange wTextRange = item as WTextRange;
		XmlParagraphItem xmlParagraphItem = item as XmlParagraphItem;
		if (item is WFormField)
		{
			WriteFormField(item as WFormField);
		}
		else if (item is IWField)
		{
			WriteBeginField(item as WField);
		}
		else if (wTextRange != null)
		{
			WriteText(wTextRange);
		}
		else if (item is WPicture && (item as WPicture).ImageRecord != null)
		{
			WriteImage(item as WPicture);
		}
		else if (item is BookmarkStart)
		{
			WriteBookmarkStart(item as BookmarkStart);
		}
		else if (item is BookmarkEnd)
		{
			WriteBookmarkEnd(item as BookmarkEnd);
		}
		else if (item is WSymbol)
		{
			WriteSymbol(item as WSymbol);
		}
		else if (item is IWTextBox)
		{
			WriteTextBoxShape(item as WTextBox);
		}
		else if (item is ShapeObject)
		{
			if (item.IsNotFieldShape())
			{
				WriteShapeObject(item as ShapeObject);
			}
		}
		else if (item is WFieldMark)
		{
			WriteFieldMarkAndText(item as WFieldMark);
		}
		else if (item is WComment)
		{
			WriteComment(item as WComment);
		}
		else if (item is WFootnote)
		{
			WriteFootnote(item as WFootnote);
		}
		else if (item is Break)
		{
			WriteBreak(item as Break, (WParagraph)paragraph);
		}
		else if (item is Watermark)
		{
			WriteWatermark(item as Watermark);
		}
		else if (item is TableOfContent)
		{
			WriteTOC(item as TableOfContent);
		}
		else if (item is WCommentMark)
		{
			WriteCommMark(item as WCommentMark);
		}
		else if (item is WOleObject)
		{
			WriteOleObject(item as WOleObject);
		}
		else if (item is WAbsoluteTab)
		{
			WriteAbsoluteTab(item as WAbsoluteTab);
		}
		else if (item is InlineContentControl)
		{
			ParagraphItemCollection paragraphItems = (item as InlineContentControl).ParagraphItems;
			for (int i = 0; i < paragraphItems.Count; i++)
			{
				WriteParaItem(paragraphItems[i], paragraph);
			}
		}
		else
		{
			if (xmlParagraphItem == null || xmlParagraphItem.MathParaItemsCollection == null || xmlParagraphItem.MathParaItemsCollection.Count <= 0)
			{
				return;
			}
			foreach (ParagraphItem item2 in xmlParagraphItem.MathParaItemsCollection)
			{
				WriteParaItem(item2, paragraph);
			}
		}
	}

	private void WriteDocumentEscher()
	{
		EscherClass escher = m_document.Escher;
		if (escher != null)
		{
			(m_currWriter as WordWriterBase).Escher = escher;
		}
	}

	private void WriteAbsoluteTab(WAbsoluteTab absoluteTab)
	{
		WTextRange wTextRange = new WTextRange(m_document);
		wTextRange.Text = absoluteTab.Text;
		wTextRange.ApplyCharacterFormat(absoluteTab.CharacterFormat);
		WriteText(wTextRange);
	}

	private void WriteWatermarkParagraphs()
	{
		if (m_document.Sections.Count == 0)
		{
			m_document.AddSection();
		}
		foreach (WSection section in m_document.Sections)
		{
			for (int i = 0; i < 6; i++)
			{
				if (section.HeadersFooters[i].ChildEntities.Count == 0 && section.HeadersFooters[i].WriteWatermark)
				{
					WParagraph entity = new WParagraph(m_document);
					section.HeadersFooters[i].ChildEntities.Add(entity);
				}
			}
		}
	}

	private void WriteText(WTextRange text)
	{
		UpdateCharStyleIndex(text.CharacterFormat.CharStyleName, isParaBreak: false);
		CharacterPropertiesConverter.FormatToSprms(text.CharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
		if (text.Text != SpecialCharacters.FootnoteAsciiStr)
		{
			string text2 = text.Text;
			if (CurrentField is WTextFormField)
			{
				text2 = FormFieldPropertiesConverter.FormatText(CurrentField.TextFormat, text2);
			}
			else if (CurrentField != null && CurrentField.IsFieldWithoutSeparator && CurrentWriter.CHPX.PropertyModifiers[2050] == null)
			{
				CurrentWriter.CHPX.PropertyModifiers.SetBoolValue(2050, flag: true);
			}
			WriteTextChunks(text2, text.SafeText);
		}
		else
		{
			CurrentWriter.WriteMarker(WordChunkType.Footnote);
		}
		CurrentWriter.CHPX.PropertyModifiers.Clear();
	}

	private void WriteTextChunks(string text, bool safeText)
	{
		if (safeText)
		{
			CurrentWriter.WriteSafeChunk(text);
		}
		else
		{
			CurrentWriter.WriteChunk(text);
		}
	}

	private void WriteBeginField(WField field)
	{
		if (field.FieldEnd == null && field.FieldType == FieldType.FieldUnknown)
		{
			return;
		}
		if (field is WMergeField)
		{
			(field as WMergeField).UpdateFieldMarks();
		}
		else if (field is WIfField)
		{
			(field as WIfField).UpdateExpString();
		}
		else if (field is WSeqField)
		{
			(field as WSeqField).UpdateFieldMarks();
		}
		m_fieldStack.Push(field);
		UpdateCharStyleIndex(field.CharacterFormat.CharStyleName, isParaBreak: false);
		CharacterPropertiesConverter.FormatToSprms(field.CharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
		string text = (field.IsFormField() ? FieldTypeDefiner.GetFieldCode(field.FieldType) : string.Empty);
		if (field.IsFieldWithoutSeparator)
		{
			if (field.FieldSeparator != null && field.FieldEnd != null)
			{
				field.RemoveFieldSeparator(field.FieldEnd);
				field.FieldSeparator = null;
			}
			CurrentWriter.CHPXStickProperties = true;
			if (CurrentWriter.CHPX.PropertyModifiers[2050] == null)
			{
				CurrentWriter.CHPX.PropertyModifiers.SetBoolValue(2050, flag: true);
			}
			CurrentWriter.WriteMarker(WordChunkType.FieldBeginMark);
			if (!string.IsNullOrEmpty(text))
			{
				CurrentWriter.WriteSafeChunk(text);
			}
			CurrentWriter.CHPXStickProperties = false;
		}
		else
		{
			CurrentWriter.InsertStartField(text, field, hasSeparator: false);
		}
	}

	private void WriteOleObjectCharProps(WField field)
	{
		if (field.Owner is WOleObject)
		{
			WOleObject obj = field.Owner as WOleObject;
			int result = 0;
			if (int.TryParse(obj.OleStorageName, out result))
			{
				CurrentWriter.CHPX.PropertyModifiers.SetIntValue(27139, result);
			}
			CurrentWriter.CHPX.PropertyModifiers.SetBoolValue(2058, flag: true);
		}
	}

	private void WriteFormField(WFormField field)
	{
		m_fieldStack.Push(field);
		FormField formField = null;
		if (field.HasFFData)
		{
			formField = new FormField(field.FieldType);
			FormFieldPropertiesConverter.WriteFormFieldProperties(formField, field);
		}
		CharacterPropertiesConverter.FormatToSprms(field.CharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
		string empty = string.Empty;
		CurrentWriter.InsertFormField(empty, formField, field);
		if (formField != null && field.FormFieldType == FormFieldType.TextInput && (field as WTextFormField).TextRange.Text.Length == 0 && formField.DefaultTextInputValue.Length > 0)
		{
			(field as WTextFormField).TextRange.Text = formField.DefaultTextInputValue;
		}
	}

	private void WriteTable(IWTable table)
	{
		CurrentWriter.PAPX.PropertyModifiers.Clear();
		if (table != null)
		{
			(table as WTable).UpdateGridSpan();
			int i = 0;
			for (int count = table.Rows.Count; i < count; i++)
			{
				m_tableNestingLevel++;
				WTableRow wTableRow = table.Rows[i];
				int count2 = wTableRow.Cells.Count;
				for (int j = 0; j < count2; j++)
				{
					WTableCell wTableCell = wTableRow.Cells[j];
					CurrentWriter.CHPX.PropertyModifiers.Clear();
					SetTableNestingLevel(CurrentWriter.PAPX.PropertyModifiers, m_tableNestingLevel);
					WriteParagraphs((BodyItemCollection)wTableCell.ChildEntities, isTableBody: true);
					UpdateCharStyleIndex(wTableCell.CharacterFormat.CharStyleName, isParaBreak: false);
					CharacterPropertiesConverter.FormatToSprms(wTableCell.CharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
					SetTableNestingLevel(CurrentWriter.PAPX.PropertyModifiers, m_tableNestingLevel);
					CurrentWriter.WriteCellMark(m_tableNestingLevel);
					int k = 0;
					for (int count3 = m_bookmarksAfterCell.Count; k < count3; k++)
					{
						CurrentWriter.InsertBookmarkEnd(m_bookmarksAfterCell[k]);
					}
					m_bookmarksAfterCell.Clear();
				}
				if (m_tableNestingLevel == 1)
				{
					SetCellMark(CurrentWriter.PAPX.PropertyModifiers, value: true);
					CurrentWriter.PAPX.PropertyModifiers.SetBoolValue(9239, flag: true);
					SetTableNestingLevel(CurrentWriter.PAPX.PropertyModifiers, m_tableNestingLevel);
				}
				else
				{
					SetCellMark(CurrentWriter.PAPX.PropertyModifiers, value: true);
					SetTableNestingLevel(CurrentWriter.PAPX.PropertyModifiers, m_tableNestingLevel);
					CurrentWriter.PAPX.PropertyModifiers.SetBoolValue(9291, flag: true);
					CurrentWriter.PAPX.PropertyModifiers.SetBoolValue(9292, flag: true);
				}
				if ((table as WTable).IsFromHTML)
				{
					UpdateHtmlTableBorders(table as WTable);
				}
				WriteTableProps(CurrentWriter, wTableRow, table);
				CurrentWriter.WriteRowMark(m_tableNestingLevel, count2);
				m_tableNestingLevel--;
			}
		}
		CurrentWriter.PAPX.PropertyModifiers.Clear();
	}

	private void UpdateHtmlTableBorders(WTable table)
	{
		ApplyHtmlTableBorder(table.TableFormat.Borders.Top);
		ApplyHtmlTableBorder(table.TableFormat.Borders.Left);
		ApplyHtmlTableBorder(table.TableFormat.Borders.Right);
		ApplyHtmlTableBorder(table.TableFormat.Borders.Bottom);
		table.TableFormat.Borders.Horizontal.BorderType = BorderStyle.Cleared;
		table.TableFormat.Borders.Vertical.BorderType = BorderStyle.Cleared;
	}

	private void ApplyHtmlTableBorder(Border border)
	{
		if (border.LineWidth == 0f || border.BorderType != 0 || border.HasNoneStyle)
		{
			border.HasNoneStyle = true;
		}
	}

	private void SetTableNestingLevel(SinglePropertyModifierArray sprms, int value)
	{
		if (value > 0)
		{
			sprms.SetIntValue(26185, value);
		}
		else
		{
			sprms.RemoveValue(26185);
		}
	}

	private void SetCellMark(SinglePropertyModifierArray sprms, bool value)
	{
		if (value)
		{
			sprms.SetBoolValue(9238, flag: true);
		}
		else
		{
			sprms.RemoveValue(9238);
		}
	}

	private void WriteTableProps(IWordWriterBase writer, WTableRow row, IWTable table)
	{
		TablePropertiesConverter.FormatToSprms(row, CurrentWriter.PAPX.PropertyModifiers, writer.StyleSheet);
		CharacterPropertiesConverter.FormatToSprms(row.CharacterFormat, writer.CHPX.PropertyModifiers, writer.StyleSheet);
	}

	private void WriteImage(IWPicture picture)
	{
		WPicture wPicture = picture as WPicture;
		wPicture.IsHeaderPicture = ((CurrentWriter is WordHeaderFooterWriter || CurrentWriter is WordHFTextBoxWriter) ? true : false);
		int height = (int)Math.Round(wPicture.Size.Height * 20f);
		int width = (int)Math.Round(wPicture.Size.Width * 20f);
		if (wPicture.TextWrappingStyle == TextWrappingStyle.Inline)
		{
			WriteInlinePicture(wPicture, height, width);
		}
		else
		{
			WritePictureShape(wPicture, height, width);
		}
	}

	private void WritePictureShape(WPicture wPict, int height, int width)
	{
		CheckShapeForCloning(wPict);
		if (wPict.CharacterFormat != null && CurrentWriter != null && CurrentWriter.CHPX != null)
		{
			CharacterPropertiesConverter.FormatToSprms(wPict.CharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
		}
		int yaTop = (int)Math.Round(wPict.VerticalPosition * 20f);
		int xaLeft = (int)Math.Round(wPict.HorizontalPosition * 20f);
		PictureShapeProps pictureShapeProps = new PictureShapeProps();
		if (wPict.HorizontalOrigin == HorizontalOrigin.LeftMargin || wPict.HorizontalOrigin == HorizontalOrigin.RightMargin || wPict.HorizontalOrigin == HorizontalOrigin.InsideMargin || wPict.HorizontalOrigin == HorizontalOrigin.OutsideMargin)
		{
			pictureShapeProps.RelHrzPos = HorizontalOrigin.Margin;
		}
		else
		{
			pictureShapeProps.RelHrzPos = wPict.HorizontalOrigin;
		}
		pictureShapeProps.RelVrtPos = wPict.VerticalOrigin;
		pictureShapeProps.XaLeft = xaLeft;
		pictureShapeProps.YaTop = yaTop;
		pictureShapeProps.Width = (int)((float)width / 100f * wPict.WidthScale);
		pictureShapeProps.Height = (int)((float)height / 100f * wPict.HeightScale);
		pictureShapeProps.TextWrappingType = wPict.TextWrappingType;
		if (wPict.TextWrappingStyle == TextWrappingStyle.Behind)
		{
			pictureShapeProps.IsBelowText = true;
		}
		else
		{
			pictureShapeProps.IsBelowText = wPict.IsBelowText;
		}
		pictureShapeProps.TextWrappingStyle = wPict.TextWrappingStyle;
		pictureShapeProps.HorizontalAlignment = wPict.HorizontalAlignment;
		pictureShapeProps.VerticalAlignment = wPict.VerticalAlignment;
		pictureShapeProps.Spid = wPict.ShapeId;
		pictureShapeProps.TxbxCount = 0;
		pictureShapeProps.AlternativeText = wPict.AlternativeText;
		pictureShapeProps.Name = wPict.Name;
		CurrentWriter.InsertShape(wPict, pictureShapeProps);
		if (wPict.EmbedBody != null)
		{
			WriteEmbedBody(wPict.EmbedBody, pictureShapeProps.Spid);
		}
	}

	private void WriteEmbedBody(WTextBody text, int shapeId)
	{
		WTextBox wTextBox = new WTextBox(m_document);
		wTextBox.SetTextBody(text);
		for (int i = 0; i < text.Items.Count; i++)
		{
			TextBodyItem textBodyItem = text.Items[i];
			if (!(textBodyItem is WParagraph))
			{
				continue;
			}
			WParagraph wParagraph = textBodyItem as WParagraph;
			for (int num = wParagraph.Items.Count - 1; num >= 0; num--)
			{
				ParagraphItem paragraphItem = wParagraph.Items[num];
				if ((paragraphItem is BookmarkStart && !(paragraphItem as BookmarkStart).Name.StartsWith("OLE_LINK")) || (paragraphItem is BookmarkEnd && !(paragraphItem as BookmarkEnd).Name.StartsWith("OLE_LINK")))
				{
					wParagraph.Items.Remove(paragraphItem);
				}
			}
		}
		wTextBox.TextBoxSpid = shapeId;
		wTextBox.TextBoxFormat.TextBoxIdentificator = (m_currWriter as WordWriterBase).NextTextId;
		PrepareTextBoxColl(wTextBox);
		if (m_document.Escher.FindContainerBySpid(shapeId) is MsofbtSpContainer msofbtSpContainer)
		{
			if (msofbtSpContainer.ShapeOptions.Properties[267] is FOPTEBid fOPTEBid)
			{
				fOPTEBid.Value = (uint)wTextBox.TextBoxFormat.TextBoxIdentificator;
			}
			else
			{
				msofbtSpContainer.ShapeOptions.Properties.Add(new FOPTEBid(267, isBid: false, (uint)wTextBox.TextBoxFormat.TextBoxIdentificator));
			}
		}
	}

	private void WriteInlinePicture(WPicture wPict, int height, int width)
	{
		CheckShapeForCloning(wPict);
		CharacterPropertiesConverter.FormatToSprms(wPict.CharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
		if (wPict.Document.Settings.CompatibilityMode != 0 && wPict.PictureShape.ShapeContainer != null && wPict.PictureShape.ShapeContainer.ShapeOptions != null)
		{
			wPict.PictureShape = ConvertToInlineShape(wPict.PictureShape);
		}
		if (wPict.PictureShape.ShapeContainer == null || wPict.PictureShape.ShapeContainer.Shape == null || (wPict.PictureShape.ShapeContainer != null && wPict.PictureShape.ShapeContainer.Bse.Blip.IsDib) || (wPict.IsMetaFile && wPict.PictureShape.ShapeContainer.Bse.Blip is MsofbtImage) || wPict.Document.IsReadOnly)
		{
			CurrentWriter.InsertImage(wPict, height, width);
			return;
		}
		wPict.PictureShape.ShapeContainer.CheckOptContainer();
		PictureShapeProps pictureShapeProps = new PictureShapeProps();
		pictureShapeProps.AlternativeText = wPict.AlternativeText;
		pictureShapeProps.Name = wPict.Name;
		wPict.PictureShape.ShapeContainer.WritePictureOptions(pictureShapeProps, wPict);
		wPict.PictureShape.PictureDescriptor.SetBasePictureOptions(height, width, wPict.HeightScale, wPict.WidthScale);
		(m_currWriter as WordWriterBase).InsertInlineShapeObject(wPict.PictureShape);
		if (wPict.PictureShape.ShapeContainer.Bse.IsPictureInShapeField && IsInShapeField(wPict))
		{
			wPict.PictureShape.ShapeContainer.Bse.IsInlineBlip = false;
		}
	}

	private bool IsInShapeField(WPicture picture)
	{
		for (IEntity previousSibling = picture.PreviousSibling; previousSibling != null; previousSibling = previousSibling.PreviousSibling)
		{
			if (previousSibling is WFieldMark)
			{
				WFieldMark wFieldMark = previousSibling as WFieldMark;
				if (wFieldMark.Type == FieldMarkType.FieldSeparator && wFieldMark.ParentField != null && wFieldMark.ParentField.FieldType == FieldType.FieldShape)
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	private InlineShapeObject ConvertToInlineShape(InlineShapeObject pictureShape)
	{
		uint num = 0u;
		if (pictureShape.ShapeContainer.ShapeOptions.Properties.ContainsKey(459))
		{
			num = pictureShape.ShapeContainer.GetPropertyValue(459);
		}
		num = (uint)Math.Round((double)num / 12700.0 * 8.0);
		pictureShape.PictureDescriptor.BorderLeft.LineWidth = (byte)num;
		pictureShape.PictureDescriptor.BorderTop.LineWidth = (byte)num;
		pictureShape.PictureDescriptor.BorderRight.LineWidth = (byte)num;
		pictureShape.PictureDescriptor.BorderBottom.LineWidth = (byte)num;
		BorderStyle borderStyle = BorderStyle.None;
		if (pictureShape.ShapeContainer.ShapeOptions.Properties.ContainsKey(461))
		{
			TextBoxLineStyle propertyValue = (TextBoxLineStyle)pictureShape.ShapeContainer.GetPropertyValue(461);
			borderStyle = pictureShape.GetBorderStyle(LineDashing.Solid, propertyValue);
			if (propertyValue == TextBoxLineStyle.Simple)
			{
				borderStyle = BorderStyle.Single;
			}
		}
		if (pictureShape.ShapeContainer.ShapeOptions.Properties.ContainsKey(462))
		{
			LineDashing propertyValue2 = (LineDashing)pictureShape.ShapeContainer.GetPropertyValue(462);
			borderStyle = pictureShape.GetBorderStyle(propertyValue2, TextBoxLineStyle.Simple);
			if (propertyValue2 == LineDashing.Solid && borderStyle == BorderStyle.None)
			{
				borderStyle = BorderStyle.Single;
			}
		}
		if (borderStyle != 0)
		{
			pictureShape.PictureDescriptor.BorderLeft.BorderType = (byte)borderStyle;
			pictureShape.PictureDescriptor.BorderTop.BorderType = (byte)borderStyle;
			pictureShape.PictureDescriptor.BorderRight.BorderType = (byte)borderStyle;
			pictureShape.PictureDescriptor.BorderBottom.BorderType = (byte)borderStyle;
		}
		if (pictureShape.ShapeContainer.ShapeOptions.Properties.ContainsKey(448))
		{
			uint propertyValue3 = pictureShape.ShapeContainer.GetPropertyValue(448);
			int num2 = WordColor.ConvertColorToId(WordColor.ConvertRGBToColor(propertyValue3));
			pictureShape.PictureDescriptor.BorderLeft.LineColor = (byte)num2;
			pictureShape.PictureDescriptor.BorderTop.LineColor = (byte)num2;
			pictureShape.PictureDescriptor.BorderRight.LineColor = (byte)num2;
			pictureShape.PictureDescriptor.BorderBottom.LineColor = (byte)num2;
			pictureShape.ShapeContainer.ShapePosition.SetPropertyValue(924, propertyValue3);
			pictureShape.ShapeContainer.ShapePosition.SetPropertyValue(923, propertyValue3);
			pictureShape.ShapeContainer.ShapePosition.SetPropertyValue(926, propertyValue3);
			pictureShape.ShapeContainer.ShapePosition.SetPropertyValue(925, propertyValue3);
		}
		return pictureShape;
	}

	private void WriteShapeObject(ShapeObject shapeObject)
	{
		CheckShapeForCloning(shapeObject);
		CharacterPropertiesConverter.FormatToSprms(shapeObject.CharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
		if (shapeObject is InlineShapeObject)
		{
			(m_currWriter as WordWriterBase).InsertInlineShapeObject(shapeObject as InlineShapeObject);
			return;
		}
		(m_currWriter as WordWriterBase).InsertShapeObject(shapeObject);
		WriteShapeObjTextBody(shapeObject);
	}

	private void WriteShapeObjTextBody(ShapeObject shapeObject)
	{
		if (m_currWriter is WordHeaderFooterWriter)
		{
			int i = 0;
			for (int count = shapeObject.AutoShapeTextCollection.Count; i < count; i++)
			{
				HFTextBoxCollection.Add(shapeObject.AutoShapeTextCollection[i] as WTextBox);
			}
		}
		else
		{
			int j = 0;
			for (int count2 = shapeObject.AutoShapeTextCollection.Count; j < count2; j++)
			{
				TextBoxCollection.Add(shapeObject.AutoShapeTextCollection[j] as WTextBox);
			}
		}
	}

	private void WriteSectionEnd(IWSection section)
	{
		if (CurrentWriter is WordWriter)
		{
			if (m_secNumber != 0)
			{
				SectionPropertiesConverter.Import(m_mainWriter.SectionProperties, section as WSection);
				m_mainWriter.WriteMarker(WordChunkType.SectionEnd);
			}
			m_currSection = section;
			m_secNumber++;
		}
	}

	private void WriteBookmarkStart(BookmarkStart start)
	{
		m_mainWriter.InsertBookmarkStart(start.Name, start);
	}

	private void WriteBookmarkEnd(BookmarkEnd end)
	{
		m_mainWriter.InsertBookmarkEnd(end.Name);
	}

	private void WriteBreak(Break docBreak, WParagraph paragraph)
	{
		CharacterPropertiesConverter.FormatToSprms(docBreak.CharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
		if (docBreak.BreakType == BreakType.ColumnBreak)
		{
			m_mainWriter.WriteMarker(WordChunkType.ColumnBreak);
		}
		else if (docBreak.BreakType == BreakType.PageBreak)
		{
			if (CurrentWriter is WordWriter)
			{
				(CurrentWriter as WordWriter).InsertPageBreak();
			}
		}
		else if ((docBreak.BreakType == BreakType.LineBreak || docBreak.BreakType == BreakType.TextWrappingBreak) && docBreak.TextRange.Text == ControlChar.CarriegeReturn)
		{
			WriteText(docBreak.TextRange);
		}
		else
		{
			CurrentWriter.WriteMarker(WordChunkType.LineBreak);
		}
	}

	private void WriteSymbol(WSymbol symbol)
	{
		if (CurrentWriter.StyleSheet.FontNameToIndex(symbol.FontName) == -1)
		{
			CurrentWriter.StyleSheet.UpdateFontName(symbol.FontName);
		}
		UpdateCharStyleIndex(symbol.CharacterFormat.CharStyleName, isParaBreak: false);
		CharacterPropertiesConverter.FormatToSprms(symbol.CharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
		SymbolDescriptor symbolDescriptor = new SymbolDescriptor();
		symbolDescriptor.CharCode = symbol.CharacterCode;
		symbolDescriptor.CharCodeExt = symbol.CharCodeExt;
		symbolDescriptor.FontCode = (short)CurrentWriter.StyleSheet.FontNameToIndex(symbol.FontName);
		CurrentWriter.CHPX.PropertyModifiers.SetByteArrayValue(27145, symbolDescriptor.Save());
		CurrentWriter.WriteMarker(WordChunkType.Symbol);
	}

	private void WriteTextBoxShape(WTextBox textBoxItem)
	{
		textBoxItem.TextBoxFormat.IsHeaderTextBox = CurrentWriter is WordHeaderFooterWriter;
		CheckShapeForCloning(textBoxItem);
		if (textBoxItem.CharacterFormat != null && CurrentWriter != null && CurrentWriter.CHPX != null)
		{
			CharacterPropertiesConverter.FormatToSprms(textBoxItem.CharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
		}
		new TextBoxProps();
		textBoxItem.TextBoxSpid = CurrentWriter.InsertTextBox(textBoxItem.Visible, textBoxItem.TextBoxFormat);
		PrepareTextBoxColl(textBoxItem);
	}

	private void PrepareTextBoxColl(WTextBox textBoxItem)
	{
		if (m_currWriter is WordHeaderFooterWriter)
		{
			HFTextBoxCollection.Add(textBoxItem);
		}
		else
		{
			TextBoxCollection.Add(textBoxItem);
		}
	}

	private void CheckShapeForCloning(ParagraphItem shapeItem)
	{
		if (shapeItem.IsCloned)
		{
			m_document.CloneShapeEscher(m_document, shapeItem);
		}
	}

	private void WriteFieldMarkAndText(WFieldMark fldMark)
	{
		if (CurrentField == null)
		{
			return;
		}
		if (fldMark.Type == FieldMarkType.FieldEnd && CurrentField != null && CurrentField.FieldType == FieldType.FieldDocVariable && fldMark.PreviousSibling is WField && m_document.UpdateFields)
		{
			bool cHPXStickProperties = CurrentWriter.CHPXStickProperties;
			CurrentWriter.CHPXStickProperties = true;
			WriteFieldSeparator();
			CurrentWriter.CHPXStickProperties = cHPXStickProperties;
		}
		CharacterPropertiesConverter.FormatToSprms((CurrentField is WFormField) ? CurrentField.CharacterFormat : fldMark.CharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
		if (fldMark.Type == FieldMarkType.FieldSeparator)
		{
			WriteFieldSeparator();
		}
		else if (CurrentField.IsFieldWithoutSeparator)
		{
			if (CurrentWriter.CHPX.PropertyModifiers[2050] == null)
			{
				CurrentWriter.CHPX.PropertyModifiers.SetBoolValue(2050, flag: true);
			}
			CurrentWriter.WriteMarker(WordChunkType.FieldEndMark);
			m_fieldStack.Pop();
		}
		else
		{
			WriteFieldEnd();
		}
	}

	private void WriteFieldSeparator()
	{
		if (CurrentField != null)
		{
			if (CurrentField.FieldType == FieldType.FieldEmbed || CurrentField.FieldType == FieldType.FieldLink)
			{
				WriteOleObjectCharProps(CurrentField);
			}
			else if (CurrentField.FieldType == FieldType.FieldOCX)
			{
				CurrentWriter.CHPX.PropertyModifiers.SetIntValue(27139, (CurrentField as WControlField).StoragePicLocation);
			}
		}
		CurrentWriter.InsertFieldSeparator();
	}

	private void WriteFieldEnd()
	{
		CurrentWriter.InsertEndField();
		if (m_fieldStack.Count > 0)
		{
			m_fieldStack.Pop();
		}
	}

	private void WriteComment(WComment comment)
	{
		if (comment.AppendItems)
		{
			WriteCommItems(comment);
		}
		else
		{
			CountCommOffset(comment);
		}
		CommentCollection.Add(comment);
		WCommentFormat format = comment.Format;
		(CurrentWriter as WordWriter).InsertComment(format);
	}

	private void WriteFootnote(WFootnote footnote)
	{
		footnote.EnsureFtnMarker();
		if (footnote.FootnoteType == FootnoteType.Footnote)
		{
			FootnoteCollection.Add(footnote);
		}
		else
		{
			EndnoteCollection.Add(footnote);
		}
		UpdateCharStyleIndex(footnote.MarkerCharacterFormat.CharStyleName, isParaBreak: false);
		CharacterPropertiesConverter.FormatToSprms(footnote.MarkerCharacterFormat, CurrentWriter.CHPX.PropertyModifiers, CurrentWriter.StyleSheet);
		(CurrentWriter as WordWriter).InsertFootnote(footnote);
	}

	private void WriteWatermark(Watermark watermark)
	{
		if (!(watermark is PictureWatermark) || (watermark as PictureWatermark).Picture != null)
		{
			SizeF pageSize = m_document.LastSection.PageSetup.PageSize;
			MarginsF margins = m_document.LastSection.PageSetup.Margins;
			float maxWidth = pageSize.Width - margins.Left - margins.Right;
			CurrentWriter.InsertWatermark(watermark, UnitsConvertor.Instance, maxWidth);
		}
	}

	private void WriteTOC(TableOfContent toc)
	{
		WriteBeginField(toc.TOCField);
	}

	private void WriteFieldWithoutSeparator(string fieldCode, WField field)
	{
		if (field.FieldEnd != null)
		{
			CurrentWriter.CHPXStickProperties = true;
			if (CurrentWriter.CHPX.PropertyModifiers[2050] == null)
			{
				CurrentWriter.CHPX.PropertyModifiers.SetBoolValue(2050, flag: true);
			}
			CurrentWriter.WriteMarker(WordChunkType.FieldBeginMark);
			CurrentWriter.WriteSafeChunk(fieldCode);
			CurrentWriter.CHPXStickProperties = false;
			CurrentWriter.WriteMarker(WordChunkType.FieldEndMark);
		}
		else if (!string.IsNullOrEmpty(fieldCode))
		{
			CurrentWriter.WriteSafeChunk(fieldCode);
		}
	}

	private void WriteCommMark(WCommentMark commMark)
	{
		if (commMark.Type == CommentMarkType.CommentStart)
		{
			DictionaryEntry value = new DictionaryEntry((CurrentWriter as WordWriter).GetTextPos(), 0);
			if (!CommentOffsets.ContainsKey(commMark.CommentId))
			{
				CommentOffsets.Add(commMark.CommentId, value);
			}
		}
		else if (CommentOffsets.ContainsKey(commMark.CommentId))
		{
			DictionaryEntry value = CommentOffsets[commMark.CommentId];
			value.Value = (CurrentWriter as WordWriter).GetTextPos();
			CommentOffsets[commMark.CommentId] = value;
		}
	}

	private void WriteOleObject(WOleObject oleObject)
	{
		WField field = oleObject.Field;
		WriteBeginField(field);
	}

	private void AddListPictures()
	{
		foreach (ListStyle listStyle in m_document.ListStyles)
		{
			if (listStyle == null || listStyle.Levels == null)
			{
				break;
			}
			int i = 0;
			for (int count = listStyle.Levels.Count; i < count; i++)
			{
				WListLevel wListLevel = listStyle.Levels[i];
				if (wListLevel != null && wListLevel.PicBullet != null)
				{
					ListPicture.Add(wListLevel.PicBullet);
					int listPictureIndex = ListPicture.Count - 1;
					wListLevel.CharacterFormat.ListPictureIndex = listPictureIndex;
					wListLevel.CharacterFormat.ListHasPicture = true;
				}
			}
		}
	}

	private void AddPictures(WListFormat listFormat)
	{
	}

	private void WriteParagraphProperties(IWParagraph paragraph)
	{
		List<SinglePropertyModifierRecord> list = new List<SinglePropertyModifierRecord>();
		List<SinglePropertyModifierRecord> list2 = new List<SinglePropertyModifierRecord>();
		WriteListProperties(paragraph, list2, list);
		if (paragraph.IsInCell && CurrentWriter.PAPX.PropertyModifiers.GetInt(26185, 1) > 1 && paragraph != (paragraph.Owner as WTableCell).LastParagraph)
		{
			CurrentWriter.PAPX.PropertyModifiers.RemoveValue(9291);
		}
		ParagraphPropertiesConverter.FormatToSprms(paragraph.ParagraphFormat, CurrentWriter.PAPX.PropertyModifiers, CurrentWriter.StyleSheet);
		if (paragraph.ParagraphFormat.m_unParsedSprms != null && paragraph.ParagraphFormat.m_unParsedSprms.Count > 0)
		{
			foreach (SinglePropertyModifierRecord unParsedSprm in paragraph.ParagraphFormat.m_unParsedSprms)
			{
				CurrentWriter.PAPX.PropertyModifiers.InsertAt(unParsedSprm.Clone(), 0);
			}
		}
		if (list2.Count > 0)
		{
			foreach (SinglePropertyModifierRecord item in list2)
			{
				CurrentWriter.PAPX.PropertyModifiers.InsertAt(item.Clone(), 0);
			}
		}
		if (list.Count > 0)
		{
			foreach (SinglePropertyModifierRecord item2 in list)
			{
				CurrentWriter.PAPX.PropertyModifiers.Add(item2.Clone());
			}
		}
		WriteParagraphStyle(CurrentWriter, paragraph);
		CurrentWriter.PAPX.PropertyModifiers.SortSprms();
		CurrentWriter.BreakCHPX.PropertyModifiers.Clear();
		UpdateCharStyleIndex(paragraph.BreakCharacterFormat.CharStyleName, isParaBreak: true);
		CharacterPropertiesConverter.FormatToSprms(paragraph.BreakCharacterFormat, CurrentWriter.BreakCHPX.PropertyModifiers, CurrentWriter.StyleSheet);
	}

	private void WriteParagraphStyle(IWordWriterBase writer, IWParagraph paragraph)
	{
		string styleName = paragraph.StyleName;
		if (styleName != null)
		{
			int num = writer.StyleSheet.StyleNameToIndex(styleName, isCharacter: false);
			if (num > -1)
			{
				if (num == 0 && paragraph.ParagraphFormat.m_unParsedSprms != null && paragraph.ParagraphFormat.m_unParsedSprms.Contain(17920))
				{
					num = paragraph.ParagraphFormat.m_unParsedSprms[17920].ShortValue;
				}
				writer.CurrentStyleIndex = num;
				writer.PAPX.PropertyModifiers.SetByteArrayValue(17920, BitConverter.GetBytes((ushort)num));
			}
		}
		else
		{
			writer.CurrentStyleIndex = 0;
		}
	}

	private void WriteStyleSheet(IWordWriter writer)
	{
		WordStyleSheet styleSheet = writer.StyleSheet;
		UpdateDefFormat();
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		Style style = null;
		int i = 0;
		for (int count = m_document.Styles.Count; i < count; i++)
		{
			style = m_document.Styles[i] as Style;
			if (style is WTableStyle)
			{
				continue;
			}
			WParagraphStyle wParagraphStyle = style as WParagraphStyle;
			WCharacterStyle wCharacterStyle = style as WCharacterStyle;
			bool flag = style.StyleType == StyleType.CharacterStyle;
			int num = 0;
			num = (list.Contains(style.Name) ? (-1) : styleSheet.StyleNameToIndex(style.Name, flag));
			list.Add(style.Name);
			WordStyle wordStyle;
			if (((style.StyleId > 0 && style.StyleId < 10) || style.StyleId == 105 || style.StyleId == 107) && !list2.Contains(style.StyleId))
			{
				wordStyle = new WordStyle(styleSheet, style.Name);
				wordStyle.ID = style.StyleId;
				if (style.StyleId > 0 && style.StyleId < 10 && !list2.Contains(style.StyleId))
				{
					styleSheet.RemoveStyleByIndex(wordStyle.ID);
					styleSheet.InsertStyle(wordStyle.ID, wordStyle);
				}
				else if (style.StyleId == 105 && !list2.Contains(style.StyleId))
				{
					styleSheet.RemoveStyleByIndex(11);
					styleSheet.InsertStyle(11, wordStyle);
				}
				else if (style.StyleId == 107 && !list2.Contains(style.StyleId))
				{
					styleSheet.RemoveStyleByIndex(12);
					styleSheet.InsertStyle(12, wordStyle);
				}
				list2.Add(style.StyleId);
			}
			else if (num < 0)
			{
				num = styleSheet.StylesCount;
				switch (num)
				{
				case 13:
					if (m_document.Styles.FixedIndex13HasStyle && m_document.Styles.FixedIndex13StyleName != null && m_document.Styles.FixedIndex13StyleName != string.Empty)
					{
						wordStyle = styleSheet.CreateStyle(m_document.Styles.FixedIndex13StyleName, flag);
						wordStyle.ID = style.StyleId;
						break;
					}
					wordStyle = WordStyle.Empty;
					styleSheet.InsertStyle(13, wordStyle);
					num = styleSheet.StylesCount;
					if (m_document.Styles.FixedIndex14HasStyle && m_document.Styles.FixedIndex14StyleName != null && m_document.Styles.FixedIndex14StyleName != string.Empty)
					{
						continue;
					}
					wordStyle = WordStyle.Empty;
					styleSheet.InsertStyle(14, wordStyle);
					num = styleSheet.StylesCount;
					wordStyle = styleSheet.CreateStyle(style.Name, flag);
					wordStyle.ID = style.StyleId;
					break;
				case 14:
					if (m_document.Styles.FixedIndex14HasStyle && m_document.Styles.FixedIndex14StyleName != null && m_document.Styles.FixedIndex14StyleName != string.Empty)
					{
						wordStyle = styleSheet.CreateStyle(m_document.Styles.FixedIndex14StyleName, flag);
						wordStyle.ID = style.StyleId;
						if (m_document.Styles.FixedIndex14StyleName != style.Name)
						{
							i--;
						}
					}
					else
					{
						wordStyle = WordStyle.Empty;
						styleSheet.InsertStyle(14, wordStyle);
						num = styleSheet.StylesCount;
						wordStyle = styleSheet.CreateStyle(style.Name, flag);
						wordStyle.ID = style.StyleId;
					}
					break;
				default:
					num = styleSheet.StylesCount;
					wordStyle = styleSheet.CreateStyle(style.Name, flag);
					wordStyle.ID = style.StyleId;
					break;
				}
			}
			else
			{
				wordStyle = styleSheet.GetStyleByIndex(num);
			}
			if (wParagraphStyle != null)
			{
				ParagraphPropertiesConverter.FormatToSprms(wParagraphStyle.ParagraphFormat, wordStyle.PAPX.PropertyModifiers, styleSheet);
				UpdateListInStyle(writer, wParagraphStyle, wordStyle.PAPX.PropertyModifiers);
				wordStyle.PAPX.PropertyModifiers.SortSprms();
			}
			if (style.CharacterFormat.HasKey(68))
			{
				wordStyle.CHPX.PropertyModifiers.RemoveValue(19023);
			}
			CharacterPropertiesConverter.FormatToSprms(style.CharacterFormat, wordStyle.CHPX.PropertyModifiers, styleSheet);
			wordStyle.IsPrimary = style.IsPrimaryStyle;
			wordStyle.IsSemiHidden = style.IsSemiHidden;
			wordStyle.UnhideWhenUsed = style.UnhideWhenUsed;
			wordStyle.TypeCode = style.TypeCode;
			if (style.TableStyleData != null && style.TypeCode == WordStyleType.TableStyle)
			{
				wordStyle.TableStyleData = new byte[style.TableStyleData.Length];
				Buffer.BlockCopy(style.TableStyleData, 0, wordStyle.TableStyleData, 0, style.TableStyleData.Length);
			}
			if (wCharacterStyle != null)
			{
				try
				{
					m_charStylesHash.Add(style.Name, num);
				}
				catch
				{
				}
			}
		}
		int j = 0;
		for (int count2 = m_document.Styles.Count; j < count2; j++)
		{
			style = m_document.Styles[j] as Style;
			if (!string.IsNullOrEmpty(style.Name))
			{
				bool isCharacter = style.StyleType == StyleType.CharacterStyle;
				int index = styleSheet.StyleNameToIndex(style.Name, isCharacter);
				if (style.BaseStyle != null)
				{
					int baseStyleIndex = styleSheet.StyleNameToIndex(style.BaseStyle.Name, isCharacter);
					styleSheet.GetStyleByIndex(index).BaseStyleIndex = baseStyleIndex;
				}
				if (style.NextStyle != null)
				{
					int nextStyleIndex = styleSheet.StyleNameToIndex(style.NextStyle, isCharacter);
					styleSheet.GetStyleByIndex(index).NextStyleIndex = nextStyleIndex;
				}
				if (!string.IsNullOrEmpty(style.LinkedStyleName))
				{
					int linkStyleIndex = styleSheet.StyleNameToIndex(style.LinkedStyleName);
					styleSheet.GetStyleByIndex(index).LinkStyleIndex = linkStyleIndex;
				}
			}
		}
	}

	private void UpdateDefFormat()
	{
		if (!((m_document.Styles as StyleCollection).FindFirstStyleByName("Normal") is WParagraphStyle wParagraphStyle))
		{
			return;
		}
		if (m_document.m_defParaFormat != null)
		{
			if ((wParagraphStyle.ParagraphFormat.PropertiesHash == null || wParagraphStyle.ParagraphFormat.PropertiesHash.Count == 0) && wParagraphStyle.ParagraphFormat.IsDefault)
			{
				wParagraphStyle.ParagraphFormat.ImportContainer(m_document.m_defParaFormat);
			}
			else if (m_document.m_defParaFormat.PropertiesHash.Count > 0)
			{
				foreach (KeyValuePair<int, object> item in m_document.m_defParaFormat.PropertiesHash)
				{
					if (item.Key == 20 && wParagraphStyle.ParagraphFormat.PropertiesHash.ContainsKey(20))
					{
						ParagraphPropertiesConverter.CopyBorders((Borders)item.Value, wParagraphStyle.ParagraphFormat);
					}
					if (!wParagraphStyle.ParagraphFormat.PropertiesHash.ContainsKey(item.Key))
					{
						wParagraphStyle.ParagraphFormat.PropertiesHash.Add(item.Key, item.Value);
					}
				}
			}
		}
		if (m_document.DefCharFormat == null || m_document.DefCharFormat.PropertiesHash == null)
		{
			return;
		}
		if ((wParagraphStyle.CharacterFormat.PropertiesHash == null || wParagraphStyle.CharacterFormat.PropertiesHash.Count == 0) && wParagraphStyle.CharacterFormat.IsDefault)
		{
			wParagraphStyle.CharacterFormat.ImportContainer(m_document.DefCharFormat);
			return;
		}
		foreach (KeyValuePair<int, object> item2 in m_document.DefCharFormat.PropertiesHash)
		{
			if (item2.Key == 67 && wParagraphStyle.CharacterFormat.PropertiesHash.ContainsKey(67))
			{
				Border border = wParagraphStyle.CharacterFormat.Border;
				Border border2 = (Border)item2.Value;
				if (border2.IsBorderDefined && !border.IsBorderDefined)
				{
					ParagraphPropertiesConverter.ExportBorder(border, border2);
				}
			}
			if (!wParagraphStyle.CharacterFormat.PropertiesHash.ContainsKey(item2.Key))
			{
				wParagraphStyle.CharacterFormat.PropertiesHash.Add(item2.Key, item2.Value);
			}
		}
		if (wParagraphStyle.CharacterFormat.CharStyleName == null)
		{
			wParagraphStyle.CharacterFormat.CharStyleName = m_document.DefCharFormat.CharStyleName;
		}
		if (!wParagraphStyle.CharacterFormat.HasKey(68))
		{
			string text = m_document.DefCharFormat.FontNameAscii;
			if (m_document.DefCharFormat.IsThemeFont(text))
			{
				text = m_document.DefCharFormat.FontName;
			}
			wParagraphStyle.CharacterFormat.FontNameAscii = text;
		}
		if (!wParagraphStyle.CharacterFormat.HasKey(61))
		{
			string text = m_document.DefCharFormat.FontNameBidi;
			if (m_document.DefCharFormat.IsThemeFont(text))
			{
				text = m_document.DefCharFormat.FontName;
			}
			wParagraphStyle.CharacterFormat.FontNameBidi = text;
		}
		if (!wParagraphStyle.CharacterFormat.HasKey(69))
		{
			string text = m_document.DefCharFormat.FontNameFarEast;
			if (m_document.DefCharFormat.IsThemeFont(text))
			{
				text = m_document.DefCharFormat.FontName;
			}
			wParagraphStyle.CharacterFormat.FontNameFarEast = text;
		}
		if (!wParagraphStyle.CharacterFormat.HasKey(70))
		{
			string text = m_document.DefCharFormat.FontNameNonFarEast;
			if (m_document.DefCharFormat.IsThemeFont(text))
			{
				text = m_document.DefCharFormat.FontName;
			}
			wParagraphStyle.CharacterFormat.FontNameNonFarEast = text;
		}
	}

	private void WriteDocumentProperties()
	{
		if (m_document.BuiltinDocumentProperties != null)
		{
			m_mainWriter.BuiltinDocumentProperties = m_document.BuiltinDocumentProperties.Clone();
		}
		if (m_document.CustomDocumentProperties != null)
		{
			m_mainWriter.CustomDocumentProperties = m_document.CustomDocumentProperties.Clone();
		}
		m_mainWriter.WriteProtected = m_document.WriteProtected;
		m_mainWriter.HasPicture = m_document.HasPicture;
		m_mainWriter.SttbfRMark = m_document.SttbfRMark;
		if (m_document.MacrosData != null)
		{
			m_mainWriter.MacrosStream = new MemoryStream(m_document.MacrosData);
		}
		if (m_document.MacroCommands != null)
		{
			m_mainWriter.MacroCommands = m_document.MacroCommands;
		}
		if (m_document.GrammarSpellingData != null)
		{
			m_mainWriter.GrammarSpellingData = m_document.GrammarSpellingData;
		}
		if (m_document.DOP != null)
		{
			m_mainWriter.DOP = m_document.DOP;
			m_mainWriter.DOP.OddAndEvenPagesHeaderFooter = m_document.DifferentOddAndEvenPages;
			m_mainWriter.DOP.DefaultTabWidth = (ushort)Math.Round(m_document.DefaultTabWidth * 20f);
			m_mainWriter.DOP.UpdateDateTime(m_mainWriter.BuiltinDocumentProperties);
		}
		if (m_document.AssociatedStrings != null)
		{
			m_mainWriter.AssociatedStrings = m_document.AssociatedStrings.GetAssociatedStrings();
		}
		WriteDocumentDefaultFont();
		m_mainWriter.DOP.ViewType = (byte)m_document.ViewSetup.DocumentViewType;
		if (m_document.ViewSetup.ZoomType != 0)
		{
			m_mainWriter.DOP.ZoomType = (byte)m_document.ViewSetup.ZoomType;
		}
		if (m_document.ViewSetup.ZoomPercent != 100)
		{
			m_mainWriter.DOP.ZoomPercent = (ushort)m_document.ViewSetup.ZoomPercent;
		}
		if (m_document.Variables.Count > 0)
		{
			m_mainWriter.Variables = m_document.Variables.ToByteArray();
		}
	}

	private void WriteDocumentDefaultFont()
	{
		WCharacterFormat defCharFormat = m_document.DefCharFormat;
		string text = m_document.StandardAsciiFont;
		if (string.IsNullOrEmpty(text) && defCharFormat != null && defCharFormat.HasValue(68))
		{
			text = defCharFormat.FontNameAscii;
			if (defCharFormat.IsThemeFont(text))
			{
				text = defCharFormat.FontName;
			}
		}
		m_mainWriter.StandardAsciiFont = text;
		text = m_document.StandardFarEastFont;
		if (string.IsNullOrEmpty(text) && defCharFormat != null && defCharFormat.HasValue(69))
		{
			text = defCharFormat.FontNameFarEast;
			if (defCharFormat.IsThemeFont(text))
			{
				text = defCharFormat.FontName;
			}
		}
		m_mainWriter.StandardFarEastFont = text;
		text = m_document.StandardNonFarEastFont;
		if (string.IsNullOrEmpty(text) && defCharFormat != null && defCharFormat.HasValue(70))
		{
			text = defCharFormat.FontNameNonFarEast;
			if (defCharFormat.IsThemeFont(text))
			{
				text = defCharFormat.FontName;
			}
		}
		m_mainWriter.StandardNonFarEastFont = text;
		text = m_document.StandardBidiFont;
		if (string.IsNullOrEmpty(text) && defCharFormat != null && defCharFormat.HasValue(61))
		{
			text = defCharFormat.FontNameBidi;
			if (defCharFormat.IsThemeFont(text))
			{
				text = defCharFormat.FontName;
			}
		}
		m_mainWriter.StandardBidiFont = text;
	}

	private void WriteBackground()
	{
		Background background = m_document.Background;
		if (background.Type != 0)
		{
			CheckEscher();
			EscherClass escher = m_document.Escher;
			MsofbtSpContainer msofbtSpContainer = new MsofbtSpContainer(m_document);
			msofbtSpContainer.CreateRectangleContainer();
			MsofbtSpContainer backgroundContainerValue = escher.GetBackgroundContainerValue();
			int shapeId = backgroundContainerValue.Shape.ShapeId;
			msofbtSpContainer.UpdateBackground(m_document, background);
			MsofbtDgContainer msofbtDgContainer = escher.FindDgContainerForSubDocType(ShapeDocType.Main);
			msofbtDgContainer.Children.Remove(backgroundContainerValue);
			escher.Containers.Remove(shapeId);
			msofbtDgContainer.Children.Add(msofbtSpContainer);
			escher.Containers.Add(shapeId, msofbtSpContainer);
		}
	}

	private void WritePictureBackground(MsofbtSpContainer backContainer, MsofbtSpContainer oldBackContainer, Background background, EscherClass escher)
	{
		MsofbtBSE msofbtBSE = new MsofbtBSE(m_document);
		msofbtBSE.Initialize(background.ImageRecord);
		backContainer.Bse = msofbtBSE;
		uint num = backContainer.GetPropertyValue(390);
		if (num != uint.MaxValue)
		{
			escher.ModifyBStoreByPid((int)num, msofbtBSE);
		}
		else
		{
			escher.m_msofbtDggContainer.BstoreContainer.Children.Add(msofbtBSE);
			num = (uint)escher.m_msofbtDggContainer.BstoreContainer.Children.Count;
		}
		backContainer.UpdateFillPicture(background, (int)num);
	}

	private void CheckEscher()
	{
		EscherClass escher = m_document.Escher;
		if ((escher != null && escher.m_dgContainers.Count == 0) || escher == null)
		{
			escher = new EscherClass(m_document);
			escher.CreateDgForSubDocuments();
			m_document.Escher = escher;
		}
		else if (escher.m_msofbtDggContainer.BstoreContainer == null)
		{
			escher.m_msofbtDggContainer.Children.Add(new MsofbtBstoreContainer(m_document));
		}
	}

	private void WriteCommItems(WComment comment)
	{
		int num = 0;
		if (comment.CommentedBodyPart != null)
		{
			WriteParagraphProperties(comment.OwnerParagraph);
			CurrentWriter.WriteMarker(WordChunkType.ParagraphEnd);
			num = (CurrentWriter as WordWriter).GetTextPos();
			WriteParagraphs(comment.CommentedBodyPart.BodyItems, isTableBody: false);
		}
		else if (comment.CommentedItems.Count > 0)
		{
			num = (CurrentWriter as WordWriter).GetTextPos();
			foreach (ParagraphItem commentedItem in comment.CommentedItems)
			{
				WriteParaItem(commentedItem, comment.OwnerParagraph);
			}
		}
		int textPos = (CurrentWriter as WordWriter).GetTextPos();
		comment.Format.BookmarkStartOffset = textPos - num;
		comment.Format.BookmarkEndOffset = 0;
	}

	private void CountCommOffset(WComment comment)
	{
		if (!CommentOffsets.ContainsKey(comment.Format.TagBkmk))
		{
			return;
		}
		DictionaryEntry dictionaryEntry = CommentOffsets[comment.Format.TagBkmk];
		int num = (int)dictionaryEntry.Key;
		int num2 = (int)dictionaryEntry.Value;
		if (num2 != 0)
		{
			comment.Format.BookmarkStartOffset = num2 - num;
			if (comment.Format.BookmarkStartOffset == 0)
			{
				comment.Format.BookmarkEndOffset = 1;
			}
			else
			{
				comment.Format.BookmarkEndOffset = 0;
			}
		}
	}

	internal void Close()
	{
		m_document = null;
		if (m_txbxItems != null)
		{
			m_txbxItems = null;
		}
		if (m_hfTxbxItems != null)
		{
			m_hfTxbxItems = null;
		}
		if (m_commentCollection != null)
		{
			m_commentCollection = null;
		}
		if (m_footnoteCollection != null)
		{
			m_footnoteCollection = null;
		}
		if (m_endnoteCollection != null)
		{
			m_endnoteCollection = null;
		}
		if (m_charStylesHash != null)
		{
			m_charStylesHash.Clear();
			m_charStylesHash = null;
		}
		if (m_listData != null)
		{
			m_listData.Clear();
			m_listData = null;
		}
		if (m_bookmarksAfterCell != null)
		{
			m_bookmarksAfterCell.Clear();
			m_bookmarksAfterCell = null;
		}
		if (m_fieldStack != null)
		{
			m_fieldStack.Clear();
			m_fieldStack = null;
		}
		if (m_commOffsets != null)
		{
			m_commOffsets.Clear();
			m_commOffsets = null;
		}
		if (m_listPicture != null)
		{
			m_listPicture.Clear();
			m_listPicture = null;
		}
		if (m_oleObjects != null)
		{
			m_oleObjects.Clear();
			m_oleObjects = null;
		}
		if (m_OLEObjects != null)
		{
			m_OLEObjects.Clear();
			m_OLEObjects = null;
		}
		CharacterPropertiesConverter.Close();
	}

	private void UpdateCharStyleIndex(string charStyleName, bool isParaBreak)
	{
		if (charStyleName == null || !m_charStylesHash.ContainsKey(charStyleName))
		{
			return;
		}
		ushort num = (ushort)m_charStylesHash[charStyleName];
		if (num != 0)
		{
			if (isParaBreak)
			{
				CurrentWriter.BreakCHPX.PropertyModifiers.SetUShortValue(18992, num);
			}
			else
			{
				CurrentWriter.CHPX.PropertyModifiers.SetUShortValue(18992, num);
			}
		}
	}

	private void WriteBreakAfter(WParagraph curPara, BreakType type)
	{
		if (curPara.NextSibling is WParagraph paragraph)
		{
			WriteParagraphProperties(paragraph);
		}
		switch (type)
		{
		case BreakType.PageBreak:
			m_mainWriter.WriteMarker(WordChunkType.PageBreak);
			break;
		case BreakType.ColumnBreak:
			m_mainWriter.WriteMarker(WordChunkType.ColumnBreak);
			break;
		}
	}

	private bool IsPageBreakNeedToBeSkipped(Entity entity)
	{
		Entity entity2 = entity;
		do
		{
			if (entity2.Owner == null)
			{
				return false;
			}
			entity2 = entity2.Owner;
		}
		while (!(entity2 is WTextBox) && !(entity2 is WFootnote) && !(entity2 is HeaderFooter));
		return true;
	}

	private void WriteListProperties(IWParagraph paragraph, List<SinglePropertyModifierRecord> oldSprms, List<SinglePropertyModifierRecord> newSprms)
	{
		WriteListProperties(paragraph.ListFormat, CurrentWriter as WordWriterBase);
		Dictionary<int, object> dictionary = new Dictionary<int, object>(paragraph.ListFormat.PropertiesHash);
		if (CurrentWriter.PAPX.PropertyModifiers.Count > 0)
		{
			foreach (SinglePropertyModifierRecord propertyModifier in CurrentWriter.PAPX.PropertyModifiers)
			{
				newSprms.Add(propertyModifier.Clone());
			}
		}
		paragraph.ListFormat.PropertiesHash.Clear();
		foreach (KeyValuePair<int, object> item in paragraph.ListFormat.OldPropertiesHash)
		{
			paragraph.ListFormat.PropertiesHash.Add(item.Key, item.Value);
		}
		CurrentWriter.PAPX.PropertyModifiers.Clear();
		WriteListProperties(paragraph.ListFormat, CurrentWriter as WordWriterBase);
		if (CurrentWriter.PAPX.PropertyModifiers.Count > 0)
		{
			foreach (SinglePropertyModifierRecord propertyModifier2 in CurrentWriter.PAPX.PropertyModifiers)
			{
				oldSprms.Add(propertyModifier2.Clone());
			}
		}
		paragraph.ListFormat.PropertiesHash.Clear();
		foreach (KeyValuePair<int, object> item2 in dictionary)
		{
			paragraph.ListFormat.PropertiesHash.Add(item2.Key, item2.Value);
		}
	}

	private void WriteListProperties(WListFormat listFormat, IWordWriterBase writer)
	{
		if (listFormat.ListType == ListType.NoList && listFormat.ListLevelNumber < 1)
		{
			ProcessEmptyList(listFormat, writer);
		}
		else if (listFormat.CustomStyleName != string.Empty)
		{
			ProcessList(listFormat, writer);
		}
		else if (listFormat.ListLevelNumber > 0)
		{
			writer.PAPX.PropertyModifiers.SetByteValue(9738, (byte)listFormat.ListLevelNumber);
		}
		else if (listFormat.ListLevelNumber > 0)
		{
			writer.PAPX.PropertyModifiers.SetByteValue(9738, (byte)listFormat.ListLevelNumber);
		}
	}

	private void ProcessEmptyList(WListFormat listFormat, IWordWriterBase writer)
	{
		if (listFormat.IsListRemoved)
		{
			RemoveListSprms(writer.PAPX.PropertyModifiers);
		}
		else if (listFormat.IsEmptyList)
		{
			WriteEmptyList(writer.PAPX.PropertyModifiers);
		}
	}

	private void ProcessList(WListFormat listFormat, IWordWriterBase writer)
	{
		if (listFormat.CurrentListStyle.IsBuiltInStyle)
		{
			RemoveListSprms(writer.PAPX.PropertyModifiers);
		}
		else if (m_prevStyleName != listFormat.CustomStyleName)
		{
			string name = listFormat.CurrentListStyle.Name;
			if (AdapterListIDHolder.Instance.ContainsListName(name))
			{
				ContinueCurrentList(writer, listFormat);
			}
			else
			{
				ApplyStyle(writer, listFormat, useBaseStyle: false);
			}
		}
		else
		{
			ContinueCurrentList(writer, listFormat);
		}
	}

	internal void RemoveListSprms(SinglePropertyModifierArray Sprms)
	{
		int num = Sprms.Modifiers.Count;
		for (int i = 0; i < num; i++)
		{
			SinglePropertyModifierRecord singlePropertyModifierRecord = Sprms.Modifiers[i];
			if (singlePropertyModifierRecord.TypedOptions == 17931 || singlePropertyModifierRecord.TypedOptions == 9738)
			{
				Sprms.Modifiers.Remove(singlePropertyModifierRecord);
				num--;
			}
		}
	}

	internal void WriteEmptyList(SinglePropertyModifierArray Sprms)
	{
		SinglePropertyModifierRecord singlePropertyModifierRecord = Sprms[17931];
		if (singlePropertyModifierRecord == null)
		{
			singlePropertyModifierRecord = new SinglePropertyModifierRecord(17931);
			Sprms.Modifiers.Add(singlePropertyModifierRecord);
		}
		singlePropertyModifierRecord.ShortValue = 0;
		singlePropertyModifierRecord = Sprms[9738];
		if (singlePropertyModifierRecord == null)
		{
			singlePropertyModifierRecord = new SinglePropertyModifierRecord(9738);
			Sprms.Modifiers.Add(singlePropertyModifierRecord);
		}
		singlePropertyModifierRecord.ShortValue = 0;
	}

	private void ContinueCurrentList(IWordWriterBase writer, WListFormat listFormat)
	{
		ListData listData = m_listData[listFormat.CustomStyleName];
		writer.ListProperties.ContinueCurrentList(listData, listFormat, writer.StyleSheet);
		m_prevStyleName = listFormat.CustomStyleName;
	}

	private void ApplyStyle(IWordWriterBase writer, WListFormat listFormat, bool useBaseStyle)
	{
		ListStyle listStyle = m_document.ListStyles.FindByName(listFormat.CustomStyleName);
		if (listStyle != null)
		{
			ListData listData = CreateListData(listStyle, writer.StyleSheet, listFormat);
			if (!m_listData.ContainsKey(listFormat.CustomStyleName))
			{
				m_listData.Add(listFormat.CustomStyleName, listData);
			}
			if (useBaseStyle)
			{
				int listFormatIndex = writer.ListProperties.StyleListIndexes[listFormat.CustomStyleName];
				ModifyBaseStyles(listFormatIndex, writer);
			}
			writer.ListProperties.ApplyList(listData, listFormat, writer.StyleSheet, applyToPap: true);
			m_prevStyleName = listFormat.CustomStyleName;
		}
	}

	private void ModifyBaseStyles(int listFormatIndex, IWordWriterBase writer)
	{
		int currentStyleIndex = writer.CurrentStyleIndex;
		WordStyle styleByIndex = writer.StyleSheet.GetStyleByIndex(currentStyleIndex);
		short num = -1;
		num = styleByIndex.PAPX.PropertyModifiers.GetShort(17931, -1);
		if (num != listFormatIndex && num != -1)
		{
			styleByIndex.PAPX.PropertyModifiers.SetShortValue(17931, (short)listFormatIndex);
		}
	}

	private ListData CreateListData(ListStyle listStyle, WordStyleSheet styleSheet, WListFormat lstFormat)
	{
		ListData listData = new ListData(m_listID, listStyle.IsHybrid, listStyle.IsSimple);
		AdapterListIDHolder.Instance.ListStyleIDtoName.Add(m_listID, listStyle.Name);
		ListPropertiesConverter.Import(listStyle, listData, styleSheet);
		m_listID++;
		return listData;
	}

	private void UpdateListInStyle(IWordWriterBase writer, WParagraphStyle style, SinglePropertyModifierArray sprms)
	{
		if (!style.ListFormat.IsEmptyList && !style.ListFormat.IsListRemoved && style.ListFormat.ListLevelNumber < 1 && (style.ListFormat.ListType == ListType.NoList || style.ListFormat.CurrentListStyle == null))
		{
			return;
		}
		if (style.ListFormat.IsEmptyList)
		{
			WriteEmptyList(sprms);
			return;
		}
		if (style.ListFormat.IsListRemoved)
		{
			RemoveListSprms(sprms);
			return;
		}
		if (style.ListFormat.CurrentListStyle == null && style.ListFormat.ListLevelNumber > 0)
		{
			sprms.SetByteValue(9738, (byte)style.ListFormat.ListLevelNumber);
			return;
		}
		if (style.ListFormat.CurrentListStyle == null && style.ListFormat.ListLevelNumber > 0)
		{
			sprms.SetByteValue(9738, (byte)style.ListFormat.ListLevelNumber);
			return;
		}
		string name = style.ListFormat.CurrentListStyle.Name;
		if (AdapterListIDHolder.Instance.ContainsListName(name))
		{
			short shortValue = writer.ListProperties.StyleListIndexes[name];
			if (style.ListFormat.ListLevelNumber != -1)
			{
				sprms.SetByteValue(9738, (byte)style.ListFormat.ListLevelNumber);
			}
			if (sprms[17931] == null)
			{
				sprms.Add(new SinglePropertyModifierRecord(17931));
			}
			sprms[17931].ShortValue = shortValue;
			return;
		}
		ListStyle currentListStyle = style.ListFormat.CurrentListStyle;
		WListFormat listFormat = style.ListFormat;
		ListData listData = CreateListData(currentListStyle, writer.StyleSheet, listFormat);
		m_listData.Add(listFormat.CustomStyleName, listData);
		int num = writer.ListProperties.ApplyList(listData, listFormat, writer.StyleSheet, applyToPap: false);
		if (listFormat.ListLevelNumber != -1)
		{
			sprms.SetByteValue(9738, (byte)listFormat.ListLevelNumber);
		}
		if (sprms[17931] == null)
		{
			sprms.Add(new SinglePropertyModifierRecord(17931));
		}
		sprms[17931].ShortValue = (short)num;
	}

	private void ResetLists()
	{
		m_prevStyleName = null;
		AdapterListIDHolder.Instance.ListStyleIDtoName.Clear();
		if (m_listData != null && m_listData.Count > 0)
		{
			m_listData.Clear();
		}
	}
}
