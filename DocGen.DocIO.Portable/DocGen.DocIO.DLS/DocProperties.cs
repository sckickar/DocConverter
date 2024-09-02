namespace DocGen.DocIO.DLS;

public class DocProperties
{
	private IWordDocument m_document;

	private DocumentVersion m_version;

	private Hyphenation m_hyphenation;

	public bool FormFieldShading
	{
		get
		{
			return (m_document as WordDocument).DOP.FormFieldShading;
		}
		set
		{
			(m_document as WordDocument).DOP.FormFieldShading = value;
		}
	}

	public DocumentVersion Version => m_version;

	public Hyphenation Hyphenation
	{
		get
		{
			if (m_hyphenation == null)
			{
				m_hyphenation = new Hyphenation(m_document);
			}
			return m_hyphenation;
		}
	}

	internal DocProperties(IWordDocument document)
	{
		m_document = document;
	}

	internal void SetVersion(DocumentVersion version)
	{
		m_version = version;
	}

	internal void Close()
	{
		m_document = null;
	}
}
