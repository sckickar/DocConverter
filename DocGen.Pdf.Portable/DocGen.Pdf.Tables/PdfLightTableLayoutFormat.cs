using System;
using DocGen.Pdf.Graphics;

namespace DocGen.Pdf.Tables;

public class PdfLightTableLayoutFormat : PdfLayoutFormat
{
	private int m_startColumn;

	private int m_endColumn;

	public int StartColumnIndex
	{
		get
		{
			return m_startColumn;
		}
		set
		{
			m_startColumn = value;
		}
	}

	public int EndColumnIndex
	{
		get
		{
			return m_endColumn;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("EndColumnIndex");
			}
			m_endColumn = value;
		}
	}

	public PdfLightTableLayoutFormat()
	{
	}

	public PdfLightTableLayoutFormat(PdfLayoutFormat baseFormat)
		: base(baseFormat)
	{
	}
}
