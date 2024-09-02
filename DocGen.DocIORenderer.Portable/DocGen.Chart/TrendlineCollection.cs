using System;
using System.Collections;

namespace DocGen.Chart;

[Serializable]
internal class TrendlineCollection : CollectionBase
{
	public Trendline this[int index] => base.List[index] as Trendline;

	public event EventHandler Changed;

	public int IndexOf(Trendline trendline)
	{
		return base.InnerList.IndexOf(trendline);
	}

	public void Add(Trendline trendline)
	{
		base.List.Add(trendline);
	}

	public void Insert(int index, Trendline trendline)
	{
		base.List.Insert(index, trendline);
	}

	public void Remove(Trendline trendline)
	{
		base.List.Remove(trendline);
	}

	protected override void OnClear()
	{
		IEnumerator enumerator = GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				((Trendline)enumerator.Current).Changed -= OnTrendlineChaged;
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
		((Trendline)value).Changed += OnTrendlineChaged;
		BroadcastChange();
		base.OnInsertComplete(index, value);
	}

	protected override void OnRemove(int index, object value)
	{
		((Trendline)value).Changed -= OnTrendlineChaged;
		base.OnRemove(index, value);
	}

	protected override void OnRemoveComplete(int index, object value)
	{
		BroadcastChange();
		base.OnRemoveComplete(index, value);
	}

	protected override void OnSet(int index, object value, object newValue)
	{
		((Trendline)value).Changed -= OnTrendlineChaged;
		base.OnSet(index, value, newValue);
	}

	protected override void OnSetComplete(int index, object oldValue, object newValue)
	{
		((Trendline)newValue).Changed -= OnTrendlineChaged;
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

	private void OnTrendlineChaged(object sender, EventArgs e)
	{
		BroadcastChange();
	}
}
