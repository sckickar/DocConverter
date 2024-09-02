using System;
using System.Collections;
using DocGen.OfficeChart.Implementation.Shapes;

namespace DocGen.OfficeChart.Implementation.Collections;

internal class WorksheetChartsCollection : CollectionBaseEx<object>, IOfficeChartShapes, IEnumerable, IParentApplication
{
	private WorksheetBaseImpl m_sheet;

	public new IOfficeChartShape this[int index]
	{
		get
		{
			if (index < 0 || index >= base.InnerList.Count)
			{
				throw new ArgumentOutOfRangeException("Chart index");
			}
			return base.InnerList[index] as IOfficeChartShape;
		}
	}

	public WorksheetChartsCollection(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
	}

	public IOfficeChart AddChart()
	{
		IOfficeChart officeChart = m_sheet.Shapes.AddChart();
		base.InnerList.Add(officeChart);
		return officeChart;
	}

	protected internal IOfficeChartShape InnerAddChart(IOfficeChartShape chart)
	{
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		base.InnerList.Add(chart);
		return chart;
	}

	protected internal IOfficeChartShape AddChart(IOfficeChartShape chart)
	{
		if (chart == null)
		{
			throw new ArgumentNullException("chart");
		}
		InnerAddChart(chart);
		m_sheet.InnerShapes.AddShape((ShapeImpl)chart);
		return chart;
	}

	private void SetParents()
	{
		m_sheet = FindParent(typeof(WorksheetBaseImpl), bCheckSubclasses: true) as WorksheetBaseImpl;
		if (m_sheet == null)
		{
			throw new ArgumentNullException("Can't find parent worksheet.");
		}
	}

	public IOfficeChartShape Add()
	{
		return m_sheet.Shapes.AddChart();
	}

	public new void RemoveAt(int index)
	{
		if (index < 0 || index >= base.Count)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		IOfficeChartShape obj = base.InnerList[index] as IOfficeChartShape;
		base.InnerList.RemoveAt(index);
		obj.Remove();
	}
}
