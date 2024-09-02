using System.Collections.Generic;
using DocGen.Drawing;

namespace DocGen.Pdf;

public class TextLine
{
	private List<TextWord> m_wordCollection = new List<TextWord>();

	private RectangleF m_lineBounds;

	private float m_fontSize;

	private string m_fontName;

	private FontStyle m_fontStyle;

	private string m_text;

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

	public float FontSize
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

	public string FontName
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

	public FontStyle FontStyle
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

	public List<TextWord> WordCollection
	{
		get
		{
			return m_wordCollection;
		}
		internal set
		{
			m_wordCollection = value;
		}
	}

	public RectangleF Bounds
	{
		get
		{
			return m_lineBounds;
		}
		internal set
		{
			m_lineBounds = value;
		}
	}
}
