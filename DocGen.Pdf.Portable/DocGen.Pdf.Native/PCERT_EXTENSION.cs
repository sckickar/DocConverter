namespace DocGen.Pdf.Native;

internal struct PCERT_EXTENSION
{
	public nint pszObjId;

	public bool fCritical;

	public CryptographicApiStore Value;
}
