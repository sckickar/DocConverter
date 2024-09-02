using System;

namespace DocGen.Pdf.Security;

internal class PKCS12PasswordParameter : Asn1Encode
{
	private DerInteger m_iterations;

	private Asn1Octet m_octet;

	internal Number Iterations => m_iterations.Value;

	internal byte[] Octets => m_octet.GetOctets();

	internal PKCS12PasswordParameter(byte[] bytes, int iteration)
	{
		m_octet = new DerOctet(bytes);
		m_iterations = new DerInteger(iteration);
	}

	private PKCS12PasswordParameter(Asn1Sequence sequence)
	{
		if (sequence.Count != 2)
		{
			throw new ArgumentException("Invalid length in sequence");
		}
		m_octet = Asn1Octet.GetOctetString(sequence[0]);
		m_iterations = DerInteger.GetNumber(sequence[1]);
	}

	internal static PKCS12PasswordParameter GetPBEParameter(object obj)
	{
		if (obj is PKCS12PasswordParameter)
		{
			return (PKCS12PasswordParameter)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new PKCS12PasswordParameter((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry");
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(m_octet, m_iterations);
	}
}
