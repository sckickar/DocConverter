namespace DocGen.DocIO.DLS;

internal class CellContentControl : WTextBody, ICellContentControl
{
	private ContentControlProperties m_contentControlProperties;

	private WCharacterFormat m_BreakCharacterFormat;

	private WTableCell m_ownerCell;

	private WTableCell m_mappedCell;

	internal WTableCell MappedCell
	{
		get
		{
			return m_mappedCell;
		}
		set
		{
			m_mappedCell = value;
		}
	}

	internal WTableCell OwnerCell
	{
		get
		{
			return m_ownerCell;
		}
		set
		{
			m_ownerCell = value;
		}
	}

	public ContentControlProperties ContentControlProperties
	{
		get
		{
			return m_contentControlProperties;
		}
		set
		{
			m_contentControlProperties = value;
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

	public CellContentControl(WordDocument document)
		: base(document, null)
	{
		m_contentControlProperties = new ContentControlProperties(document, this);
		m_BreakCharacterFormat = new WCharacterFormat(document);
	}

	internal new void Close()
	{
		if (m_contentControlProperties != null)
		{
			m_contentControlProperties.Close();
			m_contentControlProperties = null;
		}
		if (m_BreakCharacterFormat != null)
		{
			m_BreakCharacterFormat.Close();
			m_BreakCharacterFormat = null;
		}
	}
}
