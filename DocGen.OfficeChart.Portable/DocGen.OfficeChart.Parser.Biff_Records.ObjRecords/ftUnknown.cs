using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftUnknown : ObjSubRecord
{
	private byte[] m_data;

	public byte[] RecordData => m_data;

	[CLSCompliant(false)]
	public ftUnknown(TObjSubRecordType type, ushort length, byte[] buffer)
		: base(type, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		m_data = new byte[base.Length];
		Array.Copy(buffer, 0, m_data, 0, base.Length);
	}

	public override void FillArray(DataProvider provider, int iOffset)
	{
		provider.WriteInt16(iOffset, (short)base.Type);
		iOffset += 2;
		provider.WriteInt16(iOffset, (short)base.Length);
		iOffset += 2;
		provider.WriteBytes(iOffset, m_data, 0, m_data.Length);
	}

	public override object Clone()
	{
		ftUnknown obj = (ftUnknown)base.Clone();
		obj.m_data = CloneUtils.CloneByteArray(m_data);
		return obj;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return base.Length + 4;
	}
}
