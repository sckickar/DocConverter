using System;
using System.Collections.Generic;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf;

internal class XObjectElement
{
	private string m_objectName;

	private string m_objectType;

	private PdfMatrix m_imageInfo;

	private PdfDictionary m_xObjectDictionary;

	private bool m_isExtractTextData;

	private bool m_isExtractLineCollection;

	internal bool IsPdfium;

	internal List<TextElement> m_extractTextElement = new List<TextElement>();

	internal bool m_isPrintSelected;

	internal double m_pageHeight;

	internal bool IsExtractTextData
	{
		get
		{
			return m_isExtractTextData;
		}
		set
		{
			m_isExtractTextData = value;
		}
	}

	internal bool IsExtractLineCollection
	{
		get
		{
			return m_isExtractLineCollection;
		}
		set
		{
			m_isExtractLineCollection = value;
		}
	}

	internal List<TextElement> ExtractTextElements
	{
		get
		{
			return m_extractTextElement;
		}
		set
		{
			m_extractTextElement = value;
		}
	}

	internal string ObjectName
	{
		get
		{
			return m_objectName;
		}
		set
		{
			m_objectName = value;
		}
	}

	internal PdfMatrix ImageInfo
	{
		get
		{
			return m_imageInfo;
		}
		set
		{
			m_imageInfo = value;
		}
	}

	internal PdfDictionary XObjectDictionary
	{
		get
		{
			return m_xObjectDictionary;
		}
		set
		{
			m_xObjectDictionary = value;
		}
	}

	internal string ObjectType
	{
		get
		{
			return m_objectType;
		}
		set
		{
			m_objectType = value;
		}
	}

	public XObjectElement(PdfDictionary xobjectDictionary, string name)
	{
		m_xObjectDictionary = xobjectDictionary;
		m_objectName = name;
		GetObjectType();
	}

	public XObjectElement(PdfDictionary xobjectDictionary, string name, PdfMatrix tm)
	{
		m_xObjectDictionary = xobjectDictionary;
		m_objectName = name;
		ImageInfo = tm;
		GetObjectType();
	}

	public PdfRecordCollection Render(PdfPageResources resources)
	{
		if (ObjectType == "Form")
		{
			return new ContentParser((m_xObjectDictionary as PdfStream).GetDecompressedData()).ReadContent();
		}
		return null;
	}

