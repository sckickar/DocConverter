using System;
using System.IO;
using System.Text;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Note)]
[CLSCompliant(false)]
internal class NoteRecord : BiffRecordRaw
{
	private const int DEF_FIXED_PART_SIZE = 10;

	[BiffRecordPos(0, 2)]
	private ushort m_usRow;

	[BiffRecordPos(2, 2)]
	private ushort m_usColumn;

	[BiffRecordPos(4, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(4, 1, TFieldType.Bit)]
	private bool m_bShow;

	[BiffRecordPos(6, 2)]
	private ushort m_usObjId;

	[BiffRecordPos(8, 2)]
	private ushort m_usAuthorNameLen;

	private string m_strAuthorName = string.Empty;

	public ushort Row
	{
		get
		{
			return m_usRow;
		}
		set
		{
			m_usRow = value;
		}
	}

	public ushort Column
	{
		get
		{
			return m_usColumn;
		}
		set
		{
			m_usColumn = value;
		}
	}

	public string AuthorName
	{
		get
		{
			return m_strAuthorName;
		}
		set
		{
			m_strAuthorName = value;
			m_usAuthorNameLen = (ushort)((value != null) ? ((ushort)m_strAuthorName.Length) : 0);
		}
	}

	public ushort ObjId
	{
		get
		{
			return m_usObjId;
		}
		set
		{
			m_usObjId = value;
		}
	}

	public bool IsVisible
	{
		get
		{
			return m_bShow;
		}
		set
		{
			m_bShow = value;
		}
	}

	public ushort Reserved => m_usOptions;

	public override int MinimumRecordSize => 8;

	public NoteRecord()
	{
		m_bShow = false;
	}

	public NoteRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public NoteRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usRow = provider.ReadUInt16(iOffset);
		m_usColumn = provider.ReadUInt16(iOffset + 2);
		m_usOptions = provider.ReadUInt16(iOffset + 4);
		m_bShow = provider.ReadBit(iOffset + 4, 1);
		m_usObjId = provider.ReadUInt16(iOffset + 6);
		m_usAuthorNameLen = provider.ReadUInt16(iOffset + 8);
		if (m_usAuthorNameLen > 0)
		{
			m_strAuthorName = provider.ReadString(10, m_usAuthorNameLen, out var _, isByteCounted: false);
		}
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		GetStoreSize(OfficeVersion.Excel97to2003);
		m_iLength = 0;
		int num = iOffset;
		provider.WriteUInt16(iOffset, m_usRow);
		provider.WriteUInt16(iOffset + 2, m_usColumn);
		provider.WriteUInt16(iOffset + 4, m_usOptions);
		provider.WriteBit(iOffset + 4, m_bShow, 1);
		provider.WriteUInt16(iOffset + 6, m_usObjId);
		provider.WriteUInt16(iOffset + 8, m_usAuthorNameLen);
		iOffset += 10;
		if (m_usAuthorNameLen > 0)
		{
			provider.WriteStringNoLenUpdateOffset(ref iOffset, m_strAuthorName);
			m_iLength = iOffset - num;
			if (m_iLength % 2 != 0)
			{
				provider.WriteByte(iOffset, 0);
				m_iLength++;
			}
		}
		else
		{
			provider.WriteByte(iOffset++, 0);
			provider.WriteByte(iOffset++, 0);
			m_iLength = iOffset - num;
		}
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		int num;
		if (m_usAuthorNameLen > 0)
		{
			num = Encoding.Unicode.GetByteCount(m_strAuthorName) + 1;
			if (num % 2 != 0)
			{
				num++;
			}
		}
		else
		{
			num = 2;
		}
		return 10 + num;
	}
}
