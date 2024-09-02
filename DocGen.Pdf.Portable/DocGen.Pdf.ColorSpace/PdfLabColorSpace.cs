using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.ColorSpace;

public class PdfLabColorSpace : PdfColorSpaces, IPdfWrapper
{
	private double[] m_whitePoint = new double[3] { 0.9505, 1.0, 1.089 };

	private double[] m_blackPoint;

	private double[] m_range;

	public double[] BlackPoint
	{
		get
		{
			return m_blackPoint;
		}
		set
		{
			if (value != null && value.Length != 3)
			{
				throw new ArgumentOutOfRangeException("BlackPoint", "BlackPoint array must have 3 values.");
			}
			m_blackPoint = value;
			Initialize();
		}
	}

	public double[] Range
	{
		get
		{
			return m_range;
		}
		set
		{
			if (value != null && value.Length != 4)
			{
				throw new ArgumentOutOfRangeException("Range", "Range array must have 4 values.");
			}
			m_range = value;
			Initialize();
		}
	}

	public double[] WhitePoint
	{
		get
		{
			return m_whitePoint;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("WhitePoint", "WhitePoint array cannot be null.");
			}
			if (value.Length != 3)
			{
				throw new ArgumentOutOfRangeException("WhitePoint", "WhitePoint array must have 3 values.");
			}
			m_whitePoint = value;
			Initialize();
		}
	}

	public PdfLabColorSpace()
	{
		Initialize();
	}

	private void Initialize()
	{
		lock (PdfColorSpaces.s_syncObject)
		{
			IPdfCache pdfCache = PdfDocument.Cache.Search(this);
			IPdfPrimitive pdfPrimitive = null;
			pdfPrimitive = ((pdfCache != null) ? pdfCache.GetInternals() : CreateInternals());
			((IPdfCache)this).SetInternals(pdfPrimitive);
		}
	}

	private PdfArray CreateInternals()
	{
		PdfArray pdfArray = new PdfArray();
		if (pdfArray != null)
		{
			PdfName element = new PdfName("Lab");
			pdfArray.Add(element);
			PdfDictionary pdfDictionary = new PdfDictionary();
			pdfDictionary.SetProperty("WhitePoint", new PdfArray(m_whitePoint));
			if (m_blackPoint != null)
			{
				pdfDictionary.SetProperty("BlackPoint", new PdfArray(m_blackPoint));
			}
			if (m_range != null)
			{
				pdfDictionary.SetProperty("Range", new PdfArray(m_range));
			}
			pdfArray.Add(pdfDictionary);
		}
		return pdfArray;
	}
}
