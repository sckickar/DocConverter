using System;

namespace DocGen.Pdf.Security;

internal class OcspStatus : Asn1Encode
{
	private int m_tagNumber;

	private Asn1Encode m_value;

	internal int TagNumber => m_tagNumber;

	internal OcspStatus(Asn1Tag choice)
	{
		m_tagNumber = choice.TagNumber;
		int tagNumber = choice.TagNumber;
		if (tagNumber == 0 || tagNumber == 2)
		{
			m_value = new DerNull(0);
		}
	}

	internal OcspStatus()
	{
	}

	internal OcspStatus GetStatus(object obj)
	{
		if (obj == null || obj is OcspStatus)
		{
			return (OcspStatus)obj;
		}
		if (obj is Asn1Tag)
		{
			return new OcspStatus((Asn1Tag)obj);
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	public override Asn1 GetAsn1()
	{
		return new DerTag(isExplicit: false, m_tagNumber, m_value);
	}
}
