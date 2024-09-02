namespace DocGen.Pdf.Security;

internal abstract class X509ExtensionBase : IX509Extension
{
	protected abstract X509Extensions GetX509Extensions();

	public virtual Asn1Octet GetExtension(DerObjectID oid)
	{
		X509Extensions x509Extensions = GetX509Extensions();
		if (x509Extensions != null)
		{
			X509Extension extension = x509Extensions.GetExtension(oid);
			if (extension != null)
			{
				return extension.Value;
			}
		}
		return null;
	}
}
