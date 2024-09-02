using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class TilingPattern : Pattern
{
	private int m_paintType;

	private int m_tilingType;

	private PdfArray m_boundBox;

	private PdfNumber m_xStep;

	private PdfNumber m_yStep;

	private PdfDictionary m_resource;

	private PdfArray m_patternMatrix;

	private PdfStream m_data;

	private Bitmap m_embeddedImage;

	private Matrix m_tilingPatternMatrix;

	internal Rectangle BoundingRectangle => GetBoundingRectangle();

	internal Matrix TilingPatternMatrix
	{
		get
		{
			return m_tilingPatternMatrix;
		}
		set
		{
			m_tilingPatternMatrix = value;
		}
	}

	internal int PaintType
	{
		get
		{
			return m_paintType;
		}
		set
		{
			m_paintType = value;
		}
	}

	internal int TilingType
	{
		get
		{
			return m_tilingType;
		}
		set
		{
			m_tilingType = value;
		}
	}

	internal PdfArray BBox
	{
		get
		{
			return m_boundBox;
		}
		set
		{
			m_boundBox = value;
		}
	}

	internal PdfNumber XStep
	{
		get
		{
			return m_xStep;
		}
		set
		{
			m_xStep = value;
		}
	}

	internal PdfNumber YStep
	{
		get
		{
			return m_yStep;
		}
		set
		{
			m_yStep = value;
		}
	}

	internal PdfDictionary Resources
	{
		get
		{
			return m_resource;
		}
		set
		{
			m_resource = value;
		}
	}

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

	internal PdfStream Data
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	internal Bitmap EmbeddedImage
	{
		get
		{
			return m_embeddedImage;
		}
		set
		{
			m_embeddedImage = value;
		}
	}

	public TilingPattern()
	{
	}

	public TilingPattern(PdfDictionary dictionary)
	{
		m_tilingType = (dictionary.Items[new PdfName("TilingType")] as PdfNumber).IntValue;
		m_paintType = (dictionary.Items[new PdfName("PaintType")] as PdfNumber).IntValue;
		m_xStep = dictionary.Items[new PdfName("XStep")] as PdfNumber;
		m_yStep = dictionary.Items[new PdfName("YStep")] as PdfNumber;
		m_resource = GetResource(dictionary);
		m_boundBox = dictionary.Items[new PdfName("BBox")] as PdfArray;
		m_patternMatrix = (dictionary.Items.ContainsKey(new PdfName("Matrix")) ? (dictionary.Items[new PdfName("Matrix")] as PdfArray) : new PdfArray(new int[6] { 1, 0, 0, 1, 0, 0 }));
		m_data = ((dictionary is PdfStream) ? (dictionary as PdfStream) : null);
	}

	private Rectangle GetBoundingRectangle()
	{
		int intValue = (m_boundBox[0] as PdfNumber).IntValue;
		int intValue2 = (m_boundBox[1] as PdfNumber).IntValue;
		int intValue3 = (m_boundBox[2] as PdfNumber).IntValue;
		int intValue4 = (m_boundBox[3] as PdfNumber).IntValue;
		return new Rectangle(intValue, intValue2, intValue3, intValue4);
	}

	internal PdfBrush CreateBrush()
	{
		return PdfBrushes.Transparent;
	}

	private PdfDictionary GetResource(PdfDictionary dictionary)
	{
		if (dictionary.Items[new PdfName("Resources")] is PdfDictionary result)
		{
			return result;
		}
		if ((dictionary.Items[new PdfName("Resources")] as PdfReferenceHolder).Object is PdfDictionary result2)
		{
			return result2;
		}
		return null;
	}
}
