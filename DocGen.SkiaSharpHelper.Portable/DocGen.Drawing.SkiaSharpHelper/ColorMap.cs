namespace DocGen.Drawing.SkiaSharpHelper;

internal sealed class ColorMap
{
	private Color m_oldColor;

	private Color m_newColor;

	public Color OldColor
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

	public Color NewColor
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

	public ColorMap()
	{
		m_oldColor = Color.Empty;
		m_newColor = Color.Empty;
	}
}
