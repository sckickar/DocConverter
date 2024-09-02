using System;
using System.IO;

namespace SkiaSharp;

public class SKFrontBufferedStream : Stream
{
	public const int DefaultBufferSize = 4096;

	private readonly long totalBufferSize;

	private readonly long totalLength;

	private readonly bool disposeStream;

	private Stream underlyingStream;

	private long currentOffset;

	private long bufferedSoFar;

	private byte[] internalBuffer;

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public override long Length => totalLength;

	public override long Position
	{
		get
		{
			return currentOffset;
		}
		set
		{
			Seek(value, SeekOrigin.Begin);
		}
	}

	public SKFrontBufferedStream(Stream stream)
		: this(stream, 4096L, disposeUnderlyingStream: false)
	{
	}

	public SKFrontBufferedStream(Stream stream, long bufferSize)
		: this(stream, bufferSize, disposeUnderlyingStream: false)
	{
	}

	public SKFrontBufferedStream(Stream stream, bool disposeUnderlyingStream)
		: this(stream, 4096L, disposeUnderlyingStream)
	{
	}

	public SKFrontBufferedStream(Stream stream, long bufferSize, bool disposeUnderlyingStream)
	{
		underlyingStream = stream;
		totalBufferSize = bufferSize;
		totalLength = (stream.CanSeek ? stream.Length : (-1));
		disposeStream = disposeUnderlyingStream;
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		long num = currentOffset;
		if (internalBuffer == null && currentOffset < totalBufferSize)
		{
			internalBuffer = new byte[totalBufferSize];
		}
		if (currentOffset < bufferedSoFar)
		{
			int num2 = ReadFromBuffer(buffer, offset, count);
			count -= num2;
			offset += num2;
		}
		if (count > 0 && bufferedSoFar < totalBufferSize)
		{
			int num3 = BufferAndWriteTo(buffer, offset, count);
			count -= num3;
			offset += num3;
		}
		if (count > 0)
		{
			int num4 = ReadDirectlyFromStream(buffer, offset, count);
			count -= num4;
			offset += num4;
			if (num4 > 0)
			{
				internalBuffer = null;
			}
		}
		return (int)(currentOffset - num);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (currentOffset > totalBufferSize)
		{
			throw new InvalidOperationException("The position cannot be changed once the stream has moved past the buffer.");
		}
		long num = offset;
		switch (origin)
		{
		case SeekOrigin.Current:
			num = Position + offset;
			break;
		case SeekOrigin.End:
			if (Length == -1)
			{
				throw new InvalidOperationException("Can't seek from end as the underlying stream is not seekable.");
			}
			num = Length + offset;
			break;
		}
		if (num <= currentOffset)
		{
			currentOffset = num;
		}
		else
		{
			long num2 = num - currentOffset;
			currentOffset += Read(null, 0, (int)num2);
		}
		return Position;
	}

	public override void SetLength(long value)
	{
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
	}

	private int ReadFromBuffer(byte[] dst, int offset, int size)
	{
		int num = Math.Min(size, (int)(bufferedSoFar - currentOffset));
		if (dst != null && offset < dst.Length)
		{
			Buffer.BlockCopy(internalBuffer, (int)currentOffset, dst, offset, num);
		}
		currentOffset += num;
		return num;
	}

	private int BufferAndWriteTo(byte[] dst, int offset, int size)
	{
		int count = Math.Min(size, (int)(totalBufferSize - bufferedSoFar));
		int num = underlyingStream.Read(internalBuffer, (int)currentOffset, count);
		if (dst != null && offset < dst.Length)
		{
			Buffer.BlockCopy(internalBuffer, (int)currentOffset, dst, offset, num);
		}
		bufferedSoFar += num;
		currentOffset = bufferedSoFar;
		return num;
	}

	private int ReadDirectlyFromStream(byte[] dst, int offset, int size)
	{
		long num = 0L;
		num = ((dst != null) ? underlyingStream.Read(dst, offset, size) : underlyingStream.Seek(size, SeekOrigin.Current));
		currentOffset += num;
		return (int)num;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		internalBuffer = null;
		if (disposeStream && underlyingStream != null)
		{
			underlyingStream.Dispose();
		}
		underlyingStream = null;
	}
}
