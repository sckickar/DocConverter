using System;
using System.Collections;

namespace DocGen.Pdf;

public class PdfStampCollection : PdfCollection
{
	private struct PdfPageTemplateEnumerator : IEnumerator
	{
		private PdfStampCollection m_stamps;

		private int m_currentIndex;

		public object Current
		{
			get
			{
				CheckIndex();
				return m_stamps[m_currentIndex];
			}
		}

		internal PdfPageTemplateEnumerator(PdfStampCollection stamps)
		{
			if (stamps == null)
			{
				throw new ArgumentNullException("stamps");
			}
			m_stamps = stamps;
			m_currentIndex = -1;
		}

		public bool MoveNext()
		{
			m_currentIndex++;
			return m_currentIndex < m_stamps.Count;
		}

		public void Reset()
		{
			m_currentIndex = -1;
		}

		private void CheckIndex()
		{
			if (m_currentIndex < 0 || m_currentIndex >= m_stamps.Count)
			{
				throw new IndexOutOfRangeException();
			}
		}
	}

	public PdfPageTemplateElement this[int index] => base.List[index] as PdfPageTemplateElement;

	public int Add(PdfPageTemplateElement template)
	{
		if (template == null)
		{
			throw new ArgumentNullException("template");
		}
		base.List.Add(template);
		return base.List.Count - 1;
	}

	public PdfPageTemplateElement Add(float x, float y, float width, float height)
	{
		PdfPageTemplateElement pdfPageTemplateElement = new PdfPageTemplateElement(x, y, width, height);
		Add(pdfPageTemplateElement);
		return pdfPageTemplateElement;
	}

	public bool Contains(PdfPageTemplateElement template)
	{
		if (template == null)
		{
			throw new ArgumentNullException("template");
		}
		return base.List.Contains(template);
	}

	public void Insert(int index, PdfPageTemplateElement template)
	{
		if (template == null)
		{
			throw new ArgumentNullException("template");
		}
		base.List.Insert(index, template);
	}

	public void Remove(PdfPageTemplateElement template)
	{
		if (template == null)
		{
			throw new ArgumentNullException("template");
		}
		base.List.Remove(template);
	}

	public void RemoveAt(int index)
	{
		base.List.RemoveAt(index);
	}

	public void Clear()
	{
		base.List.Clear();
	}

	public new IEnumerator GetEnumerator()
	{
		return new PdfPageTemplateEnumerator(this);
	}
}
