using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Fonts;

internal class StandardWidthTable : WidthTable
{
	private float[] m_widths;

	public override float this[int index]
	{
		get
		{
			if (index < 0 || index >= m_widths.Length)
			{
				throw new ArgumentOutOfRangeException("index", "The character is not supported by the font.");
			}
			return m_widths[index];
		}
	}

	public int Length => m_widths.Length;

	internal StandardWidthTable(float[] widths)
	{
		if (widths == null)
		{
			throw new ArgumentNullException("widths");
		}
		m_widths = widths;
	}

	public override WidthTable Clone()
	{
		StandardWidthTable obj = MemberwiseClone() as StandardWidthTable;
		obj.m_widths = (float[])m_widths.Clone();
		return obj;
	}

	internal override PdfArray ToArray()
	{
		return new PdfArray(m_widths);
	}
}
