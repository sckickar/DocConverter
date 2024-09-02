using System;
using System.Collections;

namespace DocGen.Chart;

internal class ChartAxisLabelCollection : CollectionBase, IChartAxisLabelModel
{
	public ChartAxisLabel this[int index]
	{
		get
		{
			if (index < 0 || index >= base.List.Count)
			{
				throw new IndexOutOfRangeException($"Invalid Index {index} items count {base.List.Count}");
			}
			return base.List[index] as ChartAxisLabel;
		}
	}

	public event EventHandler Changed;

	public int IndexOf(ChartAxisLabel label)
	{
		return base.InnerList.IndexOf(label);
	}

	public void Add(ChartAxisLabel label)
	{
		if (label == null)
		{
			throw new ArgumentNullException("label");
		}
		if (base.List.IndexOf(label) == -1)
		{
			base.List.Add(label);
		}
	}

	public void Insert(int index, ChartAxisLabel label)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", index, "Value can not be less than 0.");
		}
		if (label == null)
		{
			throw new ArgumentNullException("label");
		}
		base.List.Insert(index, label);
	}

	public void Remove(ChartAxisLabel label)
	{
		base.List.Remove(label);
	}

	public ChartAxisLabel GetLabelAt(int index)
	{
		return this[index];
	}

	protected override void OnClearComplete()
	{
		BroadcastChange();
		base.OnClearComplete();
	}

	protected override void OnInsertComplete(int index, object value)
	{
		BroadcastChange();
		base.OnInsertComplete(index, value);
	}

	protected override void OnRemoveComplete(int index, object value)
	{
		BroadcastChange();
		base.OnRemoveComplete(index, value);
	}

	protected override void OnSetComplete(int index, object oldValue, object newValue)
	{
		BroadcastChange();
		base.OnSetComplete(index, oldValue, newValue);
	}

	protected override void OnValidate(object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
	}

	private void BroadcastChange()
	{
		if (this.Changed != null)
		{
			this.Changed(this, EventArgs.Empty);
		}
	}
}
