using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Pdf.IO;
using DocGen.Pdf.Parsing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfBookmarkBase : IPdfWrapper, IEnumerable
{
	private List<PdfBookmarkBase> m_list = new List<PdfBookmarkBase>();

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfCrossTable m_crossTable = new PdfCrossTable();

	private List<PdfBookmark> bookmark;

	private List<PdfBookmarkBase> m_booklist;

	private bool m_isExpanded;

	private int parentIndex = -1;

	internal List<long> m_bookmarkReference = new List<long>();

	public int Count
	{
		get
		{
			if (m_crossTable.Document is PdfLoadedDocument)
			{
				if (m_booklist == null)
				{
					m_booklist = new List<PdfBookmarkBase>();
					for (int i = 0; i < List.Count; i++)
					{
						m_booklist.Add(List[i]);
					}
				}
				return List.Count;
			}
			return List.Count;
		}
	}

	public PdfBookmark this[int index] => List[index] as PdfBookmark;

	internal virtual List<PdfBookmarkBase> List => m_list;

	internal PdfDictionary Dictionary => m_dictionary;

	internal PdfCrossTable CrossTable => m_crossTable;

	internal bool IsExpanded
	{
		get
		{
			if (Dictionary.ContainsKey("Count"))
			{
				if ((Dictionary["Count"] as PdfNumber).IntValue < 0)
				{
					return false;
				}
				return true;
			}
			return m_isExpanded;
		}
		set
		{
			m_isExpanded = value;
			if (Count > 0)
			{
				int value2 = ((!m_isExpanded) ? (-List.Count) : List.Count);
				m_dictionary.SetNumber("Count", value2);
			}
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	internal PdfBookmarkBase()
	{
	}

	internal PdfBookmarkBase(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		m_dictionary = dictionary;
		if (crossTable != null)
		{
			m_crossTable = crossTable;
		}
	}

	public PdfBookmark Add(string title)
	{
		if (title == null)
		{
			throw new ArgumentNullException("title");
		}
		PdfBookmark pdfBookmark = ((Count < 1) ? null : this[Count - 1]);
		PdfBookmark pdfBookmark2 = new PdfBookmark(title, this, pdfBookmark, null);
		if (pdfBookmark != null)
		{
			pdfBookmark.Next = pdfBookmark2;
		}
		List.Add(pdfBookmark2);
		UpdateFields();
		return pdfBookmark2;
	}

	public bool Contains(PdfBookmark outline)
	{
		return List.Contains(outline);
	}

	public void Remove(string title)
	{
		if (title == null)
		{
			throw new ArgumentNullException("title");
		}
		int index = -1;
		if (bookmark == null || bookmark.Count < List.Count)
		{
			bookmark = new List<PdfBookmark>();
			Dictionary<PdfPageBase, object> dictionary = null;
			if (m_crossTable.Document is PdfLoadedDocument)
			{
				dictionary = (m_crossTable.Document as PdfLoadedDocument).CreateBookmarkDestinationDictionary();
			}
			for (int i = 0; i < List.Count; i++)
			{
				if (!(List[i] is PdfBookmark))
				{
					throw new Exception("bookmark");
				}
				bookmark.Add(List[i] as PdfBookmark);
			}
			for (int j = 0; j < List.Count; j++)
			{
				if (List[j] is PdfBookmark && List[j].List != null && List[j].List.Count > 0)
				{
					if (!(List[j] is PdfBookmark))
					{
						throw new Exception("bookmark");
					}
					if (List[j].List != null && List[j].List.Count > 0)
					{
						ChildBookmark(List[j].List);
					}
					bookmark.Add(List[j] as PdfBookmark);
				}
			}
			if (dictionary != null)
			{
				foreach (List<object> value in dictionary.Values)
				{
					foreach (PdfBookmark item in value)
					{
						bookmark.Add(item);
					}
				}
			}
			if (m_booklist == null || m_booklist.Count < List.Count)
			{
				m_booklist = new List<PdfBookmarkBase>();
				for (int k = 0; k < bookmark.Count; k++)
				{
					m_booklist.Add(bookmark[k]);
				}
			}
		}
		else
		{
			for (int l = 0; l < List.Count; l++)
			{
				if (!(List[l] is PdfBookmark))
				{
					throw new Exception("bookmark");
				}
				if (!bookmark.Contains(List[l] as PdfBookmark))
				{
					bookmark.Add(List[l] as PdfBookmark);
					m_list.RemoveAt(l);
				}
			}
		}
		for (int m = 0; m < bookmark.Count; m++)
		{
			if (bookmark[m] is PdfLoadedBookmark)
			{
				if ((bookmark[m] as PdfLoadedBookmark).Title.Equals(title))
				{
					index = m;
					break;
				}
			}
			else if (bookmark[m] != null && bookmark[m].Title.Equals(title))
			{
				index = m;
				break;
			}
		}
		for (int n = 0; n < m_booklist.Count; n++)
		{
			PdfLoadedBookmark pdfLoadedBookmark = m_booklist[n] as PdfLoadedBookmark;
			PdfBookmark pdfBookmark = m_booklist[n] as PdfBookmark;
			if (pdfLoadedBookmark != null)
			{
				if (pdfLoadedBookmark.Title.Equals(title))
				{
					parentIndex = n;
					break;
				}
			}
			else if (pdfBookmark != null && pdfBookmark.Title.Equals(title))
			{
				parentIndex = n;
				break;
			}
		}
		int count = m_booklist.Count;
		RemoveAt(index);
		if (parentIndex < m_list.Count && parentIndex < m_booklist.Count && count == m_booklist.Count)
		{
			m_booklist.RemoveAt(parentIndex);
			m_list.RemoveAt(parentIndex);
		}
	}

	private void ChildBookmark(List<PdfBookmarkBase> pdfBookmarkList)
	{
		for (int i = 0; i < pdfBookmarkList.Count; i++)
		{
			if (pdfBookmarkList[i].List != null && pdfBookmarkList[i].List.Count > 0)
			{
				ChildBookmark(pdfBookmarkList[i].List);
			}
			bookmark.Add(pdfBookmarkList[i] as PdfBookmark);
		}
	}

	public void RemoveAt(int index)
	{
		if (bookmark == null || bookmark.Count < List.Count)
		{
			bookmark = new List<PdfBookmark>();
			Dictionary<PdfPageBase, object> dictionary = null;
			for (int i = 0; i < List.Count; i++)
			{
				if (!(List[i] is PdfBookmark))
				{
					throw new Exception("bookmark");
				}
				bookmark.Add(List[i] as PdfBookmark);
			}
			if (dictionary != null)
			{
				foreach (List<object> value in dictionary.Values)
				{
					foreach (PdfBookmark item in value)
					{
						if (!bookmark.Contains(item))
						{
							bookmark.Add(item);
						}
					}
				}
			}
			if (m_booklist == null || m_booklist.Count < List.Count)
			{
				m_booklist = new List<PdfBookmarkBase>();
				for (int j = 0; j < bookmark.Count; j++)
				{
					m_booklist.Add(bookmark[j]);
				}
			}
		}
		if (index < 0 || index >= bookmark.Count)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (index >= List.Count && index >= bookmark.Count)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (bookmark[index] != null)
		{
			PdfBookmark pdfBookmark = bookmark[index];
			if (index == 0)
			{
				if (pdfBookmark.Dictionary.ContainsKey("Next"))
				{
					m_dictionary.SetProperty("First", pdfBookmark.Dictionary["Next"]);
				}
				else if (!pdfBookmark.Dictionary.ContainsKey("Prev"))
				{
					if (List.Count > 1)
					{
						m_dictionary.SetProperty("First", pdfBookmark.Dictionary["First"]);
					}
					else
					{
						m_dictionary.Remove("First");
						m_dictionary.Remove("Last");
					}
				}
				else
				{
					m_dictionary.SetProperty("First", pdfBookmark.Dictionary["Next"]);
				}
			}
			else if (pdfBookmark.Parent != null && pdfBookmark.Previous == null && pdfBookmark.Next != null)
			{
				pdfBookmark.Parent.Dictionary.SetProperty("First", pdfBookmark.Dictionary["Next"]);
				pdfBookmark.Next.Dictionary.Remove("Prev");
			}
			else if (pdfBookmark.Parent != null && pdfBookmark.Previous != null && pdfBookmark.Next != null)
			{
				if (pdfBookmark.Dictionary["Prev"] is PdfReferenceHolder)
				{
					((pdfBookmark.Dictionary["Prev"] as PdfReferenceHolder).Object as PdfDictionary).SetProperty("Next", pdfBookmark.Dictionary["Next"]);
				}
				else
				{
					pdfBookmark.Previous.Dictionary.SetProperty("Next", pdfBookmark.Dictionary["Next"]);
				}
				PdfReferenceHolder pdfReferenceHolder = pdfBookmark.Dictionary["Next"] as PdfReferenceHolder;
				if (pdfReferenceHolder != null && m_crossTable.CrossTable != null)
				{
					(m_crossTable.GetObject(pdfReferenceHolder) as PdfDictionary).SetProperty("Prev", pdfBookmark.Dictionary["Prev"]);
				}
			}
			else if (pdfBookmark.Parent != null && pdfBookmark.Previous != null && pdfBookmark.Next == null)
			{
				if (pdfBookmark.Dictionary["Prev"] is PdfReferenceHolder)
				{
					((pdfBookmark.Dictionary["Prev"] as PdfReferenceHolder).Object as PdfDictionary).Remove("Next");
				}
				else
				{
					pdfBookmark.Previous.Dictionary.Remove("Next");
				}
				pdfBookmark.Parent.Dictionary.SetProperty("Last", pdfBookmark.Dictionary["Prev"]);
			}
			else
			{
				pdfBookmark.Parent.Dictionary.Remove("First");
			}
		}
		else if (bookmark[index] is PdfLoadedBookmark)
		{
			PdfLoadedBookmark pdfLoadedBookmark = bookmark[index] as PdfLoadedBookmark;
			if (index == 0)
			{
				if (pdfLoadedBookmark.Dictionary.ContainsKey("Next"))
				{
					m_dictionary.SetProperty("First", pdfLoadedBookmark.Dictionary["Next"]);
				}
				else if (!pdfLoadedBookmark.Dictionary.ContainsKey("Prev"))
				{
					if (List.Count > 1)
					{
						m_dictionary.SetProperty("First", pdfLoadedBookmark.Dictionary["First"]);
					}
					else
					{
						m_dictionary.Remove("First");
						m_dictionary.Remove("Last");
					}
				}
				else
				{
					m_dictionary.SetProperty("First", pdfLoadedBookmark.Dictionary["Next"]);
				}
			}
			else if (pdfLoadedBookmark.Parent != null && pdfLoadedBookmark.Previous == null && pdfLoadedBookmark.Next != null)
			{
				pdfLoadedBookmark.Parent.Dictionary.SetProperty("First", pdfLoadedBookmark.Dictionary["Next"]);
				pdfLoadedBookmark.Next.Dictionary.Remove("Prev");
			}
			else if (pdfLoadedBookmark.Parent != null && pdfLoadedBookmark.Previous != null && pdfLoadedBookmark.Next != null)
			{
				if (pdfLoadedBookmark.Dictionary["Prev"] is PdfReferenceHolder)
				{
					((pdfLoadedBookmark.Dictionary["Prev"] as PdfReferenceHolder).Object as PdfDictionary).SetProperty("Next", pdfLoadedBookmark.Dictionary["Next"]);
				}
				else
				{
					pdfLoadedBookmark.Previous.Dictionary.SetProperty("Next", pdfLoadedBookmark.Dictionary["Next"]);
				}
				PdfReferenceHolder pdfReferenceHolder2 = pdfLoadedBookmark.Dictionary["Next"] as PdfReferenceHolder;
				if (pdfReferenceHolder2 != null)
				{
					(m_crossTable.GetObject(pdfReferenceHolder2) as PdfDictionary).SetProperty("Prev", pdfLoadedBookmark.Dictionary["Prev"]);
				}
			}
			else if (pdfLoadedBookmark.Parent != null && pdfLoadedBookmark.Previous != null && pdfLoadedBookmark.Next == null)
			{
				if (pdfLoadedBookmark.Dictionary["Prev"] is PdfReferenceHolder)
				{
					((pdfLoadedBookmark.Dictionary["Prev"] as PdfReferenceHolder).Object as PdfDictionary).Remove("Next");
				}
				else
				{
					pdfLoadedBookmark.Previous.Dictionary.Remove("Next");
				}
				pdfLoadedBookmark.Parent.Dictionary.SetProperty("Last", pdfLoadedBookmark.Dictionary["Prev"]);
			}
			else
			{
				pdfLoadedBookmark.Parent.Dictionary.Remove("First");
			}
		}
		if (index < m_list.Count && index < m_booklist.Count)
		{
			m_list.RemoveAt(index);
			m_booklist.RemoveAt(index);
		}
		UpdateFields();
		bookmark.RemoveAt(index);
	}

	public void Clear()
	{
		if (CrossTable != null && CrossTable.Document != null)
		{
			for (int num = Count - 1; num >= 0; num--)
			{
				RemoveAt(num);
			}
		}
		List.Clear();
		if (m_booklist != null)
		{
			m_booklist.Clear();
		}
	}

	public PdfBookmark Insert(int index, string title)
	{
		if (title == null)
		{
			throw new ArgumentNullException("title");
		}
		if (index < 0 || index > Count)
		{
			throw new IndexOutOfRangeException();
		}
		if (title == null)
		{
			throw new ArgumentNullException("title");
		}
		PdfBookmark pdfBookmark;
		if (index == Count)
		{
			pdfBookmark = Add(title);
		}
		else
		{
			PdfBookmark pdfBookmark2 = this[index];
			PdfBookmark pdfBookmark3 = ((index == 0) ? null : this[index - 1]);
			pdfBookmark = new PdfBookmark(title, this, pdfBookmark3, pdfBookmark2);
			List.Insert(index, pdfBookmark);
			if (pdfBookmark3 != null)
			{
				pdfBookmark3.Next = pdfBookmark;
			}
			pdfBookmark2.Previous = pdfBookmark;
			UpdateFields();
		}
		return pdfBookmark;
	}

	private void GetBookmarkCollection(List<PdfBookmark> pageBookmarks, List<PdfBookmark> bookmarks)
	{
		if (pageBookmarks == null)
		{
			return;
		}
		foreach (PdfBookmark pageBookmark in pageBookmarks)
		{
			bookmarks.Add(pageBookmark as PdfBookmark);
		}
	}

	internal void Dispose()
	{
		List.Clear();
		if (m_booklist != null)
		{
			m_booklist.Clear();
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return List.GetEnumerator();
	}

	internal void ReproduceTree()
	{
		PdfLoadedBookmark pdfLoadedBookmark = GetFirstBookMark(this);
		for (bool flag = pdfLoadedBookmark != null; flag && pdfLoadedBookmark.Dictionary != null; flag = pdfLoadedBookmark != null)
		{
			pdfLoadedBookmark.SetParent(this);
			m_list.Add(pdfLoadedBookmark);
			pdfLoadedBookmark = pdfLoadedBookmark.Next as PdfLoadedBookmark;
		}
	}

	private void UpdateFields()
	{
		if (Count > 0)
		{
			int value = ((!IsExpanded && Dictionary.ContainsKey("Count")) ? (-List.Count) : List.Count);
			m_dictionary.SetNumber("Count", value);
			m_dictionary.SetProperty("First", new PdfReferenceHolder(this[0]));
			m_dictionary.SetProperty("Last", new PdfReferenceHolder(this[Count - 1]));
		}
		m_dictionary.Modify();
	}

	private PdfLoadedBookmark GetFirstBookMark(PdfBookmarkBase bookmark)
	{
		PdfLoadedBookmark result = null;
		PdfDictionary dictionary = bookmark.Dictionary;
		if (dictionary.ContainsKey("First"))
		{
			result = new PdfLoadedBookmark(CrossTable.GetObject(dictionary["First"]) as PdfDictionary, CrossTable);
		}
		return result;
	}
}
