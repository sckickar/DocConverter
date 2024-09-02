using System;

namespace DocGen.Pdf.Security;

internal class OneTimeResponseHelper : Asn1Encode
{
	private CertificateIdentityHelper m_id;

	private OcspStatus m_certificateStatus;

	private GeneralizedTime m_currentUpdate;

	private GeneralizedTime m_nextUpdate;

	private X509Extensions m_extensions;

	internal CertificateIdentityHelper CertificateIdentification => m_id;

	internal GeneralizedTime NextUpdate => m_nextUpdate;

	internal GeneralizedTime CurrentUpdate => m_currentUpdate;

	internal OcspStatus Status => m_certificateStatus;

	public OneTimeResponseHelper(Asn1Sequence sequence)
	{
		CertificateIdentityHelper certificateIdentityHelper = new CertificateIdentityHelper();
		OcspStatus ocspStatus = new OcspStatus();
		m_id = certificateIdentityHelper.GetCertificateIdentity(sequence[0]);
		m_certificateStatus = ocspStatus.GetStatus(sequence[1]);
		m_currentUpdate = (GeneralizedTime)sequence[2];
		if (sequence.Count > 4)
		{
			m_nextUpdate = GeneralizedTime.GetGeneralizedTime((Asn1Tag)sequence[3], isExplicit: true);
			m_extensions = X509Extensions.GetInstance((Asn1Tag)sequence[4], explicitly: true);
		}
		else if (sequence.Count > 3)
		{
			Asn1Tag asn1Tag = (Asn1Tag)sequence[3];
			if (asn1Tag.m_tagNumber == 0)
			{
				m_nextUpdate = GeneralizedTime.GetGeneralizedTime(asn1Tag, isExplicit: true);
			}
			else
			{
				m_extensions = X509Extensions.GetInstance(asn1Tag, explicitly: true);
			}
		}
	}

	internal OneTimeResponseHelper()
	{
	}

	internal OneTimeResponseHelper GetResponse(object obj)
	{
		if (obj == null || obj is OneTimeResponseHelper)
		{
			return (OneTimeResponseHelper)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new OneTimeResponseHelper((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection(m_id, m_certificateStatus, m_currentUpdate);
		if (m_nextUpdate != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: true, 0, m_nextUpdate));
		}
		if (m_extensions != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: true, 1, m_extensions));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
