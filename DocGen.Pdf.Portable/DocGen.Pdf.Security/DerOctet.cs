namespace DocGen.Pdf.Security;

internal class DerOctet : Asn1Octet
{
	internal DerOctet(byte[] bytes)
		: base(bytes)
	{
	}

	internal DerOctet(Asn1Encode asn1)
		: base(asn1)
	{
	}

	internal override void Encode(DerStream stream)
	{
		stream.WriteEncoded(4, m_value);
	}
}
