using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

internal class AlternateChunk : TextBodyItem
{
	private string m_targetId;

	private string m_contentPath;

	private string m_contentType;

	private bool m_isParaItem;

	private ImportOptions m_importOption = ImportOptions.UseDestinationStyles;

	private List<Entity> altChunkBookmarks;

	private List<Entity> bkmkCollection = new List<Entity>();

	private WSection firstChunkLastSection;

	private byte m_bFlags;

	internal string TargetId
	{
		get
		{
			return m_targetId;
		}
		set
		{
			m_targetId = value;
		}
	}

	internal string ContentExtension
	{
		get
		{
			if (m_contentPath != null)
			{
				return m_contentPath.Split('.')[^1];
			}
			return string.Empty;
		}
	}

	internal ImportOptions ImportOption
	{
		get
		{
			return m_importOption;
		}
		set
		{
			m_importOption = value;
		}
	}

	internal string ContentType
	{
		get
		{
			return m_contentType;
		}
		set
		{
			m_contentType = value;
		}
	}

	internal string ContentPath
	{
		get
		{
			return m_contentPath;
		}
		set
		{
			m_contentPath = value;
		}
	}

	internal Stream Stream
	{
		get
		{
			Part part = base.Document.DocxPackage.FindPart("word/" + m_contentPath);
			if (part != null)
			{
				return part.DataStream;
			}
			return base.Document.DocxPackage.FindPart(m_contentPath).DataStream;
		}
	}

	internal List<Entity> AltChunkBookmarks
	{
		get
		{
			if (altChunkBookmarks == null)
			{
				altChunkBookmarks = new List<Entity>();
			}
			return altChunkBookmarks;
		}
	}

	internal bool IsParagraphItem
	{
		get
		{
			return m_isParaItem;
		}
		set
		{
			m_isParaItem = value;
		}
	}

	internal bool IsOwnerDocHavingOneSection
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

	internal bool IsFirstChunk
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

	internal bool IsLastChunk
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

	internal WSection FirstChunkLastSection
	{
		get
		{
			return firstChunkLastSection;
		}
		set
		{
			firstChunkLastSection = value;
		}
	}

	public override EntityType EntityType => EntityType.AlternateChunk;

	public EntityCollection ChildEntities => null;

	internal AlternateChunk(WordDocument doc)
		: base(doc)
	{
	}

	internal new AlternateChunk Clone()
	{
		return (AlternateChunk)CloneImpl();
	}

	protected override object CloneImpl()
	{
		return (AlternateChunk)base.CloneImpl();
	}

	internal override TextBodyItem GetNextTextBodyItemValue()
	{
		if (base.NextSibling != null)
		{
			return base.NextSibling as TextBodyItem;
		}
		if (base.Owner is WTableCell)
		{
			return (base.Owner as WTableCell).GetNextTextBodyItem();
		}
		if (base.Owner is WTextBody)
		{
			if (base.OwnerTextBody.Owner is WTextBox)
			{
				return (base.OwnerTextBody.Owner as WTextBox).GetNextTextBodyItem();
			}
			if (base.OwnerTextBody.Owner is WSection)
			{
				return GetNextInSection(base.OwnerTextBody.Owner as WSection);
			}
		}
		return null;
	}

	internal bool GetFormatType(string extension)
	{
		string text = extension.ToLower();
		if (text != null)
		{
			int length = text.Length;
			if (length != 3)
			{
				if (length == 4)
				{
					char c = text[2];
					if (c != 'c')
					{
						if (c != 'm')
						{
							if (c == 't' && (text == "dotx" || text == "dotm"))
							{
								goto IL_0132;
							}
						}
						else if (text == "html")
						{
							goto IL_0132;
						}
					}
					else if (text == "docx" || text == "docm")
					{
						goto IL_0132;
					}
				}
			}
			else
			{
				char c = text[0];
				if ((uint)c <= 104u)
				{
					if (c != 'd')
					{
						if (c != 'h' || !(text == "htm"))
						{
							goto IL_0134;
						}
					}
					else
					{
						switch (text)
						{
						case "doc":
						case "dot":
						case "dat":
							break;
						default:
							goto IL_0134;
						}
					}
					goto IL_0132;
				}
				if (c != 'r')
				{
					if (c != 't')
					{
						if (c == 'x' && text == "xml")
						{
							goto IL_0132;
						}
					}
					else if (text == "txt")
					{
						goto IL_0132;
					}
				}
				else if (text == "rtf")
				{
					goto IL_0132;
				}
			}
		}
		goto IL_0134;
		IL_0134:
		return false;
		IL_0132:
		return true;
	}

