namespace DocGen.Pdf.Graphics.Fonts;

internal class OtfGlyphInfo
{
	private int m_index;

	private int m_charCode;

	private char[] m_chars;

	private float m_width;

	internal int leadingX;

	internal int leadingY;

	internal int xAdvance;

	internal int yAdvance;

	internal int m_placment;

	internal bool unsupportedGlyph;

	internal int Index
	{
		get
		{
			return m_index;
		}
		set
		{
			m_index = value;
		}
	}

	internal int CharCode
	{
		get
		{
			return m_charCode;
		}
		set
		{
			m_charCode = value;
		}
	}

	internal char[] Characters
	{
		get
		{
			return m_chars;
		}
		set
		{
			m_chars = value;
		}
	}

	internal float Width
	{
		get
		{
			return m_width;
		}
		set
		{
			m_width = value;
		}
	}

	internal OtfGlyphInfo(int charCode, int index, float width)
	{
		m_charCode = charCode;
		m_index = index;
		m_width = width;
		if (charCode > -1)
		{
			Characters = char.ConvertFromUtf32(charCode).ToCharArray();
		}
	}

	internal OtfGlyphInfo(OtfGlyphInfo glyph, int x, int y)
		: this(glyph)
	{
		leadingX = x;
		leadingY = y;
	}

	internal OtfGlyphInfo(OtfGlyphInfo glyph)
	{
		m_index = glyph.Index;
		m_chars = glyph.Characters;
		m_charCode = glyph.CharCode;
		m_width = glyph.Width;
		leadingX = glyph.leadingX;
		leadingY = glyph.leadingY;
		xAdvance = glyph.xAdvance;
		yAdvance = glyph.yAdvance;
		m_placment = glyph.m_placment;
	}

	internal OtfGlyphInfo(OtfGlyphInfo glyph, int xPlacement, int yPlacement, int xAdvance, int yAdvance, int m_placment)
		: this(glyph)
	{
		leadingX = xPlacement;
		leadingY = yPlacement;
		this.xAdvance = xAdvance;
		this.yAdvance = yAdvance;
		this.m_placment = m_placment;
	}

	internal bool HasOffset()
	{
		if (!HasAdvance())
		{
			return HasPlacement();
		}
		return true;
	}

	internal bool HasPlacement()
	{
		return m_placment != 0;
	}

	internal bool HasAdvance()
	{
		if (xAdvance == 0)
		{
			return yAdvance != 0;
		}
		return true;
	}
}
