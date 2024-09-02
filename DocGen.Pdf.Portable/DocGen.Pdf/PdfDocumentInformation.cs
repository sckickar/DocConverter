using System;
using System.Collections.Generic;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.Pdf.Xmp;

namespace DocGen.Pdf;

public class PdfDocumentInformation : IPdfWrapper
{
	private XmpMetadata m_xmp;

	private PdfCatalog m_catalog;

	private string m_author;

	private string m_title;

	private string m_subject;

	private string m_keywords;

	private string m_creator;

	private string m_producer;

	internal DateTime m_creationDate = DateTime.Now;

	internal DateTime m_modificationDate = DateTime.Now;

	internal PdfDictionary m_dictionary;

	internal bool ConformanceEnabled;

	private string m_arrayString = "";

	private string m_customValue = "";

	internal bool isRemoveModifyDate;

	internal bool isRemoveCreationDate;

	private CustomMetadata m_customMetadata;

	private ZugferdConformanceLevel m_zugferdConformanceLevel;

	private ZugferdVersion m_zugferdVersion;

	private string m_language;

	internal bool isConformanceCheck;

	private string m_label;

	internal bool m_autoTag;

	public DateTime CreationDate
	{
		get
		{
			if (!(m_dictionary["CreationDate"] is PdfString dateTimeStringValue))
			{
				return m_creationDate = DateTime.Now;
			}
			m_creationDate = m_dictionary.GetDateTime(dateTimeStringValue);
			return m_creationDate;
		}
		set
		{
			if (m_creationDate != value)
			{
				m_creationDate = value;
				m_dictionary.SetDateTime("CreationDate", m_creationDate);
			}
		}
	}

	public DateTime ModificationDate
	{
		get
		{
			if (!(m_dictionary["ModDate"] is PdfString dateTimeStringValue))
			{
				return m_modificationDate = DateTime.Now;
			}
			m_modificationDate = m_dictionary.GetDateTime(dateTimeStringValue);
			return m_modificationDate;
		}
		set
		{
			m_modificationDate = value;
			m_dictionary.SetDateTime("ModDate", m_modificationDate);
		}
	}

	public string Title
	{
		get
		{
			if (!(m_dictionary["Title"] is PdfString pdfString))
			{
				return m_title = string.Empty;
			}
			m_title = pdfString.Value.Replace("\0", string.Empty);
			return m_title;
		}
		set
		{
			if (value != null)
			{
				m_title = value;
				m_dictionary.SetString("Title", m_title);
			}
			UpdateMetadata();
		}
	}

	public string Author
	{
		get
		{
			if (!(m_dictionary["Author"] is PdfString pdfString))
			{
				return m_author = string.Empty;
			}
			m_author = pdfString.Value;
			return m_author;
		}
		set
		{
			if (value != null)
			{
				m_author = value;
				m_dictionary.SetString("Author", m_author);
			}
			UpdateMetadata();
			if (m_xmp != null && m_xmp.DublinCoreSchema != null)
			{
				m_xmp.DublinCoreSchema.Creator.Add(value);
			}
		}
	}

	public string Subject
	{
		get
		{
			if (!(m_dictionary["Subject"] is PdfString pdfString))
			{
				return m_subject = string.Empty;
			}
			m_subject = pdfString.Value;
			return m_subject;
		}
		set
		{
			if (value != null)
			{
				m_subject = value;
				m_dictionary.SetString("Subject", m_subject);
			}
			UpdateMetadata();
		}
	}

	public string Keywords
	{
		get
		{
			if (!(m_dictionary["Keywords"] is PdfString pdfString))
			{
				return m_keywords = string.Empty;
			}
			m_keywords = pdfString.Value;
			return m_keywords;
		}
		set
		{
			if (value != null)
			{
				m_keywords = value;
				m_dictionary.SetString("Keywords", m_keywords);
			}
			if (m_catalog != null && m_catalog.Metadata != null)
			{
				m_xmp = XmpMetadata;
			}
			UpdateMetadata();
		}
	}

	public string Creator
	{
		get
		{
			if (!(m_dictionary["Creator"] is PdfString pdfString))
			{
				return m_creator = string.Empty;
			}
			m_creator = pdfString.Value;
			return m_creator;
		}
		set
		{
			if (value != null)
			{
				m_creator = value;
				m_dictionary.SetString("Creator", m_creator);
			}
			UpdateMetadata();
		}
	}

	public string Producer
	{
		get
		{
			if (!(m_dictionary["Producer"] is PdfString pdfString))
			{
				return m_producer = string.Empty;
			}
			m_producer = pdfString.Value;
			return m_producer;
		}
		set
		{
			if (value != null)
			{
				m_producer = value;
				m_dictionary.SetString("Producer", m_producer);
			}
			UpdateMetadata();
		}
	}

