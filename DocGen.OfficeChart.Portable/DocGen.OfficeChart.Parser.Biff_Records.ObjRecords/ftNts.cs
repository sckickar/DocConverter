using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftNts : ObjSubRecord
{
	private const int DEF_RECORD_SIZE = 26;

	private byte[] m_data;

	public byte[] Data => m_data;

	[CLSCompliant(false)]
	public ftNts(TObjSubRecordType type, ushort length, byte[] buffer)
		: base(type, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		m_data = (byte[])buffer.Clone();
	}

	public override void FillArray(DataProvider provider, int iOffset)
	{
		provider.WriteInt16(iOffset, (short)base.Type);
		iOffset += 2;
		provider.WriteInt16(iOffset, 22);
		iOffset += 2;
		provider.WriteBytes(iOffset, m_data, 0, m_data.Length);
	}

	public override object Clone()
	{
		ftNts obj = (ftNts)base.Clone();
		obj.m_data = CloneUtils.CloneByteArray(m_data);
		return obj;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 26;
	}
}
