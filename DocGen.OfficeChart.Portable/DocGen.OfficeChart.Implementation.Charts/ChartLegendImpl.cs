using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

[CLSCompliant(false)]
internal class ChartLegendImpl : CommonObject, IOfficeChartLegend
{
	private const int DEF_POSITION = 5;

	private ChartLegendRecord m_serieLegend;

	private ChartPosRecord m_pos;

	private ChartAttachedLabelLayoutRecord m_attachedLabelLayout;

	private ChartTextAreaImpl m_text;

	private ChartFrameFormatImpl m_frame;

	private bool m_includeInLayout = true;

	private ChartImpl m_parentChart;

	private ChartLegendEntriesColl m_collEntries;

	private IOfficeChartLayout m_layout;

	private ChartParagraphType m_paraType;

	private UnknownRecord m_legendTextPropsStream;

	private bool m_IsDefaultTextSettings = true;

	private bool m_IsChartTextArea;

	private ushort m_chartExPosition;

	public IOfficeChartFrameFormat FrameFormat
	{
		get
		{
			if (m_frame == null)
			{
				m_frame = new ChartFrameFormatImpl(base.Application, this, bAutoSize: true, bIsInteriorGrey: false, bSetDefaults: true);
				m_frame.Interior.UseAutomaticFormat = true;
				if (m_parentChart.Workbook.Version != 0)
				{
					m_frame.HasLineProperties = false;
				}
			}
			return m_frame;
		}
	}

	public IOfficeChartTextArea TextArea
	{
		get
		{
			if (m_text == null)
			{
				m_text = new ChartTextAreaImpl(base.Application, this);
			}
			if (!IsChartTextArea)
			{
				m_text.ParagraphType = ChartParagraphType.CustomDefault;
			}
			return m_text;
		}
	}

	public bool IncludeInLayout
	{
		get
		{
			return m_includeInLayout;
		}
		set
		{
			if (m_parentChart.Workbook.Version == OfficeVersion.Excel97to2003)
			{
				throw new ArgumentException("This property is not supported for the current workbook version");
			}
			if (value != m_includeInLayout)
			{
				m_includeInLayout = value;
			}
		}
	}

	public int X
	{
		get
		{
			return LegendRecord.X;
		}
		set
		{
			SetCustomPosition();
			LegendRecord.X = value;
			m_layout.Left = LegendRecord.X;
			PositionRecord.X1 = value;
		}
	}

	public int Y
	{
		get
		{
			return LegendRecord.Y;
		}
		set
		{
			SetCustomPosition();
			LegendRecord.Y = value;
			m_layout.Top = LegendRecord.Y;
			PositionRecord.Y1 = value;
		}
	}

	public OfficeLegendPosition Position
	{
		get
		{
			return LegendRecord.Position;
		}
		set
		{
			if (value == OfficeLegendPosition.NotDocked)
			{
				SetCustomPosition();
				return;
			}
			SetDefPosition();
			IsVerticalLegend = value != 0 && value != OfficeLegendPosition.Top;
			LegendRecord.Position = value;
		}
	}

	public bool IsVerticalLegend
	{
		get
		{
			return LegendRecord.IsVerticalLegend;
		}
		set
		{
			LegendRecord.IsVerticalLegend = value;
		}
	}

	public IChartLegendEntries LegendEntries => m_collEntries;

	internal bool IsDefaultTextSettings
	{
		get
		{
			return m_IsDefaultTextSettings;
		}
		set
		{
			m_IsDefaultTextSettings = value;
		}
	}

	internal bool IsChartTextArea
	{
		get
		{
			return m_IsChartTextArea;
		}
		set
		{
			m_IsChartTextArea = value;
		}
	}

	public int Width
	{
		get
		{
			return LegendRecord.Width;
		}
		set
		{
			LegendRecord.Width = value;
		}
	}

	public int Height
	{
		get
		{
			return LegendRecord.Height;
		}
		set
		{
			LegendRecord.Height = value;
		}
	}

	public bool ContainsDataTable
	{
		get
		{
			return LegendRecord.ContainsDataTable;
		}
		set
		{
			LegendRecord.ContainsDataTable = value;
		}
	}

