using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftCblsData : ObjSubRecord
{
	private byte m_btCheckState;

	private bool m_threeD;

	public ExcelCheckState CheckState
	{
		get
		{
			return (ExcelCheckState)m_btCheckState;
		}
		set
		{
			m_btCheckState = (byte)value;
		}
	}

	public bool Display3DShading
	{
		get
		{
			return m_threeD;
		}
		set
		{
			m_threeD = value;
		}
	}

	public ftCblsData()
		: base(TObjSubRecordType.ftCblsData)
	{
	}

	public ftCblsData(TObjSubRecordType type, ushort length, byte[] buffer)
		: base(type, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		m_btCheckState = buffer[0];
		m_threeD = buffer[6] != 3;
	}

	public override void FillArray(DataProvider provider, int iOffset)
	{
		provider.WriteInt16(iOffset, (short)base.Type);
		iOffset += 2;
		short value = (short)(GetStoreSize(OfficeVersion.Excel97to2003) - 4);
		provider.WriteInt16(iOffset, value);
		iOffset += 2;
		provider.WriteByte(iOffset, m_btCheckState);
		iOffset++;
		provider.WriteInt32(iOffset, 0);
		iOffset += 4;
		provider.WriteByte(iOffset, 0);
		iOffset++;
		byte value2 = (byte)(m_threeD ? 2u : 3u);
		provider.WriteByte(iOffset, value2);
		iOffset++;
		provider.WriteByte(iOffset, 0);
		iOffset++;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 12;
	}
}
