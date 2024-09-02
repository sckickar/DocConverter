using System.Collections.Generic;

namespace DocGen.DocIO.DLS;

internal class CSSStyle
{
	private List<CSSStyleItem> m_styleCollection;

	internal List<CSSStyleItem> StyleCollection
	{
		get
		{
			if (m_styleCollection == null)
			{
				m_styleCollection = new List<CSSStyleItem>();
			}
			return m_styleCollection;
		}
		set
		{
			m_styleCollection = value;
		}
	}

	internal CSSStyleItem GetCSSStyleItem(string styleName, CSSStyleItem.CssStyleType styleType)
	{
		for (int i = 0; i < StyleCollection.Count; i++)
		{
			CSSStyleItem cSSStyleItem = StyleCollection[i];
			if (cSSStyleItem.StyleName == styleName && cSSStyleItem.StyleType == styleType)
			{
				return cSSStyleItem;
			}
		}
		return null;
	}

	internal void Close()
	{
		if (m_styleCollection != null)
		{
			int num;
			for (num = 0; num < m_styleCollection.Count; num++)
			{
				CSSStyleItem cSSStyleItem = m_styleCollection[num];
				m_styleCollection.Remove(cSSStyleItem);
				cSSStyleItem.Close();
				num--;
			}
		}
	}
}
