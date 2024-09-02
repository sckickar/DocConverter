using System;
using System.Text;

namespace DocGen.Compression.Zip;

internal class RFC2898Key
{
	private int m_block;

	private byte[] m_rfcBuffer;

	private int m_endOffset;

	private int m_iterations;

	private byte[] m_salt;

	private int m_startOffset;

	private byte[] m_password;

	private int m_blockSizeValue = 64;

	private byte[] m_inner;

	private byte[] m_outer;

	private bool m_hashing;

	private byte[] m_keyVal;

	private byte[] m_buffer;

	private long m_count;

	private uint[] m_stateSHA1;

	private uint[] m_expandedBuffer;

	private byte[] m_hmacHashVal;

	private byte[] m_sha1HashVal;

	internal byte[] Hash => (byte[])m_hmacHashVal.Clone();

	internal RFC2898Key(string password, byte[] salt, int iterations)
		: this(Encoding.UTF8.GetBytes(password), salt, iterations)
	{
	}

	internal RFC2898Key(byte[] password, byte[] salt, int iterations)
	{
		m_password = password;
		m_salt = salt;
		m_iterations = iterations;
		InitializeKey(password);
		m_stateSHA1 = new uint[5];
		m_buffer = new byte[64];
		m_expandedBuffer = new uint[80];
		InitializeState();
		InitializeRfc2898();
	}

	private byte[] ByteArray(int input)
	{
		byte[] bytes = BitConverter.GetBytes(input);
		byte[] result = new byte[4]
		{
			bytes[3],
			bytes[2],
			bytes[1],
			bytes[0]
		};
		if (!BitConverter.IsLittleEndian)
		{
			return bytes;
		}
		return result;
	}

	private byte[] DeriveCryptographicKey()
	{
		byte[] array = ByteArray(m_block);
		byte[] array2 = new byte[m_salt.Length + array.Length];
		Buffer.BlockCopy(m_salt, 0, array2, 0, m_salt.Length);
		Buffer.BlockCopy(array, 0, array2, m_salt.Length, array.Length);
		ComputeHash(array2);
		byte[] array3 = Hash;
		byte[] array4 = array3;
		for (int i = 2; i <= m_iterations; i++)
		{
			array3 = ComputeHash(array3);
			for (int j = 0; j < 20; j++)
			{
				array4[j] ^= array3[j];
			}
		}
		m_block++;
		return array4;
	}

	internal byte[] GetBytes(int length)
	{
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		byte[] array = new byte[length];
		int i = 0;
		int num = m_endOffset - m_startOffset;
		if (num > 0)
		{
			if (length < num)
			{
				Buffer.BlockCopy(m_rfcBuffer, m_startOffset, array, 0, length);
				m_startOffset += length;
				return array;
			}
			Buffer.BlockCopy(m_rfcBuffer, m_startOffset, array, 0, num);
			m_startOffset = (m_endOffset = 0);
			i += num;
		}
		for (; i < length; i += 20)
		{
			byte[] src = DeriveCryptographicKey();
			int num2 = length - i;
			if (num2 > 20)
			{
				Buffer.BlockCopy(src, 0, array, i, 20);
				continue;
			}
			Buffer.BlockCopy(src, 0, array, i, num2);
			i += num2;
			Buffer.BlockCopy(src, num2, m_rfcBuffer, m_startOffset, 20 - num2);
			m_endOffset += 20 - num2;
			return array;
		}
		return array;
	}

	private void InitializeRfc2898()
	{
		if (m_rfcBuffer != null)
		{
			Array.Clear(m_rfcBuffer, 0, m_rfcBuffer.Length);
		}
		m_rfcBuffer = new byte[20];
		m_block = 1;
		m_startOffset = (m_endOffset = 0);
	}

	private void UpdateInnerAndOuterArrays()
	{
		if (m_inner == null)
		{
			m_inner = new byte[m_blockSizeValue];
		}
		if (m_outer == null)
		{
			m_outer = new byte[m_blockSizeValue];
		}
		for (int i = 0; i < m_blockSizeValue; i++)
		{
			m_inner[i] = 54;
			m_outer[i] = 92;
		}
		for (int j = 0; j < m_keyVal.Length; j++)
		{
			byte[] inner = m_inner;
			int num = j;
			inner[num] ^= m_keyVal[j];
			byte[] outer = m_outer;
			int num2 = j;
			outer[num2] ^= m_keyVal[j];
		}
	}

	private void InitializeKey(byte[] keyVal)
	{
		m_inner = null;
		m_outer = null;
		if (keyVal.Length > m_blockSizeValue)
		{
			m_keyVal = ComputeHash(keyVal);
		}
		else
		{
			m_keyVal = (byte[])keyVal.Clone();
		}
		UpdateInnerAndOuterArrays();
	}

