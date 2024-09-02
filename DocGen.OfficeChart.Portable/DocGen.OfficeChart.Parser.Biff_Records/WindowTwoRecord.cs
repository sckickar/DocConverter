using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.WindowTwo)]
[CLSCompliant(false)]
internal class WindowTwoRecord : BiffRecordRaw
{
	[Flags]
	private enum OptionFlags : ushort
	{
		DisplayFormulas = 1,
		DisplayGridlines = 2,
		DisplayRowColHeadings = 4,
		FreezePanes = 8,
		DisplayZeros = 0x10,
		DefaultHeader = 0x20,
		Arabic = 0x40,
		DisplayGuts = 0x80,
		FreezePanesNoSplit = 0x100,
		Selected = 0x200,
		Paged = 0x400,
		SavedInPageBreakPreview = 0x800
	}

	private const int DEF_MAX_RECORD_SIZE = 18;

	internal const int DEF_MAX_CHART_SHEET_SIZE = 10;

	[BiffRecordPos(0, 2)]
	private OptionFlags m_options = OptionFlags.DisplayGridlines | OptionFlags.DisplayRowColHeadings | OptionFlags.DisplayZeros | OptionFlags.DefaultHeader | OptionFlags.DisplayGuts;

	[BiffRecordPos(2, 2)]
	private ushort m_usTopRow;

	[BiffRecordPos(4, 2)]
	private ushort m_usLeftCol;

	[BiffRecordPos(6, 4, true)]
	private int m_iHeaderColor = 64;

	private ushort m_usPageBreakZoom;

	private ushort m_usNormalZoom;

	private int m_iReserved;

	private int m_iOriginalLength;

	public ushort TopRow
	{
		get
		{
			return m_usTopRow;
		}
		set
		{
			m_usTopRow = value;
		}
	}

	public ushort LeftColumn
	{
		get
		{
			return m_usLeftCol;
		}
		set
		{
			m_usLeftCol = value;
		}
	}

	public int HeaderColor
	{
		get
		{
			return m_iHeaderColor;
		}
		set
		{
			m_iHeaderColor = value;
		}
	}

	public bool IsDisplayFormulas
	{
		get
		{
			return (m_options & OptionFlags.DisplayFormulas) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DisplayFormulas;
			}
			else
			{
				m_options &= ~OptionFlags.DisplayFormulas;
			}
		}
	}

	public bool IsDisplayGridlines
	{
		get
		{
			return (m_options & OptionFlags.DisplayGridlines) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DisplayGridlines;
			}
			else
			{
				m_options &= ~OptionFlags.DisplayGridlines;
			}
		}
	}

	public bool IsDisplayRowColHeadings
	{
		get
		{
			return (m_options & OptionFlags.DisplayRowColHeadings) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DisplayRowColHeadings;
			}
			else
			{
				m_options &= ~OptionFlags.DisplayRowColHeadings;
			}
		}
	}

	public bool IsFreezePanes
	{
		get
		{
			return (m_options & OptionFlags.FreezePanes) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.FreezePanes;
			}
			else
			{
				m_options &= ~OptionFlags.FreezePanes;
			}
		}
	}

	public bool IsDisplayZeros
	{
		get
		{
			return (m_options & OptionFlags.DisplayZeros) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DisplayZeros;
			}
			else
			{
				m_options &= ~OptionFlags.DisplayZeros;
			}
		}
	}

	public bool IsDefaultHeader
	{
		get
		{
			return (m_options & OptionFlags.DefaultHeader) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DefaultHeader;
			}
			else
			{
				m_options &= ~OptionFlags.DefaultHeader;
			}
		}
	}

	public bool IsArabic
	{
		get
		{
			return (m_options & OptionFlags.Arabic) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.Arabic;
			}
			else
			{
				m_options &= ~OptionFlags.Arabic;
			}
		}
	}

	public bool IsDisplayGuts
	{
		get
		{
			return (m_options & OptionFlags.DisplayGuts) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.DisplayGuts;
			}
			else
			{
				m_options &= ~OptionFlags.DisplayGuts;
			}
		}
	}

	public bool IsFreezePanesNoSplit
	{
		get
		{
			return (m_options & OptionFlags.FreezePanesNoSplit) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.FreezePanesNoSplit;
			}
			else
			{
				m_options &= ~OptionFlags.FreezePanesNoSplit;
			}
		}
	}

	public bool IsSelected
	{
		get
		{
			return (m_options & OptionFlags.Selected) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.Selected;
			}
			else
			{
				m_options &= ~OptionFlags.Selected;
			}
		}
	}

	public bool IsPaged
	{
		get
		{
			return (m_options & OptionFlags.Paged) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.Paged;
			}
			else
			{
				m_options &= ~OptionFlags.Paged;
			}
		}
	}

	public bool IsSavedInPageBreakPreview
	{
		get
		{
			return (m_options & OptionFlags.SavedInPageBreakPreview) != 0;
		}
		set
		{
			if (value)
			{
				m_options |= OptionFlags.SavedInPageBreakPreview;
			}
			else
			{
				m_options &= ~OptionFlags.SavedInPageBreakPreview;
			}
		}
	}

	public ushort Options => (ushort)m_options;

	public override int MinimumRecordSize => 10;

	public override int MaximumRecordSize => 18;

	internal int OriginalLength
	{
		get
		{
			return m_iOriginalLength;
		}
		set
		{
			m_iOriginalLength = value;
		}
	}

	public WindowTwoRecord()
	{
	}

	public WindowTwoRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public WindowTwoRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_options = (OptionFlags)provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usTopRow = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usLeftCol = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_iHeaderColor = provider.ReadInt32(iOffset);
		iOffset += 4;
		if (m_iLength > 10)
		{
			m_usPageBreakZoom = provider.ReadUInt16(iOffset);
			iOffset += 2;
			m_usNormalZoom = provider.ReadUInt16(iOffset);
			iOffset += 2;
		}
		if (m_iLength > 14)
		{
			m_iReserved = provider.ReadInt32(iOffset);
		}
		m_iOriginalLength = m_iLength;
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteUInt16(iOffset, (ushort)m_options);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usTopRow);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usLeftCol);
		iOffset += 2;
		provider.WriteInt32(iOffset, m_iHeaderColor);
		iOffset += 4;
		provider.WriteUInt16(iOffset, m_usPageBreakZoom);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usNormalZoom);
		iOffset += 2;
		provider.WriteInt32(iOffset, m_iReserved);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (m_iOriginalLength <= 0)
		{
			return 18;
		}
		return m_iOriginalLength;
	}
}
