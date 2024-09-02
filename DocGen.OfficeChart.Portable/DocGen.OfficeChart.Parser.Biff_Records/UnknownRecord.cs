using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Unknown)]
[CLSCompliant(false)]
internal class UnknownRecord : BiffRecordRawWithArray, ICloneable
{
	private static UnknownRecord _empty = new UnknownRecord();

	private byte[] m_tempData;

	public static BiffRecordRaw Empty => _empty;

	public override bool NeedDataArray => true;

	public new int RecordCode
	{
		get
		{
			return base.RecordCode;
		}
		set
		{
			m_iCode = value;
		}
	}

	public int DataLen
	{
		get
		{
			return m_iLength;
		}
		set
		{
			m_iLength = value;
		}
	}

	public UnknownRecord()
	{
	}

	public UnknownRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public UnknownRecord(BinaryReader reader, out int itemSize)
	{
		m_iCode = reader.ReadInt16();
		m_iLength = reader.ReadInt16();
		m_data = new byte[m_iLength];
		reader.BaseStream.Read(m_data, 0, m_iLength);
		itemSize = m_iLength;
	}

	public UnknownRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		m_tempData = new byte[m_data.Length];
		m_data.CopyTo(m_tempData, 0);
	}

	public override void InfillInternalData(OfficeVersion version)
	{
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return m_iLength;
	}

	public new object Clone()
	{
		UnknownRecord unknownRecord = (UnknownRecord)base.Clone();
		if (m_tempData != null)
		{
			unknownRecord.m_tempData = CloneUtils.CloneByteArray(m_tempData);
		}
		return unknownRecord;
	}
}
