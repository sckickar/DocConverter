using System;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedSignatureItemCollection : PdfCollection
{
	public PdfLoadedSignatureItem this[int index]
	{
		get
		{
			if (index < 0 || index >= base.Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return base.List[index] as PdfLoadedSignatureItem;
		}
	}

	internal void Add(PdfLoadedSignatureItem item)
	{
		if (item == null)
		{
			throw new NullReferenceException("item");
		}
		base.List.Add(item);
	}

	internal int Remove(PdfLoadedSignatureItem item)
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

	internal PdfLoadedSignatureItemCollection Clone()
	{
		PdfLoadedSignatureItemCollection pdfLoadedSignatureItemCollection = new PdfLoadedSignatureItemCollection();
		for (int i = 0; i < base.List.Count; i++)
		{
			if (base.List[i] is PdfLoadedSignatureItem)
			{
				pdfLoadedSignatureItemCollection.Add((base.List[i] as PdfLoadedSignatureItem).Clone());
			}
		}
		return pdfLoadedSignatureItemCollection;
	}
}
