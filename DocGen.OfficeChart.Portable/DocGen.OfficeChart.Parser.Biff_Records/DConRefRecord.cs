using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.DCONRef)]
[CLSCompliant(false)]
internal class DConRefRecord : BiffRecordRaw
{
	[BiffRecordPos(0, 2)]
	private ushort m_usFirstRow;

	[BiffRecordPos(2, 2)]
	private ushort m_usLastRow;

	[BiffRecordPos(4, 1)]
	private byte m_btFirstColumn;

	[BiffRecordPos(5, 1)]
	private byte m_btLastColumn;

	[BiffRecordPos(6, TFieldType.String16Bit)]
	private string m_strWorkbookName;

	public ushort FirstRow
	{
		get
		{
			return m_usFirstRow;
		}
		set
		{
			m_usFirstRow = value;
		}
	}

	public ushort LastRow
	{
		get
		{
			return m_usLastRow;
		}
		set
		{
			m_usLastRow = value;
		}
	}

	public byte FirstColumn
	{
		get
		{
			return m_btFirstColumn;
		}
		set
		{
			m_btFirstColumn = value;
		}
	}

	public byte LastColumn
	{
		get
		{
			return m_btLastColumn;
		}
		set
		{
			m_btLastColumn = value;
		}
	}

	public string WorkbookName
	{
		get
		{
			return m_strWorkbookName;
		}
		set
		{
			m_strWorkbookName = value;
		}
	}

	public DConRefRecord()
	{
	}

	public DConRefRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public DConRefRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usFirstRow = provider.ReadUInt16(iOffset);
		m_usLastRow = provider.ReadUInt16(iOffset + 2);
		m_btFirstColumn = provider.ReadByte(iOffset + 4);
		m_btLastColumn = provider.ReadByte(iOffset + 5);
		m_strWorkbookName = provider.ReadString16Bit(iOffset + 6, out var _);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usFirstRow);
		provider.WriteUInt16(iOffset + 2, m_usLastRow);
		provider.WriteByte(iOffset + 4, m_btFirstColumn);
		provider.WriteByte(iOffset + 5, m_btLastColumn);
		provider.WriteString16Bit(iOffset + 6, m_strWorkbookName);
		m_iLength = GetStoreSize(version);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 9 + m_strWorkbookName.Length * 2;
	}
}
