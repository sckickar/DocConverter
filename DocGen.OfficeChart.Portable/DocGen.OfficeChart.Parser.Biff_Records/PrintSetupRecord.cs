using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[CLSCompliant(false)]
[Biff(TBIFFRecord.PrintSetup)]
internal class PrintSetupRecord : BiffRecordRaw
{
	public const ushort ErrorBitMask = 3072;

	public const int ErrorStartBit = 10;

	private const int DEF_RECORD_SIZE = 34;

	[BiffRecordPos(0, 2)]
	private ushort m_usPaperSize = 9;

	[BiffRecordPos(2, 2)]
	private ushort m_usScale = 100;

	[BiffRecordPos(4, 2)]
	private short m_sPageStart = 1;

	[BiffRecordPos(6, 2)]
	private ushort m_usFitWidth = 1;

	[BiffRecordPos(8, 2)]
	private ushort m_usFitHeight = 1;

	[BiffRecordPos(10, 2)]
	private ushort m_usOptions = 4;

	[BiffRecordPos(10, 0, TFieldType.Bit)]
	private bool m_bLeftToRight;

	[BiffRecordPos(10, 1, TFieldType.Bit)]
	private bool m_bNotLandscape = true;

	[BiffRecordPos(10, 2, TFieldType.Bit)]
	private bool m_bNotValidSettings = true;

	[BiffRecordPos(10, 3, TFieldType.Bit)]
	private bool m_bNoColor;

	[BiffRecordPos(10, 4, TFieldType.Bit)]
	private bool m_bDraft;

	[BiffRecordPos(10, 5, TFieldType.Bit)]
	private bool m_bNotes;

	[BiffRecordPos(10, 6, TFieldType.Bit)]
	private bool m_bNoOrientation = true;

	[BiffRecordPos(10, 7, TFieldType.Bit)]
	private bool m_bUsePage;

	[BiffRecordPos(11, 1, TFieldType.Bit)]
	private bool m_bPrintNotes;

	[BiffRecordPos(12, 2)]
	private ushort m_usHResolution = 600;

	[BiffRecordPos(14, 2)]
	private ushort m_usVResolution = 600;

	[BiffRecordPos(16, 8, TFieldType.Float)]
	private double m_dbHeaderMargin = 0.5;

	[BiffRecordPos(24, 8, TFieldType.Float)]
	private double m_dbFooterMargin = 0.5;

	[BiffRecordPos(32, 2)]
	private ushort m_usCopies = 1;

	public ushort PaperSize
	{
		get
		{
			return m_usPaperSize;
		}
		set
		{
			m_usPaperSize = value;
			m_bNotValidSettings = false;
		}
	}

	public ushort Scale
	{
		get
		{
			return m_usScale;
		}
		set
		{
			m_usScale = value;
			m_bNotValidSettings = false;
		}
	}

	public short PageStart
	{
		get
		{
			return m_sPageStart;
		}
		set
		{
			m_sPageStart = value;
			m_bNotValidSettings = false;
		}
	}

	public ushort FitWidth
	{
		get
		{
			return m_usFitWidth;
		}
		set
		{
			m_usFitWidth = value;
			m_bNotValidSettings = false;
		}
	}

	public ushort FitHeight
	{
		get
		{
			return m_usFitHeight;
		}
		set
		{
			m_usFitHeight = value;
			m_bNotValidSettings = false;
		}
	}

	public ushort HResolution
	{
		get
		{
			return m_usHResolution;
		}
		set
		{
			m_usHResolution = value;
			m_bNotValidSettings = false;
		}
	}

	public ushort VResolution
	{
		get
		{
			return m_usVResolution;
		}
		set
		{
			m_usVResolution = value;
			m_bNotValidSettings = false;
		}
	}

	public double HeaderMargin
	{
		get
		{
			return m_dbHeaderMargin;
		}
		set
		{
			m_dbHeaderMargin = value;
			m_bNotValidSettings = false;
		}
	}

	public double FooterMargin
	{
		get
		{
			return m_dbFooterMargin;
		}
		set
		{
			m_dbFooterMargin = value;
			m_bNotValidSettings = false;
		}
	}

	public ushort Copies
	{
		get
		{
			return m_usCopies;
		}
		set
		{
			m_usCopies = value;
			m_bNotValidSettings = false;
		}
	}

	public bool IsLeftToRight
	{
		get
		{
			return m_bLeftToRight;
		}
		set
		{
			m_bLeftToRight = value;
			m_bNotValidSettings = false;
		}
	}

