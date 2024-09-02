using System;

namespace DocGen.Pdf.Security;

internal class CipherBlockChainingMode : ICipher
{
	private byte[] m_bytes;

	private byte[] m_cbcBytes;

	private byte[] m_cbcNextBytes;

	private int m_size;

	private ICipher m_cipher;

	private bool m_isEncryption;

	internal ICipher Cipher => m_cipher;

	public string AlgorithmName => m_cipher.AlgorithmName + "/CBC";

	public bool IsBlock => false;

	public int BlockSize => m_cipher.BlockSize;

	internal CipherBlockChainingMode(ICipher cipher)
	{
		m_cipher = cipher;
		m_size = cipher.BlockSize;
		m_bytes = new byte[m_size];
		m_cbcBytes = new byte[m_size];
		m_cbcNextBytes = new byte[m_size];
	}

	public void Initialize(bool isEncryption, ICipherParam parameters)
	{
		bool isEncryption2 = m_isEncryption;
		m_isEncryption = isEncryption;
		if (parameters is InvalidParameter)
		{
			InvalidParameter obj = (InvalidParameter)parameters;
			byte[] invalidBytes = obj.InvalidBytes;
			if (invalidBytes.Length != m_size)
			{
				throw new ArgumentException("Invalid size in block");
			}
			Array.Copy(invalidBytes, 0, m_bytes, 0, invalidBytes.Length);
			parameters = obj.Parameters;
		}
		Reset();
		if (parameters != null)
		{
			m_cipher.Initialize(m_isEncryption, parameters);
		}
		else if (isEncryption2 != m_isEncryption)
		{
			throw new ArgumentException("cannot change encrypting state without providing key.");
		}
	}

	public int ProcessBlock(byte[] inBytes, int inOffset, byte[] outBytes, int outOffset)
	{
		if (!m_isEncryption)
		{
			return DecryptBlock(inBytes, inOffset, outBytes, outOffset);
		}
		return EncryptBlock(inBytes, inOffset, outBytes, outOffset);
	}

	public void Reset()
	{
		Array.Copy(m_bytes, 0, m_cbcBytes, 0, m_bytes.Length);
		Array.Clear(m_cbcNextBytes, 0, m_cbcNextBytes.Length);
		m_cipher.Reset();
	}

	private int EncryptBlock(byte[] inBytes, int inOffset, byte[] outBytes, int outOffset)
	{
		if (inOffset + m_size > inBytes.Length)
		{
			throw new Exception("Invalid length in input bytes");
		}
		for (int i = 0; i < m_size; i++)
		{
			m_cbcBytes[i] ^= inBytes[inOffset + i];
		}
		int result = m_cipher.ProcessBlock(m_cbcBytes, 0, outBytes, outOffset);
		Array.Copy(outBytes, outOffset, m_cbcBytes, 0, m_cbcBytes.Length);
		return result;
	}

	private int DecryptBlock(byte[] inBytes, int inOffset, byte[] outBytes, int outOffset)
	{
		if (inOffset + m_size > inBytes.Length)
		{
			throw new Exception("Invalid length in input bytes");
		}
		Array.Copy(inBytes, inOffset, m_cbcNextBytes, 0, m_size);
		int result = m_cipher.ProcessBlock(inBytes, inOffset, outBytes, outOffset);
		for (int i = 0; i < m_size; i++)
		{
			outBytes[outOffset + i] ^= m_cbcBytes[i];
		}
		byte[] cbcBytes = m_cbcBytes;
		m_cbcBytes = m_cbcNextBytes;
		m_cbcNextBytes = cbcBytes;
		return result;
	}
}
