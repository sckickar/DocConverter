namespace DocGen.Office.Markdown;

internal class MdPicture : IMdInline
{
	private byte[] m_imageBytes;

	private string m_altText;

	private string m_imageFormat;

	private string m_url;

	internal string AltText
	{
		get
		{
			return m_altText;
		}
		set
		{
			m_altText = value;
		}
	}

	internal byte[] ImageBytes
	{
		get
		{
			return m_imageBytes;
		}
		set
		{
			m_imageBytes = value;
		}
	}

	internal string ImageFormat
	{
		get
		{
			return m_imageFormat;
		}
		set
		{
			m_imageFormat = value;
		}
	}

	internal string Url
	{
		get
		{
			return m_url;
		}
		set
		{
			m_url = value;
		}
	}

	public void Close()
	{
		if (m_imageBytes != null)
		{
			m_imageBytes = null;
		}
	}
}
