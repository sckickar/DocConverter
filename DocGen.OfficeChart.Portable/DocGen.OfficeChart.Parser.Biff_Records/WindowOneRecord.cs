using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.WindowOne)]
[CLSCompliant(false)]
internal class WindowOneRecord : BiffRecordRaw
{
	private enum OptionFlags
	{
		Hidden = 1,
		Iconic = 2,
		Reserved = 4,
		HScroll = 8,
		VScroll = 0x10,
		Tabs = 0x20
	}

	private const int DEF_RECORD_SIZE = 18;

	[BiffRecordPos(0, 2)]
	private ushort m_usHHold = 240;

	[BiffRecordPos(2, 2)]
	private ushort m_usVHold = 90;

	[BiffRecordPos(4, 2)]
	private ushort m_usWidth = 11340;

	[BiffRecordPos(6, 2)]
	private ushort m_usHeight = 6795;

	[BiffRecordPos(8, 2)]
	private OptionFlags m_options = (OptionFlags)56;

	[BiffRecordPos(10, 2)]
	private ushort m_usSelectedTab;

	[BiffRecordPos(12, 2)]
	private ushort m_usDisplayedTab;

	[BiffRecordPos(14, 2)]
	private ushort m_usNumSelTabs = 1;

	[BiffRecordPos(16, 2)]
	private ushort m_usTabWidthRatio = 600;

	public ushort HHold
	{
		get
		{
			return m_usHHold;
		}
		set
		{
			m_usHHold = value;
		}
	}

	public ushort VHold
	{
		get
		{
			return m_usVHold;
		}
		set
		{
			m_usVHold = value;
		}
	}

	public ushort Width
	{
		get
		{
			return m_usWidth;
		}
		set
		{
			m_usWidth = value;
		}
	}

	public ushort Height
	{
		get
		{
			return m_usHeight;
		}
		set
		{
			m_usHeight = value;
		}
	}

	public ushort SelectedTab
	{
		get
		{
			return m_usSelectedTab;
		}
		set
		{
			m_usSelectedTab = value;
		}
	}

	public ushort DisplayedTab
	{
		get
		{
			return m_usDisplayedTab;
		}
		set
		{
			m_usDisplayedTab = value;
		}
	}

	public ushort NumSelectedTabs
	{
		get
		{
			return m_usNumSelTabs;
		}
		set
		{
			m_usNumSelTabs = value;
		}
	}

	public ushort TabWidthRatio
	{
		get
		{
			return m_usTabWidthRatio;
		}
		set
		{
			m_usTabWidthRatio = value;
		}
	}

	public bool IsHidden
	{
		get
		{
			return (m_options & OptionFlags.Hidden) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.Hidden;
			}
			else
			{
				m_options &= (OptionFlags)(-2);
			}
		}
	}

	public bool IsIconic
	{
		get
		{
			return (m_options & OptionFlags.Iconic) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.Iconic;
			}
			else
			{
				m_options &= (OptionFlags)(-3);
			}
		}
	}

	public bool IsHScroll
	{
		get
		{
			return (m_options & OptionFlags.HScroll) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.HScroll;
			}
			else
			{
				m_options &= (OptionFlags)(-9);
			}
		}
	}

	public bool IsVScroll
	{
		get
		{
			return (m_options & OptionFlags.VScroll) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.VScroll;
			}
			else
			{
				m_options &= (OptionFlags)(-17);
			}
		}
	}

	public bool IsTabs
	{
		get
		{
			return (m_options & OptionFlags.Tabs) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.Tabs;
			}
			else
			{
				m_options &= (OptionFlags)(-33);
			}
		}
	}

	public bool Reserved
	{
		get
		{
			return (m_options & OptionFlags.Reserved) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.Reserved;
			}
			else
			{
				m_options &= (OptionFlags)(-5);
			}
		}
	}

	public ushort Options => (ushort)m_options;

	public override int MinimumRecordSize => 18;

	public override int MaximumRecordSize => 18;

	public WindowOneRecord()
	{
	}

	public WindowOneRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public WindowOneRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usHHold = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usVHold = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usWidth = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usHeight = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_options = (OptionFlags)provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usSelectedTab = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usDisplayedTab = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usNumSelTabs = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usTabWidthRatio = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 18;
		provider.WriteUInt16(iOffset, m_usHHold);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usVHold);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usWidth);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usHeight);
		iOffset += 2;
		provider.WriteUInt16(iOffset, (ushort)m_options);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usSelectedTab);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usDisplayedTab);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usNumSelTabs);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usTabWidthRatio);
	}
}
