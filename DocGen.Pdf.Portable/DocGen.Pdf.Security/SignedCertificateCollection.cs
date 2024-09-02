using System;

namespace DocGen.Pdf.Security;

internal class SignedCertificateCollection : Asn1Encode
{
	private Asn1Sequence m_sequence;

	private DerInteger m_version;

	private Algorithms m_signature;

	private X509Name m_issuerName;

	private X509Time m_currentUpdate;

	private X509Time m_nextUpdate;

	private Asn1Sequence m_revokedCertificates;

	private X509Extensions m_crls;

	internal X509Name Issuer => m_issuerName;

	internal X509Time CurrentUpdate => m_currentUpdate;

	internal X509Time NextUpdate => m_nextUpdate;

	internal Algorithms Signature => m_signature;

	internal SignedCertificateCollection(Asn1Sequence sequence)
	{
		if (sequence.Count < 3 || sequence.Count > 7)
		{
			throw new ArgumentException("Invalid size in sequence");
		}
		int num = 0;
		m_sequence = sequence;
		if (sequence[num] is DerInteger)
		{
			m_version = DerInteger.GetNumber(sequence[num++]);
		}
		else
		{
			m_version = new DerInteger(0);
		}
		m_signature = Algorithms.GetAlgorithms(sequence[num++]);
		m_issuerName = X509Name.GetName(sequence[num++]);
		m_currentUpdate = X509Time.GetTime(sequence[num++]);
		if (num < sequence.Count && (sequence[num] is DerUtcTime || sequence[num] is GeneralizedTime || sequence[num] is X509Time))
		{
			m_nextUpdate = X509Time.GetTime(sequence[num++]);
		}
		if (num < sequence.Count && !(sequence[num] is DerTag))
		{
			m_revokedCertificates = Asn1Sequence.GetSequence(sequence[num++]);
		}
		if (num < sequence.Count && sequence[num] is DerTag)
		{
			m_crls = X509Extensions.GetInstance(sequence[num]);
		}
	}

	internal static SignedCertificateCollection GetCertificateList(object obj)
	{
		if (obj != null)
		{
			if (obj is SignedCertificateCollection result)
			{
				return result;
			}
			if (obj is Asn1Sequence)
			{
				return new SignedCertificateCollection((Asn1Sequence)obj);
			}
		}
		return null;
	}

	internal RevocationListEntry[] GetRevokedCertificates()
	{
		if (m_revokedCertificates == null)
		{
			return new RevocationListEntry[0];
		}
		RevocationListEntry[] array = new RevocationListEntry[m_revokedCertificates.Count];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new RevocationListEntry(Asn1Sequence.GetSequence(m_revokedCertificates[i]));
		}
		return array;
	}

	internal X509Extensions GetExtensions()
	{
		return m_crls;
	}

	public override Asn1 GetAsn1()
	{
		return m_sequence;
	}
}
