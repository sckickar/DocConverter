using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartBoppCustom)]
[CLSCompliant(false)]
internal class ChartBoppCustomRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usQuantity;

	private byte[] m_bits;

	public ushort Counter => m_usQuantity;

	public byte[] BitFields
	{
		get
		{
			return m_bits;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_bits = value;
			m_usQuantity = (ushort)value.Length;
		}
	}

	public ChartBoppCustomRecord()
	{
	}

	public ChartBoppCustomRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartBoppCustomRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usQuantity = provider.ReadUInt16(iOffset);
		m_bits = new byte[m_usQuantity];
		provider.ReadArray(iOffset + 2, m_bits);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usQuantity);
		provider.WriteBytes(iOffset + 2, m_bits, 0, m_bits.Length);
		m_iLength = m_bits.Length + 2;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 2 + m_usQuantity;
	}

	public override object Clone()
	{
		ChartBoppCustomRecord obj = (ChartBoppCustomRecord)base.Clone();
		obj.m_bits = CloneUtils.CloneByteArray(m_bits);
		return obj;
	}
}
