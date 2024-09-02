namespace DocGen.Office;

internal class TrueTypeFontMetrics : ICloneable
{
	public float Ascent;

	public float Descent;

	public float UnitPerEM;

	public float Leading;

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

	public float GetAscent(TrueTypeFontStringFormat format)
	{
		return Ascent * (GetSize(format) / UnitPerEM);
	}

	public float GetLeading(TrueTypeFontStringFormat format)
	{
		return Leading * (GetSize(format) / UnitPerEM);
	}

	public float GetDescent(TrueTypeFontStringFormat format)
	{
		return Descent * (GetSize(format) / UnitPerEM);
	}

	public float GetLineGap(TrueTypeFontStringFormat format)
	{
		return (float)LineGap * (GetSize(format) / UnitPerEM);
	}

	public float GetHeight(TrueTypeFontStringFormat format)
	{
		if (GetDescent(format) < 0f)
		{
			return GetAscent(format) - GetDescent(format) + GetLeading(format);
		}
		return GetAscent(format) + GetDescent(format) + GetLeading(format);
	}

	public float GetSize(TrueTypeFontStringFormat format)
	{
		return Size;
	}

	public object Clone()
	{
		TrueTypeFontMetrics obj = (TrueTypeFontMetrics)MemberwiseClone();
		obj.WidthTable = WidthTable.Clone();
		return obj;
	}
}
