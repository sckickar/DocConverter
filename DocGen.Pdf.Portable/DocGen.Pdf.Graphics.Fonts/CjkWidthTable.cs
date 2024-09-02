using System;
using System.Collections.Generic;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Graphics.Fonts;

internal class CjkWidthTable : WidthTable
{
	private List<CjkWidth> m_width;

	private int m_defaultWidth;

	public int DefaultWidth => m_defaultWidth;

	public override float this[int index]
	{
		get
		{
			int num = DefaultWidth;
			foreach (CjkWidth item in m_width)
			{
				if (index >= item.From && index <= item.To)
				{
					num = item[index];
				}
			}
			return num;
		}
	}

	public CjkWidthTable(int defaultWidth)
	{
		m_width = new List<CjkWidth>();
		m_defaultWidth = defaultWidth;
	}

	public void Add(CjkWidth widths)
	{
		if (widths == null)
		{
			throw new ArgumentNullException("widths");
		}
		m_width.Add(widths);
	}

	public override WidthTable Clone()
	{
		CjkWidthTable cjkWidthTable = MemberwiseClone() as CjkWidthTable;
		cjkWidthTable.m_width = new List<CjkWidth>(m_width.Count);
		foreach (CjkWidth item in m_width)
		{
			cjkWidthTable.m_width.Add(item.Clone());
		}
		return cjkWidthTable;
	}

	internal override PdfArray ToArray()
	{
		PdfArray pdfArray = new PdfArray();
		foreach (CjkWidth item in m_width)
		{
			item.AppendToArray(pdfArray);
		}
		return pdfArray;
	}
}
