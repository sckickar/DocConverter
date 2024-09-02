using System;

namespace DocGen.Pdf.Graphics;

public sealed class PdfBlend : PdfBlendBase
{
	private float[] m_factors;

	public float[] Factors
	{
		get
		{
			return m_factors;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("Factors");
			}
			m_factors = SetArray(value) as float[];
		}
	}

	public PdfBlend()
	{
	}

	public PdfBlend(int count)
		: base(count)
	{
	}

	internal PdfColorBlend GenerateColorBlend(PdfColor[] colours, PdfColorSpace colorSpace)
	{
		if (colours == null)
		{
			throw new ArgumentNullException("colours");
		}
		if (base.Positions == null)
		{
			base.Positions = new float[1];
		}
		PdfColorBlend pdfColorBlend = new PdfColorBlend(base.Count);
		float[] array = base.Positions;
		PdfColor[] array2 = null;
		if (array.Length == 1)
		{
			array = new float[3]
			{
				0f,
				base.Positions[0],
				1f
			};
			array2 = new PdfColor[3]
			{
				colours[0],
				colours[0],
				colours[1]
			};
		}
		else
		{
			PdfColor color = colours[0];
			PdfColor color2 = colours[1];
			array2 = new PdfColor[base.Count];
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				array2[i] = PdfBlendBase.Interpolate(m_factors[i], color, color2, colorSpace);
			}
		}
		pdfColorBlend.Positions = array;
		pdfColorBlend.Colors = array2;
		return pdfColorBlend;
	}

	internal PdfBlend ClonePdfBlend()
	{
		PdfBlend pdfBlend = MemberwiseClone() as PdfBlend;
		if (m_factors != null)
		{
			pdfBlend.Factors = m_factors.Clone() as float[];
		}
		if (base.Positions != null)
		{
			pdfBlend.Positions = base.Positions.Clone() as float[];
		}
		return pdfBlend;
	}
}
