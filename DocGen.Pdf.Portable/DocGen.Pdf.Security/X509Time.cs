using System;

namespace DocGen.Pdf.Security;

internal class X509Time : Asn1Encode
{
	internal Asn1 m_time;

	internal X509Time(Asn1 time)
	{
		m_time = time;
	}

	internal static X509Time GetTime(object obj)
	{
		if (obj == null || obj is X509Time)
		{
			return (X509Time)obj;
		}
		if (obj is DerUtcTime)
		{
			return new X509Time((DerUtcTime)obj);
		}
		if (obj is GeneralizedTime)
		{
			return new X509Time((GeneralizedTime)obj);
		}
		throw new ArgumentException("Invalid entry");
	}

	internal string GetTime()
	{
		if (m_time is DerUtcTime)
		{
			return ((DerUtcTime)m_time).AdjustedTimeString;
		}
		if (m_time is GeneralizedTime)
		{
			return ((GeneralizedTime)m_time).TimeString;
		}
		return null;
	}

	internal DateTime ToDateTime()
	{
		try
		{
			if (m_time is DerUtcTime)
			{
				return ((DerUtcTime)m_time).ToAdjustedDateTime();
			}
			if (m_time is GeneralizedTime)
			{
				return ((GeneralizedTime)m_time).ToDateTime();
			}
			return DateTime.Now;
		}
		catch (FormatException)
		{
			throw new InvalidOperationException("Invalid entry");
		}
	}

	public override Asn1 GetAsn1()
	{
		return m_time;
	}

	public override string ToString()
	{
		return GetTime();
	}
}
