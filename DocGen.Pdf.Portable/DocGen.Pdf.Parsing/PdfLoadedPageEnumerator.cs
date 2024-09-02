using System;
using System.Collections;

namespace DocGen.Pdf.Parsing;

public class PdfLoadedPageEnumerator : IEnumerator
{
	private PdfLoadedPageCollection m_collection;

	private int m_index = -1;

	public object Current
	{
		get
		{
			if (m_index < 0 && m_index >= m_collection.Count)
			{
				throw new InvalidOperationException("The index is out of range.");
			}
			return m_collection[m_index];
		}
	}

	public PdfLoadedPageEnumerator(PdfLoadedPageCollection collection)
	{
		if (collection == null)
		{
			throw new ArgumentNullException("collection");
		}
		m_collection = collection;
	}

	public bool MoveNext()
	{
		m_index++;
		return m_index < m_collection.Count;
	}

	public void Reset()
	{
		m_index = -1;
	}
}
