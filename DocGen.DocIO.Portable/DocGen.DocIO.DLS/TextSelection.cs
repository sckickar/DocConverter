using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public class TextSelection : IEnumerable
{
	private WParagraph m_para;

	private WTextRange m_startTr;

	private WTextRange m_endTr;

	private List<WTextRange> m_items = new List<WTextRange>();

	internal int m_startCut;

	internal int m_endCut;

	private int m_startIndex;

	private int m_endIndex;

	private WTextRange[] m_cachedRanges;

	internal TextSelectionList SelectionChain;

	public string SelectedText
	{
		get
		{
			if (m_startTr == null || m_endTr == null)
			{
				return string.Empty;
			}
			int num = 0;
			num = ((!(m_startTr.Owner is Break) || ((m_startTr.Owner as Break).BreakType != BreakType.LineBreak && (m_startTr.Owner as Break).BreakType != BreakType.TextWrappingBreak)) ? m_startTr.StartPos : (m_startTr.Owner as Break).StartPos);
			if (m_startCut != 0)
			{
				num += m_startCut;
			}
			int num2 = 0;
			num2 = ((!(m_endTr.Owner is Break) || ((m_endTr.Owner as Break).BreakType != BreakType.LineBreak && (m_endTr.Owner as Break).BreakType != BreakType.TextWrappingBreak)) ? ((m_endCut >= 0) ? (m_endTr.StartPos + m_endCut - num) : (m_endTr.StartPos + m_endTr.TextLength - num)) : ((m_endCut >= 0) ? ((m_endTr.Owner as Break).StartPos + m_endCut - num) : ((m_endTr.Owner as Break).StartPos + (m_endTr.Owner as Break).TextRange.Text.Length - num)));
			if (num2 < 0)
			{
				throw new Exception("Text selection was modified. This could be done while modification of source document.");
			}
			if (m_startTr.Owner is Break && ((m_startTr.Owner as Break).BreakType == BreakType.LineBreak || (m_startTr.Owner as Break).BreakType == BreakType.TextWrappingBreak))
			{
				return (m_startTr.Owner as Break).OwnerParagraph.Text.Substring(num, num2);
			}
			if (!(m_startTr.Owner is InlineContentControl))
			{
				return OwnerParagraph.Text.Substring(num, num2);
			}
			return m_startTr.GetOwnerParagraphValue().Text.Substring(num, num2);
		}
	}

	public string this[int index]
	{
		get
		{
			string text = m_items[index].Text;
			if (index == 0 && m_startCut > 0)
			{
				text = text.Substring(m_startCut);
			}
			if (index == m_items.Count - 1 && m_endCut != -1)
			{
				text = text.Substring(0, m_endCut - m_startCut);
			}
			return text;
		}
		set
		{
			WTextRange wTextRange = m_items[index];
			string text = value;
			if (index == 0 && m_startCut > 0)
			{
				text = wTextRange.Text.Substring(0, m_startCut) + text;
			}
			if (index == m_items.Count && m_endCut != -1)
			{
				text += wTextRange.Text.Substring(m_endCut);
			}
			wTextRange.Text = text;
		}
	}

	public int Count => m_items.Count;

	internal WParagraph OwnerParagraph
	{
		get
		{
			if (m_startTr != null)
			{
				m_para = m_startTr.OwnerParagraph;
			}
			return m_para;
		}
	}

	internal WTextRange StartTextRange => m_startTr;

	internal WTextRange EndTextRange => m_endTr;

	public TextSelection(WParagraph para, int startCharPos, int endCharPos)
	{
		m_para = para;
		if (m_para.Items.Count == 0)
		{
			return;
		}
		m_startIndex = FindUtils.GetStartRangeIndex(m_para, startCharPos + 1, out var tr);
		if (tr == null)
		{
			return;
		}
		if (tr.Owner is Break && ((tr.Owner as Break).BreakType == BreakType.LineBreak || (tr.Owner as Break).BreakType == BreakType.TextWrappingBreak))
		{
			tr.StartPos = (tr.Owner as Break).StartPos;
		}
		m_startCut = startCharPos - tr.StartPos;
		m_startTr = tr;
		m_endIndex = FindUtils.GetStartRangeIndex(m_para, endCharPos, out tr);
		if (m_endIndex < m_startIndex || tr == null)
		{
			for (int num = para.Items.Count; num > 0; num--)
			{
				ParagraphItem paragraphItem = para[num - 1];
				if (paragraphItem is WTextRange)
				{
					tr = paragraphItem as WTextRange;
					break;
				}
			}
			m_endCut = endCharPos - tr.StartPos - 1;
		}
		else
		{
			if (tr.Owner is Break && ((tr.Owner as Break).BreakType == BreakType.LineBreak || (tr.Owner as Break).BreakType == BreakType.TextWrappingBreak))
			{
				tr.StartPos = (tr.Owner as Break).StartPos;
			}
			m_endCut = endCharPos - tr.StartPos;
		}
		m_endTr = tr;
		if (m_endCut == tr.TextLength)
		{
			m_endCut = -1;
		}
		else if (tr.Owner is Break && ((tr.Owner as Break).BreakType == BreakType.LineBreak || (tr.Owner as Break).BreakType == BreakType.TextWrappingBreak) && m_endCut == (tr.Owner as Break).TextRange.Text.Length)
		{
			m_endCut = -1;
		}
		if (FindUtils.EnsureSameOwner(m_startTr, m_endTr))
		{
			GetTextRanges(m_startTr);
		}
	}

	public WTextRange[] GetRanges()
	{
		if (m_startTr.Owner is InlineContentControl && (m_startTr.Owner as InlineContentControl).ParagraphItems.Count == 0)
		{
			return null;
		}
		if (OwnerParagraph != null && OwnerParagraph.Items.Count == 0)
		{
			return null;
		}
		EnsureIndexes();
		if (m_startCut > 0 || m_endCut != -1)
		{
			SplitRanges();
		}
		return m_items.ToArray();
	}

	public WTextRange GetAsOneRange()
	{
		WParagraph ownerParagraph = OwnerParagraph;
		if (m_items.Count > 0 && m_items[0].Owner is Break && ((m_items[0].Owner as Break).BreakType == BreakType.LineBreak || (m_items[0].Owner as Break).BreakType == BreakType.TextWrappingBreak))
		{
			ownerParagraph = (m_items[0].Owner as Break).OwnerParagraph;
			int indexInOwnerCollection = (m_items[0].Owner as Break).GetIndexInOwnerCollection();
			if (ownerParagraph != null && ownerParagraph.Items.Count > 0)
			{
				Break @break = m_items[0].Owner as Break;
				ownerParagraph.Items.Remove(@break);
				@break.TextRange = null;
				ownerParagraph.Items.Insert(indexInOwnerCollection, m_items[0]);
			}
		}
		if (m_items[0].Owner is InlineContentControl)
		{
			if ((m_items[0].Owner as InlineContentControl).ParagraphItems.Count == 0 || m_items[0].GetOwnerParagraphValue().Items.Count == 0)
			{
				return null;
			}
		}
		else if (ownerParagraph == null || ownerParagraph.Items.Count == 0)
		{
			return null;
		}
		EnsureIndexes();
		if (m_startCut > 0 || m_endCut != -1)
		{
			SplitRanges();
		}
		if (Count > 1)
		{
			string selectedText = SelectedText;
			while (m_items.Count > 1)
			{
				if (m_items[1].Owner is Break && ((m_items[1].Owner as Break).BreakType == BreakType.LineBreak || (m_items[1].Owner as Break).BreakType == BreakType.TextWrappingBreak))
				{
					(m_items[1].Owner as Break).RemoveSelf();
				}
				else
				{
					m_items[1].RemoveSelf();
				}
				m_items.RemoveAt(1);
			}
			m_startTr.Text = selectedText;
			m_endTr = m_startTr;
		}
		return m_items[0];
	}

	internal int SplitAndErase()
	{
		if (m_startTr.Owner is InlineContentControl && (m_startTr.Owner as InlineContentControl).ParagraphItems.Count == 0)
		{
			return 0;
		}
		if (OwnerParagraph != null && OwnerParagraph.Items.Count == 0)
		{
			return 0;
		}
		EnsureIndexes();
		if (m_startCut > 0 || m_endCut != -1)
		{
			SplitRanges();
		}
		if (Count > 0)
		{
			while (m_items.Count > 0)
			{
				m_items[0].RemoveSelf();
				m_items.RemoveAt(0);
			}
			m_startTr = null;
			m_endTr = m_startTr;
		}
		return m_startIndex;
	}

	private void GetTextRanges(WTextRange startElement)
	{
		ParagraphItemCollection paragraphItemCollection = ((startElement.Owner is InlineContentControl) ? (startElement.Owner as InlineContentControl).ParagraphItems : ((startElement.Owner is Break) ? (startElement.Owner as Break).OwnerParagraph.Items : startElement.OwnerParagraph.Items));
		for (int i = m_startIndex; i <= m_endIndex; i++)
		{
			WTextRange wTextRange = ((!(paragraphItemCollection[i] is Break) || ((paragraphItemCollection[i] as Break).BreakType != BreakType.LineBreak && (paragraphItemCollection[i] as Break).BreakType != BreakType.TextWrappingBreak)) ? (paragraphItemCollection[i] as WTextRange) : (paragraphItemCollection[i] as Break).TextRange);
			if (wTextRange != null)
			{
				m_items.Add(wTextRange);
			}
		}
	}

	internal void CacheRanges()
	{
		if (m_cachedRanges != null)
		{
			return;
		}
		WTextRange[] ranges = GetRanges();
		if (ranges != null)
		{
			m_cachedRanges = new WTextRange[ranges.Length];
			int i = 0;
			for (int num = ranges.Length; i < num; i++)
			{
				m_cachedRanges[i] = (WTextRange)ranges[i].Clone();
			}
		}
	}

	internal void CopyTo(WParagraph para, int startIndex, bool saveFormatting, WCharacterFormat srcFormat)
	{
		CacheRanges();
		WTextRange[] cachedRanges = m_cachedRanges;
		for (int i = 0; i < cachedRanges.Length; i++)
		{
			WTextRange wTextRange = (WTextRange)cachedRanges[i].Clone();
			if (saveFormatting && srcFormat != null)
			{
				wTextRange.CharacterFormat.ImportContainer(srcFormat);
			}
			para.Items.Insert(startIndex, wTextRange);
			startIndex++;
		}
	}

	internal void CopyTo(InlineContentControl inlineContentControl, int startIndex, bool saveFormatting, WCharacterFormat srcFormat)
	{
		CacheRanges();
		WTextRange[] cachedRanges = m_cachedRanges;
		for (int i = 0; i < cachedRanges.Length; i++)
		{
			WTextRange wTextRange = (WTextRange)cachedRanges[i].Clone();
			if (saveFormatting && srcFormat != null)
			{
				wTextRange.CharacterFormat.ImportContainer(srcFormat);
			}
			inlineContentControl.ParagraphItems.Insert(startIndex, wTextRange);
			startIndex++;
		}
	}

	public IEnumerator GetEnumerator()
	{
		string[] array = new string[Count];
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			array[i] = this[i];
		}
		return array.GetEnumerator();
	}

	private void EnsureIndexes()
	{
		if (m_startTr.Owner != OwnerParagraph && (!(m_startTr.Owner is Break) || ((m_startTr.Owner as Break).BreakType != BreakType.LineBreak && (m_startTr.Owner as Break).BreakType != BreakType.TextWrappingBreak)) && !(m_startTr.Owner is InlineContentControl) && m_endTr.Owner != OwnerParagraph && (!(m_endTr.Owner is Break) || ((m_endTr.Owner as Break).BreakType != BreakType.LineBreak && (m_endTr.Owner as Break).BreakType != BreakType.TextWrappingBreak)) && !(m_endTr.Owner is InlineContentControl))
		{
			throw new InvalidOperationException();
		}
		int num = 0;
		num = GetItemsCount(m_startTr);
		if (m_startTr.Owner is Break && ((m_startTr.Owner as Break).BreakType == BreakType.LineBreak || (m_startTr.Owner as Break).BreakType == BreakType.TextWrappingBreak))
		{
			m_startIndex = (m_startTr.Owner as Break).GetIndexInOwnerCollection();
		}
		else if (m_startTr.Owner is InlineContentControl && (m_startIndex >= num || m_startTr != (m_startTr.Owner as InlineContentControl).ParagraphItems[m_startIndex]))
		{
			m_startIndex = m_startTr.GetIndexInOwnerCollection();
		}
		else if (!(m_startTr.Owner is InlineContentControl) && (m_startIndex >= num || m_startTr != OwnerParagraph.Items[m_startIndex]))
		{
			m_startIndex = m_startTr.GetIndexInOwnerCollection();
		}
		num = GetItemsCount(m_endTr);
		if (m_endTr.Owner is Break && ((m_endTr.Owner as Break).BreakType == BreakType.LineBreak || (m_endTr.Owner as Break).BreakType == BreakType.TextWrappingBreak))
		{
			m_endIndex = (m_endTr.Owner as Break).GetIndexInOwnerCollection();
		}
		else if (m_endTr.Owner is InlineContentControl && (m_endIndex >= num || m_endTr != (m_endTr.Owner as InlineContentControl).ParagraphItems[m_endIndex]))
		{
			m_endIndex = m_endTr.GetIndexInOwnerCollection();
		}
		else if (!(m_endTr.Owner is InlineContentControl) && (m_endIndex >= num || m_endTr != m_endTr.OwnerParagraph.Items[m_endIndex]))
		{
			m_endIndex = m_endTr.GetIndexInOwnerCollection();
		}
	}

	private int GetItemsCount(WTextRange textRange)
	{
		if (textRange.Owner is Break && ((textRange.Owner as Break).BreakType == BreakType.LineBreak || (textRange.Owner as Break).BreakType == BreakType.TextWrappingBreak))
		{
			return (textRange.Owner as Break).OwnerParagraph.Items.Count;
		}
		if (OwnerParagraph == null && textRange.Owner is InlineContentControl)
		{
			return (textRange.Owner as InlineContentControl).ParagraphItems.Count;
		}
		return textRange.OwnerParagraph.Items.Count;
	}

	internal void SplitRanges()
	{
		if (m_startCut > 0)
		{
			WTextRange wTextRange = new WTextRange(m_startTr.GetOwnerParagraphValue().Document);
			wTextRange.Text = m_startTr.Text.Substring(0, m_startCut);
			wTextRange.CharacterFormat.ImportContainer(m_startTr.CharacterFormat);
			m_startTr.Text = m_startTr.Text.Substring(m_startCut);
			if (m_startTr.Owner is InlineContentControl)
			{
				(m_startTr.Owner as InlineContentControl).ParagraphItems.Insert(m_startIndex, wTextRange);
			}
			else
			{
				OwnerParagraph.Items.Insert(m_startIndex, wTextRange);
			}
			m_startIndex++;
			m_endIndex++;
			if (SelectionChain != null)
			{
				UpdateFollowingSelections(forStart: true);
			}
			if (Count == 1 && m_endCut >= 0)
			{
				m_endCut -= m_startCut;
			}
			m_startCut = 0;
		}
		if (m_endCut > 0)
		{
			WTextRange wTextRange2 = new WTextRange(m_endTr.GetOwnerParagraphValue().Document);
			wTextRange2.Text = m_endTr.Text.Substring(m_endCut);
			wTextRange2.CharacterFormat.ImportContainer(m_endTr.CharacterFormat);
			m_endTr.Text = m_endTr.Text.Substring(0, m_endCut);
			if (m_endTr.Owner is InlineContentControl)
			{
				(m_endTr.Owner as InlineContentControl).ParagraphItems.Insert(m_endIndex + 1, wTextRange2);
			}
			else
			{
				OwnerParagraph.Items.Insert(m_endIndex + 1, wTextRange2);
			}
			if (SelectionChain != null)
			{
				UpdateFollowingSelections(forStart: false);
			}
			m_endCut = -1;
		}
	}

	private void UpdateFollowingSelections(bool forStart)
	{
		foreach (TextSelection item in SelectionChain)
		{
			if (item == this)
			{
				continue;
			}
			WTextRange wTextRange = (forStart ? m_startTr : m_endTr);
			int num = (forStart ? m_startCut : m_endCut);
			if (item.m_startTr == wTextRange)
			{
				if (!forStart)
				{
					item.m_startTr = (WTextRange)m_endTr.NextSibling;
					item.m_items[0] = item.m_startTr;
					item.m_startTr.SafeText = true;
				}
				item.m_startCut -= num;
			}
			if (item.m_endTr == wTextRange)
			{
				if (!forStart)
				{
					item.m_endTr = (WTextRange)m_endTr.NextSibling;
					item.m_items[item.m_items.Count - 1] = item.m_endTr;
					item.m_endTr.SafeText = true;
				}
				if (item.m_endCut >= 0)
				{
					item.m_endCut -= num;
				}
			}
		}
	}
}
