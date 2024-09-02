using System;
using System.Collections;

namespace DocGen.Chart;

internal class ChartAxisGroupingLabelCollection : CollectionBase, IChartAxisGroupingLabelModel
{
	public ChartAxisGroupingLabel this[int index] => base.List[index] as ChartAxisGroupingLabel;

	public event EventHandler Changed;

	public int IndexOf(ChartAxisGroupingLabel label)
	{
		return base.InnerList.IndexOf(label);
	}

	public void Add(ChartAxisGroupingLabel label)
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

	public void Insert(int index, ChartAxisGroupingLabel label)
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

	public void Remove(ChartAxisGroupingLabel label)
	{
		base.List.Remove(label);
	}

	public ChartAxisGroupingLabel GetGroupingLabelAt(int index)
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
