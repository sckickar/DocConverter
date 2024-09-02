namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class Seconds : TimeBase
{
	private int m_decimalPlaces;

	internal int DecimalPlaces
	{
		get
		{
			return m_decimalPlaces;
		}
		set
		{
			m_decimalPlaces = value;
		}
	}
}
