using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Fonts;

internal class CjkDifferentWidth : CjkWidth
{
	private int m_from;

	private int[] m_width;

	internal override int From => m_from;

	internal override int To => From + m_width.Length - 1;

	internal override int this[int index]
	{
		get
		{
			if (index < From || index > To)
			{
				throw new ArgumentOutOfRangeException("index", "Index is out of range.");
			}
			return m_width[index - From];
		}
	}

	public CjkDifferentWidth(int from, int[] widths)
	{
		if (widths == null)
		{
			throw new ArgumentNullException("widths");
		}
		m_from = from;
		m_width = widths;
	}

	internal override void AppendToArray(PdfArray arr)
	{
		arr.Add(new PdfNumber(From));
		PdfArray element = new PdfArray(m_width);
		arr.Add(element);
	}

	public override CjkWidth Clone()
	{
		CjkDifferentWidth obj = MemberwiseClone() as CjkDifferentWidth;
		obj.m_width = (int[])m_width.Clone();
		return obj;
	}
}
