namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class PageLayout : DefaultPageLayout, INamedObject
{
	private string m_name;

	private PageUsage m_pageUsage;

	private int m_columnsCount;

	private float m_columnsGap;

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal PageUsage PageUsage
	{
		get
		{
			return m_pageUsage;
		}
		set
		{
			m_pageUsage = value;
		}
	}

	internal int ColumnsCount
	{
		get
		{
			return m_columnsCount;
		}
		set
		{
			m_columnsCount = value;
		}
	}

	internal float ColumnsGap
	{
		get
		{
			return m_columnsGap;
		}
		set
		{
			m_columnsGap = value;
		}
	}
}