	public XmpMetadata XmpMetadata
	{
		get
		{
			if (m_xmp == null)
			{
				if (m_catalog.Metadata == null)
				{
					if (m_catalog.LoadedDocument != null)
					{
						m_xmp = new XmpMetadata(m_catalog.LoadedDocument.DocumentInformation);
					}
					else
					{
						m_xmp = new XmpMetadata(m_catalog.Pages.Document.DocumentInformation);
					}
					if (!isConformanceCheck)
					{
						m_catalog.SetProperty("Metadata", new PdfReferenceHolder(m_xmp));
					}
				}
				else if (m_dictionary.Changed && !m_catalog.Changed)
				{
					m_xmp = new XmpMetadata(m_catalog.LoadedDocument.DocumentInformation);
					if (!isConformanceCheck)
					{
						m_catalog.SetProperty("Metadata", new PdfReferenceHolder(m_xmp));
					}
					if (m_customMetadata != null)
					{
						m_customMetadata.Xmp = m_xmp;
					}
				}
				else
				{
					m_xmp = m_catalog.Metadata;
					if (!isConformanceCheck)
					{
						m_catalog.SetProperty("Metadata", new PdfReferenceHolder(m_xmp));
					}
				}
			}
			else if (m_catalog.Metadata != null && m_catalog.LoadedDocument != null && m_dictionary.Changed && !m_catalog.Changed)
			{
				m_xmp = new XmpMetadata(m_catalog.LoadedDocument.DocumentInformation);
				if (!isConformanceCheck)
				{
					m_catalog.SetProperty("Metadata", new PdfReferenceHolder(m_xmp));
				}
			}
			if (m_customMetadata != null)
			{
				m_customMetadata.Xmp = m_xmp;
			}
			return m_xmp;
		}
	}

	public CustomMetadata CustomMetadata
	{
		get
		{
			if (m_customMetadata == null)
			{
				m_customMetadata = new CustomMetadata();
				m_customMetadata.Dictionary = m_dictionary;
				if (CustomMetadata.Dictionary != null)
				{
					new Dictionary<PdfName, IPdfPrimitive>();
					foreach (KeyValuePair<PdfName, IPdfPrimitive> item in new Dictionary<PdfName, IPdfPrimitive>(CustomMetadata.Dictionary.Items))
					{
						if (!(item.Key.Value != "Author") || !(item.Key.Value != "Title") || !(item.Key.Value != "Subject") || !(item.Key.Value != "Trapped") || !(item.Key.Value != "Keywords") || !(item.Key.Value != "Producer") || !(item.Key.Value != "CreationDate") || !(item.Key.Value != "ModDate") || !(item.Key.Value != "Creator"))
						{
							continue;
						}
						if (item.Value is PdfReferenceHolder)
						{
							if (!((item.Value as PdfReferenceHolder).Object is PdfArray pdfArray))
							{
								continue;
							}
							for (int i = 0; i < pdfArray.Count; i++)
							{
								if (pdfArray[i] is PdfString { Value: "0" } pdfString)
								{
									m_arrayString = pdfString.Value + m_arrayString;
								}
							}
							if (!m_arrayString.Equals(""))
							{
								m_customValue = m_arrayString;
							}
						}
						else if (item.Value is PdfString)
						{
							m_customValue = (item.Value as PdfString).Value;
							CustomMetadata[item.Key.Value] = m_customValue;
							if (XmpMetadata != null && XmpMetadata.CustomSchema != null)
							{
								XmpMetadata.CustomSchema[item.Key.Value] = m_customValue;
							}
						}
					}
				}
			}
			return m_customMetadata;
		}
		set
		{
			m_customMetadata = value;
			SetCustomDictionary(value);
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	internal ZugferdConformanceLevel ZugferdConformanceLevel
	{
		get
		{
			return m_zugferdConformanceLevel;
		}
		set
		{
			m_zugferdConformanceLevel = value;
		}
	}

	internal ZugferdVersion ZugferdVersion
	{
		get
		{
			return m_zugferdVersion;
		}
		set
		{
			m_zugferdVersion = value;
		}
	}

	public string Language
	{
		get
		{
			if (!(PdfCrossTable.Dereference(m_catalog["Lang"]) is PdfString pdfString))
			{
				return m_language = null;
			}
			m_language = pdfString.Value;
			return m_language;
		}
		set
		{
			if (value != null)
			{
				m_language = value;
				m_catalog["Lang"] = new PdfString(m_language);
			}
		}
	}

	internal string Label
	{
		get
		{
			return m_label;
		}
		set
		{
			if (value != null)
			{
				m_label = value;
			}
		}
	}

	internal PdfCatalog Catalog => m_catalog;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	internal PdfDocumentInformation(PdfCatalog catalog)
	{
		if (catalog == null)
		{
			throw new ArgumentNullException("catalog");
		}
		m_dictionary = new PdfDictionary();
		if (PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A1B && PdfDocument.ConformanceLevel != PdfConformanceLevel.Pdf_A1A && catalog.LoadedDocument == null)
		{
			m_dictionary.SetDateTime("CreationDate", m_creationDate);
		}
		m_catalog = catalog;
	}

	internal PdfDocumentInformation(PdfDictionary dictionary, PdfCatalog catalog)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException("dictionary");
		}
		if (catalog == null)
		{
			throw new ArgumentNullException("catalog");
		}
		m_dictionary = dictionary;
		m_catalog = catalog;
	}

