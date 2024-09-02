using System;

namespace DocGen.Pdf.Security;

internal class TimeStampIdentifier
{
	private Asn1Octet m_hash;

	private TimeStampCertIssuerDetails m_issuerDetail;

	internal static TimeStampIdentifier GetTimeStampCertID(object obj)
	{
		if (obj == null || obj is TimeStampIdentifier)
		{
			return (TimeStampIdentifier)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new TimeStampIdentifier((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence : " + obj.GetType().Name + ".");
	}

	internal TimeStampIdentifier(Asn1Sequence sequence)
	{
		if (sequence.Count < 1 || sequence.Count > 2)
		{
			throw new ArgumentException("Invalid sequence size : " + sequence.Count);
		}
		m_hash = Asn1Octet.GetOctetString(sequence[0]);
		if (sequence.Count > 1)
		{
			m_issuerDetail = TimeStampCertIssuerDetails.GetIssuerDetails(sequence[1]);
		}
	}
}
