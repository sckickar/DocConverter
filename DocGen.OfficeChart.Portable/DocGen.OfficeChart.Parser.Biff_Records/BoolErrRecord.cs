using System;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.BoolErr)]
[CLSCompliant(false)]
internal class BoolErrRecord : CellPositionBase, IValueHolder
{
	private const int DEF_RECORD_SIZE = 8;

	[BiffRecordPos(6, 1)]
	private byte m_BoolOrError;

	[BiffRecordPos(7, 1)]
	private byte m_IsErrorCode;

	public byte BoolOrError
	{
		get
		{
			return m_BoolOrError;
		}
		set
		{
			m_BoolOrError = value;
		}
	}

	public bool IsErrorCode
	{
		get
		{
			return m_IsErrorCode == 1;
		}
		set
		{
			m_IsErrorCode = (value ? ((byte)1) : ((byte)0));
		}
	}

	public override int MinimumRecordSize => 8;

	public override int MaximumRecordSize => 8;

	public object Value
	{
		get
		{
			if (!IsErrorCode)
			{
				return BoolOrError != 0;
			}
			return BoolOrError;
		}
		set
		{
			if (value is bool)
			{
				IsErrorCode = false;
				BoolOrError = (byte)value;
			}
			else
			{
				IsErrorCode = true;
				BoolOrError = (byte)value;
			}
		}
	}

	protected override void ParseCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_BoolOrError = provider.ReadByte(iOffset);
		m_IsErrorCode = provider.ReadByte(iOffset + 1);
	}

	protected override void InfillCellData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteByte(iOffset, m_BoolOrError);
		provider.WriteByte(iOffset + 1, m_IsErrorCode);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num = 8;
		if (version != 0)
		{
			num += 4;
		}
		return num;
	}

	public static int ReadValue(DataProvider provider, int recordStart, OfficeVersion version)
	{
		recordStart += 10;
		if (version != 0)
		{
			recordStart += 4;
		}
		return provider.ReadInt16(recordStart);
	}
}
