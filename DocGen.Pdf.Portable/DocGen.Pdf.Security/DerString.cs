namespace DocGen.Pdf.Security;

internal abstract class DerString : Asn1, IAsn1String
{
	public abstract string GetString();

	public override string ToString()
	{
		return GetString();
	}

	public override int GetHashCode()
	{
		return GetString().GetHashCode();
	}
}
