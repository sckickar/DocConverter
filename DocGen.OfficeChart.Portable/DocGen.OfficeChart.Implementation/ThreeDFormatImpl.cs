using System;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation.Charts;
using DocGen.OfficeChart.Interfaces;
using DocGen.OfficeChart.Parser.Biff_Records;
using DocGen.OfficeChart.Parser.Biff_Records.Charts;

namespace DocGen.OfficeChart.Implementation;

internal class ThreeDFormatImpl : CommonObject, IThreeDFormat, ICloneParent
{
	private ShadowData m_chartShadowFormat = new ShadowData();

	private WorkbookImpl m_parentBook;

	private int m_bevelTopHeight = -1;

	private int m_bevelTopWidth = -1;

	private int m_bevelBottomHeight = -1;

	private int m_bevelBottomWidth = -1;

	private string m_prestShape;

	private double m_lightningLatitude;

	private double m_lightningLongitutde;

	private double m_lightningAngle;

	public Office2007ChartBevelProperties BevelTop
	{
		get
		{
			return m_chartShadowFormat.BevelTop;
		}
		set
		{
			if (value != BevelTop)
			{
				m_chartShadowFormat.BevelTop = value;
			}
		}
	}

	public Office2007ChartBevelProperties BevelBottom
	{
		get
		{
			return m_chartShadowFormat.BevelBottom;
		}
		set
		{
			if (value != BevelBottom)
			{
				m_chartShadowFormat.BevelBottom = value;
			}
		}
	}

	public Office2007ChartMaterialProperties Material
	{
		get
		{
			return m_chartShadowFormat.Material;
		}
		set
		{
			if (value != Material)
			{
				m_chartShadowFormat.Material = value;
			}
		}
	}

	public Office2007ChartLightingProperties Lighting
	{
		get
		{
			return m_chartShadowFormat.Lighting;
		}
		set
		{
			if (value != Lighting)
			{
				m_chartShadowFormat.Lighting = value;
			}
		}
	}

	public int BevelTopHeight
	{
		get
		{
			return (int)Helper.EmuToPoint(m_bevelTopHeight);
		}
		set
		{
			if (value < 0 || value > 1584)
			{
				throw new ArgumentException("Invalid BevalTopHeight " + value + "The value ranges from 0 to 1584");
			}
			m_bevelTopHeight = Helper.PointToEmu(value);
		}
	}

	public int BevelBottomHeight
	{
		get
		{
			return (int)Helper.EmuToPoint(m_bevelBottomHeight);
		}
		set
		{
			if (value < 0 || value > 1584)
			{
				throw new ArgumentException("Invalid BevelBottomHeight " + value + "The value ranges from 0 to 1584");
			}
			m_bevelBottomHeight = Helper.PointToEmu(value);
		}
	}

	public int BevelTopWidth
	{
		get
		{
			return (int)Helper.EmuToPoint(m_bevelTopWidth);
		}
		set
		{
			if (value < 0 || value > 1584)
			{
				throw new ArgumentException("Invalid BevelTopWidth " + value + "The value ranges from 0 to 1584");
			}
			m_bevelTopWidth = Helper.PointToEmu(value);
		}
	}

	public int BevelBottomWidth
	{
		get
		{
			return (int)Helper.EmuToPoint(m_bevelBottomWidth);
		}
		set
		{
			if (value < 0 || value > 1584)
			{
				throw new ArgumentException("Invalid BevelBottomWidth " + value + "The value ranges from 0 to 1584");
			}
			m_bevelBottomWidth = Helper.PointToEmu(value);
		}
	}

	public string PresetShape
	{
		get
		{
			return m_prestShape;
		}
		set
		{
			m_prestShape = value;
		}
	}

	internal bool IsBevelTopWidthSet => m_bevelTopWidth != -1;

	internal bool IsBevelTopHeightSet => m_bevelTopHeight != -1;

	internal bool IsBevelBottomWidthSet => m_bevelBottomWidth != -1;

	internal bool IsBevelBottomHeightSet => m_bevelBottomHeight != -1;

	internal double LightningLatitude
	{
		get
		{
			return m_lightningLatitude;
		}
		set
		{
			m_lightningLatitude = value;
		}
	}

	internal double LightningLongitude
	{
		get
		{
			return m_lightningLongitutde;
		}
		set
		{
			m_lightningLongitutde = value;
		}
	}

	internal double LightningAngle
	{
		get
		{
			return m_lightningAngle;
		}
		set
		{
			m_lightningAngle = value;
		}
	}

	public ThreeDFormatImpl(IApplication application, object parent)
		: base(application, parent)
	{
		SetParents();
		if (base.Parent is ChartWallOrFloorImpl)
		{
			(base.Parent as ChartWallOrFloorImpl).HasShapeProperties = true;
		}
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

	public ThreeDFormatImpl Clone(object parent)
	{
		if (parent == null)
		{
			throw new ArgumentNullException("parent");
		}
		ThreeDFormatImpl threeDFormatImpl = (ThreeDFormatImpl)MemberwiseClone();
		threeDFormatImpl.m_chartShadowFormat = (ShadowData)CloneUtils.CloneCloneable(m_chartShadowFormat);
		threeDFormatImpl.SetParent(parent);
		threeDFormatImpl.SetParents();
		threeDFormatImpl.m_chartShadowFormat.ChartObject = threeDFormatImpl.FindParent(typeof(ChartImpl)) as ChartImpl;
		return threeDFormatImpl;
	}
}
