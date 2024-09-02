using DocGen.OfficeChart.Implementation.XmlSerialization.Charts;

namespace DocGen.OfficeChart.Implementation.Charts;

internal class ChartExDataCache
{
	private string m_categoryFormula;

	private string m_seriesFormula;

	private object[] m_seriesValues;

	private object[] m_categoryValues;

	private bool m_isRowWiseCategory;

	private bool m_isRowWiseSeries;

	private string m_seriesFormatCode;

	private string m_categoryFormatCode;

	internal string CategoryFormula
	{
		get
		{
			return m_categoryFormula;
		}
		set
		{
			m_categoryFormula = value;
		}
	}

	internal string SeriesFormula
	{
		get
		{
			return m_seriesFormula;
		}
		set
		{
			m_seriesFormula = value;
		}
	}

	internal object[] SeriesValues
	{
		get
		{
			return m_seriesValues;
		}
		set
		{
			m_seriesValues = value;
		}
	}

	internal object[] CategoryValues
	{
		get
		{
			return m_categoryValues;
		}
		set
		{
			m_categoryValues = value;
		}
	}

	internal bool IsRowWiseCategory
	{
		get
		{
			return m_isRowWiseCategory;
		}
		set
		{
			m_isRowWiseCategory = value;
		}
	}

	internal bool IsRowWiseSeries
	{
		get
		{
			return m_isRowWiseSeries;
		}
		set
		{
			m_isRowWiseSeries = value;
		}
	}

	internal string SeriesFormatCode
	{
		get
		{
			return m_seriesFormatCode;
		}
		set
		{
			m_seriesFormatCode = value;
		}
	}

	internal string CategoriesFormatCode
	{
		get
		{
			return m_categoryFormatCode;
		}
		set
		{
			m_categoryFormatCode = value;
		}
	}

	internal void CopyProperties(ChartSerieImpl serie, WorkbookImpl workbook)
	{
		if (m_categoryFormatCode != null)
		{
			serie.CategoriesFormatCode = m_categoryFormatCode;
		}
		if (m_seriesFormatCode != null)
		{
			serie.FormatCode = m_seriesFormatCode;
		}
		if (m_categoryValues != null)
		{
			serie.EnteredDirectlyCategoryLabels = m_categoryValues;
		}
		if (m_seriesValues != null)
		{
			serie.EnteredDirectlyValues = m_seriesValues;
		}
		if (m_categoryFormula != null && m_categoryFormula != "")
		{
			IRange range = ChartParser.GetRange(workbook, m_categoryFormula);
			if (range is IName { RefersToRange: not null } name)
			{
				serie.CategoryLabels = (IOfficeDataRange)name.RefersToRange;
			}
			else
			{
				serie.CategoryLabelsIRange = range;
			}
		}
		if (m_seriesFormula != null && m_seriesFormula != null)
		{
			IRange range2 = ChartParser.GetRange(workbook, m_seriesFormula);
			if (range2 is IName { RefersToRange: not null } name2)
			{
				serie.ValuesIRange = name2.RefersToRange;
			}
			else
			{
				serie.ValuesIRange = range2;
			}
		}
		serie.IsRowWiseCategory = m_isRowWiseCategory;
		serie.IsRowWiseSeries = m_isRowWiseSeries;
	}
}
