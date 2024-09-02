using System.Collections.Generic;

namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class CommonStyles
{
	private List<NumberStyle> m_numbeStyles;

	private List<CurrencyStyle> m_currencyStyles;

	private List<PercentageStyle> m_percentageStyles;

	private List<DateStyle> m_dateStyles;

	private List<TimeStyle> m_timeStyles;

	private List<BooleanStyle> m_booleanStyles;

	private List<TextStyle> m_textStyles;

	private List<DefaultStyle> m_defaultStyles;

	private ODFStyleCollection m_odfStyles;

	internal List<NumberStyle> NumbeStyles
	{
		get
		{
			if (m_numbeStyles == null)
			{
				m_numbeStyles = new List<NumberStyle>();
			}
			return m_numbeStyles;
		}
		set
		{
			m_numbeStyles = value;
		}
	}

	internal List<CurrencyStyle> CurrencyStyles
	{
		get
		{
			if (m_currencyStyles == null)
			{
				m_currencyStyles = new List<CurrencyStyle>();
			}
			return m_currencyStyles;
		}
		set
		{
			m_currencyStyles = value;
		}
	}

	internal List<PercentageStyle> PercentageStyles
	{
		get
		{
			if (m_percentageStyles == null)
			{
				m_percentageStyles = new List<PercentageStyle>();
			}
			return m_percentageStyles;
		}
		set
		{
			m_percentageStyles = value;
		}
	}

	internal List<DateStyle> DateStyles
	{
		get
		{
			if (m_dateStyles == null)
			{
				m_dateStyles = new List<DateStyle>();
			}
			return m_dateStyles;
		}
		set
		{
			m_dateStyles = value;
		}
	}

	internal List<TimeStyle> TimeStyles
	{
		get
		{
			if (m_timeStyles == null)
			{
				m_timeStyles = new List<TimeStyle>();
			}
			return m_timeStyles;
		}
		set
		{
			m_timeStyles = value;
		}
	}

	internal List<BooleanStyle> BooleanStyles
	{
		get
		{
			if (m_booleanStyles == null)
			{
				m_booleanStyles = new List<BooleanStyle>();
			}
			return m_booleanStyles;
		}
		set
		{
			m_booleanStyles = value;
		}
	}

	internal List<TextStyle> TextStyles
	{
		get
		{
			if (m_textStyles == null)
			{
				m_textStyles = new List<TextStyle>();
			}
			return m_textStyles;
		}
		set
		{
			m_textStyles = value;
		}
	}

	internal List<DefaultStyle> DefaultStyles
	{
		get
		{
			if (m_defaultStyles == null)
			{
				m_defaultStyles = new List<DefaultStyle>();
			}
			return m_defaultStyles;
		}
		set
		{
			m_defaultStyles = value;
		}
	}

	internal ODFStyleCollection OdfStyles
	{
		get
		{
			if (m_odfStyles == null)
			{
				m_odfStyles = new ODFStyleCollection();
			}
			return m_odfStyles;
		}
		set
		{
			m_odfStyles = value;
		}
	}
}
