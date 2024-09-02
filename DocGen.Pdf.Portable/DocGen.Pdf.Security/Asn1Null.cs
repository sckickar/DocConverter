namespace DocGen.Pdf.Security;

internal abstract class Asn1Null : Asn1
{
	internal Asn1Null()
		: base(Asn1UniversalTags.Null)
	{
	}

	internal static Asn1Null GetInstance()
	{
		return new DerNull(0);
	}

	private byte[] ToArray()
	{
		return new byte[0];
	}

	internal byte[] AsnEncode()
	{
		return Asn1Encode(ToArray());
	}

	public override string ToString()
	{
		return "NULL";
	}

	internal override void Encode(DerStream derOut)
	{
		derOut.WriteEncoded(5, ToArray());
	}
}
