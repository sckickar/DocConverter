using System;
using System.Collections.Generic;
using System.IO;
using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartDataTableImpl : CommonObject, IOfficeChartDataTable
{
	private ChartDatRecord m_chartDat;

	private List<BiffRecordRaw> m_arrRecords = new List<BiffRecordRaw>();

	private ChartTextAreaImpl m_text;

	internal bool HasShapeProperties;

	internal MemoryStream shapeStream;

	public bool HasHorzBorder
	{
		get
		{
			return m_chartDat.HasHorizontalBorders;
		}
		set
		{
			m_chartDat.HasHorizontalBorders = value;
		}
	}

	public bool HasVertBorder
	{
		get
		{
			return m_chartDat.HasVerticalBorders;
		}
		set
		{
			m_chartDat.HasVerticalBorders = value;
		}
	}

	public bool HasBorders
	{
		get
		{
			return m_chartDat.HasBorders;
		}
		set
		{
			m_chartDat.HasBorders = value;
		}
	}

	public bool ShowSeriesKeys
	{
		get
		{
			return m_chartDat.ShowSeriesKeys;
		}
		set
		{
			m_chartDat.ShowSeriesKeys = value;
		}
	}

	public IOfficeChartTextArea TextArea
	{
		get
		{
			if (m_text == null)
			{
				m_text = new ChartTextAreaImpl(base.Application, this);
				m_text.FontName = "Calibri";
			}
			else
			{
				ChartParserCommon.CheckDefaultSettings(m_text);
			}
			return m_text;
		}
	}

	public ChartDataTableImpl(IApplication application, object parent)
		: base(application, parent)
	{
		m_chartDat = (ChartDatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartDat);
		m_chartDat.HasBorders = true;
		m_chartDat.HasHorizontalBorders = true;
		m_chartDat.HasVerticalBorders = true;
	}

	[CLSCompliant(false)]
	public ChartDataTableImpl(IApplication application, object parent, IList<BiffRecordRaw> data, ref int iPos)
		: base(application, parent)
	{
		Parse(data, ref iPos);
	}

	private void Parse(IList<BiffRecordRaw> data, ref int iPos)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		BiffRecordRaw biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.ChartDat);
		m_chartDat = (ChartDatRecord)biffRecordRaw;
		iPos++;
		biffRecordRaw = data[iPos];
		biffRecordRaw.CheckTypeCode(TBIFFRecord.Begin);
		m_arrRecords.Add(biffRecordRaw);
		iPos++;
		int num = 1;
		while (num != 0)
		{
			biffRecordRaw = data[iPos];
			if (biffRecordRaw.TypeCode == TBIFFRecord.End)
			{
				num--;
			}
			else if (biffRecordRaw.TypeCode == TBIFFRecord.Begin)
			{
				num++;
			}
			m_arrRecords.Add(biffRecordRaw);
			iPos++;
		}
	}

	[CLSCompliant(false)]
	public void Serialize(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		records.Add(m_chartDat);
		if (m_arrRecords.Count == 0)
		{
			m_arrRecords.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
			ChartLegendRecord chartLegendRecord = (ChartLegendRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartLegend);
			chartLegendRecord.IsVerticalLegend = true;
			chartLegendRecord.ContainsDataTable = true;
			chartLegendRecord.Position = OfficeLegendPosition.NotDocked;
			chartLegendRecord.Spacing = ExcelLegendSpacing.Medium;
			m_arrRecords.Add(chartLegendRecord);
			m_arrRecords.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
			ChartPosRecord chartPosRecord = (ChartPosRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPos);
			chartPosRecord.TopLeft = 3;
			m_arrRecords.Add(chartPosRecord);
			ChartTextRecord chartTextRecord = (ChartTextRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartText);
			chartTextRecord.Options2 = 10816;
			m_arrRecords.Add(chartTextRecord);
			m_arrRecords.Add(BiffRecordFactory.GetRecord(TBIFFRecord.Begin));
			chartPosRecord = (ChartPosRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartPos);
			chartPosRecord.TopLeft = 2;
			chartPosRecord.BottomRight = 2;
			m_arrRecords.Add(chartPosRecord);
			m_arrRecords.Add(BiffRecordFactory.GetRecord(TBIFFRecord.ChartFontx));
			m_arrRecords.Add(BiffRecordFactory.GetRecord(TBIFFRecord.ChartAI));
			m_arrRecords.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
			m_arrRecords.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
			m_arrRecords.Add(BiffRecordFactory.GetRecord(TBIFFRecord.End));
		}
		records.AddList(m_arrRecords);
	}

	public ChartDataTableImpl Clone(object parent)
	{
		ChartDataTableImpl chartDataTableImpl = new ChartDataTableImpl(base.Application, parent);
		chartDataTableImpl.m_bIsDisposed = m_bIsDisposed;
		if (m_arrRecords != null)
		{
			List<BiffRecordRaw> list = new List<BiffRecordRaw>();
			int i = 0;
			for (int count = m_arrRecords.Count; i < count; i++)
			{
				BiffRecordRaw item = (BiffRecordRaw)m_arrRecords[i].Clone();
				list.Add(item);
			}
			chartDataTableImpl.m_arrRecords = list;
		}
		if (m_chartDat != null)
		{
			chartDataTableImpl.m_chartDat = (ChartDatRecord)m_chartDat.Clone();
		}
		if (m_text != null)
		{
			chartDataTableImpl.m_text = (ChartTextAreaImpl)m_text.Clone(chartDataTableImpl);
		}
		if (shapeStream != null)
		{
			shapeStream.Position = 0L;
			chartDataTableImpl.shapeStream = (MemoryStream)CloneUtils.CloneStream(shapeStream);
			chartDataTableImpl.HasShapeProperties = HasShapeProperties;
		}
		return chartDataTableImpl;
	}
}
