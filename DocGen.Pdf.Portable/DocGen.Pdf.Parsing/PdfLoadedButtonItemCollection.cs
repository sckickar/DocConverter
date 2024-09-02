using System;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedButtonItemCollection : PdfCollection
{
	public PdfLoadedButtonItem this[int index]
	{
		get
		{
			if (index < 0 || index >= base.Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return base.List[index] as PdfLoadedButtonItem;
		}
	}

	internal void Add(PdfLoadedButtonItem item)
	{
		if (item == null)
		{
			throw new NullReferenceException("item");
		}
		base.List.Add(item);
	}

	internal int Remove(PdfLoadedButtonItem item)
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

	internal PdfLoadedButtonItemCollection Clone()
	{
		PdfLoadedButtonItemCollection pdfLoadedButtonItemCollection = new PdfLoadedButtonItemCollection();
		for (int i = 0; i < base.List.Count; i++)
		{
			if (base.List[i] is PdfLoadedButtonItem)
			{
				pdfLoadedButtonItemCollection.Add((base.List[i] as PdfLoadedButtonItem).Clone());
			}
		}
		return pdfLoadedButtonItemCollection;
	}
}
