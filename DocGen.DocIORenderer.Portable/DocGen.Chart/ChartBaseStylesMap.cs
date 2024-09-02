using System.Collections;

namespace DocGen.Chart;

internal class ChartBaseStylesMap
{
	private const int c_maxLevel = 16;

	internal const string StandardName = "Standard";

	private Hashtable m_table = new Hashtable();

	public ChartBaseStyleInfo this[string name] => Lookup(name);

	public ChartBaseStylesMap()
	{
		Register(new ChartBaseStyleInfo("Standard"));
	}

	public void Register(ChartBaseStyleInfo style)
	{
		m_table[style.Name] = style;
		style.Identity = new ChartBaseStyleIdentity(this);
	}

	public ChartBaseStyleInfo Lookup(string name)
	{
		if (m_table.ContainsKey(name))
		{
			ChartBaseStyleInfo chartBaseStyleInfo = m_table[name] as ChartBaseStyleInfo;
			if (chartBaseStyleInfo != null)
			{
				chartBaseStyleInfo.Identity = new ChartBaseStyleIdentity(this);
			}
			return chartBaseStyleInfo;
		}
		return null;
	}

	public void Remove(string name)
	{
		m_table.Remove(name);
	}

	public void Clear()
	{
		m_table.Clear();
	}

	internal ChartStyleInfo[] GetBaseStyles(ChartStyleInfo styleInfo)
	{
		ArrayList arrayList = new ArrayList();
		while (styleInfo.HasBaseStyle)
		{
			styleInfo = Lookup(styleInfo.BaseStyle);
			arrayList.Add(styleInfo);
		}
		arrayList.Add(Lookup("Standard"));
		return arrayList.ToArray(typeof(ChartStyleInfo)) as ChartStyleInfo[];
	}

	internal ChartStyleInfo[] GetSubBaseStyles(ChartStyleInfo styleInfo, ChartStyleInfo baseStyleInfo)
	{
		ArrayList arrayList = new ArrayList();
		while (styleInfo.HasBaseStyle)
		{
			styleInfo = Lookup(styleInfo.BaseStyle);
			arrayList.Add(styleInfo);
		}
		arrayList.Add(baseStyleInfo);
		arrayList.AddRange(GetBaseStyles(baseStyleInfo));
		return arrayList.ToArray(typeof(ChartStyleInfo)) as ChartStyleInfo[];
	}

	internal ChartStyleInfo[] GetSubBaseStyles(ChartStyleInfo styleInfo, ChartStyleInfo[] styles)
	{
		ArrayList arrayList = new ArrayList();
		while (styleInfo.HasBaseStyle)
		{
			styleInfo = Lookup(styleInfo.BaseStyle);
			arrayList.Add(styleInfo);
		}
		for (int i = 0; i < styles.Length; i++)
		{
			styleInfo = styles[i];
			arrayList.Add(styleInfo);
			while (styleInfo.HasBaseStyle)
			{
				styleInfo = Lookup(styleInfo.BaseStyle);
				arrayList.Add(styleInfo);
			}
		}
		arrayList.Add(Lookup("Standard"));
		return arrayList.ToArray(typeof(ChartStyleInfo)) as ChartStyleInfo[];
	}
}
