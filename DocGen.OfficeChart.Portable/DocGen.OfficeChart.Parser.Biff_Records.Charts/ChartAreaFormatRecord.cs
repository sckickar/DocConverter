using System;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartAreaFormat)]
[CLSCompliant(false)]
internal class ChartAreaFormatRecord : BiffRecordRaw
{
	public const int DEF_RECORD_SIZE = 16;

	[BiffRecordPos(0, 4, true)]
	private int m_iForeground;

	[BiffRecordPos(4, 4, true)]
	private int m_iBackground;

	[BiffRecordPos(8, 2)]
	private ushort m_usPattern;

	[BiffRecordPos(10, 2)]
	private ushort m_usOptions;

	[BiffRecordPos(10, 0, TFieldType.Bit)]
	private bool m_bAutomaticFormat = true;

	[BiffRecordPos(10, 1, TFieldType.Bit)]
	private bool m_bSwapColorsOnNegative;

	[BiffRecordPos(12, 2)]
	private ushort m_usForegroundIndex;

	[BiffRecordPos(14, 2)]
	private ushort m_usBackgroundIndex;

	public int ForegroundColor
	{
		get
		{
			return m_iForeground;
		}
		set
		{
			if (value != m_iForeground)
			{
				m_iForeground = value;
			}
		}
	}

	public Color BackgroundColor
	{
		get
		{
			return ColorExtension.FromArgb(m_iBackground);
		}
		set
		{
			int num = value.ToArgb() & 0xFFFFFF;
			if (num != m_iBackground)
			{
				m_iBackground = num;
			}
		}
	}

	public OfficePattern Pattern
	{
		get
		{
			return (OfficePattern)m_usPattern;
		}
		set
		{
			m_usPattern = (ushort)value;
		}
	}

	public ushort Options => m_usOptions;

	public OfficeKnownColors ForegroundColorIndex
	{
		get
		{
			return (OfficeKnownColors)m_usForegroundIndex;
		}
		set
		{
			ushort num = (ushort)value;
			if (num != m_usForegroundIndex)
			{
				m_usForegroundIndex = num;
			}
		}
	}

	public OfficeKnownColors BackgroundColorIndex
	{
		get
		{
			return (OfficeKnownColors)m_usBackgroundIndex;
		}
		set
		{
			ushort num = (ushort)value;
			if (num != m_usBackgroundIndex)
			{
				m_usBackgroundIndex = num;
			}
		}
	}

	public bool UseAutomaticFormat
	{
		get
		{
			return m_bAutomaticFormat;
		}
		set
		{
			m_bAutomaticFormat = value;
		}
	}

	public bool SwapColorsOnNegative
	{
		get
		{
			return m_bSwapColorsOnNegative;
		}
		set
		{
			m_bSwapColorsOnNegative = value;
		}
	}

	public override int MinimumRecordSize => 16;

	public override int MaximumRecordSize => 16;

	public ChartAreaFormatRecord()
	{
	}

	public ChartAreaFormatRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartAreaFormatRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 16;
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_iForeground = ReadColor(provider, ref iOffset);
		m_iBackground = ReadColor(provider, ref iOffset);
		m_usPattern = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usOptions = provider.ReadUInt16(iOffset);
		m_bAutomaticFormat = provider.ReadBit(iOffset, 0);
		m_bSwapColorsOnNegative = provider.ReadBit(iOffset, 1);
		iOffset += 2;
		m_usForegroundIndex = provider.ReadUInt16(iOffset);
		iOffset += 2;
		m_usBackgroundIndex = provider.ReadUInt16(iOffset);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_usOptions &= 3;
		m_iLength = GetStoreSize(version);
		WriteColor(provider, ref iOffset, m_iForeground);
		WriteColor(provider, ref iOffset, m_iBackground);
		provider.WriteUInt16(iOffset, m_usPattern);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usOptions);
		provider.WriteBit(iOffset, m_bAutomaticFormat, 0);
		provider.WriteBit(iOffset, m_bSwapColorsOnNegative, 1);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usForegroundIndex);
		iOffset += 2;
		provider.WriteUInt16(iOffset, m_usBackgroundIndex);
	}

	private int ReadColor(DataProvider provider, ref int iOffset)
	{
		byte red = provider.ReadByte(iOffset++);
		byte green = provider.ReadByte(iOffset++);
		byte blue = provider.ReadByte(iOffset++);
		iOffset++;
		return Color.FromArgb(255, red, green, blue).ToArgb();
	}

	private void WriteColor(DataProvider provider, ref int iOffset, int iColor)
	{
		Color color = ColorExtension.FromArgb(iColor);
		provider.WriteByte(iOffset++, color.R);
		provider.WriteByte(iOffset++, color.G);
		provider.WriteByte(iOffset++, color.B);
		provider.WriteByte(iOffset++, 0);
	}
}
