using System;
using System.IO;
using System.Text;
using DocGen.Compression.Zip;

namespace DocGen.Office;

internal class ReferenceRegisterRecord : ReferenceRecord
{
	private ushort m_Id = 13;

	private string m_Libid;

	private string m_name;

	private Encoding m_type;

	internal ushort Id => m_Id;

	internal string Libid
	{
		get
		{
			return m_Libid;
		}
		set
		{
			m_Libid = value;
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
		Libid = encodingType.GetString(array, 0, array.Length);
		dirData.Position += 6L;
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
		dirData.Write(BitConverter.GetBytes(m_Id), 0, 2);
		byte[] bytes2 = EncodingType.GetBytes(Libid);
		dirData.Write(BitConverter.GetBytes(bytes2.Length + 10), 0, 4);
		dirData.Write(BitConverter.GetBytes(bytes2.Length), 0, 4);
		dirData.Write(bytes2, 0, bytes2.Length);
		dirData.Write(BitConverter.GetBytes(0L), 0, 6);
	}
}
