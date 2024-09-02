using System.Collections;
using System.ComponentModel;

namespace DocGen.Chart;

internal class ChartSeriesModel : CollectionBase, IEditableChartSeriesModel, IChartSeriesModel, IChartSeriesCategory, IChartEditableCategory
{
	protected class SeriesEntity
	{
		internal double[] m_yValues;

		internal double m_x;

		internal bool m_isEmpty;

		internal string m_category;

		public double X
		{
			get
			{
				return m_x;
			}
			set
			{
				m_x = value;
			}
		}

		public double[] Y
		{
			get
			{
				return m_yValues;
			}
			set
			{
				m_yValues = value;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return m_isEmpty;
			}
			set
			{
				m_isEmpty = value;
			}
		}

		public string Category
		{
			get
			{
				return m_category;
			}
			set
			{
				m_category = value;
			}
		}

		public SeriesEntity(double x, double[] yValues)
		{
			m_x = x;
			m_yValues = yValues;
		}

		public SeriesEntity(string category)
		{
			m_category = category;
		}

		public SeriesEntity(double x, double[] yValues, string category)
		{
			m_x = x;
			m_yValues = yValues;
			m_category = category;
		}

		public SeriesEntity(double x, double[] yValues, bool isEmpty)
		{
			m_x = x;
			m_yValues = yValues;
			m_isEmpty = isEmpty;
		}

		public SeriesEntity(double x, double[] yValues, bool isEmpty, string category)
		{
			m_x = x;
			m_yValues = yValues;
			m_isEmpty = isEmpty;
			m_category = category;
		}

		public SeriesEntity(double x, double y)
			: this(x, new double[1] { y })
		{
		}

		public SeriesEntity(double x, double y, string category)
			: this(x, new double[1] { y }, category)
		{
		}
	}

	public event ListChangedEventHandler Changed;

	public double GetX(int xIndex)
	{
		return (base.List[xIndex] as SeriesEntity).m_x;
	}

	public double[] GetY(int xIndex)
	{
		return (base.List[xIndex] as SeriesEntity).m_yValues;
	}

	public bool GetEmpty(int xIndex)
	{
		return (base.List[xIndex] as SeriesEntity).m_isEmpty;
	}

	public string GetCategory(int xIndex)
	{
		string text = ((base.List.Count > xIndex) ? (base.List[xIndex] as SeriesEntity).m_category : xIndex.ToString());
		if (text != null)
		{
			return text;
		}
		return GetX(xIndex).ToString();
	}

	public object GetCategoryOrX(int xIndex)
	{
		object obj = null;
		if (base.List.Count > xIndex)
		{
			obj = (base.List[xIndex] as SeriesEntity).m_category;
		}
		if (obj == null)
		{
			obj = GetX(xIndex);
		}
		return obj;
	}

	public void Add(double x, double[] yValues)
	{
		base.List.Add(new SeriesEntity(x, yValues));
	}

	public void Add(double x, double[] yValues, string category)
	{
		base.List.Add(new SeriesEntity(x, yValues, category));
	}

	public void Add(double x, double[] yValues, bool isEmpty)
	{
		base.List.Add(new SeriesEntity(x, yValues, isEmpty));
	}

	public void Add(double x, double[] yValues, bool isEmpty, string category)
	{
		base.List.Add(new SeriesEntity(x, yValues, isEmpty, category));
	}

	public void Insert(int xIndex, double x, double[] yValues)
	{
		base.List.Insert(xIndex, new SeriesEntity(x, yValues));
	}

	public void Insert(int xIndex, double x, double[] yValues, string category)
	{
		base.List.Insert(xIndex, new SeriesEntity(x, yValues, category));
	}

	public void SetX(int xIndex, double value)
	{
		(base.List[xIndex] as SeriesEntity).m_x = value;
		RaiseChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, xIndex));
	}

	internal void SetCategoryX(int xIndex, double value)
	{
		(base.List[xIndex] as SeriesEntity).m_x = value;
	}

	public void SetY(int xIndex, double[] yValues)
	{
		(base.List[xIndex] as SeriesEntity).m_yValues = yValues;
		RaiseChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, xIndex));
	}

	public void SetEmpty(int xIndex, bool isEmpty)
	{
		(base.List[xIndex] as SeriesEntity).m_isEmpty = isEmpty;
		RaiseChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, xIndex));
	}

	public void SetCategory(int xIndex, string category)
	{
		(base.List[xIndex] as SeriesEntity).m_category = category;
		RaiseChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, xIndex));
	}

	public void Remove(int xIndex)
	{
		base.List.RemoveAt(xIndex);
	}

	internal bool ContainsAnyEmptyPoint()
	{
		return false;
	}

	protected override void OnClear()
	{
	}

	protected override void OnClearComplete()
	{
		RaiseChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
	}

	protected override void OnInsert(int index, object value)
	{
	}

	protected override void OnInsertComplete(int index, object value)
	{
		RaiseChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
	}

	protected override void OnRemove(int index, object value)
	{
	}

	protected override void OnRemoveComplete(int index, object value)
	{
		RaiseChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
	}

	protected override void OnSet(int index, object value, object newValue)
	{
	}

	protected override void OnSetComplete(int index, object newValue, object oldValue)
	{
		RaiseChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index, index));
	}

	protected void RaiseChanged(ListChangedEventArgs args)
	{
		if (this.Changed != null)
		{
			this.Changed(this, args);
		}
	}

	public void Add(string category)
	{
		base.List.Add(new SeriesEntity(category));
	}

	internal void SetIsEmpty(int index, bool isSummary)
	{
		if (index < base.List.Count && base.List[index] != null)
		{
			(base.List[index] as SeriesEntity).IsEmpty = isSummary;
		}
	}
}
