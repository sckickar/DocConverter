using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.ColorSpace;

public class PdfCalGrayColorSpace : PdfColorSpaces, IPdfWrapper
{
	private double[] m_whitePoint = new double[3] { 0.9505, 1.0, 1.089 };

	private double m_gama = 1.0;

	private double[] m_blackPoint;

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

	public double Gamma
	{
		get
		{
			return m_gama;
		}
		set
		{
			m_gama = value;
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

	public PdfCalGrayColorSpace()
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
			PdfName element = new PdfName("CalGray");
			pdfArray.Add(element);
			PdfDictionary pdfDictionary = new PdfDictionary();
			pdfDictionary.SetProperty("WhitePoint", new PdfArray(m_whitePoint));
			pdfDictionary.SetProperty("Gamma", new PdfNumber(m_gama));
			if (m_blackPoint != null)
			{
				pdfDictionary.SetProperty("BlackPoint", new PdfArray(m_blackPoint));
			}
			pdfArray.Add(pdfDictionary);
		}
		return pdfArray;
	}
}
