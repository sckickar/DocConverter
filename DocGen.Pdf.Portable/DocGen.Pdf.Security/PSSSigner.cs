using System;

namespace DocGen.Pdf.Security;

internal class PSSSigner : ISigner
{
	public const byte m_trailer = 188;

	private readonly IMessageDigest m_digest1;

	private readonly IMessageDigest m_digest2;

	private readonly IMessageDigest m_pssDigest;

	private readonly ICipherBlock m_cipher;

	private SecureRandomAlgorithm m_random;

	private int m_hashLength;

	private int m_digestLength;

	private int m_saltLength;

	private int m_emBits;

	private byte[] m_salt;

	private byte[] m_Dash;

	private byte[] m_blockCipher;

	private byte m_trailerBlock;

	public string AlgorithmName => m_pssDigest.AlgorithmName + "withRSAandMGF1";

	public static PSSSigner CreateRawSigner(ICipherBlock m_cipher, IMessageDigest digest)
	{
		return new PSSSigner(m_cipher, new NullMessageDigest(), digest, digest, digest.MessageDigestSize, 188);
	}

	public static PSSSigner CreateRawSigner(ICipherBlock m_cipher, IMessageDigest contentDigest, IMessageDigest m_pssDigest, int saltLen, byte m_trailerBlock)
	{
		return new PSSSigner(m_cipher, new NullMessageDigest(), contentDigest, m_pssDigest, saltLen, m_trailerBlock);
	}

	public PSSSigner(ICipherBlock m_cipher, IMessageDigest digest)
		: this(m_cipher, digest, digest.MessageDigestSize)
	{
	}

	public PSSSigner(ICipherBlock m_cipher, IMessageDigest digest, int saltLen)
		: this(m_cipher, digest, saltLen, 188)
	{
	}

	public PSSSigner(ICipherBlock m_cipher, IMessageDigest contentDigest, IMessageDigest m_pssDigest, int saltLen)
		: this(m_cipher, contentDigest, m_pssDigest, saltLen, 188)
	{
	}

	public PSSSigner(ICipherBlock m_cipher, IMessageDigest digest, int saltLen, byte m_trailerBlock)
		: this(m_cipher, digest, digest, saltLen, 188)
	{
	}

	public PSSSigner(ICipherBlock m_cipher, IMessageDigest contentDigest, IMessageDigest m_pssDigest, int saltLen, byte m_trailerBlock)
		: this(m_cipher, contentDigest, contentDigest, m_pssDigest, saltLen, m_trailerBlock)
	{
	}

	private PSSSigner(ICipherBlock m_cipher, IMessageDigest m_digest1, IMessageDigest m_digest2, IMessageDigest m_pssDigest, int saltLen, byte m_trailerBlock)
	{
		this.m_cipher = m_cipher;
		this.m_digest1 = m_digest1;
		this.m_digest2 = m_digest2;
		this.m_pssDigest = m_pssDigest;
		m_hashLength = m_digest2.MessageDigestSize;
		m_digestLength = m_pssDigest.MessageDigestSize;
		m_saltLength = saltLen;
		m_salt = new byte[saltLen];
		m_Dash = new byte[8 + saltLen + m_hashLength];
		this.m_trailerBlock = m_trailerBlock;
	}

	public void BlockUpdate(byte[] bytes, int offset, int length)
	{
		m_digest1.BlockUpdate(bytes, offset, length);
	}

	public byte[] GenerateSignature()
	{
		m_digest1.DoFinal(m_Dash, m_Dash.Length - m_hashLength - m_saltLength);
		if (m_saltLength != 0)
		{
			m_random.NextBytes(m_salt);
			m_salt.CopyTo(m_Dash, m_Dash.Length - m_saltLength);
		}
		byte[] array = new byte[m_hashLength];
		m_digest2.BlockUpdate(m_Dash, 0, m_Dash.Length);
		m_digest2.DoFinal(array, 0);
		m_blockCipher[m_blockCipher.Length - m_saltLength - 1 - m_hashLength - 1] = 1;
		m_salt.CopyTo(m_blockCipher, m_blockCipher.Length - m_saltLength - m_hashLength - 1);
		byte[] array2 = ComputeMask(array, 0, array.Length, m_blockCipher.Length - m_hashLength - 1);
		for (int i = 0; i != array2.Length; i++)
		{
			m_blockCipher[i] ^= array2[i];
		}
		m_blockCipher[0] &= (byte)(255 >> m_blockCipher.Length * 8 - m_emBits);
		array.CopyTo(m_blockCipher, m_blockCipher.Length - m_hashLength - 1);
		m_blockCipher[m_blockCipher.Length - 1] = m_trailerBlock;
		byte[] result = m_cipher.ProcessBlock(m_blockCipher, 0, m_blockCipher.Length);
		ClearBlock(m_blockCipher);
		return result;
	}

