namespace DocGen.Pdf;

internal class ImagePointer
{
	private int m_x;

	private int m_y;

	private int m_width;

	private int m_height;

	private JBIG2Image m_bitmap;

	internal ImagePointer(JBIG2Image bitmap)
	{
		m_bitmap = bitmap;
		m_height = bitmap.Height;
		m_width = bitmap.Width;
	}

	internal void SetPointer(int x, int y)
	{
		m_x = x;
		m_y = y;
	}

	internal int NextPixel()
	{
		if (m_y < 0 || m_y >= m_height || m_x >= m_width)
		{
			return 0;
		}
		if (m_x < 0)
		{
			m_x++;
			return 0;
		}
		int pixel = m_bitmap.GetPixel(m_x, m_y);
		m_x++;
		return pixel;
	}
}
