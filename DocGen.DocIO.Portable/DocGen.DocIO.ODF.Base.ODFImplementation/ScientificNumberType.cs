namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class ScientificNumberType : CommonType
{
	private int m_minExponenDigits;

	internal int MinExponentDigits
	{
		get
		{
			return m_minExponenDigits;
		}
		set
		{
			m_minExponenDigits = value;
		}
	}
}
