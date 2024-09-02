using System;
using System.IO;

namespace DocGen.Pdf;

internal class ByteInputBuffer
{
	private byte[] buf;

	private int count;

	private int pos;

	public ByteInputBuffer(byte[] buf)
	{
		this.buf = buf;
		count = buf.Length;
	}

	public ByteInputBuffer(byte[] buf, int offset, int length)
	{
		this.buf = buf;
		pos = offset;
		count = offset + length;
	}

	public virtual void setByteArray(byte[] buf, int offset, int length)
	{
		if (buf == null)
		{
			if (length < 0 || count + length > this.buf.Length)
			{
				throw new ArgumentException();
			}
			if (offset < 0)
			{
				pos = count;
				count += length;
			}
			else
			{
				count = offset + length;
				pos = offset;
			}
		}
		else
		{
			if (offset < 0 || length < 0 || offset + length > buf.Length)
			{
				throw new ArgumentException();
			}
			this.buf = buf;
			count = offset + length;
			pos = offset;
		}
	}

	public virtual void addByteArray(byte[] data, int off, int len)
	{
		lock (this)
		{
			if (len < 0 || off < 0 || len + off > buf.Length)
			{
				throw new ArgumentException();
			}
			if (count + len <= buf.Length)
			{
				Array.Copy(data, off, buf, count, len);
				count += len;
				return;
			}
			if (count - pos + len <= buf.Length)
			{
				Array.Copy(buf, pos, buf, 0, count - pos);
			}
			else
			{
				byte[] sourceArray = buf;
				buf = new byte[count - pos + len];
				Array.Copy(sourceArray, count, buf, 0, count - pos);
			}
			count -= pos;
			pos = 0;
			Array.Copy(data, off, buf, count, len);
			count += len;
		}
	}

	public virtual int readChecked()
	{
		if (pos < count)
		{
			return buf[pos++] & 0xFF;
		}
		throw new EndOfStreamException();
	}

	public virtual int read()
	{
		if (pos < count)
		{
			return buf[pos++] & 0xFF;
		}
		return -1;
	}
}
