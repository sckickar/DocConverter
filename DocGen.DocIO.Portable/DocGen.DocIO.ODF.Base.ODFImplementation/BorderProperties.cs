namespace DocGen.DocIO.ODF.Base.ODFImplementation;

internal class BorderProperties
{
	private ODFBorder m_border;

	private ODFBorder m_borderTop;

	private ODFBorder m_borderBottom;

	private ODFBorder m_borderLeft;

	private ODFBorder m_borderRight;

	private ODFBorder m_diagonalLeft;

	private ODFBorder m_diagonalRight;

	internal byte borderFlags;

	internal const ushort BorderKey = 0;

	internal const ushort BorderTopKey = 1;

	internal const ushort BorderBottomKey = 2;

	internal const ushort BorderLeftKey = 3;

	internal const ushort BorderRightKey = 4;

	internal ODFBorder Border
	{
		get
		{
			return m_border;
		}
		set
		{
			borderFlags = (byte)((borderFlags & 0xFEu) | 1u);
			m_border = value;
		}
	}

	internal ODFBorder BorderTop
	{
		get
		{
			return m_borderTop;
		}
		set
		{
			borderFlags = (byte)((borderFlags & 0xFDu) | 2u);
			m_borderTop = value;
		}
	}

	internal ODFBorder BorderBottom
	{
		get
		{
			return m_borderBottom;
		}
		set
		{
			borderFlags = (byte)((borderFlags & 0xFBu) | 4u);
			m_borderBottom = value;
		}
	}

	internal ODFBorder BorderLeft
	{
		get
		{
			return m_borderLeft;
		}
		set
		{
			borderFlags = (byte)((borderFlags & 0xF7u) | 8u);
			m_borderLeft = value;
		}
	}

	internal ODFBorder BorderRight
	{
		get
		{
			return m_borderRight;
		}
		set
		{
			borderFlags = (byte)((borderFlags & 0xEFu) | 0x10u);
			m_borderRight = value;
		}
	}

	internal ODFBorder DiagonalLeft
	{
		get
		{
			return m_diagonalLeft;
		}
		set
		{
			m_diagonalLeft = value;
		}
	}

	internal ODFBorder DiagonalRight
	{
		get
		{
			return m_diagonalRight;
		}
		set
		{
			m_diagonalRight = value;
		}
	}

	internal void Dispose()
	{
		if (m_border != null)
		{
			m_border = null;
		}
		if (m_borderBottom != null)
		{
			m_borderBottom = null;
		}
		if (m_borderLeft != null)
		{
			m_borderLeft = null;
		}
		if (m_borderRight != null)
		{
			m_borderRight = null;
		}
		if (m_borderTop != null)
		{
			m_borderTop = null;
		}
		if (m_diagonalLeft != null)
		{
			m_diagonalLeft = null;
		}
		if (m_diagonalRight != null)
		{
			m_diagonalRight = null;
		}
	}
}
