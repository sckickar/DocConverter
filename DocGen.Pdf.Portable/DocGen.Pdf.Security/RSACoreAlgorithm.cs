using System;

namespace DocGen.Pdf.Security;

internal class RSACoreAlgorithm
{
	private RsaKeyParam m_key;

	private bool m_isEncryption;

	private int m_bitSize;

	internal int InputBlockSize
	{
		get
		{
			if (m_isEncryption)
			{
				return (m_bitSize - 1) / 8;
			}
			return (m_bitSize + 7) / 8;
		}
	}

	internal int OutputBlockSize
	{
		get
		{
			if (m_isEncryption)
			{
				return (m_bitSize + 7) / 8;
			}
			return (m_bitSize - 1) / 8;
		}
	}

	internal void Initialize(bool isEncryption, ICipherParam parameters)
	{
		if (!(parameters is RsaKeyParam))
		{
			throw new Exception("Invalid RSA key");
		}
		m_key = (RsaKeyParam)parameters;
		m_isEncryption = isEncryption;
		m_bitSize = m_key.Modulus.BitLength;
	}

	internal Number ConvertInput(byte[] bytes, int offset, int length)
	{
		int num = (m_bitSize + 7) / 8;
		if (length > num)
		{
			throw new Exception("Invalid length in inputs");
		}
		Number number = new Number(1, bytes, offset, length);
		if (number.CompareTo(m_key.Modulus) >= 0)
		{
			throw new Exception("Invalid length in inputs");
		}
		return number;
	}

	internal byte[] ConvertOutput(Number result)
	{
		byte[] array = result.ToByteArrayUnsigned();
		if (m_isEncryption)
		{
			int outputBlockSize = OutputBlockSize;
			if (array.Length < outputBlockSize)
			{
				byte[] array2 = new byte[outputBlockSize];
				array.CopyTo(array2, array2.Length - array.Length);
				array = array2;
			}
		}
		return array;
	}

	internal Number ProcessBlock(Number input)
	{
		if (m_key is RsaPrivateKeyParam)
		{
			RsaPrivateKeyParam obj = (RsaPrivateKeyParam)m_key;
			Number p = obj.P;
			Number q = obj.Q;
			Number dP = obj.DP;
			Number dQ = obj.DQ;
			Number qInv = obj.QInv;
			Number number = input.Remainder(p).ModPow(dP, p);
			Number number2 = input.Remainder(q).ModPow(dQ, q);
			return number.Subtract(number2).Multiply(qInv).Mod(p)
				.Multiply(q)
				.Add(number2);
		}
		return input.ModPow(m_key.Exponent, m_key.Modulus);
	}
}
