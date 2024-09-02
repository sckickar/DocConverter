using System;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartFormatCollection : CollectionBaseEx<ChartFormatImpl>
{
	private const int DEF_ARRAY_VALUE = -1;

	public const int DEF_ARRAY_CAPACITY = 8;

	private static readonly TBIFFRecord[] DEF_NEED_SECONDARY_AXIS = new TBIFFRecord[4]
	{
		TBIFFRecord.ChartPie,
		TBIFFRecord.ChartRadar,
		TBIFFRecord.ChartRadarArea,
		TBIFFRecord.ChartBoppop
	};

	private int[] m_arrOrder;

	private ChartParentAxisImpl m_parentAxis;

	private bool m_isParetoFormat;

	public new ChartFormatImpl this[int index]
	{
		get
		{
			if (m_arrOrder[index] == -1)
			{
				throw new ArgumentException("Index out of bounds.");
			}
			return base.List[m_arrOrder[index]];
		}
	}

	public bool IsPrimary => m_parentAxis.IsPrimary;

	public bool NeedSecondaryAxis
	{
		get
		{
			if (!IsPrimary || base.Count < 1)
			{
				return false;
			}
			ChartFormatImpl chartFormatImpl = base.List[0];
			TBIFFRecord formatRecordType = chartFormatImpl.FormatRecordType;
			if (!chartFormatImpl.Is3D)
			{
				if (Array.IndexOf(DEF_NEED_SECONDARY_AXIS, formatRecordType) == -1)
				{
					if (formatRecordType == TBIFFRecord.ChartBar)
					{
						return chartFormatImpl.IsHorizontalBar;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	internal bool IsParetoFormat
	{
		get
		{
			return m_isParetoFormat;
		}
		set
		{
			m_isParetoFormat = value;
		}
	}

	internal bool IsBarChartAxes
	{
		get
		{
			if (base.Count < 1)
			{
				return false;
			}
			for (int i = 0; i < base.List.Count; i++)
			{
				ChartFormatImpl chartFormatImpl = base.List[i];
				if (chartFormatImpl.FormatRecordType == TBIFFRecord.ChartBar && chartFormatImpl.IsHorizontalBar)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal bool IsPercentStackedAxis
	{
		get
		{
			if (base.Count < 1)
			{
				return false;
			}
			for (int i = 0; i < base.List.Count; i++)
			{
				ChartFormatImpl chartFormatImpl = base.List[i];
				if ((chartFormatImpl.FormatRecordType == TBIFFRecord.ChartArea && chartFormatImpl.IsCategoryBrokenDown) || (chartFormatImpl.FormatRecordType == TBIFFRecord.ChartBar && chartFormatImpl.ShowAsPercentsBar) || (chartFormatImpl.FormatRecordType == TBIFFRecord.ChartLine && chartFormatImpl.ShowAsPercentsLine))
				{
					return true;
				}
			}
			return false;
		}
	}

	public ChartFormatCollection(IApplication application, object parent)
		: base(application, parent)
	{
		m_arrOrder = new int[8];
		for (int i = 0; i < 8; i++)
		{
			m_arrOrder[i] = -1;
		}
		SetParents();
	}

	public void SetParents()
	{
		m_parentAxis = (ChartParentAxisImpl)FindParent(typeof(ChartParentAxisImpl));
		if (m_parentAxis == null)
		{
			throw new ApplicationException("Can't find parent axis.");
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		for (int i = 0; i < base.Count; i++)
		{
			base.List[i].Serialize(records);
		}
	}

	public new ChartFormatImpl Add(ChartFormatImpl formatToAdd)
	{
		return Add(formatToAdd, bCanReplace: false);
	}

	public ChartFormatImpl Add(ChartFormatImpl formatToAdd, bool bCanReplace)
	{
		if (formatToAdd == null)
		{
			throw new ArgumentNullException("formatToAdd");
		}
		int drawingZOrder = formatToAdd.DrawingZOrder;
		int num = m_arrOrder[drawingZOrder];
		if (num < 0 || !bCanReplace)
		{
			base.Add(formatToAdd);
			num = base.Count - 1;
			formatToAdd = m_parentAxis.Formats.AddFormat(formatToAdd, drawingZOrder, num, IsPrimary);
		}
		else
		{
			base[num] = formatToAdd;
		}
		return formatToAdd;
	}

	public ChartFormatImpl FindOrAdd(ChartFormatImpl formatToAdd)
	{
		ChartFormatImpl chartFormatImpl = null;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			ChartFormatImpl chartFormatImpl2 = base.InnerList[i];
			if (formatToAdd == chartFormatImpl2)
			{
				chartFormatImpl = chartFormatImpl2;
				break;
			}
		}
		if (chartFormatImpl == null)
		{
			chartFormatImpl = Add(formatToAdd, bCanReplace: false);
		}
		return chartFormatImpl;
	}

	public bool ContainsIndex(int index)
	{
		if (index < 8 && index >= 0)
		{
			return m_arrOrder[index] != -1;
		}
		return false;
	}

	public new void Remove(ChartFormatImpl toRemove)
	{
		if (toRemove == null)
		{
			throw new ArgumentNullException("toRemove");
		}
		int drawingZOrder = toRemove.DrawingZOrder;
		if (((ChartSeriesCollection)m_parentAxis.m_parentChart.Series).GetCountOfSeriesWithSameDrawingOrder(drawingZOrder) != 0)
		{
			throw new ArgumentException("Can't remove format.");
		}
		int num = m_arrOrder[drawingZOrder];
		RemoveAt(num);
		m_arrOrder[drawingZOrder] = -1;
		m_parentAxis.Formats.RemoveFormat(num, drawingZOrder, IsPrimary);
	}

	public void UpdateIndexesAfterRemove(int removeIndex)
	{
		for (int i = 0; i < 8; i++)
		{
			if (m_arrOrder[i] > removeIndex)
			{
				m_arrOrder[i]--;
			}
		}
	}

	public void UpdateSeriesByChartGroup(int newIndex, int OldIndex)
	{
		ChartSeriesCollection chartSeriesCollection = (ChartSeriesCollection)m_parentAxis.m_parentChart.Series;
		int i = 0;
		for (int count = chartSeriesCollection.Count; i < count; i++)
		{
			ChartSerieImpl chartSerieImpl = (ChartSerieImpl)chartSeriesCollection[i];
			if (chartSerieImpl.ChartGroup == OldIndex)
			{
				chartSerieImpl.ChartGroup = newIndex;
			}
		}
	}

	public new void Clear()
	{
		base.Clear();
		for (int i = 0; i < 8; i++)
		{
			m_arrOrder[i] = -1;
		}
	}

	public override object Clone(object parent)
	{
		ChartFormatCollection obj = (ChartFormatCollection)base.Clone(parent);
		obj.m_arrOrder = CloneUtils.CloneIntArray(m_arrOrder);
		obj.m_isParetoFormat = m_isParetoFormat;
		obj.SetParents();
		return obj;
	}

	public void SetIndex(int index, int Value)
	{
		if (index >= 8 || index < 0 || Value < 0 || Value >= base.List.Count)
		{
			throw new ArgumentException("Index is out of bounds");
		}
		m_arrOrder[index] = Value;
	}

	public void UpdateFormatsOnAdding(int index)
	{
		this[index].DrawingZOrder = index + 1;
		UpdateSeriesByChartGroup(index + 1, index);
		m_arrOrder[index + 1] = m_arrOrder[index];
		m_arrOrder[index] = -1;
	}

	public void UpdateFormatsOnRemoving(int index)
	{
		this[index].DrawingZOrder = index - 1;
		UpdateSeriesByChartGroup(index - 1, index);
		m_arrOrder[index - 1] = m_arrOrder[index];
		m_arrOrder[index] = -1;
	}

	public ChartFormatImpl GetFormat(int iOrder, bool bDelete)
	{
		int num = m_arrOrder[iOrder];
		if (num == -1)
		{
			throw new ArgumentException("Can't find format by current index.");
		}
		ChartFormatImpl result = base.List[num];
		if (bDelete)
		{
			m_arrOrder[iOrder] = -1;
			RemoveAt(num);
			int i = 0;
			for (int num2 = m_arrOrder.Length; i < num2; i++)
			{
				int num3 = m_arrOrder[i];
				if (num3 > num)
				{
					num3 = (m_arrOrder[i] = num3 - 1);
				}
			}
		}
		return result;
	}

	public void AddFormat(ChartFormatImpl format)
	{
		if (format == null)
		{
			throw new ArgumentNullException("format");
		}
		int drawingZOrder = format.DrawingZOrder;
		base.Add(format);
		int num = base.Count - 1;
		m_arrOrder[drawingZOrder] = num;
	}
}
