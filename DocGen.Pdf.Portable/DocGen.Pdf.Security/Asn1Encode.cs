using System.IO;

namespace DocGen.Pdf.Security;

internal abstract class Asn1Encode : IAsn1
{
	public abstract Asn1 GetAsn1();

	internal byte[] GetEncoded()
	{
		MemoryStream memoryStream = new MemoryStream();
		new Asn1DerStream(memoryStream).WriteObject(this);
		return memoryStream.ToArray();
	}

	internal byte[] GetEncoded(string encoding)
	{
		if (encoding.Equals("DER"))
		{
			MemoryStream memoryStream = new MemoryStream();
			new DerStream(memoryStream).WriteObject(this);
			return memoryStream.ToArray();
		}
		return GetEncoded();
	}

	public byte[] GetDerEncoded()
	{
		try
		{
			return GetEncoded("DER");
		}
		catch (IOException)
		{
			return null;
		}
	}

	public sealed override int GetHashCode()
	{
		return GetAsn1().GetAsn1Hash();
	}

	public sealed override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is IAsn1 asn))
		{
			return false;
		}
		Asn1 asn2 = GetAsn1();
		Asn1 asn3 = asn.GetAsn1();
		if (asn2 != asn3)
		{
			return asn2.Asn1Equals(asn3);
		}
		return true;
	}
}
