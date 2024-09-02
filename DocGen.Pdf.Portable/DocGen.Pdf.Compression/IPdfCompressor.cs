using System.IO;

namespace DocGen.Pdf.Compression;

internal interface IPdfCompressor
{
	CompressionType Type { get; }

	string Name { get; }

	byte[] Compress(byte[] data);

	byte[] Compress(string data);

	Stream Compress(Stream inputStream);

	byte[] Decompress(string value);

	byte[] Decompress(byte[] value);

	Stream Decompress(Stream inputStream);
}
