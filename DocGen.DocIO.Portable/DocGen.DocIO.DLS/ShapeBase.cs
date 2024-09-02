using System.IO;

namespace DocGen.DocIO.DLS;

public abstract class ShapeBase : ShapeCommon
{
	private int m_ZOrderPosition = int.MaxValue;

	private HorizontalOrigin m_HorizontalOrigin;

	private VerticalOrigin m_VerticalOrigin;

	private HorizontalOrigin m_relHorzOrigin;

	private VerticalOrigin m_relVertOrigin;

	private HorizontalOrigin m_relWidthHorzOrigin;

	private VerticalOrigin m_relHeightVertOrigin;

	internal WrapFormat m_WrapFormat;

	private float m_relHorzpos;

	private float m_relvertpos;

	private float m_relHeight;

	private float m_relWidth;

	private ShapeHorizontalAlignment m_horAlignment;

	private ShapeVerticalAlignment m_vertAlignment;

	private float m_horizPosition;

	private float m_vertPosition;

	private byte m_bFlags;

	internal byte m_bFlags1 = 24;

	private const byte LeftEdgeExtentKey = 1;

	private const byte RightEdgeExtentKey = 2;

	private const byte TopEdgeExtentKey = 3;

	private const byte BottomEdgeExtentKey = 4;

	public HorizontalOrigin HorizontalOrigin
	{
		get
		{
			return m_HorizontalOrigin;
		}
		set
		{
			m_HorizontalOrigin = value;
		}
	}

	internal HorizontalOrigin RelativeWidthHorizontalOrigin
	{
		get
		{
			return m_relWidthHorzOrigin;
		}
		set
		{
			m_relWidthHorzOrigin = value;
		}
	}

	internal VerticalOrigin RelativeHeightVerticalOrigin
	{
		get
		{
			return m_relHeightVertOrigin;
		}
		set
		{
			m_relHeightVertOrigin = value;
		}
	}

	internal HorizontalOrigin RelativeHorizontalOrigin
	{
		get
		{
			return m_relHorzOrigin;
		}
		set
		{
			m_relHorzOrigin = value;
		}
	}

	internal VerticalOrigin RelativeVerticalOrigin
	{
		get
		{
			return m_relVertOrigin;
		}
		set
		{
			m_relVertOrigin = value;
		}
	}

	public ShapeHorizontalAlignment HorizontalAlignment
	{
		get
		{
			return m_horAlignment;
		}
		set
		{
			m_horAlignment = value;
		}
	}

	public float HorizontalPosition
	{
		get
		{
			return m_horizPosition;
		}
		set
		{
			m_horizPosition = value;
		}
	}

	internal float RelativeHorizontalPosition
	{
		get
		{
			return m_relHorzpos;
		}
		set
		{
			m_relHorzpos = value;
		}
	}

	internal float RelativeVerticalPosition
	{
		get
		{
			return m_relvertpos;
		}
		set
		{
			m_relvertpos = value;
		}
	}

	internal float RelativeHeight
	{
		get
		{
			return m_relHeight;
		}
		set
		{
			m_relHeight = value;
		}
	}

	internal float RelativeWidth
	{
		get
		{
			return m_relWidth;
		}
		set
		{
			m_relWidth = value;
		}
	}

	internal bool IsRelativeVerticalPosition
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsRelativeHorizontalPosition
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal bool IsRelativeHeight
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool IsRelativeWidth
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (byte)((m_bFlags & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	public VerticalOrigin VerticalOrigin
	{
		get
		{
			return m_VerticalOrigin;
		}
		set
		{
			m_VerticalOrigin = value;
		}
	}

	public ShapeVerticalAlignment VerticalAlignment
	{
		get
		{
			return m_vertAlignment;
		}
		set
		{
			m_vertAlignment = value;
		}
	}

	public float VerticalPosition
	{
		get
		{
			return m_vertPosition;
		}
		set
		{
			m_vertPosition = value;
		}
	}

	public WrapFormat WrapFormat
	{
		get
		{
			if (m_WrapFormat == null)
			{
				m_WrapFormat = new WrapFormat();
			}
			return m_WrapFormat;
		}
		set
		{
			m_WrapFormat = value;
		}
	}

	internal int ZOrderPosition
	{
		get
		{
			return m_ZOrderPosition;
		}
		set
		{
			m_ZOrderPosition = value;
		}
	}

	public bool IsBelowText
	{
		get
		{
			return WrapFormat.IsBelowText;
		}
		set
		{
			WrapFormat.IsBelowText = value;
		}
	}

	internal bool LayoutInCell
	{
		get
		{
			return (m_bFlags1 & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	public bool LockAnchor
	{
		get
		{
			return (m_bFlags1 & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal new bool IsCloned
	{
		get
		{
			return (m_bFlags1 & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	public bool Visible
	{
		get
		{
			return (m_bFlags1 & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags1 = (byte)((m_bFlags1 & 0xF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal float LeftEdgeExtent
	{
		get
		{
			if (!base.PropertiesHash.ContainsKey(1))
			{
				return 0f;
			}
			return (float)base.PropertiesHash[1];
		}
		set
		{
			SetKeyValue(1, value);
		}
	}

	internal float TopEdgeExtent
	{
		get
		{
			if (!base.PropertiesHash.ContainsKey(3))
			{
				return 0f;
			}
			return (float)base.PropertiesHash[3];
		}
		set
		{
			SetKeyValue(3, value);
		}
	}

	internal float RightEdgeExtent
	{
		get
		{
			if (!base.PropertiesHash.ContainsKey(2))
			{
				return 0f;
			}
			return (float)base.PropertiesHash[2];
		}
		set
		{
			SetKeyValue(2, value);
		}
	}

	internal float BottomEdgeExtent
	{
		get
		{
			if (!base.PropertiesHash.ContainsKey(4))
			{
				return 0f;
			}
			return (float)base.PropertiesHash[4];
		}
		set
		{
			SetKeyValue(4, value);
		}
	}

	internal ShapeBase(WordDocument doc)
		: base(doc)
	{
	}

	protected override object CloneImpl()
	{
		ShapeBase shapeBase = (ShapeBase)base.CloneImpl();
		if (WrapFormat != null && WrapFormat.WrapPolygon != null)
		{
			shapeBase.WrapFormat.WrapPolygon = WrapFormat.WrapPolygon.Clone();
		}
		return shapeBase;
	}

	internal override void Close()
	{
		if (m_WrapFormat != null)
		{
			m_WrapFormat.Close();
			m_WrapFormat = null;
		}
		if (m_docxProps != null)
		{
			foreach (Stream value in m_docxProps.Values)
			{
				value.Close();
			}
			m_docxProps.Clear();
			m_docxProps = null;
		}
		base.Close();
	}
}
