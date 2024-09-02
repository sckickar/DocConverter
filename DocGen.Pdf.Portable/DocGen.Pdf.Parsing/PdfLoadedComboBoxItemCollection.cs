using System;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedComboBoxItemCollection : PdfCollection
{
	public PdfLoadedComboBoxItem this[int index]
	{
		get
		{
			if (index < 0 || index >= base.Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return base.List[index] as PdfLoadedComboBoxItem;
		}
	}

	internal void Add(PdfLoadedComboBoxItem item)
	{
		if (item == null)
		{
			throw new NullReferenceException("item");
		}
		base.List.Add(item);
	}

	internal int Remove(PdfLoadedComboBoxItem item)
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

	internal PdfLoadedComboBoxItemCollection Clone()
	{
		PdfLoadedComboBoxItemCollection pdfLoadedComboBoxItemCollection = new PdfLoadedComboBoxItemCollection();
		for (int i = 0; i < base.List.Count; i++)
		{
			if (base.List[i] is PdfLoadedComboBoxItem)
			{
				pdfLoadedComboBoxItemCollection.Add((base.List[i] as PdfLoadedComboBoxItem).Clone());
			}
		}
		return pdfLoadedComboBoxItemCollection;
	}
}
