namespace DocGen.PdfViewer.Base;

internal class TexturePaint
{
	private int m_heights;

	private int m_widths;

	private string m_images;

	public int Height
	{
		get
		{
			return m_heights;
		}
		set
		{
			m_heights = value;
		}
	}

	public int Width
	{
		get
		{
			return m_widths;
		}
		set
		{
			m_widths = value;
		}
	}

	public string Image
	{
		get
		{
			return m_images;
		}
		set
		{
			m_images = value;
		}
	}
}
