using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartStyleInfoIdentity : StyleInfoIdentityBase
{
	private ChartSeriesComposedStylesModel m_data;

	private int m_index;

	private IStyleInfo[] m_baseStyles;

	private bool m_isOffLine;

	public ChartStyleInfoIdentity(ChartSeriesComposedStylesModel data, int index, bool offLine)
	{
		m_data = data;
		m_index = index;
		m_isOffLine = offLine;
	}

	public override IStyleInfo[] GetBaseStyles(IStyleInfo thisStyleInfo)
	{
		if (m_baseStyles == null)
		{
			IStyleInfo[] baseStyles = m_data.GetBaseStyles(thisStyleInfo, m_index);
			m_baseStyles = baseStyles;
		}
		return m_baseStyles;
	}

	public override void OnStyleChanged(StyleInfoBase style, StyleInfoProperty sip)
	{
		if (!m_isOffLine)
		{
			m_baseStyles = null;
			m_data.ChangeStyle(style as ChartStyleInfo, m_index);
		}
	}
}
