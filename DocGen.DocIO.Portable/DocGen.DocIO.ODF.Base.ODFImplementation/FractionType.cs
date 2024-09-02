namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class FractionType : CommonType
{
	private int m_minDenominatorDigits;

	private int m_minNumeratorDigits;

	private int m_denominatorValue;

	internal int MinDenominatorDigits
	{
		get
		{
			return m_minDenominatorDigits;
		}
		set
		{
			m_minDenominatorDigits = value;
		}
	}

	internal int MinNumeratorDigits
	{
		get
		{
			return m_minNumeratorDigits;
		}
		set
		{
			m_minNumeratorDigits = value;
		}
	}

	internal int DenominatorValue
	{
		get
		{
			return m_denominatorValue;
		}
		set
		{
			m_denominatorValue = value;
		}
	}
}
