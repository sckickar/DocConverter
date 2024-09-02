using System;
using System.Collections;

namespace DocGen.Chart;

internal class ChartTitlesList : ChartBaseList
{
	public ChartTitle this[int index]
	{
		get
		{
			return base.List[index] as ChartTitle;
		}
		set
		{
			base.List[index] = value;
		}
	}

	public ChartTitle this[string name]
	{
		get
		{
			ChartTitle result = null;
			IEnumerator enumerator = GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					ChartTitle chartTitle = (ChartTitle)enumerator.Current;
					if (chartTitle.Name == name)
					{
						result = chartTitle;
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

	public int Add(ChartTitle value)
	{
		return base.List.Add(value);
	}

	public bool Contains(ChartTitle value)
	{
		return base.List.Contains(value);
	}

	public void Remove(ChartTitle value)
	{
		base.List.Remove(value);
	}

	public int IndexOf(ChartTitle value)
	{
		return base.List.IndexOf(value);
	}

	public void Insert(int index, ChartTitle value)
	{
		base.List.Insert(index, value);
	}

	protected override bool Validate(object obj)
	{
		if (obj != null)
		{
			return !base.List.Contains(obj);
		}
		return false;
	}
}
