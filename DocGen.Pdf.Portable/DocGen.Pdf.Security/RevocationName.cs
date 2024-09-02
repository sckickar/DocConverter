using System;

namespace DocGen.Pdf.Security;

internal class RevocationName : Asn1Encode
{
	private OcspTag[] m_names;

	internal OcspTag[] Names => (OcspTag[])m_names.Clone();

	private RevocationName(Asn1Sequence sequence)
	{
		m_names = new OcspTag[sequence.Count];
		for (int i = 0; i != sequence.Count; i++)
		{
			OcspTag ocspTag = new OcspTag();
			m_names[i] = ocspTag.GetOcspName(sequence[i]);
		}
	}

	internal RevocationName()
	{
	}

	public RevocationName GetCrlName(object obj)
	{
		if (obj == null || obj is RevocationName)
		{
			return (RevocationName)obj;
		}
		if (obj is Asn1Sequence)
		{
			return new RevocationName((Asn1Sequence)obj);
		}
		throw new ArgumentException("Invalid entry in sequence");
	}

	public RevocationName GetCrlName(Asn1Tag tag, bool isExplicit)
	{
		return GetCrlName(Asn1Sequence.GetSequence(tag, isExplicit));
	}

	public override Asn1 GetAsn1()
	{
		Asn1Encode[] names = m_names;
		return new DerSequence(names);
	}
}
