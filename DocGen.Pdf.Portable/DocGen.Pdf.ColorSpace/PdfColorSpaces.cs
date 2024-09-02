using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.ColorSpace;

public abstract class PdfColorSpaces : IPdfWrapper, IPdfCache
{
	internal PdfResources resources;

	protected static object s_syncObject = new object();

	private IPdfPrimitive m_colorInternals;

	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfArray colorspace = new PdfArray();

	IPdfPrimitive IPdfWrapper.Element => m_colorInternals;

	bool IPdfCache.EqualsTo(IPdfCache obj)
	{
		return false;
	}

	IPdfPrimitive IPdfCache.GetInternals()
	{
		return m_colorInternals;
	}

	void IPdfCache.SetInternals(IPdfPrimitive internals)
	{
		if (internals == null)
		{
			throw new ArgumentNullException("internals");
		}
		m_colorInternals = internals;
	}
}
