using System;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedCheckBoxItemCollection : PdfLoadedStateItemCollection
{
	public new PdfLoadedCheckBoxItem this[int index] => base[index] as PdfLoadedCheckBoxItem;

	internal int IndexOf(PdfLoadedCheckBoxItem item)
	{
		return IndexOf((PdfLoadedStateItem)item);
	}

	internal void Add(PdfLoadedCheckBoxItem item)
	{
		Add((PdfLoadedStateItem)item);
	}

	internal int Remove(PdfLoadedCheckBoxItem item)
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
		Remove((PdfLoadedStateItem)item);
		return num;
	}

	internal new PdfLoadedCheckBoxItemCollection Clone()
	{
		PdfLoadedCheckBoxItemCollection pdfLoadedCheckBoxItemCollection = new PdfLoadedCheckBoxItemCollection();
		for (int i = 0; i < base.List.Count; i++)
		{
			if (base.List[i] is PdfLoadedCheckBoxItem)
			{
				pdfLoadedCheckBoxItemCollection.Add((base.List[i] as PdfLoadedCheckBoxItem).Clone());
			}
		}
		return pdfLoadedCheckBoxItemCollection;
	}
}
