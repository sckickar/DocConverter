using System;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedRadioButtonItemCollection : PdfLoadedStateItemCollection
{
	public new PdfLoadedRadioButtonItem this[int index] => base[index] as PdfLoadedRadioButtonItem;

	internal int IndexOf(PdfLoadedRadioButtonItem item)
	{
		return IndexOf((PdfLoadedStateItem)item);
	}

	internal void Add(PdfLoadedRadioButtonItem item)
	{
		Add((PdfLoadedStateItem)item);
	}

	internal int Remove(PdfLoadedRadioButtonItem item)
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

	internal new PdfLoadedRadioButtonItemCollection Clone()
	{
		PdfLoadedRadioButtonItemCollection pdfLoadedRadioButtonItemCollection = new PdfLoadedRadioButtonItemCollection();
		for (int i = 0; i < base.List.Count; i++)
		{
			if (base.List[i] is PdfLoadedRadioButtonItem)
			{
				pdfLoadedRadioButtonItemCollection.Add((base.List[i] as PdfLoadedRadioButtonItem).Clone());
			}
		}
		return pdfLoadedRadioButtonItemCollection;
	}
}
