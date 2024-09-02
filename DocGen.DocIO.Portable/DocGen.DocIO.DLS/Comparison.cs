using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DocGen.DocIO.DLS;

internal class Comparison
{
	private WordDocument Document;

	private Dictionary<int, string> m_imagesHash;

	private HashAlgorithm m_hashAlgorithm;

	private List<BlockContentControl> m_blockContentControls;

	private List<InlineContentControl> m_inlineContentControls;

	private List<Shape> m_shapes;

	private List<GroupShape> m_groupShapes;

	private List<WPicture> m_pictures;

	private List<WTextBox> m_textBoxes;

	private List<WChart> m_charts;

	private List<WField> m_fields;

	private int bodyItemIndex;

	private int paraItemIndex;

	private bool m_isComparedImages;

	private List<WTable> m_tables;

	private List<WMath> m_maths;

	private List<WOleObject> m_oles;

	private List<TableOfContent> m_tocs;

	internal bool IsComparingMatchedCells;

	private List<string> m_revisedDocListStyles;

	internal Dictionary<int, string> ImagesHash
	{
		get
		{
			if (m_imagesHash == null)
			{
				m_imagesHash = new Dictionary<int, string>();
			}
			return m_imagesHash;
		}
	}

	internal HashAlgorithm HashAlgorithm
	{
		get
		{
			m_hashAlgorithm = new SHA512Managed();
			return m_hashAlgorithm;
		}
	}

	internal List<BlockContentControl> BlockContentControls
	{
		get
		{
			if (m_blockContentControls == null)
			{
				m_blockContentControls = new List<BlockContentControl>();
			}
			return m_blockContentControls;
		}
	}

	internal List<InlineContentControl> InlineContentControls
	{
		get
		{
			if (m_inlineContentControls == null)
			{
				m_inlineContentControls = new List<InlineContentControl>();
			}
			return m_inlineContentControls;
		}
	}

	internal List<Shape> Shapes
	{
		get
		{
			if (m_shapes == null)
			{
				m_shapes = new List<Shape>();
			}
			return m_shapes;
		}
	}

	internal List<GroupShape> GroupShapes
	{
		get
		{
			if (m_groupShapes == null)
			{
				m_groupShapes = new List<GroupShape>();
			}
			return m_groupShapes;
		}
	}

	internal List<WPicture> Pictures
	{
		get
		{
			if (m_pictures == null)
			{
				m_pictures = new List<WPicture>();
			}
			return m_pictures;
		}
	}

	internal List<WTextBox> TextBoxes
	{
		get
		{
			if (m_textBoxes == null)
			{
				m_textBoxes = new List<WTextBox>();
			}
			return m_textBoxes;
		}
	}

	internal List<WChart> Charts
	{
		get
		{
			if (m_charts == null)
			{
				m_charts = new List<WChart>();
			}
			return m_charts;
		}
	}

	internal List<WField> Fields
	{
		get
		{
			if (m_fields == null)
			{
				m_fields = new List<WField>();
			}
			return m_fields;
		}
	}

	internal List<WMath> Maths
	{
		get
		{
			if (m_maths == null)
			{
				m_maths = new List<WMath>();
			}
			return m_maths;
		}
	}

	internal List<WOleObject> OLEs
	{
		get
		{
			if (m_oles == null)
			{
				m_oles = new List<WOleObject>();
			}
			return m_oles;
		}
	}

	internal List<TableOfContent> TOCs
	{
		get
		{
			if (m_tocs == null)
			{
				m_tocs = new List<TableOfContent>();
			}
			return m_tocs;
		}
	}

	internal bool IsComparedImages
	{
		get
		{
			return m_isComparedImages;
		}
		set
		{
			m_isComparedImages = value;
		}
	}

	internal List<WTable> Tables
	{
		get
		{
			if (m_tables == null)
			{
				m_tables = new List<WTable>();
			}
			return m_tables;
		}
	}

	internal List<string> RevisedDocListStyles
	{
		get
		{
			if (m_revisedDocListStyles == null)
			{
				m_revisedDocListStyles = new List<string>();
			}
			return m_revisedDocListStyles;
		}
	}

	internal Comparison(WordDocument wordDocument)
	{
		Document = wordDocument;
	}

	internal string GetImageHash(WPicture image)
	{
		int imageId = image.ImageRecord.ImageId;
		string empty = string.Empty;
		if (!ImagesHash.ContainsKey(imageId))
		{
			empty = ConvertBytesAsHash(image.ImageBytes);
			ImagesHash.Add(imageId, empty);
		}
		else
		{
			empty = ImagesHash[imageId];
		}
		return empty;
	}

