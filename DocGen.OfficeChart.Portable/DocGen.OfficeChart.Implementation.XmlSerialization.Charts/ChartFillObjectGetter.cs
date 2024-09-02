using System;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Interfaces;

namespace DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

internal class ChartFillObjectGetter : IChartFillObjectGetter
{
	private ChartSerieDataFormatImpl m_parentFormat;

	public ChartBorderImpl Border
	{
		get
		{
			m_parentFormat.HasLineProperties = true;
			return m_parentFormat.LineProperties as ChartBorderImpl;
		}
	}

	public ChartInteriorImpl Interior
	{
		get
		{
			m_parentFormat.HasInterior = true;
			return m_parentFormat.Interior as ChartInteriorImpl;
		}
	}

	public IInternalFill Fill
	{
		get
		{
			m_parentFormat.HasInterior = true;
			return m_parentFormat.Fill as IInternalFill;
		}
	}

	public ShadowImpl Shadow => m_parentFormat.Shadow as ShadowImpl;

	public ThreeDFormatImpl ThreeD => m_parentFormat.ThreeD as ThreeDFormatImpl;

	public ChartFillObjectGetter(ChartSerieDataFormatImpl dataFormat)
	{
		if (dataFormat == null)
		{
			throw new ArgumentNullException("dataFormat");
		}
		m_parentFormat = dataFormat;
	}
}
