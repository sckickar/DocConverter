using System;

namespace DocGen.Pdf.Security;

internal class RSAKey : Asn1Encode
{
	private Number m_modulus;

	private Number m_publicExponent;

	private Number m_privateExponent;

	private Number m_prime1;

	private Number m_prime2;

	private Number m_exponent1;

	private Number m_exponent2;

	private Number m_coefficient;

	internal Number Modulus => m_modulus;

	internal Number PublicExponent => m_publicExponent;

	internal Number PrivateExponent => m_privateExponent;

	internal Number Prime1 => m_prime1;

	internal Number Prime2 => m_prime2;

	internal Number Exponent1 => m_exponent1;

	internal Number Exponent2 => m_exponent2;

	internal Number Coefficient => m_coefficient;

	internal RSAKey(Number modulus, Number publicExponent, Number privateExponent, Number prime1, Number prime2, Number exponent1, Number exponent2, Number coefficient)
	{
		m_modulus = modulus;
		m_publicExponent = publicExponent;
		m_privateExponent = privateExponent;
		m_prime1 = prime1;
		m_prime2 = prime2;
		m_exponent1 = exponent1;
		m_exponent2 = exponent2;
		m_coefficient = coefficient;
	}

	public RSAKey(Asn1Sequence sequence)
	{
		if (((DerInteger)sequence[0]).Value.IntValue != 0)
		{
			throw new ArgumentException("Invalid RSA key");
		}
		m_modulus = ((DerInteger)sequence[1]).Value;
		m_publicExponent = ((DerInteger)sequence[2]).Value;
		m_privateExponent = ((DerInteger)sequence[3]).Value;
		m_prime1 = ((DerInteger)sequence[4]).Value;
		m_prime2 = ((DerInteger)sequence[5]).Value;
		m_exponent1 = ((DerInteger)sequence[6]).Value;
		m_exponent2 = ((DerInteger)sequence[7]).Value;
		m_coefficient = ((DerInteger)sequence[8]).Value;
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(new DerInteger(0), new DerInteger(Modulus), new DerInteger(PublicExponent), new DerInteger(PrivateExponent), new DerInteger(Prime1), new DerInteger(Prime2), new DerInteger(Exponent1), new DerInteger(Exponent2), new DerInteger(Coefficient));
	}
}
