using System;
using System.Text;

namespace DocGen.Pdf.Security;

internal class LargeDecimal
{
	private readonly Number m_bigDigit;

	private readonly int m_scale;

	public int Scale => m_scale;

	public LargeDecimal(Number digit, int value)
	{
		if (value < 0)
		{
			throw new ArgumentException("value may not be negative");
		}
		m_bigDigit = digit;
		m_scale = value;
	}

	private void CheckScale(LargeDecimal value)
	{
		if (m_scale != value.m_scale)
		{
			throw new ArgumentException("same vlaue");
		}
	}

	public LargeDecimal AdjustScale(int newScale)
	{
		if (newScale < 0)
		{
			throw new ArgumentException("Not be negative");
		}
		if (newScale == m_scale)
		{
			return this;
		}
		return new LargeDecimal(m_bigDigit.ShiftLeft(newScale - m_scale), newScale);
	}

	public LargeDecimal Add(LargeDecimal value)
	{
		CheckScale(value);
		return new LargeDecimal(m_bigDigit.Add(value.m_bigDigit), m_scale);
	}

	public LargeDecimal Negate()
	{
		return new LargeDecimal(m_bigDigit.Negate(), m_scale);
	}

	public LargeDecimal Subtract(LargeDecimal value)
	{
		return Add(value.Negate());
	}

	public LargeDecimal Subtract(Number value)
	{
		return new LargeDecimal(m_bigDigit.Subtract(value.ShiftLeft(m_scale)), m_scale);
	}

	public int CompareTo(Number val)
	{
		return m_bigDigit.CompareTo(val.ShiftLeft(m_scale));
	}

	public Number Floor()
	{
		return m_bigDigit.ShiftRight(m_scale);
	}

	public Number Round()
	{
		LargeDecimal largeDecimal = new LargeDecimal(Number.One, 1);
		return Add(largeDecimal.AdjustScale(m_scale)).Floor();
	}

	public override string ToString()
	{
		if (m_scale == 0)
		{
			return m_bigDigit.ToString();
		}
		Number number = Floor();
		Number number2 = m_bigDigit.Subtract(number.ShiftLeft(m_scale));
		if (m_bigDigit.SignValue < 0)
		{
			number2 = Number.One.ShiftLeft(m_scale).Subtract(number2);
		}
		if (number.SignValue == -1 && !number2.Equals(Number.Zero))
		{
			number = number.Add(Number.One);
		}
		string value = number.ToString();
		char[] array = new char[m_scale];
		string text = number2.ToString(2);
		int length = text.Length;
		int num = m_scale - length;
		for (int i = 0; i < num; i++)
		{
			array[i] = '0';
		}
		for (int j = 0; j < length; j++)
		{
			array[num + j] = text[j];
		}
		string value2 = new string(array);
		StringBuilder stringBuilder = new StringBuilder(value);
		stringBuilder.Append(".");
		stringBuilder.Append(value2);
		return stringBuilder.ToString();
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is LargeDecimal largeDecimal))
		{
			return false;
		}
		if (m_bigDigit.Equals(largeDecimal.m_bigDigit))
		{
			return m_scale == largeDecimal.m_scale;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_bigDigit.GetHashCode() ^ m_scale;
	}
}
