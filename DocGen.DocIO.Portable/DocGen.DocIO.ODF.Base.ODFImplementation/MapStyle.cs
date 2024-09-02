namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class MapStyle
{
	private string m_applyStyleName;

	private string m_condition;

	private string m_baseCellAddress;

	internal string ApplyStyleName
	{
		get
		{
			return m_applyStyleName;
		}
		set
		{
			m_applyStyleName = value;
		}
	}

	internal string Condition
	{
		get
		{
			return m_condition;
		}
		set
		{
			m_condition = value;
		}
	}

	internal string BaseCellAddress
	{
		get
		{
			return m_baseCellAddress;
		}
		set
		{
			m_baseCellAddress = value;
		}
	}
}
