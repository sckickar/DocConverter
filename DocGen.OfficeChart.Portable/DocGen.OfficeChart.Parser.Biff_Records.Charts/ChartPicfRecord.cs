using System;
using System.IO;

namespace DocGen.OfficeChart.Parser.Biff_Records.Charts;

[Biff(TBIFFRecord.ChartPicf)]
[CLSCompliant(false)]
internal class ChartPicfRecord : BiffRecordRaw
{
	[Flags]
	public enum TPicture
	{
		Stretched = 1,
		Stacked = 2
	}

	public enum TImageFormat
	{
		WindowsMetafile = 2,
		MacintoshPICT = 2,
		WindowsBitmap = 9
	}

	public enum TEnvironment
	{
		Windows = 1,
		Macintosh
	}

	public const int DefaultRecordSize = 14;

	[BiffRecordPos(0, 2)]
	private ushort m_usPictureType;

	[BiffRecordPos(2, 2)]
	private ushort m_usImageFormat;

	[BiffRecordPos(4, 1)]
	private byte m_Environment;

	[BiffRecordPos(5, 1)]
	private byte m_usOptions;

	[BiffRecordPos(5, 0, TFieldType.Bit)]
	private bool m_bFormatOnly;

	[BiffRecordPos(5, 1, TFieldType.Bit)]
	private bool m_bPictureTopBottom;

	[BiffRecordPos(5, 2, TFieldType.Bit)]
	private bool m_bPictureBackFront;

	[BiffRecordPos(5, 3, TFieldType.Bit)]
	private bool m_bPictureSides;

	[BiffRecordPos(6, 8, TFieldType.Float)]
	private double m_numScale;

	public TPicture PictureType
	{
		get
		{
			return (TPicture)m_usPictureType;
		}
		set
		{
			m_usPictureType = (ushort)value;
		}
	}

	public TImageFormat ImageFormat
	{
		get
		{
			return (TImageFormat)m_usImageFormat;
		}
		set
		{
			m_usImageFormat = (ushort)value;
		}
	}

	public TEnvironment Environment
	{
		get
		{
			return (TEnvironment)m_Environment;
		}
		set
		{
			m_Environment = (byte)value;
		}
	}

	public byte Options => m_usOptions;

	public bool IsFormatOnly
	{
		get
		{
			return m_bFormatOnly;
		}
		set
		{
			m_bFormatOnly = value;
		}
	}

	public bool IsPictureTopBottom
	{
		get
		{
			return m_bPictureTopBottom;
		}
		set
		{
			m_bPictureTopBottom = value;
		}
	}

	public bool IsPictureBackFront
	{
		get
		{
			return m_bPictureBackFront;
		}
		set
		{
			m_bPictureBackFront = value;
		}
	}

	public bool IsPictureSides
	{
		get
		{
			return m_bPictureSides;
		}
		set
		{
			m_bPictureSides = value;
		}
	}

	public double Scale
	{
		get
		{
			return m_numScale;
		}
		set
		{
			m_numScale = value;
		}
	}

	public override int MinimumRecordSize => 14;

	public override int MaximumRecordSize => 14;

	public ChartPicfRecord()
	{
	}

	public ChartPicfRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public ChartPicfRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		m_usPictureType = provider.ReadUInt16(iOffset);
		m_usImageFormat = provider.ReadUInt16(iOffset + 2);
		m_Environment = provider.ReadByte(iOffset + 4);
		m_usOptions = provider.ReadByte(iOffset + 5);
		m_bFormatOnly = provider.ReadBit(iOffset + 5, 0);
		m_bPictureTopBottom = provider.ReadBit(iOffset + 5, 1);
		m_bPictureBackFront = provider.ReadBit(iOffset + 5, 2);
		m_bPictureSides = provider.ReadBit(iOffset + 5, 3);
		m_numScale = provider.ReadDouble(iOffset + 6);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		provider.WriteUInt16(iOffset, m_usPictureType);
		provider.WriteUInt16(iOffset + 2, m_usImageFormat);
		provider.WriteByte(iOffset + 4, m_Environment);
		provider.WriteByte(iOffset + 5, m_usOptions);
		provider.WriteBit(iOffset + 5, m_bFormatOnly, 0);
		provider.WriteBit(iOffset + 5, m_bPictureTopBottom, 1);
		provider.WriteBit(iOffset + 5, m_bPictureBackFront, 2);
		provider.WriteBit(iOffset + 5, m_bPictureSides, 3);
		provider.WriteDouble(iOffset + 6, m_numScale);
		m_iLength = 14;
	}
}
