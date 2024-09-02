using System;
using System.Collections;
using System.Collections.Generic;
using DocGen.OfficeChart.Implementation.Collections;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartCategoryCollection : CollectionBaseEx<IOfficeChartCategory>, IOfficeChartCategories, IParentApplication, ICollection<IOfficeChartCategory>, IEnumerable<IOfficeChartCategory>, IEnumerable, ICloneParent, IList<IOfficeChartCategory>
{
	private ChartImpl m_chart;

	public new ChartCategory this[int index]
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public IOfficeChartCategory this[string name]
	{
		get
		{
			int i = 0;
			for (int count = base.Count; i < count; i++)
			{
				if (m_chart.Categories[i].Name == name)
				{
					return m_chart.Categories[i];
				}
			}
			return null;
		}
	}

	public ChartCategoryCollection(IApplication application, object parent)
		: base(application, parent)
	{
		m_chart = (ChartImpl)FindParent(typeof(ChartImpl));
		if (m_chart == null)
		{
			throw new Exception("cannot find parent chart.");
		}
	}

	public IOfficeChartCategory Add(ChartSerieImpl serieToAdd)
	{
		throw new ArgumentException("This method is not supported");
	}

	public void Remove(string serieName)
	{
	}

	public IOfficeChartCategory Add(string name, OfficeChartType serieType)
	{
		return null;
	}

	public IOfficeChartCategory Add(OfficeChartType serieType)
	{
		throw new ArgumentException("This method is not supported");
	}

	public IOfficeChartCategory Add()
	{
		ChartCategory chartCategory = new ChartCategory(base.Application, this);
		base.Add(chartCategory);
		return chartCategory;
	}

	protected override void OnClear()
	{
		base.OnClear();
	}

	public override object Clone(object parent)
	{
		throw new ArgumentException("This method is not supported");
	}

	public List<BiffRecordRaw> GetEnteredRecords(int siIndex)
	{
		return null;
	}

	public IOfficeChartCategory Add(string name)
	{
		new ChartSerieImpl(base.Application, this).Name = name;
		if (m_chart.HasTitle && m_chart.ChartTitle == null)
		{
			m_chart.ChartTitle = name;
		}
		return null;
	}

	private List<List<BiffRecordRaw>> GetArrays(int siIndex)
	{
		return null;
	}

	public IOfficeChartCategory Add(IRange Categorylabel, IRange Values)
	{
		ChartCategory chartCategory = new ChartCategory(base.Application, this);
		chartCategory.CategoryLabel = (m_chart.ChartData as ChartData)[Categorylabel];
		chartCategory.Values = (m_chart.ChartData as ChartData)[Values];
		base.Add(chartCategory);
		return chartCategory;
	}
}
