using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.BOF)]
[CLSCompliant(false)]
internal class BOFRecord : BiffRecordRaw
{
	public enum TType
	{
		TYPE_WORKBOOK = 5,
		TYPE_VB_MODULE = 6,
		TYPE_WORKSHEET = 16,
		TYPE_CHART = 32,
		TYPE_EXCEL_4_MACRO = 64,
		TYPE_WORKSPACE_FILE = 256
	}

	private const int DEF_RECORD_SIZE = 16;

	[BiffRecordPos(0, 2)]
	private ushort m_usVersion = 1536;

	[BiffRecordPos(2, 2)]
	private ushort m_usType = 5;

	[BiffRecordPos(4, 2)]
	private ushort m_usBuild = 6214;

	[BiffRecordPos(6, 2)]
	private ushort m_usYear = 1997;

	[BiffRecordPos(8, 4, true)]
	private int m_iHistory = 98497;

	[BiffRecordPos(12, 4, true)]
	private int m_iRVersion = 1542;

	private bool m_bIsNested;

	public ushort Version
	{
		get
		{
			return m_usVersion;
		}
		set
		{
			m_usVersion = value;
		}
	}

	public TType Type
	{
		get
		{
			return (TType)m_usType;
		}
		set
		{
			m_usType = (ushort)value;
		}
	}

	public ushort Build
	{
		get
		{
			return m_usBuild;
		}
		set
		{
			m_usBuild = value;
		}
	}

	public ushort Year
	{
		get
		{
			return m_usYear;
		}
		set
		{
			m_usYear = value;
		}
	}

	public int History
	{
		get
		{
			return m_iHistory;
		}
		set
		{
			m_iHistory = value;
		}
	}

	public int RequeredVersion
	{
		get
		{
			return m_iRVersion;
		}
		set
		{
			m_iRVersion = value;
		}
	}

	public override int MinimumRecordSize => 16;

	public override int MaximumRecordSize => 16;

	public bool IsNested
	{
		get
		{
			return m_bIsNested;
		}
		set
		{
			m_bIsNested = value;
		}
	}

	public override bool IsAllowShortData => true;

	public override bool NeedDecoding => false;

	public BOFRecord()
	{
	}

	public BOFRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public BOFRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usVersion = provider.ReadUInt16(iOffset);
		m_usType = provider.ReadUInt16(iOffset + 2);
		m_usBuild = provider.ReadUInt16(iOffset + 4);
		m_usYear = provider.ReadUInt16(iOffset + 6);
		m_iHistory = provider.ReadInt32(iOffset + 8);
		m_iRVersion = provider.ReadInt32(iOffset + 12);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 16;
		provider.WriteUInt16(iOffset, m_usVersion);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usType);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usBuild);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usYear);
		iOffset += 2;
		provider.WriteInt32(iOffset, m_iHistory);
		iOffset += 4;
		provider.WriteInt32(iOffset, m_iRVersion);
		iOffset += 4;
	}
}
