using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Lists;

public class PdfListItemCollection : PdfCollection
{
	public PdfListItem this[int index]
	{
		get
		{
			if (index < 0 || index >= base.Count)
			{
				throw new IndexOutOfRangeException("The index should be less than item's count or more or equel to 0");
			}
			return (PdfListItem)base.List[index];
		}
	}

	public PdfListItemCollection()
	{
	}

	public PdfListItemCollection(string[] items)
		: this()
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		foreach (string text in items)
		{
			Add(text);
		}
	}

	public int Add(PdfListItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		base.List.Add(item);
		return base.List.Count - 1;
	}

	public int Add(PdfListItem item, float itemIndent)
	{
		item.TextIndent = itemIndent;
		return Add(item);
	}

	public PdfListItem Add(string text)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		PdfListItem pdfListItem = new PdfListItem(text);
		base.List.Add(pdfListItem);
		return pdfListItem;
	}

	public PdfListItem Add(string text, float itemIndent)
	{
		PdfListItem pdfListItem = Add(text);
		pdfListItem.TextIndent = itemIndent;
		return pdfListItem;
	}

	public PdfListItem Add(string text, PdfFont font)
	{
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		if (font == null)
		{
			throw new ArgumentNullException("font");
		}
		PdfListItem pdfListItem = new PdfListItem(text, font);
		base.List.Add(pdfListItem);
		return pdfListItem;
	}

	public PdfListItem Add(string text, PdfFont font, float itemIndent)
	{
		PdfListItem pdfListItem = Add(text, font);
		pdfListItem.TextIndent = itemIndent;
		return pdfListItem;
	}

	public void Insert(int index, PdfListItem item)
	{
		if (index < 0 || index >= base.Count)
		{
			throw new ArgumentException("The index should be less than item's count or more or equal to 0", "index");
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		base.List.Insert(index, item);
	}

	public void Insert(int index, PdfListItem item, float itemIndent)
	{
		item.TextIndent = itemIndent;
		base.List.Insert(index, item);
	}

	public void Remove(PdfListItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (!base.List.Contains(item))
		{
			throw new ArgumentException("The list doesn't contain this item", "item");
		}
		base.List.Remove(item);
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= base.Count)
		{
			throw new ArgumentException("The index should be less than item's count or more or equal to 0", "index");
		}
		base.List.RemoveAt(index);
	}

	public int IndexOf(PdfListItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		return base.List.IndexOf(item);
	}

	public void Clear()
	{
		base.List.Clear();
	}
}
