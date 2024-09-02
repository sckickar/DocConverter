using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

[CLSCompliant(false)]
internal class ftEnd : ObjSubRecord
{
	private const int DEF_RECORD_SIZE = 4;

	public ftEnd()
		: base(TObjSubRecordType.ftEnd)
	{
	}

	public ftEnd(TObjSubRecordType type, ushort length, byte[] buffer)
		: base(type, length, buffer)
	{
	}

	protected override void Parse(byte[] buffer)
	{
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 4;
	}
}
