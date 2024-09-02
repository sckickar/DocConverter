using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

internal class WidgetAnnotationCollection : PdfCollection, IPdfWrapper
{
	private PdfArray m_array = new PdfArray();

	public WidgetAnnotation this[int index] => (WidgetAnnotation)base.List[index];

	public IPdfPrimitive Element => m_array;

	public int Add(WidgetAnnotation annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		return DoAdd(annotation);
	}

	public void Insert(int index, WidgetAnnotation annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		DoInsert(index, annotation);
	}

	public void Remove(WidgetAnnotation annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		DoRemove(annotation);
	}

	public void RemoveAt(int index)
	{
		DoRemoveAt(index);
	}

	public int IndexOf(WidgetAnnotation annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		return base.List.IndexOf(annotation);
	}

	public bool Contains(WidgetAnnotation annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		return base.List.Contains(annotation);
	}

	public void Clear()
	{
		DoClear();
	}

	private int DoAdd(WidgetAnnotation annotation)
	{
		m_array.Add(new PdfReferenceHolder(annotation));
		base.List.Add(annotation);
		return base.List.Count - 1;
	}

	private void DoInsert(int index, WidgetAnnotation annotation)
	{
		m_array.Insert(index, new PdfReferenceHolder(annotation));
		base.List.Insert(index, annotation);
	}

	private void DoRemove(WidgetAnnotation annotation)
	{
		int index = base.List.IndexOf(annotation);
		m_array.RemoveAt(index);
		base.List.RemoveAt(index);
	}

	private void DoRemoveAt(int index)
	{
		m_array.RemoveAt(index);
		base.List.RemoveAt(index);
	}

	private new void DoClear()
	{
		m_array.Clear();
		base.List.Clear();
	}
}
