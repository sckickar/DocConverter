using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.ColorSpace;

public class PdfCalRGBColorSpace : PdfColorSpaces, IPdfWrapper
{
	private double[] m_whitePoint = new double[3] { 0.9505, 1.0, 1.089 };

	private double[] m_blackPoint;

	private double[] m_gama;

	private double[] m_matrix;

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

	public double[] Gamma
	{
		get
		{
			return m_gama;
		}
		set
		{
			if (value != null && value.Length != 3)
			{
				throw new ArgumentOutOfRangeException("Gamma", "Gamma array must have 3 values.");
			}
			m_gama = value;
			Initialize();
		}
	}

	public double[] Matrix
	{
		get
		{
			return m_matrix;
		}
		set
		{
			if (value != null && value.Length != 9)
			{
				throw new ArgumentOutOfRangeException("Matrix", "Matrix array must have 9 values.");
			}
			m_matrix = value;
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

	public PdfCalRGBColorSpace()
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
			PdfName element = new PdfName("CalRGB");
			pdfArray.Add(element);
			PdfDictionary pdfDictionary = new PdfDictionary();
			pdfDictionary.SetProperty("WhitePoint", new PdfArray(m_whitePoint));
			if (m_gama != null)
			{
				pdfDictionary.SetProperty("Gamma", new PdfArray(m_gama));
			}
			if (m_blackPoint != null)
			{
				pdfDictionary.SetProperty("BlackPoint", new PdfArray(m_blackPoint));
			}
			if (m_matrix != null)
			{
				pdfDictionary.SetProperty("Matrix", new PdfArray(m_matrix));
			}
			pdfArray.Add(pdfDictionary);
		}
		return pdfArray;
	}
}
