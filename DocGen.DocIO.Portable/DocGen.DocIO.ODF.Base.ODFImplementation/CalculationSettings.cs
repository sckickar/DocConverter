namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class CalculationSettings
{
	private bool m_useRegularExpressions;

	internal bool UseRegularExpressions
	{
		get
		{
			return m_useRegularExpressions;
		}
		set
		{
			m_useRegularExpressions = value;
		}
	}
}
