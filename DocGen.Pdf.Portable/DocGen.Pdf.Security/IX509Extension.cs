namespace DocGen.Pdf.Security;

internal interface IX509Extension
{
	Asn1Octet GetExtension(DerObjectID id);
}
