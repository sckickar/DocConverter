using System;

namespace DocGen.OfficeChart.Parser.Biff_Records.ObjRecords;

internal abstract class ObjSubRecord : ICloneable
{
	protected const int HeaderSize = 4;

	private TObjSubRecordType m_Type;

	private ushort m_usLength;

	public TObjSubRecordType Type => m_Type;

	[CLSCompliant(false)]
	public ushort Length
	{
		get
		{
			return m_usLength;
		}
		protected set
		{
			m_usLength = value;
		}
	}

	private ObjSubRecord()
	{
	}

	protected ObjSubRecord(TObjSubRecordType type)
	{
		m_Type = type;
	}

	[CLSCompliant(false)]
	protected ObjSubRecord(TObjSubRecordType type, ushort length, byte[] buffer)
	{
		m_Type = type;
		m_usLength = length;
		Parse(buffer);
	}

	protected abstract void Parse(byte[] buffer);

	public virtual void FillArray(DataProvider provider, int iOffset)
	{
		provider.WriteInt16(iOffset, (short)Type);
		iOffset += 2;
		ushort value = (ushort)(GetStoreSize(OfficeVersion.Excel97to2003) - 4);
		provider.WriteUInt16(iOffset, value);
		iOffset += 2;
		Serialize(provider, iOffset);
	}

	protected virtual void Serialize(DataProvider provider, int iOffset)
	{
	}

	public abstract int GetStoreSize(OfficeVersion version);

	public virtual object Clone()
	{
		return MemberwiseClone();
	}
}
