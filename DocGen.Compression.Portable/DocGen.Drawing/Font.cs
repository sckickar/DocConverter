using System;

namespace DocGen.Drawing;

public sealed class Font : IDisposable, IClone
{
	private string m_fontFamilyName;

	private float m_fontSize;

	private bool m_bold;

	private bool m_italic;

	private bool m_strikeout;

	private UnderlineStyle m_underlineStyle;

	public byte m_charSet = 1;

	private object m_fontImpl;

	private const int DEF_INCORRECT_HASH = -1;

	private int m_iHashCode = -1;

	private GraphicsUnit m_fontUnit;

	private bool m_gdiVerticalFont;

	public float Size
	{
		get
		{
			return m_fontSize;
		}
		set
		{
			m_fontSize = value;
		}
	}

	internal GraphicsUnit Unit => m_fontUnit;

	internal FontStyle Style
	{
		get
		{
			FontStyle fontStyle = FontStyle.Regular;
			if (m_bold)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (m_italic)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (m_underlineStyle != 0)
			{
				fontStyle |= FontStyle.Underline;
			}
			if (m_strikeout)
			{
				fontStyle |= FontStyle.Strikeout;
			}
			return fontStyle;
		}
	}

	public string Name
	{
		get
		{
			return m_fontFamilyName;
		}
		set
		{
			m_fontFamilyName = value;
		}
	}

	public string FontFamilyName
	{
		get
		{
			return m_fontFamilyName;
		}
		set
		{
			m_fontFamilyName = value;
		}
	}

	public float SizeInPoints
	{
		get
		{
			return m_fontSize;
		}
		set
		{
			m_fontSize = value;
		}
	}

	public bool Bold
	{
		get
		{
			return m_bold;
		}
		set
		{
			m_bold = value;
		}
	}

	public bool Italic
	{
		get
		{
			return m_italic;
		}
		set
		{
			m_italic = value;
		}
	}

	public bool Strikeout
	{
		get
		{
			return m_strikeout;
		}
		set
		{
			m_strikeout = value;
		}
	}

	public UnderlineStyle UnderlineStyle
	{
		get
		{
			return m_underlineStyle;
		}
		set
		{
			m_underlineStyle = value;
		}
	}

	public bool Underline
	{
		get
		{
			if (m_underlineStyle == UnderlineStyle.Single)
			{
				return true;
			}
			return false;
		}
	}

	internal object FontImpl
	{
		get
		{
			return m_fontImpl;
		}
		set
		{
			m_fontImpl = value;
		}
	}

	internal byte GdiCharSet => m_charSet;

	internal bool GdiVerticalFont => m_gdiVerticalFont;

	public override bool Equals(object obj)
	{
		if (obj is Font font)
		{
			if (font.m_fontFamilyName == m_fontFamilyName && font.m_fontSize == m_fontSize && font.m_bold == m_bold && font.m_italic == m_italic && font.m_strikeout == m_strikeout)
			{
				return font.m_underlineStyle == m_underlineStyle;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		if (m_iHashCode == -1)
		{
			m_iHashCode = m_fontFamilyName.GetHashCode() + m_fontSize.GetHashCode() + m_bold.GetHashCode() + m_italic.GetHashCode() + m_strikeout.GetHashCode() + m_underlineStyle.GetHashCode();
		}
		return m_iHashCode;
	}

	public void Dispose()
	{
		m_fontFamilyName = null;
	}

	public Font()
	{
	}

	public Font(string fontFamilyName, float fontSize)
	{
		m_fontFamilyName = fontFamilyName;
		m_fontSize = fontSize;
	}

	internal Font(string fontFamilyName, float fontSize, int style)
	{
		m_fontFamilyName = fontFamilyName;
		m_fontSize = fontSize;
		if (((uint)style & (true ? 1u : 0u)) != 0)
		{
			m_bold = true;
		}
		if (((uint)style & 2u) != 0)
		{
			m_italic = true;
		}
		if (((uint)style & 4u) != 0)
		{
			m_underlineStyle = UnderlineStyle.Single;
		}
		if (((uint)style & 8u) != 0)
		{
			m_strikeout = true;
		}
	}

	internal Font(string familyName, float size, FontStyle fontstyle, GraphicsUnit unit)
	{
		m_fontFamilyName = familyName;
		m_fontUnit = unit;
		m_fontSize = size;
		m_bold = (fontstyle & FontStyle.Bold) != 0;
		m_italic = (fontstyle & FontStyle.Italic) != 0;
		m_underlineStyle = (((fontstyle & FontStyle.Underline) != 0) ? UnderlineStyle.Single : UnderlineStyle.None);
		m_strikeout = (fontstyle & FontStyle.Strikeout) != 0;
	}

	internal Font(string FontName, float size, FontStyle fontstyle, GraphicsUnit unit, byte charSet, bool gdiVerticalFont)
	{
		m_fontFamilyName = FontName;
		m_charSet = charSet;
		m_fontUnit = unit;
		m_fontSize = size;
		m_gdiVerticalFont = gdiVerticalFont;
		m_bold = (fontstyle & FontStyle.Bold) != 0;
		m_italic = (fontstyle & FontStyle.Italic) != 0;
		m_underlineStyle = (((fontstyle & FontStyle.Underline) != 0) ? UnderlineStyle.Single : UnderlineStyle.None);
		m_strikeout = (fontstyle & FontStyle.Strikeout) != 0;
	}

	public Font(string FontName, float size, FontStyle fontstyle, GraphicsUnit unit, byte charSet)
	{
		m_fontFamilyName = FontName;
		m_charSet = charSet;
		m_fontUnit = unit;
		m_fontSize = size;
		m_bold = (fontstyle & FontStyle.Bold) != 0;
		m_italic = (fontstyle & FontStyle.Italic) != 0;
		m_underlineStyle = (((fontstyle & FontStyle.Underline) != 0) ? UnderlineStyle.Single : UnderlineStyle.None);
		m_strikeout = (fontstyle & FontStyle.Strikeout) != 0;
	}

	internal Font(string FontName, float size, FontStyle fontstyle)
	{
		m_fontFamilyName = FontName;
		m_fontSize = size;
		m_bold = (fontstyle & FontStyle.Bold) != 0;
		m_italic = (fontstyle & FontStyle.Italic) != 0;
		m_underlineStyle = (((fontstyle & FontStyle.Underline) != 0) ? UnderlineStyle.Single : UnderlineStyle.None);
		m_strikeout = (fontstyle & FontStyle.Strikeout) != 0;
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
