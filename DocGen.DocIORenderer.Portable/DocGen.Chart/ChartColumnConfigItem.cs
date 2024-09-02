using System;
using System.ComponentModel;
using DocGen.Drawing;

namespace DocGen.Chart;

internal class ChartColumnConfigItem : ChartConfigItem
{
	private ChartColumnShadingMode shadingMode = ChartColumnShadingMode.PhongCylinder;

	private Color lightColor = Color.White;

	private double lightAngle = -Math.PI / 4.0;

	private double phongAlpha = 20.0;

	private ChartColumnType m_columnType;

	private SizeF m_cornerRadius = SizeF.Empty;

	[DefaultValue(ChartColumnType.Box)]
	public ChartColumnType ColumnType
	{
		get
		{
			return m_columnType;
		}
		set
		{
			if (m_columnType != value)
			{
				m_columnType = value;
				RaisePropertyChanged("ColumnType");
			}
		}
	}

	[DefaultValue(ChartColumnShadingMode.PhongCylinder)]
	public ChartColumnShadingMode ShadingMode
	{
		get
		{
			return shadingMode;
		}
		set
		{
			if (shadingMode != value)
			{
				shadingMode = value;
				RaisePropertyChanged("ShadingMode");
			}
		}
	}

	[DefaultValue(typeof(Color), "White")]
	public Color LightColor
	{
		get
		{
			return lightColor;
		}
		set
		{
			if (lightColor != value)
			{
				lightColor = value;
				RaisePropertyChanged("LightColor");
			}
		}
	}

	[DefaultValue(-Math.PI / 4.0)]
	public double LightAngle
	{
		get
		{
			return lightAngle;
		}
		set
		{
			if (lightAngle != value)
			{
				lightAngle = value;
				RaisePropertyChanged("LightAngle");
			}
		}
	}

	[DefaultValue(20.0)]
	public double PhongAlpha
	{
		get
		{
			return phongAlpha;
		}
		set
		{
			if (phongAlpha != value)
			{
				phongAlpha = value;
				RaisePropertyChanged("PhongAlpha");
			}
		}
	}

	public SizeF CornerRadius
	{
		get
		{
			return m_cornerRadius;
		}
		set
		{
			m_cornerRadius = value;
			RaisePropertyChanged("CornerRadius");
		}
	}
}
