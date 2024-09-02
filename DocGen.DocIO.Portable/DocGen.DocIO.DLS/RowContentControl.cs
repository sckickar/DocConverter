namespace DocGen.DocIO.DLS;

internal class RowContentControl : IRowContentControl
{
	private ContentControlProperties m_controlProperties;

	private WCharacterFormat m_BreakCharacterFormat;

	private WTableRow m_ownerRow;

	public ContentControlProperties ContentControlProperties
	{
		get
		{
			return m_controlProperties;
		}
		set
		{
			m_controlProperties = value;
		}
	}

	public WCharacterFormat BreakCharacterFormat
	{
		get
		{
			return m_BreakCharacterFormat;
		}
		set
		{
			m_BreakCharacterFormat = value;
		}
	}

	internal WTableRow OwnerRow
	{
		get
		{
			return m_ownerRow;
		}
		set
		{
			m_ownerRow = value;
		}
	}

	public RowContentControl(WordDocument document)
	{
		m_controlProperties = new ContentControlProperties(document);
		m_BreakCharacterFormat = new WCharacterFormat(document);
	}

	internal void Close()
	{
		if (m_controlProperties != null)
		{
			m_controlProperties.Close();
			m_controlProperties = null;
		}
		if (m_BreakCharacterFormat != null)
		{
			m_BreakCharacterFormat.Close();
			m_BreakCharacterFormat = null;
		}
	}
}
