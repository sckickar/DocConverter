using System;
using System.IO;

namespace DocGen.Compression.Zip;

public class NetCompressor : Stream
{
	private CompressedStreamWriter writer;

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

	public override long Length
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override long Position
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public NetCompressor(CompressionLevel compressionLevel, Stream outputStream)
	{
		writer = new CompressedStreamWriter(outputStream, bNoWrap: true, compressionLevel, bCloseStream: false);
	}

	public void Write(byte[] data, int size, bool close)
	{
		writer.Write(data, 0, size, close);
	}

	public override void Flush()
	{
		writer.Close();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		writer.Write(buffer, offset, count, bCloseAfterWrite: false);
	}
}
