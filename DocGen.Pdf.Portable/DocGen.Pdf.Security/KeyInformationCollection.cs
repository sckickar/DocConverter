using System;

namespace DocGen.Pdf.Security;

internal class KeyInformationCollection
{
	internal static KeyInformation CreatePrivateKeyInfo(CipherParameter key)
	{
		if (key is RsaKeyParam)
		{
			Algorithms algorithms = new Algorithms(PKCSOIDs.RsaEncryption, DerNull.Value);
			RSAKey rSAKey;
			if (key is RsaPrivateKeyParam)
			{
				RsaPrivateKeyParam rsaPrivateKeyParam = (RsaPrivateKeyParam)key;
				rSAKey = new RSAKey(rsaPrivateKeyParam.Modulus, rsaPrivateKeyParam.PublicExponent, rsaPrivateKeyParam.Exponent, rsaPrivateKeyParam.P, rsaPrivateKeyParam.Q, rsaPrivateKeyParam.DP, rsaPrivateKeyParam.DQ, rsaPrivateKeyParam.QInv);
			}
			else
			{
				RsaKeyParam rsaKeyParam = (RsaKeyParam)key;
				rSAKey = new RSAKey(rsaKeyParam.Modulus, Number.Zero, rsaKeyParam.Exponent, Number.Zero, Number.Zero, Number.Zero, Number.Zero, Number.Zero);
			}
			return new KeyInformation(algorithms, rSAKey.GetAsn1());
		}
		throw new ArgumentException("Invalid Key");
	}

	internal static KeyInformation CreatePrivateKeyInfo(char[] passPhrase, EncryptedPrivateKey encInfo)
	{
		return CreatePrivateKeyInfo(passPhrase, isPkcs12empty: false, encInfo);
	}

	internal static KeyInformation CreatePrivateKeyInfo(char[] passPhrase, bool isPkcs12empty, EncryptedPrivateKey encInfo)
	{
		Algorithms encryptionAlgorithm = encInfo.EncryptionAlgorithm;
		PasswordUtility passwordUtility = new PasswordUtility();
		IBufferedCipher obj = (passwordUtility.CreateEncoder(encryptionAlgorithm) as IBufferedCipher) ?? throw new Exception("Unknown encryption algorithm");
		ICipherParam parameters = passwordUtility.GenerateCipherParameters(encryptionAlgorithm, passPhrase, isPkcs12empty);
		obj.Initialize(forEncryption: false, parameters);
		return KeyInformation.GetInformation(obj.DoFinal(encInfo.EncryptedData));
	}
}
