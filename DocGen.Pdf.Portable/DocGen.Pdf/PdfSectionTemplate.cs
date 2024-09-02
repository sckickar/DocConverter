namespace DocGen.Pdf;

public class PdfSectionTemplate : PdfDocumentTemplate
{
	private bool m_left;

	private bool m_top;

	private bool m_right;

	private bool m_bottom;

	private bool m_stamp;

	public bool ApplyDocumentLeftTemplate
	{
		get
		{
			return m_left;
		}
		set
		{
			m_left = value;
		}
	}

	public bool ApplyDocumentTopTemplate
	{
		get
		{
			return m_top;
		}
		set
		{
			m_top = value;
		}
	}

	public bool ApplyDocumentRightTemplate
	{
		get
		{
			return m_right;
		}
		set
		{
			m_right = value;
		}
	}

	public bool ApplyDocumentBottomTemplate
	{
		get
		{
			return m_bottom;
		}
		set
		{
			m_bottom = value;
		}
	}

	public bool ApplyDocumentStamps
	{
		get
		{
			return m_stamp;
		}
		set
		{
			m_stamp = value;
		}
	}

	public PdfSectionTemplate()
	{
		m_left = (m_top = (m_right = (m_bottom = (m_stamp = true))));
	}
}
