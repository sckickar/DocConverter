using System;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation;

internal class ShadowImpl : CommonObject, IShadow, ICloneParent
{
	private ShadowData m_chartShadowFormat = new ShadowData();

	private WorkbookImpl m_parentBook;

	private bool m_HasCustomShadowStyle;

	private int m_Transparency = -1;

	private int m_Size;

	private int m_Blur;

	private ChartMarkerFormatRecord m_Shadow;

	private int m_Angle;

	private int m_Distance;

	private ChartColor m_shadowColor;

	internal Stream m_glowStream;

	[CLSCompliant(false)]
	public ChartMarkerFormatRecord ShadowFormat
	{
		get
		{
			if (m_Shadow == null)
			{
				m_Shadow = (ChartMarkerFormatRecord)BiffRecordFactory.GetRecord(TBIFFRecord.ChartMarkerFormat);
				m_Shadow.IsAutoColor = true;
			}
			return m_Shadow;
		}
	}

	public Office2007ChartPresetsOuter ShadowOuterPresets
	{
		get
		{
			return m_chartShadowFormat.ShadowOuterPresets;
		}
		set
		{
			if (value != ShadowOuterPresets)
			{
				m_chartShadowFormat.ShadowOuterPresets = value;
			}
		}
	}

	public Office2007ChartPresetsInner ShadowInnerPresets
	{
		get
		{
			return m_chartShadowFormat.ShadowInnerPresets;
		}
		set
		{
			if (value != ShadowInnerPresets)
			{
				m_chartShadowFormat.ShadowInnerPresets = value;
			}
		}
	}

	public bool HasCustomShadowStyle
	{
		get
		{
			return m_HasCustomShadowStyle;
		}
		set
		{
			m_HasCustomShadowStyle = value;
		}
	}

	public Office2007ChartPresetsPerspective ShadowPerspectivePresets
	{
		get
		{
			return m_chartShadowFormat.ShadowPrespectivePresets;
		}
		set
		{
			if (value != ShadowPerspectivePresets)
			{
				m_chartShadowFormat.ShadowPrespectivePresets = value;
			}
		}
	}

	public int Transparency
	{
		get
		{
			if (m_Transparency != -1)
			{
				return 100 - m_Transparency;
			}
			return m_Transparency;
		}
		set
		{
			if (value != -1)
			{
				if (value < 0 || value > 100)
				{
					throw new NotSupportedException("The Value of the transparency should be between(0-100)");
				}
				m_Transparency = 100 - value;
			}
		}
	}

	internal ChartColor ColorObject => m_shadowColor;

	public int Size
	{
		get
		{
			return m_Size / 1000;
		}
		set
		{
			if (HasCustomShadowStyle)
			{
				if (value <= 0 || value > 200)
				{
					throw new NotSupportedException("The value of the size should be between(0-200)");
				}
				m_Size = value * 1000;
			}
		}
	}

	public int Blur
	{
		get
		{
			return m_Blur / 12700;
		}
		set
		{
			if (HasCustomShadowStyle)
			{
				if (value < 0 || value > 100)
				{
					throw new NotSupportedException("The Value of the blur should be between(0-100)");
				}
				m_Blur = value * 12700;
			}
		}
	}

	public int Angle
	{
		get
		{
			return m_Angle / 60000;
		}
		set
		{
			if (HasCustomShadowStyle)
			{
				if (value < 0 || value > 359)
				{
					throw new NotSupportedException("The Value of the angle should be between(0-359)");
				}
				m_Angle = value * 60000;
			}
		}
	}

	public Color ShadowColor
	{
		get
		{
			return m_shadowColor.GetRGB(m_parentBook);
		}
		set
		{
			m_shadowColor.SetRGB(value);
		}
	}

	public int Distance
	{
		get
		{
			return m_Distance / 12700;
		}
		set
		{
			if (HasCustomShadowStyle)
			{
				if (value < 0 || value > 200)
				{
					throw new NotSupportedException("The Value of the distance should be between(0-200)");
				}
				m_Distance = value * 12700;
			}
		}
	}

	public ShadowImpl(IApplication application, object parent)
		: base(application, parent)
	{
		InitializeColors();
		SetParents();
		if (base.Parent is ChartWallOrFloorImpl)
		{
			(base.Parent as ChartWallOrFloorImpl).HasShapeProperties = true;
		}
	}

	private void InitializeColors()
	{
		m_shadowColor = new ChartColor(ColorExtension.Empty);
		m_shadowColor.AfterChange += ShadowColorChanged;
	}

	internal void ShadowColorChanged()
	{
		OfficeKnownColors indexed = m_shadowColor.GetIndexed(m_parentBook);
		ShadowFormat.FillColorIndex = (ushort)indexed;
		ShadowFormat.IsNotShowInt = indexed == OfficeKnownColors.Black;
	}

	private void SetParents()
	{
		m_parentBook = (WorkbookImpl)FindParent(typeof(WorkbookImpl));
		if (m_parentBook == null)
		{
			throw new ApplicationException("cannot find parent objects.");
		}
	}

