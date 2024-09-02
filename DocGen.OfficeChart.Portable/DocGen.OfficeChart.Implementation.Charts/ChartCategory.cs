using System;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartCategory : CommonObject, IOfficeChartCategory, IParentApplication
{
	private ChartSeriesCollection m_series;

	private bool m_isfiltered;

	private IRange m_categoryLabel;

	private IRange m_Value;

	private WorkbookImpl m_book;

	private ChartImpl m_chart;

	private ChartSeriesCollection series;

	private ChartCategoryCollection m_categoryColl;

	public bool Filter_customize;

	private string m_categoryName;

	public bool IsFiltered
	{
		get
		{
			return m_isfiltered;
		}
		set
		{
			m_isfiltered = value;
			if (!m_book.IsWorkbookOpening)
			{
				Filter_customize = true;
			}
		}
	}

	public IRange CategoryLabelIRange
	{
		get
		{
			return m_categoryLabel;
		}
		set
		{
			m_categoryLabel = value;
		}
	}

	public IOfficeDataRange CategoryLabel
	{
		get
		{
			return new ChartDataRange(m_chart)
			{
				Range = CategoryLabelIRange
			};
		}
		set
		{
			int firstRow = value.FirstRow;
			int firstColumn = value.FirstColumn;
			int lastRow = value.LastRow;
			int lastColumn = value.LastColumn;
			CategoryLabelIRange = m_chart.Workbook.Worksheets[0][firstRow, firstColumn, lastRow, lastColumn];
		}
	}

	public IRange ValuesIRange
	{
		get
		{
			return m_Value;
		}
		set
		{
			m_Value = value;
		}
	}

	public IOfficeDataRange Values
	{
		get
		{
			return new ChartDataRange(m_chart)
			{
				Range = ValuesIRange
			};
		}
		set
		{
			int firstRow = value.FirstRow;
			int firstColumn = value.FirstColumn;
			int lastRow = value.LastRow;
			int lastColumn = value.LastColumn;
			ValuesIRange = m_chart.Workbook.Worksheets[0][firstRow, firstColumn, lastRow, lastColumn];
		}
	}

	public string Name
	{
		get
		{
			return m_categoryName;
		}
		set
		{
			m_categoryName = value;
		}
	}

	public ChartCategory(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
		InitializeCollections();
	}

	private void InitializeCollections()
	{
	}

	private void SetParents()
	{
		object obj = FindParent(typeof(WorkbookImpl));
		if (obj == null)
		{
			throw new ArgumentNullException("Can't find parent workbook.");
		}
		m_book = (WorkbookImpl)obj;
		obj = FindParent(typeof(ChartImpl));
		if (obj == null)
		{
			throw new ArgumentNullException("Can't find parent chart.");
		}
		m_chart = (ChartImpl)obj;
		obj = FindParent(typeof(ChartCategoryCollection));
		if (obj == null)
		{
			throw new ArgumentNullException("Can't find parent series collection.");
		}
		m_categoryColl = (ChartCategoryCollection)obj;
	}
}