	public ExcelLegendSpacing Spacing
	{
		get
		{
			return LegendRecord.Spacing;
		}
		set
		{
			LegendRecord.Spacing = value;
		}
	}

	public bool AutoPosition
	{
		get
		{
			return LegendRecord.AutoPosition;
		}
		set
		{
			LegendRecord.AutoPosition = value;
		}
	}

	public bool AutoSeries
	{
		get
		{
			return LegendRecord.AutoSeries;
		}
		set
		{
			LegendRecord.AutoSeries = value;
		}
	}

	public bool AutoPositionX
	{
		get
		{
			return LegendRecord.AutoPositionX;
		}
		set
		{
			LegendRecord.AutoPositionX = value;
		}
	}

	public bool AutoPositionY
	{
		get
		{
			return LegendRecord.AutoPositionY;
		}
		set
		{
			LegendRecord.AutoPositionY = value;
		}
	}

	public IOfficeChartLayout Layout
	{
		get
		{
			if (m_layout == null)
			{
				m_layout = new ChartLayoutImpl(base.Application, this, m_parentChart);
			}
			return m_layout;
		}
		set
		{
			m_layout = value;
		}
	}

	public ChartParagraphType ParagraphType
	{
		get
		{
			return m_paraType;
		}
		set
		{
			m_paraType = value;
		}
	}

	public ChartAttachedLabelLayoutRecord AttachedLabelLayout
	{
		get
		{
			if (m_attachedLabelLayout == null)
			{
				m_attachedLabelLayout = (ChartAttachedLabelLayoutRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartAttachedLabelLayout);
			}
			return m_attachedLabelLayout;
		}
	}

	internal ushort ChartExPosition
	{
		get
		{
			return m_chartExPosition;
		}
		set
		{
			m_chartExPosition = value;
		}
	}

	internal ChartLegendRecord LegendRecord
	{
		get
		{
			if (m_serieLegend == null)
			{
				m_serieLegend = (ChartLegendRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartLegend);
			}
			return m_serieLegend;
		}
	}

	internal ChartPosRecord PositionRecord
	{
		get
		{
			if (m_pos == null)
			{
				m_pos = (ChartPosRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPos);
			}
			return m_pos;
		}
	}

