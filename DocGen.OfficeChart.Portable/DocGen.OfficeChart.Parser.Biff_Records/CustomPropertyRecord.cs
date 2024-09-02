using System;
using System.IO;
using System.Text;

namespace DocGen.OfficeChart.Parser.Biff_Records;

[Biff(TBIFFRecord.CustomProperty)]
[CLSCompliant(false)]
internal class CustomPropertyRecord : BiffRecordRaw
{
	private const int DEF_FIXED_SIZE = 7;

	private static readonly byte[] DEF_HEADER = new byte[2] { 0, 16 };

	private const int DEF_MAX_NAME_LENGTH = 255;

	private string m_strName;

	private string m_strValue;

	public string Name
	{
		get
		{
			return m_strName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("value - string cannot be empty.");
			}
			if (value.Length > 255)
			{
				throw new ArgumentException("value - string is too long.");
			}
			m_strName = value;
		}
	}

	public string Value
	{
		get
		{
			return m_strValue;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException("value - string cannot be empty.");
			}
			m_strValue = value;
		}
	}

	public CustomPropertyRecord()
	{
	}

	public CustomPropertyRecord(Stream stream, out int itemSize)
		: base(stream, out itemSize)
	{
	}

	public CustomPropertyRecord(int iReserve)
		: base(iReserve)
	{
	}

	public override void ParseStructure(DataProvider provider, int iOffset, int iLength, OfficeVersion version)
	{
		iOffset += 2;
		int stringLength = provider.ReadInt32(iOffset);
		iOffset += 4;
		int num = provider.ReadByte(iOffset);
		iOffset++;
		m_strName = provider.ReadString(iOffset, num, Encoding.UTF8, isUnicode: true);
		iOffset += num;
		m_strValue = provider.ReadString(iOffset, stringLength, Encoding.Unicode, isUnicode: true);
	}

	public override void InfillInternalData(DataProvider provider, int iOffset, OfficeVersion version)
	{
		m_iLength = GetStoreSize(version);
		provider.WriteBytes(iOffset, DEF_HEADER, 0, DEF_HEADER.Length);
		iOffset += DEF_HEADER.Length;
		int num = iOffset;
		iOffset += 4;
		iOffset++;
		byte[] bytes = Encoding.UTF8.GetBytes(m_strName);
		int num2 = bytes.Length;
		provider.WriteBytes(iOffset, bytes, 0, num2);
		provider.WriteByte(num + 4, (byte)num2);
		iOffset += num2;
		bytes = Encoding.Unicode.GetBytes(m_strValue);
		num2 = bytes.Length;
		provider.WriteBytes(iOffset, bytes, 0, num2);
		provider.WriteInt32(num, num2);
	}

	public override int GetStoreSize(OfficeVersion version)
	{
		return 7 + Encoding.UTF8.GetByteCount(m_strName) + Encoding.Unicode.GetByteCount(m_strValue);
	}
}
