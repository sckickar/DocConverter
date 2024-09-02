using System;

namespace DocGen.Pdf.Security;

internal class SubjectKeyID : Asn1Encode
{
	private byte[] m_bytes;

	internal static SubjectKeyID GetIdentifier(object obj)
	{
		if (obj is SubjectKeyID)
		{
			return (SubjectKeyID)obj;
		}
		if (obj is PublicKeyInformation)
		{
			return new SubjectKeyID((PublicKeyInformation)obj);
		}
		if (obj is Asn1Octet)
		{
			return new SubjectKeyID((Asn1Octet)obj);
		}
		if (obj is X509Extension)
		{
			return GetIdentifier(X509Extension.ConvertValueToObject((X509Extension)obj));
		}
		throw new ArgumentException("Invalid entry");
	}

	internal SubjectKeyID(Asn1Octet keyID)
	{
		m_bytes = keyID.GetOctets();
	}

	internal SubjectKeyID(PublicKeyInformation publicKey)
	{
		m_bytes = GetDigest(publicKey);
	}

	internal byte[] GetKeyIdentifier()
	{
		return m_bytes;
	}

	public override Asn1 GetAsn1()
	{
		return new DerOctet(m_bytes);
	}

	internal static PublicKeyInformation CreateSubjectKeyID(CipherParameter publicKey)
	{
		if (publicKey is RsaKeyParam)
		{
			RsaKeyParam rsaKeyParam = (RsaKeyParam)publicKey;
			return new PublicKeyInformation(new Algorithms(PKCSOIDs.RsaEncryption, DerNull.Value), new RSAPublicKey(rsaKeyParam.Modulus, rsaKeyParam.Exponent).GetAsn1());
		}
		if (publicKey is ECPublicKeyParam)
		{
			ECPublicKeyParam eCPublicKeyParam = (ECPublicKeyParam)publicKey;
			if (eCPublicKeyParam.AlgorithmName == "ECGOST3410")
			{
				if (eCPublicKeyParam.PublicKeyParamSet == null)
				{
					throw new Exception("Not a CryptoPro parameter set");
				}
				EllipticPoint pointQ = eCPublicKeyParam.PointQ;
				Number byteBI = pointQ.PointX.ToIntValue();
				Number byteBI2 = pointQ.PointY.ToIntValue();
				byte[] array = new byte[64];
				DecompressBytes(array, 0, byteBI);
				DecompressBytes(array, 32, byteBI2);
				ECGostAlgorithm eCGostAlgorithm = new ECGostAlgorithm(eCPublicKeyParam.PublicKeyParamSet, CRYPTOIDs.IDR3411X94);
				return new PublicKeyInformation(new Algorithms(CRYPTOIDs.IDR3410, eCGostAlgorithm.GetAsn1()), new DerOctet(array));
			}
			ECX962Params eCX962Params;
			if (eCPublicKeyParam.PublicKeyParamSet == null)
			{
				EllipticCurveParams parameters = eCPublicKeyParam.Parameters;
				eCX962Params = new ECX962Params(new ECX9Field(parameters.Curve, parameters.PointG, parameters.NumberX, parameters.NumberY, parameters.ECSeed()));
			}
			else
			{
				eCX962Params = new ECX962Params(eCPublicKeyParam.PublicKeyParamSet);
			}
			Asn1Octet asn1Octet = (Asn1Octet)new ECx9Point(eCPublicKeyParam.PointQ).GetAsn1();
			return new PublicKeyInformation(new Algorithms(ECDSAOIDs.IdECPublicKey, eCX962Params.GetAsn1()), asn1Octet.GetOctets());
		}
		throw new Exception("Invalid Key");
	}

	private static void DecompressBytes(byte[] encKey, int offset, Number byteBI)
	{
		byte[] array = byteBI.ToByteArray();
		int num = (byteBI.BitLength + 7) / 8;
		for (int i = 0; i < num; i++)
		{
			encKey[offset + i] = array[array.Length - 1 - i];
		}
	}

	private static byte[] GetDigest(PublicKeyInformation publicKey)
	{
		SHA1MessageDigest sHA1MessageDigest = new SHA1MessageDigest();
		byte[] array = new byte[((IMessageDigest)sHA1MessageDigest).MessageDigestSize];
		byte[] bytes = publicKey.PublicKey.GetBytes();
		((IMessageDigest)sHA1MessageDigest).Update(bytes, 0, bytes.Length);
		((IMessageDigest)sHA1MessageDigest).DoFinal(array, 0);
		return array;
	}
}
