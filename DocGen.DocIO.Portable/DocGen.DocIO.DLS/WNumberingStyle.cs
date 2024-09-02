namespace DocGen.DocIO.DLS;

internal class WNumberingStyle : Style
{
	private WParagraphFormat m_paragraphFormat;

	protected WListFormat m_listFormat;

	private int m_listIndex = -1;

	private int m_listLevel = -1;

	public WParagraphFormat ParagraphFormat => m_paragraphFormat;

	public new WNumberingStyle BaseStyle => base.BaseStyle as WNumberingStyle;

	public override StyleType StyleType => StyleType.NumberingStyle;

	public WListFormat ListFormat
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

	internal int ListIndex
	{
		get
		{
			return m_listIndex;
		}
		set
		{
			m_listIndex = value;
		}
	}

	internal int ListLevel
	{
		get
		{
			return m_listLevel;
		}
		set
		{
			m_listLevel = value;
		}
	}

	internal WNumberingStyle(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_paragraphFormat = new WParagraphFormat(base.Document);
		m_paragraphFormat.SetOwner(this);
	}

	public override IStyle Clone()
	{
		return (WNumberingStyle)CloneImpl();
	}

	protected override object CloneImpl()
	{
		WNumberingStyle wNumberingStyle = (WNumberingStyle)base.CloneImpl();
		wNumberingStyle.m_paragraphFormat = new WParagraphFormat(base.Document);
		wNumberingStyle.m_paragraphFormat.ImportContainer(ParagraphFormat);
		wNumberingStyle.m_paragraphFormat.SetOwner(wNumberingStyle);
		wNumberingStyle.m_listFormat = new WListFormat(base.Document, this);
		wNumberingStyle.m_listFormat.ImportContainer(ListFormat);
		wNumberingStyle.m_listFormat.SetOwner(wNumberingStyle);
		return wNumberingStyle;
	}

	internal override void Close()
	{
		base.Close();
		if (m_paragraphFormat != null)
		{
			m_paragraphFormat.Close();
			m_paragraphFormat = null;
		}
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("paragraph-format", m_paragraphFormat);
	}

	internal override bool CompareStyleBetweenDocuments(Style style)
	{
		if (StyleType != style.StyleType)
		{
			return false;
		}
		WNumberingStyle wNumberingStyle = (WNumberingStyle)style;
		bool flag = false;
		if (wNumberingStyle.ListFormat != null && ListFormat != null && !ListFormat.Compare(wNumberingStyle.ListFormat))
		{
			flag = true;
		}
		if (!flag && ParagraphFormat != null && wNumberingStyle.ParagraphFormat != null && !ParagraphFormat.Compare(wNumberingStyle.ParagraphFormat))
		{
			flag = true;
		}
		if (flag)
		{
			ListFormat.CompareProperties(wNumberingStyle.ListFormat);
			if (wNumberingStyle.ListFormat.CurrentListStyle != null)
			{
				ListFormat.ApplyStyle(wNumberingStyle.ListFormat.CurrentListStyle.Name);
			}
			ParagraphFormat.CompareProperties(wNumberingStyle.ParagraphFormat);
			ParagraphFormat.IsChangedFormat = true;
			ParagraphFormat.FormatChangeAuthorName = base.Document.m_authorName;
			ParagraphFormat.FormatChangeDateTime = base.Document.m_dateTime;
			base.Document.ParagraphFormatChange(ParagraphFormat);
		}
		return true;
	}
}
