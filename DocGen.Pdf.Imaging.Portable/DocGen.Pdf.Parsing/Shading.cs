using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;
using DocGen.PdfViewer.Base;

namespace DocGen.Pdf.Parsing;

internal abstract class Shading
{
	private Colorspace m_alternateColorspace;

	private bool m_isRectangle;

	private bool m_isCircle;

	private string m_rectangleWidth;

	internal Colorspace AlternateColorspace
	{
		get
		{
			return m_alternateColorspace;
		}
		set
		{
			m_alternateColorspace = value;
		}
	}

	public Shading()
	{
	}

	public Shading(PdfDictionary dictionary)
	{
		m_alternateColorspace = GetColorspace(dictionary);
	}

	private Colorspace GetColorspace(PdfDictionary dictionary)
	{
		if (dictionary.Items.ContainsKey(new PdfName("ColorSpace")))
		{
			IPdfPrimitive pdfPrimitive = dictionary.Items[new PdfName("ColorSpace")];
			PdfReferenceHolder pdfReferenceHolder = pdfPrimitive as PdfReferenceHolder;
			if (pdfReferenceHolder != null)
			{
				if (pdfReferenceHolder.Object is PdfArray pdfArray)
				{
					return Colorspace.CreateColorSpace((pdfArray[0] as PdfName).Value, pdfArray);
				}
				PdfName pdfName = pdfReferenceHolder.Object as PdfName;
				if (pdfName != null)
				{
					return Colorspace.CreateColorSpace(pdfName.Value);
				}
			}
			if (pdfPrimitive is PdfArray pdfArray2)
			{
				return Colorspace.CreateColorSpace((pdfArray2[0] as PdfName).Value, pdfArray2);
			}
			PdfName pdfName2 = pdfPrimitive as PdfName;
			if (pdfName2 != null)
			{
				return Colorspace.CreateColorSpace(pdfName2.Value);
			}
		}
		return null;
	}

	internal virtual void SetOperatorValues(bool IsRectangle, bool IsCircle, string RectangleWidth)
	{
		m_isRectangle = IsRectangle;
		m_isCircle = IsCircle;
		m_rectangleWidth = RectangleWidth;
	}

	internal abstract PdfBrush GetBrushColor(Matrix transformMatrix);
}
