namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class DefaultStyle
{
	private ODFFontFamily m_family;

	private MapStyle m_map;

	private ODFParagraphProperties m_paragraphProperties;

	private OTableCellProperties m_tableCellProperties;

	private OTableColumnProperties m_tableColumnProperties;

	private OTableProperties m_tableProperties;

	private OTableRowProperties m_tableRowProperties;

	private TextProperties m_textProperties;

	private SectionProperties m_sectionProperties;

	private string m_name;

	internal byte StylePropFlag;

	internal const byte MapKey = 0;

	internal const byte ParagraphPropertiesKey = 1;

	internal const byte TableCellPropertiesKey = 2;

	internal const byte TableColumnPropertiesKey = 3;

	internal const byte TablePropertiesKey = 4;

	internal const byte TableRowPropertiesKey = 5;

	internal const byte TextPropertiesKey = 6;

	internal const byte SectionPropertykey = 7;

	internal string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal ODFFontFamily Family
	{
		get
		{
			return m_family;
		}
		set
		{
			m_family = value;
		}
	}

	internal MapStyle Map
	{
		get
		{
			return m_map;
		}
		set
		{
			StylePropFlag = (byte)((StylePropFlag & 0xFEu) | 1u);
			m_map = value;
		}
	}

	internal ODFParagraphProperties ParagraphProperties
	{
		get
		{
			if (m_paragraphProperties == null)
			{
				m_paragraphProperties = new ODFParagraphProperties();
			}
			return m_paragraphProperties;
		}
		set
		{
			StylePropFlag = (byte)((StylePropFlag & 0xFDu) | 2u);
			m_paragraphProperties = value;
		}
	}

	internal SectionProperties ODFSectionProperties
	{
		get
		{
			if (m_sectionProperties == null)
			{
				m_sectionProperties = new SectionProperties();
			}
			return m_sectionProperties;
		}
		set
		{
			StylePropFlag = (byte)((StylePropFlag & 0x7Fu) | 0x80u);
			m_sectionProperties = value;
		}
	}

	internal OTableCellProperties TableCellProperties
	{
		get
		{
			return m_tableCellProperties;
		}
		set
		{
			StylePropFlag = (byte)((StylePropFlag & 0xFBu) | 4u);
			m_tableCellProperties = value;
		}
	}

	internal OTableColumnProperties TableColumnProperties
	{
		get
		{
			return m_tableColumnProperties;
		}
		set
		{
			StylePropFlag = (byte)((StylePropFlag & 0xF7u) | 8u);
			m_tableColumnProperties = value;
		}
	}

	internal OTableProperties TableProperties
	{
		get
		{
			return m_tableProperties;
		}
		set
		{
			StylePropFlag = (byte)((StylePropFlag & 0xEFu) | 0x10u);
			m_tableProperties = value;
		}
	}

	internal OTableRowProperties TableRowProperties
	{
		get
		{
			return m_tableRowProperties;
		}
		set
		{
			StylePropFlag = (byte)((StylePropFlag & 0xDFu) | 0x20u);
			m_tableRowProperties = value;
		}
	}

	internal TextProperties Textproperties
	{
		get
		{
			return m_textProperties;
		}
		set
		{
			StylePropFlag = (byte)((StylePropFlag & 0xBFu) | 0x40u);
			m_textProperties = value;
		}
	}

	internal void Dispose()
	{
		if (m_map != null)
		{
			m_map = null;
		}
		if (m_paragraphProperties != null)
		{
			m_paragraphProperties.Close();
			m_paragraphProperties = null;
		}
		if (m_tableCellProperties != null)
		{
			m_tableCellProperties = null;
		}
		if (m_tableColumnProperties != null)
		{
			m_tableColumnProperties = null;
		}
		if (m_tableRowProperties != null)
		{
			m_tableRowProperties = null;
		}
		if (m_tableProperties != null)
		{
			m_tableProperties = null;
		}
		if (m_textProperties != null)
		{
			m_textProperties = null;
		}
		if (m_sectionProperties != null)
		{
			m_sectionProperties.Close();
			m_sectionProperties = null;
		}
	}
}