	internal void Update()
	{
		int num = 0;
		WSection wSection = null;
		Entity ownerTextBody = GetOwnerTextBody(this);
		if (ownerTextBody is WSection)
		{
			wSection = ownerTextBody as WSection;
			num = wSection.GetIndexInOwnerCollection();
		}
		WordDocument wordDocument = null;
		try
		{
			wordDocument = new WordDocument();
			wordDocument.m_AltChunkOwner = base.Document;
			FormatType formatType = FormatType.Docx;
			formatType = FormatType.Doc;
			wordDocument.Open(Stream, formatType, XHTMLValidationType.None);
			wordDocument.UpdateAlternateChunks();
		}
		catch
		{
			return;
		}
		bool importStyles = base.Document.ImportStyles;
		if ((ImportOption & ImportOptions.KeepSourceFormatting) != 0)
		{
			base.Document.ImportOptions = ImportOptions.KeepSourceFormatting;
		}
		else
		{
			base.Document.ImportStyles = false;
		}
		if (IsOwnerDocHavingOneSection && IsFirstChunk && wSection != null && wSection.Body.ChildEntities.Count > 0 && wSection.Body.ChildEntities[0] == this)
		{
			FirstChunkLastSection = wordDocument.LastSection.Clone();
		}
		int altchunkIndex = GetIndexInOwnerCollection();
		WTextBody ownerTextBody2 = base.OwnerTextBody;
		ownerTextBody2.ChildEntities.Remove(this);
		int count = ownerTextBody2.ChildEntities.Count;
		WParagraph lastParagraph = wordDocument.LastParagraph;
		if (lastParagraph != null)
		{
			lastParagraph.IsLastItem = true;
		}
		if (wordDocument.Sections.Count == 1)
		{
			WSection wSection2 = wordDocument.Sections[0];
			WTextBody body = wSection2.Body;
			if (!(ownerTextBody is HeaderFooter) && !(ownerTextBody is WFootnote) && !(ownerTextBody is WComment) && altchunkIndex == 0 && wSection != null && ownerTextBody2.Owner is WSection)
			{
				UpdateHeaderFooter(wSection2, ownerTextBody2, wSection);
			}
			for (int i = 0; i < body.ChildEntities.Count; i++)
			{
				if (i == 0 && IsParagraphItem && body.ChildEntities[0] is WParagraph)
				{
					MergeAltChunkFirstParagraph(ref altchunkIndex, ownerTextBody2, body);
					continue;
				}
				if (i == 0 && IsParagraphItem && body.ChildEntities[0] is WTable)
				{
					MergeAltChunkFirstTable(ref altchunkIndex, ownerTextBody2, body);
					continue;
				}
				Entity entity = body.ChildEntities[i];
				CheckPictureTextWrappingStyle(ownerTextBody2, entity);
				ownerTextBody2.ChildEntities.Insert(altchunkIndex + i, body.ChildEntities[i].Clone());
			}
		}
		else if (wordDocument.Sections.Count > 1)
		{
			int num2 = 0;
			int num3 = altchunkIndex;
			int num4 = 0;
			for (int j = 0; j < wordDocument.Sections.Count; j++)
			{
				if (ownerTextBody is HeaderFooter || ownerTextBody is WFootnote || ownerTextBody is WComment)
				{
					WSection wSection3 = wordDocument.Sections[j];
					if (j > 0)
					{
						num3 += num4;
					}
					for (int k = 0; k < wSection3.Body.ChildEntities.Count; k++)
					{
						Entity entity2 = wSection3.Body.ChildEntities[k].Clone();
						ownerTextBody2.ChildEntities.Insert(num3 + k, entity2);
					}
					num4 = wSection3.Body.ChildEntities.Count;
					continue;
				}
				if (j == 0)
				{
					WSection wSection4 = wordDocument.Sections[0];
					WTextBody body2 = wSection4.Body;
					if (wSection != null)
					{
						if (IsOwnerDocHavingOneSection && !IsFirstChunk)
						{
							ClearExistingHeadersFooters(wSection);
						}
						UpdateHeaderFooter(wSection4, ownerTextBody2, wSection);
					}
					num2 = altchunkIndex + body2.ChildEntities.Count;
					for (int l = 0; l < body2.ChildEntities.Count; l++)
					{
						if (l == 0 && IsParagraphItem)
						{
							MergeAltChunkFirstParagraph(ref altchunkIndex, ownerTextBody2, body2);
							continue;
						}
						Entity entity3 = body2.ChildEntities[l];
						CheckPictureTextWrappingStyle(ownerTextBody2, entity3);
						ownerTextBody2.ChildEntities.Insert(altchunkIndex + l, body2.ChildEntities[l].Clone());
					}
					continue;
				}
				base.Document.Sections.Insert(num + j, wordDocument.Sections[j].Clone());
				if (j != wordDocument.Sections.Count - 1)
				{
					continue;
				}
				WSection wSection5 = base.Document.Sections[num];
				WSection wSection6 = base.Document.Sections[num + j];
				if (IsOwnerDocHavingOneSection && IsLastChunk)
				{
					ClearExistingHeadersFooters(wSection6);
					if (FirstChunkLastSection != null)
					{
						UpdateHeaderFooter(FirstChunkLastSection, ownerTextBody2, wSection6);
					}
					else
					{
						wSection6.HeadersFooters.LinkToPrevious = true;
					}
				}
				while (wSection5.Body.ChildEntities.Count > num2)
				{
					wSection6.Body.ChildEntities.Add(wSection5.Body.ChildEntities[num2]);
				}
			}
		}
		UpdateBookmarks(ownerTextBody2, count);
		base.Document.ImportOptions = ImportOptions.UseDestinationStyles;
		base.Document.ImportStyles = importStyles;
		wordDocument.Close();
	}

