using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf;

internal class EmbeddedFileParams : IPdfWrapper
{
	private DateTime m_creationDate = DateTime.Now;

	private DateTime m_modificationDate = DateTime.Now;

	private int m_size;

	private PdfDictionary m_dictionary = new PdfDictionary();

	public DateTime CreationDate
	{
		get
		{
			return m_creationDate;
		}
		set
		{
			m_creationDate = value;
			m_dictionary.SetDateTime("CreationDate", value);
		}
	}

	public DateTime ModificationDate
	{
		get
		{
			return m_modificationDate;
		}
		set
		{
			m_modificationDate = value;
			m_dictionary.SetDateTime("ModDate", value);
		}
	}

	internal int Size
	{
		get
		{
			return m_size;
		}
		set
		{
			if (m_size != value)
			{
				m_size = value;
				m_dictionary.SetNumber("Size", m_size);
			}
		}
	}

	public IPdfPrimitive Element => m_dictionary;

	public EmbeddedFileParams()
	{
		CreationDate = DateTime.Now;
		ModificationDate = DateTime.Now;
	}
}
