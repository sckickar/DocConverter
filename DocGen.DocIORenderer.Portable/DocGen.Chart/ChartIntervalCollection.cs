using System.Collections;
using System.Text;

namespace DocGen.Chart;

internal sealed class ChartIntervalCollection : IEnumerable
{
	private Hashtable m_table = new Hashtable();

	private ChartDateTimeRange m_parent;

	public ChartDateTimeInterval this[string name]
	{
		get
		{
			object obj = m_table[name];
			if (obj != null)
			{
				return obj as ChartDateTimeInterval;
			}
			return null;
		}
	}

	public ChartIntervalCollection(ChartDateTimeRange parent)
	{
		m_parent = parent;
	}

	public void Reset()
	{
		ChartDateTimeInterval chartDateTimeInterval = this["default"];
		Clear();
		if (chartDateTimeInterval != null)
		{
			m_table.Add("default", chartDateTimeInterval);
		}
	}

	public void Clear()
	{
		m_table.Clear();
	}

	public void Register(string name, ChartDateTimeInterval interval)
	{
		interval.SetParent(m_parent);
		m_table[name] = interval;
	}

	public void Remove(string name)
	{
		this[name].SetParent(null);
		m_table.Remove(name);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string key in m_table.Keys)
		{
			ChartDateTimeInterval chartDateTimeInterval = this[key];
			stringBuilder.AppendFormat("Interval Name: {0}, Details: {1}\r\n", key, chartDateTimeInterval.ToString());
		}
		return stringBuilder.ToString();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_table.Values.GetEnumerator();
	}
}
