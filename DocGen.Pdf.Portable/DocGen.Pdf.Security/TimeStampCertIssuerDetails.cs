using System;

namespace DocGen.Pdf.Security;

internal class TimeStampCertIssuerDetails : Asn1Encode
{
	private DerNames m_issuerName;

	private DerInteger m_serialNumber;

	private DerBitString m_issuerId;

	internal static TimeStampCertIssuerDetails GetIssuerDetails(object obj)
	{
		if (obj == null || obj is TimeStampCertIssuerDetails)
		{
			return (TimeStampCertIssuerDetails)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new TimeStampCertIssuerDetails((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence : " + obj.GetType().Name, "obj");
	}

	private TimeStampCertIssuerDetails(Asn1Sequence sequence)
	{
		if (sequence.Count != 2 && sequence.Count != 3)
		{
			throw new ArgumentException("Invalid sequence size : " + sequence.Count);
		}
		m_issuerName = DerNames.GetDerNames(sequence[0]);
		m_serialNumber = DerInteger.GetNumber(sequence[1]);
		if (sequence.Count == 3)
		{
			m_issuerId = DerBitString.GetString(sequence[2]);
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_issuerName, m_serialNumber);
		if (m_issuerId != null)
		{
			asn1EncodeCollection.Add(m_issuerId);
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
