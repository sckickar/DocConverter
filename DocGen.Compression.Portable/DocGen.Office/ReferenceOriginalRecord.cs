using System;
using System.IO;
using System.Text;
using DocGen.Compression.Zip;

namespace DocGen.Office;

internal class ReferenceOriginalRecord : ReferenceRecord
{
	private ushort m_Id = 51;

	private string m_Libid = string.Empty;

	private Encoding m_type;

	private string m_name;

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
		int num = (int)ZipArchive.ReadUInt32(dirData);
		byte[] array = new byte[num];
		dirData.Read(array, 0, num);
		Libid = EncodingType.GetString(array, 0, array.Length);
	}

	internal override void SerializeRecord(Stream dirData)
	{
		dirData.Write(BitConverter.GetBytes(Id), 0, 2);
		byte[] bytes = EncodingType.GetBytes(Libid);
		dirData.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
		dirData.Write(bytes, 0, bytes.Length);
	}
}
