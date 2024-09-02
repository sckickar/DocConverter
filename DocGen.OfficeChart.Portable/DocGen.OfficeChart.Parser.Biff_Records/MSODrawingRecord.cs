using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.MSODrawing)]
[CLSCompliant(false)]
internal class MSODrawingRecord : BiffRecordRawWithArray, ICloneable, ILengthSetter
{
	public int RecordLength
	{
		get
		{
			return base.Length;
		}
		set
		{
			if (value < 0 && value > m_data.Length)
			{
				throw new ArgumentOutOfRangeException("RecordLength");
			}
			m_iLength = value;
		}
	}

	public override bool NeedDataArray => true;

	public MSODrawingRecord()
	{
	}

	public MSODrawingRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public MSODrawingRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_iLength = m_data.Length;
	}

	public void SetData(int length, byte[] data)
	{
		if (length < 0 || data.Length < length)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		m_data = new byte[length];
		Array.Copy(data, 0, m_data, 0, length);
	}

	public void SetLength(int iLength)
	{
		m_iLength = iLength;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return m_data.Length;
	}
}
