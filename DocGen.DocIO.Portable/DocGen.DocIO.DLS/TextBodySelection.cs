using System;

namespace DocGen.DocIO.DLS;

public class TextBodySelection
{
	private WTextBody m_textBody;

	private int m_itemStartIndex;

	private int m_itemEndIndex;

	private int m_pItemStartIndex;

	private int m_pItemEndIndex;

	public WTextBody TextBody => m_textBody;

	public int ItemStartIndex
	{
		get
		{
			return m_itemStartIndex;
		}
		set
		{
			m_itemStartIndex = value;
		}
	}

	public int ItemEndIndex
	{
		get
		{
			return m_itemEndIndex;
		}
		set
		{
			m_itemEndIndex = value;
		}
	}

	public int ParagraphItemStartIndex
	{
		get
		{
			return m_pItemStartIndex;
		}
		set
		{
			m_pItemStartIndex = value;
		}
	}

	public int ParagraphItemEndIndex
	{
		get
		{
			return m_pItemEndIndex;
		}
		set
		{
			m_pItemEndIndex = value;
		}
	}

	public TextBodySelection(ParagraphItem itemStart, ParagraphItem itemEnd)
	{
		WParagraph ownerParagraph = itemStart.OwnerParagraph;
		WParagraph ownerParagraph2 = itemEnd.OwnerParagraph;
		if (ownerParagraph.Owner != ownerParagraph2.Owner)
		{
			throw new ArgumentException("itemStart and itemEnd must be contained in one text body");
		}
		m_textBody = ownerParagraph.OwnerTextBody;
		m_itemStartIndex = ownerParagraph.GetIndexInOwnerCollection();
		m_itemEndIndex = ownerParagraph2.GetIndexInOwnerCollection();
		m_pItemStartIndex = itemStart.GetIndexInOwnerCollection();
		m_pItemEndIndex = itemEnd.GetIndexInOwnerCollection();
		if (itemEnd is BookmarkEnd bookmarkEnd && bookmarkEnd.Name != "_GoBack" && !bookmarkEnd.HasRenderableItemBefore())
		{
			WParagraph wParagraph = ownerParagraph2.PreviousSibling as WParagraph;
			if (ownerParagraph != ownerParagraph2 && wParagraph != null)
			{
				m_itemEndIndex = wParagraph.GetIndexInOwnerCollection();
				m_pItemEndIndex = wParagraph.ChildEntities.Count;
				ownerParagraph2 = wParagraph;
			}
		}
		ValidateIndexes();
	}

	public TextBodySelection(ITextBody textBody, int itemStartIndex, int itemEndIndex, int pItemStartIndex, int pItemEndIndex)
	{
		if (textBody == null)
		{
			throw new ArgumentNullException("textBody");
		}
		m_textBody = (WTextBody)textBody;
		m_itemStartIndex = itemStartIndex;
		m_itemEndIndex = itemEndIndex;
		m_pItemStartIndex = pItemStartIndex;
		m_pItemEndIndex = pItemEndIndex;
		ValidateIndexes();
	}

	private void ValidateIndexes()
	{
		if (m_itemStartIndex < 0 || m_itemStartIndex >= m_textBody.Items.Count)
		{
			throw new ArgumentOutOfRangeException("m_itemStartIndex", "m_itemStartIndex is less than 0 or greater than " + m_textBody.Items.Count);
		}
		if (m_itemEndIndex < m_itemStartIndex || m_itemEndIndex >= m_textBody.Items.Count)
		{
			throw new ArgumentOutOfRangeException("m_itemEndIndex", "m_itemEndIndex is less than " + m_itemStartIndex + " or greater than " + m_textBody.Items.Count);
		}
		WParagraph wParagraph = m_textBody.Items[m_itemStartIndex] as WParagraph;
		WParagraph wParagraph2 = m_textBody.Items[m_itemEndIndex] as WParagraph;
		if (wParagraph != null && (m_pItemStartIndex < 0 || m_pItemStartIndex > wParagraph.Items.Count))
		{
			throw new ArgumentOutOfRangeException("m_pItemStartIndex", "m_pItemStartIndex is less than 0 or greater than " + wParagraph.Items.Count);
		}
		if (wParagraph2 != null && (m_pItemEndIndex < 0 || m_pItemEndIndex > wParagraph2.Items.Count))
		{
			throw new ArgumentOutOfRangeException("m_pItemEndIndex", "m_pItemEndIndex is less than 0 or greater than " + wParagraph2.Items.Count);
		}
	}
}
