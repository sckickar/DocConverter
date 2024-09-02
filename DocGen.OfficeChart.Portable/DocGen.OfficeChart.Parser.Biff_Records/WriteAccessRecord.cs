using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.WriteAccess)]
[CLSCompliant(false)]
internal class WriteAccessRecord : BiffRecordRaw
{
	private const string DEF_USER_NAME = "User";

	private const int DEF_MIN_SIZE = 112;

	private const int DEF_MAX_SIZE = 112;

	private const byte DEF_SPACE = 32;

	private string m_strUserName = "User";

	public string UserName
	{
		get
		{
			return m_strUserName;
		}
		set
		{
			m_strUserName = value;
		}
	}

	public override int MinimumRecordSize => 112;

	public override int MaximumRecordSize => 112;

	public override bool IsAllowShortData => true;

	public WriteAccessRecord()
	{
	}

	public WriteAccessRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public WriteAccessRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		if ((long)provider.ReadUInt16(iOffset) < (long)iLength)
		{
			m_strUserName = provider.ReadString16Bit(iOffset, out var _);
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = iOffset;
		if (m_strUserName == null)
		{
			m_strUserName = "User";
		}
		provider.WriteUInt16(iOffset, (ushort)m_strUserName.Length);
		iOffset += 2;
		provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strUserName, bUnicode: false);
		if (iOffset - m_iLength < 112)
		{
			int num = 0;
			int num2 = m_iLength - iOffset;
			while (num < num2)
			{
				provider.WriteByte(iOffset, 32);
				num++;
				iOffset++;
			}
		}
		m_iLength = 112;
	}
}
