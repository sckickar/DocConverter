using DocGen.Drawing;
using DocGen.Pdf.Graphics;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Parsing;

internal class DeviceN : Colorspace
{
	private Colorspace m_alternateColorspace;

	private Function m_function;

	internal override int Components => 1;

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

	internal override Color GetColor(string[] pars)
	{
		return AlternateColorspace.GetColor(AlternateColorspace.ToParams(Function.ColorTransferFunction(ToDouble(pars))));
	}

	internal override Color GetColor(byte[] bytes, int offset)
	{
		return Colorspace.GetRgbColor(bytes, offset);
	}

	internal override PdfBrush GetBrush(string[] pars, PdfPageResources resource)
	{
		return new PdfPen(GetColor(pars)).Brush;
	}

	internal void SetValue(PdfArray array)
	{
		m_alternateColorspace = GetColorspace(array);
		m_function = Function.CreateFunction(array);
	}

	private Colorspace GetColorspace(PdfArray array)
	{
		PdfName pdfName = array[2] as PdfName;
		if (pdfName != null)
		{
			return Colorspace.CreateColorSpace(pdfName.Value);
		}
		PdfReferenceHolder pdfReferenceHolder = array[2] as PdfReferenceHolder;
		if (pdfReferenceHolder != null && pdfReferenceHolder.Object is PdfArray pdfArray)
		{
			return Colorspace.CreateColorSpace((pdfArray[0] as PdfName).Value, pdfArray);
		}
		return new DeviceRGB();
	}

	private static double[] ToDouble(string[] pars)
	{
		float[] array = new float[pars.Length];
		double[] array2 = new double[pars.Length];
		for (int i = 0; i < pars.Length; i++)
		{
			float.TryParse(pars[i], out array[i]);
			array2[i] = array[i];
		}
		return array2;
	}
}
