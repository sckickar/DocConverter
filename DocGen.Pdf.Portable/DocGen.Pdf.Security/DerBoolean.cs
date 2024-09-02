using System;

namespace DocGen.Pdf.Security;

internal class DerBoolean : Asn1
{
	private byte m_value;

	internal static readonly DerBoolean False = new DerBoolean(value: false);

	internal static readonly DerBoolean True = new DerBoolean(value: true);

	internal bool IsTrue => m_value != 0;

	internal DerBoolean(byte[] bytes)
	{
		if (bytes.Length != 1)
		{
			throw new ArgumentException("Invalid length in bytes");
		}
		m_value = bytes[0];
	}

	private DerBoolean(bool value)
	{
		m_value = (byte)(value ? byte.MaxValue : 0);
	}

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(1, new byte[1] { m_value });
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		if (!(asn1 is DerBoolean derBoolean))
		{
			return false;
		}
		return IsTrue == derBoolean.IsTrue;
	}

	public override int GetHashCode()
	{
		return IsTrue.GetHashCode();
	}

	public override string ToString()
	{
		if (!IsTrue)
		{
			return "FALSE";
		}
		return "TRUE";
	}

	internal static DerBoolean GetBoolean(object obj)
	{
		if (obj == null || obj is DerBoolean)
		{
			return (DerBoolean)obj;
		}
		throw new ArgumentException("Invalid entry");
	}
}