	internal string ConvertBytesAsHash(byte[] bytes)
	{
		_ = string.Empty;
		byte[] array = HashAlgorithm.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	internal void CompareImagesInDoc(WordDocument orginalDocument)
	{
		if (IsComparedImages)
		{
			return;
		}
		ImageCollection images = orginalDocument.Images;
		ImageCollection images2 = Document.Images;
		List<int> list = new List<int>();
		foreach (int key in images2.m_collection.Keys)
		{
			ImageRecord imageRecord = images2.m_collection[key];
			foreach (int key2 in images.m_collection.Keys)
			{
				if (!list.Contains(key2))
				{
					ImageRecord imageRecord2 = images.m_collection[key2];
					if (CompareBytes(imageRecord2.ImageBytes, imageRecord.ImageBytes))
					{
						imageRecord.comparedImageName = (imageRecord2.comparedImageName = "Org" + imageRecord2.ImageId + "Rev" + imageRecord.ImageId);
						list.Add(imageRecord2.ImageId);
					}
				}
			}
		}
		IsComparedImages = true;
		orginalDocument.Comparison.IsComparedImages = true;
	}

	internal static byte[] ConvertStreamToBytes(Stream input1)
	{
		using MemoryStream memoryStream = new MemoryStream();
		input1.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}

	internal static bool CompareStream(Stream input1, Stream input2)
	{
		byte[] input3 = ConvertStreamToBytes(input1);
		byte[] input4 = ConvertStreamToBytes(input2);
		return CompareBytes(input3, input4);
	}

	internal static bool CompareBytes(byte[] input1, byte[] input2)
	{
		if (input1 == null || input2 == null)
		{
			return false;
		}
		if (input1.Length != input2.Length)
		{
			return false;
		}
		for (int i = 0; i < input1.Length; i++)
		{
			if (input1[i] != input2[i])
			{
				return false;
			}
		}
		return true;
	}

	internal StringBuilder GetAsString(WordDocument document)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (WSection section in document.Sections)
		{
			WTextBody body = section.Body;
			stringBuilder.Append(body.GetAsString());
		}
		return stringBuilder;
	}

	internal void AddComparisonCollection(WordDocument document)
	{
		foreach (WSection section in document.Sections)
		{
			WTextBody body = section.Body;
			AddComparisonCollection(body);
		}
	}

	internal void AddComparisonCollection(WTextBody textBody, int startBodyItemIndex = 0, int startParaItemIndex = 0)
	{
		for (bodyItemIndex = startBodyItemIndex; bodyItemIndex < textBody.ChildEntities.Count; bodyItemIndex++)
		{
			IEntity entity = textBody.ChildEntities[bodyItemIndex];
			switch (entity.EntityType)
			{
			case EntityType.Paragraph:
			{
				WParagraph wParagraph = entity as WParagraph;
				AddComparisonCollection(wParagraph.Items, startParaItemIndex);
				break;
			}
			case EntityType.Table:
			{
				WTable item2 = entity as WTable;
				Tables.Add(item2);
				break;
			}
			case EntityType.BlockContentControl:
			{
				BlockContentControl item = entity as BlockContentControl;
				BlockContentControls.Add(item);
				break;
			}
			}
		}
	}

	private void AddComparisonCollection(ParagraphItemCollection paraItems, int startIndex = 0)
	{
		for (int i = startIndex; i < paraItems.Count; i++)
		{
			Entity entity = paraItems[i];
			switch (entity.EntityType)
			{
			case EntityType.InlineContentControl:
			{
				InlineContentControl inlineContentControl = entity as InlineContentControl;
				InlineContentControls.Add(inlineContentControl);
				AddComparisonCollection(inlineContentControl.ParagraphItems);
				break;
			}
			case EntityType.Shape:
			case EntityType.AutoShape:
			{
				Shape item9 = entity as Shape;
				Shapes.Add(item9);
				break;
			}
			case EntityType.GroupShape:
			{
				GroupShape item8 = entity as GroupShape;
				GroupShapes.Add(item8);
				break;
			}
			case EntityType.Picture:
			{
				WPicture item7 = entity as WPicture;
				Pictures.Add(item7);
				break;
			}
			case EntityType.TextBox:
			{
				WTextBox item6 = entity as WTextBox;
				TextBoxes.Add(item6);
				break;
			}
			case EntityType.Chart:
			{
				WChart item5 = entity as WChart;
				Charts.Add(item5);
				break;
			}
			case EntityType.Field:
			case EntityType.MergeField:
			case EntityType.SeqField:
			case EntityType.EmbededField:
			case EntityType.ControlField:
			case EntityType.TextFormField:
			case EntityType.DropDownFormField:
			case EntityType.CheckBox:
			{
				WField item4 = entity as WField;
				Fields.Add(item4);
				i = TraverseTillFieldEnd(entity as ParagraphItem);
				break;
			}
			case EntityType.Math:
			{
				WMath item3 = entity as WMath;
				Maths.Add(item3);
				break;
			}
			case EntityType.OleObject:
			{
				WOleObject item2 = entity as WOleObject;
				OLEs.Add(item2);
				i = TraverseTillFieldEnd(entity as ParagraphItem);
				break;
			}
			case EntityType.TOC:
			{
				TableOfContent item = entity as TableOfContent;
				TOCs.Add(item);
				i = TraverseTillFieldEnd(entity as ParagraphItem);
				break;
			}
			}
		}
	}

	private int TraverseTillFieldEnd(ParagraphItem paraItem)
	{
		WField wField = ((paraItem is TableOfContent) ? (paraItem as TableOfContent).TOCField : ((paraItem is WOleObject) ? (paraItem as WOleObject).Field : (paraItem as WField)));
		WParagraph ownerParagraph = paraItem.OwnerParagraph;
		WParagraph ownerParagraph2 = wField.FieldEnd.OwnerParagraph;
		if (ownerParagraph == ownerParagraph2)
		{
			return wField.FieldEnd.Index;
		}
		WTextBody ownerTextBody = ownerParagraph2.OwnerTextBody;
		if (ownerTextBody != null)
		{
			AddComparisonCollection(ownerTextBody, ownerParagraph2.Index, wField.FieldEnd.Index + 1);
			return ownerParagraph.Items.Count;
		}
		return paraItem.Index;
	}

