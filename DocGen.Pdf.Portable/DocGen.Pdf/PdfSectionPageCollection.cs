using System;
using System.Collections;

namespace DocGen.Pdf;

public class PdfSectionPageCollection : IEnumerable
{
	private PdfSection m_section;

	public PdfPage this[int index]
	{
		get
		{
			if (index < 0 && index > Count)
			{
				throw new ArgumentOutOfRangeException("index");
			}
			return m_section[index];
		}
	}

	public int Count => m_section.Count;

	private PdfSectionPageCollection()
	{
	}

	internal PdfSectionPageCollection(PdfSection section)
	{
		if (section == null)
		{
			throw new ArgumentNullException("section");
		}
		m_section = section;
	}

	public PdfPage Add()
	{
		return m_section.Add();
	}

	public void Add(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_section.Add(page);
	}

	public void Insert(int index, PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		if (index < 0 && index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		m_section.Insert(index, page);
	}

	public int IndexOf(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		return m_section.IndexOf(page);
	}

	public bool Contains(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		return m_section.Contains(page);
	}

	public void Remove(PdfPage page)
	{
		if (page == null)
		{
			throw new ArgumentNullException("page");
		}
		m_section.Remove(page);
	}

	public void RemoveAt(int index)
	{
		if (index < 0 && index > Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		m_section.RemoveAt(index);
	}

	public void Clear()
	{
		m_section = null;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)m_section).GetEnumerator();
	}
}
