namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class CommonType
{
	private int m_decimalPlaces;

	private bool m_grouping;

	private int m_minIntegerDigits;

	internal byte nFormatFlags;

	internal const byte DecimalPlacesKey = 0;

	internal const byte GroupingKey = 1;

	internal const byte MinIntegerDigitsKey = 2;

	internal int DecimalPlaces
	{
		get
		{
			return m_decimalPlaces;
		}
		set
		{
			nFormatFlags = (byte)((nFormatFlags & 0xFEu) | 1u);
			m_decimalPlaces = value;
		}
	}

	internal bool Grouping
	{
		get
		{
			return m_grouping;
		}
		set
		{
			nFormatFlags = (byte)((nFormatFlags & 0xFDu) | 2u);
			m_grouping = value;
		}
	}

	internal int MinIntegerDigits
	{
		get
		{
			return m_minIntegerDigits;
		}
		set
		{
			nFormatFlags = (byte)((nFormatFlags & 0xFBu) | 4u);
			m_minIntegerDigits = value;
		}
	}
}
