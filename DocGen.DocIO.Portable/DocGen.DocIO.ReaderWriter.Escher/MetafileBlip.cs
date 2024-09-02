using System.IO;
using DocGen.Compression;
using DocGen.DocIO.DLS.Entities;
using DocGen.DocIO.ReaderWriter.Biff_Records;

namespace DocGen.DocIO.ReaderWriter.Escher;

internal class MetafileBlip : Blip
{
	private byte[] m_rgbUid;

	private byte[] m_rgbUidPrimary;

	private uint m_cbSave;

	private byte m_fCompression;

	private byte[] m_pvBits;

	private int m_length;

	private int m_rectLeft;

	private int m_rectTop;

	private int m_rectRight;

	private int m_rectBottom;

	private int m_rectWidth;

	private int m_rectHeight;

	private CompressionMethod m_compressionMethod;

	private byte m_fFilter;

	private byte[] m_compressedImage;

	private byte[] m_uncompressedImage;

	private Metafile m_srcMetafile;

	internal Metafile Metafile
	{
		get
		{
			return m_srcMetafile;
		}
		set
		{
			m_srcMetafile = value;
		}
	}

	public MetafileBlip()
	{
		m_rgbUid = new byte[16];
		m_rgbUidPrimary = new byte[16];
	}

	public override Image Read(Stream stream, int length, bool hasPrimaryUid)
	{
		for (int i = 0; i < 16; i++)
		{
			m_rgbUid[i] = (byte)stream.ReadByte();
		}
		m_length = BaseWordRecord.ReadInt32(stream);
		m_rectLeft = BaseWordRecord.ReadInt32(stream);
		m_rectTop = BaseWordRecord.ReadInt32(stream);
		m_rectRight = BaseWordRecord.ReadInt32(stream);
		m_rectBottom = BaseWordRecord.ReadInt32(stream);
		m_rectWidth = BaseWordRecord.ReadInt32(stream);
		m_rectHeight = BaseWordRecord.ReadInt32(stream);
		m_cbSave = BaseWordRecord.ReadUInt32(stream);
		m_fCompression = (byte)stream.ReadByte();
		m_fFilter = (byte)stream.ReadByte();
		m_pvBits = new byte[m_cbSave];
		stream.Read(m_pvBits, 0, m_pvBits.Length);
		MemoryStream stream2 = new MemoryStream(m_pvBits);
		if (m_fCompression == 0)
		{
			CompressedStreamReader compressedStreamReader = new CompressedStreamReader(stream2);
			MemoryStream memoryStream = new MemoryStream();
			byte[] array = new byte[4096];
			while (true)
			{
				int num = compressedStreamReader.Read(array, 0, array.Length);
				if (num <= 0)
				{
					break;
				}
				memoryStream.Write(array, 0, num);
			}
			memoryStream.Position = 0L;
			return new Metafile(memoryStream);
		}
		return new Metafile(stream2);
	}

	internal override void Write(Stream stream, MemoryStream image, MSOBlipType imageFormat, byte[] Uid)
	{
		m_uncompressedImage = image.ToArray();
		m_length = m_uncompressedImage.Length;
		m_compressedImage = m_uncompressedImage;
		WriteDefaults(stream, m_length, Uid);
		stream.Write(Uid, 0, Uid.Length);
		BaseWordRecord.WriteInt32(stream, m_length);
		BaseWordRecord.WriteInt32(stream, m_rectLeft);
		BaseWordRecord.WriteInt32(stream, m_rectTop);
		BaseWordRecord.WriteInt32(stream, m_rectRight);
		BaseWordRecord.WriteInt32(stream, m_rectBottom);
		BaseWordRecord.WriteInt32(stream, m_rectWidth);
		BaseWordRecord.WriteInt32(stream, m_rectHeight);
		BaseWordRecord.WriteInt32(stream, m_compressedImage.Length);
		stream.WriteByte((byte)m_compressionMethod);
		stream.WriteByte(m_fFilter);
		stream.Write(m_compressedImage, 0, m_compressedImage.Length);
	}

	private void WriteDefaults(Stream stream, long size, byte[] Uid)
	{
		MSOFBH mSOFBH = new MSOFBH();
		mSOFBH.Msofbt = MSOFBT.msofbtBSE;
		mSOFBH.Inst = 2u;
		mSOFBH.Version = 2u;
		mSOFBH.Length = (uint)(size + 94);
		mSOFBH.Write(stream);
		FBSE fBSE = new FBSE();
		fBSE.Win32 = MSOBlipType.msoblipEMF;
		fBSE.MacOS = MSOBlipType.msoblipPICT;
		for (int i = 0; i < 16; i++)
		{
			fBSE.Uid[i] = Uid[i];
		}
		fBSE.Usage = MSOBlipUsage.msoblipUsageDefault;
		fBSE.Name = 0;
		fBSE.Size = (uint)(size + 58);
		fBSE.Delay = 68u;
		fBSE.Ref = 1u;
		fBSE.Tag = 255;
		fBSE.Unused2 = 0;
		fBSE.Unused3 = 0;
		fBSE.Write(stream);
		MSOFBH mSOFBH2 = new MSOFBH();
		mSOFBH2.Length = (uint)((int)size + 50);
		mSOFBH2.Msofbt = MSOFBT.msofbtBlipEMF;
		mSOFBH2.Inst = 980u;
		mSOFBH2.Version = 0u;
		mSOFBH2.Write(stream);
	}

	internal override void Close()
	{
		base.Close();
		m_rgbUid = null;
		m_rgbUidPrimary = null;
		m_compressedImage = null;
		m_uncompressedImage = null;
		if (m_srcMetafile != null)
		{
			m_srcMetafile.Dispose();
			m_srcMetafile = null;
		}
	}
}
