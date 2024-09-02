using System;
using System.Globalization;
using System.Text;

namespace DocGen.Pdf.Security;

internal class GeneralizedTime : Asn1
{
	internal const string GMT = "GMT";

	internal const char TZ = 'Z';

	internal const string Day = "dd";

	internal const string Month = "MM";

	internal const string Year = "yyyy";

	internal const string Hours = "HH";

	internal const string Minutes = "mm";

	internal const string Seconds = "ss";

	private readonly string m_time;

	internal string TimeString => m_time;

	internal GeneralizedTime(byte[] bytes)
	{
		m_time = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
	}

	internal DateTime ToDateTime()
	{
		string time = m_time;
		string format;
		if (time.EndsWith("Z"))
		{
			if (m_time.IndexOf('.') == 14)
			{
				int count = time.Length - time.IndexOf('.') - 2;
				format = "yyyyMMddHHmmss." + FormatString(count) + "\\Z";
			}
			else
			{
				format = "yyyyMMddHHmmss\\Z";
			}
		}
		else if (m_time.IndexOf('.') == 14)
		{
			int count2 = time.Length - 1 - time.IndexOf('.');
			format = "yyyyMMddHHmmss." + FormatString(count2);
		}
		else
		{
			format = "yyyyMMddHHmmss";
		}
		DateTime result = DateTime.ParseExact(time, format, DateTimeFormatInfo.InvariantInfo);
		if (0 == 0)
		{
			return result;
		}
		return result.ToUniversalTime();
	}

	private string FormatString(int count)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < count; i++)
		{
			stringBuilder.Append('f');
		}
		return stringBuilder.ToString();
	}

	internal override void Encode(DerStream dStream)
	{
		dStream.WriteEncoded(24, Encoding.UTF8.GetBytes(m_time));
	}

	protected override bool IsEquals(Asn1 asn1Object)
	{
		throw new NotImplementedException();
	}

	public override int GetHashCode()
	{
		return m_time.GetHashCode();
	}

	internal static GeneralizedTime GetGeneralizedTime(object obj)
	{
		if (obj == null || obj is GeneralizedTime)
		{
			return (GeneralizedTime)obj;
		}
		return null;
	}

	internal static GeneralizedTime GetGeneralizedTime(Asn1Tag tag, bool isExplicit)
	{
		Asn1 @object = tag.GetObject();
		if (isExplicit || @object is GeneralizedTime)
		{
			return GetGeneralizedTime(@object);
		}
		return new GeneralizedTime(((Asn1Octet)@object).GetOctets());
	}
}
