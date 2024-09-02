namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class CurrencySymbol : LanguageStyle
{
	private string m_data;

	internal string Data
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	public override bool Equals(object obj)
	{
		CurrencySymbol currencySymbol = obj as CurrencySymbol;
		return m_data.Equals(currencySymbol.Data);
	}
}
