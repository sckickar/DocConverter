using System;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedListFieldItemCollection : PdfCollection
{
	public PdfLoadedListFieldItem this[int index]
	{
		get
		{
			if (index < 0 || index >= base.Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return base.List[index] as PdfLoadedListFieldItem;
		}
	}

	internal void Add(PdfLoadedListFieldItem item)
	{
		if (item == null)
		{
			throw new NullReferenceException("item");
		}
		base.List.Add(item);
	}

	internal int Remove(PdfLoadedListFieldItem item)
	{
		if (item == null)
		{
			throw new NullReferenceException("item");
		}
		int num = base.List.IndexOf(item);
		if (base.List.Count != 0)
		{
			base.List.RemoveAt(num);
		}
		base.List.Remove(item);
		return num;
	}

	internal PdfLoadedListFieldItemCollection Clone()
	{
		PdfLoadedListFieldItemCollection pdfLoadedListFieldItemCollection = new PdfLoadedListFieldItemCollection();
		for (int i = 0; i < base.List.Count; i++)
		{
			if (base.List[i] is PdfLoadedListFieldItem)
			{
				pdfLoadedListFieldItemCollection.Add((base.List[i] as PdfLoadedListFieldItem).Clone());
			}
		}
		return pdfLoadedListFieldItemCollection;
	}
}