	private void CheckPictureTextWrappingStyle(WTextBody textBody, Entity entity)
	{
		if (textBody.Owner == null || (textBody.Owner.EntityType != EntityType.TextBox && textBody.Owner.EntityType != EntityType.Shape) || entity == null || !(entity is WParagraph))
		{
			return;
		}
		foreach (ParagraphItem childEntity in (entity as WParagraph).ChildEntities)
		{
			if (childEntity is WPicture && !(childEntity as WPicture).IsShape && (childEntity as WPicture).TextWrappingStyle != 0)
			{
				(childEntity as WPicture).SetTextWrappingStyleValue(TextWrappingStyle.Inline);
			}
		}
	}

	private void MergeAltChunkFirstTable(ref int altchunkIndex, WTextBody ownerTextBody, WTextBody altChunkTextBody)
	{
		WTableCell wTableCell = ((altChunkTextBody.ChildEntities[0] as WTable).ChildEntities[0] as WTableRow).ChildEntities[0] as WTableCell;
		WParagraph wParagraph = ownerTextBody.ChildEntities[altchunkIndex - 1].Clone() as WParagraph;
		if (wTableCell.ChildEntities[0] is WParagraph && wParagraph != null)
		{
			for (int i = 0; i < wParagraph.ChildEntities.Count; i++)
			{
				(wTableCell.ChildEntities[0] as WParagraph).ChildEntities.Insert(i, wParagraph.ChildEntities[i]);
				(ownerTextBody.ChildEntities[altchunkIndex - 1] as WParagraph).ChildEntities[i].RemoveSelf();
			}
		}
		ownerTextBody.ChildEntities.Insert(altchunkIndex - 1, altChunkTextBody.ChildEntities[0].Clone());
		altchunkIndex--;
	}

