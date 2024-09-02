using System;

namespace DocGen.Pdf.Security;

internal class RSAPublicKey : Asn1Encode
{
	private Number m_modulus;

	private Number m_publicExponent;

	internal Number Modulus => m_modulus;

	internal Number PublicExponent => m_publicExponent;

	internal static RSAPublicKey GetPublicKey(object obj)
	{
		if (obj == null || obj is RSAPublicKey)
		{
			return (RSAPublicKey)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new RSAPublicKey((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry");
	}

	internal RSAPublicKey(Number modulus, Number publicExponent)
	{
		m_modulus = modulus;
		m_publicExponent = publicExponent;
	}

	private RSAPublicKey(Asn1Sequence sequence)
	{
		m_modulus = DerInteger.GetNumber(sequence[0]).PositiveValue;
		m_publicExponent = DerInteger.GetNumber(sequence[1]).PositiveValue;
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(new DerInteger(Modulus), new DerInteger(PublicExponent));
	}
}
