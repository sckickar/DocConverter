using System;

namespace DocGen.Pdf.Security;

internal class SecureRandomAlgorithm : Random
{
	private static readonly IAlgorithmGenerator m_sha1Generator = new DigestAlgorithmGenerator(new SHA1MessageDigest());

	private static readonly IAlgorithmGenerator m_sha256Generator = new DigestAlgorithmGenerator(new SHA256MessageDigest());

	private SecureRandomAlgorithm[] m_master = new SecureRandomAlgorithm[1];

	private double DoubleScale = Math.Pow(2.0, 64.0);

	protected IAlgorithmGenerator m_generator;

	private SecureRandomAlgorithm Algorithm
	{
		get
		{
			if (m_master[0] == null)
			{
				IAlgorithmGenerator sha256Generator = m_sha256Generator;
				sha256Generator = new RWAlgorithmGenerator(sha256Generator, 32);
				SecureRandomAlgorithm secureRandomAlgorithm = (m_master[0] = new SecureRandomAlgorithm(sha256Generator));
				secureRandomAlgorithm.SetBytes(DateTime.Now.Ticks);
				secureRandomAlgorithm.GenerateBytes(1 + secureRandomAlgorithm.Next(32));
			}
			return m_master[0];
		}
	}

	internal byte[] GetBytes(int length)
	{
		return Algorithm.GenerateBytes(length);
	}

	internal SecureRandomAlgorithm()
		: this(m_sha1Generator)
	{
		SetBytes(GetBytes(8));
	}

	internal SecureRandomAlgorithm(IAlgorithmGenerator generator)
	{
		m_generator = generator;
	}

	public virtual byte[] GenerateBytes(int length)
	{
		SetBytes(DateTime.Now.Ticks);
		byte[] array = new byte[length];
		NextBytes(array);
		return array;
	}

	public virtual void SetBytes(byte[] bytes)
	{
		m_generator.AddMaterial(bytes);
	}

	public virtual void SetBytes(long value)
	{
		m_generator.AddMaterial(value);
	}

	public override int Next()
	{
		int num;
		do
		{
			num = NextInt() & 0x7FFFFFFF;
		}
		while (num == int.MaxValue);
		return num;
	}

	public override int Next(int maxValue)
	{
		if (maxValue < 2)
		{
			if (maxValue < 0)
			{
				throw new ArgumentOutOfRangeException("maxValue");
			}
			return 0;
		}
		if ((maxValue & -maxValue) == maxValue)
		{
			int num = NextInt() & 0x7FFFFFFF;
			return (int)((long)maxValue * (long)num >> 31);
		}
		int num2;
		int num3;
		do
		{
			num2 = NextInt() & 0x7FFFFFFF;
			num3 = num2 % maxValue;
		}
		while (num2 - num3 + (maxValue - 1) < 0);
		return num3;
	}

	public override int Next(int minValue, int maxValue)
	{
		if (maxValue <= minValue)
		{
			if (maxValue == minValue)
			{
				return minValue;
			}
			throw new ArgumentException("Invalid max value");
		}
		int num = maxValue - minValue;
		if (num > 0)
		{
			return minValue + Next(num);
		}
		int num2;
		do
		{
			num2 = NextInt();
		}
		while (num2 < minValue || num2 >= maxValue);
		return num2;
	}

	public override void NextBytes(byte[] buffer)
	{
		m_generator.FillNextBytes(buffer);
	}

	public virtual void NextBytes(byte[] buffer, int start, int length)
	{
		m_generator.FillNextBytes(buffer, start, length);
	}

	public override double NextDouble()
	{
		return Convert.ToDouble((ulong)NextLong()) / DoubleScale;
	}

	public virtual int NextInt()
	{
		byte[] array = new byte[4];
		NextBytes(array);
		int num = 0;
		for (int i = 0; i < 4; i++)
		{
			num = (num << 8) + (array[i] & 0xFF);
		}
		return num;
	}

	public virtual long NextLong()
	{
		return (long)(((ulong)(uint)NextInt() << 32) | (uint)NextInt());
	}
}
