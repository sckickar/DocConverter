using System;
using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;
using DocGen.DocIO.ReaderWriter.Escher;
using DocGen.Drawing;

namespace DocGen.DocIO.DLS;

internal class DocReaderAdapterBase
{
	internal struct PrepareTableInfo
	{
		internal bool InTable;

		internal int Level;

		internal int PrevLevel;

		internal PrepareTableState State;

		internal PrepareTableInfo(WordReaderBase reader, int prevLevel)
		{
			InTable = reader.HasTableBody;
			PrevLevel = prevLevel;
			Level = (InTable ? reader.PAPXSprms.GetInt(26185, 1) : 0);
			if (Level > PrevLevel)
			{
				State = PrepareTableState.EnterTable;
			}
			else if (Level < PrevLevel)
			{
				State = PrepareTableState.LeaveTable;
			}
			else
			{
				State = PrepareTableState.NoChange;
			}
		}
	}

	internal struct ComplexTable
	{
		internal WTable Table;

		internal WTable OneRowTable;

		internal ComplexTable(WTable oneRowTable)
		{
			Table = null;
			OneRowTable = oneRowTable;
		}

		internal void SetNull()
		{
			Table = null;
			OneRowTable = null;
		}

		internal void AppendOneRowToTable()
		{
			Table = OneRowTable;
			OneRowTable = null;
		}

		internal void CreateOneRowTable(WordDocument docEx)
		{
			OneRowTable = new WTable(docEx);
			OneRowTable.ResetCells(1, 1);
			OneRowTable.TableFormat.Borders.BorderType = BorderStyle.None;
		}
	}

	private const int DEF_WMFPLACEABLEHEADER_KEY = -1698247209;

	protected List<WPicture> m_listPic;

	protected WTextBody m_textBody;

	protected ITextBodyItem m_currParagraph;

	private bool m_cellFinished;

	private bool m_rowFinished;

	private WTable m_currTable;

	private Stack<WTable> m_tablesNested = new Stack<WTable>();

	protected Stack<WTextBody> m_nestedTextBodies = new Stack<WTextBody>();

	private WField m_currField;

	private Stack<WField> m_fieldStack = new Stack<WField>();

	private bool m_isPostFixBkmkStart;

	protected bool m_finalize;

	private WordChunkType m_prevChunkType;

	private BookmarkInfo m_bookmarkInfo;

	private List<WTable> nestedTable = new List<WTable>();

	protected WordDocument DocumentEx;

	protected WParagraph CurrentParagraph
	{
		get
		{
			if (m_currParagraph == null)
			{
				m_currParagraph = m_textBody.AddParagraph();
			}
			return m_currParagraph as WParagraph;
		}
	}

	protected WField CurrentField
	{
		get
		{
			if (m_fieldStack.Count > 0)
			{
				m_currField = m_fieldStack.Peek();
			}
			else
			{
				m_currField = null;
			}
			return m_currField;
		}
	}

	internal List<WPicture> ListPictures
	{
		get
		{
			if (m_listPic == null)
			{
				m_listPic = new List<WPicture>();
			}
			return m_listPic;
		}
	}

	internal void Init(WordDocument doc)
	{
		DocumentEx = doc;
		m_textBody = null;
		m_finalize = true;
	}

	protected void ReadTextBody(WordReaderBase reader, WTextBody textBody)
	{
		m_textBody = textBody;
		HeaderFooter headerFooter = ((textBody is HeaderFooter) ? (textBody as HeaderFooter) : null);
		m_currParagraph = null;
		while (!EndOfTextBody(reader, reader.ReadChunk()))
		{
			Preparation(reader);
			ProcessChunk(reader, headerFooter);
		}
		if (m_finalize)
		{
			Finalize(reader);
		}
	}

	protected virtual bool EndOfTextBody(WordReaderBase reader, WordChunkType chunkType)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	protected virtual void Preparation(WordReaderBase reader)
	{
		int num = ((m_nestedTextBodies.Count != 0) ? (m_tablesNested.Count + 1) : 0);
		if (m_prevChunkType != WordChunkType.TableRow && num > reader.PAPXSprms.GetInt(26185, 1))
		{
			if (num > 0)
			{
				reader.PAPXSprms.SetIntValue(26185, num);
			}
			else
			{
				reader.PAPXSprms.RemoveValue(26185);
			}
		}
		PrepareTableInfo prepti = new PrepareTableInfo(reader, num);
		PrepareParagraph(reader, ref prepti);
		PrepareTable(reader, ref prepti);
		SetPostfixBkmks();
	}