	internal void RemoveFromDocCollection(Entity entity)
	{
		switch (entity.EntityType)
		{
		case EntityType.InlineContentControl:
		{
			InlineContentControl item8 = entity as InlineContentControl;
			if (InlineContentControls.Contains(item8))
			{
				InlineContentControls.Remove(item8);
			}
			break;
		}
		case EntityType.BlockContentControl:
		{
			BlockContentControl item12 = entity as BlockContentControl;
			if (BlockContentControls.Contains(item12))
			{
				BlockContentControls.Remove(item12);
			}
			break;
		}
		case EntityType.Shape:
		case EntityType.AutoShape:
		{
			Shape item4 = entity as Shape;
			if (Shapes.Contains(item4))
			{
				Shapes.Remove(item4);
			}
			break;
		}
		case EntityType.GroupShape:
		{
			GroupShape item10 = entity as GroupShape;
			if (GroupShapes.Contains(item10))
			{
				GroupShapes.Remove(item10);
			}
			break;
		}
		case EntityType.Picture:
		{
			WPicture item6 = entity as WPicture;
			if (Pictures.Contains(item6))
			{
				Pictures.Remove(item6);
			}
			break;
		}
		case EntityType.TextBox:
		{
			WTextBox item2 = entity as WTextBox;
			if (TextBoxes.Contains(item2))
			{
				TextBoxes.Remove(item2);
			}
			break;
		}
		case EntityType.Chart:
		{
			WChart item11 = entity as WChart;
			if (Charts.Contains(item11))
			{
				Charts.Remove(item11);
			}
			break;
		}
		case EntityType.Field:
		{
			WField item9 = entity as WField;
			if (Fields.Contains(item9))
			{
				Fields.Remove(item9);
			}
			break;
		}
		case EntityType.Table:
		{
			WTable item7 = entity as WTable;
			if (Tables.Contains(item7))
			{
				Tables.Remove(item7);
			}
			break;
		}
		case EntityType.Math:
		{
			WMath item5 = entity as WMath;
			if (Maths.Contains(item5))
			{
				Maths.Remove(item5);
			}
			break;
		}
		case EntityType.OleObject:
		{
			WOleObject item3 = entity as WOleObject;
			if (OLEs.Contains(item3))
			{
				OLEs.Remove(item3);
			}
			break;
		}
		case EntityType.TOC:
		{
			TableOfContent item = entity as TableOfContent;
			if (TOCs.Contains(item))
			{
				TOCs.Remove(item);
			}
			break;
		}
		}
	}

