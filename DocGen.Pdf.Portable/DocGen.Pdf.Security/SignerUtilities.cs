using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class SignerUtilities
{
	internal Dictionary<string, string> m_algms = new Dictionary<string, string>();

	internal Dictionary<string, DerObjectID> m_oids = new Dictionary<string, DerObjectID>();

	internal SignerUtilities()
	{
		m_algms["MD2WITHRSA"] = "MD2withRSA";
		m_algms["MD2WITHRSAENCRYPTION"] = "MD2withRSA";
		m_algms[PKCSOIDs.MD2WithRsaEncryption.ID] = "MD2withRSA";
		m_algms[PKCSOIDs.RsaEncryption.ID] = "RSA";
		m_algms["SHA1WITHRSA"] = "SHA-1withRSA";
		m_algms["SHA1WITHRSAENCRYPTION"] = "SHA-1withRSA";
		m_algms[PKCSOIDs.Sha1WithRsaEncryption.ID] = "SHA-1withRSA";
		m_algms["SHA-1WITHRSA"] = "SHA-1withRSA";
		m_algms["SHA256WITHRSA"] = "SHA-256withRSA";
		m_algms["SHA256WITHRSAENCRYPTION"] = "SHA-256withRSA";
		m_algms[PKCSOIDs.Sha256WithRsaEncryption.ID] = "SHA-256withRSA";
		m_algms["SHA-256WITHRSA"] = "SHA-256withRSA";
		m_algms["SHA1WITHRSAANDMGF1"] = "SHA-1withRSAandMGF1";
		m_algms["SHA-1WITHRSAANDMGF1"] = "SHA-1withRSAandMGF1";
		m_algms["SHA1WITHRSA/PSS"] = "SHA-1withRSAandMGF1";
		m_algms["SHA-1WITHRSA/PSS"] = "SHA-1withRSAandMGF1";
		m_algms["SHA224WITHRSAANDMGF1"] = "SHA-224withRSAandMGF1";
		m_algms["SHA-224WITHRSAANDMGF1"] = "SHA-224withRSAandMGF1";
		m_algms["SHA224WITHRSA/PSS"] = "SHA-224withRSAandMGF1";
		m_algms["SHA-224WITHRSA/PSS"] = "SHA-224withRSAandMGF1";
		m_algms["SHA256WITHRSAANDMGF1"] = "SHA-256withRSAandMGF1";
		m_algms["SHA-256WITHRSAANDMGF1"] = "SHA-256withRSAandMGF1";
		m_algms["SHA256WITHRSA/PSS"] = "SHA-256withRSAandMGF1";
		m_algms["SHA-256WITHRSA/PSS"] = "SHA-256withRSAandMGF1";
		m_algms["SHA384WITHRSA"] = "SHA-384withRSA";
		m_algms["SHA512WITHRSA"] = "SHA-512withRSA";
		m_algms["SHA384WITHRSAENCRYPTION"] = "SHA-384withRSA";
		m_algms[PKCSOIDs.Sha384WithRsaEncryption.ID] = "SHA-384withRSA";
		m_algms["SHA-384WITHRSA"] = "SHA-384withRSA";
		m_algms["SHA-512WITHRSA"] = "SHA-512withRSA";
		m_algms[PKCSOIDs.Sha512WithRsaEncryption.ID] = "SHA-512withRSA";
		m_algms["SHA384WITHRSAANDMGF1"] = "SHA-384withRSAandMGF1";
		m_algms["SHA-384WITHRSAANDMGF1"] = "SHA-384withRSAandMGF1";
		m_algms["SHA384WITHRSA/PSS"] = "SHA-384withRSAandMGF1";
		m_algms["SHA-384WITHRSA/PSS"] = "SHA-384withRSAandMGF1";
		m_algms["SHA512WITHRSAANDMGF1"] = "SHA-512withRSAandMGF1";
		m_algms["SHA-512WITHRSAANDMGF1"] = "SHA-512withRSAandMGF1";
		m_algms["SHA512WITHRSA/PSS"] = "SHA-512withRSAandMGF1";
		m_algms["SHA-512WITHRSA/PSS"] = "SHA-512withRSAandMGF1";
		m_algms["DSAWITHSHA256"] = "SHA-256withDSA";
		m_algms["DSAWITHSHA-256"] = "SHA-256withDSA";
		m_algms["SHA256/DSA"] = "SHA-256withDSA";
		m_algms["SHA-256/DSA"] = "SHA-256withDSA";
		m_algms["SHA256WITHDSA"] = "SHA-256withDSA";
		m_algms["SHA-256WITHDSA"] = "SHA-256withDSA";
		m_algms[NISTOIDs.DSAWithSHA256.ID] = "SHA-256withDSA";
		m_algms["RIPEMD160WITHRSA"] = "RIPEMD160withRSA";
		m_algms["RIPEMD160WITHRSAENCRYPTION"] = "RIPEMD160withRSA";
		m_algms[NISTOIDs.RsaSignatureWithRipeMD160.ID] = "RIPEMD160withRSA";
		m_oids["SHA-1withRSA"] = PKCSOIDs.Sha1WithRsaEncryption;
		m_oids["SHA-256withRSA"] = PKCSOIDs.Sha256WithRsaEncryption;
		m_oids["SHA-384withRSA"] = PKCSOIDs.Sha384WithRsaEncryption;
		m_oids["SHA-512withRSA"] = PKCSOIDs.Sha512WithRsaEncryption;
		m_oids["RIPEMD160withRSA"] = NISTOIDs.RsaSignatureWithRipeMD160;
		m_algms["NONEWITHECDSA"] = "NONEwithECDSA";
		m_algms["ECDSAWITHNONE"] = "NONEwithECDSA";
		m_algms["ECDSA"] = "SHA-1withECDSA";
		m_algms["SHA1/ECDSA"] = "SHA-1withECDSA";
		m_algms["SHA-1/ECDSA"] = "SHA-1withECDSA";
		m_algms["ECDSAWITHSHA1"] = "SHA-1withECDSA";
		m_algms["ECDSAWITHSHA-1"] = "SHA-1withECDSA";
		m_algms["SHA1WITHECDSA"] = "SHA-1withECDSA";
		m_algms["SHA-1WITHECDSA"] = "SHA-1withECDSA";
		m_algms[ECDSAOIDs.ECDSAwithSHA1.ID] = "SHA-1withECDSA";
		m_algms[ECBrainpoolIDs.EllipticSignSignWithSha1.ID] = "SHA-1withECDSA";
		m_algms["SHA224/ECDSA"] = "SHA-224withECDSA";
		m_algms["SHA-224/ECDSA"] = "SHA-224withECDSA";
		m_algms["ECDSAWITHSHA224"] = "SHA-224withECDSA";
		m_algms["ECDSAWITHSHA-224"] = "SHA-224withECDSA";
		m_algms["SHA224WITHECDSA"] = "SHA-224withECDSA";
		m_algms["SHA-224WITHECDSA"] = "SHA-224withECDSA";
		m_algms[ECDSAOIDs.ECDSAwithSHA224.ID] = "SHA-224withECDSA";
		m_algms["SHA256/ECDSA"] = "SHA-256withECDSA";
		m_algms["SHA-256/ECDSA"] = "SHA-256withECDSA";
		m_algms["ECDSAWITHSHA256"] = "SHA-256withECDSA";
		m_algms["ECDSAWITHSHA-256"] = "SHA-256withECDSA";
		m_algms["SHA256WITHECDSA"] = "SHA-256withECDSA";
		m_algms["SHA-256WITHECDSA"] = "SHA-256withECDSA";
		m_algms[ECDSAOIDs.ECDSAwithSHA256.ID] = "SHA-256withECDSA";
		m_algms["SHA384/ECDSA"] = "SHA-384withECDSA";
		m_algms["SHA-384/ECDSA"] = "SHA-384withECDSA";
		m_algms["ECDSAWITHSHA384"] = "SHA-384withECDSA";
		m_algms["ECDSAWITHSHA-384"] = "SHA-384withECDSA";
		m_algms["SHA384WITHECDSA"] = "SHA-384withECDSA";
		m_algms["SHA-384WITHECDSA"] = "SHA-384withECDSA";
		m_algms[ECDSAOIDs.ECDSAwithSHA384.ID] = "SHA-384withECDSA";
		m_algms["SHA512/ECDSA"] = "SHA-512withECDSA";
		m_algms["SHA-512/ECDSA"] = "SHA-512withECDSA";
		m_algms["ECDSAWITHSHA512"] = "SHA-512withECDSA";
		m_algms["ECDSAWITHSHA-512"] = "SHA-512withECDSA";
		m_algms["SHA512WITHECDSA"] = "SHA-512withECDSA";
		m_algms["SHA-512WITHECDSA"] = "SHA-512withECDSA";
		m_algms[ECDSAOIDs.ECDSAwithSHA512.ID] = "SHA-512withECDSA";
		m_algms["RIPEMD160/ECDSA"] = "RIPEMD160withECDSA";
		m_algms["ECDSAWITHRIPEMD160"] = "RIPEMD160withECDSA";
		m_algms["RIPEMD160WITHECDSA"] = "RIPEMD160withECDSA";
		m_algms[ECBrainpoolIDs.EllipticSignWithRipeMD160.ID] = "RIPEMD160withECDSA";
		m_oids["SHA-1withECDSA"] = ECDSAOIDs.ECDSAwithSHA1;
		m_oids["SHA-224withECDSA"] = ECDSAOIDs.ECDSAwithSHA224;
		m_oids["SHA-256withECDSA"] = ECDSAOIDs.ECDSAwithSHA256;
		m_oids["SHA-384withECDSA"] = ECDSAOIDs.ECDSAwithSHA384;
		m_oids["SHA-512withECDSA"] = ECDSAOIDs.ECDSAwithSHA512;
	}

	internal DerObjectID GetOID(string mechanism)
	{
		if (mechanism == null)
		{
			throw new ArgumentNullException("mechanism");
		}
		mechanism = mechanism.ToUpperInvariant();
		string text = m_algms[mechanism];
		if (text != null)
		{
			mechanism = text;
		}
		return m_oids[mechanism];
	}

	internal ISigner GetSigner(string algorithm)
	{
		if (algorithm == null)
		{
			throw new ArgumentNullException("algorithm");
		}
		algorithm = algorithm.ToUpperInvariant();
		string text = m_algms[algorithm];
		if (text == null)
		{
			text = algorithm;
		}
		if (text.Equals("SHA-1withRSA"))
		{
			return new RMDSigner(new SHA1MessageDigest());
		}
		if (text.Equals("SHA-256withRSA"))
		{
			return new RMDSigner(new SHA256MessageDigest());
		}
		if (text.Equals("SHA-384withRSA"))
		{
			return new RMDSigner(new SHA384MessageDigest());
		}
		if (text.Equals("SHA-512withRSA"))
		{
			return new RMDSigner(new SHA512MessageDigest());
		}
		if (text.Equals("RIPEMD160withRSA"))
		{
			return new RMDSigner(new RIPEMD160MessageDigest());
		}
		if (text.Equals("RAWRSASSA-PSS"))
		{
			return PSSSigner.CreateRawSigner(new RSAAlgorithm(), new SHA1MessageDigest());
		}
		if (text.Equals("PSSwithRSA"))
		{
			return new PSSSigner(new RSAAlgorithm(), new SHA1MessageDigest());
		}
		if (text.Equals("SHA-1withRSAandMGF1"))
		{
			return new PSSSigner(new RSAAlgorithm(), new SHA1MessageDigest());
		}
		if (text.Equals("SHA-256withRSAandMGF1"))
		{
			return new PSSSigner(new RSAAlgorithm(), new SHA256MessageDigest());
		}
		if (text.Equals("SHA-384withRSAandMGF1"))
		{
			return new PSSSigner(new RSAAlgorithm(), new SHA384MessageDigest());
		}
		if (text.Equals("SHA-512withRSAandMGF1"))
		{
			return new PSSSigner(new RSAAlgorithm(), new SHA512MessageDigest());
		}
		if (text.Equals("SHA-1withECDSA"))
		{
			return new DSASigner(new ECDSAAlgorithm(), new SHA1MessageDigest());
		}
		if (text.Equals("SHA-256withECDSA"))
		{
			return new DSASigner(new ECDSAAlgorithm(), new SHA256MessageDigest());
		}
		if (text.Equals("SHA-384withECDSA"))
		{
			return new DSASigner(new ECDSAAlgorithm(), new SHA384MessageDigest());
		}
		if (text.Equals("SHA-512withECDSA"))
		{
			return new DSASigner(new ECDSAAlgorithm(), new SHA512MessageDigest());
		}
		if (text.Equals("RIPEMD160withECDSA"))
		{
			return new DSASigner(new ECDSAAlgorithm(), new RIPEMD160MessageDigest());
		}
		throw new Exception("Signer " + algorithm + " not recognised.");
	}

	internal string GetEncoding(DerObjectID oid)
	{
		return m_algms[oid.ID];
	}

	internal string GetAlgorithmName(DerObjectID oid)
	{
		foreach (string key in m_oids.Keys)
		{
			if (m_oids[key] == oid)
			{
				return key;
			}
		}
		return oid.ID;
	}
}
