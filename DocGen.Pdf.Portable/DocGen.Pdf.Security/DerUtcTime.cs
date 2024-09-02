using System;
using System.Globalization;
using System.Text;

namespace DocGen.Pdf.Security;

internal class DerUtcTime : Asn1
{
	private string m_time;

	internal string AdjustedTimeString
	{
		get
		{
			string timeString = TimeString;
			return ((timeString[0] < '5') ? "20" : "19") + timeString;
		}
	}

	internal string TimeString
	{
		get
		{
			if (m_time.IndexOf('-') < 0 && m_time.IndexOf('+') < 0)
			{
				if (m_time.Length == 11)
				{
					return m_time.Substring(0, 10) + "00GMT+00:00";
				}
				return m_time.Substring(0, 12) + "GMT+00:00";
			}
			int num = m_time.IndexOf('-');
			if (num < 0)
			{
				num = m_time.IndexOf('+');
			}
			string text = m_time;
			if (num == m_time.Length - 3)
			{
				text += "00";
			}
			if (num == 10)
			{
				return text.Substring(0, 10) + "00GMT" + text.Substring(10, 3) + ":" + text.Substring(13, 2);
			}
			return text.Substring(0, 12) + "GMT" + text.Substring(12, 3) + ":" + text.Substring(15, 2);
		}
	}

	internal static DerUtcTime GetUtcTime(object obj)
	{
		if (obj == null || obj is DerUtcTime)
		{
			return (DerUtcTime)obj;
		}
		throw new ArgumentException("Invalid entry");
	}

	internal static DerUtcTime GetInstance(Asn1Tag tag, bool isExplicit)
	{
		Asn1 @object = tag.GetObject();
		if (isExplicit || @object is DerUtcTime)
		{
			return GetUtcTime(@object);
		}
		return new DerUtcTime(((Asn1Octet)@object).GetOctets());
	}

	internal DerUtcTime(string time)
	{
		if (time == null)
		{
			throw new ArgumentNullException("time");
		}
		m_time = time;
		try
		{
			ToDateTime();
		}
		catch (FormatException ex)
		{
			throw new ArgumentException("Invalid date format : " + ex.Message);
		}
	}

	internal DerUtcTime(DateTime time)
	{
		m_time = time.ToString("yyMMddHHmmss") + "Z";
	}

	internal DerUtcTime(byte[] bytes)
	{
		m_time = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
	}

	public DateTime ToDateTime()
	{
		return DateTime.ParseExact(TimeString, "yyMMddHHmmss'GMT'zzz", DateTimeFormatInfo.InvariantInfo).ToUniversalTime();
	}

	internal DateTime ToAdjustedDateTime()
	{
		return DateTime.ParseExact(AdjustedTimeString, "yyyyMMddHHmmss'GMT'zzz", DateTimeFormatInfo.InvariantInfo).ToUniversalTime();
	}

	private byte[] GetBytes()
	{
		return Encoding.UTF8.GetBytes(m_time);
	}

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(23, GetBytes());
	}

	protected override bool IsEquals(Asn1 asn1)
	{
		if (!(asn1 is DerUtcTime derUtcTime))
		{
			return false;
		}
		return m_time.Equals(derUtcTime.m_time);
	}

	public override int GetHashCode()
	{
		return m_time.GetHashCode();
	}

	public override string ToString()
	{
		return m_time;
	}
}
