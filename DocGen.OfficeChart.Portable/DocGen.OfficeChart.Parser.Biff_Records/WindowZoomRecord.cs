using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.WindowZoom)]
[CLSCompliant(false)]
internal class WindowZoomRecord : BiffRecordRaw
{
	private const int DEF_RECORD_SIZE = 4;

	[BiffRecordPos(0, 2)]
	private ushort m_usNscl = 100;

	[BiffRecordPos(2, 2)]
	private ushort m_usDscl = 100;

	public ushort NumMagnification
	{
		get
		{
			return m_usNscl;
		}
		set
		{
			m_usNscl = value;
		}
	}

	public ushort DenumMagnification
	{
		get
		{
			return m_usDscl;
		}
		set
		{
			m_usDscl = value;
		}
	}

	public int Zoom
	{
		get
		{
			return (int)((double)(int)m_usNscl * 100.0 / (double)(int)m_usDscl);
		}
		set
		{
			if (value < 10 || value > 400)
			{
				throw new ArgumentOutOfRangeException("Zoom", "Zoom must be in range from 10 and 400.");
			}
			m_usNscl = (ushort)value;
			m_usDscl = 100;
		}
	}

	public WindowZoomRecord()
	{
	}

	public WindowZoomRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public WindowZoomRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usNscl = provider.ReadUInt16(iOffset);
		m_usDscl = provider.ReadUInt16(iOffset + 2);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 4;
		provider.WriteUInt16(iOffset, m_usNscl);
		provider.WriteUInt16(iOffset + 2, m_usDscl);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 4;
	}
}
