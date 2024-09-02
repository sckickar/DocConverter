using System.Collections.Generic;

namespace DocGen.Office.Markdown;

internal class MdTableCell
{
	private List<IMdInline> m_Items;

	internal List<IMdInline> Items
	{
		get
		{
			if (m_Items == null)
			{
				m_Items = new List<IMdInline>();
			}
			return m_Items;
		}
		set
		{
			m_Items = value;
		}
	}

	internal void Close()
	{
		foreach (IMdInline item in Items)
		{
			item.Close();
		}
		if (m_Items != null)
		{
			m_Items.Clear();
			m_Items = null;
		}
	}
}
