using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.Pdf;

public class TextWord
{
	private string m_text;

	private RectangleF m_wordBounds;

	private float m_fontSize;

	private string m_fontName;

	private FontStyle m_fontStyle;

	private List<TextGlyph> m_glyphs = new List<TextGlyph>();

	private Color m_textcolor;

	public string Text
	{
		get
		{
			return m_text;
		}
		internal set
		{
			m_text = value;
		}
	}

	public RectangleF Bounds
	{
		get
		{
			return m_wordBounds;
		}
		internal set
		{
			m_wordBounds = value;
		}
	}

	internal Color TextColor
	{
		get
		{
			return m_textcolor;
		}
		set
		{
			m_textcolor = value;
		}
	}

	internal float FontSize
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

	internal string FontName
	{
		get
		{
			return m_fontName;
		}
		set
		{
			m_fontName = value;
		}
	}

	internal FontStyle FontStyle
	{
		get
		{
			return m_fontStyle;
		}
		set
		{
			m_fontStyle = value;
		}
	}

	public List<TextGlyph> Glyphs => m_glyphs;
}
