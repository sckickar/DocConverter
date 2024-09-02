using System.IO;

namespace DocGen.Pdf.Security;

internal interface IAsn1Octet : IAsn1
{
	Stream GetOctetStream();
}
