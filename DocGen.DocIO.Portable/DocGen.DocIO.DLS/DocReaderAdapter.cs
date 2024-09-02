using System;
using System.Collections.Generic;
using DocGen.DocIO.ReaderWriter;
using DocGen.DocIO.ReaderWriter.Biff_Records;
using DocGen.DocIO.ReaderWriter.DataStreamParser.Escher;

namespace DocGen.DocIO.DLS;

internal class DocReaderAdapter : DocReaderAdapterBase
{
	internal abstract class SubDocumentAdapter : DocReaderAdapterBase
	{
		internal void ReadSubDocBody(WordReader reader, WordDocument documentEx)
		{
			Init(documentEx);
			Read(reader);
			reader.UnfreezeStreamPos();
		}

		internal abstract void Read(WordReader reader);
	}

	internal class HeaderFooterAdapter : SubDocumentAdapter
	{
		private int m_currentHFType;

		private bool m_itemEnd;

		private HFTextboxAdapter m_hfTxbxAdapter;

		internal HFTextboxAdapter TextBoxAdapter => m_hfTxbxAdapter;

		internal override void Read(WordReader reader)
		{
			WordHeaderFooterReader wordHeaderFooterReader = reader.GetSubdocumentReader(WordSubdocument.HeaderFooter) as WordHeaderFooterReader;
			wordHeaderFooterReader.Bookmarks = reader.Bookmarks;
			if (reader.TablesData.HasSubdocument(WordSubdocument.HeaderTextBox))
			{
				ReadSubdocument(reader, WordSubdocument.HeaderTextBox);
			}
			int count = DocumentEx.Sections.Count;
			m_finalize = false;
			WTextBody wTextBody = null;
			wordHeaderFooterReader.MoveToSection(1);
			for (int i = 0; i < 6; i++)
			{
				wTextBody = new WTextBody(DocumentEx, null);
				ReadTextBody(wordHeaderFooterReader, wTextBody);
				RemoveLastParagraph(wTextBody);
				if (wTextBody.ChildEntities.Count > 0)
				{
					SetSeparatorBody(wTextBody, i);
				}
			}
			for (int j = 0; j < count; j++)
			{
				wordHeaderFooterReader.MoveToSection(j + 1);
				WSection wSection = DocumentEx.Sections[j];
				m_itemEnd = false;
				wordHeaderFooterReader.MoveToItem(6);
				wordHeaderFooterReader.HeaderType = HeaderType.EvenHeader;
				m_currentHFType = 0;
				while (!m_itemEnd)
				{
					wTextBody = wSection.HeadersFooters[m_currentHFType];
					ReadTextBody(wordHeaderFooterReader, wTextBody);
					if (wordHeaderFooterReader.HeaderType == HeaderType.EvenFooter || wordHeaderFooterReader.HeaderType == HeaderType.FirstPageFooter || wordHeaderFooterReader.HeaderType == HeaderType.OddFooter)
					{
						DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 20);
					}
					else if (wordHeaderFooterReader.HeaderType != HeaderType.InvalidValue)
					{
						DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 22);
					}
					m_nestedTextBodies.Clear();
				}
				m_itemEnd = false;
				m_currentHFType = 0;
				RemoveHFLastParagraphs(wSection);
			}
		}

		private void SetSeparatorBody(WTextBody textBody, int index)
		{
			switch (index)
			{
			case 0:
				DocumentEx.Footnotes.Separator = textBody;
				break;
			case 1:
				DocumentEx.Footnotes.ContinuationSeparator = textBody;
				break;
			case 2:
				DocumentEx.Footnotes.ContinuationNotice = textBody;
				break;
			case 3:
				DocumentEx.Endnotes.Separator = textBody;
				break;
			case 4:
				DocumentEx.Endnotes.ContinuationSeparator = textBody;
				break;
			case 5:
				DocumentEx.Endnotes.ContinuationNotice = textBody;
				break;
			}
		}

		internal override void Close()
		{
			base.Close();
			if (m_hfTxbxAdapter != null)
			{
				m_hfTxbxAdapter.Close();
				m_hfTxbxAdapter = null;
			}
		}

		protected override bool EndOfTextBody(WordReaderBase reader, WordChunkType chunkType)
		{
			m_currentHFType += ((chunkType == WordChunkType.EndOfSubdocText && m_currentHFType < 5) ? 1 : 0);
			m_itemEnd = chunkType == WordChunkType.DocumentEnd;
			if (chunkType != WordChunkType.DocumentEnd)
			{
				return chunkType == WordChunkType.EndOfSubdocText;
			}
			return true;
		}

		protected override void ReadTextBox(WordReaderBase reader, FileShapeAddress fspa)
		{
			if (m_hfTxbxAdapter != null)
			{
				bool skipPositionOrigins = reader.TablesData.Fib.NFibNew > 193;
				WTextBox wTextBox = m_hfTxbxAdapter.ReadTextBoxShape(fspa, skipPositionOrigins);
				if (wTextBox != null)
				{
					CharacterPropertiesConverter.SprmsToFormat(reader, wTextBox.CharacterFormat);
					base.CurrentParagraph.Items.Add(wTextBox);
					CheckTrackChanges(wTextBox, reader);
				}
			}
		}

		protected override bool ReadWatermark(WordReaderBase reader)
		{
			return reader.ReadWatermark(DocumentEx, m_textBody);
		}

		protected override void ReadAutoShapeTextBox(int shapeId, ShapeObject shapeObj)
		{
			if (m_hfTxbxAdapter != null)
			{
				WTextBox autoShapeTextBox = m_hfTxbxAdapter.GetAutoShapeTextBox(shapeId);
				if (autoShapeTextBox != null)
				{
					shapeObj.IsHeaderAutoShape = true;
					autoShapeTextBox.SetOwner(shapeObj);
					shapeObj.AutoShapeTextCollection.Add(autoShapeTextBox);
				}
			}
		}

		protected override void CheckTextEmbed(DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.ShapeBase shape, WPicture picture)
		{
			if (m_hfTxbxAdapter != null)
			{
				WTextBox autoShapeTextBox = m_hfTxbxAdapter.GetAutoShapeTextBox(shape.ShapeProps.Spid);
				if (autoShapeTextBox != null)
				{
					picture.EmbedBody = autoShapeTextBox.TextBoxBody;
				}
			}
		}

		private void ReadSubdocument(WordReader reader, WordSubdocument wsType)
		{
			SubDocumentAdapter subDocumentAdapter = null;
			if (wsType == WordSubdocument.HeaderTextBox)
			{
				subDocumentAdapter = (m_hfTxbxAdapter = new HFTextboxAdapter());
			}
			subDocumentAdapter?.ReadSubDocBody(reader, DocumentEx);
		}

		private void RemoveHFLastParagraphs(IWSection section)
		{
			for (int i = 0; i < 6; i++)
			{
				BodyItemCollection items = section.HeadersFooters[i].Items;
				if (items.LastItem is IWParagraph iWParagraph && iWParagraph.Items.Count == 0)
				{
					items.Remove(iWParagraph);
				}
			}
		}

		private void RemoveLastParagraph(WTextBody textBody)
		{
			if (textBody.Items.LastItem is IWParagraph iWParagraph && iWParagraph.Items.Count == 0)
			{
				textBody.Items.Remove(iWParagraph);
			}
		}
	}

	internal class AnnotationAdapter : SubDocumentAdapter
	{
		private List<WComment> m_comments = new List<WComment>();

		private WComment m_currComment;

		private int m_currCommentIndex;

		internal WComment CurrentComment
		{
			get
			{
				if (m_currCommentIndex < m_comments.Count)
				{
					return m_comments[m_currCommentIndex];
				}
				return null;
			}
		}

		internal List<WComment> Comments => m_comments;

		internal override void Read(WordReader reader)
		{
			WordAnnotationReader wordAnnotationReader = reader.GetSubdocumentReader(WordSubdocument.Annotation) as WordAnnotationReader;
			wordAnnotationReader.Bookmarks = reader.Bookmarks;
			do
			{
				AddComment(wordAnnotationReader);
				ReadTextBody(wordAnnotationReader, m_currComment.TextBody);
				m_currCommentIndex++;
			}
			while (wordAnnotationReader.ChunkType != WordChunkType.DocumentEnd);
			m_currCommentIndex = 0;
		}

		internal WComment GetNextComment()
		{
			if (m_currCommentIndex < m_comments.Count)
			{
				return m_comments[m_currCommentIndex++];
			}
			return null;
		}

		private void AddComment(WordAnnotationReader reader)
		{
			m_currComment = new WComment(DocumentEx);
			ReadCommentFormat(reader, m_currComment.Format);
			m_comments.Add(m_currComment);
		}

		protected override bool EndOfTextBody(WordReaderBase reader, WordChunkType chunkType)
		{
			if ((reader as WordAnnotationReader).ItemNumber == m_currCommentIndex)
			{
				return chunkType == WordChunkType.DocumentEnd;
			}
			return true;
		}

		internal override void Close()
		{
			base.Close();
			if (m_comments != null)
			{
				m_comments.Clear();
				m_comments = null;
			}
			m_currComment = null;
		}

		private void ReadCommentFormat(WordAnnotationReader reader, WCommentFormat format)
		{
			AnnotationDescriptor descriptor = reader.Descriptor;
			if (descriptor != null)
			{
				format.UserInitials = descriptor.UserInitials;
				format.User = reader.User;
				format.BookmarkStartOffset = reader.BookmarkStartOffset;
				format.BookmarkEndOffset = reader.BookmarkEndOffset;
				format.Position = reader.Position;
				format.TagBkmk = descriptor.TagBkmk.ToString();
			}
		}
	}

	internal class FootnoteAdapter : SubDocumentAdapter
	{
		internal List<WFootnote> m_footEndNotes = new List<WFootnote>();

		protected WFootnote m_currFootEndNote;

		internal int m_currFootEndnoteIndex;

		protected int m_footEndNotesCount;

		internal override void Read(WordReader reader)
		{
			WordSubdocumentReader wordSubdocumentReader = Init(reader);
			wordSubdocumentReader.Bookmarks = reader.Bookmarks;
			for (int i = 0; i < m_footEndNotesCount; i++)
			{
				AddFootEndNote(wordSubdocumentReader);
				wordSubdocumentReader.MoveToItem(m_currFootEndnoteIndex);
				ReadTextBody(wordSubdocumentReader, m_currFootEndNote.TextBody);
				m_currFootEndnoteIndex++;
			}
			m_currFootEndnoteIndex = 0;
		}

		internal WFootnote GetNextFootEndNote()
		{
			if (m_currFootEndnoteIndex < m_footEndNotes.Count)
			{
				return m_footEndNotes[m_currFootEndnoteIndex++];
			}
			return null;
		}

		protected virtual WordSubdocumentReader Init(WordReader reader)
		{
			m_footEndNotesCount = reader.TablesData.Footnotes.Count - 1;
			return reader.GetSubdocumentReader(WordSubdocument.Footnote) as WordFootnoteReader;
		}

		protected virtual void AddFootEndNote(IWordSubdocumentReader reader)
		{
			m_currFootEndNote = new WFootnote(DocumentEx);
			m_currFootEndNote.FootnoteType = FootnoteType.Footnote;
			m_footEndNotes.Add(m_currFootEndNote);
		}

		internal override void Close()
		{
			base.Close();
			if (m_footEndNotes != null)
			{
				m_footEndNotes.Clear();
				m_footEndNotes = null;
			}
			m_currFootEndNote = null;
		}

		protected override bool EndOfTextBody(WordReaderBase reader, WordChunkType chunkType)
		{
			if ((reader as WordFootnoteReader).ItemNumber == m_currFootEndnoteIndex)
			{
				return chunkType == WordChunkType.DocumentEnd;
			}
			return true;
		}

		private void ReadFootnoteFormat(IWordSubdocumentReader reader)
		{
		}
	}

	internal class EndnoteAdapter : FootnoteAdapter
	{
		protected override WordSubdocumentReader Init(WordReader reader)
		{
			m_footEndNotesCount = reader.TablesData.Endnotes.Count - 1;
			return reader.GetSubdocumentReader(WordSubdocument.Endnote) as WordEndnoteReader;
		}

		protected override void AddFootEndNote(IWordSubdocumentReader reader)
		{
			m_currFootEndNote = new WFootnote(DocumentEx);
			m_currFootEndNote.FootnoteType = FootnoteType.Endnote;
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 17);
			m_footEndNotes.Add(m_currFootEndNote);
		}
	}

	internal class TextboxAdapter : SubDocumentAdapter
	{
		protected WTextBox m_currTextBox;

		protected int m_txbxCount;

		protected WordSubdocument m_textBoxType;

		protected int m_currentTxbxIndex;

		protected ShapeObjectTextCollection m_textBoxCollection = new ShapeObjectTextCollection();

		internal override void Read(WordReader reader)
		{
			WordSubdocumentReader wordSubdocumentReader = Init(reader);
			wordSubdocumentReader.Bookmarks = reader.Bookmarks;
			m_finalize = false;
			for (int i = 0; i < m_txbxCount - 1; i++)
			{
				if (CreateAndAddTextBox(reader))
				{
					wordSubdocumentReader.MoveToItem(m_currentTxbxIndex);
					ReadTextBody(wordSubdocumentReader, m_currTextBox.TextBoxBody);
					m_nestedTextBodies.Clear();
				}
				m_currentTxbxIndex++;
			}
		}

		internal override void Close()
		{
			base.Close();
			m_currTextBox = null;
			if (m_textBoxCollection != null)
			{
				m_textBoxCollection.Close();
				m_textBoxCollection = null;
			}
		}

		protected override bool EndOfTextBody(WordReaderBase reader, WordChunkType chunkType)
		{
			if (chunkType != WordChunkType.DocumentEnd)
			{
				return m_currentTxbxIndex != (reader as WordTextBoxReader).ItemNumber;
			}
			return true;
		}

		protected virtual WordSubdocumentReader Init(WordReader reader)
		{
			if (reader.TablesData.ArtObj.MainDocTxBxs != null)
			{
				m_txbxCount = reader.TablesData.ArtObj.MainDocTxBxs.Count;
			}
			m_textBoxType = WordSubdocument.TextBox;
			return reader.GetSubdocumentReader(WordSubdocument.TextBox) as WordTextBoxReader;
		}

		private bool CreateAndAddTextBox(WordReaderBase baseReader)
		{
			bool result = false;
			int shapeObjectId = baseReader.TablesData.ArtObj.GetShapeObjectId(m_textBoxType, m_currentTxbxIndex);
			if (shapeObjectId != 0)
			{
				m_currTextBox = new WTextBox(DocumentEx);
				m_textBoxCollection.AddTextBox(shapeObjectId, m_currTextBox);
				result = true;
			}
			return result;
		}

		internal WTextBox ReadTextBoxShape(FileShapeAddress fspa, bool skipPositionOrigins)
		{
			WTextBox textBox = m_textBoxCollection.GetTextBox(fspa.Spid);
			if (textBox == null)
			{
				return null;
			}
			MsofbtSpContainer msofbtSpContainer = null;
			if (DocumentEx.Escher.Containers.ContainsKey(fspa.Spid))
			{
				msofbtSpContainer = DocumentEx.Escher.Containers[fspa.Spid] as MsofbtSpContainer;
			}
			TextBoxPropertiesConverter.Export(msofbtSpContainer, fspa, textBox.TextBoxFormat, skipPositionOrigins);
			textBox.Visible = msofbtSpContainer.ShapeOptions.Visible;
			return textBox;
		}

		internal WTextBox GetAutoShapeTextBox(int shapeId)
		{
			return m_textBoxCollection.GetTextBox(shapeId);
		}
	}

	internal class HFTextboxAdapter : TextboxAdapter
	{
		protected override WordSubdocumentReader Init(WordReader reader)
		{
			if (reader.TablesData.ArtObj.HfDocTxBxs != null)
			{
				m_txbxCount = reader.TablesData.ArtObj.HfDocTxBxs.Count;
			}
			m_textBoxType = WordSubdocument.HeaderTextBox;
			return reader.GetSubdocumentReader(WordSubdocument.HeaderTextBox) as WordHFTextBoxReader;
		}

		protected override bool EndOfTextBody(WordReaderBase reader, WordChunkType chunkType)
		{
			if (chunkType != WordChunkType.DocumentEnd)
			{
				return m_currentTxbxIndex != (reader as WordHFTextBoxReader).ItemNumber;
			}
			return true;
		}
	}

	private AnnotationAdapter m_annAdapter;

	private FootnoteAdapter m_ftnAdapter;

	private EndnoteAdapter m_endNoteAdapter;

	private TextboxAdapter m_txbxAdapter;

	private HeaderFooterAdapter m_hfAdapter;

	public void Read(WordReader reader, WordDocument wordDoc)
	{
		AdapterListIDHolder.Instance.LfoStyleIDtoName.Clear();
		AdapterListIDHolder.Instance.ListStyleIDtoName.Clear();
		Init(wordDoc);
		ReadPassword(reader);
		reader.ReadDocumentHeader(wordDoc);
		wordDoc.WriteProtected = reader.TablesData.Fib.FReadOnlyRecommended;
		wordDoc.HasPicture = reader.TablesData.Fib.FHasPic;
		wordDoc.WordVersion = reader.TablesData.Fib.FibVersion;
		ReadDOP(reader);
		wordDoc.IsOpening = true;
		ReadStyleSheet(reader);
		wordDoc.FontSubstitutionTable = reader.StyleSheet.FontSubstitutionTable;
		ReadEscher(reader);
		ReadBackground();
		ReadSubDocument(reader, WordSubdocument.Footnote);
		ReadSubDocument(reader, WordSubdocument.Annotation);
		ReadSubDocument(reader, WordSubdocument.Endnote);
		ReadSubDocument(reader, WordSubdocument.TextBox);
		do
		{
			IWSection iWSection = wordDoc.AddSection();
			ReadTextBody(reader, iWSection.Body);
			ReadSectionFormat(reader, iWSection);
			DocumentEx.SectionFormatChange(iWSection as WSection);
		}
		while (reader.ChunkType != WordChunkType.DocumentEnd);
		ReadSubDocument(reader, WordSubdocument.HeaderFooter);
		ReadDocumentProperties(reader);
		if (wordDoc.Watermark.Type == WatermarkType.NoWatermark)
		{
			CheckWatermark(wordDoc.Sections[0]);
		}
		if (DocumentEx.HasListStyle())
		{
			ParseListPicture();
		}
		reader.ReadDocumentEnd();
		wordDoc.FFNStringTable = reader.m_docInfo.TablesData.FFNStringTable;
		if (reader.StyleSheet.StylesCount > 0)
		{
			wordDoc.HasStyleSheets = true;
		}
		wordDoc.Settings.SetCompatibilityModeValue(CompatibilityMode.Word2003);
		wordDoc.IsOpening = false;
		reader.Close();
		Close();
	}

	private void ReadPassword(WordReader reader)
	{
		if (DocumentEx.Password != null)
		{
			reader.NeedPassword += DocumentEx.GetPasswordValue;
		}
	}

	private void ReadStyleSheet(WordReader reader)
	{
		WordStyleSheet styleSheet = reader.StyleSheet;
		int stylesCount = styleSheet.StylesCount;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		for (int i = 0; i < stylesCount; i++)
		{
			WordStyle styleByIndex = styleSheet.GetStyleByIndex(i);
			if (styleByIndex.Name == null)
			{
				continue;
			}
			string text = UpdateStyleNameBasedOnId(styleByIndex);
			if (dictionary.ContainsKey(text))
			{
				string key = text;
				text = text + "_" + dictionary[text];
				while (dictionary.ContainsKey(text))
				{
					text = text + "_" + dictionary[text];
				}
				dictionary[key]++;
			}
			else
			{
				dictionary.Add(text, 0);
			}
			if (i == 13)
			{
				styleSheet.IsFixedIndex13HasStyle = true;
				styleSheet.FixedIndex13StyleName = text;
			}
			if (i == 14)
			{
				styleSheet.IsFixedIndex14HasStyle = true;
				styleSheet.FixedIndex14StyleName = text;
			}
			styleByIndex.Name = text;
			styleSheet.StyleNames.Add(i, text);
			if (!styleByIndex.IsCharacterStyle)
			{
				IStyle style = DocumentEx.AddStyle(StyleType.ParagraphStyle, text);
				WParagraphStyle wParagraphStyle = style as WParagraphStyle;
				wParagraphStyle.StyleId = styleByIndex.ID;
				wParagraphStyle.IsPrimaryStyle = styleByIndex.IsPrimary;
				wParagraphStyle.IsSemiHidden = styleByIndex.IsSemiHidden;
				wParagraphStyle.UnhideWhenUsed = styleByIndex.UnhideWhenUsed;
				(style as Style).TypeCode = styleByIndex.TypeCode;
				if (styleByIndex.TypeCode == WordStyleType.TableStyle && styleByIndex.TableStyleData != null)
				{
					Style style2 = style as Style;
					style2.TableStyleData = new byte[styleByIndex.TableStyleData.Length];
					Buffer.BlockCopy(styleByIndex.TableStyleData, 0, style2.TableStyleData, 0, styleByIndex.TableStyleData.Length);
				}
				if (styleByIndex.PAPX != null)
				{
					ParagraphPropertiesConverter.SprmsToFormat(styleByIndex.PAPX.PropertyModifiers, wParagraphStyle.ParagraphFormat, reader.SttbfRMarkAuthorNames, reader.StyleSheet);
				}
				if (styleByIndex.CHPX != null)
				{
					CharacterPropertiesConverter.SprmsToFormat(styleByIndex.CHPX.PropertyModifiers, wParagraphStyle.CharacterFormat, styleByIndex.StyleSheet, reader.SttbfRMarkAuthorNames, isNewPropertyHash: true);
				}
				if (reader.HasList())
				{
					if (styleByIndex.PAPX != null)
					{
						ListPropertiesConverter.Export(wParagraphStyle.ListFormat, styleByIndex.PAPX.PropertyModifiers, reader);
					}
					if (wParagraphStyle.ListFormat.CurrentListLevel != null)
					{
						wParagraphStyle.ListFormat.CurrentListLevel.ParaStyleName = wParagraphStyle.Name;
					}
				}
			}
			else
			{
				WCharacterStyle wCharacterStyle = (WCharacterStyle)DocumentEx.AddStyle(StyleType.CharacterStyle, text);
				wCharacterStyle.StyleId = styleByIndex.ID;
				wCharacterStyle.IsPrimaryStyle = styleByIndex.IsPrimary;
				wCharacterStyle.IsSemiHidden = styleByIndex.IsSemiHidden;
				wCharacterStyle.UnhideWhenUsed = styleByIndex.UnhideWhenUsed;
				wCharacterStyle.TypeCode = styleByIndex.TypeCode;
				if (styleByIndex.TypeCode == WordStyleType.TableStyle && styleByIndex.TableStyleData != null)
				{
					Style style3 = wCharacterStyle;
					style3.TableStyleData = new byte[styleByIndex.TableStyleData.Length];
					Buffer.BlockCopy(styleByIndex.TableStyleData, 0, style3.TableStyleData, 0, styleByIndex.TableStyleData.Length);
				}
				if (styleByIndex.CHPX != null)
				{
					CharacterPropertiesConverter.SprmsToFormat(styleByIndex.CHPX.PropertyModifiers, wCharacterStyle.CharacterFormat, styleByIndex.StyleSheet, reader.SttbfRMarkAuthorNames, isNewPropertyHash: true);
				}
			}
		}
		int j = 0;
		for (int count = DocumentEx.Styles.Count; j < count; j++)
		{
			Style style4 = DocumentEx.Styles[j] as Style;
			if (string.IsNullOrEmpty(style4.Name))
			{
				continue;
			}
			int index = styleSheet.StyleNameToIndex(style4.Name, style4.StyleType == StyleType.CharacterStyle);
			int baseStyleIndex = styleSheet.GetStyleByIndex(index).BaseStyleIndex;
			if (baseStyleIndex != 4095)
			{
				if (styleSheet.StyleNames.ContainsKey(baseStyleIndex) && style4.StyleId != 0)
				{
					string text2 = styleSheet.StyleNames[baseStyleIndex];
					if (text2 != null)
					{
						style4.ApplyBaseStyle(text2);
					}
				}
			}
			else if (style4.BaseStyle != null && style4 is WParagraphStyle)
			{
				style4.RemoveBaseStyle();
			}
			int nextStyleIndex = styleSheet.GetStyleByIndex(index).NextStyleIndex;
			if (nextStyleIndex != 4095)
			{
				WordStyle styleByIndex2 = styleSheet.GetStyleByIndex(nextStyleIndex);
				if (styleByIndex2 != null)
				{
					style4.NextStyle = styleByIndex2.Name;
				}
			}
			int linkStyleIndex = styleSheet.GetStyleByIndex(index).LinkStyleIndex;
			if (linkStyleIndex != 4095 && linkStyleIndex != 0)
			{
				WordStyle styleByIndex3 = styleSheet.GetStyleByIndex(linkStyleIndex);
				if (styleByIndex3 != null)
				{
					style4.LinkedStyleName = styleByIndex3.Name;
				}
			}
		}
		DocumentEx.Styles.FixedIndex13HasStyle = styleSheet.IsFixedIndex13HasStyle;
		DocumentEx.Styles.FixedIndex14HasStyle = styleSheet.IsFixedIndex14HasStyle;
		DocumentEx.Styles.FixedIndex13StyleName = styleSheet.FixedIndex13StyleName;
		DocumentEx.Styles.FixedIndex14StyleName = styleSheet.FixedIndex14StyleName;
	}

	private string UpdateStyleNameBasedOnId(WordStyle wordStyle)
	{
		WParagraphStyle wParagraphStyle = new WParagraphStyle(DocumentEx);
		string result = wordStyle.Name;
		Dictionary<string, int> builtinStyleIds = wParagraphStyle.GetBuiltinStyleIds();
		Dictionary<string, string> builtinStyles = wParagraphStyle.GetBuiltinStyles();
		if (builtinStyleIds.ContainsValue(wordStyle.ID))
		{
			foreach (KeyValuePair<string, int> item in builtinStyleIds)
			{
				if (item.Value != wordStyle.ID)
				{
					continue;
				}
				foreach (KeyValuePair<string, string> item2 in builtinStyles)
				{
					if (item2.Key.Replace(" ", string.Empty) == item.Key)
					{
						result = item2.Value;
						if (!DocumentEx.StyleNameIds.ContainsKey(item.Key))
						{
							DocumentEx.StyleNameIds.Add(item.Key, item2.Value);
						}
						break;
					}
				}
				break;
			}
		}
		return result;
	}

	private void ReadEscher(WordReader reader)
	{
		if (reader.Escher != null)
		{
			DocumentEx.Escher = reader.Escher;
		}
	}

	private void ReadBackground()
	{
		DocumentEx.ReadBackground();
	}

	private void ReadSubDocument(WordReader reader, WordSubdocument wsType)
	{
		if (SubDocumentExists(reader, wsType))
		{
			SubDocumentAdapter subDocumentAdapter = null;
			switch (wsType)
			{
			case WordSubdocument.HeaderFooter:
				subDocumentAdapter = (m_hfAdapter = new HeaderFooterAdapter());
				break;
			case WordSubdocument.Footnote:
				subDocumentAdapter = (m_ftnAdapter = new FootnoteAdapter());
				break;
			case WordSubdocument.Endnote:
				subDocumentAdapter = (m_endNoteAdapter = new EndnoteAdapter());
				break;
			case WordSubdocument.Annotation:
				subDocumentAdapter = (m_annAdapter = new AnnotationAdapter());
				break;
			case WordSubdocument.TextBox:
				subDocumentAdapter = (m_txbxAdapter = new TextboxAdapter());
				break;
			}
			subDocumentAdapter?.ReadSubDocBody(reader, DocumentEx);
		}
	}

	private void ReadSectionFormat(WordReader reader, IWSection sec)
	{
		if (reader.ChunkType != WordChunkType.DocumentEnd)
		{
			ReadListFormat(reader, base.CurrentParagraph.ListFormat);
			ReadCharacterFormat(reader, base.CurrentParagraph.BreakCharacterFormat);
			ReadParagraphFormat(reader, base.CurrentParagraph);
			CheckTrackChanges(base.CurrentParagraph, reader);
		}
		SectionPropertiesConverter.Export(reader, sec as WSection, parseAll: true);
		m_currParagraph = null;
	}

	private void ReadDocumentProperties(WordReader reader)
	{
		if (reader.BuiltinDocumentProperties != null)
		{
			DocumentEx.m_builtinProp = reader.BuiltinDocumentProperties;
		}
		if (reader.CustomDocumentProperties != null)
		{
			DocumentEx.m_customProp = reader.CustomDocumentProperties;
		}
		DocumentEx.SttbfRMark = reader.SttbfRMark;
		if (reader.MacrosStream != null)
		{
			DocumentEx.MacrosData = reader.MacrosStream.ToArray();
		}
		if (reader.Variables != null)
		{
			DocumentEx.Variables.UpdateVariables(reader.Variables);
		}
		if (reader.MacroCommands != null)
		{
			DocumentEx.MacroCommands = reader.MacroCommands;
		}
		if (reader.AssociatedStrings != null)
		{
			DocumentEx.AssociatedStrings.Parse(reader.AssociatedStrings);
		}
		if (reader.GrammarSpellingData != null)
		{
			DocumentEx.GrammarSpellingData = reader.GrammarSpellingData;
		}
		if (reader.DOP != null)
		{
			DocumentEx.DifferentOddAndEvenPages = reader.DOP.OddAndEvenPagesHeaderFooter;
			DocumentEx.DefaultTabWidth = (float)(int)reader.DOP.DefaultTabWidth / 20f;
		}
		if (reader.DOP.ViewType != 1)
		{
			DocumentEx.ViewSetup.DocumentViewType = (DocumentViewType)reader.DOP.ViewType;
		}
		if (reader.DOP.ZoomType != 0)
		{
			DocumentEx.ViewSetup.ZoomType = (ZoomType)reader.DOP.ZoomType;
		}
		if (reader.DOP.ZoomPercent != 0 && reader.DOP.ZoomPercent != 100)
		{
			DocumentEx.ViewSetup.SetZoomPercentValue(reader.DOP.ZoomPercent);
		}
		DocumentEx.StandardAsciiFont = reader.StandardAsciiFont;
		DocumentEx.StandardFarEastFont = reader.StandardFarEastFont;
		DocumentEx.StandardNonFarEastFont = reader.StandardNonFarEastFont;
		DocumentEx.StandardBidiFont = reader.StandardBidiFont;
		DocumentEx.Properties.SetVersion(reader.Version);
	}

	private void ReadBuiltInDocumentProperties(WordReader reader)
	{
		if (reader.BuiltinDocumentProperties != null)
		{
			DocumentEx.m_builtinProp = reader.BuiltinDocumentProperties;
		}
	}

	private void ReadDOP(WordReader reader)
	{
		if (reader.DOP != null)
		{
			DocumentEx.DOP = reader.DOP;
		}
	}

	private bool SubDocumentExists(WordReader reader, WordSubdocument wsType)
	{
		return reader.TablesData.HasSubdocument(wsType);
	}

	internal override void Close()
	{
		base.Close();
		if (m_annAdapter != null)
		{
			m_annAdapter.Close();
			m_annAdapter = null;
		}
		if (m_ftnAdapter != null)
		{
			m_ftnAdapter.Close();
			m_ftnAdapter = null;
		}
		if (m_endNoteAdapter != null)
		{
			m_endNoteAdapter.Close();
			m_endNoteAdapter = null;
		}
		if (m_txbxAdapter != null)
		{
			m_txbxAdapter.Close();
			m_txbxAdapter = null;
		}
		if (m_hfAdapter != null)
		{
			m_hfAdapter.Close();
			m_hfAdapter = null;
		}
	}

	protected override bool EndOfTextBody(WordReaderBase reader, WordChunkType chunkType)
	{
		if (chunkType != WordChunkType.SectionEnd)
		{
			return chunkType == WordChunkType.DocumentEnd;
		}
		return true;
	}

	protected override void ReadAnnotation(WordReaderBase reader)
	{
		if (m_annAdapter == null)
		{
			return;
		}
		WComment nextComment = m_annAdapter.GetNextComment();
		if (nextComment != null)
		{
			if (nextComment.Format.TagBkmk == "" || nextComment.Format.TagBkmk == "-1")
			{
				nextComment.Format.UpdateTagBkmk();
			}
			UpdateCommentMarks(nextComment);
			base.CurrentParagraph.Items.Add(nextComment);
			DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 10);
			DocumentEx.SetTriggerElement(ref DocumentEx.m_notSupportedElementFlag, 6);
		}
	}

	protected override void ReadFootnote(WordReaderBase reader)
	{
		WordReader wordReader = reader as WordReader;
		bool flag = true;
		bool isCustomFootnoteSplittedText = false;
		string empty = string.Empty;
		while (wordReader != null && flag)
		{
			WFootnote wFootnote = null;
			empty = reader.TextChunk;
			if (wordReader.IsFootnote)
			{
				wFootnote = m_ftnAdapter.GetNextFootEndNote();
				if (wordReader.CustomFnSplittedTextLength > -1)
				{
					isCustomFootnoteSplittedText = true;
					empty = ((wordReader.CustomFnSplittedTextLength < wordReader.TextChunk.Length) ? reader.TextChunk.Substring(wordReader.CustomFnSplittedTextLength) : empty);
				}
			}
			else if (wordReader.IsEndnote)
			{
				wFootnote = m_endNoteAdapter.GetNextFootEndNote();
			}
			flag = IsMultipleFootNoteEndNoteMarker(ref empty, wordReader, wFootnote, isCustomFootnoteSplittedText);
			if (wFootnote != null)
			{
				base.CurrentParagraph.Items.Add(wFootnote);
				DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_1, 21);
				if (reader.ChunkType != WordChunkType.Footnote)
				{
					wFootnote.CustomMarker = empty;
					wFootnote.IsAutoNumbered = false;
				}
				if (reader.ChunkType == WordChunkType.Symbol)
				{
					SymbolDescriptor symbolDescriptor = reader.GetSymbolDescriptor();
					wFootnote.SymbolCode = symbolDescriptor.CharCode;
					wFootnote.SymbolFontName = reader.StyleSheet.FontNamesList[symbolDescriptor.FontCode];
				}
				ReadCharacterFormat(reader, wFootnote.MarkerCharacterFormat);
				CheckTrackChanges(wFootnote, reader);
				ReadParagraphFormat(reader, base.CurrentParagraph);
				DocumentEx.ParagraphFormatChange(base.CurrentParagraph.ParagraphFormat);
			}
		}
	}

	internal new bool StartsWithExt(string text, string value)
	{
		return text.StartsWith(value);
	}

	private bool IsMultipleFootNoteEndNoteMarker(ref string footNoteMarker, WordReader wReader, WFootnote footnote, bool isCustomFootnoteSplittedText)
	{
		bool flag = wReader.CustomFnSplittedTextLength < wReader.TextChunk.Length;
		string text = ((isCustomFootnoteSplittedText && flag) ? wReader.TextChunk.Substring(wReader.CustomFnSplittedTextLength) : wReader.TextChunk);
		if (footnote == null)
		{
			return false;
		}
		if (footnote.TextBody.Paragraphs.Count > 0 && !StartsWithExt(footnote.TextBody.Paragraphs[0].Text, text))
		{
			int num = 0;
			footNoteMarker = text[0].ToString();
			while (++num < text.Length && StartsWithExt(footnote.TextBody.Paragraphs[0].Text, footNoteMarker + text[num]))
			{
				footNoteMarker += text[num];
			}
			text = text.Replace(footNoteMarker, string.Empty);
			WParagraph wParagraph = null;
			if (wReader.IsFootnote && m_ftnAdapter.m_currFootEndnoteIndex < m_ftnAdapter.m_footEndNotes.Count)
			{
				wParagraph = m_ftnAdapter.m_footEndNotes[m_ftnAdapter.m_currFootEndnoteIndex].TextBody.Paragraphs[0];
			}
			else if (wReader.IsEndnote && m_endNoteAdapter.m_currFootEndnoteIndex < m_endNoteAdapter.m_footEndNotes.Count)
			{
				wParagraph = m_endNoteAdapter.m_footEndNotes[m_endNoteAdapter.m_currFootEndnoteIndex].TextBody.Paragraphs[0];
			}
			if (wParagraph != null && StartsWithExt(wParagraph.Text, text))
			{
				wReader.TextChunk = text;
				return true;
			}
			footNoteMarker = ((isCustomFootnoteSplittedText && flag) ? footNoteMarker : wReader.TextChunk);
		}
		return false;
	}

	protected override void ReadDocumentEnd(WordReaderBase reader)
	{
	}

	protected override void ReadTextBox(WordReaderBase reader, FileShapeAddress fspa)
	{
		if (m_txbxAdapter != null)
		{
			bool skipPositionOrigins = reader.TablesData.Fib.NFibNew > 193;
			WTextBox wTextBox = m_txbxAdapter.ReadTextBoxShape(fspa, skipPositionOrigins);
			if (wTextBox != null)
			{
				CharacterPropertiesConverter.SprmsToFormat(reader, wTextBox.CharacterFormat);
				DocumentEx.SetTriggerElement(ref DocumentEx.m_supportedElementFlag_2, 13);
				base.CurrentParagraph.Items.Add(wTextBox);
				CheckTrackChanges(wTextBox, reader);
			}
		}
	}

	protected override void CheckTextEmbed(DocGen.DocIO.ReaderWriter.DataStreamParser.Escher.ShapeBase shape, WPicture picture)
	{
		if (m_txbxAdapter != null)
		{
			WTextBox autoShapeTextBox = m_txbxAdapter.GetAutoShapeTextBox(shape.ShapeProps.Spid);
			if (autoShapeTextBox != null)
			{
				picture.EmbedBody = autoShapeTextBox.TextBoxBody;
			}
		}
	}

	protected override void ReadAutoShapeTextBox(int shapeId, ShapeObject shapeObj)
	{
		if (m_txbxAdapter != null)
		{
			WTextBox autoShapeTextBox = m_txbxAdapter.GetAutoShapeTextBox(shapeId);
			if (autoShapeTextBox != null)
			{
				shapeObj.IsHeaderAutoShape = false;
				shapeObj.AutoShapeTextCollection.Add(autoShapeTextBox);
			}
		}
	}

	protected override void ProcessCommText(WordReaderBase reader, WParagraph para)
	{
		if (!(reader is WordReader) || m_annAdapter == null || para == null || para.Items.Count == 0 || m_annAdapter.Comments == null || m_annAdapter.Comments.Count == 0)
		{
			return;
		}
		WComment currentComment = m_annAdapter.CurrentComment;
		if (currentComment == null)
		{
			return;
		}
		if (reader.StartTextPos >= currentComment.Format.StartTextPos && reader.EndTextPos <= currentComment.Format.Position)
		{
			currentComment.CommentedItems.InnerList.Add(para.LastItem);
		}
		else if (para.LastItem is WTextRange && currentComment.Format.StartTextPos <= reader.EndTextPos)
		{
			if (reader.StartTextPos < currentComment.Format.StartTextPos && reader.EndTextPos <= currentComment.Format.Position)
			{
				SplitCommText(para, reader.StartTextPos, currentComment.Format.StartTextPos);
				currentComment.CommentedItems.InnerList.Add(para.LastItem);
			}
			else if (reader.StartTextPos > currentComment.Format.StartTextPos && reader.EndTextPos > currentComment.Format.Position && reader.StartTextPos < currentComment.Format.Position)
			{
				SplitCommText(para, reader.StartTextPos, currentComment.Format.Position);
				currentComment.CommentedItems.InnerList.Add(para.LastItem.PreviousSibling as ParagraphItem);
			}
			else if (reader.StartTextPos < currentComment.Format.StartTextPos && reader.EndTextPos > currentComment.Format.Position)
			{
				SplitCommText(para, reader.StartTextPos, currentComment.Format.StartTextPos);
				SplitCommText(para, reader.StartTextPos, currentComment.Format.Position);
				currentComment.CommentedItems.InnerList.Add(para.LastItem.PreviousSibling as ParagraphItem);
			}
		}
	}

	private void SplitCommText(WParagraph para, int startTextPos, int splitPos)
	{
		if (splitPos > startTextPos)
		{
			WTextRange wTextRange = para.LastItem as WTextRange;
			string text = wTextRange.Text;
			int num = splitPos - startTextPos;
			if (num <= text.Length)
			{
				wTextRange.Text = text.Substring(0, num);
				string text2 = text.Substring(num, text.Length - num);
				para.AppendText(text2).ApplyCharacterFormat(wTextRange.CharacterFormat);
			}
		}
	}

	private void UpdateCommentMarks(WComment comment)
	{
		int count = comment.CommentedItems.Count;
		if (comment.CommentedItems.Count != 0)
		{
			WCommentMark wCommentMark = new WCommentMark(DocumentEx, comment.Format.TagBkmk);
			WCommentMark wCommentMark2 = new WCommentMark(DocumentEx, comment.Format.TagBkmk, CommentMarkType.CommentEnd);
			wCommentMark.Comment = comment;
			wCommentMark2.Comment = comment;
			comment.CommentRangeStart = wCommentMark;
			comment.CommentRangeEnd = wCommentMark2;
			ParagraphItem paragraphItem = comment.CommentedItems[0];
			if (paragraphItem.PreviousSibling == null)
			{
				paragraphItem.OwnerParagraph.Items.Insert(0, wCommentMark);
			}
			else
			{
				int indexInOwnerCollection = paragraphItem.GetIndexInOwnerCollection();
				paragraphItem.OwnerParagraph.Items.Insert(indexInOwnerCollection, wCommentMark);
			}
			ParagraphItem paragraphItem2 = comment.CommentedItems[count - 1];
			if (paragraphItem2.NextSibling == null)
			{
				paragraphItem2.OwnerParagraph.Items.Add(wCommentMark2);
				return;
			}
			int indexInOwnerCollection2 = paragraphItem2.GetIndexInOwnerCollection();
			paragraphItem.OwnerParagraph.Items.Insert(indexInOwnerCollection2 + 1, wCommentMark2);
		}
	}

	private void ParseListPicture()
	{
		foreach (ListStyle listStyle in DocumentEx.ListStyles)
		{
			int i = 0;
			for (int count = listStyle.Levels.Count; i < count; i++)
			{
				WListLevel wListLevel = listStyle.Levels[i];
				int picIndex = wListLevel.PicIndex;
				if (picIndex >= 0 && picIndex != int.MaxValue && picIndex <= base.ListPictures.Count - 1)
				{
					WPicture picBullet = base.ListPictures[picIndex];
					wListLevel.PicBullet = picBullet;
				}
			}
		}
	}

	private void CheckWatermark(WSection section)
	{
		if (!section.HeadersFooters.OddHeader.WriteWatermark && !section.HeadersFooters.FirstPageHeader.WriteWatermark && !section.HeadersFooters.EvenHeader.WriteWatermark)
		{
			section.Document.InsertWatermark(WatermarkType.NoWatermark);
		}
	}
}
