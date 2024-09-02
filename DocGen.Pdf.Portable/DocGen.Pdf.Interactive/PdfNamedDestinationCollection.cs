using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfNamedDestinationCollection : IEnumerable, IPdfWrapper
{
	private List<PdfNamedDestination> namedCollections = new List<PdfNamedDestination>();

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfCrossTable m_crossTable = new PdfCrossTable();

	internal int count;

	private PdfArray m_namedDestination = new PdfArray();

	private Dictionary<string, PdfNamedDestination> m_destCollection = new Dictionary<string, PdfNamedDestination>();

	public int Count => namedCollections.Count;

	public PdfNamedDestination this[int index]
	{
		get
		{
			if (index < 0 || index > Count - 1)
			{
				throw new ArgumentOutOfRangeException("index", "Index is out of range.");
			}
			return namedCollections[index];
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	internal PdfCrossTable CrossTable => m_crossTable;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfNamedDestinationCollection()
	{
		Initialize();
	}

	internal PdfNamedDestinationCollection(PdfDictionary dictionary, PdfCrossTable crossTable)
	{
		m_dictionary = dictionary;
		if (crossTable != null)
		{
			m_crossTable = crossTable;
		}
		if (Dictionary != null && Dictionary.ContainsKey("Dests"))
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(Dictionary["Dests"]) as PdfDictionary;
			if (pdfDictionary != null && pdfDictionary.ContainsKey("Names"))
			{
				AddCollection(pdfDictionary);
			}
			else if (pdfDictionary != null && pdfDictionary.ContainsKey("Kids") && PdfCrossTable.Dereference(pdfDictionary["Kids"]) is PdfArray pdfArray)
			{
				for (int i = 0; i < pdfArray.Count; i++)
				{
					FindDestination(PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary);
				}
			}
		}
		CrossTable.Document.Catalog.BeginSave += Dictionary_BeginSave;
		CrossTable.Document.Catalog.Modify();
	}

	public void Add(PdfNamedDestination namedDestination)
	{
		if (namedDestination == null)
		{
			throw new ArgumentNullException("The named destination value can't be null");
		}
		namedCollections.Add(namedDestination);
	}

	public bool Contains(PdfNamedDestination namedDestination)
	{
		if (namedDestination == null)
		{
			throw new ArgumentNullException("The named destination value can't be null");
		}
		return namedCollections.Contains(namedDestination);
	}

	public void Remove(string title)
	{
		if (title == null)
		{
			throw new ArgumentNullException("The title can't be null");
		}
		int index = -1;
		for (int i = 0; i < namedCollections.Count; i++)
		{
			if (namedCollections[i].Title.Equals(title))
			{
				index = i;
				break;
			}
		}
		RemoveAt(index);
	}

	public void RemoveAt(int index)
	{
		if (index >= namedCollections.Count)
		{
			throw new PdfException("The index value should not be greater than or equal to the count ");
		}
		namedCollections.RemoveAt(index);
	}

	public void Clear()
	{
		namedCollections.Clear();
	}

	public void Insert(int index, PdfNamedDestination namedDestination)
	{
		if (namedDestination == null)
		{
			throw new ArgumentNullException("The named destination value can't be null");
		}
		if (index < 0 || index > Count)
		{
			throw new IndexOutOfRangeException("The index can't be less then zero or greater then Count.");
		}
		namedCollections.Insert(index, namedDestination);
	}

	internal void Initialize()
	{
		Dictionary.BeginSave += Dictionary_BeginSave;
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		foreach (PdfNamedDestination namedCollection in namedCollections)
		{
			m_namedDestination.Add(new PdfString(namedCollection.Title));
			m_namedDestination.Add(new PdfReferenceHolder(namedCollection));
			m_destCollection[namedCollection.Title] = namedCollection;
		}
		bool flag = true;
		if (ars != null && ars.Writer != null && ars.Writer.Document != null && ars.Writer.Document.Catalog != null)
		{
			PdfCatalogNames names = ars.Writer.Document.Catalog.Names;
			if (names != null && names.m_dictionary.ContainsKey("Dests"))
			{
				flag = false;
			}
		}
		if (Dictionary.ContainsKey("Dests"))
		{
			PdfDictionary pdfDictionary = PdfCrossTable.Dereference(Dictionary["Dests"]) as PdfDictionary;
			if (pdfDictionary != null && !pdfDictionary.ContainsKey("Kids"))
			{
				ReviseNamedDestinationOrder();
				pdfDictionary.SetProperty("Names", new PdfReferenceHolder(m_namedDestination));
			}
			else if (pdfDictionary != null && pdfDictionary.ContainsKey("Kids") && pdfDictionary.ContainsKey("Limits"))
			{
				ReviseNamedDestinationOrder();
				pdfDictionary.Clear();
				pdfDictionary.SetProperty("Names", new PdfReferenceHolder(m_namedDestination));
			}
		}
		else if (flag)
		{
			ReviseNamedDestinationOrder();
			PdfDictionary pdfDictionary2 = new PdfDictionary();
			pdfDictionary2.SetProperty("Names", new PdfReferenceHolder(m_namedDestination));
			Dictionary.SetProperty("Dests", new PdfReferenceHolder(pdfDictionary2));
		}
		else
		{
			ReviseNamedDestinationOrder();
			Dictionary.SetProperty("Names", new PdfReferenceHolder(m_namedDestination));
		}
		if (m_destCollection.Count > 0)
		{
			m_destCollection.Clear();
		}
	}

	private void ReviseNamedDestinationOrder()
	{
		if (m_destCollection == null || m_destCollection.Count <= 0)
		{
			return;
		}
		System.StringComparer ordinal = System.StringComparer.Ordinal;
		List<string> list = new List<string>(m_destCollection.Keys);
		list.Sort(ordinal);
		m_namedDestination.Clear();
		foreach (string item in list)
		{
			m_namedDestination.Add(new PdfString(item));
			m_namedDestination.Add(new PdfReferenceHolder(m_destCollection[item]));
		}
		list.Clear();
	}

	private void FindDestination(PdfDictionary destination)
	{
		if (destination != null && destination.ContainsKey("Names"))
		{
			AddCollection(destination);
		}
		else if (destination != null && destination.ContainsKey("Kids") && PdfCrossTable.Dereference(destination["Kids"]) is PdfArray pdfArray)
		{
			for (int i = 0; i < pdfArray.Count; i++)
			{
				FindDestination(PdfCrossTable.Dereference(pdfArray[i]) as PdfDictionary);
			}
		}
	}

	private void AddCollection(PdfDictionary namedDictionary)
	{
		if (!(PdfCrossTable.Dereference(namedDictionary["Names"]) is PdfArray pdfArray))
		{
			return;
		}
		for (int i = 1; i <= pdfArray.Count; i += 2)
		{
			PdfReferenceHolder pdfReferenceHolder = pdfArray[i] as PdfReferenceHolder;
			PdfDictionary pdfDictionary = null;
			if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfArray)
			{
				pdfDictionary = new PdfDictionary();
				pdfDictionary.SetProperty("D", new PdfArray(pdfReferenceHolder.Object as PdfArray));
			}
			else if (pdfReferenceHolder == null && pdfArray[i] is PdfArray)
			{
				pdfDictionary = new PdfDictionary();
				PdfArray array = pdfArray[i] as PdfArray;
				pdfDictionary.SetProperty("D", new PdfArray(array));
			}
			else if (pdfReferenceHolder != null)
			{
				pdfDictionary = pdfReferenceHolder.Object as PdfDictionary;
			}
			else if (pdfDictionary == null && pdfReferenceHolder == null && pdfArray[i] is PdfString && pdfArray.Count > i + 1 && PdfCrossTable.Dereference(pdfArray[i + 1]) is PdfArray)
			{
				i++;
				pdfDictionary = new PdfDictionary();
				if (PdfCrossTable.Dereference(pdfArray[i]) is PdfArray array2)
				{
					pdfDictionary.SetProperty("D", new PdfArray(array2));
				}
			}
			if (pdfDictionary != null)
			{
				PdfLoadedNamedDestination pdfLoadedNamedDestination = new PdfLoadedNamedDestination(pdfDictionary, m_crossTable);
				if (pdfArray[i - 1] is PdfString pdfString)
				{
					pdfLoadedNamedDestination.Title = pdfString.Value;
				}
				namedCollections.Add(pdfLoadedNamedDestination);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return namedCollections.GetEnumerator();
	}
}
