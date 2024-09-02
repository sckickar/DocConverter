using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xmp;

namespace DocGen.Pdf;

public class CustomMetadata : IEnumerable
{
	private PdfDictionary m_dictionary;

	private XmpMetadata m_xmp;

	private Dictionary<string, string> m_customMetaDataDictionary = new Dictionary<string, string>();

	public string this[string key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key value should not be null");
			}
			if (key.Contains(" "))
			{
				key = key.Replace(" ", "#20");
			}
			return m_customMetaDataDictionary[key];
		}
		set
		{
			if (key != "Author" && key != "Title" && key != "Subject" && key != "Trapped" && key != "Keywords" && key != "Producer" && key != "CreationDate" && key != "ModDate" && key != "Creator")
			{
				m_customMetaDataDictionary[key] = value;
				Dictionary[key] = new PdfString(value);
				if (Xmp == null || Xmp.CustomSchema == null)
				{
					return;
				}
				if (Xmp.CustomSchema.CustomData.ContainsKey(key))
				{
					if (value != Xmp.CustomSchema[key])
					{
						m_xmp.CustomSchema[key] = value;
					}
				}
				else
				{
					m_xmp.CustomSchema[key] = value;
				}
				return;
			}
			throw new PdfException("The Custom property requires a unique name,which must not be on of the standard property names Title,Author,Subject,Keyword,Creator,Producer,CreationDate,ModDate and Trapped");
		}
	}

	public int Count => m_customMetaDataDictionary.Count;

	internal PdfDictionary Dictionary
	{
		get
		{
			return m_dictionary;
		}
		set
		{
			m_dictionary = value;
		}
	}

	internal XmpMetadata Xmp
	{
		get
		{
			return m_xmp;
		}
		set
		{
			m_xmp = value;
		}
	}

	public void Remove(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key value should not be null");
		}
		if (key.Contains(" "))
		{
			key = key.Replace(" ", "#20");
		}
		m_customMetaDataDictionary.Remove(key);
		Dictionary.Remove(key);
	}

	public bool ContainsKey(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key value should not be null");
		}
		if (key.Contains(" "))
		{
			key = key.Replace(" ", "#20");
		}
		if (m_customMetaDataDictionary.ContainsKey(key))
		{
			return true;
		}
		return false;
	}

	public void Add(string key, string value)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key value should not be null ");
		}
		if (value == null)
		{
			throw new ArgumentNullException("Value parmeter should not be null ");
		}
		m_customMetaDataDictionary[key] = value;
	}

	public IEnumerator GetEnumerator()
	{
		return m_customMetaDataDictionary.GetEnumerator();
	}
}
