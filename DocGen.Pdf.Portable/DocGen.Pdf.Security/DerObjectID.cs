using System;
using System.IO;
using System.Text;

namespace DocGen.Pdf.Security;

internal class DerObjectID : Asn1
{
	private string m_id;

	private byte[] m_bytes;

	private static readonly DerObjectID[] m_objects = new DerObjectID[1024];

	private const long m_limit = 72057594037927808L;

	public string ID => m_id;

	internal DerObjectID(string id)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (!IsValidIdentifier(id))
		{
			throw new FormatException("Invalid ID");
		}
		m_id = id;
	}

	internal DerObjectID(DerObjectID id, string branchId)
	{
		if (!IsValidBranchID(branchId, 0))
		{
			throw new ArgumentException("Invalid branch ID");
		}
		m_id = id.ID + "." + branchId;
	}

	internal DerObjectID(byte[] bytes)
	{
		m_id = GetID(bytes);
		m_bytes = Asn1Constants.Clone(bytes);
	}

	public virtual DerObjectID Branch(string id)
	{
		return new DerObjectID(this, id);
	}

	private void WriteField(Stream stream, long fieldValue)
	{
		byte[] array = new byte[9];
		int num = 8;
		array[num] = (byte)(fieldValue & 0x7F);
		while (fieldValue >= 128)
		{
			fieldValue >>= 7;
			array[--num] = (byte)((fieldValue & 0x7F) | 0x80);
		}
		stream.Write(array, num, 9 - num);
	}

	private void WriteField(Stream stream, Number fieldValue)
	{
		int num = (fieldValue.BitLength + 6) / 7;
		if (num == 0)
		{
			stream.WriteByte(0);
			return;
		}
		Number number = fieldValue;
		byte[] array = new byte[num];
		for (int num2 = num - 1; num2 >= 0; num2--)
		{
			array[num2] = (byte)(((uint)number.IntValue & 0x7Fu) | 0x80u);
			number = number.ShiftRight(7);
		}
		array[num - 1] &= 127;
		stream.Write(array, 0, array.Length);
	}

	private void GetOutput(MemoryStream stream)
	{
		ObjectIdentityToken objectIdentityToken = new ObjectIdentityToken(m_id);
		string s = objectIdentityToken.NextToken();
		int num = int.Parse(s) * 40;
		s = objectIdentityToken.NextToken();
		if (s.Length <= 18)
		{
			WriteField(stream, num + long.Parse(s));
		}
		else
		{
			WriteField(stream, new Number(s).Add(Number.ValueOf(num)));
		}
		while (objectIdentityToken.HasMoreTokens)
		{
			s = objectIdentityToken.NextToken();
			if (s.Length <= 18)
			{
				WriteField(stream, long.Parse(s));
			}
			else
			{
				WriteField(stream, new Number(s));
			}
		}
	}

	internal byte[] GetBytes()
	{
		lock (this)
		{
			if (m_bytes == null)
			{
				MemoryStream memoryStream = new MemoryStream();
				GetOutput(memoryStream);
				m_bytes = memoryStream.ToArray();
			}
		}
		return m_bytes;
	}

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(6, GetBytes());
	}

	public override int GetHashCode()
	{
		return m_id.GetHashCode();
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		if (!(asn1 is DerObjectID derObjectID))
		{
			return false;
		}
		return m_id.Equals(derObjectID.m_id);
	}

	public override string ToString()
	{
		return m_id;
	}

	internal static DerObjectID GetID(object obj)
	{
		if (obj == null || obj is DerObjectID)
		{
			return (DerObjectID)obj;
		}
		if (obj is byte[])
		{
			return FromOctetString((byte[])obj);
		}
		throw new ArgumentException("Illegal object");
	}

	internal static DerObjectID GetID(Asn1Tag obj, bool isExplicit)
	{
		return GetID(obj.GetObject());
	}

	private static bool IsValidBranchID(string branchID, int start)
	{
		bool flag = false;
		int num = branchID.Length;
		while (--num >= start)
		{
			char c = branchID[num];
			if ('0' <= c && c <= '9')
			{
				flag = true;
				continue;
			}
			if (c == '.')
			{
				if (!flag)
				{
					return false;
				}
				flag = false;
				continue;
			}
			return false;
		}
		return flag;
	}

	private static bool IsValidIdentifier(string id)
	{
		if (id.Length < 3 || id[1] != '.')
		{
			return false;
		}
		char c = id[0];
		if (c < '0' || c > '2')
		{
			return false;
		}
		return IsValidBranchID(id, 2);
	}

	private static string GetID(byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		long num = 0L;
		Number number = null;
		bool flag = true;
		for (int i = 0; i != bytes.Length; i++)
		{
			int num2 = bytes[i];
			if (num <= 72057594037927808L)
			{
				num += num2 & 0x7F;
				if ((num2 & 0x80) == 0)
				{
					if (flag)
					{
						if (num < 40)
						{
							stringBuilder.Append('0');
						}
						else if (num < 80)
						{
							stringBuilder.Append('1');
							num -= 40;
						}
						else
						{
							stringBuilder.Append('2');
							num -= 80;
						}
						flag = false;
					}
					stringBuilder.Append('.');
					stringBuilder.Append(num);
					num = 0L;
				}
				else
				{
					num <<= 7;
				}
				continue;
			}
			if (number == null)
			{
				number = Number.ValueOf(num);
			}
			number = number.Or(Number.ValueOf(num2 & 0x7F));
			if ((num2 & 0x80) == 0)
			{
				if (flag)
				{
					stringBuilder.Append('2');
					number = number.Subtract(Number.ValueOf(80L));
					flag = false;
				}
				stringBuilder.Append('.');
				stringBuilder.Append(number);
				number = null;
				num = 0L;
			}
			else
			{
				number = number.ShiftLeft(7);
			}
		}
		return stringBuilder.ToString();
	}

	internal static DerObjectID FromOctetString(byte[] bytes)
	{
		int num = Asn1Constants.GetHashCode(bytes) & 0x3FF;
		lock (m_objects)
		{
			DerObjectID derObjectID = m_objects[num];
			if (derObjectID != null && Asn1Constants.AreEqual(bytes, derObjectID.GetBytes()))
			{
				return derObjectID;
			}
			return m_objects[num] = new DerObjectID(bytes);
		}
	}
}
