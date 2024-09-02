using System;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedStateItemCollection : PdfCollection
{
	public PdfLoadedStateItem this[int index]
	{
		get
		{
			if (index < 0 || index >= base.Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return base.List[index] as PdfLoadedStateItem;
		}
	}

	internal int IndexOf(PdfLoadedStateItem item)
	{
		return base.List.IndexOf(item);
	}

	internal void Add(PdfLoadedStateItem item)
	{
		if (item == null)
		{
			throw new NullReferenceException("item");
		}
		base.List.Add(item);
	}

	internal int Remove(PdfLoadedStateItem item)
	{
		if (item == null)
		{
			throw new NullReferenceException("item");
		}
		int num = base.List.IndexOf(item);
		if (base.List.Count != 0 && num >= 0)
		{
			base.List.RemoveAt(num);
		}
		base.List.Remove(item);
		return num;
	}

	internal PdfLoadedStateItemCollection Clone()
	{
		PdfLoadedStateItemCollection pdfLoadedStateItemCollection = new PdfLoadedStateItemCollection();
		for (int i = 0; i < base.List.Count; i++)
		{
			if (base.List[i] is PdfLoadedRadioButtonItem)
			{
				pdfLoadedStateItemCollection.Add((base.List[i] as PdfLoadedRadioButtonItem).Clone());
			}
			else if (base.List[i] is PdfLoadedCheckBoxItem)
			{
				pdfLoadedStateItemCollection.Add((base.List[i] as PdfLoadedCheckBoxItem).Clone());
			}
		}
		return pdfLoadedStateItemCollection;
	}
}
