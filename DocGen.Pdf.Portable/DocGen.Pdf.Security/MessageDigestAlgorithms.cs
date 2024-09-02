using System.Collections.Generic;
using System.IO;

namespace DocGen.Pdf.Security;

internal class MessageDigestAlgorithms
{
	internal const string SHA1 = "SHA-1";

	internal const string SHA256 = "SHA-256";

	internal const string SHA384 = "SHA-384";

	internal const string SHA512 = "SHA-512";

	internal const string RIPEMD160 = "RIPEMD160";

	private readonly Dictionary<string, string> m_names = new Dictionary<string, string>();

	private readonly Dictionary<string, string> m_digests = new Dictionary<string, string>();

	private MessageDigestFinder m_finder = new MessageDigestFinder();

	internal MessageDigestAlgorithms()
	{
		m_names["1.2.840.113549.2.5"] = "MD5";
		m_names["1.3.14.3.2.26"] = "SHA1";
		m_names["2.16.840.1.101.3.4.2.1"] = "SHA256";
		m_names["2.16.840.1.101.3.4.2.2"] = "SHA384";
		m_names["2.16.840.1.101.3.4.2.3"] = "SHA512";
		m_names["1.3.36.3.2.1"] = "RIPEMD160";
		m_names["1.2.840.113549.1.1.4"] = "MD5";
		m_names["1.2.840.113549.1.1.5"] = "SHA1";
		m_names["1.2.840.113549.1.1.11"] = "SHA256";
		m_names["1.2.840.113549.1.1.12"] = "SHA384";
		m_names["1.2.840.113549.1.1.13"] = "SHA512";
		m_names["1.2.840.113549.2.5"] = "MD5";
		m_names["1.2.840.10040.4.3"] = "SHA1";
		m_names["2.16.840.1.101.3.4.3.2"] = "SHA256";
		m_names["2.16.840.1.101.3.4.3.3"] = "SHA384";
		m_names["2.16.840.1.101.3.4.3.4"] = "SHA512";
		m_names["1.3.36.3.3.1.2"] = "RIPEMD160";
		m_names["1.2.840.10045.4.3.2"] = "SHA256";
		m_names["1.2.840.10045.4.3.3"] = "SHA384";
		m_names["1.2.840.10045.4.3.4"] = "SHA512";
		m_digests["MD5"] = "1.2.840.113549.2.5";
		m_digests["MD-5"] = "1.2.840.113549.2.5";
		m_digests["SHA1"] = "1.3.14.3.2.26";
		m_digests["SHA-1"] = "1.3.14.3.2.26";
		m_digests["SHA256"] = "2.16.840.1.101.3.4.2.1";
		m_digests["SHA-256"] = "2.16.840.1.101.3.4.2.1";
		m_digests["SHA384"] = "2.16.840.1.101.3.4.2.2";
		m_digests["SHA-384"] = "2.16.840.1.101.3.4.2.2";
		m_digests["SHA512"] = "2.16.840.1.101.3.4.2.3";
		m_digests["SHA-512"] = "2.16.840.1.101.3.4.2.3";
		m_digests["RIPEMD160"] = "1.3.36.3.2.1";
		m_digests["RIPEMD-160"] = "1.3.36.3.2.1";
	}

	internal IMessageDigest GetMessageDigest(string hashAlgorithm)
	{
		return m_finder.GetDigest(hashAlgorithm);
	}

	internal byte[] Digest(Stream data, string hashAlgorithm)
	{
		IMessageDigest messageDigest = GetMessageDigest(hashAlgorithm);
		return Digest(data, messageDigest);
	}

	internal byte[] Digest(Stream data, IMessageDigest messageDigest)
	{
		byte[] array = new byte[8192];
		int length;
		while ((length = data.Read(array, 0, array.Length)) > 0)
		{
			messageDigest.Update(array, 0, length);
		}
		byte[] array2 = new byte[messageDigest.MessageDigestSize];
		messageDigest.DoFinal(array2, 0);
		return array2;
	}

	internal string GetDigest(string id)
	{
		if (m_names.TryGetValue(id, out string value))
		{
			return value;
		}
		return id;
	}

	internal string GetAllowedDigests(string name)
	{
		m_digests.TryGetValue(name.ToUpperInvariant(), out string value);
		return value;
	}

	internal byte[] Digest(string algorithm, byte[] bytes)
	{
		return Digest(m_finder.GetDigest(algorithm), bytes, 0, bytes.Length);
	}

	internal byte[] Digest(IMessageDigest digest, byte[] bytes, int offset, int length)
	{
		digest.Update(bytes, offset, length);
		byte[] array = new byte[digest.MessageDigestSize];
		digest.DoFinal(array, 0);
		return array;
	}
}
