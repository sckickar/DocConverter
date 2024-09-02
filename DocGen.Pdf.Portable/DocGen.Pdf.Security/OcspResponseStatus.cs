namespace DocGen.Pdf.Security;

internal class OcspResponseStatus : DerCatalogue
{
	internal const int Successful = 0;

	internal const int MalformedRequest = 1;

	internal const int InternalError = 2;

	internal const int TryLater = 3;

	internal const int SignatureRequired = 5;

	internal const int Unauthorized = 6;

	internal OcspResponseStatus(DerCatalogue value)
		: base(value.Value.IntValue)
	{
	}
}
