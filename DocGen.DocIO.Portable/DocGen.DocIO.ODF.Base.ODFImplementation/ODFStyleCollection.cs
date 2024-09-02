using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class ODFStyleCollection : CollectionBase<ODFStyle>
{
	private Dictionary<string, ODFStyle> m_dictStyles;

	internal Dictionary<string, ODFStyle> DictStyles
	{
		get
		{
			if (m_dictStyles == null)
			{
				m_dictStyles = new Dictionary<string, ODFStyle>();
			}
			return m_dictStyles;
		}
		set
		{
			m_dictStyles = value;
		}
	}

	internal string Add(ODFStyle style)
	{
		string text = style.Name;
		if (string.IsNullOrEmpty(style.Name))
		{
			text = CollectionBase<ODFStyle>.GenerateDefaultName(MapName(style), DictStyles.Values);
		}
		if (!DictStyles.ContainsKey(text))
		{
			string text2 = ContainsValue(style);
			if (text2 != null)
			{
				text = text2;
			}
			else
			{
				style.Name = text;
				DictStyles.Add(text, style);
			}
		}
		return text;
	}

	internal string Add(ODFStyle style, int index)
	{
		string text = style.Name;
		if (string.IsNullOrEmpty(style.Name))
		{
			text = CollectionBase<ODFStyle>.GenerateDefaultName(MapName(style), DictStyles.Values);
		}
		if (!DictStyles.ContainsKey(text))
		{
			string text2 = ContainsValue(style);
			if (text2 != null)
			{
				text = text2;
			}
			else
			{
				style.Name = text;
				DictStyles.Add(index.ToString(), style);
			}
		}
		return text;
	}

	private string ContainsValue(ODFStyle style)
	{
		string result = null;
		foreach (ODFStyle value in DictStyles.Values)
		{
			if (value.Family == style.Family && value.Equals(style))
			{
				result = value.Name;
				break;
			}
		}
		return result;
	}

	private string MapName(ODFStyle style)
	{
		string text = "ce";
		return style.Family switch
		{
			ODFFontFamily.Table => "ta", 
			ODFFontFamily.Table_Column => "co", 
			ODFFontFamily.Table_Row => "ro", 
			ODFFontFamily.Paragraph => "P", 
			ODFFontFamily.Text => "T", 
			ODFFontFamily.Section => "S", 
			ODFFontFamily.Graphic => "A", 
			_ => text, 
		};
	}

	internal void Dispose()
	{
		if (m_dictStyles == null)
		{
			return;
		}
		foreach (ODFStyle value in m_dictStyles.Values)
		{
			value.Dispose();
		}
		m_dictStyles.Clear();
		m_dictStyles = null;
	}
}
