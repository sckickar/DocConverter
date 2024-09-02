using System;
using System.IO;
using DocGen.Drawing;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.Bitmap)]
[CLSCompliant(false)]
internal class BitmapRecord : BiffContinueRecordRaw
{
	private const int DEF_ALIGN = 4;

	private const int DEF_HEADER_START = 8;

	[BiffRecordPos(0, 2)]
	private ushort m_usUnknown = 9;

	[BiffRecordPos(2, 2)]
	private ushort m_usUnknown2 = 1;

	[BiffRecordPos(4, 4, true)]
	private int m_iTotalSize;

	[BiffRecordPos(8, 4, true)]
	private int m_iHeaderSize = 12;

	[BiffRecordPos(12, 2)]
	private ushort m_usWidth;

	[BiffRecordPos(14, 2)]
	private ushort m_usHeight;

	[BiffRecordPos(16, 2)]
	private ushort m_usPlanes = 1;

	[BiffRecordPos(18, 2)]
	private ushort m_usColorDepth = 24;

	private Image m_bitmap;

	private nint m_scan0;

	public ushort Unknown
	{
		get
		{
			return m_usUnknown;
		}
		set
		{
			m_usUnknown = value;
		}
	}

	public ushort Unknown2
	{
		get
		{
			return m_usUnknown2;
		}
		set
		{
			m_usUnknown2 = value;
		}
	}

	public int TotalSize
	{
		get
		{
			return m_iTotalSize;
		}
		set
		{
			m_iTotalSize = value;
		}
	}

	public int HeaderSize
	{
		get
		{
			return m_iHeaderSize;
		}
		set
		{
			m_iHeaderSize = value;
		}
	}

	public ushort Width
	{
		get
		{
			return m_usWidth;
		}
		set
		{
			m_usWidth = value;
		}
	}

	public ushort Height
	{
		get
		{
			return m_usHeight;
		}
		set
		{
			m_usHeight = value;
		}
	}

	public ushort Planes
	{
		get
		{
			return m_usPlanes;
		}
		set
		{
			m_usPlanes = value;
		}
	}

	public ushort ColorDepth
	{
		get
		{
			return m_usColorDepth;
		}
		set
		{
			m_usColorDepth = value;
		}
	}

	public Image Picture
	{
		get
		{
			return m_bitmap;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_bitmap = value;
			m_usWidth = (ushort)Picture.Width;
			m_usHeight = (ushort)Picture.Height;
		}
	}

	public BitmapRecord()
	{
	}

	public BitmapRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public BitmapRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure()
	{
		base.ParseStructure();
		m_usUnknown = BiffRecordRaw.GetUInt16(m_data, 0);
		m_usUnknown2 = BiffRecordRaw.GetUInt16(m_data, 2);
		m_iTotalSize = BiffRecordRaw.GetInt32(m_data, 4);
		m_iHeaderSize = BiffRecordRaw.GetInt32(m_data, 8);
		m_usWidth = BiffRecordRaw.GetUInt16(m_data, 12);
		m_usHeight = BiffRecordRaw.GetUInt16(m_data, 14);
		m_usPlanes = BiffRecordRaw.GetUInt16(m_data, 16);
		m_usColorDepth = BiffRecordRaw.GetUInt16(m_data, 18);
		int num = 3 * m_usWidth;
		int num2 = num % 4;
		if (num2 != 0)
		{
			num2 = 4 - num2;
			num += num2;
		}
		int num3 = TotalSize - HeaderSize;
		byte[] array = new byte[num3];
		int srcOffset = 8 + HeaderSize;
		Buffer.BlockCopy(m_data, srcOffset, array, 0, num3);
		MemoryStream stream = new MemoryStream(array);
		m_bitmap = new Image(stream);
	}

	public override void InfillInternalData(OfficeVersion version)
	{
		byte[] imageData = GetImageData();
		AutoGrowData = true;
		int num = imageData.Length;
		int num2 = (m_iLength = num + HeaderSize + 8);
		m_iLength = ((num2 > MaximumRecordSize) ? MaximumRecordSize : num2);
		int num3 = ManualHeaderInfill();
		SetBytes(num3, imageData, 0, m_iLength - num3);
		base.InfillInternalData(version);
		if (num2 > MaximumRecordSize)
		{
			int num4 = m_iLength - num3;
			int length = num - num4;
			base.Builder.AppendBytes(imageData, num4, length);
			m_iLength = base.Builder.Total;
		}
	}

	private byte[] GetImageData()
	{
		byte[] imageData = m_bitmap.ImageData;
		int num = imageData.Length;
		int srcOffset = 0;
		byte[] array = new byte[num];
		Buffer.BlockCopy(imageData, srcOffset, array, 0, num);
		m_iTotalSize = num + 8;
		return array;
	}

	private int ManualHeaderInfill()
	{
		SetUInt16(0, m_usUnknown);
		SetUInt16(2, m_usUnknown2);
		SetInt32(4, m_iTotalSize);
		SetInt32(8, m_iHeaderSize);
		SetUInt16(12, m_usWidth);
		SetUInt16(14, m_usHeight);
		SetUInt16(16, m_usPlanes);
		SetUInt16(18, m_usColorDepth);
		return 8 + HeaderSize;
	}
}
