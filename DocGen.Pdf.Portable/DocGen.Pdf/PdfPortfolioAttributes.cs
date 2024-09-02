using System.Collections.Generic;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

public class PdfPortfolioAttributes : IPdfWrapper
{
	private PdfDictionary m_dictionary;

	private string[] m_attributeKeys;

	private Dictionary<string, string> m_attributes = new Dictionary<string, string>();

	private PdfPortfolioSchemaCollection m_schemaAttributes = new PdfPortfolioSchemaCollection();

	public string[] AttributesKey
	{
		get
		{
			string[] array = new string[m_attributes.Count];
			m_attributes.Keys.CopyTo(array, 0);
			return array;
		}
	}

	internal PdfPortfolioSchemaCollection SchemaAttributes
	{
		get
		{
			foreach (KeyValuePair<string, string> attribute in m_attributes)
			{
				m_schemaAttributes.Add(attribute.Key, attribute.Value);
			}
			return m_schemaAttributes;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfPortfolioAttributes()
	{
		Initialize();
	}

	internal PdfPortfolioAttributes(PdfDictionary dictionary)
	{
		if (m_dictionary != null)
		{
			return;
		}
		m_dictionary = dictionary;
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in dictionary.Items)
		{
			if (!(item.Key.Value == "Type"))
			{
				if (item.Value is PdfString)
				{
					m_attributes.Add(item.Key.Value, (item.Value as PdfString).Value);
				}
				else if (item.Value is PdfNumber)
				{
					m_attributes.Add(item.Key.Value, (item.Value as PdfNumber).FloatValue.ToString());
				}
			}
		}
	}

	private void Initialize()
	{
		m_dictionary = new PdfDictionary();
		m_dictionary.SetProperty("Type", new PdfName("CollectionItem"));
	}

	public void AddAttributes(string key, string value)
	{
		if (!m_attributes.ContainsKey(key) && !m_dictionary.ContainsKey(key))
		{
			m_attributes.Add(key, value);
			m_dictionary.SetProperty(key, new PdfString(value));
		}
	}

	public void RemoveAttributes(string key)
	{
		if (m_attributes.ContainsKey(key) && m_dictionary.ContainsKey(key))
		{
			m_attributes.Remove(key);
			m_dictionary.Remove(key);
		}
	}

	public PdfPortfolioSchemaCollection GetAttributes()
	{
		return SchemaAttributes;
	}
}
