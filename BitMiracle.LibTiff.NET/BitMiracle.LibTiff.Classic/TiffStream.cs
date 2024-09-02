using System;
using System.IO;

namespace BitMiracle.LibTiff.Classic;

public class TiffStream
{
	public virtual int Read(object clientData, byte[] buffer, int offset, int count)
	{
		return ((clientData as Stream) ?? throw new ArgumentException("Can't get underlying stream to read from")).Read(buffer, offset, count);
	}

	public virtual void Write(object clientData, byte[] buffer, int offset, int count)
	{
		((clientData as Stream) ?? throw new ArgumentException("Can't get underlying stream to write to")).Write(buffer, offset, count);
	}

	public virtual long Seek(object clientData, long offset, SeekOrigin origin)
	{
		if (offset == -1)
		{
			return -1L;
		}
		return ((clientData as Stream) ?? throw new ArgumentException("Can't get underlying stream to seek in")).Seek(offset, origin);
	}

	public virtual void Close(object clientData)
	{
		((clientData as Stream) ?? throw new ArgumentException("Can't get underlying stream to close")).Close();
	}

	public virtual long Size(object clientData)
	{
		return ((clientData as Stream) ?? throw new ArgumentException("Can't get underlying stream to retrieve size from")).Length;
	}
}
