using System;

namespace DocGen.Pdf.Security;

internal class TimeStampCertificate : Asn1Encode
{
	private Asn1Sequence m_certificates;

	private Asn1Sequence m_policies;

	internal TimeStampIdentifier[] Certificates
	{
		get
		{
			TimeStampIdentifier[] array = new TimeStampIdentifier[m_certificates.Count];
			for (int i = 0; i != m_certificates.Count; i++)
			{
				array[i] = TimeStampIdentifier.GetTimeStampCertID(m_certificates[i]);
			}
			return array;
		}
	}

	internal static TimeStampCertificate GetTimeStanpCertificate(object obj)
	{
		if (obj == null || obj is TimeStampCertificate)
		{
			return (TimeStampCertificate)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new TimeStampCertificate((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence : " + obj.GetType().Name + ".");
	}

	internal TimeStampCertificate(Asn1Sequence sequence)
	{
		if (sequence.Count < 1 || sequence.Count > 2)
		{
			throw new ArgumentException("Invalid sequence size " + sequence.Count);
		}
		m_certificates = Asn1Sequence.GetSequence(sequence[0]);
		if (sequence.Count > 1)
		{
			m_policies = Asn1Sequence.GetSequence(sequence[1]);
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_certificates);
		if (m_policies != null)
		{
			asn1EncodeCollection.Add(m_policies);
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
