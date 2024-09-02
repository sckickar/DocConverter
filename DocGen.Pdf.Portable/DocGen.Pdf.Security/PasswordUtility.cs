using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class PasswordUtility
{
	private const string m_pkcs12 = "Pkcs12";

	private readonly Dictionary<string, string> m_algorithms = new Dictionary<string, string>();

	private readonly Dictionary<string, string> m_type = new Dictionary<string, string>();

	private readonly Dictionary<string, DerObjectID> m_ids = new Dictionary<string, DerObjectID>();

	internal PasswordUtility()
	{
		m_algorithms["PBEWITHSHAAND40BITRC4"] = "PBEwithSHA-1and40bitRC4";
		m_algorithms["PBEWITHSHA1AND40BITRC4"] = "PBEwithSHA-1and40bitRC4";
		m_algorithms["PBEWITHSHA-1AND40BITRC4"] = "PBEwithSHA-1and40bitRC4";
		m_algorithms[PKCSOIDs.PbeWithShaAnd40BitRC4.ID] = "PBEwithSHA-1and40bitRC4";
		m_algorithms["PBEWITHSHAAND3-KEYDESEDE-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		m_algorithms["PBEWITHSHAAND3-KEYTRIPLEDES-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		m_algorithms["PBEWITHSHA1AND3-KEYDESEDE-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		m_algorithms["PBEWITHSHA1AND3-KEYTRIPLEDES-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		m_algorithms["PBEWITHSHA-1AND3-KEYDESEDE-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		m_algorithms["PBEWITHSHA-1AND3-KEYTRIPLEDES-CBC"] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		m_algorithms[PKCSOIDs.PbeWithShaAnd3KeyTripleDesCbc.ID] = "PBEwithSHA-1and3-keyDESEDE-CBC";
		m_algorithms["PBEWITHSHAAND40BITRC2-CBC"] = "PBEwithSHA-1and40bitRC2-CBC";
		m_algorithms["PBEWITHSHA1AND40BITRC2-CBC"] = "PBEwithSHA-1and40bitRC2-CBC";
		m_algorithms["PBEWITHSHA-1AND40BITRC2-CBC"] = "PBEwithSHA-1and40bitRC2-CBC";
		m_algorithms[PKCSOIDs.PbewithShaAnd40BitRC2Cbc.ID] = "PBEwithSHA-1and40bitRC2-CBC";
		m_algorithms["PBEWITHSHAAND128BITAES-CBC-BC"] = "PBEwithSHA-1and128bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA1AND128BITAES-CBC-BC"] = "PBEwithSHA-1and128bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA-1AND128BITAES-CBC-BC"] = "PBEwithSHA-1and128bitAES-CBC-BC";
		m_algorithms["PBEWITHSHAAND192BITAES-CBC-BC"] = "PBEwithSHA-1and192bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA1AND192BITAES-CBC-BC"] = "PBEwithSHA-1and192bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA-1AND192BITAES-CBC-BC"] = "PBEwithSHA-1and192bitAES-CBC-BC";
		m_algorithms["PBEWITHSHAAND256BITAES-CBC-BC"] = "PBEwithSHA-1and256bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA1AND256BITAES-CBC-BC"] = "PBEwithSHA-1and256bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA-1AND256BITAES-CBC-BC"] = "PBEwithSHA-1and256bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA256AND128BITAES-CBC-BC"] = "PBEwithSHA-256and128bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA-256AND128BITAES-CBC-BC"] = "PBEwithSHA-256and128bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA256AND192BITAES-CBC-BC"] = "PBEwithSHA-256and192bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA-256AND192BITAES-CBC-BC"] = "PBEwithSHA-256and192bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA256AND256BITAES-CBC-BC"] = "PBEwithSHA-256and256bitAES-CBC-BC";
		m_algorithms["PBEWITHSHA-256AND256BITAES-CBC-BC"] = "PBEwithSHA-256and256bitAES-CBC-BC";
		m_type["Pkcs12"] = "Pkcs12";
		m_type["PBEwithSHA-1and128bitRC4"] = "Pkcs12";
		m_type["PBEwithSHA-1and40bitRC4"] = "Pkcs12";
		m_type["PBEwithSHA-1and3-keyDESEDE-CBC"] = "Pkcs12";
		m_type["PBEwithSHA-1and2-keyDESEDE-CBC"] = "Pkcs12";
		m_type["PBEwithSHA-1and128bitRC2-CBC"] = "Pkcs12";
		m_type["PBEwithSHA-1and40bitRC2-CBC"] = "Pkcs12";
		m_type["PBEwithSHA-1and256bitAES-CBC-BC"] = "Pkcs12";
		m_type["PBEwithSHA-256and128bitAES-CBC-BC"] = "Pkcs12";
		m_type["PBEwithSHA-256and192bitAES-CBC-BC"] = "Pkcs12";
		m_type["PBEwithSHA-256and256bitAES-CBC-BC"] = "Pkcs12";
		m_ids["PBEwithSHA-1and128bitRC4"] = PKCSOIDs.PbeWithShaAnd128BitRC4;
		m_ids["PBEwithSHA-1and40bitRC4"] = PKCSOIDs.PbeWithShaAnd40BitRC4;
		m_ids["PBEwithSHA-1and3-keyDESEDE-CBC"] = PKCSOIDs.PbeWithShaAnd3KeyTripleDesCbc;
		m_ids["PBEwithSHA-1and2-keyDESEDE-CBC"] = PKCSOIDs.PbeWithShaAnd2KeyTripleDesCbc;
		m_ids["PBEwithSHA-1and128bitRC2-CBC"] = PKCSOIDs.PbeWithShaAnd128BitRC2Cbc;
		m_ids["PBEwithSHA-1and40bitRC2-CBC"] = PKCSOIDs.PbewithShaAnd40BitRC2Cbc;
	}

	internal PasswordGenerator GetEncoder(string type, IMessageDigest digest, byte[] key, byte[] salt, int iterationCount)
	{
		if (type.Equals("Pkcs12"))
		{
			PasswordGenerator passwordGenerator = new PKCS12AlgorithmGenerator(digest);
			passwordGenerator.Init(key, salt, iterationCount);
			return passwordGenerator;
		}
		throw new ArgumentException("Invalid Password Based Encryption type: " + type, "type");
	}

	internal bool IsPkcs12(string algorithm)
	{
		string text = m_algorithms[algorithm.ToUpperInvariant()];
		if (text != null)
		{
			return "Pkcs12".Equals(m_type[text]);
		}
		return false;
	}

	internal ICipherParam GenerateCipherParameters(DerObjectID algorithmOid, char[] password, bool isWrong, Asn1Encode parameters)
	{
		return GenerateCipherParameters(algorithmOid.ID, password, isWrong, parameters);
	}

	internal ICipherParam GenerateCipherParameters(Algorithms algID, char[] password, bool isWrong)
	{
		return GenerateCipherParameters(algID.ObjectID.ID, password, isWrong, algID.Parameters);
	}

	internal ICipherParam GenerateCipherParameters(string algorithm, char[] password, bool isWrong, Asn1Encode pbeParameters)
	{
		string text = m_algorithms[algorithm.ToUpperInvariant()];
		byte[] array = null;
		byte[] salt = null;
		int iterationCount = 0;
		if (IsPkcs12(text))
		{
			PKCS12PasswordParameter pBEParameter = PKCS12PasswordParameter.GetPBEParameter(pbeParameters);
			salt = pBEParameter.Octets;
			iterationCount = pBEParameter.Iterations.IntValue;
			array = PasswordGenerator.ToBytes(password, isWrong);
		}
		ICipherParam parameters = null;
		if (text.StartsWith("PBEwithSHA-1"))
		{
			PasswordGenerator encoder = GetEncoder(m_type[text], new SHA1MessageDigest(), array, salt, iterationCount);
			if (text.Equals("PBEwithSHA-1and128bitAES-CBC-BC"))
			{
				parameters = encoder.GenerateParam("AES", 128, 128);
			}
			else if (text.Equals("PBEwithSHA-1and192bitAES-CBC-BC"))
			{
				parameters = encoder.GenerateParam("AES", 192, 128);
			}
			else if (text.Equals("PBEwithSHA-1and256bitAES-CBC-BC"))
			{
				parameters = encoder.GenerateParam("AES", 256, 128);
			}
			else if (text.Equals("PBEwithSHA-1and128bitRC4"))
			{
				parameters = encoder.GenerateParam("RC4", 128);
			}
			else if (text.Equals("PBEwithSHA-1and40bitRC4"))
			{
				parameters = encoder.GenerateParam("RC4", 40);
			}
			else if (text.Equals("PBEwithSHA-1and3-keyDESEDE-CBC"))
			{
				parameters = encoder.GenerateParam("DESEDE", 192, 64);
			}
			else if (text.Equals("PBEwithSHA-1and2-keyDESEDE-CBC"))
			{
				parameters = encoder.GenerateParam("DESEDE", 128, 64);
			}
			else if (text.Equals("PBEwithSHA-1and128bitRC2-CBC"))
			{
				parameters = encoder.GenerateParam("RC2", 128, 64);
			}
			else if (text.Equals("PBEwithSHA-1and40bitRC2-CBC"))
			{
				parameters = encoder.GenerateParam("RC2", 40, 64);
			}
			else if (text.Equals("PBEwithSHA-1andDES-CBC"))
			{
				parameters = encoder.GenerateParam("DES", 64, 64);
			}
			else if (text.Equals("PBEwithSHA-1andRC2-CBC"))
			{
				parameters = encoder.GenerateParam("RC2", 64, 64);
			}
		}
		else if (text.StartsWith("PBEwithSHA-256"))
		{
			PasswordGenerator encoder2 = GetEncoder(m_type[text], new SHA256MessageDigest(), array, salt, iterationCount);
			if (text.Equals("PBEwithSHA-256and128bitAES-CBC-BC"))
			{
				parameters = encoder2.GenerateParam("AES", 128, 128);
			}
			else if (text.Equals("PBEwithSHA-256and192bitAES-CBC-BC"))
			{
				parameters = encoder2.GenerateParam("AES", 192, 128);
			}
			else if (text.Equals("PBEwithSHA-256and256bitAES-CBC-BC"))
			{
				parameters = encoder2.GenerateParam("AES", 256, 128);
			}
		}
		else if (text.StartsWith("PBEwithHmac"))
		{
			string algorithm2 = text.Substring("PBEwithHmac".Length);
			IMessageDigest digest = new MessageDigestFinder().GetDigest(algorithm2);
			PasswordGenerator encoder3 = GetEncoder(m_type[text], digest, array, salt, iterationCount);
			int keySize = digest.MessageDigestSize * 8;
			parameters = encoder3.GenerateParam(keySize);
		}
		Array.Clear(array, 0, array.Length);
		return FixDataEncryptionParity(text, parameters);
	}

	internal object CreateEncoder(DerObjectID algorithmOid)
	{
		return CreateEncoder(algorithmOid.ID);
	}

	internal object CreateEncoder(Algorithms algId)
	{
		string iD = algId.ObjectID.ID;
		return CreateEncoder(iD);
	}

	internal object CreateEncoder(string algorithm)
	{
		string text = m_algorithms[algorithm.ToUpperInvariant()];
		if (text.StartsWith("PBEwithMD2") || text.StartsWith("PBEwithMD5") || text.StartsWith("PBEwithSHA-1") || text.StartsWith("PBEwithSHA-256"))
		{
			if (text.EndsWith("AES-CBC-BC") || text.EndsWith("AES-CBC-OPENSSL"))
			{
				return CipherUtils.GetCipher("AES/CBC");
			}
			if (text.EndsWith("DES-CBC"))
			{
				return CipherUtils.GetCipher("DES/CBC");
			}
			if (text.EndsWith("DESEDE-CBC"))
			{
				return CipherUtils.GetCipher("DESEDE/CBC");
			}
			if (text.EndsWith("RC2-CBC"))
			{
				return CipherUtils.GetCipher("RC2/CBC");
			}
			if (text.EndsWith("RC4"))
			{
				return CipherUtils.GetCipher("RC4");
			}
		}
		return null;
	}

	private ICipherParam FixDataEncryptionParity(string mechanism, ICipherParam parameters)
	{
		if (!mechanism.EndsWith("DES-CBC") & !mechanism.EndsWith("DESEDE-CBC"))
		{
			return parameters;
		}
		if (parameters is InvalidParameter)
		{
			InvalidParameter invalidParameter = (InvalidParameter)parameters;
			return new InvalidParameter(FixDataEncryptionParity(mechanism, invalidParameter.Parameters), invalidParameter.InvalidBytes);
		}
		byte[] keys = ((KeyParameter)parameters).Keys;
		for (int i = 0; i < keys.Length; i++)
		{
			int num = keys[i];
			keys[i] = (byte)(((uint)num & 0xFEu) | (((uint)((num >> 1) ^ (num >> 2) ^ (num >> 3) ^ (num >> 4) ^ (num >> 5) ^ (num >> 6) ^ (num >> 7)) ^ 1u) & 1u));
		}
		return new KeyParameter(keys);
	}
}