	private void MergeAltChunkFirstParagraph(ref int altchunkIndex, WTextBody ownerTextBody, WTextBody altChunkTextBody)
	{
		WParagraph wParagraph = altChunkTextBody.ChildEntities[0].Clone() as WParagraph;
		WParagraph wParagraph2 = ownerTextBody.ChildEntities[altchunkIndex - 1].Clone() as WParagraph;
		(ownerTextBody.ChildEntities[altchunkIndex - 1] as WParagraph).ImportStyle(wParagraph.ParaStyle);
		wParagraph.ParagraphFormat.UpdateSourceFormatting((ownerTextBody.ChildEntities[altchunkIndex - 1] as WParagraph).ParagraphFormat);
		wParagraph.BreakCharacterFormat.UpdateSourceFormatting((ownerTextBody.ChildEntities[altchunkIndex - 1] as WParagraph).BreakCharacterFormat);
		int num;
		for (num = 0; num < wParagraph.ChildEntities.Count; num++)
		{
			(ownerTextBody.ChildEntities[altchunkIndex - 1] as WParagraph).ChildEntities.Add(wParagraph.ChildEntities[num]);
			num--;
		}
		wParagraph2.ClearItems();
		ownerTextBody.ChildEntities.Insert(altchunkIndex, wParagraph2);
		altchunkIndex--;
	}

	private void UpdateBookmarks(WTextBody body, int oldChildCount)
	{
		if (altChunkBookmarks != null && altChunkBookmarks.Count > 0)
		{
			foreach (Entity altChunkBookmark in altChunkBookmarks)
			{
				if (altChunkBookmark is BookmarkStart)
				{
					bkmkCollection.Add(altChunkBookmark);
				}
				else if (altChunkBookmark is BookmarkEnd && !(altChunkBookmark as BookmarkEnd).IsAfterParagraphMark)
				{
					foreach (Entity item3 in bkmkCollection)
					{
						if (item3 is BookmarkStart && (item3 as BookmarkStart).Name == (altChunkBookmark as BookmarkEnd).Name)
						{
							bkmkCollection.Remove(item3);
							break;
						}
					}
				}
				else
				{
					bkmkCollection.Add(altChunkBookmark);
				}
			}
			altChunkBookmarks.Clear();
		}
		if (bkmkCollection.Count <= 0)
		{
			return;
		}
		bool bkmkStartInserted = false;
		bool bkmkEndInserted = false;
		foreach (Entity item4 in bkmkCollection)
		{
			if (item4 is BookmarkStart)
			{
				for (int i = Index; i < body.ChildEntities.Count; i++)
				{
					Entity item = body.ChildEntities[i];
					InsertBkmkStart(item, item4 as BookmarkStart, ref bkmkStartInserted);
					if (bkmkStartInserted)
					{
						break;
					}
				}
				continue;
			}
			if (item4 is BookmarkEnd)
			{
				(item4 as BookmarkEnd).IsAfterParagraphMark = false;
			}
			int num = body.ChildEntities.Count - oldChildCount + (Index - 1);
			while (num < body.ChildEntities.Count && num >= 0)
			{
				Entity item2 = body.ChildEntities[num];
				InsertBkmkEnd(item2, item4 as BookmarkEnd, ref bkmkEndInserted);
				if (bkmkEndInserted)
				{
					break;
				}
				num--;
			}
		}
		bkmkCollection.Clear();
	}

	private void InsertBkmkStart(Entity item, BookmarkStart bookmark, ref bool bkmkStartInserted)
	{
		if (item is WParagraph)
		{
			WParagraph obj = item as WParagraph;
			bookmark.Index = 0;
			obj.Items.Insert(0, bookmark);
			bkmkStartInserted = true;
		}
		else
		{
			if (!(item is WTable))
			{
				return;
			}
			foreach (Entity childEntity in (item as WTable).Rows[0].Cells[0].ChildEntities)
			{
				InsertBkmkStart(childEntity, bookmark, ref bkmkStartInserted);
			}
		}
	}

