using System.IO;

namespace DocGen.Compression;

internal static class Helper
{
	internal static void Close(this Stream stream)
	{
		stream.Flush();
		stream.Dispose();
	}
}
