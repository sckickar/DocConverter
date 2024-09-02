using System;
using System.IO;

namespace DocGen.Compression.Zip;

public class ZippedContentStream : Stream
{
	private MemoryStream m_stream = new MemoryStream();

	private Stream m_deflateStream;

	private uint m_uiCrc32;

	private long m_lSize;

	public override bool CanRead => m_deflateStream.CanRead;

	public override bool CanSeek => m_deflateStream.CanSeek;

	public override bool CanWrite => m_deflateStream.CanWrite;

	public override long Length => m_deflateStream.Length;

	public override long Position
	{
		get
		{
			return m_deflateStream.Position;
		}
		set
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public Stream ZippedContent
	{
		get
		{
			m_deflateStream.Close();
			return m_stream;
		}
	}

	[CLSCompliant(false)]
	public uint Crc32 => m_uiCrc32;

	public long UnzippedSize => m_lSize;

	private ZippedContentStream()
	{
		m_deflateStream = CreateDeflateStream(m_stream);
	}

	public ZippedContentStream(ZipArchive.CompressorCreator createCompressor)
	{
		m_deflateStream = createCompressor(m_stream);
	}

	private Stream CreateDeflateStream(Stream stream)
	{
		return new NetCompressor(CompressionLevel.Best, stream);
	}

	public override void Flush()
	{
		m_deflateStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public override void SetLength(long value)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		m_deflateStream.Write(buffer, offset, count);
		m_uiCrc32 = ZipCrc32.ComputeCrc(buffer, offset, count, m_uiCrc32);
		m_lSize += count;
	}
}
