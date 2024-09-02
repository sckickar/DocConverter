using System;
using System.IO;
using System.Text;
using DocGen.Compression.Zip;

namespace DocGen.Office;

internal class ReferenceProjectRecord : ReferenceRecord
{
	private ushort m_Id = 14;

	private string m_LibAbsolute = string.Empty;

	private string m_LibRelative = string.Empty;

	private uint m_MajorVersion;

	private ushort m_MinorVersion = 1;

	private Encoding m_type;

	private string m_name;

	internal ushort Id => m_Id;

	internal string LibAbsolute
	{
		get
		{
			return m_LibAbsolute;
		}
		set
		{
			m_LibAbsolute = value;
		}
	}

	internal string LibRelative
	{
		get
		{
			return m_LibRelative;
		}
		set
		{
			m_LibRelative = value;
		}
	}

	internal uint MajorVersion
	{
		get
		{
			return m_MajorVersion;
		}
		set
		{
			m_MajorVersion = value;
		}
	}

	internal ushort MinorVersion
	{
		get
		{
			return m_MinorVersion;
		}
		set
		{
			m_MinorVersion = value;
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
		Encoding encodingType = EncodingType;
		int num = (int)ZipArchive.ReadUInt32(dirData);
		byte[] array = new byte[num];
		dirData.Read(array, 0, num);
		LibAbsolute = encodingType.GetString(array, 0, array.Length);
		num = (int)ZipArchive.ReadUInt32(dirData);
		array = new byte[num];
		dirData.Read(array, 0, num);
		LibRelative = encodingType.GetString(array, 0, array.Length);
		MajorVersion = ZipArchive.ReadUInt32(dirData);
		MinorVersion = ZipArchive.ReadUInt16(dirData);
	}

	internal override void SerializeRecord(Stream dirData)
	{
		if (!string.IsNullOrEmpty(Name))
		{
			byte[] bytes = EncodingType.GetBytes(Name);
			dirData.Write(BitConverter.GetBytes(22), 0, 2);
			dirData.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
			dirData.Write(bytes, 0, bytes.Length);
			bytes = Encoding.Unicode.GetBytes(Name);
			dirData.Write(BitConverter.GetBytes(62), 0, 2);
			dirData.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
			dirData.Write(bytes, 0, bytes.Length);
		}
		dirData.Write(BitConverter.GetBytes(14), 0, 2);
		byte[] bytes2 = EncodingType.GetBytes(LibAbsolute);
		byte[] bytes3 = EncodingType.GetBytes(LibRelative);
		dirData.Write(BitConverter.GetBytes(bytes2.Length + bytes3.Length + 16), 0, 4);
		dirData.Write(BitConverter.GetBytes(bytes2.Length), 0, 4);
		dirData.Write(bytes2, 0, bytes2.Length);
		dirData.Write(BitConverter.GetBytes(bytes3.Length), 0, 4);
		dirData.Write(bytes3, 0, bytes3.Length);
		dirData.Write(BitConverter.GetBytes(MajorVersion), 0, 4);
		dirData.Write(BitConverter.GetBytes(MinorVersion), 0, 2);
	}
}
