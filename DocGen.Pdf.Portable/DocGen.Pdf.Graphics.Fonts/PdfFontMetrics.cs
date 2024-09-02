namespace DocGen.Pdf.Graphics.Fonts;

internal class PdfFontMetrics : ICloneable
{
	public float Ascent;

	public float Descent;

	public string Name;

	public string PostScriptName;

	public float Size;

	public float Height;

	public int FirstChar;

	public int LastChar;

	public int LineGap;

	public float SubScriptSizeFactor;

	public float SuperscriptSizeFactor;

	private WidthTable m_widthTable;

	internal bool isUnicodeFont;

	internal bool IsBold;

	public WidthTable WidthTable
	{
		get
		{
			return m_widthTable;
		}
		set
		{
			m_widthTable = value;
		}
	}

	public float GetAscent(PdfStringFormat format)
	{
		return Ascent * 0.001f * GetSize(format);
	}

	public float GetDescent(PdfStringFormat format)
	{
		return Descent * 0.001f * GetSize(format);
	}

	public float GetLineGap(PdfStringFormat format)
	{
		return (float)LineGap * 0.001f * GetSize(format);
	}

	public float GetHeight(PdfStringFormat format)
	{
		if (GetDescent(format) < 0f)
		{
			return GetAscent(format) - GetDescent(format) + GetLineGap(format);
		}
		return GetAscent(format) + GetDescent(format) + GetLineGap(format);
	}

	public float GetSize(PdfStringFormat format)
	{
		float num = Size;
		if (format != null)
		{
			switch (format.SubSuperScript)
			{
			case PdfSubSuperScript.SubScript:
				num /= 1.5f;
				break;
			case PdfSubSuperScript.SuperScript:
				num /= 1.5f;
				break;
			}
		}
		return num;
	}

	public object Clone()
	{
		PdfFontMetrics obj = (PdfFontMetrics)MemberwiseClone();
		obj.WidthTable = WidthTable.Clone();
		return obj;
	}
}
