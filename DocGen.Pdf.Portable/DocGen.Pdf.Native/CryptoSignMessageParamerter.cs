namespace DocGen.Pdf.Native;

internal struct CryptoSignMessageParamerter
{
	public uint SizeInBytes;

	public uint EncodingType;

	public nint SigningCertPointer;

	public CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;

	public nint HashAuxInfo;

	public uint MessageCertificateCount;

	public nint MessageCertificate;

	public uint MessageCrlCount;

	public nint MessageCrl;

	public uint AuthenticatedAttributeCount;

	public nint AuthenticatedAttribute;

	public uint UnauthenticatedAttributeCount;

	public nint UnauthenticatedAttribute;

	public uint CrytographicSilentFlag;

	public uint InnerContentType;

	public CRYPT_ALGORITHM_IDENTIFIER HashEncryptionAlgorithm;

	public nint HashEncryptionAux;
}
