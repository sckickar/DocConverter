using System;
using System.Collections.Generic;

namespace DocGen.Pdf.Security;

internal class RMDSigner : ISigner
{
	private ICipherBlock m_rsaEngine = new Pkcs1Encoding(new RSAAlgorithm());

	private Algorithms m_id;

	private IMessageDigest m_digest;

	private bool m_isSigning;

	private static readonly Dictionary<string, DerObjectID> m_map;

	public string AlgorithmName => m_digest.AlgorithmName + "withRSA";

	static RMDSigner()
	{
		m_map = new Dictionary<string, DerObjectID>();
		m_map["SHA-1"] = X509Objects.IdSha1;
		m_map["SHA-256"] = NISTOIDs.SHA256;
		m_map["SHA-384"] = NISTOIDs.SHA384;
		m_map["SHA-512"] = NISTOIDs.SHA512;
		m_map["RIPEMD160"] = NISTOIDs.RipeMD160;
	}

	internal RMDSigner(IMessageDigest digest)
	{
		m_digest = digest;
		if (digest.AlgorithmName.Equals("NULL"))
		{
			m_id = null;
		}
		else
		{
			m_id = new Algorithms(m_map[digest.AlgorithmName], DerNull.Value);
		}
	}

	public void Initialize(bool isSigning, ICipherParam parameters)
	{
		m_isSigning = isSigning;
		CipherParameter cipherParameter = (CipherParameter)parameters;
		if (isSigning && !cipherParameter.IsPrivate)
		{
			throw new Exception("Private key required.");
		}
		if (!isSigning && cipherParameter.IsPrivate)
		{
			throw new Exception("Public key required.");
		}
		Reset();
		m_rsaEngine.Initialize(isSigning, parameters);
	}

	public void Update(byte input)
	{
		m_digest.Update(input);
	}

	public void BlockUpdate(byte[] input, int inOff, int length)
	{
		m_digest.Update(input, inOff, length);
	}

	public byte[] GenerateSignature()
	{
		if (!m_isSigning)
		{
			throw new InvalidOperationException("Invalid entry");
		}
		byte[] array = new byte[m_digest.MessageDigestSize];
		m_digest.DoFinal(array, 0);
		byte[] array2 = DerEncode(array);
		return m_rsaEngine.ProcessBlock(array2, 0, array2.Length);
	}

	public bool ValidateSignature(byte[] signature)
	{
		if (m_isSigning)
		{
			throw new InvalidOperationException("Invalid entry");
		}
		byte[] array = new byte[m_digest.MessageDigestSize];
		m_digest.DoFinal(array, 0);
		byte[] array2;
		byte[] array3;
		try
		{
			array2 = m_rsaEngine.ProcessBlock(signature, 0, signature.Length);
			array3 = DerEncode(array);
		}
		catch (Exception)
		{
			return false;
		}
		if (array2.Length == array3.Length)
		{
			for (int i = 0; i < array2.Length; i++)
			{
				if (array2[i] != array3[i])
				{
					return false;
				}
			}
		}
		else
		{
			if (array2.Length != array3.Length - 2)
			{
				return false;
			}
			int num = array2.Length - array.Length - 2;
			int num2 = array3.Length - array.Length - 2;
			array3[1] -= 2;
			array3[3] -= 2;
			for (int j = 0; j < array.Length; j++)
			{
				if (array2[num + j] != array3[num2 + j])
				{
					return false;
				}
			}
			for (int k = 0; k < num; k++)
			{
				if (array2[k] != array3[k])
				{
					return false;
				}
			}
		}
		return true;
	}

	public void Reset()
	{
		m_digest.Reset();
	}

	private byte[] DerEncode(byte[] hash)
	{
		if (m_id == null)
		{
			return hash;
		}
		return new DigestInformation(m_id, hash).GetDerEncoded();
	}
}
