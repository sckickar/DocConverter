using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartSeriesAxisImpl : ChartAxisImpl, IOfficeChartSeriesAxis, IOfficeChartAxis, IScalable
{
	private const int DEF_MAX_SPACING_VALUE = 31999;

	private ChartCatserRangeRecord m_chartCatserRange;

	public int LabelFrequency
	{
		get
		{
			return TickLabelSpacing;
		}
		set
		{
			TickLabelSpacing = value;
		}
	}

	public int TickLabelSpacing
	{
		get
		{
			return m_chartCatserRange.LabelsFrequency;
		}
		set
		{
			if (value < 0 || value > 31999)
			{
				throw new ArgumentOutOfRangeException("Value must be great then 0 and less then 31999.");
			}
			m_chartCatserRange.LabelsFrequency = (ushort)value;
		}
	}

	public int TickMarksFrequency
	{
		get
		{
			return TickMarkSpacing;
		}
		set
		{
			TickMarkSpacing = value;
		}
	}

	public int TickMarkSpacing
	{
		get
		{
			return m_chartCatserRange.TickMarksFrequency;
		}
		set
		{
			if (value < 0 || value > 31999)
			{
				throw new ArgumentOutOfRangeException("Value must be great then 0 and less then 31999.");
			}
			m_chartCatserRange.TickMarksFrequency = (ushort)value;
		}
	}

	public override bool ReversePlotOrder
	{
		get
		{
			return m_chartCatserRange.IsReverse;
		}
		set
		{
			m_chartCatserRange.IsReverse = value;
		}
	}

	[CLSCompliant(false)]
	protected override ExcelObjectTextLink TextLinkType => ExcelObjectTextLink.ZAxis;

	public int CrossesAt
	{
		get
		{
			return m_chartCatserRange.CrossingPoint;
		}
		set
		{
			m_chartCatserRange.CrossingPoint = (ushort)value;
		}
	}

	public bool IsBetween
	{
		get
		{
			return m_chartCatserRange.IsBetween;
		}
		set
		{
			m_chartCatserRange.IsBetween = value;
		}
	}

	public bool IsLogScale
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public double LogBase
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public double MaximumValue
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public double MinimumValue
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public ChartSeriesAxisImpl(IApplication application, object parent)
		: base(application, parent)
	{
		base.AxisId = 63149376;
	}

	public ChartSeriesAxisImpl(IApplication application, object parent, OfficeAxisType axisType)
		: this(application, parent, axisType, bIsPrimary: true)
	{
	}

	public ChartSeriesAxisImpl(IApplication application, object parent, OfficeAxisType axisType, bool bIsPrimary)
		: base(application, parent, axisType, bIsPrimary)
	{
		base.AxisId = 63149376;
	}

	[CLSCompliant(false)]
	public ChartSeriesAxisImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: this(application, parent, data, ref iPos, isPrimary: true)
	{
	}

	[CLSCompliant(false)]
	public ChartSeriesAxisImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos, bool isPrimary)
		: base(application, parent, data, ref iPos, isPrimary)
	{
		base.AxisId = 63149376;
	}

	private void ParseMaxCross(BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		record.CheckTypeCode(TBIFFRecord.ChartCatserRange);
		m_chartCatserRange = (ChartCatserRangeRecord)record;
	}

	protected override void ParseWallsOrFloor(IList<BiffRecordRaw> data, ref int iPos)
	{
		throw new NotSupportedException("Current axis type doesn't support walls or floors");
	}

	[CLSCompliant(false)]
	protected override void ParseData(BiffRecordRaw record, IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (record.TypeCode == TBIFFRecord.ChartCatserRange)
		{
			ParseMaxCross(record);
		}
	}

	[CLSCompliant(false)]
	public override void Serialize(OffsetArrayList records)
	{
		ChartAxisRecord chartAxisRecord = (ChartAxisRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxis);
		chartAxisRecord.AxisType = ChartAxisRecord.ChartAxisType.SeriesAxis;
		records.Add(chartAxisRecord);
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		records.Add((BiffRecordRaw)m_chartCatserRange.Clone());
		SerializeTickRecord(records);
		SerializeNumberFormat(records);
		SerializeFont(records);
		SerializeAxisBorder(records);
		SerializeGridLines(records);
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
	}

	protected override void InitializeVariables()
	{
		m_chartCatserRange = (ChartCatserRangeRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartCatserRange);
		base.InitializeVariables();
	}

	public override ChartAxisImpl Clone(object parent, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		ChartSeriesAxisImpl chartSeriesAxisImpl = (ChartSeriesAxisImpl)base.Clone(parent, dicFontIndexes, dicNewSheetNames);
		if (m_chartCatserRange != null)
		{
			chartSeriesAxisImpl.m_chartCatserRange = (ChartCatserRangeRecord)m_chartCatserRange.Clone();
		}
		return chartSeriesAxisImpl;
	}
}
