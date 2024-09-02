using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.ColorSpace;

public class PdfIndexedColorSpace : PdfColorSpaces
{
	private PdfColorSpaces m_basecolorspace = new PdfDeviceColorSpace(PdfColorSpace.RGB);

	private int m_maxColorIndex;

	private byte[] m_indexedColorTable;

	private PdfStream m_stream = new PdfStream();

	public PdfColorSpaces BaseColorSpace
	{
		get
		{
			return m_basecolorspace;
		}
		set
		{
			m_basecolorspace = value;
			Initialize();
		}
	}

	public int MaxColorIndex
	{
		get
		{
			return m_maxColorIndex;
		}
		set
		{
			m_maxColorIndex = value;
			Initialize();
		}
	}

	public byte[] IndexedColorTable
	{
		get
		{
			return m_indexedColorTable;
		}
		set
		{
			m_indexedColorTable = value;
			Initialize();
		}
	}

	public PdfIndexedColorSpace()
	{
		m_stream.BeginSave += Stream_BeginSave;
		Initialize();
	}

	public byte[] GetProfileData()
	{
		_ = new byte[1000];
		return m_indexedColorTable;
	}

	protected void Save()
	{
		byte[] array = null;
		array = ((m_indexedColorTable != null) ? m_indexedColorTable : GetProfileData());
		m_stream.Clear();
		m_stream.InternalStream.Write(array, 0, array.Length);
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
			PdfName element = new PdfName("Indexed");
			pdfArray.Add(element);
			PdfReferenceHolder element2 = new PdfReferenceHolder(m_stream);
			if (m_basecolorspace != null)
			{
				if (m_basecolorspace is PdfCalGrayColorSpace)
				{
					PdfReferenceHolder element3 = new PdfReferenceHolder(m_basecolorspace);
					pdfArray.Add(element3);
				}
				else if (m_basecolorspace is PdfCalRGBColorSpace)
				{
					PdfReferenceHolder element4 = new PdfReferenceHolder(m_basecolorspace);
					pdfArray.Add(element4);
				}
				else if (m_basecolorspace is PdfLabColorSpace)
				{
					PdfReferenceHolder element5 = new PdfReferenceHolder(m_basecolorspace);
					pdfArray.Add(element5);
				}
				else if (m_basecolorspace is PdfDeviceColorSpace)
				{
					switch ((m_basecolorspace as PdfDeviceColorSpace).DeviceColorSpaceType.ToString())
					{
					case "RGB":
					{
						PdfName element8 = new PdfName("DeviceRGB");
						pdfArray.Add(element8);
						break;
					}
					case "CMYK":
					{
						PdfName element7 = new PdfName("DeviceCMYK");
						pdfArray.Add(element7);
						break;
					}
					case "GrayScale":
					{
						PdfName element6 = new PdfName("DeviceGray");
						pdfArray.Add(element6);
						break;
					}
					}
				}
				pdfArray.Add(new PdfNumber(m_maxColorIndex));
			}
			pdfArray.Add(element2);
		}
		return pdfArray;
	}

	private void Stream_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}
}
