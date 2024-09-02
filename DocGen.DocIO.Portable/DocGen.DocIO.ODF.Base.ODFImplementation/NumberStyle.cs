using System;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class NumberStyle : DataStyle
{
	private NumberType m_number;

	private FractionType m_fraction;

	private ScientificNumberType m_scientificNumber;

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

	internal FractionType Fraction
	{
		get
		{
			if (m_fraction == null)
			{
				m_fraction = new FractionType();
			}
			return m_fraction;
		}
		set
		{
			m_fraction = value;
		}
	}

	internal ScientificNumberType ScientificNumber
	{
		get
		{
			if (m_scientificNumber == null)
			{
				m_scientificNumber = new ScientificNumberType();
			}
			return m_scientificNumber;
		}
		set
		{
			m_scientificNumber = value;
		}
	}

	internal bool HasKey(int propertyKey, int flagname)
	{
		return (flagname & (ushort)Math.Pow(2.0, propertyKey)) >> propertyKey != 0;
	}

	internal void Dispose()
	{
		if (m_number != null)
		{
			m_number.Dispose();
		}
		if (m_fraction != null)
		{
			m_fraction = null;
		}
		if (m_scientificNumber != null)
		{
			m_scientificNumber = null;
		}
		Dispose1();
	}
}
