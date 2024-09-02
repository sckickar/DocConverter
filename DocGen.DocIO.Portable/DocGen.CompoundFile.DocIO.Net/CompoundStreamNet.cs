using System;
using System.IO;

namespace DocGen.CompoundFile.DocIO.Net;

internal class CompoundStreamNet : CompoundStream, ICompoundItem
{
	private CompoundFile m_parentFile;

	private DirectoryEntry m_entry;

	private Stream m_stream;

	public DirectoryEntry Entry => m_entry;

	protected Stream Stream
	{
		get
		{
			return m_stream;
		}
		set
		{
			m_stream = value;
		}
	}

	public CompoundFile ParentFile => m_parentFile;

	public override long Length
	{
		get
		{
			if (m_stream == null)
			{
				return m_entry.Size;
			}
			return m_stream.Length;
		}
	}

	public override long Position
	{
		get
		{
			return m_stream.Position;
		}
		set
		{
			m_stream.Position = value;
		}
	}

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => true;

	public CompoundStreamNet(CompoundFile file, DirectoryEntry entry)
		: base(entry.Name)
	{
		if (file == null)
		{
			throw new ArgumentNullException("file");
		}
		if (entry == null)
		{
			throw new ArgumentNullException("entry");
		}
		if (entry.Type != DirectoryEntry.EntryType.Stream)
		{
			throw new ArgumentOutOfRangeException("entry");
		}
		m_parentFile = file;
		m_entry = entry;
	}

	public virtual void Open()
	{
		if (m_stream == null)
		{
			m_stream = m_parentFile.GetEntryStream(m_entry);
		}
	}

	public override int Read(byte[] buffer, int offset, int length)
	{
		return m_stream.Read(buffer, offset, length);
	}

	public override void Write(byte[] buffer, int offset, int length)
	{
		m_stream.Write(buffer, offset, length);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return m_stream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		m_stream.SetLength(value);
	}

	public new void Dispose()
	{
		Flush();
		m_stream.Dispose();
		m_stream = null;
	}

	public override void Flush()
	{
		if (m_stream != null)
		{
			m_parentFile.SetEntryStream(m_entry, m_stream);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (m_stream != null)
		{
			base.Dispose(disposing);
			m_stream.Dispose();
			m_stream = null;
			m_parentFile = null;
			m_entry = null;
			GC.SuppressFinalize(this);
		}
	}
}