	public bool IsNotLandscape
	{
		get
		{
			return m_bNotLandscape;
		}
		set
		{
			m_bNotLandscape = value;
			m_bNotValidSettings = false;
		}
	}

	public bool IsNotValidSettings
	{
		get
		{
			return m_bNotValidSettings;
		}
		set
		{
			m_bNotValidSettings = value;
		}
	}

	public bool IsNoColor
	{
		get
		{
			return m_bNoColor;
		}
		set
		{
			m_bNoColor = value;
			m_bNotValidSettings = false;
		}
	}

	public bool IsDraft
	{
		get
		{
			return m_bDraft;
		}
		set
		{
			m_bDraft = value;
			m_bNotValidSettings = false;
		}
	}

	public bool IsNotes
	{
		get
		{
			return m_bNotes;
		}
		set
		{
			m_bNotes = value;
			m_bNotValidSettings = false;
		}
	}

	public bool IsNoOrientation
	{
		get
		{
			return m_bNoOrientation;
		}
		set
		{
			m_bNoOrientation = value;
			m_bNotValidSettings = false;
		}
	}

	public bool IsUsePage
	{
		get
		{
			return m_bUsePage;
		}
		set
		{
			m_bUsePage = value;
			m_bNotValidSettings = false;
		}
	}

	public bool IsPrintNotesAsDisplayed
	{
		get
		{
			return m_bPrintNotes;
		}
		set
		{
			m_bPrintNotes = value;
			m_bNotValidSettings = false;
		}
	}

	public OfficePrintErrors PrintErrors
	{
		get
		{
			return (OfficePrintErrors)(BiffRecordRaw.GetUInt16BitsByMask(m_usOptions, 3072) >> 10);
		}
		set
		{
			BiffRecordRaw.SetUInt16BitsByMask(ref m_usOptions, 3072, (ushort)((int)value << 10));
			m_bNotValidSettings = false;
		}
	}

	public override int MinimumRecordSize => 34;

	public override int MaximumRecordSize => 34;

	public PrintSetupRecord()
	{
	}

	public PrintSetupRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public PrintSetupRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iLength = 34;
		m_usPaperSize = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usScale = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_sPageStart = provider.ReadInt16(iOffset);
		iOffset += 2;
		m_usFitWidth = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usFitHeight = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bLeftToRight = provider.ReadBit(iOffset, 0);
		m_bNotLandscape = provider.ReadBit(iOffset, 1);
		m_bNotValidSettings = provider.ReadBit(iOffset, 2);
		if (m_bNotValidSettings)
		{
			m_usScale = 100;
		}
		m_bNoColor = provider.ReadBit(iOffset, 3);
		m_bDraft = provider.ReadBit(iOffset, 4);
		m_bNotes = provider.ReadBit(iOffset, 5);
		m_bNoOrientation = provider.ReadBit(iOffset, 6);
		if (m_bNoOrientation || m_bNotValidSettings)
		{
			m_bNotLandscape = true;
		}
		m_bUsePage = provider.ReadBit(iOffset, 7);
		iOffset++;
		m_bPrintNotes = provider.ReadBit(iOffset, 1);
		iOffset++;
		m_usHResolution = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usVResolution = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_dbHeaderMargin = provider.ReadDouble(iOffset);
		iOffset += 8;
		m_dbFooterMargin = provider.ReadDouble(iOffset);
		iOffset += 8;
		m_usCopies = provider.ReadUInt16(iOffset);
		iOffset += 2;
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = 34;
		provider.WriteUInt16(iOffset, m_usPaperSize);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usScale);
		iOffset += 2;
		provider.WriteInt16(iOffset, m_sPageStart);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usFitWidth);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usFitHeight);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bLeftToRight, 0);
		provider.WriteBit(iOffset, m_bNotLandscape, 1);
		provider.WriteBit(iOffset, m_bNotValidSettings, 2);
		provider.WriteBit(iOffset, m_bNoColor, 3);
		provider.WriteBit(iOffset, m_bDraft, 4);
		provider.WriteBit(iOffset, m_bNotes, 5);
		provider.WriteBit(iOffset, m_bNoOrientation, 6);
		provider.WriteBit(iOffset, m_bUsePage, 7);
		iOffset++;
		provider.WriteBit(iOffset, m_bPrintNotes, 1);
		iOffset++;
		provider.WriteUInt16(iOffset, m_usHResolution);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usVResolution);
		iOffset += 2;
		provider.WriteDouble(iOffset, m_dbHeaderMargin);
		iOffset += 8;
		provider.WriteDouble(iOffset, m_dbFooterMargin);
		iOffset += 8;
		provider.WriteUInt16(iOffset, m_usCopies);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 34;
	}
}