	internal void Insertion(WordDocument originalDocument, int currRevParaItemIndex, int revBodyItemIndex, int currRevSecIndex, int orgCurrMatchParaItemIndex, int orgCurrMatchBodyItemIndex, int orgCurrMatchSecIndex, WTextBody orgTextBody = null, WTextBody revTextBody = null)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = 0;
		WTextBody wTextBody = null;
		WTextBody wTextBody2 = null;
		WTextBody wTextBody3 = null;
		WordDocument wordDocument = null;
		WParagraph wParagraph = null;
		bool flag = false;
		if (orgTextBody == null)
		{
			num = Document.m_bodyItemIndex;
			num2 = Document.m_paraItemIndex;
			num3 = originalDocument.m_bodyItemIndex;
			_ = originalDocument.m_paraItemIndex;
			num4 = originalDocument.m_sectionIndex;
			wTextBody2 = Document.Sections[currRevSecIndex].Body;
			wTextBody = Document.Sections[Document.m_sectionIndex].Body;
			wTextBody3 = originalDocument.Sections[orgCurrMatchSecIndex].Body;
			wordDocument = originalDocument;
			if (Document.m_sectionIndex != currRevSecIndex)
			{
				flag = true;
			}
			if (num >= wTextBody.Items.Count)
			{
				InsertSectAtOrgDocument(originalDocument, Document, orgCurrMatchSecIndex, orgCurrMatchBodyItemIndex, orgCurrMatchParaItemIndex);
				return;
			}
		}
		else
		{
			num = revTextBody.m_bodyItemIndex;
			num2 = revTextBody.m_paraItemIndex;
			num3 = orgTextBody.m_bodyItemIndex;
			_ = orgTextBody.m_paraItemIndex;
			wTextBody = (wTextBody2 = revTextBody);
			wTextBody3 = orgTextBody;
			wordDocument = orgTextBody.Document;
		}
		Entity entity = wTextBody.Items[num];
		Entity entity2 = ((revBodyItemIndex == wTextBody2.Items.Count) ? null : wTextBody2.Items[revBodyItemIndex]);
		WordDocumentPart wordDocumentPart = null;
		WParagraph wParagraph2 = null;
		WParagraph wParagraph3 = null;
		BookmarkEnd bookmarkEnd = null;
		List<TextBodyItem> list = new List<TextBodyItem>();
		while (entity is BlockContentControl || entity is WTable)
		{
			list.Add(entity.Clone() as TextBodyItem);
			num++;
			entity = entity.NextSibling as Entity;
			if (entity == entity2)
			{
				break;
			}
		}
		if (entity == entity2 && list.Count > 0 && (entity2 == null || entity2 is BlockContentControl || entity2 is WTable || (entity2 is WParagraph && currRevParaItemIndex == 0)))
		{
			wordDocumentPart = new WordDocumentPart();
			wordDocumentPart.Sections.Add(new WSection(Document));
			AddBlockContentControInDocumentPart(wordDocumentPart, list, null);
		}
		else
		{
			wParagraph2 = Document.GetOwnerParagraphToInsertBookmark(entity, isStart: true);
			List<TextBodyItem> list2 = new List<TextBodyItem>();
			if (entity2 == null)
			{
				entity2 = wTextBody2.Items[wTextBody2.Items.Count - 1];
			}
			else if (entity2 is BlockContentControl || entity2 is WTable)
			{
				entity2 = entity2.PreviousSibling as Entity;
			}
			if (num2 > 0 && num3 < orgCurrMatchBodyItemIndex && num4 == orgCurrMatchSecIndex && !flag && HasRenderableItemsBefore(wParagraph2, num2 - 1))
			{
				wParagraph2 = wParagraph2.AppendTextToParagraphEnd(originalDocument, wParagraph2, orgTextBody);
				if (entity == entity2)
				{
					return;
				}
				num = wParagraph2.Index;
				num2 = 0;
			}
			BookmarkStart bookmarkStart = new BookmarkStart(Document, "Insertion");
			wParagraph2.Items.Insert(num2, bookmarkStart);
			while (entity2 is BlockContentControl || entity2 is WTable)
			{
				list2.Add(entity2.Clone() as TextBodyItem);
				entity2 = entity2.PreviousSibling as Entity;
			}
			wParagraph3 = Document.GetOwnerParagraphToInsertBookmark(entity2, isStart: false);
			if (wParagraph3 == wParagraph2)
			{
				currRevParaItemIndex++;
				if (num != revBodyItemIndex)
				{
					currRevParaItemIndex = wParagraph3.Items.Count;
				}
			}
			else if (currRevParaItemIndex == -1)
			{
				currRevParaItemIndex = wParagraph3.Items.Count;
			}
			bookmarkEnd = new BookmarkEnd(Document, bookmarkStart.Name);
			wParagraph3.Items.Insert(currRevParaItemIndex, bookmarkEnd);
			BookmarksNavigator bookmarksNavigator = new BookmarksNavigator(Document);
			bookmarksNavigator.MoveToBookmark(bookmarkStart.Name);
			Document.IsComparing = true;
			wordDocumentPart = bookmarksNavigator.GetContent();
			Document.IsComparing = false;
			AddBlockContentControInDocumentPart(wordDocumentPart, list, list2);
		}
		Document.MarkInsertRevision(wordDocumentPart);
		if (bookmarkEnd != null)
		{
			int bkmkEndPreviosItemIndex = bookmarkEnd.GetIndexInOwnerCollection() - 1;
			if (IsBkmkEndInFirstItem(wParagraph3, bookmarkEnd, bkmkEndPreviosItemIndex, list.Count > 0) && (entity2 != wTextBody2.Items[revBodyItemIndex] || (!(wParagraph3.PreviousSibling is WTable) && !(wParagraph3.PreviousSibling is BlockContentControl))))
			{
				wParagraph = new WParagraph(Document);
				wordDocumentPart.Sections[wordDocumentPart.Sections.Count - 1].Body.Items.Add(wParagraph);
			}
			Document.Bookmarks.Remove(Document.Bookmarks.FindByName("Insertion"));
			if (wordDocumentPart.Sections[wordDocumentPart.Sections.Count - 1].Body.LastParagraph is WParagraph wParagraph4 && wParagraph4.Items.Count == 0 && wParagraph3.Items.Count > 0)
			{
				wParagraph4.RemoveEntityRevision(isNeedToRemoveFormatRev: false);
				wParagraph4.BreakCharacterFormat.IsInsertRevision = false;
				wParagraph4.BreakCharacterFormat.FormatChangeAuthorName = string.Empty;
				wParagraph4.BreakCharacterFormat.FormatChangeDateTime = DateTime.MinValue;
			}
		}
		if (wordDocumentPart.Sections.Count == 0 || (wordDocumentPart.Sections.Count == 1 && wordDocumentPart.Sections[0].Body.ChildEntities.Count == 0))
		{
			return;
		}
		if (wordDocumentPart.Sections.Count != 1 && wordDocumentPart.Sections[0].Body.ChildEntities.Count == 0)
		{
			WParagraph wParagraph5 = new WParagraph(Document);
			wordDocumentPart.Sections[0].Body.ChildEntities.Add(wParagraph5);
			wParagraph5.AddInsMark();
		}
		Entity entity3 = null;
		WParagraph wParagraph6 = null;
		bool flag2 = false;
		int num5 = orgCurrMatchBodyItemIndex;
		bool flag3 = false;
		if (orgCurrMatchBodyItemIndex == wTextBody3.Items.Count)
		{
			flag3 = true;
			Document.IsComparing = true;
			wordDocument.IsComparing = true;
			wordDocument.UpdateRevisionOnComparing = true;
			wTextBody3.Items.Insert(orgCurrMatchBodyItemIndex, wordDocumentPart.Sections[0].Body.Items[0].Clone());
			wordDocument.UpdateRevisionOnComparing = false;
			if (wordDocumentPart.Sections.Count == 1 && wordDocumentPart.Sections[0].Body.Items.Count == 1)
			{
				if (wTextBody3.Items[orgCurrMatchBodyItemIndex] is WParagraph wParagraph7)
				{
					wParagraph7.BreakCharacterFormat.IsInsertRevision = false;
					wParagraph7.RemoveFormatRevision(wParagraph7.BreakCharacterFormat);
				}
				return;
			}
			if (wordDocumentPart.Sections[0].Body.Items.Count > 1 && wordDocumentPart.Sections[0].Body.Items[0] is WParagraph && wordDocumentPart.Sections[0].Body.Items[1] is WParagraph)
			{
				(wordDocumentPart.Sections[0].Body.Items[0] as WParagraph).Items.Clear();
				(wordDocumentPart.Sections[0].Body.Items[0] as WParagraph).ParagraphFormat.ClearFormatting();
			}
			else
			{
				wordDocumentPart.Sections[0].Body.Items[0].RemoveSelf();
			}
			num5++;
			Document.IsComparing = false;
			wordDocument.IsComparing = false;
		}
		entity3 = wTextBody3.Items[orgCurrMatchBodyItemIndex];
		if (entity3 is BlockContentControl || entity3 is WTable)
		{
			WParagraph wParagraph8 = new WParagraph(wordDocument);
			wTextBody3.Items.Insert(num5, wParagraph8);
			entity3 = wParagraph8;
			flag2 = true;
		}
		wParagraph6 = Document.GetOwnerParagraphToInsertBookmark(entity3, isStart: true);
		if (flag3)
		{
			orgCurrMatchParaItemIndex = wParagraph6.Items.Count;
		}
		BookmarkStart bookmarkStart2 = new BookmarkStart(wordDocument, "OriginalDocument");
		wParagraph6.Items.Insert(orgCurrMatchParaItemIndex, bookmarkStart2);
		BookmarkEnd entity4 = new BookmarkEnd(wordDocument, bookmarkStart2.Name);
		wParagraph6.Items.Insert(orgCurrMatchParaItemIndex + 1, entity4);
		BookmarksNavigator bookmarksNavigator2 = new BookmarksNavigator(wordDocument);
		bookmarksNavigator2.MoveToBookmark(bookmarkStart2.Name);
		if (wParagraph != null && wParagraph == wordDocumentPart.Sections[wordDocumentPart.Sections.Count - 1].Body.LastParagraph && wParagraph6.HasRenderableItemFromIndex(orgCurrMatchParaItemIndex + 1))
		{
			wParagraph6.ParagraphFormat.UpdateSourceFormatting(wParagraph.ParagraphFormat);
			wParagraph.ListFormat.ImportListFormat(wParagraph6.ListFormat);
			wParagraph6.BreakCharacterFormat.UpdateSourceFormatting(wParagraph.BreakCharacterFormat);
		}
		Document.IsComparing = true;
		wordDocument.IsComparing = true;
		wordDocument.UpdateRevisionOnComparing = true;
		bookmarksNavigator2.ReplaceContent(wordDocumentPart);
		wordDocument.UpdateRevisionOnComparing = false;
		Bookmark currentBookmark = bookmarksNavigator2.CurrentBookmark;
		WParagraph ownerParagraph = currentBookmark.BookmarkStart.OwnerParagraph;
		WParagraph ownerParagraph2 = currentBookmark.BookmarkEnd.OwnerParagraph;
		wordDocument.Bookmarks.Remove(wordDocument.Bookmarks.FindByName("OriginalDocument"));
		Entity entity5 = wordDocumentPart.Sections[0].Body.Items[0];
		if (flag2)
		{
			if (ownerParagraph.ChildEntities.Count == 0)
			{
				ownerParagraph.RemoveSelf();
			}
			else
			{
				ownerParagraph.BreakCharacterFormat.IsInsertRevision = true;
				ownerParagraph.BreakCharacterFormat.AuthorName = Document.m_authorName;
				ownerParagraph.BreakCharacterFormat.RevDateTime = Document.m_dateTime;
				ownerParagraph.UpdateParagraphRevision(ownerParagraph, isIncludeParaItems: false);
			}
			if (ownerParagraph2.ChildEntities.Count == 0)
			{
				ownerParagraph2.RemoveSelf();
			}
			else
			{
				ownerParagraph2.BreakCharacterFormat.IsInsertRevision = true;
				ownerParagraph2.BreakCharacterFormat.AuthorName = Document.m_authorName;
				ownerParagraph2.BreakCharacterFormat.RevDateTime = Document.m_dateTime;
				ownerParagraph2.UpdateParagraphRevision(ownerParagraph, isIncludeParaItems: false);
			}
		}
		else if (orgCurrMatchParaItemIndex == 0 && !ownerParagraph.IsInCell && (entity5 is BlockContentControl || entity5 is WTable))
		{
			ownerParagraph.RemoveSelf();
		}
		Document.IsComparing = false;
		wordDocument.IsComparing = false;
	}

	private bool IsBkmkEndInFirstItem(WParagraph paragraph, ParagraphItem bkmkEnd, int bkmkEndPreviosItemIndex, bool isItemsBefore)
	{
		int num = 0;
		while (num <= bkmkEndPreviosItemIndex && bkmkEndPreviosItemIndex < paragraph.ChildEntities.Count)
		{
			if (paragraph.Items[num] is BookmarkEnd)
			{
				num++;
				continue;
			}
			if (paragraph.Items[num] is BookmarkStart && bkmkEnd is BookmarkEnd)
			{
				if ((paragraph.Items[num] as BookmarkStart).Name != (bkmkEnd as BookmarkEnd).Name)
				{
					num++;
					continue;
				}
				return isItemsBefore;
			}
			return false;
		}
		return true;
	}

	private bool HasRenderableItemsBefore(WParagraph para, int endIndex)
	{
		while (endIndex > -1)
		{
			if (!(para.Items[endIndex] is BookmarkStart) && !(para.Items[endIndex] is BookmarkEnd) && !(para.Items[endIndex] is EditableRangeStart) && !(para.Items[endIndex] is EditableRangeEnd))
			{
				return true;
			}
			endIndex--;
		}
		return false;
	}

	private void AddBlockContentControInDocumentPart(WordDocumentPart documentPart, List<TextBodyItem> startInsertBodyItems, List<TextBodyItem> endInsertBodyItems)
	{
		if (startInsertBodyItems != null)
		{
			for (int num = startInsertBodyItems.Count - 1; num >= 0; num--)
			{
				documentPart.Sections[0].Body.Items.Insert(0, startInsertBodyItems[num]);
			}
		}
		if (endInsertBodyItems == null)
		{
			return;
		}
		foreach (TextBodyItem endInsertBodyItem in endInsertBodyItems)
		{
			documentPart.Sections[documentPart.Sections.Count - 1].Body.Items.Add(endInsertBodyItem);
		}
	}

	private void InsertSectAtOrgDocument(WordDocument originalDocument, WordDocument revisedDocument, int orgMatchedSecIndex, int orgMatchedBodyItemIndex, int orgStartRangeIndex)
	{
		WordDocumentPart wordDocumentPart = new WordDocumentPart();
		WSection wSection = revisedDocument.Sections[revisedDocument.m_sectionIndex].CloneWithoutBodyItems();
		wordDocumentPart.Sections.Add(wSection);
		(wSection.AddParagraph() as WParagraph).AddInsMark();
		wordDocumentPart.Sections.Add(new WSection(revisedDocument));
		string text = "Section" + Guid.NewGuid().ToString().Replace("-", "_");
		Entity entity = originalDocument.Sections[orgMatchedSecIndex].Body.Items[orgMatchedBodyItemIndex];
		WParagraph ownerParagraphToInsertBookmark = Document.GetOwnerParagraphToInsertBookmark(entity, isStart: true);
		BookmarkStart entity2 = new BookmarkStart(originalDocument, text);
		ownerParagraphToInsertBookmark.Items.Insert(orgStartRangeIndex, entity2);
		BookmarkEnd entity3 = new BookmarkEnd(originalDocument, text);
		ownerParagraphToInsertBookmark.Items.Insert(orgStartRangeIndex + 1, entity3);
		BookmarksNavigator bookmarksNavigator = new BookmarksNavigator(originalDocument);
		bookmarksNavigator.MoveToBookmark(text);
		Document.IsComparing = true;
		originalDocument.IsComparing = true;
		originalDocument.UpdateRevisionOnComparing = true;
		bookmarksNavigator.ReplaceContent(wordDocumentPart);
		Document.IsComparing = false;
		originalDocument.IsComparing = false;
		originalDocument.UpdateRevisionOnComparing = false;
		originalDocument.Bookmarks.Remove(originalDocument.Bookmarks.FindByName(text));
		WParagraph obj = originalDocument.Sections[originalDocument.m_sectionIndex].Body.Items[originalDocument.m_matchBodyItemIndex] as WParagraph;
		obj.BreakCharacterFormat.IsDeleteRevision = true;
		obj.BreakCharacterFormat.AuthorName = Document.m_authorName;
		obj.BreakCharacterFormat.RevDateTime = Document.m_dateTime;
		obj.UpdateParagraphRevision(obj, isIncludeParaItems: false);
	}

	internal void ApplyInsRev(WTextBody textBody, int startIndex, int endIndex)
	{
		for (int i = startIndex; i < endIndex; i++)
		{
			textBody.Items[i].AddInsMark();
		}
	}

	internal void ApplyDelRevision(WordDocument orgDoc, WordDocument revDoc, int endSecIndex, int endBodyItemIndex, int endParaItemIndex, ref bool isNeedToInsert, bool isDocumentEnd, WTextBody orgTextBody = null)
	{
		if (orgTextBody == null)
		{
			int num = orgDoc.m_bodyItemIndex;
			WParagraph orgLastPara = null;
			if (orgDoc.m_paraItemIndex > 0)
			{
				WParagraph wParagraph = orgDoc.Sections[orgDoc.m_sectionIndex].Body.Items[num] as WParagraph;
				wParagraph.ApplyDelRevision(wParagraph, orgDoc.m_paraItemIndex, wParagraph.Items.Count);
				bool flag = IsNotAllPreviousParaItemsDeleted(wParagraph, orgDoc.m_paraItemIndex);
				if (!(isDocumentEnd && flag) || orgDoc.m_paraItemIndex >= wParagraph.Items.Count)
				{
					wParagraph.BreakCharacterFormat.IsDeleteRevision = true;
					wParagraph.BreakCharacterFormat.AuthorName = Document.m_authorName;
					wParagraph.BreakCharacterFormat.RevDateTime = Document.m_dateTime;
				}
				WParagraph wParagraph2 = revDoc.Sections[revDoc.m_sectionIndex].Body.Items[revDoc.m_bodyItemIndex] as WParagraph;
				if (isDocumentEnd && wParagraph2 != null && flag && revDoc.m_paraItemIndex < wParagraph2.Items.Count)
				{
					wParagraph2.Insertion(orgDoc, wParagraph2.Items.Count, wParagraph.Items.Count, orgDoc.m_bodyItemIndex, orgDoc.m_sectionIndex, orgTextBody);
					revDoc.UpdateIndex(revDoc.m_bodyItemIndex + 1, 0, 0, 0, 0);
				}
				wParagraph.UpdateParagraphRevision(wParagraph, isIncludeParaItems: false);
				num++;
			}
			for (int i = orgDoc.m_sectionIndex; i <= endSecIndex; i++)
			{
				WTextBody body = orgDoc.Sections[i].Body;
				if (endSecIndex == orgDoc.m_sectionIndex)
				{
					ApplyDelRev(body, num, endBodyItemIndex);
					break;
				}
				if (i == orgDoc.m_sectionIndex)
				{
					ApplyDelForFirstSection(orgDoc, revDoc, body, num, endBodyItemIndex, ref isNeedToInsert, endParaItemIndex, endSecIndex, ref orgLastPara);
				}
				else if (i == endSecIndex)
				{
					if (orgLastPara != null)
					{
						ApplyDelRev(body, 0, endBodyItemIndex - 1);
						orgLastPara.ApplyDelRevision(orgLastPara, 0, orgLastPara.Items.Count);
					}
					else
					{
						ApplyDelRev(body, 0, endBodyItemIndex);
					}
				}
				else
				{
					ApplyDelRev(body, 0, body.Items.Count);
				}
			}
		}
		else
		{
			int num2 = orgTextBody.m_bodyItemIndex;
			if (orgTextBody.m_paraItemIndex > 0)
			{
				WParagraph wParagraph3 = orgTextBody.Items[num2] as WParagraph;
				wParagraph3.ApplyDelRevision(wParagraph3, orgTextBody.m_paraItemIndex, wParagraph3.Items.Count);
				wParagraph3.BreakCharacterFormat.IsDeleteRevision = true;
				wParagraph3.BreakCharacterFormat.AuthorName = Document.m_authorName;
				wParagraph3.BreakCharacterFormat.RevDateTime = Document.m_dateTime;
				wParagraph3.UpdateParagraphRevision(wParagraph3, isIncludeParaItems: false);
				num2++;
			}
			ApplyDelRev(orgTextBody, num2, endBodyItemIndex);
		}
	}

	private bool IsNotAllPreviousParaItemsDeleted(WParagraph paragraph, int paraItemIndex)
	{
		for (int num = paraItemIndex - 1; num >= 0; num--)
		{
			if (!paragraph.Items[num].IsDeleteRevision)
			{
				return true;
			}
		}
		return false;
	}

	private void ApplyDelForFirstSection(WordDocument orgDoc, WordDocument revDoc, WTextBody textBody, int orgBodyItemIndex, int endBodyItemIndex, ref bool isNeedToInsert, int endParaItemIndex, int endSecIndex, ref WParagraph orgLastPara)
	{
		if (orgBodyItemIndex == textBody.Items.Count && orgBodyItemIndex > 0 && textBody.Items[orgBodyItemIndex - 1] is WParagraph)
		{
			WParagraph obj = textBody.Items[orgBodyItemIndex - 1] as WParagraph;
			obj.BreakCharacterFormat.IsDeleteRevision = true;
			obj.BreakCharacterFormat.AuthorName = Document.m_authorName;
			obj.BreakCharacterFormat.FormatChangeDateTime = Document.m_dateTime;
			obj.UpdateParagraphRevision(obj, isIncludeParaItems: false);
			if (revDoc.m_bodyItemIndex > 0 && revDoc.Sections[revDoc.m_sectionIndex].Body.Items[revDoc.m_bodyItemIndex - 1] is WParagraph)
			{
				WParagraph wParagraph = revDoc.Sections[revDoc.m_sectionIndex].Body.Items[revDoc.m_bodyItemIndex - 1] as WParagraph;
				if (!isNeedToInsert && endBodyItemIndex > 0 && endParaItemIndex == 0)
				{
					orgLastPara = orgDoc.Sections[endSecIndex].Body.Items[endBodyItemIndex - 1] as WParagraph;
					return;
				}
				revDoc.m_bodyItemIndex--;
				revDoc.m_paraItemIndex = wParagraph.Items.Count;
				isNeedToInsert = true;
			}
		}
		else
		{
			ApplyDelRev(textBody, orgBodyItemIndex, textBody.Items.Count);
		}
	}

	internal void ApplyDelRev(WTextBody textBody, int startIndex, int endIndex)
	{
		for (int i = startIndex; i < endIndex; i++)
		{
			textBody.Items[i].AddDelMark();
		}
	}

	internal void MoveCurrPosition(WordDocument orgDoc, TextBodyItem orgItem, TextBodyItem revItem, WTextBody orgTextBody = null)
	{
		if (orgTextBody == null)
		{
			orgDoc.m_paraItemIndex = 0;
			orgDoc.m_bodyItemIndex = orgItem.Index;
			orgDoc.m_sectionIndex = orgItem.GetOwnerSection(orgItem).Index;
			orgDoc.UpdateMatchIndex();
			if (orgDoc.m_bodyItemIndex >= orgDoc.Sections[orgDoc.m_sectionIndex].Body.Items.Count)
			{
				orgDoc.m_sectionIndex++;
				orgDoc.UpdateIndex(0, 0, 0, 0, 0);
			}
			else
			{
				orgDoc.UpdateIndex(orgItem.Index + 1, 0, 0, 0, 0);
			}
			if (Document.m_bodyItemIndex >= revItem.OwnerTextBody.Count)
			{
				Document.m_sectionIndex++;
				Document.UpdateIndex(0, 0, 0, 0, 0);
			}
			else
			{
				Document.UpdateIndex(revItem.Index + 1, 0, 0, 0, 0);
			}
		}
		else
		{
			orgTextBody.m_paraItemIndex = 0;
			orgTextBody.m_bodyItemIndex = orgItem.Index;
			orgTextBody.UpdateMatchIndex();
			orgTextBody.UpdateIndex(orgItem.Index + 1, 0, 0, 0, 0);
			revItem.OwnerTextBody.UpdateIndex(revItem.Index + 1, 0, 0, 0, 0);
		}
	}

	internal void InsertAndDeleteUnmatchedItems(WordDocument orgDoc, TextBodyItem orgTextBodyItem, TextBodyItem revTextBodyItem, WTextBody orgTextbody = null)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		WordDocument revDoc = null;
		WTextBody wTextBody = null;
		bool flag = false;
		bool flag2 = false;
		if (orgTextbody == null)
		{
			num = orgTextBodyItem.Index;
			num2 = orgTextBodyItem.GetOwnerSection(orgTextBodyItem).Index;
			num3 = revTextBodyItem.Index;
			num4 = revTextBodyItem.GetOwnerSection(revTextBodyItem).Index;
			revDoc = Document;
			flag = orgDoc.m_bodyItemIndex != num || orgDoc.m_sectionIndex != num2 || orgDoc.m_paraItemIndex > 0;
			flag2 = Document.m_bodyItemIndex != num3 || Document.m_sectionIndex != num4 || Document.m_paraItemIndex > 0;
		}
		else
		{
			num = orgTextBodyItem.Index;
			num3 = revTextBodyItem.Index;
			wTextBody = revTextBodyItem.OwnerTextBody;
			flag = orgTextbody.m_bodyItemIndex != num || orgTextbody.m_paraItemIndex > 0;
			flag2 = wTextBody.m_bodyItemIndex != num3 || wTextBody.m_paraItemIndex > 0;
		}
		if (flag)
		{
			ApplyDelRevision(orgDoc, revDoc, num2, num, 0, ref flag2, isDocumentEnd: false, orgTextbody);
		}
		if (flag2)
		{
			Insertion(orgDoc, -1, num3, num4, 0, num, num2, orgTextbody, wTextBody);
		}
	}

	internal bool CompareDocxProps(Dictionary<string, Stream> DocxProps1, Dictionary<string, Stream> DocxProps2)
	{
		if (DocxProps1 != null && DocxProps2 != null)
		{
			if (DocxProps1.Count != DocxProps2.Count)
			{
				return false;
			}
			foreach (string key in DocxProps1.Keys)
			{
				if (!DocxProps2.ContainsKey(key) || DocxProps1[key].Length != DocxProps2[key].Length)
				{
					return false;
				}
			}
		}
		return true;
	}

	internal void Dispose()
	{
		if (m_imagesHash != null)
		{
			m_imagesHash.Clear();
			m_imagesHash = null;
		}
		if (m_blockContentControls != null)
		{
			m_blockContentControls.Clear();
			m_blockContentControls = null;
		}
		if (m_inlineContentControls != null)
		{
			m_inlineContentControls.Clear();
			m_inlineContentControls = null;
		}
		if (m_shapes != null)
		{
			m_shapes.Clear();
			m_shapes = null;
		}
		if (m_groupShapes != null)
		{
			m_groupShapes.Clear();
			m_groupShapes = null;
		}
		if (m_pictures != null)
		{
			m_pictures.Clear();
			m_pictures = null;
		}
		if (m_textBoxes != null)
		{
			m_textBoxes.Clear();
			m_textBoxes = null;
		}
		if (m_charts != null)
		{
			m_charts.Clear();
			m_charts = null;
		}
		if (m_fields != null)
		{
			m_fields.Clear();
			m_fields = null;
		}
		if (m_tables != null)
		{
			m_tables.Clear();
			m_tables = null;
		}
		if (m_maths != null)
		{
			m_maths.Clear();
			m_maths = null;
		}
		if (m_oles != null)
		{
			m_oles.Clear();
			m_oles = null;
		}
		if (m_tocs != null)
		{
			m_tocs.Clear();
			m_tocs = null;
		}
		if (m_revisedDocListStyles != null)
		{
			m_revisedDocListStyles.Clear();
			m_revisedDocListStyles = null;
		}
	}
}
