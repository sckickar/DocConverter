using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfListFieldItemCollection : PdfCollection, IPdfWrapper
{
	private PdfArray m_items = new PdfArray();

	private PdfField m_field;

	public PdfListFieldItem this[int index] => (PdfListFieldItem)base.List[index];

	IPdfPrimitive IPdfWrapper.Element => m_items;

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

	public PdfListFieldItemCollection()
	{
	}

	internal PdfListFieldItemCollection(PdfField field)
	{
		m_field = field;
	}

	public int Add(PdfListFieldItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		return DoAdd(item);
	}

	public void Insert(int index, PdfListFieldItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		DoInsert(index, item);
	}

	public void Remove(PdfListFieldItem item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (base.List.Contains(item))
		{
			DoRemove(item);
		}
	}

	public void RemoveAt(int index)
	{
		if (index < 0 || index >= base.List.Count)
		{
			throw new ArgumentNullException("index");
		}
		DoRemoveAt(index);
	}

	public bool Contains(PdfListFieldItem item)
	{
		return base.List.Contains(item);
	}

	public int IndexOf(PdfListFieldItem item)
	{
		return base.List.IndexOf(item);
	}

	public void Clear()
	{
		m_items.Clear();
		base.List.Clear();
		FormFieldsRemovedArgs formFieldsRemovedArgs = new FormFieldsRemovedArgs(m_field);
		formFieldsRemovedArgs.MethodName = "Clear";
		if (m_field != null && m_field.Form != null)
		{
			m_field.Form.OnFormFieldRemoved(formFieldsRemovedArgs);
		}
	}

	private int DoAdd(PdfListFieldItem item)
	{
		m_items.Add(((IPdfWrapper)item).Element);
		base.List.Add(item);
		item.m_field = m_field;
		item.m_index = base.List.IndexOf(item);
		FormFieldsAddedArgs formFieldsAddedArgs = new FormFieldsAddedArgs(m_field);
		formFieldsAddedArgs.Item = item;
		formFieldsAddedArgs.MethodName = "Add";
		OnItemAdded(formFieldsAddedArgs);
		if (m_field != null && m_field.Form != null)
		{
			m_field.Form.OnFormFieldAdded(formFieldsAddedArgs);
		}
		return base.List.Count - 1;
	}

	private void DoInsert(int index, PdfListFieldItem item)
	{
		m_items.Insert(index, ((IPdfWrapper)item).Element);
		base.List.Insert(index, item);
		FormFieldsAddedArgs formFieldsAddedArgs = new FormFieldsAddedArgs(m_field);
		formFieldsAddedArgs.Index = index;
		formFieldsAddedArgs.Item = item;
		formFieldsAddedArgs.MethodName = "Insert";
		if (m_field != null && m_field.Form != null)
		{
			m_field.Form.OnFormFieldAdded(formFieldsAddedArgs);
		}
	}

	private void DoRemoveAt(int index)
	{
		m_items.RemoveAt(index);
		PdfListFieldItem item = base.List[index] as PdfListFieldItem;
		FormFieldsRemovedArgs formFieldsRemovedArgs = new FormFieldsRemovedArgs(m_field);
		formFieldsRemovedArgs.Item = item;
		formFieldsRemovedArgs.MethodName = "Remove At";
		formFieldsRemovedArgs.Index = index;
		OnItemRemoved(formFieldsRemovedArgs);
		if (m_field != null && m_field.Form != null)
		{
			m_field.Form.OnFormFieldRemoved(formFieldsRemovedArgs);
		}
		base.List.RemoveAt(index);
	}

	private void DoRemove(PdfListFieldItem item)
	{
		int index = base.List.IndexOf(item);
		m_items.RemoveAt(index);
		FormFieldsRemovedArgs formFieldsRemovedArgs = new FormFieldsRemovedArgs(m_field);
		formFieldsRemovedArgs.Item = item;
		formFieldsRemovedArgs.MethodName = "Remove";
		OnItemRemoved(formFieldsRemovedArgs);
		if (m_field != null && m_field.Form != null)
		{
			m_field.Form.OnFormFieldRemoved(formFieldsRemovedArgs);
		}
		base.List.RemoveAt(index);
	}
}
