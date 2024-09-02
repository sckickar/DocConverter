namespace DocGen.Compression.Zip;

internal sealed class SecurityConstants
{
	internal const int AesPassVerLength = 2;

	internal const int AesMacLength = 10;

	internal const int ZipCryptoPassLength = 12;

	internal const int ZipCryptoPassVerPos = 11;

	internal static readonly byte[] AesEncryptionHeader = new byte[8] { 1, 153, 7, 0, 2, 0, 65, 69 };

	internal const int PKZipEncryptionHeader = 23;

	internal const CompressionMethod AES = (CompressionMethod)99;

	internal const int Rfc2898BlockSize = 20;

	internal const int PassIterations = 1000;

	internal const int AesBlockSize = 16;
}
