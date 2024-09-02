using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartMarkerFormat)]
[CLSCompliant(false)]
internal class ChartMarkerFormatRecord : BiffRecordRaw
{
	public enum TMarker
	{
		NoMarker,
		Square,
		Diamond,
		Triangle,
		X,
		Star,
		DowJones,
		StandardDeviation,
		Circle,
		PlusSign
	}

	public const int DEF_RECORD_SIZE = 20;

	[BiffRecordPos(0, 4, true)]
	private int m_iForeColor;

	[BiffRecordPos(4, 4, true)]
	private int m_iBackColor;

	[BiffRecordPos(8, 2)]
	private ushort m_usMarkerType = 1;

	[BiffRecordPos(10, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(12, 2)]
	private ushort m_usBorderColorIndex;

	[BiffRecordPos(14, 2)]
	private ushort m_usFillColorIndex;

	[BiffRecordPos(16, 4, true)]
	private int m_iLineSize = 100;

	[BiffRecordPos(10, 0, TFieldType.Bit)]
	private bool m_bAutoColor = true;

	[BiffRecordPos(10, 4, TFieldType.Bit)]
	private bool m_bNotShowInt;

	[BiffRecordPos(10, 5, TFieldType.Bit)]
	private bool m_bNotShowBrd;

	private byte m_flagOptions;

	public int ForeColor
	{
		get
		{
			return m_iForeColor;
		}
		set
		{
			m_iForeColor = value;
			m_flagOptions |= 1;
		}
	}

	public int BackColor
	{
		get
		{
			return m_iBackColor;
		}
		set
		{
			m_iBackColor = value;
		}
	}

	public OfficeChartMarkerType MarkerType
	{
		get
		{
			return (OfficeChartMarkerType)m_usMarkerType;
		}
		set
		{
			m_usMarkerType = (ushort)value;
			m_flagOptions |= 16;
		}
	}

	public ushort Options => m_usOptions;

	public ushort BorderColorIndex
	{
		get
		{
			return m_usBorderColorIndex;
		}
		set
		{
			m_usBorderColorIndex = value;
		}
	}

	public ushort FillColorIndex
	{
		get
		{
			return m_usFillColorIndex;
		}
		set
		{
			m_usFillColorIndex = value;
			m_flagOptions |= 1;
		}
	}

	public int LineSize
	{
		get
		{
			return m_iLineSize;
		}
		set
		{
			m_iLineSize = value;
			m_flagOptions |= 32;
		}
	}

	public bool IsAutoColor
	{
		get
		{
			return m_bAutoColor;
		}
		set
		{
			m_bAutoColor = value;
			if (value)
			{
				m_flagOptions = 0;
			}
		}
	}

	public bool IsNotShowInt
	{
		get
		{
			return m_bNotShowInt;
		}
		set
		{
			m_bNotShowInt = value;
			m_flagOptions |= 4;
		}
	}

	public bool IsNotShowBrd
	{
		get
		{
			return m_bNotShowBrd;
		}
		set
		{
			m_bNotShowBrd = value;
			m_flagOptions |= 8;
		}
	}

	public override int MinimumRecordSize => 20;

	public override int MaximumRecordSize => 20;

	internal bool HasLineProperties => (m_flagOptions & 2) != 0;

	internal byte FlagOptions
	{
		get
		{
			return m_flagOptions;
		}
		set
		{
			m_flagOptions = value;
		}
	}

	public ChartMarkerFormatRecord()
	{
	}

	public ChartMarkerFormatRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartMarkerFormatRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iForeColor = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_iBackColor = provider.ReadInt32(iOffset);
		iOffset += 4;
		m_usMarkerType = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bAutoColor = provider.ReadBit(iOffset, 0);
		m_bNotShowInt = provider.ReadBit(iOffset, 4);
		m_bNotShowBrd = provider.ReadBit(iOffset, 5);
		iOffset += 2;
		m_usBorderColorIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usFillColorIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_iLineSize = provider.ReadInt32(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteInt32(iOffset, m_iForeColor);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iBackColor);
		iOffset += 4;
		provider.WriteUInt16(iOffset, m_usMarkerType);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bAutoColor, 0);
		provider.WriteBit(iOffset, m_bNotShowInt, 4);
		provider.WriteBit(iOffset, m_bNotShowBrd, 5);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usBorderColorIndex);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usFillColorIndex);
		iOffset += 2;
		provider.WriteInt32(iOffset, m_iLineSize);
	}
}
