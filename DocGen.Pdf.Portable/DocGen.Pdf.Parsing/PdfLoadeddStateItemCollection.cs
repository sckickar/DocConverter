using System;

namespace DocGen.Pdf.Parsing;

public class PdfLoadeddStateItemCollection : PdfCollection
{
	public PdfLoadedStateItem this[int index]
	{
		get
		{
			if (index < 0 || index >= base.Count)
			{
				throw new IndexOutOfRangeException("index");
			}
			return base.List[index] as PdfLoadedCheckBoxItem;
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
		base.List.RemoveAt(num);
		base.List.Remove(item);
		return num;
	}
}
