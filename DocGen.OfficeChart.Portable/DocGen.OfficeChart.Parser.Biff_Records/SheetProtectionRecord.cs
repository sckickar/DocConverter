using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.SheetProtection)]
[CLSCompliant(false)]
internal class SheetProtectionRecord : BiffRecordRaw
{
	public const int ErrorIndicatorType = 3;

	private const int DEF_OPTION_OFFSET = 19;

	private const int DEF_STORE_SIZE = 23;

	private readonly byte[] DEF_EMBEDED_DATA = new byte[8] { 0, 2, 0, 1, 255, 255, 255, 255 };

	[BiffRecordPos(19, 2)]
	private ushort m_usOpt = 17408;

	private bool m_bIsContainProtection;

	private short m_sType;

	public int ProtectedOptions
	{
		get
		{
			return m_usOpt;
		}
		set
		{
			m_usOpt = (ushort)value;
		}
	}

	public bool ContainProtection
	{
		get
		{
			return m_bIsContainProtection;
		}
		set
		{
			m_bIsContainProtection = value;
		}
	}

	public override int MinimumRecordSize => 19;

	public override int MaximumRecordSize => 23;

	internal short Type
	{
		get
		{
			return m_sType;
		}
		set
		{
			m_sType = value;
		}
	}

	public SheetProtectionRecord()
	{
	}

	public SheetProtectionRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public SheetProtectionRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_bIsContainProtection = iLength > 19;
		m_sType = provider.ReadInt16(iOffset + 12);
		if (m_bIsContainProtection)
		{
			m_usOpt = (ushort)provider.ReadInt32(iOffset + 19);
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteBytes(iOffset, new byte[m_iLength]);
		provider.WriteUInt16(iOffset, 2151);
		if (m_sType == 3)
		{
			iOffset += 12;
			provider.WriteInt16(iOffset, m_sType);
			iOffset += 2;
			provider.WriteByte(iOffset, 1);
			iOffset++;
			provider.WriteInt32(iOffset, 0);
		}
		else
		{
			provider.WriteBytes(iOffset + 11, DEF_EMBEDED_DATA, 0, 8);
			provider.WriteUInt16(iOffset + 19, m_usOpt);
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		if (!m_bIsContainProtection)
		{
			return 19;
		}
		return 23;
	}
}
