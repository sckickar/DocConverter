using System;

namespace DocGen.Pdf.Security;

internal class RevocationListHelper
{
	private CertificateCollection m_certificateList;

	private string m_algorithm;

	private byte[] m_bytes;

	internal RevocationListHelper(CertificateCollection certificateList)
	{
		m_certificateList = certificateList;
		Algorithms signatureAlgorithm = certificateList.SignatureAlgorithm;
		Asn1Encode parameters = certificateList.SignatureAlgorithm.Parameters;
		if (parameters != null && !DerNull.Value.Equals(parameters))
		{
			if (signatureAlgorithm.ObjectID.ID == PKCSOIDs.RsaCrlAlgorithmIdntifier.ID)
			{
				Asn1Sequence sequence = Asn1Sequence.GetSequence(parameters);
				Algorithms algorithms = null;
				for (int i = 0; i != sequence.Count; i++)
				{
					Asn1Tag asn1Tag = (Asn1Tag)sequence[i];
					switch (asn1Tag.TagNumber)
					{
					case 0:
						algorithms = Algorithms.GetAlgorithms(asn1Tag);
						break;
					default:
						throw new ArgumentException("Invalid entry in sequence");
					case 1:
					case 2:
					case 3:
						break;
					}
				}
				string text = "";
				string iD = algorithms.ObjectID.ID;
				m_algorithm = (PKCSOIDs.Sha1WithRsaEncryption.ID.Equals(iD) ? "SHA1" : (NISTOIDs.SHA256.ID.Equals(iD) ? "SHA256" : (NISTOIDs.SHA384.ID.Equals(iD) ? "SHA384" : (NISTOIDs.SHA512.ID.Equals(iD) ? "SHA512" : ((!NISTOIDs.RipeMD160.ID.Equals(iD)) ? iD : "RIPEMD160"))))) + "withRSAandMGF1";
			}
			else
			{
				m_algorithm = signatureAlgorithm.ObjectID.ID;
			}
		}
		else
		{
			m_algorithm = signatureAlgorithm.ObjectID.ID;
		}
		if (m_certificateList.SignatureAlgorithm.Parameters != null)
		{
			m_bytes = m_certificateList.SignatureAlgorithm.Parameters.GetDerEncoded();
		}
		else
		{
			m_bytes = null;
		}
	}

	internal bool Validate(X509Certificate signerCertificate, X509Certificate issuerCertificate, DateTime signDate)
	{
		if (signDate == DateTime.MaxValue)
		{
			return false;
		}
		if (m_certificateList.Issuer.Equivalent(signerCertificate.IssuerDN) && signDate.CompareTo(m_certificateList.NextUpdate.ToDateTime()) < 0 && issuerCertificate != null && Validate(issuerCertificate.GetPublicKey()) && m_certificateList.IsRevoked(signerCertificate))
		{
			return true;
		}
		return false;
	}

	private bool Validate(CipherParameter publicKey)
	{
		if (!m_certificateList.SignatureAlgorithm.Equals(m_certificateList.CertificateList.Signature))
		{
			return false;
		}
		ISigner signer = new SignerUtilities().GetSigner(m_algorithm);
		signer.Initialize(isSigning: false, publicKey);
		byte[] derEncoded = m_certificateList.CertificateList.GetDerEncoded();
		signer.BlockUpdate(derEncoded, 0, derEncoded.Length);
		if (!signer.ValidateSignature(m_certificateList.Signature.GetBytes()))
		{
			return false;
		}
		return true;
	}
}
