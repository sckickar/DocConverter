using System;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Fonts;

internal class CjkSameWidth : CjkWidth
{
	private int m_from;

	private int m_to;

	private int m_width;

	internal override int From => m_from;

	internal override int To => m_to;

	internal override int this[int index]
	{
		get
		{
			if (index < From || index > To)
			{
				throw new ArgumentOutOfRangeException("index", "Index is out of range.");
			}
			return m_width;
		}
	}

	public CjkSameWidth(int from, int to, int width)
	{
		if (from > to)
		{
			throw new ArgumentException("'From' can't be grater than 'to'.");
		}
		m_from = from;
		m_to = to;
		m_width = width;
	}

	internal override void AppendToArray(PdfArray arr)
	{
		arr.Add(new PdfNumber(From));
		arr.Add(new PdfNumber(To));
		arr.Add(new PdfNumber(m_width));
	}

	public override CjkWidth Clone()
	{
		return MemberwiseClone() as CjkWidth;
	}
}
