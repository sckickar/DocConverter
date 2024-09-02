using System;
using System.IO;
using System.Text;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.BoundSheet)]
[CLSCompliant(false)]
internal class BoundSheetRecord : BiffRecordRaw
{
	public enum SheetType
	{
		Worksheet = 0,
		Chart = 2,
		VisualBasicModule = 6
	}

	[BiffRecordPos(0, 4, true)]
	private int m_iBOFPosition = 1394;

	[BiffRecordPos(4, 1, true)]
	private byte m_Visibility;

	[BiffRecordPos(5, 1, true)]
	private byte m_SheetType;

	[BiffRecordPos(6, TFieldType.String)]
	private string m_strSheetName = "Sheet1";

	private int m_iSheetIndex = -1;

	private BOFRecord m_bof;

	public int BOFPosition
	{
		get
		{
			return m_iBOFPosition;
		}
		set
		{
			m_iBOFPosition = value;
		}
	}

	public string SheetName
	{
		get
		{
			return m_strSheetName;
		}
		set
		{
			m_strSheetName = value;
		}
	}

	public int SheetIndex
	{
		get
		{
			return m_iSheetIndex;
		}
		set
		{
			m_iSheetIndex = value;
		}
	}

	public SheetType BoundSheetType
	{
		get
		{
			return (SheetType)m_SheetType;
		}
		set
		{
			m_SheetType = (byte)value;
		}
	}

	public OfficeWorksheetVisibility Visibility
	{
		get
		{
			return (OfficeWorksheetVisibility)m_Visibility;
		}
		set
		{
			m_Visibility = (byte)value;
		}
	}

	public override int MinimumRecordSize => 8;

	public BOFRecord BOF
	{
		get
		{
			return m_bof;
		}
		set
		{
			m_bof = value;
		}
	}

	public override int StartDecodingOffset => 4;

	public BoundSheetRecord()
	{
	}

	public BoundSheetRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public BoundSheetRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iBOFPosition = provider.ReadInt32(iOffset);
		m_Visibility = provider.ReadByte(iOffset + 4);
		m_SheetType = provider.ReadByte(iOffset + 5);
		m_strSheetName = provider.ReadString8Bit(iOffset + 6, out var _);
		InternalDataIntegrityCheck();
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		int num = iOffset;
		provider.WriteInt32(iOffset, m_iBOFPosition);
		provider.WriteByte(iOffset + 4, m_Visibility);
		provider.WriteByte(iOffset + 5, m_SheetType);
		iOffset += 6;
		provider.WriteString8BitUpdateOffset(ref iOffset, m_strSheetName);
		m_iLength = iOffset - num;
	}

	private void InternalDataIntegrityCheck()
	{
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 8 + Encoding.Unicode.GetByteCount(m_strSheetName);
	}
}
