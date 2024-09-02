namespace DocGen.Pdf;

public class PdfCompressionOptions
{
	private bool m_removeMetadata = true;

	private bool m_compressImages = true;

	private int m_imageQuality = 70;

	private bool m_optimizePageContents = true;

	private bool m_optimizeFont = true;

	public bool RemoveMetadata
	{
		get
		{
			return m_removeMetadata;
		}
		set
		{
			m_removeMetadata = value;
		}
	}

	public bool CompressImages
	{
		get
		{
			return m_compressImages;
		}
		set
		{
			m_compressImages = value;
		}
	}

	public int ImageQuality
	{
		get
		{
			return m_imageQuality;
		}
		set
		{
			m_imageQuality = value;
		}
	}

	public bool OptimizePageContents
	{
		get
		{
			return m_optimizePageContents;
		}
		set
		{
			m_optimizePageContents = value;
		}
	}

	public bool OptimizeFont
	{
		get
		{
			return m_optimizeFont;
		}
		set
		{
			m_optimizeFont = value;
		}
	}
}
