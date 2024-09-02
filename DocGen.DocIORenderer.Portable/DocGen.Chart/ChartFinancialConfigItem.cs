using System.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Chart;

internal class ChartFinancialConfigItem : ChartConfigItem
{
	private ChartFinancialColorMode m_colorsMode;

	private Color m_upColor = Color.Green;

	private Color m_downColor = Color.Red;

	private byte m_darkLightPower = 100;

	[DefaultValue(ChartFinancialColorMode.Fixed)]
	public ChartFinancialColorMode ColorsMode
	{
		get
		{
			return m_colorsMode;
		}
		set
		{
			if (m_colorsMode != value)
			{
				m_colorsMode = value;
				RaisePropertyChanged("ColorsMode");
			}
		}
	}

	[DefaultValue(typeof(Color), "Green")]
	public Color PriceUpColor
	{
		get
		{
			return m_upColor;
		}
		set
		{
			if (m_upColor != value)
			{
				m_upColor = value;
				RaisePropertyChanged("PriceUpColor");
			}
		}
	}

	[DefaultValue(typeof(Color), "Red")]
	public Color PriceDownColor
	{
		get
		{
			return m_downColor;
		}
		set
		{
			if (m_downColor != value)
			{
				m_downColor = value;
				RaisePropertyChanged("PriceDownColor");
			}
		}
	}

	[DefaultValue(100)]
	public byte DarkLightPower
	{
		get
		{
			return m_darkLightPower;
		}
		set
		{
			if (m_darkLightPower != value)
			{
				m_darkLightPower = value;
				RaisePropertyChanged("DarkLightPower");
			}
		}
	}
}
