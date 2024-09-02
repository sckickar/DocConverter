using System;

namespace DocGen.Pdf.Security;

internal class RevocationPointList : Asn1Encode
{
	private readonly Asn1Sequence m_sequence;

	internal RevocationPointList GetCrlPointList(object obj)
	{
		if (obj is RevocationPointList || obj == null)
		{
			return (RevocationPointList)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new RevocationPointList((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	internal RevocationPointList()
	{
	}

	private RevocationPointList(Asn1Sequence sequence)
	{
		m_sequence = sequence;
	}

	internal RevocationDistribution[] GetDistributionPoints()
	{
		RevocationDistribution[] array = new RevocationDistribution[m_sequence.Count];
		RevocationDistribution revocationDistribution = new RevocationDistribution();
		for (int i = 0; i != m_sequence.Count; i++)
		{
			array[i] = revocationDistribution.GetCrlDistribution(m_sequence[i]);
		}
		return array;
	}

	public override Asn1 GetAsn1()
	{
		return m_sequence;
	}
}
