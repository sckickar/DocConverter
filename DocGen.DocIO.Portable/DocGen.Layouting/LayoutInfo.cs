using DocGen.Drawing;

namespace DocGen.Layouting;

internal class LayoutInfo : ILayoutInfo
{
	private ushort m_bFlags = 64;

	private ChildrenLayoutDirection m_childrenLayoutDirection;

	private SizeF m_size;

	private SyncFont m_font;

	public bool IsClipped
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

	public SizeF Size
	{
		get
		{
			return m_size;
		}
		set
		{
			m_size = value;
		}
	}

	public bool IsSkip
	{
		get
		{
			return (m_bFlags & 2) >> 1 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFDu) | (value ? 2u : 0u));
		}
	}

	public bool IsSkipBottomAlign
	{
		get
		{
			return (m_bFlags & 4) >> 2 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFBu) | (value ? 4u : 0u));
		}
	}

	public bool IsVerticalText
	{
		get
		{
			return (m_bFlags & 8) >> 3 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFF7u) | (value ? 8u : 0u));
		}
	}

	public bool IsLineContainer
	{
		get
		{
			return (m_bFlags & 0x10) >> 4 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFEFu) | (value ? 16u : 0u));
		}
	}

	public ChildrenLayoutDirection ChildrenLayoutDirection => m_childrenLayoutDirection;

	public bool IsLineBreak
	{
		get
		{
			return (m_bFlags & 0x20) >> 5 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFDFu) | (value ? 32u : 0u));
		}
	}

	internal bool IsLineNumberItem
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

	public bool TextWrap
	{
		get
		{
			return (m_bFlags & 0x40) >> 6 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFBFu) | (value ? 64u : 0u));
		}
	}

	public bool IsPageBreakItem
	{
		get
		{
			return (m_bFlags & 0x80) >> 7 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFF7Fu) | (value ? 128u : 0u));
		}
	}

	public bool IsFirstItemInPage
	{
		get
		{
			return (m_bFlags & 0x100) >> 8 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFEFFu) | (value ? 256u : 0u));
		}
	}

	public bool IsKeepWithNext
	{
		get
		{
			return (m_bFlags & 0x200) >> 9 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFDFFu) | (value ? 512u : 0u));
		}
	}

	public bool IsHiddenRow
	{
		get
		{
			return (m_bFlags & 0x400) >> 10 != 0;
		}
		set
		{
			m_bFlags = (ushort)((m_bFlags & 0xFFFFFBFFu) | (int)((value ? 1u : 0u) << 10));
		}
	}

	public SyncFont Font
	{
		get
		{
			return m_font;
		}
		set
		{
			m_font = value;
		}
	}

	public LayoutInfo()
	{
		IsSkip = true;
	}

	public LayoutInfo(ChildrenLayoutDirection childLayoutDirection)
	{
		m_childrenLayoutDirection = childLayoutDirection;
	}
}
