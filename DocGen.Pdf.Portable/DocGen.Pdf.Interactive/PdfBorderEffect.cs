using System;
using DocGen.Pdf.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Interactive;

public class PdfBorderEffect : IPdfWrapper
{
	private PdfDictionary m_dictionary = new PdfDictionary();

	private PdfBorderEffectStyle m_style;

	private float m_intensity;

	public PdfBorderEffectStyle Style
	{
		get
		{
			return m_style;
		}
		set
		{
			m_style = value;
			Dictionary.SetProperty("S", new PdfName(StyleToEffect(m_style)));
		}
	}

	public float Intensity
	{
		get
		{
			return m_intensity;
		}
		set
		{
			if (value >= 0f && value <= 2f)
			{
				m_intensity = value;
				Dictionary.SetNumber("I", m_intensity);
				return;
			}
			throw new Exception("Intensity range only 0 to 2");
		}
	}

	internal PdfDictionary Dictionary => m_dictionary;

	IPdfPrimitive IPdfWrapper.Element => m_dictionary;

	public PdfBorderEffect()
	{
		Initialize();
	}

	internal PdfBorderEffect(PdfDictionary dictionary)
	{
		if (!dictionary.ContainsKey("BE") || !(PdfCrossTable.Dereference(dictionary["BE"]) is PdfDictionary pdfDictionary))
		{
			return;
		}
		if (pdfDictionary.ContainsKey("I"))
		{
			IPdfPrimitive pdfPrimitive = pdfDictionary["I"];
			if (pdfPrimitive is PdfNumber)
			{
				m_intensity = (pdfPrimitive as PdfNumber).FloatValue;
			}
		}
		if (pdfDictionary.ContainsKey("S"))
		{
			IPdfPrimitive pdfPrimitive2 = pdfDictionary["S"];
			if (pdfPrimitive2.ToString() != null)
			{
				m_style = GetBorderEffect(pdfPrimitive2.ToString());
			}
		}
	}

	protected void Initialize()
	{
		Dictionary.BeginSave += Dictionary_BeginSave;
	}

	private void Dictionary_BeginSave(object sender, SavePdfPrimitiveEventArgs ars)
	{
		Dictionary.SetProperty("S", new PdfName(StyleToEffect(m_style)));
		Dictionary.SetNumber("I", m_intensity);
	}

	private string StyleToEffect(PdfBorderEffectStyle effect)
	{
		if (effect == PdfBorderEffectStyle.Cloudy)
		{
			return "C";
		}
		return "S";
	}

	private PdfBorderEffectStyle GetBorderEffect(string beffect)
	{
		if (beffect == "/C")
		{
			return PdfBorderEffectStyle.Cloudy;
		}
		return PdfBorderEffectStyle.Solid;
	}
}
