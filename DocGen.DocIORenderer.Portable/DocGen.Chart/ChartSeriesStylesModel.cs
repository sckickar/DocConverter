using System.Collections;
using DocGen.Styles;

namespace DocGen.Chart;

internal class ChartSeriesStylesModel : IChartSeriesStylesModel
{
	private const int c_seriesIndex = -1;

	private ChartStyleInfo m_seriesStyle;

	private IChartSeriesStylesHost m_host;

	private ChartSeriesComposedStylesModel m_volatileData;

	private Hashtable m_styles = new Hashtable();

	public ChartStyleInfo Style => m_seriesStyle;

	public IChartSeriesComposedStylesModel ComposedStyles => m_volatileData;

	public event ChartStyleChangedEventHandler Changed;

	public ChartSeriesStylesModel(IChartSeriesStylesHost host)
	{
		m_host = host;
		m_seriesStyle = new ChartStyleInfo();
		m_volatileData = new ChartSeriesComposedStylesModel(this);
	}

	public ChartStyleInfo GetStyleAt(int index)
	{
		if (m_styles.ContainsKey(index))
		{
			return m_styles[index] as ChartStyleInfo;
		}
		return null;
	}

	public void ChangeStyleAt(ChartStyleInfo style, int index)
	{
		ChartStyleInfo styleAt = GetStyleAt(index);
		if (styleAt == null)
		{
			m_styles[index] = new ChartStyleInfo(style);
		}
		else
		{
			styleAt.ModifyStyle(style, StyleModifyType.Changes);
		}
		RaiseStyleChanged(index);
	}

	public void ChangeStyle(ChartStyleInfo style)
	{
		ChartStyleInfo seriesStyle = m_seriesStyle;
		if (seriesStyle == null)
		{
			m_seriesStyle = new ChartStyleInfo(style);
		}
		else
		{
			seriesStyle.ModifyStyle(style, StyleModifyType.Changes);
		}
	}

	public ChartStyleInfo[] GetBaseStyles(IStyleInfo styleInfo, int index)
	{
		ChartStyleInfo[] array = null;
		ChartStyleInfo styleInfo2 = styleInfo as ChartStyleInfo;
		if (index == -1)
		{
			return m_host.GetStylesMap().GetSubBaseStyles(styleInfo2, m_seriesStyle);
		}
		if (m_styles[index] == null)
		{
			return m_host.GetStylesMap().GetSubBaseStyles(styleInfo2, m_seriesStyle);
		}
		ChartStyleInfo chartStyleInfo = m_styles[index] as ChartStyleInfo;
		return m_host.GetStylesMap().GetSubBaseStyles(styleInfo2, new ChartStyleInfo[2] { chartStyleInfo, m_seriesStyle });
	}

	protected virtual void RaiseStyleChanged(int index)
	{
		if (this.Changed != null)
		{
			this.Changed(this, new ChartStyleChangedEventArgs(ChartStyleChangedEventArgs.Type.Changed, index));
		}
	}
}
