using System;

namespace DocGen.Pdf.Security;

internal class ECPrivateKeyParam : Asn1Encode
{
	private readonly Asn1Sequence sequence;

	public ECPrivateKeyParam(Asn1Sequence sequence)
	{
		if (sequence == null)
		{
			throw new ArgumentNullException("sequence");
		}
		this.sequence = sequence;
	}

	public Number GetKey()
	{
		Asn1Octet asn1Octet = (Asn1Octet)sequence[1];
		return new Number(1, asn1Octet.GetOctets());
	}

	public override Asn1 GetAsn1()
	{
		return sequence;
	}
}
