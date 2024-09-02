using System;

namespace DocGen.Pdf.Security;

internal class RevocationDistribution : Asn1Encode
{
	private RevocationDistributionType m_distributionPoint;

	private RevocationName m_issuer;

	internal RevocationDistributionType DistributionPointName => m_distributionPoint;

	private RevocationDistribution(Asn1Sequence sequence)
	{
		for (int i = 0; i != sequence.Count; i++)
		{
			Asn1Tag tag = Asn1Tag.GetTag(sequence[i]);
			switch (tag.TagNumber)
			{
			case 0:
			{
				RevocationDistributionType revocationDistributionType = new RevocationDistributionType();
				m_distributionPoint = revocationDistributionType.GetDistributionType(tag, isExplicit: true);
				break;
			}
			case 2:
			{
				RevocationName revocationName = new RevocationName();
				m_issuer = revocationName.GetCrlName(tag, isExplicit: false);
				break;
			}
			}
		}
	}

	internal RevocationDistribution()
	{
	}

	public RevocationDistribution GetCrlDistribution(object obj)
	{
		if (obj == null || obj is RevocationDistribution)
		{
			return (RevocationDistribution)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new RevocationDistribution((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in CRL distribution point");
	}

	public override Asn1 GetAsn1()
	{
		Asn1EncodeCollection asn1EncodeCollection = new Asn1EncodeCollection();
		if (m_distributionPoint != null)
		{
			asn1EncodeCollection.Add(new DerTag(0, m_distributionPoint));
		}
		if (m_issuer != null)
		{
			asn1EncodeCollection.Add(new DerTag(isExplicit: false, 2, m_issuer));
		}
		return new DerSequence(asn1EncodeCollection);
	}
}
