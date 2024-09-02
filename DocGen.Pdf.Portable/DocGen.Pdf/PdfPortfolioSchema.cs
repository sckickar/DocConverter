using System.Collections.Generic;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfPortfolioSchema : IPdfWrapper
{
	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfPortfolioSchemaField m_schemaField;

	private string[] fieldkeys;

	private Dictionary<string, PdfPortfolioSchemaField> m_fieldCollections = new Dictionary<string, PdfPortfolioSchemaField>();

	public string[] FieldKeys
	{
		get
		{
			string[] array = new string[m_fieldCollections.Count];
			m_fieldCollections.Keys.CopyTo(array, 0);
			return array;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfPortfolioSchema()
	{
		Initialize();
	}

	internal PdfPortfolioSchema(PdfDictionary schemaDictionary)
	{
		m_dictionary = schemaDictionary;
		if (m_dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in m_dictionary.Items)
		{
			if (item.Key.Value == "Type")
			{
				continue;
			}
			PdfDictionary pdfDictionary = null;
			if (m_dictionary[item.Key] is PdfDictionary)
			{
				pdfDictionary = m_dictionary[item.Key] as PdfDictionary;
			}
			else if (m_dictionary[item.Key] is PdfReferenceHolder)
			{
				pdfDictionary = (m_dictionary[item.Key] as PdfReferenceHolder).Object as PdfDictionary;
			}
			if (pdfDictionary != null)
			{
				m_schemaField = new PdfPortfolioSchemaField(pdfDictionary);
				if (m_schemaField != null)
				{
					m_fieldCollections.Add(m_schemaField.Name, m_schemaField);
				}
			}
		}
	}

	public void AddSchemaField(PdfPortfolioSchemaField field)
	{
		if (!m_fieldCollections.ContainsKey(field.Name) && !m_dictionary.ContainsKey(field.Name))
		{
			m_fieldCollections.Add(field.Name, field);
			m_dictionary.SetProperty(field.Name, field);
		}
	}

	public void RemoveField(string key)
	{
		if (m_fieldCollections.ContainsKey(key) && m_dictionary.ContainsKey(key))
		{
			m_fieldCollections.Remove(key);
			m_dictionary.Remove(key);
		}
	}

	public Dictionary<string, PdfPortfolioSchemaField> GetSchemaField()
	{
		return m_fieldCollections;
	}

	private void Initialize()
	{
		m_dictionary.SetProperty("Type", new PdfName("CollectionSchema"));
	}
}
