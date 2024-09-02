using System;
using System.ComponentModel;
using DocGen.Chart.Drawing;
using DocGen.Drawing;
using DocGen.Drawing.SkiaSharpHelper;

namespace DocGen.Chart;

internal sealed class ChartSeriesLegendItem : ChartLegendItem
{
	private const int c_emptyIndex = -1;

	private int m_pointIndex = -1;

	private ChartSeries m_series;

	private bool m_drawSeriesIcon = true;

	public ChartSeries Series => m_series;

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

	public ChartSeriesLegendItem(ChartSeries series)
		: this(series, -1)
	{
		m_text = series.Text;
	}

	public ChartSeriesLegendItem(ChartSeries series, int index)
	{
		m_series = series;
		m_pointIndex = index;
		base.Symbol = (series.LegendItemUseSeriesStyle ? ((ChartSymbolInfo)CopySymbol(series.Style.Symbol, base.Symbol)) : base.Symbol);
	}

	public void Refresh(bool useSeriesStyle)
	{
		m_isChecked = m_series.Visible;
		if (!useSeriesStyle || m_style.IsStyleChanged)
		{
			return;
		}
		if (m_pointIndex != -1)
		{
			ChartStyleInfo offlineStyle = m_series.GetOfflineStyle(m_pointIndex);
			m_style.TextColor = offlineStyle.TextColor;
			base.Symbol = offlineStyle.Symbol;
			m_style.Border = offlineStyle.Border;
			m_style.Interior = offlineStyle.Interior;
			m_style.ImageList = offlineStyle.Images;
			m_style.ImageIndex = offlineStyle.ImageIndex;
			m_text = offlineStyle.Text;
			if (Series.Type == ChartSeriesType.Pie)
			{
				if (offlineStyle.TextFormat != string.Empty)
				{
					m_text = string.Format(offlineStyle.TextFormat, Series.Points[m_pointIndex].Category);
				}
				else
				{
					m_text = Series.Points[m_pointIndex].Category;
				}
			}
		}
		else
		{
			ChartStyleInfo offlineStyle2 = m_series.GetOfflineStyle();
			m_style.TextColor = offlineStyle2.TextColor;
			base.Symbol = offlineStyle2.Symbol;
			m_style.Border = offlineStyle2.Border;
			m_style.Interior = offlineStyle2.Interior;
			m_style.ImageList = offlineStyle2.Images;
			m_style.ImageIndex = offlineStyle2.ImageIndex;
			m_text = m_series.Name;
		}
		RaiseChanged(this, EventArgs.Empty);
	}

	private object CopySymbol(object src, object target)
	{
		foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(src))
		{
			property.SetValue(target, property.GetValue(src));
		}
		return target;
	}

	protected override void DrawIcon(Graphics g, RectangleF bounds, ChartLegendItemType shape)
	{
		if (m_drawSeriesIcon)
		{
			if (m_pointIndex == -1)
			{
				m_series.Renderer.DrawIcon(g, Rectangle.Round(bounds), m_isDrawingShadow, base.ShadowColor);
			}
			else
			{
				m_series.Renderer.DrawIcon(m_pointIndex, g, Rectangle.Round(bounds), m_isDrawingShadow, base.ShadowColor);
			}
		}
		else
		{
			base.DrawIcon(g, bounds, shape);
		}
	}

	protected override void OnCheckedChanged(EventArgs args)
	{
		m_series.Visible = m_isChecked;
		if (m_series.Trendlines.Count > 0)
		{
			for (int i = 0; i < m_series.Trendlines.Count; i++)
			{
				m_series.Trendlines[i].Visible = m_isChecked;
			}
		}
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