	private void ProcessChunk(WordReaderBase reader, HeaderFooter headerFooter)
	{
		ProcessBookmarkAfterParaEnd(reader);
		switch (reader.ChunkType)
		{
		case WordChunkType.Text:
			if (reader is IWordReader)
			{
				IWordReader wordReader2 = reader as IWordReader;
				if (wordReader2.IsEndnote || wordReader2.IsFootnote)
				{
					int customFnSplittedTextLength = (reader as WordReader).CustomFnSplittedTextLength;
					if (customFnSplittedTextLength > -1)
					{
						ReadCustomFootnote(reader, customFnSplittedTextLength, 0);
					}
					else
					{
						ReadFootnote(reader);
					}
					break;
				}
			}
			ReadText(reader);
			break;
		case WordChunkType.ParagraphEnd:
			ReadParagraphEnd(reader);
			break;
		case WordChunkType.PageBreak:
			ReadBreak(reader, BreakType.PageBreak);
			break;
		case WordChunkType.ColumnBreak:
			ReadBreak(reader, BreakType.ColumnBreak);
			break;
		case WordChunkType.LineBreak:
			ReadBreak(reader, BreakType.LineBreak);
			break;
		case WordChunkType.DocumentEnd:
			ReadDocumentEnd(reader);
			break;
		case WordChunkType.Image:
		{
			WField wField = ((m_fieldStack.Count > 0) ? m_fieldStack.Peek() : null);
			if (wField != null && FieldTypeDefiner.IsFormField(wField.FieldType) && reader.CHPXSprms.GetBoolean(2054, defValue: false))
			{
				FormField formField = reader.GetFormField(wField.FieldType);
				FormFieldPropertiesConverter.ReadFormFieldProperties(wField as WFormField, formField);
				(wField as WFormField).HasFFData = true;
			}
			else
			{
				ReadImage(reader);
			}
			break;
		}
		case WordChunkType.Shape:
			ReadShape(reader, headerFooter);
			break;
		case WordChunkType.Table:
			ReadTable(reader);
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_2, 11);
			break;
		case WordChunkType.TableRow:
			ReadTableRow(reader);
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_2, 16);
			break;
		case WordChunkType.TableCell:
			ReadTableCell(reader);
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_2, 10);
			break;
		case WordChunkType.Footnote:
			if (reader is WordFootnoteReader || reader is WordEndnoteReader)
			{
				ReadFootnoteMarker(reader);
			}
			else if (reader is IWordReader)
			{
				ReadFootnote(reader);
			}
			break;
		case WordChunkType.FieldBeginMark:
			if (reader.CurrentBookmark != null)
			{
				AppendBookmark(reader.CurrentBookmark, reader.IsBookmarkStart);
			}
			ReadFldBeginMark(reader);
			break;
		case WordChunkType.FieldSeparator:
			InsertFldSeparator(reader);
			break;
		case WordChunkType.FieldEndMark:
			InsertFldEndMark(reader);
			break;
		case WordChunkType.Tab:
			ReadTab(reader);
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_2, 9);
			break;
		case WordChunkType.Annotation:
			ReadAnnotation(reader);
			DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 4);
			break;
		case WordChunkType.Symbol:
			if (reader is IWordReader)
			{
				IWordReader wordReader = reader as IWordReader;
				if (wordReader.IsEndnote || wordReader.IsFootnote)
				{
					ReadFootnote(reader);
					break;
				}
			}
			ReadSymbol(reader);
			break;
		case WordChunkType.CurrentPageNumber:
			ReadCurrentPageNumber(reader);
			break;
		default:
			throw new InvalidOperationException("Unsupported WordChunkType occured");
		}
		ProcessCommText(reader, m_currParagraph as WParagraph);
		ProcessBookmarks(reader);
		m_prevChunkType = reader.ChunkType;
	}

	private void ReadCustomFootnote(WordReaderBase reader, int splittedTextLength, int startPos)
	{
		Addtext(reader, reader.TextChunk.Substring(startPos, splittedTextLength), isFromFootNoteSplittedText: true);
		ReadFootnote(reader);
		int num = 0;
		if (CurrentParagraph != null && CurrentParagraph.ChildEntities.LastItem is WFootnote)
		{
			reader.m_docInfo.TablesData.Footnotes.m_footEndnoteRefIndex++;
			num = (CurrentParagraph.ChildEntities.LastItem as WFootnote).m_strCustomMarker.Length;
		}
		startPos += splittedTextLength + num;
		int num2 = reader.TextChunk.Length - startPos;
		if (reader.TextChunk.Length > startPos && startPos + num2 <= reader.TextChunk.Length)
		{
			string text = reader.TextChunk.Substring(startPos, num2);
			IWordReader wordReader = reader as IWordReader;
			if (wordReader.IsEndnote || wordReader.IsFootnote)
			{
				splittedTextLength = (reader as WordReader).CustomFnSplittedTextLength;
				(reader as WordReader).CustomFnSplittedTextLength -= startPos;
				splittedTextLength = (reader as WordReader).CustomFnSplittedTextLength;
				if (splittedTextLength > -1)
				{
					ReadCustomFootnote(reader, splittedTextLength, startPos);
				}
				else
				{
					ReadFootnote(reader);
				}
			}
			else
			{
				Addtext(reader, text, isFromFootNoteSplittedText: true);
			}
		}
		(reader as WordReader).CustomFnSplittedTextLength = -1;
	}

	private void ProcessBookmarkAfterParaEnd(WordReaderBase reader)
	{
		if (reader.BookmarkAfterParaEnd != null)
		{
			AppendBookmark(reader.BookmarkAfterParaEnd, reader.IsBKMKStartAfterParaEnd);
			reader.BookmarkAfterParaEnd = null;
		}
	}

	private void ReadFootnoteMarker(WordReaderBase reader)
	{
		IWTextRange iWTextRange = CurrentParagraph.AppendText(reader.TextChunk);
		ReadCharacterFormat(reader, iWTextRange.CharacterFormat);
	}

	private void PrepareParagraph(WordReaderBase reader, ref PrepareTableInfo prepti)
	{
	}

	private void PrepareTable(IWordReaderBase reader, ref PrepareTableInfo prepti)
	{
		if (prepti.InTable)
		{
			if (m_cellFinished && reader.ChunkType != WordChunkType.TableRow)
			{
				AppendTableCell(ref prepti);
			}
			else if (m_rowFinished && prepti.State != PrepareTableState.LeaveTable)
			{
				AppendTableRow();
			}
		}
		switch (prepti.State)
		{
		case PrepareTableState.EnterTable:
			if (prepti.PrevLevel == 0)
			{
				m_nestedTextBodies.Push(m_textBody);
			}
			EnsureUpperTable(prepti.Level, reader);
			break;
		case PrepareTableState.LeaveTable:
			EnsureLowerTable(prepti.Level, reader);
			break;
		}
		if (m_cellFinished)
		{
			m_cellFinished = false;
		}
		if (m_rowFinished)
		{
			m_currParagraph = null;
			m_rowFinished = false;
		}
	}

	private void EnsureLowerTable(int level, IWordReaderBase reader)
	{
		while (m_tablesNested.Count > level)
		{
			m_tablesNested.Pop();
		}
		if (m_currTable == null)
		{
			return;
		}
		if (level == 0)
		{
			m_textBody = m_nestedTextBodies.Pop();
			if (m_currTable.Owner == null)
			{
				m_textBody.Items.Add(m_currTable);
			}
			foreach (WTable item in nestedTable)
			{
				if (item.PreferredTableWidth.WidthType == FtsWidth.Auto)
				{
					item.UpdateGridSpan();
				}
				else
				{
					item.UpdateGridSpan(item);
				}
			}
			if (m_currTable.PreferredTableWidth.WidthType == FtsWidth.Auto)
			{
				m_currTable.UpdateGridSpan();
			}
			else
			{
				m_currTable.UpdateGridSpan(m_currTable);
			}
			if (nestedTable.Count > 0)
			{
				nestedTable.Clear();
			}
			UpdateTableGridAfterValue(m_currTable, reader);
			m_currParagraph = null;
			m_currTable = null;
		}
		else
		{
			WTable currTable = m_currTable;
			m_currTable = m_tablesNested.Pop();
			m_textBody = m_currTable.LastCell;
			if (m_currTable.Owner == null)
			{
				m_textBody.Items.Add(currTable);
			}
			UpdateTableGridAfterValue(currTable, reader);
			nestedTable.Add(currTable);
			m_currParagraph = null;
		}
	}

	private void EnsureUpperTable(int level, IWordReaderBase reader)
	{
		do
		{
			if (m_currTable != null)
			{
				m_tablesNested.Push(m_currTable);
			}
			m_currTable = new WTable(DocumentEx);
			reader.TableRowWidthStack.Push(new Dictionary<WTableRow, short>());
			reader.MaximumTableRowWidth.Add(0);
			AppendTableRow();
		}
		while (m_tablesNested.Count < level - 1);
	}

	private void UpdateTableGridAfterValue(WTable table, IWordReaderBase reader)
	{
		if (reader.TableRowWidthStack.Count <= 0 || reader.MaximumTableRowWidth.Count <= 0)
		{
			return;
		}
		Dictionary<WTableRow, short> dictionary = reader.TableRowWidthStack.Pop();
		if (dictionary.Count != table.Rows.Count)
		{
			return;
		}
		short num = reader.MaximumTableRowWidth[reader.MaximumTableRowWidth.Count - 1];
		foreach (WTableRow key in dictionary.Keys)
		{
			if (num > dictionary[key])
			{
				key.RowFormat.AfterWidth = (float)(num - dictionary[key]) / 20f;
			}
		}
		reader.MaximumTableRowWidth.RemoveAt(reader.MaximumTableRowWidth.Count - 1);
	}

	private void AppendTableRow()
	{
		m_textBody = m_currTable.AddRow(isCopyFormat: false, autoPopulateCells: false).AddCell(isCopyFormat: false);
	}

	private void AppendTableCell(ref PrepareTableInfo prepti)
	{
		m_textBody = m_currTable.LastRow.AddCell(isCopyFormat: false);
	}

	protected virtual void Finalize(WordReaderBase reader)
	{
		int prevLevel = ((m_nestedTextBodies.Count != 0) ? (m_tablesNested.Count + 1) : 0);
		if (!(reader is WordSubdocumentReader) || !(reader as WordSubdocumentReader).IsNextItemPos)
		{
			PrepareTableInfo prepti = new PrepareTableInfo(reader, prevLevel);
			PrepareTable(reader, ref prepti);
		}
		if (reader is WordTextBoxReader || reader.ChunkType == WordChunkType.SectionEnd)
		{
			reader.RestoreBookmark();
			m_bookmarkInfo = null;
		}
		else
		{
			SetPostfixBkmks();
		}
	}

	private void SetPostfixBkmks()
	{
		if (m_bookmarkInfo != null)
		{
			AppendBookmark(m_bookmarkInfo, m_isPostFixBkmkStart);
			m_bookmarkInfo = null;
		}
	}

	private void ProcessBookmarks(WordReaderBase reader)
	{
		if (reader.CurrentBookmark == null || !(reader.CurrentBookmark.Name != "_PictureBullets"))
		{
			return;
		}
		bool isCellGroupBookmark = reader.CurrentBookmark.IsCellGroupBookmark;
		if ((reader.ChunkType == WordChunkType.ParagraphEnd && IsParagraphBefore(reader)) || reader.ChunkType == WordChunkType.TableCell || reader.ChunkType == WordChunkType.TableRow)
		{
			if ((reader is WordTextBoxReader && reader.ChunkType == WordChunkType.ParagraphEnd) || (reader.CurrentBookmark.IsCellGroupBookmark && !reader.IsBookmarkStart && reader.CurrentBookmark.EndPos > reader.EndTextPos))
			{
				reader.RestoreBookmark();
			}
			else if (reader.CurrentBookmark.IsCellGroupBookmark && !reader.IsBookmarkStart && (reader.ChunkType == WordChunkType.TableCell || reader.ChunkType == WordChunkType.TableRow))
			{
				WTableRow lastRow = m_currTable.LastRow;
				WTableCell wTableCell = null;
				wTableCell = ((lastRow.Cells.Count <= reader.CurrentBookmark.EndCellIndex) ? lastRow.Cells[lastRow.Cells.Count - 1] : lastRow.Cells[reader.CurrentBookmark.EndCellIndex]);
				BookmarkEnd bookmarkEnd = ((wTableCell.LastParagraph == null) ? wTableCell.AddParagraph() : wTableCell.LastParagraph).AppendBookmarkEnd(reader.CurrentBookmark.Name);
				bookmarkEnd.IsCellGroupBkmk = isCellGroupBookmark;
				if (reader.ChunkType == WordChunkType.TableRow)
				{
					bookmarkEnd.IsAfterRowMark = true;
				}
			}
			else
			{
				m_bookmarkInfo = reader.CurrentBookmark;
				m_isPostFixBkmkStart = reader.IsBookmarkStart;
			}
		}
		else if (reader.ChunkType != WordChunkType.FieldBeginMark)
		{
			if (reader.ChunkType != WordChunkType.ParagraphEnd)
			{
				AppendBookmark(reader.CurrentBookmark, reader.IsBookmarkStart);
				reader.BookmarkAfterParaEnd = null;
			}
			else
			{
				reader.BookmarkAfterParaEnd = reader.CurrentBookmark;
				reader.IsBKMKStartAfterParaEnd = reader.IsBookmarkStart;
			}
		}
		reader.CurrentBookmark = null;
	}

	private bool IsParagraphBefore(WordReaderBase reader)
	{
		if (reader.IsBookmarkStart)
		{
			string textChunk = reader.TextChunk;
			int num = reader.CurrentTextPosition - textChunk.Length;
			if (textChunk.Substring(0, reader.CurrentBookmark.StartPos - num) == ControlChar.CarriegeReturn)
			{
				return true;
			}
		}
		return false;
	}

	private void AppendBookmark(BookmarkInfo bkmrInfo, bool isBookmarkStart)
	{
		if (isBookmarkStart)
		{
			BookmarkStart bookmarkStart = CurrentParagraph.AppendBookmarkStart(bkmrInfo.Name);
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 5);
			bookmarkStart.ColumnFirst = bkmrInfo.StartCellIndex;
			bookmarkStart.ColumnLast = bkmrInfo.EndCellIndex;
			bookmarkStart.IsCellGroupBkmk = bkmrInfo.IsCellGroupBookmark;
		}
		else
		{
			BookmarkEnd bookmarkEnd = CurrentParagraph.AppendBookmarkEnd(bkmrInfo.Name);
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 4);
			bookmarkEnd.IsCellGroupBkmk = bkmrInfo.IsCellGroupBookmark;
		}
	}

	private void ReadText(WordReaderBase reader)
	{
		if (reader.CHPXSprms.GetBoolean(2133, defValue: false) && reader.TextChunk.Length == 1)
		{
			switch (reader.TextChunk[0])
			{
			case '\u0006':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 18);
				break;
			case '\n':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 0);
				break;
			case '\v':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 29);
				break;
			case '\f':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 7);
				break;
			case '\u000e':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 1);
				break;
			case '\u000f':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 12);
				break;
			case '\u0010':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 14);
				break;
			case '\u0016':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 17);
				break;
			case '\u0017':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 16);
				break;
			case '\u0018':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 20);
				break;
			case '\u0019':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 21);
				break;
			case '\u001a':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 27);
				break;
			case '\u001b':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 3);
				break;
			case '\u001c':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 9);
				break;
			case '\u001d':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 11);
				break;
			case '\u001e':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 13);
				break;
			case '!':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 23);
				break;
			case '"':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 0);
				break;
			case '#':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 1);
				break;
			case '$':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 2);
				break;
			case '%':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 22);
				break;
			case '&':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 8);
				break;
			case ')':
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 26);
				break;
			}
		}
		Addtext(reader, reader.TextChunk, isFromFootNoteSplittedText: false);
	}

	internal void Addtext(WordReaderBase reader, string text, bool isFromFootNoteSplittedText)
	{
		string text2 = (isFromFootNoteSplittedText ? text : reader.TextChunk);
		if (text2 == null || text2.Length == 0)
		{
			return;
		}
		if (text2 == ControlChar.CarriegeReturn)
		{
			Break @break = new Break(DocumentEx, BreakType.LineBreak);
			@break.TextRange.Text = ControlChar.CarriegeReturn;
			AddItem(@break, CurrentParagraph);
			ReadCharacterFormat(reader, @break.CharacterFormat);
			CheckTrackChanges(@break, reader);
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 7);
			return;
		}
		if (text2.Contains('\u001f'.ToString()))
		{
			text = text2;
			int num = text.IndexOf('\u001f');
			while (true)
			{
				switch (num)
				{
				case 0:
					num++;
					break;
				case -1:
					return;
				}
				AddTextRange(reader, text.Substring(0, num));
				text = text.Substring(num);
				num = text.IndexOf('\u001f');
				if (num == -1 && text != string.Empty)
				{
					AddTextRange(reader, text);
					DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_2, 7);
				}
			}
		}
		AddTextRange(reader, text2);
	}

	private void AddTextRange(WordReaderBase reader, string text)
	{
		IWTextRange iWTextRange = new WTextRange(DocumentEx);
		iWTextRange.Text = text;
		AddItem(iWTextRange as ParagraphItem, CurrentParagraph);
		ReadCharacterFormat(reader, iWTextRange.CharacterFormat);
		CheckTrackChanges(iWTextRange as ParagraphItem, reader);
		if (CurrentField is WTextFormField && iWTextRange.CharacterFormat.PropertiesHash.ContainsKey(106))
		{
			iWTextRange.CharacterFormat.PropertiesHash.Remove(106);
		}
	}

	private void ReadParagraphEnd(WordReaderBase reader)
	{
		if (m_currParagraph != null || reader.PAPXSprms.Modifiers.Count != 1 || reader.PAPXSprms.Modifiers[0].OptionType != WordSprmOptionType.sprmPTableProps)
		{
			ReadListFormat(reader, CurrentParagraph.ListFormat);
			ReadCharacterFormat(reader, CurrentParagraph.BreakCharacterFormat);
			ReadParagraphFormat(reader, CurrentParagraph);
			CheckTrackChanges(CurrentParagraph, reader);
			m_currParagraph = null;
		}
	}

	private void ReadSymbol(WordReaderBase reader)
	{
		SymbolDescriptor symbolDescriptor = reader.GetSymbolDescriptor();
		WSymbol wSymbol = new WSymbol(DocumentEx);
		AddItem(wSymbol, CurrentParagraph);
		wSymbol.CharacterCode = symbolDescriptor.CharCode;
		wSymbol.CharCodeExt = symbolDescriptor.CharCodeExt;
		wSymbol.FontName = reader.StyleSheet.FontNamesList[symbolDescriptor.FontCode];
		ReadCharacterFormat(reader, wSymbol.CharacterFormat);
		CheckTrackChanges(wSymbol, reader);
		for (int i = 1; i < reader.TextChunk.Length; i++)
		{
			AddItem((WSymbol)wSymbol.Clone(), CurrentParagraph);
		}
	}

	private void ReadCurrentPageNumber(WordReaderBase reader)
	{
		if (reader.CHPXSprms.Contain(2133))
		{
			CurrentParagraph.AppendField("", FieldType.FieldPage);
		}
	}

	private void ReadTab(WordReaderBase reader)
	{
	}

	private void ReadTableCell(WordReaderBase reader)
	{
		m_cellFinished = true;
		WTableCell lastCell = m_currTable.LastCell;
		if (lastCell.Items.Count == 0)
		{
			m_currParagraph = lastCell.AddParagraph() as WParagraph;
		}
		if (m_currParagraph != null || m_prevChunkType == WordChunkType.ParagraphEnd || m_prevChunkType == WordChunkType.TableRow || m_prevChunkType == WordChunkType.TableCell)
		{
			ReadListFormat(reader, CurrentParagraph.ListFormat);
			ReadParagraphFormat(reader, CurrentParagraph);
		}
		ReadCharacterFormat(reader, lastCell.CharacterFormat);
		CurrentParagraph.BreakCharacterFormat.ImportContainer(lastCell.CharacterFormat);
		CheckTrackChanges(CurrentParagraph, reader);
		m_currParagraph = null;
	}

	private void ReadTableRow(WordReaderBase reader)
	{
		m_rowFinished = true;
		if (m_prevChunkType != WordChunkType.TableCell && m_currTable != null)
		{
			foreach (Entity childEntity in m_currTable.LastCell.ChildEntities)
			{
				if (!(childEntity is WParagraph))
				{
					continue;
				}
				for (int i = 0; i < (childEntity as WParagraph).ChildEntities.Count; i++)
				{
					Entity entity2 = (childEntity as WParagraph).ChildEntities[i];
					if (entity2 is BookmarkStart || entity2 is BookmarkEnd)
					{
						(m_currTable.LastCell.PreviousSibling as WTableCell).LastParagraph.ChildEntities.Add(entity2);
						i--;
					}
				}
			}
			m_currTable.LastRow.Cells.RemoveAt(m_currTable.LastRow.Cells.Count - 1);
		}
		if (m_currTable != null)
		{
			ReadTableRowFormat(reader, m_currTable);
			if (m_currTable.Rows.Count > 1 && IsSplitTableRows(reader, m_currTable.Rows[m_currTable.Rows.Count - 2].RowFormat.m_unParsedSprms, reader.PAPXSprms))
			{
				WTextBody textBody = m_textBody;
				if (m_textBody == m_currTable.LastCell)
				{
					textBody = ((reader.PAPXSprms.GetInt(26185, 1) <= 1) ? m_nestedTextBodies.Peek() : m_tablesNested.Peek().LastCell);
					WTable wTable = new WTable(m_currTable.Document);
					wTable.Rows.Add(m_currTable.LastRow);
					if (m_currTable.Owner == null)
					{
						textBody.Items.Add(m_currTable);
						if (reader.PAPXSprms[13837] == null)
						{
							WParagraph wParagraph = new WParagraph(m_currTable.Document);
							textBody.Items.Add(wParagraph);
							wParagraph.BreakCharacterFormat.Hidden = true;
						}
					}
					UpdateTableGridAfterValue(m_currTable, reader);
					m_currTable = wTable;
				}
			}
			if (m_currTable.LastRow.Cells.Count < 1)
			{
				m_currTable.Rows.Remove(m_currTable.LastRow);
			}
			else
			{
				m_currTable.LastRow.HasTblPrEx = true;
			}
		}
		else
		{
			WSymbol wSymbol = CurrentParagraph.AppendSymbol(7);
			ReadCharacterFormat(reader, wSymbol.CharacterFormat);
			CheckTrackChanges(wSymbol, reader);
			wSymbol.FontName = (reader.CHPXSprms.Contain(19038) ? reader.GetFontName(19038) : reader.GetFontName(19023));
			m_rowFinished = false;
		}
		SinglePropertyModifierArray singlePropertyModifierArray = null;
		if (m_currTable == null || m_currTable.LastRow == null)
		{
			return;
		}
		SinglePropertyModifierRecord singlePropertyModifierRecord = reader.PAPX.PropertyModifiers[25707];
		if (singlePropertyModifierRecord != null && reader.m_streamsManager.DataStream != null)
		{
			if (reader.m_streamsManager.DataStream.Length > singlePropertyModifierRecord.UIntValue)
			{
				reader.m_streamsManager.DataStream.Position = singlePropertyModifierRecord.UIntValue;
			}
			if (reader.m_streamsManager.DataStream.Position + 2 < reader.m_streamsManager.DataStream.Length)
			{
				int num = reader.m_streamsManager.DataReader.ReadUInt16();
				if (num <= 16290)
				{
					SinglePropertyModifierArray singlePropertyModifierArray2 = new SinglePropertyModifierArray();
					if (reader.m_streamsManager.DataStream.Position + num <= reader.m_streamsManager.DataStream.Length)
					{
						singlePropertyModifierArray2.Parse(reader.m_streamsManager.DataStream, num);
					}
					singlePropertyModifierArray = singlePropertyModifierArray2;
				}
			}
		}
		if (!reader.PAPX.PropertyModifiers.Contain(54836) && singlePropertyModifierArray != null && singlePropertyModifierArray[54836] != null)
		{
			Spacings spacings = new Spacings(singlePropertyModifierArray[54836]);
			Paddings paddings = new Paddings();
			if (!spacings.IsEmpty)
			{
				TablePropertiesConverter.ExportPaddings(spacings, paddings);
				m_currTable.LastRow.RowFormat.SetPropertyValue(3, paddings);
			}
			else
			{
				TablePropertiesConverter.ExportDefaultPaddings(paddings);
				m_currTable.LastRow.RowFormat.SetPropertyValue(3, paddings);
			}
		}
		singlePropertyModifierArray?.Clear();
	}

	private static void ExportDefaultPaddings(Paddings destination)
	{
		destination.Left = 0f;
		destination.Right = 0f;
		destination.Top = 0f;
		destination.Bottom = 0f;
	}

	private bool IsSplitTableRows(WordReaderBase reader, SinglePropertyModifierArray previousRowSprms, SinglePropertyModifierArray currentRowSprms)
	{
		bool flag = false;
		SinglePropertyModifierArray singlePropertyModifierArray = previousRowSprms?.Clone();
		int[] array = new int[3] { 29801, 22074, 22116 };
		if (singlePropertyModifierArray != null && currentRowSprms != null)
		{
			int[] array2 = array;
			foreach (int option in array2)
			{
				SinglePropertyModifierRecord newSprm = singlePropertyModifierArray.GetNewSprm(option, 13928);
				SinglePropertyModifierRecord newSprm2 = currentRowSprms.GetNewSprm(option, 13928);
				if (newSprm != null && newSprm2 != null)
				{
					if (!CompareArray(newSprm.ByteArray, newSprm2.ByteArray))
					{
						flag = true;
						break;
					}
				}
				else if ((newSprm != null && newSprm2 == null) || (newSprm == null && newSprm2 != null))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			flag = CompareBidiAndPositioning();
		}
		if (!flag)
		{
			SinglePropertyModifierRecord singlePropertyModifierRecord = null;
			SinglePropertyModifierRecord singlePropertyModifierRecord2 = null;
			SinglePropertyModifierRecord singlePropertyModifierRecord3 = previousRowSprms?[25707];
			if (singlePropertyModifierRecord3 != null && reader.m_streamsManager.DataStream != null)
			{
				if (reader.m_streamsManager.DataStream.Length > singlePropertyModifierRecord3.UIntValue)
				{
					reader.m_streamsManager.DataStream.Position = singlePropertyModifierRecord3.UIntValue;
				}
				if (reader.m_streamsManager.DataStream.Position + 2 < reader.m_streamsManager.DataStream.Length)
				{
					int num = reader.m_streamsManager.DataReader.ReadUInt16();
					if (num <= 16290)
					{
						SinglePropertyModifierArray singlePropertyModifierArray2 = new SinglePropertyModifierArray();
						if (reader.m_streamsManager.DataStream.Position + num < reader.m_streamsManager.DataStream.Length)
						{
							singlePropertyModifierArray2.Parse(reader.m_streamsManager.DataStream, num);
						}
						singlePropertyModifierRecord = singlePropertyModifierArray2[22074];
					}
				}
			}
			singlePropertyModifierRecord3 = currentRowSprms[25707];
			if (singlePropertyModifierRecord3 != null && reader.m_streamsManager.DataStream != null)
			{
				if (reader.m_streamsManager.DataStream.Length > singlePropertyModifierRecord3.UIntValue)
				{
					reader.m_streamsManager.DataStream.Position = singlePropertyModifierRecord3.UIntValue;
				}
				if (reader.m_streamsManager.DataStream.Position + 2 < reader.m_streamsManager.DataStream.Length)
				{
					int num2 = reader.m_streamsManager.DataReader.ReadUInt16();
					if (num2 <= 16290)
					{
						SinglePropertyModifierArray singlePropertyModifierArray3 = new SinglePropertyModifierArray();
						if (reader.m_streamsManager.DataStream.Position + num2 < reader.m_streamsManager.DataStream.Length)
						{
							singlePropertyModifierArray3.Parse(reader.m_streamsManager.DataStream, num2);
						}
						singlePropertyModifierRecord2 = singlePropertyModifierArray3[22074];
					}
				}
			}
			if (singlePropertyModifierRecord != null && singlePropertyModifierRecord2 != null)
			{
				if (!CompareArray(singlePropertyModifierRecord.ByteArray, singlePropertyModifierRecord2.ByteArray))
				{
					flag = true;
				}
				else if ((singlePropertyModifierRecord != null && singlePropertyModifierRecord2 == null) || (singlePropertyModifierRecord == null && singlePropertyModifierRecord2 != null))
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	private bool CompareBidiAndPositioning()
	{
		RowFormat rowFormat = m_currTable.Rows[m_currTable.Rows.Count - 2].RowFormat;
		RowFormat rowFormat2 = m_currTable.LastRow.RowFormat;
		if (rowFormat.Bidi != rowFormat2.Bidi)
		{
			return true;
		}
		RowFormat.TablePositioning positioning = rowFormat.Positioning;
		RowFormat.TablePositioning positioning2 = rowFormat2.Positioning;
		if (positioning.AllowOverlap == positioning2.AllowOverlap && positioning.DistanceFromBottom == positioning2.DistanceFromBottom && positioning.DistanceFromLeft == positioning2.DistanceFromLeft && positioning.DistanceFromRight == positioning2.DistanceFromRight && positioning.DistanceFromTop == positioning2.DistanceFromTop && positioning.HorizPosition == positioning2.HorizPosition && positioning.HorizPositionAbs == positioning2.HorizPositionAbs && positioning.HorizRelationTo == positioning2.HorizRelationTo && positioning.VertPosition == positioning2.VertPosition && positioning.VertPositionAbs == positioning2.VertPositionAbs)
		{
			return positioning.VertRelationTo != positioning2.VertRelationTo;
		}
		return true;
	}

	private bool CompareArray(byte[] buffer1, byte[] buffer2)
	{
		bool result = true;
		for (int i = 0; i < buffer1.Length; i++)
		{
			if (buffer1[i] != buffer2[i])
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void ReadTable(WordReaderBase reader)
	{
		ReadTableRow(reader);
	}

	protected virtual void ReadAnnotation(WordReaderBase reader)
	{
	}

	protected virtual void ReadFootnote(WordReaderBase reader)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	private void ReadBreak(WordReaderBase reader, BreakType breakType)
	{
		Break @break = null;
		if (breakType == BreakType.LineBreak)
		{
			@break = new Break(DocumentEx, BreakType.LineBreak);
			@break.TextRange.Text = reader.TextChunk;
		}
		else
		{
			@break = new Break(DocumentEx, breakType);
		}
		AddItem(@break, CurrentParagraph);
		ReadCharacterFormat(reader, @break.CharacterFormat);
		if (@break.CharacterFormat.BreakClear == BreakClearType.All)
		{
			@break.m_breakType = BreakType.TextWrappingBreak;
		}
		CheckTrackChanges(@break, reader);
	}

	protected virtual void ReadDocumentEnd(WordReaderBase reader)
	{
	}

	protected virtual void ReadShape(WordReaderBase reader, HeaderFooter headerFooter)
	{
		if (reader.ReadWatermark(DocumentEx, headerFooter))
		{
			if (headerFooter != null)
			{
				headerFooter.WriteWatermark = true;
			}
			return;
		}
		FileShapeAddress fSPA = reader.GetFSPA();
		if (fSPA == null)
		{
			return;
		}
		fSPA.IsHeaderShape = reader is WordHeaderFooterReader;
		MsofbtSpContainer msofbtSpContainer = null;
		if (DocumentEx.Escher.Containers.ContainsKey(fSPA.Spid))
		{
			msofbtSpContainer = DocumentEx.Escher.Containers[fSPA.Spid] as MsofbtSpContainer;
		}
		if (msofbtSpContainer != null && (msofbtSpContainer.Shape.ShapeType == EscherShapeType.msosptTextBox || (msofbtSpContainer.Shape.ShapeType == EscherShapeType.msosptRectangle && msofbtSpContainer.FindContainerByMsofbt(MSOFBT.msofbtClientTextbox) != null)))
		{
			ReadTextBox(reader, fSPA);
		}
		else if (msofbtSpContainer != null && msofbtSpContainer.Shape.ShapeType == EscherShapeType.msosptPictureFrame)
		{
			DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.ShapeBase drawingObject = reader.GetDrawingObject();
			if (drawingObject is PictureShape)
			{
				ReadPictureShape(reader, drawingObject, msofbtSpContainer);
			}
		}
		else
		{
			ReadAutoShape(reader);
		}
	}

	protected virtual bool ReadWatermark(WordReaderBase reader)
	{
		return false;
	}

	private bool IsShapeFieldResult(IEntity endItem)
	{
		if (CurrentField == null)
		{
			return false;
		}
		IEntity nextSibling = CurrentField.NextSibling;
		string text = string.Empty;
		while (nextSibling != null && nextSibling != endItem)
		{
			if (nextSibling is WTextRange)
			{
				text += (nextSibling as WTextRange).Text;
				if (FieldTypeDefiner.GetFieldType(text) == FieldType.FieldShape)
				{
					return true;
				}
			}
			nextSibling = nextSibling.NextSibling;
		}
		return false;
	}

	protected virtual void ReadTextBox(WordReaderBase reader, FileShapeAddress fspa)
	{
	}

	private void ReadPictureShape(WordReaderBase reader, DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.ShapeBase shape, MsofbtSpContainer shapeContainer)
	{
		PictureShape pictureShape = shape as PictureShape;
		if (pictureShape.ImageRecord == null)
		{
			return;
		}
		WPicture wPicture = new WPicture(DocumentEx);
		CurrentParagraph.LoadPicture(wPicture, pictureShape.ImageRecord);
		AddItem(wPicture, CurrentParagraph);
		BaseProps shapeProps = pictureShape.ShapeProps;
		float height = (float)shapeProps.Height / 20f;
		float width = (float)shapeProps.Width / 20f;
		float verticalPosition = (float)shapeProps.YaTop / 20f;
		float horizontalPosition = (float)shapeProps.XaLeft / 20f;
		SizeF size = new SizeF(width, height);
		wPicture.Size = size;
		wPicture.HeightScale = 100f;
		wPicture.WidthScale = 100f;
		wPicture.VerticalOrigin = pictureShape.ShapeProps.RelVrtPos;
		wPicture.HorizontalOrigin = pictureShape.ShapeProps.RelHrzPos;
		wPicture.VerticalPosition = verticalPosition;
		wPicture.HorizontalPosition = horizontalPosition;
		if (wPicture.PreviousSibling is WFieldMark && (wPicture.PreviousSibling as WFieldMark).Type == FieldMarkType.FieldSeparator && IsShapeFieldResult(wPicture.PreviousSibling))
		{
			wPicture.TextWrappingStyle = TextWrappingStyle.Inline;
			shapeContainer.Bse.IsInlineBlip = true;
			shapeContainer.Bse.IsPictureInShapeField = true;
		}
		else
		{
			wPicture.TextWrappingStyle = pictureShape.ShapeProps.TextWrappingStyle;
		}
		wPicture.TextWrappingType = pictureShape.ShapeProps.TextWrappingType;
		wPicture.IsBelowText = pictureShape.ShapeProps.IsBelowText;
		wPicture.HorizontalAlignment = pictureShape.ShapeProps.HorizontalAlignment;
		wPicture.VerticalAlignment = pictureShape.ShapeProps.VerticalAlignment;
		wPicture.ShapeId = pictureShape.ShapeProps.Spid;
		wPicture.IsHeaderPicture = pictureShape.ShapeProps.IsHeaderShape;
		wPicture.AlternativeText = pictureShape.PictureProps.AlternativeText;
		wPicture.Name = pictureShape.PictureProps.Name;
		if (shapeContainer.ShapePosition != null)
		{
			wPicture.LayoutInCell = shapeContainer.ShapePosition.AllowInTableCell;
			wPicture.Visible = shapeContainer.ShapeOptions.Visible;
		}
		if (shapeContainer.ShapeOptions.Properties.ContainsKey(263))
		{
			wPicture.ChromaKeyColor = WordColor.ConvertRGBToColor(shapeContainer.ShapeOptions.GetPropertyValue(263));
		}
		if (shapeContainer.ShapeOptions != null)
		{
			UpdateImageCroppingPostion(wPicture, shapeContainer);
			wPicture.DistanceFromBottom = (float)Math.Round((float)shapeContainer.ShapeOptions.DistanceFromBottom / 12700f, 2);
			wPicture.DistanceFromLeft = (float)Math.Round((float)shapeContainer.ShapeOptions.DistanceFromLeft / 12700f, 2);
			wPicture.DistanceFromRight = (float)Math.Round((float)shapeContainer.ShapeOptions.DistanceFromRight / 12700f, 2);
			wPicture.DistanceFromTop = (float)Math.Round((float)shapeContainer.ShapeOptions.DistanceFromTop / 12700f, 2);
			wPicture.Rotation = (int)shapeContainer.ShapeOptions.Roation / 65536;
		}
		if (!wPicture.IsBelowText && wPicture.TextWrappingStyle == TextWrappingStyle.Behind)
		{
			wPicture.TextWrappingStyle = TextWrappingStyle.InFrontOfText;
		}
		if ((wPicture.TextWrappingStyle == TextWrappingStyle.Through || wPicture.TextWrappingStyle == TextWrappingStyle.Tight) && shapeContainer.ShapeOptions != null && shapeContainer.ShapeOptions.Properties.Contains(899))
		{
			wPicture.WrapPolygon = new WrapPolygon();
			wPicture.WrapPolygon.Edited = false;
			for (int i = 0; i < shapeContainer.ShapeOptions.WrapPolygonVertices.Coords.Count; i++)
			{
				wPicture.WrapPolygon.Vertices.Add(shapeContainer.ShapeOptions.WrapPolygonVertices.Coords[i]);
			}
		}
		CheckTextEmbed(shape, wPicture);
		wPicture.PictureShape.ShapeContainer = shapeContainer;
	}

	internal void UpdateImageCroppingPostion(WPicture picture, MsofbtSpContainer ShapeContainer)
	{
		int propertyValue = (int)ShapeContainer.ShapeOptions.GetPropertyValue(258);
		if (propertyValue != -1)
		{
			picture.FillRectangle.LeftOffset = GetPictureCropValue(propertyValue);
		}
		propertyValue = (int)ShapeContainer.ShapeOptions.GetPropertyValue(259);
		if (propertyValue != -1)
		{
			picture.FillRectangle.RightOffset = GetPictureCropValue(propertyValue);
		}
		propertyValue = (int)ShapeContainer.ShapeOptions.GetPropertyValue(256);
		if (propertyValue != -1)
		{
			picture.FillRectangle.TopOffset = GetPictureCropValue(propertyValue);
		}
		propertyValue = (int)ShapeContainer.ShapeOptions.GetPropertyValue(257);
		if (propertyValue != -1)
		{
			picture.FillRectangle.BottomOffset = GetPictureCropValue(propertyValue);
		}
	}

	private float GetPictureCropValue(int propValue)
	{
		return (float)Math.Round((float)((double)(float)propValue * 1.5259) / 1000f, 3);
	}

	protected virtual void CheckTextEmbed(DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.ShapeBase shape, WPicture picture)
	{
	}

	private void ReadAutoShape(WordReaderBase reader)
	{
		ShapeObject shapeObject = new ShapeObject(DocumentEx);
		shapeObject.FSPA = reader.GetFSPA();
		ReadCharacterFormat(reader, shapeObject.CharacterFormat);
		if (reader is WordHeaderFooterReader)
		{
			shapeObject.IsHeaderAutoShape = true;
		}
		if (DocumentEx.Escher.Containers.ContainsKey(shapeObject.FSPA.Spid))
		{
			AddItem(shapeObject, CurrentParagraph);
			CheckTrackChanges(shapeObject, reader);
			BaseContainer baseContainer = DocumentEx.Escher.Containers[shapeObject.FSPA.Spid];
			if (baseContainer is MsofbtSpgrContainer)
			{
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 15);
			}
			else
			{
				DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 25);
			}
			ReadAutoShapeText(DocumentEx.Escher.Containers[shapeObject.FSPA.Spid], shapeObject);
			if (baseContainer is MsofbtSpContainer && (baseContainer as MsofbtSpContainer).ShapePosition != null)
			{
				shapeObject.AllowInCell = (baseContainer as MsofbtSpContainer).ShapePosition.AllowInTableCell;
			}
		}
	}

	private bool IsUnsupportedSpType(BaseContainer baseContainer)
	{
		bool result = false;
		if (baseContainer is MsofbtSpContainer && (baseContainer as MsofbtSpContainer).Shape.ShapeType == EscherShapeType.msosptHostControl)
		{
			result = true;
		}
		return result;
	}

	private void ReadAutoShapeText(BaseContainer shapeContainer, ShapeObject shapeObj)
	{
		BaseEscherRecord baseEscherRecord = null;
		int i = 0;
		for (int count = shapeContainer.Children.Count; i < count; i++)
		{
			baseEscherRecord = shapeContainer.Children[i] as BaseEscherRecord;
			if (baseEscherRecord is MsofbtSp)
			{
				MsofbtSp msofbtSp = baseEscherRecord as MsofbtSp;
				if (msofbtSp.ShapeType != EscherShapeType.msosptPictureFrame)
				{
					ReadAutoShapeTextBox(msofbtSp.ShapeId, shapeObj);
				}
			}
			else if (baseEscherRecord is BaseContainer)
			{
				ReadAutoShapeText(baseEscherRecord as BaseContainer, shapeObj);
			}
		}
	}

	protected virtual void ReadAutoShapeTextBox(int shapeId, ShapeObject shapeObj)
	{
	}

	internal bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}

	private void ReadImage(WordReaderBase reader)
	{
		if (CurrentField is WMergeField)
		{
			FormField formField = reader.GetFormField(CurrentField.FieldType);
			FormFieldPropertiesConverter.ReadFormFieldProperties(CurrentField as WFormField, formField);
		}
		else if (CurrentField is WFormField && reader.CHPXSprms.GetBoolean(2054, defValue: false))
		{
			FormField formField2 = reader.GetFormField(CurrentField.FieldType);
			FormFieldPropertiesConverter.ReadFormFieldProperties(CurrentField as WFormField, formField2);
			(CurrentField as WFormField).HasFFData = true;
		}
		else
		{
			if (CurrentField != null && (CurrentField.FieldType == FieldType.FieldPageRef || CurrentField.FieldType == FieldType.FieldRef || CurrentField.FieldType == FieldType.FieldHyperlink || CurrentField.FieldType == FieldType.FieldNoteRef) && reader.CHPXSprms.GetBoolean(2054, defValue: false))
			{
				return;
			}
			WordImageReader wordImageReader = (WordImageReader)reader.GetImageReader(DocumentEx);
			ShapeObject shapeObject = new InlineShapeObject(DocumentEx);
			(shapeObject as InlineShapeObject).ShapeContainer = wordImageReader.InlineShapeContainer;
			(shapeObject as InlineShapeObject).PictureDescriptor = wordImageReader.PictureDescriptor;
			if (wordImageReader.UnparsedData != null)
			{
				(shapeObject as InlineShapeObject).UnparsedData = wordImageReader.UnparsedData;
			}
			CharacterPropertiesConverter.SprmsToFormat(reader, shapeObject.CharacterFormat);
			if (wordImageReader.ImageRecord == null || wordImageReader.ImageRecord.m_imageBytes == null)
			{
				SinglePropertyModifierRecord singlePropertyModifierRecord = reader.CHPXSprms[2058];
				if (singlePropertyModifierRecord != null && singlePropertyModifierRecord.BoolValue)
				{
					(shapeObject as InlineShapeObject).IsOLE = true;
					(shapeObject as InlineShapeObject).OLEContainerId = reader.GetPicLocation();
				}
				if (!reader.CHPXSprms.GetBoolean(2108, defValue: false))
				{
					AddItem(shapeObject, CurrentParagraph);
					CheckTrackChanges(shapeObject, reader);
					InlineShapeObject inlineShapeObject = shapeObject as InlineShapeObject;
					string text = ((inlineShapeObject.ShapeContainer != null && inlineShapeObject.ShapeContainer.Shape != null) ? inlineShapeObject.ShapeContainer.Shape.ShapeType.ToString().TrimStart() : null);
					if (text != null && text != "msosptTextBox" && StartsWithExt(text, "msosptText"))
					{
						DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 31);
					}
					else
					{
						DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 10);
					}
				}
				return;
			}
			WPicture wPicture = null;
			if (reader.CHPXSprms.GetBoolean(2108, defValue: false) && wordImageReader.ImageRecord.m_imageBytes.Length != 0)
			{
				wPicture = new WPicture(DocumentEx);
				if (wordImageReader.InlineShapeContainer.Bse.Blip.Header.Type == MSOFBT.msofbtBlipWMF)
				{
					PlaceableMetafileHeader(wordImageReader);
				}
				wPicture.LoadImage(wordImageReader.ImageRecord);
				ListPictures.Add(wPicture);
			}
			else
			{
				wPicture = new WPicture(DocumentEx);
				if (wordImageReader.InlineShapeContainer.Bse.Blip.Header.Type == MSOFBT.msofbtBlipWMF)
				{
					PlaceableMetafileHeader(wordImageReader);
				}
				CurrentParagraph.LoadPicture(wPicture, wordImageReader.ImageRecord);
				AddItem(wPicture, CurrentParagraph);
				wPicture.IsShape = true;
				if (CurrentField != null && (CurrentField.FieldType == FieldType.FieldLink || CurrentField.FieldType == FieldType.FieldEmbed) && CurrentField.Owner is WOleObject)
				{
					(CurrentField.Owner as WOleObject).SetOlePicture(wPicture);
				}
			}
			float height = (float)wordImageReader.Height / 20f;
			float heightScaleValue = (float)wordImageReader.HeightScale / 10f;
			float width = (float)wordImageReader.Width / 20f;
			float widthScaleValue = (float)wordImageReader.WidthScale / 10f;
			CharacterPropertiesConverter.SprmsToFormat(reader, wPicture.CharacterFormat);
			CheckTrackChanges(wPicture, reader);
			SizeF size = new SizeF(width, height);
			wPicture.Size = size;
			wPicture.SetHeightScaleValue(heightScaleValue);
			wPicture.SetWidthScaleValue(widthScaleValue);
			wPicture.PictureShape = shapeObject as InlineShapeObject;
			wPicture.AlternativeText = wordImageReader.AlternativeText;
			wPicture.Name = wordImageReader.Name;
			if (wPicture.PictureShape != null && wPicture.PictureShape.ShapeContainer != null && wPicture.PictureShape.ShapeContainer.ShapeOptions != null)
			{
				wPicture.Rotation = wPicture.PictureShape.ShapeContainer.ShapeOptions.Roation / 65536;
			}
			if (wPicture.PictureShape != null && wPicture.PictureShape.ShapeContainer != null && wPicture.PictureShape.ShapeContainer.ShapePosition != null)
			{
				wPicture.LayoutInCell = wPicture.PictureShape.ShapeContainer.ShapePosition.AllowInTableCell;
				wPicture.Visible = wPicture.PictureShape.ShapeContainer.ShapePosition.Visible;
			}
			if (wPicture.PictureShape != null && wPicture.PictureShape.ShapeContainer != null && wPicture.PictureShape.ShapeContainer.ShapeOptions != null)
			{
				UpdateImageCroppingPostion(wPicture, wPicture.PictureShape.ShapeContainer);
			}
			if (wPicture.PictureShape != null && wPicture.PictureShape.ShapeContainer != null && wPicture.PictureShape.ShapeContainer.ShapeOptions != null && wPicture.PictureShape.ShapeContainer.ShapeOptions.Properties.ContainsKey(263))
			{
				wPicture.ChromaKeyColor = WordColor.ConvertRGBToColor(wPicture.PictureShape.ShapeContainer.ShapeOptions.GetPropertyValue(263));
			}
		}
	}

	private void PlaceableMetafileHeader(WordImageReader imageReader)
	{
		MsofbtMetaFile msofbtMetaFile = imageReader.InlineShapeContainer.Bse.Blip as MsofbtMetaFile;
		float num = (float)msofbtMetaFile.m_rectWidth / 12700f;
		_ = (float)msofbtMetaFile.m_rectHeight / 12700f;
		byte[] bytes = BitConverter.GetBytes((short)msofbtMetaFile.m_rectLeft);
		byte[] bytes2 = BitConverter.GetBytes((short)msofbtMetaFile.m_rectTop);
		byte[] bytes3 = BitConverter.GetBytes((short)msofbtMetaFile.m_rectRight);
		byte[] bytes4 = BitConverter.GetBytes((short)msofbtMetaFile.m_rectBottom);
		short num2 = (short)((float)(short)msofbtMetaFile.m_rectRight * 72f / num);
		byte[] bytes5 = BitConverter.GetBytes(num2);
		byte[] bytes6 = BitConverter.GetBytes(CalculateCheckSum(0, (short)msofbtMetaFile.m_rectLeft, (short)msofbtMetaFile.m_rectTop, (short)msofbtMetaFile.m_rectRight, (short)msofbtMetaFile.m_rectBottom, num2, 0));
		byte[] array = new byte[22]
		{
			215,
			205,
			198,
			154,
			0,
			0,
			bytes[0],
			bytes[1],
			bytes2[0],
			bytes2[1],
			bytes3[0],
			bytes3[1],
			bytes4[0],
			bytes4[1],
			bytes5[0],
			bytes5[1],
			0,
			0,
			0,
			0,
			bytes6[0],
			bytes6[1]
		};
		byte[] imageBytes = imageReader.ImageRecord.ImageBytes;
		byte[] array2 = new byte[imageReader.ImageRecord.ImageBytes.Length + 22];
		Buffer.BlockCopy(array, 0, array2, 0, 22);
		Buffer.BlockCopy(imageBytes, 0, array2, array.Length, imageBytes.Length);
		imageReader.ImageRecord.ImageBytes = array2;
	}

	private short CalculateCheckSum(short Handle, short left, short top, short right, short bottom, short Inch, short reserved)
	{
		return (short)(0x5711 ^ Handle ^ left ^ top ^ right ^ bottom ^ Inch ^ (reserved & 0xFFFF) ^ ((int)(reserved & 0xFFFF0000u) >> 16));
	}

	private void AddItem(ParagraphItem item, IWParagraph para)
	{
		para.Items.Add(item);
		switch (item.EntityType)
		{
		case EntityType.OleObject:
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 29);
			break;
		case EntityType.Picture:
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 31);
			break;
		case EntityType.Break:
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 6);
			break;
		case EntityType.Field:
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 18);
			if ((item as WField).FieldType == FieldType.FieldHyperlink)
			{
				DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 23);
			}
			break;
		case EntityType.Symbol:
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_2, 8);
			break;
		case EntityType.Shape:
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_2, 6);
			break;
		case EntityType.TextRange:
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_2, 12);
			break;
		}
	}

	private void ReadFldBeginMark(WordReaderBase reader)
	{
		FieldDescriptor fld = reader.GetFld();
		m_currField = new WField(DocumentEx);
		m_currField.FieldType = FieldType.FieldUnknown;
		if (fld != null)
		{
			if (Enum.IsDefined(typeof(FieldType), fld.Type))
			{
				m_currField = m_currField.CreateFieldByType(string.Empty, fld.Type);
			}
			else
			{
				m_currField.SourceFieldType = (short)fld.Type;
			}
		}
		if (reader.CHPX != null)
		{
			CharacterPropertiesConverter.SprmsToFormat(reader.CHPXSprms, m_currField.CharacterFormat, reader.StyleSheet, reader.SttbfRMarkAuthorNames, isNewPropertyHash: true);
		}
		InsertStartField(reader);
	}

	private void InsertFldSeparator(WordReaderBase reader)
	{
		WFieldMark wFieldMark = new WFieldMark(DocumentEx, FieldMarkType.FieldSeparator);
		AddItem(wFieldMark, CurrentParagraph);
		CharacterPropertiesConverter.SprmsToFormat(reader, wFieldMark.CharacterFormat);
		CheckTrackChanges(wFieldMark, reader);
		WField currentField = CurrentField;
		if (currentField == null)
		{
			return;
		}
		currentField.FieldSeparator = wFieldMark;
		if (currentField is WEmbedField)
		{
			(currentField as WEmbedField).StoragePicLocation = reader.GetPicLocation();
		}
		else
		{
			if (!(currentField is WControlField))
			{
				return;
			}
			int picLocation = reader.GetPicLocation();
			WControlField wControlField = currentField as WControlField;
			if (picLocation > 0)
			{
				wControlField.StoragePicLocation = picLocation;
				if (reader.m_streamsManager.ObjectPoolStream != null)
				{
					wControlField.OleObject.ParseObjectPool(reader.m_streamsManager.ObjectPoolStream, wControlField.StoragePicLocation.ToString(), DocumentEx.OleObjectCollection);
				}
			}
		}
	}

	private void InsertFldEndMark(WordReaderBase reader)
	{
		if (m_fieldStack.Count <= 0)
		{
			return;
		}
		WField currentField = CurrentField;
		if (currentField.FieldType == FieldType.FieldFillIn && CurrentParagraph.Items.Count > 0 && CurrentField.FieldSeparator == null)
		{
			InsertFldSeparator(reader);
		}
		WFieldMark wFieldMark = new WFieldMark(DocumentEx, FieldMarkType.FieldEnd);
		AddItem(wFieldMark, CurrentParagraph);
		currentField.FieldEnd = wFieldMark;
		CharacterPropertiesConverter.SprmsToFormat(reader, wFieldMark.CharacterFormat);
		CheckTrackChanges(wFieldMark, reader);
		DocumentEx.UpdateFieldRevision(currentField);
		if (!currentField.IsFormField())
		{
			UpdateFieldType(reader, currentField, wFieldMark);
		}
		if (m_fieldStack != null && m_fieldStack.Count > 0)
		{
			WField wField = m_fieldStack.Pop();
			if (wField.FieldType != FieldType.FieldTOC)
			{
				wField.UpdateFieldCode(wField.GetFieldCodeForUnknownFieldType());
			}
			if (wField.FieldType == FieldType.FieldDate || wField.FieldType == FieldType.FieldTime)
			{
				wField.Update();
			}
		}
	}

	private void UpdateFieldType(WordReaderBase reader, WField field, WFieldMark endMark)
	{
		if (field.FieldType == FieldType.FieldUnknown)
		{
			field.SetUnknownFieldType();
			if (field.FieldType == FieldType.FieldUnknown)
			{
				return;
			}
		}
		FieldType fieldType = field.FieldType;
		switch (fieldType)
		{
		case FieldType.FieldLink:
		case FieldType.FieldEmbed:
			ReadOleObject(reader, fieldType);
			break;
		case FieldType.FieldTOC:
			field.ReplaceAsTOCField();
			break;
		default:
			if (!field.IsFormField())
			{
				break;
			}
			goto case FieldType.FieldIf;
		case FieldType.FieldIf:
		case FieldType.FieldMergeField:
			field = field.ReplaceValidField();
			m_fieldStack.Pop();
			m_fieldStack.Push(field);
			break;
		}
	}

	private void InsertStartField(WordReaderBase reader)
	{
		WField currField = m_currField;
		m_fieldStack.Push(currField);
		AddItem(CurrentField, CurrentParagraph);
		CharacterPropertiesConverter.SprmsToFormat(reader, m_currField.CharacterFormat);
		CheckTrackChanges(CurrentField, reader);
	}

	private void ReadOleObject(WordReaderBase reader, FieldType type)
	{
		WOleObject wOleObject = new WOleObject(DocumentEx);
		WField wField = m_fieldStack.Pop();
		wField.UpdateFieldCode(wField.GetFieldCodeForUnknownFieldType());
		if (wField is WEmbedField)
		{
			wOleObject.OleStorageName = (wField as WEmbedField).StoragePicLocation.ToString();
		}
		else
		{
			wOleObject.OleStorageName = reader.GetPicLocation().ToString();
		}
		if (type == FieldType.FieldEmbed)
		{
			wOleObject.SetLinkType(OleLinkType.Embed);
		}
		else
		{
			wOleObject.SetLinkType(OleLinkType.Link);
		}
		if ((!DocumentEx.Settings.PreserveOleImageAsImage || wOleObject.LinkType != 0 || !(wField.FieldValue == "Paint.Picture")) && reader.m_streamsManager.ObjectPoolStream != null && reader.m_streamsManager.ObjectPoolStream.Length != 0L)
		{
			wOleObject.ParseObjectPool(reader.m_streamsManager.ObjectPoolStream);
		}
		if (wField.FieldSeparator != null && wField.FieldSeparator.NextSibling is WPicture)
		{
			wOleObject.SetOlePicture(wField.FieldSeparator.NextSibling as WPicture);
		}
		if (DocumentEx.Settings.PreserveOleImageAsImage && wOleObject.LinkType == OleLinkType.Embed && wField.FieldValue == "Paint.Picture")
		{
			ParagraphItem entity = wOleObject.OlePicture.Clone() as WPicture;
			int index = wField.Index;
			if (CurrentParagraph.Items.InnerList[index] is WField)
			{
				(CurrentParagraph.Items.InnerList[index] as WField).RemoveSelf();
			}
			CurrentParagraph.Items.Insert(index, entity);
		}
		else
		{
			int index2 = wField.Index;
			WParagraph ownerParagraph = wField.OwnerParagraph;
			DocumentEx.IsSkipFieldDetach = true;
			ownerParagraph.Items.RemoveAt(index2);
			DocumentEx.IsSkipFieldDetach = false;
			ownerParagraph.Items.Insert(index2, wOleObject);
			wField.SetOwner(wOleObject);
			wOleObject.Field = wField;
		}
	}

	protected void ReadListFormat(WordReaderBase reader, WListFormat listFormat)
	{
		if (reader.HasList())
		{
			_ = reader.ListInfo;
			ListPropertiesConverter.Export(listFormat, reader.PAPXSprms, reader);
		}
	}

	protected void ReadCharacterFormat(WordReaderBase reader, WCharacterFormat charFormat)
	{
		CharacterPropertiesConverter.SprmsToFormat(reader, charFormat);
	}

	protected void ReadParagraphFormat(WordReaderBase reader, IWParagraph paragraph)
	{
		ParagraphPropertiesConverter.SprmsToFormat(reader.PAPXSprms, paragraph.ParagraphFormat, reader.SttbfRMarkAuthorNames, reader.StyleSheet);
		UpdateParagraphStyle(paragraph, reader);
		if (reader.PAPXSprms != null)
		{
			ParagraphPropertiesConverter.UpdateDirectParagraphFormatting(paragraph.ParagraphFormat, reader.PAPXSprms);
		}
	}

	protected void UpdateParagraphStyle(IWParagraph paragraph, WordReaderBase reader)
	{
		int num = reader.CurrentStyleIndex;
		WordStyle styleByIndex = reader.StyleSheet.GetStyleByIndex(num);
		bool flag = false;
		SinglePropertyModifierRecord newSprm = reader.PAPX.PropertyModifiers.GetNewSprm(50689, 9828);
		SinglePropertyModifierRecord newSprm2 = reader.PAPX.PropertyModifiers.GetNewSprm(17920, 9828);
		if (newSprm != null && newSprm.ByteArray[0] == 0 && newSprm2 != null)
		{
			ushort ushortValue = newSprm2.UshortValue;
			ushort num2 = BitConverter.ToUInt16(newSprm.ByteArray, 1);
			ushort num3 = BitConverter.ToUInt16(newSprm.ByteArray, 3);
			if (num3 >= num2 && ushortValue >= num2 && ushortValue <= num3)
			{
				int num4 = 5;
				ushort num5 = (ushort)(num3 - num2 + 1);
				ushort[] array = new ushort[num5];
				for (int i = 0; i < num5; i++)
				{
					if (num4 + 1 >= newSprm.ByteArray.Length)
					{
						break;
					}
					array[i] = BitConverter.ToUInt16(newSprm.ByteArray, num4);
					num4 += 2;
				}
				ushortValue = array[ushortValue - num2];
				if (reader.StyleSheet.GetStyleByIndex(ushortValue) != null)
				{
					styleByIndex = reader.StyleSheet.GetStyleByIndex(ushortValue);
					num = ushortValue;
					flag = true;
				}
			}
		}
		if (!flag && newSprm2 != null && reader.StyleSheet.GetStyleByIndex(newSprm2.ShortValue) != null)
		{
			styleByIndex = reader.StyleSheet.GetStyleByIndex(newSprm2.ShortValue);
			num = newSprm2.ShortValue;
		}
		IWParagraphStyle iWParagraphStyle = null;
		if (styleByIndex.IsCharacterStyle)
		{
			iWParagraphStyle = DocumentEx.Styles.FindByName("Normal") as IWParagraphStyle;
		}
		else
		{
			string name = ((!reader.StyleSheet.StyleNames.ContainsKey(num)) ? styleByIndex.Name : reader.StyleSheet.StyleNames[num]);
			iWParagraphStyle = DocumentEx.Styles.FindByName(name, StyleType.ParagraphStyle) as IWParagraphStyle;
		}
		if (iWParagraphStyle != null)
		{
			(paragraph as WParagraph).ApplyStyle(iWParagraphStyle, isDomChanges: false);
			return;
		}
		if (paragraph.ParagraphFormat.m_unParsedSprms == null)
		{
			paragraph.ParagraphFormat.m_unParsedSprms = new SinglePropertyModifierArray();
		}
		paragraph.ParagraphFormat.m_unParsedSprms.SetShortValue(17920, (short)num);
	}

	private void ReadTableRowFormat(WordReaderBase reader, WTable table)
	{
		TablePropertiesConverter.SprmsToFormat(reader, table.LastRow.RowFormat);
		WTableRow lastRow = table.LastRow;
		ReadCharacterFormat(reader, lastRow.CharacterFormat);
		if (lastRow.RowFormat.HasKey(122))
		{
			Revision revision = DocumentEx.CreateNewRevision(RevisionType.Formatting, lastRow.RowFormat.FormatChangeAuthorName, lastRow.RowFormat.FormatChangeDateTime, null);
			lastRow.RowFormat.Revisions.Add(revision);
			revision.Range.Items.Add(lastRow.RowFormat);
			DocumentEx.UpdateTableFormatRevision(lastRow);
		}
		if (lastRow.CharacterFormat.IsInsertRevision)
		{
			DocumentEx.TableRowRevision(RevisionType.Insertions, lastRow, reader);
			DocumentEx.UpdateTableRowRevision(lastRow);
		}
		if (lastRow.CharacterFormat.IsDeleteRevision)
		{
			DocumentEx.TableRowRevision(RevisionType.Deletions, lastRow, reader);
			DocumentEx.UpdateTableRowRevision(lastRow);
		}
	}

	internal void CheckTrackChanges(ParagraphItem item, WordReaderBase reader)
	{
		WCharacterFormat charFormat = item.GetCharFormat();
		if (charFormat.HasKey(105))
		{
			DocumentEx.CharacterFormatChange(charFormat, item, reader);
		}
		if (charFormat.IsInsertRevision)
		{
			string authorName = DocumentEx.GetAuthorName(reader, isInsertKey: true);
			DateTime dateTime = DocumentEx.GetDateTime(reader, isInsertKey: true, charFormat);
			DocumentEx.ParagraphItemRevision(item, RevisionType.Insertions, authorName, dateTime, null, isNestedRevision: true, null, null, null);
		}
		if (charFormat.IsDeleteRevision)
		{
			string authorName2 = DocumentEx.GetAuthorName(reader, isInsertKey: false);
			DateTime dateTime2 = DocumentEx.GetDateTime(reader, isInsertKey: false, charFormat);
			DocumentEx.ParagraphItemRevision(item, RevisionType.Deletions, authorName2, dateTime2, null, isNestedRevision: true, null, null, null);
		}
	}

	internal void CheckTrackChanges(WParagraph paragraph, WordReaderBase reader)
	{
		DocumentEx.CharacterFormatChange(paragraph.BreakCharacterFormat, null, reader);
		DocumentEx.ParagraphFormatChange(paragraph.ParagraphFormat);
		DocumentEx.UpdateLastItemRevision(paragraph, paragraph.Items);
	}

	protected virtual void ProcessCommText(WordReaderBase reader, WParagraph para)
	{
	}

	internal virtual void Close()
	{
		if (m_listPic != null)
		{
			m_listPic.Clear();
			m_listPic = null;
		}
		m_textBody = null;
		m_currParagraph = null;
		m_currTable = null;
		if (m_tablesNested != null)
		{
			m_tablesNested.Clear();
			m_tablesNested = null;
		}
		if (m_nestedTextBodies != null)
		{
			m_nestedTextBodies.Clear();
			m_nestedTextBodies = null;
		}
		m_currField = null;
		if (m_fieldStack != null)
		{
			m_fieldStack.Clear();
			m_fieldStack = null;
		}
		m_bookmarkInfo = null;
		DocumentEx = null;
	}
}
