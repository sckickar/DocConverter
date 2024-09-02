using System;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedTextBoxItemCollection : PdfCollection
{
	public PdfLoadedTexBoxItem this[int index]
	{
		get
		{
			if (index < 0 || index >= base.Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return base.List[index] as PdfLoadedTexBoxItem;
		}
	}

	internal void Add(PdfLoadedTexBoxItem item)
	{
		if (item == null)
		{
			throw new NullReferenceException("item");
		}
		base.List.Add(item);
	}

	internal int Remove(PdfLoadedTexBoxItem item)
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

	internal PdfLoadedTextBoxItemCollection Clone()
	{
		PdfLoadedTextBoxItemCollection pdfLoadedTextBoxItemCollection = new PdfLoadedTextBoxItemCollection();
		for (int i = 0; i < base.List.Count; i++)
		{
			if (base.List[i] is PdfLoadedTexBoxItem)
			{
				pdfLoadedTextBoxItemCollection.Add((base.List[i] as PdfLoadedTexBoxItem).Clone());
			}
		}
		return pdfLoadedTextBoxItemCollection;
	}
}
