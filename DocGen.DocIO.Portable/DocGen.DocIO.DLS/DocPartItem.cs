namespace DocGen.DocIO.DLS;

internal class DocPartItem
{
	private string m_docPartCategory;

	private string m_docPartGallery;

	private byte m_bFlags;

	internal string DocPartCategory
	{
		get
		{
			return m_docPartCategory;
		}
		set
		{
			m_docPartCategory = value;
		}
	}

	internal string DocPartGallery
	{
		get
		{
			return m_docPartGallery;
		}
		set
		{
			m_docPartGallery = value;
		}
	}

	internal bool IsDocPartUnique
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFEu) | (value ? 1u : 0u));
		}
	}
}
