using System;

namespace DocGen.Pdf.Security;

internal class Pkcs1Encoding : ICipherBlock
{
	private const string m_enableSequrity = "DocGen.Pdf.Security";

	private const int m_length = 10;

	private static readonly bool[] m_securityLengthEnabled;

	private SecureRandomAlgorithm m_random;

	private ICipherBlock m_cipher;

	private bool m_isEncryption;

	private bool m_isPrivateKey;

	private bool m_useSecurityLength;

	internal bool SecurityLengthEnabled
	{
		get
		{
			return m_securityLengthEnabled[0];
		}
		set
		{
			m_securityLengthEnabled[0] = value;
		}
	}

	public string AlgorithmName => m_cipher.AlgorithmName + "/PKCS1Padding";

	public int InputBlock
	{
		get
		{
			int inputBlock = m_cipher.InputBlock;
			if (!m_isEncryption)
			{
				return inputBlock;
			}
			return inputBlock - 10;
		}
	}

	public int OutputBlock
	{
		get
		{
			int outputBlock = m_cipher.OutputBlock;
			if (!m_isEncryption)
			{
				return outputBlock - 10;
			}
			return outputBlock;
		}
	}

	static Pkcs1Encoding()
	{
		string text = null;
		m_securityLengthEnabled = new bool[1] { text?.Equals("true") ?? true };
	}

	internal Pkcs1Encoding(ICipherBlock cipher)
	{
		m_cipher = cipher;
		m_useSecurityLength = SecurityLengthEnabled;
	}

	public ICipherBlock GetUnderlyingCipher()
	{
		return m_cipher;
	}

	public void Initialize(bool forEncryption, ICipherParam parameters)
	{
		m_random = new SecureRandomAlgorithm();
		CipherParameter cipherParameter = (CipherParameter)parameters;
		m_cipher.Initialize(forEncryption, parameters);
		m_isPrivateKey = cipherParameter.IsPrivate;
		m_isEncryption = forEncryption;
	}

	public byte[] ProcessBlock(byte[] input, int inOff, int length)
	{
		if (!m_isEncryption)
		{
			return DecodeBlock(input, inOff, length);
		}
		return EncodeBlock(input, inOff, length);
	}

	private byte[] EncodeBlock(byte[] input, int inOff, int inLen)
	{
		if (inLen > InputBlock)
		{
			throw new ArgumentException("Input data too large");
		}
		byte[] array = new byte[m_cipher.InputBlock];
		if (m_isPrivateKey)
		{
			array[0] = 1;
			for (int i = 1; i != array.Length - inLen - 1; i++)
			{
				array[i] = byte.MaxValue;
			}
		}
		else
		{
			m_random.NextBytes(array);
			array[0] = 2;
			for (int j = 1; j != array.Length - inLen - 1; j++)
			{
				while (array[j] == 0)
				{
					array[j] = (byte)m_random.NextInt();
				}
			}
		}
		array[array.Length - inLen - 1] = 0;
		Array.Copy(input, inOff, array, array.Length - inLen, inLen);
		return m_cipher.ProcessBlock(array, 0, array.Length);
	}

	private byte[] DecodeBlock(byte[] input, int inOff, int inLen)
	{
		byte[] array = m_cipher.ProcessBlock(input, inOff, inLen);
		if (array.Length < OutputBlock)
		{
			throw new Exception("Invalid block. Block truncated");
		}
		byte b = array[0];
		if (b != 1 && b != 2)
		{
			throw new Exception("Invalid block type");
		}
		if (m_useSecurityLength && array.Length != m_cipher.OutputBlock)
		{
			throw new Exception("Invalid size");
		}
		int i;
		for (i = 1; i != array.Length; i++)
		{
			byte b2 = array[i];
			if (b2 == 0)
			{
				break;
			}
			if (b == 1 && b2 != byte.MaxValue)
			{
				throw new Exception("Invalid block padding");
			}
		}
		i++;
		if (i > array.Length || i < 10)
		{
			throw new Exception("no data in block");
		}
		byte[] array2 = new byte[array.Length - i];
		Array.Copy(array, i, array2, 0, array2.Length);
		return array2;
	}
}
