namespace DocGen.DocIO.DLS;

public class WTableStyle : Style, IWTableStyle, IStyle
{
	private WParagraphFormat m_paragraphFormat;

	private WListFormat m_listFormat;

	private TableStyleCellProperties m_cellProperties;

	private TableStyleRowProperties m_rowProperties;

	private TableStyleTableProperties m_tableProperties;

	private ConditionalFormattingStyleCollection m_conditionalFormattingStyles;

	public WParagraphFormat ParagraphFormat => m_paragraphFormat;

	internal WListFormat ListFormat
	{
		get
		{
			if (m_listFormat == null)
			{
				m_listFormat = new WListFormat(base.Document, this);
			}
			return m_listFormat;
		}
	}

	public TableStyleCellProperties CellProperties => m_cellProperties;

	public TableStyleRowProperties RowProperties => m_rowProperties;

	public TableStyleTableProperties TableProperties => m_tableProperties;

	internal new WTableStyle BaseStyle => base.BaseStyle as WTableStyle;

	public override StyleType StyleType => StyleType.TableStyle;

	public ConditionalFormattingStyleCollection ConditionalFormattingStyles => m_conditionalFormattingStyles;

	internal WTableStyle(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_paragraphFormat = new WParagraphFormat(base.Document);
		m_paragraphFormat.SetOwner(this);
		m_cellProperties = new TableStyleCellProperties(base.Document);
		m_cellProperties.SetOwner(this);
		m_rowProperties = new TableStyleRowProperties(base.Document);
		m_rowProperties.SetOwner(this);
		m_tableProperties = new TableStyleTableProperties(base.Document);
		m_tableProperties.SetOwner(this);
		m_conditionalFormattingStyles = new ConditionalFormattingStyleCollection(base.Document);
	}

	internal ConditionalFormattingStyle ConditionalFormat(ConditionalFormattingType conditionCode)
	{
		return m_conditionalFormattingStyles.Add(conditionCode);
	}

	public override void ApplyBaseStyle(string styleName)
	{
		base.ApplyBaseStyle(styleName);
		if (BaseStyle != null)
		{
			m_paragraphFormat.ApplyBase(BaseStyle.ParagraphFormat);
			m_cellProperties.ApplyBase(BaseStyle.CellProperties);
			m_rowProperties.ApplyBase(BaseStyle.RowProperties);
			m_tableProperties.ApplyBase(BaseStyle.TableProperties);
		}
	}

	public void ApplyBaseStyle(BuiltinTableStyle tableStyle)
	{
		IStyle builtInTableStyle = base.Document.GetBuiltInTableStyle(tableStyle);
		ApplyBaseStyle(builtInTableStyle.Name);
	}

	private void CheckNormalStyle()
	{
		WTableStyle wTableStyle = base.Document.Styles.FindByName("Normal Table", StyleType.TableStyle) as WTableStyle;
		if (wTableStyle == null)
		{
			wTableStyle = (WTableStyle)Style.CreateBuiltinStyle(BuiltinTableStyle.TableNormal, base.Document);
			base.Document.Styles.Add(wTableStyle);
			base.Document.StyleNameIds.Add("TableNormal", wTableStyle.Name);
		}
	}

	public override IStyle Clone()
	{
		return (WTableStyle)CloneImpl();
	}

	internal override void ApplyBaseStyle(Style baseStyle)
	{
		base.ApplyBaseStyle(baseStyle);
		if (BaseStyle != null)
		{
			m_paragraphFormat.ApplyBase(BaseStyle.ParagraphFormat);
			m_cellProperties.ApplyBase(BaseStyle.CellProperties);
			m_rowProperties.ApplyBase(BaseStyle.RowProperties);
			m_tableProperties.ApplyBase(BaseStyle.TableProperties);
		}
	}

	protected override object CloneImpl()
	{
		WTableStyle wTableStyle = (WTableStyle)base.CloneImpl();
		wTableStyle.m_paragraphFormat = new WParagraphFormat(base.Document);
		wTableStyle.m_paragraphFormat.ImportContainer(ParagraphFormat);
		wTableStyle.m_paragraphFormat.CopyFormat(ParagraphFormat);
		wTableStyle.m_paragraphFormat.SetOwner(wTableStyle);
		wTableStyle.m_cellProperties = new TableStyleCellProperties(base.Document);
		wTableStyle.m_cellProperties.ImportContainer(CellProperties);
		wTableStyle.m_cellProperties.SetOwner(this);
		wTableStyle.m_rowProperties = new TableStyleRowProperties(base.Document);
		wTableStyle.m_rowProperties.ImportContainer(RowProperties);
		wTableStyle.m_rowProperties.SetOwner(this);
		wTableStyle.m_tableProperties = new TableStyleTableProperties(base.Document);
		wTableStyle.m_tableProperties.ImportContainer(TableProperties);
		wTableStyle.m_tableProperties.SetOwner(this);
		return wTableStyle;
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

	internal override bool CompareStyleBetweenDocuments(Style style)
	{
		if (StyleType != style.StyleType)
		{
			return false;
		}
		WTableStyle wTableStyle = (WTableStyle)style;
		bool flag = false;
		if (wTableStyle.CellProperties != null && CellProperties != null && !CellProperties.Compare(wTableStyle.CellProperties))
		{
			flag = true;
		}
		if (!flag && wTableStyle.RowProperties != null && RowProperties != null && !RowProperties.Compare(wTableStyle.RowProperties))
		{
			flag = true;
		}
		if (!flag && wTableStyle.TableProperties != null && TableProperties != null && !TableProperties.Compare(wTableStyle.TableProperties))
		{
			flag = true;
		}
		if (flag)
		{
			CellProperties.CompareProperties(wTableStyle.CellProperties);
			RowProperties.CompareProperties(wTableStyle.RowProperties);
			TableProperties.CompareProperties(wTableStyle.TableProperties);
		}
		return true;
	}
}
