using System;
using System.Collections;
using System.Collections.Generic;

namespace DocGen.Pdf;

public class PdfCollection : IEnumerable
{
	private List<object> m_list;

	public int Count => m_list.Count;

	protected List<object> List => m_list;

	internal List<object> InternalList => m_list;

	public PdfCollection()
	{
		m_list = new List<object>();
	}

	internal void CopyTo(IPdfWrapper[] array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		m_list.CopyTo(array, index);
	}

	public IEnumerator GetEnumerator()
	{
		return m_list.GetEnumerator();
	}

	internal void DoClear()
	{
		List.Clear();
	}
}
