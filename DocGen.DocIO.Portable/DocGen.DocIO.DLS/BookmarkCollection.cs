using System;

namespace DocGen.DocIO.DLS;

public class BookmarkCollection : CollectionImpl
{
	public Bookmark this[string name] => FindByName(name);

	public Bookmark this[int index] => base.InnerList[index] as Bookmark;

	internal BookmarkCollection(WordDocument doc)
		: base(doc, doc)
	{
	}

	public Bookmark FindByName(string name)
	{
		string text = name.Replace('-', '_');
		for (int i = 0; i < base.InnerList.Count; i++)
		{
			Bookmark bookmark = base.InnerList[i] as Bookmark;
			if (bookmark.Name.ToUpper() == text.ToUpper())
			{
				return bookmark;
			}
		}
		return null;
	}

	public void RemoveAt(int index)
	{
		Bookmark bookmark = base.InnerList[index] as Bookmark;
		Remove(bookmark);
	}

	public void Remove(Bookmark bookmark)
	{
		base.InnerList.Remove(bookmark);
		BookmarkStart bookmarkStart = bookmark.BookmarkStart;
		BookmarkEnd bookmarkEnd = bookmark.BookmarkEnd;
		bookmarkStart?.RemoveSelf();
		bookmarkEnd?.RemoveSelf();
	}

	public void Clear()
	{
		while (base.InnerList.Count > 0)
		{
			int index = base.InnerList.Count - 1;
			RemoveAt(index);
		}
	}

	internal void Add(Bookmark bookmark)
	{
		base.InnerList.Add(bookmark);
	}

	internal void AttachBookmarkStart(BookmarkStart bookmarkStart)
	{
		Bookmark bookmark = this[bookmarkStart.Name];
		if (bookmark != null)
		{
			bookmarkStart.SetName(bookmarkStart.Name + Guid.NewGuid());
			bookmarkStart.RemoveSelf();
		}
		else
		{
			bookmark = new Bookmark(bookmarkStart);
			Add(bookmark);
		}
	}

	internal void AttachBookmarkEnd(BookmarkEnd bookmarkEnd)
	{
		Bookmark bookmark = this[bookmarkEnd.Name];
		if (bookmark == null)
		{
			return;
		}
		BookmarkEnd bookmarkEnd2 = bookmark.BookmarkEnd;
		if (bookmarkEnd2 != null)
		{
			bookmarkEnd.RemoveSelf();
			if (bookmark.BookmarkEnd == null)
			{
				bookmark.SetEnd(bookmarkEnd2);
			}
		}
		else
		{
			bookmark.SetEnd(bookmarkEnd);
		}
	}
}
