using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class DefaultStyleCollection : CollectionBase<PageLayout>
{
	private Dictionary<string, DefaultStyle> m_defaultStyles;

	internal Dictionary<string, DefaultStyle> DefaultStyles
	{
		get
		{
			if (m_defaultStyles == null)
			{
				m_defaultStyles = new Dictionary<string, DefaultStyle>();
			}
			return m_defaultStyles;
		}
		set
		{
			m_defaultStyles = value;
		}
	}

	internal string Add(DefaultStyle style)
	{
		string text = style.Name;
		if (string.IsNullOrEmpty(style.Name))
		{
			text = CollectionBase<PageLayout>.GenerateDefaultName(MapName(style), DefaultStyles.Values);
		}
		if (!m_defaultStyles.ContainsKey(text))
		{
			string text2 = ContainsValue(style);
			if (text2 != null)
			{
				text = text2;
			}
			else
			{
				style.Name = text;
				DefaultStyles.Add(text, style);
			}
		}
		return text;
	}

	private string ContainsValue(DefaultStyle style)
	{
		string result = null;
		foreach (DefaultStyle value in DefaultStyles.Values)
		{
			if (value.Equals(style))
			{
				result = value.Name;
				break;
			}
		}
		return result;
	}

	private string MapName(DefaultStyle style)
	{
		string text = "ce";
		return style.Family switch
		{
			ODFFontFamily.Table => "ta", 
			ODFFontFamily.Table_Column => "co", 
			ODFFontFamily.Table_Row => "ro", 
			ODFFontFamily.Paragraph => "P", 
			ODFFontFamily.Text => "T", 
			_ => text, 
		};
	}
}
