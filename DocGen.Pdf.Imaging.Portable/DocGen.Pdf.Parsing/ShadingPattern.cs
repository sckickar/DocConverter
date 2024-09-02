using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal class ShadingPattern : Pattern
{
	private Shading m_shading;

	private PdfDictionary m_shadingDictionary;

	private PdfArray m_patternMatrix;

	private PdfArray m_bBox;

	private bool m_isRectangle;

	private bool m_isCircle;

	private string m_rectangleWidth;

	internal new PdfArray PatternMatrix
	{
		get
		{
			return m_patternMatrix;
		}
		set
		{
			m_patternMatrix = value;
		}
	}

	internal Shading ShadingType
	{
		get
		{
			return m_shading;
		}
		set
		{
			m_shading = value;
		}
	}

	internal PdfArray BBox
	{
		get
		{
			if (m_bBox == null && m_shadingDictionary.ContainsKey(new PdfName("BBox")))
			{
				m_bBox = m_shadingDictionary[new PdfName("BBox")] as PdfArray;
			}
			return m_bBox;
		}
		set
		{
			m_bBox = value;
		}
	}

	public ShadingPattern()
	{
	}

	public ShadingPattern(PdfDictionary dictionary, bool IsRectangle, bool IsCircle, string RectangleWidth)
	{
		m_shadingDictionary = dictionary;
		m_shading = CreateShading(dictionary);
		m_patternMatrix = (dictionary.Items.ContainsKey(new PdfName("Matrix")) ? (dictionary.Items[new PdfName("Matrix")] as PdfArray) : new PdfArray(new int[6] { 1, 0, 0, 1, 0, 0 }));
		m_isRectangle = IsRectangle;
		m_isCircle = IsCircle;
		m_rectangleWidth = RectangleWidth;
	}

	internal void SetShadingValue(IPdfPrimitive shadingResource)
	{
		m_shadingDictionary = shadingResource as PdfDictionary;
		if (m_shadingDictionary != null)
		{
			m_shading = CreateShading(m_shadingDictionary);
			m_patternMatrix = (m_shadingDictionary.Items.ContainsKey(new PdfName("Matrix")) ? (m_shadingDictionary.Items[new PdfName("Matrix")] as PdfArray) : new PdfArray(new int[6] { 1, 0, 0, 1, 0, 0 }));
		}
	}

	internal override void SetOperatorValues(bool IsRectangle, bool IsCircle, string RectangleWidth)
	{
		m_isRectangle = IsRectangle;
		m_isCircle = IsCircle;
		m_rectangleWidth = RectangleWidth;
	}

	internal Shading CreateShading(PdfDictionary dictionary)
	{
		if (dictionary.Items.ContainsKey(new PdfName("Shading")))
		{
			IPdfPrimitive pdfPrimitive = dictionary.Items[new PdfName("Shading")];
			PdfDictionary pdfDictionary = pdfPrimitive as PdfDictionary;
			if (pdfDictionary == null)
			{
				pdfDictionary = (pdfPrimitive as PdfReferenceHolder).Object as PdfDictionary;
			}
			if (pdfDictionary != null)
			{
				switch ((pdfDictionary.Items[new PdfName("ShadingType")] as PdfNumber).IntValue)
				{
				case 2:
					if (pdfPrimitive is PdfDictionary)
					{
						return new AxialShading(pdfPrimitive as PdfDictionary);
					}
					return new AxialShading((pdfPrimitive as PdfReferenceHolder).Object as PdfDictionary);
				case 3:
					if (pdfPrimitive is PdfDictionary)
					{
						return new RadialShading(pdfPrimitive as PdfDictionary);
					}
					return new RadialShading((pdfPrimitive as PdfReferenceHolder).Object as PdfDictionary);
				}
			}
		}
		else if (dictionary.Items.ContainsKey(new PdfName("ShadingType")))
		{
			switch ((dictionary.Items[new PdfName("ShadingType")] as PdfNumber).IntValue)
			{
			case 2:
				return new AxialShading(dictionary);
			case 3:
				return new RadialShading(dictionary);
			}
		}
		return null;
	}

	internal PdfBrush CreateBrush()
	{
		if (ShadingType != null)
		{
			ShadingType.SetOperatorValues(m_isRectangle, m_isCircle, m_rectangleWidth);
			return ShadingType.GetBrushColor(ConvertArrayToMatrix(PatternMatrix));
		}
		return PdfBrushes.Transparent;
	}

	private Matrix ConvertArrayToMatrix(PdfArray array)
	{
		return new Matrix((array[0] as PdfNumber).FloatValue, (array[1] as PdfNumber).FloatValue, (array[2] as PdfNumber).FloatValue, (array[3] as PdfNumber).FloatValue, (array[4] as PdfNumber).FloatValue, (array[5] as PdfNumber).FloatValue);
	}
}