	object ICloneParent.Clone(object parent)
	{
		return Clone(parent);
	}

	public ShadowImpl Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ShadowImpl obj = (ShadowImpl)MemberwiseClone();
		obj.m_chartShadowFormat = (ShadowData)CloneUtils.CloneCloneable(m_chartShadowFormat);
		obj.SetParent(parent);
		obj.SetParents();
		return obj;
	}

	public void CustomShadowStyles(Office2007ChartPresetsOuter iOuter, int iTransparency, int iSize, int iBlur, int iAngle, int iDistance, bool CustomShadowStyle)
	{
		if (!CustomShadowStyle)
		{
			throw new NotSupportedException("It should be set true to implement the custom shadow style");
		}
		if (iOuter == Office2007ChartPresetsOuter.NoShadow)
		{
			throw new NotSupportedException("The method does not accept Noshadow");
		}
		if (iSize <= 0 || iSize > 200)
		{
			throw new NotSupportedException("The value of the size should be between(0-200)");
		}
		if (iTransparency < 0 || iTransparency > 100)
		{
			throw new NotSupportedException("The Value of the transparency should be between(0-100)");
		}
		if (iBlur < 0 || iBlur > 100)
		{
			throw new NotSupportedException("The Value of the blur should be between(0-100)");
		}
		if (iAngle < 0 || iAngle > 359)
		{
			throw new NotSupportedException("The Value of the angle should be between(0-359)");
		}
		if (iDistance < 0 || iDistance > 200)
		{
			throw new NotSupportedException("The Value of the distance should be between(0-200)");
		}
		m_HasCustomShadowStyle = CustomShadowStyle;
		m_chartShadowFormat.ShadowOuterPresets = iOuter;
		m_Transparency = (100 - iTransparency) * 1000;
		m_Size = iSize * 1000;
		m_Blur = iBlur * 12700;
		m_Angle = iAngle * 60000;
		m_Distance = iDistance * 12700;
	}

	public void CustomShadowStyles(Office2007ChartPresetsInner iInner, int iTransparency, int iBlur, int iAngle, int iDistance, bool CustomShadowStyle)
	{
		if (!CustomShadowStyle)
		{
			throw new NotSupportedException("It should be set true to implement the custom shadow style");
		}
		if (iInner == Office2007ChartPresetsInner.NoShadow)
		{
			throw new NotSupportedException("The method does not accept Noshadow");
		}
		if (iTransparency < 0 || iTransparency > 100)
		{
			throw new NotSupportedException("The Value of the transparency should be between(0-100)");
		}
		if (iBlur < 0 || iBlur > 100)
		{
			throw new NotSupportedException("The Value of the blur should be between(0-100)");
		}
		if (iAngle < 0 || iAngle > 359)
		{
			throw new NotSupportedException("The Value of the angle should be between(0-359)");
		}
		if (iDistance < 0 || iDistance > 200)
		{
			throw new NotSupportedException("The Value of the distance should be between(0-200)");
		}
		m_HasCustomShadowStyle = CustomShadowStyle;
		m_chartShadowFormat.ShadowInnerPresets = iInner;
		m_Transparency = (100 - iTransparency) * 1000;
		m_Blur = iBlur * 12700;
		m_Angle = iAngle * 60000;
		m_Distance = iDistance * 12700;
	}

	public void CustomShadowStyles(Office2007ChartPresetsPerspective iPerspective, int iTransparency, int iSize, int iBlur, int iAngle, int iDistance, bool CustomShadowStyle)
	{
		if (!CustomShadowStyle)
		{
			throw new NotSupportedException("It should be set true to implement the custom shadow style");
		}
		if (iPerspective == Office2007ChartPresetsPerspective.NoShadow)
		{
			throw new NotSupportedException("The method does not accept Noshadow");
		}
		if (iSize <= 0 || iSize > 200)
		{
			throw new NotSupportedException("The value of the size should be between(0-200)");
		}
		if (iTransparency < 0 || iTransparency > 100)
		{
			throw new NotSupportedException("The Value of the transparency should be between(0-100)");
		}
		if (iBlur < 0 || iBlur > 100)
		{
			throw new NotSupportedException("The Value of the blur should be between(0-100)");
		}
		if (iAngle < 0 || iAngle > 359)
		{
			throw new NotSupportedException("The Value of the angle should be between(0-359)");
		}
		if (iDistance < 0 || iDistance > 200)
		{
			throw new NotSupportedException("The Value of the distance should be between(0-200)");
		}
		m_HasCustomShadowStyle = CustomShadowStyle;
		m_chartShadowFormat.ShadowPrespectivePresets = iPerspective;
		m_Transparency = (100 - iTransparency) * 1000;
		m_Size = iSize * 1000;
		m_Blur = iBlur * 12700;
		m_Angle = iAngle * 60000;
		m_Distance = iDistance * 12700;
	}
}