	private void InsertBkmkEnd(Entity item, BookmarkEnd bookmark, ref bool bkmkEndInserted)
	{
		if (item is WParagraph)
		{
			(item as WParagraph).Items.Add(bookmark);
			bkmkEndInserted = true;
		}
		else if (item is WTable)
		{
			WTableCell lastCell = (item as WTable).LastCell;
			InsertBkmkEnd(lastCell.LastParagraph as Entity, bookmark, ref bkmkEndInserted);
		}
	}

	private void ClearExistingHeadersFooters(WSection section)
	{
		section.HeadersFooters[HeaderFooterType.FirstPageHeader].ChildEntities.Clear();
		section.HeadersFooters[HeaderFooterType.FirstPageFooter].ChildEntities.Clear();
		section.HeadersFooters[HeaderFooterType.OddHeader].ChildEntities.Clear();
		section.HeadersFooters[HeaderFooterType.OddFooter].ChildEntities.Clear();
		section.HeadersFooters[HeaderFooterType.EvenHeader].ChildEntities.Clear();
		section.HeadersFooters[HeaderFooterType.EvenFooter].ChildEntities.Clear();
	}

	private void UpdateHeaderFooter(WSection altChunkFirstSection, WTextBody ownerTextBody, WSection section)
	{
		WTextBody wTextBody = null;
		if (altChunkFirstSection.HeadersFooters.Header != null)
		{
			wTextBody = altChunkFirstSection.HeadersFooters.Header;
		}
		if (wTextBody != null && wTextBody.ChildEntities.Count > 0)
		{
			if (section != null && section.PreviousHeaderCount != 0)
			{
				while (section.HeadersFooters.Header.ChildEntities.Count > section.PreviousHeaderCount)
				{
					section.HeadersFooters.Header.ChildEntities.RemoveAt(section.PreviousHeaderCount);
				}
				section.PreviousHeaderCount = 0;
			}
			else
			{
				section.PreviousHeaderCount = (short)section.HeadersFooters.Header.ChildEntities.Count;
			}
			if (section.HeadersFooters.Header.ChildEntities.Count > 0)
			{
				section.HeadersFooters.Header.AddParagraph();
			}
			for (int i = 0; i < wTextBody.ChildEntities.Count; i++)
			{
				Entity entity = wTextBody.ChildEntities[i].Clone();
				section.HeadersFooters.Header.ChildEntities.Add(entity);
			}
			if ((wTextBody as HeaderFooter).Watermark.Type != 0 && section.HeadersFooters.Header.Watermark.Type == WatermarkType.NoWatermark)
			{
				if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.TextWatermark)
				{
					section.HeadersFooters.Header.Watermark = (TextWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
				else if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.PictureWatermark)
				{
					section.HeadersFooters.Header.Watermark = (PictureWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
				else
				{
					section.HeadersFooters.Header.Watermark = (Watermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
			}
			wTextBody = null;
		}
		if (altChunkFirstSection.HeadersFooters.FirstPageHeader != null)
		{
			wTextBody = altChunkFirstSection.HeadersFooters.FirstPageHeader;
		}
		if (wTextBody != null && wTextBody.ChildEntities.Count > 0)
		{
			if (section.HeadersFooters.FirstPageHeader.ChildEntities.Count > 0)
			{
				section.HeadersFooters.FirstPageHeader.AddParagraph();
			}
			for (int j = 0; j < wTextBody.ChildEntities.Count; j++)
			{
				Entity entity2 = wTextBody.ChildEntities[j].Clone();
				section.HeadersFooters.FirstPageHeader.ChildEntities.Add(entity2);
			}
			if ((wTextBody as HeaderFooter).Watermark.Type != 0 && section.HeadersFooters.FirstPageHeader.Watermark.Type == WatermarkType.NoWatermark)
			{
				if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.TextWatermark)
				{
					section.HeadersFooters.FirstPageHeader.Watermark = (TextWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
				else if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.PictureWatermark)
				{
					section.HeadersFooters.FirstPageHeader.Watermark = (PictureWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
				else
				{
					section.HeadersFooters.FirstPageHeader.Watermark = (Watermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
			}
			wTextBody = null;
		}
		if (altChunkFirstSection.HeadersFooters.EvenHeader != null)
		{
			wTextBody = altChunkFirstSection.HeadersFooters.EvenHeader;
		}
		if (wTextBody != null && wTextBody.ChildEntities.Count > 0)
		{
			if (section.HeadersFooters.EvenHeader.ChildEntities.Count > 0)
			{
				section.HeadersFooters.EvenHeader.AddParagraph();
			}
			for (int k = 0; k < wTextBody.ChildEntities.Count; k++)
			{
				Entity entity3 = wTextBody.ChildEntities[k].Clone();
				section.HeadersFooters.EvenHeader.ChildEntities.Add(entity3);
			}
			if ((wTextBody as HeaderFooter).Watermark.Type != 0 && section.HeadersFooters.EvenHeader.Watermark.Type == WatermarkType.NoWatermark)
			{
				if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.TextWatermark)
				{
					section.HeadersFooters.EvenHeader.Watermark = (TextWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
				else if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.PictureWatermark)
				{
					section.HeadersFooters.EvenHeader.Watermark = (PictureWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
				else
				{
					section.HeadersFooters.EvenHeader.Watermark = (Watermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
			}
			wTextBody = null;
		}
		if (altChunkFirstSection.HeadersFooters.Footer != null)
		{
			wTextBody = altChunkFirstSection.HeadersFooters.Footer;
		}
		if (wTextBody != null && wTextBody.ChildEntities.Count > 0)
		{
			if (section != null && section.PreviousFooterCount != 0)
			{
				while (section.HeadersFooters.Footer.ChildEntities.Count > section.PreviousFooterCount)
				{
					section.HeadersFooters.Footer.ChildEntities.RemoveAt(section.PreviousFooterCount);
				}
				section.PreviousFooterCount = 0;
			}
			else
			{
				section.PreviousFooterCount = (short)section.HeadersFooters.Footer.ChildEntities.Count;
			}
			if (section.HeadersFooters.Footer.ChildEntities.Count > 0)
			{
				section.HeadersFooters.Footer.AddParagraph();
			}
			for (int l = 0; l < wTextBody.ChildEntities.Count; l++)
			{
				Entity entity4 = wTextBody.ChildEntities[l].Clone();
				section.HeadersFooters.Footer.ChildEntities.Add(entity4);
			}
			if ((wTextBody as HeaderFooter).Watermark.Type != 0 && section.HeadersFooters.Footer.Watermark.Type == WatermarkType.NoWatermark)
			{
				if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.TextWatermark)
				{
					section.HeadersFooters.Footer.Watermark = (TextWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
				else if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.PictureWatermark)
				{
					section.HeadersFooters.Footer.Watermark = (PictureWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
				else
				{
					section.HeadersFooters.Footer.Watermark = (Watermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
			}
			wTextBody = null;
		}
		if (altChunkFirstSection.HeadersFooters.FirstPageFooter != null)
		{
			wTextBody = altChunkFirstSection.HeadersFooters.FirstPageFooter;
		}
		if (wTextBody != null && wTextBody.ChildEntities.Count > 0)
		{
			if (section.HeadersFooters.FirstPageFooter.ChildEntities.Count > 0)
			{
				section.HeadersFooters.FirstPageFooter.AddParagraph();
			}
			for (int m = 0; m < wTextBody.ChildEntities.Count; m++)
			{
				Entity entity5 = wTextBody.ChildEntities[m].Clone();
				section.HeadersFooters.FirstPageFooter.ChildEntities.Add(entity5);
			}
			if ((wTextBody as HeaderFooter).Watermark.Type != 0 && section.HeadersFooters.FirstPageFooter.Watermark.Type == WatermarkType.NoWatermark)
			{
				if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.TextWatermark)
				{
					section.HeadersFooters.FirstPageFooter.Watermark = (TextWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
				else if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.PictureWatermark)
				{
					section.HeadersFooters.FirstPageFooter.Watermark = (PictureWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
				else
				{
					section.HeadersFooters.FirstPageFooter.Watermark = (Watermark)(wTextBody as HeaderFooter).Watermark.Clone();
				}
			}
			wTextBody = null;
		}
		if (altChunkFirstSection.HeadersFooters.EvenFooter != null)
		{
			wTextBody = altChunkFirstSection.HeadersFooters.EvenFooter;
		}
		if (wTextBody == null || wTextBody.ChildEntities.Count <= 0)
		{
			return;
		}
		if (section.HeadersFooters.EvenFooter.ChildEntities.Count > 0)
		{
			section.HeadersFooters.EvenFooter.AddParagraph();
		}
		for (int n = 0; n < wTextBody.ChildEntities.Count; n++)
		{
			Entity entity6 = wTextBody.ChildEntities[n].Clone();
			section.HeadersFooters.EvenFooter.ChildEntities.Add(entity6);
		}
		if ((wTextBody as HeaderFooter).Watermark.Type != 0 && section.HeadersFooters.EvenFooter.Watermark.Type == WatermarkType.NoWatermark)
		{
			if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.TextWatermark)
			{
				section.HeadersFooters.EvenFooter.Watermark = (TextWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
			}
			else if ((wTextBody as HeaderFooter).Watermark.Type == WatermarkType.PictureWatermark)
			{
				section.HeadersFooters.EvenFooter.Watermark = (PictureWatermark)(wTextBody as HeaderFooter).Watermark.Clone();
			}
			else
			{
				section.HeadersFooters.EvenFooter.Watermark = (Watermark)(wTextBody as HeaderFooter).Watermark.Clone();
			}
		}
		wTextBody = null;
	}

	internal override bool CheckDeleteRev()
	{
		return false;
	}

	internal override void SetChangedPFormat(bool check)
	{
	}

	internal override void SetChangedCFormat(bool check)
	{
	}

	internal override void SetDeleteRev(bool check)
	{
	}

	internal override void SetInsertRev(bool check)
	{
	}

	internal override bool HasTrackedChanges()
	{
		return false;
	}

	public override int Replace(Regex pattern, string replace)
	{
		return 1;
	}

	public override int Replace(string given, string replace, bool caseSensitive, bool wholeWord)
	{
		return 0;
	}

	public override int Replace(Regex pattern, TextSelection textSelection)
	{
		return 0;
	}

	public override int Replace(Regex pattern, TextSelection textSelection, bool saveFormatting)
	{
		return 0;
	}

	public int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord)
	{
		return 0;
	}

	public int Replace(string given, TextSelection textSelection, bool caseSensitive, bool wholeWord, bool saveFormatting)
	{
		return 0;
	}

	internal int ReplaceFirst(string given, string replace, bool caseSensitive, bool wholeWord)
	{
		return 0;
	}

	internal int ReplaceFirst(Regex pattern, string replace)
	{
		return 0;
	}

	protected override void CreateLayoutInfo()
	{
		m_layoutInfo = new LayoutInfo();
	}

	internal override void InitLayoutInfo(Entity entity, ref bool isLastTOCEntry)
	{
		m_layoutInfo = null;
		if (this == entity)
		{
			isLastTOCEntry = true;
		}
	}

	internal override void RemoveCFormatChanges()
	{
	}

	internal override void RemovePFormatChanges()
	{
	}

	internal override void AcceptCChanges()
	{
	}

	internal override void AcceptPChanges()
	{
	}

	internal override bool CheckChangedCFormat()
	{
		return false;
	}

	internal override bool CheckInsertRev()
	{
		return false;
	}

	public override TextSelection Find(Regex pattern)
	{
		return null;
	}

	public TextSelection Find(string given, bool caseSensitive, bool wholeWord)
	{
		return null;
	}

	internal override void MakeChanges(bool acceptChanges)
	{
	}

	internal override TextSelectionList FindAll(Regex pattern, bool isDocumentComparison)
	{
		return null;
	}

	internal override void Close()
	{
		m_targetId = null;
		m_contentPath = null;
		m_contentType = null;
		firstChunkLastSection = null;
		base.Close();
	}

	internal override bool CheckChangedPFormat()
	{
		return false;
	}

	internal override void AddDelMark()
	{
	}

	internal override void AddInsMark()
	{
	}
}
