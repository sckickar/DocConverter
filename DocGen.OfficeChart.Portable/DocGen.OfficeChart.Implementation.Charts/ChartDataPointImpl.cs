using System;
using System.Collections.Generic;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartDataPointImpl : CommonObject, IOfficeChartDataPoint, IParentApplication
{
	private ChartDataLabelsImpl m_dataLabels;

	private int m_iIndex;

	private ChartSerieDataFormatImpl m_dataFormat;

	private ChartImpl m_parentChart;

	private bool m_bHasDataPoint;

	private bool m_defaultMarker;

	private bool m_bBubble3D;

	private int m_explosion;

	private bool m_bHasExplosion;

	private bool m_setAsTotal;

	public IOfficeChartDataLabels DataLabels
	{
		get
		{
			CreateDataLabels();
			return m_dataLabels;
		}
		internal set
		{
			m_dataLabels = value as ChartDataLabelsImpl;
		}
	}

	public IOfficeChartSerieDataFormat DataFormat
	{
		get
		{
			if (m_dataFormat == null && (base.Parent as ChartDataPointsCollection).Parent is ChartSerieImpl)
			{
				m_dataFormat = new ChartSerieDataFormatImpl(base.Application, this);
			}
			return m_dataFormat;
		}
	}

	public ChartSerieDataFormatImpl InnerDataFormat
	{
		get
		{
			return m_dataFormat;
		}
		set
		{
			m_dataFormat = value;
			if (value != null)
			{
				value.SetParent(this);
				value.SetParents();
			}
		}
	}

	public int Index
	{
		get
		{
			return m_iIndex;
		}
		set
		{
			m_iIndex = value;
		}
	}

	public ChartSerieDataFormatImpl DataFormatOrNull => m_dataFormat;

	public bool IsDefault => m_iIndex == 65535;

	public bool HasDataLabels => m_dataLabels != null;

	internal bool HasDataPoint
	{
		get
		{
			return m_bHasDataPoint;
		}
		set
		{
			m_bHasDataPoint = value;
		}
	}

	public bool IsDefaultmarkertype
	{
		get
		{
			return m_defaultMarker;
		}
		set
		{
			m_defaultMarker = value;
		}
	}

	internal bool Bubble3D
	{
		get
		{
			return m_bBubble3D;
		}
		set
		{
			m_bBubble3D = value;
		}
	}

	internal int Explosion
	{
		get
		{
			return m_explosion;
		}
		set
		{
			m_explosion = value;
			m_bHasExplosion = true;
		}
	}

	internal bool HasExplosion => m_bHasExplosion;

	public bool SetAsTotal
	{
		get
		{
			return m_setAsTotal;
		}
		set
		{
			m_setAsTotal = value;
		}
	}

	public ChartDataPointImpl(IApplication application, object parent, int index)
		: base(application, parent)
	{
		m_iIndex = index;
		if ((parent as ChartDataPointsCollection).Parent is ChartSerieImpl)
		{
			m_dataFormat = new ChartSerieDataFormatImpl(application, this);
			m_dataFormat.DataFormat.PointNumber = (ushort)m_iIndex;
		}
		else
		{
			m_dataFormat = null;
		}
		m_parentChart = (ChartImpl)FindParent(typeof(ChartImpl));
		if (m_parentChart == null)
		{
			throw new Exception("cannot find parent chart.");
		}
	}

	[CLSCompliant(false)]
	public void SerializeDataLabels(OffsetArrayList records)
	{
		if (m_dataLabels != null)
		{
			((ISerializable)m_dataLabels).Serialize((IList<IBiffStorage>)records);
		}
	}

	[CLSCompliant(false)]
	public void SerializeDataFormat(OffsetArrayList records)
	{
		if (records == null)
		{
			throw new ArgumentNullException("records");
		}
		if (m_dataFormat != null)
		{
			m_dataFormat.UpdateDataFormatInDataPoint();
			m_dataFormat.Serialize(records);
		}
	}

	public void SetDataLabels(ChartTextAreaImpl textArea)
	{
		if (textArea == null)
		{
			throw new ArgumentNullException("textArea");
		}
		CreateDataLabels();
		m_dataLabels.TextArea = textArea;
	}

	private void CreateDataLabels()
	{
		if (m_dataLabels != null)
		{
			return;
		}
		m_dataLabels = new ChartDataLabelsImpl(base.Application, this, Index);
		if (base.Parent is ChartDataPointsCollection chartDataPointsCollection)
		{
			if (!chartDataPointsCollection.IsLoading)
			{
				m_dataLabels.IsCreated = true;
			}
			if (chartDataPointsCollection.DefaultDataPoint is ChartDataPointImpl { HasDataLabels: not false } chartDataPointImpl && !IsDefault)
			{
				IOfficeChartDataLabels dataLabels = chartDataPointImpl.DataLabels;
				m_dataLabels.IsLegendKey = dataLabels.IsLegendKey;
				m_dataLabels.IsValue = dataLabels.IsValue;
				m_dataLabels.IsCategoryName = dataLabels.IsCategoryName;
				m_dataLabels.IsSeriesName = dataLabels.IsSeriesName;
				m_dataLabels.IsPercentage = dataLabels.IsPercentage;
				m_dataLabels.IsBubbleSize = dataLabels.IsBubbleSize;
			}
		}
	}

	public object Clone(object parent, Dictionary<int, int> dicFontIndexes, Dictionary<string, string> dicNewSheetNames)
	{
		ChartDataPointImpl chartDataPointImpl = new ChartDataPointImpl(base.Application, parent, m_iIndex);
		if (m_dataLabels != null)
		{
			chartDataPointImpl.m_dataLabels = (ChartDataLabelsImpl)m_dataLabels.Clone(chartDataPointImpl, dicFontIndexes, dicNewSheetNames);
		}
		if (m_dataFormat != null)
		{
			chartDataPointImpl.m_dataFormat = m_dataFormat.Clone(chartDataPointImpl);
		}
		chartDataPointImpl.m_bHasExplosion = m_bHasExplosion;
		chartDataPointImpl.m_explosion = m_explosion;
		chartDataPointImpl.m_defaultMarker = m_defaultMarker;
		chartDataPointImpl.m_setAsTotal = m_setAsTotal;
		chartDataPointImpl.m_bHasDataPoint = m_bHasDataPoint;
		return chartDataPointImpl;
	}

	public void UpdateSerieIndex()
	{
		if (m_dataLabels != null)
		{
			m_dataLabels.UpdateSerieIndex();
		}
		if (m_dataFormat != null)
		{
			m_dataFormat.UpdateSerieIndex();
		}
	}

	public void ChangeChartStockHigh_Low_CloseType()
	{
		DataFormat.MarkerStyle = OfficeChartMarkerType.DowJones;
		m_dataFormat.IsAutoMarker = false;
		m_dataFormat.MarkerForegroundColorIndex = (OfficeKnownColors)79;
		m_dataFormat.MarkerBackgroundColorIndex = (OfficeKnownColors)79;
		m_dataFormat.LineProperties.LinePattern = OfficeChartLinePattern.None;
		m_dataFormat.LineProperties.LineWeight = OfficeChartLineWeight.Hairline;
		m_dataFormat.LineProperties.ColorIndex = (OfficeKnownColors)77;
	}

	public void ChangeChartStockVolume_High_Low_CloseType()
	{
		DataFormat.MarkerStyle = OfficeChartMarkerType.DowJones;
		m_dataFormat.IsAutoMarker = false;
		m_dataFormat.MarkerForegroundColorIndex = (OfficeKnownColors)77;
		m_dataFormat.MarkerBackgroundColorIndex = (OfficeKnownColors)77;
		OfficeChartType destinationType = m_parentChart.DestinationType;
		m_parentChart.DestinationType = OfficeChartType.Line;
		m_dataFormat.LineProperties.LinePattern = OfficeChartLinePattern.None;
		m_parentChart.DestinationType = destinationType;
	}

	public void ChangeIntimateBuble(OfficeChartType typeToChange)
	{
		if (!(m_parentChart.Workbook as WorkbookImpl).IsWorkbookOpening)
		{
			DataFormat.LineProperties.LinePattern = OfficeChartLinePattern.Solid;
			DataFormat.Is3DBubbles = typeToChange != OfficeChartType.Bubble;
		}
	}

	public void CloneDataFormat(ChartSerieDataFormatImpl serieFormat)
	{
		if (serieFormat != null && (m_dataFormat == null || !m_dataFormat.IsFormatted))
		{
			ChartDataFormatRecord dataFormat = m_dataFormat.DataFormat;
			m_dataFormat = serieFormat.Clone(this);
			m_dataFormat.DataFormat = dataFormat;
		}
	}

	public void ClearDataFormats(ChartSerieDataFormatImpl format)
	{
		if (m_dataFormat != null && m_dataFormat.IsFormatted)
		{
			ChartDataFormatRecord dataFormat = m_dataFormat.DataFormat;
			m_dataFormat = format.Clone(this);
			m_dataFormat.DataFormat = dataFormat;
		}
	}
}
