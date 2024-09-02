using System;
using System.Collections;

namespace DocGen.Pdf.Security;

internal class SignedDetails : Asn1Encode
{
	private DerInteger m_version;

	private Asn1Set m_digestAlgorithms;

	private ContentInformation m_contentInformation;

	private Asn1Set m_certificates;

	private Asn1Set m_crls;

	private Asn1Set m_signerInformation;

	private bool m_certsBer;

	private bool m_crlsBerObject;

	internal ContentInformation ContentInformation => m_contentInformation;

	internal Asn1Set SignerInformation => m_signerInformation;

	internal SignedDetails(Asn1Sequence seq)
	{
		IEnumerator enumerator = seq.GetEnumerator();
		enumerator.MoveNext();
		m_version = (DerInteger)enumerator.Current;
		enumerator.MoveNext();
		m_digestAlgorithms = (Asn1Set)enumerator.Current;
		enumerator.MoveNext();
		m_contentInformation = DocGen.Pdf.Security.ContentInformation.GetInformation(enumerator.Current);
		while (enumerator.MoveNext())
		{
			Asn1 asn = (Asn1)enumerator.Current;
			if (asn is Asn1Tag)
			{
				Asn1Tag asn1Tag = (Asn1Tag)asn;
				switch (asn1Tag.TagNumber)
				{
				case 0:
					m_certsBer = asn1Tag is BerTag;
					m_certificates = Asn1Set.GetAsn1Set(asn1Tag, isExplicit: false);
					break;
				case 1:
					m_crlsBerObject = asn1Tag is BerTag;
					m_crls = Asn1Set.GetAsn1Set(asn1Tag, isExplicit: false);
					break;
				default:
					throw new ArgumentException("Invalid entry in tag value : " + asn1Tag.TagNumber);
				}
			}
			else
			{
				m_signerInformation = (Asn1Set)asn;
			}
		}
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_version, m_digestAlgorithms, m_contentInformation);
		if (m_certificates != null)
		{
			if (m_certsBer)
			{
				asn1EncodeCollection.Add(new BerTag(IsExplicit: false, 0, m_certificates));
			}
			else
			{
				asn1EncodeCollection.Add(new DerTag(isExplicit: false, 0, m_certificates));
			}
		}
		if (m_crls != null)
		{
			if (m_crlsBerObject)
			{
				asn1EncodeCollection.Add(new BerTag(IsExplicit: false, 1, m_crls));
			}
			else
			{
				asn1EncodeCollection.Add(new DerTag(isExplicit: false, 1, m_crls));
			}
		}
		asn1EncodeCollection.Add(m_signerInformation);
		return new BerSequence(asn1EncodeCollection);
	}
}
