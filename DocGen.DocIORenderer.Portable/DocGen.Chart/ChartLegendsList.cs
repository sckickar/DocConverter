using System;
using System.Collections;

namespace DocGen.Chart;

internal class ChartLegendsList : ChartBaseList
{
	public ChartLegend this[int index]
	{
		get
		{
			return base.List[index] as ChartLegend;
		}
		set
		{
			base.List[index] = value;
		}
	}

	public ChartLegend this[string name]
	{
		get
		{
			ChartLegend result = null;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ChartLegend chartLegend = (ChartLegend)enumerator.Current;
					if (chartLegend.Name == name)
					{
						result = chartLegend;
					}
				}
				return result;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}
	}

	public int Add(ChartLegend value)
	{
		return base.List.Add(value);
	}

	public bool Contains(ChartLegend value)
	{
		return base.List.Contains(value);
	}

	public void Remove(ChartLegend value)
	{
		base.List.Remove(value);
	}

	public int IndexOf(ChartLegend value)
	{
		return base.List.IndexOf(value);
	}

	public void Insert(int index, ChartLegend value)
	{
		base.List.Insert(index, value);
	}

	protected override bool Validate(object obj)
	{
		if (obj is ChartLegend value)
		{
			return !Contains(value);
		}
		return false;
	}
}
