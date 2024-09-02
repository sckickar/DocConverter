using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class PageLayoutCollection : CollectionBase<PageLayout>
{
	private Dictionary<string, PageLayout> m_dictStyles;

	internal Dictionary<string, PageLayout> DictStyles
	{
		get
		{
			if (m_dictStyles == null)
			{
				m_dictStyles = new Dictionary<string, PageLayout>();
			}
			return m_dictStyles;
		}
		set
		{
			m_dictStyles = value;
		}
	}

	internal string Add(PageLayout layout)
	{
		string text = layout.Name;
		if (string.IsNullOrEmpty(layout.Name))
		{
			text = CollectionBase<PageLayout>.GenerateDefaultName("pl", DictStyles.Values);
		}
		if (!DictStyles.ContainsKey(text))
		{
			layout.Name = text;
			DictStyles.Add(text, layout);
		}
		return text;
	}

	internal void Remove(string key)
	{
		if (DictStyles.ContainsKey(key))
		{
			DictStyles.Remove(key);
		}
	}

	internal void Dispose()
	{
		if (m_dictStyles == null)
		{
			return;
		}
		foreach (PageLayout value in m_dictStyles.Values)
		{
			value.Dispose();
		}
		m_dictStyles.Clear();
		m_dictStyles = null;
	}
}
