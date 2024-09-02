using System;
using System.IO;
using System.Text;

namespace DocGen.Pdf.Security;

internal sealed class PdfHexEncoder
{
	private static readonly IHexEncoder encoder = new HexStringEncoder();

	public static byte[] Decode(string data)
	{
		MemoryStream memoryStream = new MemoryStream((data.Length + 1) / 2);
		encoder.DecodeString(data, memoryStream);
		return memoryStream.ToArray();
	}

	public static string DecodeString(string data)
	{
		byte[] array = Convert.FromBase64String(data);
		return Encoding.UTF8.GetString(array, 0, array.Length);
	}
}
