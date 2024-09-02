namespace DocGen.Pdf.Security;

public interface IPdfExternalSigner
{
	string HashAlgorithm { get; }

	byte[] Sign(byte[] message, out byte[] timeStampResponse);
}
