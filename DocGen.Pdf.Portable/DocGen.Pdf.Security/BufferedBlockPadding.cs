using System;

namespace DocGen.Pdf.Security;

internal class BufferedBlockPadding : BufferedCipher
{
	private IPadding m_padding;

	internal BufferedBlockPadding(ICipher cipher, IPadding padding)
	{
		m_cipher = cipher;
		m_padding = padding;
		m_bytes = new byte[cipher.BlockSize];
		m_offset = 0;
	}

	internal BufferedBlockPadding(ICipher cipher)
		: this(cipher, new Pkcs7Padding())
	{
	}

	public override void Initialize(bool isEncryption, ICipherParam parameters)
	{
		m_isEncryption = isEncryption;
		SecureRandomAlgorithm random = null;
		Reset();
		m_padding.Initialize(random);
		m_cipher.Initialize(isEncryption, parameters);
	}

	public override int GetOutputSize(int length)
	{
		int num = length + m_offset;
		int num2 = num % m_bytes.Length;
		if (num2 == 0)
		{
			if (m_isEncryption)
			{
				return num + m_bytes.Length;
			}
			return num;
		}
		return num - num2 + m_bytes.Length;
	}

	public override int GetUpdateOutputSize(int length)
	{
		int num = length + m_offset;
		int num2 = num % m_bytes.Length;
		if (num2 == 0)
		{
			return num - m_bytes.Length;
		}
		return num - num2;
	}

	public override int ProcessByte(byte input, byte[] output, int outOff)
	{
		int result = 0;
		if (m_offset == m_bytes.Length)
		{
			result = m_cipher.ProcessBlock(m_bytes, 0, output, outOff);
			m_offset = 0;
		}
		m_bytes[m_offset++] = input;
		return result;
	}

	public override int ProcessBytes(byte[] input, int inOffset, int length, byte[] output, int outOffset)
	{
		if (length < 0)
		{
			throw new ArgumentException("Invalid length");
		}
		int blockSize = BlockSize;
		int updateOutputSize = GetUpdateOutputSize(length);
		if (updateOutputSize > 0 && outOffset + updateOutputSize > output.Length)
		{
			throw new Exception("Invalid buffer length");
		}
		int num = 0;
		int num2 = m_bytes.Length - m_offset;
		if (length > num2)
		{
			Array.Copy(input, inOffset, m_bytes, m_offset, num2);
			num += m_cipher.ProcessBlock(m_bytes, 0, output, outOffset);
			m_offset = 0;
			length -= num2;
			inOffset += num2;
			while (length > m_bytes.Length)
			{
				num += m_cipher.ProcessBlock(input, inOffset, output, outOffset + num);
				length -= blockSize;
				inOffset += blockSize;
			}
		}
		Array.Copy(input, inOffset, m_bytes, m_offset, length);
		m_offset += length;
		return num;
	}

	public override int DoFinal(byte[] output, int outOff)
	{
		int blockSize = m_cipher.BlockSize;
		int num = 0;
		if (m_isEncryption)
		{
			if (m_offset == blockSize)
			{
				if (outOff + 2 * blockSize > output.Length)
				{
					Reset();
					throw new Exception("output buffer too short");
				}
				num = m_cipher.ProcessBlock(m_bytes, 0, output, outOff);
				m_offset = 0;
			}
			m_padding.AddPadding(m_bytes, m_offset);
			num += m_cipher.ProcessBlock(m_bytes, 0, output, outOff + num);
			Reset();
		}
		else
		{
			if (m_offset != blockSize)
			{
				Reset();
				throw new Exception("incomplete in decryption");
			}
			num = m_cipher.ProcessBlock(m_bytes, 0, m_bytes, 0);
			m_offset = 0;
			try
			{
				num -= m_padding.Count(m_bytes);
				Array.Copy(m_bytes, 0, output, outOff, num);
			}
			finally
			{
				Reset();
			}
		}
		return num;
	}
}
