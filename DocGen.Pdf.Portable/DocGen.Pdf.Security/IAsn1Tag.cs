namespace DocGen.Pdf.Security;

internal interface IAsn1Tag : IAsn1
{
	int TagNumber { get; }

	IAsn1 GetParser(int tagNumber, bool isExplicit);
}