	public Stack<GraphicsState> Render(GraphicsObject g, PdfPageResources resources, Stack<GraphicsState> graphicsStates, Stack<GraphicObjectData> m_objects, float currentPageHeight, out List<Glyph> glyphList)
	{
		glyphList = new List<Glyph>();
		if (ObjectType == "Form")
		{
			PdfRecordCollection contentElements = new ContentParser((m_xObjectDictionary as PdfStream).GetDecompressedData()).ReadContent();
			PageResourceLoader pageResourceLoader = new PageResourceLoader();
			PdfDictionary pdfDictionary = new PdfDictionary();
			PdfDictionary xObjectDictionary = XObjectDictionary;
			PdfPageResources pdfPageResources = new PdfPageResources();
			if (xObjectDictionary.ContainsKey("Resources"))
			{
				pdfDictionary = ((xObjectDictionary["Resources"] is PdfReference) ? ((xObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary) : ((!(xObjectDictionary["Resources"] is PdfReferenceHolder)) ? (xObjectDictionary["Resources"] as PdfDictionary) : ((xObjectDictionary["Resources"] as PdfReferenceHolder).Object as PdfDictionary)));
				Dictionary<string, PdfMatrix> commonMatrix = new Dictionary<string, PdfMatrix>();
				pdfPageResources = pageResourceLoader.UpdatePageResources(pdfPageResources, pageResourceLoader.GetImageResources(pdfDictionary, null, ref commonMatrix));
				pdfPageResources = pageResourceLoader.UpdatePageResources(pdfPageResources, pageResourceLoader.GetFontResources(pdfDictionary));
				pdfPageResources = pageResourceLoader.UpdatePageResources(pdfPageResources, pageResourceLoader.GetExtendedGraphicResources(pdfDictionary));
				pdfPageResources = pageResourceLoader.UpdatePageResources(pdfPageResources, pageResourceLoader.GetColorSpaceResource(pdfDictionary));
				pdfPageResources = pageResourceLoader.UpdatePageResources(pdfPageResources, pageResourceLoader.GetShadingResource(pdfDictionary));
				pdfPageResources = pageResourceLoader.UpdatePageResources(pdfPageResources, pageResourceLoader.GetPatternResource(pdfDictionary));
			}
			Matrix matrix = default(Matrix);
			matrix = Matrix.Identity;
			if (xObjectDictionary.ContainsKey("Matrix"))
			{
				PdfArray pdfArray = new PdfArray();
				if (xObjectDictionary["Matrix"] is PdfArray && xObjectDictionary["Matrix"] is PdfArray pdfArray2)
				{
					float floatValue = (pdfArray2[0] as PdfNumber).FloatValue;
					float floatValue2 = (pdfArray2[1] as PdfNumber).FloatValue;
					float floatValue3 = (pdfArray2[2] as PdfNumber).FloatValue;
					float floatValue4 = (pdfArray2[3] as PdfNumber).FloatValue;
					float floatValue5 = (pdfArray2[4] as PdfNumber).FloatValue;
					float floatValue6 = (pdfArray2[5] as PdfNumber).FloatValue;
					matrix = new Matrix(floatValue, floatValue2, floatValue3, floatValue4, floatValue5, floatValue6);
					if (floatValue5 != 0f || floatValue6 != 0f)
					{
						g.TranslateTransform(floatValue5, 0f - floatValue6);
					}
					if (floatValue != 0f || floatValue4 != 0f)
					{
						g.ScaleTransform(floatValue, floatValue4);
					}
					double num = Math.Acos(floatValue);
					double num2 = Math.Round(180.0 / Math.PI * num);
					double num3 = Math.Asin(floatValue2);
					double num4 = Math.Round(180.0 / Math.PI * num3);
					if (num2 == num4)
					{
						g.RotateTransform(0f - (float)num2);
					}
					else if (!double.IsNaN(num4))
					{
						g.RotateTransform(0f - (float)num4);
					}
					else if (!double.IsNaN(num2))
					{
						g.RotateTransform(0f - (float)num2);
					}
				}
			}
			if (pdfDictionary != null)
			{
				DeviceCMYK cmyk = new DeviceCMYK();
				ImageRenderer imageRenderer = new ImageRenderer(contentElements, pdfPageResources, g, cmyk, currentPageHeight);
				imageRenderer.m_objects = m_objects;
				Matrix ctm = m_objects.ToArray()[0].Ctm;
				Matrix ctm2 = matrix * ctm;
				m_objects.ToArray()[0].drawing2dMatrixCTM = new Matrix((float)ctm2.M11, (float)ctm2.M12, (float)ctm2.M21, (float)ctm2.M22, (float)ctm2.OffsetX, (float)ctm2.OffsetY);
				m_objects.ToArray()[0].Ctm = ctm2;
				imageRenderer.m_selectablePrintDocument = m_isPrintSelected;
				imageRenderer.m_pageHeight = (float)m_pageHeight;
				imageRenderer.isXGraphics = true;
				imageRenderer.isExtractLineCollection = m_isExtractLineCollection;
				imageRenderer.RenderAsImage();
				imageRenderer.isExtractLineCollection = false;
				imageRenderer.isXGraphics = false;
				if (imageRenderer.extractTextElement.Count != 0)
				{
					ExtractTextElements = imageRenderer.extractTextElement;
					IsExtractLineCollection = true;
				}
				while (imageRenderer.xobjectGraphicsCount > 0)
				{
					m_objects.Pop();
					imageRenderer.xobjectGraphicsCount--;
				}
				glyphList = imageRenderer.imageRenderGlyphList;
				m_objects.ToArray()[0].Ctm = ctm;
			}
		}
		return graphicsStates;
	}

	private void GetObjectType()
	{
		if (m_xObjectDictionary != null && m_xObjectDictionary.ContainsKey("Subtype"))
		{
			m_objectType = (m_xObjectDictionary["Subtype"] as PdfName).Value;
		}
	}
}
