using System;

namespace DocGen.Pdf.Security;

internal class SignaturePrivateKey
{
	private ICipherParam m_key;

	private string m_hashAlgorithm;

	private string m_encryptionAlgorithm;

	internal SignaturePrivateKey(ICipherParam key, string hashAlgorithm)
	{
		m_key = key;
		MessageDigestAlgorithms messageDigestAlgorithms = new MessageDigestAlgorithms();
		m_hashAlgorithm = messageDigestAlgorithms.GetDigest(messageDigestAlgorithms.GetAllowedDigests(hashAlgorithm));
		if (key is RsaKeyParam)
		{
			m_encryptionAlgorithm = "RSA";
			return;
		}
		if (key is EllipticKeyParam)
		{
			m_encryptionAlgorithm = "ECDSA";
			return;
		}
		throw new ArgumentException("Invalid key");
	}

	internal SignaturePrivateKey(string hashAlgorithm, string encryptionAlgorithm)
	{
		MessageDigestAlgorithms messageDigestAlgorithms = new MessageDigestAlgorithms();
		m_hashAlgorithm = messageDigestAlgorithms.GetDigest(messageDigestAlgorithms.GetAllowedDigests(hashAlgorithm));
		if (encryptionAlgorithm == null)
		{
			m_encryptionAlgorithm = "RSA";
		}
		else
		{
			m_encryptionAlgorithm = encryptionAlgorithm;
		}
	}

	internal byte[] Sign(byte[] bytes)
	{
		string algorithm = m_hashAlgorithm + "with" + m_encryptionAlgorithm;
		ISigner signer = new SignerUtilities().GetSigner(algorithm);
		signer.Initialize(isSigning: true, m_key);
		signer.BlockUpdate(bytes, 0, bytes.Length);
		return signer.GenerateSignature();
	}

	internal string GetHashAlgorithm()
	{
		return m_hashAlgorithm;
	}

	internal string GetEncryptionAlgorithm()
	{
		return m_encryptionAlgorithm;
	}
}