	internal byte[] ComputeHash(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (!m_hashing)
		{
			UpdateBlock(m_inner, 0, m_inner.Length, m_inner, 0);
			m_hashing = true;
		}
		UpdateBlock(data, 0, data.Length, data, 0);
		if (!m_hashing)
		{
			UpdateBlock(m_inner, 0, m_inner.Length, m_inner, 0);
			m_hashing = true;
		}
		UpdateFinalBlock(new byte[0], 0, 0);
		byte[] sha1HashVal = m_sha1HashVal;
		m_sha1HashVal = null;
		Initialize();
		UpdateBlock(m_outer, 0, m_outer.Length, m_outer, 0);
		UpdateBlock(sha1HashVal, 0, sha1HashVal.Length, sha1HashVal, 0);
		m_hashing = false;
		UpdateFinalBlock(new byte[0], 0, 0);
		m_hmacHashVal = m_sha1HashVal;
		byte[] result = (byte[])m_hmacHashVal.Clone();
		Initialize();
		m_hashing = false;
		return result;
	}

	private void UpdateBlock(byte[] input, int inputOff, int count, byte[] output, int outputOff)
	{
		UpdateHashData(input, inputOff, count);
		if (output != null && (input != output || inputOff != outputOff))
		{
			Buffer.BlockCopy(input, inputOff, output, outputOff, count);
		}
	}

	private void UpdateFinalBlock(byte[] input, int inputOff, int count)
	{
		UpdateHashData(input, inputOff, count);
		m_sha1HashVal = UpdateEndHash();
		byte[] dst = new byte[count];
		if (count != 0)
		{
			Buffer.BlockCopy(input, inputOff, dst, 0, count);
		}
	}

	private void Initialize()
	{
		InitializeState();
		Array.Clear(m_buffer, 0, m_buffer.Length);
		Array.Clear(m_expandedBuffer, 0, m_expandedBuffer.Length);
	}

	private void InitializeState()
	{
		m_count = 0L;
		m_stateSHA1[0] = 1732584193u;
		m_stateSHA1[1] = 4023233417u;
		m_stateSHA1[2] = 2562383102u;
		m_stateSHA1[3] = 271733878u;
		m_stateSHA1[4] = 3285377520u;
	}

	private void UpdateHashData(byte[] inputData, int startOffSet, int size)
	{
		int num = size;
		int num2 = startOffSet;
		int num3 = (int)(m_count & 0x3F);
		m_count += num;
		uint[] stateSHA = m_stateSHA1;
		byte[] buffer = m_buffer;
		uint[] expandedBuffer = m_expandedBuffer;
		if (num3 > 0 && num3 + num >= 64)
		{
			Buffer.BlockCopy(inputData, num2, m_buffer, num3, 64 - num3);
			num2 += 64 - num3;
			num -= 64 - num3;
			SHAModify(expandedBuffer, stateSHA, buffer);
			num3 = 0;
		}
		while (num >= 64)
		{
			Buffer.BlockCopy(inputData, num2, m_buffer, 0, 64);
			num2 += 64;
			num -= 64;
			SHAModify(expandedBuffer, stateSHA, buffer);
		}
		if (num > 0)
		{
			Buffer.BlockCopy(inputData, num2, m_buffer, num3, num);
		}
	}

	private byte[] UpdateEndHash()
	{
		byte[] array = new byte[20];
		int num = 64 - (int)(m_count & 0x3F);
		if (num <= 8)
		{
			num += 64;
		}
		byte[] array2 = new byte[num];
		array2[0] = 128;
		long num2 = m_count * 8;
		array2[num - 8] = (byte)((num2 >> 56) & 0xFF);
		array2[num - 7] = (byte)((num2 >> 48) & 0xFF);
		array2[num - 6] = (byte)((num2 >> 40) & 0xFF);
		array2[num - 5] = (byte)((num2 >> 32) & 0xFF);
		array2[num - 4] = (byte)((num2 >> 24) & 0xFF);
		array2[num - 3] = (byte)((num2 >> 16) & 0xFF);
		array2[num - 2] = (byte)((num2 >> 8) & 0xFF);
		array2[num - 1] = (byte)(num2 & 0xFF);
		UpdateHashData(array2, 0, array2.Length);
		DWORDToBigEndian(array, m_stateSHA1, 5);
		m_sha1HashVal = array;
		return array;
	}

