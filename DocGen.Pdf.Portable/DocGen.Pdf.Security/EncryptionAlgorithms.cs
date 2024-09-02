using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class EncryptionAlgorithms
{
	private readonly Dictionary<string, string> algorithmNames = new Dictionary<string, string>();

	internal EncryptionAlgorithms()
	{
		algorithmNames["1.2.840.113549.1.1.1"] = "RSA";
		algorithmNames["1.2.840.10040.4.1"] = "DSA";
		algorithmNames["1.2.840.113549.1.1.2"] = "RSA";
		algorithmNames["1.2.840.113549.1.1.4"] = "RSA";
		algorithmNames["1.2.840.113549.1.1.5"] = "RSA";
		algorithmNames["1.2.840.113549.1.1.14"] = "RSA";
		algorithmNames["1.2.840.113549.1.1.11"] = "RSA";
		algorithmNames["1.2.840.113549.1.1.12"] = "RSA";
		algorithmNames["1.2.840.113549.1.1.13"] = "RSA";
		algorithmNames["1.2.840.10040.4.3"] = "DSA";
		algorithmNames["2.16.840.1.101.3.4.3.1"] = "DSA";
		algorithmNames["2.16.840.1.101.3.4.3.2"] = "DSA";
		algorithmNames["1.3.14.3.2.29"] = "RSA";
		algorithmNames["1.3.36.3.3.1.2"] = "RSA";
		algorithmNames["1.3.36.3.3.1.3"] = "RSA";
		algorithmNames["1.3.36.3.3.1.4"] = "RSA";
		algorithmNames["1.2.643.2.2.19"] = "ECGOST3410";
		algorithmNames["1.2.840.113549.1.1.10"] = "RSAandMGF1";
		algorithmNames["1.2.840.10045.2.1"] = "ECDSA";
		algorithmNames["1.2.840.10045.4.1"] = "ECDSA";
		algorithmNames["1.2.840.10045.4.3.1"] = "ECDSA";
		algorithmNames["1.2.840.10045.4.3.2"] = "ECDSA";
		algorithmNames["1.2.840.10045.4.3.3"] = "ECDSA";
		algorithmNames["1.2.840.10045.4.3.4"] = "ECDSA";
	}

	internal string GetAlgorithm(string oid)
	{
		if (algorithmNames.TryGetValue(oid, out string value))
		{
			return value;
		}
		return oid;
	}
}
