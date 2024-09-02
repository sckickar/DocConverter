using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using DocGen.Compression;
using DocGen.Pdf.Primitives;

namespace DocGen.Pdf.Compression;

internal class PdfZlibCompressor : IPdfCompressor
{
	private const int DefaultBufferSize = 32;

	private static string DefaultName = StreamFilters.FlateDecode.ToString();

	private PdfCompressionLevel m_level;

	public string Name => DefaultName;

	public CompressionType Type => CompressionType.Zlib;

	public Encoding Encoding => System.Text.Encoding.UTF8;

	public PdfCompressionLevel Level
	{
		get
		{
			return m_level;
		}
		set
		{
			if (m_level != value)
			{
				m_level = value;
			}
		}
	}

	public PdfZlibCompressor()
	{
		m_level = PdfCompressionLevel.Normal;
	}

	public PdfZlibCompressor(PdfCompressionLevel level)
		: this()
	{
		m_level = level;
	}

	public byte[] Compress(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte[] array = null;
		using MemoryStream inputStream = new MemoryStream(data);
		using Stream stream = Compress(inputStream);
		return PdfStream.StreamToBytes(stream);
	}

	public Stream Compress(Stream inputStream)
	{
		if (inputStream == null)
		{
			throw new ArgumentNullException("inputStream");
		}
		MemoryStream memoryStream = new MemoryStream();
		DocGen.Compression.CompressionLevel level = (DocGen.Compression.CompressionLevel)Level;
		CompressedStreamWriter compressedStreamWriter = new CompressedStreamWriter(memoryStream, level, bCloseStream: false);
		byte[] array = null;
		int num = 90000000;
		array = ((inputStream.Length <= num) ? new byte[inputStream.Length] : new byte[inputStream.Length / 4]);
		inputStream.Position = 0L;
		int length;
		while ((length = inputStream.Read(array, 0, array.Length)) > 0)
		{
			compressedStreamWriter.Write(array, 0, length, bCloseAfterWrite: false);
		}
		compressedStreamWriter.Close();
		return memoryStream;
	}

	public byte[] Compress(string data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte[] array = null;
		using MemoryStream inputStream = new MemoryStream(Encoding.GetBytes(data));
		using Stream stream = Compress(inputStream);
		return PdfStream.StreamToBytes(stream);
	}

	public byte[] Decompress(string data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		byte[] array = null;
		using MemoryStream inputStream = new MemoryStream(Encoding.GetBytes(data));
		using Stream stream = Decompress(inputStream);
		return PdfStream.StreamToBytes(stream);
	}

	public byte[] Decompress(byte[] data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.Length == 0 || data.Length == 1)
		{
			return data;
		}
		byte[] array = null;
		using MemoryStream inputStream = new MemoryStream(data);
		using Stream stream = Decompress(inputStream);
		return PdfStream.StreamToBytes(stream);
	}

	public Stream Decompress(Stream inputStream)
	{
		if (inputStream == null)
		{
			throw new ArgumentNullException("inputStream");
		}
		MemoryStream memoryStream = new MemoryStream();
		byte[] array = new byte[32];
		CompressedStreamReader compressedStreamReader = new CompressedStreamReader(inputStream);
		try
		{
			int count;
			while ((count = compressedStreamReader.Read(array, 0, array.Length)) > 0)
			{
				memoryStream.Write(array, 0, count);
			}
		}
		catch (Exception ex)
		{
			if (ex.Message == "Wrong block length.")
			{
				inputStream.Position = 0L;
				compressedStreamReader = new CompressedStreamReader(inputStream);
				array = new byte[1];
				memoryStream = new MemoryStream();
				try
				{
					int count;
					while ((count = compressedStreamReader.Read(array, 0, array.Length)) > 0)
					{
						memoryStream.Write(array, 0, count);
					}
				}
				catch
				{
				}
			}
			else
			{
				if (!(ex.Message == "Checksum check failed."))
				{
					throw;
				}
				try
				{
					inputStream.Position = 0L;
					inputStream.ReadByte();
					inputStream.ReadByte();
					using DeflateStream deflateStream = new DeflateStream(inputStream, CompressionMode.Decompress, leaveOpen: true);
					array = new byte[4096];
					memoryStream = new MemoryStream();
					while (true)
					{
						int num = deflateStream.Read(array, 0, 4096);
						if (num > 0)
						{
							memoryStream.Write(array, 0, num);
							continue;
						}
						break;
					}
				}
				catch
				{
				}
			}
		}
		return memoryStream;
	}
}
