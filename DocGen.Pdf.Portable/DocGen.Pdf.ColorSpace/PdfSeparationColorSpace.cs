using System;
using DocGen.Pdf.Functions;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.ColorSpace;

public class PdfSeparationColorSpace : PdfColorSpaces, IPdfWrapper
{
	private string m_colorant;

	private PdfFunction m_function;

	private PdfStream m_stream = new PdfStream();

	private PdfColorSpaces m_alterantecolorspaces = new PdfDeviceColorSpace(PdfColorSpace.CMYK);

	public PdfColorSpaces AlternateColorSpaces
	{
		get
		{
			return m_alterantecolorspaces;
		}
		set
		{
			m_alterantecolorspaces = value;
			Initialize();
		}
	}

	public string Colorant
	{
		get
		{
			return m_colorant;
		}
		set
		{
			m_colorant = value;
			Initialize();
		}
	}

	public PdfFunction TintTransform
	{
		get
		{
			return m_function;
		}
		set
		{
			m_function = value;
			Initialize();
		}
	}

	public PdfSeparationColorSpace()
	{
		m_stream.Compress = true;
		m_stream.SetProperty("Filter", new PdfName("FlateDecode"));
		m_stream.BeginSave += Stream_BeginSave;
		Initialize();
	}

	public byte[] GetProfileData()
	{
		return new byte[0];
	}

