using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DocGen.Pdf.Security;

internal class X509Certificate2Signature
{
	private X509Certificate2 certificate;

	private string hashAlgorithm;

	private string encryptionAlgorithm;

	public X509Certificate2Signature(X509Certificate2 cert, string hashAlgorithm)
	{
		if (!cert.HasPrivateKey)
		{
			throw new ArgumentException("No private key.");
		}
		certificate = cert;
		MessageDigestAlgorithms messageDigestAlgorithms = new MessageDigestAlgorithms();
		this.hashAlgorithm = messageDigestAlgorithms.GetDigest(messageDigestAlgorithms.GetAllowedDigests(hashAlgorithm));
		encryptionAlgorithm = "RSA";
	}

	public virtual string GetHashAlgorithm()
	{
		return hashAlgorithm;
	}

	public virtual string GetEncryptionAlgorithm()
	{
		return encryptionAlgorithm;
	}

	public byte[] Sign(byte[] message)
	{
		byte[] result = null;
		using (RSA rSA = certificate.GetRSAPrivateKey())
		{
			switch (hashAlgorithm)
			{
			case "SHA256":
				result = rSA.SignData(message, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				break;
			case "SHA384":
				result = rSA.SignData(message, HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1);
				break;
			case "SHA512":
				result = rSA.SignData(message, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);
				break;
			case "SHA1":
				result = rSA.SignData(message, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
				break;
			}
		}
		return result;
	}
}
