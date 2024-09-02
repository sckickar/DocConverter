using System;

namespace DocGen.Pdf.Security;

internal class CertificateIdentityHelper : Asn1Encode
{
	private Algorithms m_hash;

	private Asn1Octet m_issuerName;

	private Asn1Octet m_issuerKey;

	private DerInteger m_serialNumber;

	internal DerInteger SerialNumber => m_serialNumber;

	internal CertificateIdentityHelper()
	{
	}

	internal CertificateIdentityHelper(Algorithms hashAlgorithm, Asn1Octet issuerNameHash, Asn1Octet issuerKeyHash, DerInteger serialNumber)
	{
		m_hash = hashAlgorithm;
		m_issuerName = issuerNameHash;
		m_issuerKey = issuerKeyHash;
		m_serialNumber = serialNumber;
	}

	private CertificateIdentityHelper(Asn1Sequence sequence)
	{
		if (sequence.Count != 4)
		{
			throw new ArgumentException("Invalid length in sequence");
		}
		m_hash = Algorithms.GetAlgorithms(sequence[0]);
		m_issuerName = Asn1Octet.GetOctetString(sequence[1]);
		m_issuerKey = Asn1Octet.GetOctetString(sequence[2]);
		m_serialNumber = DerInteger.GetNumber(sequence[3]);
	}

	internal CertificateIdentityHelper GetCertificateIdentity(object obj)
	{
		if (obj == null || obj is CertificateIdentityHelper)
		{
			return (CertificateIdentityHelper)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new CertificateIdentityHelper((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(m_hash, m_issuerName, m_issuerKey, m_serialNumber);
	}
}
