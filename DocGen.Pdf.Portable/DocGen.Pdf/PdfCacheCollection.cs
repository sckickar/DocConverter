using System.Collections.Generic;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf;

internal class PdfCacheCollection
{
	private List<List<object>> m_referenceObjects;

	private List<PdfFont> m_fontCollection;

	private List<object> this[int index] => m_referenceObjects[index];

	internal List<PdfFont> FontCollection
	{
		get
		{
			if (m_fontCollection == null)
			{
				m_fontCollection = new List<PdfFont>();
			}
			return m_fontCollection;
		}
		set
		{
			m_fontCollection = value;
		}
	}

	internal List<List<object>> ReferenceObjects => m_referenceObjects;

	public PdfCacheCollection()
	{
		m_referenceObjects = new List<List<object>>();
		m_fontCollection = new List<PdfFont>();
	}

	public IPdfCache Search(IPdfCache obj)
	{
		IPdfCache result = null;
		List<object> list = GetGroup(obj);
		if (list == null)
		{
			list = CreateNewGroup();
		}
		else if (list.Count > 0)
		{
			result = (IPdfCache)list[0];
		}
		list.Add(obj);
		return result;
	}

	public bool Contains(IPdfCache obj)
	{
		bool result = false;
		if (obj != null)
		{
			result = GetGroup(obj) != null;
		}
		return result;
	}

	public IPdfCache ContainsFont(IPdfCache obj)
	{
		for (int i = 0; i < FontCollection.Count; i++)
		{
			PdfTrueTypeFont pdfTrueTypeFont = FontCollection[i] as PdfTrueTypeFont;
			if (obj is PdfTrueTypeFont pdfTrueTypeFont2 && pdfTrueTypeFont != null && pdfTrueTypeFont2.IsEmbedFont == pdfTrueTypeFont.IsEmbedFont && pdfTrueTypeFont2.Unicode == pdfTrueTypeFont.Unicode && obj.EqualsTo(pdfTrueTypeFont))
			{
				return pdfTrueTypeFont;
			}
		}
		return null;
	}

	public void AddFont(PdfFont obj)
	{
		FontCollection.Add(obj);
	}

	public int GroupCount(IPdfCache obj)
	{
		int result = 0;
		if (obj != null)
		{
			List<object> group = GetGroup(obj);
			if (group != null)
			{
				result = group.Count;
			}
		}
		return result;
	}

	public void Remove(IPdfCache obj)
	{
		if (obj == null)
		{
			return;
		}
		List<object> group = GetGroup(obj);
		if (group != null)
		{
			group.Remove(obj);
			if (group.Count == 0)
			{
				RemoveGroup(group);
			}
		}
	}

	public void Clear()
	{
		if (m_referenceObjects != null)
		{
			int i = 0;
			for (int count = m_referenceObjects.Count; i < count; i++)
			{
				m_referenceObjects[i].Clear();
			}
			m_referenceObjects.Clear();
		}
	}

	private List<object> CreateNewGroup()
	{
		List<object> list = new List<object>();
		m_referenceObjects.Add(list);
		return list;
	}

	private List<object> GetGroup(IPdfCache result)
	{
		List<object> result2 = null;
		if (result != null)
		{
			int i = 0;
			for (int count = m_referenceObjects.Count; i < count; i++)
			{
				if (m_referenceObjects.Count <= 0)
				{
					continue;
				}
				List<object> list = m_referenceObjects[i];
				if (list.Count > 0)
				{
					IPdfCache pdfCache = (IPdfCache)list[0];
					if (result is PdfTrueTypeFont && pdfCache is PdfTrueTypeFont && (result as PdfTrueTypeFont).Unicode == (pdfCache as PdfTrueTypeFont).Unicode && result.EqualsTo(pdfCache))
					{
						result2 = list;
						break;
					}
				}
				else
				{
					RemoveGroup(list);
				}
			}
		}
		return result2;
	}

	private void RemoveGroup(List<object> group)
	{
		if (group != null)
		{
			m_referenceObjects.Remove(group);
		}
	}
}
