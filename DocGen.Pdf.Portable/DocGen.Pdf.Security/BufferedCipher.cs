using System;

namespace DocGen.Pdf.Security;

internal class BufferedCipher : BufferedBlockPaddingBase
{
	internal byte[] m_bytes;

	internal int m_offset;

	internal bool m_isEncryption;

	internal ICipher m_cipher;

	public override string AlgorithmName => m_cipher.AlgorithmName;

	public override int BlockSize => m_cipher.BlockSize;

	protected BufferedCipher()
	{
	}

	internal BufferedCipher(ICipher cipher)
	{
		if (cipher == null)
		{
			throw new ArgumentNullException("cipher");
		}
		m_cipher = cipher;
		m_bytes = new byte[cipher.BlockSize];
		m_offset = 0;
	}

	public override void Initialize(bool isEncryption, ICipherParam parameters)
	{
		m_isEncryption = isEncryption;
		Reset();
		m_cipher.Initialize(isEncryption, parameters);
	}

	public override int GetUpdateOutputSize(int length)
	{
		int num = length + m_offset;
		int num2 = num % m_bytes.Length;
		return num - num2;
	}

	public override int GetOutputSize(int length)
	{
		return length + m_offset;
	}

	public override int ProcessByte(byte input, byte[] bytes, int offset)
	{
		m_bytes[m_offset++] = input;
		if (m_offset == m_bytes.Length)
		{
			if (offset + m_bytes.Length > bytes.Length)
			{
				throw new Exception("output buffer too short");
			}
			m_offset = 0;
			return m_cipher.ProcessBlock(m_bytes, 0, bytes, offset);
		}
		return 0;
	}

	public override byte[] ProcessByte(byte input)
	{
		int updateOutputSize = GetUpdateOutputSize(1);
		byte[] array = ((updateOutputSize > 0) ? new byte[updateOutputSize] : null);
		int num = ProcessByte(input, array, 0);
		if (updateOutputSize > 0 && num < updateOutputSize)
		{
			byte[] array2 = new byte[num];
			Array.Copy(array, 0, array2, 0, num);
			array = array2;
		}
		return array;
	}

	public override byte[] ProcessBytes(byte[] input, int offset, int length)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (length < 1)
		{
			return null;
		}
		int updateOutputSize = GetUpdateOutputSize(length);
		byte[] array = ((updateOutputSize > 0) ? new byte[updateOutputSize] : null);
		int num = ProcessBytes(input, offset, length, array, 0);
		if (updateOutputSize > 0 && num < updateOutputSize)
		{
			byte[] array2 = new byte[num];
			Array.Copy(array, 0, array2, 0, num);
			array = array2;
		}
		return array;
	}

	public override int ProcessBytes(byte[] input, int inOffset, int length, byte[] output, int outOffset)
	{
		if (length < 1)
		{
			return 0;
		}
		int blockSize = BlockSize;
		GetUpdateOutputSize(length);
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
		if (m_offset == m_bytes.Length)
		{
			num += m_cipher.ProcessBlock(m_bytes, 0, output, outOffset + num);
			m_offset = 0;
		}
		return num;
	}

	public override byte[] DoFinal()
	{
		byte[] array = BufferedBlockPaddingBase.EmptyBuffer;
		int outputSize = GetOutputSize(0);
		if (outputSize > 0)
		{
			array = new byte[outputSize];
			int num = DoFinal(array, 0);
			if (num < array.Length)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, 0, array2, 0, num);
				array = array2;
			}
		}
		else
		{
			Reset();
		}
		return array;
	}

	public override byte[] DoFinal(byte[] input, int inOffset, int inLength)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		int outputSize = GetOutputSize(inLength);
		byte[] array = BufferedBlockPaddingBase.EmptyBuffer;
		if (outputSize > 0)
		{
			array = new byte[outputSize];
			int num = ((inLength > 0) ? ProcessBytes(input, inOffset, inLength, array, 0) : 0);
			num += DoFinal(array, num);
			if (num < array.Length)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, 0, array2, 0, num);
				array = array2;
			}
		}
		else
		{
			Reset();
		}
		return array;
	}

	public override int DoFinal(byte[] bytes, int offset)
	{
		try
		{
			if (m_offset != 0)
			{
				m_cipher.ProcessBlock(m_bytes, 0, m_bytes, 0);
				Array.Copy(m_bytes, 0, bytes, offset, m_offset);
			}
			return m_offset;
		}
		finally
		{
			Reset();
		}
	}

	public override void Reset()
	{
		Array.Clear(m_bytes, 0, m_bytes.Length);
		m_offset = 0;
		m_cipher.Reset();
	}
}
