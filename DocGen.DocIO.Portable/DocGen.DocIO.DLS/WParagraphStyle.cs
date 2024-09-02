using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

public class WParagraphStyle : Style, IWParagraphStyle, IStyle
{
	protected WParagraphFormat m_prFormat;

	protected WListFormat m_listFormat;

	private int m_listIndex = -1;

	private int m_listLevel = -1;

	public WParagraphFormat ParagraphFormat => m_prFormat;

	public new WParagraphStyle BaseStyle => base.BaseStyle as WParagraphStyle;

	public override StyleType StyleType => StyleType.ParagraphStyle;

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

	public WParagraphStyle(IWordDocument doc)
		: base((WordDocument)doc)
	{
		m_prFormat = new WParagraphFormat(base.Document);
		m_prFormat.SetOwner(this);
		if ((doc as WordDocument).CreateBaseStyle)
		{
			(doc as WordDocument).CreateBaseStyle = false;
			ApplyBaseStyle(BuiltinStyle.Normal);
			(doc as WordDocument).CreateBaseStyle = true;
		}
	}

	public override void ApplyBaseStyle(string styleName)
	{
		base.ApplyBaseStyle(styleName);
		if (BaseStyle != null)
		{
			m_prFormat.ApplyBase(BaseStyle.ParagraphFormat);
		}
	}

	public override IStyle Clone()
	{
		return (WParagraphStyle)CloneImpl();
	}

	internal override bool Compare(Style style)
	{
		if (!base.Compare(style))
		{
			return false;
		}
		if ((style as WParagraphStyle).ParagraphFormat != null && ParagraphFormat != null)
		{
			if (!ParagraphFormat.Compare((style as WParagraphStyle).ParagraphFormat))
			{
				return false;
			}
			if (!ListFormat.Compare((style as WParagraphStyle).ListFormat))
			{
				return false;
			}
		}
		return true;
	}

	internal override void ApplyBaseStyle(Style baseStyle)
	{
		base.ApplyBaseStyle(baseStyle);
		if (BaseStyle != null)
		{
			m_prFormat.ApplyBase(BaseStyle.ParagraphFormat);
		}
	}

	internal WListFormat GetListFormatIncludeBaseStyle()
	{
		for (WParagraphStyle wParagraphStyle = this; wParagraphStyle != null; wParagraphStyle = wParagraphStyle.BaseStyle)
		{
			if (wParagraphStyle.ListFormat.ListType != ListType.NoList || wParagraphStyle.ListFormat.IsEmptyList)
			{
				return wParagraphStyle.ListFormat;
			}
		}
		return null;
	}

	internal int GetListLevelNumberIncludeBaseStyle(ref WParagraphStyle levelNumberStyle)
	{
		for (WParagraphStyle wParagraphStyle = this; wParagraphStyle != null; wParagraphStyle = wParagraphStyle.BaseStyle)
		{
			if (wParagraphStyle.ListFormat.HasKey(0))
			{
				levelNumberStyle = wParagraphStyle;
				return wParagraphStyle.ListFormat.ListLevelNumber;
			}
		}
		return 0;
	}

	protected override object CloneImpl()
	{
		WParagraphStyle wParagraphStyle = (WParagraphStyle)base.CloneImpl();
		wParagraphStyle.m_prFormat = new WParagraphFormat(base.Document);
		wParagraphStyle.m_prFormat.ImportContainer(ParagraphFormat);
		wParagraphStyle.m_prFormat.CopyFormat(ParagraphFormat);
		wParagraphStyle.m_prFormat.SetOwner(wParagraphStyle);
		wParagraphStyle.m_listFormat = new WListFormat(base.Document, this);
		wParagraphStyle.m_listFormat.ImportContainer(ListFormat);
		wParagraphStyle.m_listFormat.SetOwner(wParagraphStyle);
		if (BaseStyle != null)
		{
			wParagraphStyle.ApplyBaseStyle(BaseStyle);
		}
		return wParagraphStyle;
	}

	void IWParagraphStyle.Close()
	{
		Close();
	}

	internal override void Close()
	{
		base.Close();
		if (m_prFormat != null)
		{
			m_prFormat.Close();
			m_prFormat = null;
		}
		if (m_listFormat != null)
		{
			m_listFormat.Close();
			m_listFormat = null;
		}
	}

	internal override bool CompareStyleBetweenDocuments(Style style)
	{
		if (StyleType != style.StyleType)
		{
			return false;
		}
		WParagraphStyle wParagraphStyle = (WParagraphStyle)style;
		bool flag = false;
		if (wParagraphStyle.CharacterFormat != null && base.CharacterFormat != null && !base.CharacterFormat.Compare(wParagraphStyle.CharacterFormat))
		{
			flag = true;
		}
		if (!flag && wParagraphStyle.ListFormat != null && ListFormat != null && !ListFormat.Compare(wParagraphStyle.ListFormat))
		{
			flag = true;
		}
		if (!flag && ParagraphFormat != null && wParagraphStyle.ParagraphFormat != null && !ParagraphFormat.Compare(wParagraphStyle.ParagraphFormat))
		{
			flag = true;
		}
		if (flag)
		{
			base.CharacterFormat.CompareProperties(wParagraphStyle.CharacterFormat);
			base.CharacterFormat.IsChangedFormat = true;
			base.CharacterFormat.FormatChangeAuthorName = base.Document.m_authorName;
			base.CharacterFormat.FormatChangeDateTime = base.Document.m_dateTime;
			base.Document.CharacterFormatChange(base.CharacterFormat, null, null);
			ListFormat.CompareProperties(wParagraphStyle.ListFormat);
			if (wParagraphStyle.ListFormat.CurrentListStyle != null)
			{
				ListFormat.ApplyStyle(wParagraphStyle.ListFormat.CurrentListStyle.Name);
			}
			ParagraphFormat.CompareProperties(wParagraphStyle.ParagraphFormat);
			ParagraphFormat.IsChangedFormat = true;
			ParagraphFormat.FormatChangeAuthorName = base.Document.m_authorName;
			ParagraphFormat.FormatChangeDateTime = base.Document.m_dateTime;
			base.Document.ParagraphFormatChange(ParagraphFormat);
			AddDefaultFormat(base.CharacterFormat, ParagraphFormat);
		}
		return true;
	}

	private void AddDefaultFormat(WCharacterFormat characterFormat, WParagraphFormat paragraphFormat)
	{
		foreach (KeyValuePair<int, object> item in characterFormat.Document.DefCharFormat.PropertiesHash)
		{
			if (!characterFormat.OldPropertiesHash.ContainsKey(item.Key))
			{
				characterFormat.OldPropertiesHash[item.Key] = item.Value;
			}
		}
		foreach (KeyValuePair<int, object> item2 in paragraphFormat.Document.DefParaFormat.PropertiesHash)
		{
			if (!paragraphFormat.OldPropertiesHash.ContainsKey(item2.Key))
			{
				paragraphFormat.OldPropertiesHash[item2.Key] = item2.Value;
			}
		}
	}

	protected override void InitXDLSHolder()
	{
		base.InitXDLSHolder();
		base.XDLSHolder.AddElement("paragraph-format", m_prFormat);
	}
}
