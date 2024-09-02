using System.Collections.Generic;

namespace DocGen.Office;

internal class CDEFTable
{
	internal Dictionary<int, int> m_records = new Dictionary<int, int>();

	internal Dictionary<int, int> Records
	{
		get
		{
			return m_records;
		}
		set
		{
			m_records = value;
		}
	}

	internal int GetValue(int glyph)
	{
		Records.TryGetValue(glyph, out var value);
		return value;
	}

	internal bool IsMark(int glyph)
	{
		if (Records.ContainsKey(glyph) && GetValue(glyph) == 3)
		{
			return true;
		}
		return false;
	}
}
