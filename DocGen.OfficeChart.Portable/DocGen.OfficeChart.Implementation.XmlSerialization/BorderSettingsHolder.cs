using System;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Implementation.XmlSerialization;

internal class BorderSettingsHolder : IBorder, IParentApplication, ICloneable
{
	private ChartColor m_color = new ChartColor(OfficeKnownColors.Black);

	private OfficeLineStyle m_lineStyle;

	private bool m_bShowDiagonalLine;

	private bool m_bIsEmptyBorder = true;

	public OfficeKnownColors Color
	{
		get
		{
			if (m_color.ColorType != ColorType.Indexed)
			{
				return OfficeKnownColors.Black;
			}
			return (OfficeKnownColors)m_color.Value;
		}
		set
		{
			m_color.SetIndexed(value);
		}
	}

	public ChartColor ColorObject => m_color;

	public Color ColorRGB
	{
		get
		{
			if (m_color.ColorType != ColorType.RGB)
			{
				return ColorExtension.Empty;
			}
			return ColorExtension.FromArgb(m_color.Value);
		}
		set
		{
			m_color.SetRGB(value);
		}
	}

	public OfficeLineStyle LineStyle
	{
		get
		{
			return m_lineStyle;
		}
		set
		{
			m_lineStyle = value;
		}
	}

	public bool ShowDiagonalLine
	{
		get
		{
			return m_bShowDiagonalLine;
		}
		set
		{
			m_bShowDiagonalLine = value;
		}
	}

	internal bool IsEmptyBorder
	{
		get
		{
			return m_bIsEmptyBorder;
		}
		set
		{
			m_bIsEmptyBorder = value;
		}
	}

	public IApplication Application
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public object Parent
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
