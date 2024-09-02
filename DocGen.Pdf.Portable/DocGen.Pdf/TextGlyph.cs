using DocGen.Drawing;

namespace DocGen.Pdf;

public class TextGlyph
{
	private RectangleF m_glyphBounds;

	private char m_text;

	private float m_fontSize;

	private string m_fontName;

	private FontStyle m_fontStyle;

	private int m_rotateAngle;

	private Color m_textcolor;

	public char Text
	{
		get
		{
			return m_text;
		}
		set
		{
			m_text = value;
		}
	}

	internal int RotateAngle
	{
		get
		{
			return m_rotateAngle;
		}
		set
		{
			m_rotateAngle = value;
		}
	}

	public RectangleF Bounds
	{
		get
		{
			return m_glyphBounds;
		}
		internal set
		{
			m_glyphBounds = value;
		}
	}

	public Color TextColor
	{
		get
		{
			return m_textcolor;
		}
		internal set
		{
			m_textcolor = value;
		}
	}

	public float FontSize
	{
		get
		{
			return m_fontSize;
		}
		internal set
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
		internal set
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
		internal set
		{
			m_fontStyle = value;
		}
	}
}
