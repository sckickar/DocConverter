using System;
using System.IO;

namespace DocGen.Pdf;

internal class JPXRandomAccessStream
{
	private Stream is_Renamed;

	private int maxsize;

	private int inc;

	private byte[] buf;

	private int len;

	private int pos;

	private bool complete;

	internal virtual int Pos => pos;

	internal JPXRandomAccessStream()
	{
	}

	internal JPXRandomAccessStream(Stream is_Renamed, int size, int inc, int maxsize)
	{
		if (size < 0 || inc <= 0 || maxsize <= 0 || is_Renamed == null)
		{
			throw new ArgumentException();
		}
		this.is_Renamed = is_Renamed;
		if (size < int.MaxValue)
		{
			size++;
		}
		buf = new byte[size];
		this.inc = inc;
		if (maxsize < int.MaxValue)
		{
			maxsize++;
		}
		this.maxsize = maxsize;
		pos = 0;
		len = 0;
		complete = false;
	}

	private void growBuffer()
	{
		int num = inc;
		if (buf.Length + num > maxsize)
		{
			num = maxsize - buf.Length;
		}
		if (num <= 0)
		{
			throw new IOException("Reached maximum cache size (" + maxsize + ")");
		}
		byte[] destinationArray;
		try
		{
			destinationArray = new byte[buf.Length + inc];
		}
		catch (OutOfMemoryException)
		{
			throw new IOException("Out of memory to cache input data");
		}
		Array.Copy(buf, 0, destinationArray, 0, len);
		buf = destinationArray;
	}

	private void readInput()
	{
		if (complete)
		{
			throw new ArgumentException("Already reached EOF");
		}
		int num = (int)(is_Renamed.Length - is_Renamed.Position);
		if (num == 0)
		{
			num = 1;
		}
		while (len + num > buf.Length)
		{
			growBuffer();
		}
		int num2;
		do
		{
			num2 = is_Renamed.Read(buf, len, num);
			if (num2 > 0)
			{
				len += num2;
				num -= num2;
			}
		}
		while (num > 0 && num2 > 0);
		if (num2 <= 0)
		{
			complete = true;
			is_Renamed = null;
		}
	}

	internal virtual void close()
	{
		buf = null;
		if (!complete)
		{
			is_Renamed = null;
		}
	}

	internal virtual void seek(int off)
	{
		if (complete && off > len)
		{
			throw new EndOfStreamException();
		}
		pos = off;
	}

	internal virtual int length()
	{
		while (!complete)
		{
			readInput();
		}
		return len;
	}

	internal virtual byte readByte()
	{
		return read();
	}

	internal virtual byte read()
	{
		if (pos < len)
		{
			return buf[pos++];
		}
		while (!complete && pos >= len)
		{
			readInput();
		}
		if (pos != len)
		{
			_ = pos;
			_ = len;
		}
		return buf[pos++];
	}

	internal virtual void readFully(byte[] b, int off, int n)
	{
		if (pos + n <= len)
		{
			Array.Copy(buf, pos, b, off, n);
			pos += n;
			return;
		}
		while (!complete && pos + n > len)
		{
			readInput();
		}
		if (pos + n > len)
		{
			throw new EndOfStreamException();
		}
		Array.Copy(buf, pos, b, off, n);
		pos += n;
	}

	internal virtual byte readUnsignedByte()
	{
		if (pos < len)
		{
			return buf[pos++];
		}
		return read();
	}

	internal virtual short readShort()
	{
		if (pos + 1 < len)
		{
			return (short)((buf[pos++] << 8) | (0xFF & buf[pos++]));
		}
		return (short)((read() << 8) | read());
	}

	internal virtual int readUnsignedShort()
	{
		if (pos + 1 < len)
		{
			return ((0xFF & buf[pos++]) << 8) | (0xFF & buf[pos++]);
		}
		return (read() << 8) | read();
	}

	internal virtual int readInt()
	{
		if (pos + 3 < len)
		{
			return (buf[pos++] << 24) | ((0xFF & buf[pos++]) << 16) | ((0xFF & buf[pos++]) << 8) | (0xFF & buf[pos++]);
		}
		return (read() << 24) | (read() << 16) | (read() << 8) | read();
	}

	internal virtual long readUnsignedInt()
	{
		if (pos + 3 < len)
		{
			return -1L & (long)((buf[pos++] << 24) | ((0xFF & buf[pos++]) << 16) | ((0xFF & buf[pos++]) << 8) | (0xFF & buf[pos++]));
		}
		return -1L & (long)((read() << 24) | (read() << 16) | (read() << 8) | read());
	}

	internal virtual long readLong()
	{
		if (pos + 7 < len)
		{
			return (long)(((ulong)buf[pos++] << 56) | ((ulong)buf[pos++] << 48) | ((ulong)buf[pos++] << 40) | ((ulong)buf[pos++] << 32) | ((ulong)buf[pos++] << 24) | ((ulong)buf[pos++] << 16) | ((ulong)buf[pos++] << 8) | buf[pos++]);
		}
		return (long)(((ulong)read() << 56) | ((ulong)read() << 48) | ((ulong)read() << 40) | ((ulong)read() << 32) | ((ulong)read() << 24) | ((ulong)read() << 16) | ((ulong)read() << 8) | read());
	}

	internal virtual float readFloat()
	{
		int value = ((pos + 3 >= len) ? ((read() << 24) | (read() << 16) | (read() << 8) | read()) : ((buf[pos++] << 24) | ((0xFF & buf[pos++]) << 16) | ((0xFF & buf[pos++]) << 8) | (0xFF & buf[pos++])));
		return BitConverter.ToSingle(BitConverter.GetBytes(value), 0);
	}

	internal virtual double readDouble()
	{
		long value = (long)((pos + 7 >= len) ? (((ulong)read() << 56) | ((ulong)read() << 48) | ((ulong)read() << 40) | ((ulong)read() << 32) | ((ulong)read() << 24) | ((ulong)read() << 16) | ((ulong)read() << 8) | read()) : (((ulong)buf[pos++] << 56) | ((ulong)buf[pos++] << 48) | ((ulong)buf[pos++] << 40) | ((ulong)buf[pos++] << 32) | ((ulong)buf[pos++] << 24) | ((ulong)buf[pos++] << 16) | ((ulong)buf[pos++] << 8) | buf[pos++]));
		return BitConverter.ToDouble(BitConverter.GetBytes(value), 0);
	}

	internal virtual int skipBytes(int n)
	{
		if (complete && pos + n > len)
		{
			throw new EndOfStreamException();
		}
		pos += n;
		return n;
	}

	internal virtual void flush()
	{
	}

	internal virtual void write(byte b)
	{
		throw new IOException("read-only");
	}

	internal virtual void writeByte(int v)
	{
		throw new IOException("read-only");
	}

	internal virtual void writeShort(int v)
	{
		throw new IOException("read-only");
	}

	internal virtual void writeInt(int v)
	{
		throw new IOException("read-only");
	}

	internal virtual void writeLong(long v)
	{
		throw new IOException("read-only");
	}

	internal virtual void writeFloat(float v)
	{
		throw new IOException("read-only");
	}

	internal virtual void writeDouble(double v)
	{
		throw new IOException("read-only");
	}
}
