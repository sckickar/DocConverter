namespace DocGen.Pdf.Security;

internal class SingnedCertificate : Asn1Encode
{
	internal Asn1Sequence m_sequence;

	internal DerInteger m_version;

	internal DerInteger m_serialNumber;

	internal Algorithms m_signature;

	internal X509Name m_issuer;

	internal X509Time m_startDate;

	internal X509Time m_endDate;

	internal X509Name m_subject;

	internal PublicKeyInformation m_publicKeyInformation;

	internal DerBitString m_issuerID;

	internal DerBitString m_subjectID;

	internal X509Extensions m_extensions;

	internal int Version => m_version.Value.IntValue + 1;

	internal DerInteger SerialNumber => m_serialNumber;

	internal Algorithms Signature => m_signature;

	internal X509Name Issuer => m_issuer;

	internal X509Time StartDate => m_startDate;

	internal X509Time EndDate => m_endDate;

	internal X509Name Subject => m_subject;

	internal PublicKeyInformation SubjectPublicKeyInfo => m_publicKeyInformation;

	internal DerBitString IssuerUniqueID => m_issuerID;

	internal DerBitString SubjectUniqueID => m_subjectID;

	internal X509Extensions Extensions => m_extensions;

	internal static SingnedCertificate GetCertificate(object obj)
	{
		if (obj is SingnedCertificate)
		{
			return (SingnedCertificate)obj;
		}
		if (obj != null)
		{
			return new SingnedCertificate(Asn1Sequence.GetSequence(obj));
		}
		return null;
	}

	internal SingnedCertificate(Asn1Sequence sequence)
	{
		int num = 0;
		m_sequence = sequence;
		if (sequence[0] is DerTag || sequence[0] is Asn1Tag)
		{
			m_version = DerInteger.GetNumber((Asn1Tag)sequence[0], isExplicit: true);
		}
		else
		{
			num = -1;
			m_version = new DerInteger(0);
		}
		m_serialNumber = DerInteger.GetNumber(sequence[num + 1]);
		m_signature = Algorithms.GetAlgorithms(sequence[num + 2]);
		m_issuer = X509Name.GetName(sequence[num + 3]);
		Asn1Sequence asn1Sequence = (Asn1Sequence)sequence[num + 4];
		m_startDate = X509Time.GetTime(asn1Sequence[0]);
		m_endDate = X509Time.GetTime(asn1Sequence[1]);
		m_subject = X509Name.GetName(sequence[num + 5]);
		m_publicKeyInformation = PublicKeyInformation.GetPublicKeyInformation(sequence[num + 6]);
		for (int num2 = sequence.Count - (num + 6) - 1; num2 > 0; num2--)
		{
			Asn1Tag asn1Tag = sequence[num + 6 + num2] as Asn1Tag;
			switch (asn1Tag.TagNumber)
			{
			case 1:
				m_issuerID = DerBitString.GetString(asn1Tag, isExplicit: false);
				break;
			case 2:
				m_subjectID = DerBitString.GetString(asn1Tag, isExplicit: false);
				break;
			case 3:
				m_extensions = X509Extensions.GetInstance(asn1Tag);
				break;
			}
		}
	}

	public override Asn1 GetAsn1()
	{
		return m_sequence;
	}
}
