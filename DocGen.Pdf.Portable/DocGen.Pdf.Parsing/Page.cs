using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class Page
{
	private const int c_ShadowWidth = 4;

	private const int c_ShadowHeight = 4;

	internal List<RectangleF> matchTextPositions = new List<RectangleF>();

	private PdfUnitConvertor m_unitConverter = new PdfUnitConvertor();

	private PdfPageResources m_resources;

	private PdfRecordCollection m_recordCollection;

	private PdfPageBase m_page;

	private int m_actualWidth;

	private int m_actualHeight;

	private RectangleF m_bounds;

	private double m_rotation;

	private string errorText;

	private bool _isRotationInitialized;

	private bool _isCropboxInitialized;

	private bool _isMediaboxInitialized;

	private RectangleF _cropboxRectangle = RectangleF.Empty;

	private RectangleF _mediaboxRectangle = RectangleF.Empty;

	public int CurrentLeftLocation;

	public int ActualWidth => m_actualWidth;

	public int ActualHeight => m_actualHeight;

	public RectangleF Bounds
	{
		get
		{
			return m_bounds;
		}
		set
		{
			m_bounds = value;
		}
	}

	public float Width
	{
		get
		{
			return m_bounds.Width;
		}
		set
		{
			m_bounds.Width = value;
		}
	}

	public float Height
	{
		get
		{
			return m_bounds.Height;
		}
		set
		{
			m_bounds.Height = value;
		}
	}

	internal double Rotation
	{
		get
		{
			if (_isRotationInitialized)
			{
				return m_rotation;
			}
			if (m_page != null)
			{
				if (m_page.Rotation == PdfPageRotateAngle.RotateAngle90)
				{
					m_rotation = 90.0;
				}
				else if (m_page.Rotation == PdfPageRotateAngle.RotateAngle180)
				{
					m_rotation = 180.0;
				}
				else if (m_page.Rotation == PdfPageRotateAngle.RotateAngle270)
				{
					m_rotation = 270.0;
				}
			}
			_isRotationInitialized = true;
			return m_rotation;
		}
	}

	internal PdfPageResources Resources => m_resources;

	internal PdfRecordCollection RecordCollection => m_recordCollection;

	internal RectangleF CropBox
	{
		get
		{
			if (_isCropboxInitialized)
			{
				return _cropboxRectangle;
			}
			PdfDictionary dictionary = m_page.Dictionary;
			if (dictionary.ContainsKey("CropBox") && dictionary.GetValue(m_page.Dictionary.CrossTable, "CropBox", "Parent") is PdfArray pdfArray)
			{
				float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
				float height = (((pdfArray[3] as PdfNumber).FloatValue != 0f) ? (pdfArray[3] as PdfNumber).FloatValue : (pdfArray[1] as PdfNumber).FloatValue);
				float floatValue2 = (pdfArray[0] as PdfNumber).FloatValue;
				float floatValue3 = (pdfArray[1] as PdfNumber).FloatValue;
				_cropboxRectangle = new RectangleF(new PointF(floatValue2, floatValue3), new SizeF(floatValue, height));
			}
			_isCropboxInitialized = true;
			return _cropboxRectangle;
		}
	}

	internal RectangleF MediaBox
	{
		get
		{
			if (_isMediaboxInitialized)
			{
				return _mediaboxRectangle;
			}
			PdfDictionary dictionary = m_page.Dictionary;
			if (dictionary.ContainsKey("MediaBox") && dictionary["MediaBox"] is PdfArray pdfArray)
			{
				float floatValue = (pdfArray[2] as PdfNumber).FloatValue;
				float height = (((pdfArray[3] as PdfNumber).FloatValue != 0f) ? (pdfArray[3] as PdfNumber).FloatValue : (pdfArray[1] as PdfNumber).FloatValue);
				float floatValue2 = (pdfArray[0] as PdfNumber).FloatValue;
				float floatValue3 = (pdfArray[1] as PdfNumber).FloatValue;
				_mediaboxRectangle = new RectangleF(new PointF(floatValue2, floatValue3), new SizeF(floatValue, height));
			}
			_isMediaboxInitialized = true;
			return _mediaboxRectangle;
		}
	}

	static Page()
	{
	}

	public Page(PdfPageBase page)
	{
		m_page = page;
	}

	internal void Initialize(PdfPageBase page, bool needParsing)
	{
		try
		{
			if (needParsing && m_recordCollection == null)
			{
				m_resources = PageResourceLoader.Instance.GetPageResources(page);
				using MemoryStream memoryStream = new MemoryStream();
				byte[] array = PdfString.StringToByte("\r\n");
				for (int i = 0; i < page.Contents.Count; i++)
				{
					PdfStream pdfStream = null;
					IPdfPrimitive pdfPrimitive = page.Contents[i];
					if (pdfPrimitive is PdfReferenceHolder)
					{
						pdfStream = (page.Contents[i] as PdfReferenceHolder).Object as PdfStream;
					}
					else if (pdfPrimitive is PdfStream)
					{
						pdfStream = pdfPrimitive as PdfStream;
					}
					if (pdfStream != null)
					{
						byte[] decompressedData = pdfStream.GetDecompressedData();
						memoryStream.Write(decompressedData, 0, decompressedData.Length);
						memoryStream.Write(array, 0, array.Length);
					}
				}
				memoryStream.Position = 0L;
				ContentParser contentParser = new ContentParser(memoryStream.ToArray());
				m_recordCollection = contentParser.ReadContent();
			}
			SizeF sizeF = new SizeF(m_unitConverter.ConvertToPixels(page.Size.Width, PdfGraphicsUnit.Point), m_unitConverter.ConvertToPixels(page.Size.Height, PdfGraphicsUnit.Point));
			Width = sizeF.Width;
			Height = sizeF.Height;
			m_actualWidth = (int)Width;
			m_actualHeight = (int)Height;
		}
		catch (Exception ex)
		{
			errorText = ex.StackTrace;
		}
	}
}
