using DocGen.Drawing;

namespace DocGen.Pdf.Graphics;

internal sealed class ColorMap
{
	private Color m_oldColor;

	private Color m_newColor;

	internal Color OldColor
	{
		get
		{
			return m_oldColor;
		}
		set
		{
			m_oldColor = value;
		}
	}

	internal Color NewColor
	{
		get
		{
			return m_newColor;
		}
		set
		{
			m_newColor = value;
		}
	}

	internal ColorMap()
	{
		m_oldColor = Color.Empty;
		m_newColor = Color.Empty;
	}
}
