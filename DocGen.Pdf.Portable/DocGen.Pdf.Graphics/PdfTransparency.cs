using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics;

internal class PdfTransparency : IPdfWrapper
{
	private PdfDictionary m_dictionary = new PdfDictionary();

	public float Stroke => GetNumber("CA");

	public float Fill => GetNumber("ca");

	public PdfBlendMode Mode
	{
		get
		{
			string name = GetName("ca");
			return (PdfBlendMode)Enum.Parse(typeof(PdfBlendMode), name, ignoreCase: true);
		}
	}

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfTransparency(float stroke, float fill, PdfBlendMode mode)
	{
		if (stroke < 0f)
		{
			throw new ArgumentOutOfRangeException("stroke", "The value can't be less then zero.");
		}
		if (fill < 0f)
		{
			throw new ArgumentOutOfRangeException("fill", "The value can't be less then zero.");
		}
		if (PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1B || PdfDocument.ConformanceLevel == PdfConformanceLevel.Pdf_A1A)
		{
			stroke = ((stroke == 0f) ? 1f : stroke);
			fill = ((fill == 0f) ? 1f : fill);
			mode = ((mode == PdfBlendMode.Normal) ? mode : PdfBlendMode.Normal);
		}
		m_dictionary.SetNumber("CA", stroke);
		m_dictionary.SetNumber("ca", fill);
		m_dictionary.SetName("BM", mode.ToString());
	}

	public override bool Equals(object obj)
	{
		bool result = false;
		if (obj != null && obj is PdfTransparency pdfTransparency)
		{
			result = true;
			result &= pdfTransparency.Stroke != Stroke;
			result &= pdfTransparency.Fill != Fill;
			result &= pdfTransparency.Mode != Mode;
		}
		return result;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private float GetNumber(string keyName)
	{
		float result = 0f;
		if (m_dictionary[keyName] is PdfNumber pdfNumber)
		{
			result = pdfNumber.FloatValue;
		}
		return result;
	}

	private string GetName(string keyName)
	{
		string result = null;
		PdfName pdfName = m_dictionary[keyName] as PdfName;
		if (pdfName != null)
		{
			result = pdfName.Value;
		}
		return result;
	}
}
