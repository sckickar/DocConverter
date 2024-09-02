namespace DocGen.Chart;

internal class ChartAxisCollection : ChartBaseList
{
	public ChartAxis this[int index]
	{
		get
		{
			return base.List[index] as ChartAxis;
		}
		set
		{
			base.List[index] = value;
		}
	}

	public int Add(ChartAxis value)
	{
		return base.List.Add(value);
	}

	public int AddRange(ChartAxis[] values)
	{
		for (int i = 0; i < values.Length; i++)
		{
			base.List.Add(values[i]);
		}
		return base.List.Count - 1;
	}

	public bool Contains(ChartAxis value)
	{
		return base.List.Contains(value);
	}

	public void Remove(ChartAxis value)
	{
		base.List.Remove(value);
	}

	public int IndexOf(ChartAxis value)
	{
		return base.List.IndexOf(value);
	}

	public void Insert(int index, ChartAxis value)
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
