using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DocGen.DocIO.DLS.Rendering;
using DocGen.DocIO.Rendering;
using DocGen.Layouting;

namespace DocGen.DocIO.DLS;

public class ParagraphItemCollection : EntityCollection
{
	private static readonly Type[] DEF_ELEMENT_TYPES = new Type[1] { typeof(ParagraphItem) };

	public new ParagraphItem this[int index] => base.InnerList[index] as ParagraphItem;

	protected WParagraph OwnerParagraph => base.Owner as WParagraph;

	protected override Type[] TypesOfElement => DEF_ELEMENT_TYPES;

	public ParagraphItemCollection(WordDocument doc)
		: base(doc)
	{
	}

	internal ParagraphItemCollection(WParagraph owner)
		: base(owner.Document, owner)
	{
	}

	internal ParagraphItemCollection(InlineContentControl owner)
		: base(owner.Document, owner)
	{
	}

	internal void MoveParaItems(ParagraphItemCollection desParaItems, bool isRemoveLastInlineCC)
	{
		for (int num = base.Count - 1; num >= 0; num--)
		{
			Entity entity = this[num].Clone();
			if (entity is InlineContentControl)
			{
				InlineContentControl inlineContentControl = entity as InlineContentControl;
				inlineContentControl.ParagraphItems.Clear();
				desParaItems.Insert(0, inlineContentControl);
				(this[num] as InlineContentControl).ParagraphItems.MoveParaItems(inlineContentControl.ParagraphItems, isRemoveLastInlineCC);
				if (num != base.Count - 1 || isRemoveLastInlineCC)
				{
					Remove(this[num]);
				}
			}
			else
			{
				Remove(this[num]);
				desParaItems.Insert(0, entity);
			}
		}
	}

