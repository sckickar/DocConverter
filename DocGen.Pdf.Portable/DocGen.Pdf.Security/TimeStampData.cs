using System;
using System.Collections;

namespace DocGen.Pdf.Security;

internal class TimeStampData : Asn1Encode
{
	private readonly DerInteger m_version;

	private readonly DerObjectID m_policyId;

	private readonly MessageStamp m_messageImprint;

	private readonly DerInteger m_serialNumber;

	private readonly GeneralizedTime m_generalizedTime;

	private readonly DerPrecision m_precision;

	private readonly DerBoolean m_isOrdered;

	private readonly DerInteger m_nonce;

	private readonly DerName m_timeStampName;

	private readonly X509Extensions m_extensions;

	internal DerInteger Version => m_version;

	internal MessageStamp MessageImprint => m_messageImprint;

	internal DerObjectID Policy => m_policyId;

	internal DerInteger SerialNumber => m_serialNumber;

	internal DerPrecision Precision => m_precision;

	internal GeneralizedTime GeneralizedTime => m_generalizedTime;

	internal DerBoolean IsOrdered => m_isOrdered;

	internal DerInteger Nonce => m_nonce;

	internal DerName TimeStampName => m_timeStampName;

	internal X509Extensions Extensions => m_extensions;

	internal TimeStampData(Asn1Sequence sequence)
	{
		IEnumerator enumerator = sequence.GetEnumerator();
		enumerator.MoveNext();
		m_version = DerInteger.GetNumber(enumerator.Current);
		enumerator.MoveNext();
		m_policyId = DerObjectID.GetID(enumerator.Current);
		enumerator.MoveNext();
		if ((enumerator.Current as Asn1Sequence).Count != 2)
		{
			throw new ArgumentException("Invalid entry in sequence", "seq");
		}
		m_messageImprint = MessageStamp.GetMessageStamp(enumerator.Current);
		enumerator.MoveNext();
		m_serialNumber = DerInteger.GetNumber(enumerator.Current);
		enumerator.MoveNext();
		m_generalizedTime = DocGen.Pdf.Security.GeneralizedTime.GetGeneralizedTime(enumerator.Current);
		m_isOrdered = DerBoolean.False;
		while (enumerator.MoveNext())
		{
			Asn1 asn = (Asn1)enumerator.Current;
			if (asn is Asn1Tag)
			{
				DerTag derTag = (DerTag)asn;
				switch (derTag.TagNumber)
				{
				case 0:
					m_timeStampName = DerName.GetDerName(derTag, isExplicit: true);
					break;
				case 1:
					m_extensions = X509Extensions.GetInstance(derTag, explicitly: false);
					break;
				default:
					throw new ArgumentException("Invalid tag value");
				}
			}
			if (asn is DerSequence)
			{
				m_precision = DerPrecision.GetDerPrecision(asn);
			}
			if (asn is DerBoolean)
			{
				m_isOrdered = DerBoolean.GetBoolean(asn);
			}
			if (asn is DerInteger)
			{
				m_nonce = DerInteger.GetNumber(asn);
			}
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_version, m_policyId, m_messageImprint, m_serialNumber, m_generalizedTime);
		if (m_precision != null)
		{
			asn1EncodeCollection.Add(m_precision);
		}
		if (m_isOrdered != null && m_isOrdered.IsTrue)
		{
			asn1EncodeCollection.Add(m_isOrdered);
		}
		if (m_nonce != null)
		{
			asn1EncodeCollection.Add(m_nonce);
		}
		if (m_timeStampName != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: true, 0, m_timeStampName));
		}
		if (m_extensions != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: false, 1, m_extensions));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
