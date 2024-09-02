using System;
using System.IO;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Row)]
[CLSCompliant(false)]
internal class RowRecord : BiffRecordRaw, IOutline
{
	internal enum OptionFlags
	{
		Colapsed = 0x10,
		ZeroHeight = 0x20,
		BadFontHeight = 0x40,
		Formatted = 0x80,
		ShowOutlineGroups = 0x100,
		SpaceAbove = 0x10000000,
		SpaceBelow = 0x20000000
	}

	public const ushort DEF_OUTLINE_LEVEL_MASK = 7;

	public const double DEF_MAX_HEIGHT = 409.5;

	internal const int DEF_RECORD_SIZE = 16;

	[BiffRecordPos(0, 2)]
	private ushort m_usRowNumber;

	[BiffRecordPos(2, 2)]
	private ushort m_usFirstCol;

	[BiffRecordPos(4, 2)]
	private ushort m_usLastCol;

	[BiffRecordPos(6, 2)]
	private ushort m_usHeigth;

	[BiffRecordPos(8, 4, true)]
	private int m_iReserved;

	[BiffRecordPos(12, 4, true)]
	private OptionFlags m_optionFlags = OptionFlags.ShowOutlineGroups;

	private WorksheetImpl m_sheet;

	public int Options
	{
		get
		{
			return (int)m_optionFlags;
		}
		set
		{
			m_optionFlags = (OptionFlags)value;
		}
	}

	public ushort RowNumber
	{
		get
		{
			return m_usRowNumber;
		}
		set
		{
			m_usRowNumber = value;
		}
	}

	public ushort FirstColumn
	{
		get
		{
			return m_usFirstCol;
		}
		set
		{
			m_usFirstCol = value;
		}
	}

	public ushort LastColumn
	{
		get
		{
			return m_usLastCol;
		}
		set
		{
			m_usLastCol = value;
		}
	}

	public ushort Height
	{
		get
		{
			return m_usHeigth;
		}
		set
		{
			m_usHeigth = value;
		}
	}

	public ushort ExtendedFormatIndex
	{
		get
		{
			return (ushort)((int)(m_optionFlags & (OptionFlags)268369920) >> 16);
		}
		set
		{
			int optionFlags = (int)m_optionFlags;
			optionFlags &= -268369921;
			optionFlags |= (value << 16) & 0xFFF0000;
			if (value != 15)
			{
				IsFormatted = true;
			}
			m_optionFlags = (OptionFlags)optionFlags;
		}
	}

	public ushort OutlineLevel
	{
		get
		{
			return (ushort)(m_optionFlags & (OptionFlags)7);
		}
		set
		{
			if (value > 7)
			{
				throw new ArgumentOutOfRangeException();
			}
			int optionFlags = (int)m_optionFlags;
			optionFlags &= -8;
			optionFlags |= value & 7;
			m_optionFlags = (OptionFlags)optionFlags;
		}
	}

	public bool IsCollapsed
	{
		get
		{
			return (m_optionFlags & OptionFlags.Colapsed) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= OptionFlags.Colapsed;
			}
			else
			{
				m_optionFlags &= (OptionFlags)(-17);
			}
		}
	}

	public bool IsHidden
	{
		get
		{
			return (m_optionFlags & OptionFlags.ZeroHeight) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= OptionFlags.ZeroHeight;
			}
			else
			{
				m_optionFlags &= (OptionFlags)(-33);
			}
		}
	}

	public bool IsBadFontHeight
	{
		get
		{
			return (m_optionFlags & OptionFlags.BadFontHeight) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= OptionFlags.BadFontHeight;
			}
			else
			{
				m_optionFlags &= (OptionFlags)(-65);
			}
		}
	}

	public bool IsFormatted
	{
		get
		{
			return (m_optionFlags & OptionFlags.Formatted) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= OptionFlags.Formatted;
			}
			else
			{
				m_optionFlags &= (OptionFlags)(-129);
			}
		}
	}

	public bool IsSpaceAboveRow
	{
		get
		{
			return (m_optionFlags & OptionFlags.SpaceAbove) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= OptionFlags.SpaceAbove;
			}
			else
			{
				m_optionFlags &= (OptionFlags)(-268435457);
			}
		}
	}

	public bool IsSpaceBelowRow
	{
		get
		{
			return (m_optionFlags & OptionFlags.SpaceBelow) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= OptionFlags.SpaceBelow;
			}
			else
			{
				m_optionFlags &= (OptionFlags)(-536870913);
			}
		}
	}

	public bool IsGroupShown
	{
		get
		{
			return (m_optionFlags & OptionFlags.ShowOutlineGroups) != 0;
		}
		set
		{
			if (value)
			{
				m_optionFlags |= OptionFlags.ShowOutlineGroups;
			}
			else
			{
				m_optionFlags &= (OptionFlags)(-257);
			}
		}
	}

	public int Reserved => m_iReserved;

	public override int MinimumRecordSize => 16;

	public override int MaximumRecordSize => 16;

	public override int MaximumMemorySize => 16;

	ushort IOutline.Index
	{
		get
		{
			return RowNumber;
		}
		set
		{
			RowNumber = value;
		}
	}

	internal WorksheetImpl Worksheet
	{
		get
		{
			return m_sheet;
		}
		set
		{
			m_sheet = value;
		}
	}

	public RowRecord()
	{
	}

	public RowRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public RowRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usRowNumber = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usFirstCol = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usLastCol = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usHeigth = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_iReserved = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_optionFlags = (OptionFlags)provider.ReadInt32(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		IsFormatted = ExtendedFormatIndex != 15;
		provider.WriteUInt16(iOffset, m_usRowNumber);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usFirstCol);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usLastCol);
		iOffset += 2;
		if (Worksheet != null)
		{
			ushort value = (IsBadFontHeight ? m_usHeigth : ((ushort)(Worksheet.PageSetup as PageSetupImpl).DefaultRowHeight));
			provider.WriteUInt16(iOffset, value);
		}
		else
		{
			provider.WriteUInt16(iOffset, m_usHeigth);
		}
		iOffset += 2;
		provider.WriteInt32(iOffset, m_iReserved);
		iOffset += 4;
		provider.WriteInt32(iOffset, (int)m_optionFlags);
	}
}