	private void SetCustomDictionary(CustomMetadata value)
	{
		if (value == null || value.Dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<PdfName, IPdfPrimitive> item in value.Dictionary.Items)
		{
			if (item.Key.Value != "Author" && item.Key.Value != "Title" && item.Key.Value != "Subject" && item.Key.Value != "Trapped" && item.Key.Value != "Keywords" && item.Key.Value != "Producer" && item.Key.Value != "CreationDate" && item.Key.Value != "ModDate" && item.Key.Value != "Creator")
			{
				m_dictionary[item.Key] = item.Value;
			}
		}
	}

	internal void AddCustomMetaDataInfo(string metaDataName, string metaDataValue)
	{
		Dictionary[metaDataName] = new PdfString(metaDataValue);
		m_dictionary = Dictionary;
		if (CustomMetadata.ContainsKey(metaDataName))
		{
			if (CustomMetadata[metaDataName] != metaDataValue)
			{
				CustomMetadata[metaDataName] = metaDataValue;
			}
		}
		else
		{
			CustomMetadata[metaDataName] = metaDataValue;
		}
	}

	internal void ApplyPdfXConformance()
	{
		Dictionary["GTS_PDFXConformance"] = new PdfString("PDF/X-1a:2001");
		Dictionary["Trapped"] = new PdfName("False");
		Dictionary["GTS_PDFXVersion"] = new PdfString("PDF/X-1:2001");
		ModificationDate = DateTime.Now;
		if (Title == string.Empty)
		{
			Title = " ";
		}
	}

	public bool Remove(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new Exception("The key must not be null or empty.");
		}
		key = ((key == "ModificationDate") ? "ModDate" : key);
		if (m_dictionary != null && m_dictionary.ContainsKey(key))
		{
			m_dictionary.Remove(key);
			if (m_dictionary.Changed && m_catalog != null && m_catalog.LoadedDocument != null)
			{
				m_catalog.LoadedDocument.DocumentInformation.Dictionary.Remove(key);
				if (key == "ModDate")
				{
					m_catalog.LoadedDocument.DocumentInformation.isRemoveModifyDate = true;
				}
				else if (key == "CreationDate")
				{
					m_catalog.LoadedDocument.DocumentInformation.isRemoveCreationDate = true;
				}
				m_xmp = new XmpMetadata(m_catalog.LoadedDocument.DocumentInformation);
				m_catalog.SetProperty("Metadata", new PdfReferenceHolder(m_xmp));
			}
			return true;
		}
		return false;
	}

	public void RemoveModificationDate()
	{
		if (m_dictionary != null && m_dictionary.ContainsKey("ModDate"))
		{
			m_dictionary.Remove("ModDate");
			if (m_dictionary.Changed && !m_catalog.Changed)
			{
				m_catalog.LoadedDocument.DocumentInformation.Dictionary.Remove("ModDate");
				m_catalog.LoadedDocument.DocumentInformation.CustomMetadata.Remove("ModDate");
				m_catalog.LoadedDocument.DocumentInformation.isRemoveModifyDate = true;
				m_xmp = new XmpMetadata(m_catalog.LoadedDocument.DocumentInformation);
				m_catalog.SetProperty("Metadata", new PdfReferenceHolder(m_xmp));
			}
		}
	}

	private void UpdateMetadata()
	{
		if (m_xmp == null && m_catalog != null && m_catalog.Metadata != null && m_catalog.LoadedDocument.Conformance == PdfConformanceLevel.None)
		{
			_ = XmpMetadata;
		}
		else
		{
			ModificationDate = m_modificationDate;
		}
	}

	internal void ResetXmp()
	{
		m_xmp = null;
	}
}
