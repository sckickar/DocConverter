using System;
using System.IO;
using System.Text;

namespace DocGen.Pdf.Security;

internal class Asn1Identifier : Asn1
{
	private char m_tokenSeparator = '.';

	private string m_id;

	private byte[] m_body;

	private MemoryStream m_stream;

	internal byte[] Body => m_body;

	public Asn1Identifier(string id)
		: base(Asn1UniversalTags.ObjectIdentifier)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("Oid");
		}
		m_id = id;
	}

	public Asn1Identifier(byte[] bytes)
		: base(Asn1UniversalTags.ObjectIdentifier)
	{
		m_id = CreateIdentifier(bytes);
		m_body = ((bytes == null) ? null : ((byte[])bytes.Clone()));
	}

	public Asn1Identifier GetIdentifier(object obj)
	{
		return (Asn1Identifier)obj;
	}

	private byte[] ToArray()
	{
		string[] array = m_id.Split(new char[1] { m_tokenSeparator });
		int num = int.Parse(array[0]);
		int num2 = int.Parse(array[1]);
		MemoryStream memoryStream = new MemoryStream();
		AppendField(num * 40 + num2, memoryStream);
		for (int i = 2; i < array.Length; i++)
		{
			string text = array[i];
			if (text.Length < 18)
			{
				AppendField(int.Parse(text), memoryStream);
			}
			else
			{
				AppendField(text, memoryStream);
			}
		}
		byte[] result = memoryStream.ToArray();
		memoryStream.Dispose();
		return result;
	}

	private void AppendField(long value, Stream stream)
	{
		if (value >= 128)
		{
			if (value >= 16384)
			{
				if (value >= 2097152)
				{
					if (value >= 268435456)
					{
						if (value >= 34359738368L)
						{
							if (value >= 4398046511104L)
							{
								if (value >= 562949953421312L)
								{
									if (value >= 72057594037927936L)
									{
										stream.WriteByte((byte)((value >> 56) | 0x80));
									}
									stream.WriteByte((byte)((value >> 49) | 0x80));
								}
								stream.WriteByte((byte)((value >> 42) | 0x80));
							}
							stream.WriteByte((byte)((value >> 35) | 0x80));
						}
						stream.WriteByte((byte)((value >> 28) | 0x80));
					}
					stream.WriteByte((byte)((value >> 21) | 0x80));
				}
				stream.WriteByte((byte)((value >> 14) | 0x80));
			}
			stream.WriteByte((byte)((value >> 7) | 0x80));
		}
		stream.WriteByte((byte)(value & 0x7F));
	}

	private void AppendField(string value, Stream stream)
	{
		int num = (Encoding.UTF8.GetBytes(value).Length + 6) / 7;
		if (num == 0)
		{
			stream.WriteByte(0);
			return;
		}
		int num2 = Convert.ToInt16(value);
		byte[] array = new byte[num];
		for (int num3 = num - 1; num3 >= 0; num3--)
		{
			array[num3] = (byte)(((uint)num2 & 0x7Fu) | 0x80u);
			num2 >>= 7;
		}
		array[num - 1] &= 127;
		stream.Write(array, 0, array.Length);
	}

	internal byte[] Asn1Encode()
	{
		return Asn1Encode(ToArray());
	}

	protected override bool IsEquals(Asn1 asn1Object)
	{
		throw new NotImplementedException();
	}

	public override int GetHashCode()
	{
		throw new NotImplementedException();
	}

	internal override void Encode(DerStream derStr)
	{
		derStr.WriteEncoded(6, ToArray());
	}

	private string CreateIdentifier(byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		long num = 0L;
		bool flag = true;
		for (int i = 0; i != bytes.Length; i++)
		{
			int num2 = bytes[i];
			if (num >= 36028797018963968L)
			{
				continue;
			}
			num = num * 128 + (num2 & 0x7F);
			if (((uint)num2 & 0x80u) != 0)
			{
				continue;
			}
			if (flag)
			{
				switch ((int)num / 40)
				{
				case 0:
					stringBuilder.Append('0');
					break;
				case 1:
					stringBuilder.Append('1');
					num -= 40;
					break;
				default:
					stringBuilder.Append('2');
					num -= 80;
					break;
				}
				flag = false;
			}
			stringBuilder.Append('.');
			stringBuilder.Append(num);
			num = 0L;
		}
		return stringBuilder.ToString();
	}
}
