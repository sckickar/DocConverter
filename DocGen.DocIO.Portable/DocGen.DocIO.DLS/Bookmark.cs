using System;

namespace DocGen.DocIO.DLS;

public class Bookmark
{
	private BookmarkStart m_bkmkStart;

	private BookmarkEnd m_bkmkEnd;

	public string Name => m_bkmkStart.Name;

	public BookmarkStart BookmarkStart => m_bkmkStart;

	public BookmarkEnd BookmarkEnd => m_bkmkEnd;

	public short FirstColumn
	{
		get
		{
			if (BookmarkStart != null)
			{
				return BookmarkStart.ColumnFirst;
			}
			return -1;
		}
		set
		{
			if (BookmarkStart != null)
			{
				BookmarkStart.ColumnFirst = value;
			}
		}
	}

	public short LastColumn
	{
		get
		{
			if (BookmarkStart != null)
			{
				return BookmarkStart.ColumnLast;
			}
			return -1;
		}
		set
		{
			if (BookmarkStart != null)
			{
				BookmarkStart.ColumnLast = value;
			}
		}
	}

	public Bookmark(BookmarkStart start)
		: this(start, null)
	{
	}

	public Bookmark(BookmarkStart start, BookmarkEnd end)
	{
		m_bkmkStart = start;
		m_bkmkEnd = end;
	}

	internal void SetStart(BookmarkStart start)
	{
		m_bkmkStart = start;
	}

	internal void SetEnd(BookmarkEnd end)
	{
		if (HasValidPosition(end))
		{
			m_bkmkEnd = end;
		}
	}

	private bool HasValidPosition(BookmarkEnd bookmarkEnd)
	{
		InlineContentControl inlineContentControl = ((bookmarkEnd != null && bookmarkEnd.Owner != null) ? (bookmarkEnd.Owner as InlineContentControl) : null);
		if (inlineContentControl != null && inlineContentControl.ContentControlProperties.Type == ContentControlType.Text)
		{
			InlineContentControl inlineContentControl2 = ((BookmarkStart != null && BookmarkStart.Owner != null) ? (BookmarkStart.Owner as InlineContentControl) : null);
			string message = $"Cannot add Bookmark with name \"{bookmarkEnd.Name}\" at invalid positions in the Content Control";
			if (inlineContentControl2 == null || (inlineContentControl2.ContentControlProperties.Type != 0 && inlineContentControl2.ContentControlProperties.Type != ContentControlType.Text && inlineContentControl2.ContentControlProperties.Type != ContentControlType.BuildingBlockGallery && inlineContentControl2.ContentControlProperties.Type != ContentControlType.RepeatingSection))
			{
				throw new Exception(message);
			}
			for (int num = inlineContentControl2.ParagraphItems.IndexOf(BookmarkStart); num >= 0; num--)
			{
				Entity entity = inlineContentControl2.ParagraphItems[num];
				if (!(entity is BookmarkStart) && !(entity is BookmarkEnd) && !(entity is EditableRangeStart) && !(entity is EditableRangeEnd))
				{
					throw new Exception(message);
				}
			}
		}
		return true;
	}
}
