using DocGen.Drawing;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal abstract class GradientShading : Shading
{
	private PdfArray m_coordinate;

	private PdfArray m_extented;

	private Function m_function;

	private PdfArray m_domain;

	private Color m_background;

	internal Color Background
	{
		get
		{
			return m_background;
		}
		set
		{
			m_background = value;
		}
	}

	internal PdfArray Coordinate
	{
		get
		{
			return m_coordinate;
		}
		set
		{
			m_coordinate = value;
		}
	}

	internal PdfArray Extented
	{
		get
		{
			return m_extented;
		}
		set
		{
			m_extented = value;
		}
	}

	internal PdfArray Domain
	{
		get
		{
			m_domain = Function.Domain;
			return m_domain;
		}
		set
		{
			m_domain = Function.Domain;
		}
	}

	internal Function Function
	{
		get
		{
			return m_function;
		}
		set
		{
			m_function = value;
		}
	}

	public GradientShading(PdfDictionary dic)
		: base(dic)
	{
		m_coordinate = (dic.Items.ContainsKey(new PdfName("Coords")) ? (dic.Items[new PdfName("Coords")] as PdfArray) : null);
		m_extented = (dic.Items.ContainsKey(new PdfName("Extend")) ? (dic.Items[new PdfName("Extend")] as PdfArray) : null);
		m_function = Function.CreateFunction(GetFunction(dic));
		m_background = GetBackgroundColor(dic);
	}

	public GradientShading()
	{
	}

	private PdfDictionary GetFunction(PdfDictionary dictionary)
	{
		if (dictionary.Items.ContainsKey(new PdfName("Function")))
		{
			if (dictionary.Items[new PdfName("Function")] is PdfDictionary result)
			{
				return result;
			}
			if (dictionary.Items[new PdfName("Function")] is PdfArray pdfArray)
			{
				if (pdfArray.Count >= 4 && pdfArray[3] is PdfDictionary)
				{
					return pdfArray[3] as PdfDictionary;
				}
				PdfReferenceHolder pdfReferenceHolder = pdfArray[3] as PdfReferenceHolder;
				if (pdfReferenceHolder != null)
				{
					return pdfReferenceHolder.Object as PdfDictionary;
				}
			}
			if ((dictionary.Items[new PdfName("Function")] as PdfReferenceHolder).Object is PdfDictionary result2)
			{
				return result2;
			}
		}
		return null;
	}

	protected Color GetColor(double data)
	{
		double[] values = Function.ColorTransferFunction(new double[1] { data });
		return base.AlternateColorspace.GetColor(base.AlternateColorspace.ToParams(values));
	}

	private Color GetBackgroundColor(PdfDictionary dictionary)
	{
		if (dictionary.Items.ContainsKey(new PdfName("Background")))
		{
			PdfArray pdfArray = dictionary.Items[new PdfName("Background")] as PdfArray;
			return Color.FromArgb(255, (int)((pdfArray[0] as PdfNumber).FloatValue * 255f), (int)((pdfArray[1] as PdfNumber).FloatValue * 255f), (int)(pdfArray[2] as PdfNumber).FloatValue * 255);
		}
		return Color.Transparent;
	}
}
