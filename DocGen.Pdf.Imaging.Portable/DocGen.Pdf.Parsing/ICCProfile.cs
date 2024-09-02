using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class ICCProfile
{
	private int m_n;

	private Colorspace m_alternateColorspace;

	private PdfArray m_iccProfileValue;

	private PdfDictionary m_alternateColorspaceDictionary;

	internal Colorspace AlternateColorspace
	{
		get
		{
			if (m_alternateColorspace == null)
			{
				m_alternateColorspace = GetAlternateColorSpace();
			}
			return m_alternateColorspace;
		}
	}

	internal int N
	{
		get
		{
			return m_n;
		}
		set
		{
			m_n = value;
		}
	}

	public ICCProfile()
	{
	}

	public ICCProfile(PdfArray array)
	{
		m_iccProfileValue = array;
		SetValue(array[1] as PdfReferenceHolder);
	}

	private Colorspace GetAlternateColorSpace()
	{
		return N switch
		{
			1 => new DeviceGray(), 
			3 => new DeviceRGB(), 
			4 => new DeviceCMYK(), 
			_ => throw new NotSupportedException(), 
		};
	}

	private void SetValue(PdfReferenceHolder pdfReference)
	{
		if (pdfReference != null && pdfReference.Object is PdfDictionary)
		{
			m_alternateColorspaceDictionary = pdfReference.Object as PdfDictionary;
			if (m_alternateColorspaceDictionary.ContainsKey("N"))
			{
				m_n = (m_alternateColorspaceDictionary["N"] as PdfNumber).IntValue;
			}
		}
	}
}