	public ChartLegendImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_serieLegend = (ChartLegendRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartLegend);
		m_pos = (ChartPosRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPos);
		m_pos.TopLeft = 5;
		m_text = new ChartTextAreaImpl(application, this);
		m_collEntries = new ChartLegendEntriesColl(application, parent);
		SetParents();
		m_paraType = ChartParagraphType.Default;
	}

	private void SetParents()
	{
		m_parentChart = (ChartImpl)FindParent(typeof(ChartImpl));
		if (m_parentChart == null)
		{
			throw new ApplicationException("Can't find parent object.");
		}
	}

	public void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartLegend);
		m_serieLegend = (ChartLegendRecord)data[iPos];
		iPos += 2;
		int num = 1;
		while (num != 0)
		{
			biffRecordRaw = data[iPos];
			switch (biffRecordRaw.TypeCode)
			{
			case TBIFFRecord.Begin:
				iPos = BiffRecordRaw.SkipBeginEndBlock(data, iPos);
				break;
			case TBIFFRecord.End:
				num--;
				break;
			case TBIFFRecord.ChartPos:
				m_pos = (ChartPosRecord)biffRecordRaw;
				break;
			case TBIFFRecord.ChartAttachedLabelLayout:
				if (m_attachedLabelLayout == null)
				{
					m_attachedLabelLayout = (Layout.ManualLayout as ChartManualLayoutImpl).AttachedLabelLayout;
				}
				m_attachedLabelLayout = (ChartAttachedLabelLayoutRecord)biffRecordRaw;
				break;
			case TBIFFRecord.ChartText:
				m_text = new ChartTextAreaImpl(base.Application, this);
				iPos = m_text.Parse(data, iPos) - 1;
				IsChartTextArea = true;
				m_text.ParagraphType = ChartParagraphType.CustomDefault;
				break;
			case TBIFFRecord.ChartFrame:
				m_frame = new ChartFrameFormatImpl(base.Application, this, bSetDefaults: false);
				m_frame.Parse(data, ref iPos);
				iPos--;
				break;
			case TBIFFRecord.ChartTextPropsStream:
				m_legendTextPropsStream = (UnknownRecord)data[iPos];
				break;
			}
			iPos++;
		}
	}

	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		records.Add((ChartLegendRecord)m_serieLegend.Clone());
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
		if (m_pos != null)
		{
			records.Add((ChartPosRecord)m_pos.Clone());
		}
		if (m_text != null)
		{
			m_text.Serialize(records, bIsLegendEntry: true);
		}
		if (m_frame != null)
		{
			m_frame.Serialize(records);
		}
		if (m_attachedLabelLayout != null)
		{
			SerializeRecord(records, m_attachedLabelLayout);
		}
		if (m_legendTextPropsStream != null)
		{
			records.Add(m_legendTextPropsStream);
		}
		records.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
	}

	[CLSCompliant(false)]
	protected virtual void SerializeRecord(IList<IBiffStorage> records, BiffRecordRaw record)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (record != null)
		{
			records.Add((BiffRecordRaw)record.Clone());
		}
	}

	public ChartLegendImpl Clone(object parent, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		ChartLegendImpl chartLegendImpl = (ChartLegendImpl)MemberwiseClone();
		chartLegendImpl.SetParent(parent);
		chartLegendImpl.SetParents();
		chartLegendImpl.m_serieLegend = (ChartLegendRecord)CloneUtils.CloneCloneable(m_serieLegend);
		chartLegendImpl.m_pos = (ChartPosRecord)CloneUtils.CloneCloneable(m_pos);
		chartLegendImpl.m_collEntries = m_collEntries.Clone(chartLegendImpl, dicFontIndexes, dicNewSheetNames);
		if (m_frame != null)
		{
			chartLegendImpl.m_frame = m_frame.Clone(chartLegendImpl);
		}
		if (m_text != null)
		{
			chartLegendImpl.m_text = (ChartTextAreaImpl)m_text.Clone(chartLegendImpl, dicFontIndexes, dicNewSheetNames);
		}
		if (m_layout != null)
		{
			chartLegendImpl.m_layout = m_layout;
		}
		chartLegendImpl.m_paraType = m_paraType;
		if (m_legendTextPropsStream != null)
		{
			chartLegendImpl.m_legendTextPropsStream = (UnknownRecord)m_legendTextPropsStream.Clone();
		}
		if (m_attachedLabelLayout != null)
		{
			chartLegendImpl.m_attachedLabelLayout = (ChartAttachedLabelLayoutRecord)m_attachedLabelLayout.Clone();
		}
		return chartLegendImpl;
	}

	public void Clear()
	{
		if (m_frame != null)
		{
			m_frame.Clear();
		}
		if (m_collEntries != null)
		{
			m_collEntries.Clear();
		}
		m_pos = (ChartPosRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPos);
		m_serieLegend = (ChartLegendRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartLegend);
	}

	public void Delete()
	{
		m_parentChart.HasLegend = false;
	}

	private void SetDefPosition()
	{
		AutoPosition = true;
		AutoPositionX = true;
		AutoPositionY = true;
		LegendRecord.X = 0;
		LegendRecord.Y = 0;
		LegendRecord.Width = 0;
		LegendRecord.Height = 0;
		PositionRecord.X1 = 0;
		PositionRecord.X2 = 0;
		PositionRecord.Y1 = 0;
		PositionRecord.Y2 = 0;
		m_parentChart.ChartProperties.IsAlwaysAutoPlotArea = false;
	}

	private void SetCustomPosition()
	{
		AutoPosition = false;
		AutoPositionX = false;
		AutoPositionY = false;
		LegendRecord.Position = OfficeLegendPosition.NotDocked;
		m_parentChart.ChartProperties.IsAlwaysAutoPlotArea = true;
	}
}
