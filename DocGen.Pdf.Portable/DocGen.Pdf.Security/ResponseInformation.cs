using System;

namespace DocGen.Pdf.Security;

internal class ResponseInformation : Asn1Encode
{
	private static readonly DerInteger Version1 = new DerInteger(0);

	private bool m_versionPresent;

	private DerInteger m_version;

	private RevocationResponseIdentifier m_responderIdentifier;

	private GeneralizedTime m_producedTime;

	private Asn1Sequence m_sequence;

	private X509Extensions m_responseExtensions;

	internal Asn1Sequence Sequence => m_sequence;

	private ResponseInformation(Asn1Sequence sequence)
	{
		int num = 0;
		Asn1Encode asn1Encode = sequence[0];
		if (asn1Encode is Asn1Tag)
		{
			Asn1Tag asn1Tag = (Asn1Tag)asn1Encode;
			if (asn1Tag.TagNumber == 0)
			{
				m_versionPresent = true;
				m_version = DerInteger.GetNumber(asn1Tag, isExplicit: true);
				num++;
			}
			else
			{
				m_version = Version1;
			}
		}
		else
		{
			m_version = Version1;
		}
		RevocationResponseIdentifier revocationResponseIdentifier = new RevocationResponseIdentifier();
		m_responderIdentifier = revocationResponseIdentifier.GetResponseID(sequence[num++]);
		m_producedTime = (GeneralizedTime)sequence[num++];
		m_sequence = (Asn1Sequence)sequence[num++];
		if (sequence.Count > num)
		{
			m_responseExtensions = X509Extensions.GetInstance((Asn1Tag)sequence[num], explicitly: true);
		}
	}

	internal ResponseInformation()
	{
	}

	internal ResponseInformation GetInformation(object obj)
	{
		if (obj == null || obj is ResponseInformation)
		{
			return (ResponseInformation)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new ResponseInformation((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		if (m_versionPresent || !m_version.Equals(Version1))
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: true, 0, m_version));
		}
		asn1EncodeCollection.Add(m_responderIdentifier, m_producedTime, m_sequence);
		if (m_responseExtensions != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: true, 1, m_responseExtensions));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
