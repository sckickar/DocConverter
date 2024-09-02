using System;
using System.Collections;

namespace DocGen.Chart;

[Serializable]
internal class ChartStripLineCollection : CollectionBase
{
	public ChartStripLine this[int index] => base.List[index] as ChartStripLine;

	public event EventHandler Changed;

	public int IndexOf(ChartStripLine stripLine)
	{
		return base.InnerList.IndexOf(stripLine);
	}

	public void Add(ChartStripLine stripLine)
	{
		base.List.Add(stripLine);
	}

	public void Insert(int index, ChartStripLine stripLine)
	{
		base.List.Insert(index, stripLine);
	}

	public void Remove(ChartStripLine stripLine)
	{
		base.List.Remove(stripLine);
	}

	protected override void OnClear()
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((ChartStripLine)enumerator.Current).Changed -= OnStripLineChaged;
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
		base.OnClear();
	}

	protected override void OnClearComplete()
	{
		BroadcastChange();
		base.OnClearComplete();
	}

	protected override void OnInsert(int index, object value)
	{
		base.OnInsert(index, value);
	}

	protected override void OnInsertComplete(int index, object value)
	{
		((ChartStripLine)value).Changed += OnStripLineChaged;
		BroadcastChange();
		base.OnInsertComplete(index, value);
	}

	protected override void OnRemove(int index, object value)
	{
		((ChartStripLine)value).Changed -= OnStripLineChaged;
		base.OnRemove(index, value);
	}

	protected override void OnRemoveComplete(int index, object value)
	{
		BroadcastChange();
		base.OnRemoveComplete(index, value);
	}

	protected override void OnSet(int index, object value, object newValue)
	{
		((ChartStripLine)value).Changed -= OnStripLineChaged;
		base.OnSet(index, value, newValue);
	}

	protected override void OnSetComplete(int index, object oldValue, object newValue)
	{
		((ChartStripLine)newValue).Changed += OnStripLineChaged;
		BroadcastChange();
		base.OnSetComplete(index, oldValue, newValue);
	}

	private void BroadcastChange()
	{
		if (this.Changed != null)
		{
			this.Changed(this, EventArgs.Empty);
		}
	}

	private void OnStripLineChaged(object sender, EventArgs e)
	{
		BroadcastChange();
	}
}
