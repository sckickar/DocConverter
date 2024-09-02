using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.TextObject)]
[CLSCompliant(false)]
internal class TextObjectRecord : BiffRecordRawWithArray
{
	public const ushort HAlignmentBitMask = 14;

	public const ushort VAlignmentBitMask = 112;

	[BiffRecordPos(0, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(1, 2, TFieldType.Bit)]
	private bool m_bLockText;

	[BiffRecordPos(2, 2)]
	private ushort m_usRotation;

	[BiffRecordPos(4, 4)]
	private uint m_uiReserved1;

	[BiffRecordPos(8, 2)]
	private ushort m_usReserved2;

	[BiffRecordPos(10, 2)]
	private ushort m_usTextLen;

	[BiffRecordPos(12, 2)]
	private ushort m_usFormattingRunsLen;

	[BiffRecordPos(14, 4)]
	private uint m_uiReserved3;

	public OfficeCommentHAlign HAlignment
	{
		get
		{
			return (OfficeCommentHAlign)(BiffRecordRaw.GetUInt16BitsByMask(m_usOptions, 14) >> 1);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usOptions, 14, (ushort)((ushort)value << 1));
		}
	}

	public OfficeCommentVAlign VAlignment
	{
		get
		{
			return (OfficeCommentVAlign)(BiffRecordRaw.GetUInt16BitsByMask(m_usOptions, 112) >> 4);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usOptions, 112, (ushort)((ushort)value << 4));
		}
	}

	public bool IsLockText
	{
		get
		{
			return m_bLockText;
		}
		set
		{
			m_bLockText = value;
		}
	}

	public OfficeTextRotation Rotation
	{
		get
		{
			return (OfficeTextRotation)m_usRotation;
		}
		set
		{
			m_usRotation = (ushort)value;
		}
	}

	public ushort TextLen
	{
		get
		{
			return m_usTextLen;
		}
		set
		{
			m_usTextLen = value;
		}
	}

	public ushort FormattingRunsLen
	{
		get
		{
			return m_usFormattingRunsLen;
		}
		set
		{
			m_usFormattingRunsLen = value;
		}
	}

	public uint Reserved1 => m_uiReserved1;

	public ushort Reserved2 => m_usReserved2;

	public uint Reserved3 => m_uiReserved3;

	public override int MinimumRecordSize => 18;

	public override int MaximumRecordSize => 18;

	public TextObjectRecord()
	{
	}

	public TextObjectRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public TextObjectRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		m_usOptions = GetUInt16(0);
		m_bLockText = GetBit(1, 1);
		m_usRotation = GetUInt16(2);
		m_uiReserved1 = GetUInt32(4);
		m_usReserved2 = GetUInt16(8);
		m_usTextLen = GetUInt16(10);
		m_usFormattingRunsLen = GetUInt16(12);
		m_uiReserved3 = GetUInt32(14);
		m_data = new byte[0];
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		m_iLength = MinimumRecordSize;
		m_data = new byte[m_iLength];
		SetUInt16(0, m_usOptions);
		SetBit(1, m_bLockText, 2);
		SetBit(1, m_bLockText, 1);
		SetUInt16(2, m_usRotation);
		SetUInt32(4, m_uiReserved1);
		SetUInt16(8, m_usReserved2);
		SetUInt16(10, m_usTextLen);
		SetUInt16(12, m_usFormattingRunsLen);
		SetUInt32(14, m_uiReserved3);
	}
}
