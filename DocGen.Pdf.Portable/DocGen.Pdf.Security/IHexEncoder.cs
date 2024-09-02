using System.IO;

namespace DocGen.Pdf.Security;

internal interface IHexEncoder
{
	int Encode(byte[] data, int offset, int length, Stream outputStream);

	int Decode(byte[] data, int offset, int length, Stream outputStream);

	int DecodeString(string data, Stream outputStream);
}
