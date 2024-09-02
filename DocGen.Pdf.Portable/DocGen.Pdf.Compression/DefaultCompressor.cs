using System;
using System.IO;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Compression;

internal class DefaultCompressor : IPdfCompressor
{
	public string Name => string.Empty;

	public CompressionType Type => CompressionType.None;

	public byte[] Compress(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return data;
	}

	public Stream Compress(Stream inputStream)
	{
		if (inputStream == null)
		{
			throw new ArgumentNullException("inputStream");
		}
		return inputStream;
	}

	public byte[] Compress(string data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return PdfString.StringToByte(data);
	}

	public byte[] Decompress(string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return PdfString.StringToByte(value);
	}

	public byte[] Decompress(byte[] value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return value;
	}

	public Stream Decompress(Stream inputStream)
	{
		if (inputStream == null)
		{
			throw new ArgumentNullException("inputStream");
		}
		return inputStream;
	}
}
