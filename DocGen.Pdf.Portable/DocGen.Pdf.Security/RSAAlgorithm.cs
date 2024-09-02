using System;

namespace DocGen.Pdf.Security;

internal class RSAAlgorithm : ICipherBlock
{
	private RSACoreAlgorithm m_rsaCoreEngine = new RSACoreAlgorithm();

	private RsaKeyParam m_key;

	private SecureRandomAlgorithm m_random;

	public string AlgorithmName => "RSA";

	public int InputBlock => m_rsaCoreEngine.InputBlockSize;

	public int OutputBlock => m_rsaCoreEngine.OutputBlockSize;

	public void Initialize(bool isEncryption, ICipherParam parameter)
	{
		m_rsaCoreEngine.Initialize(isEncryption, parameter);
		m_key = (RsaKeyParam)parameter;
		m_random = new SecureRandomAlgorithm();
	}

	public byte[] ProcessBlock(byte[] bytes, int offset, int length)
	{
		if (m_key == null)
		{
			throw new InvalidOperationException("Invalid RSA engine");
		}
		Number number = m_rsaCoreEngine.ConvertInput(bytes, offset, length);
		Number result;
		if (m_key is RsaPrivateKeyParam)
		{
			RsaPrivateKeyParam rsaPrivateKeyParam = (RsaPrivateKeyParam)m_key;
			Number publicExponent = rsaPrivateKeyParam.PublicExponent;
			if (publicExponent != null)
			{
				Number modulus = rsaPrivateKeyParam.Modulus;
				Number number2 = CreateRandomInRange(Number.One, modulus.Subtract(Number.One), m_random);
				Number input = number2.ModPow(publicExponent, modulus).Multiply(number).Mod(modulus);
				Number number3 = m_rsaCoreEngine.ProcessBlock(input);
				Number val = number2.ModInverse(modulus);
				result = number3.Multiply(val).Mod(modulus);
			}
			else
			{
				result = m_rsaCoreEngine.ProcessBlock(number);
			}
		}
		else
		{
			result = m_rsaCoreEngine.ProcessBlock(number);
		}
		return m_rsaCoreEngine.ConvertOutput(result);
	}

	internal Number CreateRandomInRange(Number minimum, Number maximum, SecureRandomAlgorithm random)
	{
		if (minimum.CompareTo(maximum) >= 0)
		{
			return minimum;
		}
		if (minimum.BitLength > maximum.BitLength / 2)
		{
			return CreateRandomInRange(Number.Zero, maximum.Subtract(minimum), random).Add(minimum);
		}
		for (int i = 0; i < 1000; i++)
		{
			Number number = new Number(maximum.BitLength, random);
			if (number.CompareTo(minimum) >= 0 && number.CompareTo(maximum) <= 0)
			{
				return number;
			}
		}
		return new Number(maximum.Subtract(minimum).BitLength - 1, random).Add(minimum);
	}
}
