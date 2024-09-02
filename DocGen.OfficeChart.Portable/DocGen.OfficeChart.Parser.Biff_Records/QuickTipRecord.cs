using System;
using System.IO;
using System.Text;
using DocGen.OfficeChart.Implementation.Exceptions;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.QuickTip)]
[CLSCompliant(false)]
internal class QuickTipRecord : BiffRecordRawWithArray
{
	private const int DEF_FIXED_PART_SIZE = 10;

	[BiffRecordPos(0, 2)]
	private ushort m_usRecordId = 2048;

	private TAddr m_addrCellRange;

	private string m_strToolTip = string.Empty;

	public TAddr CellRange
	{
		get
		{
			return m_addrCellRange;
		}
		set
		{
			m_addrCellRange = value;
		}
	}

	public string ToolTip
	{
		get
		{
			return m_strToolTip;
		}
		set
		{
			m_strToolTip = ((value[value.Length - 1] != 0) ? (value + "\0") : value);
		}
	}

	public override int MinimumRecordSize => 10;

	public QuickTipRecord()
	{
	}

	public QuickTipRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public QuickTipRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		m_usRecordId = GetUInt16(0);
		if (m_usRecordId != 2048)
		{
			throw new WrongBiffRecordDataException("QuickTip first word must be 0x0800.");
		}
		if (m_iLength % 2 != 0)
		{
			throw new WrongBiffRecordDataException();
		}
		m_addrCellRange = GetAddr(2);
		m_strToolTip = Encoding.Unicode.GetString(m_data, 10, m_iLength - 10);
		int num = m_strToolTip.IndexOf('\0');
		if (num != m_strToolTip.Length - 1)
		{
			throw new WrongBiffRecordDataException("Zero-terminated string does not fit data array.");
		}
		m_strToolTip = m_strToolTip.Remove(num, 1);
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_data = new byte[GetStoreSize(OfficeVersion.Excel97to2003)];
		SetUInt16(0, m_usRecordId);
		m_iLength = 2;
		SetAddr(m_iLength, m_addrCellRange);
		m_iLength += 8;
		byte[] bytes = Encoding.Unicode.GetBytes(m_strToolTip);
		int num = bytes.Length;
		SetBytes(m_iLength, bytes);
		m_iLength += num;
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 10 + Encoding.Unicode.GetByteCount(m_strToolTip);
	}
}
