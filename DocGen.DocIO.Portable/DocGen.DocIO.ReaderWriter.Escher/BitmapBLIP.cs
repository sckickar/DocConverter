using System.IO;
using DocGen.DocIO.DLS.Entities;

namespace DocGen.DocIO.ReaderWriter.Escher;

internal class BitmapBLIP : Blip
{
	private byte[] m_rgbUid;

	private byte[] m_rgbUidPrimary;

	private byte m_tag;

	private MemoryStream m_pvImageBytes;

	public byte[] RgbUid
	{
		get
		{
			return m_rgbUid;
		}
		set
		{
			m_rgbUid = value;
		}
	}

	public byte[] RgbUidPrimary
	{
		get
		{
			return m_rgbUidPrimary;
		}
		set
		{
			m_rgbUidPrimary = value;
		}
	}

	public byte Tag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	public MemoryStream ImageBytes
	{
		get
		{
			return m_pvImageBytes;
		}
		set
		{
			m_pvImageBytes = value;
		}
	}

	public BitmapBLIP()
	{
		m_rgbUid = new byte[16];
		m_rgbUidPrimary = new byte[16];
	}

	public override Image Read(Stream stream, int length, bool hasPrimaryUid)
	{
		stream.Read(RgbUid, 0, RgbUid.Length);
		int num = 16;
		if (hasPrimaryUid)
		{
			stream.Read(RgbUidPrimary, 0, RgbUidPrimary.Length);
			num += 16;
		}
		Tag = (byte)stream.ReadByte();
		ImageBytes = null;
		num++;
		byte[] array = new byte[length - num];
		stream.Read(array, 0, array.Length);
		ImageBytes = new MemoryStream(array, 0, array.Length);
		return new Image(ImageBytes);
	}

	internal override void Write(Stream stream, MemoryStream image, MSOBlipType imageFormat, byte[] id)
	{
		WriteDefaults(stream, image.Length, imageFormat, id);
		byte[] array = new byte[image.Length];
		image.Position = 0L;
		image.Read(array, 0, array.Length);
		stream.Write(array, 0, array.Length);
	}

	private void WriteDefaults(Stream stream, long size, MSOBlipType type, byte[] id)
	{
		MSOFBH mSOFBH = new MSOFBH();
		mSOFBH.Msofbt = MSOFBT.msofbtBSE;
		mSOFBH.Inst = 6u;
		mSOFBH.Version = 2u;
		mSOFBH.Length = (uint)(size + 61);
		mSOFBH.Write(stream);
		FBSE fBSE = new FBSE();
		fBSE.Win32 = type;
		fBSE.MacOS = type;
		id.CopyTo(fBSE.Uid, 0);
		fBSE.Usage = MSOBlipUsage.msoblipUsageDefault;
		fBSE.Name = 0;
		fBSE.Size = (uint)(size + 25);
		fBSE.Delay = 68u;
		fBSE.Ref = 1u;
		fBSE.Tag = 255;
		fBSE.Unused2 = 0;
		fBSE.Unused3 = 0;
		fBSE.Write(stream);
		MSOFBH mSOFBH2 = new MSOFBH();
		mSOFBH2.Length = (uint)((int)size + 17);
		mSOFBH2.Msofbt = (MSOFBT)(61464 + type);
		mSOFBH2.Inst = 1760u;
		mSOFBH2.Version = 0u;
		mSOFBH2.Write(stream);
		stream.Write(id, 0, id.Length);
		stream.WriteByte(byte.MaxValue);
	}

	internal override void Close()
	{
		base.Close();
		m_rgbUid = null;
		m_rgbUidPrimary = null;
		m_pvImageBytes.Close();
		m_pvImageBytes = null;
	}
}
