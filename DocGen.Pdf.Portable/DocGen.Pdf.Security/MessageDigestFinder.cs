using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class MessageDigestFinder
{
	private enum DigestAlgorithm
	{
		SHA_1,
		SHA_256,
		MD5,
		SHA_384,
		SHA_512,
		RIPEMD160
	}

	private readonly Dictionary<string, string> m_algorithms = new Dictionary<string, string>();

	private readonly Dictionary<string, DerObjectID> m_ids = new Dictionary<string, DerObjectID>();

	internal MessageDigestFinder()
	{
		((DigestAlgorithm)(object)Enums.GetArbitraryValue(typeof(DigestAlgorithm))).ToString();
		m_algorithms["SHA1"] = "SHA-1";
		m_algorithms[new DerObjectID("1.3.14.3.2.26").ID] = "SHA-1";
		m_algorithms["SHA256"] = "SHA-256";
		m_algorithms[NISTOIDs.SHA256.ID] = "SHA-256";
		m_algorithms["SHA384"] = "SHA-384";
		m_algorithms[NISTOIDs.SHA384.ID] = "SHA-384";
		m_algorithms["SHA512"] = "SHA-512";
		m_algorithms[NISTOIDs.SHA512.ID] = "SHA-512";
		m_algorithms["MD5"] = "MD5";
		m_algorithms[PKCSOIDs.MD5.ID] = "MD5";
		m_algorithms["RIPEMD-160"] = "RIPEMD160";
		m_algorithms["RIPEMD160"] = "RIPEMD160";
		m_algorithms[NISTOIDs.RipeMD160.ID] = "RIPEMD160";
		m_ids["SHA-1"] = new DerObjectID("1.3.14.3.2.26");
		m_ids["SHA-256"] = NISTOIDs.SHA256;
		m_ids["SHA-384"] = NISTOIDs.SHA384;
		m_ids["SHA-512"] = NISTOIDs.SHA512;
		m_ids["MD5"] = PKCSOIDs.MD5;
		m_ids["RIPEMD160"] = NISTOIDs.RipeMD160;
	}

	internal IMessageDigest GetMessageDigest(DerObjectID id)
	{
		return GetDigest(id.ID);
	}

	internal IMessageDigest GetDigest(string algorithm)
	{
		string text = algorithm.ToUpperInvariant();
		string text2 = m_algorithms[text];
		if (text2 == null)
		{
			text2 = text;
		}
		return (DigestAlgorithm)(object)Enums.GetEnumValue(typeof(DigestAlgorithm), text2) switch
		{
			DigestAlgorithm.SHA_1 => new SHA1MessageDigest(), 
			DigestAlgorithm.SHA_256 => new SHA256MessageDigest(), 
			DigestAlgorithm.SHA_384 => new SHA384MessageDigest(), 
			DigestAlgorithm.SHA_512 => new SHA512MessageDigest(), 
			DigestAlgorithm.MD5 => new MessageDigest5(), 
			DigestAlgorithm.RIPEMD160 => new RIPEMD160MessageDigest(), 
			_ => throw new Exception("Invalid message"), 
		};
	}

	internal byte[] CalculateDigest(string algorithm, byte[] bytes)
	{
		IMessageDigest digest = GetDigest(algorithm);
		digest.Update(bytes, 0, bytes.Length);
		return DoFinal(digest);
	}

	internal byte[] DoFinal(IMessageDigest digest)
	{
		byte[] array = new byte[digest.MessageDigestSize];
		digest.DoFinal(array, 0);
		return array;
	}
}
