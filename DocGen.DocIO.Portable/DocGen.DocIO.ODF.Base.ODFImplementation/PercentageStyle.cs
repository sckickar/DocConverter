namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class PercentageStyle : DataStyle
{
	private NumberType m_number;

	internal NumberType Number
	{
		get
		{
			if (m_number == null)
			{
				m_number = new NumberType();
			}
			return m_number;
		}
		set
		{
			m_number = value;
		}
	}

	internal PercentageStyle()
	{
	}

	internal void Dispose()
	{
		if (m_number != null)
		{
			m_number.Dispose();
		}
	}
}
