namespace DocGen.OfficeChart.Implementation.Shapes;

internal class ShapeFrame
{
	private long m_offsetX;

	private long m_offsetY;

	private long m_offsetCX;

	private long m_offsetCY;

	private int m_rotation;

	private long m_chOffsetX;

	private long m_chOffsetY;

	private long m_chOffsetCX;

	private long m_chOffsetCY;

	private ShapeImpl m_baseShape;

	internal long OffsetX
	{
		get
		{
			return m_offsetX;
		}
		set
		{
			m_offsetX = value;
		}
	}

	internal long OffsetY
	{
		get
		{
			return m_offsetY;
		}
		set
		{
			m_offsetY = value;
		}
	}

	internal long OffsetCX
	{
		get
		{
			return m_offsetCX;
		}
		set
		{
			m_offsetCX = value;
		}
	}

	internal long OffsetCY
	{
		get
		{
			return m_offsetCY;
		}
		set
		{
			m_offsetCY = value;
		}
	}

	internal long ChOffsetX
	{
		get
		{
			return m_chOffsetX;
		}
		set
		{
			m_chOffsetX = value;
		}
	}

	internal long ChOffsetY
	{
		get
		{
			return m_chOffsetY;
		}
		set
		{
			m_chOffsetY = value;
		}
	}

	internal long ChOffsetCX
	{
		get
		{
			return m_chOffsetCX;
		}
		set
		{
			m_chOffsetCX = value;
		}
	}

	internal long ChOffsetCY
	{
		get
		{
			return m_chOffsetCY;
		}
		set
		{
			m_chOffsetCY = value;
		}
	}

	internal int Rotation
	{
		get
		{
			return m_rotation;
		}
		set
		{
			m_rotation = value;
		}
	}

	internal ShapeFrame(ShapeImpl shape)
	{
		m_baseShape = shape;
	}

	internal void SetAnchor(int rotation, long offsetX, long offsetY, long offsetCx, long offsetCy)
	{
		m_rotation = rotation;
		m_offsetX = offsetX;
		m_offsetY = offsetY;
		m_offsetCX = offsetCx;
		m_offsetCY = offsetCy;
	}

	internal void SetChildAnchor(long childOffsetX, long childOffsetY, long childOffsetCx, long childOffsetCy)
	{
		m_chOffsetCX = childOffsetCx;
		m_chOffsetCY = childOffsetCy;
		m_chOffsetX = childOffsetX;
		m_chOffsetY = childOffsetY;
	}

	internal ShapeFrame Clone(object parent)
	{
		ShapeFrame obj = (ShapeFrame)MemberwiseClone();
		obj.m_baseShape = (ShapeImpl)parent;
		return obj;
	}

	internal void SetParent(ShapeImpl shape)
	{
		m_baseShape = shape;
	}

	internal void Close()
	{
		m_baseShape = null;
	}
}
