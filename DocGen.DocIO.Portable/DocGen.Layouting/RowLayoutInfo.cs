namespace DocGen.Layouting;

internal class RowLayoutInfo : LayoutInfo, ILayoutSpacingsInfo
{
	private ushort m_bFlags;

	private double m_rowHeight;

	private Spacings m_margins;

	private Spacings m_paddings;

	internal bool IsFootnoteReduced
	{
		get
		{
			return (m_bFlags & 1) != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFEu) | (value ? 1u : 0u));
		}
	}

	internal bool IsFootnoteSplitted
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFDu) | ((value ? 1u : 0u) << 1));
		}
	}

	internal bool IsExactlyRowHeight
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFBu) | ((value ? 1u : 0u) << 2));
		}
	}

	internal bool IsRowSplitted
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFF7u) | ((value ? 1u : 0u) << 3));
		}
	}

	internal double RowHeight
	{
		get
		{
			if (m_rowHeight < 0.0)
			{
				m_rowHeight = 0.0 - m_rowHeight;
			}
			return m_rowHeight;
		}
	}

	internal bool IsRowHasVerticalMergeContinueCell
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFEFu) | ((value ? 1u : 0u) << 4));
		}
	}

	internal bool IsRowHasVerticalMergeEndCell
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFDFu) | ((value ? 1u : 0u) << 5));
		}
	}

	internal bool IsRowHasVerticalMergeStartCell
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFBFu) | ((value ? 1u : 0u) << 6));
		}
	}

	internal bool IsRowHasVerticalTextCell
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFF7Fu) | ((value ? 1u : 0u) << 7));
		}
	}

	internal bool IsRowBreakByPageBreakBefore
	{
		get
		{
			return (m_bFlags & 0x100) >> 8 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFEFFu) | ((value ? 1u : 0u) << 8));
		}
	}

	internal bool IsRowHeightExceedsClientByFloatingItem
	{
		get
		{
			return (m_bFlags & 0x200) >> 9 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFDFFu) | ((value ? 1u : 0u) << 9));
		}
	}

	internal bool IsCellPaddingUpdated
	{
		get
		{
			return (m_bFlags & 0x400) >> 10 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFBFFu) | ((value ? 1u : 0u) << 10));
		}
	}

	internal bool IsRowSplittedByFloatingItem
	{
		get
		{
			return (m_bFlags & 0x800) >> 11 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xF7FFu) | ((value ? 1u : 0u) << 11));
		}
	}

	public Spacings Paddings
	{
		get
		{
			if (m_paddings == null)
			{
				m_paddings = new Spacings();
			}
			return m_paddings;
		}
	}

	public Spacings Margins
	{
		get
		{
			if (m_margins == null)
			{
				m_margins = new Spacings();
			}
			return m_margins;
		}
	}

	public RowLayoutInfo(bool isExactlyRow, float rowHeight)
	{
		IsExactlyRowHeight = isExactlyRow;
		m_rowHeight = rowHeight;
	}
}
