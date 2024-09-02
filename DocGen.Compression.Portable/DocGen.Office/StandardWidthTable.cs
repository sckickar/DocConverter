using System;

namespace DocGen.Office;

internal class StandardWidthTable : WidthTable
{
	private int[] m_widths;

	public override int this[int index]
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

	internal StandardWidthTable(int[] widths)
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
		obj.m_widths = (int[])m_widths.Clone();
		return obj;
	}
}
