using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedListItemCollection : PdfCollection
{
	private PdfLoadedChoiceField m_field;

	public PdfLoadedListItem this[int index]
	{
		get
		{
			if (index < 0 || index >= base.List.Count)
			{
				throw new IndexOutOfRangeException("Index");
			}
			return base.List[index] as PdfLoadedListItem;
		}
	}

	internal PdfLoadedListItemCollection(PdfLoadedChoiceField field)
	{
		m_field = field;
	}

	public int Add(PdfLoadedListItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		PdfArray items = GetItems();
		PdfArray array = GetArray(item);
		items.Add(array);
		m_field.Dictionary.SetProperty("Opt", items);
		base.List.Add(item);
		return base.List.Count - 1;
	}

	internal int AddItem(PdfLoadedListItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		base.List.Add(item);
		return base.List.Count - 1;
	}

	public void Insert(int index, PdfLoadedListItem item)
	{
		if (index < 0 || index > base.List.Count)
		{
			throw new IndexOutOfRangeException("index");
		}
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		PdfArray items = GetItems();
		PdfArray array = GetArray(item);
		items.Insert(index, array);
		m_field.Dictionary.SetProperty("Opt", items);
		base.List.Insert(index, item);
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index > base.List.Count)
		{
			throw new IndexOutOfRangeException("index");
		}
		PdfArray items = GetItems();
		items.RemoveAt(index);
		m_field.Dictionary.SetProperty("Opt", items);
		base.List.RemoveAt(index);
	}

	public void Clear()
	{
		PdfArray items = GetItems();
		items.Clear();
		m_field.Dictionary.SetProperty("Opt", items);
		base.List.Clear();
	}

	private PdfArray GetItems()
	{
		PdfArray result = new PdfArray();
		if (m_field.Dictionary.ContainsKey("Opt"))
		{
			result = m_field.CrossTable.GetObject(m_field.Dictionary["Opt"]) as PdfArray;
		}
		return result;
	}

	private PdfArray GetArray(PdfLoadedListItem item)
	{
		PdfArray pdfArray = new PdfArray();
		if (item.Value != string.Empty)
		{
			pdfArray.Add(new PdfString(item.Value));
		}
		if (item.Text != string.Empty)
		{
			pdfArray.Add(new PdfString(item.Text));
		}
		return pdfArray;
	}
}