	private void SHAModify(uint[] expandedBuffer, uint[] state, byte[] block)
	{
		uint num = state[0];
		uint num2 = state[1];
		uint num3 = state[2];
		uint num4 = state[3];
		uint num5 = state[4];
		DWORDFromBigEndian(expandedBuffer, 16, block);
		SHAExpansion(expandedBuffer);
		int i;
		for (i = 0; i < 20; i += 5)
		{
			num5 += ((num << 5) | (num >> 27)) + (num4 ^ (num2 & (num3 ^ num4))) + expandedBuffer[i] + 1518500249;
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += ((num5 << 5) | (num5 >> 27)) + (num3 ^ (num & (num2 ^ num3))) + expandedBuffer[i + 1] + 1518500249;
			num = (num << 30) | (num >> 2);
			num3 += ((num4 << 5) | (num4 >> 27)) + (num2 ^ (num5 & (num ^ num2))) + expandedBuffer[i + 2] + 1518500249;
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += ((num3 << 5) | (num3 >> 27)) + (num ^ (num4 & (num5 ^ num))) + expandedBuffer[i + 3] + 1518500249;
			num4 = (num4 << 30) | (num4 >> 2);
			num += ((num2 << 5) | (num2 >> 27)) + (num5 ^ (num3 & (num4 ^ num5))) + expandedBuffer[i + 4] + 1518500249;
			num3 = (num3 << 30) | (num3 >> 2);
		}
		for (; i < 40; i += 5)
		{
			num5 += ((num << 5) | (num >> 27)) + (num2 ^ num3 ^ num4) + expandedBuffer[i] + 1859775393;
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += ((num5 << 5) | (num5 >> 27)) + (num ^ num2 ^ num3) + expandedBuffer[i + 1] + 1859775393;
			num = (num << 30) | (num >> 2);
			num3 += ((num4 << 5) | (num4 >> 27)) + (num5 ^ num ^ num2) + expandedBuffer[i + 2] + 1859775393;
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += ((num3 << 5) | (num3 >> 27)) + (num4 ^ num5 ^ num) + expandedBuffer[i + 3] + 1859775393;
			num4 = (num4 << 30) | (num4 >> 2);
			num += ((num2 << 5) | (num2 >> 27)) + (num3 ^ num4 ^ num5) + expandedBuffer[i + 4] + 1859775393;
			num3 = (num3 << 30) | (num3 >> 2);
		}
		for (; i < 60; i += 5)
		{
			num5 += (uint)((int)(((num << 5) | (num >> 27)) + ((num2 & num3) | (num4 & (num2 | num3))) + expandedBuffer[i]) + -1894007588);
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += (uint)((int)(((num5 << 5) | (num5 >> 27)) + ((num & num2) | (num3 & (num | num2))) + expandedBuffer[i + 1]) + -1894007588);
			num = (num << 30) | (num >> 2);
			num3 += (uint)((int)(((num4 << 5) | (num4 >> 27)) + ((num5 & num) | (num2 & (num5 | num))) + expandedBuffer[i + 2]) + -1894007588);
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += (uint)((int)(((num3 << 5) | (num3 >> 27)) + ((num4 & num5) | (num & (num4 | num5))) + expandedBuffer[i + 3]) + -1894007588);
			num4 = (num4 << 30) | (num4 >> 2);
			num += (uint)((int)(((num2 << 5) | (num2 >> 27)) + ((num3 & num4) | (num5 & (num3 | num4))) + expandedBuffer[i + 4]) + -1894007588);
			num3 = (num3 << 30) | (num3 >> 2);
		}
		for (; i < 80; i += 5)
		{
			num5 += (uint)((int)(((num << 5) | (num >> 27)) + (num2 ^ num3 ^ num4) + expandedBuffer[i]) + -899497514);
			num2 = (num2 << 30) | (num2 >> 2);
			num4 += (uint)((int)(((num5 << 5) | (num5 >> 27)) + (num ^ num2 ^ num3) + expandedBuffer[i + 1]) + -899497514);
			num = (num << 30) | (num >> 2);
			num3 += (uint)((int)(((num4 << 5) | (num4 >> 27)) + (num5 ^ num ^ num2) + expandedBuffer[i + 2]) + -899497514);
			num5 = (num5 << 30) | (num5 >> 2);
			num2 += (uint)((int)(((num3 << 5) | (num3 >> 27)) + (num4 ^ num5 ^ num) + expandedBuffer[i + 3]) + -899497514);
			num4 = (num4 << 30) | (num4 >> 2);
			num += (uint)((int)(((num2 << 5) | (num2 >> 27)) + (num3 ^ num4 ^ num5) + expandedBuffer[i + 4]) + -899497514);
			num3 = (num3 << 30) | (num3 >> 2);
		}
		state[0] += num;
		state[1] += num2;
		state[2] += num3;
		state[3] += num4;
		state[4] += num5;
	}

	private void SHAExpansion(uint[] input)
	{
		for (int i = 16; i < 80; i++)
		{
			uint num = input[i - 3] ^ input[i - 8] ^ input[i - 14] ^ input[i - 16];
			input[i] = (num << 1) | (num >> 31);
		}
	}

	private void DWORDFromBigEndian(uint[] input, int digits, byte[] block)
	{
		int num = 0;
		int num2 = 0;
		while (num < digits)
		{
			input[num] = (uint)((block[num2] << 24) | (block[num2 + 1] << 16) | (block[num2 + 2] << 8) | block[num2 + 3]);
			num++;
			num2 += 4;
		}
	}

	private void DWORDToBigEndian(byte[] blockData, uint[] output, int digits)
	{
		int num = 0;
		int num2 = 0;
		while (num < digits)
		{
			blockData[num2] = (byte)((output[num] >> 24) & 0xFFu);
			blockData[num2 + 1] = (byte)((output[num] >> 16) & 0xFFu);
			blockData[num2 + 2] = (byte)((output[num] >> 8) & 0xFFu);
			blockData[num2 + 3] = (byte)(output[num] & 0xFFu);
			num++;
			num2 += 4;
		}
	}
}
