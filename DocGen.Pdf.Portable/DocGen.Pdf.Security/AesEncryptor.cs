using System;

namespace DocGen.Pdf.Security;

internal class AesEncryptor
{
	private int c_blockSize = 16;

	private Aes m_aes;

	private byte[] m_cbcV = new byte[16];

	private byte[] m_nextBlockV = new byte[16];

	private int m_ivOff;

	private byte[] m_buf = new byte[16];

	private bool m_isEncryption;

	internal AesEncryptor(byte[] key, byte[] iv, bool isEncryption)
	{
		if (key.Length == c_blockSize)
		{
			m_aes = new Aes(Aes.KeySize.Bits128, key);
		}
		else
		{
			m_aes = new Aes(Aes.KeySize.Bits256, key);
		}
		Array.Copy(iv, 0, m_buf, 0, iv.Length);
		Array.Copy(iv, 0, m_cbcV, 0, iv.Length);
		if (isEncryption)
		{
			m_ivOff = m_buf.Length;
		}
		m_isEncryption = isEncryption;
	}

	internal void ProcessBytes(byte[] input, int inOff, int length, byte[] output, int outOff)
	{
		if (length < 0)
		{
			throw new ArgumentException("input data length cannot be negative");
		}
		int num = 0;
		int num2 = m_buf.Length - m_ivOff;
		if (length > num2)
		{
			Array.Copy(input, inOff, m_buf, m_ivOff, num2);
			num += ProcessBlock(m_buf, 0, output, outOff);
			m_ivOff = 0;
			length -= num2;
			inOff += num2;
			while (length > m_buf.Length)
			{
				num += ProcessBlock(input, inOff, output, outOff + num);
				length -= c_blockSize;
				inOff += c_blockSize;
			}
		}
		Array.Copy(input, inOff, m_buf, m_ivOff, length);
		m_ivOff += length;
	}

	internal int Finalize(byte[] output)
	{
		int num = 0;
		int num2 = 0;
		if (m_isEncryption)
		{
			if (m_ivOff == c_blockSize)
			{
				num = ProcessBlock(m_buf, 0, output, num2);
				m_ivOff = 0;
			}
			AddPadding(m_buf, m_ivOff);
			return num + ProcessBlock(m_buf, 0, output, num2 + num);
		}
		if (m_ivOff == c_blockSize)
		{
			num = ProcessBlock(m_buf, 0, output, 0);
			m_ivOff = 0;
		}
		return num - CheckPadding(output);
	}

	internal int GetBlockSize(int length)
	{
		int num = length + m_ivOff;
		int num2 = num % m_buf.Length;
		if (num2 == 0)
		{
			return num - m_buf.Length;
		}
		return num - num2;
	}

	internal int CalculateOutputSize()
	{
		int ivOff = m_ivOff;
		int num = ivOff % m_buf.Length;
		if (num == 0)
		{
			if (m_isEncryption)
			{
				return ivOff + m_buf.Length;
			}
			return ivOff;
		}
		return ivOff - num + m_buf.Length;
	}

	private int ProcessBlock(byte[] input, int inOff, byte[] outBytes, int outOff)
	{
		int num = 0;
		if (inOff + c_blockSize > input.Length)
		{
			throw new ArgumentException("input buffer length is too short");
		}
		if (m_isEncryption)
		{
			for (int i = 0; i < c_blockSize; i++)
			{
				m_cbcV[i] ^= input[inOff + i];
			}
			num = m_aes.Cipher(m_cbcV, outBytes, outOff);
			Array.Copy(outBytes, outOff, m_cbcV, 0, m_cbcV.Length);
		}
		else
		{
			Array.Copy(input, inOff, m_nextBlockV, 0, c_blockSize);
			num = m_aes.InvCipher(m_nextBlockV, outBytes, outOff);
			for (int j = 0; j < c_blockSize; j++)
			{
				outBytes[outOff + j] ^= m_cbcV[j];
			}
			byte[] cbcV = m_cbcV;
			m_cbcV = m_nextBlockV;
			m_nextBlockV = cbcV;
		}
		return num;
	}

	private static int AddPadding(byte[] input, int inOff)
	{
		byte b = (byte)(input.Length - inOff);
		while (inOff < input.Length)
		{
			input[inOff] = b;
			inOff++;
		}
		return b;
	}

	private static int CheckPadding(byte[] input)
	{
		int num = input[^1] & 0xFF;
		for (int i = 1; i <= num; i++)
		{
			if (input[^i] != num)
			{
				num = 0;
			}
		}
		return num;
	}
}
