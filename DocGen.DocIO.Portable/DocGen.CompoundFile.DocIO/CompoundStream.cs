using System;
using System.IO;

namespace DocGen.CompoundFile.DocIO;

internal abstract class CompoundStream : Stream
{
	private string m_strStreamName;

	public string Name
	{
		get
		{
			return m_strStreamName;
		}
		protected set
		{
			m_strStreamName = value;
		}
	}

	public CompoundStream(string streamName)
	{
		m_strStreamName = streamName;
	}

	public virtual void CopyTo(CompoundStream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		byte[] buffer = new byte[32768];
		_ = Position;
		int count;
		while ((count = Read(buffer, 0, 32768)) > 0)
		{
			stream.Write(buffer, 0, count);
		}
	}
}
