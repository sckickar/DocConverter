using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DocGen.OfficeChart.Parser.Biff_Records.MsoDrawing;

[MsoDrawing(MsoRecords.msofbtBSE)]
[CLSCompliant(false)]
internal class MsofbtBSE : MsoBase
{
	private const int DEF_NAME_OFFSET = 36;

	private static readonly MsoBlipType[] DEF_BITMAP_BLIPS = new MsoBlipType[3]
	{
		MsoBlipType.msoblipDIB,
		MsoBlipType.msoblipPNG,
		MsoBlipType.msoblipJPEG
	};

	private static readonly MsoBlipType[] DEF_PICT_BLIPS = new MsoBlipType[3]
	{
		MsoBlipType.msoblipEMF,
		MsoBlipType.msoblipPICT,
		MsoBlipType.msoblipWMF
	};

	[BiffRecordPos(0, 1)]
	private byte m_btReqWin32;

	[BiffRecordPos(1, 1)]
	private byte m_btReqMac;

	[BiffRecordPos(20, 4)]
	private uint m_uiSize;

	[BiffRecordPos(24, 4)]
	private uint m_uiRefCount;

	[BiffRecordPos(28, 4)]
	private uint m_uiFileOffset;

	[BiffRecordPos(32, 1)]
	private byte m_btUsage;

	[BiffRecordPos(33, 1)]
	private byte m_btNameLength;

	[BiffRecordPos(34, 1)]
	private byte m_btUnused1;

	[BiffRecordPos(35, 1)]
	private byte m_btUnused2;

	private string m_strBlipName = string.Empty;

	private MsoBase m_msoPicture;

	private int m_iIndex;

	private string m_strPicturePath;

	public byte RequiredWin32
	{
		get
		{
			return m_btReqWin32;
		}
		set
		{
			m_btReqWin32 = value;
		}
	}

	public byte RequiredMac
	{
		get
		{
			return m_btReqMac;
		}
		set
		{
			m_btReqMac = value;
		}
	}

	public string BlipName
	{
		get
		{
			return m_strBlipName;
		}
		set
		{
			m_strBlipName = value;
			m_btNameLength = (byte)((value != null) ? ((byte)m_strBlipName.Length) : 0);
		}
	}

	public uint SizeInStream
	{
		get
		{
			return m_uiSize;
		}
		set
		{
			m_uiSize = value;
		}
	}

	public uint RefCount
	{
		get
		{
			return m_uiRefCount;
		}
		set
		{
			m_uiRefCount = value;
		}
	}

	public uint FileOffset
	{
		get
		{
			return m_uiFileOffset;
		}
		set
		{
			m_uiFileOffset = value;
		}
	}

	public MsoBlipUsage BlipUsage
	{
		get
		{
			return (MsoBlipUsage)m_btUsage;
		}
		set
		{
			m_btUsage = (byte)value;
		}
	}

	public byte NameLength => m_btNameLength;

	public byte Unused1 => m_btUnused1;

	public byte Unused2 => m_btUnused2;

	public MsoBlipType BlipType
	{
		get
		{
			return (MsoBlipType)base.Instance;
		}
		set
		{
			base.Instance = (int)value;
		}
	}

	public IPictureRecord PictureRecord
	{
		get
		{
			return m_msoPicture as IPictureRecord;
		}
		set
		{
			m_msoPicture = value as MsoBase;
		}
	}

	public override bool NeedDataArray => true;

	public int Index
	{
		get
		{
			return m_iIndex;
		}
		set
		{
			m_iIndex = value;
		}
	}

	public string PicturePath
	{
		get
		{
			return m_strPicturePath;
		}
		set
		{
			m_strPicturePath = value;
		}
	}

	public MsofbtBSE(MsoBase parent)
		: base(parent)
	{
	}

	public MsofbtBSE(MsoBase parent, byte[] data, int iOffset)
		: base(parent, data, iOffset)
	{
	}

	protected override void OnDispose()
	{
		base.OnDispose();
	}

	public new void Dispose()
	{
		if (m_msoPicture != null)
		{
			m_msoPicture.Dispose();
		}
	}

	public override void InfillInternalData(Stream stream, int iOffset, List<int> arrBreaks, List<List<BiffRecordRaw>> arrRecords)
	{
		m_uiSize = 0u;
		m_btNameLength = 0;
		m_iLength = 36;
		stream.WriteByte(m_btReqWin32);
		stream.WriteByte(m_btReqMac);
		stream.Position += 18L;
		long position = stream.Position;
		MsoBase.WriteUInt32(stream, m_uiSize);
		MsoBase.WriteUInt32(stream, m_uiRefCount);
		MsoBase.WriteUInt32(stream, m_uiFileOffset);
		stream.WriteByte(m_btUsage);
		stream.WriteByte(m_btNameLength);
		stream.WriteByte(m_btUnused1);
		stream.WriteByte(m_btUnused2);
		if (m_btNameLength > 0)
		{
			byte[] bytes = Encoding.Unicode.GetBytes(m_strBlipName);
			int num = bytes.Length;
			stream.Write(bytes, 0, num);
			m_iLength += num;
		}
		if (m_msoPicture != null)
		{
			long position2 = stream.Position;
			m_msoPicture.FillArray(stream);
			m_uiSize = (uint)(stream.Position - position2);
			m_iLength += (int)m_uiSize;
			position2 = stream.Position;
			stream.Position = position;
			MsoBase.WriteUInt32(stream, m_uiSize);
			stream.Position = position2;
		}
	}

	public override void ParseStructure(Stream stream)
	{
		m_btReqWin32 = (byte)stream.ReadByte();
		m_btReqMac = (byte)stream.ReadByte();
		stream.Position += 18L;
		m_uiSize = MsoBase.ReadUInt32(stream);
		m_uiRefCount = MsoBase.ReadUInt32(stream);
		m_uiFileOffset = MsoBase.ReadUInt32(stream);
		m_btUsage = (byte)stream.ReadByte();
		m_btNameLength = (byte)stream.ReadByte();
		m_btUnused1 = (byte)stream.ReadByte();
		m_btUnused2 = (byte)stream.ReadByte();
		int num = 36;
		if (m_btNameLength > 0)
		{
			byte[] array = new byte[m_btNameLength];
			stream.Read(array, 0, m_btNameLength);
			m_strBlipName = Encoding.Unicode.GetString(array, 0, array.Length);
			num += m_btNameLength;
		}
		if (num == m_iLength)
		{
			m_msoPicture = null;
		}
		else if (Array.IndexOf(DEF_BITMAP_BLIPS, BlipType) != -1)
		{
			m_msoPicture = new MsoBitmapPicture(this, stream);
		}
		else if (Array.IndexOf(DEF_PICT_BLIPS, BlipType) != -1)
		{
			m_msoPicture = new MsoMetafilePicture(this, stream);
		}
	}

	protected override object InternalClone()
	{
		MsofbtBSE msofbtBSE = (MsofbtBSE)base.InternalClone();
		if (m_msoPicture != null)
		{
			msofbtBSE.m_msoPicture = m_msoPicture.Clone(msofbtBSE);
		}
		return msofbtBSE;
	}
}
