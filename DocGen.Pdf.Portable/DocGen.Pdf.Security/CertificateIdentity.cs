using System;

namespace DocGen.Pdf.Security;

internal class CertificateIdentity
{
	internal const string Sha1 = "1.3.14.3.2.26";

	private readonly CertificateIdentityHelper m_id;

	internal CertificateIdentityHelper ID => m_id;

	internal CertificateIdentity(string hashAlgorithm, X509Certificate issuerCert, Number serialNumber)
	{
		Algorithms algorithms = new Algorithms(new DerObjectID(hashAlgorithm), DerNull.Value);
		try
		{
			string iD = algorithms.ObjectID.ID;
			X509Name subject = SingnedCertificate.GetCertificate(Asn1.FromByteArray(issuerCert.GetTbsCertificate())).Subject;
			byte[] bytes = new MessageDigestFinder().CalculateDigest(iD, subject.GetEncoded());
			PublicKeyInformation publicKeyInformation = SubjectKeyID.CreateSubjectKeyID(issuerCert.GetPublicKey());
			byte[] bytes2 = new MessageDigestFinder().CalculateDigest(iD, publicKeyInformation.PublicKey.GetBytes());
			m_id = new CertificateIdentityHelper(algorithms, new DerOctet(bytes), new DerOctet(bytes2), new DerInteger(serialNumber));
		}
		catch (Exception)
		{
			throw new Exception("Invalid certificate ID");
		}
	}
}
