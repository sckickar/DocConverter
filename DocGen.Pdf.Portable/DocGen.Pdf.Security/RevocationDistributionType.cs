using System;

namespace DocGen.Pdf.Security;

internal class RevocationDistributionType : Asn1Encode
{
	private Asn1Encode m_name;

	private int m_type;

	public const int FullName = 0;

	public const int NameRelativeToCrlIssuer = 1;

	internal int PointType => m_type;

	internal Asn1Encode Name => m_name;

	internal RevocationDistributionType GetDistributionType(Asn1Tag tag, bool isExplicit)
	{
		return GetDistributionType(Asn1Tag.GetTag(tag, isExplicit: true));
	}

	internal RevocationDistributionType GetDistributionType(object obj)
	{
		if (obj == null || obj is RevocationDistributionType)
		{
			return (RevocationDistributionType)obj;
		}
		if (obj is Asn1Tag)
		{
			return new RevocationDistributionType((Asn1Tag)obj);
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	internal RevocationDistributionType(Asn1Tag tag)
	{
		m_type = tag.TagNumber;
		if (m_type == 0)
		{
			RevocationName revocationName = new RevocationName();
			m_name = revocationName.GetCrlName(tag, isExplicit: false);
		}
		else
		{
			m_name = Asn1Set.GetAsn1Set(tag, isExplicit: false);
		}
	}

	internal RevocationDistributionType()
	{
	}

	public override Asn1 GetAsn1()
	{
		return new DerTag(isExplicit: false, m_type, m_name);
	}
}
