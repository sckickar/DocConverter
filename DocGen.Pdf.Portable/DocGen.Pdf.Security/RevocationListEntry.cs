using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Security;

internal class RevocationListEntry : Asn1Encode
{
	internal Asn1Sequence m_sequence;

	internal DerInteger m_userCertificate;

	internal X509Time m_revocationDate;

	internal X509Extensions m_crlEntryExtensions;

	internal string m_serialNumber;

	internal DerInteger UserCertificate => m_userCertificate;

	internal RevocationListEntry(Asn1Sequence sequence)
	{
		if (sequence.Count >= 2 && sequence.Count <= 3)
		{
			m_sequence = sequence;
			m_userCertificate = DerInteger.GetNumber(sequence[0]);
			m_revocationDate = X509Time.GetTime(sequence[1]);
			m_serialNumber = PdfString.BytesToHex(m_userCertificate.Value.ToByteArray());
		}
	}

	public override Asn1 GetAsn1()
	{
		return m_sequence;
	}
}
