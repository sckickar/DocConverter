using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[CLSCompliant(false)]
internal class MsoBitmapPicture : MsoBase, IDisposable, IPictureRecord
{
	public const int DEF_DIB_HEADER_SIZE = 14;

	private static readonly byte[] DEF_DIB_ID = new byte[2] { 66, 77 };

	private static readonly byte[] DEF_RESERVED = new byte[4];

	public const int DEF_COLOR_USED_OFFSET = 32;

	private const uint DEF_COLOR_SIZE = 4u;

	internal const uint BlipDIBWithTwoUIDs = 1961u;

	internal const uint BlipPNGWithTwoUIDs = 1761u;

	internal const uint BlipJPEGWithTwoUIDs = 1347u;

	private byte[] m_arrRgbUid = new byte[16];

	private byte[] m_arrRgbUidPrimary;

	private byte m_btTag = byte.MaxValue;

	private int m_iPictureDataOffset;

	private Image m_picture;

	private Stream m_pictStream;

	private MemoryStream m_stream;

	public Image Picture
	{
		get
		{
			return m_picture;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_picture = value;
			EvaluateHash();
		}
	}

	public Stream PictureStream
	{
		get
		{
			return m_pictStream;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			m_pictStream = value;
		}
	}

	public byte[] RgbUid
	{
		get
		{
			return m_arrRgbUid;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length != m_arrRgbUid.Length)
			{
				throw new ArgumentOutOfRangeException("value.Length");
			}
			m_arrRgbUid = value;
		}
	}

	public byte Tag
	{
		get
		{
			return m_btTag;
		}
		set
		{
			m_btTag = value;
		}
	}

	public int PictureDataOffset
	{
		get
		{
			return m_iPictureDataOffset;
		}
		set
		{
			m_iPictureDataOffset = value;
		}
	}

	public bool IsDib => (base.Parent as MsofbtBSE).BlipType == MsoBlipType.msoblipDIB;

	public MsoBitmapPicture(MsoBase parent)
		: base(parent)
	{
	}

	public MsoBitmapPicture(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public MsoBitmapPicture(MsoBase parent, Stream stream)
		: base(parent, stream, null)
	{
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		if (base.MsoRecordType == (MsoRecords)0)
		{
			base.MsoRecordType = (MsoRecords)61470;
		}
		m_iLength = 0;
		stream.Write(m_arrRgbUid, 0, 16);
		m_iLength += 16;
		if (HasTwoUIDs())
		{
			stream.Write(m_arrRgbUidPrimary, 0, 16);
			m_iLength += 16;
		}
		stream.WriteByte(m_btTag);
		m_iLength++;
		int num = (IsDib ? 14 : 0);
		MemoryStream memoryStream = new MemoryStream();
		byte[] imageData = m_picture.ImageData;
		memoryStream.Write(imageData, 0, imageData.Length);
		memoryStream.Position = 0L;
		int num2 = (int)memoryStream.Length;
		byte[] buffer = new byte[10240];
		memoryStream.Position = num;
		iOffset = num;
		while (iOffset < num2)
		{
			int num3 = memoryStream.Read(buffer, 0, 10240);
			stream.Write(buffer, 0, num3);
			iOffset += num3;
		}
		m_iLength += num2 - num;
	}

	public override void ParseStructure(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		stream.Read(m_arrRgbUid, 0, 16);
		int num = LoadPrimaryUID(stream) + 16;
		m_btTag = (byte)stream.ReadByte();
		CreateImageStream(stream, m_iPictureDataOffset = num + 1);
		m_picture = new Image(m_stream);
	}

	private int LoadPrimaryUID(Stream stream)
	{
		int num = 0;
		if (HasTwoUIDs())
		{
			m_arrRgbUidPrimary = new byte[16];
			stream.Read(m_arrRgbUidPrimary, 0, 16);
			num += 16;
		}
		return num;
	}

	protected override object InternalClone()
	{
		MsoBitmapPicture msoBitmapPicture = (MsoBitmapPicture)base.InternalClone();
		if (m_arrRgbUid != null)
		{
			msoBitmapPicture.m_arrRgbUid = CloneUtils.CloneByteArray(m_arrRgbUid);
		}
		if (m_stream != null)
		{
			msoBitmapPicture.m_stream = UtilityMethods.CloneStream(m_stream);
		}
		if (m_picture != null)
		{
			msoBitmapPicture.m_picture = ((m_stream != null) ? (msoBitmapPicture.m_picture = new Image(msoBitmapPicture.m_stream)) : m_picture.Clone());
		}
		return msoBitmapPicture;
	}

	private void CreateImageStream(Stream stream, int iOffset)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (m_stream != null)
		{
			m_stream.Dispose();
		}
		int num = m_iLength - iOffset;
		bool isDib = IsDib;
		int iFullSize = num + (isDib ? 14 : 0);
		m_stream = new MemoryStream(num + 14);
		int num2 = num;
		if (isDib)
		{
			uint uiSize = MsoBase.ReadUInt32(stream);
			stream.Position -= 4L;
			AddBitMapHeaderToStream(m_stream, iFullSize, uiSize, GetDibColorsCount(stream, iOffset));
		}
		byte[] buffer = new byte[10240];
		int num3;
		while ((num3 = stream.Read(buffer, 0, Math.Min(10240, num2))) > 0 && num2 > 0)
		{
			m_stream.Write(buffer, 0, num3);
			num2 -= num3;
		}
		m_stream.Position = 0L;
	}

	private bool HasTwoUIDs()
	{
		int instance = base.Instance;
		if ((long)instance != 1961 && (long)instance != 1761)
		{
			return (long)instance == 1347;
		}
		return true;
	}

	private uint GetDibColorsCount(Stream stream, int iOffset)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] array = new byte[4];
		stream.Position += 32L;
		stream.Read(array, 0, 4);
		stream.Position -= 36L;
		return BitConverter.ToUInt32(array, 0);
	}

	private void EvaluateHash()
	{
		MemoryStream memoryStream = new MemoryStream();
		byte[] imageData = m_picture.ImageData;
		memoryStream.Write(imageData, 0, imageData.Length);
		memoryStream.Position = 0L;
	}

	public static void AddBitMapHeaderToStream(MemoryStream ms, int iFullSize, uint uiSize, uint dibColorCount)
	{
		byte[] bytes = BitConverter.GetBytes(iFullSize);
		ms.Write(DEF_DIB_ID, 0, DEF_DIB_ID.Length);
		ms.Write(bytes, 0, bytes.Length);
		ms.Write(DEF_RESERVED, 0, DEF_RESERVED.Length);
		bytes = BitConverter.GetBytes(uiSize + 14 + dibColorCount * 4);
		ms.Write(bytes, 0, bytes.Length);
	}

	protected override void OnDispose()
	{
		if (m_stream != null)
		{
			m_stream.Dispose();
			m_stream = null;
		}
	}
}
