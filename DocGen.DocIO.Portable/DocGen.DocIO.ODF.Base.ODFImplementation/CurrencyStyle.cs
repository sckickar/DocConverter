namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class CurrencyStyle : DataStyle
{
	private CurrencySymbol m_currencySymbol;

	private NumberType m_number;

	private bool m_automaticOrder;

	internal CurrencySymbol CurrencySymbol
	{
		get
		{
			if (m_currencySymbol == null)
			{
				m_currencySymbol = new CurrencySymbol();
			}
			return m_currencySymbol;
		}
		set
		{
			m_currencySymbol = value;
		}
	}

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

	internal bool AutomaticOrder
	{
		get
		{
			return m_automaticOrder;
		}
		set
		{
			m_automaticOrder = value;
		}
	}

	public override bool Equals(object obj)
	{
		if (!(obj is CurrencyStyle currencyStyle))
		{
			return false;
		}
		bool flag = false;
		flag = m_currencySymbol.Equals(currencyStyle.CurrencySymbol);
		if (!flag)
		{
			return flag;
		}
		return m_number.Equals(currencyStyle.Number);
	}

	internal void Dispose()
	{
		if (m_number != null)
		{
			m_number.Dispose();
		}
		if (m_currencySymbol != null)
		{
			m_currencySymbol = null;
		}
	}
}
