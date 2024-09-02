using System;
using System.Collections.Generic;
using System.IO;
using DocGen.Compression;
using DocGen.Drawing;
using DocGen.OfficeChart.Implementation;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[CLSCompliant(false)]
internal class MsoMetafilePicture : MsoBase, IDisposable, IPictureRecord
{
	private const int DEF_BUFFER_SIZE = 32768;

	private const int DEF_UID_OFFSET = 0;

	private const int DEF_METAFILE_SIZE_OFFSET = 16;

	private const int DEF_COMPRESSED_SIZE_OFFSET = 40;

	internal const uint BlipEMFWithTwoUIDs = 981u;

	internal const uint BlipWMFWithTwoUIDs = 535u;

	internal const uint BlipPICTWithTwoUIDs = 1347u;

	internal const uint BlipTIFFWithTwoUIDs = 1765u;

	private MemoryStream m_stream;

	private byte[] m_arrCompressedPicture;

	private byte[] m_arrRgbUid = new byte[16];

	private byte[] m_arrRgbUidPrimary;

	private int m_iMetafileSize;

	private Rectangle m_rcBounds;

	private Point m_ptSize;

	private int m_iSavedSize;

	private MsoBlipCompression m_compression;

	private MsoBlipFilter m_filter = MsoBlipFilter.msofilterNone;

	private Image m_picture;

