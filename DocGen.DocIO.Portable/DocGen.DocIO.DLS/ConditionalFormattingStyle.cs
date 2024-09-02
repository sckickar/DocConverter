namespace DocGen.DocIO.DLS;

public class ConditionalFormattingStyle : Style
{
	private WParagraphFormat m_paragraphFormat;

	private TableStyleCellProperties m_cellProperties;

	private TableStyleRowProperties m_rowProperties;

	private TableStyleTableProperties m_tableProperties;

	private ConditionalFormattingType m_conditionalFormattingType;

	public WParagraphFormat ParagraphFormat => m_paragraphFormat;

	public TableStyleCellProperties CellProperties => m_cellProperties;

	public TableStyleRowProperties RowProperties => m_rowProperties;

	public TableStyleTableProperties TableProperties => m_tableProperties;

	public ConditionalFormattingType ConditionalFormattingType => m_conditionalFormattingType;

	public override StyleType StyleType => StyleType.TableStyle;

	internal ConditionalFormattingStyle(ConditionalFormattingType conditionCode, IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_conditionalFormattingType = conditionCode;
		m_paragraphFormat = new WParagraphFormat(base.Document);
		m_paragraphFormat.SetOwner(this);
		m_cellProperties = new TableStyleCellProperties(base.Document);
		m_cellProperties.SetOwner(this);
		m_rowProperties = new TableStyleRowProperties(base.Document);
		m_rowProperties.SetOwner(this);
		m_tableProperties = new TableStyleTableProperties(base.Document);
		m_tableProperties.SetOwner(this);
	}

	public override IStyle Clone()
	{
		return (ConditionalFormattingStyle)CloneImpl();
	}

	protected override object CloneImpl()
	{
		ConditionalFormattingStyle conditionalFormattingStyle = (ConditionalFormattingStyle)base.CloneImpl();
		conditionalFormattingStyle.m_paragraphFormat = new WParagraphFormat(base.Document);
		conditionalFormattingStyle.m_paragraphFormat.ImportContainer(ParagraphFormat);
		conditionalFormattingStyle.m_paragraphFormat.SetOwner(conditionalFormattingStyle);
		conditionalFormattingStyle.m_cellProperties = new TableStyleCellProperties(base.Document);
		conditionalFormattingStyle.m_cellProperties.ImportContainer(CellProperties);
		conditionalFormattingStyle.m_cellProperties.SetOwner(this);
		conditionalFormattingStyle.m_rowProperties = new TableStyleRowProperties(base.Document);
		conditionalFormattingStyle.m_rowProperties.ImportContainer(RowProperties);
		conditionalFormattingStyle.m_rowProperties.SetOwner(this);
		conditionalFormattingStyle.m_tableProperties = new TableStyleTableProperties(base.Document);
		conditionalFormattingStyle.m_tableProperties.ImportContainer(TableProperties);
		conditionalFormattingStyle.m_tableProperties.SetOwner(this);
		return conditionalFormattingStyle;
	}

	internal override void Close()
	{
		base.Close();
		if (m_paragraphFormat != null)
		{
			m_paragraphFormat.Close();
			m_paragraphFormat = null;
		}
		if (m_cellProperties != null)
		{
			m_cellProperties.Close();
			m_cellProperties = null;
		}
		if (m_rowProperties != null)
		{
			m_rowProperties.Close();
			m_rowProperties = null;
		}
		if (m_tableProperties != null)
		{
			m_tableProperties.Close();
			m_tableProperties = null;
		}
	}
}