	protected void Save()
	{
		byte[] array = null;
		array = GetProfileData();
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
			PdfName element = new PdfName("Separation");
			pdfArray.Add(element);
			if (m_colorant != null)
			{
				PdfName element2 = new PdfName(m_colorant);
				pdfArray.Add(element2);
			}
			else
			{
				PdfName element3 = new PdfName("All");
				pdfArray.Add(element3);
			}
			if (m_alterantecolorspaces != null)
			{
				if (m_alterantecolorspaces is PdfCalGrayColorSpace)
				{
					new PdfName("CalGray");
					PdfReferenceHolder element4 = new PdfReferenceHolder(m_alterantecolorspaces);
					pdfArray.Add(element4);
				}
				else if (m_alterantecolorspaces is PdfCalRGBColorSpace)
				{
					new PdfName("CalRGB");
					PdfReferenceHolder element5 = new PdfReferenceHolder(m_alterantecolorspaces);
					pdfArray.Add(element5);
				}
				else if (m_alterantecolorspaces is PdfLabColorSpace)
				{
					new PdfName("Lab");
					PdfReferenceHolder element6 = new PdfReferenceHolder(m_alterantecolorspaces);
					pdfArray.Add(element6);
				}
				else if (m_alterantecolorspaces is PdfDeviceColorSpace)
				{
					switch ((m_alterantecolorspaces as PdfDeviceColorSpace).DeviceColorSpaceType.ToString())
					{
					case "RGB":
					{
						PdfName element9 = new PdfName("DeviceRGB");
						pdfArray.Add(element9);
						break;
					}
					case "CMYK":
					{
						PdfName element8 = new PdfName("DeviceCMYK");
						pdfArray.Add(element8);
						break;
					}
					case "GrayScale":
					{
						PdfName element7 = new PdfName("DeviceGray");
						pdfArray.Add(element7);
						break;
					}
					}
				}
			}
			else
			{
				PdfName element10 = new PdfName("DeviceCMYK");
				pdfArray.Add(element10);
			}
			if (m_function != null)
			{
				if (m_alterantecolorspaces is PdfCalGrayColorSpace)
				{
					PdfExponentialInterpolationFunction pdfExponentialInterpolationFunction = m_function as PdfExponentialInterpolationFunction;
					pdfExponentialInterpolationFunction.Dictionary.SetProperty("FunctionType", new PdfNumber(2));
					pdfExponentialInterpolationFunction.Dictionary.SetProperty("Domain", new PdfArray(new double[2] { 0.0, 1.0 }));
					pdfExponentialInterpolationFunction.Dictionary.SetProperty("Range", new PdfArray(new double[2] { 0.0, 1.0 }));
					pdfExponentialInterpolationFunction.Dictionary.SetProperty("C0", new PdfArray(new double[1]));
					if (pdfExponentialInterpolationFunction.C1.Length != 1)
					{
						throw new ArgumentOutOfRangeException();
					}
					pdfExponentialInterpolationFunction.Dictionary.SetProperty("C1", new PdfArray(pdfExponentialInterpolationFunction.C1));
					pdfExponentialInterpolationFunction.Dictionary.SetProperty("N", new PdfNumber(1));
					PdfReferenceHolder element11 = new PdfReferenceHolder(pdfExponentialInterpolationFunction);
					pdfArray.Add(element11);
				}
				else if (m_alterantecolorspaces is PdfCalRGBColorSpace)
				{
					PdfExponentialInterpolationFunction pdfExponentialInterpolationFunction2 = m_function as PdfExponentialInterpolationFunction;
					pdfExponentialInterpolationFunction2.Dictionary.SetProperty("FunctionType", new PdfNumber(2));
					pdfExponentialInterpolationFunction2.Dictionary.SetProperty("Domain", new PdfArray(new double[2] { 0.0, 1.0 }));
					pdfExponentialInterpolationFunction2.Dictionary.SetProperty("Range", new PdfArray(new double[6] { 0.0, 1.0, 0.0, 1.0, 0.0, 1.0 }));
					pdfExponentialInterpolationFunction2.Dictionary.SetProperty("C0", new PdfArray(new double[3]));
					if (pdfExponentialInterpolationFunction2.C1.Length != 3)
					{
						throw new ArgumentOutOfRangeException();
					}
					pdfExponentialInterpolationFunction2.Dictionary.SetProperty("C1", new PdfArray(pdfExponentialInterpolationFunction2.C1));
					pdfExponentialInterpolationFunction2.Dictionary.SetProperty("N", new PdfNumber(1));
					PdfReferenceHolder element12 = new PdfReferenceHolder(pdfExponentialInterpolationFunction2);
					pdfArray.Add(element12);
				}
				else if (m_alterantecolorspaces is PdfLabColorSpace)
				{
					PdfExponentialInterpolationFunction pdfExponentialInterpolationFunction3 = m_function as PdfExponentialInterpolationFunction;
					pdfExponentialInterpolationFunction3.Dictionary.SetProperty("FunctionType", new PdfNumber(2));
					pdfExponentialInterpolationFunction3.Dictionary.SetProperty("Domain", new PdfArray(new double[2] { 0.0, 1.0 }));
					pdfExponentialInterpolationFunction3.Dictionary.SetProperty("Range", new PdfArray(new double[6] { 0.0, 100.0, 0.0, 100.0, 0.0, 100.0 }));
					pdfExponentialInterpolationFunction3.Dictionary.SetProperty("C0", new PdfArray(new double[3]));
					if (pdfExponentialInterpolationFunction3.C1.Length != 3)
					{
						throw new ArgumentOutOfRangeException();
					}
					pdfExponentialInterpolationFunction3.Dictionary.SetProperty("C1", new PdfArray(pdfExponentialInterpolationFunction3.C1));
					pdfExponentialInterpolationFunction3.Dictionary.SetProperty("N", new PdfNumber(1));
					PdfReferenceHolder element13 = new PdfReferenceHolder(pdfExponentialInterpolationFunction3);
					pdfArray.Add(element13);
				}
				else if (m_alterantecolorspaces is PdfDeviceColorSpace)
				{
					switch ((m_alterantecolorspaces as PdfDeviceColorSpace).DeviceColorSpaceType.ToString())
					{
					case "RGB":
					{
						PdfExponentialInterpolationFunction pdfExponentialInterpolationFunction6 = m_function as PdfExponentialInterpolationFunction;
						pdfExponentialInterpolationFunction6.Dictionary.SetProperty("FunctionType", new PdfNumber(2));
						pdfExponentialInterpolationFunction6.Dictionary.SetProperty("Domain", new PdfArray(new double[2] { 0.0, 1.0 }));
						pdfExponentialInterpolationFunction6.Dictionary.SetProperty("Range", new PdfArray(new double[6] { 0.0, 1.0, 0.0, 1.0, 0.0, 1.0 }));
						pdfExponentialInterpolationFunction6.Dictionary.SetProperty("C0", new PdfArray(new double[3]));
						if (pdfExponentialInterpolationFunction6.C1.Length == 3)
						{
							pdfExponentialInterpolationFunction6.Dictionary.SetProperty("C1", new PdfArray(pdfExponentialInterpolationFunction6.C1));
							pdfExponentialInterpolationFunction6.Dictionary.SetProperty("N", new PdfNumber(1));
							PdfReferenceHolder element16 = new PdfReferenceHolder(pdfExponentialInterpolationFunction6);
							pdfArray.Add(element16);
							break;
						}
						throw new ArgumentOutOfRangeException();
					}
					case "CMYK":
					{
						PdfExponentialInterpolationFunction pdfExponentialInterpolationFunction5 = m_function as PdfExponentialInterpolationFunction;
						pdfExponentialInterpolationFunction5.Dictionary.SetProperty("FunctionType", new PdfNumber(2));
						pdfExponentialInterpolationFunction5.Dictionary.SetProperty("C0", new PdfArray(pdfExponentialInterpolationFunction5.C0));
						if (pdfExponentialInterpolationFunction5.C1.Length == 4)
						{
							pdfExponentialInterpolationFunction5.Dictionary.SetProperty("C1", new PdfArray(pdfExponentialInterpolationFunction5.C1));
							pdfExponentialInterpolationFunction5.Dictionary.SetProperty("N", new PdfNumber(1));
							PdfReferenceHolder element15 = new PdfReferenceHolder(pdfExponentialInterpolationFunction5);
							pdfArray.Add(element15);
							break;
						}
						throw new ArgumentOutOfRangeException();
					}
					case "GrayScale":
					{
						PdfExponentialInterpolationFunction pdfExponentialInterpolationFunction4 = m_function as PdfExponentialInterpolationFunction;
						pdfExponentialInterpolationFunction4.Dictionary.SetProperty("FunctionType", new PdfNumber(2));
						pdfExponentialInterpolationFunction4.Dictionary.SetProperty("Domain", new PdfArray(new double[2] { 0.0, 1.0 }));
						pdfExponentialInterpolationFunction4.Dictionary.SetProperty("Range", new PdfArray(new double[2] { 0.0, 1.0 }));
						pdfExponentialInterpolationFunction4.Dictionary.SetProperty("C0", new PdfArray(new double[1]));
						if (pdfExponentialInterpolationFunction4.C1.Length == 1)
						{
							pdfExponentialInterpolationFunction4.Dictionary.SetProperty("C1", new PdfArray(pdfExponentialInterpolationFunction4.C1));
							pdfExponentialInterpolationFunction4.Dictionary.SetProperty("N", new PdfNumber(1));
							PdfReferenceHolder element14 = new PdfReferenceHolder(pdfExponentialInterpolationFunction4);
							pdfArray.Add(element14);
							break;
						}
						throw new ArgumentOutOfRangeException();
					}
					}
				}
			}
			else
			{
				float[] array = new float[2] { 0f, 1f };
				float[] array2 = new float[8] { 0f, 1f, 0f, 1f, 0f, 1f, 0f, 1f };
				m_stream.SetProperty("FunctionType", new PdfNumber(4));
				m_stream.SetProperty("Domain", new PdfArray(array));
				m_stream.SetProperty("Range", new PdfArray(array2));
			}
		}
		return pdfArray;
	}

	private void Stream_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Save();
	}
}
