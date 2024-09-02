using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartValueAxisImpl : ChartAxisImpl, IOfficeChartValueAxis, IOfficeChartAxis, IScalable
{
	public static readonly double[] DEF_DISPLAY_UNIT_VALUES = new double[10] { 0.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, 10000000.0, 100000000.0, 1000000000.0, 1000000000000.0 };

	private bool m_bHasDisplayUnitLabel;

	private ChartValueRangeRecord m_chartValueRange;

	private double m_logBase = 10.0;

	private double m_displayUnitCustom = 1.0;

	private OfficeChartDisplayUnit m_displayUnit;

	private ChartWrappedTextAreaImpl m_displayUnitLabel;

	private bool m_bAutoTickLabelSpacing = true;

	private bool m_isChangeAutoCross;

	private bool m_isChangeAutoCrossInLoading;

	internal bool m_isDisplayUnitPercentage;

	public double MinimumValue
	{
		get
		{
			CheckValueRangeRecord();
			return m_chartValueRange.NumMin;
		}
		set
		{
			if (!IsAutoMax && value >= MaximumValue)
			{
				throw new ArgumentOutOfRangeException("MinimumValue");
			}
			CheckValueRangeRecord();
			m_chartValueRange.NumMin = value;
			IsAutoMin = false;
		}
	}

	public double MaximumValue
	{
		get
		{
			CheckValueRangeRecord();
			return m_chartValueRange.NumMax;
		}
		set
		{
			if (!IsAutoMin && value <= MinimumValue)
			{
				throw new ArgumentOutOfRangeException("MaximumValue");
			}
			CheckValueRangeRecord();
			m_chartValueRange.NumMax = value;
			IsAutoMax = false;
		}
	}

	public virtual double MajorUnit
	{
		get
		{
			CheckValueRangeRecord();
			return m_chartValueRange.NumMajor;
		}
		set
		{
			SetMajorUnit(value);
		}
	}

	public virtual double MinorUnit
	{
		get
		{
			CheckValueRangeRecord();
			return m_chartValueRange.NumMinor;
		}
		set
		{
			SetMinorUnit(value);
		}
	}

	internal double CrossValue
	{
		get
		{
			return CrossesAt;
		}
		set
		{
			CrossesAt = value;
		}
	}

	public virtual double CrossesAt
	{
		get
		{
			return m_chartValueRange.NumCross;
		}
		set
		{
			m_chartValueRange.NumCross = value;
			IsAutoCross = false;
		}
	}

	public virtual bool IsAutoMin
	{
		get
		{
			if (!CheckValueRangeRecord(throwException: false))
			{
				return true;
			}
			return m_chartValueRange.IsAutoMin;
		}
		set
		{
			SetAutoMin(check: true, value);
		}
	}

	public virtual bool IsAutoMax
	{
		get
		{
			if (!CheckValueRangeRecord(throwException: false))
			{
				return true;
			}
			return m_chartValueRange.IsAutoMax;
		}
		set
		{
			SetAutoMax(check: true, value);
		}
	}

	public new bool AutoTickLabelSpacing
	{
		get
		{
			if (IsAutoMajor && IsAutoMax && IsAutoMin)
			{
				return IsAutoMinor;
			}
			return false;
		}
		set
		{
			m_bAutoTickLabelSpacing = value;
		}
	}

	public virtual bool IsAutoMajor
	{
		get
		{
			return m_chartValueRange.IsAutoMajor;
		}
		set
		{
			m_chartValueRange.IsAutoMajor = value;
		}
	}

	public virtual bool IsAutoMinor
	{
		get
		{
			return m_chartValueRange.IsAutoMinor;
		}
		set
		{
			m_chartValueRange.IsAutoMinor = value;
		}
	}

	public virtual bool IsAutoCross
	{
		get
		{
			if (!CheckValueRangeRecord(throwException: false))
			{
				return true;
			}
			return m_chartValueRange.IsAutoCross;
		}
		set
		{
			CheckValueRangeRecord();
			m_chartValueRange.IsAutoCross = value;
			if (value)
			{
				IsMaxCross = false;
			}
			if (base.ParentChart != null && base.ParentChart.ParentWorkbook != null && !base.ParentWorkbook.IsWorkbookOpening && value)
			{
				m_isChangeAutoCross = true;
				IsChangeAutoCrossInLoading = false;
			}
		}
	}

	internal bool IsChangeAutoCross
	{
		get
		{
			return m_isChangeAutoCross;
		}
		set
		{
			m_isChangeAutoCross = value;
		}
	}

	internal bool IsChangeAutoCrossInLoading
	{
		get
		{
			return m_isChangeAutoCrossInLoading;
		}
		set
		{
			m_isChangeAutoCrossInLoading = value;
		}
	}

	public bool IsLogScale
	{
		get
		{
			if (!CheckValueRangeRecord(throwException: false))
			{
				return false;
			}
			return m_chartValueRange.IsLogScale;
		}
		set
		{
			CheckValueRangeRecord();
			m_chartValueRange.IsLogScale = value;
			if (value)
			{
				if (!m_chartValueRange.IsAutoMin && m_chartValueRange.NumMin < 1.0)
				{
					m_chartValueRange.NumMin = 1.0;
				}
				if (!m_chartValueRange.IsAutoMax && m_chartValueRange.NumMax < 1.0)
				{
					m_chartValueRange.NumMax = 1.0;
				}
			}
		}
	}

	public double LogBase
	{
		get
		{
			return m_logBase;
		}
		set
		{
			m_logBase = value;
		}
	}

	public override bool ReversePlotOrder
	{
		get
		{
			return m_chartValueRange.IsReverse;
		}
		set
		{
			m_chartValueRange.IsReverse = value;
		}
	}

	public virtual bool IsMaxCross
	{
		get
		{
			return m_chartValueRange.IsMaxCross;
		}
		set
		{
			m_chartValueRange.IsMaxCross = value;
			if (value)
			{
				IsAutoCross = false;
			}
		}
	}

	[CLSCompliant(false)]
	protected ChartValueRangeRecord ChartValueRange
	{
		get
		{
			return m_chartValueRange;
		}
		set
		{
			m_chartValueRange = value;
		}
	}

	public double DisplayUnitCustom
	{
		get
		{
			CheckValueRangeRecord();
			return m_displayUnitCustom;
		}
		set
		{
			CheckValueRangeRecord();
			if (value <= 0.0)
			{
				throw new ArgumentOutOfRangeException("The value must be large than zero.");
			}
			m_displayUnitCustom = value;
			DisplayUnit = OfficeChartDisplayUnit.Custom;
		}
	}

	public OfficeChartDisplayUnit DisplayUnit
	{
		get
		{
			CheckValueRangeRecord();
			return m_displayUnit;
		}
		set
		{
			CheckValueRangeRecord();
			m_displayUnit = value;
			if (value == OfficeChartDisplayUnit.None)
			{
				m_bHasDisplayUnitLabel = false;
				m_displayUnitCustom = 1.0;
				m_displayUnitLabel = null;
			}
			else
			{
				if ((int)value < DEF_DISPLAY_UNIT_VALUES.Length)
				{
					m_displayUnitCustom = DEF_DISPLAY_UNIT_VALUES[(int)value];
				}
				if (!base.ParentAxis.ParentChart.ParentWorkbook.IsWorkbookOpening)
				{
					HasDisplayUnitLabel = true;
				}
			}
			if (!base.ParentChart.Loading && m_isDisplayUnitPercentage)
			{
				m_isDisplayUnitPercentage = false;
			}
		}
	}

	public bool HasDisplayUnitLabel
	{
		get
		{
			CheckValueRangeRecord();
			return m_bHasDisplayUnitLabel;
		}
		set
		{
			CheckValueRangeRecord();
			if (!base.ParentWorkbook.IsWorkbookOpening && m_displayUnit == OfficeChartDisplayUnit.None)
			{
				throw new NotSupportedException("Doesnot support display unit label in DisplayUnit None mode.");
			}
			if (value && m_displayUnitLabel == null)
			{
				CreateDispalayUnitLabel();
			}
			m_bHasDisplayUnitLabel = value;
		}
	}

	public IOfficeChartTextArea DisplayUnitLabel
	{
		get
		{
			CheckValueRangeRecord();
			if (!HasDisplayUnitLabel)
			{
				return null;
			}
			return m_displayUnitLabel;
		}
	}

	[CLSCompliant(false)]
	protected override ExcelObjectTextLink TextLinkType => ExcelObjectTextLink.YAxis;

	public ChartValueAxisImpl(IApplication application, object parent)
		: base(application, parent)
	{
	}

	public ChartValueAxisImpl(IApplication application, object parent, OfficeAxisType axisType)
		: this(application, parent, axisType, bIsPrimary: true)
	{
	}

	public ChartValueAxisImpl(IApplication application, object parent, OfficeAxisType axisType, bool bIsPrimary)
		: base(application, parent, axisType, bIsPrimary)
	{
		base.AxisId = (base.IsPrimary ? 57253888 : 61870848);
	}

	[CLSCompliant(false)]
	public ChartValueAxisImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: this(application, parent, data, ref iPos, isPrimary: true)
	{
	}

	[CLSCompliant(false)]
	public ChartValueAxisImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos, bool isPrimary)
		: base(application, parent, data, ref iPos, isPrimary)
	{
		base.AxisId = (base.IsPrimary ? 57253888 : 61870848);
	}

	protected void SetAutoMin(bool check, bool value)
	{
		CheckValueRangeRecord(check);
		m_chartValueRange.IsAutoMin = value;
	}

	protected void SetAutoMax(bool check, bool value)
	{
		CheckValueRangeRecord(check);
		m_chartValueRange.IsAutoMax = value;
	}

	[CLSCompliant(false)]
	protected virtual void ParseMaxCross(BiffRecordRaw record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		record.CheckTypeCode(TBIFFRecord.ChartValueRange);
		m_chartValueRange = (ChartValueRangeRecord)record;
	}

	protected override void ParseWallsOrFloor(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		base.ParentChart.Floor = new ChartWallOrFloorImpl(base.Application, base.ParentChart, bWalls: false, data, ref iPos);
	}

	[CLSCompliant(false)]
	protected override void ParseData(BiffRecordRaw record, IList<BiffRecordRaw> data, ref int iPos)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		switch (record.TypeCode)
		{
		case TBIFFRecord.ChartValueRange:
			ParseMaxCross(record);
			break;
		case TBIFFRecord.ChartAxisDisplayUnits:
			ParseDisplayUnits((ChartAxisDisplayUnitsRecord)record);
			break;
		case TBIFFRecord.ChartBegDispUnit:
			ParseDisplayUnitLabel(data, ref iPos);
			break;
		}
	}

	private void ParseDisplayUnits(ChartAxisDisplayUnitsRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		m_displayUnit = record.DisplayUnit;
		m_displayUnitCustom = record.DisplayUnitValue;
		m_bHasDisplayUnitLabel = record.IsShowLabels;
	}

	private void ParseDisplayUnitLabel(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartBegDispUnit);
		iPos++;
		biffRecordRaw = ChartTextAreaImpl.UnwrapRecord(data[iPos]);
		while (biffRecordRaw.TypeCode != TBIFFRecord.ChartEndDispUnit)
		{
			if (biffRecordRaw.TypeCode == TBIFFRecord.ChartText)
			{
				m_displayUnitLabel = new ChartWrappedTextAreaImpl(base.Application, this, data, ref iPos);
				iPos--;
			}
			iPos++;
			biffRecordRaw = ChartTextAreaImpl.UnwrapRecord(data[iPos]);
		}
	}

	[CLSCompliant(false)]
	public override void Serialize(OffsetArrayList records)
	{
		Serialize(records, ChartAxisRecord.ChartAxisType.ValueAxis);
	}

	[CLSCompliant(false)]
	protected void Serialize(OffsetArrayList records, ChartAxisRecord.ChartAxisType axisType)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		ChartAxisRecord chartAxisRecord = (ChartAxisRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxis);
		chartAxisRecord.AxisType = axisType;
		records.Add(chartAxisRecord);
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		records.Add((BiffRecordRaw)m_chartValueRange.Clone());
		SerializeDisplayUnits(records);
		SerializeNumberFormat(records);
		SerializeTickRecord(records);
		SerializeFont(records);
		SerializeAxisBorder(records);
		if (base.IsPrimary)
		{
			SerializeGridLines(records);
			SerializeWallsOrFloor(records);
		}
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
	}

	[CLSCompliant(false)]
	protected virtual void SerializeWallsOrFloor(OffsetArrayList records)
	{
		base.ParentChart.SerializeFloor(records);
	}

	private void SerializeDisplayUnits(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_displayUnit != 0)
		{
			ChartAxisDisplayUnitsRecord chartAxisDisplayUnitsRecord = (ChartAxisDisplayUnitsRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAxisDisplayUnits);
			chartAxisDisplayUnitsRecord.IsShowLabels = m_bHasDisplayUnitLabel;
			chartAxisDisplayUnitsRecord.DisplayUnitValue = m_displayUnitCustom;
			chartAxisDisplayUnitsRecord.DisplayUnit = m_displayUnit;
			records.Add(chartAxisDisplayUnitsRecord);
			ChartBegDispUnitRecord chartBegDispUnitRecord = (ChartBegDispUnitRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartBegDispUnit);
			chartBegDispUnitRecord.IsShowLabels = m_bHasDisplayUnitLabel;
			records.Add(chartBegDispUnitRecord);
			if (m_bHasDisplayUnitLabel)
			{
				m_displayUnitLabel.Serialize(records);
			}
			ChartEndDispUnitRecord chartEndDispUnitRecord = (ChartEndDispUnitRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartEndDispUnit);
			chartEndDispUnitRecord.IsShowLabels = m_bHasDisplayUnitLabel;
			records.Add(chartEndDispUnitRecord);
		}
	}

	protected override void InitializeVariables()
	{
		m_chartValueRange = (ChartValueRangeRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartValueRange);
		base.InitializeVariables();
	}

	protected bool CheckValueRangeRecord()
	{
		return CheckValueRangeRecord(!base.ParentWorkbook.IsWorkbookOpening && !base.ParentWorkbook.Saving && !base.ParentWorkbook.IsCreated);
	}

	protected virtual bool CheckValueRangeRecord(bool throwException)
	{
		return true;
	}

	public override ChartAxisImpl Clone(object parent, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		ChartValueAxisImpl chartValueAxisImpl = (ChartValueAxisImpl)base.Clone(parent, dicFontIndexes, dicNewSheetNames);
		if (m_chartValueRange != null)
		{
			chartValueAxisImpl.m_chartValueRange = (ChartValueRangeRecord)m_chartValueRange.Clone();
		}
		if (m_displayUnitLabel != null)
		{
			chartValueAxisImpl.m_displayUnitLabel = (ChartWrappedTextAreaImpl)m_displayUnitLabel.Clone(chartValueAxisImpl, dicFontIndexes, dicNewSheetNames);
		}
		chartValueAxisImpl.m_displayUnit = m_displayUnit;
		return chartValueAxisImpl;
	}

	private void CreateDispalayUnitLabel()
	{
		m_displayUnitLabel = new ChartWrappedTextAreaImpl(base.Application, this, ExcelObjectTextLink.DisplayUnit);
		m_displayUnitLabel.TextRecord.IsAutoText = true;
		m_displayUnitLabel.Bold = true;
		m_displayUnitLabel.IsAutoMode = true;
		if (base.AxisType == OfficeAxisType.Value)
		{
			m_displayUnitLabel.TextRotationAngle = 270;
		}
	}

	public void SetMajorUnit(double value)
	{
		if (value <= 0.0 || (!IsAutoMinor && value < MinorUnit))
		{
			throw new ArgumentOutOfRangeException("MajorUnit");
		}
		m_chartValueRange.NumMajor = value;
		IsAutoMajor = false;
	}

	public void SetMinorUnit(double value)
	{
		if (value <= 0.0 || (!IsAutoMajor && value > m_chartValueRange.NumMajor))
		{
			throw new ArgumentOutOfRangeException("MinorUnit");
		}
		m_chartValueRange.NumMinor = value;
		IsAutoMinor = false;
	}
}
