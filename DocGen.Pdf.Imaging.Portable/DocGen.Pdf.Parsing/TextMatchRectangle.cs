using System.Drawing;
using DocGen.Drawing;

namespace DocGen.Pdf.Parsing;

internal class TextMatchRectangle
{
	private System.Drawing.RectangleF m_rect;

	private string m_text = string.Empty;

	private float m_textWidth;

	private float m_scaleX;

	private Font m_textFont;

	internal System.Drawing.RectangleF Rect
	{
		get
		{
			return m_rect;
		}
		set
		{
			m_rect = value;
		}
	}

	internal string Text
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

	internal float TextWidth
	{
		get
		{
			return m_textWidth;
		}
		set
		{
			m_textWidth = value;
		}
	}

	internal float ScaleX
	{
		get
		{
			return m_scaleX;
		}
		set
		{
			m_scaleX = value;
		}
	}

	internal Font TextFont
	{
		get
		{
			return m_textFont;
		}
		set
		{
			m_textFont = value;
		}
	}

	public TextMatchRectangle(System.Drawing.RectangleF rec, string txt, float txtWidth, float scaleX, Font font)
	{
		m_rect = rec;
		m_text = txt;
		m_textWidth = txtWidth;
		m_scaleX = scaleX;
		m_textFont = font;
	}
}
