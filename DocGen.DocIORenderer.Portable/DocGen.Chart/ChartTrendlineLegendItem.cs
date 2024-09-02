using System;
using System.ComponentModel;
using DocGen.Chart.Drawing;

namespace DocGen.Chart;

internal sealed class ChartTrendlineLegendItem : ChartLegendItem
{
	private const int c_emptyIndex = -1;

	private ChartSeries m_series;

	private Trendline m_trendline;

	private bool m_drawSeriesIcon = true;

	public Trendline Trendline => m_trendline;

	public bool DrawSeriesIcon
	{
		get
		{
			return m_drawSeriesIcon;
		}
		set
		{
			m_drawSeriesIcon = value;
		}
	}

	public ChartTrendlineLegendItem(ChartSeries series, Trendline trend)
		: this(series, trend, -1)
	{
		m_text = Trendline.Name;
		m_series = series;
	}

	public ChartTrendlineLegendItem(ChartSeries series, Trendline trendline, int index)
	{
		m_trendline = trendline;
		base.Symbol = (series.LegendItemUseSeriesStyle ? ((ChartSymbolInfo)CopySymbol(series.Style.Symbol, base.Symbol)) : base.Symbol);
		base.Symbol.Color = trendline.Color;
	}

	public void Refresh(bool useSeriesStyle)
	{
		m_isChecked = m_trendline.Visible;
		if (useSeriesStyle && !m_style.IsStyleChanged)
		{
			ChartStyleInfo offlineStyle = m_series.GetOfflineStyle();
			m_style.TextColor = offlineStyle.TextColor;
			base.Symbol = offlineStyle.Symbol;
			m_style.Border = offlineStyle.Border;
			m_style.Interior = offlineStyle.Interior;
			m_style.ImageList = offlineStyle.Images;
			m_style.ImageIndex = offlineStyle.ImageIndex;
			m_text = m_series.Text;
			RaiseChanged(this, EventArgs.Empty);
		}
	}

	private object CopySymbol(object src, object target)
	{
		foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(src))
		{
			property.SetValue(target, property.GetValue(src));
		}
		return target;
	}

	protected override void OnCheckedChanged(EventArgs args)
	{
		m_trendline.Visible = m_isChecked;
		base.OnCheckedChanged(args);
	}

	protected override BrushInfo GetBrushInfo()
	{
		BrushInfo brushInfo = m_style.Interior;
		if (m_isDrawingShadow)
		{
			brushInfo = new BrushInfo(m_style.ShadowColor);
		}
		else
		{
			ChartColumnConfigItem columnItem = m_series.ConfigItems.ColumnItem;
			if (m_series.ChartModel.ColorModel.AllowGradient && columnItem.ShadingMode == ChartColumnShadingMode.PhongCylinder && brushInfo.Style == BrushStyle.Solid)
			{
				ChartSeriesRenderer.PhongShadingColors(brushInfo.BackColor, brushInfo.BackColor, columnItem.LightColor, columnItem.LightAngle, columnItem.PhongAlpha, out var colors, out var _);
				brushInfo = new BrushInfo(GradientStyle.Horizontal, colors);
			}
		}
		return brushInfo;
	}
}
