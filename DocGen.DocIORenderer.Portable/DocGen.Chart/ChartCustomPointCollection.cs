namespace DocGen.Chart;

internal class ChartCustomPointCollection : ChartBaseList
{
	public ChartCustomPoint this[int index]
	{
		get
		{
			return base.List[index] as ChartCustomPoint;
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(ChartCustomPoint value)
	{
		return base.List.Add(value);
	}

	public bool Contains(ChartCustomPoint value)
	{
		return base.List.Contains(value);
	}

	public void Remove(ChartCustomPoint value)
	{
		base.List.Remove(value);
	}

	public int IndexOf(ChartCustomPoint value)
	{
		return base.List.IndexOf(value);
	}

	public void Insert(int index, ChartCustomPoint value)
	{
		base.List.Insert(index, value);
	}

	protected override bool Validate(object obj)
	{
		if (obj != null && obj is ChartCustomPoint)
		{
			return !base.List.Contains(obj);
		}
		return false;
	}
}
