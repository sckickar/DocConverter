using System;
using System.IO;
using System.Text;
using DocGen.Compression.Zip;

namespace DocGen.Office;

internal class ReferenceControlRecord : ReferenceRecord
{
	private ushort m_Id = 47;

	private string m_libTwiddled;

	private string m_extLibid;

	private Guid m_originalType;

	private uint m_cookie;

	private string m_name;

	private Encoding m_type;

	internal ushort Id => m_Id;

	internal string LibTwiddled
	{
		get
		{
			return m_libTwiddled;
		}
		set
		{
			m_libTwiddled = value;
		}
	}

	internal string ExtLibId
	{
		get
		{
			return m_extLibid;
		}
		set
		{
			m_extLibid = value;
		}
	}

	internal Guid OriginalType
	{
		get
		{
			return m_originalType;
		}
		set
		{
			m_originalType = value;
		}
	}

	internal uint Cookie
	{
		get
		{
			return m_cookie;
		}
		set
		{
			m_cookie = value;
		}
	}

	internal override string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
		}
	}

	internal override Encoding EncodingType
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	internal override void ParseRecord(Stream dirData)
	{
		dirData.Position += 4L;
		int num = (int)ZipArchive.ReadUInt32(dirData);
		byte[] array = new byte[num];
		dirData.Read(array, 0, num);
		LibTwiddled = EncodingType.GetString(array, 0, array.Length);
		dirData.Position += 6L;
		if (ZipArchive.ReadUInt16(dirData) == 22)
		{
			num = (int)ZipArchive.ReadUInt32(dirData);
			array = new byte[num];
			dirData.Read(array, 0, num);
			Name = EncodingType.GetString(array, 0, array.Length);
			if (ZipArchive.ReadUInt16(dirData) == 62)
			{
				num = (int)ZipArchive.ReadUInt32(dirData);
				dirData.Position += num + 2;
			}
		}
		dirData.Position += 4L;
		num = (int)ZipArchive.ReadUInt32(dirData);
		array = new byte[num];
		dirData.Read(array, 0, num);
		m_extLibid = EncodingType.GetString(array, 0, array.Length);
		dirData.Position += 6L;
		array = new byte[16];
		dirData.Read(array, 0, 16);
		OriginalType = new Guid(array);
		Cookie = ZipArchive.ReadUInt32(dirData);
	}

	internal override void SerializeRecord(Stream dirData)
	{
		dirData.Write(BitConverter.GetBytes(m_Id), 0, 2);
		byte[] bytes = EncodingType.GetBytes(LibTwiddled);
		dirData.Write(BitConverter.GetBytes(bytes.Length + 10), 0, 4);
		dirData.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirData.Write(bytes, 0, bytes.Length);
		dirData.Write(BitConverter.GetBytes(0), 0, 4);
		dirData.Write(BitConverter.GetBytes(0), 0, 2);
		dirData.Write(BitConverter.GetBytes(48), 0, 2);
		byte[] bytes2 = EncodingType.GetBytes(ExtLibId);
		dirData.Write(BitConverter.GetBytes(bytes2.Length + 30), 0, 4);
		dirData.Write(BitConverter.GetBytes(bytes2.Length), 0, 4);
		dirData.Write(bytes2, 0, bytes2.Length);
		dirData.Write(BitConverter.GetBytes(0), 0, 4);
		dirData.Write(BitConverter.GetBytes(0), 0, 2);
		dirData.Write(OriginalType.ToByteArray(), 0, 16);
		dirData.Write(BitConverter.GetBytes(Cookie), 0, 4);
	}
}