	public void Initialize(bool isSigning, ICipherParam parameters)
	{
		m_random = new SecureRandomAlgorithm();
		m_cipher.Initialize(isSigning, parameters);
		RsaKeyParam rsaKeyParam = parameters as RsaKeyParam;
		m_emBits = rsaKeyParam.Modulus.BitLength - 1;
		if (m_emBits < 8 * m_hashLength + 8 * m_saltLength + 9)
		{
			throw new ArgumentException("Small key is used for hash");
		}
		m_blockCipher = new byte[(m_emBits + 7) / 8];
	}

	public void Reset()
	{
		m_digest1.Reset();
	}

	public void Update(byte input)
	{
		m_digest1.Update(input);
	}

	public bool ValidateSignature(byte[] signature)
	{
		m_digest1.DoFinal(m_Dash, m_Dash.Length - m_hashLength - m_saltLength);
		byte[] array = m_cipher.ProcessBlock(signature, 0, signature.Length);
		array.CopyTo(m_blockCipher, m_blockCipher.Length - array.Length);
		if (m_blockCipher[m_blockCipher.Length - 1] != m_trailerBlock)
		{
			ClearBlock(m_blockCipher);
			return false;
		}
		byte[] array2 = ComputeMask(m_blockCipher, m_blockCipher.Length - m_hashLength - 1, m_hashLength, m_blockCipher.Length - m_hashLength - 1);
		for (int i = 0; i != array2.Length; i++)
		{
			m_blockCipher[i] ^= array2[i];
		}
		m_blockCipher[0] &= (byte)(255 >> m_blockCipher.Length * 8 - m_emBits);
		for (int j = 0; j != m_blockCipher.Length - m_hashLength - m_saltLength - 2; j++)
		{
			if (m_blockCipher[j] != 0)
			{
				ClearBlock(m_blockCipher);
				return false;
			}
		}
		if (m_blockCipher[m_blockCipher.Length - m_hashLength - m_saltLength - 2] != 1)
		{
			ClearBlock(m_blockCipher);
			return false;
		}
		Array.Copy(m_blockCipher, m_blockCipher.Length - m_saltLength - m_hashLength - 1, m_Dash, m_Dash.Length - m_saltLength, m_saltLength);
		m_digest2.BlockUpdate(m_Dash, 0, m_Dash.Length);
		m_digest2.DoFinal(m_Dash, m_Dash.Length - m_hashLength);
		int num = m_blockCipher.Length - m_hashLength - 1;
		for (int k = m_Dash.Length - m_hashLength; k != m_Dash.Length; k++)
		{
			if ((m_blockCipher[num] ^ m_Dash[k]) != 0)
			{
				ClearBlock(m_Dash);
				ClearBlock(m_blockCipher);
				return false;
			}
			num++;
		}
		ClearBlock(m_Dash);
		ClearBlock(m_blockCipher);
		return true;
	}

	private byte[] ComputeMask(byte[] maskD, int maskOff, int maskLen, int size)
	{
		byte[] array = new byte[size];
		byte[] array2 = new byte[m_digestLength];
		byte[] array3 = new byte[4];
		int i = 0;
		m_pssDigest.Reset();
		for (; i < size / m_digestLength; i++)
		{
			ComputeItoOSP(i, array3);
			m_pssDigest.BlockUpdate(maskD, maskOff, maskLen);
			m_pssDigest.BlockUpdate(array3, 0, array3.Length);
			m_pssDigest.DoFinal(array2, 0);
			array2.CopyTo(array, i * m_digestLength);
		}
		if (i * m_digestLength < size)
		{
			ComputeItoOSP(i, array3);
			m_pssDigest.BlockUpdate(maskD, maskOff, maskLen);
			m_pssDigest.BlockUpdate(array3, 0, array3.Length);
			m_pssDigest.DoFinal(array2, 0);
			Array.Copy(array2, 0, array, i * m_digestLength, array.Length - i * m_digestLength);
		}
		return array;
	}

	private void ComputeItoOSP(int m, byte[] osp)
	{
		osp[0] = (byte)((uint)m >> 24);
		osp[1] = (byte)((uint)m >> 16);
		osp[2] = (byte)((uint)m >> 8);
		osp[3] = (byte)m;
	}

	private void ClearBlock(byte[] input)
	{
		Array.Clear(input, 0, input.Length);
	}
}
