namespace DocGen.Pdf.Native;

internal struct CertificateContext
{
	public int CertEncodingType;

	public nint EncodedCertificate;

	public int EncodedCertificateSize;

	public nint CertificateInformation;

	public nint CertificateStore;
}
