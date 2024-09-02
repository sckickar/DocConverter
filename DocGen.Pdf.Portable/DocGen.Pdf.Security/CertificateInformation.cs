namespace DocGen.Pdf.Security;

internal class CertificateInformation : Asn1Encode
{
	private X509Name m_name;

	private DerInteger m_serialNumber;

	internal X509Name Name => m_name;

	internal DerInteger SerialNumber => m_serialNumber;

	internal static CertificateInformation GetCertificateInformation(object obj)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj is CertificateInformation result)
		{
			return result;
		}
		return new CertificateInformation(Asn1Sequence.GetSequence(obj));
	}

	internal CertificateInformation(Asn1Sequence sequence)
	{
		m_name = X509Name.GetName(sequence[0]);
		m_serialNumber = (DerInteger)sequence[1];
	}

	public override Asn1 GetAsn1()
	{
		return new DerSequence(m_name, m_serialNumber);
	}
}
