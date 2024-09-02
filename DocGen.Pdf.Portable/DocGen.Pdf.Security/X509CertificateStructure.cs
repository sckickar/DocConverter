namespace DocGen.Pdf.Security;

internal class X509CertificateStructure : Asn1Encode
{
	private SingnedCertificate m_tbsCert;

	private Algorithms m_sigAlgID;

	private DerBitString m_sig;

	internal SingnedCertificate TbsCertificate => m_tbsCert;

	internal int Version => m_tbsCert.Version;

	internal DerInteger SerialNumber => m_tbsCert.SerialNumber;

	internal X509Name Issuer => m_tbsCert.Issuer;

	internal X509Time StartDate => m_tbsCert.StartDate;

	internal X509Time EndDate => m_tbsCert.EndDate;

	internal X509Name Subject => m_tbsCert.Subject;

	internal PublicKeyInformation SubjectPublicKeyInfo => m_tbsCert.SubjectPublicKeyInfo;

	internal Algorithms SignatureAlgorithm => m_sigAlgID;

	internal DerBitString Signature => m_sig;

	internal static X509CertificateStructure GetInstance(object obj)
	{
		if (obj is X509CertificateStructure)
		{
			return (X509CertificateStructure)obj;
		}
		if (obj != null)
		{
			return new X509CertificateStructure(Asn1Sequence.GetSequence(obj));
		}
		return null;
	}

	private X509CertificateStructure(Asn1Sequence seq)
	{
		m_tbsCert = SingnedCertificate.GetCertificate(seq[0]);
		m_sigAlgID = Algorithms.GetAlgorithms(seq[1]);
		m_sig = DerBitString.GetString(seq[2]);
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(m_tbsCert, m_sigAlgID, m_sig);
	}
}
