using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfActionCollection : PdfCollection
{
	private PdfArray m_actions = new PdfArray();

	private PdfAction this[int index] => (PdfAction)base.List[index];

	public int Add(PdfAction action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return DoAdd(action);
	}

	public void Insert(int index, PdfAction action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		DoInsert(index, action);
	}

	public int IndexOf(PdfAction action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return base.List.IndexOf(action);
	}

	public bool Contains(PdfAction action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		return base.List.Contains(action);
	}

	public void Clear()
	{
		DoClear();
	}

	public void Remove(PdfAction action)
	{
		if (action == null)
		{
			throw new ArgumentNullException("action");
		}
		DoRemove(action);
	}

	public void RemoveAt(int index)
	{
		DoRemoveAt(index);
	}

	private int DoAdd(PdfAction action)
	{
		m_actions.Add(new PdfReferenceHolder(action));
		base.List.Add(action);
		return base.List.Count - 1;
	}

	private void DoInsert(int index, PdfAction action)
	{
		m_actions.Insert(index, new PdfReferenceHolder(action));
		base.List.Insert(index, action);
	}

	private new void DoClear()
	{
		m_actions.Clear();
		base.List.Clear();
	}

	private void DoRemove(PdfAction action)
	{
		int index = base.List.IndexOf(action);
		m_actions.RemoveAt(index);
		base.List.Remove(action);
	}

	private void DoRemoveAt(int index)
	{
		m_actions.RemoveAt(index);
		base.List.RemoveAt(index);
	}
}