	private Stream m_pictStream;

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
				throw new ArgumentNullException("Picture");
			}
			m_picture = value;
			MemoryStream memoryStream = SerializeMetafile(m_picture);
			m_arrCompressedPicture = CompressMetafile(memoryStream, 0);
			memoryStream.Dispose();
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

	public MsoMetafilePicture(MsoBase parent)
		: base(parent)
	{
	}

	public MsoMetafilePicture(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	public MsoMetafilePicture(MsoBase parent, Stream stream)
		: base(parent, stream, null)
	{
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		m_rcBounds.Y = 0;
		m_rcBounds.X = 0;
		m_rcBounds.Width = m_picture.Width;
		m_rcBounds.Height = m_picture.Height;
		m_ptSize.X = (int)ApplicationImpl.ConvertFromPixel(m_picture.Width, MeasureUnits.EMU);
		m_ptSize.Y = (int)ApplicationImpl.ConvertFromPixel(m_picture.Height, MeasureUnits.EMU);
		if (base.MsoRecordType == (MsoRecords)0)
		{
			base.MsoRecordType = (MsoRecords)61466;
			base.Instance = 980;
		}
		m_compression = MsoBlipCompression.msoCompressionDeflate;
		int num = 0;
		stream.Write(m_arrRgbUid, 0, m_arrRgbUid.Length);
		num += m_arrRgbUid.Length;
		if (HasTwoUIDs())
		{
			stream.Write(m_arrRgbUidPrimary, 0, m_arrRgbUidPrimary.Length);
			num += m_arrRgbUidPrimary.Length;
		}
		MsoBase.WriteInt32(stream, m_iMetafileSize);
		num += 4;
		MsoBase.WriteInt32(stream, m_rcBounds.Left);
		num += 4;
		MsoBase.WriteInt32(stream, m_rcBounds.Top);
		num += 4;
		MsoBase.WriteInt32(stream, m_rcBounds.Right);
		num += 4;
		MsoBase.WriteInt32(stream, m_rcBounds.Bottom);
		num += 4;
		MsoBase.WriteInt32(stream, m_ptSize.X);
		num += 4;
		MsoBase.WriteInt32(stream, m_ptSize.Y);
		num += 4;
		MsoBase.WriteInt32(stream, m_iSavedSize);
		num += 4;
		stream.WriteByte((byte)m_compression);
		num++;
		stream.WriteByte((byte)m_filter);
		num++;
		stream.Write(m_arrCompressedPicture, 0, m_arrCompressedPicture.Length);
		num += m_arrCompressedPicture.Length;
		m_iLength = num;
	}

	public override void ParseStructure(Stream stream)
	{
		long position = stream.Position;
		stream.Read(m_arrRgbUid, 0, 16);
		LoadPrimaryUID(stream);
		m_iMetafileSize = MsoBase.ReadInt32(stream);
		int left = MsoBase.ReadInt32(stream);
		int top = MsoBase.ReadInt32(stream);
		int right = MsoBase.ReadInt32(stream);
		int bottom = MsoBase.ReadInt32(stream);
		m_rcBounds = Rectangle.FromLTRB(left, top, right, bottom);
		left = MsoBase.ReadInt32(stream);
		top = MsoBase.ReadInt32(stream);
		m_ptSize = new Point(left, top);
		m_iSavedSize = MsoBase.ReadInt32(stream);
		m_compression = (MsoBlipCompression)stream.ReadByte();
		m_filter = (MsoBlipFilter)stream.ReadByte();
		if (m_stream != null)
		{
			m_stream.Dispose();
		}
		m_stream = new MemoryStream();
		long num = stream.Position - position;
		int num2 = (int)(m_iLength - num);
		MemoryStream memoryStream = new MemoryStream(num2);
		memoryStream.SetLength(num2);
		byte[] array = new byte[num2];
		stream.Read(array, 0, array.Length);
		memoryStream.Write(array, 0, array.Length);
		memoryStream.Position = 0L;
		m_arrCompressedPicture = new byte[memoryStream.Length];
		memoryStream.Read(m_arrCompressedPicture, 0, (int)memoryStream.Length);
		memoryStream.Position = 0L;
		if (m_compression == MsoBlipCompression.msoCompressionDeflate)
		{
			CompressedStreamReader compressedStreamReader = new CompressedStreamReader(memoryStream);
			byte[] array2 = new byte[32768];
			int count;
			while ((count = compressedStreamReader.Read(array2, 0, array2.Length)) > 0)
			{
				m_stream.Write(array2, 0, count);
			}
		}
		else
		{
			m_stream.Write(m_arrCompressedPicture, 0, m_arrCompressedPicture.Length);
		}
		m_stream.Position = 0L;
		m_picture = new Image(m_stream);
		memoryStream.Dispose();
	}

	private bool HasTwoUIDs()
	{
		int instance = base.Instance;
		if ((long)instance != 981 && (long)instance != 535 && (long)instance != 1347)
		{
			return (long)instance == 1765;
		}
		return true;
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

	public static MemoryStream SerializeMetafile(Image picture)
	{
		if (picture == null)
		{
			return null;
		}
		MemoryStream memoryStream = new MemoryStream();
		_ = picture.Height;
		_ = picture.Width;
		byte[] imageData = picture.ImageData;
		memoryStream.Write(imageData, 0, imageData.Length);
		memoryStream.Position = 0L;
		return memoryStream;
	}

	private byte[] CompressMetafile(Stream metaFile, int iDataOffset)
	{
		MemoryStream memoryStream = new MemoryStream();
		m_iMetafileSize = (int)metaFile.Length;
		CompressedStreamWriter compressedStreamWriter = new CompressedStreamWriter(memoryStream, CompressionLevel.Best, bCloseStream: false);
		int num = 0;
		byte[] array = new byte[32768];
		metaFile.Position = 0L;
		long length = metaFile.Length;
		while ((num = metaFile.Read(array, 0, 32768)) > 0)
		{
			compressedStreamWriter.Write(array, 0, num, metaFile.Position + 1 >= length);
		}
		memoryStream.Position = 0L;
		m_iLength = iDataOffset;
		m_iSavedSize = (int)memoryStream.Length;
		byte[] array2 = new byte[memoryStream.Length];
		memoryStream.Position = 0L;
		memoryStream.Read(array2, 0, (int)memoryStream.Length);
		return array2;
	}

	protected override object InternalClone()
	{
		MsoMetafilePicture msoMetafilePicture = (MsoMetafilePicture)base.InternalClone();
		msoMetafilePicture.m_arrCompressedPicture = CloneUtils.CloneByteArray(m_arrCompressedPicture);
		if (m_stream != null)
		{
			msoMetafilePicture.m_stream = UtilityMethods.CloneStream(m_stream);
			msoMetafilePicture.m_stream.Position = 0L;
		}
		return msoMetafilePicture;
	}

	protected override void OnDispose()
	{
		if (m_stream != null)
		{
			m_stream.Dispose();
			m_stream = null;
		}
	}

	~MsoMetafilePicture()
	{
		if (m_stream != null)
		{
			Dispose();
		}
	}
}
