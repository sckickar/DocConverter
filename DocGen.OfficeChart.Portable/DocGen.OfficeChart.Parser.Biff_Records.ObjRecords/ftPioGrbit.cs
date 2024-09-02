using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftPioGrbit : ObjSubRecord
{
	private const int DEF_RECORD_SIZE = 6;

	private bool m_bIsActiveX;

	private byte[] m_data;

	public bool IsActiveX => m_bIsActiveX;

	[CLSCompliant(false)]
	public ftPioGrbit(TObjSubRecordType type, ushort length, byte[] buffer)
		: base(type, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		if (base.Length == 0)
		{
			base.Length = 2;
			m_data = new byte[base.Length];
		}
		m_data = (byte[])buffer.Clone();
		m_bIsActiveX = BiffRecordRaw.GetBit(buffer, 0, 5);
	}

	public override void FillArray(DataProvider provider, int iOffset)
	{
		provider.WriteInt16(iOffset, (short)base.Type);
		iOffset += 2;
		provider.WriteInt16(iOffset, 2);
		iOffset += 2;
		provider.WriteBytes(iOffset, m_data, 0, m_data.Length);
	}

	public override object Clone()
	{
		ftPioGrbit obj = (ftPioGrbit)base.Clone();
		obj.m_data = CloneUtils.CloneByteArray(m_data);
		return obj;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 6;
	}
}
