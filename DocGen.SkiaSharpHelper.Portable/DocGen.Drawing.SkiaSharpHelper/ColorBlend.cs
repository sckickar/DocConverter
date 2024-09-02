namespace DocGen.Drawing.SkiaSharpHelper;

internal sealed class ColorBlend
{
	private Color[] m_colors;

	private float[] m_positions;

	internal Color[] Colors
	{
		get
		{
			return m_colors;
		}
		set
		{
			m_colors = value;
		}
	}

	internal float[] Positions
	{
		get
		{
			return m_positions;
		}
		set
		{
			m_positions = value;
		}
	}

	internal ColorBlend()
	{
		m_colors = new Color[1];
		m_positions = new float[1];
	}

	internal ColorBlend(int count)
	{
		m_colors = new Color[count];
		m_positions = new float[count];
	}
}