	internal void CloneItemsTo(ParagraphItemCollection items)
	{
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			ParagraphItem paragraphItem = (ParagraphItem)this[i].Clone();
			if (paragraphItem != null)
			{
				paragraphItem.SetOwner(items.Owner);
				items.UnsafeAdd(paragraphItem);
			}
		}
	}

	internal void UnsafeRemoveAt(int index)
	{
		RemoveFromInnerList(index);
	}

	internal void UnsafeAdd(ParagraphItem item)
	{
		AddToInnerList(item);
		if (base.Document == null)
		{
			return;
		}
		if (item is WField && (item as WField).FieldEnd != null)
		{
			base.Document.ClonedFields.Push(item as WField);
		}
		else if (item is TableOfContent && ((TableOfContent)item).TOCField.FieldEnd != null)
		{
			base.Document.ClonedFields.Push(((TableOfContent)item).TOCField);
		}
		else if (item is WOleObject && (item as WOleObject).Field.FieldEnd != null)
		{
			base.Document.ClonedFields.Push((item as WOleObject).Field);
		}
		else if (item is WFieldMark && base.Document.ClonedFields.Count > 0)
		{
			WField wField = base.Document.ClonedFields.Peek();
			if ((item as WFieldMark).Type == FieldMarkType.FieldSeparator)
			{
				wField.FieldSeparator = item as WFieldMark;
				return;
			}
			wField = base.Document.ClonedFields.Pop();
			wField.FieldEnd = item as WFieldMark;
		}
	}

	protected override void OnInsertComplete(int index, Entity entity)
	{
		base.OnInsertComplete(index, entity);
		if (base.Joined && entity.Owner != null && OwnerParagraph != null)
		{
			ParagraphItem obj = (ParagraphItem)entity;
			int itemPos = 0;
			if (index > 0)
			{
				itemPos = this[index - 1].EndPos;
			}
			obj.AttachToParagraph(OwnerParagraph, itemPos);
		}
		else if (base.Joined && entity.Owner is WMergeField)
		{
			(entity as ParagraphItem).ParaItemCharFormat.ApplyBase((entity.Owner as WMergeField).CharacterFormat.BaseFormat);
		}
		else if (base.Joined && OwnerParagraph == null && entity.Owner is InlineContentControl)
		{
			ParagraphItem paragraphItem = (ParagraphItem)entity;
			InlineContentControl inlineContentControl = paragraphItem.Owner as InlineContentControl;
			WParagraph ownerParagraphValue = paragraphItem.GetOwnerParagraphValue();
			int itemPos2 = 0;
			if (paragraphItem.Index == 0 && inlineContentControl != null)
			{
				itemPos2 = inlineContentControl.StartPos;
			}
			else if (paragraphItem.Index > 0)
			{
				itemPos2 = this[index - 1].EndPos;
			}
			paragraphItem.AttachToParagraph(ownerParagraphValue, itemPos2);
		}
	}

	protected override void OnRemove(int index)
	{
		Entity entity = this[index];
		if (base.Joined)
		{
			this[index].Detach();
		}
		base.OnRemove(IndexOf(entity));
	}

	protected override void OnClear()
	{
		if (base.Joined)
		{
			for (int i = 0; i < base.Count; i++)
			{
				this[i].Detach();
			}
		}
		base.OnClear();
	}

	internal override void Close()
	{
		if (base.InnerList != null && base.InnerList.Count > 0)
		{
			while (base.InnerList.Count > 0)
			{
				if (base.InnerList[base.InnerList.Count - 1] is ParagraphItem paragraphItem)
				{
					paragraphItem.Close();
					if (paragraphItem.Owner == null)
					{
						base.InnerList.Remove(paragraphItem);
					}
					else
					{
						Remove(paragraphItem);
					}
				}
			}
		}
		base.Close();
	}

	internal IWidget GetCurrentWidget(int index)
	{
		return base.InnerList[index] as IWidget;
	}

	internal void GetMinimumAndMaximumWordWidthInPara(ref float maximumWordWidthInPara, ref float minumWordWidthInPara)
	{
		if (base.InnerList.Count <= 0)
		{
			return;
		}
		Entity entity = base.InnerList[0] as Entity;
		StringBuilder stringBuilder = new StringBuilder();
		Dictionary<WTextRange, int> dictionary = new Dictionary<WTextRange, int>();
		List<float> list = new List<float>();
		while (true)
		{
			if (!(entity is WField) && entity is WTextRange && !string.IsNullOrEmpty((entity as WTextRange).Text))
			{
				dictionary.Add(entity as WTextRange, stringBuilder.Length);
				stringBuilder.Append((entity as WTextRange).Text);
			}
			else if (entity is WField || entity is WOleObject)
			{
				WField wField = ((entity is WField) ? (entity as WField) : (((entity as WOleObject).Field != null) ? (entity as WOleObject).Field : null));
				if (wField != null && wField.FieldEnd != null)
				{
					entity = ((wField.FieldSeparator == null) ? wField.FieldEnd : wField.FieldSeparator);
				}
			}
			else if (entity is WPicture && (entity as WPicture).TextWrappingStyle != TextWrappingStyle.InFrontOfText && (entity as WPicture).TextWrappingStyle != TextWrappingStyle.Behind)
			{
				list.Add((entity as WPicture).Width);
			}
			else if (entity is InlineContentControl && (entity as InlineContentControl).ParagraphItems.Count > 0)
			{
				(entity as InlineContentControl).ParagraphItems.GetMinimumAndMaximumWordWidthInPara(ref maximumWordWidthInPara, ref minumWordWidthInPara);
			}
			if (entity.NextSibling == null || (entity is Break && ((entity as Break).BreakType == BreakType.LineBreak || (entity as Break).BreakType == BreakType.TextWrappingBreak)))
			{
				break;
			}
			entity = entity.NextSibling as Entity;
		}
		string text = stringBuilder.ToString();
		MatchCollection matchCollection = new Regex("[\\s-\\t]").Matches(text);
		string empty = string.Empty;
		for (int i = 0; i <= matchCollection.Count; i++)
		{
			if (matchCollection.Count <= 0)
			{
				break;
			}
			int wordStartIndex = 0;
			int num = ((i != 0) ? (matchCollection[i - 1].Index + 1) : 0);
			int lengthTillDelimeter = GetLengthTillDelimeter(matchCollection, i, text, num);
			int num2 = num;
			int wordCount = num2;
			empty = text.Substring(num, lengthTillDelimeter);
			float width = MeasureMinMaxWordWidth(dictionary, num2, wordStartIndex, num, lengthTillDelimeter, wordCount, empty);
			UpdateMinMaxWordWidth(ref minumWordWidthInPara, ref maximumWordWidthInPara, width);
		}
		if (matchCollection.Count == 0)
		{
			int wordStartIndex = 0;
			int num = 0;
			int lengthTillDelimeter = text.Length;
			int num2 = num;
			int wordCount = num2;
			float width2 = MeasureMinMaxWordWidth(dictionary, num2, wordStartIndex, num, lengthTillDelimeter, wordCount, text);
			UpdateMinMaxWordWidth(ref minumWordWidthInPara, ref maximumWordWidthInPara, width2);
		}
		float num3 = 0f;
		if (list.Count > 0)
		{
			float num4 = list[0];
			for (int j = 1; j < list.Count; j++)
			{
				num4 = Math.Max(num4, list[j]);
			}
			num3 = num4;
		}
		if (minumWordWidthInPara == 0f || num3 > minumWordWidthInPara)
		{
			minumWordWidthInPara = num3;
		}
		if (maximumWordWidthInPara == 0f || num3 > maximumWordWidthInPara)
		{
			maximumWordWidthInPara = num3;
		}
	}

	private float MeasureMinMaxWordWidth(Dictionary<WTextRange, int> spans, int index, int wordStartIndex, int textstartIndex, int lengthOfMatchedtext, int wordCount, string matchedText)
	{
		string text = string.Empty;
		float num = 0f;
		DrawingContext drawingContext = DocumentLayouter.DrawingContext;
		string text2 = string.Empty;
		foreach (WTextRange key in spans.Keys)
		{
			int num2 = spans[key];
			int textLength = key.TextLength;
			if (index <= num2 + textLength)
			{
				wordStartIndex = index - num2;
				if (textstartIndex + lengthOfMatchedtext <= num2 + textLength)
				{
					wordCount = textstartIndex + lengthOfMatchedtext - (num2 + wordStartIndex);
				}
				else
				{
					wordCount = textLength - wordStartIndex;
					index += wordCount;
				}
				try
				{
					text = key.Text.Substring(wordStartIndex, wordCount);
					text2 += text;
				}
				catch (ArgumentOutOfRangeException)
				{
				}
			}
			if (!string.IsNullOrEmpty(text) && IsNeedToCalculateWordWidth(text))
			{
				num += drawingContext.MeasureTextRange(key, text).Width;
			}
			if (text2 == matchedText)
			{
				break;
			}
		}
		return num;
	}

	private bool IsNeedToCalculateWordWidth(string text)
	{
		bool flag = false;
		bool flag2 = false;
		foreach (char c in text)
		{
			if (c >= '\u3000' && c <= 'ヿ')
			{
				flag = true;
				continue;
			}
			if (c >= '一' && c <= '龯')
			{
				flag2 = true;
				continue;
			}
			return true;
		}
		return !(flag && flag2);
	}

	private void UpdateMinMaxWordWidth(ref float minumWordWidthInPara, ref float maximumWordWidthInPara, float width)
	{
		if (width != 0f && (minumWordWidthInPara == 0f || width < minumWordWidthInPara))
		{
			minumWordWidthInPara = width;
		}
		if (width != 0f && (maximumWordWidthInPara == 0f || width > maximumWordWidthInPara))
		{
			maximumWordWidthInPara = width;
		}
	}

	private int GetLengthTillDelimeter(MatchCollection matches, int i, string text, int startIndex)
	{
		if (i == 0)
		{
			return matches[0].Index;
		}
		if (i < matches.Count)
		{
			return matches[i].Index - startIndex;
		}
		return text.Length - startIndex;
	}
}
