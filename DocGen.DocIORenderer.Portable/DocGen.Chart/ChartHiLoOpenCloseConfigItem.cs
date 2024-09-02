using System.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Chart;

internal class ChartHiLoOpenCloseConfigItem : ChartConfigItem
{
	private ChartOpenCloseDrawMode m_drawMode;

	private Color m_openTipColor = Color.Empty;

	private Color m_closeTipColor = Color.Empty;

	[DefaultValue(ChartOpenCloseDrawMode.Both)]
	public ChartOpenCloseDrawMode DrawMode
	{
		get
		{
			return m_drawMode;
		}
		set
		{
			if (m_drawMode != value)
			{
				m_drawMode = value;
				RaisePropertyChanged("DrawMode");
			}
		}
	}

	public Color OpenTipColor
	{
		get
		{
			return m_openTipColor;
		}
		set
		{
			if (m_openTipColor != value)
			{
				m_openTipColor = value;
				RaisePropertyChanged("OpenTipColor");
			}
		}
	}

	public Color CloseTipColor
	{
		get
		{
			return m_closeTipColor;
		}
		set
		{
			if (m_closeTipColor != value)
			{
				m_closeTipColor = value;
				RaisePropertyChanged("CloseTipColor");
			}
		}
	}
}
