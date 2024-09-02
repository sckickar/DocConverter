using System;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfAppearanceState : IPdfWrapper
{
	private PdfTemplate m_on;

	private PdfTemplate m_off;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private string m_onMappingName = "Yes";

	private string m_offMappingName = "Off";

	public PdfTemplate On
	{
		get
		{
			return m_on;
		}
		set
		{
			if (m_on != value)
			{
				m_on = value;
			}
		}
	}

	public PdfTemplate Off
	{
		get
		{
			return m_off;
		}
		set
		{
			if (m_off != value)
			{
				m_off = value;
			}
		}
	}

	public string OnMappingName
	{
		get
		{
			return m_onMappingName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("OnMappingName");
			}
			m_onMappingName = value;
		}
	}

	public string OffMappingName
	{
		get
		{
			return m_offMappingName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("OffMappingName");
			}
			m_offMappingName = value;
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfAppearanceState()
	{
		m_dictionary.BeginSave += Dictionary_BeginSave;
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		if (m_on != null)
		{
			m_dictionary[PdfName.EncodeName(m_onMappingName)] = new PdfReferenceHolder(m_on);
		}
		if (m_off != null)
		{
			m_dictionary[PdfName.EncodeName(m_offMappingName)] = new PdfReferenceHolder(m_off);
		}
	}
}
