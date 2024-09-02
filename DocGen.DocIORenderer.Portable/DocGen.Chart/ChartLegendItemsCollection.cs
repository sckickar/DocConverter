using System.ComponentModel;

namespace DocGen.Chart;

[TypeConverter(typeof(CollectionConverter))]
internal class ChartLegendItemsCollection : ChartBaseList
{
	public ChartLegendItem this[int index] => base.List[index] as ChartLegendItem;

	public int Add(ChartLegendItem value)
	{
		return base.List.Add(value);
	}

	public void AddRange(ChartLegendItem[] range)
	{
		foreach (ChartLegendItem value in range)
		{
			Add(value);
		}
	}

	public void Remove(ChartLegendItem value)
	{
		base.List.Remove(value);
	}

	public void Insert(int index, ChartLegendItem value)
	{
		base.List.Insert(index, value);
	}

	public int IndexOf(ChartLegendItem value)
	{
		return base.List.IndexOf(value);
	}

	public new ChartLegendItem[] ToArray()
	{
		return ToArray(typeof(ChartLegendItem)) as ChartLegendItem[];
	}

	public bool Contains(ChartLegendItem value)
	{
		return base.List.Contains(value);
	}

	protected override bool Validate(object obj)
	{
		return obj != null;
	}
}
