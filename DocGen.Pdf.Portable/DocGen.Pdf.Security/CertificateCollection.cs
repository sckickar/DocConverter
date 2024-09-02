using System;

namespace DocGen.Pdf.Security;

internal class CertificateCollection : Asn1Encode
{
	private readonly SignedCertificateCollection m_certificates;

	private readonly Algorithms m_id;

	private readonly DerBitString m_signature;

	private RevocationListEntry[] m_revokedCertificates;

	internal Algorithms SignatureAlgorithm => m_id;

	internal DerBitString Signature => m_signature;

	internal X509Time CurrentUpdate => m_certificates.CurrentUpdate;

	internal X509Time NextUpdate => m_certificates.NextUpdate;

	internal X509Name Issuer => m_certificates.Issuer;

	internal SignedCertificateCollection CertificateList => m_certificates;

	internal RevocationListEntry[] RevokedCertificates
	{
		get
		{
			if (m_revokedCertificates == null)
			{
				m_revokedCertificates = m_certificates.GetRevokedCertificates();
			}
			return m_revokedCertificates;
		}
	}

	private CertificateCollection(Asn1Sequence sequence)
	{
		if (sequence.Count != 3)
		{
			throw new ArgumentException("Invalid size in sequence");
		}
		m_certificates = SignedCertificateCollection.GetCertificateList(sequence[0]);
		m_id = Algorithms.GetAlgorithms(sequence[1]);
		m_signature = DerBitString.GetString(sequence[2]);
	}

	internal static CertificateCollection GetCertificateList(object obj)
	{
		if (obj is CertificateCollection)
		{
			return (CertificateCollection)obj;
		}
		if (obj != null)
		{
			return new CertificateCollection(Asn1Sequence.GetSequence(obj));
		}
		return null;
	}

	internal bool IsRevoked(X509Certificate certificate)
	{
		RevocationListEntry[] revokedCertificates = RevokedCertificates;
		if (revokedCertificates != null)
		{
			Number serialNumber = certificate.SerialNumber;
			for (int i = 0; i < revokedCertificates.Length; i++)
			{
				if (revokedCertificates[i].UserCertificate != null && revokedCertificates[i].UserCertificate.Value.Equals(serialNumber))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(m_certificates, m_id, m_signature);
	}
}
