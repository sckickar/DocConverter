using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftCbls : ObjSubRecord
{
	private byte m_btChecked;

	public ExcelCheckState CheckState
	{
		get
		{
			return (ExcelCheckState)m_btChecked;
		}
		set
		{
			m_btChecked = (byte)value;
		}
	}

	public ftCbls()
		: base(TObjSubRecordType.ftCbls)
	{
	}

	public ftCbls(TObjSubRecordType type, ushort length, byte[] buffer)
		: base(type, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		m_btChecked = buffer[0];
	}

	public override void FillArray(DataProvider provider, int iOffset)
	{
		provider.WriteInt16(iOffset, (short)base.Type);
		iOffset += 2;
		short value = (short)(GetStoreSize(OfficeVersion.Excel97to2003) - 4);
		provider.WriteInt16(iOffset, value);
		iOffset += 2;
		provider.WriteByte(iOffset, m_btChecked);
		iOffset++;
		provider.WriteInt32(iOffset, 0);
		iOffset += 4;
		provider.WriteInt32(iOffset, 0);
		iOffset += 4;
		provider.WriteByte(iOffset, 0);
		iOffset++;
		provider.WriteByte(iOffset, 3);
		iOffset++;
		provider.WriteByte(iOffset, 0);
		iOffset++;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 16;
	}
}
