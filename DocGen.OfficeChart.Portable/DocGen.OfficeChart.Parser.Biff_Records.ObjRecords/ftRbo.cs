using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftRbo : ObjSubRecord
{
	public ftRbo()
		: base(TObjSubRecordType.ftRbo)
	{
	}

	public ftRbo(ushort length, byte[] buffer)
		: base(TObjSubRecordType.ftRbo, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
	}

	public override void FillArray(DataProvider provider, int iOffset)
	{
		provider.WriteInt16(iOffset, (short)base.Type);
		iOffset += 2;
		short value = (short)(GetStoreSize(OfficeVersion.Excel97to2003) - 4);
		provider.WriteInt16(iOffset, value);
		iOffset += 2;
		provider.WriteByte(iOffset, 0);
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
		return 10;
	}
}
