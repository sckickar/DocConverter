using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfRadioButtonItemCollection : PdfCollection, IPdfWrapper
{
	private PdfArray m_array = new PdfArray();

	private PdfRadioButtonListField m_field;

	public PdfRadioButtonListItem this[int index] => (PdfRadioButtonListItem)base.List[index];

	IPdfPrimitive IPdfWrapper.Element => m_array;

	internal event EventHandler<FormFieldsAddedArgs> ItemsAdded;

	internal event EventHandler<FormFieldsRemovedArgs> ItemsRemoved;

	internal void OnItemAdded(FormFieldsAddedArgs e)
	{
		this.ItemsAdded?.Invoke(this, e);
	}

	internal void OnItemRemoved(FormFieldsRemovedArgs e)
	{
		this.ItemsRemoved?.Invoke(this, e);
	}

	public PdfRadioButtonItemCollection(PdfRadioButtonListField field)
	{
		m_field = field;
	}

	public int Add(PdfRadioButtonListItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		return DoAdd(item);
	}

	public void Insert(int index, PdfRadioButtonListItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		DoInsert(index, item);
	}

	public void Remove(PdfRadioButtonListItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		DoRemove(item);
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= base.List.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		PdfRadioButtonListItem items = (PdfRadioButtonListItem)base.List[index];
		m_array.RemoveAt(index);
		FormFieldsRemovedArgs formFieldsRemovedArgs = new FormFieldsRemovedArgs(m_field);
		formFieldsRemovedArgs.MethodName = "Remove At";
		formFieldsRemovedArgs.Index = index;
		formFieldsRemovedArgs.Items = items;
		OnItemRemoved(formFieldsRemovedArgs);
		if (m_field != null && m_field.Form != null)
		{
			m_field.Form.OnFormFieldRemoved(formFieldsRemovedArgs);
		}
		base.List.RemoveAt(index);
	}

	public int IndexOf(PdfRadioButtonListItem item)
	{
		return base.List.IndexOf(item);
	}

	public bool Contains(PdfRadioButtonListItem item)
	{
		return base.List.Contains(item);
	}

	public void Clear()
	{
		DoClear();
	}

	private int DoAdd(PdfRadioButtonListItem item)
	{
		m_array.Add(new PdfReferenceHolder(item));
		item.SetField(m_field);
		base.List.Add(item);
		FormFieldsAddedArgs formFieldsAddedArgs = new FormFieldsAddedArgs(m_field);
		formFieldsAddedArgs.Items = item;
		formFieldsAddedArgs.MethodName = "Add";
		OnItemAdded(formFieldsAddedArgs);
		if (m_field != null && m_field.Form != null)
		{
			m_field.Form.OnFormFieldAdded(formFieldsAddedArgs);
		}
		return base.List.Count - 1;
	}

	private void DoInsert(int index, PdfRadioButtonListItem item)
	{
		m_array.Insert(index, new PdfReferenceHolder(item));
		item.SetField(m_field);
		base.List.Insert(index, item);
		FormFieldsAddedArgs formFieldsAddedArgs = new FormFieldsAddedArgs(m_field);
		formFieldsAddedArgs.Index = index;
		formFieldsAddedArgs.Items = item;
		formFieldsAddedArgs.MethodName = "Insert";
		if (m_field != null && m_field.Form != null)
		{
			m_field.Form.OnFormFieldAdded(formFieldsAddedArgs);
		}
	}

	private void DoRemove(PdfRadioButtonListItem item)
	{
		if (base.List.Contains(item))
		{
			int index = base.List.IndexOf(item);
			m_array.RemoveAt(index);
			item.SetField(null);
			FormFieldsRemovedArgs formFieldsRemovedArgs = new FormFieldsRemovedArgs(m_field);
			formFieldsRemovedArgs.Items = item;
			formFieldsRemovedArgs.MethodName = "Remove";
			OnItemRemoved(formFieldsRemovedArgs);
			if (m_field != null && m_field.Form != null)
			{
				m_field.Form.OnFormFieldRemoved(formFieldsRemovedArgs);
			}
			base.List.RemoveAt(index);
		}
	}

	private new void DoClear()
	{
		foreach (PdfRadioButtonListItem item in base.List)
		{
			item.SetField(null);
		}
		m_array.Clear();
		base.List.Clear();
		FormFieldsRemovedArgs formFieldsRemovedArgs = new FormFieldsRemovedArgs(m_field);
		formFieldsRemovedArgs.MethodName = "Clear";
		if (m_field != null && m_field.Form != null)
		{
			m_field.Form.OnFormFieldRemoved(formFieldsRemovedArgs);
		}
	}
}
