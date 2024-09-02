using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Functions;

public abstract class PdfFunction : IPdfWrapper
{
	private PdfDictionary m_dictionary;

	internal PdfArray Domain
	{
		get
		{
			return m_dictionary["Domain"] as PdfArray;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Domain");
			}
			m_dictionary.SetProperty("Domain", value);
		}
	}

	internal PdfArray Range
	{
		get
		{
			return m_dictionary["Range"] as PdfArray;
		}
		set
		{
			m_dictionary.SetProperty("Range", value);
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	internal PdfFunction(PdfDictionary dic)
	{
		m_dictionary = dic;
	}
}
