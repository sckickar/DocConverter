using System;

namespace DocGen.Pdf.Security;

internal class X509Extension
{
	internal bool m_critical;

	internal Asn1Octet m_value;

	internal bool IsCritical => m_critical;

	internal Asn1Octet Value => m_value;

	internal X509Extension(bool critical, Asn1Octet value)
	{
		m_critical = critical;
		m_value = value;
	}

	internal Asn1Encode GetParsedValue()
	{
		return ConvertValueToObject(this);
	}

	public override int GetHashCode()
	{
		int asn1Hash = Value.GetAsn1Hash();
		if (!IsCritical)
		{
			return ~asn1Hash;
		}
		return asn1Hash;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is X509Extension x509Extension))
		{
			return false;
		}
		if (Value.Equals(x509Extension.Value))
		{
			return IsCritical == x509Extension.IsCritical;
		}
		return false;
	}

	public static Asn1 ConvertValueToObject(X509Extension ext)
	{
		try
		{
			return Asn1.FromByteArray(ext.Value.GetOctets());
		}
		catch (Exception innerException)
		{
			throw new ArgumentException("can't convert extension", innerException);
		}
	}
}
