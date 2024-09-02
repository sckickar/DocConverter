using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartValueRange)]
[CLSCompliant(false)]
internal class ChartValueRangeRecord : BiffRecordRaw, IMaxCross
{
	public const int DEF_RECORD_SIZE = 42;

	[BiffRecordPos(0, 8, TFieldType.Float)]
	private double m_dNumMin;

	[BiffRecordPos(8, 8, TFieldType.Float)]
	private double m_dNumMax;

	[BiffRecordPos(16, 8, TFieldType.Float)]
	private double m_dNumMajor;

	[BiffRecordPos(24, 8, TFieldType.Float)]
	private double m_dNumMinor;

	[BiffRecordPos(32, 8, TFieldType.Float)]
	private double m_dNumCross;

	[BiffRecordPos(40, 2)]
	private ushort m_usFormatFlags;

	[BiffRecordPos(40, 0, TFieldType.Bit)]
	private bool m_bAutoMin = true;

	[BiffRecordPos(40, 1, TFieldType.Bit)]
	private bool m_bAutoMax = true;

	[BiffRecordPos(40, 2, TFieldType.Bit)]
	private bool m_bAutoMajor = true;

	[BiffRecordPos(40, 3, TFieldType.Bit)]
	private bool m_bAutoMinor = true;

	[BiffRecordPos(40, 4, TFieldType.Bit)]
	private bool m_bAutoCross = true;

	[BiffRecordPos(40, 5, TFieldType.Bit)]
	private bool m_bLogScale;

	[BiffRecordPos(40, 6, TFieldType.Bit)]
	private bool m_bReverse;

	[BiffRecordPos(40, 7, TFieldType.Bit)]
	private bool m_bMaxCross;

	public double NumMin
	{
		get
		{
			return m_dNumMin;
		}
		set
		{
			m_dNumMin = value;
		}
	}

	public double NumMax
	{
		get
		{
			return m_dNumMax;
		}
		set
		{
			m_dNumMax = value;
		}
	}

	public double NumMajor
	{
		get
		{
			return m_dNumMajor;
		}
		set
		{
			m_dNumMajor = value;
		}
	}

	public double NumMinor
	{
		get
		{
			return m_dNumMinor;
		}
		set
		{
			m_dNumMinor = value;
		}
	}

	public double NumCross
	{
		get
		{
			return m_dNumCross;
		}
		set
		{
			m_dNumCross = value;
		}
	}

	public ushort FormatFlags => m_usFormatFlags;

	public bool IsAutoMin
	{
		get
		{
			return m_bAutoMin;
		}
		set
		{
			m_bAutoMin = value;
		}
	}

	public bool IsAutoMax
	{
		get
		{
			return m_bAutoMax;
		}
		set
		{
			m_bAutoMax = value;
		}
	}

	public bool IsAutoMajor
	{
		get
		{
			return m_bAutoMajor;
		}
		set
		{
			m_bAutoMajor = value;
		}
	}

	public bool IsAutoMinor
	{
		get
		{
			return m_bAutoMinor;
		}
		set
		{
			m_bAutoMinor = value;
		}
	}

	public bool IsAutoCross
	{
		get
		{
			return m_bAutoCross;
		}
		set
		{
			m_bAutoCross = value;
		}
	}

	public bool IsLogScale
	{
		get
		{
			return m_bLogScale;
		}
		set
		{
			m_bLogScale = value;
		}
	}

	public bool IsReverse
	{
		get
		{
			return m_bReverse;
		}
		set
		{
			m_bReverse = value;
		}
	}

	public bool IsMaxCross
	{
		get
		{
			return m_bMaxCross;
		}
		set
		{
			m_bMaxCross = value;
		}
	}

	public override int MinimumRecordSize => 42;

	public override int MaximumRecordSize => 42;

	public ChartValueRangeRecord()
	{
	}

	public ChartValueRangeRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartValueRangeRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_dNumMin = provider.ReadDouble(iOffset);
		iOffset += 8;
		m_dNumMax = provider.ReadDouble(iOffset);
		iOffset += 8;
		m_dNumMajor = provider.ReadDouble(iOffset);
		iOffset += 8;
		m_dNumMinor = provider.ReadDouble(iOffset);
		iOffset += 8;
		m_dNumCross = provider.ReadDouble(iOffset);
		iOffset += 8;
		m_usFormatFlags = provider.ReadUInt16(iOffset);
		m_bAutoMin = provider.ReadBit(iOffset, 0);
		m_bAutoMax = provider.ReadBit(iOffset, 1);
		m_bAutoMajor = provider.ReadBit(iOffset, 2);
		m_bAutoMinor = provider.ReadBit(iOffset, 3);
		m_bAutoCross = provider.ReadBit(iOffset, 4);
		m_bLogScale = provider.ReadBit(iOffset, 5);
		m_bReverse = provider.ReadBit(iOffset, 6);
		m_bMaxCross = provider.ReadBit(iOffset, 7);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteDouble(iOffset, m_dNumMin);
		iOffset += 8;
		provider.WriteDouble(iOffset, m_dNumMax);
		iOffset += 8;
		provider.WriteDouble(iOffset, m_dNumMajor);
		iOffset += 8;
		provider.WriteDouble(iOffset, m_dNumMinor);
		iOffset += 8;
		provider.WriteDouble(iOffset, m_dNumCross);
		iOffset += 8;
		provider.WriteUInt16(iOffset, m_usFormatFlags);
		provider.WriteBit(iOffset, m_bAutoMin, 0);
		provider.WriteBit(iOffset, m_bAutoMax, 1);
		provider.WriteBit(iOffset, m_bAutoMajor, 2);
		provider.WriteBit(iOffset, m_bAutoMinor, 3);
		provider.WriteBit(iOffset, m_bAutoCross, 4);
		provider.WriteBit(iOffset, m_bLogScale, 5);
		provider.WriteBit(iOffset, m_bReverse, 6);
		provider.WriteBit(iOffset, m_bMaxCross, 7);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 42;
	}
}
