using System;
using DocGen.OfficeChart.Implementation.Charts;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

internal class ShadowData : ICloneable
{
	private ushort m_ShadowOuterPresets;

	private ushort m_ShadowInnerPresets;

	private ushort m_ShadowPrespectivePresets;

	private ushort m_BevelTop;

	private ushort m_BevelBottom;

	private ChartImpl m_chartObject;

	private ushort m_Material;

	private ushort m_Lighting;

	public Office2007ChartPresetsOuter ShadowOuterPresets
	{
		get
		{
			return (Office2007ChartPresetsOuter)m_ShadowOuterPresets;
		}
		set
		{
			m_ShadowOuterPresets = (ushort)value;
		}
	}

	public Office2007ChartPresetsInner ShadowInnerPresets
	{
		get
		{
			return (Office2007ChartPresetsInner)m_ShadowInnerPresets;
		}
		set
		{
			m_ShadowInnerPresets = (ushort)value;
		}
	}

	public Office2007ChartPresetsPerspective ShadowPrespectivePresets
	{
		get
		{
			return (Office2007ChartPresetsPerspective)m_ShadowPrespectivePresets;
		}
		set
		{
			m_ShadowPrespectivePresets = (ushort)value;
		}
	}

	public Office2007ChartMaterialProperties Material
	{
		get
		{
			return (Office2007ChartMaterialProperties)m_Material;
		}
		set
		{
			m_Material = (ushort)value;
		}
	}

	public Office2007ChartLightingProperties Lighting
	{
		get
		{
			return (Office2007ChartLightingProperties)m_Lighting;
		}
		set
		{
			m_Lighting = (ushort)value;
		}
	}

	public Office2007ChartBevelProperties BevelTop
	{
		get
		{
			return (Office2007ChartBevelProperties)m_BevelTop;
		}
		set
		{
			m_BevelTop = (ushort)value;
		}
	}

	public Office2007ChartBevelProperties BevelBottom
	{
		get
		{
			return (Office2007ChartBevelProperties)m_BevelBottom;
		}
		set
		{
			m_BevelBottom = (ushort)value;
		}
	}

	internal ChartImpl ChartObject
	{
		get
		{
			return m_chartObject;
		}
		set
		{
			m_chartObject = value;
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}
}
